using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace real_time_chat_web.Migrations
{
    /// <inheritdoc />
    public partial class addRoomsUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RoomsUser",
                columns: table => new
                {
                    IdRooms = table.Column<int>(type: "int", nullable: false),
                    IdUser = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    IdPerAdd = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DayAdd = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoomsUser", x => new { x.IdRooms, x.IdUser });
                    table.ForeignKey(
                        name: "FK_RoomsUser_AspNetUsers_IdPerAdd",
                        column: x => x.IdPerAdd,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RoomsUser_AspNetUsers_IdUser",
                        column: x => x.IdUser,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RoomsUser_rooms_IdRooms",
                        column: x => x.IdRooms,
                        principalTable: "rooms",
                        principalColumn: "IdRooms",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RoomsUser_IdPerAdd",
                table: "RoomsUser",
                column: "IdPerAdd");

            migrationBuilder.CreateIndex(
                name: "IX_RoomsUser_IdUser",
                table: "RoomsUser",
                column: "IdUser");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RoomsUser");
        }
    }
}
