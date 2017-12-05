using Microsoft.AspNet.Identity;
using RootSolutions.Common.Logger;
using RootSolutions.Common.Services;
using RootSolutions.Common.Web.Controllers;
using RootSolutions.Common.Web.Infrastructure;
using RootSolutions.NetDiet.Therapist.API.Dtos;
using RootSolutions.NetDiet.Therapist.API.Infrastructure;
using RootSolutions.NetDiet.Therapist.API.Models;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Security;

namespace RootSolutions.NetDiet.Therapist.API.Controllers
{
    [RoutePrefix("api/accounts")]
    public class AccountsController : BaseApiControllerWithUserManagement<
        NdUser,
        RsUserManagerWithMessageService<NdUser, NdDbContext, NdEmailService>,
        RsRoleManager<NdUser, NdDbContext>,
        NdDbContext, DefaultLogger>
    {
        [Route("user/{id:guid}", Name = "GetUserById"), Authorize(Roles = "DevAdmin, Admin"), HttpGet]
        public async Task<IHttpActionResult> GetUser(string id)
        {
            _logger.Debug("Begin");
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                _logger.Error(string.Format("User with id [{0}] was not found", id));
                return NotFound();
            }

            return Ok(user.CreateUserInfoDto());
        }

        [Route("info"), Authorize(Roles = "DevAdmin, Admin, Therapist"), HttpGet]
        public async Task<IHttpActionResult> GetMyInformation()
        {
            _logger.Debug("Begin");
            var id = User.Identity.GetUserId();

            return await GetUser(id);
        }

        [Route("create/therapist"), Authorize(Roles = "DevAdmin, Admin"), HttpPost]
        public async Task<IHttpActionResult> CreateTherapist(CreateTherapistDto createTherapistDto)
        {
            _logger.Debug("Begin");

            if (!ModelState.IsValid)
            {
                _logger.Error(CreateLogString(ModelState));
                return BadRequest(ModelState);
            }

            var user = new NdUser()
            {
                Email = createTherapistDto.Email,
                FirstName = createTherapistDto.FirstName,
                Gender = createTherapistDto.Gender,
                Institute = createTherapistDto.Institute,
                LastName = createTherapistDto.LastName,
                PhoneNumber = createTherapistDto.PhoneNumber,
                Title = createTherapistDto.Title,
                WebPage = createTherapistDto.WebPage
            };
            var password = Membership.GeneratePassword(8, 1);

            IdentityResult addUserResult = await _userManager.CreateAsync(user, password);
            if (!addUserResult.Succeeded)
            {
                _logger.Error(string.Format("Create therapist with e-mail [{0}] failed", createTherapistDto.Email),
                    new Exception(string.Join(Environment.NewLine, addUserResult.Errors)));
                return GetErrorResult(addUserResult);
            }

            IdentityResult addUserToRoleResult = await _userManager.AddToRoleAsync(user.Id, Role.Therapist.ToString());
            if (!addUserToRoleResult.Succeeded)
            {
                _logger.Error(string.Format("Add therapist with e-mail [{0}] to role [Therapist] failed", createTherapistDto.Email),
                    new Exception(string.Join(Environment.NewLine, addUserResult.Errors)));
                return GetErrorResult(addUserResult);
            }

            try
            {
                Directory.CreateDirectory(HttpContext.Current.Server.MapPath(Path.Combine("~/Results", user.Id)));
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Create folder for therapist with e-mail [{0}] failed", createTherapistDto.Email), ex);
                return InternalServerError(ex);
            }

            //try
            //{
            //    string code = await _userManager.GenerateEmailConfirmationTokenAsync(user.Id);
            //    var callbackUrl = new Uri(Url.Link("ConfirmEmailRoute", new { userId = user.Id, code = code }));
            //    await _userManager.SendEmailAsync(user.Id, "Confirm your account", NdEmailService.CreateConfirmEmailWithPasswordBody(callbackUrl.ToString(), password));
            //}
            //catch (Exception ex)
            //{
            //    _logger.Error(string.Format("Error sending ConfirmEmail email for therapist. Email: [{0}]", createTherapistDto.Email), ex);
            //    return InternalServerError(ex);
            //}

            Uri locationHeader = new Uri(Url.Link("GetUserById", new { id = user.Id }));
            return Created(locationHeader, user.CreateCreateUserReturnDto(_userManager, Request));
        }
    }
}