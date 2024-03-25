using System.ComponentModel.DataAnnotations;

namespace BuildBazaar.Models
{
    public class BuildModel
    {
        [Key]
        public uint buildID { get; set; }
        public uint userID { get; set; }
        public uint imageID { get; set; }
        public string buildName { get; set; }        
        public string filePath { get; set; }

    }
}