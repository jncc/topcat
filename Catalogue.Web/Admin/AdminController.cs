﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Http;
using Catalogue.Data.Import;
using Catalogue.Data.Import.Mappings;
using Catalogue.Data.Seed;
using NUnit.Framework.Constraints;
using Raven.Client;

namespace Catalogue.Web.Admin
{
    /*Slightly weird controller, should only be used once in live system, when importing the data*/
    public class AdminController : ApiController
    {

        public class FileSpec
        {
            public string Path { get; set; }         
        }

        public Boolean Post(FileSpec file)
        {

            using (var db = WebApiApplication.DocumentStore.OpenSession())
            {
                var importer = Importer.CreateImporter<ActivitiesMapping>(db);
                //importer.SkipBadRecords = true; // todo remove this
                importer.Import(file.Path);
                db.SaveChanges();
            }
            return true;
        }

        /*using put as is convenient*/
        public Boolean Put()
        {
            Seeder.Seed(WebApiApplication.DocumentStore);
            return true;
        }
        
        /*used for testing excpetion handling*/
        public void Get()
        {
            IDocumentStore documentStore = WebApiApplication.DocumentStore;
            
            throw new IOException("My Test Message");
        
        }
    }
}
