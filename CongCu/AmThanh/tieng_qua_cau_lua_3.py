# -*- coding: utf-8 -*-
# TIENG QUA CAU LUA - BAN 3 (15/10/2026). Nguoi dung nghe ban 2 (3 mau): "chua giong tieng lua that, nghe con don dieu qua".
# Vi sao ban 1 / 2 nghe gia + don dieu: lua = on loc dai TINH + rung phan phat DEU 12 Hz; tanh tach cung mot dai loc; vu no = mot bao
# tat mu duy nhat -> tai nghe ra "may moc", lan nao cung nhu lan nao.
#
# Ban 3 dung MO HINH LUA THAT (kieu Farnell "Designing Sound", chuong Lua) - ba thanh phan NGAU NHIEN KHONG DEU:
#   - LIEM LUA (lapping): on tram rat thap (~30 Hz) khuech dai roi CAT -> song vuong ngau nhien, moi lan doi dau la mot "phup";
#     loc ve dai 150-900 Hz -> tieng lua liem / phu phu khong co nhip deu.
#   - XI HOI (hissing): on cao 1-8 kHz nhan voi (on cham)^4 -> tung con xi bat chot, luc co luc khong.
#   - LACH TACH (crackling): xung ngan, moi xung mot TAN SO CONG HUONG, do dai, do to, vi tri rieng (phan bo theo dot - lua "no"
#     lach tach tung tran), xen tieng "BOP" tram hon nhu mat go.
# Bay: lua lon + tieng gio xe (dai on troi tan so khong deu) + nhung cu "phut" lua vat ngau nhien (khong deu 12 Hz nhu ban cu).
# No: tieng nut chat + "UM" than (tut cao do, bao hoa de con dai trung) + tieng ren LAN NHIEU DOT + khoi lua BUNG ra + manh vo NAY
# nhieu lan + mua dat da roi + tan lua lach tach tat dan + vang stereo (moi kenh mot xung).
# Moi tieng co BA BIEN THE (hat khac nhau) - trong game chon ngau nhien + lech cao do nhe -> khong lan nao giong lan nao.
# STEREO: moi thanh phan sinh rieng tung kenh / moi su kien mot vi tri. Nang luong chinh 200 Hz - 4 kHz, < 200 Hz <= ~25%, cat < 120 Hz.
#
# Chay: python CongCu/AmThanh/tieng_qua_cau_lua_3.py [thu_muc_ra]   (mac dinh CongCu/AmThanh/QuaCauLuaThu3)

import os, sys
import numpy as np
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import tieng_bam as tb

SR = tb.SR
PI2 = tb.PI2
GIAY_BAY = 3.0


# ------------------------------------------------------------------ loc FFT (tron vong - vong lap lien mach)
def loc(x, h):
    n = len(x)
    f = np.fft.rfftfreq(n, 1 / SR)
    return np.fft.irfft(np.fft.rfft(x) * h(f), n)


def dai(fc, q):
    """Loc dai cong huong (dap ung cua bo loc bac 2) tam fc, he so Q."""
    return lambda f: 1.0 / np.sqrt(1 + (q * (f / fc - fc / np.maximum(f, 1e-3))) ** 2)


def thap(fc, bac=2):
    return lambda f: 1.0 / np.sqrt(1 + (f / fc) ** (2 * bac))


def cao(fc, bac=2):
    return lambda f: (f / fc) ** bac / np.sqrt(1 + (f / fc) ** (2 * bac))


def cham(n, hz, r):
    """On bien thien cham (loc thap hz), chuan ve trung binh 0 do lech 1."""
    y = loc(r.standard_normal(n), thap(hz, 2))
    return (y - y.mean()) / (y.std() + 1e-12)


# ------------------------------------------------------------------ ba thanh phan lua
def liem_lua(n, r, tam=30.0, manh=1.0, dai_tu=150.0, dai_den=900.0):
    """LIEM LUA: on ~30 Hz khuech dai + cat -> song vuong ngau nhien; loc ve dai trung."""
    y = loc(r.standard_normal(n), dai(tam, 4.0))
    y = np.clip(y / (y.std() + 1e-12) * 3.0 * manh, -0.9, 0.9)
    y = loc(y, lambda f: cao(25)(f) * cao(dai_tu, 2)(f) * thap(dai_den, 2)(f))
    return y / (y.std() + 1e-12)


def xi_hoi(n, r, hz=1.5, mu=4):
    """XI HOI: on 1-8 kHz nhan (on cham)^mu -> con xi bat chot."""
    on = loc(r.standard_normal(n), lambda f: cao(1000, 2)(f) * thap(8000, 2)(f))
    bao = np.maximum(cham(n, hz, r) * 0.6 + 0.5, 0) ** mu
    y = on * bao
    return y / (y.std() + 1e-12)


def lach_tach(n, r, mat_do, vong=False, bao_mat=None, nang_bop=0.12, rong=1.0, cao_min=1200, cao_max=9000):
    """LACH TACH stereo: moi xung mot tan so cong huong, do dai, do to, vi tri; mat do theo DOT (on cham ^3)."""
    ra = np.zeros((n, 2))
    dot = np.maximum(cham(n, 2.5, r) * 0.5 + 0.55, 0) ** 3
    if bao_mat is not None:
        dot = dot * bao_mat
    p = dot / (dot.mean() + 1e-12) * mat_do / SR
    vt = np.nonzero(r.random(n) < p)[0]
    for i in vt:
        bop = r.random() < nang_bop
        if bop:   # "bop" mat go: tram hon, dai hon, to hon
            fc = np.exp(r.uniform(np.log(380), np.log(1100))); tau = r.uniform(0.006, 0.02); a = r.uniform(0.6, 1.4); q = r.uniform(2, 5)
        else:
            fc = np.exp(r.uniform(np.log(cao_min), np.log(cao_max))); tau = np.exp(r.uniform(np.log(0.0003), np.log(0.006)))
            a = np.exp(r.uniform(np.log(0.05), np.log(1.0))); q = r.uniform(1.0, 3.0)
        d = int(min(0.08, tau * 7) * SR) + 16
        t = np.arange(d) / SR
        xung = loc(r.standard_normal(d) * np.exp(-t / tau), dai(fc, q)) * a
        xung *= np.minimum(1, t / 0.0002)
        st = tb.dat_vi_tri(xung, np.clip(r.uniform(-1, 1) * rong, -1, 1))
        cs = (i + np.arange(d)) % n if vong else i + np.arange(d)
        hop = cs < n
        ra[cs[hop]] += st[hop]
    return ra


def lua(n, hat, liem=1.0, xi=0.25, tach=1.0, mat_do=60, vong=False, bao_mat=None, tam_liem=30.0, nang_bop=0.12,
        dai_den=900.0, chung=0.45):
    """Mot ngon lua STEREO: liem + xi doc lap tung kenh (mot phan chung), lach tach tung xung mot cho."""
    r = np.random.default_rng(hat)
    ra = np.zeros((n, 2))
    lc = liem_lua(n, r, tam_liem, dai_den=dai_den)
    xc = xi_hoi(n, r)
    for k in range(2):
        ra[:, k] = liem * (chung * lc + (1 - chung) * liem_lua(n, r, tam_liem * r.uniform(0.85, 1.15), dai_den=dai_den)) \
            + xi * (chung * xc + (1 - chung) * xi_hoi(n, r))
    if bao_mat is not None:
        ra *= bao_mat[:, None]
    ra += lach_tach(n, r, mat_do, vong, bao_mat, nang_bop) * tach * 2.0
    return ra


# ------------------------------------------------------------------ cac tieng phu
def gio_xe(n, r, f0=700, f1=2200, hz=1.2):
    """Gio xe khi khoi lua lao qua khong khi: on dai, TAN SO TAM troi ngau nhien (khong deu), moi kenh mot duong."""
    ra = np.zeros((n, 2))
    for k in range(2):
        z = r.standard_normal(n)
        # chia 6 dai, moi dai mot bao cham -> tam tan so "troi"
        y = np.zeros(n)
        tams = np.geomspace(f0, f1, 6)
        w = cham(n, hz, r)
        for j, fc in enumerate(tams):
            g = np.exp(-0.5 * ((w * 1.3 - (j - 2.5) / 1.2)) ** 2)
            y += loc(z, dai(fc, 2.5)) * g
        ra[:, k] = y / (y.std() + 1e-12)
    return ra


def phut_vat(n, r, so_moi_giay, vong=False, rong=0.8):
    """Nhung cu 'phut' lua vat ngau nhien (khong deu): on dai trung, len mem, tat 40-140 ms, moi cu mot cho."""
    ra = np.zeros((n, 2))
    t_ = 0.0
    while True:
        t_ += r.exponential(1.0 / so_moi_giay)
        i = int(t_ * SR)
        if i >= n: break
        d = int(SR * r.uniform(0.12, 0.3)); t = np.arange(d) / SR
        len_ = r.uniform(0.01, 0.04); tat = r.uniform(0.04, 0.14)
        e = np.minimum(1, t / len_) * np.exp(-np.maximum(t - len_, 0) / tat)
        x = loc(r.standard_normal(d), dai(r.uniform(260, 700), 1.2)) * e * r.uniform(0.4, 1.0)
        st = tb.dat_vi_tri(x, r.uniform(-rong, rong))
        cs = (i + np.arange(d)) % n if vong else i + np.arange(d)
        hop = cs < n
        ra[cs[hop]] += st[hop]
    return ra


def um(t, r, f0, f1, giay_tat, lech=0.025):
    """'UM' than vu no: sin tut cao do f0 -> f1 + hai bac, BAO HOA (tanh) -> sinh hoa am dai trung; hai kenh lech tan so."""
    ra = np.zeros((len(t), 2))
    for k in range(2):
        f = (f1 + (f0 - f1) * np.exp(-t / 0.06)) * (1 + lech * (k - 0.5))
        pha = PI2 * np.cumsum(f) / SR + r.uniform(0, PI2)
        x = np.sin(pha) + 0.5 * np.sin(2 * pha + 0.3) + 0.25 * np.sin(3 * pha + 1.1)
        ra[:, k] = np.tanh(2.5 * x * np.exp(-t / giay_tat)) * np.minimum(1, t / 0.002)
    return ra


def ren_lan(t, r, giay, dot=4):
    """Tieng ren LAN NHIEU DOT sau vu no: on thap 150-600 Hz, bao = tat dan x nhung dot bung ngau nhien."""
    ra = np.zeros((len(t), 2))
    n = len(t)
    zc = loc(r.standard_normal(n), lambda f: cao(140, 2)(f) * thap(650, 2)(f))   # phan chung: tieng ren co "tam"
    for k in range(2):
        z = 0.6 * zc + 0.8 * loc(r.standard_normal(n), lambda f: cao(140, 2)(f) * thap(650, 2)(f))
        b = np.exp(-t / giay)
        for _ in range(dot):
            c = r.uniform(0.05, giay * 1.6); w = r.uniform(0.04, 0.16)
            b += r.uniform(0.3, 0.9) * np.exp(-t / giay) * np.exp(-0.5 * ((t - c) / w) ** 2)
        b *= 0.7 + 0.6 * np.maximum(cham(n, 9.0, r) * 0.4 + 0.6, 0)
        ra[:, k] = z / (z.std() + 1e-12) * b * np.minimum(1, t / 0.004)
    return ra


def nut(t, r):
    """Tieng NUT chat dau vu no: xung rong 1-2 ms, hai kenh khac nhau + tre 0,3 ms."""
    ra = np.zeros((len(t), 2))
    for k in range(2):
        ra[:, k] = loc(r.standard_normal(len(t)), lambda f: cao(400, 2)(f) * thap(9000, 2)(f)) * np.exp(-t / r.uniform(0.0012, 0.0022))
    d = int(0.0003 * SR)
    ra[:, 1] = np.concatenate([np.zeros(d), ra[:-d, 1]])
    return ra


def manh_nay(tron, r, so, bd0, bd1, nang=1.0):
    """Manh vo (da, than cui) roi va NAY 2-4 lan, khoang nay ngan dan, moi lan nhe dan; moi manh mot cho, mot bo cong huong."""
    for _ in range(so):
        bd = r.uniform(bd0, bd1)
        lech = np.clip(r.normal(0, 0.6), -1, 1)
        fcs = [r.uniform(900, 2400) * m for m in (1.0, r.uniform(1.5, 2.3), r.uniform(2.6, 3.8))]
        a = r.uniform(0.25, 0.8) * nang * np.exp(-bd * 1.2)
        khoang = r.uniform(0.06, 0.16)
        for lan in range(r.integers(2, 5)):
            d = int(0.05 * SR); t = np.arange(d) / SR
            x = np.zeros(d)
            for j, fc in enumerate(fcs):
                x += np.sin(PI2 * fc * r.uniform(0.97, 1.03) * t + r.uniform(0, PI2)) * np.exp(-t / r.uniform(0.003, 0.012)) / (1 + j)
            x += loc(r.standard_normal(d), cao(1500, 2)) * np.exp(-t / 0.0015) * 0.6
            tb.cong(tron, tb.dat_vi_tri(x * a, lech), bd)
            bd += khoang; khoang *= r.uniform(0.5, 0.75); a *= r.uniform(0.35, 0.6)
            if bd >= len(tron) / SR - 0.06: break


def mua_dat(n, r, bd, giay, mat_do, nang=1.0):
    """Mua dat da / tan than roi lop dop sau vu no: rat nhieu hat nho, mat do tang roi giam."""
    t = np.arange(n) / SR
    bao = np.clip((t - bd) / 0.1, 0, 1) * np.exp(-np.maximum(t - bd - 0.1, 0) / giay)
    x = lach_tach(n, r, mat_do, bao_mat=bao, nang_bop=0.0, cao_min=1800, cao_max=6500)
    return x * 0.6 * nang


def vang_uot(tron, giay, r, muc, som=True):
    """Vang STEREO: phan xa SOM khac nhau hai kenh (8-40 ms) + duoi on, moi kenh mot xung; chi cong phan UOT."""
    n = int(SR * giay); t = np.arange(n) / SR
    ra = np.zeros((len(tron) + n, 2))
    for k in range(2):
        xung = loc(r.standard_normal(n), lambda f: cao(200, 2)(f) * thap(4500, 2)(f)) * np.exp(-t / (giay / 5.0))
        xung[:int(0.012 * SR)] = 0
        if som:
            for _ in range(6):
                xung[int(r.uniform(0.008, 0.04) * SR)] += r.uniform(-1, 1) * 6
        xung /= np.sqrt(np.sum(xung ** 2)) + 1e-9
        nn = len(tron) + n
        ra[:, k] = np.fft.irfft(np.fft.rfft(tron[:, k], nn) * np.fft.rfft(xung, nn), nn)
    return np.pad(tron, ((0, n), (0, 0))) + ra * muc


def chuan(x, dinh_db, vong=False):
    """Can dai pho cho loa dien thoai: cat < 120 Hz, ha nhe < 220 Hz, giam nhe > 7 kHz; chuan dinh."""
    h = lambda f: cao(120, 4)(f) * (0.55 + 0.45 * (f / 220) ** 2 / (1 + (f / 220) ** 2)) * (1 / np.sqrt(1 + (f / 9000) ** 4))
    x = np.stack([loc(x[:, 0], h), loc(x[:, 1], h)], axis=1)
    x = x - x.mean(axis=0)
    x = x / (np.max(np.abs(x)) + 1e-9) * (10 ** (dinh_db / 20))
    if not vong:
        m = int(0.01 * SR)
        x[-m:] *= np.linspace(1, 0, m)[:, None]
    return x


# ================================================================== BA TIENG (moi tieng mot ham, nhan hat -> bien the)
def tieng_ban(hat, kieu):
    r = np.random.default_rng(hat)
    g = 0.9 if kieu == "A" else 1.1
    n = int(SR * g); t = np.arange(n) / SR
    tron = np.zeros((n, 2))
    # "PHUNG" lua bat len: khoi lua bung nhanh (liem lua manh, len 15 ms, tat 0,25 s)
    # moi bien the: khoi lua lon / nho, tat nhanh / cham, tieng "phung" cao / thap (do: bien the B cu chi lech 1,2-1,7 dB)
    tat = (0.22 if kieu == "A" else 0.3) * r.uniform(0.6, 1.5)
    bm = np.minimum(1, t / r.uniform(0.008, 0.03)) * np.exp(-t / tat)
    tron += lua(n, hat + 1, liem=1.3, xi=r.uniform(0.2, 0.5), tach=r.uniform(0.4, 1.0), mat_do=r.uniform(90, 200), bao_mat=bm,
                tam_liem=r.uniform(35, 55), dai_den=1200)
    tron += um(t, r, r.uniform(200, 320), r.uniform(120, 170), r.uniform(0.03, 0.08)) * r.uniform(0.2, 0.5)
    # lao di: gio xe to dan roi nho (qua roi tay), lech dan sang mot ben
    dinh = r.uniform(0.05, 0.14)
    bg = np.minimum(1, t / dinh) * np.exp(-np.maximum(t - dinh, 0) / r.uniform(0.12, 0.28))
    gx = gio_xe(n, r, 600, 2400, 3.0) * bg[:, None] * r.uniform(0.3, 0.6)
    p = np.linspace(0, r.choice([-0.5, 0.5]), n)
    gx[:, 0] *= np.sqrt(1 - p); gx[:, 1] *= np.sqrt(1 + p)
    tron += gx
    tron += phut_vat(n, r, 9, rong=0.9) * bm[:, None] * 0.8
    tron = vang_uot(tron, 0.35, r, 0.25)
    return chuan(tron, -4)


def tieng_bay(hat, kieu):
    """Vong lap GIAY_BAY giay, lien mach: moi thanh phan loc tron vong, su kien quan ve dau."""
    r = np.random.default_rng(hat)
    n = int(SR * GIAY_BAY)
    x = lua(n, hat + 1, liem=1.0, xi=0.35, tach=0.8, mat_do=55 if kieu == "A" else 40, vong=True,
            tam_liem=34 if kieu == "A" else 26, nang_bop=0.1, dai_den=1000 if kieu == "A" else 750, chung=0.4)
    # nhung con BUNG / LANG cua khoi lua (do bay ban dau: bao bien thien 0,20 - bang ban cu, nghe deu): on cham 0,7 Hz ^1,5
    # (tan so chon so chu ky nguyen trong vong lap nho loc FFT tron vong)
    bung = np.maximum(cham(n, 0.9, r) * 0.45 + 0.75, 0.15) ** 1.5
    x *= bung[:, None]
    x += gio_xe(n, r, 500, 2000, 0.9) * (0.35 if kieu == "A" else 0.28)
    x += phut_vat(n, r, 5.5 if kieu == "A" else 4.0, vong=True) * (1.1 if kieu == "A" else 1.4)
    if kieu == "B":
        # khoi lua lon: them mot lop liem lua tram hon, cham hon (gam)
        x += lua(n, hat + 9, liem=0.9, xi=0.0, tach=0.0, mat_do=1, vong=True, tam_liem=18, dai_den=600, chung=0.3) * 0.8
    return chuan(x, -7, vong=True)


def tieng_no(hat, kieu):
    """Moi bien the khac nhau CA CAU TRUC (do 3 bien the dau tien: bao bien do giong nhau 0,95-0,99 -> van don dieu):
    do dai tat, luc khoi lua bung, co / khong no phu, so dot ren, so manh vo, cao do 'um' deu gieo ngau nhien."""
    r = np.random.default_rng(hat)
    to = (1.0 if kieu == "A" else 1.25) * r.uniform(0.7, 1.35)      # vu no lon / nho
    g = 2.6 if kieu == "A" else 3.2
    n = int(SR * g); t = np.arange(n) / SR
    tron = np.zeros((n, 2))
    tron += nut(t, r) * r.uniform(0.8, 1.5)
    tron += um(t, r, r.uniform(130, 210), r.uniform(60, 85), 0.16 * to) * 0.55 * to
    tron += ren_lan(t, r, 0.3 * to, dot=r.integers(2, 7)) * 0.5 * to
    # khoi lua BUNG ra: tre 0-90 ms sau tieng nut, len 25-80 ms, tat dan theo co vu no
    tre = r.uniform(0.0, 0.09); len_ = r.uniform(0.025, 0.08)
    bm = np.clip((t - tre) / len_, 0, 1) * np.exp(-np.maximum(t - tre, 0) / (0.5 * to))
    tron += lua(n, hat + 1, liem=1.4, xi=0.5, tach=0.6, mat_do=180, bao_mat=bm, tam_liem=r.uniform(30, 50), dai_den=1300, chung=0.4) * 0.9
    # no phu (45% so lan): mot tui lua / hoi bung them o mot ben, 0,12-0,45 s sau
    if r.random() < 0.45:
        bd = r.uniform(0.12, 0.45); t2 = t[:n - int(bd * SR)]
        p2 = r.uniform(-0.8, 0.8)
        phu = nut(t2, r) * 0.5 + um(t2, r, r.uniform(160, 240), 90, 0.08) * 0.3
        phu += lua(len(t2), hat + 3, liem=1.2, xi=0.4, tach=0.5, mat_do=150, bao_mat=np.minimum(1, t2 / 0.02) * np.exp(-t2 / 0.2), chung=0.3) * 0.5
        phu[:, 0] *= np.sqrt(1 - p2); phu[:, 1] *= np.sqrt(1 + p2)
        tb.cong(tron, phu, bd)
    # gio xe toa hai ben (khong khi bi day ra)
    for k, p in ((0, -0.9), (1, 0.9)):
        b = np.minimum(1, t / 0.015) * np.exp(-t / r.uniform(0.12, 0.25))
        gx = gio_xe(n, r, 2400, 500, 4.0)[:, k] * b
        tron += tb.dat_vi_tri(gx, p) * 0.35
    manh_nay(tron, r, int(r.integers(4, 13) * to), 0.04, 0.7, nang=0.9)
    tron += mua_dat(n, r, r.uniform(0.1, 0.25), 0.45 * to, r.uniform(150, 320), nang=r.uniform(0.6, 1.2))
    # than lua con chay lach tach roi tat
    be = np.clip((t - 0.2) / 0.3, 0, 1) * np.exp(-np.maximum(t - 0.5, 0) / (0.55 * to))
    tron += lua(n, hat + 2, liem=0.4, xi=0.2, tach=1.0, mat_do=r.uniform(30, 60), bao_mat=be, nang_bop=0.18) * 0.7
    tron = vang_uot(tron, 0.8, r, 0.45)
    return chuan(tron, -2)


def ghep(bans, bay, nos):
    """Nghe nhu trong game: 3 lan ban (3 bien the), moi qua bay 1,1 s tu trai sang phai roi no ben phai."""
    out = np.zeros((int(SR * 11.0), 2))
    t0 = 0.0
    for i in range(3):
        b = bans[i].copy(); b[:, 0] *= 1.2; b[:, 1] *= 0.8
        tb.cong(out, b, t0)
        d = int(SR * 1.1); vt = int(SR * (i * 0.73)) % len(bay)
        lap = np.concatenate([bay, bay])[vt:vt + d].copy()
        m = int(0.08 * SR)
        lap[:m] *= np.linspace(0, 1, m)[:, None]; lap[-m:] *= np.linspace(1, 0.3, m)[:, None]
        p = np.linspace(-0.5, 0.6, d); gg = (p + 1) * np.pi / 4
        lap[:, 0] *= np.cos(gg) * np.sqrt(2); lap[:, 1] *= np.sin(gg) * np.sqrt(2)
        tb.cong(out, lap * 0.8, t0 + 0.12)
        o = nos[i].copy(); o[:, 0] *= 0.75; o[:, 1] *= 1.25
        tb.cong(out, o, t0 + 0.12 + 1.1 - 0.03)
        t0 += 3.6
    return out


def phan_tich(ten, x):
    m = x.mean(axis=1)
    P = np.abs(np.fft.rfft(m)) ** 2
    f = np.fft.rfftfreq(len(m), 1 / SR)
    pt = [P[(f >= a) & (f < b)].sum() / P.sum() * 100 for a, b in [(0, 200), (200, 1000), (1000, 4000), (4000, 22050)]]
    l, r = x[:, 0], x[:, 1]
    tq = float(np.sum(l * r) / np.sqrt(np.sum(l * l) * np.sum(r * r)))
    print('%-28s %.2f s  <200/200-1k/1-4k/>4k: %s  tuong quan %.2f  dinh %.2f' % (ten, len(x) / SR,
          ' / '.join('%.0f%%' % p for p in pt), tq, np.max(np.abs(x))))


if __name__ == '__main__':
    ra = sys.argv[1] if len(sys.argv) > 1 else os.path.join('CongCu', 'AmThanh', 'QuaCauLuaThu3')
    os.makedirs(ra, exist_ok=True)
    for kieu, ten in (("A", "A_duoc_lua"), ("B", "B_khoi_lua_lon")):
        bans = [tieng_ban(700 + 10 * i + (0 if kieu == "A" else 500), kieu) for i in range(3)]
        bay = tieng_bay(800 + (0 if kieu == "A" else 500), kieu)
        nos = [tieng_no(900 + 10 * i + (0 if kieu == "A" else 500), kieu) for i in range(3)]
        noi = np.max(np.abs(bay[0] - bay[-1])); d = np.max(np.abs(np.diff(bay, axis=0)), axis=1)
        print('   vong lap %s: noi %.4f, xep hang %.0f%% cac buoc' % (ten, noi, 100 * np.mean(d < noi)))
        for i in range(3):
            tb.ghi(os.path.join(ra, '%s_ban%d.wav' % (ten, i + 1)), bans[i]); phan_tich('%s_ban%d' % (ten, i + 1), bans[i])
            tb.ghi(os.path.join(ra, '%s_no%d.wav' % (ten, i + 1)), nos[i]); phan_tich('%s_no%d' % (ten, i + 1), nos[i])
        tb.ghi(os.path.join(ra, '%s_bay.wav' % ten), bay); phan_tich('%s_bay' % ten, bay)
        g = ghep(bans, bay, nos)
        g = g / np.max(np.abs(g)) * 0.95
        tb.ghi(os.path.join(ra, '%s_ghep_nghe_thu.wav' % ten), g); phan_tich('%s_ghep' % ten, g)
