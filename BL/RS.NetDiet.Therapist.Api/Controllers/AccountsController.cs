using Microsoft.AspNet.Identity;
using RS.NetDiet.Therapist.Api.Infrastructure;
using RS.NetDiet.Therapist.Api.Models;
using RS.NetDiet.Therapist.DataModel;
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
        [HttpGet]
        public IHttpActionResult GetUsers()
        {
            NdLogger.Debug("Begin");
            return Ok(NdUserManager.Users.ToList().Select(u => Factory.Create(u)));
        }

        [Authorize(Roles = "DevAdmin, Admin")]
        [Route("user/{id:guid}", Name = "GetUserById")]
        [HttpGet]
        public async Task<IHttpActionResult> GetUser(string id)
        {
            NdLogger.Debug("Begin");
            var user = await NdUserManager.FindByIdAsync(id);

            if (user != null)
            {
                return Ok(Factory.Create(user));
            }

            NdLogger.Debug(string.Format("User was not found [id: {0}]", id));
            return NotFound();
        }

        [Authorize(Roles = "DevAdmin, Admin")]
        [Route("user/{email}")]
        [HttpGet]
        public async Task<IHttpActionResult> GetUserByEmail(string email)
        {
            NdLogger.Debug("Begin");
            var user = await NdUserManager.FindByEmailAsync(email);

            if (user != null)
            {
                return Ok(Factory.Create(user));
            }

            NdLogger.Debug(string.Format("User was not found [email: {0}]", email));
            return NotFound();
        }

        [Authorize(Roles = "DevAdmin, Admin")]
        [Route("create/therapist")]
        [HttpPost]
        public async Task<IHttpActionResult> CreateTherapis(CreateTherapistDto createTherapistDto)
        {
            NdLogger.Debug("Begin");
            if (!ModelState.IsValid)
            {
                NdLogger.Error(string.Format(
                    "Model state is not valid [ModelState: {0}]", 
                    string.Join(Environment.NewLine, ModelState.Select(x => string.Format("{0}: {1}", x.Key, x.Value)))));
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
                NdLogger.Error(string.Format(
                    "Create user failed [email: {0}, Reason: {1}]",
                    createTherapistDto.Email,
                    string.Join(Environment.NewLine, addUserResult.Errors)));
                return GetErrorResult(addUserResult);
            }

            IdentityResult addUserToRoleResult = await NdUserManager.AddToRoleAsync(user.Id, "Therapist");
            if (!addUserToRoleResult.Succeeded)
            {
                NdLogger.Error(string.Format(
                    "Add user to roles failed [email: {0}, Reason: {1}]",
                    createTherapistDto.Email,
                    string.Join(Environment.NewLine, addUserResult.Errors)));
                return GetErrorResult(addUserResult);
            }

            try
            {
                Directory.CreateDirectory(HttpContext.Current.Server.MapPath(Path.Combine("~/Results", user.Id)));
            }
            catch(Exception ex)
            {
                NdLogger.Error(string.Format("Error creating folder for therapist [email: {0}]", createTherapistDto.Email), ex);
                return InternalServerError(ex);
            }

            Uri locationHeader = new Uri(Url.Link("GetUserById", new { id = user.Id }));
            return Created(locationHeader, Factory.Create(user));
        }

        [Authorize(Roles = "DevAdmin")]
        [Route("create/admin")]
        [HttpPost]
        public async Task<IHttpActionResult> CreateAdmin(CreateAdminDto createAdminDto)
        {
            NdLogger.Debug("Begin");
            if (!ModelState.IsValid)
            {
                NdLogger.Error(string.Format(
                    "Model state is not valid [ModelState: {0}]",
                    string.Join(Environment.NewLine, ModelState.Select(x => string.Format("{0}: {1}", x.Key, x.Value)))));
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
                UserName = createAdminDto.UserName
            };

            IdentityResult addUserResult = await NdUserManager.CreateAsync(user, createAdminDto.Password);
            if (!addUserResult.Succeeded)
            {
                NdLogger.Error(string.Format(
                    "Create user failed [email: {0}, Reason: {1}]",
                    createAdminDto.Email,
                    string.Join(Environment.NewLine, addUserResult.Errors)));
                return GetErrorResult(addUserResult);
            }

            IdentityResult addUserToRoleResult = await NdUserManager.AddToRoleAsync(user.Id, "Admin");
            if (!addUserToRoleResult.Succeeded)
            {
                NdLogger.Error(string.Format(
                    "Add user to roles failed [email: {0}, Reason: {1}]",
                    createAdminDto.Email,
                    string.Join(Environment.NewLine, addUserResult.Errors)));
                return GetErrorResult(addUserResult);
            }

            Uri locationHeader = new Uri(Url.Link("GetUserById", new { id = user.Id }));
            return Created(locationHeader, Factory.Create(user));
        }

        [Authorize(Roles = "DevAdmin, Admin, Therapist")]
        [Route("changepassword")]
        [HttpPost]
        public async Task<IHttpActionResult> ChangePassword(ChangePasswordDto changePasswordDto)
        {
            NdLogger.Debug("Begin");
            if (!ModelState.IsValid)
            {
                NdLogger.Error(string.Format(
                    "Model state is not valid [ModelState: {0}]",
                    string.Join(Environment.NewLine, ModelState.Select(x => string.Format("{0}: {1}", x.Key, x.Value)))));
                return BadRequest(ModelState);
            }

            IdentityResult result = await NdUserManager.ChangePasswordAsync(User.Identity.GetUserId(), changePasswordDto.OldPassword, changePasswordDto.NewPassword);
            if (!result.Succeeded)
            {
                NdLogger.Error(string.Format(
                    "Change password failed [id: {0}, Reason: {1}]",
                    User.Identity.GetUserId(),
                    string.Join(Environment.NewLine, result.Errors)));
                return GetErrorResult(result);
            }

            return Ok();
        }

        [Authorize(Roles = "DevAdmin")]
        [Route("user/{id:guid}")]
        [HttpDelete]
        public async Task<IHttpActionResult> DeleteUser(string id)
        {
            NdLogger.Debug("Begin");
            var ndUser = await NdUserManager.FindByIdAsync(id);

            if (ndUser != null)
            {
                IdentityResult result = await NdUserManager.DeleteAsync(ndUser);
                if (!result.Succeeded)
                {
                    NdLogger.Error(string.Format(
                        "Delete user failed [id: {0}, Reason: {1}]",
                        id,
                        string.Join(Environment.NewLine, result.Errors)));
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
            NdLogger.Debug("Begin");
            var ndUser = await NdUserManager.FindByIdAsync(id);

            if (ndUser == null)
            {
                NdLogger.Debug(string.Format("User was not found [id: {0}]", id));
                return NotFound();
            }

            var currentRoles = await NdUserManager.GetRolesAsync(ndUser.Id);
            var rolesNotExists = rolesToAssign.Except(NdRoleManager.Roles.Select(x => x.Name)).ToArray();
            if (rolesNotExists.Count() > 0)
            {
                ModelState.AddModelError("", string.Format("Roles '{0}' does not exixts in the system", string.Join(",", rolesNotExists)));
                NdLogger.Error(string.Format(
                    "Model state is not valid [ModelState: {0}]",
                    string.Join(Environment.NewLine, ModelState.Select(x => string.Format("{0}: {1}", x.Key, x.Value)))));
                return BadRequest(ModelState);
            }

            IdentityResult removeResult = await NdUserManager.RemoveFromRolesAsync(ndUser.Id, currentRoles.ToArray());
            if (!removeResult.Succeeded)
            {
                ModelState.AddModelError("", "Failed to remove user roles");
                NdLogger.Error(string.Format(
                    "Model state is not valid [ModelState: {0}]",
                    string.Join(Environment.NewLine, ModelState.Select(x => string.Format("{0}: {1}", x.Key, x.Value)))));
                return BadRequest(ModelState);
            }

            IdentityResult addResult = await NdUserManager.AddToRolesAsync(ndUser.Id, rolesToAssign);
            if (!addResult.Succeeded)
            {
                ModelState.AddModelError("", "Failed to add user roles");
                NdLogger.Error(string.Format(
                    "Model state is not valid [ModelState: {0}]",
                    string.Join(Environment.NewLine, ModelState.Select(x => string.Format("{0}: {1}", x.Key, x.Value)))));
                return BadRequest(ModelState);
            }

            return Ok();
        }
    }
}