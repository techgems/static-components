using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace TechGems.StaticComponents.Headless;

/// <summary>
/// An immutable, read-only list of the HTML attributes a consumer placed on a headless input component
/// that were not bound to one of its C# properties. Pass it to an element inside the component's
/// Razor template via the <c>static-attributes</c> tag helper to apply them to the rendered markup.
/// </summary>
/// <remarks>
/// The filtering methods (<see cref="Only"/>, <see cref="Except"/>, <see cref="WithPrefix"/> and
/// <see cref="WithoutPrefix"/>) each return a new collection, so they can be chained to route
/// different attributes to different elements in the same template.
/// </remarks>
public sealed class StaticAttributeCollection : IReadOnlyList<TagHelperAttribute>
{
    /// <summary>
    /// Attribute names that are never captured because a <c>static-*</c> tag helper owns them.
    /// </summary>
    private static readonly HashSet<string> ReservedNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "type"
    };

    private readonly IReadOnlyList<TagHelperAttribute> _attributes;

    /// <summary>
    /// An empty collection. Applying it via <c>static-attributes</c> is a no-op.
    /// </summary>
    public static StaticAttributeCollection Empty { get; } = new(Array.Empty<TagHelperAttribute>());

    /// <summary>
    /// Creates an empty collection.
    /// </summary>
    public StaticAttributeCollection() : this(Array.Empty<TagHelperAttribute>())
    {
    }

    private StaticAttributeCollection(IReadOnlyList<TagHelperAttribute> attributes)
    {
        _attributes = attributes;
    }

    /// <summary>
    /// Builds a collection from a tag helper's output attributes, which at the start of
    /// <c>ProcessAsync</c> contain exactly the attributes that were not bound to a C# property.
    /// Reserved names are dropped.
    /// </summary>
    /// <param name="attributes">The attribute list to capture from.</param>
    /// <returns>A new collection holding the capturable attributes, in source order.</returns>
    internal static StaticAttributeCollection Capture(TagHelperAttributeList attributes)
    {
        if (attributes is null || attributes.Count == 0)
        {
            return Empty;
        }

        var captured = attributes.Where(x => !ReservedNames.Contains(x.Name)).ToArray();

        return captured.Length == 0 ? Empty : new StaticAttributeCollection(captured);
    }

    /// <summary>
    /// The number of attributes in the collection.
    /// </summary>
    public int Count => _attributes.Count;

    /// <summary>
    /// Gets the attribute at the given position.
    /// </summary>
    /// <param name="index">The zero-based position.</param>
    public TagHelperAttribute this[int index] => _attributes[index];

    /// <summary>
    /// Gets the attribute with the given name, or <c>null</c> when it is not present.
    /// Name comparison is case-insensitive.
    /// </summary>
    /// <param name="name">The attribute name to look for.</param>
    public TagHelperAttribute? this[string name] =>
        _attributes.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Returns true when an attribute with the given name is present. Name comparison is case-insensitive.
    /// </summary>
    /// <param name="name">The attribute name to look for.</param>
    public bool Contains(string name) => this[name] is not null;

    /// <summary>
    /// Returns a new collection containing only the attributes whose names match one of <paramref name="names"/>.
    /// </summary>
    /// <param name="names">The attribute names to keep.</param>
    public StaticAttributeCollection Only(params string[] names)
    {
        return Filter(x => MatchesAnyName(x, names));
    }

    /// <summary>
    /// Returns a new collection with the attributes whose names match one of <paramref name="names"/> removed.
    /// </summary>
    /// <param name="names">The attribute names to drop.</param>
    public StaticAttributeCollection Except(params string[] names)
    {
        return Filter(x => !MatchesAnyName(x, names));
    }

    /// <summary>
    /// Returns a new collection containing only the attributes whose names start with one of
    /// <paramref name="prefixes"/>. Useful for routing framework-specific attributes such as
    /// <c>x-</c> (Alpine) or <c>hx-</c> (HTMX) to a wrapper element.
    /// </summary>
    /// <param name="prefixes">The name prefixes to keep.</param>
    public StaticAttributeCollection WithPrefix(params string[] prefixes)
    {
        return Filter(x => MatchesAnyPrefix(x, prefixes));
    }

    /// <summary>
    /// Returns a new collection with the attributes whose names start with one of
    /// <paramref name="prefixes"/> removed.
    /// </summary>
    /// <param name="prefixes">The name prefixes to drop.</param>
    public StaticAttributeCollection WithoutPrefix(params string[] prefixes)
    {
        return Filter(x => !MatchesAnyPrefix(x, prefixes));
    }

    /// <inheritdoc />
    public IEnumerator<TagHelperAttribute> GetEnumerator() => _attributes.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private StaticAttributeCollection Filter(Func<TagHelperAttribute, bool> predicate)
    {
        if (Count == 0)
        {
            return Empty;
        }

        var filtered = _attributes.Where(predicate).ToArray();

        return filtered.Length == 0 ? Empty : new StaticAttributeCollection(filtered);
    }

    private static bool MatchesAnyName(TagHelperAttribute attribute, string[]? names)
    {
        if (names is null || names.Length == 0)
        {
            return false;
        }

        return names.Any(x => string.Equals(attribute.Name, x, StringComparison.OrdinalIgnoreCase));
    }

    private static bool MatchesAnyPrefix(TagHelperAttribute attribute, string[]? prefixes)
    {
        if (prefixes is null || prefixes.Length == 0)
        {
            return false;
        }

        return prefixes.Any(x => !string.IsNullOrEmpty(x)
            && attribute.Name.StartsWith(x, StringComparison.OrdinalIgnoreCase));
    }
}
