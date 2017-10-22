namespace RS.NetDiet.Therapist.Api.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "the.NetDietRoles",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Name, unique: true, name: "RoleNameIndex");
            
            CreateTable(
                "the.NetDietUserRoles",
                c => new
                    {
                        UserId = c.String(nullable: false, maxLength: 128),
                        RoleId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.UserId, t.RoleId })
                .ForeignKey("the.NetDietRoles", t => t.RoleId, cascadeDelete: true)
                .ForeignKey("the.NetDietUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.RoleId);
            
            CreateTable(
                "the.NetDietUsers",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        FirstName = c.String(nullable: false, maxLength: 100),
                        LastName = c.String(nullable: false, maxLength: 100),
                        Clinic = c.String(maxLength: 100),
                        Gender = c.Int(nullable: false),
                        Title = c.Int(nullable: false),
                        Email = c.String(maxLength: 256),
                        EmailConfirmed = c.Boolean(nullable: false),
                        PasswordHash = c.String(),
                        SecurityStamp = c.String(),
                        PhoneNumber = c.String(),
                        PhoneNumberConfirmed = c.Boolean(nullable: false),
                        TwoFactorEnabled = c.Boolean(nullable: false),
                        LockoutEndDateUtc = c.DateTime(),
                        LockoutEnabled = c.Boolean(nullable: false),
                        AccessFailedCount = c.Int(nullable: false),
                        UserName = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.UserName, unique: true, name: "UserNameIndex");
            
            CreateTable(
                "the.NetDietUserClaims",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false, maxLength: 128),
                        ClaimType = c.String(),
                        ClaimValue = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("the.NetDietUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "the.NetDietUserLogins",
                c => new
                    {
                        LoginProvider = c.String(nullable: false, maxLength: 128),
                        ProviderKey = c.String(nullable: false, maxLength: 128),
                        UserId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.LoginProvider, t.ProviderKey, t.UserId })
                .ForeignKey("the.NetDietUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("the.NetDietUserRoles", "UserId", "the.NetDietUsers");
            DropForeignKey("the.NetDietUserLogins", "UserId", "the.NetDietUsers");
            DropForeignKey("the.NetDietUserClaims", "UserId", "the.NetDietUsers");
            DropForeignKey("the.NetDietUserRoles", "RoleId", "the.NetDietRoles");
            DropIndex("the.NetDietUserLogins", new[] { "UserId" });
            DropIndex("the.NetDietUserClaims", new[] { "UserId" });
            DropIndex("the.NetDietUsers", "UserNameIndex");
            DropIndex("the.NetDietUserRoles", new[] { "RoleId" });
            DropIndex("the.NetDietUserRoles", new[] { "UserId" });
            DropIndex("the.NetDietRoles", "RoleNameIndex");
            DropTable("the.NetDietUserLogins");
            DropTable("the.NetDietUserClaims");
            DropTable("the.NetDietUsers");
            DropTable("the.NetDietUserRoles");
            DropTable("the.NetDietRoles");
        }
    }
}
