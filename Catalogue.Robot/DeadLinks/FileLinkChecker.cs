using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catalogue.Robot.DeadLinks
{
    public class FileLinkChecker
    {
        public LinkCheckResult Check(string id, Uri fileUri)
        {
            string uri = fileUri.ToString();

            try
            {
                if (Directory.Exists(uri) || File.Exists(uri))
                {
                    return new LinkCheckResult { Status = LinkCheckStatus.Ok , Record = id };
                }
                else
                {
                    return new LinkCheckResult { Status = LinkCheckStatus.Missing, Record = id };
                }
            }
            catch (IOException ex)
            {
                return new LinkCheckResult { Status = LinkCheckStatus.Fail, Record = id, Message = ex.Message };
            }
        }
    }
}
