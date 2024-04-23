using System.Collections.Generic;
using System.Linq;
using System;
using Core;
using Zenject;

public class GameService : ITurnsReceiver, IGameDataProvider
{
  private static readonly (int, int) white_house = (0, 5);
  private static readonly (int, int) black_house = (12, 17);

  private const int OUT_OF_BOARD = 24;

  public event Action<GameData> OnNewGameDataReceived;

  private readonly GameData _actualData;
  private readonly List<IDice> _dices;

  [Inject]
  public GameService(IEnumerable<IDice> dices, int countFieldPositions)
  {
    _dices = dices.ToList();
    _actualData = new GameData(_dices.Count, countFieldPositions);
  }

  public void RollDices()
  {
    _dices.ForEach(dice => dice.Roll());
    if (_dices.All(dice => dice.Value == _dices[0].Value))
    {
      _actualData.DicesResult = _dices.Select(dice => (dice.Value, 2)).ToList();
      return;
    }

    _actualData.DicesResult = _dices.Select(dice => (dice.Value, 1)).ToList();
    OnNewGameDataReceived?.Invoke(_actualData);
  }

  /// <summary>
  /// Public method for trying to make a turn. It calls validation.
  /// </summary>
  /// <param name="cell"> The cell from which try to move the checker. </param>
  /// <param name="cubeId"> ID of the cube used for the move. </param>
  public void MakeTurn(int cell, int cubeId)
  {
    if (!ValidateTurn(cell, cubeId, out int destinationCell))
      return;

    MoveChecker(cell, destinationCell);
    OnNewGameDataReceived?.Invoke(_actualData);
  }

  /// <summary>
  /// Method for moving checker for some value. It receives only correct arguments!
  /// </summary>
  /// <param name="startCell"> The cell from which try to move the checker </param>
  /// <param name="destinationCell"> The value to which we move the checkbox </param>
  private void MoveChecker(int startCell, int destinationCell)
  {
    _actualData.Field[startCell].CountCheckers--;
    if (_actualData.Field[startCell].CountCheckers == 0)
      _actualData.Field[startCell].PlayerId = -1;

    if (destinationCell == OUT_OF_BOARD)
      return;

    _actualData.Field[destinationCell].CountCheckers++;
    if (_actualData.Field[destinationCell].CountCheckers == 1)
      _actualData.Field[destinationCell].PlayerId = _actualData.PlayerIdInTurn;
  }

  /// <summary>
  /// Method to validate a move, returns true and destination cell if the move is correct, otherwise -- false.
  /// </summary>
  /// <param name="cell"> The cell from which try to move the checker. </param>
  /// <param name="cubeId"> ID of the cube used for the move. </param>
  /// <param name="destination"> Variable to transmit via out if the code is correct. </param>
  /// <returns> True -- turn is valid, otherwise -- false. </returns>
  private bool ValidateTurn(int cell, int cubeId, out int destination)
  {
    destination = -1;
    if ((IsValidStartCell(cell) && IsValidCube(cubeId)) is false)
      return false;

    destination = GetDestinationCell(cell, _dices[cubeId].Value);
    
    // TODO: add lock check.
    
    return destination != -1;
  }

  private bool IsOkayLocking()
  {
    return false;
  }
  
  /// <summary>
  /// The method calculates the index on the segment of the field where you want to move
  /// the checker based on the value of the cube and checker output rules.
  /// </summary>
  /// <param name="cell"> The cell from which move the checker. </param>
  /// <param name="diceValue"> Value on dice. </param>
  /// <returns> Index of destination segment. </returns>
  private int GetDestinationCell(int cell, int diceValue)
  {
    CheckerPosition position = new(cell, _actualData.PlayerIdInTurn);
    int destination = position.MoveChecker(diceValue);

    if (destination < 0)
    {
      if (IsAllCheckersInHome() is false)
        return -1;

      return destination == -1 || IsTheNearestToOutChecker(destination) ? OUT_OF_BOARD : -1;
    }

    if (_actualData.Field[destination].CountCheckers != 0 &&
        _actualData.Field[destination].PlayerId != _actualData.PlayerIdInTurn)
      return -1;

    return destination;
  }

  private bool IsGameFinished()
  {
    // TODO: Check for 0 checkers one of players on board. 
    return false;
  }

  public void InitGame()
  {
    // TODO: finish it

    _actualData.Field[23].CountCheckers = 3;
    _actualData.Field[23].PlayerId = 0;

    //_actualData.NextPlayer();
    //_actualData.Field[11].CountCheckers = 3;
    //_actualData.Field[11].PlayerId = 1;
    
    OnNewGameDataReceived!.Invoke(_actualData);
  }

  private (int, int) GetHouse()
    => _actualData.PlayerIdInTurn == 0 ? white_house : black_house;

  // Validation methods
  private bool IsTheNearestToOutChecker(int currentDistance)
  {
    (int, int) house = GetHouse();
    var distances = new List<int>();
    for (int i = house.Item1; i <= house.Item2; i++)
    {
      for (int j = 0; j < _actualData.DicesResult.Count; j++)
      {
        if (_actualData.DicesResult[j].Item2 != 0)
          distances.Add(
            new CheckerPosition(i, _actualData.PlayerIdInTurn).MoveChecker(_actualData.DicesResult[j].Item1));
      }
    }

    return !distances.Any(distance => distance < currentDistance);
  }

  private bool IsValidStartCell(int cell)
  {
    if (cell < 0 || cell > _actualData.Field.Count)
      return false;

    if (_actualData.Field[cell].CountCheckers == 0)
      return false;

    return _actualData.Field[cell].PlayerId == _actualData.PlayerIdInTurn;
  }

  private bool IsValidCube(int cubeId)
  {
    if (cubeId < 0 || cubeId > _dices.Count)
      return false;

    // Check if dice was rolled and if it was used
    if (_dices[cubeId].Value == 0 || _actualData.DicesResult[cubeId].Item2 == 0)
      return false;

    return true;
  }

  private bool IsAllCheckersInHome()
  {
    (int, int) house = GetHouse();
    for (int i = 0; i < _actualData.Field.Count; i++)
    {
      if (_actualData.Field[i].PlayerId == _actualData.PlayerIdInTurn && (i < house.Item1 || i > house.Item2))
        return false;
    }

    return true;
  }

  private bool IsTherePossibleMove()
  {
    return false;
  }
}