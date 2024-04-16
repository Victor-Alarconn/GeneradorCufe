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

        //public static void GenerarIvasYAgregarElementos(XDocument xmlDoc, IEnumerable<Productos> listaProductos)
        //{
        //    XNamespace cbc = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2";
        //    XNamespace cac = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2";

        //    decimal totalImpuesto = Math.Round(listaProductos.Sum(p => p.IvaTotal), 2);
        //    decimal consumo = Math.Round(listaProductos.Sum(p => p.Consumo), 2);
        //    decimal totalBaseImponible = Math.Round(listaProductos.Sum(p => p.Neto), 2);
        //    bool hayProductosConIPO = listaProductos.Any(p => p.Iva == 0 && p.Consumo > 0);
        //    bool hayProductosConIVA = listaProductos.Any(p => p.Iva > 0);
        //    bool hayProductosSinIVA = listaProductos.Any(p => p.Iva == 0 && p.Consumo == 0);

        //    var taxTotalElement = xmlDoc.Descendants(cac + "TaxTotal").FirstOrDefault();
        //    if (taxTotalElement != null)
        //    {
        //        if (hayProductosConIPO)
        //        {
        //            var nuevoImpuestoElement = new XElement(cac + "TaxTotal"); // nuevo taxtotal 
        //            nuevoImpuestoElement.Add(new XElement(cbc + "TaxAmount", "200.00")); // Establecer el monto del nuevo impuesto
        //            nuevoImpuestoElement.Element(cbc + "TaxAmount")?.SetAttributeValue("currencyID", "COP");

        //            taxTotalElement.Element(cbc + "TaxAmount")?.SetValue(consumo.ToString("F2", CultureInfo.InvariantCulture));
        //            var taxSubtotalElement = new XElement(cac + "TaxSubtotal");
        //            taxSubtotalElement.Add(new XElement(cbc + "TaxableAmount", totalBaseImponible.ToString("F2", CultureInfo.InvariantCulture)));
        //            taxSubtotalElement.Element(cbc + "TaxableAmount")?.SetAttributeValue("currencyID", "COP");
        //            taxSubtotalElement.Add(new XElement(cbc + "TaxAmount", consumo.ToString("F2", CultureInfo.InvariantCulture)));
        //            taxSubtotalElement.Element(cbc + "TaxAmount")?.SetAttributeValue("currencyID", "COP");
        //            var taxCategoryElement = new XElement(cac + "TaxCategory");
        //            taxCategoryElement.Add(new XElement(cbc + "Percent", "8.00"));
        //            var taxSchemeElement = new XElement(cac + "TaxScheme");
        //            taxSchemeElement.Add(new XElement(cbc + "ID", "04"));
        //            taxSchemeElement.Add(new XElement(cbc + "Name", "INC"));
        //            taxCategoryElement.Add(taxSchemeElement);
        //            taxSubtotalElement.Add(taxCategoryElement);
        //            taxTotalElement.Add(taxSubtotalElement);
        //        }

        //        if (hayProductosConIVA || hayProductosSinIVA)
        //        {
        //            taxTotalElement.Element(cbc + "TaxAmount")?.SetValue(totalImpuesto.ToString("F2", CultureInfo.InvariantCulture));

        //            if (hayProductosConIVA)
        //            {
        //                var gruposIVA = listaProductos.Where(p => p.Iva > 0).GroupBy(p => p.Iva);
        //                foreach (var grupo in gruposIVA)
        //                {
        //                    decimal totalImpuestoGrupo = Math.Round(grupo.Sum(p => p.IvaTotal), 2);
        //                    string porcentajeIVAFormateado = grupo.Key.ToString("F2", CultureInfo.InvariantCulture);
        //                    var taxSubtotalElementConIVA = GenerarElementoTaxSubtotal(xmlDoc, porcentajeIVAFormateado, grupo.ToList());
        //                    taxTotalElement.Add(taxSubtotalElementConIVA);
        //                }
        //            }

        //            if (hayProductosSinIVA)
        //            {
        //                var taxSubtotalElementSinIVA = GenerarElementoTaxSubtotal(xmlDoc, "0.00", listaProductos.Where(p => p.Iva == 0).ToList());
        //                taxTotalElement.Add(taxSubtotalElementSinIVA);
        //            }
        //        }

        //        var taxSubtotalTemplate = xmlDoc.Descendants(cac + "TaxSubtotal").FirstOrDefault();
        //        taxSubtotalTemplate?.Remove();
        //    }
        //}

        //private static XElement GenerarElementoTaxSubtotal(XDocument xmlDoc, string porcentajeImpuesto, decimal totalImpuesto)
        //{
        //    XNamespace cbc = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2";
        //    XNamespace cac = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2";

        //    // Crear un nuevo elemento TaxSubtotal
        //    var taxSubtotalElement = new XElement(cac + "TaxSubtotal");

        //    // Calcular el total de la base imponible para los productos dados
        //    decimal totalBaseImponible = 0; // No estoy seguro de cómo obtener esto en tu contexto, así que asegúrate de calcularlo correctamente

        //    // Establecer los valores calculados en el elemento TaxSubtotal
        //    taxSubtotalElement.Add(new XElement(cbc + "TaxableAmount", totalBaseImponible.ToString("F2", CultureInfo.InvariantCulture)));
        //    taxSubtotalElement.Element(cbc + "TaxableAmount")?.SetAttributeValue("currencyID", "COP");

        //    // Establecer los valores de impuestos en el elemento TaxSubtotal
        //    taxSubtotalElement.Add(new XElement(cbc + "TaxAmount", totalImpuesto.ToString("F2", CultureInfo.InvariantCulture)));
        //    taxSubtotalElement.Element(cbc + "TaxAmount")?.SetAttributeValue("currencyID", "COP");

        //    // Agregar la categoría de impuesto al elemento TaxSubtotal
        //    var taxCategoryElement = new XElement(cac + "TaxCategory");
        //    taxCategoryElement.Add(new XElement(cbc + "Percent", porcentajeImpuesto));

        //    var taxSchemeElement = new XElement(cac + "TaxScheme");
        //    taxSchemeElement.Add(new XElement(cbc + "ID", "01"));
        //    taxSchemeElement.Add(new XElement(cbc + "Name", "IVA")); // Cambiar "IVA" si es necesario

        //    taxCategoryElement.Add(taxSchemeElement);
        //    taxSubtotalElement.Add(taxCategoryElement);

        //    // Retornar el elemento TaxSubtotal generado
        //    return taxSubtotalElement;
        //}

        public static void GenerarIvasYAgregarElementos(XDocument xmlDoc, IEnumerable<Productos> listaProductos)
        {
            XNamespace cbc = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2";
            XNamespace cac = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2";

            decimal totalImpuestoIVA = Math.Round(listaProductos.Where(p => p.Iva > 0).Sum(p => p.IvaTotal), 2);
            decimal totalImpuestoIPO = Math.Round(listaProductos.Where(p => p.Iva == 0 && p.Consumo > 0).Sum(p => p.Consumo), 2);
            decimal totalBaseImponible = Math.Round(listaProductos.Where(p => p.Consumo == 0).Sum(p => p.Neto), 2);
            decimal totalBaseImponibleIPO = Math.Round(listaProductos.Where(p => p.Consumo > 0).Sum(p => p.Neto), 2);

            bool hayProductosConIPO = listaProductos.Any(p => p.Iva == 0 && p.Consumo > 0);
            bool hayProductosConIVA = listaProductos.Any(p => p.Iva > 0);
            bool hayProductosSinIVA = listaProductos.Any(p => p.Iva == 0 && p.Consumo == 0);

            // Crear TaxTotal para productos con IVA
            var taxTotalElementIVA = new XElement(cac + "TaxTotal");
            taxTotalElementIVA.Add(new XElement(cbc + "TaxAmount", totalImpuestoIVA.ToString("F2", CultureInfo.InvariantCulture)));
            taxTotalElementIVA.Element(cbc + "TaxAmount")?.SetAttributeValue("currencyID", "COP");

            // Crear TaxTotal para productos con IPO
            var taxTotalElementIPO = new XElement(cac + "TaxTotal");
            taxTotalElementIPO.Add(new XElement(cbc + "TaxAmount", totalImpuestoIPO.ToString("F2", CultureInfo.InvariantCulture)));
            taxTotalElementIPO.Element(cbc + "TaxAmount")?.SetAttributeValue("currencyID", "COP");

            // Agregar TaxTotal de productos con IVA al XML
            xmlDoc.Descendants(cac + "TaxTotal").FirstOrDefault()?.AddAfterSelf(taxTotalElementIVA);

            // Agregar TaxTotal de productos con IPO al XML
            xmlDoc.Descendants(cac + "TaxTotal").FirstOrDefault()?.AddAfterSelf(taxTotalElementIPO);

            // Agregar los detalles de los impuestos para productos con IVA
            if (hayProductosConIVA)
            {
                var gruposIVA = listaProductos.Where(p => p.Iva > 0).GroupBy(p => p.Iva);
                foreach (var grupo in gruposIVA)
                {
                    decimal totalImpuestoGrupo = Math.Round(grupo.Sum(p => p.IvaTotal), 2);
                    string porcentajeIVAFormateado = grupo.Key.ToString("F2", CultureInfo.InvariantCulture);
                    var taxSubtotalElementConIVA = GenerarElementoTaxSubtotal(xmlDoc, porcentajeIVAFormateado, totalBaseImponible, totalImpuestoGrupo, "01", "IVA");
                    taxTotalElementIVA.Add(taxSubtotalElementConIVA);
                }
            }

            // Agregar los detalles de los impuestos para productos con IPO
            if (hayProductosConIPO)
            {
                var taxSubtotalElementIPO = GenerarElementoTaxSubtotal(xmlDoc, "8.00", totalBaseImponibleIPO, totalImpuestoIPO, "04", "INC");
                taxTotalElementIPO.Add(taxSubtotalElementIPO);
            }

            // Agregar los detalles de los impuestos para productos sin IVA
            if (hayProductosSinIVA)
            {
                var taxSubtotalElementSinIVA = GenerarElementoTaxSubtotal(xmlDoc, "0.00", totalBaseImponible, 0, "01", "IVA");
                taxTotalElementIVA.Add(taxSubtotalElementSinIVA);
            }

            var taxTotalTemplate = xmlDoc.Descendants(cac + "TaxTotal").FirstOrDefault();
            taxTotalTemplate?.Remove();
        }

        private static XElement GenerarElementoTaxSubtotal(XDocument xmlDoc, string porcentajeImpuesto, decimal totalBaseImponible, decimal totalImpuesto, string idImpuesto, string nombreImpuesto)
        {
            XNamespace cbc = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2";
            XNamespace cac = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2";

            // Crear un nuevo elemento TaxSubtotal
            var taxSubtotalElement = new XElement(cac + "TaxSubtotal");

            // Establecer el monto imponible y el monto del impuesto en el elemento TaxSubtotal
            taxSubtotalElement.Add(new XElement(cbc + "TaxableAmount", totalBaseImponible.ToString("F2", CultureInfo.InvariantCulture)));
            taxSubtotalElement.Element(cbc + "TaxableAmount")?.SetAttributeValue("currencyID", "COP");
            taxSubtotalElement.Add(new XElement(cbc + "TaxAmount", totalImpuesto.ToString("F2", CultureInfo.InvariantCulture)));
            taxSubtotalElement.Element(cbc + "TaxAmount")?.SetAttributeValue("currencyID", "COP");

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
