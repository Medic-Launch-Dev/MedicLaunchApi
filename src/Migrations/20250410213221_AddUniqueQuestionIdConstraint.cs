using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedicLaunchApi.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueQuestionIdConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TextbookLesson_QuestionId",
                table: "TextbookLesson");

            migrationBuilder.CreateIndex(
                name: "IX_TextbookLesson_QuestionId",
                table: "TextbookLesson",
                column: "QuestionId",
                unique: true,
                filter: "[QuestionId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TextbookLesson_QuestionId",
                table: "TextbookLesson");

            migrationBuilder.CreateIndex(
                name: "IX_TextbookLesson_QuestionId",
                table: "TextbookLesson",
                column: "QuestionId");
        }
    }
}
