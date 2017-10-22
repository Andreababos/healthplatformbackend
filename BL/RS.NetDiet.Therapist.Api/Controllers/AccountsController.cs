using Microsoft.AspNet.Identity;
using RS.NetDiet.Therapist.Api.Infrastructure;
using RS.NetDiet.Therapist.Api.Models;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace RS.NetDiet.Therapist.Api.Controllers
{
    [RoutePrefix("api/accounts")]
    public class AccountsController : BaseApiController
    {
        [Authorize(Roles = "DevAdmin, Admin")]
        [Route("users")]
        public IHttpActionResult GetUsers()
        {
            return Ok(NdUserManager.Users.ToList().Select(u => Factory.Create(u)));
        }

        [Authorize(Roles = "DevAdmin, Admin")]
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

        [Authorize(Roles = "DevAdmin, Admin")]
        [Route("user/{username}")]
        public async Task<IHttpActionResult> GetUserByUsername(string username)
        {
            var user = await NdUserManager.FindByNameAsync(username);

            if (user != null)
            {
                return Ok(Factory.Create(user));
            }

            return NotFound();
        }

        [Authorize(Roles = "DevAdmin, Admin")]
        [Route("user/{email}")]
        public async Task<IHttpActionResult> GetUserByEmail(string email)
        {
            var user = await NdUserManager.FindByEmailAsync(email);

            if (user != null)
            {
                return Ok(Factory.Create(user));
            }

            return NotFound();
        }

        [Authorize(Roles = "DevAdmin, Admin")]
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
                Gender = createTherapistDto.Gender.Value,
                LastName = createTherapistDto.LastName,
                PhoneNumber = createTherapistDto.PhoneNumber,
                Title = createTherapistDto.Title.Value,
                UserName = createTherapistDto.Email
            };

            IdentityResult addUserResult = await NdUserManager.CreateAsync(user, PasswordGenerator.Generate());
            if (!addUserResult.Succeeded)
            {
                return GetErrorResult(addUserResult);
            }

            IdentityResult addUserToRoleResult = await NdUserManager.AddToRoleAsync(user.Id, "Therapist");
            if (!addUserToRoleResult.Succeeded)
            {
                return GetErrorResult(addUserResult);
            }

            try
            {
                Directory.CreateDirectory(HttpContext.Current.Server.MapPath(Path.Combine("~/Results", user.Id)));
            }
            catch(Exception ex)
            {
                return InternalServerError(ex);
            }

            Uri locationHeader = new Uri(Url.Link("GetUserById", new { id = user.Id }));
            return Created(locationHeader, Factory.Create(user));
        }

        [Authorize(Roles = "DevAdmin")]
        [Route("create/admin")]
        public async Task<IHttpActionResult> CreateAdmin(CreateAdminDto createAdminDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = new NdUser()
            {
                Email = createAdminDto.Email,
                EmailConfirmed = true,
                FirstName = createAdminDto.FirstName,
                Gender = createAdminDto.Gender.Value,
                LastName = createAdminDto.LastName,
                PhoneNumber = createAdminDto.PhoneNumber,
                PhoneNumberConfirmed = true,
                Title = createAdminDto.Title.Value,
                UserName = createAdminDto.Email
            };

            IdentityResult addUserResult = await NdUserManager.CreateAsync(user, createAdminDto.Password);
            if (!addUserResult.Succeeded)
            {
                return GetErrorResult(addUserResult);
            }

            IdentityResult addUserToRoleResult = await NdUserManager.AddToRoleAsync(user.Id, "Admin");
            if (!addUserToRoleResult.Succeeded)
            {
                return GetErrorResult(addUserResult);
            }

            Uri locationHeader = new Uri(Url.Link("GetUserById", new { id = user.Id }));

            return Created(locationHeader, Factory.Create(user));
        }

        [Authorize(Roles = "DevAdmin, Admin, Therapist")]
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

        [Authorize(Roles = "DevAdmin")]
        [Route("user/{id:guid}")]
        public async Task<IHttpActionResult> DeleteUser(string id)
        {
            var ndUser = await NdUserManager.FindByIdAsync(id);

            if (ndUser != null)
            {
                IdentityResult result = await NdUserManager.DeleteAsync(ndUser);

                if (!result.Succeeded)
                {
                    return GetErrorResult(result);
                }

                return Ok();

            }

            return NotFound();
        }

        [Authorize(Roles = "DevAdmin")]
        [Route("user/{id:guid}/roles")]
        [HttpPut]
        public async Task<IHttpActionResult> AssignRolesToUser([FromUri] string id, [FromBody] string[] rolesToAssign)
        {
            var ndUser = await NdUserManager.FindByIdAsync(id);

            if (ndUser == null)
            {
                return NotFound();
            }

            var currentRoles = await NdUserManager.GetRolesAsync(ndUser.Id);
            var rolesNotExists = rolesToAssign.Except(NdRoleManager.Roles.Select(x => x.Name)).ToArray();
            if (rolesNotExists.Count() > 0)
            {
                ModelState.AddModelError("", string.Format("Roles '{0}' does not exixts in the system", string.Join(",", rolesNotExists)));
                return BadRequest(ModelState);
            }

            IdentityResult removeResult = await NdUserManager.RemoveFromRolesAsync(ndUser.Id, currentRoles.ToArray());
            if (!removeResult.Succeeded)
            {
                ModelState.AddModelError("", "Failed to remove user roles");
                return BadRequest(ModelState);
            }

            IdentityResult addResult = await NdUserManager.AddToRolesAsync(ndUser.Id, rolesToAssign);
            if (!addResult.Succeeded)
            {
                ModelState.AddModelError("", "Failed to add user roles");
                return BadRequest(ModelState);
            }

            return Ok();
        }
    }
}