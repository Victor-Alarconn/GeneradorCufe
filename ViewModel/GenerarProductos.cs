using GeneradorCufe.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace GeneradorCufe.ViewModel
{
    public class GenerarProductos
    {
        public static void MapInvoiceLine(XDocument xmlDoc, List<Productos> listaProductos) // Productos
        {
            // Namespace específico para los elementos bajo 'sts'
            XNamespace sts = "dian:gov:co:facturaelectronica:Structures-2-1";
            // Namespace para elementos 'cbc'
            XNamespace cbc = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2";
            // Namespace para elementos 'cac'
            XNamespace cac = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2";

            // Obtener el elemento 'cac:InvoiceLine' para utilizarlo como plantilla para agregar nuevos productos
            var invoiceLineTemplate = xmlDoc.Descendants(cac + "InvoiceLine").FirstOrDefault();

            // Verificar si la plantilla existe
            if (invoiceLineTemplate != null)
            {
                // Iterar sobre cada producto en la lista y agregarlos al XML
                for (int i = 0; i < listaProductos.Count; i++)
                {
                    var producto = listaProductos[i];
                    // Crear un nuevo elemento 'cac:InvoiceLine' basado en la plantilla
                    var invoiceLineElement = new XElement(invoiceLineTemplate);

                    // Establecer los valores del producto en el nuevo elemento
                    string cantidadformateada = producto.Cantidad.ToString("F2", CultureInfo.InvariantCulture);
                    invoiceLineElement.Element(cbc + "ID")?.SetValue((i + 1).ToString());
                    invoiceLineElement.Element(cbc + "InvoicedQuantity")?.SetValue(cantidadformateada);
                    invoiceLineElement.Element(cbc + "LineExtensionAmount")?.SetValue(producto.Neto); // cufe  ValFac

                    // Establecer los valores del impuesto
                    // Establecer los valores del impuesto
                    var taxTotalElement = invoiceLineElement.Element(cac + "TaxTotal");
                    if (taxTotalElement != null)
                    {
                        // Verificar si el total de impuestos es 0.00 y hay consumo
                        if (producto.IvaTotal == 0.00m && producto.Consumo > 0.00m)
                        {
                            // Eliminar la plantilla de TaxSubtotal si existe
                            var taxSubtotalTemplate = taxTotalElement.Elements(cac + "TaxSubtotal").FirstOrDefault();
                            taxSubtotalTemplate?.Remove();

                            // Crear y establecer los elementos TaxAmount, Percent, ID y Name directamente
                            var taxSubtotalElement = new XElement(cac + "TaxSubtotal");
                            taxSubtotalElement.Add(new XElement(cbc + "TaxableAmount", producto.Neto.ToString("F2", CultureInfo.InvariantCulture)));
                            taxSubtotalElement.Element(cbc + "TaxableAmount")?.SetAttributeValue("currencyID", "COP");
                            taxSubtotalElement.Add(new XElement(cbc + "TaxAmount", producto.Consumo.ToString("F2", CultureInfo.InvariantCulture)));
                            taxSubtotalElement.Element(cbc + "TaxAmount")?.SetAttributeValue("currencyID", "COP");

                            var taxCategoryElement = new XElement(cac + "TaxCategory");
                            taxCategoryElement.Add(new XElement(cbc + "Percent", "8.00"));

                            var taxSchemeElement = new XElement(cac + "TaxScheme");
                            taxSchemeElement.Add(new XElement(cbc + "ID", "04"));
                            taxSchemeElement.Add(new XElement(cbc + "Name", "INC"));

                            taxCategoryElement.Add(taxSchemeElement);
                            taxSubtotalElement.Add(taxCategoryElement);

                            // Agregar el elemento TaxSubtotal al TaxTotal
                            taxTotalElement.Add(taxSubtotalElement);

                            // Reemplazar el elemento TaxAmount existente con el valor de producto.Consumo
                            var existingTaxAmountElement = taxTotalElement.Element(cbc + "TaxAmount");
                            existingTaxAmountElement?.SetValue(producto.Consumo.ToString("F2", CultureInfo.InvariantCulture));
                        }
                        else
                        {
                            // Si no se cumple la condición anterior, establecer los valores predeterminados del impuesto
                            taxTotalElement.Element(cbc + "TaxAmount")?.SetValue(producto.IvaTotal); // cufe ValImp1

                            var taxSubtotalElement = taxTotalElement.Element(cac + "TaxSubtotal");
                            if (taxSubtotalElement != null)
                            {
                                string valorformateada = producto.Valor.ToString("F2", CultureInfo.InvariantCulture);
                                taxSubtotalElement.Element(cbc + "TaxableAmount")?.SetValue(producto.Neto);
                                taxSubtotalElement.Element(cbc + "TaxAmount")?.SetValue(producto.IvaTotal);

                                var taxCategoryElement = taxSubtotalElement.Element(cac + "TaxCategory");
                                if (taxCategoryElement != null)
                                {
                                    // Formatear el valor del IVA con dos decimales
                                    string formattedIva = producto.Iva.ToString("F2", CultureInfo.InvariantCulture);

                                    // Establecer el valor formateado en el elemento Percent
                                    taxCategoryElement.Element(cbc + "Percent")?.SetValue(formattedIva);

                                    // Agregar la parte faltante para TaxScheme dentro de TaxCategory
                                    var taxSchemeElement = taxCategoryElement.Element(cac + "TaxScheme");
                                    if (taxSchemeElement == null)
                                    {
                                        // Si no existe el elemento TaxScheme, lo creamos
                                        taxSchemeElement = new XElement(cac + "TaxScheme");
                                        taxCategoryElement.Add(taxSchemeElement);
                                    }

                                    // Verificar el valor del IVA del producto
                                    if (producto.Iva == 8)
                                    {
                                        // Si el IVA es 8, cambiar el ID y el nombre del esquema de impuestos
                                        taxSchemeElement.Element(cbc + "ID")?.SetValue("04");
                                        taxSchemeElement.Element(cbc + "Name")?.SetValue("INC");
                                    }
                                    else
                                    {
                                        // Si no es 8, mantener los valores predeterminados (ID = 01, Name = IVA)
                                        taxSchemeElement.Element(cbc + "ID")?.SetValue("01");
                                        taxSchemeElement.Element(cbc + "Name")?.SetValue("IVA");
                                    }
                                }
                            }
                        }
                    }

                    // Establecer los valores del ítem
                    var itemElement = invoiceLineElement.Element(cac + "Item");
                    if (itemElement != null)
                    {
                        itemElement.Element(cbc + "Description")?.SetValue(producto.Detalle);

                        var standardItemIdentificationElement = itemElement.Element(cac + "StandardItemIdentification");
                        if (standardItemIdentificationElement != null)
                        {
                            standardItemIdentificationElement.Element(cbc + "ID")?.SetValue(producto.Codigo);
                        }
                    }

                    var priceElement = invoiceLineElement.Element(cac + "Price");
                    if (priceElement != null)
                    {
                        // Formatear el precio con dos decimales utilizando CultureInfo.InvariantCulture
                        string formattedPrice = producto.Valor.ToString("F2", CultureInfo.InvariantCulture);
                        // Asignar el precio formateado al elemento XML
                        priceElement.Element(cbc + "PriceAmount")?.SetValue(formattedPrice);

                        // Formatear la cantidad con dos decimales utilizando CultureInfo.InvariantCulture
                        string formattedQuantity = producto.Cantidad.ToString("F2", CultureInfo.InvariantCulture);
                        // Asignar la cantidad formateada al elemento XML
                        priceElement.Element(cbc + "BaseQuantity")?.SetValue(formattedQuantity);
                    }

                    // Agregar el nuevo elemento 'cac:InvoiceLine' al XML
                    xmlDoc.Descendants(cac + "InvoiceLine").LastOrDefault()?.AddAfterSelf(invoiceLineElement);
                }
                invoiceLineTemplate.Remove();
            }
        }
    }
}
