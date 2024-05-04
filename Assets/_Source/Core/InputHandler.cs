using UnityEngine;
using Zenject;
using System;

namespace Core
{
  public class InputHandler : MonoBehaviour
  {
    private int _cell;
    private ITurnsReceiver _receiver;
    private GameData _data;
      
      
    // To delete
    private GameService _service;
    [Inject]
    public void Init(IGameDataProvider provider, ITurnsReceiver receiver, GameService service)
    {
      provider.OnNewGameDataReceived += GetGameData;
      _service = service;
      _receiver = receiver;
    }

    private void GetGameData(GameData data)
      => _data = data;
    
    public void SelectCheckerPosition(int id)
    {
      if (_data is null)
        return;
      
      Checker checker = Array.Find(_data.Checkers, x => x.Id == id);
      if (checker is null)
        return;
      
      _cell = checker.Position;
    }
    
    public void TryToMakeTurn(int cubeId)
    {
      if (_data is null)
        return;
      
      const int countDicesForDouble = 4;
      if (_data.DicesResult.Length == countDicesForDouble)
      {
        int firstNonZeroIndex = Array.FindIndex(_data.DicesResult, x => x != 0);
        _receiver.MakeTurn(_cell, firstNonZeroIndex);
      }
      else
        _receiver.MakeTurn(_cell, cubeId);
    }

    public void StartGame()
      => _service.InitGame();
  }
}