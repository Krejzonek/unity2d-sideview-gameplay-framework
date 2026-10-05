using TMPro;
using UnityEngine;

public class DamageNumber : MonoBehaviour
{
    [SerializeField] private TMP_Text damageText;
    [SerializeField] private float moveSpeed = 1.5f;
    [SerializeField] private float lifetime = 0.7f;

    private float timer;

    public void Setup(float damage, bool isCrit, bool isHeadshot, ElementType element = ElementType.None, bool isHealing = false)
    {
        damageText.text = isHealing ? "+" + Mathf.RoundToInt(damage).ToString() : Mathf.RoundToInt(damage).ToString();

        if (isHealing)
            damageText.color = Color.green;
        else if (isCrit || isHeadshot)
            damageText.color = Color.red;
        else
            damageText.color = GetElementColor(element);
    }

    private Color GetElementColor(ElementType element)
    {
        switch (element)
        {
            case ElementType.Fire:
                return new Color(1f, 0.3f, 0.1f);

            case ElementType.Water:
                return new Color(0.2f, 0.6f, 1f);

            case ElementType.Earth:
                return new Color(0.55f, 0.35f, 0.15f);

            case ElementType.Wind:
                return new Color(0.4f, 1f, 0.5f);

            case ElementType.Lightning:
                return new Color(1f, 0.9f, 0.1f);

            case ElementType.Ice:
                return new Color(0.4f, 0.9f, 1f);

            case ElementType.Light:
                return new Color(1f, 1f, 0.6f);

            case ElementType.Dark:
                return new Color(0.5f, 0.2f, 0.8f);

            case ElementType.Poison:
                return new Color(0.6f, 1f, 0.1f);

            default:
                return Color.white;
        }
    }

    private void Update()
    {
        transform.position += Vector3.up * moveSpeed * Time.deltaTime;

        timer += Time.deltaTime;

        if (timer >= lifetime)
            Destroy(gameObject);
    }
}