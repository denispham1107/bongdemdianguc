# -*- coding: utf-8 -*-
# TIENG QUA CAU BANG + MUA BANG - BAN 4 (16/10/2026). Nguoi dung chon mau B "Mua da" cua ban 3 (tieng_qua_cau_bang_3.py) cho CA HAI
# ky nang, nhung: "am thanh co cam giac van con MONG va hoi LEP, tao lai giong vay nhung cho am thanh stereo VANG DAY hon".
# Do ban 3 B: tieng bay 7% nang luong 200 Hz - 1 kHz (91% o 1-4 kHz), tieng ban 11% -> thieu than; vang 0,25-0,5 s o muc 0,15-0,25 -> kho.
# Ban 4 = GIU chat "Mua da" (khong lop lua, khong "piu", khong rit) + lam DAY:
#   - THAN trung-tram: "toc" tram + day hon (150-700 Hz, tat 25-45 ms), lop BANG NGHIEN thu hai o dai thap (350-1300 Hz), tang bang to hon;
#   - tieng bay them lop hoi lanh dai GIUA (350-1100 Hz, muot - khong cat / khong bung nhu lua);
#   - VANG STEREO DAY: vang moi kenh mot xung (phan xa som khac nhau) dai 1,0-1,5 s, muc 0,45-0,6 + lop vang ngan 0,35 s (day "phong");
#   - NHAN DOI lop chinh (ban sao lech 7-15 ms + cao do lech 2-3%, dat lech trai / phai) -> day va rong.
# Do: % nang luong 200 Hz - 1 kHz (muc tieu >= 25-30%), ti le Side/Mid (do rong), duoi vang (thoi gian tu dinh xuong -30 dB), tuong
# quan 0,15-0,45, < 200 Hz <= 25%, > 4 kHz <= ~10%, do phang 150 ms dau (khong co cao do).
#
# Chay: python CongCu/AmThanh/tieng_qua_cau_bang_4.py [thu_muc_ra]   (mac dinh CongCu/AmThanh/QuaCauBangThu4)

import os, sys
import numpy as np
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import tieng_bam as tb
import tieng_qua_cau_lua_3 as q3
import tieng_qua_cau_bang_2 as b2
import tieng_qua_cau_bang_3 as b3

SR = tb.SR
PI2 = tb.PI2
GIAY_BAY = 3.0
GIAY_ROI_MUA = 0.96
loc, dai, thap, cao, cham = q3.loc, q3.dai, q3.thap, q3.cao, q3.cham


def vang_day(tron, r, dai_giay=1.2, muc=0.5):
    """Vang stereo DAY: lop dai (moi kenh mot xung, phan xa som khac nhau) + lop ngan 0,35 s (than "phong")."""
    a = q3.vang_uot(tron, dai_giay, r, muc)
    b = q3.vang_uot(tron, 0.35, r, muc * 0.6)
    n = max(len(a), len(b))
    ra = np.zeros((n, 2)); ra[:len(a)] += a; ra[:len(b)] += b - np.pad(tron, ((0, len(b) - len(tron)), (0, 0)))
    return ra


# BAN "TRONG" (nguoi dung: "them 1 it do am vang TRONG SUOT cua bang de toi nghe") - bat bang co TRONG; tat = ban day duoc gui truoc.
# Chi MOT IT: ban 1 bi che "y chang thuy tinh" vi ngan 20-250 ms, 5 phan am, to - o day 2-3 phan am, tat 60-150 ms, nho hon than ~12-16 dB,
# va mot lop vang dai sang 1,5-3,5 kHz tat ~0,8 s ("vong trong long bang"). Do phang duoi phai giu xa vung thuy tinh (ban 1: 0,03-0,16).
TRONG = False


def ngan_trong(n, r, so=4, bd0=0.0, bd1=0.25, muc=0.13):
    """Vai tieng ngan tinh the KHE (2-3 phan am khong dieu hoa, tat 60-150 ms), moi cai mot cho (lech bang am luong)."""
    ra = np.zeros((n, 2))
    for _ in range(so):
        bd = r.uniform(bd0, bd1)
        d = int(0.5 * SR); t = np.arange(d) / SR
        f0 = np.exp(r.uniform(np.log(1400), np.log(2800)))
        tat = r.uniform(0.06, 0.15)
        x = sum(a * np.sin(PI2 * f0 * ti * r.uniform(0.98, 1.02) * t + r.uniform(0, PI2)) * np.exp(-t / (tat / (1 + 0.6 * j)))
                for j, (ti, a) in enumerate(((1.0, 1.0), (2.41, 0.45), (3.9, 0.2))) if f0 * ti < 4200)
        x *= np.minimum(1, t / 0.002) * r.uniform(0.4, 1.0)
        tb.cong(ra, b2.pan_am_luong(x, r.uniform(-0.8, 0.8)), bd)
    return ra * muc


def vang_trong(tron, r, muc=0.22, giay=0.8):
    """Lop vang SANG (1,5-3,5 kHz) moi kenh mot xung - 'vong trong long bang'. Chi phan uot."""
    m = int(giay * SR); tm = np.arange(m) / SR
    ra = np.zeros((len(tron) + m, 2))
    for k in range(2):
        h = loc(r.standard_normal(m), lambda f: cao(1500, 2)(f) * thap(3500, 4)(f)) * np.exp(-tm / (giay / 5))
        h[:int(0.02 * SR)] = 0
        h /= np.sqrt(np.sum(h ** 2)) + 1e-12
        nn = len(tron) + m
        ra[:, k] = np.fft.irfft(np.fft.rfft(tron[:, k], nn) * np.fft.rfft(h, nn), nn)
    return ra * muc


def them_trong(tron, r, so, bd1=0.25):
    """Neu TRONG: them ngan tinh the khe + vang sang vao tieng (truoc vang_day)."""
    if not TRONG:
        return tron
    tron = tron + ngan_trong(len(tron), r, so, 0.0, bd1)
    v = vang_trong(tron, r)
    out = np.zeros((len(v), 2)); out[:len(tron)] += tron; out += v
    return out


def nhan_doi(x, r, tre=(0.007, 0.015), lech_cao=0.025, muc=0.55):
    """Nhan doi: ban sao tre 7-15 ms, cao do lech ~2,5% (doi toc doc bang noi suy), dat lech sang ben kia -> day + rong."""
    n = len(x)
    k = 1 + r.choice([-1, 1]) * lech_cao
    idx = np.arange(n) * k
    y = np.stack([np.interp(idx, np.arange(n), x[:, c], right=0) for c in range(2)], axis=1)
    d = int(r.uniform(*tre) * SR)
    y = np.vstack([np.zeros((d, 2)), y[:-d]])
    y = y[:, ::-1]                       # dao kenh -> ban sao nam ben kia
    return x + y * muc


def toc_day(t, r, bd, nang=1.0, f=(150, 700), tat=0.035):
    return b3.toc(t, r, bd, nang, f, tat)


def vun_tram(t, r, bd, giay, mat_do=700, nang=1.0):
    """Lop BANG NGHIEN thu hai o dai thap (350-1300 Hz) - than cua tieng vo."""
    return b2.vun_bang(t, r, bd, giay, mat_do, (350, 1300), nang=nang, chung=0.45)


# ================================================================== BAN
def tieng_ban(hat):
    r = np.random.default_rng(hat)
    vang_dai, vang_muc = r.uniform(0.7, 1.3), r.uniform(0.35, 0.55)
    n = int(SR * 0.9); t = np.arange(n) / SR
    tron = np.zeros((n, 2))
    tron += b3.phut_lanh(n, r, np.minimum(1, t / 0.005) * np.exp(-t / r.uniform(0.08, 0.15))) * 0.8
    tron += b3.phut_lanh(n, r, np.minimum(1, t / 0.008) * np.exp(-t / r.uniform(0.1, 0.18)), 350, 1100) * 0.7       # hoi lanh dai giua
    tron += b2.vun_bang(t, r, r.uniform(0.0, 0.02), r.uniform(0.06, 0.12), r.uniform(600, 1100), (1000, 2800), nang=1.0)
    tron += vun_tram(t, r, 0.002, r.uniform(0.05, 0.1), r.uniform(400, 700), nang=0.8)
    tron += toc_day(t, r, 0.0, r.uniform(0.5, 0.7))
    dinh = r.uniform(0.04, 0.1)
    bg = np.minimum(1, t / dinh) * np.exp(-np.maximum(t - dinh, 0) / r.uniform(0.12, 0.25))
    x = (b3.xi_xoay(n, r, quay_hz=r.uniform(2, 4)) + b3.xi_xoay(n, r, 400, 1100, quay_hz=r.uniform(1.5, 3)) * 0.7) * bg[:, None] * 0.45
    p = np.linspace(0, r.choice([-0.6, 0.6]), n); g = (p + 1) * np.pi / 4
    x[:, 0] *= np.cos(g) * np.sqrt(2); x[:, 1] *= np.sin(g) * np.sqrt(2)
    tron += x
    tron += b2.dong_bang(n, r, r.uniform(150, 300), bao_mat=np.exp(-t / 0.25))
    tron = them_trong(tron, r, 2, 0.15)
    tron = nhan_doi(tron, r)
    tron = vang_day(tron, r, vang_dai, vang_muc)
    return q3.chuan(tron, -4)


# ================================================================== BAY (vong lap)
def tieng_bay(hat):
    r = np.random.default_rng(hat)
    n = int(SR * GIAY_BAY)
    x = b3.xi_xoay(n, r, 1200, 2700, quay_hz=round(1.3 * GIAY_BAY) / GIAY_BAY, sau=0.55) * 0.85
    # lop hoi lanh DAI GIUA (muot, khong cat / bung nhu lua) - than cua tieng bay
    x += b3.xi_xoay(n, r, 350, 1100, quay_hz=round(0.7 * GIAY_BAY) / GIAY_BAY, sau=0.4) * 0.8
    x += b3.xi_xoay(n, r, 220, 520, quay_hz=round(0.5 * GIAY_BAY) / GIAY_BAY, sau=0.3) * 0.35
    x += b2.dong_bang(n, r, 90, vong=True) * 1.2
    t = np.arange(n) / SR
    for _ in range(int(GIAY_BAY * 5)):
        bd = r.uniform(0, GIAY_BAY)
        tt = (t - bd) % GIAY_BAY
        x += b2.vun_bang(tt, r, 0.0, r.uniform(0.015, 0.04), r.uniform(500, 900), (700, 2400), nang=r.uniform(0.12, 0.3))
    bung = np.maximum(cham(n, 0.9, r) * 0.45 + 0.75, 0.15) ** 1.5
    x *= bung[:, None]
    # vang ngan cho day (vong lap: vang tich chap TRON VONG de khong vet noi)
    xung = []
    m = int(0.5 * SR); tm = np.arange(m) / SR
    for k in range(2):
        h = loc(r.standard_normal(m), lambda f: cao(200, 2)(f) * thap(4000, 2)(f)) * np.exp(-tm / 0.1)
        h /= np.sqrt(np.sum(h ** 2)) + 1e-12
        hh = np.zeros(n); hh[:m] = h
        xung.append(np.fft.irfft(np.fft.rfft(x[:, k]) * np.fft.rfft(hh), n))
    x = x + np.stack(xung, axis=1) * 0.45
    return q3.chuan(x, -7, vong=True)


# ================================================================== NO
def tieng_no(hat):
    r = np.random.default_rng(hat)
    # (do lan 1: vang day lam 3 bien the giong nhau - chenh 2,1-4,1 dB; noi rong co vu no + do dai vang tung bien the)
    to = r.uniform(0.6, 1.5)
    n = int(SR * 2.2); t = np.arange(n) / SR
    tron = np.zeros((n, 2))
    tron += toc_day(t, r, 0.0, r.uniform(1.0, 1.3), (140, 650), 0.045)
    tron += b2.vun_bang(t, r, 0.001, r.uniform(0.15, 0.28) * to, r.uniform(1100, 1800), (700, 2600), nang=1.3)
    tron += vun_tram(t, r, 0.002, r.uniform(0.15, 0.3) * to, r.uniform(800, 1300), nang=1.1)
    tron += b3.phut_lanh(n, r, np.minimum(1, t / 0.008) * np.exp(-t / r.uniform(0.2, 0.4))) * 0.6
    tron += b3.phut_lanh(n, r, np.minimum(1, t / 0.01) * np.exp(-t / r.uniform(0.25, 0.45)), 350, 1100) * 0.6
    b2.cuc_roi(tron, r, int(r.integers(5, 9) * to), 0.02, 0.15, 220, 550, (0.012, 0.04), nang=1.1, nay=(2, 4), rong=0.7)
    b2.cuc_roi(tron, r, int(r.integers(10, 18) * to), 0.03, 0.8, 550, 1600, nang=0.6, nay=(1, 3))
    tron += b2.dong_bang(n, r, r.uniform(250, 450), bao_mat=np.clip((t - 0.06) / 0.1, 0, 1) * np.exp(-np.maximum(t - 0.16, 0) / 0.5)) * 1.2
    tron += b2.ken_ket(t, r, r.uniform(0.15, 0.3), r.uniform(0.3, 0.5), r.uniform(300, 480)) * 0.25
    if r.random() < 0.4:
        bd = r.uniform(0.12, 0.4)
        phu = b2.vun_bang(t, r, bd, 0.06, 700, nang=0.5) + toc_day(t, r, bd, 0.35)
        p2 = r.uniform(-0.85, 0.85); phu[:, 0] *= np.sqrt(1 - p2); phu[:, 1] *= np.sqrt(1 + p2)
        tron += phu
    tron = them_trong(tron, r, int(r.integers(3, 6)), 0.3)
    tron = nhan_doi(tron, r)
    tron = vang_day(tron, r, r.uniform(0.9, 1.8), r.uniform(0.4, 0.65))
    return q3.chuan(tron, -2)


# ================================================================== KHOI BANG VO
def tieng_vo(hat, manh=1.0):
    r = np.random.default_rng(hat)
    n = int(SR * (1.7 if manh <= 1 else 2.2)); t = np.arange(n) / SR
    tron = np.zeros((n, 2))
    tk = r.uniform(0.1, 0.25) if manh <= 1 else 0.0
    if tk > 0:
        tron += b2.ken_ket(t, r, 0.0, tk, r.uniform(280, 480)) * 0.45
    tron += toc_day(t, r, tk, (0.7 if manh <= 1 else 1.2), (140, 650), 0.04)
    tron += b2.vun_bang(t, r, tk + 0.002, r.uniform(0.16, 0.3) * manh, r.uniform(1000, 1800), (600, 2400), nang=1.2)
    tron += vun_tram(t, r, tk + 0.003, r.uniform(0.15, 0.3) * manh, r.uniform(700, 1200), nang=1.0)
    if manh > 1:
        tron += b3.phut_lanh(n, r, np.minimum(1, t / 0.008) * np.exp(-t / 0.3)) * 0.5
        tron += b3.phut_lanh(n, r, np.minimum(1, t / 0.01) * np.exp(-t / 0.35), 350, 1100) * 0.5
    b2.cuc_roi(tron, r, int(r.integers(4, 8) * manh), tk + 0.01, tk + 0.15, 200, 520, (0.012, 0.04), nang=1.1, nay=(2, 4), rong=0.7)
    b2.cuc_roi(tron, r, int(r.integers(8, 16) * manh), tk + 0.02, tk + 0.9, 520, 1600, nang=0.6, nay=(1, 3))
    tron = them_trong(tron, r, int(r.integers(3, 6) * manh), tk + 0.3)
    tron = nhan_doi(tron, r)
    tron = vang_day(tron, r, 1.2, 0.5)
    return q3.chuan(tron, -2 if manh > 1 else -3)


# ================================================================== MUA BANG (mua da)
def tieng_mua_roi(hat):
    r = np.random.default_rng(hat)
    n = int(SR * 1.1); t = np.arange(n) / SR
    x = np.clip(t / GIAY_ROI_MUA, 0, 1)
    to = 1.0 / (1 + 8 * (1 - x))
    tron = b3.phut_lanh(n, r, to * np.minimum(1, t / 0.05), 900, 2800) * 0.6
    tron += b3.phut_lanh(n, r, to * np.minimum(1, t / 0.05), 300, 900) * 0.5
    # (bo 30% phan mono chung: do trong game giua tran Mua bang tuong quan o loa 0,78 - gan mono; clip roi 0,55 -> ~0,3)
    return q3.chuan(tron, -6)


def tieng_mua_cham(hat):
    r = np.random.default_rng(hat)
    n = int(SR * 0.8); t = np.arange(n) / SR
    tron = np.zeros((n, 2))
    tron += toc_day(t, r, 0.0, r.uniform(0.8, 1.1), (180, 900), r.uniform(0.015, 0.025))
    tron += b2.vun_bang(t, r, 0.001, r.uniform(0.03, 0.07), r.uniform(800, 1400), (900, 2600), nang=1.0)
    tron += vun_tram(t, r, 0.002, r.uniform(0.03, 0.06), r.uniform(500, 900), nang=0.7)
    b2.cuc_roi(tron, r, int(r.integers(2, 5)), 0.01, 0.25, 400, 1300, nang=0.6, nay=(1, 3))
    tron += b2.dong_bang(n, r, 120, bao_mat=np.exp(-t / 0.2)) * 0.8
    tron = them_trong(tron, r, int(r.integers(1, 3)), 0.1)
    tron = vang_day(tron, r, 0.9, 0.35)
    return q3.chuan(tron, -4)


if __name__ == '__main__':
    import tieng_qua_cau_bang as v1
    ra = sys.argv[1] if len(sys.argv) > 1 else os.path.join('CongCu', 'AmThanh', 'QuaCauBangThu4')
    os.makedirs(ra, exist_ok=True)
    if '--trong' in sys.argv:
        TRONG = True
        sys.argv.remove('--trong')
    ra = sys.argv[1] if len(sys.argv) > 1 else os.path.join('CongCu', 'AmThanh', 'QuaCauBangThu4')
    ten = 'B_mua_da_day' + ('_trong' if TRONG else '')
    bo = {
        'ban': [tieng_ban(6100 + 10 * i) for i in range(3)],
        'no': [tieng_no(6200 + 10 * i) for i in range(3)],
        'vo': [tieng_vo(6300 + 10 * i) for i in range(3)],
        'vono': [tieng_vo(6400 + 10 * i, 1.6) for i in range(3)],
        'muaroi': [tieng_mua_roi(6500 + 10 * i) for i in range(3)],
        'muacham': [tieng_mua_cham(6600 + 10 * i) for i in range(3)],
    }
    bay = tieng_bay(6700)
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
