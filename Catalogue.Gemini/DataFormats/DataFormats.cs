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
                { "Database", "glyphicon-hdd", new DataFormatInfoCollection
                    {
                        { "medin", "Database" }, // http://vocab.ndg.nerc.ac.uk/term/M010/1/DB
                        { "mdb", "Microsoft Access Database" },
                        { "mdf", "Microsoft SQL Server Database" }, // made up - not apparently in pronom
                        { "gdb", "InterBase Database" },
                    }
                },
                { "Spreadsheet", "glyphicon-list", new DataFormatInfoCollection
                    {
                        { "medin", "Delimited" }, // http://vocab.ndg.nerc.ac.uk/term/M010/1/DEL
                        { "csv", "Comma Separated Values" },
                        { "tsv", "Tab-separated values" }, // nice consistency, pronom (!)
                        { "xls", "Microsoft Excel for Windows" },
                        { "xlsx", "Microsoft Excel for Windows" },
                    }
                },
                { "Documents", "glyphicon-font", new DataFormatInfoCollection
                    {
                        { "medin", "Documents"}, // http://vocab.ndg.nerc.ac.uk/term/M010/1/DOC
                        { "medin", "Text" }, // http://vocab.ndg.nerc.ac.uk/term/M010/1/TXT
                        { "docx", "Microsoft Word for Windows "},
                        { "pdf", "Acrobat Portable Document Format" } // made up - pronom only appears to contain specific versions
                    }
                },
                { "Geospatial", "glyphicon-map-marker", new DataFormatInfoCollection
                    {
                        { "medin", "Geographic Information System" }, // http://vocab.ndg.nerc.ac.uk/term/M010/1/GIS
                        { "medin", "Google Earth and Oceans" }, // http://vocab.ndg.nerc.ac.uk/term/M010/1/KMX
                        { "shp", "ESRI Arc/View ShapeFile" },
                    }
                },
                { "Image", "glyphicon-picture", new DataFormatInfoCollection
                    {
                        { "medin", "Image" }, // http://vocab.ndg.nerc.ac.uk/term/M010/1/IMG
                        { "png", "Portable Network Graphics" },
                        { "jpg", "JPEG File Interchange Format" },
                        { "tiff", "Tagged Image File Format" },
                    }
                },
                { "Audio", "glyphicon-volume-up", new DataFormatInfoCollection 
                    {
                        { "mp3", "MPEG 1/2 Audio Layer 3" },
                    }
                },
                { "Video", "glyphicon-facetime-video", new DataFormatInfoCollection
                    {
                        { "medin", "Movie" }, // http://vocab.ndg.nerc.ac.uk/term/M010/1/MOV
                        { "mpg", "MPEG-2 Video Format" },
                    }
                },
                { "Other", "glyphicon-th", new DataFormatInfoCollection()
                    // this provides the fall-through default glyph
                },
        };
    }

    public class DataFormatGroup
    {
        public string Name { get; set; }
        public string Glyph { get; set; }
        public DataFormatInfoCollection Formats { get; set; }
    }

    public class DataFormatInfo
    {
        public string Code { get; set; }
        public string Name { get; set; }
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

