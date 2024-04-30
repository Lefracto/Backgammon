namespace Core
{
  public interface ITurnValidator
  {
    public int ValidateTurn(int cell, int cubeId, GameData data);
    public bool IsTherePossibleMoves();
  }
}