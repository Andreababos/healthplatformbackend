using RS.NetDiet.Therapist.Api.Models;
using RS.NetDiet.Therapist.Api.Services;
using RS.NetDiet.Therapist.DataModel;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;

namespace RS.NetDiet.Therapist.Api.Controllers
{
    [RoutePrefix("api/therapists")]
    public class TherapistController : BaseApiController
    {
        [Authorize(Roles = "DevAdmin, Admin")]
        [Route("all")]
        [HttpGet]
        public async Task<IHttpActionResult> GetTherapists()
        {
            var therapistRole = await NdRoleManager.FindByNameAsync(Role.Therapist.ToString());

            return Ok(NdUserManager.Users
                .Where(x => x.Roles.Any(y => y.RoleId == therapistRole.Id))
                .OrderBy(x => x.FirstName).ThenBy(x => x.LastName)
                .ToList().Select(x => Factory.CreateTherapist(x)));
        }

        [Authorize(Roles = "DevAdmin, Admin")]
        [Route("{id:guid}")]
        [HttpGet]
        public async Task<IHttpActionResult> GetTherapistInfo(string id)
        {
            NdLogger.Debug("Begin");
            var user = await NdUserManager.FindByIdAsync(id);
            var therapistRole = await NdRoleManager.FindByNameAsync(Role.Therapist.ToString());

            if (user != null && user.Roles.Any(x => x.RoleId == therapistRole.Id))
            {
                return Ok(Factory.CreateUserInfo(user));
            }

            NdLogger.Debug(string.Format("Therapist was not found [id: {0}]", id));
            return NotFound();
        }

        [Authorize(Roles = "DevAdmin, Admin")]
        [Route("email/{email}")]
        [HttpGet]
        public async Task<IHttpActionResult> GetTherapistsByEmail(string email)
        {
            var therapistRole = await NdRoleManager.FindByNameAsync(Role.Therapist.ToString());

            return Ok(NdUserManager.Users
                .Where(x => x.Roles.Any(y => y.RoleId == therapistRole.Id) && x.Email.Contains(email))
                .OrderBy(x => x.FirstName).ThenBy(x => x.LastName)
                .ToList().Select(x => Factory.CreateTherapist(x)));
        }

        [Authorize(Roles = "DevAdmin, Admin")]
        [Route("name/{name}")]
        [HttpGet]
        public async Task<IHttpActionResult> GetTherapistsByName(string name)
        {
            var therapistRole = await NdRoleManager.FindByNameAsync(Role.Therapist.ToString());
            var words = name.Split(' ');

            return Ok(NdUserManager.Users
                .Where(x => x.Roles.Any(y => y.RoleId == therapistRole.Id) &&
                (words.Any(y => x.FirstName.Contains(y)) || words.Any(y => x.LastName.Contains(y))))
                .OrderBy(x => x.FirstName).ThenBy(x => x.LastName)
                .ToList().Select(x => Factory.CreateTherapist(x)));
        }

        [Authorize(Roles = "DevAdmin, Admin")]
        [Route("update/{id:guid}")]
        [HttpPost]
        public async Task<IHttpActionResult> UpdateTherapistInfo(UserInfoDto therapistInfoDto)
        {
            NdLogger.Debug("Begin");
            var user = await NdUserManager.FindByIdAsync(therapistInfoDto.Id);
            var emailBefore = user.Email;

            if (!string.IsNullOrWhiteSpace(therapistInfoDto.Clinic) && !user.Clinic.Equals(therapistInfoDto.Clinic))
                user.Clinic = therapistInfoDto.Clinic;
            if (!string.IsNullOrWhiteSpace(therapistInfoDto.Email) && !user.Email.Equals(therapistInfoDto.Email))
            {
                user.EmailConfirmed = false;
                user.Email = therapistInfoDto.Email;
            }
            if (!string.IsNullOrWhiteSpace(therapistInfoDto.FirstName) && !user.FirstName.Equals(therapistInfoDto.FirstName))
                user.FirstName = therapistInfoDto.FirstName;
            if (therapistInfoDto.Gender.HasValue && !user.Gender.Equals(therapistInfoDto.Gender.Value))
                user.Gender = therapistInfoDto.Gender.Value;
            if (!string.IsNullOrWhiteSpace(therapistInfoDto.LastName) && !user.LastName.Equals(therapistInfoDto.LastName))
                user.LastName = therapistInfoDto.LastName;
            if (!string.IsNullOrWhiteSpace(therapistInfoDto.PhoneNumber) && !user.PhoneNumber.Equals(therapistInfoDto.PhoneNumber))
                user.PhoneNumber = therapistInfoDto.PhoneNumber;
            if (therapistInfoDto.Title.HasValue && !user.Title.Equals(therapistInfoDto.Title.Value))
                user.Title = therapistInfoDto.Title.Value;

            var result = await NdUserManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                NdLogger.Error(string.Format(
                    "Update user info failed [email: {0}, Reason: {1}]",
                    user.Email,
                    string.Join(Environment.NewLine, result.Errors)));
                return GetErrorResult(result);
            }
            await NdDbContext.SaveChangesAsync();

            try
            {
                await NdUserManager.SendEmailAsync(user.Id, "Account Information Chaged", NdEmailService.CreateAccountInformationChangedBody(Factory.CreateUserInfo(user)));
            }
            catch (Exception ex)
            {
                NdLogger.Error(string.Format("Error sending AccountInformationChanged email for therapist [email: {0}]", user.Email), ex);
            }

            if (!emailBefore.Equals(user.Email))
            {
                try
                {
                    string code = await NdUserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                    var callbackUrl = new Uri(Url.Link("ConfirmEmailRoute", new { userId = user.Id, code = code }));
                    await NdUserManager.SendEmailAsync(user.Id, "Confirm your account", NdEmailService.CreateConfirmEmailBody(callbackUrl.ToString()));
                }
                catch (Exception ex)
                {
                    NdLogger.Error(string.Format("Error sending ConfirmEmail email for therapist [email: {0}]", user.Email), ex);
                }
            }

            return Ok();
        }
    }
}