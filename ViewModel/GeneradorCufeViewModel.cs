using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using static GeneradorCufe.Model.GeneradorCufeModel;

namespace GeneradorCufe.ViewModel
{

    public class InvoiceViewModel : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private string _numeroFactura;
        public string NumeroFactura
        {
            get { return _numeroFactura; }
            set { _numeroFactura = value; OnPropertyChanged(); }
        }

        private DateTime _fechaFactura;
        public DateTime FechaFactura
        {
            get { return _fechaFactura; }
            set { _fechaFactura = value; OnPropertyChanged(); }
        }

        // Nuevas propiedades
        private string _valorSubtotal;
        public string ValorSubtotal
        {
            get { return _valorSubtotal; }
            set { _valorSubtotal = value; OnPropertyChanged(); }
        }

        private string _horaGeneracion;
        public string HoraGeneracion
        {
            get { return _horaGeneracion; }
            set { _horaGeneracion = value; OnPropertyChanged(); }
        }

        private string _valorIVA;
        public string ValorIVA
        {
            get { return _valorIVA; }
            set { _valorIVA = value; OnPropertyChanged(); }
        }

        private string _valorImpuesto2;
        public string ValorImpuesto2
        {
            get { return _valorImpuesto2; }
            set { _valorImpuesto2 = value; OnPropertyChanged(); }
        }

        private string _valorImpuesto3;
        public string ValorImpuesto3
        {
            get { return _valorImpuesto3; }
            set { _valorImpuesto3 = value; OnPropertyChanged(); }
        }

        private string _totalPagar;
        public string TotalPagar
        {
            get { return _totalPagar; }
            set { _totalPagar = value; OnPropertyChanged(); }
        }

        private string _nitFacturadorElectronico;
        public string NITFacturadorElectronico
        {
            get { return _nitFacturadorElectronico; }
            set { _nitFacturadorElectronico = value; OnPropertyChanged(); }
        }

        private string _numeroIdentificacionCliente;
        public string NumeroIdentificacionCliente
        {
            get { return _numeroIdentificacionCliente; }
            set { _numeroIdentificacionCliente = value; OnPropertyChanged(); }
        }

        private string _claveTecnicaControl;
        public string ClaveTecnicaControl
        {
            get { return _claveTecnicaControl; }
            set { _claveTecnicaControl = value; OnPropertyChanged(); }
        }

        private string _cadenaCUFE;
        public string CadenaCUFE
        {
            get { return _cadenaCUFE; }
            set { _cadenaCUFE = value; OnPropertyChanged(nameof(CadenaCUFE)); }
        }

        private string _cufe;
        public string CUFE
        {
            get => _cufe;
            set
            {
                if (_cufe != value)
                {
                    _cufe = value;
                    OnPropertyChanged(nameof(CUFE));
                    if (MyInvoice != null && MyInvoice.UUID != null)
                    {
                        MyInvoice.UUID.Text = _cufe; // Actualizar solo el Text.
                    }
                }
            }
        }

        private string _setTestId;
        public string SetTestId
        {
            get => _setTestId;
            set
            {
                if (_setTestId != value)
                {
                    _setTestId = value;
                    OnPropertyChanged(nameof(SetTestId));
                    if (MyInvoice != null && MyInvoice.DATA != null) 
                    {
                        if (MyInvoice.DATA.Partnership == null)
                            MyInvoice.DATA.Partnership = new Partnership(); // Inicializa Partnership si es necesario

                        MyInvoice.DATA.Partnership.SetTestID = _setTestId; 
                    }
                }
            }
        }



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
            authorizationPeriod.SetDates(new DateTime(2019, 01, 19), new DateTime(2030, 01, 19));

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
                    SchemeName = "CUFE-SHA384"
                    // Text  se encuentra en la propiedad CUFE donde se almacena el valor osea arriba
                },
                InvoiceTypeCode = "01",
                Note = "Prueba Factura Electronica Datos Reales 2",
                DocumentCurrencyCode = "COP",
                LineCountNumeric = 1,
                // Inicializar AccountingSupplierParty y otras propiedades necesarias aquí
            };
            DateTime issueDate = new DateTime(2024, 02, 12);
            DateTime issueTime = DateTime.ParseExact("00:00:00-05:00", "HH:mm:ssK", CultureInfo.InvariantCulture);

            MyInvoice.SetIssueDateAndTime(issueDate, issueTime);

            MyInvoice.AccountingSupplierParty = new AccountingSupplierParty
            {
                AdditionalAccountID = 1,
                Party = new Party
                {
                    PartyName = new PartyName { Name = "RM SOFT CASA DE SOFTWARE S.A.S" },
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
                                Name = new CbcName { Text = "Colombia", LanguageID = "es" }
                            }
                        }
                    },
                    PartyTaxScheme = new PartyTaxScheme
                    {
                        RegistrationName = "RM SOFT CASA DE SOFTWARE S.A.S",
                        CompanyID = new CompanyID
                        {
                            Text = 900770401,
                            SchemeID = 8,
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
                                Name = new CbcName { Text = "Colombia", LanguageID = "es" }
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
                        RegistrationName = "RM SOFT CASA DE SOFTWARE S.A.S",
                        CompanyID = new CompanyID
                        {
                            Text = 900770401,
                            SchemeID = 8,
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
                    ID = new ID
                    {
                        SchemeName = "13",
                        Text = "1017173008",
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
                                Name = new CbcName { Text = "Colombia", LanguageID = "es" } // esta bueno
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
                                Name = new CbcName { Text = "Colombia", LanguageID = "es" }
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
                    Description = "Frambuesas",
                    StandardItemIdentification = new StandardItemIdentification
                    {
                        ID = "900770401-8",
                        SchemeID = "999"
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
                    ID = 900770401,
                    TechKey = "fc8eac422eba16e22ffd8c6f94b3f40a6e38162c",
                    SetTestID = this.SetTestId  // Rm

                }
            };
        }

        public string GenerateXML(Invoice MyInvoice)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Invoice));

            // Crear los espacios de nombres
            XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
            namespaces.Add("ds", "http://www.w3.org/2000/09/xmldsig#");
            MyInvoice.Xmlns = "urn:oasis:names:specification:ubl:schema:xsd:Invoice-2";
            namespaces.Add("cac", "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2");
            namespaces.Add("cbc", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");
            namespaces.Add("ext", "urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2");
            namespaces.Add("sts", "dian:gov:co:facturaelectronica:Structures-2-1");
            namespaces.Add("xades", "http://uri.etsi.org/01903/v1.3.2#");
            namespaces.Add("xades141", "http://uri.etsi.org/01903/v1.4.1#");
            namespaces.Add("xsi", "http://www.w3.org/2001/XMLSchema-instance");

            // Establecer el atributo schemaLocation en el elemento raíz

            MyInvoice.SchemaLocation = "urn:oasis:names:specification:ubl:schema:xsd:Invoice-2 http://docs.oasis-open.org/ubl/os-UBL-2.1/xsd/maindoc/UBL-Invoice-2.1.xsd";

            // Configurar el XmlWriter para la serialización
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.OmitXmlDeclaration = false;
            string xmlResult;
            using (StringWriter textWriter = new StringWriter())
            using (XmlWriter xmlWriter = XmlWriter.Create(textWriter, settings))
            {
                // Serializar el objeto Invoice
                serializer.Serialize(xmlWriter, MyInvoice, namespaces);
                xmlResult = textWriter.ToString();
            }

            // Ajustar el XML generado para corregir la estructura
            xmlResult = AdjustXML(xmlResult);

            // Sobrescribir la salida XML para ajustes adicionales si son necesarios
            xmlResult = xmlResult.Replace("<UBLExtensions>", "<ext:UBLExtensions>");
            xmlResult = xmlResult.Replace("</UBLExtensions>", "</ext:UBLExtensions>");
            xmlResult = xmlResult.Replace("schemaLocation=", "xsi:schemaLocation=");

            return xmlResult;
        }

        public string AdjustXML(string xmlResult)
        {
            // Cargar el XML resultante en un XDocument
            XDocument doc = XDocument.Parse(xmlResult);

            // Encontrar todos los elementos que necesitan ser ajustados
            var items = doc.Descendants(XName.Get("StandardItemIdentification", "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2"));

            foreach (var item in items)
            {
                // Verifica si el elemento ID existe y tiene un atributo schemeID
                var idElement = item.Element(XName.Get("ID", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2"));
                if (idElement != null && item.Attribute("schemeID") != null)
                {
                    // Mover el atributo schemeID al elemento ID
                    var schemeID = item.Attribute("schemeID").Value;
                    idElement.SetAttributeValue("schemeID", schemeID);

                    // Remover el atributo schemeID del elemento StandardItemIdentification
                    item.Attribute("schemeID").Remove();
                }
            }

            var partyIdentifications = doc.Descendants(XName.Get("PartyIdentification", "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2"));
            foreach (var partyIdentification in partyIdentifications)
            {
                var idElement = partyIdentification.Element(XName.Get("ID", "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2"));
                if (idElement != null)
                {
                    // Crear un nuevo elemento ID con el espacio de nombres correcto (cbc) y los atributos necesarios
                    XElement newIdElement = new XElement(XName.Get("ID", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2"),
                        new XAttribute(XName.Get("schemeName"), idElement.Attribute("schemeName")?.Value),
                        idElement.Value);

                    // Reemplazar el elemento ID antiguo con el nuevo
                    idElement.ReplaceWith(newIdElement);
                }
            }

            // Encontrar todos los elementos <cbc:CbcName> y reemplazarlos con <cbc:Name>
            var cbcNameElements = doc.Descendants(XName.Get("CbcName", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2"));
            foreach (var cbcNameElement in cbcNameElements.ToList()) // ToList para evitar modificaciones durante la iteración
            {
                // Crear un nuevo elemento <cbc:Name> con los mismos atributos y valor que <cbc:CbcName>
                XElement newNameElement = new XElement(XName.Get("Name", "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2"),
                    cbcNameElement.Attributes(),
                    cbcNameElement.Value);

                // Reemplazar el elemento <cbc:CbcName> con el nuevo <cbc:Name>
                cbcNameElement.ReplaceWith(newNameElement);
            }

            // Convertir el XDocument modificado de nuevo a una cadena de texto XML
            using (var writer = new StringWriter())
            {
                doc.Save(writer);
                return writer.ToString();
            }
        }



        public (string xmlContent, string base64Content) GenerateXMLAndBase64(Invoice MyInvoice)
        {
            // Generar el XML como lo estás haciendo actualmente
            string xmlContent = GenerateXML(MyInvoice);

            // Convertir el XML en un arreglo de bytes
            byte[] bytes = Encoding.UTF8.GetBytes(xmlContent);

            // Codificar el arreglo de bytes en base64
            string base64Encoded = Convert.ToBase64String(bytes);

            return (xmlContent, base64Encoded);
        }


        public void SaveXMLToFile(string xmlContent, string filePath)
        {
            File.WriteAllText(filePath, xmlContent);
        }
    }

    public static class DataSerializer
    {
        private const string FilePath = "data.txt";

        public static void SaveData(string data)
        {
            // Escribe los datos en el archivo de texto
            File.WriteAllText(FilePath, data);
        }

        public static string LoadData()
        {
            if (!File.Exists(FilePath))
            {
                // Si el archivo no existe, devuelve una cadena vacía
                return "";
            }

            // Lee los datos desde el archivo de texto
            return File.ReadAllText(FilePath);
        }
    }


}
