# -*- coding: utf-8 -*-
# TIENG NHAO LON (16/10/2026, nguoi dung): "am thanh hieu ung trong suot thoi gian nhan vat cuon nguoi nhao lon ve phia truoc, lam tieng dong
# nhao lon cho CHAN THAT". Nguoi choi khac trong suong mu thi khong nghe (TiengViTri lo).
#
# Nhao lon THAT = chuoi va cham cua co the voi mat dat, khong phai mot "vut" lien tuc. Ba tieng, PHAT THEO SU KIEN cua vong lon (NhaoLon.GocLat):
#   vut  (luc bat dau)        - than lao di + AO CHOANG phat (vai co xat lao xao, khong cao do) ~0,4-0,5 s
#   lan  (goc lat qua ~80 do) - VAI / LUNG CHAM DAT: "huych" than nguoi (tram, cung pha hai kenh) + dat soi lao xao + ao chà dat, lan tiep ~0,3 s
#   dap  (goc lat qua ~300 do)- HAI CHAN DAP (got + mui, cach nhau 50-120 ms) + soi ban + TRUOT nguoi dung lai (co / khong) + ao ru xuong
# Vong lan 8 m dai 0,76 s; lan ngan hon thi su kien den som hon - tieng van khop vi game phat theo goc lat (khong keo gian clip).
#
# Lam theo memory quy-trinh-tieng-ky-nang-moi: moi lop rieng tung kenh + ~40-60% chung; su kien nho (hat soi) moi cai mot cho, lech CHI bang am
# luong; am tram hai kenh CUNG PHA (b2.bum_chung); khong "piu", khong ngan; 3 bien the KHAC CAU TRUC; dai dien thoai.
#
# Ba MAU:
#   A "dat_soi"  - san nghia dia dat kho lan soi: soi lao xao ro, huych vua.
#   B "dat_mem"  - dat mem am + AO CHOANG nhieu: huych em, tieng ao phat / chà dai, it soi.
#   C "manh"     - lao manh: vut lon hon, huych nang, soi bat tung (vien soi nay), truot dai.
#
# Chay: python CongCu/AmThanh/tieng_nhao_lon.py [thu_muc_ra] [loc ten mau...]   (mac dinh CongCu/AmThanh/NhaoLonThu)

import os, sys
import numpy as np
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import tieng_bam as tb
import tieng_qua_cau_lua_3 as q3
import tieng_qua_cau_bang_2 as b2
import tieng_qua_cau_bang_3 as b3
import tieng_thien_thach_3 as t3
import tieng_tang_hinh as th
import tieng_giat_set_3 as g3
import tieng_sam_set as ss

SR = tb.SR
loc, dai, thap, cao, cham = q3.loc, q3.dai, q3.thap, q3.cao, q3.cham
on2, loc2, quet2, keo_lech, bao_len_xuong = th.on2, th.loc2, th.quet2, th.keo_lech, th.bao_len_xuong


# ------------------------------------------------------------------ lop co ban
def ao(n, r, bao, f=(700, 3400), chung=0.3, giat=22.0, sau=0.8):
    """AO CHOANG co xat / phat: on dai trung-cao, bien do GIAT ngau nhien (nep vai dap vao nhau) rieng tung kenh + chung; khong cao do."""
    x = loc2(on2(n, r, chung), lambda ff: cao(f[0], 2)(ff) * thap(f[1], 3)(ff) * (0.6 + 0.6 * dai(1600, 1.2)(ff)))
    m = np.stack([np.maximum(1 + sau * (0.6 * cham(n, giat, r) + 0.4 * cham(n, giat * 2.3, r)), 0.05) for _ in range(2)], axis=1)
    x = x * m
    return x / (np.max(np.abs(x)) + 1e-12) * bao[:, None]


def gio_than(n, r, bao, f0=350, f1=1900, chung=0.35):
    """Luong gio than nguoi lao qua (whoosh): on thong thap MO theo bao (nhanh -> sang hon), hai kenh lech tan so nhe."""
    fc = f0 + (f1 - f0) * bao / (np.max(bao) + 1e-12)
    x = quet2(on2(n, r, chung), fc + 1, kieu='thap', lech=0.05)
    x = x / (np.max(np.abs(x)) + 1e-12)
    return x * bao[:, None]


def soi(n, r, bd, giay, mat_do, nang=1.0, f=(700, 3600)):
    """SOI / DAT VUN lao xao: hat va cham tat 0,3-3 ms, moi hat mot CHO (lech chi bang am luong), mat do giam dan."""
    t = np.arange(n) / SR
    e = np.clip((t - bd) / 0.006, 0, 1) * np.exp(-np.maximum(t - bd, 0) / giay)
    vt = np.nonzero(r.random(n) < e * mat_do / SR)[0]
    ra = np.zeros((n, 2))
    nhan = []
    for _ in range(6):
        d = int(r.uniform(0.0015, 0.006) * SR)
        fc = np.exp(r.uniform(np.log(f[0]), np.log(f[1])))
        k = loc(np.pad(r.standard_normal(d) * np.exp(-np.arange(d) / (d / 3)), (0, 256)), dai(fc, 2.0))
        nhan.append(k / (np.max(np.abs(k)) + 1e-12))
    for i in vt:
        k = nhan[r.integers(0, 6)]
        a = np.exp(r.uniform(np.log(0.12), 0))
        g = (r.uniform(-0.9, 0.9) + 1) * np.pi / 4
        m = min(len(k), n - i)
        ra[i:i + m, 0] += k[:m] * a * np.cos(g)
        ra[i:i + m, 1] += k[:m] * a * np.sin(g)
    return ra / (np.max(np.abs(ra)) + 1e-12) * nang


def cha_dat(n, r, bd, giay, nang=1.0, f=(350, 2600), giat=35.0, len_=0.012):
    """CHA / TRUOT tren dat: on dai rong, bao len nhanh tat dan, NHAM (bien do giat nhanh sau) - rieng tung kenh 50% chung."""
    t = np.arange(n) / SR
    x = loc2(on2(n, r, 0.35), lambda ff: cao(f[0], 2)(ff) * thap(f[1], 2)(ff))
    nham = np.stack([np.maximum(1 + 0.9 * cham(n, giat, r), 0.0) for _ in range(2)], axis=1)
    x = x * nham
    x = x / (np.max(np.abs(x)) + 1e-12)
    return x * bao_len_xuong(t, len_, giay, bd)[:, None] * nang


def huych(n, r, bd, f0, f1, giay, nang=1.0):
    """"HUYCH" than nguoi cham dat: bum_chung (tram, cung pha) nhe + "toc" da thit 250-900 Hz - than dat ~ 120-300 Hz (loa dien thoai)."""
    t = np.arange(n) / SR
    tt = np.maximum(t - bd, 0)
    # (do lan 1: bum 0,55 + toc 220-900 -> < 200 Hz 45-60%; nay than dat o 250-1100 Hz, bum nhe chi giu "thit")
    x = b2.bum_chung(tt, r, f0, f1, giay, 0.32 * nang) * (t >= bd)[:, None] * 1.1
    x += b3.toc(t, r, bd, 0.9 * nang, (260, 1100), 0.02)
    return x


def vien_soi(tron, r, so, bd0, bd1, nang=1.0):
    """Vien soi bi hat bat ra roi NAY 1-3 lan (cach nhau ngan dan) - b2.cuc_roi (lech chi bang am luong)."""
    b2.cuc_roi(tron, r, so, bd0, bd1, 900, 2600, (0.003, 0.012), nang=nang, nay=(1, 3), rong=0.9)


# ------------------------------------------------------------------ ba tieng
CAU_HINH = {
    #           vut(muc gio, muc ao)  lan(huych, soi mat do, soi muc, cha muc)   dap(got, soi, truot co, truot dai)  vien soi
    'dat_soi': dict(gio=0.8, ao=0.55, huych=1.0, soi_md=1800, soi=0.75, cha=0.55, got=0.9, soi_dap=0.7, truot=0.7, truot_dai=(0.12, 0.22), vien=(1, 3), f_huych=(270, 175)),
    'dat_mem': dict(gio=0.7, ao=0.95, huych=0.75, soi_md=500, soi=0.35, cha=0.8, got=0.65, soi_dap=0.3, truot=0.55, truot_dai=(0.15, 0.28), vien=(0, 1), f_huych=(250, 165)),
    'manh':    dict(gio=1.0, ao=0.6, huych=1.25, soi_md=2400, soi=0.85, cha=0.6, got=1.15, soi_dap=0.85, truot=0.95, truot_dai=(0.2, 0.34), vien=(3, 6), f_huych=(290, 185)),
}


def vut(hat, kieu):
    c = CAU_HINH[kieu]; r = np.random.default_rng(hat)
    dinh = r.uniform(0.12, 0.22)               # luc nhanh nhat (lao nhanh dau vong)
    dai_ = dinh + r.uniform(0.22, 0.38)
    n = int(SR * (dai_ + 0.25)); t = np.arange(n) / SR
    bao = np.clip(t / dinh, 0, 1) ** 1.6 * np.exp(-np.maximum(t - dinh, 0) / ((dai_ - dinh) / 2.2))
    tron = gio_than(n, r, bao, r.uniform(280, 420), r.uniform(1500, 2300)) * c['gio']
    # ao choang: phat manh luc lao + mot nhip "phach" (khong / mot / hai nhip - CAU TRUC khac nhau)
    tron += ao(n, r, bao ** 0.8) * c['ao']
    for _ in range(int(r.integers(0, 3))):
        bd = r.uniform(0.04, dinh + 0.1)
        tron += ao(n, r, bao_len_xuong(t, 0.006, r.uniform(0.03, 0.07), bd), (900, 3600), 0.4, 60.0, 0.9) * c['ao'] * r.uniform(0.5, 0.9)
    # buoc dap chan lay da (tuy bien the): mot tieng chan nhe luc bat dau
    if r.random() < 0.6:
        tron += huych(n, r, r.uniform(0.0, 0.03), 280, 185, 0.03, 0.35) + soi(n, r, 0.005, 0.05, 600, 0.25)
    tron = ss.tam(tron, 0.05)
    return th.hoan_tat(tron, r, r.uniform(0.5, 0.8), r.uniform(0.18, 0.28), -4)


def lan(hat, kieu):
    c = CAU_HINH[kieu]; r = np.random.default_rng(hat)
    n = int(SR * 0.85); t = np.arange(n) / SR
    tron = np.zeros((n, 2))
    # VAI cham dat roi LUNG cham dat (1 hoac 2 nhip "huych", cach 50-110 ms)
    nhip = [0.0] + ([r.uniform(0.05, 0.11)] if r.random() < 0.65 else [])
    for j, bd in enumerate(nhip):
        f0, f1 = c['f_huych']
        tron += huych(n, r, bd + 0.005, f0 * r.uniform(0.92, 1.08), f1 * r.uniform(0.95, 1.05), r.uniform(0.04, 0.07),
                      c['huych'] * (1.0 if j == 0 else r.uniform(0.5, 0.8)))
    # dat soi bi de / lan qua
    # (do lan 2: 1-4 kHz chi 8-14% - duc, mat tieng dat soi; x1,8)
    tron += soi(n, r, 0.004, r.uniform(0.08, 0.16), c['soi_md'] * r.uniform(0.7, 1.3), c['soi'] * 1.8)
    tron += soi(n, r, r.uniform(0.08, 0.16), r.uniform(0.05, 0.1), c['soi_md'] * 0.4, c['soi'] * 0.9)
    # ao + than CHA DAT trong luc lan tiep (giam dan)
    tron += cha_dat(n, r, 0.01, r.uniform(0.12, 0.22), c['cha'] * 1.4, (400, 3000), r.uniform(25, 45))
    tron += ao(n, r, bao_len_xuong(t, 0.02, r.uniform(0.1, 0.18), 0.0), (650, 3000), 0.45, 30.0) * 0.35 * c['ao'] / 0.6
    vien_soi(tron, r, int(r.integers(*c['vien'])) if c['vien'][1] > c['vien'][0] else c['vien'][0], 0.02, 0.18, 0.45)
    tron = ss.tam(tron, 0.05)
    return th.hoan_tat(tron, r, r.uniform(0.4, 0.7), r.uniform(0.15, 0.24), -3)


def dap(hat, kieu):
    c = CAU_HINH[kieu]; r = np.random.default_rng(hat)
    n = int(SR * 0.9); t = np.arange(n) / SR
    tron = np.zeros((n, 2))
    # HAI CHAN dap: chan truoc (got) roi chan sau, cach 50-120 ms, moi chan mot ben (lech nhe bang am luong); doi khi them nhip thu ba (buoc dung lai)
    so_buoc = 2 + (1 if r.random() < 0.35 else 0)
    bd = 0.0
    for j in range(so_buoc):
        a = c['got'] * (1.0 if j == 0 else r.uniform(0.55, 0.85))
        b = huych(n, r, bd + 0.003, r.uniform(270, 320), r.uniform(175, 200), r.uniform(0.02, 0.035), 0.55 * a)
        b += b3.toc(t, r, bd + 0.002, 0.6 * a, (450, 1800), r.uniform(0.008, 0.015))      # de giay cung "cop"
        b += soi(n, r, bd + 0.003, r.uniform(0.04, 0.08), c['soi_md'] * 0.7, c['soi_dap'] * a * 1.8)
        tron += keo_lech(b, (-0.25 if j % 2 == 0 else 0.25) * r.uniform(0.6, 1.2))
        bd += r.uniform(0.05, 0.12) if j == 0 else r.uniform(0.12, 0.22)
    # TRUOT nguoi dung lai (de giay cha dat soi) - co / khong, dai ngan
    if r.random() < 0.8:
        bd_t = r.uniform(0.01, 0.05); dt = r.uniform(*c['truot_dai'])
        bao = np.clip((t - bd_t) / 0.02, 0, 1) * np.clip(1 - (t - bd_t - 0.02) / dt, 0, 1) ** 1.5
        x = loc2(on2(n, r, 0.35), lambda ff: cao(400, 2)(ff) * thap(3000, 2)(ff))
        x *= np.stack([np.maximum(1 + 0.8 * cham(n, 40, r), 0) for _ in range(2)], axis=1)
        tron += x / (np.max(np.abs(x)) + 1e-12) * bao[:, None] * c['truot']
        tron += soi(n, r, bd_t + 0.01, dt * 0.6, c['soi_md'] * 0.6, c['soi_dap'] * 1.1)
    # ao choang RU xuong sau khi dung
    tron += ao(n, r, bao_len_xuong(t, 0.03, r.uniform(0.08, 0.16), r.uniform(0.05, 0.14)), (700, 3200), 0.45, 18.0) * c['ao'] * 0.6
    vien_soi(tron, r, max(0, int(r.integers(c['vien'][0], c['vien'][1] + 1)) - 1), 0.01, 0.12, 0.35)
    tron = ss.tam(tron, 0.05)
    return th.hoan_tat(tron, r, r.uniform(0.4, 0.7), r.uniform(0.15, 0.22), -4)


MAU = {'A_dat_soi': ('dat_soi', 22100), 'B_dat_mem': ('dat_mem', 22300), 'C_manh': ('manh', 22500)}


# ------------------------------------------------------------------ thoi diem su kien (y NhaoLon.cs)
def goc_lat(u):
    tt = np.clip((u - 0.06) / 0.84, 0, 1)
    return 360 * tt ** 3 * (tt * (tt * 6 - 15) + 10)


GOC_LAN, GOC_DAP = 80.0, 300.0


def thoi_diem(quang):
    """(giay cham vai, giay dap chan, tong) cua mot vong lan quang m."""
    tong = 0.34 + 0.052 * quang
    u = np.linspace(0, 1, 20001); g = goc_lat(u)
    return u[np.argmax(g >= GOC_LAN)] * tong, u[np.argmax(g >= GOC_DAP)] * tong, tong


def ghep_game(vuts, lans, daps, phat=g3.can_bang):
    """MO PHONG TRONG GAME (phat can bang nhu cac ky nang khac): 1) minh lan 8 m (giua man); 2) nguoi khac ben trai lan 8 m tu -45 sang -25 do;
    3) minh lan 3 m; 4) nguoi khac ben phai lan 6 m tu +20 sang +40 do."""
    out = np.zeros((int(SR * 6.0), 2))
    lich = [(0.3, 8, 0, 0), (1.6, 8, -45, -25), (2.9, 3, 0, 0), (4.2, 6, 20, 40)]
    for k, (bd, q, az0, az1) in enumerate(lich):
        tl, td, tong = thoi_diem(q)
        az = lambda tt: az0 + (az1 - az0) * min(1.0, tt / tong)
        a = 1.0 if az0 == 0 else 0.7
        v = vuts[k % 3]
        av = np.linspace(az0, az1, len(v)) if az0 != az1 else 0
        tb.cong(out, phat(v, av) * a, bd)
        tb.cong(out, phat(lans[k % 3], az(tl)) * a, bd + tl)
        tb.cong(out, phat(daps[k % 3], az(td)) * a, bd + td)
    return out / np.max(np.abs(out)) * 0.95


if __name__ == '__main__':
    ra = sys.argv[1] if len(sys.argv) > 1 else os.path.join('CongCu', 'AmThanh', 'NhaoLonThu')
    os.makedirs(ra, exist_ok=True)
    chi = sys.argv[2:]
    for q in (8, 5, 2, 0.8):
        print('lan %.1f m: cham vai %.3f s, dap chan %.3f s, tong %.3f s' % ((q,) + thoi_diem(q)))
    for ten, (kieu, hat0) in MAU.items():
        if chi and not any(c in ten for c in chi): continue
        bo = {'vut': [vut(hat0 + 10 * i, kieu) for i in range(3)], 'lan': [lan(hat0 + 100 + 10 * i, kieu) for i in range(3)],
              'dap': [dap(hat0 + 200 + 10 * i, kieu) for i in range(3)]}
        for loai, ds in bo.items():
            for i, x in enumerate(ds):
                tb.ghi(os.path.join(ra, '%s_%s%d.wav' % (ten, loai, i + 1)), x)
                th.do('%s_%s%d' % (ten, loai, i + 1), x)
            print('   chenh bien the %s: %s dB; can bang giua tq %s' % (loai, ' / '.join('%.1f' % c for c in th.chenh_bien_the(ds)),
                  ' / '.join('%.2f' % g3.tq(g3.can_bang(x, 0)) for x in ds)))
        g = ghep_game(bo['vut'], bo['lan'], bo['dap'])
        tb.ghi(os.path.join(ra, '%s_ghep_nhu_trong_game.wav' % ten), g)
        th.do('%s_ghep' % ten, g)
