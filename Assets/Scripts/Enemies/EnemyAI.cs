using UnityEngine;

/// <summary>
/// TRI TUE QUAI VAT (rat don gian, dung kieu Diablo):
/// dung tai cho -> thay nguoi choi trong tam thi duoi theo -> den gan thi vung don.
/// Bi dong bang thi cham hoac dung han, bi chay thi van duoi nhung mat mau dan.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class EnemyAI : MonoBehaviour
{
    [Header("Di chuyen")]
    public float moveSpeed = 2.6f;

    /// <summary>Co thi loi nuoc se lam cham lai. Khong co cung chay binh thuong.</summary>
    public LoiNuoc loiNuoc;
    public float turnSpeed = 8f;

    [Header("Chien dau")]
    public float aggroRange = 14f;
    public float attackRange = 1.9f;
    public float attackDamage = 9f;
    public float attackCooldown = 1.6f;
    public float attackAnimTime = 0.75f;
    public float attackHitAt = 0.45f;      // % thoi diem trong don danh gay sat thuong

    [Header("Danh tu xa (de trong thi danh can chien nhu cu)")]
    [Tooltip("Bat len thi khong lao vao dam ma dung tu xa nem phep")]
    public bool danhTuXa = false;
    [Tooltip("Vao gan hon khoang nay thi LUI RA, khong de nguoi choi ap sat")]
    public float khoangLui = 5.5f;
    public float satThuongCau = 16f;
    public float banKinhNo = 2.6f;
    public float tocDoCau = 13f;

    [Tooltip("Bat len thi thay qua cau lua bang THIEN THACH goi tu tren troi xuong")]
    public bool danhBangThienThach = false;
    [Tooltip("Bat len thi thay qua cau lua bang TIA SET phong thang sang nguoi choi")]
    public bool danhBangTiaSet = false;
    public Color tiaMauLoi = Color.white;
    public Color tiaMauQuang = new Color(0.45f, 0.70f, 1f, 1f);
    [Tooltip("Cho phep phep bay ra - de trong thi tu tim xuong ban tay")]
    public Transform diemPhongPhep;

    [Header("Tham chieu")]
    public Transform target;
    public ProceduralAnimator anim;
    public Damageable health;
    [Tooltip("Quai dung model san (khong co ProceduralAnimator) thi gan bo hoat hinh rieng vao day")]
    public ModelHoatHinh hoatHinhRieng;

    CharacterController cc;
    float cooldown;
    float attackTimer = -1f;
    bool hitApplied;
    Vector3 velocity;
    Vector3 wanderDir;
    float wanderTimer;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        // Tu gan phan loi nuoc. Gan o day chu khong gan luc dung ban do vi quai
        // duoc tao tu prefab, con nguoi choi thi moi man mot duong dung khac
        // nhau - gan o Awake thi cho nao cung co, va man khong co nuoc thi
        // LoiNuoc tra ve he so 1 nen khong ton gi.
        if (loiNuoc == null)
        {
            loiNuoc = GetComponent<LoiNuoc>();
            if (loiNuoc == null) loiNuoc = gameObject.AddComponent<LoiNuoc>();
        }
        if (anim == null) anim = GetComponentInChildren<ProceduralAnimator>();
        if (health == null) health = GetComponent<Damageable>();
    }

    void Start()
    {
        if (target == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) target = p.transform;
        }
        wanderDir = Random.insideUnitSphere;
        wanderDir.y = 0f;
        wanderDir.Normalize();
    }

    void Update()
    {
        if (health != null && health.IsDead) return;

        float dt = Time.deltaTime;
        if (cooldown > 0f) cooldown -= dt;

        // Bi dong bang / lam cham
        float speedMul = 1f;
        if (loiNuoc != null) speedMul *= loiNuoc.HeSoToc;   // loi nuoc thi nang chan
        var frozen = GetComponent<FrozenEffect>();
        if (frozen != null) speedMul = Mathf.Clamp01(1f - frozen.slow);

        // Dang bi loc cuon len troi thi khong tu dieu khien duoc nua
        if (GetComponent<WhirledEffect>() != null) return;

        // Bi DONG BANG hoan toan: dung im va KHONG DANH DUOC.
        //
        // Truoc day dong bang chi giam speedMul o tren, tuc con dung yen ma van
        // vung tay danh nguoi choi - "dong bang" ma van sat thuong thi khong ra
        // dong bang. Xu ly giong het bi set choang.
        if (frozen != null && frozen.IsFullyFrozen)
        {
            attackTimer = -1f;
            hitApplied = false;
            velocity.x = 0f;
            velocity.z = 0f;
            ApplyGravity(dt);
            if (anim != null) anim.SetMoveSpeed(0f);
            return;
        }

        // Bi set danh choang: dung ngay tai cho, don danh dang giua chung cung bo luon
        var stunned = GetComponent<StunnedEffect>();
        if (stunned != null && stunned.IsStunned)
        {
            speedMul = 0f;
            attackTimer = -1f;
            hitApplied = false;

            velocity.x = 0f;
            velocity.z = 0f;
            ApplyGravity(dt);
            if (anim != null) anim.SetMoveSpeed(0f);
            return;
        }

        // Dang trong don danh
        if (attackTimer >= 0f)
        {
            attackTimer += dt;
            if (!hitApplied && attackTimer >= attackAnimTime * attackHitAt)
            {
                hitApplied = true;
                TryHit();
            }
            if (attackTimer >= attackAnimTime) attackTimer = -1f;

            ApplyGravity(dt);
            if (anim != null) anim.SetMoveSpeed(0f);
            FaceTarget(dt);
            return;
        }

        Vector3 move = Vector3.zero;
        float dist = target != null ? Vector3.Distance(FlatPos(transform.position), FlatPos(target.position)) : 999f;
        bool targetAlive = target != null;
        if (targetAlive)
        {
            var td = target.GetComponent<Damageable>();
            if (td != null && td.IsDead) targetAlive = false;
        }

        if (targetAlive && dist <= aggroRange && speedMul > 0.02f)
        {
            // Ke danh tu xa bi ap sat thi LUI RA cho du tam nem, vua lui vua
            // quay mat ve phia nguoi choi. Khong co doan nay thi mu phu thuy
            // dung im cho nguoi choi den tan noi chem, phi ca loi danh tam xa.
            if (danhTuXa && dist < khoangLui)
            {
                Vector3 raXa = FlatPos(transform.position) - FlatPos(target.position);
                if (raXa.sqrMagnitude > 0.001f) raXa.Normalize();

                raXa += Separation() * 0.6f;
                raXa.y = 0f;
                if (raXa.sqrMagnitude > 0.001f) raXa.Normalize();

                velocity.x = raXa.x * moveSpeed * speedMul;
                velocity.z = raXa.z * moveSpeed * speedMul;
                ApplyGravity(dt);

                if (anim != null) anim.SetMoveSpeed(1f);
                FaceTarget(dt);

                // Vua lui vua nem duoc, khong phai dung han lai moi danh
                if (cooldown <= 0f) StartAttack();
                return;
            }

            if (dist > TamDanhHieuDung())
            {
                Vector3 dir = FlatPos(target.position) - FlatPos(transform.position);
                dir.Normalize();

                // Tach nhau ra mot chut de khong chong len nhau
                dir += Separation() * 0.6f;
                dir.y = 0f;
                if (dir.sqrMagnitude > 0.001f) dir.Normalize();

                move = dir * moveSpeed * speedMul;
            }
            else if (cooldown <= 0f)
            {
                StartAttack();
            }
        }
        else if (speedMul > 0.02f)
        {
            // Di lang thang cho do tinh
            wanderTimer -= dt;
            if (wanderTimer <= 0f)
            {
                wanderTimer = Random.Range(2.5f, 5.5f);
                wanderDir = Random.insideUnitSphere;
                wanderDir.y = 0f;
                wanderDir.Normalize();
                if (Random.value < 0.45f) wanderDir = Vector3.zero;
            }
            move = wanderDir * moveSpeed * 0.28f * speedMul;
        }

        velocity.x = move.x;
        velocity.z = move.z;
        ApplyGravity(dt);

        if (anim != null)
            anim.SetMoveSpeed(new Vector2(move.x, move.z).magnitude / Mathf.Max(0.01f, moveSpeed));

        if (move.sqrMagnitude > 0.01f)
        {
            Quaternion want = Quaternion.LookRotation(new Vector3(move.x, 0f, move.z));
            transform.rotation = Quaternion.Slerp(transform.rotation, want, turnSpeed * dt);
        }
        else
        {
            FaceTarget(dt);
        }
    }

    void ApplyGravity(float dt)
    {
        if (cc == null || !cc.enabled) return;
        if (cc.isGrounded && velocity.y < 0f) velocity.y = -2f;
        velocity.y += -22f * dt;
        cc.Move(velocity * dt);
    }

    void FaceTarget(float dt)
    {
        if (target == null) return;
        Vector3 dir = FlatPos(target.position) - FlatPos(transform.position);
        if (dir.sqrMagnitude < 0.01f) return;
        Quaternion want = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(transform.rotation, want, turnSpeed * 0.6f * dt);
    }

    void StartAttack()
    {
        attackTimer = 0f;
        hitApplied = false;
        cooldown = attackCooldown;
        if (anim != null) anim.PlayAttack(attackAnimTime);
        if (hoatHinhRieng != null) hoatHinhRieng.RaDon(attackAnimTime);
    }

    void TryHit()
    {
        if (target == null) return;

        if (danhTuXa) { NemPhep(); return; }

        // Tru ban kinh khieng RA TRUOC roi moi so voi tam tay: dieu kien dung la
        // "mat vom co nam trong tam tay khong", khong phai "tam nguoi choi co
        // nam trong tam tay cong ban kinh khong".
        //
        // Nhan 1,35 vao ca tong (attackRange + banKinh) thi ban kinh cung bi
        // nhan theo, cho quai thua ra 0,35 x 3,04 = 1,06 met tam voi.
        float dist = Vector3.Distance(FlatPos(transform.position), FlatPos(target.position));
        if (dist - BanKinhKhiengMucTieu() > attackRange * 1.35f) return;

        var d = target.GetComponent<Damageable>();
        if (d == null || d.IsDead) return;

        // Damageable tu chuyen don nay vao khieng neu muc tieu dang co khieng
        d.TakeDamage(attackDamage, DamageType.Physical,
                     target.position + Vector3.up * 1.1f);
    }

    /// <summary>
    /// Tam danh THUC TE, co tinh den cai khieng cua muc tieu.
    ///
    /// Muc tieu bat khieng thi vach chan day quai ra tan mep vom - xa hon tam
    /// tay cua no. Giu nguyen attackRange thi quai dung do vung vay ma khong
    /// bao gio danh trung, va khieng khong bao gio mat mau: bat tu.
    ///
    /// Noi tam ra bang dung ban kinh khieng thi quai danh duoc VAO VOM, va
    /// Damageable se chuyen don do vao mau khieng.
    /// </summary>
    float TamDanhHieuDung()
    {
        return attackRange + BanKinhKhiengMucTieu();
    }

    /// <summary>Ban kinh khieng cua muc tieu, 0 neu muc tieu khong co khieng.</summary>
    float BanKinhKhiengMucTieu()
    {
        if (target == null) return 0f;

        var k = target.GetComponent<Khieng>();
        return (k != null && k.DangBat) ? k.banKinh : 0f;
    }

    /// <summary>
    /// Nem mot qua cau lua ve phia nguoi choi.
    ///
    /// Dung lai dung khoi <see cref="Fireball"/> cua nguoi choi, chi doi hai cai
    /// mat na: cham vao thi tinh voi lop Player, va gay sat thuong cung chi cho
    /// lop Player. Nho vay qua cau khong bao gio no vao dong bon cua chinh no.
    ///
    /// Ngam len NGUC nguoi choi chu khong ngam vao chan: ngam chan thi qua cau
    /// bay la la mat dat, gap mo dat cao la no som giua duong.
    /// </summary>
    void NemPhep()
    {
        var d = target.GetComponent<Damageable>();
        if (d == null || d.IsDead) return;

        if (danhBangThienThach) { GoiThienThach(); return; }
        if (danhBangTiaSet) { PhongTiaSet(); return; }

        Vector3 tu = diemPhongPhep != null
            ? diemPhongPhep.position
            : transform.position + Vector3.up * 1.35f + transform.forward * 0.45f;

        Vector3 den = target.position + Vector3.up * 1.05f;
        Vector3 huong = den - tu;
        if (huong.sqrMagnitude < 0.01f) return;

        // "Khieng" phai co trong mask VA CHAM: nguoi choi bat khieng thi cau lua
        // no ngay tren mat cau chu khong bay xuyen qua. Dan cua NGUOI CHOI thi
        // KHONG co lop nay trong mask, nen van ban tu trong khieng ra ngoai duoc.
        var fb = Fireball.Spawn(tu, huong.normalized,
                                LayerMask.GetMask("Player", "Ground", "Default", "Khieng"),
                                LayerMask.GetMask("Player"));
        if (fb == null) return;

        // Yeu hon qua cau cua nguoi choi nhieu: nguoi choi chiu don tu ca dam
        // quai cung luc, de nguyen 38 sat thuong nhu ban goc thi ba mu phu thuy
        // ban trung mot luot la het mau.
        fb.impactDamage = satThuongCau;
        fb.blastRadius = banKinhNo;
        fb.speed = tocDoCau;
        fb.burnSeconds = 2.2f;
    }

    /// <summary>
    /// GOI MOT KHOI THIEN THACH xuong dau nguoi choi.
    ///
    /// Dung lai dung khoi <see cref="ThienThach"/> cua nguoi choi nen hieu ung
    /// y het, chi doi hai cai mat na de qua da khong no vao dong bon cua chinh
    /// no. MOI LAN DANH CHI MOT QUA - <c>Spawn</c> chu khong phai
    /// <c>SpawnLoat</c>: mot loat ba qua tu muoi con Quy du la sau muoi khoi da
    /// cung roi, khong con nhin ra san choi.
    ///
    /// KHAC qua cau lua o cho: thien thach roi tu TREN TROI xuong cho nguoi choi
    /// DANG DUNG, khong bay theo duong thang tu tay quai. Nguoi choi buoc ra la
    /// tranh duoc - do la cho hay cua don danh nay, va cung la ly do de tam ban
    /// 15 m ma van choi duoc.
    ///
    /// VUNG LUA DE LAI PHAI NHO. Nguyen ban cua nguoi choi de lai mot vung lua
    /// ban kinh 4,5 m chay 5 giay; muoi con Quy du danh lien tuc thi ca dau
    /// truong thanh bien lua va nguoi choi khong con cho nao dat chan.
    /// </summary>
    void GoiThienThach()
    {
        Vector3 diemNgam = target.position;

        var tt = ThienThach.Spawn(diemNgam,
                                  LayerMask.GetMask("Player", "Ground", "Default", "Khieng"),
                                  LayerMask.GetMask("Player"));
        if (tt == null) return;

        tt.impactDamage = satThuongCau;
        tt.blastRadius = banKinhNo;

        // CHI GAY SAT THUONG - khong de lai vung lua nhu ban cua nguoi choi.
        // Phai gan RO RANG ve 0: ThienThach mac dinh chay 5 giay, khong gan gi
        // thi moi qua da cua quai deu dot san them nam giay nua.
        tt.chayBanKinh = 0f;
        tt.chayThoiGian = 0f;
        tt.chaySatThuongMoiGiay = 0f;
    }

    /// <summary>
    /// PHONG MOT TIA SET thang sang nguoi choi.
    ///
    /// Dung lai dung khoi <see cref="GiatSet"/> cua nguoi choi nen hieu ung y
    /// het, chi doi mau va tat phan LAN: quai chi co mot muc tieu la nguoi
    /// choi, khong co gi de mach dien nhay tiep sang.
    ///
    /// KHAC thien thach o cho don nay TRUNG NGAY, khong co 0,8 giay roi de ne.
    /// Bu lai tam ban ngan hon nhieu.
    /// </summary>
    void PhongTiaSet()
    {
        Vector3 tu = diemPhongPhep != null
            ? diemPhongPhep.position
            : transform.position + Vector3.up * 1.6f + transform.forward * 0.5f;

        Vector3 den = target.position + Vector3.up * 1.05f;
        Vector3 h = den - tu;
        if (h.sqrMagnitude < 0.01f) return;

        // Tam truyen vao phai TINH TU CHO PHONG chu khong phai tu chan quai:
        // diem phong o tren cao va nhoi ra truoc, nen no da an mat mot doan.
        // EnemyAI KHONG co truong damageMask - hai ham danh xa kia cung lay
        // mask ngay tai cho nhu the nay. Chi lop Player: tia khong bao gio
        // giat vao dong bon cua chinh no.
        GiatSet.PhongCuaQuai(tu, h.normalized, LayerMask.GetMask("Player"),
                             satThuongCau, attackRange + 2f,
                             tiaMauLoi, tiaMauQuang);
    }

    /// <summary>Day nhe ra khoi cac con quai khac dung qua sat.</summary>
    Vector3 Separation()
    {
        Vector3 push = Vector3.zero;
        var others = Physics.OverlapSphere(transform.position, 1.2f, LayerMask.GetMask("Enemy"),
                                           QueryTriggerInteraction.Ignore);
        for (int i = 0; i < others.Length; i++)
        {
            if (others[i].transform == transform) continue;
            Vector3 away = transform.position - others[i].transform.position;
            away.y = 0f;
            float d = away.magnitude;
            if (d > 0.001f) push += away / (d * d);
        }
        return Vector3.ClampMagnitude(push, 1f);
    }

    static Vector3 FlatPos(Vector3 v) { return new Vector3(v.x, 0f, v.z); }
}
