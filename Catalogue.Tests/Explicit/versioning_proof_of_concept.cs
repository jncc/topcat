using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Catalogue.Data.Model;
using Catalogue.Tests.Utility;
using FluentAssertions;
using NUnit.Framework;
using Raven.Client.Bundles.Versioning;

namespace Catalogue.Tests.Explicit
{
    class versioning_proof_of_concept : DatabaseTestFixture
    {
        [Test, Explicit]
        public void try_out_versioning()
        {


            var record = Db.Query<Record>().First(r => r.Gemini.Title == "Moray Firth benthic biotope map");
            string id = "records/" + record.Id;

            record.Notes = "blah";
            Db.SaveChanges();

            var revisions = Db.Advanced.GetRevisionsFor<Record>(id, 0, 10);

            record.Should().NotBeNull();
            revisions.Should().NotBeEmpty();
        }
    }
}
