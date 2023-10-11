namespace Statical.NavAdapter;


/// <summary>
/// This enumeration represents the different object types of NAV. The underlying numbers match the
/// values of dbo.Object table.
/// </summary>
public enum NavObjectType : int
{
    /// <summary>
    /// Value that represents the Dynamics NAV table object type.
    /// </summary>
    Table = 1,

    /// <summary>
    /// Value that represents the Dynamics NAV form object type.
    /// </summary>
    Form = 2,

    /// <summary>
    /// Value that represents the Dynamics NAV report object type.
    /// </summary>
    Report = 3,

    /// <summary>
    /// Value that represents the Dynamics NAV dataport object type.
    /// </summary>
    Dataport = 4,

    /// <summary>
    /// Value that represents the Dynamics NAV codeunit object type.
    /// </summary>
    Codeunit = 5,

    /// <summary>
    /// Value that represents the Dynamics NAV XML port object type.
    /// </summary>
    XMLPort = 6,

    /// <summary>
    /// Value that represents the Dynamics NAV menusuite object type.
    /// </summary>
    MenuSuite = 7,

    /// <summary>
    /// Value that represents the Dynamics NAV page object type.
    /// </summary>
    Page = 8,

    /// <summary>
    /// Value that represents the Dynamics NAV query object type.
    /// </summary>
    Query = 9,
}
