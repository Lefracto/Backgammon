using UnityEngine;
using Zenject;

namespace Core
{
  public class InputHandler : MonoBehaviour
  {
    private int _cell;
    private GameService _service;

    [Inject]
    public void Init(GameService service)
    {
      // Test
      _service = service;
      service.RollDices();
      service.InitGame();
      service.MakeTurn(0, 0);
      service.MakeTurn(0, 1);
      service.RollDices();
    }
    
    public void SelectChecker(int cell)
      => _cell = cell;

    public void TryToMakeTurn(int cubeId)
      => _service.MakeTurn(_cell, cubeId);
  }
}