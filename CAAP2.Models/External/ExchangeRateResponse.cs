using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAAP2.Models.External
{
    public class ExchangeRateResponse
    {
        public decimal? Compra { get; set; }
        public decimal? Venta { get; set; }
        public string Fecha { get; set; }
    }
}

