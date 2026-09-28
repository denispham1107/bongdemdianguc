# -*- coding: utf-8 -*-
# VE LAI NAM ICON: GIO LOC, LOC XOAY, MAY GIONG, TOC BIEN, HOA LOC XOAY (28/09/2026).
#
# Nguoi dung: nam icon cu "qua tho va so sai, net ve nhu game tre em" - ve lai cho hop noi dung tung ky nang va boi canh
# kinh di cua game; "khong can su dung MCP Blender". Dung chung bo ham voi sinh_icon_ve_lai.py / _2.py.
#
#   - Gio loc (GioLoc): QUAT BA con loc nho xam trang (ky nang tung 3 loc toa 15 do), chan khoi bui den cuon, set nho trong long.
#   - Loc xoay (Loc): mot PHEU loc khong lo, mieng tren vanh may cuon, dai gio xoan di len, set trong long, bui + manh bia mo
#     va da bi cuon bay.
#   - May giong (MayGiong): may XAM DEN bi set roi sang tung mang, mua xoi, vai tia set, nuoc ban duoi dat.
#   - Toc bien (TocBien): bong phu thuy tan thanh hat tim o cho cu, vet khong gian + bong mo, hien ra o cho moi voi vong phep.
#   - Hoa loc xoay (HoaLocXoay): con loc nho o giua PHINH RA thanh pheu loc lon trong suot bao quanh, vong song lan rong.
#
# PHEU LOC: moi dai gio la mot duong xoan quanh truc (ban kinh theo do cao), nua TRUOC sang, nua SAU mo; mot chieu xoay
# (goc tang theo do cao, nhu ChieuQuayGioLoc trong game).
#
# Chay: python CongCu/Icon/sinh_icon_ve_lai_3.py  [--xem <thu muc>]
import math
import os
import random
import sys
from PIL import Image, ImageChops, ImageDraw, ImageFilter

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from sinh_icon_ve_lai import (W, S, moi, P, cong, sang, mo, phat_sang, to_mau, nhan, fbm, ban_kinh_tu_tam, chaikin,
                              ngoi_sao, lerp, thang_mau, xuat, dia_nut)
from sinh_icon_ve_lai_2 import dai_mau, cay_tia, ve_cay_tia, gop_tia, may


# ------------------------------------------------------------------ pheu loc

def pheu_loc(cx, day_y, dinh_y, r_day, r_dinh, hat, so_dai=40, mau=(205, 212, 220), lac=0.025, do_day=1.0, sang_k=1.0,
             mu=1.7, loe=0.0, loe_cao=0.09):
    """Ve mot pheu loc -> (lop RGB dai gio, mat na L than pheu)."""
    rd = random.Random(hat)
    lop = moi()
    d = ImageDraw.Draw(lop)
    H = day_y - dinh_y
    pha = rd.uniform(0, 6.28)

    # mu nho = than to ngang len som; loe = CHAN LOE rong ra sat dat (nguoi dung 28/09/2026: "nhin giong cay kem" - chan
    # nhon hoat, phai to ra) - tat dan theo do cao loe_cao
    def r_at(t):
        return r_day + (r_dinh - r_day) * (t ** mu) + loe * math.exp(-t / loe_cao)

    def x_at(t):
        return cx + math.sin(t * 3.2 + pha) * lac * (0.3 + t)

    # than pheu mo (trong suot)
    than = []
    buoc = 40
    for i in range(buoc + 1):
        t = i / buoc
        than.append((x_at(t) + r_at(t), day_y - H * t))
    for i in range(buoc, -1, -1):
        t = i / buoc
        than.append((x_at(t) - r_at(t), day_y - H * t))
    mn = Image.new("L", (W, W), 0)
    ImageDraw.Draw(mn).polygon([P(*p) for p in than], fill=255)
    mn = mo(mn, 6)

    # dai gio xoan: ve nua SAU truoc (mo) roi nua TRUOC (sang)
    dai = []
    for _ in range(so_dai):
        g0 = rd.uniform(0, 2 * math.pi)
        vong = rd.uniform(1.4, 2.8)
        t0 = rd.uniform(0.0, 0.75)
        t1 = min(1.0, t0 + rd.uniform(0.25, 0.7))
        dai.append((g0, vong, t0, t1, rd.uniform(0.55, 1.0)))
    for truoc in (False, True):
        for (g0, vong, t0, t1, k) in dai:
            n = 36
            cu = None
            for i in range(n + 1):
                t = t0 + (t1 - t0) * i / n
                g = g0 + vong * 2 * math.pi * t          # MOT chieu: goc tang theo do cao
                r = r_at(t)
                sau = math.sin(g)
                x = x_at(t) + r * math.cos(g)
                y = day_y - H * t + r * 0.16 * sau
                o_truoc = sau > 0
                if cu is not None and o_truoc == truoc:
                    mo_dau = min(1.0, min(i, n - i) / 6.0)       # hai dau dai mo dan
                    b = k * mo_dau * (0.55 + 0.45 * abs(sau)) * (1.0 if truoc else 0.38) * sang_k
                    rong = max(2, int((3 + 16 * r / max(r_dinh, 1e-3) * 0.5) * do_day * (1.0 if truoc else 0.7)))
                    d.line([P(*cu), P(x, y)], fill=lerp((0, 0, 0), mau, b), width=rong)
                cu = (x, y)
    lop = mo(lop, 1.6)
    # van gio: xe bang nhieu doc theo do cao
    lop = nhan(lop, fbm(hat, 10, 3).point(lambda v: min(255, 90 + v)))
    lop = cong(to_mau(ImageChops.multiply(mn, doc_theo_do_cao(day_y, dinh_y)), tuple(int(c * 0.22) for c in mau)), lop)
    return phat_sang(lop, 8, 0.6), mn


def doc_theo_do_cao(day_y, dinh_y):
    """Mat na: day pheu dac hon, dinh loang (than pheu trong suot dan len tren)."""
    col = Image.new("L", (1, W))
    ds = []
    for y in range(W):
        yy = (y + 0.5) / W
        t = (day_y - yy) / max(1e-6, day_y - dinh_y)
        ds.append(int(max(0.0, min(1.0, 0.9 - 0.5 * t)) * 255))
    col.putdata(ds)
    return col.resize((W, W))


def bui_chan(cx, cy, rong, hat, mau=(80, 70, 62), k=1.0):
    """Khoi bui den cuon quanh chan loc."""
    rd = random.Random(hat)
    mn = Image.new("L", (W, W), 0)
    d = ImageDraw.Draw(mn)
    for _ in range(22):
        x = cx + rd.uniform(-rong, rong)
        y = cy + rd.uniform(-rong * 0.25, rong * 0.12)
        r = rd.uniform(0.25, 0.55) * rong
        d.ellipse([P(x - r, y - r * 0.6), P(x + r, y + r * 0.6)], fill=rd.randint(120, 220))
    mn = ImageChops.multiply(mo(mn, 10), fbm(hat, 11, 3).point(lambda v: min(255, 50 + v)))
    return to_mau(mn.point(lambda v: min(255, int(v * k))), mau)


def set_trong_loc(cx, day_y, dinh_y, r_dinh, hat, mau=(80, 130, 255), co=1.0):
    rd = random.Random(hat)
    loi, quang = moi(), moi()
    for lech in (-0.3, 0.35):
        a = (cx + r_dinh * lech, dinh_y + 0.02)
        b = (cx + r_dinh * lech * 0.2, dinh_y + (day_y - dinh_y) * rd.uniform(0.55, 0.8))
        ve_cay_tia(cay_tia(a, b, rd, 0.13, 5, 2), loi, quang, int(6 * co), int(20 * co), mau)
    return gop_tia(loi, quang, 0.9)


# ------------------------------------------------------------------ GIO LOC
#
# 28/09/2026 lan hai: nguoi dung che ban "quat ba con loc" van so sai, "khong can giu theo anh cu" -> thiet ke lai: MOT con loc
# NGHIENG VE TRUOC nhu dang lao di (Gio loc bay 9,5 m/s xuyen moi vat), than la DAI GIO BAN RONG co van keo ngang, vet CHEM GIO
# hinh luoi liem quan quanh, vet bui nau den cuon phia sau, BIA MO + DA BI HAT TUNG len khong (hat tung 80%), set nho trong long.

def van_ngang(hat, doc=36, ngang=3):
    """Nhieu keo dai theo chieu ngang - van gio chay."""
    rd = random.Random(hat)
    nho = Image.new("L", (ngang, doc))
    nho.putdata([rd.randint(0, 255) for _ in range(ngang * doc)])
    return nho.resize((W, W), Image.BICUBIC)


def gio_loc():
    rd = random.Random(1)
    nen = moi()
    cx, day, dinh = 0.44, 0.84, 0.16
    nghieng = 0.16                        # dinh lech ve phai: dang lao toi truoc

    def x_at(t):
        return cx + nghieng * t ** 1.3 + math.sin(t * 5.0) * 0.012

    # Than TO NGANG va CHAN LOE (nguoi dung 28/09/2026: "nhin giong cay kem" - duoi chan nhon hoat)
    def r_at(t):
        return 0.070 + 0.150 * t ** 1.15 + 0.085 * math.exp(-t / 0.09)

    H = day - dinh

    # ---- vet bui cuon phia sau (duoi trai) ----
    bui = Image.new("L", (W, W), 0)
    db = ImageDraw.Draw(bui)
    for i in range(34):
        t = i / 33.0
        x = cx - 0.02 - 0.34 * t + rd.uniform(-0.02, 0.02)
        y = day - 0.01 - 0.10 * t * t + rd.uniform(-0.02, 0.02)
        r = (0.06 + 0.07 * t) * rd.uniform(0.7, 1.1)
        db.ellipse([P(x - r, y - r * 0.6), P(x + r, y + r * 0.6)], fill=int(230 * (1 - 0.6 * t)))
    bui = ImageChops.multiply(mo(bui, 12), fbm(81, 10, 4).point(lambda v: min(255, 40 + v)))
    nen = cong(nen, to_mau(bui, (150, 112, 78)))
    nen = cong(nen, bui_chan(x_at(0.0), day, 0.19, 88, (150, 115, 82), 1.1))     # cum bui om chan loe
    da_nho = moi()
    dd = ImageDraw.Draw(da_nho)
    for _ in range(26):
        x, y = rd.uniform(0.10, 0.45), rd.uniform(0.66, 0.88)
        r = rd.uniform(3, 7)
        dd.ellipse([x * W - r, y * W - r, x * W + r, y * W + r], fill=(95, 82, 70))
    nen = cong(nen, da_nho)

    # ---- than loc: dai gio ban rong (nua sau mo truoc, nua truoc sang sau) ----
    def dai_gio(truoc, g0, vong, t0, t1, day_k, sang_k):
        lop = Image.new("L", (W, W), 0)
        d = ImageDraw.Draw(lop)
        n = 60
        tren, duoi = [], []
        for i in range(n + 1):
            t = t0 + (t1 - t0) * i / n
            g = g0 + vong * 2 * math.pi * t
            r = r_at(t)
            x = x_at(t) + r * math.cos(g)
            y = day - H * t + r * 0.20 * math.sin(g)
            rong = (0.006 + 0.03 * r / 0.2) * day_k * math.sin(math.pi * i / n) ** 0.6
            tren.append((x, y - rong))
            duoi.append((x, y + rong))
        # chi giu doan cung phia (truoc / sau)
        doan, cu = [], []
        for i in range(n + 1):
            t = t0 + (t1 - t0) * i / n
            o_truoc = math.sin(g0 + vong * 2 * math.pi * t) > 0
            if o_truoc == truoc:
                cu.append(i)
            elif cu:
                doan.append(cu); cu = []
        if cu:
            doan.append(cu)
        for ds in doan:
            if len(ds) < 2:
                continue
            poly = [P(*tren[i]) for i in ds] + [P(*duoi[i]) for i in reversed(ds)]
            d.polygon(poly, fill=int(255 * sang_k))
        return lop

    dai_ds = []
    for _ in range(30):
        dai_ds.append((rd.uniform(0, 6.28), rd.uniform(1.3, 2.2), rd.uniform(0.0, 0.72), rd.uniform(0.3, 0.7), rd.uniform(0.7, 1.3)))
    sau = Image.new("L", (W, W), 0)
    truoc = Image.new("L", (W, W), 0)
    for (g0, vong, t0, dai, day_k) in dai_ds:
        t1 = min(1.0, t0 + dai)
        sau = ImageChops.add(sau, dai_gio(False, g0, vong, t0, t1, day_k, 0.16))
        truoc = ImageChops.add(truoc, dai_gio(True, g0, vong, t0, t1, day_k, 0.27))
    van = van_ngang(83).point(lambda v: min(255, 20 + int(v * 1.1)))
    sau = ImageChops.multiply(mo(sau, 2.5), van)
    truoc = ImageChops.multiply(mo(truoc, 1.5), van)
    # vien sang o mep tren moi dai (canh dai gio bat sang)
    canh = ImageChops.subtract(truoc, ImageChops.offset(truoc, 0, 5))
    than = cong(to_mau(sau, (130, 132, 136)), to_mau(truoc, (215, 218, 224)), to_mau(canh, (255, 255, 255)))
    # quang loc mo
    quang = Image.new("L", (W, W), 0)
    dq = ImageDraw.Draw(quang)
    for i in range(30):
        t = i / 29
        r = r_at(t) * 1.1
        dq.ellipse([P(x_at(t) - r, day - H * t - r * 0.3), P(x_at(t) + r, day - H * t + r * 0.3)], fill=70)
    nen = cong(nen, to_mau(mo(quang, 18), (90, 92, 96)))
    nen = cong(nen, phat_sang(than, 7, 0.35))

    # set nho trong long loc
    loi, q = moi(), moi()
    ve_cay_tia(cay_tia((x_at(0.95) - 0.04, day - H * 0.95), (x_at(0.35), day - H * 0.35), rd, 0.13, 5, 2), loi, q, 5, 16, (90, 150, 255))
    nen = cong(nen, sang(gop_tia(loi, q, 0.6), 0.5))

    # ---- vet chem gio luoi liem quan quanh ----
    chem = moi()
    dc = ImageDraw.Draw(chem)
    for (t, rx_k, ry_k, bat, het, day_c, mau) in [(0.28, 2.0, 0.42, 0.08, 0.60, 0.034, (235, 238, 240)),
                                                   (0.58, 1.75, 0.36, 0.55, 1.05, 0.030, (215, 218, 222)),
                                                   (0.84, 1.5, 0.30, 0.05, 0.52, 0.024, (195, 198, 202))]:
        cxc, cyc = x_at(t), day - H * t
        R = r_at(t) * rx_k + 0.05
        pts_ngoai, pts_trong = [], []
        n = 50
        for i in range(n + 1):
            u = bat + (het - bat) * i / n
            g = u * 2 * math.pi
            dd_ = day_c * math.sin(math.pi * i / n) ** 0.8
            pts_ngoai.append(P(cxc + math.cos(g) * R, cyc + math.sin(g) * R * ry_k))
            pts_trong.append(P(cxc + math.cos(g) * (R - dd_), cyc + math.sin(g) * (R - dd_) * ry_k - dd_ * 0.6))
        dc.polygon(pts_ngoai + list(reversed(pts_trong)), fill=mau)
    chem = nhan(chem, van_ngang(85).point(lambda v: min(255, 110 + v)))
    nen = cong(nen, phat_sang(mo(chem, 1.2), 9, 0.9))

    # ---- bia mo + da bi hat tung ----
    bay = moi()
    dbay = ImageDraw.Draw(bay)
    for (x, y, s_, goc, la_bia) in [(0.79, 0.40, 0.072, 0.55, True), (0.80, 0.63, 0.036, 2.1, False),
                                     (0.66, 0.17, 0.030, 1.2, False), (0.24, 0.44, 0.028, 0.3, False)]:
        if la_bia:
            pts = [(-0.55, 1.0), (0.55, 1.0), (0.55, -0.35), (0.35, -0.8), (0.0, -0.95), (-0.35, -0.8), (-0.55, -0.35)]
        else:
            pts = [(math.cos(k * 1.05) * rd.uniform(0.6, 1.0), math.sin(k * 1.05) * rd.uniform(0.6, 1.0)) for k in range(6)]
        poly = [P(x + (px * math.cos(goc) - py * math.sin(goc)) * s_, y + (px * math.sin(goc) + py * math.cos(goc)) * s_) for (px, py) in pts]
        dbay.polygon(poly, fill=(128, 124, 118), outline=(225, 220, 210))
        if not la_bia:       # mat tren trai sang hon: da co khoi
            tam_ = P(x, y)
            dbay.polygon([tam_] + poly[2:5], fill=(170, 165, 158))
        if la_bia:   # chu thap khac tren bia + vet nut
            dbay.line([P(x + (0.0 * math.cos(goc) - (-0.55) * math.sin(goc)) * 1, y)], fill=(0, 0, 0))
            for (a, b) in [((0.0, -0.55), (0.0, 0.25)), ((-0.25, -0.25), (0.25, -0.25)), ((-0.3, 0.5), (0.2, 0.85))]:
                pa = P(x + (a[0] * math.cos(goc) - a[1] * math.sin(goc)) * s_, y + (a[0] * math.sin(goc) + a[1] * math.cos(goc)) * s_)
                pb = P(x + (b[0] * math.cos(goc) - b[1] * math.sin(goc)) * s_, y + (b[0] * math.sin(goc) + b[1] * math.cos(goc)) * s_)
                dbay.line([pa, pb], fill=(70, 66, 62), width=5)
        # vet gio keo theo vat bay
        for k in range(3):
            dy = (k - 1) * s_ * 0.5
            dbay.line([P(x - s_ * 1.1, y + dy + s_ * 0.3), P(x - s_ * 2.8, y + dy + s_ * 1.0)], fill=(150, 152, 155), width=3)
    nen = cong(nen, phat_sang(bay, 4, 0.4))

    # hat bui li ti bay quanh
    li = moi()
    dl = ImageDraw.Draw(li)
    for _ in range(50):
        t = rd.uniform(0.05, 0.95)
        g = rd.uniform(0, 6.28)
        r = r_at(t) * rd.uniform(1.1, 1.6)
        x = x_at(t) + math.cos(g) * r
        y = day - H * t + math.sin(g) * r * 0.25
        rr = rd.uniform(2, 4)
        dl.ellipse([x * W - rr, y * W - rr, x * W + rr, y * W + rr], fill=(170, 160, 145))
    nen = cong(nen, li)
    return nen


# ------------------------------------------------------------------ LOC XOAY

def loc_xoay():
    nen = moi()
    rd = random.Random(2)
    cx, day, dinh, r_day, r_dinh = 0.50, 0.86, 0.12, 0.10, 0.335
    nen = cong(nen, bui_chan(cx, day, 0.27, 21, (100, 92, 84), 1.2))
    lop, mn = pheu_loc(cx, day, dinh, r_day, r_dinh, 22, 80, (215, 222, 228), 0.03, 1.3, 1.0, mu=1.25, loe=0.13, loe_cao=0.08)
    nen = cong(nen, lop)
    # vanh may cuon o mieng tren
    vanh = Image.new("L", (W, W), 0)
    dv = ImageDraw.Draw(vanh)
    for _ in range(26):
        g = rd.uniform(0, 2 * math.pi)
        x = cx + math.cos(g) * r_dinh * 1.02
        y = dinh + math.sin(g) * r_dinh * 0.18
        r = rd.uniform(0.035, 0.07)
        dv.ellipse([P(x - r, y - r * 0.55), P(x + r, y + r * 0.55)], fill=rd.randint(120, 220))
    vanh = ImageChops.multiply(mo(vanh, 8), fbm(23, 12, 3).point(lambda v: min(255, 60 + v)))
    nen = cong(nen, to_mau(vanh, (175, 182, 190)))
    nen = cong(nen, set_trong_loc(cx, day, dinh, r_dinh, 24, (90, 140, 255), 1.0))
    # manh bia mo, da bi cuon bay quanh pheu
    manh = moi()
    dm = ImageDraw.Draw(manh)
    for i in range(9):
        t = rd.uniform(0.15, 0.85)
        g = rd.uniform(0, 2 * math.pi)
        r = (r_day + (r_dinh - r_day) * t ** 1.25 + 0.13 * math.exp(-t / 0.08)) * 1.2
        x = cx + math.cos(g) * r
        y = day - (day - dinh) * t + math.sin(g) * r * 0.16
        s = rd.uniform(0.012, 0.024)
        goc = rd.uniform(0, math.pi)
        if i % 3 == 0:        # bia mo: hinh chu nhat dau tron
            pts = [(-0.6, 1), (0.6, 1), (0.6, -0.4), (0.3, -0.9), (-0.3, -0.9), (-0.6, -0.4)]
        else:                 # da vun
            pts = [(math.cos(k * 1.3 + i) * rd.uniform(0.6, 1), math.sin(k * 1.3 + i) * rd.uniform(0.6, 1)) for k in range(5)]
        dm.polygon([P(x + (px * math.cos(goc) - py * math.sin(goc)) * s, y + (px * math.sin(goc) + py * math.cos(goc)) * s)
                    for (px, py) in pts], fill=(120, 118, 112), outline=(190, 188, 180))
    nen = cong(nen, manh)
    return nen


# ------------------------------------------------------------------ MAY GIONG

def may_giong():
    rd = random.Random(3)
    nen = moi()
    # mua xoi (nghieng nhe)
    mua = moi()
    dm = ImageDraw.Draw(mua)
    for _ in range(120):
        x = rd.uniform(0.14, 0.86)
        y = rd.uniform(0.30, 0.82)
        dai = rd.uniform(0.04, 0.09)
        dm.line([P(x, y), P(x - dai * 0.18, y + dai)], fill=lerp((0, 0, 0), (120, 150, 190), rd.uniform(0.4, 1.0)), width=2)
    nen = cong(nen, mo(mua, 0.8))
    # nuoc ban duoi dat
    ban = moi()
    db = ImageDraw.Draw(ban)
    for _ in range(14):
        x, y = rd.uniform(0.2, 0.8), rd.uniform(0.80, 0.87)
        r = rd.uniform(0.012, 0.028)
        db.ellipse([P(x - r, y - r * 0.3), P(x + r, y + r * 0.3)], outline=(110, 150, 200), width=2)
    nen = cong(nen, phat_sang(ban, 4, 0.8))
    # tia set
    loi, quang = moi(), moi()
    ve_cay_tia(cay_tia((0.40, 0.30), (0.36, 0.84), rd, 0.11, 6, 3), loi, quang, 8, 26, (90, 150, 255))
    ve_cay_tia(cay_tia((0.66, 0.31), (0.70, 0.70), rd, 0.12, 5, 2), loi, quang, 5, 18, (80, 130, 240))
    nen = cong(nen, gop_tia(loi, quang, 1.0))
    ch = moi()
    ImageDraw.Draw(ch).ellipse([P(0.29, 0.815), P(0.43, 0.865)], fill=(230, 240, 255))
    nen = cong(nen, phat_sang(mo(ch, 3), 14, 1.3))
    # may XAM DEN, roi sang tung mang (cho tia phat ra)
    vung = [(0.50, 0.16, 0.34, 0.10), (0.24, 0.23, 0.18, 0.08), (0.76, 0.22, 0.18, 0.08), (0.50, 0.28, 0.28, 0.07)]
    mn, lop_may = may(vung, 61, (46, 50, 58), (150, 170, 210), rd)
    mang = ImageChops.multiply(mn, cong(ban_kinh_tu_tam(0.40, 0.27, 0.0, 0.11, 1.0, 0.0), ban_kinh_tu_tam(0.66, 0.27, 0.0, 0.08, 0.8, 0.0)))
    lop_may = cong(lop_may, to_mau(mang, (170, 195, 255)))
    nen = Image.composite(lop_may, nen, mn.point(lambda v: min(255, int(v * 1.4))))
    return nen


# ------------------------------------------------------------------ TOC BIEN

def bong_phu_thuy(cx, day_y, co):
    """Mat na bong phu thuy trum mu (cung dang voi icon Tang hinh), dung o (cx, day_y), cao co."""
    khung = [(0.50, 0.105), (0.565, 0.145), (0.605, 0.22), (0.62, 0.30), (0.655, 0.36), (0.72, 0.405),
             (0.755, 0.47), (0.765, 0.58), (0.79, 0.70), (0.83, 0.82),
             (0.72, 0.84), (0.62, 0.815), (0.50, 0.85), (0.38, 0.815), (0.28, 0.84), (0.17, 0.82),
             (0.21, 0.70), (0.235, 0.58), (0.245, 0.47), (0.28, 0.405), (0.345, 0.36), (0.38, 0.30),
             (0.395, 0.22), (0.435, 0.145)]
    khung = chaikin(khung, 3)
    k = co / 0.745
    pts = [(cx + (x - 0.5) * k, day_y + (y - 0.85) * k) for (x, y) in khung]
    mn = Image.new("L", (W, W), 0)
    ImageDraw.Draw(mn).polygon([P(*p) for p in pts], fill=255)
    return mn


def vien_sang(mn, r=24, k=3.0):
    return ImageChops.multiply(mn, ImageChops.invert(mo(mn, r))).point(lambda v: min(255, int(v * k)))


def toc_bien():
    rd = random.Random(4)
    nen = moi()
    tim = (170, 110, 255)
    di, den = (0.27, 0.80), (0.71, 0.70)
    # vet khong gian noi hai cho: dai sang xoan
    vet = moi()
    dv = ImageDraw.Draw(vet)
    for j in range(7):
        pha = rd.uniform(0, 6.28)
        cu = None
        for i in range(41):
            t = i / 40
            x = di[0] + (den[0] - di[0]) * t
            y = di[1] - 0.20 + (den[1] - di[1] - 0.0) * t + math.sin(t * 9 + pha) * 0.025 * math.sin(t * math.pi)
            if cu:
                dv.line([P(*cu), P(x, y)], fill=lerp((0, 0, 0), (150, 100, 255), 0.4 + 0.6 * t), width=3 + int(4 * t))
            cu = (x, y)
    nen = cong(nen, phat_sang(mo(vet, 2), 12, 1.0))
    # cho cu: bong tan thanh hat, hai bong mo phia sau
    for k_mo, dx, nguong in [(0.40, -0.0, 120), (0.70, 0.15, 60)]:
        mn = bong_phu_thuy(di[0] + dx, di[1] - 0.02 * (dx > 0), 0.40)
        tan = ImageChops.multiply(mn, fbm(41 + int(dx * 100), 14, 3).point(lambda v, n=nguong: 0 if v < n else min(255, (v - n) * 3)))
        nen = cong(nen, to_mau(vien_sang(tan, 16, 2.6), tuple(int(c * k_mo) for c in tim)))
    hat = moi()
    dh = ImageDraw.Draw(hat)
    for _ in range(70):
        x = di[0] + rd.uniform(-0.12, 0.2)
        y = di[1] - rd.uniform(0.0, 0.40)
        r = rd.uniform(2, 6)
        dh.rectangle([x * W - r, y * W - r, x * W + r, y * W + r], fill=lerp((60, 30, 120), (200, 160, 255), rd.random()))
    nen = cong(nen, phat_sang(hat, 5, 0.9))
    # vong phep o cho moi
    vong = moi()
    dvg = ImageDraw.Draw(vong)
    cx, cy, rx, ry = den[0], den[1] + 0.02, 0.19, 0.05
    for (kk, w) in [(1.0, 5), (0.78, 3)]:
        dvg.ellipse([P(cx - rx * kk, cy - ry * kk), P(cx + rx * kk, cy + ry * kk)], outline=(190, 140, 255), width=w)
    for i in range(18):
        g = 2 * math.pi * i / 18
        dvg.line([P(cx + math.cos(g) * rx * 0.86, cy + math.sin(g) * ry * 0.86), P(cx + math.cos(g) * rx * 0.94, cy + math.sin(g) * ry * 0.94)],
                 fill=(220, 190, 255), width=3)
    nen = cong(nen, phat_sang(vong, 12, 1.4))
    # cot sang bung len
    cot = ImageChops.multiply(ban_kinh_tu_tam(cx, cy - 0.16, 0.0, 0.2, 1.0, 0.0), doc_theo_do_cao(cy, cy - 0.45))
    nen = cong(nen, to_mau(cot, (70, 40, 130)))
    # bong o cho moi: ro, vien sang tim trang
    mn = bong_phu_thuy(den[0], den[1], 0.46)
    nen = cong(nen, to_mau(vien_sang(mn, 24, 3.2), tim), to_mau(vien_sang(mn, 6, 4.0), (235, 215, 255)))
    nen = cong(nen, to_mau(ImageChops.multiply(mn, fbm(44, 10, 3)).point(lambda v: v // 5), (80, 50, 140)))
    # khoang mat toi han trong mu (nhu icon Tang hinh)
    k = 0.46 / 0.745
    lo = Image.new("L", (W, W), 0)
    ImageDraw.Draw(lo).ellipse([P(den[0] - 0.065 * k, den[1] + (0.205 - 0.85) * k), P(den[0] + 0.065 * k, den[1] + (0.375 - 0.85) * k)], fill=255)
    nen = ImageChops.multiply(nen, ImageChops.invert(mo(lo, 5)).convert("RGB"))
    vm = Image.new("L", (W, W), 0)
    ImageDraw.Draw(vm).ellipse([P(den[0] - 0.065 * k, den[1] + (0.205 - 0.85) * k), P(den[0] + 0.065 * k, den[1] + (0.375 - 0.85) * k)], outline=255, width=4)
    nen = cong(nen, to_mau(mo(vm, 2), (190, 150, 255)))
    mat = moi()
    dmt = ImageDraw.Draw(mat)
    k = 0.46 / 0.745
    for ex in (-0.028, 0.028):
        x = den[0] + ex * k
        y = den[1] + (0.293 - 0.85) * k
        dmt.polygon([P(x - 0.02 * k, y), P(x, y - 0.008 * k), P(x + 0.02 * k, y), P(x, y + 0.007 * k)], fill=(240, 220, 255))
    nen = cong(nen, phat_sang(mat, 5, 2.0))
    return nen


# ------------------------------------------------------------------ HOA LOC XOAY

def hoa_loc_xoay():
    rd = random.Random(5)
    nen = moi()
    cx, day = 0.50, 0.86
    # pheu lon trong suot (hinh sap thanh)
    lon, _ = pheu_loc(cx, day, 0.13, 0.04, 0.33, 51, 44, (150, 165, 175), 0.03, 0.9, 0.42)
    nen = cong(nen, lon)
    # vong song lan rong (phinh ra)
    vong = moi()
    dv = ImageDraw.Draw(vong)
    for (r, y, c) in [(0.16, 0.56, (190, 170, 130)), (0.24, 0.48, (150, 135, 105)), (0.32, 0.40, (110, 100, 80))]:
        dv.ellipse([P(cx - r, y - r * 0.2), P(cx + r, y + r * 0.2)], outline=c, width=4)
    nen = cong(nen, phat_sang(vong, 9, 1.0))
    # mui ten phinh ra hai ben
    mt = moi()
    dm = ImageDraw.Draw(mt)
    for s in (-1, 1):
        x0 = cx + s * 0.13
        x1 = cx + s * 0.29
        y0, y1 = 0.62, 0.50
        dm.line([P(x0, y0), P(x1, y1)], fill=(230, 200, 140), width=6)
        g = math.atan2(y1 - y0, x1 - x0)
        for dg in (2.6, -2.6):
            dm.line([P(x1, y1), P(x1 + math.cos(g + dg) * 0.04, y1 + math.sin(g + dg) * 0.04)], fill=(230, 200, 140), width=6)
    nen = cong(nen, phat_sang(mt, 8, 1.1))
    # loc nho dac o giua
    nen = cong(nen, bui_chan(cx, day, 0.10, 52, (110, 90, 66), 1.0))
    nen = cong(nen, sang(set_trong_loc(cx, 0.86, 0.13, 0.33, 54, (100, 150, 255), 0.6), 0.35))
    nho, _ = pheu_loc(cx, day, 0.50, 0.022, 0.11, 53, 40, (250, 228, 185), 0.02, 1.1, 1.5)
    nen = cong(nen, nho)
    return nen


if __name__ == "__main__":
    ds = [("GioLoc", gio_loc, (0.20, 0.11, 0.04)), ("Loc", loc_xoay, (0.03, 0.17, 0.18)), ("MayGiong", may_giong, (0.05, 0.08, 0.16)),
          ("TocBien", toc_bien, (0.09, 0.04, 0.15)), ("HoaLocXoay", hoa_loc_xoay, (0.10, 0.09, 0.07))]
    # 29/09/2026: GioLoc + TocBien da VE LAI LAN HAI bang sinh_icon_ve_lai_5.py - bo qua tru khi co --cu (khong thi de mat icon moi)
    if "--cu" not in sys.argv:
        ds = [d for d in ds if d[0] not in ("GioLoc", "TocBien")]
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
        tam.save(os.path.join(thu_muc, "icon_moi_3.png"))
        print("xem truoc:", os.path.join(thu_muc, "icon_moi_3.png"))
