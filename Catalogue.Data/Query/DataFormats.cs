using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using static Catalogue.Data.Query.DataFormatGroups;

namespace Catalogue.Data.Query
{
    public enum DataFormatGroups { Database, Spreadsheet, Documents, Geospatial, Image, Audio, Video, Other }

    public enum DataTypes
    {
        [DataFormatAttr("Database (medin)", Database)]
        DatabaseMedin,
        [DataFormatAttr("Microsoft Access Database (mdb)", Database)]
        DatabaseMicrosoftAccess,
        [DataFormatAttr("Microsoft SQL Server Database (mdf)", Database)]
        DatabaseMicrosoftSqlServer,
        [DataFormatAttr("InterBase Database (gdb)", Database)]
        DatabaseInterBase,
        [DataFormatAttr("Delimited (medin)", Spreadsheet)]
        SpreadsheetMedin,
        [DataFormatAttr("Comma Separated Values (csv)", Spreadsheet)]
        SpreadsheetCsv,
        [DataFormatAttr("Tab-separated values (tsv)", Spreadsheet)]
        SpreadsheetTsv,
        [DataFormatAttr("Microsoft Excel for Windows (xls)", Spreadsheet)]
        SpreadsheetXls,
        [DataFormatAttr("Microsoft Excel for Windows (xlsx)", Spreadsheet)]
        SpreadsheetXlsx,
        [DataFormatAttr("Documents (medin)", Documents)]
        DocumentsMedin,
        [DataFormatAttr("Text (medin)", Documents)]
        DocumentsTextMedin,
        [DataFormatAttr("Microsoft Word for Windows (docx)", Documents)]
        DocumentsMicrosoftWord,
        [DataFormatAttr("Acrobat Portable Document Format (pdf)", Documents)]
        DocumentsPdf,
        [DataFormatAttr("Geographic Information System (medin)", Geospatial)]
        GeospatialGisMedin,
        [DataFormatAttr("ESRI Arc/View ShapeFile (shp)", Geospatial)]
        GeospatialShapeFile,
        [DataFormatAttr("Geospatial (raster)", Geospatial)]
        GeospatialRaster,
        [DataFormatAttr("Geospatial (vector polygon)", Geospatial)]
        GeospatialVectorPolygon,
        [DataFormatAttr("Geospatial (vector line)", Geospatial)]
        GeospatialVectorLine,
        [DataFormatAttr("Geospatial (vector point)", Geospatial)]
        GeospatialVectorPoint,
        [DataFormatAttr("Image (medin)", Image)]
        ImageMedin,
        [DataFormatAttr("Portable Network Graphics (png)", Image)]
        ImagePng,
        [DataFormatAttr("JPEG File Interchange Format (jpg)", Image)]
        ImageJpeg,
        [DataFormatAttr("Tagged Image File Format (tiff)", Image)]
        ImageTiff,
        [DataFormatAttr("MPEG 1/2 Audio Layer 3 (mp3)", Audio)]
        AudioMp3,
        [DataFormatAttr("Movie (medin)", Video)]
        VideoMovieMedin,
        [DataFormatAttr("MPEG-2 Video Format (mpg)", Video)]
        VideoMpg,
        [DataFormatAttr("Other", Other)]
        OtherFormat
    }

    public static class DataFormats
    {
        public static string GetName(this DataTypes formats)
        {
            DataFormatAttr attr = GetAttr(formats);
            return attr.Name;
        }

        public static DataFormatGroups GetGroup(this DataTypes formats)
        {
            DataFormatAttr attr = GetAttr(formats);
            return attr.Group;
        }

        public static List<string> GetFormatsForGroup(DataFormatGroups group)
        {
            var result = new List<string>();
            var dataFormats = Enum.GetValues(typeof(DataTypes)).Cast<DataTypes>();

            foreach (var dataFormat in dataFormats)
            {
                var dataFormatGroup = dataFormat.GetGroup();
                if (dataFormatGroup.Equals(group))
                    result.Add(dataFormat.GetName());
            }
            return result;
        }

        private static DataFormatAttr GetAttr(DataTypes formats)
        {
            return (DataFormatAttr)Attribute.GetCustomAttribute(ForValue(formats), typeof(DataFormatAttr));
        }

        private static MemberInfo ForValue(DataTypes formats)
        {
            return typeof(DataTypes).GetField(Enum.GetName(typeof(DataTypes), formats));
        }

        private static MemberInfo ForGroup(DataFormatGroups group)
        {
            return typeof(Query.DataFormats).GetField(Enum.GetName(typeof(DataTypes), group));
        }

    }
}
