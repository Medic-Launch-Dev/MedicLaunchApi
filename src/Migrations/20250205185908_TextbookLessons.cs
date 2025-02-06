using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedicLaunchApi.Migrations
{
    /// <inheritdoc />
    public partial class TextbookLessons : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TextbookLesson",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SpecialityId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TextbookLesson", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TextbookLesson_Speciality_SpecialityId",
                        column: x => x.SpecialityId,
                        principalTable: "Speciality",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TextbookLessonContent",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TextbookLessonId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Heading = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Text = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TextbookLessonContent", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TextbookLessonContent_TextbookLesson_TextbookLessonId",
                        column: x => x.TextbookLessonId,
                        principalTable: "TextbookLesson",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TextbookLesson_SpecialityId",
                table: "TextbookLesson",
                column: "SpecialityId");

            migrationBuilder.CreateIndex(
                name: "IX_TextbookLessonContent_TextbookLessonId",
                table: "TextbookLessonContent",
                column: "TextbookLessonId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TextbookLessonContent");

            migrationBuilder.DropTable(
                name: "TextbookLesson");
        }
    }
}
