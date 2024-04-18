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
        public static void MapInvoiceLine(XDocument xmlDoc, List<Productos> listaProductos, Movimiento movimiento) // Productos
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
                int idCounter = 0;
                // Iterar sobre cada producto en la lista y agregarlos al XML
                for (int i = 0; i < listaProductos.Count; i++)
                {
                    var producto = listaProductos[i];
                    idCounter++;
                    // Crear un nuevo elemento 'cac:InvoiceLine' basado en la plantilla
                    var invoiceLineElement = new XElement(invoiceLineTemplate);

                    // Establecer los valores del producto en el nuevo elemento
                    string cantidadformateada = producto.Cantidad.ToString("F2", CultureInfo.InvariantCulture);
                    invoiceLineElement.Element(cbc + "ID")?.SetValue(idCounter.ToString());
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
                // Verificar si movimiento.Numero_bolsa tiene un valor diferente de 0
                if (movimiento.Numero_bolsa != 0)
                {
                    idCounter++;
                    decimal valorBolsa = Math.Round(movimiento.Valor_bolsa / movimiento.Numero_bolsa, 2);
                    // Crear un nuevo elemento 'cac:InvoiceLine' para el impuesto a la bolsa
                    var invoiceLineBolsaElement = new XElement(cac + "InvoiceLine");
                    invoiceLineBolsaElement.Add(new XElement(cbc + "ID", idCounter.ToString())); // Establecer el ID
                    invoiceLineBolsaElement.Add(new XElement(cbc + "InvoicedQuantity",
                                        new XAttribute("unitCode", "94"),
                                        movimiento.Numero_bolsa.ToString("F2", CultureInfo.InvariantCulture)));
                    invoiceLineBolsaElement.Add(new XElement(cbc + "LineExtensionAmount",
                                         new XAttribute("currencyID", "COP"),
                                         "0.00"));
                    invoiceLineBolsaElement.Add(new XElement(cac + "PricingReference",
                        new XElement(cac + "AlternativeConditionPrice",
                            new XElement(cbc + "PriceAmount",
                             new XAttribute("currencyID", "COP"),
                             valorBolsa.ToString("F2", CultureInfo.InvariantCulture)),
                            new XElement(cbc + "PriceTypeCode", "03"),
                            new XElement(cbc + "PriceType", "Otro valor")
                        )
                    ));

                    // Agregar TaxTotal para el impuesto a la bolsa
                    var taxTotalBolsaElement = new XElement(cac + "TaxTotal");
                    taxTotalBolsaElement.Add(new XElement(cbc + "TaxAmount", new XAttribute("currencyID", "COP"), movimiento.Valor_bolsa.ToString("F2", CultureInfo.InvariantCulture))); // Establecer el monto del impuesto

                    var taxSubtotalBolsaElement = new XElement(cac + "TaxSubtotal",
                        new XElement(cbc + "TaxableAmount",
                                     new XAttribute("currencyID", "COP"),
                                     "0.00"),
                        new XElement(cbc + "TaxAmount",
                                     new XAttribute("currencyID", "COP"),
                                     movimiento.Valor_bolsa.ToString("F2", CultureInfo.InvariantCulture)),
                        new XElement(cbc + "BaseUnitMeasure",
                                     new XAttribute("unitCode", "94"),
                                     movimiento.Numero_bolsa.ToString("F2", CultureInfo.InvariantCulture)),
                        new XElement(cbc + "PerUnitAmount",
                                     new XAttribute("currencyID", "COP"),
                                     valorBolsa.ToString("F2", CultureInfo.InvariantCulture)),
                        new XElement(cac + "TaxCategory",
                    new XElement(cac + "TaxScheme",
                                new XElement(cbc + "ID", "22"), // Establecer el ID del esquema de impuestos
                                new XElement(cbc + "Name", "INC Bolsas") // Establecer el nombre del esquema de impuestos
                            )
                        )
                    );
                    taxTotalBolsaElement.Add(taxSubtotalBolsaElement);
                    invoiceLineBolsaElement.Add(taxTotalBolsaElement);

                    invoiceLineBolsaElement.Add(new XElement(cac + "Item",
                        new XElement(cbc + "Description", "IMPUESTO A LA BOLSA PLASTICA"),
                        new XElement(cac + "StandardItemIdentification",
                            new XElement(cbc + "ID", "3", new XAttribute("schemeID", "999"))
                        )
                    ));

                    invoiceLineBolsaElement.Add(new XElement(cac + "Price",
                        new XElement(cbc + "PriceAmount",
                            new XAttribute("currencyID", "COP"),
                            "0.00"),
                        new XElement(cbc + "BaseQuantity", movimiento.Numero_bolsa.ToString("F2", CultureInfo.InvariantCulture), new XAttribute("unitCode", "NIU"))
                    ));

                    // Agregar el nuevo elemento 'cac:InvoiceLine' al XML
                    xmlDoc.Descendants(cac + "InvoiceLine").LastOrDefault()?.AddAfterSelf(invoiceLineBolsaElement);
                }

                invoiceLineTemplate.Remove();
            }
        }

        public static void MapCreditNoteLine(XDocument xmlDoc, List<Productos> listaProductos, Movimiento movimiento) // Productos
        {
            // Namespace específico para los elementos bajo 'sts'
            XNamespace sts = "dian:gov:co:facturaelectronica:Structures-2-1";
            // Namespace para elementos 'cbc'
            XNamespace cbc = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2";
            // Namespace para elementos 'cac'
            XNamespace cac = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2";

            // Obtener el elemento 'cac:InvoiceLine' para utilizarlo como plantilla para agregar nuevos productos
            var invoiceLineTemplate = xmlDoc.Descendants(cac + "CreditNoteLine").FirstOrDefault();

            // Verificar si la plantilla existe
            if (invoiceLineTemplate != null)
            {
                int idCounter = 0;
                // Iterar sobre cada producto en la lista y agregarlos al XML
                for (int i = 0; i < listaProductos.Count; i++)
                {
                    var producto = listaProductos[i];
                    idCounter++;
                    // Crear un nuevo elemento 'cac:InvoiceLine' basado en la plantilla
                    var invoiceLineElement = new XElement(invoiceLineTemplate);

                    // Establecer los valores del producto en el nuevo elemento
                    string cantidadformateada = producto.Cantidad.ToString("F2", CultureInfo.InvariantCulture);
                    invoiceLineElement.Element(cbc + "ID")?.SetValue(idCounter.ToString());
                    invoiceLineElement.Element(cbc + "CreditedQuantity")?.SetValue(cantidadformateada);
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
                    xmlDoc.Descendants(cac + "CreditNoteLine").LastOrDefault()?.AddAfterSelf(invoiceLineElement);
                }
                // Verificar si movimiento.Numero_bolsa tiene un valor diferente de 0
                if (movimiento.Numero_bolsa != 0)
                {
                    idCounter++;
                    decimal valorBolsa = Math.Round(movimiento.Valor_bolsa / movimiento.Numero_bolsa, 2);
                    // Crear un nuevo elemento 'cac:InvoiceLine' para el impuesto a la bolsa
                    var invoiceLineBolsaElement = new XElement(cac + "CreditNoteLine");
                    invoiceLineBolsaElement.Add(new XElement(cbc + "ID", idCounter.ToString())); // Establecer el ID
                    invoiceLineBolsaElement.Add(new XElement(cbc + "CreditedQuantity",
                                        new XAttribute("unitCode", "94"),
                                        movimiento.Numero_bolsa.ToString("F2", CultureInfo.InvariantCulture)));
                    invoiceLineBolsaElement.Add(new XElement(cbc + "LineExtensionAmount",
                                         new XAttribute("currencyID", "COP"),
                                         "0.00"));
                    invoiceLineBolsaElement.Add(new XElement(cac + "PricingReference",
                        new XElement(cac + "AlternativeConditionPrice",
                            new XElement(cbc + "PriceAmount",
                             new XAttribute("currencyID", "COP"),
                             valorBolsa.ToString("F2", CultureInfo.InvariantCulture)),
                            new XElement(cbc + "PriceTypeCode", "03"),
                            new XElement(cbc + "PriceType", "Otro valor")
                        )
                    ));

                    // Agregar TaxTotal para el impuesto a la bolsa
                    var taxTotalBolsaElement = new XElement(cac + "TaxTotal");
                    taxTotalBolsaElement.Add(new XElement(cbc + "TaxAmount", new XAttribute("currencyID", "COP"), movimiento.Valor_bolsa.ToString("F2", CultureInfo.InvariantCulture))); // Establecer el monto del impuesto

                    var taxSubtotalBolsaElement = new XElement(cac + "TaxSubtotal",
                        new XElement(cbc + "TaxableAmount",
                                     new XAttribute("currencyID", "COP"),
                                     "0.00"),
                        new XElement(cbc + "TaxAmount",
                                     new XAttribute("currencyID", "COP"),
                                     movimiento.Valor_bolsa.ToString("F2", CultureInfo.InvariantCulture)),
                        new XElement(cbc + "BaseUnitMeasure",
                                     new XAttribute("unitCode", "94"),
                                     movimiento.Numero_bolsa.ToString("F2", CultureInfo.InvariantCulture)),
                        new XElement(cbc + "PerUnitAmount",
                                     new XAttribute("currencyID", "COP"),
                                     valorBolsa.ToString("F2", CultureInfo.InvariantCulture)),
                        new XElement(cac + "TaxCategory",
                    new XElement(cac + "TaxScheme",
                                new XElement(cbc + "ID", "22"), // Establecer el ID del esquema de impuestos
                                new XElement(cbc + "Name", "INC Bolsas") // Establecer el nombre del esquema de impuestos
                            )
                        )
                    );
                    taxTotalBolsaElement.Add(taxSubtotalBolsaElement);
                    invoiceLineBolsaElement.Add(taxTotalBolsaElement);

                    invoiceLineBolsaElement.Add(new XElement(cac + "Item",
                        new XElement(cbc + "Description", "IMPUESTO A LA BOLSA PLASTICA"),
                        new XElement(cac + "StandardItemIdentification",
                            new XElement(cbc + "ID", "3", new XAttribute("schemeID", "999"))
                        )
                    ));

                    invoiceLineBolsaElement.Add(new XElement(cac + "Price",
                        new XElement(cbc + "PriceAmount",
                            new XAttribute("currencyID", "COP"),
                            "0.00"),
                        new XElement(cbc + "BaseQuantity", movimiento.Numero_bolsa.ToString("F2", CultureInfo.InvariantCulture), new XAttribute("unitCode", "NIU"))
                    ));

                    // Agregar el nuevo elemento 'cac:InvoiceLine' al XML
                    xmlDoc.Descendants(cac + "CreditNoteLine").LastOrDefault()?.AddAfterSelf(invoiceLineBolsaElement);
                }

                invoiceLineTemplate.Remove();
            }
        }
    }
}