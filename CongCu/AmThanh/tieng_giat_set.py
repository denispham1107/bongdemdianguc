# -*- coding: utf-8 -*-
# TIENG GIUT SET + QUA CAU DIEN (16/10/2026, nguoi dung): "them am thanh stereo cho skill Giut set - luc PHONG tia set ra, khi tia set DANH
# TRUNG doi thu, khi tia set LAN ra. Ap dung luon cho tia dien trong Qua cau dien. Khi Qua cau dien dang hien dien, cho am thanh dien chay
# XET LIEN TUC ben trong qua cau." Nguoi choi khac trong suong mu thi khong nghe (TiengViTri lo).
# Bon tieng: phong (~0,9 s) · trung (~0,7 s, moi ke trung dau) · lan (~0,5 s, moi nhip lan - Giut set lan 5 nhip cach 0,07 s) · cau (VONG LAP 3 s).
#
# Theo memory quy-trinh-tieng-ky-nang-moi NGAY TU DAU: moi lop sinh rieng tung kenh + ~40-50% chung (sinh cap on MOT lan - tieng_tang_hinh.loc2);
# su kien nho (tia lua, xet) moi cai mot cho, lech CHI bang am luong; am co cao do (vo vo dien) HAI KENH CUNG PHA, CUNG cao do (lech cao do -> pha
# troi -> doan nguoc pha); KHONG truot cao do nhanh (laser), KHONG ngan dai khong dieu hoa (thuy tinh), KHONG "rac rac" bang nghien (Qua cau bang);
# 3 bien the khac CAU TRUC; < 200 Hz <= ~25%, cat < 120 Hz, > 4 kHz <= ~10%, 200 Hz - 1 kHz >= ~25%.
#
# Dien that: "TACH / CHAT" phong dien (xung rong dai cuc ngan, bao hoa), XEO XEO (hang tram phong dien li ti day dac, chap chon khong deu),
# VO VO / "BZZT" (ho quang: chuoi hai dieu hoa co dinh, bien do giat cuc ngan), ram nhe (khong khi gian no).
#
# Ba MAU:
#   A "set_tu_nhien" - tia set that: phong = "TACH" xe khong khi + xeo xeo + ram nhe; trung = "chat" + xeo xeo bung + "huych"; lan = "tach" ngan.
#   B "dien_cao_the" - ho quang dien cao the: phong = "BZZZT" vo vo giat + xet; trung = "xeo" chay + "chat"; lan = "bzt" ngan.
#   C "roi_dien"     - roi dien + tia lua: phong = "vut" xe gio + "TACH" + toe tia lua; trung = toe tia lua nay + xeo chay; lan = "chat" + tia lua.
#
# Chay: python CongCu/AmThanh/tieng_giat_set.py [thu_muc_ra] [loc_ten_mau...]   (mac dinh CongCu/AmThanh/GiatSetThu)

import os, sys
import numpy as np
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import tieng_bam as tb
import tieng_qua_cau_lua_3 as q3
import tieng_qua_cau_bang_2 as b2
import tieng_tang_hinh as th

SR = tb.SR
PI2 = tb.PI2
GIAY_VONG = 3.0
loc, dai, thap, cao, cham = q3.loc, q3.dai, q3.thap, q3.cao, q3.cham
on2, loc2, quet2, keo_lech, xoay, bao_len_xuong = th.on2, th.loc2, th.quet2, th.keo_lech, th.xoay, th.bao_len_xuong


# ------------------------------------------------------------------ thanh phan DIEN
def tach(t, r, bd, nang=1.0, rong=0.5, giay=0.012, f=(700, 3600)):
    """"TACH / CHAT" phong dien: 3-7 xung on CUC NGAN (0,3-2 ms) trong 4-18 ms, loc dai rong, BAO HOA; hai kenh 50% chung, lech am luong."""
    n = len(t)
    x2 = np.zeros((n, 2))
    i0 = int(bd * SR)
    if i0 >= n: return x2
    so = int(r.integers(3, 8))
    zc = r.standard_normal(n)
    for _ in range(so):
        i = i0 + int(r.uniform(0, 0.018) * SR)
        d = int(r.uniform(0.0003, 0.002) * SR) + 8
        if i + d >= n: continue
        a = r.uniform(0.4, 1.0)
        for k in range(2):
            x2[i:i + d, k] += (0.5 * zc[i:i + d] + 0.7 * r.standard_normal(d)) * np.hanning(d) * a
    x2 = loc2(x2, lambda ff: cao(f[0], 2)(ff) * thap(f[1], 3)(ff))
    # than "chat" dai giua tat ~giay
    tt = np.maximum(t - bd, 0)
    than = loc2(on2(n, r, 0.55), lambda ff: dai(r.uniform(450, 900), 1.3)(ff)) * (np.exp(-tt / giay) * (t >= bd))[:, None]
    x2 = x2 / (np.max(np.abs(x2)) + 1e-12) + than / (np.max(np.abs(than)) + 1e-12) * 0.8
    x2 = np.tanh(2.2 * x2)
    return keo_lech(x2 / (np.max(np.abs(x2)) + 1e-12), r.uniform(-rong, rong)) * nang


def xeo(n, r, bao, mat_do=1500, f=(1100, 3600), chung=0.4, giat=40.0):
    """XEO XEO: phong dien li ti DAY DAC, mat do CHAP CHON khong deu (on loc thap 'giat' Hz ^3); moi hat 0,2-1 ms qua loc dai; hai kenh
    phan lon rieng + chung. Khac 'rac rac' bang nghien (hat cong huong tram hon, cum theo vo): o day hat SANG, NGAN, nhip giat nhanh."""
    dot = np.maximum(cham(n, giat, r) * 0.7 + 0.4, 0) ** 3
    p = bao * dot / (dot.mean() + 1e-12) * mat_do / SR

    def mot():
        vt = np.nonzero(r.random(n) < p)[0]
        x = np.zeros(n)
        x[vt] = np.exp(r.uniform(np.log(0.1), 0, len(vt))) * np.sign(r.uniform(-1, 1, len(vt)))
        return loc(x, lambda ff: cao(f[0], 2)(ff) * thap(f[1], 3)(ff) + 0.35 * dai(np.sqrt(f[0] * 550), 1.5)(ff))
    c = mot()
    ra = np.stack([chung * c + (1 - chung) * mot(), chung * c + (1 - chung) * mot()], axis=1)
    return ra / (np.max(np.abs(ra)) + 1e-12)


def vo_vo(t, r, f0, bao, giat=30.0, sau=0.75, tram=1.0, so=None, vong=False, bao_hoa=2.0, sang=0.8):
    """VO VO / "BZZT" ho quang: 2-3 HO QUANG RIENG (moi cai cao do lech 3-8% quanh f0 140-240 Hz, chuoi dieu hoa rang cua qua hai dai cong
    huong 400-700 + 1,2-2 kHz, bien do GIAT rieng), moi cai mot cho trai / phai bang AM LUONG. Khong truot cao do.
    ⚠️ (do lan 1: mot ho quang chung hai kenh -> tuong quan 0,44-0,86, vong lap Qua cau dien gan mono. Hai NGUON khac cao do o hai ben
    thi gop mono van cong du - khong nguoc pha nhu mot nguon lech pha.)"""
    n = len(t)
    ra = np.zeros((n, 2))
    so = int(r.integers(2, 4)) if so is None else so
    cho_hq = np.linspace(-0.82, 0.82, so)   # (±0,9: hai ho quang khac cao do gan nhu tach han -> clip ngan tuong quan -0,02; ±0,75: vong lap 0,56) + r.uniform(-0.15, 0.15, so); r.shuffle(cho_hq)
    for j in range(so):
        fj = f0 * (1 + (r.uniform(0.03, 0.08) * r.choice([-1, 1]) if j > 0 else 0.0))
        rung = 1 + 0.006 * cham(n, 8, r)                 # rung nho (dien ap dao dong), khong phai truot
        tan = fj * rung
        if vong:      # vong lap: tong so chu ky trong vong phai NGUYEN, khong thi pha nhay o cho noi (do lan 2: noi xep hang 88%)
            tong = np.sum(tan) / SR; tan = tan * max(1, round(tong)) / tong
        pha = PI2 * np.cumsum(tan) / SR + r.uniform(0, PI2)
        x = sum(np.sin(h * pha) / h ** 0.85 for h in range(1, 22) if fj * h < 4200)
        f1, f2 = r.uniform(380, 700), r.uniform(1200, 2000)
        x = loc(x, lambda ff: tram * dai(f1, 2.0)(ff) + sang * dai(f2, 2.5)(ff) + 0.15)
        x = np.tanh(bao_hoa * x / (np.max(np.abs(x)) + 1e-12))
        g = np.maximum(cham(n, giat, r), -1.5)
        x = x * np.clip(1 - sau + sau * (0.5 + 0.5 * np.tanh(2 * g)), 0, 1) * (1.0 if j == 0 else r.uniform(0.7, 1.0))
        # (do lan 2: ho quang chinh o giua -> B van 0,4-0,68; nay cac ho quang TRAI DEU hai ben)
        lech = cho_hq[j]
        gg = (lech + 1) * np.pi / 4
        ra[:, 0] += x * np.cos(gg); ra[:, 1] += x * np.sin(gg)
    return ra / (np.max(np.abs(ra)) + 1e-12) * bao[:, None]


def ram(t, r, bd, giay, nang=1.0):
    """Ram nhe (khong khi gian no sau tia): um_chung tram ~140-200 Hz + on thap 200-500 Hz cuon, hai kenh cung pha."""
    tt = np.maximum(t - bd, 0)
    co = (t >= bd)[:, None]
    x = b2.um_chung(tt, r, r.uniform(220, 280), r.uniform(130, 160), giay * 0.4) * co * 0.6
    cuon = loc2(on2(len(t), r, 0.55), lambda ff: cao(160, 2)(ff) * thap(520, 2)(ff))
    cuon *= (np.maximum(cham(len(t), 6, r) * 0.4 + 1, 0.2) * np.clip(tt / 0.02, 0, 1) * np.exp(-tt / giay) * (t >= bd))[:, None]
    x = x + cuon / (np.max(np.abs(cuon)) + 1e-12) * 0.8
    return x * nang


def tia_lua(tron, r, so, bd0, bd1, nang=1.0, nay=(1, 3), rong=0.9):
    """TIA LUA toe ra roi nay: moi tia lua mot "tic" on 0,4-1,5 ms dai 1,8-3,6 kHz (khong ngan), nay 1-2 lan nho dan; lech bang am luong."""
    n = len(tron)
    for _ in range(so):
        bd = r.uniform(bd0, bd1)
        lech = np.clip(r.normal(0, 0.55) * rong, -1, 1)
        a = np.exp(r.uniform(np.log(0.15), 0)) * nang
        fc = np.exp(r.uniform(np.log(1800), np.log(3600)))
        for j in range(int(r.integers(nay[0], nay[1] + 1))):
            i = int((bd + j * r.uniform(0.04, 0.12)) * SR)
            d = int(r.uniform(0.0004, 0.0015) * SR) + 6
            if i + d + 256 >= n: break
            x = loc(np.pad(r.standard_normal(d) * np.hanning(d), (0, 256)), dai(fc, 1.8))
            x = x / (np.max(np.abs(x)) + 1e-12) * a * (0.55 ** j)
            g = (lech + 1) * np.pi / 4
            tron[i:i + len(x), 0] += x * np.cos(g)
            tron[i:i + len(x), 1] += x * np.sin(g)


def xe_gio(n, r, bd, giay, nang=1.0):
    """"VUT" xe gio khi roi dien quat ra: on dai quet 600 -> 2 kHz -> 900 (tren ON, khong co cao do), len 25 ms, tat 'giay'."""
    t = np.arange(n) / SR
    tt = np.maximum(t - bd, 0)
    fc = 700 + 1300 * np.exp(-((tt - 0.03) / 0.05) ** 2)
    x = quet2(on2(n, r, 0.5), fc, 1.2) * (np.clip(tt / 0.025, 0, 1) * np.exp(-np.maximum(tt - 0.025, 0) / giay) * (t >= bd))[:, None]
    return keo_lech(x / (np.max(np.abs(x)) + 1e-12), np.linspace(r.uniform(-0.7, 0.7), r.uniform(-0.7, 0.7), n)) * nang


def hoan_tat(tron, r, giay_vang, muc_vang, dinh_db):
    return th.hoan_tat(tron, r, giay_vang, muc_vang, dinh_db)


# ================================================================== MAU A "SET TU NHIEN"
def A_phong(hat):
    r = np.random.default_rng(hat)
    n = int(SR * 0.9); t = np.arange(n) / SR
    tron = np.zeros((n, 2))
    so = int(r.choice([1, 1, 2, 3]))                 # 1-3 tieng "tach" noi nhau (tia giat nhieu nhanh)
    for j in range(so):
        tron += tach(t, r, j * r.uniform(0.02, 0.06), (1.0 if j == 0 else r.uniform(0.4, 0.7)), 0.4, r.uniform(0.008, 0.02))
    dx = r.uniform(0.12, 0.35)
    tron += xeo(n, r, bao_len_xuong(t, 0.004, dx), r.uniform(1200, 2400)) * r.uniform(0.45, 0.7)
    tron += ram(t, r, 0.005, r.uniform(0.15, 0.35), r.uniform(0.35, 0.6))
    return hoan_tat(tron, r, r.uniform(0.5, 0.9), r.uniform(0.3, 0.45), -3)


def A_trung(hat):
    r = np.random.default_rng(hat)
    n = int(SR * 0.7); t = np.arange(n) / SR
    tron = np.zeros((n, 2))
    tron += tach(t, r, 0.0, 1.0, 0.3, r.uniform(0.01, 0.025), (500, 3000))
    dx = r.uniform(0.08, 0.25)
    tron += xeo(n, r, bao_len_xuong(t, 0.003, dx), r.uniform(1500, 3000), (900, 3200)) * r.uniform(0.5, 0.8)
    tron += b2.bum_chung(t, r, r.uniform(240, 300), r.uniform(130, 150), r.uniform(0.025, 0.05), 0.9) * r.uniform(0.2, 0.4)
    if r.random() < 0.5:
        tron += tach(t, r, r.uniform(0.05, 0.15), r.uniform(0.3, 0.5), 0.8, 0.008)
    return hoan_tat(tron, r, r.uniform(0.4, 0.7), r.uniform(0.25, 0.4), -4)


def A_lan(hat):
    r = np.random.default_rng(hat)
    n = int(SR * 0.5); t = np.arange(n) / SR
    tron = tach(t, r, 0.0, 1.0, 0.6, r.uniform(0.006, 0.015), (800, 3000))
    tron += xeo(n, r, bao_len_xuong(t, 0.002, r.uniform(0.04, 0.12)), r.uniform(1000, 2000)) * r.uniform(0.35, 0.6)
    return hoan_tat(tron, r, r.uniform(0.3, 0.5), r.uniform(0.2, 0.35), -5)


def A_cau(hat):
    r = np.random.default_rng(hat)
    n = int(SR * GIAY_VONG); t = np.arange(n) / SR
    bung = np.maximum(cham(n, 1.0, r) * 0.45 + 0.8, 0.2) ** 1.5
    tron = xeo(n, r, bung, 1400, (1000, 3400), chung=0.25) * 0.7
    tron += vo_vo(t, r, r.choice([150.0, 165.0, 180.0]), bung * 0.6, giat=25, sau=0.6, tram=1.2, vong=True) * 0.35
    for _ in range(int(GIAY_VONG * 3)):           # "tach" ngau nhien quanh qua cau (vong lap: dat theo modulo)
        bd = r.uniform(0, GIAY_VONG)
        x = tach(np.arange(int(0.15 * SR)) / SR, r, 0.0, r.uniform(0.25, 0.7), 0.8, 0.008)
        i = int(bd * SR); idx = (i + np.arange(len(x))) % n
        tron[idx] += x
    tron = xoay(tron, round(0.7 * GIAY_VONG) / GIAY_VONG, 0.45, r)
    return q3.chuan(tron, -6, vong=True)


# ================================================================== MAU B "DIEN CAO THE"
def B_phong(hat):
    r = np.random.default_rng(hat)
    giay = r.uniform(0.25, 0.5)
    n = int(SR * (giay + 0.6)); t = np.arange(n) / SR
    tron = np.zeros((n, 2))
    bb = np.clip(t / 0.005, 0, 1) * np.clip((giay - t) / 0.06, 0, 1)
    tron += vo_vo(t, r, r.uniform(150, 230), bb, giat=r.uniform(25, 60), sau=0.7) * 0.8
    tron += xeo(n, r, bb * 0.8 + 0.05, r.uniform(1000, 2000)) * 0.5
    tron += tach(t, r, 0.0, r.uniform(0.6, 1.0), 0.4, 0.012)
    if r.random() < 0.6:
        tron += tach(t, r, giay * r.uniform(0.6, 1.0), r.uniform(0.4, 0.7), 0.6, 0.01)
    return hoan_tat(tron, r, r.uniform(0.5, 0.8), r.uniform(0.3, 0.45), -3)


def B_trung(hat):
    r = np.random.default_rng(hat)
    n = int(SR * 0.7); t = np.arange(n) / SR
    tron = np.zeros((n, 2))
    tron += tach(t, r, 0.0, 1.0, 0.3, 0.015, (500, 3000))
    dxeo = r.uniform(0.1, 0.3)
    tron += xeo(n, r, bao_len_xuong(t, 0.005, dxeo), r.uniform(2000, 3500), (900, 3000), giat=r.uniform(15, 40)) * 0.75     # "xeo" chay
    tron += vo_vo(t, r, r.uniform(160, 240), bao_len_xuong(t, 0.003, r.uniform(0.04, 0.12)), giat=50, sau=0.6) * r.uniform(0.3, 0.55)
    tron += b2.bum_chung(t, r, r.uniform(240, 300), r.uniform(130, 150), 0.03, 0.9) * r.uniform(0.15, 0.3)
    return hoan_tat(tron, r, r.uniform(0.4, 0.7), r.uniform(0.25, 0.4), -4)


def B_lan(hat):
    r = np.random.default_rng(hat)
    giay = r.uniform(0.06, 0.16)
    n = int(SR * 0.5); t = np.arange(n) / SR
    bb = np.clip(t / 0.003, 0, 1) * np.clip((giay - t) / 0.02, 0, 1)
    tron = vo_vo(t, r, r.uniform(170, 260), bb, giat=60, sau=0.6) * 0.8
    tron += tach(t, r, 0.0, r.uniform(0.6, 1.0), 0.6, 0.008)
    tron += xeo(n, r, bb + 0.02, 1500) * 0.4
    return hoan_tat(tron, r, r.uniform(0.3, 0.5), r.uniform(0.2, 0.35), -5)


def B_cau(hat):
    r = np.random.default_rng(hat)
    n = int(SR * GIAY_VONG); t = np.arange(n) / SR
    bung = np.maximum(cham(n, 1.2, r) * 0.4 + 0.85, 0.25) ** 1.5
    tron = vo_vo(t, r, 165.0, bung, giat=20, sau=0.55, tram=1.3, so=3, vong=True) * 0.55
    tron += xeo(n, r, bung, 1100, chung=0.25) * 0.6
    for _ in range(int(GIAY_VONG * 2.5)):
        bd = r.uniform(0, GIAY_VONG)
        x = tach(np.arange(int(0.15 * SR)) / SR, r, 0.0, r.uniform(0.25, 0.6), 0.8, 0.008)
        i = int(bd * SR); idx = (i + np.arange(len(x))) % n
        tron[idx] += x
    tron = xoay(tron, round(0.7 * GIAY_VONG) / GIAY_VONG, 0.45, r)
    return q3.chuan(tron, -6, vong=True)


# ================================================================== MAU C "ROI DIEN"
def C_phong(hat):
    r = np.random.default_rng(hat)
    n = int(SR * 0.9); t = np.arange(n) / SR
    tron = np.zeros((n, 2))
    truoc = r.uniform(0.03, 0.08)
    tron += xe_gio(n, r, 0.0, r.uniform(0.04, 0.09), r.uniform(0.4, 0.7))
    tron += tach(t, r, truoc, 1.0, 0.4, r.uniform(0.01, 0.02))
    tia_lua(tron, r, int(r.integers(6, 18)), truoc, truoc + r.uniform(0.15, 0.4), r.uniform(0.3, 0.55))
    tron += xeo(n, r, bao_len_xuong(t, 0.004, r.uniform(0.08, 0.2), truoc), 1500) * 0.4
    tron += ram(t, r, truoc, r.uniform(0.1, 0.25), r.uniform(0.25, 0.45))
    return hoan_tat(tron, r, r.uniform(0.5, 0.8), r.uniform(0.3, 0.45), -3)


def C_trung(hat):
    r = np.random.default_rng(hat)
    n = int(SR * 0.7); t = np.arange(n) / SR
    tron = tach(t, r, 0.0, 1.0, 0.3, 0.015, (500, 3000))
    tia_lua(tron, r, int(r.integers(8, 20)), 0.005, r.uniform(0.1, 0.3), r.uniform(0.35, 0.6), nay=(1, 3))
    tron += xeo(n, r, bao_len_xuong(t, 0.01, r.uniform(0.1, 0.3)), 2500, (900, 3000), giat=20) * r.uniform(0.5, 0.75)
    tron += b2.bum_chung(t, r, r.uniform(240, 300), r.uniform(130, 150), 0.03, 0.9) * r.uniform(0.15, 0.3)
    return hoan_tat(tron, r, r.uniform(0.4, 0.7), r.uniform(0.25, 0.4), -4)


def C_lan(hat):
    r = np.random.default_rng(hat)
    n = int(SR * 0.5); t = np.arange(n) / SR
    tron = tach(t, r, 0.0, 1.0, 0.6, r.uniform(0.006, 0.014), (700, 3000))
    tia_lua(tron, r, int(r.integers(1, 12)), 0.003, r.uniform(0.04, 0.25), r.uniform(0.25, 0.5), nay=(1, 2))
    tron += xeo(n, r, bao_len_xuong(t, 0.002, r.uniform(0.04, 0.1)), 1200) * 0.35
    return hoan_tat(tron, r, r.uniform(0.3, 0.5), r.uniform(0.2, 0.35), -5)


def C_cau(hat):
    r = np.random.default_rng(hat)
    n = int(SR * GIAY_VONG); t = np.arange(n) / SR
    bung = np.maximum(cham(n, 1.0, r) * 0.45 + 0.8, 0.2) ** 1.5
    tron = xeo(n, r, bung, 1200, (1000, 3200), chung=0.25, giat=25) * 0.65
    tron += vo_vo(t, r, 175.0, bung * 0.5, giat=30, sau=0.65, vong=True) * 0.3
    tmp = np.zeros((n + int(0.6 * SR), 2))
    tia_lua(tmp, r, int(GIAY_VONG * 10), 0.0, GIAY_VONG, 0.35, nay=(1, 2))
    tron += tmp[:n]; tron[:len(tmp) - n] += tmp[n:]
    for _ in range(int(GIAY_VONG * 2)):
        bd = r.uniform(0, GIAY_VONG)
        x = tach(np.arange(int(0.15 * SR)) / SR, r, 0.0, r.uniform(0.25, 0.6), 0.8, 0.008)
        i = int(bd * SR); idx = (i + np.arange(len(x))) % n
        tron[idx] += x
    tron = xoay(tron, round(0.7 * GIAY_VONG) / GIAY_VONG, 0.45, r)
    return q3.chuan(tron, -6, vong=True)


MAU = {
    'A_set_tu_nhien': (A_phong, A_trung, A_lan, A_cau, 11100),
    'B_dien_cao_the': (B_phong, B_trung, B_lan, B_cau, 11300),
    'C_roi_dien': (C_phong, C_trung, C_lan, C_cau, 11500),
}


def ghep(phong, trung, lan, cau):
    """Giong trong game. 0-2 s: Giut set 3 tia (trai / giua / phai), moi tia trung roi lan 4 nhip cach 0,07 s ra xa dan. 2-3,6 s: lan nua.
    4-9 s: Qua cau dien lo lung ben phai (vong lap), cu 0,4 s ban mot luot (tieng phong nho) + trung 2-4 ke quanh no."""
    out = np.zeros((int(SR * 10.0), 2))
    r = np.random.default_rng(5)
    for lan_tung, g in enumerate((0.2, 2.2)):
        tb.cong(out, phong[lan_tung % 3], g)
        for k, p in enumerate((-0.6, 0.0, 0.6)):
            tb.cong(out, keo_lech(trung[(k + lan_tung) % 3], p) * 0.8, g + 0.01)
            for h in range(4):
                tb.cong(out, keo_lech(lan[(k + h) % 3], np.clip(p * (1 + 0.25 * (h + 1)), -1, 1)) * (0.6 * 0.85 ** h), g + 0.01 + 0.07 * (h + 1))
    vong = np.vstack([cau] * 2)[:int(5.0 * SR)]
    vong = vong * np.minimum(1, np.arange(len(vong)) / (0.1 * SR))[:, None] * np.minimum(1, (len(vong) - np.arange(len(vong))) / (0.15 * SR))[:, None]
    tb.cong(out, keo_lech(vong, 0.45) * 0.7, 4.3)
    for i in range(11):
        g = 4.4 + 0.4 * i
        tb.cong(out, keo_lech(phong[i % 3], 0.45) * 0.45, g)
        for j in range(int(r.integers(2, 5))):
            tb.cong(out, keo_lech(trung[(i + j) % 3], r.uniform(0.0, 1.0)) * 0.5, g + 0.01)
    return out / np.max(np.abs(out)) * 0.95


if __name__ == '__main__':
    ra = sys.argv[1] if len(sys.argv) > 1 else os.path.join('CongCu', 'AmThanh', 'GiatSetThu')
    os.makedirs(ra, exist_ok=True)
    chi = sys.argv[2:]
    for ten, (fp, ft, fl, fc, hat0) in MAU.items():
        if chi and not any(c in ten for c in chi): continue
        bo = {'phong': [fp(hat0 + 10 * i) for i in range(3)], 'trung': [ft(hat0 + 100 + 10 * i) for i in range(3)],
              'lan': [fl(hat0 + 200 + 10 * i) for i in range(3)]}
        cau = fc(hat0 + 300)
        for loai, ds in bo.items():
            for i, x in enumerate(ds):
                tb.ghi(os.path.join(ra, '%s_%s%d.wav' % (ten, loai, i + 1)), x)
                th.do('%s_%s%d' % (ten, loai, i + 1), x)
            print('   chenh bien the %s: %s dB' % (loai, ' / '.join('%.1f' % c for c in th.chenh_bien_the(ds))))
        tb.ghi(os.path.join(ra, '%s_cau.wav' % ten), cau)
        th.do('%s_cau' % ten, cau)
        noi = np.max(np.abs(cau[0] - cau[-1])); dd = np.max(np.abs(np.diff(cau, axis=0)), axis=1)
        m = cau.mean(axis=1); e = np.sqrt(np.convolve(m ** 2, np.ones(4410) / 4410, 'same'))[4410:-4410]
        print('   vong lap %s: noi %.4f (xep hang %.0f%% cac buoc), bao bien thien %.2f' % (ten, noi, 100 * np.mean(dd < noi), (e.max() - e.min()) / e.max()))
        g = ghep(bo['phong'], bo['trung'], bo['lan'], cau)
        tb.ghi(os.path.join(ra, '%s_ghep_nghe_thu.wav' % ten), g)
        th.do('%s_ghep' % ten, g)
