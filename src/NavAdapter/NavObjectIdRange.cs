using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics.Contracts;

namespace Statical.NavAdapter;

/// <summary>
/// Defines a contiguous sequence of integer identifiers for NAV objects. The sequence has an optional
/// lower or upper bound (both must not be absent).
/// </summary>
public class NavObjectIdRange
{
    /// <summary>
    /// <para>Creates an integer object range specifying a series of object identifiers (without any specification
    /// of object type).</para>
    /// <para>Requires either a lower or upper limit - or both to be defined.<para>
    /// </summary>
    /// <param name="lower">The lower object id limit</param>
    /// <param name="upper">The upper object id limit</param>
    public NavObjectIdRange(int? lower, int? upper)
    {
        Contract.Requires(lower is not null || upper is not null);
        Contract.Requires(lower == null || upper == null || lower.Value <= upper.Value);


        if (!lower.HasValue && !upper.HasValue) // TODO necessary?
        {
            throw new ArgumentException("Either lower or upper must be not null");
        }
        if (lower.HasValue && upper.HasValue && lower.Value > upper.Value)
        {
            throw new ArgumentException("Lower must not be greater than upper");
        }
        Lower = lower;
        Upper = upper;
    }

    /// <summary>
    /// The lower limit. The lower limit is optional (nullable), as long as the upper limit is not also optional.
    /// </summary>
    public int? Lower { get; private set; }

    /// <summary>
    /// The upper limit. The upper limit is optional (nullable), as long as the lower limit is not also optional.
    /// </summary>
    public int? Upper { get; private set; }

    /// <summary>
    /// Returns a string representation of this filter for printing
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return NavFilterExpression();
    }

    /// <summary>
    /// Create an SQL where clause over the NAV object id range.
    /// </summary>
    /// <param name="idRanges">A sequence of object id ranges.</param>
    /// <returns></returns>
    public static string SqlWhereClause(ISet<NavObjectIdRange> idRanges)
    {
        return string.Join(" OR ", idRanges.Select(range => range.SqlWhereClause()));
    }

    private string SqlWhereClause()
    {
        if (Lower.HasValue && Upper.HasValue)
        {
            return "(" + Lower.Value + " <= ID AND ID <= " + Upper.Value + ")";
        }
        else if (Upper.HasValue)
        {
            return "ID <= " + Upper.Value;
        }
        else if (Lower.HasValue)
        {
            return Lower.Value + " <= ID";
        }
        else
        {
            throw new InvalidOperationException("Either lower or higher must be not null");
        }
    }

    /// <summary>
    /// Create a NAV filter expression over the NAV object id range.
    /// </summary>
    /// <param name="idRanges">A sequence of object id ranges.</param>
    /// <returns>An object filter expression</returns>
    public static string NavFilterExpression(ISet<NavObjectIdRange> idRanges)
    {
        return string.Join("|", idRanges.Select(range => range.NavFilterExpression()));
    }

    private string NavFilterExpression()
    {
        if (Lower.HasValue && Upper.HasValue)
        {
            return Lower.Value + ".." + Upper.Value;
        }
        else if (Upper.HasValue)
        {
            return ".." + Upper.Value;
        }
        else if (Lower.HasValue)
        {
            return Lower.Value + "..";
        }
        else
        {
            throw new InvalidOperationException("Either low or high must be not null");
        }
    }

}
