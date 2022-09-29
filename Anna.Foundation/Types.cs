namespace Anna.Foundation;

public readonly struct IntSize
{
    public IntSize(int w, int h)
    {
        Width = w;
        Height = h;
    }

    public override string ToString()
    {
        return $"({Width}, {Height})";
    }

    public readonly int Width;
    public readonly int Height;
}