# -*- coding: utf-8 -*-
"""
VE BINH MAU VA BINH MANA (ky nang dung binh + vat pham roi ra khi giet quai).

Nguoi dung 13/09/2026: "ve binh mau / binh mana phu hop voi phong cach cua game
kinh di rung ron".

Mot dang binh cho ca hai: binh gia kim bung tron co dai, NUT BIT LA MOT CAI SO
nho bang xuong, dai sat gi quanh co binh co dinh tan, thuy tinh xanh den duc.
  - Binh mau: mau do sam phat sang, bot khi noi len, mau RI ra ngoai tu mieng
    binh chay dai xuong than, hai hoc mat so ro do.
  - Binh mana: nuoc xanh lam phat sang xoay cuon, chu run khac mo tren thuy
    tinh, hoc mat so ro xanh.

Ra hai loai anh moi binh:
  Assets/Resources/Icons/BinhMau.png, BinhMana.png  - 256 RGB NEN DEN, cung kieu
      voi anh ky nang khac: IconKyNang CONG anh nay len cai dia nut (cho den
      khong anh huong, cho sang loe len).
  Assets/Resources/VatPham/BinhMau.png, BinhMana.png - 256 RGBA NEN TRONG, co
      quang sang - hinh binh roi tren mat dat.

Chi dung PIL.  Chay: python CongCu/Icon/sinh_binh.py
"""
import os, math, random
from PIL import Image, ImageDraw, ImageFilter, ImageChops

S = 1024
GOC = os.path.normpath(os.path.join(os.path.dirname(__file__), "..", ".."))


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


def doc_y(toi, sang, y0, y1):
    """Anh chuyen mau theo chieu doc tu y0 (toi) den y1 (sang)."""
    g = Image.linear_gradient("L").resize((S, S))       # 0 o tren, 255 o duoi
    g = g.point(lambda v: int(max(0, min(255, (v / 255.0 * S - y0) / max(1, (y1 - y0)) * 255))))
    return to_mau(g, toi, sang)


def ap_khoi(nen, m, d, mo, k_sang, k_toi, mau_sang=(255, 235, 200)):
    b = m.filter(ImageFilter.GaussianBlur(mo))
    lech = ImageChops.offset(b, d, d)
    phu(nen, mau_sang, giao(nhan(ImageChops.subtract(b, lech), k_sang), m))
    phu(nen, (0, 0, 0), giao(nhan(ImageChops.subtract(lech, b), k_toi), m))


def vien_toi(nen, m, mo, k):
    trong = m.filter(ImageFilter.GaussianBlur(mo))
    phu(nen, (0, 0, 0), giao(nhan(ImageChops.subtract(m, trong), k), m))


# ---------------------------------------------------------------- hinh dang
CX = 512
BUNG_Y, BUNG_R = 640, 262          # bung tron
CO_X0, CO_X1, CO_Y0, CO_Y1 = CX - 74, CX + 74, 300, 430
MIENG_Y = 300


def ve_binh(loai):
    random.seed(4242 if loai == "mau" else 9191)
    if loai == "mau":
        NUOC_TOI, NUOC_SANG = (70, 0, 2), (235, 28, 16)
        QUANG = (255, 40, 10)
        MAT_SO = (255, 50, 20)
    else:
        NUOC_TOI, NUOC_SANG = (4, 10, 70), (70, 170, 255)
        QUANG = (40, 120, 255)
        MAT_SO = (90, 190, 255)

    anh = Image.new("RGBA", (S, S), (0, 0, 0, 0))

    m_bung = mat_na()
    ImageDraw.Draw(m_bung).ellipse((CX - BUNG_R, BUNG_Y - BUNG_R, CX + BUNG_R, BUNG_Y + BUNG_R), fill=255)
    m_co = mat_na()
    ImageDraw.Draw(m_co).rounded_rectangle((CO_X0, CO_Y0, CO_X1, BUNG_Y - BUNG_R + 60), 20, fill=255)
    m_thuy = ImageChops.lighter(m_bung, m_co)

    # ---- bong do duoi day binh ----
    m_bong = mat_na()
    ImageDraw.Draw(m_bong).ellipse((CX - 250, BUNG_Y + BUNG_R - 40, CX + 250, BUNG_Y + BUNG_R + 40), fill=255)
    phu(anh, (0, 0, 0), nhan(m_bong.filter(ImageFilter.GaussianBlur(26)), 0.8))

    # ---- quang sang quanh binh (chi hien ro tren anh nen trong) ----
    cong_mask = nhan(m_bung.filter(ImageFilter.GaussianBlur(70)), 0.55)
    lop_q = Image.new("RGBA", (S, S), QUANG + (0,))
    lop_q.putalpha(cong_mask)
    anh.alpha_composite(lop_q)

    # ---- thuy tinh: long toi duc, hoi xanh den ----
    kinh = to_mau(nhieu(S // 6, 50, 3), (14, 20, 18), (40, 52, 46))
    phu(anh, kinh, nhan(m_thuy, 0.92))

    # ---- nuoc ben trong: den muc 0,38 tu tren bung ----
    muc = BUNG_Y - BUNG_R * 0.30
    m_nuoc = mat_na()
    dn = ImageDraw.Draw(m_nuoc)
    dn.ellipse((CX - BUNG_R + 22, BUNG_Y - BUNG_R + 22, CX + BUNG_R - 22, BUNG_Y + BUNG_R - 22), fill=255)
    dn.rectangle((0, 0, S, muc), fill=0)
    # mat nuoc hoi cong (elip)
    dn.ellipse((CX - BUNG_R * 0.93, muc - 26, CX + BUNG_R * 0.93, muc + 26), fill=255)

    nuoc = doc_y(NUOC_SANG, NUOC_TOI, muc, BUNG_Y + BUNG_R)
    van = nhieu(S // 5, 70, 4)
    nuoc = Image.blend(nuoc, to_mau(van, NUOC_TOI, NUOC_SANG), 0.35)
    phu(anh, nuoc, m_nuoc)

    if loai == "mana":
        # vong xoay cuon trong nuoc
        m_xoay = mat_na()
        dx = ImageDraw.Draw(m_xoay)
        for k in range(3):
            r0 = 60 + k * 55
            dx.arc((CX - r0, BUNG_Y + 40 - r0 * 0.6, CX + r0, BUNG_Y + 40 + r0 * 0.6), 200 + k * 30, 470 + k * 30,
                   fill=255, width=14)
        cong_sang(anh, (60, 150, 255), giao(nhan(m_xoay.filter(ImageFilter.GaussianBlur(6)), 0.8), m_nuoc))

    # tam nuoc phat sang
    m_loi = mat_na()
    ImageDraw.Draw(m_loi).ellipse((CX - 130, BUNG_Y - 40, CX + 130, BUNG_Y + 170), fill=255)
    cong_sang(anh, QUANG, giao(nhan(m_loi.filter(ImageFilter.GaussianBlur(60)), 0.75), m_nuoc))

    # bot khi
    m_bot = mat_na()
    db = ImageDraw.Draw(m_bot)
    for _ in range(26):
        bx = CX + random.uniform(-170, 170)
        by = random.uniform(muc + 20, BUNG_Y + BUNG_R - 70)
        br = random.uniform(6, 20)
        db.ellipse((bx - br, by - br, bx + br, by + br), outline=255, width=4)
    phu(anh, tuple(min(255, c + 90) for c in NUOC_SANG), giao(nhan(m_bot, 0.8), m_nuoc))

    # vach mat nuoc sang
    m_mat = mat_na()
    ImageDraw.Draw(m_mat).arc((CX - BUNG_R * 0.93, muc - 26, CX + BUNG_R * 0.93, muc + 26), 180, 360, fill=255, width=8)
    phu(anh, tuple(min(255, c + 120) for c in NUOC_SANG), nhan(m_mat.filter(ImageFilter.GaussianBlur(2)), 0.85))

    # ---- mau RI ra ngoai / chu run khac ----
    if loai == "mau":
        m_ri = mat_na()
        dr = ImageDraw.Draw(m_ri)
        for gx, dai, rong in ((CX - 58, 300, 22), (CX + 40, 190, 18), (CX - 10, 110, 14)):
            dr.rounded_rectangle((gx - rong / 2, MIENG_Y - 10, gx + rong / 2, MIENG_Y + dai), rong / 2, fill=255)
            dr.ellipse((gx - rong * 0.85, MIENG_Y + dai - rong, gx + rong * 0.85, MIENG_Y + dai + rong * 0.8), fill=255)
        m_ri = m_ri.filter(ImageFilter.GaussianBlur(2))
        phu(anh, (0, 0, 0), nhan(ImageChops.offset(m_ri, 4, 6).filter(ImageFilter.GaussianBlur(4)), 0.6))
        phu(anh, (150, 6, 4), m_ri)
        ap_khoi(anh, m_ri, 4, 3, 2.4, 1.6, (255, 150, 130))
    else:
        m_run = mat_na()
        du = ImageDraw.Draw(m_run)
        # ngoi sao nam canh nguoc mo tren than binh
        diem = []
        for k in range(5):
            g = math.radians(90 + k * 72)
            diem.append((CX + math.cos(g) * 120, BUNG_Y + 60 + math.sin(g) * 120))
        for k in range(5):
            du.line(diem[k] + diem[(k + 2) % 5], fill=255, width=7)
        du.ellipse((CX - 140, BUNG_Y + 60 - 140, CX + 140, BUNG_Y + 60 + 140), outline=255, width=6)
        cong_sang(anh, (120, 200, 255), nhan(m_run.filter(ImageFilter.GaussianBlur(2)), 0.55))

    # ---- vien thuy tinh + phan chieu ----
    vien_toi(anh, m_thuy, 26, 1.6)
    m_vien = ImageChops.subtract(m_thuy, m_thuy.filter(ImageFilter.MinFilter(15)))
    phu(anh, (60, 70, 62), nhan(m_vien, 0.9))
    m_chieu = mat_na()
    dc = ImageDraw.Draw(m_chieu)
    dc.arc((CX - BUNG_R + 50, BUNG_Y - BUNG_R + 50, CX + BUNG_R - 50, BUNG_Y + BUNG_R - 50), 200, 258, fill=255, width=26)
    dc.rounded_rectangle((CO_X0 + 22, CO_Y0 + 30, CO_X0 + 40, BUNG_Y - BUNG_R + 40), 8, fill=255)
    phu(anh, (255, 255, 255), nhan(m_chieu.filter(ImageFilter.GaussianBlur(5)), 0.75))
    m_dom = mat_na()
    ImageDraw.Draw(m_dom).ellipse((CX - 150, BUNG_Y - 175, CX - 112, BUNG_Y - 137), fill=255)
    phu(anh, (255, 255, 255), m_dom.filter(ImageFilter.GaussianBlur(3)))

    # ---- mieng binh: vanh thuy tinh day ----
    m_mieng = mat_na()
    ImageDraw.Draw(m_mieng).rounded_rectangle((CO_X0 - 20, MIENG_Y - 22, CO_X1 + 20, MIENG_Y + 26), 22, fill=255)
    phu(anh, (0, 0, 0), nhan(ImageChops.offset(m_mieng, 4, 8).filter(ImageFilter.GaussianBlur(6)), 0.7))
    phu(anh, to_mau(nhieu(S // 4, 40, 2), (38, 48, 42), (92, 104, 96)), m_mieng)
    ap_khoi(anh, m_mieng, 4, 3, 2.4, 2.4)

    # ---- dai sat gi quanh co binh + dinh tan ----
    sat = to_mau(nhieu(S // 5, 70, 2), (40, 30, 24), (120, 92, 66))
    m_dai = mat_na()
    ImageDraw.Draw(m_dai).rounded_rectangle((CO_X0 - 16, 408, CO_X1 + 16, 452), 10, fill=255)
    phu(anh, (0, 0, 0), nhan(ImageChops.offset(m_dai, 4, 7).filter(ImageFilter.GaussianBlur(5)), 0.75))
    phu(anh, sat, m_dai)
    phu(anh, (90, 40, 14), giao(nhan(nhieu(S // 12, 90, 4), 0.5), m_dai))
    ap_khoi(anh, m_dai, 3, 2, 2.6, 2.6)
    for dx in (-60, 0, 60):
        md = mat_na()
        ImageDraw.Draw(md).ellipse((CX + dx - 9, 421, CX + dx + 9, 439), fill=255)
        phu(anh, (180, 156, 120), md)
        ap_khoi(anh, md, 2, 2, 3, 3)

    # ---- nut bit: cai SO nho bang xuong ----
    SY = 205
    xuong = to_mau(nhieu(S // 8, 50, 2), (140, 124, 96), (222, 208, 176))
    m_so = mat_na()
    ds = ImageDraw.Draw(m_so)
    ds.ellipse((CX - 92, SY - 100, CX + 92, SY + 62), fill=255)
    ds.rounded_rectangle((CX - 58, SY + 20, CX + 58, SY + 108), 22, fill=255)
    phu(anh, (0, 0, 0), nhan(ImageChops.offset(m_so, 6, 10).filter(ImageFilter.GaussianBlur(9)), 0.8))
    phu(anh, xuong, m_so)
    vien_toi(anh, m_so, 20, 1.3)
    ap_khoi(anh, m_so, 5, 4, 1.4, 2.2)
    m_hoc = mat_na()
    dh = ImageDraw.Draw(m_hoc)
    for dau in (-1, 1):
        pts = [(-64, -6), (-14, 12), (-12, 40), (-44, 44), (-66, 20)]
        dh.polygon([(CX + dau * x, SY + y) for x, y in pts], fill=255)
    dh.polygon([(CX, SY + 54), (CX - 12, SY + 80), (CX + 12, SY + 80)], fill=255)
    dh.rectangle((CX - 40, SY + 90, CX + 40, SY + 98), fill=255)
    phu(anh, (10, 4, 4), m_hoc.filter(ImageFilter.GaussianBlur(1.2)))
    m_rang = mat_na()
    for k in range(-2, 3):
        ImageDraw.Draw(m_rang).line((CX + k * 14, SY + 84, CX + k * 14, SY + 106), fill=255, width=3)
    phu(anh, (40, 28, 18), m_rang)
    m_mat = mat_na()
    for dau in (-1, 1):
        ex, ey = CX + dau * 40, SY + 24
        ImageDraw.Draw(m_mat).ellipse((ex - 11, ey - 8, ex + 11, ey + 8), fill=255)
    cong_sang(anh, MAT_SO, nhan(m_mat.filter(ImageFilter.GaussianBlur(20)), 0.9))
    phu(anh, MAT_SO, m_mat)

    return anh


def luu(loai, ten):
    anh = ve_binh(loai)
    hop = anh.split()[3].point(lambda v: 255 if v > 10 else 0).getbbox()
    cx, cy = (hop[0] + hop[2]) / 2.0, (hop[1] + hop[3]) / 2.0
    nua = max(hop[2] - hop[0], hop[3] - hop[1]) / 2.0 + 10
    cat = anh.crop((int(cx - nua), int(cy - nua), int(cx + nua), int(cy + nua))).resize((256, 256), Image.LANCZOS)

    duong_vp = os.path.join(GOC, "Assets", "Resources", "VatPham")
    os.makedirs(duong_vp, exist_ok=True)
    cat.save(os.path.join(duong_vp, ten + ".png"))

    # Anh ky nang: NEN DEN, thu nho 82% de ruot khong cham vanh dia nut
    icon = Image.new("RGB", (256, 256), (0, 0, 0))
    nho = cat.resize((210, 210), Image.LANCZOS)
    den = Image.new("RGBA", (256, 256), (0, 0, 0, 255))
    den.alpha_composite(nho, (23, 23))
    icon.paste(den.convert("RGB"))
    icon.save(os.path.join(GOC, "Assets", "Resources", "Icons", ten + ".png"))
    print("ghi", ten)


luu("mau", "BinhMau")
luu("mana", "BinhMana")
