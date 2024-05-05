using System.Collections.Generic;

namespace Core
{
  public interface IPossibleMovesProvider
  {
    public IEnumerable<PossibleMove> GetPossibleMoves(int cell, GameData data);
  }
}