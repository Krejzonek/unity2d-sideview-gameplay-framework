using UnityEngine;
using UnityEngine.EventSystems;

public class MobileActionButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    [SerializeField] private GameInputAction action;
    [SerializeField] private GameInputManager inputManager;

    private bool pressed;

    private void Awake()
    {
        if (inputManager == null)
            inputManager = GameInputManager.Instance;
    }

    private void OnEnable()
    {
        if (inputManager == null)
            inputManager = GameInputManager.Instance;
    }

    private void OnDisable()
    {
        Release();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (inputManager == null)
            inputManager = GameInputManager.Instance;

        if (inputManager == null || pressed)
            return;

        pressed = true;
        inputManager.SetMobileAction(action, true);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Release();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Release();
    }

    private void Release()
    {
        if (!pressed)
            return;

        pressed = false;

        if (inputManager == null)
            inputManager = GameInputManager.Instance;

        if (inputManager != null)
            inputManager.SetMobileAction(action, false);
    }
}