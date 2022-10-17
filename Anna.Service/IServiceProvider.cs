namespace Anna.Service;

public interface IServiceProvider
{
    TService GetInstance<TService>() where TService : class;

    THasArg GetInstance<THasArg, TArg>(TArg arg)
        where THasArg : class, IHasArg<TArg>;

    void PopArg<TArg>(out TArg arg);
}

// ReSharper disable once UnusedTypeParameter
public interface IHasArg<TArg>
{
}

public interface IHasServiceProviderContainer
{
    IServiceProvider Dic { get; }
}