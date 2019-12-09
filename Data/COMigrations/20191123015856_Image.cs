using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace CampOrno.Data.COMigrations
{
    public partial class Image : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "imageContent",
                schema: "CO",
                table: "Campers",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "imageFileName",
                schema: "CO",
                table: "Campers",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "imageMimeType",
                schema: "CO",
                table: "Campers",
                maxLength: 256,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "imageContent",
                schema: "CO",
                table: "Campers");

            migrationBuilder.DropColumn(
                name: "imageFileName",
                schema: "CO",
                table: "Campers");

            migrationBuilder.DropColumn(
                name: "imageMimeType",
                schema: "CO",
                table: "Campers");
        }
    }
}
