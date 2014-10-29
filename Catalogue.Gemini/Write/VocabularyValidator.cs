using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Catalogue.Gemini.Model;
using Raven.Client;

namespace Catalogue.Gemini.Write
{
    public interface IVocabularyValidator
    {
        VocabularyValidationResult Valdiate(Vocabulary sourceVocab, bool allowControlledUpdates);
    }

    public class VocabularyValidationResult
    {
        public List<VocabularyValidationResultMessage> Errors { get; set; }
        public List<VocabularyValidationResultMessage> Warnings { get; set; }
    }

    public class VocabularyValidationResultMessage
    {
        public string Message { get; set; }
        public string FieldName { get; set; }
    }

    public class VocabularyValidator : IVocabularyValidator
    {
        //check uri format is correct
        //check publication date format.
        //check existing keyword values haven't been changed to duplicate other existing values

        private readonly IDocumentSession db;

        public VocabularyValidator(IDocumentSession db)
        {
            this.db = db;
        }

        public VocabularyValidationResult Valdiate(Vocabulary sourceVocab, bool allowControlledUpdates)
        {
            var result = new VocabularyValidationResult {Errors = new List<VocabularyValidationResultMessage>(), Warnings = new List<VocabularyValidationResultMessage>()};
            //check uri format is correct
            var r1 = ValidateVocabularyUri(sourceVocab.Id);
            if (r1 != null) result.Errors.Add(r1);

            var targetVocab = db.Load<Vocabulary>(sourceVocab.Id);

            //check existing keyword values haven't been changed to duplicate other existing values
            result.Errors.AddRange(ValidateKeywordChanges(sourceVocab, targetVocab));
            
            //Check for updates to controlled vocabs and duplicates that will be removed and warn
            result.Warnings.AddRange(ValidateKeywordAdditions(sourceVocab, targetVocab));

            //validate additions to controlled vocabs

            result.Errors.AddRange(ValidateControlledVocab(sourceVocab, targetVocab, allowControlledUpdates));

            //check publication date format.
            var r2 = ValidatePublicationDate(sourceVocab.PublicationDate);
            if (r2 != null) result.Errors.Add(r2);

            return result;
        }

        private List<VocabularyValidationResultMessage> ValidateControlledVocab(Vocabulary sourceVocab, Vocabulary targetVocab, bool allowControlledUpdates)
        {
            var result = new List<VocabularyValidationResultMessage>();

            if (targetVocab == null || allowControlledUpdates) return result;

            var invalidKeywords =
                sourceVocab.Keywords.Where(x => new HashSet<Guid>(targetVocab.Keywords.Select(y => y.Id)).Contains(x.Id)).Select(x => x);

            foreach (var invalidKeyword in invalidKeywords)
            {
                result.Add(new VocabularyValidationResultMessage
                    {
                        FieldName = invalidKeyword.Id.ToString(),
                        Message =
                            String.Format(
                                "The keyword {0} annot be added to the vocabulary {1} because it is controlled",
                                targetVocab.Id, invalidKeyword.Value)
                    });
            }

            return result;
        }

        private IEnumerable<VocabularyValidationResultMessage> ValidateKeywordAdditions(Vocabulary sourceVocab, Vocabulary targetVocab)
        {
            HashSet<Guid> targetIdHash = targetVocab != null ? new HashSet<Guid>(targetVocab.Keywords.Select(x => x.Id)) : new HashSet<Guid>();

            var duplicates = from i in sourceVocab.Keywords.Where(s => (!targetIdHash.Any() || !targetIdHash.Contains(s.Id)))
                             group i by i.Value.ToLowerInvariant()
                                 into g
                                 select g.OrderBy(p => p.Value).First();

            return
                duplicates.Select(
                    keyword =>
                    new VocabularyValidationResultMessage
                        {
                            FieldName = keyword.Id.ToString(),
                            Message =
                                String.Format("The keyword {0} is duplicated and the duplicate will not be saved.",
                                              keyword.Value)
                        }).ToList();
        }

        private IEnumerable<VocabularyValidationResultMessage> ValidateKeywordChanges(Vocabulary source, Vocabulary target)
        {
            if (target == null) return new List<VocabularyValidationResultMessage>();

            var targetIdHash = new HashSet<Guid>(target.Keywords.Select(x => x.Id));

            var dupedKeywords =
                source.Keywords.Where(
                    s => targetIdHash.Contains(s.Id) && target.Keywords.Any(t => t.Id != s.Id && t.Value.Equals(s.Value,StringComparison.InvariantCultureIgnoreCase)));

            return (from duplicate1 in dupedKeywords
                    let targetVal = target.Keywords.Where(x => x.Id == duplicate1.Id).Select(x => x.Value)
                    select new VocabularyValidationResultMessage
                        {
                            FieldName = duplicate1.Id.ToString(),
                            Message = String.Format(
                            "Cannot change the value of keyword {0} to {1} because it will create a duplicate keyword",
                            targetVal, duplicate1.Value) 
                        }
                        ).ToList();
        }

        private VocabularyValidationResultMessage ValidatePublicationDate(string publicationDate)
        {
            if (String.IsNullOrWhiteSpace(publicationDate)) return null;
            
            DateTime date;
            
            if (!DateTime.TryParse(publicationDate, out date))
            {
                return new VocabularyValidationResultMessage
                    {
                        FieldName = "PublicationDate",
                        Message = String.Format("{0} cannot be parsed as a valid date", publicationDate)
                    };
            }

            return null;
        }

        private VocabularyValidationResultMessage ValidateVocabularyUri(string id)
        {
            Uri url;


            if (String.IsNullOrWhiteSpace(id))
            {
                return new VocabularyValidationResultMessage
                    {
                        Message = "A vocabulary must have a properly formed Id",
                        FieldName = "Id"
                    };
            }

            if (Uri.TryCreate(id, UriKind.Absolute, out url))
            {
                if (url.Scheme != Uri.UriSchemeHttp)
                {

                    return new VocabularyValidationResultMessage
                        {
                            Message = String.Format("Resource locator {0} is not an http url", id),
                            FieldName = "Id"
                        };
                }
            }
            else
            {
                return new VocabularyValidationResultMessage
                    {
                        Message = String.Format("Resource locator {0} is not a valid url", id),
                        FieldName = "Id"
                    };
            }

            return null;
        }
    }
}
