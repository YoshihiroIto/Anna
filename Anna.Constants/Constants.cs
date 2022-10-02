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

public enum Operations
{
    SortEntry,
    MoveCursorUp,
    MoveCursorDown,
    MoveCursorLeft,
    MoveCursorRight,
    ToggleSelectionCursorEntry,
    OpenEntry,
    JumpToParentDirectory,
    JumpToRootDirectory
}
