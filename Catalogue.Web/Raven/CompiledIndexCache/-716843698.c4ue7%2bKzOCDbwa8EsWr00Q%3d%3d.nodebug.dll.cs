using Raven.Abstractions;
using Raven.Database.Linq;
using System.Linq;
using System.Collections.Generic;
using System.Collections;
using System;
using Raven.Database.Linq.PrivateExtensions;
using Lucene.Net.Documents;
using System.Globalization;
using System.Text.RegularExpressions;
using Raven.Database.Indexing;


public class Index_Items_2fSpatialIndex : Raven.Database.Linq.AbstractViewGenerator
{
	public Index_Items_2fSpatialIndex()
	{
		this.ViewText = @"docs.Records.Select(e => new {
    __ = SpatialGenerate(""SpatialField"", e.Wkt, Raven.Abstractions.Indexing.SpatialSearchStrategy.QuadPrefixTree, 16)
})";
		this.ForEntityNames.Add("Records");
		this.AddMapDefinition(docs => docs.Where(__document => string.Equals(__document["@metadata"]["Raven-Entity-Name"], "Records", System.StringComparison.InvariantCultureIgnoreCase)).Select((Func<dynamic, dynamic>)(e => new {
			__ = SpatialGenerate("SpatialField", e.Wkt, Raven.Abstractions.Indexing.SpatialSearchStrategy.QuadPrefixTree, 16),
			__document_id = e.__document_id
		})));
		this.AddField("__");
		this.AddField("__document_id");
	}
}
