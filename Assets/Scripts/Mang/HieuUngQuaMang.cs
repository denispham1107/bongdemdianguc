using UnityEngine;

/// <summary>
/// DONG BANG, CHOANG VA MAU KHIENG DI QUA MANG.
///
/// Truoc day ba thu nay KHONG di qua mang: moi may tu tinh cho ban sao cua
/// nguoi khac. Ma CombatUtil.AreaFreeze va AreaShock tung Random.value - moi
/// may gieo rieng. Ket qua: A thay B bi dong bang, B thi van chay nhay; hay
/// khieng cua B vo tren may A trong khi tren may B no con nua mau.
///
/// Quy uoc giu nguyen: MAY CHU SO HUU LA TRONG TAI.
///   - Nguoi choi: may cua chinh ho quyet ho co bi dong bang/choang khong, va
///     khieng con bao nhieu. Gui kem goi trang thai 60 lan moi giay.
///   - Quai: chu phong quyet, gui kem goi quai.
/// May khac chi VE LAI. Va ban sao (mauDoMayKhacQuyet) khong tu gieo hieu ung
/// cho minh nua - xem cac cho gac trong FrozenEffect.Apply, StunnedEffect.Apply,
/// IceStorm va Khieng.HapThu.
///
/// Hieu ung ap tu mang duoc "giu song" bang mot khoang ngan
/// (<see cref="GiuSongGiay"/>) va duoc lam moi moi goi tin. Goi ngung den
/// (ho tan bang, hoac mat mang) thi hieu ung tu tan sau khoang ay - khong can
/// mot goi "het dong bang" rieng, va mat goi cung khong de lai mot khoi bang
/// vinh vien.
/// </summary>
public static class HieuUngQuaMang
{
    /// <summary>Hieu ung ap tu mang song them bao lau neu khong duoc lam moi.
    /// Dai hon khoang cach hai goi quai (100 ms) mot chut cho khoi nhap nhay.</summary>
    public const float GiuSongGiay = 0.35f;

    // Cac bit trong byte co - dung chung cho nguoi choi va quai
    public const byte CoBang = 1 << 0;           // co lop bang (cham hoac dung im)
    public const byte CoBangHoanToan = 1 << 1;   // dong cung hoan toan
    public const byte CoChoang = 1 << 2;         // dang choang

    /// <summary>Doc trang thai hieu ung that tren mot vat - de gui di.</summary>
    public static byte DocCo(GameObject go)
    {
        if (go == null) return 0;
        byte co = 0;

        var f = go.GetComponent<FrozenEffect>();
        if (f != null && f.remaining > 0f)
        {
            co |= CoBang;
            if (f.IsFullyFrozen) co |= CoBangHoanToan;
        }

        var st = go.GetComponent<StunnedEffect>();
        if (st != null && st.IsStunned) co |= CoChoang;

        return co;
    }

    /// <summary>Mau khieng con lai, 0..1. 0 la khong co khieng.</summary>
    public static float DocKhieng(GameObject go)
    {
        if (go == null) return 0f;
        var k = go.GetComponent<Khieng>();
        return k != null && k.DangBat ? k.Mau01 : 0f;
    }

    /// <summary>
    /// Ve lai trang thai hieu ung len mot ban sao, theo loi ke cua may chu so
    /// huu. Goi moi lan goi tin den.
    /// </summary>
    public static void ApCo(Damageable d, byte co)
    {
        if (d == null || d.IsDead) return;
        var go = d.gameObject;

        // ---- Dong bang ----
        var f = go.GetComponent<FrozenEffect>();
        if ((co & CoBang) != 0)
        {
            // Gan THANG, khong qua FrozenEffect.Apply - Apply da bi gac cho ban
            // sao de ban sao khong tu gieo hieu ung.
            //
            // Vua gan moi thi DAT THANG thoi gian song, dung lay Max: component
            // moi co san remaining = 3 giay, lay Max(3; 0,35) la khoi bang ket
            // lai 3 giay sau khi goi tin da ngung. Phep thu bat duoc dung cho nay.
            if (f == null)
            {
                f = go.AddComponent<FrozenEffect>();
                f.remaining = GiuSongGiay;
            }
            f.slow = (co & CoBangHoanToan) != 0 ? 1f : 0.5f;
            f.remaining = Mathf.Max(f.remaining, GiuSongGiay);
        }
        else if (f != null && f.remaining > 0.05f)
        {
            // Ben kia da tan bang - tan theo ngay, dung doi het khoang giu song
            f.remaining = 0.05f;
        }

        // ---- Choang ----
        var st = go.GetComponent<StunnedEffect>();
        if ((co & CoChoang) != 0)
        {
            if (st == null)
            {
                st = go.AddComponent<StunnedEffect>();
                st.remaining = GiuSongGiay;       // mac dinh 2 giay - xem ghi chu dong bang
                DamagePopup.SpawnText(go.transform.position + Vector3.up * 2.1f, "CHOANG!",
                                      new Color(0.75f, 0.90f, 1f));
            }
            st.remaining = Mathf.Max(st.remaining, GiuSongGiay);
        }
        else if (st != null && st.remaining > 0.05f)
        {
            st.remaining = 0.05f;
        }
    }

    /// <summary>
    /// Ve lai khieng cua mot ban sao nguoi choi.
    ///
    /// Day la NGUON DUY NHAT cua khieng tren ban sao - phat lai phep Khieng
    /// khong dung khieng nua (xem PlayerController.Release). Hai nguon lech nhau
    /// vai chuc mili giay la du de mot goi "chua co khieng" den sau dap vo cai
    /// khieng vua dung.
    ///
    /// Ba truong hop:
    ///   - ben kia co khieng ma ben nay chua co: dung khieng ngay.
    ///   - ca hai cung co: dat mau cho khop, vom se sang/mo theo.
    ///   - ben kia het khieng ma ben nay con: VO no - de nguoi xem thay khieng
    ///     vo dung luc no vo that, chu khong tat im lang.
    /// </summary>
    public static void ApKhieng(PlayerController pc, float mau01)
    {
        if (pc == null) return;
        var k = pc.GetComponent<Khieng>();

        if (mau01 > 0.001f)
        {
            if (k == null || !k.DangBat)
            {
                k = Khieng.Bat(pc.gameObject, pc.khiengMau, pc.khiengBanKinh);
                var d = pc.GetComponent<Damageable>();
                if (d != null) d.khieng = k;
            }
            k.DatMauTuMang(mau01);
        }
        else if (k != null && k.DangBat)
        {
            k.VoTuMang();
        }
    }
}
