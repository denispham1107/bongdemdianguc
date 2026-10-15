# -*- coding: utf-8 -*-
# TIENG QUA CAU LUA (15/10/2026, nguoi dung): BA tieng cho ky nang Qua cau lua - BAN ra, BAY (vong lap lien mach), VA CHAM + NO.
# STEREO (quy tac nguoi dung), nang luong chinh 200 Hz - 4 kHz (loa dien thoai khong phat duoi ~200 Hz - xem tieng_bam.py).
#
# Ba MAU de nguoi dung nghe chon (moi mau: ban, bay (2 s lap), no + mot file GHEP nghe nhu trong game):
#   A "hoa_nguc"  - lua dia nguc: ban = phung bung chay + vut gio len + tieng gam ma tram ngan; bay = lua reo cuon + tanh tach;
#                   no = no than dai trung + lua phut toa + tieng gao ma rat nhe + tan lua lach tach roi.
#   B "hac_thuat" - phep hac am: ban = hop am lech (tritone) vut len + vut gio; bay = tieng hu xoay co cao do + lua reo nhe;
#                   no = no + tieng kim loai nghich tai vang dai + lua.
#   C "lua_that"  - lua that, tho: ban = tieng "phup" khi ga bat lua; bay = duoc lua phan phat (rung 9-14 Hz) + tanh tach day;
#                   no = no nang + manh vo + lua chay ri rao keo dai.
#
# Chay: python CongCu/AmThanh/tieng_qua_cau_lua.py [thu_muc_ra]   (mac dinh CongCu/AmThanh/QuaCauLuaThu)

import os, sys
import numpy as np
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import tieng_bam as tb

SR = tb.SR
PI2 = tb.PI2
rng = np.random.default_rng(1510)
GIAY_BAY = 2.0          # vong lap tieng bay: moi dieu bien co SO CHU KY NGUYEN trong 2 s, loc FFT tron vong -> lap khong vet noi


def svf(x, fc, q=0.7, kieu='bang'):
    """Loc trang thai (Chamberlin) voi tan so cat THAY DOI theo tung mau (fc la mang)."""
    fc = np.broadcast_to(np.asarray(fc, dtype=float), x.shape)
    f = 2 * np.sin(np.pi * np.clip(fc, 20, SR / 6) / SR)
    thap = bang = 0.0
    ra = np.empty_like(x)
    for i in range(len(x)):
        cao = x[i] - thap - q * bang
        bang += f[i] * cao
        thap += f[i] * bang
        ra[i] = bang if kieu == 'bang' else thap
    return ra


def on_muot(n, giay_bien, hat, vong=False):
    """Nhieu bien thien cham (0..1), dung lam bao bien do. vong=True -> lap lien mach (loc FFT tron vong)."""
    r = np.random.default_rng(hat)
    x = r.standard_normal(n)
    X = np.fft.rfft(x)
    f = np.fft.rfftfreq(n, 1 / SR)
    X *= np.exp(-(f * giay_bien) ** 2)
    y = np.fft.irfft(X, n)
    y = (y - y.min()) / (y.max() - y.min() + 1e-9)
    return y


def tanh_tach(n, so_moi_giay, hat, thap=1500, cao=6500, vong=False, bao_mat=None):
    """Tieng lua tanh tach STEREO: xung ngan 1-4 ms o thoi diem ngau nhien, moi xung mot vi tri trai/phai."""
    r = np.random.default_rng(hat)
    ra = np.zeros((n, 2))
    so = int(so_moi_giay * n / SR)
    for _ in range(so):
        i = r.integers(0, n)
        if bao_mat is not None and r.random() > bao_mat[i]:
            continue
        dai = int(SR * r.uniform(0.002, 0.009))
        t = np.arange(dai) / SR
        xung = r.standard_normal(dai) * np.exp(-t / r.uniform(0.0008, 0.003)) * r.uniform(0.3, 1.0) ** 2
        lech = r.uniform(-0.8, 0.8)
        st = tb.dat_vi_tri(xung, lech)
        chi_so = (i + np.arange(dai)) % n if vong else i + np.arange(dai)
        hop = chi_so < n
        ra[chi_so[hop]] += st[hop]
    for k in range(2):
        ra[:, k] = tb.loc_dai(ra[:, k], thap, cao)
    return ra


def lua_reo(n, thap, cao, hat, vong=False, rung_hz=0.0, rung_sau=0.0):
    """Lua reo STEREO: hai kenh = phan chung + phan rieng (rong ma khong vo), bien do troi cham (+ rung phan phat)."""
    r = np.random.default_rng(hat)
    chung = tb.loc_dai(r.standard_normal(n), thap, cao)
    ra = np.zeros((n, 2))
    t = np.arange(n) / SR
    for k in range(2):
        rieng = tb.loc_dai(r.standard_normal(n), thap, cao)
        bien = 0.55 + 0.45 * on_muot(n, 0.35, hat + 7 + k, vong)
        if rung_hz > 0:
            bien *= 1 - rung_sau * (0.5 + 0.5 * np.sin(PI2 * rung_hz * t + k * 1.3)) * on_muot(n, 0.15, hat + 20 + k, vong)
        ra[:, k] = (chung * 0.6 + rieng * 0.8) * bien
    return ra


def vut(t, f0, f1, giay_len, giay_tat, hat, q=0.45):
    """Vut gio: on qua loc bang quet tu f0 len f1 (hoac xuong)."""
    r = np.random.default_rng(hat)
    fc = f0 * (f1 / f0) ** np.clip(t / max(giay_len + giay_tat * 0.5, 1e-4), 0, 1)
    x = svf(r.standard_normal(len(t)), fc, q)
    return x * tb.bao(t, giay_len, giay_tat)


def gam_ma(t, f, giay_len, giay_tat, hat, quet=1.0):
    """Tieng gam / gao ma: song rang cua cao do rung (jitter) qua hai formant dai trung (400-1200 Hz) - nghe duoc tren loa nho."""
    r = np.random.default_rng(hat)
    rung = 1 + 0.03 * tb.loc_dai(r.standard_normal(len(t)), 0, 30) * 8
    fq = f * (quet ** (t / t[-1])) * rung
    pha = np.cumsum(fq) / SR
    rang = 2 * (pha % 1) - 1
    x = tb.loc_dai(rang, 350, 1300) + 0.5 * tb.loc_dai(rang, 1800, 2800)
    x += tb.loc_dai(r.standard_normal(len(t)), 500, 2500) * 0.35      # hoi khan
    return x * tb.bao(t, giay_len, giay_tat)


def ket_thuc(tron, giay_vang, do_am, hat, dinh_db):
    uot = tb.vang(tron.mean(axis=1), giay_vang, do_am, hat)
    kho = np.pad(tron, ((0, int(SR * giay_vang)), (0, 0)))
    return tb.chuan_hoa(uot * 0.5 + kho * 0.5, dinh_db)


def chuan_hoa_vong(x, dinh_db):
    """Chuan hoa cho vong lap: cat < 120 Hz (loc FFT tron vong -> van lien mach), KHONG tat dan cuoi file."""
    x = np.stack([tb.loc_dai(x[:, 0], 120, 9000), tb.loc_dai(x[:, 1], 120, 9000)], axis=1)
    x = x - x.mean(axis=0)
    return x / (np.max(np.abs(x)) + 1e-9) * (10 ** (dinh_db / 20))


def no_than(t, f_than, giay_tat, hat):
    """Than vu no o DAI TRUNG: xung on rong + on 180-900 Hz tat nhanh + cong huong quet xuong (khong de nang luong roi < 200 Hz)."""
    r = np.random.default_rng(hat)
    dap = tb.loc_dai(r.standard_normal(len(t)), 300, 9000) * tb.bao(t, 0.0005, 0.012)
    than = tb.loc_dai(r.standard_normal(len(t)), 180, 900) * tb.bao(t, 0.002, giay_tat)
    pha = PI2 * np.cumsum(f_than * (1 + 1.2 * np.exp(-t / 0.03))) / SR
    cong_huong = (np.sin(pha) + 0.6 * np.sin(2.1 * pha) + 0.35 * np.sin(3.3 * pha)) * tb.bao(t, 0.002, giay_tat * 0.8)
    return dap * 0.8 + than * 1.2 + cong_huong * 0.5


# ======================================================== MAU A: HOA NGUC
def a_ban():
    t = tb.thoi_gian(0.75)
    tron = np.zeros((len(t), 2))
    phung = tb.loc_dai(rng.standard_normal(len(t)), 220, 1100) * tb.bao(t, 0.012, 0.11)
    tb.cong(tron, tb.dat_vi_tri(phung, 0.0))
    tb.cong(tron, tb.dat_vi_tri(vut(t, 380, 2600, 0.16, 0.18, 101), -0.35), 0.02)
    tb.cong(tron, tb.dat_vi_tri(vut(t, 450, 3200, 0.14, 0.16, 102) * 0.8, 0.4), 0.035)
    tb.cong(tron, tb.dat_vi_tri(gam_ma(t, 92, 0.05, 0.16, 103, 0.8) * 0.28, -0.1), 0.01)
    bm = tb.bao(t, 0.01, 0.2)
    tb.cong(tron, tanh_tach(len(t), 90, 104, bao_mat=bm) * 0.9)
    return ket_thuc(tron, 0.35, 0.3, 105, -4)


def a_bay():
    n = int(SR * GIAY_BAY)
    x = lua_reo(n, 220, 1600, 111, vong=True) * 1.0
    x += lua_reo(n, 1200, 3800, 112, vong=True) * 0.35
    x += tanh_tach(n, 32, 113, vong=True) * 2.2
    return chuan_hoa_vong(x, -7)


def a_no():
    t = tb.thoi_gian(1.5)
    tron = np.zeros((len(t), 2))
    tb.cong(tron, tb.dat_vi_tri(no_than(t, 105, 0.22, 121), 0.0))
    tb.cong(tron, tb.dat_vi_tri(vut(t, 2600, 420, 0.03, 0.45, 122), -0.45), 0.01)    # lua phut toa ra
    tb.cong(tron, tb.dat_vi_tri(vut(t, 2200, 380, 0.04, 0.5, 123), 0.5), 0.02)
    tb.cong(tron, tb.dat_vi_tri(gam_ma(t, 170, 0.08, 0.3, 124, 0.55) * 0.16, 0.25), 0.05)
    bm = tb.bao(t, 0.02, 0.45)
    tb.cong(tron, tanh_tach(len(t), 160, 125, bao_mat=bm) * 1.1)
    tb.cong(tron, lua_reo(len(t), 260, 1400, 126) * tb.bao(t, 0.05, 0.35)[:, None] * 0.35)
    return ket_thuc(tron, 0.7, 0.32, 127, -2)


# ======================================================== MAU B: HAC THUAT
def hop_am(t, goc, cac_ti, quet, hat):
    r = np.random.default_rng(hat)
    x = np.zeros_like(t)
    for k, ti in enumerate(cac_ti):
        for lech in (-0.006, 0.006):
            fq = goc * ti * (1 + lech) * (quet ** np.clip(t / 0.35, 0, 1))
            pha = np.cumsum(fq) / SR
            x += (2 * (pha % 1) - 1) * (0.7 ** k)
    return tb.loc_dai(x, 250, 3000)


def b_ban():
    t = tb.thoi_gian(0.8)
    tron = np.zeros((len(t), 2))
    tb.cong(tron, tb.dat_vi_tri(hop_am(t, 220, [1.0, 1.414, 2.0], 1.6, 201) * tb.bao(t, 0.06, 0.18) * 0.22, -0.3))
    tb.cong(tron, tb.dat_vi_tri(hop_am(t, 233, [1.0, 1.5], 1.7, 202) * tb.bao(t, 0.07, 0.17) * 0.18, 0.3), 0.01)
    tb.cong(tron, tb.dat_vi_tri(vut(t, 500, 3000, 0.17, 0.2, 203), 0.0), 0.04)
    tb.cong(tron, tb.dat_vi_tri(tb.loc_dai(rng.standard_normal(len(t)), 250, 1000) * tb.bao(t, 0.01, 0.08) * 0.6, 0.0))
    return ket_thuc(tron, 0.5, 0.38, 204, -4)


def b_bay():
    n = int(SR * GIAY_BAY)
    t = np.arange(n) / SR
    x = lua_reo(n, 260, 1500, 211, vong=True) * 0.65
    # tieng hu xoay: hai tone lech (220 / 311 Hz - tritone) dieu bien tron 3 Hz lech pha hai kenh (cho cam giac xoay)
    for k in range(2):
        hu = (np.sin(PI2 * 220 * t) + 0.7 * np.sin(PI2 * 311 * t + 0.5) + 0.3 * np.sin(PI2 * 440 * t)) * \
             (0.55 + 0.45 * np.sin(PI2 * 3.0 * t + k * np.pi))
        x[:, k] += hu * 0.22
    x += tanh_tach(n, 14, 213, vong=True) * 1.5
    return chuan_hoa_vong(x, -7)


def b_no():
    t = tb.thoi_gian(1.8)
    tron = np.zeros((len(t), 2))
    tb.cong(tron, tb.dat_vi_tri(no_than(t, 115, 0.2, 221), 0.0))
    # tieng kim loai nghich tai (hoa am le) vang dai - nhu chuong nut
    tb.cong(tron, tb.dat_vi_tri(tb.chuong(t, 311.1, 0.55, 0.45), -0.35), 0.005)
    tb.cong(tron, tb.dat_vi_tri(tb.chuong(t, 440.0, 0.45, 0.3), 0.4), 0.012)
    tb.cong(tron, tb.dat_vi_tri(vut(t, 2400, 400, 0.03, 0.4, 222), 0.2), 0.01)
    bm = tb.bao(t, 0.02, 0.3)
    tb.cong(tron, tanh_tach(len(t), 70, 223, bao_mat=bm) * 0.9)
    return ket_thuc(tron, 0.9, 0.4, 224, -2)


# ======================================================== MAU C: LUA THAT
def c_ban():
    t = tb.thoi_gian(0.6)
    tron = np.zeros((len(t), 2))
    # "phup" bat lua: on dai trung, cong huong quet xuong rat nhanh
    pha = PI2 * np.cumsum(260 * (1 + 1.5 * np.exp(-t / 0.02))) / SR
    phup = (tb.loc_dai(rng.standard_normal(len(t)), 200, 1400) + 0.8 * np.sin(pha)) * tb.bao(t, 0.006, 0.07)
    tb.cong(tron, tb.dat_vi_tri(phup, 0.0))
    tb.cong(tron, tb.dat_vi_tri(vut(t, 700, 1800, 0.08, 0.16, 301), -0.25), 0.02)
    tb.cong(tron, tb.dat_vi_tri(vut(t, 600, 1500, 0.09, 0.15, 302), 0.3), 0.03)
    bm = tb.bao(t, 0.01, 0.25)
    tb.cong(tron, tanh_tach(len(t), 140, 303, bao_mat=bm) * 1.1)
    return ket_thuc(tron, 0.2, 0.2, 304, -4)


def c_bay():
    n = int(SR * GIAY_BAY)
    # duoc lua phan phat: rung 12 Hz (24 chu ky / 2 s - lap lien mach)
    x = lua_reo(n, 200, 1800, 311, vong=True, rung_hz=12.0, rung_sau=0.55)
    x += lua_reo(n, 1500, 4500, 312, vong=True, rung_hz=12.0, rung_sau=0.4) * 0.3
    x += tanh_tach(n, 70, 313, vong=True) * 2.6
    return chuan_hoa_vong(x, -7)


def c_no():
    t = tb.thoi_gian(2.0)
    tron = np.zeros((len(t), 2))
    tb.cong(tron, tb.dat_vi_tri(no_than(t, 95, 0.32, 321) * 1.1, 0.0))
    tb.cong(tron, tb.dat_vi_tri(vut(t, 1800, 300, 0.02, 0.55, 322) * 0.8, -0.3), 0.005)
    # manh vo roi (go / da nho, lech trai phai)
    r = np.random.default_rng(323)
    for _ in range(14):
        bd = r.uniform(0.08, 0.9)
        tm = tb.thoi_gian(0.08)
        manh = tb.loc_dai(r.standard_normal(len(tm)), r.uniform(900, 1600), r.uniform(2500, 5000)) * tb.bao(tm, 0.0005, 0.012)
        tb.cong(tron, tb.dat_vi_tri(manh * r.uniform(0.2, 0.55) * np.exp(-bd * 2), r.uniform(-0.8, 0.8)), bd)
    bm = tb.bao(t, 0.05, 0.8)
    tb.cong(tron, tanh_tach(len(t), 120, 324, bao_mat=bm) * 1.0)
    tb.cong(tron, lua_reo(len(t), 220, 1500, 325) * tb.bao(t, 0.08, 0.7)[:, None] * 0.4)
    return ket_thuc(tron, 0.6, 0.28, 326, -2)


def ghep(ban, bay, no):
    """Ban nghe thu nhu trong game: ban -> bay 1,4 s (lap) -> no; tieng bay to dan / tat dan nhanh."""
    dai_bay = int(SR * 1.4)
    lap = np.concatenate([bay] * 2)[:dai_bay].copy()
    n = int(0.06 * SR)
    lap[:n] *= np.linspace(0, 1, n)[:, None]
    lap[-n:] *= np.linspace(1, 0, n)[:, None]
    x = np.zeros((int(SR * 0.15) + dai_bay + len(no) + SR // 2, 2))
    tb.cong(x, ban, 0.0)
    tb.cong(x, lap, 0.15)
    tb.cong(x, no, 0.15 + 1.4 - 0.02)
    return x


def phan_tich(ten, x):
    m = x.mean(axis=1)
    P = np.abs(np.fft.rfft(m)) ** 2
    f = np.fft.rfftfreq(len(m), 1 / SR)
    pt = [P[(f >= a) & (f < b)].sum() / P.sum() * 100 for a, b in [(0, 200), (200, 1000), (1000, 4000), (4000, 22050)]]
    l, r = x[:, 0], x[:, 1]
    tq = float(np.sum(l * r) / np.sqrt(np.sum(l * l) * np.sum(r * r)))
    print('%-30s %.2f s  <200/200-1k/1-4k/>4k: %s  tuong quan %.2f  dinh %.2f' % (ten, len(x) / SR,
          ' / '.join('%.0f%%' % p for p in pt), tq, np.max(np.abs(x))))
    return pt, tq


if __name__ == '__main__':
    ra = sys.argv[1] if len(sys.argv) > 1 else os.path.join('CongCu', 'AmThanh', 'QuaCauLuaThu')
    os.makedirs(ra, exist_ok=True)
    for ten, fb, fy, fn in [('A_hoa_nguc', a_ban, a_bay, a_no), ('B_hac_thuat', b_ban, b_bay, b_no),
                            ('C_lua_that', c_ban, c_bay, c_no)]:
        ban, bay, no = fb(), fy(), fn()
        # vong lap: do buoc nhay o cho noi (mau cuoi -> mau dau) so voi buoc nhay trung binh
        noi = np.max(np.abs(bay[0] - bay[-1]))
        tb_buoc = np.mean(np.max(np.abs(np.diff(bay, axis=0)), axis=1))
        print('   vong lap %s: buoc nhay o cho noi %.4f (trung binh moi mau %.4f)' % (ten, noi, tb_buoc))
        for loai, x in [('ban', ban), ('bay', bay), ('no', no), ('ghep_nghe_thu', ghep(ban, bay, no))]:
            d = os.path.join(ra, '%s_%s.wav' % (ten, loai))
            tb.ghi(d, x)
            phan_tich(os.path.basename(d), x)
