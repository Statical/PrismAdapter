using System;
using System.Text;

namespace Statical.NavAdapter;

/// <summary>
/// This class represents metadata of an object. This can be used for update detection in the NAV
/// mirrored database.
/// </summary>
public class NavObjectMetadata
{

    /// <summary>
    /// The object identifier
    /// </summary>
    public NavObjectReference ObjectReference { get; set; }

    /// <summary>
    /// The NAV object's name (source is field in dbo.Object table)
    /// </summary>
    public string Name { get; set;}

    /// <summary>
    /// The BLOB Size of the NAV object (source is field in dbo.Object table)
    /// </summary>
    public int BlobSize { get; set; }

    /// <summary>
    /// The NAV object's version list (source is field in dbo.Object table)
    /// </summary>
    public string VersionList { get; set; }

    /// <summary>
    /// Combination of Date and Time fields of dbo.Object table
    /// </summary>
    public DateTime Time { get; set; }

    /// <summary>
    /// The timestamp (or rowversion) column value of NAV's Object table. Can be used for strict
    /// comparison, which will give differences on any change in the object table, including
    /// locking and unlocking of objects
    /// </summary>
    public string RowVersion { get; set; }

    /// <summary>
    /// Combine the metadata into a string. Useful for debugging purposes.
    /// </summary>
    /// <returns>A string representation of the metadata.</returns>
    public override string ToString() => new StringBuilder()
            .Append("NavObjectMetadata{")
            .Append("ObjectReference=").Append(ObjectReference.ToString()).Append(',')
            .Append("Name=").Append(Name).Append(',')
            .Append("BlobSize=").Append(BlobSize).Append(',')
            .Append("VersionList=").Append(VersionList).Append(',')
            .Append("Time=").Append(Time).Append(',')
            .Append("RowVersion=").Append(RowVersion)
            .Append('}').ToString();
}
