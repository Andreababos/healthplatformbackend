using Microsoft.AspNet.Identity.EntityFramework;
using System.Data.Entity;

namespace RS.NetDiet.Therapist.Api.Infrastructure
{
    public class NdDbContext : IdentityDbContext<NdUser>
    {
        public NdDbContext() : base("NetDiet", throwIfV1Schema: false)
        {
            Configuration.ProxyCreationEnabled = false;
            Configuration.LazyLoadingEnabled = false;
        }

        public static NdDbContext Create()
        {
            return new NdDbContext();
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<NdUser>().ToTable("NetDietUsers", "the");
            modelBuilder.Entity<IdentityRole>().ToTable("NetDietRoles", "the");
            modelBuilder.Entity<IdentityUserRole>().ToTable("NetDietUserRoles", "the");
            modelBuilder.Entity<IdentityUserClaim>().ToTable("NetDietUserClaims", "the");
            modelBuilder.Entity<IdentityUserLogin>().ToTable("NetDietUserLogins", "the");
        }
    }
}