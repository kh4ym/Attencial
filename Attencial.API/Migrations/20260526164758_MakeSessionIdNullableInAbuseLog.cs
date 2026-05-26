using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Attencial.API.Migrations
{
    /// <inheritdoc />
    public partial class MakeSessionIdNullableInAbuseLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AbuseLogs_AttendanceSessions_SessionId",
                table: "AbuseLogs");

            migrationBuilder.AlterColumn<int>(
                name: "SessionId",
                table: "AbuseLogs",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddForeignKey(
                name: "FK_AbuseLogs_AttendanceSessions_SessionId",
                table: "AbuseLogs",
                column: "SessionId",
                principalTable: "AttendanceSessions",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AbuseLogs_AttendanceSessions_SessionId",
                table: "AbuseLogs");

            migrationBuilder.AlterColumn<int>(
                name: "SessionId",
                table: "AbuseLogs",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AbuseLogs_AttendanceSessions_SessionId",
                table: "AbuseLogs",
                column: "SessionId",
                principalTable: "AttendanceSessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
