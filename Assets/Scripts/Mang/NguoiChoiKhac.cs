using UnityEngine;

/// <summary>
/// SINH VA QUAN LY NHAN VAT CUA NHUNG NGUOI CHOI KHAC.
///
/// Nhan vat cua nguoi khac dung y het nhan vat cua minh - cung prefab, cung
/// bo ky nang, cung mau. Chi khac dung MOT dieu: no khong doc ban phim cua
/// may nay (<c>tuDocInput = false</c>), ma cho y muon den qua duong truyen.
///
/// VI SAO KHONG LAM MOT PREFAB RIENG "hinh bong nguoi khac": vi den buoc danh
/// nhau, nhan vat do phai chiu sat thuong, phai co khien, phai chet - tuc la
/// phai la mot nhan vat that su. Mot cai vo rong nhin giong nguoi choi thi se
/// phai di bo sung tung thu mot, va moi lan quen mot thu la mot lo hong khong
/// ai thay cho den luc dang danh nhau.
///
/// Cai duy nhat phai tat la CAMERA: chi nhan vat cua minh moi duoc camera bam
/// theo. Hai camera trong mot canh thi Unity chon bua mot cai, va nguoi choi
/// dot nhien nhin the gioi bang mat nguoi khac.
/// </summary>
public static class NguoiChoiKhac
{
    /// <summary>
    /// Dung mot nhan vat cho nguoi choi khac tai <paramref name="cho"/>.
    ///
    /// Tra ve <c>PlayerController</c> cua no, hoac null neu khong dung noi.
    /// </summary>
    public static PlayerController Sinh(string uid, string ten, Vector3 cho)
    {
        GameObject go = null;

        if (GameAssets.I != null && GameAssets.I.playerPrefab != null)
        {
            go = Object.Instantiate(GameAssets.I.playerPrefab, cho, Quaternion.identity);
        }
        else
        {
            // Khong co prefab thi muon chinh duong dung cua GameBootstrap -
            // dung mot nhan vat bang code y nhu nhan vat cua minh.
            var bs = Object.FindAnyObjectByType<GameBootstrap>();
            if (bs == null) return null;
            go = bs.BuildPlayerFromCode();
            if (go != null) go.transform.position = cho;
        }

        if (go == null) return null;

        go.name = "NguoiChoi_" + (string.IsNullOrEmpty(ten) ? uid : ten);

        // KHONG de tag "Player": nhieu cho trong game van goi
        // FindGameObjectWithTag("Player") va chi lay CAI DAU TIEN tim thay -
        // de nguyen thi camera hoac quai co the vo phai nguoi khac thay vi
        // nguoi choi cua may nay.
        go.tag = "Untagged";

        var pc = go.GetComponent<PlayerController>();
        if (pc != null)
        {
            // Y muon den tu duong truyen, khong phai tu ban phim may nay
            pc.tuDocInput = false;
            pc.input = GoiInput.Rong(0f);

            // Camera cua rieng minh - khong duoc bam theo nguoi khac
            pc.cameraRig = null;
        }

        // Go moi camera di kem (neu prefab co san mot cai)
        foreach (var cam in go.GetComponentsInChildren<Camera>(true))
            cam.gameObject.SetActive(false);

        foreach (var rig in go.GetComponentsInChildren<CameraRig>(true))
            rig.enabled = false;

        // Bo doc input cua nhan vat nay khong duoc chay: no se doc ban phim
        // cua may nay va dieu khien nguoi khac
        var doc = go.GetComponent<DocInput>();
        if (doc != null) doc.enabled = false;

        var dir = GameDirector.Instance;
        if (dir != null) dir.ThemNguoiChoi(go.transform);

        return pc;
    }

    /// <summary>Nguoi choi roi tran - go nhan vat khoi canh.</summary>
    public static void Bo(PlayerController pc)
    {
        if (pc == null) return;

        var dir = GameDirector.Instance;
        if (dir != null) dir.BoNguoiChoi(pc.transform);

        Object.Destroy(pc.gameObject);
    }
}
