# WhereLib

A UI- and model-independent .NET library for composable filtering and text search.

## Rules

- `FilterRule<TParameter>` holds a typed filter parameter, its expected `Value` and an optional `LogicalOperator`.
- `SearchRule<TParameter>` holds a typed searchable parameter, `SearchText` and an optional `LogicalOperator`.
- `SearchOptions` controls the matching algorithm and its behavior.
- `WhereEngine.Apply(...)` applies filtering first, then search, without mutating the source sequence.

The operator belongs to the rule it follows: it combines that rule's result with the next rule. The final rule's operator is ignored. Missing operators default to `And` for filter rules and `Or` for search rules. The combined filter result and search result are joined with `And`. Each rule list is evaluated from left to right.

## Search modes

- `SearchMode.Contains`: substring matching by default; with `IsStrict=true`, the entire candidate must equal the query.
- `SearchMode.Levenshtein`: fuzzy edit-distance matching. By default, the query may match a substring; with `IsStrict=true`, the whole candidate is compared.
- `IsCaseSensitive` controls case handling.
- `MaxLevenshteinDistance` controls the accepted edit distance and defaults to 2.

All `SearchOptions` properties are observable, so a UI can change search behavior while the query is active.

## Example

~~~csharp
var filters = new[]
{
    new FilterRule<TicketFilter>(TicketFilter.IsOpen, true, LogicalOperator.And),
    new FilterRule<TicketFilter>(TicketFilter.HasOwner, true)
};

var searches = new[]
{
    new SearchRule<TicketSearch>(TicketSearch.Title, "network", LogicalOperator.Or),
    new SearchRule<TicketSearch>(TicketSearch.Description, "timeout")
};

var options = new SearchOptions
{
    Mode = SearchMode.Levenshtein,
    IsCaseSensitive = false,
    IsStrict = false,
    MaxLevenshteinDistance = 2
};

IReadOnlyList<Ticket> result = WhereEngine.Apply<Ticket, TicketFilter, TicketSearch,
    FilterRule<TicketFilter>, SearchRule<TicketSearch>>(
        tickets,
        filters,
        (ticket, rule) => EvaluateFilter(ticket, rule),
        searches,
        (ticket, rule) => SelectSearchText(ticket, rule),
        options);
~~~
