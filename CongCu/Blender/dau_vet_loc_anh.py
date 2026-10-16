# -*- coding: utf-8 -*-
# ANH cho DAU VET LOC XOAY CUON (16/10/2026, nguoi dung lan 2: "dau vet tren mat dat phai SAN SUI, NHAM, CO VET MAU chu khong duoc dung tron bong").
# Chay TRONG Blender (MCP): exec(open(r'.../CongCu/Blender/dau_vet_loc_anh.py', encoding='utf-8').read())
# Moi nhieu sinh bang FFT (tuan hoan) -> anh LAP LIEN MACH.
#   DatXoi_Mau.png  (1024, sRGB)  - dat xoi: nen = anh dat dia hinh Act2 (Anh_Act2_DatTroc, lap 2 x 2), cuc dat von (sang toi theo do cao), ke nut toi,
#                                    am toi loang, MAU KHO loang + giot ban (do sam -> nau den).
#   DatXoi_Phap.png (1024, tuyen tinh) - normal map tu ban do cao (cuc dat + ke nut + vet xuoc) -> san, nham.
#   VetMau.png      (512, sRGB, alpha) - lop PHU len goc cay / chan bia / nen nha: vet mau (mang loang + giot ban + vet chay) + bui ban nau den.

import bpy, numpy as np, os

GOC = r'C:\Users\HP\Documents\GameUnity\Diablo25D'
RA = os.path.join(GOC, 'Assets', 'Resources', 'KyNang', 'LocXoay')
rng = np.random.default_rng(16102026)


def nhieu(n, beta, f_min=1.0, f_max=None, seed=None):
    """Nhieu tuan hoan n x n: on trang qua bo loc 1/f^beta trong mien tan so [f_min, f_max] (chu ky / anh). Chuan 0..1."""
    r = np.random.default_rng(seed) if seed is not None else rng
    z = r.standard_normal((n, n))
    F = np.fft.fft2(z)
    fx = np.fft.fftfreq(n) * n
    f = np.sqrt(fx[:, None] ** 2 + fx[None, :] ** 2)
    h = np.where(f > 0, 1.0 / np.maximum(f, 1e-6) ** beta, 0.0)
    h[f < f_min] = 0
    if f_max: h[f > f_max] = 0
    x = np.real(np.fft.ifft2(F * h))
    x -= x.min(); x /= x.max() + 1e-12
    return x


def doc_anh(duong):
    im = bpy.data.images.load(duong, check_existing=False)
    w, h = im.size
    p = np.empty(w * h * 4, dtype=np.float32); im.pixels.foreach_get(p)
    bpy.data.images.remove(im)
    return p.reshape(h, w, 4)


def ghi_anh(ten, arr, tuyen_tinh=False):
    h, w = arr.shape[:2]
    im = bpy.data.images.new(ten, w, h, alpha=True, float_buffer=False)
    if tuyen_tinh:
        im.colorspace_settings.name = 'Non-Color'
    a = np.ones((h, w, 4), dtype=np.float32)
    a[..., :arr.shape[2]] = arr
    im.pixels.foreach_set(a.astype(np.float32).ravel())
    im.filepath_raw = os.path.join(RA, ten + '.png'); im.file_format = 'PNG'
    im.save()
    bpy.data.images.remove(im)


def mau_srgb(c):
    """(lan 1 doi mau sang tuyen tinh -> anh toi den: anh 8 bit cua Blender, pixels la gia tri sRGB luu thang) - giu nguyen."""
    return np.asarray(c, dtype=np.float32)


# ================================================================== DAT XOI
# (lan 1: 1024 lap 2 x 2 lo duong noi; ke nut thanh mang deu nhu da huou cao co - gia; toi den) -> 512 mot o, nut manh dut quang, sang hon
N = 512
nen = doc_anh(os.path.join(GOC, 'Assets', 'Terrain', 'Anh_Act2_DatTroc.png'))[..., :3]
von = nhieu(N, 1.1, 14, 140, 11)
song = nhieu(N, 1.8, 2, 16, 12)
k = nhieu(N, 1.5, 3, 30, 13)
dut = np.clip((nhieu(N, 2.0, 2, 10, 19) - 0.45) * 3, 0, 1)                                       # nut chi o vai vung
nut = np.clip((1 - np.abs(2 * k - 1)) ** 16 * 3.0 - 0.4, 0, 1) * dut
cuc = np.clip((nhieu(N, 0.9, 25, 180, 14) - 0.6) * 4, 0, 1)
cao = 0.5 * von + 0.35 * song + 0.5 * cuc - 0.45 * nut
cao = (cao - cao.min()) / (cao.max() - cao.min())
am = np.clip((nhieu(N, 2.0, 1, 10, 15) - 0.5) * 2.0, 0, 1)
sang = (0.72 + 0.42 * cao) * (1 - 0.45 * nut) * (1 - 0.22 * am)
dat = nen * sang[..., None] * np.array([0.9, 0.85, 0.8])
# MAU KHO: vai mang ban toe (bien rang cua) + giot ban xung quanh - do sam, chu khong den
loang = nhieu(N, 2.2, 1, 8, 16) * 0.65 + nhieu(N, 0.9, 16, 160, 17) * 0.35
mang = np.clip((loang - 0.64) * 10, 0, 1)
giot = np.clip((nhieu(N, 0.5, 50, 250, 18) - 0.82) * 10, 0, 1) * np.clip((loang - 0.5) * 4, 0, 1)
mau_m = np.clip(mang + giot, 0, 1) * 0.85
mau_c = mau_srgb([0.36, 0.045, 0.03]) * (0.65 + 0.45 * von[..., None])
dat = dat * (1 - mau_m[..., None]) + mau_c * mau_m[..., None]
ghi_anh('DatXoi_Mau', np.clip(dat, 0, 1))
hcao = cao + 0.06 * mau_m
gx = (np.roll(hcao, -1, 1) - np.roll(hcao, 1, 1)) * 0.5
gy = (np.roll(hcao, -1, 0) - np.roll(hcao, 1, 0)) * 0.5
s = 7.0
nx, ny, nz = -gx * s, -gy * s, np.ones_like(hcao)
d = np.sqrt(nx ** 2 + ny ** 2 + nz ** 2)
pn = np.stack([nx / d * 0.5 + 0.5, ny / d * 0.5 + 0.5, nz / d * 0.5 + 0.5], axis=-1)
ghi_anh('DatXoi_Phap', pn, tuyen_tinh=True)

# ================================================================== VET MAU (lop phu)
M = 512
l1 = nhieu(M, 2.0, 1, 10, 21) * 0.7 + nhieu(M, 1.0, 16, 120, 22) * 0.3
mang = np.clip((l1 - 0.58) * 8, 0, 1)
giot = np.clip((nhieu(M, 0.6, 40, 220, 23) - 0.78) * 9, 0, 1) * np.clip((l1 - 0.42) * 4, 0, 1)
# vet CHAY doc (giot mau chay xuong): nhieu keo dai theo v
ch = nhieu(M, 1.2, 8, 90, 24)
ch = np.clip((np.repeat(ch.mean(axis=0, keepdims=True), M, 0) * 0.6 + ch * 0.4 - 0.62) * 7, 0, 1) * np.clip((l1 - 0.35) * 3, 0, 1)
mau_a = np.clip(mang + giot + 0.8 * ch, 0, 1)
bui = np.clip((nhieu(M, 1.4, 3, 60, 25) - 0.5) * 1.6, 0, 1) * 0.28                            # bui ban nhe (lan 1: 0,55 phu kin nhu may xam)
toi = nhieu(M, 0.8, 30, 200, 26)
c_mau = mau_srgb([0.4, 0.035, 0.025]) * (0.6 + 0.55 * toi[..., None])
c_bui = mau_srgb([0.16, 0.12, 0.08]) * np.ones((M, M, 1))
a = np.clip(mau_a * 0.95 + bui * (1 - mau_a), 0, 1)
c = (c_mau * mau_a[..., None] + c_bui * (bui * (1 - mau_a))[..., None]) / np.maximum(a[..., None], 1e-4)
ghi_anh('VetMau', np.concatenate([np.clip(c, 0, 1), a[..., None]], axis=-1))
print('xong', os.path.getsize(os.path.join(RA, 'DatXoi_Mau.png')), os.path.getsize(os.path.join(RA, 'VetMau.png')),
      'mau dat %.0f%%' % (mau_m.mean() * 100), 'phu vet %.0f%%' % (a.mean() * 100))
