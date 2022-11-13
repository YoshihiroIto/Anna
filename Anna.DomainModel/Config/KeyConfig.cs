using Anna.Service.Services;
using Avalonia.Input;
using System.Diagnostics;
using IServiceProvider=Anna.Service.IServiceProvider;

namespace Anna.DomainModel.Config;

public sealed class KeyConfig : ConfigBase<KeyConfigData>
{
    public const string FileName = "Key.json";

    public KeyConfig(IServiceProvider dic)
        : base(dic)
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

    public override void SetDefault(IDefaultValueService defaultValue)
    {
        Keys = new KeyData[]
        {
            new(Key.S, KeyModifiers.None, Operations.SortEntry),
            //
            new(Key.Up, KeyModifiers.None, Operations.MoveCursorUp),
            new(Key.Down, KeyModifiers.None, Operations.MoveCursorDown),
            new(Key.Left, KeyModifiers.None, Operations.MoveCursorLeft),
            new(Key.Right, KeyModifiers.None, Operations.MoveCursorRight),
            //
            new(Key.Space, KeyModifiers.None, Operations.ToggleSelectionCursorEntry),
            //
            new(Key.J, KeyModifiers.None, Operations.JumpFolder),
            new(Key.Back, KeyModifiers.None, Operations.JumpToParentFolder),
            new(Key.OemPipe, KeyModifiers.None, Operations.JumpToRootFolder),
            //
            new(Key.Enter, KeyModifiers.None, Operations.OpenEntry),
            new(Key.Enter, KeyModifiers.Shift, Operations.OpenExternal1),
            new(Key.Enter, defaultValue.MetaKey, Operations.OpenAssociatedApp),
            new(Key.F3, KeyModifiers.None, Operations.OpenTerminal),
            new(Key.V, KeyModifiers.None, Operations.PreviewEntry),
            //
            new(Key.C, KeyModifiers.None, Operations.CopyEntry),
            new(Key.M, KeyModifiers.None, Operations.MoveEntry),
            new(Key.D, KeyModifiers.None, Operations.DeleteEntry),
            new(Key.Delete, KeyModifiers.None, Operations.DeleteEntry),
            new(Key.N, KeyModifiers.None, Operations.RenameEntry),
            //
            new(Key.K, KeyModifiers.None, Operations.MakeFolder),
            new(Key.K, KeyModifiers.Shift, Operations.MakeFile),
            //
            new(Key.P, KeyModifiers.None, Operations.CompressEntry),
            new(Key.U, KeyModifiers.None, Operations.DecompressEntry),
            //
            new(Key.G, KeyModifiers.Shift, Operations.EmptyTrashCan),
            new(Key.G, defaultValue.MetaKey, Operations.OpenTrashCan),
            //
            new(Key.W, KeyModifiers.Shift, Operations.OpenAnna),
            new(Key.W, defaultValue.MetaKey, Operations.CloseAnna),
            //
            new(Key.D1, KeyModifiers.Shift, Operations.SetListMode1),
            new(Key.D2, KeyModifiers.Shift, Operations.SetListMode2),
            new(Key.D3, KeyModifiers.Shift, Operations.SetListMode3),
            new(Key.D4, KeyModifiers.Shift, Operations.SetListMode4),
            new(Key.D5, KeyModifiers.Shift, Operations.SetListMode5),
        };
    }

    [DebuggerDisplay("Key={Key}, Modifier={Modifier}, Operation={Operation}")]
    public readonly record struct KeyData(Key Key, KeyModifiers Modifier, Operations Operation);
}

public enum Operations
{
    SortEntry,
    //
    MoveCursorUp,
    MoveCursorDown,
    MoveCursorLeft,
    MoveCursorRight,
    //
    ToggleSelectionCursorEntry,
    //
    JumpFolder,
    JumpToParentFolder,
    JumpToRootFolder,
    //
    OpenEntry,
    OpenExternal1,
    OpenExternal2,
    OpenAssociatedApp,
    OpenTerminal,
    PreviewEntry,
    //
    CopyEntry,
    MoveEntry,
    DeleteEntry,
    RenameEntry,
    //
    MakeFolder,
    MakeFile,
    //
    CompressEntry,
    DecompressEntry,
    //
    EmptyTrashCan,
    OpenTrashCan,
    //
    OpenAnna,
    CloseAnna,
    //
    SetListMode1,
    SetListMode2,
    SetListMode3,
    SetListMode4,
    SetListMode5,
}