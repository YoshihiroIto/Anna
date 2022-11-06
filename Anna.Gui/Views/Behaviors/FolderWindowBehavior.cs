using Anna.DomainModel.Config;
using Avalonia;
using Avalonia.Data;
using Avalonia.Xaml.Interactivity;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using FolderWindow=Anna.Gui.Views.Windows.FolderWindow;
using IServiceProvider=Anna.Service.IServiceProvider;

namespace Anna.Gui.Views.Behaviors;

public sealed class FolderWindowPositionBehavior : Behavior<FolderWindow>
{
    public static readonly StyledProperty<IServiceProvider> DicProperty =
        AvaloniaProperty.Register<FolderWindowPositionBehavior, IServiceProvider>(
            nameof(Dic),
            defaultBindingMode: BindingMode.OneTime,
            notifying: (obj, isBefore) =>
            {
                if (isBefore == false)
                    ((FolderWindowPositionBehavior)obj).ViewModelNotifying();
            }
        );

    public IServiceProvider Dic
    {
        get => GetValue(DicProperty);
        set => SetValue(DicProperty, value);
    }

    private ObservableCollection<FolderWindowConfigData> FolderWindows =>
        Dic.GetInstance<AppConfig>().Data.FolderWindows;

    private void ViewModelNotifying()
    {
        if (AssociatedObject is null)
            return;

        var id = AssociatedObject.ViewModel.Id;
        var configData = FolderWindows.FirstOrDefault(x => x.Id == id);

        if (configData is not null)
        {
            AssociatedObject.Width = configData.Width;
            AssociatedObject.Height = configData.Height;
            AssociatedObject.Position = new PixelPoint(configData.X, configData.Y);
        }
    }

    protected override void OnAttached()
    {
        base.OnAttached();

        if (AssociatedObject is not null)
            AssociatedObject.Closing += AssociatedObjectOnClosed;
    }
    
    protected override void OnDetaching()
    {
        if (AssociatedObject is not null)
            AssociatedObject.Closing -= AssociatedObjectOnClosed;

        base.OnDetaching();
    }

    private void AssociatedObjectOnClosed(object? sender, EventArgs e)
    {
        if (AssociatedObject is null)
            return;

        var id = AssociatedObject.ViewModel.Id;

        var configData = FolderWindows.FirstOrDefault(x => x.Id == id);
        if (configData is not null)
            FolderWindows.Remove(configData);

        var newConfigData = new FolderWindowConfigData(
            id,
            AssociatedObject.Position.X,
            AssociatedObject.Position.Y,
            AssociatedObject.Width,
            AssociatedObject.Height);

        FolderWindows.Add(newConfigData);
    }
}