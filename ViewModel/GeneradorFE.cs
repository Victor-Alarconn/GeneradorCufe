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
    public class GeneradorFE
    {

        public static (string CUFE, List<Productos> listaProductos, Adquiriente adquiriente, Movimiento movimiento, Encabezado encabezado) UpdateXmlWithViewModelData(XDocument xmlDoc, Emisor emisor, Factura factura, string cadenaConexion)
        {
            try
            {
            
                XNamespace sts = "dian:gov:co:facturaelectronica:Structures-2-1";
                XNamespace cbc = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2";
                XNamespace cac = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2";

                // Crear una instancia de la clase Productos_Consulta
                Productos_Consulta productosConsulta = new Productos_Consulta();
                Encabezado_Consulta encabezadoConsulta = new Encabezado_Consulta();
                Codigos_Consulta codigosConsulta = new Codigos_Consulta();
                Movimiento_Consulta movimientoConsulta = new Movimiento_Consulta();
                FormaPago_Consulta formaPagoConsulta = new FormaPago_Consulta();
                Adquiriente_Consulta adquirienteConsulta = new Adquiriente_Consulta();

                // Llamar al método ConsultarProductosPorFactura para obtener la lista de productos
                Encabezado encabezado = encabezadoConsulta.ConsultarEncabezado(factura, cadenaConexion);
                Movimiento movimiento = movimientoConsulta.ConsultarValoresTotales(factura, cadenaConexion);
                List<Productos> listaProductos = productosConsulta.ConsultarProductosPorFactura(factura, cadenaConexion);
                List<FormaPago> listaFormaPago = formaPagoConsulta.ConsultarFormaPago(factura, cadenaConexion);

                // Obtener la hora en formato DateTimeOffset
                DateTimeOffset horaConDesplazamiento = DateTimeOffset.ParseExact(movimiento.Hora_dig, "HH:mm:ss", CultureInfo.InvariantCulture);

                // Agregar el desplazamiento horario
                string horaformateada = horaConDesplazamiento.ToString("HH:mm:sszzz", CultureInfo.InvariantCulture);

                string nitCompleto = emisor.Nit_emisor ?? "";
                string[] partesNit = nitCompleto.Split('-');
                string Nit = partesNit.Length > 0 ? partesNit[0] : ""; // Obtiene la parte antes del guion
                string Dv = partesNit.Length > 1 ? partesNit[1] : ""; // Obtiene el dígito verificador después del guion

                string construir = GeneradorCufe_Cude.ConstruirCadenaCUFE(movimiento, listaProductos, factura, horaformateada, Nit, emisor, encabezado); // se puede resumir 
                string CUFE = GeneradorCufe_Cude.GenerarCUFE(construir);



                string nota = $"Factura de Venta Emitida por {nitCompleto}-{emisor.Nombre_emisor}";


                xmlDoc.Descendants(sts + "InvoiceAuthorization").FirstOrDefault()?.SetValue(encabezado.Autorizando);
                xmlDoc.Descendants(cbc + "StartDate").FirstOrDefault()?.SetValue(encabezado.Fecha_inicio.ToString("yyyy-MM-dd"));
                xmlDoc.Descendants(cbc + "EndDate").FirstOrDefault()?.SetValue(encabezado.Fecha_termina.ToString("yyyy-MM-dd"));

                // Actualizaciones para 'AuthorizedInvoices'
                var authorizedInvoicesElement = xmlDoc.Descendants(sts + "AuthorizedInvoices").FirstOrDefault();
                if (authorizedInvoicesElement != null)
                {
                    authorizedInvoicesElement.Element(sts + "Prefix")?.SetValue(encabezado.Prefijo);
                    authorizedInvoicesElement.Element(sts + "From")?.SetValue(encabezado.R_inicio);
                    authorizedInvoicesElement.Element(sts + "To")?.SetValue(encabezado.R_termina);
                }
                DateTimeOffset now = DateTimeOffset.Now;
                xmlDoc.Descendants(cbc + "CustomizationID").FirstOrDefault()?.SetValue("10"); // Indicador del tipo de operación (10 para facturación electrónica)
                                                                                              // Obtener el valor del perfil de ejecución del XML
                string perfilEjecucionID = emisor.Url_emisor.Equals("docum", StringComparison.OrdinalIgnoreCase) ? "1" : "2";

                // Actualizar el valor del perfil de ejecución en el XML
                xmlDoc.Descendants(cbc + "ProfileExecutionID").FirstOrDefault()?.SetValue(perfilEjecucionID);

                xmlDoc.Descendants(cbc + "ID").FirstOrDefault()?.SetValue(factura.Facturas);
                xmlDoc.Descendants(cbc + "UUID").FirstOrDefault()?.SetValue(CUFE);
                xmlDoc.Descendants(cbc + "UUID").Attributes("schemeID").FirstOrDefault()?.SetValue(perfilEjecucionID);
                DateTime fechaFactura = movimiento.Fecha_Factura;
                DateTime fechaHoy = DateTime.Today;

                if ((fechaHoy - fechaFactura).TotalDays > 2)
                {
                    xmlDoc.Descendants(cbc + "IssueDate").FirstOrDefault()?.SetValue(fechaFactura.ToString("yyyy-MM-dd"));
                }
                else
                {
                    xmlDoc.Descendants(cbc + "IssueDate").FirstOrDefault()?.SetValue(fechaHoy.ToString("yyyy-MM-dd"));
                }

                xmlDoc.Descendants(cbc + "IssueTime").FirstOrDefault()?.SetValue(horaformateada);
                xmlDoc.Descendants(cbc + "InvoiceTypeCode").FirstOrDefault()?.SetValue("01"); 
                                                                                             
                var nuevaNotaElement = new XElement(cbc + "Note");
                // Verificar si se cumple la condición para agregar una nota adicional
                if (movimiento.Retiene > 0.00m && emisor.Retiene_emisor == 3)
                {
                    // Agregar una nota adicional
                    string notaAdicional = " Retencion del " + movimiento.Retiene + " del % 3.5";

                    // Establecer el valor de la nota adicional en el nuevo elemento Note
                    nuevaNotaElement.SetValue(notaAdicional);

                    // Agregar el nuevo elemento Note al XML
                    xmlDoc.Descendants(cbc + "Note").FirstOrDefault()?.AddAfterSelf(nuevaNotaElement);
                }

                // Establecer la nota original

                xmlDoc.Descendants(cbc + "Note").FirstOrDefault()?.SetValue(nota);
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


                string ciudadCompleta = emisor.Ciudad_emisor ?? ""; // se cambia por el codigo de la ciudad
                string[] partesCiudad = ciudadCompleta.Split(',');

                Codigos codigos = codigosConsulta.ConsultarCodigos(ciudadCompleta); // Consulta para obtener los códigos de ciudad y departamento

                GenerarEmisor.MapearInformacionEmisor(xmlDoc, emisor, encabezado, codigos, listaProductos);  // Información del emisor

                string nitValue = listaProductos[0].Nit;
                Adquiriente adquiriente = adquirienteConsulta.ConsultarAdquiriente(nitValue, cadenaConexion);
                GenerarAdquiriente.MapAccountingCustomerParty(xmlDoc, nitValue, cadenaConexion, adquiriente, codigos); // Información del adquiriente


                // Información del medio de pago
                //var paymentMeansElement = xmlDoc.Descendants(cac + "PaymentMeans").FirstOrDefault();
                //if (paymentMeansElement != null)
                //{
                //    paymentMeansElement.Element(cbc + "ID")?.SetValue("1");
                //    paymentMeansElement.Element(cbc + "PaymentMeansCode")?.SetValue("10");
                //    paymentMeansElement.Element(cbc + "PaymentID")?.SetValue("Efectivo");
                //}

                emisor.Codigo_FormaPago_emisor = GenerarFormasPago.GenerarFormaPagos(xmlDoc, listaFormaPago, movimiento.Dias);
                GenerarIvas.GenerarIvasYAgregarElementos(xmlDoc, listaProductos, movimiento); // Calcular el total del IVA de todos los productos


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

                    // Calcular el porcentaje de retención y formatearlo a "2.50"
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


                decimal Valor = movimiento.Retiene != 0 && emisor.Retiene_emisor == 2 ? Math.Round(movimiento.Valor + movimiento.Retiene, 2) : movimiento.Valor;

                decimal ValorNeto = movimiento.Valor_neto == 0.00m ? Math.Round(listaProductos.Sum(producto => producto.Neto), 2) : movimiento.Valor_neto;

                var legalMonetaryTotalElement = xmlDoc.Descendants(cac + "LegalMonetaryTotal").FirstOrDefault();
                if (legalMonetaryTotalElement != null)
                {
                    legalMonetaryTotalElement.Element(cbc + "LineExtensionAmount")?.SetValue(ValorNeto); // Total Valor Bruto antes de tributos 
                    legalMonetaryTotalElement.Element(cbc + "TaxExclusiveAmount")?.SetValue(ValorNeto); // Total Valor Base Imponible
                    legalMonetaryTotalElement.Element(cbc + "TaxInclusiveAmount")?.SetValue(Valor); // Total Valor Bruto más tributos
                    legalMonetaryTotalElement.Element(cbc + "PayableAmount")?.SetValue(Valor); // Total Valor a Pagar // cufe ValTot
                }


                if (factura.Recibo != "0" && !string.IsNullOrEmpty(factura.Recibo))
                {

                }

                GenerarProductos.MapInvoiceLine(xmlDoc, listaProductos, movimiento); // Llamada a la función para mapear la información de InvoiceLine


                XNamespace invoiceNs = "urn:oasis:names:specification:ubl:schema:xsd:Invoice-2";
                var dataElement = xmlDoc.Descendants(invoiceNs + "DATA").FirstOrDefault();

                if (dataElement != null)
                {
                    // Modificar los elementos dentro de <DATA>
                    dataElement.Element(invoiceNs + "UBL21")?.SetValue("true");

                    // Buscar el elemento <Partnership> dentro de <DATA> con el espacio de nombres completo
                    var partnershipElement = dataElement.Descendants(invoiceNs + "Partnership").FirstOrDefault();
                    if (partnershipElement != null)
                    {
                        partnershipElement.Element(invoiceNs + "ID")?.SetValue("900770401");
                        partnershipElement.Element(invoiceNs + "TechKey")?.SetValue(encabezado.Llave_tecnica);
                        if (emisor.Url_emisor.Equals("docum", StringComparison.OrdinalIgnoreCase))
                        {
                            partnershipElement.Element(invoiceNs + "SetTestID")?.Remove();
                        }
                        else
                        {
                            partnershipElement.Element(invoiceNs + "SetTestID")?.SetValue("e84ce8bd-5bc9-434c-bc0e-4e34454a45a5");
                        }


                    }
                }

                // Retornar la cadena de conexión
                return (CUFE, listaProductos, adquiriente, movimiento, encabezado);
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
