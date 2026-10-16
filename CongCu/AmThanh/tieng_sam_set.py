# -*- coding: utf-8 -*-
# TIENG SAM SET (16/10/2026, nguoi dung): "them am thanh stereo cho skill Sam set - luc tia set no PHONG tu tren cao xuong, khi tia set danh trung MAT DAT
# hoac KE DICH nghe tieng CHAY XEM; am thanh chan that stereo nhu tieng set ngoai doi that, bao dam KHONG BI UNITY BOP mat am thanh". Nguoi dung chon:
# chi Sam set (May giong lam rieng sau), CO them SAM REN cuon cho ca tran set. Trong suong mu thi khong nghe (TiengViTri).
# Ba tieng: danh (moi tia: "RET" xe toac tu tren xuong + "DOANG" no - ~0,7-1 s) · chay (moi cho trung: chay xem "xeeo" + khoi xi ~0,5-0,9 s) ·
# ren (moi lan tung: sam ren cuon 4 s ca vung).
# Set that: tieng XE (khong khi bi xe doc kenh set - lao xao day dac, sang dan khi toi gan) -> tieng NO "doang / toac" (chum phong dien cuc ngan
# chong nhau trong 20-60 ms, KHONG phai mot xung + "huych" nhu tieng sung - bai hoc Giut set ban 1) -> sam ren cuon (on tram doi dot khong deu, lan
# qua lai tren troi). Phat CAN BANG (Editor / may tinh 2D + panStereo - TiengViTri.PhatCanBang) de Unity khong bop stereo.
# Quy trinh memory: moi lop rieng tung kenh, su kien rai trai / phai bang am luong, 3 bien the khac cau truc, < 200 Hz <= ~25% (sam ren cho phep
# hon mot chut nhung than o 150-500 Hz de loa dien thoai nghe duoc), > 4 kHz <= ~10%, mo phong ra loa (tieng_giat_set_3.can_bang) truoc khi gui.
#
# Ba MAU:
#   A "set_gan"     - set danh SAT ben: "ret" ngan 60-150 ms -> "DOANG" to, gion + chay xem manh; sam ren day.
#   B "set_xe_troi" - "RẸẸẸT" xe dai 0,2-0,45 s sang dan tu tren cao xuong -> "doang"; sam ren cuon lan qua lai.
#   C "bao_set"     - "ret" + "doang" kem tieng XET dien (phong dien con lai) + chay xem co ho quang ri ri; sam ren co nhieu tieng dung xa.
#
# Chay: python CongCu/AmThanh/tieng_sam_set.py [thu_muc_ra] [loc_ten_mau...]   (mac dinh CongCu/AmThanh/SamSetThu)

import os, sys
import numpy as np
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import tieng_bam as tb
import tieng_qua_cau_lua_3 as q3
import tieng_tang_hinh as th
import tieng_giat_set as g1
import tieng_giat_set_3 as g3
import tieng_giat_set_4 as g4

SR = tb.SR
PI2 = tb.PI2
loc, dai, thap, cao, cham = q3.loc, q3.dai, q3.thap, q3.cao, q3.cham
on2, loc2, keo_lech = th.on2, th.loc2, th.keo_lech
xeo, dat = g1.xeo, g3.dat


def xe(n, r, bd, giay, nang=1.0):
    """"RET" XE TOAC tu tren cao xuong: xeo day dac hai dai, to dan + SANG dan (thong thap mo 900 Hz -> 3,4 kHz) theo hinh mu - tia set lao xuong
    gan dan; rieng tung kenh. Khong co cao do."""
    t = np.arange(n) / SR
    tt = t - bd
    x = np.clip(tt / giay, 0, 1)
    b = (0.15 + 0.85 * x ** 2) * (tt >= 0) * (tt <= giay + 0.005) * np.maximum(cham(n, 40, r) * 0.35 + 1, 0.2)
    s = xeo(n, r, b + 1e-4, r.uniform(5000, 9000), (700, 3400), chung=0.25, giat=60)
    s += xeo(n, r, b + 1e-4, 4000, (300, 1000), chung=0.25, giat=50) * 0.8
    fc = 900 * (3400 / 900) ** x
    s = th.quet2(s, np.maximum(fc, 300), kieu='thap')
    return s / (np.max(np.abs(s)) + 1e-12) * (b / (np.max(b) + 1e-12))[:, None] * nang


def doang(n, r, bd, nang=1.0, giay_than=None):
    """"DOANG / TOAC" no: 15-40 phong dien cuc ngan (0,2-1,5 ms) chong nhau trong 20-60 ms, moi cai mot cho, loc rong + bao hoa; than on 150-600 Hz
    tat 80-200 ms (KHONG phai sin tram 'huych'); hai kenh rieng."""
    t = np.arange(n) / SR
    ra = np.zeros((n, 2))
    i0 = int(bd * SR)
    rong = r.uniform(0.02, 0.06)
    for _ in range(int(r.integers(15, 41))):
        i = i0 + int(r.uniform(0, rong) ** 1.5 / rong ** 0.5 * SR)
        d = int(r.uniform(0.0002, 0.0015) * SR) + 6
        if i + d >= n: continue
        a = r.uniform(0.3, 1.0)
        g = (r.uniform(-0.9, 0.9) + 1) * np.pi / 4
        z = r.standard_normal(d) * np.hanning(d) * a
        ra[i:i + d, 0] += z * np.cos(g) * np.sqrt(2); ra[i:i + d, 1] += r.standard_normal(d) * np.hanning(d) * a * np.sin(g) * np.sqrt(2)
    ra = loc2(ra, lambda ff: cao(250, 2)(ff) * thap(3600, 3)(ff))
    ra = np.tanh(2.0 * ra / (np.max(np.abs(ra)) + 1e-12))
    tt = np.maximum(t - bd, 0)
    gt = r.uniform(0.08, 0.2) if giay_than is None else giay_than
    than = loc2(on2(n, r, 0.35), lambda ff: cao(150, 2)(ff) * thap(600, 2)(ff)) * (np.clip(tt / 0.004, 0, 1) * np.exp(-tt / gt) * (t >= bd))[:, None]
    ra = ra / (np.max(np.abs(ra)) + 1e-12) + than / (np.max(np.abs(than)) + 1e-12) * 0.9
    return ra / (np.max(np.abs(ra)) + 1e-12) * nang


def ren(n, r, bd, giay, nang=1.0, so_dot=(3, 6), dung_xa=0):
    """SAM REN cuon: on tram (than 150-500 Hz + day 120-200) doi DOT khong deu (3-6 dot cuon), moi dot lan tu mot ben sang ben kia; vang dai.
    dung_xa: so tieng 'dung' xa (doang nho, mo) xen vao."""
    t = np.arange(n) / SR
    ra = np.zeros((n, 2))
    zL = loc2(on2(n, r, 0.3), lambda ff: cao(110, 2)(ff) * thap(520, 2)(ff))
    zL /= np.max(np.abs(zL)) + 1e-12
    tong_bao = np.zeros(n)
    tb0 = bd
    for k in range(int(r.integers(*so_dot))):
        d = r.uniform(0.4, 1.2)
        dot_b = np.clip((t - tb0) / r.uniform(0.05, 0.2), 0, 1) * np.exp(-np.maximum(t - tb0 - 0.1, 0) / (d * 0.5)) * (t >= tb0)
        dot_b *= r.uniform(0.5, 1.0) * (0.85 ** k)
        p0, p1 = r.uniform(-0.8, 0.8), r.uniform(-0.8, 0.8)
        lech = p0 + (p1 - p0) * np.clip((t - tb0) / d, 0, 1)
        g = (lech + 1) * np.pi / 4
        ra[:, 0] += zL[:, 0] * dot_b * np.cos(g) * np.sqrt(2)
        ra[:, 1] += zL[:, 1] * dot_b * np.sin(g) * np.sqrt(2)
        tong_bao += dot_b
        tb0 += r.uniform(0.2, 0.7)
        if tb0 > bd + giay * 0.7: break
    ra *= np.maximum(cham(n, 8, r) * 0.35 + 1, 0.2)[:, None]             # rung lac nhanh cua sam
    # lop "lao xao" trung 400-1200 Hz theo bao (tieng sam co hat)
    hat = xeo(n, r, tong_bao / (np.max(tong_bao) + 1e-12) + 1e-4, 1500, (350, 1100), chung=0.2, giat=20)
    ra = ra / (np.max(np.abs(ra)) + 1e-12) + hat * 0.35
    for _ in range(dung_xa):
        tt = bd + r.uniform(0.3, giay * 0.8)
        x = doang(n, r, tt, r.uniform(0.2, 0.4), r.uniform(0.15, 0.3))
        ra += loc2(x, thap(1500, 2))
    ra *= np.exp(-np.maximum(t - bd - giay * 0.6, 0) / (giay * 0.25))[:, None]
    return ra / (np.max(np.abs(ra)) + 1e-12) * nang


def hoan_tat(tron, r, giay_vang, muc_vang, dinh_db):
    return th.hoan_tat(tron, r, giay_vang, muc_vang, dinh_db)


def tam(tron, k=0.3):
    """Them phan GIUA (trung binh hai kenh) - do lan 1: cac lop rieng tung kenh cho tuong quan 0,01-0,14 (co clip -0,01) = loang, mat tam.
    k 0,3 -> ~0,25."""
    return tron + k * tron.mean(axis=1, keepdims=True)


CAU_HINH = {
    # kieu: (do dai xe, muc xe, muc doang, muc chay, dung_xa ren, xet dien)
    'set_gan': ((0.06, 0.15), 0.6, 1.0, 1.0, 0, False),
    'set_xe_troi': ((0.2, 0.45), 1.0, 0.85, 0.75, 1, False),
    'bao_set': ((0.08, 0.25), 0.8, 0.9, 0.8, 3, True),
}


def danh(hat, kieu):
    dx, m_xe, m_doang, _, _, xet = CAU_HINH[kieu]
    r = np.random.default_rng(hat)
    gx = r.uniform(*dx)
    n = int(SR * (gx + 1.0)); t = np.arange(n) / SR
    tron = xe(n, r, 0.0, gx, m_xe)
    tron += doang(n, r, gx, m_doang)
    if r.random() < 0.5:                       # tia giat lai: "doang" thu hai nho
        tron += doang(n, r, gx + r.uniform(0.04, 0.12), m_doang * r.uniform(0.3, 0.55))
    if xet:
        for _ in range(int(r.integers(2, 6))):
            g4.rai(tron, r, g4.xet(r) * r.uniform(0.3, 0.6), gx + r.uniform(0.02, 0.3))
    tron += xeo(n, r, np.clip((t - gx) / 0.01, 0, 1) * np.exp(-np.maximum(t - gx, 0) / r.uniform(0.1, 0.25)) + 1e-4, 3000, (900, 3200), chung=0.2) * 0.35
    return hoan_tat(tam(tron), r, r.uniform(0.6, 1.0), r.uniform(0.35, 0.5), -2)


def chay_xem(hat, kieu):
    _, _, _, m_chay, _, xet = CAU_HINH[kieu]
    r = np.random.default_rng(hat)
    giay = r.uniform(0.45, 0.85)
    n = int(SR * (giay + 0.6)); t = np.arange(n) / SR
    tron = g4.chay(n, r, 0.0, giay, 1.0)
    # than "lep bep" trung 250-800 Hz (do lan 1: 200 Hz - 1 kHz chi 13-24% - mong, choi)
    bc = np.clip(t / 0.03, 0, 1) * np.exp(-np.maximum(t - giay * 0.3, 0) / (giay * 0.5))
    tron += xeo(n, r, bc + 1e-4, 2500, (250, 800), chung=0.2, giat=20) * 0.8
    # khoi xi bay len sau cho chay
    xi = loc2(on2(n, r, 0.2), lambda ff: cao(900, 2)(ff) * thap(3000, 4)(ff))
    tron += xi / (np.max(np.abs(xi)) + 1e-12) * (np.clip((t - 0.05) / 0.1, 0, 1) * np.exp(-np.maximum(t - 0.15, 0) / (giay * 0.6)))[:, None] * 0.3
    if xet:
        hq = g3.ho_quang(t, r, r.uniform(180, 260), giat=(15, 25), sau=0.6) * np.exp(-t / 0.25)
        tron += dat(hq, r.uniform(-0.5, 0.5)) * 0.25
    return hoan_tat(tam(tron), r, r.uniform(0.4, 0.7), r.uniform(0.3, 0.45), -4) * m_chay


def sam_ren(hat, kieu):
    _, _, _, _, dung_xa, _ = CAU_HINH[kieu]
    r = np.random.default_rng(hat)
    n = int(SR * 4.2)
    tron = ren(n, r, 0.05, 3.6, 1.0, (3, 6) if kieu != 'set_xe_troi' else (4, 7), dung_xa)
    return hoan_tat(tam(tron), r, r.uniform(1.2, 1.8), r.uniform(0.4, 0.55), -3)


MAU = {'A_set_gan': ('set_gan', 16100), 'B_set_xe_troi': ('set_xe_troi', 16300), 'C_bao_set': ('bao_set', 16500)}


def ghep_game(danhs, chays, rens, phat=g3.can_bang):
    """MO PHONG TRONG GAME: nguoi choi giua man, tung Sam set phia truoc lech phai (vung 6 m o ~+15 do): sam ren o tam vung; 22 tia trong 2,4 s
    (0,11 s/tia, cai dau o giua) roi ngau nhien -40..+60 do - moi tia: tieng danh + tieng chay xem cung cho (toi da 4 / 4 cung luc, tia sau x0,6).
    Hai lan tung (0,2 s va 5,2 s)."""
    out = np.zeros((int(SR * 10.0), 2))
    r = np.random.default_rng(7)
    for lan, g in enumerate((0.2, 5.2)):
        tb.cong(out, phat(rens[lan % 3], 15.0) * 0.8, g)
        lich = []
        for i in range(22):
            bd = g + i * 0.11 + (r.uniform(0.04, 0.1) if i else 0)
            az = 15.0 if i == 0 else r.uniform(-40, 60)
            lich.append((bd, az))
        song_danh, song_chay = [], []
        for i, (bd, az) in enumerate(lich):
            song_danh = [x for x in song_danh if x > bd]; song_chay = [x for x in song_chay if x > bd]
            if len(song_danh) < 4:
                x = danhs[r.integers(0, 3)]
                tb.cong(out, phat(x, az) * (1.0 if i == 0 else 0.6), bd); song_danh.append(bd + len(x) / SR * 0.5)
            if len(song_chay) < 4:
                x = chays[r.integers(0, 3)]
                tb.cong(out, phat(x, az) * 0.6, bd + 0.05); song_chay.append(bd + len(x) / SR * 0.6)
    return out / np.max(np.abs(out)) * 0.95



# ================================================================== B2 (nguoi dung chon B "Set xe troi" nhung: "am thanh set con NGAN va MONG qua, chua co
# do DAY nen van con giong am MONO lam - tao lai")
# Do ban B: tieng danh 0,7-1 s co ich (xe 0,2-0,45 s + doang than 80-200 ms), < 200 Hz chi 2-5%, vang 0,6-1,0 s muc 0,35-0,5; sam ren < 200 Hz 5-8%.
# B2: xe DAI 0,4-0,75 s co than tram 200-600 Hz ro; doang than 120-500 Hz tat 0,25-0,5 s; MOI TIA KEM sam ren ngan 1,2-2 s; than < 200 Hz ~12-20%
# (120-250 Hz - van tren nguong cat 120); vang stereo DAY 1,6-2,4 s (b4.vang_day: moi kenh mot xung + lop vang ngan "phong"); NHAN DOI lop chinh
# (ban sao tre 7-15 ms, cao do lech ~2,5%, dao kenh) -> rong; sam ren tran 5 s, than tram day hon, cuon nhieu dot hon.
import tieng_qua_cau_bang_4 as b4


def than_tram(n, r, bd, giay, nang=1.0, f=(120, 260)):
    """Than TRAM cua set / sam: on 120-260 Hz hai kenh 50% chung, len 10-30 ms tat 'giay', rung lac."""
    t = np.arange(n) / SR
    tt = t - bd
    b = np.clip(tt / r.uniform(0.01, 0.03), 0, 1) * np.exp(-np.maximum(tt, 0) / giay) * (tt >= 0) * np.maximum(cham(n, 6, r) * 0.3 + 1, 0.2)
    z = loc2(on2(n, r, 0.5), lambda ff: cao(f[0], 2)(ff) * thap(f[1], 2)(ff))
    return z / (np.max(np.abs(z)) + 1e-12) * b[:, None] * nang


def danh2(hat):
    r = np.random.default_rng(hat)
    gx = r.uniform(0.4, 0.75)
    n = int(SR * (gx + 2.4)); t = np.arange(n) / SR
    tron = xe(n, r, 0.0, gx, 1.0)
    tron += xeo(n, r, (np.clip(t / gx, 0, 1) ** 2) * (t <= gx) + 1e-4, 3000, (200, 600), chung=0.4, giat=30) * 0.7     # than cua tieng xe
    tron += doang(n, r, gx, 1.0, r.uniform(0.25, 0.5))
    if r.random() < 0.6:
        tron += doang(n, r, gx + r.uniform(0.05, 0.15), r.uniform(0.35, 0.6), r.uniform(0.2, 0.35))
    tron += than_tram(n, r, gx, r.uniform(0.25, 0.45), 1.5)     # (do lan 1: x0,9 -> < 200 Hz 7-10%)
    # sam ren ngan theo sau moi tia
    tron += ren(n, r, gx + r.uniform(0.05, 0.15), r.uniform(1.2, 2.0), r.uniform(0.45, 0.7), (2, 4), 0)
    tron += than_tram(n, r, gx + 0.1, r.uniform(0.5, 0.9), 0.9)
    tron += xeo(n, r, np.clip((t - gx) / 0.01, 0, 1) * np.exp(-np.maximum(t - gx, 0) / r.uniform(0.15, 0.3)) + 1e-4, 3000, (900, 3200), chung=0.3) * 0.35
    tron = b4.nhan_doi(tam(tron, 0.25), r)
    tron = b4.vang_day(tron, r, r.uniform(1.6, 2.4), r.uniform(0.5, 0.65))
    return q3.chuan(tron, -2)


def chay2(hat):
    r = np.random.default_rng(hat)
    giay = r.uniform(0.6, 1.1)
    n = int(SR * (giay + 0.6)); t = np.arange(n) / SR
    tron = g4.chay(n, r, 0.0, giay, 1.0)
    bc = np.clip(t / 0.03, 0, 1) * np.exp(-np.maximum(t - giay * 0.3, 0) / (giay * 0.5))
    tron += xeo(n, r, bc + 1e-4, 2500, (250, 800), chung=0.25, giat=20) * 0.9
    xi = loc2(on2(n, r, 0.3), lambda ff: cao(900, 2)(ff) * thap(3000, 4)(ff))
    tron += xi / (np.max(np.abs(xi)) + 1e-12) * (np.clip((t - 0.05) / 0.1, 0, 1) * np.exp(-np.maximum(t - 0.15, 0) / (giay * 0.6)))[:, None] * 0.3
    tron = b4.nhan_doi(tam(tron, 0.25), r)
    tron = b4.vang_day(tron, r, r.uniform(0.9, 1.3), r.uniform(0.35, 0.5))
    return q3.chuan(tron, -4) * 0.8


def ren2(hat):
    r = np.random.default_rng(hat)
    n = int(SR * 5.6)
    tron = ren(n, r, 0.05, 5.0, 1.0, (5, 9), 1)
    tron += than_tram(n, r, 0.08, 1.6, 1.3) + than_tram(n, r, r.uniform(1.0, 2.0), 1.2, 0.8)
    tron = b4.nhan_doi(tam(tron, 0.25), r)
    tron = b4.vang_day(tron, r, r.uniform(1.8, 2.6), r.uniform(0.5, 0.65))
    return q3.chuan(tron, -3)


if __name__ == '__main__' and '--b2' in sys.argv:
    ra = os.path.join('CongCu', 'AmThanh', 'SamSetThu')
    d0 = {'danh': [danh(16300 + 10 * i, 'set_xe_troi') for i in range(3)], 'chay': [chay_xem(16400 + 10 * i, 'set_xe_troi') for i in range(3)],
          'ren': [sam_ren(16500 + 10 * i, 'set_xe_troi') for i in range(3)]}
    bo = {'danh': [danh2(17100 + 10 * i) for i in range(3)], 'chay': [chay2(17200 + 10 * i) for i in range(3)], 'ren': [ren2(17300 + 10 * i) for i in range(3)]}
    def do_day(x):
        m = x.mean(axis=1); e = np.sqrt(np.convolve(m ** 2, np.ones(441) / 441, 'same'))
        dinh = e.max(); dai = np.sum(e > dinh * 10 ** (-20 / 20)) / SR      # thoi gian tren muc -20 dB so voi dinh
        return dai, np.sqrt(np.mean(m ** 2)) / (np.max(np.abs(m)) + 1e-12)
    for loai in ('danh', 'chay', 'ren'):
        for i, x in enumerate(bo[loai]):
            tb.ghi(os.path.join(ra, 'B2_%s%d.wav' % (loai, i + 1)), x)
            th.do('B2_%s%d' % (loai, i + 1), x)
        print('   %s: chenh bien the %s dB; game can bang %s; thoi gian tren -20 dB %s s (ban B %s); RMS/dinh %s (ban B %s)' % (loai,
              ' / '.join('%.1f' % c for c in th.chenh_bien_the(bo[loai])), ' / '.join('%.2f' % g3.tq(g3.can_bang(x, 0)) for x in bo[loai]),
              ' / '.join('%.2f' % do_day(x)[0] for x in bo[loai]), ' / '.join('%.2f' % do_day(x)[0] for x in d0[loai]),
              ' / '.join('%.2f' % do_day(x)[1] for x in bo[loai]), ' / '.join('%.2f' % do_day(x)[1] for x in d0[loai])))
    g = ghep_game(bo['danh'], bo['chay'], bo['ren'])
    tb.ghi(os.path.join(ra, 'B2_ghep_nhu_trong_game.wav'), g)
    th.do('B2_ghep_game', g)
    sys.exit(0)

if __name__ == '__main__':
    ra = sys.argv[1] if len(sys.argv) > 1 else os.path.join('CongCu', 'AmThanh', 'SamSetThu')
    os.makedirs(ra, exist_ok=True)
    chi = sys.argv[2:]
    for ten, (kieu, hat0) in MAU.items():
        if chi and not any(c in ten for c in chi): continue
        bo = {'danh': [danh(hat0 + 10 * i, kieu) for i in range(3)], 'chay': [chay_xem(hat0 + 100 + 10 * i, kieu) for i in range(3)],
              'ren': [sam_ren(hat0 + 200 + 10 * i, kieu) for i in range(3)]}
        for loai, ds in bo.items():
            for i, x in enumerate(ds):
                tb.ghi(os.path.join(ra, '%s_%s%d.wav' % (ten, loai, i + 1)), x)
                th.do('%s_%s%d' % (ten, loai, i + 1), x)
            print('   %s: chenh bien the %s dB; 30ms/300ms %s; game can bang %s | DOI CHUNG 3D spread 120: %s' % (loai,
                  ' / '.join('%.1f' % c for c in th.chenh_bien_the(ds)),
                  ' / '.join('%.2f' % (np.sum(np.abs(x).max(axis=1)[:1323] ** 2) / np.sum(np.abs(x).max(axis=1)[:13230] ** 2)) for x in ds),
                  ' / '.join('%.2f' % g3.tq(g3.can_bang(x, 0)) for x in ds), ' / '.join('%.2f' % g3.tq(g3.unity(x, 0, 120)) for x in ds)))
        g = ghep_game(bo['danh'], bo['chay'], bo['ren'])
        tb.ghi(os.path.join(ra, '%s_ghep_nhu_trong_game.wav' % ten), g)
        th.do('%s_ghep_game' % ten, g)
        gd = ghep_game(bo['danh'], bo['chay'], bo['ren'], phat=lambda x, az: g3.unity(x, az, 120))
        print('   ghep DOI CHUNG phat 3D spread 120: tq %.2f' % g3.tq(gd))
