using Catalogue.Utilities.DriveMapping;
using FluentAssertions;
using NUnit.Framework;

namespace Catalogue.Tests.Slow.Catalogue.Utilities
{
    class jncc_drive_mappings_tests
    {
        [Test]
        public void z_path_converted_to_unc_test()
        {
            var testPath = @"Z:\dataset_folder\my_dataset.xlsx";
            var uncPath = JnccDriveMappings.GetUncPath(testPath);

            uncPath.Should().Be(@"\\JNCC-CORPFILE\JNCC Corporate Data\dataset_folder\my_dataset.xlsx");
        }

        [Test]
        public void g_path_converted_to_unc_test()
        {
            var testPath = @"G:\dataset_folder\my_dataset.xlsx";
            var uncPath = JnccDriveMappings.GetUncPath(testPath);

            uncPath.Should().Be(@"\\JNCC-CORPFILE\Corporate Apps\dataset_folder\my_dataset.xlsx");
        }

        [Test]
        public void j_path_converted_to_unc_test()
        {
            var testPath = @"J:\dataset_folder\my_dataset.xlsx";
            var uncPath = JnccDriveMappings.GetUncPath(testPath);

            uncPath.Should().Be(@"\\JNCC-CORPFILE\gis\dataset_folder\my_dataset.xlsx");
        }

        [Test]
        public void p_path_converted_to_unc_test()
        {
            var testPath = @"P:\dataset_folder\my_dataset.xlsx";
            var uncPath = JnccDriveMappings.GetUncPath(testPath);

            uncPath.Should().Be(@"\\JNCC-CORPFILE\Purchase-Logs\dataset_folder\my_dataset.xlsx");
        }

        [Test]
        public void y_path_converted_to_unc_test()
        {
            var testPath = @"Y:\dataset_folder\my_dataset.xlsx";
            var uncPath = JnccDriveMappings.GetUncPath(testPath);

            uncPath.Should().Be(@"\\JNCC-CORPFILE\Teams and Staff\dataset_folder\my_dataset.xlsx");
        }

        [Test]
        public void x_path_converted_to_unc_test()
        {
            var testPath = @"X:\dataset_folder\my_dataset.xlsx";
            var uncPath = JnccDriveMappings.GetUncPath(testPath);

            uncPath.Should().Be(@"\\JNCC-CORPFILE\Marine Survey\dataset_folder\my_dataset.xlsx");
        }

        [Test]
        public void h_path_converted_to_unc_test()
        {
            var testPath = @"H:\dataset_folder\my_dataset.xlsx";
            var uncPath = JnccDriveMappings.GetUncPath(testPath);

            uncPath.Should().Be(@"\\JNCC-ABNAS2\private\dataset_folder\my_dataset.xlsx");
        }

        [Test]
        public void l_path_converted_to_unc_test()
        {
            var testPath = @"L:\dataset_folder\my_dataset.xlsx";
            var uncPath = JnccDriveMappings.GetUncPath(testPath);

            uncPath.Should().Be(@"\\JNCC-ABNAS\marine-data\dataset_folder\my_dataset.xlsx");
        }

        [Test]
        public void r_path_converted_to_unc_test()
        {
            var testPath = @"R:\dataset_folder\my_dataset.xlsx";
            var uncPath = JnccDriveMappings.GetUncPath(testPath);

            uncPath.Should().Be(@"\\JNCC-ABNAS2\reference_material\dataset_folder\my_dataset.xlsx");
        }

        [Test]
        public void t_path_converted_to_unc_test()
        {
            var testPath = @"T:\dataset_folder\my_dataset.xlsx";
            var uncPath = JnccDriveMappings.GetUncPath(testPath);

            uncPath.Should().Be(@"\\JNCC-ABNAS2\cfs\dataset_folder\my_dataset.xlsx");
        }

        [Test]
        public void path_converted_to_unc_test_with_wrong_case()
        {
            var testPath = @"z:\dataset_folder\my_dataset.xlsx";
            var uncPath = JnccDriveMappings.GetUncPath(testPath);

            uncPath.Should().Be(@"\\JNCC-CORPFILE\JNCC Corporate Data\dataset_folder\my_dataset.xlsx");
        }

        [Test]
        public void path_already_unc_path_test()
        {
            var testPath = @"\\jncc-corpfile\jncc corporate data\dataset_folder\my_dataset.xlsx";
            var uncPath = JnccDriveMappings.GetUncPath(testPath);

            uncPath.Should().Be(@"\\jncc-corpfile\jncc corporate data\dataset_folder\my_dataset.xlsx");
        }
    }
}
