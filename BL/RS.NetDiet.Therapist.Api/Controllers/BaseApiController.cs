using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using RS.NetDiet.Therapist.Api.Infrastructure;
using RS.NetDiet.Therapist.Api.Models;
using RS.NetDiet.Therapist.Api.Providers;
using RS.NetDiet.Therapist.DataModel;
using System.Net.Http;
using System.Web.Http;

namespace RS.NetDiet.Therapist.Api.Controllers
{
    public class BaseApiController : ApiController
    {
        private Factory factory;
        private NdUserManager _ndUserManager = null;
        private NdRoleManager _ndRoleManager = null;
        private NdDbContext _ndDbContext = null;
        private ILogger logger = null;
        private ResultsRepository resultsRepository = null;
        private PatientsRepository patientsRepository = null;

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

        protected NdDbContext NdDbContext
        {
            get
            {
                return _ndDbContext ?? Request.GetOwinContext().Get<NdDbContext>();
            }
        }

        protected ResultsRepository _resultsRepository
        {
            get
            {
                if (resultsRepository == null)
                {
                    resultsRepository = new ResultsRepository();
                }
                return resultsRepository;
            }
        }

        protected PatientsRepository _patientsRepository
        {
            get
            {
                if (patientsRepository == null)
                {
                    patientsRepository = new PatientsRepository();
                }
                return patientsRepository;
            }
        }

        public BaseApiController()
        {
        }

        protected Factory _factory
        {
            get
            {
                if (factory == null)
                {
                    factory = new Factory(Request, NdUserManager);
                }
                return factory;
            }
        }


        protected ILogger _logger
        {
            get
            {
                if (logger == null)
                {
                    logger = new LogProvider();
                }
                return logger;
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