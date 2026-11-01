# Anh cho hieu ung CHEM cua quai Bo xuong (01/11/2026, nguoi dung: "dung lai toan hieu ung chem... cho thay ro hieu ung chem,
# moi lan chem vao doi thu co hieu ung danh trung va doi thu bi chay mau"). Chay TRONG Blender qua MCP:
#   exec(open(r"...\CongCu\Blender\chem_bo_xuong.py", encoding="utf-8").read()); nuong_tat_ca(r"...\Assets\Resources\KyNang\BoXuong")
# Sinh bang numpy roi luu bang bpy (anh PNG RGBA, TUYEN TINH - Unity tat sRGB):
#   VetChem.png  512x256 : vet luoi kiem quet. Truc x = doc cung chem (u), truc y = tu chuoi kiem (v=0) toi mui kiem (v=1).
#                          R = than vet (soi keo doc cung, day dan ve mep mui), G = mep nong sang sat mui kiem.
#   VetCat.png   256x128 : nhat cat loe tai cho trung (luoi liem mong): mau that - loi trang nong, quang do mau (cong sang).
#   GiotMau.png  256x256 : 2x2 giot mau bay (khoi mau meo + giot vu tinh + vet bong uot). RGB mau, A do phu.
#   BanMau.png   512x512 : 2x2 vet mau ban tren dat (vung giua + gai toe + giot xa). RGB mau, A do phu.
import numpy as np
import bpy, os

RNG = np.random.default_rng(1101)


def _nhieu_lap(w, h, tan_x, tan_y, rng):
    # Nhieu gia tri lap lien mach: luoi ngau nhien tan_x x tan_y noi suy cosin
    g = rng.random((tan_y, tan_x))
    xs = np.linspace(0, tan_x, w, endpoint=False)
    ys = np.linspace(0, tan_y, h, endpoint=False)
    x0 = np.floor(xs).astype(int); y0 = np.floor(ys).astype(int)
    fx = xs - x0; fy = ys - y0
    fx = (1 - np.cos(fx * np.pi)) * 0.5; fy = (1 - np.cos(fy * np.pi)) * 0.5
    x1 = (x0 + 1) % tan_x; y1 = (y0 + 1) % tan_y
    a = g[np.ix_(y0, x0)]; b = g[np.ix_(y0, x1)]; c = g[np.ix_(y1, x0)]; d = g[np.ix_(y1, x1)]
    top = a + (b - a) * fx[None, :]
    bot = c + (d - c) * fx[None, :]
    return top + (bot - top) * fy[:, None]


def _fbm(w, h, tx, ty, lop, rng):
    tong = np.zeros((h, w)); bien = 1.0; chuan = 0.0
    for i in range(lop):
        tong += _nhieu_lap(w, h, tx * 2 ** i, ty * 2 ** i, rng) * bien
        chuan += bien; bien *= 0.5
    return tong / chuan


def _luu(path, rgba):
    h, w, _ = rgba.shape
    ten = os.path.splitext(os.path.basename(path))[0]
    if ten in bpy.data.images:
        bpy.data.images.remove(bpy.data.images[ten])
    img = bpy.data.images.new(ten, w, h, alpha=True, float_buffer=False)
    img.colorspace_settings.name = 'Non-Color'
    img.pixels.foreach_set(np.clip(rgba, 0, 1).astype(np.float32).ravel())
    img.filepath_raw = path
    img.file_format = 'PNG'
    img.save()
    return img


def _smooth(a, b, x):
    t = np.clip((x - a) / (b - a), 0, 1)
    return t * t * (3 - 2 * t)


def vet_chem(w=512, h=256):
    # y=0 chuoi kiem, y=1 mui kiem. Soi keo dai THEO x (doc cung chem): nhieu tan so thap theo x, cao theo y.
    v = np.linspace(0, 1, h)[:, None] * np.ones((1, w))
    u = np.ones((h, 1)) * np.linspace(0, 1, w)[None, :]
    soi = _fbm(w, h, 2, 24, 3, RNG)               # soi doc cung
    soi2 = _fbm(w, h, 3, 48, 2, RNG)
    # Mep trong rach: vet chi bat dau o v ~ 0,25 + rach theo x
    rach = _fbm(w, h, 6, 1, 3, RNG)[0:1, :]
    mep_trong = 0.18 + 0.22 * rach
    than = _smooth(mep_trong, mep_trong + 0.30, v) * _smooth(1.0, 0.94, v)
    # Day dan ve mep mui + soi
    than = than * (0.35 + 0.65 * v ** 1.6) * (0.55 + 0.75 * soi) * (0.75 + 0.5 * soi2)
    # Vai soi sang manh nam ngang (vet xuoc luoi)
    for _ in range(7):
        y = RNG.uniform(0.45, 0.9); d = RNG.uniform(0.004, 0.012)
        than += 0.35 * np.exp(-((v - y) / d) ** 2) * (0.4 + 0.6 * _fbm(w, 1, 4, 1, 2, RNG))
    than = np.clip(than, 0, 1)
    # Mep nong sat mui kiem: duong manh o v ~ 0,90-0,93, dao dong nhe theo x
    lech = 0.015 * (_fbm(w, 1, 5, 1, 2, RNG) - 0.5)
    mep = np.exp(-((v - (0.915 + lech)) / 0.022) ** 2)
    mep = np.clip(mep * (0.8 + 0.4 * soi2), 0, 1)
    rgba = np.zeros((h, w, 4)); rgba[..., 0] = than; rgba[..., 1] = mep; rgba[..., 3] = 1
    return rgba


def vet_cat(w=256, h=128):
    # Luoi liem mong nam ngang: day nhat o giua, nhon hai dau, hoi cong
    x = np.linspace(-1, 1, w)[None, :] * np.ones((h, 1))
    y = np.linspace(-1, 1, h)[:, None] * np.ones((1, w))
    cong = 0.28 * (1 - x ** 2)                     # cong len o giua
    day = 0.10 * (1 - np.abs(x) ** 1.4)           # nhon hai dau
    d = np.abs(y - cong + 0.14)
    loi = np.exp(-(d / np.maximum(day * 0.35, 1e-3)) ** 2) * (np.abs(x) < 1)
    quang = np.exp(-(d / np.maximum(day * 1.8 + 0.04, 1e-3)) ** 2) * (1 - np.abs(x) ** 2) ** 0.7
    # Anh MAU THAT cho shader hat cong sang: loi trang nong, quang do mau
    loi = np.clip(loi, 0, 1); quang = np.clip(quang, 0, 1)
    rgba = np.zeros((h, w, 4))
    rgba[..., 0] = np.clip(1.00 * loi + 0.85 * quang, 0, 1)
    rgba[..., 1] = np.clip(0.80 * loi + 0.10 * quang, 0, 1)
    rgba[..., 2] = np.clip(0.70 * loi + 0.05 * quang, 0, 1)
    rgba[..., 3] = np.maximum(loi, quang)
    return rgba


def _khoi_mau(n, rng, kieu):
    # Mot o n x n: hinh mau. kieu 'giot' (giot bay, tron meo) hoac 'ban' (vet ban tren dat, gai toe)
    c = (np.arange(n) + 0.5) / n * 2 - 1
    X, Y = np.meshgrid(c, c)
    R = np.sqrt(X ** 2 + Y ** 2); A = np.arctan2(Y, X)
    nh = _fbm(n, n, 4, 4, 3, rng)
    if kieu == 'giot':
        # Khoi chinh keo dai mot huong + duoi
        goc = rng.uniform(0, np.pi * 2)
        Xr = X * np.cos(goc) + Y * np.sin(goc); Yr = -X * np.sin(goc) + Y * np.cos(goc)
        r0 = np.sqrt((Xr / 0.62) ** 2 + (Yr / 0.44) ** 2) + 0.25 * (nh - 0.5)
        phu = _smooth(1.0, 0.92, r0)
        for _ in range(rng.integers(2, 5)):   # giot vu tinh
            cx, cy = rng.uniform(-0.75, 0.75, 2); rr = rng.uniform(0.06, 0.14)
            phu = np.maximum(phu, _smooth(rr, rr * 0.7, np.sqrt((X - cx) ** 2 + (Y - cy) ** 2)))
    else:
        # Vung giua MEO (be cong toa do bang nhieu) + mep SAC - tron deu thi trong nhu mat troi hoat hinh
        n1 = _fbm(n, n, 3, 3, 3, rng); n2 = _fbm(n, n, 3, 3, 3, rng)
        Xw = X + 0.30 * (n1 - 0.5); Yw = Y + 0.30 * (n2 - 0.5)
        Rw = np.sqrt(Xw ** 2 + Yw ** 2)
        bk = 0.27 + 0.10 * (nh - 0.5)
        gai = np.zeros_like(R)
        for _ in range(rng.integers(14, 22)):
            ga = rng.uniform(-np.pi, np.pi); dai = rng.uniform(0.40, 0.90); rong = rng.uniform(0.025, 0.08)
            da = np.angle(np.exp(1j * (A - ga)))
            canh = np.abs(da) * R
            thuon = rong * np.clip(1 - R / dai, 0, 1) ** 1.2
            gai = np.maximum(gai, _smooth(thuon + 0.006, thuon * 0.5, canh) * (R < dai))
            # giot dai o cuoi gai (mau bay theo huong toe)
            if rng.random() < 0.6:
                rr = dai + rng.uniform(0.02, 0.06); s = rng.uniform(0.018, 0.04)
                cx, cy = rr * np.cos(ga), rr * np.sin(ga)
                dx, dy = X - cx, Y - cy
                doc = dx * np.cos(ga) + dy * np.sin(ga); ngang = -dx * np.sin(ga) + dy * np.cos(ga)
                gai = np.maximum(gai, _smooth(1.0, 0.7, np.sqrt((doc / (s * 2.2)) ** 2 + (ngang / s) ** 2)))
        phu = np.maximum(_smooth(bk + 0.012, bk - 0.008, Rw), gai)
        for _ in range(rng.integers(10, 18)):  # giot xa
            ga = rng.uniform(-np.pi, np.pi); rr = rng.uniform(0.45, 0.92); s = rng.uniform(0.012, 0.035)
            cx, cy = rr * np.cos(ga), rr * np.sin(ga)
            phu = np.maximum(phu, _smooth(s, s * 0.75, np.sqrt((X - cx) ** 2 + (Y - cy) ** 2)))
        phu = phu * (R < 0.97)
    # Mau: do sam o giua, den hon o mep (mau dong vien), vet bong uot nho
    mep = _smooth(0.0, 0.6, phu)
    sang = 0.55 + 0.45 * nh
    r = (0.30 + 0.22 * (1 - mep) * 0 + 0.20 * mep) * sang
    g = 0.012 * sang
    b = 0.010 * sang
    if kieu == 'giot':
        bong = np.exp(-(((X + 0.18) / 0.16) ** 2 + ((Y - 0.16) / 0.10) ** 2)) * phu
        r = r + 0.45 * bong; g = g + 0.25 * bong; b = b + 0.22 * bong
    rgba = np.zeros((n, n, 4))
    rgba[..., 0] = r; rgba[..., 1] = g; rgba[..., 2] = b; rgba[..., 3] = np.clip(phu * (0.85 + 0.15 * nh), 0, 1)
    return rgba


def bang_2x2(o, kieu, rng):
    n = o
    out = np.zeros((2 * n, 2 * n, 4))
    for i in range(4):
        cy, cx = divmod(i, 2)
        out[cy * n:(cy + 1) * n, cx * n:(cx + 1) * n] = _khoi_mau(n, rng, kieu)
    return out


def nuong_tat_ca(thu_muc):
    os.makedirs(thu_muc, exist_ok=True)
    _luu(os.path.join(thu_muc, 'VetChem.png'), vet_chem())
    _luu(os.path.join(thu_muc, 'VetCat.png'), vet_cat())
    _luu(os.path.join(thu_muc, 'GiotMau.png'), bang_2x2(128, 'giot', np.random.default_rng(7)))
    _luu(os.path.join(thu_muc, 'BanMau.png'), bang_2x2(256, 'ban', np.random.default_rng(11)))
    return 'ok'
