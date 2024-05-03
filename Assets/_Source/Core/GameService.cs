using System.Linq;
using System;
using Core;
using Zenject;

public class GameService : ITurnsReceiver, IGameDataProvider
{
  // TODO: make one with validation
  private const int OUT_OF_BOARD = 24;

  public event Action<GameData> OnNewGameDataReceived;
  private readonly ITurnValidator _validator;
  private readonly GameData _actualData;

  [Inject]
  public GameService(ITurnValidator validator, GameData data)
  {
    _validator = validator;
    _actualData = data;
  }

  /// <summary>
  /// Method updates values of DiceResult (from 1 till 6) in actual GameData
  /// </summary>
  private void RollDice()
  {
    int die1 = new Random().Next(1, 7);
    int die2 = new Random().Next(1, 7);
    //int die2 = die1;

    //int die1 = 1;
    //int die2 = 2;
    // Check for double. According to the backgammon rules, if two are the same, it is 4 moves
    _actualData.DicesResult = die1 == die2 ? new[] { die1, die1, die2, die2 } : new[] { die1, die2 };
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
    int destinationCell = _validator.ValidateTurn(cell, cubeId, _actualData);
    if (destinationCell == -1)
    {
      OnNewGameDataReceived?.Invoke(_actualData);
      return;
    }

    // Calculating queue number for checker
    Checker startChecker = _actualData.GetUpperChecker(cell);
    Checker destinationChecker = _actualData.GetUpperChecker(destinationCell);
    startChecker!.QueueNumber = destinationChecker?.QueueNumber + 1 ?? 0;

    // Change position
    startChecker!.Position = destinationCell;
    _actualData.DicesResult[cubeId] = 0;
    _actualData.LastChangedCheckerId = startChecker.Id;

    // Rider lies =(
    _actualData.Response = destinationCell == OUT_OF_BOARD
      ? GameServiceResponse.ValidCheckerExit
      : GameServiceResponse.ValidCheckerMove;

    NextPlayerWithPossibleMoves();
    OnNewGameDataReceived?.Invoke(_actualData);
  }

  /// <summary>
  /// Changes player (rolls dice accordingly) until someone has a possible move.
  /// </summary>
  private void NextPlayerWithPossibleMoves()
  {
    if (IsGameFinished())
    {
      OnNewGameDataReceived?.Invoke(_actualData);
      return;
    }

    if (_actualData.DicesResult.All(value => value == 0))
    {
      _actualData.NextPlayer();
      RollDice();
      return;
    }

    while (_validator.IsTherePossibleMoves() is false)
    {
      OnNewGameDataReceived?.Invoke(_actualData);
      _actualData.NextPlayer();
      RollDice();
    }
  }

  /// <summary>
  /// Method checks if some player leave all checkers from board.
  /// </summary>
  /// <returns>true -- if there are only one player checkers on board. otherwise -- false </returns>
  private bool IsGameFinished()
  {
    var whiteCheckers = _actualData.Checkers.Where(checker => checker.PlayerId == 0);
    var blackCheckers = _actualData.Checkers.Where(checker => checker.PlayerId == 1);

    bool isFinished = whiteCheckers.All(checker => checker.Position == OUT_OF_BOARD) ||
                      blackCheckers.All(checker => checker.Position == OUT_OF_BOARD);

    if (isFinished)
      _actualData.Response = GameServiceResponse.GameFinished;

    return isFinished;
  }

  /// <summary>
  /// Spawn black and white checkers of their bases.
  /// </summary>
  public void InitGame()
  {
    const int startCheckersIndex = 0;
    const int middleCheckersIndex = 15;
    const int lastCheckersIndex = 30;

    const int whiteCheckersSpawn = 23;
    const int blackCheckersSpawn = 11;

    InitializeCheckers(whiteCheckersSpawn, 0, startCheckersIndex, middleCheckersIndex);
    //_actualData.Checkers[14] = new Checker(22, 0)  { QueueNumber = 0 };
    
    InitializeCheckers(blackCheckersSpawn, 1, middleCheckersIndex, lastCheckersIndex);

    /*
    for (int i = 15; i < 30; i++)
    {
      _actualData.Checkers[i] = new Checker(19 - i % 15, 1)  { QueueNumber = 0 };
    }

    _actualData.CountMoves = 3;
    */
    RollDice();
  }

  private void InitializeCheckers(int head, int playerId, int startIndex, int endIndex)
  {
    const int countCheckersForPlayer = 15;
    for (int i = startIndex; i < endIndex; i++)
      _actualData.Checkers[i] = new Checker(head, playerId)
      {
        QueueNumber = i % countCheckersForPlayer
      };
  }
}