using System;
using System.Collections.Generic;

namespace Catalogue.Gemini.Model
{

    public class VocabularyKeyword : IComparable<VocabularyKeyword>
    {
        public VocabularyKeyword()
        {
        }

        public VocabularyKeyword(Guid Id, string Value)
        {
            this.Id = Id;
            this.Value = Value;
        }

        public Guid Id { get; set; }
        public string Value { get; set; }


        protected bool Equals(VocabularyKeyword other)
        {
            return string.Equals(Value, other.Value, StringComparison.InvariantCultureIgnoreCase) && other.Id == this.Id;
        }

        public int CompareTo(VocabularyKeyword other)
        {
            return System.String.Compare(this.Value, other.Value, System.StringComparison.Ordinal);
        }

        public override bool Equals(object obj)
        {

            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((VocabularyKeyword)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (this.Id + "::" + this.Value).GetHashCode() * 397;
            }
        }
    }

    public class Vocabulary
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string PublicationDate { get; set; }

        public bool Controlled { get; set; } // is this a controlled (limited) list or are new values allowed?
        public bool Publishable { get; set; } // is this a vocabulary of publishable keywords?
     
        public List<VocabularyKeyword> Keywords { get; set; }
    }
}
