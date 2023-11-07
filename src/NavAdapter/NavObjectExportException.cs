using System;
using System.Runtime.Serialization;
using System.Diagnostics.Contracts;

namespace Statical.NavAdapter;

/// <summary>
/// Exception thrown when a NAV object could not be exported.
/// </summary>
public class NavObjectExportException : NavAdapterException
{
    /// <summary>
    /// Constructs a new NavExportException.
    /// </summary>
    public NavObjectExportException()
        : base()
    {
    }

    /// <summary>
    /// Constructs a new NavExportException.
    /// </summary>
    public NavObjectExportException(string message)
        : base(message)
    {
        Contract.Requires(message is not null);
    }

    /// <summary>
    /// Constructs a new NavExportException.
    /// </summary>
    public NavObjectExportException(string message, Exception inner)
        : base(message, inner)
    {
        Contract.Requires(message is not null);
        Contract.Requires(inner is not null);
    }
}
