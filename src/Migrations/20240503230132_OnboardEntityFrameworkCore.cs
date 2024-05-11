using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedicLaunchApi.Migrations
{
    /// <inheritdoc />
    public partial class OnboardEntityFrameworkCore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Speciality",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Speciality", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Question",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    QuestionType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    QuestionText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CorrectAnswerLetter = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Explanation = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ClinicalTips = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LearningPoints = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    QuestionState = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SpecialityId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Question", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Question_Speciality_SpecialityId",
                        column: x => x.SpecialityId,
                        principalTable: "Speciality",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AnswerOption",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Letter = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Text = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    QuestionId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnswerOption", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AnswerOption_Question_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "Question",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AnswerOption_QuestionId",
                table: "AnswerOption",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_Question_SpecialityId",
                table: "Question",
                column: "SpecialityId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AnswerOption");

            migrationBuilder.DropTable(
                name: "Question");

            migrationBuilder.DropTable(
                name: "Speciality");
        }
    }
}
