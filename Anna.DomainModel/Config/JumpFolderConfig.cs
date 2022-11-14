using Anna.Foundation;
using Anna.Service.Services;
using Avalonia.Input;
using System.Diagnostics;
using IServiceProvider=Anna.Service.IServiceProvider;

namespace Anna.DomainModel.Config;

public sealed class JumpFolderConfig : ConfigBase<JumpFolderConfigData>
{
    public const string FileName = "JumpFolder.json";

    public JumpFolderConfig(IServiceProvider dic)
        : base(dic)
    {
    }
}

public sealed class JumpFolderConfigData : ConfigData
{
    #region Paths

    private PathData[] _Paths = Array.Empty<PathData>();

    public PathData[] Paths
    {
        get => _Paths;
        set => SetProperty(ref _Paths, value);
    }

    #endregion

    public override void SetDefault(IPlatformValueService platformValue)
    {
        const int count =
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
    public sealed class PathData : NotificationObject
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