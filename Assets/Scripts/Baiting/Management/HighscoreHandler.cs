using TMPro;
using UnityEngine;

public class HighscoreHandler : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI highscoreText;
    [SerializeField] private TextMeshProUGUI currentDepthText;
    
    private DepthHandler depthHandler;
    
    private GameStateManager gameStateManager;
    
    private const string HighscoreKey = "Highscore";
    private const string DepthText = " Fathoms";

    private void Start()
    {
        depthHandler = FindFirstObjectByType<DepthHandler>();
        
        gameStateManager = GameStateManager.instance;
        gameStateManager.GameStateChangedEvent += OnGameStateChanged;
    }
    
    private void OnGameStateChanged(GameState newGameState)
    {
        if (newGameState != GameState.Dead) return;
        
        UpdateHighscoreTexts();
    }

    private void UpdateHighscoreTexts()
    {
        var currentDepth = depthHandler.LastFullDepth;
        
        SetCurrentDepthText(currentDepth);

        CheckForHighscoreAndSetTexts(currentDepth);
    }

    private void SetCurrentDepthText(int currentDepth)
    {
        currentDepthText.text = currentDepth + DepthText;
    }

    private void CheckForHighscoreAndSetTexts(int currentDepth)
    {
        if (!PlayerPrefs.HasKey(HighscoreKey))
        {
            SetNewHighscore(currentDepth);
            return;
        }
        
        var formerHighscore = GetHighscore();
        
        if (currentDepth <= formerHighscore)
        {
            SetHighscoreText(formerHighscore);
            return;
        }
        
        SetNewHighscore(currentDepth);
    }

    private void SetNewHighscore(int currentDepth)
    {
        SetHighscore(currentDepth);
        SetHighscoreText(currentDepth);
    }

    private static void SetHighscore(int newHighscore)
    {
        PlayerPrefs.SetInt(HighscoreKey, newHighscore);
        PlayerPrefs.Save();
    }

    private void SetHighscoreText(int currentDepth)
    {
        highscoreText.text = currentDepth + DepthText;
    }
    
    private int GetHighscore() => PlayerPrefs.GetInt(HighscoreKey);
}
