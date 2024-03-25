using System.ComponentModel.DataAnnotations;

namespace BuildBazaar.Models
{
    public class NoteModel
    {
        [Key]
        public uint noteID { get; set; }
        public uint buildID { get; set; }
        public string filePath { get; set; }

    }
}