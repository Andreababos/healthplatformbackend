using Microsoft.AspNet.Identity.EntityFramework;
using RS.NetDiet.Therapist.Api.Models;
using System.ComponentModel.DataAnnotations;

namespace RS.NetDiet.Therapist.Api.Infrastructure
{
    public class NdUser : IdentityUser
    {
        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; }

        [Required]
        [MaxLength(100)]
        public string LastName { get; set; }

        [MaxLength(100)]
        public string Clinic { get; set; }

        [Required]
        public Gender Gender { get; set; }
    }
}