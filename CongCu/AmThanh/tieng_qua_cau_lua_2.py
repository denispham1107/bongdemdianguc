# -*- coding: utf-8 -*-
# TIENG QUA CAU LUA - BAN 2 (15/10/2026, nguoi dung: "am thanh nghe van con qua mono, tao lai cho that ro la stereo va
# chan that tu qua cau lua khi bay va khi va cham vo tung").
#
# Vi sao ban 1 (tieng_qua_cau_lua.py, mau C) nghe mono:
#   - phan TO NHAT cua tieng no (no_than) va tieng ban ("phup") dat GIUA, mot nguon mono cho ca hai kenh;
#   - lua_reo: 60% la phan CHUNG hai kenh;
#   - vang (tb.vang) tron lai phan kho MONO;
#   -> clip ban / no tuong quan hai kenh 0,59 / 0,62 (bay 0,25). Them nua trong Unity, Spread 90 cua AudioSource 3D bop hep them
#      (do o loa 0,85).
# Ban 2: MOI LOP lam rieng tung kenh (on doc lap trai / phai, cong huong lech tan so), cac lop chuyen dong QUET trai -> phai,
# manh vo / tan lua phan tan hai ben, vang stereo chi phan uot (moi kenh mot xung). Muc tieu: tuong quan hai kenh <= 0,3.
# Van giu: nang luong chinh 200 Hz - 4 kHz, duoi 200 Hz <= ~25%, cat < 120 Hz (loa dien thoai).
#
# Ba MAU de nguoi dung chon (moi mau: ban, bay (2 s lap), no + file GHEP nghe nhu trong game, qua bay tu trai sang phai):
#   A "lua_that_rong"  - giu chat "Lua that" nguoi dung da chon, lam rong: lua phan phat, tanh tach, no nang + manh vo.
#   B "lua_gam"        - lua gam du doi: tieng gam lua tram day hon, no hai nhip (va cham chat roi qua cau lua BUNG ra), lua chay dai.
#   C "lua_xoay"       - qua cau lua XOAY: ngon lua quay vong trai <-> phai, no bung xoan oc toa hai ben.
#
# Chay: python CongCu/AmThanh/tieng_qua_cau_lua_2.py [thu_muc_ra]   (mac dinh CongCu/AmThanh/QuaCauLuaThu2)

import os, sys
import numpy as np
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import tieng_bam as tb
import tieng_qua_cau_lua as ql

SR = tb.SR
PI2 = tb.PI2
GIAY_BAY = 2.0


def on(n, thap, cao, hat):
    return tb.loc_dai(np.random.default_rng(hat).standard_normal(n), thap, cao)


def on_st(n, thap, cao, hat, chung=0.12):
    """On STEREO: hai kenh doc lap, chi mot phan nho chung (khong 'lo' o giua)."""
    c = on(n, thap, cao, hat)
    return np.stack([on(n, thap, cao, hat + 1) + chung * c, on(n, thap, cao, hat + 2) + chung * c], axis=1)


def pan(mono, p):
    """Mono -> stereo, p (so hoac mang theo mau) -1 trai .. +1 phai, giu cong suat."""
    p = np.broadcast_to(np.asarray(p, dtype=float), mono.shape)
    g = (np.clip(p, -1, 1) + 1) * np.pi / 4
    return np.stack([mono * np.cos(g), mono * np.sin(g)], axis=1) * np.sqrt(2)


def xoay(n, hz, sau, pha=0.0):
    """Bao bien do nguoc pha hai kenh -> nguon lua 'quay' trai <-> phai. hz phai la so chu ky nguyen trong vong lap."""
    t = np.arange(n) / SR
    s = np.sin(PI2 * hz * t + pha)
    return np.stack([1 - sau * (0.5 + 0.5 * s), 1 - sau * (0.5 - 0.5 * s)], axis=1)


def vut_st(t, f0, f1, giay_len, giay_tat, hat, p0, p1, q=0.45):
    """Vut gio / lua phut co VI TRI di chuyen tu p0 sang p1."""
    x = ql.vut(t, f0, f1, giay_len, giay_tat, hat, q)
    pp = p0 + (p1 - p0) * np.clip(t / max(giay_len + giay_tat, 1e-4), 0, 1)
    return pan(x, pp)


def tanh_tach_rong(n, so_moi_giay, hat, thap=1300, cao=6000, vong=False, bao_mat=None, rong=1.0):
    """Tanh tach STEREO rong: moi xung mot cho trong [-rong, rong], xung manh hon o hai ben."""
    r = np.random.default_rng(hat)
    ra = np.zeros((n, 2))
    so = int(so_moi_giay * n / SR)
    for _ in range(so):
        i = r.integers(0, n)
        if bao_mat is not None and r.random() > bao_mat[i]:
            continue
        dai = int(SR * r.uniform(0.002, 0.01))
        tt = np.arange(dai) / SR
        xung = r.standard_normal(dai) * np.exp(-tt / r.uniform(0.0008, 0.003)) * r.uniform(0.3, 1.0) ** 2
        lech = np.sign(r.uniform(-1, 1)) * r.uniform(0.25, 1.0) * rong
        st = tb.dat_vi_tri(xung, lech)
        cs = (i + np.arange(dai)) % n if vong else i + np.arange(dai)
        hop = cs < n
        ra[cs[hop]] += st[hop]
    for k in range(2):
        ra[:, k] = tb.loc_dai(ra[:, k], thap, cao)
    return ra


def lua_reo_rong(n, thap, cao, hat, rung_hz=0.0, rung_sau=0.0, troi=0.35):
    """Lua reo: hai kenh DOC LAP (khac lua_reo ban 1 co 60% chung), bien do troi + rung phan phat lech pha hai kenh."""
    t = np.arange(n) / SR
    ra = on_st(n, thap, cao, hat, chung=0.15)
    for k in range(2):
        bien = 0.5 + 0.5 * ql.on_muot(n, troi, hat + 7 + k)
        if rung_hz > 0:
            bien *= 1 - rung_sau * (0.5 + 0.5 * np.sin(PI2 * rung_hz * t + k * 2.1)) * ql.on_muot(n, 0.15, hat + 20 + k)
        ra[:, k] *= bien
    return ra


def gam_lua(n, f, hat, sau=1.0):
    """Tieng 'gam' cua khoi lua lon: cong huong dai trung bi on dieu bien (nhu gio lua trong ong khoi), moi kenh tan so lech."""
    r = np.random.default_rng(hat)
    t = np.arange(n) / SR
    ra = np.zeros((n, 2))
    for k in range(2):
        fk = f * (1.0 + 0.045 * (k - 0.5))
        dieu = 0.6 + 0.4 * tb.loc_dai(r.standard_normal(n), 0, 25) * 6
        pha = PI2 * np.cumsum(fk * (1 + 0.02 * tb.loc_dai(r.standard_normal(n), 0, 12) * 10)) / SR
        # vong lap: keo pha cho tron SO CHU KY NGUYEN trong n mau (khong vet noi khi lap)
        tong = pha[-1] + PI2 * fk / SR
        pha -= (tong - PI2 * round(tong / PI2)) * np.arange(n) / n
        x = (np.sin(pha) + 0.55 * np.sin(2 * pha) + 0.3 * np.sin(3 * pha)) * dieu
        x += tb.loc_dai(r.standard_normal(n), fk * 1.5, fk * 6) * 1.4
        ra[:, k] = x * sau
    return ra


def vang_uot(tron, giay, hat, muc):
    """Vang STEREO chi phan UOT: moi kenh tich chap mot xung rieng (ban 1 tron lai phan kho mono)."""
    r = np.random.default_rng(hat)
    n = int(SR * giay)
    t = np.arange(n) / SR
    ra = np.zeros((len(tron) + n, 2))
    for k in range(2):
        xung = tb.loc_dai(r.standard_normal(n), 200, 5000) * np.exp(-t / (giay / 5.0))
        xung /= np.sqrt(np.sum(xung ** 2)) + 1e-9
        nn = len(tron) + n
        ra[:, k] = np.fft.irfft(np.fft.rfft(tron[:, k], nn) * np.fft.rfft(xung, nn), nn)
    kho = np.pad(tron, ((0, n), (0, 0)))
    return kho + ra * muc


def ket(tron, giay_vang, hat, muc_vang, dinh_db):
    return tb.chuan_hoa(vang_uot(tron, giay_vang, hat, muc_vang), dinh_db)


def no_st(t, f_than, giay_tat, hat, lech_hz=0.06):
    """Than vu no STEREO: xung dap va than on DOC LAP tung kenh, cong huong lech tan so hai kenh (giua van co luc)."""
    r = np.random.default_rng(hat)
    ra = np.zeros((len(t), 2))
    for k in range(2):
        f = f_than * (1 + lech_hz * (k - 0.5))
        dap = tb.loc_dai(r.standard_normal(len(t)), 300, 9000) * tb.bao(t, 0.0005, 0.012)
        than = tb.loc_dai(r.standard_normal(len(t)), 180, 1000) * tb.bao(t, 0.002, giay_tat)
        pha = PI2 * np.cumsum(f * (1 + 1.2 * np.exp(-t / 0.03))) / SR
        ch = (np.sin(pha) + 0.6 * np.sin(2.1 * pha) + 0.35 * np.sin(3.3 * pha)) * tb.bao(t, 0.002, giay_tat * 0.8)
        ra[:, k] = dap * 0.8 + than * 1.2 + ch * 0.5
    # tre 0,4 ms kenh phai: dau song vo hai ben khong trung khit (rong hon, khong "doi")
    d = int(0.0004 * SR)
    ra[:, 1] = np.concatenate([np.zeros(d), ra[:-d, 1]])
    return ra


def manh_vo(tron, so, hat, bd0=0.06, bd1=0.95, rong=1.0, nang=1.0):
    """Manh vo / tan lua bay toa hai ben (moi manh mot cho, cang ve sau cang nho)."""
    r = np.random.default_rng(hat)
    for _ in range(so):
        bd = r.uniform(bd0, bd1)
        tm = tb.thoi_gian(0.09)
        m = tb.loc_dai(r.standard_normal(len(tm)), r.uniform(800, 1500), r.uniform(2400, 5200)) * tb.bao(tm, 0.0005, r.uniform(0.006, 0.016))
        lech = np.sign(r.uniform(-1, 1)) * r.uniform(0.35, 1.0) * rong
        tb.cong(tron, tb.dat_vi_tri(m * r.uniform(0.2, 0.6) * np.exp(-bd * 2) * nang, lech), bd)


# ======================================================== MAU A: LUA THAT (RONG)
def a_ban():
    t = tb.thoi_gian(0.7)
    tron = np.zeros((len(t), 2))
    # "phup" bat lua: hai kenh on rieng + cong huong lech (255 / 268 Hz)
    for k, f in ((0, 255.0), (1, 268.0)):
        pha = PI2 * np.cumsum(f * (1 + 1.5 * np.exp(-t / 0.02))) / SR
        tron[:, k] += (on(len(t), 200, 1400, 401 + k) + 0.7 * np.sin(pha)) * tb.bao(t, 0.006, 0.07)
    # lua phut ra truoc: hai vut toa sang hai ben
    tb.cong(tron, vut_st(t, 650, 1900, 0.08, 0.16, 403, -0.1, -0.9), 0.015)
    tb.cong(tron, vut_st(t, 560, 1600, 0.09, 0.15, 404, 0.1, 0.9), 0.03)
    tb.cong(tron, tanh_tach_rong(len(t), 150, 405, bao_mat=tb.bao(t, 0.01, 0.25)) * 1.1)
    return ket(tron, 0.25, 406, 0.35, -4)


def a_bay():
    n = int(SR * GIAY_BAY)
    x = lua_reo_rong(n, 200, 1800, 411, rung_hz=12.0, rung_sau=0.55)
    x += lua_reo_rong(n, 1500, 4500, 412, rung_hz=12.0, rung_sau=0.4) * 0.3
    x *= xoay(n, 1.0, 0.35)                       # lua lan troi nhe trai <-> phai
    x += tanh_tach_rong(n, 75, 413, vong=True) * 2.6
    return ql.chuan_hoa_vong(x, -7)


def a_no():
    t = tb.thoi_gian(2.2)
    tron = np.zeros((len(t), 2))
    tb.cong(tron, no_st(t, 95, 0.32, 421) * 1.1)
    tb.cong(tron, vut_st(t, 1900, 300, 0.02, 0.55, 422, -0.1, -0.95) * 0.8, 0.005)   # lua phut toa hai ben
    tb.cong(tron, vut_st(t, 1700, 280, 0.02, 0.6, 423, 0.1, 0.95) * 0.8, 0.02)
    manh_vo(tron, 18, 424)
    tb.cong(tron, tanh_tach_rong(len(t), 130, 425, bao_mat=tb.bao(t, 0.05, 0.8)) * 1.0)
    tb.cong(tron, lua_reo_rong(len(t), 220, 1500, 426) * tb.bao(t, 0.08, 0.7)[:, None] * 0.4)
    return ket(tron, 0.6, 427, 0.45, -2)


# ======================================================== MAU B: LUA GAM
def b_ban():
    t = tb.thoi_gian(0.8)
    tron = np.zeros((len(t), 2))
    tb.cong(tron, gam_lua(len(t), 150, 501) * tb.bao(t, 0.02, 0.12)[:, None] * 0.5)
    for k, f in ((0, 210.0), (1, 228.0)):
        pha = PI2 * np.cumsum(f * (1 + 1.8 * np.exp(-t / 0.025))) / SR
        tron[:, k] += (on(len(t), 180, 1200, 502 + k) * 1.2 + 0.6 * np.sin(pha)) * tb.bao(t, 0.008, 0.09)
    tb.cong(tron, vut_st(t, 380, 2400, 0.14, 0.2, 504, -0.2, -0.85), 0.01)
    tb.cong(tron, vut_st(t, 420, 2700, 0.13, 0.2, 505, 0.2, 0.85), 0.03)
    tb.cong(tron, tanh_tach_rong(len(t), 110, 506, bao_mat=tb.bao(t, 0.02, 0.3)) * 1.0)
    return ket(tron, 0.35, 507, 0.4, -4)


def b_bay():
    n = int(SR * GIAY_BAY)
    x = gam_lua(n, 140, 511) * 0.35                                   # tieng gam trong long khoi lua
    x += lua_reo_rong(n, 180, 1500, 512, rung_hz=9.0, rung_sau=0.6) * 1.1
    x += lua_reo_rong(n, 1200, 4000, 513, rung_hz=14.0, rung_sau=0.35) * 0.35
    x *= xoay(n, 0.5, 0.3)
    x += tanh_tach_rong(n, 55, 514, vong=True) * 2.4
    return ql.chuan_hoa_vong(x, -7)


def b_no():
    t = tb.thoi_gian(2.6)
    tron = np.zeros((len(t), 2))
    # nhip 1: va cham chat (ngan, sang)
    tb.cong(tron, no_st(t, 120, 0.12, 521) * 0.75)
    # nhip 2 (sau 0,07 s): qua cau lua BUNG ra - than tram dai hon + tieng gam lua
    t2 = tb.thoi_gian(2.5)
    tb.cong(tron, no_st(t2, 88, 0.42, 522, lech_hz=0.09) * 1.15, 0.07)
    tb.cong(tron, gam_lua(len(t2), 120, 523) * tb.bao(t2, 0.04, 0.35)[:, None] * 0.5, 0.07)
    tb.cong(tron, vut_st(t2, 2200, 260, 0.03, 0.7, 524, 0.0, -1.0), 0.08)
    tb.cong(tron, vut_st(t2, 2000, 240, 0.03, 0.75, 525, 0.0, 1.0), 0.1)
    manh_vo(tron, 22, 526, bd0=0.03, bd1=1.1, nang=1.1)
    tb.cong(tron, tanh_tach_rong(len(t), 140, 527, bao_mat=tb.bao(t, 0.1, 1.0)) * 1.1)
    tb.cong(tron, lua_reo_rong(len(t), 200, 1400, 528) * tb.bao(t, 0.12, 1.0)[:, None] * 0.5)
    return ket(tron, 0.8, 529, 0.5, -2)


# ======================================================== MAU C: LUA XOAY
def c_ban():
    t = tb.thoi_gian(0.75)
    tron = np.zeros((len(t), 2))
    for k, f in ((0, 240.0), (1, 262.0)):
        pha = PI2 * np.cumsum(f * (1 + 1.5 * np.exp(-t / 0.02))) / SR
        tron[:, k] += (on(len(t), 200, 1400, 601 + k) + 0.6 * np.sin(pha)) * tb.bao(t, 0.006, 0.07)
    # vut xoay: vi tri quay mot vong trai -> phai -> trai trong 0,35 s
    x = ql.vut(t, 500, 2600, 0.15, 0.2, 603)
    tb.cong(tron, pan(x, np.sin(PI2 * t / 0.35) * 0.9), 0.01)
    tb.cong(tron, tanh_tach_rong(len(t), 130, 604, bao_mat=tb.bao(t, 0.01, 0.25)) * 1.0)
    return ket(tron, 0.3, 605, 0.4, -4)


def c_bay():
    n = int(SR * GIAY_BAY)
    x = lua_reo_rong(n, 200, 1800, 611, rung_hz=12.0, rung_sau=0.5)
    x += lua_reo_rong(n, 1400, 4200, 612, rung_hz=12.0, rung_sau=0.4) * 0.3
    x *= xoay(n, 2.0, 0.7)                       # ngon lua quay 2 vong / giay trai <-> phai
    hu = np.zeros((n, 2))                         # tieng "vu vu" cua ngon lua quay (on dai hep, xoay lech pha)
    hu += on_st(n, 500, 1100, 613, chung=0.1) * xoay(n, 2.0, 0.85, np.pi)
    x += hu * 0.5
    x += tanh_tach_rong(n, 60, 614, vong=True) * 2.4
    return ql.chuan_hoa_vong(x, -7)


def c_no():
    t = tb.thoi_gian(2.3)
    tron = np.zeros((len(t), 2))
    tb.cong(tron, no_st(t, 98, 0.3, 621) * 1.05)
    # bung xoan oc: 4 luong lua phut ra lan luot trai, phai, trai, phai, xa dan
    for i, (p, bd) in enumerate(((-0.95, 0.0), (0.95, 0.05), (-0.6, 0.11), (0.6, 0.17))):
        tb.cong(tron, vut_st(t, 2000 - 150 * i, 300, 0.02, 0.45, 622 + i, p * 0.2, p) * (0.8 - 0.1 * i), bd)
    manh_vo(tron, 18, 627)
    tb.cong(tron, tanh_tach_rong(len(t), 125, 628, bao_mat=tb.bao(t, 0.05, 0.8)) * 1.0)
    lua = lua_reo_rong(len(t), 220, 1500, 629) * tb.bao(t, 0.08, 0.7)[:, None] * 0.4
    tb.cong(tron, lua * xoay(len(t), 1.5, 0.6))
    return ket(tron, 0.6, 630, 0.45, -2)


def ghep(ban, bay, no):
    """Nghe nhu trong game: ban (ben trai) -> bay 1,4 s tu trai sang phai -> no (ben phai)."""
    dai_bay = int(SR * 1.4)
    lap = np.concatenate([bay] * 2)[:dai_bay].copy()
    n = int(0.06 * SR)
    lap[:n] *= np.linspace(0, 1, n)[:, None]
    lap[-n:] *= np.linspace(1, 0, n)[:, None]
    p = np.linspace(-0.5, 0.5, dai_bay)
    g = (p + 1) * np.pi / 4
    lap[:, 0] *= np.cos(g) * np.sqrt(2)
    lap[:, 1] *= np.sin(g) * np.sqrt(2)
    x = np.zeros((int(SR * 0.15) + dai_bay + len(no) + SR // 2, 2))
    b = ban.copy(); b[:, 0] *= 1.2; b[:, 1] *= 0.8
    o = no.copy(); o[:, 0] *= 0.8; o[:, 1] *= 1.2
    tb.cong(x, b, 0.0)
    tb.cong(x, lap, 0.15)
    tb.cong(x, o, 0.15 + 1.4 - 0.02)
    return x


def phan_tich(ten, x):
    m = x.mean(axis=1)
    P = np.abs(np.fft.rfft(m)) ** 2
    f = np.fft.rfftfreq(len(m), 1 / SR)
    pt = [P[(f >= a) & (f < b)].sum() / P.sum() * 100 for a, b in [(0, 200), (200, 1000), (1000, 4000), (4000, 22050)]]
    l, r = x[:, 0], x[:, 1]
    tq = float(np.sum(l * r) / np.sqrt(np.sum(l * l) * np.sum(r * r)))
    s = (l - r) / 2
    sm = float(np.sum(s * s) / np.sum(m * m))
    print('%-34s %.2f s  <200/200-1k/1-4k/>4k: %s  tuong quan %.2f  side/mid %.2f  dinh %.2f' % (ten, len(x) / SR,
          ' / '.join('%.0f%%' % p for p in pt), tq, sm, np.max(np.abs(x))))
    return pt, tq


if __name__ == '__main__':
    ra = sys.argv[1] if len(sys.argv) > 1 else os.path.join('CongCu', 'AmThanh', 'QuaCauLuaThu2')
    os.makedirs(ra, exist_ok=True)
    for ten, fb, fy, fn in [('A_lua_that_rong', a_ban, a_bay, a_no), ('B_lua_gam', b_ban, b_bay, b_no),
                            ('C_lua_xoay', c_ban, c_bay, c_no)]:
        ban, bay, no = fb(), fy(), fn()
        noi = np.max(np.abs(bay[0] - bay[-1]))
        tb_buoc = np.mean(np.max(np.abs(np.diff(bay, axis=0)), axis=1))
        print('   vong lap %s: buoc nhay o cho noi %.4f (trung binh moi mau %.4f)' % (ten, noi, tb_buoc))
        for loai, x in [('ban', ban), ('bay', bay), ('no', no), ('ghep_nghe_thu', ghep(ban, bay, no))]:
            d = os.path.join(ra, '%s_%s.wav' % (ten, loai))
            tb.ghi(d, x)
            phan_tich(os.path.basename(d), x)
