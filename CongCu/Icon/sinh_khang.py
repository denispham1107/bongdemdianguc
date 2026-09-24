# -*- coding: utf-8 -*-
# Icon BON KY NANG BI DONG "KHANG ..." (19/09/2026).
#
# Nguoi dung: "Su dung script ve icon skill, khong can su dung blender mcp".
#
# Moi icon la mot TAM KHIEN mang dau hieu cua he: ngon lua, tinh the bang, tia set, xoay gio.
# Mau khien lay theo mau nhom trong Sach phep (SachPhep.MauNhom) de nhin la biet thuoc he nao.
# Anh ra 256x256 RGB nen den - dung khuon voi cac icon khac trong Assets/Resources/Icons.
#
# Chay: python CongCu/Icon/sinh_khang.py
import os
import math
from PIL import Image, ImageDraw, ImageFilter

GOC = os.path.join(os.path.dirname(__file__), "..", "..")
RA = os.path.join(GOC, "Assets", "Resources", "Icons")
S = 256


def khuon_khien(co=1.0):
    """Duong vien tam khien (heater shield): vai thang o tren, thon nhon xuong duoi."""
    cx, cy = S * 0.5, S * 0.52
    w, h = S * 0.40 * co, S * 0.44 * co
    diem = []
    # canh tren
    diem.append((cx - w, cy - h))
    diem.append((cx + w, cy - h))
    # suon phai phinh ra roi thon vao mui duoi
    for i in range(1, 21):
        t = i / 20.0
        x = cx + w * (1.0 - t * t * 0.98)
        y = cy - h + (2.0 * h) * (0.35 + 0.65 * t)
        diem.append((x, y))
    diem.append((cx, cy + h))
    for i in range(20, 0, -1):
        t = i / 20.0
        x = cx - w * (1.0 - t * t * 0.98)
        y = cy - h + (2.0 * h) * (0.35 + 0.65 * t)
        diem.append((x, y))
    return diem


def to_khien(mau_toi, mau_sang):
    """Tam khien: long khien chuyen mau tu tren xuong, co vien sang."""
    nen = Image.new("RGB", (S, S), (0, 0, 0))
    d = ImageDraw.Draw(nen)

    # long khien - ve tung dai ngang de co chuyen mau
    khuon = khuon_khien()
    mat_na = Image.new("L", (S, S), 0)
    ImageDraw.Draw(mat_na).polygon(khuon, fill=255)

    doc = Image.new("RGB", (S, S), (0, 0, 0))
    dd = ImageDraw.Draw(doc)
    for y in range(S):
        t = y / float(S)
        c = tuple(int(mau_toi[k] * (1.0 - t) + mau_sang[k] * t * 0.55) for k in range(3))
        dd.line([(0, y), (S, y)], fill=c)
    nen.paste(doc, (0, 0), mat_na)

    # vien sang
    d.line(khuon + [khuon[0]], fill=mau_sang, width=5, joint="curve")

    # anh sang hat ra sau khien
    hao = Image.new("RGB", (S, S), (0, 0, 0))
    ImageDraw.Draw(hao).polygon(khuon_khien(1.06), fill=tuple(int(c * 0.55) for c in mau_sang))
    hao = hao.filter(ImageFilter.GaussianBlur(14))
    from PIL import ImageChops
    return ImageChops.lighter(nen, hao)


def them_dau_hieu(nen, ve, mau):
    """Ve dau hieu cua he len giua khien, co quang sang quanh net."""
    from PIL import ImageChops
    lop = Image.new("RGB", (S, S), (0, 0, 0))
    ve(ImageDraw.Draw(lop), mau)
    quang = lop.filter(ImageFilter.GaussianBlur(9))
    ra = ImageChops.lighter(nen, quang)
    return ImageChops.lighter(ra, lop)


# ---------------- dau hieu tung he ----------------

def _ngon_lua(cx, day, cao, rong, lech):
    """Duong vien mot ngon lua: dinh nhon o tren, phinh o duoi, day tron."""
    n = 60
    trai, phai = [], []
    for i in range(n + 1):
        t = i / float(n)                       # 0 o dinh -> 1 o day
        # be ngang: 0 o dinh, phinh to dan xuong duoi roi tron lai o day
        r = rong * (math.sin(math.pi * min(1.0, t * 0.94)) ** 0.62)
        r = r * (0.45 + 0.75 * t) + rong * 0.34 * t * t
        # dinh ngon lua liem cong sang mot ben
        dx = lech * (1.0 - t) ** 2
        y = day - cao * (1.0 - t)
        trai.append((cx + dx - r, y))
        phai.append((cx + dx + r, y))
    return trai + phai[::-1]


def dau_lua(d, mau):
    """Ngon lua: mot luoi lon liem cong + mot luoi nho ben canh + loi sang o giua."""
    cx, cy = S * 0.5, S * 0.68
    # luoi nho ben phai, thap hon
    d.polygon(_ngon_lua(cx + S * 0.075, cy, S * 0.20, S * 0.055, -S * 0.020), fill=mau)
    # ngon chinh
    d.polygon(_ngon_lua(cx, cy, S * 0.33, S * 0.105, S * 0.045), fill=mau)
    # loi sang ben trong
    d.polygon(_ngon_lua(cx + S * 0.008, cy - S * 0.012, S * 0.185, S * 0.050, S * 0.022),
              fill=(255, 255, 255))


def dau_bang(d, mau):
    """Tinh the bang: sau nhanh, moi nhanh co hai gai nho."""
    cx, cy = S * 0.5, S * 0.53
    r = S * 0.17
    for k in range(6):
        g = math.pi * k / 3.0
        x, y = cx + math.cos(g) * r, cy + math.sin(g) * r
        d.line([(cx, cy), (x, y)], fill=mau, width=7)
        for chieu in (-1, 1):
            g2 = g + chieu * math.pi / 5.0
            gx = cx + math.cos(g) * r * 0.60
            gy = cy + math.sin(g) * r * 0.60
            d.line([(gx, gy), (gx + math.cos(g2) * r * 0.36, gy + math.sin(g2) * r * 0.36)],
                   fill=mau, width=5)
    d.ellipse([cx - 9, cy - 9, cx + 9, cy + 9], fill=(255, 255, 255))


def dau_set(d, mau):
    """Tia set gay khuc."""
    cx, cy = S * 0.5, S * 0.52
    than = [(cx + S * 0.055, cy - S * 0.20), (cx - S * 0.035, cy - S * 0.01),
            (cx + S * 0.020, cy - S * 0.01), (cx - S * 0.060, cy + S * 0.20),
            (cx + S * 0.075, cy - S * 0.03), (cx + S * 0.015, cy - S * 0.03),
            (cx + S * 0.105, cy - S * 0.20)]
    d.polygon(than, fill=mau)
    d.line(than + [than[0]], fill=(255, 255, 255), width=3, joint="curve")


def dau_phong(d, mau):
    """Xoay gio: ba vong xoay oc."""
    cx, cy = S * 0.5, S * 0.53
    for lop, (r0, r1, day) in enumerate([(S * 0.055, S * 0.175, 8), (S * 0.035, S * 0.125, 6)]):
        diem = []
        for i in range(61):
            t = i / 60.0
            g = 2.6 * math.pi * t + lop * math.pi
            r = r0 + (r1 - r0) * t
            diem.append((cx + math.cos(g) * r, cy + math.sin(g) * r * 0.78))
        d.line(diem, fill=mau, width=day, joint="curve")
    # hai vet gio thoi ngang
    for dy in (-S * 0.115, S * 0.115):
        d.line([(cx - S * 0.19, cy + dy), (cx + S * 0.10, cy + dy)], fill=mau, width=6)


def dau_toc_do(d, mau):
    """Toc do di chuyen (25/09/2026): hai mui ten gay (>>) lao sang phai + ba vet gio phia sau."""
    cx, cy = S * 0.53, S * 0.52
    for k, dx in enumerate((-S * 0.035, S * 0.085)):
        a = S * 0.105
        d.line([(cx + dx - a * 0.75, cy - a), (cx + dx + a * 0.25, cy), (cx + dx - a * 0.75, cy + a)],
               fill=mau, width=15 if k == 1 else 12, joint="curve")
    for dy, dai in ((-S * 0.085, 0.13), (0.0, 0.17), (S * 0.085, 0.13)):
        x1 = cx - S * 0.14
        d.line([(x1 - S * dai, cy + dy), (x1, cy + dy)], fill=mau, width=6)


BO = [
    ("KhangLua",   (0.42, 0.14, 0.05), (1.00, 0.55, 0.20), dau_lua,   (255, 236, 205)),
    ("KhangBang",  (0.07, 0.22, 0.38), (0.55, 0.85, 1.00), dau_bang,  (226, 246, 255)),
    ("KhangSet",   (0.20, 0.13, 0.42), (0.70, 0.60, 1.00), dau_set,   (240, 234, 255)),
    ("KhangPhong", (0.22, 0.24, 0.22), (0.80, 0.82, 0.78), dau_phong, (246, 248, 244)),
    # Toc do di chuyen (25/09/2026): mau nhom BI DONG trong Sach phep (0,72 0,90 0,66)
    ("TocDo",      (0.14, 0.26, 0.10), (0.72, 0.90, 0.66), dau_toc_do, (240, 255, 232)),
]

for ten, toi, sang, ve, mau_hieu in BO:
    mau_toi = tuple(int(c * 255) for c in toi)
    mau_sang = tuple(int(c * 255) for c in sang)
    anh = to_khien(mau_toi, mau_sang)
    anh = them_dau_hieu(anh, ve, mau_hieu)
    duong = os.path.join(RA, ten + ".png")
    anh.save(duong)
    print("ok", duong)
