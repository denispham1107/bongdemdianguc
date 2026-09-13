# -*- coding: utf-8 -*-
"""
VE CON SO DEM NGUOC VAO TRAN (0..9) VA VONG PHU CHU PHIA SAU.

Nguoi dung 13/09/2026 (anh chup man dem nguoc): "con so dem nguoc dang thiet ke qua
don gian, thiet ke lai sao cho phu hop voi phong cach rung ron, dang so cua game".

CUNG MOT NGON NGU HINH voi anh ten game "AC QUY TRO LAI" (CongCu/TieuDe/sinh_tieu_de.py):
font Grenze Gotisch Black, long chu do mau chuyen doc co van chay doc va vet nut, mau
NHO GIOT tu chan net, canh tren sang nhu luoi dao, vien den, quang do, bong do.

Moi chu so mot anh, CUNG KICH THUOC khung va CUNG DUONG CHAN, de ghep "10" tu "1" va
"0" luc chay ma hai so van thang hang. Them vong phu chu (hai vong tron, chu run, ngoi
sao nam canh nguoc) de xoay cham phia sau con so.

Chay: python CongCu/DemNguoc/sinh_so_dem_nguoc.py   (Pillow, khong can numpy)
Ra:   Assets/Resources/GiaoDien/DemNguoc/So0.png .. So9.png, VongPhuChu.png
"""
import os, math, random
from PIL import Image, ImageDraw, ImageFont, ImageFilter, ImageChops

GOC = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", ".."))
FONT = os.path.join(GOC, "CongCu", "Fonts", "GrenzeGotisch[wght].ttf")
RA = os.path.join(GOC, "Assets", "Resources", "GiaoDien", "DemNguoc")

RONG, CAO = 300, 420            # anh cuoi moi chu so
K = 3                           # ve gap ba roi thu nho cho mep muot
W, H = RONG * K, CAO * K


def mau(c):
    return tuple(int(round(v * 255)) for v in c)


def to_mau(mask, c, a=1.0):
    lop = Image.new("RGBA", mask.size, mau(c) + (0,))
    al = mask if a >= 1.0 else mask.point(lambda v: int(v * a))
    lop.putalpha(al)
    return lop


def chuyen_doc(size, tren, duoi, y0, y1):
    w, h = size
    cot = Image.new("RGB", (1, h))
    for y in range(h):
        k = min(1.0, max(0.0, (y - y0) / float(max(1, y1 - y0))))
        k = k ** 0.8
        cot.putpixel((0, y), mau(tuple(tren[i] * (1 - k) + duoi[i] * k for i in range(3))))
    return cot.resize((w, h))


# Co chu: chu so cao nhat ("8") chiem ~62% chieu cao khung, chan chu o 72%
font = ImageFont.truetype(FONT, 100 * K)
font.set_variation_by_axes([900])
bb8 = font.getbbox("8")
co = int(100 * K * (H * 0.60) / (bb8[3] - bb8[1]))
font = ImageFont.truetype(FONT, co)
font.set_variation_by_axes([900])
bb8 = font.getbbox("8")
CHAN = int(H * 0.70)                               # duong chan chung cua moi chu so
DINH = CHAN - (bb8[3] - bb8[1])


def ve_so(chu, seed):
    rnd = random.Random(seed)
    bb = font.getbbox(chu)
    tw = bb[2] - bb[0]
    x0 = (W - tw) // 2 - bb[0]
    y0 = CHAN - bb[3]

    mat = Image.new("L", (W, H), 0)
    ImageDraw.Draw(mat).text((x0, y0), chu, font=font, fill=255)
    hop = mat.getbbox()
    px = mat.load()

    # ---- Giot mau: tu diem THAP NHAT cua net o moi cot (chu so co day cong, khong co chan phang) ----
    giot = Image.new("L", (W, H), 0)
    dg = ImageDraw.Draw(giot)
    ung_vien = []
    for x in range(hop[0] + 5 * K, hop[2] - 5 * K, 2 * K):
        y_thap = -1
        for y in range(hop[3] - 1, hop[1] + (hop[3] - hop[1]) // 2, -1):
            if px[x, y] > 200:
                y_thap = y
                break
        if y_thap < 0:
            continue
        # net phai day that tai day (khong phai mep xien mong)
        if px[x, y_thap - 10 * K] > 200:
            ung_vien.append((x, y_thap))
    rnd.shuffle(ung_vien)
    dat = []
    for x, y in ung_vien:
        if len(dat) >= 4:
            break
        if any(abs(x - d[0]) < 26 * K for d in dat):
            continue
        dat.append((x, y))
    dat.sort()
    for chiSo, (x, yday) in enumerate(dat):
        r = rnd.uniform(3.5, 6.5) * K
        dai = (rnd.uniform(60, 95) if chiSo % 2 == 0 else rnd.uniform(18, 42)) * K
        ytop = yday - 5 * K
        n = int(dai)
        for i in range(0, n, K):
            k = i / float(n)
            ri = r * (1.0 - 0.62 * k ** 0.7)
            dg.rectangle([x - ri, ytop + i, x + ri, ytop + i + K], fill=255)
        rd = r * 0.95
        dg.ellipse([x - rd, ytop + n - rd * 0.6, x + rd, ytop + n + rd * 1.6], fill=255)
        dg.ellipse([x - r * 2.0, ytop - r * 1.2, x + r * 2.0, ytop + r * 1.6], fill=255)
    giot = giot.filter(ImageFilter.GaussianBlur(0.8 * K)).point(lambda v: 255 if v > 110 else int(v * 2.2))
    mat_du = ImageChops.lighter(mat, giot)

    # ---- Long chu: do mau chuyen doc + van chay doc + vet nut ----
    long_chu = chuyen_doc((W, H), (1.00, 0.34, 0.18), (0.62, 0.04, 0.03), hop[1], hop[3])
    long_giot = chuyen_doc((W, H), (0.78, 0.07, 0.04), (0.48, 0.02, 0.015), hop[3] - 20 * K, hop[3] + 100 * K)
    long_chu = Image.composite(long_giot, long_chu, ImageChops.subtract(giot, mat))

    hat = Image.effect_noise((W // (4 * K), H // (24 * K)), 50).resize((W, H), Image.BICUBIC)
    dom = Image.effect_noise((W // (30 * K), H // (30 * K)), 60).resize((W, H), Image.BICUBIC)
    hat = ImageChops.multiply(hat.point(lambda v: int(min(255, 226 + (v - 128) * 0.38))),
                              dom.point(lambda v: int(min(255, 234 + (v - 128) * 0.28))))
    long_chu = ImageChops.multiply(long_chu, Image.merge("RGB", (hat, hat, hat)))

    nut = Image.new("L", (W, H), 255)
    dn = ImageDraw.Draw(nut)
    for _ in range(26):
        x = rnd.uniform(hop[0], hop[2]); y = rnd.uniform(hop[1], hop[3])
        goc = rnd.uniform(0, 6.283)
        for _ in range(rnd.randint(4, 9)):
            goc += rnd.uniform(-0.9, 0.9)
            dx = math.cos(goc) * rnd.uniform(6, 16) * K; dy = math.sin(goc) * rnd.uniform(6, 16) * K
            dn.line([x, y, x + dx, y + dy], fill=rnd.randint(95, 150), width=max(1, int(1.3 * K)))
            x += dx; y += dy
    nut = nut.filter(ImageFilter.GaussianBlur(0.5 * K))
    long_chu = ImageChops.multiply(long_chu, Image.merge("RGB", (nut, nut, nut)))

    # ---- Canh sang tren, bong trong duoi ----
    lech = mat_du.transform(mat_du.size, Image.AFFINE, (1, 0, 0, 0, 1, 3 * K))
    canh_tren = ImageChops.subtract(mat_du, lech).filter(ImageFilter.GaussianBlur(0.9 * K))
    lech2 = mat_du.transform(mat_du.size, Image.AFFINE, (1, 0, 0, 0, 1, -6 * K))
    bong_trong = ImageChops.subtract(mat_du, lech2).filter(ImageFilter.GaussianBlur(2 * K))

    # ---- Vien, quang, bong do ----
    vien = mat_du.filter(ImageFilter.MaxFilter(2 * (5 * K) + 1)).filter(ImageFilter.GaussianBlur(0.8 * K))
    quang = mat_du.filter(ImageFilter.MaxFilter(2 * (7 * K) + 1)).filter(ImageFilter.GaussianBlur(20 * K))
    bong = vien.transform(vien.size, Image.AFFINE, (1, 0, -8 * K, 0, 1, -12 * K)).filter(ImageFilter.GaussianBlur(9 * K))

    anh = Image.new("RGBA", (W, H), (0, 0, 0, 0))
    anh = Image.alpha_composite(anh, to_mau(bong, (0, 0, 0), 0.90))
    anh = Image.alpha_composite(anh, to_mau(quang, (1.0, 0.14, 0.05), 0.85))
    anh = Image.alpha_composite(anh, to_mau(vien, (0.06, 0.004, 0.004), 1.0))
    lop_long = long_chu.convert("RGBA"); lop_long.putalpha(mat_du)
    anh = Image.alpha_composite(anh, lop_long)
    anh = Image.alpha_composite(anh, to_mau(ImageChops.multiply(bong_trong, mat_du), (0.10, 0.0, 0.0), 0.45))
    anh = Image.alpha_composite(anh, to_mau(ImageChops.multiply(canh_tren, mat_du), (1.0, 0.76, 0.55), 0.95))
    return anh.resize((RONG, CAO), Image.LANCZOS)


def ve_vong(co=512):
    k = 2
    S = co * k
    c = S / 2.0
    rnd = random.Random(666)
    m = Image.new("L", (S, S), 0)
    d = ImageDraw.Draw(m)
    for r, day in ((0.47, 9), (0.40, 5), (0.25, 4)):
        R = S * r
        d.ellipse([c - R, c - R, c + R, c + R], outline=255, width=day * k)
    # chu run giua hai vong ngoai
    rg = S * 0.435
    for i in range(30):
        g = i / 30.0 * 6.2832
        tx, ty = c + math.cos(g) * rg, c + math.sin(g) * rg
        ox, oy = math.cos(g), math.sin(g)
        vx, vy = -oy, ox
        for _ in range(rnd.randint(2, 3)):
            a = (rnd.uniform(-9, 9) * k, rnd.uniform(-11, 11) * k)
            b = (rnd.uniform(-9, 9) * k, rnd.uniform(-11, 11) * k)
            d.line([tx + vx * a[0] + ox * a[1], ty + vy * a[0] + oy * a[1],
                    tx + vx * b[0] + ox * b[1], ty + vy * b[0] + oy * b[1]], fill=255, width=4 * k)
    # ngoi sao nam canh nguoc
    R = S * 0.40
    diem = [(c + math.cos(math.radians(90 + i * 72)) * R, c + math.sin(math.radians(90 + i * 72)) * R) for i in range(5)]
    for i in range(5):
        d.line([diem[i], diem[(i + 2) % 5]], fill=255, width=5 * k)
    m = m.filter(ImageFilter.GaussianBlur(0.8 * k))

    anh = Image.new("RGBA", (S, S), (0, 0, 0, 0))
    anh = Image.alpha_composite(anh, to_mau(m.filter(ImageFilter.GaussianBlur(14 * k)), (0.9, 0.08, 0.03), 0.9))
    anh = Image.alpha_composite(anh, to_mau(m, (0.62, 0.05, 0.03), 1.0))
    anh = Image.alpha_composite(anh, to_mau(m.filter(ImageFilter.GaussianBlur(1.2 * k)), (1.0, 0.35, 0.15), 0.35))
    return anh.resize((co, co), Image.LANCZOS)


os.makedirs(RA, exist_ok=True)
for so in range(10):
    anh = ve_so(str(so), 1300 + so * 17)
    duong = os.path.join(RA, "So%d.png" % so)
    anh.save(duong, optimize=True)
    print("ghi", duong, os.path.getsize(duong), "byte")
duong = os.path.join(RA, "VongPhuChu.png")
ve_vong().save(duong, optimize=True)
print("ghi", duong, os.path.getsize(duong), "byte")
print("co chu", co // K, "px, chan chu y", CHAN // K, "/", CAO)
