using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NavAdapter
{

    /// <summary>
    /// This class represents a unique reference to an NAV object in a database
    /// </summary>
    public class NavObjectReference
    {

        /// <summary>
        /// The object type
        /// </summary>
        public NavObjectType Type { get; private set; }

        /// <summary>
        /// The object id 
        /// </summary>
        public int Id { get; private set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="id"></param>
        public NavObjectReference(NavObjectType type, int id) 
        {
            this.Type = type;
            this.Id = id;
        }

        public override string ToString()
        {
            return (new StringBuilder())
                .Append("NavObjectReference{")
                .Append("Type=").Append(Type.ToString()).Append(",")
                .Append("Id=").Append(Id)
                .Append("}").ToString();
        }
    }
}
