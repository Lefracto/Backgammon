namespace Core
{
  public interface ITurnValidator
  {
    public int ValidateTurn(int cell, int diceId, GameData data);
    public bool IsTherePossibleMoves();
  }
}