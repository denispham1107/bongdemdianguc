# -*- coding: utf-8 -*-
# TIENG GIO LOC - BAN 2 (16/10/2026). Nguoi dung nghe ban 1 (tieng_gio_loc.py, A Loc gam / B Loc rit / C Loc cat): "van chua giong tieng gio loc lam".
# Vi sao: ban 1 chi la ON loc dai troi cham + dao trai / phai nhe - nghe ra "gio chung chung", khong co dau hieu XOAY.
# Ban 2 - ba dac trung cua loc that:
#   - "VU... VU..." XOAY: moi vong xoay mot luong gio LUOT qua (on dai, tam dai len roi xuong 0,25-0,4 s nhu vat bay qua sat tai - tren ON, khong co
#     cao do nen khong "laser"), lan tu ben nay sang ben kia, 2-3,5 vong / giay, chong nhau;
#   - GAM tram LIEN TUC (kieu "tau hoa" cua loc xoay that): on 90-450 Hz day, cuon theo dot cham;
#   - GIO HU: vai dai cong huong HEP (Q 5-8) 350-900 Hz troi CHAM theo con giat (khong truot nhanh) - "hu... u...";
#   - BUI: cat lao xao + soi lach cach + PHAN PHAT (la / vai bi giat: on dieu bien 14-24 Hz).
# Vong lap 3 s lien mach (tieng_may_giong.vong_tron / su kien modulo). Phat CAN BANG. Mo phong ra loa (tieng_gio_loc.ghep_game).
# Ba MAU (cung bui):
#   A "vu_xoay"  - "vu vu" xoay ro nhat (luong gio luot qua lai), gam vua, hu nhe.
#   B "gam_tau"  - GAM day lien tuc kieu tau hoa la chinh + vu xoay cham, hu nhe.
#   C "hu_gio"   - gio HU lon xuong theo con giat + vu xoay, gam nhe.
#
# Chay: python CongCu/AmThanh/tieng_gio_loc_2.py [thu_muc_ra] [loc_ten_mau...]   (mac dinh CongCu/AmThanh/GioLocThu2)

import os, sys
import numpy as np
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import tieng_bam as tb
import tieng_qua_cau_lua_3 as q3
import tieng_tang_hinh as th
import tieng_giat_set as g1
import tieng_giat_set_3 as g3
import tieng_sam_set as ss
import tieng_may_giong as mg
import tieng_gio_loc as gl

SR = tb.SR
PI2 = tb.PI2
GIAY = 3.0
loc, dai, thap, cao, cham = q3.loc, q3.dai, q3.thap, q3.cao, q3.cham
on2, loc2 = th.on2, th.loc2
xeo, dat = g1.xeo, g3.dat


def vu(n, r, vong_moi_giay=3.0, f_thap=300, f_cao=1600, giay=(0.25, 0.4), nang=1.0):
    """"VU" XOAY: moi vong mot luong gio LUOT qua - on qua dai (Q 1,5) tam len f_thap -> f_cao roi xuong lai (hinh chuong), bao len xuong cung nhip,
    lan tu ben nay sang ben kia (chieu xoay giu nguyen), dat theo nhip vong (lech ngau nhien 15%) - tron vong."""
    ra = np.zeros((n, 2))
    so = int(round(vong_moi_giay * n / SR))
    chieu = r.choice([-1, 1])
    for i in range(so):
        d = r.uniform(*giay); m = int(d * SR); tm = np.arange(m) / SR
        u = tm / d
        chuong = np.sin(np.pi * u) ** 1.5
        fc = f_thap * (f_cao / f_thap) ** chuong * r.uniform(0.85, 1.15)
        # (do lan 1: mot nguon on mono lan qua giua -> tuong quan 0,35-0,38) - hai kenh ON RIENG (45% chung), cung bao + cung quet, lech bang am luong
        z = on2(m, r, 0.45)
        x2 = np.stack([th.quet(z[:, k], np.maximum(fc * (1 + 0.04 * (k - 0.5)), 60), 1.5) for k in range(2)], axis=1) * (chuong * r.uniform(0.5, 1.0))[:, None]
        x2 /= np.max(np.abs(x2)) + 1e-12
        lech = chieu * np.linspace(-0.9, 0.9, m)
        g = (lech + 1) * np.pi / 4
        s = np.stack([x2[:, 0] * np.cos(g), x2[:, 1] * np.sin(g)], axis=1) * np.sqrt(2) * r.uniform(0.6, 1.0)
        bd = (i / vong_moi_giay + r.uniform(-0.15, 0.15) / vong_moi_giay)
        idx = (int(bd * SR) + np.arange(m)) % n
        ra[idx] += s
    return ra / (np.max(np.abs(ra)) + 1e-12) * nang


def gam(n, r, f=(90, 450), nang=1.0):
    """GAM tram lien tuc kieu tau hoa: on 90-450 Hz (hai kenh 40% chung) + lop 400-900 Hz nhe, cuon theo dot cham + rung lac 6-10 Hz."""
    z = loc2(on2(n, r, 0.4), lambda ff: cao(f[0], 2)(ff) * thap(f[1], 3)(ff))
    z2 = loc2(on2(n, r, 0.3), lambda ff: cao(400, 2)(ff) * thap(900, 2)(ff))
    x = z / (np.max(np.abs(z)) + 1e-12) + z2 / (np.max(np.abs(z2)) + 1e-12) * 0.35
    x *= (np.maximum(cham(n, 0.5, r) * 0.25 + 1, 0.4) * np.maximum(cham(n, 8, r) * 0.15 + 1, 0.5))[:, None]
    return x / (np.max(np.abs(x)) + 1e-12) * nang


def hu(n, r, so=3, f=(350, 900), q=(5, 8), nang=1.0):
    """GIO HU: vai dai cong huong HEP tren on, tam troi CHAM (con giat 0,3-0,7 Hz, +-20%), bao theo con giat; moi dai mot ben."""
    ra = np.zeros((n, 2))
    for j in range(so):
        f0 = np.exp(r.uniform(np.log(f[0]), np.log(f[1])))
        tam = f0 * np.exp(0.2 * cham(n, r.uniform(0.3, 0.7), r))
        x = th.quet(r.standard_normal(n), tam, r.uniform(*q))
        x = x / (np.max(np.abs(x)) + 1e-12) * np.maximum(cham(n, r.uniform(0.3, 0.6), r) * 0.6 + 0.6, 0.0) ** 2
        ra += dat(x, np.linspace(-0.7, 0.7, so)[j] + r.uniform(-0.15, 0.15))
    return ra / (np.max(np.abs(ra)) + 1e-12) * nang


def phan_phat(n, r, mat_do=6.0, nang=1.0):
    """PHAN PHAT (la / vai bi gio giat): chum on 0,15-0,5 s dieu bien bien do 14-24 Hz (vo vo phach), dai 300-2000 Hz, moi chum mot cho."""
    ra = np.zeros((n, 2))
    for _ in range(int(mat_do * n / SR)):
        d = r.uniform(0.15, 0.5); m = int(d * SR); tm = np.arange(m) / SR
        dieu = 0.5 + 0.5 * np.sin(PI2 * r.uniform(14, 24) * tm + r.uniform(0, PI2)) ** 2
        b = np.sin(np.pi * tm / d) * dieu
        x = loc2(on2(m, r, 0.3), lambda ff: cao(300, 2)(ff) * thap(2000, 2)(ff)) * b[:, None]
        idx = (r.integers(0, n) + np.arange(m)) % n
        g = (r.uniform(-0.9, 0.9) + 1) * np.pi / 4
        ra[idx] += x / (np.max(np.abs(x)) + 1e-12) * np.array([np.cos(g), np.sin(g)]) * np.sqrt(2) * r.uniform(0.3, 0.8)
    return ra * nang


CAU_HINH = {
    # kieu: (vu vong/giay, vu nang, gam nang, hu nang, hu so)
    'vu_xoay': (3.0, 1.0, 0.75, 0.3, 2),     # (do lan 1: gam 0,55 -> 200 Hz - 1 kHz 29% - mong)
    'gam_tau': (2.0, 0.55, 1.0, 0.25, 2),
    'hu_gio': (2.33, 0.75, 0.4, 0.8, 3),
}


def thoi(hat, kieu):
    vps, m_vu, m_gam, m_hu, so_hu = CAU_HINH[kieu]
    r = np.random.default_rng(hat)
    n = int(SR * GIAY)
    x = vu(n, r, vps, nang=m_vu)
    x += mg.vong_tron(lambda m: gam(m, r, nang=m_gam), n, r)
    x += mg.vong_tron(lambda m: hu(m, r, so_hu, nang=m_hu), n, r)
    x = mg.vang_tron(ss.tam(x, 0.2), r, 0.9, 0.45)
    return q3.chuan(x, -4, vong=True)


def bui(hat, kieu):
    vps = CAU_HINH[kieu][0]
    r = np.random.default_rng(hat)
    n = int(SR * GIAY)
    x = gl.bui_cuon(n, r, 1500, 30, 6, vps)
    x += phan_phat(n, r, 5.0, 0.8)
    x = loc2(x, thap(3600, 4))
    x = mg.vang_tron(x, r, 0.4, 0.3)
    return q3.chuan(x, -5, vong=True)


MAU = {'A_vu_xoay': ('vu_xoay', 20100), 'B_gam_tau': ('gam_tau', 20300), 'C_hu_gio': ('hu_gio', 20500)}

if __name__ == '__main__':
    ra = sys.argv[1] if len(sys.argv) > 1 else os.path.join('CongCu', 'AmThanh', 'GioLocThu2')
    os.makedirs(ra, exist_ok=True)
    chi = sys.argv[2:]
    for ten, (kieu, hat0) in MAU.items():
        if chi and not any(c in ten for c in chi): continue
        x1 = thoi(hat0, kieu); x2 = bui(hat0 + 100, kieu)
        for loai, x in (('thoi', x1), ('bui', x2)):
            tb.ghi(os.path.join(ra, '%s_%s.wav' % (ten, loai)), x)
            th.do('%s_%s' % (ten, loai), x)
            dd = np.abs(np.diff(x[:, 0]))
            m = x.mean(axis=1); e = np.sqrt(np.convolve(m ** 2, np.ones(4410) / 4410, 'same'))[4410:-4410]
            print('   %s: noi %.4f (buoc quanh %.4f / %.4f), bao bien thien %.2f; game can bang %.2f' % (loai, abs(x[0, 0] - x[-1, 0]),
                  dd[-1], dd[0], (e.max() - e.min()) / e.max(), g3.tq(g3.can_bang(x, 15))))
        g = gl.ghep_game(x1, x2)
        tb.ghi(os.path.join(ra, '%s_ghep_nhu_trong_game.wav' % ten), g)
        th.do('%s_ghep_game' % ten, g)
        # nghe ro mot con loc dung GAN (khong bay) - de danh gia chat tieng
        gan = np.vstack([x1 * 0.75 + x2 * 0.6] * 2)
        tb.ghi(os.path.join(ra, '%s_mot_loc_dung_gan.wav' % ten), gan / np.max(np.abs(gan)) * 0.95)
