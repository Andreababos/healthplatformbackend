using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;

namespace RS.NetDiet.Therapist.Api.Infrastructure
{
    public class NdRoleManager : RoleManager<IdentityRole>
    {
        public NdRoleManager(IRoleStore<IdentityRole, string> roleStore)
                : base(roleStore)
        {
        }

        public static NdRoleManager Create(IdentityFactoryOptions<NdRoleManager> options, IOwinContext context)
        {
            var appRoleManager = new NdRoleManager(new RoleStore<IdentityRole>(context.Get<NdDbContext>()));

            return appRoleManager;
        }
    }
}