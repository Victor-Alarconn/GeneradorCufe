
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

using iTextSharp.text;
using System.IO;
using iTextSharp.text.pdf;
using iTextSharp.tool.xml;
using Document = iTextSharp.text.Document;
using Paragraph = iTextSharp.text.Paragraph;
using iTextSharp.text.pdf.qrcode;

using Rectangle = iTextSharp.text.Rectangle;
using Font = iTextSharp.text.Font;
using System.Drawing;
using Image = iTextSharp.text.Image;
using System.Drawing.Imaging;
using GeneradorCufe.Model;
using System.Globalization;


namespace GeneradorCufe.ViewModel
{
    public class GeneradorPDF
    {
      
        public  GeneradorPDF()
        {
            
        }

        public static void CrearPDF(string rutaArchivo, Emisor emisor, Factura factura, List<Productos> listaProductos, string cufe, Adquiriente adquiriente, Movimiento movimiento, Encabezado encabezado1)
        {
            try
            {
                string PrefijoNC = "";
                if (!string.IsNullOrEmpty(factura.Recibo) && factura.Recibo != "0")
                {
                    PrefijoNC = "NC" + factura.Recibo;
                }

                    Document documento = new Document(PageSize.A4, 15, 18, 15, 18);
                PdfWriter.GetInstance(documento, new FileStream(rutaArchivo, FileMode.Create));
                documento.Open();

                // Encabezado
                PdfPTable encabezado = new PdfPTable(3);
                encabezado.HorizontalAlignment = Element.ALIGN_LEFT;
                encabezado.WidthPercentage = 100;
                encabezado.SetWidths(new float[] { 2, 4, 2 });
                encabezado.DefaultCell.Border = iTextSharp.text.Rectangle.NO_BORDER;

                string RutaLogo = "C:\\Users\\Programacion01\\source\\repos\\RepoVictor\\GeneradorCufe\\xml\\logo.png";
                if (File.Exists(RutaLogo))
                {
                    Image logo = Image.GetInstance(RutaLogo);
                    logo.ScaleAbsoluteWidth(120); // Ajustar el ancho de la imagen
                    logo.ScaleAbsoluteHeight(80); // Ajustar también la altura de la imagen si es necesario
                    logo.BackgroundColor = BaseColor.WHITE;
                    logo.Alignment = Element.ALIGN_MIDDLE;

                    // Agregar el logo en la primera columna (izquierda)
                    PdfPCell cLogo = new PdfPCell(logo);
                    cLogo.Border = Rectangle.NO_BORDER;
                    encabezado.AddCell(cLogo);
                }
                else
                {
                    PdfPCell cEmpty = new PdfPCell(new Phrase(" "))
                    {
                        Border = Rectangle.NO_BORDER
                    };
                    encabezado.AddCell(cEmpty);
                }

                // Datos del emisor (ficticios)
                string nombreEmisor = emisor.Nombre_emisor;
                string nitEmisor = emisor.Nit_emisor;
                string representanteEmisor = "Representante: JURIDICA - Responsable IVA";
                string direccionEmisor = "Dirección: " + emisor.Direccion_emisor;
                string correoEmisor = emisor.Correo_emisor;
                string telefonoEmisor = "Teléfono: " + emisor.Telefono_emisor;
                string factura1;
                if (!string.IsNullOrEmpty(factura.Recibo) && factura.Recibo != "0")
                {
                    PrefijoNC = "NC" + factura.Recibo;
                    factura1 = "Nro. Doc: " + PrefijoNC;
                }
                else
                {
                    factura1 = "Nro. Doc: " + factura.Facturas;
                }


                // Crear tabla para los datos del emisor
                var tDatosEmisor = new PdfPTable(1);
                tDatosEmisor.WidthPercentage = 100;
                tDatosEmisor.DefaultCell.Border = Rectangle.NO_BORDER;

                var fnt = FontFactory.GetFont("Helvetica", 9, Font.NORMAL);

                // Agregar cada dato del emisor en una sola celda
                var cDatosEmisor = new PdfPCell(new Phrase(
                    $"{nombreEmisor}\n" +
                    $"NIT: {nitEmisor}\n\n" +
                    $"{representanteEmisor}\n" +
                    $"{direccionEmisor}\n\n" +
                    $"{correoEmisor}\n\n" +
                    $"{telefonoEmisor}\n\n"+
                    $"{factura1}", fnt));

                cDatosEmisor.Border = Rectangle.NO_BORDER;
                cDatosEmisor.HorizontalAlignment = Element.ALIGN_CENTER;

                // Agregar celda de datos del emisor a la segunda columna
                encabezado.AddCell(cDatosEmisor);

                // Create a table for qr and general information
                PdfPTable table = new PdfPTable(1);
                table.WidthPercentage = 100;
                table.DefaultCell.Border = Rectangle.NO_BORDER;

                string nitCompleto = emisor.Nit_emisor ?? "";
                string[] partesNit = nitCompleto.Split('-');
                string NitFact = partesNit.Length > 0 ? partesNit[0] : "";

                // Obtener la hora en formato DateTimeOffset
                DateTimeOffset horaConDesplazamiento = DateTimeOffset.ParseExact(movimiento.Hora_dig, "HH:mm:ss", CultureInfo.InvariantCulture);

                // Agregar el desplazamiento horario
                string horaformateada = horaConDesplazamiento.ToString("HH:mm:sszzz", CultureInfo.InvariantCulture);
                string fechaFac = movimiento.Fecha_Factura.ToString("yyyy-MM-dd");

                // Construir el texto del código QR
                string TextoQR;
                if (!string.IsNullOrEmpty(factura.Recibo) && factura.Recibo != "0")
                {
                    PrefijoNC = "NC" + factura.Recibo;
                    TextoQR = $"PrefijoNC:{PrefijoNC}" +
                              $"FecFac:{fechaFac}" +
                              $"HorFac:{horaformateada}" +
                              $"NitFac:{NitFact}" +
                              $"DocAdq:{adquiriente.Nit_adqui}" +
                              $"ValFac:{movimiento.Valor_neto}" +
                              $"ValIva:{movimiento.Valor_iva}" +
                              $"ValOtroIm:0.00" +
                              $"ValTolFac:{movimiento.Valor}" +
                              $"CUDE:{emisor.Codigo_municipio_emisor}";
                }
                else
                {
                    TextoQR = $"NumFac:{factura.Facturas}" +
                              $"FecFac:{fechaFac}" +
                              $"HorFac:{horaformateada}" +
                              $"NitFac:{NitFact}" +
                              $"DocAdq:{adquiriente.Nit_adqui}" +
                              $"ValFac:{movimiento.Valor_neto}" +
                              $"ValIva:{movimiento.Valor_iva}" +
                              $"ValOtroIm:0.00" +
                              $"ValTolFac:{movimiento.Valor}" +
                              $"CUFE:{cufe}";
                }


                if (string.IsNullOrEmpty(TextoQR)) TextoQR = "TextoQR"; // Use a default value if the text is null or empty
                Image imageQr = CrearQR(TextoQR);
                imageQr.Border = Rectangle.NO_BORDER;
                imageQr.Alignment = Element.ALIGN_CENTER;
                imageQr.ScaleToFit(85, 85);
                PdfPCell qrCell = new PdfPCell();
                if (!string.IsNullOrEmpty(factura.Recibo) && factura.Recibo != "0")
                {
                    qrCell.AddElement(new Phrase("Nota Crédito", FontFactory.GetFont("Helvetica", 9, Font.BOLD)));
                    qrCell.HorizontalAlignment = Element.ALIGN_CENTER;
                }
                else
                {
                    qrCell.AddElement(new Phrase("Factura Electrónica De Venta", FontFactory.GetFont("Helvetica", 9, Font.BOLD)));
                }
                qrCell.AddElement(imageQr);
                qrCell.Border = Rectangle.NO_BORDER;
                qrCell.PaddingBottom = 1;
                qrCell.HorizontalAlignment = Element.ALIGN_CENTER;

                // Agregar qrCell a la tabla
                table.AddCell(qrCell);

                // Agregar tabla de qr y datos del emisor a la tercera columna (derecha)
                encabezado.AddCell(table);

                documento.Add(encabezado);


                // Segundo encabezado (Información del adquiriente)
                PdfPTable encabezado2 = new PdfPTable(3);
                encabezado2.HorizontalAlignment = Element.ALIGN_LEFT;
                encabezado2.WidthPercentage = 100;
                encabezado2.SetWidths(new float[] { 4f, 1f, 1f });
                encabezado2.SpacingAfter = 3;

                // Datos del adquiriente (ficticios)
                string nombreAdquiriente = adquiriente.Nombre_adqu;
                string identificacionAdquiriente = adquiriente.Nit_adqui;
                string direccionAdquiriente = adquiriente.Direccion_adqui;
                string correoAdquiriente = adquiriente.Correo_adqui;
                string telefonoAdquiriente = adquiriente.Telefono_adqui;
                string Orden = "Orden de Compra: " + movimiento.Numero;

                // Obtener el texto correspondiente al tipo de documento del adquiriente
                string tipoDocumentoTexto;
                switch (adquiriente.Tipo_doc)
                {
                    case "A":
                        tipoDocumentoTexto = "NIT";
                        break;
                    case "C":
                        tipoDocumentoTexto = "C.C";
                        break;
                    case "E":
                        tipoDocumentoTexto = "Ced.Extranj";
                        break;
                    case "I":
                        tipoDocumentoTexto = "Tarj.Ident";
                        break;
                    case "P":
                        tipoDocumentoTexto = "Pasaporte";
                        break;
                    default:
                        tipoDocumentoTexto = "DOCUMENTO DE IDENTIFICACIÓN";
                        break;
                }
                // Crear el texto del adquiriente con el tipo de documento y número de identificación en la misma línea
                string iAdquiriente = "Cliente: " + nombreAdquiriente + " - " + tipoDocumentoTexto + ": " + identificacionAdquiriente + "             " +
                                      "Medio de pago: Contado\n" +
                                      "DIRECCIÓN: " + direccionAdquiriente.ToUpper().Replace("\r\n", " ") +
                                      " " + adquiriente.Nombre_municipio_adqui + "     " +
                                       "Forma de pago: Transferencia Débito Bancaria\n" +
                                      "CORREO: " + correoAdquiriente + "                                  " +
                                      "Vendedor:" + movimiento.Vendedor + "\n" +
                                      "Telefono: " + telefonoAdquiriente +"\n" +
                                      Orden;



                PdfPCell cAdquiriente = new PdfPCell(new Phrase(iAdquiriente, FontFactory.GetFont("Helvetica", 8, Font.NORMAL)));
                cAdquiriente.BorderColor = BaseColor.GRAY;
                cAdquiriente.Border = Rectangle.BOX;
                cAdquiriente.VerticalAlignment = Element.ALIGN_MIDDLE;
                cAdquiriente.ExtraParagraphSpace = 3;
                cAdquiriente.Padding = 2;
                cAdquiriente.PaddingRight = 1;
                cAdquiriente.PaddingLeft = 3;
                cAdquiriente.Rowspan = 1;

                // Fecha de emisión (ficticia)
                DateTime fechaEmision = DateTime.Now;
                string horaEmision = DateTime.Now.ToString("HH:mm:ss");
                string iEmisionFactura = "FECHA DE EMISIÓN\r\n" + fechaEmision.ToString("yyyy-MM-dd") + "\r\n" + horaEmision;
                PdfPCell cEmisionFactura = new PdfPCell(new Phrase(iEmisionFactura, FontFactory.GetFont("Helvetica", 8, Font.NORMAL)));
                cEmisionFactura.BorderColor = BaseColor.GRAY;
                cEmisionFactura.Border = Rectangle.BOX;
                cEmisionFactura.HorizontalAlignment = Element.ALIGN_CENTER;
                cEmisionFactura.VerticalAlignment = Element.ALIGN_MIDDLE;
                cEmisionFactura.Padding = 1;

                // Fecha de vencimiento (ficticia)
                DateTime fechaVencimiento = DateTime.Now.AddDays(30); // Ejemplo: 30 días después de la fecha de emisión
                string iVenciFactura = "FECHA DE VENCIMIENTO\r\n" + fechaVencimiento.ToString("yyyy-MM-dd");
                PdfPCell cVenciFactura = new PdfPCell(new Phrase(iVenciFactura, FontFactory.GetFont("Helvetica", 8, Font.NORMAL)));
                cVenciFactura.BorderColor = BaseColor.GRAY;
                cVenciFactura.Border = Rectangle.BOX;
                cVenciFactura.HorizontalAlignment = Element.ALIGN_CENTER;
                cVenciFactura.VerticalAlignment = Element.ALIGN_MIDDLE;
                cVenciFactura.Padding = 1;

                encabezado2.AddCell(cAdquiriente);
                encabezado2.AddCell(cEmisionFactura);
                encabezado2.AddCell(cVenciFactura);
                documento.Add(encabezado2);


                // Crear un nuevo objeto PdfPTable para el nuevo renglón
                PdfPTable nuevoRenglon = new PdfPTable(1);
                nuevoRenglon.WidthPercentage = 100;

                // Agregar una celda con el dato deseado al nuevo objeto PdfPTable
                string nuevoDato = "CUFE: " + cufe; // Este sería tu nuevo dato
                PdfPCell celdaNuevoDato = new PdfPCell(new Phrase(nuevoDato, FontFactory.GetFont("Helvetica", 9, Font.NORMAL)));
                celdaNuevoDato.Border = Rectangle.NO_BORDER; // Eliminar bordes
                celdaNuevoDato.HorizontalAlignment = Element.ALIGN_LEFT; // Alinear a la izquierda
                celdaNuevoDato.Padding = 2;

                // Agregar la celda al nuevo objeto PdfPTable
                nuevoRenglon.AddCell(celdaNuevoDato);

                // Añadir el nuevo objeto PdfPTable al documento
                documento.Add(nuevoRenglon);

                if (!string.IsNullOrEmpty(factura.Recibo) && factura.Recibo != "0")
                {

                    // Crear un nuevo objeto PdfPTable para el renglón CUFE
                    PdfPTable nuevoRenglonCUDE = new PdfPTable(1);
                    nuevoRenglonCUDE.WidthPercentage = 100;

                    // Agregar una celda con el texto CUFE al nuevo objeto PdfPTable
                    string nuevoDatoCUDE = "CUDE: " + emisor.Codigo_municipio_emisor;
                    PdfPCell celdaNuevoDatoCUDE = new PdfPCell(new Phrase(nuevoDatoCUDE, FontFactory.GetFont("Helvetica", 9, Font.NORMAL)));
                    celdaNuevoDatoCUDE.Border = Rectangle.NO_BORDER; // Eliminar bordes
                    celdaNuevoDatoCUDE.HorizontalAlignment = Element.ALIGN_LEFT; // Alinear a la izquierda
                    celdaNuevoDatoCUDE.Padding = 2;

                    // Agregar la celda al nuevo objeto PdfPTable
                    nuevoRenglonCUDE.AddCell(celdaNuevoDatoCUDE);

                    // Añadir el nuevo objeto PdfPTable al documento
                    documento.Add(nuevoRenglonCUDE);
                }

                // Creación de la tabla con 9 columnas
                PdfPTable tabla = new PdfPTable(9);
                tabla.HorizontalAlignment = Element.ALIGN_CENTER;
                tabla.WidthPercentage = 100;
                float[] anchosColumnas = new float[] { 1, 1, 2, 3, 1, 2, 1, 2, 2 }; // Anchuras de las columnas en porcentaje
                tabla.SetWidths(anchosColumnas);
                tabla.SpacingBefore = 10;

                // Encabezados de la tabla
                string[] encabezados = { "Nro.", "Artículo", "Cantidad", "Descripción", "U/M", "Precio.Uni", "IVA %", "Vl.IVA", "Vr. Parcial" };
                foreach (string encabezadoTabla in encabezados) // Cambiar el nombre de la variable para evitar el conflicto de nombres
                {
                    PdfPCell celdaEncabezado = new PdfPCell(new Phrase(encabezadoTabla, FontFactory.GetFont("Helvetica", 8, Font.BOLD)));
                    celdaEncabezado.BackgroundColor = BaseColor.LIGHT_GRAY;
                    celdaEncabezado.HorizontalAlignment = Element.ALIGN_CENTER;
                    celdaEncabezado.VerticalAlignment = Element.ALIGN_MIDDLE;
                    tabla.AddCell(celdaEncabezado);
                }

                int contador = 1;

                // Llenar la tabla con los datos de la lista de productos
                foreach (var producto in listaProductos)
                {
                    // Agrega el número de producto
                    PdfPCell celdaNumero = new PdfPCell(new Phrase(contador.ToString(), FontFactory.GetFont("Helvetica", 8, Font.NORMAL)));
                    celdaNumero.HorizontalAlignment = Element.ALIGN_CENTER;
                    celdaNumero.VerticalAlignment = Element.ALIGN_MIDDLE;
                    tabla.AddCell(celdaNumero);

                    PdfPCell celdaCodigo = new PdfPCell(new Phrase(producto.Codigo, FontFactory.GetFont("Helvetica", 8, Font.NORMAL)));
                    celdaCodigo.HorizontalAlignment = Element.ALIGN_CENTER;
                    celdaCodigo.VerticalAlignment = Element.ALIGN_MIDDLE;
                    tabla.AddCell(celdaCodigo);

                    PdfPCell celdaCantidad = new PdfPCell(new Phrase(producto.Cantidad.ToString("0.00"), FontFactory.GetFont("Helvetica", 8, Font.NORMAL)));
                    celdaCantidad.HorizontalAlignment = Element.ALIGN_CENTER;
                    celdaCantidad.VerticalAlignment = Element.ALIGN_MIDDLE;
                    tabla.AddCell(celdaCantidad);

                    PdfPCell celdaDetalle = new PdfPCell(new Phrase(producto.Detalle, FontFactory.GetFont("Helvetica", 8, Font.NORMAL)));
                    celdaDetalle.HorizontalAlignment = Element.ALIGN_CENTER;
                    celdaDetalle.VerticalAlignment = Element.ALIGN_MIDDLE;
                    tabla.AddCell(celdaDetalle);

                    // Resto de las celdas con los datos correspondientes del producto
                    PdfPCell celdaUM = new PdfPCell(new Phrase("UND", FontFactory.GetFont("Helvetica", 8, Font.NORMAL)));
                    celdaUM.HorizontalAlignment = Element.ALIGN_CENTER;
                    celdaUM.VerticalAlignment = Element.ALIGN_MIDDLE;
                    tabla.AddCell(celdaUM);

                    // Añade las celdas restantes del producto (Neto, Iva, IvaTotal, Valor)
                    PdfPCell celdaNeto = new PdfPCell(new Phrase(producto.Neto.ToString(), FontFactory.GetFont("Helvetica", 8, Font.NORMAL)));
                    celdaNeto.HorizontalAlignment = Element.ALIGN_CENTER;
                    celdaNeto.VerticalAlignment = Element.ALIGN_MIDDLE;
                    tabla.AddCell(celdaNeto);

                    PdfPCell celdaIva = new PdfPCell(new Phrase(producto.Iva.ToString(), FontFactory.GetFont("Helvetica", 8, Font.NORMAL)));
                    celdaIva.HorizontalAlignment = Element.ALIGN_CENTER;
                    celdaIva.VerticalAlignment = Element.ALIGN_MIDDLE;
                    tabla.AddCell(celdaIva);

                    PdfPCell celdaIvaTotal = new PdfPCell(new Phrase(producto.IvaTotal.ToString(), FontFactory.GetFont("Helvetica", 8, Font.NORMAL)));
                    celdaIvaTotal.HorizontalAlignment = Element.ALIGN_CENTER;
                    celdaIvaTotal.VerticalAlignment = Element.ALIGN_MIDDLE;
                    tabla.AddCell(celdaIvaTotal);

                    PdfPCell celdaValor = new PdfPCell(new Phrase(producto.Valor.ToString("#,###,##0.00"), FontFactory.GetFont("Helvetica", 8, Font.NORMAL)));
                    celdaValor.HorizontalAlignment = Element.ALIGN_CENTER;
                    celdaValor.VerticalAlignment = Element.ALIGN_MIDDLE;
                    tabla.AddCell(celdaValor);

                    contador++;
                }


                // Agregar la tabla al documento
                documento.Add(tabla);

                // Calcula la cantidad de productos en la lista
                int cantidadProductos = listaProductos.Count;

                // Calcula la cantidad de saltos de línea que deseas dejar al final de la tabla
                int cantidadSaltosLinea = Math.Max(28 - cantidadProductos, 1); // Deja al menos un salto de línea

                // Agrega la cantidad de saltos de línea necesarios al final de la tabla
                for (int i = 0; i < cantidadSaltosLinea; i++)
                {
                    documento.Add(new Phrase("\n", FontFactory.GetFont("Helvetica", 8, Font.NORMAL)));
                }



                // Tabla de totales
                var tTotales = new PdfPTable(3);
                tTotales.WidthPercentage = 100;
                tTotales.SetWidths(new float[] { 4.5f, 1.5f, 1.5f });

                // Datos ficticios

                // Simulación de valor ficticio para el total neto
                decimal vTotalAP = movimiento.Valor; // Total neto ficticio
                string TextoAdicional = encabezado1.Notas;
                string moneda = "COP";
                string TextoConstancia = "Esta factura se asimila en sus efectos legales a la letra de cambio (segun el artículo 774 del Código del Comercio), con esta declara el comprador haber recibido real y materialmente la mercancia y/o servicio descrito en este titulo valor";
                string TextoResolucion = encabezado1.Resolucion;
                string Textonota = encabezado1.Nota_fin;
                string iMontoLetras = montoALetras(vTotalAP, moneda).ToUpper();

                string iDatos = $"{TextoAdicional}\r\n\r\n{Textonota}\r\n\r\n{TextoResolucion}\r\n\r\n{TextoConstancia}\n\n\n\n\n\n\nSon: {iMontoLetras}".Trim();

                var fnt7 = FontFactory.GetFont("Helvetica", 9, Font.NORMAL);
                var cDatos = new PdfPCell(new Phrase(iDatos, fnt7));
                cDatos.BorderColor = BaseColor.GRAY;
                cDatos.Border = Rectangle.BOX;
                cDatos.HorizontalAlignment = Element.ALIGN_LEFT;
                cDatos.VerticalAlignment = Element.ALIGN_TOP;
                cDatos.Rowspan = 11;
                cDatos.Padding = 3;
                cDatos.PaddingRight = 3;
                cDatos.PaddingLeft = 3;

                var fnt8 = FontFactory.GetFont("Helvetica", 7, Font.BOLD); //f10
                var fnt10 = FontFactory.GetFont("Helvetica", 9, Font.BOLD); //f10
                var ciSubtotalPU = new PdfPCell(new Phrase("Descuentos", fnt8));
                ciSubtotalPU.BorderColor = BaseColor.GRAY;
                ciSubtotalPU.Border = Rectangle.BOX;
                ciSubtotalPU.BorderWidthBottom = 0f;
                ciSubtotalPU.HorizontalAlignment = Element.ALIGN_CENTER;
                ciSubtotalPU.VerticalAlignment = Element.ALIGN_MIDDLE;
                ciSubtotalPU.Padding = 3;

                var fnt9 = FontFactory.GetFont("Helvetica", 8, Font.NORMAL);

                // Simulación de valores ficticios para los descuentos y el subtotal
                decimal descuentosDetalle = movimiento.Valor_dsto; // Valor ficticio de descuentos
                decimal subtotalPU = movimiento.Valor_neto; // Valor ficticio del subtotal

                string ivSubtotaoPU = subtotalPU.ToString("$#,###,##0.00");
                var cvSubtotalPU = new PdfPCell(new Phrase(descuentosDetalle.ToString("$#,###,##0.00"), fnt9));
                cvSubtotalPU.BorderColor = BaseColor.GRAY;
                cvSubtotalPU.Border = Rectangle.BOX;
                cvSubtotalPU.BorderWidthBottom = 0f;
                cvSubtotalPU.HorizontalAlignment = Element.ALIGN_CENTER;
                cvSubtotalPU.VerticalAlignment = Element.ALIGN_MIDDLE;
                cvSubtotalPU.Padding = 3;

                var ciDescuentosDetalle = new PdfPCell(new Phrase("Valor Exento", fnt8));
                ciDescuentosDetalle.BorderColor = BaseColor.GRAY;
                ciDescuentosDetalle.Border = Rectangle.BOX;
                ciDescuentosDetalle.BorderWidthTop = 0f;
                ciDescuentosDetalle.BorderWidthBottom = 0f;
                ciDescuentosDetalle.HorizontalAlignment = Element.ALIGN_CENTER;
                ciDescuentosDetalle.VerticalAlignment = Element.ALIGN_MIDDLE;
                ciDescuentosDetalle.Padding = 3;

                var ivDescuentosDetalle = descuentosDetalle.ToString("$#,###,##0.00");

                var cvDescuentosDetalle = new PdfPCell(new Phrase(ivDescuentosDetalle, fnt9));
                cvDescuentosDetalle.BorderColor = BaseColor.GRAY;
                cvDescuentosDetalle.Border = Rectangle.BOX;
                cvDescuentosDetalle.BorderWidthTop = 0f;
                cvDescuentosDetalle.BorderWidthBottom = 0f;
                cvDescuentosDetalle.HorizontalAlignment = Element.ALIGN_CENTER;
                cvDescuentosDetalle.VerticalAlignment = Element.ALIGN_MIDDLE;
                cvDescuentosDetalle.Padding = 3;

                var ciRecargosDetalle = new PdfPCell(new Phrase("Valor Gravado", fnt8));
                ciRecargosDetalle.BorderColor = BaseColor.GRAY;
                ciRecargosDetalle.Border = Rectangle.BOX;
                ciRecargosDetalle.BorderWidthTop = 0f;
                ciRecargosDetalle.BorderWidthBottom = 0f;
                ciRecargosDetalle.HorizontalAlignment = Element.ALIGN_CENTER;
                ciRecargosDetalle.VerticalAlignment = Element.ALIGN_MIDDLE;
                ciRecargosDetalle.Padding = 3;

                // Simulación de valores ficticios para los recargos
                decimal recargosDetalle = movimiento.Valor_neto; // Valor ficticio de recargos

                // Convertir el valor de recargos a cadena
                string iRecargosDetalle = recargosDetalle.ToString("$#,###,##0.00");

                // Crear la celda con el valor de recargos
                var cvRecargosDetalle = new PdfPCell(new Phrase(iRecargosDetalle, fnt9));
                cvRecargosDetalle.BorderColor = BaseColor.GRAY;
                cvRecargosDetalle.Border = Rectangle.BOX;
                cvRecargosDetalle.BorderWidthTop = 0f;
                cvRecargosDetalle.BorderWidthBottom = 0f;
                cvRecargosDetalle.HorizontalAlignment = Element.ALIGN_CENTER;
                cvRecargosDetalle.VerticalAlignment = Element.ALIGN_MIDDLE;
                cvRecargosDetalle.Padding = 3;

                var ciSubtotalNG = new PdfPCell(new Phrase("Valor IVA", fnt8));
                ciSubtotalNG.BorderColor = BaseColor.GRAY;
                ciSubtotalNG.Border = Rectangle.BOX;
                ciSubtotalNG.BorderWidthTop = 0f;
                ciSubtotalNG.BorderWidthBottom = 0f;
                ciSubtotalNG.HorizontalAlignment = Element.ALIGN_CENTER;
                ciSubtotalNG.VerticalAlignment = Element.ALIGN_MIDDLE;
                ciSubtotalNG.Padding = 3;

                decimal VlrIva = movimiento.Valor_iva; // Monto exclusivo de impuestos ficticio

                // Convertir el subtotal no gravado a cadena
                string iSubtotalNG = VlrIva.ToString("$#,###,##0.00");

                // Crear la celda con el valor del subtotal no gravado ficticio
                var cvSubtotalNG = new PdfPCell(new Phrase(iSubtotalNG, fnt9));
                cvSubtotalNG.BorderColor = BaseColor.GRAY;
                cvSubtotalNG.Border = Rectangle.BOX;
                cvSubtotalNG.BorderWidthBottom = 0f;
                cvSubtotalNG.BorderWidthTop = 0f;
                cvSubtotalNG.HorizontalAlignment = Element.ALIGN_CENTER;
                cvSubtotalNG.VerticalAlignment = Element.ALIGN_MIDDLE;
                cvSubtotalNG.Padding = 3;

                var ciSubtotalBG = new PdfPCell(new Phrase("Impoconsumo", fnt8));
                ciSubtotalBG.BorderColor = BaseColor.GRAY;
                ciSubtotalBG.Border = Rectangle.BOX;
                ciSubtotalBG.BorderWidthTop = 0f;
                ciSubtotalBG.BorderWidthBottom = 0f;
                ciSubtotalBG.HorizontalAlignment = Element.ALIGN_CENTER;
                ciSubtotalBG.VerticalAlignment = Element.ALIGN_MIDDLE;
                ciSubtotalBG.Padding = 3;

                // Convertir el monto exclusivo de impuestos a cadena
                string iSubtotalBG = movimiento.Ipoconsumo.ToString("$#,###,##0.00");

                // Crear la celda con el valor del monto exclusivo de impuestos ficticio
                var cvSubtotalBG = new PdfPCell(new Phrase(iSubtotalBG, fnt9));
                cvSubtotalBG.BorderColor = BaseColor.GRAY;
                cvSubtotalBG.Border = Rectangle.BOX;
                cvSubtotalBG.BorderWidthTop = 0f;
                cvSubtotalBG.BorderWidthBottom = 0f;
                cvSubtotalBG.HorizontalAlignment = Element.ALIGN_CENTER;
                cvSubtotalBG.VerticalAlignment = Element.ALIGN_MIDDLE;
                cvSubtotalBG.Padding = 3;

                var ciTotalImpuesto = new PdfPCell(new Phrase("Impto. Bolsas", fnt8));
                ciTotalImpuesto.BorderColor = BaseColor.GRAY;
                ciTotalImpuesto.Border = Rectangle.BOX;
                ciTotalImpuesto.BorderWidthTop = 0f;
                ciTotalImpuesto.BorderWidthBottom = 0f;
                ciTotalImpuesto.HorizontalAlignment = Element.ALIGN_CENTER;
                ciTotalImpuesto.VerticalAlignment = Element.ALIGN_MIDDLE;
                ciTotalImpuesto.Padding = 3;

                // Simulación de valores ficticios para el total de impuestos
                decimal totalImpuesto = movimiento.Valor_bolsa; // Total de impuestos ficticio

                // Convertir el total de impuestos a cadena
                string iTotalImpuesto = totalImpuesto.ToString("$#,###,##0.00");

                // Crear la celda con el valor del total de impuestos ficticio
                var cvTotalImpuesto = new PdfPCell(new Phrase(iTotalImpuesto, fnt9));
                cvTotalImpuesto.BorderColor = BaseColor.GRAY;
                cvTotalImpuesto.Border = Rectangle.BOX;
                cvTotalImpuesto.BorderWidthTop = 0f;
                cvTotalImpuesto.BorderWidthBottom = 0f;
                cvTotalImpuesto.HorizontalAlignment = Element.ALIGN_CENTER;
                cvTotalImpuesto.VerticalAlignment = Element.ALIGN_MIDDLE;
                cvTotalImpuesto.Padding = 3;

                var ciTotalMI = new PdfPCell(new Phrase("TOTAL (=)", fnt10));
                ciTotalMI.BorderColor = BaseColor.DARK_GRAY;
                ciTotalMI.Border = Rectangle.BOX;
        
                ciTotalMI.HorizontalAlignment = Element.ALIGN_CENTER;
                ciTotalMI.VerticalAlignment = Element.ALIGN_MIDDLE;
                ciTotalMI.Padding = 3;

                // Simulación de valor ficticio para el total más impuesto
                decimal totalMI = movimiento.Valor; // Total más impuesto ficticio

                // Convertir el total más impuesto a cadena
                string iTotalMI = totalMI.ToString("$#,###,##0.00");

                // Crear la celda con el valor del total más impuesto ficticio
                var cvTotalMI = new PdfPCell(new Phrase(iTotalMI, fnt10));
                cvTotalMI.BorderColor = BaseColor.DARK_GRAY;
                cvTotalMI.Border = Rectangle.BOX;
  
                cvTotalMI.HorizontalAlignment = Element.ALIGN_CENTER;
                cvTotalMI.VerticalAlignment = Element.ALIGN_MIDDLE;
                cvTotalMI.Padding = 3;

                var ciDescuentoGlobal = new PdfPCell(new Phrase("Rete. Fuente", fnt8));
                ciDescuentoGlobal.BorderColor = BaseColor.GRAY;
                ciDescuentoGlobal.Border = Rectangle.BOX;
                ciDescuentoGlobal.BorderWidthTop = 0f;
                ciDescuentoGlobal.BorderWidthBottom = 0f;
                ciDescuentoGlobal.HorizontalAlignment = Element.ALIGN_CENTER;
                ciDescuentoGlobal.VerticalAlignment = Element.ALIGN_MIDDLE;
                ciDescuentoGlobal.Padding = 3;

                // Simulación de valor ficticio para el descuento global
                decimal descuentoGlobal = movimiento.Retiene; // Descuento global ficticio

                // Convertir el descuento global a cadena
                string iDescuentoGlobal = descuentoGlobal.ToString("$#,###,##0.00");

                // Crear la celda con el valor del descuento global ficticio
                var cvDescuentoGlobal = new PdfPCell(new Phrase(iDescuentoGlobal, fnt9));
                cvDescuentoGlobal.BorderColor = BaseColor.GRAY;
                cvDescuentoGlobal.Border = Rectangle.BOX;
                cvDescuentoGlobal.BorderWidthBottom = 0f;
                cvDescuentoGlobal.BorderWidthTop = 0f;
                cvDescuentoGlobal.HorizontalAlignment = Element.ALIGN_CENTER;
                cvDescuentoGlobal.VerticalAlignment = Element.ALIGN_MIDDLE;
                cvDescuentoGlobal.Padding = 2;

                var ciRecargoGlobal = new PdfPCell(new Phrase("Rete. ICA", fnt8));
                ciRecargoGlobal.BorderColor = BaseColor.GRAY;
                ciRecargoGlobal.Border = Rectangle.BOX;
                ciRecargoGlobal.BorderWidthTop = 0f;
                ciRecargoGlobal.BorderWidthBottom = 0f;
                ciRecargoGlobal.HorizontalAlignment = Element.ALIGN_CENTER;
                ciRecargoGlobal.VerticalAlignment = Element.ALIGN_MIDDLE;
                ciRecargoGlobal.Padding = 3;

                // Simulación de valor ficticio para el recargo global
                decimal recargoGlobal = 00.0m; // Recargo global ficticio

                // Convertir el recargo global a cadena
                string iRecargoGlobal = recargoGlobal.ToString("$#,###,##0.00");

                // Crear la celda con el valor del recargo global ficticio
                var cvRecargoGlobal = new PdfPCell(new Phrase(iRecargoGlobal, fnt9));
                cvRecargoGlobal.BorderColor = BaseColor.GRAY;
                cvRecargoGlobal.Border = Rectangle.BOX;
                cvRecargoGlobal.BorderWidthTop = 0f;
                cvRecargoGlobal.BorderWidthBottom = 0f;
                cvRecargoGlobal.HorizontalAlignment = Element.ALIGN_CENTER;
                cvRecargoGlobal.VerticalAlignment = Element.ALIGN_MIDDLE;
                cvRecargoGlobal.Padding = 3;

                var ciAnticipo = new PdfPCell(new Phrase("Valor Excluido", fnt8));
                ciAnticipo.BorderColor = BaseColor.GRAY;
                ciAnticipo.Border = Rectangle.BOX;
                ciAnticipo.BorderWidthTop = 0f;
                ciAnticipo.BorderWidthBottom = 0f;
                ciAnticipo.HorizontalAlignment = Element.ALIGN_CENTER;
                ciAnticipo.VerticalAlignment = Element.ALIGN_MIDDLE;
                ciAnticipo.Padding = 3;

                // Simulación de valor ficticio para el anticipo
                decimal anticipo = movimiento.Exentas; // Anticipo ficticio

                // Convertir el anticipo a cadena
                string iAnticipo = anticipo.ToString("$#,###,##0.00");

                // Crear la celda con el valor del anticipo ficticio
                var cvAnticipo = new PdfPCell(new Phrase(iAnticipo, fnt9));
                cvAnticipo.BorderColor = BaseColor.GRAY;
                cvAnticipo.Border = Rectangle.BOX;
                cvAnticipo.BorderWidthBottom = 0f;
                cvAnticipo.BorderWidthTop = 0f;
                cvAnticipo.HorizontalAlignment = Element.ALIGN_CENTER;
                cvAnticipo.VerticalAlignment = Element.ALIGN_MIDDLE;
                cvAnticipo.Padding = 3;

                var ciTotalNeto = new PdfPCell(new Phrase("Total menos rete.", fnt10));
                ciTotalNeto.BorderColor = BaseColor.GRAY;
                ciTotalNeto.Border = Rectangle.BOX;
                ciTotalNeto.HorizontalAlignment = Element.ALIGN_CENTER;
                ciTotalNeto.VerticalAlignment = Element.ALIGN_MIDDLE;
                ciTotalNeto.Padding = 3;

               

                // Convertir el total neto a cadena
                string iTotalNeto = vTotalAP.ToString("$#,###,##0.00");

                // Crear la celda con el valor del total neto ficticio
                var cvTotalNeto = new PdfPCell(new Phrase(iTotalNeto, fnt10));
                cvTotalNeto.BorderColor = BaseColor.GRAY;
                cvTotalNeto.Border = Rectangle.BOX;
                cvTotalNeto.HorizontalAlignment = Element.ALIGN_CENTER;
                cvTotalNeto.VerticalAlignment = Element.ALIGN_MIDDLE;
                cvTotalNeto.Padding = 3;
               

                tTotales.AddCell(cDatos);
                tTotales.AddCell(ciSubtotalPU);
                tTotales.AddCell(cvSubtotalPU);
                tTotales.AddCell(ciDescuentosDetalle);
                tTotales.AddCell(cvDescuentosDetalle);
                tTotales.AddCell(ciAnticipo);
                tTotales.AddCell(cvAnticipo);
                tTotales.AddCell(ciRecargosDetalle);
                tTotales.AddCell(cvRecargosDetalle);
                tTotales.AddCell(ciSubtotalNG);
                tTotales.AddCell(cvSubtotalNG);
                tTotales.AddCell(ciSubtotalBG);
                tTotales.AddCell(cvSubtotalBG);
                tTotales.AddCell(ciTotalImpuesto);
                tTotales.AddCell(cvTotalImpuesto);
                tTotales.AddCell(ciTotalMI);
                tTotales.AddCell(cvTotalMI);
                tTotales.AddCell(ciDescuentoGlobal);
                tTotales.AddCell(cvDescuentoGlobal);
                tTotales.AddCell(ciRecargoGlobal);
                tTotales.AddCell(cvRecargoGlobal);
               
                tTotales.AddCell(ciTotalNeto);
                tTotales.AddCell(cvTotalNeto);

                tTotales.AddCell(cDatos);

                documento.Add(tTotales);


                // Sección de firmas
                PdfPTable seccionFirmas = new PdfPTable(3); // Cambiamos a 3 columnas
                seccionFirmas.WidthPercentage = 100;
                seccionFirmas.SpacingBefore = 10f; // Espacio antes de la tabla
                seccionFirmas.SetWidths(new float[] { 1, 0.5f, 1 }); // Ancho de las columnas

                // Firma y sello del cliente
                PdfPCell celdaFirmaCliente = new PdfPCell(new Phrase("Proveedor Tecnológico: Cadena S.A. Nit 890930534", FontFactory.GetFont("Helvetica", 9, Font.NORMAL)));
                celdaFirmaCliente.HorizontalAlignment = Element.ALIGN_CENTER;
                celdaFirmaCliente.Border = Rectangle.NO_BORDER;
                celdaFirmaCliente.PaddingTop = 6;
                seccionFirmas.AddCell(celdaFirmaCliente);

                // Celda vacía sin bordes
                PdfPCell celdaVacia = new PdfPCell();
                celdaVacia.Border = PdfPCell.NO_BORDER;
                seccionFirmas.AddCell(celdaVacia);

                // Revisado y entregado
                PdfPCell celdaRevisadoEntregado = new PdfPCell(new Phrase("Software: RMSOFT CASA DE SOFTWARE  S.A.S. Nit 90074400-7", FontFactory.GetFont("Helvetica", 9, Font.NORMAL)));
                celdaRevisadoEntregado.HorizontalAlignment = Element.ALIGN_CENTER;
                celdaRevisadoEntregado.Border = Rectangle.NO_BORDER;
                celdaRevisadoEntregado.PaddingTop = 6;
                seccionFirmas.AddCell(celdaRevisadoEntregado);

                // Añadir la tabla de firmas al documento
                documento.Add(seccionFirmas);


                documento.Close();
            }
            catch (Exception ex)
            {
                // Manejar cualquier excepción que pueda ocurrir
                Console.WriteLine("Error al crear el PDF: " + ex.ToString());
            }
        }


        public static Image CrearQR(string texto, int size = 3)
        {
            var qrGenerator = new QRCoder.QRCodeGenerator();
            var qrData = qrGenerator.CreateQrCode(texto, QRCoder.QRCodeGenerator.ECCLevel.Q);
            var qrCode = new QRCoder.QRCode(qrData);
            var qrBitmap = qrCode.GetGraphic(size); // Ajusta el tamaño aquí

            // Convertimos el objeto Bitmap a un objeto Image
            Image imagenQr;
            using (var memoryStream = new MemoryStream())
            {
                qrBitmap.Save(memoryStream, ImageFormat.Png);
                imagenQr = Image.GetInstance(memoryStream.ToArray());
            }

            return imagenQr;
        }



        public static string montoALetras(decimal valor, string moneda)
        {
            var entero = Convert.ToInt64(Math.Truncate(valor));
            var decimales = Convert.ToInt32(Math.Round((valor - entero) * 100, 2));

            var ret = numeroALetras(entero);

            switch (moneda)
            {
                case "COP":
                    moneda = "peso(s) colombiano(s)";
                    break;

                case "USD":
                    moneda = "dólar(es) estadounidense(s)";
                    break;

                case "EUR":
                    moneda = "euro(s)";
                    break;

                case "GBP":
                    moneda = "libra(s) esterlina(s)";
                    break;
            }

            if (decimales == 0)
                ret += " " + moneda + " exactos";
            else if (decimales == 1)
                ret += " " + moneda + " con un centavo";
            else ret += " " + moneda + " con " + numeroALetras(decimales) + " centavos";

            return ret;
        }

        public static string numeroALetras(double valor)
        {
            string ret = null;
            valor = Math.Truncate(valor);

            if (valor == 0) ret = "cero";
            else if (valor == 1) ret = "uno";
            else if (valor == 2) ret = "dos";
            else if (valor == 3) ret = "tres";
            else if (valor == 4) ret = "cuatro";
            else if (valor == 5) ret = "cinco";
            else if (valor == 6) ret = "seis";
            else if (valor == 7) ret = "siete";
            else if (valor == 8) ret = "ocho";
            else if (valor == 9) ret = "nueve";
            else if (valor == 10) ret = "diez";
            else if (valor == 11) ret = "once";
            else if (valor == 12) ret = "doce";
            else if (valor == 13) ret = "trece";
            else if (valor == 14) ret = "catorce";
            else if (valor == 15) ret = "quince";
            else if (valor < 20) ret = "dieci" + numeroALetras(valor - 10);
            else if (valor == 20) ret = "veinte";
            else if (valor < 30) ret = "veinti" + numeroALetras(valor - 20);
            else if (valor == 30) ret = "treinta";
            else if (valor == 40) ret = "cuarenta";
            else if (valor == 50) ret = "cincuenta";
            else if (valor == 60) ret = "sesenta";
            else if (valor == 70) ret = "setenta";
            else if (valor == 80) ret = "ochenta";
            else if (valor == 90) ret = "noventa";
            else if (valor < 100) ret = numeroALetras(Math.Truncate(valor / 10) * 10) + " Y " + numeroALetras(valor % 10);
            else if (valor == 100) ret = "cien";
            else if (valor < 200) ret = "ciento " + numeroALetras(valor - 100);
            else if ((valor == 200) || (valor == 300) || (valor == 400) || (valor == 600) || (valor == 800)) ret = numeroALetras(Math.Truncate(valor / 100)) + "cientos";
            else if (valor == 500) ret = "quinientos";
            else if (valor == 700) ret = "setecientos";
            else if (valor == 900) ret = "novecientos";
            else if (valor < 1000) ret = numeroALetras(Math.Truncate(valor / 100) * 100) + " " + numeroALetras(valor % 100);
            else if (valor == 1000) ret = "mil";
            else if (valor < 2000) ret = "mil " + numeroALetras(valor % 1000);
            else if (valor < 1000000)
            {
                ret = numeroALetras(Math.Truncate(valor / 1000)) + " mil";
                if ((valor % 1000) > 0)
                {
                    ret += " " + numeroALetras(valor % 1000);
                }
            }
            else if (valor == 1000000)
            {
                ret = "un millón";
            }
            else if (valor < 2000000)
            {
                ret = "un millón " + numeroALetras(valor % 1000000);
            }
            else if (valor < 1000000000000)
            {
                ret = numeroALetras(Math.Truncate(valor / 1000000)) + " millones ";
                if ((valor - Math.Truncate(valor / 1000000) * 1000000) > 0)
                {
                    ret += " " + numeroALetras(valor - Math.Truncate(valor / 1000000) * 1000000);
                }
            }
            else if (valor == 1000000000000) ret = "un billón";
            else if (valor < 2000000000000) ret = "un billón " + numeroALetras(valor - Math.Truncate(valor / 1000000000000) * 1000000000000);
            else
            {
                ret = numeroALetras(Math.Truncate(valor / 1000000000000)) + " billones";
                if ((valor - Math.Truncate(valor / 1000000000000) * 1000000000000) > 0)
                {
                    ret += " " + numeroALetras(valor - Math.Truncate(valor / 1000000000000) * 1000000000000);
                }
            }

            return ret;
        }

        public static string CrearTextoResolucion(long numero, DateTime fecha, string prefijo, long desde, long hasta, DateTime? vigencia = null)
        {
            var ret = "Resolución número " + numero + " autorizada el " + fecha.ToString("yyyy-MM-dd") + " desde " + prefijo + desde + " hasta " + prefijo + hasta;
            if (vigencia.HasValue)
                ret += ", vigente hasta el " + vigencia.Value.ToString("yyyy-MM-dd") + ".";
            else ret += ".";

            return ret;
        }
    }
}
