using System;
using System.Runtime.Serialization;
using System.Diagnostics.Contracts;

namespace Statical.NavAdapter;

/// <summary>
/// Exception thrown when a NAV adapter operation fails.
/// </summary>
[Serializable]
public class NavAdapterException : Exception, ISerializable
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
        Contract.Requires(message != null);
    }

    /// <summary>
    /// Constructs a new NavAdapterException.
    /// </summary>
    public NavAdapterException(string message, Exception inner)
        : base(message, inner)
    {
        Contract.Requires(message != null);
        Contract.Requires(inner != null);
    }

    public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        Contract.Requires(info != null);

        base.GetObjectData(info, context);
        if (info == null)
            throw new ArgumentNullException("info");
    }
}
