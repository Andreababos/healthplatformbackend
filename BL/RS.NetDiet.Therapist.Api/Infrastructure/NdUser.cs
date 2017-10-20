using Microsoft.AspNet.Identity.EntityFramework;
using RS.NetDiet.Therapist.Api.Models;
using System.ComponentModel.DataAnnotations;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;

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

        [Required]
        public Title Title { get; set; }
        
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<NdUser> manager, string authenticationType)
        {
            var userIdentity = await manager.CreateIdentityAsync(this, authenticationType);
            
            return userIdentity;
        }
    }
}