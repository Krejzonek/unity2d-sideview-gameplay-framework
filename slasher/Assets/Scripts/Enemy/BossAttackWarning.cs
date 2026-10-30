using UnityEngine;

public class BossAttackWarning : MonoBehaviour
{
    private LineRenderer lineRenderer;

    private void Awake()
    {
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.useWorldSpace = true;
        lineRenderer.loop = false;
        lineRenderer.positionCount = 0;
        lineRenderer.startWidth = 0.06f;
        lineRenderer.endWidth = 0.06f;
        lineRenderer.sortingOrder = 100;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.enabled = false;
    }

    public void Show(BossAttackData attack, Vector2 origin, float direction, float bossHeight)
    {
        if (attack == null)
            return;

        lineRenderer.startColor = attack.warningColor;
        lineRenderer.endColor = attack.warningColor;
        lineRenderer.enabled = true;

        switch (attack.warningShape)
        {
            case BossWarningShape.Horizontal:
                DrawRectangle(
                    origin + new Vector2(direction * attack.warningLength / 2f, bossHeight / 2f),
                    attack.warningLength,
                    bossHeight,
                    0f);
                break;

            case BossWarningShape.Vertical:
                DrawRectangle(
                    origin + Vector2.up * attack.warningHeight / 2f,
                    attack.warningWidth,
                    attack.warningHeight,
                    0f);
                break;

            case BossWarningShape.Circle:
                DrawCircle(origin, attack.warningRadius);
                break;
        }
    }

    private void DrawRectangle(Vector2 center, float width, float height, float rotation)
    {
        Vector2 half = new Vector2(width / 2f, height / 2f);

        Vector2[] points =
        {
            new Vector2(-half.x, -half.y),
            new Vector2(-half.x, half.y),
            new Vector2(half.x, half.y),
            new Vector2(half.x, -half.y),
            new Vector2(-half.x, -half.y)
        };

        for (int i = 0; i < points.Length; i++)
            points[i] = center + Rotate(points[i], rotation);

        lineRenderer.positionCount = points.Length;

        for (int i = 0; i < points.Length; i++)
            lineRenderer.SetPosition(i, points[i]);
    }

    private void DrawCircle(Vector2 center, float radius)
    {
        int segments = 48;
        lineRenderer.positionCount = segments + 1;

        for (int i = 0; i <= segments; i++)
        {
            float angle = i * Mathf.PI * 2f / segments;
            Vector2 point = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
            lineRenderer.SetPosition(i, point);
        }
    }

    private Vector2 Rotate(Vector2 point, float angle)
    {
        float radians = angle * Mathf.Deg2Rad;
        float sin = Mathf.Sin(radians);
        float cos = Mathf.Cos(radians);

        return new Vector2(point.x * cos - point.y * sin, point.x * sin + point.y * cos);
    }

    public void Hide()
    {
        if (lineRenderer != null)
        {
            lineRenderer.positionCount = 0;
            lineRenderer.enabled = false;
        }
    }

    private void OnDestroy()
    {
        if (lineRenderer != null && lineRenderer.material != null)
            Destroy(lineRenderer.material);
    }
}