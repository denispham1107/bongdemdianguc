# -*- coding: utf-8 -*-
# TIENG QUA CAU BANG + BANG VO + MUA BANG (16/10/2026, nguoi dung): ban ra · bay (vong lap) · va cham + no; khoi bang tren dat VO
# (het gio moi cap, cap 5 no tung, ke bi dong bang chet vo tan); qua bang roi cua Mua bang (roi + cham dat). STEREO, theo vi tri.
# Lam DUNG quy trinh memory `quy-trinh-tieng-ky-nang-moi` (bai hoc tu Qua cau lua / Thien thach):
#   - moi lop sinh RIENG tung kenh, ~40% phan chung; su kien nho (manh bang, tinh the) moi cai mot cho;
#   - BANG THAT: manh bang = xung on kich CONG HUONG KHONG DIEU HOA (ti le 1 : 2,3 : 3,9 : 5,4 ... nhu thuy tinh / bang dac), tat 15-250 ms;
#     tieng NUT bang = chuoi vi xung day 5-25 ms (vet nut lan rat nhanh) qua cong huong 1-3 kHz; tieng KEN KET (bang chiu luc) = chuoi
#     xung thua 30-90 / s qua dai 350-900 Hz truoc khi vo; gio lanh = gio_xe (tam tan so troi khong deu);
#   - 3 BIEN THE moi tieng mot lan, gieo ngau nhien CA CAU TRUC (so manh, co / khong nut phu, thoi diem, do lon);
#   - vat lao toi (qua bang roi Mua bang): to theo 1 / khoang cach + sang dan; cu va cham noi truoc roi moi vo vun;
#   - dai dien thoai: < 200 Hz <= ~25%, cat < 120 Hz, > 4 kHz <= ~10%.
#
# Hai MAU de nguoi dung chon:
#   A "pha_le"   - bang pha le trong veo: tieng ngan tinh the khi bay, vo nhu thuy tinh trong, manh leng keng sang.
#   B "bang_da"  - bang da tu nhien nang: gio lanh day khi bay, nut "rac" + ken ket, khoi bang vo dac + vun, it leng keng.
#
# Chay: python CongCu/AmThanh/tieng_qua_cau_bang.py [thu_muc_ra]   (mac dinh CongCu/AmThanh/QuaCauBangThu)

import os, sys
import numpy as np
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import tieng_bam as tb
import tieng_qua_cau_lua_3 as q3
import tieng_thien_thach_2 as t2
import tieng_thien_thach_3 as t3

SR = tb.SR
PI2 = tb.PI2
GIAY_BAY = 3.0
GIAY_ROI_MUA = 0.96          # IceStorm.fallTime - qua roi nhanh dan (k^2), cham dat o 0,96 s
loc, dai, thap, cao, cham = q3.loc, q3.dai, q3.thap, q3.cao, q3.cham


# ------------------------------------------------------------------ manh bang / tinh the
def cong_huong_bang(d, r, f0, tat, sang=1.0):
    """Mot manh bang: cong huong KHONG dieu hoa (bang / thuy tinh dac), moi phan am tat rieng; tra ve mang mono dai d mau."""
    t = np.arange(d) / SR
    x = np.zeros(d)
    for j, (ti, a) in enumerate(((1.0, 1.0), (2.32, 0.7), (3.87, 0.5), (5.4, 0.32), (7.1, 0.18))):
        f = f0 * ti * r.uniform(0.97, 1.03)
        if f > 5200: break
        x += a * sang ** j * np.sin(PI2 * f * t + r.uniform(0, PI2)) * np.exp(-t / (tat / (1 + 0.5 * j)))
    x += loc(r.standard_normal(d), cao(1500, 2)) * np.exp(-t / 0.0012) * 0.5        # tiep xuc "tach"
    return x * np.minimum(1, t / 0.0003)


def manh_bang(tron, r, so, bd0, bd1, f_min=900, f_max=2600, tat=(0.02, 0.12), nang=1.0, nay=(1, 4), sang=1.0, rong=1.0):
    """Nhieu manh bang roi leng keng, NAY 1-3 lan (khoang nay ngan dan, nhe dan), moi manh mot cho, phan bo thua dan theo thoi gian."""
    dai_tron = len(tron) / SR
    for _ in range(so):
        bd = bd0 + (bd1 - bd0) * r.random() ** 1.6
        f0 = np.exp(r.uniform(np.log(f_min), np.log(f_max)))
        lech = np.clip(r.normal(0, 0.6) * rong, -1, 1)
        a = np.exp(r.uniform(np.log(0.15), 0)) * nang * np.exp(-bd * 1.2)
        khoang = r.uniform(0.05, 0.14)
        for lan in range(r.integers(nay[0], nay[1])):
            d = int(SR * 0.3)
            x = cong_huong_bang(d, r, f0 * r.uniform(0.98, 1.02), r.uniform(*tat), sang)
            # lech trai / phai CHI bang am luong: tb.dat_vi_tri them tre 0-0,6 ms o kenh xa - voi am CO CAO DO 1-3 kHz tre ay lech pha
            # > 180 do -> hai kenh nguoc pha (do lan 2: tuong quan -0,01 / -0,10; loa dien thoai gop mono se triet tieu)
            g = (lech + 1) * np.pi / 4
            tb.cong(tron, np.stack([x * a * np.cos(g), x * a * np.sin(g)], axis=1) * np.sqrt(2), bd)
            bd += khoang; khoang *= r.uniform(0.5, 0.75); a *= r.uniform(0.3, 0.55)
            if bd >= dai_tron - 0.3: break


def nut_bang(t, r, bd, dai_nut, f_tam=1800, mat_do=4000, nang=1.0):
    """Tieng NUT bang: vet nut lan rat nhanh = chuoi vi xung day trong dai_nut giay qua cong huong f_tam; hai kenh rieng."""
    n = len(t)
    ra = np.zeros((n, 2))
    e = np.clip((t - bd) / 0.0008, 0, 1) * np.exp(-np.maximum(t - bd, 0) / dai_nut)
    def chuoi():
        p = e * mat_do / SR
        vt = np.nonzero(r.random(n) < p)[0]
        x = np.zeros(n)
        x[vt] = np.exp(r.uniform(np.log(0.15), 0, len(vt))) * np.sign(r.uniform(-1, 1, len(vt)))
        return x
    chung = chuoi()     # ~45% chung hai kenh: do lan 1 tuong quan ~0 / am (-0,23) - loa dien thoai gop mono se triet tieu
    for k in range(2):
        x = 0.55 * chung + 0.85 * chuoi()
        y = loc(x, lambda f: cao(500, 2)(f) * thap(6000, 2)(f) * (0.4 + dai(f_tam * (1 + 0.04 * k), 2.5)(f) + 0.6 * dai(f_tam * 2.1, 3)(f)))
        ra[:, k] = y
    return ra / (np.max(np.abs(ra)) + 1e-12) * nang


def ken_ket(t, r, bd, giay, f_tam=600, nhip=(30, 90), nang=1.0):
    """Tieng KEN KET (bang chiu luc truoc khi vo): chuoi xung thua co nhip tang dan qua dai 350-900 Hz, hai kenh lech."""
    n = len(t)
    ra = np.zeros((n, 2))
    for k in range(2):
        x = np.zeros(n)
        tt = bd + r.uniform(0, 0.01)
        while tt < bd + giay:
            i = int(tt * SR)
            if i >= n: break
            x[i] = r.uniform(0.4, 1.0)
            tien = (tt - bd) / giay
            tt += 1.0 / (nhip[0] + (nhip[1] - nhip[0]) * tien) * r.uniform(0.6, 1.4)
        y = loc(x, lambda f: dai(f_tam * (1 + 0.06 * (k - 0.5)), 4)(f) + 0.5 * dai(f_tam * 1.7, 4)(f))
        e = np.clip((t - bd) / (giay * 0.6), 0, 1)
        ra[:, k] = y * e
    return ra / (np.max(np.abs(ra)) + 1e-12) * nang


def ngan_tinh_the(n, r, so_giong=5, f_min=1100, f_max=2800, vong=False):
    """Tieng NGAN tinh the (bang pha le): vai sin cao lech nhau hai kenh (phach cham), bien do troi ngau nhien; vong lap lien mach."""
    t = np.arange(n) / SR
    ra = np.zeros((n, 2))
    for _ in range(so_giong):
        f = np.exp(r.uniform(np.log(f_min), np.log(f_max)))
        if vong: f = round(f * n / SR) * SR / n
        for k in range(2):
            fk = f + (r.uniform(0.6, 2.0) * (1 if k else -1) if not vong else (round(r.uniform(1, 3)) * SR / n) * (1 if k else -1))
            am = np.maximum(cham(n, 2.0, r) * 0.6 + 0.3, 0) ** 2
            ra[:, k] += np.sin(PI2 * fk * t + r.uniform(0, PI2)) * am
    return ra / (np.max(np.abs(ra)) + 1e-12)


def gio_lanh(n, r, f0=600, f1=2600, hz=1.2):
    # gio_xe hai kenh DOC LAP hoan toan (tuong quan ~0) - them ~40% phan chung de co "tam" (do lan 3: ban / bay 0,01-0,05)
    g = q3.gio_xe(n, r, f0, f1, hz)
    c = g.mean(axis=1, keepdims=True)
    return 0.85 * g + 0.42 * c


def suong_xi(n, r, mat_do, bao_mat=None, vong=False):
    """Tieng 'xi xi' dong bang (tinh the bang moc): lach tach rat min, nho, dai 1,6-4 kHz."""
    return q3.lach_tach(n, r, mat_do, vong=vong, bao_mat=bao_mat, nang_bop=0.0, cao_min=1600, cao_max=4200) * 0.5


# ================================================================== BAN
def tieng_ban(hat, kieu):
    r = np.random.default_rng(hat)
    pha_le = kieu == "A"
    n = int(SR * 1.0); t = np.arange(n) / SR
    tron = np.zeros((n, 2))
    # "phut" khi lanh bung ra (on dai trung, hai kenh rieng) + "um" nhe
    zc = r.standard_normal(n); tat = r.uniform(0.05, 0.1)
    for k in range(2):
        tron[:, k] += loc(0.6 * zc + 0.8 * r.standard_normal(n), lambda f: cao(300, 2)(f) * thap(2200, 2)(f)) * np.exp(-t / tat) * np.minimum(1, t / 0.004)
    tron += q3.um(t, r, r.uniform(320, 420), r.uniform(180, 240), r.uniform(0.03, 0.06)) * r.uniform(0.25, 0.45)
    # vut gio lanh lao di, lech sang mot ben
    dinh = r.uniform(0.05, 0.12)
    bg = np.minimum(1, t / dinh) * np.exp(-np.maximum(t - dinh, 0) / r.uniform(0.12, 0.25))
    gx = gio_lanh(n, r, 700, 2800, 3.0) * bg[:, None] * r.uniform(0.4, 0.65)
    tron += t2.pan_quet(gx, np.linspace(0, r.choice([-0.6, 0.6]), n))
    # tinh the bang ket lai: chum ngan sang (A ro, B nho) + xi dong bang
    tron += suong_xi(n, r, r.uniform(150, 300), bao_mat=np.exp(-t / 0.25))
    manh_bang(tron, r, int(r.integers(4, 9) * (1.4 if pha_le else 0.7)), 0.01, 0.35, 1400, 3200, (0.03, 0.15) if pha_le else (0.015, 0.06),
              nang=0.5 if pha_le else 0.3, nay=(1, 2))
    if pha_le:
        nt = ngan_tinh_the(n, r, 4, 1300, 2900) * (np.minimum(1, t / 0.02) * np.exp(-t / 0.35))[:, None]
        tron += nt * 0.35
    tron = q3.vang_uot(tron, 0.4 if pha_le else 0.3, r, 0.35 if pha_le else 0.25)
    return q3.chuan(tron, -4)


# ================================================================== BAY (vong lap)
def tieng_bay(hat, kieu):
    r = np.random.default_rng(hat)
    pha_le = kieu == "A"
    n = int(SR * GIAY_BAY)
    x = gio_lanh(n, r, 500, 2400, 0.9) * (0.8 if pha_le else 1.1)
    # gio lanh tram (khoi khi lanh cuon quanh qua) - lop liem lua tram KHONG xi / tach dung lam luong khi cuon
    x += q3.lua(n, hat + 1, liem=0.9, xi=0.0, tach=0.0, mat_do=1, vong=True, tam_liem=22, dai_den=700, chung=0.35) * (0.5 if pha_le else 0.9)
    x += suong_xi(n, r, 45 if pha_le else 30, vong=True)
    if pha_le:
        x += ngan_tinh_the(n, r, 6, 1200, 2800, vong=True) * 0.22
    # tinh the bong ra leng keng ngau nhien (vong lap: dat quan ve dau)
    tron = np.zeros((n + int(0.4 * SR), 2))
    manh_bang(tron, r, int(GIAY_BAY * (7 if pha_le else 3)), 0.0, GIAY_BAY, 1500, 3400, (0.02, 0.08), nang=0.35, nay=(1, 2))
    du = tron[n:].copy(); tron = tron[:n]; tron[:len(du)] += du
    x += tron
    bung = np.maximum(cham(n, 0.9, r) * 0.45 + 0.75, 0.15) ** 1.5
    x *= bung[:, None]
    return q3.chuan(x, -7, vong=True)


# ================================================================== NO (qua cau bang trung)
def tieng_no(hat, kieu):
    r = np.random.default_rng(hat)
    pha_le = kieu == "A"
    to = r.uniform(0.75, 1.3)
    n = int(SR * 2.6); t = np.arange(n) / SR
    tron = np.zeros((n, 2))
    tron += q3.nut(t, r) * r.uniform(1.0, 1.5)
    tron += nut_bang(t, r, 0.0, r.uniform(0.012, 0.03) * to, r.uniform(1400, 2400), nang=r.uniform(1.0, 1.4))
    tron += q3.um(t, r, r.uniform(200, 280), r.uniform(110, 140), 0.09 * to) * 0.9
    # tang bang LON vo ra (cong huong 400-900 Hz, tat lau) - do lan 1: 200 Hz-1 kHz chi 3-11%, nghe mong / leng keng
    manh_bang(tron, r, int(r.integers(3, 6) * to), 0.0, 0.08, 400, 900, (0.06, 0.18), nang=1.1, nay=(2, 4), sang=0.7, rong=0.6)
    # vo tung: manh bang lon nho toa hai ben, nay
    manh_bang(tron, r, int(r.integers(14, 26) * to), 0.0, 0.7, 700 if not pha_le else 1000, 2600 if not pha_le else 3400,
              (0.04, 0.22) if pha_le else (0.02, 0.09), nang=0.9, nay=(1, 4), sang=1.0 if pha_le else 0.7)
    # khi lanh phut toa (gio xe hai ben) + xi dong bang (bang moc len tren dat) + ken ket ngan (B)
    for k, p in ((0, -0.9), (1, 0.9)):
        b = np.minimum(1, t / 0.01) * np.exp(-t / r.uniform(0.15, 0.3))
        tron += tb.dat_vi_tri(gio_lanh(n, r, 2600, 500, 4.0)[:, k] * b, p) * 0.35
    tron += suong_xi(n, r, r.uniform(300, 500), bao_mat=np.clip((t - 0.05) / 0.1, 0, 1) * np.exp(-np.maximum(t - 0.15, 0) / 0.5)) * 1.3
    if not pha_le:
        tron += ken_ket(t, r, r.uniform(0.12, 0.25), r.uniform(0.25, 0.45), r.uniform(450, 750)) * 0.35
    else:
        tron += ngan_tinh_the(n, r, 4, 1300, 3000) * (np.exp(-t / 0.6) * np.minimum(1, t / 0.01))[:, None] * 0.3
    if r.random() < 0.4:     # nut phu o mot ben
        bd = r.uniform(0.1, 0.35)
        phu = nut_bang(t, r, bd, 0.015, r.uniform(1500, 2600), nang=0.5)
        p2 = r.uniform(-0.85, 0.85); phu[:, 0] *= np.sqrt(1 - p2); phu[:, 1] *= np.sqrt(1 + p2)
        tron += phu
    tron = q3.vang_uot(tron, 0.9 if pha_le else 0.6, r, 0.45 if pha_le else 0.3)
    return q3.chuan(tron, -2)


# ================================================================== KHOI BANG VO (het gio / ke bi dong bang chet)
def tieng_vo(hat, kieu, manh=1.0):
    """manh 1 = khoi bang het gio vo; 1,6 = cap 5 no tung (them cu dap + khi lanh + nhieu manh hon)."""
    r = np.random.default_rng(hat)
    pha_le = kieu == "A"
    n = int(SR * (1.8 if manh <= 1 else 2.4)); t = np.arange(n) / SR
    tron = np.zeros((n, 2))
    # ken ket truoc khi vo (0,1-0,3 s) roi NUT + vo vun
    tk = r.uniform(0.08, 0.22) if manh <= 1 else 0.0
    if tk > 0:
        tron += ken_ket(t, r, 0.0, tk, r.uniform(400, 800), (25, 110)) * (0.35 if pha_le else 0.55)
    tron += nut_bang(t, r, tk, r.uniform(0.015, 0.035), r.uniform(1300, 2300), nang=1.2 * manh)
    if manh > 1:
        tron += q3.nut(t, r) * 1.3 + t3.bum(t, r, r.uniform(180, 240), 80, 0.12) * 0.7
        for k, p in ((0, -0.9), (1, 0.9)):
            b = np.minimum(1, t / 0.01) * np.exp(-t / 0.25)
            tron += tb.dat_vi_tri(gio_lanh(n, r, 2600, 500, 4.0)[:, k] * b, p) * 0.4
    # khoi vo thanh tang lon (cong huong thap, tat lau) + vun nho
    manh_bang(tron, r, int(r.integers(3, 6) * manh), tk + 0.005, tk + 0.12, 450, 900, (0.05, 0.16), nang=1.0, nay=(2, 4), sang=0.7)
    manh_bang(tron, r, int(r.integers(10, 20) * manh), tk + 0.01, tk + 0.9, 1100 if pha_le else 900, 3200 if pha_le else 2400,
              (0.03, 0.16) if pha_le else (0.015, 0.07), nang=0.7, nay=(1, 4))
    tron += t3.da_nghien(t, r, tk + 0.002, r.uniform(0.05, 0.1), r.uniform(1200, 2200), nang=0.45 if pha_le else 0.7)
    tron += suong_xi(n, r, 120, bao_mat=np.clip((t - tk) / 0.02, 0, 1) * np.exp(-np.maximum(t - tk, 0) / 0.6)) * 0.8
    tron = q3.vang_uot(tron, 0.7 if pha_le else 0.5, r, 0.4 if pha_le else 0.28)
    return q3.chuan(tron, -2 if manh > 1 else -3)


# ================================================================== MUA BANG: qua roi + cham dat
def tieng_mua_roi(hat, kieu):
    """1,2 s, cham dat o 0,96 s: qua roi NHANH DAN (k^2) -> gan dan theo 1/khoang cach + sang dan + rit lanh nho."""
    r = np.random.default_rng(hat)
    n = int(SR * 1.2); t = np.arange(n) / SR
    x = np.clip(t / GIAY_ROI_MUA, 0, 1)
    tron = t3.gam_lao_toi(t, r, GIAY_ROI_MUA, f_xa=600, f_gan=r.uniform(2600, 3200)) * 0.8
    # rit lanh: on dai hep truot xuong nhe
    tron += t2.rit(t, r, r.uniform(1700, 2200), r.uniform(1100, 1400), GIAY_ROI_MUA) * (x ** 2.2)[:, None] * 0.25
    tron += suong_xi(n, r, 90, bao_mat=x ** 2) * 0.8
    tron = t2.pan_quet(tron, r.uniform(-0.5, 0.5) * (1 - x))
    return q3.chuan(tron, -4)


def tieng_mua_cham(hat, kieu):
    r = np.random.default_rng(hat)
    pha_le = kieu == "A"
    n = int(SR * 1.1); t = np.arange(n) / SR
    tron = np.zeros((n, 2))
    tron += nut_bang(t, r, 0.0, r.uniform(0.008, 0.02), r.uniform(1600, 2600), nang=1.0)
    tron += q3.um(t, r, r.uniform(260, 340), 160, 0.05) * 0.6
    manh_bang(tron, r, int(r.integers(1, 3)), 0.0, 0.05, 500, 1000, (0.04, 0.12), nang=0.8, nay=(1, 3), sang=0.7, rong=0.5)
    manh_bang(tron, r, int(r.integers(5, 11)), 0.0, 0.4, 1200, 3200 if pha_le else 2600, (0.02, 0.1) if pha_le else (0.015, 0.05),
              nang=0.7, nay=(1, 3))
    tron += suong_xi(n, r, 200, bao_mat=np.exp(-t / 0.3))
    tron = q3.vang_uot(tron, 0.4, r, 0.25)
    return q3.chuan(tron, -4)


# ================================================================== GHEP nghe thu (nhu trong game)
def dat(out, x, bd, p=0.0, g=1.0):
    y = x.copy() * g
    y[:, 0] *= np.sqrt(1 - p); y[:, 1] *= np.sqrt(1 + p)
    tb.cong(out, y, bd)


def ghep_qua_cau(bans, bay, nos, vos):
    """4 qua (chum) ban ben trai -> bay 1,1 s sang phai -> 4 vu no (qua sau x0,45) -> 1,6 s sau 4 khoi bang vo (x0,5, lech nhau)."""
    out = np.zeros((int(SR * 7.0), 2))
    dat(out, bans[0], 0.0, -0.5)
    d = int(SR * 1.1)
    for i in range(4):
        vt = int(SR * i * 0.61) % len(bay)
        lap = np.concatenate([bay, bay])[vt:vt + d].copy()
        m = int(0.06 * SR); lap[:m] *= np.linspace(0, 1, m)[:, None]; lap[-m:] *= np.linspace(1, 0.3, m)[:, None]
        p = np.linspace(-0.5, 0.3 + 0.12 * i, d); gg = (p + 1) * np.pi / 4
        lap[:, 0] *= np.cos(gg) * np.sqrt(2); lap[:, 1] *= np.sin(gg) * np.sqrt(2)
        tb.cong(out, lap * 0.45, 0.1)
        dat(out, nos[i % 3], 1.2 + 0.03 * i, 0.3 + 0.12 * i, 1.0 if i == 0 else 0.45)
        dat(out, vos[i % 3], 1.2 + 1.6 + 0.07 * i, 0.3 + 0.12 * i, 1.0 if i == 0 else 0.5)
    return out


def ghep_mua_bang(rois, chams, vos, vonos):
    """Mua bang 3 s: moi 0,128 s mot qua roi (toi da 3 tieng roi cung luc, x0,35) + cham dat (x0,5); hai khoi bang moc roi vo;
    them mot khoi cap 5 no tung va mot ke bi dong bang chet vo tan (dung tieng vo, cao do 0,9)."""
    r = np.random.default_rng(77)
    out = np.zeros((int(SR * 7.5), 2))
    k = 0
    for i in range(int(3.0 / 0.128)):
        bd = i * 0.128
        p = r.uniform(-0.7, 0.7)
        if i % 3 == 0:   # tran 3 tieng roi cung luc ~ cu 3 qua mot tieng to
            ro = rois[k % 3][:int(GIAY_ROI_MUA * SR)]
            dat(out, ro, bd, p, 0.35)
        dat(out, chams[k % 3], bd + GIAY_ROI_MUA, p, 0.5)
        k += 1
    dat(out, vos[0], 1.5 + 1.6, -0.4, 0.8)
    dat(out, vos[1], 2.2 + 1.6, 0.5, 0.8)
    dat(out, vonos[0], 2.6 + 2.6, 0.1, 1.0)
    vt = vos[2][::1].copy()
    idx = np.arange(0, len(vt) - 1, 0.9).astype(int)      # cao do 0,9 (khoi to hon)
    dat(out, vt[idx], 4.2, -0.2, 0.9)
    return out


def phan_tich(ten, x):
    m = x.mean(axis=1)
    P = np.abs(np.fft.rfft(m)) ** 2
    f = np.fft.rfftfreq(len(m), 1 / SR)
    pt = [P[(f >= a) & (f < b)].sum() / P.sum() * 100 for a, b in [(0, 200), (200, 1000), (1000, 4000), (4000, 22050)]]
    l, rr = x[:, 0], x[:, 1]
    tq = float(np.sum(l * rr) / np.sqrt(np.sum(l * l) * np.sum(rr * rr)))
    print('%-26s %.2f s  <200/200-1k/1-4k/>4k: %s  tuong quan %.2f  dinh %.2f' % (ten, len(x) / SR,
          ' / '.join('%.0f%%' % p for p in pt), tq, np.max(np.abs(x))))


if __name__ == '__main__':
    ra = sys.argv[1] if len(sys.argv) > 1 else os.path.join('CongCu', 'AmThanh', 'QuaCauBangThu')
    os.makedirs(ra, exist_ok=True)
    for kieu, ten in (("A", "A_pha_le"), ("B", "B_bang_da")):
        lech = 0 if kieu == "A" else 500
        bo = {
            'ban': [tieng_ban(3100 + 10 * i + lech, kieu) for i in range(3)],
            'no': [tieng_no(3200 + 10 * i + lech, kieu) for i in range(3)],
            'vo': [tieng_vo(3300 + 10 * i + lech, kieu) for i in range(3)],
            'vono': [tieng_vo(3400 + 10 * i + lech, kieu, 1.6) for i in range(3)],
            'muaroi': [tieng_mua_roi(3500 + 10 * i + lech, kieu) for i in range(3)],
            'muacham': [tieng_mua_cham(3600 + 10 * i + lech, kieu) for i in range(3)],
        }
        bay = tieng_bay(3700 + lech, kieu)
        noi = np.max(np.abs(bay[0] - bay[-1])); dd = np.max(np.abs(np.diff(bay, axis=0)), axis=1)
        print('   vong lap bay %s: noi %.4f, xep hang %.0f%% cac buoc' % (ten, noi, 100 * np.mean(dd < noi)))
        for loai, ds in bo.items():
            for i, x in enumerate(ds):
                tb.ghi(os.path.join(ra, '%s_%s%d.wav' % (ten, loai, i + 1)), x); phan_tich('%s_%s%d' % (ten, loai, i + 1), x)
        tb.ghi(os.path.join(ra, '%s_bay.wav' % ten), bay); phan_tich('%s_bay' % ten, bay)
        for tg, g in (('ghep_qua_cau', ghep_qua_cau(bo['ban'], bay, bo['no'], bo['vo'])),
                      ('ghep_mua_bang', ghep_mua_bang(bo['muaroi'], bo['muacham'], bo['vo'], bo['vono']))):
            g = g / np.max(np.abs(g)) * 0.95
            tb.ghi(os.path.join(ra, '%s_%s.wav' % (ten, tg)), g); phan_tich('%s_%s' % (ten, tg), g)
