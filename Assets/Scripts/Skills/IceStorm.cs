using UnityEngine;

/// <summary>
/// KY NANG 2 - MUA BANG.
/// Goi mot con bao lanh phu xuong khu vuc chi dinh: tuyet roi, suong lanh la dat,
/// va tung tang bang lon lien tuc roi xuong. Moi cu roi trung se no ra hoi lanh,
/// dung gai bang tu duoi dat len.
///
/// Tang bang roi TRUNG ke dich (quai hay nguoi choi khac) thi:
///   - CHAC CHAN lam cham 50% trong 2 giay;
///   - va 35% so lan DONG CUNG han 1,5 giay - khong di, khong tung duoc phep.
/// Tang bang KHONG BAO GIO nham vao chinh nguoi tung phep.
/// </summary>
public class IceStorm : MonoBehaviour
{
    [Header("Pham vi")]
    public float radius = 5.5f;
    public float duration = 5f;

    [Header("Manh bang roi")]
    // 0,128 thay vi 0,16: trong 5 giay tha duoc 39 tang thay vi 31 - dung
    // them 8 tang theo yeu cau.
    public float shardInterval = 0.128f;
    // 29,04 = 16 x 1,65 x 1,10. Tang 65% roi tang tiep 10%.
    //
    // Sua o day CHUA DU: Skill_MuaBang.prefab cung luu san mot ban, va gia tri
    // trong prefab luon thang gia tri mac dinh trong code.
    public float shardDamage = 29.04f;
    public float shardRadius = 1.7f;
    // 20 m - BANG do cao xuat phat cua tia set trong Sam set (boltHeight).
    //
    // Truoc day de 7 m vi so tang bang roi xuyen qua ngay truoc ong kinh. Do
    // lai trong Play mode thi noi lo ay khong thanh: may quay dung o do cao
    // 3,9 m va lui ra sau nguoi choi, nen tang roi o 20 m van nam gon trong
    // khung hinh - tang gan may quay nhat do duoc la 9,75 m.
    public float fallHeight = 20f;

    // 0,96 giay chu khong 0,336.
    //
    // GIU NGUYEN TOC DO ROI da chinh o lan truoc: 7 m / 0,336 s = 20,8 m/s, nen
    // 20 m phai het 20 / 20,8 = 0,96 s. De nguyen 0,336 s cho quang duong gap
    // gan ba lan thi tang lao xuong voi 59 m/s - nhanh den muc gan nhu khong
    // kip nhin thay gi.
    public float fallTime = 0.96f;

    [Header("Dong bang")]
    [Tooltip("Dong cung hoan toan bao lau (khong di, khong tung phep)")]
    public float freezeSeconds = 1.5f;

    [Tooltip("Xac suat tang bang lam ke dich DONG CUNG khi roi trung")]
    [Range(0f, 1f)]
    public float freezeChance = 0.35f;

    [Header("Lam cham khi trung")]
    [Tooltip("Giam bao nhieu phan toc do - ap CHAC CHAN cho moi muc tieu trung tang bang")]
    [Range(0f, 1f)]
    public float chamTiLe = 0.5f;

    [Tooltip("Lam cham keo dai bao lau sau khi trung")]
    public float chamGiay = 2f;

    [Tooltip("Khi lanh trong vung - cham hon nhung chi khi dang dung trong bao")]
    public float chillSlow = 0.55f;

    [Header("Nham muc tieu")]
    [Tooltip("Xac suat tang bang nham vao ke dich thay vi roi bua. 1 = luon nham")]
    [Range(0f, 1f)]
    public float aimAtEnemyChance = 1f;

    [Tooltip("Do tan ra quanh muc tieu, tinh bang met")]
    public float aimSpread = 0.55f;

    public LayerMask damageMask;

    /// <summary>Nguoi tung phep - truyen xuong tung manh sinh ra sau. Manh
    /// roi tre vai giay nen khong the doc mot bien tam luc do; phai giu o day
    /// roi giao lai.</summary>
    public Damageable boQua;

    float timer, shardTimer, chillTimer;
    static readonly Collider[] buffer = new Collider[64];

    public static IceStorm Spawn(Vector3 center, LayerMask damageMask)
    {
        GameObject go;
        IceStorm st;

        var pf = GameAssets.I != null ? GameAssets.I.iceStormPrefab : null;
        if (pf != null)
        {
            go = GameAssets.Make(pf, center);
            st = go.GetComponent<IceStorm>();
            if (st == null) st = go.AddComponent<IceStorm>();
        }
        else
        {
            go = new GameObject("IceStorm");
            go.transform.position = center;
            st = go.AddComponent<IceStorm>();
        }

        st.damageMask = damageMask;
        return st;
    }

    void Start()
    {
        VfxFactory.IceStormField(transform.position, radius, duration);
        CameraShake.Shake(0.5f, 0.06f);
    }

    void Update()
    {
        float dt = Time.deltaTime;
        timer += dt;
        shardTimer -= dt;
        chillTimer -= dt;

        // Tha manh bang xuong nhung diem ngau nhien trong vung
        if (timer < duration && shardTimer <= 0f)
        {
            shardTimer = shardInterval;

            Vector3 target = ChonDiemRoi();

            var shard = VfxFactory.IceShardFalling(target, fallHeight, fallTime);
            var fall = shard.GetComponent<FallingShard>();
            if (fall != null)
            {
                fall.damage = shardDamage;
                fall.impactRadius = shardRadius;
                fall.damageMask = damageMask;
                fall.boQua = boQua;
                fall.freezeSeconds = freezeSeconds;
                fall.freezeChance = freezeChance;
                fall.chamTiLe = chamTiLe;
                fall.chamGiay = chamGiay;
            }
        }

        // Khong khi lanh: quai dung trong vung bi cham dan lai
        if (timer < duration && chillTimer <= 0f)
        {
            chillTimer = 0.4f;
            var cols = Physics.OverlapSphere(transform.position, radius, damageMask,
                                             QueryTriggerInteraction.Collide);
            for (int i = 0; i < cols.Length; i++)
            {
                var d = cols[i].GetComponentInParent<Damageable>();
                if (d == null || d.IsDead) continue;

                // KHONG uop lanh chinh nguoi tung phep. Truoc day thieu dong
                // nay: phu thuy dung trong con bao cua chinh minh thi bi cham
                // 55% - tu trung phep minh.
                if (boQua != null && d == boQua) continue;

                // Ban sao lay lop bang tu goi tin - xem HieuUngQuaMang
                if (d.mauDoMayKhacQuyet) continue;

                var fr = d.GetComponent<FrozenEffect>();
                if (fr == null)
                {
                    fr = d.gameObject.AddComponent<FrozenEffect>();
                    fr.slow = chillSlow;
                    fr.remaining = 1.2f;
                }
                else
                {
                    fr.remaining = Mathf.Max(fr.remaining, 1.2f);
                    fr.slow = Mathf.Max(fr.slow, chillSlow);
                }
            }
        }

        if (timer >= duration + 1.5f) Destroy(gameObject);
    }

    /// <summary>
    /// Cho tha tang bang: UU TIEN dau mot ke dich con song trong vung, khong co
    /// con nao thi moi roi bua.
    ///
    /// Van TAN RA quanh muc tieu chu khong dap dung mot diem. Ba muoi chin tang
    /// cung roi vao dung mot toa do thi chung chong khit len nhau va het ra
    /// "mua" - trong nhu mot cai cot bang. Tan 0,55 m van nam gon trong ban
    /// kinh no 1,7 m nen khong mat sat thuong.
    ///
    /// Khi co nhieu ke dich thi chon ngau nhien mot con trong so dang song
    /// (lay mau kieu ho chua - khong phai gom thanh danh sach roi moi boc), nho
    /// vay tang bang rai deu giua cac muc tieu chu khong dam het vao mot con.
    /// </summary>
    Vector3 ChonDiemRoi()
    {
        if (Random.value < aimAtEnemyChance)
        {
            int n = Physics.OverlapSphereNonAlloc(transform.position, radius, buffer,
                                                  damageMask, QueryTriggerInteraction.Collide);
            int song = 0;
            Damageable chon = null;
            for (int i = 0; i < n; i++)
            {
                var d = buffer[i].GetComponentInParent<Damageable>();
                if (d == null || d.IsDead) continue;

                // KHONG NHAM VAO CHINH NGUOI TUNG PHEP (nguoi dung bao 12/09/2026,
                // co anh chup: tang bang cu dap lien tuc xuong dau chinh minh).
                //
                // Khi choi mang, enemyMask co ca lop Player, nen nguoi tung cung
                // nam trong danh sach quet duoc - va vi anh ta gan nhu luon dung
                // giua vung minh vua nham, gan nhu tang nao cung chon anh ta.
                // AreaFreeze co bo qua boQua nen khong mat mau, nhung ca tran
                // mua do xuong dau minh trong khi ke dich ben canh khong dinh
                // giot nao.
                if (boQua != null && d == boQua) continue;

                song++;
                if (Random.Range(0, song) == 0) chon = d;
            }

            if (chon != null)
            {
                Vector2 lech = Random.insideUnitCircle * aimSpread;
                Vector3 p = chon.transform.position + new Vector3(lech.x, 0f, lech.y);
                p.y = VfxFactory.GroundY(p);
                return p;
            }
        }

        Vector2 rnd = Random.insideUnitCircle * radius * 0.92f;
        Vector3 bua = transform.position + new Vector3(rnd.x, 0f, rnd.y);
        bua.y = VfxFactory.GroundY(bua);
        return bua;
    }
}
