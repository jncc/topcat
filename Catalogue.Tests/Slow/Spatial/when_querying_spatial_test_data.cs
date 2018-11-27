using System.Linq;
using Catalogue.Data.Indexes;
using Catalogue.Data.Model;
using Catalogue.Data.Seed;
using FluentAssertions;
using NUnit.Framework;
using Raven.Client.Documents;
using Raven.Client.Documents.Indexes.Spatial;

namespace Catalogue.Tests.Slow.Spatial
{
    internal class when_querying_spatial_test_data : SeededDbTest
    {
        [Test, Ignore("raven4")]
        public void non_intersecting_boxes_should_not_intersect()
        {
            Db.Query<Record, RecordSpatialIndex>()
                .Spatial(
                    x => x.Wkt,
                    criteria => criteria.RelatesToShape(Seeder.BoundingBoxContainingNothing, SpatialRelation.Intersects)
                    )
                //.Spatial(
                //    x =>
                //        x.RelatesToShape(FieldNames.Spatial, Seeder.BoundingBoxContainingNothing,
                //            SpatialRelation.Intersects))
                .Count().Should().Be(0);
        }
        // raven 4
        //[Test]
        //public void intersecting_boxes_should_intersect()
        //{
        //    Db.Query<Record, RecordSpatialIndex>()
        //        .Customize(
        //            x =>
        //                x.RelatesToShape(FieldNames.Spatial, Seeder.BoundingBoxContainingSmallBox,
        //                    SpatialRelation.Intersects))
        //        .Count().Should().Be(1);
        //}

        //[Test]
        //public void inner_box_should_be_within_outer_box()
        //{
        //    Db.Query<Record, RecordSpatialIndex>()
        //        .Customize(
        //            x =>
        //                x.RelatesToShape(FieldNames.Spatial, Seeder.BoundingBoxContainingSmallBox,
        //                    SpatialRelation.Within))
        //        .Count().Should().Be(1);
        //}

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
//            Db.Query<Record, RecordSpatialIndex>()
//                .Customize(x => x.RelatesToShape(FieldNames.Spatial, Seeder.BoundingBoxContainingSmallBox, SpatialRelation.Intersects))
//                .Where(i => i.Gemini.Title.StartsWith("Small"))
//                .Count().Should().Be(1);
//        }
    }
}