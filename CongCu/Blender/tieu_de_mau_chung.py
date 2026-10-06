# Ham dung chung cho 2 ban tieu de (chay trong Blender qua MCP: exec(open(...).read()))
import bpy, bmesh, math, random, mathutils, os
from mathutils.bvhtree import BVHTree

FONTS = r"C:\Users\HP\Documents\GameUnity\Diablo25D\CongCu\Fonts" + "\\"
RA = r"C:\Users\HP\AppData\Local\Temp\claude\C--Users-HP-Documents-GameUnity-Diablo25D\ba500520-6802-430a-ad3d-7f8c1c5a3a76\scratchpad\td2" + "\\"
TEN_CANH = "TieuDe2B"

def canh():
    sc = bpy.data.scenes.get(TEN_CANH) or bpy.data.scenes.new(TEN_CANH)
    for w in bpy.context.window_manager.windows: w.scene = sc
    return sc

def bo_suu_tap(sc, ten):
    c = bpy.data.collections.get(ten)
    if c is None:
        c = bpy.data.collections.new(ten); sc.collection.children.link(c)
    return c

def xoa(ten):
    o = bpy.data.objects.get(ten)
    if o: bpy.data.objects.remove(o, do_unlink=True)

def chu_luoi(sc, coll, ten, chuoi, font, rong, tam_y, extrude, bevel, bevel_res=3, gian=1.0, tu=1.0):
    """Dung chu -> luoi (the gioi), rong = be ngang (m), tra ve object luoi."""
    xoa(ten); xoa(ten + "_Goc")
    f = bpy.data.fonts.load(FONTS + font, check_existing=True)
    cu = bpy.data.curves.new(ten + "_Goc", 'FONT'); cu.body = chuoi; cu.font = f
    cu.align_x = 'CENTER'; cu.align_y = 'CENTER'; cu.space_character = gian; cu.space_word = tu
    cu.extrude = extrude; cu.bevel_depth = bevel; cu.bevel_resolution = bevel_res; cu.resolution_u = 10
    ob = bpy.data.objects.new(ten + "_Goc", cu); coll.objects.link(ob)
    bpy.context.view_layer.update()
    dg = bpy.context.evaluated_depsgraph_get()
    bb = [mathutils.Vector(c) for c in ob.evaluated_get(dg).bound_box]
    w = max(v.x for v in bb) - min(v.x for v in bb)
    k = rong / w
    ob.scale = (k, k, k)
    ob.location = (-(max(v.x for v in bb) + min(v.x for v in bb)) / 2 * k, tam_y - (max(v.y for v in bb) + min(v.y for v in bb)) / 2 * k, 0)
    bpy.context.view_layer.update(); dg = bpy.context.evaluated_depsgraph_get()
    me = bpy.data.meshes.new_from_object(ob.evaluated_get(dg)); me.name = ten; me.transform(ob.matrix_world)
    lo = bpy.data.objects.new(ten, me); coll.objects.link(lo)
    ob.hide_render = True; ob.hide_viewport = True
    for p in me.polygons: p.use_smooth = True
    return lo

def bvh_cua(lo):
    me = lo.data
    return BVHTree.FromPolygons([lo.matrix_world @ v.co for v in me.vertices], [tuple(p.vertices) for p in me.polygons])

def giot_mau(coll, ten, lo, z, seed, mat_do=0.6, buoc=(0.04, 0.14), dai=(0.04, 0.6), r=(0.012, 0.028), tranh=(), duoi_toi=-0.2, roi=0.2):
    """Giot mau NURBS tu diem thap nhat cua net chu (tia quet tu duoi len)."""
    xoa(ten)
    bvh = bvh_cua(lo); rnd = random.Random(seed)
    xs = [lo.matrix_world @ v.co for v in lo.data.vertices]
    x0 = min(v.x for v in xs); x1 = max(v.x for v in xs)
    cv = bpy.data.curves.new(ten, 'CURVE'); cv.dimensions = '3D'; cv.bevel_depth = 1.0; cv.bevel_resolution = 4; cv.use_fill_caps = True; cv.resolution_u = 12
    x = x0; n = 0
    while x < x1:
        x += rnd.uniform(*buoc)
        if any(a < x < b for a, b in tranh): continue
        hit = bvh.ray_cast(mathutils.Vector((x, -3.0, z)), mathutils.Vector((0, 1, 0)), 6.0)
        if hit[0] is None or hit[0].y > duoi_toi: continue
        if rnd.random() > mat_do: continue
        y0 = hit[0].y
        L = rnd.uniform(*dai) * (rnd.random() ** 0.7)
        rr = rnd.uniform(*r)
        prof = [(0.00, 2.3), (0.05, 1.35), (0.22, 0.9), (0.5, 0.72), (0.78, 0.9), (0.9, 1.35), (0.97, 1.15), (1.0, 0.25)]
        sp = cv.splines.new('NURBS'); sp.points.add(len(prof) - 1)
        lech = rnd.uniform(-0.01, 0.01)
        for i, (t, k) in enumerate(prof):
            sp.points[i].co = (x + lech * t * t, y0 + 0.01 - (L + 0.01) * t, z, 1.0); sp.points[i].radius = rr * k
        sp.use_endpoint_u = True; sp.order_u = 3; n += 1
        if rnd.random() < roi:
            sp2 = cv.splines.new('NURBS'); sp2.points.add(3)
            yd = y0 - L - rnd.uniform(0.06, 0.2)
            for i, (dy, k) in enumerate([(0.03, 0.15), (0.0, 1.0), (-0.025, 1.05), (-0.045, 0.2)]):
                sp2.points[i].co = (x, yd + dy, z, 1.0); sp2.points[i].radius = rr * 0.9 * k
            sp2.use_endpoint_u = True; sp2.order_u = 3
    o = bpy.data.objects.new(ten, cv); coll.objects.link(o)
    return o, n

def mau_ban(coll, ten, seed, vung, so, co=(0.003, 0.02), z=-0.05, dep=0.25, so_cum=6, tranh_vung=()):
    """Mau ban: cac giot det (ellipsoid) rai trong vung (x0,x1,y0,y1), tap trung quanh vai tam."""
    xoa(ten)
    rnd = random.Random(seed); bm = bmesh.new()
    x0, x1, y0, y1 = vung
    tams = [(rnd.uniform(x0, x1), rnd.uniform(y0, y1)) for _ in range(so_cum)]
    for i in range(so):
        cx, cy = rnd.choice(tams)
        d = abs(rnd.gauss(0, 0.35)); a = rnd.uniform(0, 2 * math.pi)
        x = cx + math.cos(a) * d; y = cy + math.sin(a) * d * 0.6
        if not (x0 - 0.3 < x < x1 + 0.3): continue
        if any(a0 < x < a1 and b0 < y < b1 for a0, a1, b0, b1 in tranh_vung): continue
        rr = rnd.uniform(*co) * (1.0 / (1 + d * 2))
        res = bmesh.ops.create_icosphere(bm, subdivisions=2, radius=rr)
        sx = rnd.uniform(1.0, 2.6)
        mt = mathutils.Matrix.Rotation(a, 4, 'Z') @ mathutils.Matrix.Diagonal((sx, 1.0, dep, 1.0))
        bmesh.ops.transform(bm, matrix=mt, verts=res["verts"])
        bmesh.ops.translate(bm, verts=res["verts"], vec=(x, y, z))
    me = bpy.data.meshes.new(ten); bm.to_mesh(me); bm.free()
    for p in me.polygons: p.use_smooth = True
    o = bpy.data.objects.new(ten, me); coll.objects.link(o)
    return o

def vat_lieu_mau(ten, mau=(0.09, 0.0, 0.003), nham=0.12):
    m = bpy.data.materials.get(ten) or bpy.data.materials.new(ten); m.use_nodes = True
    nt = m.node_tree; [nt.nodes.remove(n) for n in list(nt.nodes)]
    N = nt.nodes.new; L = nt.links.new
    out = N('ShaderNodeOutputMaterial'); bs = N('ShaderNodeBsdfPrincipled')
    tc = N('ShaderNodeTexCoord'); nz = N('ShaderNodeTexNoise'); nz.inputs["Scale"].default_value = 18; nz.inputs["Detail"].default_value = 6
    L(tc.outputs["Object"], nz.inputs["Vector"])
    rp = N('ShaderNodeValToRGB'); rp.color_ramp.elements[0].position = 0.4; rp.color_ramp.elements[0].color = (mau[0] * 0.35, 0, 0, 1)
    rp.color_ramp.elements[1].position = 0.7; rp.color_ramp.elements[1].color = (mau[0], mau[1], mau[2], 1)
    L(nz.outputs["Fac"], rp.inputs["Fac"]); L(rp.outputs["Color"], bs.inputs["Base Color"])
    bs.inputs["Roughness"].default_value = nham
    bs.inputs["Coat Weight"].default_value = 1.0; bs.inputs["Coat Roughness"].default_value = 0.04
    bs.inputs["Subsurface Weight"].default_value = 0.3; bs.inputs["Subsurface Radius"].default_value = (0.5, 0.03, 0.01)
    bp = N('ShaderNodeBump'); bp.inputs["Strength"].default_value = 0.08
    L(nz.outputs["Fac"], bp.inputs["Height"]); L(bp.outputs["Normal"], bs.inputs["Normal"])
    L(bs.outputs[0], out.inputs[0])
    return m

def dat_den(sc, ten, vt, nhin, nl, mau, co):
    o = bpy.data.objects.get(ten)
    if o is None:
        o = bpy.data.objects.new(ten, bpy.data.lights.new(ten, 'AREA')); sc.collection.objects.link(o)
    o.location = vt; o.rotation_euler = (mathutils.Vector(nhin) - mathutils.Vector(vt)).to_track_quat('-Z', 'Y').to_euler()
    o.data.energy = nl; o.data.color = mau; o.data.size = co
    return o

def may_quay(sc, ortho, cy, rx, ry):
    cam = bpy.data.objects.get("TD2_Cam")
    if cam is None:
        cam = bpy.data.objects.new("TD2_Cam", bpy.data.cameras.new("TD2_Cam")); sc.collection.objects.link(cam)
    cam.data.type = 'ORTHO'; cam.data.ortho_scale = ortho
    cam.location = (0, cy, 10); cam.rotation_euler = (0, 0, 0); sc.camera = cam
    r = sc.render
    try: r.engine = 'BLENDER_EEVEE'
    except TypeError: pass
    r.resolution_x, r.resolution_y, r.resolution_percentage = rx, ry, 100
    r.film_transparent = True; r.image_settings.file_format = 'PNG'; r.image_settings.color_mode = 'RGBA'
    sc.view_settings.view_transform = 'Standard'; sc.view_settings.look = 'None'
    w = bpy.data.worlds.get("TD2_TheGioi") or bpy.data.worlds.new("TD2_TheGioi"); sc.world = w; w.use_nodes = True
    bg = next(n for n in w.node_tree.nodes if n.type == 'BACKGROUND'); bg.inputs[0].default_value = (0.03, 0.01, 0.01, 1); bg.inputs[1].default_value = 0.5
    return cam

def compositor(sc, nguong=0.12, manh=0.9, co=0.85, mau=(1.0, 0.12, 0.06), he_so_alpha=3.5):
    ng = bpy.data.node_groups.get("TD2_Comp") or bpy.data.node_groups.new("TD2_Comp", 'CompositorNodeTree')
    for n in list(ng.nodes): ng.nodes.remove(n)
    if not any(i.in_out == 'OUTPUT' for i in ng.interface.items_tree):
        ng.interface.new_socket("Image", in_out='OUTPUT', socket_type='NodeSocketColor')
    rl = ng.nodes.new('CompositorNodeRLayers'); rl.scene = sc
    gl = ng.nodes.new('CompositorNodeGlare'); gl.name = "Glare"
    gl.inputs["Type"].default_value = 'Fog Glow'; gl.inputs["Quality"].default_value = 'High'
    gl.inputs["Threshold"].default_value = nguong; gl.inputs["Strength"].default_value = manh; gl.inputs["Size"].default_value = co
    gl.inputs["Tint"].default_value = (mau[0], mau[1], mau[2], 1)
    sep = ng.nodes.new('CompositorNodeSeparateColor'); bw = ng.nodes.new('CompositorNodeRGBToBW')
    nh = ng.nodes.new('ShaderNodeMath'); nh.operation = 'MULTIPLY'; nh.inputs[1].default_value = he_so_alpha; nh.use_clamp = True; nh.name = "HeSoAlpha"
    mx = ng.nodes.new('ShaderNodeMath'); mx.operation = 'MAXIMUM'
    sa = ng.nodes.new('CompositorNodeSetAlpha'); sa.inputs["Type"].default_value = 'Replace Alpha'
    go = ng.nodes.new('NodeGroupOutput')
    L = ng.links.new
    L(rl.outputs["Image"], gl.inputs["Image"]); L(rl.outputs["Image"], sep.inputs["Image"])
    L(gl.outputs["Glare"], bw.inputs["Image"]); L(bw.outputs[0], nh.inputs[0])
    L(sep.outputs["Alpha"], mx.inputs[0]); L(nh.outputs[0], mx.inputs[1])
    L(gl.outputs["Image"], sa.inputs["Image"]); L(mx.outputs[0], sa.inputs["Alpha"]); L(sa.outputs[0], go.inputs[0])
    sc.compositing_node_group = ng; sc.render.use_compositing = True

def render_nen(sc, ten_file):
    """Render bang timer (lenh MCP khong cho het gio); ghi RA + ten_file."""
    p = RA + ten_file
    if os.path.exists(p): os.remove(p)
    sc.render.filepath = p
    def chay():
        bpy.ops.render.render(write_still=True, scene=sc.name); return None
    bpy.app.timers.register(chay, first_interval=0.1)
    return p

def khoi_sau(coll, ten, rong, cao, tam_y, z, mau, dam, seed, co_nhieu=3.5):
    """Dai khoi do tham mo phia sau chu (tam phang, alpha tan ra mep)."""
    xoa(ten)
    me = bpy.data.meshes.new(ten)
    me.from_pydata([(-rong/2, tam_y - cao/2, z), (rong/2, tam_y - cao/2, z), (rong/2, tam_y + cao/2, z), (-rong/2, tam_y + cao/2, z)], [], [(0, 1, 2, 3)])
    me.uv_layers.new()
    o = bpy.data.objects.new(ten, me); coll.objects.link(o)
    m = bpy.data.materials.get(ten + "Mat") or bpy.data.materials.new(ten + "Mat"); m.use_nodes = True
    try: m.surface_render_method = 'BLENDED'
    except Exception: pass
    nt = m.node_tree; [nt.nodes.remove(n) for n in list(nt.nodes)]
    N = nt.nodes.new; L = nt.links.new
    out = N('ShaderNodeOutputMaterial'); em = N('ShaderNodeEmission'); tr = N('ShaderNodeBsdfTransparent'); mx = N('ShaderNodeMixShader')
    tc = N('ShaderNodeTexCoord')
    nz = N('ShaderNodeTexNoise'); nz.noise_dimensions = '4D'
    nz.inputs["Scale"].default_value = co_nhieu; nz.inputs["Detail"].default_value = 10; nz.inputs["Roughness"].default_value = 0.6; nz.inputs["W"].default_value = seed
    mp = N('ShaderNodeMapping'); mp.inputs["Scale"].default_value = (rong / cao, 1, 1)
    L(tc.outputs["UV"], mp.inputs["Vector"]); L(mp.outputs["Vector"], nz.inputs["Vector"])
    rp = N('ShaderNodeValToRGB'); rp.color_ramp.elements[0].position = 0.3; rp.color_ramp.elements[0].color = (0, 0, 0, 1)
    rp.color_ramp.elements[1].position = 0.8; rp.color_ramp.elements[1].color = (mau[0], mau[1], mau[2], 1)
    L(nz.outputs["Fac"], rp.inputs["Fac"]); L(rp.outputs["Color"], em.inputs["Color"]); em.inputs["Strength"].default_value = 1.0
    gd = N('ShaderNodeTexGradient'); gd.gradient_type = 'SPHERICAL'
    mp2 = N('ShaderNodeMapping'); mp2.inputs["Location"].default_value = (-1.0, -1.0, 0); mp2.inputs["Scale"].default_value = (2.0, 2.0, 1)  # Mapping: nhan truoc roi cong -> UV 0..1 thanh -1..1
    L(tc.outputs["UV"], mp2.inputs["Vector"]); L(mp2.outputs["Vector"], gd.inputs["Vector"])
    mr = N('ShaderNodeMapRange'); mr.inputs["From Min"].default_value = 0.25; mr.inputs["From Max"].default_value = 0.75
    L(nz.outputs["Fac"], mr.inputs["Value"])
    nh = N('ShaderNodeMath'); nh.operation = 'MULTIPLY'; L(gd.outputs["Fac"], nh.inputs[0]); L(mr.outputs["Result"], nh.inputs[1])
    nh2 = N('ShaderNodeMath'); nh2.operation = 'MULTIPLY'; nh2.inputs[1].default_value = dam; nh2.use_clamp = True; L(nh.outputs[0], nh2.inputs[0])
    L(nh2.outputs[0], mx.inputs[0]); L(tr.outputs[0], mx.inputs[1]); L(em.outputs[0], mx.inputs[2]); L(mx.outputs[0], out.inputs[0])
    o.data.materials.append(m)
    return o

def compositor_bong(sc, nguong=0.3, manh=0.7, co=0.8, mau=(1.0, 0.12, 0.06), he_so_alpha=2.5, bong_px=14, bong_dam=0.8):
    """Nhu compositor() + BONG TOI OM SAT VIEN CHU (lam mo alpha chu, dat duoi chu) - tach chu khoi nen ma khong thanh mang den."""
    ng = bpy.data.node_groups.get("TD2_Comp") or bpy.data.node_groups.new("TD2_Comp", 'CompositorNodeTree')
    for n in list(ng.nodes): ng.nodes.remove(n)
    if not any(i.in_out == 'OUTPUT' for i in ng.interface.items_tree):
        ng.interface.new_socket("Image", in_out='OUTPUT', socket_type='NodeSocketColor')
    N = ng.nodes.new; L = ng.links.new
    rl = N('CompositorNodeRLayers'); rl.scene = sc
    gl = N('CompositorNodeGlare'); gl.name = "Glare"
    gl.inputs["Type"].default_value = 'Fog Glow'; gl.inputs["Quality"].default_value = 'High'
    gl.inputs["Threshold"].default_value = nguong; gl.inputs["Strength"].default_value = manh; gl.inputs["Size"].default_value = co
    gl.inputs["Tint"].default_value = (mau[0], mau[1], mau[2], 1)
    sep = N('CompositorNodeSeparateColor')
    bl = N('CompositorNodeBlur'); bl.inputs["Size"].default_value = (bong_px, bong_px); bl.name = "BongMo"
    sep2 = N('CompositorNodeSeparateColor')
    nhb = N('ShaderNodeMath'); nhb.operation = 'MULTIPLY'; nhb.inputs[1].default_value = bong_dam * 1.6; nhb.use_clamp = True; nhb.name = "BongDam"
    den = N('CompositorNodeRGB'); den.outputs[0].default_value = (0.012, 0.002, 0.002, 1)
    sab = N('CompositorNodeSetAlpha'); sab.inputs["Type"].default_value = 'Replace Alpha'
    ao = N('CompositorNodeAlphaOver')
    sep3 = N('CompositorNodeSeparateColor'); bw = N('CompositorNodeRGBToBW')
    nh = N('ShaderNodeMath'); nh.operation = 'MULTIPLY'; nh.inputs[1].default_value = he_so_alpha; nh.use_clamp = True; nh.name = "HeSoAlpha"
    mx = N('ShaderNodeMath'); mx.operation = 'MAXIMUM'
    sa = N('CompositorNodeSetAlpha'); sa.inputs["Type"].default_value = 'Replace Alpha'
    go = N('NodeGroupOutput')
    L(rl.outputs["Image"], gl.inputs["Image"])
    L(rl.outputs["Image"], bl.inputs["Image"]); L(bl.outputs[0], sep2.inputs["Image"]); L(sep2.outputs["Alpha"], nhb.inputs[0])
    L(den.outputs[0], sab.inputs["Image"]); L(nhb.outputs[0], sab.inputs["Alpha"])
    L(sab.outputs[0], ao.inputs["Background"]); L(gl.outputs["Image"], ao.inputs["Foreground"])
    L(ao.outputs[0], sep3.inputs["Image"])
    L(gl.outputs["Glare"], bw.inputs["Image"]); L(bw.outputs[0], nh.inputs[0])
    L(sep3.outputs["Alpha"], mx.inputs[0]); L(nh.outputs[0], mx.inputs[1])
    L(ao.outputs[0], sa.inputs["Image"]); L(mx.outputs[0], sa.inputs["Alpha"]); L(sa.outputs[0], go.inputs[0])
    sc.compositing_node_group = ng; sc.render.use_compositing = True
