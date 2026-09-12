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
    static bool DungTrenDat(Vector3 p, out float y)
    {
        y = 0f;
        RaycastHit hit;
        if (!Physics.Raycast(p + Vector3.up * 60f, Vector3.down, out hit, 120f, ~0,
                             QueryTriggerInteraction.Ignore)) return false;
        y = hit.point.y;
        if (hit.collider is TerrainCollider) return true;
        // Act1 dung bang code: mat dat co the la mot lop luoi thuong o lop Ground
        return hit.collider.gameObject.layer == LayerMask.NameToLayer("Ground");
    }

    /// <summary>
    /// Diem nay co nam duoi mat nuoc khong.
    ///
    /// Do bang HOP BAO cua luoi nuoc chu khong doc tung tam giac: luc chay thi
    /// luoi nuoc co the khong doc duoc, va hop bao thi chi rong hon mot chut -
    /// nguoi choi hien ra cach bo them mot met khong ai phan nan.
    /// </summary>
    static bool DuoiNuoc(Vector3 p)
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

    static bool VuongVatCan(Vector3 p)
    {
        return Physics.CheckCapsule(p + Vector3.up * (BanKinhNguoi + 0.1f),
                                    p + Vector3.up * 1.9f, BanKinhNguoi,
                                    1 << 0, QueryTriggerInteraction.Ignore);
    }
}
