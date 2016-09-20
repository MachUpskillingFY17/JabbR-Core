using System.ComponentModel.DataAnnotations;

namespace JabbR_Core.Models
{
    public class Settings
    {
        [Key]
        public int Key { get; set; }
        public string RawSettings { get; set; }
    }
}