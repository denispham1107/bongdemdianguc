# -*- coding: utf-8 -*-
"""
VE ANH QUYEN SACH PHEP (nut "Sach phep" tren HUD).

Nguoi dung 13/09/2026: "hien dang ve rat so sai, ve lai quyen sach giong quyen
sach cua phu thuy va phu hop phong cach rung ron, kinh di".

Ban cu ve bang 4 hinh chu nhat trong OnGUI. Ban nay la mot anh RGBA ve san:
bia da thuoc mau huyet du nut ne, gay sach co dai noi, boc goc bang dong den
dinh tan, vong phu chu khac chu run bao ngoi sao nam canh nguoc, dau lau o
giua voi hai hoc mat chay do, day da khoa ben phai va mep giay ngai vang.

Ve tren khung 1024 roi thu con 256 (khu rang cua), nen trong + bong do mo
de dat len nen tron cua nut.

Chi dung PIL (may khong co numpy).
Chay:  python CongCu/Icon/sinh_sach_phep.py
Ra:    Assets/Resources/GiaoDien/SachPhep.png
"""
import os, math, random
from PIL import Image, ImageDraw, ImageFilter, ImageChops

random.seed(1107)
S = 1024
GOC = os.path.normpath(os.path.join(os.path.dirname(__file__), "..", ".."))
RA = os.path.join(GOC, "Assets", "Resources", "GiaoDien", "SachPhep.png")


# ---------------------------------------------------------------- tien ich
def mat_na():
    return Image.new("L", (S, S), 0)


def nhan(m, k):
    return m.point(lambda v: min(255, int(v * k)))


def giao(a, b):
    return ImageChops.multiply(a, b)


def phu(nen, mau, m):
    """Dat mau (tuple RGB hoac anh RGB) len nen theo mat na m."""
    if isinstance(mau, tuple):
        lop = Image.new("RGBA", (S, S), mau + (0,))
    else:
        lop = mau.convert("RGBA")
    lop.putalpha(m)
    nen.alpha_composite(lop)


def cong_sang(nen, mau, m):
    """Cong anh sang (phat sang) - chi cong vao RGB, giu alpha cua nen."""
    lop = Image.new("RGB", (S, S), mau)
    lop = ImageChops.multiply(lop, Image.merge("RGB", (m, m, m)))
    r, g, b, a = nen.split()
    rgb = ImageChops.add(Image.merge("RGB", (r, g, b)), lop)
    nen.paste(Image.merge("RGBA", rgb.split() + (a,)))


def nhieu(co, sigma, mo):
    n = Image.effect_noise((co, co), sigma)
    if co != S:
        n = n.resize((S, S), Image.BICUBIC)
    return n.filter(ImageFilter.GaussianBlur(mo)) if mo > 0 else n


def to_mau(n, toi, sang):
    kenh = []
    for i in range(3):
        a, b = toi[i], sang[i]
        kenh.append(n.point(lambda v, a=a, b=b: int(a + (b - a) * v / 255.0)))
    return Image.merge("RGB", kenh)


def tron(*ds):
    """Tron nhieu anh xam theo trung binh."""
    kq = ds[0]
    for i, d in enumerate(ds[1:], start=2):
        kq = Image.blend(kq, d, 1.0 / i)
    return kq


def noi_khoi(m, d, mo, k_sang, k_toi):
    """Tra ve (sang, toi): vien sang tren-trai, vien toi duoi-phai ben trong m."""
    b = m.filter(ImageFilter.GaussianBlur(mo))
    lech = ImageChops.offset(b, d, d)
    sang = giao(nhan(ImageChops.subtract(b, lech), k_sang), m)
    toi = giao(nhan(ImageChops.subtract(lech, b), k_toi), m)
    return sang, toi


def ap_khoi(nen, m, d, mo, k_sang, k_toi, mau_sang=(255, 235, 200)):
    s, t = noi_khoi(m, d, mo, k_sang, k_toi)
    phu(nen, mau_sang, s)
    phu(nen, (0, 0, 0), t)


def vien_toi(nen, m, mo, k):
    """Toi dan vao mep trong cua mat na (cu, sat mep)."""
    trong = m.filter(ImageFilter.GaussianBlur(mo))
    phu(nen, (0, 0, 0), giao(nhan(ImageChops.subtract(m, trong), k), m))


# ---------------------------------------------------------------- kich thuoc
X0, Y0, X1, Y1 = 230, 140, 800, 890        # bia truoc
GAY = 78                                    # be ngang gay sach
DAY = 40                                    # do day khoi giay lo ra
CX = (X0 + GAY + X1) // 2 - 12              # tam huy hieu
CY = 505

anh = Image.new("RGBA", (S, S), (0, 0, 0, 0))

# ---------------------------------------------------------------- bia sau + giay
m_sau = mat_na()
ImageDraw.Draw(m_sau).rounded_rectangle((X0 + DAY + 8, Y0 + DAY, X1 + DAY + 8, Y1 + DAY), 30, fill=255)
phu(anh, to_mau(nhieu(S // 4, 60, 2), (22, 5, 5), (58, 14, 11)), m_sau)
vien_toi(anh, m_sau, 14, 2.0)

m_giay = mat_na()
ImageDraw.Draw(m_giay).rounded_rectangle((X0 + 18, Y0 + 12, X1 + DAY - 2, Y1 + DAY - 8), 16, fill=255)
giay = to_mau(nhieu(S // 2, 50, 1), (140, 116, 74), (214, 192, 140))
phu(anh, giay, m_giay)
# van mep giay: soc doc ben phai, soc ngang ben duoi
m_van = mat_na()
dv = ImageDraw.Draw(m_van)
for x in range(X1 - 10, X1 + DAY, 4):
    dv.line((x, Y0 + 20, x, Y1 + DAY - 14), fill=random.randint(60, 150), width=1)
for y in range(Y1 - 10, Y1 + DAY, 4):
    dv.line((X0 + 26, y, X1 + DAY - 10, y), fill=random.randint(60, 150), width=1)
phu(anh, (70, 48, 26), giao(m_van, m_giay))
# giay ngai o mep ngoai + vet o do
phu(anh, (60, 30, 12), giao(nhan(nhieu(S // 16, 90, 6), 0.9), m_giay))
vien_toi(anh, m_giay, 10, 1.6)

# ---------------------------------------------------------------- bia truoc
m_bia = mat_na()
ImageDraw.Draw(m_bia).rounded_rectangle((X0, Y0, X1, Y1), 30, fill=255)

van_to = nhieu(S // 10, 70, 3)
van_nho = nhieu(S, 45, 1.2)
van_da = tron(van_to, van_to, van_nho)
da = to_mau(van_da, (38, 7, 6), (112, 26, 18))
phu(anh, da, m_bia)

# vet nut da: duong di ngau nhien, khac chim xuong (toi) + mep sang
m_nut = mat_na()
dn = ImageDraw.Draw(m_nut)
for _ in range(26):
    x, y = random.uniform(X0, X1), random.uniform(Y0, Y1)
    goc = random.uniform(0, math.tau)
    for _ in range(random.randint(6, 16)):
        goc += random.uniform(-0.9, 0.9)
        dai = random.uniform(8, 26)
        nx, ny = x + math.cos(goc) * dai, y + math.sin(goc) * dai
        dn.line((x, y, nx, ny), fill=255, width=random.choice((2, 2, 3)))
        x, y = nx, ny
m_nut = giao(m_nut.filter(ImageFilter.GaussianBlur(0.8)), m_bia)
phu(anh, (230, 150, 120), giao(nhan(ImageChops.offset(m_nut, 2, 2), 0.25), m_bia))
phu(anh, (10, 2, 2), nhan(m_nut, 0.85))

# gay sach toi hon + dai noi
m_gay = mat_na()
ImageDraw.Draw(m_gay).rounded_rectangle((X0, Y0, X0 + GAY, Y1), 30, fill=255)
ImageDraw.Draw(m_gay).rectangle((X0 + 30, Y0, X0 + GAY, Y1), fill=255)
m_gay = giao(m_gay, m_bia)
phu(anh, (12, 2, 2), nhan(m_gay, 0.55))
m_ranh = mat_na()
ImageDraw.Draw(m_ranh).line((X0 + GAY, Y0 + 6, X0 + GAY, Y1 - 6), fill=255, width=7)
phu(anh, (0, 0, 0), nhan(m_ranh.filter(ImageFilter.GaussianBlur(3)), 0.9))
for i in range(5):
    y = Y0 + 95 + i * (Y1 - Y0 - 190) / 4.0
    m_dai = mat_na()
    ImageDraw.Draw(m_dai).rounded_rectangle((X0 - 4, y - 13, X0 + GAY - 6, y + 13), 12, fill=255)
    phu(anh, to_mau(van_da, (30, 6, 5), (84, 20, 14)), m_dai)
    ap_khoi(anh, m_dai, 5, 4, 1.2, 3.0, (200, 120, 90))

vien_toi(anh, m_bia, 55, 1.5)
ap_khoi(anh, m_bia, 9, 7, 0.9, 2.4, (190, 110, 80))

# duong chi khau quanh bia (nam phia trong gay)
m_chi = mat_na()
dc = ImageDraw.Draw(m_chi)
L, T, R, B = X0 + GAY + 20, Y0 + 26, X1 - 26, Y1 - 26
canh = [((L, T), (R, T)), ((R, T), (R, B)), ((R, B), (L, B)), ((L, B), (L, T))]
for (ax, ay), (bx, by) in canh:
    dai = math.hypot(bx - ax, by - ay)
    n = int(dai // 26)
    for k in range(n):
        u0, u1 = k / n, (k + 0.55) / n
        dc.line((ax + (bx - ax) * u0, ay + (by - ay) * u0,
                 ax + (bx - ax) * u1, ay + (by - ay) * u1), fill=255, width=5)
phu(anh, (0, 0, 0), nhan(ImageChops.offset(m_chi, 2, 3).filter(ImageFilter.GaussianBlur(1.5)), 0.7))
phu(anh, (150, 118, 80), nhan(m_chi, 0.85))

# ---------------------------------------------------------------- vong phu chu
R_NGOAI, R_TRONG = 186, 150
kim = to_mau(tron(nhieu(S // 6, 60, 2), nhieu(S, 40, 1)), (70, 50, 22), (176, 136, 70))

m_vong = mat_na()
dvg = ImageDraw.Draw(m_vong)
dvg.ellipse((CX - R_NGOAI, CY - R_NGOAI, CX + R_NGOAI, CY + R_NGOAI), outline=255, width=10)
dvg.ellipse((CX - R_TRONG, CY - R_TRONG, CX + R_TRONG, CY + R_TRONG), outline=255, width=7)
# chu run giua hai vong: moi ky tu la 2-3 net ngan ngau nhien
r_giua = (R_NGOAI + R_TRONG) / 2.0
for k in range(22):
    g = k / 22.0 * math.tau
    tx, ty = CX + math.cos(g) * r_giua, CY + math.sin(g) * r_giua
    ox, oy = math.cos(g), math.sin(g)         # huong ra ngoai
    px, py = -oy, ox                          # huong doc vong
    for _ in range(random.randint(2, 3)):
        a = (random.uniform(-7, 7), random.uniform(-9, 9))
        b = (random.uniform(-7, 7), random.uniform(-9, 9))
        dvg.line((tx + px * a[0] + ox * a[1], ty + py * a[0] + oy * a[1],
                  tx + px * b[0] + ox * b[1], ty + py * b[0] + oy * b[1]), fill=255, width=4)
# ngoi sao nam canh NGUOC (mot mui chi xuong)
diem = []
for k in range(5):
    g = math.radians(90 + k * 72)
    diem.append((CX + math.cos(g) * (R_TRONG - 4), CY + math.sin(g) * (R_TRONG - 4)))
m_sao = mat_na()
ds = ImageDraw.Draw(m_sao)
for k in range(5):
    ds.line(diem[k] + diem[(k + 2) % 5], fill=255, width=8)

# ranh khac: bong toi ben duoi net vang
phu(anh, (0, 0, 0), nhan(ImageChops.offset(m_vong, 3, 4).filter(ImageFilter.GaussianBlur(2)), 0.9))
phu(anh, kim, m_vong)
ap_khoi(anh, m_vong, 2, 1.5, 3.0, 2.0, (255, 230, 170))

# sao: net do sam + chay sang
phu(anh, (0, 0, 0), nhan(ImageChops.offset(m_sao, 3, 4).filter(ImageFilter.GaussianBlur(2)), 0.8))
phu(anh, (150, 18, 10), m_sao)
cong_sang(anh, (190, 30, 10), nhan(m_sao.filter(ImageFilter.GaussianBlur(14)), 0.55))
cong_sang(anh, (255, 90, 40), nhan(m_sao.filter(ImageFilter.GaussianBlur(2)), 0.35))

# ---------------------------------------------------------------- dau lau
SX, SY = CX, CY - 22
xuong = to_mau(tron(nhieu(S // 8, 50, 2), nhieu(S, 35, 1)), (150, 132, 100), (226, 212, 180))

m_lau = mat_na()
dl = ImageDraw.Draw(m_lau)
dl.ellipse((SX - 92, SY - 108, SX + 92, SY + 70), fill=255)                 # so
dl.rounded_rectangle((SX - 64, SY + 20, SX + 64, SY + 118), 28, fill=255)   # ham tren
dl.ellipse((SX - 98, SY + 8, SX - 40, SY + 62), fill=255)                   # go ma
dl.ellipse((SX + 40, SY + 8, SX + 98, SY + 62), fill=255)
m_ham = mat_na()
ImageDraw.Draw(m_ham).rounded_rectangle((SX - 50, SY + 112, SX + 50, SY + 158), 20, fill=255)

bong = ImageChops.offset(ImageChops.lighter(m_lau, m_ham), 8, 12).filter(ImageFilter.GaussianBlur(10))
phu(anh, (0, 0, 0), nhan(bong, 0.85))

for m in (m_ham, m_lau):
    phu(anh, xuong, m)
    vien_toi(anh, m, 22, 1.3)
    ap_khoi(anh, m, 6, 5, 1.4, 2.2)

# hoc mat nhon, cau co (ac)
m_hoc = mat_na()
dh = ImageDraw.Draw(m_hoc)
for dau in (-1, 1):
    pts = [(-74, -12), (-16, 10), (-14, 44), (-48, 50), (-76, 24)]
    dh.polygon([(SX + dau * x, SY + y) for x, y in pts], fill=255)
dh.polygon([(SX, SY + 58), (SX - 15, SY + 94), (SX + 15, SY + 94)], fill=255)   # mui
dh.rectangle((SX - 46, SY + 122, SX + 46, SY + 132), fill=255)                  # khe mieng
m_hoc = m_hoc.filter(ImageFilter.GaussianBlur(1.2))
phu(anh, (8, 2, 2), m_hoc)
# rang
m_rang = mat_na()
dr = ImageDraw.Draw(m_rang)
for k in range(-3, 4):
    x = SX + k * 13
    dr.line((x, SY + 104, x, SY + 150), fill=255, width=3)
phu(anh, (30, 18, 12), nhan(m_rang, 0.9))
# vet nut tren so
m_nutso = mat_na()
dnn = ImageDraw.Draw(m_nutso)
x, y = SX + 30, SY - 104
for dx, dy in ((-8, 22), (10, 18), (-14, 20), (6, 14)):
    dnn.line((x, y, x + dx, y + dy), fill=255, width=3)
    x, y = x + dx, y + dy
phu(anh, (40, 26, 18), giao(m_nutso, m_lau))

# mat chay do
m_mat = mat_na()
dm = ImageDraw.Draw(m_mat)
for dau in (-1, 1):
    ex, ey = SX + dau * 44, SY + 24
    dm.ellipse((ex - 13, ey - 9, ex + 13, ey + 9), fill=255)
cong_sang(anh, (255, 40, 10), nhan(m_mat.filter(ImageFilter.GaussianBlur(26)), 0.9))
cong_sang(anh, (255, 60, 20), nhan(m_mat.filter(ImageFilter.GaussianBlur(9)), 1.0))
phu(anh, (255, 70, 30), m_mat)
m_loi = mat_na()
dm2 = ImageDraw.Draw(m_loi)
for dau in (-1, 1):
    ex, ey = SX + dau * 44, SY + 24
    dm2.ellipse((ex - 5, ey - 4, ex + 5, ey + 4), fill=255)
phu(anh, (255, 220, 150), m_loi.filter(ImageFilter.GaussianBlur(1.5)))

# ---------------------------------------------------------------- boc goc dong den
dong = to_mau(tron(nhieu(S // 5, 70, 2), nhieu(S, 50, 1)), (52, 42, 30), (150, 124, 84))
HINH_GOC = [(-8, -8), (132, -8), (132, 16), (96, 26), (62, 44), (44, 62), (26, 96), (16, 132), (-8, 132)]
DINH = [(26, 26), (92, 12), (12, 92)]


def boc_goc(cx, cy, sx, sy):
    m = mat_na()
    ImageDraw.Draw(m).polygon([(cx + sx * x, cy + sy * y) for x, y in HINH_GOC], fill=255)
    m = giao(m, m_bia.filter(ImageFilter.MaxFilter(17)))
    phu(anh, (0, 0, 0), nhan(ImageChops.offset(m, 6, 8).filter(ImageFilter.GaussianBlur(6)), 0.8))
    phu(anh, dong, m)
    phu(anh, (40, 70, 55), giao(nhan(nhieu(S // 12, 90, 5), 0.35), m))   # ri xanh
    vien_toi(anh, m, 12, 1.2)
    ap_khoi(anh, m, 4, 3, 2.4, 2.4, (255, 225, 170))
    for dx, dy in DINH:
        px, py = cx + sx * dx, cy + sy * dy
        md = mat_na()
        ImageDraw.Draw(md).ellipse((px - 9, py - 9, px + 9, py + 9), fill=255)
        phu(anh, (0, 0, 0), nhan(ImageChops.offset(md, 2, 3).filter(ImageFilter.GaussianBlur(2)), 0.9))
        phu(anh, (170, 146, 104), md)
        ap_khoi(anh, md, 3, 2, 3.0, 3.0, (255, 240, 200))


boc_goc(X1, Y0, -1, 1)
boc_goc(X1, Y1, -1, -1)
boc_goc(X0 + GAY - 10, Y0, 1, 1)
boc_goc(X0 + GAY - 10, Y1, 1, -1)

# ---------------------------------------------------------------- day da + khoa
m_day = mat_na()
ImageDraw.Draw(m_day).rounded_rectangle((X1 - 52, CY - 44, X1 + DAY + 30, CY + 44), 10, fill=255)
phu(anh, (0, 0, 0), nhan(ImageChops.offset(m_day, 6, 9).filter(ImageFilter.GaussianBlur(7)), 0.85))
phu(anh, to_mau(van_da, (26, 12, 8), (74, 40, 26)), m_day)
vien_toi(anh, m_day, 10, 1.4)
ap_khoi(anh, m_day, 4, 3, 1.8, 2.6, (255, 200, 160))

m_khoa = mat_na()
dk = ImageDraw.Draw(m_khoa)
dk.rounded_rectangle((X1 - 78, CY - 60, X1 - 20, CY + 60), 14, fill=255)
m_lo = mat_na()
ImageDraw.Draw(m_lo).rounded_rectangle((X1 - 64, CY - 44, X1 - 34, CY + 44), 8, fill=255)
m_khoa = ImageChops.subtract(m_khoa, m_lo)
phu(anh, (0, 0, 0), nhan(ImageChops.offset(m_khoa, 6, 8).filter(ImageFilter.GaussianBlur(6)), 0.9))
phu(anh, dong, m_khoa)
ap_khoi(anh, m_khoa, 4, 3, 2.6, 2.6, (255, 225, 170))
m_ghim = mat_na()
ImageDraw.Draw(m_ghim).rounded_rectangle((X1 - 88, CY - 7, X1 - 40, CY + 7), 6, fill=255)
phu(anh, (160, 136, 96), m_ghim)
ap_khoi(anh, m_ghim, 2, 2, 3.0, 3.0, (255, 240, 200))

# ---------------------------------------------------------------- giot mau tu mep tren
m_mau = mat_na()
dmau = ImageDraw.Draw(m_mau)
for gx, dai, rong in ((CX - 120, 70, 16), (CX + 70, 120, 18), (CX + 96, 46, 12)):
    dmau.rounded_rectangle((gx - rong / 2, Y0 + 4, gx + rong / 2, Y0 + dai), rong / 2, fill=255)
    dmau.ellipse((gx - rong * 0.8, Y0 + dai - rong * 0.9, gx + rong * 0.8, Y0 + dai + rong * 0.7), fill=255)
dmau.rectangle((CX - 150, Y0 + 2, CX + 130, Y0 + 16), fill=255)
m_mau = giao(m_mau.filter(ImageFilter.GaussianBlur(2)), m_bia)
phu(anh, (0, 0, 0), nhan(ImageChops.offset(m_mau, 2, 4).filter(ImageFilter.GaussianBlur(3)), 0.6))
phu(anh, (82, 2, 2), m_mau)
ap_khoi(anh, m_mau, 3, 2, 1.6, 1.6, (255, 120, 110))

# ---------------------------------------------------------------- nghieng, bong, hao quang
anh = anh.rotate(7, resample=Image.BICUBIC, center=(S / 2, S / 2))
alpha = anh.split()[3]

ket = Image.new("RGBA", (S, S), (0, 0, 0, 0))
hao = Image.new("RGBA", (S, S), (150, 10, 4, 0))
hao.putalpha(nhan(alpha.filter(ImageFilter.GaussianBlur(40)), 0.35))
ket.alpha_composite(hao)
bong_do = Image.new("RGBA", (S, S), (0, 0, 0, 0))
bong_do.putalpha(nhan(ImageChops.offset(alpha, 14, 22).filter(ImageFilter.GaussianBlur(18)), 0.8))
ket.alpha_composite(bong_do)
ket.alpha_composite(anh)

# cat vuong quanh hinh roi thu con 256
hop = ket.split()[3].point(lambda v: 255 if v > 8 else 0).getbbox()
cx, cy = (hop[0] + hop[2]) / 2.0, (hop[1] + hop[3]) / 2.0
nua = max(hop[2] - hop[0], hop[3] - hop[1]) / 2.0 + 6
ket = ket.crop((int(cx - nua), int(cy - nua), int(cx + nua), int(cy + nua)))
ket = ket.resize((256, 256), Image.LANCZOS)
ket.save(RA)
print("ghi", RA, ket.size)
