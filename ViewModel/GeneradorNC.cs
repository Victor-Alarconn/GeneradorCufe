using GeneradorCufe.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace GeneradorCufe.ViewModel
{
    public class GeneradorNC
    {
        public static void GeneradorNotaCredito(XDocument xmlDoc, Emisor emisor)
        {
            XNamespace cbc = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2";
            XNamespace cac = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2";

            // Actualizar 'CustomizationID'
            xmlDoc.Descendants(cbc + "CustomizationID").FirstOrDefault()?.SetValue("");

            // Actualizar 'ProfileExecutionID'
            xmlDoc.Descendants(cbc + "ProfileExecutionID").FirstOrDefault()?.SetValue("");

            // Actualizar 'ID'
            xmlDoc.Descendants(cbc + "ID").FirstOrDefault()?.SetValue("");

            // Actualizar 'UUID'
            xmlDoc.Descendants(cbc + "UUID").FirstOrDefault()?.SetValue("");
            xmlDoc.Descendants(cbc + "UUID").FirstOrDefault()?.SetAttributeValue("schemeID", "2");
            xmlDoc.Descendants(cbc + "UUID").FirstOrDefault()?.SetAttributeValue("schemeName", "CUDE-SHA384");

            // Actualizar 'IssueDate' y 'IssueTime'
            xmlDoc.Descendants(cbc + "IssueDate").FirstOrDefault()?.SetValue("");
            xmlDoc.Descendants(cbc + "IssueTime").FirstOrDefault()?.SetValue("");

            // Actualizar 'CreditNoteTypeCode'
            xmlDoc.Descendants(cbc + "CreditNoteTypeCode").FirstOrDefault()?.SetValue("");

            // Actualizar 'Note'
            xmlDoc.Descendants(cbc + "Note").FirstOrDefault()?.SetValue("");

            // Actualizar 'DocumentCurrencyCode'
            xmlDoc.Descendants(cbc + "DocumentCurrencyCode").FirstOrDefault()?.SetValue("COP");

            // Actualizar 'LineCountNumeric'
            xmlDoc.Descendants(cbc + "LineCountNumeric").FirstOrDefault()?.SetValue("");

            // Actualizar 'DiscrepancyResponse'
            var discrepancyResponseElement = xmlDoc.Descendants(cac + "DiscrepancyResponse").FirstOrDefault();
            if (discrepancyResponseElement != null)
            {
                discrepancyResponseElement.Element(cbc + "ReferenceID")?.SetValue("");
                discrepancyResponseElement.Element(cbc + "ResponseCode")?.SetValue("");
                discrepancyResponseElement.Element(cbc + "Description")?.SetValue("");
            }
        }
    }
}
