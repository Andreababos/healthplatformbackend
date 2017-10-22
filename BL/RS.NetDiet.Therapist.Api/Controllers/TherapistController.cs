using RS.NetDiet.Therapist.Api.Models;
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
        public async Task<IHttpActionResult> GetTherapists()
        {
            var therapistRole = await NdRoleManager.FindByNameAsync(Role.Therapist.ToString());

            return Ok(NdUserManager.Users.Where(x => x.Roles.Any(y => y.RoleId == therapistRole.Id)).ToList().Select(x => Factory.CreateTherapist(x)));
        }
    }
}