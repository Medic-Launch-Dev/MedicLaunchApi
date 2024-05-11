using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedicLaunchApi.Migrations
{
    /// <inheritdoc />
    public partial class Subscription : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "SubscriptionCreatedDate",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SubscriptionExpiryDate",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SubscriptionPlanId",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SubscriptionCreatedDate",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "SubscriptionExpiryDate",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "SubscriptionPlanId",
                table: "AspNetUsers");
        }
    }
}
