# -*- coding: utf-8 -*-
# Nhac nen man DANG NHAP + SANH (mot ban dung chung) - STEREO, lap lien mach.
#
# Khac ban "Nghia dia" (nhac_kinh_di.py, chi la nen u am): ban nay co GIAI DIEU
# CHU DE de nho + bon doan tang giam kich tinh, 60 nhip/phut, 128 giay = 32 o:
#   A  0-32   nhip tim, piano don doc, tieng thi tham, chuong mo dau
#   B 32-64   them day vi ba rung (tremolo), dong ca nhe, trong dinh am
#   C 64-96   cao trao: tieng "braam" tram, piano quang tam nang, dong ca lon
#   D 96-128  lang xuong: hop nhac lac dieu, thi tham, tieng rit dan vao vong moi
# Giai dieu chu de = "Dies Irae" (thanh ca tang le Trung co, pham vi cong cong)
# chuyen sang Re thu hoa am: Re Do# Re Sib Do# La Sib Sib.
#
# Lap lien mach nhu nhac_kinh_di.py: su kien dat vao bo dem dai L (tran quan ve
# dau), lop lien tuc co tan so k / L, tieng vang tich chap VONG.
#
# Chay: python CongCu/AmThanh/nhac_sanh.py [duong_ra.wav] [hat_ngau_nhien]

import sys, wave
import numpy as np

SR = 44100
DAI_GIAY = 128.0
L = int(SR * DAI_GIAY)
HAT = int(sys.argv[2]) if len(sys.argv) > 2 else 2610
rng = np.random.default_rng(HAT)
t_bai = np.arange(L) / SR
PI2 = 2 * np.pi

def dat_do_dai(giay, hat):
    # Cho script khac (nhac_tran.py) dung lai bo nhac cu voi do dai / hat rieng.
    # Khong goi thi ban sanh van ra dung nhu cu.
    global DAI_GIAY, L, t_bai, rng
    DAI_GIAY = giay
    L = int(SR * giay)
    t_bai = np.arange(L) / SR
    rng = np.random.default_rng(hat)

def midi(m):
    return 440.0 * 2.0 ** ((m - 69) / 12.0)

def tuan_hoan(f):
    return round(f * DAI_GIAY) / DAI_GIAY

def bo_dem():
    return np.zeros((2, L))

def dat(buf, giay, s):
    n = s.shape[1]
    assert n <= L
    idx = (int(round(giay * SR)) + np.arange(n)) % L
    buf[:, idx] += s

def duong_bao(n, len_giay, xuong_giay):
    e = np.ones(n)
    a = min(n, int(len_giay * SR)); r = min(n, int(xuong_giay * SR))
    if a > 0: e[:a] = np.sin(np.linspace(0, np.pi / 2, a)) ** 2
    if r > 0: e[n - r:] *= np.cos(np.linspace(0, np.pi / 2, r)) ** 2
    return e

def loc_fft(x, dap_ung):
    X = np.fft.rfft(x, axis=-1)
    f = np.fft.rfftfreq(x.shape[-1], 1.0 / SR)
    return np.fft.irfft(X * dap_ung(f), n=x.shape[-1], axis=-1)

def dai_qua(fc, rong_oct):
    # bo loc dai qua hinh chuong tren truc log
    return lambda f: np.exp(-0.5 * ((np.log2(np.maximum(f, 1)) - np.log2(fc)) / rong_oct) ** 2)

def nhieu_cham(so_hai=6, tan_max=0.25):
    y = np.zeros(L)
    kmax = max(1, int(tan_max * DAI_GIAY))
    for _ in range(so_hai):
        k = rng.integers(1, kmax + 1)
        y += rng.uniform(0.3, 1.0) * np.sin(PI2 * k / DAI_GIAY * t_bai + rng.uniform(0, PI2))
    return y / (np.abs(y).max() + 1e-9)

def theo_doan(a, b, c, d):
    # Duong muc to nho theo doan A/B/C/D, tron, tuan hoan (noi D -> A)
    moc = np.array([16.0, 48.0, 80.0, 112.0])
    return np.interp(t_bai, moc, np.array([a, b, c, d]), period=DAI_GIAY)

def lech_trai_phai(s, p):
    # p -1 (trai) .. +1 (phai), giu cong suat; s mot kenh -> (2, n)
    g = (p + 1) * np.pi / 4
    return np.vstack([s * np.cos(g), s * np.sin(g)]) * np.sqrt(2)

# ---------------------------------------------------------------- hoa am
VONG = [  # moi hop am 8 giay; doan C thay Solm bang Mib (hop am Napoli u toi)
    {"goc": 38, "giua": [50, 53, 57, 62], "tren": [62, 65, 69]},          # Rem
    {"goc": 34, "giua": [46, 53, 58, 62], "tren": [62, 65, 70]},          # Sib
    {"goc": 31, "giua": [43, 50, 55, 58, 63], "tren": [62, 67, 70]},      # Solm b6
    {"goc": 33, "giua": [45, 52, 55, 58, 61], "tren": [61, 64, 70]},      # La7 b9
]
MIB = {"goc": 39, "giua": [51, 55, 58, 63], "tren": [63, 67, 70]}

def hop_am_luc(giay):
    i = int(giay // 8) % 4
    if 64 <= giay < 96 and i == 2:
        return MIB
    return VONG[i]

CAU_1 = [50, 49, 50, 46, 49, 45, 46, 46]       # Re Do# Re Sib Do# La Sib Sib
CAU_2 = [53, 53, 55, 53, 52, 50, 49, 50]       # Fa Fa Sol Fa Mi Re Do# Re

# ---------------------------------------------------------------- nhac cu
def piano(m, dai, luc):
    f0 = midi(m)
    n = int((dai + 3.5) * SR)
    t = np.arange(n) / SR
    day = []
    for cent in (-0.9, 0.9):               # hai day lech nhe -> phach
        f = f0 * 2 ** (cent / 1200)
        s = np.zeros(n)
        for k in range(1, 25):
            fk = k * f * np.sqrt(1 + 0.00035 * k * k)   # phan am gian (day thep)
            if fk > 9000: break
            a = k ** -0.9 * np.exp(-k * (0.20 - 0.11 * luc))
            tau = (6.0 if m < 55 else 3.5) / (1 + 0.3 * k)
            s += a * np.exp(-t / tau) * np.sin(PI2 * fk * t + rng.uniform(0, PI2))
        day.append(s)
    bua = loc_fft(rng.standard_normal(n) * np.exp(-t / 0.008), dai_qua(1800, 1.2))
    env = np.ones(n); i = int(dai * SR)
    env[i:] = np.exp(-(t[i:] - dai) / 0.4)       # nha phim
    p = np.clip((m - 55) / 30, -0.6, 0.6)        # not tram trai, cao phai nhu dan that
    trai = (0.62 * day[0] + 0.38 * day[1] + 0.15 * bua) * env
    phai = (0.38 * day[0] + 0.62 * day[1] + 0.15 * bua) * env
    return np.vstack([trai * (1 - p), phai * (1 + p)]) * luc

def day_vi(nots, dai, rung=True):
    # Day vi tremolo (vi ba rung) - rang cua bot qua loc, moi giong nhip rung rieng
    n = int((dai + 4) * SR)
    t = np.arange(n) / SR
    out = np.zeros((2, n))
    for m in nots:
        f0 = midi(m)
        for k in range(2):
            for v in range(2):
                f = f0 * 2 ** (rng.uniform(-8, 8) / 1200)
                rv = 1 + 0.003 * np.sin(PI2 * rng.uniform(4.5, 5.5) * t + rng.uniform(0, PI2))
                pha = PI2 * f * np.cumsum(rv) / SR + rng.uniform(0, PI2)
                s = np.zeros(n)
                for h in range(1, 22):
                    if h * f > 6000: break
                    s += np.sin(h * pha) / h / (1 + (h * f / 900) ** 2)
                if rung:
                    tr = rng.uniform(7.5, 10.5)
                    s *= 0.45 + 0.55 * np.abs(np.sin(PI2 * tr * t + rng.uniform(0, PI2))) ** 1.5
                out[k] += s
    return out * duong_bao(n, 2.5, 4.0)

def formant(fn, nguyen_am):
    bang = {"o": ((450, 80, 1.0), (800, 90, 0.5), (2830, 120, 0.12)),
            "a": ((700, 90, 1.0), (1150, 100, 0.55), (2650, 130, 0.15))}
    return sum(a * np.exp(-0.5 * ((fn - fc) / bw) ** 2) for fc, bw, a in bang[nguyen_am]) + 0.02

def dong_ca(nots, dai, nguyen_am):
    n = int((dai + 4) * SR)
    t = np.arange(n) / SR
    out = np.zeros((2, n))
    for m in nots:
        for k in range(2):
            for v in range(4):
                f = midi(m) * 2 ** (rng.uniform(-10, 10) / 1200)
                rv = 1 + 0.0045 * np.sin(PI2 * rng.uniform(4.6, 5.8) * t + rng.uniform(0, PI2))
                pha = PI2 * f * np.cumsum(rv) / SR
                s = np.zeros(n)
                for h in range(1, 40):
                    if h * f > 4500: break
                    s += formant(h * f, nguyen_am) * np.sin(h * pha + rng.uniform(0, PI2))
                out[k] += s
    return out * duong_bao(n, 3.0, 4.0)

def braam(nots, dai=6.5):
    # Tieng "braam" kieu trailer kinh di: rang cua tram, loc mo bung len roi dong lai
    n = int(dai * SR)
    t = np.arange(n) / SR
    fc = 260 + 1900 * (1 - np.exp(-t / 0.18)) * np.exp(-t / 1.3)
    out = np.zeros((2, n))
    for m in nots:
        for k in range(2):
            for v in range(3):
                f = midi(m) * 2 ** (rng.uniform(-14, 14) / 1200)
                pha = PI2 * f * t + rng.uniform(0, PI2)
                s = np.zeros(n)
                for h in range(1, 48):
                    if h * f > 5000: break
                    s += np.sin(h * pha) / h / (1 + (h * f / fc) ** 2)
                out[k] += s
    out = np.tanh(1.6 * out / (np.abs(out).max() + 1e-9))
    sub = np.sin(PI2 * midi(nots[0] - 12) * t) * 0.6
    out += sub
    return out * duong_bao(n, 0.03, 3.5)

def trong_dinh_am(m, luc=1.0, dai=3.0):
    n = int(dai * SR)
    t = np.arange(n) / SR
    f0 = midi(m)
    s = np.zeros(n)
    for r, a, tau in ((1.0, 1.0, 1.4), (1.5, 0.5, 0.9), (1.98, 0.35, 0.7), (2.44, 0.2, 0.5)):
        f = f0 * r * (1 + 0.06 * np.exp(-t / 0.05))     # cao do tut nhe khi go
        s += a * np.exp(-t / tau) * np.sin(PI2 * np.cumsum(f) / SR)
    s += 0.5 * loc_fft(rng.standard_normal(n) * np.exp(-t / 0.02), dai_qua(300, 1.0))
    s = np.tanh(1.3 * s)
    return lech_trai_phai(s, rng.uniform(-0.2, 0.2)) * luc

def nhip_tim(luc=1.0):
    n = int(0.9 * SR)
    t = np.arange(n) / SR
    def dap(tre, f0, a):
        tt = np.maximum(t - tre, 0)
        f = f0 * (1 + 0.5 * np.exp(-tt / 0.03))
        s = np.sin(PI2 * np.cumsum(f) / SR) * np.exp(-tt / 0.09) * (t >= tre)
        return a * s
    s = dap(0.0, 55, 1.0) + dap(0.30, 47, 0.7)
    s = np.tanh(2.2 * s)          # bao hoa -> sinh hoa am 100-300 Hz, loa dien thoai nghe duoc
    return np.vstack([s, s]) * luc

def chuong(m, dai=14.0):
    n = int(dai * SR)
    t = np.arange(n) / SR
    f0 = midi(m)
    out = np.zeros((2, n))
    for r, a, tau in ((0.5, 1.0, 9.0), (1.0, 0.8, 6.5), (1.183, 0.55, 5.0), (1.506, 0.35, 4.0),
                      (2.0, 0.6, 3.5), (2.514, 0.25, 2.6), (3.011, 0.2, 2.0), (4.166, 0.12, 1.4)):
        for k in range(2):
            f = f0 * r + (0.35 if k else -0.35) * r
            out[k] += a * np.exp(-t / tau) * np.sin(PI2 * f * t + rng.uniform(0, PI2))
    go = loc_fft(rng.standard_normal((2, n)) * np.exp(-t / 0.012), dai_qua(2200, 0.6))
    return (out + 0.6 * go) * duong_bao(n, 0.003, 0.5)

def hop_nhac(m, dai_not=3.5):
    n = int(dai_not * SR)
    t = np.arange(n) / SR
    f = midi(m) * 2 ** (rng.uniform(-25, 25) / 1200)     # lac dieu
    s = (np.sin(PI2 * f * t) * np.exp(-t / 1.4)
         + 0.35 * np.sin(PI2 * f * 2.76 * t) * np.exp(-t / 0.35)
         + 0.15 * np.sin(PI2 * f * 5.40 * t) * np.exp(-t / 0.12))
    return s * duong_bao(n, 0.002, 0.3)

def thi_tham(dai):
    # Tieng thi tham: nhieu loc qua cac dai phu am, ngat thanh "am tiet", troi tu ben nay sang ben kia
    n = int(dai * SR)
    t = np.arange(n) / SR
    z = rng.standard_normal(n)
    s = (loc_fft(z, dai_qua(1300, 0.35)) + 0.8 * loc_fft(z, dai_qua(2600, 0.3))
         + 0.5 * loc_fft(z, dai_qua(5200, 0.4)))
    tiet = np.zeros(n)
    tg = 0.15
    while tg < dai - 0.3:
        w = rng.uniform(0.04, 0.11)
        tiet += rng.uniform(0.3, 1.0) * np.exp(-0.5 * ((t - tg) / w) ** 2)
        tg += rng.uniform(0.12, 0.32) + (rng.uniform(0.3, 0.8) if rng.random() < 0.18 else 0)
    s *= tiet * duong_bao(n, 0.4, 0.8)
    p = np.sin(PI2 * t / dai * rng.uniform(0.6, 1.2) + rng.uniform(0, PI2)) * 0.85
    return lech_trai_phai(s, p)

def tieng_rit(dai=4.0):
    n = int(dai * SR)
    t = np.arange(n) / SR
    out = np.zeros((2, n))
    len_ = (t / t[-1]) ** 2.2
    for k in range(2):
        for m in (93, 94, 95, 97):
            f = midi(m) * (1 + 0.004 * np.sin(PI2 * 6.3 * t + rng.uniform(0, PI2)))
            keo = 0.6 + 0.4 * np.abs(np.sin(PI2 * rng.uniform(7, 11) * t))
            out[k] += np.sin(PI2 * np.cumsum(f) / SR + rng.uniform(0, PI2)) * keo
    out += 2.5 * loc_fft(rng.standard_normal((2, n)), dai_qua(3200, 0.4))
    return out * len_ * duong_bao(n, 0.0, 0.12)

# ---------------------------------------------------------------- cac lop
def lop_drone():
    out = bo_dem()
    for f, a in ((midi(26), 0.5), (midi(38), 0.35), (midi(45), 0.12)):
        for k, lech in ((0, -0.1), (1, 0.1)):
            out[k] += a * np.sin(PI2 * tuan_hoan(f + lech) * t_bai + rng.uniform(0, PI2))
    return out * (0.75 + 0.25 * nhieu_cham(4, 0.05))

def lop_gio():
    z = loc_fft(rng.standard_normal((2, L)), dai_qua(520, 0.9))
    for k in range(2):
        z[k] *= np.clip(0.55 + 0.45 * nhieu_cham(6, 0.12), 0.05, 1)
    return z * theo_doan(1.0, 0.7, 0.45, 1.0)

def lop_day_tram():
    # Day tram giu not goc moi hop am, doan B, C va dau D
    out = bo_dem()
    for g in range(32, 104, 8):
        ha = hop_am_luc(g)
        to = 0.7 if g < 64 else (1.0 if g < 96 else 0.55)
        dat(out, g, day_vi([ha["goc"], ha["goc"] + 12], 8.0, rung=False) * to)
    return out

def lop_day_rung():
    out = bo_dem()
    for g in range(32, 112, 8):
        ha = hop_am_luc(g)
        to = 0.6 if g < 64 else (1.0 if g < 96 else 0.45 if g < 104 else 0.25)
        dat(out, g, day_vi(ha["giua"], 8.0) * to)
    return out

def lop_piano():
    out = bo_dem()
    def cau(bd, nots, nhip, them_quang, luc, quang_duoi=False):
        for i, m in enumerate(nots):
            dai = nhip * (2 if i == len(nots) - 1 else 1)
            s = piano(m, dai, luc)
            if them_quang:
                s += piano(m + 12, dai, luc * 0.7)
            if quang_duoi:
                s += piano(m - 12, dai, luc * 0.9)
            dat(out, bd + i * nhip + rng.uniform(-0.015, 0.015), s)
    cau(4, CAU_1, 1.0, False, 0.55)
    cau(20, CAU_2, 1.0, False, 0.55)
    cau(36, CAU_1, 1.0, True, 0.70)
    cau(52, CAU_2, 1.0, True, 0.70)
    cau(64, CAU_1, 2.0, True, 1.0, quang_duoi=True)    # cao trao: 2 giay mot not, ba quang tam
    cau(80, CAU_2, 2.0, True, 1.0, quang_duoi=True)
    return out

def lop_dong_ca():
    out = bo_dem()
    for g in range(32, 64, 8):
        dat(out, g, dong_ca(hop_am_luc(g)["tren"], 8.0, "o") * 0.55)
    for g in range(64, 96, 8):
        dat(out, g, dong_ca([m + 0 for m in hop_am_luc(g)["tren"]] + [hop_am_luc(g)["tren"][0] + 12], 8.0, "a"))
    return out

def lop_braam():
    out = bo_dem()
    for g in range(64, 96, 8):
        goc = hop_am_luc(g)["goc"] + (12 if hop_am_luc(g)["goc"] < 36 else 0)
        dat(out, g, braam([goc, goc + 7, goc + 12]))
    return out

def lop_trong():
    out = bo_dem()
    for g in range(32, 64, 8):
        dat(out, g, trong_dinh_am(hop_am_luc(g)["goc"], 0.6))
    for g in range(64, 96, 4):
        dat(out, g, trong_dinh_am(hop_am_luc(g)["goc"], 1.0))
    # hoi trong ve dan len truoc cao trao
    for i in range(32):
        g = 60 + i * 0.125
        dat(out, g, trong_dinh_am(38, 0.12 + 0.6 * (i / 31) ** 2, 1.5))
    return out

def lop_tim():
    out = bo_dem()
    to = theo_doan(1.0, 0.8, 0.55, 1.0)
    for g in range(128):
        dat(out, g, nhip_tim(to[int(g * SR)]))
    return out

def lop_chuong():
    out = bo_dem()
    for g, m, p in ((0, 50, -0.3), (32, 50, 0.3), (64, 45, -0.2), (64.02, 57, 0.4), (96, 50, 0.0)):
        s = chuong(m)
        s[0] *= 1 - p; s[1] *= 1 + p
        dat(out, g, s)
    return out

def lop_hop_nhac():
    out = bo_dem()
    vt = 99.0
    for nots, nhip in ((CAU_1, 1.5), (CAU_2, 1.4)):
        for i, m in enumerate(nots):
            s = hop_nhac(m + 24)
            p = 0.55 if i % 2 else -0.55
            dat(out, vt + rng.uniform(-0.07, 0.07), np.vstack([s * (1 - p), s * (1 + p)]))
            vt += nhip * rng.uniform(0.95, 1.12)
        vt += 1.2
    return out

def lop_thi_tham():
    out = bo_dem()
    for g, d in ((6, 7), (17, 9), (101, 7), (110, 11)):
        dat(out, g, thi_tham(d))
    return out

def lop_rit():
    out = bo_dem()
    for g in (60.0, 124.0):
        dat(out, g, tieng_rit())
    return out

# ---------------------------------------------------------------- vang + tron
def phan_hoi_vang(rt_thap=6.5, rt_giua=4.8, rt_cao=2.0, dai=7.0):
    n = int(dai * SR)
    t = np.arange(n) / SR
    ir = np.zeros((2, n))
    for k in range(2):
        z = rng.standard_normal(n)
        thap = loc_fft(z, lambda f: 1 / (1 + (f / 300) ** 4))
        cao = loc_fft(z, lambda f: (f / 3000) ** 4 / (1 + (f / 3000) ** 4))
        giua = z - thap - cao
        for b, rt in ((thap, rt_thap), (giua, rt_giua), (cao, rt_cao)):
            ir[k] += b * np.exp(-6.9 * t / rt)
        ir[k, :int(0.04 * SR)] = 0
        for _ in range(10):
            ir[k, int(rng.uniform(0.04, 0.14) * SR)] += rng.uniform(0.3, 0.8) * rng.choice([-1, 1])
    return ir / np.sqrt((ir ** 2).sum(axis=1, keepdims=True))

def vang_vong(x, ir):
    out = np.zeros_like(x)
    for k in range(2):
        h = np.zeros(L); h[:ir.shape[1]] = ir[k]
        out[k] = np.fft.irfft(np.fft.rfft(x[k]) * np.fft.rfft(h), n=L)
    return out

def chuan_luc_vang(x):
    # Chuan theo do to LUC DANG KEU (lop chi keu vai giay khong bi day len qua muc)
    m = x.mean(axis=0)
    b = int(0.1 * SR)
    khoi = np.sqrt((m[:len(m) // b * b].reshape(-1, b) ** 2).mean(axis=1))
    hoat = khoi[khoi > 0.1 * khoi.max()]
    return x / (np.sqrt((hoat ** 2).mean()) + 1e-12)

def chinh():
    duong_ra = sys.argv[1] if len(sys.argv) > 1 else "CongCu/AmThanh/NhacThu/sanh_thu1.wav"
    kho, gui = bo_dem(), bo_dem()
    lop = [  # ten, ham, muc kho, muc gui vang
        ("drone", lop_drone, 0.10, 0.10),
        ("gio", lop_gio, 0.05, 0.20),
        ("day_tram", lop_day_tram, 0.16, 0.35),
        ("day_rung", lop_day_rung, 0.14, 0.45),
        ("piano", lop_piano, 0.30, 0.55),
        ("dong_ca", lop_dong_ca, 0.10, 0.50),
        ("braam", lop_braam, 0.28, 0.45),
        ("trong", lop_trong, 0.22, 0.45),
        ("tim", lop_tim, 0.24, 0.12),
        ("chuong", lop_chuong, 0.14, 0.75),
        ("hop_nhac", lop_hop_nhac, 0.10, 0.75),
        ("thi_tham", lop_thi_tham, 0.06, 0.40),
        ("rit", lop_rit, 0.03, 0.70),
    ]
    for ten, ham, mk, mg in lop:
        x = chuan_luc_vang(ham())
        kho += x * mk
        gui += x * mg
        print("  xong lop", ten, flush=True)
    tron = kho + 0.9 * vang_vong(gui, phan_hoi_vang())
    tron = loc_fft(tron, lambda f: (f / 25) ** 2 / (1 + (f / 25) ** 2))
    # Do rong stereo theo dai (nhu ban Nghia dia): tram < 150 Hz Side 0,3, tren 0,62
    giua = (tron[0] + tron[1]) / 2
    ben = loc_fft((tron[0] - tron[1]) / 2, lambda f: 0.3 + 0.32 * (f / 150) ** 4 / (1 + (f / 150) ** 4))
    tron = np.vstack([giua + ben, giua - ben])
    tron = loc_fft(tron, lambda f: (0.6 + 0.4 * (f / 160) ** 2 / (1 + (f / 160) ** 2))
                   * (1 + 0.5 * dai_qua(2000, 1.0)(f)))
    tron *= 10 ** (-21 / 20) / np.sqrt((tron ** 2).mean())
    dinh = np.abs(tron).max()
    if dinh > 0.89:
        tron *= 0.89 / dinh       # ha tuyen tinh, khong nen meo
    pcm = (np.clip(tron, -1, 1) * 32767).astype("<i2").T.copy()
    with wave.open(duong_ra, "wb") as w:
        w.setnchannels(2); w.setsampwidth(2); w.setframerate(SR)
        w.writeframes(pcm.tobytes())
    print("da ghi", duong_ra, "dinh truoc khi ha", round(float(dinh), 3))

if __name__ == "__main__":
    chinh()
