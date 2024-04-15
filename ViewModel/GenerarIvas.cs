using GeneradorCufe.Model;
using Org.BouncyCastle.Crypto.Macs;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace GeneradorCufe.ViewModel
{
    public  class GenerarIvas
    {

        public static void GenerarIvasYAgregarElementos(XDocument xmlDoc, IEnumerable<Productos> listaProductos)
        {
            XNamespace cbc = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2";
            XNamespace cac = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2";

            decimal totalImpuesto = Math.Round(listaProductos.Sum(p => p.IvaTotal), 2);
            decimal consumo = Math.Round(listaProductos.Sum(p => p.Consumo), 2);
            decimal totalBaseImponible = Math.Round(listaProductos.Sum(p => p.Neto), 2);
            bool hayProductosConIPO = listaProductos.Any(p => p.Iva == 0 && p.Consumo > 0);
            bool hayProductosConIVA = listaProductos.Any(p => p.Iva > 0);
            bool hayProductosSinIVA = listaProductos.Any(p => p.Iva == 0 && p.Consumo == 0);

            var taxTotalElement = xmlDoc.Descendants(cac + "TaxTotal").FirstOrDefault();
            if (taxTotalElement != null)
            {
                if (hayProductosConIPO)
                {
                    taxTotalElement.Element(cbc + "TaxAmount")?.SetValue(consumo.ToString("F2", CultureInfo.InvariantCulture));
                    var taxSubtotalElement = new XElement(cac + "TaxSubtotal");
                    taxSubtotalElement.Add(new XElement(cbc + "TaxableAmount", totalBaseImponible.ToString("F2", CultureInfo.InvariantCulture)));
                    taxSubtotalElement.Element(cbc + "TaxableAmount")?.SetAttributeValue("currencyID", "COP");
                    taxSubtotalElement.Add(new XElement(cbc + "TaxAmount", consumo.ToString("F2", CultureInfo.InvariantCulture)));
                    taxSubtotalElement.Element(cbc + "TaxAmount")?.SetAttributeValue("currencyID", "COP");
                    var taxCategoryElement = new XElement(cac + "TaxCategory");
                    taxCategoryElement.Add(new XElement(cbc + "Percent", "8.00"));
                    var taxSchemeElement = new XElement(cac + "TaxScheme");
                    taxSchemeElement.Add(new XElement(cbc + "ID", "04"));
                    taxSchemeElement.Add(new XElement(cbc + "Name", "INC"));
                    taxCategoryElement.Add(taxSchemeElement);
                    taxSubtotalElement.Add(taxCategoryElement);
                    taxTotalElement.Add(taxSubtotalElement);
                }

                if (hayProductosConIVA || hayProductosSinIVA)
                {
                    taxTotalElement.Element(cbc + "TaxAmount")?.SetValue(totalImpuesto.ToString("F2", CultureInfo.InvariantCulture));

                    if (hayProductosConIVA)
                    {
                        var gruposIVA = listaProductos.Where(p => p.Iva > 0).GroupBy(p => p.Iva);
                        foreach (var grupo in gruposIVA)
                        {
                            decimal totalImpuestoGrupo = Math.Round(grupo.Sum(p => p.IvaTotal), 2);
                            string porcentajeIVAFormateado = grupo.Key.ToString("F2", CultureInfo.InvariantCulture);
                            var taxSubtotalElementConIVA = GenerarElementoTaxSubtotal(xmlDoc, porcentajeIVAFormateado, grupo.ToList());
                            taxTotalElement.Add(taxSubtotalElementConIVA);
                        }
                    }

                    if (hayProductosSinIVA)
                    {
                        var taxSubtotalElementSinIVA = GenerarElementoTaxSubtotal(xmlDoc, "0.00", listaProductos.Where(p => p.Iva == 0).ToList());
                        taxTotalElement.Add(taxSubtotalElementSinIVA);
                    }
                }

                var taxSubtotalTemplate = xmlDoc.Descendants(cac + "TaxSubtotal").FirstOrDefault();
                taxSubtotalTemplate?.Remove();
            }
        }


        private static XElement GenerarElementoTaxSubtotal(XDocument xmlDoc, string porcentajeIVA, List<Productos> productos)
        {
            XNamespace sts = "dian:gov:co:facturaelectronica:Structures-2-1";
            XNamespace cbc = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2";
            XNamespace cac = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2";

            // Crear un nuevo elemento TaxSubtotal
            var taxSubtotalElement = new XElement(cac + "TaxSubtotal");

            // Calcular el total de la base imponible para los productos dados
            decimal totalBaseImponible = productos.Sum(p => p.Neto);

            // Establecer los valores calculados en el elemento TaxSubtotal
            taxSubtotalElement.Add(new XElement(cbc + "TaxableAmount", totalBaseImponible.ToString("F2", CultureInfo.InvariantCulture)));
            taxSubtotalElement.Element(cbc + "TaxableAmount")?.SetAttributeValue("currencyID", "COP");

            // Crear y establecer los elementos TaxAmount, Percent, ID y Name
            taxSubtotalElement.Add(new XElement(cbc + "TaxAmount", "0.00")); // Inicialmente establecemos el impuesto en 0.00
            taxSubtotalElement.Element(cbc + "TaxAmount")?.SetAttributeValue("currencyID", "COP");

            var taxCategoryElement = new XElement(cac + "TaxCategory");
            taxCategoryElement.Add(new XElement(cbc + "Percent", porcentajeIVA));

            var taxSchemeElement = new XElement(cac + "TaxScheme");
            taxSchemeElement.Add(new XElement(cbc + "ID", "01"));
            taxSchemeElement.Add(new XElement(cbc + "Name", "IVA"));

            taxCategoryElement.Add(taxSchemeElement);
            taxSubtotalElement.Add(taxCategoryElement);

            // Calcular el TaxAmount individual para los productos con este porcentaje de IVA
            decimal totalImpuesto = productos.Sum(p => p.IvaTotal);
            taxSubtotalElement.Element(cbc + "TaxAmount")?.SetValue(totalImpuesto.ToString("F2", CultureInfo.InvariantCulture));

            // Retornar el elemento TaxSubtotal generado
            return taxSubtotalElement;
        }

    }
}
