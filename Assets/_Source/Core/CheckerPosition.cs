using UnityEngine;

namespace Core
{
  public class CheckerPosition
  {
    private readonly int _playerId;
    private (int, int) _convertedIndex;

    public bool IsHeadPosition => _convertedIndex.Item2 == 0;
    public CheckerPosition(int position, int playerId)
    {
      Debug.Log(position + " - pos");
      _convertedIndex.Item1 = position / 12;
      _convertedIndex.Item2 = (position - _convertedIndex.Item1 * 12);
      _playerId = playerId;
      
    }
    public int MoveChecker(int value)
    {
      Debug.Log("val" + value);
      Debug.Log("in -- " + _convertedIndex);
      _convertedIndex.Item2 -= value;
      switch (_convertedIndex.Item2)
      {
        case < 0 when _convertedIndex.Item1 != _playerId:
          _convertedIndex.Item2 += 12;
          _convertedIndex.Item1 = _playerId;
          break;
        case < 0 when _convertedIndex.Item1 == _playerId:
          Debug.Log("out -- " + _convertedIndex.Item2);
          return _convertedIndex.Item2;
      }
      Debug.Log("out --" + _convertedIndex);
      return ToNormalIndex();
    }
    private int ToNormalIndex()
      => _convertedIndex.Item1 * 12 + _convertedIndex.Item2;
  }
}