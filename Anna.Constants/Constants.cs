using NetEscapades.EnumGenerators;

namespace Anna.Constants;

[EnumExtensions]
public enum Cultures
{
    En,
    Ja
}

public enum SortModes
{
    Name,
    Extension,
    Timestamp,
    Size,
    Attributes,
}

public enum SortOrders
{
    Ascending,
    Descending
}

public enum ResultCode
{
    Ok,
    Error
}

[Flags]
public enum Directions
{
    Up = 1 << 0,
    Down = 1 << 1,
    Left = 1 << 2,
    Right = 1 << 3
}

public enum FileEntryFormat
{
    Image,
    Text
}

public enum EntryDeleteModes
{
    Delete,
    TrashCan
}

public enum DeleteStrategies
{
    Skip,
    Delete,
    AllDelete,
}

public static class Constants
{
    // todo: ReadOnlyHashSet
    public static readonly HashSet<string> SupportedImageFormats = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpeg",
        ".jpg",
        ".png",
        ".dng",
        ".webp",
        ".gif",
        ".bmp",
        ".ico",
        ".astc",
        ".ktx",
        ".pkm",
        ".wbmp",
        ".cr2",
        ".nef",
        ".arw"
    };
}

