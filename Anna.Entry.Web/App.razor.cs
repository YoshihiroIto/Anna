using Avalonia.Web.Blazor;

namespace Anna.Web;

public partial class App
{
    protected override void OnParametersSet()
    {
        base.OnParametersSet();
        
        WebAppBuilder.Configure<Anna.GuiApp>()
            .SetupWithSingleViewLifetime();
    }
}