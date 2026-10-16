# -*- coding: utf-8 -*-
# TIENG KHIEN (16/10/2026, nguoi dung): "am thanh luc BAT khien, am thanh khi khien NO TUNG, va khi cac don danh cua ke thu DANH TRUNG khien se nghe
# tieng DOI ra BOONG BOONG". Nguoi choi khac trong suong mu thi khong nghe (TiengViTri lo).
# Ba tieng: bat (~1,2 s) · trung (moi don trung khien ~1,2-1,8 s, "boong" + tieng doi lai) · vo (~2,2 s).
#
# Lam theo memory quy-trinh-tieng-ky-nang-moi:
#   - "boong" la am CO CAO DO ngan dai -> bay "thuy tinh" (nhieu phan am khong dieu hoa ngan dai o dai cao): o day phan am chinh 200-900 Hz, phan am
#     cao tat NHANH (tat ~ 1/ti le^1,2), khong phan am nao ngan > 0,4 s tren 2 kHz; KHONG truot cao do (laser).
#   - stereo: moi PHAN AM dat mot cho (lech CHI bang am luong, hai kenh cung pha - khong tuong quan am), tieng DOI lai lech ve phia ben kia, vang chi
#     cong phan uot, mot xung moi kenh.
#   - 3 bien the KHAC CAU TRUC (cao do goc, so phan am, co / khong tieng doi thu hai, do dai ngan) + cao do +-6% trong game; dai dien thoai.
#
# Ba MAU:
#   A "chuong_dong"  - khien nhu CHUONG DONG / CONG: "toc" danh + "boong" ngan am, phach "oong" cham, tieng doi lai hai lan.
#   B "vom_nang_luong" - vom NANG LUONG: "boong" tram hon, tron hon + lop u u dien nhe (khong re), "uoong" phinh ra; doi lai mot lan.
#   C "khien_sat"    - KHIEN SAT: "keng" kim loai chat hon + than "boong" ngan hon, hat vun / tia lua nho; doi lai mot lan.
#
# Chay: python CongCu/AmThanh/tieng_khieng.py [thu_muc_ra] [loc ten mau...]   (mac dinh CongCu/AmThanh/KhiengThu)

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


# ------------------------------------------------------------------ "boong" bang tong hop modal
TI_LE = {
    # (ti le tan so, bien do, he so thoi gian tat) - chuong / cong: phan am thap ngan dai, cao tat nhanh
    'chuong_dong': [(0.5, 0.35, 1.3), (1.0, 1.0, 1.0), (1.19, 0.55, 0.8), (1.5, 0.5, 0.65), (2.0, 0.45, 0.5), (2.52, 0.3, 0.35), (3.01, 0.22, 0.25),
                    (4.07, 0.16, 0.15), (5.43, 0.12, 0.1), (6.8, 0.09, 0.07), (8.2, 0.06, 0.05)],
    'vom_nang_luong': [(0.5, 0.45, 1.2), (1.0, 1.0, 1.0), (1.5, 0.3, 0.6), (2.0, 0.5, 0.55), (3.0, 0.25, 0.35), (4.0, 0.16, 0.2), (5.0, 0.12, 0.12),
                       (6.0, 0.09, 0.08), (7.0, 0.06, 0.06)],
    'khien_sat': [(1.0, 1.0, 1.0), (1.58, 0.6, 0.55), (2.14, 0.5, 0.4), (2.76, 0.4, 0.3), (3.47, 0.3, 0.2), (4.6, 0.2, 0.12), (5.9, 0.12, 0.08)],
}


def boong(n, r, bd, f0, kieu, tat, nang=1.0, rong=0.8, phach=(1.0, 3.5), so_phan_am=None, tran_cao=3800.0, sang=1.0, giua=0.65):
    """Mot cu "BOONG": moi phan am = phan GIUA (hai kenh cung pha, lech trai/phai bang am luong) + phan RIENG TRAI (cao do +d) + RIENG PHAI (-d),
    d 0,3-0,8% -> moi kenh tu co phach "oong" cham, hai kenh khong trung (do lan 1: chi phan giua -> tuong quan 0,5-0,87). Phan am < 150 Hz bo
    (dai dien thoai; lan 1: < 200 Hz toi 33-79%), > tran_cao bo; phan am cao tat nhanh; 'sang' nhan bien do phan am cao (do lan 1: 1-4 kHz 0-8%)."""
    t = np.arange(n) / SR
    tt = t - bd
    co = tt >= 0
    ttp = np.maximum(tt, 0)
    ra = np.zeros((n, 2))
    ds = TI_LE[kieu] if so_phan_am is None else TI_LE[kieu][:so_phan_am]
    rieng = np.sqrt(max(0.0, 1 - giua ** 2))
    for (k, a, h) in ds:
        f = f0 * k * r.uniform(0.995, 1.005)
        if f > tran_cao or f < 150: continue
        tk = tat * h
        a = a * (sang if f > 1000 else 1.0)
        e = np.exp(-ttp / tk) * co * np.minimum(1, ttp / 0.002) * a
        ph = r.uniform(0, PI2)
        df = r.uniform(*phach)
        x = (np.sin(PI2 * f * ttp + ph) + 0.6 * np.sin(PI2 * (f + df) * ttp + ph + 1.0)) * e
        g = (np.clip(r.uniform(-rong, rong), -1, 1) + 1) * np.pi / 4
        ra[:, 0] += giua * x * np.cos(g) * np.sqrt(2); ra[:, 1] += giua * x * np.sin(g) * np.sqrt(2)
        d = r.uniform(0.003, 0.008)
        for kk, s_ in ((0, 1), (1, -1)):
            fk = f * (1 + s_ * d)
            y = (np.sin(PI2 * fk * ttp + r.uniform(0, PI2)) + 0.5 * np.sin(PI2 * (fk + r.uniform(*phach)) * ttp + r.uniform(0, PI2))) * e
            ra[:, kk] += rieng * y
    return ra / (np.max(np.abs(ra)) + 1e-12) * nang


def anh_kim(n, r, bd, giay, f=(1400, 3800), nang=1.0):
    """ANH KIM LOAI luc danh: on dai 1,4-3,8 kHz qua 4-6 cong huong rong (Q 4-8, KHONG ngan dai) tat 60-200 ms; hai kenh 30% chung."""
    t = np.arange(n) / SR
    x = on2(n, r, 0.3)
    fcs = np.exp(r.uniform(np.log(f[0]), np.log(f[1]), int(r.integers(4, 7))))
    y = sum(loc2(x, dai(fc, r.uniform(4, 8))) * r.uniform(0.5, 1.0) for fc in fcs)
    y = y / (np.max(np.abs(y)) + 1e-12)
    return y * bao_len_xuong(t, 0.001, giay, bd)[:, None] * nang


def go(n, r, bd, nang=1.0, f=(500, 2600), tat=0.01):
    """Cu GO / danh vao mat khien (on loc dai rat ngan) - "toc"."""
    return b3.toc(np.arange(n) / SR, r, bd, nang, f, tat)


def doi_lai(x, r, so, tre=(0.17, 0.28), giam=(0.35, 0.55), lech=0.6):
    """TIENG DOI lai ("boong ... boong"): ban sao tre 0,17-0,28 s, nho dan, nghieng ve hai ben xen ke, toi hon (loc thong thap)."""
    n = len(x)
    them = int(SR * (tre[1] * so + 0.1))
    ra = np.pad(x, ((0, them), (0, 0)))
    tong_tre = 0.0; a = 1.0
    for j in range(so):
        tong_tre += r.uniform(*tre); a *= r.uniform(*giam)
        i = int(tong_tre * SR)
        y = np.stack([loc(x[:, k], thap(2200 - 500 * j, 2)) for k in range(2)], axis=1)
        y = keo_lech(y, lech if j % 2 == 0 else -lech) * a
        ra[i:i + n] += y
    return ra


def u_dien(n, r, bao, f0=110.0):
    """U U NANG LUONG nhe (vom nang luong): rang cua tram qua dai 200-900 Hz, hai kenh cung pha lech cao do 0,3% - khong re."""
    t = np.arange(n) / SR
    ra = np.zeros((n, 2))
    ph0 = r.uniform(0, PI2)
    for k in range(2):
        f = f0 * (1 + 0.003 * (k - 0.5)) * (1 + 0.004 * cham(n, 3, r))
        ph = PI2 * np.cumsum(f) / SR + ph0
        x = sum(np.sin(h * ph) / h for h in range(1, 10))
        ra[:, k] = loc(x, lambda ff: cao(200, 2)(ff) * thap(900, 2)(ff))
    ra /= np.max(np.abs(ra)) + 1e-12
    return ra * bao[:, None]


# ------------------------------------------------------------------ cau hinh mau
CAU_HINH = {
    'chuong_dong':    dict(f0=(255, 320), tat=(0.55, 0.9), go=0.7, go_f=(800, 3200), doi=2, u=0.0, vun=0.0, sang=2.2, kim=0.45),
    'vom_nang_luong': dict(f0=(235, 285), tat=(0.6, 1.0), go=0.5, go_f=(600, 2600), doi=1, u=0.3, vun=0.0, sang=1.8, kim=0.3),
    'khien_sat':      dict(f0=(280, 360), tat=(0.35, 0.6), go=1.0, go_f=(1000, 3600), doi=1, u=0.0, vun=0.35, sang=2.8, kim=0.6),
}


def trung(hat, kieu):
    """Moi don danh TRUNG khien: "toc" + "BOONG" + tieng doi lai."""
    c = CAU_HINH[kieu]; r = np.random.default_rng(hat)
    tat = r.uniform(*c['tat'])
    n = int(SR * (tat * 2.2 + 0.2)); t = np.arange(n) / SR
    f0 = r.uniform(*c['f0'])
    tron = boong(n, r, 0.004, f0, kieu, tat, 1.0, 0.85, so_phan_am=int(r.integers(5, len(TI_LE[kieu]) + 1)), sang=c['sang'])
    tron += anh_kim(n, r, 0.002, r.uniform(0.06, 0.2), nang=c['kim'] * r.uniform(0.7, 1.2))
    tron += go(n, r, 0.0, c['go'] * r.uniform(0.7, 1.2), c['go_f'], r.uniform(0.006, 0.014))
    # than "thich" cua don dap vao (nhe, cung pha)
    tron += b2.um_chung(t, r, f0 * r.uniform(0.95, 1.05), f0 * 0.75, r.uniform(0.03, 0.06), lech=0.003) * 0.2
    if c['u'] > 0:
        tron += u_dien(n, r, bao_len_xuong(t, 0.01, tat * 0.6, 0.0), f0) * c['u']
    if c['vun'] > 0 and r.random() < 0.7:
        tron += nl.soi(n, r, 0.002, r.uniform(0.04, 0.09), r.uniform(1500, 3000), c['vun'], (1500, 3600))
    so_doi = c['doi'] + (1 if r.random() < 0.35 else 0) - (1 if r.random() < 0.2 else 0)
    tron = doi_lai(tron, r, max(0, so_doi), lech=r.uniform(0.4, 0.8) * (1 if r.random() < 0.5 else -1))
    tron = ss.tam(tron, 0.05)
    return th.hoan_tat(tron, r, r.uniform(0.9, 1.4), r.uniform(0.18, 0.28), -3)


def bat(hat, kieu):
    """BAT khien: vom HINH THANH - luong khi cuon lai + chum phan am "uoong" PHINH dan (bao nguoc) roi khep lai bang mot "boong" nhe."""
    c = CAU_HINH[kieu]; r = np.random.default_rng(hat)
    len_ = r.uniform(0.3, 0.55)
    n = int(SR * (len_ + 1.6)); t = np.arange(n) / SR
    f0 = r.uniform(*c['f0']) * r.uniform(0.85, 0.95)
    # chum phan am phinh ra: boong voi bao NGUOC (len dan theo ham mu), dung lai o len_
    x = boong(n, r, 0.0, f0, kieu, 9.0, 1.0, 0.9, so_phan_am=4)
    bp = (np.clip(t / len_, 0, 1) ** 2.2) * np.exp(-np.maximum(t - len_, 0) / 0.08)
    tron = x * bp[:, None] * 0.7
    # khi cuon lai thanh vom (thong thap mo dan)
    tron += tbn.gio_bat(n, r, 0.0, len_, 0, 300, r.uniform(1600, 2400), 0.35, nguoc=True) * r.uniform(0.35, 0.6)
    # khep vom: "boong" nhe + doi lai
    tron += boong(n, r, len_, f0 * r.uniform(1.0, 1.06), kieu, r.uniform(*c['tat']) * 0.8, r.uniform(0.6, 0.85), 0.8, sang=c['sang'])
    tron += anh_kim(n, r, len_, r.uniform(0.05, 0.12), nang=c['kim'] * 0.5)
    tron += go(n, r, len_, c['go'] * 0.4, c['go_f'], 0.01)
    if c['u'] > 0:
        tron += u_dien(n, r, np.clip(t / len_, 0, 1) * np.exp(-np.maximum(t - len_, 0) / 0.6), f0) * c['u'] * 1.3
    if r.random() < 0.6:
        tron = doi_lai(tron, r, 1, (0.2, 0.3), (0.25, 0.4), r.uniform(0.4, 0.7))
    tron = ss.tam(tron, 0.05)
    return th.hoan_tat(tron, r, r.uniform(0.9, 1.3), r.uniform(0.18, 0.28), -4)


def vo(hat, kieu):
    """KHIEN NO TUNG: "BOONG" lon meo (bao hoa) + nut xe toac + manh nang luong / kim loai vang ra + luong khi bung + than tram + tieng doi lai."""
    c = CAU_HINH[kieu]; r = np.random.default_rng(hat)
    n = int(SR * 2.4); t = np.arange(n) / SR
    f0 = r.uniform(*c['f0']) * r.uniform(0.8, 0.9)
    x = boong(n, r, 0.01, f0, kieu, r.uniform(*c['tat']) * 1.3, 1.0, 0.95, sang=c['sang'])
    x += anh_kim(n, r, 0.01, r.uniform(0.15, 0.3), nang=c['kim'] * 1.2)
    tron = np.tanh(2.2 * x) * 0.8                                         # meo - khien vo
    tron += tbn.xe_rach(n, r, 0.0, r.uniform(0.04, 0.09), r.uniform(1500, 2500), r.uniform(3000, 5000), r.uniform(0.6, 0.9))
    tron += tbn.bup(n, r, 0.01, r.uniform(260, 300), r.uniform(170, 190), r.uniform(0.08, 0.12), 1.0, 1.2)
    tron += tbn.gio_bat(n, r, 0.015, 0.01, r.uniform(0.2, 0.4), 350, r.uniform(2400, 3200)) * r.uniform(0.6, 0.9)
    # manh vang ra roi xuong (kim loai / nang luong) - moi manh mot "tinh" ngan + nay
    b2.cuc_roi(tron, r, int(r.integers(4, 9)), 0.1, 0.7, 900, 2600, (0.006, 0.02), nang=0.5, nay=(1, 3), rong=0.9)
    if c['vun'] > 0:
        tron += nl.soi(n, r, 0.01, 0.2, 1500, 0.4, (1500, 3600))
    if c['u'] > 0:
        tron += u_dien(n, r, bao_len_xuong(t, 0.005, 0.25, 0.0), f0) * 0.5
    tron = doi_lai(tron, r, c['doi'] + int(r.integers(0, 2)), (0.2, 0.32), (0.3, 0.45), r.uniform(0.5, 0.8))
    tron = ss.tam(tron, 0.05)
    return th.hoan_tat(tron, r, r.uniform(1.2, 1.6), r.uniform(0.2, 0.3), -2)


MAU = {'A_chuong_dong': ('chuong_dong', 24100), 'B_vom_nang_luong': ('vom_nang_luong', 24300), 'C_khien_sat': ('khien_sat', 24500)}


def ghep_game(bats, trungs, vos, phat=g3.can_bang):
    """MO PHONG TRONG GAME: 1) minh bat khien (giua); 2) 6 don quai trung khien lien tiep (cach 0,25-0,6 s) - tieng bam theo nguoi (giua); 3) khien vo;
    4) nguoi khac ben trai (-35 do) bat khien, an 3 don, vo."""
    out = np.zeros((int(SR * 12.0), 2))
    r = np.random.default_rng(7)
    tb.cong(out, phat(bats[0], 0), 0.3)
    tt = 1.4
    for k in range(6):
        tb.cong(out, phat(trungs[k % 3], 0) * r.uniform(0.75, 1.0), tt)
        tt += r.uniform(0.25, 0.6)
    tb.cong(out, phat(vos[0], 0), tt + 0.3)
    b0 = tt + 3.0
    tb.cong(out, phat(bats[1], -35) * 0.7, b0)
    tt = b0 + 1.2
    for k in range(3):
        tb.cong(out, phat(trungs[(k + 1) % 3], -35) * 0.7, tt); tt += r.uniform(0.35, 0.7)
    tb.cong(out, phat(vos[1], -35) * 0.7, tt + 0.2)
    return out / np.max(np.abs(out)) * 0.95


if __name__ == '__main__':
    ra = sys.argv[1] if len(sys.argv) > 1 else os.path.join('CongCu', 'AmThanh', 'KhiengThu')
    os.makedirs(ra, exist_ok=True)
    chi = sys.argv[2:]
    for ten, (kieu, hat0) in MAU.items():
        if chi and not any(c in ten for c in chi): continue
        bo = {'bat': [bat(hat0 + 10 * i, kieu) for i in range(3)], 'trung': [trung(hat0 + 100 + 10 * i, kieu) for i in range(3)],
              'vo': [vo(hat0 + 200 + 10 * i, kieu) for i in range(3)]}
        for loai, ds in bo.items():
            for i, x in enumerate(ds):
                tb.ghi(os.path.join(ra, '%s_%s%d.wav' % (ten, loai, i + 1)), x)
                th.do('%s_%s%d' % (ten, loai, i + 1), x)
            print('   chenh bien the %s: %s dB' % (loai, ' / '.join('%.1f' % c for c in th.chenh_bien_the(ds))))
        g = ghep_game(bo['bat'], bo['trung'], bo['vo'])
        tb.ghi(os.path.join(ra, '%s_ghep_nhu_trong_game.wav' % ten), g)
        th.do('%s_ghep' % ten, g)
