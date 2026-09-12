using UnityEngine;

/// <summary>
/// DOC PHIM, CHUOT VA CAN CAM UNG - CHI DOC, KHONG THI HANH GI.
///
/// Toan bo phan "nguoi choi dang muon gi" duoc gom vao day, doi thanh
/// <see cref="GoiInput"/> roi giao cho <see cref="PlayerController"/> thi hanh.
/// Truoc day hai viec nay tron trong cung mot ham, khien khong the chay lai
/// mot chuoi thao tac o may khac - dieu bat buoc phai lam khi choi mang.
///
/// File nay la NOI DUY NHAT trong duong dieu khien nhan vat duoc phep goi
/// <c>Input.*</c> hay doc <c>CamUng</c>. Cho nao khac can biet nguoi choi
/// muon gi thi doc tu goi.
///
/// Doi truc o day chu khong o PlayerController: "day can sang phai" phu thuoc
/// camera cua RIENG may nay, ma may trong tai thi khong co camera do. Doi
/// xong roi gui di thi ai cung hieu giong nhau.
/// </summary>
public class DocInput : MonoBehaviour
{
    PlayerController nguoiChoi;
    CameraRig cameraRig;
    int demSoThuTu;

    void Awake()
    {
        nguoiChoi = GetComponent<PlayerController>();
    }

    /// <summary>
    /// Nguoi choi vua bam phim ky nang nao? Tra ve 0..6, khong bam thi -1.
    ///
    /// MOI KY NANG CO BA CACH BAM, de neu mot cach bi ket thi con duong khac:
    ///   - Hang so tren cung:  1 2 3 4
    ///   - Ban phim so ben phai
    ///   - Hang phim chu:      Z X V B
    /// Hang Z X V B la duong thoat khi hang so bi thu khac an mat - bo go tieng
    /// Viet kieu VNI lay so 1-9 lam dau thanh, bam so ra dau chu khong toi game.
    /// (Khong dung C vi C la phim doi goc camera, khong dung Q/E vi do la phim
    /// xoay camera.)
    /// </summary>
    public static int BamPhimKyNang()
    {
        // Tra ve SO HIEU KY NANG, nhung phim so la so cua O chu khong phai cua
        // ky nang: nguoi choi keo Sam set sang o 1 trong Sach phep thi bam phim
        // 1 phai ra Sam set. Cach doi nam o BamPhimO ngay duoi.
        int o = BamPhimO();
        return o < 0 ? -1 : SachPhep.KyNangTaiO(o);
    }

    /// <summary>O ky nang vua duoc bam, 0..6; khong bam thi -1.</summary>
    public static int BamPhimO()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1)
            || Input.GetKeyDown(KeyCode.Z)) return 0;
        if (Input.GetKeyDown(KeyCode.Alpha5) || Input.GetKeyDown(KeyCode.Keypad5)
            || Input.GetKeyDown(KeyCode.N)) return 4;
        if (Input.GetKeyDown(KeyCode.Alpha6) || Input.GetKeyDown(KeyCode.Keypad6)
            || Input.GetKeyDown(KeyCode.M)) return 5;
        if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2)
            || Input.GetKeyDown(KeyCode.X)) return 1;
        if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3)
            || Input.GetKeyDown(KeyCode.V)) return 2;
        if (Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.Keypad4)
            || Input.GetKeyDown(KeyCode.B)) return 3;
        if (Input.GetKeyDown(KeyCode.Alpha7) || Input.GetKeyDown(KeyCode.Keypad7)
            || Input.GetKeyDown(KeyCode.G)) return 6;
        return -1;
    }

    /// <summary>Doc mot khung hinh y muon cua nguoi choi.</summary>
    public GoiInput Doc(float dt)
    {
        GoiInput g = GoiInput.Rong(dt);
        g.soThuTu = ++demSoThuTu;

        // DANG MO SACH PHEP: tra ve goi RONG (nhung van tang so thu tu, va van
        // giu dt - xem PlayerController.Update).
        //
        // Khong chan o day thi moi cu cham de keo tha trong bang deu bi hieu
        // thanh "bam chuot trai xuong san" - nhan vat chay di trong khi nguoi
        // choi dang sap xep ky nang, va bam phim 1..7 van ban ra phep.
        if (CuaSoSachPhep.DangMo) return g;

        g.laCamUng = CamUng.DangDung;
        g.coBamPhim = Input.anyKeyDown;
        g.kyNang = BamPhimKyNang();

        if (cameraRig == null && nguoiChoi != null) cameraRig = nguoiChoi.cameraRig;
        Vector3 fwd = cameraRig != null ? cameraRig.FlatForward : Vector3.forward;
        Vector3 right = new Vector3(fwd.z, 0f, -fwd.x);

        // --- Huong di: ban phim + can joystick ---
        float ix = Input.GetAxisRaw("Horizontal");
        float iz = Input.GetAxisRaw("Vertical");

        // Can cong THANG vao hai truc ban phim, nen khong phai viet lai doan
        // doi truc o duoi.
        if (CamUng.DangDung && CamUng.DangKeo)
        {
            ix += CamUng.Huong.x;
            iz += CamUng.Huong.y;
        }

        if (Mathf.Abs(ix) > 0.01f || Mathf.Abs(iz) > 0.01f)
        {
            Vector3 huong = fwd * iz + right * ix;

            // KHONG chuan hoa thang: can day nua chung thi di cham, day het thi
            // di nhanh. Chuan hoa la moi cai chum tay deu thanh chay het toc.
            g.huongDi = huong.sqrMagnitude > 1f ? huong.normalized : huong;
        }

        // --- Huong dang day can, de tu ngam tren may cam ung ---
        if (CamUng.DangKeo && CamUng.Huong.sqrMagnitude > 0.04f)
        {
            Vector3 h = fwd * CamUng.Huong.y + right * CamUng.Huong.x;
            h.y = 0f;
            if (h.sqrMagnitude > 0.0001f) g.huongCan = h.normalized;
        }

        // --- Chuot trai: di den diem bam ---
        //
        // TAT HAN tren may cam ung. Tren WebGL dien thoai, moi cu cham man hinh
        // deu duoc trinh duyet bao lai thanh mot cu bam CHUOT TRAI - nen khong
        // chan o day thi cham vao dau nhan vat cung chay den do, ke ca khi dang
        // bam nut ky nang hay xoay may quay.
        if (!CamUng.DangDung && Input.GetMouseButton(0) && !ConTroTrenThanhKyNang())
        {
            Vector3 diem;
            if (nguoiChoi != null && nguoiChoi.GetAimPoint(out diem))
            {
                g.muonDiToi = true;
                g.diemToi = diem;
            }
        }

        // --- Diem ngam de tung ky nang ---
        if (nguoiChoi != null)
        {
            Vector3 ngam;
            g.coNgam = nguoiChoi.GetAimPoint(out ngam);
            g.diemNgam = ngam;
        }

        return g;
    }

    static bool ConTroTrenThanhKyNang()
    {
        // Thanh ky nang nam duoi day man hinh - khong di chuyen khi bam vao do
        if (Input.mousePosition.y < Screen.height * 0.09f) return true;

        // Nut SACH PHEP o goc phai tren: bam vao no ma khong chan o day thi
        // nhan vat vua mo sach vua chay ve phia goc man hinh.
        float s = Screen.height / 1080f;
        var tam = new Vector2(Screen.width - 62f * s, Screen.height - 62f * s);
        return Vector2.Distance(Input.mousePosition, tam) <= 46f * s;
    }
}
