using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IIT_Academica_API.Migrations
{
    /// <inheritdoc />
    public partial class updatednotificationsentity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_AspNetUsers_AdminId",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_AdminId",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "AdminId",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "TargetRole",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Notifications");

            migrationBuilder.RenameColumn(
                name: "CreatedDate",
                table: "Notifications",
                newName: "PostedDate");

            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "Notifications",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "Notifications",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PostedByUserId",
                table: "Notifications",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "PostedByUserId",
                table: "Notifications");

            migrationBuilder.RenameColumn(
                name: "PostedDate",
                table: "Notifications",
                newName: "CreatedDate");

            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "Notifications",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<int>(
                name: "AdminId",
                table: "Notifications",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TargetRole",
                table: "Notifications",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "Notifications",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_AdminId",
                table: "Notifications",
                column: "AdminId");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_AspNetUsers_AdminId",
                table: "Notifications",
                column: "AdminId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
