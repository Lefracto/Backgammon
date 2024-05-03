using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class DiceView : MonoBehaviour
{
  private IGameDataProvider _provider;

  [SerializeField] private Sprite[] _diceSprites;
  [SerializeField] private Image[] _diceImages;
  [SerializeField] private Color _usedDiceColor;

  [Inject]
  public void Init(IGameDataProvider provider)
  {
    _provider = provider;
    _provider.OnNewGameDataReceived += RedrawDice;
  }

  private void RedrawDice(GameData newData)
  {
    for (int i = 0; i < _diceImages.Length; i++)
    {
      _diceImages[i].gameObject.SetActive(true);
      _diceImages[i].color = Color.white;
      if (i < newData.DicesResult.Length)
      {
        if (newData.DicesResult[i] == 0)
          _diceImages[i].color = _usedDiceColor;
        else
          _diceImages[i].sprite = _diceSprites[newData.DicesResult[i] - 1];
      }
      else
        _diceImages[i].gameObject.SetActive(false);
    }
  }
}