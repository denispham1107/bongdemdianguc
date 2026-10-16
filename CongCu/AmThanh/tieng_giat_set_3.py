# -*- coding: utf-8 -*-
# TIENG GIUT SET + QUA CAU DIEN - BAN 3 (16/10/2026). Nguoi dung vao game nghe B2 "zzzt" + vong lap B3: "van con QUA MONO, khong phai dang STEREO
# CHAN THAT - tao lai toan bo".
# Do: clip B2 / B3 tuong quan 0,17-0,25 nhung TRONG GAME (menu 129) loa 0,54 / 0,49. Mo phong Unity (moi kenh cua clip dat o goc +-spread/2 roi pan
# equal-power) khop: spread 120 day 0,17 -> 0,38 · 0,25 -> 0,44. Va "zzzt" la am CO CAO DO: 2-3 ho quang lech nhau 3-8% cao do hoa thanh MOT diem
# giua dau du tuong quan thap.
# Ban 3 GIU chat "zzzt" (nguoi dung chon) nhung STEREO THAT:
#   - HAI BEN KHAC HAN: ho quang trai va phai cao do lech 12-25% + am sac (dai cong huong) khac + nhip giat RIENG tung kenh -> do lech am luong
#     hai tai doi lien tuc (ILD) - tai nghe ra hai nguon tach biet; ho quang giua chi ~35%;
#   - TIA XET RAI RAC: nhieu "xet" ngan (10-40 ms) moi cai mot cho trai / phai (chi lech am luong);
#   - CHUYEN DONG: ho quang chay qua lai trai <-> phai (mau "chay");
#   - vang moi kenh mot xung.
# Trong game kem: tieng PHONG chay doc tia tu tay toi muc tieu; spread rong hon cho phong + vong lap (mo phong Unity: 150 -> ~0,2).
# Ba MAU (cung chat zzzt):
#   S1 "hai_ben"  - hai ho quang tach trai / phai + xet rai rac, dung yen.
#   S2 "chay"     - cac ho quang CHAY qua lai trai <-> phai 2-5 lan / giay (dong dien chay doc tia).
#   S3 "bao_quanh" - ho quang hai ben + lop xet / vang bao quanh rong (khong gian), tam giua nhe.
#
# Chay: python CongCu/AmThanh/tieng_giat_set_3.py [thu_muc_ra] [loc_ten_mau...]   (mac dinh CongCu/AmThanh/GiatSetThu3)

import os, sys
import numpy as np
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import tieng_bam as tb
import tieng_qua_cau_lua_3 as q3
import tieng_tang_hinh as th
import tieng_giat_set as g1
import tieng_giat_set_2 as g2

SR = tb.SR
PI2 = tb.PI2
GIAY_VONG = 3.0
loc, dai, thap, cao, cham = q3.loc, q3.dai, q3.thap, q3.cao, q3.cham
on2, loc2, keo_lech, xoay, bao_len_xuong = th.on2, th.loc2, th.keo_lech, th.xoay, th.bao_len_xuong
xeo = g1.xeo


def ho_quang(t, r, f0, vong=False, giat=(8, 14), sau=0.3, bao_hoa=1.25):
    """MOT ho quang don (mono): rang cua f0 qua hai dai cong huong ngau nhien, bien do giat deu (khong re). Vong lap: so chu ky nguyen."""
    n = len(t)
    tan = f0 * (1 + 0.006 * cham(n, 8, r))
    if vong:
        tong = np.sum(tan) / SR; tan = tan * max(1, round(tong)) / tong
    pha = PI2 * np.cumsum(tan) / SR + r.uniform(0, PI2)
    x = sum(np.sin(h * pha) / h ** 0.85 for h in range(1, 24) if f0 * h < 4300)
    f1, f2 = r.uniform(380, 750), r.uniform(1100, 2300)
    x = loc(x, lambda ff: 1.1 * dai(f1, 2.0)(ff) + r.uniform(0.45, 0.9) * dai(f2, 2.5)(ff) + 0.15)
    x = np.tanh(bao_hoa * x / (np.max(np.abs(x)) + 1e-12))
    g = np.maximum(cham(n, r.uniform(*giat), r), -1.5)
    return x * np.clip(1 - sau + sau * (0.5 + 0.5 * np.tanh(2 * g)), 0, 1)


def dat(x, lech):
    """Mono -> stereo, lech (so hoac mang -1..1) CHI bang am luong."""
    g = (np.clip(lech, -1, 1) + 1) * np.pi / 4
    return np.stack([x * np.cos(g), x * np.sin(g)], axis=1)


def zzz_stereo(t, r, f0, bao, kieu, vong=False, giua=0.35):
    """'zzzt' STEREO THAT: ho quang TRAI va PHAI cao do lech 12-25% (khac han), giat rieng; ho quang giua nho. kieu 'hai_ben' dung yen,
    'chay' chay qua lai, 'bao_quanh' hai ben + rong."""
    n = len(t)
    lech_cd = r.uniform(0.12, 0.25)
    fL, fR = f0 * (1 - lech_cd / 2), f0 * (1 + lech_cd / 2)
    if r.random() < 0.5: fL, fR = fR, fL
    ra = np.zeros((n, 2))
    if kieu == 'chay':
        hz = r.uniform(2, 5) if not vong else round(r.uniform(2, 4) * GIAY_VONG) / GIAY_VONG
        for j, f in enumerate((fL, fR, f0 * r.uniform(1.3, 1.45))):
            pha = r.uniform(0, PI2) + j * PI2 / 3
            lech = 0.9 * np.sin(PI2 * hz * (1 + 0.15 * j * (not vong)) * t + pha)
            ra += dat(ho_quang(t, r, f, vong), lech) * (1.0 if j < 2 else 0.6)
    else:
        rong = 0.9 if kieu == 'bao_quanh' else 0.8
        ra += dat(ho_quang(t, r, fL, vong), -rong) * 1.0
        ra += dat(ho_quang(t, r, fR, vong), rong) * 1.0
        ra += dat(ho_quang(t, r, f0, vong), r.uniform(-0.15, 0.15)) * giua
    ra /= np.max(np.abs(ra)) + 1e-12
    return ra * bao[:, None]


def xet_rai(n, r, bao, mat_do, rong=1.0, vong=False, f0=(160, 260)):
    """TIA XET rai rac: moi xet 10-40 ms (ho quang ngan + chut xeo), moi cai MOT CHO trai / phai."""
    ra = np.zeros((n, 2))
    p = bao * mat_do / SR
    vt = np.nonzero(r.random(n) < p)[0]
    for i in vt:
        d = r.uniform(0.01, 0.04); m = int((d + 0.02) * SR); tm = np.arange(m) / SR
        bz = np.clip(tm / 0.003, 0, 1) * np.clip((d - tm) / 0.006, 0, 1)
        z = ho_quang(tm, r, r.uniform(*f0), giat=(20, 30), sau=0.4) * bz * r.uniform(0.3, 1.0)
        s2 = dat(z, np.clip(r.uniform(-1, 1) * rong, -1, 1))
        idx = i + np.arange(m)
        if vong: idx %= n
        else:
            ok = idx < n; idx = idx[ok]; s2 = s2[ok]
        ra[idx] += s2
    return ra


def vang_rieng(tron, r, giay, muc):
    return q3.vang_uot(tron, giay, r, muc)


def hoan_tat(tron, r, giay_vang, muc_vang, dinh_db):
    return th.hoan_tat(tron, r, giay_vang, muc_vang, dinh_db)


def phong(hat, kieu):
    r = np.random.default_rng(hat)
    giu = r.uniform(0.45, 0.9)
    n = int(SR * (giu + 0.7)); t = np.arange(n) / SR
    tat = r.uniform(0.015, 0.025)
    b = np.clip(t / r.uniform(0.015, 0.025), 0, 1) * np.clip((giu - t) / tat, 0, 1)
    if r.random() < 0.5:
        b = b * (0.75 + 0.25 * np.clip(t / giu, 0, 1))
    tron = zzz_stereo(t, r, r.uniform(140, 200), b, kieu)
    tron += xet_rai(n, r, b, r.uniform(10, 25), 1.0) * 0.5
    tron += xeo(n, r, b, r.uniform(500, 900), chung=0.1, giat=15) * 0.08
    if kieu == 'bao_quanh':
        tron += loc2(on2(n, r, 0.1), lambda ff: cao(1500, 2)(ff) * thap(3600, 4)(ff)) * (b * 0.15)[:, None]
    return hoan_tat(tron, r, r.uniform(0.5, 0.8) * (1.4 if kieu == 'bao_quanh' else 1), r.uniform(0.35, 0.5), -3)


def trung(hat, kieu):
    r = np.random.default_rng(hat)
    giu = r.uniform(0.2, 0.4)
    n = int(SR * (giu + 0.6)); t = np.arange(n) / SR
    b = np.clip(t / 0.012, 0, 1) * np.clip((giu - t) / 0.02, 0, 1)
    tron = zzz_stereo(t, r, r.uniform(170, 240), b, kieu)
    tron += xet_rai(n, r, b, r.uniform(15, 35), 1.0) * 0.55
    tron += xeo(n, r, bao_len_xuong(t, 0.0, 0.01, r.uniform(0.08, 0.2)), r.uniform(1000, 1800), (900, 3000), chung=0.1, giat=15) * 0.2
    return hoan_tat(tron, r, r.uniform(0.4, 0.7), r.uniform(0.3, 0.45), -4)


def lan(hat, kieu):
    r = np.random.default_rng(hat)
    giu = r.uniform(0.12, 0.25)
    n = int(SR * (giu + 0.45)); t = np.arange(n) / SR
    b = np.clip(t / 0.01, 0, 1) * np.clip((giu - t) / 0.018, 0, 1)
    tron = zzz_stereo(t, r, r.uniform(180, 260), b, kieu, giua=0.25)
    tron += xet_rai(n, r, b, r.uniform(15, 30), 1.0) * 0.4
    tron = keo_lech(tron, np.linspace(r.uniform(-0.4, 0.4), r.uniform(-0.4, 0.4), n))
    return hoan_tat(tron, r, r.uniform(0.3, 0.5), r.uniform(0.25, 0.4), -5)


def cau(hat, kieu):
    """Vong lap Qua cau dien 3 s: giu cau truc B3 (than tram hai nguon, 'sss' min) nhung ho quang hai ben khac han + xet rai rac + (S2) chay vong."""
    r = np.random.default_rng(hat)
    n = int(SR * GIAY_VONG); t = np.arange(n) / SR
    bung = np.maximum(cham(n, 1.0, r) * 0.3 + 0.9, 0.35) ** 1.5
    tron = zzz_stereo(t, r, 165.0, bung, kieu, vong=True, giua=0.25) * 1.0
    if kieu != 'chay':      # them mot cap ho quang thu hai (cao hon) cung tach hai ben
        tron += zzz_stereo(t, r, 228.0, bung * np.maximum(cham(n, 0.8, r) * 0.3 + 0.8, 0.2), 'hai_ben', vong=True, giua=0.0) * 0.6
    for f0, lech in ((141.0, -0.7), (156.0, 0.7)):        # than tram hai nguon (chu ky nguyen trong vong)
        pha = PI2 * f0 * t + r.uniform(0, PI2)
        than = loc(sum(np.sin(h * pha + 0.3 * h) / h for h in range(1, 7)), thap(700, 2)) * bung * np.maximum(cham(n, 1.5, r) * 0.25 + 1, 0.3)
        tron += dat(than / (np.max(np.abs(than)) + 1e-12), lech) * 0.7
    tron += xet_rai(n, r, bung, 18, 1.0, vong=True) * 0.5
    sss = loc2(on2(n, r, 0.1), lambda ff: cao(1400, 2)(ff) * thap(3400, 4)(ff))
    sss *= (np.maximum(cham(n, 3, r) * 0.4 + 0.8, 0.1) * bung)[:, None]
    tron += sss / (np.max(np.abs(sss)) + 1e-12) * (1.3 if kieu == 'bao_quanh' else 0.9)
    m = int(1.0 * SR); tm = np.arange(m) / SR
    for k in range(2):
        h = loc(r.standard_normal(m), lambda f: cao(200, 2)(f) * thap(4000, 2)(f)) * np.exp(-tm / (0.3 if kieu == 'bao_quanh' else 0.2))
        h[:int(0.015 * SR)] = 0
        h /= np.sqrt(np.sum(h ** 2)) + 1e-12
        hh = np.zeros(n); hh[:m] = h
        tron[:, k] += np.fft.irfft(np.fft.rfft(tron[:, k]) * np.fft.rfft(hh), n) * 0.7
    return q3.chuan(tron, -5, vong=True)


MAU = {'S1_hai_ben': ('hai_ben', 14100), 'S2_chay': ('chay', 14300), 'S3_bao_quanh': ('bao_quanh', 14500)}


# ------------------------------------------------------------------ MO PHONG UNITY (tieng 3D, clip stereo, spread)
def unity(x, az, spread):
    """Tieng phat trong game: moi kenh clip dat o goc az -/+ spread/2 (do; az co the la mang theo thoi gian), pan equal-power theo sin goc."""
    out = np.zeros_like(x)
    az = np.broadcast_to(np.asarray(az, dtype=float), (len(x),)) if np.ndim(az) else np.full(len(x), float(az))
    for k, lech in ((0, -spread / 2), (1, spread / 2)):
        g = np.clip(np.sin(np.radians(az + lech)), -1, 1); thg = (g + 1) * np.pi / 4
        out[:, 0] += x[:, k] * np.cos(thg); out[:, 1] += x[:, k] * np.sin(thg)
    return out


def can_bang(x, az, spread=None):
    """Cach phat MOI de xuat (Editor / may tinh: AudioSource 2D + panStereo): giu NGUYEN hai kenh clip, chi ha kenh phia xa (can bang) theo
    sin(goc) - stereo cua clip con nguyen o moi goc, van nghe ro ben. (Unity 3D spread 120: o giua bop clip 0,17 -> 0,38, o +40 do ben phai
    chi x1,07; web PannerNode equalpower: giua giu 0,17 nhung +40 do gop kenh trai vao phai -> 0,75.) Tham so spread bo qua (giu chu ky goi)."""
    p = np.sin(np.radians(np.clip(np.broadcast_to(np.asarray(az, dtype=float), (len(x),)), -90, 90)))
    gL = np.where(p > 0, np.cos(p * np.pi / 2), 1.0)
    gR = np.where(p < 0, np.cos(-p * np.pi / 2), 1.0)
    return np.stack([x[:, 0] * gL, x[:, 1] * gR], axis=1)


def tq(x, a=None, b=None):
    l, rr = x[:, 0], x[:, 1]
    if a is not None:
        l = loc(l, lambda f: cao(a, 2)(f) * thap(b, 2)(f)); rr = loc(rr, lambda f: cao(a, 2)(f) * thap(b, 2)(f))
    return float(np.sum(l * rr) / np.sqrt(np.sum(l * l) * np.sum(rr * rr) + 1e-20))


SPREAD_PHONG, SPREAD_TRUNG, SPREAD_CAU = 150.0, 150.0, 150.0   # (spread 120 cho trung / lan: mo phong game 0,32-0,61)


def ghep_game(phongs, trungs, lans, vong_cau, spread_phong=SPREAD_PHONG, spread_trung=SPREAD_TRUNG, spread_cau=SPREAD_CAU, chay_doc_tia=True, phat=None):
    """MO PHONG TRONG GAME (nguoi choi giua man, nhin xuong): 0-4 s hai lan Giut set - phong o tay CHAY doc tia toi ke dau (0,2 s), 3 ke o -40 / 0 / +40 do,
    lan ra xa hon; 4-9 s Qua cau dien o +25 do: vong lap + moi 0,4 s tieng lan o qua cau + trung o 2-4 ke quanh no."""
    out = np.zeros((int(SR * 10.0), 2))
    r = np.random.default_rng(5)
    unity = phat if phat is not None else globals()['unity']
    for lan_tung, g in enumerate((0.2, 2.2)):
        x = phongs[lan_tung % 3]
        az_dich = (-40.0, 0.0, 40.0)[lan_tung]
        az = np.clip(np.arange(len(x)) / (0.2 * SR), 0, 1) * az_dich if chay_doc_tia else 0.0
        tb.cong(out, unity(x, az, spread_phong), g)
        for k, a0 in enumerate((-40.0, 0.0, 40.0)):
            tb.cong(out, unity(trungs[(k + lan_tung) % 3], a0, spread_trung) * 0.8, g + 0.01)
            for h in range(4):
                tb.cong(out, unity(lans[(k + h) % 3], np.clip(a0 * (1 + 0.3 * (h + 1)), -85, 85), spread_trung) * (0.6 * 0.85 ** h), g + 0.01 + 0.07 * (h + 1))
    vong = np.vstack([vong_cau] * 2)[:int(5.0 * SR)]
    vong = vong * np.minimum(1, np.arange(len(vong)) / (0.1 * SR))[:, None] * np.minimum(1, (len(vong) - np.arange(len(vong))) / (0.15 * SR))[:, None]
    tb.cong(out, unity(vong, 25.0, spread_cau) * 0.9, 4.3)
    for i in range(11):
        g = 4.4 + 0.4 * i
        tb.cong(out, unity(lans[i % 3], 25.0, spread_trung) * 0.45, g)
        for j in range(int(r.integers(2, 5))):
            tb.cong(out, unity(trungs[(i + j) % 3], r.uniform(-20, 70), spread_trung) * 0.45, g + 0.01)
    return out / np.max(np.abs(out)) * 0.95


if __name__ == '__main__':
    ra = sys.argv[1] if len(sys.argv) > 1 else os.path.join('CongCu', 'AmThanh', 'GiatSetThu3')
    os.makedirs(ra, exist_ok=True)
    chi = sys.argv[2:]
    # doi chung: B2 / B3 dang trong game, mo phong Unity spread 120 o giua
    d2 = os.path.join('CongCu', 'AmThanh', 'GiatSetThu2')
    def doc(p):
        import wave
        w = wave.open(p); return np.frombuffer(w.readframes(w.getnframes()), '<i2').reshape(-1, 2) / 32768
    for f in ('B2_zzzt_phong1', 'B2_zzzt_trung1', 'B3_cau'):
        x = doc(os.path.join(d2, f + '.wav'))
        print('DOI CHUNG %-16s clip %.2f (dai zzz 150-1000 Hz %.2f) | game spread 120: %.2f, 150: %.2f' % (f, tq(x), tq(x, 150, 1000), tq(unity(x, 0, 120)), tq(unity(x, 0, 150))))
    for ten, (kieu, hat0) in MAU.items():
        if chi and not any(c in ten for c in chi): continue
        bo = {'phong': [phong(hat0 + 10 * i, kieu) for i in range(3)], 'trung': [trung(hat0 + 100 + 10 * i, kieu) for i in range(3)],
              'lan': [lan(hat0 + 200 + 10 * i, kieu) for i in range(3)]}
        vc = cau(hat0 + 300, kieu)
        for loai, ds in bo.items():
            for i, x in enumerate(ds):
                tb.ghi(os.path.join(ra, '%s_%s%d.wav' % (ten, loai, i + 1)), x)
                th.do('%s_%s%d' % (ten, loai, i + 1), x)
            sp = SPREAD_PHONG if loai == 'phong' else SPREAD_TRUNG
            print('   %s: chenh bien the %s dB; tq dai zzz %s; game spread %d: %s' % (loai, ' / '.join('%.1f' % c for c in th.chenh_bien_the(ds)),
                  ' / '.join('%.2f' % tq(x, 150, 1000) for x in ds), sp, ' / '.join('%.2f' % tq(unity(x, 0, sp)) for x in ds)))
        tb.ghi(os.path.join(ra, '%s_cau.wav' % ten), vc)
        th.do('%s_cau' % ten, vc)
        dd = np.abs(np.diff(vc[:, 0]))
        print('   cau: tq dai zzz %.2f; game spread %d: %.2f; noi %.4f (buoc quanh %.4f / %.4f)' % (tq(vc, 150, 1000), SPREAD_CAU, tq(unity(vc, 0, SPREAD_CAU)),
              abs(vc[0, 0] - vc[-1, 0]), dd[-1], dd[0]))
        g = ghep_game(bo['phong'], bo['trung'], bo['lan'], vc, phat=can_bang)
        tb.ghi(os.path.join(ra, '%s_ghep_nhu_trong_game.wav' % ten), g)
        th.do('%s_ghep_game' % ten, g)
