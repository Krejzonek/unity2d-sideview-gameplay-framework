using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RebindButton : MonoBehaviour
{
    [SerializeField] private GameInputAction action;
    [SerializeField] private InputDeviceType device;
    [SerializeField] private Button button;
    [SerializeField] private TMP_Text bindingText;
    [SerializeField] private SettingsRebindPanel rebindPanel;

    private GameInputManager inputManager;

    private void Awake()
    {
        inputManager = GameInputManager.Instance;

        if (button == null)
            button = GetComponent<Button>();

        if (button != null)
        {
            button.onClick.AddListener(StartRebinding);
        }
        else
            Debug.LogError("Button not found on " + gameObject.name);
    }

    private void OnEnable()
    {
        if (inputManager == null)
            inputManager = GameInputManager.Instance;

        if (inputManager != null)
            inputManager.BindingChanged += OnBindingChanged;

        Refresh();
    }

    private void OnDisable()
    {
        if (inputManager != null)
            inputManager.BindingChanged -= OnBindingChanged;
    }

    private void OnDestroy()
    {
        if (button != null)
            button.onClick.RemoveListener(StartRebinding);
    }

    private void StartRebinding()
    {
        if (inputManager == null)
            inputManager = GameInputManager.Instance;

        if (inputManager == null || rebindPanel == null)
            return;

        rebindPanel.Open(this);

        inputManager.StartRebinding(action, device);
    }

    public void Refresh()
    {
        if (inputManager == null)
            inputManager = GameInputManager.Instance;

        if (inputManager == null || bindingText == null)
            return;

        bindingText.text = inputManager.GetBindingDisplayName(action, device);
    }

    private void OnBindingChanged(GameInputAction changedAction, InputDeviceType changedDevice, string displayName)
    {
        if (changedAction == action && changedDevice == device)
        {
            if (bindingText != null)
                bindingText.text = displayName;

            if (rebindPanel != null)
                rebindPanel.Close();
        }
    }

    public InputDeviceType GetDevice()
    {
        return device;
    }
}