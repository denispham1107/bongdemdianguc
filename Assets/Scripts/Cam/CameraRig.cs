using UnityEngine;

public enum ViewMode
{
    ThirdPerson3D = 0,   // 3D theo vai, xoay tu do
    Isometric25D = 1,    // 2.5D nghieng kieu Diablo 2
    TopDown2D = 2,       // 2D nhin thang tu tren xuong
    Side2D = 3           // 2D nhin ngang nhu game man ngang
}

/// <summary>
/// CAMERA DOI GOC NHIN TU DO: 3D -> 2.5D -> 2D.
///
/// Meo o day: luon dung MOT camera phoi canh. Muon ra hinh "phang nhu 2D"
/// thi lui camera that xa va thu hep goc nhin lai - anh se gan nhu khong con
/// do sau, giong het che do truc giao (orthographic), nhung van chuyen canh
/// muot ma giua cac che do.
/// </summary>
public class CameraRig : MonoBehaviour
{
    [System.Serializable]
    public class View
    {
        public string name;
        public float yaw;
        public float pitch;
        public float distance;
        public float fov;
        public float height;         // ngam cao hon nhan vat bao nhieu
        public bool freeOrbit;       // cho phep xoay bang chuot / Q E
    }

    [Header("Muc tieu bam theo")]
    public Transform target;
    public float followSmooth = 0.12f;

    [Header("Cac goc nhin (bam C de doi)")]
    public View[] views =
    {
        new View { name = "3D tu do",        yaw = 25f, pitch = 22f, distance = 7.5f,  fov = 55f, height = 1.1f, freeOrbit = true },
        new View { name = "2.5D (Diablo)",   yaw = 45f, pitch = 48f, distance = 13f,   fov = 36f, height = 1.0f, freeOrbit = false },
        new View { name = "2D tu tren xuong",yaw = 0f,  pitch = 88f, distance = 40f,   fov = 15f, height = 0.6f, freeOrbit = false },
        new View { name = "2D nhin ngang",   yaw = 0f,  pitch = 8f,  distance = 38f,   fov = 13f, height = 1.3f, freeOrbit = false },
    };

    [Header("Giu CHUOT PHAI de tu xoay camera")]
    [Tooltip("Keo chuot cang xa thi xoay cang nhieu")]
    public float orbitSensitivity = 4.2f;
    [Tooltip("Goc chuc xuong thap nhat / cao nhat, do")]
    public float minPitch = 4f;
    public float maxPitch = 88f;

    [Header("Phim")]
    public KeyCode cycleKey = KeyCode.C;
    public KeyCode rotateLeftKey = KeyCode.Q;
    public KeyCode rotateRightKey = KeyCode.E;

    [Header("Chuyen canh")]
    public float blendSpeed = 3.2f;
    public float zoomSpeed = 4f;

    /// <summary>Do lech goc ngang do nguoi choi tu xoay. GIU LAI sau khi tha chuot.</summary>
    public float YawOffset { get { return yawOffset; } }

    /// <summary>Do lech goc chuc do nguoi choi tu xoay. GIU LAI sau khi tha chuot.</summary>
    public float PitchOffset { get { return pitchOffset; } }

    /// <summary>Do lech phong to/thu nho. Cong khai de bang chan doan hien duoc.</summary>
    public float ZoomOffset { get { return zoomOffset; } }

    public int CurrentIndex { get; private set; }
    public ViewMode Mode { get { return (ViewMode)CurrentIndex; } }
    public string CurrentName { get { return views[CurrentIndex].name; } }
    public float LastSwitchTime { get; private set; } = -99f;

    /// <summary>Huong "truoc mat" tren mat dat theo camera (de dieu khien WASD).</summary>
    public Vector3 FlatForward
    {
        get
        {
            Vector3 f = transform.forward;
            f.y = 0f;
            if (f.sqrMagnitude < 0.001f) f = Vector3.forward;
            return f.normalized;
        }
    }

    Camera cam;
    float curYaw, curPitch, curDist, curFov, curHeight;
    // Do nhay khi keo ngon tren man hinh cam ung: bao nhieu DO xoay cho moi
    // DIEM ANH keo di. 0,16 nghia la keo ngang 400 diem anh xoay duoc 64 do -
    // vua het mot lan quet ngon cai la thay duoc canh ben.
    const float nhayCamUng = 0.16f;

    float yawOffset, zoomOffset;
    float pitchOffset;      // do lech goc chuc do nguoi choi tu xoay - GIU LAI sau khi tha chuot
    bool orbiting;          // dang giu chuot phai de xoay
    Vector3 focus, focusVel;

    // Rung camera
    static CameraRig instance;
    float shakeTime, shakeDur, shakeAmp;

    void Awake()
    {
        instance = this;
        cam = GetComponent<Camera>();
        if (cam == null) cam = gameObject.AddComponent<Camera>();
        SnapToView();
    }

    void Start()
    {
        if (target == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) target = p.transform;
        }
        if (target != null) focus = target.position;
        SnapToView();
    }

    void SnapToView()
    {
        var v = views[CurrentIndex];
        curYaw = v.yaw + yawOffset;
        curPitch = v.pitch + pitchOffset;
        curDist = v.distance + zoomOffset;
        curFov = v.fov;
        curHeight = v.height;
    }

    public void SetView(int index)
    {
        // Khoa goc nhin thi doi che do cung khong duoc - do la mot cach doi goc
        // nhin nhu moi cach khac.
        if (CamUng.KhoaCam) return;

        index = Mathf.Clamp(index, 0, views.Length - 1);
        if (index == CurrentIndex) return;
        CurrentIndex = index;
        LastSwitchTime = Time.time;
        zoomOffset = 0f;
        yawOffset = 0f;
        pitchOffset = 0f;   // bam C la ve dung goc nhin chuan cua che do do
    }

    public void CycleView()
    {
        SetView((CurrentIndex + 1) % views.Length);
    }

    void Update()
    {
        // ================================================================
        //  KHOA GOC NHIN: dung im, khong doc bat cu thao tac nao
        // ================================================================
        //
        // Ra o NGAY DAU Update chu khong loc tung thao tac mot. Loc tung cai
        // thi moi duong vao moi phai nho chan lai - phim, chuot, con lan, cu
        // chi cam ung, nut doi goc nhin - va chi can bo sot mot cai la goc nhin
        // van nhay duoc du dang khoa.
        //
        // Khong dung cur* lai lam gi: chung da la goc dang co, va khong ai cap
        // nhat chung nua thi camera dung yen o dung do. LateUpdate van chay
        // binh thuong nen camera VAN BAM THEO nhan vat - khoa la khoa goc nhin,
        // khong phai dong bang camera lai mot cho.
        if (CamUng.KhoaCam)
        {
            orbiting = false;
            return;
        }

        // ---- Doi goc nhin ----
        // Chi tren PC. May cam ung khong co ban phim that, nhung ban phim ao cua
        // he dieu hanh van co the bat len va gui phim vao game.
        if (!CamUng.DangDung)
        {
            if (Input.GetKeyDown(cycleKey)) CycleView();
            if (Input.GetKeyDown(KeyCode.F1)) SetView(0);
            if (Input.GetKeyDown(KeyCode.F2)) SetView(1);
            if (Input.GetKeyDown(KeyCode.F3)) SetView(2);
            if (Input.GetKeyDown(KeyCode.F4)) SetView(3);
        }

        var v = views[CurrentIndex];

        // TREN MAY CAM UNG, KHONG AI DUOC XOAY CAMERA.
        //
        // Nguoi choi tren dien thoai khong co chuot phai, chuot giua hay con lan
        // - nhung TRINH DUYET van gui nhung thu do vao. Dat hai ngon len man hinh
        // (mot ngon giu can, mot ngon bam ky nang) la Safari/Chrome hieu thanh cu
        // chi CUON hoac PINCH cua trang web, roi bao lai cho Unity thanh
        // "Mouse ScrollWheel". Giu lau mot cho co the thanh chuot phai. Nguoi choi
        // khong he lam gi voi camera ma goc nhin tu nhien nhay di.
        //
        // Xoa ba do lech ngay tu dau moi khung, vo dieu kien. Day la chot chan:
        // du sau nay co cho nao khac day gia tri vao - mot cu chi trinh duyet moi,
        // hay chinh doan duoi day sot lai - thi khung hinh sau no cung bi xoa,
        // camera luon tro ve dung goc chuan cua che do nhin.
        bool camUng = CamUng.DangDung;
        if (camUng)
        {
            // CHI nhan cu chi do HUD tu doc tu Input.touches - da loai ngon dang
            // giu can va ngon dang bam nut ra. Moi duong chuot va con lan o duoi
            // van bi chan sach, vi tren dien thoai chung khong phai nguoi choi
            // gui ma la trinh duyet tu dich ra.
            orbiting = CamUng.XoayCam.sqrMagnitude > 1f;
            if (orbiting)
                Orbit(CamUng.XoayCam.x * nhayCamUng, CamUng.XoayCam.y * nhayCamUng);

            if (Mathf.Abs(CamUng.ChumZoom) > 0.5f)
                zoomOffset = Mathf.Clamp(zoomOffset - CamUng.ChumZoom * v.distance * 0.0022f,
                                         -v.distance * 0.45f, v.distance * 1.1f);
        }

        // ---- GIU CHUOT PHAI: tu xoay camera moi huong ----
        // Do lech duoc CONG DON vao yawOffset / pitchOffset, ma hai bien do khong
        // bi xoa khi tha chuot -> tha ra la camera dung yen o dung goc vua xoay toi.
        if (!camUng)
        {
            orbiting = Input.GetMouseButton(1);
            if (orbiting)
                Orbit(Input.GetAxis("Mouse X") * orbitSensitivity,
                      Input.GetAxis("Mouse Y") * orbitSensitivity);
        }

        // ---- Xoay ngang bang phim Q / E hoac chuot giua ----
        float rot = 0f;
        if (!camUng)
        {
            if (Input.GetKey(rotateLeftKey)) rot -= 1f;
            if (Input.GetKey(rotateRightKey)) rot += 1f;
            if (Input.GetMouseButton(2))
                rot += Input.GetAxis("Mouse X") * 6f;
        }

        if (Mathf.Abs(rot) > 0.001f)
            yawOffset += rot * (v.freeOrbit ? 90f : 60f) * Time.deltaTime;

        // ---- Phong to / thu nho ----
        float wheel = camUng ? 0f : Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(wheel) > 0.001f)
            zoomOffset = Mathf.Clamp(zoomOffset - wheel * v.distance * 0.9f,
                                     -v.distance * 0.45f, v.distance * 1.1f);

        // ---- Goc chuc ----
        float pitchTarget = Mathf.Clamp(v.pitch + pitchOffset, minPitch, maxPitch);
        if (!camUng && Input.GetMouseButton(2))
            pitchTarget = Mathf.Clamp(curPitch - Input.GetAxis("Mouse Y") * 4f, minPitch, maxPitch);

        // ---- Truot muot sang thong so cua goc nhin dang chon ----
        // Luc dang keo chuot thi bam that sat, khong tre - co the thi moi thay "dinh tay".
        float speed = orbiting ? 26f : blendSpeed;
        float k = 1f - Mathf.Exp(-speed * Time.deltaTime);
        curYaw = Mathf.LerpAngle(curYaw, v.yaw + yawOffset, k);
        curPitch = Mathf.Lerp(curPitch, pitchTarget, k);
        curDist = Mathf.Lerp(curDist, v.distance + zoomOffset, k);
        curFov = Mathf.Lerp(curFov, v.fov, k);
        curHeight = Mathf.Lerp(curHeight, v.height, k);
    }

    /// <summary>
    /// Xoay camera di mot doan. Do lech duoc CONG DON va khong bi xoa khi tha chuot,
    /// nen tha tay ra la camera dung yen o dung goc vua xoay toi.
    /// Chi bam phim C (hoac F1..F4) moi tra ve goc chuan cua che do nhin.
    /// </summary>
    public void Orbit(float deltaYaw, float deltaPitch)
    {
        // Lop chan THU HAI, canh cai o dau Update.
        //
        // Chan o Update thoi thi van chua du: do lech VAN duoc cong don trong
        // luc khoa, va den luc nguoi choi mo khoa ra thi camera nhay mot phat
        // sang goc da tich luy suot thoi gian do.
        if (CamUng.KhoaCam) return;

        var v = views[CurrentIndex];

        yawOffset += deltaYaw;
        pitchOffset -= deltaPitch;

        // Kep ngay tai day de do lech khong tich luy vo han khi da cham tran / san
        pitchOffset = Mathf.Clamp(v.pitch + pitchOffset, minPitch, maxPitch) - v.pitch;
    }

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 want = target.position + Vector3.up * curHeight;
        focus = Vector3.SmoothDamp(focus, want, ref focusVel, followSmooth);

        Quaternion rot = Quaternion.Euler(curPitch, curYaw, 0f);
        Vector3 pos = focus - rot * Vector3.forward * curDist;

        // Rung khi no
        if (shakeTime < shakeDur)
        {
            shakeTime += Time.deltaTime;
            float fade = 1f - Mathf.Clamp01(shakeTime / Mathf.Max(0.01f, shakeDur));
            Vector3 off = new Vector3(
                Mathf.PerlinNoise(Time.time * 32f, 0f) - 0.5f,
                Mathf.PerlinNoise(0f, Time.time * 30f) - 0.5f,
                Mathf.PerlinNoise(Time.time * 27f, 7f) - 0.5f);
            pos += off * shakeAmp * fade * 2f;
        }

        transform.position = pos;
        transform.rotation = rot;
        if (cam != null) cam.fieldOfView = curFov;
    }

    // ================================================================
    //  RUNG CAMERA
    // ================================================================
    public static void Shake(float duration, float amplitude)
    {
        if (instance == null) return;
        instance.shakeDur = duration;
        instance.shakeAmp = Mathf.Max(instance.shakeAmp * 0.4f, amplitude);
        instance.shakeTime = 0f;
    }
}

/// <summary>Cho cac script khac goi rung camera cho gon.</summary>
public static class CameraShake
{
    public static void Shake(float duration, float amplitude)
    {
        CameraRig.Shake(duration, amplitude);
    }
}
