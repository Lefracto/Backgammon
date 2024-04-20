using System;

namespace Core
{
  public class DiceD6 : IDice
  {
    public int Value { get; private set; }

    public void Roll()
    {
      Value = new Random()
    }
  }
}