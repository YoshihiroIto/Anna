using Anna.Constants;
using Anna.UseCase;
using Avalonia.Input;

namespace Anna.DomainModel.Config;

public class KeyConfig : ConfigBase<KeyConfigData>
{
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
        Keys = new[] { new KeyData { Key = Key.S, Modifier = KeyModifiers.None, Operation = Operations.SortEntry } };
    }
}

public struct KeyData
{
    public Key Key { get; set; }
    public KeyModifiers Modifier { get; set; }
    public Operations Operation { get; set; }
}