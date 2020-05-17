namespace Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ReplaceBelongsToProjectByProjectId : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Tags", "ProjectId", c => c.Int());
            CreateIndex("dbo.Tags", "ProjectId");
            AddForeignKey("dbo.Tags", "ProjectId", "dbo.Projects", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Tags", "ProjectId", "dbo.Projects");
            DropIndex("dbo.Tags", new[] { "ProjectId" });
            DropColumn("dbo.Tags", "ProjectId");
        }
    }
}
