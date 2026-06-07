using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lilliput.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddInvoiceFormSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GeneratedAt",
                table: "Invoices");

            migrationBuilder.RenameColumn(
                name: "VisitDate",
                table: "Invoices",
                newName: "DateVisited");

            migrationBuilder.RenameColumn(
                name: "KidsCount",
                table: "Invoices",
                newName: "ExpectedKidsCount");

            migrationBuilder.RenameColumn(
                name: "BookingType",
                table: "Invoices",
                newName: "Notes");

            migrationBuilder.AlterColumn<Guid>(
                name: "BookingId",
                table: "Invoices",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<int>(
                name: "ActualKidsCount",
                table: "Invoices",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "Discount",
                table: "Invoices",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ExtraCharges",
                table: "Invoices",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "PdfFileName",
                table: "Invoices",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PdfUrl",
                table: "Invoices",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PricePerChild",
                table: "Invoices",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PricePerTeacher",
                table: "Invoices",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalAmount",
                table: "Invoices",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActualKidsCount",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "Discount",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "ExtraCharges",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "PdfFileName",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "PdfUrl",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "PricePerChild",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "PricePerTeacher",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "TotalAmount",
                table: "Invoices");

            migrationBuilder.RenameColumn(
                name: "Notes",
                table: "Invoices",
                newName: "BookingType");

            migrationBuilder.RenameColumn(
                name: "ExpectedKidsCount",
                table: "Invoices",
                newName: "KidsCount");

            migrationBuilder.RenameColumn(
                name: "DateVisited",
                table: "Invoices",
                newName: "VisitDate");

            migrationBuilder.AlterColumn<Guid>(
                name: "BookingId",
                table: "Invoices",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "GeneratedAt",
                table: "Invoices",
                type: "timestamp with time zone",
                nullable: true);
        }
    }
}
