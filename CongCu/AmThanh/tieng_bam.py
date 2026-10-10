# -*- coding: utf-8 -*-
# TIENG BAM NUT (10/10/2026, nguoi dung): moi lan bam nut o dang nhap, dang ky, sanh / phong va nut giao dien trong tran.
# HAI tieng (nguoi dung chon): "thuong" cho moi nut + "xac nhan" nang hon cho nut chinh (VAO GAME, TAO TAI KHOAN, OK, TAO PHONG,
# SAN SANG, MO KHOA / NANG CAP). STEREO (quy tac nguoi dung: moi am thanh stereo, khong mono) - hai kenh vang khac nhau.
#
# Ba MAU de nguoi dung nghe chon (moi mau mot cap thuong + xac nhan):
#   A "da_mo"   - go vao bia da: tieng thup + cong huong phien da + vun da lach cach, vang ham mo. Xac nhan: cu go nang + chuong Re4.
#   B "xuong"   - tieng xuong kho go nhau (hai nhip sat), hoi tho thi tham rat ngan. Xac nhan: xuong gay + nhip trong tram.
#   C "chuong"  - chuong sat ri nho (cac hoa am le, khong hai hoa), rit nhe. Xac nhan: chuong lon Re + La tram, ngan dai hon.
#
# Chay: python CongCu/AmThanh/tieng_bam.py [thu_muc_ra]   (mac dinh CongCu/AmThanh/TiengBamThu)
# Chi dung numpy (may khong co scipy): loc bang FFT.

import os, sys, wave
import numpy as np

SR = 44100
rng = np.random.default_rng(1010)
PI2 = 2 * np.pi


def thoi_gian(giay):
    return np.arange(int(SR * giay)) / SR


def loc_dai(x, thap, cao):
    """Loc thong dai bang FFT, mep mem (cosin) de khong reo."""
    n = len(x)
    X = np.fft.rfft(x)
    f = np.fft.rfftfreq(n, 1 / SR)
    m = np.ones_like(f)
    if thap > 0:
        m *= np.clip((f - thap * 0.7) / (thap * 0.3 + 1e-9), 0, 1)
    if cao < SR / 2:
        m *= np.clip((cao * 1.3 - f) / (cao * 0.3 + 1e-9), 0, 1)
    return np.fft.irfft(X * m, n)


def bao(t, len_giay, tat_giay):
    """Bao: len thang (len_giay) roi tat theo ham mu (hang so tat_giay)."""
    e = np.exp(-np.maximum(t - len_giay, 0) / tat_giay)
    e *= np.clip(t / max(len_giay, 1e-4), 0, 1)
    return e


def vang(x, giay, do_am, hat):
    """Tieng vang STEREO: tich chap voi xung on giam mu, MOI KENH MOT xung khac nhau -> hai kenh khong trung."""
    r = np.random.default_rng(hat)
    n = int(SR * giay)
    t = np.arange(n) / SR
    ra = []
    for k in range(2):
        xung = r.standard_normal(n) * np.exp(-t / (giay / 5.0))
        xung = loc_dai(xung, 150, 5000)
        xung /= np.sqrt(np.sum(xung ** 2)) + 1e-9
        ky = np.fft.irfft(np.fft.rfft(x, len(x) + n) * np.fft.rfft(xung, len(x) + n), len(x) + n)
        ra.append(ky)
    kho = np.zeros((len(x) + n, 2))
    kho[:len(x), 0] = x
    kho[:len(x), 1] = x
    uot = np.stack(ra, axis=1)
    return kho * (1 - do_am) + uot * do_am * 2.2


def dat_vi_tri(mono, lech):
    """Mono -> stereo voi lech trai/phai (-1..1) + tre vai mau o kenh xa (do rong)."""
    l = mono * np.sqrt(0.5 * (1 - lech))
    r = mono * np.sqrt(0.5 * (1 + lech))
    tre = int(abs(lech) * 0.0006 * SR)
    if lech > 0 and tre:
        l = np.concatenate([np.zeros(tre), l[:-tre]])
    elif lech < 0 and tre:
        r = np.concatenate([np.zeros(tre), r[:-tre]])
    return np.stack([l, r], axis=1)


def cong(dich, nguon, bat_dau=0.0):
    i = int(bat_dau * SR)
    n = min(len(nguon), len(dich) - i)
    dich[i:i + n] += nguon[:n]


def chuan_hoa(x, dinh_db=-3.0):
    # LOA DIEN THOAI (nen tang chinh) gan nhu khong phat duoi ~200 Hz: cat duoi 120 Hz cho moi tieng; phan "nang" cua tieng
    # xac nhan phai nam o dai trung (ban dau A / B xac nhan 93-94% nang luong < 200 Hz - tren dien thoai gan nhu im)
    x = np.stack([loc_dai(x[:, 0], 120, SR / 2), loc_dai(x[:, 1], 120, SR / 2)], axis=1)
    x = x - x.mean(axis=0)
    dinh = np.max(np.abs(x)) + 1e-9
    x = x / dinh * (10 ** (dinh_db / 20))
    # tat dan 8 ms cuoi - khong click khi het file
    n = int(0.008 * SR)
    x[-n:] *= np.linspace(1, 0, n)[:, None]
    return x


def ghi(duong, x):
    x = np.clip(x, -1, 1)
    du = (x * 32767).astype('<i2')
    with wave.open(duong, 'wb') as w:
        w.setnchannels(2)
        w.setsampwidth(2)
        w.setframerate(SR)
        w.writeframes(du.tobytes())


# ------------------------------------------------------------------ cac mau

def go_da(t, nang):
    """Cu go vao da: thup tram (sin quet xuong) + hat da (on loc cao, rat ngan)."""
    f0 = 95 if nang else 140
    pha = PI2 * np.cumsum(f0 * (1 + 0.8 * np.exp(-t / 0.012))) / SR
    thup = np.sin(pha) * bao(t, 0.002, 0.07 if nang else 0.045) * (0.4 if nang else 0.6)
    # cong huong cua phien da (hoa am le cua tam da: ~2,7 / 4,1 / 6,3 lan) - phan nghe duoc tren loa nho
    for he, b, tat in [(2.7, 0.55, 0.05), (4.1, 0.4, 0.035), (6.3, 0.25, 0.022)]:
        thup += b * np.sin(PI2 * f0 * he * t + rng.uniform(0, PI2)) * bao(t, 0.001, tat * (1.4 if nang else 1.0))
    vun = loc_dai(rng.standard_normal(len(t)), 1800, 7000) * bao(t, 0.0005, 0.012) * 0.55
    vun2 = loc_dai(rng.standard_normal(len(t)), 900, 3000) * bao(t, 0.001, 0.03) * 0.35
    return thup + vun + vun2


def chuong(t, f, ngan, nang=1.0):
    """Chuong kim loai: hoa am LE (chuong that khong hai hoa), moi hoa am tat rieng."""
    ti = [0.5, 1.0, 1.183, 1.506, 2.0, 2.514, 2.662, 3.011]
    bien = [0.35, 1.0, 0.55, 0.45, 0.4, 0.25, 0.2, 0.15]
    x = np.zeros_like(t)
    for k, (a, b) in enumerate(zip(ti, bien)):
        x += b * np.sin(PI2 * f * a * t + rng.uniform(0, PI2)) * np.exp(-t / (ngan / (1 + 0.6 * k)))
    x *= np.clip(t / 0.002, 0, 1) * nang
    return x


def mau_da_mo(nang):
    dai = 0.7 if nang else 0.35
    t = thoi_gian(dai)
    tron = np.zeros((len(t), 2))
    cong(tron, dat_vi_tri(go_da(t, nang), -0.15))
    if nang:
        # nhip go thu hai nhe hon, lech phai + chuong tang le Re (D3) ngan
        t2 = thoi_gian(dai - 0.06)
        cong(tron, dat_vi_tri(go_da(t2, False) * 0.45, 0.35), 0.06)
        cong(tron, dat_vi_tri(chuong(t, 293.7, 0.35, 0.22), 0.1))
    return chuan_hoa(vang(tron[:, 0] * 0.5 + tron[:, 1] * 0.5, 0.45 if nang else 0.25, 0.28, 11)[:, :] * 0.6
                     + np.pad(tron, ((0, int(SR * (0.45 if nang else 0.25))), (0, 0))) * 0.4, -3 if nang else -5)


def xuong_go(t, cao):
    """Xuong kho go: xung on loc dai hep (cong huong ~ cao Hz), tat rat nhanh."""
    on = rng.standard_normal(len(t))
    x = loc_dai(on, cao * 0.7, cao * 1.4) * bao(t, 0.0003, 0.010)
    x += np.sin(PI2 * cao * 0.5 * t) * bao(t, 0.0003, 0.008) * 0.6
    return x


def mau_xuong(nang):
    dai = 0.6 if nang else 0.3
    t = thoi_gian(dai)
    tron = np.zeros((len(t), 2))
    cong(tron, dat_vi_tri(xuong_go(t, 1600), -0.4))
    cong(tron, dat_vi_tri(xuong_go(t, 2100) * 0.7, 0.45), 0.028)
    # hoi tho thi tham rat ngan (on loc 1-4 kHz, len cham tat nhanh)
    tho = loc_dai(rng.standard_normal(len(t)), 1000, 4200) * bao(t, 0.03, 0.06) * 0.12
    cong(tron, dat_vi_tri(tho, 0.2), 0.01)
    if nang:
        # xuong GAY (nhieu manh lach cach) + nhip trong tram
        for k, tg in enumerate([0.0, 0.017, 0.031, 0.052]):
            cong(tron, dat_vi_tri(xuong_go(t, 1300 + 400 * k) * (0.9 - 0.15 * k), [-0.5, 0.4, -0.1, 0.6][k]), 0.07 + tg)
        trong = np.sin(PI2 * np.cumsum(70 * (1 + 0.5 * np.exp(-t / 0.02))) / SR) * bao(t, 0.003, 0.12) * 0.35
        # than trong (mat da trong) o dai trung - nghe duoc tren loa dien thoai
        than = loc_dai(rng.standard_normal(len(t)), 250, 900) * bao(t, 0.002, 0.06) * 0.5
        cong(tron, dat_vi_tri(trong + than, 0.0))
    return chuan_hoa(vang(tron.mean(axis=1), 0.35 if nang else 0.2, 0.22, 21) * 0.5
                     + np.pad(tron, ((0, int(SR * (0.35 if nang else 0.2))), (0, 0))) * 0.5, -3 if nang else -5)


def mau_chuong(nang):
    dai = 1.0 if nang else 0.35
    t = thoi_gian(dai)
    tron = np.zeros((len(t), 2))
    if nang:
        cong(tron, dat_vi_tri(chuong(t, 293.7, 0.45), -0.25))          # Re
        cong(tron, dat_vi_tri(chuong(t, 220.0, 0.55, 0.7), 0.3), 0.012)  # La tram
    else:
        cong(tron, dat_vi_tri(chuong(t, 880.0, 0.09, 0.8), -0.2))
    # tieng rit sat ri: on loc hep quet xuong rat nhe
    rit = loc_dai(rng.standard_normal(len(t)), 2500, 5200) * bao(t, 0.004, 0.025) * (0.25 if nang else 0.35)
    cong(tron, dat_vi_tri(rit, 0.35))
    return chuan_hoa(vang(tron.mean(axis=1), 0.6 if nang else 0.25, 0.3, 31) * 0.5
                     + np.pad(tron, ((0, int(SR * (0.6 if nang else 0.25))), (0, 0))) * 0.5, -3 if nang else -5)


if __name__ == '__main__':
    ra = sys.argv[1] if len(sys.argv) > 1 else os.path.join('CongCu', 'AmThanh', 'TiengBamThu')
    os.makedirs(ra, exist_ok=True)
    for ten, ham in [('A_da_mo', mau_da_mo), ('B_xuong', mau_xuong), ('C_chuong', mau_chuong)]:
        for loai, nang in [('thuong', False), ('xacnhan', True)]:
            x = ham(nang)
            duong = os.path.join(ra, '%s_%s.wav' % (ten, loai))
            ghi(duong, x)
            l, r = x[:, 0], x[:, 1]
            tq = float(np.sum(l * r) / np.sqrt(np.sum(l * l) * np.sum(r * r)))
            print('%-28s %.2f s  dinh %.2f  tuong quan L/R %.3f' % (os.path.basename(duong), len(x) / SR, np.max(np.abs(x)), tq))
