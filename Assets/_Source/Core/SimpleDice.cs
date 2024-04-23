using System;
using UnityEngine;
using Random = System.Random;

namespace Core
{
  /// <summary>
  /// The class of cubes with N different faces that have equal probability of falling.
  /// </summary>
  public class SimpleDice : IDice
  {
    private const int MIN_VALUE = 1;

    private readonly Random _random;
    private readonly int _minValue;
    private readonly int _maxValue;
    
    public SimpleDice(int minValue, int maxValue)
    {
      if (minValue > maxValue)
        throw new ArgumentException("Min value should be less than max value");
      
      if (minValue < MIN_VALUE)
        throw new ArgumentException($"Min value should be greater or equal than {MIN_VALUE}");
      
      Value = 0;
      _maxValue = minValue;
      _maxValue = maxValue;
      _random = new Random();
    }

    public int Value { get; private set; }
    
    public void Roll()
    {
      // + 1 for min -- test
      Value = _random.Next(_minValue + 1, _maxValue + 1);
    }
  }
}