﻿using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Anna.Gui.Views.Panels;

public sealed partial class EntriesStatsPanel : UserControl
{
    public EntriesStatsPanel()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}