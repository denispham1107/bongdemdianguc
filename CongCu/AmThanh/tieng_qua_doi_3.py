# -*- coding: utf-8 -*-
# TIENG QUA + BAY DOI - BAN 3 (17/10/2026). Ban 2 (tieng_qua_doi_2.py) nguoi dung: "tieng qua keu DA DAT nhung hoi ngan, cho dai them; tieng doi vo
# canh CHUA DAT, nghe nhu tieng SUNG BAN LIEN THANH". Nguyen nhan doi ban 2: moi cu vo canh len GAT 3 ms + "thup" tram 250-900 Hz, nhip deu 9-14 / giay
# -> chuoi xung nhip deu = sung lien thanh.
#
# QUA: giu NGUYEN mo hinh ban 2 (tieng_qua_doi_2.tieng_qua / chuoi_qua), moi tieng DAI THEM x1,35 va them 1-2 tieng moi dot.
# DOI (ban 3): bay doi that = MAY tieng PHANH PHACH SOT SOAT lien tuc, khong co cu nao noi bat:
#   - moi cu vo canh len MEM 12-20 ms, tat 40-70 ms, HAI NHIP (dap xuong to + nhac len nhe 45%), ben trong la MANG DA RUNG (on dai 450-2600 Hz nhan
#     bien do nhanh 60-120 Hz ngau nhien = tieng soat soat), than "phup" tram chi con 12%;
#   - nhip moi con 6-12 / giay, LECH ngau nhien +-15% moi cu (khong deu), bien do moi cu +-40%;
#   - con o XA tron thanh LOP SOT SOAT lien tuc (on rong dieu bien bang tong nhip nhieu con), con GAN moi nghe ra tung cu;
#   - chit chit 3-4,4 kHz rat it; gio lua hai kenh doc lap truot theo bay.
# DO "SUNG": duong bao 5 ms -> (1) do GAT = P99 / trung vi; (2) NHIP DEU = dinh tu tuong quan duong bao o tre 60-170 ms. So voi ban 2 cung cach do.
#
# Chay: python CongCu/AmThanh/tieng_qua_doi_3.py [thu_muc_ra] [loc ten mau...]   (mac dinh CongCu/AmThanh/QuaDoiThu3)

import os, sys
import numpy as np
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import tieng_bam as tb
import tieng_qua_cau_lua_3 as q3
import tieng_tang_hinh as th
import tieng_giat_set_3 as g3
import tieng_qua_doi_2 as v2

SR = tb.SR
PI2 = tb.PI2
loc, dai, thap, cao, cham = q3.loc, q3.dai, q3.thap, q3.cao, q3.cham
on2, loc2, keo_lech, bao_len_xuong = th.on2, th.loc2, th.keo_lech, th.bao_len_xuong


# ================================================================== QUA (ban 2, dai them)
def qua(hat, kieu):
    c = dict(v2.CAU_HINH[kieu])
    c['d'] = (c['d'][0] * 1.35, c['d'][1] * 1.35)
    c['so'] = (c['so'][0] + 1, c['so'][1] + 2)
    r = np.random.default_rng(hat)
    n = int(SR * 10.0)
    tron = np.zeros((n, 2))
    ben = r.uniform(0.4, 0.85)
    f0 = r.uniform(*c['f0'])
    het = v2.chuoi_qua(tron, r, 0.05, ben, r.uniform(*c['xa']), int(r.integers(*c['so'])), f0, c['d'], c)
    if c['dap_lai'] and r.random() < 0.75:
        het = max(het, v2.chuoi_qua(tron, r, het + r.uniform(0.2, 0.6), -r.uniform(0.3, 0.8), r.uniform(0.6, 0.85), int(r.integers(2, 4)),
                                    f0 * r.uniform(0.85, 1.1), (c['d'][0] * 0.8, c['d'][1] * 0.8), c))
    so_them = int(r.integers(c['them_qua'][0], c['them_qua'][1] + 1)) if c['them_qua'][1] > 0 else 0
    for q in range(so_them):
        het = max(het, v2.chuoi_qua(tron, r, r.uniform(0.15, 2.0), r.uniform(-0.9, 0.9), r.uniform(0.3, 0.7), int(r.integers(3, 6)),
                                    r.uniform(*c['f0']), c['d'], c))
    if kieu == 'bay_qua_hoang_loan' or r.random() < 0.35:
        v2.vo_canh_bay_di(tron, r, min(het - r.uniform(0.0, 0.4), 8.5), ben, int(r.integers(5, 9)), 0.7 if kieu == 'bay_qua_hoang_loan' else 0.4)
    tron = tron[:int(SR * min(10.0, het + 0.8))]
    return th.hoan_tat(tron, r, r.uniform(*c['vang']), r.uniform(*c['muc_vang']), -3)


# ================================================================== DOI (ban 3)
def kernel_canh(r, so=10):
    """Mot chu ky vo canh: DAP XUONG + NHAC LEN, len mem, ben trong mang da rung soat soat."""
    ds = []
    for _ in range(so):
        d = int(r.uniform(0.07, 0.11) * SR)
        tt = np.arange(d) / SR
        len_ = r.uniform(0.012, 0.02)
        tat = r.uniform(0.04, 0.07)
        xuong = np.clip(tt / len_, 0, 1) ** 2 * np.exp(-np.maximum(tt - len_, 0) / tat * 2.2)
        bd2 = r.uniform(0.045, 0.065)
        len2 = np.clip((tt - bd2) / (len_ * 1.2), 0, 1) ** 2 * np.exp(-np.maximum(tt - bd2 - len_ * 1.2, 0) / (tat * 0.35)) * 0.45
        bao = xuong + len2
        # mang da rung: on dai trung x dieu bien nhanh ngau nhien 60-120 Hz
        f_rung = r.uniform(60, 120)
        rung = 0.55 + 0.45 * np.abs(np.sin(PI2 * f_rung * tt + r.uniform(0, PI2))) * (1 + 0.3 * r.standard_normal(d)).clip(0)
        mang = loc(r.standard_normal(d), lambda ff: cao(450, 2)(ff) * thap(r.uniform(1800, 2600), 3)(ff) * (0.7 + 0.5 * dai(1100, 1.0)(ff)))
        mang = mang / (np.max(np.abs(mang)) + 1e-9) * rung
        than = loc(r.standard_normal(d), lambda ff: cao(250, 2)(ff) * thap(700, 2)(ff))
        than = than / (np.max(np.abs(than)) + 1e-9) * 0.12
        x = (mang + than) * bao
        ds.append(x / (np.max(np.abs(x)) + 1e-9))
    return ds


CAU_DOI = {
    'qua_den_tham_thiet': dict(doi=(35, 50), cao=(1.5, 5.0), lua=0.9, chit=0.4, may=0.55),
    'qua_gia_tram_khan':  dict(doi=(50, 70), cao=(2.0, 6.0), lua=1.2, chit=0.25, may=0.75),
    'bay_qua_hoang_loan': dict(doi=(30, 45), cao=(1.2, 3.5), lua=0.8, chit=0.9, may=0.5),
}


def doi(hat, kieu, chieu=1):
    c = CAU_DOI[kieu]; r = np.random.default_rng(hat)
    tong = r.uniform(3.8, 5.0)
    n = int(SR * (tong + 0.5)); t = np.arange(n) / SR
    L = np.zeros(n); R = np.zeros(n)
    ker = kernel_canh(r)
    so = int(r.integers(*c['doi']))
    giua = tong * r.uniform(0.45, 0.55)
    # LOP MAY SOT SOAT: tong dieu bien cua nhieu con XA (khong nghe ra tung cu) - hai kenh on doc lap, truot theo bay
    am = np.zeros(n)
    for _ in range(24):
        hz = r.uniform(6, 12)
        pha = PI2 * np.cumsum(hz * (1 + 0.12 * cham(n, 2, r))) / SR + r.uniform(0, PI2)
        am += np.clip(np.sin(pha), 0, None) ** 2
    am /= np.max(am)
    may = loc2(on2(n, r, 0.3), lambda ff: cao(400, 2)(ff) * thap(2400, 3)(ff))
    may = may / (np.max(np.abs(may)) + 1e-9) * (0.35 + 0.65 * am)[:, None]
    bao_may = np.exp(-((t - giua) / 1.1) ** 2)
    may = keo_lech(may * bao_may[:, None], np.clip(chieu * (t - giua) / 1.4, -1, 1) * 0.85)
    # tung con (gan moi nghe ro tung cu)
    for b in range(so):
        lech_t = r.normal(0, 0.45)
        sau = r.uniform(-3.5, 3.5)
        hm = r.uniform(*c['cao'])
        toc = r.uniform(6, 10)
        hz = r.uniform(6, 12)
        d_vuong = np.sqrt(sau ** 2 + hm ** 2)
        tk = [r.uniform(0, 1 / hz)]
        while tk[-1] < tong: tk.append(tk[-1] + (1 / hz) * r.uniform(0.85, 1.15))
        tk = np.array(tk[:-1])
        xm = chieu * (tk - giua - lech_t) * toc
        kc = np.sqrt(xm ** 2 + d_vuong ** 2)
        a = np.minimum(1.0, 1.8 / kc) * r.uniform(0.5, 1.0)
        sang = np.clip(2.5 / kc, 0.2, 1)
        pp = np.clip(xm / np.sqrt(xm ** 2 + d_vuong ** 2 * 0.6), -1, 1)
        for i, tki in enumerate(tk):
            ai = a[i] * r.uniform(0.6, 1.4)
            if ai < 0.03: continue
            sig = ker[r.integers(0, len(ker))]
            if sang[i] < 0.9: sig = loc(sig, thap(800 + 2600 * sang[i], 2))
            i0 = int(tki * SR)
            if i0 < 0 or i0 >= n: continue
            mm = min(len(sig), n - i0)
            gg = (pp[i] + 1) * np.pi / 4
            L[i0:i0 + mm] += sig[:mm] * ai * np.cos(gg) * np.sqrt(2)
            R[i0:i0 + mm] += sig[:mm] * ai * np.sin(gg) * np.sqrt(2)
            if r.random() < 0.004 * c['chit']:
                dd = int(r.uniform(0.01, 0.025) * SR); tt = np.arange(dd) / SR
                f = r.uniform(3000, 4400) * (1 - 0.18 * tt / tt[-1])
                ch = np.sin(PI2 * np.cumsum(f) / SR) * np.sin(np.pi * tt / tt[-1]) ** 2 * ai * 0.3
                m2 = min(dd, n - i0)
                L[i0:i0 + m2] += ch[:m2] * np.cos(gg) * np.sqrt(2); R[i0:i0 + m2] += ch[:m2] * np.sin(gg) * np.sqrt(2)
    tron = np.stack([L, R], axis=1)
    tron += may * c['may'] * np.max(np.abs(tron))
    # gio lua ca bay
    bao = np.exp(-((t - giua) / 0.9) ** 2)
    x = loc2(on2(n, r, 0.25), lambda ff: cao(200, 2)(ff) * thap(1500, 2)(ff)) * (1 + 0.5 * cham(n, 9, r))[:, None]
    x = x / (np.max(np.abs(x)) + 1e-9) * bao[:, None]
    tron += keo_lech(x, np.clip(chieu * (t - giua) / 1.3, -1, 1) * 0.8) * c['lua'] * 0.25 * np.max(np.abs(tron))
    return th.hoan_tat(tron, r, r.uniform(0.9, 1.4), r.uniform(0.22, 0.32), -3)


def do_sung(x):
    """(do gat P99 / trung vi duong bao 5 ms trong doan to, dinh tu tuong quan duong bao o tre 60-170 ms) - sung lien thanh: gat cao + nhip deu."""
    m = x.mean(axis=1)
    e = np.sqrt(np.convolve(m ** 2, np.ones(220) / 220, 'same'))[::110]       # 5 ms, buoc 2,5 ms
    mk = e > np.max(e) * 0.25
    ee = e[mk]
    gat = np.percentile(ee, 99) / (np.median(ee) + 1e-12)
    s = e[np.nonzero(mk)[0][0]:np.nonzero(mk)[0][-1]]
    s = s - s.mean()
    ac = np.correlate(s, s, 'full')[len(s) - 1:]
    ac /= ac[0] + 1e-12
    lo, hi = int(0.06 / 0.0025), int(0.17 / 0.0025)
    return gat, float(np.max(ac[lo:hi]))


MAU = {'A_qua_den_tham_thiet': ('qua_den_tham_thiet', 26100), 'B_qua_gia_tram_khan': ('qua_gia_tram_khan', 26300),
       'C_bay_qua_hoang_loan': ('bay_qua_hoang_loan', 26500)}


def ghep_game(quas, dois):
    out = np.zeros((int(SR * 28.0), 2))
    tb.cong(out, quas[0], 0.5)
    tb.cong(out, dois[0], 9.0)
    tb.cong(out, v2.lat(quas[1]), 14.5)
    tb.cong(out, v2.lat(dois[1]), 22.5)
    return out / np.max(np.abs(out)) * 0.95


if __name__ == '__main__':
    ra = sys.argv[1] if len(sys.argv) > 1 else os.path.join('CongCu', 'AmThanh', 'QuaDoiThu3')
    os.makedirs(ra, exist_ok=True)
    chi = sys.argv[2:]
    # doi chung: do "sung" cua doi ban 2
    for kieu, hat in [('qua_den_tham_thiet', 25200), ('qua_gia_tram_khan', 25400)]:
        g, a = do_sung(v2.doi(hat, kieu))
        print('DOI CHUNG doi ban 2 (%s): do gat %.2f, nhip deu %.2f' % (kieu, g, a))
    for ten, (kieu, hat0) in MAU.items():
        if chi and not any(c in ten for c in chi): continue
        bo = {'qua': [qua(hat0 + 10 * i, kieu) for i in range(3)], 'doi': [doi(hat0 + 100 + 10 * i, kieu) for i in range(3)]}
        for loai, ds in bo.items():
            for i, x in enumerate(ds):
                tb.ghi(os.path.join(ra, '%s_%s%d.wav' % (ten, loai, i + 1)), x)
                th.do('%s_%s%d' % (ten, loai, i + 1), x)
                if loai == 'doi':
                    g, a = do_sung(x)
                    Lr = np.convolve(x[:, 0] ** 2, np.ones(4410), 'same'); Rr = np.convolve(x[:, 1] ** 2, np.ones(4410), 'same')
                    pos = (Rr - Lr) / (Rr + Lr + 1e-12); tot = Lr + Rr; idx = np.nonzero(tot > np.max(tot) * 0.2)[0]
                    print('      do gat %.2f, nhip deu %.2f; lech dau %.2f giua %.2f cuoi %.2f' % (g, a, pos[idx[0] + 2205], pos[idx[len(idx) // 2]], pos[idx[-1] - 2205]))
            print('   chenh bien the %s: %s dB' % (loai, ' / '.join('%.1f' % c for c in th.chenh_bien_the(ds))))
        g = ghep_game(bo['qua'], bo['doi'])
        tb.ghi(os.path.join(ra, '%s_ghep_nhu_trong_game.wav' % ten), g)
        th.do('%s_ghep' % ten, g)
