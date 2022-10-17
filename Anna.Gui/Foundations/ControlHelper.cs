using Avalonia.Controls;
using System;

namespace Anna.Gui.Foundations;

public static class ControlHelper
{
    public static Window FindOwnerWindow(IControl control)
    {
        return FindParent<Window>(control) ?? throw new InvalidOperationException();
    }

    public static T? FindParent<T>(IControl control)
        where T : class
    {
        var parent = control;

        while (true)
        {
            switch (parent)
            {
                case T p:
                    return p;

                case null:
                    return null;

                default:
                    parent = parent.Parent;
                    break;
            }
        }
    }
}