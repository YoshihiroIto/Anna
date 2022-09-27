using Anna.UseCase;
using SimpleInjector;

namespace Anna.ServiceProvider;

public class ServiceProviderContainerBase : Container, IServiceProviderContainer
{
    public ServiceProviderContainerBase()
    {
        RegisterSingleton<IServiceProviderContainer>(() => this);
        
#if RELEASE
        Options.EnableAutoVerification = false;
#endif
    }
}