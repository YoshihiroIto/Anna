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

// ReSharper disable once PartialTypeWithSinglePart
public partial class SortModeAndOrderDialog : DialogBase<SortModeAndOrderDialogViewModel>
{
    public SortModeAndOrderDialog()
    {
        InitializeComponent();

#if DEBUG
        this.AttachDevTools();
#endif

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
        if (DoMoveFocus(e))
            return;

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
        ViewModel.ResultSortMode = sortModes;
        _states = States.Order;
        UpdateView();
    }

    private void SetSortOrder(SortOrders sortOrders)
    {
        ViewModel.ResultSortOrder = sortOrders;
        ViewModel.DialogResult = DialogResultTypes.Ok;
        Close();
    }

    private void Cancel()
    {
        ViewModel.DialogResult = DialogResultTypes.Cancel;
        Close();
    }

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