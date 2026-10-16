# -*- coding: utf-8 -*-
# TIENG NUT GAY BE KHI LOC XOAY BUT VAT (16/10/2026, nguoi dung: "khi Loc xoay cuon do vat, nho cay o duoi dat len, cho them am thanh NUT GAY BE de the
# hien ro cac do vat va cay coi bi PHA roi moi cuon len"). Nguoi dung chon: vat RUNG NUT tai cho ~0,3 s roi moi bay; chia HAI loai tieng.
# Moi clip ~1,3 s: 0-0,3 s rung (ken ket / nut lach tach tang dan) -> ~0,3 s gay / vo BUNG (luc vat bat khoi dat) -> manh / dat roi lao xao.
#   cay - CAY BAT GOC: go nut "rac rac" (xung kich cong huong go 250-1600 Hz Q 4-8 tat 15-60 ms, day dan) + ken ket than go + "RAC" gay lon (chum
#         nut chong nhau, bao hoa) + RE BAT KHOI DAT (on tram 120-500 Hz xe + dat vun roi lop dop).
#   da  - BIA / DA / LO VO: da cot ket (nghien thua) -> "CHOANG" vo (chum phong ngan + cong huong da 1,3 / 2,2 kHz) + tang da "coc" duc nay (b2.cuc_roi
#         - lech CHI bang am luong) + soi vun lao xao + bui dat.
# Khong mot xung + "huych" kieu tieng sung (bai hoc Giut set ban 1): tieng gay la CHUM nhieu nut trong 20-60 ms + tieng rung truoc do.
# Phat CAN BANG (nhu tieng Loc xoay). Quy trinh memory: rieng tung kenh + phan giua, < 200 Hz <= ~25%, > 4 kHz <= ~10%, 3 bien the khac cau truc.
#
# Chay: python CongCu/AmThanh/tieng_loc_xoay_nut.py [thu_muc_ra]   (mac dinh CongCu/AmThanh/LocXoayNutThu)

import os, sys, wave
import numpy as np
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import tieng_bam as tb
import tieng_qua_cau_lua_3 as q3
import tieng_tang_hinh as th
import tieng_qua_cau_bang_2 as b2
import tieng_thien_thach_3 as t3
import tieng_giat_set as g1
import tieng_giat_set_3 as g3
import tieng_sam_set as ss

SR = tb.SR
PI2 = tb.PI2
GIAY_RUNG = 0.3
loc, dai, thap, cao, cham = q3.loc, q3.dai, q3.thap, q3.cao, q3.cham
on2, loc2 = th.on2, th.loc2
xeo, dat = g1.xeo, g3.dat


def nut_go(r, to=1.0, f=(250, 1600)):
    """MOT tieng nut go (mono): xung on 0,3-1 ms kich 2-3 cong huong go (Q 4-8) tat 15-60 ms."""
    d = int(0.09 * SR); t = np.arange(d) / SR
    z = r.standard_normal(d) * np.exp(-t / r.uniform(0.0003, 0.001))
    fcs = np.exp(r.uniform(np.log(f[0]), np.log(f[1]), int(r.integers(2, 4))))
    x = sum(loc(z, dai(fc, r.uniform(4, 8))) * np.exp(-t / r.uniform(0.015, 0.06)) / (1 + 0.5 * j) for j, fc in enumerate(fcs))
    x += loc(z, lambda ff: cao(1200, 2)(ff) * thap(3500, 2)(ff)) * 0.4
    return x / (np.max(np.abs(x)) + 1e-12) * to


def rai(tron, r, x, bd, lech=None):
    if x.ndim == 1: x = dat(x, r.uniform(-0.8, 0.8) if lech is None else lech)
    i = int(bd * SR); m = min(len(x), len(tron) - i)
    if m > 0: tron[i:i + m] += x[:m]


def cay(hat):
    r = np.random.default_rng(hat)
    n = int(SR * 1.4); t = np.arange(n) / SR
    tron = np.zeros((n, 2))
    gay = GIAY_RUNG + r.uniform(-0.03, 0.03)
    # rung: ken ket than go + nut lach tach tang dan (mat do tang toi luc gay)
    tron += b2.ken_ket(t, r, 0.0, gay, r.uniform(200, 380), (8, 40)) * r.uniform(0.35, 0.6)
    tt = r.uniform(0.0, 0.05)
    while tt < gay:
        rai(tron, r, nut_go(r, r.uniform(0.15, 0.5)), tt)
        tt += r.uniform(0.02, 0.09) * (1 - 0.6 * tt / gay)
    # "RAC" gay lon: chum 5-10 nut chong nhau trong 20-60 ms, bao hoa
    chum = np.zeros((int(0.2 * SR), 2))
    for _ in range(int(r.integers(5, 11))):
        rai(chum, r, nut_go(r, r.uniform(0.5, 1.0), (180, 1300)), r.uniform(0, 0.06))
    chum = np.tanh(1.8 * chum / (np.max(np.abs(chum)) + 1e-12))
    rai(tron, r, chum * 1.2, gay)
    if r.random() < 0.6:                       # gay them mot nhip
        rai(tron, r, np.tanh(1.5 * dat(nut_go(r, 1.0, (200, 1200)), r.uniform(-0.6, 0.6))) * 0.6, gay + r.uniform(0.06, 0.15))
    # re bat khoi dat: on tram xe + dat vun
    bd = gay + 0.01
    re = loc2(on2(n, r, 0.4), lambda ff: cao(120, 2)(ff) * thap(500, 2)(ff))
    re *= (np.clip((t - bd) / 0.02, 0, 1) * np.exp(-np.maximum(t - bd, 0) / r.uniform(0.15, 0.3)) * np.maximum(cham(n, 30, r) * 0.5 + 1, 0.1))[:, None]
    tron += re / (np.max(np.abs(re)) + 1e-12) * 0.8
    tron += xeo(n, r, np.clip((t - bd) / 0.02, 0, 1) * np.exp(-np.maximum(t - bd, 0) / 0.25) + 1e-4, 2500, (300, 1100), chung=0.25, giat=25) * 0.5
    tron += q3.mua_dat(n, r, bd + 0.05, r.uniform(0.25, 0.5), r.uniform(150, 300), 0.7) * 1.0
    return th.hoan_tat(ss.tam(tron, 0.2), r, r.uniform(0.5, 0.8), r.uniform(0.3, 0.45), -3)


def da(hat):
    r = np.random.default_rng(hat)
    n = int(SR * 1.3); t = np.arange(n) / SR
    tron = np.zeros((n, 2))
    vo = GIAY_RUNG + r.uniform(-0.03, 0.03)
    # rung: da cot ket - nghien thua tang dan + vai tieng "cach" nho
    tron += t3.da_nghien(t, r, 0.0, vo * 0.6, r.uniform(300, 600), 0.35) * np.clip(t / vo, 0, 1)[:, None] ** 1.5
    for _ in range(int(r.integers(2, 5))):
        tron += b3_toc(t, r, r.uniform(0.05, vo), r.uniform(0.15, 0.35))
    # "CHOANG" vo: chum phong ngan + cong huong da
    tron += ss.doang(n, r, vo, 1.0, r.uniform(0.06, 0.12))
    tron += t3.da_nghien(t, r, vo + 0.002, r.uniform(0.12, 0.25), r.uniform(1800, 3000), 0.9)
    # tang da coc duc nay + soi vun + bui dat
    b2.cuc_roi(tron, r, int(r.integers(3, 7)), vo + 0.03, vo + 0.4, 250, 800, (0.01, 0.035), nang=1.0, nay=(2, 3), rong=0.8)
    b2.cuc_roi(tron, r, int(r.integers(6, 14)), vo + 0.05, vo + 0.7, 700, 2000, nang=0.5, nay=(1, 3))
    tron += q3.mua_dat(n, r, vo + 0.05, r.uniform(0.2, 0.4), r.uniform(150, 280), 0.6)
    return th.hoan_tat(ss.tam(tron, 0.2), r, r.uniform(0.5, 0.8), r.uniform(0.3, 0.45), -3)


def b3_toc(t, r, bd, nang):
    import tieng_qua_cau_bang_3 as b3
    return b3.toc(t, r, bd, nang, (300, 1500), 0.012)


def doc(p):
    w = wave.open(p); return np.frombuffer(w.readframes(w.getnframes()), '<i2').reshape(-1, 2) / 32768


if __name__ == '__main__':
    ra = sys.argv[1] if len(sys.argv) > 1 else os.path.join('CongCu', 'AmThanh', 'LocXoayNutThu')
    os.makedirs(ra, exist_ok=True)
    bo = {'cay': [cay(23100 + 10 * i) for i in range(3)], 'da': [da(23200 + 10 * i) for i in range(3)]}
    for loai, ds in bo.items():
        for i, x in enumerate(ds):
            tb.ghi(os.path.join(ra, '%s%d.wav' % (loai, i + 1)), x)
            th.do('%s%d' % (loai, i + 1), x)
        print('   %s: chenh bien the %s dB; game can bang %s; 30ms/300ms (tu luc gay) %s' % (loai, ' / '.join('%.1f' % c for c in th.chenh_bien_the(ds)),
              ' / '.join('%.2f' % g3.tq(g3.can_bang(x, 10)) for x in ds),
              ' / '.join('%.2f' % (lambda m: np.sum(m[:1323] ** 2) / np.sum(m[:13230] ** 2))(np.abs(x).max(axis=1)[int(0.27 * SR):]) for x in ds)))
    # nghe thu nhu trong game: Loc xoay (gio hu x1,25) di qua, but 2 cay + 3 bia / da lien tiep (lech trai / phai), moi cai x1,25
    hu = [doc(os.path.join('CongCu', 'AmThanh', 'GioLocThu3', 'LocXoay_hu_%d.wav' % i)) for i in (1, 2, 3)]
    out = np.zeros((int(SR * 8.5), 2))
    tb.cong(out, g3.can_bang(hu[0], 10) * 0.8, 0.2)
    lich = [(1.0, 'cay', -20), (1.9, 'da', 5), (2.6, 'da', 25), (3.4, 'cay', 15), (4.4, 'da', -10)]
    for k, (bd, loai, az) in enumerate(lich):
        tb.cong(out, g3.can_bang(bo[loai][k % 3], az) * 0.8, bd)
    tb.ghi(os.path.join(ra, 'ghep_loc_xoay_but_vat.wav'), out / np.max(np.abs(out)) * 0.95)
