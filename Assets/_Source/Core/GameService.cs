using System.Collections.Generic;
using System.Linq;
using System;
using Core;

public class GameService : ITurnsReceiver, IGameDataProvider
{
  public event Action<GameData> OnNewGameDataReceived;

  private GameData _actualGameData;
  private List<IDice> _dices;

  public void MakeTurn(int cell, int cubeId)
  {
    if (!ValidateTurn(cell, cubeId))
      return;

    int directionOfMove = (_actualGameData.PlayerIdInTurn & 1) == 0 ? 1 : -1;
    int destinationCell = cell + directionOfMove * _dices[cubeId].Value;

    _actualGameData.Field[cell].CountCheckers -= 1;

    if (_actualGameData.Field[cell].CountCheckers == 0)
      _actualGameData.Field[cell].PlayerId = -1;

    _actualGameData.Field[destinationCell].CountCheckers += 1;
    _actualGameData.Field[destinationCell].PlayerId = _actualGameData.PlayerIdInTurn;

    // - ход сделан, нужно изменить кубик 
    // _actualGameData.DicesResult[cubeId].Item2 = 1;

    OnNewGameDataReceived?.Invoke(_actualGameData);
  }

  public void RollDices()
  {
    _dices.ForEach(dice => dice.Roll());
    if (_dices.All(dice => dice.Value == _dices[0].Value))
    {
      _actualGameData.DicesResult = _dices.Select(dice => (dice.Value, 2)).ToList();
      return;
    }

    _actualGameData.DicesResult = _dices.Select(dice => (dice.Value, 1)).ToList();
    OnNewGameDataReceived?.Invoke(_actualGameData);
  }

  private bool ValidateTurn(int cell, int cubeId)
  {
    // Use different if's for better readability and debugging|possibility to write messages

    if (_dices[cubeId].Value == 0)
      return false;

    if (_actualGameData.Field[cell].CountCheckers == 0)
      return false;

    if (!IsCellOccupiedByCurrentPlayer(cell) &&
        !IsDestinationCellValid(cell + _dices[cubeId].Value))
      return false;


    int directionOfMove = (_actualGameData.PlayerIdInTurn & 1) == 0 ? 1 : -1;
    int destinationCell = cell + directionOfMove * _dices[cubeId].Value;

    if (_actualGameData.Field[destinationCell].CountCheckers != 0 &&
        _actualGameData.Field[destinationCell].PlayerId != _actualGameData.PlayerIdInTurn)
    {
      return false;
    }

    // TODO: check for more difficult logic: locking, checkout

    return true;
  }

  private int GetDestinationCell(int cell, int cubeValue)
  {
    int destination = cell + cubeValue;
    if (_actualGameData.PlayerIdInTurn == 0)
    {
      // destination = destination > IsPossibleToRemoveChecker() ? 23 : destination;
    }

    return 0;
  }

  
  private bool IsCellOccupiedByCurrentPlayer(int cell)
    => _actualGameData.Field[cell].PlayerId == _actualGameData.PlayerIdInTurn;
  
  private bool IsDestinationCellValid(int destinationCell)
    => _actualGameData.Field[destinationCell].CountCheckers == 0 ||
       _actualGameData.Field[destinationCell].PlayerId == _actualGameData.PlayerIdInTurn;
}