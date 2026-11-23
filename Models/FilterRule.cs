using WhereLib.Enums;

namespace WhereLib.Models;

/// <summary>
/// One filtering condition. The meaning and expected type of Value are defined by the consumer's selector.
/// </summary>
/// <typeparam name="TParameter">Enum identifying the filterable property.</typeparam>
public class FilterRule<TParameter> : WhereRule<TParameter> where TParameter : struct, Enum
{
    private object? value;

    /// <summary>
    /// Gets or sets the value compared by this filtering rule.
    /// </summary>
    public object? Value
    {
        get => value;
        set => SetField(ref this.value, value);
    }

    /// <summary>
    /// Initializes an empty filtering rule for XAML object construction.
    /// </summary>
    public FilterRule() { }

    /// <summary>
    /// Initializes a filtering rule for a parameter, comparison value and optional operator.
    /// </summary>
    public FilterRule(TParameter whereParametr, object? value = null, LogicalOperator? @operator = null) : base(whereParametr, @operator)
    {
        this.value = value;
    }
}
