using Catalogue.Utilities.PathMapping;
using FluentAssertions;
using NUnit.Framework;

namespace Catalogue.Tests.Slow.Catalogue.Utilities
{
    class jncc_drive_mappings_tests
    {
        [Test]
        public void path_converted_to_unc_test()
        {
            var testPath = @"Z:\dataset_folder\my_dataset.xlsx";
            var uncPath = JnccDriveMappings.GetUncPath(testPath);

            uncPath.Should().Be(@"\\JNCC-CORPFILE\JNCC Corporate Data\dataset_folder\my_dataset.xlsx");
        }
    }
}
