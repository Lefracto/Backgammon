using System.Collections.Generic;

namespace Core
{
  public interface ITurnValidator
  {
    public int ValidateTurn(int cell, int diceId, GameData data);
    public List<PossibleMove> GetPossibleMoves(int cell, GameData data);
    public bool IsTherePossibleMoves();
  }
}