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

            ndUserManager.UserValidator = new UserValidator<NdUser>(ndUserManager)
            {
                RequireUniqueEmail = true
            };

            ndUserManager.PasswordValidator = new PasswordValidator
            {
                RequiredLength = 8,
                RequireNonLetterOrDigit = true,
                RequireDigit = true,
                RequireLowercase = true,
                RequireUppercase = true,
            };

            return ndUserManager;
        }
    }
}