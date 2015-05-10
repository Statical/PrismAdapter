using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics.Contracts;

namespace Statical.NavAdapter
{
    /// <summary>
    /// Exception thrown when a NAV object could not be exported because it is not included
    /// in the currently loaded NAV license.
    /// </summary>
    [Serializable]
    public class NavObjectLicenseException : NavObjectExportException, ISerializable
    {
        /// <summary>
        /// Constructs a new NavObjectLicenseException.
        /// </summary>
        public NavObjectLicenseException()
            : base()
        {
        }

        /// <summary>
        /// Constructs a new NavObjectLicenseException.
        /// </summary>
        public NavObjectLicenseException(string message)
            : base(message)
        {
            Contract.Requires(message != null);
        }

        /// <summary>
        /// Constructs a new NavObjectLicenseException.
        /// </summary>
        public NavObjectLicenseException(string message, Exception inner)
            : base(message, inner)
        {
            Contract.Requires(message != null);
            Contract.Requires(inner != null);
        }

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            Contract.Requires(info != null);

            base.GetObjectData(info, context);
            if (info == null)
                throw new ArgumentNullException("info");
        }
    }
}
