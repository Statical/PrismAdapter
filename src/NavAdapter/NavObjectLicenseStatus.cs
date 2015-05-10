using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;

namespace Statical.NavAdapter
{
    /// <summary>
    /// License status of an nAV object
    /// </summary>
    public class NavObjectLicenseStatus
    {
        /// <summary>
        /// Whether the object is licensed or not
        /// </summary>
        public bool IsLicensed { get; private set; }

        public NavObjectLicenseStatus(bool isLicensed)
        {
            this.IsLicensed = isLicensed;
        }
    }
}
