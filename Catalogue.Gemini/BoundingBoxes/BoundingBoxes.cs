using System.Collections.Generic;
using Catalogue.Gemini.Model;

namespace Catalogue.Gemini.BoundingBoxes
{
    public static class BoundingBoxes
    {
        /// <summary>
        /// A list of known boxes, grouped into groups.
        /// </summary>
        public static List<BoundingBoxGroup> Groups = new List<BoundingBoxGroup>
        {
            new BoundingBoxGroup
            {
                Name = "Overseas Territories", Boxes = new List<KnownBoundingBox>
                {
                    new KnownBoundingBox { Name = "Anguilla", Box = new BoundingBox { North = 21.9259m, West = -63.8981m, South = 17.9476m, East = -60.6832m } },
                    new KnownBoundingBox { Name = "Ascension", Box = new BoundingBox { North = -4.5588m, West = -17.7888m, South = -11.3278m, East = -10.9331m } },
                    new KnownBoundingBox { Name = "Bermuda", Box = new BoundingBox { North = -60.7055m, West = -68.8268m, South = 28.9132m, East = 35.7224m } },
//                  new KnownBoundingBox { Name = "British Antarctic Territory", Box = new BoundingBox { North = 0m, West = 0m, South = 0m, East = 0m } },
                    new KnownBoundingBox { Name = "British Indian Ocean Territory", Box = new BoundingBox { North = -2.2838m, West = 67.8877m, South = -10.7916m, East = 75.8501m } },
                    new KnownBoundingBox { Name = "British Virgin Islands", Box = new BoundingBox { North = 22.0819m, West = -65.8426m, South = 17.9642m, East = -63.2995m } },
                    new KnownBoundingBox { Name = "Cayman Islands", Box = new BoundingBox { North = 20.6724m, West = -83.5972m, South = 17.5841m, East = -78.7236m } },
                    new KnownBoundingBox { Name = "Falkland Islands", Box = new BoundingBox { North = -47.6801m, West = -65.0039m, South = -56.2455m, East = -52.3114m } },
                    new KnownBoundingBox { Name = "Gibraltar", Box = new BoundingBox { North = 36.1627m, West = -5.3995m, South = 36.0099m, East = -4.8924m } },
                    new KnownBoundingBox { Name = "Montserrat", Box = new BoundingBox { North = 17.0349m, West = -63.0455m, South = 15.8419m, East = -61.8228m } },
                    new KnownBoundingBox { Name = "Pitcairn Islands", Box = new BoundingBox { North = -20.5819m, West = -133.4247m, South = -28.4096m, East = -121.1124m } },
                    new KnownBoundingBox { Name = "South Georgia and South Sandwich Islands", Box = new BoundingBox { North = -50.1569m, West = -48.0574m, South = -62.7833m, East = -19.8471m } },
//                  new KnownBoundingBox { Name = "Sovereign Base Areas Cyprus", Box = new BoundingBox { North = 0m, West = 0m, South = 0m, East = 0m } },
                    new KnownBoundingBox { Name = "St. Helena", Box = new BoundingBox { North = -2.1668m, West = -9.2564m, South = -19.362m, East = -12.5713m } },
                    new KnownBoundingBox { Name = "Tristan da Cunha", Box = new BoundingBox { North = -5.5058m, West = -16.8885m, South = -43.7033m, East = -33.743m } },
                    new KnownBoundingBox { Name = "Turks and Caicos Islands", Box = new BoundingBox { North = -67.6678m, West = -72.8136m, South = 20.5453m, East = 25.0624m } },
                }
            }
        }; 
    }

    public class BoundingBoxGroup
    {
        public string Name { get; set; }
        public List<KnownBoundingBox> Boxes { get; set; }
    }

    public class KnownBoundingBox
    {
        public string Name { get; set; }
        public BoundingBox Box { get; set; }
    }
}
