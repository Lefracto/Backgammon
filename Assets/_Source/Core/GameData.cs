using System.Linq;
using System;
using Core;

[Serializable]
public class GameData
{
  private const int START_COUNT_CHECKERS = 30;
  public int PlayerIdInTurn { get; private set; }
  public int[] DicesResult { get; set; }
  public Checker[] Checkers { get; set; }
  public bool MayMoveFromHead { get; set; }

  public int CountCheckers(int playerId)
    => Checkers.Count(checker => checker.PlayerId == playerId);
  
  public GameData()
  {
    Checkers = new Checker[START_COUNT_CHECKERS];
    DicesResult = new[] { 0, 0 };
    MayMoveFromHead = true;
    PlayerIdInTurn = 0;
  }
  
  public void NextPlayer()
  {
    PlayerIdInTurn = PlayerIdInTurn == 0 ? 1 : 0;
    MayMoveFromHead = true;
  }
  
  // TODO: add Service response, last checkers changes, moves counter
}