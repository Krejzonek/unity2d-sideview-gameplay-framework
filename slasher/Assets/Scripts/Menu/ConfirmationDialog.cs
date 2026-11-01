using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ConfirmationDialog : MonoBehaviour
{
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private Button cancelButton;
    [SerializeField] private Button confirmButton;

    private Action confirmAction;
    private GameObject previousSelected;

    private void Awake()
    {
        if (cancelButton != null)
            cancelButton.onClick.AddListener(Cancel);

        if (confirmButton != null)
            confirmButton.onClick.AddListener(Confirm);

        if (gameObject.activeSelf)
            gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        if (cancelButton != null)
            cancelButton.onClick.RemoveListener(Cancel);

        if (confirmButton != null)
            confirmButton.onClick.RemoveListener(Confirm);
    }

    public void Show(string message, Action onConfirm)
    {
        confirmAction = onConfirm;

        if (EventSystem.current != null)
            previousSelected = EventSystem.current.currentSelectedGameObject;

        if (messageText != null)
            messageText.text = message;

        gameObject.SetActive(true);

        if (EventSystem.current != null && cancelButton != null && cancelButton.gameObject.activeInHierarchy)
            EventSystem.current.SetSelectedGameObject(cancelButton.gameObject);
    }

    private void Cancel()
    {
        confirmAction = null;
        gameObject.SetActive(false);
        RestoreSelection();
    }

    private void Confirm()
    {
        Action action = confirmAction;
        confirmAction = null;

        gameObject.SetActive(false);
        RestoreSelection();

        action?.Invoke();
    }

    private void RestoreSelection()
    {
        if (EventSystem.current != null && previousSelected != null && previousSelected.activeInHierarchy)
            EventSystem.current.SetSelectedGameObject(previousSelected);

        previousSelected = null;
    }
}