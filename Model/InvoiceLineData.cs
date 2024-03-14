using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneradorCufe.Model
{
    public class InvoiceLineData
    {
        public string InvoiceLineID { get; set; }
        public string InvoiceLineInvoicedQuantity { get; set; }
        public string InvoiceLineLineExtensionAmount { get; set; }
        public string InvoiceLineTaxAmount { get; set; }
        public string InvoiceLineTaxableAmount { get; set; }
        public string InvoiceLinePercent { get; set; }
        public string ItemDescription { get; set; }
        public string ItemID { get; set; }
        public string PriceCurrencyID { get; set; }
        public string PricePriceAmount { get; set; }
        public string PriceBaseUnitCode { get; set; }
        public string PriceBaseQuantity { get; set; }
    }
}
