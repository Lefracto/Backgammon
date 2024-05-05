using System.Collections.Generic;

namespace Core
{
  public struct PossibleMove
  {
    public int Destination { get; set; }
    public List<int> DiceToUse { get; set; }

    public PossibleMove(int destination, IEnumerable<int> diceToUse)
    {
      Destination = destination;
      DiceToUse = new List<int>(diceToUse);
    }
  }
}