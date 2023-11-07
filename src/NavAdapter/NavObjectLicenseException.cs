using System;
using System.Runtime.Serialization;
using System.Diagnostics.Contracts;

namespace Statical.NavAdapter;

/// <summary>
/// Exception thrown when a NAV object could not be exported because it is not included
/// in the currently loaded NAV license.
/// </summary>
public class NavObjectLicenseException : NavObjectExportException
{
    /// <summary>
    /// Constructs a new NavObjectLicenseException.
    /// </summary>
    public NavObjectLicenseException()
        : base()
    {
    }

    /// <summary>
    /// Constructs a new NavObjectLicenseException.
    /// </summary>
    public NavObjectLicenseException(string message)
        : base(message)
    {
        Contract.Requires(message is not null);
    }

    /// <summary>
    /// Constructs a new NavObjectLicenseException.
    /// </summary>
    public NavObjectLicenseException(string message, Exception inner)
        : base(message, inner)
    {
        Contract.Requires(message is not null);
        Contract.Requires(inner is not null);
    }
}
