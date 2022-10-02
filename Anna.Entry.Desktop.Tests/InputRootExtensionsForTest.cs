using Avalonia.Input;
using Avalonia.Input.Raw;

namespace Anna.Entry.Desktop.Tests;

public static class InputRootExtensionsForTest
{
    public static Task PressKeyAsync(
        this IInputRoot inputRoot,
        Key key,
        RawInputModifiers modifiers = RawInputModifiers.None,
        double delayMs = 30)
    {
        KeyDown(inputRoot, key, modifiers);
        KeyUp(inputRoot, key, modifiers);

        return Task.Delay(TimeSpan.FromMilliseconds(delayMs));
    }

    public static void KeyDown(this IInputRoot inputRoot, Key key, RawInputModifiers modifiers = RawInputModifiers.None)
    {
        KeyboardDevice.Instance?.ProcessRawEvent(new RawKeyEventArgs(KeyboardDevice.Instance,
            0,
            inputRoot,
            RawKeyEventType.KeyDown,
            key,
            modifiers));
    }

    public static void KeyUp(this IInputRoot inputRoot, Key key, RawInputModifiers modifiers = RawInputModifiers.None)
    {
        KeyboardDevice.Instance?.ProcessRawEvent(new RawKeyEventArgs(KeyboardDevice.Instance,
            0,
            inputRoot,
            RawKeyEventType.KeyUp,
            key,
            modifiers));
    }
}