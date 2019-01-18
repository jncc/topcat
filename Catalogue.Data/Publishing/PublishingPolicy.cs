using System;
using System.Collections.Generic;
using System.Linq;
using Catalogue.Data.Extensions;
using Catalogue.Data.Model;
using Catalogue.Gemini.Model;
using Catalogue.Utilities.Text;

namespace Catalogue.Data.Publishing
{
    public static class PublishingPolicy
    {
        public static PublishingPolicyResult GetPublishingPolicyResult(Record record)
        {
            var result = new PublishingPolicyResult();
            
            if (record.Publication?.OpenData?.Publishable == true)
            {
                var canonicalResources = record.Publication.OpenData.Resources;

                if (record.Gemini.ResourceType.Equals("publication")) {
                    result.Message = "This is a JNCC publication.";
                    result.HubRecord = true;
                    result.HubResources = canonicalResources;
                    result.GovRecord = false;
                } else // datasets, non-geographic datasets, and services
                {
                    if (HasDoiAndPreviouslyPublished(record))
                    {
                        result.Message = "This record has a DOI and cannot be republished.";
                        result.HubRecord = false;
                        result.GovRecord = false;
                    }
                    else if (IsDarwinPlusRecord(record))
                    {
                        result.Message = "This is a Darwin Plus record.";
                        result.HubRecord = false;
                        result.GovRecord = true;
                        result.GovResources = GetCanonicalResourceStrings(canonicalResources);
                    }
                    else if (HasUnknownOwnership(record)) {
                        result.Message = "This record is not fully Open Data as it has unknown ownership.";
                        result.HubRecord = true;
                        result.HubResources = canonicalResources;
                        result.GovRecord = false;
                    }
                    else if (HasRestrictiveLicensing(record))
                    {
                        result.Message = "This record is not fully Open Data as it has a restrictive licence.";
                        result.HubRecord = true;
                        result.HubResources = canonicalResources;
                        result.GovRecord = false;
                    }
                    else
                    {
                        result.Message = "This is an Open Data record.";
                        result.HubRecord = true;
                        result.HubResources = canonicalResources;
                        result.GovRecord = true;
                    }
                }
            }
            else
            {
                result.Message = "This record has not been marked as publishable.";
                result.HubRecord = false;
                result.GovRecord = false;
            }

            return result;
        }

        private static bool IsDarwinPlusRecord(Record record)
        {
            // TODO: Change this when we decide what keyword to use
            return record.Gemini.Keywords.Any(x => x.Value.Equals("Darwin Plus"));
        }

        private static bool HasDoiAndPreviouslyPublished(Record record)
        {
            return record.DigitalObjectIdentifier.IsNotBlank() && record.Publication.OpenData.LastSuccess != null;
        }

        private static bool HasRestrictiveLicensing(Record record)
        {
            // TODO: Change this when we decide what keyword to use
            return record.Gemini.Keywords.Any(x => x.Value.Equals("Restrictive Licence"));
        }

        private static bool HasUnknownOwnership(Record record)
        {
            // TODO: Change this when we decide what keyword to use
            return record.Gemini.Keywords.Any(x => x.Value.Equals("Unknown Ownership"));
        }

        private static List<string> GetCanonicalResourceStrings(List<Resource> resources)
        {
            var canonicalStrings = new List<string>();
            if (resources != null)
            {
                foreach (var resource in resources)
                {
                    canonicalStrings.Add(resource.Path);
                }
            }

            return canonicalStrings;
        }
    }
}
