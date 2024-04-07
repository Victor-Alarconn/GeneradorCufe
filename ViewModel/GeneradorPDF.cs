
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

namespace GeneradorCufe.ViewModel
{
    public class GeneradorPDF
    {

        public  GeneradorPDF()
        {
            
        }

        public void CrearPDF(string rutaArchivo)
        {
            try
            {
                Document documento = new Document(PageSize.A4, 50, 50, 25, 25);
                PdfWriter.GetInstance(documento, new FileStream(rutaArchivo, FileMode.Create));
                documento.Open();
                documento.Add(new Paragraph("Hola"));
                documento.Close();
            }
            catch (Exception ex)
            {
                // Manejar cualquier excepción que pueda ocurrir
                Console.WriteLine("Error al crear el PDF: " + ex.ToString());
            }
        }








    }
}
