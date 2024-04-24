namespace Core
{
  public class Checker
  {
    public Checker(int position, int playerId)
    {
      Position = position;
      PlayerId = playerId;
    }
    public int Position { get; set; }
    public int QueueNumber { get; set; }
    public int PlayerId { get; set; }
  }
}