using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics.Contracts;

namespace Statical.NavAdapter
{
    /// <summary>
    /// This interface declares functions for interacting with an NAV database.
    /// </summary>
    public interface INavAdapter : IDisposable
    {
        /// <summary>
        /// The name of the adapter.
        /// </summary>
        string AdapterName { get; }

        /// <summary>
        /// The description of the adapter.
        /// </summary>
        string AdapterDescription { get; }

        /// <summary>
        /// The version of the adapter.
        /// </summary>
        string AdapterVersion { get; }

        /// <summary>
        /// Does the adapter support the DesignObjectAsync operation?
        /// </summary>
        bool SupportsDesignObject { get; }

        /// <summary>
        /// Dynamics NAV environment
        /// </summary>
        NavEnvironment Context { get; }

        /// <summary>
        /// Queries an NAV database for object metadata. Implementations will typically query the dbo.Object table of NAV.
        /// </summary>
        /// <param name="idRanges">NAV style filter expression on ID field, e.g. "0..50000|100000..". 
        /// The function must return metadata for all objects matching that filter</param>
        /// <param name="cancellationToken">A token to allow cancellation of the operation</param>
        /// <exception cref="NavAdapterException">In case on an error</exception>
        /// <returns>Metadata for objects matching at least one of the ranges  of idRanges</returns>
        Task<ISet<NavObjectMetadata>> ObjectMetadataAsync(ISet<NavObjectIdRange> idRanges, CancellationToken cancellationToken);

        /// <summary>
        /// Exports the txt representation of an object in NAV to a stream. Implementations will typically export the object 
        /// (using C/Front or finsql.exe command-line depending on NAV version)
        /// </summary>
        /// <param name="navObjectRef">The object to export.</param>
        /// <param name="outStream">Out stream for exported object</param>
        /// <param name="cancellationToken">A token to allow cancellation of the operation</param>
        /// <exception cref="NavAdapterException">In case on an error</exception>
        /// <returns>Task encapsulating asynchronous export operation</returns>
        Task<NavObjectLicenseStatus> ExportSingleAsync(NavObjectReference navObjectRef, Stream outStream, CancellationToken cancellationToken);
        
        /// <summary>
        /// Exports multiple objects from NAV to a file to the specified file name.
        /// Implementations will typically export the object (using C/Front or finsql.exe command-line).
        /// </summary>
        /// <param name="idRanges">The object ranges to export</param>
        /// <param name="filePath">The file to export to.</param>
        /// <param name="cancellationToken">A token to allow cancellation of the operation</param>
        /// <exception cref="NavAdapterException">In case on an error</exception>
        /// <exception cref="NavObjectLicenseException">In case objects cannot be exported due to license issue.</exception> 
        /// Implementations should ignore unlicensed objects, if possible.</exception>
        /// <returns>Task encapsulating asynchronous operation</returns>
        Task ExportMultipleAsync(ISet<NavObjectIdRange> idRanges, string filePath, CancellationToken cancellationToken);

        /// <summary>
        /// Opens C/AL designer for the specified object. Requires the property SupportsDesignObject to be true.
        /// </summary>
        /// <param name="navObjectRef">The object to open.</param>
        /// <returns>A task encapsulating the operation.</returns>
        /// <exception cref="NavAdapterException">In case on an error</exception>
        /// <exception cref="NotSupportedException">If the operation is not supported see the SupportsDesignObject property</exception>
        Task DesignObjectAsync(NavObjectReference navObjectRef);

        /// <summary>
        /// Tests connectivity to NAV. The function must return metadata for all objects matching that filter.
        /// </summary>
        /// <returns>A task encapsulating a set of errors. If no error found, then an empty set is returned.</returns>
        Task<ISet<string>> TestAsync(CancellationToken cancellationToken);
    }

    #region CodeContracts
    /// <summary>
    /// Code Contracts contract specification.
    /// </summary>
    [ContractClassFor(typeof(INavAdapter))]
    internal sealed class NavAdapterCodeContract : INavAdapter
    {
        public string AdapterName { get { return null; } }

        public string AdapterDescription { get { return null; } }

        public string AdapterVersion { get { return null; } }

        public NavEnvironment Context { get { return null; } }

        public bool SupportsDesignObject { get { return default(bool); } }

        public Task<ISet<NavObjectMetadata>> ObjectMetadataAsync(ISet<NavObjectIdRange> idRanges, CancellationToken cancellationToken)
        {
            Contract.Requires(idRanges != null);
            Contract.Requires(Contract.ForAll(idRanges, x => x != null));

            return default(Task<ISet<NavObjectMetadata>>);
        }

        public Task<NavObjectLicenseStatus> ExportSingleAsync(NavObjectReference navObjectRef, Stream outStream, CancellationToken cancellationToken)
        {
            Contract.Requires(navObjectRef != null);
            Contract.Requires(outStream != null);
            Contract.Requires(outStream.CanWrite);

            return default(Task<NavObjectLicenseStatus>);
        }

        public Task ExportMultipleAsync(ISet<NavObjectIdRange> idRanges, string filePath, CancellationToken cancellationToken)
        {
            Contract.Requires(idRanges != null);
            Contract.Requires(Contract.ForAll(idRanges, x => x != null));
            Contract.Requires(!String.IsNullOrWhiteSpace(filePath));

            return default(Task);
        }

        public Task DesignObjectAsync(NavObjectReference navObjectRef)
        {
            Contract.Requires(SupportsDesignObject);
            Contract.Requires(navObjectRef != null);

            return default(Task);
        }

        public Task<ISet<string>> TestAsync(CancellationToken cancellationToken)
        {
            // pending code contracts support for async results
            //Contract.Ensures(
            //    Contract.Result<Task<ISet<string>>>().IsCanceled ||
            //    Contract.Result<Task<ISet<string>>>().IsFaulted ||
            //    Contract.ForAll(Contract.Result<Task<ISet<string>>>().Result, x => x != null));

            return default(Task<ISet<string>>);
        }

        public void Dispose()
        { 
        }

        [ContractInvariantMethod]
        void ObjectInvariant()
        {
            Contract.Invariant(!String.IsNullOrWhiteSpace(AdapterName));
            Contract.Invariant(!String.IsNullOrWhiteSpace(AdapterVersion));
            Contract.Invariant(AdapterDescription != null);
            Contract.Invariant(Context != null);
        }
    }
    #endregion
}
