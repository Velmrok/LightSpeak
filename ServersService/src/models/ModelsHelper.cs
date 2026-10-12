namespace ServersService.src.models;

public static class ModelsHelper
{
    public static (string Low, string High) NormalizePair(string a, string b)
    => string.CompareOrdinal(a, b) < 0 ? (a, b) : (b, a);
}