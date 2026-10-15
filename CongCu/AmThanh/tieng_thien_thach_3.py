# -*- coding: utf-8 -*-
# TIENG THIEN THACH - BAN 3 (16/10/2026). Nguoi dung nghe ban 2 (tieng_thien_thach_2.py): "van chua giong tieng thien thach bay roi
# va vo tung lam, tao lai".
# Ban 2 yeu o dau (do + soi lai): (1) roi to dan DEU (x^2) - vat lao toi that thi to theo 1 / khoang cach, DON DAP o phut chot;
# (2) khong co dau hieu "dang lao toi gan": tieng phai SANG DAN (xa thi duc - khong khi nuot dai cao, gan thi choi); (3) tieng rit
# qua mo; (4) tieng no nghieng ve lua mem (80% nang luong 200 Hz - 1 kHz), thieu cu VA CHAM nang + tieng DA NGHIEN "rac rac".
# Ban 3 - bo sung dung cac dau hieu ay:
#   ROI: tieng GAM lao toi (on rong, tan so cat quet 350 Hz -> 4,5 kHz theo khoang cach, to theo 1 / khoang cach), tieng RAM nhu sam
#       (tram, rung hon loan 15-40 Hz, bao hoa -> nghe duoc tren loa nho), lua chay lach tach, tieng RIT ROI (kieu bom roi: hai giong
#       cao do truot xuong 1300 -> 520 Hz, co hoa am - mau A ro, mau B nho), vi tri luot tu tren mot ben vao cho roi.
#   NO: cu NUT + "BUM" than bao hoa (40-60 ms dau cuc manh) + DA NGHIEN (mat do rat day hat va cham 0,8-5 kHz trong 0,3 s, hai kenh rieng)
#       + da vo lon nho NAY + dat da do xuong lop dop 1,5 s + RAM dat lan nhieu dot (nhu sam) + khoi lua bung nho + tan lua.
#   CHAY: giu tieng lua that ban 2 (nguoi dung dong y thay tieng lua chay).
# Moi tieng roi / no BA BIEN THE (gieo ngau nhien ca cau truc) - game chon ngau nhien + cao do +-6%.
#
# Hai MAU:
#   A "dien_anh"  - nhu phim: rit roi RO, gam sang, no cuc manh + vang dai.
#   B "thuc_te"   - it rit, gam tram nhu sam, da nghien + dat da do nhieu, vang ngan hon.
#
# Chay: python CongCu/AmThanh/tieng_thien_thach_3.py [thu_muc_ra]   (mac dinh CongCu/AmThanh/ThienThachThu3)

import os, sys
import numpy as np
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import tieng_bam as tb
import tieng_qua_cau_lua_3 as q3
import tieng_thien_thach_2 as t2

SR = tb.SR
PI2 = tb.PI2
GIAY_ROI = 0.8
DAI_ROI = 1.0
loc, dai, thap, cao, cham = q3.loc, q3.dai, q3.thap, q3.cao, q3.cham


def loc_quet(z, fc, bac=2, buoc_giay=0.01):
    """Loc thong thap co tan so cat THAY DOI theo thoi gian (fc mang): chia khung chong Hann, moi khung loc FFT."""
    n = len(z)
    y = np.zeros(n)
    buoc = int(buoc_giay * SR); dk = buoc * 2
    win = np.hanning(dk)
    f = np.fft.rfftfreq(dk, 1 / SR)
    for i in range(0, n - dk + 1, buoc):
        h = thap(fc[min(n - 1, i + buoc)], bac)(f)
        y[i:i + dk] += np.fft.irfft(np.fft.rfft(z[i:i + dk] * win) * h, dk)
    return y


def gan_dan(t, giay, d0=12.0, d1=0.6):
    """Khoang cach ao tu d0 (luc dau) toi d1 (luc cham dat), van toc deu -> do to ~ 1/d, do sang ~ log."""
    x = np.clip(t / giay, 0, 1)
    d = d0 + (d1 - d0) * x
    to = (d1 / d)
    sang = 1 - np.log(d / d1) / np.log(d0 / d1)       # 0 xa .. 1 gan
    return to, sang


def gam_lao_toi(t, r, giay, f_xa=350, f_gan=4500):
    """Tieng GAM lao toi: on rong qua loc thong thap quet theo khoang cach, moi kenh mot nguon + 40% chung, rung hon loan."""
    n = len(t)
    to, sang = gan_dan(t, giay)
    fc = f_xa * (f_gan / f_xa) ** np.clip(sang, 0, 1)
    zc = r.standard_normal(n)
    ra = np.zeros((n, 2))
    for k in range(2):
        z = 0.4 * zc + 0.8 * r.standard_normal(n)
        y = loc_quet(z, fc * (1 + 0.04 * (k - 0.5)))
        rung = 0.65 + 0.35 * np.maximum(cham(n, 30, r) * 0.5 + 0.6, 0)
        ra[:, k] = y / (y.std() + 1e-12) * rung
    return ra * to[:, None]


def ram_sam(t, r, f0=55, nang=1.0):
    """Tieng RAM nhu sam: sin tram rung hon loan 15-40 Hz + on tram, BAO HOA (tanh) -> sinh hoa am 110-600 Hz."""
    n = len(t)
    ra = np.zeros((n, 2))
    for k in range(2):
        f = f0 * (1 + 0.05 * (k - 0.5)) * (1 + 0.04 * cham(n, 6, r))
        pha = PI2 * np.cumsum(f) / SR + r.uniform(0, PI2)
        rung = np.maximum(cham(n, 35, r) * 0.6 + 0.6, 0)
        x = (np.sin(pha) + 0.4 * loc(r.standard_normal(n), lambda ff: cao(40, 2)(ff) * thap(260, 2)(ff)) * 3) * rung
        ra[:, k] = np.tanh(2.2 * x * nang)
    return ra


def rit_roi(t, r, f0, f1, giay):
    """Tieng RIT ROI kieu bom roi: hai giong (trai / phai lech 1,5%) cao do truot xuong, hoa am 2, 3, rung nhe 6 Hz; to theo khoang cach."""
    n = len(t)
    x = np.clip(t / giay, 0, 1)
    ra = np.zeros((n, 2))
    for k in range(2):
        f = f0 * (f1 / f0) ** (x ** 1.2) * (1 + 0.015 * (k - 0.5)) * (1 + 0.006 * np.sin(PI2 * 6.0 * t + r.uniform(0, PI2)))
        pha = PI2 * np.cumsum(f) / SR + r.uniform(0, PI2)
        s = np.sin(pha) + 0.35 * np.sin(2 * pha + 0.4) + 0.15 * np.sin(3 * pha + 1.0)
        hoi = loc(r.standard_normal(n), dai(1500, 1.5)) * 0.25          # hoi gio trong tieng rit
        ra[:, k] = s + hoi
    to, _ = gan_dan(t, giay, 10.0, 0.8)
    return ra * to[:, None] * np.clip(t / 0.08, 0, 1)[:, None]


def da_nghien(t, r, bd, giay, mat_do=2500, nang=1.0):
    """DA NGHIEN "rac rac": hang nghin hat va cham tat 0,3-2 ms, cong huong 800-5000 Hz, mat do giam nhanh; hai kenh doc lap."""
    n = len(t)
    ra = np.zeros((n, 2))
    e = np.clip((t - bd) / 0.004, 0, 1) * np.exp(-np.maximum(t - bd, 0) / giay)
    for k in range(2):
        p = e * mat_do / SR
        vt = np.nonzero(r.random(n) < p)[0]
        x = np.zeros(n)
        a = np.exp(r.uniform(np.log(0.1), 0, len(vt)))
        x[vt] = a * np.sign(r.uniform(-1, 1, len(vt)))
        # moi hat la xung -> loc dai rong 800-5000 Hz + vai cong huong da (1,3 / 2,2 / 3,4 kHz)
        y = loc(x, lambda f: cao(800, 2)(f) * thap(5000, 2)(f) * (0.5 + dai(1300, 3)(f) + 0.8 * dai(2200, 3)(f) + 0.6 * dai(3400, 3)(f)))
        ra[:, k] = y
    return ra / (np.max(np.abs(ra)) + 1e-12) * nang


def bum(t, r, f0, f1, giay, nang=1.0):
    """'BUM' va cham: sin tut cao do f0 -> f1 nhanh + on tram, BAO HOA manh trong 40-60 ms dau (cu dap) roi tat dan."""
    n = len(t)
    ra = np.zeros((n, 2))
    for k in range(2):
        f = (f1 + (f0 - f1) * np.exp(-t / 0.035)) * (1 + 0.03 * (k - 0.5))
        pha = PI2 * np.cumsum(f) / SR + r.uniform(0, PI2)
        x = np.sin(pha) + 0.5 * np.sin(2 * pha + 0.2) + 0.6 * loc(r.standard_normal(n), lambda ff: cao(60, 2)(ff) * thap(500, 2)(ff)) * 4
        e = np.exp(-t / giay) * (1 + 2.5 * np.exp(-t / 0.05))
        ra[:, k] = np.tanh(3.0 * x * e * nang) * np.minimum(1, t / 0.0015)
    return ra


# ================================================================== ROI
def tieng_roi(hat, kieu):
    r = np.random.default_rng(hat)
    dien_anh = kieu == "A"
    n = int(SR * DAI_ROI); t = np.arange(n) / SR
    to, sang = gan_dan(t, GIAY_ROI)
    tron = np.zeros((n, 2))
    # (do lan 1: tren 4 kHz 18-25% - choi; gioi han tan so cat 3,2 kHz)
    tron += gam_lao_toi(t, r, GIAY_ROI, f_gan=r.uniform(2600, 3200)) * (1.0 if dien_anh else 0.85)
    ram = ram_sam(t, r, r.uniform(48, 62), nang=r.uniform(0.8, 1.2)) * to[:, None]
    tron += ram * (1.0 if dien_anh else 1.4)
    tron += q3.lua(n, hat + 1, liem=0.8, xi=0.15, tach=0.6, mat_do=r.uniform(100, 180), bao_mat=to / to.max(),
                   tam_liem=r.uniform(26, 38), dai_den=1000, chung=0.4) * 0.5
    f0 = r.uniform(1150, 1450); f1 = f0 * r.uniform(0.38, 0.48)
    tron += rit_roi(t, r, f0, f1, GIAY_ROI) * (r.uniform(0.35, 0.5) if dien_anh else r.uniform(0.08, 0.14))
    # sau 0,8 s (neu game cham dat muon): giu muc, khong tat
    p0 = r.choice([-1, 1]) * r.uniform(0.5, 0.85)
    tron = t2.pan_quet(tron, p0 * (1 - np.clip(t / GIAY_ROI, 0, 1)) ** 1.3)
    tron = q3.vang_uot(tron, 0.4, r, 0.3 if dien_anh else 0.2)
    return q3.chuan(tron, -3)


# ================================================================== NO
def tieng_no(hat, kieu):
    r = np.random.default_rng(hat)
    dien_anh = kieu == "A"
    to = r.uniform(0.75, 1.3)
    n = int(SR * (3.6 if dien_anh else 3.2)); t = np.arange(n) / SR
    tron = np.zeros((n, 2))
    tron += q3.nut(t, r) * r.uniform(1.3, 1.9)
    # (do lan 1: cu dap dau chi bang muc 0,3 s sau - khong noi; nay BUM manh hon, phan sau nho lai)
    tron += bum(t, r, r.uniform(150, 210), r.uniform(48, 62), 0.25 * to * (1.25 if dien_anh else 1.0)) * (1.4 if dien_anh else 1.15)
    tron += da_nghien(t, r, 0.003, r.uniform(0.08, 0.16) * to, mat_do=r.uniform(1800, 3200), nang=1.0 if dien_anh else 1.3)
    tron += q3.ren_lan(t, r, 0.5 * to * (1.3 if dien_anh else 1.0), dot=r.integers(3, 8)) * 0.3
    tron += ram_sam(t, r, r.uniform(45, 58), 0.9) * (np.exp(-t / (0.55 * to)) * np.clip(t / 0.03, 0, 1))[:, None] * 0.45
    tron += t2.dat_bung(t, r, r.uniform(0.25, 0.5) * to) * 0.5
    t2.da_vo(tron, r, int(r.integers(10, 20) * to * (1.0 if dien_anh else 1.3)), 0.02, 0.6, nang=0.65)
    # dat da do xuong lop dop (soi nho mat do cao, keo dai)
    tron += q3.mua_dat(n, r, r.uniform(0.18, 0.35), r.uniform(0.6, 1.0) * to, r.uniform(280, 450), nang=r.uniform(0.6, 0.9) * (0.8 if dien_anh else 1.2))
    bm = np.clip((t - 0.02) / 0.04, 0, 1) * np.exp(-np.maximum(t - 0.02, 0) / (0.4 * to))
    tron += q3.lua(n, hat + 1, liem=1.0, xi=0.4, tach=0.5, mat_do=120, bao_mat=bm, tam_liem=r.uniform(26, 40), dai_den=1000, chung=0.4) * 0.35
    for k, p in ((0, -0.95), (1, 0.95)):
        b = np.minimum(1, t / 0.01) * np.exp(-t / r.uniform(0.12, 0.25))
        tron += tb.dat_vi_tri(q3.gio_xe(n, r, 2600, 450, 4.0)[:, k] * b, p) * 0.3
    if r.random() < 0.35:
        bd = r.uniform(0.12, 0.45); t3 = t[:n - int(bd * SR)]
        phu = q3.nut(t3, r) * 0.6 + da_nghien(t3, r, 0.002, 0.06, 1500, 0.5)
        p2 = r.uniform(-0.85, 0.85)
        phu[:, 0] *= np.sqrt(1 - p2); phu[:, 1] *= np.sqrt(1 + p2)
        tb.cong(tron, phu, bd)
    be = np.clip((t - 0.3) / 0.3, 0, 1) * np.exp(-np.maximum(t - 0.6, 0) / (0.6 * to))
    tron += q3.lua(n, hat + 2, liem=0.3, xi=0.2, tach=1.0, mat_do=r.uniform(30, 55), bao_mat=be, nang_bop=0.2) * 0.45
    tron = q3.vang_uot(tron, 1.2 if dien_anh else 0.7, r, 0.55 if dien_anh else 0.35)
    return q3.chuan(tron, -1)


if __name__ == '__main__':
    ra = sys.argv[1] if len(sys.argv) > 1 else os.path.join('CongCu', 'AmThanh', 'ThienThachThu3')
    os.makedirs(ra, exist_ok=True)
    for kieu, ten in (("A", "A_dien_anh"), ("B", "B_thuc_te")):
        lech = 0 if kieu == "A" else 500
        rois = [tieng_roi(2100 + 10 * i + lech, kieu) for i in range(3)]
        nos = [tieng_no(2200 + 10 * i + lech, kieu) for i in range(3)]
        chay = t2.tieng_chay(1300 + lech, "B")
        for i in range(3):
            tb.ghi(os.path.join(ra, '%s_roi%d.wav' % (ten, i + 1)), rois[i]); t2.phan_tich('%s_roi%d' % (ten, i + 1), rois[i])
            tb.ghi(os.path.join(ra, '%s_no%d.wav' % (ten, i + 1)), nos[i]); t2.phan_tich('%s_no%d' % (ten, i + 1), nos[i])
        tb.ghi(os.path.join(ra, '%s_chay.wav' % ten), chay); t2.phan_tich('%s_chay' % ten, chay)
        g = t2.ghep(rois, nos, chay)
        g = g / np.max(np.abs(g)) * 0.95
        tb.ghi(os.path.join(ra, '%s_ghep_nghe_thu.wav' % ten), g); t2.phan_tich('%s_ghep' % ten, g)
