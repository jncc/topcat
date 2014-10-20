using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Catalogue.Gemini.Model;

namespace Catalogue.Web.Search
{
    public class SearchInputModel
    {
         
        //public SearchInputModel(int pageNumber, string query, int numberOfRecords)
        //{
        //    PageNumber = pageNumber;
        //    Query = query;
        //    NumberOfRecords = numberOfRecords;
        //    SearchType = SearchType.FullText;
        //}

        //public SearchInputModel(int pageNumber, Keyword keyword, int numberOfRecords)
        //{
        //    PageNumber = pageNumber;
        //    Keyword = keyword;
        //    SearchType = SearchType.Keyword;
        //}

        public int NumberOfRecords { get; set; }
        public int PageNumber { get; set; }
        public string Query { get; set; }
        public MetadataKeyword Keyword { get; set; }
        public SearchType SearchType { get; set; }
    }

}