using TMPro;
using UnityEngine;

public class SettingsRebindPanel : MonoBehaviour
{
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private SelectionBorder selectionBorder;

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

        if (selectionBorder != null)
            selectionBorder.gameObject.SetActive(false);

        gameObject.SetActive(true);
    }

    public void Close()
    {
        gameObject.SetActive(false);

        if (selectionBorder != null)
            selectionBorder.gameObject.SetActive(true);
    }
}