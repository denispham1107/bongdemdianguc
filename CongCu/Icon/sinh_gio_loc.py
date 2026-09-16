# -*- coding: utf-8 -*-
# Icon ky nang GIO LOC (16/09/2026): ba con loc nho tu icon Loc xoay (Icons/Loc.png), nhuom nau.
# Nguoi dung chon: dung hinh Loc xoay doi mau nau, khong dung Blender.
# Chay: python CongCu/Icon/sinh_gio_loc.py
import os
from PIL import Image, ImageChops

GOC = os.path.join(os.path.dirname(__file__), "..", "..")
nguon = Image.open(os.path.join(GOC, "Assets/Resources/Icons/Loc.png")).convert("RGB")
S = nguon.size[0]

def nau(im):
    # Do sang -> nau dat; phan sang nhat nga vang kem cho khong bet mau
    L = im.convert("L")
    r = L.point(lambda v: min(255, int(v * 0.90)))
    g = L.point(lambda v: min(255, int(v * 0.64)))
    b = L.point(lambda v: min(255, int(v * 0.42)))
    return Image.merge("RGB", (r, g, b))

loc = nau(nguon)
ra = Image.new("RGB", (S, S), (0, 0, 0))
# (ti le, tam x, day y) - con giua to va o truoc, hai con hai ben nho hon va lui len
for k, cx, day in [(0.50, 0.24, 0.80), (0.50, 0.76, 0.80), (0.62, 0.50, 0.98)]:
    w = int(S * k)
    nho = loc.resize((w, w), Image.LANCZOS)
    lop = Image.new("RGB", (S, S), (0, 0, 0))
    lop.paste(nho, (int(cx * S - w / 2), int(day * S - w)))
    ra = ImageChops.lighter(ra, lop)

ra.save(os.path.join(GOC, "Assets/Resources/Icons/GioLoc.png"))
print("ok", ra.size)
