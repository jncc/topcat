using Catalogue.Data.Model;
using System.Collections.Generic;

namespace Catalogue.Data.Publishing
{
    public class PublishingPolicyResult
    {
        public bool GovRecord { get; set; }
        public List<string> GovResources { get; set; }
        public bool HubRecord { get; set; }
        public List<Resource> HubResources { get; set; }
        public string Message { get; set; }
    }
}
