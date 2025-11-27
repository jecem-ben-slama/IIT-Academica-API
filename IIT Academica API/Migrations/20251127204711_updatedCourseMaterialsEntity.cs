using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IIT_Academica_API.Migrations
{
    /// <inheritdoc />
    public partial class updatedCourseMaterialsEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FilePath",
                table: "CourseMaterials",
                newName: "MaterialType");

            migrationBuilder.RenameColumn(
                name: "AccessUntilDate",
                table: "CourseMaterials",
                newName: "UploadDate");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "CourseMaterials",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FilePathOrUrl",
                table: "CourseMaterials",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "CourseMaterials");

            migrationBuilder.DropColumn(
                name: "FilePathOrUrl",
                table: "CourseMaterials");

            migrationBuilder.RenameColumn(
                name: "UploadDate",
                table: "CourseMaterials",
                newName: "AccessUntilDate");

            migrationBuilder.RenameColumn(
                name: "MaterialType",
                table: "CourseMaterials",
                newName: "FilePath");
        }
    }
}
