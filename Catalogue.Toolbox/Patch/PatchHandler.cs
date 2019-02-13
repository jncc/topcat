using log4net;
using Raven.Client.Documents;
using System;
using System.Linq;

namespace Catalogue.Toolbox.Patch
{
    public class PatchHandler
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(PatchHandler));

        readonly IDocumentStore store;

        public PatchHandler(IDocumentStore store)
        {
            this.store = store;
        }

        public void RunPatch(PatchOptions options)
        {
            Logger.Info("Running data patch");

            var patcher = InstantiatePatcher(options, store);

            var results = patcher.Patch();
            var successes = results.Count(r => r.Success);
            var failures = results.Count(r => !r.Success);

            if (failures > 0)
            {
                Logger.Error($"Something went wrong, there are {failures} failures");
            }
            else
            {
                Logger.Info($"Patch completed successfully! {successes} records updated");
            }
        }

        IDataPatcher InstantiatePatcher(PatchOptions options, IDocumentStore docStore)
        {
            var type = typeof(IDataPatcher).Assembly.GetType("Catalogue.Toolbox.Patch.DataPatchers." + options.Name);

            if (type == null)
                throw new Exception($"The data patcher '{options.Name}' couldn't be found or does not exist.");

            return (IDataPatcher)Activator.CreateInstance(type, docStore);
        }
    }
}
