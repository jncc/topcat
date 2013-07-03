using System.Threading;
using Raven.Client;

namespace Catalogue.Tests.Utility
{
    public class RavenUtility
    {
        public static void WaitForIndexing(IDocumentStore store, string db = null)
        {
            var databaseCommands = store.DatabaseCommands;
            if (db != null)
                databaseCommands = databaseCommands.ForDatabase(db);
            SpinWait.SpinUntil(() => databaseCommands.GetStatistics().StaleIndexes.Length == 0);
        }
    }
}
