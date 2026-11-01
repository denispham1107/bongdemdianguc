# -*- coding: utf-8 -*-
# TIENG CU LUOT BO XUONG (01/11/2026, nguoi dung chon "lam 3 mau de nghe chon").
# Hai tieng, PHAT THEO SU KIEN (khong mot clip keo gian - cu luot dai ngan tuy khoang cach):
#   lao  (luc bat dau luot) - gio than lao toi ~0,25-0,4 s + xuong khua + lop dac trung cua tung mau
#   toi  (luc cham muc tieu) - cu dap nho vao than / vom, xuong lach cach, bui
# Lam theo memory quy-trinh-tieng-ky-nang-moi: moi lop rieng tung kenh + phan chung, am tram cung pha, su kien nho lech CHI bang am luong, 3 bien the
# KHAC CAU TRUC, dai dien thoai, khong mo dau bang mot xung bao hoa + rem (bay "tieng sung"), khong truot cao do nhanh (bay "laser").
#
# Ba MAU:
#   A "hon_lao"  - hon ma: gio co hoi lanh hu mem (cong huong hep theo toc do) + xuong khua nhe; toi = huych mem + xuong rac.
#   B "xuong_kiem" - xuong + kim loai: xuong khua day, luoi kiem re gio + cha; toi = "cop" kiem cung, xuong do xuong.
#   C "bui_dat"  - bat dat: chan dap dat lay da, soi tung, gio day; toi = huych nang + soi bat.
#
# Chay: python CongCu/AmThanh/tieng_luot_bo_xuong.py [thu_muc_ra]   (mac dinh CongCu/AmThanh/LuotBoXuongThu)

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
loc, dai, thap, cao, cham = q3.loc, q3.dai, q3.thap, q3.cao, q3.cham
on2, loc2, quet2, keo_lech, bao_len_xuong = th.on2, th.loc2, th.quet2, th.keo_lech, th.bao_len_xuong


def xuong_khua(n, r, bd, giay, mat_do, nang=1.0, f=(900, 2800)):
    """XUONG khua vao nhau: hat "cach" kho tat 1-4 ms, co dai cong huong go/xuong, moi hat mot cho (lech chi bang am luong), mat do giat theo nhip chay."""
    t = np.arange(n) / SR
    e = np.clip((t - bd) / 0.01, 0, 1) * np.exp(-np.maximum(t - bd, 0) / giay)
    e = e * np.maximum(0.25 + 0.75 * (0.5 + 0.5 * np.sin(2 * np.pi * r.uniform(11, 17) * t + r.uniform(0, 6))), 0)
    vt = np.nonzero(r.random(n) < e * mat_do / SR)[0]
    ra = np.zeros((n, 2))
    nhan = []
    for _ in range(8):
        d = int(r.uniform(0.002, 0.007) * SR)
        fc = np.exp(r.uniform(np.log(f[0]), np.log(f[1])))
        k = loc(np.pad(r.standard_normal(d) * np.exp(-np.arange(d) / (d / 2.5)), (0, 300)), dai(fc, 4.5))
        nhan.append(k / (np.max(np.abs(k)) + 1e-12))
    for i in vt:
        k = nhan[r.integers(0, 8)]
        a = np.exp(r.uniform(np.log(0.2), 0))
        g = (r.uniform(-0.8, 0.8) + 1) * np.pi / 4
        m = min(len(k), n - i)
        ra[i:i + m, 0] += k[:m] * a * np.cos(g)
        ra[i:i + m, 1] += k[:m] * a * np.sin(g)
    return ra / (np.max(np.abs(ra)) + 1e-12) * nang


def hu_gio(n, r, bao, f0, f1, q=14.0):
    """Hoi lanh hu: dai cong huong hep tam theo bao (nhanh -> cao), hai kenh lech tan so - gio that, khong phai ong sao (on lam nguon)."""
    x = np.zeros((n, 2))
    fc = f0 + (f1 - f0) * bao / (np.max(bao) + 1e-12)
    y = on2(n, r, 0.25)
    # chia khuc 12 ms, moi khuc mot bo loc - du min cho cu luot 0,4 s
    b = int(0.012 * SR)
    for k in range(2):
        tam = fc * (1.0 + (0.03 if k else -0.03))
        for i in range(0, n, b):
            j = min(n, i + b * 2)
            kh = y[i:j, k] * np.hanning(j - i)
            ff = dai(float(tam[min(n - 1, i + b // 2)]), q)
            x[i:j, k] += loc(kh, ff)
    x = x / (np.max(np.abs(x)) + 1e-12)
    return x * bao[:, None]


def re_kiem(n, r, bao, f=(1500, 3400)):
    """Luoi kiem RE gio + rit nhe kim loai: on dai cao, bien do rung nhanh theo luoi - khong cao do truot."""
    x = loc2(on2(n, r, 0.35), lambda ff: cao(f[0], 2)(ff) * thap(f[1], 3)(ff))
    x *= np.stack([np.maximum(1 + 0.55 * cham(n, 70, r), 0.1) for _ in range(2)], axis=1)
    return x / (np.max(np.abs(x)) + 1e-12) * bao[:, None]


CAU_HINH = {
    #            gio    hu    xuong(md, muc) kiem  chan  soi    huych  toc(muc, dai)        rac    vien
    'hon_lao':    dict(gio=0.75, hu=0.8, xmd=160, x=0.4, kiem=0.0, chan=0.0, soi=0.12, huych=0.75, toc=0.45, f_toc=(280, 1100), rac=0.7, vien=(0, 2)),
    'xuong_kiem': dict(gio=0.7, hu=0.0, xmd=520, x=0.85, kiem=0.6, chan=0.0, soi=0.2, huych=0.7, toc=1.0, f_toc=(650, 2300), rac=1.05, vien=(1, 3)),
    'bui_dat':    dict(gio=1.0, hu=0.0, xmd=220, x=0.4, kiem=0.0, chan=0.9, soi=0.8, huych=1.15, toc=0.65, f_toc=(320, 1300), rac=0.55, vien=(3, 6)),
}


def lao(hat, kieu):
    c = CAU_HINH[kieu]; r = np.random.default_rng(hat)
    dinh = r.uniform(0.07, 0.15)
    dai_ = dinh + r.uniform(0.16, 0.34)
    n = int(SR * (dai_ + 0.3)); t = np.arange(n) / SR
    bao = np.clip(t / dinh, 0, 1) ** 1.3 * np.exp(-np.maximum(t - dinh, 0) / ((dai_ - dinh) / 2.0))
    tron = nl.gio_than(n, r, bao, r.uniform(320, 460), r.uniform(1500, 2400)) * c['gio']
    if c['hu'] > 0:
        tron += hu_gio(n, r, bao, r.uniform(380, 520), r.uniform(800, 1250)) * c['hu']
    tron += xuong_khua(n, r, r.uniform(0.0, 0.03), dai_ * 0.55, c['xmd'] * r.uniform(0.7, 1.3), c['x'])
    if c['kiem'] > 0:
        tron += re_kiem(n, r, bao ** 1.4) * c['kiem']
    if c['chan'] > 0:
        # chan dap lay da (1-2 nhip) + soi tung sau gio
        for j in range(int(r.integers(1, 3))):
            bd = j * r.uniform(0.04, 0.08) + r.uniform(0.0, 0.015)
            tron += nl.huych(n, r, bd, r.uniform(270, 320), r.uniform(175, 200), r.uniform(0.02, 0.035), 0.55 * c['chan'] * (1.0 if j == 0 else 0.7))
    if c['soi'] > 0:
        tron += nl.soi(n, r, 0.01, dai_ * 0.6, 1500 * r.uniform(0.7, 1.3), c['soi'] * 1.5)
    if r.random() < 0.5 and c['vien'][1] > 0:
        nl.vien_soi(tron, r, int(r.integers(1, 3)), 0.03, dai_, 0.3)
    tron = ss.tam(tron, 0.05)
    return th.hoan_tat(tron, r, r.uniform(0.35, 0.6), r.uniform(0.12, 0.2), -4)


def toi(hat, kieu):
    c = CAU_HINH[kieu]; r = np.random.default_rng(hat)
    n = int(SR * 0.75); t = np.arange(n) / SR
    tron = np.zeros((n, 2))
    # lao vao mem 5-12 ms roi moi cham: khong co xung bao hoa dau tien kieu tieng sung
    bd = r.uniform(0.005, 0.012)
    tron += nl.huych(n, r, bd, r.uniform(250, 310), r.uniform(165, 195), r.uniform(0.035, 0.065), c['huych'])
    tron += b3.toc(t, r, bd + r.uniform(0.0, 0.004), 0.75 * c['toc'], c['f_toc'], r.uniform(0.012, 0.024))
    # xuong do xuong lach cach sau cu cham: co / khong dot hai
    tron += xuong_khua(n, r, bd + r.uniform(0.01, 0.03), r.uniform(0.07, 0.13), 900 * r.uniform(0.6, 1.3), c['rac'])
    if r.random() < 0.55:
        tron += xuong_khua(n, r, r.uniform(0.09, 0.17), r.uniform(0.03, 0.07), 500 * r.uniform(0.6, 1.2), c['rac'] * 0.55)
    if c['soi'] > 0.3:
        tron += nl.soi(n, r, bd + 0.003, r.uniform(0.07, 0.14), 1400 * r.uniform(0.7, 1.3), c['soi'] * 1.2)
    tron += nl.cha_dat(n, r, bd, r.uniform(0.05, 0.11), 0.35, (420, 2800), r.uniform(28, 45))
    v0, v1 = c['vien']
    if v1 > 0:
        nl.vien_soi(tron, r, int(r.integers(v0, v1 + 1)), 0.015, 0.16, 0.35)
    tron = ss.tam(tron, 0.05)
    return th.hoan_tat(tron, r, r.uniform(0.35, 0.6), r.uniform(0.14, 0.22), -3)


MAU = {'A_hon_lao': ('hon_lao', 24100), 'B_xuong_kiem': ('xuong_kiem', 24300), 'C_bui_dat': ('bui_dat', 24500)}


def ghep_game(laos, tois, phat=g3.can_bang):
    """MO PHONG TRONG GAME (phat can bang): Bo xuong lao tu trai 45 do vao mat (3,5 m = 0,2 s), tu phai vao, hai con cung luc gan nhau, roi mot con ngoai xa."""
    out = np.zeros((int(SR * 6.0), 2))
    lich = [(0.3, 0.2, -45, 0, 1.0), (1.8, 0.14, 40, 5, 1.0), (3.3, 0.2, -30, 0, 0.9), (3.42, 0.17, 25, 0, 0.75), (4.9, 0.2, 70, 55, 0.5)]
    for k, (bd, keo, az0, az1, a) in enumerate(lich):
        v = laos[k % 3]
        tb.cong(out, phat(v, np.linspace(az0, az1, len(v))) * a, bd)
        tb.cong(out, phat(tois[k % 3], az1) * a, bd + keo)
    return out / np.max(np.abs(out)) * 0.95


if __name__ == '__main__':
    ra = sys.argv[1] if len(sys.argv) > 1 else os.path.join('CongCu', 'AmThanh', 'LuotBoXuongThu')
    os.makedirs(ra, exist_ok=True)
    for ten, (kieu, hat0) in MAU.items():
        bo = {'lao': [lao(hat0 + 10 * i, kieu) for i in range(3)], 'toi': [toi(hat0 + 100 + 10 * i, kieu) for i in range(3)]}
        for loai, ds in bo.items():
            for i, x in enumerate(ds):
                tb.ghi(os.path.join(ra, '%s_%s%d.wav' % (ten, loai, i + 1)), x)
                th.do('%s_%s%d' % (ten, loai, i + 1), x)
                # bay "tieng sung": nang luong 30 ms dau / 300 ms dau
                e = np.sum(x ** 2, axis=1)
                print('      sung %.2f' % (e[:int(0.03 * SR)].sum() / (e[:int(0.3 * SR)].sum() + 1e-12)))
            print('   chenh bien the %s: %s dB; can bang giua tq %s' % (loai, ' / '.join('%.1f' % c for c in th.chenh_bien_the(ds)),
                  ' / '.join('%.2f' % g3.tq(g3.can_bang(x, 0)) for x in ds)))
        g = ghep_game(bo['lao'], bo['toi'])
        tb.ghi(os.path.join(ra, '%s_ghep_nhu_trong_game.wav' % ten), g)
        th.do('%s_ghep' % ten, g)
