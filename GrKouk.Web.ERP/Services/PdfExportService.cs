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
    public byte[] GenerateFinancialPdf(List<KartelaLine> movements)
    {
        QuestPDF.Settings.License = LicenseType.Community; 
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(10));
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
                        header.Cell().Element(CellStyle).Text("Date");
                        header.Cell().Element(CellStyle).Text("Document");
                        header.Cell().Element(CellStyle).Text("Reference");
                        header.Cell().Element(CellStyle).Text("Debit");
                        header.Cell().Element(CellStyle).Text("Credit");
                        header.Cell().Element(CellStyle).Text("Total");
                    });

                    // Data rows
                    foreach (var movement in movements)
                    {
                        table.Cell().Element(CellStyle).Text(movement.TransDate.ToShortDateString());
                        table.Cell().Element(CellStyle).Text(movement.DocSeriesCode);
                        table.Cell().Element(CellStyle).Text(movement.RefCode);
                        table.Cell().Element(CellStyle).Text($"{movement.Debit:C}");
                        table.Cell().Element(CellStyle).Text($"{movement.Credit:C}");
                        table.Cell().Element(CellStyle).Text($"{movement.RunningTotal:C}");
                    }

                    IContainer CellStyle(IContainer container) =>
                        container.Border(1).Padding(5);
                });
            });
        });

        return document.GeneratePdf();
    }
}