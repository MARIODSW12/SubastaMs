using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Subasta.Infrastructure.Dtos
{
    public class ExportDataDto
    {
        public List<string> rowTitles { get; init; }
        public string title { get; init; }
        public List<List<string>> rows { get; init; }
        public string? groupBy { get; init; }

    }
}
