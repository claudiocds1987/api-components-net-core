using System.ComponentModel.DataAnnotations;

namespace ApiComponents.Models
{
    public class Position
    {
        public int id { get; set; }
        [Required] public string description { get; set; }
    }
}
