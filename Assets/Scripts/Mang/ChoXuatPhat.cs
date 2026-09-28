using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// CHO XUAT PHAT NGAU NHIEN CHO TUNG NGUOI CHOI (het dem nguoc 10 giay).
///
/// Truoc day ca phong cung sinh ra o MOT diem trong scene, xep bon goc mot o
/// vuong 1,6 m - vao tran la dam vao nhau ngay. Nguoi dung xin: moi nguoi hien
/// ra o mot cho NGAU NHIEN tren ban do, va KHONG duoc gan nhau qua.
///
/// CAI KHO: "khong gan nhau" doi hoi cac may phai BIET CHO CUA NHAU - ma luc
/// vua vao man thi chua ai gui goi tin nao ca. Neu moi may tu boc mot cho rieng
/// thi hai nguoi co the cung roi vao mot goc nghia dia.
///
/// CACH LAM: gieo hat ngau nhien tu MA PHONG - con so ma moi may deu co san va
/// deu giong nhau. Cung mot hat, cung mot ban do, cung mot doan code thi moi
/// may tinh ra CUNG MOT danh sach cho; moi nguoi chi viec lay cho theo ghe cua
/// minh. Khong ton mot goi tin nao, va khong co cuoc dua nao.
///
/// Cho hop le: dung tren dia hinh (tia chieu xuong cham Ground/dia hinh truoc),
/// khong duoi nuoc, khong vuong bia mo / da / cay / nha (CheckCapsule lop
/// Default), va cach nhung nguoi truoc it nhat <see cref="CachNhauToiThieu"/>.
/// </summary>
public static class ChoXuatPhat
{
    /// <summary>Hai nguoi choi khong duoc hien ra gan nhau hon chung nay met.</summary>
    public const float CachNhauToiThieu = 22f;

    /// <summary>Boc bao nhieu lan cho MOI nguoi truoc khi chiu ha tieu chuan.</summary>
    const int SoLanBoc = 220;

    /// <summary>Ban kinh nguoi choi (de kiem vat can).</summary>
    const float BanKinhNguoi = 0.45f;

    /// <summary>
    /// Cho dung cua tung ghe, tinh chung cho ca phong. Luon ra cung ket qua
    /// tren moi may neu <paramref name="hat"/> giong nhau.
    /// </summary>
    public static List<Vector3> ChoChoCaPhong(int hat, int soGhe, Vector3 tam, float banKinh)
    {
        var ra = new List<Vector3>();
        var cu = Random.state;                       // tra lai de khong pha cac phep ngau nhien khac
        Random.InitState(hat);

        for (int i = 0; i < soGhe; i++)
        {
            Vector3 tot = Vector3.zero;
            float xaNhat = -1f;
            bool coCho = false;

            for (int lan = 0; lan < SoLanBoc; lan++)
            {
                // Boc deu theo DIEN TICH (can bac hai) - khong thi moi nguoi
                // tum lai giua ban do
                float g = Random.Range(0f, Mathf.PI * 2f);
                float r = banKinh * Mathf.Sqrt(Random.value);
                var p = tam + new Vector3(Mathf.Cos(g) * r, 0f, Mathf.Sin(g) * r);

                float y;
                if (!DungTrenDat(p, out y)) continue;
                p.y = y + 0.1f;
                if (DuoiNuoc(p)) continue;
                if (VuongVatCan(p)) continue;

                float gan = GanNhat(p, ra);
                if (gan >= CachNhauToiThieu) { tot = p; coCho = true; break; }

                // Chua du xa: giu lai cho XA NHAT tim duoc, phong khi ban do
                // chat qua khong tim ra cho nao du chuan
                if (gan > xaNhat) { xaNhat = gan; tot = p; coCho = true; }
            }

            if (!coCho) tot = tam;                   // ban do khong co cho nao: ve giua
            ra.Add(tot);
        }

        Random.state = cu;
        return ra;
    }

    /// <summary>Tran Doi: hai GOC DOI cach nhau it nhat chung nay met (ban kinh Act2 62 m - hai phia ban do).</summary>
    public const float CachHaiDoiToiThieu = 55f;
    /// <summary>Tran Doi: dong doi dung quanh goc doi trong khoang nay (met).</summary>
    public const float DongDoiGanToiThieu = 2.5f, DongDoiGanToiDa = 6f;
    /// <summary>Hai dong doi khong dung sat nhau hon (met).</summary>
    public const float DongDoiCachNhau = 2.2f;

    /// <summary>
    /// CHO XUAT PHAT TRAN DOI (nguoi dung 28/09/2026: "dong doi gan nhau"): moi doi mot GOC o mot phia ban do, hai goc cach
    /// nhau it nhat <see cref="CachHaiDoiToiThieu"/>; moi nguoi dung quanh goc doi minh 2,5 - 6 m, cach dong doi >= 2,2 m.
    /// Nhu <see cref="ChoChoCaPhong"/>: gieo tu ma phong va bang doi cua phong nen moi may ra CUNG MOT danh sach.
    /// </summary>
    /// <param name="doiTheoGhe">Doi cua tung ghe (0/1; -1 = ghe trong).</param>
    public static List<Vector3> ChoChoDoi(int hat, IList<sbyte> doiTheoGhe, Vector3 tam, float banKinh)
    {
        var ra = new List<Vector3>();
        var cu = Random.state;
        Random.InitState(hat ^ 0x5D0D01);

        // ---- Hai goc doi: goc A o mot vanh ngau nhien, goc B phia doi dien xa nhat ----
        Vector3 gocA = tam, gocB = tam;
        bool coA = false;
        for (int lan = 0; lan < SoLanBoc && !coA; lan++)
        {
            float g = Random.Range(0f, Mathf.PI * 2f);
            float r = banKinh * Random.Range(0.45f, 0.78f);
            var p = tam + new Vector3(Mathf.Cos(g) * r, 0f, Mathf.Sin(g) * r);
            float y;
            if (!ChoDungDuoc(p, out y)) continue;
            p.y = y + 0.1f; gocA = p; coA = true;
        }
        float xaNhat = -1f;
        for (int lan = 0; lan < SoLanBoc; lan++)
        {
            Vector3 nguoc = tam - (gocA - tam);
            float g = Mathf.Atan2(nguoc.z - tam.z, nguoc.x - tam.x) + Random.Range(-0.9f, 0.9f);
            float r = banKinh * Random.Range(0.45f, 0.85f);
            var p = tam + new Vector3(Mathf.Cos(g) * r, 0f, Mathf.Sin(g) * r);
            float y;
            if (!ChoDungDuoc(p, out y)) continue;
            p.y = y + 0.1f;
            float xa = new Vector2(p.x - gocA.x, p.z - gocA.z).magnitude;
            if (xa > xaNhat) { xaNhat = xa; gocB = p; }
            if (xa >= CachHaiDoiToiThieu) break;
        }

        // ---- Tung nguoi quanh goc doi minh ----
        var daCo = new List<Vector3>();
        for (int i = 0; i < doiTheoGhe.Count; i++)
        {
            int d = doiTheoGhe[i];
            Vector3 goc = d == CheDoTran.DoiB ? gocB : gocA;
            Vector3 tot = goc; bool coCho = false; float xaNhatCho = -1f;
            for (int lan = 0; lan < SoLanBoc; lan++)
            {
                float g = Random.Range(0f, Mathf.PI * 2f);
                float r = Random.Range(DongDoiGanToiThieu, DongDoiGanToiDa);
                var p = goc + new Vector3(Mathf.Cos(g) * r, 0f, Mathf.Sin(g) * r);
                float y;
                if (!ChoDungDuoc(p, out y)) continue;
                p.y = y + 0.1f;
                float gan = GanNhat(p, daCo);
                if (gan >= DongDoiCachNhau) { tot = p; coCho = true; break; }
                if (gan > xaNhatCho) { xaNhatCho = gan; tot = p; coCho = true; }
            }
            if (!coCho) tot = goc;
            daCo.Add(tot);
            ra.Add(tot);
        }

        Random.state = cu;
        return ra;
    }

    /// <summary>Dung duoc: tren dat, khong duoi nuoc, khong vuong vat can.</summary>
    static bool ChoDungDuoc(Vector3 p, out float y)
    {
        if (!DungTrenDat(p, out y)) return false;
        var q = new Vector3(p.x, y + 0.1f, p.z);
        return !DuoiNuoc(q) && !VuongVatCan(q);
    }

    /// <summary>Hat gieo tu ma phong - moi may deu tinh ra cung mot so.</summary>
    public static int HatTuMaPhong(string maPhong)
    {
        if (string.IsNullOrEmpty(maPhong)) return 12345;
        int h = 17;
        for (int i = 0; i < maPhong.Length; i++) h = h * 31 + maPhong[i];
        return h;
    }

    static float GanNhat(Vector3 p, List<Vector3> ds)
    {
        float m = float.MaxValue;
        for (int i = 0; i < ds.Count; i++)
            m = Mathf.Min(m, new Vector2(p.x - ds[i].x, p.z - ds[i].z).magnitude);
        return ds.Count == 0 ? float.MaxValue : m;
    }

    /// <summary>Tia chieu tu tren xuong phai cham DAT truoc tien (khong phai mai nha, bia).</summary>
    public static bool DungTrenDat(Vector3 p, out float y)
    {
        y = 0f;
        RaycastHit hit;
        if (!Physics.Raycast(p + Vector3.up * 60f, Vector3.down, out hit, 120f, ~0,
                             QueryTriggerInteraction.Ignore)) return false;
        y = hit.point.y;
        if (hit.collider is TerrainCollider) return true;
        // Mat dat cung co the la mot lop luoi thuong o lop Ground
        return hit.collider.gameObject.layer == LayerMask.NameToLayer("Ground");
    }

    /// <summary>
    /// Diem nay co nam duoi mat nuoc khong.
    ///
    /// Do bang HOP BAO cua luoi nuoc chu khong doc tung tam giac: luc chay thi
    /// luoi nuoc co the khong doc duoc, va hop bao thi chi rong hon mot chut -
    /// nguoi choi hien ra cach bo them mot met khong ai phan nan.
    /// </summary>
    public static bool DuoiNuoc(Vector3 p)
    {
        var nuoc = GameObject.Find("MatNuoc");
        if (nuoc == null) return false;
        foreach (var r in nuoc.GetComponentsInChildren<Renderer>())
        {
            var b = r.bounds;
            if (p.x < b.min.x - 1f || p.x > b.max.x + 1f
                || p.z < b.min.z - 1f || p.z > b.max.z + 1f) continue;
            if (p.y < b.max.y + 0.3f) return true;    // dat thap hon mat nuoc = long chao
        }
        return false;
    }

    public static bool VuongVatCan(Vector3 p)
    {
        return Physics.CheckCapsule(p + Vector3.up * (BanKinhNguoi + 0.1f),
                                    p + Vector3.up * 1.9f, BanKinhNguoi,
                                    1 << 0, QueryTriggerInteraction.Ignore);
    }
}
