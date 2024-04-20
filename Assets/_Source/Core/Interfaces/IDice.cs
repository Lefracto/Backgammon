namespace Core
{
  public interface IDice
  {
    public int Value { get; }
    public void Roll();
  }
}