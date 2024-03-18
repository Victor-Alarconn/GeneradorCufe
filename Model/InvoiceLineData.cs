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
        public string TaxSchemeID { get; set; }
        public string TaxSchemeName { get; set; }
        public string ItemDescription { get; set; }
        public string ItemID { get; set; }
        public string PriceCurrencyID { get; set; }
        public string PricePriceAmount { get; set; }
        public string PriceBaseUnitCode { get; set; }
        public string PriceBaseQuantity { get; set; }
    }

    public class FacturaElectronica
    {
        public List<InvoiceLineData> ObtenerProductos()
        {
            var listaProductos = new List<InvoiceLineData>();

            // Crear los objetos InvoiceLineData para cada producto
            listaProductos.Add(new InvoiceLineData
            {
                InvoiceLineID = "1",
                InvoiceLineInvoicedQuantity = "1.00",
                InvoiceLineLineExtensionAmount = "100000.00",
                InvoiceLineTaxAmount = "19000.00",
                InvoiceLineTaxableAmount = "100000.00",
                InvoiceLinePercent = "19.00",
                TaxSchemeID = "01",
                TaxSchemeName ="IVA",
                ItemDescription = "Frambuesas",
                ItemID = "900770401-8",
                PriceCurrencyID = "COP",
                PricePriceAmount = "100000.00",
                PriceBaseUnitCode = "EA",
                PriceBaseQuantity = "1.00"
            });

            listaProductos.Add(new InvoiceLineData
            {
                InvoiceLineID = "2",
                InvoiceLineInvoicedQuantity = "1.00",
                InvoiceLineLineExtensionAmount = "214285.72",
                InvoiceLineTaxAmount = "10714.28",
                InvoiceLineTaxableAmount = "214285.72",
                InvoiceLinePercent = "5.00",
                TaxSchemeID = "01",
                TaxSchemeName = "IVA",
                ItemDescription = "manzanas",
                ItemID = "900770401-8",
                PriceCurrencyID = "COP",
                PricePriceAmount = "214285.72",
                PriceBaseUnitCode = "EA",
                PriceBaseQuantity = "1.00"
            });

            listaProductos.Add(new InvoiceLineData
            {
                InvoiceLineID = "3",
                InvoiceLineInvoicedQuantity = "1.00",
                InvoiceLineLineExtensionAmount = "160301.72",
                InvoiceLineTaxAmount = "25648.28",
                InvoiceLineTaxableAmount = "160301.72",
                InvoiceLinePercent = "16.00",
                TaxSchemeID = "01",
                TaxSchemeName = "IVA",
                ItemDescription = "peras",
                ItemID = "900770401-8",
                PriceCurrencyID = "COP",
                PricePriceAmount = "160301.72",
                PriceBaseUnitCode = "EA",
                PriceBaseQuantity = "1.00"
            });

            return listaProductos;
        }
    }
}
