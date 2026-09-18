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

**Menu `Diablo 2.5D`** trong Unity (77 mục): 1 nướng asset + dựng màn, 3 tự kiểm tra,
4 chạy thử & chụp hình, 14 chống ô vuông đen, 15–17 ảnh vỏ cây / đá mộ / sứt mẻ bia,
18–18d chạy thử cây cháy, 19 hồi sinh sau lốc, 20 nướng điểm mồi lửa, 21 nút khoá góc nhìn,
22 thanh kỹ năng bản PC, **26 chạy thử mạng (sảnh phòng), 27 chạy thử khoá tài khoản, 28 chụp màn đăng nhập·sảnh, 29 xuất bản WebGL, 30/30b bơm input Act2·Act1 (bước 1), 31 dự đoán & hiệu chỉnh (bước 2), 32 nhiều người một cảnh (bước 3), 33 nội suy (bước 4), 34 PvP (bước 5), 35 ghép phòng cùng màn, 36 tự gắn bộ nối mạng, 37 kỹ năng qua mạng, 38 quái chung & bù trễ, 39 đòn của quái qua mạng, 40 máu khởi đầu, 41 nhịp bước qua mạng, 42 chế độ điều khiển, 43 kiểm toán bước 5, 44 sửa bước 5, 45 bốn người, 46 hiệu ứng qua mạng, 47 chế độ bốn bộ xương, 48 cài đặt đồ hoạ, 49 cầu lửa trúng người·khiên, 50 giao diện đăng nhập·sảnh·phòng, 51/51b/51c màn chính từ Act2 (dựng · chọn góc · chụp lò lửa), 52 tên trên đầu nhân vật, 53 HUD kinh dị trong trận, 54/54b/54c mười lò lửa Act2 (đặt · chạy thử · lốc xoáy cuốn lò), 55 kết trận (người sống sót cuối cùng), 56 đợt quái Act2 · chỗ xuất phát, 57 bàn phím ảo che ô nhập, 58 mưa băng·sấm sét (đóng băng, choáng), 59 sách phép (kéo thả ô kỹ năng), 60 cấp độ·kinh nghiệm·điểm kỹ năng, 61 kinh nghiệm theo từng kỹ năng, 62 thiên thạch đánh ngã, 63 đánh ngã người chơi khác qua mạng, 64 quái vòng ngoài truy lùng sau 60 giây, 65 bình máu · bình mana (rơi, nhặt, uống, mạng), 66 nút KỸ NĂNG ở sảnh (Sách phép xem trước) · con mắt quỷ, 67 Mưa băng đóng băng người chơi khác (qua mạng), 68 Quả cầu băng, 69 Giựt sét (20 m · 75 · 4 tia · 15% choáng), 70 Quả cầu lửa (85 · vệt lửa mới), 71/71b Gió lốc (chạy thử · chụp ảnh), 72 Lửa địa ngục, 73 Tàng hình, 74 Quả cầu điện, 75 Hoá lốc xoáy · Gió lốc hồi mana, 76 Tốc biến**.
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
  (+1, +3, +6…) và mạnh thêm 5% máu · sát thương mỗi đợt. **Vào trận chờ đúng 30 giây** mới ra đợt đầu (`GiayChoDotDau`).
  **Mỗi đợt thêm cố định 20 con vòng ngoài** (`SinhQuaiXa`, `SoQuaiXaMoiDot`), loại ngẫu nhiên, cách người chơi GẦN NHẤT
  **20–25 m** (13/09/2026 đổi từ 10 con ở 55–65 m); không đủ chỗ thì thả gần khoảng nhất có thể và đếm vào `SoQuaiXaDungKhoang`.
  20 con này cho cả phòng — chơi một mình lên cấp 10 sau ~5 đợt, cấp 20 sau ~16 đợt (`kinhnghiem.md`).
  **Sau 60 giây con vòng ngoài nào còn sống tự truy lùng người gần nhất** (`EnemyAI.HenTruyLung`, bỏ giới hạn
  `aggroRange` 14 m); đang truy lùng mà kẹt thì vòng vật cản, kẹt mãi thì đổi chỗ sang 10–14 m cạnh người chơi,
  quái đánh xa bị kẹt trong 1,5× tầm thì bắn tại chỗ. Quái thường KHÔNG có (menu 64 kiểm, có đối chứng). Act2 KHÔNG còn rải quái sẵn, không dòng quỷ dữ/quỷ cây riêng,
  không chế độ bốn bộ xương. Menu 56 kiểm. Chỗ xuất phát của mỗi người: ngẫu nhiên theo **mã phòng** (`ChoXuatPhat.cs`),
  cách nhau ≥ 22 m — mọi máy tính ra cùng một danh sách nên không cần gói tin nào.
- ⚠️ **Chế độ chạy thử "bốn bộ xương" (chỉ còn Act1) đang BẬT** (`GameDirector.CheDoBonBoXuong = true`, hằng số
  trong code): vào màn chỉ 4 bộ xương, giết hết đợi 30 giây ra 4 con mới. Menu 47 kiểm.
  **Tắt trước khi phát hành** — không thì game chỉ còn bốn con bộ xương.
- **Mười sáu kỹ năng**: Cầu lửa, Mưa băng, Sấm sét, Lốc xoáy, Thiên thạch, Khiên, Giựt sét (0–6) + **Bình máu (7), Bình mana (8)**
  (13/09/2026) + **Quả cầu băng (9)** + **Gió lốc (10)** (16/09/2026) + **Lửa địa ngục (11)** (17/09/2026) + **Tàng hình (12)** + **Quả cầu điện (13)** + **Hoá lốc xoáy (14)** + **Tốc biến (15)** (18/09/2026). Vẫn **7 ô** (người dùng chọn) — kỹ năng không có sẵn ô phải kéo vào ô trong Sách phép.
  ⚠️ Số hiệu mới luôn **thêm ở cuối**, không chèn: số hiệu đi qua gói tin và nằm trong thứ tự ô đã lưu của người chơi.
- **Quả cầu băng** (`Skills/QuaCauBang.cs`, hình `Vfx/VfxQuaCauBang.cs`): 3 quả toé quạt như Quả cầu lửa (đường bay/va chạm chép
  `Fireball.Update`), sát thương gốc **65**, hồi chiêu **0,55 s**, 12 năng lượng, nổ vùng 3,4 m; trúng là **chậm 50% trong 2 s** và **40% ĐÓNG BĂNG 1,5 s**
  (18/09/2026: không đi, không tung phép — `FrozenEffect.Apply` như Mưa băng, qua mạng bằng bit `CoBangHoanToan`); **cấp 5 ra 5 quả** (`SoQuaTheoCap`). Hình **dựng bằng Blender MCP** (`CongCu/Blender/qua_cau_bang.blend`) → `Resources/KyNang/QuaCauBang/`
  (lõi pha lê FBX, ảnh sương lạnh, mảnh băng) + icon `Resources/Icons/CauBang.png`; dựng bằng code, **không** có prefab trong GameAssets
  (thêm trường prefab là phải sửa hai scene). Lưới FBX xoay 180° (đuôi gai −Y Blender → +Z Unity). Menu 68 kiểm.
  Cụm gai băng trên đất của **Mưa băng** dùng chung cỡ hình `QuaCauBang.BanKinhHinhBang` (2,55 m, người dùng xin 16/09/2026) —
  chỉ hình, vùng sát thương mỗi tảng vẫn 1,7 m (menu 68 mục J đo kích thước thật).
  **Mưa băng rơi QUẢ CẦU BĂNG** thay tảng băng (16/09/2026): `VfxFactory.QuaCauBangRoi` (luồng khí lạnh/vệt băng như Quả cầu băng, bản nhẹ:
  nửa lượng hạt, không đèn riêng), rơi **THẲNG ĐỨNG** (người dùng chốt 16/09/2026 — tôi từng cho rơi chéo, người dùng không muốn);
  `FallingShard` giữ nguyên rơi/sát thương/đóng băng, chạm đất thì `ThaDuoiQuaCauBang`. Menu 68 mục K.
  **Quả rơi KHÔNG CÒN KHỐI CẦU — chỉ vệt sáng băng như sao băng** (17/09/2026, người dùng; trước đó cùng ngày là cầu tròn bọc khí lạnh,
  đã bỏ): `VfxFactory.DungVetSaoBang` (TrailRenderer 0,17 s, rộng 0,66→0,36 m × ngẫu nhiên 0,7–1, đầu là tấm `DauSaoBang` giọt sáng mũi nhọn răng cưa xoay theo hướng rơi (`DauSaoBangHuong`); ảnh `VetSaoBang.png`/`DauSaoBang.png` vẽ bằng Blender MCP `CongCu/Blender/vet_sao_bang.blend`),
  giữ hào quang, luồng khí lạnh, mảnh băng; chỉ bản `banRoi` — kỹ năng Quả cầu băng vẫn lõi có gai.
  **Cụm gai băng chỉ mọc khi trúng quái / người chơi khác** (người dùng 17/09/2026; 16/09 là cả đồ vật): trong 1,7 m có Damageable còn sống
  trên `damageMask` trừ người tung (`FallingShard.TrungKeDich`) → nổ đủ + gai; không trúng ai (đất trống, đồ vật) → `IceImpact(..., coGai:false)`
  tắt `CumGai*` + `HaoQuang*`, giữ chớp, vòng lạnh, sương, giọt nước, mảnh băng, vết sương giá. Mưa băng nhắm kẻ địch 100%
  (`aimAtEnemyChance = 1`) nên có kẻ địch là gần như quả nào cũng có gai. Menu 68 mục L.
- **Gió lốc** (`Skills/GioLoc.cs`, hình `Vfx/VfxGioLoc.cs`, hiệu ứng `Combat/BiHatTung.cs`, 16/09/2026): **1 lốc** mỗi lần tung (17/09/2026, trước là 3). **Hình dựng bằng
  Blender MCP** (17/09/2026, `CongCu/Blender/gio_loc.blend` → `Resources/KyNang/GioLoc/`: `LocNho.fbx` 3 vỏ + dải gió cao 5 m, ảnh gió liền mạch
  `GioDai`/`GioSoi`, flipbook `BuiDenCuon`; chân ×1,68 so với gốc, nhỏ dần về 0 ở 2,3 m; toàn bộ bề ngang ×1,1 lúc chạy `HeSoBanKinhGioLoc`), xám trắng như Lốc xoáy, **xoáy MỘT chiều đi lên** (mọi lớp quay âm quanh +Y + UV trượt âm — chiều
  chọn bằng số đo trên lưới, `VfxFactory.ChieuQuayGioLoc`), khói bụi đen cuộn quanh thân + vệt phía sau, **2 tia sét luôn đánh từ đỉnh xuống trong lòng lốc** mỗi 0,45 s như Lốc xoáy, bề dày ×5/15,37, **bám theo lốc** (`LightningArc.BamTheo` — tia ghim toạ độ thế giới bị bỏ lại 1,2–2,7 m), hai tia đối diện cách ~1,2 m ở đỉnh, thu vào theo vỏ trong cùng ở chân (`VfxFactory.GioLocSetTrongLoc`, chỉ hình; tia khi trúng đối thủ đã bỏ). Bay **9,5 m/s**
  (người dùng chốt 17/09/2026) **xuyên mọi vật cản / người** — bám mặt đất bằng `GioLoc.MatDatY` (CHỈ lớp Ground; `GroundY` gồm cả Default làm lốc trèo lên mái nhà), tan sau 4,5 s; **cấp 5: 2 lốc song song cách 4 m, tốn gấp đôi năng lượng** (`GioLoc.SoLocTheoCap` theo cấp NGƯỜI TUNG); mỗi lốc trúng mỗi mục tiêu **một lần** 75, vùng 2,42 m, **55% hất tung** 0,5 s cao 1,5 m (mỗi lốc gieo riêng, khiên chặn); 20 năng lượng · hồi chiêu 0,4 · niệm 0,38.
  ⚠️ 18/09/2026 người dùng đổi: **mỗi lần trúng kẻ địch hồi 10 mana** (`ManaHoiMoiLanTrung`, cố định cả 5 cấp — chỉ cộng trên máy
  của chính người tung, `pc.tuDocInput`, không thì bản sao mạng cũng cộng) và **cấp 5 chỉ tốn 25 năng lượng** (`NangLuongCan`,
  con số cố định thay cho 20 × 1,1⁴ × 2 = 58,6 — cấp 4 tốn 26,6 nên cấp 5 lại rẻ hơn, đúng ý người dùng).
  Bị hất = khoá như ngã + **ngắt chiêu** (`PlayerController.NgatChieu`, `EnemyAI.NgatDon`). Bit mạng **`CoHatTung` = bit thứ 5**, mặt nạ gói
  người chơi và gói quái đã nới **0x1F** (còn trống bit 7 gói người chơi, bit 6–7 gói quái). Lướt qua lò lửa thì `DapTatRoiChayLai(30)`.
  Icon `python CongCu/Icon/sinh_gio_loc.py`. Menu 71 kiểm, 71b chụp ảnh.
- **Tốc biến** (`Skills/TocBien.cs`, hiệu ứng `Vfx/VfxTocBien.cs`, 18/09/2026, nhóm **HỖ TRỢ**): dịch chuyển tức thời tối đa **15 m**
  tới chỗ ngắm; ngắm vào chỗ không đứng được thì **lùi dần 0,5 m** về phía mình tới điểm trống gần nhất (đi xuyên tường/bia mộ được);
  **40 năng lượng**, hồi chiêu **5 s − 0,25 s mỗi cấp** (`HoiChieuTheoCap`, cấp 5 còn 4 s — `PlayerController.HoiChieuTocBien` đọc cấp
  hiện tại, đừng dùng trường `tocBienCooldown` vì đó chỉ là cấp 1). **Không niệm** (`castTime` 0,01 s — vẫn đủ để gói phép bay sang máy
  khác; bản sao chỉ chạy hiệu ứng, không tự kéo mình đi). ⚠️ Chỗ đến tính bằng `TocBien.MatDatY` **chỉ lớp Ground** — dùng
  `VfxFactory.GroundY` thì "trong lòng khối đá" hoá ra "trên nóc đá" và người chơi nháy được lên mái nhà mồ (menu 76 đo 18/09/2026).
  **CẤP 5 GỠ TRÓI** (người dùng 18/09/2026): dùng được NGAY khi đang choáng / ngã / đóng băng / hất tung — ngoại lệ DUY NHẤT của
  cái chặn trong `PlayerController.CastAt` (`TocBien.CapNamGoTroiDuoc`) — và nháy xong `TocBien.GoSachTrangThai` xoá sạch
  `FrozenEffect` (qua `Thaw()`), `StunnedEffect`, `BiDanhNga`, `BiHatTung`, `BurningEffect`. Đang bị **Lốc xoáy** cuốn
  (`WhirledEffect`) thì chịu. ⚠️ Đo thử đừng dính cháy chung với đóng băng: `BurningEffect.Start` gọi `frozen.Thaw()` nên
  lửa nuốt mất cái đóng băng trước khi đo (menu 76 mục G tách riêng).
  Đổi `transform.position` phải **tắt `CharacterController` rồi bật lại**. Icon Blender MCP. Menu 76 kiểm.
- **Hoá lốc xoáy** (`Skills/HoaLocXoay.cs`, 18/09/2026): bấm là cơn **Gió lốc đang bay của LẦN TUNG GẦN NHẤT** (mỗi cơn mang
  `GioLoc.lucTung`; cấp 5 hai cơn cùng mốc → hoá cả hai) **phình to thành Lốc xoáy** — `PhinhToThanhLoc` chỉ đổi tỉ lệ HÌNH
  0,42 → 1,00 trong 0,55 s, không đụng sức hút/sát thương. Cơn lốc mới giữ **tốc độ 9,5 m/s của Gió lốc** (Lốc xoáy gốc 3,4),
  sống 6 s, và sát thương cộng cả hai: `Tornado.donChamMotLan` = 75 × cấp Gió lốc (trường MỚI, mỗi kẻ một lần) + 20/giây và
  tia sét × cấp Lốc xoáy. **45 năng lượng · hồi chiêu 0,5 · niệm 0,38**; không có Gió lốc nào đang bay thì `CastAt` từ chối
  TRƯỚC khi trừ mana (`HoaLocXoay.CoLocDeHoa`). Icon Blender MCP `CongCu/Blender/hoa_loc_xoay.blend`. Menu 75 kiểm.
- **Quả cầu điện** (`Skills/QuaCauDien.cs`, hình `Vfx/VfxQuaCauDien.cs`, 18/09/2026): quả cầu lơ lửng hiện **ngay cạnh kẻ địch gần
  chỗ ngắm nhất** (không có ai thì đứng đúng chỗ ngắm), tầm ngắm **18 m = Thiên thạch**; cứ **0,4 s bắn một lượt, ĐỦ 10 LƯỢT MỚI TAN**
  (⚠️ 18/09/2026 người dùng chốt: quanh đó không có ai thì cầu **đứng nguyên chỗ chờ**, lượt không tính — chỉ hết hạn `GiayChoToiDa` **30 s** mới tan),
  mỗi lượt **5 tia — mỗi kẻ MỘT tia**, chọn 5 kẻ gần cầu nhất trong **9 m** (quái lẫn người chơi khác, dư thì bỏ); mỗi tia
  **155,52** = `GiatSet.SatThuongNguoiChoi × CapDo.SatThuongTheoCap(5)` (đọc thẳng từ Giựt sét, không chép tay) và **30% choáng 1,5 s**;
  **55 năng lượng** · hồi chiêu **5 s** · niệm 0,62. Hình **dựng bằng Blender MCP** (`CongCu/Blender/qua_cau_dien.blend` →
  `Resources/KyNang/QuaCauDien/`: `CauDien.fbx` = lõi cầu **tối** + **vành sáng** + vỏ cung điện, ảnh hào quang **hình vòng khuyên**,
  `TiaDien`/`HatDien`), ba lớp quay ngược chiều nhau; icon `Resources/Icons/CauDien.png`. ⚠️ Vỏ điện là **năm đường VIỀN TRẮNG lớn
  vòng quanh, nằm NGOÀI mặt cầu** (người dùng xác nhận 18/09/2026) — thứ họ chê "răng cưa rất xấu" là mấy **gai thẳng ngắn chĩa ra**
  của bản đầu, đã bỏ hẳn; cũng **không có tia sét nhỏ bắn ra liên tục** (đã thử rồi bỏ). Menu 74 kiểm.
- ⚠️ **`StunnedEffect.Apply` từng kéo mọi cú choáng ngắn hơn 2 giây thành 2 giây** (trường `remaining` khai báo sẵn 2 và `Apply` lấy
  `Max` ngay từ lần đầu) — sửa 18/09/2026: component MỚI nhận đúng số giây. Ảnh hưởng cả Giựt sét (1,5 s). Cùng cái bẫy `AddComponent`
  mang giá trị mặc định của `FrozenEffect`.
- **Tàng hình** (`Combat/TangHinh.cs`, shader `S_TangHinh`, 18/09/2026): thay TOÀN BỘ vật liệu model bằng shader viền fresnel (khác
  `FrozenEffect` chỉ phủ thêm lớp); 20 giây, hồi chiêu 30 s, 30 năng lượng; **quái không thấy** (`GameDirector.GanNhat` bỏ qua, `EnemyAI`
  chọn lại ngay), **miễn mọi hiệu ứng** (`TangHinh.ChanHieuUng` chặn ở đầu mỗi `Apply`, và xoá hiệu ứng đang dính) nhưng **vẫn ăn sát thương**;
  đi nhanh +20%; **đòn đầu tiên bằng kỹ năng gây sát thương nhân đôi TOÀN BỘ sát thương của kỹ năng ấy** (mọi vệt Mưa băng, mọi quả trong chùm,
  cả sát thương cháy — vì nhân vào `manhHon`) rồi tan (Khiên / bình không tính); **hai giây cuối thân NHẤP NHÁY** (0,22 s, độ hiện 1 ↔ 0,12) và
  **chỉ máy của mình thấy** (`laCuaMinh && !tuMang` — bản sao không đếm được giờ thật); người khác chỉ thấy khi mình di chuyển,
  mình luôn thấy thân mình mờ. Bit mạng `CoTangHinh` = bit thứ 6 → **mặt nạ byte cờ nay 0x3F** (gói người chơi dùng hết 8 bit).
  ⚠️ Đòn đầu quyết định ở **`BeginCast`** (lúc bắt đầu niệm) chứ không ở `Release`, vì gói phép bay đi từ đó: cờ `donTangHinh` đi nhờ
  **bit cao của byte cấp kỹ năng** trong gói 17 byte, không thì máy kia phát lại phép với sát thương thường. Menu 73 kiểm.
- **Lửa địa ngục** (`Skills/LuaDiaNguc.cs`, 17/09/2026): **5 quả** `Fireball` (18/09/2026, trước là 4) toả 18° rồi **tự dí** (`Fireball.tocQueo` 360°/s,
  giữ ≥ 1,3 m trên mặt đất — đo đất ở BA chỗ: dưới quả, theo hướng bay, về phía mục tiêu; không thì đâm đất Act2) tối đa 5 kẻ gần người tung nhất
  trong 20 m; cấp 1 = impactDamage prefab Quả cầu lửa × 1,2⁴ (176); **31 năng lượng** (⚠️ số nằm trong **prefab `Player_Sorceress`**) · hồi chiêu 0,5 ·
  niệm 0,38; màu quả và vụ nổ **giống hệt Quả cầu lửa** (18/09/2026 bỏ lớp nhuộm đỏ sẫm); icon Blender MCP. Menu 72 kiểm.
- **Giựt sét** (16/09/2026, hằng trong `GiatSet`: `TamNguoiChoi` 20 m, `SatThuongNguoiChoi` 75, `SoTiaNguoiChoi` 4, `XacSuatChoangNguoiChoi` 0,15,
  `GiayChoangNguoiChoi` 1,5 + 0,15 s/cấp): tối đa 4 tia cùng lúc (mỗi tia một kẻ địch phía trước), mỗi tia vẫn lan 5 lần; **mỗi cú trúng kể cả
  tia lan** gieo 15% choáng (`StunnedEffect.Apply` — bản sao mạng tự bỏ qua). Giựt sét của **quái** (`PhongCuaQuai`) không choáng. Menu 69 kiểm.
- **Quả cầu lửa** sát thương **85** (16/09/2026) — ⚠️ số nằm trong **prefab `Skill_QuaCauLua`** (đè code). Vệt lửa phía sau vẽ lại bằng
  **Blender MCP** (`CongCu/Blender/vet_lua_qua_cau_lua.blend`): hạt `Flames` đổi từ `Tex_flame.png` (một **hình tam giác**) sang flipbook
  `Flipbooks/LuaDuoi` (4×4 đám lửa cuộn) + `TrailRenderer` vệt lửa dài `KyNang/QuaCauLua/VetLuaDai`; sửa lúc chạy trong
  `VfxFactory.NangCapDuoiLua` (gọi từ `Fireball.Spawn`, cả quả cầu của quái), nổ thì `ThaDuoiLua` thả vệt ra tan dần. Menu 70 kiểm.
- ⚠️ **Bình máu / mana** (`CapDo.KyBinhMau/KyBinhMana`, `PlayerController.UongBinh`, hằng `MauMoiBinh 100`, `ManaMoiBinh 50`,
  `HoiChieuBinh 0.5` — HẰNG chứ không phải trường public để prefab không đè): mở 1 điểm, `CapToiDaCua` = 1; không niệm, không
  đi qua gói kỹ năng. **Bình rơi CHUNG cả phòng** (`QuanLyBinhRoi` + `BinhRoi`): máy trọng tài quái gieo 10%/10% khi quái chết và
  gửi `LoaiBinhRoi`; máy nào có nhân vật tới gần 3,5 m thì xin (`LoaiXinBinh`); **chủ phòng giao cho người xin TRƯỚC**
  (`LoaiBinhThuoc`), mọi máy thấy bình bay vào đúng người, chỉ máy người ấy cộng số bình. Cả ba gói gửi lặp 3 lần (kênh
  như UDP). Menu 65 kiểm (tỉ lệ, nhặt, uống, số bình trên ô, 5 ca mạng).
- **CẤP ĐỘ VÀ KINH NGHIỆM** (`Assets/Scripts/Player/CapDo.cs`, 13/09/2026) — **mọi con số ở `kinhnghiem.md`**.
  Vào trận ai cũng cấp 1, tối đa **cấp 20** (13/09/2026, trước là 10); **tính theo từng trận, không cất lại** (mỗi trận là một ván đấu riêng).
  Mỗi cấp: máu +15%, mana +10%, tốc độ +3,5% **chỉ tới cấp 10** (`CapTangTocToiDa`) (nhân dồn), và +1 điểm kỹ năng. Bảy kỹ năng **đều khoá lúc đầu**,
  cấp 1 có sẵn 1 điểm. Kỹ năng tối đa cấp 5: +20% sát thương, +10% mana, hiệu ứng +0,15 s mỗi cấp; riêng Khiên
  +15% máu khiên. Mở/nâng trong **Sách phép** (chữ cột trái ×1,15, phần thân chi tiết ×1,20 và cuộn được bằng con lăn / vuốt — `CuaSoSachPhep.HeSoChuKho/HeSoChuThan`).
- ⚠️ **Cấp kỹ năng đi kèm từng gói tung phép** (`GoiTin.MotPhep.capKyNang`, gói 17 byte): phép của người cấp 5
  phải mạnh đúng cấp 5 trên mọi máy. `PlayerController.capPhepDangTung` là cấp của NGƯỜI TUNG, không phải của
  người xem. Kinh nghiệm giết quái do **chủ phòng** chia (gói `LoaiKinhNghiem`, 4 byte) vì chỉ nó chạy AI quái;
  kinh nghiệm giết người đi theo gói `LoaiChet` mà máy nạn nhân đã gửi.
- ⚠️ **Mọi đường gây sát thương của người chơi phải gọi `GhiKeDanh` trước `TakeDamage`** — kinh nghiệm, bảng điểm
  và dòng "Bị … hạ" chỉ đọc `Damageable.keDanhCuoi`. 13/09/2026 sót ở 6 chỗ (Mưa băng, Sấm sét, lốc cuốn, cháy, vũng lửa
  Thiên thạch, cây cháy) → giết bằng chúng không ai được gì. Thêm kỹ năng / hiệu ứng gây sát thương mới thì **chạy menu 61**.
- **SÁCH PHÉP** (`SachPhep.cs` + `CuaSoSachPhep.cs`, 12/09/2026): người chơi tự kéo thả kỹ năng vào các ô.
  `SachPhep` chỉ giữ **bản đồ ô → số hiệu kỹ năng**; số hiệu KHÔNG đổi (nó là thứ đi qua mạng và vào
  `PlayerController.CastAt`), chỉ chỗ ngồi đổi. **Hai bộ ô riêng**: `OTron` (cảm ứng) và `OVuong` (máy tính) —
  bảy nút tròn xếp hai cung không có "thứ tự trái sang phải" dùng chung được với hàng ô vuông. Lưu trong
  PlayerPrefs (`diablo25d.sachphep.*`). Ô trong bảng dùng **chính** `GameHUD.LechNut` nên hình cụm nút trong
  bảng khớp cụm nút ngoài trận (menu 59 đo: lệch 0,00%).
  ⚠️ **Cột danh sách xếp theo NHÓM HỆ** (người dùng chốt 18/09/2026), không theo số hiệu nữa: LỬA (Cầu lửa · Thiên thạch ·
  Lửa địa ngục) → BĂNG (Quả cầu băng · Mưa băng · Tàng hình) → SÉT (Giựt sét · Sấm sét · Quả cầu điện) → PHONG (Gió lốc ·
  Lốc xoáy · Hoá lốc xoáy) → HỖ TRỢ (Bình máu · Bình mana · Khiên · Tốc biến). Bảng ở `SachPhep.KyNangTheoNhom` / `TenNhom` / `MauNhom`; mỗi nhóm có một
  dòng tiêu đề thấp hơn hàng thường, nên **mọi chỗ tính vị trí phải đi qua `CuaSoSachPhep.YCuaDong` / `CaoDong`**, không được
  nhân "chỉ số × chiều cao hàng" (phép thử menu 59 hỏi `CuaSoSachPhep.VungHangKyNang`). Số hiệu kỹ năng KHÔNG đổi.
  ⚠️ Thứ tự ô ghi **thẳng localStorage** trên WebGL (`CD_DocChuoi/CD_GhiChuoi` trong `CauNoiCaiDat.jslib`, 14/09/2026) — PlayerPrefs
  WebGL ghi không đồng bộ, đóng tab ngay là mất; ngoài WebGL vẫn PlayerPrefs.
- **Nút KỸ NĂNG ở sảnh** (14/09/2026, chỗ cũ của CÀI ĐẶT — CÀI ĐẶT lên đầu trang bên trái ĐĂNG XUẤT): mở Sách phép **xem trước**
  (`CuaSoSachPhep.MoXemTruoc`): 9 kỹ năng đủ màu, không ổ khoá, kéo thả được, không nút mở khoá / số bình; thông số đọc từ
  nhân vật trưng bày của MainMenu. `CuaSoSachPhep` là static — sảnh phải đóng nó khi rời sảnh (`OnDestroy`), không thì vào trận
  nó vẫn phủ màn hình. Menu 66 kiểm.
- ⚠️ Mở Sách phép thì phải **khoá hết input trận đấu**: `DocInput.Doc` trả gói rỗng, `GUI.Button` của thanh kỹ
  năng và của chính nút Sách phép ngừng nhận bấm (IMGUI cho cái vẽ TRƯỚC giành sự kiện, nên cái nằm "dưới"
  bảng vẫn ăn cú bấm nếu không chặn).
- Góc phải trên bản cảm ứng: **nút con mắt** (khoá góc nhìn — ảnh con mắt quỷ `Resources/GiaoDien/MatQuy*.png` từ
  `CongCu/Icon/sinh_mat_quy.py`, đang khoá có vết chém máu chéo), dưới nó là **nút Sách phép**. Nút đổi góc nhìn
  hình máy quay đã **bỏ hẳn** (12/09/2026, người dùng xin) — phím C bản máy tính vẫn còn.
- ⚠️ **Đóng băng và choáng có tác dụng lên CẢ NGƯỜI CHƠI** (12/09/2026). Trước đó chỉ `EnemyAI` đọc
  `FrozenEffect`/`StunnedEffect`, `PlayerController` không đọc dòng nào — vỏ băng chỉ là lớp vật liệu phủ lên
  hình, người bên trong vẫn chạy và vẫn bấm đủ bảy kỹ năng. Nay có `PlayerController.HeSoTocBang` và
  `DangBiKhoaCung`; chặn ở `CastAt` (lúc BẮT ĐẦU niệm) chứ không ngắt phép đang niệm — gói "tôi vừa tung phép"
  đã gửi đi rồi, ngắt giữa chừng là hai máy kể hai chuyện khác nhau.
- **Thiên thạch của người chơi đánh ngã 40% · 1,5 s** (`BiDanhNga.cs`): lật **model con** (va chạm ở gốc vẫn đứng), xoay
  quanh trục của GỐC (model Meshy xoay sẵn 180° — xoay trục riêng là ngã úp). Thiên thạch Quỷ dữ mặc định 0%. Bit
  `CoNga` trong byte hiệu ứng mạng — ⚠️ byte cờ gói tin nay dành **6 bit** (0x3F, từ Tàng hình 18/09/2026); gói người chơi đã dùng hết 8 bit (2 + 6) — thêm hiệu ứng thứ 7 phải nới byte hoặc dùng byte khác, ở CẢ gói người chơi (`VietTrangThai`/`DocTrangThai`) lẫn gói quái, không thì bit mới bị cắt im lặng (menu 63). Chữ nổi trên đầu dùng font Inter (`VeSoSatThuong`) — trước đây font mặc định, mất dấu trên web.
- **`FrozenEffect` có HAI đồng hồ**: `remaining` (vỏ băng + lớp chậm `slow`) và `dongCungConLai` (đóng cứng
  hoàn toàn). Mưa băng trúng là chắc chắn chậm 50% trong 2 s, và 35% số lần đóng cứng 1,5 s. Sấm sét: 35% choáng.
  Đừng suy "đóng cứng" từ `slow > 0,85` như bản cũ — hai tầng chồng nhau thì công thức ấy sai.
  Bắt đầu đóng cứng thì chữ **"ĐÓNG BĂNG!"** bay lên (`FrozenEffect.BaoChuDongBang`, gọi ở `Apply` và `HieuUngQuaMang.ApCo`); vỏ băng
  phủ cả **SkinnedMeshRenderer** (14/09/2026 — trước chỉ `MeshRenderer`, model có xương không có vỏ), dày khi đóng cứng, mỏng khi
  chỉ chậm. Menu 67 kiểm qua đường mạng thật.
- **Mưa băng và Sấm sét KHÔNG nhắm vào chính người tung** (`boQua`): khi chơi mạng `damageMask` có cả lớp
  `Player`, mà người tung luôn đứng giữa vùng mình nhắm nên gần như tảng nào cũng chọn anh ta. Menu 58 giữ sẵn
  mẫu đối chứng tái hiện lỗi này.
- Chặn bản sao mạng tự đi trong `HandleMovement` là `!tuDocInput && health.mauDoMayKhacQuyet` — **không được**
  quay lại chỉ mỗi `!tuDocInput`: mọi phép thử bơm input đều đặt cờ đó, và chúng sẽ đo một nhân vật bị khoá chân.
- ⚠️ **Chữ tiếng Việt có dấu phải dùng font Inter** (`GiaoDien.ChuThuong/ChuDam`, file ở
  `Assets/Resources/Fonts`). Font mặc định của Unity thiếu ạ ả ấ ệ ơ ư… — trong Editor vẫn hiện đúng vì
  Windows vẽ bù, lên web thì mất chữ. Kiểm bằng cách đọc cmap của file font (menu 48), đừng tin `HasCharacter`.
- Phép thử mạng vào Play phải **cất phiên đăng nhập đang lưu** (`diablo25d_refresh`) rồi trả lại — không thì
  màn đăng nhập tự đăng nhập chen giữa và tráo tài khoản (menu 26/27/28/48/50 đều làm).
- **Cài đặt đồ hoạ** (nút CÀI ĐẶT ở đầu trang sảnh, `CaiDatDoHoa.cs`): **4 mức** Cao / Trung bình / Yếu / Rất yếu, lưu trong
  `localStorage` khoá `diablo25d.mucDoHoa2`; **người chưa từng chọn khởi động ở Yếu** (`CaiDatDoHoa.MacDinh`, 13/09/2026 — trước là Cao), ai đã chọn thì giữ (khoá cũ `diablo25d.mucDoHoa` 3 mức tự chuyển: 2 cũ = Rất yếu). Mức
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
- **Cài được như ứng dụng (PWA)**: `web/manifest.webmanifest` + `web/sw.js` (bộ chạy nền **không cache gì** — Unity đã tự
  quản lý cache bản build; giữ index.html cũ là ghép mã cũ với dữ liệu mới) + thẻ `apple-*` trong template. Nút "CÀI ĐẶT
  ỨNG DỤNG" và dòng nhắc iOS nằm **trong màn chờ tải**, không nổi trên khung game (cạnh phải/dưới là chỗ cần điều khiển).
  ⚠️ **Manifest xin `"display": "fullscreen"`** (13/09/2026 — `standalone` làm Android giữ thanh trạng thái + thanh điều hướng;
  iOS kín màn hình nhờ thẻ `apple-*`, không nhờ manifest). Dự phòng trong trang: Android + đã cài + chưa toàn màn hình → cú chạm
  đầu gọi Fullscreen API (`nenGoiToanManHinh`). Ứng dụng Android đã cài chỉ nhận manifest mới khi Chrome tự kiểm lại (≤1 ngày) hoặc cài lại.
  Icon sinh từ ảnh người dùng gửi bằng `python CongCu/Icon/sinh_icon.py` → `web/icons/` (thường 92%, **maskable 68%** vì
  Android cắt icon, apple-touch 180). Số đo: `PlayTestShots/pwa.txt`.
- **Bàn phím điện thoại**: Unity không biết bàn phím ảo che bao nhiêu (canvas không đổi kích thước). `index.html` đo bằng
  `window.visualViewport` rồi `SendMessage("BanPhimAo", "DatChe", <tỉ lệ>)`; `ManDangNhap.TinhBoCuc` bỏ ảnh tiêu đề và
  đẩy khung lên, ưu tiên giữ ô nhập trên mép bàn phím. Menu 57 kiểm. Vật thể **phải tên "BanPhimAo"** (SendMessage tìm theo tên).
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
