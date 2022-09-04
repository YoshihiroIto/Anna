namespace Anna.ServiceProvider;

public class Container : SimpleInjector.Container
{
    public Container()
    {
        Options.ResolveUnregisteredConcreteTypes = true;
        
#if DEBUG
        Verify();
#endif
    }
}