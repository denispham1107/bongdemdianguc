# -*- coding: utf-8 -*-
# TIENG BINH MAU / BINH MANA (16/10/2026, nguoi dung): "am thanh khi nguoi choi su dung binh mau va binh mana - nghe nhu tieng NUOC CHAY nguoi
# choi dang UONG"; "am thanh khi binh bi HUT ve phia nguoi choi, va am thanh nguoi choi da LAY DUOC binh". Nguoi choi khac trong suong mu
# thi khong nghe (TiengViTri lo).
#
# NUOC THAT = BOT KHI: moi bot khi trong chat long keu mot "blip" cong huong Minnaert (song sin tat 3-30 ms, cao do NHICH LEN khi bot noi);
# dong chay = hang tram bot nho ngau nhien + tieng xoi (on dai trung, nhieu loan). Khong dung on loc tinh (= tieng xe / gio).
#   uong  : (mo nut) -> rot chay vao mieng: dong bot + "oc oc" khi khi chui vao co binh (bot to, deu theo nhip) + 2-4 nhip NUOT "uc"
#           (toc than co hong + um tram ngan + tach luoi) -> (tho ra "haa"). Mau: long dac, bot tram hon; mana: long loang, bot sang hon.
#   hut   : gio vut lao toi SANG DAN + TO DAN (theo 1/khoang cach) + nuoc OC ACH trong binh (2-3 lan lac) + thuy tinh lach cach nhe.
#   nhat  : binh cham vao dai lung / tui: 1-2 tieng "canh" thuy tinh (40-90 mode khong dieu hoa, kich CHUNG hai kenh) + toc tui da + oc ach.
#
# Ba MAU:
#   A "that"     - chan that: uong uc uc, rot nuoc, tho ra; hut: gio + oc ach; nhat: canh thuy tinh + tui da.
#   B "phep"     - thuoc PHEP: them bot sui lap lanh + tieng ngan nhe khi uong xong; hut: gio xoay phep sang dan; nhat: canh + lap lanh nho.
#   C "dac"      - long DAC, binh day: "oc oc" to va tram, nuot nang; hut: vut tram + oc ach nang; nhat: canh day + oc ach.
#
# Lam theo memory quy-trinh-tieng-ky-nang-moi: am co cao do (bot, mode thuy tinh) lech trai/phai CHI bang am luong, moi bot mot cho; am tram
# hai kenh cung pha; 3 bien the khac CAU TRUC (so nhip nuot, co / khong mo nut, tho ra, so lan lac...); dai dien thoai.
#
# Chay: python CongCu/AmThanh/tieng_binh.py [thu_muc_ra] [loc ten mau...]   (mac dinh CongCu/AmThanh/BinhThu)

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
PI2 = tb.PI2
loc, dai, thap, cao, cham = q3.loc, q3.dai, q3.thap, q3.cao, q3.cham
on2, loc2, quet2, keo_lech, bao_len_xuong = th.on2, th.loc2, th.quet2, th.keo_lech, th.bao_len_xuong


# ------------------------------------------------------------------ bot khi
def bot(tron, r, bd, f0, tau, len_=0.35, a=1.0, p=0.0):
    """MOT bot khi (Minnaert): sin tat theo tau, cao do nhich len len_ (ti le) trong mot tau; lech trai/phai CHI bang am luong."""
    n = len(tron)
    i0 = int(bd * SR)
    if i0 >= n or i0 < 0: return
    d = min(n - i0, int(tau * 7 * SR))
    tt = np.arange(d) / SR
    f = f0 * (1 + len_ * np.minimum(tt / tau, 3.0))
    x = np.sin(PI2 * np.cumsum(f) / SR) * np.exp(-tt / tau) * np.minimum(1, tt / 0.0004)
    g = (np.clip(p, -1, 1) + 1) * np.pi / 4
    tron[i0:i0 + d, 0] += x * a * np.cos(g) * np.sqrt(2)
    tron[i0:i0 + d, 1] += x * a * np.sin(g) * np.sqrt(2)


def pan_rong(r, rong=1.0):
    """Vi tri mot su kien nho: 65% lech HAN mot ben (0,65-1), 35% gan giua - lech chi bang am luong. (Do lan 1: pan deu +/-0,75 -> tuong quan
    0,67-0,73 - gan mono: nguon pan am luong deu khap [-1, 1] cho tuong quan ~0,64 du rong het.)"""
    # (do lan 2: lech 0,49-0,75 van cho tuong quan 0,73 - mot nguon pan am luong p = 0,6 tu no da tuong quan 0,6; phai gan HAN mot kenh)
    if r.random() < 0.75:
        return (1 if r.random() < 0.5 else -1) * r.uniform(0.85, 1.0)
    return r.uniform(-0.3, 0.3) * rong


def dong_bot(tron, r, toc_t, f=(400, 1800), ty_tat=(0.03, 0.07), rong=0.8, nang=1.0, len_=(0.15, 0.6)):
    """Dong bot ngau nhien: toc_t = so bot / giay theo thoi gian (mang dai n). Bot nho (cao) nhieu va nho tieng hon bot to."""
    n = len(tron)
    vt = np.nonzero(r.random(n) < toc_t / SR)[0]
    for i in vt:
        u = r.random()
        fc = np.exp(np.log(f[0]) + (np.log(f[1]) - np.log(f[0])) * u ** 0.7)
        tau = 1.0 / (np.pi * r.uniform(*ty_tat) * fc)
        a = (f[0] / fc) ** 0.6 * np.exp(r.uniform(np.log(0.25), 0)) * nang
        bot(tron, r, i / SR, fc, tau, r.uniform(*len_), a, pan_rong(r, rong))


def xoi(n, r, bao, f=(450, 2600), chung=0.45, giat=16.0):
    """Tieng XOI / chay cua dong nuoc (khong cao do): on dai trung, bien do nhieu loan rieng tung kenh."""
    x = loc2(on2(n, r, chung), lambda ff: cao(f[0], 2)(ff) * thap(f[1], 3)(ff) * (0.7 + 0.5 * dai(1100, 1.0)(ff)))
    m = np.stack([np.maximum(1 + 0.7 * cham(n, giat, r), 0.05) for _ in range(2)], axis=1)
    x = x * m
    return x / (np.max(np.abs(x)) + 1e-12) * bao[:, None]


def oc(tron, r, bd, f0=420, a=1.0, p=0.0, tau=0.028):
    """"OC" - bong khi TO chui vao co binh: bot to (cao do nhich len manh) + nhieu bot nho bam theo + chut xoi."""
    bot(tron, r, bd, f0, tau, r.uniform(0.7, 1.1), a, p)
    bot(tron, r, bd + r.uniform(0.004, 0.012), f0 * r.uniform(1.35, 1.7), tau * 0.6, 0.6, a * 0.45, -p)
    for _ in range(int(r.integers(2, 6))):
        fc = f0 * r.uniform(1.8, 4.0)
        bot(tron, r, bd + r.uniform(0.0, 0.04), fc, 1.0 / (np.pi * 0.05 * fc), r.uniform(0.2, 0.6), a * r.uniform(0.1, 0.3), pan_rong(r))


def nuot(tron, r, bd, nang=1.0, f_um=(250, 195)):
    """NUOT "UC": toc co hong (on 220-900 Hz tat 25 ms) + um tram ngan CUNG PHA (b2.um_chung lech 0,3%) + tach luoi nho."""
    n = len(tron); t = np.arange(n) / SR
    tt = np.maximum(t - bd, 0)
    u = b2.um_chung(tt, r, f_um[0], f_um[1], r.uniform(0.022, 0.035), lech=0.003) * (t >= bd)[:, None]
    tron += u * 0.35 * nang
    tron += b3.toc(t, r, bd, 0.9 * nang, (220, 950), r.uniform(0.02, 0.035))
    tron += b3.toc(t, r, bd + r.uniform(0.03, 0.06), 0.25 * nang, (1300, 3200), 0.003)


def tho_ra(n, r, bd, giay, nang=1.0):
    """Tho ra "haa" sau khi uong (hoi tho - khong cao do)."""
    t = np.arange(n) / SR
    bao = bao_len_xuong(t, 0.05, giay, bd)
    return th.thi_tham(n, r, [(720, 1250, 2600), (650, 1200, 2500), (900, 1600, 2700)], bao, 0.4) * nang


def sui_lap_lanh(tron, r, bd, giay, toc=140, nang=1.0, f=(1600, 3400)):
    """Bot SUI li ti (nuoc phep) + vai tieng ngan nho: bot cao, nhieu, rat nho tieng, moi bot mot cho."""
    n = len(tron); t = np.arange(n) / SR
    toc_t = toc * bao_len_xuong(t, 0.06, giay, bd)
    dong_bot(tron, r, toc_t, f, (0.02, 0.05), 1.0, 0.35 * nang, (0.1, 0.3))
    for _ in range(int(r.integers(2, 5))):
        fc = r.uniform(1500, 3000)
        bot(tron, r, bd + r.uniform(0, giay), fc, r.uniform(0.06, 0.14), 0.0, 0.12 * nang, r.uniform(-0.8, 0.8))


def thuy_tinh(tron, r, bd, f1, nang=1.0, p=0.0, rong=0.5, day=1.0):
    """"CANH" binh thuy tinh: mode khong dieu hoa (cao tat nhanh), kich CHUNG hai kenh; moi mode mot cho (CHI am luong) quanh p."""
    n = len(tron)
    i0 = int(bd * SR)
    if i0 >= n: return
    d = min(n - i0, int(0.5 * SR))
    tt = np.arange(d) / SR
    ti = [1.0, 2.21, 2.93, 3.61, 4.37, 5.12]
    tat = [0.16, 0.085, 0.06, 0.045, 0.03, 0.022]
    bd_ = [1.0, 0.55, 0.42, 0.3, 0.18, 0.12]
    for k in range(len(ti)):
        fk = f1 * ti[k] * r.uniform(0.985, 1.015)
        if fk > 4600: continue
        x = np.sin(PI2 * fk * tt * (1 - 0.004 * np.exp(-tt / 0.01)) + r.uniform(0, PI2)) * np.exp(-tt / (tat[k] * day))
        x *= np.minimum(1, tt / 0.0003) * bd_[k] * nang * (fk < 4000 or 0.5)
        # moi mode MOT BEN xen ke (lech chi bang am luong, kich chung): pan nhe quanh p thi ca chum mode tuong quan 0,9 (do lan 1)
        g = (np.clip(p + (1 if k % 2 == 0 else -1) * r.uniform(0.75, 1.0) * rong * 2, -1, 1) + 1) * np.pi / 4
        tron[i0:i0 + d, 0] += x * np.cos(g) * np.sqrt(2)
        tron[i0:i0 + d, 1] += x * np.sin(g) * np.sqrt(2)
    # tich cham nho luc va
    t = np.arange(n) / SR
    tron += b3.toc(t, r, bd, 0.35 * nang, (1500, 3600), 0.0025)


def oc_ach(tron, r, bd, giay, nang=1.0, f=(380, 1500)):
    """Nuoc OC ACH trong binh (mot lan lac): on thap swish + vai bot."""
    n = len(tron); t = np.arange(n) / SR
    bao = bao_len_xuong(t, giay * 0.3, giay * 0.5, bd)
    tron += xoi(n, r, bao, (300, 1300), 0.4, 25.0) * 0.45 * nang
    toc_t = 260 * bao
    dong_bot(tron, r, toc_t, f, (0.04, 0.08), 0.7, 0.8 * nang, (0.3, 0.8))


# ------------------------------------------------------------------ cau hinh mau
CAU_HINH = {
    'that': dict(f_bot={'mau': (330, 2000), 'mana': (450, 2700)}, f_oc={'mau': (330, 430), 'mana': (420, 540)}, oc=0.8, xoi=0.45,
                 nuot=1.0, tho=0.55, sui=0.0, gio=0.8, phep=0.0, canh=1.0, f_canh=(1050, 1450), tui=0.8),
    'phep': dict(f_bot={'mau': (380, 2100), 'mana': (520, 2800)}, f_oc={'mau': (360, 460), 'mana': (460, 580)}, oc=0.65, xoi=0.4,
                 nuot=0.9, tho=0.35, sui=1.0, gio=0.75, phep=0.8, canh=0.9, f_canh=(1150, 1550), tui=0.6),
    'dac':  dict(f_bot={'mau': (260, 1600), 'mana': (340, 2000)}, f_oc={'mau': (260, 340), 'mana': (320, 420)}, oc=1.15, xoi=0.45,
                 nuot=1.25, tho=0.45, sui=0.0, gio=0.9, phep=0.0, canh=1.0, f_canh=(820, 1100), tui=1.0),
}


def uong(hat, kieu, loai):
    c = CAU_HINH[kieu]; r = np.random.default_rng(hat)
    so_nuot = int(r.integers(2, 5))
    nhip = r.uniform(0.26, 0.38)
    bd_rot = 0.0
    co_nut = r.random() < 0.55
    if co_nut: bd_rot = r.uniform(0.09, 0.14)
    dai_rot = so_nuot * nhip + r.uniform(0.05, 0.15)
    co_tho = r.random() < 0.75
    tong = bd_rot + dai_rot + (0.55 if co_tho else 0.15) + 0.25
    n = int(SR * tong); t = np.arange(n) / SR
    tron = np.zeros((n, 2))
    # (mo nut / ngam mieng binh) "poc" nho: bot to + toc
    if co_nut:
        tron += b3.toc(t, r, 0.0, 0.35, (500, 2200), 0.006)
        bot(tron, r, 0.002, r.uniform(700, 950), 0.012, 0.9, 0.5, r.uniform(-0.2, 0.2))
    # DONG ROT: toc bot len nhanh, phap phong theo nhip nuot (nuot thi dong chay ngat nhe), tat cuoi
    tr = t - bd_rot
    bao_rot = np.clip(tr / 0.06, 0, 1) * np.clip((dai_rot - tr) / 0.08, 0, 1)
    phap = 0.65 + 0.35 * np.cos(PI2 * np.maximum(tr, 0) / nhip) ** 2
    toc_bot = 520 * bao_rot * phap * (1 + 0.25 * cham(n, 6, r))
    dong_bot(tron, r, np.maximum(toc_bot, 0), c['f_bot'][loai], (0.03, 0.07), 0.75, 0.9)
    # (do lan 1: 1-4 kHz chi 8-19% o mau - duc nhu qua dien thoai; xoi mo len 3,2-3,6 kHz)
    tron += xoi(n, r, bao_rot * phap, (500, 3600) if loai == 'mana' else (420, 3200)) * c['xoi']
    # "OC OC": khi chui vao co binh, deu theo nhip nhung lech ngau nhien
    # (do lan 2: "oc" lech +/-0,25 -> tuong quan 0,97 - lop gan mono to nhat; nay XEN KE hai ben 0,5-0,85, bot phu bung ra hai phia)
    bd = bd_rot + r.uniform(0.04, 0.09)
    ben = 1 if r.random() < 0.5 else -1
    while bd < bd_rot + dai_rot - 0.05:
        oc(tron, r, bd, r.uniform(*c['f_oc'][loai]), c['oc'] * r.uniform(0.6, 1.0), ben * r.uniform(0.5, 0.85), r.uniform(0.022, 0.034))
        ben = -ben
        bd += r.uniform(0.1, 0.17)
    # NUOT
    for j in range(so_nuot):
        bdn = bd_rot + (j + 0.75) * nhip + r.uniform(-0.03, 0.03)
        nuot(tron, r, bdn, c['nuot'] * r.uniform(0.8, 1.1), (r.uniform(235, 270), r.uniform(185, 205)))
    # THO RA
    if co_tho:
        tron += tho_ra(n, r, bd_rot + dai_rot + r.uniform(0.04, 0.1), r.uniform(0.18, 0.3), c['tho'])
    # PHEP: bot sui lap lanh lan ra sau khi uong
    if c['sui'] > 0:
        sui_lap_lanh(tron, r, bd_rot + dai_rot * 0.6, r.uniform(0.35, 0.55), 160 if loai == 'mana' else 120, c['sui'],
                     (1800, 3600) if loai == 'mana' else (1400, 3000))
    return th.hoan_tat(tron, r, r.uniform(0.4, 0.65), r.uniform(0.18, 0.26), -3)


THOI_GIAN_HUT = 0.42      # binh o mep vung hut 3,5 m bay vao nguoi: 4t + 11t^2 = 3,5 -> 0,42 s (BinhRoi.Update)


def hut(hat, kieu):
    c = CAU_HINH[kieu]; r = np.random.default_rng(hat)
    toi = THOI_GIAN_HUT * r.uniform(0.85, 1.1)
    n = int(SR * (toi + 0.3)); t = np.arange(n) / SR
    # khoang cach con lai (m): 3,5 -> 0 theo s = 4t + 11 t^2; to theo 1/khoang cach (khong x^2), SANG dan
    s = np.clip(4 * t + 11 * t ** 2 * (THOI_GIAN_HUT / toi) ** 2, 0, 3.5)
    kc = np.maximum(3.5 - s, 0.25)
    bao = np.clip(t / 0.03, 0, 1) * np.where(t < toi, 0.6 / kc, (0.6 / 0.25) * np.exp(-(t - toi) / 0.07))
    bao = bao / np.max(bao)
    tron = nl.gio_than(n, r, bao, r.uniform(380, 520), r.uniform(1900, 2700)) * c['gio']
    if c['phep'] > 0:
        # gio xoay PHEP: on dai hep hon, tam truot len theo bao (sang dan) + xoay trai/phai
        fc = 900 + 1800 * bao
        x = quet2(on2(n, r, 0.4), fc, 5.0, lech=0.04)
        x = th.xoay(x / (np.max(np.abs(x)) + 1e-12), r.uniform(5, 8), 0.5, r)
        tron += x * bao[:, None] * 0.5 * c['phep']
        sui_lap_lanh(tron, r, toi * 0.35, toi * 0.8, 120, 0.7 * c['phep'], (1800, 3400))
    # nuoc OC ACH trong binh khi binh bi giat di: 2-3 lan lac
    so_lac = int(r.integers(2, 4))
    for j in range(so_lac):
        oc_ach(tron, r, r.uniform(0.0, 0.05) + j * toi / so_lac, r.uniform(0.08, 0.13), 0.9 * (0.7 + 0.3 * j / so_lac),
               (300, 1100) if kieu == 'dac' else (380, 1500))
    # thuy tinh lach cach nhe (nut binh / chat long va thanh binh) - co / khong
    if r.random() < 0.6:
        thuy_tinh(tron, r, r.uniform(0.05, toi * 0.7), r.uniform(*c['f_canh']) * 1.3, 0.18, r.uniform(-0.4, 0.4), 0.4, 0.6)
    return th.hoan_tat(tron, r, r.uniform(0.3, 0.5), r.uniform(0.15, 0.22), -4)


def nhat(hat, kieu):
    c = CAU_HINH[kieu]; r = np.random.default_rng(hat)
    n = int(SR * 0.9); t = np.arange(n) / SR
    tron = np.zeros((n, 2))
    # binh cham vao tui da / dai lung: toc than + "canh" thuy tinh voi binh khac (1-2 tieng)
    # (do lan 1: 1-4 kHz 93-97% - chi con thuy tinh chot chua, mong; than tui x2,5, thuy tinh x0,45, oc ach x2)
    tron += b3.toc(t, r, 0.0, 2.0 * c['tui'], (230, 900), r.uniform(0.025, 0.04))
    tron += b2.um_chung(t, r, r.uniform(250, 290), r.uniform(200, 225), 0.03, lech=0.003) * 0.5 * c['tui']
    tron += nl.cha_dat(n, r, 0.005, r.uniform(0.05, 0.09), 0.6 * c['tui'], (450, 2400), 40.0)    # da tui co xat
    so_canh = 1 + (1 if r.random() < 0.6 else 0)
    bd = r.uniform(0.0, 0.015)
    for j in range(so_canh):
        thuy_tinh(tron, r, bd, r.uniform(*c['f_canh']) * (1.0 if j == 0 else r.uniform(1.15, 1.4)),
                  0.45 * c['canh'] * (1.0 if j == 0 else r.uniform(0.4, 0.7)), r.uniform(-0.3, 0.3), 0.5, 1.2 if kieu == 'dac' else 1.0)
        bd += r.uniform(0.04, 0.09)
    oc_ach(tron, r, r.uniform(0.01, 0.03), r.uniform(0.12, 0.2), 1.6, (300, 1100) if kieu == 'dac' else (400, 1600))
    if c['phep'] > 0:
        sui_lap_lanh(tron, r, 0.03, r.uniform(0.3, 0.45), 100, 0.8 * c['phep'], (1800, 3400))
    return th.hoan_tat(tron, r, r.uniform(0.35, 0.55), r.uniform(0.12, 0.2), -3)


MAU = {'A_that': ('that', 23100), 'B_phep': ('phep', 23300), 'C_dac': ('dac', 23500)}


def ghep_game(uong_mau, uong_mana, huts, nhats, phat=g3.can_bang):
    """MO PHONG TRONG GAME (phat can bang nhu cac ky nang khac, nhan vat giua man):
    0,3 s binh mau o +35 do bi hut (bay vao giua) -> nhat; 1,3 s uong binh mau; 3,0 s binh mana o -30 do bi hut -> nhat; 3,9 s uong binh mana;
    5,8 s nguoi choi khac o +45 do (xa hon) uong binh mau."""
    out = np.zeros((int(SR * 7.6), 2))
    for k, (bd, az) in enumerate([(0.3, 35.0), (3.0, -30.0)]):
        h = huts[k % 3]
        tb.cong(out, phat(h, np.linspace(az, 0, len(h)) * (np.arange(len(h)) / SR < THOI_GIAN_HUT) + 0), bd)
        tb.cong(out, phat(nhats[k % 3], 0), bd + THOI_GIAN_HUT)
    tb.cong(out, phat(uong_mau[0], 0), 1.3)
    tb.cong(out, phat(uong_mana[1], 0), 3.9)
    tb.cong(out, phat(uong_mau[2], 45) * 0.6, 5.8)
    return out / np.max(np.abs(out)) * 0.95


if __name__ == '__main__':
    ra = sys.argv[1] if len(sys.argv) > 1 else os.path.join('CongCu', 'AmThanh', 'BinhThu')
    os.makedirs(ra, exist_ok=True)
    chi = sys.argv[2:]
    for ten, (kieu, hat0) in MAU.items():
        if chi and not any(c in ten for c in chi): continue
        bo = {'uong_mau': [uong(hat0 + 10 * i, kieu, 'mau') for i in range(3)],
              'uong_mana': [uong(hat0 + 50 + 10 * i, kieu, 'mana') for i in range(3)],
              'hut': [hut(hat0 + 100 + 10 * i, kieu) for i in range(3)],
              'nhat': [nhat(hat0 + 200 + 10 * i, kieu) for i in range(3)]}
        for loai, ds in bo.items():
            for i, x in enumerate(ds):
                tb.ghi(os.path.join(ra, '%s_%s%d.wav' % (ten, loai, i + 1)), x)
                th.do('%s_%s%d' % (ten, loai, i + 1), x)
            print('   chenh bien the %s: %s dB; can bang giua tq %s; tq 150 ms dau %s' % (
                loai, ' / '.join('%.1f' % c for c in th.chenh_bien_the(ds)),
                ' / '.join('%.2f' % g3.tq(g3.can_bang(x, 0)) for x in ds),
                ' / '.join('%.2f' % g3.tq(x[:int(0.15 * SR)]) for x in ds)))
        g = ghep_game(bo['uong_mau'], bo['uong_mana'], bo['hut'], bo['nhat'])
        tb.ghi(os.path.join(ra, '%s_ghep_nhu_trong_game.wav' % ten), g)
        th.do('%s_ghep' % ten, g)
