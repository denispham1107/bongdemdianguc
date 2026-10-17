# BIA THANH GIA NUT BE (17/10/2026, nguoi dung: "30% bia mo cay thanh gia bi nut be chi con ngau nhien 1/2 va 2/3 phan than"; chon: phan gay
# NAM DUOI DAT canh be, dung bang Blender MCP). Chay trong Blender MCP: exec(open(r"...\thanh_gia_vo.py", encoding="utf-8").read()).
#
# Nguon: CongCu/ThanhGiaVo/*.obj (Unity menu ThanhGiaVo.XuatVaChon, toa do cuc bo luoi Unity, z dung) nhap y nguyen truc, x100 (cao 1,52).
# Hinh thanh gia: be 0-0,20 (rong 0,62) · bac 0,20-0,38 (0,46) · tru 0,38-1,52 (0,20) · hai canh tay 0,92-1,15 (0,88).
# "1/2" / "2/3" = phan CON LAI theo chieu cao ca bia: gay o ~0,76 (chi con tru, mat ca canh tay) / ~1,01 (gay ngang giua hai canh tay).
# Mat gay: khoi cat co day LUOI LOM CHOM (nhieu chong 3 tang, bien do ~4 cm, nghieng 6-12 do, them vai rang cua sau) -> boolean EXACT
#   (hole tolerant - ban sut me _vo0/_vo1 co lo ho): DIFFERENCE = goc con dung, INTERSECT = phan gay (hai mat gay khop nhau).
#   Them 1-2 mieng me o mep mat gay (khoi cat nho) tren goc con dung de mep vo khong thang.
# Da vun: 3 manh da nho (khoi 20 mat bop meo, 4-9 cm).
# Xuat: CongCu/ThanhGiaVo/Ra/<ten>.txt (v x y z / n x y z moi goc tam giac, don vi luoi Unity = /100) -> Unity menu 146 dung .asset.
import bpy, bmesh, math, os, random
from mathutils import Vector, noise, Matrix

GOC = r"C:\Users\HP\Documents\GameUnity\Diablo25D\CongCu\ThanhGiaVo"
RA = os.path.join(GOC, "Ra")
CAT = {"1_2": 0.76, "2_3": 1.01}
# Lan 1 cat ca ban sut me _vo0/_vo1 (luoi HO tu menu 17) -> boolean ra dam mong, gai chia ngang o cho gay -> nay MOI bia vo cat tu luoi
# thanh gia NGUYEN (kin), nhieu kieu mat gay: 2 kieu con 1/2 + 3 kieu con 2/3 (ten ThanhGiaVo_{con|gay}{1_2|2_3}_{a|b|c})
VIEC = [("LIB_005_TS_cross", "1_2", 11, "a"), ("LIB_005_TS_cross", "1_2", 21, "b"),
        ("LIB_005_TS_cross", "2_3", 12, "a"), ("LIB_005_TS_cross", "2_3", 22, "b"), ("LIB_005_TS_cross", "2_3", 32, "c")]
sc = bpy.data.scenes["ThanhGiaVo"]

def xoa(ten):
    o = bpy.data.objects.get(ten)
    if o: me = o.data; bpy.data.objects.remove(o);
    if ten in bpy.data.meshes: bpy.data.meshes.remove(bpy.data.meshes[ten])

def khoi_cat(ten, zc, seed):
    # mat day lom chom phu rong 1,4 x 0,8 quanh truc, dun len toi z = 3
    rnd = random.Random(seed)
    nghieng = math.radians(rnd.uniform(10, 22)); huong = rnd.uniform(0, 2 * math.pi)
    # bac gay so le: mot ben thap hon 5-10 cm, ranh gioi lom chom
    bacX = rnd.uniform(-0.25, 0.25); bacH = rnd.uniform(0.05, 0.10) * rnd.choice((-1, 1))
    lech = Vector((rnd.uniform(0, 50), rnd.uniform(0, 50), rnd.uniform(0, 50)))
    bm = bmesh.new()
    nx, ny, W, D = 56, 32, 1.4, 0.8
    grid = []
    for j in range(ny + 1):
        hang = []
        for i in range(nx + 1):
            x = -W / 2 + W * i / nx; y = -D / 2 + D * j / ny
            p = Vector((x * 9, y * 9, 0)) + lech
            h = 0.05 * noise.noise(p) + 0.022 * noise.noise(p * 2.3) + 0.01 * noise.noise(p * 5.1)
            h += 0.06 * max(0.0, noise.noise(p * 0.6 + Vector((7, 3, 1))))       # vai rang cua sau
            ranh = bacX + 0.06 * noise.noise(Vector((0, y * 6, 3)) + lech)
            h += bacH * (0.5 + 0.5 * math.tanh((x - ranh) / 0.015))           # bac gay so le
            h += math.tan(nghieng) * (x * math.cos(huong) + y * math.sin(huong))
            hang.append(bm.verts.new((x, y, zc + h)))
        grid.append(hang)
    for j in range(ny):
        for i in range(nx):
            bm.faces.new((grid[j][i], grid[j + 1][i], grid[j + 1][i + 1], grid[j][i + 1]))
    # dun mep len thanh khoi kin
    bien = [e for e in bm.edges if e.is_boundary]
    r = bmesh.ops.extrude_edge_only(bm, edges=bien)
    vmoi = [v for v in r["geom"] if isinstance(v, bmesh.types.BMVert)]
    for v in vmoi: v.co.z = 3.0
    bmesh.ops.contextual_create(bm, geom=[e for e in bm.edges if e.is_boundary])
    bmesh.ops.recalc_face_normals(bm, faces=bm.faces)
    me = bpy.data.meshes.new(ten); bm.to_mesh(me); bm.free()
    o = bpy.data.objects.new(ten, me); sc.collection.objects.link(o)
    return o

def khoi_me(ten, tam, kich, seed):
    # mieng me nho: khoi 20 mat bop meo
    rnd = random.Random(seed)
    bm = bmesh.new(); bmesh.ops.create_icosphere(bm, subdivisions=1, radius=1.0)
    for v in bm.verts:
        v.co = Vector((v.co.x * kich[0], v.co.y * kich[1], v.co.z * kich[2])) * rnd.uniform(0.75, 1.15)
    bmesh.ops.translate(bm, verts=bm.verts, vec=tam)
    me = bpy.data.meshes.new(ten); bm.to_mesh(me); bm.free()
    o = bpy.data.objects.new(ten, me); sc.collection.objects.link(o)
    return o

def bool_ap(dich, cong, kieu, kiem=False):
    m = dich.modifiers.new("B", 'BOOLEAN'); m.operation = kieu; m.object = cong; m.solver = 'EXACT'
    try: m.use_hole_tolerant = True
    except Exception: pass
    dg = bpy.context.evaluated_depsgraph_get()
    me = bpy.data.meshes.new_from_object(dich.evaluated_get(dg))
    dich.modifiers.remove(m)
    # kiem: boolean hong (mat qua nhieu mat / mat chieu cao) thi GIU luoi cu - lan dau mieng me lam goc con dung chi con tam be
    zc = [v.co.z for v in dich.data.vertices]; zm = [v.co.z for v in me.vertices]
    if kiem and (len(me.polygons) < 0.7 * len(dich.data.polygons) or not zm or max(zm) < max(zc) - 0.15 or min(zm) > min(zc) + 0.05):
        print("  [bo qua boolean hong]", dich.name, len(dich.data.polygons), "->", len(me.polygons))
        bpy.data.meshes.remove(me); return False
    old = dich.data; dich.data = me; bpy.data.meshes.remove(old); return True

def ban_sao(src, ten):
    xoa(ten)
    o = bpy.data.objects.new(ten, src.data.copy()); o.data.name = ten; sc.collection.objects.link(o)
    return o

def tach_khoi(me):
    # cac khoi roi (be, bac, tru + canh tay la cac vo rieng CHONG len nhau -> boolean exact tinh sai: goc con dung mat ca be)
    bm = bmesh.new(); bm.from_mesh(me); bm.verts.ensure_lookup_table()
    con = set(range(len(bm.verts))); khoi = []
    while con:
        s0 = con.pop(); hang = [s0]; nhom = {s0}
        while hang:
            v = bm.verts[hang.pop()]
            for e in v.link_edges:
                w = e.other_vert(v).index
                if w not in nhom: nhom.add(w); con.discard(w); hang.append(w)
        zs = [bm.verts[i].co.z for i in nhom]; khoi.append((nhom, min(zs), max(zs)))
    bm.free(); return khoi

def loc_khoi(o, giu):
    # giu cac khoi (tap dinh) theo ham giu(zmin, zmax) -> xoa phan con lai
    bm = bmesh.new(); bm.from_mesh(o.data); bm.verts.ensure_lookup_table()
    xoa_v = []
    for nhom, z0, z1 in tach_khoi(o.data):
        if not giu(z0, z1): xoa_v += [bm.verts[i] for i in nhom]
    bmesh.ops.delete(bm, geom=xoa_v, context='VERTS'); bm.to_mesh(o.data); bm.free()

def lam(ten, phan, seed, hau="a"):
    src = bpy.data.objects[ten]
    zc = CAT[phan]
    cat = khoi_cat("TAM_cat", zc, seed)
    cz = [v.co.z for v in cat.data.vertices if v.co.z < 2.9]; c0, c1 = min(cz), max(cz)
    con = ban_sao(src, "ThanhGiaVo_con" + phan + "_" + hau); gay = ban_sao(src, "ThanhGiaVo_gay" + phan + "_" + hau)
    # khoi nam han duoi mat cat: giu nguyen o goc con dung; khoi nam han tren: giu o phan gay; khoi cat ngang: boolean rieng
    duoi = ban_sao(src, "TAM_duoi"); loc_khoi(duoi, lambda z0, z1: z1 < c0)
    tren = ban_sao(src, "TAM_tren"); loc_khoi(tren, lambda z0, z1: z0 > c1)
    loc_khoi(con, lambda z0, z1: not (z1 < c0) and not (z0 > c1))
    loc_khoi(gay, lambda z0, z1: not (z1 < c0) and not (z0 > c1))
    bool_ap(con, cat, 'DIFFERENCE')
    bool_ap(gay, cat, 'INTERSECT')
    for dich, them in ((con, duoi), (gay, tren)):
        bm = bmesh.new(); bm.from_mesh(dich.data); bm.from_mesh(them.data); bm.to_mesh(dich.data); bm.free()
        bpy.data.objects.remove(them)
    # 1-2 mieng me o mep mat gay tren goc con dung
    rnd = random.Random(seed + 100)
    for k in range(rnd.randint(1, 2)):
        goc = rnd.uniform(0, 2 * math.pi)
        tam = Vector((math.cos(goc) * 0.11, math.sin(goc) * 0.11, zc - rnd.uniform(0.02, 0.06)))
        me_o = khoi_me("TAM_me", tam, (0.05, 0.05, 0.07), seed + k)
        bool_ap(con, me_o, 'DIFFERENCE', True)
        bpy.data.objects.remove(me_o)
    bpy.data.objects.remove(cat)
    for o in (con, gay):
        bm = bmesh.new(); bm.from_mesh(o.data)
        bmesh.ops.remove_doubles(bm, verts=bm.verts, dist=1e-5)
        bm.to_mesh(o.data); bm.free()
        # bo manh roi nho (< 10 cm) - rang cua mat cat tach ra vai mau da lo lung canh cho gay
        bm = bmesh.new(); bm.from_mesh(o.data); bm.verts.ensure_lookup_table(); xoa_v = []
        for nhom, z0, z1 in tach_khoi(o.data):
            ps = [bm.verts[i].co for i in nhom]
            dx = max(p.x for p in ps) - min(p.x for p in ps); dy = max(p.y for p in ps) - min(p.y for p in ps)
            if max(dx, dy, z1 - z0) < 0.10: xoa_v += [bm.verts[i] for i in nhom]
            # goc con dung: khoi nao KHONG cham tu than xuong (day > 0,5 m) la doan canh tay bi mat cat tach roi -> lo lung giua khong trung
            elif o is con and z0 > 0.5: xoa_v += [bm.verts[i] for i in nhom]
            elif o is gay and len(nhom) < max(len(k[0]) for k in tach_khoi(o.data)): xoa_v += [bm.verts[i] for i in nhom]
        if xoa_v: print("  bo", len(xoa_v), "dinh manh roi nho o", o.name)
        bmesh.ops.delete(bm, geom=xoa_v, context='VERTS')
        bmesh.ops.recalc_face_normals(bm, faces=bm.faces)
        bm.to_mesh(o.data); bm.free()
    return con, gay

def da_vun(seed, n=3):
    ds = []
    for k in range(n):
        ten = "DaVunThanhGia%d" % k; xoa(ten)
        rnd = random.Random(seed + k)
        bm = bmesh.new(); bmesh.ops.create_icosphere(bm, subdivisions=1, radius=1.0)
        s = (rnd.uniform(0.04, 0.09), rnd.uniform(0.03, 0.07), rnd.uniform(0.025, 0.05))
        for v in bm.verts:
            v.co = Vector((v.co.x * s[0], v.co.y * s[1], v.co.z * s[2])) * rnd.uniform(0.7, 1.2)
        me = bpy.data.meshes.new(ten); bm.to_mesh(me); bm.free()
        o = bpy.data.objects.new(ten, me); sc.collection.objects.link(o); ds.append(o)
    return ds

def xuat(o):
    os.makedirs(RA, exist_ok=True)
    me = o.data
    me.calc_loop_triangles()
    try: cn = [Vector(c.vector) for c in me.corner_normals]
    except Exception: cn = None
    dong = []
    for t in me.loop_triangles:
        for li in t.loops:
            v = me.vertices[me.loops[li].vertex_index].co / 100.0
            # mat gay / da: pha dang phang (moi tam giac mot phap tuyen) - mep sac nhu da vo
            n = t.normal
            dong.append("%.6f %.6f %.6f %.5f %.5f %.5f" % (v.x, v.y, v.z, n.x, n.y, n.z))
    with open(os.path.join(RA, o.name + ".txt"), "w", encoding="utf-8", newline="\n") as f:
        f.write("\n".join(dong))
    return len(me.loop_triangles)
