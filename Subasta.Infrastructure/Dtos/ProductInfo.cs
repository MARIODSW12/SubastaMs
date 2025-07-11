using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Subasta.Infrastructure.Dtos
{
    public class ProductInfo
    {
        public string idProducto { get; init;}
        public string nombreProducto { get; init;}
        public string categoria { get; init;}
        public decimal precioBase { get; init;}
        public string descripcion { get; init;}
        public string imagenProducto { get; init;}
        public string idSubastador { get; init;}
        public int cantidadProducto { get; init; }
    }
}
