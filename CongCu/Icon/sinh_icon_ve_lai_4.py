# -*- coding: utf-8 -*-
# VE LAI NAM ICON BI DONG: KHANG LUA, KHANG BANG, KHANG SET, KHANG PHONG, TOC DO DI CHUYEN (29/09/2026).
#
# Nguoi dung: nam icon cu (tam khien phang + ky hieu trang, sinh_khang.py) "qua tho va so sai, net ve nhu game tre em" - ve
# lai cho hop noi dung tung ky nang va hop boi canh kinh di cua game; "khong can su dung MCP Blender".
#
#   - Bon icon KHANG: TAM KHIEN SAT GOTHIC (vanh thep dinh tan, long khien trang men mau he, an he khac chim phat sang,
#     tray xuoc) DUNG CHAN mot don nguyen to lao toi tu goc tren phai - dong nguyen to BI TACH, DAT QUA HAI MEP khien
#     (duong dong chay vong quanh vat can), cho va cham toe ra. Ky nang giam sat thuong cua he do -> "do duoc, dat ra".
#       Lua: luoi lua cuon + tan lua, mep khien nung do.   Bang: gio buot + manh bang vo tan, suong gia bam mat khien.
#       Set: tia set giang trung vanh khien, dien chay vong theo vanh roi phong xuong dat.   Phong: gio xe, bui + la kho + da vun.
#   - Toc do: phu thuy trum mu (cung dang icon Tang hinh / Toc bien) LAO NGUOI toi, keo theo ba tan anh mo dan, vat ao rach
#     tung bay, vet gio xe ngang va bui dat tung sau got. Mau xanh nhom BI DONG.
#
# Anh ra 256x256 nen DEN - IconKyNang CONG anh vao dia nut (cho den khong anh huong). Ve o 1024 roi thu nho. Chi dung Pillow.
#
# Chay: python CongCu/Icon/sinh_icon_ve_lai_4.py  [--xem <thu muc>]
import math
import os
import random
import sys
from PIL import Image, ImageChops, ImageDraw, ImageFilter, ImageOps

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from sinh_icon_ve_lai import (W, S, moi, P, cong, sang, mo, phat_sang, to_mau, nhan, fbm, doc_theo_y, ban_kinh_tu_tam,
                              chaikin, bezier, ngoi_sao, lerp, thang_mau, xuat, dia_nut)
from sinh_icon_ve_lai_2 import dai_mau, cay_tia, ve_cay_tia, gop_tia
from sinh_icon_ve_lai_3 import bong_phu_thuy, vien_sang


# ------------------------------------------------------------------ tam khien

def _vien_khien_don_vi():
    """Duong vien khien don vi (u -0,5..0,5 ngang, v 0..1 tu tren xuong): vai hoi vong, mui nhon giua canh tren, day nhon."""
    tren = [(-0.5, 0.075), (-0.36, 0.045), (-0.20, 0.055), (-0.07, 0.035), (0.0, -0.015),
            (0.07, 0.035), (0.20, 0.055), (0.36, 0.045), (0.5, 0.075)]
    phai = [bezier((0.5, 0.075), (0.53, 0.50), (0.36, 0.80), (0.0, 1.0), i / 24.0) for i in range(1, 25)]
    trai = [(-x, y) for (x, y) in reversed(phai[:-1])]
    return tren + phai + trai


def _dat(vien, cx, top, rong, cao, co=1.0, tam_v=0.48):
    return [(cx + u * rong * co, top + (tam_v + (v - tam_v) * co) * cao) for (u, v) in vien]


def noi_khoi(mn, r, d):
    """Den tu tren trai: 128 + H(x+d) - H(x-d), H = mat na lam mo."""
    h = mo(mn, r)
    return ImageChops.subtract(ImageChops.offset(h, -d, -d), ImageChops.offset(h, d, d), 1.0, 128)


def khien(cx, cy, cao, men, mau_an, ve_an, hat):
    """Tra ve (anh RGB, mat na L, vien ngoai [(x,y)], diem giua long khien)."""
    rd = random.Random(hat)
    rong = cao * 0.80
    top = cy - cao * 0.5
    don_vi = _vien_khien_don_vi()
    ngoai = _dat(don_vi, cx, top, rong, cao)
    trong = _dat(don_vi, cx, top, rong, cao, 0.80)
    mn = Image.new("L", (W, W), 0)
    ImageDraw.Draw(mn).polygon([P(*p) for p in ngoai], fill=255)
    mn = mo(mn, 1.2)
    mt = Image.new("L", (W, W), 0)
    ImageDraw.Draw(mt).polygon([P(*p) for p in trong], fill=255)
    mt = mo(mt, 1.2)
    vanh = ImageChops.subtract(mn, mt)

    # --- vanh THEP: xam lanh, nhieu mai, noi khoi hai mep
    n1 = fbm(hat, 12, 4)
    xam_vanh = n1.point(lambda v: 70 + v * 70 // 255)
    nk = noi_khoi(mn, 7, 5)
    nk2 = noi_khoi(mt, 5, 4)
    sang_k = ImageChops.add(nk.point(lambda v: max(0, v - 128) * 3), nk2.point(lambda v: max(0, v - 128) * 2))
    toi_k = ImageChops.add(nk.point(lambda v: max(0, 128 - v) * 3), nk2.point(lambda v: max(0, 128 - v) * 2))
    xam_vanh = ImageChops.subtract(ImageChops.add(xam_vanh, sang_k), toi_k)
    thep = dai_mau(xam_vanh, [(0.0, (6, 6, 9)), (0.35, (58, 57, 64)), (0.62, (132, 130, 140)), (0.85, (205, 202, 212)), (1.0, (255, 252, 245))])

    # --- long khien: MEN mau he, sat den lo ra o cho mon, vet tray
    n2 = fbm(hat + 7, 9, 4)
    mon = mo(fbm(hat + 11, 30, 3).point(lambda v: 255 if v > 222 else 0), 2)
    xam_men = n2.point(lambda v: 95 + v * 60 // 255)
    # long khien chim xuong: mep tren trai toi, duoi phai sang (nguoc voi vanh)
    xam_men = ImageChops.subtract(ImageChops.add(xam_men, nk2.point(lambda v: max(0, 128 - v) * 2)), nk2.point(lambda v: max(0, v - 128) * 3))
    tray = Image.new("L", (W, W), 0)
    dtr = ImageDraw.Draw(tray)
    for _ in range(38):
        x = cx + rd.uniform(-0.8, 0.8) * rong * 0.4
        y = cy + rd.uniform(-0.45, 0.35) * cao
        g = rd.uniform(-0.9, 0.9) + (math.pi * 0.25 if rd.random() < 0.5 else 0)
        l = rd.uniform(0.015, 0.06)
        dtr.line([P(x, y), P(x + math.cos(g) * l, y + math.sin(g) * l)], fill=rd.randint(40, 110), width=rd.randint(1, 3))
    xam_men = ImageChops.add(xam_men, tray)
    long_men = dai_mau(xam_men, [(0.0, (0, 0, 0)), (0.3, tuple(int(c * 0.45) for c in men)), (0.62, men),
                                 (0.9, lerp(men, (255, 255, 255), 0.45)), (1.0, (255, 255, 255))])
    sat_den = dai_mau(xam_men, [(0.0, (0, 0, 0)), (0.5, (30, 28, 30)), (1.0, (95, 90, 92))])
    long_men = Image.composite(sat_den, long_men, mon)

    anh = Image.composite(thep, long_men, vanh)
    anh = nhan(anh, mn)
    # bong toi dan xuong day (anh sang tu tren)
    anh = nhan(anh, doc_theo_y(lambda y: 1.0 - 0.45 * max(0.0, min(1.0, (y - top) / cao))))

    # --- dinh tan tren vanh: giua vien ngoai va vien trong
    dt = moi()
    ddt = ImageDraw.Draw(dt)
    for i in range(0, len(ngoai), 3):
        x = (ngoai[i][0] + trong[i][0]) / 2
        y = (ngoai[i][1] + trong[i][1]) / 2
        r = 0.0115
        ddt.ellipse([P(x - r, y - r), P(x + r, y + r)], fill=(28, 27, 30))
        ddt.ellipse([P(x - r * 0.75, y - r * 0.75), P(x + r * 0.35, y + r * 0.35)], fill=(150, 148, 156))
        ddt.ellipse([P(x - r * 0.55, y - r * 0.55), P(x - r * 0.05, y - r * 0.05)], fill=(245, 242, 235))
    mn_dt = Image.new("L", (W, W), 0)
    dmd = ImageDraw.Draw(mn_dt)
    for i in range(0, len(ngoai), 3):
        x = (ngoai[i][0] + trong[i][0]) / 2
        y = (ngoai[i][1] + trong[i][1]) / 2
        dmd.ellipse([P(x - 0.0115, y - 0.0115), P(x + 0.0115, y + 0.0115)], fill=255)
    anh = Image.composite(mo(dt, 1), anh, mo(mn_dt, 1))

    # --- AN HE khac chim: ranh toi quanh, long an phat sang mau he
    tam_long = (cx, top + cao * 0.46)
    an = Image.new("L", (W, W), 0)
    ve_an(ImageDraw.Draw(an), tam_long[0], tam_long[1], cao * 0.34)
    an = mo(an, 1.5)
    ranh = mo(an.filter(ImageFilter.MaxFilter(9)), 3)
    anh = nhan(anh, ImageChops.invert(ranh).point(lambda v: 40 + v * 215 // 255))
    anh = cong(anh, to_mau(an, mau_an), sang(to_mau(mo(an, 10), mau_an), 0.9), sang(to_mau(mo(an, 30), mau_an), 0.55))
    loi_an = ImageChops.multiply(an, ImageChops.invert(mo(ImageChops.invert(an), 4)))
    anh = cong(anh, to_mau(loi_an.point(lambda v: v // 2), (255, 255, 255)))
    return anh, mn, ngoai, tam_long


# ------------------------------------------------------------------ an cua tung he

def an_lua(d, x, y, s):
    """Ngon lua ba luoi kieu gothic."""
    def luoi(dx, cao, rong, cong_):
        trai, phai = [], []
        for i in range(31):
            t = i / 30.0
            r = rong * math.sin(math.pi * min(1.0, 0.08 + t * 0.92)) ** 0.8 * (0.3 + 0.7 * t)
            ox = cong_ * (1 - t) ** 2
            yy = y + s * 0.5 - cao * (1 - t)
            trai.append((x + dx + ox - r, yy))
            phai.append((x + dx + ox + r, yy))
        return [P(*p) for p in trai + phai[::-1]]
    d.polygon(luoi(0, s * 1.18, s * 0.24, s * 0.20), fill=255)
    d.polygon(luoi(-s * 0.20, s * 0.55, s * 0.13, -s * 0.16), fill=255)
    d.polygon(luoi(s * 0.19, s * 0.66, s * 0.12, s * 0.20), fill=255)


def an_bang(d, x, y, s):
    r = s * 0.48
    for k in range(6):
        g = math.pi * k / 3.0 - math.pi / 2
        ex, ey = x + math.cos(g) * r, y + math.sin(g) * r
        d.line([P(x, y), P(ex, ey)], fill=255, width=int(W * s * 0.075))
        for t, l in ((0.45, 0.30), (0.72, 0.22)):
            gx, gy = x + math.cos(g) * r * t, y + math.sin(g) * r * t
            for c in (-1, 1):
                g2 = g + c * 0.75
                d.line([P(gx, gy), P(gx + math.cos(g2) * r * l, gy + math.sin(g2) * r * l)], fill=255, width=int(W * s * 0.05))
        # mui kim cuong o dau nhanh
        d.polygon([P(ex + math.cos(g) * r * 0.16, ey + math.sin(g) * r * 0.16), P(ex + math.cos(g + 1.57) * r * 0.07, ey + math.sin(g + 1.57) * r * 0.07),
                   P(ex - math.cos(g) * r * 0.08, ey - math.sin(g) * r * 0.08), P(ex + math.cos(g - 1.57) * r * 0.07, ey + math.sin(g - 1.57) * r * 0.07)], fill=255)
    d.polygon([P(x, y - r * 0.2), P(x + r * 0.17, y), P(x, y + r * 0.2), P(x - r * 0.17, y)], fill=255)


def an_set(d, x, y, s):
    pts = [(0.16, -0.52), (-0.20, 0.02), (0.02, 0.02), (-0.16, 0.54), (0.26, -0.08), (0.04, -0.08), (0.30, -0.52)]
    d.polygon([P(x + u * s, y + v * s) for (u, v) in pts], fill=255)


def an_phong(d, x, y, s):
    """Ba dai gio cuon xoan oc quanh tam (triskel)."""
    for k in range(3):
        g0 = 2 * math.pi * k / 3.0
        pts = []
        for i in range(40):
            t = i / 39.0
            g = g0 + t * 2.4
            r = s * (0.08 + 0.42 * t)
            pts.append(P(x + math.cos(g) * r, y + math.sin(g) * r))
        for i in range(len(pts) - 1):
            w = int(W * s * (0.03 + 0.07 * (i / len(pts))))
            d.line([pts[i], pts[i + 1]], fill=255, width=max(2, w))
    d.ellipse([P(x - s * 0.07, y - s * 0.07), P(x + s * 0.07, y + s * 0.07)], fill=255)


# ------------------------------------------------------------------ dong chay vong quanh khien

def dong_chay(tam, ax, ay, huong, so, rong, hat, dau=-1.35, cuoi=1.5):
    """Duong dong chay the (potential flow) vong qua khien - khien xap xi elip ban truc ax, ay. Tinh trong khong gian da co
    elip thanh hinh tron don vi, dong toi theo 'huong'. Tra ve [(diem [(x,y)], b)], b = do lech ngang ban dau."""
    rd = random.Random(hat)
    hx, hy = huong[0] / ax, huong[1] / ay
    l = math.hypot(hx, hy)
    hx, hy = hx / l, hy / l
    nx, ny = -hy, hx
    kq = []
    for i in range(so):
        b = -rong + 2 * rong * (i + rd.uniform(0.2, 0.8)) / so
        x, y = dau, b
        pts = []
        for _ in range(900):
            r2 = x * x + y * y
            if r2 < 1.03:
                break
            u = 1 - (x * x - y * y) / (r2 * r2)
            v = -2 * x * y / (r2 * r2)
            m = math.hypot(u, v) + 1e-6
            x += u / m * 0.012
            y += v / m * 0.012
            gx, gy = x * hx + y * nx, x * hy + y * ny
            pts.append((tam[0] + gx * ax, tam[1] + gy * ay))
            if x > cuoi:
                break
        kq.append((pts, b))
    return kq


def diem_va_cham(tam, ax, ay, huong):
    hx, hy = huong[0] / ax, huong[1] / ay
    l = math.hypot(hx, hy)
    return (tam[0] - hx / l * ax, tam[1] - hy / l * ay)


def vanh_sang_gan(mn, diem, r0, r1):
    """Mep khien (vien ngoai) sang dan ve phia diem va cham."""
    mep = ImageChops.multiply(mn, ImageChops.invert(mo(mn, 10))).point(lambda v: min(255, v * 3))
    return ImageChops.multiply(mep, ban_kinh_tu_tam(diem[0], diem[1], r0, r1))


# ------------------------------------------------------------------ KHANG LUA

HUONG = (-0.78, 0.62)            # nguyen to lao tu goc tren phai xuong duoi trai
TAM = (0.47, 0.53)
CAO = 0.62
AX, AY = 0.235, 0.315


def khang_lua():
    rd = random.Random(21)
    nen = moi()
    dong = dong_chay(TAM, AX, AY, HUONG, 30, 1.0, 21)
    cuong = moi().convert("L")
    dc = ImageDraw.Draw(cuong)
    for pts, b in dong:
        n = len(pts)
        if n < 3:
            continue
        gan = max(0.0, 1.0 - abs(b) / 1.0)
        for i in range(n - 1):
            t = i / n
            vao = min(1.0, t * 8.0)
            ra = max(0.0, 1.0 - max(0.0, t - 0.40) / 0.45) ** 1.5
            val = int(255 * vao * ra * (0.55 + 0.45 * gan))
            dc.line([P(*pts[i]), P(*pts[i + 1])], fill=val, width=int(10 + 34 * gan * ra))
    cuong = mo(cuong, 9)
    xoan = fbm(22, 11, 4)
    cuong = ImageChops.multiply(cuong, xoan.point(lambda v: min(255, max(0, (v - 60) * 2))))
    cuong = cuong.point(lambda v: min(255, int(v * 1.9)))
    va = diem_va_cham(TAM, AX, AY, HUONG)
    no = ban_kinh_tu_tam(va[0], va[1], 0.0, 0.2, 1.0, 0.0)
    cuong = ImageChops.add(cuong, ImageChops.multiply(no, xoan.point(lambda v: min(255, v + 60))))
    lua = dai_mau(cuong, [(0.0, (0, 0, 0)), (0.25, (70, 6, 0)), (0.5, (190, 45, 4)), (0.72, (255, 125, 18)),
                          (0.9, (255, 215, 110)), (1.0, (255, 250, 225))])
    nen = cong(nen, lua, sang(mo(lua, 26), 0.5))
    k, mn, vien, _ = khien(TAM[0], TAM[1], CAO, (150, 26, 12), (255, 150, 50), an_lua, 101)
    nen = Image.composite(k, nen, mn)
    # mep khien nung do phia va cham
    nung = vanh_sang_gan(mn, va, 0.02, 0.42)
    nen = cong(nen, to_mau(nung, (255, 120, 30)), sang(to_mau(mo(nung, 12), (255, 90, 10)), 1.2))
    # luoi lua liem qua hai mep, ve DE len khien mot chut
    lien = ImageChops.multiply(cuong, ImageChops.invert(mn).point(lambda v: min(255, v + 0)))
    nen = cong(nen, sang(dai_mau(mo(lien, 3), [(0.0, (0, 0, 0)), (0.6, (120, 25, 0)), (1.0, (255, 170, 60))]), 0.35))
    # tan lua bay theo dong
    tan = moi()
    dt = ImageDraw.Draw(tan)
    for pts, b in dong:
        for _ in range(3):
            if len(pts) < 10:
                continue
            i = rd.randint(len(pts) // 3, len(pts) - 1)
            x, y = pts[i]
            r = rd.uniform(2.5, 6.5)
            dt.ellipse([x * W - r, y * W - r, x * W + r, y * W + r], fill=lerp((255, 90, 10), (255, 235, 150), rd.random()))
    # tia toe ra o cho va cham
    for _ in range(26):
        g = math.atan2(-HUONG[1], -HUONG[0]) + rd.uniform(-1.6, 1.6)
        l = rd.uniform(0.04, 0.16)
        x0, y0 = va[0] + math.cos(g) * 0.02, va[1] + math.sin(g) * 0.02
        dt.line([P(x0, y0), P(x0 + math.cos(g) * l, y0 + math.sin(g) * l)], fill=(255, 200, 90), width=rd.randint(2, 4))
    nen = cong(nen, phat_sang(tan, 6, 1.4))
    return nen


# ------------------------------------------------------------------ KHANG BANG

def _manh_bang(d, x, y, g, dai, rong, mau_than, mau_canh):
    ux, uy = math.cos(g), math.sin(g)
    nx, ny = -uy, ux
    pts = [(x + ux * dai, y + uy * dai), (x + nx * rong + ux * dai * 0.1, y + ny * rong + uy * dai * 0.1),
           (x - ux * dai * 0.55, y - uy * dai * 0.55), (x - nx * rong * 0.8 - ux * dai * 0.05, y - ny * rong * 0.8 - uy * dai * 0.05)]
    d.polygon([P(*p) for p in pts], fill=mau_than)
    d.line([P(*pts[0]), P(*pts[2])], fill=mau_canh, width=2)
    d.line([P(*pts[0]), P(*pts[1])], fill=mau_canh, width=3)


def _gai_suong(d, x, y, g, l, sau, rd, mau):
    """Suong gia: canh cay nho re nhanh."""
    x2, y2 = x + math.cos(g) * l, y + math.sin(g) * l
    d.line([P(x, y), P(x2, y2)], fill=mau, width=max(1, sau))
    if sau <= 0:
        return
    for c in (-1, 1):
        if rd.random() < 0.85:
            _gai_suong(d, x + math.cos(g) * l * rd.uniform(0.4, 0.8), y + math.sin(g) * l * rd.uniform(0.4, 0.8),
                       g + c * rd.uniform(0.6, 1.1), l * 0.55, sau - 1, rd, mau)


def khang_bang():
    rd = random.Random(31)
    nen = moi()
    dong = dong_chay(TAM, AX, AY, HUONG, 24, 1.05, 31)
    gio = moi().convert("L")
    dg = ImageDraw.Draw(gio)
    for pts, b in dong:
        n = len(pts)
        gan = max(0.0, 1.0 - abs(b) / 1.05)
        for i in range(n - 1):
            t = i / max(1, n)
            val = int(255 * min(1.0, t * 6) * max(0.0, 1 - max(0.0, t - 0.45) / 0.45) * (0.45 + 0.55 * gan))
            dg.line([P(*pts[i]), P(*pts[i + 1])], fill=val, width=int(3 + 9 * gan))
    gio = ImageChops.multiply(mo(gio, 4), fbm(32, 14, 3).point(lambda v: min(255, max(0, (v - 50) * 2))))
    nen = cong(nen, to_mau(gio, (120, 190, 255)), sang(to_mau(mo(gio, 18), (60, 130, 230)), 0.9))
    va = diem_va_cham(TAM, AX, AY, HUONG)
    # suong lanh cuon o cho va cham
    suong = ImageChops.multiply(ban_kinh_tu_tam(va[0], va[1], 0.0, 0.2, 1.0, 0.0), fbm(33, 10, 4))
    nen = cong(nen, to_mau(suong, (110, 170, 230)))
    # manh bang dang lao toi (thuong nguon)
    bang = moi()
    db = ImageDraw.Draw(bang)
    for pts, b in dong:
        if len(pts) < 30 or abs(b) > 0.85:
            continue
        for _ in range(2):
            i = rd.randint(3, max(4, len(pts) // 3))
            (x, y), (x2, y2) = pts[i], pts[i + 1]
            g = math.atan2(y2 - y, x2 - x)
            _manh_bang(db, x, y, g, rd.uniform(0.045, 0.075), rd.uniform(0.012, 0.02), (110, 175, 235), (240, 252, 255))
            # vet buot phia sau manh
            db.line([P(x, y), P(x - math.cos(g) * 0.09, y - math.sin(g) * 0.09)], fill=(70, 120, 190), width=3)
    nen = cong(nen, phat_sang(bang, 7, 0.9))
    k, mn, vien, _ = khien(TAM[0], TAM[1], CAO, (24, 70, 140), (150, 220, 255), an_bang, 102)
    nen = Image.composite(k, nen, mn)
    # suong gia bam mat khien phia va cham
    sg = moi()
    ds = ImageDraw.Draw(sg)
    for _ in range(70):
        g = rd.uniform(0, 2 * math.pi)
        rr = rd.uniform(0.0, 0.16)
        x, y = va[0] + math.cos(g) * rr - HUONG[0] * 0.05, va[1] + math.sin(g) * rr - HUONG[1] * 0.05
        _gai_suong(ds, x, y, rd.uniform(0, 6.28), rd.uniform(0.02, 0.045), 3, rd, (200, 235, 255))
    sg = nhan(sg, ImageChops.multiply(mn, ban_kinh_tu_tam(va[0], va[1], 0.03, 0.30)))
    nen = cong(nen, phat_sang(sg, 4, 0.8))
    lanh = vanh_sang_gan(mn, va, 0.02, 0.40)
    nen = cong(nen, to_mau(lanh, (190, 235, 255)), sang(to_mau(mo(lanh, 10), (90, 170, 255)), 1.1))
    # manh vo toe ra hai ben mep + tinh the lap lanh
    vo = moi()
    dv = ImageDraw.Draw(vo)
    g0 = math.atan2(-HUONG[1], -HUONG[0])
    for _ in range(30):
        c = rd.choice([-1, 1])
        g = g0 + c * rd.uniform(0.9, 1.7)
        l = rd.uniform(0.04, 0.2)
        x, y = va[0] + math.cos(g) * l, va[1] + math.sin(g) * l
        r = rd.uniform(0.006, 0.016)
        gg = rd.uniform(0, 6.28)
        dv.polygon([P(x + math.cos(gg) * r, y + math.sin(gg) * r), P(x + math.cos(gg + 2.3) * r * 0.7, y + math.sin(gg + 2.3) * r * 0.7),
                    P(x + math.cos(gg + 4.0) * r * 0.9, y + math.sin(gg + 4.0) * r * 0.9)], fill=lerp((140, 200, 255), (255, 255, 255), rd.random()))
    for _ in range(10):
        ngoi_sao(dv, rd.uniform(0.2, 0.85) * W, rd.uniform(0.15, 0.85) * W, rd.uniform(6, 13), (225, 245, 255))
    nen = cong(nen, phat_sang(vo, 5, 1.0))
    return nen


# ------------------------------------------------------------------ KHANG SET

def khang_set():
    rd = random.Random(41)
    nen = moi()
    va = diem_va_cham(TAM, AX, AY, HUONG)
    # tia set giang tu goc tren phai trung vanh khien
    loi, quang = moi(), moi()
    cay = cay_tia((0.93, 0.05), va, rd, 0.20, 6, 3)
    ve_cay_tia(cay, loi, quang, 9, 30, (130, 90, 255))
    # dien chay vong theo vanh khien roi phong xuong dat o hai ben mui khien
    k, mn, vien, _ = khien(TAM[0], TAM[1], CAO, (70, 40, 150), (205, 170, 255), an_set, 103)
    i_va = min(range(len(vien)), key=lambda i: (vien[i][0] - va[0]) ** 2 + (vien[i][1] - va[1]) ** 2)
    n = len(vien)
    for chieu in (-1, 1):
        dai = int(n * rd.uniform(0.33, 0.45))
        pts = []
        for j in range(dai):
            p = vien[(i_va + chieu * j) % n]
            pts.append(p)
        pts = pts[::2]
        cham = []
        for a, b in zip(pts, pts[1:]):
            cham += cay_tia(a, b, rd, 0.35, 2, 0, 2)[0][0][:-1]
        cham.append(pts[-1])
        dl, dq = ImageDraw.Draw(loi), ImageDraw.Draw(quang)
        dl.line([P(*p) for p in cham], fill=(235, 225, 255), width=4, joint="curve")
        dq.line([P(*p) for p in cham], fill=(120, 80, 255), width=16, joint="curve")
        # nhanh nho bat ra ngoai vanh
        for _ in range(4):
            p = rd.choice(pts[1:])
            g = math.atan2(p[1] - TAM[1], p[0] - TAM[0]) + rd.uniform(-0.5, 0.5)
            ve_cay_tia(cay_tia(p, (p[0] + math.cos(g) * rd.uniform(0.05, 0.11), p[1] + math.sin(g) * rd.uniform(0.05, 0.11)), rd, 0.3, 4, 0, 1),
                       loi, quang, 5, 16, (120, 80, 255))
    # phong xuong dat tu day khien
    day = vien[min(range(n), key=lambda i: -vien[i][1])]
    for dx in (-0.13, 0.05, 0.16):
        ve_cay_tia(cay_tia(day, (day[0] + dx, 0.93), rd, 0.3, 5, 1, 1), loi, quang, 5, 18, (120, 80, 255))
    tia = gop_tia(loi, quang, 1.0)
    # sau khien: phan tia nam sau bi khien che; truoc khien: phan chay tren vanh
    nen = cong(nen, tia)
    nen = Image.composite(k, nen, ImageChops.subtract(mn, mo(ImageChops.subtract(mn, mo(mn, 14).point(lambda v: 255 if v > 250 else 0)), 4)))
    vanh = ImageChops.multiply(mn, ImageChops.invert(mo(mn, 14).point(lambda v: 255 if v > 250 else 0)))
    nen = cong(nen, nhan(tia, mo(vanh, 3)))
    # loe o cho trung + mep khien nhiem dien
    nen = cong(nen, to_mau(ban_kinh_tu_tam(va[0], va[1], 0.0, 0.07, 1.0, 0.0), (240, 230, 255)),
               to_mau(ban_kinh_tu_tam(va[0], va[1], 0.0, 0.22, 0.6, 0.0), (110, 70, 230)))
    tl = moi()
    dtl = ImageDraw.Draw(tl)
    for _ in range(40):
        g = rd.uniform(0, 6.28)
        l = rd.uniform(0.03, 0.12)
        x0, y0 = va[0] + math.cos(g) * 0.015, va[1] + math.sin(g) * 0.015
        dtl.line([P(x0, y0), P(x0 + math.cos(g) * l, y0 + math.sin(g) * l)], fill=(225, 205, 255), width=2)
    # tia lua dien o chan
    for _ in range(18):
        x = day[0] + rd.uniform(-0.2, 0.22)
        y = 0.9 + rd.uniform(-0.02, 0.03)
        ngoi_sao(dtl, x * W, y * W, rd.uniform(5, 11), (210, 190, 255))
    nen = cong(nen, phat_sang(tl, 5, 1.2))
    return nen


# ------------------------------------------------------------------ KHANG PHONG

def khang_phong():
    rd = random.Random(51)
    nen = moi()
    dong = dong_chay(TAM, AX, AY, HUONG, 22, 1.05, 51)
    gio = moi().convert("L")
    dg = ImageDraw.Draw(gio)
    for idx, (pts, b) in enumerate(dong):
        n = len(pts)
        gan = max(0.0, 1.0 - abs(b) / 1.05)
        pha = rd.uniform(0, 6.28)
        for i in range(n - 1):
            t = i / max(1, n)
            dut = max(0.0, math.sin(t * 9 + pha)) ** 0.6           # dai gio dut quang thanh tung vet dai
            val = int(255 * min(1.0, t * 6) * max(0.0, 1 - max(0.0, t - 0.5) / 0.5) * (0.5 + 0.5 * gan) * dut)
            dg.line([P(*pts[i]), P(*pts[i + 1])], fill=val, width=int(4 + 9 * gan))
    gio = mo(gio, 2)
    nen = cong(nen, to_mau(gio, (215, 222, 210)), sang(to_mau(mo(gio, 16), (120, 135, 118)), 0.9))
    va = diem_va_cham(TAM, AX, AY, HUONG)
    # bui cuon o chan va quanh cho va cham
    bui = ImageChops.multiply(ban_kinh_tu_tam(0.30, 0.86, 0.0, 0.24, 1.0, 0.0), fbm(53, 10, 4))
    bui = ImageChops.add(bui, ImageChops.multiply(ban_kinh_tu_tam(va[0], va[1], 0.0, 0.15, 0.8, 0.0), fbm(54, 12, 4)))
    nen = cong(nen, to_mau(bui, (105, 92, 72)))
    k, mn, vien, _ = khien(TAM[0], TAM[1], CAO, (74, 86, 70), (200, 212, 190), an_phong, 104)
    nen = Image.composite(k, nen, mn)
    ria = vanh_sang_gan(mn, va, 0.02, 0.45)
    nen = cong(nen, to_mau(ria, (225, 232, 220)), sang(to_mau(mo(ria, 10), (150, 165, 145)), 0.8))
    # vet gio vut qua mep truoc mat khien (mong)
    truoc = ImageChops.multiply(gio, ImageChops.invert(mo(mn, 6)))
    nen = cong(nen, sang(to_mau(truoc, (230, 235, 225)), 0.5))
    # la kho, da vun bi cuon theo dong (sau khi qua khien)
    vat = moi()
    dv = ImageDraw.Draw(vat)
    for pts, b in dong[::2]:
        if len(pts) < 40:
            continue
        i = rd.randint(len(pts) // 2, len(pts) - 5)
        x, y = pts[i]
        g = rd.uniform(0, 6.28)
        if rd.random() < 0.55:
            r = rd.uniform(0.010, 0.017)
            la = [(math.cos(g) * r * 1.6, math.sin(g) * r * 1.6), (math.cos(g + 1.9) * r * 0.6, math.sin(g + 1.9) * r * 0.6),
                  (math.cos(g + 3.14) * r * 1.6, math.sin(g + 3.14) * r * 1.6), (math.cos(g - 1.9) * r * 0.6, math.sin(g - 1.9) * r * 0.6)]
            dv.polygon([P(x + u, y + v) for (u, v) in la], fill=lerp((95, 60, 25), (170, 120, 55), rd.random()))
        else:
            r = rd.uniform(0.007, 0.014)
            da = [(math.cos(g + a) * r * rd.uniform(0.7, 1.2), math.sin(g + a) * r * rd.uniform(0.7, 1.2)) for a in (0, 1.3, 2.4, 3.6, 4.9)]
            dv.polygon([P(x + u, y + v) for (u, v) in da], fill=lerp((70, 68, 66), (140, 136, 128), rd.random()))
    nen = cong(nen, phat_sang(vat, 4, 0.5))
    return nen


# ------------------------------------------------------------------ TOC DO DI CHUYEN

def _mn(pts, lam_muot=0):
    m = Image.new("L", (W, W), 0)
    ImageDraw.Draw(m).polygon([P(*p) for p in (chaikin(pts, lam_muot) if lam_muot else pts)], fill=255)
    return mo(m, 1.2)


def _thep(xam):
    return dai_mau(xam, [(0.0, (6, 6, 9)), (0.35, (55, 55, 62)), (0.62, (128, 126, 136)), (0.85, (200, 198, 208)), (1.0, (255, 252, 245))])


def _chieu(mn, r, d, k=3):
    """(sang, toi) tu noi khoi den tren trai."""
    nk = noi_khoi(mn, r, d)
    return nk.point(lambda v: min(255, max(0, v - 128) * k)), nk.point(lambda v: min(255, max(0, 128 - v) * k))


# Ung sat gothic nhin nghieng, mui quay sang phai
UNG = [(0.395, 0.235), (0.600, 0.235), (0.588, 0.300), (0.585, 0.470), (0.615, 0.545), (0.690, 0.590), (0.765, 0.622),
       (0.815, 0.648), (0.842, 0.628), (0.838, 0.668), (0.800, 0.705), (0.720, 0.718), (0.560, 0.712), (0.525, 0.698),
       (0.500, 0.722), (0.418, 0.722), (0.405, 0.680), (0.382, 0.610), (0.402, 0.480), (0.405, 0.300)]
MUI_THEP = [(0.655, 0.575), (0.690, 0.590), (0.765, 0.622), (0.815, 0.648), (0.842, 0.628), (0.838, 0.668), (0.800, 0.705),
            (0.720, 0.712), (0.680, 0.700), (0.662, 0.640)]
GIAP_ONG = [(0.535, 0.300), (0.588, 0.300), (0.585, 0.470), (0.605, 0.530), (0.560, 0.520), (0.540, 0.440)]
CO_UNG = [(0.385, 0.215), (0.612, 0.215), (0.600, 0.300), (0.398, 0.300)]
DE = [(0.405, 0.690), (0.520, 0.690), (0.560, 0.700), (0.800, 0.692), (0.800, 0.705), (0.720, 0.722), (0.560, 0.716),
      (0.525, 0.700), (0.500, 0.726), (0.415, 0.726)]


def toc_do():
    rd = random.Random(61)
    nen = moi()
    xanh = (110, 235, 120)
    # --- vet gio xe ngang phia sau
    vet = moi()
    dv = ImageDraw.Draw(vet)
    for _ in range(20):
        y = rd.uniform(0.30, 0.74)
        x1 = rd.uniform(0.28, 0.44)
        dai = rd.uniform(0.14, 0.30)
        for j in range(12):
            t = j / 12
            dv.line([P(x1 - dai * t, y), P(x1 - dai * (t + 1 / 12), y)], fill=lerp((0, 0, 0), (160, 235, 150), (1 - t) * rd.uniform(0.35, 0.9)),
                    width=max(2, int(6 * (1 - t))))
    nen = cong(nen, phat_sang(mo(vet, 1.5), 8, 0.6))
    # --- CANH MA sau co ung: nam long vu dai thon, tung len ra sau, ruot van khoi
    canh = Image.new("L", (W, W), 0)
    dc = ImageDraw.Draw(canh)
    goc = (0.44, 0.36)
    gan_long = []
    for k in range(7):
        g = math.radians(172 + k * 11)                           # tu ngang ra sau -> xien len
        dai = 0.36 - k * 0.022
        tren, duoi, truc = [], [], []
        for j in range(30):
            u = j / 29.0
            gg = g + 0.30 * u * u                                  # dau long vut cong len
            x = goc[0] + math.cos(gg) * dai * u
            y = goc[1] + math.sin(gg) * dai * u
            r = 0.030 * math.sin(math.pi * min(1.0, 0.12 + u * 0.95)) ** 0.7 * (1 - u) ** 0.35 + 0.003
            nx, ny = -math.sin(gg), math.cos(gg)
            tren.append((x + nx * r, y + ny * r))
            duoi.append((x - nx * r * 0.6, y - ny * r * 0.6))
            truc.append((x, y))
        dc.polygon([P(*p) for p in tren + duoi[::-1]], fill=120 + k * 18)
        gan_long.append(truc)
    canh = mo(canh, 1.5)
    van = fbm(66, 16, 3)
    canh_s = ImageChops.multiply(canh, van.point(lambda v: min(255, 70 + v)))
    nen = cong(nen, to_mau(vien_sang(canh, 8, 2.4), xanh), to_mau(canh_s.point(lambda v: v // 3), (60, 150, 70)),
               sang(to_mau(mo(canh, 22), (40, 120, 50)), 0.7))
    gl = moi()
    dgl = ImageDraw.Draw(gl)
    for truc in gan_long:
        dgl.line([P(*p) for p in truc[1:-4]], fill=(200, 255, 190), width=3, joint="curve")
    nen = cong(nen, phat_sang(mo(gl, 1), 4, 0.8))
    # --- lua ma toe ra tu got
    lua = Image.new("L", (W, W), 0)
    dl = ImageDraw.Draw(lua)
    for _ in range(14):
        y = rd.uniform(0.60, 0.72)
        dai = rd.uniform(0.10, 0.24)
        x0 = 0.41
        for j in range(10):
            t = j / 10
            dl.line([P(x0 - dai * t, y + 0.02 * math.sin(t * 6 + y * 40)), P(x0 - dai * (t + 0.1), y + 0.02 * math.sin((t + 0.1) * 6 + y * 40))],
                    fill=int(255 * (1 - t)), width=max(2, int(16 * (1 - t))))
    lua = ImageChops.multiply(mo(lua, 5), fbm(67, 12, 3).point(lambda v: min(255, v * 2)))
    nen = cong(nen, dai_mau(lua, [(0.0, (0, 0, 0)), (0.4, (20, 90, 30)), (0.75, (110, 230, 110)), (1.0, (230, 255, 220))]))
    # --- bui dat tung o de
    bui = ImageChops.multiply(ban_kinh_tu_tam(0.40, 0.76, 0.0, 0.18, 1.0, 0.0), fbm(62, 11, 4))
    nen = cong(nen, to_mau(bui, (110, 92, 66)))

    # --- UNG: da den, mui thep, giap ong chan, co ung ve, de, dai khoa
    mn = _mn(UNG)
    da_xam = fbm(68, 22, 4).point(lambda v: 55 + v * 45 // 255)
    sg, tg = _chieu(mn, 12, 6, 3)
    da_xam = ImageChops.subtract(ImageChops.add(da_xam, sg), tg)
    # nep gap da o co chan
    nep = Image.new("L", (W, W), 0)
    dn = ImageDraw.Draw(nep)
    for (y, x0, x1) in [(0.50, 0.41, 0.56), (0.535, 0.40, 0.575), (0.57, 0.40, 0.60)]:
        dn.arc([P(x0, y - 0.03), P(x1, y + 0.03)], 10, 170, fill=90, width=4)
    da_xam = ImageChops.add(da_xam, mo(nep, 2))
    da = dai_mau(da_xam, [(0.0, (0, 0, 0)), (0.35, (20, 15, 12)), (0.6, (52, 39, 30)), (0.82, (108, 84, 64)), (1.0, (200, 170, 138))])
    anh = nhan(da, mn)
    for vung, day in [(MUI_THEP, 8), (GIAP_ONG, 6), (CO_UNG, 6)]:
        m = ImageChops.multiply(_mn(vung), mn if vung is not CO_UNG else _mn(vung))
        x = fbm(69 + day, 14, 4).point(lambda v: 90 + v * 60 // 255)
        s1, t1 = _chieu(m, day, 5, 3)
        x = ImageChops.subtract(ImageChops.add(x, s1), t1)
        anh = Image.composite(_thep(x), anh, m)
    # mui thep chia khop (ba duong cong ngang qua mu ban chan)
    kh = moi()
    dk = ImageDraw.Draw(kh)
    for (x, r) in [(0.705, 0.07), (0.755, 0.06)]:
        dk.arc([P(x - r * 0.5, 0.60), P(x + r * 0.5, 0.74)], 200, 330, fill=(20, 20, 24), width=5)
        dk.arc([P(x - r * 0.5 + 0.006, 0.60), P(x + r * 0.5 + 0.006, 0.74)], 200, 330, fill=(200, 198, 210), width=2)
    anh = cong(nhan(anh, ImageChops.invert(mo(kh.convert("L"), 1)).point(lambda v: 60 + v * 195 // 255)), sang(kh, 0.0))
    # de ung
    de = ImageChops.multiply(_mn(DE), mn)
    anh = Image.composite(Image.new("RGB", (W, W), (14, 11, 10)), anh, de)
    # hai dai da + khoa sat quanh co chan
    for (y, h) in [(0.405, 0.036), (0.470, 0.036)]:
        dai = ImageChops.multiply(_mn([(0.39, y), (0.60, y - 0.012), (0.60, y - 0.012 + h), (0.39, y + h)]), mn)
        xd = fbm(70, 18, 3).point(lambda v: 60 + v * 40 // 255)
        s2, t2 = _chieu(dai, 3, 3, 3)
        xd = ImageChops.subtract(ImageChops.add(xd, s2), t2)
        anh = Image.composite(dai_mau(xd, [(0.0, (0, 0, 0)), (0.5, (40, 26, 18)), (1.0, (150, 110, 80))]), anh, dai)
        kh = Image.new("L", (W, W), 0)
        ImageDraw.Draw(kh).rectangle([P(0.545, y - 0.004), P(0.585, y + h - 0.004)], outline=255, width=6)
        kh = mo(kh, 1)
        s3, t3 = _chieu(kh, 2, 2, 3)
        anh = Image.composite(_thep(ImageChops.subtract(ImageChops.add(Image.new("L", (W, W), 140), s3), t3)), anh, kh)
    # dinh tan tren giap ong + co ung
    dt = moi()
    ddt = ImageDraw.Draw(dt)
    for (x, y) in [(0.562, 0.325), (0.566, 0.375), (0.572, 0.43), (0.42, 0.258), (0.50, 0.258), (0.58, 0.258), (0.70, 0.64), (0.78, 0.665)]:
        r = 0.009
        ddt.ellipse([P(x - r, y - r), P(x + r, y + r)], fill=(26, 25, 28))
        ddt.ellipse([P(x - r * 0.7, y - r * 0.7), P(x + r * 0.3, y + r * 0.3)], fill=(150, 148, 156))
        ddt.ellipse([P(x - r * 0.5, y - r * 0.5), P(x - r * 0.05, y - r * 0.05)], fill=(245, 242, 235))
    anh = Image.composite(dt, anh, dt.convert("L").point(lambda v: 255 if v > 0 else 0))
    # banh thuc o got
    bt = moi()
    db = ImageDraw.Draw(bt)
    cx, cy = 0.365, 0.655
    db.line([P(0.40, 0.655), P(cx, cy)], fill=(150, 148, 156), width=6)
    for k in range(8):
        g = k * math.pi / 4
        db.line([P(cx, cy), P(cx + math.cos(g) * 0.028, cy + math.sin(g) * 0.028)], fill=(190, 188, 196), width=4)
    db.ellipse([P(cx - 0.008, cy - 0.008), P(cx + 0.008, cy + 0.008)], fill=(230, 228, 236))
    # bong toi dan ve de, ghep ung len nen
    anh = nhan(anh, doc_theo_y(lambda y: 1.0 - 0.35 * max(0.0, min(1.0, (y - 0.23) / 0.5))))
    goc_xoay, tam_xoay = -9, (0.50 * W, 0.47 * W)
    anh = anh.rotate(goc_xoay, Image.BICUBIC, center=tam_xoay)
    mn = mn.rotate(goc_xoay, Image.BICUBIC, center=tam_xoay)
    bt = bt.rotate(goc_xoay, Image.BICUBIC, center=tam_xoay)
    nen = cong(Image.composite(anh, nen, mn), bt)
    # anh sang ma xanh hat len mep sau ung + vien sang tren trai
    mep_sau = ImageChops.multiply(vien_sang(mn, 10, 2.5), ban_kinh_tu_tam(0.38, 0.45, 0.05, 0.30))
    nen = cong(nen, to_mau(mep_sau, (90, 220, 100)))
    # vien sang mong quanh ca chiec ung - de noi tren dia nut toi o co nho
    nen = cong(nen, to_mau(vien_sang(mn, 5, 2.0), (120, 150, 115)), sang(to_mau(mo(mn, 18), (40, 80, 40)), 0.35))
    # hat ma lot theo sau
    hat = moi()
    dh = ImageDraw.Draw(hat)
    for _ in range(55):
        x = rd.uniform(0.12, 0.45)
        y = rd.uniform(0.22, 0.74)
        r = rd.uniform(2, 5)
        dh.rectangle([x * W - r, y * W - r, x * W + r, y * W + r], fill=lerp((40, 110, 40), (190, 255, 170), rd.random()))
    nen = cong(nen, phat_sang(hat, 5, 0.9))
    return nen


if __name__ == "__main__":
    ds = [("KhangLua", khang_lua, (0.16, 0.05, 0.02)), ("KhangBang", khang_bang, (0.03, 0.08, 0.14)),
          ("KhangSet", khang_set, (0.08, 0.05, 0.16)), ("KhangPhong", khang_phong, (0.08, 0.09, 0.08)),
          ("TocDo", toc_do, (0.06, 0.10, 0.05))]
    chi = [a for a in sys.argv[1:] if not a.startswith("--") and a in [d[0] for d in ds]]
    kq = []
    for ten, ham, nen_he in ds:
        if chi and ten not in chi:
            continue
        kq.append((ten, xuat(ham(), ten), nen_he))
    if "--xem" in sys.argv:
        thu_muc = sys.argv[sys.argv.index("--xem") + 1]
        tam = Image.new("RGB", (len(kq) * (S + 10) + 20 + len(kq) * 100, S * 2 + 20), (24, 22, 26))
        for i, (ten, anh, nen_he) in enumerate(kq):
            tam.paste(anh.convert("RGB"), (i * (S + 10), 0))
            nut = dia_nut(anh, nen_he)
            tam.paste(nut, (i * (S + 10), S + 10), nut)
            nho = nut.resize((84, 84), Image.LANCZOS)
            tam.paste(nho, (len(kq) * (S + 10) + 20 + i * 100, S + 10), nho)
        tam.save(os.path.join(thu_muc, "icon_moi_4.png"))
        print("xem truoc:", os.path.join(thu_muc, "icon_moi_4.png"))
