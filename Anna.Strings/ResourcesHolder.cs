using Anna.Constants;
using System.Globalization;

namespace Anna.Strings;

public class ResourcesHolder
{
    public Resources Instance { get; } = new();

    public event EventHandler? CultureChanged;

    public void SetCulture(Cultures culture)
    {
        Resources.Culture = CultureInfo.GetCultureInfo(culture.ToString());

        Thread.CurrentThread.CurrentCulture = Resources.Culture;
        Thread.CurrentThread.CurrentUICulture = Resources.Culture;

        CultureChanged?.Invoke(null, EventArgs.Empty);
    }
}