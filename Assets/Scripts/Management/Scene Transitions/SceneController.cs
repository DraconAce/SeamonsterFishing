using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : Singleton<SceneController>
{
    [SerializeField] private List<LevelRepresentation> scenesList;

    private GameStateManager gameStateManager;
    private readonly Dictionary<Level, LevelRepresentation> scenesDictionary = new(); //identifier / level rep

    public override void OnCreated()
    {
        base.OnCreated();

        gameStateManager = GameStateManager.instance;
        CreateScenesDictionary();
    }

    private void CreateScenesDictionary()
    {
        foreach(var level in scenesList)
            scenesDictionary.Add(level.LevelIdentifier, level);
    }

    public void SwitchToScene(Level levelIdentifier)
    {
        var levelRep = scenesDictionary[levelIdentifier];
        var sceneName = levelRep.SceneName;
        
        gameStateManager.ChangeGameState(levelRep.InitialLevelGameState);
        
        TrySwitchToScene(sceneName);
    }

    private void TrySwitchToScene(string scene)
    {
        try
        {
            SceneManager.LoadScene(scene);
        }
        catch (Exception e)
        {
            var sceneNotFoundException = new SceneNotLoadedException(scene);
            
            Debug.LogError(sceneNotFoundException.Message);
            Debug.LogError(e.StackTrace);

            throw sceneNotFoundException;
        }
    }

    private class SceneNotLoadedException : Exception
    {
        public SceneNotLoadedException(string scene) : base($"Requested scene could not be loaded: {scene}") { }
    }
}