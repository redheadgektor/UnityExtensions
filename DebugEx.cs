using UnityEngine;

public static class DebugEx
{
    public static void DrawBox(Vector3 pos, Quaternion rot, Vector3 scale, Color c, float duration = 0)
    {
        Matrix4x4 m = new Matrix4x4();
        m.SetTRS(pos, rot, scale);

        var point1 = m.MultiplyPoint(new Vector3(-0.5f, -0.5f, 0.5f));
        var point2 = m.MultiplyPoint(new Vector3(0.5f, -0.5f, 0.5f));
        var point3 = m.MultiplyPoint(new Vector3(0.5f, -0.5f, -0.5f));
        var point4 = m.MultiplyPoint(new Vector3(-0.5f, -0.5f, -0.5f));

        var point5 = m.MultiplyPoint(new Vector3(-0.5f, 0.5f, 0.5f));
        var point6 = m.MultiplyPoint(new Vector3(0.5f, 0.5f, 0.5f));
        var point7 = m.MultiplyPoint(new Vector3(0.5f, 0.5f, -0.5f));
        var point8 = m.MultiplyPoint(new Vector3(-0.5f, 0.5f, -0.5f));

        Debug.DrawLine(point1, point2, c, duration);
        Debug.DrawLine(point2, point3, c, duration);
        Debug.DrawLine(point3, point4, c, duration);
        Debug.DrawLine(point4, point1, c, duration);

        Debug.DrawLine(point5, point6, c, duration);
        Debug.DrawLine(point6, point7, c, duration);
        Debug.DrawLine(point7, point8, c, duration);
        Debug.DrawLine(point8, point5, c, duration);

        Debug.DrawLine(point1, point5, c, duration);
        Debug.DrawLine(point2, point6, c, duration);
        Debug.DrawLine(point3, point7, c, duration);
        Debug.DrawLine(point4, point8, c, duration);
    }

    public static void DrawBounds(Bounds b, Color color, float duration = 0)
    {
        var p1 = new Vector3(b.min.x, b.min.y, b.min.z);
        var p2 = new Vector3(b.max.x, b.min.y, b.min.z);
        var p3 = new Vector3(b.max.x, b.min.y, b.max.z);
        var p4 = new Vector3(b.min.x, b.min.y, b.max.z);

        Debug.DrawLine(p1, p2, color, duration);
        Debug.DrawLine(p2, p3, color, duration);
        Debug.DrawLine(p3, p4, color, duration);
        Debug.DrawLine(p4, p1, color, duration);

        var p5 = new Vector3(b.min.x, b.max.y, b.min.z);
        var p6 = new Vector3(b.max.x, b.max.y, b.min.z);
        var p7 = new Vector3(b.max.x, b.max.y, b.max.z);
        var p8 = new Vector3(b.min.x, b.max.y, b.max.z);

        Debug.DrawLine(p5, p6, color, duration);
        Debug.DrawLine(p6, p7, color, duration);
        Debug.DrawLine(p7, p8, color, duration);
        Debug.DrawLine(p8, p5, color, duration);

        Debug.DrawLine(p1, p5, color, duration);
        Debug.DrawLine(p2, p6, color, duration);
        Debug.DrawLine(p3, p7, color, duration);
        Debug.DrawLine(p4, p8, color, duration);
    }

    public static void DrawCircle(Vector3 center, float radius, Color color, float duration = 0, int quality = 2)
    {
        for (int angle = 0; angle < 360; angle = angle + quality)
        {
            var heading = Vector3.forward - center;
            var direction = heading / heading.magnitude;
            var point = center + Quaternion.Euler(0, angle, 0) * Vector3.forward * radius;
            var point2 = center + Quaternion.Euler(0, angle + quality, 0) * Vector3.forward * radius;
            Debug.DrawLine(point, point2, color, duration);
        }
    }

    public static void DrawWireSphere(Vector3 center, float radius, Color color, float duration = 0, int quality = 3)
    {
        quality = Mathf.Clamp(quality, 1, 10);

        int segments = quality << 2;
        int subdivisions = quality << 3;
        int halfSegments = segments >> 1;
        float strideAngle = 360F / subdivisions;
        float segmentStride = 180F / segments;

        Vector3 first;
        Vector3 next;
        for (int i = 0; i < segments; i++)
        {
            first = (Vector3.forward * radius);
            first = Quaternion.AngleAxis(segmentStride * (i - halfSegments), Vector3.right) * first;

            for (int j = 0; j < subdivisions; j++)
            {
                next = Quaternion.AngleAxis(strideAngle, Vector3.up) * first;
                Debug.DrawLine(first + center, next + center, color, duration);
                first = next;
            }
        }

        Vector3 axis;
        for (int i = 0; i < segments; i++)
        {
            first = (Vector3.forward * radius);
            first = Quaternion.AngleAxis(segmentStride * (i - halfSegments), Vector3.up) * first;
            axis = Quaternion.AngleAxis(90F, Vector3.up) * first;

            for (int j = 0; j < subdivisions; j++)
            {
                next = Quaternion.AngleAxis(strideAngle, axis) * first;
                Debug.DrawLine(first + center, next + center, color, duration);
                first = next;
            }
        }
    }

    public static void DrawCross(Vector3 point, float size, Color color, float time = 0)
    {
        Debug.DrawRay(point, Vector3.left * size, color, time);
        Debug.DrawRay(point, Vector3.right * size, color, time);
        Debug.DrawRay(point, Vector3.down * size, color, time);
        Debug.DrawRay(point, Vector3.up * size, color, time);
        Debug.DrawRay(point, Vector3.forward * size, color, time);
        Debug.DrawRay(point, Vector3.back * size, color, time);
    }
}
