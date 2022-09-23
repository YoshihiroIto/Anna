using Anna.Constants;
using Anna.Gui.Views.Dialogs.Base;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System;
using System.Collections.Generic;

namespace Anna.Gui.Views.Dialogs;

public partial class SortModeAndOrderDialog : DialogBase
{
    public SortModeAndOrderDialog()
    {
        InitializeComponent();

#if DEBUG
        this.AttachDevTools();
#endif

        var modeNameButton = this.FindControl<Button>("ModeNameButton");
        if (modeNameButton is not null)
        {
            modeNameButton.AttachedToVisualTree += (_, _) =>
                FocusManager.Instance?.Focus(modeNameButton, NavigationMethod.Directional);
        }

        UpdateView();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void ModeButton_OnClick(object? sender, RoutedEventArgs e)
    {
        SetSortMode((SortModes)((sender as Control)?.Tag ?? throw new NullReferenceException()));
    }

    private void OrderButton_OnClick(object? sender, RoutedEventArgs e)
    {
        SetSortOrder((SortOrders)((sender as Control)?.Tag ?? throw new NullReferenceException()));
    }

    private void CancelButton_OnClick(object? sender, RoutedEventArgs e)
    {
        Cancel();
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Down or Key.Right:
                MoveFocus(true);
                e.Handled = true;
                return;

            case Key.Up or Key.Left:
                MoveFocus(false);
                e.Handled = true;
                return;
        }

        if (_states == States.Mode)
        {
            if (KeyToSortMode.TryGetValue(e.Key, out var value))
            {
                SetSortMode(value);
                e.Handled = true;
                return;
            }
        }

        if (_states == States.Order)
        {
            if (KeyToSortOrder.TryGetValue(e.Key, out var value))
            {
                SetSortOrder(value);
                e.Handled = true;
                return;
            }
        }

        if (e.Key == Key.C)
            Cancel();
    }

    private void UpdateView()
    {
        switch (_states)
        {
            case States.Mode:
                this.FindControl<Panel>("ModePanel")!.IsEnabled = true;
                this.FindControl<Panel>("OrderPanel")!.IsEnabled = false;
                break;

            case States.Order:
                this.FindControl<Panel>("ModePanel")!.IsEnabled = false;
                this.FindControl<Panel>("OrderPanel")!.IsEnabled = true;

                FocusManager.Instance?
                    .Focus(this.FindControl<Button>("OrderAscendingButton"), NavigationMethod.Directional);
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void SetSortMode(SortModes sortModes)
    {
        ViewModel.SortMode = sortModes;
        _states = States.Order;
        UpdateView();
    }

    private void SetSortOrder(SortOrders sortOrders)
    {
        ViewModel.SortOrder = sortOrders;
        ViewModel.DialogResult = DialogResultTypes.Ok;
        Close();
    }

    private void Cancel()
    {
        ViewModel.DialogResult = DialogResultTypes.Cancel;
        Close();
    }

    private static void MoveFocus(bool isNext)
    {
        var current = FocusManager.Instance?.Current;
        if (current is null)
            return;

        var next = KeyboardNavigationHandler.GetNext(current,
        isNext
            ? NavigationDirection.Next
            : NavigationDirection.Previous);

        if (next is not null)
            FocusManager.Instance?.Focus(next, NavigationMethod.Directional);
    }

    private SortModeAndOrderDialogViewModel ViewModel =>
        DataContext as SortModeAndOrderDialogViewModel ?? throw new NullReferenceException();

    private States _states = States.Mode;

    private static readonly IReadOnlyDictionary<Key, SortModes> KeyToSortMode = new Dictionary<Key, SortModes>
    {
        { Key.N, SortModes.Name },
        { Key.E, SortModes.Extension },
        { Key.S, SortModes.Size },
        { Key.T, SortModes.Timestamp },
        { Key.R, SortModes.Attributes }
    };

    private static readonly IReadOnlyDictionary<Key, SortOrders> KeyToSortOrder = new Dictionary<Key, SortOrders>
    {
        { Key.A, SortOrders.Ascending }, { Key.E, SortOrders.Descending }
    };

    private enum States
    {
        Mode,
        Order
    }
}