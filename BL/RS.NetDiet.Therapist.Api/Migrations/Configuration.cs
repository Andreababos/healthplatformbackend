namespace RS.NetDiet.Therapist.Api.Migrations
{
    using Infrastructure;
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using Models;
    using System;
    using System.Data.Entity.Migrations;

    internal sealed class Configuration : DbMigrationsConfiguration<NdDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = true;
        }

        protected override void Seed(NdDbContext context)
        {
            //  This method will be called after migrating to the latest version.

            var userManager = new UserManager<NdUser>(new UserStore<NdUser>(new NdDbContext()));

            var user = new NdUser()
            {
                UserName = "devadmin",
                Email = "tuzi92@yahoo.com",
                EmailConfirmed = true,
                PhoneNumber = "+40745024467",
                PhoneNumberConfirmed = true,
                FirstName = "Developer",
                LastName = "Admin",
                Gender = Gender.Male
            };

            userManager.Create(user, "4Zist#kbasszonmeg");
        }
    }
}
