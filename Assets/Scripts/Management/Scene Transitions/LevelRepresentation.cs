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

    public Level LevelIdentifier => levelIdentifier;

    public GameState InitialLevelGameState => initialLevelGameState;

    public string SceneName => sceneName;

#if UNITY_EDITOR
    [SerializeField] private SceneAsset sceneAsset;

    private void OnValidate()
    {
        if (sceneAsset == null) return;

        sceneName = sceneAsset.name;
    }
    #endif
}