using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedicLaunchApi.Migrations
{
    /// <inheritdoc />
    public partial class AddQuestionIdToTextbookLesson : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "QuestionId",
                table: "TextbookLesson",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TextbookLesson_QuestionId",
                table: "TextbookLesson",
                column: "QuestionId");

            migrationBuilder.AddForeignKey(
                name: "FK_TextbookLesson_Question_QuestionId",
                table: "TextbookLesson",
                column: "QuestionId",
                principalTable: "Question",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TextbookLesson_Question_QuestionId",
                table: "TextbookLesson");

            migrationBuilder.DropIndex(
                name: "IX_TextbookLesson_QuestionId",
                table: "TextbookLesson");

            migrationBuilder.DropColumn(
                name: "QuestionId",
                table: "TextbookLesson");
        }
    }
}
