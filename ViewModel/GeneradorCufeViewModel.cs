using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using static GeneradorCufe.Model.GeneradorCufeModel;

namespace GeneradorCufe.ViewModel
{

        public class InvoiceViewModel
        {
        public DianExtensions MyDianExtensions { get; set; }
        public Invoice MyInvoice { get; set; }
        public UBLExtensions MyUBLExtensions { get; set; }

        public InvoiceViewModel()
        {
            // Inicialización de MyUBLExtensions y DianExtensions
            MyUBLExtensions = new UBLExtensions
            {
                UBLExtension = new UBLExtension
                {
                    ExtensionContent = new ExtensionContent
                    {
                        DianExtensions = new DianExtensions
                        {
                            InvoiceControl = new InvoiceControl
                            {
                                InvoiceAuthorization = 18760000001,

                                AuthorizedInvoices = new AuthorizedInvoices
                                {
                                    Prefix = "SETT",
                                    From = 1,
                                    To = 5000000
                                }
                            }
                        }
                    }
                }
            };
            // Crear y configurar AuthorizationPeriod
            AuthorizationPeriod authorizationPeriod = new AuthorizationPeriod();
            authorizationPeriod.SetDates(new DateTime(2019, 1, 19), new DateTime(2030, 1, 19));

            // Asignar AuthorizationPeriod al InvoiceControl
            MyUBLExtensions.UBLExtension.ExtensionContent.DianExtensions.InvoiceControl.AuthorizationPeriod = authorizationPeriod;

            // Inicialización independiente de MyInvoice con sus propias propiedades
            MyInvoice = new Invoice
            {
                UBLExtensions = MyUBLExtensions,
                CustomizationID = 10,
                ProfileExecutionID = 2,
                ID = "SETT1",
                UUID = new UUID
                {
                    SchemeID = 2,
                    SchemeName = "CUFE-SHA384",
                    Text = "f1c96d3ff4fc199817fa21ea2bc8a929b9b8c8b0fb50db6885ea48470e9ebabef994094272dbab11ddc93d8893dacb69"
                },
                InvoiceTypeCode = "01",
                Note = "Prueba Factura Electronica Datos Reales 2",
                DocumentCurrencyCode = "COP",
                LineCountNumeric = 1,
                // Inicializar AccountingSupplierParty y otras propiedades necesarias aquí
            };
            DateTime issueDate = new DateTime(2022, 4, 7);
            DateTime issueTime = DateTime.ParseExact("12:53:36-05:00", "HH:mm:ssK", CultureInfo.InvariantCulture);

            MyInvoice.SetIssueDateAndTime(issueDate, issueTime);

            MyInvoice.AccountingSupplierParty = new AccountingSupplierParty
            {
                AdditionalAccountID = 1,
                Party = new Party
                {
                    PartyName = new PartyName { Name = "Cadena S.A." },
                    PhysicalLocation = new PhysicalLocation
                    {
                        Address = new Address
                        {
                            ID = "05380",
                            CityName = "LA ESTRELLA",
                            PostalZone = "055460",
                            CountrySubentity = "Antioquia",
                            CountrySubentityCode = "05",
                            AddressLine = new AddressLine { Line = "Cra. 50 #97a Sur-180 a 97a Sur-394" },
                            Country = new Country
                            {
                                IdentificationCode = "CO",
                                Name = new Name { Text = "Colombia", LanguageID = "es" } // Error
                            }
                        }
                    },
                    PartyTaxScheme = new PartyTaxScheme
                    {
                        RegistrationName = "Cadena S.A.",
                        CompanyID = new CompanyID
                        {
                            Text = 890930534,
                            SchemeID = 0,
                            SchemeName = 31,
                            SchemeAgencyID = 195,
                            SchemeAgencyName = "CO, DIAN (Dirección de Impuestos y Aduanas Nacionales)"
                        },
                        TaxLevelCode = "R-99-PN",
                        RegistrationAddress = new RegistrationAddress
                        {
                            ID = "05380",
                            CityName = "LA ESTRELLA",
                            PostalZone = "055460",
                            CountrySubentity = "Antioquia",
                            CountrySubentityCode = "05",
                            AddressLine = new AddressLine { Line = "Cra. 50 #97a Sur-180 a 97a Sur-394" },
                            Country = new Country
                            {
                                IdentificationCode = "CO",
                                Name = new Name { Text = "Colombia", LanguageID = "es" }
                            }
                        },
                        TaxScheme = new TaxScheme
                        {
                            ID = "01",
                            Name = "IVA"
                        }
                    },
                    PartyLegalEntity = new PartyLegalEntity
                    {
                        RegistrationName = "Cadena S.A.",
                        CompanyID = new CompanyID
                        {
                            Text = 890930534,
                            SchemeID = 0,
                            SchemeName = 31,
                            SchemeAgencyID = 195,
                            SchemeAgencyName = "CO, DIAN (Dirección de Impuestos y Aduanas Nacionales)"
                        },
                        CorporateRegistrationScheme = new CorporateRegistrationScheme
                        {
                            ID = "SETT",
                            Name = "1485596",
                        },
                    },


                    Contact = new Contact
                    {
                        ElectronicMail = "xxxxx@xxxxx.com.co"
                    }
                }
            };

            MyInvoice.AccountingCustomerParty = new AccountingCustomerParty
            {
                AdditionalAccountID = 2,
                PartyIdentification = new PartyIdentification
                {
                    ID = new ID //Error
                    {
                        SchemeName = 13,
                        Text = 1017173008,
                    }
                },
                Party = new Party
                {
                    PartyName = new PartyName { Name = "ADQUIRIENTE DE EJEMPLO" },
                    PhysicalLocation = new PhysicalLocation
                    {
                        Address = new Address
                        {
                            ID = "66001",
                            CityName = "PEREIRA",
                            PostalZone = "54321",
                            CountrySubentity = "Risaralda",
                            CountrySubentityCode = "66",
                            AddressLine = new AddressLine { Line = "CR 9 A N0 99 - 07 OF 802" },
                            Country = new Country
                            {
                                IdentificationCode = "CO",
                                Name = new Name { Text = "Colombia", LanguageID = "es" }
                            }
                        }
                    },
                    PartyTaxScheme = new PartyTaxScheme
                    {
                        RegistrationName = "ADQUIRIENTE DE EJEMPLO",
                        CompanyID = new CompanyID
                        {
                            Text = 1017173008,
                            SchemeID = 3,
                            SchemeName = 31,
                            SchemeAgencyID = 195,
                            SchemeAgencyName = "CO, DIAN (Dirección de Impuestos y Aduanas Nacionales)"
                        },
                        TaxLevelCode = "R-99-PN",
                        RegistrationAddress = new RegistrationAddress
                        {
                            ID = "66001",
                            CityName = "PEREIRA",
                            PostalZone = "54321",
                            CountrySubentity = "Risaralda",
                            CountrySubentityCode = "66",
                            AddressLine = new AddressLine { Line = "CR 9 A N0 99 - 07 OF 802" },
                            Country = new Country
                            {
                                IdentificationCode = "CO",
                                Name = new Name { Text = "Colombia", LanguageID = "es" }
                            }
                        },
                        TaxScheme = new TaxScheme
                        {
                            ID = "01",
                            Name = "IVA"
                        }
                    },
                    PartyLegalEntity = new PartyLegalEntity
                    {
                        RegistrationName = "ADQUIRIENTE DE EJEMPLO",
                        CompanyID = new CompanyID
                        {
                            Text = 1017173008,
                            SchemeID = 3,
                            SchemeName = 31,
                            SchemeAgencyID = 195,
                            SchemeAgencyName = "CO, DIAN (Dirección de Impuestos y Aduanas Nacionales)"
                        },
                        CorporateRegistrationScheme = new CorporateRegistrationScheme
                        {
                            Name = "1485596",
                        }
                    },
                    Contact = new Contact
                    {
                        ElectronicMail = "xxxx@xxxx.com"
                    }
                }
            };

            MyInvoice.PaymentMeans = new PaymentMeans
            {
                ID = "1",
                PaymentMeansCode = 10,
                PaymentID = "Efectivo",
            };

            MyInvoice.TaxTotal = new TaxTotal
            {
                TaxAmount = new TaxAmount
                {
                    CurrencyID = "COP",
                    Text = 19000.00.ToString("F2", CultureInfo.InvariantCulture)
                },
                TaxSubtotal = new TaxSubtotal
                {
                    TaxableAmount = new TaxableAmount
                    {
                        CurrencyID = "COP",
                        Text = 100000.00.ToString("F2", CultureInfo.InvariantCulture)
                    },
                    TaxAmount = new TaxAmount
                    {
                        CurrencyID = "COP",
                        Text = 19000.00.ToString("F2", CultureInfo.InvariantCulture)
                    },
                    TaxCategory = new TaxCategory
                    {
                        Percent = 19.00.ToString("F2", CultureInfo.InvariantCulture),
                        TaxScheme = new TaxScheme
                        {
                            ID = "01",
                            Name = "IVA"
                        }
                    }
                }
            };

            MyInvoice.LegalMonetaryTotal = new LegalMonetaryTotal
            {
                LineExtensionAmount = new LineExtensionAmount
                {
                    CurrencyID = "COP",
                    Text = 100000.00.ToString("F2", CultureInfo.InvariantCulture)
                },
                TaxExclusiveAmount = new TaxExclusiveAmount
                {
                    CurrencyID = "COP",
                    Text = 100000.00.ToString("F2", CultureInfo.InvariantCulture)
                },
                TaxInclusiveAmount = new TaxInclusiveAmount
                {
                    CurrencyID = "COP",
                    Text = 119000.00.ToString("F2", CultureInfo.InvariantCulture)
                },
                PayableAmount = new PayableAmount
                {
                    CurrencyID = "COP",
                    Text = 119000.00.ToString("F2", CultureInfo.InvariantCulture)
                }
            };

            MyInvoice.InvoiceLine = new InvoiceLine
            {
                ID = "1",
                InvoicedQuantity = 1.00.ToString("F2", CultureInfo.InvariantCulture),
                LineExtensionAmount = new LineExtensionAmount
                {
                    CurrencyID = "COP",
                    Text = 100000.00.ToString("F2", CultureInfo.InvariantCulture)
                },
                TaxTotal = new TaxTotal
                {
                    TaxAmount = new TaxAmount
                    {
                        CurrencyID = "COP",
                        Text = 19000.00.ToString("F2", CultureInfo.InvariantCulture)
                    },
                    TaxSubtotal = new TaxSubtotal
                    {
                        TaxableAmount = new TaxableAmount
                        {
                            CurrencyID = "COP",
                            Text = 100000.00.ToString("F2", CultureInfo.InvariantCulture)
                        },
                        TaxAmount = new TaxAmount
                        {
                            CurrencyID = "COP",
                            Text = 19000.00.ToString("F2", CultureInfo.InvariantCulture)
                        },
                        TaxCategory = new TaxCategory
                        {
                            Percent = 19.00.ToString("F2", CultureInfo.InvariantCulture),
                            TaxScheme = new TaxScheme
                            {
                                ID = "01",
                                Name = "IVA"
                            }
                        }
                    }
                },
                Item = new Item
                {
                    Description = "Frambuesa",
                    StandardItemIdentification = new StandardItemIdentification
                    {
                        SchemeID = "999",
                        ID = "03222314-7"
                       
                    }
                },
                Price = new Price
                {
                    PriceAmount = new PriceAmount
                    {
                        CurrencyID = "COP",
                        Text = 100000.00.ToString("F2", CultureInfo.InvariantCulture)

                    },

                    BaseQuantity = new BaseQuantity
                    {
                        UnitCode = "EA",
                        Text = 1.00.ToString("F2", CultureInfo.InvariantCulture)
                    }


                }
            };

            MyInvoice.DATA = new DATA
            {
                UBL21 = true,

                Partnership = new Partnership
                {
                    ID = 99999999,
                    TechKey = "fc8eac422eba16e22ffd8c6f94b3f40a6e38162c",
                    SetTestID = "b9ca446f-395e-44e2-847d-300e1a0f61fe"

                }
            };
        }



        public string GenerateXML(Invoice MyInvoice)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Invoice));
            XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();

            // Añadir espacios de nombres
            namespaces.Add("ds", "http://www.w3.org/2000/09/xmldsig#");
            namespaces.Add("cac", "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2");
            namespaces.Add("cbc", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
            namespaces.Add("ext", "urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2");
            namespaces.Add("sts", "dian:gov:co:facturaelectronica:Structures-2-1");
            namespaces.Add("xades", "http://uri.etsi.org/01903/v1.3.2#");
            namespaces.Add("xades141", "http://uri.etsi.org/01903/v1.4.1#");
            namespaces.Add("xsi", "http://www.w3.org/2001/XMLSchema-instance");

            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.OmitXmlDeclaration = false; // O ajustar según sea necesario

            using (StringWriter textWriter = new StringWriter())
            using (XmlWriter xmlWriter = XmlWriter.Create(textWriter, settings))
            {
                serializer.Serialize(xmlWriter, MyInvoice, namespaces);

                // La conversión a string ya incluye el xmlns correctamente por la definición en la clase
                return textWriter.ToString();
            }
        }







        public void SaveXMLToFile(string xmlContent, string filePath)
            {
                File.WriteAllText(filePath, xmlContent);
            }
        }
}
