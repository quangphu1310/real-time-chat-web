using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace real_time_chat_web.Migrations
{
    /// <inheritdoc />
    public partial class updaterooms : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RoomsIdRooms",
                table: "RoomsUser",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RoomsUser_RoomsIdRooms",
                table: "RoomsUser",
                column: "RoomsIdRooms");

            migrationBuilder.AddForeignKey(
                name: "FK_RoomsUser_rooms_RoomsIdRooms",
                table: "RoomsUser",
                column: "RoomsIdRooms",
                principalTable: "rooms",
                principalColumn: "IdRooms");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RoomsUser_rooms_RoomsIdRooms",
                table: "RoomsUser");

            migrationBuilder.DropIndex(
                name: "IX_RoomsUser_RoomsIdRooms",
                table: "RoomsUser");

            migrationBuilder.DropColumn(
                name: "RoomsIdRooms",
                table: "RoomsUser");
        }
    }
}
