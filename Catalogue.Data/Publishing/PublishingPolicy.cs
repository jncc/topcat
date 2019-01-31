using Catalogue.Data.Model;
using Catalogue.Utilities.Text;
using System.Collections.Generic;
using System.Linq;

namespace Catalogue.Data.Publishing
{
    public static class PublishingPolicy
    {
        public static PublishingPolicyResult GetPublishingPolicyResult(Record record)
        {
            var result = new PublishingPolicyResult();
            var canonicalResources = new List<Resource>();
            if (record.Publication?.Data?.Resources?.Count > 0)
            {
                canonicalResources = record.Publication.Data.Resources;
            }

            if (HasDoiAndPreviouslyPublished(record))
            {
                result.Message = "This record has a DOI and cannot be republished.";
                result.HubRecord = false;
                result.GovRecord = false;
            } else if (record.Gemini.ResourceType.Equals("publication")) {
                result.Message = "This is a JNCC publication.";
                result.HubRecord = true;
                result.HubResources = canonicalResources;
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

            return result;
        }

        private static bool IsDarwinPlusRecord(Record record)
        {
            // TODO: Change this when we decide what keyword to use
            return record.Gemini.Keywords.Any(x => x.Value.Equals("Darwin Plus"));
        }

        private static bool HasDoiAndPreviouslyPublished(Record record)
        {
            return record.DigitalObjectIdentifier.IsNotBlank() && record.Publication?.Gov?.LastSuccess != null;
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
