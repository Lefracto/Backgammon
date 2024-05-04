namespace Core
{
  public struct PossibleMove
  {
    public int Destination { get; set; }
    public int[] DiceToUse { get; set; }

    public PossibleMove(int destination, int[] diceToUse)
    {
      Destination = destination;
      DiceToUse = diceToUse;
    }
  }
}