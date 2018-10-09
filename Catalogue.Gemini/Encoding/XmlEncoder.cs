﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Catalogue.Gemini.Model;
using Catalogue.Utilities.Text;

namespace Catalogue.Gemini.Encoding
{
    public interface IXmlEncoder
    {
        /// <summary>
        /// Creates an ISO XML metadata document using the given metadata.
        /// </summary>
        XDocument Create(string id, Metadata metadata);
    }

    public class XmlEncoder : IXmlEncoder
    {
        public static readonly XNamespace gmd = "http://www.isotc211.org/2005/gmd";
        public static readonly XNamespace gco = "http://www.isotc211.org/2005/gco";
        public static readonly XNamespace srv = "http://www.isotc211.org/2005/srv";
        public static readonly XNamespace gml = "http://www.opengis.net/gml/3.2";
        public static readonly XNamespace xlink = "http://www.w3.org/1999/xlink";

        public XDocument Create(string id, Metadata m)
        {
            // see http://data.gov.uk/sites/default/files/UK%20GEMINI%20Encoding%20Guidance%201.4.pdf
            
            return new XDocument(
                new XElement(gmd + "MD_Metadata",
                    new XAttribute(XNamespace.Xmlns + "gmd", gmd.NamespaceName),
                    new XAttribute(XNamespace.Xmlns + "gco", gco.NamespaceName),
                    new XAttribute(XNamespace.Xmlns + "gml", gml.NamespaceName),
                    MakeFileIdentifier(id),
                    MakeDatasetLanguage(m), // continually confused with MetadataLanguage
                    MakeResourceType(m),
                    MakeMetadataPointOfContact(m),
                    MakeMetadataDate(m),
                    new XElement(gmd + "identificationInfo", 
                        new XElement(gmd + "MD_DataIdentification",
                            new XAttribute("id", "_" + id),
                            new XElement(gmd + "citation",
                                new XElement(gmd + "CI_Citation",
                                    MakeTitle(m),
                                    MakeDatasetReferenceDate(m),
                                    MakeUniqueResourceIdentifier(id))),
                            MakeAbstract(m),
                            MakeResponsibleOrganisation(m),
                            MakeKeywords(m),
                            MakeLimitationsOnPublicAccessAndUseConstraints(m), // think ienum will actually work                             
                            MakeDatasetLanguage(m),
                            MakeTopicCategory(m),
                            new XElement(gmd + "extent",
                                new XElement(gmd + "EX_Extent",
                                    MakeBoundingBox(m),
                                    MakeTemporalExtent(m))),
                            MakeAdditionalInformationSource(m))),
                    new XElement(gmd + "distributionInfo",
                        new XElement(gmd + "MD_Distribution",
                            MakeDataFormat(m),
                            MakeResourceLocator(m, id))),
                    MakeLineage(m)));
        }

        #region Elements

        XElement MakeFileIdentifier(string id)
        {
            return new XElement(gmd + "fileIdentifier", new XElement(gco + "CharacterString", id));
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
            return new XElement(gmd + "title",
                new XElement(gco + "CharacterString", metadata.Title));
        }

        XElement MakeMetadataPointOfContact(Metadata metadata)
        {
            return new XElement(gmd + "contact",
                Shared.MakeResponsiblePartyNode(metadata.MetadataPointOfContact));
        }

        XElement MakeMetadataDate(Metadata metadata)
        {
            return new XElement(gmd + "dateStamp",
                new XElement(gco + "Date", metadata.MetadataDate.ToString("yyyy-MM-dd")));
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

        XElement MakeUniqueResourceIdentifier(string id)
        {
            return new XElement(gmd + "identifier",
                new XElement(gmd + "MD_Identifier",
                    new XElement(gmd + "code",
                        new XElement(gco + "CharacterString", id))));
        }

        XElement MakeAbstract(Metadata metadata)
        {
            return new XElement(gmd + "abstract",
                new XElement(gco + "CharacterString", metadata.Abstract));
        }

        XElement MakeResponsibleOrganisation(Metadata metadata)
        {
            return new XElement(gmd + "pointOfContact",
                Shared.MakeResponsiblePartyNode(metadata.ResponsibleOrganisation));
        }

        XElement MakeKeywords(Metadata metadata)
        {
            return new XElement(gmd + "descriptiveKeywords",
                new XElement(gmd + "MD_Keywords",
                    from keyword in metadata.Keywords
                    select new XElement(gmd + "keyword",
                        new XElement(gco + "CharacterString", keyword.Value))));

            // String vocab = metadata.Keywords.FirstOrDefault().Vocab;
            // a half implemented attempt to encode the vocab along with keywords
                        /*, new XElement(gmd + "thesaurusName",
                new XElement(gmd + "CI_Citation",
                    new XElement(gmd + "title",
                        new XElement(gco + "CharacterString", vocab)),
                        new XElement(gmd+"date", 
                            new XElement(gmd+"CI_Date", 
                                new XElement(gmd+"date",
                                    new XElement(gco + "Date", metadata.DatasetReferenceDate)), 
                                    new XElement(gmd+"dateType", 
                                        new XElement(gmd + "CI_DateTypeCode",
                            new XAttribute("codeList", "http://standards.iso.org/ittf/PubliclyAvailableStandards/ISO_19139_Schemas/resources/codelist/gmxCodelists.xml#CI_DateTypeCode"),
                            new XAttribute("codeListValue", "publication")))))))));*/

            // writing massiviely nested iso compatible xml documents by hand, bleurgh
        }

        XElement MakeLimitationsOnPublicAccessAndUseConstraints(Metadata metadata)
        {
            return new XElement(gmd + "resourceConstraints",
                new XElement(gmd + "MD_LegalConstraints",
                    new XElement(gmd + "useLimitation",
                        new XElement(gco + "CharacterString", metadata.UseConstraints)),
                    new XElement(gmd + "accessConstraints",
                        new XElement(gmd + "MD_RestrictionCode",
                            new XAttribute("codeList", "http://standards.iso.org/ittf/PubliclyAvailableStandards/ISO_19139_Schemas/resources/codelist/gmxCodelists.xml#MD_RestrictionCode"),
                            new XAttribute("codeListValue", "otherRestrictions"),
                            "otherRestrictions")),
                    new XElement(gmd + "otherConstraints",
                        new XElement(gco + "CharacterString", metadata.LimitationsOnPublicAccess))
                    ));
        }
        
        XElement MakeDatasetLanguage(Metadata metadata)
        {
            // this is required unfortunately by ISO19115 but not Gemini - default to eng
            
            return new XElement(gmd + "language",
                new XElement(gmd + "LanguageCode",
                    new XAttribute("codeList", "http://www.loc.gov/standards/iso639-2/php/code_list.php"),
                    new XAttribute("codeListValue", "eng"),
                    "eng"));
        }

        XElement MakeTopicCategory(Metadata metadata)
        {
            return new XElement(gmd + "topicCategory",
                new XElement(gmd + "MD_TopicCategoryCode", metadata.TopicCategory));
        }

        XElement MakeAdditionalInformationSource(Metadata metadata)
        {
            if (String.IsNullOrWhiteSpace(metadata.AdditionalInformationSource))
                return null;

            return new XElement(gmd + "supplementalInformation",
                new XElement(gco + "CharacterString", metadata.AdditionalInformationSource));
        }

        XElement MakeBoundingBox(Metadata metadata)
        {
            if (metadata.BoundingBox == null)
            {
                return null;
            }
            return new XElement(gmd + "geographicElement",
                new XElement(gmd + "EX_GeographicBoundingBox",
                    new XElement(gmd + "westBoundLongitude",
                        new XElement(gco + "Decimal", metadata.BoundingBox.West)),
                    new XElement(gmd + "eastBoundLongitude",
                        new XElement(gco + "Decimal", metadata.BoundingBox.East)),
                    new XElement(gmd + "southBoundLatitude",
                        new XElement(gco + "Decimal", metadata.BoundingBox.South)),
                    new XElement(gmd + "northBoundLatitude",
                        new XElement(gco + "Decimal", metadata.BoundingBox.North))));
        }

        XElement MakeTemporalExtent(Metadata metadata)
        {
            return new XElement(gmd + "temporalElement",
                new XElement(gmd + "EX_TemporalExtent",
                    new XElement(gmd + "extent",
                        new XElement(gml + "TimePeriod",
                            new XAttribute(gml + "id", "t1"),
                            new XElement(gml + "beginPosition", metadata.TemporalExtent.Begin),
                            new XElement(gml + "endPosition", metadata.TemporalExtent.End)))));
        }

        XElement MakeDataFormat(Metadata metadata)
        {
            return new XElement(gmd + "distributionFormat",
                new XElement(gmd + "MD_Format",
                    new XElement(gmd + "name",
                        new XElement(gco + "CharacterString", metadata.DataFormat)),
                    new XElement(gmd + "version",
                        new XElement(gco + "CharacterString", "Unknown")))); 
            // not supporting versions for data format
        }

        XElement MakeResourceLocator(Metadata metadata, string id)
        {
            string fileName = WebificationUtility.ToUrlFriendlyString(Path.GetFileName(metadata.ResourceLocator));
            // but this doesn't work so well when the resource locator contains the guid like this
            // http://data.jncc.gov.uk/data/d4ddd363-97eb-4ef6-9b8b-6f019f434103-ProcessedImages.zip
            // so remove the proceeding guid
            if (fileName.IsNotBlank() && fileName.StartsWith(id + "-"))
                fileName = fileName.Replace(id + "-", String.Empty);

            return new XElement(gmd + "transferOptions",
                new XElement(gmd + "MD_DigitalTransferOptions",
                    new XElement(gmd + "onLine",
                        new XElement(gmd + "CI_OnlineResource",
                            new XElement(gmd + "linkage",
                                new XElement(gmd + "URL", metadata.ResourceLocator)),
                            new XElement(gmd + "name", 
                                new XElement(gco + "CharacterString", fileName))))));
        }

        XElement MakeLineage(Metadata metadata)
        {
            return new XElement(gmd + "dataQualityInfo",
                new XElement(gmd + "DQ_DataQuality",
                    new XElement(gmd + "scope",
                        new XElement(gmd + "DQ_Scope",
                            new XElement(gmd + "level",
                                new XElement(gmd + "MD_ScopeCode",
                                    new XAttribute("codeList", "http://standards.iso.org/ittf/PubliclyAvailableStandards/ISO_19139_Schemas/resources/codelist/gmxCodelists.xml#MD_ScopeCode"),
                                    new XAttribute("codeListValue", "dataset"))))),
                    new XElement(gmd + "lineage",
                        new XElement(gmd + "LI_Lineage",
                            new XElement(gmd + "statement",
                                new XElement(gco + "CharacterString", metadata.Lineage))))));
        }

        static class Shared
        {
            public static XElement MakeResponsiblePartyNode(ResponsibleParty c)
            {
                if (c == null || c.Name.IsBlank())
                    return null;

                return new XElement(gmd + "CI_ResponsibleParty",
                    new XElement(gmd + "organisationName",
                        new XElement(gco + "CharacterString", c.Name)),
                    new XElement(gmd + "contactInfo",
                        new XElement(gmd + "CI_Contact",
                            new XElement(gmd + "address",
                                new XElement(gmd + "CI_Address",
                                    new XElement(gmd + "electronicMailAddress",
                                        new XElement(gco + "CharacterString", c.Email)))))),
                    new XElement(gmd + "role",
                        new XElement(gmd + "CI_RoleCode",
                            new XAttribute("codeList", "http://standards.iso.org/ittf/PubliclyAvailableStandards/ISO_19139_Schemas/resources/codelist/gmxCodelists.xml#CI_RoleCode"),
                            new XAttribute("codeListValue", c.Role),
                            c.Role))); // todo should ideally store the human-readable meaning as the element value, but for now using the codeListValue
            }
        }

        #endregion
    }
}
