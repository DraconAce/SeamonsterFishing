using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : Singleton<SceneController>
{
    [SerializeField] private List<LevelRepresentation> scenesList;

    private InputManager inputManager;
    private GameStateManager gameStateManager;
    private readonly Dictionary<Level, LevelRepresentation> scenesDictionary = new(); //identifier / level rep
    
    public LevelRepresentation CurrentLevelRepresentation { get; private set; }
    
    public event Action OnSceneLoadedEvent;

    public override void OnCreated()
    {
        base.OnCreated();
        
        SceneManager.sceneLoaded += OnSceneLoaded;

        gameStateManager = GameStateManager.instance;
        inputManager = InputManager.instance;
        
        CreateScenesDictionary();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) => OnSceneLoadedEvent?.Invoke();

    private void CreateScenesDictionary()
    {
        foreach(var level in scenesList) 
            scenesDictionary.Add(level.LevelIdentifier, level);
    }

    public void SwitchToScene(Level levelIdentifier)
    {
        var levelRep = scenesDictionary[levelIdentifier];
        
        SwitchToScene(levelRep);
    }
    
    public void SwitchToScene(LevelRepresentation levelRep)
    {
        var sceneName = levelRep.SceneName;
        
        gameStateManager.ChangeGameState(levelRep.InitialLevelGameState);

        Action toggleCursor = () => { ToggleCursorForLevel(levelRep); };
        
        OnSceneLoadedEvent += () =>
        {
            toggleCursor.Invoke();
            OnSceneLoadedEvent -= toggleCursor;
        };

        if (!TrySwitchToScene(sceneName)) return;
        
        CurrentLevelRepresentation = levelRep;
    }

    public void ToggleCursorForLevel(LevelRepresentation levelRep)
    {
        Cursor.visible = !levelRep.HideCursorOnLoad;

        var cursorNotActiveMode = DetermineCursorNotActiveMode();

        Cursor.lockState = levelRep.HideCursorOnLoad ? cursorNotActiveMode : CursorLockMode.None;
    }

    private CursorLockMode DetermineCursorNotActiveMode()
    {
        var playerIsUsingGamepad = inputManager.LatestDevice == PlayerDevice.Gamepad;
        var cursorNotActiveMode = playerIsUsingGamepad ? CursorLockMode.Locked : CursorLockMode.Confined;
        return cursorNotActiveMode;
    }

    public void ToggleCursorForLevel(bool isCursorActive)
    {
        Cursor.visible = isCursorActive;
        
        var cursorNotActiveMode = DetermineCursorNotActiveMode();

        Cursor.lockState = isCursorActive ? CursorLockMode.None : cursorNotActiveMode;
    }

    private bool TrySwitchToScene(string scene)
    {
        try
        {
            SceneManager.LoadScene(scene);
            return true;
        }
        catch (Exception e)
        {
            var sceneNotFoundException = new SceneNotLoadedException(scene);
            
            Debug.LogError(sceneNotFoundException.Message);
            Debug.LogError(e.StackTrace);

            throw sceneNotFoundException;
        }
    }

    public void SceneStarted(GameState state)
    {
        var levelRep = scenesDictionary.Values.FirstOrDefault(level => level.InitialLevelGameState == state);
        
        if(levelRep == null) return;
        
        CurrentLevelRepresentation = levelRep;
        ToggleCursorForLevel(levelRep);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private class SceneNotLoadedException : Exception
    {
        public SceneNotLoadedException(string scene) : base($"Requested scene could not be loaded: {scene}") { }
    }
}