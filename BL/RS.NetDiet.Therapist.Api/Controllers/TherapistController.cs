using RS.NetDiet.Therapist.Api.Models;
using RS.NetDiet.Therapist.DataModel;
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
    }
}