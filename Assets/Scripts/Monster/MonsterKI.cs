using System;
using System.Collections;
using UnityEngine;

public class MonsterKI : MonoBehaviour
{
    public FightMonsterBehaviourTreeManager BehaviourTreeManager { get; set; }

    public event Action<int> StartBehaviourEvent;
    public event Action<int> RequestSpecificBehaviourEvent;

    public void ForwardActionRequest(int indexOfRequestedBehaviour) => StartBehaviourEvent?.Invoke(indexOfRequestedBehaviour);

    public void RequestDirectStartOfBehaviour(int behaviourIndex) => RequestSpecificBehaviourEvent?.Invoke(behaviourIndex);
}