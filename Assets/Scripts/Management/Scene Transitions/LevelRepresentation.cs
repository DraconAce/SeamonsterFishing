using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "Level Representation", menuName = "Create Level Representation", order = 0)]
public class LevelRepresentation : ScriptableObject
{
    public Level LevelIdentifier = Level.MainMenu;
    public GameState InitialLevelGameState = GameState.None;
    
    #if UNITY_EDITOR
    [SerializeField] private SceneAsset sceneAsset;
    
    private void OnValidate()
    {
        if (sceneAsset == null) return;

        sceneName = sceneAsset.name;
    }
    #endif
    
    private string sceneName;
    public string SceneName => sceneName;
}