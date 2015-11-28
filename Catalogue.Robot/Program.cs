using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Catalogue.Data;
using Catalogue.Data.Model;
using Catalogue.Data.Query;
using Catalogue.Robot.Publishing.DataGovUk;
using Raven.Client;
using Raven.Client.Document;

namespace Catalogue.Robot
{
    class Program
    {
        public static IDocumentStore DocumentStore { get; private set; }

        static void Main(string[] args)
        {
            Init();

            using (var db = DocumentStore.OpenSession())
            {
                if (args.First() == "mark")
                {
                    string keyword = args.Skip(1).First();

                    var recordQueryer = new RecordQueryer(db);
                    var query = new RecordQueryInputModel
                        {
                            K = new[] { keyword },
                            N = 1024,
                        };

                    int count = recordQueryer.Query(query).Count();

                    var records = recordQueryer.Query(query).ToList();

                    if (records.Count != count)
                        throw new Exception("Too many records.");


                    foreach (var record in records)
                    {
                        if (record.Publication == null)
                            record.Publication = new PublicationInfo();

                        record.Publication.DataGovUkPublisher = new DataGovUkPublisher();
                    }
                    db.SaveChanges();
                    Console.WriteLine("Marked {0} records.", records.Count);
                }
            }

//            var record = Db.Load<Record>(new Guid("679434f5-baab-47b9-98e4-81c8e3a1a6f9"));
//            record.Gemini.ResourceLocator = String.Format("http://example.com/{0}", record.Id);
//            var xml = new global::Catalogue.Gemini.Encoding.XmlEncoder().Create(record.Id, record.Gemini);
//            string filename = "topcat-record-" + record.Id.ToString().ToLower() + ".xml";
//            xml.Save(Path.Combine(@"C:\work", filename));
        }


        static void Init()
        {
            try
            {
                // for local dev, we expect to use the db running in the local dev web app,
                // so the DatabaseFactory just uses the dev connection string in the dev app.config file
                DocumentStore = DatabaseFactory.Production();
            }
            catch (HttpRequestException ex)
            {                
                throw new Exception("Unable to connect to the Topcat database.", ex);
            }
        }
    }
}
