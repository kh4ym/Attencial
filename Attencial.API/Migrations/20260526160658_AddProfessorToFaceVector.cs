using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Attencial.API.Migrations
{
    /// <inheritdoc />
    public partial class AddProfessorToFaceVector : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FaceVectors_Students_StudentId",
                table: "FaceVectors");

            migrationBuilder.AlterColumn<int>(
                name: "StudentId",
                table: "FaceVectors",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<int>(
                name: "ProfessorId",
                table: "FaceVectors",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_FaceVectors_ProfessorId",
                table: "FaceVectors",
                column: "ProfessorId");

            migrationBuilder.AddForeignKey(
                name: "FK_FaceVectors_Professors_ProfessorId",
                table: "FaceVectors",
                column: "ProfessorId",
                principalTable: "Professors",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_FaceVectors_Students_StudentId",
                table: "FaceVectors",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FaceVectors_Professors_ProfessorId",
                table: "FaceVectors");

            migrationBuilder.DropForeignKey(
                name: "FK_FaceVectors_Students_StudentId",
                table: "FaceVectors");

            migrationBuilder.DropIndex(
                name: "IX_FaceVectors_ProfessorId",
                table: "FaceVectors");

            migrationBuilder.DropColumn(
                name: "ProfessorId",
                table: "FaceVectors");

            migrationBuilder.AlterColumn<int>(
                name: "StudentId",
                table: "FaceVectors",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_FaceVectors_Students_StudentId",
                table: "FaceVectors",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
