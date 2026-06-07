using Lilliput.Api.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Lilliput.Api.Services;

public class InvoicePdfService
{
    private const string Accent = "#F47C20";
    private const string Dark = "#252525";
    private const string SoftText = "#666666";
    private const string LightBg = "#FFF4EC";
    private const string Border = "#E8E2DC";

    public byte[] GenerateInvoicePdf(Invoice invoice)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(36);
                page.DefaultTextStyle(x => x.FontSize(10).FontColor(Dark));

                page.Header().Element(c => ComposeHeader(c, invoice));

                page.Content().PaddingTop(24).Column(column =>
                {
                    column.Spacing(18);

                    column.Item().Row(row =>
                    {
                        row.RelativeItem().Element(c => ComposeBillTo(c, invoice));
                        row.ConstantItem(230).Element(c => ComposeInvoiceInfo(c, invoice));
                    });

                    column.Item().Element(c => ComposeTable(c, invoice));

                    column.Item().Row(row =>
                    {
                        row.RelativeItem().Element(c => ComposePaymentDetails(c, invoice));
                        row.ConstantItem(230).Element(c => ComposeTotals(c, invoice));
                    });

                    column.Item().Element(c => ComposeNotes(c, invoice));
                });

                page.Footer().Element(ComposeFooter);
            });
        }).GeneratePdf();
    }

    private static void ComposeHeader(IContainer container, Invoice invoice)
    {
        container.Column(column =>
        {
            column.Item()
                .Background(Accent)
                .Padding(18)
                .Row(row =>
                {
                    row.RelativeItem().Column(left =>
                    {
                        left.Item().Text(invoice.CompanyName.ToUpper())
                            .FontSize(18)
                            .Bold()
                            .FontColor(Colors.White);

                        left.Item().PaddingTop(4).Text(invoice.CompanyAddress)
                            .FontSize(9)
                            .FontColor(Colors.White);

                        left.Item().Text(BuildCompanyContact(invoice))
                            .FontSize(9)
                            .FontColor(Colors.White);
                    });

                    row.ConstantItem(150).AlignRight().AlignMiddle().Text("INVOICE")
                        .FontSize(28)
                        .Bold()
                        .FontColor(Colors.White);
                });

            column.Item().Height(6).Background("#D96A18");
        });
    }

    private static void ComposeBillTo(IContainer container, Invoice invoice)
    {
        container
            .Border(1)
            .BorderColor(Border)
            .Padding(16)
            .Column(column =>
            {
                column.Spacing(5);

                column.Item().Text("BILL TO")
                    .FontSize(9)
                    .Bold()
                    .FontColor(Accent);

                column.Item().Text(invoice.SchoolName)
                    .FontSize(14)
                    .Bold();

                column.Item().Text(invoice.Location)
                    .FontSize(10)
                    .FontColor(SoftText);

                column.Item().Text(invoice.SchoolEmail)
                    .FontSize(10)
                    .FontColor(SoftText);
            });
    }

    private static void ComposeInvoiceInfo(IContainer container, Invoice invoice)
    {
        container
            .Background(LightBg)
            .Border(1)
            .BorderColor(Border)
            .Padding(16)
            .Column(column =>
            {
                column.Spacing(8);

                AddInfoRow(column, "invoice no", invoice.InvoiceNumber);
                AddInfoRow(column, "invoice date", invoice.CreatedAt.ToString("dd/MM/yyyy"));
                AddInfoRow(column, "visit date", invoice.DateVisited.ToString("dd/MM/yyyy"));
                AddInfoRow(column, "status", invoice.Status);
                AddInfoRow(column, "reference", invoice.PaymentReference);
            });
    }

    private static void ComposeTable(IContainer container, Invoice invoice)
    {
        container.Column(column =>
        {
            column.Item().Text("Invoice Details")
                .FontSize(14)
                .Bold();

            column.Item().PaddingTop(8).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(4);
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                });

                table.Header(header =>
                {
                    HeaderCell(header.Cell(), "description");
                    HeaderCell(header.Cell().AlignRight(), "qty");
                    HeaderCell(header.Cell().AlignRight(), "rate");
                    HeaderCell(header.Cell().AlignRight(), "total");
                });

                AddRow(table, "Children", invoice.ActualKidsCount, invoice.PricePerChild);
                AddRow(table, "Teachers / Adults", invoice.TeachersCount, invoice.PricePerTeacher);

                AddFlatRow(table, "Extra charges", invoice.ExtraCharges);

                if (invoice.Discount > 0)
                    AddFlatRow(table, "Discount", -invoice.Discount);
            });
        });
    }

    private static void ComposeTotals(IContainer container, Invoice invoice)
    {
        var subtotal =
            (invoice.ActualKidsCount * invoice.PricePerChild) +
            (invoice.TeachersCount * invoice.PricePerTeacher) +
            invoice.ExtraCharges;

        container.Column(column =>
        {
            column.Spacing(8);

            AddMoneyRow(column, "subtotal", subtotal);

            if (invoice.Discount > 0)
                AddMoneyRow(column, "discount", -invoice.Discount);

            column.Item()
                .Background(Accent)
                .Padding(14)
                .Row(row =>
                {
                    row.RelativeItem().Text("TOTAL DUE")
                        .FontSize(12)
                        .Bold()
                        .FontColor(Colors.White);

                    row.ConstantItem(110).AlignRight().Text($"€{invoice.TotalAmount:0.00}")
                        .FontSize(16)
                        .Bold()
                        .FontColor(Colors.White);
                });
        });
    }

    private static void ComposePaymentDetails(IContainer container, Invoice invoice)
    {
        container
            .Border(1)
            .BorderColor(Border)
            .Padding(16)
            .Column(column =>
            {
                column.Spacing(6);

                column.Item().Text("PAYMENT DETAILS")
                    .FontSize(9)
                    .Bold()
                    .FontColor(Accent);

                column.Item().Text($"Bank: {Fallback(invoice.BankName, "to be confirmed")}");
                column.Item().Text($"IBAN: {Fallback(invoice.Iban, "to be confirmed")}");
                column.Item().Text($"BIC: {Fallback(invoice.Bic, "to be confirmed")}");
                column.Item().Text($"Reference: {Fallback(invoice.PaymentReference, invoice.InvoiceNumber)}")
                    .Bold();
            });
    }

    private static void ComposeNotes(IContainer container, Invoice invoice)
    {
        var notes = string.IsNullOrWhiteSpace(invoice.Notes)
            ? "Thank you for visiting Lilliput Adventure Centre. Payment is due within 30 days unless otherwise agreed."
            : invoice.Notes;

        container
            .Background("#FAFAFA")
            .Border(1)
            .BorderColor(Border)
            .Padding(14)
            .Column(column =>
            {
                column.Item().Text("NOTES")
                    .FontSize(9)
                    .Bold()
                    .FontColor(Accent);

                column.Item().PaddingTop(5).Text(notes)
                    .FontSize(10)
                    .FontColor(SoftText);
            });
    }

    private static void ComposeFooter(IContainer container)
    {
        container
            .BorderTop(1)
            .BorderColor(Border)
            .PaddingTop(10)
            .AlignCenter()
            .Text("Lilliput Adventure Centre • Mullingar, Co. Westmeath • Thank you")
            .FontSize(9)
            .FontColor(SoftText);
    }

    private static void AddRow(TableDescriptor table, string description, int qty, decimal rate)
    {
        BodyCell(table.Cell(), description);
        BodyCell(table.Cell().AlignRight(), qty.ToString());
        BodyCell(table.Cell().AlignRight(), $"€{rate:0.00}");
        BodyCell(table.Cell().AlignRight(), $"€{qty * rate:0.00}");
    }

    private static void AddFlatRow(TableDescriptor table, string description, decimal amount)
    {
        BodyCell(table.Cell(), description);
        BodyCell(table.Cell().AlignRight(), "-");
        BodyCell(table.Cell().AlignRight(), "-");
        BodyCell(table.Cell().AlignRight(), $"€{amount:0.00}");
    }

    private static void HeaderCell(IContainer container, string text)
    {
        container
            .Background(LightBg)
            .BorderBottom(1)
            .BorderColor(Border)
            .PaddingVertical(9)
            .PaddingHorizontal(8)
            .Text(text.ToUpper())
            .FontSize(8)
            .Bold()
            .FontColor(Accent);
    }

    private static void BodyCell(IContainer container, string text)
    {
        container
            .BorderBottom(1)
            .BorderColor(Border)
            .PaddingVertical(9)
            .PaddingHorizontal(8)
            .Text(text);
    }

    private static void AddInfoRow(ColumnDescriptor column, string label, string value)
    {
        column.Item().Row(row =>
        {
            row.RelativeItem().Text(label)
                .FontSize(9)
                .FontColor(SoftText);

            row.ConstantItem(110).AlignRight().Text(Fallback(value, "-"))
                .FontSize(9)
                .Bold();
        });
    }

    private static void AddMoneyRow(ColumnDescriptor column, string label, decimal amount)
    {
        column.Item().Row(row =>
        {
            row.RelativeItem().Text(label)
                .FontSize(10)
                .FontColor(SoftText);

            row.ConstantItem(110).AlignRight().Text($"€{amount:0.00}")
                .FontSize(10)
                .Bold();
        });
    }

    private static string BuildCompanyContact(Invoice invoice)
    {
        var parts = new List<string>();

        if (!string.IsNullOrWhiteSpace(invoice.CompanyEmail))
            parts.Add(invoice.CompanyEmail);

        if (!string.IsNullOrWhiteSpace(invoice.CompanyPhone))
            parts.Add(invoice.CompanyPhone);

        return parts.Count == 0 ? "Lilliput Adventure Centre" : string.Join(" • ", parts);
    }

    private static string Fallback(string? value, string fallback)
    {
        return string.IsNullOrWhiteSpace(value) ? fallback : value;
    }
}