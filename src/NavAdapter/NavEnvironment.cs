using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;

namespace NavAdapter
{
    public class NavEnvironment
    {
        public string FinSqlPath { get; set; }
        public string DbServer { get; set; }
        public string DbName { get; set; }
        public string DbUserId { get; set; }
        public string DbPassword { get; set; }

        public string DbConnectionString()
        {
            var builder = (SqlConnectionStringBuilder) null;
            if (IsNtAuthentication())
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

        public Boolean IsNtAuthentication()
        {
            return (DbUserId == null || DbUserId == "") ? true : false;
        }
    
    }
}
