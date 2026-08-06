using UnityEngine;

public static class StarRatingCalculator
{
    /// <summary>
    /// Converts an overall score percent (0-100) into a star count (0-5),
    /// based on ascending thresholds. E.g. thresholds [30,50,65,80,95]:
    /// score >= 30 -> 1 star, >= 50 -> 2 stars, ... >= 95 -> 5 stars.
    /// </summary>
    public static int ComputeStars(float overallScorePercent, float[] thresholds)
    {
        if (thresholds == null || thresholds.Length == 0) return 0;

        int stars = 0;
        for (int i = 0; i < thresholds.Length; i++)
        {
            if (overallScorePercent >= thresholds[i])
                stars = i + 1;
        }
        return Mathf.Clamp(stars, 0, 5);
    }
}