using System;
using System.Collections.Generic;
using System.Linq;
using Catalogue.Data.Extensions;
using Catalogue.Data.Model;
using Catalogue.Gemini.Model;
using Catalogue.Utilities.Text;

namespace Catalogue.Data.Publishing
{
    public class PublishingPolicy
    {
        public PublishingPolicyResult GetPublishingPolicyResult(Record record)
        {
            var result = new PublishingPolicyResult();
            var canonicalResources = record.Publication.OpenData.Resources;

            // Publishable and signed off
            if (record.Publication?.OpenData?.Publishable == true)
            {
                if (HasDoiAndPreviouslyPublished(record))
                {
                    result.Message = "This record has a DOI and cannot be republished";
                    result.HubRecord = false;
                    result.GovRecord = false;
                } else if (IsDarwinPlusRecord(record))
                {
                    result.Message = "This is a Darwin Plus record";
                    result.HubRecord = false;
                    result.GovRecord = true;
                    result.GovResources = GetCanonicalResourceStrings(canonicalResources);
                // } else if open data issue? {    
                } else if (record.Gemini.ResourceType.Equals("publication")) {
                    result.Message = "This is a JNCC publication";
                    result.HubRecord = true;
                    result.HubResources = canonicalResources;
                    result.GovRecord = false;
                } else 
                {
                    result.Message = "This is an Open Data record";
                    result.HubRecord = true;
                    result.HubResources = canonicalResources;
                    result.GovRecord = true;
                }
                // Data available on request
                // Datahub dataset?
            }
            else
            {
                result.Message = "This record has not been marked as publishable";
                result.HubRecord = false;
                result.GovRecord = false;
            }

            return result;
        }

        private bool IsDarwinPlusRecord(Record record)
        {
            // Change this when we decide what keyword to use
            return record.Gemini.Keywords.Any(x => x.Value.Equals("Darwin Plus"));
        }

        private bool HasDoiAndPreviouslyPublished(Record record)
        {
            return record.DigitalObjectIdentifier.IsNotBlank() && record.Publication.OpenData.LastSuccess != null;
        }

        private List<string> GetCanonicalResourceStrings(List<Resource> resources)
        {
            var canonicalStrings = new List<string>();
            foreach (var resource in resources)
            {
                canonicalStrings.Add(resource.Path);
            }

            return canonicalStrings;
        }
    }

    public class PublishingPolicyResult
    {
        public bool GovRecord { get; set; }
        public List<string> GovResources { get; set; }
        public bool HubRecord { get; set; }
        public List<Resource> HubResources { get; set; }
        public string Message { get; set; }
    }
}
