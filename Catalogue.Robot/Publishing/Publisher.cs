using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catalogue.Robot.Publishing
{
    public abstract class Publisher
    {
        // publication = { List<SuccessfulPublication> Publications, PublicationTarget[Type] target }

        // foreach target we support

        public abstract void Publish();
    }
}
