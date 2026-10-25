public enum ElementType
{
    None,
    Fire,
    Water,
    Earth,
    Wind,
    Lightning,
    Ice,
    Light,
    Dark,
    Poison
}

public static class ElementSystem
{
    public static float GetDamageMultiplier(ElementType attackElement, ElementType targetElement)
    {
        if (attackElement == ElementType.None || targetElement == ElementType.None)
            return 1f;

        switch (attackElement)
        {
            case ElementType.Water:
                if (targetElement == ElementType.Fire) return 1.2f;
                if (targetElement == ElementType.Lightning) return 0.8f;
                break;

            case ElementType.Fire:
                if (targetElement == ElementType.Ice) return 1.2f;
                if (targetElement == ElementType.Water) return 0.8f;
                break;

            case ElementType.Ice:
                if (targetElement == ElementType.Wind) return 1.2f;
                if (targetElement == ElementType.Fire) return 0.8f;
                break;

            case ElementType.Wind:
                if (targetElement == ElementType.Earth) return 1.2f;
                if (targetElement == ElementType.Ice) return 0.8f;
                break;

            case ElementType.Earth:
                if (targetElement == ElementType.Lightning) return 1.2f;
                if (targetElement == ElementType.Wind) return 0.8f;
                break;

            case ElementType.Lightning:
                if (targetElement == ElementType.Water) return 1.2f;
                if (targetElement == ElementType.Earth) return 0.8f;
                break;

            case ElementType.Light:
                if (targetElement == ElementType.Dark) return 1.2f;
                if (targetElement == ElementType.Poison) return 0.8f;
                break;

            case ElementType.Dark:
                if (targetElement == ElementType.Light) return 1.2f;
                if (targetElement == ElementType.Poison) return 0.8f;
                break;

            case ElementType.Poison:
                if (targetElement == ElementType.Light) return 1.2f;
                break;
        }

        return 1f;
    }
}