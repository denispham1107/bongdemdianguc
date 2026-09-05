using UnityEngine;

/// <summary>
/// BO DIEM MOI LUA CUA MOT CAI LUOI CAY - rai deu tren khap be mat, luu san
/// trong <c>Assets/Resources/DiemLua/&lt;ten luoi&gt;.asset</c>.
///
/// ===================================================================
///  VI SAO PHAI NUONG SAN CHU KHONG DOC LUOI LUC CHAY
/// ===================================================================
///
/// Muon lua bam theo tung canh lon canh nho thi phai biet be mat cai cay nam o
/// dau, tuc phai doc <c>mesh.vertices</c>. Nhung <b>46 trong 58 cay Act2 nam
/// trong <c>map_luoi.fbx</c> voi Read/Write TAT</b> - doc lucchay se nem loi
/// (trong Editor thi doc duoc, nen loi nay chi lo ra o ban build).
///
/// Bat Read/Write cho file do thi Unity giu mot ban sao CPU cua <b>ca tam ban
/// do</b> - 12,7 MB, ma trong do cay chi la mot phan nho. Tren dien thoai do la
/// mot cai gia rat dat cho mot hieu ung.
///
/// Nuong san thi khac: chi con <b>vai tram diem</b> cho moi loai luoi, dung
/// chung cho moi cai cay xai luoi do. Chin loai luoi cay Act2 gop lai chua toi
/// mot phan nghin so dinh that.
///
/// Cay Act1 thi khong can asset nay: luoi cua no dung bang code luc chay nen
/// doc thang duoc. Xem <see cref="CayChay.LayDiemMoi"/>.
/// </summary>
public class DiemLuaCay : ScriptableObject
{
    /// <summary>Toa do TRONG LUOI (chua qua transform) cua tung diem moi lua.</summary>
    public Vector3[] diem;

    /// <summary>Ten luoi da nuong ra bo diem nay - de doi chieu khi nghi ngo.</summary>
    public string tenLuoi;

    /// <summary>So dinh cua luoi goc luc nuong.</summary>
    public int soDinhGoc;

    /// <summary>Thu muc trong Resources - ca ben nuong lan ben doc deu dung hang nay.</summary>
    public const string ThuMuc = "DiemLua";

    /// <summary>Nap bo diem cua mot luoi, khong co thi tra ve null.</summary>
    public static DiemLuaCay Nap(string tenLuoi)
    {
        if (string.IsNullOrEmpty(tenLuoi)) return null;
        return Resources.Load<DiemLuaCay>(ThuMuc + "/" + tenLuoi);
    }
}
