using System.Linq;
using System.Web.Http;
using Catalogue.Gemini.DataFormats;
using Catalogue.Utilities.Text;
using FluentAssertions;
using NUnit.Framework;

namespace Catalogue.Web.Controllers.Formats
{
    public class FormatsController : ApiController
    {
        public DataFormatGroupCollection Get(string q)
        {
            // slightly weird projection - in order to filter out the formats (and groups)
            // we don't want, we have to query and then reproject into the correct static type

            if (q.IsNotBlank())
            {
                var query = from g in DataFormats.Known
                            from f in g.Formats
                            where g.Name.ToLowerInvariant().Contains(q)
                                     || f.Name.ToLowerInvariant().Contains(q)
                                     || f.Code.ToLowerInvariant().Contains(q)
                            group f by g into formats
                            select new DataFormatGroup
                            {

                                Name = formats.Key.Name,
                                Glyph = formats.Key.Glyph,
                                Formats = new DataFormatCollection(formats)
                            };

                return new DataFormatGroupCollection(query);                
            }
            else
            {
                return DataFormats.Known;
            }
        }
    }

    class formats_controller_tests
    {
        [Test]
        public void should_return_all_formats_when_query_is_null()
        {
            var c = new FormatsController();

            var result = c.Get(null);

            result.Should().Contain(g => g.Name == "Database");
            result.Should().Contain(g => g.Name == "Geospatial");
            result.Count.Should().BeGreaterThan(5);
        }

        [Test]
        public void should_return_correct_results_for_microsoft_query()
        {
            var c = new FormatsController();

            var result = c.Get("microsoft");

            result.SelectMany(g => g.Formats)
                .Should().OnlyContain(f => f.Name.ToLowerInvariant().Contains("microsoft"));
        }
    }

}