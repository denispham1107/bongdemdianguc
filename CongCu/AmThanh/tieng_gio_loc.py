# -*- coding: utf-8 -*-
# TIENG GIO LOC (16/10/2026, nguoi dung): "them am thanh stereo cho skill Gio loc - am thanh GIO LOC THOI va am thanh BUI BI CUON THEO (chan that stereo
# nhu gio loc thoi manh ngoai doi that, bao dam KHONG BI UNITY BOP)". Trong suong mu thi khong nghe (TiengViTri).
# Gio loc: 2 con (cap 5: 3) bay 9,5 m/s, song 4,5 s -> hai VONG LAP 3 s bam theo TUNG con loc:
#   thoi - gio xoay: ON loc dai TROI (xoay) + bien do / tam dai DAO trai <-> phai theo vong xoay (so chu ky nguyen trong vong) + gam tram cuon
#          (nhieu loan 150-600 Hz) + hut gio "vu vu" (dai cong huong rong troi cham - khong truot cao do nhanh: khong laser);
#   bui  - cat / soi / la kho bi cuon: hat cat li ti day dac (lao xao), soi nho lach cach, la xao xac - moi hat mot cho, ca dam QUAY vong.
# Vong lap noi lien mach: su kien theo modulo, loc FFT tron vong, on khong tuan hoan -> tieng_may_giong.vong_tron.
# Phat CAN BANG (Editor / may tinh 2D + panStereo - TiengViTri.PhatCanBang). Quy trinh memory: rieng tung kenh + phan giua ~0,25 (tieng_sam_set.tam),
# < 200 Hz <= ~25%, > 4 kHz <= ~10%, 3 bien the... (vong lap: 1 file moi loai), mo phong ra loa truoc khi gui.
#
# Ba MAU:
#   A "loc_gam"   - lốc GAM manh: gio cuon day, tram, xoay nhanh; bui cat day.
#   B "loc_rit"   - gio RIT / hu (dai cong huong hep hon, troi cao thap theo con giat) + bui thua, soi lach cach.
#   C "loc_cat"   - BAO CAT: lop cat xoay ro nhat (xao xac day, la kho), gio vua.
#
# Chay: python CongCu/AmThanh/tieng_gio_loc.py [thu_muc_ra] [loc_ten_mau...]   (mac dinh CongCu/AmThanh/GioLocThu)

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

SR = tb.SR
PI2 = tb.PI2
GIAY = 3.0
loc, dai, thap, cao, cham = q3.loc, q3.dai, q3.thap, q3.cao, q3.cham
on2, loc2 = th.on2, th.loc2
xeo, dat = g1.xeo, g3.dat


def xoay_lr(x, hz, sau, pha):
    """Dao trai <-> phai theo vong xoay (bao nguoc pha hai kenh) - hz phai cho so chu ky nguyen trong vong lap."""
    t = np.arange(len(x)) / SR
    s = np.sin(PI2 * hz * t + pha)
    y = x.copy()
    y[:, 0] *= 1 - sau * (0.5 + 0.5 * s)
    y[:, 1] *= 1 - sau * (0.5 - 0.5 * s)
    return y


def gio_xoay(n, r, f=(300, 1400), q=1.0, hz_xoay=3.0, sau=0.6, gam=1.0):
    """GIO XOAY: ON qua dai TROI (tam dai len xuong theo con giat + nhip xoay), dao trai <-> phai theo vong xoay; + gam tram cuon 150-600 Hz."""
    t = np.arange(n) / SR
    z = on2(n, r, 0.35)
    ra = np.zeros((n, 2))
    pha = r.uniform(0, PI2)
    for k in range(2):
        tam = np.exp(np.log(f[0] * f[1]) / 2 + 0.35 * cham(n, 0.5, r) + 0.18 * np.sin(PI2 * hz_xoay * t + pha + k * np.pi))
        y = th.quet(z[:, k], tam, q)
        ra[:, k] = y / (np.max(np.abs(y)) + 1e-12)
    ra *= np.maximum(cham(n, 0.6, r) * 0.3 + 1, 0.3)[:, None]
    ra = xoay_lr(ra, hz_xoay, sau, pha)
    g = loc2(on2(n, r, 0.45), lambda ff: cao(140, 2)(ff) * thap(600, 2)(ff)) * np.maximum(cham(n, 4, r) * 0.4 + 1, 0.1)[:, None]
    ra += xoay_lr(g / (np.max(np.abs(g)) + 1e-12), hz_xoay, sau * 0.5, pha + 0.8) * 0.7 * gam
    return ra / (np.max(np.abs(ra)) + 1e-12)


def bui_cuon(n, r, mat_cat=1500, mat_soi=25, mat_la=8, hz_xoay=3.0):
    """BUI BI CUON: cat li ti (mg.hat_mua dai 1,2-3,4 kHz, ngan) + lop cat lao xao (xeo) + soi lach cach (hat to 600-1800 Hz) + la kho xao xac (chum xeo
    ngan 30-80 ms); ca dam QUAY trai <-> phai theo vong xoay."""
    # (do lan 1: hat dat deu -1..1 bang am luong -> tuong quan ~0,32 tu than; cat C > 4 kHz 18%) - day hat ra hai ben (rong 1,6), dai cat thap hon
    x = mg.hat_mua(n, r, mat_cat, (0.04, 0.4), (1100, 2800), (0.0008, 0.003), rong=1.6) * 0.8
    x += xeo(n, r, np.maximum(cham(n, 2, r) * 0.3 + 1, 0.2), 2500, (700, 2800), chung=0.15, giat=12) * 0.35
    x += mg.hat_mua(n, r, mat_soi, (0.2, 1.0), (600, 1800), (0.004, 0.012), rong=1.6) * 1.2
    la = np.zeros((n, 2))
    for _ in range(int(mat_la * n / SR)):
        d = r.uniform(0.03, 0.08); m = int((d + 0.01) * SR); tm = np.arange(m) / SR
        b = np.clip(tm / 0.005, 0, 1) * np.clip((d - tm) / 0.01, 0, 1)
        v = xeo(m, r, b + 1e-4, 4000, (700, 2800), chung=0.3, giat=40)
        v = dat(v.mean(axis=1), r.uniform(-0.9, 0.9)) * r.uniform(0.2, 0.6)
        idx = (r.integers(0, n) + np.arange(m)) % n
        la[idx] += v
    x += la
    return xoay_lr(x, hz_xoay, 0.5, r.uniform(0, PI2))


CAU_HINH = {
    # kieu: (dai gio, Q, sau xoay, gam, cat, soi, la, vong / giay)
    'loc_gam': ((220, 1100), 0.9, 0.6, 1.3, 1800, 30, 8, 3.0),
    'loc_rit': ((450, 1800), 2.2, 0.55, 0.6, 900, 40, 4, 2.0),
    'loc_cat': ((300, 1300), 1.0, 0.5, 0.9, 2600, 25, 14, 2.67),
}


def thoi(hat, kieu):
    f, q, sau, gam, _, _, _, hz = CAU_HINH[kieu]
    r = np.random.default_rng(hat)
    n = int(SR * GIAY)
    x = mg.vong_tron(lambda m: gio_xoay(m, r, f, q, hz, sau, gam), n, r)
    x = mg.vang_tron(ss.tam(x, 0.2), r, 0.8, 0.4)
    return q3.chuan(x, -4, vong=True)


def bui(hat, kieu):
    _, _, _, _, cat, soi, la, hz = CAU_HINH[kieu]
    r = np.random.default_rng(hat)
    n = int(SR * GIAY)
    x = bui_cuon(n, r, cat, soi, la, hz)
    x = loc2(x, thap(3600, 4))      # (do lan 2: hat ngan 0,8 ms qua dai Q 1,3 lot len cao - C > 4 kHz 16%)
    x = mg.vang_tron(x, r, 0.4, 0.3)
    return q3.chuan(x, -5, vong=True)


MAU = {'A_loc_gam': ('loc_gam', 19100), 'B_loc_rit': ('loc_rit', 19300), 'C_loc_cat': ('loc_cat', 19500)}


def ghep_game(th_, bu, so_loc=2, phat=g3.can_bang):
    """MO PHONG TRONG GAME: nguoi choi giua man, tung Gio loc ve phia truoc-phai: 2 loc (+-15 do quanh +20 do) bay 9,5 m/s 4,5 s tu 1 m truoc mat ra xa;
    am luong theo khoang cach (TiengViTri: 1 trong 4 m -> 0,25 o 25 m) va SUONG (tat dan qua 25 m); hai lan tung (0,3 s va 5,3 s)."""
    n = int(SR * 10.5)
    out = np.zeros((n, 2))
    for g in (0.3, 5.3):
        for j in range(so_loc):
            huong = 20.0 + (j - (so_loc - 1) / 2) * 30.0
            m = int(4.5 * SR); tt = np.arange(m) / SR
            d = 1.0 + 9.5 * tt
            # goc tren loa: loc o khoang cach d theo huong 'huong' so voi truoc mat (nhin tu nguoi choi o giua, camera sau lung)
            x_ = d * np.sin(np.radians(huong)); z_ = d * np.cos(np.radians(huong)) + 12.0   # camera ~12 m sau nguoi choi
            az = np.degrees(np.arctan2(x_, z_))
            kc = np.where(d <= 4, 1.0, np.where(d <= 25, 1 - 0.75 * (d - 4) / 21, 0.25 * np.clip(1 - (d - 25) / 20, 0, 1)))
            thay = np.clip((25 - d) / 3.0, 0, 1)
            b = kc * thay * np.minimum(1, tt / 0.1) * np.minimum(1, (4.5 - tt) / 0.1)
            for x, he in ((th_, 0.7), (bu, 0.6)):
                i0 = int(r_off[j] * SR) % len(x)
                v = np.vstack([x] * 3)[i0:i0 + m]
                tb.cong(out, phat(v, az) * b[:, None] * he, g)
    return out / np.max(np.abs(out)) * 0.95


r_off = [0.0, 1.3, 2.1]

if __name__ == '__main__':
    ra = sys.argv[1] if len(sys.argv) > 1 else os.path.join('CongCu', 'AmThanh', 'GioLocThu')
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
            print('   %s: noi %.4f (buoc quanh %.4f / %.4f), bao bien thien %.2f; game can bang %.2f | 3D spread 120 %.2f' % (loai, abs(x[0, 0] - x[-1, 0]),
                  dd[-1], dd[0], (e.max() - e.min()) / e.max(), g3.tq(g3.can_bang(x, 15)), g3.tq(g3.unity(x, 15, 120))))
        g = ghep_game(x1, x2)
        tb.ghi(os.path.join(ra, '%s_ghep_nhu_trong_game.wav' % ten), g)
        th.do('%s_ghep_game' % ten, g)
        gd = ghep_game(x1, x2, phat=lambda x, az: g3.unity(x, az, 120))
        print('   ghep DOI CHUNG phat 3D spread 120: tq %.2f' % g3.tq(gd))
