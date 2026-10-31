# Anh mau moi cho quai Quy cay (model Meshy "Horned Shadowfiend") - nguoi dung 01/11/2026: "trang xam toan than qua xau", gui anh quy cay
# long / vo xu xam nau sam, dau soi sang mau be, sung + gay xam be.
# Anh goc Meshy toi (TB 68/255) nhung gan nhu KHONG SAC (bao hoa 0,018, hoi nga xanh) -> trong game thanh xam bac.
# Giu CHI TIET anh goc (do sang tung texel), to lai bang thang mau nau xam -> be cua anh mau, gian tuong phan o dau sang, AO nhe nhuom nau.
# Chay trong Blender MCP (da nap FBX + khung 0):
#   exec(open(r"...quy_cay_mau.py", encoding="utf-8").read()); dung_vat_lieu(anh_goc); nuong_anh(duong_dan)
import bpy

TEN = 'char1'

def _lin(c):
    c = c / 255.0
    return c / 12.92 if c <= 0.04045 else ((c + 0.055) / 1.055) ** 2.4

def _mau(r, g, b):
    return (_lin(r), _lin(g), _lin(b), 1.0)

# Thang mau theo do sang tuyen tinh anh goc (phan vi 5/25/50/75/95 = 0,014 / 0,040 / 0,062 / 0,085 / 0,135)
THANG = [(0.014, _mau(20, 16, 13)), (0.040, _mau(48, 40, 33)), (0.062, _mau(76, 66, 56)),
         (0.085, _mau(102, 90, 77)), (0.135, _mau(140, 128, 112)), (0.20, _mau(172, 160, 142))]
NAU_AO = _mau(150, 110, 82)


def dung_vat_lieu(anh_goc):
    o = bpy.data.objects[TEN]
    mat = bpy.data.materials.get('QC_Moi') or bpy.data.materials.new('QC_Moi')
    mat.use_nodes = True
    nt = mat.node_tree; nt.nodes.clear(); L = nt.links.new
    N = lambda t: nt.nodes.new(t)
    img = bpy.data.images.get('QC_Goc') or bpy.data.images.load(anh_goc)
    img.name = 'QC_Goc'
    tex = N('ShaderNodeTexImage'); tex.image = img
    bw = N('ShaderNodeRGBToBW'); L(tex.outputs[0], bw.inputs[0])
    ramp = N('ShaderNodeValToRGB')
    el = ramp.color_ramp.elements
    el[0].position, el[0].color = THANG[0]
    el[1].position, el[1].color = THANG[-1]
    for pos, c in THANG[1:-1]:
        e = el.new(pos); e.color = c
    L(bw.outputs[0], ramp.inputs[0])
    ao = N('ShaderNodeAmbientOcclusion'); ao.samples = 16; ao.inputs['Distance'].default_value = 0.05
    aoR = N('ShaderNodeMapRange'); aoR.inputs[1].default_value = 0.45; aoR.inputs[2].default_value = 1.0
    aoR.inputs[3].default_value = 0.80; aoR.inputs[4].default_value = 1.0
    L(ao.outputs['AO'], aoR.inputs[0])
    nauAO = N('ShaderNodeMix'); nauAO.data_type = 'RGBA'
    nauAO.inputs[6].default_value = NAU_AO; nauAO.inputs[7].default_value = (1, 1, 1, 1)
    L(aoR.outputs[0], nauAO.inputs[0])
    nhan = N('ShaderNodeMix'); nhan.data_type = 'RGBA'; nhan.blend_type = 'MULTIPLY'; nhan.inputs[0].default_value = 1.0
    L(ramp.outputs[0], nhan.inputs[6]); L(nauAO.outputs[2], nhan.inputs[7])
    em = N('ShaderNodeEmission'); L(nhan.outputs[2], em.inputs[0])
    out = N('ShaderNodeOutputMaterial'); L(em.outputs[0], out.inputs[0])
    o.data.materials.clear(); o.data.materials.append(mat)
    return mat


def nuong_anh(duong_dan, co=2048):
    o = bpy.data.objects[TEN]
    nt = bpy.data.materials['QC_Moi'].node_tree
    img = bpy.data.images.get('QC_Mau')
    if img: bpy.data.images.remove(img)
    img = bpy.data.images.new('QC_Mau', co, co, alpha=False, float_buffer=False)
    img.colorspace_settings.name = 'sRGB'
    ni = nt.nodes.get('QC_Dich') or nt.nodes.new('ShaderNodeTexImage'); ni.name = 'QC_Dich'; ni.image = img
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
