using TMPro;
using UnityEngine;

public class SkillNamePopup : MonoBehaviour
{
    [SerializeField] private TMP_Text skillNameText;
    [SerializeField] private float duration = 1.5f;

    public void Initialize(string skillName)
    {
        if (skillNameText != null)
            skillNameText.text = skillName;

        Destroy(gameObject, duration);
    }
}