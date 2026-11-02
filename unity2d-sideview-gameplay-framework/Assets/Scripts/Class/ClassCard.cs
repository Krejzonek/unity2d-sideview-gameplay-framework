using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ClassCard : MonoBehaviour
{
    [SerializeField] private ClassSelector classSelector;
    [SerializeField] private Image classColorImage;
    [SerializeField] private TMP_Text classNameText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private Button selectButton;

    private Class classData;

    private void Awake()
    {
        if (selectButton != null)
            selectButton.onClick.AddListener(SelectClass);
    }

    public void Setup(Class classData, ClassSelector selector)
    {
        this.classData = classData;
        classSelector = selector;

        if (classData == null)
            return;

        if (classColorImage != null)
            classColorImage.color = classData.classColor;

        if (classNameText != null)
            classNameText.text = classData.className;

        if (descriptionText != null)
            descriptionText.text = classData.description;
    }

    private void SelectClass()
    {
        if (classSelector != null && classData != null)
            classSelector.ShowClass(classData);
    }

    private void OnDestroy()
    {
        if (selectButton != null)
            selectButton.onClick.RemoveListener(SelectClass);
    }
}