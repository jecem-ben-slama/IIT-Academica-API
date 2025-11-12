using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IIT_Academica_API.Migrations
{
    /// <inheritdoc />
    public partial class FkFixed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TeacherSubjects",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RegistrationCode = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TeacherId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeacherSubjects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeacherSubjects_Users_TeacherId",
                        column: x => x.TeacherId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CourseMaterials",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AccessUntilDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TeacherSubjectId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseMaterials", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CourseMaterials_TeacherSubjects_TeacherSubjectId",
                        column: x => x.TeacherSubjectId,
                        principalTable: "TeacherSubjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Enrollments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    TeacherSubjectId = table.Column<int>(type: "int", nullable: false),
                    EnrollmentDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Enrollments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Enrollments_TeacherSubjects_TeacherSubjectId",
                        column: x => x.TeacherSubjectId,
                        principalTable: "TeacherSubjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Enrollments_Users_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CourseMaterials_TeacherSubjectId",
                table: "CourseMaterials",
                column: "TeacherSubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Enrollments_StudentId",
                table: "Enrollments",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_Enrollments_TeacherSubjectId",
                table: "Enrollments",
                column: "TeacherSubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_TeacherSubjects_RegistrationCode",
                table: "TeacherSubjects",
                column: "RegistrationCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TeacherSubjects_TeacherId",
                table: "TeacherSubjects",
                column: "TeacherId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CourseMaterials");

            migrationBuilder.DropTable(
                name: "Enrollments");

            migrationBuilder.DropTable(
                name: "TeacherSubjects");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
