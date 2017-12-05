using RootSolutions.NetDiet.Therapist.API.Models;

namespace RootSolutions.NetDiet.Therapist.API.Dtos
{
    public class UserInfoDto
    {
        public string Email { get; set; }

        public string FirstName { get; set; }

        public Gender Gender { get; set; }

        public string Id { get; set; }

        public string Institute { get; set; }

        public string LastName { get; set; }
        
        public string PhoneNumber { get; set; }

        public Title Title { get; set; }

        public string WebPage { get; set; }
    }
}