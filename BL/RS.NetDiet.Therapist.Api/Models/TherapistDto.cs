namespace RS.NetDiet.Therapist.Api.Models
{
    public class TherapistDto
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public Gender Gender { get; set; }

        public Title Title { get; set; }

        public string Clinic { get; set; }
    }
}