using RootSolutions.NetDiet.Therapist.API.Models;
using System.ComponentModel.DataAnnotations;

namespace RootSolutions.NetDiet.Therapist.API.Dtos
{
    public class CreateTherapistDto
    {
        [Required]
        [EmailAddress]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public Gender Gender { get; set; }

        [Required]
        public string Institute { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        [Phone]
        [DataType(DataType.PhoneNumber)]
        public string PhoneNumber { get; set; }

        [Required]
        public Title Title { get; set; }

        public string WebPage { get; set; }
    }
}