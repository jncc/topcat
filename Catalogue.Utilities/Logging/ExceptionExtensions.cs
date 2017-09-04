using System;
using log4net;

namespace Catalogue.Utilities.Logging
{
    public static class ExceptionExtensions
    {
        public static void LogAndThrow(this Exception e, ILog logger)
        {
            logger.Error(e);
            throw e;
        }
    }
}
