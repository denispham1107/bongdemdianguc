# -*- coding: utf-8 -*-
# TIENG TANG HINH (16/10/2026, nguoi dung): "them am thanh stereo cho skill Tang hinh - luc co the chuyen qua TRONG SUOT, luc tu tang
# hinh chuyen qua BINH THUONG, luc VONG TRON SAT THUONG xuat hien, va neu TRUNG ke dich thi co tieng GAY SAT THUONG". Nguoi choi
# khac trong suong mu thi khong nghe (TiengViTri lo).
# Bon tieng: an (bien trong suot ~1,6 s) · hien (hien hinh ~1,3 s) · vong (vong phep 5 m hien 0,18 s, giu, tat toi 1,9 s -> ~2,4 s)
# · trung (moi ke trung vong phep ~0,8 s). Hien + vong + trung xay ra CUNG LUC (TangHinh.Tat -> NoVong).
#
# Lam theo memory quy-trinh-tieng-ky-nang-moi NGAY TU BAN DAU:
#   - moi lop sinh RIENG tung kenh + ~40-50% chung; su kien nho (hat bui, manh) moi cai mot cho, lech CHI bang am luong (khong tre
#     giua hai kenh - am co cao do lech pha -> tuong quan am); am tram hai kenh CUNG PHA (b2.um_chung / bum_chung);
#   - KHONG "piu" truot cao do nhanh (= sung laser), KHONG ngan dai nhieu phan am khong dieu hoa (= thuy tinh), KHONG dung lai lop lua
#     (liem lua, gio xe) hay "mua da" cua Qua cau bang (tru mau B co chu y he BANG);
#   - 3 bien the moi tieng KHAC CAU TRUC; dai dien thoai: < 200 Hz <= ~25%, cat < 120 Hz, > 4 kHz <= ~10%, 200 Hz - 1 kHz >= ~25%.
#
# Ba MAU:
#   A "bong_ma"  - hoi tho hut vao + tieng thi tham + than tan thanh bui sang; hien hinh = bui tu lai, tho ra "phu"; vong = "ommm" nghi
#                  le + dong ca khe xoay quanh; trung = nhat chem "xoet" + "huych".
#   B "bang_suong" - suong gia bo phu than (lach tach min) roi tan; hien hinh = vo suong nut "rac" bong ra; vong = song bang lan ra
#                  hai ben + hoi lanh xoay; trung = manh bang dap "toc rac". (he BANG cua Tang hinh - nhung de gan Qua cau bang)
#   C "hu_khong" - khong gian bi hut (luong khi xoay pha "vuuu" dong lai) + "up"; hien hinh = "phup" ap suat bung ra; vong = song xung
#                  kich + khi cuon xoay quanh; trung = "phap" ap suat + vet khi.
#
# Chay: python CongCu/AmThanh/tieng_tang_hinh.py [thu_muc_ra]   (mac dinh CongCu/AmThanh/TangHinhThu)

import os, sys
import numpy as np
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import tieng_bam as tb
import tieng_qua_cau_lua_3 as q3
import tieng_qua_cau_bang_2 as b2
import tieng_qua_cau_bang_3 as b3

SR = tb.SR
PI2 = tb.PI2
loc, dai, thap, cao, cham = q3.loc, q3.dai, q3.thap, q3.cao, q3.cham


# ------------------------------------------------------------------ cong cu chung
def on2(n, r, chung=0.5):
    """On hai kenh: phan chung + phan rieng (tuong quan ~ chung^2)."""
    zc = r.standard_normal(n)
    return np.stack([chung * zc + np.sqrt(1 - chung ** 2) * r.standard_normal(n) for _ in range(2)], axis=1)


def quet(x, fc_t, q=1.5, so=16, kieu='dai'):
    """Loc QUET theo thoi gian (tren ON - khong phai cao do truot): ngan bo loc co dinh, noi suy theo log tan so."""
    lo, hi = max(40.0, fc_t.min() * 0.85), fc_t.max() * 1.15
    tams = np.geomspace(lo, hi, so)
    if kieu == 'dai':
        bank = np.stack([loc(x, dai(fc, q)) for fc in tams])
    else:
        bank = np.stack([loc(x, thap(fc, 2)) for fc in tams])
    pos = np.interp(np.log(fc_t), np.log(tams), np.arange(so))
    i0 = np.clip(np.floor(pos).astype(int), 0, so - 2)
    w = pos - i0
    idx = np.arange(len(x))
    return bank[i0, idx] * (1 - w) + bank[i0 + 1, idx] * w


def quet2(x2, fc_t, q=1.5, kieu='dai', lech=0.03):
    """quet cho hai kenh, kenh phai tan so lech nhe (khong cung mot cong huong)."""
    return np.stack([quet(x2[:, k], fc_t * (1 + lech * (k - 0.5)), q, kieu=kieu) for k in range(2)], axis=1)


def xoay(x, hz, sau, r, pha=None):
    """Quay ca lop trai <-> phai bang AM LUONG (bao nguoc nhau hai kenh)."""
    t = np.arange(len(x)) / SR
    s = np.sin(PI2 * hz * t + (r.uniform(0, PI2) if pha is None else pha))
    y = x.copy()
    y[:, 0] *= 1 - sau * (0.5 + 0.5 * s)
    y[:, 1] *= 1 - sau * (0.5 - 0.5 * s)
    return y


def loc2(x2, h):
    """Loc CUNG mot bo loc cho hai kenh cua on2 (giu phan chung). ⚠️ Lan 1-4 viet loc(on2(...)[:, k]) trong vong lap kenh: moi kenh
    goi on2 MOI -> hai nguon ON DOC LAP (tuong quan ~0, vong C 0,03)."""
    return np.stack([loc(x2[:, k], h) for k in range(2)], axis=1)


def keo_lech(x, p):
    """Dat lech trai/phai (p mang hoac so -1..1) bang am luong (equal power)."""
    g = (np.clip(p, -1, 1) + 1) * np.pi / 4
    y = x.copy()
    y[:, 0] *= np.cos(g) * np.sqrt(2)
    y[:, 1] *= np.sin(g) * np.sqrt(2)
    return y


def hat_bui(n, r, mat_do_t, f=(1400, 3400), rong_t=None, nang=1.0):
    """Hat bui sang li ti (tan thanh bui / tu lai): moi hat 3-9 ms on loc dai, moi hat mot cho - lech CHI bang am luong.
    mat_do_t: so hat / giay theo thoi gian; rong_t: do toa trai phai (0..1) theo thoi gian."""
    ra = np.zeros((n, 2))
    p = mat_do_t / SR
    vt = np.nonzero(r.random(n) < p)[0]
    nhan = []
    for _ in range(4):        # 4 hat mau (khac tan so) - tranh moi hat giong nhau
        d = int(r.uniform(0.003, 0.009) * SR)
        fc = np.exp(r.uniform(np.log(f[0]), np.log(f[1])))
        k = loc(np.pad(r.standard_normal(d) * np.hanning(d), (0, 512)), dai(fc, 1.6))
        nhan.append(k / (np.max(np.abs(k)) + 1e-12))
    for i in vt:
        k = nhan[r.integers(0, 4)]
        a = np.exp(r.uniform(np.log(0.15), 0))
        rr = 1.0 if rong_t is None else rong_t[i]
        g = (np.clip(r.uniform(-1, 1) * rr, -1, 1) + 1) * np.pi / 4
        e = min(len(k), n - i)
        ra[i:i + e, 0] += k[:e] * a * np.cos(g)
        ra[i:i + e, 1] += k[:e] * a * np.sin(g)
    return ra * nang


def thi_tham(n, r, nguyen_am, bao, chung=0.35):
    """Tieng THI THAM / hoi tho: on qua 3 formant (nguyen am) - khong co cao do. nguyen_am: [(F1, F2, F3), ...] doi dan theo thoi gian."""
    t = np.arange(n) / SR
    x2 = on2(n, r, chung)
    so = len(nguyen_am)
    vt = np.linspace(0, so - 1, n)
    ra = np.zeros((n, 2))
    for j, (q, a) in enumerate(((5.0, 1.0), (7.0, 0.6), (9.0, 0.3))):
        fc = np.interp(vt, np.arange(so), [na[j] for na in nguyen_am])
        ra += quet2(x2, fc, q, lech=0.04) * a
    ra += np.stack([loc(x2[:, k], lambda f: cao(1800, 2)(f) * thap(3800, 4)(f)) for k in range(2)], axis=1) * 0.25   # hoi "h"
    ra /= np.max(np.abs(ra)) + 1e-12
    return ra * bao[:, None]


def ommm(t, r, f0, bao, lech_kenh=0.006):
    """"Ommm" nghi le: mot giong tram DIEU HOA (khong truot, khong ngan kim loai), hai kenh lech cao do 0,6% (phach cham), qua formant 'o'."""
    n = len(t)
    ra = np.zeros((n, 2))
    pha0 = r.uniform(0, PI2, 8)
    rung = 1 + 0.004 * cham(n, 4, r)
    for k in range(2):
        f = f0 * (1 + lech_kenh * (k - 0.5)) * rung
        pha = PI2 * np.cumsum(f) / SR
        x = sum(np.sin(h * pha + pha0[h - 1]) / h ** 0.9 for h in range(1, 8))
        ra[:, k] = loc(x, lambda ff: dai(450, 2.0)(ff) + 0.7 * dai(800, 3.0)(ff) + 0.15)
    ra /= np.max(np.abs(ra)) + 1e-12
    return ra * bao[:, None]


def flanger(x, d_t, muc=0.75):
    """Khi xoay pha (jet): cong ban tre thay doi d_t (giay) - tren ON nghe nhu luong khi bi hut, khong co cao do."""
    n = len(x)
    idx = np.arange(n)
    out = x.copy()
    for k in range(x.shape[1]):
        dk = d_t       # (do lan 1: tre lech 15% giua hai kenh -> tuong quan AM -0,05..-0,14; nay cung tre)
        out[:, k] = x[:, k] + muc * np.interp(idx - dk * SR, idx, x[:, k], left=0)
    return out


def bao_len_xuong(t, len_, tat, bd=0.0):
    tt = t - bd
    return np.clip(tt / max(len_, 1e-4), 0, 1) * np.exp(-np.maximum(tt - len_, 0) / tat) * (tt >= 0)


def vang(tron, r, giay, muc):
    return q3.vang_uot(tron, giay, r, muc)


def hoan_tat(tron, r, giay_vang, muc_vang, dinh_db):
    return q3.chuan(vang(tron, r, giay_vang, muc_vang), dinh_db)


# ================================================================== MAU A "BONG MA"
NGUYEN_AM = {'u': (320, 800, 2300), 'o': (480, 950, 2500), 'a': (720, 1200, 2600), 'e': (520, 1750, 2550), 'h': (900, 1600, 2700)}


def A_an(hat):
    r = np.random.default_rng(hat)
    hut = r.uniform(0.45, 0.85)                      # thoi diem het "hut vao"
    n = int(SR * (hut + 1.2)); t = np.arange(n) / SR
    tron = np.zeros((n, 2))
    # hoi hut vao: on qua dai QUET len 450 -> 1,6-2,6 kHz, to dan, cat nhanh o 'hut'
    # (do lan 1: 450 -> 2,6 kHz cho 200 Hz-1 kHz chi 7-9%, > 4 kHz 11-13% - mong, choi; nay 280 -> 1,1-1,9 kHz + hoi tho tram hon)
    fc = 280 * (r.uniform(1100, 1900) / 280) ** np.clip(t / hut, 0, 1)
    bh = np.clip(t / hut, 0, 1) ** r.uniform(1.6, 2.6) * np.exp(-np.maximum(t - hut, 0) / 0.05)
    tron += quet2(on2(n, r, 0.5), fc, 1.2) * bh[:, None] * 1.1
    # thi tham nguoc (to dan) - nguyen am ngau nhien
    ds = list(NGUYEN_AM.values())
    na = [ds[i] for i in r.choice(len(ds), int(r.integers(2, 4)), replace=False)]
    tron += thi_tham(n, r, na, bh * r.uniform(0.6, 1.0))
    tron += thi_tham(n, r, [NGUYEN_AM['u'], NGUYEN_AM['o']], bh * r.uniform(0.4, 0.7))
    # than tan thanh BUI: hat sang toa dan ra hai ben, day nhat quanh 'hut' roi thua dan
    md = r.uniform(250, 500) * np.exp(-((t - hut * r.uniform(0.85, 1.1)) / r.uniform(0.25, 0.4)) ** 2)
    tron += hat_bui(n, r, md, (900, 2600), np.clip(0.3 + t / (hut + 0.6), 0, 1), r.uniform(0.2, 0.45))
    # "vut" nho luc bien mat (khong phai bien the nao cung co)
    if r.random() < 0.75:
        tt = np.maximum(t - hut, 0)
        tron += b2.um_chung(tt, r, r.uniform(240, 300), r.uniform(150, 190), r.uniform(0.04, 0.08)) * (t >= hut)[:, None] * r.uniform(0.25, 0.45)
    # duoi: khong khi rong lang lai
    tron += thi_tham(n, r, [NGUYEN_AM['h'], NGUYEN_AM['u']], bao_len_xuong(t, 0.05, r.uniform(0.25, 0.5), hut) * 0.25)
    tron = keo_lech(tron, np.linspace(0, r.uniform(-0.3, 0.3), n))
    return hoan_tat(tron, r, r.uniform(1.0, 1.5), r.uniform(0.35, 0.55), -4)


def A_hien(hat):
    r = np.random.default_rng(hat)
    tu = r.uniform(0.2, 0.4)                         # bui tu lai xong -> tho ra
    n = int(SR * (tu + 1.0)); t = np.arange(n) / SR
    tron = np.zeros((n, 2))
    md = r.uniform(250, 500) * np.clip(t / tu, 0, 1) ** 1.5 * (t < tu + 0.03)
    tron += hat_bui(n, r, md, (1300, 3300), np.clip(1.0 - t / (tu * 1.2), 0.15, 1), r.uniform(0.4, 0.6))   # bui TU VAO giua
    # tho ra "phu": dai quet XUONG 2,2 kHz -> 450 Hz, bung o 'tu' roi tat
    fc = np.where(t < tu, 2200, 450 + 1750 * np.exp(-np.maximum(t - tu, 0) / r.uniform(0.12, 0.25)))
    bt = bao_len_xuong(t, 0.02, r.uniform(0.15, 0.3), tu)
    tron += quet2(on2(n, r, 0.5), fc, 1.2) * bt[:, None] * 1.1
    tron += b2.bum_chung(np.maximum(t - tu, 0), r, r.uniform(170, 220), r.uniform(105, 125), r.uniform(0.06, 0.1), 0.8) \
        * (t >= tu)[:, None] * r.uniform(0.3, 0.5)
    ds = list(NGUYEN_AM.values())
    tron += thi_tham(n, r, [ds[r.integers(0, 5)], NGUYEN_AM['h']], bt * r.uniform(0.3, 0.55))
    tron = keo_lech(tron, np.linspace(r.uniform(-0.3, 0.3), 0, n))
    return hoan_tat(tron, r, r.uniform(0.9, 1.3), r.uniform(0.3, 0.5), -4)


def A_vong(hat):
    r = np.random.default_rng(hat)
    n = int(SR * 2.3); t = np.arange(n) / SR
    tron = np.zeros((n, 2))
    bv = np.clip(t / 0.18, 0, 1) * np.clip((1.9 - t) / 0.9, 0, 1) ** 1.3
    het = r.uniform(1.6, 2.1)
    bv = np.clip(t / 0.18, 0, 1) * np.clip((het - t) / 0.9, 0, 1) ** 1.3 * np.maximum(cham(n, 1.2, r) * 0.25 + 1, 0.4)
    bung = r.uniform(0.4, 1.6)
    tron += b2.bum_chung(t, r, r.uniform(190, 240), r.uniform(105, 120), r.uniform(0.09, 0.15), 1.0) * 0.4 * bung
    # "phut" luc vong bung ra
    tron += quet2(on2(n, r, 0.45), 600 + 1800 * np.exp(-t / 0.08), 1.0) * bao_len_xuong(t, 0.01, 0.12)[:, None] * 0.8 * bung
    if r.random() < 0.6:      # nhip thu hai (vong "dap" them mot lan)
        bd2 = r.uniform(0.45, 1.0)
        tron += quet2(on2(n, r, 0.45), 500 + 1400 * np.exp(-np.maximum(t - bd2, 0) / 0.1), 1.0) * bao_len_xuong(t, 0.03, 0.2, bd2)[:, None] * r.uniform(0.4, 0.7)
    # dong ca khe "aaah / ooo" XOAY quanh (vong phep xoay cham)
    ds = list(NGUYEN_AM.values())
    for _ in range(int(r.integers(2, 4))):
        na = [ds[i] for i in r.choice(len(ds), 2, replace=False)]
        tron += xoay(thi_tham(n, r, na, bv * r.uniform(0.3, 0.55)), r.uniform(0.35, 0.7), 0.7, r)
    tron += ommm(t, r, r.choice([110.0, 123.5, 130.8]) * r.uniform(0.98, 1.02), bv) * r.uniform(0.12, 0.45)
    # hat sang bay len trong vong
    tron += hat_bui(n, r, r.uniform(80, 160) * bv, (1500, 3400), None, 0.35)
    return hoan_tat(tron, r, r.uniform(1.3, 1.8), r.uniform(0.4, 0.55), -2)


def A_trung(hat):
    r = np.random.default_rng(hat)
    n = int(SR * 0.6); t = np.arange(n) / SR
    tron = np.zeros((n, 2))
    so = int(r.choice([1, 1, 2, 3]))
    for j in range(so):
        bd = j * r.uniform(0.05, 0.09)
        nhanh = r.uniform(0.04, 0.08)
        # (do lan 2: 700 + 1,8-2,6 kHz -> 200 Hz-1 kHz 16-26%, > 4 kHz 10-12%; nay quet thap hon)
        fc = 380 + r.uniform(900, 1600) * np.exp(-np.maximum(t - bd, 0) / nhanh)
        x = quet2(on2(n, r, 0.5), fc, 1.4) * bao_len_xuong(t, 0.008, nhanh * 1.2, bd)[:, None]
        tron += keo_lech(x, np.linspace(r.uniform(-0.6, 0.6), r.uniform(-0.6, 0.6), n)) * (1.0 if j == 0 else 0.6)
    # (do lan 1: < 200 Hz 30-34% - bum qua to)
    tron += b2.bum_chung(t, r, r.uniform(220, 280), r.uniform(125, 145), r.uniform(0.02, 0.05), 1.0) * r.uniform(0.15, 0.35)
    tron += b3.toc(t, r, 0.004, r.uniform(0.5, 1.0), (220, 800), r.uniform(0.02, 0.04))
    tron += thi_tham(n, r, [NGUYEN_AM['a'], NGUYEN_AM['h']], bao_len_xuong(t, 0.02, r.uniform(0.05, 0.25), 0.02) * r.uniform(0.15, 0.45))
    return hoan_tat(tron, r, r.uniform(0.5, 0.8), r.uniform(0.25, 0.4), -4)


# ================================================================== MAU B "BANG SUONG"
def B_an(hat):
    r = np.random.default_rng(hat)
    phu = r.uniform(0.5, 0.9)                        # suong phu kin than
    n = int(SR * (phu + 1.0)); t = np.arange(n) / SR
    tron = np.zeros((n, 2))
    bphu = np.clip(t / phu, 0, 1) ** 1.5 * np.exp(-np.maximum(t - phu, 0) / r.uniform(0.12, 0.25))
    tron += b2.dong_bang(n, r, r.uniform(250, 450), bao_mat=bphu + 0.02) * 1.4
    tron += b3.xi_xoay(n, r, 1200, 2700, quay_hz=r.uniform(1, 2.5)) * bphu[:, None] * 0.5
    tron += b3.xi_xoay(n, r, 350, 1000, quay_hz=r.uniform(0.6, 1.5)) * bphu[:, None] * 0.6
    tron += b2.vun_bang(t, r, phu * r.uniform(0.8, 1.0), r.uniform(0.05, 0.1), r.uniform(300, 600), (500, 1800), nang=0.45)
    tron += hat_bui(n, r, r.uniform(150, 300) * np.exp(-((t - phu) / 0.3) ** 2), (1300, 3300), None, 0.3)
    return hoan_tat(tron, r, r.uniform(1.0, 1.5), r.uniform(0.35, 0.5), -4)


def B_hien(hat):
    r = np.random.default_rng(hat)
    tk = r.uniform(0.08, 0.2)
    n = int(SR * (tk + 1.0)); t = np.arange(n) / SR
    tron = np.zeros((n, 2))
    if r.random() < 0.7:
        tron += b2.ken_ket(t, r, 0.0, tk, r.uniform(300, 480)) * r.uniform(0.25, 0.5)
    tron += b3.toc(t, r, tk, r.uniform(0.5, 0.7), (180, 800), 0.03)
    tron += b2.vun_bang(t, r, tk, r.uniform(0.12, 0.22), r.uniform(700, 1200), (600, 2400), nang=0.9)
    tron += b2.vun_bang(t, r, tk + 0.003, r.uniform(0.1, 0.2), r.uniform(400, 700), (350, 1200), nang=0.7, chung=0.45)
    b2.cuc_roi(tron, r, int(r.integers(2, 12)), tk + 0.02, tk + r.uniform(0.25, 0.7), 400, 1400, nang=r.uniform(0.3, 0.7), nay=(1, 2))
    tron += b3.phut_lanh(n, r, bao_len_xuong(t, 0.01, r.uniform(0.15, 0.3), tk), 900, 2600) * 0.5
    return hoan_tat(tron, r, r.uniform(0.9, 1.3), r.uniform(0.35, 0.5), -4)


def B_vong(hat):
    r = np.random.default_rng(hat)
    n = int(SR * 2.3); t = np.arange(n) / SR
    tron = np.zeros((n, 2))
    bv = np.clip(t / 0.18, 0, 1) * np.clip((1.9 - t) / 0.9, 0, 1) ** 1.3
    het = r.uniform(1.6, 2.1)
    bv = np.clip(t / 0.18, 0, 1) * np.clip((het - t) / 0.9, 0, 1) ** 1.3 * np.maximum(cham(n, 1.2, r) * 0.25 + 1, 0.4)
    bung = r.uniform(0.6, 1.3)
    tron += b3.toc(t, r, 0.0, bung, (150, 700), 0.045)
    tron += b2.bum_chung(t, r, r.uniform(190, 230), r.uniform(105, 120), r.uniform(0.08, 0.12), 0.9) * 0.4 * bung
    if r.random() < 0.6:      # dot nut thu hai lan ra
        bd2 = r.uniform(0.3, 0.8)
        tron += b2.vun_bang(t, r, bd2, r.uniform(0.08, 0.2), r.uniform(400, 800), (500, 1800), nang=r.uniform(0.3, 0.55))
    # song bang LAN RA: lach tach suong gia, gan giua luc dau roi toa ra hai ben
    rong = np.clip(0.1 + t / 0.6, 0, 1)
    tron += hat_bui(n, r, r.uniform(300, 900) * bao_len_xuong(t, 0.05, r.uniform(0.3, 0.8)), (1200, 3200), rong, 0.6)
    tron += b2.vun_bang(t, r, 0.01, r.uniform(0.2, 0.35), r.uniform(500, 900), (450, 1600), nang=0.6)
    tron += xoay(b3.xi_xoay(n, r, 1100, 2600, quay_hz=1.0), r.uniform(0.4, 0.7), 0.6, r) * bv[:, None] * 0.5
    tron += xoay(b3.xi_xoay(n, r, 320, 950, quay_hz=0.7), r.uniform(0.3, 0.6), 0.5, r) * bv[:, None] * 0.6
    tron += b2.dong_bang(n, r, 120, bao_mat=bv + 0.01) * 0.8
    return hoan_tat(tron, r, r.uniform(1.3, 1.8), r.uniform(0.4, 0.55), -2)


def B_trung(hat):
    r = np.random.default_rng(hat)
    n = int(SR * 0.6); t = np.arange(n) / SR
    tron = np.zeros((n, 2))
    # (do lan 1: < 200 Hz 30-34%, tuong quan 0,34-0,43 - toc 60% chung qua to)
    tron += b3.toc(t, r, 0.0, r.uniform(0.5, 0.8), (220, 900), r.uniform(0.015, 0.03))
    tron += b2.bum_chung(t, r, r.uniform(220, 270), r.uniform(125, 140), r.uniform(0.02, 0.04), 0.8) * r.uniform(0.1, 0.25)
    tron += b2.vun_bang(t, r, 0.002, r.uniform(0.03, 0.15), r.uniform(700, 1200), (700, 2400), nang=r.uniform(0.6, 1.0), chung=0.25)
    b2.cuc_roi(tron, r, int(r.integers(1, 6)), 0.02, r.uniform(0.12, 0.35), 450, 1300, nang=0.45, nay=(1, 2))
    if r.random() < 0.4:
        bd2 = r.uniform(0.04, 0.1)
        tron += keo_lech(b3.toc(t, r, bd2, 0.4, (300, 1100), 0.015), r.uniform(-0.8, 0.8))
    tron += b3.phut_lanh(n, r, bao_len_xuong(t, 0.005, r.uniform(0.06, 0.12)), 900, 2600) * 0.35
    return hoan_tat(tron, r, r.uniform(0.5, 0.8), r.uniform(0.35, 0.5), -4)


# ================================================================== MAU C "HU KHONG"
def C_an(hat):
    r = np.random.default_rng(hat)
    hut = r.uniform(0.4, 1.0)
    n = int(SR * (hut + 1.1)); t = np.arange(n) / SR
    tron = np.zeros((n, 2))
    x = on2(n, r, 0.65)       # (do lan 2: chung 0,5 -> tuong quan -0,01..0,13)
    # luong khi bi hut: tre xoay pha 0,6 ms -> 7-10 ms (khi "vuuu" tram dan), thong thap DONG dan 4 kHz -> 400 Hz
    d = 0.0006 + r.uniform(0.007, 0.010) * np.clip(t / hut, 0, 1) ** 1.3
    x = flanger(x, d, 0.8)
    fc = 3800 * (450 / 3800) ** np.clip(t / (hut * 1.1), 0, 1)
    x = quet2(x, fc, kieu='thap')
    bh = np.clip(t / hut, 0, 1) ** 2.0 * np.exp(-np.maximum(t - hut, 0) / 0.03)
    tron += x * bh[:, None] * 1.2
    if r.random() < 0.75:
        tron += b2.bum_chung(np.maximum(t - hut, 0), r, r.uniform(200, 240), r.uniform(110, 125), r.uniform(0.06, 0.1), 1.0) \
            * (t >= hut)[:, None] * r.uniform(0.3, 0.6)
    # du am "hu khong": luong tram 200-500 Hz rung nhe, tat dan (do lan 1: Q 2,5 -> do phang duoi 0,07 = vo vo co cao do; nay Q 1,1)
    hk = loc2(on2(n, r, 0.5), dai(r.uniform(280, 420), 1.1))
    tron += xoay(hk, r.uniform(0.8, 1.6), 0.5, r) * bao_len_xuong(t, 0.03, r.uniform(0.15, 0.7), hut)[:, None] * r.uniform(0.25, 0.55)
    return hoan_tat(tron, r, r.uniform(1.0, 1.4), r.uniform(0.35, 0.5), -4)


def C_hien(hat):
    r = np.random.default_rng(hat)
    # bien the khac CAU TRUC (do lan 2: chenh 1,1-1,8 dB): co / khong doan HUT NGUOC truoc "phup", do dai tat 0,1-0,5 s
    truoc = r.uniform(0.1, 0.28) if r.random() < 0.6 else 0.0
    n = int(SR * (1.2 + truoc)); t = np.arange(n) / SR
    tron = np.zeros((n, 2))
    if truoc > 0:
        bh = np.clip(t / truoc, 0, 1) ** 2.5 * (t < truoc)
        x = flanger(on2(n, r, 0.6), 0.0006 + 0.006 * np.clip(t / truoc, 0, 1), 0.8)
        tron += quet2(x, 500 + 1800 * np.clip(t / truoc, 0, 1), kieu='thap') * bh[:, None] * r.uniform(0.4, 0.8)
    t0 = np.maximum(t - truoc, 0); co = (t >= truoc)[:, None]
    tron += b2.bum_chung(t0, r, r.uniform(230, 270), r.uniform(118, 132), r.uniform(0.05, 0.1), 1.1) * co * r.uniform(0.3, 0.5)
    if r.random() < 0.5:      # "phup" nho thu hai
        bd2 = truoc + r.uniform(0.08, 0.2)
        tron += quet2(on2(n, r, 0.5), 400 + 1600 * np.exp(-np.maximum(t - bd2, 0) / 0.06), kieu='thap') * bao_len_xuong(t, 0.004, 0.08, bd2)[:, None] * r.uniform(0.4, 0.7)
    # "phup" ap suat bung ra: thong thap MO 400 -> 3,5 kHz roi dong lai, khi xoay pha tre 9 -> 0,6 ms
    tm = r.uniform(0.02, 0.07)
    tat = r.uniform(0.1, 0.5)
    fc = 400 + 3100 * np.clip(t0 / tm, 0, 1) * np.exp(-np.maximum(t0 - tm, 0) / tat)
    x = flanger(on2(n, r, 0.5), 0.0006 + 0.009 * np.exp(-t0 / tat), 0.8)
    tron += quet2(x, fc + 1, kieu='thap') * bao_len_xuong(t, 0.004, tat * r.uniform(1.0, 1.4), truoc)[:, None] * 1.1
    tron += b3.toc(t, r, truoc, r.uniform(0.2, 0.6), (300, 1200), 0.015)
    return hoan_tat(tron, r, r.uniform(0.8, 1.4), r.uniform(0.3, 0.5), -4)


def C_vong(hat):
    r = np.random.default_rng(hat)
    n = int(SR * 2.3); t = np.arange(n) / SR
    tron = np.zeros((n, 2))
    bv = np.clip(t / 0.18, 0, 1) * np.clip((1.9 - t) / 0.9, 0, 1) ** 1.3
    het = r.uniform(1.6, 2.1)
    bv = np.clip(t / 0.18, 0, 1) * np.clip((het - t) / 0.9, 0, 1) ** 1.3 * np.maximum(cham(n, 1.2, r) * 0.25 + 1, 0.4)
    bung = r.uniform(0.4, 1.5)
    # (do lan 3: chenh bien the 1,6-1,8 dB - phan giu giong nhau) -> 1-3 dot DAP manh len o thoi diem ngau nhien
    for _ in range(int(r.integers(1, 4))):
        bv = bv * (1 + r.uniform(0.5, 1.2) * np.exp(-((t - r.uniform(0.35, 1.5)) / r.uniform(0.1, 0.25)) ** 2))
    tron += b2.bum_chung(t, r, r.uniform(220, 260), r.uniform(100, 115), r.uniform(0.12, 0.18), 1.2) * 0.5 * bung
    # song xung kich: khi lao ra, thong thap mo nhanh roi dong cham
    dong = r.uniform(0.2, 0.6)
    fc = 300 + 3000 * np.clip(t / 0.05, 0, 1) * np.exp(-np.maximum(t - 0.05, 0) / dong)
    tron += quet2(on2(n, r, 0.45), fc, kieu='thap') * bao_len_xuong(t, 0.006, dong)[:, None] * 0.9 * bung
    if r.random() < 0.6:
        bd2 = r.uniform(0.4, 1.0)
        tron += quet2(on2(n, r, 0.45), 300 + 1500 * np.exp(-np.maximum(t - bd2, 0) / 0.15), kieu='thap') * bao_len_xuong(t, 0.02, 0.25, bd2)[:, None] * r.uniform(0.4, 0.7)
    # khi cuon XOAY quanh vong (xoay pha cham + quay trai phai)
    d = 0.003 + 0.004 * (0.5 + 0.5 * np.sin(PI2 * r.uniform(0.3, 0.6) * t + r.uniform(0, PI2)))
    # (do lan 4: on chung 0,4 + quay sau 0,7 -> tuong quan 0,03-0,05, loang; nay 0,65 / 0,5)
    k = flanger(loc2(on2(n, r, 0.5), lambda f: cao(250, 2)(f) * thap(2200, 2)(f)), d, 0.85)
    tron += xoay(k, r.uniform(0.4, 0.8), 0.5, r) * bv[:, None] * r.uniform(0.4, 0.8)
    hk = loc2(on2(n, r, 0.5), dai(r.uniform(260, 380), 1.1))
    tron += xoay(hk, r.uniform(0.6, 1.2), 0.5, r) * bv[:, None] * r.uniform(0.25, 0.55)
    return hoan_tat(tron, r, r.uniform(1.3, 1.8), r.uniform(0.4, 0.55), -2)


def C_trung(hat):
    r = np.random.default_rng(hat)
    n = int(SR * 0.6); t = np.arange(n) / SR
    tron = np.zeros((n, 2))
    tron += b2.bum_chung(t, r, r.uniform(240, 290), r.uniform(130, 150), r.uniform(0.02, 0.045), 1.1) * r.uniform(0.2, 0.4)
    tron += b3.toc(t, r, 0.0, r.uniform(0.4, 0.9), (400, 1600), r.uniform(0.01, 0.025))
    if r.random() < 0.45:
        bd2 = r.uniform(0.05, 0.12)
        tron += keo_lech(b3.toc(t, r, bd2, r.uniform(0.3, 0.5), (500, 1800), 0.012), r.uniform(-0.8, 0.8))
    x = flanger(on2(n, r, 0.5), 0.0008 + r.uniform(0.004, 0.008) * np.clip(t / 0.2, 0, 1), 0.8)
    fc = 3200 * (500 / 3200) ** np.clip(t / r.uniform(0.15, 0.3), 0, 1)
    x = quet2(x, fc, kieu='thap') * bao_len_xuong(t, 0.003, r.uniform(0.08, 0.16))[:, None]
    tron += keo_lech(x, np.linspace(0, r.uniform(-0.6, 0.6), n)) * 0.8
    return hoan_tat(tron, r, r.uniform(0.5, 0.8), r.uniform(0.25, 0.4), -4)


MAU = {
    'A_bong_ma': (A_an, A_hien, A_vong, A_trung, 9100),
    'B_bang_suong': (B_an, B_hien, B_vong, B_trung, 9300),
    'C_hu_khong': (C_an, C_hien, C_vong, C_trung, 9500),
}


# ------------------------------------------------------------------ do
def do_phang(x, a=300, b=4000):
    m = x.mean(axis=1)
    if len(m) < 1024: return 0.0
    P = np.abs(np.fft.rfft(m * np.hanning(len(m)))) ** 2
    f = np.fft.rfftfreq(len(m), 1 / SR)
    s = P[(f >= a) & (f < b)] + 1e-20
    return float(np.exp(np.mean(np.log(s))) / np.mean(s))


def do(ten, x):
    m = x.mean(axis=1)
    P = np.abs(np.fft.rfft(m)) ** 2
    f = np.fft.rfftfreq(len(m), 1 / SR)
    pt = [P[(f >= a) & (f < b)].sum() / P.sum() * 100 for a, b in [(0, 200), (200, 1000), (1000, 4000), (4000, 22050)]]
    l, rr = x[:, 0], x[:, 1]
    tq = float(np.sum(l * rr) / np.sqrt(np.sum(l * l) * np.sum(rr * rr)))
    s = (l - rr) / 2; mm = (l + rr) / 2
    sm = np.sqrt(np.sum(s ** 2) / np.sum(mm ** 2))
    e = np.sqrt(np.convolve(m ** 2, np.ones(441) / 441, 'same'))
    idinh = int(np.argmax(e))
    duoi = np.nonzero(e[idinh:] > e[idinh] * 10 ** (-30 / 20))[0]
    dau = do_phang(x[:int(0.15 * SR)])
    sau = do_phang(x[idinh + int(0.05 * SR):]) if len(x) - idinh > int(0.2 * SR) else 0
    print('%-26s %.2f s  <200/200-1k/1-4k/>4k: %3.0f/%3.0f/%3.0f/%3.0f%%  tq %.2f  S/M %.2f  -30dB %.2f s  phang dau %.2f duoi %.2f' %
          (ten, len(x) / SR, *pt, tq, sm, (duoi[-1] if len(duoi) else 0) / SR, dau, sau))
    return pt, tq


def chenh_bien_the(ds):
    """Chenh TB duong bao (dB) giua cac cap bien the."""
    n = min(len(x) for x in ds)
    bao = []
    for x in ds:
        m = x[:n].mean(axis=1)
        e = np.sqrt(np.convolve(m ** 2, np.ones(882) / 882, 'same')) + 1e-5
        bao.append(20 * np.log10(e))
    ra = []
    for i in range(len(bao)):
        for j in range(i + 1, len(bao)):
            mk = np.maximum(bao[i], bao[j]) > np.max(bao[i]) - 40
            ra.append(np.mean(np.abs(bao[i][mk] - bao[j][mk])))
    return ra


def ghep(an, hien, vong, trung):
    """Giong trong game: tang hinh (giua) ... 1,6 s ... hien hinh + vong phep (giua) + 3 ke trung (trai xa, phai gan, phai xa)."""
    out = np.zeros((int(SR * 12.0), 2))
    for lan in range(2):
        g = lan * 6.0
        tb.cong(out, an[lan], g + 0.2)
        tb.cong(out, hien[lan], g + 2.4)
        tb.cong(out, vong[lan], g + 2.4)
        for k, (p, a) in enumerate(((-0.7, 0.55), (0.35, 0.85), (0.8, 0.5))):
            tb.cong(out, keo_lech(trung[(k + lan) % 3], p) * a, g + 2.4)
    return out / np.max(np.abs(out)) * 0.95


if __name__ == '__main__':
    ra = sys.argv[1] if len(sys.argv) > 1 else os.path.join('CongCu', 'AmThanh', 'TangHinhThu')
    os.makedirs(ra, exist_ok=True)
    chi = [a for a in sys.argv[2:]]
    for ten, (fa, fh, fv, ft, hat0) in MAU.items():
        if chi and not any(c in ten for c in chi): continue
        bo = {'an': [fa(hat0 + 10 * i) for i in range(3)], 'hien': [fh(hat0 + 100 + 10 * i) for i in range(3)],
              'vong': [fv(hat0 + 200 + 10 * i) for i in range(3)], 'trung': [ft(hat0 + 300 + 10 * i) for i in range(3)]}
        for loai, ds in bo.items():
            for i, x in enumerate(ds):
                tb.ghi(os.path.join(ra, '%s_%s%d.wav' % (ten, loai, i + 1)), x)
                do('%s_%s%d' % (ten, loai, i + 1), x)
            print('   chenh bien the %s: %s dB' % (loai, ' / '.join('%.1f' % c for c in chenh_bien_the(ds))))
        g = ghep(bo['an'], bo['hien'], bo['vong'], bo['trung'])
        tb.ghi(os.path.join(ra, '%s_ghep_nghe_thu.wav' % ten), g)
        do('%s_ghep' % ten, g)
