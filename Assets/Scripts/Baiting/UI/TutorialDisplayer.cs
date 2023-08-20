using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class TutorialDisplayer : MonoBehaviour
{
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private Ease fadeEase = Ease.InSine;
    [SerializeField] private GameObject screensContainer;
    [SerializeField] private CanvasGroup[] tutorialScreens;
    [SerializeField] private UnityEvent onScreensFinished;
    
    private int currentScreenIndex = -1;

    private Sequence fadeSequence;
    private PauseManager pauseManager;
    private InputManager inputManager;
    private PlayerSingleton playerSingleton;

    private readonly InputAction anyKey = new ("AnyKey", binding: "/*/<button>", type: InputActionType.Button);

    private IEnumerator Start()
    {
        pauseManager = PauseManager.instance;
        inputManager = InputManager.instance;
        playerSingleton = PlayerSingleton.instance;
        
        playerSingleton.DisableMovementControls = true;
        
        screensContainer.SetActive(true);
        
        yield return new WaitForEndOfFrame();
        ShowTutorialAndPauseGame();
    }

    private void ShowTutorialAndPauseGame()
    {
        anyKey.Enable();
        anyKey.performed += OnContinue;

        pauseManager.ToggleGamePause();

        inputManager.playerInput.currentActionMap.Disable();

        DoTutorialTextFades();
    }
    
    private void DoTutorialTextFades()
    {
        fadeSequence?.Kill(true);
        
        fadeSequence = DOTween.Sequence();
        
        if(currentScreenIndex >= 0)
        {
            var formerScreen = tutorialScreens[currentScreenIndex];
            
            if(!Mathf.Approximately(formerScreen.alpha, 0)) 
                fadeSequence.Append(CreateFadeTextTween(0, formerScreen));
        }
        
        if(currentScreenIndex < tutorialScreens.Length - 1)
        {
            var nextScreen = tutorialScreens[++currentScreenIndex];

            if (!Mathf.Approximately(nextScreen.alpha, 1))
                fadeSequence.Append(CreateFadeTextTween(1, nextScreen));
        }

        fadeSequence.SetUpdate(true);
        
        fadeSequence.Play();
    }
    
    private Tween CreateFadeTextTween(float fadeTarget, CanvasGroup text)
    {
        var alphaDiff = Mathf.Abs(fadeTarget - text.alpha);
        
        var duration = fadeDuration * alphaDiff; //can do because alpha always between 0 & 1
        
        return text.DOFade(fadeTarget, duration)
            .SetEase(fadeEase);
    }

    private void OnContinue(InputAction.CallbackContext context)
    {
        if (!HasMoreScreensToShow)
        {
            CloseTutorialScreensAndStartGame();
            return;
        }

        DoTutorialTextFades();
    }

    private bool HasMoreScreensToShow => currentScreenIndex < tutorialScreens.Length - 1;

    private void CloseTutorialScreensAndStartGame()
    {
        anyKey.performed -= OnContinue;
        anyKey.Disable();

        pauseManager.ToggleGamePause();

        inputManager.playerInput.currentActionMap.Enable();

        DoTutorialTextFades();

        playerSingleton.DisableMovementControls = false;
        
        onScreensFinished?.Invoke();
        
        screensContainer.SetActive(false);
    }
}