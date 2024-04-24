using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Core
{
  public class ServiceInstaller : MonoInstaller
  {
    public override void InstallBindings()
    {
      Container.Bind<GameService>().AsSingle().NonLazy();
      Container.Bind<IGameDataProvider>().To<GameService>().FromResolve();
    }
  }
}