# -*- coding: utf-8 -*-
# LO LUA BANG DA - dung trong Blender (chay nen: blender -b --factory-startup -P lo_lua_da.py -- <thu_muc_ra>)
#
# 1. Luoi CHI TIET: chan de bat giac, tru da, dau tru, chau da - voxel remesh
#    roi duc bang nhieu (san sui, loi lom nhu da dieo tay).
# 2. Vat lieu nguon: da xam nau + reu o chan + muoi gan mieng lua, vet nut
#    (Voronoi khoang cach toi canh) va MAU dong trong nut roi chay thanh vet
#    xuong duoi.
# 3. Luoi GON cho game (~7000 mat): decimate tu luoi chi tiet, UV tu dong.
# 4. Nuong tu chi tiet -> gon: mau, phap tuyen, do nham, AO.
# 5. Xuat FBX + PNG.
import bpy, bmesh, math, random, sys, os, time
import numpy as np
from mathutils import Vector, noise

argv = sys.argv[sys.argv.index('--') + 1:] if '--' in sys.argv else []
RA = argv[0] if argv else os.path.abspath('lolua_ra')
KT = int(argv[1]) if len(argv) > 1 else 1024
os.makedirs(RA, exist_ok=True)
t0 = time.time()
def log(*a):
    print('[LOLUA %.0fs]' % (time.time() - t0), *a, flush=True)

bpy.ops.wm.read_factory_settings(use_empty=True)
sc = bpy.context.scene
sc.render.engine = 'CYCLES'
sc.cycles.device = 'CPU'
random.seed(7)

# ------------------------------------------------------------------ luoi quay
def lathe(ten, profile, segs, xoay=0.0):
    """Quay mot bien dang (r, z) quanh truc Z. Diem r=0 thanh dinh tam."""
    bm = bmesh.new()
    vong = []
    for (r, z) in profile:
        if r < 1e-6:
            vong.append([bm.verts.new((0, 0, z))])
        else:
            v = []
            for i in range(segs):
                a = xoay + 2 * math.pi * i / segs
                v.append(bm.verts.new((r * math.cos(a), r * math.sin(a), z)))
            vong.append(v)
    for k in range(len(vong) - 1):
        A, B = vong[k], vong[k + 1]
        if len(A) == 1 and len(B) == 1:
            continue
        for i in range(segs):
            j = (i + 1) % segs
            if len(A) == 1:
                bm.faces.new((A[0], B[i], B[j]))
            elif len(B) == 1:
                bm.faces.new((A[i], B[0], A[j]))
            else:
                bm.faces.new((A[i], B[i], B[j], A[j]))
    bmesh.ops.recalc_face_normals(bm, faces=bm.faces)
    me = bpy.data.meshes.new(ten)
    bm.to_mesh(me); bm.free()
    ob = bpy.data.objects.new(ten, me)
    sc.collection.objects.link(ob)
    return ob

def chon(obs, active=None):
    bpy.ops.object.select_all(action='DESELECT')
    for o in obs:
        o.select_set(True)
    bpy.context.view_layer.objects.active = active or obs[0]

def ap_modifier(ob, m):
    chon([ob])
    bpy.ops.object.modifier_apply(modifier=m.name)

# Chan de + tru + dau tru: bat giac (8 canh), nhu da deo
chan = lathe('ChanDe', [
    (0.0, 0.00), (0.47, 0.00), (0.47, 0.12), (0.43, 0.16), (0.35, 0.16), (0.35, 0.24),
    (0.31, 0.27), (0.23, 0.28), (0.205, 0.34), (0.19, 0.80), (0.215, 0.84), (0.285, 0.87),
    (0.305, 0.93), (0.27, 0.965), (0.0, 0.965)], 8, math.pi / 8)
# Chau da: tron, thanh day, long chau sau
chau = lathe('Chau', [
    (0.0, 0.93), (0.22, 0.93), (0.34, 1.00), (0.46, 1.12), (0.53, 1.24), (0.545, 1.29),
    (0.52, 1.315), (0.46, 1.315), (0.435, 1.27), (0.37, 1.18), (0.25, 1.11), (0.0, 1.09)], 28)
log('dung hinh xong')

# ------------------------------------------------------------------ chi tiet
chi_tiet = []
for ob, vx in ((chan, 0.0075), (chau, 0.0070)):
    m = ob.modifiers.new('rm', 'REMESH'); m.mode = 'VOXEL'; m.voxel_size = vx
    ap_modifier(ob, m)
    chi_tiet.append(ob)
cao = chan
chon([chan, chau], chan); bpy.ops.object.join()
cao.name = 'LoLua_ChiTiet'
log('remesh xong, dinh =', len(cao.data.vertices))

def ss(a, b, t):
    t = max(0.0, min(1.0, (t - a) / (b - a)))
    return t * t * (3 - 2 * t)

# ---- Nhieu tinh bang numpy (ca khoi mot lan). Ban dau goi mathutils.noise
# tung dinh: 140 nghin dinh mat 1403 giay. ----
rng = np.random.RandomState(1666)
perm = np.arange(256); rng.shuffle(perm); perm = np.concatenate([perm, perm])
grad3 = np.array([[1,1,0],[-1,1,0],[1,-1,0],[-1,-1,0],[1,0,1],[-1,0,1],[1,0,-1],[-1,0,-1],
                  [0,1,1],[0,-1,1],[0,1,-1],[0,-1,-1]], dtype=np.float64)
def fade(t): return t * t * t * (t * (t * 6 - 15) + 10)
def perlin(P):
    x, y, z = P[:, 0], P[:, 1], P[:, 2]
    xi = np.floor(x).astype(int); yi = np.floor(y).astype(int); zi = np.floor(z).astype(int)
    xf = x - xi; yf = y - yi; zf = z - zi
    xi &= 255; yi &= 255; zi &= 255
    u, v, w = fade(xf), fade(yf), fade(zf)
    def g(ix, iy, iz, dx, dy, dz):
        gg = grad3[perm[perm[perm[ix] + iy] + iz] % 12]
        return gg[:, 0] * dx + gg[:, 1] * dy + gg[:, 2] * dz
    n000 = g(xi, yi, zi, xf, yf, zf); n100 = g(xi + 1, yi, zi, xf - 1, yf, zf)
    n010 = g(xi, yi + 1, zi, xf, yf - 1, zf); n110 = g(xi + 1, yi + 1, zi, xf - 1, yf - 1, zf)
    n001 = g(xi, yi, zi + 1, xf, yf, zf - 1); n101 = g(xi + 1, yi, zi + 1, xf - 1, yf, zf - 1)
    n011 = g(xi, yi + 1, zi + 1, xf, yf - 1, zf - 1); n111 = g(xi + 1, yi + 1, zi + 1, xf - 1, yf - 1, zf - 1)
    x00 = n000 + u * (n100 - n000); x10 = n010 + u * (n110 - n010)
    x01 = n001 + u * (n101 - n001); x11 = n011 + u * (n111 - n011)
    y0 = x00 + v * (x10 - x00); y1 = x01 + v * (x11 - x01)
    return y0 + w * (y1 - y0)
def fbm(P, oct=4):
    s = 0.0; a = 0.5; f = 1.0
    for i in range(oct):
        s = s + a * perlin(P * f + np.array([i * 17.3, -i * 9.1, i * 3.7])); f *= 2.0; a *= 0.55
    return s
def hash01(c, k):
    h = (c[:, 0] * (73856093 + k * 7) ^ c[:, 1] * (19349663 + k * 13) ^ c[:, 2] * (83492791 + k * 29)) & 0x7fffffff
    h = (h * 1103515245 + 12345) & 0x7fffffff
    return (h % 100000) / 100000.0
def cellF1(P):
    base = np.floor(P).astype(np.int64); best = np.full(len(P), 9.0)
    for dx in (-1, 0, 1):
        for dy in (-1, 0, 1):
            for dz in (-1, 0, 1):
                c = base + np.array([dx, dy, dz])
                off = np.stack([hash01(c, 1), hash01(c, 2), hash01(c, 3)], 1)
                best = np.minimum(best, np.linalg.norm(c + off - P, axis=1))
    return best

me = cao.data
n = len(me.vertices)
co = np.empty(n * 3); me.vertices.foreach_get('co', co); co = co.reshape(-1, 3)
no = np.empty(n * 3); me.vertices.foreach_get('normal', no); no = no.reshape(-1, 3)
S1 = np.array([13.1, 7.7, 2.9]); S2 = np.array([-4.2, 9.5, 21.3])
d = 0.013 * fbm(co * 2.4 + S1, 4)                       # u lon: khoi da khong deu
d += 0.010 * (cellF1(co * 7.0 + S2) - 0.45)              # mat da deo: o loi lom
d += 0.0026 * perlin(co * 38.0 + S1)                     # hat san sui
z = co[:, 2]
vanh = np.clip((z - 1.25) / 0.06, 0, 1); vanh = vanh * vanh * (3 - 2 * vanh)
d -= 0.020 * np.maximum(0.0, perlin(co * 9.0 + S2) - 0.1) * vanh   # mieng chau sut me
co = co + no * d[:, None]
me.vertices.foreach_set('co', co.ravel())
me.update()
log('duc mat da xong')

# ------------------------------------------------------------------ vat lieu nguon
def vl_nguon():
    mat = bpy.data.materials.new('DaNguon')
    mat.use_nodes = True
    nt = mat.node_tree; N = nt.nodes; L = nt.links
    for n in list(N): N.remove(n)
    out = N.new('ShaderNodeOutputMaterial')
    bsdf = N.new('ShaderNodeBsdfPrincipled')
    L.new(bsdf.outputs['BSDF'], out.inputs['Surface'])
    tc = N.new('ShaderNodeTexCoord')
    toaDo = tc.outputs['Object']

    def math_(op, a, b=None, c=None):
        n = N.new('ShaderNodeMath'); n.operation = op
        for i, x in enumerate((a, b, c)):
            if x is None: continue
            if isinstance(x, (int, float)): n.inputs[i].default_value = x
            else: L.new(x, n.inputs[i])
        return n.outputs[0]

    def noi(vec, scale, detail=6.0, rough=0.55):
        n = N.new('ShaderNodeTexNoise')
        n.inputs['Scale'].default_value = scale
        n.inputs['Detail'].default_value = detail
        n.inputs['Roughness'].default_value = rough
        L.new(vec, n.inputs['Vector'])
        return n.outputs['Fac']

    def cong_vec(vec, off):
        n = N.new('ShaderNodeVectorMath'); n.operation = 'ADD'
        L.new(vec, n.inputs[0]); n.inputs[1].default_value = off
        return n.outputs[0]

    def ss_(x, a, b):
        n = N.new('ShaderNodeMapRange'); n.data_type = 'FLOAT'; n.interpolation_type = 'SMOOTHSTEP'
        n.clamp = True
        L.new(x, n.inputs['Value'])
        n.inputs['From Min'].default_value = a; n.inputs['From Max'].default_value = b
        n.inputs['To Min'].default_value = 0.0; n.inputs['To Max'].default_value = 1.0
        return n.outputs['Result']

    def nut_tai(dz):
        """Vet nut o toa do dich len dz (de ve vet mau chay XUONG tu nut)."""
        v = cong_vec(toaDo, (0.0, 0.0, dz))
        # Bop meo toa do bang nhieu cho nut ngoan ngoeo
        mo = N.new('ShaderNodeTexNoise'); mo.inputs['Scale'].default_value = 5.0
        mo.inputs['Detail'].default_value = 3.0
        L.new(v, mo.inputs['Vector'])
        lech = N.new('ShaderNodeVectorMath'); lech.operation = 'SUBTRACT'
        L.new(mo.outputs['Color'], lech.inputs[0]); lech.inputs[1].default_value = (0.5, 0.5, 0.5)
        lech2 = N.new('ShaderNodeVectorMath'); lech2.operation = 'SCALE'
        L.new(lech.outputs[0], lech2.inputs[0]); lech2.inputs['Scale'].default_value = 0.16
        vv = N.new('ShaderNodeVectorMath'); vv.operation = 'ADD'
        L.new(v, vv.inputs[0]); L.new(lech2.outputs[0], vv.inputs[1])
        vo = N.new('ShaderNodeTexVoronoi'); vo.feature = 'DISTANCE_TO_EDGE'
        vo.inputs['Scale'].default_value = 3.2
        L.new(vv.outputs[0], vo.inputs['Vector'])
        vo2 = N.new('ShaderNodeTexVoronoi'); vo2.feature = 'DISTANCE_TO_EDGE'
        vo2.inputs['Scale'].default_value = 9.0
        L.new(vv.outputs[0], vo2.inputs['Vector'])
        lon = math_('SUBTRACT', 1.0, ss_(vo.outputs['Distance'], 0.0, 0.020))
        nho = math_('SUBTRACT', 1.0, ss_(vo2.outputs['Distance'], 0.0, 0.012))
        # Chi mot so vung bi nut (mat na nhieu), nut nho con thua hon
        mask = ss_(noi(v, 1.6, 2.0), 0.46, 0.60)
        mask2 = ss_(noi(cong_vec(v, (3.3, 1.1, 0.0)), 2.4, 2.0), 0.55, 0.66)
        return math_('MAXIMUM', math_('MULTIPLY', lon, mask), math_('MULTIPLY', nho, mask2))

    nut = nut_tai(0.0)
    # Vet mau chay xuong: nut o phia TREN diem nay, cang xa cang nhat
    doc = N.new('ShaderNodeVectorMath'); doc.operation = 'MULTIPLY'
    L.new(toaDo, doc.inputs[0]); doc.inputs[1].default_value = (34.0, 34.0, 1.6)
    soi = ss_(noi(doc.outputs[0], 1.0, 2.0), 0.48, 0.62)     # soi mau doc manh
    chay = None
    for dz, w in ((0.025, 1.0), (0.055, 0.85), (0.09, 0.65), (0.14, 0.45), (0.20, 0.25)):
        x = math_('MULTIPLY', nut_tai(dz), w)
        chay = x if chay is None else math_('MAXIMUM', chay, x)
    chay = math_('MULTIPLY', chay, soi)
    mauChe = ss_(noi(cong_vec(toaDo, (7.0, 2.0, 5.0)), 1.3, 2.0), 0.35, 0.55)  # nut nao ri mau
    mau = math_('MULTIPLY', math_('MAXIMUM', nut, chay), mauChe)
    mau = math_('MINIMUM', math_('MULTIPLY', mau, 1.4), 1.0)

    # Mau da
    ramp = N.new('ShaderNodeValToRGB')
    L.new(noi(toaDo, 4.0, 8.0, 0.6), ramp.inputs['Fac'])
    e = ramp.color_ramp.elements
    e[0].position = 0.25; e[0].color = (0.060, 0.055, 0.050, 1)
    e[1].position = 0.75; e[1].color = (0.30, 0.275, 0.24, 1)
    mid = e.new(0.5); mid.color = (0.165, 0.150, 0.135, 1)
    tach = N.new('ShaderNodeSeparateXYZ'); L.new(toaDo, tach.inputs[0])
    z = tach.outputs['Z']
    # Reu am o chan, muoi den gan mieng lua
    reu = math_('MULTIPLY', ss_(noi(cong_vec(toaDo, (2.0, 9.0, 1.0)), 3.0, 4.0), 0.5, 0.7),
                math_('SUBTRACT', 1.0, ss_(z, 0.05, 0.55)))
    mix1 = N.new('ShaderNodeMix'); mix1.data_type = 'RGBA'
    L.new(reu, mix1.inputs['Factor']); L.new(ramp.outputs['Color'], mix1.inputs['A'])
    mix1.inputs['B'].default_value = (0.07, 0.085, 0.045, 1)
    muoi = math_('MULTIPLY', ss_(z, 1.02, 1.30), ss_(noi(toaDo, 2.5, 3.0), 0.35, 0.65))
    mix2 = N.new('ShaderNodeMix'); mix2.data_type = 'RGBA'
    L.new(muoi, mix2.inputs['Factor']); L.new(mix1.outputs['Result'], mix2.inputs['A'])
    mix2.inputs['B'].default_value = (0.025, 0.022, 0.02, 1)
    # Long nut toi han
    mix3 = N.new('ShaderNodeMix'); mix3.data_type = 'RGBA'
    L.new(math_('MULTIPLY', nut, 0.85), mix3.inputs['Factor']); L.new(mix2.outputs['Result'], mix3.inputs['A'])
    mix3.inputs['B'].default_value = (0.02, 0.015, 0.012, 1)
    # Mau: do sam, cho day thi gan den
    mauMau = N.new('ShaderNodeMix'); mauMau.data_type = 'RGBA'
    L.new(ss_(mau, 0.3, 1.0), mauMau.inputs['Factor'])
    mauMau.inputs['A'].default_value = (0.13, 0.006, 0.004, 1)
    mauMau.inputs['B'].default_value = (0.045, 0.0015, 0.001, 1)
    mix4 = N.new('ShaderNodeMix'); mix4.data_type = 'RGBA'
    L.new(ss_(mau, 0.05, 0.35), mix4.inputs['Factor']); L.new(mix3.outputs['Result'], mix4.inputs['A'])
    L.new(mauMau.outputs['Result'], mix4.inputs['B'])
    L.new(mix4.outputs['Result'], bsdf.inputs['Base Color'])

    # Do nham: da kho 0,9 - mau uot 0,22
    nham = N.new('ShaderNodeMapRange'); nham.clamp = True
    L.new(ss_(mau, 0.05, 0.4), nham.inputs['Value'])
    nham.inputs['To Min'].default_value = 0.92; nham.inputs['To Max'].default_value = 0.22
    L.new(nham.outputs['Result'], bsdf.inputs['Roughness'])

    # Loi lom: hat min + khe nut sau
    cao_ = math_('ADD', math_('MULTIPLY', noi(toaDo, 32.0, 6.0, 0.6), 0.5),
                 math_('MULTIPLY', noi(toaDo, 9.0, 4.0), 0.35))
    cao_ = math_('SUBTRACT', cao_, math_('MULTIPLY', nut, 1.3))
    bump = N.new('ShaderNodeBump')
    bump.inputs['Strength'].default_value = 1.0
    bump.inputs['Distance'].default_value = 0.004
    L.new(cao_, bump.inputs['Height'])
    L.new(bump.outputs['Normal'], bsdf.inputs['Normal'])
    return mat

cao.data.materials.clear()
cao.data.materials.append(vl_nguon())
bpy.ops.object.select_all(action='DESELECT')
cao.select_set(True); bpy.context.view_layer.objects.active = cao
bpy.ops.object.shade_smooth()
log('vat lieu nguon xong')

# ------------------------------------------------------------------ luoi gon
chon([cao]); bpy.ops.object.duplicate()
gon = bpy.context.view_layer.objects.active
gon.name = 'LoLuaDa'
so_mat = len(gon.data.polygons)
m = gon.modifiers.new('dc', 'DECIMATE'); m.ratio = min(1.0, 8000.0 / (2.0 * so_mat))
ap_modifier(gon, m)
gon.data.materials.clear()
chon([gon]); bpy.ops.object.shade_smooth()
bpy.ops.object.mode_set(mode='EDIT')
bpy.ops.mesh.select_all(action='SELECT')
bpy.ops.uv.smart_project(angle_limit=math.radians(62), island_margin=0.004)
bpy.ops.object.mode_set(mode='OBJECT')
log('luoi gon:', len(gon.data.polygons), 'mat, tu', so_mat)

# ------------------------------------------------------------------ nuong
vlGon = bpy.data.materials.new('LoLuaDa_Da'); vlGon.use_nodes = True
gon.data.materials.append(vlGon)
nodeAnh = vlGon.node_tree.nodes.new('ShaderNodeTexImage')
vlGon.node_tree.nodes.active = nodeAnh

def nuong(ten, kieu, mau_sac=False, **kw):
    im = bpy.data.images.new(ten, KT, KT, alpha=False, float_buffer=not mau_sac)
    im.colorspace_settings.name = 'sRGB' if mau_sac else 'Non-Color'
    nodeAnh.image = im
    chon([cao, gon], gon)
    t = time.time()
    bpy.ops.object.bake(type=kieu, use_selected_to_active=True, cage_extrusion=0.02,
                        max_ray_distance=0.06, margin=12, use_clear=True, **kw)
    log('nuong', ten, '%.0fs' % (time.time() - t))
    a = np.empty(KT * KT * 4, dtype=np.float32); im.pixels.foreach_get(a)
    return a.reshape(KT, KT, 4)

sc.cycles.samples = 4
albedo = nuong('Albedo', 'DIFFUSE', True, pass_filter={'COLOR'})
nham = nuong('Nham', 'ROUGHNESS')
phap = nuong('Phap', 'NORMAL', normal_space='TANGENT')
sc.cycles.samples = 24
ao = nuong('AO', 'AO')

def ghi(ten, arr):
    im = bpy.data.images.new(ten, KT, KT, alpha=True)
    im.colorspace_settings.name = 'sRGB'
    im.pixels.foreach_set(np.clip(arr, 0, 1).astype(np.float32).ravel())
    im.filepath_raw = os.path.join(RA, ten + '.png'); im.file_format = 'PNG'; im.save()

# AO nhan vao mau (60%) - khe sau toi hon ma khong can them mot texture
aoK = ao[..., :1]
alb = albedo.copy(); alb[..., :3] = alb[..., :3] * (0.4 + 0.6 * aoK); alb[..., 3] = 1
ghi('LoLuaDa_Albedo', alb)
nm = phap.copy(); nm[..., 3] = 1
ghi('LoLuaDa_Normal', nm)
ms = np.zeros_like(nham); ms[..., 3] = 1.0 - nham[..., 0]
ghi('LoLuaDa_KimLoaiBong', ms)
log('ghi texture xong')

# ------------------------------------------------------------------ than hong
than = []
for i in range(34):
    bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=2, radius=random.uniform(0.03, 0.065))
    o = bpy.context.view_layer.objects.active
    r = math.sqrt(random.random()) * 0.33
    a = random.random() * 2 * math.pi
    o.location = (r * math.cos(a), r * math.sin(a), 1.12 + random.uniform(0, 0.08) + (0.33 - r) * 0.12)
    o.rotation_euler = (random.random() * 3, random.random() * 3, random.random() * 3)
    o.scale = (1.0, random.uniform(0.7, 1.2), random.uniform(0.6, 0.9))
    for v in o.data.vertices:
        v.co += v.normal * 0.25 * noise.noise(v.co * 3.0 + Vector((i, i * 0.3, 1)))
    than.append(o)
chon(than, than[0]); bpy.ops.object.join()
thanOb = bpy.context.view_layer.objects.active; thanOb.name = 'LoLuaDa_Than'
bpy.ops.object.transform_apply(location=False, rotation=True, scale=True)
thanOb.data.materials.append(bpy.data.materials.new('LoLuaDa_Than'))
bpy.ops.object.mode_set(mode='EDIT'); bpy.ops.mesh.select_all(action='SELECT')
bpy.ops.uv.smart_project(angle_limit=math.radians(66), island_margin=0.01)
bpy.ops.object.mode_set(mode='OBJECT')

# ------------------------------------------------------------------ xuat
chon([gon, thanOb], gon)
bpy.ops.export_scene.fbx(filepath=os.path.join(RA, 'LoLuaDa.fbx'), use_selection=True,
                         apply_unit_scale=True, apply_scale_options='FBX_SCALE_ALL',
                         axis_forward='-Z', axis_up='Y', bake_space_transform=True,
                         use_mesh_modifiers=True, mesh_smooth_type='OFF', add_leaf_bones=False,
                         path_mode='STRIP')
bb = [gon.matrix_world @ Vector(c) for c in gon.bound_box]
log('XONG: luoi gon %d tam giac, than %d tam giac, cao %.3f m, rong %.3f m' % (
    sum(len(p.vertices) - 2 for p in gon.data.polygons),
    sum(len(p.vertices) - 2 for p in thanOb.data.polygons),
    max(v.z for v in bb) - min(v.z for v in bb), max(v.x for v in bb) - min(v.x for v in bb)))
bpy.ops.wm.save_as_mainfile(filepath=os.path.join(RA, 'LoLuaDa.blend'))
