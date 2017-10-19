using System.ComponentModel.DataAnnotations;

namespace RS.NetDiet.Therapist.Api.Models
{
    public class CreateRoleDto
    {
        [Required]
        [StringLength(256, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 2)]
        public string Name { get; set; }
    }
}