using System.Linq;
using System;
using Core;
using ModestTree;

[Serializable]
public class GameData
{
  private const int START_COUNT_CHECKERS = 30;
  public int PlayerIdInTurn { get; private set; }
  public int[] DicesResult { get; set; }

  public Checker[] Checkers { get; set; }

  // transfer to GameService?
  public bool MayMoveFromHead { get; set; }
  public int LastChangedCheckerId { get; set; }
  public int CountMoves { get; set; }

  public GameServiceResponse Response { get; set; }

  public int CountCheckers(int playerId)
    => Checkers.Count(checker => checker.PlayerId == playerId);

  public GameData()
  {
    Checkers = new Checker[START_COUNT_CHECKERS];
    DicesResult = new[] { 0, 0 };
    MayMoveFromHead = true;
    PlayerIdInTurn = 0;
  }

  public Checker GetUpperChecker(int cell)
  {
    // No MaxBy there =( (yet)
    var checkersOnPosition = Checkers.Where(checker => checker.Position == cell).ToList();
    if (checkersOnPosition.IsEmpty())
      return null;

    Checker maxQueueChecker = checkersOnPosition[0];
    foreach (Checker checker in checkersOnPosition.Where(checker => maxQueueChecker.QueueNumber < checker.QueueNumber))
      maxQueueChecker = checker;
    
    return maxQueueChecker;
  }

  public void NextPlayer()
  {
    PlayerIdInTurn = PlayerIdInTurn == 0 ? 1 : 0;
    //LastChangedCheckerId = -1;
    CountMoves++;
    MayMoveFromHead = true;
  }

  public int[] GetUniquePositions()
    => Checkers
      .Where(checker => checker.PlayerId == PlayerIdInTurn)
      .Select(checker => checker.Position)
      .Distinct()
      .ToArray();
}