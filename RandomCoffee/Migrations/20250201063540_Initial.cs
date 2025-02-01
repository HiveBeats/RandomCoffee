using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RandomCoffee.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Groups",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Groups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OutBoxMessages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ChatId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    ReplyToMessageId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    Text = table.Column<string>(type: "TEXT", maxLength: 4096, nullable: false),
                    ParseMode = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutBoxMessages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Coffees",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    AnnouncedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    GroupId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Coffees", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Coffees_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Participant",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserName = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    ScheduledAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CoffeeId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Participant", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Participant_Coffees_CoffeeId",
                        column: x => x.CoffeeId,
                        principalTable: "Coffees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Coffees_AnnouncedAt",
                table: "Coffees",
                column: "AnnouncedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Coffees_GroupId",
                table: "Coffees",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_OutBoxMessages_CreatedAt",
                table: "OutBoxMessages",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_OutBoxMessages_ProcessedAt",
                table: "OutBoxMessages",
                column: "ProcessedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Participant_CoffeeId",
                table: "Participant",
                column: "CoffeeId");

            migrationBuilder.CreateIndex(
                name: "IX_Participant_ScheduledAt",
                table: "Participant",
                column: "ScheduledAt");

            migrationBuilder.CreateIndex(
                name: "IX_Participant_UserName",
                table: "Participant",
                column: "UserName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OutBoxMessages");

            migrationBuilder.DropTable(
                name: "Participant");

            migrationBuilder.DropTable(
                name: "Coffees");

            migrationBuilder.DropTable(
                name: "Groups");
        }
    }
}
