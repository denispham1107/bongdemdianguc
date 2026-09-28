# -*- coding: utf-8 -*-
# VE LAI LAN HAI HAI ICON: GIO LOC, TOC BIEN (29/09/2026).
#
# Nguoi dung: hai icon ve lai hom 28/09 (sinh_icon_ve_lai_3.py) "van con qua xau" - ve va thiet ke lai cho hop noi dung tung ky
# nang va boi canh game, khong can Blender MCP.
#
#   - Gio loc: BA CON LOC TOA HINH QUAT tu mot diem (dung ky nang: 3 loc cach nhau 15 do), con giua to phia truoc, hai con ben
#     nho hon nghieng ra ngoai; vet bui tren dat noi ve diem tung. Than loc KHONG con la cac dai xoan (nhin ra lo xo) ma dung
#     THE TICH TUNG DIEM ANH: pheu tron xoay, mat truoc + mat sau thay qua nhau, van gio quan quanh truc, mep DAC hon giua (nhin
#     xuyen qua nhieu lop vo hon), den tu trai. Mau MAY GIONG xam xanh nhu trong game; bui cuon o chan, set trong long loc giua,
#     BIA MO + da vun bi hat tung (80% hat tung).
#   - Toc bien: khong ve hinh nguoi (hai ban truoc hinh nguoi ra kieu hoat hinh). Cho cu: khoi tim bay len, vong sang CO lai
#     (dung hieu ung "bien mat" trong game); VET PHEP cong vut bay sang cho moi; cho moi: VONG PHEP khac ky tu NO ra, cot sang,
#     hat tim toe len (hieu ung "hien ra").
#
# Anh ra 256x256 nen DEN (IconKyNang CONG vao dia nut). Ve o 1024 roi thu nho. Chi dung Pillow.
# Chay: python CongCu/Icon/sinh_icon_ve_lai_5.py [GioLoc|TocBien] [--xem <thu muc>]
import math
import os
import random
import sys
from PIL import Image, ImageChops, ImageDraw, ImageFilter

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from sinh_icon_ve_lai import (W, S, moi, P, cong, sang, mo, phat_sang, to_mau, nhan, fbm, doc_theo_y, ban_kinh_tu_tam,
                              chaikin, bezier, ngoi_sao, lerp, xuat, dia_nut)
from sinh_icon_ve_lai_2 import dai_mau, cay_tia, ve_cay_tia, gop_tia, may
from sinh_icon_ve_lai_4 import noi_khoi


def _muot(a, b, x):
    t = max(0.0, min(1.0, (x - a) / (b - a)))
    return t * t * (3 - 2 * t)


# ------------------------------------------------------------------ pheu loc the tich

def _van(hat):
    """Cac so hang sin - tuan hoan theo s (chu vi) nen van gio lien mach quanh pheu."""
    rd = random.Random(hat)
    ds = []
    for _ in range(5):
        ds.append((1.0, rd.randint(2, 6), rd.uniform(-1.5, 1.5), rd.uniform(0, 6.283)))
    for _ in range(7):
        ds.append((0.45, rd.randint(11, 28), rd.uniform(-6, 6), rd.uniform(0, 6.283)))
    for _ in range(3):
        ds.append((0.15, rd.randint(35, 60), rd.uniform(-14, 14), rd.uniform(0, 6.283)))
    tong = sum(a for a, _, _, _ in ds)
    return ds, tong


def _mat_do(van, s, t):
    ds, tong = van
    v = 0.0
    for a, ks, kt, ph in ds:
        v += a * math.sin(6.2832 * (ks * s + kt * t) + ph)
    d = 0.5 + 0.5 * v / tong * 1.9
    d = max(0.0, min(1.0, d))
    return d ** 1.5


def pheu_the_tich(cx, day_y, dinh_y, r_day, r_dinh, loe, nghieng, xoan, hat, toi, sang_, do_duc=0.55, mu=1.3):
    """Ve pheu loc the tich -> (RGB, alpha L). r(t) = r_day + (r_dinh-r_day) t^mu + loe e^(-t/0.08); truc nghieng theo t."""
    van = _van(hat)
    rd = random.Random(hat + 1)
    pha = rd.uniform(0, 6.28)
    pha2 = rd.uniform(0, 6.28)
    mau = Image.new("RGB", (W, W), (0, 0, 0))
    al = Image.new("L", (W, W), 0)
    pm, pa = mau.load(), al.load()
    y0, y1 = int(dinh_y * W), int(day_y * W)
    for py in range(y0, y1):
        t = (day_y - (py + 0.5) / W) / (day_y - dinh_y)
        r = r_day + (r_dinh - r_day) * t ** mu + loe * math.exp(-t / 0.09)
        r *= 1 + 0.07 * math.sin(t * 17 + pha) + 0.045 * math.sin(t * 37 + pha2)      # mep lon nhon
        xc = cx + nghieng * t + 0.028 * math.sin(t * 3.4 + pha)                     # truc cong chu S
        doc = _muot(0.0, 0.06, t) * (1 - _muot(0.70, 1.0, t))                        # dinh tan vao may
        x0, x1 = int((xc - r) * W), int((xc + r) * W) + 1
        for px in range(max(0, x0), min(W, x1)):
            u = ((px + 0.5) / W - xc) / r
            if u <= -1 or u >= 1:
                continue
            z = math.sqrt(1 - u * u)
            th = math.asin(u)
            sf = th / 6.2832 + xoan * t
            sb = (math.pi - th) / 6.2832 + xoan * t
            df = _mat_do(van, sf, t)
            db = _mat_do(van, sb, t)
            day = 1.0 / max(z, 0.22)                                 # nhin xuyen nhieu vo hon o mep
            af = 1 - (1 - do_duc * (0.25 + 0.75 * df)) ** day
            ab = 1 - (1 - do_duc * 0.55 * (0.2 + 0.8 * db)) ** day
            bien = 1 - _muot(0.90, 1.0, abs(u))
            af *= doc * bien
            ab *= doc * bien
            den = max(0.0, min(1.0, 0.55 - 0.55 * u + 0.25 * z))    # den tu trai
            kf = (0.28 + 0.72 * den) * (0.45 + 0.55 * df)
            kb = 0.35 * (0.3 + 0.7 * db)
            cf = [toi[i] + (sang_[i] - toi[i]) * kf for i in range(3)]
            cb = [toi[i] * 0.8 + (sang_[i] - toi[i]) * kb * 0.5 for i in range(3)]
            a = af + ab * (1 - af)
            if a <= 0.002:
                continue
            c = [(cf[i] * af + cb[i] * ab * (1 - af)) / a for i in range(3)]
            pm[px, py] = (int(c[0]), int(c[1]), int(c[2]))
            pa[px, py] = int(min(1.0, a) * 255)
    return mau, al


def bui_xoay(cx, cy, rx, ry, hat, mau, k=1.0):
    """Dam bui cuon o chan loc: nhieu + vong cung xoay quanh."""
    # elip: tao mat na elip mo
    mn = Image.new("L", (W, W), 0)
    ImageDraw.Draw(mn).ellipse([P(cx - rx, cy - ry), P(cx + rx, cy + ry)], fill=255)
    mn = mo(mn, int(W * ry * 0.8))
    mn = ImageChops.multiply(mn, fbm(hat, 12, 4).point(lambda v: min(255, max(0, (v - 50) * 2))))
    lop = to_mau(mn.point(lambda v: int(v * k)), mau)
    vong = moi()
    dv = ImageDraw.Draw(vong)
    rd = random.Random(hat)
    for i in range(5):
        f = 0.55 + 0.12 * i
        g0 = rd.uniform(0, 360)
        dv.arc([P(cx - rx * f, cy - ry * f * 0.8), P(cx + rx * f, cy + ry * f * 0.8)], g0, g0 + rd.uniform(70, 140),
               fill=tuple(int(c * 0.9) for c in mau), width=4)
    return cong(lop, sang(mo(vong, 2), 0.7 * k))


def bia_mo(cx, cy, cao, goc, hat):
    """Bia mo da bi hat tung: phien da dau vom, khac chu thap, vet nut, noi khoi. -> (RGB, mat na)."""
    rong = cao * 0.62
    pts = [(-0.5, 1.0), (-0.5, 0.32)]
    for i in range(13):
        g = math.pi * (1 - i / 12)
        pts.append((0.5 * math.cos(g), 0.32 - 0.30 * math.sin(g)))
    pts += [(0.5, 0.32), (0.5, 1.0)]
    # mep sut me
    rd = random.Random(hat)
    pts = [(x + rd.uniform(-0.02, 0.02), y + rd.uniform(-0.015, 0.015)) for (x, y) in pts]
    ca, sa = math.cos(goc), math.sin(goc)

    def Q(u, v):
        x, y = u * rong, (v - 0.55) * cao
        return (cx + x * ca - y * sa, cy + x * sa + y * ca)
    mn = Image.new("L", (W, W), 0)
    ImageDraw.Draw(mn).polygon([P(*Q(u, v)) for (u, v) in pts], fill=255)
    mn = mo(mn, 1.2)
    xam = fbm(hat + 3, 18, 4).point(lambda v: 70 + v * 70 // 255)
    nk = noi_khoi(mn, 6, 4)
    xam = ImageChops.subtract(ImageChops.add(xam, nk.point(lambda v: max(0, v - 128) * 3)), nk.point(lambda v: max(0, 128 - v) * 3))
    # chu thap khac chim + vet nut
    kh = Image.new("L", (W, W), 0)
    dk = ImageDraw.Draw(kh)
    dk.line([P(*Q(0, 0.18)), P(*Q(0, 0.72))], fill=255, width=int(W * cao * 0.07))
    dk.line([P(*Q(-0.24, 0.36)), P(*Q(0.24, 0.36))], fill=255, width=int(W * cao * 0.07))
    x, y = 0.3, 0.2
    nut = [(x, y)]
    for _ in range(6):
        x += rd.uniform(-0.12, 0.05)
        y += rd.uniform(0.08, 0.14)
        nut.append((x, y))
    dk.line([P(*Q(*p)) for p in nut], fill=180, width=3)
    kh = mo(kh, 1.5)
    xam = ImageChops.subtract(xam, kh.point(lambda v: v * 2 // 3))
    xam = ImageChops.add(xam, ImageChops.offset(kh, -3, -3).point(lambda v: v // 5))
    da = dai_mau(xam, [(0.0, (4, 4, 6)), (0.35, (48, 50, 56)), (0.62, (112, 114, 122)), (0.85, (175, 176, 184)), (1.0, (230, 230, 236))])
    return nhan(da, mn), mn


def da_vun(d, x, y, r, rd):
    g = rd.uniform(0, 6.28)
    pts = [(x + math.cos(g + a) * r * rd.uniform(0.7, 1.15), y + math.sin(g + a) * r * rd.uniform(0.7, 1.15)) for a in (0, 1.2, 2.3, 3.5, 4.8)]
    d.polygon([P(*p) for p in pts], fill=(88, 88, 94))
    d.polygon([P(*p) for p in pts[:3]] + [P(x, y)], fill=(160, 160, 168))


# ------------------------------------------------------------------ GIO LOC

TOI_MAY = (40, 44, 56)
SANG_MAY = (205, 210, 228)


def may_dinh(vung, hat, k=1.0):
    """Dam may giong cuon tren mieng loc (ham may cua icon May gio): toi o day, vien sang."""
    mn, lop = may(vung, hat, (34, 38, 50), (150, 162, 196), random.Random(hat))
    return mn, sang(lop, k)


def gio_loc():
    rd = random.Random(7)
    nen = moi()
    goc_tung = (0.50, 0.99)
    # vet bui tren dat tu diem tung toa ra ba chan loc (hinh quat)
    chan = [(0.25, 0.73), (0.75, 0.73), (0.50, 0.85)]
    vet = Image.new("L", (W, W), 0)
    dv = ImageDraw.Draw(vet)
    for (x, y) in chan:
        for j in range(20):
            t = j / 20
            a = (goc_tung[0] + (x - goc_tung[0]) * t, goc_tung[1] + (y - goc_tung[1]) * t)
            b = (goc_tung[0] + (x - goc_tung[0]) * (t + 0.05), goc_tung[1] + (y - goc_tung[1]) * (t + 0.05))
            dv.line([P(*a), P(*b)], fill=int(120 * t), width=int(8 + 30 * t))
    vet = ImageChops.multiply(mo(vet, 10), fbm(8, 14, 3).point(lambda v: min(255, v + 40)))
    nen = cong(nen, to_mau(vet, (92, 96, 108)))
    # hai con loc ben (nho, xa hon, nghieng ra ngoai), dinh tan vao may
    for (cx, dx, hat) in [(0.25, -0.10, 11), (0.75, 0.10, 12)]:
        nen = cong(nen, bui_xoay(cx, 0.73, 0.14, 0.042, hat, (92, 96, 108), 1.0))
        m, a = pheu_the_tich(cx, 0.74, 0.30, 0.060, 0.105, 0.035, dx, 1.1, hat, (28, 31, 40), (150, 156, 176), 0.55, 1.0)
        nen = Image.composite(m, nen, a)
        mn, lop = may_dinh([(cx + dx, 0.31, 0.10, 0.05)], hat + 30, 0.8)
        nen = Image.composite(lop, nen, mn.point(lambda v: min(255, v * 2)))
    # con loc giua (to, gan): chan rong loe ra, than to dan, mieng tan vao may
    nen = cong(nen, bui_xoay(0.50, 0.85, 0.24, 0.065, 13, (116, 118, 128), 1.1))
    m, a = pheu_the_tich(0.50, 0.865, 0.20, 0.095, 0.185, 0.07, 0.03, 1.25, 14, TOI_MAY, SANG_MAY, 0.60, 1.0)
    # set trong long loc giua
    loi, quang = moi(), moi()
    ve_cay_tia(cay_tia((0.54, 0.24), (0.48, 0.70), rd, 0.22, 6, 2), loi, quang, 6, 22, (110, 120, 255))
    tia = gop_tia(loi, quang, 0.9)
    nen = cong(nen, nhan(sang(tia, 0.9), mo(a, 6).point(lambda v: min(255, v * 2))))
    nen = Image.composite(m, nen, a)
    nen = cong(nen, nhan(sang(loi, 0.8), mo(a, 3)), sang(to_mau(mo(a, 30), (40, 50, 90)), 0.25))
    mn, lop = may_dinh([(0.53, 0.19, 0.22, 0.07), (0.38, 0.21, 0.11, 0.045), (0.67, 0.20, 0.12, 0.05)], 40, 1.0)
    nen = Image.composite(lop, nen, mn.point(lambda v: min(255, int(v * 1.6))))
    # set loe trong may
    nen = cong(nen, sang(to_mau(ImageChops.multiply(mn, ban_kinh_tu_tam(0.55, 0.20, 0.0, 0.14, 1.0, 0.0)), (110, 120, 230)), 0.9))
    # bui mong phu truoc chan loc giua
    nen = cong(nen, sang(bui_xoay(0.50, 0.88, 0.21, 0.04, 15, (130, 130, 138), 1.0), 0.65))
    # bia mo bi hat tung ben phai con loc giua + da vun
    b, bm = bia_mo(0.80, 0.42, 0.19, 0.55, 21)
    vetb = moi()
    dvb = ImageDraw.Draw(vetb)
    for k in range(4):
        dvb.arc([P(0.58 + k * 0.01, 0.38 + k * 0.012), P(0.92, 0.72 - k * 0.01)], 200 + k * 6, 300, fill=(120, 125, 140), width=3)
    nen = cong(nen, sang(mo(vetb, 1.5), 0.6))
    nen = Image.composite(b, nen, bm)
    nen = cong(nen, sang(to_mau(ImageChops.multiply(mo(bm, 8), ImageChops.invert(bm)), (90, 100, 140)), 0.5))
    dvd = moi()
    ddv = ImageDraw.Draw(dvd)
    for (x, y, r) in [(0.19, 0.44, 0.020), (0.32, 0.36, 0.015), (0.66, 0.34, 0.017), (0.87, 0.62, 0.015), (0.14, 0.58, 0.013), (0.70, 0.54, 0.012)]:
        da_vun(ddv, x, y, r, rd)
    nen = Image.composite(dvd, nen, dvd.convert("L").point(lambda v: 255 if v > 0 else 0).filter(ImageFilter.GaussianBlur(1)))
    # hat bui nho bay quanh
    hat = moi()
    dh = ImageDraw.Draw(hat)
    for _ in range(90):
        g = rd.uniform(0, 6.28)
        rr = rd.uniform(0.20, 0.32)
        x, y = 0.5 + math.cos(g) * rr, 0.58 + math.sin(g) * rr * 0.7
        r = rd.uniform(1.5, 4)
        dh.ellipse([x * W - r, y * W - r, x * W + r, y * W + r], fill=lerp((80, 82, 92), (190, 195, 210), rd.random()))
    nen = cong(nen, sang(hat, 0.8))
    return nen


# ------------------------------------------------------------------ TOC BIEN

TIM = (190, 105, 255)
TIM_SANG = (245, 222, 255)
TIM_SAU = (80, 26, 150)


def vong_phep(cx, cy, rx, ry, k, hat):
    """Vong phep phoi canh: hai vanh, vach ky tu, sao sau canh noi trong, chu khac tren vanh."""
    lop = moi()
    d = ImageDraw.Draw(lop)
    c1 = lerp((0, 0, 0), TIM_SANG, k)
    c2 = lerp((0, 0, 0), TIM, k)
    d.ellipse([P(cx - rx, cy - ry), P(cx + rx, cy + ry)], outline=c1, width=5)
    d.ellipse([P(cx - rx * 0.86, cy - ry * 0.86), P(cx + rx * 0.86, cy + ry * 0.86)], outline=c2, width=3)
    d.ellipse([P(cx - rx * 0.52, cy - ry * 0.52), P(cx + rx * 0.52, cy + ry * 0.52)], outline=c2, width=3)
    rd = random.Random(hat)
    # ky tu tren vanh giua hai vong
    for i in range(22):
        g = 2 * math.pi * i / 22
        x, y = cx + math.cos(g) * rx * 0.93, cy + math.sin(g) * ry * 0.93
        l = 0.012
        kieu = rd.randint(0, 2)
        if kieu == 0:
            d.line([P(x, y - l * 0.35), P(x, y + l * 0.35)], fill=c1, width=3)
        elif kieu == 1:
            d.line([P(x - l * 0.6, y), P(x + l * 0.6, y)], fill=c1, width=3)
            d.line([P(x, y - l * 0.3), P(x, y + l * 0.3)], fill=c1, width=2)
        else:
            d.ellipse([P(x - l * 0.35, y - l * 0.25), P(x + l * 0.35, y + l * 0.25)], outline=c1, width=2)
    # sao sau canh noi trong vong giua (hai tam giac)
    for lech in (0, math.pi / 3):
        pts = [P(cx + math.cos(lech + 2 * math.pi * j / 3 - math.pi / 2) * rx * 0.86,
                 cy + math.sin(lech + 2 * math.pi * j / 3 - math.pi / 2) * ry * 0.86) for j in range(3)]
        d.polygon(pts, outline=c2)
        d.line(pts + [pts[0]], fill=c2, width=3)
    return phat_sang(lop, 10, 1.4)


def toc_bien():
    rd = random.Random(15)
    nen = moi()
    di, den = (0.25, 0.77), (0.72, 0.77)
    # ---- CHO CU: khoi tim bay len + vong sang co lai
    khoi = Image.new("L", (W, W), 0)
    dk = ImageDraw.Draw(khoi)
    for i in range(9):
        t = i / 8
        x = di[0] + 0.03 * math.sin(t * 5) - 0.05 * t
        y = di[1] - 0.05 - 0.40 * t
        r = 0.05 + 0.04 * t
        dk.ellipse([P(x - r, y - r), P(x + r, y + r)], fill=int(230 * (1 - t * 0.7)))
    khoi = mo(khoi, 22)
    khoi = ImageChops.multiply(khoi, fbm(16, 11, 4).point(lambda v: min(255, max(0, (v - 70) * 2))))
    nen = cong(nen, to_mau(khoi, (95, 40, 150)), to_mau(ImageChops.multiply(khoi, fbm(17, 20, 3)).point(lambda v: max(0, v - 80) * 2), (185, 120, 245)))
    nen = cong(nen, vong_phep(di[0], di[1], 0.12, 0.034, 0.35, 3))
    co = moi()
    dco = ImageDraw.Draw(co)
    for (f, k) in [(0.75, 0.5), (0.45, 0.8), (0.2, 1.0)]:
        dco.ellipse([P(di[0] - 0.12 * f, di[1] - 0.034 * f), P(di[0] + 0.12 * f, di[1] + 0.034 * f)], outline=lerp((0, 0, 0), TIM_SANG, k), width=4)
    nen = cong(nen, phat_sang(co, 8, 1.2))
    hat = moi()
    dh = ImageDraw.Draw(hat)
    for _ in range(45):
        x = di[0] + rd.uniform(-0.09, 0.07)
        y = di[1] - rd.uniform(0.02, 0.46)
        r = rd.uniform(2, 5)
        dh.rectangle([x * W - r, y * W - r, x * W + r, y * W + r], fill=lerp(TIM_SAU, TIM_SANG, rd.random()))
    nen = cong(nen, phat_sang(hat, 5, 0.9))
    # ---- VET PHEP cong vut tu cho cu sang cho moi
    p0, p1, p2, p3 = (di[0] + 0.01, di[1] - 0.12), (0.30, 0.10), (0.64, 0.08), (den[0], den[1] - 0.14)
    vet = moi()
    dv = ImageDraw.Draw(vet)
    n = 80
    for i in range(n):
        t0, t1 = i / n, (i + 1) / n
        a, b = bezier(p0, p1, p2, p3, t0), bezier(p0, p1, p2, p3, t1)
        dv.line([P(*a), P(*b)], fill=lerp((0, 0, 0), TIM, 0.25 + 0.75 * t0), width=int(4 + 30 * t0 ** 1.6))
    loi = moi()
    dl = ImageDraw.Draw(loi)
    for i in range(n):
        t0, t1 = i / n, (i + 1) / n
        a, b = bezier(p0, p1, p2, p3, t0), bezier(p0, p1, p2, p3, t1)
        dl.line([P(*a), P(*b)], fill=lerp((0, 0, 0), TIM_SANG, 0.2 + 0.8 * t0), width=int(2 + 9 * t0 ** 1.6))
    # vet toc do song song (dut quang) doc duong bay
    for lech in (-0.028, 0.028, -0.05):
        for i in range(0, n, 2):
            t0 = i / n
            if t0 > 0.75:
                break
            a, b = bezier(p0, p1, p2, p3, t0), bezier(p0, p1, p2, p3, t0 + 0.015)
            dx, dy = b[0] - a[0], b[1] - a[1]
            l = math.hypot(dx, dy) + 1e-9
            nx, ny = -dy / l * lech, dx / l * lech
            dv.line([P(a[0] + nx, a[1] + ny), P(b[0] + nx, b[1] + ny)], fill=lerp((0, 0, 0), TIM, 0.5 * t0 + 0.2), width=3)
    nen = cong(nen, sang(mo(vet, 24), 1.0), mo(vet, 4), phat_sang(mo(loi, 2), 6, 1.2))
    for i in range(14):
        t = rd.uniform(0.1, 0.95)
        x, y = bezier(p0, p1, p2, p3, t)
        s2 = moi()
        ngoi_sao(ImageDraw.Draw(s2), (x + rd.uniform(-0.03, 0.03)) * W, (y + rd.uniform(-0.03, 0.03)) * W, rd.uniform(6, 14), TIM_SANG)
        nen = cong(nen, phat_sang(s2, 4, 1.0))
    # ---- CHO MOI: vong phep no ra + cot sang + hat toe len
    nen = cong(nen, vong_phep(den[0], den[1], 0.20, 0.056, 1.0, 5))
    song = moi()
    ds = ImageDraw.Draw(song)
    for (f, k) in [(1.25, 0.45), (1.45, 0.22)]:
        ds.ellipse([P(den[0] - 0.20 * f, den[1] - 0.056 * f), P(den[0] + 0.20 * f, den[1] + 0.056 * f)], outline=lerp((0, 0, 0), TIM, k), width=4)
    nen = cong(nen, phat_sang(song, 10, 1.0))
    cot = ImageChops.multiply(ban_kinh_tu_tam(den[0], den[1] - 0.2, 0.0, 0.13, 1.0, 0.0),
                              doc_theo_y(lambda y: _muot(den[1] - 0.52, den[1] - 0.02, y) * (1 - _muot(den[1] - 0.01, den[1] + 0.02, y))))
    cot = ImageChops.multiply(cot, fbm(18, 6, 3).resize((W // 8, W)).resize((W, W)).point(lambda v: min(255, 80 + v)))
    nen = cong(nen, to_mau(cot, (150, 80, 230)), to_mau(cot.point(lambda v: max(0, v - 150) * 2), TIM_SANG))
    hat2 = moi()
    dh2 = ImageDraw.Draw(hat2)
    for _ in range(40):
        g = rd.uniform(-2.7, -0.45)
        l = rd.uniform(0.05, 0.22)
        x0, y0 = den[0] + rd.uniform(-0.06, 0.06), den[1] - 0.02
        dh2.line([P(x0, y0), P(x0 + math.cos(g) * l, y0 + math.sin(g) * l)], fill=lerp(TIM_SAU, TIM_SANG, rd.random()), width=rd.randint(2, 4))
    nen = cong(nen, phat_sang(hat2, 5, 1.1))
    loe = moi()
    ngoi_sao(ImageDraw.Draw(loe), p3[0] * W, p3[1] * W, 60, TIM_SANG, 0.10)
    nen = cong(nen, phat_sang(loe, 14, 1.6), to_mau(ban_kinh_tu_tam(p3[0], p3[1], 0.0, 0.07, 1.0, 0.0), (220, 170, 255)))
    return nen


if __name__ == "__main__":
    ds = [("GioLoc", gio_loc, (0.07, 0.09, 0.13)), ("TocBien", toc_bien, (0.09, 0.04, 0.15))]
    chi = [a for a in sys.argv[1:] if not a.startswith("--") and a in [d[0] for d in ds]]
    kq = []
    for ten, ham, nen_he in ds:
        if chi and ten not in chi:
            continue
        kq.append((ten, xuat(ham(), ten), nen_he))
    if "--xem" in sys.argv:
        thu_muc = sys.argv[sys.argv.index("--xem") + 1]
        tam = Image.new("RGB", (len(kq) * (S * 2 + 10) + 20 + S + 110, S * 2 + 10), (24, 22, 26))
        for i, (ten, anh, nen_he) in enumerate(kq):
            tam.paste(anh.convert("RGB").resize((S * 2, S * 2)), (i * (S * 2 + 10), 0))
        x0 = len(kq) * (S * 2 + 10) + 20
        for i, (ten, anh, nen_he) in enumerate(kq):
            nut = dia_nut(anh, nen_he)
            tam.paste(nut, (x0 + i * 0, i * (S + 10)), nut)
            nho = nut.resize((84, 84), Image.LANCZOS)
            tam.paste(nho, (x0 + S + 10, i * (S + 10)), nho)
        tam.save(os.path.join(thu_muc, "icon_moi_5.png"))
        print("xem truoc:", os.path.join(thu_muc, "icon_moi_5.png"))
