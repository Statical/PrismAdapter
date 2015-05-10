using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;

namespace Statical.NavAdapter
{
    /// <summary>
    /// Defines access to a NAV environment.
    /// </summary>
    public class NavEnvironment
    {
        /// <summary>
        /// The file system path to the finsql.exe executable file.
        /// </summary>
        public string FinSqlPath { get; set; }

        /// <summary>
        /// The database server domain name or IP address.
        /// </summary>
        public string DbServer { get; set; }

        /// <summary>
        /// The database name.
        /// </summary>
        public string DbName { get; set; }

        /// <summary>
        /// The database user identifier.
        /// </summary>
        public string DbUserId { get; set; }

        /// <summary>
        /// The database user password.
        /// </summary>
        public string DbPassword { get; set; }

        /// <summary>
        /// The database connection string.
        /// </summary>
        public string DbConnectionString
        {
            get
            {
                var builder = (SqlConnectionStringBuilder)null;
                if (IsNtAuthentication)
                {
                    builder = new SqlConnectionStringBuilder()
                    {
                        DataSource = DbServer,
                        InitialCatalog = DbName,
                        ConnectTimeout = 6,
                        IntegratedSecurity = true
                    };
                }
                else
                {
                    builder = new SqlConnectionStringBuilder()
                    {
                        DataSource = DbServer,
                        InitialCatalog = DbName,
                        ConnectTimeout = 6,
                        IntegratedSecurity = false,
                        UserID = DbUserId,
                        Password = DbPassword
                    };
                }
                return builder.ConnectionString;
            }
        }

        /// <summary>
        /// Is the environment using NT authentication.
        /// </summary>
        public Boolean IsNtAuthentication
        {
            get
            {
                return String.IsNullOrEmpty(DbUserId);
            }
        }
    
    }
}
