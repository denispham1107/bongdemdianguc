using UnityEngine;

/// <summary>
/// THU VIEN PREFAB CUA GAME.
///
/// Dat tren vat the "GAME" trong scene. Moi thu trong game (nhan vat, quai vat,
/// phep thuat, hieu ung) deu tro toi mot PREFAB co that trong thu muc
/// Assets/Prefabs, nen ban co the bam vao xem, sua, keo tha nhu moi game Unity khac.
///
/// Neu mot o bi de trong, game van chay binh thuong: no se tu dung hinh bang code
/// (cach cu). Nho vay du ban lo xoa prefab nao thi game cung khong vo.
/// </summary>
public class GameAssets : MonoBehaviour
{
    static GameAssets inst;
    static int lastSearchFrame = -1;

    /// <summary>Thu vien dang dung trong scene (tu tim neu chua co).</summary>
    public static GameAssets I
    {
        get
        {
            if (inst != null) return inst;
            if (Time.frameCount != lastSearchFrame)
            {
                lastSearchFrame = Time.frameCount;
                inst = Object.FindAnyObjectByType<GameAssets>();
            }
            return inst;
        }
    }

    [Header("Nhan vat")]
    public GameObject playerPrefab;

    [Tooltip("Theo dung thu tu MonsterType: Quy lun, Bo xuong, Xac song, Quy khong lo, Phu thuy, Quy du, Quy cay.")]
    public GameObject[] enemyPrefabs = new GameObject[7];

    [Header("Ky nang")]
    public GameObject fireballPrefab;         // qua cau lua dang bay
    public GameObject iceStormPrefab;         // bo dieu khien phep Mua bang
    public GameObject iceStormFieldPrefab;    // vung bao tuyet
    public GameObject iceShardPrefab;         // tang bang roi xuong
    public GameObject lightningStormPrefab;      // bo dieu khien phep Sam set
    public GameObject lightningStormFieldPrefab; // vung giong (may den + vong dien)
    public GameObject tornadoPrefab;             // con loc xoay

    [Header("Hieu ung")]
    public GameObject fireExplosionPrefab;
    public GameObject iceImpactPrefab;
    public GameObject lightningImpactPrefab;  // set cham dat
    public GameObject stunnedPrefab;          // vong dom sang khi quai bi choang
    public GameObject burningPrefab;          // lua bam tren nguoi quai
    public GameObject castFirePrefab;         // tich tu phep lua o dau gay
    public GameObject castIcePrefab;          // tich tu phep bang
    public GameObject castLightningPrefab;    // tich tu phep set
    public GameObject frozenShatterPrefab;    // vo bang vo tan
    public GameObject hitBurstPrefab;         // tia mau khi trung don

    [Header("Canh vat (de trang tri them)")]
    public GameObject brazierPrefab;
    public GameObject[] rockPrefabs;
    public GameObject[] pillarPrefabs;
    public GameObject[] treePrefabs;          // cay chet khang khiu
    public GameObject[] leafyTreePrefabs;     // cay con xanh la
    public GameObject[] bushPrefabs;          // bui ram thap

    void Awake()
    {
        inst = this;
    }

    void OnEnable()
    {
        inst = this;
    }

    // ================================================================
    //  TIEN ICH
    // ================================================================

    /// <summary>Lay prefab quai theo loai (null neu chua gan).</summary>
    public static GameObject EnemyPrefab(MonsterType type)
    {
        if (I == null || I.enemyPrefabs == null) return null;
        int i = (int)type;
        if (i < 0 || i >= I.enemyPrefabs.Length) return null;
        return I.enemyPrefabs[i];
    }

    /// <summary>Tao ban sao cua prefab tai mot vi tri (tra ve null neu chua co prefab).</summary>
    public static GameObject Make(GameObject prefab, Vector3 pos, Quaternion rot, Transform parent)
    {
        if (prefab == null) return null;
        var go = Object.Instantiate(prefab, pos, rot, parent);
        go.name = prefab.name;
        return go;
    }

    public static GameObject Make(GameObject prefab, Vector3 pos)
    {
        return Make(prefab, pos, Quaternion.identity, null);
    }
}
