# -*- coding: utf-8 -*-
"""
DUNG ANH TIEU DE "AC QUY TRO LAI" - dung chung cho man Loading (trang web) va
man trong game (OnGUI), de hai noi GIONG HET nhau va khong phu thuoc font may.

Font: Grenze Gotisch (SIL OFL, CongCu/Fonts) - chu Gothic, co du 134 chu co dau
tieng Viet (doc cmap bang BangKyTuFont). Chi dung de dung anh, khong vao game.

Chay: python CongCu/TieuDe/sinh_tieu_de.py   (can Pillow, KHONG can numpy)
Ra:   Assets/Resources/GiaoDien/TieuDe.png
      Assets/WebGLTemplates/Diablo25D/TemplateData/tieude.png
"""
import io, os, random
from PIL import Image, ImageDraw, ImageFont, ImageFilter, ImageChops

GOC = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", ".."))
FONT = os.path.join(GOC, "CongCu", "Fonts", "GrenzeGotisch[wght].ttf")
CHU = "ÁC QUỶ TRỞ LẠI"
RA = [os.path.join(GOC, "Assets", "Resources", "GiaoDien", "TieuDe.png"),
      os.path.join(GOC, "Assets", "WebGLTemplates", "Diablo25D", "TemplateData", "tieude.png")]

RONG, CAO = 1800, 560          # anh cuoi
K = 2                          # ve gap doi roi thu nho cho mep muot
W, H = RONG * K, CAO * K
rnd = random.Random(1666)


def mau(c):
    return tuple(int(round(v * 255)) for v in c)


def to_mau(mask, c, a=1.0):
    """Lop mau dac c, alpha = mask * a."""
    lop = Image.new("RGBA", mask.size, mau(c) + (0,))
    al = mask if a >= 1.0 else mask.point(lambda v: int(v * a))
    lop.putalpha(al)
    return lop


def chuyen_doc(size, tren, duoi, y0, y1):
    """Anh RGB chuyen mau doc tu 'tren' (y0) xuong 'duoi' (y1)."""
    w, h = size
    cot = Image.new("RGB", (1, h))
    for y in range(h):
        k = min(1.0, max(0.0, (y - y0) / float(max(1, y1 - y0))))
        k = k ** 0.8
        cot.putpixel((0, y), mau(tuple(tren[i] * (1 - k) + duoi[i] * k for i in range(3))))
    return cot.resize((w, h))


# ---------------- 1. Mat na chu ----------------
font = ImageFont.truetype(FONT, 100 * K)
font.set_variation_by_axes([900])                 # Black - net day nhat
bb = font.getbbox(CHU)
co = int(100 * K * (W * 0.86) / (bb[2] - bb[0]))
font = ImageFont.truetype(FONT, co)
font.set_variation_by_axes([900])
bb = font.getbbox(CHU)
tw, th = bb[2] - bb[0], bb[3] - bb[1]
x0 = (W - tw) // 2 - bb[0]
y0 = int(H * 0.40) - th // 2 - bb[1]              # de cho giot mau phia duoi

mat = Image.new("L", (W, H), 0)
ImageDraw.Draw(mat).text((x0, y0), CHU, font=font, fill=255)
hop = mat.getbbox()                               # hop chu that tren anh

# Duong chan chu: dung chu "C" (khong dau, khong dau duoi) de do day net
bbC = font.getbbox("C")
chan = y0 + bbC[3]
dinh = y0 + font.getbbox("C")[1]

# ---------------- 2. Giot mau chay tu chan chu ----------------
# Khong dat giot duoi chu "Ạ" (dau nang nam duoi chan, de nham voi giot)
xA = x0 + font.getlength(CHU[:-2])
xA1 = x0 + font.getlength(CHU[:-1])
giot = Image.new("L", (W, H), 0)
dg = ImageDraw.Draw(giot)
px = mat.load()
cot_co_net = []
for x in range(hop[0] + 6 * K, hop[2] - 6 * K, 3 * K):
    if xA - 10 * K <= x <= xA1 + 10 * K:
        continue
    # net chu cham chan chu tai cot nay?
    if px[x, chan - 3 * K] > 200 and px[x, chan - 12 * K] > 200:
        cot_co_net.append(x)
rnd.shuffle(cot_co_net)
dat = []
for x in cot_co_net:
    if len(dat) >= 16:
        break
    if any(abs(x - d) < 38 * K for d in dat):
        continue
    dat.append(x)
dat.sort()
for chiSo, x in enumerate(dat):
    r = rnd.uniform(3.5, 8.0) * K                 # nua be rong than giot
    # Vai giot dai (cu 3 giot mot giot dai, rai deu ca dong chu), con lai ngan -
    # giot deu nhau trong nhu hang que; de ngau nhien thi giot dai don mot phia
    dai = (rnd.uniform(110, 170) if chiSo % 3 == 1 else rnd.uniform(18, 70)) * K
    ytop = chan - 6 * K
    # than giot hep dan, ve bang nhieu doan ngang
    n = int(dai)
    for i in range(0, n, K):
        k = i / float(n)
        ri = r * (1.0 - 0.62 * k ** 0.7)             # thon dan truoc khi phinh thanh giot
        dg.rectangle([x - ri, ytop + i, x + ri, ytop + i + K], fill=255)
    # dau giot tron phinh ra
    rd = r * 0.95
    dg.ellipse([x - rd, ytop + n - rd * 0.6, x + rd, ytop + n + rd * 1.6], fill=255)
    # bo tron cho noi giot vao chan chu
    dg.ellipse([x - r * 2.2, ytop - r * 1.2, x + r * 2.2, ytop + r * 1.6], fill=255)
giot = giot.filter(ImageFilter.GaussianBlur(0.8 * K)).point(lambda v: 255 if v > 110 else int(v * 2.2))
mat_du = ImageChops.lighter(mat, giot)

# ---------------- 3. Long chu: do mau chuyen doc + van san + vet nut ----------------
long_chu = chuyen_doc((W, H), (0.93, 0.16, 0.08), (0.36, 0.015, 0.012), dinh, chan)
# giot mau sam hon than chu
long_giot = chuyen_doc((W, H), (0.50, 0.03, 0.02), (0.30, 0.01, 0.01), chan, chan + 180 * K)
long_chu = Image.composite(long_giot, long_chu, ImageChops.subtract(giot, mat))

# Van mau chay DOC (nhieu keo gian theo chieu doc) + dom mo lon - khong lam tam
# tam kieu bot bien (ban dau: nhieu min tung diem, nhin ro ri)
hat = Image.effect_noise((W // (5 * K), H // (28 * K)), 50).resize((W, H), Image.BICUBIC)
dom = Image.effect_noise((W // (40 * K), H // (40 * K)), 60).resize((W, H), Image.BICUBIC)
hat = ImageChops.multiply(hat.point(lambda v: int(min(255, 205 + (v - 128) * 0.45))),
                          dom.point(lambda v: int(min(255, 215 + (v - 128) * 0.35))))
long_chu = ImageChops.multiply(long_chu, Image.merge("RGB", (hat, hat, hat)))

nut = Image.new("L", (W, H), 255)
dn = ImageDraw.Draw(nut)
for _ in range(70):
    x = rnd.uniform(hop[0], hop[2]); y = rnd.uniform(hop[1], chan)
    goc = rnd.uniform(0, 6.283)
    for _ in range(rnd.randint(4, 9)):
        import math
        goc += rnd.uniform(-0.9, 0.9)
        dx = math.cos(goc) * rnd.uniform(8, 22) * K; dy = math.sin(goc) * rnd.uniform(8, 22) * K
        dn.line([x, y, x + dx, y + dy], fill=rnd.randint(60, 120), width=max(1, int(1.2 * K)))
        x += dx; y += dy
nut = nut.filter(ImageFilter.GaussianBlur(0.5 * K))
long_chu = ImageChops.multiply(long_chu, Image.merge("RGB", (nut, nut, nut)))

# ---------------- 4. Canh sang phia tren (nhu luoi dao) va bong trong phia duoi ----------------
lech = mat_du.transform(mat_du.size, Image.AFFINE, (1, 0, 0, 0, 1, 3 * K))   # dich xuong -> mep tren lo ra
canh_tren = ImageChops.subtract(mat_du, lech).filter(ImageFilter.GaussianBlur(0.9 * K))
lech2 = mat_du.transform(mat_du.size, Image.AFFINE, (1, 0, 0, 0, 1, -5 * K))
bong_trong = ImageChops.subtract(mat_du, lech2).filter(ImageFilter.GaussianBlur(2 * K))

# ---------------- 5. Vien, quang, bong do ----------------
vien = mat_du.filter(ImageFilter.MaxFilter(2 * (4 * K) + 1)).filter(ImageFilter.GaussianBlur(0.7 * K))
quang = mat_du.filter(ImageFilter.MaxFilter(2 * (6 * K) + 1)).filter(ImageFilter.GaussianBlur(22 * K))
bong = vien.transform(vien.size, Image.AFFINE, (1, 0, -7 * K, 0, 1, -11 * K)).filter(ImageFilter.GaussianBlur(9 * K))

anh = Image.new("RGBA", (W, H), (0, 0, 0, 0))
anh = Image.alpha_composite(anh, to_mau(bong, (0, 0, 0), 0.85))
anh = Image.alpha_composite(anh, to_mau(quang, (1.0, 0.10, 0.04), 0.55))
anh = Image.alpha_composite(anh, to_mau(vien, (0.07, 0.005, 0.005), 1.0))
lop_long = long_chu.convert("RGBA"); lop_long.putalpha(mat_du)
anh = Image.alpha_composite(anh, lop_long)
anh = Image.alpha_composite(anh, to_mau(ImageChops.multiply(bong_trong, mat_du), (0.10, 0.0, 0.0), 0.55))
anh = Image.alpha_composite(anh, to_mau(ImageChops.multiply(canh_tren, mat_du), (1.0, 0.62, 0.42), 0.75))

anh = anh.resize((RONG, CAO), Image.LANCZOS)
# Cat sat phan co hinh (alpha > 2%), chua le cho quang do - bo cuc trong game va
# tren trang web tinh theo dung ti le cua chu, khong theo khoang trong thua
hopA = anh.getchannel("A").point(lambda v: 255 if v > 5 else 0).getbbox()
le = 10
anh = anh.crop((max(0, hopA[0] - le), max(0, hopA[1] - le), min(RONG, hopA[2] + le), min(CAO, hopA[3] + le)))
print("cat con", anh.size)
for duong in RA:
    os.makedirs(os.path.dirname(duong), exist_ok=True)
    anh.save(duong, optimize=True)
    print("ghi", duong, os.path.getsize(duong), "byte")
print("co chu", co // K, "px; hop chu", [v // K for v in hop], "; chan chu y", chan // K, "; so giot", len(dat))

# Gan ma bam cua anh vao trang Loading (tieude.png?v=<ma>): anh doi thi duong dan
# doi, trinh duyet khong giu anh cu trong bo nho dem
import hashlib, re
ma = hashlib.md5(open(RA[1], "rb").read()).hexdigest()[:10]
trang = os.path.join(GOC, "Assets", "WebGLTemplates", "Diablo25D", "index.html")
html = io.open(trang, encoding="utf-8").read()
moi, so = re.subn(r"tieude\.png\?v=[0-9a-zA-Z]*", "tieude.png?v=" + ma, html)
io.open(trang, "w", encoding="utf-8", newline="").write(moi)
print("gan ma", ma, "vao", so, "cho trong index.html")
