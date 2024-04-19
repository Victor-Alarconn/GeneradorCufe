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

        public static void GenerarIvasYAgregarElementos(XDocument xmlDoc, IEnumerable<Productos> listaProductos, Movimiento movimiento)
        {
            XNamespace cbc = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2";
            XNamespace cac = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2";

            decimal totalImpuestoIVA = Math.Round(listaProductos.Where(p => p.Iva > 0).Sum(p => p.IvaTotal), 2);
            decimal totalImpuestoIPO = Math.Round(listaProductos.Where(p => p.Iva == 0 && p.Consumo > 0).Sum(p => p.Consumo), 2);
            decimal totalBaseImponibleIPO = Math.Round(listaProductos.Where(p => p.Consumo > 0).Sum(p => p.Neto), 2);

            bool hayProductosConIPO = listaProductos.Any(p => p.Iva == 0 && p.Consumo > 0);
            bool hayProductosConIVA = listaProductos.Any(p => p.Iva > 0);
            bool hayProductosSinIVA = listaProductos.Any(p => p.Iva == 0 && p.Consumo == 0);
            bool hayBolsa = movimiento.Numero_bolsa != 0;

            if (hayProductosConIVA || hayProductosSinIVA)
            {
                // Crear TaxTotal para productos con IVA
                var taxTotalElementIVA = new XElement(cac + "TaxTotal");
                taxTotalElementIVA.Add(new XElement(cbc + "TaxAmount", totalImpuestoIVA.ToString("F2", CultureInfo.InvariantCulture)));
                taxTotalElementIVA.Element(cbc + "TaxAmount")?.SetAttributeValue("currencyID", "COP");

                // Agregar TaxTotal de productos con IVA al XML
                xmlDoc.Descendants(cac + "TaxTotal").FirstOrDefault()?.AddAfterSelf(taxTotalElementIVA);

                if (hayProductosConIVA)
                {
                    var gruposIVA = listaProductos.Where(p => p.Iva > 0).GroupBy(p => p.Iva);
                    foreach (var grupo in gruposIVA)
                    {
                        decimal totalNetoGrupo = Math.Round(grupo.Sum(p => p.Neto), 2);
                        decimal totalImpuestoGrupo = Math.Round(grupo.Sum(p => p.IvaTotal), 2);
                        string porcentajeIVAFormateado = grupo.Key.ToString("F2", CultureInfo.InvariantCulture);
                        var taxSubtotalElementConIVA = GenerarElementoTaxSubtotal(xmlDoc, porcentajeIVAFormateado, totalNetoGrupo, totalImpuestoGrupo, "01", "IVA", movimiento);
                        taxTotalElementIVA.Add(taxSubtotalElementConIVA);
                    }
                }

                // Agregar los detalles de los impuestos para productos sin IVA
                if (hayProductosSinIVA)
                {

                    decimal totalBaseImponibleSinIVA = Math.Round(listaProductos.Where(p => p.Iva == 0 && p.Consumo == 0).Sum(p => p.Neto), 2);
                    var taxSubtotalElementSinIVA = GenerarElementoTaxSubtotal(xmlDoc, "0.00", totalBaseImponibleSinIVA, 0, "01", "IVA", movimiento);
                    taxTotalElementIVA.Add(taxSubtotalElementSinIVA);
                }
            }

            if (hayProductosConIPO) // Agregar los detalles de los impuestos para productos con IPO
            {

                // Crear TaxTotal para productos con IPO
                var taxTotalElementIPO = new XElement(cac + "TaxTotal");
                taxTotalElementIPO.Add(new XElement(cbc + "TaxAmount", totalImpuestoIPO.ToString("F2", CultureInfo.InvariantCulture)));
                taxTotalElementIPO.Element(cbc + "TaxAmount")?.SetAttributeValue("currencyID", "COP");

                // Agregar TaxTotal de productos con IPO al XML
                xmlDoc.Descendants(cac + "TaxTotal").FirstOrDefault()?.AddAfterSelf(taxTotalElementIPO);

                var taxSubtotalElementIPO = GenerarElementoTaxSubtotal(xmlDoc, "8.00", totalBaseImponibleIPO, totalImpuestoIPO, "04", "INC", movimiento);
                taxTotalElementIPO.Add(taxSubtotalElementIPO);
            }

            if (hayBolsa)
            {
                var taxTotalElementBolsa = new XElement(cac + "TaxTotal");
                taxTotalElementBolsa.Add(new XElement(cbc + "TaxAmount", movimiento.Valor_bolsa.ToString("F2", CultureInfo.InvariantCulture)));
                taxTotalElementBolsa.Element(cbc + "TaxAmount")?.SetAttributeValue("currencyID", "COP");
                decimal valorBolsa = Math.Round(movimiento.Valor_bolsa / movimiento.Numero_bolsa, 2);

                // Agregar TaxTotal de productos con IPO al XML
                xmlDoc.Descendants(cac + "TaxTotal").FirstOrDefault()?.AddAfterSelf(taxTotalElementBolsa);

                var taxSubtotalElementBolsa = GenerarElementoTaxSubtotal(xmlDoc, "0.00", 0, valorBolsa, "22", "INC Bolsas", movimiento);
                taxTotalElementBolsa.Add(taxSubtotalElementBolsa);
            }

            var taxTotalTemplate = xmlDoc.Descendants(cac + "TaxTotal").FirstOrDefault();
            taxTotalTemplate?.Remove();
        }

        private static XElement GenerarElementoTaxSubtotal(XDocument xmlDoc, string porcentajeImpuesto, decimal totalBaseImponible, decimal totalImpuesto, string idImpuesto, string nombreImpuesto, Movimiento movimiento)
        {
            XNamespace cbc = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2";
            XNamespace cac = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2";

            // Crear un nuevo elemento TaxSubtotal
            var taxSubtotalElement = new XElement(cac + "TaxSubtotal");

            // Establecer el monto imponible y el monto del impuesto en el elemento TaxSubtotal
            taxSubtotalElement.Add(new XElement(cbc + "TaxableAmount", totalBaseImponible.ToString("F2", CultureInfo.InvariantCulture)));
            taxSubtotalElement.Element(cbc + "TaxableAmount")?.SetAttributeValue("currencyID", "COP");
            // Establecer el monto del impuesto en el elemento TaxSubtotal
            if (idImpuesto == "22") // Si es el impuesto de la bolsa
            {
                // Usar el valor del impuesto de la bolsa
                taxSubtotalElement.Add(new XElement(cbc + "TaxAmount", movimiento.Valor_bolsa.ToString("F2", CultureInfo.InvariantCulture)));
            }
            else
            {
                // Usar el valor totalImpuesto proporcionado
                taxSubtotalElement.Add(new XElement(cbc + "TaxAmount", totalImpuesto.ToString("F2", CultureInfo.InvariantCulture)));
            }
            taxSubtotalElement.Element(cbc + "TaxAmount")?.SetAttributeValue("currencyID", "COP");

            // Agregar campos adicionales para el impuesto de la bolsa
            if (idImpuesto == "22") 
            {
                taxSubtotalElement.Add(new XElement(cbc + "BaseUnitMeasure", movimiento.Numero_bolsa.ToString("F2", CultureInfo.InvariantCulture))); // Medida base
                taxSubtotalElement.Element(cbc + "BaseUnitMeasure")?.SetAttributeValue("unitCode", "94"); // Código de unidad
                decimal valorBasebolsa = Math.Round(totalImpuesto * movimiento.Numero_bolsa, 2);
                taxSubtotalElement.Add(new XElement(cbc + "PerUnitAmount",
                                      new XAttribute("currencyID", "COP"),
                                      totalImpuesto.ToString("F2", CultureInfo.InvariantCulture)));
            }

            // Agregar la categoría de impuesto al elemento TaxSubtotal
            var taxCategoryElement = new XElement(cac + "TaxCategory");
            taxCategoryElement.Add(new XElement(cbc + "Percent", porcentajeImpuesto));

            var taxSchemeElement = new XElement(cac + "TaxScheme");
            taxSchemeElement.Add(new XElement(cbc + "ID", idImpuesto));
            taxSchemeElement.Add(new XElement(cbc + "Name", nombreImpuesto));

            taxCategoryElement.Add(taxSchemeElement);
            taxSubtotalElement.Add(taxCategoryElement);

            // Retornar el elemento TaxSubtotal generado
            return taxSubtotalElement;
        }



    }
}
