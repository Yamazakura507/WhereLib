using System.ComponentModel;
using System.Runtime.CompilerServices;
using WhereLib.Enums;

namespace WhereLib.Models;

/// <summary>
/// Base for one typed filtering or search condition.
/// Its Operator joins this condition to the next condition in the same list.
/// </summary>
/// <typeparam name="TParameter">Enum identifying the property evaluated by the rule.</typeparam>
public abstract class WhereRule<TParameter> : INotifyPropertyChanged where TParameter : struct, Enum
{
    private TParameter whereParametr;
    private LogicalOperator? @operator;

    /// <summary>
    /// Gets or sets the enum value identifying the property evaluated by this rule.
    /// </summary>
    public TParameter WhereParametr
    {
        get => whereParametr;
        set => SetField(ref whereParametr, value);
    }

    /// <summary>
    /// Gets or sets the operator that combines this rule with the next rule.
    /// Null selects the engine's default operator. The final rule's operator is ignored.
    /// </summary>
    public LogicalOperator? Operator
    {
        get => @operator;
        set => SetField(ref @operator, value);
    }

    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Initializes an empty rule for XAML object construction.
    /// </summary>
    protected WhereRule() { }

    /// <summary>
    /// Initializes a rule for the supplied parameter and optional logical operator.
    /// </summary>
    protected WhereRule(TParameter whereParametr, LogicalOperator? @operator = null)
    {
        this.whereParametr = whereParametr;
        this.@operator = @operator;
    }

    /// <summary>
    /// Raises property changed notifications when a rule value changes.
    /// </summary>
    protected bool SetField<TValue>(ref TValue field, TValue value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<TValue>.Default.Equals(field, value)) return false;

        field = value;

        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        return true;
    }
}
