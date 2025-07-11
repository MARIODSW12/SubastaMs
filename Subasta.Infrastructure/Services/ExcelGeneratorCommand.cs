using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Subasta.Infrastructure.Dtos;

namespace Subasta.Infrastructure.Services
{
    public class ExcelGeneratorCommand: IRequest<byte[]>
    {
        public UserInfoDto UserInfo { get; set; }
        public ExportDataDto ExcelData { get; set; }
        public ExcelGeneratorCommand(UserInfoDto userInfo, ExportDataDto excelData)
        {
            UserInfo = userInfo ?? throw new ArgumentNullException(nameof(userInfo));
            ExcelData = excelData ?? throw new ArgumentNullException(nameof(excelData));
        }

    }
}
