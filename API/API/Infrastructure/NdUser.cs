using RootSolutions.Common.Web.Infrastructure;
using RootSolutions.NetDiet.Therapist.API.Models;
using System.ComponentModel.DataAnnotations;

namespace RootSolutions.NetDiet.Therapist.API.Infrastructure
{
    public class NdUser : RsUser
    {
        [MaxLength(100)]
        public string Institute { get; set; }

        public string WebPage { get; set; }

        [Required]
        public Gender Gender { get; set; }

        [Required]
        public Title Title { get; set; }

        [Required]
        public bool MustChangePassword { get; set; }
    }
}