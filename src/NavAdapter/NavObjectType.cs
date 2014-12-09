using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NavAdapter
{

    /// <summary>
    /// This enumeration represents the different object types of NAV. The underlying numbers match the
    /// values of dbo.Object table.
    /// </summary>
    public enum NavObjectType : int
    {
        Table = 1,
        Form = 2,
        Report = 3,
        Dataport = 4,
        Codeunit = 5,
        XMLPort = 6,
        MenuSuite = 7,
        Page = 8,
        Query = 9,
    }

}
