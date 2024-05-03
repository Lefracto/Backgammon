namespace Core
{
  public struct MoveInfo
  {
    public MoveInfo(int checkerId, int previousPosition, bool headMoveStatus, int diceValue)
    {
      ChangedCheckerId = checkerId;
      PreviousPosition = previousPosition;
      HeadMoveStatus = headMoveStatus;
      DiceValue = diceValue;
    }

    public int ChangedCheckerId { get; }
    public int PreviousPosition { get; }
    public bool HeadMoveStatus { get; }
    public int DiceValue { get; set; }
  }
}