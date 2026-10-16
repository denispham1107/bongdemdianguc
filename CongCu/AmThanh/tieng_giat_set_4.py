# -*- coding: utf-8 -*-
# TIENG GIUT SET + QUA CAU DIEN - BAN 4, HOAN TOAN MOI (16/10/2026). Nguoi dung bo tat ca ban truoc (B2 "zzzt", B3, S1-S3): "lam lai am thanh
# hoan toan moi, nghe ro tieng XET XET, CHIU CHIU LAZER cua set dien, tieng CHAY khi bi set dien danh trung".
# ⚠️ O day nguoi dung MUON chat "laser" (truot cao do nhanh) - khac Mua bang (bi che "nhu sung laser" vi khong muon).
# Thanh phan:
#   - CHIU: "chiu / piu" set dien - cao do TRUOT XUONG nhanh 2,4-3,4 kHz -> 300-600 Hz trong 50-160 ms, giau dieu hoa (FM nhe) + chut on dien;
#     moi cai MOT CHO trai / phai (chi lech am luong - am co cao do);
#   - XET: chum phong dien ngan 15-60 ms (xeo day dac + chop ho quang), lap lai khong deu "xet xet";
#   - CHAY: tieng thit / do vat bi dot "xeeeo" - xeo li ti RAT day (tieng chien mo) + hoi xi thong cao + lop "lep bep" bop nho, 0,4-0,9 s;
#   - KHONG cu no don + "huych" tram (bai hoc ban 1: = tieng sung).
# Stereo: moi lop sinh rieng tung kenh (on doc lap, chung 0,2-0,3), su kien rai trai / phai; mo phong cach phat MOI trong game
# (tieng_giat_set_3.can_bang - giu nguyen hai kenh, chi ha kenh phia xa) + tieng phong CHAY doc tia tu tay toi muc tieu.
# Ba MAU:
#   A "laser_dien"  - CHIU CHIU noi bat (2-4 chiu moi lan phong, lan = 1 chiu) + xet; trung = chiu + chay.
#   B "xet_xet"     - XET XET day dac la chinh, chiu ngan xen vao; trung = xet + chay.
#   C "set_dot_chay" - chiu + xet vua; trung = CHAY to va dai (xeo + khoi xi + lep bep).
#
# Chay: python CongCu/AmThanh/tieng_giat_set_4.py [thu_muc_ra] [loc_ten_mau...]   (mac dinh CongCu/AmThanh/GiatSetThu4)

import os, sys
import numpy as np
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import tieng_bam as tb
import tieng_qua_cau_lua_3 as q3
import tieng_tang_hinh as th
import tieng_giat_set as g1
import tieng_giat_set_3 as g3

SR = tb.SR
PI2 = tb.PI2
GIAY_VONG = 3.0
loc, dai, thap, cao, cham = q3.loc, q3.dai, q3.thap, q3.cao, q3.cham
on2, loc2, keo_lech = th.on2, th.loc2, th.keo_lech
xeo, dat = g1.xeo, g3.dat


def chiu(r, giay=None, f_tren=None, f_duoi=None):
    """MOT tieng "CHIU" (mono): cao do truot xuong nhanh theo ham mu, giau dieu hoa (FM nhe), bao len 2 ms tat dan + chut on dien."""
    giay = r.uniform(0.05, 0.16) if giay is None else giay
    # (do lan 1: 2,4-3,4 kHz -> 300-600 Hz: 200 Hz - 1 kHz chi 12-33%, 1-4 kHz 63-86% - mong, choi)
    f_tren = r.uniform(1900, 2900) if f_tren is None else f_tren
    f_duoi = r.uniform(190, 380) if f_duoi is None else f_duoi
    m = int((giay + 0.05) * SR); t = np.arange(m) / SR
    f = f_duoi + (f_tren - f_duoi) * np.exp(-t / (giay * 0.35))
    pha = PI2 * np.cumsum(f) / SR
    mod = r.uniform(0.6, 1.6) * np.sin(PI2 * np.cumsum(f * r.uniform(1.4, 2.1)) / SR)
    x = np.sin(pha + mod) + 0.35 * np.sin(2 * pha + 0.5 * mod) + 0.15 * np.sin(3 * pha)
    x += loc(r.standard_normal(m), lambda ff: cao(900, 2)(ff) * thap(3200, 3)(ff)) * 0.25 * np.exp(-t / 0.02)
    e = np.minimum(1, t / 0.002) * np.exp(-t / (giay * 0.75))
    x = loc(x * e, thap(3800, 4))
    return x / (np.max(np.abs(x)) + 1e-12)


def chiu_st(r, lech=None, **kw):
    """CHIU STEREO: tieng chinh lech ve phia 'lech' + ban NHAN DOI (cao do x1,05-1,12, truot nhanh / cham hon 10-20%, tre 8-16 ms) o BEN KIA.
    (do lan 1: chiu mono dat gan giua -> phong / lan tuong quan 0,44-0,70.) Hai ban khac cao do nen gop mono khong triet tieu."""
    x = chiu(r, **kw)
    giay = kw.get('giay'); f_tren = kw.get('f_tren'); f_duoi = kw.get('f_duoi')
    k = r.uniform(1.05, 1.12)
    y = chiu(r, giay=(giay if giay is not None else r.uniform(0.05, 0.16)) * r.uniform(0.8, 1.2),
             f_tren=(f_tren if f_tren is not None else r.uniform(1900, 2900)) * k, f_duoi=(f_duoi if f_duoi is not None else r.uniform(190, 380)) * k)
    d = int(r.uniform(0.008, 0.016) * SR)
    y = np.concatenate([np.zeros(d), y])
    m = max(len(x), len(y)); x = np.pad(x, (0, m - len(x))); y = np.pad(y, (0, m - len(y)))
    # (do lan 2: chinh o +-0,4 ro sang ca hai kenh -> phong A van 0,47-0,50; nay hai ban DAT HAN hai ben, lech cao do 5-12%)
    ben = np.sign(lech) if (lech is not None and abs(lech) > 0.05) else r.choice([-1, 1])
    p = ben * r.uniform(0.55, 0.85)
    p2 = -ben * r.uniform(0.55, 0.85)
    return dat(x, p) + dat(y, p2) * r.uniform(0.55, 0.8)


def xet(r, giay=None, mat_do=None):
    """MOT tieng "XET" (stereo, ngan 15-60 ms): xeo day dac rieng tung kenh + chop ho quang ngan."""
    giay = r.uniform(0.015, 0.06) if giay is None else giay
    m = int((giay + 0.03) * SR); t = np.arange(m) / SR
    b = np.clip(t / 0.002, 0, 1) * np.clip((giay - t) / 0.006, 0, 1)
    x = xeo(m, r, b + 1e-3, r.uniform(4000, 8000) if mat_do is None else mat_do, (800, 3400), chung=0.25, giat=60)
    x += xeo(m, r, b + 1e-3, 3000, (350, 1000), chung=0.25, giat=60) * 0.7
    hq = g3.ho_quang(t, r, r.uniform(180, 300), giat=(30, 50), sau=0.5) * b
    x += dat(hq, r.uniform(-0.6, 0.6)) * 0.5
    return x / (np.max(np.abs(x)) + 1e-12) * b[:, None]


def chay(n, r, bd, giay, nang=1.0, rong=0.9):
    """CHAY "xeeeo" (tieng do vat / thit bi set dot): xeo li ti RAT day (chien mo) dai 1-3,5 kHz + hoi xi + lep bep bop nho;
    len 20-40 ms, giu, tat cham; moi kenh rieng."""
    t = np.arange(n) / SR
    tt = t - bd
    b = np.clip(tt / r.uniform(0.02, 0.04), 0, 1) * np.exp(-np.maximum(tt - giay * 0.3, 0) / (giay * 0.5)) * (tt >= 0)
    b *= np.maximum(cham(n, 6, r) * 0.25 + 1, 0.3)
    x = xeo(n, r, b + 1e-4, r.uniform(9000, 14000), (1000, 3500), chung=0.2, giat=25) * 0.8
    x += xeo(n, r, b + 1e-4, 4000, (400, 1200), chung=0.2, giat=20) * 0.5
    xi = loc2(on2(n, r, 0.2), lambda ff: cao(1500, 2)(ff) * thap(3600, 4)(ff))
    x += xi / (np.max(np.abs(xi)) + 1e-12) * b[:, None] * 0.35
    tmp = np.zeros((n + int(0.3 * SR), 2))
    g1.tia_lua(tmp, r, int(r.integers(4, 12) * giay / 0.5), bd + 0.02, bd + giay * 0.8, 0.5, nay=(1, 1), rong=rong)   # lep bep
    x += tmp[:n]
    return x / (np.max(np.abs(x)) + 1e-12) * nang


def rai(tron, r, x2, bd, lech=None):
    """Cong su kien x2 (mono hoac stereo) vao tron tai bd, mono thi dat lech trai / phai bang am luong."""
    if x2.ndim == 1:
        x2 = dat(x2, r.uniform(-0.85, 0.85) if lech is None else lech)
    i = int(bd * SR)
    m = min(len(x2), len(tron) - i)
    if m > 0: tron[i:i + m] += x2[:m]


def hoan_tat(tron, r, giay_vang, muc_vang, dinh_db):
    return th.hoan_tat(tron, r, giay_vang, muc_vang, dinh_db)


# ------------------------------------------------------------------ tieng tung kieu
CAU_HINH = {
    # kieu: (so chiu phong, muc chiu, mat do xet / giay, muc xet, muc chay khi trung, do dai chay)
    'laser_dien': ((2, 5), 1.0, (8, 16), 0.5, 0.55, (0.35, 0.6)),
    'xet_xet': ((1, 3), 0.55, (22, 40), 0.9, 0.55, (0.35, 0.6)),
    'set_dot_chay': ((1, 3), 0.8, (12, 22), 0.6, 1.0, (0.6, 0.95)),
}


def phong(hat, kieu):
    so_chiu, m_chiu, xps, m_xet, _, _ = CAU_HINH[kieu]
    r = np.random.default_rng(hat)
    giu = r.uniform(0.25, 0.55)
    n = int(SR * (giu + 0.8)); tron = np.zeros((n, 2))
    # chuoi CHIU: cai dau o giua-lech nhe, cac cai sau rai hai ben, khoang cach khong deu
    tt = 0.0
    for j in range(int(r.integers(*so_chiu))):
        rai(tron, r, chiu_st(r, r.uniform(-0.4, 0.4) if j == 0 else None) * m_chiu * (1.0 if j == 0 else r.uniform(0.55, 0.9)), tt)
        tt += r.uniform(0.05, 0.14)
    # XET XET suot thoi gian phong
    so_xet = int(r.uniform(*xps) * giu) + 1
    for _ in range(so_xet):
        rai(tron, r, xet(r) * m_xet * r.uniform(0.5, 1.0), r.uniform(0.0, giu))
    return hoan_tat(tron, r, r.uniform(0.5, 0.8), r.uniform(0.35, 0.5), -3)


def trung(hat, kieu):
    _, m_chiu, xps, m_xet, m_chay, d_chay = CAU_HINH[kieu]
    r = np.random.default_rng(hat)
    giay = r.uniform(*d_chay)
    n = int(SR * (giay + 0.8)); tron = np.zeros((n, 2))
    rai(tron, r, chiu_st(r, r.uniform(-0.4, 0.4), giay=r.uniform(0.04, 0.1), f_tren=r.uniform(1800, 2700)) * m_chiu * 0.8, 0.0)
    for _ in range(int(r.uniform(*xps) * 0.25) + 1):
        rai(tron, r, xet(r) * m_xet * r.uniform(0.5, 1.0), r.uniform(0.0, 0.2))
    tron += chay(n, r, 0.01, giay, m_chay)
    return hoan_tat(tron, r, r.uniform(0.4, 0.7), r.uniform(0.3, 0.45), -4)


def lan(hat, kieu):
    _, m_chiu, xps, m_xet, m_chay, _ = CAU_HINH[kieu]
    r = np.random.default_rng(hat)
    n = int(SR * 0.7); tron = np.zeros((n, 2))
    rai(tron, r, chiu_st(r, r.uniform(-0.5, 0.5), giay=r.uniform(0.05, 0.12)) * max(m_chiu, 0.6), 0.0)
    for _ in range(int(r.integers(1, 4))):
        rai(tron, r, xet(r) * m_xet * r.uniform(0.4, 0.9), r.uniform(0.0, 0.12))
    if r.random() < 0.5:
        tron += chay(n, r, 0.03, r.uniform(0.15, 0.3), m_chay * 0.35)
    return hoan_tat(tron, r, r.uniform(0.3, 0.5), r.uniform(0.3, 0.4), -5)


def cau(hat, kieu):
    """Vong lap Qua cau dien 3 s: XET XET lien tuc rai hai ben + CHIU nho thinh thoang + vo vo tram hai nguon + xi min; noi tron vong."""
    so_chiu, m_chiu, xps, m_xet, _, _ = CAU_HINH[kieu]
    r = np.random.default_rng(hat)
    n = int(SR * GIAY_VONG); t = np.arange(n) / SR
    tron = np.zeros((n, 2))
    def rai_vong(x2, bd, lech=None):
        if x2.ndim == 1: x2 = dat(x2, r.uniform(-0.9, 0.9) if lech is None else lech)
        idx = (int(bd * SR) + np.arange(len(x2))) % n
        tron[idx] += x2
    for _ in range(int(r.uniform(*xps) * GIAY_VONG * 1.3)):
        rai_vong(xet(r) * m_xet * r.uniform(0.4, 1.0), r.uniform(0, GIAY_VONG))
    for _ in range(int(GIAY_VONG * (3 if kieu == 'laser_dien' else 1.5))):
        rai_vong(chiu_st(r, giay=r.uniform(0.04, 0.1), f_tren=r.uniform(1700, 2600)) * m_chiu * r.uniform(0.3, 0.6), r.uniform(0, GIAY_VONG))
    bung = np.maximum(cham(n, 1.0, r) * 0.3 + 0.9, 0.35)
    for f0, lech in ((141.0, -0.7), (156.0, 0.7)):        # vo vo tram hai nguon (chu ky nguyen trong vong)
        pha = PI2 * f0 * t + r.uniform(0, PI2)
        than = loc(sum(np.sin(h * pha + 0.3 * h) / h for h in range(1, 7)), thap(700, 2)) * bung
        tron += dat(than / (np.max(np.abs(than)) + 1e-12), lech) * 0.25
    xi = loc2(on2(n, r, 0.15), lambda ff: cao(1400, 2)(ff) * thap(3400, 4)(ff)) * (np.maximum(cham(n, 3, r) * 0.4 + 0.8, 0.1))[:, None]
    tron += xi / (np.max(np.abs(xi)) + 1e-12) * 0.25
    m = int(0.8 * SR); tm = np.arange(m) / SR
    for k in range(2):
        h = loc(r.standard_normal(m), lambda f: cao(200, 2)(f) * thap(4000, 2)(f)) * np.exp(-tm / 0.15)
        h[:int(0.012 * SR)] = 0
        h /= np.sqrt(np.sum(h ** 2)) + 1e-12
        hh = np.zeros(n); hh[:m] = h
        tron[:, k] += np.fft.irfft(np.fft.rfft(tron[:, k]) * np.fft.rfft(hh), n) * 0.6
    return q3.chuan(tron, -5, vong=True)


MAU = {'A_laser_dien': ('laser_dien', 15100), 'B_xet_xet': ('xet_xet', 15300), 'C_set_dot_chay': ('set_dot_chay', 15500)}


def cau_nhu_phong(hat):
    """Vong lap Qua cau dien BAN 2 (nguoi dung chon mau A nhung: "tieng dien trong Qua cau dien cung giong nhu trong 3,5 giay dau tien cua A" -
    tuc chuoi PHONG / TRUNG / LAN cua Giut set): CHIU CHIU (chiu stereo nhan doi) noi nhau 0,1-0,35 s rai hai ben + XET XET + nen CHAY xeo nhe;
    bo vo vo tram hai nguon cua ban 1. Su kien dat theo modulo, vang tron vong - noi lien mach."""
    so_chiu, m_chiu, xps, m_xet, m_chay, _ = CAU_HINH['laser_dien']
    r = np.random.default_rng(hat)
    n = int(SR * GIAY_VONG); t = np.arange(n) / SR
    tron = np.zeros((n, 2))
    def rai_vong(x2, bd):
        if x2.ndim == 1: x2 = dat(x2, r.uniform(-0.9, 0.9))
        idx = (int(bd * SR) + np.arange(len(x2))) % n
        tron[idx] += x2
    tt = 0.0
    while tt < GIAY_VONG:                       # chuoi chiu nhu luc phong: cum 2-4 cai roi nghi
        for j in range(int(r.integers(2, 5))):
            rai_vong(chiu_st(r) * m_chiu * r.uniform(0.55, 1.0), tt % GIAY_VONG)
            tt += r.uniform(0.05, 0.14)
        tt += r.uniform(0.1, 0.35)
    for _ in range(int(r.uniform(*xps) * GIAY_VONG * 1.2)):
        rai_vong(xet(r) * m_xet * r.uniform(0.5, 1.0), r.uniform(0, GIAY_VONG))
    # nen chay xeo nhe (tron vong: sinh dai gap doi roi gap nua sau vao nua dau)
    c = chay(2 * n, r, 0.0, 4 * GIAY_VONG, 1.0)
    c = c[:n] * np.linspace(0, 1, n)[:, None] + c[n:] * np.linspace(1, 0, n)[:, None]
    tron += c / (np.max(np.abs(c)) + 1e-12) * 0.18
    m = int(0.8 * SR); tm = np.arange(m) / SR
    for k in range(2):
        h = loc(r.standard_normal(m), lambda f: cao(200, 2)(f) * thap(4000, 2)(f)) * np.exp(-tm / 0.15)
        h[:int(0.012 * SR)] = 0
        h /= np.sqrt(np.sum(h ** 2)) + 1e-12
        hh = np.zeros(n); hh[:m] = h
        tron[:, k] += np.fft.irfft(np.fft.rfft(tron[:, k]) * np.fft.rfft(hh), n) * 0.6
    return q3.chuan(tron, -5, vong=True)


if __name__ == '__main__' and '--cau2' in sys.argv:
    ra = os.path.join('CongCu', 'AmThanh', 'GiatSetThu4')
    kieu, hat0 = MAU['A_laser_dien']
    bo = {'phong': [phong(hat0 + 10 * i, kieu) for i in range(3)], 'trung': [trung(hat0 + 100 + 10 * i, kieu) for i in range(3)],
          'lan': [lan(hat0 + 200 + 10 * i, kieu) for i in range(3)]}
    vc = cau_nhu_phong(15700)
    tb.ghi(os.path.join(ra, 'A2_cau.wav'), vc); th.do('A2_cau', vc)
    dd = np.abs(np.diff(vc[:, 0]))
    m = vc.mean(axis=1); e = np.sqrt(np.convolve(m ** 2, np.ones(4410) / 4410, 'same'))[4410:-4410]
    print('   cau: noi %.4f (buoc quanh %.4f / %.4f), bao bien thien %.2f; game can bang %.2f' % (abs(vc[0, 0] - vc[-1, 0]), dd[-1], dd[0],
          (e.max() - e.min()) / e.max(), g3.tq(g3.can_bang(vc, 25))))
    g = g3.ghep_game(bo['phong'], bo['trung'], bo['lan'], vc, phat=g3.can_bang)
    tb.ghi(os.path.join(ra, 'A2_ghep_nhu_trong_game.wav'), g); th.do('A2_ghep_game', g)
    sys.exit(0)

if __name__ == '__main__':
    ra = sys.argv[1] if len(sys.argv) > 1 else os.path.join('CongCu', 'AmThanh', 'GiatSetThu4')
    os.makedirs(ra, exist_ok=True)
    chi = sys.argv[2:]
    for ten, (kieu, hat0) in MAU.items():
        if chi and not any(c in ten for c in chi): continue
        bo = {'phong': [phong(hat0 + 10 * i, kieu) for i in range(3)], 'trung': [trung(hat0 + 100 + 10 * i, kieu) for i in range(3)],
              'lan': [lan(hat0 + 200 + 10 * i, kieu) for i in range(3)]}
        vc = cau(hat0 + 300, kieu)
        for loai, ds in bo.items():
            for i, x in enumerate(ds):
                tb.ghi(os.path.join(ra, '%s_%s%d.wav' % (ten, loai, i + 1)), x)
                th.do('%s_%s%d' % (ten, loai, i + 1), x)
            print('   %s: chenh bien the %s dB; 30ms/300ms %s; game (can bang, giua / +40 do) %s' % (loai,
                  ' / '.join('%.1f' % c for c in th.chenh_bien_the(ds)),
                  ' / '.join('%.2f' % (np.sum(np.abs(x).max(axis=1)[:1323] ** 2) / np.sum(np.abs(x).max(axis=1)[:13230] ** 2)) for x in ds),
                  ' / '.join('%.2f-%.2f' % (g3.tq(g3.can_bang(x, 0)), g3.tq(g3.can_bang(x, 40))) for x in ds)))
        tb.ghi(os.path.join(ra, '%s_cau.wav' % ten), vc)
        th.do('%s_cau' % ten, vc)
        dd = np.abs(np.diff(vc[:, 0]))
        m = vc.mean(axis=1); e = np.sqrt(np.convolve(m ** 2, np.ones(4410) / 4410, 'same'))[4410:-4410]
        print('   cau: noi %.4f (buoc quanh %.4f / %.4f), bao bien thien %.2f' % (abs(vc[0, 0] - vc[-1, 0]), dd[-1], dd[0], (e.max() - e.min()) / e.max()))
        g = g3.ghep_game(bo['phong'], bo['trung'], bo['lan'], vc, phat=g3.can_bang)
        tb.ghi(os.path.join(ra, '%s_ghep_nhu_trong_game.wav' % ten), g)
        th.do('%s_ghep_game' % ten, g)
