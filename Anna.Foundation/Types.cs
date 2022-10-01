namespace Anna.Foundation;

public readonly struct IntSize
{
    public IntSize(int w, int h)
    {
        Width = w;
        Height = h;
    }

    public static bool operator ==(IntSize x, IntSize y) => x.Width == y.Width && x.Height == y.Height;
    public static bool operator !=(IntSize x, IntSize y) => x.Width != y.Width || x.Height != y.Height;

    public bool Equals(IntSize other) => Width == other.Width && Height == other.Height;
    public override bool Equals(object? obj) => obj is IntSize other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(Width, Height);

    public override string ToString()
    {
        return $"({Width}, {Height})";
    }

    public readonly int Width;
    public readonly int Height;
}