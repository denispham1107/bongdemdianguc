# ANH DUNG NHAM DUOI VUC + ANH HUONG GAN (05/10/2026). Chay TRONG Blender qua MCP (numpy cua Blender), dang bpy.app.timers vi tinh
# khoang cach Voronoi mat ~3,5 phut (lenh MCP truc tiep het gio):
#   exec(open(r"<du an>\CongCu\Blender\dung_nham_gan.py", encoding="utf-8").read())
# Ra: Assets/Resources/DiaNguc/DungNham.png (1024, mau theo do nong) + HuongGan.png (512: RG huong tiep tuyen gan goc kep, B nhieu).
# Moi thu LAP LIEN MACH: diem Voronoi cuon vong (anh nho nhat), nhieu dai tan bang FFT (tu tuan hoan).
# Nguoi dung: "cac duong dung nham con thang va tron qua" -> chon (1) gan lom chom / do rong doi / nhanh phu / vung nong chay,
# (2) mau theo do nong, (4) bo lop gan thu hai (shader).
import bpy, numpy as np, time, os

DU_AN = r"C:\Users\HP\Documents\GameUnity\Diablo25D"
RA = os.path.join(DU_AN, r"Assets\Resources\DiaNguc")
R = 1024

fy = np.fft.fftfreq(R)[:, None] * R; fx = np.fft.fftfreq(R)[None, :] * R
fr = np.sqrt(fx ** 2 + fy ** 2)

def nhieu(f, bw, seed, n=R):
    ffy = np.fft.fftfreq(n)[:, None] * n; ffx = np.fft.fftfreq(n)[None, :] * n; ff = np.sqrt(ffx ** 2 + ffy ** 2)
    g = np.random.default_rng(seed).standard_normal((n, n))
    z = np.real(np.fft.ifft2(np.fft.fft2(g) * np.exp(-((ff - f) / bw) ** 2)))
    return ((z - z.mean()) / z.std()).astype(np.float32)

def ss(a, b, x):
    t = np.clip((x - a) / (b - a), 0, 1); return t * t * (3 - 2 * t)

sig = lambda x: 1 / (1 + np.exp(-x))

def hat_luoi(n, jit, seed):
    r = np.random.default_rng(seed)
    return ((np.mgrid[0:n, 0:n].reshape(2, -1).T + 0.5 + (r.random((n * n, 2)) - 0.5) * jit) / n).astype(np.float32)

def canh(P, X, Y):
    """Khoang cach CHINH XAC toi canh Voronoi gan nhat (hai diem gan nhat, cuon vong)."""
    out = np.empty((R, R), np.float32)
    for a in range(0, R, 64):
        px = X[a:a + 64, :, None]; py = Y[a:a + 64, :, None]
        dx = (px - P[None, None, :, 1] + 0.5) % 1.0 - 0.5; dy = (py - P[None, None, :, 0] + 0.5) % 1.0 - 0.5
        d2 = dx * dx + dy * dy
        idx = np.argpartition(d2, 2, axis=2)[:, :, :2]
        o = np.argsort(np.take_along_axis(d2, idx, 2), axis=2); idx = np.take_along_axis(idx, o, 2)
        r1x = np.take_along_axis(dx, idx[:, :, :1], 2)[..., 0]; r1y = np.take_along_axis(dy, idx[:, :, :1], 2)[..., 0]
        r2x = np.take_along_axis(dx, idx[:, :, 1:2], 2)[..., 0]; r2y = np.take_along_axis(dy, idx[:, :, 1:2], 2)[..., 0]
        out[a:a + 64] = ((r2x ** 2 + r2y ** 2) - (r1x ** 2 + r1y ** 2)) / (2 * np.sqrt((r2x - r1x) ** 2 + (r2y - r1y) ** 2) + 1e-9)
    return out

def luu(arr, path):
    n = arr.shape[0]
    im = bpy.data.images.new("DN_Tam", n, n, alpha=True, float_buffer=False); im.colorspace_settings.name = 'Non-Color'
    im.pixels[:] = arr.ravel(); im.filepath_raw = path; im.file_format = 'PNG'; im.save(); bpy.data.images.remove(im)

def viec():
    t0 = time.time()
    yy, xx = np.mgrid[0:R, 0:R].astype(np.float32) / R
    # BE MEO nhieu tang: uon lon (van giu hinh tang vo) + rang cua vua + rang cua nho. Ban dau 0,022 / 0,006: mat hinh mang vo, gan xoan nhu giun.
    X = xx + 0.011 * nhieu(3.5, 1.5, 1) + 0.0035 * nhieu(18, 6, 2) + 0.0016 * nhieu(60, 20, 3)
    Y = yy + 0.011 * nhieu(3.5, 1.5, 4) + 0.0035 * nhieu(18, 6, 5) + 0.0016 * nhieu(60, 20, 6)
    dChinh = canh(hat_luoi(12, 0.85, 11), X, Y)                                                   # khe chinh (~3,8 m moi tang)
    dNhanh = canh(hat_luoi(26, 0.9, 12), X + 0.0025 * nhieu(30, 10, 7), Y + 0.0025 * nhieu(30, 10, 8))   # nhanh nut phu
    dVun = canh(hat_luoi(55, 0.9, 13), X + 0.0015 * nhieu(80, 25, 9), Y + 0.0015 * nhieu(80, 25, 10))    # vet vun tren vo

    # DO RONG GAN doi doc duong: phinh / that / doan nguoi (toi, hep)
    nW = 0.6 * nhieu(7, 3, 21) + 0.4 * nhieu(16, 6, 22)
    w = 0.0036 * (0.25 + 1.6 * sig(1.6 * nW)); w = w * np.where(nW < -1.1, 0.35, 1.0)
    loi = np.exp(-(dChinh / w) ** 2) * (0.8 + 0.2 * nhieu(25, 10, 23))
    quang = 0.17 * np.exp(-dChinh / (1.5 * w + 0.002))                                    # do loang vao mep vo
    mNhanh = ss(0.2, 1.0, nhieu(9, 4, 24)) * np.exp(-dChinh / 0.02)                        # nhanh chi o vai cho, gan khe chinh
    wN = 0.0013 * (0.4 + 1.2 * sig(1.5 * nhieu(20, 8, 25)))
    nhanh = 0.7 * np.exp(-(dNhanh / wN) ** 2) * mNhanh + 0.08 * np.exp(-dNhanh / (3 * wN + 0.002)) * mNhanh
    nV = 0.85 * nhieu(5, 2, 26) + 0.15 * nhieu(30, 12, 27) + 1.0 * np.exp(-dChinh / 0.005) - 0.3
    vung = ss(1.15, 1.5, nV) * (0.86 + 0.14 * nhieu(10, 4, 28))                           # vung nong chay (mat min: tan so cao ra lom dom cham)
    vo = 0.045 + 0.02 * nhieu(35, 14, 29) + 0.012 * nhieu(90, 30, 30) + 0.03 * np.exp(-dChinh / 0.008)
    vo += 0.045 * np.exp(-(dVun / 0.0011) ** 2) * ss(0.0, 0.8, nhieu(12, 5, 31))
    H = np.maximum.reduce([vo, loi, nhanh, vung]); H = np.clip(H + quang * (1 - H), 0, 1)

    # BANG MAU THEO DO NONG: vo nau den -> do sam -> cam -> vang -> trang vang
    moc = np.array([0.0, 0.10, 0.30, 0.55, 0.80, 1.0])
    mau = np.array([[0.035, 0.018, 0.015], [0.16, 0.030, 0.012], [0.55, 0.07, 0.010], [0.95, 0.33, 0.03], [1.0, 0.66, 0.16], [1.0, 0.93, 0.68]])
    C = np.stack([np.interp(H, moc, mau[:, k]) for k in range(3)], -1).astype(np.float32)
    luu(np.concatenate([C, np.ones((R, R, 1), np.float32)], -1), os.path.join(RA, "DungNham.png"))

    # ANH HUONG GAN: tensor cau truc cua do sang -> huong TIEP TUYEN dang goc kep x do ket hop (RG), nhieu dai tan (B)
    L = C[..., 0] * 0.3 + C[..., 1] * 0.5 + C[..., 2] * 0.2
    def mo(a, s):
        return np.real(np.fft.ifft2(np.fft.fft2(a) * np.exp(-2 * (np.pi ** 2) * (s ** 2) * ((fx / R) ** 2 + (fy / R) ** 2))))
    Lm = mo(L, 1.5)
    gx = (np.roll(Lm, -1, 1) - np.roll(Lm, 1, 1)) * 0.5; gy = (np.roll(Lm, -1, 0) - np.roll(Lm, 1, 0)) * 0.5
    Jxx = mo(gx * gx, 6); Jyy = mo(gy * gy, 6); Jxy = mo(gx * gy, 6)
    d = Jxx - Jyy; r = np.sqrt(d * d + 4 * Jxy * Jxy); coh = r / (Jxx + Jyy + 1e-9)
    vx = (-d / (r + 1e-12)) * coh; vy = (-2 * Jxy / (r + 1e-12)) * coh
    nho = lambda a: a.reshape(R // 2, 2, R // 2, 2).mean(axis=(1, 3))
    n = np.random.default_rng(111).standard_normal((R // 2, R // 2))
    f2y = np.fft.fftfreq(R // 2)[:, None] * (R // 2); f2x = np.fft.fftfreq(R // 2)[None, :] * (R // 2); f2 = np.sqrt(f2x ** 2 + f2y ** 2)
    nz = np.real(np.fft.ifft2(np.fft.fft2(n) * np.exp(-((f2 - 19) / 7.0) ** 2))); nz = np.clip(0.5 + (nz - nz.mean()) / nz.std() * 0.22, 0, 1)
    out = np.zeros((R // 2, R // 2, 4), np.float32)
    out[..., 0] = nho(vx) * 0.5 + 0.5; out[..., 1] = nho(vy) * 0.5 + 0.5; out[..., 2] = nz; out[..., 3] = 1
    luu(out, os.path.join(RA, "HuongGan.png"))
    print("[dung_nham_gan] xong", round(time.time() - t0, 1), "s")
    return None

bpy.app.timers.register(viec, first_interval=0.2)
