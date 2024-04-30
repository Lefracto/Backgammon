using Zenject;

namespace Core
{
  public class ServiceInstaller : MonoInstaller
  {
    public override void InstallBindings()
    {
      GameData data = new();

      Container.Bind<GameData>().FromInstance(data).WhenInjectedInto<GameService>();
      Container.Bind<ITurnValidator>().To<TurnValidator>().AsSingle().NonLazy();
      
      Container.Bind<GameService>().AsSingle().NonLazy();
      Container.Bind<ITurnsReceiver>().To<GameService>().FromResolve().NonLazy();
      Container.Bind<IGameDataProvider>().To<GameService>().FromResolve().NonLazy();
    }
  }
}