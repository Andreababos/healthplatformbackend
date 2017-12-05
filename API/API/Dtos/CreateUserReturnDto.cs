using System.Collections.Generic;

namespace RootSolutions.NetDiet.Therapist.API.Dtos
{
    public class CreateUserReturnDto : UserInfoDto
    {
        public string Url { get; set; }

        public IEnumerable<string> Roles { get; set; }
    }
}