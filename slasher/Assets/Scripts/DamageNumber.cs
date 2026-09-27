using TMPro;
using UnityEngine;

public class DamageNumber : MonoBehaviour
{
    [SerializeField] private TMP_Text damageText;
    [SerializeField] private float moveSpeed = 1.5f;
    [SerializeField] private float lifetime = 0.7f;

    private float timer;

    public void Setup(float damage, bool isCrit, bool isHeadshot, bool isHealing = false)
    {
        damageText.text = Mathf.RoundToInt(damage).ToString();

        if (isHealing)
            damageText.color = Color.green;
        else
            damageText.color = isCrit || isHeadshot ? Color.red : Color.white;
    }

    private void Update()
    {
        transform.position += Vector3.up * moveSpeed * Time.deltaTime;

        timer += Time.deltaTime;

        if (timer >= lifetime)
            Destroy(gameObject);
    }
}