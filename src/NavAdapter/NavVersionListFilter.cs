using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using System.Data;
using System.Diagnostics.Contracts;

namespace Statical.NavAdapter;

/// <summary>
/// Defines a text pattern to be compared to NAV's version list.
/// </summary>
public class NavVersionListFilter
{
    /// <summary>
    /// <para>Creates an NavVersionListFilter with a filter/pattern.</para>
    /// <para>Requires either a lower or upper limit - or both to be defined.<para>
    /// </summary>
    /// <param name="filter">The text to match against version list, may include wildcards (*)</param>
    public NavVersionListFilter(string filter)
    {
        Contract.Requires(! String.IsNullOrWhiteSpace(filter));
        Filter = filter;
    }

    /// <summary>
    /// The filter. Required.
    /// </summary>
    public string Filter { get; private set; }
    
    /// <summary>
    /// Returns a string representation of this filter for printing
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return Filter;
    }

    /// <summary>
    /// Create an SQL where clause corresponding to a set of filters.
    /// </summary>
    /// <param name="versionExclusions">The set of filters to be excluded</param>
    /// <returns></returns>
    public static string SqlWhereClause(ISet<NavVersionListFilter> versionExclusions)
    {
        var conditions = new HashSet<string>();
        for (int i = 0; i < versionExclusions.Count; i++)
        {
            var condition = "[Version List] NOT LIKE " + SqlParameterName(i);
            conditions.Add(condition);
        }
        return string.Join(" AND ", conditions);
    }

    /// <summary>
    /// Enriches a SQL command with parameters for filtering.
    /// </summary>
    /// <param name="command">The command to be enriched with parameters</param>
    /// <param name="versionExclusions">The set of filters to be excluded.</param>
    public static void AddSqlParameters(SqlCommand command, ISet<NavVersionListFilter> versionExclusions)
    {
        for (int i = 0; i < versionExclusions.Count; i++)
        {
            var param = new SqlParameter(SqlParameterName(i), SqlDbType.VarChar, 250)
            {
                Value = versionExclusions.ElementAt(i).Filter.Replace('*', '%')
            };
            command.Parameters.Add(param);
        }
    }

    /// <summary>
    /// Returns the name of the i'th version list parameter.
    /// </summary>
    /// <param name="i">Index of entry in exclusions.</param>
    /// <returns>The name of the i'th version list parameter</returns>
    private static string SqlParameterName(int i) => "@versionFilter" + i.ToString();

    /// <summary>
    /// Create a NAV filter expression for a set of exclusions.
    /// </summary>
    /// <param name="versionExclusions">A set of version patterns to be excluded</param>
    /// <returns>An NAV object filter expression</returns>
    public static string NavFilterExpression(ISet<NavVersionListFilter> versionExclusions)
    {
        return string.Join("&", versionExclusions.Select(filter => "<>" + filter));
    }
}
