namespace Anna.UseCase;

public interface IServiceProviderContainer
{
    TService GetInstance<TService>() where TService : class;

    THasArgService GetInstance<THasArgService, TArg>(TArg arg)
        where THasArgService : class, IHasArg<TArg>;

    void PopArg<TArg>(out TArg arg);
}

// ReSharper disable once UnusedTypeParameter
public interface IHasArg<TArg>
{
}

public interface IHasServiceProviderContainer
{
    IServiceProviderContainer ServiceProviderContainer { get; }
}