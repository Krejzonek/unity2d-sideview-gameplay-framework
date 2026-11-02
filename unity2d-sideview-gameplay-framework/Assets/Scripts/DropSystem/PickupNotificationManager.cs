
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PickupNotificationManager : MonoBehaviour
{
    [SerializeField] private GameObject notificationPrefab;
    [SerializeField] private Transform notificationContainer;
    [SerializeField] private float notificationDuration = 10f;

    private class NotificationData
    {
        public string itemName;
        public int amount;
        public float timer;
        public GameObject gameObject;
        public TMP_Text text;
    }

    private readonly Dictionary<string, NotificationData> activeNotifications = new();
    private readonly List<NotificationData> notifications = new();

    public void ShowPickup(string itemKey, string itemName, int amount)
    {
        if (string.IsNullOrEmpty(itemKey) || amount <= 0)
            return;

        if (activeNotifications.TryGetValue(itemKey, out NotificationData existing))
        {
            existing.amount += amount;
            existing.timer = notificationDuration;
            UpdateText(existing);
            return;
        }

        if (notificationPrefab == null || notificationContainer == null)
            return;

        GameObject instance = Instantiate(notificationPrefab, notificationContainer);
        TMP_Text text = instance.GetComponentInChildren<TMP_Text>();

        if (text == null)
        {
            Destroy(instance);
            return;
        }

        NotificationData notification = new NotificationData
        {
            itemName = itemName,
            amount = amount,
            timer = notificationDuration,
            gameObject = instance,
            text = text
        };

        activeNotifications.Add(itemKey, notification);
        notifications.Add(notification);
        UpdateText(notification);
    }

    private void Update()
    {
        for (int i = notifications.Count - 1; i >= 0; i--)
        {
            NotificationData notification = notifications[i];
            notification.timer -= Time.deltaTime;

            if (notification.timer > 0f)
                continue;

            string key = null;

            foreach (var pair in activeNotifications)
            {
                if (pair.Value == notification)
                {
                    key = pair.Key;
                    break;
                }
            }

            if (key != null)
                activeNotifications.Remove(key);

            Destroy(notification.gameObject);
            notifications.RemoveAt(i);
        }
    }

    private void UpdateText(NotificationData notification)
    {
        notification.text.text = $"{notification.itemName} ×{notification.amount}";
    }
}
