# -*- coding: utf-8 -*-
# TIENG QUA + BAY DOI - BAN 2 (17/10/2026). Ban 1 (tieng_qua_doi.py) nguoi dung che: "chua giong tieng qua keu, chua giong tieng doi vo canh, qua mong
# va don dieu, van giong mono". Ly do do / doc lai ban 1:
#   - qua = CHUOI XUNG rang cua qua 3 dai -> "coi dien tu", khong co am bat "k", khong vo giong, khong khan hoi;
#   - doi = moi cu dap canh mot "tach" on 12-28 ms 500-1 600 Hz -> lach tach, khong co luong GIO bi day (than 250-900 Hz) cua canh;
#   - khong gian = mot cho + MOT lop vang chung -> nghe nhu mot diem.
#
# QUA (ban 2): chuoi HOA AM (a_k ~ 1/k^0,75 toi 4,5 kHz) theo duong cao do (len nhe roi sut, rung FM 2-4% ngau nhien) + HOA AM PHU (k + 1/2: vo giong /
#   chu ky doi, lon dan ve cuoi) + DOAN HON LOAN ngan (cao do nhay) + on KHAN loc qua CUNG cac formant; mo dau am bat "K" (on 2-3,5 kHz 6 ms) + F2 TRUOT
#   tu cao xuong ("k-aaa"); cuoi tieng GIONG KHAN DUT QUANG (vocal fry: xung thua 50-110 / giay) - "tham thiet". Formant F1 750-1050 / F2 1450-1900 /
#   F3 2400-2900 + cong huong mui 500-650. Dat KHONG GIAN: tia truc tiep lech CHI bang am luong (am co cao do - memory 4c) + 8-14 PHAN XA SOM (bia,
#   cay: tre 8-90 ms, moi phan xa mot ben / mot do loc) + vang dai stereo.
# DOI (ban 2): moi con MOT DUONG BAY 3D (ngang qua nguoi nghe, lech ngang -3..3 m, cao 1,5-5 m, 6-11 m/s), canh vo 9-14 lan / giay; moi cu = "PHUP"
#   gio bi day (on loc 250-900 Hz, len 3 ms tat 25-50 ms) + MANG CANH BAT (1,2-3 kHz, 4-8 ms, 25-40%); to theo 1 / khoang cach that, CANG GAN CANG
#   SANG (lop cao nhan do gan); ca bay: lop GIO LUA hai kenh on DOC LAP truot theo ca bay + tieng chit chit 3-4,4 kHz thua.
#
# Ba MAU:
#   A "qua_den_tham_thiet" - 1 con qua den keu tham thiet keo dai, vo giong cuoi tieng, mot con khac dap lai xa; bay doi 30-45 con.
#   B "qua_gia_tram_khan"  - qua gia (raven) tram khan "krraa", it tieng hon nhung nang; bay doi 45-65 con day dac + gio lua lon.
#   C "bay_qua_hoang_loan" - 3-4 con qua keu chen nhau hai ben + vo canh bay toan; bay doi 25-40 con bay thap, chit chit ro.
#
# Chay: python CongCu/AmThanh/tieng_qua_doi_2.py [thu_muc_ra] [loc ten mau...]   (mac dinh CongCu/AmThanh/QuaDoiThu2)

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
on2, loc2, keo_lech, bao_len_xuong = th.on2, th.loc2, th.keo_lech, th.bao_len_xuong


# ================================================================== QUA
def tieng_qua(r, d, f0, len_=0.05, xuong=0.28, vo=0.4, khan=0.35, fry=0.15, formant=(900, 1700, 2650), mui=580, phu_cuoi=1.0):
    """MOT tieng qua (mono). d giay, f0 Hz. vo: muc hoa am phu (vo giong); khan: on khan; fry: phan cuoi giong khan dut quang."""
    n = int(d * SR)
    t = np.arange(n) / SR
    u = t / d
    # duong cao do: len nhe 15% dau, giu, sut ve cuoi; rung FM ngau nhien
    duong = f0 * (1 + len_ * np.clip(u / 0.15, 0, 1) - xuong * np.clip((u - 0.3) / 0.7, 0, 1) ** 1.4)
    duong *= 1 + 0.025 * cham(n, 14, r) + 0.012 * cham(n, 45, r)
    # doan hon loan ngan (cao do nhay) - 0-2 doan
    for _ in range(int(r.integers(0, 3))):
        a = r.uniform(0.25, 0.8); b = a + r.uniform(0.04, 0.1)
        mk = (u > a) & (u < b)
        duong[mk] *= 1 + 0.12 * cham(int(mk.sum()) + 8, 120, r)[:mk.sum()]
    pha = PI2 * np.cumsum(duong) / SR
    src = np.zeros(n)
    kmax = int(4600 / max(f0 * 0.7, 100))
    for k in range(1, kmax + 1):
        src += np.sin(k * pha + r.uniform(0, PI2)) / k ** 0.75 * (np.minimum(1, 4600 / (k * duong)) )
    # hoa am PHU (k + 1/2): vo giong, lon dan
    m_vo = vo * (0.3 + 0.7 * np.clip(u / 0.8, 0, 1)) * (0.6 + 0.4 * np.abs(cham(n, 8, r)))
    for k in range(0, kmax):
        src += m_vo * np.sin((k + 0.5) * pha + r.uniform(0, PI2)) / (k + 1) ** 0.85 * 0.8
    # GIONG KHAN cuoi tieng (vocal fry): bien do xung thua 50-110 / giay
    if fry > 0:
        tf = np.clip((u - (1 - fry)) / fry, 0, 1)
        xung = np.zeros(n)
        i = int((1 - fry) * n)
        while i < n:
            xung[i] = 1.0
            i += int(SR / r.uniform(50, 110))
        fryb = np.convolve(xung, np.exp(-np.arange(int(0.006 * SR)) / (0.0015 * SR)), 'same')
        src *= (1 - tf) + tf * (0.25 + 1.6 * fryb)
    # on KHAN qua cung formant
    on = r.standard_normal(n) * np.std(src) * khan * (1 + 0.6 * np.abs(cham(n, 30, r)))
    nguon = src + on
    # formant + F2 TRUOT tu cao xuong o 40 ms dau ("k-aaa"): hai lop F2 (dau / sau) tron theo thoi gian
    F1, F2, F3 = formant
    y = loc(nguon, dai(F1 * r.uniform(0.93, 1.07), 2.2)) * 1.25
    f2a = loc(nguon, dai(F2 * 1.35, 3.0)); f2b = loc(nguon, dai(F2 * r.uniform(0.94, 1.06), 3.0))
    tr = np.clip(t / 0.045, 0, 1)
    y += (f2a * (1 - tr) + f2b * tr) * 0.8
    y += loc(nguon, dai(F3 * r.uniform(0.95, 1.05), 4.0)) * 0.35
    y += loc(nguon, dai(mui, 2.5)) * 0.45
    y = np.tanh(1.8 * y / (np.max(np.abs(y)) + 1e-9))
    # am bat "K"
    kn = int(0.007 * SR)
    k = loc(np.pad(r.standard_normal(kn) * np.exp(-np.arange(kn) / (kn / 3)), (0, 256)), lambda ff: dai(2700, 1.5)(ff))[:kn]
    y[:kn] += k / (np.max(np.abs(k)) + 1e-9) * 0.55
    bao = np.clip(t / 0.012, 0, 1) * np.clip((d - t) / 0.05, 0, 1) * (1 - 0.3 * u * phu_cuoi)
    return y * bao


def dat_khong_gian(tron, r, x, bd, p, xa, so_px=(8, 15)):
    """Dat tieng mono x vao tron tai bd (giay): tia truc tiep lech CHI bang am luong (p -1..1), xa 0..1 (nho + toi); + PHAN XA SOM (moi cai mot ben,
    tre 8-90 ms, loc thap rieng, nho dan) - dong gop do rong khong lam nguoc pha (tre lon hon nhieu chu ky)."""
    x = loc(x, thap(4500 - 2700 * xa, 2)) * (1 - 0.5 * xa)
    i0 = int(bd * SR)
    def cong(sig, i, pp, a):
        m = min(len(sig), len(tron) - i)
        if m <= 0 or i < 0: return
        g = (np.clip(pp, -1, 1) + 1) * np.pi / 4
        tron[i:i + m, 0] += sig[:m] * a * np.cos(g) * np.sqrt(2)
        tron[i:i + m, 1] += sig[:m] * a * np.sin(g) * np.sqrt(2)
    cong(x, i0, p, 1.0)
    so = int(r.integers(*so_px))
    for j in range(so):
        tre = r.uniform(0.008, 0.09) * (1 + j * 0.15)
        a = r.uniform(0.15, 0.42) * (1 - j / (so + 3)) * (0.6 + 0.6 * xa)
        sig = loc(x, thap(r.uniform(1500, 3500), 2))
        pp = (1 if r.random() < 0.5 else -1) * r.uniform(0.6, 1.0)          # moi phan xa ve mot ben
        cong(sig, i0 + int(tre * SR), pp, a)


def chuoi_qua(tron, r, bd0, p, xa, so_tieng, f0, d_t, cauhinh):
    bd = bd0
    for j in range(so_tieng):
        d = r.uniform(*d_t) * (1 - 0.07 * j)
        x = tieng_qua(r, d, f0 * r.uniform(0.95, 1.05) * (1 - 0.025 * j), r.uniform(0.02, 0.09), r.uniform(*cauhinh['xuong']),
                      r.uniform(*cauhinh['vo']), r.uniform(*cauhinh['khan']), r.uniform(*cauhinh['fry']), cauhinh['formant'], cauhinh['mui'])
        dat_khong_gian(tron, r, x * r.uniform(0.75, 1.0), bd, p + r.uniform(-0.06, 0.06), xa)
        bd += d + r.uniform(*cauhinh['gian'])
    return bd


def vo_canh_bay_di(tron, r, bd, p, so=7, nang=0.6):
    """Qua vo canh bay toan: nhip "PHAP" canh long (gio day 200-900 Hz + long co xat), nho dan, truot ben."""
    t = np.arange(len(tron)) / SR
    for j in range(so):
        b = bd + j * r.uniform(0.1, 0.15)
        bao = bao_len_xuong(t, 0.006, r.uniform(0.04, 0.07), b)
        x = loc2(on2(len(tron), r, 0.5), lambda ff: cao(200, 2)(ff) * thap(900, 2)(ff)) * bao[:, None]
        x += nl.ao(len(tron), r, bao * 0.5, (900, 3200), 0.4, 50.0, 0.8)
        tron += keo_lech(x / (np.max(np.abs(x)) + 1e-9), np.clip(p + j * 0.07, -1, 1)) * nang * (1 - j / (so + 2))


CAU_HINH = {
    'qua_den_tham_thiet': dict(f0=(520, 640), d=(0.55, 0.85), so=(3, 6), xuong=(0.25, 0.4), vo=(0.35, 0.7), khan=(0.3, 0.5), fry=(0.15, 0.3),
                                formant=(950, 1750, 2700), mui=600, gian=(0.28, 0.5), dap_lai=True, them_qua=(0, 1), xa=(0.15, 0.35),
                                vang=(1.8, 2.4), muc_vang=(0.4, 0.5), doi=(30, 46), chit=0.5, lua=0.8, cao=(1.5, 5.0)),
    'qua_gia_tram_khan':  dict(f0=(330, 430), d=(0.4, 0.65), so=(2, 5), xuong=(0.18, 0.3), vo=(0.5, 0.9), khan=(0.45, 0.7), fry=(0.2, 0.35),
                                formant=(780, 1500, 2450), mui=520, gian=(0.35, 0.65), dap_lai=False, them_qua=(0, 1), xa=(0.2, 0.4),
                                vang=(2.0, 2.6), muc_vang=(0.42, 0.52), doi=(45, 66), chit=0.3, lua=1.3, cao=(2.0, 6.0)),
    'bay_qua_hoang_loan': dict(f0=(560, 720), d=(0.28, 0.45), so=(3, 6), xuong=(0.2, 0.35), vo=(0.3, 0.6), khan=(0.3, 0.5), fry=(0.08, 0.18),
                                formant=(1000, 1850, 2800), mui=620, gian=(0.12, 0.35), dap_lai=False, them_qua=(2, 4), xa=(0.25, 0.55),
                                vang=(1.6, 2.2), muc_vang=(0.36, 0.46), doi=(25, 41), chit=1.0, lua=0.9, cao=(1.2, 3.5)),
}


def qua(hat, kieu):
    """Con qua chinh lech PHAI (ban lat = trai). Cau truc ngau nhien: so tieng, dap lai / them qua, vo canh."""
    c = CAU_HINH[kieu]; r = np.random.default_rng(hat)
    n = int(SR * 7.0)
    tron = np.zeros((n, 2))
    ben = r.uniform(0.4, 0.85)
    f0 = r.uniform(*c['f0'])
    het = chuoi_qua(tron, r, 0.05, ben, r.uniform(*c['xa']), int(r.integers(*c['so'])), f0, c['d'], c)
    # con khac dap lai (xa, ben kia)
    if c['dap_lai'] and r.random() < 0.75:
        het = max(het, chuoi_qua(tron, r, het + r.uniform(0.2, 0.6), -r.uniform(0.3, 0.8), r.uniform(0.6, 0.85), int(r.integers(2, 4)),
                                 f0 * r.uniform(0.85, 1.1), (c['d'][0] * 0.8, c['d'][1] * 0.8), c))
    so_them = int(r.integers(c['them_qua'][0], c['them_qua'][1] + 1)) if c['them_qua'][1] > 0 else 0
    for q in range(so_them):
        het = max(het, chuoi_qua(tron, r, r.uniform(0.15, 1.6), r.uniform(-0.9, 0.9), r.uniform(0.3, 0.7), int(r.integers(2, 5)),
                                 r.uniform(*c['f0']), c['d'], c))
    if kieu == 'bay_qua_hoang_loan' or r.random() < 0.35:
        vo_canh_bay_di(tron, r, min(het - r.uniform(0.0, 0.4), 5.5), ben, int(r.integers(5, 9)), 0.7 if kieu == 'bay_qua_hoang_loan' else 0.4)
    tron = tron[:int(SR * min(7.0, het + 0.8))]
    return th.hoan_tat(tron, r, r.uniform(*c['vang']), r.uniform(*c['muc_vang']), -3)


# ================================================================== DOI
def kernel_canh(r, so=8):
    """Mau mot cu vo canh: (lop GIO tram 250-900 Hz, lop MANG CANH bat 1,2-3 kHz) - tach rieng de lop cao nhan do gan."""
    ds = []
    for _ in range(so):
        d = int(r.uniform(0.03, 0.055) * SR)
        tt = np.arange(d)
        e = np.minimum(1, tt / (0.003 * SR)) * np.exp(-tt / (r.uniform(0.012, 0.022) * SR))
        gio = loc(np.pad(r.standard_normal(d) * e, (0, 512)), lambda ff: cao(r.uniform(220, 300), 2)(ff) * thap(r.uniform(700, 1000), 2)(ff))[:d + 256]
        dm = int(r.uniform(0.004, 0.008) * SR)
        em = np.zeros(d + 256); bdm = int(r.uniform(0.0, 0.012) * SR)
        em[bdm:bdm + dm] = np.exp(-np.arange(dm) / (dm / 3))
        mang = loc(r.standard_normal(d + 256) * em, lambda ff: dai(r.uniform(1300, 2800), 1.5)(ff))
        gio /= np.max(np.abs(gio)) + 1e-9; mang /= np.max(np.abs(mang)) + 1e-9
        ds.append((gio, mang * r.uniform(0.25, 0.4)))
    return ds


def doi(hat, kieu, chieu=1):
    """Bay doi bay VUT NGANG qua nguoi nghe: chieu 1 = trai -> phai."""
    c = CAU_HINH[kieu]; r = np.random.default_rng(hat)
    tong = r.uniform(3.6, 4.8)
    n = int(SR * (tong + 0.5)); t = np.arange(n) / SR
    L = np.zeros(n); R = np.zeros(n)
    ker = kernel_canh(r)
    so = int(r.integers(*c['doi']))
    giua = tong * r.uniform(0.45, 0.55)
    for b in range(so):
        lech_t = r.normal(0, 0.4)
        sau = r.uniform(-3.0, 3.0)                   # lech truoc / sau so voi nguoi nghe (m)
        hm = r.uniform(*c['cao'])
        toc = r.uniform(6, 11)
        hz = r.uniform(9, 14)
        d_vuong = np.sqrt(sau ** 2 + hm ** 2)
        tk = np.arange(r.uniform(0, 1 / hz), tong, 1 / hz) + r.normal(0, 0.006, size=1)[0]
        tk = tk + r.normal(0, 0.003, size=len(tk))
        xm = chieu * (tk - giua - lech_t) * toc       # vi tri ngang (m), + = ben phai
        kc = np.sqrt(xm ** 2 + d_vuong ** 2)
        a = np.minimum(1.0, 2.0 / kc) * r.uniform(0.55, 1.0)
        gan = np.clip(2.5 / kc, 0, 1)                # lop mang canh (cao) nhan do gan -> xa thi toi
        pp = np.clip(xm / np.sqrt(xm ** 2 + d_vuong ** 2 * 0.6), -1, 1)
        for i, tki in enumerate(tk):
            if a[i] < 0.03: continue
            g, m = ker[r.integers(0, len(ker))]
            sig = g * (0.85 + 0.3 * r.random()) + m * gan[i]
            i0 = int(tki * SR)
            if i0 < 0 or i0 >= n: continue
            mm = min(len(sig), n - i0)
            gg = (pp[i] + 1) * np.pi / 4
            L[i0:i0 + mm] += sig[:mm] * a[i] * np.cos(gg) * np.sqrt(2)
            R[i0:i0 + mm] += sig[:mm] * a[i] * np.sin(gg) * np.sqrt(2)
            if r.random() < 0.004 * c['chit']:
                dd = int(r.uniform(0.01, 0.025) * SR); tt = np.arange(dd) / SR
                f = r.uniform(3000, 4400) * (1 - 0.18 * tt / tt[-1])
                ch = np.sin(PI2 * np.cumsum(f) / SR) * np.sin(np.pi * tt / tt[-1]) ** 2 * a[i] * 0.35
                m2 = min(dd, n - i0)
                L[i0:i0 + m2] += ch[:m2] * np.cos(gg) * np.sqrt(2); R[i0:i0 + m2] += ch[:m2] * np.sin(gg) * np.sqrt(2)
    tron = np.stack([L, R], axis=1)
    # GIO LUA ca bay: hai kenh on DOC LAP (chung 0,25), bao theo mat do bay gan, truot ben
    bao = np.exp(-((t - giua) / 0.9) ** 2)
    x = loc2(on2(n, r, 0.25), lambda ff: cao(200, 2)(ff) * thap(1600, 2)(ff)) * (1 + 0.5 * cham(n, 11, r))[:, None]
    x = x / (np.max(np.abs(x)) + 1e-9) * bao[:, None]
    tron += keo_lech(x, np.clip(chieu * (t - giua) / 1.3, -1, 1) * 0.8) * c['lua'] * 0.3 * np.max(np.abs(tron))
    return th.hoan_tat(tron, r, r.uniform(0.9, 1.4), r.uniform(0.22, 0.32), -3)


MAU = {'A_qua_den_tham_thiet': ('qua_den_tham_thiet', 25100), 'B_qua_gia_tram_khan': ('qua_gia_tram_khan', 25300),
       'C_bay_qua_hoang_loan': ('bay_qua_hoang_loan', 25500)}


def lat(x):
    return x[:, ::-1].copy()


def ghep_game(quas, dois):
    out = np.zeros((int(SR * 24.0), 2))
    tb.cong(out, quas[0], 0.5)
    tb.cong(out, dois[0], 7.0)
    tb.cong(out, lat(quas[1]), 12.5)
    tb.cong(out, lat(dois[1]), 18.5)
    return out / np.max(np.abs(out)) * 0.95


if __name__ == '__main__':
    ra = sys.argv[1] if len(sys.argv) > 1 else os.path.join('CongCu', 'AmThanh', 'QuaDoiThu2')
    os.makedirs(ra, exist_ok=True)
    chi = sys.argv[2:]
    for ten, (kieu, hat0) in MAU.items():
        if chi and not any(c in ten for c in chi): continue
        bo = {'qua': [qua(hat0 + 10 * i, kieu) for i in range(3)], 'doi': [doi(hat0 + 100 + 10 * i, kieu) for i in range(3)]}
        for loai, ds in bo.items():
            for i, x in enumerate(ds):
                tb.ghi(os.path.join(ra, '%s_%s%d.wav' % (ten, loai, i + 1)), x)
                th.do('%s_%s%d' % (ten, loai, i + 1), x)
                if loai == 'doi':
                    Lr = np.convolve(x[:, 0] ** 2, np.ones(4410), 'same'); Rr = np.convolve(x[:, 1] ** 2, np.ones(4410), 'same')
                    pos = (Rr - Lr) / (Rr + Lr + 1e-12); tot = Lr + Rr; idx = np.nonzero(tot > np.max(tot) * 0.2)[0]
                    print('      lech (R-L)/(R+L): dau %.2f giua %.2f cuoi %.2f' % (pos[idx[0] + 2205], pos[idx[len(idx) // 2]], pos[idx[-1] - 2205]))
            print('   chenh bien the %s: %s dB; tq 30 ms tung doan (min) %s' % (loai, ' / '.join('%.1f' % c for c in th.chenh_bien_the(ds)),
                  ' / '.join('%.2f' % min(g3.tq(x[i:i + 4410]) for i in range(0, len(x) - 4410, 2205) if np.sum(x[i:i + 4410] ** 2) > 1e-3) for x in ds)))
        g = ghep_game(bo['qua'], bo['doi'])
        tb.ghi(os.path.join(ra, '%s_ghep_nhu_trong_game.wav' % ten), g)
        th.do('%s_ghep' % ten, g)
