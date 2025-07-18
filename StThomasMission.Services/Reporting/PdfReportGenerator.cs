using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using StThomasMission.Core.DTOs;
using StThomasMission.Core.Enums;
using StThomasMission.Services.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace StThomasMission.Services.Reporting
{
    public class PdfReportGenerator<T> : IReportGenerator<T> where T : class
    {
        public PdfReportGenerator()
        {
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public Task<byte[]> GenerateReportAsync(ReportFormat format, IEnumerable<T> data, string reportTitle)
        {
            if (format != ReportFormat.PDF)
            {
                throw new System.ArgumentException("Invalid format specified for PDF generator.", nameof(format));
            }

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(1, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header()
                        .Text(reportTitle)
                        .SemiBold().FontSize(16).FontColor(Colors.Blue.Medium);

                    page.Content()
                        .PaddingVertical(1, Unit.Centimetre)
                        .Table(table =>
                        {
                            var headers = typeof(T).GetProperties().Select(p => p.Name).ToArray();

                            table.ColumnsDefinition(columns =>
                            {
                                foreach (var _ in headers) columns.RelativeColumn();
                            });

                            table.Header(header =>
                            {
                                foreach (var text in headers)
                                {
                                    header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text(text).Bold();
                                }
                            });

                            foreach (var item in data)
                            {
                                foreach (var prop in typeof(T).GetProperties())
                                {
                                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                                        .Text(prop.GetValue(item)?.ToString() ?? string.Empty);
                                }
                            }
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text(x =>
                        {
                            x.Span("Page ");
                            x.CurrentPageNumber();
                        });
                });
            });

            return Task.FromResult(document.GeneratePdf());
        }
    }
}