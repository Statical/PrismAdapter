using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Diagnostics;

namespace NavAdapter
{
    public class Nav2013Adapter : BaseAdapter, IDisposable
    {

        private Process proc = null;

        public Nav2013Adapter(NavEnvironment env) : base(env)
        {
            // nothing
        }

        private void ExportFilter(string filter, string filePath)
        {
            string tmpLogFile = Path.GetTempFileName();

            string finsqlFullName = ValidateFinsqlPath();

            try
            {

                StringBuilder args = new StringBuilder()
                    .Append(Param("command", "exportobjects")).Append(",")
                    .Append(Param("filter", filter)).Append(",")
                    .Append(Param("file", filePath)).Append(",")
                    .Append(Param("servername", env.DbServer)).Append(",")
                    .Append(Param("database", env.DbName)).Append(",");
                if (env.IsNtAuthentication())
                {
                    args
                        .Append(Param("ntauthentication", "1")).Append(",");
                }
                else
                {
                    args
                        .Append(Param("ntauthentication", "0")).Append(",")
                        .Append(Param("username", env.DbUserId)).Append(",")
                        .Append(Param("password", env.DbPassword)).Append(",");
                }
                args
                    .Append(Param("logfile", tmpLogFile));

                // Execute command
                proc = new Process();
                proc.StartInfo.FileName = "CMD.EXE";
                proc.StartInfo.Arguments = " /S /C \"" + Quote(finsqlFullName) + " " + args.ToString() + "\"";
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.RedirectStandardError = true;
                proc.StartInfo.CreateNoWindow = true;
                proc.Start();
                var err = proc.StandardError.ReadToEnd();
                if (err != "")
                {
                    throw new ApplicationException(
                        "Failed to export: " + err);                           
                }
                proc.WaitForExit();
                if (proc.ExitCode == 0)
                {
                    // Export was successful, but we have had situations where the export file does not exist, so checking 
                    if (! File.Exists(filePath))
                    {
                        throw new ApplicationException(
                            "Export finished with exit code 0, but the export file '" 
                            + filePath 
                            + "' does not exist. Possible cause is that finsql.exe is earlier than NAV 2013");
                    }
                }
                else
                {
                    // The export file may have been created, but we ignore that, and clean up instead
                    SafeDelete(filePath);
                    // Throw exception
                    if (!File.Exists(tmpLogFile))
                    {
                        throw new ApplicationException("Could not open export log file: " + tmpLogFile);
                    }
                    var logText = File.ReadAllText(tmpLogFile);
                    throw new ApplicationException("Export failed with the following log messages:" + Environment.NewLine + logText);
                }
            }
            finally
            {
                // Delete temporary files
                SafeDelete(tmpLogFile);
                proc = null;
            }
        }

        private string ValidateFinsqlPath()
        {
            // Validate finsql path exists and normalize name
            string finsqlFullName = null;
            try
            {
                FileInfo finsql = new FileInfo(env.FinSqlPath);
                finsqlFullName = finsql.FullName;
            }
            catch (Exception e)
            {
                throw new ApplicationException("Finsql path is not a valid file name: " + env.FinSqlPath);
            }

            if (!File.Exists(finsqlFullName))
            {
                throw new ApplicationException("Finsql exe does not exist: " + finsqlFullName);
            }
            return finsqlFullName;
        }

        public override void Dispose()
        {
            if (proc != null)
            {
                try
                {
                    proc.Kill();
                }
                finally
                {
                    proc.Dispose();
                }
            }
        }

        ~Nav2013Adapter()
        {
            Dispose();
        }

        public override void ExportSingle(NavObjectReference oref, Stream outStream)
        {
            var filter = "Type=" + oref.Type + ";ID=" + oref.Id;
            string tmpExportFile = Path.GetTempPath() + Guid.NewGuid().ToString() + ".txt";
            try
            {
                ExportFilter(filter, tmpExportFile);
                using (var fs = new FileStream(tmpExportFile, FileMode.Open))
                {
                    fs.CopyTo(outStream);
                }
            }
            finally
            {
                // Delete temporary files
                SafeDelete(tmpExportFile);
            }

        }

        public override void ExportMultiple(ISet<ObjectIdRange> idRanges, string filePath)
        {
            if (!filePath.EndsWith(".txt"))
            {
                throw new InvalidOperationException("filePath parameter must end with .txt");
            }
            var filter = "ID=" + ObjectIdRange.NavFilterExpression(idRanges);
            ExportFilter(filter, filePath);
        }

        /// <summary>
        /// Tests connectivity to database using base class and also that FinSqlPath exists.
        /// </summary>
        /// <returns>Set of error messages.</returns>
        public override ISet<string> Test()
        {
            var result = base.Test();
            try
            {
                var dummy = ValidateFinsqlPath();
            }
            catch (Exception e)
            {
                result.Add(e.Message);
            }
            return result;
        }

        /// <summary>
        /// Opens C/AL designer for the specified object.
        /// </summary>
        /// <param name="oref">The object to open.</param>
        public override void DesignObject(NavObjectReference oref)
        {
            string tmpLogFile = Path.GetTempFileName();

            string finsqlFullName = ValidateFinsqlPath();

            try
            {
                //designobject=Page 21
                StringBuilder args = new StringBuilder()
                    .Append(Param("designobject", oref.Type.ToString() + " " + oref.Id)).Append(",")
                    .Append(Param("servername", env.DbServer)).Append(",")
                    .Append(Param("database", env.DbName)).Append(",");
                if (env.IsNtAuthentication())
                {
                    args
                        .Append(Param("ntauthentication", "1")).Append(",");
                }
                else
                {
                    args
                        .Append(Param("ntauthentication", "0")).Append(",")
                        .Append(Param("username", env.DbUserId)).Append(",")
                        .Append(Param("password", env.DbPassword)).Append(",");
                }
                args
                    .Append(Param("logfile", tmpLogFile));

                // Execute command in background

                proc = new Process();
                proc.StartInfo.FileName = "CMD.EXE";
                proc.StartInfo.Arguments = " /S /C \"" + Quote(finsqlFullName) + " " + args.ToString() + "\"";
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.CreateNoWindow = true;
                proc.Start();

            }
            finally
            {
                // Delete temporary files
                SafeDelete(tmpLogFile);
                proc = null;
            }
        }

        private string Quote(string s)
        {
            return "\"" + s + "\"";
        }

        private string Param(string parameterName, string parameterValue)
        {
            return " " + parameterName + "=" + Quote(parameterValue);
        }

        private void SafeDelete(string filePath)
        {
            if (File.Exists(filePath))
            {
                try
                {
                    File.Delete(filePath);
                }
                catch (Exception _)
                {
                    // Ignore
                }
            }
        }

    }
}
