namespace ServersService.src.permissions;

[Flags]
public enum Permission : long
{
    None = 0,
    WriteOnChannel = 1L << 0,
    ReadOnChannel = 1L << 1,
    All = ~0L
}