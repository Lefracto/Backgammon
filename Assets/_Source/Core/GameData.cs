using System.Collections.Generic;
using System.Linq;
using System;
using Core;

[Serializable]
public class GameData
{
  public int PlayerIdInTurn { get; private set; }
  
  // First item is for dice value, second item is for count of using of this dice
  public List<(int, int)> DicesResult { get; set; }
  public List<FieldSegment> Field { get; set; }
  public bool WasHeadMove { get; set; }
  
  public int CountCheckers(int playerId)
    => Field.Where(position => position.PlayerId == playerId).Sum(position => position.CountCheckers);
  
  public GameData(int countDices, int countFieldPositions)
  {
    DicesResult = Enumerable.Repeat((0, 0), countDices).ToList();
    PlayerIdInTurn = 0;

    Field = new List<FieldSegment>();
    for (int i = 0; i < countFieldPositions; i++)
      Field.Add(new FieldSegment());
  }
  
  public void NextPlayer()
    => PlayerIdInTurn = PlayerIdInTurn == 0 ? 1 : 0;
  
}