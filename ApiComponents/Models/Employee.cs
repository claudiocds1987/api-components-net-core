
using System;
using System.ComponentModel.DataAnnotations;


namespace ApiComponents.Models
{
    public class Employee
    {
        public int id { get; set; }
        [Required] public string name { get; set; }

        [Required] public string surname { get; set; }

        [Required] public int countryId { get; set; }

        [Required] public DateTime birthDate { get; set; }

        [Required] public int positionId { get; set; }

        [Required] public bool active { get; set; }

        public string imgUrl { get; set; }

        [Required] public int genderId { get; set; }
    }
}
