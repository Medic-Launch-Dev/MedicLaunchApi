using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedicLaunchApi.Migrations
{
    /// <inheritdoc />
    public partial class AddTrialQuestionTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TrialQuestion",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    QuestionType = table.Column<int>(type: "int", nullable: false),
                    QuestionText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CorrectAnswerLetter = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Explanation = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ClinicalTips = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LearningPoints = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    QuestionState = table.Column<int>(type: "int", nullable: false),
                    SpecialityId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    VideoUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrialQuestion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrialQuestion_Speciality_SpecialityId",
                        column: x => x.SpecialityId,
                        principalTable: "Speciality",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TrialAnswerOption",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Letter = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Text = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TrialQuestionId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrialAnswerOption", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrialAnswerOption_TrialQuestion_TrialQuestionId",
                        column: x => x.TrialQuestionId,
                        principalTable: "TrialQuestion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TrialAnswerOption_TrialQuestionId",
                table: "TrialAnswerOption",
                column: "TrialQuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_TrialQuestion_SpecialityId",
                table: "TrialQuestion",
                column: "SpecialityId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TrialAnswerOption");

            migrationBuilder.DropTable(
                name: "TrialQuestion");
        }
    }
}
