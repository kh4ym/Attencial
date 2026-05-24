using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Attencial.API.Migrations
{
    /// <inheritdoc />
    public partial class RenameAzureFieldsToRekognition : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AzurePersonId",
                table: "Students",
                newName: "RekognitionExternalId");

            migrationBuilder.RenameColumn(
                name: "AzurePersonId",
                table: "FaceVectors",
                newName: "RekognitionFaceId");

            migrationBuilder.RenameColumn(
                name: "AzureFaceId",
                table: "FaceVectors",
                newName: "RekognitionExternalId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RekognitionExternalId",
                table: "Students",
                newName: "AzurePersonId");

            migrationBuilder.RenameColumn(
                name: "RekognitionFaceId",
                table: "FaceVectors",
                newName: "AzurePersonId");

            migrationBuilder.RenameColumn(
                name: "RekognitionExternalId",
                table: "FaceVectors",
                newName: "AzureFaceId");
        }
    }
}
