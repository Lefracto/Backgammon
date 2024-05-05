using System.Collections.Generic;

namespace Core
{
  public interface IPossibleMovesIndicator
  {
    public void HighlightAvailableCheckers(List<PossibleMove> availablePositions);
  }
}