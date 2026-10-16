# -*- coding: utf-8 -*-
# TIENG TOC BIEN (16/10/2026, nguoi dung): "am thanh khi nhan vat BIEN MAT va am thanh khi nhan vat XUAT HIEN o cho moi". Nguoi choi khac
# trong suong mu thi khong nghe (TiengViTri lo - moi tieng theo vi tri cua no: bien mat o cho cu, hien ra o cho moi).
# Hai tieng ngan (~0,6-1,2 s), phat CUNG LUC (Toc bien khong niem, dich chuyen tuc thoi) o hai cho cach nhau toi 15 m.
#
# Lam theo memory quy-trinh-tieng-ky-nang-moi: moi lop rieng tung kenh + ~30-45% chung; su kien nho moi cai mot cho, lech CHI bang am luong; am
# tram hai kenh CUNG PHA (b2.bum_chung / um_chung); KHONG "piu" truot cao do (= laser), KHONG ngan nhieu phan am (= thuy tinh), KHONG dung lop
# dac trung cua ky nang khac: luong khi xoay pha "vuuu" (flanger) = Tang hinh "Hu khong", gio hu = Gio loc / Loc xoay, liem lua = Qua cau lua.
#
# Ba MAU:
#   A "bong_toi" - than nguoi VUT di: ao choang "phach" + luong gio bat ra + hoi tho toi "hu"; hien ra = gio HUT VAO (to dan) + "phup" + chan cham
#                  dat + ao ru.
#   B "xe_khong_gian" - khong gian bi XE RACH (chuoi nut xe day dan) roi SUP "up"; hien ra = xe ngan + "BUP" ap suat bung ra + dat bui vang.
#   C "khoi_no"  - "BUP" khoi nen bat ra (nhu phao khoi) + khoi XI tan dan; hien ra = "bup" khoi + bui dat + ao.
#
# Chay: python CongCu/AmThanh/tieng_toc_bien.py [thu_muc_ra] [loc ten mau...]   (mac dinh CongCu/AmThanh/TocBienThu)

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

SR = tb.SR
loc, dai, thap, cao, cham = q3.loc, q3.dai, q3.thap, q3.cao, q3.cham
on2, loc2, quet2, keo_lech, bao_len_xuong = th.on2, th.loc2, th.quet2, th.keo_lech, th.bao_len_xuong


def gio_bat(n, r, bd, len_, tat, f0, f1, chung=0.35, nguoc=False):
    """Luong khi bat ra (nguoc=False: len nhanh, tat dan, thong thap MO roi DONG) hoac HUT VAO (nguoc=True: to dan, sang dan, cat dot ngot)."""
    t = np.arange(n) / SR
    if not nguoc:
        bao = bao_len_xuong(t, len_, tat, bd)
    else:
        bao = (np.clip((t - bd) / len_, 0, 1) ** 2.5) * (t < bd + len_) + np.exp(-np.maximum(t - bd - len_, 0) / 0.012) * (t >= bd + len_)
        bao *= (t >= bd)
    k = bao / (np.max(bao) + 1e-12)
    x = quet2(on2(n, r, chung), f0 + (f1 - f0) * k + 1, kieu='thap', lech=0.06)
    x = x / (np.max(np.abs(x)) + 1e-12)
    return x * bao[:, None]


def xi_khoi(n, r, bd, giay, f=(500, 2600), chung=0.35):
    """Khoi XI tan dan: on dai rong, bao tat dai, bien do dap nhe cham (khoi cuon) rieng tung kenh."""
    t = np.arange(n) / SR
    x = loc2(on2(n, r, chung), lambda ff: cao(f[0], 2)(ff) * thap(f[1], 2)(ff))
    m = np.stack([np.maximum(1 + 0.5 * cham(n, 6, r), 0.1) for _ in range(2)], axis=1)
    x = x * m / (np.max(np.abs(x * m)) + 1e-12)
    fc = f[1] * (0.35 + 0.65 * np.exp(-np.maximum(t - bd, 0) / (giay * 0.6)))
    x = quet2(x, fc, kieu='thap')
    return x / (np.max(np.abs(x)) + 1e-12) * bao_len_xuong(t, 0.02, giay, bd)[:, None]


def xe_rach(n, r, bd, giay, mat_dau, mat_cuoi, nang=1.0, f=(900, 3200)):
    """XE RACH: chuoi NUT ngan (0,5-2 ms) mat do tang dan mat_dau -> mat_cuoi /s, moi nut qua 2-3 cong huong rong (Q 2-3, KHONG ngan), moi nut
    mot cho (lech chi bang am luong) - nhu vai / khong khi bi xe toac."""
    t = np.arange(n) / SR
    tt = (t - bd) / giay
    md = np.where((tt >= 0) & (tt <= 1), mat_dau + (mat_cuoi - mat_dau) * np.clip(tt, 0, 1) ** 2, 0)
    vt = np.nonzero(r.random(n) < md / SR)[0]
    nhan = []
    for _ in range(8):
        d = int(r.uniform(0.0005, 0.002) * SR)
        z = np.pad(r.standard_normal(d), (0, 400))
        fc = np.exp(r.uniform(np.log(f[0]), np.log(f[1]), 2))
        k = loc(z, lambda ff: dai(fc[0], r.uniform(2, 3))(ff) + 0.7 * dai(fc[1], r.uniform(2, 3))(ff) + 0.2 * cao(600, 2)(ff))
        nhan.append(k / (np.max(np.abs(k)) + 1e-12))
    ra = np.zeros((n, 2))
    for i in vt:
        k = nhan[r.integers(0, 8)]
        a = np.exp(r.uniform(np.log(0.2), 0)) * (0.4 + 0.6 * np.clip((i / SR - bd) / giay, 0, 1))
        g = (r.uniform(-0.9, 0.9) + 1) * np.pi / 4
        m = min(len(k), n - i)
        ra[i:i + m, 0] += k[:m] * a * np.cos(g)
        ra[i:i + m, 1] += k[:m] * a * np.sin(g)
    return ra / (np.max(np.abs(ra)) + 1e-12) * nang


def bup(n, r, bd, f0, f1, giay, nang=1.0, khi=1.0):
    """"BUP" ap suat: than bum_chung (cung pha) + LUONG KHI bat ra cuc ngan (thong thap mo 3 kHz trong 8 ms roi dong) - phan chinh o 200 Hz-2 kHz."""
    t = np.arange(n) / SR
    tt = np.maximum(t - bd, 0)
    # (do lan 1: b2.bum_chung lech cao do hai kenh 1,5% -> than dai 0,1 s troi pha thanh NGUOC (tuong quan -0,06..-0,21); nay lech 0,3%)
    x = b2.um_chung(tt, r, f0, f1, giay, lech=0.003) * (t >= bd)[:, None] * 0.25 * nang
    x += gio_bat(n, r, bd, 0.004, r.uniform(0.04, 0.08), 350, r.uniform(2400, 3400), 0.2) * khi * nang
    x += b3.toc(t, r, bd, 0.5 * nang, (300, 1400), 0.015)
    return x


# ================================================================== MAU A "BONG TOI"
def A_mat(hat):
    r = np.random.default_rng(hat)
    n = int(SR * 1.0); t = np.arange(n) / SR
    tron = np.zeros((n, 2))
    # ao choang PHACH (1-3 nhip) + than vut di
    # (do lan 1: chenh bien the 1,5-1,9 dB - giong nhau; nay gieo ca do manh ao / gio, so nhip, them / bot luong gio thu hai)
    so = int(r.integers(1, 4)); bd = 0.0; a_ao = r.uniform(0.5, 1.3)
    for j in range(so):
        tron += nl.ao(n, r, bao_len_xuong(t, 0.004, r.uniform(0.04, 0.12), bd), (800, 3600), 0.3, 55.0, 0.9) * a_ao * (1.0 if j == 0 else r.uniform(0.4, 0.8))
        bd += r.uniform(0.03, 0.1)
    tron += gio_bat(n, r, 0.01, r.uniform(0.03, 0.06), r.uniform(0.08, 0.3), 380, r.uniform(1800, 3000)) * r.uniform(0.5, 1.2)
    if r.random() < 0.5:
        tron += gio_bat(n, r, r.uniform(0.08, 0.16), 0.03, r.uniform(0.1, 0.2), 320, r.uniform(1400, 2200)) * r.uniform(0.4, 0.7)
    # hoi tho toi "hu" (khong cao do) - co / khong
    if r.random() < 0.7:
        bt = bao_len_xuong(t, 0.03, r.uniform(0.12, 0.25), r.uniform(0.0, 0.04))
        tron += th.thi_tham(n, r, [th.NGUYEN_AM['h'], th.NGUYEN_AM['u']], bt, 0.3) * r.uniform(0.35, 0.6)
    # bui dat bi da len o cho dung + khoi tan
    tron += nl.soi(n, r, 0.01, r.uniform(0.05, 0.12), r.uniform(500, 1200), r.uniform(0.25, 0.45))
    tron += xi_khoi(n, r, 0.05, r.uniform(0.25, 0.5), (500, 2200)) * r.uniform(0.15, 0.3)
    tron = ss.tam(tron, 0.05)
    return th.hoan_tat(tron, r, r.uniform(0.6, 0.9), r.uniform(0.18, 0.28), -4)


def A_hien(hat):
    r = np.random.default_rng(hat)
    hut = r.uniform(0.1, 0.2)
    n = int(SR * (hut + 0.9)); t = np.arange(n) / SR
    tron = gio_bat(n, r, 0.0, hut, 0, 300, r.uniform(2000, 2800), 0.35, nguoc=True) * r.uniform(0.7, 1.0)
    tron += bup(n, r, hut, r.uniform(280, 320), r.uniform(175, 195), r.uniform(0.05, 0.08), r.uniform(0.7, 1.0), 0.6)
    # chan cham dat (mot / hai chan)
    for j in range(int(r.integers(1, 3))):
        bd = hut + 0.01 + j * r.uniform(0.06, 0.12)
        b = b3.toc(t, r, bd, 0.5, (450, 1800), 0.012) + nl.soi(n, r, bd, 0.05, 900, 0.5)
        tron += keo_lech(b, r.uniform(-0.3, 0.3))
    tron += nl.ao(n, r, bao_len_xuong(t, 0.03, r.uniform(0.08, 0.15), hut + r.uniform(0.03, 0.1)), (700, 3200), 0.3, 18.0) * r.uniform(0.4, 0.7)
    tron += xi_khoi(n, r, hut, r.uniform(0.2, 0.45), (500, 2200)) * r.uniform(0.12, 0.25)
    tron = ss.tam(tron, 0.05)
    return th.hoan_tat(tron, r, r.uniform(0.6, 0.9), r.uniform(0.18, 0.28), -4)


# ================================================================== MAU B "XE KHONG GIAN"
def B_mat(hat):
    r = np.random.default_rng(hat)
    xe = r.uniform(0.1, 0.24)
    n = int(SR * (xe + 1.0)); t = np.arange(n) / SR
    tron = xe_rach(n, r, 0.0, xe, r.uniform(150, 400), r.uniform(1500, 2800), 1.0)
    tron += gio_bat(n, r, 0.0, xe, 0, 400, 1800, 0.35, nguoc=True) * r.uniform(0.3, 0.5)          # khi bi hut vao vet rach
    tt = np.maximum(t - xe, 0)
    tron += b2.um_chung(tt, r, r.uniform(280, 330), r.uniform(170, 190), r.uniform(0.05, 0.09), lech=0.003) * (t >= xe)[:, None] * r.uniform(0.35, 0.55)
    tron += b3.toc(t, r, xe, 0.5, (300, 1200), 0.02)
    # du am "rung" khong gian: on 250-700 Hz tat dan, dap nhe - co / khong, dai ngan
    if r.random() < 0.75:
        x = loc2(on2(n, r, 0.4), lambda ff: cao(250, 2)(ff) * thap(800, 2)(ff))
        x *= np.stack([np.maximum(1 + 0.6 * cham(n, 9, r), 0) for _ in range(2)], axis=1)
        tron += x / (np.max(np.abs(x)) + 1e-12) * bao_len_xuong(t, 0.02, r.uniform(0.15, 0.4), xe)[:, None] * r.uniform(0.25, 0.45)
    tron = ss.tam(tron, 0.05)
    return th.hoan_tat(tron, r, r.uniform(0.7, 1.0), r.uniform(0.2, 0.3), -4)


def B_hien(hat):
    r = np.random.default_rng(hat)
    xe = r.uniform(0.03, 0.08)
    n = int(SR * (xe + 1.0)); t = np.arange(n) / SR
    tron = xe_rach(n, r, 0.0, xe, r.uniform(800, 1500), r.uniform(2500, 4000), r.uniform(0.6, 0.9))
    tron += bup(n, r, xe, r.uniform(300, 350), r.uniform(180, 200), r.uniform(0.06, 0.1), 1.0, 1.0)
    # dat bui bi ap suat thoi vang ra (rai rac) + vien soi nay
    tron += nl.soi(n, r, xe + 0.01, r.uniform(0.08, 0.2), r.uniform(800, 1800), r.uniform(0.35, 0.6))
    nl.vien_soi(tron, r, int(r.integers(0, 4)), xe + 0.02, xe + 0.2, 0.35)
    tron += xi_khoi(n, r, xe, r.uniform(0.15, 0.35), (500, 2400)) * r.uniform(0.15, 0.3)
    tron = ss.tam(tron, 0.05)
    return th.hoan_tat(tron, r, r.uniform(0.7, 1.0), r.uniform(0.2, 0.3), -4)


# ================================================================== MAU C "KHOI NO"
def C_mat(hat):
    r = np.random.default_rng(hat)
    n = int(SR * 1.3); t = np.arange(n) / SR
    bd = r.uniform(0.0, 0.03)
    tron = bup(n, r, bd, r.uniform(260, 300), r.uniform(170, 185), r.uniform(0.06, 0.1), 1.0, r.uniform(1.0, 1.4))
    if r.random() < 0.5:     # "bup" doi (hai nhip sat nhau)
        tron += bup(n, r, bd + r.uniform(0.03, 0.06), r.uniform(300, 340), r.uniform(185, 200), 0.04, 0.5, 0.8)
    tron += xi_khoi(n, r, bd + 0.01, r.uniform(0.35, 0.7), (600, r.uniform(2400, 3200))) * r.uniform(0.45, 0.7)
    tron += nl.ao(n, r, bao_len_xuong(t, 0.004, r.uniform(0.05, 0.1), bd), (800, 3600), 0.3, 50.0, 0.9) * r.uniform(0.3, 0.6)
    tron = ss.tam(tron, 0.05)
    return th.hoan_tat(tron, r, r.uniform(0.6, 0.9), r.uniform(0.18, 0.28), -4)


def C_hien(hat):
    r = np.random.default_rng(hat)
    n = int(SR * 1.3); t = np.arange(n) / SR
    bd = r.uniform(0.0, 0.03)
    tron = bup(n, r, bd, r.uniform(290, 330), r.uniform(180, 195), r.uniform(0.05, 0.09), 1.0, r.uniform(1.0, 1.3))
    tron += xi_khoi(n, r, bd + 0.01, r.uniform(0.3, 0.6), (600, r.uniform(2200, 3000))) * r.uniform(0.4, 0.65)
    tron += nl.soi(n, r, bd + 0.01, r.uniform(0.06, 0.15), r.uniform(600, 1400), r.uniform(0.3, 0.5))
    if r.random() < 0.6:
        bd2 = bd + r.uniform(0.04, 0.1)
        tron += keo_lech(b3.toc(t, r, bd2, 0.45, (450, 1800), 0.012) + nl.soi(n, r, bd2, 0.05, 900, 0.4), r.uniform(-0.3, 0.3))
    tron += nl.ao(n, r, bao_len_xuong(t, 0.03, r.uniform(0.08, 0.15), bd + r.uniform(0.05, 0.12)), (700, 3200), 0.3, 18.0) * r.uniform(0.3, 0.55)
    tron = ss.tam(tron, 0.05)
    return th.hoan_tat(tron, r, r.uniform(0.6, 0.9), r.uniform(0.18, 0.28), -4)


MAU = {'A_bong_toi': (A_mat, A_hien, 23100), 'B_xe_khong_gian': (B_mat, B_hien, 23300), 'C_khoi_no': (C_mat, C_hien, 23500)}


def ghep_game(mats, hiens, phat=g3.can_bang):
    """MO PHONG TRONG GAME: 1) minh nhay (bien mat + hien ra deu ~giua man, may quay theo minh); 2) nguoi khac nhay tu -40 sang +20 do;
    3) minh nhay; 4) nguoi khac nhay tu +35 sang -10 do. Hai tieng cung luc (toc bien tuc thoi)."""
    out = np.zeros((int(SR * 6.5), 2))
    lich = [(0.3, -5, 5, 1.0), (1.8, -40, 20, 0.7), (3.3, 5, -5, 1.0), (4.8, 35, -10, 0.7)]
    for k, (bd, a0, a1, a) in enumerate(lich):
        tb.cong(out, phat(mats[k % 3], a0) * a, bd)
        tb.cong(out, phat(hiens[(k + 1) % 3], a1) * a, bd + 0.02)
    return out / np.max(np.abs(out)) * 0.95


if __name__ == '__main__':
    ra = sys.argv[1] if len(sys.argv) > 1 else os.path.join('CongCu', 'AmThanh', 'TocBienThu')
    os.makedirs(ra, exist_ok=True)
    chi = sys.argv[2:]
    for ten, (fm, fh, hat0) in MAU.items():
        if chi and not any(c in ten for c in chi): continue
        bo = {'bien_mat': [fm(hat0 + 10 * i) for i in range(3)], 'hien_ra': [fh(hat0 + 100 + 10 * i) for i in range(3)]}
        for loai, ds in bo.items():
            for i, x in enumerate(ds):
                tb.ghi(os.path.join(ra, '%s_%s%d.wav' % (ten, loai, i + 1)), x)
                th.do('%s_%s%d' % (ten, loai, i + 1), x)
            print('   chenh bien the %s: %s dB; 30ms/300ms %s' % (loai, ' / '.join('%.1f' % c for c in th.chenh_bien_the(ds)),
                  ' / '.join('%.2f' % (lambda m: np.sum(m[:1323] ** 2) / np.sum(m[:13230] ** 2))(np.abs(x).max(axis=1)) for x in ds)))
        g = ghep_game(bo['bien_mat'], bo['hien_ra'])
        tb.ghi(os.path.join(ra, '%s_ghep_nhu_trong_game.wav' % ten), g)
        th.do('%s_ghep' % ten, g)
