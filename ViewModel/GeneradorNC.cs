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
        public static (string CUFE, List<Productos> listaProductos, Adquiriente adquiriente, Movimiento movimiento, Encabezado encabezado) GeneradorNotaCredito(XDocument xmlDoc, Emisor emisor, Factura factura)
        {
            try
            {

                XNamespace cbc = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2";
                XNamespace cac = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2";

                Productos_Consulta productosConsulta = new Productos_Consulta();
                Encabezado_Consulta encabezadoConsulta = new Encabezado_Consulta();
                Codigos_Consulta codigosConsulta = new Codigos_Consulta();
                Movimiento_Consulta movimientoConsulta = new Movimiento_Consulta();
                FormaPago_Consulta formaPagoConsulta = new FormaPago_Consulta();
                Adquiriente_Consulta adquirienteConsulta = new Adquiriente_Consulta();
                string PrefijoNC = "NC" + factura.Recibo;
                string cadenaConexion="";

                // Llamar al método ConsultarProductosPorFactura para obtener la lista de productos
                Encabezado encabezado = encabezadoConsulta.ConsultarEncabezado(factura, cadenaConexion);
                Movimiento movimiento = movimientoConsulta.ConsultarValoresTotalesNC(factura, cadenaConexion);
                List<Productos> listaProductos = productosConsulta.ConsultarProductosNota(factura, cadenaConexion);
                List<FormaPago> listaFormaPago = formaPagoConsulta.ConsultarFormaPago(factura, cadenaConexion);

                string nitValue = listaProductos[0].Nit;

                Adquiriente adquiriente = adquirienteConsulta.ConsultarAdquiriente(nitValue, cadenaConexion, factura);

                string horaProducto = listaProductos.First().Hora_Digitada; // Aquí supongo que hay un atributo "Hora" en tu objeto Producto
                DateTimeOffset horaConDesplazamiento = DateTimeOffset.ParseExact(horaProducto, "HH:mm:ss", CultureInfo.InvariantCulture);
                string horaformateada = horaConDesplazamiento.ToString("HH:mm:sszzz", CultureInfo.InvariantCulture);

                string nitCompleto = emisor.Nit_emisor ?? "";
                string[] partesNit = nitCompleto.Split('-');
                string Nit = partesNit.Length > 0 ? partesNit[0] : ""; // Obtiene la parte antes del guion
                string Dv = partesNit.Length > 1 ? partesNit[1] : ""; // Obtiene el dígito verificador después del guion

                string cufe;
                string hora = "";

                movimiento.Nit = adquiriente.Nit_adqui;

                string construirCUDE = GeneradorCufe_Cude.ConstruirCadenaCUDE(movimiento, listaProductos, factura, horaformateada, Nit, emisor, hora, PrefijoNC);
                string cude = GeneradorCufe_Cude.GenerarCUFE(construirCUDE);
                emisor.cude = cude;
                if (string.IsNullOrEmpty(movimiento.Dato_Cufe) || movimiento.Dato_Cufe == "0")
                {
                    // Consultar los valores totales para la construcción del CUFE
                    Movimiento movimiento1 = movimientoConsulta.ConsultarValoresTotales(factura, cadenaConexion);
                    List<Productos> listaProductosCufe1 = productosConsulta.ConsultarProductosPorFactura(factura, cadenaConexion);

                    string horaDig = movimiento.Hora_dig ?? "00:00:00";
                    DateTimeOffset horaCon = DateTimeOffset.ParseExact(horaDig, "HH:mm:ss", CultureInfo.InvariantCulture);
                    hora = horaCon.ToString("HH:mm:sszzz", CultureInfo.InvariantCulture);

                    // Construir el CUFE
                    string construir = GeneradorCufe_Cude.ConstruirCadenaCUFE(movimiento1, listaProductosCufe1, factura, hora, Nit, emisor, encabezado);
                    cufe = GeneradorCufe_Cude.GenerarCUFE(construir);
                }
                else
                {
                    cufe = movimiento.Dato_Cufe;
                }

               

                // Actualizar 'CustomizationID'
                xmlDoc.Descendants(cbc + "CustomizationID").FirstOrDefault()?.SetValue("20"); // 22 o sin referencia a facturas

                string perfilEjecucionID = emisor.Url_emisor.Equals("docum", StringComparison.OrdinalIgnoreCase) ? "1" : "2";

                // Actualizar 'ProfileExecutionID'
                xmlDoc.Descendants(cbc + "ProfileExecutionID").FirstOrDefault()?.SetValue(perfilEjecucionID); // 1 produccion , 2 pruebas
                xmlDoc.Descendants(cbc + "ID").FirstOrDefault()?.SetValue(PrefijoNC);
                xmlDoc.Descendants(cbc + "UUID").FirstOrDefault()?.SetValue(cude);
                xmlDoc.Descendants(cbc + "UUID").FirstOrDefault()?.SetAttributeValue("schemeID", perfilEjecucionID);
                xmlDoc.Descendants(cbc + "UUID").FirstOrDefault()?.SetAttributeValue("schemeName", "CUDE-SHA384");

                // Actualizar 'IssueDate' y 'IssueTime'
                DateTime fechaProducto = listaProductos.FirstOrDefault()?.Fecha ?? DateTime.Today;
                DateTime fechaHoy = DateTime.Today;

                if ((fechaHoy - fechaProducto).TotalDays < 30)
                {
                    fechaProducto = DateTime.Today;
                }

                string fechaProductoString = fechaProducto.ToString("yyyy-MM-dd");

                xmlDoc.Descendants(cbc + "IssueDate").FirstOrDefault()?.SetValue(fechaProductoString);
                xmlDoc.Descendants(cbc + "IssueTime").FirstOrDefault()?.SetValue(horaformateada);


                // Actualizar 'CreditNoteTypeCode'
                xmlDoc.Descendants(cbc + "CreditNoteTypeCode").FirstOrDefault()?.SetValue("91");
                string nota = $"Nota Credito Emitida por {nitCompleto}-{emisor.Nombre_emisor}";

                // Actualizar 'Note'
                xmlDoc.Descendants(cbc + "Note").FirstOrDefault()?.SetValue(nota);

                // Actualizar 'DocumentCurrencyCode'
                xmlDoc.Descendants(cbc + "DocumentCurrencyCode").FirstOrDefault()?.SetValue("COP");

                // Verificar si movimiento.Numero_bolsa tiene un valor diferente de 0
                if (movimiento.Numero_bolsa != 0)
                {
                    // Sumar 1 al conteo de productos si hay un valor en movimiento.Numero_bolsa
                    xmlDoc.Descendants(cbc + "LineCountNumeric").FirstOrDefault()?.SetValue(listaProductos.Count + 1);
                }
                else
                {
                    // Establecer el conteo de productos sin sumar 1 si movimiento.Numero_bolsa es 0
                    xmlDoc.Descendants(cbc + "LineCountNumeric").FirstOrDefault()?.SetValue(listaProductos.Count);
                }

                // Actualizar 'DiscrepancyResponse'
                var discrepancyResponseElement = xmlDoc.Descendants(cac + "DiscrepancyResponse").FirstOrDefault();
                if (discrepancyResponseElement != null)
                {
                    discrepancyResponseElement.Element(cbc + "ReferenceID")?.SetValue("Sección de la factura la cual se le aplica la correción");
                    discrepancyResponseElement.Element(cbc + "ResponseCode")?.SetValue("2"); // se puede cmabiar 
                    discrepancyResponseElement.Element(cbc + "Description")?.SetValue("Anulación de factura electrónica");
                }

                // Actualizar 'BillingReference'
                var billingReferenceElement = xmlDoc.Descendants(cac + "BillingReference").FirstOrDefault();
                if (billingReferenceElement != null)
                {
                    var invoiceDocumentReferenceElement = billingReferenceElement.Element(cac + "InvoiceDocumentReference");
                    if (invoiceDocumentReferenceElement != null)
                    {
                        invoiceDocumentReferenceElement.Element(cbc + "ID")?.SetValue(factura.Facturas);
                        invoiceDocumentReferenceElement.Element(cbc + "UUID")?.SetValue(cufe);
                        invoiceDocumentReferenceElement.Element(cbc + "UUID")?.SetAttributeValue("schemeName", "CUFE-SHA384");
                        invoiceDocumentReferenceElement.Element(cbc + "IssueDate")?.SetValue(movimiento.Fecha_Factura.ToString("yyyy-MM-dd"));
                    }
                }

                string ciudadCompleta = emisor.Ciudad_emisor ?? "";
                string[] partesCiudad = ciudadCompleta.Split(',');
                string Municipio = partesCiudad.Length > 0 ? partesCiudad[0].Trim() : ""; // Obtiene el municipio (primer elemento después de dividir)
                string Departamento = partesCiudad.Length > 1 ? partesCiudad[1].Trim() : ""; // Obtiene el departamento (segundo elemento después de dividir)
                Codigos codigos = codigosConsulta.ConsultarCodigos(ciudadCompleta);

                GenerarEmisor.MapearInformacionEmisor(xmlDoc, emisor, encabezado, codigos, listaProductos, factura);

               
                GenerarAdquiriente.MapAccountingCustomerParty(xmlDoc, nitValue, cadenaConexion, adquiriente, codigos);

                emisor.Codigo_FormaPago_emisor = GenerarFormasPago.GenerarFormaPagos(xmlDoc, listaFormaPago, movimiento.Dias);

                // Calcular el total del IVA de todos los productos
                GenerarIvas.GenerarIvasYAgregarElementos(xmlDoc, listaProductos, movimiento);

                decimal retiene = movimiento.Retiene;

                // Obtener el elemento WithholdingTaxTotal si existe
                var withholdingTaxTotalElement = xmlDoc.Descendants(cac + "WithholdingTaxTotal").FirstOrDefault();
                decimal ValorNeto = movimiento.Valor_neto == 0.00m ? Math.Round(listaProductos.Sum(producto => producto.Neto), 2) : movimiento.Valor_neto;
                // Verificar si retiene es mayor que 0.00 y si el movimiento.Retiene es igual a 2
                if (retiene > 0.00m && emisor.Retiene_emisor == 2)
                {
                    // Reemplazar los valores del elemento WithholdingTaxTotal si retiene es mayor que 0.00
                    withholdingTaxTotalElement?.Element(cbc + "TaxAmount")?.SetValue(retiene.ToString("F2", CultureInfo.InvariantCulture));
                    withholdingTaxTotalElement?.Element(cac + "TaxSubtotal")?.Element(cbc + "TaxableAmount")?.SetValue(ValorNeto);
                    withholdingTaxTotalElement?.Element(cac + "TaxSubtotal")?.Element(cbc + "TaxAmount")?.SetValue(retiene.ToString("F2", CultureInfo.InvariantCulture));

                    decimal porcentajeRetencion = (retiene / ValorNeto) * 100;
                    string porcentajeFormateado = porcentajeRetencion.ToString("F2", CultureInfo.InvariantCulture);

                    withholdingTaxTotalElement?.Element(cac + "TaxSubtotal")?.Element(cac + "TaxCategory")?.Element(cbc + "Percent")?.SetValue(porcentajeFormateado);
                    withholdingTaxTotalElement?.Element(cac + "TaxSubtotal")?.Element(cac + "TaxCategory")?.Element(cac + "TaxScheme")?.Element(cbc + "ID")?.SetValue("06");
                    withholdingTaxTotalElement?.Element(cac + "TaxSubtotal")?.Element(cac + "TaxCategory")?.Element(cac + "TaxScheme")?.Element(cbc + "Name")?.SetValue("ReteFuente");
                }
                else
                {
                    // Eliminar el elemento WithholdingTaxTotal si retiene no es mayor que 0.00 o si el movimiento.Retiene no es igual a 2
                    withholdingTaxTotalElement?.Remove();
                }

                decimal Valor;
                Valor = movimiento.Retiene != 0 && emisor.Retiene_emisor == 2 ? Math.Round(movimiento.Valor + movimiento.Retiene, 2) : movimiento.Valor;

                if (movimiento.Valor == 0.00m)
                {
                    Valor = Math.Round(listaProductos.Sum(producto => producto.Valor2), 2);
                }
               

                decimal Excluidos = Math.Round(listaProductos.Where(producto => producto.Excluido == 2).Sum(producto => producto.Neto), 2);
                decimal Exentos = Math.Round(listaProductos.Where(producto => producto.Excluido != 2).Sum(producto => producto.Neto), 2);

                var legalMonetaryTotalElement = xmlDoc.Descendants(cac + "LegalMonetaryTotal").FirstOrDefault();
                if (legalMonetaryTotalElement != null)
                {
                    legalMonetaryTotalElement.Element(cbc + "LineExtensionAmount")?.SetValue(ValorNeto.ToString("F2", CultureInfo.InvariantCulture)); // Total Valor Bruto antes de tributos 
                    legalMonetaryTotalElement.Element(cbc + "TaxExclusiveAmount")?.SetValue(Exentos.ToString("F2", CultureInfo.InvariantCulture)); // Total Valor Base Imponible
                    legalMonetaryTotalElement.Element(cbc + "TaxInclusiveAmount")?.SetValue(Valor.ToString("F2", CultureInfo.InvariantCulture)); // Total Valor Bruto más tributos
                    legalMonetaryTotalElement.Element(cbc + "PayableAmount")?.SetValue(Valor.ToString("F2", CultureInfo.InvariantCulture)); // Total Valor a Pagar // cufe ValTot
                }

                GenerarProductos.MapCreditNoteLine(xmlDoc, listaProductos, movimiento); // Llamada a la función para mapear la información de InvoiceLine

                // Buscar el elemento <DATA> dentro del elemento <CreditNote> con el espacio de nombres completo
                XNamespace creditNoteNs = "urn:oasis:names:specification:ubl:schema:xsd:CreditNote-2";
                var dataElement = xmlDoc.Descendants(creditNoteNs + "DATA").FirstOrDefault();

                if (dataElement != null)
                {
                    dataElement.Element(creditNoteNs + "UBL21")?.SetValue("true");

                    // Buscar el elemento <Partnership> dentro de <DATA> con el espacio de nombres completo
                    var partnershipElement = dataElement.Descendants(creditNoteNs + "Partnership").FirstOrDefault();
                    if (partnershipElement != null)
                    {
                        partnershipElement.Element(creditNoteNs + "ID")?.SetValue("900770401");
                        partnershipElement.Element(creditNoteNs + "TechKey")?.SetValue(encabezado.Llave_tecnica);
                        if (emisor.Url_emisor.Equals("docum", StringComparison.OrdinalIgnoreCase))
                        {
                            partnershipElement.Element(creditNoteNs + "SetTestID")?.Remove();
                        }
                        else
                        {
                            partnershipElement.Element(creditNoteNs + "SetTestID")?.SetValue("e84ce8bd-5bc9-434c-bc0e-4e34454a45a5");
                        }
                    }
                }
                return (cufe, listaProductos, adquiriente, movimiento, encabezado);
            }
            catch (Exception ex)
            {
                Factura_Consulta facturaConsulta = new Factura_Consulta();
                facturaConsulta.MarcarComoConError(factura, ex);
                return (null, null, null, null, null);
            }

        }

        }
    }
