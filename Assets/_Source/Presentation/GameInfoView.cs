﻿using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using Zenject;
using TMPro;

namespace Presentation
{
  public class GameInfoView : MonoBehaviour
  {
    [SerializeField] private string _blackPlayerText;
    [SerializeField] private string _whitePlayerText;

    [Space(15)] [SerializeField] private Image _dice1;
    [SerializeField] private Image _dice2;

    [Space(10)] [SerializeField] private List<Sprite> _diceSprites;

    [Space(15)] [SerializeField] private TMP_Text _messageText;
    [SerializeField] private TMP_Text _currentPlayerText;
    [SerializeField] private TMP_Text _countMovesText;
    [SerializeField] private TMP_Text _mayHeadMoveText;


    [Inject]
    public void Init(IGameDataProvider provider)
      => provider.OnNewGameDataReceived += RedrawInfo;


    private void RedrawInfo(GameData data)
    {
      string text = data.PlayerIdInTurn == 0 ? _whitePlayerText : _blackPlayerText;
      _currentPlayerText.text = text;

      if (data.DicesResult[0] != 0)
        _dice1.sprite = _diceSprites[data.DicesResult[0] - 1];

      if (data.DicesResult[1] != 0)
        _dice2.sprite = _diceSprites[data.DicesResult[1] - 1];

      _messageText.text = data.Response.ToString();
      _countMovesText.text = data.CountMoves.ToString();
      _mayHeadMoveText.text = data.MayMoveFromHead ? "Да" : "Нет(кроме первого хода)";
    }
  }
}