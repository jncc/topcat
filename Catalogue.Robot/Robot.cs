using Catalogue.Data.Model;
using Raven.Client;
using System;
using System.Linq;

namespace Catalogue.Robot
{
    public class Robot
    {
        // todo: split "instance" stuff out into a separate class

        readonly IDocumentStore store;

        public Robot(IDocumentStore store)
        {
            this.store = store;
        }

        public void Start()
        {
            Console.WriteLine("I'm a robot");

            using (var db = store.OpenSession())
            {
                var record = db.Query<Record>().First(r => r.Gemini.Title.StartsWith("sea"));
                Console.WriteLine(record.Gemini.Title);
            }
        }

        public void Stop()
        {
            Console.WriteLine("I'm stopping");
        }


    }
}
