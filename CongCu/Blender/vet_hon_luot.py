# VET HON XANH cua cu luot Bo xuong (nguoi dung 01/11/2026, chon "bong mo + vet hon xanh").
# Chay TRONG Blender qua MCP: exec(open(duong_dan).read()); ve("<thu_muc Resources/KyNang/BoXuong>")
# Anh 512x128 lat theo TrailRenderer: u = 0 o DAU vet (sau lung con quai), u = 1 o DUOI; xam tren alpha de to mau luc chay.
# Nhieu keo dai theo u (si hon chay doc vet), lap lien mach theo v khong can - mep mo ve 0.
import bpy
import numpy as np


def nhieu_keo(w, h, rng, tan_u, tan_v):
    """Nhieu loc tan so: tan_u thap = keo dai doc vet."""
    f = np.fft.fft2(rng.standard_normal((h, w)))
    fy = np.fft.fftfreq(h)[:, None]
    fx = np.fft.fftfreq(w)[None, :]
    loc = np.exp(-(fx / tan_u) ** 2 - (fy / tan_v) ** 2)
    n = np.fft.ifft2(f * loc).real
    n -= n.min()
    n /= max(1e-6, n.max())
    return n


def ve(thu_muc, w=512, h=128, seed=1107):
    rng = np.random.default_rng(seed)
    u = np.linspace(0.0, 1.0, w)[None, :]
    v = np.linspace(-1.0, 1.0, h)[:, None]

    # Si hon: 3 tang, keo dai doc u
    si = 0.55 * nhieu_keo(w, h, rng, 0.012, 0.10) + 0.30 * nhieu_keo(w, h, rng, 0.03, 0.22) + 0.15 * nhieu_keo(w, h, rng, 0.08, 0.40)
    si = np.clip((si - 0.38) / 0.34, 0.0, 1.0)

    # Be ngang: loi mem + mep rach (be ngang doi theo u bang nhieu), thu hep ve duoi
    lech = (nhieu_keo(w, 1, rng, 0.03, 1.0)[0][None, :] - 0.5) * 0.25
    rong = 0.62 * (1.0 - 0.55 * u) + 0.08
    d = np.abs(v - lech) / rong
    than = np.exp(-(d ** 2) * 2.2)
    loi = np.exp(-(d ** 2) * 22.0)

    # Doc vet: sang o dau, tan o duoi, vao mem o u = 0 (khong co mep cat)
    doc = np.clip(u / 0.04, 0.0, 1.0) * (1.0 - u) ** 1.25

    a = (than * (0.30 + 0.70 * si) + loi * 0.55) * doc
    a = np.clip(a * 1.25, 0.0, 1.0)

    px = np.zeros((h, w, 4), dtype=np.float32)
    px[..., 0] = px[..., 1] = px[..., 2] = 1.0
    px[..., 3] = a.astype(np.float32)

    ten = "VetHon_BoXuong"
    im = bpy.data.images.get(ten)
    if im is not None:
        bpy.data.images.remove(im)
    im = bpy.data.images.new(ten, w, h, alpha=True, float_buffer=False)
    im.colorspace_settings.name = "sRGB"
    im.alpha_mode = "STRAIGHT"
    im.pixels.foreach_set(px[::-1].ravel())
    im.filepath_raw = thu_muc.rstrip("/\\") + "/VetHon.png"
    im.file_format = "PNG"
    im.save()
    return {"tb_alpha": float(a.mean()), "max_alpha": float(a.max()), "duong_dan": im.filepath_raw}
