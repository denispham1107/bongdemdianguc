# -*- coding: utf-8 -*-
# TIENG MAY GIONG (16/10/2026, nguoi dung): "them am thanh stereo cho skill May giong, chi can them hieu ung am thanh GIONG va am thanh MUA ROI, cac am
# thanh tu cac tia set thi cho giong nhu Sam set". Trong suong mu thi khong nghe (TiengViTri).
# May giong song 0,35 s tu may + 5 s dung yen + 4 s tu bay (1,5 m/s) - hai tieng VONG LAP bam theo dam may:
#   giong - gio giat tung con + sam ren cuon LIEN TUC (khong tieng no - tieng no la cua tung tia, dung SamSetDanh);
#   mua   - mua rao: hang tram hat mua li ti / giay (moi hat mot cho trai / phai) + hat to "lop" + man "shhh" + nuoc chay.
# Tia set: dung NGUYEN tieng Sam set (SamSetDanh + SamSetChay) - khong lam o day.
# Vong lap noi lien mach: su kien dat theo modulo, loc FFT tron vong, lop sinh dai gap doi roi ghep nua sau vao nua dau bang cross-fade equal-power.
# Quy trinh memory: rieng tung kenh + phan giua ~0,25, < 200 Hz <= ~25%, > 4 kHz <= ~10%, mo phong ra loa bang cach phat CAN BANG.
#
# Ba MAU:
#   A "giong_bao" - gio giat manh + sam ren day lien tuc; mua rao to.
#   B "mua_rao"   - MUA la chinh (rao rao day + lop dop hat to); giong nhe: ren xa, gio vua.
#   C "giong_xa"  - sam ren tram xa cuon dai, gio hu nhe; mua vua deu.
#
# Chay: python CongCu/AmThanh/tieng_may_giong.py [thu_muc_ra] [loc_ten_mau...]   (mac dinh CongCu/AmThanh/MayGiongThu)

import os, sys, wave
import numpy as np
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import tieng_bam as tb
import tieng_qua_cau_lua_3 as q3
import tieng_tang_hinh as th
import tieng_giat_set as g1
import tieng_giat_set_3 as g3
import tieng_sam_set as ss

SR = tb.SR
PI2 = tb.PI2
GIAY_GIONG, GIAY_MUA = 4.0, 3.0
loc, dai, thap, cao, cham = q3.loc, q3.dai, q3.thap, q3.cao, q3.cham
on2, loc2 = th.on2, th.loc2
xeo, dat = g1.xeo, g3.dat


def vong_tron(f, n, r):
    """Sinh lop dai 2n roi ghep nua sau vao nua dau bang cross-fade EQUAL-POWER -> noi lien mach (lop on khong tuan hoan)."""
    x = f(2 * n)
    a = np.sqrt(np.linspace(0, 1, n))[:, None]
    return x[:n] * a + x[n:] * np.sqrt(1 - a ** 2)


def gio(n, r, manh=1.0, f=(250, 1200)):
    """GIO GIAT: on dai rong, tam dai TROI cham (gio rit len / ha xuong), bien do theo tung CON giat 0,3-0,6 Hz; rieng tung kenh, con giat lech nhau."""
    t = np.arange(n) / SR
    z = on2(n, r, 0.35)
    ra = np.zeros((n, 2))
    for k in range(2):
        tam = np.exp(np.log(f[0] * f[1]) / 2 + 0.45 * cham(n, 0.4, r))
        y = th.quet(z[:, k], tam, 1.2)
        con = np.maximum(cham(n, r.uniform(0.3, 0.6), r) * 0.55 + 0.8, 0.08) ** 2
        ra[:, k] = y / (np.max(np.abs(y)) + 1e-12) * con
    return ra / (np.max(np.abs(ra)) + 1e-12) * manh


def ren_lien_tuc(n, r, manh=1.0, so_dot_moi_giay=1.6):
    """SAM REN cuon LIEN TUC: nhieu dot ren (ss.ren) noi nhau, khong tieng no."""
    ra = np.zeros((n, 2))
    tt = 0.0
    while tt < n / SR:
        d = r.uniform(1.2, 2.4)
        m = int((d + 1.0) * SR)
        x = ss.ren(m, r, 0.0, d, r.uniform(0.4, 1.0), (2, 4), 0)
        i = int(tt * SR); e = min(m, n - i)
        if e > 0: ra[i:i + e] += x[:e]
        tt += r.exponential(1.0 / so_dot_moi_giay)
    ra += ss.than_tram(n, r, 0.0, 1e9, 0.35)          # nen tram rung rinh suot
    return ra / (np.max(np.abs(ra)) + 1e-12) * manh


def hat_mua(n, r, mat_do, to=(0.05, 0.4), f=(1500, 3800), tat=(0.001, 0.004), rong=1.0):
    """Hat mua li ti: moi hat mot xung on 1-4 ms qua dai, mot cho trai / phai (chi am luong), do to ngau nhien (phan bo luy thua). Tron vong."""
    ra = np.zeros((n, 2))
    so = int(mat_do * n / SR)
    mau = []
    for _ in range(6):
        d = int(r.uniform(*tat) * SR) + 8
        fc = np.exp(r.uniform(np.log(f[0]), np.log(f[1])))
        k = loc(np.pad(r.standard_normal(d) * np.exp(-np.arange(d) / (d * 0.35)), (0, 256)), dai(fc, 1.3))
        mau.append(k / (np.max(np.abs(k)) + 1e-12))
    vt = r.integers(0, n, so)
    a = np.exp(r.uniform(np.log(to[0]), np.log(to[1]), so))
    g = (np.clip(r.uniform(-1, 1, so) * rong, -1, 1) + 1) * np.pi / 4
    loai = r.integers(0, 6, so)
    for i, ai, gi, li in zip(vt, a, g, loai):
        k = mau[li]; idx = (i + np.arange(len(k))) % n
        ra[idx, 0] += k * ai * np.cos(gi); ra[idx, 1] += k * ai * np.sin(gi)
    return ra


def mua(n, r, day=1.0, lop_dop=1.0):
    """MUA RAO: hat li ti (mat do 600-1200 / giay) + hat to "lop" (dai 400-1200 Hz, tat 8-20 ms, 20-50 / giay) + man "shhh" (on 600-3500 Hz rieng
    tung kenh, phap phong) + nuoc chay (on 250-700 Hz)."""
    x = hat_mua(n, r, 900 * day, (0.05, 0.5), (1500, 3300), (0.001, 0.004)) * 1.0
    x += hat_mua(n, r, 35 * lop_dop, (0.2, 1.0), (380, 1200), (0.008, 0.02)) * 1.6
    shh = loc2(on2(n, r, 0.3), lambda ff: cao(600, 2)(ff) * thap(3500, 4)(ff)) * np.maximum(cham(n, 0.7, r) * 0.2 + 1, 0.5)[:, None]
    x += shh / (np.max(np.abs(shh)) + 1e-12) * 0.55 * day
    nuoc = loc2(on2(n, r, 0.3), lambda ff: cao(250, 2)(ff) * thap(700, 2)(ff)) * np.maximum(cham(n, 2.0, r) * 0.35 + 1, 0.2)[:, None]
    x += nuoc / (np.max(np.abs(nuoc)) + 1e-12) * 0.35
    return x


def vang_tron(x, r, giay=0.5, muc=0.4):
    """Vang moi kenh mot xung, tich chap TRON VONG (khong vet noi)."""
    n = len(x)
    m = int(giay * SR); tm = np.arange(m) / SR
    out = x.copy()
    for k in range(2):
        h = loc(r.standard_normal(m), lambda f: cao(200, 2)(f) * thap(4000, 2)(f)) * np.exp(-tm / (giay / 4))
        h[:int(0.012 * SR)] = 0
        h /= np.sqrt(np.sum(h ** 2)) + 1e-12
        hh = np.zeros(n); hh[:m] = h
        out[:, k] += np.fft.irfft(np.fft.rfft(x[:, k]) * np.fft.rfft(hh), n) * muc
    return out


CAU_HINH = {
    # kieu: (gio, ren, ren dot / giay, mua day, lop dop)
    'giong_bao': (1.0, 1.0, 1.8, 1.2, 1.2),
    'mua_rao': (0.5, 0.5, 1.0, 1.5, 1.6),
    'giong_xa': (0.35, 1.0, 0.9, 0.9, 0.8),
}


def giong(hat, kieu):
    m_gio, m_ren, dps, _, _ = CAU_HINH[kieu]
    r = np.random.default_rng(hat)
    n = int(SR * GIAY_GIONG)
    x = vong_tron(lambda m: ren_lien_tuc(m, r, m_ren, dps), n, r)
    x += gio(n, r, m_gio, (220, 1100) if kieu != 'giong_xa' else (300, 900))
    x = vang_tron(ss.tam(x, 0.25), r, 1.2, 0.5)
    return q3.chuan(x, -4, vong=True)


def mua_vong(hat, kieu):
    _, _, _, day, lop = CAU_HINH[kieu]
    r = np.random.default_rng(hat)
    n = int(SR * GIAY_MUA)
    x = mua(n, r, day, lop)
    # (do lan 1: them phan giua 0,25 -> tuong quan 0,33-0,37, > 4 kHz 10%; hat mua da rai trai / phai nen bo phan giua)
    x = vang_tron(x, r, 0.4, 0.35)
    return q3.chuan(x, -5, vong=True)


MAU = {'A_giong_bao': ('giong_bao', 18100), 'B_mua_rao': ('mua_rao', 18300), 'C_giong_xa': ('giong_xa', 18500)}


def doc(p):
    w = wave.open(p); return np.frombuffer(w.readframes(w.getnframes()), '<i2').reshape(-1, 2) / 32768


def ghep_game(gi, mu, danhs, chays, phat=g3.can_bang):
    """MO PHONG TRONG GAME: nguoi choi giua man, May giong o +15 do; 0,35 s may tu -> giong + mua bat dau (len 0,3 s), 36 tia (0,25 s / tia) rai -25..+55
    do dung TIENG SAM SET (danh + chay, toi da 4 + 4); 5,35 s may tu bay sang +40 do trong 4 s; het (9,35 s) tat dan 0,4 s."""
    T = 10.0
    n = int(SR * T)
    out = np.zeros((n, 2))
    r = np.random.default_rng(9)
    t = np.arange(n) / SR
    az = np.where(t < 5.35, 15.0, 15.0 + 25.0 * np.clip((t - 5.35) / 4.0, 0, 1))
    bao = np.clip((t - 0.35) / 0.3, 0, 1) * np.clip((9.75 - t) / 0.4, 0, 1)
    for x, he in ((gi, 0.8), (mu, 0.75)):
        v = np.vstack([x] * int(np.ceil(n / len(x)) + 1))[:n]
        out += phat(v, az) * bao[:, None] * he
    song_d, song_c = [], []
    for i in range(36):
        bd = 0.35 + i * 0.25
        a = (15.0 if bd < 5.35 else 15.0 + 25.0 * min(1, (bd - 5.35) / 4.0)) + r.uniform(-30, 30)
        song_d = [x for x in song_d if x > bd]; song_c = [x for x in song_c if x > bd]
        if len(song_d) < 4:
            x = danhs[r.integers(0, 3)]; tb.cong(out, phat(x, a) * 0.6, bd); song_d.append(bd + len(x) / SR * 0.5)
        if len(song_c) < 4:
            x = chays[r.integers(0, 3)]; tb.cong(out, phat(x, a) * 0.5, bd + 0.05); song_c.append(bd + len(x) / SR * 0.6)
    return out / np.max(np.abs(out)) * 0.95


if __name__ == '__main__':
    ra = sys.argv[1] if len(sys.argv) > 1 else os.path.join('CongCu', 'AmThanh', 'MayGiongThu')
    os.makedirs(ra, exist_ok=True)
    chi = sys.argv[2:]
    dS = os.path.join('CongCu', 'AmThanh', 'SamSetThu')
    danhs = [doc(os.path.join(dS, 'B2_danh%d.wav' % i)) for i in (1, 2, 3)]
    chays = [doc(os.path.join(dS, 'B2_chay%d.wav' % i)) for i in (1, 2, 3)]
    for ten, (kieu, hat0) in MAU.items():
        if chi and not any(c in ten for c in chi): continue
        gi = giong(hat0, kieu); mu = mua_vong(hat0 + 100, kieu)
        for loai, x in (('giong', gi), ('mua', mu)):
            tb.ghi(os.path.join(ra, '%s_%s.wav' % (ten, loai)), x)
            th.do('%s_%s' % (ten, loai), x)
            dd = np.abs(np.diff(x[:, 0]))
            m = x.mean(axis=1); e = np.sqrt(np.convolve(m ** 2, np.ones(4410) / 4410, 'same'))[4410:-4410]
            print('   %s: noi %.4f (buoc quanh %.4f / %.4f), bao bien thien %.2f; game can bang %.2f | 3D spread 120 %.2f' % (loai, abs(x[0, 0] - x[-1, 0]),
                  dd[-1], dd[0], (e.max() - e.min()) / e.max(), g3.tq(g3.can_bang(x, 15)), g3.tq(g3.unity(x, 15, 120))))
        g = ghep_game(gi, mu, danhs, chays)
        tb.ghi(os.path.join(ra, '%s_ghep_nhu_trong_game.wav' % ten), g)
        th.do('%s_ghep_game' % ten, g)
