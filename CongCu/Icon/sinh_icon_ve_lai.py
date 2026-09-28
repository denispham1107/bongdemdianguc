# -*- coding: utf-8 -*-
# VE LAI BA ICON: QUA CAU BANG, TANG HINH, LUA DIA NGUC (28/09/2026).
#
# Nguoi dung: ba icon cu "qua tho va so sai, net ve nhu game tre em" - ve lai cho hop noi dung tung ky nang va hop
# boi canh kinh di cua game; "khong can su dung MCP Blender".
#
#   - Qua cau bang: KHOI BANG PHA LE nhieu mat cat (nhu qua cau trong game tu 28/09/2026), vet nut, loi sang, vien lanh;
#     phia sau la vet suong gia, manh bang, tinh the lap lanh.
#   - Tang hinh: phu thuy trum mu CHI CON VIEN SANG (dung hieu ung vien fresnel trong game), than duoi tan thanh hat,
#     doi mat mo trong mu, duoi chan la vong phep (vong no khi hien hinh).
#   - Lua dia nguc: 5 qua cau lua toa quat roi UON CONG di ve mot muc tieu, duoi lua cuon; nen an chu ngu giac do sam.
#
# Anh ra 256x256 nen DEN - IconKyNang CONG anh vao dia nut (cho den khong anh huong, cho sang thi loe len). Ve o 1024
# roi thu nho cho net muot. Chi dung Pillow (may khong co numpy).
#
# Chay: python CongCu/Icon/sinh_icon_ve_lai.py  [--xem]   (--xem: ghep them anh xem truoc tren dia nut vao thu muc tam)
import math
import os
import random
import sys
from PIL import Image, ImageChops, ImageDraw, ImageEnhance, ImageFilter

GOC = os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", "..")
RA = os.path.join(GOC, "Assets", "Resources", "Icons")
W = 1024
S = 256


# ------------------------------------------------------------------ tien ich

def moi(mau=(0, 0, 0)):
    return Image.new("RGB", (W, W), mau)


def P(x, y):
    return (x * W, y * W)


def cong(*ds):
    a = ds[0]
    for b in ds[1:]:
        a = ImageChops.add(a, b)
    return a


def sang(img, k):
    return ImageEnhance.Brightness(img).enhance(k)


def mo(img, r):
    return img.filter(ImageFilter.GaussianBlur(r))


def phat_sang(img, ban_kinh, k):
    """Them quang: ban mo cong vao ban goc."""
    return cong(img, sang(mo(img, ban_kinh), k))


def to_mau(mat_na, mau):
    """Mat na L -> lop RGB mang mau (nhan)."""
    lop = Image.new("RGB", (W, W), mau)
    return ImageChops.multiply(lop, mat_na.convert("RGB"))


def nhan(img, mat_na):
    return ImageChops.multiply(img, mat_na.convert("RGB"))


def nhieu(o, hat, tuong_phan=1.0):
    """Nhieu gia tri (value noise) muot: luoi o x o ngau nhien phong to bicubic -> anh L."""
    rd = random.Random(hat)
    nho = Image.new("L", (o, o))
    nho.putdata([rd.randint(0, 255) for _ in range(o * o)])
    to = nho.resize((W, W), Image.BICUBIC)
    if tuong_phan != 1.0:
        to = ImageEnhance.Contrast(to).enhance(tuong_phan)
    return to


def fbm(hat, o0=6, tang=4):
    """Nhieu nhieu tang (fractal) -> L."""
    kq = None
    trong = 0.0
    w = 1.0
    for i in range(tang):
        n = nhieu(o0 * (2 ** i), hat * 31 + i)
        kq = n if kq is None else Image.blend(kq, n, w / (trong + w))
        trong += w
        w *= 0.55
    return ImageEnhance.Contrast(kq).enhance(2.2)


def doc_theo_y(ham):
    """Mat na L theo toa do y (0..1 tu tren xuong)."""
    col = Image.new("L", (1, W))
    col.putdata([int(max(0.0, min(1.0, ham((y + 0.5) / W))) * 255) for y in range(W)])
    return col.resize((W, W))


def ban_kinh_tu_tam(cx, cy, r0, r1, trong=1.0, ngoai=0.0):
    """Mat na L radial: trong r0 = trong, ngoai r1 = ngoai (noi suy muot)."""
    n = 256
    nho = Image.new("L", (n, n))
    px = []
    for y in range(n):
        for x in range(n):
            d = math.hypot((x + 0.5) / n - cx, (y + 0.5) / n - cy)
            t = min(1.0, max(0.0, (d - r0) / max(1e-6, r1 - r0)))
            t = t * t * (3 - 2 * t)
            px.append(int((trong * (1 - t) + ngoai * t) * 255))
    nho.putdata(px)
    return nho.resize((W, W), Image.BICUBIC)


def chaikin(diem, lan=3, kin=True):
    for _ in range(lan):
        moi_ds = []
        n = len(diem)
        for i in range(n if kin else n - 1):
            a, b = diem[i], diem[(i + 1) % n]
            moi_ds.append((0.75 * a[0] + 0.25 * b[0], 0.75 * a[1] + 0.25 * b[1]))
            moi_ds.append((0.25 * a[0] + 0.75 * b[0], 0.25 * a[1] + 0.75 * b[1]))
        diem = moi_ds
    return diem


def bezier(p0, p1, p2, p3, t):
    u = 1 - t
    return (u * u * u * p0[0] + 3 * u * u * t * p1[0] + 3 * u * t * t * p2[0] + t * t * t * p3[0],
            u * u * u * p0[1] + 3 * u * u * t * p1[1] + 3 * u * t * t * p2[1] + t * t * t * p3[1])


def ngoi_sao(d, x, y, r, mau, mong=0.18):
    """Tinh the lap lanh bon canh."""
    d.polygon([(x, y - r), (x + r * mong, y - r * mong), (x + r, y), (x + r * mong, y + r * mong),
               (x, y + r), (x - r * mong, y + r * mong), (x - r, y), (x - r * mong, y - r * mong)], fill=mau)


def lerp(a, b, t):
    return tuple(int(a[i] + (b[i] - a[i]) * t) for i in range(3))


def thang_mau(ds, t):
    """ds = [(moc, mau)...] tang dan."""
    t = max(0.0, min(1.0, t))
    for i in range(len(ds) - 1):
        if t <= ds[i + 1][0]:
            k = (t - ds[i][0]) / max(1e-6, ds[i + 1][0] - ds[i][0])
            return lerp(ds[i][1], ds[i + 1][1], k)
    return ds[-1][1]


def xuat(img, ten):
    nho = img.resize((S, S), Image.LANCZOS).convert("RGBA")
    duong = os.path.join(RA, ten + ".png")
    nho.save(duong)
    print("da ghi", os.path.normpath(duong))
    return nho


# ------------------------------------------------------------------ QUA CAU BANG

def qua_cau_bang():
    rd = random.Random(7)
    nen = moi()
    cx, cy, R = 0.575, 0.415, 0.235

    # ---- Vet suong gia phia sau (keo ve duoi trai) ----
    huong = (-0.72, 0.69)
    suong = Image.new("L", (W, W), 0)
    ds = ImageDraw.Draw(suong)
    for i in range(26):
        t = i / 25.0
        x = cx + huong[0] * (0.10 + 0.52 * t) + rd.uniform(-0.03, 0.03)
        y = cy + huong[1] * (0.10 + 0.52 * t) + rd.uniform(-0.03, 0.03)
        r = (0.16 - 0.10 * t) * W
        ds.ellipse([x * W - r, y * W - r * 0.8, x * W + r, y * W + r * 0.8], fill=int(120 * (1 - t) + 30))
    suong = mo(suong, 34)
    suong = ImageChops.multiply(suong, fbm(3, 5, 4))
    suong = ImageEnhance.Brightness(suong).enhance(2.2)
    nen = cong(nen, to_mau(suong, (95, 170, 235)))

    # Vet toc do mong
    vet = moi()
    dv = ImageDraw.Draw(vet)
    for i in range(14):
        lech = rd.uniform(-0.17, 0.17)
        x0 = cx + lech * huong[1] + huong[0] * rd.uniform(0.12, 0.22)
        y0 = cy - lech * huong[0] + huong[1] * rd.uniform(0.12, 0.22)
        dai = rd.uniform(0.16, 0.42)
        for k in range(6):
            a = k / 6.0
            b = (k + 1) / 6.0
            c = lerp((170, 225, 255), (10, 30, 60), a)
            dv.line([P(x0 + huong[0] * dai * a, y0 + huong[1] * dai * a), P(x0 + huong[0] * dai * b, y0 + huong[1] * dai * b)],
                    fill=c, width=max(2, int(5 * (1 - a))))
    nen = cong(nen, phat_sang(mo(vet, 1.5), 6, 1.2))

    # ---- Quang lanh quanh khoi ----
    quang = ban_kinh_tu_tam(cx, cy, R * 0.8, R * 1.55, 1.0, 0.0)
    nen = cong(nen, to_mau(quang, (18, 55, 120)))

    # ---- Khoi bang: hai vong dinh, tam giac hoa ----
    ngoai = []
    n1 = 13
    for i in range(n1):
        g = 2 * math.pi * i / n1 + rd.uniform(-0.12, 0.12)
        r = R * rd.uniform(0.86, 1.0)
        ngoai.append((cx + math.cos(g) * r, cy + math.sin(g) * r))
    trong = []
    n2 = 6
    for i in range(n2):
        g = 2 * math.pi * (i + 0.5) / n2 + rd.uniform(-0.2, 0.2)
        r = R * rd.uniform(0.38, 0.52)
        trong.append((cx - 0.03 + math.cos(g) * r, cy - 0.035 + math.sin(g) * r))
    tam = (cx - 0.045, cy - 0.05)

    mat = []
    for i in range(n2):
        mat.append([tam, trong[i], trong[(i + 1) % n2]])
    # noi vong trong - vong ngoai: moi dinh ngoai noi voi dinh trong gan nhat theo goc
    def goc(p):
        return math.atan2(p[1] - cy, p[0] - cx)
    for i in range(n1):
        a, b = ngoai[i], ngoai[(i + 1) % n1]
        gm = goc(((a[0] + b[0]) / 2, (a[1] + b[1]) / 2))
        j = min(range(n2), key=lambda k: abs(math.atan2(math.sin(goc(trong[k]) - gm), math.cos(goc(trong[k]) - gm))))
        mat.append([a, b, trong[j]])
    for j in range(n2):
        a, b = trong[j], trong[(j + 1) % n2]
        gm = goc(((a[0] + b[0]) / 2, (a[1] + b[1]) / 2))
        i = min(range(n1), key=lambda k: abs(math.atan2(math.sin(goc(ngoai[k]) - gm), math.cos(goc(ngoai[k]) - gm))))
        mat.append([a, b, ngoai[i]])

    den = (-0.52, -0.62, 0.59)
    khoi = moi()
    dk = ImageDraw.Draw(khoi)
    for m in mat:
        gx = sum(p[0] for p in m) / 3 - cx
        gy = sum(p[1] for p in m) / 3 - cy
        rr = min(0.97, math.hypot(gx, gy) / R)
        nz = math.sqrt(max(0.02, 1 - rr * rr))
        nx, ny = gx / R + rd.uniform(-0.35, 0.35), gy / R + rd.uniform(-0.35, 0.35)
        l = math.sqrt(nx * nx + ny * ny + nz * nz)
        b = max(0.0, (nx * den[0] + ny * den[1] + nz * den[2]) / l)
        b = b ** 1.4
        c = thang_mau([(0.0, (4, 14, 42)), (0.40, (16, 58, 132)), (0.75, (70, 150, 225)), (1.0, (190, 235, 255))], b)
        dk.polygon([P(*p) for p in m], fill=c)
    mat_na = Image.new("L", (W, W), 0)
    ImageDraw.Draw(mat_na).polygon([P(*p) for p in ngoai], fill=255)

    # Van nut va bot khi ben trong (trang duc, khong cong sang qua)
    nut = Image.new("L", (W, W), 0)
    dn = ImageDraw.Draw(nut)
    for _ in range(7):
        x, y = cx + rd.uniform(-0.12, 0.12), cy + rd.uniform(-0.12, 0.12)
        g = rd.uniform(0, 2 * math.pi)
        for _k in range(rd.randint(3, 6)):
            dai = rd.uniform(0.02, 0.06)
            g += rd.uniform(-0.8, 0.8)
            x2, y2 = x + math.cos(g) * dai, y + math.sin(g) * dai
            dn.line([P(x, y), P(x2, y2)], fill=rd.randint(90, 170), width=3)
            x, y = x2, y2
    for _ in range(40):
        x, y = cx + rd.uniform(-0.17, 0.17), cy + rd.uniform(-0.17, 0.17)
        r = rd.uniform(2, 7)
        dn.ellipse([x * W - r, y * W - r, x * W + r, y * W + r], fill=rd.randint(60, 140))
    nut = ImageChops.multiply(mo(nut, 1.2), mat_na)
    khoi = cong(khoi, to_mau(nut, (200, 235, 255)))

    # Canh mat cat sang
    canh = moi()
    dc = ImageDraw.Draw(canh)
    for m in mat:
        for i in range(3):
            dc.line([P(*m[i]), P(*m[(i + 1) % 3])], fill=(120, 200, 250), width=3)
    canh = nhan(canh, mat_na)
    khoi = cong(khoi, sang(mo(canh, 0.8), 0.75))

    # Vien fresnel + loi sang lech ve phia den
    mo_mn = mo(mat_na, 26)
    vien = ImageChops.multiply(mat_na, ImageChops.invert(mo_mn))
    vien = ImageEnhance.Brightness(vien).enhance(1.9)
    khoi = cong(khoi, to_mau(vien, (140, 215, 255)))
    loi = ban_kinh_tu_tam(cx - 0.02, cy - 0.03, 0.0, R * 0.42, 1.0, 0.0)
    khoi = cong(khoi, to_mau(ImageChops.multiply(loi, mat_na), (70, 140, 210)))

    khoi = nhan(khoi, mat_na)
    nen = ImageChops.lighter(nen, khoi) if False else Image.composite(khoi, nen, mat_na)
    # vien ngoai sac
    ve = moi()
    ImageDraw.Draw(ve).line([P(*p) for p in ngoai + [ngoai[0]]], fill=(190, 238, 255), width=4)
    nen = cong(nen, phat_sang(ve, 9, 0.9))

    # Diem loe dac biet tren canh (specular)
    loe = moi()
    dl = ImageDraw.Draw(loe)
    ngoi_sao(dl, *P(cx - 0.12, cy - 0.15), 44, (255, 255, 255), 0.12)
    ngoi_sao(dl, *P(cx + 0.09, cy - 0.19), 20, (220, 245, 255), 0.15)
    nen = cong(nen, phat_sang(loe, 8, 1.5))

    # ---- Manh bang bay theo + lap lanh ----
    manh = moi()
    dm = ImageDraw.Draw(manh)
    for (mx, my, r) in [(0.30, 0.66, 0.045), (0.21, 0.55, 0.03), (0.40, 0.76, 0.028), (0.16, 0.75, 0.022)]:
        pts = []
        k = rd.randint(4, 6)
        for i in range(k):
            g = 2 * math.pi * i / k + rd.uniform(-0.3, 0.3)
            rr = r * rd.uniform(0.6, 1.0) * (1.6 if i % 2 == 0 else 1.0)
            pts.append(P(mx + math.cos(g) * rr, my + math.sin(g) * rr * 0.8))
        dm.polygon(pts, fill=(70, 150, 225), outline=(200, 240, 255))
    for _ in range(26):
        t = rd.uniform(0.15, 0.7)
        x = cx + huong[0] * t + rd.uniform(-0.12, 0.12)
        y = cy + huong[1] * t + rd.uniform(-0.12, 0.12)
        ngoi_sao(dm, *P(x, y), rd.uniform(6, 16), (200, 240, 255))
    nen = cong(nen, phat_sang(manh, 7, 1.1))
    return nen


# ------------------------------------------------------------------ TANG HINH

def tang_hinh():
    rd = random.Random(11)
    nen = moi()

    # ---- Vong phep duoi chan (vong no khi hien hinh) ----
    vong = moi()
    dv = ImageDraw.Draw(vong)
    cx, cy, rx, ry = 0.5, 0.835, 0.36, 0.085
    for (k, w, c) in [(1.0, 5, (60, 200, 210)), (0.86, 3, (40, 150, 190)), (0.62, 2, (40, 130, 170))]:
        dv.ellipse([P(cx - rx * k, cy - ry * k), P(cx + rx * k, cy + ry * k)], outline=c, width=w)
    # ki tu phep giua hai vong
    for i in range(24):
        g = 2 * math.pi * i / 24
        a = (cx + math.cos(g) * rx * 0.93, cy + math.sin(g) * ry * 0.93)
        b = (cx + math.cos(g) * rx * 0.88, cy + math.sin(g) * ry * 0.88)
        dv.line([P(*a), P(*b)], fill=(90, 220, 220), width=4)
        if i % 3 == 0:
            m = (cx + math.cos(g + 0.08) * rx * 0.905, cy + math.sin(g + 0.08) * ry * 0.905)
            dv.ellipse([m[0] * W - 5, m[1] * W - 3, m[0] * W + 5, m[1] * W + 3], fill=(120, 240, 235))
    # ngoi sao sau canh dep theo phoi canh
    sao = []
    for i in range(6):
        g = -math.pi / 2 + 2 * math.pi * i / 6
        sao.append((cx + math.cos(g) * rx * 0.62, cy + math.sin(g) * ry * 0.62))
    for i in range(6):
        dv.line([P(*sao[i]), P(*sao[(i + 2) % 6])], fill=(35, 120, 150), width=2)
    nen = cong(nen, phat_sang(vong, 12, 1.4))

    # ---- Bong phu thuy trum mu ----
    khung = [(0.50, 0.105), (0.565, 0.145), (0.605, 0.22), (0.62, 0.30), (0.655, 0.36), (0.72, 0.405),
             (0.755, 0.47), (0.765, 0.58), (0.79, 0.70), (0.83, 0.82),
             (0.72, 0.84), (0.62, 0.815), (0.50, 0.85), (0.38, 0.815), (0.28, 0.84), (0.17, 0.82),
             (0.21, 0.70), (0.235, 0.58), (0.245, 0.47), (0.28, 0.405), (0.345, 0.36), (0.38, 0.30),
             (0.395, 0.22), (0.435, 0.145)]
    khung = chaikin(khung, 3)
    mat_na = Image.new("L", (W, W), 0)
    ImageDraw.Draw(mat_na).polygon([P(*p) for p in khung], fill=255)

    # Mu: khoang mat toi han
    lo_mat = Image.new("L", (W, W), 0)
    ImageDraw.Draw(lo_mat).ellipse([P(0.435, 0.205), P(0.565, 0.375)], fill=255)
    lo_mat = mo(lo_mat, 10)

    # Vien fresnel: sang o mep bong, toi dan vao trong - dung kieu shader Tang hinh trong game
    mo1 = mo(mat_na, 30)
    vien = ImageChops.multiply(mat_na, ImageChops.invert(mo1))
    vien = ImageEnhance.Brightness(vien).enhance(3.4)
    mo2 = mo(mat_na, 7)
    vien_sac = ImageChops.multiply(mat_na, ImageChops.invert(mo2))
    vien_sac = ImageEnhance.Brightness(vien_sac).enhance(4.0)

    than = cong(to_mau(vien, (40, 170, 190)), to_mau(vien_sac, (150, 255, 245)))
    # Mep mu va vien khoang mat cung loe
    vm = Image.new("L", (W, W), 0)
    ImageDraw.Draw(vm).ellipse([P(0.435, 0.205), P(0.565, 0.375)], outline=255, width=6)
    vm = ImageChops.multiply(mo(vm, 3), mat_na)
    than = cong(than, to_mau(vm, (90, 220, 220)))
    # Nep ao choang (duong doc mo)
    nep = Image.new("L", (W, W), 0)
    dn = ImageDraw.Draw(nep)
    for x0, x1 in [(0.44, 0.36), (0.50, 0.50), (0.56, 0.64), (0.40, 0.28), (0.60, 0.72)]:
        dn.line([P(x0, 0.43), P((x0 + x1) / 2, 0.62), P(x1, 0.82)], fill=110, width=5)
    nep = ImageChops.multiply(mo(nep, 4), mat_na)
    than = cong(than, to_mau(nep, (40, 150, 165)))
    # Khuc xa mo ben trong (nhu nhin qua kinh)
    kx = ImageChops.multiply(nhieu(9, 5).resize((W, W)).filter(ImageFilter.GaussianBlur(6)), mat_na)
    kx = ImageEnhance.Brightness(ImageEnhance.Contrast(kx).enhance(1.6)).enhance(0.22)
    than = cong(than, to_mau(kx, (70, 160, 170)))
    # Khoang mat toi han roi doi mat
    than = ImageChops.multiply(than, ImageChops.invert(lo_mat).convert("RGB"))
    mat = moi()
    dm = ImageDraw.Draw(mat)
    for x in (0.472, 0.528):
        dm.polygon([P(x - 0.022, 0.293), P(x, 0.284), P(x + 0.022, 0.293), P(x, 0.300)], fill=(170, 255, 240))
    than = cong(than, phat_sang(mat, 6, 2.0))

    # ---- Tan hat tu than duoi ----
    tan = doc_theo_y(lambda y: 1.0 - max(0.0, (y - 0.50) / 0.30))
    n = fbm(21, 8, 4)
    giu = ImageChops.add(ImageChops.multiply(tan, Image.new("L", (W, W), 255)), n, scale=1.0, offset=-150)
    giu = giu.point(lambda v: 0 if v < 70 else (255 if v > 140 else int((v - 70) * 255 / 70)))
    than = nhan(than, giu)
    # hat bay len
    hat = moi()
    dh = ImageDraw.Draw(hat)
    for _ in range(85):
        y = rd.uniform(0.46, 0.84)
        x = rd.uniform(0.20, 0.80)
        # chi ngay mep va duoi than
        k = (y - 0.40) / 0.44
        r = rd.uniform(2, 5) * (0.5 + k)
        c = lerp((25, 90, 105), (120, 220, 210), rd.random() * k)
        if rd.random() < 0.5:
            dh.rectangle([x * W - r, y * W - r, x * W + r, y * W + r], fill=c)
        else:
            dh.ellipse([x * W - r, y * W - r, x * W + r, y * W + r], fill=c)
    hat = nhan(hat, mo(mat_na, 40).point(lambda v: min(255, v * 3)))
    than = cong(than, phat_sang(hat, 5, 0.8))

    nen = cong(nen, phat_sang(than, 14, 0.9))
    # quang lanh mo phia sau
    nen = cong(nen, to_mau(ban_kinh_tu_tam(0.5, 0.42, 0.05, 0.42, 0.25, 0.0), (20, 80, 90)))
    return nen


# ------------------------------------------------------------------ LUA DIA NGUC

def lua_dia_nguc():
    rd = random.Random(23)
    nen = moi()

    # ---- An chu ngu giac do sam (nen) ----
    an = moi()
    da = ImageDraw.Draw(an)
    cx, cy, r = 0.5, 0.5, 0.40
    da.ellipse([P(cx - r, cy - r), P(cx + r, cy + r)], outline=(120, 12, 6), width=7)
    da.ellipse([P(cx - r * 0.9, cy - r * 0.9), P(cx + r * 0.9, cy + r * 0.9)], outline=(90, 8, 4), width=3)
    dinh = [(cx + math.cos(-math.pi / 2 + i * 2 * math.pi / 5) * r * 0.9,
             cy + math.sin(-math.pi / 2 + i * 2 * math.pi / 5) * r * 0.9) for i in range(5)]
    for i in range(5):
        da.line([P(*dinh[i]), P(*dinh[(i + 2) % 5])], fill=(105, 10, 5), width=5)
    for i in range(30):
        g = 2 * math.pi * i / 30
        da.line([P(cx + math.cos(g) * r * 0.92, cy + math.sin(g) * r * 0.92),
                 P(cx + math.cos(g) * r * 0.98, cy + math.sin(g) * r * 0.98)], fill=(110, 12, 6), width=4)
    an = ImageChops.multiply(an, fbm(41, 6, 3).point(lambda v: min(255, 60 + v)).convert("RGB"))
    nen = cong(nen, phat_sang(mo(an, 1.5), 12, 1.0))
    nen = cong(nen, to_mau(ban_kinh_tu_tam(0.5, 0.5, 0.0, 0.45, 0.35, 0.0), (70, 6, 2)))

    # ---- Nam qua cau lua toa quat roi di ve muc tieu ----
    goc_x, goc_y = 0.20, 0.83
    dich = (0.76, 0.24)
    huong0 = math.atan2(dich[1] - goc_y, dich[0] - goc_x)
    lop = moi()
    dau = []
    tien_do = [0.50, 0.63, 0.74, 0.61, 0.47]
    for i, lech in enumerate([-66, -33, 0, 33, 66]):
        g = huong0 + math.radians(lech)
        p0 = (goc_x, goc_y)
        p1 = (goc_x + math.cos(g) * 0.50, goc_y + math.sin(g) * 0.50)
        p2 = (dich[0] - math.cos(huong0) * 0.14 + math.cos(g + math.pi / 2) * 0.05 * (lech / 46.0),
              dich[1] - math.sin(huong0) * 0.14 + math.sin(g + math.pi / 2) * 0.05 * (lech / 46.0))
        p3 = dich
        th = tien_do[i]
        t0 = max(0.0, th - 0.46)
        ds = ImageDraw.Draw(lop)
        buoc = 90
        for s in range(buoc + 1):
            f = s / buoc
            t = t0 + (th - t0) * f
            x, y = bezier(p0, p1, p2, p3, t)
            # uon luon nhu lua cuon
            tt = min(1.0, t + 0.01)
            x2, y2 = bezier(p0, p1, p2, p3, tt)
            vx, vy = x2 - x, y2 - y
            l = math.hypot(vx, vy) + 1e-9
            nx, ny = -vy / l, vx / l
            lac = math.sin(f * 14 + i * 1.7) * 0.012 * (1 - f)
            x += nx * lac
            y += ny * lac
            rr = (0.008 + 0.036 * f ** 1.5) * W
            c = thang_mau([(0.0, (60, 4, 0)), (0.45, (190, 40, 4)), (0.8, (255, 120, 20)), (1.0, (255, 200, 90))], f)
            ds.ellipse([x * W - rr, y * W - rr, x * W + rr, y * W + rr], fill=c)
        dau.append(bezier(p0, p1, p2, p3, th))
    # xe lua thanh luoi nhu ngon lua (nhieu), roi toa sang
    lop = nhan(lop, fbm(55, 10, 4).point(lambda v: min(255, int(v * 1.5))))
    lop = mo(lop, 3)
    nen = cong(nen, phat_sang(lop, 18, 1.1))

    # dau qua cau: tang trang - vang - cam
    dq = moi()
    dd = ImageDraw.Draw(dq)
    for (x, y) in dau:
        for (rr, c) in [(0.056, (190, 40, 4)), (0.040, (255, 120, 20)), (0.027, (255, 205, 100)), (0.015, (255, 250, 225))]:
            dd.ellipse([P(x - rr, y - rr), P(x + rr, y + rr)], fill=c)
    dq = nhan(mo(dq, 2), fbm(66, 14, 3).point(lambda v: min(255, 120 + v)))
    nen = cong(nen, phat_sang(dq, 18, 1.1))

    # tia lua va tan tro bay
    tro = moi()
    dt = ImageDraw.Draw(tro)
    for _ in range(90):
        x, y = rd.uniform(0.15, 0.85), rd.uniform(0.15, 0.85)
        r = rd.uniform(2, 6)
        dt.ellipse([x * W - r, y * W - r, x * W + r, y * W + r], fill=lerp((200, 60, 5), (255, 200, 80), rd.random()))
    tro = nhan(tro, ban_kinh_tu_tam(0.5, 0.5, 0.2, 0.45, 1.0, 0.0))
    nen = cong(nen, phat_sang(tro, 5, 1.0))
    return nen


# ------------------------------------------------------------------ xem truoc tren dia nut (giong IconKyNang.Ve)

def dia_nut(ruot, nen_he):
    den = (-0.40, 0.62, 0.68)
    l = math.sqrt(sum(v * v for v in den))
    den = tuple(v / l for v in den)
    a = ruot.convert("RGB").load()
    kq = Image.new("RGBA", (S, S))
    o = kq.load()

    def muot(c0, c1, x):
        t = min(1.0, max(0.0, (x - c0) / max(c1 - c0, 1e-6)))
        return t * t * (3 - 2 * t)
    for yy in range(S):
        for x in range(S):
            u = (x + 0.5) / S * 2 - 1
            v = -((yy + 0.5) / S * 2 - 1)          # Unity: y tu duoi len
            d = math.hypot(u, v)
            a_dia = 1 - muot(0.965, 1.0, d)
            if a_dia <= 0.001:
                o[x, yy] = (0, 0, 0, 0)
                continue
            f = min(1.0, d) ** 3.2
            hx, hy = (u / d, v / d) if d > 1e-5 else (0, 0)
            N = (hx * f, hy * f, math.sqrt(max(1e-4, 1 - f * f)))
            dif = max(0.0, min(1.0, N[0] * den[0] + N[1] * den[1] + N[2] * den[2]))
            c = [nen_he[k] * (0.35 + 0.95 * dif) + dif ** 18 * 0.30 for k in range(3)]
            rim = muot(0.62, 0.99, d) * max(0.0, min(1.0, -(N[0] * den[0] + N[1] * den[1] + N[2] * den[2])))
            for k, m in enumerate((0.45, 0.50, 0.68)):
                c[k] += m * rim * 0.45
            k2 = 1 - muot(0.80, 0.99, d)
            p = a[x, yy]
            for k in range(3):
                c[k] += p[k] / 255.0 * k2
            vien = muot(0.88, 0.97, d) * (1 - muot(0.97, 1.0, d))
            for k, m in enumerate((1.0, 0.97, 0.88)):
                c[k] = c[k] + (m - c[k]) * vien * 0.40
            o[x, yy] = tuple(int(max(0.0, min(1.0, c[k])) * 255) for k in range(3)) + (int(a_dia * 255),)
    return kq


if __name__ == "__main__":
    kq = {}
    kq["CauBang"] = xuat(qua_cau_bang(), "CauBang")
    kq["TangHinh"] = xuat(tang_hinh(), "TangHinh")
    kq["LuaDiaNguc"] = xuat(lua_dia_nguc(), "LuaDiaNguc")
    if "--xem" in sys.argv:
        thu_muc = sys.argv[sys.argv.index("--xem") + 1]
        nen = {"CauBang": (0.04, 0.17, 0.34), "TangHinh": (0.03, 0.13, 0.16), "LuaDiaNguc": (0.24, 0.02, 0.02)}
        tam = Image.new("RGB", (S * 3 * 2 + 40, S * 2 + 20), (24, 22, 26))
        for i, ten in enumerate(["CauBang", "TangHinh", "LuaDiaNguc"]):
            tam.paste(kq[ten].convert("RGB"), (i * (S + 10), 0))
            nut = dia_nut(kq[ten], nen[ten])
            tam.paste(nut, (i * (S + 10), S + 10), nut)
            nho = nut.resize((84, 84), Image.LANCZOS)
            tam.paste(nho, (3 * (S + 10) + 20 + i * 100, S + 10), nho)
        tam.save(os.path.join(thu_muc, "icon_moi.png"))
        print("xem truoc:", os.path.join(thu_muc, "icon_moi.png"))
