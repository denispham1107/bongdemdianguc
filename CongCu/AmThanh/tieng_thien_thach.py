# -*- coding: utf-8 -*-
# TIENG THIEN THACH (15/10/2026, nguoi dung): BA tieng - ROI (lao tu tren troi xuong, ~0,8 s, to dan toi luc cham dat), VA CHAM + NO,
# LUA CHAY (vong lap lien mach, vung lua sau vu no chay 5 s). STEREO (quy tac nguoi dung), nang luong chinh 200 Hz - 4 kHz
# (loa dien thoai khong phat duoi ~200 Hz - xem tieng_bam.py). Dung chung bo cong cu voi tieng_qua_cau_lua.py.
#
# Ba MAU de nguoi dung nghe chon (moi mau: roi, no, chay (3 s lap) + mot file GHEP nghe nhu trong game):
#   A "da_lua_that"  - cung chat voi Qua cau lua "Lửa thật": roi = khong khi bi xe toac (rung hon loan 25 Hz) to dan + tanh tach;
#                      no = da vo toac "rac" + thup nang + mua soi da roi; chay = dong lua trai rong reo + tieng go no lach tach.
#   B "dia_nguc"     - kinh di: roi = tieng ru ma quet xuong + vut gio; no = no + tieng kim loai nghich tai + tieng gam ma tat dan;
#                      chay = lua reo + tieng than thi tham rat nhe trong lua.
#   C "dien_anh"     - hoanh trang: roi = tieng gam tron to dan (cao do keo len) + vut sac luc cham; no = no rong vang dai + manh vo;
#                      chay = lua reo day, it tanh tach.
#
# Chay: python CongCu/AmThanh/tieng_thien_thach.py [thu_muc_ra]   (mac dinh CongCu/AmThanh/ThienThachThu)

import os, sys
import numpy as np
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import tieng_bam as tb
import tieng_qua_cau_lua as ql

SR = tb.SR
PI2 = tb.PI2
rng = np.random.default_rng(1511)
GIAY_ROI = 0.8          # thien thach roi ~27 m o 34 m/s = 0,79 s (ThienThach.doCaoRoi / doNghieng / speed)
DAI_ROI = 1.0           # file dai hon mot chut: cham dat muon thi van con tieng; cham dat la game tat ngay
GIAY_CHAY = 3.0         # vong lap lua chay


def bao_roi(t, mu=2.2):
    """To dan toi GIAY_ROI (luc cham dat) roi giu - game tat tieng roi ngay luc no."""
    x = np.clip(t / GIAY_ROI, 0, 1) ** mu
    return np.where(t <= GIAY_ROI, x, 1.0) * np.clip(t / 0.03, 0, 1)


def soi_roi(n, so, hat, bat_dau, ket_thuc, tat_mu=2.0):
    """Mua soi da roi xuong: xung go ngan (900-4500 Hz), thua dan theo thoi gian, moi vien mot vi tri trai/phai."""
    r = np.random.default_rng(hat)
    ra = np.zeros((n, 2))
    for _ in range(so):
        u = r.random() ** tat_mu
        bd = bat_dau + u * (ket_thuc - bat_dau)
        i = int(bd * SR)
        if i >= n:
            continue
        tm = tb.thoi_gian(0.05)
        f0 = r.uniform(900, 2200)
        manh = tb.loc_dai(r.standard_normal(len(tm)), f0, f0 * 2.4) * tb.bao(tm, 0.0004, r.uniform(0.004, 0.012))
        manh *= r.uniform(0.25, 1.0) * (1 - 0.7 * u)
        st = tb.dat_vi_tri(manh, r.uniform(-0.85, 0.85))
        m = min(len(st), n - i)
        ra[i:i + m] += st[:m]
    return ra


def ket_thuc_vong(x, dinh_db):
    return ql.chuan_hoa_vong(x, dinh_db)


# ======================================================== MAU A: DA LUA THAT
def a_roi():
    t = tb.thoi_gian(DAI_ROI)
    n = len(t)
    tron = np.zeros((n, 2))
    # khong khi bi xe toac: lua reo rong, rung hon loan 25 Hz, tam loc quet len (lai gan -> sang hon)
    for k, lech in [(0, -0.3), (1, 0.3)]:
        x = ql.svf(rng.standard_normal(n), 350 * (1 + 3.5 * np.clip(t / GIAY_ROI, 0, 1) ** 1.5), 0.6)
        rung = 1 - 0.45 * (0.5 + 0.5 * np.sin(PI2 * 25 * t + rng.uniform(0, PI2))) * ql.on_muot(n, 0.02, 400 + k)
        tb.cong(tron, tb.dat_vi_tri(x * rung * bao_roi(t), lech))
    gam = tb.loc_dai(rng.standard_normal(n), 200, 700) * bao_roi(t, 1.6) * 0.8
    tb.cong(tron, tb.dat_vi_tri(gam, 0.0))
    tb.cong(tron, ql.tanh_tach(n, 70, 401, bao_mat=bao_roi(t, 1.2)) * 1.4)
    return tb.chuan_hoa(tron, -3)


def a_no():
    t = tb.thoi_gian(2.6)
    n = len(t)
    tron = np.zeros((n, 2))
    # da vo toac "rac" (rat ngan, sang) + thup nang (dai trung)
    rac = tb.loc_dai(rng.standard_normal(n), 1200, 7000) * tb.bao(t, 0.0003, 0.02)
    tb.cong(tron, tb.dat_vi_tri(rac * 0.9, -0.1))
    tb.cong(tron, tb.dat_vi_tri(ql.no_than(t, 140, 0.3, 411) * 1.25, 0.0))
    # dat cat bung len (on 300-2000 Hz tat 0,5 s)
    dat = tb.loc_dai(rng.standard_normal(n), 300, 2000) * tb.bao(t, 0.01, 0.5) * 0.5
    tb.cong(tron, tb.dat_vi_tri(dat, 0.25), 0.01)
    tb.cong(tron, soi_roi(n, 46, 412, 0.15, 1.6) * 0.9)
    tb.cong(tron, ql.lua_reo(n, 240, 1600, 413) * tb.bao(t, 0.15, 0.8)[:, None] * 0.35)
    return ql.ket_thuc(tron, 0.7, 0.3, 414, -1)


def a_chay():
    n = int(SR * GIAY_CHAY)
    x = ql.lua_reo(n, 200, 1700, 421, vong=True) * 1.0
    x += ql.lua_reo(n, 1300, 4200, 422, vong=True) * 0.3
    x += ql.tanh_tach(n, 55, 423, vong=True) * 2.4
    x += ql.tanh_tach(n, 9, 424, thap=500, cao=2200, vong=True) * 3.5     # go no "bop" tram hon
    return ket_thuc_vong(x, -6)


# ======================================================== MAU B: DIA NGUC
def b_roi():
    t = tb.thoi_gian(DAI_ROI)
    n = len(t)
    tron = np.zeros((n, 2))
    ru = ql.gam_ma(t, 330, 0.05, 5.0, 431, quet=0.45) * bao_roi(t, 1.3) * 0.5        # tieng ru quet xuong
    tb.cong(tron, tb.dat_vi_tri(ru, -0.3))
    ru2 = ql.gam_ma(t, 247, 0.05, 5.0, 432, quet=0.5) * bao_roi(t, 1.3) * 0.35
    tb.cong(tron, tb.dat_vi_tri(ru2, 0.35))
    for k, lech in [(0, -0.4), (1, 0.4)]:     # vut STEREO: moi kenh mot nguon on rieng (mot nguon o giua = gan nhu mono)
        vut = ql.svf(rng.standard_normal(n), 500 * (1 + 3 * np.clip(t / GIAY_ROI, 0, 1) ** 2), 0.5) * bao_roi(t, 2.5)
        tb.cong(tron, tb.dat_vi_tri(vut * 0.75, lech))
    return tb.chuan_hoa(tron, -3)


def b_no():
    t = tb.thoi_gian(3.0)
    n = len(t)
    tron = np.zeros((n, 2))
    tb.cong(tron, tb.dat_vi_tri(ql.no_than(t, 95, 0.35, 441) * 1.2, 0.0))
    tb.cong(tron, tb.dat_vi_tri(tb.chuong(t, 277.2, 0.7, 0.4), -0.35), 0.008)
    tb.cong(tron, tb.dat_vi_tri(tb.chuong(t, 392.0, 0.55, 0.3), 0.4), 0.015)
    tb.cong(tron, tb.dat_vi_tri(ql.gam_ma(t, 120, 0.15, 0.6, 442, 0.6) * 0.3, 0.1), 0.05)
    tb.cong(tron, soi_roi(n, 24, 443, 0.15, 1.2) * 0.6)
    return ql.ket_thuc(tron, 1.0, 0.4, 444, -1)


def b_chay():
    n = int(SR * GIAY_CHAY)
    t = np.arange(n) / SR
    x = ql.lua_reo(n, 220, 1600, 451, vong=True) * 0.9
    x += ql.tanh_tach(n, 25, 452, vong=True) * 1.8
    # tieng than thi tham (on dai 900-3000, bao cham 1/3 Hz - 1 chu ky / 3 s: lap lien mach)
    for k in range(2):
        tho = tb.loc_dai(np.random.default_rng(453 + k).standard_normal(n), 900, 3000)
        tho *= (0.5 + 0.5 * np.sin(PI2 * t / GIAY_CHAY + k * 2.1)) ** 3
        x[:, k] += tho * 0.18
    return ket_thuc_vong(x, -6)


# ======================================================== MAU C: DIEN ANH
def c_roi():
    t = tb.thoi_gian(DAI_ROI)
    n = len(t)
    tron = np.zeros((n, 2))
    # gam tron: hai tone rang cua cao do keo len (180 -> 320 Hz) qua loc dai trung
    for k, (f0, lech) in enumerate([(180, -0.25), (183, 0.25)]):
        fq = f0 * (1 + 0.8 * np.clip(t / GIAY_ROI, 0, 1) ** 2)
        pha = np.cumsum(fq) / SR
        rang = tb.loc_dai(2 * (pha % 1) - 1, 220, 2500)
        tb.cong(tron, tb.dat_vi_tri(rang * bao_roi(t, 1.8) * 0.35, lech))
    for k, lech in [(0, -0.4), (1, 0.4)]:
        vut = ql.svf(rng.standard_normal(n), 600 * (1 + 4 * np.clip(t / GIAY_ROI, 0, 1) ** 3), 0.4) * bao_roi(t, 3.0)
        tb.cong(tron, tb.dat_vi_tri(vut * 0.75, lech))
    tb.cong(tron, tb.dat_vi_tri(tb.loc_dai(rng.standard_normal(n), 200, 900) * bao_roi(t, 1.5) * 0.5, 0.0))
    return tb.chuan_hoa(tron, -3)


def c_no():
    t = tb.thoi_gian(3.2)
    n = len(t)
    tron = np.zeros((n, 2))
    tb.cong(tron, tb.dat_vi_tri(ql.no_than(t, 80, 0.55, 461) * 1.3, 0.0))
    tb.cong(tron, tb.dat_vi_tri(ql.vut(t, 2400, 300, 0.02, 0.7, 462) * 0.8, -0.4), 0.005)
    tb.cong(tron, tb.dat_vi_tri(ql.vut(t, 2000, 280, 0.03, 0.75, 463) * 0.8, 0.45), 0.012)
    tb.cong(tron, soi_roi(n, 30, 464, 0.2, 1.4) * 0.6)
    return ql.ket_thuc(tron, 1.4, 0.45, 465, -1)


def c_chay():
    n = int(SR * GIAY_CHAY)
    x = ql.lua_reo(n, 200, 1400, 471, vong=True) * 1.0
    x += ql.lua_reo(n, 900, 3000, 472, vong=True) * 0.45
    x += ql.tanh_tach(n, 12, 473, vong=True) * 1.4
    return ket_thuc_vong(x, -6)


def ghep(roi, no, chay):
    """Nghe thu nhu trong game: roi (cat o 0,8 s) -> no -> lua chay 5 s (to dan 0,3 s, 2 s cuoi nho dan) chong len tieng no."""
    cat = roi[:int(SR * GIAY_ROI)].copy()
    m = int(0.02 * SR)
    cat[-m:] *= np.linspace(1, 0, m)[:, None]
    dai_chay = int(SR * 5.0)
    lap = np.concatenate([chay] * 3)[:dai_chay].copy()
    tt = np.arange(dai_chay) / SR
    vo = np.clip(tt / 0.3, 0, 1) * np.clip((5.0 - tt) / 2.0, 0, 1)
    lap *= vo[:, None] * 0.8
    x = np.zeros((int(SR * (GIAY_ROI + 5.5)) + len(no), 2))
    tb.cong(x, cat, 0.0)
    tb.cong(x, no, GIAY_ROI)
    tb.cong(x, lap, GIAY_ROI + 0.05)
    return x / max(1.0, np.max(np.abs(x)) / 0.9)


if __name__ == '__main__':
    ra = sys.argv[1] if len(sys.argv) > 1 else os.path.join('CongCu', 'AmThanh', 'ThienThachThu')
    os.makedirs(ra, exist_ok=True)
    for ten, fr, fn, fc in [('A_da_lua_that', a_roi, a_no, a_chay), ('B_dia_nguc', b_roi, b_no, b_chay),
                            ('C_dien_anh', c_roi, c_no, c_chay)]:
        roi, no, chay = fr(), fn(), fc()
        noi = np.max(np.abs(chay[0] - chay[-1]))
        tb_buoc = np.mean(np.max(np.abs(np.diff(chay, axis=0)), axis=1))
        print('   vong lap %s: buoc nhay o cho noi %.4f (trung binh moi mau %.4f)' % (ten, noi, tb_buoc))
        for loai, x in [('roi', roi), ('no', no), ('chay', chay), ('ghep_nghe_thu', ghep(roi, no, chay))]:
            d = os.path.join(ra, '%s_%s.wav' % (ten, loai))
            tb.ghi(d, x)
            ql.phan_tich(os.path.basename(d), x)
