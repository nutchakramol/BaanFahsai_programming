// ===================================================
// FILE: ScoringEngine.cs
// The core scoring algorithm. Pure function-style: (state) -> score.
// No namespace — kept consistent with the rest of the project for now.
// ===================================================
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class ScoringEngine
{
    /// <summary>
    /// Computes a normalized [0,1] score for a single placed item against
    /// its schema's heat zones. Takes the BEST matching zone (multiple
    /// ideal spots = take max, not sum, so item isn't penalized for
    /// only being near one of several equally-valid spots).
    /// </summary>
    public static float ComputeProximityScore(PlacedItem item, ItemSchemaSO schema)
    {
        if (schema.heatZones == null || schema.heatZones.Count == 0)
            return 1f; // no spatial requirement = always "correct" spatially

        float bestScore = 0f;
        foreach (var zone in schema.heatZones)
        {
            float distance = Vector2.Distance(item.WorldPosition, zone.worldPosition);
            float zoneScore = EvaluateZoneFalloff(distance, zone);

            Debug.Log($"[Proximity] Item at {item.WorldPosition}, zone '{zone.zoneLabel}' at {zone.worldPosition}, distance: {distance}, zoneScore: {zoneScore}");

            bestScore = Mathf.Max(bestScore, zoneScore * zone.weight);
        }
        return Mathf.Clamp01(bestScore);
    }

    /// <summary>
    /// Continuous radii-based falloff: full score inside innerRadius,
    /// decays via curve between inner/outer, zero beyond outerRadius.
    /// Falls back to a simple linear fade if the curve has no keyframes
    /// (empty AnimationCurve evaluates to 0 everywhere, which silently
    /// broke proximity scoring — this guards against that).
    /// </summary>
    private static float EvaluateZoneFalloff(float distance, HeatZoneDefinition zone)
    {
        if (distance <= zone.innerRadius) return 1f;
        if (distance >= zone.outerRadius) return 0f;

        float t = (distance - zone.innerRadius) / (zone.outerRadius - zone.innerRadius);

        if (zone.falloffCurve != null && zone.falloffCurve.length > 0)
            return zone.falloffCurve.Evaluate(t);

        return 1f - t; // linear fade fallback
    }

    /// <summary>
    /// Functional appropriateness score — is this item in a room
    /// tagged suitable for it?
    /// </summary>
    public static float ComputeAppropriatenessScore(RoomTag currentRoomTag, ItemSchemaSO schema)
    {
        if (schema.appropriateRoomTags == null || schema.appropriateRoomTags.Count == 0)
            return 1f; // no restriction

        return schema.appropriateRoomTags.Contains(currentRoomTag) ? 1f : 0.2f;
        // 0.2 floor, not 0 — avoids punishing exploratory placement too harshly.
    }

    /// <summary>
    /// Final composite score for one item. Weighted blend — tune weights
    /// per design pass without touching call sites.
    /// </summary>
    public static float ComputeItemScore(
        PlacedItem item,
        ItemSchemaSO schema,
        RoomTag roomTag,
        float proximityWeight = 0.7f,
        float appropriatenessWeight = 0.3f)
    {
        float proximity = ComputeProximityScore(item, schema);
        float appropriateness = ComputeAppropriatenessScore(roomTag, schema);

        return proximity * proximityWeight + appropriateness * appropriatenessWeight;
    }

    /// <summary>
    /// Aggregates all placed items against level requirements.
    /// Returns a per-requirement breakdown + overall normalized score.
    /// </summary>
    public static LevelScoreResult ComputeLevelScore(
        List<PlacedItem> placedItems,
        Dictionary<string, ItemSchemaSO> schemaLookup,
        Dictionary<string, RoomTag> roomTagLookup,
        List<LevelRequirement> requirements)
    {
        var result = new LevelScoreResult();
        foreach (var req in requirements)
        {
            var matchingItems = placedItems
                .Where(p => schemaLookup[p.ItemSchemaId].category == req.requiredCategory)
                .ToList();
            var scores = matchingItems.Select(p =>
                ComputeItemScore(p, schemaLookup[p.ItemSchemaId], roomTagLookup[p.CurrentRoomId])
            ).ToList();
            float avgScore = scores.Count > 0 ? scores.Average() : 0f;
            bool countMet = matchingItems.Count >= req.minCount;
            bool scoreMet = avgScore >= req.minAvgScore;
            result.RequirementResults.Add(new RequirementResult
            {
                RequirementId = req.requirementId,
                Satisfied = countMet && scoreMet,
                AverageScore = avgScore,
                ItemsPlaced = matchingItems.Count
            });
        }

        foreach (var reqResult in result.RequirementResults)
        {
            Debug.Log($"[ScoringEngine] Requirement: {reqResult.RequirementId}, ItemsPlaced: {reqResult.ItemsPlaced}, AvgScore: {reqResult.AverageScore}, Satisfied: {reqResult.Satisfied}");
        }

        var attemptedRequirements = result.RequirementResults
            .Where(r => r.ItemsPlaced > 0)
            .ToList();

        float qualityScore = attemptedRequirements.Count > 0
            ? attemptedRequirements.Average(r => r.AverageScore)
            : 0f;

        int totalRequirements = result.RequirementResults.Count;
        int satisfiedRequirements = result.RequirementResults.Count(r => r.Satisfied);

        float completionRatio = totalRequirements > 0
            ? (float)satisfiedRequirements / totalRequirements
            : 0f;

        result.OverallScorePercent = qualityScore * completionRatio * 100f;
        Debug.Log($"[ScoringEngine] Attempted requirements: {attemptedRequirements.Count}, Overall: {result.OverallScorePercent}%");

        return result;
    }
}

[System.Serializable]
public class LevelScoreResult
{
    public List<RequirementResult> RequirementResults = new List<RequirementResult>();
    public float OverallScorePercent;
}

[System.Serializable]
public class RequirementResult
{
    public string RequirementId;
    public bool Satisfied;
    public float AverageScore;
    public int ItemsPlaced;
}