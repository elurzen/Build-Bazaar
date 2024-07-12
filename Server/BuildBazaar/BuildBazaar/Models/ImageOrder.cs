using System.ComponentModel.DataAnnotations;

namespace BuildBazaar.Models
{
    public class ImageOrder
    {
        [Key]
        public uint imageID { get; set; }
        public int imageOrder { get; set; }

        

    }
}