using Microsoft.AspNet.Identity;
using RS.NetDiet.Therapist.Api.Infrastructure;
using RS.NetDiet.Therapist.Api.Models;
using RS.NetDiet.Therapist.Api.Services;
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
            _logger.Debug("Begin");
            return Ok(NdUserManager.Users.ToList().Select(u => _factory.Create(u)));
        }

        [Authorize(Roles = "DevAdmin, Admin")]
        [Route("user/{id:guid}", Name = "GetUserById")]
        [HttpGet]
        public async Task<IHttpActionResult> GetUser(string id)
        {
            _logger.Debug(string.Format("Begin. Id: [{0}]", id));
            var user = await NdUserManager.FindByIdAsync(id);

            if (user != null)
            {
                _logger.Debug(string.Format("User found. Id: [{0}]", id));
                return Ok(_factory.Create(user));
            }

            _logger.Debug(string.Format("User was not found. Id: [{0}]", id));
            return NotFound();
        }

        [Authorize(Roles = "DevAdmin, Admin, Therapist")]
        [Route("info")]
        [HttpGet]
        public async Task<IHttpActionResult> GetMyInfo()
        {
            _logger.Debug("Begin");
            var id = User.Identity.GetUserId();
            var user = await NdUserManager.FindByIdAsync(id);

            if (user != null)
            {
                _logger.Debug(string.Format("User found. Id: [{0}]", id));
                return Ok(_factory.CreateUserInfo(user));
            }

            _logger.Debug(string.Format("User was not found. Id: [{0}]", id));
            return NotFound();
        }

        [Authorize(Roles = "DevAdmin, Admin")]
        [Route("create/therapist")]
        [HttpPost]
        public async Task<IHttpActionResult> CreateTherapist(CreateTherapistDto createTherapistDto)
        {
            _logger.Debug(string.Format("Begin. Clinic: [{0}], Email: [{1}], FirstName: [{2}], Gender: [{3}], LastName: [{4}], PhoneNumber: [{5}], Title: [{6}]", 
                createTherapistDto.Clinic,
                createTherapistDto.Email,
                createTherapistDto.FirstName,
                createTherapistDto.Gender.ToString(),
                createTherapistDto.LastName,
                createTherapistDto.PhoneNumber,
                createTherapistDto.Title.ToString()));
            if (!ModelState.IsValid)
            {
                _logger.Error(string.Format(
                    "Model state is not valid. ModelState: [{0}]", 
                    string.Join(Environment.NewLine, ModelState.Select(x => string.Format("{0}: {1}", x.Key, x.Value)))));
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
                Title = createTherapistDto.Title,
                UserName = createTherapistDto.Email
            };

            var password = PasswordGenerator.Generate();
            IdentityResult addUserResult = await NdUserManager.CreateAsync(user, password);
            if (!addUserResult.Succeeded)
            {
                _logger.Error(string.Format(
                    "Create user failed. Email: [{0}], Reason: [{1}]",
                    createTherapistDto.Email,
                    string.Join(Environment.NewLine, addUserResult.Errors)));
                return GetErrorResult(addUserResult);
            }

            IdentityResult addUserToRoleResult = await NdUserManager.AddToRoleAsync(user.Id, "Therapist");
            if (!addUserToRoleResult.Succeeded)
            {
                _logger.Error(string.Format(
                    "Add user to roles failed. Email: [{0}], Reason: [{1}]",
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
                _logger.Error(string.Format("Error creating folder for therapist. Email: [{0}]", createTherapistDto.Email), ex);
                return InternalServerError(ex);
            }

            try
            {
                string code = await NdUserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                var callbackUrl = new Uri(Url.Link("ConfirmEmailRoute", new { userId = user.Id, code = code }));
                await NdUserManager.SendEmailAsync(user.Id, "Confirm your account", NdEmailService.CreateConfirmEmailWithPasswordBody(callbackUrl.ToString(), password));
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Error sending ConfirmEmail email for therapist. Email: [{0}]", createTherapistDto.Email), ex);
                return InternalServerError(ex);
            }

            Uri locationHeader = new Uri(Url.Link("GetUserById", new { id = user.Id }));
            return Created(locationHeader, _factory.Create(user));
        }

        [AllowAnonymous]
        [Route("confirmemail", Name = "ConfirmEmailRoute")]
        [HttpGet]
        public async Task<IHttpActionResult> ConfirmEmail(string userId = "", string code = "")
        {
            _logger.Debug(string.Format("Begin. UserId: [{0}], Code: [{1}]"));
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(code))
            {
                ModelState.AddModelError("", "User Id and Code are required");
                _logger.Error(string.Format(
                    "Model state is not valid. ModelState: [{0}]",
                    string.Join(Environment.NewLine, ModelState.Select(x => string.Format("{0}: {1}", x.Key, x.Value)))));
                return BadRequest(ModelState);
            }

            IdentityResult result = await NdUserManager.ConfirmEmailAsync(userId, code);
            if (result.Succeeded)
            {
                _logger.Debug(string.Format("Email confirmed successfully. Id: [{0}]", userId));
                return Ok();
            }
            else
            {
                _logger.Error(string.Format(
                    "Confirm email failed. Id: [{0}], Code: [{1}], Reason: [{2}]",
                    userId, code,
                    string.Join(Environment.NewLine, result.Errors)));
                return GetErrorResult(result);
            }
        }

        [AllowAnonymous]
        [Route("resetpassword", Name = "ResetPasswordRoute")]
        [HttpPost]
        public async Task<IHttpActionResult> ResetPassword(ResetPasswordDto resetPasswordDto)
        {
            _logger.Debug(string.Format("Begin. Code: [{0}], Id: [{1}]",
                resetPasswordDto.Code,
                resetPasswordDto.Id));
            if (!ModelState.IsValid)
            {
                _logger.Error(string.Format(
                    "Model state is not valid. ModelState: [{0}]",
                    string.Join(Environment.NewLine, ModelState.Select(x => string.Format("{0}: {1}", x.Key, x.Value)))));
                return BadRequest(ModelState);
            }

            IdentityResult result = await NdUserManager.ResetPasswordAsync(resetPasswordDto.Id, resetPasswordDto.Code, resetPasswordDto.Password);
            if (result.Succeeded)
            {
                _logger.Debug(string.Format("Password reseted successfully. Id: [{0}]", resetPasswordDto.Id));

                try
                {
                    await NdUserManager.SendEmailAsync(resetPasswordDto.Id, "Password Reseted", NdEmailService.CreatePasswordResetedBody());
                }
                catch (Exception ex)
                {
                    _logger.Error(string.Format("Error sending PasswordReseted email. Id: [{0}]", resetPasswordDto.Id), ex);
                    return InternalServerError(ex);
                }

                return Ok();
            }
            else
            {
                _logger.Error(string.Format(
                    "Reset password failed. Id: [{0}], Code: [{1}], Reason: [{2}]",
                    resetPasswordDto.Id, resetPasswordDto.Code, 
                    string.Join(Environment.NewLine, result.Errors)));
                return GetErrorResult(result);
            }
        }

        [Authorize(Roles = "DevAdmin")]
        [Route("create/admin")]
        [HttpPost]
        public async Task<IHttpActionResult> CreateAdmin(CreateAdminDto createAdminDto)
        {
            _logger.Debug(string.Format("Begin. Email: [{0}], FirstName: [{1}], Gender: [{2}], LastName: [{3}], PhoneNumber: [{4}], Title: [{5}], UserName: [{6}]", 
                createAdminDto.Email,
                createAdminDto.FirstName,
                createAdminDto.Gender.ToString(),
                createAdminDto.LastName,
                createAdminDto.PhoneNumber,
                createAdminDto.Title.ToString(),
                createAdminDto.UserName));
            if (!ModelState.IsValid)
            {
                _logger.Error(string.Format(
                    "Model state is not valid. ModelState: [{0}]",
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
                _logger.Error(string.Format(
                    "Create admin failed. Email: [{0}], Reason: [{1}]",
                    createAdminDto.Email,
                    string.Join(Environment.NewLine, addUserResult.Errors)));
                return GetErrorResult(addUserResult);
            }

            IdentityResult addUserToRoleResult = await NdUserManager.AddToRoleAsync(user.Id, "Admin");
            if (!addUserToRoleResult.Succeeded)
            {
                _logger.Error(string.Format(
                    "Add admin to roles failed. Email: [{0}], Reason: [{1}]",
                    createAdminDto.Email,
                    string.Join(Environment.NewLine, addUserResult.Errors)));
                return GetErrorResult(addUserResult);
            }

            Uri locationHeader = new Uri(Url.Link("GetUserById", new { id = user.Id }));
            _logger.Debug(string.Format("Admin created successfully. Email: [{0}]", createAdminDto.Email));
            return Created(locationHeader, _factory.Create(user));
        }

        [Authorize(Roles = "DevAdmin, Admin, Therapist")]
        [Route("changepassword")]
        [HttpPost]
        public async Task<IHttpActionResult> ChangePassword(ChangePasswordDto changePasswordDto)
        {
            _logger.Debug("Begin");
            if (!ModelState.IsValid)
            {
                _logger.Error(string.Format(
                    "Model state is not valid. ModelState: [{0}]",
                    string.Join(Environment.NewLine, ModelState.Select(x => string.Format("{0}: {1}", x.Key, x.Value)))));
                return BadRequest(ModelState);
            }

            var id = User.Identity.GetUserId();
            IdentityResult result = await NdUserManager.ChangePasswordAsync(id, changePasswordDto.OldPassword, changePasswordDto.NewPassword);
            if (!result.Succeeded)
            {
                _logger.Error(string.Format(
                    "Change password failed. Id: [{0}], Reason: [{1}]",
                    User.Identity.GetUserId(),
                    string.Join(Environment.NewLine, result.Errors)));
                return GetErrorResult(result);
            }

            _logger.Debug(string.Format("Password changed successfully. Id: [{0}]", id));
            return Ok();
        }

        [AllowAnonymous]
        [Route("requestpasswordreset")]
        [HttpPost]
        public async Task<IHttpActionResult> RequestPasswordReset([FromBody] string email)
        {
            _logger.Debug(string.Format("Begin. Email: [{0}]", email));
            var user = await NdUserManager.FindByEmailAsync(email);
            if (user == null)
            {
                _logger.Debug(string.Format("User was not found. Email: [{0}]", email));
                return NotFound();
            }

            try
            {
                string code = await NdUserManager.GeneratePasswordResetTokenAsync(user.Id);
                var callbackUrl = new Uri(Url.Link("ResetPasswordRoute", new { userId = user.Id, code = code }));
                await NdUserManager.SendEmailAsync(user.Id, "Reset Password", NdEmailService.CreateResetPasswordBody(callbackUrl.ToString()));
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Error sending ResetPassword email for user. Email: [{0}]", email), ex);
                return InternalServerError(ex);
            }

            _logger.Debug(string.Format("Reset password email sent succeessfully. Email: [{0}]", email));
            return Ok();
        }

        [Authorize(Roles = "DevAdmin")]
        [Route("user/{id:guid}")]
        [HttpDelete]
        public async Task<IHttpActionResult> DeleteUser(string id)
        {
            _logger.Debug(string.Format("Begin. Id: [{0}]", id));
            var ndUser = await NdUserManager.FindByIdAsync(id);

            if (ndUser != null)
            {
                IdentityResult result = await NdUserManager.DeleteAsync(ndUser);
                if (!result.Succeeded)
                {
                    _logger.Error(string.Format(
                        "Delete user failed. Id: [{0}], Reason: [{1}]",
                        id,
                        string.Join(Environment.NewLine, result.Errors)));
                    return GetErrorResult(result);
                }

                _logger.Debug(string.Format("User deleted successfully. Id: [{0}]", id));
                return Ok();
            }

            _logger.Debug(string.Format("User was not found [id: {0}]", id));
            return NotFound();
        }

        [Authorize(Roles = "DevAdmin")]
        [Route("user/{id:guid}/roles")]
        [HttpPost]
        public async Task<IHttpActionResult> AssignRolesToUser([FromUri] string id, [FromBody] string[] rolesToAssign)
        {
            _logger.Debug(string.Format("Begin. Id: [{0}], Roles: [{1}]", id, string.Join(", ", rolesToAssign)));
            var ndUser = await NdUserManager.FindByIdAsync(id);

            if (ndUser == null)
            {
                _logger.Debug(string.Format("User was not found. Id: [{0}]", id));
                return NotFound();
            }

            var currentRoles = await NdUserManager.GetRolesAsync(ndUser.Id);
            var rolesNotExists = rolesToAssign.Except(NdRoleManager.Roles.Select(x => x.Name)).ToArray();
            if (rolesNotExists.Count() > 0)
            {
                ModelState.AddModelError("", string.Format("Roles '{0}' does not exixts in the system", string.Join(",", rolesNotExists)));
                _logger.Error(string.Format(
                    "Model state is not valid. ModelState: [{0}]",
                    string.Join(Environment.NewLine, ModelState.Select(x => string.Format("{0}: {1}", x.Key, x.Value)))));
                return BadRequest(ModelState);
            }

            IdentityResult removeResult = await NdUserManager.RemoveFromRolesAsync(ndUser.Id, currentRoles.ToArray());
            if (!removeResult.Succeeded)
            {
                ModelState.AddModelError("", "Failed to remove user roles");
                _logger.Error(string.Format(
                    "Model state is not valid. ModelState: [{0}]",
                    string.Join(Environment.NewLine, ModelState.Select(x => string.Format("{0}: {1}", x.Key, x.Value)))));
                return BadRequest(ModelState);
            }

            IdentityResult addResult = await NdUserManager.AddToRolesAsync(ndUser.Id, rolesToAssign);
            if (!addResult.Succeeded)
            {
                ModelState.AddModelError("", "Failed to add user roles");
                _logger.Error(string.Format(
                    "Model state is not valid [ModelState: {0}]",
                    string.Join(Environment.NewLine, ModelState.Select(x => string.Format("{0}: {1}", x.Key, x.Value)))));
                return BadRequest(ModelState);
            }

            _logger.Debug(string.Format("User assigned to roles successfully. Id: [{0}], Roles: [{1}]", id, string.Join(", ", rolesToAssign)));
            return Ok();
        }

        [Authorize(Roles = "DevAdmin, Admin, Therapist")]
        [Route("update")]
        [HttpPost]
        public async Task<IHttpActionResult> UpdateMyInfo(UserInfoDto myInfoDto)
        {
            _logger.Debug(string.Format("Begin. Clinic: [{0}], Email: [{1}], FirstName: [{2}], Gender: [{3}], LastName: [{4}], PhoneNumber: [{5}], Title: [{6}]",
                myInfoDto.Clinic,
                myInfoDto.Email,
                myInfoDto.FirstName,
                myInfoDto.Gender.HasValue ? myInfoDto.Gender.Value.ToString() : string.Empty,
                myInfoDto.LastName,
                myInfoDto.PhoneNumber,
                myInfoDto.Title.HasValue ? myInfoDto.Title.Value.ToString() : string.Empty));
            var id = User.Identity.GetUserId();
            var user = await NdUserManager.FindByIdAsync(id);
            var emailChanged = false;

            try
            {
                if (!string.IsNullOrWhiteSpace(myInfoDto.Clinic) && user.Clinic != myInfoDto.Clinic)
                    user.Clinic = myInfoDto.Clinic;
                if (!string.IsNullOrWhiteSpace(myInfoDto.Email) && user.Email != myInfoDto.Email)
                {
                    emailChanged = true;
                    user.EmailConfirmed = false;
                    user.Email = myInfoDto.Email;
                }
                if (!string.IsNullOrWhiteSpace(myInfoDto.FirstName) && user.FirstName != myInfoDto.FirstName)
                    user.FirstName = myInfoDto.FirstName;
                if (myInfoDto.Gender.HasValue && user.Gender != myInfoDto.Gender.Value)
                    user.Gender = myInfoDto.Gender.Value;
                if (!string.IsNullOrWhiteSpace(myInfoDto.LastName) && user.LastName != myInfoDto.LastName)
                    user.LastName = myInfoDto.LastName;
                if (!string.IsNullOrWhiteSpace(myInfoDto.PhoneNumber) && user.PhoneNumber != myInfoDto.PhoneNumber)
                    user.PhoneNumber = myInfoDto.PhoneNumber;
                if (myInfoDto.Title.HasValue && user.Title != myInfoDto.Title.Value)
                    user.Title = myInfoDto.Title.Value;
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Update my info failed. Id: [{0}]", id), ex);
                return InternalServerError(ex);
            }

            var result = await NdUserManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                _logger.Error(string.Format(
                    "Update my info failed. Email: [{0}], Reason: [{1}]",
                    user.Email,
                    string.Join(Environment.NewLine, result.Errors)));
                return GetErrorResult(result);
            }
            await NdDbContext.SaveChangesAsync();

            try
            {
                await NdUserManager.SendEmailAsync(id, "Account Information Chaged", NdEmailService.CreateAccountInformationChangedBody(_factory.CreateUserInfo(user)));
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Error sending AccountInformationChanged email. Email: [{0}]", user.Email), ex);
                return InternalServerError(ex);
            }

            if (emailChanged)
            {
                try
                {
                    string code = await NdUserManager.GenerateEmailConfirmationTokenAsync(id);
                    var callbackUrl = new Uri(Url.Link("ConfirmEmailRoute", new { userId = id, code = code }));
                    await NdUserManager.SendEmailAsync(id, "Confirm your account", NdEmailService.CreateConfirmEmailBody(callbackUrl.ToString()));
                }
                catch (Exception ex)
                {
                    _logger.Error(string.Format("Error sending ConfirmEmail email. Email: {0}]", user.Email), ex);
                    return InternalServerError(ex);
                }
            }

            _logger.Debug(string.Format("My info updated. Id: [{0}]", id));
            return Ok();
        }
    }
}