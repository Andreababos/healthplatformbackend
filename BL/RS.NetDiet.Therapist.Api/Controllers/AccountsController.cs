using Microsoft.AspNet.Identity;
using RS.NetDiet.Therapist.Api.Infrastructure;
using RS.NetDiet.Therapist.Api.Models;
using System;
using System.Threading.Tasks;
using System.Web.Http;

namespace RS.NetDiet.Therapist.Api.Controllers
{
    [RoutePrefix("api/accounts")]
    public class AccountsController : BaseApiController
    {
        //[Route("users")]
        //public IHttpActionResult GetUsers()
        //{
        //    return Ok(NdUserManager.Users.ToList().Select(u => Factory.Create(u)));
        //}

        [Route("user/{id:guid}", Name = "GetUserById")]
        public async Task<IHttpActionResult> GetUser(string Id)
        {
            var user = await NdUserManager.FindByIdAsync(Id);

            if (user != null)
            {
                return Ok(Factory.Create(user));
            }

            return NotFound();
        }

        //[Route("user/{username}")]
        //public async Task<IHttpActionResult> GetUserByName(string username)
        //{
        //    var user = await NdUserManager.FindByNameAsync(username);

        //    if (user != null)
        //    {
        //        return Ok(Factory.Create(user));
        //    }

        //    return NotFound();
        //}

        [Route("create/therapist")]
        public async Task<IHttpActionResult> CreateTherapis(CreateTherapistDto createTherapistDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = new NdUser()
            {
                Clinic = createTherapistDto.Clinic,
                Email = createTherapistDto.Email,
                FirstName = createTherapistDto.FirstName,
                Gender = createTherapistDto.Gender,
                LastName = createTherapistDto.LastName,
                PhoneNumber = createTherapistDto.PhoneNumber,
                UserName = createTherapistDto.Email
            };

            IdentityResult addUserResult = await NdUserManager.CreateAsync(user, createTherapistDto.Password);

            if (!addUserResult.Succeeded)
            {
                return GetErrorResult(addUserResult);
            }

            Uri locationHeader = new Uri(Url.Link("GetUserById", new { id = user.Id }));

            return Created(locationHeader, Factory.Create(user));
        }

        [Route("changepassword")]
        public async Task<IHttpActionResult> ChangePassword(ChangePasswordDto changePasswordDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            IdentityResult result = await NdUserManager.ChangePasswordAsync(User.Identity.GetUserId(), changePasswordDto.OldPassword, changePasswordDto.NewPassword);

            if (!result.Succeeded)
            {
                return GetErrorResult(result);
            }

            return Ok();
        }
    }
}