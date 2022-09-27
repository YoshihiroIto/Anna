namespace Anna.UseCase
{
    public interface IServiceProviderContainer
    {
        TService GetInstance<TService>() where TService : class;
    }
}