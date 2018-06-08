using System;
using System.Threading;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;

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
            throw new Exception("todo");
// raven4
//            var databaseCommands = store.DatabaseCommands;
//            if (db != null)
//                databaseCommands = databaseCommands.ForDatabase(db);
//            SpinWait.SpinUntil(() => databaseCommands.GetStatistics().StaleIndexes.Length == 0);
        }
    }
}
