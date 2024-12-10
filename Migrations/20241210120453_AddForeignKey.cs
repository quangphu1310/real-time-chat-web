using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace real_time_chat_web.Migrations
{
    /// <inheritdoc />
    public partial class AddForeignKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Messages_rooms_RoomId",
                table: "Messages");

            migrationBuilder.DropForeignKey(
                name: "FK_videoCalls_AspNetUsers_CreatedBy",
                table: "videoCalls");

            migrationBuilder.DropForeignKey(
                name: "FK_videoCalls_rooms_RoomId",
                table: "videoCalls");

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_rooms_RoomId",
                table: "Messages",
                column: "RoomId",
                principalTable: "rooms",
                principalColumn: "IdRooms",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_videoCalls_AspNetUsers_CreatedBy",
                table: "videoCalls",
                column: "CreatedBy",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_videoCalls_rooms_RoomId",
                table: "videoCalls",
                column: "RoomId",
                principalTable: "rooms",
                principalColumn: "IdRooms");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Messages_rooms_RoomId",
                table: "Messages");

            migrationBuilder.DropForeignKey(
                name: "FK_videoCalls_AspNetUsers_CreatedBy",
                table: "videoCalls");

            migrationBuilder.DropForeignKey(
                name: "FK_videoCalls_rooms_RoomId",
                table: "videoCalls");

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_rooms_RoomId",
                table: "Messages",
                column: "RoomId",
                principalTable: "rooms",
                principalColumn: "IdRooms",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_videoCalls_AspNetUsers_CreatedBy",
                table: "videoCalls",
                column: "CreatedBy",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_videoCalls_rooms_RoomId",
                table: "videoCalls",
                column: "RoomId",
                principalTable: "rooms",
                principalColumn: "IdRooms",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
