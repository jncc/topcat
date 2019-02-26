namespace Catalogue.Data.Model
{
    public class Image
    {
        public string Url { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public ImageCrops Crops { get; set; }
    }

    public class ImageCrops
    {
        public string SquareUrl { get; set; }
        public string ThumbnailUrl { get; set; }
    }
}
