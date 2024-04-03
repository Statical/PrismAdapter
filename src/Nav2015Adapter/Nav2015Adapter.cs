using Statical.NavAdapter.Nav2013;
using System.Diagnostics.Contracts;

namespace Statical.NavAdapter.Nav2015;

/// <summary>
/// Dynamics NAV 2015 NavAdapter. Implementation is based on finsql.exe command-line.
/// The implementation ignores unlicensed objects.
/// </summary>
public sealed class Nav2015Adapter : Nav2013Adapter, INavAdapter
{
    #region Constructors
    /// <summary>
    /// Constructs a new Nav2015 based on the given NAV environment.
    /// </summary>
    /// <param name="env">The NAV environment to connect to</param>
    public Nav2015Adapter(NavEnvironment env) : base(env)
    {
        Contract.Requires(env is not null);
        this.AdapterName = "Dynamics NAV 2015";
        this.AdapterDescription = "Adapter to interface with Dynamics NAV 2015.";
        this.AdapterVersion = "1.2";
    }
    #endregion

    #region Implementation Helpers

    protected override string FinSqlArguments(string filter, bool singleObject, string filePath, string tmpLogFile)
    {
        var result = base.FinSqlArguments(filter, singleObject, filePath, tmpLogFile);
        // When exporting multiple objects, we ignore unlicensed objects. Otherwise we fail on unlicensed objects.
        return result + ", ExportTxtSkipUnlicensed=" + (singleObject ? "0" : "1");
    }

    #endregion
}
