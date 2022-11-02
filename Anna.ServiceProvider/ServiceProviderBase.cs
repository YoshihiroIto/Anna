using Anna.Service;
using SimpleInjector;
using System.Runtime.CompilerServices;
using IServiceProvider=Anna.Service.IServiceProvider;

namespace Anna.ServiceProvider;

public class ServiceProviderBase : Container, IServiceProvider
{
    public ServiceProviderBase()
    {
        RegisterSingleton<IServiceProvider>(() => this);

#if RELEASE
        Options.EnableAutoVerification = false;
#endif
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TService GetInstance<TService>(Type type) where TService : class
    {
        return (TService)GetInstance(type);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public THasArg GetInstance<THasArg, TArg>(TArg arg)
        where THasArg : class, IHasArg<TArg>
    {
        if (arg is null)
            throw new ArgumentNullException(nameof(arg));

        ArgStore<TArg>.Arg = arg;

        return GetInstance<THasArg>();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void PopArg<TArg>(out TArg arg)
    {
        arg = ArgStore<TArg>.Arg;

        ArgStore<TArg>.Arg = default!;
    }

    private static class ArgStore<T>
    {
 #pragma warning disable CS8618
        public static T Arg;
 #pragma warning restore CS8618
    }
}