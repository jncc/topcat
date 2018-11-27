﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Catalogue.Gemini.Roles;
using FluentAssertions;

namespace Catalogue.Web.Controllers.Roles
{
    public class RolesController : ApiController
    {
        public List<string> Get(String q)
        {
            if (!String.IsNullOrWhiteSpace(q))
            {
                return (from r in ResponsiblePartyRoles.Allowed
                        where r.ToLowerInvariant().Contains(q)
                        select r).ToList();
            }
            return ResponsiblePartyRoles.Allowed;
        }
    }

    class roles_controller_tests
    {
        public void should_get_all_roles()
        {
            var c = new RolesController();
            var result = c.Get("");

            result.Should().Equal(ResponsiblePartyRoles.Allowed);
        }

        public void should_get_roles_with_search()
        {
            var c = new RolesController();
            var result = c.Get("p");

            result.Count.Should().Be(5);
        }
    }
}
