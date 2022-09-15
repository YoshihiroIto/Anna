namespace Anna.DomainModel;

public static class EntryComparison
{
    public static int ByName(Entry x, Entry y)
    {
        return string.Compare(x.Name, y.Name, StringComparison.OrdinalIgnoreCase);
    }
}