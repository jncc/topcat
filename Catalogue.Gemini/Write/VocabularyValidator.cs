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
        public List<string> Errors { get; set; }
        public List<string> Warnings { get; set; }
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
            var result = new VocabularyValidationResult {Errors = new List<string>(), Warnings = new List<string>()};
            //check uri format is correct
            var r1 = ValidateVocabularyUri(sourceVocab.Id);
            if (r1 != String.Empty) result.Errors.Add(r1);

            var targetVocab = db.Load<Vocabulary>(sourceVocab.Id);

            //check existing keyword values haven't been changed to duplicate other existing values
            result.Errors.AddRange(ValidateKeywordChanges(sourceVocab, targetVocab));
            
            //Check for updates to controlled vocabs and duplicates that will be removed and warn
            result.Warnings.AddRange(ValidateKeywordAdditions(sourceVocab, targetVocab));

            //validate additions to controlled vocabs

            var r2 =ValidateControlledVocab(sourceVocab, targetVocab, allowControlledUpdates);
            if (r2 != String.Empty) result.Errors.Add(r2);

            //check publication date format.
            var r3 = ValidatePublicationDate(sourceVocab.PublicationDate);
            if (r3 != String.Empty) result.Errors.Add(r3);

            return result;
        }

        private string ValidateControlledVocab(Vocabulary sourceVocab, Vocabulary targetVocab, bool allowControlledUpdates)
        {
            var result = String.Empty;

            if (targetVocab == null || allowControlledUpdates) return result;

            var invalidKeywoards =
                sourceVocab.Keywords.Where(x => new HashSet<Guid>(targetVocab.Keywords.Select(y => y.Id)).Contains(x.Id)).Select(x => x.Value).ToArray();

            if (invalidKeywoards.Any())
            {
                var keywordList = String.Join(", ", invalidKeywoards);
                result = String.Format("The following keywords cannot be added to the vocabulary {0} because it is controlled: {1}",targetVocab.Id,keywordList);
            }

            return result;
        }

        private IEnumerable<string> ValidateKeywordAdditions(Vocabulary sourceVocab, Vocabulary targetVocab)
        {
            HashSet<Guid> targetIdHash = targetVocab != null ? new HashSet<Guid>(targetVocab.Keywords.Select(x => x.Id)) : new HashSet<Guid>();

            var duplicates = from i in sourceVocab.Keywords.Where(s => (!targetIdHash.Any() || !targetIdHash.Contains(s.Id)))
                             group i by i.Value.ToLowerInvariant()
                                 into g
                                 select g.OrderBy(p => p.Value).First();

            return duplicates.Select(keyword => String.Format("The keyword {0} is duplicated and the duplicate will not be saved.", keyword.Value)).ToList();
        }

        private IEnumerable<string> ValidateKeywordChanges(Vocabulary source, Vocabulary target)
        {
            if (target == null) return new List<string>();

            var targetIdHash = new HashSet<Guid>(target.Keywords.Select(x => x.Id));

            var dupedKeywords =
                source.Keywords.Where(
                    s => targetIdHash.Contains(s.Id) && target.Keywords.Any(t => t.Id != s.Id && t.Value.Equals(s.Value,StringComparison.InvariantCultureIgnoreCase)));

            return (from duplicate1 in dupedKeywords
                    let targetVal = target.Keywords.Where(x => x.Id == duplicate1.Id).Select(x => x.Value)
                    select
                        String.Format(
                            "Cannot change the value of keyword {0} to {1} because it will create a duplicate keyword",
                            targetVal, duplicate1.Value)).ToList();
        }

        private string ValidatePublicationDate(string publicationDate)
        {
            var result = String.Empty;
            if (String.IsNullOrWhiteSpace(publicationDate)) return result;
            
            DateTime date;
            
            if (!DateTime.TryParse(publicationDate, out date))
            {
                result =  String.Format("{0} cannot be parsed as a valid date", publicationDate);
            }

            return result;
        }

        private String ValidateVocabularyUri(string id)
        {
            Uri url;

            if (String.IsNullOrWhiteSpace(id))
            {
                return "A vocabulary must have a properly formed Id";
            }

            if (Uri.TryCreate(id, UriKind.Absolute, out url))
            {
                if (url.Scheme != Uri.UriSchemeHttp)
                {

                    return String.Format("Resource locator {0} is not an http url", id);
                }
            }
            else
            {
                return String.Format("Resource locator {0} is not a valid url", id);
            }

            return String.Empty;
        }
    }
}
