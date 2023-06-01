﻿using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public abstract class AbstractMenu : MonoBehaviour, IInputEventSubscriber
{
    [SerializeField] protected string[] menuInputActions;
    [SerializeField] private GameObject menuContainer;
    [SerializeField] private UnityEvent onMenuOpened;
    [SerializeField] private UnityEvent onMenuClosed;
    
    private CanvasGroup menuGroup;
    private InputManager inputManager;

    private bool initialCloseExecuted;

    protected virtual bool UseInputActions => true;
    
    public bool SubscribedToStarted => true;
    public bool SubscribedToPerformed => false;
    public bool SubscribedToCanceled => false;
    
    public string[] ActionsToSubscribeTo => menuInputActions;

    protected virtual void Start()
    {
        TryGetComponent(out menuGroup);
        CloseMenu(false);

        initialCloseExecuted = true;

        if (!UseInputActions) return;
        inputManager = InputManager.instance;
        inputManager.SubscribeToActions(this);
    }

    public void InputStarted(InputAction.CallbackContext callContext) => InputStartedImpl();

    protected virtual void InputStartedImpl(){}

    protected void OpenMenu()
    {
        ToggleMenuCanvas(true);
        OpenMenuImpl();
        
        onMenuOpened?.Invoke();
    }

    private void ToggleMenuCanvas(bool activate)
    {
        menuContainer.SetActive(activate);
        menuGroup.alpha = activate ? 1 : 0;
    }

    protected virtual void OpenMenuImpl(){}

    protected void CloseMenu(bool invokeEvent = true)
    {
        ToggleMenuCanvas(false);
        
        if(initialCloseExecuted) CloseMenuImpl();

        if (!invokeEvent) return;
        onMenuClosed?.Invoke();
    }

    protected virtual void CloseMenuImpl(){}

    protected virtual void OnDestroy()
    {
        if (!UseInputActions || inputManager == null) return;
        UnsubscribeOnDestroy();
    }

    public void UnsubscribeOnDestroy() => inputManager.UnsubscribeFromActions(this);
}