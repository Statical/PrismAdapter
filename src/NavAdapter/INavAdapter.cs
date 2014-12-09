using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;

namespace NavAdapter
{

    /// <summary>
    /// This interface declares functions for interacting with an NAV database.
    /// </summary>
    public interface INavAdapter : IDisposable
    {

        /// <summary>
        /// Queries an NAV database for object metadata. Implementations will typically query the dbo.Object table of NAV.
        /// </summary>
        /// <param name="idFilterExpression">NAV style filter expression on ID field, e.g. "0..50000|100000..". 
        /// The function must return metadata for all objects matching that filter</param>
        /// <returns>Metadata for objects matching at least one of the ranges  of idRanges</returns>
        ISet<NavObjectMetadata> ObjectMetadata(ISet<ObjectIdRange> idRanges);

        /// <summary>
        /// Exports the txt representation of an object in NAV to a stream. Implementations will typically export the object 
        /// (using C/Front or finsql.exe command-line depending on NAV version)
        /// </summary>
        /// <param name="oref">The object to export.</param>
        /// <param name="outStream">Out stream for exported object</param>
        void ExportSingle(NavObjectReference oref, Stream outStream);

        /// <summary>
        /// Exports multiple objects from NAV to a file to the specified file name.
        /// Implementations to will typically export the object (using C/Front or finsql.exe command-line).
        /// </summary>
        /// <param name="idRanges">The object ranges to export</param>
        /// <param name="filePath">The file to export to.</param>
        void ExportMultiple(ISet<ObjectIdRange> idRanges, string filePath);

        /// <summary>
        /// Opens C/AL designer for the specified object.
        /// </summary>
        /// <param name="oref">The object to open.</param>
        void DesignObject(NavObjectReference oref);

        /// <summary>
        /// Tests connectivity to NAV.
        /// </summary>
        /// The function must return metadata for all objects matching that filter</param>
        /// <returns>A set of errors. If no error found, then an empty set is returned.</returns>
        ISet<string> Test();
    }
}
