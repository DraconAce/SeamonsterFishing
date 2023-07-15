using System;
using System.Globalization;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class DepthCounterCard : MonoBehaviour, IPoolObject
{
    [Serializable]
    private struct DepthCardRepresentation
    {
        public RectTransform cardRectTransform;
        public TextMeshProUGUI cardText;
    }
    
    [SerializeField] private DepthCardRepresentation currentlyShownCard;
    [SerializeField] private DepthCardRepresentation currentlyHiddenCard;

    [SerializeField] private float rolloverTime;

    private Vector2 shownPosition;
    private Vector2 hiddenPosition;
    private Vector2 outOfFramePosition;
    
    private Sequence rollOverSequence;
    
    public int CurrentNumber { get; private set; }
    
    private void Start()
    {
        shownPosition = currentlyShownCard.cardRectTransform.anchoredPosition;
        hiddenPosition = currentlyHiddenCard.cardRectTransform.anchoredPosition;

        DetermineOutOfFramePosition();
    }

    private void DetermineOutOfFramePosition()
    {
        outOfFramePosition = shownPosition;
        var distanceToMove = Mathf.Abs(shownPosition.y - hiddenPosition.y);

        outOfFramePosition.y += distanceToMove;
    }
    
    public bool IsCardShowingNumber(int number) => CurrentNumber == number;

    public void SetInitialNumber(int number)
    {
        CurrentNumber = number;
        currentlyShownCard.cardText.text = number.ToString();
    }

    public void RollOverNumber(int number)
    {
        CurrentNumber = number;
        
        rollOverSequence?.Complete(true);
        
        currentlyHiddenCard.cardText.text = number.ToString();
        
        StartRollOverNumbersTween();
    }

    private void StartRollOverNumbersTween()
    {
        rollOverSequence = DOTween.Sequence();
        
        var shownCardRect = currentlyShownCard.cardRectTransform;
        var hiddenCardRect = currentlyHiddenCard.cardRectTransform;
        
        rollOverSequence.Append(shownCardRect.DOAnchorPos(outOfFramePosition, rolloverTime));

        rollOverSequence.Join(hiddenCardRect.DOAnchorPos(shownPosition, rolloverTime));
        
        rollOverSequence.OnComplete(() =>
        {
            shownCardRect.anchoredPosition = hiddenPosition;
            (currentlyShownCard, currentlyHiddenCard) = (currentlyHiddenCard, currentlyShownCard);
        });
    }

    public PoolObjectContainer ContainerOfObject { get; set; }
    public void ResetInstance() { }

    public void OnReturnInstance()
    {
        currentlyShownCard.cardText.text = "";
        currentlyShownCard.cardText.text = "";
        
        currentlyShownCard.cardRectTransform.anchoredPosition = shownPosition;
        currentlyHiddenCard.cardRectTransform.anchoredPosition = hiddenPosition;
    }
}