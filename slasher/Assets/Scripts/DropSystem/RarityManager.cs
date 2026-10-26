using UnityEngine;

public static class RarityManager
{

    public static bool RollMisc()
    {
        return Random.Range(0f, 100f) < 10f;
    }

    public static MiscType GetRandomMisc()
    {
        int roll = Random.Range(0, 3);

        switch (roll)
        {
            case 0:
                return MiscType.CritRate;

            case 1:
                return MiscType.Knockback;

            case 2:
                return MiscType.Stun;

            default:
                return MiscType.None;
        }
    }
}