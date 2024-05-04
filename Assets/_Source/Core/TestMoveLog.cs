namespace Core
{
  public struct TestMoveLog
  {
    public TestMoveLog(int checkerId, int previousPosition, bool headMoveStatus, int diceValue)
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