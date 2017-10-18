using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;

namespace RS.NetDiet.Therapist.Api.Infrastructure
{
    public class NdUserManager : UserManager<NdUser>
    {
        public NdUserManager(IUserStore<NdUser> store)
            : base(store)
        {
        }

        public static NdUserManager Create(IdentityFactoryOptions<NdUserManager> options, IOwinContext context)
        {
            var ndDbContext = context.Get<NdDbContext>();
            var ndUserManager = new NdUserManager(new UserStore<NdUser>(ndDbContext));

            return ndUserManager;
        }
    }
}