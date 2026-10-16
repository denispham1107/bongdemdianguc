# -*- coding: utf-8 -*-
# TIENG QUA KEU THAM THIET + BAY DOI DAP CANH BAY (17/10/2026, nguoi dung): "cu moi 1 phut them am thanh stereo tieng qua keu tham thiet, sau khi qua
# keu 30 giay them tieng bay doi dap canh bay". Nguoi dung chon: chi trong tran Act2, qua keu tu HUONG NGAU NHIEN xa xa, doi bay VUT NGANG tu ben nay
# sang ben kia qua dau, am luong theo thanh Hieu ung.
#
# HUONG NUONG SAN TRONG CLIP (WebGL khong co panStereo): moi bien the co ban LAT trai/phai -> game phat 2D giua, chon ngau nhien.
#
# QUA THAT: tieng "QUAAA" khan = nguon xung (rang cua) cao do 380-750 Hz co RUNG cao do tung chu ky (jitter 4-9%) + nhay bien do (shimmer) + chen DUOI
#   HOA AM (f0/2, chu ky doi) tung luc -> giong vo, re; loc 3 dai cong huong (formant 1-1,4 / 1,7-2,3 / 2,8-3,3 kHz) + hoi on; duong cao do VONG LEN roi
#   SUT XUONG (keo dai, "tham thiet": dai 0,45-0,9 s, cuoi giong vo gay). 2-5 tieng mot dot, cach 0,25-0,6 s; xa: loc thap + vang nghia dia stereo lon.
#   Am co cao do -> lech trai/phai CHI bang am luong (memory 4c), do rong nho vang hai kenh khac xung.
# DOI THAT: moi con dap canh 9-16 lan / giay (canh da: "phach" on dai 300-2500 Hz tat 10-25 ms, nua xuong to hon nua len), MOI CON MOT VI TRI + nhip rieng;
#   ca bay bay qua: vi tri tung con truot trai -> phai (lech nhau), to theo 1 / khoang cach + SANG dan khi gan (loc thap quet), them tieng gio vut va tieng
#   chit chit nho (2,4-3,8 kHz, ngan) tung luc.
#
# Ba MAU:
#   A "xa_nghia_dia"  - 1-2 con qua XA, vang lon; bay doi vua (16-24 con) bay qua cao.
#   B "tham_thiet"    - 1 con qua GAN hon, tieng keo dai rat tham thiet, giong vo gay; bay doi 24-34 con, chit chit ro.
#   C "bay_qua"       - 2-3 con qua cai nhau (chong len, hai ben), cuoi co tieng vo canh bay di; bay doi day dac 36-50 con + gio vut manh.
#
# Chay: python CongCu/AmThanh/tieng_qua_doi.py [thu_muc_ra] [loc ten mau...]   (mac dinh CongCu/AmThanh/QuaDoiThu)

import os, sys
import numpy as np
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import tieng_bam as tb
import tieng_qua_cau_lua_3 as q3
import tieng_tang_hinh as th
import tieng_giat_set_3 as g3
import tieng_nhao_lon as nl

SR = tb.SR
PI2 = tb.PI2
loc, dai, thap, cao, cham = q3.loc, q3.dai, q3.thap, q3.cao, q3.cham
on2, loc2, quet2, keo_lech, bao_len_xuong = th.on2, th.loc2, th.quet2, th.keo_lech, th.bao_len_xuong


# ------------------------------------------------------------------ QUA
def qua_mot_tieng(r, dai_s, f0, len_=0.25, xuong=0.35, vo=0.4, khan=0.5):
    """Mot tieng "QUAAA" mono: xung rang cua theo tung CHU KY (jitter, shimmer, duoi hoa am) qua 3 formant + hoi on."""
    n = int(dai_s * SR)
    t = np.arange(n) / SR
    u = t / dai_s
    # duong cao do: vong len nhanh (0-20%) roi sut xuong dai; cuoi gay xuong (vo giong)
    duong = f0 * (1 + len_ * np.sin(np.pi * np.clip(u / 0.4, 0, 1)) ** 1.5 - xuong * np.clip((u - 0.35) / 0.65, 0, 1) ** 1.3)
    duong *= 1 + 0.03 * cham(n, 9, r)
    x = np.zeros(n)
    i = 0
    dem = 0
    while i < n:
        f = max(120.0, duong[i])
        T = (1.0 / f) * (1 + r.normal(0, 0.06 * khan + 0.02))       # jitter tung chu ky
        if r.random() < vo * u[i] * 0.6: T *= 2.0                      # duoi hoa am (chu ky doi) - vo giong, nhieu dan ve cuoi
        d = max(8, int(T * SR))
        a = 1 + r.normal(0, 0.12 * khan + 0.04)                        # shimmer
        k = np.arange(min(d, n - i))
        # xung rang cua + doc xuong (nguon thanh mon, giau hoa am)
        x[i:i + len(k)] += a * (1 - 2 * k / d) * np.exp(-k / (d * 0.6))
        i += d; dem += 1
    # 3 formant (mo mieng "a" khan cua qua), lech nhe theo bien the
    # (do lan 1: 200 Hz-1 kHz chi 22-35% - mong; formant 1 ha 900-1250 + nang x1,4, them than 500-700 Hz)
    F = [r.uniform(900, 1250), r.uniform(1700, 2300), r.uniform(2800, 3300)]
    y = (loc(x, dai(F[0], 2.5)) * 1.4 + loc(x, dai(F[1], 4.0)) * 0.7 + loc(x, dai(F[2], 5.0)) * 0.3 + loc(x, dai(r.uniform(500, 700), 2.0)) * 0.6)
    y += loc(r.standard_normal(n), lambda ff: dai(1600, 1.2)(ff)) * 0.06 * khan * np.max(np.abs(y))      # hoi on khan
    y = np.tanh(2.2 * y / (np.max(np.abs(y)) + 1e-9))
    bao = np.clip(t / 0.025, 0, 1) * np.clip((dai_s - t) / 0.08, 0, 1) * (1 - 0.35 * u)
    return y * bao


def dot_qua(r, so, f0s, gian, p, xa, dai_t, tham=1.0, tron=None, bd0=0.0, n_tong=None):
    """Mot dot keu cua MOT con qua o vi tri p (-1..1, lech chi bang am luong), xa 0..1 (xa: nho + loc thap)."""
    bd = bd0
    for j in range(so):
        d = r.uniform(*dai_t)
        f0 = r.uniform(*f0s) * (1 - 0.04 * j)
        x = qua_mot_tieng(r, d, f0, r.uniform(0.15, 0.35) * tham, r.uniform(0.25, 0.45) * tham, r.uniform(0.25, 0.6) * tham, r.uniform(0.4, 0.8))
        x = loc(x, thap(4200 - 2600 * xa, 2)) * (1 - 0.55 * xa) * r.uniform(0.75, 1.0)
        i0 = int(bd * SR)
        m = min(len(x), len(tron) - i0)
        if m <= 0: break
        g = (np.clip(p + r.uniform(-0.08, 0.08), -1, 1) + 1) * np.pi / 4
        tron[i0:i0 + m, 0] += x[:m] * np.cos(g) * np.sqrt(2)
        tron[i0:i0 + m, 1] += x[:m] * np.sin(g) * np.sqrt(2)
        bd += d + r.uniform(*gian)
    return bd


def canh_vo(tron, r, bd, p, so=6, nang=0.6):
    """Qua vo canh bay di: vai nhip "phap phap" (canh long chim, tram hon doi) roi nho dan."""
    t = np.arange(len(tron)) / SR
    for j in range(so):
        b = bd + j * r.uniform(0.11, 0.16)
        x = nl.ao(len(tron), r, bao_len_xuong(t, 0.008, r.uniform(0.03, 0.05), b), (250, 2000), 0.5, 40.0, 0.7)
        tron += keo_lech(x, np.clip(p + j * 0.08, -1, 1)) * nang * (1 - j / (so + 2))


CAU_HINH = {
    'xa_nghia_dia': dict(so_qua=(1, 3), so_tieng=(3, 6), xa=(0.55, 0.8), tham=0.9, dai=(0.4, 0.7), f0=(420, 620), vang=(1.8, 2.4), muc_vang=(0.42, 0.55),
                          doi=(16, 25), chit=0.35, gio=0.5),
    'tham_thiet':   dict(so_qua=(1, 2), so_tieng=(2, 5), xa=(0.2, 0.4), tham=1.35, dai=(0.6, 0.95), f0=(380, 560), vang=(1.6, 2.2), muc_vang=(0.45, 0.58),
                          doi=(24, 35), chit=0.7, gio=0.6),
    'bay_qua':      dict(so_qua=(2, 4), so_tieng=(3, 6), xa=(0.3, 0.6), tham=1.0, dai=(0.35, 0.6), f0=(450, 750), vang=(1.4, 2.0), muc_vang=(0.32, 0.45),
                          doi=(36, 51), chit=0.5, gio=1.0),
}


def qua(hat, kieu):
    """Tieng qua (huong ngau nhien: phan lon lech HAN mot ben - ban lat duoc tao rieng)."""
    c = CAU_HINH[kieu]; r = np.random.default_rng(hat)
    n = int(SR * 6.0)
    tron = np.zeros((n, 2))
    so_qua = int(r.integers(*c['so_qua']))
    ben = r.uniform(0.35, 0.85)                 # con chinh o ben PHAI (ban lat = ben trai)
    het = 0.0
    for q in range(so_qua):
        p = ben if q == 0 else -r.uniform(0.2, 0.8) * (1 if r.random() < 0.7 else -1)
        bd0 = 0.05 if q == 0 else r.uniform(0.3, 1.5)
        het = max(het, dot_qua(r, int(r.integers(*c['so_tieng'])), c['f0'], (0.22, 0.6), p, r.uniform(*c['xa']), c['dai'], c['tham'], tron, bd0))
    if kieu == 'bay_qua' or r.random() < 0.4:
        canh_vo(tron, r, min(het + r.uniform(0.0, 0.3), 4.5), ben, int(r.integers(4, 8)), 0.55 if kieu == 'bay_qua' else 0.35)
    tron = tron[:int(SR * min(6.0, het + 1.0))]
    return th.hoan_tat(tron, r, r.uniform(*c['vang']), r.uniform(*c['muc_vang']), -3)


# ------------------------------------------------------------------ DOI
PHACH = None


def nhan_phach(r):
    """6 mau 'phach' canh da: on dai trung tat 10-25 ms (nua xuong cua nhip canh)."""
    ds = []
    for _ in range(6):
        d = int(r.uniform(0.012, 0.028) * SR)
        e = np.exp(-np.arange(d) / (d / 3.5)) * np.minimum(1, np.arange(d) / (0.0015 * SR))
        fc = r.uniform(500, 1600)
        k = loc(np.pad(r.standard_normal(d) * e, (0, 256)), lambda ff: dai(fc, 1.2)(ff) * cao(280, 2)(ff))
        ds.append(k / (np.max(np.abs(k)) + 1e-12))
    return ds


def doi(hat, kieu, chieu=1):
    """Bay doi bay VUT NGANG: chieu 1 = trai -> phai, -1 = phai -> trai."""
    c = CAU_HINH[kieu]; r = np.random.default_rng(hat)
    tong = r.uniform(3.4, 4.6)
    n = int(SR * (tong + 0.6)); t = np.arange(n) / SR
    tron = np.zeros((n, 2))
    nhan = nhan_phach(r)
    so = int(r.integers(*c['doi']))
    giua = tong * r.uniform(0.45, 0.55)          # luc bay ngang ngay tren dau
    for b in range(so):
        lech_t = r.normal(0, 0.35)                # tung con qua dau som / muon
        lech_ngang = r.uniform(-0.25, 0.25)
        cao_m = r.uniform(2.0, 6.0)               # do cao tren dau (m)
        toc = r.uniform(7, 11)                    # m/s
        hz = r.uniform(9, 16)
        pha = r.uniform(0, 1)
        thoi = np.arange(pha / hz, tong, 1 / hz)
        for tk in thoi:
            tk2 = tk + r.normal(0, 0.004)
            x_m = (tk2 - giua - lech_t) * toc        # vi tri doc duong bay (m), 0 = ngay tren dau
            kc = np.sqrt(x_m ** 2 + cao_m ** 2)
            a = (1.0 / max(kc, 1.5)) * r.uniform(0.6, 1.0)
            if a < 0.02: continue
            k = nhan[r.integers(0, 6)]
            sang = np.clip(1.6 / kc, 0.25, 1.0)
            if sang < 0.95:
                k = loc(k, thap(900 + 3200 * sang, 2))
            p = np.clip(chieu * (x_m / np.sqrt(x_m ** 2 + 4.0)) + lech_ngang, -1, 1)    # truot ben theo duong bay
            i0 = int(tk2 * SR)
            if i0 < 0 or i0 >= n: continue
            m = min(len(k), n - i0)
            g = (p + 1) * np.pi / 4
            tron[i0:i0 + m, 0] += k[:m] * a * np.cos(g) * np.sqrt(2)
            tron[i0:i0 + m, 1] += k[:m] * a * np.sin(g) * np.sqrt(2)
            # chit chit (rat it, ngan)
            if r.random() < 0.006 * c['chit']:
                d = int(r.uniform(0.012, 0.03) * SR); tt = np.arange(d) / SR
                f = r.uniform(2400, 3800) * (1 - 0.15 * tt / tt[-1])
                ch = np.sin(PI2 * np.cumsum(f) / SR) * np.sin(np.pi * tt / tt[-1]) ** 2 * a * 0.8
                mm = min(d, n - i0)
                tron[i0:i0 + mm, 0] += ch[:mm] * np.cos(g) * np.sqrt(2)
                tron[i0:i0 + mm, 1] += ch[:mm] * np.sin(g) * np.sqrt(2)
    # gio vut ca bay: to + sang nhat luc qua dau, truot trai -> phai
    bao = np.exp(-((t - giua) / 0.7) ** 2)
    x = nl.gio_than(n, r, bao, 350, 2200) * c['gio'] * 0.35 * np.max(np.abs(tron))
    tron += keo_lech(x, np.clip(chieu * (t - giua) / 1.2, -1, 1))
    return th.hoan_tat(tron, r, r.uniform(0.8, 1.3), r.uniform(0.2, 0.3), -3)


MAU = {'A_xa_nghia_dia': ('xa_nghia_dia', 24100), 'B_tham_thiet': ('tham_thiet', 24300), 'C_bay_qua': ('bay_qua', 24500)}


def lat(x):
    return x[:, ::-1].copy()


def ghep_game(quas, dois):
    """MO PHONG TRONG GAME (thu nho thoi gian: qua o giay 0,5, doi o giay 6 thay vi 60 / 90): qua (ben phai) -> doi trai -> phai -> qua (ban lat,
    ben trai) -> doi phai -> trai."""
    out = np.zeros((int(SR * 22.0), 2))
    tb.cong(out, quas[0], 0.5)
    tb.cong(out, dois[0], 6.0)
    tb.cong(out, lat(quas[1]), 11.5)
    tb.cong(out, lat(dois[1]), 16.5)
    return out / np.max(np.abs(out)) * 0.95


if __name__ == '__main__':
    ra = sys.argv[1] if len(sys.argv) > 1 else os.path.join('CongCu', 'AmThanh', 'QuaDoiThu')
    os.makedirs(ra, exist_ok=True)
    chi = sys.argv[2:]
    for ten, (kieu, hat0) in MAU.items():
        if chi and not any(c in ten for c in chi): continue
        bo = {'qua': [qua(hat0 + 10 * i, kieu) for i in range(3)], 'doi': [doi(hat0 + 100 + 10 * i, kieu) for i in range(3)]}
        for loai, ds in bo.items():
            for i, x in enumerate(ds):
                tb.ghi(os.path.join(ra, '%s_%s%d.wav' % (ten, loai, i + 1)), x)
                th.do('%s_%s%d' % (ten, loai, i + 1), x)
                # huong: nang luong trai / phai theo thoi gian (doi phai truot trai -> phai)
                L = np.convolve(x[:, 0] ** 2, np.ones(4410), 'same'); R = np.convolve(x[:, 1] ** 2, np.ones(4410), 'same')
                pos = (R - L) / (R + L + 1e-12)
                tot = L + R
                mk = tot > np.max(tot) * 0.2
                idx = np.nonzero(mk)[0]
                print('      lech (R-L)/(R+L): dau %.2f giua %.2f cuoi %.2f' % (pos[idx[0] + 2205], pos[idx[len(idx) // 2]], pos[idx[-1] - 2205]))
            print('   chenh bien the %s: %s dB' % (loai, ' / '.join('%.1f' % c for c in th.chenh_bien_the(ds))))
        g = ghep_game(bo['qua'], bo['doi'])
        tb.ghi(os.path.join(ra, '%s_ghep_nhu_trong_game.wav' % ten), g)
        th.do('%s_ghep' % ten, g)
