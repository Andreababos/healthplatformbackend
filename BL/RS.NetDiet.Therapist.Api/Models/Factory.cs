using RS.NetDiet.Therapist.Api.Infrastructure;
using System.Net.Http;
using System.Web.Http.Routing;

namespace RS.NetDiet.Therapist.Api.Models
{
    public class Factory
    {
        private UrlHelper _UrlHelper;
        private NdUserManager _NdUserManager;

        public Factory(HttpRequestMessage request, NdUserManager ndUserManager)
        {
            _UrlHelper = new UrlHelper(request);
            _NdUserManager = ndUserManager;
        }

        public UserReturnDto Create(NdUser ndUser)
        {
            return new UserReturnDto
            {
                Url = _UrlHelper.Link("GetUserById", new { id = ndUser.Id }),
                Id = ndUser.Id,
                Email = ndUser.Email,
                Roles = _NdUserManager.GetRolesAsync(ndUser.Id).Result,
                Clinic = ndUser.Clinic,
                FirstName = ndUser.FirstName,
                Gender = ndUser.Gender,
                LastName = ndUser.LastName,
                PhoneNumber = ndUser.PhoneNumber
            };
        }
    }
}