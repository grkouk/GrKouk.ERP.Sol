using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using GrKouk.Erp.Domain.DocDefinitions;
using GrKouk.Web.ERP.Helpers;
using Microsoft.AspNetCore.Hosting;
using Syncfusion.Drawing;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;
using Syncfusion.Pdf.Grid;

namespace GrKouk.Web.ERP.Services;

// using QuestPDF.Fluent;
// using QuestPDF.Helpers;
// using QuestPDF.Infrastructure;
using QuestPDF.Drawing;

public class PdfExportService
{
    private readonly IWebHostEnvironment _webHostEnvironment;

    public PdfExportService(IWebHostEnvironment webHostEnvironment
    )
    {
        _webHostEnvironment = webHostEnvironment;
    }
    // public byte[] GenerateFinancialPdf(List<KartelaLine> movements, string reportTitle)
    // {
    //     QuestPDF.Settings.License = LicenseType.Community;
    //     var document = Document.Create(container =>
    //     {
    //         container.Page(page =>
    //         {
    //             page.Size(PageSizes.A4);
    //             page.Margin(30);
    //             page.DefaultTextStyle(x => x.FontSize(8));
    //             page.Header().Row(row =>
    //             {
    //                 // Left: Logo
    //                 // row.RelativeColumn().Image(logoPath, ImageScaling.FitHeight).Height(50);
    //
    //                 // Right: Title
    //                 row.ConstantItem(500).AlignCenter().Text(reportTitle)
    //                     .Bold().FontSize(12);
    //                 row.Spacing(20);
    //             });
    //             page.Content().Table(table =>
    //             {
    //                 table.ColumnsDefinition(columns =>
    //                 {
    //                     columns.RelativeColumn(1); // Date
    //                     columns.RelativeColumn(2); // Document Name
    //                     columns.RelativeColumn(2); // Reference
    //                     columns.RelativeColumn(1); // Debit
    //                     columns.RelativeColumn(1); // Credit
    //                     columns.RelativeColumn(1); // Total
    //                 });
    //
    //                 // Header row
    //                 table.Header(header =>
    //                 {
    //                     header.Cell().Element(CellStyle).Text("Date").AlignCenter().Bold();
    //                     header.Cell().Element(CellStyle).Text("Document").AlignCenter().Bold();
    //                     header.Cell().Element(CellStyle).Text("Reference").AlignCenter().Bold();
    //                     header.Cell().Element(CellStyle).Text("Debit").AlignCenter().Bold();
    //                     header.Cell().Element(CellStyle).Text("Credit").AlignCenter().Bold();
    //                     header.Cell().Element(CellStyle).Text("Total").AlignCenter().Bold();
    //                 });
    //
    //                 // Data rows
    //                 foreach (var movement in movements)
    //                 {
    //                     table.Cell().Element(CellStyle).Text(movement.TransDate.ToShortDateString());
    //                     table.Cell().Element(CellStyle).Text(movement.DocSeriesName);
    //                     table.Cell().Element(CellStyle).Text(movement.RefCode);
    //                     table.Cell().Element(CellStyle).AlignRight().Text($"{movement.Debit:C}");
    //                     table.Cell().Element(CellStyle).AlignRight().Text($"{movement.Credit:C}");
    //                     table.Cell().Element(CellStyle).AlignRight().Text($"{movement.RunningTotal:C}");
    //                 }
    //
    //                 IContainer CellStyle(IContainer cont) =>
    //                     cont.PaddingVertical(5).PaddingHorizontal(2);
    //             });
    //             page.Footer().Row(row =>
    //             {
    //                 row.RelativeItem().AlignLeft().Text($"Generated: {DateTime.Now:g}").FontSize(9);
    //                 row.RelativeItem().AlignRight().Text(text =>
    //                 {
    //                     text.Span("Page ").FontSize(9);
    //                     text.CurrentPageNumber().Bold();
    //                     text.Span(" of ");
    //                     text.TotalPages().Bold();
    //                 });
    //             });
    //         });
    //     });
    //     //throw new Exception("Test error from pdf export");
    //     return document.GeneratePdf();
    // }


    public byte[] GenerateTransactionPdf(List<KartelaLine> items, string reportTitle)
    {
        try
        {
            using (PdfDocument document = new PdfDocument())
            {
                document.PageSettings.Size = PdfPageSize.A4;
                // Load a Unicode-compatible TrueType font from embedded resource
                PdfFont titleFont;
                ;
                PdfFont bodyFont;
                ;
                PdfFont bodyTitleFont;
                PdfFont footerFont;
                string fontFilePath = Path.Combine(_webHostEnvironment.WebRootPath, "fonts", "NotoSans-Regular.ttf");
                if (!File.Exists(fontFilePath))
                {
                    throw new FileNotFoundException(
                        $"The file 'NotoSans-Regular.ttf' was not found in 'wwwroot/fonts'.");
                }

                FileStream fontStream = new FileStream(fontFilePath, FileMode.Open, FileAccess.Read);

                PdfTrueTypeFont titleFont1 = new PdfTrueTypeFont(fontStream, 14, PdfFontStyle.Bold);
                ;
                PdfTrueTypeFont bodyFont1 = new PdfTrueTypeFont(fontStream, 8);
                PdfTrueTypeFont bodyTitleFont1 = new PdfTrueTypeFont(fontStream, 8, PdfFontStyle.Bold);
                PdfTrueTypeFont footerFont1 = new PdfTrueTypeFont(fontStream, 8);
                titleFont = titleFont1;
                bodyFont = bodyFont1;
                bodyTitleFont = bodyTitleFont1;
                footerFont = footerFont1;

                // Add a page
                PdfPage page = document.Pages.Add();
                RectangleF bounds = new RectangleF(0, 0, document.Pages[0].GetClientSize().Width, 50);
                // Create header template
                PdfPageTemplateElement header = new PdfPageTemplateElement(bounds);
                header.Graphics.DrawString(reportTitle, titleFont, PdfBrushes.Black, new PointF(0, 10));

                // Apply header to all pages
                document.Template.Top = header;
                PdfPageTemplateElement footer = new PdfPageTemplateElement(document.PageSettings.Width, 40);

                string currentDate = $"Generated: {DateTime.Now:g}";
                footer.Graphics.DrawString(currentDate, footerFont, PdfBrushes.Black, new PointF(0, 20));

                // Draw pagination on the right
                PdfPageNumberField pageNumber = new PdfPageNumberField
                {
                    Font = footerFont,
                    Brush = PdfBrushes.Black
                };
                PdfPageCountField pageCount = new PdfPageCountField
                {
                    Font = footerFont,
                    Brush = PdfBrushes.Black
                };
                PdfCompositeField pagination = new PdfCompositeField(footerFont, PdfBrushes.Black, "Page {0} of {1}",
                    pageNumber, pageCount);
               
                SizeF pageSize = document.PageSettings.Size;
                pagination.Draw(footer.Graphics, new PointF(pageSize.Width - 150, 20));

                // Apply footer to all pages
                document.Template.Bottom = footer;

                PdfGraphics graphics = page.Graphics;

                // Calculate Totals
                decimal totalDebit = items.Sum(item => item.Debit);
                decimal totalCredit = items.Sum(item => item.Credit);

                // Create PDF grid
                PdfGrid pdfGrid = new PdfGrid();

                // Create data source
                List<object> data = new List<object>();
                foreach (var item in items)
                {
                    data.Add(new
                    {
                        TransactionDate = item.TransDate.ToString("dd-MM-yyyy"),
                        DocumentName = item.DocSeriesName,
                        ReferenceNumber = item.RefCode,
                        Debit = item.Debit.ToString("C"),
                        Credit = item.Credit.ToString("C"),
                        RunningTotal = item.RunningTotal.ToString("C"),
                        CompanyCode = item.CompanyCode,
                    });
                }

                // Assign data source
                pdfGrid.DataSource = data;
                // Set custom column widths
                pdfGrid.Columns[0].Width = 60;
                pdfGrid.Columns[1].Width = 110;
                pdfGrid.Columns[2].Width = 70; // ReferenceNumber (wider)
                pdfGrid.Columns[3].Width = 70; // Debit (narrow)
                pdfGrid.Columns[4].Width = 70; // Credit (narrow)
                pdfGrid.Columns[5].Width = 70; // RunningTotal (narrow)
                pdfGrid.Columns[6].Width = 65; // Company (narrow)
                
                // Customize header text
                PdfGridRow rowHeader = pdfGrid.Headers[0];
                rowHeader.Cells[0].Value = "Ημ/νία";
                rowHeader.Cells[1].Value = "Παραστατικό";
                rowHeader.Cells[2].Value = "Αρ.Παρ.";
                rowHeader.Cells[3].Value = "Χρέωση";
                rowHeader.Cells[4].Value = "Πίστωση";
                rowHeader.Cells[5].Value = "Υπόλοιπο";
                rowHeader.Cells[6].Value = "Εταιρεία";

                // Apply header style
                PdfGridCellStyle headerStyle = new PdfGridCellStyle
                {
                    BackgroundBrush = PdfBrushes.LightGray,
                    StringFormat = new PdfStringFormat(PdfTextAlignment.Center),
                    Font = bodyTitleFont,
                };
                for (int i = 0; i < rowHeader.Cells.Count; i++)
                {
                    rowHeader.Cells[i].Style = headerStyle;
                }
                // Apply default style first
                PdfGridCellStyle defaultCellStyle = new PdfGridCellStyle
                {
                    Font = bodyFont,
                    StringFormat = new PdfStringFormat(PdfTextAlignment.Left) // Default alignment
                };

                // Apply cell style with right alignment for Debit and Credit columns
                PdfGridCellStyle numberCellStyle = new PdfGridCellStyle
                {
                    Font = bodyFont,
                    StringFormat = new PdfStringFormat(PdfTextAlignment.Right)
                };
                foreach (PdfGridRow row in pdfGrid.Rows)
                {
                    for(int i=0; i< row.Cells.Count; i++) {
                        row.Cells[i].Style = defaultCellStyle; // Apply default first
                    }

                    row.Cells[3].Style = numberCellStyle; // Debit
                    row.Cells[4].Style = numberCellStyle; // Credit
                    row.Cells[5].Style = numberCellStyle; // Running Total
                }

                // Customize grid style
                PdfGridStyle gridStyle = new PdfGridStyle
                {
                    CellPadding = new PdfPaddings(5, 5, 5, 5),
                    BackgroundBrush = PdfBrushes.White,
                    TextBrush = PdfBrushes.Black,
                    Font = bodyFont,
                };
                pdfGrid.Style = gridStyle;
                pdfGrid.RepeatHeader = true;

                // Draw grid on the page
               // pdfGrid.Draw(page, new PointF(0, 50));
               // Draw the main grid on the page and get layout result
                // Start drawing below the header area
                PdfGridLayoutFormat layoutFormat = new PdfGridLayoutFormat()
                    { Layout = PdfLayoutType.Paginate };
                PdfLayoutResult result = pdfGrid.Draw(page, new PointF(0, 50), layoutFormat); // Use Y=50 to leave space for header

                // *** START: Add Totals Grid ***
                PdfGrid totalsGrid = new PdfGrid();
                totalsGrid.Columns.Add(7); // Add 6 columns to match main grid

                // Set column widths for totals grid to match main grid
                totalsGrid.Columns[0].Width = pdfGrid.Columns[0].Width;
                totalsGrid.Columns[1].Width = pdfGrid.Columns[1].Width;
                totalsGrid.Columns[2].Width = pdfGrid.Columns[2].Width;
                totalsGrid.Columns[3].Width = pdfGrid.Columns[3].Width;
                totalsGrid.Columns[4].Width = pdfGrid.Columns[4].Width;
                totalsGrid.Columns[5].Width = pdfGrid.Columns[5].Width;
                totalsGrid.Columns[6].Width = pdfGrid.Columns[6].Width;

                // Add totals row
                PdfGridRow totalsRow = totalsGrid.Rows.Add();

                // Set cell values for totals row
                totalsRow.Cells[0].Value = "";
                totalsRow.Cells[1].Value = "";
                totalsRow.Cells[2].Value = "Totals:";
                totalsRow.Cells[3].Value = totalDebit.ToString("C");
                totalsRow.Cells[4].Value = totalCredit.ToString("C");
                totalsRow.Cells[5].Value = ""; // No total for running total column
                totalsRow.Cells[6].Value = ""; // No total for company column

                // Style the totals row
                PdfGridCellStyle totalsLabelStyle = new PdfGridCellStyle
                {
                    Font = bodyTitleFont, // Use bold font for label
                    StringFormat = new PdfStringFormat(PdfTextAlignment.Right) // Align label right in its cell
                };
                 PdfGridCellStyle totalsNumberStyle = new PdfGridCellStyle
                {
                    Font = bodyTitleFont, // Use bold font for totals
                    StringFormat = new PdfStringFormat(PdfTextAlignment.Right) // Align numbers right
                };
                 totalsRow.Cells[2].Style = totalsLabelStyle;
                 totalsRow.Cells[3].Style = totalsNumberStyle;
                 totalsRow.Cells[4].Style = totalsNumberStyle;

                // Apply basic grid style (optional, for borders/padding)
                totalsGrid.Style.CellPadding = gridStyle.CellPadding; // Use same padding
                // totalsGrid.Style.BorderOverlapStyle = PdfBorderOverlapStyle.Overlap; // If you want borders

                // Draw the totals grid below the main grid result
                // Use the result.Page and result.Bounds.Bottom for positioning
                 float totalsY = result.Bounds.Bottom + 10; // Add 10 points padding
                 totalsGrid.Draw(result.Page, new PointF(0, totalsY));
                // *** END: Add Totals Grid ***

                // Save to memory stream
                using (MemoryStream stream = new MemoryStream())
                {
                    document.Save(stream);
                    return stream.ToArray();
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
            throw new Exception("Error generating PDF", ex);
        }
    }
}