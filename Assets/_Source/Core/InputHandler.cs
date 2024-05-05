using UnityEngine;
using Zenject;
using System;
using System.Linq;

namespace Core
{
  public class InputHandler : MonoBehaviour
  {
    private int _cell;
    private GameData _data;

    private ITurnsReceiver _receiver;
    private IPossibleMovesIndicator _possibleMovesIndicator;
    private IPossibleMovesProvider _possibleMovesProvider;


    // To delete
    private GameService _service;

    [Inject]
    public void Init(IGameDataProvider provider, ITurnsReceiver receiver, GameService service,
      IPossibleMovesProvider movesProvider, IPossibleMovesIndicator indicator)
    {
      provider.OnNewGameDataReceived += GetGameData;
      _service = service;
      _receiver = receiver;

      _possibleMovesIndicator = indicator;
      _possibleMovesProvider = movesProvider;
    }

    private void GetGameData(GameData data)
      => _data = data;

    public void SelectCheckerPosition(int id)
    {
      if (_data is null)
      {
        Debug.LogError("Data in InputHandler is null");
        return;
      }

      Checker checker = Array.Find(_data.Checkers, x => x.Id == id);
      if (checker is null)
      {
        Debug.LogError($"Checker with {id} not found by InputHandler");
        return;
      }

      _cell = checker.Position;
      var possibleMoves = _possibleMovesProvider.GetPossibleMoves(_cell, _data);
      _possibleMovesIndicator.HighlightAvailableCheckers(possibleMoves.ToList());
    }

    public void TryToMakeTurn(int[] diceIds)
    {
      if (_data is null)
        return;

      const int countDicesForDouble = 4;
      if (_data.DicesResult.Length == countDicesForDouble)
      {
        //int firstNonZeroIndex = Array.FindIndex(_data.DicesResult, x => x != 0);
        _receiver.MakeTurn(_cell, diceIds);
      }
      else
        _receiver.MakeTurn(_cell, diceIds);
    }

    public void StartGame()
      => _service.InitGame();
  }
}