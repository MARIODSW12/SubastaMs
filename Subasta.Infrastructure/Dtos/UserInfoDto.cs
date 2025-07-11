using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Subasta.Infrastructure.Dtos
{
    public class UserInfoDto
    {
        public string Id { get; init; }
        public string Name { get; init; }
        public string Email { get; init; }
        public string Phone { get; init; }
        public string Address { get; init; }

    }
}
