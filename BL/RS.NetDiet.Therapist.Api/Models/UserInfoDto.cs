namespace RS.NetDiet.Therapist.Api.Models
{
    public class UserInfoDto
    {
        public string Id { get; set; }

        public Title Title { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Email { get; set; }

        public string Clinic { get; set; }

        public Gender Gender { get; set; }

        public string PhoneNumber { get; set; }
    }
}