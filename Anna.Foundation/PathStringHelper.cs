namespace Anna.Foundation;

public static class PathStringHelper
{
    public static bool IsDotfile(string path)
    {
        return path.Length > 0 &&
               path[0] == '.' &&
               HasSingleDot(path);

        bool HasSingleDot(string str)
        {
            var count = 0;

            foreach (var c in str.AsSpan())
            {
                if (c != '.')
                    continue;

                ++count;
                if (count == 2)
                    return false;
            }

            return count == 1;
        }
    }

    public static string MakeFullPath(string srcPath, string basePath)
    {
        var fullPath = Path.IsPathRooted(srcPath)
            ? srcPath
            : Path.Combine(basePath, srcPath);

        return Normalize(fullPath);
    }

    public static string Normalize(string path)
    {
        return new FileInfo(path).FullName;
    }
}