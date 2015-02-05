using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.Filters;

namespace Catalogue.Web.Logging
{
    public class UnhandledExceptionFilter:ExceptionFilterAttribute
    {
        //public override void OnException(HttpActionExecutedContext context)
        //{
        //    Elmah.ErrorLog.GetDefault(HttpContext.Current).Log(new Elmah.Error(context.Exception));
        //}
    }
}