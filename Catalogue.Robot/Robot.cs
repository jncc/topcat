using log4net;

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
