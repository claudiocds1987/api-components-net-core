using System.ComponentModel.DataAnnotations;

namespace ApiComponents.Domain.Models
{
    public class Position
    {
        public int id { get; set; }
        [Required] public string description { get; set; } = string.Empty;
    }
}
