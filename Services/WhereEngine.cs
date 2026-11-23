using WhereLib.Enums;
using WhereLib.Models;

namespace WhereLib.Services;

/// <summary>
/// Applies typed filters and text searches to a source sequence without modifying it.
/// </summary>
public static class WhereEngine
{
    /// <summary>
    /// Filters items and then searches the surviving items. Filter conditions are combined with AND
    /// by default; search conditions are combined with OR by default. The operator on each rule joins
    /// that rule to the next rule, so the final rule's operator is intentionally ignored.
    /// Filter and search results are combined with AND.
    /// </summary>
    /// <typeparam name="TItem">The source item type.</typeparam>
    /// <typeparam name="TFilterParameter">Enum identifying filterable properties.</typeparam>
    /// <typeparam name="TSearchParameter">Enum identifying searchable properties.</typeparam>
    /// <typeparam name="TFilterRule">Concrete filter rule type used by the consumer.</typeparam>
    /// <typeparam name="TSearchRule">Concrete search rule type used by the consumer.</typeparam>
    /// <param name="source">Source items, normally already sorted.</param>
    /// <param name="filterRules">Optional ordered filter rules.</param>
    /// <param name="filterMatcher">Returns whether an item matches one filter rule.</param>
    /// <param name="searchRules">Optional ordered search rules.</param>
    /// <param name="searchValueSelector">Returns the text value for a search rule.</param>
    /// <param name="searchOptions">Options controlling all text matching.</param>
    /// <returns>Matching items in their original source order.</returns>
    public static IReadOnlyList<TItem> Apply<TItem, TFilterParameter, TSearchParameter, TFilterRule, TSearchRule>(
        this IEnumerable<TItem> source,
        IEnumerable<TFilterRule>? filterRules,
        Func<TItem, TFilterRule, bool> filterMatcher,
        IEnumerable<TSearchRule>? searchRules,
        Func<TItem, TSearchRule, string?> searchValueSelector,
        SearchOptions? searchOptions = null)
        where TFilterParameter : struct, Enum
        where TSearchParameter : struct, Enum
        where TFilterRule : FilterRule<TFilterParameter>
        where TSearchRule : SearchRule<TSearchParameter>
    {
        ArgumentNullException.ThrowIfNull(searchValueSelector);

        return source.ApplyMany<TItem, TFilterParameter, TSearchParameter, TFilterRule, TSearchRule>
            (filterRules,filterMatcher,searchRules,(item, rule) => [searchValueSelector(item, rule)], searchOptions);
    }

    /// <summary>
    /// Filters items and searches one or more text values for each search rule.
    /// A search rule matches when at least one selected text value satisfies its query.
    /// This supports items such as messages that own multiple searchable statuses or labels.
    /// Logical operator and filter behavior are identical to <see cref="Apply{TItem, TFilterParameter, TSearchParameter, TFilterRule, TSearchRule}"/>.
    /// </summary>
    /// <typeparam name="TItem">The source item type.</typeparam>
    /// <typeparam name="TFilterParameter">Enum identifying filterable properties.</typeparam>
    /// <typeparam name="TSearchParameter">Enum identifying searchable properties.</typeparam>
    /// <typeparam name="TFilterRule">Concrete filter rule type used by the consumer.</typeparam>
    /// <typeparam name="TSearchRule">Concrete search rule type used by the consumer.</typeparam>
    /// <param name="source">Source items, normally already sorted.</param>
    /// <param name="filterRules">Optional ordered filter rules.</param>
    /// <param name="filterMatcher">Returns whether an item matches one filter rule.</param>
    /// <param name="searchRules">Optional ordered search rules.</param>
    /// <param name="searchValuesSelector">Returns all text values checked by one search rule.</param>
    /// <param name="searchOptions">Options controlling all text matching.</param>
    /// <returns>Matching items in their original source order.</returns>
    public static IReadOnlyList<TItem> ApplyMany<TItem, TFilterParameter, TSearchParameter, TFilterRule, TSearchRule>(
        this IEnumerable<TItem> source,
        IEnumerable<TFilterRule>? filterRules,
        Func<TItem, TFilterRule, bool> filterMatcher,
        IEnumerable<TSearchRule>? searchRules,
        Func<TItem, TSearchRule, IEnumerable<string?>?> searchValuesSelector,
        SearchOptions? searchOptions = null)
        where TFilterParameter : struct, Enum
        where TSearchParameter : struct, Enum
        where TFilterRule : FilterRule<TFilterParameter>
        where TSearchRule : SearchRule<TSearchParameter>
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(filterMatcher);
        ArgumentNullException.ThrowIfNull(searchValuesSelector);

        TFilterRule[] filters = filterRules?.ToArray() ?? [];
        TSearchRule[] searches = searchRules?.Where(rule => !string.IsNullOrWhiteSpace(rule.SearchText)).ToArray() ?? [];

        SearchOptions options = searchOptions ?? new SearchOptions();

        return source.Where(item =>
        {
            bool passesFilters = filters.Evaluate<TFilterRule, TFilterParameter>(rule => filterMatcher(item, rule), LogicalOperator.And);

            if (!passesFilters) return false;

            return searches.Evaluate<TSearchRule, TSearchParameter>(
                rule => searchValuesSelector(item, rule)?.Any(candidate => options.IsMatch(candidate, rule.SearchText!)) == true,
                LogicalOperator.Or);
        }).ToList();
    }

    private static bool Evaluate<TRule, TParameter>(
        this IReadOnlyList<TRule> rules,
        Func<TRule, bool> matcher,
        LogicalOperator defaultOperator)
        where TRule : WhereRule<TParameter>
        where TParameter : struct, Enum
    {
        if (rules.Count == 0) return true;

        bool result = matcher(rules[0]);

        for (int index = 0; index < rules.Count - 1; index++)
        {
            bool next = matcher(rules[index + 1]);
            LogicalOperator @operator = rules[index].Operator ?? defaultOperator;

            result = @operator == LogicalOperator.And ? result && next : result || next;
        }

        return result;
    }

    /// <summary>
    /// Matches a candidate string against a query using the supplied search options.
    /// </summary>
    public static bool IsMatch(this SearchOptions options, string? candidate, string query)
    {
        ArgumentNullException.ThrowIfNull(query);
        ArgumentNullException.ThrowIfNull(options);

        if (candidate is null) return false;

        string value = options.IsCaseSensitive ? candidate : candidate.ToUpperInvariant();
        string search = options.IsCaseSensitive ? query : query.ToUpperInvariant();

        return options.Mode switch
        {
            SearchMode.Contains when options.IsStrict => string.Equals(value, search, StringComparison.Ordinal),
            SearchMode.Contains => value.Contains(search, StringComparison.Ordinal),
            SearchMode.Levenshtein => value.MatchesLevenshtein(search, options.MaxLevenshteinDistance, options.IsStrict),
            _ => throw new ArgumentOutOfRangeException(nameof(options.Mode), options.Mode, "Unsupported search mode.")
        };
    }

    private static bool MatchesLevenshtein(this string candidate, string query, int maxDistance, bool strict)
    {
        if (strict) return LevenshteinDistance(candidate, query, maxDistance) <= maxDistance;
        if (query.Length == 0) return true;

        int minimumLength = Math.Max(1, query.Length - maxDistance);
        int maximumLength = Math.Min(candidate.Length, query.Length + maxDistance);

        for (int start = 0; start < candidate.Length; start++)
        {
            for (int length = minimumLength; length <= maximumLength && start + length <= candidate.Length; length++)
            {
                if (Math.Abs(length - query.Length) > maxDistance) continue;

                string segment = candidate.Substring(start, length);

                if (LevenshteinDistance(segment, query, maxDistance) <= maxDistance) return true;
            }
        }

        return false;
    }

    private static int LevenshteinDistance(string left, string right, int limit)
    {
        if (Math.Abs(left.Length - right.Length) > limit) return limit + 1;
        if (left.Length > right.Length) (left, right) = (right, left);

        int[] previous = new int[left.Length + 1];
        int[] current = new int[left.Length + 1];

        for (int index = 0; index <= left.Length; index++) previous[index] = index;

        for (int rightIndex = 1; rightIndex <= right.Length; rightIndex++)
        {
            current[0] = rightIndex;
            int rowMinimum = current[0];

            for (int leftIndex = 1; leftIndex <= left.Length; leftIndex++)
            {
                int substitutionCost = left[leftIndex - 1] == right[rightIndex - 1] ? 0 : 1;
                current[leftIndex] = Math.Min(
                    Math.Min(current[leftIndex - 1] + 1, previous[leftIndex] + 1),
                    previous[leftIndex - 1] + substitutionCost);
                rowMinimum = Math.Min(rowMinimum, current[leftIndex]);
            }

            if (rowMinimum > limit) return limit + 1;

            (previous, current) = (current, previous);
        }

        return previous[left.Length];
    }
}
