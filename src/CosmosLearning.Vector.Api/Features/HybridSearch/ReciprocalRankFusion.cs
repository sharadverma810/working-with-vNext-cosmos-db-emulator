namespace CosmosLearning.Vector.Api.Features.HybridSearch;

public static class ReciprocalRankFusion
{
    public const int DefaultRankConstant = 60;

    public static IReadOnlyList<HybridSearchResult> Fuse(
        IReadOnlyList<HybridSearchCandidate> vectorResults,
        IReadOnlyList<KeywordSearchResult> keywordResults,
        int top,
        double vectorWeight = 1.0,
        double keywordWeight = 1.0,
        int rankConstant = DefaultRankConstant)
    {
        var scores =
            new Dictionary<string, FusionScore>(
                StringComparer.OrdinalIgnoreCase);

        // ------------------------------------------------------------
        // Vector results
        // ------------------------------------------------------------

        for (int i = 0; i < vectorResults.Count; i++)
        {
            var document = vectorResults[i];

            int rank = i + 1;

            if (!scores.TryGetValue(
                    document.Id,
                    out var score))
            {
                score = new FusionScore(document);

                scores[document.Id] = score;
            }

            score.VectorRank = rank;

            score.VectorContribution =
                vectorWeight /
                (rankConstant + rank);

            score.RrfScore +=
                score.VectorContribution;
        }

        // ------------------------------------------------------------
        // Keyword results
        // ------------------------------------------------------------

        for (int i = 0; i < keywordResults.Count; i++)
        {
            var keywordResult = keywordResults[i];

            int rank = i + 1;

            var document =
                keywordResult.Document;

            if (!scores.TryGetValue(
                    document.Id,
                    out var score))
            {
                score = new FusionScore(document);

                scores[document.Id] = score;
            }

            score.KeywordRank = rank;

            score.KeywordContribution =
                keywordWeight /
                (rankConstant + rank);

            score.RrfScore +=
                score.KeywordContribution;
        }

        // ------------------------------------------------------------
        // Final ranking
        // ------------------------------------------------------------

        return scores.Values
            .OrderByDescending(x => x.RrfScore)
            .Take(top)
            .Select(
                x => new HybridSearchResult
                {
                    Id = x.Document.Id,
                    Name = x.Document.Name,
                    Category = x.Document.Category,
                    Price = x.Document.Price,
                    Description = x.Document.Description,

                    VectorRank = x.VectorRank,
                    KeywordRank = x.KeywordRank,

                    VectorContribution =
                        x.VectorContribution,

                    KeywordContribution =
                        x.KeywordContribution,

                    RrfScore =
                        x.RrfScore
                })
            .ToList();
    }

    private sealed class FusionScore
    {
        public FusionScore(
            HybridSearchCandidate document)
        {
            Document = document;
        }

        public HybridSearchCandidate Document { get; }

        public int? VectorRank { get; set; }

        public int? KeywordRank { get; set; }

        public double VectorContribution { get; set; }

        public double KeywordContribution { get; set; }

        public double RrfScore { get; set; }
    }
}