using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace GeneradorCufe.Model
{
    public class GeneradorCufeModel
    {
        // Definir la constante para el espacio de nombres 'cbc'
        private const string CbcNamespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2";
        private const string CacNamespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2";

        [XmlRoot(ElementName = "AuthorizationPeriod")]
        public class AuthorizationPeriod
        {
            [XmlElement(ElementName = "StartDate", Namespace = CbcNamespace)]
            public string StartDate { get; set; }

            [XmlElement(ElementName = "EndDate", Namespace = CbcNamespace)]
            public string EndDate { get; set; }

            public void SetDates(DateTime startDate, DateTime endDate)
            {
                StartDate = startDate.ToString("yyyy-MM-dd");
                EndDate = endDate.ToString("yyyy-MM-dd");
            }
        }

        [XmlRoot(ElementName = "AuthorizedInvoices")]
        public class AuthorizedInvoices
        {

            [XmlElement(ElementName = "Prefix")]
            public string Prefix { get; set; }

            [XmlElement(ElementName = "From")]
            public int From { get; set; }

            [XmlElement(ElementName = "To")]
            public int To { get; set; }
        }

        [XmlRoot(ElementName = "InvoiceControl")]
        public class InvoiceControl
        {

            [XmlElement(ElementName = "InvoiceAuthorization")]
            public double InvoiceAuthorization { get; set; }

            [XmlElement(ElementName = "AuthorizationPeriod")]
            public AuthorizationPeriod AuthorizationPeriod { get; set; }

            [XmlElement(ElementName = "AuthorizedInvoices")]
            public AuthorizedInvoices AuthorizedInvoices { get; set; }
        }




        [XmlRoot(ElementName = "DianExtensions")]
        // Clase DianExtensions
        public class DianExtensions
        {
            [XmlElement(ElementName = "InvoiceControl")]
            public InvoiceControl InvoiceControl { get; set; }
        }

        // Clase UBLExtension
        [XmlRoot(ElementName = "UBLExtension", Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2")]
        public class UBLExtension
        {
            [XmlElement(ElementName = "ExtensionContent")]
            public ExtensionContent ExtensionContent { get; set; }
        }


        [XmlRoot(ElementName = "UBLExtensions", Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2")]
        public class UBLExtensions
        {
            [XmlElement(ElementName = "UBLExtension", Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonExtensionComponents-2")]
            public UBLExtension UBLExtension { get; set; }
        }


        // Clase ExtensionContent
        public class ExtensionContent
        {
            [XmlElement(ElementName = "DianExtensions", Namespace = "dian:gov:co:facturaelectronica:Structures-2-1")]
            public DianExtensions DianExtensions { get; set; }
        }




        [XmlRoot(ElementName = "UUID")]
        public class UUID
        {

            [XmlAttribute(AttributeName = "schemeID")]
            public int SchemeID { get; set; }

            [XmlAttribute(AttributeName = "schemeName")]
            public string SchemeName { get; set; }

            [XmlText]
            public string Text { get; set; }
        }

        [XmlRoot(ElementName = "PartyName")]
        public class PartyName
        {

            [XmlElement(ElementName = "Name", Namespace = CbcNamespace)]
            public string Name { get; set; }
        }

        [XmlRoot(ElementName = "AddressLine", Namespace = CacNamespace)]
        public class AddressLine
        {

            [XmlElement(ElementName = "Line", Namespace = CbcNamespace)]
            public string Line { get; set; }

        }
        [XmlRoot(ElementName = "Country", Namespace = CacNamespace)]
        public class Country
        {
            [XmlElement(ElementName = "IdentificationCode", Namespace = CbcNamespace)]
            public string IdentificationCode { get; set; }

            [XmlElement(ElementName = "CbcName", Namespace = CbcNamespace)] // Cambiado a "CbcName"
            public CbcName Name { get; set; }
        }


        public class CbcName
        {
            [XmlAttribute(AttributeName = "languageID")]
            public string LanguageID { get; set; }

            [XmlText]
            public string Text { get; set; }
        }

        [XmlRoot(ElementName = "Address")]
        public class Address
        {

            [XmlElement(ElementName = "ID", Namespace = CbcNamespace)]
            public string ID { get; set; }

            [XmlElement(ElementName = "CityName", Namespace = CbcNamespace)]
            public string CityName { get; set; }

            [XmlElement(ElementName = "PostalZone", Namespace = CbcNamespace)]
            public string PostalZone { get; set; }

            [XmlElement(ElementName = "CountrySubentity", Namespace = CbcNamespace)]
            public string CountrySubentity { get; set; }

            [XmlElement(ElementName = "CountrySubentityCode", Namespace = CbcNamespace)]
            public string CountrySubentityCode { get; set; }

            [XmlElement(ElementName = "AddressLine")]
            public AddressLine AddressLine { get; set; }

            [XmlElement(ElementName = "Country")]
            public Country Country { get; set; }
        }

        [XmlRoot(ElementName = "PhysicalLocation", Namespace = CacNamespace)]
        public class PhysicalLocation
        {

            [XmlElement(ElementName = "Address")]
            public Address Address { get; set; }
        }

        [XmlRoot(ElementName = "CompanyID")]
        public class CompanyID
        {

            [XmlAttribute(AttributeName = "schemeID")]
            public int SchemeID { get; set; }

            [XmlAttribute(AttributeName = "schemeName")]
            public int SchemeName { get; set; }

            [XmlAttribute(AttributeName = "schemeAgencyID")]
            public int SchemeAgencyID { get; set; }

            [XmlAttribute(AttributeName = "schemeAgencyName")]
            public string SchemeAgencyName { get; set; }

            [XmlText]
            public int Text { get; set; }
        }

        [XmlRoot(ElementName = "RegistrationAddress")]
        public class RegistrationAddress
        {

            [XmlElement(ElementName = "ID", Namespace = CbcNamespace)]
            public string ID { get; set; }

            [XmlElement(ElementName = "CityName", Namespace = CbcNamespace)]
            public string CityName { get; set; }

            [XmlElement(ElementName = "PostalZone", Namespace = CbcNamespace)]
            public string PostalZone { get; set; }

            [XmlElement(ElementName = "CountrySubentity", Namespace = CbcNamespace)]
            public string CountrySubentity { get; set; }

            [XmlElement(ElementName = "CountrySubentityCode", Namespace = CbcNamespace)]
            public string CountrySubentityCode { get; set; }

            [XmlElement(ElementName = "AddressLine")]
            public AddressLine AddressLine { get; set; }

            [XmlElement(ElementName = "Country")]
            public Country Country { get; set; }
        }

        [XmlRoot(ElementName = "TaxScheme")]
        public class TaxScheme
        {
            [XmlElement(ElementName = "ID", Namespace = CbcNamespace)]
            public string ID { get; set; }

            [XmlElement(ElementName = "Name", Namespace = CbcNamespace)]
            public string Name { get; set; }
        }


        [XmlRoot(ElementName = "PartyTaxScheme")]
        public class PartyTaxScheme
        {

            [XmlElement(ElementName = "RegistrationName", Namespace = CbcNamespace)]
            public string RegistrationName { get; set; }

            [XmlElement(ElementName = "CompanyID", Namespace = CbcNamespace)]
            public CompanyID CompanyID { get; set; }

            [XmlElement(ElementName = "TaxLevelCode", Namespace = CbcNamespace)]
            public string TaxLevelCode { get; set; }

            [XmlElement(ElementName = "RegistrationAddress")]
            public RegistrationAddress RegistrationAddress { get; set; }

            [XmlElement(ElementName = "TaxScheme")]
            public TaxScheme TaxScheme { get; set; }
        }

        [XmlRoot(ElementName = "CorporateRegistrationScheme")]
        public class CorporateRegistrationScheme
        {

            [XmlElement(ElementName = "ID", Namespace = CbcNamespace)]
            public string ID { get; set; }

            [XmlElement(ElementName = "Name", Namespace = CbcNamespace)]
            public string Name { get; set; }
        }

        [XmlRoot(ElementName = "PartyLegalEntity")]
        public class PartyLegalEntity
        {

            [XmlElement(ElementName = "RegistrationName", Namespace = CbcNamespace)]
            public string RegistrationName { get; set; }

            [XmlElement(ElementName = "CompanyID", Namespace = CbcNamespace)]
            public CompanyID CompanyID { get; set; }

            [XmlElement(ElementName = "CorporateRegistrationScheme")]
            public CorporateRegistrationScheme CorporateRegistrationScheme { get; set; }
        }

        [XmlRoot(ElementName = "Contact")]
        public class Contact
        {

            [XmlElement(ElementName = "ElectronicMail", Namespace = CbcNamespace)]
            public string ElectronicMail { get; set; }
        }

        [XmlRoot(ElementName = "Party")]
        public class Party
        {

            [XmlElement(ElementName = "PartyName", Namespace = CacNamespace)]
            public PartyName PartyName { get; set; }

            [XmlElement(ElementName = "PhysicalLocation")]
            public PhysicalLocation PhysicalLocation { get; set; }

            [XmlElement(ElementName = "PartyTaxScheme")]
            public PartyTaxScheme PartyTaxScheme { get; set; }

            [XmlElement(ElementName = "PartyLegalEntity")]
            public PartyLegalEntity PartyLegalEntity { get; set; }

            [XmlElement(ElementName = "Contact", Namespace = CacNamespace)]
            public Contact Contact { get; set; }
        }

        [XmlRoot(ElementName = "AccountingSupplierParty")]
        public class AccountingSupplierParty
        {

            [XmlElement(ElementName = "AdditionalAccountID", Namespace = CbcNamespace)]
            public int AdditionalAccountID { get; set; }

            [XmlElement(ElementName = "Party", Namespace = CacNamespace)]
            public Party Party { get; set; }
        }

        [XmlRoot(ElementName = "ID")]
        public class ID
        {
            [XmlText]
            public string Text { get; set; }

            [XmlAttribute(AttributeName = "schemeID")]
            public string SchemeID { get; set; }

            [XmlAttribute(AttributeName = "schemeName")]
            public string SchemeName { get; set; }
        }


        [XmlRoot(ElementName = "PartyIdentification")]
        public class PartyIdentification
        {

            [XmlElement(ElementName = "ID")]
            public ID ID { get; set; }
        }

        [XmlRoot(ElementName = "AccountingCustomerParty")]
        public class AccountingCustomerParty
        {

            [XmlElement(ElementName = "AdditionalAccountID", Namespace = CbcNamespace)]
            public int AdditionalAccountID { get; set; }

            [XmlElement(ElementName = "PartyIdentification", Namespace = CacNamespace)]
            public PartyIdentification PartyIdentification { get; set; }

            [XmlElement(ElementName = "Party")]
            public Party Party { get; set; }
        }

        [XmlRoot(ElementName = "PaymentMeans")]
        public class PaymentMeans
        {

            [XmlElement(ElementName = "ID", Namespace = CbcNamespace)]
            public string ID { get; set; }

            [XmlElement(ElementName = "PaymentMeansCode", Namespace = CbcNamespace)]
            public int PaymentMeansCode { get; set; }

            [XmlElement(ElementName = "PaymentID", Namespace = CbcNamespace)]
            public string PaymentID { get; set; }
        }

        [XmlRoot(ElementName = "TaxAmount")]
        public class TaxAmount
        {

            [XmlAttribute(AttributeName = "currencyID")]
            public string CurrencyID { get; set; }

            [XmlText]
            public string Text { get; set; }
        }

        [XmlRoot(ElementName = "TaxableAmount")]
        public class TaxableAmount
        {

            [XmlAttribute(AttributeName = "currencyID")]
            public string CurrencyID { get; set; }

            [XmlText]
            public string Text { get; set; }
        }

        [XmlRoot(ElementName = "TaxCategory")]
        public class TaxCategory
        {

            [XmlElement(ElementName = "Percent", Namespace = CbcNamespace)]
            public string Percent { get; set; }

            [XmlElement(ElementName = "TaxScheme")]
            public TaxScheme TaxScheme { get; set; }
        }

        [XmlRoot(ElementName = "TaxSubtotal")]
        public class TaxSubtotal
        {

            [XmlElement(ElementName = "TaxableAmount", Namespace = CbcNamespace)]
            public TaxableAmount TaxableAmount { get; set; }

            [XmlElement(ElementName = "TaxAmount", Namespace = CbcNamespace)]
            public TaxAmount TaxAmount { get; set; }

            [XmlElement(ElementName = "TaxCategory")]
            public TaxCategory TaxCategory { get; set; }
        }

        [XmlRoot(ElementName = "TaxTotal", Namespace = CacNamespace)]
        public class TaxTotal
        {

            [XmlElement(ElementName = "TaxAmount", Namespace = CbcNamespace)]
            public TaxAmount TaxAmount { get; set; }

            [XmlElement(ElementName = "TaxSubtotal", Namespace = CacNamespace)]
            public TaxSubtotal TaxSubtotal { get; set; }
        }

        [XmlRoot(ElementName = "LineExtensionAmount")]
        public class LineExtensionAmount
        {

            [XmlAttribute(AttributeName = "currencyID")]
            public string CurrencyID { get; set; }

            [XmlText]
            public string Text { get; set; }
        }

        [XmlRoot(ElementName = "TaxExclusiveAmount")]
        public class TaxExclusiveAmount
        {

            [XmlAttribute(AttributeName = "currencyID")]
            public string CurrencyID { get; set; }

            [XmlText]
            public string Text { get; set; }
        }

        [XmlRoot(ElementName = "TaxInclusiveAmount")]
        public class TaxInclusiveAmount
        {

            [XmlAttribute(AttributeName = "currencyID")]
            public string CurrencyID { get; set; }

            [XmlText]
            public string Text { get; set; }
        }

        [XmlRoot(ElementName = "PayableAmount")]
        public class PayableAmount
        {

            [XmlAttribute(AttributeName = "currencyID")]
            public string CurrencyID { get; set; }

            [XmlText]
            public string Text { get; set; }
        }

        [XmlRoot(ElementName = "LegalMonetaryTotal")]
        public class LegalMonetaryTotal
        {

            [XmlElement(ElementName = "LineExtensionAmount", Namespace = CbcNamespace)]
            public LineExtensionAmount LineExtensionAmount { get; set; }

            [XmlElement(ElementName = "TaxExclusiveAmount", Namespace = CbcNamespace)]
            public TaxExclusiveAmount TaxExclusiveAmount { get; set; }

            [XmlElement(ElementName = "TaxInclusiveAmount", Namespace = CbcNamespace)]
            public TaxInclusiveAmount TaxInclusiveAmount { get; set; }

            [XmlElement(ElementName = "PayableAmount", Namespace = CbcNamespace)]
            public PayableAmount PayableAmount { get; set; }
        }

        [XmlRoot(ElementName = "Item", Namespace = CacNamespace)]
        public class Item
        {
            [XmlElement(ElementName = "Description", Namespace = CbcNamespace)]
            public string Description { get; set; }

            [XmlElement(ElementName = "StandardItemIdentification", Namespace = CacNamespace)]
            public StandardItemIdentification StandardItemIdentification { get; set; }
        }

        public class StandardItemIdentification
        {
            // Cambia de usar un objeto ID a un string directamente para el ID.
            [XmlElement(ElementName = "ID", Namespace = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2")]
            public string ID { get; set; }

            // Mueve el SchemeID como un atributo de este objeto.
            [XmlAttribute(AttributeName = "schemeID")]
            public string SchemeID { get; set; }
        }






        [XmlRoot(ElementName = "PriceAmount", Namespace = CbcNamespace)]
        public class PriceAmount
        {

            [XmlAttribute(AttributeName = "currencyID")]
            public string CurrencyID { get; set; }

            [XmlText]
            public string Text { get; set; }
        }

        [XmlRoot(ElementName = "BaseQuantity", Namespace = CbcNamespace)]
        public class BaseQuantity
        {

            [XmlAttribute(AttributeName = "unitCode")]
            public string UnitCode { get; set; }

            [XmlText]
            public string Text { get; set; }
        }

        [XmlRoot(ElementName = "Price", Namespace = CacNamespace)]
        public class Price
        {

            [XmlElement(ElementName = "PriceAmount", Namespace = CbcNamespace)]
            public PriceAmount PriceAmount { get; set; }

            [XmlElement(ElementName = "BaseQuantity", Namespace = CbcNamespace)]
            public BaseQuantity BaseQuantity { get; set; }
        }

        [XmlRoot(ElementName = "InvoiceLine")]
        public class InvoiceLine
        {

            [XmlElement(ElementName = "ID", Namespace = CbcNamespace)]
            public string ID { get; set; }

            [XmlElement(ElementName = "InvoicedQuantity", Namespace = CbcNamespace)]
            public string InvoicedQuantity { get; set; }

            [XmlElement(ElementName = "LineExtensionAmount", Namespace = CbcNamespace)]
            public LineExtensionAmount LineExtensionAmount { get; set; }

            [XmlElement(ElementName = "TaxTotal")]
            public TaxTotal TaxTotal { get; set; }

            [XmlElement(ElementName = "Item")]
            public Item Item { get; set; }

            [XmlElement(ElementName = "Price", Namespace = CacNamespace)]
            public Price Price { get; set; }
        }

        [XmlRoot(ElementName = "Partnership")]
        public class Partnership
        {

            [XmlElement(ElementName = "ID")]
            public int ID { get; set; }

            [XmlElement(ElementName = "TechKey")]
            public string TechKey { get; set; }

            [XmlElement(ElementName = "SetTestID")]
            public string SetTestID { get; set; }
        }

        [XmlRoot(ElementName = "DATA")]
        public class DATA
        {

            [XmlElement(ElementName = "UBL21")]
            public bool UBL21 { get; set; }

            [XmlElement(ElementName = "Partnership")]
            public Partnership Partnership { get; set; }
        }

        [XmlRoot("Invoice", Namespace = "urn:oasis:names:specification:ubl:schema:xsd:Invoice-2", IsNullable = false)]
        public class Invoice
        {
            

            [XmlElement(ElementName = "UBLExtensions")]
            public UBLExtensions UBLExtensions { get; set; }

            [XmlElement(ElementName = "CustomizationID", Namespace = CbcNamespace)]
            public int CustomizationID { get; set; }

            [XmlElement(ElementName = "ProfileExecutionID", Namespace = CbcNamespace)]
            public int ProfileExecutionID { get; set; }

            [XmlElement(ElementName = "ID", Namespace = CbcNamespace)]
            public string ID { get; set; }

            [XmlElement(ElementName = "UUID", Namespace = CbcNamespace)]
            public UUID UUID { get; set; }

            [XmlElement(ElementName = "IssueDate", Namespace = CbcNamespace)]
            public string IssueDate { get; set; }

            [XmlElement(ElementName = "IssueTime", Namespace = CbcNamespace)]
            public string IssueTime { get; set; }

            public void SetIssueDateAndTime(DateTime date, DateTime time)
            {
                IssueDate = date.ToString("yyyy-MM-dd");
                IssueTime = time.ToString("HH:mm:sszzz");
            }
        

            [XmlElement(ElementName = "InvoiceTypeCode", Namespace = CbcNamespace)]
            public string InvoiceTypeCode { get; set; }

            [XmlElement(ElementName = "Note", Namespace = CbcNamespace)]
            public string Note { get; set; }

            [XmlElement(ElementName = "DocumentCurrencyCode", Namespace = CbcNamespace)]
            public string DocumentCurrencyCode { get; set; }

            [XmlElement(ElementName = "LineCountNumeric", Namespace = CbcNamespace)]
            public int LineCountNumeric { get; set; }

            [XmlElement(ElementName = "AccountingSupplierParty", Namespace = CacNamespace)]
            public AccountingSupplierParty AccountingSupplierParty { get; set; }

            [XmlElement(ElementName = "AccountingCustomerParty", Namespace = CacNamespace)]
            public AccountingCustomerParty AccountingCustomerParty { get; set; }

            [XmlElement(ElementName = "PaymentMeans", Namespace = CacNamespace)]
            public PaymentMeans PaymentMeans { get; set; }

            [XmlElement(ElementName = "TaxTotal", Namespace = CacNamespace)]
            public TaxTotal TaxTotal { get; set; }

            [XmlElement(ElementName = "LegalMonetaryTotal", Namespace = CacNamespace)]
            public LegalMonetaryTotal LegalMonetaryTotal { get; set; }

            [XmlElement(ElementName = "InvoiceLine", Namespace = CacNamespace)]
            public InvoiceLine InvoiceLine { get; set; }

            [XmlElement(ElementName = "DATA")]
            public DATA DATA { get; set; }

            [XmlAttribute(AttributeName = "ds")]
            public string Ds { get; set; }

            [XmlAttribute(AttributeName = "xmlns")]
            public string Xmlns { get; set; }

            [XmlAttribute(AttributeName = "cac")]
            public string Cac { get; set; }

            [XmlAttribute(AttributeName = "cbc")]
            public string Cbc { get; set; }

            [XmlAttribute(AttributeName = "ext")]
            public string Ext { get; set; }

            [XmlAttribute(AttributeName = "sts")]
            public string Sts { get; set; }

            [XmlAttribute(AttributeName = "xades")]
            public string Xades { get; set; }

            [XmlAttribute(AttributeName = "xades141")]
            public string Xades141 { get; set; }

            [XmlAttribute(AttributeName = "xsi")]
            public string Xsi { get; set; }

            [XmlAttribute(AttributeName = "schemaLocation")]
            public string SchemaLocation { get; set; }

            [XmlText]
            public string Text { get; set; }
        }
     }
}
