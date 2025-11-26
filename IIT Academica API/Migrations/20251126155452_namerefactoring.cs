using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IIT_Academica_API.Migrations
{
    /// <inheritdoc />
    public partial class namerefactoring : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AttendanceRecords_TeacherSubjects_TeacherSubjectId",
                table: "AttendanceRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_CourseMaterials_TeacherSubjects_TeacherSubjectId",
                table: "CourseMaterials");

            migrationBuilder.DropForeignKey(
                name: "FK_Enrollments_TeacherSubjects_TeacherSubjectId",
                table: "Enrollments");

            migrationBuilder.DropTable(
                name: "TeacherSubjects");

            migrationBuilder.RenameColumn(
                name: "TeacherSubjectId",
                table: "Enrollments",
                newName: "SubjectId");

            migrationBuilder.RenameIndex(
                name: "IX_Enrollments_TeacherSubjectId",
                table: "Enrollments",
                newName: "IX_Enrollments_SubjectId");

            migrationBuilder.RenameColumn(
                name: "TeacherSubjectId",
                table: "CourseMaterials",
                newName: "SubjectId");

            migrationBuilder.RenameIndex(
                name: "IX_CourseMaterials_TeacherSubjectId",
                table: "CourseMaterials",
                newName: "IX_CourseMaterials_SubjectId");

            migrationBuilder.RenameColumn(
                name: "TeacherSubjectId",
                table: "AttendanceRecords",
                newName: "SubjectId");

            migrationBuilder.RenameIndex(
                name: "IX_AttendanceRecords_TeacherSubjectId",
                table: "AttendanceRecords",
                newName: "IX_AttendanceRecords_SubjectId");

            migrationBuilder.CreateTable(
                name: "Subjects",
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
                    table.PrimaryKey("PK_Subjects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Subjects_AspNetUsers_TeacherId",
                        column: x => x.TeacherId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Subjects_RegistrationCode",
                table: "Subjects",
                column: "RegistrationCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Subjects_TeacherId",
                table: "Subjects",
                column: "TeacherId");

            migrationBuilder.AddForeignKey(
                name: "FK_AttendanceRecords_Subjects_SubjectId",
                table: "AttendanceRecords",
                column: "SubjectId",
                principalTable: "Subjects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CourseMaterials_Subjects_SubjectId",
                table: "CourseMaterials",
                column: "SubjectId",
                principalTable: "Subjects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Enrollments_Subjects_SubjectId",
                table: "Enrollments",
                column: "SubjectId",
                principalTable: "Subjects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AttendanceRecords_Subjects_SubjectId",
                table: "AttendanceRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_CourseMaterials_Subjects_SubjectId",
                table: "CourseMaterials");

            migrationBuilder.DropForeignKey(
                name: "FK_Enrollments_Subjects_SubjectId",
                table: "Enrollments");

            migrationBuilder.DropTable(
                name: "Subjects");

            migrationBuilder.RenameColumn(
                name: "SubjectId",
                table: "Enrollments",
                newName: "TeacherSubjectId");

            migrationBuilder.RenameIndex(
                name: "IX_Enrollments_SubjectId",
                table: "Enrollments",
                newName: "IX_Enrollments_TeacherSubjectId");

            migrationBuilder.RenameColumn(
                name: "SubjectId",
                table: "CourseMaterials",
                newName: "TeacherSubjectId");

            migrationBuilder.RenameIndex(
                name: "IX_CourseMaterials_SubjectId",
                table: "CourseMaterials",
                newName: "IX_CourseMaterials_TeacherSubjectId");

            migrationBuilder.RenameColumn(
                name: "SubjectId",
                table: "AttendanceRecords",
                newName: "TeacherSubjectId");

            migrationBuilder.RenameIndex(
                name: "IX_AttendanceRecords_SubjectId",
                table: "AttendanceRecords",
                newName: "IX_AttendanceRecords_TeacherSubjectId");

            migrationBuilder.CreateTable(
                name: "TeacherSubjects",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TeacherId = table.Column<int>(type: "int", nullable: false),
                    RegistrationCode = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeacherSubjects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeacherSubjects_AspNetUsers_TeacherId",
                        column: x => x.TeacherId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TeacherSubjects_RegistrationCode",
                table: "TeacherSubjects",
                column: "RegistrationCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TeacherSubjects_TeacherId",
                table: "TeacherSubjects",
                column: "TeacherId");

            migrationBuilder.AddForeignKey(
                name: "FK_AttendanceRecords_TeacherSubjects_TeacherSubjectId",
                table: "AttendanceRecords",
                column: "TeacherSubjectId",
                principalTable: "TeacherSubjects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CourseMaterials_TeacherSubjects_TeacherSubjectId",
                table: "CourseMaterials",
                column: "TeacherSubjectId",
                principalTable: "TeacherSubjects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Enrollments_TeacherSubjects_TeacherSubjectId",
                table: "Enrollments",
                column: "TeacherSubjectId",
                principalTable: "TeacherSubjects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
