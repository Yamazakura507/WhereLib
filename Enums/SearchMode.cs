namespace WhereLib.Enums;

/// <summary>
/// Text matching algorithm used by search rules.
/// </summary>
public enum SearchMode
{
    /// <summary>
    /// Exact or substring text matching.
    /// </summary>
    Contains,

    /// <summary>
    /// Fuzzy matching based on Levenshtein edit distance.
    /// </summary>
    Levenshtein
}
