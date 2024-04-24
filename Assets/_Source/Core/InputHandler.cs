using UnityEngine;
using Zenject;

namespace Core
{
  public class InputHandler : MonoBehaviour
  {
    private int _cell;
    private GameService _service;
    
    // Set for interface
    [Inject]
    public void Init(GameService service)
      => _service = service;
    
    public void SelectChecker(int cell)
      => _cell = cell;

    public void TryToMakeTurn(int cubeId)
    {
      _service.MakeTurn(_cell, cubeId); 
    }

    public void StartGame()
    {
      _service.InitGame();
    }
  }
}