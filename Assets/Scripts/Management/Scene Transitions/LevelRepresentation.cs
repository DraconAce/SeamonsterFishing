using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "Level Representation", menuName = "Create Level Representation", order = 0)]
public class LevelRepresentation : ScriptableObject
{
    [SerializeField] private Level levelIdentifier = Level.MainMenu;
    [SerializeField] private GameState initialLevelGameState = GameState.None;
    [SerializeField] private string sceneName;
    [SerializeField] private bool hideCursorOnLoad = true;

    public Level LevelIdentifier => levelIdentifier;

    public GameState InitialLevelGameState => initialLevelGameState;

    public string SceneName => sceneName;
    
    public bool HideCursorOnLoad => hideCursorOnLoad;

#if UNITY_EDITOR
    [SerializeField] private SceneAsset sceneAsset;

    private void OnValidate()
    {
        if (sceneAsset == null) return;

        sceneName = sceneAsset.name;
    }
    #endif
}