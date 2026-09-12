namespace CosmosLearning.Vector.Api.Features.HybridSearch;

public static class ReciprocalRankFusion
{
    public static IReadOnlyList<HybridSearchResult> Fuse(
        IReadOnlyList<HybridSearchCandidate> vectorResults,
        IReadOnlyList<KeywordSearchResult> keywordResults,
        int top,
        int rankConstant = 60)
    {
        var vectorRanks =
            vectorResults
                .Select(
                    (document, index) =>
                        new
                        {
                            document.Id,
                            Rank = index + 1
                        })
                .ToDictionary(
                    x => x.Id,
                    x => x.Rank);

        var keywordRanks =
            keywordResults
                .Select(
                    (result, index) =>
                        new
                        {
                            result.Document.Id,
                            Rank = index + 1
                        })
                .ToDictionary(
                    x => x.Id,
                    x => x.Rank);

        var documents =
            vectorResults
                .Concat(
                    keywordResults.Select(
                        x => x.Document))
                .GroupBy(x => x.Id)
                .Select(x => x.First());

        var fused =
            new List<HybridSearchResult>();

        foreach (HybridSearchCandidate document in documents)
        {
            double score = 0;

            int? vectorRank = null;
            int? keywordRank = null;

            if (vectorRanks.TryGetValue(
                    document.Id,
                    out int vr))
            {
                vectorRank = vr;

                score +=
                    1.0 /
                    (rankConstant + vr);
            }

            if (keywordRanks.TryGetValue(
                    document.Id,
                    out int kr))
            {
                keywordRank = kr;

                score +=
                    1.0 /
                    (rankConstant + kr);
            }

            fused.Add(
                new HybridSearchResult
                {
                    Id = document.Id,
                    Name = document.Name,
                    Category = document.Category,
                    Price = document.Price,
                    Description = document.Description,
                    VectorRank = vectorRank,
                    KeywordRank = keywordRank,
                    RrfScore = score
                });
        }

        return fused
            .OrderByDescending(x => x.RrfScore)
            .Take(top)
            .ToList();
    }
}