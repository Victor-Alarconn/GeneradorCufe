
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


namespace GeneradorCufe.ViewModel
{
    public class GeneradorPDF
    {
      
        public  GeneradorPDF()
        {
            
        }

        public static void CrearPDF(MemoryStream pdfStream, Emisor emisor, Factura factura)
        {
            try
            {
                Document documento = new Document(PageSize.A4, 15, 18, 15, 18);
                PdfWriter.GetInstance(documento, pdfStream);
                documento.Open();

                // Encabezado
                PdfPTable encabezado = new PdfPTable(3);
                encabezado.HorizontalAlignment = Element.ALIGN_LEFT;
                encabezado.WidthPercentage = 100;
                encabezado.SetWidths(new float[] { 2, 4, 2 });
                encabezado.DefaultCell.Border = iTextSharp.text.Rectangle.NO_BORDER;

                PdfPCell cEmpty5 = new PdfPCell(new Phrase(" "))
                {
                    Border = Rectangle.NO_BORDER,
                    FixedHeight = 5
                };
                PdfPCell cEmpty15 = new PdfPCell(new Phrase(" "))
                {
                    Border = Rectangle.NO_BORDER,
                    FixedHeight = 15
                };

                string RutaLogo = "C:\\Users\\Programacion01\\source\\repos\\RepoVictor\\GeneradorCufe\\xml\\Logo_ADEZ.png";
                if (File.Exists(RutaLogo))
                {
                    Image logo = Image.GetInstance(RutaLogo);
                    logo.ScaleAbsoluteWidth(120); // Ajustar el ancho de la imagen
                    logo.ScaleAbsoluteHeight(80); // Opcional: ajustar también la altura de la imagen si es necesario
                    logo.BackgroundColor = BaseColor.WHITE;
                    logo.Alignment = Element.ALIGN_MIDDLE;

                    // Agregar el logo en la primera columna (izquierda)
                    PdfPCell cLogo = new PdfPCell(logo);
                    cLogo.Border = Rectangle.NO_BORDER;
                    encabezado.AddCell(cLogo);
                }
                else
                {
                    encabezado.AddCell(cEmpty15);
                }


                // Agregar celda vacía en la tercera columna (derecha)
                encabezado.AddCell(cEmpty15);

                // Datos del emisor (ficticios)
                string nombreEmisor = "PINTURAS PANELTON Y DISTRIBUCIONES SAS";
                string nitEmisor = "123456789-0";

                var tEmisor = new PdfPTable(1);
                tEmisor.WidthPercentage = 100;
                tEmisor.DefaultCell.Border = Rectangle.NO_BORDER;

                var fnt1 = FontFactory.GetFont("Helvetica", 10, Font.BOLD); //f12
                var cNombreEmisor = new PdfPCell(new Phrase(nombreEmisor, fnt1));
                cNombreEmisor.Border = Rectangle.NO_BORDER;
                cNombreEmisor.HorizontalAlignment = Element.ALIGN_CENTER;

                var fnt2 = FontFactory.GetFont("Helvetica", 10, Font.BOLD); //f11
                var cNitEmisor = new PdfPCell(new Phrase("NIT: " + nitEmisor, fnt2));
                cNitEmisor.Border = Rectangle.NO_BORDER;
                cNitEmisor.HorizontalAlignment = Element.ALIGN_CENTER;

                var fnt3 = FontFactory.GetFont("Helvetica", 9, Font.NORMAL); //f9

                // Crear tabla interna para los datos del emisor
                var tDatosEmisor = new PdfPTable(1);
                tDatosEmisor.WidthPercentage = 100;
                tDatosEmisor.DefaultCell.Border = Rectangle.NO_BORDER;

                // Aquí eliminamos la redefinición de las variables

                var cRepresentante = new PdfPCell(new Phrase("Representante : JURIDICA - Responsable IVA", fnt3));
                cRepresentante.Border = Rectangle.NO_BORDER;
                cRepresentante.HorizontalAlignment = Element.ALIGN_CENTER;
                cRepresentante.VerticalAlignment = Element.ALIGN_TOP;

                var cDireccionEmisor = new PdfPCell(new Phrase("Dirección : Panelton Dosquebradas Cr16#70-21 Bd1 Dosq,Rda", fnt3));
                cDireccionEmisor.Border = Rectangle.NO_BORDER;
                cDireccionEmisor.HorizontalAlignment = Element.ALIGN_CENTER;

                var cCorreoEmisor = new PdfPCell(new Phrase("facturacionpinturaspaneltonsas@gmail.com\r\n", fnt3));
                cCorreoEmisor.Border = Rectangle.NO_BORDER;
                cCorreoEmisor.HorizontalAlignment = Element.ALIGN_CENTER;

                var cTelefonoEmisor = new PdfPCell(new Phrase("Telefono: 3131732", fnt3));
                cTelefonoEmisor.Border = Rectangle.NO_BORDER;
                cTelefonoEmisor.HorizontalAlignment = Element.ALIGN_CENTER;

                // Agregar datos del emisor en la segunda columna (central)
                encabezado.AddCell(cEmpty5);
                encabezado.AddCell(cNombreEmisor);
                encabezado.AddCell(cNitEmisor);
                encabezado.AddCell(cRepresentante);
                encabezado.AddCell(cDireccionEmisor);
                encabezado.AddCell(cCorreoEmisor);
                encabezado.AddCell(cTelefonoEmisor);
                encabezado.AddCell(cEmpty5);
                encabezado.AddCell(cEmpty15);
                documento.Add(encabezado);

                // Espacio adicional
                documento.Add(new Phrase("   ", FontFactory.GetFont("Helvetica", 8, Font.NORMAL)));

                // Create a table for qr
                PdfPTable table = new PdfPTable(2);
                table.WidthPercentage = 100;
                table.DefaultCell.Border = Rectangle.NO_BORDER;

                // Declarar qrCell antes de establecer su altura fija
                PdfPCell qrCell = new PdfPCell();

                // Definir la altura deseada para las celdas
                float alturaCelda = 50f;

                // Añadir elementos a qrCell
                string TextoQR = "NumFac:323200000129FecFac:2019-16-01HorFac:10:53:10-05:00NitFac:700085371DocAdq:800199436ValFac:1500000.00ValIva:285000.00ValOtroIm:0.00ValTolFac:1785000.00CUFE:e5bac48e354bc907bccff0ea7d45fbf784f0a8e7243b58337361e1fbd430489d";
                if (string.IsNullOrEmpty(TextoQR)) TextoQR = "TextoQR"; // Use a default value if the text is null or empty
                Image imageQr = CrearQR(TextoQR);
                imageQr.Border = Rectangle.NO_BORDER;
                imageQr.Alignment = Element.ALIGN_CENTER;
                qrCell.AddElement(new Phrase("Representación Gráfica De Factura Electrónica De Venta", FontFactory.GetFont("Helvetica", 12, Font.BOLD)));
                qrCell.AddElement(imageQr);
                qrCell.Border = Rectangle.NO_BORDER;
                qrCell.PaddingBottom = 10;
                qrCell.HorizontalAlignment = Element.ALIGN_CENTER;
                // Establecer la altura fija de qrCell
                qrCell.FixedHeight = alturaCelda;
                table.AddCell(qrCell);

                // Add the general information to the second column
                PdfPTable tInfoGeneral = new PdfPTable(1);
                tInfoGeneral.WidthPercentage = 100;
                tInfoGeneral.DefaultCell.Border = Rectangle.NO_BORDER;

                // Tipo de factura (ficticio)
                string Tipo = "Factura Victor";
                PdfPCell ciTipoFactura = new PdfPCell(new Phrase(Tipo, FontFactory.GetFont("Helvetica", 7, Font.NORMAL)));
                ciTipoFactura.Border = Rectangle.NO_BORDER;
                ciTipoFactura.HorizontalAlignment = Element.ALIGN_CENTER;
                ciTipoFactura.PaddingBottom = 5;

                // Número de factura (ficticio)
                string vNumeroFactura = "77777777";
                PdfPCell cNumeroFactura = new PdfPCell(new Phrase(vNumeroFactura, FontFactory.GetFont("Helvetica", 8, Font.NORMAL)));
                cNumeroFactura.BackgroundColor = BaseColor.WHITE;
                cNumeroFactura.BorderColor = BaseColor.GRAY;
                cNumeroFactura.Border = Rectangle.BOX;
                cNumeroFactura.HorizontalAlignment = Element.ALIGN_CENTER;
                cNumeroFactura.PaddingTop = 5;
                cNumeroFactura.PaddingBottom = 7;

                tInfoGeneral.AddCell(ciTipoFactura);
                tInfoGeneral.AddCell(cNumeroFactura);
                table.AddCell(tInfoGeneral);

                documento.Add(table);


                // Espacio adicional
                documento.Add(new Phrase("   ", FontFactory.GetFont("Helvetica", 8, Font.NORMAL)));

                // Segundo encabezado (Información del adquiriente)
                PdfPTable encabezado2 = new PdfPTable(3);
                encabezado2.HorizontalAlignment = Element.ALIGN_LEFT;
                encabezado2.WidthPercentage = 100;
                encabezado2.SetWidths(new float[] { 4, 1.25f, 1.25f });
                encabezado2.SpacingAfter = 10;

                // Datos del adquiriente (ficticios)
                string nombreAdquiriente = "Nombre del Adquiriente";
                string identificacionAdquiriente = "123456789";
                string direccionAdquiriente = "Dirección del Adquiriente";
                string correoAdquiriente = "correo@adquiriente.com";
                string telefonoAdquiriente = "987654321";

                string iAdquiriente = "ADQUIRIENTE: " + nombreAdquiriente + "\r\n" +
                                      "DOCUMENTO DE IDENTIFICACIÓN: " + identificacionAdquiriente + "\r\n" +
                                      "DIRECCIÓN: " + direccionAdquiriente.ToUpper().Replace("\r\n", " ");

                if (!string.IsNullOrEmpty(correoAdquiriente))
                    iAdquiriente += "\r\nCORREO ELECTRÓNICO: " + correoAdquiriente;
                if (!string.IsNullOrEmpty(telefonoAdquiriente))
                    iAdquiriente += "\r\nNÚMERO TELEFÓNICO: " + telefonoAdquiriente;

                PdfPCell cAdquiriente = new PdfPCell(new Phrase(iAdquiriente, FontFactory.GetFont("Helvetica", 8, Font.NORMAL)));
                cAdquiriente.BorderColor = BaseColor.GRAY;
                cAdquiriente.Border = Rectangle.BOX;
                cAdquiriente.VerticalAlignment = Element.ALIGN_MIDDLE;
                cAdquiriente.ExtraParagraphSpace = 3;
                cAdquiriente.Padding = 4;
                cAdquiriente.PaddingRight = 7;
                cAdquiriente.PaddingLeft = 7;
                cAdquiriente.Rowspan = 2;

                // Fecha de emisión (ficticia)
                DateTime fechaEmision = DateTime.Now;
                string horaEmision = DateTime.Now.ToString("HH:mm:ss");
                string iEmisionFactura = "FECHA DE EMISIÓN\r\n" + fechaEmision.ToString("yyyy-MM-dd") + "\r\n" + horaEmision;
                PdfPCell cEmisionFactura = new PdfPCell(new Phrase(iEmisionFactura, FontFactory.GetFont("Helvetica", 8, Font.NORMAL)));
                cEmisionFactura.BorderColor = BaseColor.GRAY;
                cEmisionFactura.Border = Rectangle.BOX;
                cEmisionFactura.HorizontalAlignment = Element.ALIGN_CENTER;
                cEmisionFactura.VerticalAlignment = Element.ALIGN_MIDDLE;
                cEmisionFactura.Padding = 4;

                // Fecha de vencimiento (ficticia)
                DateTime fechaVencimiento = DateTime.Now.AddDays(30); // Ejemplo: 30 días después de la fecha de emisión
                string iVenciFactura = "FECHA DE VENCIMIENTO\r\n" + fechaVencimiento.ToString("yyyy-MM-dd");
                PdfPCell cVenciFactura = new PdfPCell(new Phrase(iVenciFactura, FontFactory.GetFont("Helvetica", 8, Font.NORMAL)));
                cVenciFactura.BorderColor = BaseColor.GRAY;
                cVenciFactura.Border = Rectangle.BOX;
                cVenciFactura.HorizontalAlignment = Element.ALIGN_CENTER;
                cVenciFactura.VerticalAlignment = Element.ALIGN_MIDDLE;
                cVenciFactura.Padding = 4;

                // CUFE (ficticio)
                string cufe = "C123456789";
                string iCufe = "CUFE\r\n" + cufe;
                PdfPCell cCufe = new PdfPCell(new Phrase(iCufe, FontFactory.GetFont("Helvetica", 8, Font.NORMAL)));
                cCufe.BorderColor = BaseColor.GRAY;
                cCufe.Border = Rectangle.BOX;
                cCufe.HorizontalAlignment = Element.ALIGN_CENTER;
                cCufe.VerticalAlignment = Element.ALIGN_MIDDLE;
                cCufe.Padding = 4;
                cCufe.Colspan = 2;

                encabezado2.AddCell(cAdquiriente);
                encabezado2.AddCell(cEmisionFactura);
                encabezado2.AddCell(cVenciFactura);
                encabezado2.AddCell(cCufe);

                documento.Add(encabezado2);

                // Creación de la tabla con 9 columnas
                PdfPTable tabla = new PdfPTable(9);
                tabla.HorizontalAlignment = Element.ALIGN_CENTER;
                tabla.WidthPercentage = 100;
                float[] anchosColumnas = new float[] { 1, 3, 1, 3, 1, 2, 2, 1, 2 }; // Anchuras de las columnas en porcentaje
                tabla.SetWidths(anchosColumnas);
                tabla.SpacingBefore = 10;

                // Encabezados de la tabla
                string[] encabezados = { "Nro.", "Artículo", "Cantidad", "Descripción", "U/M", "Imp. Consumo", "Vr. Unidad", "IVA", "Vr. Parcial" };
                foreach (string encabezadoTabla in encabezados) // Cambiar el nombre de la variable para evitar el conflicto de nombres
                {
                    PdfPCell celdaEncabezado = new PdfPCell(new Phrase(encabezadoTabla, FontFactory.GetFont("Helvetica", 8, Font.BOLD)));
                    celdaEncabezado.BackgroundColor = BaseColor.LIGHT_GRAY;
                    celdaEncabezado.HorizontalAlignment = Element.ALIGN_CENTER;
                    celdaEncabezado.VerticalAlignment = Element.ALIGN_MIDDLE;
                    tabla.AddCell(celdaEncabezado);
                }

                // Datos de ejemplo para la tabla (puedes reemplazarlos con tus propios datos)
                List<string[]> datos = new List<string[]>
{
                    new string[] { "1", "Artículo 1", "2", "Descripción 1", "UND", "5.00", "10.00", "1.00", "20.00" },
                    new string[] { "2", "Artículo 2", "3", "Descripción 2", "UND", "6.00", "15.00", "1.50", "45.00" },
                    // Puedes agregar más filas según sea necesario
                };

                // Llenar la tabla con los datos
                foreach (string[] filaDatos in datos)
                {
                    foreach (string dato in filaDatos)
                    {
                        PdfPCell celdaDato = new PdfPCell(new Phrase(dato, FontFactory.GetFont("Helvetica", 8, Font.NORMAL)));
                        celdaDato.HorizontalAlignment = Element.ALIGN_CENTER;
                        celdaDato.VerticalAlignment = Element.ALIGN_MIDDLE;
                        tabla.AddCell(celdaDato);
                    }
                }

                // Agregar la tabla al documento
                documento.Add(tabla);

                // Espacio adicional
                documento.Add(new Phrase("   ", FontFactory.GetFont("Helvetica", 8, Font.NORMAL)));

                // Tabla de totales
                var tTotales = new PdfPTable(3);
                tTotales.WidthPercentage = 100;
                tTotales.SetWidths(new float[] { 4.5f, 3f, 1.5f });

                // Datos ficticios
                string TextoAdicional = "Información adicional: Lorem ipsum dolor sit amet.";
                string TextoConstancia = "Se hace constar que las mercancías o servicios fueron entregados real y materialmente y en todo caso, la factura será considerada irrevocablemente aceptada por el comprador si no reclamare en los tres (3) días hábiles siguientes a su recepción.";
                string TextoResolucion = "Resolución: Lorem ipsum dolor sit amet.";

                string iDatos = (TextoAdicional + "\r\n\r\n" + TextoConstancia + "\r\n\r\n" + TextoResolucion).Trim();
                var fnt7 = FontFactory.GetFont("Helvetica", 12, Font.NORMAL);
                var cDatos = new PdfPCell(new Phrase(iDatos, fnt7));
                cDatos.BorderColor = BaseColor.GRAY;
                cDatos.Border = Rectangle.BOX;
                cDatos.HorizontalAlignment = Element.ALIGN_LEFT;
                cDatos.VerticalAlignment = Element.ALIGN_MIDDLE;
                cDatos.Rowspan = 11;
                cDatos.Padding = 4;
                cDatos.PaddingRight = 7;
                cDatos.PaddingLeft = 7;

                var fnt8 = FontFactory.GetFont("Helvetica", 10, Font.BOLD); //f10
                var ciSubtotalPU = new PdfPCell(new Phrase("Descuentos", fnt8));
                ciSubtotalPU.BorderColor = BaseColor.GRAY;
                ciSubtotalPU.Border = Rectangle.BOX;
                ciSubtotalPU.BorderWidthBottom = 0f;
                ciSubtotalPU.HorizontalAlignment = Element.ALIGN_CENTER;
                ciSubtotalPU.VerticalAlignment = Element.ALIGN_MIDDLE;
                ciSubtotalPU.Padding = 4;

                var fnt9 = FontFactory.GetFont("Helvetica", 11, Font.NORMAL);

                // Simulación de valores ficticios para los descuentos y el subtotal
                decimal descuentosDetalle = 50.0m; // Valor ficticio de descuentos
                decimal subtotalPU = 1000.0m; // Valor ficticio del subtotal

                string ivSubtotaoPU = subtotalPU.ToString("$#,###,##0.##");
                var cvSubtotalPU = new PdfPCell(new Phrase(ivSubtotaoPU, fnt9));
                cvSubtotalPU.BorderColor = BaseColor.GRAY;
                cvSubtotalPU.Border = Rectangle.BOX;
                cvSubtotalPU.BorderWidthBottom = 0f;
                cvSubtotalPU.HorizontalAlignment = Element.ALIGN_CENTER;
                cvSubtotalPU.VerticalAlignment = Element.ALIGN_MIDDLE;
                cvSubtotalPU.Padding = 4;

                var ciDescuentosDetalle = new PdfPCell(new Phrase("Valor Exento", fnt8));
                ciDescuentosDetalle.BorderColor = BaseColor.GRAY;
                ciDescuentosDetalle.Border = Rectangle.BOX;
                ciDescuentosDetalle.BorderWidthTop = 0f;
                ciDescuentosDetalle.BorderWidthBottom = 0f;
                ciDescuentosDetalle.HorizontalAlignment = Element.ALIGN_CENTER;
                ciDescuentosDetalle.VerticalAlignment = Element.ALIGN_MIDDLE;
                ciDescuentosDetalle.Padding = 4;

                var ivDescuentosDetalle = descuentosDetalle != 0 ? descuentosDetalle.ToString("$#,###,##0.##") : "0";
                var cvDescuentosDetalle = new PdfPCell(new Phrase(ivDescuentosDetalle, fnt9));
                cvDescuentosDetalle.BorderColor = BaseColor.GRAY;
                cvDescuentosDetalle.Border = Rectangle.BOX;
                cvDescuentosDetalle.BorderWidthTop = 0f;
                cvDescuentosDetalle.BorderWidthBottom = 0f;
                cvDescuentosDetalle.HorizontalAlignment = Element.ALIGN_CENTER;
                cvDescuentosDetalle.VerticalAlignment = Element.ALIGN_MIDDLE;
                cvDescuentosDetalle.Padding = 4;

                var ciRecargosDetalle = new PdfPCell(new Phrase("Valor Gravado", fnt8));
                ciRecargosDetalle.BorderColor = BaseColor.GRAY;
                ciRecargosDetalle.Border = Rectangle.BOX;
                ciRecargosDetalle.BorderWidthTop = 0f;
                ciRecargosDetalle.HorizontalAlignment = Element.ALIGN_CENTER;
                ciRecargosDetalle.VerticalAlignment = Element.ALIGN_MIDDLE;
                ciRecargosDetalle.Padding = 4;

                // Simulación de valores ficticios para los recargos
                decimal recargosDetalle = 75.0m; // Valor ficticio de recargos

                // Convertir el valor de recargos a cadena
                string iRecargosDetalle = recargosDetalle.ToString("$#,###,##0.##");

                // Crear la celda con el valor de recargos
                var cvRecargosDetalle = new PdfPCell(new Phrase(iRecargosDetalle, fnt9));
                cvRecargosDetalle.BorderColor = BaseColor.GRAY;
                cvRecargosDetalle.Border = Rectangle.BOX;
                cvRecargosDetalle.BorderWidthTop = 0f;
                cvRecargosDetalle.HorizontalAlignment = Element.ALIGN_CENTER;
                cvRecargosDetalle.VerticalAlignment = Element.ALIGN_MIDDLE;
                cvRecargosDetalle.Padding = 4;

                var ciSubtotalNG = new PdfPCell(new Phrase("Valor IVA", fnt8));
                ciSubtotalNG.BorderColor = BaseColor.GRAY;
                ciSubtotalNG.Border = Rectangle.BOX;
                ciSubtotalNG.BorderWidthBottom = 0f;
                ciSubtotalNG.HorizontalAlignment = Element.ALIGN_CENTER;
                ciSubtotalNG.VerticalAlignment = Element.ALIGN_MIDDLE;
                ciSubtotalNG.Padding = 4;

                decimal taxExclusiveAmount = 200.0m; // Monto exclusivo de impuestos ficticio

                // Calcular el subtotal no gravado ficticio
                decimal subtotalNG = Math.Abs(subtotalPU - taxExclusiveAmount);

                // Convertir el subtotal no gravado a cadena
                string iSubtotalNG = subtotalNG.ToString("$#,###,##0.##");

                // Crear la celda con el valor del subtotal no gravado ficticio
                var cvSubtotalNG = new PdfPCell(new Phrase(iSubtotalNG, fnt9));
                cvSubtotalNG.BorderColor = BaseColor.GRAY;
                cvSubtotalNG.Border = Rectangle.BOX;
                cvSubtotalNG.BorderWidthBottom = 0f;
                cvSubtotalNG.HorizontalAlignment = Element.ALIGN_CENTER;
                cvSubtotalNG.VerticalAlignment = Element.ALIGN_MIDDLE;
                cvSubtotalNG.Padding = 4;

                var ciSubtotalBG = new PdfPCell(new Phrase("Impoconsumo", fnt8));
                ciSubtotalBG.BorderColor = BaseColor.GRAY;
                ciSubtotalBG.Border = Rectangle.BOX;
                ciSubtotalBG.BorderWidthTop = 0f;
                ciSubtotalBG.BorderWidthBottom = 0f;
                ciSubtotalBG.HorizontalAlignment = Element.ALIGN_CENTER;
                ciSubtotalBG.VerticalAlignment = Element.ALIGN_MIDDLE;
                ciSubtotalBG.Padding = 4;

                // Convertir el monto exclusivo de impuestos a cadena
                string iSubtotalBG = taxExclusiveAmount.ToString("$#,###,##0.##");

                // Crear la celda con el valor del monto exclusivo de impuestos ficticio
                var cvSubtotalBG = new PdfPCell(new Phrase(iSubtotalBG, fnt9));
                cvSubtotalBG.BorderColor = BaseColor.GRAY;
                cvSubtotalBG.Border = Rectangle.BOX;
                cvSubtotalBG.BorderWidthTop = 0f;
                cvSubtotalBG.BorderWidthBottom = 0f;
                cvSubtotalBG.HorizontalAlignment = Element.ALIGN_CENTER;
                cvSubtotalBG.VerticalAlignment = Element.ALIGN_MIDDLE;
                cvSubtotalBG.Padding = 4;

                var ciTotalImpuesto = new PdfPCell(new Phrase("Impto. Bolsas", fnt8));
                ciTotalImpuesto.BorderColor = BaseColor.GRAY;
                ciTotalImpuesto.Border = Rectangle.BOX;
                ciTotalImpuesto.BorderWidthTop = 0f;
                ciTotalImpuesto.BorderWidthBottom = 0f;
                ciTotalImpuesto.HorizontalAlignment = Element.ALIGN_CENTER;
                ciTotalImpuesto.VerticalAlignment = Element.ALIGN_MIDDLE;
                ciTotalImpuesto.Padding = 4;

                // Simulación de valores ficticios para el total de impuestos
                decimal totalImpuesto = 50.0m; // Total de impuestos ficticio

                // Convertir el total de impuestos a cadena
                string iTotalImpuesto = totalImpuesto.ToString("$#,###,##0.##");

                // Crear la celda con el valor del total de impuestos ficticio
                var cvTotalImpuesto = new PdfPCell(new Phrase(iTotalImpuesto, fnt9));
                cvTotalImpuesto.BorderColor = BaseColor.GRAY;
                cvTotalImpuesto.Border = Rectangle.BOX;
                cvTotalImpuesto.BorderWidthTop = 0f;
                cvTotalImpuesto.BorderWidthBottom = 0f;
                cvTotalImpuesto.HorizontalAlignment = Element.ALIGN_CENTER;
                cvTotalImpuesto.VerticalAlignment = Element.ALIGN_MIDDLE;
                cvTotalImpuesto.Padding = 4;

                var ciTotalMI = new PdfPCell(new Phrase("TOTAL (=)", fnt8));
                ciTotalMI.BorderColor = BaseColor.GRAY;
                ciTotalMI.Border = Rectangle.BOX;
                ciTotalMI.BorderWidthTop = 0f;
                ciTotalMI.HorizontalAlignment = Element.ALIGN_CENTER;
                ciTotalMI.VerticalAlignment = Element.ALIGN_MIDDLE;
                ciTotalMI.Padding = 4;

                // Simulación de valor ficticio para el total más impuesto
                decimal totalMI = 1200.0m; // Total más impuesto ficticio

                // Convertir el total más impuesto a cadena
                string iTotalMI = totalMI.ToString("$#,###,##0.##");

                // Crear la celda con el valor del total más impuesto ficticio
                var cvTotalMI = new PdfPCell(new Phrase(iTotalMI, fnt9));
                cvTotalMI.BorderColor = BaseColor.GRAY;
                cvTotalMI.Border = Rectangle.BOX;
                cvTotalMI.BorderWidthTop = 0f;
                cvTotalMI.HorizontalAlignment = Element.ALIGN_CENTER;
                cvTotalMI.VerticalAlignment = Element.ALIGN_MIDDLE;
                cvTotalMI.Padding = 4;

                var ciDescuentoGlobal = new PdfPCell(new Phrase("Rete. Fuente", fnt8));
                ciDescuentoGlobal.BorderColor = BaseColor.GRAY;
                ciDescuentoGlobal.Border = Rectangle.BOX;
                ciDescuentoGlobal.BorderWidthBottom = 0f;
                ciDescuentoGlobal.HorizontalAlignment = Element.ALIGN_CENTER;
                ciDescuentoGlobal.VerticalAlignment = Element.ALIGN_MIDDLE;
                ciDescuentoGlobal.Padding = 4;

                // Simulación de valor ficticio para el descuento global
                decimal descuentoGlobal = 100.0m; // Descuento global ficticio

                // Convertir el descuento global a cadena
                string iDescuentoGlobal = descuentoGlobal.ToString("$#,###,##0.##");

                // Crear la celda con el valor del descuento global ficticio
                var cvDescuentoGlobal = new PdfPCell(new Phrase(iDescuentoGlobal, fnt9));
                cvDescuentoGlobal.BorderColor = BaseColor.GRAY;
                cvDescuentoGlobal.Border = Rectangle.BOX;
                cvDescuentoGlobal.BorderWidthBottom = 0f;
                cvDescuentoGlobal.HorizontalAlignment = Element.ALIGN_CENTER;
                cvDescuentoGlobal.VerticalAlignment = Element.ALIGN_MIDDLE;
                cvDescuentoGlobal.Padding = 4;

                var ciRecargoGlobal = new PdfPCell(new Phrase("Rete. ICA", fnt8));
                ciRecargoGlobal.BorderColor = BaseColor.GRAY;
                ciRecargoGlobal.Border = Rectangle.BOX;
                ciRecargoGlobal.BorderWidthTop = 0f;
                ciRecargoGlobal.HorizontalAlignment = Element.ALIGN_CENTER;
                ciRecargoGlobal.VerticalAlignment = Element.ALIGN_MIDDLE;
                ciRecargoGlobal.Padding = 4;

                // Simulación de valor ficticio para el recargo global
                decimal recargoGlobal = 50.0m; // Recargo global ficticio

                // Convertir el recargo global a cadena
                string iRecargoGlobal = recargoGlobal.ToString("$#,###,##0.##");

                // Crear la celda con el valor del recargo global ficticio
                var cvRecargoGlobal = new PdfPCell(new Phrase(iRecargoGlobal, fnt9));
                cvRecargoGlobal.BorderColor = BaseColor.GRAY;
                cvRecargoGlobal.Border = Rectangle.BOX;
                cvRecargoGlobal.BorderWidthTop = 0f;
                cvRecargoGlobal.HorizontalAlignment = Element.ALIGN_CENTER;
                cvRecargoGlobal.VerticalAlignment = Element.ALIGN_MIDDLE;
                cvRecargoGlobal.Padding = 4;

                var ciAnticipo = new PdfPCell(new Phrase("Anticipo", fnt8));
                ciAnticipo.BorderColor = BaseColor.GRAY;
                ciAnticipo.Border = Rectangle.BOX;
                ciAnticipo.HorizontalAlignment = Element.ALIGN_CENTER;
                ciAnticipo.VerticalAlignment = Element.ALIGN_MIDDLE;
                ciAnticipo.Padding = 4;

                // Simulación de valor ficticio para el anticipo
                decimal anticipo = 300.0m; // Anticipo ficticio

                // Convertir el anticipo a cadena
                string iAnticipo = anticipo.ToString("$#,###,##0.##");

                // Crear la celda con el valor del anticipo ficticio
                var cvAnticipo = new PdfPCell(new Phrase(iAnticipo, fnt9));
                cvAnticipo.BorderColor = BaseColor.GRAY;
                cvAnticipo.Border = Rectangle.BOX;
                cvAnticipo.HorizontalAlignment = Element.ALIGN_CENTER;
                cvAnticipo.VerticalAlignment = Element.ALIGN_MIDDLE;
                cvAnticipo.Padding = 4;

                var ciTotalNeto = new PdfPCell(new Phrase("Total menos rete.", fnt8));
                ciTotalNeto.BorderColor = BaseColor.GRAY;
                ciTotalNeto.Border = Rectangle.BOX;
                ciTotalNeto.HorizontalAlignment = Element.ALIGN_CENTER;
                ciTotalNeto.VerticalAlignment = Element.ALIGN_MIDDLE;
                ciTotalNeto.Padding = 4;

                // Simulación de valor ficticio para el total neto
                decimal vTotalAP = 10570.20m; // Total neto ficticio

                // Convertir el total neto a cadena
                string iTotalNeto = vTotalAP.ToString("$#,###,##0.##");

                // Crear la celda con el valor del total neto ficticio
                var cvTotalNeto = new PdfPCell(new Phrase(iTotalNeto, fnt8));
                cvTotalNeto.BorderColor = BaseColor.GRAY;
                cvTotalNeto.Border = Rectangle.BOX;
                cvTotalNeto.HorizontalAlignment = Element.ALIGN_CENTER;
                cvTotalNeto.VerticalAlignment = Element.ALIGN_MIDDLE;
                cvTotalNeto.Padding = 4;
                string moneda = "COP";


                // Convertir el total neto a letras
                string iMontoLetras = montoALetras(vTotalAP, moneda).ToUpper();

                // Crear la celda para el monto en letras
                var cMontoLetras = new PdfPCell(new Phrase(iMontoLetras, fnt7));
                cMontoLetras.BorderColor = BaseColor.GRAY;
                cMontoLetras.Border = Rectangle.BOX;
                cMontoLetras.HorizontalAlignment = Element.ALIGN_CENTER;
                cMontoLetras.VerticalAlignment = Element.ALIGN_MIDDLE;
                cMontoLetras.Padding = 6;
                cMontoLetras.Colspan = 3;


                tTotales.AddCell(cDatos);
                tTotales.AddCell(ciSubtotalPU);
                tTotales.AddCell(cvSubtotalPU);
                tTotales.AddCell(ciDescuentosDetalle);
                tTotales.AddCell(cvDescuentosDetalle);
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
                tTotales.AddCell(ciAnticipo);
                tTotales.AddCell(cvAnticipo);
                tTotales.AddCell(ciTotalNeto);
                tTotales.AddCell(cvTotalNeto);
                tTotales.AddCell(cMontoLetras);

                tTotales.AddCell(cDatos);

                documento.Add(tTotales);

                // Espacio adicional
                documento.Add(new Phrase("   ", FontFactory.GetFont("Helvetica", 10, Font.NORMAL)));
                documento.Add(new Chunk("\n"));

                // Sección de firmas
                PdfPTable seccionFirmas = new PdfPTable(2);
                seccionFirmas.WidthPercentage = 100;
                seccionFirmas.SpacingBefore = 10f; // Espacio antes de la tabla
                seccionFirmas.SetWidths(new float[] { 1, 1 });

                // Firma y sello del cliente
                PdfPCell celdaFirmaCliente = new PdfPCell(new Phrase("Firma y sello del cliente", FontFactory.GetFont("Helvetica", 11, Font.NORMAL)));
                celdaFirmaCliente.HorizontalAlignment = Element.ALIGN_CENTER;
                celdaFirmaCliente.Border = PdfPCell.TOP_BORDER;
                celdaFirmaCliente.PaddingTop = 6;
                seccionFirmas.AddCell(celdaFirmaCliente);

                // Revisado y entregado
                PdfPCell celdaRevisadoEntregado = new PdfPCell(new Phrase("Revisado y entregado", FontFactory.GetFont("Helvetica", 11, Font.NORMAL)));
                celdaRevisadoEntregado.HorizontalAlignment = Element.ALIGN_CENTER;
                celdaRevisadoEntregado.Border = PdfPCell.TOP_BORDER;
                celdaRevisadoEntregado.PaddingTop = 6;
                seccionFirmas.AddCell(celdaRevisadoEntregado);
                documento.Add(seccionFirmas);

                // Espacio adicional después de las firmas
                documento.Add(new Phrase("   ", FontFactory.GetFont("Helvetica", 9, Font.NORMAL)));

                Paragraph nuevaSeccion = new Paragraph("Proveedor Tecnológico: RMSOFT CASA DE SOFTWARE  S.A.S. Nit 90074400-7", FontFactory.GetFont("Helvetica", 9, Font.NORMAL));
                documento.Add(nuevaSeccion);

                documento.Close();
            }
            catch (Exception ex)
            {
                // Manejar cualquier excepción que pueda ocurrir
                Console.WriteLine("Error al crear el PDF: " + ex.ToString());
            }
        }


        public static Image CrearQR(string texto)
        {
            var qrGenerator = new QRCoder.QRCodeGenerator();
            var qrData = qrGenerator.CreateQrCode(texto, QRCoder.QRCodeGenerator.ECCLevel.Q);
            var qrCode = new QRCoder.QRCode(qrData);
            var qrBitmap = qrCode.GetGraphic(1); 

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
