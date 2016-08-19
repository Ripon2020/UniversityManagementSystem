namespace University.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class change7Init : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.RoomAllocations", "Status", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.RoomAllocations", "Status");
        }
    }
}
