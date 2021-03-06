﻿using log4net;
using Raven.Client.Documents;
using System;
using System.Linq;

namespace Catalogue.Toolbox.Patch
{
    public class PatchHandler
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(PatchHandler));

        public void RunPatch(PatchOptions options)
        {
            Logger.Info("Running data patch");

            var patcher = InstantiatePatcher(options);

            patcher.Patch();

            Logger.Info("Patch completed successfully");
        }

        IDataPatcher InstantiatePatcher(PatchOptions options)
        {
            var type = typeof(IDataPatcher).Assembly.GetType("Catalogue.Toolbox.Patch.DataPatchers." + options.Name);

            if (type == null)
                throw new Exception($"The data patcher '{options.Name}' couldn't be found or does not exist.");

            return (IDataPatcher)Activator.CreateInstance(type);
        }
    }
}
