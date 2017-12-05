using Microsoft.AspNet.Identity.EntityFramework;
using RootSolutions.Common.Web.Infrastructure;
using System.Data.Entity;

namespace RootSolutions.NetDiet.Therapist.API.Infrastructure
{
    public class NdDbContext : RsDbContext<NdUser>
    {
        public NdDbContext() : base("NetDiet")
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<NdUser>().ToTable("NetDietUsers", "therapist");
            modelBuilder.Entity<IdentityRole>().ToTable("NetDietRoles", "therapist");
            modelBuilder.Entity<IdentityUserRole>().ToTable("NetDietUserRoles", "therapist");
            modelBuilder.Entity<IdentityUserClaim>().ToTable("NetDietUserClaims", "therapist");
            modelBuilder.Entity<IdentityUserLogin>().ToTable("NetDietUserLogins", "therapist");
        }

        public static NdDbContext Create()
        {
            return new NdDbContext();
        }
    }
}