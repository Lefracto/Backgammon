using System;
using UnityEngine;
using UnityEngine.UI;

namespace Presentation
{
  public class HighlightButton : MonoBehaviour
  {
    [SerializeField] private Button _button;
    private Action<int[]> _onMyPositionClick;
    private int[] _diceIds;
    
    public void Init(Action<int[]> onMyPositionClick, int[] diceIds)
    {
      _onMyPositionClick = onMyPositionClick;
      _diceIds = diceIds;
      _button.onClick.AddListener(MakeTurnOnMyPosition);
    }
    
    private void MakeTurnOnMyPosition()
      => _onMyPositionClick.Invoke(_diceIds);
  }
}