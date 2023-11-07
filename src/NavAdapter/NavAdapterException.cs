using System;
using System.Diagnostics.Contracts;

namespace Statical.NavAdapter;

/// <summary>
/// Exception thrown when a NAV adapter operation fails.
/// </summary>
public class NavAdapterException : Exception
{
    /// <summary>
    /// Constructs a new NavAdapterException.
    /// </summary>
    public NavAdapterException()
        : base()
    {
    }

    /// <summary>
    /// Constructs a new NavAdapterException.
    /// </summary>
    public NavAdapterException(string message)
        : base(message)
    {
        Contract.Requires(message is not null);
    }

    /// <summary>
    /// Constructs a new NavAdapterException.
    /// </summary>
    public NavAdapterException(string message, Exception inner)
        : base(message, inner)
    {
        Contract.Requires(message is not null);
        Contract.Requires(inner is not null);
    }
}
