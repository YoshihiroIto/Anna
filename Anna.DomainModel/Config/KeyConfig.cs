using Anna.Service.Services;
using Avalonia.Input;
using System.Diagnostics;

namespace Anna.DomainModel.Config;

public sealed class KeyConfig : ConfigBase<KeyConfigData>
{
    public const string Filename = "Key.json";

    public KeyConfig(IObjectSerializerService objectSerializer)
        : base(objectSerializer)
    {
    }
}

public sealed class KeyConfigData : ConfigData
{
    #region Keys

    private KeyData[] _Keys = Array.Empty<KeyData>();

    public KeyData[] Keys
    {
        get => _Keys;
        set => SetProperty(ref _Keys, value);
    }

    #endregion

    public override void SetDefault()
    {
        var metaKey = OperatingSystem.IsMacOS() ? KeyModifiers.Meta : KeyModifiers.Control;

        Keys = new KeyData[]
        {
            new(Key.S, KeyModifiers.None, Operations.SortEntry),
            new(Key.J, KeyModifiers.None, Operations.JumpFolder),
            new(Key.Up, KeyModifiers.None, Operations.MoveCursorUp),
            new(Key.Down, KeyModifiers.None, Operations.MoveCursorDown),
            new(Key.Left, KeyModifiers.None, Operations.MoveCursorLeft),
            new(Key.Right, KeyModifiers.None, Operations.MoveCursorRight),
            new(Key.Space, KeyModifiers.None, Operations.ToggleSelectionCursorEntry),
            new(Key.Enter, KeyModifiers.None, Operations.OpenEntry),
            new(Key.Back, KeyModifiers.None, Operations.JumpToParentFolder),
            new(Key.OemPipe, KeyModifiers.None, Operations.JumpToRootFolder),
            new(Key.Enter, KeyModifiers.Shift, Operations.OpenEntryByEditor1),
            new(Key.Enter, metaKey, Operations.OpenEntryByApp),
            new(Key.C, KeyModifiers.None, Operations.CopyEntry),
            new(Key.D, KeyModifiers.None, Operations.DeleteEntry),
            new(Key.Delete, KeyModifiers.None, Operations.DeleteEntry),
        };
    }

    [DebuggerDisplay("Key={Key}, Modifier={Modifier}, Operation={Operation}")]
    public readonly record struct KeyData(Key Key, KeyModifiers Modifier, Operations Operation);
}

public enum Operations
{
    SortEntry,
    JumpFolder,
    MoveCursorUp,
    MoveCursorDown,
    MoveCursorLeft,
    MoveCursorRight,
    ToggleSelectionCursorEntry,
    OpenEntry,
    OpenEntryByEditor1,
    OpenEntryByEditor2,
    OpenEntryByApp,
    JumpToParentFolder,
    JumpToRootFolder,
    CopyEntry,
    DeleteEntry,
}