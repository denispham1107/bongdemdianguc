# ANH VO CAY MOI CHO MOI CAY ACT2 + MAN CHINH (17/10/2026, nguoi dung: "vo cay san sui nhieu hon nua, than + canh con tron bong gia tao,
# mot vai cho vo than cay bi nut troc ra cho giong that"). Chay trong Blender MCP: exec(open(r"...\anh_vo_cay_moi.py").read()).
#
# Nguon: anh chup that Poly Haven (CC0) tai ve CongCu/AnhVoCay/Goc/ (2k, md5 khop, khong vao git):
#   bark_willow_02      -> VO CHINH (vo kho nut sau, ranh doc) : VoCay3_Mau (RGB mau x AO, A = do bong) + VoCay3_Phap (normal GL)
#   bark_platanus       -> LOP TRONG lo ra o cho vo troc        : VoCay3_Trong (RGB, sang + ngả vang nau nhu vo trong moi bong)
#   (jolcham_oak_bark_01 tai ve de du phong - chua dung: reu xanh nhieu, nghia dia toi thi lac tong)
# Mat na troc tu sinh (tuan hoan bang FFT -> lap lien mach):
#   VoCay3_Troc: A = cho troc (mang doc, mep rach) + vet nut dai; B = do toi (bong duoi mep vo con dinh, long vet nut);
#                RG = phap tuyen (0,5 + 0,5 n) tu do cao "do day vo" (vo 1, long troc 0, mep vo quan len 0,25) -> mep troc co bac that.
# Mau: chuyen tuyen tinh, nhan tung kenh cho TRUNG BINH = dich, tuong phan do sang x HE_SO quanh trung binh (giong anh_dat_moi.py).
import bpy, numpy as np, os

GOC = r"C:\Users\HP\Documents\GameUnity\Diablo25D\CongCu\AnhVoCay\Goc"
RA = r"C:\Users\HP\Documents\GameUnity\Diablo25D\Assets\BlenderMaps\GraveyardAct2\CayDaChua"
CO = 1024
MAU_VO = (0.33, 0.305, 0.27)      # TB sRGB vo chinh (anh cu x _Color: 0,366 0,350 0,303 - toi hon chut vi khe nut sau)
MAU_TRONG = (0.66, 0.50, 0.34)    # TB sRGB lop trong lo ra: nau vang nhat (0,60 0,46 0,33 chim vao vo duoi anh trang)
TUONG_PHAN_VO = 1.5
TUONG_PHAN_TRONG = 0.85
TI_LE_TROC = 0.18                 # 18% dien tich la mang troc ("mot vai cho"; 14% -> moi than chi 1 mang mo)
SO_VET_NUT = 9

def s2l(x): return np.where(x <= 0.04045, x / 12.92, ((x + 0.055) / 1.055) ** 2.4)
def l2s(x): x = np.clip(x, 0, 1); return np.where(x <= 0.0031308, x * 12.92, 1.055 * x ** (1 / 2.4) - 0.055)
W = np.array([0.2126, 0.7152, 0.0722])

def doc(ten):
    im = bpy.data.images.load(os.path.join(GOC, ten), check_existing=False)
    im.colorspace_settings.name = "Non-Color"
    im.scale(CO, CO)
    a = np.empty(CO * CO * 4, dtype=np.float32); im.pixels.foreach_get(a)
    bpy.data.images.remove(im)
    return a.reshape(CO, CO, 4)

def ghi(ten, a):
    im = bpy.data.images.new(ten, CO, CO, alpha=True)
    im.colorspace_settings.name = "Non-Color"
    im.pixels.foreach_set(a.astype(np.float32).ravel())
    im.filepath_raw = os.path.join(RA, ten + ".png"); im.file_format = "PNG"
    im.save(); bpy.data.images.remove(im)

def epMau(lin, dich, tp):
    lin = lin / lin.reshape(-1, 3).mean(0) * s2l(np.array(dich))
    sang = lin @ W; tb = sang.mean()
    k = (tb + (sang - tb) * tp) / np.maximum(sang, 1e-6)
    return l2s(lin * k[..., None])

bao = []
# ---------------- VO CHINH ----------------
mau = doc("bark_willow_02_diff_2k.jpg")[..., :3]
ao = doc("bark_willow_02_ao_2k.jpg")[..., 0:1]
nham = doc("bark_willow_02_rough_2k.jpg")[..., 0]
# KHE NUT TOI SAU: nuong tu ban do do cao (AO cua anh chi toi nhe) - lan 1 khong co buoc nay: o cu ly game ranh nut chim mat, than cay van phang
cao = doc("bark_willow_02_disp_2k.jpg")[..., 0]
cn = (cao - np.quantile(cao, 0.02)) / (np.quantile(cao, 0.98) - np.quantile(cao, 0.02))
khe = 0.22 + 0.78 * np.clip(cn / 0.7, 0, 1) ** 1.3
rgb = epMau(s2l(mau) * np.power(np.clip(ao, 0, 1), 1.0) * khe[..., None], MAU_VO, TUONG_PHAN_VO)
bong = 0.03 + (1.0 - nham) * 0.08
ghi("VoCay3_Mau", np.dstack([rgb, bong]))
pt = doc("bark_willow_02_nor_gl_2k.jpg"); pt[..., 3] = 1.0
ghi("VoCay3_Phap", pt)
L = rgb @ W
bao.append("VoCay3_Mau: TB %s sang %.3f lech %.3f bong %.3f-%.3f" % (np.round(rgb.reshape(-1, 3).mean(0), 3), L.mean(), L.std(), bong.min(), bong.max()))

# ---------------- LOP TRONG ----------------
mau = doc("bark_platanus_diff_2k.jpg")[..., :3]
ao = doc("bark_platanus_ao_2k.jpg")[..., 0:1]
rgb = epMau(s2l(mau) * np.power(np.clip(ao, 0, 1), 0.5), MAU_TRONG, TUONG_PHAN_TRONG)
ghi("VoCay3_Trong", np.dstack([rgb, np.ones((CO, CO))]))
L = rgb @ W
bao.append("VoCay3_Trong: TB %s sang %.3f lech %.3f" % (np.round(rgb.reshape(-1, 3).mean(0), 3), L.mean(), L.std()))

# ---------------- MAT NA TROC ----------------
rng = np.random.default_rng(1707)
fy = np.fft.fftfreq(CO)[:, None]; fx = np.fft.fftfreq(CO)[None, :]

def nhieu(sx, sy, seed):
    # nhieu tuan hoan: loc Gauss trong mien tan so (do dai tuong quan sx ngang, sy doc - tinh bang diem anh)
    r = np.random.default_rng(seed).standard_normal((CO, CO))
    g = np.exp(-2 * np.pi ** 2 * ((fx * sx) ** 2 + (fy * sy) ** 2))
    n = np.real(np.fft.ifft2(np.fft.fft2(r) * g))
    return (n - n.mean()) / n.std()

def mo(a, s):
    g = np.exp(-2 * np.pi ** 2 * ((fx * s) ** 2 + (fy * s) ** 2))
    return np.real(np.fft.ifft2(np.fft.fft2(a) * g))

# mang troc: dang DOC (vo bong theo tho go), mep rach nho
truong = nhieu(40, 95, 11) + 0.35 * nhieu(10, 22, 12) + 0.18 * nhieu(3, 6, 13)
nguong = np.quantile(truong, 1 - TI_LE_TROC)
troc = np.clip((truong - nguong) / 0.08, 0, 1)            # mep sac ~2-3 diem anh
# vet nut dai doc than: duong di ngau nhien, hep, cung la cho lo (nhung TOI - khe sau)
nut = np.zeros((CO, CO))
for i in range(SO_VET_NUT):
    x = rng.uniform(0, CO); y0 = rng.uniform(0, CO); dai = int(rng.uniform(0.25, 0.6) * CO)
    rong = rng.uniform(1.2, 2.6); dx = 0.0
    for k in range(dai):
        y = int(y0 + k) % CO
        dx = 0.85 * dx + rng.normal(0, 0.35); x = (x + dx) % CO
        t = k / dai; r = rong * (0.35 + 0.65 * np.sin(np.pi * t))   # thon hai dau
        for xx in range(int(x - r - 1), int(x + r + 2)):
            v = np.clip(r - abs(xx - x) + 0.5, 0, 1)
            nut[y, xx % CO] = max(nut[y, xx % CO], v)
lo = np.maximum(troc, nut)
# do cao "do day vo": vo 1, cho lo 0, mep vo con dinh quan len (cuon ra) mot vanh hep
h = 1.0 - lo
gan = np.clip(mo(lo, 4.0) * 3.0, 0, 1) * (1 - lo)           # vanh ngoai sat mep
h = h + 0.25 * gan
h = mo(h, 2.0)
k = 14.0                                                     # do doc bac vo (lan 1: 6, mo 1 -> o cu ly game mep troc khong co bac)
gx = (np.roll(h, -1, 1) - np.roll(h, 1, 1)) * 0.5 * k
gy = (np.roll(h, -1, 0) - np.roll(h, 1, 0)) * 0.5 * k
n = np.dstack([-gx, -gy, np.ones_like(h)]); n /= np.linalg.norm(n, axis=2, keepdims=True)
# do toi: bong duoi mep vo (trong long troc sat mep) + long vet nut
# lan 1 chi co bong trong long -> mang troc nhin nhu vet loang nhat. Nay them VANH VO DAY (mat cat vo, toi) ben ngoai mep ~1,5 cm
bongMep = np.clip(mo(1 - lo, 8.0) * 1.8, 0, 1) * troc
vanhVo = np.clip(mo(troc, 3.5) * 2.6, 0, 1) * (1 - troc)
toi = np.clip(np.maximum(np.maximum(bongMep * 0.7, vanhVo * 0.85), nut * 0.9), 0, 1)
ghi("VoCay3_Troc", np.dstack([0.5 + 0.5 * n[..., 0], 0.5 + 0.5 * n[..., 1], toi, lo]))
bao.append("VoCay3_Troc: troc %.1f%% nut %.2f%% lo %.1f%% toi TB long troc %.2f" % (100 * (troc > 0.5).mean(), 100 * (nut > 0.5).mean(), 100 * (lo > 0.5).mean(), toi[lo > 0.5].mean()))
print("\n".join(bao))
