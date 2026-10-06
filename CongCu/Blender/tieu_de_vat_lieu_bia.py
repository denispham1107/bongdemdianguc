# Vat lieu DA BIA MO cho chu tieu de trang Loading (chay trong Blender qua MCP). Anh da / gan / mat na lay tu
# chinh vat lieu Act2_DaBia cua game (Diablo25D/DaMoTriplanar). Mau = thuoc tinh "LaDa" (1 = da, 0 = giot mau).
import bpy
P = dict(ti_le=0.75, sac=(0.52, 0.52, 0.50), reu=0.10, moc=0.12, nut_co=9.0, nut_day=0.022, nut_vung=(0.42, 0.55),
         mau_B=(0.5, 0.85), chay_nguong=0.92, mau_nut=(0.5, 0.6), gan=1.2, san=0.45)
P.update(globals().get("THAM_SO", {}))
D = r"C:\Users\HP\Documents\GameUnity\Diablo25D\Assets\BlenderMaps\GraveyardAct2\CayDaChua" + "\\"
def anh(ten, khong_mau):
    im = bpy.data.images.load(D + ten, check_existing=True)
    if khong_mau: im.colorspace_settings.name = 'Non-Color'
    return im
m = bpy.data.materials.get("TD2C_DaMo") or bpy.data.materials.new("TD2C_DaMo"); m.use_nodes = True
nt = m.node_tree; [nt.nodes.remove(n) for n in list(nt.nodes)]
N = nt.nodes.new; L = nt.links.new
out = N('ShaderNodeOutputMaterial'); mx = N('ShaderNodeMixShader'); tc = N('ShaderNodeTexCoord')
mp = N('ShaderNodeMapping'); mp.inputs["Scale"].default_value = (P["ti_le"],) * 3; L(tc.outputs["Object"], mp.inputs["Vector"])
def tex(ten, km):
    t = N('ShaderNodeTexImage'); t.image = anh(ten, km); t.projection = 'BOX'; t.projection_blend = 0.3
    L(mp.outputs["Vector"], t.inputs["Vector"]); return t
tMau = tex("DaMo_Mau.png", False); tGan = tex("DaMo_Gan.png", True); tMn = tex("DaMo_MatNa.png", True)
def nhan(a, b, kep=False):
    k = N('ShaderNodeMath'); k.operation = 'MULTIPLY'; k.use_clamp = kep
    (L(a, k.inputs[0]) if not isinstance(a, float) else k.inputs.__getitem__(0).__setattr__('default_value', a))
    (L(b, k.inputs[1]) if not isinstance(b, float) else k.inputs.__getitem__(1).__setattr__('default_value', b))
    return k.outputs[0]
def khoang(sock, a, b, c=0.0, d=1.0):
    r = N('ShaderNodeMapRange'); r.inputs["From Min"].default_value = a; r.inputs["From Max"].default_value = b
    r.inputs["To Min"].default_value = c; r.inputs["To Max"].default_value = d; L(sock, r.inputs["Value"]); return r.outputs["Result"]
def tron(c_in, mau, fac):
    mi = N('ShaderNodeMix'); mi.data_type = 'RGBA'; L(fac, mi.inputs["Factor"]); L(c_in, mi.inputs[6]); mi.inputs[7].default_value = mau + (1,)
    return mi.outputs[2]
def nhieu(co, chi=4):
    n = N('ShaderNodeTexNoise'); n.inputs["Scale"].default_value = co; n.inputs["Detail"].default_value = chi; L(tc.outputs["Object"], n.inputs["Vector"]); return n
sac = N('ShaderNodeMix'); sac.data_type = 'RGBA'; sac.blend_type = 'MULTIPLY'; sac.inputs["Factor"].default_value = 1.0
sac.inputs[7].default_value = P["sac"] + (1,); L(tMau.outputs["Color"], sac.inputs[6])
sep = N('ShaderNodeSeparateColor'); L(tMn.outputs["Color"], sep.inputs["Color"])
c = tron(sac.outputs[2], (0.16, 0.26, 0.11), nhan(sep.outputs["Red"], P["reu"], True))
c = tron(c, (0.60, 0.62, 0.55), nhan(sep.outputs["Green"], P["moc"], True))
# vet nut
wn = nhieu(4.0)
wv = N('ShaderNodeVectorMath'); wv.operation = 'SCALE'; wv.inputs["Scale"].default_value = 0.12; L(wn.outputs["Color"], wv.inputs[0])
wa = N('ShaderNodeVectorMath'); wa.operation = 'ADD'; L(tc.outputs["Object"], wa.inputs[0]); L(wv.outputs[0], wa.inputs[1])
vo = N('ShaderNodeTexVoronoi'); vo.feature = 'DISTANCE_TO_EDGE'; vo.inputs["Scale"].default_value = P["nut_co"]; L(wa.outputs[0], vo.inputs["Vector"])
nut = nhan(khoang(vo.outputs["Distance"], 0.0, P["nut_day"], 1.0, 0.0), khoang(nhieu(2.2).outputs["Fac"], *P["nut_vung"]))
c = tron(c, (0.025, 0.02, 0.02), nut)
# vet mau: mang B cua mat na + vet chay doc tu dinh + mau dong trong vai vet nut
sx = N('ShaderNodeSeparateXYZ'); L(tc.outputs["Object"], sx.inputs[0])
vsm = N('ShaderNodeMapping'); vsm.inputs["Scale"].default_value = (30, 1.5, 1); L(tc.outputs["Object"], vsm.inputs["Vector"])
vsn = N('ShaderNodeTexNoise'); vsn.inputs["Scale"].default_value = 1.0; vsn.inputs["Detail"].default_value = 3; L(vsm.outputs["Vector"], vsn.inputs["Vector"])
ch = N('ShaderNodeMath'); ch.operation = 'MULTIPLY_ADD'; ch.inputs[1].default_value = 0.75
L(khoang(sx.outputs["Y"], -0.14, 0.5), ch.inputs[0]); L(vsn.outputs["Fac"], ch.inputs[2])
chay = khoang(ch.outputs[0], P["chay_nguong"], 1.0)
mB = khoang(sep.outputs["Blue"], *P["mau_B"])
m1 = N('ShaderNodeMath'); m1.operation = 'MAXIMUM'; L(mB, m1.inputs[0]); L(chay, m1.inputs[1])
mt = N('ShaderNodeMath'); mt.operation = 'MAXIMUM'; mt.use_clamp = True; L(m1.outputs[0], mt.inputs[0])
L(nhan(nut, khoang(nhieu(3.5).outputs["Fac"], *P["mau_nut"])), mt.inputs[1])
c = tron(c, (0.26, 0.02, 0.016), mt.outputs[0])
da = N('ShaderNodeBsdfPrincipled'); L(c, da.inputs["Base Color"])
L(khoang(mt.outputs[0], 0.0, 1.0, 0.88, 0.3), da.inputs["Roughness"])
nmap = N('ShaderNodeNormalMap'); nmap.inputs["Strength"].default_value = P["gan"]; L(tGan.outputs["Color"], nmap.inputs["Color"])
hh = N('ShaderNodeMath'); hh.operation = 'SUBTRACT'; L(nhieu(60, 10).outputs["Fac"], hh.inputs[0]); L(nut, hh.inputs[1])
bp = N('ShaderNodeBump'); bp.inputs["Strength"].default_value = P["san"]
L(hh.outputs[0], bp.inputs["Height"]); L(nmap.outputs["Normal"], bp.inputs["Normal"]); L(bp.outputs["Normal"], da.inputs["Normal"])
mau = N('ShaderNodeBsdfPrincipled')
crp = N('ShaderNodeValToRGB'); crp.color_ramp.elements[0].position = 0.42; crp.color_ramp.elements[0].color = (0.018, 0, 0.001, 1)
crp.color_ramp.elements[1].position = 0.62; crp.color_ramp.elements[1].color = (0.13, 0, 0.004, 1)
L(nhieu(9).outputs["Fac"], crp.inputs["Fac"]); L(crp.outputs["Color"], mau.inputs["Base Color"])
mau.inputs["Roughness"].default_value = 0.08; mau.inputs["Coat Weight"].default_value = 1.0; mau.inputs["Coat Roughness"].default_value = 0.04
mau.inputs["Subsurface Weight"].default_value = 0.3; mau.inputs["Subsurface Radius"].default_value = (0.5, 0.03, 0.01)
at = N('ShaderNodeAttribute'); at.attribute_type = 'GEOMETRY'; at.attribute_name = "LaDa"
L(at.outputs["Fac"], mx.inputs[0]); L(mau.outputs[0], mx.inputs[1]); L(da.outputs[0], mx.inputs[2]); L(mx.outputs[0], out.inputs[0])
o = bpy.data.objects["TD2C_Chu"]; o.data.materials.clear(); o.data.materials.append(m)
print("vat lieu da bia:", P)
