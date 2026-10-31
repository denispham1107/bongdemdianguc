# Anh mau moi cho quai Quy du (model Meshy "Abyssal Horned Demon") - nguoi dung 01/11/2026: "trang xam toan than qua xau", gui anh quy
# da xam be am, ranh co nau sam, tay / chan nau toi hon, sung nau xam; "them nhieu vet mau loang lo tren phan than tren".
# Giu CHI TIET cua anh goc Meshy (do sang toi tung texel) nhung TO LAI bang thang mau cua anh mau, toi ranh bang AO, nhuom tay / chan /
# sung theo trong so xuong + vi tri, roi phu vet mau (nhieu 3D theo vi tri the gioi o khung 0) chi o than tren.
# Chay trong Blender MCP (da nap FBX + chon khung 0):
#   exec(open(r"...quy_du_mau.py", encoding="utf-8").read()); dung_vat_lieu(anh_goc); nuong_anh(duong_dan)
import bpy, math

TEN = 'char1'

def _lin(c):
    c = c / 255.0
    return c / 12.92 if c <= 0.04045 else ((c + 0.055) / 1.055) ** 2.4

def _mau(r, g, b):
    return (_lin(r), _lin(g), _lin(b), 1.0)

# Thang mau theo do sang tuyen tinh cua anh goc (phan vi 5/25/50/75/95 = 0,032 / 0,074 / 0,139 / 0,228 / 0,352)
THANG = [(0.03, _mau(48, 32, 23)), (0.075, _mau(88, 66, 51)), (0.14, _mau(128, 111, 97)),
         (0.23, _mau(164, 148, 132)), (0.36, _mau(196, 182, 165))]
NAU_TOI = _mau(98, 70, 52)        # nhan tay / chan / mong vuot
SUNG = [(0.0, _mau(28, 20, 16)), (0.5, _mau(78, 62, 50)), (1.0, _mau(140, 122, 104))]
MAU_TUOI = _mau(122, 10, 8)
MAU_KHO = _mau(52, 7, 6)
MAU_LOANG = _mau(96, 26, 18)   # quang mau loang mo quanh vet
CAO_HONG = 0.95       # than tren: tu day len
CAO_CO = 1.42       # toi cam - khong len dau / sung


def mat_na():
    """Thuoc tinh dinh 'MatNaQD': R = tay / chan (dau chi), G = sung, B = vung than tren duoc phep co mau."""
    o = bpy.data.objects[TEN]
    me = o.data
    dg = bpy.context.evaluated_depsgraph_get()
    ev = o.evaluated_get(dg); m2 = ev.to_mesh()
    P = [o.matrix_world @ v.co for v in m2.vertices]
    ev.to_mesh_clear()
    gi = {g.name: g.index for g in o.vertex_groups}
    def w(v, ten):
        i = gi.get(ten)
        if i is None: return 0.0
        for g in v.groups:
            if g.group == i: return g.weight
        return 0.0
    vals = []
    for v in me.vertices:
        p = P[v.index]
        tay = w(v, 'LeftHand') + w(v, 'RightHand')
        chan = w(v, 'LeftFoot') + w(v, 'RightFoot') + w(v, 'LeftToeBase') + w(v, 'RightToeBase')
        # cang chan duoi toi dan (anh mau: ong chan nau hon dui)
        cang = max(0.0, min(1.0, (0.36 - p.z) / 0.20))
        r = max(0.0, min(1.0, tay * 0.9 + chan + cang * 0.7))
        dau = w(v, 'Head')
        # sung: hai sung cong ra hai ben + mao giua dinh dau
        sung = 0.0
        if dau > 0.5 and p.z > 1.50:
            sung = max(0.0, min(1.0, (abs(p.x) - 0.075) / 0.04)) if p.z < 1.66 else 1.0
            if abs(p.x) < 0.03 and p.z > 1.60: sung = 1.0
        than = 1.0 if CAO_HONG <= p.z <= CAO_CO else max(0.0, 1.0 - min(abs(p.z - CAO_HONG), abs(p.z - CAO_CO)) / 0.08)
        than *= max(0.0, 1.0 - tay * 1.5) * (1.0 - sung)
        vals.append((r, sung, than))
    a = me.color_attributes.get('MatNaQD') or me.color_attributes.new('MatNaQD', 'FLOAT_COLOR', 'POINT')
    for i, (r, g, b) in enumerate(vals):
        a.data[i].color = (r, g, b, 1.0)
    return vals


def dung_vat_lieu(anh_goc):
    o = bpy.data.objects[TEN]
    mat_na()
    mat = bpy.data.materials.get('QD_Moi') or bpy.data.materials.new('QD_Moi')
    mat.use_nodes = True
    nt = mat.node_tree; nt.nodes.clear(); L = nt.links.new
    N = lambda t: nt.nodes.new(t)
    img = bpy.data.images.get('QD_Goc') or bpy.data.images.load(anh_goc)
    img.name = 'QD_Goc'
    tex = N('ShaderNodeTexImage'); tex.image = img
    bw = N('ShaderNodeRGBToBW'); L(tex.outputs[0], bw.inputs[0])
    ramp = N('ShaderNodeValToRGB')
    el = ramp.color_ramp.elements
    while len(el) > 2: el.remove(el[-1])
    el[0].position, el[0].color = THANG[0]
    el[1].position, el[1].color = THANG[1]
    for pos, c in THANG[2:]:
        e = el.new(pos); e.color = c
    L(bw.outputs[0], ramp.inputs[0])
    mn = N('ShaderNodeVertexColor'); mn.layer_name = 'MatNaQD'
    sep = N('ShaderNodeSeparateColor'); L(mn.outputs[0], sep.inputs[0])
    # AO: ranh co, nach, khe ngon toi + am nau
    ao = N('ShaderNodeAmbientOcclusion'); ao.samples = 16; ao.inputs['Distance'].default_value = 0.04
    aoR = N('ShaderNodeMapRange'); aoR.inputs[1].default_value = 0.45; aoR.inputs[2].default_value = 1.0
    aoR.inputs[3].default_value = 0.78; aoR.inputs[4].default_value = 1.0
    L(ao.outputs['AO'], aoR.inputs[0])
    nauAO = N('ShaderNodeMix'); nauAO.data_type = 'RGBA'
    nauAO.inputs[6].default_value = _mau(150, 100, 72); nauAO.inputs[7].default_value = (1, 1, 1, 1)
    L(aoR.outputs[0], nauAO.inputs[0])
    nhanAO = N('ShaderNodeMix'); nhanAO.data_type = 'RGBA'; nhanAO.blend_type = 'MULTIPLY'; nhanAO.inputs[0].default_value = 1.0
    L(ramp.outputs[0], nhanAO.inputs[6]); L(nauAO.outputs[2], nhanAO.inputs[7])
    # tay / chan nau toi
    tc = N('ShaderNodeMix'); tc.data_type = 'RGBA'; tc.blend_type = 'MULTIPLY'
    tc.inputs[7].default_value = (NAU_TOI[0] / THANG[2][1][0], NAU_TOI[1] / THANG[2][1][1], NAU_TOI[2] / THANG[2][1][2], 1)
    L(sep.outputs[0], tc.inputs[0]); L(nhanAO.outputs[2], tc.inputs[6])
    # sung
    sr = N('ShaderNodeValToRGB'); e2 = sr.color_ramp.elements
    e2[0].position, e2[0].color = SUNG[0]; e2[1].position, e2[1].color = SUNG[2]
    e = e2.new(SUNG[1][0]); e.color = SUNG[1][1]
    L(bw.outputs[0], sr.inputs[0])
    sm = N('ShaderNodeMix'); sm.data_type = 'RGBA'
    L(sep.outputs[1], sm.inputs[0]); L(tc.outputs[2], sm.inputs[6]); L(sr.outputs[0], sm.inputs[7])
    # ---- vet mau loang lo than tren ----
    geo = N('ShaderNodeNewGeometry')
    # dang vet: nhieu 3D bien dang + voronoi; vet chay xuong: keo dai theo z
    keo = N('ShaderNodeMapping'); keo.inputs['Scale'].default_value = (1.0, 1.0, 0.45)
    L(geo.outputs['Position'], keo.inputs[0])
    n1 = N('ShaderNodeTexNoise'); n1.inputs['Scale'].default_value = 7.0; n1.inputs['Detail'].default_value = 8.0
    n1.inputs['Roughness'].default_value = 0.62; n1.inputs['Distortion'].default_value = 0.6
    L(keo.outputs[0], n1.inputs['Vector'])
    vo = N('ShaderNodeTexVoronoi'); vo.inputs['Scale'].default_value = 9.0
    L(geo.outputs['Position'], vo.inputs['Vector'])
    vr = N('ShaderNodeMapRange'); vr.inputs[1].default_value = 0.0; vr.inputs[2].default_value = 0.55
    vr.inputs[3].default_value = 0.22; vr.inputs[4].default_value = -0.12
    L(vo.outputs['Distance'], vr.inputs[0])
    cong = N('ShaderNodeMath'); cong.operation = 'ADD'
    L(n1.outputs['Fac'], cong.inputs[0]); L(vr.outputs[0], cong.inputs[1])
    # nguong -> mat na vet mau (mep mem 0,03)
    ng = N('ShaderNodeMapRange'); ng.inputs[1].default_value = 0.47; ng.inputs[2].default_value = 0.50
    L(cong.outputs[0], ng.inputs[0])
    # quang loang: nguong thap hon, chi phu 45%
    ng2 = N('ShaderNodeMapRange'); ng2.inputs[1].default_value = 0.40; ng2.inputs[2].default_value = 0.47
    ng2.inputs[3].default_value = 0.0; ng2.inputs[4].default_value = 0.45
    L(cong.outputs[0], ng2.inputs[0])
    lonNhat = N('ShaderNodeMath'); lonNhat.operation = 'MAXIMUM'
    L(ng.outputs[0], lonNhat.inputs[0]); L(ng2.outputs[0], lonNhat.inputs[1])
    vung = N('ShaderNodeMath'); vung.operation = 'MULTIPLY'
    L(lonNhat.outputs[0], vung.inputs[0]); L(sep.outputs[2], vung.inputs[1])
    # mau tuoi o long vet, kho tham o mep + vung nhieu
    n2 = N('ShaderNodeTexNoise'); n2.inputs['Scale'].default_value = 22.0; n2.inputs['Detail'].default_value = 4.0
    L(geo.outputs['Position'], n2.inputs['Vector'])
    loi = N('ShaderNodeMapRange'); loi.inputs[1].default_value = 0.52; loi.inputs[2].default_value = 0.68
    L(cong.outputs[0], loi.inputs[0])
    tron = N('ShaderNodeMath'); tron.operation = 'MULTIPLY'
    L(loi.outputs[0], tron.inputs[0]); L(n2.outputs['Fac'], tron.inputs[1])
    tron2 = N('ShaderNodeMapRange'); tron2.inputs[1].default_value = 0.15; tron2.inputs[2].default_value = 0.55
    L(tron.outputs[0], tron2.inputs[0])
    mauLoi = N('ShaderNodeMix'); mauLoi.data_type = 'RGBA'
    mauLoi.inputs[6].default_value = MAU_KHO; mauLoi.inputs[7].default_value = MAU_TUOI
    L(tron2.outputs[0], mauLoi.inputs[0])
    mauMix = N('ShaderNodeMix'); mauMix.data_type = 'RGBA'
    mauMix.inputs[6].default_value = MAU_LOANG
    L(ng.outputs[0], mauMix.inputs[0]); L(mauLoi.outputs[2], mauMix.inputs[7])
    # mau tham vao da: giu mot chut chi tiet anh goc (nhan voi do sang)
    chiTiet = N('ShaderNodeMapRange'); chiTiet.inputs[1].default_value = 0.03; chiTiet.inputs[2].default_value = 0.30
    chiTiet.inputs[3].default_value = 0.65; chiTiet.inputs[4].default_value = 1.25
    L(bw.outputs[0], chiTiet.inputs[0])
    mauCT = N('ShaderNodeMix'); mauCT.data_type = 'RGBA'; mauCT.blend_type = 'MULTIPLY'; mauCT.inputs[0].default_value = 1.0
    L(mauMix.outputs[2], mauCT.inputs[6]); L(chiTiet.outputs[0], mauCT.inputs[7])
    phu = N('ShaderNodeMix'); phu.data_type = 'RGBA'
    L(vung.outputs[0], phu.inputs[0]); L(sm.outputs[2], phu.inputs[6]); L(mauCT.outputs[2], phu.inputs[7])
    em = N('ShaderNodeEmission'); L(phu.outputs[2], em.inputs[0])
    out = N('ShaderNodeOutputMaterial'); L(em.outputs[0], out.inputs[0])
    o.data.materials.clear(); o.data.materials.append(mat)
    return mat


def nuong_anh(duong_dan, co=2048):
    o = bpy.data.objects[TEN]
    mat = bpy.data.materials['QD_Moi']
    nt = mat.node_tree
    img = bpy.data.images.get('QD_Mau')
    if img: bpy.data.images.remove(img)
    img = bpy.data.images.new('QD_Mau', co, co, alpha=False, float_buffer=False)
    img.colorspace_settings.name = 'sRGB'
    ni = nt.nodes.get('QD_Dich') or nt.nodes.new('ShaderNodeTexImage'); ni.name = 'QD_Dich'; ni.image = img
    nt.nodes.active = ni
    sc = bpy.context.scene
    try: sc.render.engine = 'CYCLES'
    except TypeError as e: print(e)
    sc.cycles.samples = 16
    sc.cycles.bake_type = 'EMIT'
    sc.render.bake.margin = 16
    sc.render.bake.use_selected_to_active = False
    for ob in bpy.context.selected_objects: ob.select_set(False)
    o.select_set(True); bpy.context.view_layer.objects.active = o
    bpy.ops.object.bake(type='EMIT')
    img.filepath_raw = duong_dan; img.file_format = 'PNG'
    img.save()
    return img
