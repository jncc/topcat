﻿using Catalogue.Data.Model;
using Catalogue.Gemini.Model;
using FluentAssertions;
using NUnit.Framework;
using System.Linq;

namespace Catalogue.Tests.Slow.Catalogue.Data.Seed
{
    public class when_seeding : SeededDbTest
    {
        [Test]
        public void should_seed_example_readonly_record()
        {
            var record = Db.Query<Record>()
                .First(r => r.Gemini.Title.StartsWith("An example read-only record"));

            record.ReadOnly.Should().BeTrue();
        }

        [Test]
        public void should_seed_small_box_record()
        {
            Db.Query<Record>().Count(r => r.Gemini.Title == "Small Box").Should().Be(1);
        }

        [Test]
        public void should_seed_vocabs()
        {
            Db.Query<Vocabulary>().Count().Should().Be(12);
        }
    }
}