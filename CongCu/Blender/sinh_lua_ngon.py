# -*- coding: utf-8 -*-
# Sinh flipbook NGON LUA 8x8 (1024x1024) bang nhieu Perlin cuon.
# Moi o = mot thoi diem trong doi MOT luoi lua: nhen -> vuon cao lac lu ->
# goc rut len, ngon dut ra -> tan. Mau theo nhiet do tung diem (vat den).
import numpy as np
import sys, os
import bpy

argv = sys.argv[sys.argv.index('--') + 1:] if '--' in sys.argv else []
ra = argv[0] if argv else 'LuaNgon.png'
COT, HANG, O = 8, 8, 128
rng = np.random.RandomState(1666)
perm = np.arange(256); rng.shuffle(perm); perm = np.concatenate([perm, perm])
grad3 = np.array([[1,1,0],[-1,1,0],[1,-1,0],[-1,-1,0],[1,0,1],[-1,0,1],[1,0,-1],[-1,0,-1],
                  [0,1,1],[0,-1,1],[0,1,-1],[0,-1,-1]], dtype=np.float64)

def fade(t): return t * t * t * (t * (t * 6 - 15) + 10)

def perlin3(x, y, z):
    xi = np.floor(x).astype(int); yi = np.floor(y).astype(int); zi = np.floor(z).astype(int)
    xf = x - xi; yf = y - yi; zf = z - zi
    xi &= 255; yi &= 255; zi &= 255
    u, v, w = fade(xf), fade(yf), fade(zf)
    def g(ix, iy, iz, dx, dy, dz):
        h = perm[perm[perm[ix] + iy] + iz] % 12
        gg = grad3[h]
        return gg[..., 0] * dx + gg[..., 1] * dy + gg[..., 2] * dz
    n000 = g(xi, yi, zi, xf, yf, zf)
    n100 = g(xi + 1, yi, zi, xf - 1, yf, zf)
    n010 = g(xi, yi + 1, zi, xf, yf - 1, zf)
    n110 = g(xi + 1, yi + 1, zi, xf - 1, yf - 1, zf)
    n001 = g(xi, yi, zi + 1, xf, yf, zf - 1)
    n101 = g(xi + 1, yi, zi + 1, xf - 1, yf, zf - 1)
    n011 = g(xi, yi + 1, zi + 1, xf, yf - 1, zf - 1)
    n111 = g(xi + 1, yi + 1, zi + 1, xf - 1, yf - 1, zf - 1)
    x00 = n000 + u * (n100 - n000); x10 = n010 + u * (n110 - n010)
    x01 = n001 + u * (n101 - n001); x11 = n011 + u * (n111 - n011)
    y0 = x00 + v * (x10 - x00); y1 = x01 + v * (x11 - x01)
    return y0 + w * (y1 - y0)

def fbm(x, y, z, oct=5):
    s = 0.0; a = 0.5; f = 1.0
    for i in range(oct):
        s = s + a * perlin3(x * f + i * 17.3, y * f - i * 9.1, z * f + i * 3.7)
        f *= 2.03; a *= 0.5
    return s  # ~[-0.7, 0.7]

def ss(a, b, t):
    t = np.clip((t - a) / (b - a), 0, 1)
    return t * t * (3 - 2 * t)

# Luoi toa do trong mot o: x [-1,1], y [0,1] tu duoi len
px = (np.arange(O) + 0.5) / O
X, Y = np.meshgrid(px * 2 - 1, 1 - px)      # hang 0 cua anh = tren cung

# Bang mau vat den: nhiet do -> mau
diem = np.array([0.00, 0.12, 0.30, 0.52, 0.75, 1.00, 1.30])
mau = np.array([[0.00, 0.00, 0.00],
                [0.35, 0.03, 0.00],
                [0.90, 0.18, 0.02],
                [1.00, 0.45, 0.06],
                [1.00, 0.72, 0.22],
                [1.00, 0.92, 0.55],
                [1.00, 0.98, 0.85]])

def to_mau(T):
    T = np.clip(T, 0, 1.3)
    r = np.interp(T, diem, mau[:, 0]); g = np.interp(T, diem, mau[:, 1]); b = np.interp(T, diem, mau[:, 2])
    return np.stack([r, g, b], -1)

anh = np.zeros((HANG * O, COT * O, 4), dtype=np.float64)
tong = COT * HANG
for f in range(tong):
    u = f / (tong - 1)
    # Chieu cao luoi lua, goc rut len (ngon dut khoi goc), do sang tong
    h = 0.45 + 0.50 * ss(0.0, 0.30, u)
    yb = 0.55 * ss(0.50, 1.0, u)
    sang = ss(0.0, 0.08, u) * (1 - ss(0.70, 1.0, u))

    yy = np.clip((Y - yb) / max(h - yb, 0.05), 0, 1.4)
    dong = Y - u * 1.9                                        # dong khi boc len
    n = fbm(X * 2.2, dong * 2.4, u * 1.5)
    n2 = fbm(X * 5.5 + 5.1, dong * 6.0, u * 2.4 + 3.3, oct=4)
    n3 = fbm(X * 1.3 + 11.0, dong * 1.1, u * 0.9 + 8.0, oct=2)

    # Truc lac lu, lac manh o tren
    xc = 0.16 * n3 * (0.25 + yy)
    # Be rong: day rong va tron, thu dan len dinh
    w = 0.66 * np.power(np.clip(1 - np.power(yy, 1.6), 0, 1), 0.65) * (0.8 + 0.2 * ss(0.0, 0.18, yy)) + 0.015
    xw = X - xc + 0.34 * n * (0.15 + yy)                      # mep bi xe theo nhieu
    d = np.abs(xw) / w

    # Than lua TACH thanh 2-3 luoi o nua tren: van song doc lech pha theo nhieu
    luoi = 0.5 + 0.5 * np.cos((xw * 2.6 + 1.6 * n2) * np.pi)
    tach = ss(0.25, 0.85, yy)
    D = (1 - d) * (1 - tach * (1 - luoi) * 0.95)
    # Soi mau o mep tren: nhieu nho khoet lo, cang len cao cang manh
    D = D + 0.45 * n2 * (0.15 + yy) + 0.25 * n * yy
    D = D * ss(0.0, 0.07, Y - yb + 0.03)                      # day tron, khong cat ngang
    D = D * (1 - ss(0.80, 1.12, yy))                          # tat han o dinh, khong gai nhon
    D = np.clip(D, 0, None)

    T = D * (1.35 - 0.95 * np.clip(yy, 0, 1)) * sang
    rgb = to_mau(T)
    a = ss(0.06, 0.42, T)
    # Vien o phai trong han de khung khong ro net vuong
    vien = ss(0.0, 0.08, 1 - np.abs(X)) * ss(0.0, 0.06, 1 - Y) * ss(0.0, 0.02, Y)
    a = a * vien

    r0 = (f // COT) * O; c0 = (f % COT) * O
    anh[r0:r0 + O, c0:c0 + O, :3] = rgb
    anh[r0:r0 + O, c0:c0 + O, 3] = a

# Lam mem nhe (tach duoc: 3 diem ngang roi 3 diem doc)
k = np.array([0.25, 0.5, 0.25])
for c in range(4):
    kenh = anh[..., c]
    kenh = k[0] * np.roll(kenh, 1, 1) + k[1] * kenh + k[2] * np.roll(kenh, -1, 1)
    kenh = k[0] * np.roll(kenh, 1, 0) + k[1] * kenh + k[2] * np.roll(kenh, -1, 0)
    anh[..., c] = kenh
anh = np.clip(anh, 0, 1)

# Ghi PNG bang Blender: pixel cua Blender dem tu DUOI len, nen lat doc
H, W = anh.shape[:2]
im = bpy.data.images.new('LuaNgon', W, H, alpha=True)
im.colorspace_settings.name = 'sRGB'
im.alpha_mode = 'STRAIGHT'
im.pixels.foreach_set(np.flipud(anh).astype(np.float32).ravel())
im.filepath_raw = ra
im.file_format = 'PNG'
im.save()

A = anh
dt = []
for f in range(tong):
    r0 = (f // COT) * O; c0 = (f % COT) * O
    o = A[r0:r0 + O, c0:c0 + O]
    dt.append((o[..., 3] > 0.1).mean())
m = A[..., 3] > 0.3
print('DA GHI', ra, W, H)
print('DIEN TICH lua theo khung (%%): dau %.0f, 1/4 %.0f, giua %.0f, 3/4 %.0f, cuoi %.0f' %
      (dt[0] * 100, dt[16] * 100, dt[32] * 100, dt[48] * 100, dt[63] * 100))
print('MAU trung binh cho alpha>0.3: R %.0f G %.0f B %.0f' % tuple(A[m][:, :3].mean(0) * 255))
