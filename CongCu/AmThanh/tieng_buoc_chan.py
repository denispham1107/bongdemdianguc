# -*- coding: utf-8 -*-
# TIENG BUOC CHAN (17/10/2026, nguoi dung): "khi nguoi choi va tat ca nhan vat co trong phong di chuyen deu co am thanh stereo tieng buoc chan". Nguoi dung
# chon: nguoi choi + BOT (khong quai); doi theo mat dat: DAT SOI (binh thuong) + LOI NUOC (vung nuoc Act2). Nguoi trong suong khong nghe (TiengViTri).
#
# BUOC CHAN THAT = chuoi va cham ngan (~0,25 s), khong phai mot "bup":
#   DAT SOI: GOT cham ("thich" than 220-600 Hz cung pha hai kenh + toc 230-900 Hz) -> DAT NEN + SOI lao xao (hat 0,3-3 ms, 900-3600 Hz, MOI HAT MOT CHO -
#            lech chi bang am luong) -> MUI CHAN lan (60-110 ms sau: lao xao nho + cha nhe) -> (co / khong) ao choang phat nhe.
#   LOI NUOC: chan LUN NUOC ("om": on 300-1200 Hz day nuoc + bot to 250-450 Hz) -> OC ACH (bot vua) -> GIOT BAN roi lai (bot nho 1,2-3 kHz, 50-300 ms sau,
#            moi giot mot cho) -> nuoc chay xuong ong.
# Moi loai 6 bien the KHAC CAU TRUC (cuong do got, so hat soi, co / khong lan mui, ao choang, so giot ...); game chon ngau nhien + cao do +-6%.
#
# Ba MAU:
#   A "dat_soi_kho" - dat kho lan soi: soi lao xao ro, got vua.
#   B "dat_am_mem"  - dat am mem: got em "thich", it soi, tieng dat lun nhe.
#   C "ung_da_nang" - ung da nang: got CUNG ro ("coc") + soi, da ung keu nhe.
#
# Chay: python CongCu/AmThanh/tieng_buoc_chan.py [thu_muc_ra] [loc ten mau...]   (mac dinh CongCu/AmThanh/BuocChanThu)

import os, sys
import numpy as np
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import tieng_bam as tb
import tieng_qua_cau_lua_3 as q3
import tieng_qua_cau_bang_2 as b2
import tieng_qua_cau_bang_3 as b3
import tieng_tang_hinh as th
import tieng_giat_set_3 as g3
import tieng_nhao_lon as nl
import tieng_binh as tbinh

SR = tb.SR
PI2 = tb.PI2
loc, dai, thap, cao, cham = q3.loc, q3.dai, q3.thap, q3.cao, q3.cham
on2, loc2, keo_lech, bao_len_xuong = th.on2, th.loc2, th.keo_lech, th.bao_len_xuong

def soi_rong(n, r, bd, giay, mat_do, nang=1.0, f=(900, 3200)):
    """SOI lao xao nhu nl.soi nhung moi hat GAN HAN mot kenh (tbinh.pan_rong) - do lan 1: nl.soi lech deu +-0,9 -> tuong quan buoc chan 0,55-0,69."""
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
        g = (tbinh.pan_rong(r) + 1) * np.pi / 4
        m = min(len(k), n - i)
        ra[i:i + m, 0] += k[:m] * a * np.cos(g)
        ra[i:i + m, 1] += k[:m] * a * np.sin(g)
    return ra / (np.max(np.abs(ra)) + 1e-12) * nang


def toc_rieng(t, r, bd, nang, f, tat):
    """Toc va cham: MOI KENH MOT NGUON ON rieng (lech 0-1,5 ms) - tranh lop to nhat nam giua (memory quy trinh muc 1)."""
    a = b3.toc(t, r, bd, nang, f, tat)
    b = b3.toc(t, r, bd + r.uniform(0.0, 0.0015), nang, f, tat)
    return np.stack([a[:, 0], b[:, 1]], axis=1)


CAU_HINH = {
    # A2 (ban 2 cua A, nguoi dung chon A nhung "hoi mong, con mono"): than dat tram + phan xa som + chan trai lech -0,2
    'dat_soi_kho_2': dict(got=1.1, f_got=(200, 520), soi_md=(1400, 2600), soi=0.9, f_soi=(1000, 3000), nen=0.7, mui=0.8, ao=0.25, ung=0.0, cung=0.0,
                          than=0.9, phan_xa=True, lech=-0.2),
    'dat_soi_kho': dict(got=1.0, f_got=(260, 600), soi_md=(1400, 2600), soi=1.0, f_soi=(1000, 3200), nen=0.5, mui=0.8, ao=0.25, ung=0.0, cung=0.0),
    'dat_am_mem':  dict(got=0.85, f_got=(220, 480), soi_md=(300, 700), soi=0.45, f_soi=(800, 2800), nen=1.0, mui=0.5, ao=0.3, ung=0.0, cung=0.0),
    'ung_da_nang': dict(got=1.2, f_got=(280, 700), soi_md=(900, 1800), soi=0.8, f_soi=(1000, 3100), nen=0.5, mui=0.9, ao=0.2, ung=0.5, cung=1.0),
}


def than_dat(n, r, bd, nang, f=(110, 520), tat=(0.05, 0.09)):
    """THAN DAT tram (ban 2 - nguoi dung "con hoi mong"): on loc thap MOI KENH MOT NGUON, len 3 ms tat 50-90 ms - khoi dat bi dan xuong."""
    t = np.arange(n) / SR
    # 40% CHUNG giu tam (do lan 2: hoan toan rieng -> tuong quan 0,01-0,02 = loang, mat tam)
    ra = loc2(on2(n, r, 0.63), lambda ff: cao(f[0], 2)(ff) * thap(f[1], 2)(ff)) * bao_len_xuong(t, 0.003, r.uniform(*tat), bd)[:, None]
    return ra / (np.max(np.abs(ra)) + 1e-9) * nang


def phan_xa(tron, r, so=(4, 9), muc=(0.14, 0.34)):
    """PHAN XA SOM tu bia / cay quanh (ban 2 - nguoi dung "con nghe mono"): ban kho tre 8-60 ms, loc thap rieng, MOI CAI VE MOT BEN."""
    kho = tron.copy()
    m = kho.mean(axis=1)
    for j in range(int(r.integers(*so))):
        tre = int(r.uniform(0.008, 0.06) * SR)
        sig = loc(m, thap(r.uniform(1400, 3200), 2)) * r.uniform(*muc) * (1 - j * 0.08)
        ben = 0 if r.random() < 0.5 else 1
        n = len(tron) - tre
        if n <= 0: continue
        tron[tre:, ben] += sig[:n]
        tron[tre:, 1 - ben] += sig[:n] * r.uniform(0.0, 0.25)
    return tron


def buoc_dat(hat, kieu):
    c = CAU_HINH[kieu]; r = np.random.default_rng(hat)
    n = int(SR * 0.42); t = np.arange(n) / SR
    tron = np.zeros((n, 2))
    nang = r.uniform(0.7, 1.15)
    # GOT cham: "thich" than cung pha (um_chung lech 0,3%) + toc dat
    f0 = r.uniform(*c['f_got'])
    tt = t
    tron += b2.um_chung(tt, r, f0 * 1.15, f0, r.uniform(0.014, 0.024), lech=0.003) * 0.2 * c['got'] * nang
    tron += toc_rieng(t, r, 0.0, 0.9 * c['got'] * nang, (230, 950), r.uniform(0.012, 0.022))
    if c['cung'] > 0:      # got ung CUNG "coc": toc sang ngan
        tron += toc_rieng(t, r, 0.0, 0.55 * c['cung'] * nang, (700, 2400), r.uniform(0.004, 0.008))
    # DAT NEN: on tram trung lun xuong (khong cao do)
    bao_nen = bao_len_xuong(t, 0.004, r.uniform(0.03, 0.06), 0.002)
    x = loc2(on2(n, r, 0.4), lambda ff: cao(250, 2)(ff) * thap(1400, 2)(ff))
    tron += x / (np.max(np.abs(x)) + 1e-9) * bao_nen[:, None] * 0.45 * c['nen'] * nang
    # THAN DAT tram (ban 2: day hon)
    if c.get('than', 0) > 0:
        tron += than_dat(n, r, 0.0, c['than'] * nang)
    # SOI lao xao (moi hat mot cho)
    tron += soi_rong(n, r, 0.002, r.uniform(0.04, 0.09), r.uniform(*c['soi_md']), 0.9 * c['soi'] * nang, c['f_soi'])
    # MUI CHAN lan (co / khong)
    if r.random() < 0.8:
        bd = r.uniform(0.06, 0.11)
        tron += soi_rong(n, r, bd, r.uniform(0.03, 0.06), r.uniform(*c['soi_md']) * 0.6, 0.5 * c['soi'] * c['mui'] * nang, c['f_soi'])
        tron += nl.cha_dat(n, r, bd, r.uniform(0.03, 0.06), 0.35 * c['mui'] * nang, (400, 2600), 40.0)
    # AO CHOANG phat nhe (co / khong)
    if r.random() < 0.5:
        tron += nl.ao(n, r, bao_len_xuong(t, 0.02, r.uniform(0.05, 0.1), r.uniform(0.0, 0.05)), (700, 3200), 0.4, 30.0) * c['ao']
    # DA UNG keu nhe (co / khong)
    if c['ung'] > 0 and r.random() < 0.5:
        tron += nl.ao(n, r, bao_len_xuong(t, 0.01, 0.04, r.uniform(0.03, 0.08)), (500, 1800), 0.5, 80.0, 0.9) * c['ung'] * 0.5
    if c.get('phan_xa', False):
        tron = phan_xa(tron, r)
    if c.get('lech', 0) != 0:
        tron = keo_lech(tron, c['lech'])        # CHAN TRAI lech nhe trai - game dung ban LAT cho chan phai
    return th.hoan_tat(tron, r, r.uniform(0.25, 0.4), r.uniform(0.1, 0.16), -3)


def buoc_nuoc(hat, kieu):
    c = CAU_HINH[kieu]; r = np.random.default_rng(hat)
    n = int(SR * 0.6); t = np.arange(n) / SR
    tron = np.zeros((n, 2))
    nang = r.uniform(0.75, 1.15)
    # chan LUN NUOC: "om" - on day nuoc + bot to + got cham day (nhe)
    bao = bao_len_xuong(t, 0.008, r.uniform(0.05, 0.09), 0.0)
    tron += tbinh.xoi(n, r, bao, (300, 1300), 0.25 if c.get('phan_xa', False) else 0.4, 25.0) * 0.7 * nang
    for _ in range(int(r.integers(1, 3))):
        tbinh.bot(tron, r, r.uniform(0.0, 0.02), r.uniform(250, 450), r.uniform(0.02, 0.035), r.uniform(0.6, 1.0), 0.8 * nang,
                  tbinh.pan_rong(r, 1.0 if c.get('phan_xa', False) else 0.6))
    tron += toc_rieng(t, r, 0.0, 0.35 * c['got'] * nang, (230, 800), 0.015)
    # OC ACH: bot vua
    toc = 600 * bao_len_xuong(t, 0.01, r.uniform(0.06, 0.12), 0.005)
    tbinh.dong_bot(tron, r, toc, (450, 1700), (0.04, 0.08), 0.9, 0.9 * nang, (0.3, 0.8))
    # GIOT BAN roi lai: bot nho 1,2-3 kHz, 50-300 ms sau, moi giot mot cho
    for _ in range(int(r.integers(4, 12))):
        fc = r.uniform(1200, 3000)
        tbinh.bot(tron, r, r.uniform(0.05, 0.3), fc, 1.0 / (np.pi * 0.05 * fc), r.uniform(0.3, 0.8), r.uniform(0.15, 0.4) * nang, tbinh.pan_rong(r))
    # nuoc chay xuong ong (co / khong)
    if r.random() < 0.6:
        tron += tbinh.xoi(n, r, bao_len_xuong(t, 0.03, r.uniform(0.08, 0.15), r.uniform(0.08, 0.15)), (600, 2800), 0.35, 20.0) * 0.25
    if c.get('than', 0) > 0:
        tron += than_dat(n, r, 0.0, 0.5 * c['than'] * nang, (120, 450), (0.06, 0.1))
    if c.get('phan_xa', False):
        tron = phan_xa(tron, r)
    if c.get('lech', 0) != 0:
        tron = keo_lech(tron, c['lech'])
    return th.hoan_tat(tron, r, r.uniform(0.25, 0.4), r.uniform(0.12, 0.18), -3)


MAU = {'A2_dat_soi_kho_day': ('dat_soi_kho_2', 27700), 'A_dat_soi_kho': ('dat_soi_kho', 27100), 'B_dat_am_mem': ('dat_am_mem', 27300), 'C_ung_da_nang': ('ung_da_nang', 27500)}

# nhip buoc that cua clip "diBo" (1,033 s, toc phat 0,7-1,25 theo muc di): chan trai cham dat o pha 0,20, chan phai 0,70
CHU_KY_DI = 1.0333 / 1.25


def ghep_game(dat, nuoc, phat=g3.can_bang):
    """MO PHONG TRONG GAME: 0-3,5 s minh di (giua) tren dat; 3,5-5 s minh loi nuoc; 5,5-9 s nguoi khac di ngang tu trai sang phai cach ~8 m (nho hon);
    9,5-11 s BOT chay ben phai."""
    out = np.zeros((int(SR * 11.5), 2))
    k = 0
    def chuoi(t0, t1, ds, az0, az1, a, chu_ky):
        nonlocal k
        t = t0
        chan = 0
        while t < t1:
            az = az0 + (az1 - az0) * (t - t0) / max(1e-3, t1 - t0)
            x = ds[k % len(ds)]; k += 1
            if chan == 1: x = x[:, ::-1]          # chan phai: ban LAT (nhu trong game)
            tb.cong(out, phat(x, az) * a, t)
            t += chu_ky * (0.5 if chan == 0 else 0.5)
            chan ^= 1
    chuoi(0.2, 3.5, dat, 0, 0, 1.0, CHU_KY_DI)
    chuoi(3.6, 5.2, nuoc, 0, 0, 1.0, CHU_KY_DI)
    chuoi(5.5, 9.0, dat, -60, 60, 0.45, CHU_KY_DI * 1.05)
    chuoi(9.5, 11.2, dat, 40, 40, 0.6, CHU_KY_DI * 0.95)
    return out / np.max(np.abs(out)) * 0.95


if __name__ == '__main__':
    ra = sys.argv[1] if len(sys.argv) > 1 else os.path.join('CongCu', 'AmThanh', 'BuocChanThu')
    os.makedirs(ra, exist_ok=True)
    chi = sys.argv[2:]
    for ten, (kieu, hat0) in MAU.items():
        if chi and not any(c in ten for c in chi): continue
        bo = {'dat': [buoc_dat(hat0 + 10 * i, kieu) for i in range(6)], 'nuoc': [buoc_nuoc(hat0 + 100 + 10 * i, kieu) for i in range(6)]}
        for loai, ds in bo.items():
            for i, x in enumerate(ds):
                tb.ghi(os.path.join(ra, '%s_%s%d.wav' % (ten, loai, i + 1)), x)
                th.do('%s_%s%d' % (ten, loai, i + 1), x)
            print('   chenh bien the %s: %s dB; can bang giua tq %s' % (loai, ' / '.join('%.1f' % c for c in th.chenh_bien_the(ds)[:6]),
                  ' / '.join('%.2f' % g3.tq(g3.can_bang(x, 0)) for x in ds)))
        g = ghep_game(bo['dat'], bo['nuoc'])
        tb.ghi(os.path.join(ra, '%s_ghep_nhu_trong_game.wav' % ten), g)
        th.do('%s_ghep' % ten, g)
