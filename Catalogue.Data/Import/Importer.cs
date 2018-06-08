using Catalogue.Data.Import.Mappings;
using Catalogue.Data.Model;
using Catalogue.Data.Write;
using Catalogue.Gemini.Model;
using Catalogue.Utilities.Text;
using CsvHelper;
using System;
using System.Collections.Generic;
using System.IO;
using Raven.Client.Documents.Session;

namespace Catalogue.Data.Import
{
    public class Importer
    {
        /// <summary>
        /// Helper to conveniently create an importer instance.
        /// </summary>
        public static Importer CreateImporter(IDocumentSession db, IMapping mapping)
        {
            return new Importer(
                mapping,
                new FileSystem(),
                new RecordService(db, new RecordValidator()),
                new VocabularyService(db, new VocabularyValidator()),
                new UserInfo
                {
                    DisplayName = "Topcat Importer",
                    Email = "data@jncc.gov.uk"
                });
        }

        readonly IMapping mapping;
        readonly IFileSystem fileSystem;
        readonly IRecordService recordService;
        readonly IVocabularyService vocabularyService;
        readonly UserInfo userInfo;

        public bool SkipBadRecords { get; set; }

        public readonly List<RecordServiceResult> Results = new List<RecordServiceResult>();

        public Importer(IMapping mapping, IFileSystem fileSystem, IRecordService recordService, IVocabularyService vocabularyService, UserInfo userInfo)
        {
            this.mapping = mapping;
            this.fileSystem = fileSystem;
            this.recordService = recordService;
            this.vocabularyService = vocabularyService;
            this.userInfo = userInfo;
        }

        public void Import(string path)
        {
            using (var reader = fileSystem.OpenReader(path))
            {
                Import(reader);
            }
        }

        public void Import(TextReader reader)
        {
            var csv = new CsvReader(reader);

            mapping.Apply(csv.Configuration);

            var records = csv.GetRecords<Record>();

            int n = 1;
            var keywords = new List<MetadataKeyword>();

            try
            {
                foreach (var record in records)
                {
                    var result = recordService.Insert(record, userInfo);

                    if (!result.Success)
                    {
                        if (!SkipBadRecords)
                        {
                            throw new ImportException(String.Format("Import failed due to validation errors at record {0}: {1}",
                                n, result.Validation.Errors.ToConcatenatedString(e => e.Message, "; ")));
                        }
                    }

                    n++;
                    Results.Add(result);

                    keywords.AddRange(result.Record.Gemini.Keywords);
                }
            }
            catch (CsvHelperException ex)
            {
                string info = (string) ex.Data["CsvHelper"];
                throw new ImportException("CsvHelper exception: " + info, ex);
            }

            // import new vocabs and keywords
            foreach (var vocab in mapping.RequiredVocabularies)
            {
                // if vocab already exists, Insert returns a VocabularyServiceResult with Success=false, which we just ignore
                vocabularyService.Insert(vocab);
            }
            vocabularyService.AddKeywordsToExistingControlledVocabs(keywords);
        }
    }
}

