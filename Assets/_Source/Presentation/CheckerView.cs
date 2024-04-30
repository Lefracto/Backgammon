using Core;
using UnityEngine;
using UnityEngine.UI;
using static DG.Tweening.DOTween;

namespace Presentation
{
  [RequireComponent(typeof(Button))]
  public class CheckerView : MonoBehaviour
  {
    private Checker _checker;

    public void Init(Checker checker)
      => _checker = checker;
    
    public void TransferChecker()
    {
      
    }
  }
}