using System;
using System.IO;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Analysis.Tokenattributes;
using Raven.Database.Indexing;
using Version = Lucene.Net.Util.Version;

namespace Catalogue.Data.Analyzers
{
    // taken from the RavenDB tests project!
    // needed to change the default max to 10 to get sensible instant results (see below)

    public class NGramTokenFilter : TokenFilter
    {
        public static int DEFAULT_MIN_NGRAM_SIZE = 1;
        public static int DEFAULT_MAX_NGRAM_SIZE = 2;

        private int minGram, maxGram;

        private char[] curTermBuffer;
        private int curTermLength;
        private int curGramSize;
        private int curPos;
        private int tokStart;

        private TermAttribute termAtt;
        private OffsetAttribute offsetAtt;

        /**
         * Creates NGramTokenFilter with given min and max n-grams.
         * <param name="input"><see cref="TokenStream"/> holding the input to be tokenized</param>
         * <param name="minGram">the smallest n-gram to generate</param>
         * <param name="maxGram">the largest n-gram to generate</param>
         */
        public NGramTokenFilter(TokenStream input, int minGram, int maxGram)
            : base(input)
        {

            if (minGram < 1)
            {
                throw new System.ArgumentException("minGram must be greater than zero");
            }
            if (minGram > maxGram)
            {
                throw new System.ArgumentException("minGram must not be greater than maxGram");
            }
            this.minGram = minGram;
            this.maxGram = maxGram;

            this.termAtt = (TermAttribute)AddAttribute<ITermAttribute>();
            this.offsetAtt = (OffsetAttribute)AddAttribute<IOffsetAttribute>();
        }

        /**
         * Creates NGramTokenFilter with default min and max n-grams.
         * <param name="input"><see cref="TokenStream"/> holding the input to be tokenized</param>
         */
        public NGramTokenFilter(TokenStream input)
            : this(input, DEFAULT_MIN_NGRAM_SIZE, DEFAULT_MAX_NGRAM_SIZE)
        {

        }

        /** Returns the next token in the stream, or null at EOS. */
        public override bool IncrementToken()
        {
            while (true)
            {
                if (curTermBuffer == null)
                {
                    if (!input.IncrementToken())
                    {
                        return false;
                    }
                    else
                    {
                        curTermBuffer = (char[])termAtt.TermBuffer().Clone();
                        curTermLength = termAtt.TermLength();
                        curGramSize = minGram;
                        curPos = 0;
                        tokStart = offsetAtt.StartOffset;
                    }
                }
                while (curGramSize <= maxGram)
                {
                    while (curPos + curGramSize <= curTermLength)
                    {     // while there is input
                        ClearAttributes();
                        termAtt.SetTermBuffer(curTermBuffer, curPos, curGramSize);
                        offsetAtt.SetOffset(tokStart + curPos, tokStart + curPos + curGramSize);
                        curPos++;
                        return true;
                    }
                    curGramSize++;                         // increase n-gram size
                    curPos = 0;
                }
                curTermBuffer = null;
            }
        }

        public override void Reset()
        {
            base.Reset();
            curTermBuffer = null;
        }
    }

    [NotForQuerying]
    public class NGramAnalyzer : Analyzer
    {
        public override TokenStream TokenStream(string fieldName, TextReader reader)
        {
            var tokenizer = new StandardTokenizer(Version.LUCENE_30, reader);
            tokenizer.MaxTokenLength = 255;
            TokenStream filter = new StandardFilter(tokenizer);
            filter = new LowerCaseFilter(filter);
            filter = new StopFilter(false, filter, StandardAnalyzer.STOP_WORDS_SET);
            filter = new NGramTokenFilter(filter, 2, 10); // needed to change the default max to 10 to get sensible instant results

            return filter;
        }
    }

    public class StemAnalyzer : Analyzer
    {
        public override TokenStream TokenStream(String fieldName, TextReader reader)
        {
            TokenStream filter = new StandardFilter(new LowerCaseTokenizer(reader));
            filter = new PorterStemFilter(filter);

            return filter;
        }
    }

    [NotForQuerying]
    public class CustomKeywordAnalyzer : Analyzer
    {
        public override TokenStream TokenStream(string fieldName, TextReader reader)
        {
            var tokenizer = new StandardTokenizer(Version.LUCENE_30, reader);
            tokenizer.MaxTokenLength = 255;
            TokenStream filter = new StandardFilter(tokenizer);
            filter = new LowerCaseFilter(filter);
            filter = new NGramTokenFilter(filter, 2, 30);

            return filter;
        }
    }

}
