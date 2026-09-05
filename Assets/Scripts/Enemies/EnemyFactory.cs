using UnityEngine;

/// <summary>
/// Lap rap mot con quai hoan chinh: hinh dang + va cham + mau + tri tue.
/// </summary>
public static class EnemyFactory
{
    public static GameObject Spawn(MonsterType type, Vector3 pos, Transform parent, Transform player)
    {
        // Neu co PREFAB trong Assets/Prefabs thi dung luon cho nhanh
        var pf = GameAssets.EnemyPrefab(type);
        if (pf != null)
        {
            var inst = GameAssets.Make(pf, pos, Quaternion.Euler(0f, Random.Range(0f, 360f), 0f), parent);
            var eai = inst.GetComponent<EnemyAI>();
            if (eai != null) eai.target = player;
            return inst;
        }

        return BuildFromCode(type, pos, parent, player);
    }

    /// <summary>Lap rap con quai bang code (dung de "nuong" ra prefab, hoac khi thieu prefab).</summary>
    public static GameObject BuildFromCode(MonsterType type, Vector3 pos, Transform parent, Transform player)
    {
        var go = new GameObject("Enemy_" + type);
        go.transform.SetParent(parent, false);
        go.transform.position = pos;
        go.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
        go.tag = "Enemy";

        // ---- Hinh dang ----
        var rig = MonsterFactory.Build(type, go.transform);
        SetLayerRecursive(go, LayerMask.NameToLayer("Enemy"));
        var anim = go.AddComponent<ProceduralAnimator>();
        anim.rig = rig;
        anim.casterPose = false;
        anim.strideScale = type == MonsterType.Brute ? 0.75f : 1.05f;

        // ---- Va cham ----
        var cc = go.AddComponent<CharacterController>();
        cc.height = rig.bodyHeight * 0.92f;
        cc.radius = Mathf.Max(0.22f, rig.bodyRadius * 0.75f);
        cc.center = new Vector3(0f, cc.height * 0.5f, 0f);
        cc.slopeLimit = 55f;
        cc.stepOffset = 0.4f;

        // ---- Mau ----
        var hp = go.AddComponent<Damageable>();
        hp.rig = rig;
        hp.anim = anim;

        // ---- Tri tue ----
        var ai = go.AddComponent<EnemyAI>();
        ai.target = player;
        ai.anim = anim;
        ai.health = hp;

        switch (type)
        {
            case MonsterType.Fallen:
                hp.maxHealth = hp.health = 45f;
                hp.fireResist = -0.15f;              // so lua
                ai.moveSpeed = 3.5f; ai.attackDamage = 7f;
                ai.attackRange = 1.5f; ai.attackCooldown = 1.3f; ai.attackAnimTime = 0.6f;
                ai.aggroRange = 17f;
                break;

            case MonsterType.Skeleton:
                hp.maxHealth = hp.health = 70f;
                hp.iceResist = 0.25f;                // xuong kho, it so lanh
                ai.moveSpeed = 2.7f; ai.attackDamage = 12f;
                ai.attackRange = 2.0f; ai.attackCooldown = 1.7f; ai.attackAnimTime = 0.8f;
                ai.aggroRange = 15f;
                break;

            case MonsterType.Ghoul:
                hp.maxHealth = hp.health = 88f;
                ai.moveSpeed = 3.0f; ai.attackDamage = 14f;
                ai.attackRange = 2.1f; ai.attackCooldown = 1.5f; ai.attackAnimTime = 0.7f;
                ai.aggroRange = 18f;
                break;

            default: // Brute
                hp.maxHealth = hp.health = 240f;
                hp.fireResist = 0.30f;
                ai.moveSpeed = 2.1f; ai.attackDamage = 28f;
                ai.attackRange = 2.9f; ai.attackCooldown = 2.3f; ai.attackAnimTime = 1.0f;
                ai.aggroRange = 20f;
                break;
        }

        return go;
    }

    // ================================================================
    //  QUAI DUNG MODEL SAN (dua tu Meshy vao)
    // ================================================================

    /// <summary>
    /// Dung phan KHUNG chung cho mot con quai lam tu model san: va cham, mau,
    /// khung xuong rong, bo hoat hinh. Rieng chi so chien dau thi tuy loai,
    /// nguoi goi tu dien vao <paramref name="ai"/> tra ve.
    ///
    /// <paramref name="model"/> phai la mot BAN DA DUNG trong canh (khong phai
    /// file goc trong thu muc).
    ///
    /// Khac han bon quai dung bang code o hai cho:
    ///   - Khong gan <see cref="ProceduralAnimator"/>. Bo do xoay cac khop theo
    ///     ten rieng cua no; model Meshy dat ten khop khac han nen no khong tim
    ///     thay gi, ma neu tim thay thi lai da nat clip di bo co san.
    ///   - Van gan mot <see cref="CharacterRig"/> RONG, chi de dien vai so do va
    ///     danh sach lop da vao. Thieu no thi don trung khong loe sang, hieu ung
    ///     chay va choang khong biet nguoi cao bao nhieu ma dat vong lua.
    /// </summary>
    static GameObject DungKhungTuModel(GameObject model, string ten, Transform parent,
                                       Transform player, ModelHoatHinh.KieuDon kieuDon,
                                       out EnemyAI ai, out Damageable hp, out CharacterRig rig)
    {
        var go = new GameObject(ten);
        go.transform.SetParent(parent, false);
        go.tag = "Enemy";

        model.transform.SetParent(go.transform, false);
        model.transform.localPosition = Vector3.zero;
        model.transform.localRotation = Quaternion.identity;

        // ---- Do lay so do that cua model ----
        float cao = 1.9f, banKinh = 0.34f;
        var hinh = model.GetComponentsInChildren<Renderer>(true);
        if (hinh.Length > 0)
        {
            var bao = hinh[0].bounds;
            for (int i = 1; i < hinh.Length; i++) bao.Encapsulate(hinh[i].bounds);
            cao = Mathf.Max(0.6f, bao.size.y);
            // Be ngang tinh ca vu khi va vat ao xoe, chia bot di cho khoi beo
            banKinh = Mathf.Clamp(Mathf.Max(bao.size.x, bao.size.z) * 0.30f, 0.24f, 0.48f);
        }

        // ---- Khung xuong rong, chi de dien so do ----
        rig = go.AddComponent<CharacterRig>();
        rig.bodyHeight = cao;
        rig.bodyRadius = banKinh;
        rig.root = model.transform;
        rig.hips = TimKhop(model.transform, "Hips");
        rig.spine = TimKhop(model.transform, "Spine");
        rig.head = TimKhop(model.transform, "Head");
        rig.handL = TimKhop(model.transform, "LeftHand");
        rig.handR = TimKhop(model.transform, "RightHand");
        rig.castPoint = rig.handR != null ? rig.handR : model.transform;
        rig.hitPoint = rig.spine != null ? rig.spine : model.transform;

        foreach (var r in model.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            rig.skinRenderers.Add(r);

        SetLayerRecursive(go, LayerMask.NameToLayer("Enemy"));

        // ---- Va cham ----
        var cc = go.AddComponent<CharacterController>();
        cc.height = cao * 0.92f;
        cc.radius = banKinh * 0.78f;
        cc.center = new Vector3(0f, cc.height * 0.5f, 0f);
        cc.slopeLimit = 55f;
        cc.stepOffset = 0.4f;

        // ---- Mau ----
        hp = go.AddComponent<Damageable>();
        hp.rig = rig;

        // ---- Hoat hinh rieng ----
        var hh = go.AddComponent<ModelHoatHinh>();
        hh.kieuDon = kieuDon;
        hh.hips = rig.hips;
        hh.spine = rig.spine;
        hh.head = rig.head;
        hh.tayTraiTren = TimKhop(model.transform, "LeftArm");
        hh.tayTraiDuoi = TimKhop(model.transform, "LeftForeArm");
        hh.tayPhaiTren = TimKhop(model.transform, "RightArm");
        hh.tayPhaiDuoi = TimKhop(model.transform, "RightForeArm");
        hh.boPhat = model.GetComponentInChildren<Animation>();
        hh.mau = hp;

        // ---- Tri tue ----
        ai = go.AddComponent<EnemyAI>();
        ai.target = player;
        ai.health = hp;
        ai.anim = null;                  // co chu y: xem ghi chu tren dau ham
        ai.hoatHinhRieng = hh;
        hh.ai = ai;

        return go;
    }

    /// <summary>Mu phu thuy: DUNG XA NEM QUA CAU LUA, bi ap sat thi lui ra.</summary>
    public static GameObject LapRapPhuThuy(GameObject model, Transform parent, Transform player)
    {
        EnemyAI ai; Damageable hp; CharacterRig rig;
        var go = DungKhungTuModel(model, "Enemy_PhuThuy", parent, player,
                                  ModelHoatHinh.KieuDon.NemPhep, out ai, out hp, out rig);

        hp.maxHealth = hp.health = 95f;
        hp.fireResist = 0.35f;      // ba ta song bang lua, lua kho lam gi duoc
        hp.iceResist = -0.20f;      // nhung rat so lanh
        hp.corpseSeconds = 5f;

        ai.danhTuXa = true;
        ai.diemPhongPhep = rig.castPoint;
        ai.moveSpeed = 2.3f;
        ai.turnSpeed = 6f;
        ai.aggroRange = 22f;
        ai.attackRange = 12f;            // dung tu day ma nem
        ai.khoangLui = 6.5f;             // vao gan hon thi lui ra
        ai.attackCooldown = 2f;
        ai.attackAnimTime = 0.85f;
        ai.attackHitAt = 0.45f;          // khop voi mocAn trong ModelHoatHinh
        ai.satThuongCau = 16f;
        ai.banKinhNo = 2.6f;
        ai.tocDoCau = 13f;

        return go;
    }

    /// <summary>
    /// Bo xuong: lao vao chem can chien.
    ///
    /// Chi so giu DUNG NHU con dung bang code truoc day (70 mau, chiu lanh 25%,
    /// 12 sat thuong), de doi hinh khong lam thay doi do kho cua man choi.
    /// Rieng tam voi tay noi ra mot chut vi model nay to va tay dai hon.
    /// </summary>
    public static GameObject LapRapBoXuong(GameObject model, Transform parent, Transform player)
    {
        EnemyAI ai; Damageable hp; CharacterRig rig;
        var go = DungKhungTuModel(model, "Enemy_BoXuong", parent, player,
                                  ModelHoatHinh.KieuDon.ChemToi, out ai, out hp, out rig);

        hp.maxHealth = hp.health = 70f;
        hp.iceResist = 0.25f;            // xuong kho, it so lanh
        hp.corpseSeconds = 6f;

        ai.danhTuXa = false;
        ai.moveSpeed = 2.7f;
        ai.turnSpeed = 8f;
        ai.aggroRange = 15f;
        ai.attackRange = 2.4f;
        ai.attackDamage = 12f;
        ai.attackCooldown = 1.7f;
        ai.attackAnimTime = 0.8f;
        ai.attackHitAt = 0.45f;

        return go;
    }

    /// <summary>
    /// QUY DU: dung tu xa GOI THIEN THACH xuong dau nguoi choi.
    ///
    /// Khac mu phu thuy o cho don danh khong bay theo duong thang tu tay no ma
    /// ROI TU TREN TROI xuong cho nguoi choi dang dung. Nguoi choi buoc ra la
    /// tranh duoc - nho vay moi cho no ban duoc tu xa ma van choi duoc.
    ///
    /// Hoi cham va hoi trau hon phu thuy, nhung don dau hon va tam xa hon.
    /// </summary>
    public static GameObject LapRapQuyDu(GameObject model, Transform parent, Transform player)
    {
        EnemyAI ai; Damageable hp; CharacterRig rig;
        var go = DungKhungTuModel(model, "Enemy_QuyDu", parent, player,
                                  ModelHoatHinh.KieuDon.NemPhep, out ai, out hp, out rig);

        hp.maxHealth = hp.health = 130f;
        hp.fireResist = 0.40f;      // no song bang lua, thien thach khong lam gi duoc no
        hp.iceResist = -0.15f;
        hp.corpseSeconds = 5f;

        ai.danhTuXa = true;
        ai.danhBangThienThach = true;
        ai.diemPhongPhep = rig.castPoint;
        ai.moveSpeed = 2.4f;
        ai.turnSpeed = 6f;

        // aggroRange PHAI lon hon attackRange. De nho hon thi no chi bat dau
        // chu y den nguoi choi khi da vao trong tam ban - tuc khong bao gio
        // dung o ria tam ma danh, phi ca cai tam ban.
        ai.aggroRange = 24f;
        ai.attackRange = 10f;
        ai.khoangLui = 7f;

        ai.attackCooldown = 2f;
        ai.attackAnimTime = 0.9f;
        ai.attackHitAt = 0.45f;

        ai.satThuongCau = 22f;      // ThienThach.impactDamage
        ai.banKinhNo = 2.8f;

        return go;
    }

    /// <summary>
    /// QUY CAY: chay NHANH HON NGUOI CHOI va ban tia set xanh la tu 8 m.
    ///
    /// Nguoi choi di 5,2 m/s, con nay 5,98 - nhanh hon 15%. Do la con quai duy
    /// nhat trong game KHONG THE CHAY THOAT: cu quay lung bo chay la no bam
    /// theo va rut ngan khoang cach. Muon thoat phai giet, phai dong bang, hoac
    /// phai nup sau cai gi do.
    ///
    /// Vi the tam ban chi 8 m va no CHIU DON KEM (110 mau, so lua): mot con
    /// vua nhanh vua ban xa vua trau thi khong con cach nao choi lai.
    ///
    /// Khoang lui de 4,5 m chu khong 7 nhu Quy du. No chay nhanh gap doi bon
    /// kia, de nguong lui xa thi ca dan cu giat lui ra roi lai lao vao, nhin ra
    /// mot dam gian gio chu khong ra mot dan quai vay danh.
    /// </summary>
    public static GameObject LapRapQuyCay(GameObject model, Transform parent, Transform player)
    {
        EnemyAI ai; Damageable hp; CharacterRig rig;
        var go = DungKhungTuModel(model, "Enemy_QuyCay", parent, player,
                                  ModelHoatHinh.KieuDon.NemPhep, out ai, out hp, out rig);

        hp.maxHealth = hp.health = 110f;
        hp.fireResist = -0.25f;     // cay kho, lua an rat manh
        hp.lightningResist = 0.35f; // no song bang set, set kho lam gi duoc
        hp.corpseSeconds = 5f;

        ai.danhTuXa = true;
        ai.danhBangTiaSet = true;
        ai.diemPhongPhep = rig.castPoint;

        // 5,98 = 5,2 x 1,15. Nguoi choi KHONG chay thoat duoc.
        ai.moveSpeed = 5.98f;
        ai.turnSpeed = 9f;

        ai.aggroRange = 20f;
        ai.attackRange = 8f;
        ai.khoangLui = 4.5f;

        ai.attackCooldown = 1.5f;
        ai.attackAnimTime = 0.6f;
        ai.attackHitAt = 0.40f;

        ai.satThuongCau = 14f;

        // Mau nam o QUANG chu khong o loi - xem GiatSet.PhongCuaQuai
        ai.tiaMauLoi = new Color(0.85f, 1f, 0.85f, 1f);
        ai.tiaMauQuang = new Color(0.25f, 1f, 0.30f, 1f);

        return go;
    }

    /// <summary>Tim mot khop theo ten trong ca cay con chau.</summary>
    static Transform TimKhop(Transform goc, string ten)
    {
        if (goc == null) return null;
        if (goc.name == ten) return goc;
        for (int i = 0; i < goc.childCount; i++)
        {
            var kq = TimKhop(goc.GetChild(i), ten);
            if (kq != null) return kq;
        }
        return null;
    }

    public static void SetLayerRecursive(GameObject go, int layer)
    {
        if (layer < 0) return;
        go.layer = layer;
        for (int i = 0; i < go.transform.childCount; i++)
            SetLayerRecursive(go.transform.GetChild(i).gameObject, layer);
    }
}
