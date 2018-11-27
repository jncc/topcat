using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Catalogue.Data.Indexes;
using Catalogue.Data.Model;
using Raven.Client.Documents.Session;

namespace Catalogue.Robot.DeadLinks
{
    public class Checker
    {
        IDocumentSession db;
        FileLinkChecker fileLinkChecker;

        public Checker(IDocumentSession db, FileLinkChecker fileLinkChecker)
        {
            this.db = db;
            this.fileLinkChecker = fileLinkChecker;
        }

        public List<LinkCheckResult> CheckAll()
        {
            var query = db.Query<Record, RecordStreamingIndex>();
            var enumerator = db.Advanced.Stream(query);

            var results = new List<LinkCheckResult>();

            while (enumerator.MoveNext())
            {
                var record = enumerator.Current.Document;
                var result = CheckLink(record.Id, record.Path);
                results.Add(result);
            }

            return results;
        }

        public LinkCheckResult CheckLink(string id, string link)
        {
            Uri uri;

            if (!Uri.TryCreate(link, UriKind.Absolute, out uri))
                return new LinkCheckResult { Status = LinkCheckStatus.Fail, Message = "Couldn't parse link as a URI.", Record = id };

            if (uri.Scheme == Uri.UriSchemeFile)
                return fileLinkChecker.Check(id, uri);

            if (uri.Scheme == Uri.UriSchemeHttp)
                throw new NotImplementedException();

            return new LinkCheckResult { Status = LinkCheckStatus.Fail, Message = "Unsupported URI scheme '" + uri.Scheme + "'.", Record = id };
        }
    }

    public class LinkCheckResult
    {
        public LinkCheckStatus Status  { get; set; }
        public string          Message { get; set; }
        public string            Record  { get; set; }
    }

    public enum LinkCheckStatus
    {
        Ok, Missing, Fail
    }
}
