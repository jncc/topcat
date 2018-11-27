﻿using System;
using System.Linq;
using Catalogue.Data;
using Catalogue.Data.Test;
using FluentAssertions;
using NUnit.Framework;
using Raven.Client.Documents.Indexes;
using Raven.Client.Documents.Queries.Highlighting;
using Raven.Client.Documents.Session;

namespace Catalogue.Tests.Explicit
{
    internal class highlighting_wildcard_queries : DatabaseTestFixture
    {
        [Explicit, Test]
        public void highlighting_should_work_with_wildcard_queries()
        {
            using (IDocumentSession db = ReusableDocumentStore.OpenSession())
            {
                db.Store(new Item
                {
                    Title =
                        "On the 9th. William Findley and David Redick--deputed by the Committee of Safety (as it is designated) which met on the 2d. of this month at Parkinson Ferry arrived in Camp with the Resolutions of the said Committee; and to give information of the State of things in the four Western Counties of Pennsylvania to wit--Washington Fayette Westd. & Alligany in order to see if it would prevent the March of the Army into them."
                });
                db.Store(new Item
                {
                    Title =
                        "At 10 oclock I had a meeting with these persons in presence of Govr. Howell (of New Jersey) the Secretary of the Treasury, Colo. Hamilton, & Mr. Dandridge: Govr. Mifflin was invited to be present, but excused himself on acct. of business."
                });
                db.Store(new Item
                {
                    Title =
                        "Mr. Findley began. He confined his information to such parts of the four Counties as he was best acquainted with; referring to Mr. Reddick for a recital of what fell within his knowledge, in the other parts of these Counties"
                });
                db.SaveChanges();
            }

            new SearchIndex().Execute(ReusableDocumentStore);
            RavenUtility.WaitForIndexing(ReusableDocumentStore);

            using (IDocumentSession db = ReusableDocumentStore.OpenSession())
            {
                Func<string, Highlightings> execute = q =>
                {
                    Highlightings lites;
                    db.Advanced.DocumentQuery<Item>("SearchIndex")
                        .WaitForNonStaleResults()
                        .Highlight("Title", 128, 2, out lites)
                        .Search("Title", q)
                        .ToList();
                    return lites;
                };

                execute("committee").ResultIndents.Should().NotBeEmpty(); // succeeds
                execute("committee*").ResultIndents.Should().NotBeEmpty(); // fails
            }
        }

        public class Item
        {
            public string Title { get; set; }
        }

        public class SearchIndex : AbstractMultiMapIndexCreationTask<Item>
        {
            public SearchIndex()
            {
                AddMap<Item>(docs => from doc in docs
                    select new {doc.Title});


                Index(x => x.Title, FieldIndexing.Search);
                Store(x => x.Title, FieldStorage.Yes);
                TermVector(x => x.Title, FieldTermVector.WithPositionsAndOffsets);
            }
        }
    }
}