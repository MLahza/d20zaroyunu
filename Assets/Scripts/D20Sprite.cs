using UnityEngine;

public static class D20Sprite
{
    static Sprite cached;

    public static Sprite Get()
    {
        if (cached != null) return cached;

        const int size = 256;
        float c = size * 0.5f;
        float R = size * 0.47f;
        float lineW = 2.2f;

        var hex = new Vector2[6];
        for (int k = 0; k < 6; k++)
        {
            float a = (90f + 60f * k) * Mathf.Deg2Rad;
            hex[k] = new Vector2(c + R * Mathf.Cos(a), c + R * Mathf.Sin(a));
        }

        var tri = new Vector2[3];
        for (int k = 0; k < 3; k++)
        {
            float a = (90f + 120f * k) * Mathf.Deg2Rad;
            tri[k] = new Vector2(c + R * 0.5f * Mathf.Cos(a), c + R * 0.5f * Mathf.Sin(a));
        }

        var segs = new System.Collections.Generic.List<(Vector2 a, Vector2 b)>();
        for (int k = 0; k < 6; k++) segs.Add((hex[k], hex[(k + 1) % 6]));
        for (int k = 0; k < 3; k++) segs.Add((tri[k], tri[(k + 1) % 3]));
        for (int k = 0; k < 3; k++)
        {
            int h = k * 2;
            segs.Add((tri[k], hex[(h + 1) % 6]));
            segs.Add((tri[k], hex[(h + 5) % 6]));
            segs.Add((tri[k], hex[h]));
        }

        var baseCol = new Color(0.9f, 0.9f, 0.96f, 1f);
        var faceCol = new Color(1f, 1f, 1f, 1f);
        var lineCol = new Color(0.25f, 0.27f, 0.38f, 1f);

        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp };
        var px = new Color[size * size];

        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            var p = new Vector2(x + 0.5f, y + 0.5f);

            float hexDist = PolyDistance(p, hex, out bool inHex);
            float edgeAlpha = Mathf.Clamp01((inHex ? hexDist : -hexDist) + 0.5f);
            if (edgeAlpha <= 0f) { px[y * size + x] = Color.clear; continue; }

            PolyDistance(p, tri, out bool inTri);
            Color col = inTri ? faceCol : baseCol;

            float minD = float.MaxValue;
            foreach (var s in segs) minD = Mathf.Min(minD, SegDistance(p, s.a, s.b));
            float l = 1f - Mathf.SmoothStep(lineW - 1f, lineW + 1f, minD);
            col = Color.Lerp(col, lineCol, l);

            col.a = edgeAlpha;
            px[y * size + x] = col;
        }

        tex.SetPixels(px);
        tex.Apply();
        cached = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        return cached;
    }

    static float PolyDistance(Vector2 p, Vector2[] poly, out bool inside)
    {
        float min = float.MaxValue;
        float sign = 0f;
        inside = true;
        for (int i = 0; i < poly.Length; i++)
        {
            Vector2 a = poly[i], b = poly[(i + 1) % poly.Length];
            min = Mathf.Min(min, SegDistance(p, a, b));
            float cross = (b.x - a.x) * (p.y - a.y) - (b.y - a.y) * (p.x - a.x);
            if (sign == 0f) sign = Mathf.Sign(cross);
            else if (Mathf.Sign(cross) != sign) inside = false;
        }
        return min;
    }

    static float SegDistance(Vector2 p, Vector2 a, Vector2 b)
    {
        Vector2 ab = b - a;
        float t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / ab.sqrMagnitude);
        return Vector2.Distance(p, a + ab * t);
    }
}
