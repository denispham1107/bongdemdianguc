# -*- coding: utf-8 -*-
# DAU VET LOC XOAY CUON (16/10/2026, nguoi dung): "khi Loc xoay cuon bay len het moi do vat nhu bia mo, da, nha, cay coi thi de lai DAU VET cua cac
# do vat da cuon bay len chu khong de mat dat trong tron". Nguoi dung chon: RIENG TUNG LOAI, ton tai DEN KHI VAT MOC LAI, dung bang Blender MCP.
# Chay TRONG Blender (MCP): exec(open(r'.../CongCu/Blender/dau_vet_loc_cuon.py', encoding='utf-8').read())  -> scene 'DauVetLoc', collection 'DVL'.
#
# Moi luoi CHUAN HOA (Unity scale luc chay theo kich thuoc vat that), truc Blender Z len (xuat FBX Y len):
#   GocCay0-2  - goc cay GAY: than ban kinh 1 (KHONG loe - dung ban kinh than that), dinh gay lom chom + dam go, 3-5 re NGAN 0,4-0,9 bo sat dat
#                roi cam xuong (scale = ban kinh than do tu luoi cay). Moi manh LAM SAN (chia canh + doi dinh ngau nhien).
#   ChanBia0-2 - chan bia GAY: khoi chan DUNG 1 x 1 (x rong, y day), dinh vo lom chom cao 0,2-0,5 (scale x/y = chan bia that do tu luoi).
#   VunDa0-2   - dong da VUN: 6-12 cuc da gay canh sac trong ban kinh 1 (scale deu).
#   NenNha0-1  - nen NHA MO do nat: 1 x 1, BE NEN da 4 x 4 o nut, chan tuong gay quanh mep (day 0,08) cao 0,08-0,53, dong gach + manh tuong do
#                (scale x/y theo nen nha, z = 1,2 m).
#   NenLo0-1   - nen LO LUA: vong da vo cao 0,05-0,25 ban kinh ~0,5 (scale = be lo).
#   HoDat0-2   - HO DAT xoi: dia ban kinh 1, tam lom -0,1, vanh do lom chom +0,07, cuc dat vai; UV = toa do XY (met khi scale 1) -> anh dat dia hinh.
# Mat cat PHANG (flat) - giong canh vat Act2.

import bpy, bmesh, math, random
from mathutils import Vector

sc = bpy.data.scenes.get('DauVetLoc') or bpy.data.scenes.new('DauVetLoc')
bpy.context.window.scene = sc
col = bpy.data.collections.get('DVL') or bpy.data.collections.new('DVL')
if col.name not in [c.name for c in sc.collection.children]:
    sc.collection.children.link(col)
for o in list(col.objects):
    bpy.data.objects.remove(o, do_unlink=True)


def lam_san(bm, bien, rnd, cuts=1, giu_day=True):
    """LAM SAN SUI (16/10/2026, nguoi dung: "dau vet phai san sui, nham, khong duoc tron bong"): chia nho moi canh 'cuts' lan roi doi tung dinh
    ngau nhien +-bien (dinh chung giu chung -> luoi van kin); dinh duoi day (z < -0,05) chi doi ngang."""
    if cuts > 0:
        bmesh.ops.subdivide_edges(bm, edges=list(bm.edges), cuts=cuts, use_grid_fill=True)
    for v in bm.verts:
        d = Vector((rnd.uniform(-1, 1), rnd.uniform(-1, 1), rnd.uniform(-1, 1))) * bien
        if giu_day and v.co.z < -0.05:
            d.z = 0
        v.co += d


def tao_obj(ten, bm, uv_xy=False):
    me = bpy.data.meshes.get(ten)
    if me:
        bpy.data.meshes.remove(me)
    me = bpy.data.meshes.new(ten)
    if uv_xy:
        uv = bm.loops.layers.uv.new('UVMap')
        for f in bm.faces:
            for l in f.loops:
                l[uv].uv = (l.vert.co.x, l.vert.co.y)
    bm.normal_update()
    bm.to_mesh(me)
    bm.free()
    for p in me.polygons:
        p.use_smooth = False
    ob = bpy.data.objects.new(ten, me)
    col.objects.link(ob)
    return ob


def ong(bm, pts, r0, r1, seg, rnd):
    """Ong thon theo duong pts (re cay), vanh lom chom."""
    rings = []
    n = len(pts)
    for i, p in enumerate(pts):
        t = i / (n - 1)
        d = (pts[min(i + 1, n - 1)] - pts[max(i - 1, 0)]).normalized()
        a = d.orthogonal().normalized()
        b = d.cross(a).normalized()
        r = r0 + (r1 - r0) * t ** 0.8
        ring = []
        for k in range(seg):
            g = 2 * math.pi * k / seg
            rr = r * (1 + rnd.uniform(-0.15, 0.15))
            ring.append(bm.verts.new(p + (a * math.cos(g) + b * math.sin(g) * 0.8) * rr))
        rings.append(ring)
    for i in range(n - 1):
        for k in range(seg):
            bm.faces.new((rings[i][k], rings[i][(k + 1) % seg], rings[i + 1][(k + 1) % seg], rings[i + 1][k]))
    bm.faces.new(list(reversed(rings[0])))
    tip = bm.verts.new(pts[-1] + (pts[-1] - pts[-2]).normalized() * r1)
    for k in range(seg):
        bm.faces.new((rings[-1][k], rings[-1][(k + 1) % seg], tip))


def khoi_vo(bm, tam, kich, rnd, det=0.75):
    """Mot cuc da / gach VO: hop meo 8 dinh, canh sac, nghieng ngau nhien."""
    sx, sy, sz = kich
    ax = rnd.uniform(0, 6.28)
    ca, sa = math.cos(ax), math.sin(ax)
    v = []
    for dx in (-1, 1):
        for dy in (-1, 1):
            for dz in (-1, 1):
                x = dx * sx * rnd.uniform(0.6, 1.0)
                y = dy * sy * rnd.uniform(0.6, 1.0)
                z = dz * sz * rnd.uniform(0.6, 1.0) * (det if dz > 0 else 1)
                x, y = x * ca - y * sa, x * sa + y * ca
                v.append(bm.verts.new(tam + Vector((x, y, z))))
    # thu tu: (-,-,-)0 (-,-,+)1 (-,+,-)2 (-,+,+)3 (+,-,-)4 (+,-,+)5 (+,+,-)6 (+,+,+)7
    for f in ((0, 1, 3, 2), (4, 6, 7, 5), (0, 4, 5, 1), (2, 3, 7, 6), (1, 5, 7, 3), (0, 2, 6, 4)):
        bm.faces.new([v[i] for i in f])


# ================================================================== GOC CAY
def goc_cay(ten, hat):
    rnd = random.Random(hat)
    bm = bmesh.new()
    seg = 28
    cao = rnd.uniform(0.9, 1.6)
    zs = [-0.25, 0.0, 0.12, 0.3, 0.55, 0.8]
    zs = [z for z in zs if z < cao * 0.75] + [cao * 0.75]
    rings = []
    lech = [rnd.uniform(-0.08, 0.08) for _ in range(seg)]
    for z in zs:
        loe = 1.0 + 0.06 * math.exp(-max(z, 0) / 0.15)      # (lan 1 loe 1,35 -> goc to hon than that) than THAT o 0-0,3 m ~ bang 0,3-1 m
        ring = []
        for k in range(seg):
            g = 2 * math.pi * k / seg
            r = loe * (1 + lech[k] + 0.05 * math.sin(5 * g + hat))
            ring.append(bm.verts.new((r * math.cos(g), r * math.sin(g), z)))
        rings.append(ring)
    top = []
    pha = rnd.uniform(0, 6.28)
    for k in range(seg):
        g = 2 * math.pi * k / seg
        h = cao * (0.75 + 0.18 * (0.5 + 0.5 * math.sin(g + pha)))
        if rnd.random() < 0.28:
            h += rnd.uniform(0.15, 0.55) * cao            # dam go nhon
        r = rings[-1][k].co.xy.length * rnd.uniform(0.7, 0.95)
        top.append(bm.verts.new((r * math.cos(g), r * math.sin(g), h)))
    rings.append(top)
    for i in range(len(rings) - 1):
        for k in range(seg):
            bm.faces.new((rings[i][k], rings[i][(k + 1) % seg], rings[i + 1][(k + 1) % seg], rings[i + 1][k]))
    tam = bm.verts.new((rnd.uniform(-0.15, 0.15), rnd.uniform(-0.15, 0.15), cao * rnd.uniform(0.6, 0.8)))
    for k in range(seg):
        bm.faces.new((top[k], top[(k + 1) % seg], tam))
    bm.faces.new(list(reversed(rings[0])))
    for _ in range(rnd.randint(3, 5)):                     # dam go dung nghieng tren mat gay
        g = rnd.uniform(0, 6.28)
        rr = rnd.uniform(0.2, 0.6)
        goc = Vector((rr * math.cos(g), rr * math.sin(g), cao * 0.7))
        dinh = goc + Vector((rnd.uniform(-0.15, 0.15), rnd.uniform(-0.15, 0.15), rnd.uniform(0.25, 0.6) * cao))
        w = rnd.uniform(0.06, 0.13)
        a = Vector((math.cos(g + 1.57), math.sin(g + 1.57), 0)) * w
        b = Vector((math.cos(g), math.sin(g), 0)) * w * 0.6
        v = [bm.verts.new(goc + a), bm.verts.new(goc + b), bm.verts.new(goc - a), bm.verts.new(goc - b), bm.verts.new(dinh)]
        for i in range(4):
            bm.faces.new((v[i], v[(i + 1) % 4], v[4]))
        bm.faces.new((v[3], v[2], v[1], v[0]))
    so = rnd.randint(3, 5)                                  # RE NGAN (lan 1: 5-7 re dai 1-2 ban kinh -> dau vet to gap doi cay): bo sat dat, cam xuong
    for i in range(so):
        g = 2 * math.pi * i / so + rnd.uniform(-0.4, 0.4)
        dai = rnd.uniform(0.4, 0.9)
        pts = []
        for j in range(7):
            t = j / 6
            d = 0.8 + dai * t
            z = 0.22 * (1 - t) ** 2 + 0.04 * math.sin(math.pi * t) - 0.14 * t ** 1.5 + rnd.uniform(-0.02, 0.02)
            g2 = g + 0.22 * math.sin(2.5 * t + i) + rnd.uniform(-0.04, 0.04)
            pts.append(Vector((d * math.cos(g2), d * math.sin(g2), z)))
        ong(bm, pts, rnd.uniform(0.15, 0.22), 0.04, 7, rnd)
    lam_san(bm, 0.035, rnd, 1)
    return tao_obj(ten, bm)


# ================================================================== CHAN BIA GAY
def chan_bia(ten, hat):
    rnd = random.Random(hat)
    bm = bmesh.new()
    nx = 9
    # day: hop 1 x 1 (x tu -0.5..0.5, y -0.5..0.5) tu z=-0.15; dinh vo: hang dinh tren voi do cao ngau nhien + bac gay
    dinh_cao = rnd.uniform(0.2, 0.5)
    nghieng = rnd.uniform(-0.25, 0.25)
    duoi_t, duoi_s, tren_t, tren_s = [], [], [], []
    for i in range(nx):
        x = -0.5 + i / (nx - 1)
        hz = dinh_cao + nghieng * x + rnd.uniform(-0.12, 0.12) + (rnd.uniform(0.05, 0.2) if rnd.random() < 0.25 else 0)
        hz = max(0.05, hz)
        duoi_t.append(bm.verts.new((x, -0.5, -0.15)))
        duoi_s.append(bm.verts.new((x, 0.5, -0.15)))
        tren_t.append(bm.verts.new((x, -0.5 + rnd.uniform(0, 0.06), hz)))
        tren_s.append(bm.verts.new((x, 0.5 - rnd.uniform(0, 0.06), hz + rnd.uniform(-0.06, 0.06))))
    for i in range(nx - 1):
        bm.faces.new((duoi_t[i], duoi_t[i + 1], tren_t[i + 1], tren_t[i]))       # mat truoc
        bm.faces.new((duoi_s[i + 1], duoi_s[i], tren_s[i], tren_s[i + 1]))       # mat sau
        bm.faces.new((tren_t[i], tren_t[i + 1], tren_s[i + 1], tren_s[i]))       # mat gay tren
        bm.faces.new((duoi_s[i], duoi_s[i + 1], duoi_t[i + 1], duoi_t[i]))       # day
    bm.faces.new((duoi_s[0], duoi_t[0], tren_t[0], tren_s[0]))
    bm.faces.new((duoi_t[-1], duoi_s[-1], tren_s[-1], tren_t[-1]))
    # (lan 1 co mieng vo roi toi ban kinh 1 -> khung luoi 1,6 x 1,1 thay vi 1 x 1: vun da nay o luoi VunDa rieng, dat theo co that)
    lam_san(bm, 0.022, rnd, 2)
    return tao_obj(ten, bm)


# ================================================================== DA VUN
def vun_da(ten, hat):
    rnd = random.Random(hat)
    bm = bmesh.new()
    for _ in range(rnd.randint(6, 12)):
        g = rnd.uniform(0, 6.28)
        rr = rnd.uniform(0.1, 1.0) ** 0.7
        s = rnd.uniform(0.06, 0.2) * (1.2 - 0.6 * rr)
        khoi_vo(bm, Vector((rr * math.cos(g), rr * math.sin(g), s * 0.3)), (s, s * rnd.uniform(0.6, 1.0), s * rnd.uniform(0.4, 0.8)), rnd)
    lam_san(bm, 0.01, rnd, 1, False)
    return tao_obj(ten, bm)


# ================================================================== NEN NHA MO
def nen_nha(ten, hat):
    """Nen nha mo do nat (lan 1 - chi vong gach vun thap 0,05-0,35: anh trong game mong manh so voi nha mo 4 m -> them BE NEN va tuong cao hon)."""
    rnd = random.Random(hat)
    bm = bmesh.new()
    day = 0.08
    # BE NEN da: tam 0,92 x 0,92 day 0,06, chia 4 x 4 o, moi o lech cao / nghieng nhe, thinh thoang mat o (nut vo)
    n = 4
    for i in range(n):
        for j in range(n):
            if rnd.random() < 0.12:
                continue
            cx = -0.46 + (i + 0.5) * 0.92 / n
            cy = -0.46 + (j + 0.5) * 0.92 / n
            khoi_vo(bm, Vector((cx, cy, 0.03 + rnd.uniform(-0.012, 0.012))), (0.115 * rnd.uniform(0.9, 1.0), 0.115 * rnd.uniform(0.9, 1.0), 0.035), rnd, det=1.0)
    # CHAN TUONG gay quanh mep: tung doan, cao lom chom 0,08-0,5 (goc nha cao hon), thinh thoang mat doan
    for canh in range(4):
        m = 7
        for i in range(m):
            if rnd.random() < 0.18:
                continue
            t0, t1 = i / m, (i + 1) / m
            tm = (t0 + t1) / 2
            h = rnd.uniform(0.08, 0.38) + (0.15 if (i == 0 or i == m - 1) else 0.0)
            if canh == 0:   c, d = Vector((-0.5 + tm, -0.5 + day / 2, 0)), (0.5 / m, day / 2)
            elif canh == 1: c, d = Vector((-0.5 + tm, 0.5 - day / 2, 0)), (0.5 / m, day / 2)
            elif canh == 2: c, d = Vector((-0.5 + day / 2, -0.5 + tm, 0)), (day / 2, 0.5 / m)
            else:           c, d = Vector((0.5 - day / 2, -0.5 + tm, 0)), (day / 2, 0.5 / m)
            c.z = h / 2 + 0.02
            khoi_vo(bm, c, (d[0] * 1.02, d[1] * 1.02, h / 2 + 0.03), rnd, det=1.0)
    # DONG GACH DA do: 3-4 dong o goc / giua canh + vai khoi lon nghieng (manh tuong do)
    for _ in range(rnd.randint(3, 4)):
        g = rnd.uniform(0, 6.28)
        cx, cy = 0.36 * math.cos(g), 0.36 * math.sin(g)      # (lan 2: 0,45 +-0,15 -> vun vang toi 0,75: dau vet to x1,3 nen nha that)
        for _ in range(rnd.randint(8, 14)):
            s = rnd.uniform(0.025, 0.07)
            khoi_vo(bm, Vector((cx + rnd.uniform(-0.1, 0.1), cy + rnd.uniform(-0.1, 0.1), s * 0.5 + rnd.uniform(0, 0.08))), (s, s * 0.7, s * 0.5), rnd)
    for _ in range(rnd.randint(2, 3)):
        g = rnd.uniform(0, 6.28)
        r = rnd.uniform(0.3, 0.42)
        khoi_vo(bm, Vector((r * math.cos(g), r * math.sin(g), 0.03)), (rnd.uniform(0.1, 0.16), rnd.uniform(0.04, 0.06), rnd.uniform(0.08, 0.14)), rnd)
    lam_san(bm, 0.006, rnd, 1, False)
    return tao_obj(ten, bm)


# ================================================================== NEN LO LUA
def nen_lo(ten, hat):
    rnd = random.Random(hat)
    bm = bmesh.new()
    so = rnd.randint(9, 12)
    for i in range(so):
        if rnd.random() < 0.2:
            continue
        g = 2 * math.pi * i / so + rnd.uniform(-0.1, 0.1)
        r = rnd.uniform(0.42, 0.52)
        h = rnd.uniform(0.05, 0.25)
        khoi_vo(bm, Vector((r * math.cos(g), r * math.sin(g), h / 2 - 0.03)), (rnd.uniform(0.09, 0.14), rnd.uniform(0.07, 0.1), h / 2 + 0.03), rnd)
    for _ in range(rnd.randint(4, 7)):              # da vang ra ngoai
        g = rnd.uniform(0, 6.28)
        r = rnd.uniform(0.6, 0.9)
        s = rnd.uniform(0.04, 0.09)
        khoi_vo(bm, Vector((r * math.cos(g), r * math.sin(g), s * 0.4)), (s, s * 0.8, s * 0.6), rnd)
    lam_san(bm, 0.012, rnd, 1, False)
    return tao_obj(ten, bm)


# ================================================================== HO DAT XOI
def ho_dat(ten, hat):
    rnd = random.Random(hat)
    bm = bmesh.new()
    nr, ns = 14, 48
    pha = [rnd.uniform(0, 6.28) for _ in range(4)]
    tam = bm.verts.new((0, 0, -0.1))
    rings = []
    for i in range(1, nr + 1):
        t = i / nr
        ring = []
        for k in range(ns):
            g = 2 * math.pi * k / ns
            # do sau / vanh: lom o tam, vanh do cao o r~0,72, ve 0 o mep; lom chom theo goc
            nhieu = 0.5 + 0.25 * math.sin(3 * g + pha[0]) + 0.15 * math.sin(7 * g + pha[1]) + 0.1 * math.sin(13 * g + pha[2])
            r = t * (1 + 0.12 * math.sin(2 * g + pha[3]) + 0.06 * math.sin(5 * g + pha[0]))
            z = -0.1 * (1 - (t / 0.55) ** 2) if t < 0.55 else 0.0
            z += 0.07 * nhieu * math.exp(-((t - 0.72) / 0.13) ** 2) + rnd.uniform(-0.012, 0.012) * (1 - t)
            if i == nr:
                z = -0.02
            ring.append(bm.verts.new((r * math.cos(g), r * math.sin(g), z)))
        rings.append(ring)
    for k in range(ns):
        bm.faces.new((tam, rings[0][k], rings[0][(k + 1) % ns]))
    for i in range(nr - 1):
        for k in range(ns):
            bm.faces.new((rings[i][k], rings[i + 1][k], rings[i + 1][(k + 1) % ns], rings[i][(k + 1) % ns]))
    # CUC DAT bi boc len rai tren vanh
    for _ in range(rnd.randint(28, 42)):                   # (lan 1: 10-18 - tron) cuc dat von day
        g = rnd.uniform(0, 6.28)
        rr = rnd.uniform(0.25, 1.05)
        s = rnd.uniform(0.018, 0.06)
        khoi_vo(bm, Vector((rr * math.cos(g), rr * math.sin(g), 0.03 + s * 0.3)), (s, s * 0.8, s * 0.6), rnd)
    return tao_obj(ten, bm, uv_xy=True)


def dung_tat_ca():
    ds = []
    for i in range(3):
        ds.append((goc_cay('GocCay%d' % i, 3100 + i * 17), (i * 7.0, 0)))
        ds.append((chan_bia('ChanBia%d' % i, 3200 + i * 17), (i * 3.0, 8)))
        ds.append((vun_da('VunDa%d' % i, 3300 + i * 17), (i * 3.0, 12)))
        ds.append((ho_dat('HoDat%d' % i, 3400 + i * 17), (i * 3.0, 16)))
    for i in range(2):
        ds.append((nen_nha('NenNha%d' % i, 3500 + i * 17), (i * 2.0, 20)))
        ds.append((nen_lo('NenLo%d' % i, 3600 + i * 17), (i * 2.0, 23)))
    for ob, (x, y) in ds:
        ob.location = (x, y, 0)
    return [(o.name, len(o.data.vertices)) for o, _ in ds]


print(dung_tat_ca())
