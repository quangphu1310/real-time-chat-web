using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace real_time_chat_web.Migrations
{
    /// <inheritdoc />
    public partial class addForeignKeyRoom : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_videoCalls_AspNetUsers_CreatedBy",
                table: "videoCalls");

            migrationBuilder.DropForeignKey(
                name: "FK_videoCalls_rooms_RoomId",
                table: "videoCalls");

            migrationBuilder.AddColumn<int>(
                name: "MessageId",
                table: "rooms",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RoomsIdRooms",
                table: "Messages",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_rooms_MessageId",
                table: "rooms",
                column: "MessageId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_RoomsIdRooms",
                table: "Messages",
                column: "RoomsIdRooms");

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_rooms_RoomsIdRooms",
                table: "Messages",
                column: "RoomsIdRooms",
                principalTable: "rooms",
                principalColumn: "IdRooms");

            migrationBuilder.AddForeignKey(
                name: "FK_rooms_Messages_MessageId",
                table: "rooms",
                column: "MessageId",
                principalTable: "Messages",
                principalColumn: "MessageId",
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
                name: "FK_Messages_rooms_RoomsIdRooms",
                table: "Messages");

            migrationBuilder.DropForeignKey(
                name: "FK_rooms_Messages_MessageId",
                table: "rooms");

            migrationBuilder.DropForeignKey(
                name: "FK_videoCalls_AspNetUsers_CreatedBy",
                table: "videoCalls");

            migrationBuilder.DropForeignKey(
                name: "FK_videoCalls_rooms_RoomId",
                table: "videoCalls");

            migrationBuilder.DropIndex(
                name: "IX_rooms_MessageId",
                table: "rooms");

            migrationBuilder.DropIndex(
                name: "IX_Messages_RoomsIdRooms",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "MessageId",
                table: "rooms");

            migrationBuilder.DropColumn(
                name: "RoomsIdRooms",
                table: "Messages");

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
