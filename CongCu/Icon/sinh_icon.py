# -*- coding: utf-8 -*-
"""
SINH BO ICON CHO BAN CAI DAT (PWA) TU MOT ANH GOC.

Nguoi dung gui anh dau ac quy (360x360, nen trong) va xin: tren iOS them vao
man hinh chinh, tren Android / may tinh cai dat duoc nhu mot ung dung.

Ba kieu icon, va moi kieu mot luat rieng:

  * icon-<co>.png      - nen TOI dac. iOS va Windows khong chap nhan nen trong:
                         chung ghep len nen trang, anh do tren trang thi nhat
                         va vien den bi chim.
  * maskable-<co>.png  - Android CAT icon theo hinh cua may (tron, vuong bo
                         goc, giot nuoc). Phan chac chan khong bi cat chi la
                         vong tron duong kinh 80% - nen hinh phai thu con 68%
                         va dat giua, phan con lai la nen.
  * apple-touch-icon   - 180x180, nen dac, khong bo goc (iOS tu bo goc).

Chay:  python CongCu/Icon/sinh_icon.py
Ra:    web/icons/*.png  +  web/favicon.png
"""
import io, os
from PIL import Image

GOC = os.path.join(os.path.dirname(__file__), "acquy_goc.png")
RA = os.path.join(os.path.dirname(__file__), "..", "..", "web", "icons")
RA = os.path.normpath(RA)

# Nen toi cung tong voi giao dien game (gan den, am sac do)
NEN = (11, 8, 8, 255)

def nap():
    im = Image.open(GOC).convert("RGBA")
    bb = im.getbbox()          # cat sat vien de hinh khong bi lech
    return im.crop(bb)

def dat_vao(nen_co, hinh, ti_le_hinh):
    """Dat hinh vao giua mot o vuong nen dac, chiem ti_le_hinh be ngang."""
    o = Image.new("RGBA", (nen_co, nen_co), NEN)
    r = int(nen_co * ti_le_hinh)
    w, h = hinh.size
    k = min(r / w, r / h)
    moi = hinh.resize((max(1, int(w * k)), max(1, int(h * k))), Image.LANCZOS)
    o.paste(moi, ((nen_co - moi.size[0]) // 2, (nen_co - moi.size[1]) // 2), moi)
    return o

def ghi(im, ten):
    d = os.path.join(RA, ten)
    im.save(d, "PNG", optimize=True)
    print("%-28s %sx%s  %.1f KB" % (ten, im.size[0], im.size[1], os.path.getsize(d) / 1024.0))

def main():
    os.makedirs(RA, exist_ok=True)
    hinh = nap()
    print("anh goc (da cat vien):", hinh.size)

    # Icon thuong: hinh chiem 92% - de vien nho cho de nhin
    for co in (192, 512, 1024):
        ghi(dat_vao(co, hinh, 0.92), "icon-%d.png" % co)

    # Maskable: hinh chi 68% - Android cat mat vien
    for co in (192, 512):
        ghi(dat_vao(co, hinh, 0.68), "maskable-%d.png" % co)

    ghi(dat_vao(180, hinh, 0.90), "apple-touch-icon.png")
    ghi(dat_vao(32, hinh, 1.0), "favicon-32.png")
    ghi(dat_vao(16, hinh, 1.0), "favicon-16.png")

    # Anh chia se (Open Graph) - khung ngang 1200x630
    og = Image.new("RGBA", (1200, 630), NEN)
    k = 560.0 / max(hinh.size)
    nho = hinh.resize((int(hinh.size[0] * k), int(hinh.size[1] * k)), Image.LANCZOS)
    og.paste(nho, ((1200 - nho.size[0]) // 2, (630 - nho.size[1]) // 2), nho)
    ghi(og, "anh-chia-se.png")

if __name__ == "__main__":
    main()
