﻿using System.Collections.Generic;
using System.Linq;
using Catalogue.Data.Indexes;
using Catalogue.Data.Model;
using Catalogue.Gemini.DataFormats;
using Catalogue.Gemini.Model;
using Catalogue.Utilities.Text;
using Catalogue.Web.Controllers.Search;
using Raven.Client;

namespace Catalogue.Web.Search
{
    public interface ISearchService
    {
        SearchOutputModel KeywordSearch(SearchInputModel searchInputModel);
        SearchOutputModel FullTextSearch(SearchInputModel searchInputModel);
    }

    public class SearchService : ISearchService
    {
        private readonly IDocumentSession _db;

        public SearchService(IDocumentSession db)
        {
            _db = db;
        }

        public SearchOutputModel KeywordSearch(SearchInputModel searchInputModel)
        {
            RavenQueryStatistics stats;
            
            var query = _db.Query<Record>()
                .Statistics(out stats)
                .Where(r => r.Gemini.Keywords.Any(k => k.Value.Equals(searchInputModel.Keywords.First().Value)));

            int skipNumber = searchInputModel.PageNumber * searchInputModel.NumberOfRecords;

            var results = query
                .Skip(skipNumber)
                .Take(searchInputModel.NumberOfRecords)
                .ToList()
                .Select(r => new HalfBakedResult
                    {
                        Result = r,
                        Title = r.Gemini.Title.TruncateNicely(200),
                        Snippet = r.Gemini.Abstract.TruncateNicely(200)
                    });

            return MakeSearchOutputModel(searchInputModel, stats, results);

        }

        public SearchOutputModel FullTextSearch(SearchInputModel searchInputModel)
        {
            RavenQueryStatistics stats;
            FieldHighlightings titleLites;
            FieldHighlightings titleNLites;
            FieldHighlightings abstractLites;
            FieldHighlightings abstractNLites;

            IDocumentQuery<Record> query = _db.Advanced.LuceneQuery<Record>("Records/Search")
                .Statistics(out stats)
                .Highlight("Title", 202, 1, out titleLites)
                .Highlight("TitleN", 202, 1, out titleNLites)
                .Highlight("Abstract", 202, 1, out abstractLites)
                .Highlight("AbstractN", 202, 1, out abstractNLites)
                .SetHighlighterTags("<b>", "</b>")
                .Search("Title", searchInputModel.Query).Boost(10)
                .Search("TitleN", searchInputModel.Query)
                .Search("Abstract", searchInputModel.Query)
                .Search("AbstractN", searchInputModel.Query);
            
            int skipNumber = searchInputModel.PageNumber * searchInputModel.NumberOfRecords;
            
            List<Record> results = query
                    .Skip(skipNumber)
                    .Take(searchInputModel.NumberOfRecords).ToList();



            var xs = from r in results
                     let titleFragments =
                         titleLites.GetFragments("records/" + r.Id).Concat(titleNLites.GetFragments("records/" + r.Id))
                     let abstractFragments =
                         abstractLites.GetFragments("records/" + r.Id)
                                      .Concat(abstractNLites.GetFragments("records/" + r.Id))
                     select new HalfBakedResult
                         {
                             Result = r,
                             Title = titleFragments.Select(f => f.TruncateNicely(200)).FirstOrDefault()
                                     ?? r.Gemini.Title.TruncateNicely(200),
                             Snippet = abstractFragments.Select(f => f.TruncateNicely(200)).FirstOrDefault()
                                       ?? r.Gemini.Abstract.TruncateNicely(200)
                         };

            return MakeSearchOutputModel(searchInputModel, stats, xs);
        }

        private static SearchOutputModel MakeSearchOutputModel(SearchInputModel searchInputModel, RavenQueryStatistics stats,
                                                               IEnumerable<HalfBakedResult> xs)
        {
            return new SearchOutputModel
                {
                    Total = stats.TotalResults,
                    Results = (from x in xs
                               let format = DataFormatQueries.GetDataFormatInfo(x.Result.Gemini.DataFormat)
                               select new ResultOutputModel
                                   {
                                       Id = x.Result.Id,
                                       Title = x.Title,
                                       // could be better. always want the whole title, highlighted
                                       Snippet = x.Snippet,
                                       Format = new FormatOutputModel
                                           {
                                               Group = format.Group,
                                               Glyph = format.Glyph,
                                               Name = format.Name,
                                           },
                                       Keywords = x.Result.Gemini.Keywords
                                                   .OrderBy(k => k.Vocab != "http://vocab.jncc.gov.uk/jncc-broad-category")
                                   // show first
                                                   .ThenBy(k => k.Vocab).ToList(),
                                       TopCopy = x.Result.TopCopy,
                                       Date = x.Result.Gemini.DatasetReferenceDate,
                                   })
                        .ToList(),
                    Speed = stats.DurationMilliseconds,
                    Query =
                        new QueryOutputModel
                            {
                                Q = searchInputModel.Query,
                                P = searchInputModel.PageNumber,
                                N = searchInputModel.NumberOfRecords
                            }
                };
        }
    }

    class HalfBakedResult
    {
        public Record Result { get; set; }
        public string Title { get; set; }
        public string Snippet { get; set; }
    }
} 