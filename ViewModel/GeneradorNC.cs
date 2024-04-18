using GeneradorCufe.Consultas;
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
    public class GeneradorNC
    {
        public static (string cadenaConexion, string CUFE) GeneradorNotaCredito(XDocument xmlDoc, Emisor emisor, Factura factura)
        {
            XNamespace cbc = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2";
            XNamespace cac = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2";
            string cufe = "";

            Productos_Consulta productosConsulta = new Productos_Consulta();
            Encabezado_Consulta encabezadoConsulta = new Encabezado_Consulta();
            Codigos_Consulta codigosConsulta = new Codigos_Consulta();
            Movimiento_Consulta movimientoConsulta = new Movimiento_Consulta();
            FormaPago_Consulta formaPagoConsulta = new FormaPago_Consulta();
            string PrefijoNC = "NC" + factura.Recibo;

            string cadenaConexion = "";

            if (factura.Ip_base == "200.118.190.213" || factura.Ip_base == "200.118.190.167")
            {
                cadenaConexion = $"Database={factura.Empresa}; Data Source={factura.Ip_base}; User Id=RmSoft20X;Password=**LiLo89**; ConvertZeroDateTime=True;";
            }
            else if (factura.Ip_base == "192.190.42.191")
            {
                cadenaConexion = $"Database={factura.Empresa}; Data Source={factura.Ip_base}; User Id=root;Password=**qwerty**; ConvertZeroDateTime=True;";
            }
            // Llamar al método ConsultarProductosPorFactura para obtener la lista de productos
            Encabezado encabezado = encabezadoConsulta.ConsultarEncabezado(factura, cadenaConexion);
            Movimiento movimiento = movimientoConsulta.ConsultarValoresTotales(factura, cadenaConexion);
            List<Productos> listaProductos = productosConsulta.ConsultarProductosPorFactura(factura, cadenaConexion);

            DateTimeOffset horaConDesplazamiento = DateTimeOffset.ParseExact(movimiento.Hora_dig, "HH:mm:ss", CultureInfo.InvariantCulture);
            string horaformateada = horaConDesplazamiento.ToString("HH:mm:sszzz", CultureInfo.InvariantCulture);

            // Actualizar 'CustomizationID'
            xmlDoc.Descendants(cbc + "CustomizationID").FirstOrDefault()?.SetValue("20"); // 22 o sin referencia a facturas

            // Actualizar 'ProfileExecutionID'
            xmlDoc.Descendants(cbc + "ProfileExecutionID").FirstOrDefault()?.SetValue("2"); // 1 produccion , 2 pruebas

            // Actualizar 'ID'
            xmlDoc.Descendants(cbc + "ID").FirstOrDefault()?.SetValue(PrefijoNC);

            // Actualizar 'UUID'
            xmlDoc.Descendants(cbc + "UUID").FirstOrDefault()?.SetValue("7777");
            xmlDoc.Descendants(cbc + "UUID").FirstOrDefault()?.SetAttributeValue("schemeID", "2");
            xmlDoc.Descendants(cbc + "UUID").FirstOrDefault()?.SetAttributeValue("schemeName", "CUDE-SHA384");

            // Actualizar 'IssueDate' y 'IssueTime'
            xmlDoc.Descendants(cbc + "IssueDate").FirstOrDefault()?.SetValue(movimiento.Fecha_Factura.ToString("yyyy-MM-dd"));
            xmlDoc.Descendants(cbc + "IssueTime").FirstOrDefault()?.SetValue(horaformateada);

            // Actualizar 'CreditNoteTypeCode'
            xmlDoc.Descendants(cbc + "CreditNoteTypeCode").FirstOrDefault()?.SetValue("91");

            // Actualizar 'Note'
            xmlDoc.Descendants(cbc + "Note").FirstOrDefault()?.SetValue("7777");

            // Actualizar 'DocumentCurrencyCode'
            xmlDoc.Descendants(cbc + "DocumentCurrencyCode").FirstOrDefault()?.SetValue("COP");

            // Actualizar 'LineCountNumeric'
            xmlDoc.Descendants(cbc + "LineCountNumeric").FirstOrDefault()?.SetValue("7777");

            // Actualizar 'DiscrepancyResponse'
            var discrepancyResponseElement = xmlDoc.Descendants(cac + "DiscrepancyResponse").FirstOrDefault();
            if (discrepancyResponseElement != null)
            {
                discrepancyResponseElement.Element(cbc + "ReferenceID")?.SetValue("77777");
                discrepancyResponseElement.Element(cbc + "ResponseCode")?.SetValue("7777");
                discrepancyResponseElement.Element(cbc + "Description")?.SetValue("7777");
            }

            // Actualizar 'BillingReference'
            var billingReferenceElement = xmlDoc.Descendants(cac + "BillingReference").FirstOrDefault();
            if (billingReferenceElement != null)
            {
                var invoiceDocumentReferenceElement = billingReferenceElement.Element(cac + "InvoiceDocumentReference");
                if (invoiceDocumentReferenceElement != null)
                {
                    invoiceDocumentReferenceElement.Element(cbc + "ID")?.SetValue(factura.Facturas);
                    invoiceDocumentReferenceElement.Element(cbc + "UUID")?.SetValue("b5c3b4f4aa53d3a14c3be6fdbde52c9b284723880c93fd4ed10d540a5e32a3f8b1c34cbadbe0ee253d1e50e0f6f8fa44");
                    invoiceDocumentReferenceElement.Element(cbc + "UUID")?.SetAttributeValue("schemeName", "CUFE-SHA384");
                    invoiceDocumentReferenceElement.Element(cbc + "IssueDate")?.SetValue("2019-06-04");
                }
            }

            string ciudadCompleta = emisor.Nombre_municipio_emisor ?? "";
            string[] partesCiudad = ciudadCompleta.Split(',');
            string Municipio = partesCiudad.Length > 0 ? partesCiudad[0].Trim() : ""; // Obtiene el municipio (primer elemento después de dividir)
            string Departamento = partesCiudad.Length > 1 ? partesCiudad[1].Trim() : ""; // Obtiene el departamento (segundo elemento después de dividir)
            Codigos codigos = codigosConsulta.ConsultarCodigos(ciudadCompleta);

            GenerarEmisor.MapearInformacionEmisor(xmlDoc, emisor, encabezado, codigos);

            string nitValue = listaProductos[0].Nit;
            GenerarAdquiriente.MapAccountingCustomerParty(xmlDoc, nitValue, cadenaConexion);

            // Información del medio de pago
            var paymentMeansElement = xmlDoc.Descendants(cac + "PaymentMeans").FirstOrDefault();
            if (paymentMeansElement != null)
            {
                paymentMeansElement.Element(cbc + "ID")?.SetValue("1");
                paymentMeansElement.Element(cbc + "PaymentMeansCode")?.SetValue("10");
                paymentMeansElement.Element(cbc + "PaymentID")?.SetValue("Efectivo");
            }

            // Calcular el total del IVA de todos los productos
            GenerarIvas.GenerarIvasYAgregarElementos(xmlDoc, listaProductos, movimiento);


            decimal retiene = movimiento.Retiene;

            // Obtener el elemento WithholdingTaxTotal si existe
            var withholdingTaxTotalElement = xmlDoc.Descendants(cac + "WithholdingTaxTotal").FirstOrDefault();

            // Verificar si retiene es igual a 0.00 y si existe el elemento WithholdingTaxTotal
            if (retiene == 0.00m && withholdingTaxTotalElement != null)
            {
                // Eliminar el elemento WithholdingTaxTotal si retiene es igual a 0.00
                withholdingTaxTotalElement.Remove();
            }
            else if (retiene != 0.00m)
            {
                // Reemplazar los valores del elemento WithholdingTaxTotal si retiene es diferente de 0.00
                withholdingTaxTotalElement?.Element(cbc + "TaxAmount")?.SetValue(retiene.ToString("F2", CultureInfo.InvariantCulture));
                withholdingTaxTotalElement?.Element(cac + "TaxSubtotal")?.Element(cbc + "TaxableAmount")?.SetValue(movimiento.Valor_neto);
                withholdingTaxTotalElement?.Element(cac + "TaxSubtotal")?.Element(cbc + "TaxAmount")?.SetValue(retiene.ToString("F2", CultureInfo.InvariantCulture));
                withholdingTaxTotalElement?.Element(cac + "TaxSubtotal")?.Element(cac + "TaxCategory")?.Element(cbc + "Percent")?.SetValue("3.50");
                withholdingTaxTotalElement?.Element(cac + "TaxSubtotal")?.Element(cac + "TaxCategory")?.Element(cac + "TaxScheme")?.Element(cbc + "ID")?.SetValue("06");
                withholdingTaxTotalElement?.Element(cac + "TaxSubtotal")?.Element(cac + "TaxCategory")?.Element(cac + "TaxScheme")?.Element(cbc + "Name")?.SetValue("ReteFuente");

                // Si el elemento WithholdingTaxTotal no existe, crear uno nuevo
                if (withholdingTaxTotalElement == null)
                {
                    withholdingTaxTotalElement = new XElement(cac + "WithholdingTaxTotal",
                        new XElement(cbc + "TaxAmount", retiene.ToString("F2", CultureInfo.InvariantCulture)),
                        new XElement(cac + "TaxSubtotal",
                            new XElement(cbc + "TaxableAmount", "3361344.00"),
                            new XElement(cbc + "TaxAmount", "84033.60"),
                            new XElement(cac + "TaxCategory",
                                new XElement(cbc + "Percent", "3.50"),
                                new XElement(cac + "TaxScheme",
                                    new XElement(cbc + "ID", "06"),
                                    new XElement(cbc + "Name", "ReteFuente")
                                )
                            )
                        )
                    );

                    // Agregar el elemento WithholdingTaxTotal al XML
                    xmlDoc.Root?.Add(withholdingTaxTotalElement);
                }
            }


            var legalMonetaryTotalElement = xmlDoc.Descendants(cac + "LegalMonetaryTotal").FirstOrDefault();
            if (legalMonetaryTotalElement != null)
            {
                legalMonetaryTotalElement.Element(cbc + "LineExtensionAmount")?.SetValue(movimiento.Valor_neto); // Total Valor Bruto antes de tributos
                legalMonetaryTotalElement.Element(cbc + "TaxExclusiveAmount")?.SetValue(movimiento.Valor_neto); // Total Valor Base Imponible
                legalMonetaryTotalElement.Element(cbc + "TaxInclusiveAmount")?.SetValue(movimiento.Valor); // Total Valor Bruto más tributos
                legalMonetaryTotalElement.Element(cbc + "PayableAmount")?.SetValue(movimiento.Valor); // Total Valor a Pagar // cufe ValTot
            }

            GenerarProductos.MapCreditNoteLine(xmlDoc, listaProductos, movimiento); // Llamada a la función para mapear la información de InvoiceLine

            // Buscar el elemento <DATA> dentro del elemento <CreditNote> con el espacio de nombres completo
            XNamespace creditNoteNs = "urn:oasis:names:specification:ubl:schema:xsd:CreditNote-2";
            var dataElement = xmlDoc.Descendants(creditNoteNs + "DATA").FirstOrDefault();

            // Verificar si se encontró el elemento <DATA>
            if (dataElement != null)
            {
                // Modificar los elementos dentro de <DATA>
                dataElement.Element(creditNoteNs + "UBL21")?.SetValue("true");

                // Buscar el elemento <Partnership> dentro de <DATA> con el espacio de nombres completo
                var partnershipElement = dataElement.Descendants(creditNoteNs + "Partnership").FirstOrDefault();
                if (partnershipElement != null)
                {
                    partnershipElement.Element(creditNoteNs + "ID")?.SetValue("900770401");
                    partnershipElement.Element(creditNoteNs + "TechKey")?.SetValue("fc8eac422eba16e22ffd8c6f94b3f40a6e38162c"); // pregunta 
                    partnershipElement.Element(creditNoteNs + "SetTestID")?.SetValue("e84ce8bd-5bc9-434c-bc0e-4e34454a45a5"); // pregunta 
                }
            }
            return (cadenaConexion, cufe);

        }

        }
    }
