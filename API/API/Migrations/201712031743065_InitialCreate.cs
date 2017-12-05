namespace RootSolutions.NetDiet.Therapist.API.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "therapist.NetDietRoles",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Name, unique: true, name: "RoleNameIndex");
            
            CreateTable(
                "therapist.NetDietUserRoles",
                c => new
                    {
                        UserId = c.String(nullable: false, maxLength: 128),
                        RoleId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.UserId, t.RoleId })
                .ForeignKey("therapist.NetDietRoles", t => t.RoleId, cascadeDelete: true)
                .ForeignKey("therapist.NetDietUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.RoleId);
            
            CreateTable(
                "therapist.NetDietUsers",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Institute = c.String(maxLength: 100),
                        WebPage = c.String(),
                        Gender = c.Int(nullable: false),
                        Title = c.Int(nullable: false),
                        MustChangePassword = c.Boolean(nullable: false),
                        FirstName = c.String(nullable: false, maxLength: 100),
                        LastName = c.String(nullable: false, maxLength: 100),
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
                "therapist.NetDietUserClaims",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false, maxLength: 128),
                        ClaimType = c.String(),
                        ClaimValue = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("therapist.NetDietUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "therapist.NetDietUserLogins",
                c => new
                    {
                        LoginProvider = c.String(nullable: false, maxLength: 128),
                        ProviderKey = c.String(nullable: false, maxLength: 128),
                        UserId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.LoginProvider, t.ProviderKey, t.UserId })
                .ForeignKey("therapist.NetDietUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("therapist.NetDietUserRoles", "UserId", "therapist.NetDietUsers");
            DropForeignKey("therapist.NetDietUserLogins", "UserId", "therapist.NetDietUsers");
            DropForeignKey("therapist.NetDietUserClaims", "UserId", "therapist.NetDietUsers");
            DropForeignKey("therapist.NetDietUserRoles", "RoleId", "therapist.NetDietRoles");
            DropIndex("therapist.NetDietUserLogins", new[] { "UserId" });
            DropIndex("therapist.NetDietUserClaims", new[] { "UserId" });
            DropIndex("therapist.NetDietUsers", "UserNameIndex");
            DropIndex("therapist.NetDietUserRoles", new[] { "RoleId" });
            DropIndex("therapist.NetDietUserRoles", new[] { "UserId" });
            DropIndex("therapist.NetDietRoles", "RoleNameIndex");
            DropTable("therapist.NetDietUserLogins");
            DropTable("therapist.NetDietUserClaims");
            DropTable("therapist.NetDietUsers");
            DropTable("therapist.NetDietUserRoles");
            DropTable("therapist.NetDietRoles");
        }
    }
}
