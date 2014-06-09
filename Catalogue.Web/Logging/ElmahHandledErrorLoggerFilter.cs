using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.Filters;
using System.Web.Mvc;
using Elmah;
using IExceptionFilter = System.Web.Mvc.IExceptionFilter;

namespace Catalogue.Web.Logging
{
    /// <summary>
    /// http://stackoverflow.com/questions/766610/how-to-get-elmah-to-work-with-asp-net-mvc-handleerror-attribute
    /// </summary>
    public class ElmahHandledErrorLoggerFilter : IExceptionFilter, IFilter
    {
        public void OnException(ExceptionContext context)
        {
            // Log only handled exceptions, because all other will be caught by ELMAH anyway.
            if (context.ExceptionHandled)
                ErrorSignal.FromCurrentContext().Raise(context.Exception);
        }

        public bool AllowMultiple { get; private set; }
    }
}