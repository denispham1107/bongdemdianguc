# -*- coding: utf-8 -*-
# TIENG DEM NGUOC (11/10/2026, nguoi dung): moi giay cua man dem nguoc 5 -> 1 truoc tran (ManSanh.VeDemNguoc) mot tieng, phong cach
# kinh di. HAI tieng: "nhip" (5, 4, 3, 2) + "nhip cuoi" nang hon (1). STEREO, nang luong chinh 200 Hz - 4 kHz (loa dien thoai khong
# phat duoi ~200 Hz - xem tieng_bam.py).
#
# Ba MAU de nguoi dung nghe chon (moi mau: nhip, nhip cuoi + mot file GHEP DU 5 GIAY nghe nhu trong game):
#   A "chuong_tang"  - chuong tang le dong ngan (Re4, hoa am le) + rung sat; cuoi: chuong lon hon + chuong tram Re3 + vang dai.
#   B "tim_dap"      - nhip tim (lub-dub) co than o dai trung + tieng go long nguc; cuoi: nhip tim manh + tieng gao tram.
#   C "dong_ho_ma"   - tieng tich tac dong ho cu (go go + lo xo kim loai), tieng thi tham; cuoi: tieng dong ho dien bao (gong).
#
# Chay: python CongCu/AmThanh/tieng_dem_nguoc.py [thu_muc_ra]   (mac dinh CongCu/AmThanh/DemNguocThu)

import os, sys
import numpy as np
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import tieng_bam as tb

SR = tb.SR
PI2 = tb.PI2
rng = np.random.default_rng(511)


def ket_thuc(tron, giay_vang, do_am, hat, dinh_db):
    uot = tb.vang(tron.mean(axis=1), giay_vang, do_am, hat)
    kho = np.pad(tron, ((0, int(SR * giay_vang)), (0, 0)))
    return tb.chuan_hoa(uot * 0.5 + kho * 0.5, dinh_db)


# ---------------- A: chuong tang le ----------------
def a_nhip():
    t = tb.thoi_gian(0.8)
    tron = np.zeros((len(t), 2))
    tb.cong(tron, tb.dat_vi_tri(tb.chuong(t, 293.7, 0.32, 0.9), -0.2))
    ri = tb.loc_dai(rng.standard_normal(len(t)), 1800, 4500) * tb.bao(t, 0.002, 0.03) * 0.25
    tb.cong(tron, tb.dat_vi_tri(ri, 0.4))
    return ket_thuc(tron, 0.4, 0.3, 41, -6)


def a_cuoi():
    t = tb.thoi_gian(1.6)
    tron = np.zeros((len(t), 2))
    tb.cong(tron, tb.dat_vi_tri(tb.chuong(t, 293.7, 0.6, 1.0), -0.25))
    tb.cong(tron, tb.dat_vi_tri(tb.chuong(t, 146.8, 0.9, 0.8), 0.25), 0.01)   # Re3: hoa am cao cua no van o dai trung
    tb.cong(tron, tb.dat_vi_tri(tb.go_da(t, True) * 0.5, 0.0))
    return ket_thuc(tron, 0.9, 0.35, 42, -3)


# ---------------- B: tim dap ----------------
def nhip_tim(t, manh):
    x = np.zeros_like(t)
    for tre, bien in [(0.0, 1.0), (0.16, 0.7)]:
        tt = np.maximum(t - tre, 0)
        f = 62 * (1 + 0.6 * np.exp(-tt / 0.02))
        tram = np.sin(PI2 * np.cumsum(f) / SR) * tb.bao(tt, 0.004, 0.06) * (t >= tre)
        # than nhip o dai trung (nguc, vai) - de loa dien thoai nghe duoc
        than = tb.loc_dai(rng.standard_normal(len(t)), 220, 900) * tb.bao(tt, 0.003, 0.05) * (t >= tre)
        x += bien * (tram * 0.45 + than * (1.2 if manh else 0.9))
    return x


def b_nhip():
    t = tb.thoi_gian(0.6)
    tron = tb.dat_vi_tri(nhip_tim(t, False), 0.0)
    return ket_thuc(tron, 0.25, 0.2, 51, -6)


def b_cuoi():
    t = tb.thoi_gian(1.4)
    tron = np.zeros((len(t), 2))
    tb.cong(tron, tb.dat_vi_tri(nhip_tim(t, True), 0.0))
    # tieng gao tram (on loc 300-1200 Hz, len cham, quet xuong)
    gao = tb.loc_dai(rng.standard_normal(len(t)), 300, 1200) * tb.bao(t, 0.12, 0.35) * 0.45
    tb.cong(tron, tb.dat_vi_tri(gao, -0.3), 0.05)
    return ket_thuc(tron, 0.7, 0.35, 52, -3)


# ---------------- C: dong ho ma ----------------
def tich(t, cao):
    x = tb.loc_dai(rng.standard_normal(len(t)), cao * 0.6, cao * 1.5) * tb.bao(t, 0.0004, 0.012)
    x += np.sin(PI2 * cao * 0.35 * t) * tb.bao(t, 0.0004, 0.02) * 0.4    # than go cua vo dong ho
    return x


def c_nhip():
    t = tb.thoi_gian(0.6)
    tron = np.zeros((len(t), 2))
    tb.cong(tron, tb.dat_vi_tri(tich(t, 1500), -0.35))
    tb.cong(tron, tb.dat_vi_tri(tich(t, 1150) * 0.7, 0.35), 0.11)        # tac
    lo_xo = tb.loc_dai(rng.standard_normal(len(t)), 2500, 6000) * tb.bao(t, 0.01, 0.05) * 0.12
    tb.cong(tron, tb.dat_vi_tri(lo_xo, 0.1), 0.02)
    return ket_thuc(tron, 0.3, 0.3, 61, -6)


def c_cuoi():
    t = tb.thoi_gian(1.8)
    tron = np.zeros((len(t), 2))
    tb.cong(tron, tb.dat_vi_tri(tich(t, 1500), -0.35))
    tb.cong(tron, tb.dat_vi_tri(tb.chuong(t, 220.0, 0.9, 1.0), 0.0), 0.04)   # gong dong ho bao gio
    tb.cong(tron, tb.dat_vi_tri(tb.chuong(t, 329.6, 0.7, 0.5), 0.3), 0.05)
    tho = tb.loc_dai(rng.standard_normal(len(t)), 900, 3800) * tb.bao(t, 0.15, 0.25) * 0.10
    tb.cong(tron, tb.dat_vi_tri(tho, -0.5), 0.3)
    return ket_thuc(tron, 0.9, 0.35, 62, -3)


def ghep(nhip, cuoi):
    """Ban nghe thu: nhip o giay 0,1,2,3 (so 5,4,3,2) + cuoi o giay 4 (so 1), roi 1,5 s duoi."""
    dai = int(SR * 4.0) + len(cuoi)
    x = np.zeros((dai, 2))
    for k in range(4):
        tb.cong(x, nhip, float(k))
    tb.cong(x, cuoi, 4.0)
    return x


if __name__ == '__main__':
    ra = sys.argv[1] if len(sys.argv) > 1 else os.path.join('CongCu', 'AmThanh', 'DemNguocThu')
    os.makedirs(ra, exist_ok=True)
    for ten, fn, fc in [('A_chuong_tang', a_nhip, a_cuoi), ('B_tim_dap', b_nhip, b_cuoi), ('C_dong_ho_ma', c_nhip, c_cuoi)]:
        n, c = fn(), fc()
        for loai, x in [('nhip', n), ('nhip_cuoi', c), ('ghep_5_giay', ghep(n, c))]:
            d = os.path.join(ra, '%s_%s.wav' % (ten, loai))
            tb.ghi(d, x)
            m = x.mean(axis=1)
            P = np.abs(np.fft.rfft(m)) ** 2
            f = np.fft.rfftfreq(len(m), 1 / SR)
            pt = [P[(f >= a) & (f < b)].sum() / P.sum() * 100 for a, b in [(0, 200), (200, 1000), (1000, 4000), (4000, 22050)]]
            l, r = x[:, 0], x[:, 1]
            tq = float(np.sum(l * r) / np.sqrt(np.sum(l * l) * np.sum(r * r)))
            print('%-34s %.2f s  <200/200-1k/1-4k/>4k: %s  tuong quan %.2f' % (os.path.basename(d), len(x) / SR,
                  ' / '.join('%.0f%%' % p for p in pt), tq))
