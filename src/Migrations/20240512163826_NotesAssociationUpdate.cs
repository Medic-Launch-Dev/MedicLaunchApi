using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedicLaunchApi.Migrations
{
    /// <inheritdoc />
    public partial class NotesAssociationUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Note_Speciality_SpecialityId",
                table: "Note");

            migrationBuilder.AlterColumn<string>(
                name: "SpecialityId",
                table: "Note",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<string>(
                name: "FlashcardId",
                table: "Note",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "QuestionId",
                table: "Note",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Note_FlashcardId",
                table: "Note",
                column: "FlashcardId");

            migrationBuilder.CreateIndex(
                name: "IX_Note_QuestionId",
                table: "Note",
                column: "QuestionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Note_Flashcard_FlashcardId",
                table: "Note",
                column: "FlashcardId",
                principalTable: "Flashcard",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Note_Question_QuestionId",
                table: "Note",
                column: "QuestionId",
                principalTable: "Question",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Note_Speciality_SpecialityId",
                table: "Note",
                column: "SpecialityId",
                principalTable: "Speciality",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Note_Flashcard_FlashcardId",
                table: "Note");

            migrationBuilder.DropForeignKey(
                name: "FK_Note_Question_QuestionId",
                table: "Note");

            migrationBuilder.DropForeignKey(
                name: "FK_Note_Speciality_SpecialityId",
                table: "Note");

            migrationBuilder.DropIndex(
                name: "IX_Note_FlashcardId",
                table: "Note");

            migrationBuilder.DropIndex(
                name: "IX_Note_QuestionId",
                table: "Note");

            migrationBuilder.DropColumn(
                name: "FlashcardId",
                table: "Note");

            migrationBuilder.DropColumn(
                name: "QuestionId",
                table: "Note");

            migrationBuilder.AlterColumn<string>(
                name: "SpecialityId",
                table: "Note",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Note_Speciality_SpecialityId",
                table: "Note",
                column: "SpecialityId",
                principalTable: "Speciality",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
