using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Attencial.API.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSchemaForAppealsAndCleanups : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DELETE FROM ""AttendanceAppeals"" a
                WHERE a.""Id"" NOT IN (
                    SELECT MAX(b.""Id"")
                    FROM ""AttendanceAppeals"" b
                    GROUP BY b.""SessionId"", b.""StudentId""
                );
            ");

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceAppeals_SessionId_StudentId",
                table: "AttendanceAppeals",
                columns: new[] { "SessionId", "StudentId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AttendanceAppeals_SessionId_StudentId",
                table: "AttendanceAppeals");
        }
    }
}
