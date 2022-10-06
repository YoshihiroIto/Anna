using Anna.Foundation;
using Anna.UseCase;
using Avalonia.Input;
using System.Diagnostics;

namespace Anna.DomainModel.Config;

public class JumpFolderConfig : ConfigBase<JumpFolderConfigData>
{
    public const string Filename = "JumpFolder.json";

    public JumpFolderConfig(IObjectSerializerUseCase objectSerializer)
        : base(objectSerializer)
    {
    }
}

public class JumpFolderConfigData : ConfigData
{
    #region Paths

    private PathData[] _Paths = Array.Empty<PathData>();

    public PathData[] Paths
    {
        get => _Paths;
        set => SetProperty(ref _Paths, value);
    }

    #endregion

    public override void SetDefault()
    {
        var count =
            (Key.F12 - Key.F1) +
            1 +
            (Key.Z - Key.A) +
            1 +
            (Key.D9 - Key.D0) +
            1;

        var paths = new PathData[count];
        {
            var index = 0;

            for (var key = Key.F1; key <= Key.F12; ++key)
                paths[index++] = new PathData { Key = key };

            for (var key = Key.A; key <= Key.Z; ++key)
                paths[index++] = new PathData { Key = key };

            for (var key = Key.D0; key <= Key.D9; ++key)
                paths[index++] = new PathData { Key = key };
        }
        Paths = paths;
    }

    [DebuggerDisplay("Key={Key}, Path={Path}")]
    public class PathData : NotificationObject
    {
        #region Key

        private Key _Key;

        public Key Key
        {
            get => _Key;
            set => SetProperty(ref _Key, value);
        }

        #endregion

        #region Path

        private string _Path = "";

        public string Path
        {
            get => _Path;
            set => SetProperty(ref _Path, value);
        }

        #endregion
    }
}