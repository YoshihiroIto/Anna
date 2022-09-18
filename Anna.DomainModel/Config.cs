﻿using Anna.Foundation;

namespace Anna.DomainModel;

public class Config : NotificationObject
{
    #region FilePath

    private string _FilePath = "";

    public string FilePath
    {
        get => _FilePath;
        set => SetProperty(ref _FilePath, value);
    }

    #endregion

    #region ConfigData

    private ConfigData _ConfigData = new();

    public ConfigData ConfigData
    {
        get => _ConfigData;
        set => SetProperty(ref _ConfigData, value);
    }

    #endregion
}

public class ConfigData : NotificationObject
{
}