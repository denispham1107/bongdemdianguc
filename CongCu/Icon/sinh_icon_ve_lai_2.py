# -*- coding: utf-8 -*-
# VE LAI NAM ICON: QUA CAU LUA, MUA BANG, SAM SET, GIUT SET, QUA CAU DIEN (28/09/2026).
#
# Nguoi dung: nam icon cu "qua tho va so sai, net ve nhu game tre em" - ve lai cho hop noi dung tung ky nang va boi canh
# kinh di cua game; "khong can su dung MCP Blender". Cung bo ham voi sinh_icon_ve_lai.py (ba icon ve lai truoc do).
#
#   - Qua cau lua (Lua): CHUM BA qua (ky nang ban 3 qua toe quat), qua chinh lua cuon du doi + duoi lua dai. Ve bang TRUONG
#     NHIET (anh xam: vien cau, luoi lua, duoi) xe bang nhieu roi to bang dai mau lua - cach ve lua ma khong can mo phong.
#   - Mua bang (Bang): may bang gia phia tren, cac VET SAO BANG roi THANG DUNG (dung hinh Mua bang trong game), duoi dat moc
#     cum tinh the bang pha le + vong suong.
#   - Sam set (Set): may giong tim den loe tu ben trong, tia set lon re nhanh giang xuong dat, cho chop + vong xung kich.
#   - Giut set (GiatSet): luong dien trang vien XANH DAM phong tu nguon phep, re nhanh roi LAN sang nhieu muc tieu.
#   - Qua cau dien (CauDien): loi TOI + vanh sang, vo tia dien DUT QUANG bao quanh (nhu hinh trong game), 5 tia ban ra 5 huong.
#
# Chay: python CongCu/Icon/sinh_icon_ve_lai_2.py  [--xem <thu muc>]
import math
import os
import random
import sys
from PIL import Image, ImageChops, ImageDraw, ImageFilter

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from sinh_icon_ve_lai import (W, S, RA, moi, P, cong, sang, mo, phat_sang, to_mau, nhan, nhieu, fbm, ban_kinh_tu_tam,
                              ngoi_sao, lerp, thang_mau, xuat, dia_nut)


# ------------------------------------------------------------------ tien ich rieng

def dai_mau(xam, bang):
    """To anh xam L bang dai mau [(0..1, (r,g,b))...] -> RGB."""
    lut = [thang_mau(bang, i / 255.0) for i in range(256)]
    r = xam.point([c[0] for c in lut])
    g = xam.point([c[1] for c in lut])
    b = xam.point([c[2] for c in lut])
    return Image.merge("RGB", (r, g, b))


def duong_tia(p0, p1, lech, sau, rd):
    """Tia set: doi diem giua ngau nhien vuong goc (midpoint displacement)."""
    ds = [p0, p1]
    for k in range(sau):
        moi_ds = [ds[0]]
        for a, b in zip(ds, ds[1:]):
            dx, dy = b[0] - a[0], b[1] - a[1]
            l = math.hypot(dx, dy) + 1e-9
            nx, ny = -dy / l, dx / l
            o = rd.gauss(0, 1) * lech * l
            moi_ds.append(((a[0] + b[0]) / 2 + nx * o, (a[1] + b[1]) / 2 + ny * o))
            moi_ds.append(b)
        ds = moi_ds
    return ds


def cay_tia(p0, p1, rd, lech=0.18, sau=6, nhanh=3, cap=0):
    """Tia chinh + nhanh (moi nhanh la mot cay nho hon). Tra ve [(diem, cap)]."""
    chinh = duong_tia(p0, p1, lech, sau, rd)
    kq = [(chinh, cap)]
    if cap >= 2:
        return kq
    n = len(chinh)
    for _ in range(nhanh):
        i = rd.randint(n // 6, int(n * 0.8))
        a = chinh[i]
        dx, dy = p1[0] - p0[0], p1[1] - p0[1]
        g = math.atan2(dy, dx) + rd.choice([-1, 1]) * rd.uniform(0.35, 0.9)
        dai = math.hypot(dx, dy) * rd.uniform(0.18, 0.42) * (0.7 if cap else 1.0)
        b = (a[0] + math.cos(g) * dai, a[1] + math.sin(g) * dai)
        kq += cay_tia(a, b, rd, lech, max(3, sau - 2), max(0, nhanh - 2), cap + 1)
    return kq


def ve_cay_tia(cay, loi, quang, rong_loi=7, rong_quang=26, mau_quang=(60, 110, 255)):
    """Ve tia len hai lop: loi trang manh, quang mau rong (sau se lam mo)."""
    dl, dq = ImageDraw.Draw(loi), ImageDraw.Draw(quang)
    for diem, cap in cay:
        k = [1.0, 0.55, 0.32][cap]
        pts = [P(*p) for p in diem]
        dl.line(pts, fill=lerp((0, 0, 0), (255, 255, 255), 0.55 + 0.45 * k), width=max(2, int(rong_loi * k)), joint="curve")
        dq.line(pts, fill=lerp((0, 0, 0), mau_quang, 0.6 + 0.4 * k), width=max(4, int(rong_quang * k)), joint="curve")


def gop_tia(loi, quang, k_quang=1.0):
    """Gop: quang mo hai tang + loi co vien mo nhe."""
    q1 = mo(quang, 10)
    q2 = mo(quang, 34)
    l = cong(loi, sang(mo(loi, 3), 1.2))
    return cong(sang(q2, 1.3 * k_quang), sang(q1, 1.1 * k_quang), l)


def may(vung, hat, toi, sang_vien, rd):
    """Mang may: cum BONG XOP (nhieu elip nho quanh cac vung) xe bang nhieu. vung = [(x, y, rx, ry)...].
    Tra ve (mat na L, lop RGB). Mat duoi sang hon (duoc chop / tia roi len)."""
    mn = Image.new("L", (W, W), 0)
    d = ImageDraw.Draw(mn)
    for (x, y, rx, ry) in vung:
        d.ellipse([P(x - rx, y - ry * 0.6), P(x + rx, y + ry * 0.6)], fill=200)
        for _ in range(14):
            bx = x + rd.uniform(-rx, rx) * 0.85
            by = y + rd.uniform(-ry, ry * 0.3)
            br = rd.uniform(0.35, 0.75) * ry * 1.4
            d.ellipse([P(bx - br, by - br), P(bx + br, by + br)], fill=rd.randint(170, 255))
    mn = mo(mn, 9)
    xe = fbm(hat, 9, 4)
    mn = ImageChops.multiply(mn, xe.point(lambda v: min(255, 60 + int(v * 0.9))))
    mn = mn.point(lambda v: min(255, int(v * 1.6)))
    # sang trong long may theo nhieu (cuon)
    cuon = ImageChops.multiply(mn, fbm(hat + 5, 14, 3).point(lambda v: max(0, v - 110) * 2))
    duoi = ImageChops.subtract(mn, ImageChops.offset(mn, 0, 22))
    lop = cong(to_mau(mn, toi), to_mau(cuon, tuple(int(c * 0.45) for c in sang_vien)), to_mau(mo(duoi, 5), sang_vien))
    return mn, lop


# ------------------------------------------------------------------ QUA CAU LUA

def qua_cau_lua():
    rd = random.Random(101)
    huong = (-0.72, 0.69)                       # bay len tren phai, duoi keo ve duoi trai
    g_duoi = math.atan2(huong[1], huong[0])
    bong = [(0.62, 0.38, 0.13, 1.0), (0.32, 0.25, 0.062, 0.75), (0.76, 0.70, 0.062, 0.75)]   # chum BA qua

    # ---- Luoi lua: truong nhiet ----
    nhiet = Image.new("L", (W, W), 0)
    d = ImageDraw.Draw(nhiet)
    for (cx, cy, R, manh) in bong:
        for lop_i, (so, dai_k, v) in enumerate([(13, 1.0, 95), (11, 0.7, 150), (8, 0.45, 205)]):
            for _ in range(so):
                g = g_duoi + rd.uniform(-0.32, 0.32) * (1.0 - 0.3 * lop_i)
                goc_bat_dau = g + rd.uniform(-1.1, 1.1)
                x0 = cx + math.cos(goc_bat_dau) * R * 0.8
                y0 = cy + math.sin(goc_bat_dau) * R * 0.8
                dai = R * rd.uniform(1.5, 2.8) * dai_k * (1.5 if R > 0.1 else 1.2)
                rong = R * rd.uniform(0.26, 0.42)
                nx, ny = -math.sin(g), math.cos(g)
                ngon = (x0 + math.cos(g) * dai, y0 + math.sin(g) * dai)
                cong_v = rd.uniform(-0.25, 0.25) * dai
                giua = (x0 + math.cos(g) * dai * 0.5 + nx * cong_v * 0.4, y0 + math.sin(g) * dai * 0.5 + ny * cong_v * 0.4)
                d.polygon([P(x0 + nx * rong, y0 + ny * rong), P(giua[0] + nx * rong * 0.45, giua[1] + ny * rong * 0.45), P(*ngon),
                           P(giua[0] - nx * rong * 0.45, giua[1] - ny * rong * 0.45), P(x0 - nx * rong, y0 - ny * rong)],
                          fill=int(v * manh))
    nhiet = mo(nhiet, 7)
    xe = fbm(7, 12, 4)
    nhiet = ImageChops.multiply(nhiet, xe.point(lambda v: min(255, 40 + int(v * 1.0))))
    lua = dai_mau(nhiet, [(0.0, (0, 0, 0)), (0.15, (40, 3, 0)), (0.35, (130, 20, 2)), (0.55, (215, 70, 8)),
                          (0.78, (255, 150, 35)), (1.0, (255, 215, 120))])
    lua = phat_sang(lua, 14, 0.6)

    # ---- Qua cau: be mat dung nham cuon + vien sang ----
    nen = lua
    for (cx, cy, R, manh) in bong:
        mn = Image.new("L", (W, W), 0)
        ImageDraw.Draw(mn).ellipse([P(cx - R, cy - R), P(cx + R, cy + R)], fill=255)
        mn = mo(mn, 2)
        be_mat = fbm(int(cx * 1000) + 3, 18, 4)
        tam = ban_kinh_tu_tam(cx - R * 0.15, cy - R * 0.15, 0.0, R * 1.05, 1.0, 0.25)
        tron = Image.blend(be_mat, tam, 0.55)
        mat = dai_mau(tron, [(0.0, (90, 12, 0)), (0.30, (200, 50, 4)), (0.55, (255, 130, 20)), (0.78, (255, 205, 90)),
                             (1.0, (255, 248, 215))])
        vien = ImageChops.multiply(mn, ImageChops.invert(mo(mn, R * W * 0.22))).point(lambda v: min(255, v * 2))
        mat = cong(mat, to_mau(vien, (255, 190, 80)))
        nen = Image.composite(mat, nen, mn)
        loe = moi()
        ImageDraw.Draw(loe).ellipse([P(cx - R * 1.05, cy - R * 1.05), P(cx + R * 1.05, cy + R * 1.05)], outline=(255, 150, 40), width=5)
        nen = cong(nen, phat_sang(mo(loe, 3), 14, 1.2))
        nen = cong(nen, to_mau(ban_kinh_tu_tam(cx, cy, R * 0.9, R * 2.2, 1.0, 0.0), (90, 28, 4)))

    # tan tro bay theo duoi
    tro = moi()
    dt = ImageDraw.Draw(tro)
    for _ in range(60):
        t = rd.uniform(0.1, 0.9)
        x = 0.60 + huong[0] * 0.5 * t + rd.uniform(-0.2, 0.2)
        y = 0.40 + huong[1] * 0.5 * t + rd.uniform(-0.2, 0.2)
        r = rd.uniform(2, 5)
        dt.ellipse([x * W - r, y * W - r, x * W + r, y * W + r], fill=lerp((230, 80, 10), (255, 220, 120), rd.random()))
    nen = cong(nen, phat_sang(tro, 5, 1.0))
    return nen


# ------------------------------------------------------------------ MUA BANG

def mua_bang():
    rd = random.Random(202)
    nen = moi()
    # may bang gia
    vung = [(0.50, 0.16, 0.30, 0.08), (0.30, 0.20, 0.17, 0.07), (0.70, 0.20, 0.17, 0.07), (0.50, 0.25, 0.26, 0.06)]
    _, lop_may = may(vung, 31, (26, 50, 88), (120, 190, 240), rd)
    nen = cong(nen, lop_may)

    # vong suong + cum tinh the tren dat
    vs = moi()
    dv = ImageDraw.Draw(vs)
    dv.ellipse([P(0.18, 0.76), P(0.82, 0.90)], outline=(90, 170, 230), width=5)
    dv.ellipse([P(0.28, 0.785), P(0.72, 0.875)], outline=(60, 130, 200), width=3)
    nen = cong(nen, phat_sang(vs, 10, 1.2))
    suong = Image.new("L", (W, W), 0)
    ImageDraw.Draw(suong).ellipse([P(0.14, 0.74), P(0.86, 0.93)], fill=150)
    suong = ImageChops.multiply(mo(suong, 26), fbm(37, 7, 3))
    nen = cong(nen, to_mau(suong, (110, 170, 220)))

    def cum(cx, cy, co):
        lop = moi()
        dl = ImageDraw.Draw(lop)
        dinh = [(-0.9, 0.55, -18), (-0.35, 1.0, -6), (0.15, 0.8, 9), (0.6, 0.62, 22), (0.0, 0.45, 0)]
        for (lx, cao, nghieng) in dinh:
            gx = cx + lx * co * 0.55
            h = cao * co * 1.6
            rong = co * 0.26
            g = math.radians(nghieng)
            tren = (gx + math.sin(g) * h, cy - math.cos(g) * h)
            t1 = (gx - rong, cy)
            t2 = (gx + rong, cy)
            vai1 = (gx - rong * 0.8 + math.sin(g) * h * 0.72, cy - math.cos(g) * h * 0.72)
            vai2 = (gx + rong * 0.8 + math.sin(g) * h * 0.72, cy - math.cos(g) * h * 0.72)
            # hai nua lang tru: nua trai toi, nua phai sang
            mid_d = (gx, cy)
            mid_t = (gx + math.sin(g) * h * 0.72, cy - math.cos(g) * h * 0.72)
            dl.polygon([P(*t1), P(*vai1), P(*tren), P(*mid_t), P(*mid_d)], fill=(24, 70, 140))
            dl.polygon([P(*mid_d), P(*mid_t), P(*tren), P(*vai2), P(*t2)], fill=(110, 190, 245))
            dl.line([P(*mid_d), P(*mid_t), P(*tren)], fill=(210, 245, 255), width=3)
            dl.line([P(*t1), P(*vai1), P(*tren), P(*vai2), P(*t2)], fill=(160, 225, 255), width=3)
        return phat_sang(lop, 8, 0.8)

    nen = cong(nen, cum(0.36, 0.83, 0.075))
    nen = cong(nen, cum(0.64, 0.84, 0.06))

    # vet sao bang roi thang dung
    vet = moi()
    dvt = ImageDraw.Draw(vet)
    dau = moi()
    dd = ImageDraw.Draw(dau)
    cot = [0.22, 0.31, 0.40, 0.48, 0.56, 0.64, 0.72, 0.80, 0.36, 0.60]
    for i, x in enumerate(cot):
        y_dau = rd.uniform(0.42, 0.74) if i < 8 else rd.uniform(0.34, 0.5)
        y_duoi = max(0.28, y_dau - rd.uniform(0.17, 0.32))
        buoc = 20
        for k in range(buoc):
            a = y_duoi + (y_dau - y_duoi) * k / buoc
            b = y_duoi + (y_dau - y_duoi) * (k + 1) / buoc
            f = (k + 1) / buoc
            dvt.line([P(x, a), P(x, b)], fill=lerp((0, 0, 0), (140, 210, 255), f ** 1.5), width=int(4 + 9 * f))
        r = 0.013
        dd.polygon([P(x, y_dau + r * 1.8), P(x + r * 0.7, y_dau), P(x, y_dau - r * 0.9), P(x - r * 0.7, y_dau)],
                   fill=(235, 250, 255))
    nen = cong(nen, phat_sang(mo(vet, 1.5), 10, 1.0), phat_sang(dau, 9, 1.8))

    # manh bang toe o cho cham dat + lap lanh
    lap = moi()
    dl = ImageDraw.Draw(lap)
    for _ in range(22):
        x, y = rd.uniform(0.2, 0.8), rd.uniform(0.66, 0.84)
        ngoi_sao(dl, *P(x, y), rd.uniform(5, 13), (200, 240, 255))
    nen = cong(nen, phat_sang(lap, 5, 1.0))
    return nen


# ------------------------------------------------------------------ SAM SET

def sam_set():
    rd = random.Random(303)
    nen = moi()
    # may giong tim den, loe tu ben trong
    vung = [(0.50, 0.16, 0.32, 0.09), (0.26, 0.21, 0.17, 0.07), (0.74, 0.21, 0.17, 0.07), (0.52, 0.26, 0.24, 0.06)]
    mn, lop_may = may(vung, 47, (36, 26, 64), (170, 140, 255), rd)
    nen = cong(nen, lop_may)
    loe = ImageChops.multiply(mn, ban_kinh_tu_tam(0.48, 0.24, 0.0, 0.16, 1.0, 0.0))
    nen = cong(nen, to_mau(loe, (150, 120, 255)))

    # cho giang: chop + vong xung kich + chay xem
    cham = moi()
    dc = ImageDraw.Draw(cham)
    dc.ellipse([P(0.30, 0.78), P(0.72, 0.88)], outline=(140, 120, 255), width=6)
    dc.ellipse([P(0.20, 0.75), P(0.82, 0.91)], outline=(80, 60, 190), width=3)
    dc.ellipse([P(0.43, 0.80), P(0.59, 0.86)], fill=(255, 255, 255))
    nen = cong(nen, phat_sang(mo(cham, 2), 18, 1.4))

    loi, quang = moi(), moi()
    ve_cay_tia(cay_tia((0.47, 0.27), (0.51, 0.83), rd, 0.11, 6, 4), loi, quang, 9, 30, (130, 95, 255))
    ve_cay_tia(cay_tia((0.27, 0.29), (0.31, 0.64), rd, 0.12, 5, 2), loi, quang, 5, 18, (110, 80, 240))
    ve_cay_tia(cay_tia((0.73, 0.29), (0.70, 0.56), rd, 0.12, 5, 1), loi, quang, 4, 14, (100, 80, 230))
    nen = cong(nen, gop_tia(loi, quang, 1.1))

    # tia lua dien ban ra
    tl = moi()
    dt = ImageDraw.Draw(tl)
    for _ in range(26):
        g = rd.uniform(math.pi * 1.05, math.pi * 1.95)
        r0, r1 = rd.uniform(0.02, 0.05), rd.uniform(0.08, 0.2)
        x, y = 0.51, 0.83
        dt.line([P(x + math.cos(g) * r0, y + math.sin(g) * r0 * 0.6), P(x + math.cos(g) * r1, y + math.sin(g) * r1 * 0.6)],
                fill=(200, 190, 255), width=3)
    nen = cong(nen, phat_sang(tl, 5, 1.0))
    return nen


# ------------------------------------------------------------------ GIUT SET

def giut_set():
    rd = random.Random(404)
    nen = moi()
    nguon = (0.17, 0.66)
    dich = [(0.52, 0.34), (0.82, 0.24), (0.80, 0.62), (0.56, 0.82)]
    loi, quang = moi(), moi()
    xanh = (40, 90, 255)
    # nguon -> ke thu nhat, roi LAN sang ke thu hai, ba; mot tia thang sang ke thu tu
    ve_cay_tia(cay_tia(nguon, dich[0], rd, 0.2, 6, 3), loi, quang, 8, 28, xanh)
    ve_cay_tia(cay_tia(dich[0], dich[1], rd, 0.22, 5, 1), loi, quang, 6, 22, xanh)
    ve_cay_tia(cay_tia(dich[0], dich[2], rd, 0.22, 5, 1), loi, quang, 6, 22, xanh)
    ve_cay_tia(cay_tia(nguon, dich[3], rd, 0.2, 6, 2), loi, quang, 7, 24, xanh)
    nen = cong(nen, gop_tia(loi, quang, 1.15))

    # nguon phep: cum dien bung ra
    ng = moi()
    dn = ImageDraw.Draw(ng)
    dn.ellipse([P(nguon[0] - 0.055, nguon[1] - 0.055), P(nguon[0] + 0.055, nguon[1] + 0.055)], fill=(60, 110, 255))
    dn.ellipse([P(nguon[0] - 0.03, nguon[1] - 0.03), P(nguon[0] + 0.03, nguon[1] + 0.03)], fill=(235, 245, 255))
    for _ in range(9):
        g = rd.uniform(0, 2 * math.pi)
        c = cay_tia(nguon, (nguon[0] + math.cos(g) * 0.1, nguon[1] + math.sin(g) * 0.1), rd, 0.3, 3, 0)
        for diem, _cap in c:
            dn.line([P(*p) for p in diem], fill=(200, 225, 255), width=3)
    nen = cong(nen, phat_sang(mo(ng, 2), 16, 1.4))

    # cho trung: chop hinh sao + vong nho
    ch = moi()
    dc = ImageDraw.Draw(ch)
    for (x, y) in dich:
        ngoi_sao(dc, *P(x, y), 34, (230, 240, 255), 0.12)
        dc.ellipse([P(x - 0.014, y - 0.014), P(x + 0.014, y + 0.014)], fill=(255, 255, 255))
        for _ in range(7):
            g = rd.uniform(0, 2 * math.pi)
            r0, r1 = rd.uniform(0.018, 0.03), rd.uniform(0.045, 0.075)
            dc.line([P(x + math.cos(g) * r0, y + math.sin(g) * r0), P(x + math.cos(g) * r1, y + math.sin(g) * r1)],
                    fill=(150, 190, 255), width=3)
    nen = cong(nen, phat_sang(ch, 12, 1.4))
    return nen


# ------------------------------------------------------------------ QUA CAU DIEN

def qua_cau_dien():
    rd = random.Random(505)
    nen = moi()
    cx, cy, R = 0.5, 0.5, 0.155

    # 5 tia ban ra 5 huong (moi ke mot tia)
    loi, quang = moi(), moi()
    for i in range(5):
        g = -math.pi / 2 + 2 * math.pi * i / 5 + rd.uniform(-0.25, 0.25)
        a = (cx + math.cos(g) * R * 1.35, cy + math.sin(g) * R * 1.35)
        b = (cx + math.cos(g) * 0.40, cy + math.sin(g) * 0.40)
        ve_cay_tia(cay_tia(a, b, rd, 0.22, 5, 1), loi, quang, 6, 22, (50, 120, 255))
    nen = cong(nen, gop_tia(loi, quang, 1.0))
    ch = moi()
    dc = ImageDraw.Draw(ch)
    for i in range(5):
        pass

    # hao quang vong khuyen
    vong = ImageChops.subtract(ban_kinh_tu_tam(cx, cy, R * 1.2, R * 1.9, 1.0, 0.0), ban_kinh_tu_tam(cx, cy, R * 0.9, R * 1.2, 1.0, 0.0))
    nen = cong(nen, to_mau(vong, (30, 90, 200)))

    # qua cau: loi TOI, vanh sang (fresnel), van xoay mo ben trong
    mn = Image.new("L", (W, W), 0)
    ImageDraw.Draw(mn).ellipse([P(cx - R, cy - R), P(cx + R, cy + R)], fill=255)
    vien = ImageChops.multiply(mn, ImageChops.invert(mo(mn, 30))).point(lambda v: min(255, v * 3))
    van = ImageChops.multiply(fbm(71, 10, 3).point(lambda v: max(0, v - 120) * 2), mn)
    cau = cong(to_mau(mn, (6, 12, 34)), to_mau(van, (40, 90, 200)), to_mau(vien, (150, 220, 255)))
    nen = Image.composite(cau, nen, mn)
    vs = moi()
    ImageDraw.Draw(vs).ellipse([P(cx - R, cy - R), P(cx + R, cy + R)], outline=(210, 240, 255), width=6)
    nen = cong(nen, phat_sang(vs, 12, 1.4))

    # vo tia dien DUT QUANG quanh cau (nam NGOAI mat cau)
    vo = moi()
    dv = ImageDraw.Draw(vo)
    for vong_i in range(5):
        nghieng = rd.uniform(0, math.pi)
        dep = rd.uniform(0.25, 0.9)
        r = R * rd.uniform(1.12, 1.3)
        g = rd.uniform(0, 2 * math.pi)
        while g < 2 * math.pi * 3:
            dai = rd.uniform(0.25, 0.7)
            pts = []
            for k in range(8):
                t = g + dai * k / 7
                x = math.cos(t) * r
                y = math.sin(t) * r * dep
                xr = x * math.cos(nghieng) - y * math.sin(nghieng)
                yr = x * math.sin(nghieng) + y * math.cos(nghieng)
                o = rd.uniform(-0.008, 0.008)
                pts.append(P(cx + xr + o, cy + yr + o))
            dv.line(pts, fill=(170, 225, 255), width=4)
            g += dai + rd.uniform(0.3, 0.9)
            if g > 2 * math.pi * (1 + vong_i % 2):
                break
    nen = cong(nen, phat_sang(vo, 8, 1.3))

    # loe o dau cac tia
    return nen


if __name__ == "__main__":
    ds = [("Lua", qua_cau_lua, (0.30, 0.09, 0.03)), ("Bang", mua_bang, (0.05, 0.14, 0.30)), ("Set", sam_set, (0.08, 0.06, 0.22)),
          ("GiatSet", giut_set, (0.04, 0.11, 0.28)), ("CauDien", qua_cau_dien, (0.02, 0.08, 0.20))]
    kq = []
    for ten, ham, nen_he in ds:
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
        tam.save(os.path.join(thu_muc, "icon_moi_2.png"))
        print("xem truoc:", os.path.join(thu_muc, "icon_moi_2.png"))
