using Anna.Constants;
using Anna.Strings;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;

namespace Anna.Gui;

public class ResourcesHolder
{
    public Resources Instance
    {
        get => _Instance ??= ResourcesCreator.Create();
        private set => _Instance = value;
    }

    public event EventHandler? CultureChanged;

    public void SetCulture(Cultures culture)
    {
        Resources.Culture = CultureInfo.GetCultureInfo(culture.ToString());

        Thread.CurrentThread.CurrentCulture = Resources.Culture;
        Thread.CurrentThread.CurrentUICulture = Resources.Culture;

        if (_resourcesCache.TryGetValue(culture, out var resources) == false)
        {
            resources = ResourcesCreator.Create();
            _resourcesCache.Add(culture, resources);
        }

        Instance = resources;

        CultureChanged?.Invoke(this, EventArgs.Empty);
    }

    private Resources? _Instance;

    private readonly Dictionary<Cultures, Resources> _resourcesCache = new();
}