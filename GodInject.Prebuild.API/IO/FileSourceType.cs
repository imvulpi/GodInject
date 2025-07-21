namespace GodInject.Prebuild.API.IO
{
    /// <summary>
    /// Enum for differentiating what method was used to retrieve files.
    /// Also see <see cref="FileMetaRef"/>
    /// </summary>
    public enum FileSourceType
    {
        WinNT,
        Win32,
        Other
    }
}
