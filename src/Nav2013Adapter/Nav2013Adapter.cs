using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Statical.NavAdapter;
using System.Diagnostics.Contracts;

namespace Statical.NavAdapter.Nav2013
{
    /// <summary>
    /// Dynamics NAV 2013 NavAdapter. Implementation is based on finsql.exe command-line.
    /// May also be used with newer versions of Dynamics NAV, but the Nav2015Adapter is preferred,
    /// as unlicensed objects are simply ignored.
    /// </summary>
    public class Nav2013Adapter : INavAdapter
    {
        #region Constructors
        /// <summary>
        /// Constructs a new Nav2013 based on the given NAV environment.
        /// </summary>
        /// <param name="env">The NAV environment to connect to</param>
        public Nav2013Adapter(NavEnvironment env)
        {
            Contract.Requires(env != null);
            this.Context = env;
            this.FinsqlFullName = ValidateFinsqlPath(env.FinSqlPath);
            this.AdapterName = "Dynamics NAV 2013";
            this.AdapterDescription = "Adapter to interface with Dynamics NAV 2013.";
            this.AdapterVersion = "1.0";
        }
        #endregion

        #region INavAdapter Implementation
        /// <summary>
        /// Dynamics NAV environment
        /// </summary>
        public NavEnvironment Context
        {
            get;
            protected set;
        }

        /// <summary>
        /// Full name of finsql.exe executable
        /// </summary>
        public string FinsqlFullName
        {
            get;
            protected set;
        }

        /// <summary>
        /// The name of the adapter.
        /// </summary>
        public string AdapterName
        {
            get;
            protected set;
        }

        /// <summary>
        /// The description of the adapter.
        /// </summary>
        public string AdapterDescription
        {
            get;
            protected set;
        }

        /// <summary>
        /// The version of the adapter.
        /// </summary>
        public string AdapterVersion
        {
            get;
            protected set;
        }

        /// <summary>
        /// Does the adapter support the DesignObjectAsync operation?
        /// </summary>
        public bool SupportsDesignObject { get { return true; } }

        /// <summary>
        /// Queries an NAV database for object metadata. Implementations will typically query the dbo.Object table of NAV.
        /// </summary>
        /// <param name="idRanges">NAV style filter expression on ID field, e.g. "0..50000|100000..". 
        /// The function must return metadata for all objects matching that filter</param>
        /// <param name="cancellationToken">A token to allow cancellation of the operation</param>
        /// <exception cref="NavAdapterException">In case on an error</exception>
        /// <returns>Metadata for objects matching at least one of the ranges  of idRanges</returns>
        public Task<ISet<NavObjectMetadata>> ObjectMetadataAsync(ISet<NavObjectIdRange> idRanges, CancellationToken cancellationToken)
        {
            return this.Context.ObjectMetadataAsync(idRanges, cancellationToken);
        }

        /// <summary>
        /// Exports the txt representation of an object in NAV to a stream. Implementations will typically export the object 
        /// (using C/Front or finsql.exe command-line depending on NAV version)
        /// </summary>
        /// <param name="navObjectRef">The object to export.</param>
        /// <param name="outStream">Out stream for exported object</param>
        /// <param name="cancellationToken">A token to allow cancellation of the operation</param>
        /// <exception cref="NavAdapterException">In case on an error</exception>
        /// <exception cref="CannotFindServiceTierException">In case a service tier cannot be found</exception>
        /// <returns>Task encapsulating asynchronous export operation</returns>
        public async Task<NavObjectLicenseStatus> ExportSingleAsync(NavObjectReference navObjectRef, Stream outStream, CancellationToken cancellationToken)
        {
            var filter = "Type=" + navObjectRef.Type + ";ID=" + navObjectRef.Id;
            var tmpExportFile = Path.GetTempPath() + Guid.NewGuid().ToString() + ".txt";
            try
            {
                NavObjectLicenseStatus result = await ExportFilterAsync(filter, true, tmpExportFile, cancellationToken).ConfigureAwait(false);
                if (result.IsLicensed) 
                {
                    using (var fs = new FileStream(tmpExportFile, FileMode.Open))
                    {
                        await fs.CopyToAsync(outStream, 81920, cancellationToken).ConfigureAwait(false);
                    }
                }
                return result;
            }
            finally
            {
                // Delete temporary files
                SafeDelete(tmpExportFile);
            }
        }

        /// <summary>
        /// Exports multiple objects from NAV to a file to the specified file name.
        /// Implementations to will typically export the object (using C/Front or finsql.exe command-line).
        /// </summary>
        /// <param name="idRanges">The object ranges to export</param>
        /// <param name="filePath">The file to export to.</param>
        /// <param name="cancellationToken">A token to allow cancellation of the operation</param>
        /// <exception cref="NavAdapterException">In case on an error</exception>
        /// <exception cref="CannotFindServiceTierException">In case a service tier cannot be found</exception> 
        /// <exception cref="NavObjectLicenseException">In case objects cannot be exported due to license issue.</exception> 
        /// <returns>Task encapsulating asynchronous operation</returns>
        public async Task ExportMultipleAsync(ISet<NavObjectIdRange> idRanges, string filePath, CancellationToken cancellationToken)
        {
            if (!filePath.EndsWith(".txt"))
            {
                throw new ArgumentException("filePath parameter must end with .txt");
            }
            var filter = "ID=" + NavObjectIdRange.NavFilterExpression(idRanges);
            // Here we ignore the result of ExportFilterAsync deliberately, as for multiple objects, 
            // an exception is thrown instead in license issues
            await ExportFilterAsync(filter, false, filePath, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Tests connectivity to NAV.
        /// </summary>
        /// <returns>A task encapsulating a set of errors. If no error found, then an empty set is returned.</returns>
        public async Task<ISet<string>> TestAsync(CancellationToken cancellationToken)
        {
            ISet<string> result = new HashSet<string>();
            try
            {
                result = await this.Context.TestAsync(cancellationToken).ConfigureAwait(false);
                var dummy = ValidateFinsqlPath(this.Context.FinSqlPath);
            }
            catch (Exception e)
            {
                result.Add(e.Message);
            }
            return result;
        }

        /// <summary>
        /// Opens C/AL designer for the specified object. Requires the property SupportsDesignObject to be true.
        /// </summary>
        /// <param name="navObjectRef">The object to open.</param>
        /// <returns>A task encapsulating the operation.</returns>
        /// <exception cref="NavAdapterException">In case on an error</exception>
        /// <exception cref="NotSupportedException">If the operation is not supported see the SupportsDesignObject property</exception>
        public async Task DesignObjectAsync(NavObjectReference navObjectRef)
        {
            var tmpLogFile = Path.GetTempFileName();
            try
            {
                // example arg: designobject=Page 21
                var args = new StringBuilder()
                    .Append(Param("designobject", navObjectRef.Type.ToString() + " " + navObjectRef.Id)).Append(",")
                    .Append(Param("servername", this.Context.DbServer)).Append(",")
                    .Append(Param("database", this.Context.DbName)).Append(",");
                if (this.Context.IsNtAuthentication)
                {
                    args.Append(Param("ntauthentication", "1")).Append(",");
                }
                else
                {
                    args
                        .Append(Param("ntauthentication", "0")).Append(",")
                        .Append(Param("username", this.Context.DbUserId)).Append(",")
                        .Append(Param("password", this.Context.DbPassword)).Append(",");
                }
                args.Append(Param("logfile", tmpLogFile));

                // Execute command in background

                var process = new Process();
                process.StartInfo.FileName = "CMD.EXE";
                process.StartInfo.Arguments = " /S /C \"" + Quote(this.FinsqlFullName) + " " + args.ToString() + "\"";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                process.Start();

                var dummyCts = new CancellationTokenSource();
                await NavAdapterHelper.WaitAsync(process, dummyCts.Token).ConfigureAwait(false);
            }
            finally
            {
                SafeDelete(tmpLogFile);
            }
        }
        #endregion

        #region IDisposable Implementation
        public void Dispose()
        { 
            // NOOP
        }
        #endregion

        #region Implementation Helpers

        protected virtual String FinSqlArguments(string filter, bool singleObject, string filePath, string tmpLogFile)
        {
            StringBuilder args = new StringBuilder()
                .Append(Param("command", "exportobjects")).Append(",")
                .Append(Param("filter", filter)).Append(",")
                .Append(Param("file", filePath)).Append(",")
                .Append(Param("servername", this.Context.DbServer)).Append(",")
                .Append(Param("database", this.Context.DbName)).Append(",");
            if (this.Context.IsNtAuthentication)
            {
                args
                    .Append(Param("ntauthentication", "1")).Append(",");
            }
            else
            {
                args
                    .Append(Param("ntauthentication", "0")).Append(",")
                    .Append(Param("username", this.Context.DbUserId)).Append(",")
                    .Append(Param("password", this.Context.DbPassword)).Append(",");
            }
            args
                .Append(Param("logfile", tmpLogFile));
            
            return args.ToString();
        }

        // NavObjectLicenseStatus is only returned for single object export. When exporting multiple objects, an exception is thrown, if there is a license problem.
        private async Task<NavObjectLicenseStatus> ExportFilterAsync(string filter, bool singleObject, string filePath, CancellationToken cancellationToken)
        {
            var tmpLogFile = Path.GetTempFileName();
            var process = (Process)null;

            try
            {
                string args = FinSqlArguments(filter, singleObject, filePath, tmpLogFile);

                // Execute command
                process = new Process();
                process.StartInfo.FileName = "CMD.EXE";
                process.StartInfo.Arguments = " /S /C \"" + Quote(this.FinsqlFullName) + " " + args.ToString() + "\"";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.CreateNoWindow = true;
                process.Start();
                await NavAdapterHelper.WaitAsync(process, cancellationToken).ConfigureAwait(false);
                var err = await process.StandardError.ReadToEndAsync().ConfigureAwait(false);

                cancellationToken.ThrowIfCancellationRequested();
                if (!String.IsNullOrWhiteSpace(err))
                {
                    throw new NavAdapterException("Failed to export: " + err);
                }
                if (process.ExitCode == 0)
                {
                    // Export was successful, but we have had situations where the export file does not exist, so checking 
                    if (!File.Exists(filePath))
                    {
                        throw new NavAdapterException(
                            "Export finished with exit code 0, but the export file '"
                            + filePath
                            + "' does not exist. Possible cause is that finsql.exe is earlier than NAV 2013");
                    }
                    if (singleObject)
                    {
                        return new NavObjectLicenseStatus(true);
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    // The export failed.
                    // The export file may have been created, but we ignore that, and clean up instead
                    SafeDelete(filePath);
                    // Throw exception
                    if (!File.Exists(tmpLogFile))
                    {
                        throw new NavAdapterException("Could not open export log file: " + tmpLogFile);
                    }
                    var logText = File.ReadAllText(tmpLogFile);
                    if (logText.Contains("You do not have permission to read the"))
                    {
                        if (singleObject)
                        {
                            return new NavObjectLicenseStatus(false);
                        }
                        else
                        {
                            throw new NavObjectLicenseException("Export failed with the following log messages:" + Environment.NewLine + logText);
                        }
                    } 
                    else if (logText.Contains("There are no NAV Server instances available for this database") |
                             logText.Contains("This database is registered with several NAV Server instances"))
                    {
                        throw new NavServerInstanceException("Export failed with the following log message:" + Environment.NewLine + logText);
                    }
                    else
                    {
                        throw new NavObjectExportException("Export with filter '" + filter + "' failed with the following log messages:" + Environment.NewLine + logText);
                    }
                }
            }
            finally
            {
                // Stop process
                NavAdapterHelper.TryEndProcess(process);
                // Delete temporary files
                SafeDelete(tmpLogFile);
            }
        }

        private static string ValidateFinsqlPath(string finsqlPath)
        {
            // Validate finsql path exists and normalize name
            string finsqlFullName = null;
            try
            {
                var finsql = new FileInfo(finsqlPath);
                finsqlFullName = finsql.FullName;
            }
            catch (Exception e)
            {
                throw new NavAdapterException("Finsql path is not a valid file name: " + finsqlPath, e);
            }

            if (!File.Exists(finsqlFullName))
            {
                throw new NavAdapterException("Finsql exe does not exist: " + finsqlFullName);
            }
            return finsqlFullName;
        }

        private string Param(string parameterName, string parameterValue)
        {
            return " " + parameterName + "=" + Quote(parameterValue);
        }

        private string Quote(string s)
        {
            return "\"" + s + "\"";
        }

        private void SafeDelete(string filePath)
        {
            if (File.Exists(filePath))
            {
                try
                {
                    File.Delete(filePath);
                }
                catch
                {
                    // Ignore
                }
            }
        }
        #endregion

    }
}
