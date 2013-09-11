using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Catalogue.Data;
using Catalogue.Data.Indexes;
using Catalogue.Data.Model;
using Catalogue.Tests.Utility;
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
            Db.Query<Record, Items_SpatialIndex>()
                .Customize(x => x.RelatesToShape(FieldNames.Spatial, Seeder.BoundingBoxContainingNothing, SpatialRelation.Intersects))
                .Count().Should().Be(0);
        }

        [Test]
        public void intersecting_boxes_should_intersect()
        {
            Db.Query<Record, Items_SpatialIndex>()
                .Customize(x => x.RelatesToShape(FieldNames.Spatial, Seeder.BoundingBoxContainingSmallBox, SpatialRelation.Intersects))
                .Count().Should().Be(1);
        }

        [Test]
        public void inner_box_should_be_within_outer_box()
        {
            Db.Query<Record, Items_SpatialIndex>()
                .Customize(x => x.RelatesToShape(FieldNames.Spatial, Seeder.BoundingBoxContainingSmallBox, SpatialRelation.Within))
                .Count().Should().Be(1);
        }

//        //todo:
//        [Test]
//        public void tiny_inner_box_should_be_within_large_outer_box()
//        {
//            Assert.Fail();
//        }
//
//        [Test]
//        public void should_be_able_to_combine_spatial_query_with_nonspatial_quer()
//        {
//            Db.Query<Record, Items_SpatialIndex>()
//                .Customize(x => x.RelatesToShape(FieldNames.Spatial, Seeder.BoundingBoxContainingSmallBox, SpatialRelation.Intersects))
//                .Where(i => i.Metadata.Title.StartsWith("Small"))
//                .Count().Should().Be(1);
//        }
    }
}

