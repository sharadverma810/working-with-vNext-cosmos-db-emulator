using System.Text.RegularExpressions;

namespace CosmosLearning.Vector.Api.Features.HybridSearch;

public sealed class KeywordSearchService
{
    private static readonly HashSet<string> StopWords =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "a",
            "an",
            "and",
            "are",
            "at",
            "be",
            "by",
            "for",
            "from",
            "i",
            "in",
            "is",
            "me",
            "my",
            "of",
            "on",
            "or",
            "someone",
            "that",
            "the",
            "this",
            "to",
            "with"
        };

    /*
     * Tokenizer supports:
     *
     * Normal words:
     *   keyboard
     *   gaming
     *   competitive
     *
     * Compound product terms:
     *   anti-ghosting
     *   low-latency
     *   noise-cancelling
     *
     * Technical specifications:
     *   8000Hz
     *   0.5ms
     *   2.4GHz
     *   240Hz
     *   1TB
     *   32GB
     *   26000DPI
     *   4K
     */
    private static readonly Regex TokenRegex =
        new(
            @"(?:\d+(?:\.\d+)?(?:hz|khz|mhz|ghz|ms|s|gb|tb|mb|kb|dpi|fps|mp|inch|inches|mm|cm|kg|g)|[a-z0-9]+(?:[-_][a-z0-9]+)*)",
            RegexOptions.Compiled |
            RegexOptions.IgnoreCase);

    public IReadOnlyList<KeywordSearchResult> Search(
        string query,
        IReadOnlyList<HybridSearchCandidate> documents)
    {
        string[] queryTerms =
            Tokenize(query);

        if (queryTerms.Length == 0 ||
            documents.Count == 0)
        {
            return [];
        }

        var tokenMap =
            documents.ToDictionary(
                document => document.Id,
                document => Tokenize(document.SearchText));

        double averageDocumentLength =
            tokenMap.Values
                .Average(tokens => tokens.Length);

        var documentFrequency =
            new Dictionary<string, int>(
                StringComparer.OrdinalIgnoreCase);

        /*
         * Calculate how many documents contain each term.
         */
        foreach (string[] tokens in tokenMap.Values)
        {
            foreach (string term in tokens.Distinct(
                         StringComparer.OrdinalIgnoreCase))
            {
                documentFrequency[term] =
                    documentFrequency.GetValueOrDefault(term) + 1;
            }
        }

        var scored =
            new List<KeywordSearchResult>();

        foreach (HybridSearchCandidate document in documents)
        {
            string[] tokens =
                tokenMap[document.Id];

            double score =
                CalculateBm25(
                    queryTerms,
                    tokens,
                    documentFrequency,
                    documents.Count,
                    averageDocumentLength);

            if (score > 0)
            {
                scored.Add(
                    new KeywordSearchResult
                    {
                        Document = document,
                        Score = score
                    });
            }
        }

        return scored
            .OrderByDescending(x => x.Score)
            .ToList();
    }

    private static double CalculateBm25(
        IReadOnlyList<string> queryTerms,
        IReadOnlyList<string> documentTerms,
        IReadOnlyDictionary<string, int> documentFrequency,
        int documentCount,
        double averageDocumentLength)
    {
        const double k1 = 1.2;
        const double b = 0.75;

        if (documentTerms.Count == 0 ||
            averageDocumentLength <= 0)
        {
            return 0;
        }

        var termFrequency =
            documentTerms
                .GroupBy(
                    x => x,
                    StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    g => g.Key,
                    g => g.Count(),
                    StringComparer.OrdinalIgnoreCase);

        double score = 0;

        foreach (string term in queryTerms.Distinct(
                     StringComparer.OrdinalIgnoreCase))
        {
            if (!termFrequency.TryGetValue(
                    term,
                    out int frequency))
            {
                continue;
            }

            int df =
                documentFrequency.GetValueOrDefault(term);

            if (df == 0)
            {
                continue;
            }

            /*
             * Standard BM25 inverse document frequency.
             *
             * Rare terms receive more weight.
             *
             * Example:
             *
             * "keyboard"
             *     appears in many documents
             *     -> lower IDF
             *
             * "8000hz"
             *     appears in fewer documents
             *     -> higher IDF
             */
            double idf =
                Math.Log(
                    1 +
                    ((documentCount - df + 0.5) /
                     (df + 0.5)));

            double denominator =
                frequency +
                k1 *
                (
                    1 -
                    b +
                    b *
                    (
                        documentTerms.Count /
                        averageDocumentLength
                    )
                );

            double termScore =
                idf *
                (
                    (frequency * (k1 + 1)) /
                    denominator
                );

            score += termScore;
        }

        return score;
    }

    private static string[] Tokenize(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return [];
        }

        return TokenRegex
            .Matches(text.ToLowerInvariant())
            .Select(match => match.Value)
            .Where(term => !StopWords.Contains(term))
            .ToArray();
    }
}

public sealed class KeywordSearchResult
{
    public HybridSearchCandidate Document { get; init; } = null!;

    public double Score { get; init; }
}