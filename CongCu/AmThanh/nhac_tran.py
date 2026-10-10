# -*- coding: utf-8 -*-
# Nhac nen TRONG TRAN Act2 (Nghia dia) - STEREO, lap lien mach, 192 giay.
#
# Ban dau (nhac_kinh_di.py) chi la nen u am dung yen, khong giai dieu, khong
# nhip -> nguoi dung che "qua do". Ban nay dung lai bo nhac cu cua nhac sanh
# (nhac_sanh.py, nguoi dung da duyet) va them phan NHIP de danh nhau:
#   90 nhip/phut, 72 o (moi o 2,667 s):
#   I    o  0-8   mo man: chuong, trong taiko lon, nhip tim cham, thi tham
#   II   o  8-24  day tram nay moc don (ostinato), taiko, piano cao rai rac
#   III  o 24-40  cao trao: dong ca tung giai dieu Dies Irae, braam, ken dong, trong day
#   IV   o 40-48  nghi: hop nhac lac dieu, day rung, nhip tim
#   V    o 48-64  dang lai voi hoa am khac (Sol Mib Sib La), piano quang tam tram
#   VI   o 64-72  thua dan, tieng rit dan ve chuong dau vong
# Giai dieu chu de giong nhac sanh -> hai ban noi nhau thanh mot the.
# De danh cho tieng ky nang: phan 2-8 kHz de mong (tieng no, set, bang nam o do).
#
# Chay: python CongCu/AmThanh/nhac_tran.py [duong_ra.wav] [hat_ngau_nhien]

import os, sys, wave
import numpy as np

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import nhac_sanh as ns

HAT = int(sys.argv[2]) if len(sys.argv) > 2 else 3110
ns.dat_do_dai(192.0, HAT)
SR, L, PI2 = ns.SR, ns.L, ns.PI2
rng = ns.rng
midi, dat, bo_dem, loc_fft, dai_qua = ns.midi, ns.dat, ns.bo_dem, ns.loc_fft, ns.dai_qua

NHIP = 60.0 / 90.0
O = 4 * NHIP

def gb(o, m16=0.0):
    # giay cua o thu "o", them "m16" not moc kep (1/16)
    return o * O + m16 * NHIP / 4

def theo_moc(diem):
    # duong muc to nho qua cac moc (giay, gia tri), tuan hoan dung L
    g = np.array([d[0] for d in diem]); v = np.array([d[1] for d in diem])
    return np.interp(ns.t_bai, g, v, period=ns.DAI_GIAY)

# ---------------------------------------------------------------- hoa am
VONG_A = [38, 38, 34, 33]          # Re Re Sib La (doan II, III) - moi goc 2 o
VONG_B = [43, 39, 46, 45]          # Sol Mib Sib La (doan V)
TREN = {38: [62, 65, 69], 34: [62, 65, 70], 33: [61, 64, 70],
        43: [62, 67, 70], 39: [63, 67, 70], 46: [62, 65, 70], 45: [61, 64, 70]}

def goc_o(o):
    vong = VONG_B if 48 <= o < 64 else VONG_A
    return vong[(o // 2) % 4]

# ---------------------------------------------------------------- nhac cu moi
def day_nay(m, luc=1.0, dai=0.30):
    # Day nay (spiccato): cung can day ngan, sang luc dau roi tat nhanh
    n = int((dai + 0.5) * SR)
    t = np.arange(n) / SR
    fc = 650 + 1600 * np.exp(-t / 0.05)
    out = np.zeros((2, n))
    for k in range(2):
        for v in range(2):
            f = midi(m) * 2 ** (rng.uniform(-6, 6) / 1200)
            pha = PI2 * f * t + rng.uniform(0, PI2)
            s = np.zeros(n)
            for h in range(1, 26):
                if h * f > 6000: break
                s += np.sin(h * pha) / h / (1 + (h * f / fc) ** 2)
            out[k] += s
    cung = loc_fft(rng.standard_normal((2, n)) * np.exp(-t / 0.015), dai_qua(2400, 0.7))
    env = ns.duong_bao(n, 0.006, 0.12) * np.exp(-t / 0.16)
    return (out + 0.25 * cung) * env * luc

def taiko(f0, luc=1.0, dai=1.8):
    n = int(dai * SR)
    t = np.arange(n) / SR
    f = f0 * (1 + 0.35 * np.exp(-t / 0.04))
    than = np.sin(PI2 * np.cumsum(f) / SR) * np.exp(-t / 0.45)
    than += 0.4 * np.sin(PI2 * np.cumsum(f * 1.6) / SR) * np.exp(-t / 0.25)
    da = loc_fft(rng.standard_normal(n) * np.exp(-t / 0.03), lambda ff: 1 / (1 + (ff / 450) ** 2))
    s = np.tanh(1.6 * (than + 0.7 * da / (np.abs(da).max() + 1e-9)))
    return ns.lech_trai_phai(s, rng.uniform(-0.25, 0.25)) * luc

def ken_dong(nots, dai=1.3):
    # Ken dong nhan (stab) - ngan, loc mo nhanh, bao hoa
    n = int(dai * SR)
    t = np.arange(n) / SR
    fc = 400 + 2400 * np.exp(-t / 0.12)
    out = np.zeros((2, n))
    for m in nots:
        for k in range(2):
            for v in range(3):
                f = midi(m) * 2 ** (rng.uniform(-12, 12) / 1200)
                pha = PI2 * f * t + rng.uniform(0, PI2)
                s = np.zeros(n)
                for h in range(1, 30):
                    if h * f > 5000: break
                    s += np.sin(h * pha) / h / (1 + (h * f / fc) ** 2)
                out[k] += s
    out = np.tanh(1.8 * out / (np.abs(out).max() + 1e-9))
    return out * ns.duong_bao(n, 0.015, 0.6) * np.exp(-t / 0.5)

def ca_tung(m, dai, nguyen_am="a", luc=1.0):
    # Dong ca tung tung not (khac ns.dong_ca chi ngan dai): vao nhanh 0,08 s
    n = int((dai + 0.6) * SR)
    t = np.arange(n) / SR
    out = np.zeros((2, n))
    for k in range(2):
        for v in range(3):
            f = midi(m) * 2 ** (rng.uniform(-10, 10) / 1200)
            rv = 1 + 0.004 * np.sin(PI2 * rng.uniform(4.8, 5.8) * t + rng.uniform(0, PI2))
            pha = PI2 * f * np.cumsum(rv) / SR
            s = np.zeros(n)
            for h in range(1, 40):
                if h * f > 4500: break
                s += ns.formant(h * f, nguyen_am) * np.sin(h * pha + rng.uniform(0, PI2))
            out[k] += s
    env = ns.duong_bao(n, 0.08, 0.6)
    env[int(dai * SR):] *= np.exp(-(t[int(dai * SR):] - dai) / 0.25)
    return out * env * luc

# ---------------------------------------------------------------- cac lop
def lop_day_nay():
    out = bo_dem()
    mau = [0, 0, 3, 0, 1, 0, -1, 0]           # Re Re Fa Re Mib Re Do# Re - chromatic ghe
    for o in list(range(8, 40)) + list(range(48, 68)):
        if o < 24: to = 0.6 + 0.4 * (o - 8) / 16
        elif o < 40: to = 1.0
        elif o < 64: to = 0.75 + 0.25 * (o - 48) / 16
        else: to = 0.5 - 0.1 * (o - 64)
        g = goc_o(o)
        for i, d in enumerate(mau):
            nhan = 1.0 if i in (0, 4) else 0.7
            s = day_nay(g + d, to * nhan)
            if i == 0:
                s += day_nay(g + d + 12, to * 0.6)
            dat(out, gb(o, i * 2) + rng.uniform(-0.008, 0.008), s)
    return out

def lop_taiko():
    out = bo_dem()
    def nhip(o, vt, f0, luc):
        for v in vt:
            dat(out, gb(o, v), taiko(f0, luc))
    for o in (0, 4, 40):
        dat(out, gb(o), taiko(46, 1.0, 2.2))
    for o in list(range(8, 24)) + list(range(48, 56)):
        nhip(o, (0, 6, 8), 52, 0.75)
        if o % 8 == 7:
            for i, v in enumerate((8, 10, 12, 13, 14, 15)):
                dat(out, gb(o, v), taiko(58, 0.4 + 0.1 * i))
    for o in list(range(24, 40)) + list(range(56, 64)):
        nhip(o, (0, 3, 6, 8, 10), 50, 0.9)
        if o % 4 == 3:
            for i, v in enumerate(range(8, 16)):
                dat(out, gb(o, v), taiko(60, 0.45 + 0.07 * i))
    for o in range(64, 68):
        nhip(o, (0, 8), 50, 0.7 - 0.1 * (o - 64))
    return out

def lop_trong_cao():
    out = bo_dem()
    for o in list(range(8, 40)) + list(range(48, 64)):
        vt = (4, 12) if (o < 24 or 48 <= o < 56) else (4, 12, 14)
        if o < 24 and o % 2 == 0: vt = (12,)
        for v in vt:
            dat(out, gb(o, v), taiko(rng.uniform(118, 132), 0.6))
    return out

def lop_tim():
    out = bo_dem()
    for o in list(range(0, 8)) + list(range(40, 48)) + list(range(68, 72)):
        for nh in (0, 2):
            dat(out, gb(o, nh * 4), ns.nhip_tim(0.9))
    return out

def lop_braam():
    out = bo_dem()
    for o in (24, 28, 32, 36, 56, 60):
        g = goc_o(o)
        g = g if g >= 36 else g + 12
        dat(out, gb(o), ns.braam([g, g + 7, g + 12], 6.0))   # sub tu dong = g - 12
    return out

def lop_ken():
    out = bo_dem()
    for o in (26, 30, 34, 38, 58, 62):
        nots = TREN[goc_o(o)]
        for v in (6, 10):
            dat(out, gb(o, v), ken_dong([m - 12 for m in nots]))
    return out

def lop_ca_tung():
    out = bo_dem()
    def cau(o0, nots, nang):
        for i, m in enumerate(nots):
            dat(out, gb(o0) + i * 2 * NHIP, ca_tung(m + nang, 2 * NHIP * 0.95, "a"))
    cau(24, ns.CAU_1, 12); cau(28, ns.CAU_2, 12)
    cau(32, ns.CAU_1, 24); cau(36, ns.CAU_2, 24)
    return out

def lop_ca_nen():
    out = bo_dem()
    for o in range(48, 64, 2):
        dat(out, gb(o), ns.dong_ca(TREN[goc_o(o)], 2 * O, "o"))
    return out

def lop_piano():
    out = bo_dem()
    def cau(o0, nots, nhip, nang, luc, quang_duoi=False):
        for i, m in enumerate(nots):
            dai = nhip * (2 if i == len(nots) - 1 else 1)
            s = ns.piano(m + nang, dai, luc)
            if quang_duoi:
                s += ns.piano(m + nang - 12, dai, luc * 0.9)
            dat(out, gb(o0) + i * nhip + rng.uniform(-0.01, 0.01), s)
    cau(12, ns.CAU_1, NHIP, 24, 0.5)
    cau(20, ns.CAU_2, NHIP, 24, 0.5)
    cau(48, ns.CAU_1, 2 * NHIP, 0, 0.9, True)
    cau(52, ns.CAU_2, 2 * NHIP, 0, 0.9, True)
    return out

def lop_hop_nhac():
    out = bo_dem()
    vt = gb(40) + 0.3
    for nots in (ns.CAU_1, ns.CAU_2):
        for i, m in enumerate(nots):
            s = ns.hop_nhac(m + 24)
            p = 0.55 if i % 2 else -0.55
            dat(out, vt + rng.uniform(-0.06, 0.06), np.vstack([s * (1 - p), s * (1 + p)]))
            vt += 1.25 * rng.uniform(0.95, 1.1)
        vt += 0.8
    return out

def lop_day_rung():
    out = bo_dem()
    for o in (40, 42, 44, 46):
        dat(out, gb(o), ns.day_vi([50, 53, 57, 62] if o < 44 else [46, 53, 58, 62], 2 * O) * 0.8)
    return out

def lop_chuong():
    out = bo_dem()
    for g, m, p in ((0, 50, -0.3), (gb(24), 50, 0.3), (gb(48), 45, -0.2)):
        s = ns.chuong(m)
        s[0] *= 1 - p; s[1] *= 1 + p
        dat(out, g, s)
    return out

def lop_thi_tham():
    out = bo_dem()
    for g, d in ((3, 7), (12, 8), (gb(41), 7), (gb(44) + 1, 8), (gb(68), 9)):
        dat(out, g, ns.thi_tham(d))
    return out

def lop_rit():
    out = bo_dem()
    for o in (22.5, 46.5, 70.5):
        dat(out, gb(o), ns.tieng_rit(4.0))
    return out

def lop_gio():
    z = loc_fft(rng.standard_normal((2, L)), dai_qua(520, 0.9))
    for k in range(2):
        z[k] *= np.clip(0.55 + 0.45 * ns.nhieu_cham(6, 0.12), 0.05, 1)
    return z * theo_moc([(10, 1.0), (60, 0.5), (90, 0.4), (117, 1.0), (150, 0.5), (182, 1.0)])

# ---------------------------------------------------------------- gioi han dinh
def gioi_han(x, tran=0.89, khoi=256):
    # Han dinh nhin truoc ~6 ms: trong taiko / braam khong ep ca bai nho lai
    n = x.shape[1]
    so = -(-n // khoi)
    pad = np.zeros((2, so * khoi)); pad[:, :n] = x
    dinh = np.abs(pad).max(axis=0).reshape(so, khoi).max(axis=1)
    g = np.minimum(1.0, tran / np.maximum(dinh, 1e-9))
    g = np.minimum.reduce([np.roll(g, s) for s in range(-3, 4)])
    k = 9
    g = np.convolve(np.r_[g[-k:], g, g[:k]], np.ones(k) / k, "same")[k:-k]
    tam = (np.arange(so) + 0.5) * khoi
    return x * np.interp(np.arange(n), tam, g, period=so * khoi)

# Lan 2 (do tung lop theo doan): doan II (day nay) nho hon ca doan nghi IV -> tang trong
# + day nay o II; doan IV con gio, thi tham lan -> ha rieng o IV
GIUA_DOAN = [4, 16, 32, 44, 56, 68]
DUONG_DOAN = {
    "day_nay":   [1, 1.40, 1, 1, 1, 1],
    "taiko":     [1, 1.30, 1, 1, 1, 1],
    "trong_cao": [1, 1.30, 1, 1, 1, 1],
    "gio":       [1, 1, 1, 0.55, 1, 1],
    "thi_tham":  [1, 1, 1, 0.65, 1, 1],
}

def chinh():
    duong_ra = sys.argv[1] if len(sys.argv) > 1 else "CongCu/AmThanh/NhacThu/tran_act2_thu2.wav"
    kho, gui = bo_dem(), bo_dem()
    # Muc tron do tung lop theo doan (lan 1: doan nghi IV to nhat vi hop nhac -4,1 dB va
    # day rung -7,9 dB gui vang qua nhieu; chuong lan het doan mo man)
    lop = [  # ten, ham, muc kho, muc gui vang
        ("drone", ns.lop_drone, 0.07, 0.07),
        ("gio", lop_gio, 0.04, 0.15),
        ("day_nay", lop_day_nay, 0.22, 0.28),
        ("taiko", lop_taiko, 0.30, 0.30),
        ("trong_cao", lop_trong_cao, 0.12, 0.30),
        ("tim", lop_tim, 0.18, 0.10),
        ("braam", lop_braam, 0.22, 0.40),
        ("ken", lop_ken, 0.11, 0.35),
        ("ca_tung", lop_ca_tung, 0.13, 0.45),
        ("ca_nen", lop_ca_nen, 0.05, 0.25),
        ("piano", lop_piano, 0.15, 0.30),
        ("hop_nhac", lop_hop_nhac, 0.04, 0.15),
        ("day_rung", lop_day_rung, 0.05, 0.18),
        ("chuong", lop_chuong, 0.08, 0.35),
        ("thi_tham", lop_thi_tham, 0.05, 0.40),
        ("rit", lop_rit, 0.022, 0.52),
    ]
    # NHAC_LUU = thu muc luu tung lop (.npy): chinh muc tron khong phai tong hop lai 10 phut
    luu = os.environ.get("NHAC_LUU")
    for ten, ham, mk, mg in lop:
        tep = os.path.join(luu, "%s_%d.npy" % (ten, HAT)) if luu else None
        if tep and os.path.exists(tep):
            x = np.load(tep)
        else:
            x = ns.chuan_luc_vang(ham()).astype(np.float32)
            if tep: np.save(tep, x)
        if ten in DUONG_DOAN:
            # nhan theo doan (gia tri tai giua moi doan I..VI, noi tron giua cac doan)
            x = x * theo_moc([(gb(o), v) for o, v in zip(GIUA_DOAN, DUONG_DOAN[ten])])
        kho += x * mk
        gui += x * mg
        print("  xong lop", ten, flush=True)
    tron = kho + 0.85 * ns.vang_vong(gui, ns.phan_hoi_vang())
    tron = loc_fft(tron, lambda f: (f / 25) ** 2 / (1 + (f / 25) ** 2))
    giua = (tron[0] + tron[1]) / 2
    ben = loc_fft((tron[0] - tron[1]) / 2, lambda f: 0.3 + 0.32 * (f / 150) ** 4 / (1 + (f / 150) ** 4))
    tron = np.vstack([giua + ben, giua - ben])
    # Nhu nhac sanh nhung nang 1-2 kHz NHE hon (0,25) va KHOET nhe 3-6 kHz: cho tieng ky nang
    tron = loc_fft(tron, lambda f: (0.6 + 0.4 * (f / 160) ** 2 / (1 + (f / 160) ** 2))
                   * (1 + 0.25 * dai_qua(1400, 0.9)(f)) * (1 - 0.3 * dai_qua(4200, 0.6)(f)))
    tron *= 10 ** (-20 / 20) / np.sqrt((tron ** 2).mean())
    dinh_truoc = float(np.abs(tron).max())
    tron = gioi_han(tron)
    dinh = np.abs(tron).max()
    if dinh > 0.89:
        tron *= 0.89 / dinh
    pcm = (np.clip(tron, -1, 1) * 32767).astype("<i2").T.copy()
    with wave.open(duong_ra, "wb") as w:
        w.setnchannels(2); w.setsampwidth(2); w.setframerate(SR)
        w.writeframes(pcm.tobytes())
    print("da ghi", duong_ra, "| dinh truoc han", round(dinh_truoc, 3), "sau", round(float(dinh), 3))

if __name__ == "__main__":
    chinh()
