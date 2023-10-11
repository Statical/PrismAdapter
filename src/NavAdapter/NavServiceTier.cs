using System;

namespace Statical.NavAdapter;

/// <summary>
/// Represents service tier information for a certain NAV database. Corresponds to data in 
/// the development environment "File - Database - Information".
/// </summary>
public class NavServiceTier
{
    public string ServerName { get; set; }

    public string ServiceInstance { get; set; }

    public int ManagementPort { get; set; }

    public DateTime LastActive { get; set; }

    public string Status { get; set; }
}
