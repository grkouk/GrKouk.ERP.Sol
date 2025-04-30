using System;
using System.Collections.Generic;
using GrKouk.Erp.Domain.DocDefinitions;
using GrKouk.Web.ERP.Helpers;

namespace GrKouk.Web.ERP.Services;

using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestPDF.Drawing;

public class PdfExportService
{
    public byte[] GenerateFinancialPdf(List<KartelaLine> movements,string reportTitle)
    {
        QuestPDF.Settings.License = LicenseType.Community; 
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(10));
                page.Header().Row(row =>
                {
                    // Left: Logo
                   // row.RelativeColumn().Image(logoPath, ImageScaling.FitHeight).Height(50);

                    // Right: Title
                    row.ConstantItem(500).AlignCenter().Text(reportTitle)
                        .Bold().FontSize(12);
                    row.Spacing(20);
                });
                page.Content().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(1); // Date
                        columns.RelativeColumn(2); // Document Name
                        columns.RelativeColumn(2); // Reference
                        columns.RelativeColumn(1); // Debit
                        columns.RelativeColumn(1); // Credit
                        columns.RelativeColumn(1); // Total
                    });

                    // Header row
                    table.Header(header =>
                    {
                        header.Cell().Element(CellStyle).Text("Date").AlignCenter().Bold();
                        header.Cell().Element(CellStyle).Text("Document").AlignCenter().Bold();
                        header.Cell().Element(CellStyle).Text("Reference").AlignCenter().Bold();
                        header.Cell().Element(CellStyle).Text("Debit").AlignCenter().Bold();
                        header.Cell().Element(CellStyle).Text("Credit").AlignCenter().Bold();
                        header.Cell().Element(CellStyle).Text("Total").AlignCenter().Bold();
                    });

                    // Data rows
                    foreach (var movement in movements)
                    {
                        table.Cell().Element(CellStyle).Text(movement.TransDate.ToShortDateString());
                        table.Cell().Element(CellStyle).Text(movement.DocSeriesCode);
                        table.Cell().Element(CellStyle).Text(movement.RefCode);
                        table.Cell().Element(CellStyle).AlignRight().Text($"{movement.Debit:C}");
                        table.Cell().Element(CellStyle).AlignRight().Text($"{movement.Credit:C}");
                        table.Cell().Element(CellStyle).AlignRight().Text($"{movement.RunningTotal:C}");
                    }

                    IContainer CellStyle(IContainer cont) =>
                        cont.PaddingVertical(5).PaddingHorizontal(2);
                });
                page.Footer().Row(row =>
                {
                    row.RelativeItem().AlignLeft().Text($"Generated: {DateTime.Now:g}").FontSize(9);
                    row.RelativeItem().AlignRight().Text(text =>
                    {
                        text.Span("Page ").FontSize(9);
                        text.CurrentPageNumber().Bold();
                        text.Span(" of ");
                        text.TotalPages().Bold();
                    });
                });
            });
        });

        return document.GeneratePdf();
    }
}