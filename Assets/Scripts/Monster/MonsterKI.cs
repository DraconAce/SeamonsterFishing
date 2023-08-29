using System;
using System.Collections;
using UnityEngine;

public class MonsterKI : MonoBehaviour
{
    public FightMonsterBehaviourTreeManager BehaviourTreeManager { get; private set; }

    private void Awake() => BehaviourTreeManager = GetComponent<FightMonsterBehaviourTreeManager>();

    public event Action<int> StartBehaviourEvent;

    public void ForwardActionRequest(int indexOfRequestedBehaviour) => StartBehaviourEvent?.Invoke(indexOfRequestedBehaviour);

    public void RequestDirectStartOfBehaviour(int behaviourIndex, bool useForceStop = false) => BehaviourTreeManager.RequestSpecificBehaviour(behaviourIndex, useForceStop);
}