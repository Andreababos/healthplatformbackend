using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using RS.NetDiet.Therapist.Api.Services;
using System;

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

            ndUserManager.EmailService = new NdEmailService();
            var dataProtectionProvider = options.DataProtectionProvider;
            if (dataProtectionProvider != null)
            {
                ndUserManager.UserTokenProvider = new DataProtectorTokenProvider<NdUser>(dataProtectionProvider.Create("ASP.NET Identity"))
                {
                    TokenLifespan = TimeSpan.FromHours(6)
                };
            }

            return ndUserManager;
        }
    }
}