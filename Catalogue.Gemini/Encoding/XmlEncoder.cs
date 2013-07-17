using System;
using System.Xml.Linq;
using Catalogue.Gemini.Model;

namespace Catalogue.Gemini.Encoding
{
    public interface IXmlEncoder
    {
        XDocument Encode(Metadata metadata, Guid fileIdentifier);
        Metadata Decode(XDocument document);
    }

    public class XmlEncoder : IXmlEncoder
    {
        static readonly XNamespace gmd = "http://www.isotc211.org/2005/gmd";
        static readonly XNamespace gco = "http://www.isotc211.org/2005/gco";
        static readonly XNamespace srv = "http://www.isotc211.org/2005/srv";
        static readonly XNamespace gml = "http://www.opengis.net/gml/3.2";
        static readonly XNamespace xlink = "http://www.w3.org/1999/xlink";

        public XDocument Encode(Metadata m, Guid fileIdentifier)
        {
            // todo: don't delete information that we don't support on first edit!
            // (that's more complicated than just creating a new document!)

            // tip: don't mess up the indentation!
            return new XDocument(gmd + "MD_Metadata",
                MakeFileIdentifier(fileIdentifier),
                MakeMetadataLanguage(m),
                MakeResourceType(m),
                MakeMetadataPointOfContact(m),
                MakeMetadataDate(m),
                new XElement(gmd + "identificationInfo",
                    new XElement(gmd + "MD_DataIdentification",
                        new XElement(gmd + "citation",
                            new XElement(gmd + "CI_Citation",
                                MakeTitle(m),
                                MakeDatasetReferenceDate(m),
                                MakeUniqueResourceIdentifier(m))),
                        MakeAbstract(m),
                        MakeResponsibleOrganisation(m),
                        MakeFrequencyOfUpdate(m),
                        MakeKeywords(m),
                        new XElement(gmd + "resourceContraints",
                            new XElement(gmd + "MD_LegalConstraints",
                                MakeLimitationsOnPublicAccess(m)),
                            new XElement(gmd + "MD_Constraints",
                                MakeUseConstraints(m))),
                        MakeSpatialResolution(m),
                        MakeDatasetLanguage(m),
                        new XElement(gmd + "extent",
                            MakeBoundingBox(m),
                            MakeTemporalExtent(m)))),
                new XElement(gmd + "distributionInfo",
                    new XElement(gmd + "MD_Distribution",
                        MakeDataFormat(m))),
                MakeLineage(m));


        }

        XElement MakeFileIdentifier(Guid fileIdentifier)
        {
            return new XElement(gmd + "fileIdentifier", new XElement(gco + "CharacterString", fileIdentifier.ToString()));
        }


        XElement MakeMetadataLanguage(Metadata metadata)
        {
            return new XElement(gmd + "language",
                new XElement(gmd + "LanguageCode",
                    new XAttribute("codeList", "http://www.loc.gov/standards/iso639-2/php/code_list.php"),
                    new XAttribute("codeListValue", metadata.MetadataLanguage),
                    metadata.MetadataLanguage));
        }

        XElement MakeResourceType(Metadata metadata)
        {
            return new XElement(gmd + "hierarchyLevel",
                new XElement(gmd + "MD_ScopeCode",
                    new XAttribute("codeList", "http://standards.iso.org/ittf/PubliclyAvailableStandards/ISO_19139_Schemas/resources/codelist/gmxCodelists.xml#MD_ScopeCode"),
                    new XAttribute("codeListValue", metadata.ResourceType),
                        metadata.ResourceType));
        }

        XElement MakeTitle(Metadata metadata)
        {
            return new XElement(gmd + "title", new XElement(gco + "CharacterString", metadata.Title));
        }

        XElement MakeMetadataPointOfContact(Metadata metadata)
        {
            return new XElement(gmd + "contact", Shared.MakeResponsibleParty(metadata.MetadataPointOfContact));
        }

        XElement MakeMetadataDate(Metadata metadata)
        {
            return new XElement(gmd + "dateStamp", new XElement(gco + "Date", metadata.MetadataDate));
        }

        XElement MakeDatasetReferenceDate(Metadata metadata)
        {
            return new XElement(gmd + "date",
                new XElement(gmd + "CI_Date",
                    new XElement(gmd + "date",
                        new XElement(gco + "Date", metadata.DatasetReferenceDate)),
                    new XElement(gmd + "dateType",
                        new XElement(gmd + "CI_DateTypeCode",
                            new XAttribute("codeList", "http://standards.iso.org/ittf/PubliclyAvailableStandards/ISO_19139_Schemas/resources/codelist/gmxCodelists.xml#CI_DateTypeCode"),
                            new XAttribute("codeListValue", "publication"),
                            metadata.DatasetReferenceDate))));
        }

        XElement MakeUniqueResourceIdentifier(Metadata metadata)
        {
//            string codespace = new Uri(metadata.UniqueResourceIdentifier).GetLeftPart(UriPartial.Authority);
//            string code = metadata.UniqueResourceIdentifier.Replace(codespace, String.Empty).Trim('/');

            return new XElement(gmd + "identifier",
                new XElement(gmd + "MD_Identifier",
                    new XElement(gmd + "code",
                        new XElement(gco + "CharacterString", metadata.UniqueResourceIdentifier))));
        }

        XElement MakeAbstract(Metadata metadata)
        {
            return new XElement(gmd + "abstract", new XElement(gco + "CharacterString", metadata.Abstract));
        }

        XElement MakeResponsibleOrganisation(Metadata metadata)
        {
            return new XElement(gmd + "abstract", new XElement(gco + "CharacterString", metadata.Abstract));
        }





        XElement MakeDatasetLanguage(Metadata metadata)
        {
            // this is required unfortunately by ISO19115 but not Gemini - default to metadata language  
            throw new NotImplementedException();
        }

        static class Shared
        {
            public static XElement MakeResponsibleParty(ResponsibleParty c)
            {
                return new XElement(gmd + "CI_ResponsibleParty",
                    new XElement(gmd + "organisationName",
                        new XElement(gco + "CharacterString", c.Name)),
                    new XElement(gmd + "contactInfo",
                        new XElement(gmd + "CI_Contact",
                            new XElement(gmd + "address",
                                new XElement(gmd + "CI_Address",
                                    new XElement(gmd + "electronicMailAddress",
                                        new XElement(gmd + "CharacterString", c.Email)))))),
                    new XElement(gmd + "role",
                        new XElement(gmd + "CI_RoleCode",
                            new XAttribute("codeList", "http://standards.iso.org/ittf/PubliclyAvailableStandards/ISO_19139_Schemas/resources/codelist/gmxCodelists.xml#CI_RoleCode"),
                            new XAttribute("codeListValue", c.Role),
                            c.Role)));
            }
        }



        public Metadata Decode(XDocument document)
        {
            return new Metadata();
        }
    }
}
