namespace WhereLib.Enums;

/// <summary>
/// Logical operator used to combine an item condition with the next condition in a rule list.
/// </summary>
public enum LogicalOperator
{
    /// <summary>
    /// Both the accumulated result and the next condition must match.
    /// </summary>
    And,

    /// <summary>
    /// Either the accumulated result or the next condition must match.
    /// </summary>
    Or
}
