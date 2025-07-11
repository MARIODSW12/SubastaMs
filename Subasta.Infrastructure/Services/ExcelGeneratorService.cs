
using Hangfire.MemoryStorage.Database;
using MediatR;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using OfficeOpenXml.Table;
using System.Drawing;

namespace Subasta.Infrastructure.Services
{
    public class ExcelGeneratorService : IRequestHandler<ExcelGeneratorCommand, byte[]>
    {

        public async Task<byte[]> Handle(ExcelGeneratorCommand request, CancellationToken cancellationToken)
        {
            try
            {

                ExcelPackage.License.SetNonCommercialPersonal("Samuel Alonso");

                using var package = new ExcelPackage();

                var worksheet = package.Workbook.Worksheets.Add(request.ExcelData.title);

                worksheet.Cells["A1"].Value = request.ExcelData.title;
                worksheet.Cells["A1:B1"].Merge = true;
                worksheet.Cells["A1"].Style.Font.Size = 18;
                worksheet.Cells["A1"].Style.Font.Bold = true;
                worksheet.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                worksheet.Cells["A2"].Value = $"Generated on: {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
                worksheet.Cells["A2"].Style.Font.Size = 10;

                worksheet.Cells["A3"].Value = request.UserInfo.Name;
                worksheet.Cells["A3"].Style.Font.Size = 10;
                worksheet.Cells["B3"].Value = request.UserInfo.Email;
                worksheet.Cells["B3"].Style.Font.Size = 10;
                worksheet.Cells["C3"].Value = request.UserInfo.Phone;
                worksheet.Cells["C3"].Style.Font.Size = 10;
                worksheet.Cells["D3"].Value = request.UserInfo.Address;
                worksheet.Cells["D3"].Style.Font.Size = 10;

                if (request.ExcelData.groupBy == null)
                    await CreateNormalExcel(worksheet, request);
                else
                    await CreateGroupedExcel(worksheet, request);

                var result = package.GetAsByteArray();
                return result;

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw ex;
            }
        }

        async private Task CreateNormalExcel(ExcelWorksheet worksheet, ExcelGeneratorCommand request)
        {
            var startRow = 5;
            for (int i = 0; i < request.ExcelData.rowTitles.Count; i++)
            {
                var cell = worksheet.Cells[startRow, i + 1];
                cell.Value = request.ExcelData.rowTitles[i];
                cell.Style.Font.Bold = true;
                cell.Style.Border.BorderAround(ExcelBorderStyle.Thin);
            }
            for(var i= 0; i < request.ExcelData.rows.Count; i++)
            {
                var row = startRow + 1 + i;
                for (var j = 0; j < request.ExcelData.rows[i].Count; j++)
                {
                    worksheet.Cells[row, j + 1].Value = request.ExcelData.rows[i][j];
                }
            }

            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
           
            await Task.CompletedTask;
        }

        async private Task CreateGroupedExcel(ExcelWorksheet worksheet, ExcelGeneratorCommand request)
        {
            
            var groupColumn = request.ExcelData.rowTitles.FindIndex(t => t == request.ExcelData.groupBy);

            if (groupColumn < 0)
            {
                throw new ArgumentException($"La columna '{request.ExcelData.groupBy}' no se encuentra en los títulos de fila proporcionados.");
            }

            var groupTitles = request.ExcelData.rows
                .Select(row => row[groupColumn])
                .Distinct()
                .OrderBy(title => title)
                .ToList();
            var newDataTitles = request.ExcelData.rowTitles
                .Where(title => title != request.ExcelData.groupBy)
                .ToList();

            var startRow = 5;
            for (var i = 0; i < groupTitles.Count; i++)
            {
                var cell = worksheet.Cells[startRow, 1];
                cell.Value = groupTitles[i];
                cell.Style.Font.Bold = true;
                
                var groupData = request.ExcelData.rows
                    .Where(row => row[groupColumn] == groupTitles[i])
                    .Select(row => row.Where((cell, index) => index != groupColumn).ToList())
                    .ToList();
                startRow++;
                for (int k = 0; k < newDataTitles.Count; k++)
                {
                    var cell2 = worksheet.Cells[startRow, k + 1];
                    cell2.Value = newDataTitles[k];
                    cell2.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                }
                startRow++;
                for (var k = 0; k < groupData.Count; k++)
                {
                    for (var j = 0; j < groupData[k].Count; j++)
                    {
                        worksheet.Cells[startRow, j + 1].Value = request.ExcelData.rows[k][j];
                    }
                    startRow++;
                }
                startRow++;
            }
            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
            await Task.CompletedTask;
        }
    }
}
