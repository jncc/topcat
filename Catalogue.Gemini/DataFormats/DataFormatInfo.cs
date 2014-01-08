using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catalogue.Gemini.DataFormats
{
    public static class DataFormats
    {
        public static readonly DataFormatGroupCollection Known = new DataFormatGroupCollection
            {
                 { "Spreadsheet", "glyphicon-list", new DataFormatInfoCollection
                     {
                         { "xls", "Microsoft Excel for Windows" },
                         { "xlsx", "Microsoft Excel for Windows" },
                     }
                 },
                 { "Database", "glyphicon-hdd", new DataFormatInfoCollection
                     {
                         { "mdb", "Microsoft Access Database" },
                         { "???", "Microsoft SQL Server Database" }, // not apparently in pronom
                     }
                 },
                 { "Text", "glyphicon-font", new DataFormatInfoCollection // glyphicon-align-left ?
                     {
                         { "csv", "Comma Separated Values" },
                         { "tsv", "Tab-separated values" }, // nice consistency, pronom (!)
                     }
                 },
                 { "Geospatial", "glyphicon-map-marker", new DataFormatInfoCollection
                     {
                         { "", "" },
                     }
                 },
            };

        // spreadsheet: glyphicon-list
        // database: glyphicon-hdd
        // csv, tsv: glyphicon-align-left glyphicon-font
        // spatial: glyphicon-map-marker
        // audio: glyphicon-music glyphicon-volume-up
        // image: glyphicon-picture glyphicon-eye-open glyphicon-tree-deciduous (?)
        // video: glyphicon-facetime-video glyphicon-film
        // default: glyphicon-th
    }

    public class DataFormatInfo
    {
        public string Code { get; set; }
        public string Name { get; set; }
    }

    public class DataFormatGroup
    {
        public string Name { get; set; }
        public string Glyph { get; set; }
        public DataFormatInfoCollection Formats { get; set; }
    }

    //
    // helper classes to make use of C# collection initializers
    //

    public class DataFormatGroupCollection : List<DataFormatGroup>
    {
        public void Add(string name, string glyph, DataFormatInfoCollection formats)
        {
            Add(new DataFormatGroup { Name = name, Glyph = glyph, Formats = formats });
        }
    }

    public class DataFormatInfoCollection : List<DataFormatInfo>
    {
        public void Add(string code, string name)
        {
            Add(new DataFormatInfo { Code= code, Name = name });
        }
    }
}

