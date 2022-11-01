using System.Runtime.CompilerServices;

namespace Anna.Service;

public interface IServiceProvider
{
    TService GetInstance<TService>() where TService : class;

    TService GetInstance<TService>(Type type) where TService : class;

    THasArg GetInstance<THasArg, TArg>(TArg arg)
        where THasArg : class, IHasArg<TArg>;

    void PopArg<TArg>(out TArg arg);
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    // ReSharper disable once UnusedParameter.Global
    public THasArg GetInstance<THasArg, TArg>(THasArg t, TArg arg)
        where THasArg : class, IHasArg<TArg>
    {
        return GetInstance<THasArg, TArg>(arg);
    }
}

// ReSharper disable once UnusedTypeParameter
public interface IHasArg<TArg>
{
}