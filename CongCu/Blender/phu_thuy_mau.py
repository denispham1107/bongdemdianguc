# Anh mau cho quai Phu thuy (model Meshy "Veil of the Abyss" - vo san KHONG co anh mau, chi co anh phap tuyen)
# Theo anh mau nguoi dung gui (nu tu ma): khan trum + ao dai DEN, vien khan om mat + yem nguc TRANG,
# mat trang bach xam, hoc mat tham den, mat vang, tay xam tai.
# Chay trong Blender MCP: exec(open(r"...phu_thuy_mau.py", encoding="utf-8").read())
#   rồi goi to_mau() de gan mau dinh (xem truoc), nuong_anh(duong_dan) de nuong ra PNG theo UV.
import bpy, bmesh, mathutils, math, random

TEN = 'char1'

# Toa do anh truc giao nhin thang truoc: x = 0.02 + (px-350)/1000, z = 1.35 - (py-350)/1000
def _xz(px, py):
    return (0.02 + (px - 350) / 1000.0, 1.35 - (py - 350) / 1000.0)

# Khung mo cua khan trum (mep trong cua khan), do tren anh truc giao 700x700 o khung 0
KHUNG_MO = [_xz(*p) for p in [
    (276, 100), (292, 72), (320, 60), (348, 58), (372, 66), (388, 86), (395, 120),
    (394, 160), (390, 215), (378, 262), (360, 300), (354, 340), (358, 420), (380, 482),
    (266, 488), (270, 440), (272, 380), (273, 320), (271, 260), (268, 200), (270, 140)]]
CAM = 1.43          # z cua cam (duoi la yem nguc)
DAY_YEM = 1.218     # z mep duoi yem nguc
# Mat: hinh bau duc (tam, ban kinh ngang, ban kinh doc)
MAT_TAM = _xz(330, 195); MAT_RX = 0.050; MAT_RZ = 0.080
MAT_TRAI = _xz(311, 178); MAT_PHAI = _xz(352, 178)   # hoc mat
MIENG = _xz(331, 262)

DEN = (0.085, 0.080, 0.090)
TRANG = (0.80, 0.79, 0.76)
DA = (0.66, 0.665, 0.69)
HOC_MAT = (0.10, 0.075, 0.08)
MAT_VANG = (0.85, 0.62, 0.12)
TAY = (0.50, 0.50, 0.53)


def _trong_da_giac(x, z, poly):
    c = False; n = len(poly)
    for i in range(n):
        x1, z1 = poly[i]; x2, z2 = poly[(i + 1) % n]
        if (z1 > z) != (z2 > z):
            xc = x1 + (z - z1) * (x2 - x1) / (z2 - z1)
            if x < xc: c = not c
    return c


def _lerp(a, b, t):
    t = max(0.0, min(1.0, t))
    return tuple(a[i] + (b[i] - a[i]) * t for i in range(3))


def phan_loai():
    """Tra ve list mau theo tung MAT (face) cua luoi, tinh o tu the khung hien tai."""
    o = bpy.data.objects[TEN]
    dg = bpy.context.evaluated_depsgraph_get()
    ev = o.evaluated_get(dg); me = ev.to_mesh()
    bm = bmesh.new(); bm.from_mesh(me); ev.to_mesh_clear()
    bm.transform(o.matrix_world); bm.normal_update(); bm.faces.ensure_lookup_table()
    bvh = mathutils.bvhtree.BVHTree.FromBMesh(bm)
    # trong so xuong tay theo dinh (luoi goc)
    # Ban tay: trong so xuong cua Meshy lon xon (tay phai gan vao cang tay, tay trai lan vao dui) va xuong tay
    # nam o hai ben hong trong khi ban tay tren luoi chia ra truoc bung -> loang theo canh luoi tu dau ngon tay,
    # chi trong hop bao ban tay (do tren anh truc giao truoc + canh ben o khung 0)
    bm.verts.ensure_lookup_table()
    laTay = set()
    for hop, chon in ((((-0.10, 0.07), (-0.34, -0.185), (0.87, 1.04)), lambda v: v.co.y),
                      (((0.11, 0.28), (-0.12, 0.03), (0.74, 0.90)), lambda v: v.co.z)):
        trong = lambda p: all(hop[i][0] <= p[i] <= hop[i][1] for i in range(3))
        vs = [v for v in bm.verts if trong(v.co)]
        hat = min(vs, key=chon)
        S = {hat.index}; st = [hat]
        while st:
            v = st.pop()
            for e in v.link_edges:
                w = e.other_vert(v)
                if w.index not in S and trong(w.co): S.add(w.index); st.append(w)
        laTay |= S
    huong = [mathutils.Vector((0, -1, 0)), mathutils.Vector((0, -1, 0.9)).normalized()]   # thang truoc + tu tren xuong (goc 2.5D)
    rnd = random.Random(7)
    mau = []
    for f in bm.faces:
        c = f.calc_center_median(); n = f.normal
        x, z = c.x, c.z
        lo = False
        for d in huong:
            if n.dot(d) <= 0.05: continue
            h = bvh.ray_cast(c + d * 0.004, d, 2.0)
            if h[0] is None: lo = True; break
        nhieu = rnd.uniform(-0.012, 0.012)
        if all(v.index in laTay for v in f.verts):
            col = TAY
        elif lo and _trong_da_giac(x, z, KHUNG_MO):
            dx = (x - MAT_TAM[0]) / MAT_RX; dz = (z - MAT_TAM[1]) / MAT_RZ
            r = math.sqrt(dx * dx + dz * dz)
            if z >= CAM and r < 1.0:
                col = DA
                for e in (MAT_TRAI, MAT_PHAI):
                    de = math.hypot(x - e[0], (z - e[1]) * 1.3)
                    if de < 0.006: col = MAT_VANG
                    elif de < 0.026: col = _lerp(HOC_MAT, DA, (de - 0.006) / 0.020 * 0.7)
                dm = math.hypot((x - MIENG[0]) * 0.6, z - MIENG[1])
                if dm < 0.008: col = _lerp(HOC_MAT, col, 0.3)
                col = _lerp(col, TRANG, max(0.0, r - 0.85) / 0.15 * 0.5)
            elif z >= DAY_YEM:
                col = TRANG
                if z < DAY_YEM + 0.02: col = _lerp(DEN, TRANG, (z - DAY_YEM) / 0.02)
            else:
                col = DEN
        else:
            col = DEN
        mau.append(tuple(max(0.0, ch + nhieu) for ch in col))
    bm.free()
    return mau


def to_mau():
    """Gan mau dinh theo mat (Face Corner) de xem truoc."""
    o = bpy.data.objects[TEN]
    mau = phan_loai()
    me = o.data
    a = me.color_attributes.get('MauPT') or me.color_attributes.new('MauPT', 'FLOAT_COLOR', 'CORNER')
    for p in me.polygons:
        r, g, b = mau[p.index]
        for li in p.loop_indices:
            a.data[li].color = (r, g, b, 1.0)
    me.color_attributes.active_color = a
    return mau


def _srgb_ve_tuyen_tinh(c):
    return c / 12.92 if c <= 0.04045 else ((c + 0.055) / 1.055) ** 2.4


def nuong_anh(duong_dan, co=2048):
    """Nuong mau theo mat ra anh PNG tren UV goc cua model (gia tri mau trong file = so viet o tren, sRGB)."""
    o = bpy.data.objects[TEN]
    mau = phan_loai()
    me = o.data
    a = me.color_attributes.get('MauNuong') or me.color_attributes.new('MauNuong', 'FLOAT_COLOR', 'CORNER')
    for p in me.polygons:
        r, g, b = (_srgb_ve_tuyen_tinh(ch) for ch in mau[p.index])
        for li in p.loop_indices:
            a.data[li].color = (r, g, b, 1.0)
    img = bpy.data.images.get('PT_Mau')
    if img: bpy.data.images.remove(img)
    img = bpy.data.images.new('PT_Mau', co, co, alpha=False, float_buffer=False)
    img.colorspace_settings.name = 'sRGB'
    mat = bpy.data.materials.get('PT_Nuong') or bpy.data.materials.new('PT_Nuong')
    mat.use_nodes = True
    nt = mat.node_tree; nt.nodes.clear()
    nAttr = nt.nodes.new('ShaderNodeVertexColor'); nAttr.layer_name = 'MauNuong'
    nEm = nt.nodes.new('ShaderNodeEmission')
    nOut = nt.nodes.new('ShaderNodeOutputMaterial')
    nImg = nt.nodes.new('ShaderNodeTexImage'); nImg.image = img
    nt.links.new(nAttr.outputs[0], nEm.inputs[0]); nt.links.new(nEm.outputs[0], nOut.inputs[0])
    nt.nodes.active = nImg
    me.materials.clear(); me.materials.append(mat)
    sc = bpy.context.scene
    try: sc.render.engine = 'CYCLES'
    except TypeError as e: print(e)
    sc.cycles.samples = 1
    sc.cycles.bake_type = 'EMIT'
    sc.render.bake.margin = 16
    sc.render.bake.use_selected_to_active = False
    for ob in bpy.context.selected_objects: ob.select_set(False)
    o.select_set(True); bpy.context.view_layer.objects.active = o
    bpy.ops.object.bake(type='EMIT')
    img.filepath_raw = duong_dan; img.file_format = 'PNG'
    img.save()
    return img
