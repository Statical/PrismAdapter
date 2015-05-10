using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;
using Statical.NavAdapter;
using Statical.NavAdapter.Nav2013;
using Statical.NavAdapter.Nav2015;

namespace NavAdapterTestApp
{
    class TestApp
    {
        static void Main(string[] args)
        {
            Exec(args).Wait();
        }

        private static async Task Exec(string[] args)
        {
            // Set up environment
            var env = new NavEnvironment();
            // TODO get from config file
            string adapter = ConfigurationManager.AppSettings["Adapter"];
            env.FinSqlPath = ConfigurationManager.AppSettings["FinSqlPath"];
            env.DbServer = ConfigurationManager.AppSettings["DbServer"];
            env.DbName = ConfigurationManager.AppSettings["DbName"];
            env.DbUserId = ConfigurationManager.AppSettings["DbUserId"];
            env.DbPassword = ConfigurationManager.AppSettings["DbPassword"];
            INavAdapter nav = null;
            if (adapter == "Nav2013Adapter")
            {
                nav = new Nav2013Adapter(env);
            }
            else if (adapter == "Nav2015Adapter")
            {
                nav = new Nav2015Adapter(env);
            }
            else
            {
                throw new InvalidOperationException ("Unknown adapter '" + adapter + "'");
            }

            try
            {
                var cts = new CancellationTokenSource();

                // DesignObject
                Console.WriteLine("DesignObject");
                await nav.DesignObjectAsync(new NavObjectReference(NavObjectType.Page, 50000));

                // Test
                Console.WriteLine("Test");
                var errors = await nav.TestAsync(cts.Token);
                foreach (var error in errors)
                {
                    Console.WriteLine("Error: " + error);
                }

                // ObjectMetadata
                Console.WriteLine("ObjectMetadata");
                var idRanges = new HashSet<NavObjectIdRange>();
                idRanges.Add(new NavObjectIdRange(null, 17));
                idRanges.Add(new NavObjectIdRange(19, 36));
                idRanges.Add(new NavObjectIdRange(50003, null));
                var metadata = await nav.ObjectMetadataAsync(idRanges, cts.Token);
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
                    await nav.ExportSingleAsync(new NavObjectReference(NavObjectType.Codeunit, 50000), fileStream, cts.Token);
                }

                // ExportMultiple
                Console.WriteLine("ExportMultiple");
                await nav.ExportMultipleAsync(idRanges, "test-export-multiple.txt", cts.Token);
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
