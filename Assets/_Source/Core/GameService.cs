using System.Collections.Generic;
using System.Linq;
using System;
using Core;

public class GameService : ITurnsReceiver, IGameDataProvider
{
  private static readonly (int, int) white_house = (0, 5);
  private static readonly (int, int) black_house = (12, 17);

  private const int OUT_OF_BOARD = 24;

  private const int WHITE_HEAD = 23;
  private const int BLACK_HEAD = 11;

  public event Action<GameData> OnNewGameDataReceived;
  private readonly GameData _actualData;

  public GameService()
    => _actualData = new GameData();

  private void RollDices()
  {
    int dice1 = new Random().Next(1, 7);
    int dice2 = new Random().Next(1, 7);

    _actualData.DicesResult = dice1 == dice2 ? new[] { dice1, dice1, dice2, dice2 } : new[] { dice1, dice2 };
    OnNewGameDataReceived?.Invoke(_actualData);
  }

  /// <summary>
  /// Public method for trying to make a turn. It calls validation.
  /// </summary>
  /// <param name="cell"> The cell from which try to move the checker. </param>
  /// <param name="cubeId"> ID of the cube used for the move. </param>
  public void MakeTurn(int cell, int cubeId)
  {
    // Validation
    if (ValidateTurn(cell, cubeId, out int destinationCell) is false)
      return;

    // Calculating queue number for checker
    Checker startChecker = _actualData.Checkers.FirstOrDefault(checker => checker.Position == cell);
    var checkersOnDestination = _actualData.Checkers.Where(checker => checker.Position == destinationCell).ToArray();

    if (checkersOnDestination.Any())
      startChecker!.QueueNumber = checkersOnDestination.Max(checker => checker.QueueNumber) + 1;
    else
      startChecker!.QueueNumber = 0;

    // Check for head move
    if (startChecker.Position is WHITE_HEAD or BLACK_HEAD)
    {
      // TODO: add check for the first move
      _actualData.MayMoveFromHead = false;
    }

    // Change position
    startChecker!.Position = destinationCell;
    _actualData.DicesResult[cubeId] = 0;


    // Update dices and switch the player
    if (_actualData.DicesResult.All(dice => dice == 0))
    {
      _actualData.NextPlayer();
      RollDices();
    }

    OnNewGameDataReceived?.Invoke(_actualData);
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

    destination = GetDestinationCell(cell, _actualData.DicesResult[cubeId]);

    if (cell is BLACK_HEAD or WHITE_HEAD &&
        _actualData.MayMoveFromHead is false)
      return false;

    if (IsOkayLocking(cell, destination) is false)
      return false;

    return destination != -1;
  }

  private bool IsOkayLocking(int cell, int destination)
  {
    // TODO: add lock check.
    // проверка на то, что после запираения есть хоть одна фишка соперника
    return true;
  }

  /// <summary>
  /// The method calculates the index on the segment of the field where you want to move
  /// the checker based on the value of the cube and checker output rules. Only correct parameters!!! 
  /// </summary>
  /// <param name="cell"> The cell from which move the checker. </param>
  /// <param name="diceValue"> Value on dice. </param>
  /// <returns> Index of destination segment. </returns>
  private int GetDestinationCell(int cell, int diceValue)
  {
    int destination = MoveChecker(cell, diceValue);

    if (destination < 0)
    {
      if (IsAllCheckersInHome() is false)
        return -1;

      return destination == -1 || IsTheNearestToOutChecker(destination) ? OUT_OF_BOARD : -1;
    }

    Checker checker = _actualData.Checkers.FirstOrDefault(checker => checker.Position == destination);
    if (checker != null && checker.PlayerId != _actualData.PlayerIdInTurn)
      return -1;

    return destination;
  }

  private bool IsGameFinished()
  {
    var whiteCheckers = _actualData.Checkers.Where(checker => checker.PlayerId == 0);
    var blackCheckers = _actualData.Checkers.Where(checker => checker.PlayerId == 1);

    return whiteCheckers.All(checker => checker.Position == OUT_OF_BOARD) ||
           blackCheckers.All(checker => checker.Position == OUT_OF_BOARD);
  }

  public void InitGame()
  {
    // TODO: Should I make a method for deleting duplicating of code?
    // TODO: Make constants

    // Initialising white checkers
    for (int i = 0; i < 15; i++)
    {
      _actualData.Checkers[i] = new Checker(WHITE_HEAD, 0)
      {
        QueueNumber = i
      };
    }

    // Initialising black checkers
    for (int i = 15; i < 30; i++)
    {
      _actualData.Checkers[i] = new Checker(BLACK_HEAD, 1)
      {
        QueueNumber = i % 15
      };
    }

    RollDices();
  }

  private (int, int) GetHouse()
    => _actualData.PlayerIdInTurn == 0 ? white_house : black_house;
  
  private bool IsTheNearestToOutChecker(int currentDistance)
  {
    (int, int) house = GetHouse();
    var distances = new List<int>();
    for (int i = house.Item1; i <= house.Item2; i++)
    {
      int j;
      for (j = 0; j < _actualData.DicesResult.Length; j++)
      {
        int t = _actualData.DicesResult[j];
        if (t != 0)
          distances.Add(MoveChecker(i, t));
      }
    }

    return distances.Any(distance => distance < currentDistance) is false;
  }

  private bool IsValidStartCell(int cell)
  {
    if (cell is < 0 or > 23)
      return false;

    Checker checker = _actualData.Checkers.FirstOrDefault(checker => checker.Position == cell);
    if (checker is null)
      return false;

    return checker.PlayerId == _actualData.PlayerIdInTurn;
  }

  private bool IsValidCube(int cubeId)
  {
    if (cubeId < 0 || cubeId > _actualData.DicesResult.Length)
      return false;

    return _actualData.DicesResult[cubeId] != 0;
  }

  private bool IsAllCheckersInHome()
  {
    (int, int) house = GetHouse();
    for (int i = 0; i < 30; i++)
    {
      if (_actualData.Checkers[i].PlayerId == _actualData.PlayerIdInTurn &&
          (_actualData.Checkers[i].Position < house.Item1 || _actualData.Checkers[i].Position > house.Item2))
        return false;
    }

    return true;
  }

  private int MoveChecker(int startCell, int value)
  {
    (int, int) convertedIndex;
    convertedIndex.Item1 = startCell / 12;
    convertedIndex.Item2 = startCell - convertedIndex.Item1 * 12;

    convertedIndex.Item2 -= value;
    switch (convertedIndex.Item2)
    {
      case < 0 when convertedIndex.Item1 != _actualData.PlayerIdInTurn:
        convertedIndex.Item2 += 12;
        convertedIndex.Item1 = _actualData.PlayerIdInTurn;
        break;
      case < 0 when convertedIndex.Item1 == _actualData.PlayerIdInTurn:
        return convertedIndex.Item2;
    }

    return convertedIndex.Item1 * 12 + convertedIndex.Item2;
  }

  private bool IsTherePossibleMove()
  {
    var currentPlayerCheckers =
      _actualData.Checkers.Where(checker => checker.PlayerId == _actualData.PlayerIdInTurn).ToArray();

    int dice1 = _actualData.DicesResult[0];
    int dice2 = _actualData.DicesResult[1];
    
    for (int i = 0; i < currentPlayerCheckers.Length; i++)
    {
      int move1 = GetDestinationCell(_actualData.Checkers[i].Position, dice1);
      int move2 = GetDestinationCell(_actualData.Checkers[i].Position, dice2);

      if (move1 != -1 || move2 != -1)
        return true;
    }

    return false;
  }
}