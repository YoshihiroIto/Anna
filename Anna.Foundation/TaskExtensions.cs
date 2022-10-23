using System.Runtime.CompilerServices;

namespace Anna.Foundation;

public static class TaskExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Forget(this Task task)
    {
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Forget<T>(this Task<T> task)
    {
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Forget(this ValueTask task)
    {
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Forget<T>(this ValueTask<T> task)
    {
    }
}
