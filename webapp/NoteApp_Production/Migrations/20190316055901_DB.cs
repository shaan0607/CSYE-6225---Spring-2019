using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace NoteApp_Production.Migrations
{
    public partial class DB : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NOTES",
                columns: table => new
                {
                    ID = table.Column<string>(type: "varchar(100)", nullable: false),
                    content = table.Column<string>(type: "varchar(100)", nullable: false),
                    title = table.Column<string>(type: "varchar(100)", nullable: false),
                    Created_On = table.Column<DateTime>(type: "DateTime", nullable: false),
                    Last_Updated_On = table.Column<DateTime>(type: "DateTime", nullable: false),
                    EMAIL = table.Column<string>(type: "varchar(100)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NOTES", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "USERS",
                columns: table => new
                {
                    EMAIL = table.Column<string>(type: "varchar(100)", nullable: false),
                    PASSWORD = table.Column<string>(type: "varchar(100)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_USERS", x => x.EMAIL);
                });

            migrationBuilder.CreateTable(
                name: "Attachments",
                columns: table => new
                {
                    AID = table.Column<string>(type: "varchar(100)", nullable: false),
                    URL = table.Column<string>(type: "varchar(250)", nullable: true),
                    FileName = table.Column<string>(type: "varchar(100)", nullable: true),
                    length = table.Column<string>(type: "varchar(100)", nullable: false),
                    NoteID = table.Column<string>(type: "varchar(100)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Attachments", x => x.AID);
                    table.ForeignKey(
                        name: "FK_Attachments_NOTES_NoteID",
                        column: x => x.NoteID,
                        principalTable: "NOTES",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Attachments_NoteID",
                table: "Attachments",
                column: "NoteID",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Attachments");

            migrationBuilder.DropTable(
                name: "USERS");

            migrationBuilder.DropTable(
                name: "NOTES");
        }
    }
}
