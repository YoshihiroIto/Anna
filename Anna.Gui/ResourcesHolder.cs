using Anna.Constants;
using Anna.Strings;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;

namespace Anna.Gui;

public sealed class ResourcesHolder
{
    public event EventHandler? CultureChanged;
    
    public Resources Instance
    {
        get => _Instance ??= ResourcesCreator.Create();
        private set => _Instance = value;
    }
    
    private Resources? _Instance;
    private readonly Dictionary<Cultures, Resources> _resourcesCache = new();

    public void SetCulture(Cultures culture)
    {
        Resources.Culture = CultureInfo.GetCultureInfo(culture.ToStringFast());

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
}