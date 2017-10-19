using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using RS.NetDiet.Therapist.Api.Models;
using System;
using System.Threading.Tasks;
using System.Web.Http;

namespace RS.NetDiet.Therapist.Api.Controllers
{
    [Authorize(Roles = "DevAdmin")]
    [RoutePrefix("api/roles")]
    public class RolesController : BaseApiController
    {
        [Route("{id:guid}", Name = "GetRoleById")]
        public async Task<IHttpActionResult> GetRole(string Id)
        {
            var role = await NdRoleManager.FindByIdAsync(Id);

            if (role != null)
            {
                return Ok(Factory.Create(role));
            }

            return NotFound();
        }

        [Route("", Name = "GetAllRoles")]
        public IHttpActionResult GetAllRoles()
        {
            var roles = NdRoleManager.Roles;

            return Ok(roles);
        }

        [Route("create")]
        public async Task<IHttpActionResult> Create(CreateRoleDto createRoleDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var role = new IdentityRole { Name = createRoleDto.Name };
            var result = await NdRoleManager.CreateAsync(role);

            if (!result.Succeeded)
            {
                return GetErrorResult(result);
            }

            Uri locationHeader = new Uri(Url.Link("GetRoleById", new { id = role.Id }));

            return Created(locationHeader, Factory.Create(role));
        }

        [Route("{id:guid}")]
        public async Task<IHttpActionResult> DeleteRole(string Id)
        {

            var role = await NdRoleManager.FindByIdAsync(Id);

            if (role != null)
            {
                IdentityResult result = await NdRoleManager.DeleteAsync(role);

                if (!result.Succeeded)
                {
                    return GetErrorResult(result);
                }

                return Ok();
            }

            return NotFound();
        }

        [Route("ManageUsersInRole")]
        public async Task<IHttpActionResult> ManageUsersInRole(UsersInRoleDto usersInRoleDto)
        {
            var role = await NdRoleManager.FindByIdAsync(usersInRoleDto.Id);

            if (role == null)
            {
                ModelState.AddModelError("", "Role does not exist");
                return BadRequest(ModelState);
            }

            foreach (string user in usersInRoleDto.EnrolledUsers)
            {
                var ndUser = await NdUserManager.FindByIdAsync(user);

                if (ndUser == null)
                {
                    ModelState.AddModelError("", string.Format("User: {0} does not exists", user));
                    continue;
                }

                if (!NdUserManager.IsInRole(user, role.Name))
                {
                    IdentityResult result = await NdUserManager.AddToRoleAsync(user, role.Name);

                    if (!result.Succeeded)
                    {
                        ModelState.AddModelError("", string.Format("User: {0} could not be added to role", user));
                    }
                }
            }

            foreach (string user in usersInRoleDto.RemovedUsers)
            {
                var ndUser = await NdUserManager.FindByIdAsync(user);
                if (ndUser == null)
                {
                    ModelState.AddModelError("", string.Format("User: {0} does not exists", user));
                    continue;
                }

                IdentityResult result = await NdUserManager.RemoveFromRoleAsync(user, role.Name);
                if (!result.Succeeded)
                {
                    ModelState.AddModelError("", string.Format("User: {0} could not be removed from role", user));
                }
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            return Ok();
        }
    }
}