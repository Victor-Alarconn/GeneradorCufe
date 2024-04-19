using GeneradorCufe.Model;
using Org.BouncyCastle.Crypto.Macs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace GeneradorCufe.ViewModel
{
    public class GenerarFormasPago
    {

        public void GenerarFormaPagos(XDocument xmlDoc, List<FormaPago> listaFormaPago)
        {
            XNamespace cbc = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2";
            XNamespace cac = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2";

            // Obtener el elemento padre de PaymentMeans
            var paymentMeansElementParent = xmlDoc.Descendants(cac + "Invoice").FirstOrDefault()?.Element(cac + "PaymentMeans");

            // Crear una plantilla de PaymentMeans
            var paymentMeansTemplate = new XElement(cac + "PaymentMeans",
                new XElement(cbc + "ID", "1")); // ID fijo

            // Agregar la plantilla al XML antes del bucle foreach
            paymentMeansElementParent?.Add(paymentMeansTemplate);

            if (listaFormaPago != null && listaFormaPago.Count > 0)
            {
                foreach (var formaPago in listaFormaPago)
                {
                    // Clonar la plantilla para cada forma de pago
                    var paymentMeansElement = new XElement(paymentMeansTemplate);

                    // Asignar PaymentMeansCode y PaymentID según el Id_forma de la forma de pago
                    if (formaPago.Id_forma == "00")
                    {
                        paymentMeansElement.Element(cbc + "PaymentMeansCode")?.SetValue("10");
                        paymentMeansElement.Element(cbc + "PaymentID")?.SetValue("  ");
                    }
                    else if (formaPago.Id_forma == "01")
                    {
                        paymentMeansElement.Element(cbc + "PaymentMeansCode")?.SetValue("49");
                        paymentMeansElement.Element(cbc + "PaymentID")?.SetValue("Tarjeta Débito");
                    }
                    else if (formaPago.Id_forma == "99")
                    {
                        paymentMeansElement.Element(cbc + "PaymentMeansCode")?.SetValue("48");
                        paymentMeansElement.Element(cbc + "PaymentID")?.SetValue("Tarjeta Crédito");
                        paymentMeansElement.Element(cbc + "PaymentDueDate")?.SetValue(DateTime.Now.ToString("yyyy-MM-dd"));
                    }

                    // Agregar PaymentMeans al XML
                    paymentMeansElementParent?.Add(paymentMeansElement);
                }
            }

            // Eliminar la plantilla del XML después del bucle foreach
            paymentMeansTemplate?.Remove();
        }


    }
}
