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
        readonly XNamespace gmd = "http://www.isotc211.org/2005/gmd";
        readonly XNamespace gco = "http://www.isotc211.org/2005/gco";
        readonly XNamespace srv = "http://www.isotc211.org/2005/srv";
        readonly XNamespace gml = "http://www.opengis.net/gml/3.2";
        readonly XNamespace xlink = "http://www.w3.org/1999/xlink";

        public XDocument Encode(Metadata m, Guid fileIdentifier)
        {
            // todo: don't delete information that we don't support on first edit!
            // (that's more complicated than just creating a new document!)

            // tip: don't mess up the indentation!
            return new XDocument(gmd + "MD_Metadata",
                GetFileIdentifier(fileIdentifier),
                GetMetadataLanguage(m),
                GetResourceType(m),
                GetMetadataContact(m),
                GetMetadataData(m),
                GetSpatialReferenceSystem(m),
                new XElement(gmd + "identificationInfo",
                    new XElement(gmd + "MD_DataIdentification",
                        new XElement(gmd + "CI_Citation",
                            GetTitle(m),
                            GetDatasetReferenceDate(m),
                            GetUniqueResourceIdentifier(m)),
                        GetAbstract(m),
                        GetResponsibleOrganisation(m),
                        GetFrequencyOfUpdate(m),
                        GetKeywords(m),
                        new XElement(gmd + "resourceContraints",
                            new XElement(gmd + "MD_LegalConstraints",
                                GetLimitationsOnPublicAccess(m)),
                            new XElement(gmd + "MD_Constraints",
                                GetUseConstraints(m))),
                        GetSpatialResolution(m),
                        GetDatasetLanguage(m),
                        new XElement(gmd + "extent",
                            GetBoundingBox(m),
                            GetTemporalExtent(m)))),
                new XElement(gmd + "distributionInfo",
                    new XElement(gmd + "MD_Distribution",
                        GetDataFormat(m))),
                GetLineage(m));


        }

        XElement GetFileIdentifier(Guid fileIdentifier)
        {
            return new XElement(gmd + "fileIdentifier", new XElement(gco + "CharacterString", fileIdentifier.ToString()));
        }


        XElement GetMetadataLanguage(Metadata metadata)
        {
            return new XElement(gmd + "language",
                new XElement(gmd + "LanguageCode",
                    new XAttribute("codeList", "http://www.loc.gov/standards/iso639-2/php/code_list.php"),
                    new XAttribute("codeListValue", metadata.MetadataLanguage),
                    metadata.MetadataLanguage));
        }

        XElement GetResourceType(Metadata metadata)
        {
            return new XElement(gmd + "hierarchyLevel",
                new XElement(gmd + "MD_ScopeCode",
                    new XAttribute("codeList", "http://standards.iso.org/ittf/PubliclyAvailableStandards/ISO_19139_Schemas/resources/codelist/gmxCodelists.xml#MD_ScopeCode"),
                    new XAttribute("codeListValue", metadata.ResourceType),
                        metadata.ResourceType));
        }

        XElement GetTitle(Metadata metadata)
        {
            return new XElement(gmd + "title", new XElement(gco + "CharacterString", metadata.Title));
        }

        XElement GetDatasetLanguage(Metadata metadata)
        {
            // this is required unfortunately by ISO 19115 but not Gemini - default to metadata language  
            throw new NotImplementedException();
        }





        public Metadata Decode(XDocument document)
        {
            return new Metadata();
        }
    }
}
