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

public enum Directions
{
    Up,
    Down,
    Left,
    Right
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

