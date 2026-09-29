using UnityEngine;

/// <summary>
/// DO DAU VET LOC TREN MAT DAT - dung chung menu 71 (Gio loc) va 82 (Loc xoay), 29/09/2026.
/// Bam dat so voi do cao doc THANG TU TERRAIN (SampleHeight) - doc lap voi tia do lop Ground ma VetLocDat dung.
/// </summary>
public static class ThuVetLocChung
{
    /// <summary>Do cao terrain duoi p (NaN neu p nam ngoai moi terrain).</summary>
    public static float DoCaoTerrain(Vector3 p)
    {
        foreach (var t in Terrain.activeTerrains)
        {
            var o = t.transform.position; var s = t.terrainData.size;
            if (p.x >= o.x && p.x <= o.x + s.x && p.z >= o.z && p.z <= o.z + s.z) return t.SampleHeight(p) + o.y;
        }
        return float.NaN;
    }

    /// <summary>
    /// DO BANG ANH (29/09/2026 - lan dau dai "dung hinh hoc" ma VO HINH tren anh: du an Gamma, anh sRGB 0,33 x _NhanMau 4 bi kep
    /// ve 1 -> khong toi gi). Render cung mot khung HAI lan (bat / tat dai), do do sang quanh diem giua moi lat.
    /// Tra ve ti le co dai / khong dai (1 = vo hinh).
    /// </summary>
    public static float TiLeToiTrenAnh(Camera cam, VetLocDat v, out float sangCo, out float sangKhong)
    {
        sangCo = 0f; sangKhong = 0f;
        if (v == null || cam == null || v.SoLat == 0) return 1f;
        var r = v.GetComponent<Renderer>();
        const int W = 640, H = 360;
        var rt = new RenderTexture(W, H, 24);
        Color[] co = Chup(cam, rt, W, H);
        r.enabled = false;
        Color[] khong = Chup(cam, rt, W, H);
        r.enabled = true;
        Object.Destroy(rt);
        double a = 0, b = 0; int n = 0;
        for (int i = 0; i < v.SoLat; i++)
        {
            var p = v.DiemLat(i);
            var vp = cam.WorldToViewportPoint(p[p.Length / 2]);
            if (vp.z <= 0f || vp.x < 0.02f || vp.x > 0.98f || vp.y < 0.02f || vp.y > 0.98f) continue;
            int cx = (int)(vp.x * W), cy = (int)(vp.y * H);
            for (int dy = -2; dy <= 2; dy++)
                for (int dx = -2; dx <= 2; dx++)
                {
                    int id = (cy + dy) * W + (cx + dx);
                    a += Sang(co[id]); b += Sang(khong[id]); n++;
                }
        }
        if (n == 0) return 1f;
        sangCo = (float)(a / n); sangKhong = (float)(b / n);
        return sangCo / Mathf.Max(1e-4f, sangKhong);
    }

    static float Sang(Color c) { return 0.299f * c.r + 0.587f * c.g + 0.114f * c.b; }

    static Color[] Chup(Camera cam, RenderTexture rt, int W, int H)
    {
        var cu = cam.targetTexture; cam.targetTexture = rt; cam.Render(); cam.targetTexture = cu;
        var tr = RenderTexture.active; RenderTexture.active = rt;
        var t = new Texture2D(W, H, TextureFormat.RGB24, false);
        t.ReadPixels(new Rect(0, 0, W, H), 0, 0); t.Apply();
        RenderTexture.active = tr;
        var px = t.GetPixels();
        Object.Destroy(t);
        return px;
    }

    public static VetLocDat TimDai(Transform chu)
    {
        foreach (var v in VetLocDat.DangSong) if (v != null && !v.LaChay && v.Chu == chu) return v;
        return null;
    }

    public static int DemVetChayGan(Vector3 a, Vector3 b, float banKinh)
    {
        int n = 0;
        foreach (var v in VetLocDat.DangSong)
        {
            if (v == null || !v.LaChay || v.SoLat == 0) continue;
            var p = v.DiemLat(0)[12];                            // tam luoi 5x5
            Vector3 ab = b - a; ab.y = 0f; Vector3 ap = p - a; ap.y = 0f;
            float t = ab.sqrMagnitude > 1e-4f ? Mathf.Clamp01(Vector3.Dot(ap, ab) / ab.sqrMagnitude) : 0f;
            if ((ap - ab * t).magnitude < banKinh) n++;
        }
        return n;
    }

    /// <summary>Do mot dai: so lat, quang duong, be rong trung binh, lech lon nhat so voi terrain (+0,04 m).</summary>
    public static string Do(VetLocDat v, out int soLat, out float rongTB, out float lechDatMax, out int soDiemDo)
    {
        soLat = 0; rongTB = 0f; lechDatMax = 0f; soDiemDo = 0;
        if (v == null) return "khong co dai";
        soLat = v.SoLat;
        for (int i = 0; i < soLat; i++)
        {
            var p = v.DiemLat(i);
            Vector3 d = p[p.Length - 1] - p[0]; d.y = 0f;
            rongTB += d.magnitude;
            foreach (var q in p)
            {
                float h = DoCaoTerrain(q);
                if (float.IsNaN(h)) continue;
                lechDatMax = Mathf.Max(lechDatMax, Mathf.Abs(q.y - (h + 0.04f)));
                soDiemDo++;
            }
        }
        if (soLat > 0) rongTB /= soLat;
        return string.Format("{0} lat, quang duong {1:F2} m, be rong TB {2:F2} m, lech so voi terrain lon nhat {3:F3} m ({4} diem), lat cu nhat {5:F2} s",
            soLat, v.QuangDuong, rongTB, lechDatMax, soDiemDo, v.TuoiLonNhat);
    }
}
