﻿using NetEscapades.EnumGenerators;

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

public enum ExistsCopyFileActions
{
    Skip,
    NewerTimestamp,
    Override,
    Rename,
}

public enum SamePathCopyFileActions
{
    Skip,
    Override,
}

public enum ReadOnlyDeleteActions
{
    Delete,
    AllDelete,
    Skip,
    Cancel,
}

public enum AccessFailureDeleteActions
{
    Skip,
    Cancel,
    Retry
}

public enum CopyOrMove
{
    Unset,
    Copy,
    Move
}

[EnumExtensions]
[Flags]
public enum DialogResultTypes
{
    Retry = 1 << 0,

    Yes = 1 << 10,
    No = 1 << 11,
    Ok = 1 << 12,
    Cancel = 1 << 13,
    Skip = 1 << 14,

    AllDelete = 1 << 20,
    OpenTrashCan = 1 << 21,
}

public enum ExternalApp
{
    Terminal,
    App1,
    App2,
}

public static class Constants
{
    public const uint ListModeCount = 4;
    
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
    
    // todo: ReadOnlyHashSet
    public static readonly HashSet<string> SupportedArchiveFormats = new(StringComparer.OrdinalIgnoreCase) { ".zip" };
}