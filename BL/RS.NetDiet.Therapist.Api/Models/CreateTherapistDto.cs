using System.ComponentModel.DataAnnotations;

namespace RS.NetDiet.Therapist.Api.Models
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
        public string LastName { get; set; }

        [Required]
        public Gender? Gender { get; set; }

        [Required]
        public Title? Title { get; set; }

        [Required]
        [Phone]
        [DataType(DataType.PhoneNumber)]
        public string PhoneNumber { get; set; }

        [Required]
        public string Clinic { get; set; }
    }
}