using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using RS.NetDiet.Therapist.Api.Infrastructure;
using RS.NetDiet.Therapist.Api.Models;
using System.Net.Http;
using System.Web.Http;

namespace RS.NetDiet.Therapist.Api.Controllers
{
    public class BaseApiController : ApiController
    {
        private Factory _factory;
        private NdUserManager _ndUserManager = null;
        private NdRoleManager _ndRoleManager = null;

        protected NdUserManager NdUserManager
        {
            get
            {
                return _ndUserManager ?? Request.GetOwinContext().GetUserManager<NdUserManager>();
            }
        }

        protected NdRoleManager NdRoleManager
        {
            get
            {
                return _ndRoleManager ?? Request.GetOwinContext().GetUserManager<NdRoleManager>();
            }
        }

        public BaseApiController()
        {
        }

        protected Factory Factory
        {
            get
            {
                if (_factory == null)
                {
                    _factory = new Factory(Request, NdUserManager);
                }
                return _factory;
            }
        }

        protected IHttpActionResult GetErrorResult(IdentityResult result)
        {
            if (result == null)
            {
                return InternalServerError();
            }

            if (!result.Succeeded)
            {
                if (result.Errors != null)
                {
                    foreach (string error in result.Errors)
                    {
                        ModelState.AddModelError("", error);
                    }
                }

                if (ModelState.IsValid)
                {
                    return BadRequest();
                }

                return BadRequest(ModelState);
            }

            return null;
        }
    }
}