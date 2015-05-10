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
    /// Exception thrown when an NAV Service tier instance cannot be found. Two different message can trigger this:
    ///   (1) There are no NAV Server instances available for this database
    ///   (2) This database is registered with several NAV Server instances. You must choose an instance to use before performing this activity. Do you want to continue?
    /// Information about service tier is stored in the zup file and can be found in C/SIDE's (finsql.exe) File - Database - Information menu, or 
    /// uner "Options" menu. To work around the issue, open finsql.exe without specifying an "id" parameter, open the database and select
    /// an instance under File - Database - Information. Then close the development environment.
    /// </summary>
    [Serializable]
    public class NavServerInstanceException : NavObjectExportException, ISerializable
    {
        /// <summary>
        /// Constructs a new NavServerInstanceException.
        /// </summary>
        public NavServerInstanceException()
            : base()
        {
        }

        /// <summary>
        /// Constructs a new NavServerInstanceException.
        /// </summary>
        public NavServerInstanceException(string message)
            : base(message)
        {
            Contract.Requires(message != null);
        }

        /// <summary>
        /// Constructs a new NavServerInstanceException.
        /// </summary>
        public NavServerInstanceException(string message, Exception inner)
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
