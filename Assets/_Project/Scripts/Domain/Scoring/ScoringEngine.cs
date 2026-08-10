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
            bestScore = Mathf.Max(bestScore, zoneScore * zone.weight);
        }

        return Mathf.Clamp01(bestScore);
    }

    /// <summary>
    /// Continuous radii-based falloff: full score inside innerRadius,
    /// decays via curve between inner/outer, zero beyond outerRadius.
    /// </summary>
    private static float EvaluateZoneFalloff(float distance, HeatZoneDefinition zone)
    {
        if (distance <= zone.innerRadius) return 1f;
        if (distance >= zone.outerRadius) return 0f;

        float t = (distance - zone.innerRadius) / (zone.outerRadius - zone.innerRadius);
        return zone.falloffCurve.Evaluate(t);
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

        result.OverallScorePercent = result.RequirementResults.Count > 0
            ? result.RequirementResults.Average(r => r.AverageScore) * 100f
            : 0f;

        return result;
    }
}

public class LevelScoreResult
{
    public List<RequirementResult> RequirementResults = new List<RequirementResult>();
    public float OverallScorePercent;
}

public class RequirementResult
{
    public string RequirementId;
    public bool Satisfied;
    public float AverageScore;
    public int ItemsPlaced;
}