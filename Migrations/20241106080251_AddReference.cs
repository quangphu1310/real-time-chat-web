using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace real_time_chat_web.Migrations
{
    /// <inheritdoc />
    public partial class AddReference : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                table: "ApplicationRooms",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationRooms_CreatedBy",
                table: "ApplicationRooms",
                column: "CreatedBy");

            migrationBuilder.AddForeignKey(
                name: "FK_ApplicationRooms_AspNetUsers_CreatedBy",
                table: "ApplicationRooms",
                column: "CreatedBy",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ApplicationRooms_AspNetUsers_CreatedBy",
                table: "ApplicationRooms");

            migrationBuilder.DropIndex(
                name: "IX_ApplicationRooms_CreatedBy",
                table: "ApplicationRooms");

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                table: "ApplicationRooms",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
