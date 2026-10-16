# -*- coding: utf-8 -*-
# TIENG GIUT SET + QUA CAU DIEN - BAN 2 (16/10/2026). Nguoi dung nghe ban 1 (tieng_giat_set.py, A Set tu nhien / B Dien cao the / C Roi dien):
# "nghe nhu tieng SUNG BAN ra dan chu khong phai tieng set va tieng DIEN CHAY, tao lai tat ca".
# Vi sao ban 1 ra tieng sung: tieng nao cung MO DAU bang MOT xung "TACH" bao hoa cuc ngan + "huych" tram (bum) + ram - dung cau truc tieng sung
# (cu no don, nang luong don het vao vai chuc ms dau roi tat). Do: ti le nang luong 30 ms dau / 300 ms dau (sung ~0,5-0,8) va he so dinh (dinh / RMS).
# Ban 2 - DIEN CHAY LIEN TUC, khong cu no don:
#   - "RET" xe toac: hang nghin phong dien li ti DAY DAC keo dai 0,15-0,5 s, len trong 10-30 ms (khong phai mot xung), do to chap chon;
#   - "XEO XEO" / "RE RE": tieng dien chay - phong dien ngat quang (xet xet) theo nhip khong deu + ho quang vo vo giat;
#   - KHONG "bum"/"huych" tram o dau tieng; ram (neu co) den TRE va len tu tu nhu sam xa.
# Muc tieu do: nang luong 30 ms dau / 300 ms dau <= ~0,3; he so dinh 150 ms dau thap; van giu quy trinh (stereo 0,1-0,3, dai dien thoai,
# 3 bien the khac cau truc, khong nguoc pha, khong truot cao do nhanh).
#
# Ba MAU:
#   A "set_xe_troi"   - tia set XE TOAC khong khi "rẹẹẹt" (xe dai dac) + xeo xeo + sam ran xa den tre; trung = "xeee" xeo bung; lan = "ret" ngan.
#   B "ho_quang_re"   - ho quang dien "zzzzt" re re giat + xet xet; trung = "zzt-xeo" chay; lan = "zt"; qua cau = re re ho quang + xet.
#   C "dong_dien_chay" - dong dien CHAY doc tia "xet-xet-xet" ngat quang chay tu tay toi dich (lech trai -> phai) + vo vo nen; trung = xet don
#                        dap + xeo; lan = chuoi xet ngan chay sang ben kia.
#
# Chay: python CongCu/AmThanh/tieng_giat_set_2.py [thu_muc_ra] [loc_ten_mau...]   (mac dinh CongCu/AmThanh/GiatSetThu2)

import os, sys
import numpy as np
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import tieng_bam as tb
import tieng_qua_cau_lua_3 as q3
import tieng_tang_hinh as th
import tieng_giat_set as g1

SR = tb.SR
PI2 = tb.PI2
GIAY_VONG = 3.0
loc, dai, thap, cao, cham = q3.loc, q3.dai, q3.thap, q3.cao, q3.cham
on2, loc2, quet2, keo_lech, xoay, bao_len_xuong = th.on2, th.loc2, th.quet2, th.keo_lech, th.xoay, th.bao_len_xuong
xeo, vo_vo = g1.xeo, g1.vo_vo


def bao_len(t, bd, len_, giu, tat):
    """Bao: len tuyen tinh len_ (10-30 ms - KHONG phai xung), giu, tat mu."""
    tt = t - bd
    return np.clip(tt / len_, 0, 1) * np.exp(-np.maximum(tt - len_ - giu, 0) / tat) * (tt >= 0)


def ret(n, r, bao, mat_do=5000, chung=0.35, giat=55.0):
    """"RET" XE TOAC: xeo DAY DAC hai dai (sang 900-3400 + than 350-1100) - tieng khong khi bi xe doc tia set."""
    t = np.arange(n) / SR
    x = xeo(n, r, bao, mat_do, (900, 3300), chung, giat) * 0.75
    x += xeo(n, r, bao, mat_do * 0.7, (330, 1050), chung, giat * 0.7) * 1.0     # (do lan 1: than 0,7 -> 200 Hz-1 kHz 28-43%)
    return x * bao[:, None] / (np.max(bao) + 1e-12)


def ngat_quang(n, r, nhip=(12, 35), do_dai=(0.006, 0.03), vong=False):
    """Bao NGAT QUANG "xet-xet-xet": cac xung dong dai 6-30 ms, cach nhau theo nhip khong deu (12-35 / giay), len / xuong 2 ms."""
    b = np.zeros(n)
    tt = r.uniform(0, 0.02)
    while tt < n / SR:
        d = r.uniform(*do_dai)
        i0, i1 = int(tt * SR), int((tt + d) * SR)
        m = max(2, i1 - i0)
        w = np.minimum(1, np.minimum(np.arange(m), m - 1 - np.arange(m)) / (0.002 * SR)) * r.uniform(0.4, 1.0)
        idx = np.arange(i0, i0 + m)
        if vong: idx %= n
        else: w = w[idx < n]; idx = idx[idx < n]
        b[idx] = np.maximum(b[idx], w)
        tt += 1.0 / r.uniform(*nhip) * r.uniform(0.5, 1.5)
    return b


def sam_xa(t, r, bd, giay, nang=1.0):
    """Sam ran XA: on tram 150-600 Hz, LEN TU TU 80-150 ms (khong phai cu dap), cuon theo dot khong deu, hai kenh 50% chung."""
    n = len(t)
    tt = np.maximum(t - bd, 0)
    cuon = np.maximum(cham(n, 5, r) * 0.5 + 0.9, 0.1)
    b = np.clip(tt / r.uniform(0.08, 0.15), 0, 1) ** 1.5 * np.exp(-tt / giay) * cuon * (t >= bd)
    x = loc2(on2(n, r, 0.5), lambda ff: q3.cao(150, 2)(ff) * thap(600, 2)(ff))
    return x / (np.max(np.abs(x)) + 1e-12) * b[:, None] * nang


def hoan_tat(tron, r, giay_vang, muc_vang, dinh_db):
    return th.hoan_tat(tron, r, giay_vang, muc_vang, dinh_db)


# ================================================================== MAU A "SET XE TROI"
def A_phong(hat):
    r = np.random.default_rng(hat)
    giu = r.uniform(0.12, 0.4)
    n = int(SR * (giu + 1.2)); t = np.arange(n) / SR
    tron = np.zeros((n, 2))
    b = bao_len(t, 0.0, r.uniform(0.012, 0.03), giu, r.uniform(0.05, 0.12)) * np.maximum(cham(n, 18, r) * 0.35 + 1, 0.3)
    tron += ret(n, r, b, r.uniform(4000, 7000))
    tron += keo_lech(xeo(n, r, bao_len(t, giu * 0.6, 0.03, 0.05, r.uniform(0.15, 0.35)), r.uniform(1200, 2200)), r.uniform(-0.5, 0.5)) * 0.45
    if r.random() < 0.7:
        tron += sam_xa(t, r, r.uniform(0.08, 0.2), r.uniform(0.3, 0.6), r.uniform(0.3, 0.55))
    tron = keo_lech(tron, np.linspace(r.uniform(-0.4, 0.0), r.uniform(0.0, 0.4), n))
    return hoan_tat(tron, r, r.uniform(0.6, 1.0), r.uniform(0.3, 0.45), -3)


def A_trung(hat):
    r = np.random.default_rng(hat)
    giu = r.uniform(0.06, 0.22)
    n = int(SR * (giu + 0.7)); t = np.arange(n) / SR
    tron = np.zeros((n, 2))
    b = bao_len(t, 0.0, r.uniform(0.01, 0.02), giu, r.uniform(0.06, 0.15))
    tron += ret(n, r, b, r.uniform(2500, 4500), giat=r.uniform(25, 60)) * 0.8
    tron += xeo(n, r, bao_len(t, 0.02, 0.02, giu, r.uniform(0.1, 0.25)), r.uniform(2000, 3500), (900, 3000), giat=20) * 0.6   # "xeee" chay
    tron += vo_vo(t, r, r.uniform(150, 230), bao_len(t, 0.0, 0.01, giu * 0.6, 0.05), giat=60, sau=0.7) * r.uniform(0.2, 0.4)
    return hoan_tat(tron, r, r.uniform(0.4, 0.7), r.uniform(0.25, 0.4), -4)


def A_lan(hat):
    r = np.random.default_rng(hat)
    giu = r.uniform(0.03, 0.2)
    n = int(SR * (giu + 0.45)); t = np.arange(n) / SR
    b = bao_len(t, 0.0, r.uniform(0.008, 0.016), giu, r.uniform(0.03, 0.1))
    if r.random() < 0.5:      # "ret" thu hai (tia giat lai) - bien the khac cau truc (do lan 1: chenh 1,8-2,8 dB)
        b = b + bao_len(t, giu + r.uniform(0.02, 0.06), 0.008, r.uniform(0.02, 0.06), 0.03) * r.uniform(0.4, 0.8)
    tron = ret(n, r, b, r.uniform(3000, 5000))
    tron = keo_lech(tron, np.linspace(r.uniform(-0.6, 0.6), r.uniform(-0.6, 0.6), n))
    return hoan_tat(tron, r, r.uniform(0.3, 0.5), r.uniform(0.2, 0.35), -5)


def A_cau(hat):
    r = np.random.default_rng(hat)
    n = int(SR * GIAY_VONG); t = np.arange(n) / SR
    bung = np.maximum(cham(n, 1.0, r) * 0.45 + 0.8, 0.2) ** 1.5
    tron = xeo(n, r, bung, 1600, (1000, 3300), chung=0.25, giat=30) * 0.7
    # "ret" ngan ngau nhien quanh qua cau (vong lap: dat theo modulo)
    for _ in range(int(GIAY_VONG * 2.5)):
        d = r.uniform(0.04, 0.15); m = int((d + 0.08) * SR)
        x = ret(m, r, bao_len(np.arange(m) / SR, 0.0, 0.01, d, 0.03), r.uniform(2500, 4500)) * r.uniform(0.3, 0.7)
        x = keo_lech(x, r.uniform(-0.8, 0.8))
        i = int(r.uniform(0, GIAY_VONG) * SR); idx = (i + np.arange(m)) % n
        tron[idx] += x
    tron += vo_vo(t, r, 165.0, bung * 0.5, giat=25, sau=0.6, vong=True) * 0.25
    tron = xoay(tron, round(0.7 * GIAY_VONG) / GIAY_VONG, 0.4, r)
    return q3.chuan(tron, -6, vong=True)


# ================================================================== MAU B "HO QUANG RE"
def B_phong(hat):
    r = np.random.default_rng(hat)
    giu = r.uniform(0.15, 0.7)        # (do lan 2: 0,25-0,55 -> chenh bien the 1,9-2,8 dB)
    n = int(SR * (giu + 0.7)); t = np.arange(n) / SR
    tron = np.zeros((n, 2))
    b = bao_len(t, 0.0, r.uniform(0.015, 0.03), giu, r.uniform(0.04, 0.1))
    tron += vo_vo(t, r, r.uniform(140, 210), b, giat=r.uniform(20, 50), sau=0.75) * 0.75
    tron += xeo(n, r, b * np.maximum(cham(n, 12, r) * 0.5 + 0.8, 0.1), r.uniform(1500, 3000), chung=0.3) * 0.6
    tron += ret(n, r, bao_len(t, 0.0, 0.012, r.uniform(0.04, 0.12), 0.04), 3500) * r.uniform(0.3, 0.6)
    if r.random() < 0.5:      # ho quang tat roi chap lai mot nhip
        bd2 = giu * r.uniform(0.4, 0.8)
        tron *= (1 - 0.85 * bao_len(t, bd2, 0.004, r.uniform(0.03, 0.07), 0.01))[:, None]
    tron = keo_lech(tron, np.linspace(r.uniform(-0.4, 0.0), r.uniform(0.0, 0.4), n))
    return hoan_tat(tron, r, r.uniform(0.5, 0.8), r.uniform(0.3, 0.45), -3)


def B_trung(hat):
    r = np.random.default_rng(hat)
    giu = r.uniform(0.08, 0.25)
    n = int(SR * (giu + 0.7)); t = np.arange(n) / SR
    tron = np.zeros((n, 2))
    tron += vo_vo(t, r, r.uniform(160, 240), bao_len(t, 0.0, 0.01, giu * 0.5, 0.05), giat=60, sau=0.7) * 0.6
    tron += xeo(n, r, bao_len(t, 0.005, 0.015, giu, r.uniform(0.1, 0.3)), r.uniform(2500, 4000), (900, 3000), giat=r.uniform(15, 35)) * 0.8
    return hoan_tat(tron, r, r.uniform(0.4, 0.7), r.uniform(0.25, 0.4), -4)


def B_lan(hat):
    r = np.random.default_rng(hat)
    giu = r.uniform(0.04, 0.14)
    n = int(SR * (giu + 0.45)); t = np.arange(n) / SR
    b = bao_len(t, 0.0, 0.01, giu, 0.03)
    tron = vo_vo(t, r, r.uniform(170, 260), b, giat=60, sau=0.65) * 0.7
    tron += xeo(n, r, b + 0.02, r.uniform(1500, 3000), chung=0.3) * 0.5
    tron = keo_lech(tron, np.linspace(r.uniform(-0.6, 0.6), r.uniform(-0.6, 0.6), n))
    return hoan_tat(tron, r, r.uniform(0.3, 0.5), r.uniform(0.2, 0.35), -5)


def B_cau(hat):
    r = np.random.default_rng(hat)
    n = int(SR * GIAY_VONG); t = np.arange(n) / SR
    bung = np.maximum(cham(n, 1.2, r) * 0.4 + 0.85, 0.25) ** 1.5
    tron = vo_vo(t, r, 160.0, bung, giat=22, sau=0.6, tram=1.2, so=3, vong=True) * 0.5
    tron += xeo(n, r, bung * np.maximum(ngat_quang(n, r, (8, 20), (0.02, 0.08), vong=True), 0.25), 1800, chung=0.25) * 0.65
    tron = xoay(tron, round(0.7 * GIAY_VONG) / GIAY_VONG, 0.45, r)
    return q3.chuan(tron, -6, vong=True)


# ================================================================== MAU C "DONG DIEN CHAY"
def C_phong(hat):
    r = np.random.default_rng(hat)
    giu = r.uniform(0.3, 0.6)
    n = int(SR * (giu + 0.7)); t = np.arange(n) / SR
    tron = np.zeros((n, 2))
    b = bao_len(t, 0.0, r.uniform(0.015, 0.03), giu, r.uniform(0.06, 0.12))
    ng = ngat_quang(n, r, (r.uniform(12, 20), r.uniform(25, 40)))
    tron += xeo(n, r, b * np.maximum(ng, 0.12), r.uniform(3000, 5000), (700, 3200), chung=0.3, giat=40)
    tron += xeo(n, r, b * np.maximum(ng, 0.12), 2500, (330, 1050), chung=0.3, giat=40) * 0.9   # than (do lan 1: 200 Hz-1 kHz 18-38%)
    tron += vo_vo(t, r, r.uniform(150, 220), b * np.maximum(ng, 0.3), giat=50, sau=0.5) * r.uniform(0.3, 0.5)
    # dong dien CHAY tu tay ra xa: lech trai -> phai (hoac nguoc lai) trong thoi gian giu
    huong = r.choice([-1, 1])
    tron = keo_lech(tron, huong * np.clip(np.linspace(-0.6, 0.6 + 0.6 * (n / SR - giu) / giu, n), -0.8, 0.8))
    return hoan_tat(tron, r, r.uniform(0.5, 0.8), r.uniform(0.3, 0.45), -3)


def C_trung(hat):
    r = np.random.default_rng(hat)
    giu = r.uniform(0.08, 0.25)
    n = int(SR * (giu + 0.7)); t = np.arange(n) / SR
    tron = np.zeros((n, 2))
    ng = ngat_quang(n, r, (r.uniform(20, 30), r.uniform(35, 50)), (0.006, 0.02))
    b = bao_len(t, 0.0, 0.012, giu, r.uniform(0.08, 0.2))
    tron += xeo(n, r, b * np.maximum(ng, 0.2), r.uniform(3500, 5500), (800, 3200), chung=0.3) * 0.9
    tron += xeo(n, r, b * np.maximum(ng, 0.2), 2800, (330, 1050), chung=0.3) * 0.9
    tron += xeo(n, r, bao_len(t, 0.03, 0.03, giu, r.uniform(0.1, 0.25)), 2500, (900, 2800), giat=18) * 0.5
    tron += vo_vo(t, r, r.uniform(160, 240), b * 0.6, giat=50, sau=0.6) * r.uniform(0.2, 0.4)
    return hoan_tat(tron, r, r.uniform(0.4, 0.7), r.uniform(0.25, 0.4), -4)


def C_lan(hat):
    r = np.random.default_rng(hat)
    giu = r.uniform(0.06, 0.18)
    n = int(SR * (giu + 0.45)); t = np.arange(n) / SR
    ng = ngat_quang(n, r, (25, 45), (0.005, 0.015))
    b = bao_len(t, 0.0, 0.008, giu, 0.04)
    tron = xeo(n, r, b * np.maximum(ng, 0.15), r.uniform(3000, 5000), (800, 3200), chung=0.3)
    tron += xeo(n, r, b * np.maximum(ng, 0.15), 2500, (330, 1050), chung=0.3) * 0.9
    tron += vo_vo(t, r, r.uniform(170, 250), b * 0.5, giat=60, sau=0.5) * 0.25
    tron = keo_lech(tron, np.linspace(r.uniform(-0.7, 0.0), r.uniform(0.0, 0.7), n) * r.choice([-1, 1]))
    return hoan_tat(tron, r, r.uniform(0.3, 0.5), r.uniform(0.2, 0.35), -5)


def C_cau(hat):
    r = np.random.default_rng(hat)
    n = int(SR * GIAY_VONG); t = np.arange(n) / SR
    bung = np.maximum(cham(n, 1.0, r) * 0.45 + 0.8, 0.2) ** 1.5
    ng = ngat_quang(n, r, (10, 30), (0.008, 0.04), vong=True)
    tron = xeo(n, r, bung * np.maximum(ng, 0.2), 2500, (800, 3200), chung=0.25, giat=35) * 0.8
    tron += vo_vo(t, r, 170.0, bung * np.maximum(ng, 0.4), giat=30, sau=0.5, so=3, vong=True) * 0.35
    tron = xoay(tron, round(0.7 * GIAY_VONG) / GIAY_VONG, 0.45, r)
    return q3.chuan(tron, -6, vong=True)


MAU = {
    'A_set_xe_troi': (A_phong, A_trung, A_lan, A_cau, 12100),
    'B_ho_quang_re': (B_phong, B_trung, B_lan, B_cau, 12300),
    'C_dong_dien_chay': (C_phong, C_trung, C_lan, C_cau, 12500),
}


# ================================================================== MAU B2 "ZZZT" (nguoi dung chon B nhung: "cho tieng ZZZZZZT nhieu hon va
# KEO DAI hon chut, GIAM HAN tieng RE di" - GIU ban B, ban moi khong on thi dung lai B).
# "Re" cua B = lop XEO (phong dien li ti - tieng lao xao nhu radio mat song) + ho quang GIAT manh (sau 0,75, giat 20-60 Hz) + bao hoa tanh 2,0.
# B2: ho quang la CHINH, deu va tron hon (sau 0,3, giat 8-14 Hz, bao hoa 1,2, dai sang 0,45); lop xeo con ~1/4; dai hon (phong 0,45-0,9 s,
# trung 0,2-0,4, lan 0,12-0,25, len 15-25 ms); "t" cuoi = TAT GON 15-25 ms (khong co cu no).
def zzz(t, r, f0, bao, so=None, vong=False):
    return vo_vo(t, r, f0, bao, giat=r.uniform(8, 14), sau=0.3, tram=1.1, so=so, vong=vong, bao_hoa=1.2, sang=0.45)


def B2_phong(hat):
    r = np.random.default_rng(hat)
    giu = r.uniform(0.45, 0.9)
    n = int(SR * (giu + 0.6)); t = np.arange(n) / SR
    tat = r.uniform(0.015, 0.025)
    b = np.clip(t / r.uniform(0.015, 0.025), 0, 1) * np.clip((giu - t) / tat, 0, 1)
    if r.random() < 0.5:      # do to "phong" len giua chung (ho quang manh dan) - bien the khac cau truc
        b = b * (0.75 + 0.25 * np.clip(t / giu, 0, 1))
    tron = zzz(t, r, r.uniform(140, 200), b)
    tron += xeo(n, r, b, r.uniform(800, 1500), chung=0.3, giat=20) * 0.15
    tron = keo_lech(tron, np.linspace(r.uniform(-0.4, 0.0), r.uniform(0.0, 0.4), n))
    return hoan_tat(tron, r, r.uniform(0.5, 0.8), r.uniform(0.3, 0.45), -3)


def B2_trung(hat):
    r = np.random.default_rng(hat)
    giu = r.uniform(0.2, 0.4)
    n = int(SR * (giu + 0.6)); t = np.arange(n) / SR
    b = np.clip(t / 0.012, 0, 1) * np.clip((giu - t) / 0.02, 0, 1)
    tron = zzz(t, r, r.uniform(170, 240), b)
    tron += xeo(n, r, bao_len(t, 0.0, 0.01, giu * 0.5, r.uniform(0.08, 0.2)), r.uniform(1200, 2200), (900, 3000), giat=15) * 0.22  # chay nhe
    return hoan_tat(tron, r, r.uniform(0.4, 0.7), r.uniform(0.25, 0.4), -4)


def B2_lan(hat):
    r = np.random.default_rng(hat)
    giu = r.uniform(0.12, 0.25)
    n = int(SR * (giu + 0.45)); t = np.arange(n) / SR
    b = np.clip(t / 0.01, 0, 1) * np.clip((giu - t) / 0.018, 0, 1)
    tron = zzz(t, r, r.uniform(180, 260), b, so=2)
    tron += xeo(n, r, b, 1000, chung=0.3) * 0.12
    tron = keo_lech(tron, np.linspace(r.uniform(-0.6, 0.6), r.uniform(-0.6, 0.6), n))
    return hoan_tat(tron, r, r.uniform(0.3, 0.5), r.uniform(0.2, 0.35), -5)


def B2_cau(hat):
    r = np.random.default_rng(hat)
    n = int(SR * GIAY_VONG); t = np.arange(n) / SR
    bung = np.maximum(cham(n, 1.2, r) * 0.35 + 0.9, 0.3) ** 1.5
    tron = zzz(t, r, 160.0, bung, so=3, vong=True) * 0.8
    tron += xeo(n, r, bung * np.maximum(ngat_quang(n, r, (6, 14), (0.03, 0.1), vong=True), 0.2), 900, chung=0.25) * 0.2
    tron = xoay(tron, round(0.7 * GIAY_VONG) / GIAY_VONG, 0.45, r)
    return q3.chuan(tron, -6, vong=True)


MAU['B2_zzzt'] = (B2_phong, B2_trung, B2_lan, B2_cau, 12700)


# ================================================================== QUA CAU DIEN BAN 3 (nguoi dung nghe ghep B2: GIU phan Giut set 0-4 s, phan
# Qua cau dien "nghe rat do - con qua MONG va qua MONO, cho day hon va stereo hon").
# Vi sao mong / mono: B2_cau chi co dai 200 Hz - 1 kHz (75%, 1-4 kHz 14%, > 4 kHz 0% - nhu qua dien thoai), 3 ho quang gan nhau o giua, tuong
# quan 0,37; trong ghep con bi dat lech mot ben x0,7; moi 0,4 s chong mot tieng PHONG dai 0,45-0,9 s len nhau thanh mot mo.
# Ban 3: 4 HO QUANG quanh qua cau, MOI CAI tu QUAY trai <-> phai voi toc rieng (so chu ky nguyen trong vong) - tieng dien chay vong quanh;
# THAN tram vo vo 140 Hz (< 200 Hz ~20%); lop "sss" dien sang MIN (on dai 1,4-3,4 kHz mat do rat cao - khong lao xao) rong hai ben; xung "zzt"
# manh len o cho ngau nhien; vang stereo tron vong (moi kenh mot xung). Luot ban cua qua cau dung tieng LAN ngan (khong phai PHONG dai).
def B3_cau(hat):
    r = np.random.default_rng(hat)
    n = int(SR * GIAY_VONG); t = np.arange(n) / SR
    tron = np.zeros((n, 2))
    bung = np.maximum(cham(n, 1.0, r) * 0.3 + 0.9, 0.35) ** 1.5
    # 4 ho quang, moi cai mot cao do, quay quanh voi toc rieng (chu ky nguyen trong vong lap)
    for j, (f0, vong_quay) in enumerate(((150.0, 1), (186.0, 2), (228.0, 3), (122.0, 1))):
        x = vo_vo(t, r, f0, bung * np.maximum(cham(n, 0.8, r) * 0.3 + 0.8, 0.2), giat=r.uniform(8, 14), sau=0.3, tram=1.2,
                  so=1, vong=True, bao_hoa=1.3, sang=0.95)
        x = x.mean(axis=1)                                  # mot nguon don...
        # ...LUON lech mot ben (trai / phai xen ke), dao +-0,3 quanh cho do (do lan 1: quay het -0,85..0,85 -> qua giua nhieu, tuong quan 0,43)
        lech = (0.65 if j % 2 == 0 else -0.65) + np.sin(PI2 * vong_quay / GIAY_VONG * t + r.uniform(0, PI2)) * 0.3
        g = (lech + 1) * np.pi / 4
        tron[:, 0] += x * np.cos(g) * (1.0 if j < 3 else 0.8)
        tron[:, 1] += x * np.sin(g) * (1.0 if j < 3 else 0.8)
    tron /= np.max(np.abs(tron)) + 1e-12
    # than tram: HAI nguon vo vo 141 / 156 Hz (chu ky nguyen trong vong) qua thong thap 700 Hz, mot ben trai mot ben phai - khac cao do nen
    # gop mono khong triet tieu. (do lan 1: mot nguon chung x0,35 -> < 200 Hz chi 6%)
    for f0, lech in ((141.0, -0.6), (156.0, 0.6)):
        pha = PI2 * f0 * t + r.uniform(0, PI2)
        than = sum(np.sin(h * pha + 0.3 * h) / h for h in range(1, 7))
        than = loc(than, thap(700, 2)) * bung * np.maximum(cham(n, 1.5, r) * 0.25 + 1, 0.3)
        than /= np.max(np.abs(than)) + 1e-12
        gg = (lech + 1) * np.pi / 4
        tron[:, 0] += than * np.cos(gg) * 0.85; tron[:, 1] += than * np.sin(gg) * 0.85
    # "sss" dien sang min, rong
    sss = loc2(on2(n, r, 0.2), lambda ff: cao(1400, 2)(ff) * thap(3400, 4)(ff))
    sss *= (np.maximum(cham(n, 3, r) * 0.4 + 0.8, 0.1) * bung)[:, None]
    tron += sss / (np.max(np.abs(sss)) + 1e-12) * 1.1     # (do lan 2: x0,6 -> 1-4 kHz 13%)
    # xung "zzt" manh len o cho ngau nhien
    for _ in range(int(GIAY_VONG * 2)):
        d = r.uniform(0.08, 0.25); m = int((d + 0.05) * SR); tm = np.arange(m) / SR
        bz = np.clip(tm / 0.01, 0, 1) * np.clip((d - tm) / 0.02, 0, 1)
        z = vo_vo(tm, r, r.uniform(170, 250), bz, giat=12, sau=0.3, so=1, bao_hoa=1.3, sang=0.6).mean(axis=1)
        z = keo_lech(np.stack([z, z], axis=1), r.uniform(-0.9, 0.9)) * r.uniform(0.3, 0.6)
        i = int(r.uniform(0, GIAY_VONG) * SR); idx = (i + np.arange(m)) % n
        tron[idx] += z
    # vang stereo TRON VONG (khong vet noi)
    m = int(1.0 * SR); tm = np.arange(m) / SR
    for k in range(2):
        h = loc(r.standard_normal(m), lambda f: cao(200, 2)(f) * thap(4000, 2)(f)) * np.exp(-tm / 0.2)
        h[:int(0.015 * SR)] = 0
        h /= np.sqrt(np.sum(h ** 2)) + 1e-12
        hh = np.zeros(n); hh[:m] = h
        tron[:, k] += np.fft.irfft(np.fft.rfft(tron[:, k]) * np.fft.rfft(hh), n) * 0.7
    return q3.chuan(tron, -5, vong=True)


def ghep3(phong, trung, lan, cau):
    """Nhu g1.ghep nhung Qua cau dien: vong lap nguyen do rong (qua cau truoc mat, lech nhe phai), moi luot ban = tieng LAN ngan o qua cau
    + TRUNG o 2-4 ke quanh no."""
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
    tb.cong(out, keo_lech(vong, 0.15) * 0.9, 4.3)
    for i in range(11):
        g = 4.4 + 0.4 * i
        tb.cong(out, keo_lech(lan[i % 3], 0.15) * 0.45, g)
        for j in range(int(r.integers(2, 5))):
            tb.cong(out, keo_lech(trung[(i + j) % 3], r.uniform(-0.6, 0.9)) * 0.45, g + 0.01)
    return out / np.max(np.abs(out)) * 0.95


def do_sung(x):
    """Do 'giong tieng sung': nang luong 30 ms dau / 300 ms dau, he so dinh (dinh / RMS) trong 150 ms dau."""
    m = np.abs(x).max(axis=1)
    e30 = np.sum(m[:int(0.03 * SR)] ** 2); e300 = np.sum(m[:int(0.3 * SR)] ** 2) + 1e-12
    d = m[:int(0.15 * SR)]
    return e30 / e300, d.max() / (np.sqrt(np.mean(d ** 2)) + 1e-12)


if __name__ == '__main__' and '--cau3' in sys.argv:
    # Ban 3 Qua cau dien + ghep voi tieng Giut set B2 (nguoi dung giu)
    ra = os.path.join('CongCu', 'AmThanh', 'GiatSetThu2')
    bo = {'phong': [B2_phong(12700 + 10 * i) for i in range(3)], 'trung': [B2_trung(12800 + 10 * i) for i in range(3)],
          'lan': [B2_lan(12900 + 10 * i) for i in range(3)]}
    cau = B3_cau(13300)
    tb.ghi(os.path.join(ra, 'B3_cau.wav'), cau); th.do('B3_cau', cau)
    th.do('doi chung B2_cau', B2_cau(13000))
    noi = np.max(np.abs(cau[0] - cau[-1])); dd = np.abs(np.diff(cau[:, 0]))
    m = cau.mean(axis=1); e = np.sqrt(np.convolve(m ** 2, np.ones(4410) / 4410, 'same'))[4410:-4410]
    print('   vong lap B3: noi %.4f, buoc quanh noi %.4f / %.4f, bao bien thien %.2f' % (noi, dd[-1], dd[0], (e.max() - e.min()) / e.max()))
    g = ghep3(bo['phong'], bo['trung'], bo['lan'], cau)
    tb.ghi(os.path.join(ra, 'B3_ghep_nghe_thu.wav'), g); th.do('B3_ghep', g)
    sys.exit(0)

if __name__ == '__main__':
    ra = sys.argv[1] if len(sys.argv) > 1 else os.path.join('CongCu', 'AmThanh', 'GiatSetThu2')
    os.makedirs(ra, exist_ok=True)
    chi = sys.argv[2:]
    # doi chung: ban 1 (nguoi dung che "tieng sung")
    for ten, f in (('ban1 A phong', g1.A_phong), ('ban1 A trung', g1.A_trung), ('ban1 B trung', g1.B_trung), ('ban1 C phong', g1.C_phong)):
        s30, dinh = do_sung(f(11110 if 'phong' in ten else 11210))
        print('DOI CHUNG %-14s 30ms/300ms %.2f  he so dinh %.1f' % (ten, s30, dinh))
    for ten, (fp, ft, fl, fc, hat0) in MAU.items():
        if chi and not any(c in ten for c in chi): continue
        bo = {'phong': [fp(hat0 + 10 * i) for i in range(3)], 'trung': [ft(hat0 + 100 + 10 * i) for i in range(3)],
              'lan': [fl(hat0 + 200 + 10 * i) for i in range(3)]}
        cau = fc(hat0 + 300)
        for loai, ds in bo.items():
            for i, x in enumerate(ds):
                tb.ghi(os.path.join(ra, '%s_%s%d.wav' % (ten, loai, i + 1)), x)
                th.do('%s_%s%d' % (ten, loai, i + 1), x)
            print('   chenh bien the %s: %s dB; 30ms/300ms %s; he so dinh %s' % (loai, ' / '.join('%.1f' % c for c in th.chenh_bien_the(ds)),
                  ' / '.join('%.2f' % do_sung(x)[0] for x in ds), ' / '.join('%.1f' % do_sung(x)[1] for x in ds)))
        tb.ghi(os.path.join(ra, '%s_cau.wav' % ten), cau)
        th.do('%s_cau' % ten, cau)
        noi = np.max(np.abs(cau[0] - cau[-1])); dd = np.max(np.abs(np.diff(cau, axis=0)), axis=1)
        m = cau.mean(axis=1); e = np.sqrt(np.convolve(m ** 2, np.ones(4410) / 4410, 'same'))[4410:-4410]
        print('   vong lap %s: noi %.4f (xep hang %.0f%% cac buoc), bao bien thien %.2f' % (ten, noi, 100 * np.mean(dd < noi), (e.max() - e.min()) / e.max()))
        g = g1.ghep(bo['phong'], bo['trung'], bo['lan'], cau)
        tb.ghi(os.path.join(ra, '%s_ghep_nghe_thu.wav' % ten), g)
        th.do('%s_ghep' % ten, g)
