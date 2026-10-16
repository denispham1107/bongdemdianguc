# -*- coding: utf-8 -*-
# TIENG QUA CAU BANG + MUA BANG - BAN 3 (16/10/2026). Nguoi dung nghe ban 2 (tieng_qua_cau_bang_2.py):
#   "Qua cau bang lai qua gan giong Qua cau lua. Mua bang chua dat - hien gio giong nhu tieng sung ban LAZER."
# Vi sao:
#   - GIONG LUA: ban 2 dung lai lop "liem lua" (q3.lua) lam khi lanh cuon khi bay, gio xe (q3.gio_xe) y Qua cau lua, va "um / bum" tram +
#     gio toa hai ben khi no - dung cac lop lam nen tieng lua.
#   - GIONG LAZER: tieng nut "PIU" (cao do truot xuong nhanh) = dung tieng sung lazer "piu piu"; Mua bang ~8 qua / giay, moi qua mot "piu",
#     cong tieng rit khi roi.
# Ban 3 - BAN SAC RIENG cua bang, KHONG mot lop nao cua tieng lua / khong song truot cao do:
#   - HOI LANH "XI" XOAY: on mong o dai CAO (1,4-3,6 kHz), tam dai troi, quay trai <-> phai - khong co tieng gam tram nao (lua thi tram);
#   - BANG NGHIEN "rac rac": hang tram hat va cham nho day dac (vun_bang) - cot loi tieng bang;
#   - CUC BANG "coc" DUC tat 3-25 ms (cuc_roi), KEN KET, NUOC DONG BANG lach tach min;
#   - "TOC" va cham kho, ngan (on loc 250-900 Hz tat 12 ms) thay cho "um / bum";
#   - MUA BANG = MUA DA that: tieng roi chi la "vut" on nhe sang dan (khong rit), cham dat "lop dop" lao xao + coc nho.
# Do them (so voi Qua cau lua that trong game): TUONG QUAN PHO log giua tieng bang va tieng lua cung loai (thap = khac); do "co cao do"
# 150 ms dau (do phang pho 300 Hz - 4 kHz, cao = lao xao, thap = co cao do nhu "piu" / thuy tinh).
# Dung cac ham bang tu tieng_qua_cau_bang_2.py (vun_bang, cuc_roi, cuc_bang, ken_ket, dong_bang, pan_am_luong).
#
# Hai MAU:
#   A "bang_gia"   - gio bang xoay "xi" ro, vo nghien + tang roi "coc", ken ket ro.
#   B "mua_da"     - nang ve "lop dop / rac rac" nhu mua da, hoi lanh nhe hon.
#
# Chay: python CongCu/AmThanh/tieng_qua_cau_bang_3.py [thu_muc_ra]   (mac dinh CongCu/AmThanh/QuaCauBangThu3)

import os, sys
import numpy as np
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import tieng_bam as tb
import tieng_qua_cau_lua_3 as q3
import tieng_qua_cau_bang_2 as b2

SR = tb.SR
PI2 = tb.PI2
GIAY_BAY = 3.0
GIAY_ROI_MUA = 0.96
loc, dai, thap, cao, cham = q3.loc, q3.dai, q3.thap, q3.cao, q3.cham


def xi_xoay(n, r, f_min=1200, f_max=2700, quay_hz=1.0, sau=0.5, vong=False):
    """HOI LANH "XI" XOAY: ba dai on hep-vua (Q 2) trong 1,4-3,6 kHz, moi dai mot bao troi cham, ca khoi QUAY trai <-> phai
    (bao nguoc pha hai kenh, quay_hz phai so chu ky nguyen trong vong lap); hai kenh ~40% chung."""
    t = np.arange(n) / SR
    zc = r.standard_normal(n)
    ra = np.zeros((n, 2))
    tams = np.geomspace(f_min, f_max, 3)
    baos = [np.maximum(cham(n, 1.5, r) * 0.5 + 0.6, 0.05) for _ in tams]
    for k in range(2):
        z = 0.5 * zc + 0.85 * r.standard_normal(n)
        # (do lan 1: dai 1,4-3,6 kHz -> tren 4 kHz 18-21% - choi; nay 1,2-2,7 kHz + chan thong thap 3,8 kHz bac 4)
        y = sum(loc(z, lambda f, fc=fc: dai(fc * (1 + 0.03 * (k - 0.5)), 2.0)(f) * thap(3800, 4)(f)) * b for fc, b in zip(tams, baos))
        ra[:, k] = y
    ra /= np.max(np.abs(ra)) + 1e-12
    s = np.sin(PI2 * quay_hz * t + r.uniform(0, PI2))
    ra[:, 0] *= 1 - sau * (0.5 + 0.5 * s)
    ra[:, 1] *= 1 - sau * (0.5 - 0.5 * s)
    return ra


def toc(t, r, bd, nang=1.0, f=(250, 900), tat=0.012):
    """"TOC" va cham kho, ngan: on loc 250-900 Hz tat ~12 ms; hai kenh 60% chung (khong nguoc pha)."""
    n = len(t)
    tt = np.maximum(t - bd, 0)
    e = np.exp(-tt / tat) * (t >= bd) * np.minimum(1, tt / 0.0005)
    zc = r.standard_normal(n)
    ra = np.stack([loc(0.6 * zc + 0.6 * r.standard_normal(n), lambda ff: cao(f[0], 2)(ff) * thap(f[1], 2)(ff)) * e for _ in range(2)], axis=1)
    return ra / (np.max(np.abs(ra)) + 1e-12) * nang


def phut_lanh(n, r, bao, f0=1000, f1=2800):
    """Hoi lanh PHUT ra (xi manh): on 1,2-3,6 kHz, hai kenh 40% chung."""
    # b2.xi_lanh loc thong thap bac 2 - do: tren 4 kHz 11-13% (ban / roi); them chan 3,5 kHz bac 4
    x = b2.xi_lanh(n, r, np.ones(n), f0, f1)
    x = np.stack([loc(x[:, k], thap(3500, 4)) for k in range(2)], axis=1)
    return x / (np.max(np.abs(x)) + 1e-12) * bao[:, None]


# ================================================================== BAN
def tieng_ban(hat, kieu):
    r = np.random.default_rng(hat)
    gia = kieu == "A"
    n = int(SR * 0.9); t = np.arange(n) / SR
    tron = np.zeros((n, 2))
    # "XI-IK": hoi lanh phut + bang KET lai "rac" (vun nhanh), "toc" kho nho
    tron += phut_lanh(n, r, np.minimum(1, t / 0.005) * np.exp(-t / r.uniform(0.07, 0.14))) * 0.9
    tron += b2.vun_bang(t, r, r.uniform(0.0, 0.02), r.uniform(0.05, 0.11), r.uniform(600, 1100), (1000, 2800), nang=0.9 if gia else 1.1)
    tron += toc(t, r, 0.0, r.uniform(0.3, 0.5))
    # duoi: hoi lanh xoay bay theo qua, lech dan sang mot ben
    dinh = r.uniform(0.04, 0.1)
    bg = np.minimum(1, t / dinh) * np.exp(-np.maximum(t - dinh, 0) / r.uniform(0.12, 0.25))
    x = xi_xoay(n, r, quay_hz=r.uniform(2, 4)) * bg[:, None] * r.uniform(0.35, 0.55)
    p = np.linspace(0, r.choice([-0.6, 0.6]), n); g = (p + 1) * np.pi / 4
    x[:, 0] *= np.cos(g) * np.sqrt(2); x[:, 1] *= np.sin(g) * np.sqrt(2)
    tron += x
    tron += b2.dong_bang(n, r, r.uniform(150, 300), bao_mat=np.exp(-t / 0.25))
    tron = q3.vang_uot(tron, 0.3, r, 0.2)
    return q3.chuan(tron, -4)


# ================================================================== BAY (vong lap)
def tieng_bay(hat, kieu):
    r = np.random.default_rng(hat)
    gia = kieu == "A"
    n = int(SR * GIAY_BAY)
    x = xi_xoay(n, r, 1200, 2700, quay_hz=round(1.3 * GIAY_BAY) / GIAY_BAY, sau=0.55) * 1.0
    x += xi_xoay(n, r, 900, 2000, quay_hz=round(0.7 * GIAY_BAY) / GIAY_BAY, sau=0.4) * 0.45     # lop thap hon (van > 900 Hz)
    x += b2.dong_bang(n, r, 60 if gia else 90, vong=True) * 1.2
    # tinh the bang ket / vo vun nho tren qua: "rac" ngan ngau nhien (quan ve dau -> lien mach)
    t = np.arange(n) / SR
    for _ in range(int(GIAY_BAY * (3 if gia else 5))):
        bd = r.uniform(0, GIAY_BAY)
        tt = (t - bd) % GIAY_BAY
        x += b2.vun_bang(tt, r, 0.0, r.uniform(0.015, 0.04), r.uniform(500, 900), (1100, 2800), nang=r.uniform(0.12, 0.3))
    bung = np.maximum(cham(n, 0.9, r) * 0.45 + 0.75, 0.15) ** 1.5
    x *= bung[:, None]
    return q3.chuan(x, -7, vong=True)


# ================================================================== NO (qua cau bang trung)
def tieng_no(hat, kieu):
    r = np.random.default_rng(hat)
    gia = kieu == "A"
    to = r.uniform(0.75, 1.3)
    n = int(SR * 2.2); t = np.arange(n) / SR
    tron = np.zeros((n, 2))
    tron += toc(t, r, 0.0, r.uniform(0.9, 1.2), (200, 900), 0.02)
    tron += b2.vun_bang(t, r, 0.001, r.uniform(0.15, 0.28) * to, r.uniform(1100, 1800), (700, 2600), nang=1.4)
    tron += phut_lanh(n, r, np.minimum(1, t / 0.008) * np.exp(-t / r.uniform(0.2, 0.4))) * 0.7
    b2.cuc_roi(tron, r, int(r.integers(4, 8) * to), 0.02, 0.15, 280, 650, (0.008, 0.03), nang=0.9, nay=(2, 4), rong=0.6)   # tang
    b2.cuc_roi(tron, r, int(r.integers(10, 18) * to), 0.03, 0.8, 650, 1800, nang=0.55, nay=(1, 3))                      # vun
    # bang MOC len tren dat: nuoc dong bang lach tach + ken ket
    tron += b2.dong_bang(n, r, r.uniform(250, 450), bao_mat=np.clip((t - 0.06) / 0.1, 0, 1) * np.exp(-np.maximum(t - 0.16, 0) / 0.5)) * 1.3
    tron += b2.ken_ket(t, r, r.uniform(0.15, 0.3), r.uniform(0.3, 0.5), r.uniform(320, 520)) * (0.45 if gia else 0.25)
    if r.random() < 0.4:      # mot tang vo them o mot ben
        bd = r.uniform(0.12, 0.4)
        phu = b2.vun_bang(t, r, bd, 0.06, 700, nang=0.5) + toc(t, r, bd, 0.3)
        p2 = r.uniform(-0.85, 0.85); phu[:, 0] *= np.sqrt(1 - p2); phu[:, 1] *= np.sqrt(1 + p2)
        tron += phu
    tron = q3.vang_uot(tron, 0.5, r, 0.25)
    return q3.chuan(tron, -2)


# ================================================================== KHOI BANG VO
def tieng_vo(hat, kieu, manh=1.0):
    r = np.random.default_rng(hat)
    gia = kieu == "A"
    n = int(SR * (1.7 if manh <= 1 else 2.2)); t = np.arange(n) / SR
    tron = np.zeros((n, 2))
    tk = r.uniform(0.1, 0.25) if manh <= 1 else 0.0
    if tk > 0:
        tron += b2.ken_ket(t, r, 0.0, tk, r.uniform(300, 520)) * (0.6 if gia else 0.4)
    tron += toc(t, r, tk, (0.6 if manh <= 1 else 1.1), (220, 800), 0.018)
    tron += b2.vun_bang(t, r, tk + 0.002, r.uniform(0.16, 0.3) * manh, r.uniform(1000, 1800), (600, 2400), nang=1.3)
    if manh > 1:
        tron += phut_lanh(n, r, np.minimum(1, t / 0.008) * np.exp(-t / 0.3)) * 0.6
    b2.cuc_roi(tron, r, int(r.integers(4, 8) * manh), tk + 0.01, tk + 0.15, 250, 600, (0.01, 0.035), nang=1.0, nay=(2, 4), rong=0.6)
    b2.cuc_roi(tron, r, int(r.integers(8, 16) * manh), tk + 0.02, tk + 0.9, 600, 1700, nang=0.6, nay=(1, 3))
    tron = q3.vang_uot(tron, 0.45, r, 0.22)
    return q3.chuan(tron, -2 if manh > 1 else -3)


# ================================================================== MUA BANG (mua da)
def tieng_mua_roi(hat, kieu):
    """"Vut" on nhe (1-3 kHz) sang + to dan toi luc cham dat 0,96 s - KHONG co tieng rit / truot cao do."""
    r = np.random.default_rng(hat)
    n = int(SR * 1.1); t = np.arange(n) / SR
    x = np.clip(t / GIAY_ROI_MUA, 0, 1)
    to = 1.0 / (1 + 8 * (1 - x))                     # ~ 1/khoang cach (qua roi nhanh dan)
    tron = phut_lanh(n, r, to * np.minimum(1, t / 0.05), 900, 3000) * 0.8
    tron = b2.pan_am_luong(tron.mean(axis=1), 0) * 0.3 + tron * 0.7
    return q3.chuan(tron, -6)


def tieng_mua_cham(hat, kieu):
    """Hat bang to cham dat: "toc" + "rac" lao xao ngan + 1-3 vun nay "lop dop"."""
    r = np.random.default_rng(hat)
    da = kieu == "B"
    n = int(SR * 0.8); t = np.arange(n) / SR
    tron = np.zeros((n, 2))
    tron += toc(t, r, 0.0, r.uniform(0.7, 1.0), (300, 1100), r.uniform(0.008, 0.015))
    tron += b2.vun_bang(t, r, 0.001, r.uniform(0.03, 0.07), r.uniform(800, 1400), (900, 2600), nang=1.0 if da else 0.8)
    b2.cuc_roi(tron, r, int(r.integers(2, 5)), 0.01, 0.25, 600, 1600, nang=0.6, nay=(1, 3))
    tron += b2.dong_bang(n, r, 120, bao_mat=np.exp(-t / 0.2)) * 0.8
    tron = q3.vang_uot(tron, 0.25, r, 0.15)
    return q3.chuan(tron, -4)


if __name__ == '__main__':
    import tieng_qua_cau_bang as v1
    ra = sys.argv[1] if len(sys.argv) > 1 else os.path.join('CongCu', 'AmThanh', 'QuaCauBangThu3')
    os.makedirs(ra, exist_ok=True)
    for kieu, ten in (("A", "A_bang_gia"), ("B", "B_mua_da")):
        lech = 0 if kieu == "A" else 500
        bo = {
            'ban': [tieng_ban(5100 + 10 * i + lech, kieu) for i in range(3)],
            'no': [tieng_no(5200 + 10 * i + lech, kieu) for i in range(3)],
            'vo': [tieng_vo(5300 + 10 * i + lech, kieu) for i in range(3)],
            'vono': [tieng_vo(5400 + 10 * i + lech, kieu, 1.6) for i in range(3)],
            'muaroi': [tieng_mua_roi(5500 + 10 * i + lech, kieu) for i in range(3)],
            'muacham': [tieng_mua_cham(5600 + 10 * i + lech, kieu) for i in range(3)],
        }
        bay = tieng_bay(5700 + lech, kieu)
        noi = np.max(np.abs(bay[0] - bay[-1])); dd = np.max(np.abs(np.diff(bay, axis=0)), axis=1)
        print('   vong lap bay %s: noi %.4f, xep hang %.0f%% cac buoc' % (ten, noi, 100 * np.mean(dd < noi)))
        for loai, ds in bo.items():
            for i, x in enumerate(ds):
                tb.ghi(os.path.join(ra, '%s_%s%d.wav' % (ten, loai, i + 1)), x)
        tb.ghi(os.path.join(ra, '%s_bay.wav' % ten), bay)
        for tg, g in (('ghep_qua_cau', v1.ghep_qua_cau(bo['ban'], bay, bo['no'], bo['vo'])),
                      ('ghep_mua_bang', v1.ghep_mua_bang(bo['muaroi'], bo['muacham'], bo['vo'], bo['vono']))):
            g = g / np.max(np.abs(g)) * 0.95
            tb.ghi(os.path.join(ra, '%s_%s.wav' % (ten, tg)), g)
