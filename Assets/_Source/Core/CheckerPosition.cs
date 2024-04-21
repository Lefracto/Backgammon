namespace Core
{
  public class CheckerPosition
  {
    private readonly int _playerId;
    private (int, int) _convertedIndex;

    public CheckerPosition(int position, int playerId)
    {
      _convertedIndex.Item1 = position / 12;
      _convertedIndex.Item2 = (position - 12) % 12;
      _playerId = playerId;
    }
    public int MoveChecker(int value)
    {
      _convertedIndex.Item2 -= value;
      switch (_convertedIndex.Item2)
      {
        case < 0 when _convertedIndex.Item1 == _playerId:
          // TODO: Make a constant
          _convertedIndex.Item2 += 12;
          _convertedIndex.Item1 = _playerId == 0 ? 1 : 0;
          break;
        case < 0 when _convertedIndex.Item1 == _playerId:
          return _convertedIndex.Item2;
      }
      
      return ToNormalIndex();
    }
    private int ToNormalIndex()
      => _convertedIndex.Item1 * 12 + _convertedIndex.Item2;
  }
}