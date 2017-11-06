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
            NdLogger.Debug("Begin");
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
            NdLogger.Debug(string.Format("Begin. Id: [{0}]", id));
            var user = await NdUserManager.FindByIdAsync(id);
            var therapistRole = await NdRoleManager.FindByNameAsync(Role.Therapist.ToString());

            if (user != null && user.Roles.Any(x => x.RoleId == therapistRole.Id))
            {
                NdLogger.Debug(string.Format("User found. Id: [{0}]", id));
                return Ok(Factory.CreateUserInfo(user));
            }

            NdLogger.Debug(string.Format("Therapist was not found [id: {0}]", id));
            return NotFound();
        }

        [Authorize(Roles = "DevAdmin, Admin")]
        [Route("email")]
        [HttpPost]
        public async Task<IHttpActionResult> GetTherapistsByEmail([FromBody] string email)
        {
            NdLogger.Debug(string.Format("Begin. Email: [{0}]", email));
            var therapistRole = await NdRoleManager.FindByNameAsync(Role.Therapist.ToString());

            return Ok(NdUserManager.Users
                .Where(x => x.Roles.Any(y => y.RoleId == therapistRole.Id) && x.Email.Contains(email))
                .OrderBy(x => x.FirstName).ThenBy(x => x.LastName)
                .ToList().Select(x => Factory.CreateTherapist(x)));
        }

        [Authorize(Roles = "DevAdmin, Admin")]
        [Route("name/{name}")]
        [HttpPost]
        public async Task<IHttpActionResult> GetTherapistsByName([FromBody] string name)
        {
            NdLogger.Debug(string.Format("Begin. Name: [{0}]", name));
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
            NdLogger.Debug(string.Format("Begin. Clinic: [{0}], Email: [{1}], FirstName: [{2}], Gender: [{3}], Id: [{4}], LastName: [{5}], PhoneNumber: [{6}], Title: [{7}]",
                therapistInfoDto.Clinic,
                therapistInfoDto.Email,
                therapistInfoDto.FirstName,
                therapistInfoDto.Gender.HasValue ? therapistInfoDto.Gender.Value.ToString() : string.Empty,
                therapistInfoDto.Id,
                therapistInfoDto.LastName,
                therapistInfoDto.PhoneNumber,
                therapistInfoDto.Title.HasValue ? therapistInfoDto.Title.Value.ToString() : string.Empty));
            var user = await NdUserManager.FindByIdAsync(therapistInfoDto.Id);
            var emailBefore = user.Email;

            try
            {
                if (!string.IsNullOrWhiteSpace(therapistInfoDto.Clinic) && user.Clinic != therapistInfoDto.Clinic)
                    user.Clinic = therapistInfoDto.Clinic;
                if (!string.IsNullOrWhiteSpace(therapistInfoDto.Email) && user.Email != therapistInfoDto.Email)
                {
                    user.EmailConfirmed = false;
                    user.Email = therapistInfoDto.Email;
                }
                if (!string.IsNullOrWhiteSpace(therapistInfoDto.FirstName) && user.FirstName != therapistInfoDto.FirstName)
                    user.FirstName = therapistInfoDto.FirstName;
                if (therapistInfoDto.Gender.HasValue && user.Gender != therapistInfoDto.Gender.Value)
                    user.Gender = therapistInfoDto.Gender.Value;
                if (!string.IsNullOrWhiteSpace(therapistInfoDto.LastName) && user.LastName != therapistInfoDto.LastName)
                    user.LastName = therapistInfoDto.LastName;
                if (!string.IsNullOrWhiteSpace(therapistInfoDto.PhoneNumber) && user.PhoneNumber != therapistInfoDto.PhoneNumber)
                    user.PhoneNumber = therapistInfoDto.PhoneNumber;
                if (therapistInfoDto.Title.HasValue && user.Title != therapistInfoDto.Title.Value)
                    user.Title = therapistInfoDto.Title.Value;
            }
            catch (Exception ex)
            {
                NdLogger.Error(string.Format("Update user info failed. Id: [{0}]", therapistInfoDto.Id), ex);
                return InternalServerError(ex);
            }

            var result = await NdUserManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                NdLogger.Error(string.Format(
                    "Update user info failed. Email: [{0}], Reason: [{1}]",
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
                NdLogger.Error(string.Format("Error sending AccountInformationChanged email for therapist. Email: [{0}]", user.Email), ex);
                return InternalServerError(ex);
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
                    NdLogger.Error(string.Format("Error sending ConfirmEmail email for therapist. Email: [{0}]", user.Email), ex);
                    return InternalServerError(ex);
                }
            }

            NdLogger.Debug(string.Format("My info updated. Id: [{0}]", user.Id));
            return Ok();
        }
    }
}