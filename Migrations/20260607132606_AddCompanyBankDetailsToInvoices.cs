using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lilliput.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddCompanyBankDetailsToInvoices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BankName",
                table: "Invoices",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Bic",
                table: "Invoices",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CompanyAddress",
                table: "Invoices",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CompanyEmail",
                table: "Invoices",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CompanyName",
                table: "Invoices",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CompanyPhone",
                table: "Invoices",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Iban",
                table: "Invoices",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PaymentReference",
                table: "Invoices",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BankName",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "Bic",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "CompanyAddress",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "CompanyEmail",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "CompanyName",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "CompanyPhone",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "Iban",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "PaymentReference",
                table: "Invoices");
        }
    }
}
