using Avalonia.Controls;
using System;

namespace Anna.Gui.Foundations;

public static class ControlHelper
{
    public static Window FindOwnerWindow(IControl control)
    {
        var parent = control;

        while (true)
        {
            switch (parent)
            {
                case Window window:
                    return window;
                    
                case null:
                    throw new InvalidOperationException();

                default:
                    parent = parent.Parent;
                    break;
            }
        }
    }
}