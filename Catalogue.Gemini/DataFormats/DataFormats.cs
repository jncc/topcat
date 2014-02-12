using System.Collections.Generic;

namespace Catalogue.Gemini.DataFormats
{
    public static class DataFormats
    {
        /// <summary>
        /// A list of known / common formats, categorised into simplified groups.
        /// Combines common Medin and PRONOM file formats, and will need to be added to.
        /// Please take care when doing so.
        /// </summary>
        public static readonly DataFormatGroupCollection Known = new DataFormatGroupCollection
        {
            { "Database", "glyphicon-hdd", new DataFormatCollection
                {
                    { "medin", "Database" }, // http://vocab.ndg.nerc.ac.uk/term/M010/1/DB
                    { "mdb", "Microsoft Access Database" },
                    { "mdf", "Microsoft SQL Server Database" }, // made up - not in pronom
                    { "gdb", "InterBase Database" },
                }
            },
            { "Spreadsheet", "glyphicon-list", new DataFormatCollection
                {
                    { "medin", "Delimited" }, // http://vocab.ndg.nerc.ac.uk/term/M010/1/DEL
                    { "csv", "Comma Separated Values" },
                    { "tsv", "Tab-separated values" }, // nice consistency, pronom (!)
                    { "xls", "Microsoft Excel for Windows" },
                    { "xlsx", "Microsoft Excel for Windows" },
                }
            },
            { "Documents", "glyphicon-font", new DataFormatCollection
                {
                    { "medin", "Documents"}, // http://vocab.ndg.nerc.ac.uk/term/M010/1/DOC
                    { "medin", "Text" }, // http://vocab.ndg.nerc.ac.uk/term/M010/1/TXT
                    { "docx", "Microsoft Word for Windows "},
                    { "pdf", "Acrobat Portable Document Format" } // made up - pronom only appears to contain specific versions
                }
            },
            { "Geospatial", "glyphicon-map-marker", new DataFormatCollection
                {
                    { "medin", "Geographic Information System" }, // http://vocab.ndg.nerc.ac.uk/term/M010/1/GIS
                    { "shp", "ESRI Arc/View ShapeFile" },
                    { "", "Geospatial (raster)" },
                    { "", "Geospatial (vector polygon)" },
                    { "", "Geospatial (vector line)" },
                    { "", "Geospatial (vector point)" },
                }
            },
            { "Image", "glyphicon-picture", new DataFormatCollection
                {
                    { "medin", "Image" }, // http://vocab.ndg.nerc.ac.uk/term/M010/1/IMG
                    { "png", "Portable Network Graphics" },
                    { "jpg", "JPEG File Interchange Format" },
                    { "tiff", "Tagged Image File Format" },
                }
            },
            { "Audio", "glyphicon-volume-up", new DataFormatCollection 
                {
                    { "mp3", "MPEG 1/2 Audio Layer 3" },
                }
            },
            { "Video", "glyphicon-facetime-video", new DataFormatCollection
                {
                    { "medin", "Movie" }, // http://vocab.ndg.nerc.ac.uk/term/M010/1/MOV
                    { "mpg", "MPEG-2 Video Format" },
                }
            },
            { "Other", "glyphicon-th", new DataFormatCollection()
                // this provides the fall-through default glyph
            },
        };
    }

    public class DataFormatGroup
    {
        public string Name { get; set; }
        public string Glyph { get; set; }
        public DataFormatCollection Formats { get; set; }
    }

    public class DataFormat
    {
        public string Code { get; set; }
        public string Name { get; set; }
    }

    //
    // helper classes to make use of C# collection initializers
    //

    public class DataFormatGroupCollection : List<DataFormatGroup>
    {
        public void Add(string name, string glyph, DataFormatCollection formats)
        {
            Add(new DataFormatGroup { Name = name, Glyph = glyph, Formats = formats });
        }
    }

    public class DataFormatCollection : List<DataFormat>
    {
        public void Add(string code, string name)
        {
            Add(new DataFormat { Code= code, Name = name });
        }
    }
}

