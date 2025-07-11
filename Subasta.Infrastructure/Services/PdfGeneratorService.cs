using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.IO.Font.Constants;
using MediatR;
using iText.Kernel.Pdf.Canvas.Draw;
using iText.Layout.Splitting;
using System.IO;

namespace Subasta.Infrastructure.Services
{
    public class PdfGeneratorService : IRequestHandler<PdfGeneratorCommand, byte[]>
    {

        public async Task<byte[]> Handle(PdfGeneratorCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var memoryStream = new MemoryStream();

                var writer = new PdfWriter(memoryStream);
                var pdf = new PdfDocument(writer);
                var document = new Document(pdf);
                var bodyFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

                document.SetFont(bodyFont);
                var userName = new Paragraph(request.UserInfo.Name)
                    .SetFontSize(12)
                    .SetFontColor(ColorConstants.BLACK)
                    .SetTextAlignment(TextAlignment.LEFT)
                    .SetMarginBottom(5);
                document.Add(userName);

                var userEmail = new Paragraph(request.UserInfo.Email)
                    .SetFontSize(12)
                    .SetFontColor(ColorConstants.BLACK)
                    .SetTextAlignment(TextAlignment.LEFT)
                    .SetMarginBottom(5);
                document.Add(userEmail);

                var userPhone = new Paragraph(request.UserInfo.Phone)
                    .SetFontSize(12)
                    .SetFontColor(ColorConstants.BLACK)
                    .SetTextAlignment(TextAlignment.LEFT)
                    .SetMarginBottom(5);
                document.Add(userPhone);

                var userDirection = new Paragraph(request.UserInfo.Address)
                    .SetFontSize(12)
                    .SetFontColor(ColorConstants.BLACK)
                    .SetTextAlignment(TextAlignment.LEFT)
                    .SetMarginBottom(5);
                document.Add(userDirection);

                var separator = new LineSeparator(new SolidLine())
                    .SetMarginBottom(10);
                document.Add(separator);

                var titleParagraph = new Paragraph(request.PdfData.title)
                    .SetFontSize(24)
                    .SetFontColor(ColorConstants.DARK_GRAY)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetMarginBottom(15);
                document.Add(titleParagraph);
                if (request.PdfData.groupBy == null)
                    return await CreateNormalPdf(document, request, memoryStream);
                else
                    return await CreateGroupedPdf(document, request, memoryStream);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw ex;
            }
        }

        async private Task<byte[]> CreateNormalPdf(Document document, PdfGeneratorCommand request, MemoryStream memoryStream)
        {

            var table = new Table(request.PdfData.rowTitles.Count);
            table.SetWidth(UnitValue.CreatePercentValue(100));
            table.SetKeepTogether(false);
            table.SetSplitCharacters(new DefaultSplitCharacters());

            table.SetSkipFirstHeader(false);
            table.SetSkipLastFooter(false);

            foreach (var rowTitle in request.PdfData.rowTitles)
            {
                table.AddHeaderCell(new Cell().Add(new Paragraph(rowTitle).SetFontSize(10)));
            }
            foreach (var row in request.PdfData.rows)
            {
                foreach (var cell in row)
                {
                    table.AddCell(new Cell().Add(new Paragraph(cell).SetFontSize(10)));
                }
            }

            document.Add(table);

            var footerParagraph = new Paragraph($"\n\nReporte generado el: {DateTime.Now:yyyy-MM-dd HH:mm:ss}\nEn conformidad con el ART 1071 del Código Civil Venezolano")
                .SetFontSize(8)
                .SetFontColor(ColorConstants.GRAY)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginTop(1);

            document.Add(footerParagraph);

            document.Close();

            var pdfBytes = memoryStream.ToArray();
            memoryStream.Dispose();
            return (pdfBytes);
        }

        async private Task<byte[]> CreateGroupedPdf(Document document, PdfGeneratorCommand request, MemoryStream memoryStream)
        {
            
            var groupColumn = request.PdfData.rowTitles.FindIndex(t => t == request.PdfData.groupBy);

            if (groupColumn < 0)
            {
                throw new ArgumentException($"La columna '{request.PdfData.groupBy}' no se encuentra en los títulos de fila proporcionados.");
            }

            var groupTitles = request.PdfData.rows
                .Select(row => row[groupColumn])
                .Distinct()
                .OrderBy(title => title)
                .ToList();
            var newDataTitles = request.PdfData.rowTitles
                .Where(title => title != request.PdfData.groupBy)
                .ToList();
            foreach (var groupTitle in groupTitles)
            {
               var title = new Paragraph(groupTitle)
                    .SetFontSize(16)
                    .SetFontColor(ColorConstants.DARK_GRAY)
                    .SetTextAlignment(TextAlignment.LEFT)
                    .SetMarginBottom(10)
                    .SetMarginTop(15);
                document.Add(title);

                var table = new Table(newDataTitles.Count);
                table.SetWidth(UnitValue.CreatePercentValue(100));
                table.SetKeepTogether(false);
                table.SetSplitCharacters(new DefaultSplitCharacters());

                table.SetSkipFirstHeader(false);
                table.SetSkipLastFooter(false);
                var groupData = request.PdfData.rows
                    .Where(row => row[groupColumn] == groupTitle)
                    .Select(row => row.Where((cell, index) => index != groupColumn).ToList())
                    .ToList();
                foreach (var rowTitle in newDataTitles)
                {
                    table.AddHeaderCell(new Cell().Add(new Paragraph(rowTitle).SetFontSize(10)));
                }
                foreach (var row in groupData)
                {
                    foreach (var cell in row)
                    {
                        table.AddCell(new Cell().Add(new Paragraph(cell).SetFontSize(10)));
                    }
                }

                document.Add(table);
            }

            var footerParagraph = new Paragraph($"\n\nReporte generado el: {DateTime.Now:yyyy-MM-dd HH:mm:ss}\nEn conformidad con el ART 1071 del Código Civil Venezolano")
                .SetFontSize(8)
                .SetFontColor(ColorConstants.GRAY)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginTop(1);

            document.Add(footerParagraph);

            document.Close();

            var pdfBytes = memoryStream.ToArray();
            memoryStream.Dispose();
            return (pdfBytes);
        }
    }
}
