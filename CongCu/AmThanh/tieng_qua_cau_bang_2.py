# -*- coding: utf-8 -*-
# TIENG QUA CAU BANG + MUA BANG - BAN 2 (16/10/2026). Nguoi dung nghe ban 1 (tieng_qua_cau_bang.py, mau A Pha le / B Bang da):
# "giong y chang tieng THUY TINH be chu khong phai tieng bang, lam lai".
# Vi sao ban 1 ra thuy tinh: manh bang = 5 phan am KHONG dieu hoa NGAN 20-250 ms (cung, ngan dai, co cao do ro = dac trung thuy tinh),
# them lop "ngan tinh the" (sin cao keo dai) va manh leng keng sang.
# Bang that khac thuy tinh (dung lam co so ban 2):
#   - VA CHAM: DUC, TAT RAT NHANH (3-30 ms), cong huong ROng (Q 1,5-3) - tieng "coc / cach" cua da vien, khong "keng";
#   - VO: "rac rac" LAO XAO (vun bang = hang tram hat va cham nho day dac trong 50-400 ms, nhu giam len lop bang tuyet);
#   - NUT: tieng "PIU" TRUOT CAO DO XUONG (song tan sac khi vet nut chay qua tam bang - dac trung mat ho bang nut) + "cach" kho;
#   - KEN KET truoc khi vo; khi lanh "xi" + tinh the dong bang lach tach MIN khi bang moc len.
# Do "giong thuy tinh": do PHANG PHO dai 1-4 kHz phan duoi tieng (sau cu dap) - thuy tinh = dinh nhon (phang thap), bang = lao xao (phang cao).
# Giu quy trinh memory `quy-trinh-tieng-ky-nang-moi`: moi lop rieng tung kenh + phan chung, lech trai/phai chi bang am luong cho am co
# cao do, 3 bien the khac cau truc, < 200 Hz <= 25%, > 4 kHz <= ~10%.
#
# Hai MAU:
#   A "bang_tang"  - tang bang day nhu mat ho dong: nut "piu" tram vang, vo thanh tang "coc" duc + vun, ken ket ro.
#   B "bang_tuyet" - lop bang tuyet gion: lao xao "rac rac" day dac, nut kho ngan, khi lanh xi nhieu.
#
# Chay: python CongCu/AmThanh/tieng_qua_cau_bang_2.py [thu_muc_ra]   (mac dinh CongCu/AmThanh/QuaCauBangThu2)

import os, sys
import numpy as np
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import tieng_bam as tb
import tieng_qua_cau_lua_3 as q3
import tieng_thien_thach_2 as t2
import tieng_thien_thach_3 as t3

SR = tb.SR
PI2 = tb.PI2
GIAY_BAY = 3.0
GIAY_ROI_MUA = 0.96
loc, dai, thap, cao, cham = q3.loc, q3.dai, q3.thap, q3.cao, q3.cham


def pan_am_luong(x, lech):
    g = (np.clip(lech, -1, 1) + 1) * np.pi / 4
    return np.stack([x * np.cos(g), x * np.sin(g)], axis=1) * np.sqrt(2)


# ------------------------------------------------------------------ cac thanh phan BANG THAT
def cuc_bang(d, r, f0, tat):
    """Mot cuc bang va cham: xung on 0,5-2 ms kich hai cong huong ROng (Q 1,5-3) - DUC, tat nhanh (tat 3-30 ms), khong ngan."""
    t = np.arange(d) / SR
    z = r.standard_normal(d) * np.exp(-t / r.uniform(0.0005, 0.002))
    q1, q2 = r.uniform(1.5, 3.0), r.uniform(1.5, 3.0)
    x = loc(z, lambda f: dai(f0, q1)(f) + 0.6 * dai(f0 * r.uniform(1.8, 2.4), q2)(f) + 0.25 * thap(f0 * 3, 2)(f))
    x *= np.exp(-t / tat)
    x += np.sin(PI2 * f0 * 0.5 * t) * np.exp(-t / (tat * 0.5)) * 0.4          # "coc" tram ngan
    return x / (np.max(np.abs(x)) + 1e-12)


def cuc_roi(tron, r, so, bd0, bd1, f_min, f_max, tat=(0.004, 0.025), nang=1.0, nay=(1, 3), rong=1.0):
    """Cuc bang roi xuong dat, NAY 1-2 lan ("coc... coc"), moi cuc mot cho (lech chi bang am luong)."""
    dai_tron = len(tron) / SR
    for _ in range(so):
        bd = bd0 + (bd1 - bd0) * r.random() ** 1.5
        f0 = np.exp(r.uniform(np.log(f_min), np.log(f_max)))
        lech = np.clip(r.normal(0, 0.6) * rong, -1, 1)
        a = np.exp(r.uniform(np.log(0.2), 0)) * nang * np.exp(-bd * 1.0)
        khoang = r.uniform(0.06, 0.15)
        for lan in range(r.integers(nay[0], nay[1])):
            d = int(SR * 0.08)
            x = cuc_bang(d, r, f0 * r.uniform(0.95, 1.05), r.uniform(*tat))
            tb.cong(tron, pan_am_luong(x * a, lech), bd)
            bd += khoang; khoang *= r.uniform(0.45, 0.7); a *= r.uniform(0.3, 0.5)
            if bd >= dai_tron - 0.1: break


def vun_bang(t, r, bd, giay, mat_do=900, f=(700, 2600), nang=1.0, chung=0.4):
    """VUN BANG "rac rac" lao xao: hang tram hat va cham nho DAY DAC (cum ngau nhien), moi hat loc rong quanh f ngau nhien; hai kenh
    phan lon rieng + ~40% chung. Bao: vao nhanh, tat theo giay, mat do giam dan."""
    n = len(t)
    e = np.clip((t - bd) / 0.003, 0, 1) * np.exp(-np.maximum(t - bd, 0) / giay)
    cum = np.maximum(cham(n, 25, r) * 0.6 + 0.6, 0) ** 2                 # vo thanh tung CUM (khong deu)
    def mot():
        p = e * cum * mat_do / SR
        vt = np.nonzero(r.random(n) < p)[0]
        x = np.zeros(n)
        x[vt] = np.exp(r.uniform(np.log(0.08), 0, len(vt))) * np.sign(r.uniform(-1, 1, len(vt)))
        # moi hat la cuc bang ti hon: loc dai rong (tong vai dai Q thap) - tat ~1-3 ms tu nhien cua bo loc
        return loc(x, lambda ff: dai(f[0], 1.2)(ff) + dai(np.sqrt(f[0] * f[1]), 1.4)(ff) + 0.8 * dai(f[1], 1.6)(ff))
    c = mot()
    ra = np.stack([chung * c + (1 - chung) * mot(), chung * c + (1 - chung) * mot()], axis=1)
    return ra / (np.max(np.abs(ra)) + 1e-12) * nang


def nut_piu(t, r, bd, f_tren=3200, f_duoi=260, giay=0.12, nang=1.0):
    """NUT BANG "piu": song tan sac - cao do truot xuong nhanh f_tren -> f_duoi (theo ham mu) trong 'giay', bat dau bang "cach" kho;
    hai kenh CUNG luc bat dau, toc truot lech 2% (rong dan theo thoi gian). ⚠️ Ban dau lech kenh 0,2 ms: song CO CAO DO 2-3 kHz
    lech pha > 180 do -> tuong quan am (-0,14) - loa dien thoai gop mono se triet tieu (bay nhu manh bang ban 1)."""
    n = len(t)
    ra = np.zeros((n, 2))
    zc = r.standard_normal(n)
    tt = np.maximum(t - bd, 0)
    for k in range(2):
        f = f_duoi + (f_tren - f_duoi) * np.exp(-tt / (giay * 0.28 * (1 + 0.02 * k)))
        pha = PI2 * np.cumsum(f) / SR
        s = np.sin(pha) * np.exp(-tt / giay) * (t >= bd)
        cach = loc(0.55 * zc + 0.8 * r.standard_normal(n), lambda ff: cao(800, 2)(ff) * thap(5000, 2)(ff)) * np.exp(-tt / 0.0015) * (t >= bd)
        ra[:, k] = s * 0.7 + cach * 0.9
    return ra * nang


def ken_ket(t, r, bd, giay, f_tam=420, nhip=(25, 110), nang=1.0):
    """KEN KET (bang chiu luc): chuoi xung thua nhip tang dan qua dai rong 250-700 Hz (Q 2) - "keeet" duc, khong ngan."""
    n = len(t)
    ra = np.zeros((n, 2))
    for k in range(2):
        x = np.zeros(n)
        tt = bd + r.uniform(0, 0.01)
        while tt < bd + giay:
            i = int(tt * SR)
            if i >= n: break
            x[i] = r.uniform(0.4, 1.0)
            tien = (tt - bd) / giay
            tt += 1.0 / (nhip[0] + (nhip[1] - nhip[0]) * tien) * r.uniform(0.6, 1.4)
        y = loc(x, lambda f: dai(f_tam * (1 + 0.05 * (k - 0.5)), 2.0)(f) + 0.5 * dai(f_tam * 2.2, 2.0)(f))
        ra[:, k] = y * np.clip((t - bd) / (giay * 0.6), 0, 1)
    return ra / (np.max(np.abs(ra)) + 1e-12) * nang


def um_chung(t, r, f0, f1, giay_tat, lech=0.015):
    """"Um" than (nhu q3.um) nhung hai kenh CUNG PHA luc dau, chi lech cao do 1,5% (phach cham). q3.um / t3.bum cho moi kenh mot pha
    ngau nhien -> am tram 200-400 Hz hai kenh co the nguoc pha (do: tuong quan -0,05 / -0,14 o tieng no / cham dat)."""
    ra = np.zeros((len(t), 2))
    pha0 = r.uniform(0, PI2)
    for k in range(2):
        f = (f1 + (f0 - f1) * np.exp(-t / 0.05)) * (1 + lech * (k - 0.5))
        pha = PI2 * np.cumsum(f) / SR + pha0
        x = np.sin(pha) + 0.5 * np.sin(2 * pha + 0.3) + 0.25 * np.sin(3 * pha + 1.1)
        ra[:, k] = np.tanh(2.5 * x * np.exp(-t / giay_tat)) * np.minimum(1, t / 0.002)
    return ra


def bum_chung(t, r, f0, f1, giay, nang=1.0):
    """'Bum' va cham (nhu t3.bum) - hai kenh cung pha + on tram 60% chung."""
    n = len(t)
    ra = np.zeros((n, 2))
    pha0 = r.uniform(0, PI2)
    zc = r.standard_normal(n)
    for k in range(2):
        f = (f1 + (f0 - f1) * np.exp(-t / 0.035)) * (1 + 0.015 * (k - 0.5))
        pha = PI2 * np.cumsum(f) / SR + pha0
        on = loc(0.6 * zc + 0.6 * r.standard_normal(n), lambda ff: cao(60, 2)(ff) * thap(500, 2)(ff))
        x = np.sin(pha) + 0.5 * np.sin(2 * pha + 0.2) + 0.6 * on * 4
        e = np.exp(-t / giay) * (1 + 2.5 * np.exp(-t / 0.05))
        ra[:, k] = np.tanh(3.0 * x * e * nang) * np.minimum(1, t / 0.0015)
    return ra


def gio_lanh(n, r, f0=600, f1=2600, hz=1.2):
    g = q3.gio_xe(n, r, f0, f1, hz)
    return 0.85 * g + 0.42 * g.mean(axis=1, keepdims=True)


def xi_lanh(n, r, bao, f0=900, f1=3500):
    """Khi lanh "xi" (hoi lanh phut ra): on dai 0,9-3,5 kHz, hai kenh rieng + chung."""
    zc = r.standard_normal(n)
    ra = np.stack([loc(0.5 * zc + 0.85 * r.standard_normal(n), lambda f: cao(f0, 2)(f) * thap(f1, 2)(f)) for _ in range(2)], axis=1)
    return ra / (np.max(np.abs(ra)) + 1e-12) * bao[:, None]


def dong_bang(n, r, mat_do, bao_mat=None, vong=False):
    """Tinh the bang MOC LEN lach tach rat min (dai 1,2-3,5 kHz, khong ngan)."""
    return q3.lach_tach(n, r, mat_do, vong=vong, bao_mat=bao_mat, nang_bop=0.0, cao_min=1200, cao_max=3500) * 0.5


# ================================================================== BAN
def tieng_ban(hat, kieu):
    r = np.random.default_rng(hat)
    tang = kieu == "A"
    n = int(SR * 1.0); t = np.arange(n) / SR
    tron = np.zeros((n, 2))
    tron += xi_lanh(n, r, np.minimum(1, t / 0.006) * np.exp(-t / r.uniform(0.08, 0.16)), 700, 3000) * 0.9      # phut khi lanh
    tron += um_chung(t, r, r.uniform(240, 320), r.uniform(150, 200), r.uniform(0.03, 0.06)) * r.uniform(0.3, 0.5)
    dinh = r.uniform(0.05, 0.12)
    bg = np.minimum(1, t / dinh) * np.exp(-np.maximum(t - dinh, 0) / r.uniform(0.12, 0.25))
    tron += t2.pan_quet(gio_lanh(n, r, 700, 2600, 3.0) * bg[:, None] * r.uniform(0.4, 0.65), np.linspace(0, r.choice([-0.6, 0.6]), n))
    # bang KET LAI thanh qua: vun nho + nut ngan "cach" (A: them piu nho)
    tron += vun_bang(t, r, r.uniform(0.0, 0.03), r.uniform(0.06, 0.12), r.uniform(400, 800), (900, 2600), nang=0.55 if tang else 0.8)
    if tang:
        tron += nut_piu(t, r, r.uniform(0.02, 0.06), r.uniform(2400, 3000), r.uniform(400, 600), r.uniform(0.05, 0.08), nang=0.35)
    tron += dong_bang(n, r, r.uniform(120, 250), bao_mat=np.exp(-t / 0.3))
    tron = q3.vang_uot(tron, 0.3, r, 0.25)
    return q3.chuan(tron, -4)


# ================================================================== BAY (vong lap)
def tieng_bay(hat, kieu):
    r = np.random.default_rng(hat)
    tang = kieu == "A"
    n = int(SR * GIAY_BAY)
    x = gio_lanh(n, r, 450, 2200, 0.9) * 1.0
    x += q3.lua(n, hat + 1, liem=0.9, xi=0.0, tach=0.0, mat_do=1, vong=True, tam_liem=22, dai_den=700, chung=0.35) * 0.8   # khi lanh cuon
    x += dong_bang(n, r, 40 if tang else 70, vong=True) * 1.2
    # thinh thoang "rac" nho (bang tren qua nut / vun bay ra) - dat vong (quan ve dau)
    t = np.arange(n) / SR
    so = int(GIAY_BAY * (2.0 if tang else 3.5))
    for _ in range(so):
        bd = r.uniform(0, GIAY_BAY)
        tt = (t - bd) % GIAY_BAY
        v = vun_bang(tt, r, 0.0, r.uniform(0.02, 0.05), r.uniform(500, 900), (900, 2400), nang=r.uniform(0.15, 0.35))
        x += v
    bung = np.maximum(cham(n, 0.9, r) * 0.45 + 0.75, 0.15) ** 1.5
    x *= bung[:, None]
    return q3.chuan(x, -7, vong=True)


# ================================================================== NO (qua cau bang trung)
def tieng_no(hat, kieu):
    r = np.random.default_rng(hat)
    tang = kieu == "A"
    to = r.uniform(0.75, 1.3)
    n = int(SR * 2.4); t = np.arange(n) / SR
    tron = np.zeros((n, 2))
    tron += q3.nut(t, r) * r.uniform(0.9, 1.3)
    tron += bum_chung(t, r, r.uniform(170, 230), r.uniform(80, 100), 0.08 * to) * 0.55
    tron += nut_piu(t, r, 0.0, r.uniform(2600, 3400), r.uniform(220, 380), r.uniform(0.08, 0.16) * to, nang=0.8 if tang else 0.45)
    tron += vun_bang(t, r, 0.002, r.uniform(0.12, 0.22) * to, r.uniform(900, 1500), (700, 2400), nang=1.0 if tang else 1.4)
    cuc_roi(tron, r, int(r.integers(8, 16) * to), 0.03, 0.7, 350, 1400, nang=0.8, nay=(1, 3))
    tron += xi_lanh(n, r, np.minimum(1, t / 0.01) * np.exp(-t / r.uniform(0.15, 0.3)), 800, 3200) * 0.45
    for k, p in ((0, -0.9), (1, 0.9)):
        b = np.minimum(1, t / 0.01) * np.exp(-t / r.uniform(0.15, 0.3))
        tron += tb.dat_vi_tri(q3.gio_xe(n, r, 2600, 500, 4.0)[:, k] * b, p) * 0.25
    # bang MOC len tren dat: dong bang lach tach + ken ket (A ro)
    tron += dong_bang(n, r, r.uniform(250, 450), bao_mat=np.clip((t - 0.06) / 0.1, 0, 1) * np.exp(-np.maximum(t - 0.16, 0) / 0.5)) * 1.2
    tron += ken_ket(t, r, r.uniform(0.12, 0.25), r.uniform(0.25, 0.45), r.uniform(320, 520)) * (0.4 if tang else 0.2)
    if r.random() < 0.4:
        bd = r.uniform(0.1, 0.35)
        phu = nut_piu(t, r, bd, r.uniform(2200, 3000), r.uniform(300, 500), 0.07, nang=0.4) + vun_bang(t, r, bd, 0.05, 600, nang=0.4)
        p2 = r.uniform(-0.85, 0.85); phu[:, 0] *= np.sqrt(1 - p2); phu[:, 1] *= np.sqrt(1 + p2)
        tron += phu
    tron = q3.vang_uot(tron, 0.6, r, 0.3)
    return q3.chuan(tron, -2)


# ================================================================== KHOI BANG VO
def tieng_vo(hat, kieu, manh=1.0):
    r = np.random.default_rng(hat)
    tang = kieu == "A"
    n = int(SR * (1.8 if manh <= 1 else 2.4)); t = np.arange(n) / SR
    tron = np.zeros((n, 2))
    tk = r.uniform(0.1, 0.25) if manh <= 1 else 0.0
    if tk > 0:
        tron += ken_ket(t, r, 0.0, tk, r.uniform(300, 520)) * (0.6 if tang else 0.4)
    tron += nut_piu(t, r, tk, r.uniform(2400, 3400), r.uniform(200, 360), r.uniform(0.08, 0.16), nang=(0.9 if tang else 0.55) * manh)
    if manh > 1:
        tron += q3.nut(t, r) * 1.2 + bum_chung(t, r, r.uniform(160, 220), 80, 0.12) * 0.8
        tron += xi_lanh(n, r, np.minimum(1, t / 0.01) * np.exp(-t / 0.3), 800, 3200) * 0.45
    tron += vun_bang(t, r, tk + 0.003, r.uniform(0.15, 0.3) * manh, r.uniform(1000, 1800), (600, 2200), nang=1.0 if tang else 1.3)
    cuc_roi(tron, r, int(r.integers(4, 8) * manh), tk + 0.01, tk + 0.15, 250, 600, (0.01, 0.035), nang=1.0, nay=(2, 4), rong=0.6)   # tang lon
    cuc_roi(tron, r, int(r.integers(8, 16) * manh), tk + 0.02, tk + 0.9, 600, 1600, nang=0.6, nay=(1, 3))                      # cuc nho
    tron = q3.vang_uot(tron, 0.5, r, 0.25)
    return q3.chuan(tron, -2 if manh > 1 else -3)


# ================================================================== MUA BANG
def tieng_mua_roi(hat, kieu):
    r = np.random.default_rng(hat)
    n = int(SR * 1.2); t = np.arange(n) / SR
    x = np.clip(t / GIAY_ROI_MUA, 0, 1)
    tron = t3.gam_lao_toi(t, r, GIAY_ROI_MUA, f_xa=500, f_gan=r.uniform(2400, 3000)) * 0.8
    tron += xi_lanh(n, r, x ** 2.5, 1000, 3000) * 0.3
    tron += dong_bang(n, r, 70, bao_mat=x ** 2) * 0.8
    tron = t2.pan_quet(tron, r.uniform(-0.5, 0.5) * (1 - x))
    return q3.chuan(tron, -4)


def tieng_mua_cham(hat, kieu):
    r = np.random.default_rng(hat)
    tang = kieu == "A"
    n = int(SR * 1.0); t = np.arange(n) / SR
    tron = np.zeros((n, 2))
    tron += um_chung(t, r, r.uniform(240, 320), 150, 0.04) * 0.6
    tron += nut_piu(t, r, 0.0, r.uniform(2200, 3000), r.uniform(300, 500), r.uniform(0.04, 0.07), nang=0.5 if tang else 0.3)
    tron += vun_bang(t, r, 0.002, r.uniform(0.06, 0.12), r.uniform(700, 1300), (800, 2400), nang=1.0)
    cuc_roi(tron, r, int(r.integers(3, 7)), 0.01, 0.35, 500, 1500, nang=0.6, nay=(1, 3))
    tron += dong_bang(n, r, 150, bao_mat=np.exp(-t / 0.25))
    tron = q3.vang_uot(tron, 0.35, r, 0.2)
    return q3.chuan(tron, -4)


if __name__ == '__main__':
    import tieng_qua_cau_bang as v1
    ra = sys.argv[1] if len(sys.argv) > 1 else os.path.join('CongCu', 'AmThanh', 'QuaCauBangThu2')
    os.makedirs(ra, exist_ok=True)
    for kieu, ten in (("A", "A_bang_tang"), ("B", "B_bang_tuyet")):
        lech = 0 if kieu == "A" else 500
        bo = {
            'ban': [tieng_ban(4100 + 10 * i + lech, kieu) for i in range(3)],
            'no': [tieng_no(4200 + 10 * i + lech, kieu) for i in range(3)],
            'vo': [tieng_vo(4300 + 10 * i + lech, kieu) for i in range(3)],
            'vono': [tieng_vo(4400 + 10 * i + lech, kieu, 1.6) for i in range(3)],
            'muaroi': [tieng_mua_roi(4500 + 10 * i + lech, kieu) for i in range(3)],
            'muacham': [tieng_mua_cham(4600 + 10 * i + lech, kieu) for i in range(3)],
        }
        bay = tieng_bay(4700 + lech, kieu)
        noi = np.max(np.abs(bay[0] - bay[-1])); dd = np.max(np.abs(np.diff(bay, axis=0)), axis=1)
        print('   vong lap bay %s: noi %.4f, xep hang %.0f%% cac buoc' % (ten, noi, 100 * np.mean(dd < noi)))
        for loai, ds in bo.items():
            for i, x in enumerate(ds):
                tb.ghi(os.path.join(ra, '%s_%s%d.wav' % (ten, loai, i + 1)), x)
        tb.ghi(os.path.join(ra, '%s_bay.wav' % ten), bay)
        for tg, g in (('ghep_qua_cau', v1.ghep_qua_cau(bo['ban'], bay, bo['no'], bo['vo'])),
                      ('ghep_mua_bang', v1.ghep_mua_bang(bo['muaroi'], bo['muacham'], bo['vo'], bo['vono']))):
            g = g / np.max(np.abs(g)) * 0.95
            tb.ghi(os.path.join(ra, '%s_%s.wav' % (ten, tg)), g)
