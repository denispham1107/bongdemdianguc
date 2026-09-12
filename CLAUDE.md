# Diablo 2.5D — bản đồ dự án

Game nhập vai hành động kiểu Diablo, Unity 6 (6000.5.6f1), **Built-in Render Pipeline**.
Chơi được trên PC và trên **điện thoại (cảm ứng / WebGL)** — điện thoại là nền tảng chính,
mọi quyết định về hiệu năng đều lấy nó làm chuẩn.

## Đọc gì trước khi làm gì

| Muốn biết | Đọc |
|---|---|
| **Vì sao** một thứ được làm như hiện tại, số đo, những lần đã đi sai đường | `HUONG-DAN.md` (25 mục lớn, ~278 KB) — `grep -n "^## " HUONG-DAN.md` để xem mục lục |
| Những cái bẫy đã vấp và cách tránh | file trong thư mục memory (tự nạp mỗi phiên qua `MEMORY.md`) |
| Cách một hệ thống hoạt động | comment đầu file `.cs` — mọi file đều có phần tóm tắt và **lý do** ở đầu |
| Kết quả đo và ảnh chụp của lần chạy thử gần nhất | `PlayTestShots/` |

> `HUONG-DAN.md` là **nhật ký kỹ thuật viết cho người dùng đọc**, không phải tài liệu API.
> Mỗi mục kể: hiện tượng người dùng thấy → nguyên nhân thật → cách sửa → **số đo chứng minh**.
> Sửa xong một việc đáng kể thì ghi thêm vào đó, kèm số đo.

## Ngôn ngữ

- **Trả lời người dùng bằng tiếng Việt**, kể cả phần suy nghĩ.
- **Comment trong code bằng tiếng Việt KHÔNG DẤU** (`// Ban kinh dam lua duoi goc`).
  Tên biến, tên hàm, tên file cũng vậy: `banKinhLua`, `CayChay`, `VatTheBiCuon`.
- `HUONG-DAN.md` thì viết tiếng Việt **có dấu**.
- ⚠️ **Mọi chữ HIỂN THỊ cho người chơi phải là tiếng Việt CÓ DẤU, và font phải hiển thị đủ — không mất chữ, mất
  dấu, lỗi font** (người dùng yêu cầu nhiều lần). Mọi nơi: HUD, sảnh, cài đặt, tên trên đầu, trang Loading, trang
  quản trị, ảnh chữ. Unity: font Inter qua `GiaoDien.ChuThuong/ChuDam` (GUIStyle từ `GUI.skin` phải gán `.font`);
  web: nhúng Inter kèm trang; font trang trí: đọc cmap (`BangKyTuFont.Doc`) đủ 134 chữ có dấu trước khi dùng;
  tên người chơi gõ: `GhepDauTiengViet.Ghep`. HUD trong trận đã có dấu (menu 53 quét chuỗi); chỗ nào còn sót thì báo và đề nghị sửa.

## Cấu trúc

```
Assets/Scenes/     Act1.unity, Act2.unity, MainMenu.unity
Assets/Scripts/
   GameBootstrap   dung canh luc chay (Act1 kieu cu)
   GameDirector    rai quai, dem dot, thang/thua
   Player/         PlayerController (thi hanh), DocInput (doc phim), GoiInput,
                   NguoiChoiHoatHinh
   Skills/         Fireball, IceStorm, LightningStorm, Tornado, ThienThach,
                   Khieng, GiatSet, VungLua, VatTheBiCuon, CayChay
   Combat/         Damageable + CombatUtil, cac hieu ung trang thai,
                   DamagePopup + VeSoSatThuong (so sat thuong ve bang OnGUI)
   Vfx/            VfxFactory (3 500 dong, partial) + cac component hieu ung
   Art/            ProcMesh, WorldFactory, TextureFactory, MaterialLibrary (Mats)
   Cam/            CameraRig
   UI/             GameHUD (OnGUI), CamUng, ChiBaoNgam, MayNgam,
                   ManDangNhap, ManSanh (dang nhap + sanh phong, deu OnGUI),
                   GiaoDien (bo giao dien kinh di dung chung + font Inter)
   Mang/           FirebaseMang (cau REST), HoSoMang, PhongMang, TranHienTai,
                   KenhTrucTiep (WebRTC), BatTay, DuDoan, TrangThaiNhanVat,
                   NguoiChoiKhac, GoiTin (nhi phan), NoiSuy, DongBoTran,
                   DongBoQuai, NhanDangQuai, LichSuViTri, BuTre, HieuUngQuaMang
web/               ban WebGL da xuat + /quantri/ (trang admin)
Assets/WebGLTemplates/Diablo25D/   trang bao quanh game tren web (full man hinh)
Assets/Editor/     cong cu menu "Diablo 2.5D" + cac kich ban chay thu
Assets/Shaders/    17 shader viet tay (S_*.shader)
Assets/Resources/  thu nap luc chay (DiemLua/ - diem moi lua cua cay)
Assets/BlenderMaps/GraveyardAct2/   ban do Act2 dung trong Blender
```

Hai màn dựng theo **hai cách khác hẳn nhau** — sửa gì cũng phải thử cả hai:

| | Act1 | Act2 |
|---|---|---|
| Nguồn | dựng bằng code (`WorldFactory`), rồi **bake** vào scene | vẽ trong Blender, nhập qua `map_luoi.fbx` |
| Một cái cây | 88 renderer con (`Trunk`, `Branch0`, `Leaves`…), lưới đọc được | 1 renderer, lưới **Read/Write TẮT** |
| Tên cây | `CayXanh_19`, `CayChet_11` (trong scene) / `Tree7` (trong code) | `TREE_oakA_bare_701` |
| Vật liệu vỏ cây | `M_Bark` (Standard) | `Act2_VoCay_SanSui` (`Diablo25D/BarkTriplanar`) |

## Công cụ

**Unity MCP** — chạy code C# trong Editor. Tham số phải **viết hoa**: `Code`, `Title`; nội dung
phải là `internal class CommandScript : IRunCommand { public void Execute(ExecutionResult result) }`.
Gọi sai tên tham số thì báo `No logs available`, nhìn hệt Unity treo. Sandbox **cấm Reflection**
và các namespace lạ; đoạn code dài dễ bị `COMPILATION_FAILED` với log rỗng — dài thì viết thành
script Editor có menu rồi gọi.

**Blender MCP** — có, giao diện tiếng Việt (tên modifier/node bị dịch, phải tra theo type).
⚠️ Khi người dùng bảo "dựng/thiết kế bằng Blender (MCP)": họ **đã mở sẵn Blender và kết nối MCP** — dựng
**trong Blender đó qua MCP**. Không thấy kết nối thì **dừng và bảo người dùng mở Blender MCP**; không tự chạy
`blender -b -P` để thiết kế bằng code ở ngoài. Blender MCP **render được**: `bpy.ops.render.render(write_still=True)`
ghi ảnh đúng (chỉ `Render Result` báo 0×0, đừng tin nó); ảnh rỗng thật thì kiểm `render.use_compositing`.
Việc lâu (nướng mô phỏng, render nhiều khung) chạy bằng `bpy.app.timers` rồi đợi file ra từ Bash — gọi thẳng
thì lệnh MCP hết giờ.

> **Menu 3 (Self Test) THAY scene đang mở** bằng một scene thử đầy `TEST_Vfx`, `Fireball`,
> `Tornado`. Nó không lưu nên đĩa vẫn sạch, nhưng chạy xong phải
> `EditorSceneManager.OpenScene("Assets/Scenes/Act2.unity", Single)` trả lại ngay — không thì
> người dùng quay lại Unity thấy màn của mình biến mất.

**Menu `Diablo 2.5D`** trong Unity (56 mục): 1 nướng asset + dựng màn, 3 tự kiểm tra,
4 chạy thử & chụp hình, 14 chống ô vuông đen, 15–17 ảnh vỏ cây / đá mộ / sứt mẻ bia,
18–18d chạy thử cây cháy, 19 hồi sinh sau lốc, 20 nướng điểm mồi lửa, 21 nút khoá góc nhìn,
22 thanh kỹ năng bản PC, **26 chạy thử mạng (sảnh phòng), 27 chạy thử khoá tài khoản, 28 chụp màn đăng nhập·sảnh, 29 xuất bản WebGL, 30/30b bơm input Act2·Act1 (bước 1), 31 dự đoán & hiệu chỉnh (bước 2), 32 nhiều người một cảnh (bước 3), 33 nội suy (bước 4), 34 PvP (bước 5), 35 ghép phòng cùng màn, 36 tự gắn bộ nối mạng, 37 kỹ năng qua mạng, 38 quái chung & bù trễ, 39 đòn của quái qua mạng, 40 máu khởi đầu, 41 nhịp bước qua mạng, 42 chế độ điều khiển, 43 kiểm toán bước 5, 44 sửa bước 5, 45 bốn người, 46 hiệu ứng qua mạng, 47 chế độ bốn bộ xương, 48 cài đặt đồ hoạ, 49 cầu lửa trúng người·khiên, 50 giao diện đăng nhập·sảnh·phòng, 51/51b/51c màn chính từ Act2 (dựng · chọn góc · chụp lò lửa), 52 tên trên đầu nhân vật, 53 HUD kinh dị trong trận, 54/54b/54c mười lò lửa Act2 (đặt · chạy thử · lốc xoáy cuốn lò), 55 kết trận (người sống sót cuối cùng), 56 đợt quái Act2 · chỗ xuất phát**.
Bảng đầy đủ nằm ở mục "Phần 4" trong `HUONG-DAN.md`.

## Quy tắc làm việc (rút ra từ những lần đã sai)

1. **Đo, đừng đoán.** Sửa xong phải vào Play mode đo bằng số rồi mới báo là xong. Ảnh chụp
   là bằng chứng phụ, con số mới là bằng chứng chính.
2. **Phép kiểm phải độc lập với giả thiết đang sai** — nếu dùng lại chính công thức nghi sai
   để kiểm tra thì nó sẽ luôn báo "đúng".
3. **Đồng hồ trong Play là `Time.time`**, không phải `Time.realtimeSinceStartup`: mấy khung
   đầu `deltaTime` bị kẹp ở 0,333 s nên đồng hồ thật chạy nhanh gấp mấy lần.
4. **`fps` đo trong Editor không so được giữa hai lần chạy** (nền nhảy 15,0 → 6,7 dù cảnh y
   hệt). So bằng số hạt, số renderer, số đèn.
5. **Prefab đè lên giá trị trong code.** Sửa mặc định trong script mà quên prefab thì trông
   hệt một lỗi logic.
6. **Script báo "đã sửa" không chứng minh file đã ghi** — đọc lại file từ đĩa mới biết.
7. **Dọn sạch sau khi đo**: thoát Play, kiểm `scene.isDirty == false`, không để lại vật thể
   rác trong scene người dùng đang mở.
8. Đổi vật liệu/đèn **trong cùng một lệnh MCP** với `cam.Render()` thì không kịp có tác dụng —
   phải tách hai lệnh.

## Trạng thái hiện tại

- Act2 sạch: **1011 vật thể**, 0 ô vật liệu hỏng, camera `near = 1.50`.
- **Máu người chơi: 600** (12/09/2026 người dùng chốt; trước đó 30 000 để chạy thử). Con số nằm ở bốn chỗ —
  `GameBootstrap.playerMaxHealth`, hai scene, `Player_Sorceress.prefab` — và menu 40 kiểm cả bốn (kể cả bản sao
  người chơi khác, vì nó lấy máu thẳng từ prefab).
- **Act2 dùng LUẬT ĐỢT QUÁI RIÊNG** (`GameDirector.CheDoDotQuanhNguoi`, bật theo tên scene): mỗi đợt sinh quanh **từng**
  người chơi bốn con (bộ xương · phù thủy · quỷ cây · quỷ dữ), giết hết → 30 giây → đợt sau cộng dồn thêm quái bất kì
  (+1, +3, +6…) và mạnh thêm 5% máu · sát thương mỗi đợt. Act2 KHÔNG còn rải quái sẵn, không dòng quỷ dữ/quỷ cây riêng,
  không chế độ bốn bộ xương. Menu 56 kiểm. Chỗ xuất phát của mỗi người: ngẫu nhiên theo **mã phòng** (`ChoXuatPhat.cs`),
  cách nhau ≥ 22 m — mọi máy tính ra cùng một danh sách nên không cần gói tin nào.
- ⚠️ **Chế độ chạy thử "bốn bộ xương" (chỉ còn Act1) đang BẬT** (`GameDirector.CheDoBonBoXuong = true`, hằng số
  trong code): vào màn chỉ 4 bộ xương, giết hết đợi 30 giây ra 4 con mới. Menu 47 kiểm.
  **Tắt trước khi phát hành** — không thì game chỉ còn bốn con bộ xương.
- Bảy kỹ năng chạy được: Cầu lửa, Mưa băng, Sấm sét, Lốc xoáy, Thiên thạch, Khiên, Giựt sét.
- ⚠️ **Chữ tiếng Việt có dấu phải dùng font Inter** (`GiaoDien.ChuThuong/ChuDam`, file ở
  `Assets/Resources/Fonts`). Font mặc định của Unity thiếu ạ ả ấ ệ ơ ư… — trong Editor vẫn hiện đúng vì
  Windows vẽ bù, lên web thì mất chữ. Kiểm bằng cách đọc cmap của file font (menu 48), đừng tin `HasCharacter`.
- Phép thử mạng vào Play phải **cất phiên đăng nhập đang lưu** (`diablo25d_refresh`) rồi trả lại — không thì
  màn đăng nhập tự đăng nhập chen giữa và tráo tài khoản (menu 26/27/28/48/50 đều làm).
- **Cài đặt đồ hoạ** (nút CÀI ĐẶT ở sảnh, `CaiDatDoHoa.cs`): **4 mức** Cao / Trung bình / Yếu / Rất yếu, lưu trong
  `localStorage` khoá `diablo25d.mucDoHoa2` (khoá cũ `diablo25d.mucDoHoa` 3 mức tự chuyển: 2 cũ = Rất yếu). Mức
  thấp **chỉ thu nhỏ cảnh 3D** (`KetXuatThuNho` trên camera chính, 100/75/62/50%, gắn bởi `TheoDoiCamera`) —
  khung game luôn đủ độ phân giải nên chữ/nút OnGUI sắc nét; `index.html` **không** được đặt `devicePixelRatio`.
  ⚠️ Gán `QualitySettings.shadows…` là ghi thẳng vào bộ thông số của mức Unity đang dùng — mỗi mức phải tự đặt
  đủ bộ giá trị của mình (menu 48 kiểm).
- ⚠️ **Triển khai web**: **xoá sạch `web/Build/`**, chép `Build/WebGL/Build/*`, `Build/WebGL/TemplateData/*`
  (ảnh tiêu đề + font Inter của trang Loading — thiếu là trang Loading mất tên game và font) VÀ
  `Build/WebGL/index.html` sang `web/`. Bản build **không nén sẵn** (Firebase tự nén khi gửi — nó gạt bỏ `Content-Encoding` tự đặt) và
  **tên file = MD5 nội dung** (`nameFilesAsHashes`), nên `Build/**` để `immutable` được; tên cố định +
  `immutable` từng làm **game sập lúc tải** (mã cũ ghép dữ liệu mới). `index.html` còn gắn `?v=` và
  `productVersion` (menu 29 tự gắn, báo 4/4 file): Unity coi `.data` có `?v=` là `immutable` và tự xoá bản cũ.
  **Firebase không bao giờ trả 304 cho file `no-cache`** — đừng dựa vào hỏi lại. Kiểm sau khi deploy: trong
  trình duyệt ĐÃ TỪNG vào trang, lần sau `performance` báo `Build/` 0 byte, chỉ tải trang ~2,6 KB. Lùi bản khẩn cấp:
  `firebase hosting:clone diablo25d-game@<version> diablo25d-game:live`.
- **Tên game hiển thị: "ÁC QUỶ TRỞ LẠI"** (tác giả: Phạm Minh Quân). Tiêu đề là **một ảnh** dùng chung cho trang
  Loading (`WebGLTemplates/Diablo25D/TemplateData/tieude.png`) và màn đăng nhập (`Resources/GiaoDien/TieuDe.png`),
  dựng bằng `python CongCu/TieuDe/sinh_tieu_de.py` từ font Grenze Gotisch (`CongCu/Fonts`, OFL) — script tự gắn
  `?v=<mã>` vào trang. Chữ trên trang Loading dùng Inter nhúng kèm (`TemplateData/Inter-Regular.ttf`).
  **Không đổi `productName`** trong Unity (vẫn "Diablo 2.5D"): cache dữ liệu và chỗ lưu của người chơi tính theo nó.
- **Màn chính (MainMenu.unity) dựng từ cảnh Act2** bằng menu 51 — đừng sửa tay trong scene, chạy lại menu.
  Lò đá: `Assets/Models/LoLuaDa` (FBX + texture nướng từ `CongCu/Blender/lo_lua_da.py`, chạy nền — từ nay
  dựng/thiết kế phải qua Blender MCP). Lửa + khói đen: mô phỏng Mantaflow trong Blender qua MCP
  (`CongCu/Blender/lua_lo_da.blend`) → flipbook `LuaLo` / `KhoiDen`, ba tấm đứng yên mỗi lò, dựng lúc chạy
  bởi `LoLuaDa` → `VfxFactory.LuaLoDa`. ⚠️ `VerticalBillboard` vẽ tứ giác **0,707×** kích thước đặt — đã bù √2.
- **Act2 có 10 lò lửa đá** (cùng prefab `LoLuaDa`, nhóm gốc `LoLua_Act2`, có va chạm) do **menu 54** đặt: lò giữa = chỗ
  đất khô gần tâm nhất (tâm là hồ nước), không dưới nước / trong nhà mồ / sát bia — đổi luật hay dựng lại Act2 thì chạy
  lại menu 54 rồi 54b, đừng kéo tay. **Lốc xoáy dập tắt lửa rồi cuốn cả lò đi, 30 giây sau lò mọc lại và cháy
  tiếp** (menu 54c) — `LoLuaDa.DapTat/Chay`, ngoại lệ duy nhất của luật "vật có hệ hạt thì không cuốn".
- Việc gần đây nhất (xem mục tương ứng trong `HUONG-DAN.md`):
  Thiên thạch **đốt cháy cả cái cây** (lửa lan theo bề mặt thật, cây rụi rồi mọc lại sau 30 s) ·
  Lốc xoáy trả cảnh vật về sau **30 s** thay vì 60 s ·
  **nút con mắt** khoá góc nhìn trên bản cảm ứng.
- **Nhiều người chơi — giai đoạn 1 xong** (mục "Nhiều người chơi, giai đoạn 1" trong
  `HUONG-DAN.md`). Đăng ký, đăng nhập, tạo/vào phòng, sẵn sàng, đếm ngược 10 giây — **tất cả
  nằm trong game Unity**, vẽ bằng OnGUI, gọi Firebase qua **REST** (Firebase Unity SDK không
  chạy trên WebGL). Trang web **chỉ để admin quản lý tài khoản**.
  Dự án Firebase `diablo25d-game` (asia-southeast1). Menu 26, 27, 28 chạy thật, **0 lỗi**.
- **Chơi được trên trình duyệt**: https://diablo25d-game.web.app (bản WebGL, menu 29 —
  build 3–11 phút, lần đầu người chơi tải **172,5 MB**, các lần sau 0 byte và vào game ~10 s, 0 lỗi console). Trang quản trị chuyển
  sang https://diablo25d-game.web.app/quantri/ . Mã nguồn ở
  https://github.com/denispham1107/bongdemdianguc .
  **Cần giảm dung lượng**: 155,7 MB nằm ở tài nguyên, và texture đang nén ASTC nên WebGL
  phải giải nén ra RAM mỗi lần nạp.
  Giai đoạn 2: thấy nhau trong trận, đánh nhau, **người sống sót cuối cùng thắng** (bước 6 — `KetTran.cs`,
  chủ phòng phán quyết, bảng điểm + ghi thành tích; menu 55). Mới chạy trên kênh giả lập, **chưa thử hai máy thật**.
  Còn thiếu của bước 5: trọng tài phán xử máu, bù trễ khi tính trúng.
- Dự án **đã là git repo** (nhánh `main`, ảnh chụp đầu tiên `0ef3e18`, 05/09/2026, 1175 file).
  `Assets/MeshyImports/` (920 MB model gốc Meshy) **nằm ngoài git** — file vẫn trên đĩa, Unity
  vẫn dùng bình thường, nhưng git không cứu được nếu lỡ xoá. `HUONG-DAN.md` vẫn là nơi kể
  **vì sao**, git chỉ giữ chỗ lùi lại.
