using UnityEngine;

/// <summary>
/// DIEM KHOI DAU CUA GAME (dat tren vat the "GAME" trong scene).
///
/// Cach lam viec:
/// - Neu scene DA co san the gioi va nhan vat (do cong cu "nuong asset" dung ra,
///   lay tu Assets/Prefabs), script nay chi noi cac thanh phan lai voi nhau.
/// - Neu scene con trong, no se TU DUNG toan bo man choi bang code de game van chay.
///
/// Nho vay ban vua sua tay duoc tung vat the trong scene, vua khong so game vo.
/// </summary>
public class GameBootstrap : MonoBehaviour
{
    [Header("Kich thuoc dau truong")]
    public float arenaRadius = 34f;
    public int worldSeed = 12345;

    [Header("Nhan vat")]
    // 30 000 mau la de CHAY THU cho lau, khong phai con so can bang cua game.
    // Muc that la 400 - doi lai truoc khi phat hanh, khong thi quai danh ca
    // buoi khong het mot thanh mau va khong ai biet vi sao.
    //
    // Con so nay nam trong CA HAI SCENE (Act1.unity, Act2.unity) nua: Unity
    // luu gia tri cua component vao scene, va gia tri do DE LEN mac dinh viet
    // o day. Sua moi mot cho la khong doi gi ca.
    public float playerMaxHealth = 30000f;
    public float playerMaxMana = 250f;

    [Header("Do hoa")]
    public bool useBloom = true;
    public float bloomIntensity = 0.9f;

    [Header("Tu dung neu scene con trong")]
    [Tooltip("Bat: neu scene chua co canh vat thi tu dung bang code.")]
    public bool buildWorldIfMissing = true;

    PlayerController player;
    CameraRig camRig;

    void Awake()
    {
        Application.targetFrameRate = 120;
        // Khoang bong theo muc do hoa nguoi choi chon (Cao 60 m nhu truoc day)
        QualitySettings.shadowDistance = CaiDatDoHoa.KhoangBong;

        bool worldReady = GameObject.Find("World") != null;
        if (!worldReady && buildWorldIfMissing)
        {
            var world = new GameObject("World").transform;
            WorldFactory.Build(world, arenaRadius, worldSeed);
        }
        else
        {
            // The gioi da co san trong scene - chi can dat lai bau troi va suong mu
            WorldFactory.BuildSkyAndFog();
        }

        BuildSun();
        var playerGo = EnsurePlayer();
        BuildCamera(playerGo.transform);
        EnsureDirector(playerGo.transform);
        EnsureHud();
    }

    // ================================================================
    //  ANH SANG CHUNG (anh trang lanh hat tu tren cao)
    // ================================================================
    void BuildSun()
    {
        Light sun = null;
        var existing = FindObjectsByType<Light>(FindObjectsInactive.Exclude);
        for (int i = 0; i < existing.Length; i++)
            if (existing[i].type == LightType.Directional) { sun = existing[i]; break; }

        if (sun == null)
        {
            var go = new GameObject("Moonlight");
            sun = go.AddComponent<Light>();
        }

        WorldFactory.SetupMoonlight(sun);
    }

    // ================================================================
    //  PHU THUY
    // ================================================================

    /// <summary>Tim nhan vat trong scene; khong co thi lay tu prefab; cuoi cung moi dung bang code.</summary>
    GameObject EnsurePlayer()
    {
        var existing = GameObject.FindGameObjectWithTag("Player");

        if (existing == null && GameAssets.I != null && GameAssets.I.playerPrefab != null)
        {
            existing = Instantiate(GameAssets.I.playerPrefab, new Vector3(0f, 1.2f, 0f), Quaternion.identity);
            existing.name = "Player";
        }

        if (existing == null) existing = BuildPlayerFromCode();

        player = existing.GetComponent<PlayerController>();
        var hp = existing.GetComponent<Damageable>();
        if (hp != null)
        {
            hp.maxHealth = playerMaxHealth;

            // VAO MAN LA DAY MAU, khong hoi han. Truoc day dong nay chi va khi
            // gia tri BAT THUONG (am, hoac vuot tran) - nen khi doi muc mau
            // len, nhan vat da nuong san trong scene giu nguyen con so cu:
            // maxHealth thanh 30 000 ma health van 400, thanh mau day mot vach
            // do o dau. Doi muc mau ma quen dong nay thi trong het nhu chua sua
            // gi ca.
            hp.health = playerMaxHealth;
        }
        if (player != null)
        {
            player.maxMana = playerMaxMana;
            player.mana = playerMaxMana;
        }
        return existing;
    }

    /// <summary>Dung nhan vat bang code (dung de "nuong" ra prefab, hoac khi thieu prefab).</summary>
    /// <summary>
    /// Lap nhan vat nguoi choi quanh mot MODEL SAN (dua tu Meshy vao).
    ///
    /// <paramref name="model"/> phai la mot BAN DA DUNG trong canh (khong phai
    /// file goc trong thu muc).
    ///
    /// Khac ban dung bang code o hai cho:
    ///   - Khong gan <see cref="ProceduralAnimator"/>. Bo do xoay khop theo ten
    ///     rieng cua no; model Meshy dat ten khop khac han nen no khong tim thay
    ///     gi, ma neu tim thay thi lai da nat clip di bo co san.
    ///   - Van gan mot <see cref="CharacterRig"/> RONG de dien so do va danh
    ///     sach lop da. Thieu no thi don trung khong loe sang, va nhat la
    ///     <c>castPoint</c> - cho phep bay ra - se khong co.
    /// </summary>
    public GameObject BuildPlayerFromModel(GameObject model)
    {
        var go = new GameObject("Player");
        go.tag = "Player";
        go.transform.position = new Vector3(0f, 1.2f, 0f);

        model.transform.SetParent(go.transform, false);
        model.transform.localPosition = Vector3.zero;
        model.transform.localRotation = Quaternion.identity;

        // ---- Do so do that cua model ----
        float cao = 1.9f, banKinh = 0.34f;
        var hinh = model.GetComponentsInChildren<Renderer>(true);
        if (hinh.Length > 0)
        {
            var bao = hinh[0].bounds;
            for (int i = 1; i < hinh.Length; i++) bao.Encapsulate(hinh[i].bounds);
            cao = Mathf.Max(0.8f, bao.size.y);

            // CHIEU CAO thi do tu model, con BE NGANG thi KHONG.
            // Hop bao cua model tinh ca cay gay chia ngang va vat ao xoe, do ra
            // toi 1.5 m ngang - lay theo do thi cai tru va cham beo gap ruoi
            // than nguoi, nhan vat se vuong vao moi goc da moi than cay.
            // Than nguoi thuc su chi rong khoang 0.6 m.
            banKinh = 0.32f;
        }

        // ---- Khung xuong rong, chi de dien so do va diem phong phep ----
        var rig = go.AddComponent<CharacterRig>();
        rig.bodyHeight = cao;
        rig.bodyRadius = banKinh;
        rig.root = model.transform;
        rig.hips = TimKhop(model.transform, "Hips");
        rig.spine = TimKhop(model.transform, "Spine");
        rig.head = TimKhop(model.transform, "Head");
        rig.handL = TimKhop(model.transform, "LeftHand");
        rig.handR = TimKhop(model.transform, "RightHand");
        rig.hitPoint = rig.spine != null ? rig.spine : model.transform;

        // Phep phai bay ra tu DAU GAY, khong phai tu nam tay. Cay gay dinh lien
        // vao luoi nen khong co khop rieng; dat mot diem trong nhoi len phia
        // truoc va len cao so voi ban tay, roi ngam chung vao do.
        var diem = new GameObject("CastPoint");
        diem.transform.SetParent(rig.handR != null ? rig.handR : model.transform, false);

        if (rig.handR != null)
        {
            // KHOP BAN TAY CO TRUC RIENG, khong trung truc cua nhan vat. Dat
            // thang localPosition = (0, 0.34, 0.16) thi cai 0.34 do KHONG phai
            // la "len tren" ma la mot huong nao do tuy theo xuong quay the nao -
            // lan truoc no day diem phep xuong ngang dau goi.
            // Phai neu y muon bang huong NGOAI THE roi doi ve huong rieng cua khop.
            Vector3 lech = model.transform.forward * 0.20f + Vector3.up * 0.12f;
            diem.transform.localPosition = rig.handR.InverseTransformVector(lech);
        }
        else
        {
            diem.transform.localPosition = new Vector3(0f, cao * 0.62f, 0.25f);
        }

        rig.castPoint = diem.transform;

        foreach (var r in model.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            rig.skinRenderers.Add(r);

        // ---- Va cham ----
        var cc = go.AddComponent<CharacterController>();
        cc.height = cao * 0.88f;
        cc.radius = banKinh;
        cc.center = new Vector3(0f, cc.height * 0.5f, 0f);
        cc.slopeLimit = 55f;      // doc hon 55 do thi truot xuong
        cc.stepOffset = 0.55f;    // buoc len duoc go da cao toi 55cm

        // ---- Mau ----
        var hp = go.AddComponent<Damageable>();
        hp.isPlayer = true;
        hp.maxHealth = playerMaxHealth;
        hp.health = playerMaxHealth;
        hp.rig = rig;

        // ---- Hoat hinh rieng ----
        var hh = go.AddComponent<NguoiChoiHoatHinh>();
        hh.hips = rig.hips;
        hh.spine = rig.spine;
        hh.head = rig.head;
        hh.tayTraiTren = TimKhop(model.transform, "LeftArm");
        hh.tayTraiDuoi = TimKhop(model.transform, "LeftForeArm");
        hh.tayPhaiTren = TimKhop(model.transform, "RightArm");
        hh.tayPhaiDuoi = TimKhop(model.transform, "RightForeArm");
        hh.boPhat = model.GetComponentInChildren<Animation>();
        hh.vaCham = cc;
        hh.mau = hp;

        // ---- Dieu khien ----
        var pc = go.AddComponent<PlayerController>();
        pc.rig = rig;
        pc.anim = null;                  // co chu y: xem ghi chu tren dau ham
        pc.hoatHinhRieng = hh;
        pc.health = hp;
        pc.maxMana = playerMaxMana;
        pc.mana = playerMaxMana;
        hh.tocDoDiToiDa = pc.moveSpeed;

        // Mot vung sang mo theo chan nhan vat cho de nhin trong dem
        var halo = new GameObject("HeroLight");
        halo.transform.SetParent(go.transform, false);
        halo.transform.localPosition = new Vector3(0f, 2.6f, 0f);
        var hl = halo.AddComponent<Light>();
        hl.type = LightType.Point;
        hl.color = new Color(0.62f, 0.72f, 0.95f);
        hl.intensity = 0.9f;
        hl.range = 8f;
        hl.shadows = LightShadows.None;

        EnemyFactory.SetLayerRecursive(go, LayerMask.NameToLayer("Player"));
        go.tag = "Player";
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

    public GameObject BuildPlayerFromCode()
    {
        var go = new GameObject("Player");
        go.tag = "Player";
        go.transform.position = new Vector3(0f, 1.2f, 0f);

        var rig = WizardFactory.Build(go.transform);

        var anim = go.AddComponent<ProceduralAnimator>();
        anim.rig = rig;
        anim.casterPose = true;
        anim.strideScale = 1.0f;

        var cc = go.AddComponent<CharacterController>();
        cc.height = 1.7f;
        cc.radius = 0.32f;
        cc.center = new Vector3(0f, 0.87f, 0f);
        cc.slopeLimit = 55f;      // doc hon 55 do thi truot xuong
        cc.stepOffset = 0.55f;    // buoc len duoc go da cao toi 55cm

        var hp = go.AddComponent<Damageable>();
        hp.isPlayer = true;
        hp.maxHealth = playerMaxHealth;
        hp.health = playerMaxHealth;
        hp.rig = rig;
        hp.anim = anim;

        var pc = go.AddComponent<PlayerController>();
        pc.rig = rig;
        pc.anim = anim;
        pc.health = hp;
        pc.maxMana = playerMaxMana;
        pc.mana = playerMaxMana;

        // Mot vung sang mo theo chan nhan vat cho de nhin trong dem
        var halo = new GameObject("HeroLight");
        halo.transform.SetParent(go.transform, false);
        halo.transform.localPosition = new Vector3(0f, 2.6f, 0f);
        var hl = halo.AddComponent<Light>();
        hl.type = LightType.Point;
        hl.color = new Color(0.62f, 0.72f, 0.95f);
        hl.intensity = 0.9f;
        hl.range = 8f;
        hl.shadows = LightShadows.None;

        EnemyFactory.SetLayerRecursive(go, LayerMask.NameToLayer("Player"));
        go.tag = "Player";
        return go;
    }

    // ================================================================
    //  CAMERA
    // ================================================================
    void BuildCamera(Transform target)
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            var go = new GameObject("Main Camera");
            go.tag = "MainCamera";
            cam = go.AddComponent<Camera>();
            go.AddComponent<AudioListener>();
        }

        cam.clearFlags = CameraClearFlags.Skybox;
        // near 1,5 chu khong 0,5, far 220 chu khong 400.
        //
        // Do chinh xac cua bo dem chieu sau KHONG phu thuoc far, ma phu thuoc
        // NEAR - va giam theo binh phuong khoang cach:
        //
        //     khe phan biet duoc ~ z^2 / (near * 2^bit)
        //
        // Voi vat o cach may quay 15 m, tren WebGL dien thoai (bo dem 16 bit):
        //
        //     near 0,15 -> 22,9 mm
        //     near 0,5  ->  6,9 mm
        //     near 1,5  ->  2,3 mm     <- dang dung
        //
        // Ba con so do la ly do "o vuong den" xuat hien tren dien thoai ma khong
        // co tren may ban: may ban 24 bit, cung o 15 m chi can 0,027 mm. Nang
        // near len la cach chua RE NHAT - khong dong den mot mo hinh nao.
        //
        // Cat gi khong: may quay gan nhat la 4,1 m (zoom het co o goc nhin 3D tu
        // do), va cho gan nhat cua nhan vat luc do van con cach 3,5 m. Xa nhat
        // qua ban do 109 m la khoang 150 m - o 200 m suong mu da che 98,8% roi.
        cam.nearClipPlane = 1.5f;
        cam.farClipPlane = 220f;
        cam.allowHDR = true;

        camRig = cam.GetComponent<CameraRig>();
        if (camRig == null) camRig = cam.gameObject.AddComponent<CameraRig>();
        camRig.target = target;

        if (useBloom)
        {
            var bloom = cam.GetComponent<SimpleBloom>();
            if (bloom == null) bloom = cam.gameObject.AddComponent<SimpleBloom>();
            bloom.intensity = bloomIntensity;
        }

        if (player != null) player.cameraRig = camRig;
    }

    // ================================================================
    //  NGUOI THA QUAI + THANH THONG TIN
    // ================================================================
    void EnsureDirector(Transform playerTf)
    {
        var dir = FindAnyObjectByType<GameDirector>();
        if (dir == null)
        {
            var go = new GameObject("GameDirector");
            dir = go.AddComponent<GameDirector>();
        }
        dir.player = playerTf;
        dir.arenaCenter = Vector3.zero;
        dir.arenaRadius = arenaRadius;
    }

    void EnsureHud()
    {
        var hud = FindAnyObjectByType<GameHUD>();
        if (hud == null)
        {
            var go = new GameObject("HUD");
            hud = go.AddComponent<GameHUD>();
        }

        hud.player = player;
        hud.playerHealth = player != null ? player.GetComponent<Damageable>() : null;
        hud.cameraRig = camRig;
        hud.director = GameDirector.Instance;
        if (hud.director == null) hud.director = FindAnyObjectByType<GameDirector>();
    }
}
