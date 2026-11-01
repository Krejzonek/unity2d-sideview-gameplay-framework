using System.Collections.Generic;
using UnityEngine;

public static class GamepadUIState
{
    private static readonly HashSet<GameObject> openPanels = new();

    public static bool IsOpen => openPanels.Count > 0;

    public static void Register(GameObject panel)
    {
        if (panel != null)
            openPanels.Add(panel);
    }

    public static void Unregister(GameObject panel)
    {
        if (panel != null)
            openPanels.Remove(panel);
    }

    public static void Clear()
    {
        openPanels.Clear();
    }
}