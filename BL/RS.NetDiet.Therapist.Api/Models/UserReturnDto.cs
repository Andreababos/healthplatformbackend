using System.Collections.Generic;

namespace RS.NetDiet.Therapist.Api.Models
{
    public class UserReturnDto
    {
        public string Url { get; set; }

        public string Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Email { get; set; }

        public string Clinic { get; set; }

        public string PhoneNumber { get; set; }

        public Gender? Gender { get; set; }
        
        public IList<string> Roles { get; set; }
    }
}