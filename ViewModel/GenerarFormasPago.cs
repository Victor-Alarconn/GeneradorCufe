﻿using GeneradorCufe.Model;
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

        public static void GenerarFormaPagos(XDocument xmlDoc, List<FormaPago> listaFormaPago)
        {
            try
            {
                XNamespace cbc = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2";
                XNamespace cac = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2";

                if (listaFormaPago != null && listaFormaPago.Count > 0)
                {
                    // Obtener la plantilla para PaymentMeans
                    var paymentMeansTemplate = xmlDoc.Descendants(cac + "PaymentMeans").FirstOrDefault();

                    // Modificar la plantilla para reemplazar los valores entre llaves
                    paymentMeansTemplate.Element(cbc + "ID")?.SetValue("{PaymentMeansID}");
                    paymentMeansTemplate.Element(cbc + "PaymentMeansCode")?.SetValue("{PaymentMeansPaymentMeansCode}");
                    paymentMeansTemplate.Element(cbc + "PaymentID")?.SetValue("{PaymentMeansPaymentID}");

                    foreach (var formaPago in listaFormaPago)
                    {
                        // Clonar la plantilla para cada forma de pago
                        var paymentMeansElementCloned = new XElement(paymentMeansTemplate);

                        // Reemplazar los valores entre llaves con los valores correspondientes
                        paymentMeansElementCloned.Element(cbc + "ID")?.SetValue("1");

                        if (formaPago.Id_forma == "00")
                        {
                            paymentMeansElementCloned.Element(cbc + "PaymentMeansCode")?.SetValue("10");
                            paymentMeansElementCloned.Element(cbc + "PaymentID")?.SetValue("Efectivo");
                        }
                        else if (formaPago.Id_forma == "01")
                        {
                            paymentMeansElementCloned.Element(cbc + "PaymentMeansCode")?.SetValue("49");
                            paymentMeansElementCloned.Element(cbc + "PaymentID")?.SetValue("Tarjeta Débito");
                        }
                        else if (formaPago.Id_forma == "02")
                        {
                            paymentMeansElementCloned.Element(cbc + "PaymentMeansCode")?.SetValue("48");
                            paymentMeansElementCloned.Element(cbc + "PaymentID")?.SetValue("Tarjeta Crédito");
                            paymentMeansElementCloned.Add(new XElement(cbc + "PaymentDueDate", DateTime.Now.ToString("yyyy-MM-dd")));
                        }
                        else if (formaPago.Id_forma == "03")
                        {
                            paymentMeansElementCloned.Element(cbc + "PaymentMeansCode")?.SetValue("47");
                            paymentMeansElementCloned.Element(cbc + "PaymentID")?.SetValue("Transferencia Débito Bancaria");
                          //  paymentMeansElementCloned.Add(new XElement(cbc + "PaymentDueDate", DateTime.Now.ToString("yyyy-MM-dd")));
                        }

                        // Agregar PaymentMeans al XML después de la plantilla existente
                        paymentMeansTemplate.AddAfterSelf(paymentMeansElementCloned);
                    }

                    // Eliminar la plantilla de PaymentMeans después de agregar los nuevos métodos de pago
                    paymentMeansTemplate.Remove();
                }
                else
                {
                    Console.WriteLine("La lista de formas de pago está vacía.");
                }
            }
            catch (Exception ex)
            {
                // Manejar la excepción
                Console.WriteLine("Error al generar formas de pago: " + ex.Message);
            }
        }






    }
}
