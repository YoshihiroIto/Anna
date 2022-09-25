using Anna.Constants;
using NaturalSort.Extension;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Anna.DomainModel;

public static class EntryComparison
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ByNameAscending(Entry x, Entry y)
    {
        if (x.IsParentDirectory)
            return -1;
        if (y.IsParentDirectory)
            return +1;
        
        return CompareByName(x, y);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ByNameDescending(Entry x, Entry y)
    {
        if (x.IsParentDirectory)
            return -1;
        if (y.IsParentDirectory)
            return +1;
        
        return ByNameAscending(y, x);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ByExtensionAscending(Entry x, Entry y)
    {
        if (x.IsParentDirectory)
            return -1;
        if (y.IsParentDirectory)
            return +1;
        
        return CompareByExtension(x, y);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ByExtensionDescending(Entry x, Entry y)
    {
        if (x.IsParentDirectory)
            return -1;
        if (y.IsParentDirectory)
            return +1;
    
        return ByExtensionAscending(y, x);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ByTimestampAscending(Entry x, Entry y)
    {
        if (x.IsParentDirectory)
            return -1;
        if (y.IsParentDirectory)
            return +1;
        
        if (x.Timestamp < y.Timestamp)
            return -1;
        if (x.Timestamp > y.Timestamp)
            return +1;

        return ByNameAscending(x, y);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ByTimestampDescending(Entry x, Entry y)
    {
        if (x.IsParentDirectory)
            return -1;
        if (y.IsParentDirectory)
            return +1;
        
        return ByTimestampAscending(y, x);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int BySizeAscending(Entry x, Entry y)
    {
        if (x.IsParentDirectory)
            return -1;
        if (y.IsParentDirectory)
            return +1;
        
        if (x.Size < y.Size)
            return -1;
        if (x.Size > y.Size)
            return +1;

        return ByNameAscending(x, y);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int BySizeDescending(Entry x, Entry y)
    {
        if (x.IsParentDirectory)
            return -1;
        if (y.IsParentDirectory)
            return +1;
        
        return BySizeAscending(y, x);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ByAttributesAscending(Entry x, Entry y)
    {
        if (x.IsParentDirectory)
            return -1;
        if (y.IsParentDirectory)
            return +1;
    
        if (x.Attributes < y.Attributes)
            return -1;
        if (x.Attributes > y.Attributes)
            return +1;

        return ByNameAscending(x, y);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ByAttributesDescending(Entry x, Entry y)
    {
        if (x.IsParentDirectory)
            return -1;
        if (y.IsParentDirectory)
            return +1;
    
        return ByAttributesAscending(y, x);
    }

    public static Comparison<Entry> FindEntryCompare(
        SortModes mode,
        SortOrders order)
    {
        return mode switch
        {
            SortModes.Name when order == SortOrders.Ascending => ByNameAscending,
            SortModes.Name when order == SortOrders.Descending => ByNameDescending,
            SortModes.Extension when order == SortOrders.Ascending => ByExtensionAscending,
            SortModes.Extension when order == SortOrders.Descending => ByExtensionDescending,
            SortModes.Timestamp when order == SortOrders.Ascending => ByTimestampAscending,
            SortModes.Timestamp when order == SortOrders.Descending => ByTimestampDescending,
            SortModes.Size when order == SortOrders.Ascending => BySizeAscending,
            SortModes.Size when order == SortOrders.Descending => BySizeDescending,
            SortModes.Attributes when order == SortOrders.Ascending => ByAttributesAscending,
            SortModes.Attributes when order == SortOrders.Descending => ByAttributesDescending,
            _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
        };
    }
    
    private static int CompareByName(Entry x, Entry y)
    {
        Debug.Assert(x.IsDirectory == y.IsDirectory);

        if (x.IsDirectory)
        {
            var nameWithExt = NameComparer.Compare(x.NameWithExtension, y.NameWithExtension);
            if (nameWithExt != 0)
                return nameWithExt;
        }
        else
        {
            var name = NameComparer.Compare(x.Name, y.Name);
            if (name != 0)
                return name;

            var ext = NameComparer.Compare(x.Extension, y.Extension);
            if (ext != 0)
                return ext;
        }

        return 0;
    }

    private static int CompareByExtension(Entry x, Entry y)
    {
        Debug.Assert(x.IsDirectory == y.IsDirectory);

        if (x.IsDirectory)
        {
            var nameWithExt = NameComparer.Compare(x.NameWithExtension, y.NameWithExtension);
            if (nameWithExt != 0)
                return nameWithExt;
        }
        else
        {
            var ext = NameComparer.Compare(x.Extension, y.Extension);
            if (ext != 0)
                return ext;

            var name = NameComparer.Compare(x.Name, y.Name);
            if (name != 0)
                return name;
        }

        return 0;
    }

    private static readonly NaturalSortComparer NameComparer =
        new(StringComparison.OrdinalIgnoreCase);
}