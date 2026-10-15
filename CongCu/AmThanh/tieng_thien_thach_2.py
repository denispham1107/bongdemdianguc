# -*- coding: utf-8 -*-
# TIENG THIEN THACH - BAN 2 (16/10/2026, nguoi dung: "am thanh nghe van con qua mono, tao lai cho that ro la stereo va chan that
# cua thien thach roi tu tren troi xuong va khi va cham vo tung").
# Ban 1 (tieng_thien_thach.py mau A): tieng no dat than vu no GIUA (tuong quan hai kenh 0,57), lua = on loc dai tinh (lua_reo);
# tieng roi khong co huong lao toi. Ban 2 dung lai MO HINH LUA THAT cua Qua cau lua ban 3 (tieng_qua_cau_lua_3.py: liem lua / xi hoi /
# lach tach ngau nhien) + phan rieng cua thien thach:
#   ROI (1,0 s, cham dat o 0,8 s - game tat tieng roi luc no): khoi da chay lao xuong - tieng RIT khong khi xe toac cao do truot xuong
#       (nhu vat roi tu tren cao), tieng GAM lua + liem lua to dan, lach tach tan lua bong ra, VI TRI luot tu mot ben troi vao cho roi.
#   NO: tieng NUT da vo chat + "UM" than + tieng RAM dat (ren lan nhieu dot) + DA VO NAT (nhieu manh lon nho, moi manh mot bo cong huong,
#       nay 2-4 lan) + khoi dat da bung toa hai ben + mua soi roi lop dop + khoi lua bung + tan lua lach tach tat dan.
#   CHAY (vong lap 3 s, vung lua + cay / bia / nha mo bi dot): lua that cung mo hinh.
# Moi tieng roi / no co BA BIEN THE (gieo ngau nhien ca cau truc) - game chon ngau nhien + cao do +-6% nhu Qua cau lua.
# STEREO: moi thanh phan sinh rieng tung kenh (~40% chung de giu "tam"), su kien moi cai mot vi tri.
# Nang luong chinh 200 Hz - 4 kHz, < 200 Hz <= ~25%, cat < 120 Hz (loa dien thoai).
#
# Hai MAU de nguoi dung chon:
#   A "da_lua_roi"        - khoi da chay co vua: rit + gam lua; no da vo + dat da.
#   B "thien_thach_lon"   - khoi lon nang ne: tieng gam tram day hon, rit thap hon, no to hon, ren dat dai, nhieu da vo hon.
#
# Chay: python CongCu/AmThanh/tieng_thien_thach_2.py [thu_muc_ra]   (mac dinh CongCu/AmThanh/ThienThachThu2)

import os, sys
import numpy as np
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import tieng_bam as tb
import tieng_qua_cau_lua_3 as q3

SR = tb.SR
PI2 = tb.PI2
GIAY_ROI = 0.8          # thien thach roi ~0,74-0,84 s (menu 126) - to nhat luc cham dat
DAI_ROI = 1.0
GIAY_CHAY = 3.0
loc, dai, thap, cao, cham = q3.loc, q3.dai, q3.thap, q3.cao, q3.cham


def rit(t, r, f0, f1, giay):
    """Tieng RIT khong khi xe toac quanh khoi da: on dai hep (Q cao) tam truot f0 -> f1 (thap dan khi lao toi) + vai hoa am,
    rung hon loan; moi kenh tam lech nhau mot chut."""
    n = len(t)
    ra = np.zeros((n, 2))
    x = np.clip(t / giay, 0, 1)
    for k in range(2):
        fc = f0 * (f1 / f0) ** (x ** 0.8) * (1 + 0.025 * (k - 0.5)) * (1 + 0.02 * cham(n, 18, r))
        # loc dai hep thay doi theo thoi gian: chia khung 10 ms, moi khung loc FFT voi tam rieng roi ghep chong (cua so Hann)
        z = r.standard_normal(n)
        y = np.zeros(n)
        buoc = int(0.01 * SR); dk = buoc * 2
        win = np.hanning(dk)
        for i in range(0, n - dk, buoc):
            seg = z[i:i + dk] * win
            f = np.fft.rfftfreq(dk, 1 / SR)
            fci = fc[i + buoc]
            h = dai(fci, 9.0)(f) + 0.35 * dai(fci * 2.01, 9.0)(f)
            y[i:i + dk] += np.fft.irfft(np.fft.rfft(seg) * h, dk)
        ra[:, k] = y / (y.std() + 1e-12) * (0.7 + 0.3 * np.maximum(cham(n, 12, r) * 0.5 + 0.6, 0))
    return ra


def pan_quet(x, p):
    """Doi vi tri stereo theo thoi gian (p mang -1..1), giu cong suat."""
    g = (np.clip(p, -1, 1) + 1) * np.pi / 4
    y = x.copy()
    y[:, 0] *= np.cos(g) * np.sqrt(2)
    y[:, 1] *= np.sin(g) * np.sqrt(2)
    return y


def da_vo(tron, r, so, bd0, bd1, nang=1.0, to=1.0):
    """DA VO NAT: manh lon (cong huong thap 400-1200 Hz, tat lau) lan manh nho (sang, ngan); moi manh mot cho, NAY 2-4 lan."""
    for _ in range(so):
        lon = r.random() < 0.35
        bd = r.uniform(bd0, bd1)
        lech = np.clip(r.normal(0, 0.65), -1, 1)
        goc = r.uniform(380, 1100) if lon else r.uniform(1000, 2600)
        fcs = [goc * m for m in (1.0, r.uniform(1.4, 2.0), r.uniform(2.3, 3.1), r.uniform(3.4, 4.6))]
        a = r.uniform(0.35, 0.9) * nang * (1.6 if lon else 1.0) * np.exp(-bd * 1.0)
        khoang = r.uniform(0.07, 0.2) * (1.3 if lon else 1.0)
        for lan in range(r.integers(2, 5)):
            d = int(0.09 * SR); t = np.arange(d) / SR
            x = np.zeros(d)
            for j, fc in enumerate(fcs):
                x += np.sin(PI2 * fc * r.uniform(0.97, 1.03) * t + r.uniform(0, PI2)) * np.exp(-t / (r.uniform(0.006, 0.02) * (1.8 if lon else 1.0))) / (1 + j)
            x += loc(r.standard_normal(d), cao(900 if lon else 1600, 2)) * np.exp(-t / 0.002) * 0.7
            tb.cong(tron, tb.dat_vi_tri(x * a, lech), bd)
            bd += khoang; khoang *= r.uniform(0.5, 0.75); a *= r.uniform(0.35, 0.6)
            if bd >= len(tron) / SR - 0.1: break


def dat_bung(t, r, giay):
    """Khoi dat da bi hat tung: on dai trung-thap 250-2000 Hz, bung nhanh, tat 0,3-0,6 s, toa hai ben (moi kenh mot nguon, lech thoi gian)."""
    n = len(t)
    ra = np.zeros((n, 2))
    for k in range(2):
        tre = r.uniform(0, 0.025)
        e = np.clip((t - tre) / 0.012, 0, 1) * np.exp(-np.maximum(t - tre, 0) / giay)
        e *= 0.75 + 0.5 * np.maximum(cham(n, 14, r) * 0.4 + 0.5, 0)
        ra[:, k] = loc(r.standard_normal(n), lambda f: cao(250, 2)(f) * thap(2000, 2)(f)) * e
    return ra


# ================================================================== ROI
def tieng_roi(hat, kieu):
    r = np.random.default_rng(hat)
    n = int(SR * DAI_ROI); t = np.arange(n) / SR
    x = np.clip(t / GIAY_ROI, 0, 1)
    lon = kieu == "B"
    tron = np.zeros((n, 2))
    # lua chay tren khoi da: liem lua + xi + lach tach, to dan (mu 1,6-2,4 tuy bien the)
    mu = r.uniform(1.6, 2.4)
    bm = np.where(t <= GIAY_ROI, x ** mu, 1.0) * np.clip(t / 0.03, 0, 1)
    tron += q3.lua(n, hat + 1, liem=1.3 if lon else 1.1, xi=0.45, tach=0.8, mat_do=r.uniform(90, 160), bao_mat=bm,
                   tam_liem=r.uniform(22, 30) if lon else r.uniform(30, 42), dai_den=800 if lon else 1100, chung=0.4)
    # gam: lop liem lua tram, cham (khoi lon cuon khong khi)
    tron += q3.lua(n, hat + 2, liem=1.0, xi=0.0, tach=0.0, mat_do=1, bao_mat=bm, tam_liem=16 if lon else 20, dai_den=600, chung=0.35) * (0.9 if lon else 0.6)
    # rit khong khi xe toac: cao do truot xuong
    f0 = r.uniform(1500, 2100) * (0.8 if lon else 1.0); f1 = f0 * r.uniform(0.45, 0.6)
    tron += rit(t, r, f0, f1, GIAY_ROI) * (np.where(t <= GIAY_ROI, x ** (mu + 0.3), 1.0) * np.clip(t / 0.05, 0, 1))[:, None] * r.uniform(0.25, 0.4)
    # gio xe rong
    tron += q3.gio_xe(n, r, 500, 2400, 2.0) * bm[:, None] * 0.35
    # vi tri: lao tu mot ben troi vao cho roi (tam) luc cham dat
    p0 = r.choice([-1, 1]) * r.uniform(0.45, 0.8)
    tron = pan_quet(tron, p0 * (1 - np.clip(t / GIAY_ROI, 0, 1)) ** 1.5)
    tron = q3.vang_uot(tron, 0.35, r, 0.25)
    return q3.chuan(tron, -3)


# ================================================================== NO
def tieng_no(hat, kieu):
    r = np.random.default_rng(hat)
    lon = kieu == "B"
    to = (1.25 if lon else 1.0) * r.uniform(0.75, 1.3)
    g = 3.6 if lon else 3.0
    n = int(SR * g); t = np.arange(n) / SR
    tron = np.zeros((n, 2))
    # nut da vo chat (sang hon qua cau lua - da) + "um" than tram hon + ren dat lan nhieu dot (dai hon)
    tron += q3.nut(t, r) * r.uniform(1.1, 1.7)
    vo = np.zeros((n, 2))
    for k in range(2):
        vo[:, k] = loc(r.standard_normal(n), lambda f: cao(1200, 2)(f) * thap(7000, 2)(f)) * np.exp(-t / r.uniform(0.012, 0.025))
    tron += vo * 0.8
    tron += q3.um(t, r, r.uniform(110, 170) * (0.85 if lon else 1.0), r.uniform(50, 70), 0.22 * to) * 0.6 * to
    tron += q3.ren_lan(t, r, 0.45 * to, dot=r.integers(3, 8)) * 0.6 * to
    tron += dat_bung(t, r, r.uniform(0.3, 0.6) * to) * 0.55
    # khoi lua bung (it hon qua cau lua - day la da)
    tre = r.uniform(0.0, 0.06)
    bm = np.clip((t - tre) / 0.04, 0, 1) * np.exp(-np.maximum(t - tre, 0) / (0.45 * to))
    tron += q3.lua(n, hat + 1, liem=1.2, xi=0.45, tach=0.5, mat_do=150, bao_mat=bm, tam_liem=r.uniform(26, 40), dai_den=1100, chung=0.4) * 0.6
    # gio / bui toa hai ben
    for k, p in ((0, -0.95), (1, 0.95)):
        b = np.minimum(1, t / 0.02) * np.exp(-t / r.uniform(0.15, 0.3))
        tron += tb.dat_vi_tri(q3.gio_xe(n, r, 2200, 400, 4.0)[:, k] * b, p) * 0.3
    da_vo(tron, r, int(r.integers(10, 18) * to), 0.02, 0.5, nang=1.0)
    tron += q3.mua_dat(n, r, r.uniform(0.25, 0.45), 0.6 * to, r.uniform(220, 380), nang=r.uniform(0.9, 1.4))
    # no phu (35%): mot tang da / tui lua vo them o mot ben
    if r.random() < 0.35:
        bd = r.uniform(0.15, 0.5); t2 = t[:n - int(bd * SR)]
        phu = q3.nut(t2, r) * 0.5 + q3.um(t2, r, r.uniform(150, 220), 80, 0.07) * 0.3
        p2 = r.uniform(-0.85, 0.85)
        phu[:, 0] *= np.sqrt(1 - p2); phu[:, 1] *= np.sqrt(1 + p2)
        tb.cong(tron, phu, bd)
    # than lua lach tach tat dan
    be = np.clip((t - 0.25) / 0.3, 0, 1) * np.exp(-np.maximum(t - 0.55, 0) / (0.6 * to))
    tron += q3.lua(n, hat + 2, liem=0.35, xi=0.2, tach=1.0, mat_do=r.uniform(35, 60), bao_mat=be, nang_bop=0.2) * 0.6
    tron = q3.vang_uot(tron, 0.9, r, 0.5)
    return q3.chuan(tron, -1)


# ================================================================== CHAY (vong lap)
def tieng_chay(hat, kieu):
    r = np.random.default_rng(hat)
    n = int(SR * GIAY_CHAY)
    x = q3.lua(n, hat + 1, liem=1.0, xi=0.4, tach=1.0, mat_do=50, vong=True, tam_liem=28, nang_bop=0.15, dai_den=900, chung=0.4)
    x += q3.lua(n, hat + 2, liem=0.7, xi=0.0, tach=0.0, mat_do=1, vong=True, tam_liem=18, dai_den=600, chung=0.35) * (0.7 if kieu == "B" else 0.5)
    bung = np.maximum(cham(n, 0.7, r) * 0.35 + 0.8, 0.2) ** 1.4
    x *= bung[:, None]
    x += q3.phut_vat(n, r, 2.5, vong=True) * 0.8
    return q3.chuan(x, -6, vong=True)


def ghep(rois, nos, chay):
    """Nghe nhu trong game: 3 thien thach roi lan luot (cach 0,35 s) o trai / giua / phai, no, roi lua chay 3 s."""
    out = np.zeros((int(SR * 7.0), 2))
    cho = (-0.6, 0.0, 0.6)
    for i in range(3):
        t0 = i * 0.35
        p = cho[i]
        ro = rois[i][:int(GIAY_ROI * SR)].copy() * 0.9
        ro[:, 0] *= np.sqrt(1 - p); ro[:, 1] *= np.sqrt(1 + p)
        tb.cong(out, ro, t0)
        o = nos[i].copy() * (1.0 if i == 0 else 0.8)
        o[:, 0] *= np.sqrt(1 - p); o[:, 1] *= np.sqrt(1 + p)
        tb.cong(out, o, t0 + GIAY_ROI)
    lap = np.concatenate([chay, chay])[:int(SR * 4.5)].copy()
    tt = np.arange(len(lap)) / SR
    lap *= (np.clip(tt / 0.4, 0, 1) * np.clip((4.5 - tt) / 1.5, 0, 1))[:, None]
    tb.cong(out, lap * 0.6, 1.0)
    return out


def phan_tich(ten, x):
    m = x.mean(axis=1)
    P = np.abs(np.fft.rfft(m)) ** 2
    f = np.fft.rfftfreq(len(m), 1 / SR)
    pt = [P[(f >= a) & (f < b)].sum() / P.sum() * 100 for a, b in [(0, 200), (200, 1000), (1000, 4000), (4000, 22050)]]
    l, rr = x[:, 0], x[:, 1]
    tq = float(np.sum(l * rr) / np.sqrt(np.sum(l * l) * np.sum(rr * rr)))
    print('%-30s %.2f s  <200/200-1k/1-4k/>4k: %s  tuong quan %.2f  dinh %.2f' % (ten, len(x) / SR,
          ' / '.join('%.0f%%' % p for p in pt), tq, np.max(np.abs(x))))


if __name__ == '__main__':
    ra = sys.argv[1] if len(sys.argv) > 1 else os.path.join('CongCu', 'AmThanh', 'ThienThachThu2')
    os.makedirs(ra, exist_ok=True)
    for kieu, ten in (("A", "A_da_lua_roi"), ("B", "B_thien_thach_lon")):
        lech = 0 if kieu == "A" else 500
        rois = [tieng_roi(1100 + 10 * i + lech, kieu) for i in range(3)]
        nos = [tieng_no(1200 + 10 * i + lech, kieu) for i in range(3)]
        chay = tieng_chay(1300 + lech, kieu)
        noi = np.max(np.abs(chay[0] - chay[-1])); d = np.max(np.abs(np.diff(chay, axis=0)), axis=1)
        print('   vong lap chay %s: noi %.4f, xep hang %.0f%% cac buoc' % (ten, noi, 100 * np.mean(d < noi)))
        for i in range(3):
            tb.ghi(os.path.join(ra, '%s_roi%d.wav' % (ten, i + 1)), rois[i]); phan_tich('%s_roi%d' % (ten, i + 1), rois[i])
            tb.ghi(os.path.join(ra, '%s_no%d.wav' % (ten, i + 1)), nos[i]); phan_tich('%s_no%d' % (ten, i + 1), nos[i])
        tb.ghi(os.path.join(ra, '%s_chay.wav' % ten), chay); phan_tich('%s_chay' % ten, chay)
        g = ghep(rois, nos, chay)
        g = g / np.max(np.abs(g)) * 0.95
        tb.ghi(os.path.join(ra, '%s_ghep_nghe_thu.wav' % ten), g); phan_tich('%s_ghep' % ten, g)
