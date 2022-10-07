﻿using Anna.Constants;
using Anna.UseCase;
using Avalonia.Input;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Anna.DomainModel.Config;

public class KeyConfig : ConfigBase<KeyConfigData>
{
    public const string Filename = "Key.json";

    public KeyConfig(IObjectSerializerUseCase objectSerializer)
        : base(objectSerializer)
    {
    }
}

public class KeyConfigData : ConfigData
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
        var isMacOs = RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
        var metaKey = isMacOs ? KeyModifiers.Meta : KeyModifiers.Control;
        
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
        };
    }

    [DebuggerDisplay("Key={Key}, Modifier={Modifier}, Operation={Operation}")]
    public readonly record struct KeyData(Key Key, KeyModifiers Modifier, Operations Operation);
}