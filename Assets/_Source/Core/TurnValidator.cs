using System;
using System.Collections.Generic;
using System.Linq;
using ModestTree;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Core
{
  public class TurnValidator : ITurnValidator
  {
    private static readonly (int, int) white_house = (0, 5);
    private static readonly (int, int) black_house = (12, 17);

    private const int OUT_OF_BOARD = 24;
    private const int WHITE_HEAD = 23;
    private const int BLACK_HEAD = 11;

    private GameServiceResponse ActualResponse { get; set; }
    private GameData _actualData;

    /// <summary>
    /// Method to validate a move, returns true and destination cell if the move is correct, otherwise -- false.
    /// </summary>
    /// <param name="cell"> The cell from which try to move the checker. </param>
    /// <param name="cubeId"> ID of the cube used for the move. </param>
    /// <param name="data"> .... </param>
    /// <returns> True -- turn is valid, otherwise -- false. </returns>
    public int ValidateTurn(int cell, int cubeId, GameData data)
    {
      _actualData = data;
      int possibleDestination = -1;
      Func<bool>[] validations =
      {
        () => IsValidStartCell(cell),
        () => IsValidCube(cubeId),
        () => IsOkayHeadMove(cell),
        () => IsOkayLocking(cell, possibleDestination = GetDestinationCell(cell, data.DicesResult[cubeId])),
        () => IsMoveFollowingCompleteness()
      };

      if (validations.Any(validation => validation() is false))
        data.Response = ActualResponse;

      return possibleDestination;
    }

    /// <summary>
    /// The method calculates the index on the segment of the field where you want to move
    /// the checker based on the value of the cube and checker output rules. Only correct parameters!!! 
    /// </summary>
    /// <param name="cell"> The cell from which move the checker. </param>
    /// <param name="diceValue"> Value on dice. </param>
    /// <returns> Index of destination segment. </returns>
    private int GetDestinationCell(int cell, int diceValue)
    {
      int destination = GetDestinationSegmentIndex(cell, diceValue);

      if (destination < 0)
      {
        if (IsAllCheckersInHome() is false)
          return -1;

        return destination == -1 || IsTheNearestToOutChecker(destination) ? OUT_OF_BOARD : -1;
      }

      Checker checker = _actualData.Checkers.FirstOrDefault(checker => checker.Position == destination);
      if (checker == null || checker.PlayerId == _actualData.PlayerIdInTurn)
        return destination;

      ActualResponse = GameServiceResponse.DestinationIsOccupied;
      return -1;
    }

    private bool IsOkayHeadMove(int startCell)
    {
      if (startCell != WHITE_HEAD && startCell != BLACK_HEAD)
        return true;

      if (_actualData.CountMoves is 0 or 1)
        return CheckIfSingleCheckerCanMove();

      if (!_actualData.MayMoveFromHead)
      {
        ActualResponse = GameServiceResponse.IncorrectAttemptToMoveFromHead;
        return false;
      }

      _actualData.MayMoveFromHead = false;
      return true;
    }

    private bool CheckIfSingleCheckerCanMove()
    {
      var checkers = _actualData.Checkers
        .Where(checker => checker.PlayerId == _actualData.PlayerIdInTurn &&
                          checker.Position != WHITE_HEAD &&
                          checker.Position != BLACK_HEAD)
        .ToArray();

      switch (checkers.Length)
      {
        case 0:
          _actualData.MayMoveFromHead = false;
          return true;
        case > 1:
          ActualResponse = GameServiceResponse.IncorrectAttemptToMoveFromHead;
          return false;
        default:
          return CanFullyMoveFromCurrentPosition(checkers[0]);
      }
    }

    private bool CanFullyMoveFromCurrentPosition(Checker checker)
    {
      int start = checker.Position;
      int[] nonZeroDicesValues = _actualData.DicesResult.Where(dice => dice != 0).ToArray();

      foreach (int diceValue in nonZeroDicesValues)
      {
        int destination = GetDestinationSegmentIndex(start, diceValue);
        Checker checkerOnDestination = _actualData.Checkers.FirstOrDefault(c => c.Position == destination);

        if (checkerOnDestination != null && checkerOnDestination.PlayerId != _actualData.PlayerIdInTurn)
          return true;

        start = destination;
      }

      return false;
    }

    /// <summary>
    /// Method checks existing of lock and may player make it. Only correct data!
    /// </summary>
    /// <param name="cell">Next move start cell</param>
    /// <param name="destination">Hypothetical move destination</param>
    /// <returns>true -- correct move, otherwise -- false</returns>
    private bool IsOkayLocking(int cell, int destination)
    {
      int streakStart = FindLock(cell, destination);
      if (streakStart == -1)
        return true;

      for (int i = streakStart; i < 23; i++)
      {
        Checker checker = _actualData.Checkers.FirstOrDefault(checker => checker.Position == i);
        if (checker != null && checker.PlayerId != _actualData.PlayerIdInTurn)
          return true;
      }

      ActualResponse = GameServiceResponse.IncorrectAttemptToLock;
      return false;
    }

    /// <summary>
    /// Method for searching the streak of 6 checkers in the field, if you make the next move. It gets only correct data!
    /// </summary>
    /// <param name="cell">Start cell, for making possible move</param>
    /// <param name="destination">Destination for possible move</param>
    /// <returns>Index of streak's start or -1 if it does not exist.</returns>
    private int FindLock(int cell, int destination)
    {
      const int streakLength = 6;
      const int lastFieldIndex = 23;

      Checker startChecker = _actualData.Checkers.First(checker => checker.Position == cell);
      startChecker!.Position = destination;

      int i = destination - streakLength;
      for (i = Math.Max(0, i); i <= destination; i++)
      {
        int streak = 0;
        for (int j = i; j < i + streakLength; j++)
        {
          if (i > lastFieldIndex) break;
          Checker currentChecker = _actualData.Checkers.FirstOrDefault(checker1 => checker1.Position == j);
          if (currentChecker is null || currentChecker.PlayerId != _actualData.PlayerIdInTurn)
          {
            i = j;
            break;
          }

          streak++;
        }

        if (streak != streakLength) continue;
        startChecker!.Position = cell;
        return i + streakLength;
      }

      startChecker!.Position = cell;
      return -1;
    }

    private bool IsTheNearestToOutChecker(int currentDistance)
    {
      (int, int) house = GetHouse();
      var distances = new List<int>();
      for (int i = house.Item1; i <= house.Item2; i++)
      {
        int j;
        for (j = 0; j < _actualData.DicesResult.Length; j++)
        {
          int t = _actualData.DicesResult[j];
          Checker possibleChecker = _actualData.Checkers.FirstOrDefault(checker =>
            checker.Position == i && checker.PlayerId == _actualData.PlayerIdInTurn);
          if (t != 0 && possibleChecker is not null)
            distances.Add(GetDestinationSegmentIndex(i, t));
        }
      }

      // TODO:  
      bool isThisNearestChecker =
        distances.Any(distance => distance > currentDistance) is false;

      if (isThisNearestChecker is false)
        ActualResponse = GameServiceResponse.NotTheShortestWay;

      return isThisNearestChecker;
    }

    private bool IsValidStartCell(int cell)
    {
      const int lastFieldIndex = 23;
      if (cell is < 0 or > lastFieldIndex)
      {
        ActualResponse = GameServiceResponse.UnexpectedError;
        return false;
      }

      Checker checker = _actualData.Checkers.FirstOrDefault(checker => checker.Position == cell);
      if (checker is null)
      {
        ActualResponse = GameServiceResponse.IncorrectStartCell;
        return false;
      }

      // TODO: Make one condition
      bool isPlayersChecker = checker.PlayerId == _actualData.PlayerIdInTurn;
      if (isPlayersChecker)
        ActualResponse = GameServiceResponse.IncorrectStartCell;

      return isPlayersChecker;
    }

    private bool IsValidCube(int cubeId)
    {
      if (cubeId < 0 || cubeId > _actualData.DicesResult.Length)
      {
        ActualResponse = GameServiceResponse.UnexpectedError;
        return false;
      }

      bool isThisDiceUsed = _actualData.DicesResult[cubeId] == 0;

      if (isThisDiceUsed)
        ActualResponse = GameServiceResponse.AttemptToUseUsedDice;

      return !isThisDiceUsed;
    }

    private bool IsAllCheckersInHome()
    {
      const int countCheckers = 30;
      (int, int) house = GetHouse();
      for (int i = 0; i < countCheckers; i++)
      {
        if (_actualData.Checkers[i].PlayerId != _actualData.PlayerIdInTurn ||
            (_actualData.Checkers[i].Position >= house.Item1 &&
             _actualData.Checkers[i].Position <= house.Item2) ||
            _actualData.Checkers[i].Position == OUT_OF_BOARD)
          continue;

        ActualResponse = GameServiceResponse.NotAllCheckersAtHome;
        return false;
      }

      return true;
    }


    /// <summary>
    /// Calculates the index of the segment where the checker will be moved.
    /// </summary>
    /// <param name="startCell">Current checker cell</param>
    /// <param name="value">Value of dice, which is going to be used</param>
    /// <returns>Index of field's segment</returns>
    private int GetDestinationSegmentIndex(int startCell, int value)
    {
      const int halfOfField = 12;
      (int, int) convertedIndex;
      convertedIndex.Item1 = startCell / halfOfField;
      convertedIndex.Item2 = startCell - convertedIndex.Item1 * halfOfField;

      convertedIndex.Item2 -= value;
      switch (convertedIndex.Item2)
      {
        case < 0 when convertedIndex.Item1 != _actualData.PlayerIdInTurn:
          convertedIndex.Item2 += halfOfField;
          convertedIndex.Item1 = _actualData.PlayerIdInTurn;
          break;
        case < 0 when convertedIndex.Item1 == _actualData.PlayerIdInTurn:
          return convertedIndex.Item2;
      }

      return convertedIndex.Item1 * halfOfField + convertedIndex.Item2;
    }

    public bool IsTherePossibleMoves()
    {
      // TODO: Fix it.
      bool mayMoveFromHead = _actualData.MayMoveFromHead;

      int[] uniquePositions = _actualData.Checkers
        .Where(checker => checker.PlayerId == _actualData.PlayerIdInTurn)
        .Select(checker => checker.Position)
        .Distinct()
        .ToArray();

      int[] nonZeroDiceIndexes = _actualData.DicesResult
        .Where(dice => dice != 0)
        .Distinct()
        .Select(i => _actualData.DicesResult.IndexOf(i))
        .ToArray();

      foreach (int position in uniquePositions)
      {
        foreach (int diceId in nonZeroDiceIndexes)
        {
          // Fast check move for correctness. It does it for each position &  
          int destination = GetDestinationCell(position, _actualData.DicesResult[diceId]);
          if (!IsOkayHeadMove(position) || !IsOkayLocking(position, destination) || destination == -1) continue;
          _actualData.MayMoveFromHead = mayMoveFromHead;
          return true;
        }
      }

      _actualData.MayMoveFromHead = mayMoveFromHead;
      ActualResponse = GameServiceResponse.NoMoves;
      Debug.Log("No move");
      return false;
    }

    /// <summary>
    /// Method checks the completeness rule for move.
    /// Completeness rule: player must use the biggest possible count of points. 
    /// </summary>
    /// <returns>true -- if maintain the rule, otherwise -- false</returns>
    private bool IsMoveFollowingCompleteness()
    {
      // Общее описание необходимого кода:
      // Если это последний не использованный кубик, то сразу да
      // Потом проверка на то, что после хода возможен ход оставшимся кубиком, если да - сразу да
      // В случае если этот ход блокирует оставшийся/оставшиеся, проверить, есть ли полный ход, если да -- сразу нет
      // Если полного хода нет, то проверить на то, используется ли наибольшее значение (в плане в итоге, а не в моменте)
      return true;
    }

    private bool IsFullMovePossible()
    {
      return true;
    }

    private (int, int) GetHouse()
      => _actualData.PlayerIdInTurn == 0 ? white_house : black_house;
  }
}