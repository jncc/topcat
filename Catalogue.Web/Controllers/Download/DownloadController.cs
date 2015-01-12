using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Catalogue.Data.Write;
using Catalogue.Web.Code.Account;
using Raven.Client;

namespace Catalogue.Web.Controllers.Download
{
    public class DownloadController
    {
        readonly IDocumentSession db;
        readonly IRecordService service;
        readonly IUserContext user;

        public DownloadController(IDocumentSession db, IRecordService service, IUserContext user)
        {
            this.db = db;
            this.service = service;
            this.user = user;
        }

        public string Get(string vocab, string value)
        {
            throw new NotImplementedException(); // tod 
        }
    }
}