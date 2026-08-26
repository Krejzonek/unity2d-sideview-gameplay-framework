using UnityEngine;

public class UIClass : MonoBehaviour
{
    [SerializeField] private Stats playerStats;

    public void SelectClass(Class classBonus)
    {
        playerStats.SetClass(classBonus);
        this.transform.parent.gameObject.SetActive(false);
    }
}