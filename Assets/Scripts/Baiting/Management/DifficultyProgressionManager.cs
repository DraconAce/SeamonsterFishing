using System.Collections;
using UnityEngine;

public class DifficultyProgressionManager : Singleton<DifficultyProgressionManager>
{
    [SerializeField] private DepthHandler depthHandler;
    [SerializeField] private float maxDifficultyFactor = 5f;
    [SerializeField] private float[] difficultyPerDepthThreshold = {0, 0.5f, 1f, 1f};
    
    [Header("Gradually Difficulty Increase")]
    [SerializeField] private float increasePerSecond;

    public float DifficultyFactor { get; set; }
    public float DifficultyFraction => 1 / DifficultyFactor;
    public override bool AddToDontDestroy => false;

    private Coroutine difficultyIncreaseCoroutine;
    private readonly WaitForSeconds waitOne = new(1);

    private void Start()
    {
        DifficultyFactor = difficultyPerDepthThreshold[0];
        
        depthHandler.DepthThresholdChangedEvent += OnDepthThresholdChanged;
    }
    
    private void OnDepthThresholdChanged(int currentDepthThreshold)
    {
        if(currentDepthThreshold >= difficultyPerDepthThreshold.Length) return;
        
        DifficultyFactor = difficultyPerDepthThreshold[currentDepthThreshold];
    }

    public void StartGradualDifficultyIncrease() 
        => difficultyIncreaseCoroutine = StartCoroutine(DifficultyIncreaseRoutine());

    private IEnumerator DifficultyIncreaseRoutine()
    {
        while (true)
        {
            yield return waitOne;
            
            DifficultyFactor += increasePerSecond;
            
            if(DifficultyFactor <= maxDifficultyFactor) continue;
            
            DifficultyFactor = maxDifficultyFactor;
            break;
        }
    }
    
    protected override void OnDestroy()
    {
        base.OnDestroy();
        
        depthHandler.DepthThresholdChangedEvent -= OnDepthThresholdChanged;

        if (difficultyIncreaseCoroutine == null) return;
        
        StopCoroutine(difficultyIncreaseCoroutine);
    }
}