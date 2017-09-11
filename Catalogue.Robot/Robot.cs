using Catalogue.Data.Write;
using Catalogue.Robot.Publishing.OpenData;
using Catalogue.Utilities.Logging;
using Catalogue.Utilities.Text;
using log4net;
using Newtonsoft.Json;
using Raven.Client;
using System;
using System.IO;

namespace Catalogue.Robot
{
    public class Robot
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(Robot));

        public void Start()
        {
            Logger.Info("Starting Robot");

        }

        public void Stop()
        {
            Logger.Info("Stopping Robot");
        }


    }
}
