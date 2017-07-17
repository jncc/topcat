using Catalogue.Data.Write;
using Catalogue.Web.Security;
using System;
using System.Web.Http;

namespace Catalogue.Web.Controllers.Marking
{
    public class OpenDataMarkingController : ApiController
    {
        readonly IMarkingService markingService;

        public OpenDataMarkingController(IMarkingService markingService)
        {
            this.markingService = markingService;
        }

        [HttpPut, Route("api/marking/opendata"), OpenDataPublishers]
        public IHttpActionResult MarkAsOpenData(Guid id)
        {
            markingService.MarkAsOpenData(id);
            return Ok();
        }
    }
}