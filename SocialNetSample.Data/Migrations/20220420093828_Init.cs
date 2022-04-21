using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SocialNetSample.Data.Migrations
{
    public partial class Init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BlockedUsers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SourceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TargetId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    UserIdA = table.Column<Guid>(type: "uniqueidentifier", nullable: false, computedColumnSql: "case when SourceId < TargetId then SourceId else TargetId end"),
                    UserIdB = table.Column<Guid>(type: "uniqueidentifier", nullable: false, computedColumnSql: "case when SourceId < TargetId then TargetId else SourceId end")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlockedUsers", x => x.Id);
                    table.CheckConstraint("BlockedUsers_SourceId_neq_TargetId", "[SourceId] != [TargetId]");
                    table.ForeignKey(
                        name: "FK_BlockedUsers_Users_SourceId",
                        column: x => x.SourceId,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BlockedUsers_Users_TargetId",
                        column: x => x.TargetId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "FriendRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SourceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TargetId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    UserIdA = table.Column<Guid>(type: "uniqueidentifier", nullable: false, computedColumnSql: "case when SourceId < TargetId then SourceId else TargetId end"),
                    UserIdB = table.Column<Guid>(type: "uniqueidentifier", nullable: false, computedColumnSql: "case when SourceId < TargetId then TargetId else SourceId end")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FriendRequests", x => x.Id);
                    table.CheckConstraint("FriendRequests_SourceId_neq_TargetId", "[SourceId] != [TargetId]");
                    table.ForeignKey(
                        name: "FK_FriendRequests_Users_SourceId",
                        column: x => x.SourceId,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_FriendRequests_Users_TargetId",
                        column: x => x.TargetId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Friends",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SourceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TargetId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSDATETIMEOFFSET()"),
                    UserIdA = table.Column<Guid>(type: "uniqueidentifier", nullable: false, computedColumnSql: "case when SourceId < TargetId then SourceId else TargetId end"),
                    UserIdB = table.Column<Guid>(type: "uniqueidentifier", nullable: false, computedColumnSql: "case when SourceId < TargetId then TargetId else SourceId end")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Friends", x => x.Id);
                    table.CheckConstraint("Friends_SourceId_neq_TargetId", "[SourceId] != [TargetId]");
                    table.ForeignKey(
                        name: "FK_Friends_Users_SourceId",
                        column: x => x.SourceId,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Friends_Users_TargetId",
                        column: x => x.TargetId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_BlockedUsers_SourceId",
                table: "BlockedUsers",
                column: "SourceId");

            migrationBuilder.CreateIndex(
                name: "IX_BlockedUsers_TargetId",
                table: "BlockedUsers",
                column: "TargetId");

            migrationBuilder.CreateIndex(
                name: "IX_BlockedUsers_UserIdA_UserIdB",
                table: "BlockedUsers",
                columns: new[] { "UserIdA", "UserIdB" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FriendRequests_SourceId",
                table: "FriendRequests",
                column: "SourceId");

            migrationBuilder.CreateIndex(
                name: "IX_FriendRequests_TargetId",
                table: "FriendRequests",
                column: "TargetId");

            migrationBuilder.CreateIndex(
                name: "IX_FriendRequests_UserIdA_UserIdB",
                table: "FriendRequests",
                columns: new[] { "UserIdA", "UserIdB" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Friends_SourceId",
                table: "Friends",
                column: "SourceId");

            migrationBuilder.CreateIndex(
                name: "IX_Friends_TargetId",
                table: "Friends",
                column: "TargetId");

            migrationBuilder.CreateIndex(
                name: "IX_Friends_UserIdA_UserIdB",
                table: "Friends",
                columns: new[] { "UserIdA", "UserIdB" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BlockedUsers");

            migrationBuilder.DropTable(
                name: "FriendRequests");

            migrationBuilder.DropTable(
                name: "Friends");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
