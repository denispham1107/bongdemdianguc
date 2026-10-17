# MANH DAT VUN NGON NGANG TREN MAT DAT ACT2 + MAN CHINH (17/10/2026, nguoi dung: "nhieu cho nut vo dat cat ngon ngang tum lum").
# Chay trong Blender MCP: exec(open(r"...\dat_vun.py", encoding="utf-8").read()). Dung scene RIENG "DatVun" (khong mo file, khong reset).
#
# Bon nhom manh (goc o DAY, z = 0 la mat dat; phan duoi z < 0 se bi chon):
#   CucDat0-5  cuc dat vo tron meo (icosphere chia 2 + nhieu), duong kinh 0,14-0,34 m, mat min
#   DaVun0-3   da vun goc canh (bao loi cua 10-14 diem ngau nhien), 0,07-0,20 m, mat phang
#   MangDat0-2 mang dat nut vo (da giac meo 7-10 dinh dun day 4-7 cm), 0,35-0,70 m, mat phang - Unity dat NGHIENG 8-25 do
#   DongDat0-1 dong dat cat thap (dia luoi, hinh (1 - r^2)^1,5 + nhieu), rong 1,0-1,6 m, cao 0,12-0,20 m, VANH chim xuong -0,04 m
#              (mep dong dat cat mat dat THEO GOC, khong nam song song sat mat dat - tranh z-fighting nhu "o vuong" lan truoc)
# UV: chieu hop theo truc chinh cua mat, don vi met / TILE (cung mat do anh voi terrain), lech ngau nhien moi manh.
# Vat lieu: "DatVun_Dat" (cuc dat, mang dat, dong dat) va "DatVun_Da" (da vun) - Unity gan anh DatTroc / SoiDa.
import bpy, bmesh, random, math, os
from mathutils import Vector, noise

RA = r"C:\Users\HP\Documents\GameUnity\Diablo25D\Assets\Terrain\DatVun\DatVun.fbx"
TILE = {"DatVun_Dat": 1.5, "DatVun_Da": 0.9}
rnd = random.Random(17102026)

if "DatVun" in bpy.data.scenes:
    sc = bpy.data.scenes["DatVun"]
    for o in list(sc.objects):
        bpy.data.objects.remove(o, do_unlink=True)
else:
    sc = bpy.data.scenes.new("DatVun")
bpy.context.window.scene = sc

def vatlieu(ten):
    m = bpy.data.materials.get(ten) or bpy.data.materials.new(ten)
    return m
VL = {k: vatlieu(k) for k in TILE}

def uv_hop(bm, tile, lech):
    uv = bm.loops.layers.uv.verify()
    for f in bm.faces:
        n = f.normal
        ax = max(range(3), key=lambda i: abs(n[i]))
        for l in f.loops:
            p = l.vert.co
            a, b = [(p.y, p.z), (p.x, p.z), (p.x, p.y)][ax]
            l[uv].uv = ((a / tile) + lech[0], (b / tile) + lech[1])

def tao(ten, bm, vl, min_mat):
    me = bpy.data.meshes.new(ten)
    uv_hop(bm, TILE[vl], (rnd.random(), rnd.random()))
    bm.to_mesh(me); bm.free()
    for p in me.polygons: p.use_smooth = min_mat
    me.materials.append(VL[vl])
    ob = bpy.data.objects.new(ten, me)
    sc.collection.objects.link(ob)
    return ob

def cuc_dat(i):
    r = rnd.uniform(0.07, 0.17)
    bm = bmesh.new(); bmesh.ops.create_icosphere(bm, subdivisions=2, radius=1.0)
    s = Vector((rnd.uniform(0.85, 1.25), rnd.uniform(0.85, 1.25), rnd.uniform(0.55, 0.8)))
    o = Vector((rnd.random() * 50, rnd.random() * 50, rnd.random() * 50))
    for v in bm.verts:
        d = 1.0 + 0.45 * noise.noise(v.co * 1.7 + o) + 0.18 * noise.noise(v.co * 4.1 + o)   # lan 1 (0,28 / 0,10) tron nhu soi
        v.co = Vector((v.co.x * s.x, v.co.y * s.y, v.co.z * s.z)) * d * r
    zmin = min(v.co.z for v in bm.verts)
    for v in bm.verts: v.co.z -= zmin + r * s.z * 0.35          # chon 35% chieu cao
    bm.normal_update()
    return tao("CucDat%d" % i, bm, "DatVun_Dat", False)   # mat cat phang: dat vo goc canh, khong tron nhan

def da_vun(i):
    r = rnd.uniform(0.05, 0.14)
    bm = bmesh.new()
    pts = []
    for _ in range(rnd.randint(10, 14)):
        v = Vector((rnd.gauss(0, 1), rnd.gauss(0, 1), rnd.gauss(0, 1) * 0.7)).normalized()
        pts.append(bm.verts.new(v * r * rnd.uniform(0.7, 1.15)))
    bmesh.ops.convex_hull(bm, input=pts)
    for v in [v for v in bm.verts if not v.link_faces]: bm.verts.remove(v)
    zmin = min(v.co.z for v in bm.verts)
    for v in bm.verts: v.co.z -= zmin + r * 0.3
    bm.normal_update()
    return tao("DaVun%d" % i, bm, "DatVun_Da", False)

def mang_dat(i):
    R = rnd.uniform(0.17, 0.35); day = rnd.uniform(0.04, 0.07)
    n = rnd.randint(7, 10)
    bm = bmesh.new()
    vs = []
    for k in range(n):
        a = (k + rnd.uniform(-0.3, 0.3)) / n * math.tau
        rr = R * rnd.uniform(0.65, 1.15)
        vs.append(bm.verts.new((math.cos(a) * rr, math.sin(a) * rr * rnd.uniform(0.7, 1.0), 0.0)))
    f = bm.faces.new(vs)
    ex = bmesh.ops.extrude_face_region(bm, geom=[f])
    top = [e for e in ex["geom"] if isinstance(e, bmesh.types.BMVert)]
    for v in top: v.co.z += day + rnd.uniform(-0.008, 0.008)
    bmesh.ops.triangulate(bm, faces=bm.faces[:])
    for v in bm.verts: v.co.z -= day * 0.4                       # chon 40% be day
    bmesh.ops.recalc_face_normals(bm, faces=bm.faces[:])
    bm.normal_update()
    return tao("MangDat%d" % i, bm, "DatVun_Dat", False)

def dong_dat(i):
    R = rnd.uniform(0.5, 0.8); H = rnd.uniform(0.12, 0.20)
    bm = bmesh.new()
    # luoi cuc: tam + 5 vong x 16 nan (vong ngoai cung = vanh chim)
    VONG, NAN = 5, 16
    tam = bm.verts.new((0, 0, 0)); vong = []
    for k in range(1, VONG + 1):
        vong.append([bm.verts.new((math.cos(a) * k / VONG, math.sin(a) * k / VONG, 0)) for a in [j / NAN * math.tau for j in range(NAN)]])
    for j in range(NAN):
        bm.faces.new((tam, vong[0][j], vong[0][(j + 1) % NAN]))
    for k in range(VONG - 1):
        for j in range(NAN):
            a, b = vong[k][j], vong[k][(j + 1) % NAN]; c, d = vong[k + 1][(j + 1) % NAN], vong[k + 1][j]
            bm.faces.new((a, d, c)); bm.faces.new((a, c, b))
    o = Vector((rnd.random() * 50, rnd.random() * 50, 0))
    for v in bm.verts:
        x, y = v.co.x, v.co.y
        rr = min(1.0, math.hypot(x, y))
        z = H * (1 - rr * rr) ** 1.5 * (1 + 0.35 * noise.noise(Vector((x * 2.2, y * 2.2, 0)) + o))
        if rr > 0.97: z = -0.04
        z = max(z, -0.04)
        v.co = Vector((x * R * rnd.uniform(0.95, 1.05), y * R * 0.85, z))
    bm.normal_update()
    return tao("DongDat%d" % i, bm, "DatVun_Dat", True)

# dong dat bo (lan 1 trong game nhu qua trung tron) - Unity rai CUM 6-12 cuc dat / da / mang chong nhau thay the
obs = [cuc_dat(i) for i in range(6)] + [da_vun(i) for i in range(4)] + [mang_dat(i) for i in range(3)]
for k, ob in enumerate(obs):
    ob.location = ((k % 5) * 1.8, (k // 5) * 1.8, 0)

os.makedirs(os.path.dirname(RA), exist_ok=True)
vl = sc.view_layers[0]
for o in sc.objects: o.select_set(o in obs, view_layer=vl)
# xuat goc toa do moi manh ve 0 (Unity tu dat): tam thoi dua ve goc
luu = [o.location.copy() for o in obs]
for o in obs: o.location = (0, 0, 0)
with bpy.context.temp_override(scene=sc, view_layer=vl):
  bpy.ops.export_scene.fbx(filepath=RA, use_selection=True, axis_forward='-Z', axis_up='Y', bake_space_transform=True,
                           apply_unit_scale=True, mesh_smooth_type='FACE', use_mesh_modifiers=True, add_leaf_bones=False)
for o, l in zip(obs, luu): o.location = l
print("\n".join("%s: %d tam giac, rong %.2f cao %.2f day %.3f" % (o.name, sum(len(p.vertices) - 2 for p in o.data.polygons),
      max(o.dimensions.x, o.dimensions.y), o.dimensions.z, min((o.matrix_world @ v.co).z for v in o.data.vertices) - o.location.z) for o in obs))
print("FBX:", RA, os.path.getsize(RA))
