# -*- coding: utf-8 -*-
"""
VE ANH CON MAT QUY (nut KHOA GOC NHIN tren HUD cam ung).

Nguoi dung 14/09/2026: "bieu tuong hinh con mat de khoa goc nhin camera dang thiet
ke qua so sai va con bi be hinh, hay ve lai sao cho phu hop voi phong cach rung
ron kinh di cua game".

Ban cu ghep 17 dai doc + 2 hinh tron trong OnGUI - o co nut nho tren dien thoai
(~30 diem) cac dai lech nhau nua diem anh nen mep mat rang cua, "be hinh".
Ban nay la anh RGBA ve san, co mipmap, thu nho muot o moi co:

  - mi mat bang THIT nhan nheo mau huyet du, mep uot do;
  - long trang ua vang, toi dan ra goc, gan mau do chi chit (mat do ngau);
  - trong mat lua cam - do co van tia, vanh den;
  - con nguoi KHE DOC nhu mat quy / mat ran;
  - hai giot mau chay tu mi duoi, hao quang do mo.

HAI ANH:
  MatQuy.png      - dang MO khoa goc nhin (mat mo tron, trong vang ruc)
  MatQuyKhoa.png  - dang KHOA: cung con mat nhung tat lua (trong do sam, khe
                    nguoi hep lai) va mot VET CHEM MAU cheo tu tren-trai xuong
                    duoi-phai - giu dung y nghia "gach cheo" ma nguoi dung da chon
                    cho trang thai khoa (12/09/2026).
Hai anh cat CUNG mot khung nen doi trang thai con mat khong nhay.

Ve tren khung 1024 roi thu con 256. Chi dung PIL (may khong co numpy).
Chay:  python CongCu/Icon/sinh_mat_quy.py
Ra:    Assets/Resources/GiaoDien/MatQuy.png, MatQuyKhoa.png
"""
import os, math, random
from PIL import Image, ImageDraw, ImageFilter, ImageChops

S = 1024
GOC = os.path.normpath(os.path.join(os.path.dirname(__file__), "..", ".."))
RA_MO = os.path.join(GOC, "Assets", "Resources", "GiaoDien", "MatQuy.png")
RA_KHOA = os.path.join(GOC, "Assets", "Resources", "GiaoDien", "MatQuyKhoa.png")


# ---------------------------------------------------------------- tien ich
def mat_na():
    return Image.new("L", (S, S), 0)


def nhan(m, k):
    return m.point(lambda v: min(255, int(v * k)))


def giao(a, b):
    return ImageChops.multiply(a, b)


def phu(nen, mau, m):
    if isinstance(mau, tuple):
        lop = Image.new("RGBA", (S, S), mau + (0,))
    else:
        lop = mau.convert("RGBA")
    lop.putalpha(m)
    nen.alpha_composite(lop)


def cong_sang(nen, mau, m):
    lop = Image.new("RGB", (S, S), mau)
    lop = ImageChops.multiply(lop, Image.merge("RGB", (m, m, m)))
    r, g, b, a = nen.split()
    rgb = ImageChops.add(Image.merge("RGB", (r, g, b)), lop)
    nen.paste(Image.merge("RGBA", rgb.split() + (a,)))


def nhieu(co, sigma, mo, seed):
    random.seed(seed)
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


def ap_khoi(nen, m, d, mo, k_sang, k_toi, mau_sang=(255, 220, 190)):
    b = m.filter(ImageFilter.GaussianBlur(mo))
    lech = ImageChops.offset(b, d, d)
    phu(nen, mau_sang, giao(nhan(ImageChops.subtract(b, lech), k_sang), m))
    phu(nen, (0, 0, 0), giao(nhan(ImageChops.subtract(lech, b), k_toi), m))


def vien_toi(nen, m, mo, k):
    trong = m.filter(ImageFilter.GaussianBlur(mo))
    phu(nen, (0, 0, 0), giao(nhan(ImageChops.subtract(m, trong), k), m))


def qua_hanh(cx, cy, rong, cao_tren, cao_duoi, mu=0.62, n=120):
    """Da giac hinh qua hanh: hai dau nhon, cung tren / duoi cao rieng."""
    tren, duoi = [], []
    for i in range(n + 1):
        u = i / float(n)
        d = u * 2.0 - 1.0
        k = max(0.0, 1.0 - d * d) ** mu
        x = cx - rong / 2.0 + u * rong
        tren.append((x, cy - cao_tren * k))
        duoi.append((x, cy + cao_duoi * k))
    return tren + duoi[::-1]


def ve_mat(khoa):
    rnd = random.Random(1313)
    anh = Image.new("RGBA", (S, S), (0, 0, 0, 0))
    CX, CY = 512, 500

    # ------------------------------------------------ mi mat bang thit
    m_mi = mat_na()
    ImageDraw.Draw(m_mi).polygon(qua_hanh(CX, CY + 6, 1000, 320, 280, mu=0.80), fill=255)
    m_mi = m_mi.filter(ImageFilter.GaussianBlur(3)).point(lambda v: 255 if v > 128 else 0)

    thit = to_mau(Image.blend(nhieu(S // 8, 70, 3, 11), nhieu(S // 2, 50, 1, 12), 0.45),
                  (38, 6, 6), (122, 34, 26))
    phu(anh, thit, m_mi)

    # nep nhan: cung song song voi mep mat, tren va duoi
    m_nhan = mat_na()
    dn = ImageDraw.Draw(m_nhan)
    for lop in range(1, 6):
        for dau, cao in ((-1, 250), (1, 215)):
            k = 1.0 + lop * 0.085
            pts = []
            rong = 700 + lop * 48
            for i in range(0, 61):
                u = i / 60.0
                d = u * 2 - 1
                y = CY + dau * cao * k * max(0.0, 1 - d * d) ** 0.6 + rnd.uniform(-4, 4)
                pts.append((CX - rong / 2 + u * rong, y))
            # cat thanh doan ngan cho giong nep da that
            i = rnd.randint(0, 6)
            while i < len(pts) - 2:
                j = min(len(pts) - 1, i + rnd.randint(6, 16))
                dn.line(pts[i:j + 1], fill=rnd.randint(150, 255), width=rnd.choice((4, 5, 6)))
                i = j + rnd.randint(2, 7)
    m_nhan = giao(m_nhan.filter(ImageFilter.GaussianBlur(1.5)), m_mi)
    phu(anh, (255, 150, 120), nhan(ImageChops.offset(m_nhan, -2, -3), 0.22))
    phu(anh, (14, 2, 2), nhan(m_nhan, 0.75))

    vien_toi(anh, m_mi, 60, 1.6)
    ap_khoi(anh, m_mi, 8, 8, 0.8, 2.0, (230, 130, 100))

    # ------------------------------------------------ khe mat
    m_khe = mat_na()
    ImageDraw.Draw(m_khe).polygon(qua_hanh(CX, CY, 780, 205, 175, mu=0.66), fill=255)
    m_khe = m_khe.filter(ImageFilter.GaussianBlur(1.5))

    # long trang: sang giua, do sam ra mep va goc
    trung = m_khe.filter(ImageFilter.GaussianBlur(70))
    long_trang = to_mau(nhan(trung, 1.15), (70, 14, 10), (226, 204, 158))
    phu(anh, long_trang, m_khe)
    phu(anh, (120, 70, 40), giao(nhan(nhieu(S // 3, 60, 2, 21), 0.25), m_khe))

    # mau do chi chit: duong di ngau nhien tu mep vao trong
    m_gan = mat_na()
    dg = ImageDraw.Draw(m_gan)

    def nhanh(x, y, goc, day, buoc, sau):
        for _ in range(buoc):
            goc += rnd.uniform(-0.55, 0.55)
            dai = rnd.uniform(10, 22)
            nx, ny = x + math.cos(goc) * dai, y + math.sin(goc) * dai
            dg.line((x, y, nx, ny), fill=255, width=max(1, int(round(day))))
            x, y = nx, ny
            day *= 0.93
            if sau < 2 and rnd.random() < 0.16:
                nhanh(x, y, goc + rnd.choice((-1, 1)) * rnd.uniform(0.5, 1.0), day * 0.8,
                      max(3, buoc // 2), sau + 1)

    for i in range(34):
        g = rnd.uniform(0, math.tau)
        ex = CX + math.cos(g) * 420
        ey = CY + math.sin(g) * 210
        huong = math.atan2(CY - ey, CX - ex)
        nhanh(ex, ey, huong + rnd.uniform(-0.4, 0.4), rnd.uniform(4, 7), rnd.randint(8, 16), 0)
    gan_mo = giao(m_gan.filter(ImageFilter.GaussianBlur(1.2)), m_khe)
    phu(anh, (120, 6, 6), nhan(gan_mo.filter(ImageFilter.GaussianBlur(4)), 0.45))
    phu(anh, (150, 12, 10), nhan(gan_mo, 0.9))

    # ------------------------------------------------ trong mat
    IX, IY, IR = CX, CY + 8, 178
    trong = Image.new("RGB", (S, S), (0, 0, 0))
    m_trong = mat_na()
    ImageDraw.Draw(m_trong).ellipse((IX - IR, IY - IR, IX + IR, IY + IR), fill=255)
    # chuyen mau theo ban kinh: ve nhieu vong dong tam tu ngoai vao
    dt = ImageDraw.Draw(trong)
    if khoa:
        mau_ngoai, mau_giua, mau_trong = (40, 2, 2), (150, 16, 8), (220, 60, 20)
    else:
        mau_ngoai, mau_giua, mau_trong = (70, 6, 2), (230, 80, 14), (255, 214, 90)
    for i in range(IR, 0, -2):
        u = i / float(IR)                       # 1 ngoai, 0 tam
        if u > 0.5:
            k = (u - 0.5) / 0.5
            c = tuple(int(mau_giua[j] * (1 - k) + mau_ngoai[j] * k) for j in range(3))
        else:
            k = u / 0.5
            c = tuple(int(mau_trong[j] * (1 - k) + mau_giua[j] * k) for j in range(3))
        dt.ellipse((IX - i, IY - i, IX + i, IY + i), fill=c)
    # van tia
    m_tia = mat_na()
    dtia = ImageDraw.Draw(m_tia)
    for i in range(150):
        g = rnd.uniform(0, math.tau)
        r0 = rnd.uniform(30, 90)
        r1 = rnd.uniform(120, IR - 6)
        dtia.line((IX + math.cos(g) * r0, IY + math.sin(g) * r0,
                   IX + math.cos(g) * r1, IY + math.sin(g) * r1), fill=rnd.randint(90, 255), width=rnd.choice((2, 3, 4)))
    m_tia = m_tia.filter(ImageFilter.GaussianBlur(1.3))
    trongA = trong.convert("RGBA")
    lop_toi = Image.new("RGBA", (S, S), (20, 0, 0, 0))
    lop_toi.putalpha(nhan(m_tia, 0.55))
    trongA.alpha_composite(lop_toi)
    lop_sang = Image.new("RGBA", (S, S), (255, 200, 120, 0) if not khoa else (230, 80, 50, 0))
    lop_sang.putalpha(nhan(ImageChops.offset(m_tia, 3, 0), 0.22))
    trongA.alpha_composite(lop_sang)
    # vanh den quanh trong
    m_vanh = mat_na()
    ImageDraw.Draw(m_vanh).ellipse((IX - IR, IY - IR, IX + IR, IY + IR), outline=255, width=16)
    m_vanh = m_vanh.filter(ImageFilter.GaussianBlur(5))
    lop_vanh = Image.new("RGBA", (S, S), (10, 0, 0, 0))
    lop_vanh.putalpha(m_vanh)
    trongA.alpha_composite(lop_vanh)

    m_trong_hien = giao(m_trong.filter(ImageFilter.GaussianBlur(1.2)), m_khe)
    trongA.putalpha(m_trong_hien)
    anh.alpha_composite(trongA)

    # quang lua quanh trong (chi khi mo)
    if not khoa:
        cong_sang(anh, (200, 60, 10), giao(nhan(m_trong.filter(ImageFilter.GaussianBlur(40)), 0.35), m_khe))

    # ------------------------------------------------ con nguoi khe doc
    rong_khe = 26 if khoa else 44
    m_nguoi = mat_na()
    ImageDraw.Draw(m_nguoi).polygon(
        [(IX + y_to_x, IY + y) for y, y_to_x in
         [(yy, rong_khe * max(0.0, 1 - (yy / 150.0) ** 2) ** 0.8) for yy in range(-150, 151, 4)]]
        + [(IX - rong_khe * max(0.0, 1 - (yy / 150.0) ** 2) ** 0.8, IY + yy) for yy in range(150, -151, -4)],
        fill=255)
    m_nguoi = m_nguoi.filter(ImageFilter.GaussianBlur(1.5))
    phu(anh, (150, 20, 6) if not khoa else (90, 6, 4),
        giao(nhan(m_nguoi.filter(ImageFilter.MaxFilter(15)).filter(ImageFilter.GaussianBlur(8)), 0.8), m_khe))
    phu(anh, (4, 0, 0), giao(m_nguoi, m_khe))

    # ------------------------------------------------ bong mi tren de xuong + mep uot
    m_bongmi = giao(ImageChops.subtract(m_khe, ImageChops.offset(m_khe, 0, 44)).filter(ImageFilter.GaussianBlur(18)), m_khe)
    phu(anh, (18, 0, 0), nhan(m_bongmi, 1.1))
    vien_toi(anh, m_khe, 26, 1.2)

    m_mep = m_khe.point(lambda v: 255 if v > 128 else 0)
    m_mep = ImageChops.subtract(m_mep.filter(ImageFilter.MaxFilter(17)), m_mep).filter(ImageFilter.GaussianBlur(2))
    phu(anh, (150, 26, 20), m_mep)
    phu(anh, (255, 140, 120), nhan(giao(m_mep, ImageChops.offset(m_mep, 0, 3)), 0.35))

    # dom sang uot tren trong
    m_dom = mat_na()
    dd = ImageDraw.Draw(m_dom)
    dd.ellipse((IX - 104, IY - 112, IX - 50, IY - 70), fill=255)
    dd.ellipse((IX + 58, IY + 62, IX + 78, IY + 78), fill=160)
    phu(anh, (255, 246, 230), giao(m_dom.filter(ImageFilter.GaussianBlur(4)), m_khe))

    # ------------------------------------------------ giot mau tu mi duoi
    m_giot = mat_na()
    dgi = ImageDraw.Draw(m_giot)
    for gx, dai, r in ((CX - 150, 150, 15), (CX + 96, 96, 12), (CX - 40, 58, 9)):
        # chan giot bat dau dung tai mep mi duoi o cot gx
        d = (gx - CX) / 390.0
        y0 = CY + 175 * max(0.0, 1 - d * d) ** 0.66 - 4
        n = int(dai)
        for i in range(0, n, 2):
            k = i / float(n)
            ri = r * (1.0 - 0.55 * k ** 0.7)
            dgi.rectangle((gx - ri, y0 + i, gx + ri, y0 + i + 2), fill=255)
        dgi.ellipse((gx - r * 1.05, y0 + n - r * 0.7, gx + r * 1.05, y0 + n + r * 1.5), fill=255)
        dgi.ellipse((gx - r * 2.0, y0 - r * 0.8, gx + r * 2.0, y0 + r * 1.2), fill=255)
    m_giot = m_giot.filter(ImageFilter.GaussianBlur(1.5))
    phu(anh, (0, 0, 0), nhan(ImageChops.offset(m_giot, 4, 6).filter(ImageFilter.GaussianBlur(4)), 0.7))
    phu(anh, (112, 3, 3), m_giot)
    ap_khoi(anh, m_giot, 3, 2, 2.2, 1.6, (255, 120, 100))

    # ------------------------------------------------ vet chem (dang khoa)
    if khoa:
        A = (150, 150)
        B = (874, 874)
        dx, dy = B[0] - A[0], B[1] - A[1]
        L = math.hypot(dx, dy)
        ux, uy = dx / L, dy / L
        nx, ny = -uy, ux
        tren, duoi = [], []
        for i in range(0, 101):
            u = i / 100.0
            day = 50 * math.sin(math.pi * u) ** 0.75 + rnd.uniform(-2.5, 2.5) * math.sin(math.pi * u)
            px, py = A[0] + dx * u, A[1] + dy * u
            tren.append((px + nx * day, py + ny * day))
            duoi.append((px - nx * day * 0.8, py - ny * day * 0.8))
        m_chem = mat_na()
        ImageDraw.Draw(m_chem).polygon(tren + duoi[::-1], fill=255)
        # giot chay xuong tu mep duoi vet chem
        dch = ImageDraw.Draw(m_chem)
        for u, dai, r in ((0.30, 70, 11), (0.52, 120, 14), (0.70, 60, 10)):
            px, py = A[0] + dx * u, A[1] + dy * u
            day = 50 * math.sin(math.pi * u) ** 0.75
            bx, by = px - nx * day * 0.7, py - ny * day * 0.7
            for i in range(0, dai, 2):
                k = i / float(dai)
                ri = r * (1.0 - 0.55 * k ** 0.7)
                dch.rectangle((bx - ri, by + i - 10, bx + ri, by + i - 8), fill=255)
            dch.ellipse((bx - r, by + dai - 10 - r * 0.7, bx + r, by + dai - 10 + r * 1.4), fill=255)
        m_chem = m_chem.filter(ImageFilter.GaussianBlur(1.5))

        m_vien = m_chem.filter(ImageFilter.MaxFilter(25)).filter(ImageFilter.GaussianBlur(3))
        phu(anh, (0, 0, 0), nhan(ImageChops.offset(m_vien, 8, 12).filter(ImageFilter.GaussianBlur(10)), 0.8))
        phu(anh, (8, 0, 0), m_vien)
        long_chem = to_mau(nhieu(S // 6, 60, 2, 31), (110, 2, 2), (190, 14, 8))
        phu(anh, long_chem, m_chem)
        # loi sang doc giua vet chem
        m_loi = mat_na()
        ImageDraw.Draw(m_loi).line([(A[0] + dx * u + nx * 8, A[1] + dy * u + ny * 8)
                                    for u in [i / 40.0 for i in range(6, 35)]], fill=255, width=10)
        cong_sang(anh, (255, 60, 30), nhan(m_loi.filter(ImageFilter.GaussianBlur(6)), 0.8))
        ap_khoi(anh, m_chem, 4, 3, 2.4, 1.8, (255, 140, 110))

    # ------------------------------------------------ hao quang + bong do
    alpha = anh.split()[3]
    ket = Image.new("RGBA", (S, S), (0, 0, 0, 0))
    hao = Image.new("RGBA", (S, S), (170, 12, 4, 0) if not khoa else (120, 4, 2, 0))
    hao.putalpha(nhan(alpha.filter(ImageFilter.GaussianBlur(36)), 0.45))
    ket.alpha_composite(hao)
    bong = Image.new("RGBA", (S, S), (0, 0, 0, 0))
    bong.putalpha(nhan(ImageChops.offset(alpha, 10, 16).filter(ImageFilter.GaussianBlur(16)), 0.75))
    ket.alpha_composite(bong)
    ket.alpha_composite(anh)
    return ket


mo = ve_mat(False)
khoa = ve_mat(True)

# Cat CUNG mot khung vuong cho hai anh (hop bao cua ca hai), roi thu con 256
# Nguong alpha cao (> 35%): bo phan hao quang mo ngoai cung khi tinh khung - tinh ca
# quang thi con mat chi chiem nua anh va nho xiu trong nut
hop = ImageChops.lighter(mo.split()[3], khoa.split()[3]).point(lambda v: 255 if v > 90 else 0).getbbox()
cx, cy = (hop[0] + hop[2]) / 2.0, (hop[1] + hop[3]) / 2.0
nua = max(hop[2] - hop[0], hop[3] - hop[1]) / 2.0 + 6
khung = (int(cx - nua), int(cy - nua), int(cx + nua), int(cy + nua))
for anh, duong in ((mo, RA_MO), (khoa, RA_KHOA)):
    a = anh.crop(khung).resize((256, 256), Image.LANCZOS)
    a.save(duong)
    print("ghi", duong, a.size, os.path.getsize(duong), "byte")
print("khung cat", khung)
