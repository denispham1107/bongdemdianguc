using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// MAN HINH CHINH: ten game, nut vao choi va bang huong dan phim.
/// Ve bang OnGUI nen khong can cai them package UI nao.
/// </summary>
public class MainMenuUI : MonoBehaviour
{
    [Header("Ten man choi")]
    public string act1Scene = "Act1";
    public string act2Scene = "Act2";

    [Header("Nhan vat dung lam nen")]
    public Transform showcase;          // phu thuy dung xoay tron o man hinh chinh
    public float spinSpeed = 18f;

    [Header("Choi nhieu nguoi")]
    [Tooltip("Bat len thi phai dang nhap moi vao duoc menu; tat thi choi don nhu cu")]
    public bool batChoiMang = true;

    ManDangNhap manDangNhap;
    ManSanh manSanh;

    GUIStyle title, button, small;
    Texture2D panel, line;

    void Start()
    {
        panel = Solid(new Color(0f, 0f, 0f, 0.55f));
        line = Solid(Color.white);
        Cursor.visible = true;

        // Ve toi day nghia la da ra khoi tran - xoa dau vet van truoc, khong
        // thi choi don lan sau van tuong minh dang trong mot phong nao do.
        TranHienTai.Xoa();

        if (!batChoiMang) return;

        manDangNhap = gameObject.AddComponent<ManDangNhap>();
        manSanh = gameObject.AddComponent<ManSanh>();
        manSanh.enabled = false;
        manDangNhap.daVao = () => { manSanh.enabled = true; };

        // DA DANG NHAP SAN THI VAO THANG SANH.
        //
        // Truoc day sanh chi bat khi viec dang nhap XAY RA trong luc man nay dang
        // mo. Quay ve day sau tran (bam TRO VE / ESC) thi da dang nhap tu truoc,
        // khong ai goi daVao - va nguoi choi roi vao menu choi don cu voi hai
        // nut "MAN 1 / MAN 2" thay vi sanh cho.
        if (FirebaseMang.DaDangNhap) manDangNhap.daVao();
    }

    void Update()
    {
        if (showcase != null)
            showcase.Rotate(Vector3.up, spinSpeed * Time.deltaTime, Space.World);

        // Choi mang thi man nay CHI CO dang nhap va sanh cho - menu choi don cu
        // (hai nut MAN 1 / MAN 2, phim Enter vao thang Act1) khong con nua.
        if (batChoiMang) return;

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            Play(act1Scene);
        if (Input.GetKeyDown(KeyCode.Escape))
            Quit();
    }

    void EnsureStyles(float s)
    {
        if (title == null)
        {
            title = new GUIStyle(GUI.skin.label);
            button = new GUIStyle(GUI.skin.button);
            small = new GUIStyle(GUI.skin.label);
        }

        title.fontSize = Mathf.RoundToInt(72f * s);
        title.fontStyle = FontStyle.Bold;
        title.alignment = TextAnchor.MiddleCenter;
        title.normal.textColor = new Color(0.85f, 0.15f, 0.08f);

        button.fontSize = Mathf.RoundToInt(26f * s);
        button.fontStyle = FontStyle.Bold;

        small.fontSize = Mathf.RoundToInt(18f * s);
        small.alignment = TextAnchor.MiddleCenter;
        small.normal.textColor = new Color(0.85f, 0.82f, 0.75f);
    }

    void OnGUI()
    {
        // ManDangNhap va ManSanh tu ve lay phan cua chung. Menu cu chi con
        // khi tat han choi mang (batChoiMang = false).
        if (batChoiMang) return;

        float s = Screen.height / 1080f;
        EnsureStyles(s);

        // Ten game
        GUI.Label(new Rect(0f, Screen.height * 0.12f, Screen.width, 100f * s), "DIABLO 2.5D", title);
        GUI.Label(new Rect(0f, Screen.height * 0.12f + 95f * s, Screen.width, 40f * s),
                  "Nu phu thuy - Lua, Bang, Sam set & Loc xoay", small);

        // Nut bam
        float w = 320f * s, h = 62f * s;
        float x = (Screen.width - w) * 0.5f;
        float y = Screen.height * 0.42f;

        // Act1: dau truong tron sinh bang code.
        // Act2: nghia dia ve tay trong Blender, moi dot GAP DOI so quai.
        if (GUI.Button(new Rect(x, y, w, h), "MAN 1 - DAU TRUONG", button))
            Play(act1Scene);
        if (GUI.Button(new Rect(x, y + h + 14f * s, w, h), "MAN 2 - NGHIA DIA", button))
            Play(act2Scene);
        if (GUI.Button(new Rect(x, y + (h + 14f * s) * 2f, w, h), "THOAT", button))
            Quit();

        // Bang huong dan
        string[] lines =
        {
            "CHUOT TRAI: di chuyen       GIU CHUOT PHAI: tu xoay camera",
            "PHIM 1: Qua cau lua   2: Mua bang   3: Sam set   4: Loc xoay",
            "PHIM C: doi goc nhin 3D / 2.5D / 2D      Q,E: xoay camera",
            "WASD: di chuyen truc tiep      R: choi lai      ESC: ve man hinh chinh",
        };

        float bw = 900f * s, lh = 30f * s;
        var r = new Rect((Screen.width - bw) * 0.5f, Screen.height * 0.74f, bw, lh * lines.Length + 16f * s);
        GUI.DrawTexture(r, panel, ScaleMode.StretchToFill, true);
        for (int i = 0; i < lines.Length; i++)
            GUI.Label(new Rect(r.x, r.y + 8f * s + i * lh, r.width, lh), lines[i], small);
    }

    void Play(string sceneName)
    {
        if (Application.CanStreamedLevelBeLoaded(sceneName))
            SceneManager.LoadScene(sceneName);
        else
            Debug.LogWarning("[MainMenu] Chua co scene ten: " + sceneName);
    }

    void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    static Texture2D Solid(Color c)
    {
        var t = new Texture2D(4, 4, TextureFormat.RGBA32, false);
        var px = new Color[16];
        for (int i = 0; i < px.Length; i++) px[i] = c;
        t.SetPixels(px); t.Apply();
        return t;
    }
}
