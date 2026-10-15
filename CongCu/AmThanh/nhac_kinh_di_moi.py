# -*- coding: utf-8 -*-
# Nhac nen KINH DI moi (15/10/2026) - STEREO, KHONG MOT LOP NAO LAM TU TIENG ON.
#
# Nguoi dung che ban cu (nhac_sanh.py / nhac_tran.py): "qua nhieu tieng tap am xao xac,
# choi tai". Do pho ban cu: doan co gio / thi tham / tieng rit chiem 94-97% nang luong
# o dai 1-8 kHz, do phang pho (giong tieng on) toi 0,20. Ban nay:
#   - moi nhac cu la TONG CAC SONG SIN (co cao do) - khong gio, khong thi tham, khong
#     tieng rit, khong tieng bua dan / dui trong bang tieng on;
#   - ca ban loc thong thap ~4,5 kHz, cat < 120 Hz, phan duoi 200 Hz giu <= ~25%
#     (loa dien thoai), phan chinh 200 Hz - 2 kHz;
#   - vang (reverb) tat nhanh o dai cao.
#
# Ba kieu (nguoi dung chon qua mau nghe thu):
#   A "Nha tho ma"      - dan organ tram, dong ca nghich tai, chuong dong, tieng ren
#   B "Day dan run ron" - cum day truot cao do, kim loai keo vi, nhip tim, cello don
#   C "Hop nhac ma"     - hop nhac lac dieu, dan piano nguoc, ren tre con, drone phach
# Moi kieu hai che do: "sanh" (khong nhip, u am) va "tran" (90 nhip/phut, trong, day nay).
# Giai dieu chu de: "Dies Irae" (thanh ca tang le Trung co, pham vi cong cong) o Re thu.
#
# Chay: python CongCu/AmThanh/nhac_kinh_di_moi.py <A|B|C> <sanh|tran> <giay> <duong_ra.wav> [hat]
# Ban day du lap lien mach: su kien dat vao bo dem vong (tran quan ve dau), vang tich chap vong.

import sys, wave
import numpy as np

SR = 44100
PI2 = 2 * np.pi
DAI = 40.0
L = int(SR * DAI)
rng = np.random.default_rng(1)

def dat_do_dai(giay, hat):
    global DAI, L, rng
    DAI = giay
    L = int(SR * giay)
    rng = np.random.default_rng(hat)

def midi(m):
    return 440.0 * 2.0 ** ((m - 69) / 12.0)

def bo_dem():
    return np.zeros((2, L))

def dat(buf, giay, s):
    n = min(s.shape[1], L)
    idx = (int(round(giay * SR)) + np.arange(n)) % L
    buf[:, idx] += s[:, :n]

def bao(n, len_giay, xuong_giay):
    e = np.ones(n)
    a = min(n, int(len_giay * SR)); r = min(n, int(xuong_giay * SR))
    if a > 0: e[:a] = np.sin(np.linspace(0, np.pi / 2, a)) ** 2
    if r > 0: e[n - r:] *= np.cos(np.linspace(0, np.pi / 2, r)) ** 2
    return e

def loc(x, h):
    X = np.fft.rfft(x, axis=-1)
    f = np.fft.rfftfreq(x.shape[-1], 1.0 / SR)
    return np.fft.irfft(X * h(f), n=x.shape[-1], axis=-1)

def dai_qua(fc, rong_oct):
    return lambda f: np.exp(-0.5 * ((np.log2(np.maximum(f, 1)) - np.log2(fc)) / rong_oct) ** 2)

def lech(s, p):
    g = (p + 1) * np.pi / 4
    return np.vstack([s * np.cos(g), s * np.sin(g)]) * np.sqrt(2)

def troi_cham(n, tan=0.3, so=4):
    # duong cong ngau nhien muot (tong vai song sin cham), bien do ~[-1, 1]
    t = np.arange(n) / SR
    y = sum(rng.uniform(0.4, 1) * np.sin(PI2 * rng.uniform(0.3, 1) * tan * t + rng.uniform(0, PI2))
            for _ in range(so))
    return y / (np.abs(y).max() + 1e-9)

def rang_cua(f, fc, tran=4200.0, pha0=None):
    # song rang cua co cao do f (mang Hz theo mau), loc mem o fc - chi tong song sin
    pha = PI2 * np.cumsum(f) / SR + (rng.uniform(0, PI2) if pha0 is None else pha0)
    ftb = float(np.mean(f))
    s = np.zeros(len(f))
    for h in range(1, 60):
        if h * ftb > tran: break
        s += np.sin(h * pha) / h / (1 + (h * ftb / fc) ** 2)
    return s

# ---------------------------------------------------------------- nhac cu (deu chi co song sin)
def cum_day(nots, dai, truot=0.0, sang=1400, len_=0.4, xuong=0.3, rung=True):
    # Cum day (dan day) - moi not 2 kenh x 3 giong lech nhe; "truot" = so cent truot dan
    n = int(dai * SR)
    t = np.arange(n) / SR
    r = (t / dai) ** 1.5
    out = np.zeros((2, n))
    for m in nots:
        for k in range(2):
            for v in range(3):
                c = rng.uniform(-9, 9) + truot * r * rng.uniform(0.7, 1.3)
                f = midi(m) * 2 ** (c / 1200)
                if rung:
                    f = f * (1 + 0.0035 * np.sin(PI2 * rng.uniform(4.6, 5.6) * t + rng.uniform(0, PI2)))
                else:
                    f = f * np.ones(n)
                out[k] += rang_cua(f, sang)
    return out * bao(n, dai * len_, dai * xuong) / max(1, len(nots)) ** 0.5

def formant(fn, nguyen_am):
    bang = {"o": ((450, 80, 1.0), (800, 90, 0.5), (2830, 120, 0.08)),
            "a": ((700, 90, 1.0), (1150, 100, 0.55), (2650, 130, 0.1)),
            "u": ((325, 60, 1.0), (700, 80, 0.3), (2530, 100, 0.04))}
    return sum(a * np.exp(-0.5 * ((fn - fc) / bw) ** 2) for fc, bw, a in bang[nguyen_am]) + 0.015

def dong_ca(nots, dai, nguyen_am="o", len_=0.35, xuong=0.35, so_giong=3):
    n = int(dai * SR)
    t = np.arange(n) / SR
    out = np.zeros((2, n))
    for m in nots:
        for k in range(2):
            for v in range(so_giong):
                f = midi(m) * 2 ** (rng.uniform(-11, 11) / 1200)
                rv = 1 + 0.0045 * np.sin(PI2 * rng.uniform(4.6, 5.8) * t + rng.uniform(0, PI2))
                pha = PI2 * f * np.cumsum(rv) / SR
                s = np.zeros(n)
                for h in range(1, 40):
                    if h * f > 3800: break
                    s += formant(h * f, nguyen_am) * np.sin(h * pha + rng.uniform(0, PI2))
                out[k] += s
    return out * bao(n, dai * len_, dai * xuong) / max(1, len(nots)) ** 0.5

def ren(m0, m1, dai, nguyen_am="u", p0=-0.6, p1=0.6):
    # Tieng ren ma: mot giong nguyen am truot cao do m0 -> m1, rung rong, troi trai -> phai
    n = int(dai * SR)
    t = np.arange(n) / SR
    x = t / dai
    m = m0 + (m1 - m0) * (0.5 - 0.5 * np.cos(np.pi * x))
    f = midi(m) * (1 + 0.012 * np.sin(PI2 * 5.2 * t) * np.minimum(1, t / 1.5))
    pha = PI2 * np.cumsum(f) / SR
    s = np.zeros(n)
    for h in range(1, 30):
        fh = h * f
        if fh.mean() > 3500: break
        s += formant(fh, nguyen_am) * np.sin(h * pha)
    s *= bao(n, dai * 0.35, dai * 0.45)
    p = p0 + (p1 - p0) * x
    g = (p + 1) * np.pi / 4
    return np.vstack([s * np.cos(g), s * np.sin(g)]) * np.sqrt(2)

def kim_loai(m, dai, uon=45.0):
    # Kim loai keo vi (kieu waterphone): phan am KHONG dieu hoa, cao do uon cham, moi kenh khac nhau
    n = int(dai * SR)
    t = np.arange(n) / SR
    out = np.zeros((2, n))
    for k in range(2):
        u = troi_cham(n, 0.5, 3) * uon
        for r, a in ((1.0, 1.0), (2.32, 0.55), (4.25, 0.32), (6.63, 0.18), (9.38, 0.08)):
            f = midi(m) * r * 2 ** (u * (1 + 0.2 * r) / 1200)
            if f.mean() > 4000: break
            am = 0.6 + 0.4 * troi_cham(n, 1.2, 3)
            out[k] += a * am * np.sin(PI2 * np.cumsum(f) / SR + rng.uniform(0, PI2))
    return out * bao(n, dai * 0.45, dai * 0.4)

def chuong(m, dai=12.0, p=0.0):
    # Chuong dong: phan am chuong + "tieng go" la vai phan am cao tat rat nhanh (khong tieng on)
    n = int(dai * SR)
    t = np.arange(n) / SR
    f0 = midi(m)
    out = np.zeros((2, n))
    for r, a, tau in ((0.5, 1.0, 8.0), (1.0, 0.8, 6.0), (1.183, 0.5, 4.5), (1.506, 0.35, 3.5),
                      (2.0, 0.5, 3.0), (2.514, 0.22, 2.2), (3.011, 0.16, 1.6),
                      (4.166, 0.25, 0.08), (5.43, 0.18, 0.05), (6.79, 0.12, 0.035)):
        if f0 * r > 4200: continue
        for k in range(2):
            f = f0 * r + (0.3 if k else -0.3) * r
            out[k] += a * np.exp(-t / tau) * np.sin(PI2 * f * t + rng.uniform(0, PI2))
    out[0] *= 1 - p; out[1] *= 1 + p
    return out * bao(n, 0.002, 0.5)

def hop_nhac(m, dai=3.0, p=0.0):
    n = int(dai * SR)
    t = np.arange(n) / SR
    f = midi(m) * 2 ** (rng.uniform(-28, 28) / 1200)     # lac dieu
    s = (np.sin(PI2 * f * t) * np.exp(-t / 1.2)
         + 0.3 * np.sin(PI2 * f * 2.76 * t) * np.exp(-t / 0.3)
         + 0.1 * np.sin(PI2 * f * 5.40 * t) * np.exp(-t / 0.1))
    return lech(s * bao(n, 0.003, 0.3), p)

def piano(m, dai, luc=1.0, nguoc=False):
    # Piano chi co day dan (khong tieng bua); nguoc = dao thoi gian -> tieng phong len roi tat dot ngot
    f0 = midi(m)
    n = int((dai + 2.5) * SR)
    t = np.arange(n) / SR
    day = []
    for cent in (-1.0, 1.0):
        f = f0 * 2 ** (cent / 1200)
        s = np.zeros(n)
        for k in range(1, 20):
            fk = k * f * np.sqrt(1 + 0.0004 * k * k)
            if fk > 4000: break
            a = k ** -1.0 * np.exp(-k * 0.12)
            tau = (5.0 if m < 55 else 3.0) / (1 + 0.3 * k)
            s += a * np.exp(-t / tau) * np.sin(PI2 * fk * t + rng.uniform(0, PI2))
        day.append(s)
    env = np.ones(n); i = int(dai * SR)
    env[i:] = np.exp(-(t[i:] - dai) / 0.4)
    env *= bao(n, 0.004, 0.05)
    p = np.clip((m - 57) / 30, -0.5, 0.5)
    out = np.vstack([(0.6 * day[0] + 0.4 * day[1]) * (1 - p), (0.4 * day[0] + 0.6 * day[1]) * (1 + p)]) * env * luc
    if nguoc:
        out = out[:, ::-1] * bao(n, 0.0, 0.03)
    return out

def nhip_tim(luc=1.0):
    n = int(0.9 * SR)
    t = np.arange(n) / SR
    def dap(tre, f0, a):
        tt = np.maximum(t - tre, 0)
        f = f0 * (1 + 0.5 * np.exp(-tt / 0.03))
        return a * np.sin(PI2 * np.cumsum(f) / SR) * np.exp(-tt / 0.09) * (t >= tre)
    # than 95 / 80 Hz (58 / 50 Hz bi bo loc cat 120 Hz nuot gan het: -40 dB so voi hop nhac) + bao hoa
    s = np.tanh(3.0 * (dap(0.0, 95, 1.0) + dap(0.30, 80, 0.7)))   # -> hoa am 160-500 Hz, loa dien thoai nghe duoc
    return np.vstack([s, s]) * luc

def trong(f0, luc=1.0, dai=1.5, p=0.0):
    # Trong tram: than sin tut cao do + "tieng go" sin 700-900 Hz tat 10 ms (khong tieng on)
    n = int(dai * SR)
    t = np.arange(n) / SR
    f = f0 * (1 + 0.4 * np.exp(-t / 0.035))
    # f0 truyen vao la "goc" cu (52 / 60 Hz) - than that nhan 1,8 de con lai sau bo loc cat 120 Hz
    f = f * 1.8
    s = np.sin(PI2 * np.cumsum(f) / SR) * np.exp(-t / 0.38)
    s += 0.45 * np.sin(PI2 * np.cumsum(f * 1.59) / SR) * np.exp(-t / 0.22)
    s += 0.25 * np.sin(PI2 * np.cumsum(f * 2.14) / SR) * np.exp(-t / 0.15)
    s += 0.5 * np.sin(PI2 * rng.uniform(700, 900) * t) * np.exp(-t / 0.010)
    s = np.tanh(2.6 * s)
    return lech(s * bao(n, 0.001, 0.2), p) * luc

def day_nay(m, luc=1.0, dai=0.28):
    n = int((dai + 0.4) * SR)
    t = np.arange(n) / SR
    fc = 600 + 1400 * np.exp(-t / 0.05)
    out = np.zeros((2, n))
    for k in range(2):
        for v in range(2):
            f = midi(m) * 2 ** (rng.uniform(-6, 6) / 1200)
            pha = PI2 * f * t + rng.uniform(0, PI2)
            s = np.zeros(n)
            for h in range(1, 26):
                if h * f > 4200: break
                s += np.sin(h * pha) / h / (1 + (h * f / fc) ** 2)
            out[k] += s
    return out * bao(n, 0.008, 0.12) * np.exp(-t / 0.15) * luc

def braam(nots, dai=5.0):
    n = int(dai * SR)
    t = np.arange(n) / SR
    fc = 250 + 1300 * (1 - np.exp(-t / 0.2)) * np.exp(-t / 1.3)
    out = np.zeros((2, n))
    for m in nots:
        for k in range(2):
            for v in range(3):
                f = midi(m) * 2 ** (rng.uniform(-14, 14) / 1200)
                pha = PI2 * f * t + rng.uniform(0, PI2)
                s = np.zeros(n)
                for h in range(1, 48):
                    if h * f > 4000: break
                    s += np.sin(h * pha) / h / (1 + (h * f / fc) ** 2)
                out[k] += s
    out = np.tanh(1.4 * out / (np.abs(out).max() + 1e-9))
    return out * bao(n, 0.03, 3.0)

def organ(nots, dai, sang=900):
    # Dan organ tram: ong dieu hoa 1,2,3,4,6,8 - hai kenh lech 0,15 Hz -> phach cham
    n = int(dai * SR)
    t = np.arange(n) / SR
    out = np.zeros((2, n))
    for m in nots:
        for k, lch in ((0, -0.15), (1, 0.15)):
            f0 = midi(m) + lch
            for h, a in ((1, 1.0), (2, 0.7), (3, 0.45), (4, 0.35), (6, 0.2), (8, 0.12)):
                if h * f0 > 4000: break
                out[k] += a / (1 + (h * f0 / sang) ** 2) * np.sin(PI2 * h * f0 * t + rng.uniform(0, PI2))
    return out * bao(n, dai * 0.3, dai * 0.3) / max(1, len(nots)) ** 0.5

def drone(goc, beat=0.2):
    # Nen tram lien tuc, tan so tron vong theo do dai bai de lap lien mach
    t = np.arange(L) / SR
    out = bo_dem()
    for m, a in ((goc, 0.6), (goc + 12, 0.5), (goc + 19, 0.25), (goc + 13, 0.12)):
        for k, lch in ((0, -beat), (1, beat)):
            f = round((midi(m) + lch) * DAI) / DAI
            for h, ah in ((1, 1.0), (2, 0.5), (3, 0.3), (4, 0.18)):
                out[k] += a * ah * np.sin(PI2 * h * f * t + rng.uniform(0, PI2))
    k = max(1, round(0.06 * DAI))
    song = 0.75 + 0.25 * np.sin(PI2 * k / DAI * t)
    return out * song

# ---------------------------------------------------------------- giai dieu + hoa am
CAU_1 = [50, 49, 50, 46, 49, 45, 46, 46]       # Re Do# Re Sib Do# La Sib Sib (Dies Irae)
CAU_2 = [53, 53, 55, 53, 52, 50, 49, 50]       # Fa Fa Sol Fa Mi Re Do# Re
VONG = [38, 34, 31, 33]                        # Re Sib Sol La (goc moi hop am)
CUM = {38: [62, 63, 65, 69], 34: [62, 65, 66, 70], 31: [62, 63, 67, 70], 33: [61, 64, 65, 70]}  # them not nghich tai

def goc_luc(g, cho=8.0):
    return VONG[int(g // cho) % 4]

# ---------------------------------------------------------------- ban day du kieu C (nguoi dung chon)
def chuan_hoat(x):
    # chuan theo do to LUC DANG KEU (lop chi keu vai giay khong bi day len qua muc)
    b = int(0.1 * SR)
    m = x.mean(axis=0)
    khoi = np.sqrt((m[:len(m) // b * b].reshape(-1, b) ** 2).mean(axis=1))
    hoat = khoi[khoi > 0.1 * khoi.max()] if khoi.max() > 0 else np.ones(1)
    return x / (np.sqrt((hoat ** 2).mean()) + 1e-12)

def theo_moc(diem):
    # duong muc to nho qua cac moc (giay, gia tri), tuan hoan dung L (noi cuoi -> dau)
    t = np.arange(L) / SR
    return np.interp(t, [d[0] for d in diem], [d[1] for d in diem], period=DAI)

def bai_C(che_do):
    # Muc tron giu dung nhu mau 40 giay nguoi dung da nghe; them bo cuc theo doan.
    lop = []
    def them(ten, x, mk, mg, duong=None):
        x = chuan_hoat(x)
        if duong is not None:
            x = x * duong
        lop.append((ten, x, mk, mg))
        print("  xong lop", ten, flush=True)

    if che_do == "sanh":
        # 128 s = 4 doan 32 s: A hop nhac + ren | B piano nguoc phong len, kim loai |
        # C cum day tram dang len, tim manh | D hop nhac CHAM DAN (het day cot), thua dan
        cho = 8.0
        them("drone", drone(38, 0.35), 0.08, 0.08)
        x = bo_dem()
        vt = 2.0
        i_cau = 0
        while vt < DAI - 3.5:
            nots = CAU_1 if i_cau % 2 == 0 else CAU_2
            for i, m in enumerate(nots):
                if vt >= DAI - 3.5: break
                cham = 1 + 0.5 * np.clip((vt - 96) / 30, 0, 1) ** 1.5      # doan D: hop nhac het day cot
                nang = 24 if not (32 <= vt < 64) else 12                    # doan B: xuong mot quang
                dat(x, vt, hop_nhac(m + nang, 3.0, 0.5 if i % 2 else -0.5))
                vt += 1.1 * cham * rng.uniform(0.96, 1.08)
            vt += 1.0 + (1.5 if i_cau % 2 else 0)
            i_cau += 1
        them("hop_nhac", x, 0.10, 0.5, theo_moc([(16, 1.0), (48, 0.75), (80, 0.8), (112, 1.0)]))
        x = bo_dem()
        for g in np.arange(cho - 3.0, DAI, cho):
            gc = goc_luc(g + 3.0, cho)
            for m in (gc + 12, gc + 19, gc + 25):
                dat(x, g - 0.5, piano(m, 2.6, 0.8, nguoc=True))
        them("piano_nguoc", x, 0.12, 0.45, theo_moc([(16, 0.6), (48, 1.0), (80, 1.0), (112, 0.45)]))
        x = bo_dem()
        for g in np.arange(64.0, 96.0, cho):
            gc = goc_luc(g, cho)
            dat(x, g, cum_day([gc + 12, gc + 13, gc + 19], cho + 3.0, truot=-50, sang=1100, len_=0.5, xuong=0.35))
        them("cum_day", x, 0.10, 0.4)
        x = bo_dem()
        for g in np.arange(5.0, DAI - 7, 12.0):
            m0 = rng.choice([72, 74, 76])
            dat(x, g, ren(m0, m0 - 2, 6.0, "u", rng.uniform(-0.7, 0.7), rng.uniform(-0.7, 0.7)))
        them("ren_tre", x, 0.035, 0.5)
        x = bo_dem()
        for g in np.arange(8.0, DAI - 6, 16.0):
            dat(x, g, kim_loai(rng.choice([70, 73]), 6.0, 40))
        them("kim_loai", x, 0.04, 0.5, theo_moc([(16, 0.7), (48, 1.2), (80, 1.0), (112, 0.8)]))
        x = bo_dem()
        for g, m in ((0.0, 50), (64.0, 45)):
            dat(x, g, chuong(m, 12.0, rng.uniform(-0.3, 0.3)))
        them("chuong", x, 0.05, 0.6)
        x = bo_dem()
        for g in np.arange(0, DAI - 0.5, 1.15 * DAI / (1.15 * round(DAI / 1.15))):
            dat(x, g, nhip_tim(0.8))
        them("tim", x, 0.28, 0.08, theo_moc([(16, 1.0), (48, 0.8), (80, 1.25), (112, 0.7)]))
        return lop

    # ---- trong tran: 90 nhip/phut, 72 o (2,667 s), 192 s
    # I 0-8 mo man | II 8-24 vao nhip | III 24-40 cao trao | IV 40-48 nghi | V 48-64 dang lai | VI 64-72 thua dan
    nhip = 60.0 / 90.0
    o = 4 * nhip
    so_o = int(round(DAI / o))
    cho = 2 * o
    def gb(oo, m16=0.0):
        return oo * o + m16 * nhip / 4
    def doan(*moc):
        return theo_moc([(gb(oo), v) for oo, v in moc])

    them("drone", drone(38, 0.35), 0.08, 0.08)
    x = bo_dem()
    for o0 in (0, 4, 8, 16, 40, 44, 48, 56, 64):
        vt = gb(o0)
        for nots in (CAU_1, CAU_2):
            for i, m in enumerate(nots):
                dat(x, vt, hop_nhac(m + 24, 3.0, 0.5 if i % 2 else -0.5))
                vt += nhip * (1.0 if o0 < 64 else 1.0 + 0.04 * i)
    them("hop_nhac", x, 0.10, 0.5, doan((0, 0.45), (20, 0.75), (44, 0.5), (60, 0.7), (70, 0.6)))
    x = bo_dem()
    for oo in range(1, so_o, 2):
        gc = goc_luc(gb(oo + 1), cho)
        for m in (gc + 12, gc + 19, gc + 25):
            dat(x, gb(oo + 1) - 3.0 - 0.5, piano(m, 2.6, 0.8, nguoc=True))
    them("piano_nguoc", x, 0.12, 0.45, doan((4, 0.7), (16, 1.0), (32, 0.75), (44, 0.7), (56, 0.9), (70, 0.6)))
    x = bo_dem()
    for g in (gb(2), gb(42), gb(66)):
        m0 = rng.choice([72, 74, 76])
        dat(x, g, ren(m0, m0 - 2, 6.0, "u", rng.uniform(-0.7, 0.7), rng.uniform(-0.7, 0.7)))
    them("ren_tre", x, 0.02, 0.3)
    x = bo_dem()
    for g in (gb(4), gb(44)):
        dat(x, g, kim_loai(rng.choice([70, 73]), 6.0, 40))
    them("kim_loai", x, 0.04, 0.5)
    x = bo_dem()
    for g, m, to in ((gb(0), 50, 0.6), (gb(24), 50, 1.0), (gb(48), 45, 1.0)):
        dat(x, g, chuong(m, 12.0, rng.uniform(-0.3, 0.3)) * to)
    them("chuong", x, 0.05, 0.6)
    x = bo_dem()
    for oo in list(range(0, 8)) + list(range(40, 48)) + list(range(68, 72)):
        for nh in (0, 2):
            dat(x, gb(oo, nh * 4), nhip_tim(0.8))
    them("tim", x, 0.22, 0.08)
    x = bo_dem()
    for oo in list(range(8, 40)) + list(range(48, 68)):
        if oo >= 64:
            vts = (0, 8) if oo < 66 else (0,)
        elif oo % 4 == 3 and (oo < 24 or 48 <= oo < 56):
            vts = (0, 6, 8, 10, 12, 13, 14, 15)
        elif 24 <= oo < 40 or 56 <= oo < 64:
            vts = (0, 3, 6, 8, 10) if oo % 4 != 3 else (0, 3, 6, 8, 10, 12, 13, 14, 15)
        else:
            vts = (0, 6, 8)
        for v in vts:
            manh = v in (0, 8)
            dat(x, gb(oo, v), trong(52 if manh else 60, 0.85 if manh else 0.55, 1.5, rng.uniform(-0.3, 0.3)))
    them("trong", x, 0.26, 0.22, doan((8, 1.1), (16, 1.3), (32, 1.35), (48, 1.15), (60, 1.35), (66, 0.7)))
    x = bo_dem()
    mau = [0, 0, 3, 0, 1, 0, -1, 0]
    for oo in list(range(8, 40)) + list(range(48, 68)):
        gc = goc_luc(gb(oo), cho) + 24
        for i, d in enumerate(mau):
            dat(x, gb(oo, i * 2), day_nay(gc + d, 1.0 if i in (0, 4) else 0.65))
    them("day_nay", x, 0.17, 0.25, doan((8, 1.0), (16, 1.3), (32, 1.35), (48, 1.1), (62, 1.3), (67, 0.6)))
    x = bo_dem()
    for oo in (24, 28, 32, 36, 56, 60):
        gc = goc_luc(gb(oo), cho)
        gc = gc if gc >= 36 else gc + 12
        dat(x, gb(oo), braam([gc, gc + 7, gc + 12], 5.0))
    them("braam", x, 0.10, 0.4)
    x = bo_dem()
    for oo in (26, 30, 34, 38, 58, 62):
        gc = goc_luc(gb(oo), cho)
        dat(x, gb(oo, 12), sum(piano(m, 1.2, 0.9) for m in (gc + 24, gc + 25, gc + 30, gc + 31)))
    them("piano_cum", x, 0.08, 0.35)
    x = bo_dem()
    for oo in range(24, 40, 4):
        dat(x, gb(oo), cum_day([74, 75, 77, 78], 4 * o, truot=200, sang=1500, len_=0.85, xuong=0.1))
    them("day_cang", x, 0.05, 0.35)
    return lop

# ---------------------------------------------------------------- bai (mau 40 giay A / B / C)
def bai(kieu, che_do):
    if kieu == "C" and DAI > 60:
        return bai_C(che_do)
    lop = []   # (ten, tin hieu (2,L), muc kho, muc gui vang)
    tran = che_do == "tran"
    nhip = 60.0 / 90.0 if tran else 1.0
    o = 4 * nhip
    so_o = int(DAI // o)
    cho = 8.0 if not tran else 2 * o

    def them(ten, x, mk, mg):
        b = int(0.1 * SR)
        m = x.mean(axis=0)
        khoi = np.sqrt((m[:len(m) // b * b].reshape(-1, b) ** 2).mean(axis=1))
        hoat = khoi[khoi > 0.1 * khoi.max()] if khoi.max() > 0 else np.ones(1)
        x = x / (np.sqrt((hoat ** 2).mean()) + 1e-12)
        lop.append((ten, x, mk, mg))
        print("  xong lop", ten, flush=True)

    goc0 = 38
    # -------- nen chung
    them("drone", drone(goc0 - 12 if kieu != "C" else goc0, 0.12 if kieu != "C" else 0.35), 0.08, 0.08)

    if kieu == "A":
        x = bo_dem()
        for g in np.arange(0, DAI, cho):
            gc = goc_luc(g, cho)
            dat(x, g, organ([gc + 12, gc + 19, gc + 24], cho + 3.0, 700))
        them("organ", x, 0.16, 0.35)
        x = bo_dem()
        for g in np.arange(0, DAI, cho):
            dat(x, g, dong_ca(CUM[goc_luc(g, cho)], cho + 3.5, "o"))
        them("dong_ca", x, 0.11, 0.55)
        x = bo_dem()
        for g in np.arange(0, DAI, 16.0 if not tran else 4 * o):
            dat(x, g, chuong(50 if int(g) % 32 < 16 else 45, 12.0, rng.uniform(-0.4, 0.4)))
        them("chuong", x, 0.12, 0.6)
        x = bo_dem()
        for g in np.arange(3.0, DAI - 7, 13.0):
            m0 = rng.choice([64, 67, 69]); dat(x, g, ren(m0, m0 - rng.choice([3, 5, 7]), 7.0, "u",
                                                        rng.uniform(-0.8, 0), rng.uniform(0, 0.8)))
        them("ren", x, 0.05, 0.5)
        if not tran:
            x = bo_dem()
            for i, m in enumerate(CAU_1 + CAU_2):
                if 4 + i * 2.0 + 3 < DAI:
                    dat(x, 4 + i * 2.0, dong_ca([m - 12], 2.4, "a", 0.15, 0.4, 2))
            them("ca_giai_dieu", x, 0.10, 0.4)

    elif kieu == "B":
        x = bo_dem()
        for g in np.arange(0, DAI, cho):
            gc = goc_luc(g, cho)
            dat(x, g, cum_day(CUM[gc], cho + 3.0, truot=rng.choice([-60, 60, -100]), sang=1300))
        them("cum_day", x, 0.14, 0.45)
        x = bo_dem()
        for g in np.arange(2.0, DAI - 6, 9.5):
            dat(x, g, kim_loai(rng.choice([74, 77, 79]), 6.0, 60))
        them("kim_loai", x, 0.06, 0.55)
        x = bo_dem()
        for g in np.arange(0, DAI, 16.0 if not tran else 4 * o):
            dat(x, g, chuong(45, 10.0, rng.uniform(-0.4, 0.4)))
        them("chuong", x, 0.07, 0.6)
        if not tran:
            x = bo_dem()
            for i, m in enumerate(CAU_1 + CAU_2):
                if 4 + i * 2.0 + 3 < DAI:
                    dat(x, 4 + i * 2.0, cum_day([m - 12], 2.5, 0, 1100, 0.2, 0.35))
            them("cello", x, 0.13, 0.35)

    else:  # C
        x = bo_dem()
        vt = 2.0
        for nots in (CAU_1, CAU_2, CAU_1, CAU_2):
            for i, m in enumerate(nots):
                if vt + 3 < DAI:
                    dat(x, vt, hop_nhac(m + 24, 3.0, 0.5 if i % 2 else -0.5))
                vt += (1.1 if not tran else nhip) * rng.uniform(0.96, 1.08 if not tran else 1.0)
            vt += 1.0 if not tran else 0
        them("hop_nhac", x, 0.10, 0.5)
        x = bo_dem()
        for g in np.arange(cho - 3.0, DAI, cho):
            gc = goc_luc(g + 3.0, cho)
            for m in (gc + 12, gc + 19, gc + 25):
                dat(x, g - 0.5, piano(m, 2.6, 0.8, nguoc=True))
        them("piano_nguoc", x, 0.12, 0.45)
        x = bo_dem()
        for g in np.arange(5.0, DAI - 7, 12.0):
            m0 = rng.choice([72, 74, 76]); dat(x, g, ren(m0, m0 - 2, 6.0, "u", rng.uniform(-0.7, 0.7), rng.uniform(-0.7, 0.7)))
        them("ren_tre", x, 0.035, 0.5)
        x = bo_dem()
        for g in np.arange(8.0, DAI - 6, 16.0):
            dat(x, g, kim_loai(rng.choice([70, 73]), 6.0, 40))
        them("kim_loai", x, 0.04, 0.5)

    # -------- nhip tim (sanh: cham, ca bai; tran: doan nghi)
    x = bo_dem()
    if not tran:
        for g in np.arange(0, DAI, 1.15):
            dat(x, g, nhip_tim(0.8))
        them("tim", x, 0.13 if kieu != "A" else 0.09, 0.08)

    if tran:
        # trong: nhip (0, 6, 8) / 16 + doan don cuoi moi 8 o; day nay chromatic; braam moi 8 o
        def gb(oo, m16=0.0):
            return oo * o + m16 * nhip / 4
        x = bo_dem()
        for oo in range(so_o):
            for v in (0, 6, 8) if oo % 4 != 3 else (0, 3, 6, 8, 10, 12, 13, 14, 15):
                dat(x, gb(oo, v), trong(52 if v in (0, 8) else 60, 0.85 if v in (0, 8) else 0.55, 1.5, rng.uniform(-0.3, 0.3)))
        them("trong", x, 0.26, 0.22)
        x = bo_dem()
        mau = [0, 0, 3, 0, 1, 0, -1, 0]
        for oo in range(so_o):
            gc = goc_luc(gb(oo), cho) + (12 if kieu != "C" else 24)
            for i, d in enumerate(mau):
                dat(x, gb(oo, i * 2), day_nay(gc + d, 1.0 if i in (0, 4) else 0.65))
        them("day_nay", x, 0.17 if kieu != "A" else 0.12, 0.25)
        x = bo_dem()
        for oo in range(0, so_o, 4 if kieu == "B" else 8):
            gc = goc_luc(gb(oo), cho)
            gc = gc if gc >= 36 else gc + 12
            dat(x, gb(oo), braam([gc, gc + 7, gc + 12], 5.0))
        them("braam", x, 0.15 if kieu == "B" else 0.10, 0.4)
        if kieu == "A":
            x = bo_dem()
            for i, m in enumerate(CAU_1 + CAU_2):
                if gb(4) + i * 2 * nhip + 2 < DAI:
                    dat(x, gb(4) + i * 2 * nhip, dong_ca([m + 12], 2 * nhip * 1.1, "a", 0.05, 0.3, 3))
            them("ca_tung", x, 0.12, 0.45)
        if kieu == "B":
            x = bo_dem()
            for oo in range(0, so_o, 4):
                dat(x, gb(oo), cum_day([74, 75, 77, 78], 4 * o, truot=200, sang=1600, len_=0.85, xuong=0.1))
            them("day_cang", x, 0.06, 0.35)
        if kieu == "C":
            x = bo_dem()
            for oo in range(2, so_o, 4):
                gc = goc_luc(gb(oo), cho)
                s = sum(piano(m, 1.2, 0.9) for m in (gc + 24, gc + 25, gc + 30, gc + 31))
                dat(x, gb(oo, 12), s)
            them("piano_cum", x, 0.08, 0.35)
    return lop

# ---------------------------------------------------------------- vang + tron
def phan_hoi_vang(rt_thap=4.5, rt_giua=3.6, rt_cao=1.2, dai=6.0):
    # IR vang: tiep am (dai cao tat nhanh -> khong de lai "xi" o dai cao), loc thong thap 4 kHz
    n = int(dai * SR)
    t = np.arange(n) / SR
    ir = np.zeros((2, n))
    for k in range(2):
        z = rng.standard_normal(n)
        thap = loc(z, lambda f: 1 / (1 + (f / 300) ** 4))
        cao = loc(z, lambda f: (f / 2500) ** 4 / (1 + (f / 2500) ** 4))
        giua = z - thap - cao
        for b, rt in ((thap, rt_thap), (giua, rt_giua), (cao, rt_cao)):
            ir[k] += b * np.exp(-6.9 * t / rt)
        ir[k, :int(0.03 * SR)] = 0
    ir = loc(ir, lambda f: 1 / np.sqrt(1 + (f / 4000) ** 4))
    return ir / np.sqrt((ir ** 2).sum(axis=1, keepdims=True))

def vang_vong(x, ir):
    out = np.zeros_like(x)
    for k in range(2):
        h = np.zeros(L); h[:ir.shape[1]] = ir[k]
        out[k] = np.fft.irfft(np.fft.rfft(x[k]) * np.fft.rfft(h), n=L)
    return out

def gioi_han(x, tran=0.89, khoi=256):
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

def chinh():
    kieu = sys.argv[1] if len(sys.argv) > 1 else "A"
    che_do = sys.argv[2] if len(sys.argv) > 2 else "sanh"
    giay = float(sys.argv[3]) if len(sys.argv) > 3 else 40.0
    duong_ra = sys.argv[4] if len(sys.argv) > 4 else "CongCu/AmThanh/NhacMoiThu/%s_%s.wav" % (kieu, che_do)
    hat = int(sys.argv[5]) if len(sys.argv) > 5 else 1510
    dat_do_dai(giay, hat)
    kho, gui = bo_dem(), bo_dem()
    for ten, x, mk, mg in bai(kieu, che_do):
        kho += x * mk
        gui += x * mg
    tron = kho + 0.8 * vang_vong(gui, phan_hoi_vang())
    # EQ: cat < 120 Hz (bac 4), ha nhe < 200 Hz, nang nhe 400 Hz - 1,5 kHz, thong thap 4,5 kHz (bac 4)
    tron = loc(tron, lambda f: ((f / 120) ** 4 / (1 + (f / 120) ** 4))
               * (0.9 + 0.1 * (f / 220) ** 2 / (1 + (f / 220) ** 2))
               * (1 + 0.35 * dai_qua(800, 1.0)(f))
               / np.sqrt(1 + (f / 4500) ** 8))
    # do rong stereo: tram hep, giua / cao rong
    giua = (tron[0] + tron[1]) / 2
    ben = loc((tron[0] - tron[1]) / 2, lambda f: 0.4 + 0.5 * (f / 250) ** 2 / (1 + (f / 250) ** 2))
    tron = np.vstack([giua + ben, giua - ben])
    tron *= 10 ** (-20 / 20) / np.sqrt((tron ** 2).mean())
    tron = gioi_han(tron)
    dinh = np.abs(tron).max()
    if dinh > 0.89:
        tron *= 0.89 / dinh
    pcm = (np.clip(tron, -1, 1) * 32767).astype("<i2").T.copy()
    with wave.open(duong_ra, "wb") as w:
        w.setnchannels(2); w.setsampwidth(2); w.setframerate(SR)
        w.writeframes(pcm.tobytes())
    print("da ghi", duong_ra)

if __name__ == "__main__":
    chinh()
