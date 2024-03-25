using System.ComponentModel.DataAnnotations;

namespace BuildBazaar.Models
{
    public class ImageModel
    {
        [Key]
        public uint imageID { get; set; }
        public uint buildID { get; set; }
        public string filePath { get; set; }

    }
}