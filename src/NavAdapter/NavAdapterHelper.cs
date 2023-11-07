using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Data;
using System.Diagnostics.Contracts;
using System.Data.SqlClient;

namespace Statical.NavAdapter;

public static class NavAdapterHelper
{
    #region INavAdapter Helpers
    /// <summary>
    /// Retrieve metadata for the given id range set.
    /// </summary>
    /// <param name="environment">A Dynamics NAV environment</param>
    /// <param name="idRanges">A set of object id ranges to include</param>
    /// <param name="versionExclusions">A set of version patterns to exclude (may include '*' wildcards)</param>
    /// <param name="cancellationToken">A task cancellation token to abort the operation</param>
    /// <returns>A set of object metadata</returns>
    public static async Task<ISet<NavObjectMetadata>> ObjectMetadataAsync(this NavEnvironment environment, ISet<NavObjectIdRange> idRanges, ISet<NavVersionListFilter> versionExclusions, CancellationToken cancellationToken)
    {
        Contract.Requires(environment is not null);
        Contract.Requires(idRanges is not null);
        Contract.Requires(Contract.ForAll(idRanges, x => x is not null));
        Contract.Requires(Contract.ForAll(versionExclusions, x => x is not null && ! String.IsNullOrWhiteSpace(x.Filter)));

        var result = new HashSet<NavObjectMetadata>();

        using (var connection = new SqlConnection(environment.DbConnectionString))
        {
            NavObjectType[] allObjectTypes = (NavObjectType[])Enum.GetValues(typeof(NavObjectType));
            var typeWhereClause = " Type IN (" + string.Join(',', allObjectTypes.Select(t => (int)t)) + ")";
            var idWhereClause = (idRanges.Count == 0) ? "1=1" : NavObjectIdRange.SqlWhereClause(idRanges);
            var versionWhereClause = (versionExclusions.Count == 0) ? "1=1" : NavVersionListFilter.SqlWhereClause(versionExclusions);
            var query =
                   @"SELECT 
                        ID, 
                        Type, 
                        Name, 
                        [BLOB Size], 
                        [Version List], 
                        Date, 
                        Time, 
                        timestamp 
                    FROM dbo.Object WITH (NOLOCK)
                    WHERE " + typeWhereClause + " AND (" + idWhereClause + ") AND (" + versionWhereClause + ")";

            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                using var command = new SqlCommand(null, connection);
                command.CommandText = query;
                NavVersionListFilter.AddSqlParameters(command, versionExclusions);
                command.Prepare();
                using var reader = await command.ExecuteReaderAsync(CommandBehavior.SingleResult, cancellationToken).ConfigureAwait(false);
                while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
                {
                    // fetch values
                    var id = reader.GetInt32(0);
                    var type = reader.GetInt32(1);
                    var name = reader.GetString(2);
                    var blobSize = reader.GetInt32(3);
                    var versionList = reader.GetString(4);
                    var date = reader.GetDateTime(5);
                    var time = reader.GetDateTime(6);
                    var timestamp = reader.GetSqlBinary(7);

                    var dateTime = new DateTime(date.Year, date.Month, date.Day, time.Hour, time.Minute, time.Second);

                    var metadata = new NavObjectMetadata()
                    {
                        ObjectReference = new NavObjectReference((NavObjectType)type, id),
                        Name = name,
                        BlobSize = blobSize,
                        VersionList = versionList,
                        Time = dateTime,
                        RowVersion = ToHex(timestamp.Value)
                    };
                    result.Add(metadata);
                }
            }
            finally
            {
                connection.Close();
            }
        }

        return result;
    }

    /// <summary>
    /// Lists NAV service tiers.
    /// </summary>
    /// <param name="cancellationToken">A token to allow cancellation of the operation<</param>
    /// <returns>Information about service tiers and their status</returns>
    public static async Task<ISet<NavServiceTier>> ServiceTiers(this NavEnvironment environment, CancellationToken cancellationToken)
    {
        Contract.Requires(environment is not null);

        var result = new HashSet<NavServiceTier>();

        using (var connection = new SqlConnection(environment.DbConnectionString))
        {
            var query =
                   @"SELECT 
                           [Server Computer Name], 
                           [Server Instance Name],
                           [Management Port],
                           [Last Active],
                           [Status]
                         FROM dbo.[Server Instance] WITH (NOLOCK)";

            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                using var command = new SqlCommand(null, connection);
                command.CommandText = query;
                command.Prepare();
                using var reader = await command.ExecuteReaderAsync(CommandBehavior.SingleResult, cancellationToken).ConfigureAwait(false);
                while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
                {
                    // fetch values
                    string serverName = reader.GetString(0);
                    string serverInstanceName = reader.GetString(1);
                    int managementPort = reader.GetInt32(2);
                    DateTime lastActive = reader.GetDateTime(3);
                    int status = reader.GetInt32(4);

                    string statusText = "Unknown";
                    switch (status)
                    {
                        case 0:
                            statusText = "Started";
                            break;
                        case 1:
                            statusText = "Stopped";
                            break;
                        case 2:
                            statusText = "Crashed";
                            break;
                    }

                    var serviceTier = new NavServiceTier()
                    {
                        ServerName = serverName,
                        ServiceInstance = serverInstanceName,
                        ManagementPort = managementPort,
                        LastActive = lastActive,
                        Status = statusText
                    };
                    result.Add(serviceTier);
                }
            }
            finally
            {
                connection.Close();
            }
        }

        return result;
    }

    /// <summary>
    /// Test the environment connection.
    /// </summary>
    /// <param name="environment">A Dynaics NAV environment</param>
    /// <param name="cancellationToken">A task cancellation token to abort the operation</param>
    /// <returns></returns>
    public static async Task<ISet<string>> TestAsync(this NavEnvironment environment, CancellationToken cancellationToken)
    {
        Contract.Requires(environment is not null);

        var result = new HashSet<string>();
        try
        {
            var temp = await environment.ObjectMetadataAsync(new HashSet<NavObjectIdRange>(), new HashSet<NavVersionListFilter>(), cancellationToken).ConfigureAwait(false);
            // temp -> dev/null
        }
        catch (Exception e)
        {
            result.Add("Could not connect to database: " + e.Message);
        }
        return result;
    }
    #endregion

    #region Misc Helpers
    /// <summary>
    /// Waits for the process to complete asynchronously.
    /// </summary>
    /// <param name="process">The process to wait for</param>
    /// <param name="cancellationToken">The cancellation</param>
    /// <param name="throwOnExitError">Throws a ProcessExitException if the process terminated exceptionally</param>
    /// <returns>The process</returns>
    public static async Task<Process> WaitAsync(Process process, CancellationToken cancellationToken, bool throwOnExitError = true)
    {
        Contract.Requires(process is not null);

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (process.HasExited)
            {
                break;
            }
            await Task.Run(() => process.WaitForExit(100));
        }
        return process;
    }

    /// <summary>
    /// Try to kill a process.
    /// </summary>
    /// <param name="process">The process to kill</param>
    /// <returns>True if the process was ended, false otherwise</returns>
    public static bool TryEndProcess(this Process process)
    {
        try
        {
            process.Kill();
            process.Dispose();
        }
        catch (Exception)
        {
            try
            {
                process.Dispose();
            }
            catch (ObjectDisposedException)
            { 
                // already disposed
            }
            catch (Exception)
            {
                // ignore
                return false;
            }
        }
        return true;
    }

    private static string ToHex(byte[] bs)
    {
        return string.Join("-", bs.Select(b => string.Format("{0:X2}", b)));
    }

    #endregion
}
