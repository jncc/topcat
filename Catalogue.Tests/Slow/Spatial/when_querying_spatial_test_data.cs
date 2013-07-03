using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Catalogue.Data;
using Catalogue.Data.Indexes;
using Catalogue.Data.Model;
using FluentAssertions;
using NUnit.Framework;
using Raven.Abstractions.Indexing;

namespace Catalogue.Tests.Slow.Spatial
{
    class when_querying_spatial_test_data : DatabaseTestFixture
    {
        [Test]
        public void non_intersecting_boxes_should_not_intersect()
        {
            Db.Query<Item, Items_SpatialIndex>()
                .Customize(x => x.RelatesToShape(FieldNames.Spatial, Seeder.BoundingBoxContainingNothing, SpatialRelation.Intersects))
                .Count().Should().Be(0);
        }

        [Test]
        public void intersecting_boxes_should_intersect()
        {
            Db.Query<Item, Items_SpatialIndex>()
                .Customize(x => x.RelatesToShape(FieldNames.Spatial, Seeder.BoundingBoxContainingSmallBox, SpatialRelation.Intersects))
                .Count().Should().Be(1);
        }

        [Test]
        public void small_box_should_be_within_containing_box()
        {
            Db.Query<Item, Items_SpatialIndex>()
                .Customize(x => x.RelatesToShape(FieldNames.Spatial, Seeder.BoundingBoxContainingSmallBox, SpatialRelation.Within))
                .Count().Should().Be(1);
        }

        //                .Where(i => i.Id == Seeder.SmallBox.Id) 
    }
}

