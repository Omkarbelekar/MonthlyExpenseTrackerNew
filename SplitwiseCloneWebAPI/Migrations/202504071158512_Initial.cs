namespace SplitwiseCloneWebAPI.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Expenses",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Description = c.String(nullable: false),
                        Amount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Date = c.DateTime(nullable: false),
                        GroupId = c.Int(nullable: false),
                        PaidByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.PaidByUserId, cascadeDelete: true)
                .ForeignKey("dbo.Groups", t => t.GroupId, cascadeDelete: true)
                .Index(t => t.GroupId)
                .Index(t => t.PaidByUserId);
            
            CreateTable(
                "dbo.ExpenseShares",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ExpenseId = c.Int(nullable: false),
                        OwedByUserId = c.Int(nullable: false),
                        ShareAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Expenses", t => t.ExpenseId, cascadeDelete: false)
.ForeignKey("dbo.Users", t => t.OwedByUserId, cascadeDelete: false)

                .Index(t => t.ExpenseId)
                .Index(t => t.OwedByUserId);
            
            CreateTable(
                "dbo.Users",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        Email = c.String(nullable: false),
                        PasswordHash = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.GroupMembers",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        GroupId = c.Int(nullable: false),
                        UserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Groups", t => t.GroupId, cascadeDelete: true)
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .Index(t => t.GroupId)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.Groups",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        CreatedByUserId = c.Int(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                        CreatedBy_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.CreatedBy_Id)
                .Index(t => t.CreatedBy_Id);
            
            CreateTable(
                "dbo.Payments",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        GroupId = c.Int(nullable: false),
                        FromUserId = c.Int(nullable: false),
                        ToUserId = c.Int(nullable: false),
                        Amount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Date = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.FromUserId, cascadeDelete: false)
                .ForeignKey("dbo.Groups", t => t.GroupId, cascadeDelete: true)
                .ForeignKey("dbo.Users", t => t.ToUserId, cascadeDelete: false)
                .Index(t => t.GroupId)
                .Index(t => t.FromUserId)
                .Index(t => t.ToUserId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Payments", "ToUserId", "dbo.Users");
            DropForeignKey("dbo.Payments", "GroupId", "dbo.Groups");
            DropForeignKey("dbo.Payments", "FromUserId", "dbo.Users");
            DropForeignKey("dbo.GroupMembers", "UserId", "dbo.Users");
            DropForeignKey("dbo.GroupMembers", "GroupId", "dbo.Groups");
            DropForeignKey("dbo.Expenses", "GroupId", "dbo.Groups");
            DropForeignKey("dbo.Groups", "CreatedBy_Id", "dbo.Users");
            DropForeignKey("dbo.Expenses", "PaidByUserId", "dbo.Users");
            DropForeignKey("dbo.ExpenseShares", "OwedByUserId", "dbo.Users");
            DropForeignKey("dbo.ExpenseShares", "ExpenseId", "dbo.Expenses");
            DropIndex("dbo.Payments", new[] { "ToUserId" });
            DropIndex("dbo.Payments", new[] { "FromUserId" });
            DropIndex("dbo.Payments", new[] { "GroupId" });
            DropIndex("dbo.Groups", new[] { "CreatedBy_Id" });
            DropIndex("dbo.GroupMembers", new[] { "UserId" });
            DropIndex("dbo.GroupMembers", new[] { "GroupId" });
            DropIndex("dbo.ExpenseShares", new[] { "OwedByUserId" });
            DropIndex("dbo.ExpenseShares", new[] { "ExpenseId" });
            DropIndex("dbo.Expenses", new[] { "PaidByUserId" });
            DropIndex("dbo.Expenses", new[] { "GroupId" });
            DropTable("dbo.Payments");
            DropTable("dbo.Groups");
            DropTable("dbo.GroupMembers");
            DropTable("dbo.Users");
            DropTable("dbo.ExpenseShares");
            DropTable("dbo.Expenses");
        }
    }
}
