using Anna.Constants;
using System.Globalization;

namespace Anna.Strings;

public class ResourcesHolder
{
    public Resources Instance { get; private set; } = new();

    public event EventHandler? CultureChanged;

    public void SetCulture(Cultures culture)
    {
        Resources.Culture = CultureInfo.GetCultureInfo(culture.ToString());

        Thread.CurrentThread.CurrentCulture = Resources.Culture;
        Thread.CurrentThread.CurrentUICulture = Resources.Culture;

        if (_resourcesCache.TryGetValue(culture, out var resources) == false)
        {
            resources = new Resources();
            _resourcesCache.Add(culture, resources);
        }

        Instance = resources;

        CultureChanged?.Invoke(this, EventArgs.Empty);
    }

    private readonly Dictionary<Cultures, Resources> _resourcesCache = new();
}