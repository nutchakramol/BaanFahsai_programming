using UnityEngine;

public static class StarRatingCalculator
{
    private const float PassThreshold = 50f;
    private const float OneStarThreshold = 51f;
    private const float TwoStarThreshold = 75f;
    private const float ThreeStarThreshold = 90f;

    /// <summary>
    /// Converts an overall score percent (0-100) into a star count (0-3),
    /// using fixed thresholds: 51-74% = 1 star, 75-89% = 2 stars, 90-100% = 3 stars.
    /// Below 51% still returns 0 stars (display layer decides how to color them).
    /// </summary>
    public static int ComputeStars(float overallScorePercent)
    {
        if (overallScorePercent >= ThreeStarThreshold) return 3;
        if (overallScorePercent >= TwoStarThreshold) return 2;
        if (overallScorePercent >= OneStarThreshold) return 1;
        return 0;
    }

    /// <summary>True if the score meets the 50% pass threshold.</summary>
    public static bool HasPassed(float overallScorePercent)
    {
        return overallScorePercent > PassThreshold;
    }
}