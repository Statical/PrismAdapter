using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Configuration;

using NavAdapter;

namespace NavAdapterTestApp
{
    class TestApp
    {
        static void Main(string[] args)
        {
            // Set up environment
            var env = new NavEnvironment();
            // TODO get from config file
            env.FinSqlPath = ConfigurationManager.AppSettings["FinSqlPath"];
            env.DbServer = ConfigurationManager.AppSettings["DbServer"];
            env.DbName = ConfigurationManager.AppSettings["DbName"];
            env.DbUserId = ConfigurationManager.AppSettings["DbUserId"];
            env.DbPassword = ConfigurationManager.AppSettings["DbPassword"];
            INavAdapter nav = new Nav2013Adapter(env);

            try
            {

                // DesignObject
                Console.WriteLine("DesignObject");
                nav.DesignObject (new NavObjectReference(NavObjectType.Page, 50000));

                // Test
                Console.WriteLine("Test");
                var errors = nav.Test();
                foreach (var error in errors)
                {
                    Console.WriteLine("Error: " + error);
                }

                // ObjectMetadata
                Console.WriteLine("ObjectMetadata");
                var idRanges = new HashSet<ObjectIdRange>();
                idRanges.Add(new ObjectIdRange(null, 17));
                idRanges.Add(new ObjectIdRange(19, 36));
                idRanges.Add(new ObjectIdRange(50003, null));
                var metadata = nav.ObjectMetadata(idRanges);
                // dump to file
                var metadataFile = "test-metadata.txt";
                using (var sw = new StreamWriter(metadataFile))
                {
                    foreach (var obj in metadata)
                    {
                        sw.WriteLine(obj.ToString());
                    }
                    sw.Close();
                }
                Console.WriteLine("Metadata dumped to file: " + metadataFile);

                // ExportSingle
                Console.WriteLine("ExportSingle");
                using (var fileStream = File.Create("test-export-codeunit-50000.txt"))
                {
                    nav.ExportSingle(new NavObjectReference(NavObjectType.Codeunit, 50000), fileStream);
                }

                // ExportMultiple
                Console.WriteLine("ExportMultiple");
                nav.ExportMultiple(idRanges, "test-export-multiple.txt");

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            Console.WriteLine("Press a key to exit");
            Console.ReadKey();

        }
    }
}
