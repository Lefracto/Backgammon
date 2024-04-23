using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Core
{
  public class ServiceInstaller : MonoInstaller
  {
    [SerializeField] private int _countFields;
    [SerializeField] private int _minDiceValue;
    [SerializeField] private int _maxDiceValue;

    private List<IDice> _dices;
    public override void InstallBindings()
    {
      _dices = new List<IDice>()
      {
        new SimpleDice(_minDiceValue, _maxDiceValue),
        new SimpleDice(_minDiceValue, _maxDiceValue)
      };
      
      Container.Bind<IEnumerable<IDice>>().FromInstance(_dices).WhenInjectedInto<GameService>();
      Container.Bind<int>().FromInstance(_countFields).WhenInjectedInto<GameService>();

      Container.Bind<GameService>().AsSingle().NonLazy();
      Container.Bind<IGameDataProvider>().To<GameService>().FromResolve();
    }
  }
}