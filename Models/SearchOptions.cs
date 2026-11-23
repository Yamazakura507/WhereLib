using System.ComponentModel;
using System.Runtime.CompilerServices;
using WhereLib.Enums;

namespace WhereLib.Models;

/// <summary>
/// Mutable options shared by all search rules in one query.
/// </summary>
public sealed class SearchOptions : INotifyPropertyChanged
{
    private SearchMode mode = SearchMode.Contains;
    private bool isCaseSensitive;
    private bool isStrict;
    private int maxLevenshteinDistance = 2;

    /// <summary>
    /// Gets or sets the text matching algorithm.
    /// </summary>
    public SearchMode Mode
    {
        get => mode;
        set => SetField(ref mode, value);
    }

    /// <summary>
    /// Gets or sets whether text matching distinguishes letter case.
    /// </summary>
    public bool IsCaseSensitive
    {
        get => isCaseSensitive;
        set => SetField(ref isCaseSensitive, value);
    }

    /// <summary>
    /// Gets or sets whether a candidate must match the whole value.
    /// In Contains mode this selects equality instead of substring matching.
    /// In Levenshtein mode the edit distance is compared with the whole value instead of a substring.
    /// </summary>
    public bool IsStrict
    {
        get => isStrict;
        set => SetField(ref isStrict, value);
    }

    /// <summary>
    /// Gets or sets the maximum edit distance accepted by Levenshtein search.
    /// </summary>
    public int MaxLevenshteinDistance
    {
        get => maxLevenshteinDistance;
        set
        {
            ArgumentOutOfRangeException.ThrowIfNegative(value);
            SetField(ref maxLevenshteinDistance, value);
        }
    }

    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    private bool SetField<TValue>(ref TValue field, TValue value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<TValue>.Default.Equals(field, value)) return false;

        field = value;

        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        return true;
    }
}
