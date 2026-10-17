# ANH DAT MOI CHO TERRAIN ACT2 + MAN CHINH (17/10/2026, nguoi dung: "mat dat tron bong nhin gia tao, sua cho san sui, nhieu o ga o voi, nut vo,
# dat cat ngon ngang"). Chay trong Blender MCP: exec(open(r"...\anh_dat_moi.py").read()).
#
# Nguon: anh chup that Poly Haven (CC0) tai ve CongCu/AnhDat/Goc/<ten>/{Diffuse,nor_gl,Rough,AO}.jpg (2k).
# Xu ly:
#   - mau = Diffuse x AO^0,8 (nuong khe toi vao mau - terrain Built-in khong co kenh AO);
#   - chuyen sang tuyen tinh, nhan tung kenh cho TRUNG BINH = trung binh anh lop CU (giu tong mau / do sang nguoi dung da duyet - anh cu
#     do lech sang chi 0,009-0,016 nen trong "phang", anh chup that 0,03-0,11), tang tuong phan TUONG DOI x HE_SO_TUONG_PHAN quanh trung binh;
#   - alpha = do bong = 0,04 + (1 - nham) x 0,10 (bai hoc cu: alpha cao -> dat bong nhu guong, van trang loe loet);
#   - normal map OpenGL (nor_gl, Unity dung quy uoc GL) giu nguyen, thu 1024.
import bpy, numpy as np, os

GOC = r"C:\Users\HP\Documents\GameUnity\Diablo25D\CongCu\AnhDat\Goc"
RA = r"C:\Users\HP\Documents\GameUnity\Diablo25D\Assets\Terrain\AnhDatMoi"
CO = 1024
HE_SO_TUONG_PHAN = 1.6   # lan 1 la 1,25: duoi anh trang van gan nhu khong thay chi tiet
# lop terrain -> (anh nguon, mau trung binh sRGB cua anh lop cu - do bang PIL tren Assets/Terrain/Anh_Act2_*.png)
LOP = {
    "CoChet": ("dry_ground_rocks", (0.134, 0.065, 0.020)),   # lop phu nhieu nhat: dat kho nut lan da vun (grass_ground qua phang: lech 0,016)
    "DatTroc": ("dry_ground_01", (0.201, 0.107, 0.046)),
    "SoiDa": ("brown_mud_rocks_01", (0.240, 0.213, 0.179)),
    "Bun": ("brown_mud_03", (0.101, 0.060, 0.029)),
}

def s2l(x): return np.where(x <= 0.04045, x / 12.92, ((x + 0.055) / 1.055) ** 2.4)
def l2s(x): x = np.clip(x, 0, 1); return np.where(x <= 0.0031308, x * 12.92, 1.055 * x ** (1 / 2.4) - 0.055)

def doc(duong, kenh=4):
    im = bpy.data.images.load(duong, check_existing=False)
    im.colorspace_settings.name = "Non-Color"   # doc so tho (sRGB dang byte), tu doi tuyen tinh
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

os.makedirs(RA, exist_ok=True)
bao = []
for lop, (nguon, tb) in LOP.items():
    d = os.path.join(GOC, nguon)
    mau = doc(os.path.join(d, "Diffuse.jpg"))[..., :3]
    ao = doc(os.path.join(d, "AO.jpg"))[..., 0:1]
    nham = doc(os.path.join(d, "Rough.jpg"))[..., 0]
    lin = s2l(mau) * np.power(np.clip(ao, 0.0, 1.0), 0.8)
    tbMoi = lin.reshape(-1, 3).mean(0)
    dich = s2l(np.array(tb))
    lin = lin / tbMoi * dich                                   # trung binh tung kenh = lop cu
    sang = lin @ np.array([0.2126, 0.7152, 0.0722])
    tbSang = sang.mean()
    k = (tbSang + (sang - tbSang) * HE_SO_TUONG_PHAN) / np.maximum(sang, 1e-6)   # tuong phan do sang, giu sac do
    lin = lin * k[..., None]
    rgb = l2s(lin)
    bong = 0.04 + (1.0 - nham) * 0.10
    ghi(lop + "_Mau", np.dstack([rgb, bong]))
    pt = doc(os.path.join(d, "nor_gl.jpg"))
    pt[..., 3] = 1.0
    ghi(lop + "_Phap", pt)
    L = rgb @ np.array([0.2126, 0.7152, 0.0722])
    bao.append("%s <- %s: TB sRGB %s sang %.3f lech %.3f bong %.3f-%.3f" % (lop, nguon, np.round(rgb.reshape(-1, 3).mean(0), 3), L.mean(), L.std(), bong.min(), bong.max()))
print("\n".join(bao))
