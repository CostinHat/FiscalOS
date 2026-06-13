using System.Collections.Generic;
using System.Linq;

namespace FiscalOS.LegalCore;

/// <summary>
/// A structural legal address: an ordered path of <see cref="ReferenceSegment"/>s
/// describing a location within a legal document (e.g. Article 47 -> Paragraph 3 -> Letter b).
/// It carries no legal content, interpretation, or fiscal conclusions; all operations are
/// pure structural prefix/path comparisons.
/// </summary>
public sealed class LegalReference : IEquatable<LegalReference>
{
    public IReadOnlyList<ReferenceSegment> Segments { get; }

    public LegalReference(IReadOnlyList<ReferenceSegment> segments)
    {
        ArgumentNullException.ThrowIfNull(segments);

        if (segments.Count == 0)
        {
            throw new ArgumentException("A legal reference must have at least one segment.", nameof(segments));
        }

        Segments = segments.ToArray();
    }

    /// <summary>The enclosing address (this address with its last segment removed), or null at the topmost segment.</summary>
    public LegalReference? Parent()
    {
        return Segments.Count <= 1
            ? null
            : new LegalReference(Segments.Take(Segments.Count - 1).ToArray());
    }

    /// <summary>True when this address is a strict prefix of <paramref name="other"/> (this strictly encloses other).</summary>
    public bool IsAncestorOf(LegalReference other)
    {
        ArgumentNullException.ThrowIfNull(other);

        return Segments.Count < other.Segments.Count && IsPrefixOf(other);
    }

    /// <summary>True when <paramref name="other"/> strictly encloses this address (the inverse of <see cref="IsAncestorOf"/>).</summary>
    public bool IsDescendantOf(LegalReference other)
    {
        ArgumentNullException.ThrowIfNull(other);

        return other.IsAncestorOf(this);
    }

    /// <summary>Inclusive containment: true when this address equals <paramref name="other"/> or is an ancestor of it.</summary>
    public bool Contains(LegalReference other)
    {
        ArgumentNullException.ThrowIfNull(other);

        return Equals(other) || IsAncestorOf(other);
    }

    private bool IsPrefixOf(LegalReference other)
    {
        for (var i = 0; i < Segments.Count; i++)
        {
            if (!Segments[i].Equals(other.Segments[i]))
            {
                return false;
            }
        }

        return true;
    }

    public bool Equals(LegalReference? other)
    {
        return other is not null && Segments.SequenceEqual(other.Segments);
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as LegalReference);
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (var segment in Segments)
        {
            hash.Add(segment);
        }

        return hash.ToHashCode();
    }

    public override string ToString()
    {
        return string.Join(" / ", Segments.Select(segment => segment.ToString()));
    }
}
