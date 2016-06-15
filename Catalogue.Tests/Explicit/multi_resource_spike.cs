using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Catalogue.Utilities.Text;
using CsvHelper;
using FluentAssertions;
using NUnit.Framework;

namespace Catalogue.Tests.Explicit
{
    class multi_resource_spike : DatabaseTestFixture
    {
        [Explicit, Test]
        public void do_it()
        {
            string path = @"c:\work\id-list.csv";

            using (var reader = new StreamReader(path))
            {
                var csv = new CsvReader(reader);

                csv.Configuration.HasHeaderRecord = false;

                var list = new List<Tuple<string, List<string>>>();

                while (csv.Read())
                {
                    var id = csv.GetField<string>(0);
                    var resourcePaths = csv.CurrentRecord.Skip(1)
                        .Where(value => value.IsNotBlank())
                        .ToList();

                    list.Add(new Tuple<string, List<string>>(id, resourcePaths));
                }

                list.Count.Should().Be(2);
                list[0].Item1.Should().Be("id1");
                list[0].Item2.Should().Contain(@"x:\some\path\2");
            }

        }
    }
}
