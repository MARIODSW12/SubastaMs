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
    public class PdfGeneratorCommand: IRequest<byte[]>
    {
        public UserInfoDto UserInfo { get; set; }
        public ExportDataDto PdfData { get; set; }
        public PdfGeneratorCommand(UserInfoDto userInfo, ExportDataDto pdfData)
        {
            UserInfo = userInfo ?? throw new ArgumentNullException(nameof(userInfo));
            PdfData = pdfData ?? throw new ArgumentNullException(nameof(pdfData));
        }

    }
}
