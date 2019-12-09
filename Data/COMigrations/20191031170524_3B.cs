using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace CampOrno.Data.COMigrations
{
    public partial class _3B : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                schema: "CO",
                table: "Counselors",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedOn",
                schema: "CO",
                table: "Counselors",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                schema: "CO",
                table: "Counselors",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                schema: "CO",
                table: "Counselors",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedOn",
                schema: "CO",
                table: "Counselors",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                schema: "CO",
                table: "Campers",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedOn",
                schema: "CO",
                table: "Campers",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                schema: "CO",
                table: "Campers",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                schema: "CO",
                table: "Campers",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedOn",
                schema: "CO",
                table: "Campers",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedBy",
                schema: "CO",
                table: "Counselors");

            migrationBuilder.DropColumn(
                name: "CreatedOn",
                schema: "CO",
                table: "Counselors");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                schema: "CO",
                table: "Counselors");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                schema: "CO",
                table: "Counselors");

            migrationBuilder.DropColumn(
                name: "UpdatedOn",
                schema: "CO",
                table: "Counselors");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                schema: "CO",
                table: "Campers");

            migrationBuilder.DropColumn(
                name: "CreatedOn",
                schema: "CO",
                table: "Campers");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                schema: "CO",
                table: "Campers");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                schema: "CO",
                table: "Campers");

            migrationBuilder.DropColumn(
                name: "UpdatedOn",
                schema: "CO",
                table: "Campers");
        }
    }
}
