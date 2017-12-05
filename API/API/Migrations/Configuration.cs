namespace RootSolutions.NetDiet.Therapist.API.Migrations
{
    using Infrastructure;
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using Models;
    using System;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<NdDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(NdDbContext context)
        {
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(new NdDbContext()));
            foreach (var role in Enum.GetNames(typeof(Role)))
            {
                if (!roleManager.Roles.Any(x => x.Name == role)) { roleManager.Create(new IdentityRole { Name = role }); }
            }

            var userManager = new UserManager<NdUser>(new UserStore<NdUser>(new NdDbContext()));

            var devAdmin = new NdUser()
            {
                Email = "devadmin@mintest.dk",
                EmailConfirmed = true,
                FirstName = "Tamás",
                Gender = Gender.Male,
                LastName  = "Tüzes-Kátai",
                MustChangePassword = false, 
                Title = Title.Mr,
                UserName = "devadmin"
            };
            userManager.Create(devAdmin, "j9up1uuU!");
            userManager.AddToRoles(devAdmin.Id, Role.DevAdmin.ToString());
        }
    }
}
