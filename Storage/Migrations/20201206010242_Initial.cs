using Microsoft.EntityFrameworkCore.Migrations;

namespace Memenim.Storage.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PostCommentDrafts",
                columns: table => new
                {
                    UserId = table.Column<uint>(type: "INTEGER", nullable: false),
                    PostId = table.Column<uint>(type: "INTEGER", nullable: false),
                    CommentText = table.Column<string>(type: "TEXT", nullable: false, defaultValue: ""),
                    IsAnonymous = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostCommentDrafts", x => new { x.UserId, x.PostId });
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PostCommentDrafts");
        }
    }
}
