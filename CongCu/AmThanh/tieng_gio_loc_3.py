# -*- coding: utf-8 -*-
# TIENG GIO LOC - BAN 3 (16/10/2026). Nguoi dung nghe ban 2 (tieng_gio_loc_2.py - vu xoay / gam tau / hu gio): "van con qua te, hay them NHIEU tieng GIO HU,
# tieng GIO THOI, hien con qua GIA TAO, khong he giong tieng gio loc".
# Vi sao gia: (1) VONG LAP 3 s ma con loc song 4,5 s - doan lap nghe ro; (2) chat gio DUNG YEN - gio that co TOC DO doi lien tuc theo con giat, tieng hu
# CAO LEN khi gio manh, thap xuong khi gio yeu, tieng thoi sang len khi gio manh; ban 1-2 chi dieu bien nhe quanh mot muc.
# Ban 3 (cach tong hop gio cua Farnell "Designing Sound"):
#   - TOC DO GIO w(t) 0..1: len nhanh luc loc hinh thanh, 2-4 CON GIAT manh (squall) + rung nhieu loan nhanh, tat dan luc loc tan;
#   - GIO HU: 4-6 dai cong huong HEP (Q 10-25) tren on RIENG, tam = goc x (0,65 + 0,7 w) -> "hu u u" len xuong theo con giat (troi tu tu - khong
#     truot nhanh kieu laser), to theo w^2, moi dai mot cho, xoay nhe trai <-> phai;
#   - GIO THOI: on thong thap fc = 250 + 2600 w (sang len khi manh), to theo w, rieng tung kenh 40% chung;
#   - DAP GIO tram: on 70-300 Hz x w^2 x nhieu loan 3-10 Hz;
#   - BUI CUON theo con giat (cat lao xao + soi + phan phat) x w.
# MOT CLIP 4,8 s cho TUNG con loc (khong lap), 3 bien the khac cau truc (so con giat, cho giat, so dai hu, cao do hu).
#
# Hai MAU:
#   A "hu_lon"    - gio HU nhieu va to (6 dai, Q cao) + thoi vua + bui.
#   B "thoi_manh" - gio THOI manh / dap gio day + hu vua (4 dai) + bui.
#
# Chay: python CongCu/AmThanh/tieng_gio_loc_3.py [thu_muc_ra] [loc_ten_mau...]   (mac dinh CongCu/AmThanh/GioLocThu3)

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
import tieng_gio_loc_2 as gl2

SR = tb.SR
PI2 = tb.PI2
GIAY = 4.8
loc, dai, thap, cao, cham = q3.loc, q3.dai, q3.thap, q3.cao, q3.cham
on2, loc2 = th.on2, th.loc2
xeo, dat = g1.xeo, g3.dat


def toc_do_gio(n, r, so_giat):
    """w(t): len nhanh 0,15-0,35 s, nen 0,45-0,6, so_giat con giat manh (dinh 0,85-1, len 0,3-0,6 s, ha 0,5-1 s), nhieu loan nhanh, tat 0,4 s cuoi."""
    t = np.arange(n) / SR
    T = n / SR
    w = (0.45 + 0.15 * r.random()) * np.ones(n) + 0.08 * cham(n, 0.8, r)
    cho = np.sort(r.uniform(0.4, T - 1.0, so_giat))
    for c in cho:
        a = r.uniform(0.3, 0.5)
        len_, ha = r.uniform(0.3, 0.6), r.uniform(0.5, 1.0)
        tt = t - c
        w += a * np.where(tt < 0, np.exp(-(tt / len_) ** 2 * 3), np.exp(-tt / ha))
    w += 0.06 * cham(n, 6, r)                        # nhieu loan nhanh
    vao = np.clip(t / r.uniform(0.15, 0.35), 0, 1) ** 0.7
    ra = np.clip((T - 0.05 - t) / 0.4, 0, 1)
    return np.clip(w, 0, 1.1) * vao * ra


def hu(n, r, w, so, q, f_goc, nang=1.0):
    """GIO HU: 'so' dai cong huong hep Q 'q' tren on rieng, tam = goc x (0,65 + 0,7 w) (troi tu tu theo w), to theo w^2; moi dai mot cho + xoay nhe."""
    t = np.arange(n) / SR
    ra = np.zeros((n, 2))
    goc = np.exp(r.uniform(np.log(f_goc[0]), np.log(f_goc[1]), so))
    cho = np.linspace(-0.8, 0.8, so) + r.uniform(-0.1, 0.1, so); r.shuffle(cho)
    for j in range(so):
        tam = goc[j] * (0.65 + 0.7 * w) * np.exp(0.05 * cham(n, r.uniform(0.5, 1.5), r))
        z = on2(n, r, 0.5)
        qq = r.uniform(*q)
        y = np.stack([th.quet(z[:, k], np.maximum(tam * (1 + 0.012 * (k - 0.5)), 80), qq, so=24) for k in range(2)], axis=1)
        y /= np.max(np.abs(y)) + 1e-12
        lech = np.clip(cho[j] + 0.25 * np.sin(PI2 * r.uniform(1.2, 2.5) * t + r.uniform(0, PI2)), -1, 1)
        g = (lech + 1) * np.pi / 4
        mu = (w ** 2) * r.uniform(0.5, 1.0)
        ra[:, 0] += y[:, 0] * np.cos(g) * mu; ra[:, 1] += y[:, 1] * np.sin(g) * mu
    return ra / (np.max(np.abs(ra)) + 1e-12) * nang


def thoi(n, r, w, nang=1.0):
    """GIO THOI: on thong thap fc = 250 + 2600 w (moi kenh lech nhe), to theo w; rieng tung kenh 40% chung."""
    z = on2(n, r, 0.4)
    ra = np.stack([th.quet(z[:, k], 250 + 2600 * np.clip(w, 0, 1) * (1 + 0.05 * (k - 0.5)), kieu='thap') for k in range(2)], axis=1)
    ra = loc2(ra, lambda ff: cao(140, 2)(ff))
    ra = ra / (np.max(np.abs(ra)) + 1e-12) * w[:, None]
    return ra / (np.max(np.abs(ra)) + 1e-12) * nang


def dap_gio(n, r, w, nang=1.0):
    """DAP GIO tram: on 70-300 Hz x w^2 x nhieu loan 3-10 Hz (luong gio dap vao tai / vat)."""
    # (do lan 1: 70-300 Hz -> sau bo loc 120 Hz cua chuan < 200 Hz chi 1-3%) - dai 120-320 Hz
    z = loc2(on2(n, r, 0.4), lambda ff: cao(120, 2)(ff) * thap(320, 2)(ff))
    am = np.maximum(cham(n, 6, r) * 0.5 + 1, 0.05) ** 1.5
    ra = z / (np.max(np.abs(z)) + 1e-12) * (w ** 2 * am)[:, None]
    return ra / (np.max(np.abs(ra)) + 1e-12) * nang


def bui_theo_gio(n, r, w, nang=1.0):
    x = gl.bui_cuon(n, r, 1600, 35, 7, r.uniform(1.5, 2.5))
    x += gl2.phan_phat(n, r, 5.0, 0.8)
    x = loc2(x, thap(3600, 4))
    x = x / (np.max(np.abs(x)) + 1e-12) * (np.clip(w, 0, 1) ** 1.5)[:, None]
    return x * nang


CAU_HINH = {
    # kieu: (so dai hu, Q hu, tam goc hu, muc hu, muc thoi, muc dap, muc bui)
    'hu_lon': ((5, 7), (12, 25), (380, 1100), 1.0, 0.6, 0.75, 0.45),
    'thoi_manh': ((3, 5), (8, 16), (330, 900), 0.6, 1.0, 1.2, 0.5),
}


def gio_loc(hat, kieu):
    so_hu, q_hu, f_hu, m_hu, m_thoi, m_dap, m_bui = CAU_HINH[kieu]
    r = np.random.default_rng(hat)
    n = int(SR * GIAY)
    w = toc_do_gio(n, r, int(r.integers(2, 5)))
    x = hu(n, r, w, int(r.integers(*so_hu)), q_hu, f_hu, m_hu)
    x += thoi(n, r, w, m_thoi)
    x += dap_gio(n, r, w, m_dap)
    x += bui_theo_gio(n, r, w, m_bui)
    x = ss.tam(x, 0.15)
    x = mg.vang_tron(np.vstack([x, np.zeros((int(0.6 * SR), 2))]), r, 1.0, 0.4)[:n]
    return q3.chuan(x, -3)


MAU = {'A_hu_lon': ('hu_lon', 21100), 'B_thoi_manh': ('thoi_manh', 21300)}


def ghep_game(ds, phat=g3.can_bang):
    """Nhu tieng_gio_loc.ghep_game nhung moi con loc phat MOT clip (bien the khac nhau), khong lap."""
    n = int(SR * 10.5)
    out = np.zeros((n, 2))
    k = 0
    for g in (0.3, 5.3):
        for j in range(2):
            x = ds[k % len(ds)]; k += 1
            huong = 20.0 + (j - 0.5) * 30.0
            m = min(len(x), int(4.5 * SR)); tt = np.arange(m) / SR
            d = 1.0 + 9.5 * tt
            az = np.degrees(np.arctan2(d * np.sin(np.radians(huong)), d * np.cos(np.radians(huong)) + 12.0))
            kc = np.where(d <= 4, 1.0, np.where(d <= 25, 1 - 0.75 * (d - 4) / 21, 0.25 * np.clip(1 - (d - 25) / 20, 0, 1)))
            thay = np.clip((25 - d) / 3.0, 0, 1)
            tb.cong(out, phat(x[:m], az) * (kc * thay)[:, None] * 0.7, g)
    return out / np.max(np.abs(out)) * 0.95



# ================================================================== A2 (nguoi dung chon "A_hu_lon_ba_loc_dung_gan" nhung: "cho tieng GIO HU NHIEU, DAI hon, NHO BOT am luong
# tieng gio loc lai 1 chut")
# A2: 6-8 dai hu (A: 5-6); hu to theo w^1,1 thay w^2 + w cua lop hu lam MUOT (troi cham 0,6 s) -> hu keo dai deu, khong chi vut len theo con giat;
# clip 5,8 s: loc tan o 4,5 s (trong game TiengViTri.ThaRa - tieng o lai cho cu chay het) roi tieng hu NGAN them ~1,3 s tat dan; thoi / dap / bui x0,75.
GIAY_A2 = 5.8


def toc_do_gio_a2(n, r):
    t = np.arange(n) / SR
    m = int(4.6 * SR)
    w = np.zeros(n)
    w[:m] = toc_do_gio(m, r, int(r.integers(2, 5)))
    con = np.maximum(t - 4.4, 0)
    w[m - int(0.25 * SR):] = np.maximum(w[m - int(0.25 * SR):], 0)
    duoi = (w[m - int(0.35 * SR)] if m > 0 else 0.6) * np.exp(-con / 0.45) * (t > 4.25)
    return np.maximum(w, duoi)


def muot(w, giay):
    k = int(giay * SR)
    if k < 2: return w
    ker = np.hanning(k); ker /= ker.sum()
    return np.convolve(np.pad(w, (k, k), mode='edge'), ker, 'same')[k:-k]


def hu_a2(n, r, w, so, q, f_goc, nang=1.0):
    t = np.arange(n) / SR
    ws = muot(w, 0.6)
    ra = np.zeros((n, 2))
    goc = np.exp(r.uniform(np.log(f_goc[0]), np.log(f_goc[1]), so))
    cho = np.linspace(-0.85, 0.85, so) + r.uniform(-0.08, 0.08, so); r.shuffle(cho)
    for j in range(so):
        tam = goc[j] * (0.7 + 0.6 * ws) * np.exp(0.05 * cham(n, r.uniform(0.3, 0.9), r))
        z = on2(n, r, 0.5)
        qq = r.uniform(*q)
        y = np.stack([th.quet(z[:, k], np.maximum(tam * (1 + 0.012 * (k - 0.5)), 80), qq, so=24) for k in range(2)], axis=1)
        y /= np.max(np.abs(y)) + 1e-12
        lech = np.clip(cho[j] + 0.2 * np.sin(PI2 * r.uniform(0.6, 1.5) * t + r.uniform(0, PI2)), -1, 1)
        g = (lech + 1) * np.pi / 4
        mu = (np.clip(ws, 0, 1.1) ** 1.1) * r.uniform(0.55, 1.0) * np.maximum(cham(n, r.uniform(0.2, 0.5), r) * 0.25 + 1, 0.3)
        ra[:, 0] += y[:, 0] * np.cos(g) * mu; ra[:, 1] += y[:, 1] * np.sin(g) * mu
    return ra / (np.max(np.abs(ra)) + 1e-12) * nang


def gio_loc_a2(hat):
    r = np.random.default_rng(hat)
    n = int(SR * GIAY_A2)
    w = toc_do_gio_a2(n, r)
    x = hu_a2(n, r, w, int(r.integers(6, 9)), (12, 25), (380, 1100), 1.0)
    wl = w * (np.arange(n) / SR < 4.7)          # thoi / dap / bui chi khi con loc con song
    x += thoi(n, r, wl, 0.6 * 0.75)
    x += dap_gio(n, r, wl + 1e-6, 0.75 * 0.75)
    x += bui_theo_gio(n, r, wl, 0.45 * 0.75)
    x = ss.tam(x, 0.15)
    x = mg.vang_tron(np.vstack([x, np.zeros((int(0.6 * SR), 2))]), r, 1.2, 0.45)[:n]
    return q3.chuan(x, -3)


if __name__ == '__main__' and '--a2' in sys.argv:
    ra = os.path.join('CongCu', 'AmThanh', 'GioLocThu3')
    ds = [gio_loc_a2(21500 + 10 * i) for i in range(3)]
    d0 = [gio_loc(21100 + 10 * i, 'hu_lon') for i in range(3)]
    for i, x in enumerate(ds):
        tb.ghi(os.path.join(ra, 'A2_hu_dai_%d.wav' % (i + 1)), x)
        th.do('A2_hu_dai_%d' % (i + 1), x)
    def ti_hu(x):
        # nang luong dai hep cua hu: phan nang luong 350-1300 Hz so voi toan bo, va thoi gian tren -20 dB
        m = x.mean(axis=1); P = np.abs(np.fft.rfft(m)) ** 2; f = np.fft.rfftfreq(len(m), 1 / SR)
        e = np.sqrt(np.convolve(m ** 2, np.ones(441) / 441, 'same'))
        return P[(f > 350) & (f < 1300)].sum() / P.sum(), np.sum(e > e.max() * 0.1) / SR
    print('   A2: dai hu 350-1300 Hz %s, thoi gian tren -20 dB %s s | ban A %s, %s s; game can bang %s; chenh bien the %s dB' % (
        ' / '.join('%.0f%%' % (100 * ti_hu(x)[0]) for x in ds), ' / '.join('%.2f' % ti_hu(x)[1] for x in ds),
        ' / '.join('%.0f%%' % (100 * ti_hu(x)[0]) for x in d0), ' / '.join('%.2f' % ti_hu(x)[1] for x in d0),
        ' / '.join('%.2f' % g3.tq(g3.can_bang(x, 15)) for x in ds), ' / '.join('%.1f' % c for c in th.chenh_bien_the(ds))))
    gan = np.vstack(ds)
    tb.ghi(os.path.join(ra, 'A2_ba_loc_dung_gan.wav'), gan / np.max(np.abs(gan)) * 0.95)
    sys.exit(0)


# ================================================================== LOC XOAY (nguoi dung nghe A2: "su dung ban am thanh nay cho skill LOC XOAY thay vi Gio loc; cho DO
# DAI nhieu hon, con qua ngan; them tieng SET (tieng Sam set) nhung NHO HON tieng gio hu, gio cuon"). Loc xoay song 6 s (prefab duration 6) roi tan;
# clip 7,6 s = 6 s gio + hu NGAN them ~1,5 s. Cung cach tong hop A2 (6-8 dai hu, hu muot, thoi / dap / bui x0,75). Tieng set do TiengViTri phat rieng.
def gio_loc_xoay(hat, song=6.0, giay=7.6):
    r = np.random.default_rng(hat)
    n = int(SR * giay)
    t = np.arange(n) / SR
    m = int((song + 0.1) * SR)
    w = np.zeros(n)
    w[:m] = toc_do_gio(m, r, int(r.integers(3, 6)))
    duoi = w[m - int(0.45 * SR)] * np.exp(-np.maximum(t - (song - 0.15), 0) / 0.55) * (t > song - 0.3)
    w = np.maximum(w, duoi)
    x = hu_a2(n, r, w, int(r.integers(6, 9)), (12, 25), (380, 1100), 1.0)
    wl = w * (t < song + 0.2)
    x += thoi(n, r, wl, 0.6 * 0.75)
    x += dap_gio(n, r, wl + 1e-6, 0.75 * 0.75)
    x += bui_theo_gio(n, r, wl, 0.45 * 0.75)
    x = ss.tam(x, 0.15)
    x = mg.vang_tron(np.vstack([x, np.zeros((int(0.6 * SR), 2))]), r, 1.2, 0.45)[:n]
    x[-int(0.3 * SR):] *= np.linspace(1, 0, int(0.3 * SR))[:, None]
    return q3.chuan(x, -3)


if __name__ == '__main__' and '--locxoay' in sys.argv:
    import wave
    ra = os.path.join('CongCu', 'AmThanh', 'GioLocThu3')
    ds = [gio_loc_xoay(22100 + 10 * i) for i in range(3)]
    for i, x in enumerate(ds):
        tb.ghi(os.path.join(ra, 'LocXoay_hu_%d.wav' % (i + 1)), x)
        th.do('LocXoay_hu_%d' % (i + 1), x)
    print('   chenh bien the %s dB; game can bang %s' % (' / '.join('%.1f' % c for c in th.chenh_bien_the(ds)),
          ' / '.join('%.2f' % g3.tq(g3.can_bang(x, 15)) for x in ds)))
    # nghe thu nhu trong game: loc xoay o +15 do 6 s, cu 0,45 s mot tia set (tieng Sam set B2 danh x0,35, toi da 4) - nho hon gio hu
    def doc(p_):
        w_ = wave.open(p_); return np.frombuffer(w_.readframes(w_.getnframes()), '<i2').reshape(-1, 2) / 32768
    danhs = [doc(os.path.join('CongCu', 'AmThanh', 'SamSetThu', 'B2_danh%d.wav' % i)) for i in (1, 2, 3)]
    out = np.zeros((int(SR * 18), 2)); rr = np.random.default_rng(3)
    for k, g in enumerate((0.3, 9.3)):
        tb.cong(out, g3.can_bang(ds[k], 15), g)
        song = []
        for i in range(int(6.0 / 0.45)):
            bd = g + 0.45 * (i + 1); song = [x for x in song if x > bd]
            if len(song) < 4:
                x = danhs[rr.integers(0, 3)]; tb.cong(out, g3.can_bang(x, 15 + rr.uniform(-10, 10)) * 0.35, bd); song.append(bd + 1.5)
    tb.ghi(os.path.join(ra, 'LocXoay_ghep_nhu_trong_game.wav'), out / np.max(np.abs(out)) * 0.95)
    sys.exit(0)

if __name__ == '__main__':
    ra = sys.argv[1] if len(sys.argv) > 1 else os.path.join('CongCu', 'AmThanh', 'GioLocThu3')
    os.makedirs(ra, exist_ok=True)
    chi = sys.argv[2:]
    for ten, (kieu, hat0) in MAU.items():
        if chi and not any(c in ten for c in chi): continue
        ds = [gio_loc(hat0 + 10 * i, kieu) for i in range(3)]
        for i, x in enumerate(ds):
            tb.ghi(os.path.join(ra, '%s_%d.wav' % (ten, i + 1)), x)
            th.do('%s_%d' % (ten, i + 1), x)
        print('   chenh bien the %s dB; game can bang %s' % (' / '.join('%.1f' % c for c in th.chenh_bien_the(ds)),
              ' / '.join('%.2f' % g3.tq(g3.can_bang(x, 15)) for x in ds)))
        g = ghep_game(ds)
        tb.ghi(os.path.join(ra, '%s_ghep_nhu_trong_game.wav' % ten), g)
        th.do('%s_ghep_game' % ten, g)
        gan = np.vstack(ds)
        tb.ghi(os.path.join(ra, '%s_ba_loc_dung_gan.wav' % ten), gan / np.max(np.abs(gan)) * 0.95)
