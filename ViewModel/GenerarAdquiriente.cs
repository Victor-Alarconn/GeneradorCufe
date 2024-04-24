using GeneradorCufe.Consultas;
using GeneradorCufe.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace GeneradorCufe.ViewModel
{
    public class GenerarAdquiriente
    {
        public static void MapAccountingCustomerParty(XDocument xmlDoc, string Nit, string cadenaConexion, Adquiriente adquiriente, Codigos codigos) // Información del adquiriente 
        { // esperelo aqui
            // Namespace específico para los elementos bajo 'sts'
            XNamespace sts = "dian:gov:co:facturaelectronica:Structures-2-1";
            // Namespace para elementos 'cbc'
            XNamespace cbc = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2";
            // Namespace para elementos 'cac'
            XNamespace cac = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2";


            string ciudadCompleta = codigos.Nombre_Municipio ?? "";
            adquiriente.Nombre_municipio_adqui = ciudadCompleta;
            string[] partesCiudad = ciudadCompleta.Split(',');
            string Municipio = partesCiudad.Length > 0 ? partesCiudad[0].Trim() : ""; // Obtiene el municipio (primer elemento después de dividir)
            string Departamento = partesCiudad.Length > 1 ? partesCiudad[1].Trim() : ""; // Obtiene el departamento (segundo elemento después de dividir)

            string Tipo = (adquiriente.Tipo_p == 1) ? "13" : "31";
            string AdditionalAccountID = (adquiriente.Tipo_p == 1) ? "2" : "1";

            // Información del adquiriente
            var accountingCustomerPartyElement = xmlDoc.Descendants(cac + "AccountingCustomerParty").FirstOrDefault();
            if (accountingCustomerPartyElement != null)
            {
                accountingCustomerPartyElement.Element(cbc + "AdditionalAccountID")?.SetValue(AdditionalAccountID); // Identificador de tipo de adquiriente  jurídica de la de persona

                // Información del adquiriente
                var partyElement = accountingCustomerPartyElement.Element(cac + "Party");
                if (partyElement != null)
                {
                    var partyIdentificationElement = partyElement.Element(cac + "PartyIdentification");
                    if (partyIdentificationElement != null)
                    {
                        var idElement = partyIdentificationElement.Element(cbc + "ID");
                        if (idElement != null)
                        {
                            idElement.Value = adquiriente.Nit_adqui;
                            idElement.SetAttributeValue("schemeName", Tipo); // cambio de 31 a 13
                        }


                        if (partyElement != null)
                        {
                            partyElement.Element(cac + "PartyName")?.Element(cbc + "Name")?.SetValue(adquiriente.Nombre_adqu);

                            // Información de ubicación física del adquiriente
                            var physicalLocationElement = partyElement.Element(cac + "PhysicalLocation");
                            if (physicalLocationElement != null)
                            {
                                physicalLocationElement.Element(cac + "Address")?.Element(cbc + "ID")?.SetValue(codigos.Codigo_Municipio);
                                physicalLocationElement.Element(cac + "Address")?.Element(cbc + "CityName")?.SetValue(Municipio);
                                physicalLocationElement.Element(cac + "Address")?.Element(cbc + "PostalZone")?.SetValue("660001");
                                physicalLocationElement.Element(cac + "Address")?.Element(cbc + "CountrySubentity")?.SetValue(codigos.Codigo_Departamento);
                                physicalLocationElement.Element(cac + "Address")?.Element(cbc + "CountrySubentityCode")?.SetValue(Departamento);
                                physicalLocationElement.Element(cac + "Address")?.Element(cac + "Country")?.Element(cbc + "IdentificationCode")?.SetValue("CO");
                                physicalLocationElement.Element(cac + "Address")?.Element(cac + "Country")?.Element(cbc + "Name")?.SetValue("Colombia");
                                physicalLocationElement.Element(cac + "Address")?.Element(cac + "AddressLine")?.Element(cbc + "Line")?.SetValue(adquiriente.Direccion_adqui);
                            }

                            // Información tributaria del adquiriente
                            var partyTaxSchemeElement = partyElement.Element(cac + "PartyTaxScheme");
                            if (partyTaxSchemeElement != null)
                            {
                                // Establecer el nombre de registro
                                partyTaxSchemeElement.Element(cbc + "RegistrationName")?.SetValue(adquiriente.Nombre_adqu);

                                // Establecer el ID de la compañía
                                var companyIDElement = partyTaxSchemeElement.Element(cbc + "CompanyID");
                                if (companyIDElement != null)
                                {
                                    companyIDElement.SetValue(adquiriente.Nit_adqui);
                                    companyIDElement.SetAttributeValue("schemeID", adquiriente.Dv_Adqui);
                                    companyIDElement.SetAttributeValue("schemeName", Tipo);
                                    companyIDElement.SetAttributeValue("schemeAgencyID", "195");
                                    companyIDElement.SetAttributeValue("schemeAgencyName", "CO, DIAN (Dirección de Impuestos y Aduanas Nacionales)");
                                }

                                // Establecer el código de nivel de impuestos
                                partyTaxSchemeElement.Element(cbc + "TaxLevelCode")?.SetValue("R-99-PN");

                                // Información de ubicación de registro tributario del adquiriente
                                var registrationAddressElement = partyTaxSchemeElement.Element(cac + "RegistrationAddress");
                                if (registrationAddressElement != null)
                                {
                                    registrationAddressElement.Element(cbc + "ID")?.SetValue(codigos.Codigo_Municipio);
                                    registrationAddressElement.Element(cbc + "CityName")?.SetValue(Municipio);
                                    registrationAddressElement.Element(cbc + "PostalZone")?.SetValue("660001");
                                    registrationAddressElement.Element(cbc + "CountrySubentity")?.SetValue(codigos.Codigo_Departamento);
                                    registrationAddressElement.Element(cbc + "CountrySubentityCode")?.SetValue(Departamento);
                                    registrationAddressElement.Element(cac + "Country")?.Element(cbc + "IdentificationCode")?.SetValue("CO");
                                    registrationAddressElement.Element(cac + "Country")?.Element(cbc + "Name")?.SetValue("Colombia");
                                    registrationAddressElement.Element(cac + "AddressLine")?.Element(cbc + "Line")?.SetValue(adquiriente.Direccion_adqui);
                                }

                                // Información del esquema tributario del adquiriente
                                var taxSchemeElement = partyTaxSchemeElement.Element(cac + "TaxScheme");
                                if (taxSchemeElement != null)
                                {
                                    taxSchemeElement.Element(cbc + "ID")?.SetValue("01");
                                    taxSchemeElement.Element(cbc + "Name")?.SetValue("IVA");
                                }
                            }

                            // Información legal del adquiriente
                            var partyLegalEntityElement = partyElement.Element(cac + "PartyLegalEntity");
                            if (partyLegalEntityElement != null)
                            {
                                // Establecer el nombre de registro
                                partyLegalEntityElement.Element(cbc + "RegistrationName")?.SetValue(adquiriente.Nombre_adqu);

                                // Establecer el ID de la compañía
                                var companyIDElement = partyLegalEntityElement.Element(cbc + "CompanyID");
                                if (companyIDElement != null)
                                {
                                    companyIDElement.SetValue(adquiriente.Nit_adqui);
                                    companyIDElement.SetAttributeValue("schemeID", adquiriente.Dv_Adqui);
                                    companyIDElement.SetAttributeValue("schemeName", Tipo);
                                    companyIDElement.SetAttributeValue("schemeAgencyID", "195");
                                    companyIDElement.SetAttributeValue("schemeAgencyName", "CO, DIAN (Dirección de Impuestos y Aduanas Nacionales)");
                                }
                            }


                            // Información de contacto del adquiriente
                            var contactElement = partyElement.Element(cac + "Contact");
                            if (contactElement != null)
                            {
                                contactElement.Element(cbc + "Telephone")?.SetValue(adquiriente.Telefono_adqui);
                                contactElement.Element(cbc + "ElectronicMail")?.SetValue(adquiriente.Correo_adqui);
                            }
                        }
                    }
                }
            }
        }
    }
}
