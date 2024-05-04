using UniRx;

namespace Core
{
  public class Checker
  {
    private static int _lastCheckerId;
    public readonly int PlayerId;
    public int Id { get; set; }
    public int Position { get; set; }
    public int QueueNumber { get; set; }

    public readonly ReactiveProperty<bool> IsTheUpperOnPosition = new();

    public Checker(int position, int playerId)
    {
      Id = _lastCheckerId++;
      Position = position;
      PlayerId = playerId;
    }
  }
}