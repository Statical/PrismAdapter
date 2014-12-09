using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Data;
using System.Data.SqlClient;

namespace NavAdapter
{
    public abstract class BaseAdapter : INavAdapter
    {

        public NavEnvironment env {get; private set;}

        public BaseAdapter(NavEnvironment env)
        {
            this.env = env;
        }

        public abstract void Dispose();

        /// <summary>
        /// Queries dbo.Object table of NAV and returns timestamp column as RowNumber.
        /// </summary>
        /// <param name="idFilterExpression">NAV style filter expression on ID field, e.g. "0..50000|100000..". 
        /// </param>
        /// <returns>Metadata for objects matching at least one of the ranges  of idRanges</returns>
        public ISet<NavObjectMetadata> ObjectMetadata(ISet<ObjectIdRange> idRanges)
        {
            var result = new HashSet<NavObjectMetadata>();

            using (var connection = new SqlConnection(env.DbConnectionString()))
            {
                NavObjectType[] allObjectTypes = (NavObjectType[]) Enum.GetValues(typeof(NavObjectType));
                var typeWhereClause = " Type IN (" + string.Join(",", allObjectTypes.Select(t => (int) t)) + ")";
                var idWhereClause = (idRanges.Count == 0) ? "1=1" : ObjectIdRange.WhereClause(idRanges);
                var query = 
                    @"SELECT 
                        ID, 
                        Type, 
                        Name, 
                        [BLOB Size], 
                        [Version List], 
                        Date, 
                        Time, 
                        timestamp 
                    FROM dbo.Object WITH (NOLOCK)
                    WHERE " + typeWhereClause + " AND (" + idWhereClause + ")";
                
                connection.Open();
                using (var command = new SqlCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader(CommandBehavior.SingleResult))
                    {
                        while (reader.Read())
                        {
                            // fetch values
                            var id = reader.GetInt32(0);
                            var type = reader.GetInt32(1);
                            var name = reader.GetString(2);
                            var blobSize = reader.GetInt32(3);
                            var versionList = reader.GetString(4);
                            var date = reader.GetDateTime(5);
                            var time = reader.GetDateTime(6);
                            var timestamp = reader.GetSqlBinary(7);

                            var dateTime = new DateTime(date.Year, date.Month, date.Day, time.Hour, time.Minute, time.Second);

                            NavObjectMetadata metadata = new NavObjectMetadata()
                            {
                                ObjectReference = new NavObjectReference((NavObjectType)type, id),
                                Name = name,
                                BlobSize = blobSize,
                                VersionList = versionList,
                                Time = dateTime,
                                RowVersion = ToHex(timestamp.Value)
                            };
                            result.Add(metadata);
                        }
                    }
                }
                connection.Close();
            }

            return result;
        }

        /// <summary>
        /// Tests connectivity to database.
        /// </summary>
        /// <returns>A single error or empty set.</returns>
        public virtual ISet<string> Test()
        {
            var result = new HashSet<string>();
            try
            {
                ObjectMetadata(new HashSet<ObjectIdRange>());
            }
            catch (Exception e)
            {
                result.Add("Could not connect to database: " + e.Message);
            }
            return result;
        }

        public abstract void ExportSingle(NavObjectReference oref, Stream outStream);

        public abstract void ExportMultiple(ISet<ObjectIdRange> idRanges, string filePath);

        public abstract void DesignObject(NavObjectReference oref);

        private string ToHex(byte[] bs)
        {
            return string.Join("-", bs.Select(b => string.Format("{0:X2}", b)));
        }
    }
}
