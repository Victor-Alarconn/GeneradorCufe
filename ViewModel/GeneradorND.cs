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
    public class GeneradorND
    {
        public static (string CUFE, List<Productos> listaProductos, Adquiriente adquiriente, Movimiento movimiento, Encabezado encabezado) GeneradorNotaDebito(XDocument xmlDoc, Emisor emisor, Factura factura, string cadenaConexion)
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
                string PrefijoNC = "ND" + factura.Recibo;


                // Llamar al método ConsultarProductosPorFactura para obtener la lista de productos
                Encabezado encabezado = encabezadoConsulta.ConsultarEncabezado(factura, cadenaConexion);
                Movimiento movimiento = movimientoConsulta.ConsultarValoresTotales(factura, cadenaConexion);
                List<Productos> listaProductos = productosConsulta.ConsultarProductosDebito(factura, cadenaConexion);
                List<FormaPago> listaFormaPago = formaPagoConsulta.ConsultarFormaPago(factura, cadenaConexion);
                string cufe = "";
                string hora = "";
                string nitCompleto = emisor.Nit_emisor ?? "";
                string[] partesNit = nitCompleto.Split('-');
                string Nit = partesNit.Length > 0 ? partesNit[0] : ""; // Obtiene la parte antes del guion
                string Dv = partesNit.Length > 1 ? partesNit[1] : ""; // Obtiene el dígito verificador después del guion
                if (string.IsNullOrEmpty(movimiento.Dato_Cufe) || movimiento.Dato_Cufe == "0")
                {
                    // Consultar los valores totales para la construcción del CUFE
                    movimiento = movimientoConsulta.ConsultarValoresTotales(factura, cadenaConexion);
                    List<Productos> listaProductosCufe = productosConsulta.ConsultarProductosPorFactura(factura, cadenaConexion);

                    DateTimeOffset horaCon = DateTimeOffset.ParseExact(movimiento.Hora_dig, "HH:mm:ss", CultureInfo.InvariantCulture);
                    hora = horaCon.ToString("HH:mm:sszzz", CultureInfo.InvariantCulture);

                    // Construir el CUFE
                    string construir = GeneradorCufe_Cude.ConstruirCadenaCUFE(movimiento, listaProductosCufe, factura, hora, Nit, emisor, encabezado);
                    cufe = GeneradorCufe_Cude.GenerarCUFE(construir);
                }
                else
                {
                    cufe = movimiento.Dato_Cufe;
                }
                
                    movimiento = new Movimiento
                    {
                        Nit = listaProductos.First().Nit,
                        Valor = listaProductos.First().Valor, // Asigna los valores manualmente según sea necesario
                        Valor_iva = listaProductos.First().IvaTotal,
                        Valor_dsto = 0.00M,
                        Valor_neto = listaProductos.First().Valor,
                        Exentas = 0.00M,
                        Fecha_Factura = listaProductos.First().Fecha, 
                        Hora_dig = listaProductos.First().Hora_Digitada,
                        Retiene = 0.00M,
                        Ipoconsumo = 0.00M,
                        Numero_bolsa = 0,
                        Valor_bolsa = 0,
                        Dato_Cufe = string.Empty,
                        Nota_credito = listaProductos.First().Valor,
                        Numero = string.Empty,
                        Vendedor = string.Empty,
                        Dias = 0
                    };



                string horaProducto = listaProductos.First().Hora_Digitada; // Aquí supongo que hay un atributo "Hora" en tu objeto Producto
                DateTimeOffset horaConDesplazamiento = DateTimeOffset.ParseExact(horaProducto, "HH:mm:ss", CultureInfo.InvariantCulture);
                string horaformateada = horaConDesplazamiento.ToString("HH:mm:sszzz", CultureInfo.InvariantCulture);


                string construirCUDE = GeneradorCufe_Cude.ConstruirCadenaCUDE(movimiento, listaProductos, factura, horaformateada, Nit, emisor, hora, PrefijoNC);
                emisor.cude = GeneradorCufe_Cude.GenerarCUFE(construirCUDE);
               


                // Actualizar 'CustomizationID'
                xmlDoc.Descendants(cbc + "CustomizationID").FirstOrDefault()?.SetValue("30"); // 32 o sin referencia a facturas

                string perfilEjecucionID = emisor.Url_emisor.Equals("docum", StringComparison.OrdinalIgnoreCase) ? "1" : "2";

                // Actualizar 'ProfileExecutionID'
                xmlDoc.Descendants(cbc + "ProfileExecutionID").FirstOrDefault()?.SetValue(perfilEjecucionID); // 1 produccion , 2 pruebas

                // Actualizar 'ID'
                xmlDoc.Descendants(cbc + "ID").FirstOrDefault()?.SetValue(PrefijoNC);

                // Actualizar 'UUID'
                xmlDoc.Descendants(cbc + "UUID").FirstOrDefault()?.SetValue(emisor.cude);
                xmlDoc.Descendants(cbc + "UUID").FirstOrDefault()?.SetAttributeValue("schemeID", perfilEjecucionID);
                xmlDoc.Descendants(cbc + "UUID").FirstOrDefault()?.SetAttributeValue("schemeName", "CUDE-SHA384");

                // Actualizar 'IssueDate' y 'IssueTime'
                DateTime fechaProducto = listaProductos.FirstOrDefault()?.Fecha ?? DateTime.Today;
                DateTime fechaHoy = DateTime.Today;

                if ((fechaHoy - fechaProducto).TotalDays < 14)
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
                xmlDoc.Descendants(cbc + "LineCountNumeric").FirstOrDefault()?.SetValue(listaProductos.Count);
                

                // Actualizar 'DiscrepancyResponse'
                var discrepancyResponseElement = xmlDoc.Descendants(cac + "DiscrepancyResponse").FirstOrDefault();
                if (discrepancyResponseElement != null)
                {
                    discrepancyResponseElement.Element(cbc + "ReferenceID")?.SetValue("Sección de la factura la cual se le aplica la correción victor");
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

                string nitValue = listaProductos[0].Nit;

                Adquiriente adquiriente = adquirienteConsulta.ConsultarAdquiriente(nitValue, cadenaConexion);
                GenerarAdquiriente.MapAccountingCustomerParty(xmlDoc, nitValue, cadenaConexion, adquiriente, codigos);

                emisor.Codigo_FormaPago_emisor = GenerarFormasPago.GenerarFormaPagos(xmlDoc, listaFormaPago, movimiento.Dias);

                // Calcular el total del IVA de todos los productos
                GenerarIvas.GenerarIvasYAgregarElementos(xmlDoc, listaProductos, movimiento);


                decimal retiene = movimiento.Retiene;

                // Obtener el elemento WithholdingTaxTotal si existe
                var withholdingTaxTotalElement = xmlDoc.Descendants(cac + "WithholdingTaxTotal").FirstOrDefault();

                // Verificar si retiene es mayor que 0.00 y si el movimiento.Retiene es igual a 2
                if (retiene > 0.00m && emisor.Retiene_emisor == 2)
                {
                    // Reemplazar los valores del elemento WithholdingTaxTotal si retiene es mayor que 0.00
                    withholdingTaxTotalElement?.Element(cbc + "TaxAmount")?.SetValue(retiene.ToString("F2", CultureInfo.InvariantCulture));
                    withholdingTaxTotalElement?.Element(cac + "TaxSubtotal")?.Element(cbc + "TaxableAmount")?.SetValue(movimiento.Valor_neto);
                    withholdingTaxTotalElement?.Element(cac + "TaxSubtotal")?.Element(cbc + "TaxAmount")?.SetValue(retiene.ToString("F2", CultureInfo.InvariantCulture));

                    decimal porcentajeRetencion = (retiene / movimiento.Valor_neto) * 100;
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

                if (emisor.Retiene_emisor == 2 && movimiento.Retiene != 0) // falta calcular el valor 
                {
                    Valor = Math.Round(movimiento.Nota_credito + 0, 2);
                }
                else
                {
                    Valor = movimiento.Valor_neto;
                }

                decimal VlrNeto = Math.Round(listaProductos.Sum(p => p.Valor), 2);

                var legalMonetaryTotalElement = xmlDoc.Descendants(cac + "RequestedMonetaryTotal").FirstOrDefault();
                if (legalMonetaryTotalElement != null)
                {
                    legalMonetaryTotalElement.Element(cbc + "LineExtensionAmount")?.SetValue(VlrNeto); // Total Valor Bruto antes de tributos
                    legalMonetaryTotalElement.Element(cbc + "TaxExclusiveAmount")?.SetValue(VlrNeto); // Total Valor Base Imponible
                    legalMonetaryTotalElement.Element(cbc + "TaxInclusiveAmount")?.SetValue(Valor); // Total Valor Bruto más tributos
                    legalMonetaryTotalElement.Element(cbc + "PayableAmount")?.SetValue(Valor); // Total Valor a Pagar // cufe ValTot
                }

                GenerarProductos.MapDebitNoteLine(xmlDoc, listaProductos, movimiento); // Llamada a la función para mapear la información de InvoiceLine

                // Buscar el elemento <DATA> dentro del elemento <DebitNote> con el espacio de nombres completo
                XNamespace DebitNotes = "urn:oasis:names:specification:ubl:schema:xsd:DebitNote-2";
                var dataElement = xmlDoc.Descendants(DebitNotes + "DATA").FirstOrDefault();

                if (dataElement != null)
                {
                    dataElement.Element(DebitNotes + "UBL21")?.SetValue("true");

                    // Buscar el elemento <Partnership> dentro de <DATA> con el espacio de nombres completo
                    var partnershipElement = dataElement.Descendants(DebitNotes + "Partnership").FirstOrDefault();
                    if (partnershipElement != null)
                    {
                        partnershipElement.Element(DebitNotes + "ID")?.SetValue("900770401");
                        partnershipElement.Element(DebitNotes + "TechKey")?.SetValue(encabezado.Llave_tecnica);
                        if (emisor.Url_emisor.Equals("docum", StringComparison.OrdinalIgnoreCase))
                        {
                            partnershipElement.Element(DebitNotes + "SetTestID")?.Remove();
                        }
                        else
                        {
                            partnershipElement.Element(DebitNotes + "SetTestID")?.SetValue("e84ce8bd-5bc9-434c-bc0e-4e34454a45a5");
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
