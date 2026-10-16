# -*- coding: utf-8 -*-
# TIENG KHIEN - BAN 2 (16/10/2026). Ban 1 (tieng_khieng.py: 5-8 sin sach cong lai) nguoi dung che: "con GIA TAO va DON GIAN qua".
# Vi sao gia: sin sach + bao mu deu = tieng dien tu. Kim loai THAT: hang chuc-hang tram MODE cong huong khong dieu hoa, mat do day dan len cao, moi mode
# tat mot kieu, duoc KICH bang mot cu va cham co ON (khong phai xung sach), cong / chuong lon con CHUNG cao do nhe luc dau ("bwoong" - phi tuyen), va
# luon co KHONG GIAN (phan xa som + vang). Lop phu (tieng don dap vao, nang luong chap chon, manh vo...) lam nen do "giau".
#
# Cach tong hop: "dap ung xung" IR = tong 40-90 mode (sin tat dan, cao do troi xuong nhe) cho MOI KENH (mode chung tan so, bien do moi kenh khac nhau =
# vi tri tren mat khien; ~30% mode lech tan so rieng tung kenh) -> TICH CHAP voi cu kich (on loc ngan 2-12 ms, cung / mem tuy don) bang FFT.
#
# Quy trinh tieng ky nang (memory): khong tuong quan am (mode chung cung pha), tuong quan clip 0,1-0,35; < 200 Hz <= 25%, cat < 120 Hz, > 4 kHz <= 10%;
# 3 bien the KHAC CAU TRUC; khong "piu" (cao do chi chung <= 1,5% trong 0,3 s), khong "thuy tinh" (mode > 2 kHz tat < 0,25 s).
#
# Ba MAU (deu "boong boong" + doi lai):
#   A "thep_duc"      - khien THEP DUC day: boong tram day, nhieu mode, va cham chac nich, doi lai trong khong gian nghia dia.
#   B "vom_phep_thep" - vom PHEP bang nang luong co loi kim loai: boong + song nang luong chap chon (hat tia li ti, u u dap theo nhip) bao quanh.
#   C "cong_dong"     - CONG DONG lon: "bwoong" phinh nhe, ngan dai, phach cham, doi lai nhieu lan.
#
# Chay: python CongCu/AmThanh/tieng_khieng_2.py [thu_muc_ra] [loc ten mau...]   (mac dinh CongCu/AmThanh/KhiengThu2)

import os, sys
import numpy as np
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import tieng_bam as tb
import tieng_qua_cau_lua_3 as q3
import tieng_qua_cau_bang_2 as b2
import tieng_qua_cau_bang_3 as b3
import tieng_tang_hinh as th
import tieng_giat_set_3 as g3
import tieng_sam_set as ss
import tieng_nhao_lon as nl
import tieng_toc_bien as tbn

SR = tb.SR
PI2 = tb.PI2
loc, dai, thap, cao, cham = q3.loc, q3.dai, q3.thap, q3.cao, q3.cham
on2, loc2, quet2, keo_lech, bao_len_xuong = th.on2, th.loc2, th.quet2, th.keo_lech, th.bao_len_xuong


# ------------------------------------------------------------------ khoi kim loai: bo mode
VAT = {
    #            f0 (Hz)     so mode   tat goc (s)   do troi mode   chung cao do   do sang (mode cao)   mat do (mu)
    'thep_duc':      dict(f0=(210, 260), so=(55, 80), tat=(0.9, 1.4), troi=1.9, chung=(0.004, 0.008), sang=0.55, mu=1.35),
    'vom_phep_thep': dict(f0=(230, 290), so=(40, 60), tat=(0.8, 1.2), troi=1.7, chung=(0.003, 0.007), sang=0.5, mu=1.25),
    'cong_dong':     dict(f0=(190, 235), so=(45, 70), tat=(1.4, 2.0), troi=2.1, chung=(0.008, 0.015), sang=0.45, mu=1.45),
}


def bo_mode(r, vat, f0, so):
    """Danh sach mode (tan so, bien do, thoi gian tat): mat do DAY dan len cao (khoang cach co dang tang theo mu), tan so ngau nhien khong dieu
    hoa; mode cao tat nhanh (~ f^-0,9) va yeu dan; cat > 4,2 kHz."""
    c = VAT[vat]
    ds = []
    # vai mode chinh thap (than "boong") quanh f0, 1,5-1,7 f0, 2,0-2,3 f0 - dat truoc cho co ban sac, roi rai phan con lai
    chinh = [1.0, r.uniform(1.48, 1.72), r.uniform(1.98, 2.35), r.uniform(2.6, 3.0)]
    for k, rr in enumerate(chinh):
        ds.append((f0 * rr, (1.0, 0.75, 0.6, 0.45)[k], 1.0 / rr ** 0.5))
    for i in range(so):
        x = (i + 1) / so
        rr = 1.0 + 15.0 * x ** c['mu'] * r.uniform(0.85, 1.15)
        f = f0 * rr
        if f > 4200: continue
        # (do lan 1: 1-4 kHz chi 3-10% - thieu 'keng' sang cua kim loai that) mode 1-2 kHz to hon, ngan lau hon; > 2 kHz van tat nhanh (khong thuy tinh)
        a = r.uniform(0.15, 0.6) * (1.0 / rr ** 0.35) * (c['sang'] * 3.0 if f > 1000 else 1.0)
        tk = r.uniform(0.6, 1.4) / rr ** 0.75
        if f > 2000: tk = min(tk, 0.22)
        ds.append((f, a, tk))
    return ds


def ir_kim_loai(r, vat, f0, giay, so=None, tat_goc=None):
    """IR hai kenh: moi mode CHUNG tan so + pha (tuong quan duong), bien do moi kenh khac nhau (vi tri tren mat khien); ~30% mode lech tan so rieng moi
    kenh 0,2-0,6% (khong trung) - phach o TUNG kenh. Cao do CHUNG xuong nhe luc dau (phi tuyen kim loai / cong: 'bwoong')."""
    c = VAT[vat]
    so = so or int(r.integers(*c['so']))
    tat_goc = tat_goc or r.uniform(*c['tat'])
    n = int(SR * giay); t = np.arange(n) / SR
    chung = r.uniform(*c['chung'])
    troi = 1 + chung * np.exp(-t / 0.18)                 # cao do tu (1+chung) ve 1 trong ~0,3 s
    ir = np.zeros((n, 2))
    for j, (f, a, h) in enumerate(bo_mode(r, vat, f0, so)):
        tk = tat_goc * h
        e = np.exp(-t / tk)
        # (do lan 1: mode CHINH (than boong) roi vao nhom lech rieng -> clip tuong quan AM -0,10) chi mode > 600 Hz moi duoc lech rieng
        rieng = j >= 4 and f > 600 and r.random() < 0.5
        ph0 = r.uniform(0, PI2)
        # moi mode MOT CHO tren mat khien (lech bang am luong, equal power); mode chinh gan giua (do lan 3: 0,35-1,0 ngau nhien -> tuong quan 0,5-0,7)
        p_ = r.uniform(-0.55, 0.55) if j < 4 else r.uniform(-0.95, 0.95)
        gL, gR = np.cos((p_ + 1) * np.pi / 4), np.sin((p_ + 1) * np.pi / 4)
        for k, g in ((0, gL), (1, gR)):
            fk = f * (1 + (r.uniform(-0.006, 0.006) if rieng else 0.0))
            ph = PI2 * np.cumsum(fk * troi) / SR + (ph0 if not rieng else r.uniform(0, PI2))
            ir[:, k] += a * g * e * np.sin(ph)
    return ir / (np.max(np.abs(ir)) + 1e-12)


def cu_kich(r, giay=0.006, f=(300, 5000), cung=1.0):
    """Cu KICH (vat danh vao khien): on loc dai tat trong 'giay' (cung = ngan + sang, mem = dai + toi) - hai kenh 60% chung."""
    d = int(SR * max(giay * 4, 0.02)); tt = np.arange(d) / SR
    # (do lan 2: kich hai kenh 60% chung -> CUNG mot mode o hai kenh lech pha ngau nhien -> clip tuong quan am -0,16; nay MOT cu kich chung, stereo
    # do bien do mode tung kenh + mode lech rieng)
    z = np.repeat(r.standard_normal(d)[:, None], 2, axis=1)
    e = np.exp(-tt / giay) * np.minimum(1, tt / 0.0004)
    x = loc2(z, lambda ff: cao(f[0], 2)(ff) * thap(f[1] * (0.6 + 0.4 * cung), 2)(ff)) * e[:, None]
    return x / (np.max(np.abs(x)) + 1e-12)


def tich_chap(x, ir):
    """Tich chap tung kenh bang FFT (x hai kenh voi ir hai kenh)."""
    n = len(x) + len(ir) - 1
    nn = 1 << int(np.ceil(np.log2(n)))
    y = np.stack([np.fft.irfft(np.fft.rfft(x[:, k], nn) * np.fft.rfft(ir[:, k], nn), nn)[:n] for k in range(2)], axis=1)
    return y / (np.max(np.abs(y)) + 1e-12)


def khong_gian(x, r, giay_vang=1.3, muc_vang=0.3, doi=((0.13, 0.22), (0.27, 0.42)), muc_doi=(0.42, 0.22), lech=0.7):
    """KHONG GIAN nghia dia: tieng DOI lai (phan xa tu bia mo / nha mo xa: tre 0,13-0,42 s, toi hon, lech ve hai ben xen ke) + vang uot stereo."""
    n = len(x)
    them = int(SR * (doi[-1][1] + 0.1)) if doi else 0
    y = np.pad(x, ((0, them), (0, 0)))
    ben = 1 if r.random() < 0.5 else -1
    for j, (kh, m) in enumerate(zip(doi, muc_doi)):
        i = int(r.uniform(*kh) * SR)
        z = np.stack([loc(x[:, k], lambda ff: cao(180, 2)(ff) * thap(2400 - 600 * j, 2)(ff)) for k in range(2)], axis=1)
        y[i:i + n] += keo_lech(z, ben * lech * (1 if j % 2 == 0 else -1)) * m * r.uniform(0.8, 1.2)
    return q3.vang_uot(y, giay_vang, r, muc_vang)


def nang_luong(n, r, bd, giay, mat_do=(600, 200), nang=1.0):
    """Vom NANG LUONG chap chon: hat tia li ti 1,5-3,8 kHz (moi hat mot cho, khong ngan) + u u 180-700 Hz dap nhip 9-14 Hz (khong re) - tat dan."""
    t = np.arange(n) / SR
    bao = bao_len_xuong(t, 0.003, giay, bd)
    md = mat_do[1] + (mat_do[0] - mat_do[1]) * bao
    hat = th.hat_bui(n, r, md * (t >= bd), (1500, 3800)) * 0.8
    u = loc2(on2(n, r, 0.5), lambda ff: cao(180, 2)(ff) * thap(700, 2)(ff))
    nhip = np.stack([0.55 + 0.45 * np.sin(PI2 * r.uniform(9, 14) * t + r.uniform(0, PI2)) for _ in range(2)], axis=1)
    u = u * nhip
    u = u / (np.max(np.abs(u)) + 1e-12)
    x = hat / (np.max(np.abs(hat)) + 1e-12) * 0.7 + u * 0.6
    return x * bao[:, None] * nang


# ------------------------------------------------------------------ ba tieng
NANG_LUONG = {'thep_duc': 0.0, 'vom_phep_thep': 0.45, 'cong_dong': 0.0}


def trung(hat, vat):
    """DON TRUNG KHIEN: don dap vao (thich + nut) -> kim loai "BOONG" (kich vao bo mode) -> (vom phep) nang luong chap chon -> doi lai + vang."""
    c = VAT[vat]; r = np.random.default_rng(hat)
    f0 = r.uniform(*c['f0'])
    giay = r.uniform(*c['tat']) * 1.8 + 0.4
    n = int(SR * giay)
    # cau truc khac nhau: don CUNG (kiem, mong vuot) / don NANG (chuy, than to) / don NO (cau lua) - doi cu kich + lop phu
    kieu = r.choice(['cung', 'nang', 'no'])
    gk = {'cung': 0.003, 'nang': 0.009, 'no': 0.014}[kieu]
    kich = cu_kich(r, gk, (250, 5200), {'cung': 1.0, 'nang': 0.6, 'no': 0.4}[kieu])
    ir = ir_kim_loai(r, vat, f0, giay)
    tron = tich_chap(kich, ir)[:n]
    t = np.arange(n) / SR
    # don dap vao (khong qua kim loai): "toc"/"thich" + nut chat
    tron += b3.toc(t, r, 0.0, r.uniform(0.35, 0.6), (300, 2400) if kieu != 'cung' else (900, 3600), r.uniform(0.006, 0.016))
    tron += b2.um_chung(t, r, f0 * r.uniform(1.1, 1.4), f0 * 0.9, r.uniform(0.03, 0.06), lech=0.003) * {'cung': 0.12, 'nang': 0.3, 'no': 0.35}[kieu]
    if kieu == 'no':
        tron += tbn.gio_bat(n, r, 0.0, 0.006, r.uniform(0.08, 0.16), 400, 2600, 0.35) * 0.35
    if NANG_LUONG[vat] > 0:
        tron += nang_luong(n, r, 0.005, r.uniform(0.25, 0.5), nang=NANG_LUONG[vat])
    so_doi = int(r.integers(1, 3))
    tron = khong_gian(tron, r, r.uniform(1.0, 1.5), r.uniform(0.22, 0.32), ((0.12, 0.2), (0.26, 0.4))[:so_doi], (0.45, 0.22)[:so_doi])
    tron = ss.tam(tron, 0.05)
    return q3.chuan(tron, -3)


def bat(hat, vat):
    """BAT KHIEN: kim loai NGAN LEN nhu chuong xoay (bo mode bi KEO bang on lien tuc to dan - "uooong") + luong khi cuon quanh than, khep vom bang mot
    "boong" nhe, nang luong u u lang xuong."""
    c = VAT[vat]; r = np.random.default_rng(hat)
    f0 = r.uniform(*c['f0']) * r.uniform(0.9, 1.0)
    len_ = r.uniform(0.35, 0.6)
    n = int(SR * (len_ + 1.8)); t = np.arange(n) / SR
    ir = ir_kim_loai(r, vat, f0, 1.6, so=int(r.integers(30, 50)), tat_goc=r.uniform(0.9, 1.3))
    # keo: on hong (loc thong thap) bien do to dan toi len_ roi tat nhanh -> mode ngan len tu tu (khong cao do truot)
    keo = loc2(on2(n, r, 0.4), lambda ff: cao(200, 2)(ff) * thap(2500, 2)(ff)) * ((np.clip(t / len_, 0, 1) ** 2.5) * np.exp(-np.maximum(t - len_, 0) / 0.03))[:, None]
    ngan = tich_chap(keo, ir)[:n]
    tron = ngan * 0.8
    tron += tbn.gio_bat(n, r, 0.0, len_, 0, 300, r.uniform(1700, 2600), 0.35, nguoc=True) * r.uniform(0.3, 0.5)
    # khep vom: "boong" nhe (kich mem vao bo mode)
    k2 = cu_kich(r, r.uniform(0.008, 0.014), (250, 3500), 0.5)
    b = tich_chap(k2, ir_kim_loai(r, vat, f0 * r.uniform(1.0, 1.05), 1.4))
    i = int(len_ * SR); m = min(len(b), n - i)
    tron[i:i + m] += b[:m] * r.uniform(0.55, 0.8)
    tron += nang_luong(n, r, len_, r.uniform(0.4, 0.8), (300, 80), 0.3 + NANG_LUONG[vat])
    tron = khong_gian(tron, r, r.uniform(1.0, 1.4), 0.25, ((0.15, 0.25),), (0.3,))
    tron = ss.tam(tron, 0.05)
    return q3.chuan(tron, -4)


def vo(hat, vat):
    """KHIEN NO TUNG: cu dap cuoi -> kim loai KEU CANG (ken ket) -> NUT TOAC + "BOONG" meo (bao hoa) -> xa nang luong + bum tram + khi bung -> MANH VO
    (moi manh mot bo mode NHO, tat nhanh) vang ra roi lach cach nay tren dat -> doi lai + vang dai."""
    c = VAT[vat]; r = np.random.default_rng(hat)
    f0 = r.uniform(*c['f0']) * r.uniform(0.85, 0.95)
    n = int(SR * 2.6); t = np.arange(n) / SR
    cang = r.uniform(0.05, 0.14)                    # khoang keu cang truoc khi vo
    tron = np.zeros((n, 2))
    tron += b2.ken_ket(t, r, 0.0, cang, r.uniform(380, 650), (40, 140), 0.35)
    k = cu_kich(r, 0.012, (200, 5200), 0.8)
    b = tich_chap(k, ir_kim_loai(r, vat, f0, 2.2, tat_goc=r.uniform(*c['tat']) * 1.2))
    i = int(cang * SR); m = min(len(b), n - i)
    tron[i:i + m] += np.tanh(2.0 * b[:m]) * 0.85
    tron += tbn.xe_rach(n, r, cang, r.uniform(0.04, 0.08), r.uniform(1500, 2500), r.uniform(3000, 4500), 0.6)
    tron += tbn.bup(n, r, cang, r.uniform(250, 300), r.uniform(165, 185), r.uniform(0.09, 0.13), 1.0, 1.1)
    tron += tbn.gio_bat(n, r, cang + 0.01, 0.01, r.uniform(0.25, 0.45), 350, r.uniform(2400, 3200)) * 0.6
    tron += nang_luong(n, r, cang, r.uniform(0.3, 0.6), (1400, 200), 0.35 + NANG_LUONG[vat])
    # MANH VO: moi manh mot bo mode nho (f cao hon, tat 40-150 ms), vang ra luc vo, roi cham dat nay 1-2 lan
    for _ in range(int(r.integers(6, 12))):
        fm = r.uniform(700, 2200)
        ir_m = ir_kim_loai(r, vat, fm, 0.25, so=12, tat_goc=r.uniform(0.05, 0.15))
        km = cu_kich(r, 0.002, (800, 5000), 1.0)
        bm = tich_chap(km, ir_m)
        g = r.uniform(-0.9, 0.9)
        for lan, (bd, a) in enumerate(((cang + r.uniform(0.0, 0.05), r.uniform(0.15, 0.35)),
                                        (cang + r.uniform(0.35, 0.8), r.uniform(0.1, 0.25)),
                                        (cang + r.uniform(0.9, 1.3), r.uniform(0.04, 0.1)))):
            if lan == 2 and r.random() < 0.5: continue
            i = int(bd * SR); m = min(len(bm), n - i)
            if m > 0: tron[i:i + m] += keo_lech(bm[:m], g) * a
    tron = khong_gian(tron, r, r.uniform(1.4, 1.8), 0.3, ((0.14, 0.22), (0.3, 0.45)), (0.4, 0.2))
    tron = ss.tam(tron, 0.05)
    return q3.chuan(tron, -2)


MAU = {'A_thep_duc': ('thep_duc', 25100), 'B_vom_phep_thep': ('vom_phep_thep', 25300), 'C_cong_dong': ('cong_dong', 25500)}


def ghep_game(bats, trungs, vos, phat=g3.can_bang):
    """Nhu ban 1: minh bat khien -> 6 don trung (cach 0,25-0,6 s) -> vo; nguoi khac ben trai (-35 do) bat, an 3 don, vo."""
    out = np.zeros((int(SR * 13.0), 2))
    r = np.random.default_rng(7)
    tb.cong(out, phat(bats[0], 0), 0.3)
    tt = 1.5
    for k in range(6):
        tb.cong(out, phat(trungs[k % 3], 0) * r.uniform(0.75, 1.0), tt)
        tt += r.uniform(0.3, 0.7)
    tb.cong(out, phat(vos[0], 0), tt + 0.3)
    b0 = tt + 3.4
    tb.cong(out, phat(bats[1], -35) * 0.7, b0)
    tt = b0 + 1.3
    for k in range(3):
        tb.cong(out, phat(trungs[(k + 1) % 3], -35) * 0.7, tt); tt += r.uniform(0.35, 0.7)
    tb.cong(out, phat(vos[1], -35) * 0.7, tt + 0.2)
    return out / np.max(np.abs(out)) * 0.95


if __name__ == '__main__':
    ra = sys.argv[1] if len(sys.argv) > 1 else os.path.join('CongCu', 'AmThanh', 'KhiengThu2')
    os.makedirs(ra, exist_ok=True)
    chi = sys.argv[2:]
    for ten, (vat, hat0) in MAU.items():
        if chi and not any(c in ten for c in chi): continue
        bo = {'bat': [bat(hat0 + 10 * i, vat) for i in range(3)], 'trung': [trung(hat0 + 100 + 10 * i, vat) for i in range(3)],
              'vo': [vo(hat0 + 200 + 10 * i, vat) for i in range(3)]}
        for loai, ds in bo.items():
            for i, x in enumerate(ds):
                tb.ghi(os.path.join(ra, '%s_%s%d.wav' % (ten, loai, i + 1)), x)
                th.do('%s_%s%d' % (ten, loai, i + 1), x)
            print('   chenh bien the %s: %s dB; can bang giua tq %s' % (loai, ' / '.join('%.1f' % c for c in th.chenh_bien_the(ds)),
                  ' / '.join('%.2f' % g3.tq(x) for x in ds)))
        g = ghep_game(bo['bat'], bo['trung'], bo['vo'])
        tb.ghi(os.path.join(ra, '%s_ghep_nhu_trong_game.wav' % ten), g)
        th.do('%s_ghep' % ten, g)
