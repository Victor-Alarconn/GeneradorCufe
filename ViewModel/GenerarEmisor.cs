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
    public class GenerarEmisor
    {
        public static void MapearInformacionEmisor(XDocument xmlDoc, Emisor emisor, Encabezado encabezado, Codigos codigos, List<Productos> listaProductos)
        {
            XNamespace cbc = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2";
            XNamespace cac = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2";

            Codigos_Consulta codigosConsulta = new Codigos_Consulta();
            string TipoA = (emisor.Tipo_emisor == 1) ? "2" : "1";
            string Identificador = (emisor.Tipo_emisor == 1) ? "13" : "31";

            string ciudadCompleta = codigos.Nombre_Municipio ?? "";
            string[] partesCiudad = ciudadCompleta.Split(',');
            string Municipio = partesCiudad.Length > 0 ? partesCiudad[0].Trim() : ""; // Obtiene el municipio (primer elemento después de dividir)
            string Departamento = partesCiudad.Length > 1 ? partesCiudad[1].Trim() : ""; // Obtiene el departamento (segundo elemento después de dividir)
            

            string nitCompleto = emisor.Nit_emisor ?? "";
            string[] partesNit = nitCompleto.Split('-');
            string Nit = partesNit.Length > 0 ? partesNit[0] : ""; // Obtiene la parte antes del guion
            string Dv = partesNit.Length > 1 ? partesNit[1] : ""; // Obtiene el dígito verificador después del guion

            xmlDoc.Descendants(cbc + "AdditionalAccountID").FirstOrDefault()?.SetValue(TipoA); // Identificador de tipo de organización jurídica de la persona

            var partyNameElement = xmlDoc.Descendants(cac + "Party")
                                          .Descendants(cac + "PartyName")
                                          .Descendants(cbc + "Name").FirstOrDefault();
            if (partyNameElement != null)
            {
                partyNameElement.SetValue(emisor.Nombre_emisor);
            }

            // Actualizar detalles de 'PhysicalLocation'
            var physicalLocationElement = xmlDoc.Descendants(cac + "PhysicalLocation").FirstOrDefault();
            if (physicalLocationElement != null)
            {
                physicalLocationElement.Descendants(cbc + "ID").FirstOrDefault()?.SetValue(codigos.Codigo_Municipio);
                physicalLocationElement.Descendants(cbc + "CityName").FirstOrDefault()?.SetValue(Municipio);
                physicalLocationElement.Descendants(cbc + "PostalZone").FirstOrDefault()?.SetValue("660001");
                physicalLocationElement.Descendants(cbc + "CountrySubentity").FirstOrDefault()?.SetValue(Departamento);
                physicalLocationElement.Descendants(cbc + "CountrySubentityCode").FirstOrDefault()?.SetValue(codigos.Codigo_Departamento);
                physicalLocationElement.Descendants(cac + "AddressLine")
                                       .Descendants(cbc + "Line").FirstOrDefault()?.SetValue(emisor.Direccion_emisor);
            }

            // Actualizaciones para 'PartyTaxScheme'
            var partyTaxSchemeElement = xmlDoc.Descendants(cac + "PartyTaxScheme").FirstOrDefault();
            if (partyTaxSchemeElement != null)
            {
                partyTaxSchemeElement.Element(cbc + "RegistrationName")?.SetValue(emisor.Nombre_emisor);

                partyTaxSchemeElement.Element(cbc + "CompanyID")?.SetValue(Nit);
                partyTaxSchemeElement.Element(cbc + "CompanyID")?.SetAttributeValue("schemeID", Dv);
                partyTaxSchemeElement.Element(cbc + "CompanyID")?.SetAttributeValue("schemeName", Identificador);
                partyTaxSchemeElement.Element(cbc + "CompanyID")?.SetAttributeValue("schemeAgencyID", "195");
                partyTaxSchemeElement.Element(cbc + "CompanyID")?.SetAttributeValue("schemeAgencyName", "CO, DIAN (Dirección de Impuestos y Aduanas Nacionales)");

                string valorTaxLevelCode = emisor.Retiene_emisor == 3 ? "O-15" : "R-99-PN";
                partyTaxSchemeElement.Element(cbc + "TaxLevelCode")?.SetValue(valorTaxLevelCode);


                var registrationAddressElement = partyTaxSchemeElement.Element(cac + "RegistrationAddress");
                if (registrationAddressElement != null)
                {
                    registrationAddressElement.Element(cbc + "ID")?.SetValue(codigos.Codigo_Municipio);
                    registrationAddressElement.Element(cbc + "CityName")?.SetValue(Municipio);
                    registrationAddressElement.Element(cbc + "PostalZone")?.SetValue("660001");
                    registrationAddressElement.Element(cbc + "CountrySubentity")?.SetValue(Departamento);
                    registrationAddressElement.Element(cbc + "CountrySubentityCode")?.SetValue(codigos.Codigo_Departamento);
                    registrationAddressElement.Element(cac + "AddressLine").Element(cbc + "Line")?.SetValue(emisor.Direccion_emisor);

                    var countryElement = registrationAddressElement.Element(cac + "Country");
                    if (countryElement != null)
                    {
                        countryElement.Element(cbc + "IdentificationCode")?.SetValue("CO");
                        var nameElement = countryElement.Element(cbc + "Name");
                        if (nameElement != null)
                        {
                            nameElement.SetValue("Colombia");
                            nameElement.SetAttributeValue("languageID", "es");
                        }
                    }
                }

                string Aplica = emisor.Responsable_emisor;
                var taxSchemeElement = partyTaxSchemeElement.Element(cac + "TaxScheme");

                if (taxSchemeElement != null)
                {
                    if (Aplica == "Responsable IVA")
                    {
                        taxSchemeElement.Element(cbc + "ID")?.SetValue("01"); // ID es 01 si es Responsable IVA
                        taxSchemeElement.Element(cbc + "Name")?.SetValue("IVA"); // Name es IVA si es Responsable IVA
                    }
                    else
                    {
                        taxSchemeElement.Element(cbc + "ID")?.SetValue("ZZ"); // ID es ZZ si no es Responsable IVA
                        taxSchemeElement.Element(cbc + "Name")?.SetValue("No aplica"); // Name es No aplica si no es Responsable IVA
                    }
                }
            }

            // Mapeo para PartyLegalEntity
            var partyLegalEntityElement = xmlDoc.Descendants(cac + "PartyLegalEntity").FirstOrDefault();
            if (partyLegalEntityElement != null)
            {
                partyLegalEntityElement.Element(cbc + "RegistrationName")?.SetValue(emisor.Nombre_emisor);
                // Establecer el valor de CompanyID y sus atributos si existe
                var companyIDElement = partyLegalEntityElement.Element(cbc + "CompanyID");
                if (companyIDElement != null)
                {
                    companyIDElement.SetValue(Nit);
                    companyIDElement.SetAttributeValue("schemeID", Dv);
                    companyIDElement.SetAttributeValue("schemeName", Identificador);
                    companyIDElement.SetAttributeValue("schemeAgencyID", "195");
                    companyIDElement.SetAttributeValue("schemeAgencyName", "CO, DIAN (Dirección de Impuestos y Aduanas Nacionales)");
                }

                var corporateRegistrationSchemeElement = partyLegalEntityElement.Element(cac + "CorporateRegistrationScheme");
                if (corporateRegistrationSchemeElement != null && listaProductos.Any() && string.IsNullOrEmpty(listaProductos.First().Recibo))
                {
                    corporateRegistrationSchemeElement.Element(cbc + "ID")?.SetValue(encabezado.Prefijo);
                }
                else
                {
                    corporateRegistrationSchemeElement.Element(cbc + "ID")?.SetValue("NC");
                }
            }

            // Función para formatear el número de teléfono
            string FormatearTelefono(string telefono)
            {
                // Remover paréntesis y guiones del número de teléfono
                string telefonoFormateado = telefono.Replace("(", "").Replace(")", "").Replace("-", "");

                // Devolver el número formateado
                return telefonoFormateado;
            }

            // Obtener el teléfono formateado
            string telefonoFormateado = FormatearTelefono(emisor.Telefono_emisor);

            // Mapeo para Contact
            var contactElement = xmlDoc.Descendants(cac + "Contact").FirstOrDefault();
            if (contactElement != null)
            {
                contactElement.Element(cbc + "Telephone")?.SetValue(telefonoFormateado); // Asignar el teléfono
                contactElement.Element(cbc + "ElectronicMail")?.SetValue(emisor.Correo_emisor); // Asignar el correo electrónico
            }
        }

    }
}
