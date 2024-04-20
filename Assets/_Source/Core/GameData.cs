using System.Collections.Generic;
using System.Linq;
using System;
using Core;

[Serializable]
public class GameData
{
  public int PlayerIdInTurn { get; set; }

  // First item is for dice value, second item is for count of using of this dice
  public List<(int, int)> DicesResult { get; set; }
  public List<FieldPosition> Field { get; set; }

  public GameData(int countDices, int countFieldPositions)
  {
    DicesResult = Enumerable.Repeat((0, 0), countDices).ToList();
    PlayerIdInTurn = 0;

    Field = new List<FieldPosition>();
    for (int i = 0; i < countFieldPositions; i++)
      Field.Add(new FieldPosition());
  }
}