using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.UI.WebControls;

namespace Catalogue.Web.Controllers.Sandbox
{
    public class SandboxController : ApiController
    {
        public HttpResponseMessage Get([FromUri] InputModel input)
        {
            string x = input.x;
            return new HttpResponseMessage();
        }

        public class InputModel
        {
            public string x { get; set; }
            public List<int> ys { get; set; }
        }
    }
}