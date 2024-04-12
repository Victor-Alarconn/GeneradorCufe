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

            // Actualizar 'BillingReference'
            var billingReferenceElement = xmlDoc.Descendants(cac + "BillingReference").FirstOrDefault();
            if (billingReferenceElement != null)
            {
                var invoiceDocumentReferenceElement = billingReferenceElement.Element(cac + "InvoiceDocumentReference");
                if (invoiceDocumentReferenceElement != null)
                {
                    invoiceDocumentReferenceElement.Element(cbc + "ID")?.SetValue("SETT1");
                    invoiceDocumentReferenceElement.Element(cbc + "UUID")?.SetValue("b5c3b4f4aa53d3a14c3be6fdbde52c9b284723880c93fd4ed10d540a5e32a3f8b1c34cbadbe0ee253d1e50e0f6f8fa44");
                    invoiceDocumentReferenceElement.Element(cbc + "UUID")?.SetAttributeValue("schemeName", "CUFE-SHA384");
                    invoiceDocumentReferenceElement.Element(cbc + "IssueDate")?.SetValue("2019-06-04");
                }
            }
            MapearAccountingSupplierParty(xmlDoc);
        }

        public static void MapearAccountingSupplierParty(XDocument xmlDoc)
        {
            XNamespace cbc = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2";
            XNamespace cac = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2";

            var accountingSupplierPartyElement = xmlDoc.Descendants(cac + "AccountingSupplierParty").FirstOrDefault();
            if (accountingSupplierPartyElement != null)
            {
                accountingSupplierPartyElement.Element(cbc + "AdditionalAccountID")?.SetValue("1");

                var partyElement = accountingSupplierPartyElement.Element(cac + "Party");
                if (partyElement != null)
                {
                    var partyNameElement = partyElement.Element(cac + "PartyName");
                    if (partyNameElement != null)
                    {
                        partyNameElement.Element(cbc + "Name")?.SetValue("Cadena S.A.");
                    }

                    var physicalLocationElement = partyElement.Element(cac + "PhysicalLocation");
                    if (physicalLocationElement != null)
                    {
                        var addressElement = physicalLocationElement.Element(cac + "Address");
                        if (addressElement != null)
                        {
                            addressElement.Element(cbc + "ID")?.SetValue("05380");
                            addressElement.Element(cbc + "CityName")?.SetValue("LA ESTRELLA");
                            addressElement.Element(cbc + "PostalZone")?.SetValue("54321");
                            addressElement.Element(cbc + "CountrySubentity")?.SetValue("Antioquia");
                            addressElement.Element(cbc + "CountrySubentityCode")?.SetValue("05");
                            var addressLineElement = addressElement.Element(cac + "AddressLine");
                            if (addressLineElement != null)
                            {
                                addressLineElement.Element(cbc + "Line")?.SetValue("Cra. 50 #97a Sur-180 a 97a Sur-394");
                            }
                            var countryElement = addressElement.Element(cac + "Country");
                            if (countryElement != null)
                            {
                                countryElement.Element(cbc + "IdentificationCode")?.SetValue("CO");
                                countryElement.Element(cbc + "Name")?.SetValue("Colombia");
                            }
                        }

                        var partyTaxSchemeElement = partyElement.Element(cac + "PartyTaxScheme");
                        if (partyTaxSchemeElement != null)
                        {
                            partyTaxSchemeElement.Element(cbc + "RegistrationName")?.SetValue("Cadena S.A.");
                            partyTaxSchemeElement.Element(cbc + "CompanyID")?.SetValue("890930534");
                            partyTaxSchemeElement.Element(cbc + "CompanyID")?.SetAttributeValue("schemeID", "0");
                            partyTaxSchemeElement.Element(cbc + "CompanyID")?.SetAttributeValue("schemeName", "31");
                            partyTaxSchemeElement.Element(cbc + "CompanyID")?.SetAttributeValue("schemeAgencyID", "195");
                            partyTaxSchemeElement.Element(cbc + "CompanyID")?.SetAttributeValue("schemeAgencyName", "CO, DIAN (Dirección de Impuestos y Aduanas Nacionales)");
                            partyTaxSchemeElement.Element(cbc + "TaxLevelCode")?.SetValue("O-13");

                            var registrationAddressElement = partyTaxSchemeElement.Element(cac + "RegistrationAddress");
                            if (registrationAddressElement != null)
                            {
                                registrationAddressElement.Element(cbc + "ID")?.SetValue("05380");
                                registrationAddressElement.Element(cbc + "CityName")?.SetValue("LA ESTRELLA");
                                registrationAddressElement.Element(cbc + "PostalZone")?.SetValue("055468");
                                registrationAddressElement.Element(cbc + "CountrySubentity")?.SetValue("Antioquia");
                                registrationAddressElement.Element(cbc + "CountrySubentityCode")?.SetValue("05");
                                var addressLineRegistrationElement = registrationAddressElement.Element(cac + "AddressLine");
                                if (addressLineRegistrationElement != null)
                                {
                                    addressLineRegistrationElement.Element(cbc + "Line")?.SetValue("Cra. 50 #97a Sur-180 a 97a Sur-394");
                                }
                                var countryRegistrationElement = registrationAddressElement.Element(cac + "Country");
                                if (countryRegistrationElement != null)
                                {
                                    countryRegistrationElement.Element(cbc + "IdentificationCode")?.SetValue("CO");
                                    countryRegistrationElement.Element(cbc + "Name")?.SetValue("Colombia");
                                }
                            }

                            var taxSchemeElement = partyTaxSchemeElement.Element(cac + "TaxScheme");
                            if (taxSchemeElement != null)
                            {
                                taxSchemeElement.Element(cbc + "ID")?.SetValue("01");
                                taxSchemeElement.Element(cbc + "Name")?.SetValue("IVA");
                            }
                        }

                        var partyLegalEntityElement = partyElement.Element(cac + "PartyLegalEntity");
                        if (partyLegalEntityElement != null)
                        {
                            partyLegalEntityElement.Element(cbc + "RegistrationName")?.SetValue("Cadena S.A.");
                            partyLegalEntityElement.Element(cbc + "CompanyID")?.SetValue("890930534");
                            partyLegalEntityElement.Element(cbc + "CompanyID")?.SetAttributeValue("schemeID", "0");
                            partyLegalEntityElement.Element(cbc + "CompanyID")?.SetAttributeValue("schemeName", "31");
                            partyLegalEntityElement.Element(cbc + "CompanyID")?.SetAttributeValue("schemeAgencyID", "195");
                            partyLegalEntityElement.Element(cbc + "CompanyID")?.SetAttributeValue("schemeAgencyName", "CO, DIAN (Dirección de Impuestos y Aduanas Nacionales)");

                            var corporateRegistrationSchemeElement = partyLegalEntityElement.Element(cac + "CorporateRegistrationScheme");
                            if (corporateRegistrationSchemeElement != null)
                            {
                                corporateRegistrationSchemeElement.Element(cbc + "ID")?.SetValue("NC");
                                corporateRegistrationSchemeElement.Element(cbc + "Name")?.SetValue("1485596");
                            }
                        }

                        var contactElement = partyElement.Element(cac + "Contact");
                        if (contactElement != null)
                        {
                            contactElement.Element(cbc + "ElectronicMail")?.SetValue("lxxxxx@cadena.com.co");
                        }
                    }
                }
            }
        }

    }
}
