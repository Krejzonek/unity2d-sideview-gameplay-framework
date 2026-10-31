using TMPro;
using UnityEngine;

public class SettingsRebindPanel : MonoBehaviour
{
    [SerializeField] private TMP_Text messageText;

    private GameInputManager inputManager;

    private void Awake()
    {
        inputManager = GameInputManager.Instance;     
    }

    public void Open(RebindButton rebindButton)
    {
        if (rebindButton == null)
            return;

        if (inputManager == null)
            inputManager = GameInputManager.Instance;

        if (messageText != null)
        {
            messageText.text = rebindButton.GetDevice() == InputDeviceType.Keyboard
                ? "Press Any Key or Mouse Button..."
                : "Press Any Gamepad Button...";
        }

        gameObject.SetActive(true);
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }
}