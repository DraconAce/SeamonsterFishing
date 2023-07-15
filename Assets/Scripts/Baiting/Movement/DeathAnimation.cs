using DG.Tweening;
using UnityEngine;

public class DeathAnimation : MonoBehaviour
{
    [SerializeField] private float animationDuration = 2f;
    [SerializeField] private Ease animationEase;
    
    private BaitingMonsterSingleton monsterSingleton;
    private PlayerSingleton playerSingleton;

    private void Start()
    {
        playerSingleton = PlayerSingleton.instance;
        
        monsterSingleton = BaitingMonsterSingleton.instance;
        monsterSingleton.MonsterStartedKillEvent += StartDeathAnimation;
    }

    private void StartDeathAnimation(Transform attackingMonsterTransform)
    {
        playerSingleton.DisableMovementControls = true;

        StartRotatePlayerCamTween(attackingMonsterTransform);
    }

    private void StartRotatePlayerCamTween(Transform attackingMonsterTransform)
    {
        var playerCamTransform = playerSingleton.PlayerTransform;

        var lookAtMonsterRotation = Quaternion.LookRotation(attackingMonsterTransform.position - playerCamTransform.position);

        playerCamTransform.DORotateQuaternion(lookAtMonsterRotation, animationDuration)
            .SetEase(animationEase);
    }

    private void OnDestroy() => monsterSingleton.MonsterStartedKillEvent -= StartDeathAnimation;
}