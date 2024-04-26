using Zenject;

namespace Core
{
  public class ServiceInstaller : MonoInstaller
  {
    public override void InstallBindings()
    {
      Container.Bind<GameService>().AsSingle().NonLazy();
      Container.Bind<ITurnsReceiver>().To<GameService>().FromResolve().NonLazy();
      Container.Bind<IGameDataProvider>().To<GameService>().FromResolve().NonLazy();
    }
  }
}