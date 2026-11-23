using WhereLib.Enums;

namespace WhereLib.Models;

/// <summary>
/// One text-search condition. Its text is matched against a value selected by the consumer.
/// </summary>
/// <typeparam name="TParameter">Enum identifying the searchable property.</typeparam>
public class SearchRule<TParameter> : WhereRule<TParameter> where TParameter : struct, Enum
{
    private string? searchText;

    /// <summary>
    /// Gets or sets the text searched for by this rule.
    /// </summary>
    public string? SearchText
    {
        get => searchText;
        set => SetField(ref searchText, value);
    }

    /// <summary>
    /// Initializes an empty search rule for XAML object construction.
    /// </summary>
    public SearchRule() { }

    /// <summary>
    /// Initializes a search rule for a parameter, query text and optional operator.
    /// </summary>
    public SearchRule(TParameter whereParametr, string? searchText = null, LogicalOperator? @operator = null) : base(whereParametr, @operator)
    {
        this.searchText = searchText;
    }
}
