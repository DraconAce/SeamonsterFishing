using System;
using System.Collections;
using UnityEngine;

public class MonsterKI : MonoBehaviour
{
    public FightMonsterBehaviourTreeManager BehaviourTreeManager { get; private set; }

    private void Awake() => BehaviourTreeManager = GetComponent<FightMonsterBehaviourTreeManager>();

    public event Action<int> StartBehaviourEvent;
    public event Action<int> RequestSpecificBehaviourEvent;

    public void ForwardActionRequest(int indexOfRequestedBehaviour) => StartBehaviourEvent?.Invoke(indexOfRequestedBehaviour);

    public void RequestDirectStartOfBehaviour(int behaviourIndex) => RequestSpecificBehaviourEvent?.Invoke(behaviourIndex);
}