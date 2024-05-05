using System;
using UnityEngine;
using Zenject;

namespace Core
{
  public class ServiceInstaller : MonoInstaller
  {
    [SerializeField] private InputHandler _inputHandler;
    [SerializeField] private  GameFieldView _fieldView;
    
    public override void InstallBindings()
    {
      GameData data = new();

      Container.Bind<GameData>().FromInstance(data).WhenInjectedInto<GameService>();
      Container.Bind<Action<int>>().FromInstance(_inputHandler.SelectCheckerPosition).WhenInjectedInto<GameFieldView>();
      Container.Bind<Action<int[]>>().FromInstance(_inputHandler.TryToMakeTurn).WhenInjectedInto<GameFieldView>();

      Container.Bind<IPossibleMovesIndicator>().FromInstance(_fieldView).WhenInjectedInto<InputHandler>();

      Container.Bind<ITurnValidator>().To<TurnValidator>().AsCached().NonLazy();
      Container.Bind<IPossibleMovesProvider>().To<TurnValidator>().WhenInjectedInto<InputHandler>().NonLazy();
      
      Container.Bind<GameService>().AsSingle().NonLazy();
      Container.Bind<ITurnsReceiver>().To<GameService>().FromResolve().NonLazy();
      Container.Bind<IGameDataProvider>().To<GameService>().FromResolve().NonLazy();
    }
  }
}