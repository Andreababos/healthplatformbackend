using Microsoft.AspNet.Identity;
using RootSolutions.Common.Logger;
using RootSolutions.Common.Web.Controllers;
using RootSolutions.Common.Web.Infrastructure;
using RootSolutions.NetDiet.Therapist.API.Dtos;
using RootSolutions.NetDiet.Therapist.API.Infrastructure;
using RootSolutions.NetDiet.Therapist.API.Models;
using RootSolutions.NetDiet.Therapist.API.Providers;
using RootSolutions.NetDiet.Therapist.API.Services;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.ModelBinding;
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

            try
            {
                string code = await _userManager.GenerateEmailConfirmationTokenAsync(user.Id);
                var callbackUrl = new Uri(Url.Link("ConfirmEmailRoute", new { userId = user.Id, code = code }));
                await _userManager.SendEmailAsync(user.Id, "Confirm your account", NdEmailTemplateProvider.CreateConfirmEmailWithPassword(callbackUrl.ToString(), password));
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Error sending ConfirmEmailWithPassword email for therapist with e-mail [{0}]", createTherapistDto.Email), ex);
                return InternalServerError(ex);
            }

            Uri locationHeader = new Uri(Url.Link("GetUserById", new { id = user.Id }));
            return Created(locationHeader, user.CreateCreateUserReturnDto(_userManager, Request));
        }

        [Route("create/therapists"), Authorize(Roles = "DevAdmin, Admin"), HttpPost]
        public async Task<IHttpActionResult> CreateTherapists()
        {
            _logger.Debug("Begin");
            var provider = new MultipartMemoryStreamProvider();
            var success = true;

            try
            {
                await Request.Content.ReadAsMultipartAsync(provider);
                {
                    foreach (var file in provider.Contents)
                    {
                        var extension = Path.GetExtension(file.Headers.ContentDisposition.FileName);
                        if (extension != ".csv")
                        {
                            ModelState.AddModelError("invalid_extension", string.Format("The [{0}] is not a valid file extension", extension));
                        }

                        try
                        {
                            if (!await ProcessCreateTherapistFile(file))
                            {
                                success = false;
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.Error(string.Format("Error processing file [{0}]", file.Headers.ContentDisposition.FileName), ex);
                        }
                    }
                }

                if (!success)
                {
                    return BadRequest("Not all therapist where created, check the logs for more information");
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Error processing create therapists file(s)", ex);
                return InternalServerError(ex);
            }

            return Created<object>(string.Empty, null);
        }

        #region Private Methods -------------------------------------
        private async Task<bool> ProcessCreateTherapistFile(HttpContent file)
        {
            _logger.Debug("Begin");
            var content = await file.ReadAsStringAsync();
            var data = content.Split(new string[1] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Split(','));
            var success = true;

            foreach (var line in data)
            {
                if (line.Length != 8)
                {
                    _logger.Error(string.Format(
                        "The provided data in file [{0}] with content [{1}] is not enough to create a therapist", 
                        file.Headers.ContentDisposition.FileName,
                        string.Join(", ", line)));
                    success = false;
                    continue;
                }

                try
                {
                    CreateTherapistFromFileContent(new CreateTherapistDto()
                    {
                        Title = (Title)Enum.Parse(typeof(Title), line[0]),
                        FirstName = line[1],
                        LastName = line[2],
                        Email = line[3],
                        Gender = (Gender)Enum.Parse(typeof(Gender), line[4]),
                        Institute = line[5],
                        PhoneNumber = line[6],
                        WebPage = line[7]
                    });
                }
                catch (Exception ex)
                {
                    _logger.Error(string.Format(
                        "Error creating therapist from file [{0}] with content [{1}]",
                        file.Headers.ContentDisposition.FileName,
                        string.Join(", ", line)), ex);
                    success = false;
                }
            }

            return success;
        }

        private void CreateTherapistFromFileContent(CreateTherapistDto createTherapistDto)
        {
            _logger.Debug("Begin");

            if (!ModelState.IsValid)
            {
                throw new Exception(CreateLogString(ModelState));
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

            IdentityResult addUserResult = _userManager.Create(user, password);
            if (!addUserResult.Succeeded)
            {
                throw new Exception(string.Join(Environment.NewLine, addUserResult.Errors));
            }

            IdentityResult addUserToRoleResult = _userManager.AddToRole(user.Id, Role.Therapist.ToString());
            if (!addUserToRoleResult.Succeeded)
            {
                throw new Exception(string.Join(Environment.NewLine, addUserToRoleResult.Errors));
            }

            Directory.CreateDirectory(HttpContext.Current.Server.MapPath(Path.Combine("~/Results", user.Id)));

            string code = _userManager.GenerateEmailConfirmationToken(user.Id);
            var callbackUrl = new Uri(Url.Link("ConfirmEmailRoute", new { userId = user.Id, code = code }));
            _userManager.SendEmail(user.Id, "Confirm your account", NdEmailTemplateProvider.CreateConfirmEmailWithPassword(callbackUrl.ToString(), password));
        }
        #endregion --------------------------------------------------
    }
}