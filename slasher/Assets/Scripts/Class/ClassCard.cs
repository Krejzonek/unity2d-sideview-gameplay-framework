using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ClassCard : MonoBehaviour
{
    [SerializeField] private Class classData;
    [SerializeField] private ClassSelector classSelector;
    [SerializeField] private TMP_Text classNameText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private Button selectButton;

    private void Start()
    {
        if (classData != null)
        {
            classNameText.text = classData.className;
            descriptionText.text = classData.description;
        }

        if (selectButton != null)
            selectButton.onClick.AddListener(SelectClass);
    }

    private void SelectClass()
    {
        if (classSelector != null)
            classSelector.ShowClass(classData);
    }
}