using System;
using System.Collections.Generic;
using UnityEngine;

public class DepthCounter : MonoBehaviour
{
    [SerializeField] private int numberOfCardColumns = 4;
    [SerializeField] private float cardSpacing = 0.1f;
    [SerializeField] private Vector2 firstCardPosition;

    [SerializeField] private GameObject depthCardPrefab;
    [SerializeField] private Transform poolParent;

    private DepthHandler depthHandler;
    private PrefabPool depthCardPool;

    private readonly List<DepthCounterCard> counterColumns = new();

    private void Start()
    {
        depthCardPool = PrefabPoolFactory.instance.RequestNewPool(poolParent.gameObject, depthCardPrefab, poolParent);
        
        for(var i = 0; i < numberOfCardColumns; i++)
        {
            var cardPosition = firstCardPosition + Vector2.right * (i * cardSpacing);
            
            var depthCardPoolOb = depthCardPool.RequestInstance(cardPosition, poolParent);
            depthCardPoolOb.TryGetCachedComponent<DepthCounterCard>(out var depthCard);
            
            depthCard.SetInitialNumber(0);
            
            counterColumns.Add(depthCard);
        }

        depthHandler = FindFirstObjectByType<DepthHandler>();
        
        depthHandler.DepthUpdatedEvent += OnDepthUpdated;
    }
    
    private void OnDepthUpdated(int updatedDepth)
    {
        //extract all places in the depth
        var depthRowNumbers = new List<int>();
        
        while(updatedDepth > 0)
        {
            depthRowNumbers.Add(updatedDepth % 10);
            updatedDepth /= 10;
        }

        for (var depthColumn = 0; depthColumn < depthRowNumbers.Count; depthColumn++)
        {
            if (depthColumn >= counterColumns.Count) break;
            
            var depthNumber = depthRowNumbers[depthColumn];
            var depthCard = counterColumns[depthColumn];

            if(depthCard.IsCardShowingNumber(depthNumber)) continue;
            
            counterColumns[depthColumn].RollOverNumber(depthNumber);
        }
    }

    private void OnDestroy() => depthHandler.DepthUpdatedEvent -= OnDepthUpdated;
}