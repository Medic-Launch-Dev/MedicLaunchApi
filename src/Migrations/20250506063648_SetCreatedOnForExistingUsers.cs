using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedicLaunchApi.Migrations
{
    /// <inheritdoc />
    public partial class SetCreatedOnForExistingUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedOn",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);

            // Add SQL to update existing records
            migrationBuilder.Sql(@"
                UPDATE AspNetUsers 
                SET CreatedOn = CASE
                    WHEN SubscriptionCreatedDate IS NOT NULL THEN SubscriptionCreatedDate
                    ELSE DATEADD(day, -7, GETUTCDATE())
                END
                WHERE CreatedOn IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedOn",
                table: "AspNetUsers");
        }
    }
}
