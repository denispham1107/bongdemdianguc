# HINH "XOAY NGANG DIEN THOAI" cho trang game /choi/ (01/11/2026, nguoi dung: "hien thi thong bao hinh anh yeu cau nguoi choi
# xoay ngang dien thoai de choi"; chon dien thoai DA CO xoay + MUI TEN MAU). Chay trong Blender qua MCP (exec file nay).
#   dung()            dung canh "XoayNgang": dien thoai = phien da bia mo (anh da Act2) vien sat + dinh tan, man hinh anh lua dung
#                     nham; mui ten cong mau mau co giot rot.
#   ve(thu_muc)       render 2 anh nen trong suot: xoay_dien_thoai.png (chi dien thoai, dung DOC - trang web xoay bang CSS) va
#                     xoay_mui_ten.png (chi mui ten) + xoay_xem_truoc.png (ca hai) de xem.
import bpy, math, bmesh
from mathutils import Vector

TEN_CANH = "XoayNgang"
DA = r"C:\Users\HP\Documents\GameUnity\Diablo25D\Assets\BlenderMaps\GraveyardAct2\CayDaChua" + "\\"
DUNG_NHAM = r"C:\Users\HP\Documents\GameUnity\Diablo25D\Assets\Resources\DiaNguc\DungNham.png"
RONG, CAO, DAY = 0.74, 1.42, 0.10        # dien thoai (m) - dung trong mat X-Z, mat truoc nhin ve -Y


def canh():
    sc = bpy.data.scenes.get(TEN_CANH) or bpy.data.scenes.new(TEN_CANH)
    for o in list(sc.objects): bpy.data.objects.remove(o, do_unlink=True)
    return sc


def anh(duong, khong_mau=False):
    im = bpy.data.images.load(duong, check_existing=True)
    if khong_mau: im.colorspace_settings.name = 'Non-Color'
    im.alpha_mode = 'CHANNEL_PACKED'        # alpha anh da bia la du lieu rieng cua shader game - khong phai do trong suot
    return im


def vl_da():
    m = bpy.data.materials.get("XN_Da") or bpy.data.materials.new("XN_Da"); m.use_nodes = True
    nt = m.node_tree; [nt.nodes.remove(n) for n in list(nt.nodes)]
    N = nt.nodes.new; L = nt.links.new
    out = N('ShaderNodeOutputMaterial'); bs = N('ShaderNodeBsdfPrincipled'); tc = N('ShaderNodeTexCoord')
    mp = N('ShaderNodeMapping'); mp.inputs["Scale"].default_value = (1.1,) * 3; L(tc.outputs["Object"], mp.inputs["Vector"])
    def tex(ten, km):
        t = N('ShaderNodeTexImage'); t.image = anh(DA + ten, km); t.projection = 'BOX'; t.projection_blend = 0.3
        L(mp.outputs["Vector"], t.inputs["Vector"]); return t
    tMau = tex("DaMo_Mau.png", False); tGan = tex("DaMo_Gan.png", True); tMn = tex("DaMo_MatNa.png", True)
    # chan mang sang + nhan sac xam am (nhu chu ten game)
    dk = N('ShaderNodeMix'); dk.data_type = 'RGBA'; dk.blend_type = 'DARKEN'; dk.inputs["Factor"].default_value = 1
    dk.inputs[7].default_value = (0.66, 0.66, 0.63, 1); L(tMau.outputs["Color"], dk.inputs[6])
    sac = N('ShaderNodeMix'); sac.data_type = 'RGBA'; sac.blend_type = 'MULTIPLY'; sac.inputs["Factor"].default_value = 1
    sac.inputs[7].default_value = (0.36, 0.35, 0.33, 1); L(dk.outputs[2], sac.inputs[6])
    # vet nut toi
    wn = N('ShaderNodeTexNoise'); wn.inputs["Scale"].default_value = 4; L(tc.outputs["Object"], wn.inputs["Vector"])
    wv = N('ShaderNodeVectorMath'); wv.operation = 'SCALE'; wv.inputs["Scale"].default_value = 0.1; L(wn.outputs["Color"], wv.inputs[0])
    wa = N('ShaderNodeVectorMath'); wa.operation = 'ADD'; L(tc.outputs["Object"], wa.inputs[0]); L(wv.outputs[0], wa.inputs[1])
    vo = N('ShaderNodeTexVoronoi'); vo.feature = 'DISTANCE_TO_EDGE'; vo.inputs["Scale"].default_value = 7; L(wa.outputs[0], vo.inputs["Vector"])
    nr = N('ShaderNodeMapRange'); nr.inputs["From Min"].default_value = 0; nr.inputs["From Max"].default_value = 0.02
    nr.inputs["To Min"].default_value = 1; nr.inputs["To Max"].default_value = 0; L(vo.outputs["Distance"], nr.inputs["Value"])
    vn = N('ShaderNodeTexNoise'); vn.inputs["Scale"].default_value = 2.0; L(tc.outputs["Object"], vn.inputs["Vector"])
    vr = N('ShaderNodeMapRange'); vr.inputs["From Min"].default_value = 0.45; vr.inputs["From Max"].default_value = 0.58; L(vn.outputs["Fac"], vr.inputs["Value"])
    nut = N('ShaderNodeMath'); nut.operation = 'MULTIPLY'; L(nr.outputs["Result"], nut.inputs[0]); L(vr.outputs["Result"], nut.inputs[1])
    c1 = N('ShaderNodeMix'); c1.data_type = 'RGBA'; L(nut.outputs[0], c1.inputs["Factor"]); L(sac.outputs[2], c1.inputs[6])
    c1.inputs[7].default_value = (0.02, 0.015, 0.015, 1)
    # vet mau kho (kenh B mat na)
    sep = N('ShaderNodeSeparateColor'); L(tMn.outputs["Color"], sep.inputs["Color"])
    mr = N('ShaderNodeMapRange'); mr.inputs["From Min"].default_value = 0.55; mr.inputs["From Max"].default_value = 0.85; L(sep.outputs["Blue"], mr.inputs["Value"])
    c2 = N('ShaderNodeMix'); c2.data_type = 'RGBA'; L(mr.outputs["Result"], c2.inputs["Factor"]); L(c1.outputs[2], c2.inputs[6])
    c2.inputs[7].default_value = (0.20, 0.015, 0.012, 1)
    L(c2.outputs[2], bs.inputs["Base Color"]); bs.inputs["Roughness"].default_value = 0.85
    nm = N('ShaderNodeNormalMap'); nm.inputs["Strength"].default_value = 1.3; L(tGan.outputs["Color"], nm.inputs["Color"])
    bp = N('ShaderNodeBump'); bp.inputs["Strength"].default_value = 0.4; L(nut.outputs[0], bp.inputs["Height"])
    L(nm.outputs["Normal"], bp.inputs["Normal"]); L(bp.outputs["Normal"], bs.inputs["Normal"])
    L(bs.outputs[0], out.inputs[0])
    return m


def vl_sat():
    m = bpy.data.materials.get("XN_Sat") or bpy.data.materials.new("XN_Sat"); m.use_nodes = True
    bs = next(n for n in m.node_tree.nodes if n.type == "BSDF_PRINCIPLED")
    bs.inputs["Base Color"].default_value = (0.11, 0.085, 0.075, 1); bs.inputs["Metallic"].default_value = 1.0
    bs.inputs["Roughness"].default_value = 0.42
    return m


def vl_man():
    # man hinh: dung nham phat sang diu (anh dung nham Act2), toi dan ra mep
    m = bpy.data.materials.get("XN_Man") or bpy.data.materials.new("XN_Man"); m.use_nodes = True
    nt = m.node_tree; [nt.nodes.remove(n) for n in list(nt.nodes)]
    N = nt.nodes.new; L = nt.links.new
    out = N('ShaderNodeOutputMaterial'); em = N('ShaderNodeEmission'); tc = N('ShaderNodeTexCoord')
    mp = N('ShaderNodeMapping'); mp.inputs["Scale"].default_value = (0.3, 0.58, 1); L(tc.outputs["UV"], mp.inputs["Vector"])
    t = N('ShaderNodeTexImage'); t.image = anh(DUNG_NHAM); L(mp.outputs["Vector"], t.inputs["Vector"])
    # mo toi o mep: khoang cach UV toi tam
    sx = N('ShaderNodeSeparateXYZ'); L(tc.outputs["UV"], sx.inputs[0])
    def lech(s):
        a = N('ShaderNodeMath'); a.operation = 'SUBTRACT'; L(s, a.inputs[0]); a.inputs[1].default_value = 0.5
        b = N('ShaderNodeMath'); b.operation = 'ABSOLUTE'; L(a.outputs[0], b.inputs[0]); return b.outputs[0]
    mx = N('ShaderNodeMath'); mx.operation = 'MAXIMUM'; L(lech(sx.outputs["X"]), mx.inputs[0]); L(lech(sx.outputs["Y"]), mx.inputs[1])
    vr = N('ShaderNodeMapRange'); vr.inputs["From Min"].default_value = 0.32; vr.inputs["From Max"].default_value = 0.5
    vr.inputs["To Min"].default_value = 0.8; vr.inputs["To Max"].default_value = 0.25; L(mx.outputs[0], vr.inputs["Value"])
    nh = N('ShaderNodeMix'); nh.data_type = 'RGBA'; nh.blend_type = 'MULTIPLY'; nh.inputs["Factor"].default_value = 1
    L(t.outputs["Color"], nh.inputs[6]); nh.inputs[7].default_value = (1.0, 0.55, 0.32, 1)
    L(nh.outputs[2], em.inputs["Color"]); L(vr.outputs["Result"], em.inputs["Strength"])
    L(em.outputs[0], out.inputs[0])
    return m


def vl_mau():
    m = bpy.data.materials.get("XN_Mau") or bpy.data.materials.new("XN_Mau"); m.use_nodes = True
    nt = m.node_tree; bs = next(n for n in nt.nodes if n.type == "BSDF_PRINCIPLED")
    N = nt.nodes.new; L = nt.links.new
    n = N('ShaderNodeTexNoise'); n.inputs["Scale"].default_value = 9
    cr = N('ShaderNodeValToRGB'); cr.color_ramp.elements[0].position = 0.4; cr.color_ramp.elements[0].color = (0.05, 0.0, 0.002, 1)
    cr.color_ramp.elements[1].position = 0.65; cr.color_ramp.elements[1].color = (0.32, 0.005, 0.01, 1)
    L(n.outputs["Fac"], cr.inputs["Fac"]); L(cr.outputs["Color"], bs.inputs["Base Color"])
    bs.inputs["Roughness"].default_value = 0.1; bs.inputs["Coat Weight"].default_value = 1.0; bs.inputs["Coat Roughness"].default_value = 0.05
    return m


def gan(o, sc, m):
    sc.collection.objects.link(o)
    o.data.materials.clear(); o.data.materials.append(m)
    return o


def hop_bo_tron(ten, rong, cao, day, r, seg=6):
    me = bpy.data.meshes.new(ten); bm = bmesh.new()
    bmesh.ops.create_cube(bm, size=1.0)
    for v in bm.verts: v.co = Vector((v.co.x * rong, v.co.y * day, v.co.z * cao))
    bm.to_mesh(me); bm.free()
    o = bpy.data.objects.new(ten, me)
    bv = o.modifiers.new("Vat", 'BEVEL'); bv.width = r; bv.segments = seg; bv.limit_method = 'ANGLE'
    o.modifiers.new("Min", 'WEIGHTED_NORMAL')
    for p in me.polygons: p.use_smooth = True
    return o


def dung():
    sc = canh()
    sc.render.engine = 'BLENDER_EEVEE'
    da, sat, man, mau = vl_da(), vl_sat(), vl_man(), vl_mau()

    # Than da
    than = gan(hop_bo_tron("XN_Than", RONG, CAO, DAY, 0.07), sc, da)
    # Vien sat quanh man (khung chu nhat bo tron, day 0,02) + man lom vao
    mr, mc = RONG - 0.14, CAO - 0.26
    vien = gan(hop_bo_tron("XN_Vien", mr + 0.05, mc + 0.05, 0.03, 0.02, 3), sc, sat)
    vien.location = (0, -DAY / 2 - 0.004, 0.0)
    me = bpy.data.meshes.new("XN_Man"); bm = bmesh.new()
    bmesh.ops.create_grid(bm, x_segments=1, y_segments=1, size=0.5)
    uv = bm.loops.layers.uv.new()
    for f in bm.faces:
        for lp in f.loops: lp[uv].uv = (lp.vert.co.x + 0.5, lp.vert.co.y + 0.5)
    for v in bm.verts: v.co = Vector((v.co.x * mr, 0.0, v.co.y * mc))
    bm.to_mesh(me); bm.free()
    man_o = gan(bpy.data.objects.new("XN_ManHinh", me), sc, man)
    man_o.location = (0, -DAY / 2 - 0.021, 0)
    # Dinh tan bon goc vien + loa (khe) phia tren
    for sxx in (-1, 1):
        for szz in (-1, 1):
            me2 = bpy.data.meshes.new("XN_Dinh"); bm = bmesh.new()
            bmesh.ops.create_uvsphere(bm, u_segments=16, v_segments=8, radius=0.022)
            bm.to_mesh(me2); bm.free()
            d = gan(bpy.data.objects.new("XN_Dinh", me2), sc, sat)
            d.location = (sxx * (RONG / 2 - 0.045), -DAY / 2 - 0.006, szz * (CAO / 2 - 0.06))
            d.scale = (1, 0.5, 1)
            for p in me2.polygons: p.use_smooth = True
    khe = gan(hop_bo_tron("XN_Khe", 0.18, 0.025, 0.02, 0.01, 3), sc, sat)
    khe.location = (0, -DAY / 2 - 0.004, CAO / 2 - 0.075)

    # Mui ten mau: cung tron ~100 do quanh dien thoai (phia tren-phai -> phai), ong thon dan + dau nhon + giot rot
    cu = bpy.data.curves.new("XN_MuiTen", 'CURVE'); cu.dimensions = '3D'
    cu.bevel_depth = 0.045; cu.bevel_resolution = 4; cu.use_fill_caps = True
    sp = cu.splines.new('POLY')
    R, n = 1.02, 40
    a0, a1 = math.radians(115), math.radians(10)
    sp.points.add(n - 1)
    for i in range(n):
        t = i / (n - 1); a = a0 + (a1 - a0) * t
        sp.points[i].co = (R * math.cos(a), -0.12, R * math.sin(a), 1)
        sp.points[i].radius = 0.35 + 0.65 * math.sin(min(1.0, t * 1.15) * math.pi * 0.5)
    mt = gan(bpy.data.objects.new("XN_MuiTen", cu), sc, mau)
    # dau mui ten: non huong theo tiep tuyen cuoi (chieu kim dong ho)
    ac = a1; tiep = Vector((math.sin(ac), 0, -math.cos(ac)))
    me3 = bpy.data.meshes.new("XN_DauMui"); bm = bmesh.new()
    bmesh.ops.create_cone(bm, cap_ends=True, segments=20, radius1=0.12, radius2=0.0, depth=0.26)
    bm.to_mesh(me3); bm.free()
    dau = gan(bpy.data.objects.new("XN_DauMui", me3), sc, mau)
    goc = Vector((R * math.cos(ac), -0.12, R * math.sin(ac)))
    dau.location = goc + tiep * 0.11
    dau.rotation_mode = 'QUATERNION'; dau.rotation_quaternion = Vector((0, 0, 1)).rotation_difference(tiep)
    dau.scale = (1, 0.55, 1)
    for p in me3.polygons: p.use_smooth = True
    # giot mau rot xuong tu than mui ten
    for i, (t, dai) in enumerate([(0.3, 0.09), (0.62, 0.14)]):
        a = a0 + (a1 - a0) * t
        me4 = bpy.data.meshes.new("XN_Giot"); bm = bmesh.new()
        bmesh.ops.create_uvsphere(bm, u_segments=12, v_segments=10, radius=0.024)
        for v in bm.verts:
            if v.co.z < 0: v.co.z *= dai / 0.03          # keo dai xuong duoi
        bm.to_mesh(me4); bm.free()
        g = gan(bpy.data.objects.new("XN_Giot", me4), sc, mau)
        g.location = (R * math.cos(a), -0.12, R * math.sin(a) - 0.04)
        for p in me4.polygons: p.use_smooth = True

    # Den: chinh am phia trai tren, vien do phia sau phai, den lanh yeu phia duoi
    def den(ten, loai, vt, nang, mau_d, kich=1.5):
        ld = bpy.data.lights.new(ten, loai); ld.energy = nang; ld.color = mau_d
        if loai == 'AREA': ld.size = kich
        o = bpy.data.objects.new(ten, ld); sc.collection.objects.link(o); o.location = vt
        huong = (Vector((0, 0, 0)) - Vector(vt)).normalized()
        o.rotation_mode = 'QUATERNION'; o.rotation_quaternion = Vector((0, 0, -1)).rotation_difference(huong)
    den("XN_DenChinh", 'AREA', (-1.6, -2.4, 1.8), 260, (1.0, 0.86, 0.72), 2.0)
    den("XN_DenVien", 'AREA', (1.9, 1.2, 0.8), 320, (1.0, 0.18, 0.08), 1.2)
    den("XN_DenDuoi", 'AREA', (0.6, -1.8, -1.9), 50, (0.55, 0.62, 0.8), 1.5)

    # May quay truc giao, nghieng nhe cho thay be day phien da
    cd = bpy.data.cameras.new("XN_May"); cd.type = 'ORTHO'; cd.ortho_scale = 2.9
    cam = bpy.data.objects.new("XN_May", cd); sc.collection.objects.link(cam); sc.camera = cam
    cam.location = (0.55, -6.0, 0.35)   # nhin vao TAM dien thoai: anh xoay quanh tam anh = tam dien thoai
    cam.rotation_mode = 'QUATERNION'
    cam.rotation_quaternion = (Vector((0, 0, 0)) - cam.location).to_track_quat('-Z', 'Y')   # truc Y cua may quay = len: Z the gioi thang dung (khong nghieng)

    # The gioi toi, nen trong suot, Standard (khong AgX), tat compositor
    w = bpy.data.worlds.get("XN_TheGioi") or bpy.data.worlds.new("XN_TheGioi"); sc.world = w
    w.use_nodes = True
    bg = next(nd for nd in w.node_tree.nodes if nd.type == 'BACKGROUND')
    bg.inputs["Color"].default_value = (0.02, 0.012, 0.012, 1); bg.inputs["Strength"].default_value = 0.6
    sc.view_settings.view_transform = 'Standard'; sc.view_settings.look = 'None'
    sc.render.film_transparent = True; sc.render.use_compositing = False
    sc.render.resolution_x = 900; sc.render.resolution_y = 900; sc.render.resolution_percentage = 100
    sc.render.image_settings.file_format = 'PNG'; sc.render.image_settings.color_mode = 'RGBA'
    print("da dung canh", sc.name, "-", len(sc.objects), "vat")
    return sc


def ve(thu_muc):
    sc = bpy.data.scenes[TEN_CANH]
    phone = [o for o in sc.objects if o.name.startswith(("XN_Than", "XN_Vien", "XN_ManHinh", "XN_Dinh", "XN_Khe"))]
    arrow = [o for o in sc.objects if o.name.startswith(("XN_MuiTen", "XN_DauMui", "XN_Giot"))]
    def an(ds, x):
        for o in ds: o.hide_render = x
    for ten, hien, an_ds in (("xoay_xem_truoc", phone + arrow, []), ("xoay_dien_thoai", phone, arrow), ("xoay_mui_ten", arrow, phone)):
        an(hien, False); an(an_ds, True)
        sc.render.filepath = thu_muc + "\\" + ten + ".png"
        bpy.ops.render.render(write_still=True, scene=sc.name)
    an(phone + arrow, False)
    print("da ve vao", thu_muc)
