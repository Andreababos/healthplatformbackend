using Microsoft.AspNet.Identity;
using RootSolutions.Common.Web.Infrastructure;
using RootSolutions.NetDiet.Therapist.API.Infrastructure;
using System.Net.Http;
using System.Web.Http.Routing;

namespace RootSolutions.NetDiet.Therapist.API.Dtos
{
    public static class DtoMapper
    {
        public static UserInfoDto CreateUserInfoDto(this NdUser user)
        {
            return new UserInfoDto()
            {
                Email = user.Email,
                FirstName = user.FirstName,
                Gender = user.Gender,
                Id = user.Id,
                Institute = user.Institute,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                Title = user.Title,
                WebPage = user.WebPage
            };
        }

        public static CreateUserReturnDto CreateCreateUserReturnDto(this NdUser user, UserManager<NdUser> userManager, HttpRequestMessage request)
        {
            CreateUserReturnDto createUserReturnDto = (CreateUserReturnDto)user.CreateUserInfoDto();
            createUserReturnDto.Roles = userManager.GetRolesAsync(user.Id).Result;
            createUserReturnDto.Url = (new UrlHelper(request)).Link("GetUserById", new { id = user.Id });

            return createUserReturnDto;
        }
    }
}