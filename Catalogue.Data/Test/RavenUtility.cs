using System;
using System.Threading;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using Raven.Client.Documents.Operations;

namespace Catalogue.Data.Test
{
    public static class RavenUtility
    {
        public static void WaitForIndexing(IDocumentSession session)
        {
            WaitForIndexing(session.Advanced.DocumentStore);
        }

        public static void WaitForIndexing(IDocumentStore store, string db = null)
        {
            // raven4
            //var databaseCommands = store.Maintenance;
            //if (db != null)
            //    databaseCommands = databaseCommands.ForDatabase(db);
            //SpinWait.SpinUntil(() => databaseCommands.Send(new GetStatisticsOperation()).StaleIndexes.Length == 0);
        }
    }
}
