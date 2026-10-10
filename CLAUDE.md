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
  dấu, lỗi font** (người dùng yêu cầu nhiều lần). Mọi nơi: HUD, sảnh, cài đặt, tên trên đầu (09/10/2026 kiểu "Lửa địa ngục": Playfair Display SC Black qua `GiaoDien.ChuTen` — đủ 134 chữ, quầng lửa + gạch than hồng ảnh Blender MCP; menu 52), trang Loading, trang
  quản trị, ảnh chữ. Unity: font Inter qua `GiaoDien.ChuThuong/ChuDam` (GUIStyle từ `GUI.skin` phải gán `.font`);
  web: nhúng Inter kèm trang; font trang trí: đọc cmap (`BangKyTuFont.Doc`) đủ 134 chữ có dấu trước khi dùng;
  tên người chơi gõ: `GhepDauTiengViet.Ghep`. ⚠️ **`GiaoDien.VuaO` (co / cắt chữ cho vừa nút) từng trừ phần đệm HAI LẦN** (nơi gọi trừ khỏi
  chỗ trống, `CalcSize` lại cộng vào) → nút "ĐƠN" ở sảnh hiện "Đ…" (người dùng 05/10/2026; màn 1616×587: chỗ 39,8, chữ 29 + đệm 16); nay `RongChu`
  trừ đệm. Menu 103 (cửa sổ Editor tạm ở 8 cỡ màn, vẽ đúng nút bằng `GiaoDien.Nut`): sửa 0 lần cắt, đối chứng bản cũ cắt ở 6/8 cỡ. HUD trong trận đã có dấu (menu 53 quét chuỗi); chỗ nào còn sót thì báo và đề nghị sửa.

## Cấu trúc

```
Assets/Scenes/     Act2.unity (man choi DUY NHAT), MainMenu.unity
Assets/Scripts/
   GameBootstrap   dat bau troi, nhan vat, camera, GameDirector, HUD khi vao tran
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

⚠️ **CHỈ CÒN MỘT MÀN CHƠI: Act2 (Nghĩa địa).** Act1 (Đấu trường dựng bằng code) **đã xoá vĩnh viễn** 27/09/2026 theo yêu cầu
người dùng — scene, 138 tài nguyên riêng, code dựng màn (`TerrainFactory`, `DuongFactory`, `GraveFactory`, `LeuFactory`,
`RockTexScale`, phần lớn `WorldFactory`), luật quái cũ + chế độ bốn bộ xương, menu 1/5/8/18b/30b/47. **Đừng dựng lại, đừng
thêm nhánh "nếu là Act1"**. Sảnh không còn chọn màn: phòng nào cũng vào `PhongMang.ManMacDinh` ("Act2"); phòng cũ trên
Firebase ghi "Act1" vẫn vào Act2. Cây Act2: 1 renderer, lưới **Read/Write TẮT**, tên `TREE_oakA_bare_701`, vật liệu
`Act2_VoCay_SanSui` (`Diablo25D/BarkTriplanar`).

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

**Menu `Diablo 2.5D`** trong Unity (121 mục — bỏ 1, 5, 8, 18b, 30b, 47 cùng Act1; 71c, 94 xoá cùng hình Gió lốc cũ 05/10/2026): 2 mở màn chơi, 3 tự kiểm tra,
4 chạy thử & chụp hình, 14 chống ô vuông đen, 15–17 ảnh vỏ cây / đá mộ / sứt mẻ bia,
18–18d chạy thử cây cháy, 19 hồi sinh sau lốc, 20 nướng điểm mồi lửa (cây · bia mộ · nhà mồ), 21 nút khoá góc nhìn,
22 thanh kỹ năng bản PC, **26 chạy thử mạng (sảnh phòng), 27 chạy thử khoá tài khoản, 28 chụp màn đăng nhập·sảnh, 29 xuất bản WebGL, 30 bơm input (bước 1), 31 dự đoán & hiệu chỉnh (bước 2), 32 nhiều người một cảnh (bước 3), 33 nội suy (bước 4), 34 PvP (bước 5), 35 ghép phòng cùng màn, 36 tự gắn bộ nối mạng, 37 kỹ năng qua mạng, 38 quái chung & bù trễ, 39 đòn của quái qua mạng, 40 máu khởi đầu, 41 nhịp bước qua mạng, 42 chế độ điều khiển, 43 kiểm toán bước 5, 44 sửa bước 5, 45 bốn người, 46 hiệu ứng qua mạng, 48 cài đặt đồ hoạ, 49 cầu lửa trúng người·khiên, 50 giao diện đăng nhập·sảnh·phòng, 51/51b/51c màn chính từ Act2 (dựng · chọn góc · chụp lò lửa), 52 tên trên đầu nhân vật, 53 HUD kinh dị trong trận, 54/54b/54c mười lò lửa Act2 (đặt · chạy thử · lốc xoáy cuốn lò), 55 kết trận (người sống sót cuối cùng), 56 đợt quái Act2 · chỗ xuất phát, 57 bàn phím ảo che ô nhập, 58 mưa băng·sấm sét (đóng băng, choáng), 59 sách phép (kéo thả ô kỹ năng), 60 cấp độ·kinh nghiệm·điểm kỹ năng, 61 kinh nghiệm theo từng kỹ năng, 62 thiên thạch đánh ngã, 63 đánh ngã người chơi khác qua mạng, 64 quái vòng ngoài truy lùng sau 60 giây, 65 bình máu · bình mana (rơi, nhặt, uống, mạng), 66 nút KỸ NĂNG ở sảnh (Sách phép xem trước) · con mắt quỷ, 67 Mưa băng đóng băng người chơi khác (qua mạng), 68 Quả cầu băng, 69 Giựt sét (20 m · 75 · 4 tia · 20% choáng · Quỷ cây), 70 Quả cầu lửa (85 · vệt lửa mới), 71/71b Gió lốc (chạy thử · chụp ảnh), 72 Lửa địa ngục, 73 Tàng hình, 74 Quả cầu điện, 75 Hoá lốc xoáy · Gió lốc hồi mana, 76 Tốc biến, 77 Kháng hệ (nhóm bị động), 78 cụm nút kỹ năng cảm ứng, 79 ánh sáng vòng đêm → ngày → chiều lặp lại (Act2), 80 bốn việc Sách phép (bình cấp 1–3 · quả cầu xuyên bia · băng dập lò · bị động Tốc độ), 81 Thiên thạch đốt bia mộ · nhà mồ, 82 Lốc xoáy hình Blender (một chiều, cuốn lên) · Gió lốc hất tung 20%, 83 Mây giông, 84 quả cầu NẢY · Gió lốc hình quạt, 85 xác nằm trên vũng máu · dấu "+" nâng cấp trên ô kỹ năng, 86 mở rộng Act2 thêm 50%, 87 ra – vào nhà mồ (kẹt), 88 lưới va chạm hai mặt cho nhà mồ, 89 ô vuông sáng trên mặt đất (đèn kỹ năng), 90 / 90b lửa cháy toàn thân (chạy thử · chụp các mức mật độ), 91 đặc tính quái (Quỷ cây choáng · Quỷ dữ ngã · Bộ xương đỡ đòn), 92 chế độ Đơn/Đôi (đồng đội, xuất phát, kết trận đội), 92b sảnh Đơn/Đôi trên Firebase thật, 93 chụp mức đen Lốc xoáy + Gió lốc (để người dùng chọn), 97/97b Gió lốc cuộn lên (đo dịch chuyển biểu kiến · ảnh động 3 mức), 98/98b hạt bụi đen cuốn lên Lốc xoáy + Gió lốc (đo vị trí hạt · ảnh động), 99/99b Lốc xoáy hiện tại đối thủ + Gió lốc hất ngã ngửa (đo · ảnh động), 100 Mây giông cao 8 m có trong khung máy quay game không, 101/101b/101c tư thế phù thuỷ màn chính (đo lòng bàn tay · chụp hai mặt tay · chạy thử), 102 ngọn lửa thật trên quả cầu lửa có chói hơn không, 103 chữ vừa nút ĐƠN / ĐÔI ở mọi cỡ màn hình, 105/105b khối đen ở hai đầu tia Giựt sét · chụp các mức nhánh nhỏ, 106 chỗ sét chạm đất của Sấm sét + Mây giông + Lốc xoáy, 107 quầng xanh Giựt sét + Quả cầu điện rộng 20%, 108 các mức dịu sáng Quả cầu lửa (bay · nổ), 109 chân người chơi chạm đất (Act2, 40 chỗ + đang đi, đối chứng tắt `chamDat`), 110 địa ngục ngoài bản đồ (rơi xuống mất 20% máu / giây), 111/111b dung nham chảy theo các đường gân (đo dịch chuyển · ảnh động 4 mức), 112 máy BOT ở ghế trống (sảnh, Firebase thật), 113 máy BOT trong trận (bước 2), 114 máy BOT đi lại (bước 3), 115 máy BOT đánh (bước 4), 116 màu áo người chơi (Đơn – Đôi), 117 tầm nhìn 25 m + sương chiến tranh, 118 nút thoát trận (bảng xác nhận), 119 Nhào lộn (kỹ năng 22)**.
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

- Act2 sạch: **1011 vật thể** (trước khi mở rộng), 0 ô vật liệu hỏng, camera `near = 1.50`.
- ⚠️ **ACT2 ĐÃ RỘNG THÊM 50%** (người dùng 27/09/2026, **menu 86** `Act2MoRong.cs`, chạy MỘT lần): rào mới dựng lại bằng Blender MCP
  (`hang_rao_rong.fbx`, nửa cạnh 53,7 → **66,41 m**, diện tích ×1,53, vẫn một cổng mở), terrain 109,2 → **134,63 m** (vùng |x|,|z| ≤ 51
  chép nguyên), vành mới thưa hơn: +100 bia · 13 cây · 51 đá · 2 nhà mồ · 4 lò · 6 vũng nước → **552 bia · 71 cây · 280 đá · 7 nhà mồ ·
  14 lò · 17 mặt nước**; `arenaRadius` 50 → **62** (GameDirector + GameBootstrap trong scene). ⚠️ Lưới rào Blender nhập vào Unity
  **XOAY 180°: Unity x = −Blender x, z = −Blender y** (tôi từng lấy x = +Blender x — đất vành kéo ngược rào; so chân cột thì phải xem
  **độ lệch chuẩn**, trung bình không lộ). ⚠️ **Đừng chạy lại menu 54** (xoá mất 4 lò vành — menu đã tự chặn). Vũng nước thêm sau hạ
  mực dưới gờ tràn (`DungMatNuoc(..., haTheoGoTran)`) — đĩa nước không bao giờ nổi viền tròn. Menu 54b đếm 14 lò.
  **Cổng rào** (duy nhất, cạnh bắc Unity x ≈ +12,7) dựng lại bằng Blender MCP: vòm đá liền khối + hai cánh cổng sắt ĐÓNG (người dùng
  chọn; trước là 11 viên đá vòm rời lơ lửng + tường thấp) — song cắm xuống đất từng chỗ, rào kín 0/2096 đường lọt.
  ⚠️ **Nhà mồ dùng LƯỚI VA CHẠM HAI MẶT** (menu 88, `BlenderMaps/GraveyardAct2/VaCham/`): lưới Blender có mặt lật pháp tuyến vào
  trong, CharacterController xuyên tường vào được rồi KẸT (menu 87: 18/18 lần; nay 0). Thêm nhà mồ mới / nhập lại map → chạy lại menu 88.
- ⚠️ **Máu người chơi = 1000** (05/10/2026 người dùng: "khi mới vào game cho máu người chơi là 1000"; cùng ngày trước đó 700; 27/09 là 10 000
  để chạy thử, 12/09 mức thật 600) — sửa đủ cả ba chỗ và `ThuMauKhoiDau.MauMongDoi`; menu 40: mình 1000/1000, bản sao người chơi khác 1000/1000. Con số nằm ở ba chỗ —
  `GameBootstrap.playerMaxHealth`, scene Act2, `Player_Sorceress.prefab` — và menu 40 kiểm cả ba (kể cả bản sao
  người chơi khác, vì nó lấy máu thẳng từ prefab).
- ⚠️ **MANA KHỞI ĐẦU 180 — CẢ NGƯỜI CHƠI LẪN BOT** (09/10/2026 người dùng; trước 250): số nằm ở BA chỗ như máu — `GameBootstrap.playerMaxMana`
  (code + scene Act2), `Player_Sorceress.prefab` (bản sao người khác + máy BOT lấy thẳng từ đây), nhân vật đặt sẵn trong Act2. Menu 40 kiểm cả
  mình lẫn bản sao. **Mana cấp 1 mới** (cùng ngày, code + prefab + Act2): Cầu lửa 25 · Thiên thạch 45 · Lửa địa ngục 40 · Cầu băng 25 · Mưa băng 45 ·
  Tàng hình 40 · Giựt sét 25 · Sấm sét 45 · Cầu điện 55 · Gió lốc 40 · Lốc xoáy 65 · Hoá lốc xoáy 45 · Mây giông 50 · Khiên 45 · Tốc biến 40; mỗi cấp +10%
  nhân dồn — ⚠️ **BỎ ngoại lệ "Gió lốc cấp 5 chỉ 25"** (người dùng chọn; cấp 5 = 58,56; `GioLoc.NangLuongCap5` đã xoá). **Hồi chiêu 0,9 s** cho
  Cầu lửa · Cầu băng · Giựt sét · Gió lốc (người dùng "delay skill 0,9 giây", chọn = hồi chiêu; trước 0,55 / 0,55 / 0,4 / 0,4).
  ⚠️ **10/10/2026: Gió lốc 40 → 35, Lốc xoáy 65 → 50, Mây giông 50 → 45** (code + prefab + Act2; Mây giông là hằng `MayGiong.NangLuong`). Menu 71, 83: 0 lỗi.
- ⚠️ **BỊ LỐC XOÁY CUỐN KHÔNG DÙNG ĐƯỢC KỸ NĂNG** (10/10/2026, người dùng — người chơi lẫn BOT; chọn: ngắt phép đang niệm, bình VẪN uống được,
  Tốc biến cấp 5 cũng bị chặn): `PlayerController.LyDoKhongTungDuoc` thêm `WhirledEffect` → "BẠN ĐANG BỊ LỐC XOÁY CUỐN!"; tham số `choUongBinh`
  (bình chỉ bỏ qua Lốc xoáy — choáng / đóng băng / ngã / hất tung VẪN chặn cả bình như trước); `WhirledEffect.Catch` gọi `NgatChieu`. Menu 99 F
  (0 lỗi): đang niệm bị cuốn → ngắt 1, 0 quả; đang cuốn Quả cầu lửa / Tốc biến cấp 5 bị từ chối, mana giữ, bình máu 1000 → 1200; thoát lốc → tung
  lại được. Menu 99 A sửa: Lốc xoáy 18 m, Thiên thạch 23 m (viết tay).
- ⚠️ **HIỆU ỨNG KỸ NĂNG MỚI (09/10/2026, người dùng — mọi dòng cũ "40% đóng băng / 15% choáng / 80% hất tung / chậm 50% 2 s / hất 0,7 s" bên
  dưới là LỊCH SỬ)**: **Quả cầu lửa** cấp 1–4 mỗi quả (cả quả nảy) **10% đánh ngã 1 s** (`Fireball.NgaXacSuatThuong` / `NgaGiayThuong`; người dùng
  chọn cấp 5 GIỮ 30% / 1,5 s); **Quả cầu băng** đóng băng **20% · 1 s**, chậm ~~80% · 3 s~~ **50% · 2 s** mọi lần trúng (`QuaCauBang.XacSuatDongBang`, `GiayDongBang`,
  `TiLeCham`, `GiayCham` — ⚠️ cùng ngày người dùng đổi lại phần chậm: **luôn chậm 50% · 2 s** như cũ); **Giựt sét** choáng **20%** · 1,5 s (`GiatSet.XacSuatChoangNguoiChoi`); **Gió lốc** hất tung **20% · 0,8 s** (`GioLoc.XacSuatHatTung`,
  `BiHatTung.GiayMacDinh` — chỉ Gió lốc dùng). Thời gian vẫn +0,15 s mỗi cấp. Menu 68, 69, 70, 71, 82: 0 lỗi.
- ⚠️ **09/10/2026 (lần ba, người dùng): THIÊN THẠCH TẦM 23 m + TỰ NHẮM · CẦU BĂNG 4/6 QUẢ · ẨN HOÁ LỐC XOÁY · HẤT TUNG KIỂU MỚI 1,2 s**:
  **Thiên thạch** `ThienThach.Tam` 23 m (`TamNgam(4)`; Lốc xoáy / Quả cầu điện GIỮ 18 — người dùng chọn); mỗi quả lúc bắt đầu rơi chọn kẻ địch trong
  `BanKinhTuNham` **5 m quanh chỗ ngắm** (bỏ người tung / đồng đội / Tàng hình / dưới vực): ít quả đã nhắm nhất, bằng thì gần chỗ ngắm nhất → mỗi quả
  một kẻ, hết kẻ quay vòng; không ai thì rơi lệch ngẫu nhiên như cũ (`ThienThach.LoatNham`, `MucTieuNham`; chỉ người chơi — Quỷ dữ không). ⚠️ Trên
  máy tính Thiên thạch vẫn KHÔNG kẹp tầm theo chuột (`TamCuaKyNang(4)` = 0 từ trước) — tầm 23 m là vạch ngắm cảm ứng + BOT. **Quả cầu băng**
  `SoQuaThuong` 4, `SoQuaCap5` 6. **Hoá lốc xoáy ẨN HOÀN TOÀN** (`CapDo.AnHoaLocXoay` + `KyAn`): bỏ khỏi `SachPhep.KyNangTheoNhom`, ô đã lưu có 14 →
  ô trống, `CastAt` từ chối, BOT không cộng điểm / không dùng; hiện lại: đặt false + chèn lại vào nhóm PHONG + kế hoạch BOT. Phép thử Hoá lốc xoáy bật
  `CapDo.ChoPhepKyAnChoPhepThu` (menu 75, 84). **Gió lốc hất tung** (`BiHatTung`, `GiayMacDinh` **1,2 s**, `GocNgua` **90°**): LÊN 55% (nâng 3 m
  SmoothStep, đứng thẳng 45% chặng rồi ngã nằm ngửa ngang xong đúng đỉnh) → RƠI 30% giữ nằm ngang (1 − t²), thân hạ dần cho LƯNG chạm đất (trục thân
  0,22 m) → DẬY 15%. Menu 62 H (0 lỗi): 5 quả → quái A (2 m) 3, B (3,4 m) 2, quái ở 7 m 0, rơi đúng dưới chân; đối chứng không ai → lệch ≤ 1,82 m.
  Menu 68: 4 / 6 quả, phát lại qua mạng 4. Menu 71 G: cao 3,00 m, bay 1,21 s, ngã 90°, lên 0,66 s / xuống + dậy 0,55 s, nằm ngang lúc tới đỉnh và lúc
  chạm đất 38/38, hông thấp nhất −0,68 m (lưng sát đất). Menu 75 Z: ẩn đủ 4 chỗ, `CastAt(14)` khi có Gió lốc đang bay → 0 Lốc xoáy, không trừ mana.
  Menu 82, 84, 85, 115, 66: 0 lỗi; 59 chập chờn F2 (quầng đập nhịp ×1,37 / 1,47 quanh ngưỡng 1,4 — có từ trước).
- ⚠️ **CHÂN NGƯỜI CHƠI CHẠM ĐẤT** (05/10/2026, người dùng: "trong game rất nhiều chỗ 2 chân nhân vật đứng vẫn như bay lơ lửng"; `NguoiChoiHoatHinh`,
  phần "CHAN CHAM DAT" — nhân vật mình, bản sao người chơi khác, nhân vật màn chính dùng chung). Nguyên nhân (menu 109, 40 chỗ Act2): gốc LUÔN cao hơn
  đất **0,08 m** (`skinWidth` CharacterController, đáy con nhộng đặt đúng ở gốc), tư thế đứng yên là khung BƯỚC DỞ của clip đi (một chân nhấc ~5 cm),
  đất dốc thì một chân hở tới 0,155 m. Sửa mỗi khung SAU mọi tư thế: (1) đứng yên (tròn theo 1 − mucDi) chân về bind pose khép chữ A (`GocChanBind`,
  `HeSoKhepChan` 0,4 — hai chân THẲNG bằng nhau như màn chính); (2) đế giày = min(Foot − 0,135, ToeBase − 0,036) (BakeMesh bind pose) so với mặt đất
  dưới (tia, các lớp CharacterController va chạm), **hạ thân qua xương HÔNG** (đứng yên: bên hở nhiều nhất chạm đất; đang đi: chân trụ chạm đất) — clip
  ghi vị trí hông mỗi khung, đứng yên `TraVeTuTheChuan` đặt lại → không cộng dồn (`BatDauHoaVe` trừ phần đã hạ); (3) bên đất cao **IK hai đoạn** nhấc
  lên + bàn chân **nghiêng theo mặt đất** ≤ 30° (`thichNghiDoc`; màn chính TẮT để giữ hai chân thẳng bằng nhau người dùng đã duyệt). Bỏ qua khi bị
  đánh ngã / hất tung / chết. Menu 109 (0 lỗi, đối chứng `chamDat = false` cùng chỗ): đứng yên hở lớn nhất **0,012 m** (cũ 0,155; hở > 2 cm 0/40 chỗ,
  cũ 40/40), lún nhất 0,004; đang đi chân trụ TB −0,008 m (cũ +0,043). Menu 101c, 69: 0 lỗi. ⚠️ Menu 85 mục B (Bộ xương chết lúc đang ngã, đỉnh hình
  1,00 m ≥ 1,0) hỏng CẢ khi tắt `chamDat` → lỗi có từ trước (sau 04/10), không do phần này.
- ⚠️ **ĐỊA NGỤC NGOÀI BẢN ĐỒ** (05/10/2026, người dùng: "bên dưới ngoài vùng bản đồ là địa ngục, té xuống là mất máu tới chết"; chọn vực thẳm +
  dung nham, mở vài đoạn rào SẬP, 20% máu tối đa / giây, quái cũng vậy). **Luật chơi xong** (`Combat/DiaNguc.cs`, dựng trong `GameBootstrap.Awake`):
  ai xuống dưới `NguongRoi` −6 m (đất Act2 thấp nhất −2,9) thì cứ 0,25 s mất 5% máu TỐI ĐA tới chết — Physical (không kháng), sát thương RỈ (Bộ
  xương không đỡ được), chỉ máy QUYẾT MÁU trừ; kẻ hạ = `keDanhCuoi` chụp lúc vừa rơi qua ngưỡng (đẩy người / quái xuống vực vẫn được kinh nghiệm);
  đáy = BoxCollider lớp Ground mặt trên −26,5 (`MucDungNham` −26). ⚠️ Vì đáy là lớp Ground nên Tốc biến / lốc ra ngoài mép giờ rơi xuống đáy vực.
  Menu 110 (0 lỗi): nhân vật ra ngoài mép qua ngưỡng 0,77 s, chạm đáy 1,5 s, máu 1000 → 800 → … chết 4,98 s; Bộ xương (45% đỡ đòn) chết 4,84 s,
  kẻ hạ = người chơi; bản sao quái không bị trừ; đối chứng đứng trong bản đồ 3 s không mất máu.
  **Phần HÌNH** (Blender MCP `CongCu/Blender/dia_nguc.blend`, scene `DiaNguc` + `DN_NuongAnh`): **4 đoạn rào SẬP** (ô rào giữa hai cột bỏ hẳn, hai mẩu
  bệ đá gãy lởm chởm sát cột — trong lưới rào nên có va chạm; khe đi được ~3 m) ở Unity nam (22,9; −66,4) · bắc (−28; 66,4) · tây (−66,4; −12,7) · đông
  (66,4; 33,1), tránh cổng — `hang_rao_rong.fbx` xuất lại (xuất thử bản chưa sửa khớp FBX cũ: 89 960 đỉnh, cùng khung bao); ⚠️ lưới rào KHÔNG có UV (đá
  triplanar) — đừng thêm. `Resources/DiaNguc/DiaNguc.fbx`: `VachVuc` (vách vực quanh mép đất, mép trên = độ cao đất từng 1 m đọc từ Unity, đá lởm chởm,
  chân loe dốc đá vụn, 11 880 đỉnh), `VachNgoai` (vòng vách quay VÀO trong ở ±112 m, −27,5 → +12 m), `RaoSap` (mảng song
  đổ ra mép vực thiếu ~30% song, đá vụn, song rơi — KHÔNG va chạm); màu đỉnh R ánh dung nham / G vết nứt / B tối. Ảnh nướng lặp liền mạch (nhiễu 4D trên
  hình xuyến): `DungNham.png`, `DaBazan.png` (đá phân tầng), `KhoiVuc.png`. Shader `S_VachDiaNguc` (triplanar + phát sáng theo màu đỉnh), `S_DungNham`
  (2 lớp trôi + loang cỡ lớn, sương mù chỉ ÁP MỘT PHẦN), `S_SuongVuc` (2 lớp khói đỏ sẫm ở −12 / −19 m — thiếu nó dung nham sáng đều như tấm thảm sát
  chân rào, không thấy sâu). `DiaNguc.DungHinh` nạp lúc chạy (không sửa scene) + tàn lửa / khói dọc 4 cạnh. ⚠️ Bẫy đã vấp: kiểm hướng mặt bằng MỘT mặt
  (rơi vào hàng mép giấu gần nằm ngang) → cả vách quay vào trong; vòng vách ngoài quên gán z → nằm bẹp ở 0 (vệt đen trên dung nham). Menu 110 (0 lỗi):
  E đi qua khe rào sập rơi xuống vực, F đối chứng rào nguyên bị chặn z −65,7; ảnh `PlayTestShots/dianguc_25d|3d|ngoai|canh.png`. Menu 56: 0 lỗi.
  ⚠️ **Cùng ngày: BỎ 14 CỘT ĐÁ** `CotDa` (tôi tự thêm — người dùng hỏi "cây trụ là gì", ở đáy vực trông như ống trơn; chọn "vực chỉ còn vách, dung
  nham và khói"; xoá trong Blender, xuất lại `DiaNguc.fbx`) + **tàn lửa / khói bay lên NHIỀU HƠN ×1,5** (`DiaNguc.TanLuaMoiGiay` 32 → 48, `KhoiMoiGiay`
  4,5 → 7 mỗi cạnh, bay lên 2–4,5 / 1,2–2,4 m/s). Menu 110 (0 lỗi): 0 cột; tàn lửa sống 1 265 (mức cũ ~832, ×1,52) lên TB 3,23 m/s; khói 288 (×1,60).
  ⚠️ **Cùng ngày: DUNG NHAM CHẢY THEO CÁC ĐƯỜNG GÂN** (người dùng: "cho thấy rõ các dòng dung nham đang chuyển động và chảy"; chọn "chảy theo các
  đường gân trong hình", tốc độ chọn qua ảnh động): ảnh `Resources/DiaNguc/HuongGan.png` (Blender MCP, numpy trên `DungNham.png`: RG = hướng tiếp tuyến
  gân dạng GÓC KÉP × độ kết hợp từ tensor cấu trúc, B = nhiễu dải tần lặp liền mạch); `S_DungNham.LopChay` lấy hướng gân, dấu theo dòng chảy vòng
  quanh bản đồ, **flow map hai pha** dời nhiễu → đợt sáng chạy dọc gân (`_TocChay` **0,8 m/s — người dùng chọn qua ảnh động** 0,8 / 1,5 / 2,5, `_QuangChuKy` 2 m — chu kỳ = 2 m / tốc; chu kỳ cố định làm
  mẫu méo ở tốc cao); đồng hồ toàn cục `_DN_ThoiGian` (`DiaNguc.Update`; phép thử giữ bằng `GiuDongHoDungNham`). Menu 111 (0 lỗi, hướng gân + chuyển động
  tính lại từ ẢNH CHỤP, đối chứng `_DoChay` 0): chuyển động ×6–7 hình cũ, xuôi dòng 64–90% (cũ 26–43%), tốc trung vị 0,42–0,72 / 1,0–1,48 / 2,2–3,0
  ở 0,8 / 1,5 / 2,5 m/s. ⚠️ "Dọc gân" bị hiệu ứng khẩu độ (đối chứng cũng 61–80%) — không dùng để phân biệt. Ảnh động menu 111b
  `PlayTestShots/dungnham_chay_*.gif`.
  ⚠️ **Cùng tối: ẢNH DUNG NHAM VẼ LẠI** (người dùng: "đường dung nham còn thẳng và trơn quá"; chọn gân lởm chởm + màu theo độ nóng + bỏ lớp gân thứ
  hai): `CongCu/Blender/dung_nham_gan.py` (Blender MCP numpy, chạy bằng timers ~3,5 phút) — cạnh Voronoi trên toạ độ bẻ méo 3 tầng, độ rộng đổi theo
  chỗ, nhánh nứt phụ, vũng nóng chảy, bảng màu theo độ nóng; `HuongGan.png` tính lại. Shader: lớp thứ hai = mip 5 mờ ×0,18 (quầng đỏ dưới vỏ), ~5 lần
  đọc ảnh mỗi điểm. Menu 111 TẮT vỏ trôi khi đo (vỏ mới nhiều chi tiết — tương quan bám vào vỏ trôi 0,13 m/s): 0 lỗi, ×18 đối chứng, xuôi dòng 69–86%.
  ⚠️ **06/10/2026: VỎ SẦN + ÁNH LỬA MÉP VỎ + GÂN UỐN LƯỢN** (người dùng chọn mục 3 + 5): bản đồ độ cao vỏ (gờ mép, nếp dây thừng 0,55 m, tảng nghiêng —
  phải khối LỚN, chi tiết 13–19 cm bị mip gộp mất ở cự ly chơi) nướng bóng vào màu; ALPHA `DungNham.png` = ánh lửa mặt dốc nhìn về khe, shader
  `_AnhVien` cho đập theo đợt sáng; uốn gân `_UonGan` 0,25 m / `_TocUon` 0,4 m/s (bẻ toạ độ cả ảnh lẫn hướng gân). Menu 111 C: ánh mép dao động ×5 đối
  chứng; D: gân dịch 0,36 m / 7,85 s (đối chứng 0). Trong game vỏ chỉ rõ hơn +24% vì khói `SuongVuc` nén mảng tối.
- **LUẬT ĐỢT QUÁI** (luật duy nhất của `GameDirector` từ khi xoá Act1): mỗi đợt sinh quanh **từng**
  người chơi bốn con (bộ xương · phù thủy · quỷ cây · quỷ dữ), giết hết → 30 giây → đợt sau cộng dồn thêm quái bất kì
  (+1, +3, +6…) và mạnh thêm 5% máu · sát thương mỗi đợt. ⚠️ **Sát thương quái Act2 = 0,65 × 1,05^(đợt−1)** (27/09/2026 người dùng: đợt đầu −35%, máu giữ
  nguyên — `GameDirector.HeSoSatThuongDotDau`; menu 56 mục B4b đo cả 4 loại so với con sinh thẳng từ kho quái). **Vào trận chờ đúng 30 giây** mới ra đợt đầu (`GiayChoDotDau`).
  **Mỗi đợt thêm 20 con vòng ngoài + 10 con cho MỖI người chơi thêm** (26/09/2026, người dùng; trước cố định 20 cho cả phòng:
  1 người 20 · 2 người 30 · 3 người 40 · 4 người 50 — `GameDirector.SoQuaiXaCho(số người CÒN SỐNG)`, `SoQuaiXaThemMoiNguoi`),
  loại ngẫu nhiên, cách người chơi GẦN NHẤT **20–25 m** (13/09/2026 đổi từ 10 con ở 55–65 m); không đủ chỗ thì thả gần khoảng
  nhất có thể và đếm vào `SoQuaiXaDungKhoang`. ⚠️ **Kinh nghiệm giết QUÁI ×6,318 so với gốc** (26/09/2026: +35% rồi +20%; 28/09/2026 +30%; **09/10/2026 +100% rồi +50%** mọi đợt, đều trên mức đang có — `CapDo.HeSoKnQuai` = 1,35 × 1,20 × 1,30 × 2 × 1,5 nhân `KnGocCuaQuai`; bộ xương 18 → 114 … quỷ khổng lồ 70 → 442; hạ người chơi vẫn 250) — chơi
  một mình cấp 8 sau đợt 1, cấp 10 sau 2 đợt, cấp 20 sau 7 đợt (menu 60: 0 lỗi).
  ⚠️ **09/10/2026: ĐỢT 1–3 CHỈ MỘT NỬA SỐ QUÁI** (người dùng; chọn chỉ 3 đợt đầu): quanh mỗi người **2 loại ngẫu nhiên khác nhau**, cộng dồn
  và vòng ngoài chia đôi làm tròn lên (`GameDirector.SoDotGiamQuai` 3, `SoQuanhMoiNguoi` / `SoCongDonChoDot` / `SoQuaiXaChoDot` / `TongQuaiDot`)
  — một mình 12 / 13 / 14 con, 2 người 19 / 20 / 21. ⚠️ Cùng ngày (lần hai) **đợt 4 cũng giảm** (`SoDotGiamQuai` 4: một mình 15, 2 người 22),
  từ đợt 5 như cũ (2 người 48). Menu 56 (số viết tay), 60, 64: 0 lỗi
  (`kinhnghiem.md`). Menu 60, 56 (2 người → 30 con), 64 kiểm.
  **Sau 60 giây con vòng ngoài nào còn sống tự truy lùng người gần nhất** (`EnemyAI.HenTruyLung`, bỏ giới hạn
  `aggroRange` 14 m); đang truy lùng mà kẹt thì vòng vật cản, kẹt mãi thì đổi chỗ sang 10–14 m cạnh người chơi,
  quái đánh xa bị kẹt trong 1,5× tầm thì bắn tại chỗ. Quái thường KHÔNG có (menu 64 kiểm, có đối chứng). Không rải quái sẵn, không dòng quỷ dữ/quỷ cây riêng,
  không chế độ bốn bộ xương (đã xoá cùng Act1). Menu 56 kiểm. Chỗ xuất phát của mỗi người: ngẫu nhiên theo **mã phòng** (`ChoXuatPhat.cs`),
  cách nhau ≥ 22 m — mọi máy tính ra cùng một danh sách nên không cần gói tin nào.
- **Hai mươi ba kỹ năng** (+ **Nhào lộn (22)**, 10/10/2026, nhóm HỖ TRỢ — xem mục NHÀO LỘN): Cầu lửa, Mưa băng, Sấm sét, Lốc xoáy, Thiên thạch, Khiên, Giựt sét (0–6) + **Bình máu (7), Bình mana (8)**
  (13/09/2026) + **Quả cầu băng (9)** + **Gió lốc (10)** (16/09/2026) + **Lửa địa ngục (11)** (17/09/2026) + **Tàng hình (12)** + **Quả cầu điện (13)** + **Hoá lốc xoáy (14)** + **Tốc biến (15)** (18/09/2026) + **4 kỹ năng BỊ ĐỘNG: Kháng Lửa (16), Kháng Băng (17), Kháng Sét (18), Kháng Phong (19)** (19/09/2026) + **bị động Tốc độ di chuyển (20)** (25/09/2026) + **Mây giông (21)** (25/09/2026, nhóm PHONG). Vẫn **7 ô** (người dùng chọn) — kỹ năng không có sẵn ô phải kéo vào ô trong Sách phép.
  ⚠️ Số hiệu mới luôn **thêm ở cuối**, không chèn: số hiệu đi qua gói tin và nằm trong thứ tự ô đã lưu của người chơi.
- **Quả cầu băng** (`Skills/QuaCauBang.cs`, hình `Vfx/VfxQuaCauBang.cs`): 3 quả toé quạt như Quả cầu lửa (đường bay/va chạm chép
  `Fireball.Update`), sát thương gốc **65**, hồi chiêu **0,55 s**, 12 năng lượng, nổ vùng 3,4 m; trúng là **chậm 50% trong 2 s** và **40% ĐÓNG BĂNG 1,5 s**
  (18/09/2026: không đi, không tung phép — `FrozenEffect.Apply` như Mưa băng, qua mạng bằng bit `CoBangHoanToan`); **cấp 5 ra 5 quả** (`SoQuaTheoCap`). Hình **dựng bằng Blender MCP** (`CongCu/Blender/qua_cau_bang.blend`) → `Resources/KyNang/QuaCauBang/`
  (ảnh sương lạnh, mảnh băng) + icon `Resources/Icons/CauBang.png`; dựng bằng code, **không** có prefab trong GameAssets
  (thêm trường prefab là phải sửa hai scene). Menu 68 kiểm.
  ⚠️ **28/09/2026 QUẢ CẦU VẼ LẠI: KHỐI BĂNG PHA LÊ** (người dùng chê bản cũ "khối tròn đính gai, sơ sài", chọn "khối băng pha lê",
  tông xanh lam) — Blender MCP `CongCu/Blender/cau_bang_pha_le.blend` → `CauBangPhaLe.fbx` (khối đẽo thô 63 mặt cắt + 6 tinh thể
  lăng trụ MẬP NGẮN, 212 tam giác) + `CauBangPhaLe.png` (nướng: R vết nứt, G bọt khí, B sương giá — **tuyến tính, tắt sRGB**) +
  `CauBangPhaLe.mat` (shader MỚI `Diablo25D/CauBangPhaLe`: mặt cắt phẳng lóe sáng, nứt "chìm sâu" theo góc nhìn, viền fresnel;
  hai lượt: ghi độ sâu rồi trộn; hàng đợi 3001). Lõi sáng là **đốm hạt mềm** `LoiSang` (hàng đợi 2999, vẽ trước lớp băng). Quay
  lăn trục nghiêng `VfxFactory.TrucLanKhoiBang`. ⚠️ Vết nứt phải TRỘN VỀ TRẮNG ĐỤC, **không cộng sáng** — cộng sáng ra lưới điện
  y Quả cầu điện; tinh thể dài mảnh đọc ra GAI (cái người dùng chê); lõi bằng lưới hiện thành khối xanh cạnh sắc — cả ba đã thử, bỏ.
  Vật liệu `.mat` giữ giá trị riêng: đổi mặc định trong shader phải cập nhật cả `.mat`. Bản cũ `QuaCauBang.fbx` đã xoá.
  ⚠️ **TẢNG BĂNG TRÊN ĐẤT (Quả cầu băng + Mưa băng) CŨNG LÀ KHỐI BĂNG PHA LÊ** (28/09/2026, người dùng: "quá chói, thô sơ sài"):
  Blender MCP `CongCu/Blender/tang_bang_pha_le.blend` → `TangBangPhaLe.fbx` (4 cụm `CumBang0–3`: 3–6 lăng trụ băng đẽo đỉnh gãy vát
  nghiêng ra + tảng thấp ở chân, xuất với `bake_space_transform` để lưới có trục Y lên — `bakeAxisConversion` của Unity KHÔNG
  ăn) + `TangBangPhaLe.png` (sương dày ở chân) + `TangBangPhaLe.mat`. Prefab `Vfx_NoBang` GIỮ NGUYÊN; `VfxFactory.NangCapTangBang`
  (gọi trong `IceImpact`, TRƯỚC Start của ExpandFade) thay lưới + vật liệu từng `CumGai*`, xoay ngẫu nhiên, quầng chân 1,15 → 0,5.
  Shader có `_AlphaGoc`: độ hiện = `_Color.a / _AlphaGoc` nên ExpandFade vẫn làm mờ được, cấp 5 `TangBangNo` tắt mờ vẫn đứng đặc.
  ⚠️ **MẢNH VỠ BĂNG CŨNG LÀ PHA LÊ 3D** (28/09/2026, người dùng: "mảnh nhỏ chỉ là hình tam giác, sơ sài"): Blender MCP
  `CongCu/Blender/manh_bang_pha_le.blend` → `ManhBangPhaLe.fbx` (8 mảnh: 4 phiến viền lởm chởm, 2 cục, 2 kim; 14–28 tam giác, dài
  nhất 1 m, **Read/Write BẬT** cho hạt dạng lưới) + `.png` + `.mat` (shader `Diablo25D/ManhBangPhaLe`: một lượt, NHÂN MÀU HẠT).
  `VfxFactory.DoiThanhManhBangPhaLe` đổi hạt sang dạng LƯỚI (8 mẫu ngẫu nhiên, góc 3D ngẫu nhiên, lộn nhào 3 trục cùng kiểu
  TwoConstants): `Manh3D` + `Shards` (×0,8) của `Vfx_NoBang` (trong `NangCapTangBang`), `Shards` của `Vfx_VoBang` (`FrozenShatter`),
  `ManhBangRoi` + `ManhBung` của quả cầu băng. Prefab giữ nguyên.
  ⚠️ **VỤ NỔ BĂNG KHÔNG CÒN QUẢ CẦU CHỚP SÁNG** (28/09/2026, người dùng: "đổi thành vòng sương băng cho dịu lại"): `ThayChopBangVongSuong`
  (trong `NangCapTangBang`) TẮT con `Flash` (cầu shader Ice + bloom = mái vòm trắng chói), thêm `VongSuongBang` (tấm sát đất, ảnh Blender
  MCP `CongCu/Blender/vong_suong_bang.blend` → `VongSuongBang.png` alpha từ độ xám, loang 0,37 → 1,4 bán kính trong 1 s) + `SuongVong`
  (12 cụm sương `SuongLanh` toả từ mép vòng). ⚠️ Tấm sương billboard cắm xuyên đất để lại **đường thẳng sắc** (tưởng mặt nước, ẩn hồ
  vẫn còn): shader `Diablo25D/ParticleAlphaSatDat` mờ dần 0,6 m sát đất, độ cao đất qua MaterialPropertyBlock (`MemSatDat`) — áp cho
  `SuongVong`, `Mist` của `Vfx_NoBang`, `SuongBung` của quả cầu. Vật liệu gốc `Resources/KyNang/QuaCauBang/SuongSatDat.mat` giữ shader
  trong bản build. Menu 68 mục J nay so HỆ SỐ PHÓNG vụ nổ (trước vô tình đo quả cầu Flash).
  Cụm gai băng trên đất của **Mưa băng** dùng chung cỡ hình `QuaCauBang.BanKinhHinhBang` (2,55 m, người dùng xin 16/09/2026) —
  chỉ hình, vùng sát thương mỗi tảng vẫn 1,7 m (menu 68 mục J đo kích thước thật).
  **Mưa băng rơi QUẢ CẦU BĂNG** thay tảng băng (16/09/2026): `VfxFactory.QuaCauBangRoi` (luồng khí lạnh/vệt băng như Quả cầu băng, bản nhẹ:
  nửa lượng hạt, không đèn riêng), rơi **THẲNG ĐỨNG** (người dùng chốt 16/09/2026 — tôi từng cho rơi chéo, người dùng không muốn);
  `FallingShard` giữ nguyên rơi/sát thương/đóng băng, chạm đất thì `ThaDuoiQuaCauBang`. Menu 68 mục K.
  **Quả rơi KHÔNG CÒN KHỐI CẦU — chỉ vệt sáng băng như sao băng** (17/09/2026, người dùng; trước đó cùng ngày là cầu tròn bọc khí lạnh,
  đã bỏ): `VfxFactory.DungVetSaoBang` (TrailRenderer 0,17 s, rộng 0,66→0,36 m × ngẫu nhiên 0,7–1, đầu là tấm `DauSaoBang` giọt sáng mũi nhọn răng cưa xoay theo hướng rơi (`DauSaoBangHuong`); ảnh `VetSaoBang.png`/`DauSaoBang.png` vẽ bằng Blender MCP `CongCu/Blender/vet_sao_bang.blend`),
  giữ hào quang, luồng khí lạnh, mảnh băng; chỉ bản `banRoi` — kỹ năng Quả cầu băng vẫn lõi có gai.
  ⚠️ **TẢNG BĂNG CHỈ MỌC KHI ĐÓNG BĂNG ĐƯỢC** (người dùng 19/09/2026; trước đó 17/09 là "trúng quái / người chơi khác",
  16/09 là cả đồ vật): mỗi kẻ bị `FrozenEffect` đóng cứng → **1 tảng băng dưới chân kẻ ấy** (`CombatUtil.AreaFreeze` trả
  danh sách kẻ bị đóng); không đóng được ai → `IceImpact(..., coGai:false)` tắt `CumGai*` + `HaoQuang*`, giữ chớp, vòng lạnh,
  sương, giọt nước, mảnh băng, vết sương giá. ⚠️ Vì thế `FallingShard` **gây sát thương TRƯỚC, dựng hình SAU** (bản cũ ngược lại) —
  phải gieo xác suất xong mới biết ai bị đóng băng. Tỉ lệ có tảng băng ≈ xác suất đóng băng 35% (menu 68 mục L đo 10/36).
  ⚠️ **CẤP 5: tảng băng hết giờ thì NỔ**, +100 cố định trong 3,4 m (`Combat/TangBangNo.cs`, hình `Vfx/VfxTangBangNo.cs`) —
  cho CẢ Mưa băng lẫn Quả cầu băng (Quả cầu băng vẫn mọc tảng băng mọi lần nổ, không cần đóng băng được ai). Cấp đi theo
  NGƯỜI TUNG: `IceStorm.capKyNang` → `FallingShard.capKyNang`, `QuaCauBang.capKyNang`. ⚠️ **Nổ ĐÚNG LÚC cụm gai tan** (sửa 26/09/2026 — người dùng
  thấy "khối băng biến mất một lúc rồi mới nổ"): mỗi cụm gai có `ExpandFade` riêng 1,6–2,6 s mờ rồi tự xoá, còn vụ nổ từng hẹn theo
  `AutoDestroy` 4 s của cả tảng → trống 1,3–2,3 s. Nay `TangBangNo.Gan` kéo mọi cụm gai tới CÙNG một mốc (cụm sống lâu nhất), TẮT mờ dần,
  co đường cong mọc để giữ tốc độ trồi lên, và nổ sớm hơn mốc ấy **0,12 s** (`NoSomHon`) rồi xoá cả tảng. Menu 68 mục N2 đo từng khung. ⚠️ Vụ nổ **không được**
  gọi `VfxFactory.NoQuaCauBang` (hàm ấy gọi `IceImpact` → tảng băng nổ ra tảng băng, vô tận). Menu 68 mục L + N, menu 61 ca B2b.
- **Gió lốc** (`Skills/GioLoc.cs`, hình `Vfx/VfxGioLoc.cs`, hiệu ứng `Combat/BiHatTung.cs`, 16/09/2026):
  ⚠️⚠️ **01/10/2026: HÌNH GIÓ LỐC = LỐC XOÁY THU NHỎ ×0,318** (người dùng: "hiệu ứng giống hoàn toàn Lốc xoáy, kích thước bằng Gió lốc
  hiện giờ"; chọn thu đều · bỏ màu mây giông + mây trong thân · giữ 240 hạt/giây · Hoá lốc xoáy phình từ ×0,318). `BuildGioLoc` nhân bản
  con `LocXoayHinh` của **prefab Skill_LocXoay**, thêm các lớp bụi của `Tornado.Start` (`DamBaoBuiCuonLenLocXoay`), phóng ×`HeSoHinhGioLoc`
  (5 / 15,72); MỌI hệ hạt CỤC BỘ + Hierarchy (bụi chân Lốc xoáy vốn THẾ GIỚI — ở 9,5 m/s thành vệt dài), **trọng lực hạt × 0,318**
  (trọng lực là m/s² thế giới, không thu theo tỉ lệ — bụi chân từng bay cao ×1,47), đèn StormLight thu tầm (`LightFlicker.DatTamGoc`),
  tắt đèn khi tan. Tia sét = `TornadoBolt(×0,318, "SetTrongGioLoc")`. Gameplay (tốc độ, vùng trúng 2,42, hất tung, sát thương) GIỮ NGUYÊN.
  ⚠️ **05/10/2026 ĐÃ XOÁ HÌNH CŨ** (người dùng): `BuildGioLocCu` + `LocNho.fbx`, `GioDai.png`, `GioSoi.png`, mây trong thân, bảng bán kính vỏ cũ,
  menu 71c / 94 (còn trong git trước commit này); giữ `BuiDenCuon.png` (hạt đen cuốn lên). Thiếu FBX Lốc xoáy thì `BuildGioLoc` dự phòng bằng
  `BuildTornadoCu(×0,318)`. Đối chứng menu 71 C4 nay là **Gió lốc mới nằm ngang** (IoU 0,45 so với 0,74 — bản lật ngược ra 0,70, không phân biệt
  được); menu 82 B3 kiểm vòng bụi Gió lốc = vòng Lốc xoáy ×0,318 (1,272 m). Menu 71 mục C so từng thứ với **Lốc xoáy thật**: 10/10 thành phần, phân bố bụi khử tỉ lệ, ảnh IoU 0,94 / sáng ×1,01 (cũ 0,49 / ×0,52).
  Mọi dòng bên dưới về lưới `LocNho`, màu mây giông, mây trong thân, bụi ×2, tia trong lòng vỏ là **LỊCH SỬ** (gameplay vẫn đúng).
  ⚠️ **01/10/2026 (lần hai): tia sét Lốc xoáy + Gió lốc = KIỂU SẤM SÉT** (xem mục Lốc xoáy) và bụi lên đỉnh ×2 — Gió lốc theo luôn
  (cùng `TornadoBolt` ×0,318, cùng `DamBaoBuiCuonLenLocXoay`): mỗi cơn ~930 hạt bụi sống.
  ⚠️ **01/10/2026 (lần ba): GIÓ LỐC KHÁC LỐC XOÁY BA CHỖ** (người dùng; `VfxFactory.KhacLocXoay`, gọi trong `BuildGioLoc` trước khi
  phóng): (1) **BỎ HIỆU ỨNG SÁNG** — xoá `HaoQuang` + `StormLight`, tia không loé chạm đất (`TornadoBolt(..., coLoe: false)`); chỉ còn
  2 tia sét, đường tia giữ (miệng → đất); (2) **bụi lên đỉnh ×2 hạt, quay quanh thân ×2** (`HeSoBuiLenGioLoc`, `HeSoQuayBuiGioLoc` —
  tổng 480 hạt/giây, ~1 460 hạt sống mỗi cơn); (3) **TỐI 30%** (`HeSoToiGioLoc` 0,7): vỏ qua **MaterialPropertyBlock** (vật liệu
  `M_P_LX_*` là asset dùng chung với Lốc xoáy — không sửa), bụi qua màu hạt. Menu 71 C2/C3/C4/C5 so với Lốc xoáy thật bằng hệ số
  viết tay: đã bỏ 2/2, 8/8 thành phần khớp, bụi lên đỉnh ×1,97, tốc góc ×2,00, ảnh sáng ×0,73 IoU 0,94, 0 đèn, 0 loé.
  ⚠️ **Cùng tối: + LỚP KHÓI THÂN TRÊN** `BuiThanTren` (người dùng khoanh thân trên trên ảnh: "chưa phủ bụi khói như thân dưới"; chọn
  "thêm lớp", chỉ Gió lốc): sinh ở GIỮA thân (7,5 m đơn vị Lốc xoáy) ôm thân bay lên đỉnh, hạt 3,0–5,8, quay ×2, tối 30%,
  **240 hạt/giây** (`TocBuiThanTrenGioLoc`, chọn bằng **menu 95**: bụi tách lớp vẽ trên nền đen, đo độ dày trong viền thân — không có lớp
  này thân trên ×0,73 thân dưới; 80/160/240/320 → ×0,84/0,88/0,91/0,92, bão hoà). ⚠️ Đo "chênh sáng trên ảnh đầy đủ" (bản 1 của menu
  95) KHÔNG dùng được: bụi xám đè lên vỏ xám gần như không đổi độ sáng. Gió lốc tổng 720 hạt/giây, ~2 100 hạt sống mỗi cơn.
  ⚠️ **02/10/2026: KHÓI LIỀN MẠCH, KHÔNG 2 TẦNG** (người dùng khoanh dải mỏng giữa thân): Gió lốc chỉ sống 4,5 s mà các lớp bụi mọc dần từ
  chỗ sinh (~2,8 m/s đơn vị Lốc xoáy) và lớp thân trên sinh ở GIỮA thân → 1,5 s đầu giữa thân TRỐNG (menu **95b** đo theo thời gian sau
  khi tung: lõm kẹp giữa ×0,00 / 0,10 / 0,87 lúc 0,5 / 1 / 1,5 s). ⚠️ Đo lốc ĐỨNG YÊN sau 3,5 s thì mọi cấu hình đều phẳng — không thấy
  lỗi. Người dùng chọn **khói có sẵn ngay lúc tung**: mọi lớp `Bui*` `loop + prewarm` trong `BuildGioLoc` → ×0,98–1,00 suốt đời lốc;
  lớp thân trên sinh ở **5 m** (`CaoBatDauThanTrenGioLoc`, hàm `DungBuiThanTren`), hạt/giây giữ mật độ mỗi mét (240 × 10/7,5 = 320).
  Prewarm mô phỏng trước một chu kỳ lúc tung (3–5 cơn) — chưa đo khựng trên điện thoại.
  ⚠️⚠️ **03/10/2026: BỎ HẾT BỤI CUỐN LÊN THÂN, CHỈ CÒN BỤI SÁT CHÂN + VỆT SAU LƯNG** (người dùng: "bỏ tất cả bụi khói cuốn từ chân lốc
  đến tận đỉnh; chỉ bụi cuộn sát chân lốc và khi lốc di chuyển để lại phía sau; thêm bụi khói đen xám để lại phía sau"; chọn bỏ 3 lớp,
  hai lớp vệt, ~2 s). `KhacLocXoay` xoá `BuiThanDuoi` + `BuiCuonLen`, lớp thân trên đã gỡ (các dòng "lớp thân trên / lên đỉnh ×2 / quay ×2 /
  menu 95 / 95b" ở trên là **LỊCH SỬ**, menu 95/95b đã xoá — git `282e20f`); giữ `BuiChan` (prewarm). Vệt `VfxFactory.VetSauGioLoc`: hai
  hệ **THẾ GIỚI**, con của gốc hình (không ăn tỉ lệ 0,318), sinh theo **QUÃNG ĐƯỜNG** (`rateOverDistance` — đứng yên không vệt):
  `VetBuiXam` (ảnh/màu bụi chân, 6 hạt/m) + `VetKhoiDen` (flipbook KhoiCuon xám đen, 4 hạt/m), sống ~2 s (`GiayVetGioLoc`). Menu 71
  (0 lỗi): đã bỏ 4/4, bụi trên 40% thân 6 hạt (đối chứng Lốc xoáy thật 368), đứng yên 0 hạt vệt, bay 9,5 m/s: vệt sau lưng tới 20,6 m
  (trung vị 9,4), lệch ngang ≤ 1,8 m, tuổi ≤ 2,04 s.
  ⚠️⚠️ **03/10/2026 (lần hai): THÂN = 12 DẢI GIÓ XOẮN, BỎ VỎ PHỄU + VÀNH** (người dùng: "thân lốc thấy quá rõ là hình tròn, vẽ lại
  thật tự nhiên là các luồng gió cuộn lên"; chọn Blender MCP, chỉ Gió lốc). Blender MCP `CongCu/Blender/gio_loc_xoan.blend` (scene
  `GioLocXoan` lưới, `GioXoanAnh` ảnh) → `Resources/KyNang/GioLoc/GioXoan.fbx` (3 nhóm `DaiTrong/DaiGiua/DaiNgoai`, 12 dải xoắn ốc HỞ
  0,6–1,4 vòng, đầu–cuối ở độ cao khác nhau, rộng 0,42–0,72 m thon hai đầu, mặt cắt NGHIÊNG theo chiều xoắn — dựng thẳng thì ở mép
  thân lộ vạch đứng; bán kính theo vỏ chính Lốc xoáy ×0,318 lượn ±7%; màu đỉnh mờ hai đầu) + `GioXoan.png` (sợi gió kéo dọc, mép rách,
  liền mạch theo v). `VfxFactory.ThanGioXoan` (con của gốc hình, mét thật): `KhacLocXoay` xoá `Vo0–3` + `Vanh`; mỗi nhóm quay CHẬM
  45/35/28°/s cùng chiều (quay nhanh thì dải xoắn trông như trôi xuống) + ảnh trượt dọc dải v âm 1,25/1,05/0,9 (gió chạy lên); màu = màu
  vỏ Lốc xoáy ×0,8 ×0,7. Menu 71 đo lưới NGOÀI Play: 12 dải, phủ vòng lớn nhất **33–39%** mỗi lát 0,25 m (đối chứng vỏ phễu 100%),
  12/12 xoắn cùng chiều quay, v tăng theo cao, mờ hai đầu. ⚠️ FBX đổ bóng PHẲNG → Unity tách đỉnh theo từng mặt: tách dải phải gộp
  đỉnh trùng vị trí (không thì ra 967 mảnh).
  ⚠️ **04/10/2026: 12 → 18 DẢI, LẤP KHOẢNG TRỐNG** (người dùng khoanh khoảng trống giữa thân / chân / trên): 8 dải bắt đầu từ chân (so le
  0–0,24 m, đầu thon), dải rộng 0,55–0,90 m trải đều theo góc vàng, cùng độ dốc ~1,7 vòng / thân; đầu dải mờ theo **độ dài tuyệt đối
  0,35 m** (trước 22% chiều dài → chân thưa); độ đậm mỗi dải ≤ 0,62 (màu đỉnh) để chồng nhau vẫn thấy từng luồng (bản thử 20 dải đậm phủ
  97–100% nhưng thành KHỐI BÔNG, mất luồng gió — bỏ). Menu 71 **C6** đo phủ (trực giao, trong viền thân, chỉ vẽ dải trên nền đen): chân
  **85%** / giữa **100%** / trên **94%** (Blender cùng cách đo: bản 12 dải 61–63 / 85–94 / 77%); đối chứng chỉ nhóm ngoài 70 / 98 / 81%.
  Mỗi dải vẫn hở: phủ vòng lớn nhất 42% (vỏ phễu 100%).
  ⚠️⚠️ **04/10/2026 (lần hai): THÂN PHẢI CUỘN LÊN** (người dùng: "Lốc chỉ là 1 hình dựng lên và tiến về phía trước, không hề có hiệu
  ứng cuộn từ dưới lên theo 1 hướng"). **Menu 97** (PreviewScene ngoài Play, giả lập đúng Spin + ScrollUV, camera trực giao, tìm dịch chuyển
  (dx, dy) khớp nhất giữa hai khung cách 0,1 s): bản cũ **−0,04 thân/giây = TRÔI XUỐNG** (0/12 cặp khung lên) — dải xoắn CÙNG chiều quay
  nên quay là giao điểm dải với một góc cố định tụt xuống; ảnh gió sợi kéo dọc nên trượt dọc dải gần như không thấy; đối chứng Lốc xoáy
  thật +0,137 (12/12). Người dùng chọn "dải gió + vệt gió hạt" và "chụp 3 mức để chọn": (1) Blender MCP dựng lại 18 dải **GƯƠNG x — xoắn
  NGƯỢC chiều quay** (menu 71 đổi phép kiểm: góc GIẢM theo độ cao, −1,75…−2,33 rad/m) → quay là vệt dải **LEO LÊN**; (2) ảnh `GioXoan.png`
  vẽ lại có **từng CỤM gió** (nhiễu biến đổi mạnh theo v, nền cụm 0,45 — nền 0,10 thủng 35% giữa dải, người dùng vừa xin lấp khoảng trống;
  nay 11%, bản cũ 8,6%) — dịch dọc 0,047 v đổi ảnh ×1,54; (3) quay ×`HeSoQuayCuonGioLoc` 4,5 × `MucCuonGioLoc` (203/158/126°/s ở mức 1);
  (4) lớp **`VetGioXoan`** (`VfxFactory.VetGioXoanLen`): hạt KHÔNG vẽ, chỉ vẽ ĐUÔI (Trails cục bộ, ảnh Blender MCP `VetGio.png` scene
  `VetGioAnh` — lưu LẬT NGANG vì đầu vệt Trails ở u = 0), quỹ đạo dùng lại `BuiCuonLenTheoThan` (×1,08 vỏ chính), 14 vệt/giây × mức,
  lên 1 m/s × mức, quay 2,6 rad/s × mức, sáng ×1,4 dải (cùng tối thì ban ngày chìm vào đất). Mức 1 / 1,5 / 2: dải **+0,127 / 0,170 /
  0,243 thân/giây** (12/12 lên; đứng im 0), vệt gió đo vị trí thật từng hạt: lên 0,99 / 1,50 / 1,99 m/s, **100% cùng chiều quay dải**.
  Ảnh động **menu 97b** (Play Act2 ban ngày, Gió lốc thật bay, máy quay game + máy quay cận) → `PlayTestShots/gioloc_cuon_m*_*.gif`.
  `MucCuonGioLoc` là biến tĩnh để menu 97/97b đặt — **người dùng chọn MỨC 1,5** (dải quay 304 / 236 / 189°/s, menu 71 0 lỗi); mô tả
  Gió lốc trong Sách phép viết lại (luồng gió xoắn một chiều, vệt gió cuộn lên đỉnh, bụi sát chân + vệt bụi / khói đen xám phía sau).
  ⚠️ **04/10/2026 (lần ba): HẠT BỤI ĐEN CUỐN LÊN — CẢ LỐC XOÁY LẪN GIÓ LỐC** (người dùng: "hạt bụi màu đen bị cuốn từ dưới lên bên trong
  và cả bên ngoài, từ đáy lên tận đỉnh"; chọn cả hai loại hạt · vừa phải · Gió lốc như Lốc xoáy thu theo cỡ). `VfxFactory.HatDenCuonLen`
  (gọi trong `DamBaoBuiCuonLenLocXoay` → `Tornado.Start` và `BuildGioLoc`, rồi Gió lốc phóng ×0,318): 4 lớp quỹ đạo `BuiCuonLenTheoThan` —
  `HatDenTrong/Ngoai` đất vụn, sỏi đen (ảnh Blender MCP `gio_loc_xoan.blend` scene `HatDenAnh` → `Resources/KyNang/LocXoay/HatDen.png`
  2×2, lộn nhào ±4 rad/s, 40 hạt/giây mỗi lớp) + `DenCuonTrong/Ngoai` cụm bụi đen mềm (flipbook `GioLoc/BuiDenCuon` 6×6 — **giữ lại khi xoá
  hình Gió lốc cũ**, 20/giây mỗi lớp); trong = 0,55 vỏ chính, ngoài = 1,18. Tên KHÔNG bắt đầu bằng "Bui" (phép thử đếm bụi xám theo tiền tố
  ấy); Gió lốc prewarm cả `HatDen*` / `DenCuon*`. Menu 98 (0 lỗi): mỗi lớp có hạt ở cả 5 phần cao, trong 100% r < 0,8 vỏ / ngoài 100% r > 1,0,
  ≥ 95% bay lên + quay cùng chiều thân, độ sáng màu 0,11–0,20 (đối chứng bụi xám 0,76). +~376 hạt sống mỗi cơn.
  ⚠️ **04/10/2026 (lần bốn — nay LỊCH SỬ, xem "09/10/2026 (lần ba)" ở trên: 1,2 s, nằm ngang 90°): HẤT TUNG 3 m · 0,7 s · NGÃ NGỬA TRÊN KHÔNG** (người dùng: "bị hất tung cao hơn nữa" + "tư thế ngã ngửa ra sau
  trên không trung thay vì tư thế đứng"): `BiHatTung.CaoBay` 1,5 → **3**, `GiayMacDinh` 0,5 → **0,7**; hình lật ngửa tới `GocNgua` **75°**
  (cùng chiều lật của `BiDanhNga`: quanh trục X của GỐC, nhân bên trái `rotGoc`) quanh điểm hông `TamXoayNgua` 0,9 m — ngửa trong 30% đầu,
  giữ, 28% cuối dựng lại để chạm đất bằng chân; đang bị đánh ngã thì `BiDanhNga` giữ góc. `XacNam` lấy cả `RotGoc` của hất tung. Sách phép
  ghi "lên cao 3 mét, ngã ngửa ra sau giữa không trung trong 0,7 giây". Menu 71 G đo ĐIỂM HÔNG từ tư thế thật: cao 3,00 m, bay 0,71 s, ngã
  lớn nhất 75°, 3605/3605 mẫu đầu ngã về SAU, 164/164 lần đứng thẳng lại; menu 85 0 lỗi.
  ⚠️⚠️ **04/10/2026 (lần năm): CẤP 1–4 HAI LỐC ±15°, CẤP 5 BA LỐC −30 / 0 / +30** (người dùng: "cấp đầu chỉ 2 lốc, cách xa nhau ra 1 chút;
  cấp 5 ra 3 lốc"; chọn lệch 30°): `GioLoc.SoLocTheoCap` 2 / 3 (trước 3 / 5), `GocQuat` 20 → **30**. Mọi dòng "3 lốc / 5 lốc / 20°" ở trên là
  LỊCH SỬ. **Hoá lốc xoáy**: 2 lốc thì không có cơn giữa → hoá cơn **gần chỗ ngắm** của lần bấm Hoá lốc xoáy (`HoaLocXoay.LanTungGanNhat(...,
  choNgam)`, người dùng chọn; bản sao mạng cùng `castAim`); cấp 5 vẫn hoá cơn giữa. Menu 71 / 75 (thêm D2: ngắm lệch trái → hoá cơn trái) /
  84: 0 lỗi.
  ⚠️ **HÌNH QUẠT: cấp 1–4 tung 3 lốc, cấp 5 tung 5 lốc**, cách nhau `GioLoc.GocQuat` **20°** quanh hướng ngắm (29/09/2026 người dùng "xa nhau hơn 1 chút", trước 15°; cấp 5 toả ±40°;
26/09/2026: trước đó 1 lốc, cấp 5 hai lốc song song cách 4 m). ⚠️ **Mây giông nhẹ TRONG LÒNG nửa trên thân** (29/09/2026, lúc đầu trên đỉnh rồi người dùng
đổi "nằm trong cơn lốc": hệ `MayTrongLoc`, `VfxFactory.MayDinhGioLoc`; ảnh `MayGiong` 2×2 của Mây giông, màu thân Gió lốc, mỏng, đám
1,4–2,2 m sinh trong khối hộp quanh trục 2,5–4,3 m, ±0,65 m, cục bộ; **loé khi sét trong lốc đánh** — `LoeSangMay` trên gốc hình,
`GioLocSetTrongLoc` gọi `Chop(0,7)`; menu 71 mục C2 đo từng hạt so với bán kính vỏ trong ở đúng độ cao). ⚠️ **01/10/2026 mây NHIỀU + DÀY
hơn ×1,5 và mở xuống 1,8–4,3 m** (người dùng chọn sau menu 94 chụp cũ / 1,3 / 1,5 / 1,8): vùng sinh là khối NÓN bán kính 62% vỏ trong
(`TiLeMayTrongVo`; hộp ±0,65 ở 1,8 m lòi ra 81%), số đám theo mật độ mỗi mét × `HeSoDayMayGioLoc` (≈ 26 đám, trần 42, độ đục 0,56–0,81). Năng lượng **không** nhân theo số lốc (20; cấp 5 vẫn 25). **Hình dựng bằng
  Blender MCP** (17/09/2026, `CongCu/Blender/gio_loc.blend` → `Resources/KyNang/GioLoc/`: `LocNho.fbx` 3 vỏ + dải gió cao 5 m, ảnh gió liền mạch
  `GioDai`/`GioSoi`, flipbook `BuiDenCuon`; chân ×1,68 so với gốc, nhỏ dần về 0 ở 2,3 m; toàn bộ bề ngang ×1,1 lúc chạy `HeSoBanKinhGioLoc`), xám trắng như Lốc xoáy, **xoáy MỘT chiều đi lên** (mọi lớp quay âm quanh +Y + UV trượt âm — chiều
  chọn bằng số đo trên lưới, `VfxFactory.ChieuQuayGioLoc`), khói bụi đen cuộn quanh thân + vệt phía sau, **2 tia sét luôn đánh từ đỉnh xuống trong lòng lốc** mỗi 0,45 s như Lốc xoáy, bề dày ×5/15,37, **bám theo lốc** (`LightningArc.BamTheo` — tia ghim toạ độ thế giới bị bỏ lại 1,2–2,7 m), hai tia đối diện cách ~1,2 m ở đỉnh, thu vào theo vỏ trong cùng ở chân (`VfxFactory.GioLocSetTrongLoc`, chỉ hình; tia khi trúng đối thủ đã bỏ). Bay **9,5 m/s**
  (người dùng chốt 17/09/2026) **xuyên mọi vật cản / người** — bám mặt đất bằng `GioLoc.MatDatY` (CHỈ lớp Ground; `GroundY` gồm cả Default làm lốc trèo lên mái nhà), tan sau 4,5 s; số lốc theo `GioLoc.SoLocTheoCap` (cấp NGƯỜI TUNG — đi qua gói tin nên máy kia ra đúng 3 / 5 lốc); mỗi lốc trúng mỗi mục tiêu **một lần** 75, vùng 2,42 m, **80% hất tung** (25/09/2026, trước 55%) 0,5 s cao 1,5 m (mỗi lốc gieo riêng, khiên chặn); 20 năng lượng · hồi chiêu 0,4 · niệm 0,38.
  ⚠️ 18/09/2026 người dùng đổi: **mỗi lần trúng kẻ địch hồi 10 mana** (`ManaHoiMoiLanTrung`, cố định cả 5 cấp — chỉ cộng trên máy
  của chính người tung, `pc.tuDocInput`, không thì bản sao mạng cũng cộng) và **cấp 5 chỉ tốn 25 năng lượng** (`NangLuongCan`,
  con số cố định thay cho 20 × 1,1⁴ × 2 = 58,6 — cấp 4 tốn 26,6 nên cấp 5 lại rẻ hơn, đúng ý người dùng).
  Bị hất = khoá như ngã + **ngắt chiêu** (`PlayerController.NgatChieu`, `EnemyAI.NgatDon`). Bit mạng **`CoHatTung` = bit thứ 5**, mặt nạ gói
  người chơi và gói quái đã nới **0x1F** (còn trống bit 7 gói người chơi, bit 6–7 gói quái). Lướt qua lò lửa thì `DapTatRoiChayLai(30)`.
  Icon **vẽ lại lần hai 29/09/2026** `python CongCu/Icon/sinh_icon_ve_lai_5.py` (ba lốc THỂ TÍCH toả quạt, đỉnh tan vào mây giông,
  nền đĩa xanh xám mây giông; `sinh_gio_loc.py` và phần GioLoc/TocBien của `sinh_icon_ve_lai_3.py` là bản cũ, tự chặn trừ khi `--cu`).
  Menu 71 kiểm, 71b chụp ảnh.
  ⚠️ **28/09/2026: THÂN DƯỚI TO GẤP ĐÔI** (người dùng: "nhìn như cây kem ốc quế") — Blender MCP, `CongCu/Blender/gio_loc_than_rong.blend`:
  vỏ trong r = 0,826 + 1,019·(z/5)^1,6 (chân 0,413 → 0,827, miệng giữ 1,845), cả 4 lớp nhân cùng k(z); vùng trúng GIỮ 2,42 m. Xuất lại
  `LocNho.fbx`: `axis_forward='-Z', axis_up='Y', bake_space_transform=True, apply_unit_scale=True, mesh_smooth_type='FACE',
  colors_type='LINEAR'` (đã kiểm: xuất lại bản chưa sửa khớp FBX cũ 0,000000 m). **Bụi ĐEN → BỤI XÁM CỦA LỐC XOÁY** (cùng ảnh
  `BuiXam`, cùng màu, cùng kiểu cuộn — hàm chung `VfxFactory.BuiXamChanLoc`); `BuiCuon` để CỤC BỘ (lốc bay 9,5 m/s), vệt `KhoiBui`
  cũng bụi xám. Các dòng "khói bụi đen" phía trên là LỊCH SỬ. Thân trong hơn (cùng tối): độ đục `VfxFactory.DoDucVoGioLoc`
  0,42 / 0,20 / 0,16 / 0,70 (trước 0,72 / 0,34 / 0,26 / 0,90) — menu 71c đo bụi lộ qua vỏ ×2,06.
  ⚠️ **Cùng tối: GIÓ LỐC ĐỔI SANG MÀU MÂY GIÔNG** (thân lẫn bụi — `MauMayGioLoc`, `MauBuiGioLocToi/Sang`, lấy từ
  `VfxFactory.MauMayGiongSang/Xam`); **Lốc xoáy GIỮ xám trắng** (người dùng thử đổi Lốc xoáy trước rồi bảo nhầm, quay về `2a3450d`).
  Ban đêm bản ấy chìm vào nền → người dùng chọn: **độ đục về lại 0,72 / 0,34 / 0,26 / 0,90** (dòng "thân trong hơn" trên là LỊCH SỬ)
  + màu mây ×2,5 (chọn sau khi menu 71c quét ×1,5/2/2,5/3). **29/09/2026: đen hơn một chút → ×2,25 cho CẢ thân lẫn bụi**
  (`VfxFactory.HeSoSangMayGioLoc`; cả cơn lốc đêm ×0,91, ngày ×0,88 so với ×2,5; đêm vẫn nổi hơn bản chìm) và **bụi cuộn dày ×2**
  (`HeSoBuiDayGioLoc`: 80 hạt/giây, trần 240 — chỉ Gió lốc, bụi chân Lốc xoáy giữ 40 / 120).
- **MÂY GIÔNG** (`CapDo.KyMayGiong` = 21, `Skills/MayGiong.cs`, hình `Vfx/VfxMayGiong.cs`, hiệu ứng `Combat/ChayDenToanThan.cs`,
  25/09/2026, nhóm **PHONG**, người dùng gửi ảnh mẫu): vùng mây giông ngay chỗ ngắm, tầm **= Sấm sét** (`boltRange` 12), bán kính
  **6 m**; **20 tia trong 5 giây** (0,35 s mây kết + 0,25 s/tia, đếm theo đồng hồ), **65% nhắm kẻ địch** trong vùng như Sấm sét;
  mỗi tia **125** (cấp 1) cho mọi kẻ trong **2 m**, **45% hất ngã 0,85 s** (`ThienThach.GieoDanhNga` → `BiDanhNga`, khiên chặn,
  +0,15 s/cấp), **cháy đen toàn thân 3 s — chỉ hình** (lớp than Blender phủ thêm lên MỌI Mesh + SkinnedMeshRenderer, khói đen,
  gỡ chỉ đúng lớp của mình nên không mất vỏ băng; Tàng hình chặn). **50 năng lượng · niệm 0,5 · hồi chiêu 5,5 s** (26/09/2026, trước 7 — ⚠️ ba số này là THUỘC TÍNH đọc
  hằng `MayGiong.*`, không phải trường: đổi hằng mà Play vẫn ra 7 vì prefab trong bộ nhớ giữ mặc định cũ); ⚠️ **mở khoá cần LỐC XOÁY
  cấp 5** (người dùng 26/09/2026; lúc ra mắt không điều kiện). Tia hệ **PHONG** (`GhiKeDanh(boQua, HeSat.Phong)`). Hình **Blender MCP** (`CongCu/Blender/may_giong.blend`
  → `Resources/KyNang/MayGiong/`: `MayGiong.png` 4 đám mây bồng 2×2, `ChopSet.png` loé chạm đất, `ChayDen.png` than đen liền mạch)
  + icon `Resources/Icons/MayGiong.png` (Read/Write bật). Mây ở ~~7 m~~ → ⚠️ **8 m** (04/10/2026 người dùng "bay cao hơn 1 chút", chọn 8 m) + **mưa 200 vệt/giây** (trước 160, `VfxFactory.VetMuaMoiGiay`).
  ⚠️ Menu 100 đo ở máy quay game thật: ở CẢ HAI góc mặc định ("3D tự do" nghiêng 22°, "2.5D" nghiêng 48°) tâm + đỉnh mọi đám mây — kể cả bản
  7 m cũ — đều nằm TRÊN mép màn hình, chỉ phần ĐÁY thò vào khung; 8 m thò ít hơn (3D tự do, ngắm 6 m: 21/42 đám, đáy thấp nhất ở 0,80 chiều cao
  màn hình — 7 m: 37/42, 0,69; 2.5D: 2/42 so với 3/42). Đã báo — người dùng **chốt giữ 8 m**.
  ⚠️⚠️ **28/09/2026: TIA MÂY GIÔNG NAY Y NHƯ TIA SẤM SÉT** (người dùng, chỉ đổi hình): `VfxFactory.TiaMayGiong` vẽ như
  `LightningStrike.Strike` — `LightningArc` kiểu MẶC ĐỊNH (không ảnh Blender, màu mặc định), 20 đoạn, 2–3 nhánh, 0,30 s, KHÔNG bám
  mục tiêu, + `LightningImpact` chỗ chạm đất; tia ngang trong mây cũng kiểu mặc định. Các đoạn "y Giựt sét / quầng xanh sẫm
  `MauQuangMayGiong` ×1,2" bên dưới là LỊCH SỬ (hằng đã xoá). Menu 83 mục I nay tung SẤM SÉT THẬT để so.
  ⚠️ **Chiều 25/09/2026 người dùng gửi ảnh thứ hai**: **CỘT KHÓI rủ từ đáy mây xuống TẬN MẶT ĐẤT** (chọn "một cột khói giữa
  vùng") — ảnh Blender `CotMay.png` (4 cột 2×2, uốn chữ S, mép lồi lõm, loe lên mây) vẽ bằng hạt **VerticalBillboard** 3 lớp,
  rộng `VfxFactory.BeNgangCotMay` 5 m, cao −0,35 → cao mây + 1,2 m, kèm khói cuộn dọc cột (`KhoiCot`); và **tia ĐÚNG Y GIỰT SÉT**
  (chọn "đúng y"): `GiatSet.KieuTia` bề ngang ×1, quầng `MauQuangNguoiChoi`, sống `GiayTiaHien` 0,6 s, 2 nhánh, bỏ loé hình sao
  chạm đất. Menu 83 mục I tung Giựt sét THẬT rồi so từng thông số tia (không so với hằng số); mục H đọc thẳng PNG để biết khói
  thấy được từ đâu. **Lần ba (ảnh thứ ba)**: quầng tia **XANH SẪM HƠN** `VfxFactory.MauQuangMayGiong` (0,05 0,16 1), viền + hào quang
  dày ×1,2 (`LightningArc.heSoVien` — trường MỚI mỗi tia, mặc định `HeSoVienXanh` 1,10; Giựt sét / Quả cầu điện không đổi), lõi
  vẫn trắng; và **quầng mây mỏng sát đất** quanh chân cột ~3 m (ảnh Blender `SuongDat.png` 2×2, 5 đám nằm phẳng
  HorizontalBillboard ở 0,3 m + 6 cụm mây thấp). **Lần bốn (tối 25/09)**: quầng sát đất **rộng ×1,15** (lan ~3,45 m), **đặc + cao
  ×1,10**; **toàn bộ mây XÁM ĐEN** (`VfxFactory.HeSoToiMay` 0,26 × màu cũ, đáy mây tối hơn đỉnh) — **sét rọi sáng TỪNG MẢNG**:
  mỗi tia bật `VfxFactory.MangSangTrongMay` (2 đám mây Blender cộng sáng ở chỗ tia phát ra) và cả đám loé nhẹ ×1,8 trong 0,15 s
  (`Vfx/LoeSangMay.cs`, MaterialPropertyBlock, không tạo vật liệu); loé trong mây đổi từ ảnh sao sang ảnh mây. ⚠️ Loé cả đám mạnh
  (×2,8, 0,22 s, cả tia ngang) giữ mây sáng gần hết thời gian — bỏ. **Lần năm (khuya 25/09)**: cột khói **rộng ×1,2**; **MƯA** rơi khắp
  vùng 6 m (vệt `GiotMua` Blender, 160 vệt/s, 16 m/s, **va chạm lớp Ground** rồi bật `VongNuoc` đúng chỗ chạm — sub-emitter);
  ⚠️ **BỊ ƯỚT** (`Combat/BiUot.cs`): mọi đối thủ đứng trong vùng (quét 0,2 s, trừ người tung, Tàng hình chặn) ướt, ra khỏi vùng /
  hết mưa còn **5 s**; kẻ ướt ăn **+50%** (`BiUot.HeSo`) từ **Giựt sét** (chỉ của người chơi, `GiatSet.tangKhiUot`), **Sấm sét**
  (`AreaShock`), **Quả cầu điện** và **tia Mây giông** (125 → 187,5); hình: nước nhỏ giọt `NhoNuoc` + lớp `UotBong` + chữ "BỊ ƯỚT";
  không cần bit mạng (mây phát lại trên mọi máy, sát thương do máy nạn nhân / chủ phòng tính). Tia chạm đất **cháy xém + khói y
  Sấm sét** (`SetChayDen` + `NamChuongNgai`). Ảnh Blender ở `CongCu/Blender/may_giong_mua.blend` (file RIÊNG — Blender mở cảnh trống,
  cấm open_mainfile). ⚠️ Hai bẫy đã vấp: (1) **hạt Vertical/HorizontalBillboard vẽ 0,707×** VÀ **`maxParticleSize` 0,5 ép hạt to**
  khi máy quay gần — cột khói trước đây thật ra lơ lửng (vẽ 2,04 → 5,81 m) dù phép thử đọc cỡ đặt báo "chạm đất"; nay bù chiều cao
  /0,7071, `maxParticleSize` 10, đo bằng `BakeMesh` trong Play; (2) lớp phủ **chỉ xét vật liệu GỐC** (ô đầu) khi bỏ renderer trong
  suốt — xét cả mảng thì kẻ đang ướt (lớp bóng trong suốt) KHÔNG BAO GIỜ cháy đen. **Lần sáu (26/09)**: **BỎ cột khói + quầng mây
  sát đất**; mây **to ×1,15 chỉ hình** (`HeSoToMay`, vùng mưa/ướt/sét giữ 6 m); **mưa dập lò lửa** (`LoLuaDa.DapTatTrongVung`, 30 s
  cháy lại); ⚠️ **hết 5 s mây KHÔNG tan mà TỰ BAY 4 s** (`ThoiGianBay`, 1,5 m/s ≈ 6 m, bám đất bằng `GioLoc.MatDatY`), vừa bay vừa
  mưa + sét 4 tia/giây (thêm 16, **tổng 36**, `SoTiaTong`); hướng "ngẫu nhiên" **giống nhau mọi máy**: `MayGiong.HuongBay` băm từ
  toạ độ ngắm **đã nén** như gói tin (`GoiTin.NenToaDo`, nay public) — không tốn byte nào. Lớp mây mô phỏng **Local** (bay theo gốc)
  + `AlwaysSimulate`. `MayGiong.OnDestroy` xoá luôn phần hình nếu bị xoá giữa chừng. Menu 83 kiểm; menu 61 có thêm Mây giông.
- **BỊ ĐỘNG TỐC ĐỘ DI CHUYỂN** (`CapDo.KyTocDo` = 20, 25/09/2026): mở khoá **+10% TỐC ĐỘ GỐC** (`PlayerController.TocGoc` = moveSpeed
  lúc Awake, trước mọi lần tăng theo cấp nhân vật), mỗi cấp +2,5% (`CapDo.TocThemTheoCap`, cấp 5 = +20%), CỘNG vào tốc độ đang có:
  `(moveSpeed + TocGoc × TocThemBiDong) × lội nước × băng × tàng hình`. Chỉ nhân vật của máy này — điều kiện là `!mauDoMayKhacQuyet`,
  ⚠️ **không** dùng `tuDocInput` (phép thử bơm input tắt cờ ấy, đo sẽ ra một nhân vật không bao giờ được cộng). Icon bằng script
  (vẽ lại 29/09/2026: **ủng sắt gothic có cánh ma xanh**, `CongCu/Icon/sinh_icon_ve_lai_4.py`; meta chép từ KhangPhong để có Read/Write). Menu 80 mục A
  đo thật (bơm input cùng đoạn đường): cấp 1 ×1,100, cấp 5 ×1,195. Phép thử đếm cứng "nhóm Bị động có 4" / "20 icon" đã sửa (menu 77).
- **NHÓM BỊ ĐỘNG — bốn kỹ năng Kháng** (`Combat/KhangHe.cs`, 19/09/2026): mở khoá giảm **25%** sát thương của hệ đó
  **TỪ NGƯỜI CHƠI KHÁC**, mỗi cấp thêm **5%** (cấp 5 = 45%). ⚠️ **Không chặn đòn của quái** (người dùng chỉ xin chặn đòn
  người chơi) — khác hẳn `Damageable.fireResist/iceResist/lightningResist` có sẵn, thứ áp cho mọi nguồn. **Không tung được,
  không kéo vào ô** (`CapDo.LaKyBiDong`, `PlayerController.CastAt` chặn, `CuaSoSachPhep` không cho kéo).
  ⚠️ Biết đòn đến từ người chơi khác bằng **thẻ dùng một lần** trong `Damageable.GhiKeDanh` (chỉ tin khi ghi trong CÙNG
  khung hình, `TakeDamage` xoá ngay sau khi đọc) — `keDanhCuoi` KHÔNG dùng được vì đòn của quái không ghi gì, nó giữ tên
  người chơi của đòn trước và làm đòn quái bị chặn oan. Hệ suy từ `DamageType` (Fire/Ice/Lightning/Physical → LỬA/BĂNG/SÉT/PHONG);
  **ngoại lệ duy nhất**: tia sét trong lòng Lốc xoáy / Gió lốc gọi `GhiKeDanh(boQua, HeSat.Phong)` vì hai kỹ năng ấy thuộc
  nhóm PHONG (người dùng chốt). Kháng đọc `CapDo` nên chỉ áp cho nhân vật của máy này, không áp cho bản sao. Icon vẽ bằng
  script (người dùng chọn không Blender) — **vẽ lại 29/09/2026** bằng `python CongCu/Icon/sinh_icon_ve_lai_4.py`: khiên sắt gothic
  chặn đòn nguyên tố bị tách dạt qua hai mép (`sinh_khang.py` là bản cũ, tự chặn trừ khi có `--cu`) — ảnh icon phải bật **Read/Write** thì
  `IconKyNang.Ve` mới ghép được ruột. Menu 77 kiểm.
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
  Đổi `transform.position` phải **tắt `CharacterController` rồi bật lại**. Icon vẽ lại lần hai 29/09/2026 (`sinh_icon_ve_lai_5.py`:
  không hình người — khói tím + vòng co ở chỗ cũ, vệt phép cong, vòng phép nở + cột sáng ở chỗ mới). Menu 76 kiểm.
- ⚠️ **LỐC XOÁY DỰNG LẠI BẰNG BLENDER MCP THEO ẢNH MẪU** (người dùng 25/09/2026: "giống như trên hình 100%, lốc cuốn lên
  chỉ quay xoay theo trục 1 chiều"; chọn cao 15,4 m dáng theo ảnh · bỏ mây giông + khói đen · tia kiểu Giựt sét nhiều nhánh ·
  vùng hút giữ 5,184). `CongCu/Blender/loc_xoay.blend` → `Resources/KyNang/LocXoay/`: `LocXoay.fbx` (4 vỏ phễu `Vo0–3`
  ×0,70/0,84/1,00/1,13 + vành cuộn `Vanh`; vỏ chính ~~r = 1,3 + 5,5·t^1,9~~ → ⚠️ **29/09/2026 r = 2,6 + 4,2·t^1,6** (người dùng: "thân
  dưới như cây kem ốc quế" — chân ×2, thân to dần đều, miệng 6,8 giữ; Blender MCP `loc_xoay_than_rong.blend`, cả 5 lưới nhân cùng
  k(z); quỹ đạo vật bị cuốn + tia sét theo công thức mới; vùng hút giữ 5,184; vòng bụi chân 2,0 → 4,0 cả code lẫn prefab),
  thân 15 m + vành → 15,72 m). ⚠️ **29/09/2026 BỤI CUỘN LÊN TẬN ĐỈNH** (cả Lốc xoáy lẫn Gió lốc, người dùng): lớp mới `BuiCuonLen`
  (`VfxFactory.BuiCuonLenTheoThan`) sinh trên vòng ở chân, bay lên đều tới đỉnh trong một đời hạt, dạt ra theo ĐÚNG công thức bán kính
  thân (vận tốc toả = dr/dt), CỤC BỘ — Lốc xoáy 80 hạt/giây, gắn LÚC CHẠY trong `Tornado.Start`
  (`DamBaoBuiCuonLenLocXoay` — hình trong game lấy từ prefab); Gió lốc 40 (+ 80 = 120 mỗi cơn). Cùng ngày người dùng khoanh THÂN DƯỚI
  Lốc xoáy trên ảnh ("dày đặc hơn nữa"): thêm lớp `BuiThanDuoi` 80 hạt/giây chỉ tới nửa thân + bụi chân ×2 (40 → 80, trần 240 — đặt
  trong `DamBaoBuiCuonLenLocXoay`, prefab/`BuildLocXoay` vẫn ghi 40 là số gốc) → **Lốc xoáy tổng 240 hạt/giây**; nửa thân dưới ×2,36 hạt
  so với cấu hình cũ đo cùng lượt (menu 82 B5). ⚠️ Vận tốc toả (radial) của Unity tính
  theo hướng **3 CHIỀU** từ tâm → đẩy hạt vọt qua đỉnh; dời tâm (`orbitalOffsetY`) lên theo độ cao hạt để toả nằm ngang. ⚠️ Vòng phun
  `Circle` mặc định **ĐỨNG trong mặt XY**: bụi chân Lốc xoáy (thế giới) từng phun trên vòng đứng, nửa số hạt sinh dưới đất — nay xoay
  −90° cả code lẫn prefab. Menu 82 B3b/B4, menu 71 mục C đo vị trí thật từng hạt.
  ⚠️ **29/09/2026 ĐEN HƠN** (người dùng chọn sau khi menu 93 chụp tối đi 10/20/30%): **Lốc xoáy ×0,8** (`VfxFactory.HeSoToiLocXoay` —
  màu vỏ trong code VÀ 5 vật liệu prefab `Materials/M_P_LX_Vo0–3/Vanh` đã nhân 0,8; màu bụi `MauBuiXamToi/Sang`, bụi chân prefab đặt lại
  lúc chạy; KHÔNG đụng vật liệu bụi dùng chung `BuiXamMat`), **Gió lốc ×0,9** (`HeSoSangMayGioLoc` 2,25 → 2,025). Menu 82 B3c.
  ⚠️ **KHÔNG CÓ DẤU VẾT TRÊN MẶT ĐẤT**: 29/09/2026 đã thêm (dải đất cày + vết cháy xém, `VetLocDat`, shader nhân màu `VetDatNhan`,
  ảnh Blender MCP) rồi cùng ngày người dùng bảo **xoá hết** ("xoá các vết di chuyển của lốc ở cả 2 skill", chọn xoá cả vết cháy) — đã
  gỡ code, shader, ảnh, vật liệu, file Blender (còn trong git `d2d6da0`). Menu 82 B6 kiểm lốc thật chạy 3 s sinh 0 vật thể dấu vết.
  Ảnh gió `GioVo0–3`,
  `GioVanh` (dải xoắn ốc, mỗi vòng u lệch đúng một dải → **liền mạch theo u**, render mặt phẳng UV trực giao, `filter_width`
  0,01 — bộ lọc 1,5 px làm lệch mép 10 lần), `HaoQuangDinh`, `BuiXam` (2×2). Code `Vfx/VfxLocXoayBlender.cs`
  (`BuildLocXoay`, `BanKinhLocXoay`, `TornadoBolt(Transform, scale)`); `BuildTornado` gọi nó, hình cũ còn ở `BuildTornadoCu`
  (chỉ khi thiếu FBX). Mọi thứ nằm dưới MỘT con `LocXoayHinh` → Hoá lốc xoáy phình cả cơn (trước chỉ phình vỏ đầu).
  **Một chiều + cuốn lên**: mọi lớp quay `ChieuQuayGioLoc` (góc atan2 TĂNG = chiều vật bị cuốn); trên lưới đã nhập u tăng thì
  góc TĂNG, dải trong ảnh đi lên thì u tăng → quay thế thì dải TRÔI XUỐNG, nên vật liệu **lật u** (`LatUAnhLocXoay` −1).
  **Dải khói TRÔI LÊN THẬT** (người dùng duyệt 25/09/2026 trong 4 phương án "cuộn từ dưới lên"): ảnh `GioVo0–3` liền mạch
  **cả theo v** (nhiễu 4D trên hình xuyến), phần mờ chân/miệng + xám dưới chân chuyển sang **MÀU ĐỈNH** của FBX (xuất
  `colors_type='LINEAR'`, shader nhân thẳng) nên không trôi theo ảnh; `ScrollUV` v âm `TruotLenLocXoay` 0,20/0,16/0,13/0,10
  (= 3,0/2,4/1,95/1,5 m/s); vành không trượt.
  ~~Tia sét: 2 tia/nhịp `GiatSet.KieuTia` 3–5 nhánh, bám hai đầu vào lốc; tia vào kẻ bị cuốn cũng kiểu Giựt sét.~~
  ⚠️ **01/10/2026 TIA SÉT = KIỂU SẤM SÉT** (người dùng: "giống tia sét trong Sấm sét"; chọn "từ miệng lốc xuống đất", không vết cháy):
  `TornadoBolt` 2 tia/nhịp `LightningArc` MẶC ĐỊNH (như `LightningStrike.Strike`: 20 đoạn, 2–3 nhánh, 0,30 s, bề ngang × scale) từ miệng
  xuống ĐẤT cạnh chân + loé chạm đất `VfxFactory.LoeSetChamDat` (prefab `Vfx_SetChamDat`, KHÔNG vết cháy — `LightningImpact` có 15% vết
  cháy) bán kính 2,1 × scale; loé **`DiTheo`** con lốc (không làm con: Hoá lốc xoáy phóng con đầu, Gió lốc thu nhỏ); Gió lốc: hạt loé
  đặt Hierarchy + tầm đèn × scale (`ThuLoeSet`). Tia vào kẻ bị cuốn (`Tornado.Zap`) cũng kiểu Sấm sét. Menu 82 E1 tung SẤM SÉT THẬT để so
  từng thông số; menu 71 C5 so loé Gió lốc / Lốc xoáy thật (×0,326). **Bụi lên tận đỉnh ×2** (`TocBuiCuonLenLocXoay` 80 → 160, tổng 320 —
  menu 82 B7: thân trên ×1,93, 20% trên cùng ×1,85 so với cấu hình hôm qua cùng lượt).
  `FunnelRadiusAt` = 0,9 × vỏ chính.
  ⚠️⚠️ **04/10/2026: LỐC XOÁY HIỆN NGAY TẠI ĐỐI THỦ, TẦM 18 m = THIÊN THẠCH** (người dùng; trước: sinh trước mặt rồi trượt về phía ngắm,
  tầm ngắm 12): chọn **đối thủ gần chỗ ngắm nhất** trong `Tornado.TamDanh` 18 m quanh người tung (`Tornado.ChonMucTieu`, bỏ người tung +
  đồng đội qua `CheDoTran.BoQua`), không ai thì **hiện tại chỗ ngắm** (kẹp vào 18 m — `TamCuaKyNang(3)`), rồi **BÁM THEO** đối thủ ấy
  (`Tornado.bamTheo`, 3,4 m/s; đối thủ đang bị cuốn hay đã chết thì trôi theo hướng lúc tung `huongGoc`). Lốc xoáy KHÔNG còn là phép "đơn
  thẳng" (`DonThang`) — kéo ngắm chọn điểm. Bản sao mạng phát lại với cùng `castAim` nên chọn cùng đối thủ (như Quả cầu điện). Hoá lốc xoáy
  không đổi (bamTheo null). Menu 99 (0 lỗi, tung THẬT bằng `CastAt`): tầm = Thiên thạch, hiện cách bia 0,09–0,23 m, bia bị cuốn ngay, không
  chọn bia ngoài tầm, không ai thì hiện ở 18,09 m đúng hướng; bám theo: khoảng cách giảm 5,13 m / 1,5 s (đối chứng không bám −2,23), bia chết
  → trôi lệch 4° hướng lúc tung. Lúc tan tắt MỌI hệ hạt + đèn (trước chỉ của `visual`, loc prefab thì null).
  ⚠️ Nướng prefab bằng **menu 13**; `AssetBaker.SaveMeshes` nay **bỏ qua lưới đã là asset** (lưới FBX — CreateAsset trên nó là lỗi).
  ⚠️ Menu 13 cũ sinh ảnh trùng `Tex_*_N.png` và trỏ vật liệu sang; trả về bằng git rồi **ImportAsset ForceUpdate mọi prefab dùng
  vật liệu ấy** — không thì prefab trong bộ nhớ giữ vật liệu đã xoá và khói nổ / quả cầu lửa ra MÀU HỒNG (người dùng đã gặp).
  ⚠️ Hạt cát `Grit` không quay (trục vận tốc lệch kiểu, Unity bỏ qua mô-đun) — **sửa 05/10/2026**: `BuildDebrisSwarm` đặt MỌI trục (x/y/z,
  quỹ đạo x/y/z, toả) cùng kiểu hai hằng số. Grit chỉ còn trong `BuildTornadoCu` (Lốc xoáy dựng bằng code, dự phòng khi thiếu FBX; nay public) —
  Gió lốc / Lốc xoáy trong game không có lớp này. Đo trong cảnh tạm: quay 53,4°/0,1 s, lên 0,70 m/0,1 s, 0 lỗi Unity; đối chứng lệch kiểu 0°, 0,23 m.
- **Hoá lốc xoáy** (`Skills/HoaLocXoay.cs`, 18/09/2026): bấm là cơn **Gió lốc đang bay của LẦN TUNG GẦN NHẤT** (mỗi cơn mang
  `GioLoc.lucTung`; ⚠️ 26/09/2026 **chỉ hoá cơn GIỮA** của hình quạt — `GioLoc.laLocGiua`, các cơn hai bên bay tiếp) **phình to thành Lốc xoáy** — `PhinhToThanhLoc` chỉ đổi tỉ lệ HÌNH
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
  `TiaDien`/`HatDien`), ba lớp quay ngược chiều nhau; icon `Resources/Icons/CauDien.png`. ⚠️ Vỏ điện nằm **NGOÀI mặt cầu**
  (người dùng xác nhận 18/09/2026) — thứ họ chê "răng cưa rất xấu" là mấy **gai thẳng ngắn chĩa ra** của bản đầu, đã bỏ hẳn;
  cũng **không có tia sét nhỏ bắn ra liên tục** (đã thử rồi bỏ).
  ⚠️ **19/09/2026 đổi vỏ**: năm đường gân trắng LIỀN MẠCH → **các đoạn TIA ĐIỆN MỎNG ĐỨT QUÃNG, CHỚP TẮT** (người dùng xin,
  và chốt "chớp tắt liên tục" + "mỗi đường đứt thành nhiều đoạn"). `VoTiaDien.fbx` (Blender MCP `CongCu/Blender/tia_dien_qua_cau.blend`)
  có **bốn khung** `VoTia0..3` — cùng năm đường ấy, đoạn nằm chỗ khác; `Vfx/ChopTiaDien.cs` đổi `sharedMesh` mỗi 0,045–0,105 s
  (một renderer, không bật/tắt vật thể). Ống dày **0,014** (gân cũ 0,019), zigzag bước 0,115 rad. ⚠️ Đừng làm mỏng hơn nữa:
  bản 0,011 render ở Blender rất đẹp nhưng **trong game ở cự ly chơi thật thì vỡ thành lấm tấm**, mất nét tia — phải chụp
  trong game mới biết. Menu 74 mục J kiểm (35 cụm rời, đối chứng vỏ cũ 4 cụm).
  ⚠️ Hai lưới FBX này Read/Write TẮT → trong Play `mesh.vertices` rỗng; phép đo hình lưới chạy Ở EDITOR trước khi vào Play
  (`ThuQuaCauDien.DoHinhLuoiTruocKhiChay` bật Read/Write tạm rồi trả lại).
  ⚠️ Menu 74 **tắt `GameDirector` suốt phép thử**: nó chạy lâu (mục G bắn 400 tia thật) và đợt quái 24 con sinh giữa chừng
  từng làm **GPU timeout → Windows reset driver → Unity tắt hẳn** (19/09/2026, dấu vết ở cuối `Logs/Editor.log`).
  Menu 74 kiểm.
- ⚠️ **`StunnedEffect.Apply` từng kéo mọi cú choáng ngắn hơn 2 giây thành 2 giây** (trường `remaining` khai báo sẵn 2 và `Apply` lấy
  `Max` ngay từ lần đầu) — sửa 18/09/2026: component MỚI nhận đúng số giây. Ảnh hưởng cả Giựt sét (1,5 s). Cùng cái bẫy `AddComponent`
  mang giá trị mặc định của `FrozenEffect`.
- ⚠️ **TÀNG HÌNH 90 GIÂY + VÒNG PHÉP LÚC HIỆN HÌNH** (người dùng 26/09/2026, ảnh mẫu vòng phép xanh): hết tàng hình (hết giờ HOẶC
  tan do đòn đầu — chọn "cả hai") thì `TangHinh.NoVong` nổ **vòng phép 5 m** dưới chân, gây sát thương MỘT lần cho mọi đối thủ:
  **100 + 10% MÁU TỐI ĐA, +4%/cấp** (`SatThuongVong`; cấp 5 = 26%; số 100 giữ nguyên mọi cấp — tôi tự chọn, người dùng chỉ nói phần %);
  hệ BĂNG cho Kháng Băng (`GhiKeDanh(toi, HeSat.Bang)`) nhưng loại `Physical` để kháng băng ±25% của quái không làm lệch số.
  **Hồi chiêu 10 s đếm từ lúc HIỆN HÌNH** (đang tàng hình ô kỹ năng giữ đầy, bấm lại bị từ chối); `tangHinhCooldown` nay là THUỘC TÍNH đọc
  hằng. Hình: ảnh Blender MCP `CongCu/Blender/tang_hinh_vong_phep.blend` (đường cong + compositor Fog Glow, nền ĐEN → cộng sáng; nền mờ đã trừ
  + mờ dần ra mép, không thì hiện ô vuông; cùng ngày người dùng **bỏ 4 đuôi mũi tên + lớp sương trong vòng: CHỈ NÉT SÁNG**, cắt ngưỡng 0,13) → `Resources/KyNang/TangHinh/VongPhep.png`; `Vfx/VfxVongPhepTangHinh.cs` dựng **LƯỚI 24×24 BÁM
  ĐẤT** (tia chỉ lớp Ground, +6 cm) — tấm phẳng thì đất gồ ghề che mất nửa vòng; xoay / phóng to làm trên UV (`VongPhepSang`). Qua mạng: cấp đi
  theo gói phép (`capKyNang`); `ApTuMang` bỏ qua bit cũ trễ 1,5 s sau khi nổ (không nổ vòng thứ hai). ⚠️ **Lỗi cũ đã sửa**: bản sao nhận bit
  tạo component MỚI mang `conLai` mặc định = `ThoiGian` rồi `Max` → người khác hiện hình rồi vẫn tàng hình trên máy mình 20 s (nay 90 s).
  Menu 73 mục V, W.
- **Tàng hình** (`Combat/TangHinh.cs`, shader `S_TangHinh`, 18/09/2026): thay TOÀN BỘ vật liệu model bằng shader viền fresnel (khác
  `FrozenEffect` chỉ phủ thêm lớp); 20 giây, hồi chiêu 30 s, 30 năng lượng; **quái không thấy** (`GameDirector.GanNhat` bỏ qua, `EnemyAI`
  chọn lại ngay), **miễn mọi hiệu ứng** (`TangHinh.ChanHieuUng` chặn ở đầu mỗi `Apply`, và xoá hiệu ứng đang dính) nhưng **vẫn ăn sát thương**;
  đi nhanh +20%; **đòn đầu tiên bằng kỹ năng gây sát thương nhân đôi TOÀN BỘ sát thương của kỹ năng ấy** (mọi vệt Mưa băng, mọi quả trong chùm,
  cả sát thương cháy — vì nhân vào `manhHon`) rồi tan (Khiên / bình không tính); **hai giây cuối thân NHẤP NHÁY** (0,22 s, độ hiện 1 ↔ 0,12) và
  **chỉ máy của mình thấy** (`laCuaMinh && !tuMang` — bản sao không đếm được giờ thật); người khác chỉ thấy khi mình di chuyển,
  mình luôn thấy thân mình mờ. Bit mạng `CoTangHinh` = bit thứ 6 → **mặt nạ byte cờ nay 0x3F** (gói người chơi dùng hết 8 bit).
  ⚠️ Đòn đầu quyết định ở **`BeginCast`** (lúc bắt đầu niệm) chứ không ở `Release`, vì gói phép bay đi từ đó: cờ `donTangHinh` đi nhờ
  **bit cao của byte cấp kỹ năng** trong gói 17 byte, không thì máy kia phát lại phép với sát thương thường. Menu 73 kiểm.
- ⚠️ **Lửa địa ngục, QUẢ CẦU LỬA, QUẢ CẦU BĂNG XUYÊN VẬT NHỎ** (người dùng 19/09 rồi 25/09/2026): bia, mộ, đá không chặn đường;
  **cây cối, nhà và LÒ LỬA vẫn chặn** (lò 2,33 m nhỏ hơn ngưỡng nhưng `LaVatNho` loại riêng mọi thứ có `LoLuaDa` — cả Lửa địa ngục,
  người dùng chọn). Cờ `Fireball.xuyenVatNho` (Lửa địa ngục + `SpawnChum(..., xuyenVatNho: true)` của Quả cầu lửa NGƯỜI CHƠI — quả
  cầu lửa của QUÁI không xuyên) và `QuaCauBang.xuyenVatNho` (mặc định bật); cả ba đi qua **`Fireball.VatCanChan`** dùng chung.
  `Fireball.LaVatNho`: xuyên khi collider **cao < 4 m VÀ ngang < 4 m**, trừ mặt đất, mọi thứ có `Damageable` và lò lửa. Đo bằng **kích thước, không theo tên** (40 kiểu lưới bia, tên không nói lên cỡ):
  Act2 bia mộ 0,16–3,18 m · đá 0,12–1,31 · lò lửa 2,33 | cây 6,54–17,47 · nhà mồ 4,22–5,15 · hàng rào 4,77 —
  khe trống giữa 3,18 và 4,22 rộng hơn 1 m. ⚠️ Phải quét **cả đoạn** (`SphereCastAll`) rồi lấy vật TO gần nhất: `SphereCast` chỉ
  trả một vật, mà vật ấy hay là cái bia chắn trước gốc cây — bỏ riêng nó thì quả xuyên luôn qua cây. Menu 72 mục K, menu 80 mục B.
  ⚠️ **Quả cầu băng và Mưa băng trúng LÒ LỬA thì DẬP TẮT như Gió lốc** (25/09/2026): `LoLuaDa.DapTatTrongVung(tâm, bán kính,
  GioLoc.GiayLoChayLai)` — lò trong vùng nổ 3,4 m của Quả cầu băng / vùng sát thương của tảng băng rơi (chỉ khi `damage > 0`),
  30 giây sau cháy lại. Menu 80 mục C.
- **Lửa địa ngục** (`Skills/LuaDiaNguc.cs`, 17/09/2026): **5 quả** `Fireball` (18/09/2026, trước là 4) toả 18° rồi **tự dí** (`Fireball.tocQueo` 360°/s,
  giữ ≥ 1,3 m trên mặt đất — đo đất ở BA chỗ: dưới quả, theo hướng bay, về phía mục tiêu; không thì đâm đất Act2) tối đa 5 kẻ gần người tung nhất
  trong 20 m; cấp 1 = impactDamage prefab Quả cầu lửa × 1,2⁴ (176); **31 năng lượng** (⚠️ số nằm trong **prefab `Player_Sorceress`**) · hồi chiêu 0,5 ·
  niệm 0,38; màu quả và vụ nổ **giống hệt Quả cầu lửa** (18/09/2026 bỏ lớp nhuộm đỏ sẫm); icon Blender MCP. Menu 72 kiểm.
  ⚠️ **05/10/2026: LỬA ĐỊA NGỤC CŨNG NẢY 6 LẦN như Quả cầu lửa** (người dùng): `LuaDiaNguc.SpawnChum` đặt `soLanNay = Fireball.SoLanNayNguoiChoi`;
  quả nảy giữ tên "LuaDiaNguc" + `diaNguc`. Menu 84 A3 (tung thật vào chuỗi 8 bia): 5/5 quả mang 6, 30 quả nảy, nguyên 176,3; menu 72: 0 lỗi.
- ⚠️ **25/09/2026: Giựt sét tầm 12 m = tầm SẤM SÉT** (`boltRange` 12 trong prefab — người dùng chọn giảm 20 → 12; menu 69 so hai số
  đọc từ nhân vật). **Quỷ cây tầm +20%**: dừng lại phóng ở `EnemyFactory.TamDanhQuyCay` 9,6 m (trước 8), tia bay `TamTiaQuyCay` 12 m
  (trước attackRange + 2 = 10; trường mới `EnemyAI.tamTiaSet`), giữ màu xanh lá, kiểu tia Giựt sét. ⚠️ Số nằm trong **prefab
  `Enemy_QuyCay`** (nướng bởi AssetBaker, đè code) — đã sửa cả prefab; menu 69 mục I sinh Quỷ cây THẬT từ kho quái mà đo.
- ⚠️ **09/10/2026: GIỰT SÉT TẦM 20 m** (người dùng "tăng tầm đánh thêm 8m": 12 → 20, `GiatSet.TamNguoiChoi` — không còn bằng Sấm sét, Sấm sét /
  Mây giông giữ `boltRange` 12; Sách phép ghi 20 mét). Menu 69 (0 lỗi): bia cách mặt 19,9 m trúng, 20,4 m trượt.
- **Giựt sét** (16/09/2026, hằng trong `GiatSet`: `TamNguoiChoi` 20 m (25/09 → 12, 09/10 → 20 lại), `SatThuongNguoiChoi` 75, `SoTiaNguoiChoi` 4, `XacSuatChoangNguoiChoi` 0,15,
  `GiayChoangNguoiChoi` 1,5 + 0,15 s/cấp): tối đa 4 tia cùng lúc (mỗi tia một kẻ địch phía trước), mỗi tia vẫn lan 5 lần; **mỗi cú trúng kể cả
  tia lan** gieo 15% choáng (`StunnedEffect.Apply` — bản sao mạng tự bỏ qua). Giựt sét của **quái** (`PhongCuaQuai`) không choáng. Menu 69 kiểm.
  ⚠️ **25/09/2026 VẼ LẠI theo ảnh mẫu người dùng gửi**: lõi trắng mảnh + quầng xanh + sợi rẽ nhánh + cụm điện bùng hai đầu,
  **ảnh dựng bằng Blender MCP** (`CongCu/Blender/giat_set.blend` → `Resources/KyNang/GiatSet/GiatSetLoi|GiatSetQuang.png`, ảnh XÁM để tô màu
  lúc chạy; nửa dưới 4 dải thân tia lặp liền mạch theo u, nửa trên 4 cụm bùng 2×2) — `LightningArc.anhBlender` + lớp hào quang
  `heSoHaoQuang`; **tia hiện 0,6 s** (`GiatSet.GiayTiaHien`, sáng nguyên 65% đời), **bám hai đầu** (`LightningArc.BamHaiDau`: giữa hai tay → thân
  kẻ địch); áp cho cả Giựt sét của quái (giữ màu). Phù thủy **ĐẨY HAI TAY** (`NguoiChoiHoatHinh.TuTheGiatSet`, ngắm hướng xương bằng
  `FromToRotation` chứ không cộng góc Euler), giữ tay suốt lúc tia hiện, bước đi thì hạ tay; tia mọc từ `NguoiChoiHoatHinh.DiemGiatSet`
  (giữa hai bàn tay), không từ đầu gậy. Cùng ngày người dùng chơi thử thấy "quá sáng, quá dày" → **mảnh lại** (`GiatSet.BeNgangTia`
  0,70 m, trước 1,35; lõi 1,2 · quầng 1,5 · hào quang 0,35/`HaoQuangDuc` 0,45): ⚠️ **ĐÁNH GIÁ Ở GÓC CHƠI THẬT** (máy quay "3D tự do"
  sau lưng, ban ngày) — nhìn dọc theo tia thì các tia chồng lên nhau ngay trước máy quay, ảnh chụp góc chéo không thấy
  (menu 69 mục H đếm điểm ảnh chói: ngày 16,1% → 5,6%). Lần ba (người dùng: "tia trắng mảnh hơn 50%, thêm xanh bọc ngoài"): ảnh vẽ lại
  bằng Blender MCP (lõi ống 0,00275 = một nửa; quầng mảnh ở tâm, loang rộng 70 px ×1,8), quầng 2,2, hào quang 0,55/0,60, lõi 1,0 và
  ⚠️ **màu quầng XANH ĐẬM (0,14 0,34 1)** — quầng sáng lên mà kênh đỏ/lục còn cao thì TÂM NGẢ TRẮNG (đo được: trắng tăng 7,0 → 9,6).
  "Trắng dày" khi chơi là tâm quầng cháy sáng, không phải lõi (lõi ở ảnh cận chỉ 0,6 điểm ảnh) — đo bằng **cắt ngang tia**
  (`ThuGiatSet.CatNgang`): đêm trắng 7,0 → 1,6, xanh 24 → 47,6. Lần bốn ("viền xanh sáng và dày hơn 10%"): lõi và quầng nay là
  **HAI LƯỚI riêng** — quầng rộng `LightningArc.HeSoVienXanh` 1,10, cùng đường đi/dải ảnh/nhánh (gieo một lần), chu kỳ u theo bề ngang
  gốc; quầng 2,42, hào quang 0,605/0,66. Mục H tung 5 lần lấy trung bình (một ảnh dao động hơn 10%).
  ⚠️ **Quả cầu điện DÙNG CHUNG kiểu tia** (người dùng 25/09/2026): `GiatSet.KieuTia` (gọi từ `VfxFactory.TiaCauDien`, màu
  `GiatSet.MauQuangNguoiChoi`, bám cầu → kẻ địch) — sửa kiểu tia là đổi cả hai; tia cầu điện vẫn sống 0,22 s (`GiaySongTiaCauDien`,
  cầu bắn mỗi 0,4 s). Menu 74 mục K kiểm. ⚠️ Vật liệu static tạo lúc Play **bị xoá khi thoát Play** — kiểm bằng null của Unity, đừng
  dùng cờ "đã nạp" (tia từng âm thầm quay về kiểu cũ). Menu 69 mục G kiểm (có đối chứng từng mục).
  ⚠️ **05/10/2026: HẾT "KHỐI ĐEN" Ở HAI ĐẦU TIA + NHÁNH NHỎ PHỤ** (người dùng khoanh hai đầu tia: "bị che mất hình bởi khối đen"; xin "nhiều
  nhánh sét nhỏ phụ"). Nguyên nhân: tia 10 m chỉ ~11 khúc gấp (`segments` = dài × 1,1), cả ba lớp (lõi, viền, hào quang) vuốt nhọn về 0 trong
  khúc ĐẦU / CUỐI (~0,9 m) → mỗi lớp thành TAM GIÁC cạnh thẳng, ảnh bị ép mất lõi trắng, hào quang đứt cạnh sắc = khối tối hình tam giác. Sửa
  (`LightningArc`, chỉ chế độ ảnh — Giựt sét, Quả cầu điện, Quỷ cây): chia nhỏ đường đi ≤ `BuocChiaNho` 0,15 m, vuốt theo MÉT (`MetVuotDau/Cuoi`
  0,3 m) + MỜ DẦN bằng alpha đỉnh, bề ngang chỉ thu về `TiLeNgangDau` 0,45; hào quang `AppendHaoQuang` (trước vuốt 14% theo CHỈ SỐ đỉnh).
  `DoiChungDauCu` giữ cách cũ cho phép thử. **Nhánh nhỏ phụ** `nhanhNho` = `GiatSet.SoNhanhNhoMoiMet` **1 / mét** (người dùng chọn qua ảnh menu
  105b 0 / 0,5 / 1 / 1,5; áp CẢ BA tia dùng `KieuTia`): sợi 0,35–1 m mọc 8–92% thân, ngang 0,38 thân. Menu 105 (0 lỗi, tia thật, máy quay 2.5D,
  vẽ riêng tia lên nền đen): lõi trắng ở 0,35–0,95 m từ tay thấp nhất 0,68 (cũ 0,15), quầng ở 0,35 m rộng 0,92 so với giữa tia (cũ 0,29 = tam
  giác); 8 nhánh nhỏ / tia 8,2 m. Menu 69 (sửa bộ lọc mục I: Quỷ cây có 15% choáng từ 28/09 nên lọc "choáng 0" không bắt được tia) + 74: 0 lỗi.
  ⚠️ **Cùng ngày: QUẦNG XANH RỘNG HƠN 20%** (người dùng gửi ảnh mẫu tia sét bọc quầng xanh dày; chọn "rộng hơn 20%", độ sáng giữ; chỉ Giựt sét +
  Quả cầu điện — Quỷ cây giữ): `GiatSet.KieuTia(..., dayQuang)` nhân `heSoVien` (1,10 → 1,32) và `heSoHaoQuang` (0,605 → 0,726) với
  `GiatSet.HeSoQuangDay` 1,2; tia quái (`GiatSet.cuaQuai`, đặt trong `PhongCuaQuai`) không nhân. Menu 107 (tia thật, vẽ riêng nền đen, 10 lần × 6 lát
  cắt, đối chứng hệ số 1): vùng xanh nhìn thấy ×1,23, vùng xanh sáng ×1,24; Quả cầu điện 1,32; Quỷ cây 1,10 / 0,605. ⚠️ Đo 4 lần × 3 lát ra ×1,12
  — tia gấp khúc ngẫu nhiên làm nhiễu, phải lấy nhiều lần. Menu 69, 74: 0 lỗi.
- ⚠️ **BỊ CHÁY = LỬA CHÁY LAN TRÊN CHÍNH THÂN, NẰM TRONG VIỀN NGƯỜI, TẮT NGAY** (người dùng 28/09/2026, ba lần; `Vfx/LuaToanThan.cs`,
  gọi từ `VfxFactory.AttachBurning` — prefab `Vfx_BongChay` KHÔNG dùng nữa): phủ THÊM vật liệu `Diablo25D/LuaPhuThan` (`S_LuaPhuThan.shader`,
  vật liệu gốc `Resources/KyNang/Chay/LuaPhuThan.mat` để vào bản build, mỗi kẻ một bản sao) lên mọi Skinned/MeshRenderer như lớp than
  `ChayDenToanThan`; lượt 1 lửa trên da, lượt 2 vỏ phồng 7 cm chỉ hiện ở viền. Hoa văn Blender MCP `KyNang/Chay/LuaPhuThan.png` (lặp
  liền mạch; R sọc, G đám, B nứt), toạ độ = trục phải máy quay × độ cao thân, trừ `_Goc` (hông / xương thấp nhất mỗi khung) → chạy không
  trượt. Lần 4 (người dùng: "thấy rõ đang bốc cháy + ít khói"): lớp phủ uốn lượn + nhấp nháy (`TocDoLua/XoanLua/NhapNhayLua`, đồng hồ riêng
  `_ThoiGian` thay `_Time.y`), **lưỡi lửa** (ảnh ngọn lửa đơn `Flipbooks/LuaChayNguoi`) liếm lên từ đầu/vai/tay, **khói xám ĐEN mỏng** sinh ở đỉnh
  lưỡi lửa (khói xám sáng làm quầng quanh thân / chìm vào nền được ánh lửa rọi). Khối billboard lần 2 chỉ còn là đối chứng (`DoiChungKhoiLua`).
  ⚠️ **04/10/2026: LƯỠI LỬA BAO QUANH TOÀN THÂN, KHÔNG BAY LƠ LỬNG** (người dùng: "vết lửa phải bao quanh lấy toàn thân, không được bay lơ
  lửng trên không" — mọi nhân vật + quái): lưỡi lửa KHÔNG còn bay lên (0,6–1,0 m/s × 0,45–0,7 s → vọt quá đầu 0,72 m), nay vận tốc ~0, đời
  0,30–0,45 s (xương cử động thì lưỡi sống lâu tách khỏi chi), sinh KHẮP NGƯỜI (thêm hông, vai, đùi, cẳng chân, bàn chân), 46/giây. Khói xám
  đen bốc từ đầu (lần 4) giữ. Menu 90 F (12 khung): gốc lưỡi lửa cách đoạn xương gần nhất ≤ 0,12 m, tốc độ ≤ 0,04 m/s, 55 lượt thân dưới /
  138 thân trên, vượt đỉnh đầu ≤ 0,19 m; menu 90 nay đặt `tiLeDoDon = 0` cho Bộ xương (45% đỡ đòn chặn cả cháy → lỗi chập chờn A / B). 0 lỗi.
  Hết chạy / bị gỡ: xoá ngay, gỡ đúng lớp của mình. **Nhịp cháy không phun tia trúng đòn** (`Damageable.BoQuaTiaTrungDon`).
  ⚠️ `BurningEffect.Apply` mắc bẫy AddComponent: cháy luôn ≥ 4 s và ≥ 6 máu/giây — CHƯA sửa, đã hỏi người dùng.
  ⚠️ **05/10/2026: LỬA MỎNG BỚT — MẬT ĐỘ 0,4** (người dùng: "giảm bớt mật độ lửa phủ trên người, nhiều quá, cho mỏng bớt"; chọn MỨC 4 qua ảnh
  **menu 90b** chụp 1 / 0,75 / 0,55 / 0,4 → thân phủ lửa ~98 / 90 / 75 / 56%): `LuaToanThan.HeSoMatDo` (= `MatDoChon` 0,4, đặt vào vật liệu mỗi
  khung) → shader `_MatDo` NÂNG NGƯỠNG nhiệt (−(1−mật độ)×0,45: chỉ còn mảng / sọc lửa nóng nhất, lộ thân giữa chúng; vết nứt than hồng ×mật độ) và
  số lưỡi lửa/giây ×mật độ (46 → 18,4). Khói, tàn lửa, đèn giữ. Menu 90 A nay kiểm thân có lửa 35–75% + đối chứng mật độ 1 CÙNG khung ≥ 90%
  (đo: 59–60% / 97–98%); F số lưỡi lửa ≥ 8 × mật độ. 0 lỗi.
  Menu 90 kiểm bằng ẢNH (bóng thân trắng đặc lớp 31 + nới 14 px; TẮT bloom; tính theo DIỆN TÍCH): 99–100% lửa trong viền, đối chứng 69%.
- ⚠️ **TỐI ĐA 6 NGƯỜI; CHẾ ĐỘ ĐƠN / ĐÔI** (người dùng 28/09/2026, `Mang/CheDoTran.cs`): giới hạn 4 → 6 ở `PhongMang.SoNguoiToiDa`,
  `KenhTrucTiep.SoKenhToiDa`, `GoiTin.SoGheToiDa`, luật Firebase (`toiDa`/`soNguoi` ≤ 6, `cho` ≤ 5 — ĐÃ deploy) và giao diện. **Đơn** =
  luật cũ (người sống sót cuối cùng). **Đôi** = Đội A / Đội B, tối đa 3 mỗi đội, chọn lúc TẠO PHÒNG (`phong/{ma}/cheDo` "don"/"doi",
  phòng cũ = Đơn; `nguoiChoi/{uid}/doi` 0/1, thiếu → −1). Vào phòng Đôi → đội ít người (bằng → A); tự bấm "VÀO ĐỘI …", chủ phòng có
  "CHUYỂN ĐỘI"; chủ phòng cân bằng mỗi nhịp (`CheDoTran.CanBangDoi`: thiếu đội / đội > 3 → người vào sau cùng sang đội kia). Bắt đầu
  lúc nào cũng được; cả phòng một đội → thắng ngay (đợi đủ người vào trận, tối đa 45 s). ⚠️ **Đồng đội không đánh nhau**: mọi chỗ
  kỹ năng loại người tung dùng `CheDoTran.BoQua(boQua, d)` (người tung HOẶC đồng đội — cả tự nhắm, quả nảy, khiên) + lưới an toàn
  trong `Damageable.TakeDamage`; thêm kỹ năng mới thì dùng hàm này, đừng viết `d == boQua`. `Damageable.doi` gán cho nhân vật mình và
  bản sao (`KhoiDongTranMang.GanDoi`). Xuất phát `ChoXuatPhat.ChoChoDoi` (đồng đội 2,5–6 m, hai đội ≥ 55 m). Gói kết trận 15 byte, byte
  cuối = mã đội thắng (0 = Đơn — mặc định struct). Tên trên đầu màu đội + "ĐỘI A/B"; HUD dòng thứ ba "ĐỘI A (bạn) 2/3 còn sống ·
  …"; bảng điểm xếp theo đội. Menu 92, 92b kiểm (0 lỗi). Menu 45 mục 2e và menu 55 mục B1b từng hỏng vì đợt đầu chờ 30 s (đo lúc
  chưa có quái) — nay tự sinh một đợt thật (`DamBaoCoQuai`), 0 lỗi. Phép thử mới đo quái cũng phải làm thế.
- ⚠️ **ĐẶC TÍNH QUÁI** (người dùng 28/09/2026, đặt trong `EnemyFactory.ApDacTinh` lúc sinh — KHÔNG trong prefab): **Quỷ cây** mỗi tia
  15% choáng người chơi 1 s (`EnemyAI.xacSuatChoangTia`, qua `GiatSet.PhongCuaQuai`); **Quỷ dữ** thiên thạch 15% đánh ngã 1 s
  (`xacSuatNgaThienThach`); **Bộ xương** tốc **×1,8225** (1,35 × 1,35 — người dùng xin thêm +35% CỘNG DỒN), **mỗi đòn 35 ở ĐỢT 1**
  (gốc lúc sinh 35 / 0,65 = 53,85 vì GameDirector nhân 0,65 — `SatThuongBoXuongDot1`; trước đó cùng ngày là 55), **cứ 0,3 s một đòn** (hồi chiêu 0,3, vung 0,25 —
  vung bằng hồi chiêu thì nhịp đo ra 0,356), **45% ĐỠ ĐÒN** kỹ năng người chơi (`Combat/ChongDo.cs`,
  `Damageable.tiLeDoDon`): không mất máu + tránh cả hiệu ứng của đòn ấy, mỗi đòn gieo riêng (một lần mỗi khung mỗi con), **sát thương rỉ
  không gieo** (`Damageable.LaSatThuongRi` quanh nhịp cháy / lốc cuốn / vũng lửa / cây cháy); hình vòm khiên bạc xanh + chữ **"ĐỠ ĐÒN!"**
  (`Vfx/VfxCauDoDon.cs`). ⚠️ Bản sao quái trên MÁY KHÁCH không nhân hệ số đợt → **gói "quái ra đòn" mang SÁT THƯƠNG THẬT của chủ
  phòng** (`GoiTin.MotDonQuai.satThuong`, gói 17 byte; gói cũ 15 byte → 0 = dùng số bản sao), máy khách đặt lên bản sao trước khi
  `DienLaiDon` (28/09/2026, menu 39 mục 2b/3b/7). Mạng: chỉ chủ phòng gieo, **bit 7 byte cờ gói quái** = vừa đỡ (gói quái nay dùng HẾT 8 bit). Phép thử gắn hiệu ứng
  thẳng lên Bộ xương để đo cơ chế khác phải đặt `tiLeDoDon = 0` (đã sửa menu 71 H3, ThuBangNguoiChoi B4). Menu 91 kiểm.
- ⚠️ **XÁC NẰM TRÊN VŨNG MÁU** (người dùng 26/09/2026, `Combat/XacNam.cs`, gắn trong `Damageable.Die`): quái và người chơi chết thì
  **nằm hẳn theo tư thế NGẪU NHIÊN: ngửa / sấp / nghiêng trái / nghiêng phải** (1/3 ngửa · 1/3 sấp · 1/3 nghiêng, `XacNam.KieuNam`,
  `ChonTuThe`) — lật MODEL CON 90° quanh trục của gốc như `BiDanhNga` (`XoayNam`); các bộ hoạt hình thấy `XacNam.LaXac` thì bỏ tư thế
  gục. ⚠️ "Ngẫu nhiên" **giống nhau trên mọi máy, 0 byte gói tin**: gieo từ `NhanDangQuai.id` (quái) hoặc ghế + mã phòng (người chơi,
  băm FNV — không dùng `string.GetHashCode`); chết lúc đang bị đánh ngã thì giữ nằm ngửa (lật 180° đi qua tư thế đứng). Tâm vũng máu =
  giữa xương Hips và Head sau khi nằm. **Chống lún hai bước**: xương THÂN (hông, sống lưng, cổ, đầu, đùi — không tay, bàn chân, đầu
  mút: tay dang của tư thế gốc / chân bước dở lúc chết chống cả xác lơ lửng) rồi **BakeMesh một lần** lúc ngã xong, nhấc để ≤ 10% đỉnh
  dưới đất (Quỷ cây lưng dày: chỉ theo xương thì 51% đỉnh chìm, thân biến mất). Đã thử và BỎ: duỗi chân về bind pose (tư thế gốc dạng chân). **Vũng máu nhỏ** loang dưới thân trong 2,4 s (ảnh **Blender MCP** `CongCu/Blender/vung_mau.blend`
  → `Resources/KyNang/Mau/VungMau0–3`, 4 biến thể, mép alpha 0; đĩa `GroundRing.BuildDisc` bám CHỈ lớp Ground, xoay ngẫu nhiên). Người dùng
  chọn: **xác QUÁI nằm 20 s rồi chìm 2 s và xoá** (cả vũng máu) — trước đó `corpseSeconds` 5–6 s, nay **không dùng nữa**; **xác NGƯỜI
  CHƠI nằm tới hết trận**. Cỡ thân đo từ **XƯƠNG THẬT** (`rig.bodyHeight` sai: bộ xương 2,36, quỷ cây 2,89 m trong khi hình cao 1,67/1,69);
  **chống lún** bằng xương thấp nhất (quỷ cây từng chui 57,6% xuống đất). Vũng máu dùng shader **không nhận sáng nhân 0,6**
  (`HeSoSangMau`): 1,0 thì giữa đêm đỏ chói như phát sáng, shader NHẬN sáng thì ánh trăng xanh làm máu đen kịt không thấy.
  Chết lúc đang ngã / đang bị hất: lấy tư thế gốc của `BiDanhNga`/`BiHatTung`, chờ rơi xong mới loang máu. ⚠️ Chết khi **đang đóng băng hoàn
  toàn** thì vẫn **vỡ tan** như cũ, không có xác. Menu 85 kiểm (đo bằng BakeMesh + xương Head/Hips, không đọc biến của XacNam).
- ⚠️ **DẤU "+" NÂNG CẤP TRÊN Ô KỸ NĂNG** (người dùng 26/09/2026, `GameHUD.CoDauCong/BamDauCong`): có điểm kỹ năng thì ô nào **mở khoá hoặc
  nâng cấp được** (y nút trong Sách phép: `CapDo.MoKhoaDuoc || NangCapDuoc` — ô khoá mà đủ bậc cũng có, bình máu/mana tới cấp 3 cũng có) hiện
  huy hiệu "+" nhỏ; bấm là mở khoá / nâng ngay. Hình là **ảnh Blender MCP** `Resources/GiaoDien/DauCong.png`
  (`CongCu/Blender/dau_cong.blend`, 26/09/2026 — người dùng chê bản vẽ bằng hai thanh "sơ sài"): "+" máu tươi cạnh lởm chởm, ba giọt
  chảy, vệt bóng ướt, đĩa nứt đỏ như dung nham, vành xương mười gai — nằm GỌN trong khung cũ, hình học / vùng bấm không đổi. Ô VUÔNG: giữa cạnh trên, 0,34 cạnh ô (28,6), khe tới ô khác 39,7; nút `GUI.Button` của
  "+" gọi **TRƯỚC** ô bên dưới (IMGUI cho nút gọi trước giành cú bấm), `DocInput.ConTroTrenThanhKyNang` chặn chuột trên "+". Nút TRÒN:
  bán kính **0,36r** (27/09/2026 to thêm 20%, trước 0,30r — ngón tay không ấn trúng), tâm cao 0,90r trên tâm nút, lấn vào viền chính nút ấy; khe hẹp nhất tới nút khác **9,7** (vùng
  chạm ×1,2: 5,0); cảm ứng xét "+" **trước** `NutTaiDiem` — chạm giữa nút vẫn là tung kỹ năng. Ẩn khi Sách phép mở / đã chết / hết trận.
  Menu 85 mục D.
- ⚠️ **QUẢ CẦU LỬA và QUẢ CẦU BĂNG NẢY** (người dùng 26/09/2026, chọn 1 lần · 100% · 6 m): quả nổ mà trúng kẻ địch thì sinh
  **một quả nảy** bay sang kẻ địch gần chỗ nổ nhất CHƯA dính vụ nổ (`Fireball.TamNay` 6 m, `TimKeNay`), tự dí mục tiêu, sát thương
  y quả gốc; kẻ đã trúng nằm trong `khongCham` nên không ăn lại, không bị gieo ngã lại. `soLanNay` = 1 cho quả của NGƯỜI CHƠI
  (`SpawnChum(..., soLanNay: 1)`, Quả cầu băng mặc định 1), 0 cho quả của quái và Lửa địa ngục. ⚠️ Mặt nạ vật cản của người chơi
  CÓ lớp Enemy → quả nảy sinh sát kẻ vừa trúng đâm ngay vào thân nó: `Fireball.VatCanChan(..., boRa)` bỏ qua các kẻ trong
  `khongCham`, và quả nảy xuất phát lệch 0,5 m về phía kẻ mới. Menu 84 kiểm (B/A 1,02–1,03, đối chứng bia một mình · soLanNay 0).
  ⚠️ **05/10/2026: QUẢ CẦU LỬA NẢY TỚI 6 LẦN** (người dùng: "nảy qua nảy lại 6 lần khi có đối thủ khác ở gần"; chọn giữ nguyên sát thương mỗi
  lần · chỉ sang đối thủ MỚI): `Fireball.SoLanNayNguoiChoi` 6 (`PlayerController` truyền vào `SpawnChum`); mỗi lần nảy trừ 1 và mang theo
  `khongCham` cộng dồn nên không quay lại kẻ đã trúng, hết kẻ mới trong 6 m thì dừng sớm. Quả cầu băng vẫn 1. Menu 84 A2 (chuỗi 8 bia cách 3,9 m):
  6 lần nảy, mỗi quả nảy mang đúng 85, 7 bia đầu mỗi bia trúng 1 lần, bia 8 không; đối chứng soLanNay 1 → 2 bia. 0 lỗi.
  ⚠️ **05/10/2026: QUẢ CẦU LỬA + VỤ NỔ DỊU SÁNG** (người dùng: "quá sáng, chơi bị chói"; chọn áp MỌI quả cầu lửa — người chơi, quái Phù thuỷ,
  Lửa địa ngục; chọn mức qua ảnh menu 108): `VfxFactory.DiuSang(goc, k)` nhân `_Intensity` mọi lớp phát sáng (qua MaterialPropertyBlock — không sửa
  vật liệu dùng chung) + đèn (`LightFlicker.baseIntensity`, `LightBurst.peak`); khói giữ. **Khi bay ×0,55** (`HeSoSangQuaCauLua`, gọi cuối
  `Fireball.Spawn` sau `NangCapDuoiLua`), **vụ nổ ×0,25** (`HeSoSangNoLua`, trong `FireExplosion`; Thiên thạch gọi thẳng `BuildFireExplosion` nên
  không đổi; quả cầu trên tay màn chính dùng `BuildFireballVisual` nên không đổi). ⚠️ Vụ nổ ×0,4 vẫn trắng chói (vỏ lửa + hạt bùng cộng sáng chồng nhau
  bão hoà) — phải chụp thêm mức mạnh tay. Menu 108 (đặt quả cầu đứng yên + vụ nổ ở CÙNG một chỗ, trung bình mọi khung, trừ nền): độ sáng thêm khi bay
  ×0,34, khi nổ ×0,45, điểm ảnh trắng chói khi nổ 0,87% → 0,19%; tung thật: đèn bay 3,30 (6 × 0,55), đèn nổ 5,50 (22 × 0,25). Menu 70: 0 lỗi; menu 72
  hỏng chập chờn ở mục khác nhau mỗi lần (C đường dí / K4 bia thật ngẫu nhiên — quả vẫn xuyên bia rồi trúng vật phía sau), không liên quan.
- **Quả cầu lửa** sát thương **85** (16/09/2026) — ⚠️ số nằm trong **prefab `Skill_QuaCauLua`** (đè code).
  ⚠️ **CẤP 5: 30% ĐÁNH NGÃ** 1,5 giây (người dùng 19/09/2026, `Fireball.NgaXacSuatCap5` / `CapDanhNga`): dùng lại
  `ThienThach.GieoDanhNga` nên tự bỏ qua người tung, kẻ đã chết và kẻ đang có khiên; **mỗi quả trong chùm gieo riêng**
  như loạt Thiên thạch. Mặc định `ngaXacSuat = 0` nên quả cầu của quái không dính. Menu 70 mục D. Vệt lửa phía sau vẽ lại bằng
  **Blender MCP** (`CongCu/Blender/vet_lua_qua_cau_lua.blend`): hạt `Flames` đổi từ `Tex_flame.png` (một **hình tam giác**) sang flipbook
  `Flipbooks/LuaDuoi` (4×4 đám lửa cuộn) + `TrailRenderer` vệt lửa dài `KyNang/QuaCauLua/VetLuaDai`; sửa lúc chạy trong
  `VfxFactory.NangCapDuoiLua` (gọi từ `Fireball.Spawn`, cả quả cầu của quái), nổ thì `ThaDuoiLua` thả vệt ra tan dần. Menu 70 kiểm.
  ⚠️ **04/10/2026: TÀN LỬA = NGỌN LỬA THẬT** (người dùng: tàn lửa "giống các thanh nhỏ màu lửa, không phải lửa thật; dùng Blender MCP dựng
  ngọn lửa thật, không như hình tam giác"; chọn áp cả màn chính lẫn trong trận + thay luôn lửa lõi quả cầu trên tay màn chính). Blender MCP
  `CongCu/Blender/ngon_lua_that.blend` (scene `NgonLuaThat`): **mô phỏng Mantaflow** (miền 0,8×0,8×2 m, phân giải 96, nguồn cầu r ~0,19 ở
  đáy — ⚠️ lần đầu đặt nguồn NGOÀI miền, lửa = 0), vật liệu thể tích phát sáng theo trường `flame`, Cycles trực giao khung ĐỨNG 1:2 → 16 khung
  (40–70 bước 2) → `Resources/KyNang/QuaCauLua/NgonLuaThat.png` 4×4 (128×256 mỗi ô, nền đen — cộng sáng). `VfxFactory.DoiThanhNgonLuaThat`:
  billboard ĐỨNG (không xoay, không kéo dài), cỡ 1:2, chạy hết 16 khung một đời hạt, **pivot 0,38** (gốc hạt ở chân lửa — tâm giữa ảnh thì nửa
  ngọn chìm vào lõi sáng), đời ngắn 0,23–0,42 s, bốc lên nhẹ (bản đầu 0,6 s / −0,35 thành cột ngọn nến rời bay quá đầu). `TanLuaThanhNgonLua`
  (lớp `Sparks`) gọi trong `NangCapDuoiLua` (mọi quả cầu lửa trong trận) và `TuTheTrungBay`; màn chính thêm lửa lõi `Flames` → ngọn lửa thật
  (trong trận lõi vẫn `LuaDuoi`). Menu 70 C (0 lỗi): Sparks Billboard, ảnh NgonLuaThat 4×4, rộng/cao 0,50; menu 101c 0 lỗi. **Menu 102**: ba
  quả bay song song, mỗi quả một máy quay nền đen, 20 khung × 3 lượt: tổng sáng ×1,05 (tàn lửa cũ ×1,02) so với không tàn lửa, điểm cháy trắng
  ×1,00 — ảnh một khung của menu 70 "×2,9 điểm cháy" chỉ là dao động.
- ⚠️ **MỞ KHOÁ THEO BẬC** (người dùng 19/09/2026, `CapDo.dieuKienMo` + `DuBacDeMo` + `KyCanTruoc`/`CapCanTruoc`):
  LỬA Cầu lửa **cấp 2**→Thiên thạch, Thiên thạch **cấp 5**→Lửa địa ngục · BĂNG Cầu băng 2→Mưa băng, Mưa băng 5→Tàng hình ·
  SÉT Giựt sét 2→Sấm sét, Sấm sét 5→Cầu điện · PHONG Gió lốc 2→Lốc xoáy, Lốc xoáy 5→Hoá lốc xoáy, **Lốc xoáy 5→Mây giông** (26/09/2026). **Chỉ chặn lúc MỞ KHOÁ**;
  HỖ TRỢ và BỊ ĐỘNG không có điều kiện. Sách phép hiện `SachPhep.NhacDieuKien` trên dòng cấp và trên nút.
  ⚠️ Kịch bản chạy thử nào gọi `CastAt` cũng phải mở khoá trước, **kể cả kịch bản cũ chỉ chụp ảnh**: `CastAt` từ chối
  trong IM LẶNG (không ngoại lệ, không dòng log), nên triệu chứng là thứ khác hẳn — menu 18d báo "thả 8 giây mà cây
  không bắt lửa" (19/09/2026); menu 4 và menu 7 cũng sót, đã sửa.
  ⚠️ Phép thử phải gọi **`CapDo.MoCaDuongChoPhepThu(ky)`** chứ không `MoKhoa` (đã thay ở 18 file); hàm ấy lấy điểm bằng
  `ThemDiemChoPhepThu` **không qua kinh nghiệm** — bản đầu bơm kinh nghiệm làm nhân vật nhảy lên cấp 20, mà ở cấp tối đa
  thì giết quái không còn kinh nghiệm và menu 61 đo ra "+0" ở mọi kỹ năng.
- ⚠️ **Bình máu / mana** (`CapDo.KyBinhMau/KyBinhMana`, `PlayerController.UongBinh`, hằng `MauMoiBinh 200`, `ManaMoiBinh 75`
  (19/09/2026, trước là 100/50 — phép thử phải ĐỌC HẰNG, đừng chép tay con số),
  `HoiChieuBinh 0.5` — HẰNG chứ không phải trường public để prefab không đè). ⚠️ **25/09/2026: CÓ SẴN CẤP 1** (`BatDauTranMoi`
  đặt, không tốn điểm) và **nâng tới cấp 3** (`CapDo.CapBinhToiDa`, người dùng chọn): mỗi cấp +75 máu / +45 mana
  (`MauBinhTheoCap` 200/275/350, `ManaBinhTheoCap` 75/120/165); phép thử đếm "đầu trận không kỹ năng nào mở" phải trừ hai bình
  (menu 60, 66 đã sửa). Không niệm, không
  đi qua gói kỹ năng. **Bình rơi CHUNG cả phòng** (`QuanLyBinhRoi` + `BinhRoi`): máy trọng tài quái gieo **25% bình máu / 15% bình mana** (27/09/2026, trước 10%/10%; hai lần gieo độc lập — `QuanLyBinhRoi.TiLeRoiBinh*`) khi quái chết và
  gửi `LoaiBinhRoi`; máy nào có nhân vật tới gần 3,5 m thì xin (`LoaiXinBinh`); **chủ phòng giao cho người xin TRƯỚC**
  (`LoaiBinhThuoc`), mọi máy thấy bình bay vào đúng người, chỉ máy người ấy cộng số bình. Cả ba gói gửi lặp 3 lần (kênh
  như UDP). Menu 65 kiểm (tỉ lệ, nhặt, uống, số bình trên ô, 5 ca mạng).
- **CẤP ĐỘ VÀ KINH NGHIỆM** (`Assets/Scripts/Player/CapDo.cs`, 13/09/2026) — **mọi con số ở `kinhnghiem.md`**.
  Vào trận ai cũng cấp 1, tối đa **cấp 20** (13/09/2026, trước là 10); **tính theo từng trận, không cất lại** (mỗi trận là một ván đấu riêng).
  Mỗi cấp: máu +15%, mana +10%, tốc độ +3,5% **chỉ tới cấp 10** (`CapTangTocToiDa`) (nhân dồn), và +1 điểm kỹ năng. Bảy kỹ năng **đều khoá lúc đầu**,
  cấp 1 có sẵn 1 điểm. Kỹ năng tối đa cấp 5: +20% sát thương, +10% mana, hiệu ứng +0,15 s mỗi cấp; riêng Khiên
  +15% máu khiên. Mở/nâng trong **Sách phép** (nút MỞ KHOÁ / NÂNG LÊN CẤP cao **60**, chữ 20 — `CuaSoSachPhep.CaoNutHoc`, 27/09/2026 người dùng xin to gấp 1,5) (chữ cột trái ×1,15, phần thân chi tiết ×1,20 và cuộn được bằng con lăn / vuốt — `CuaSoSachPhep.HeSoChuKho/HeSoChuThan`).
- ⚠️ **Cấp kỹ năng đi kèm từng gói tung phép** (`GoiTin.MotPhep.capKyNang`, gói 17 byte): phép của người cấp 5
  phải mạnh đúng cấp 5 trên mọi máy. `PlayerController.capPhepDangTung` là cấp của NGƯỜI TUNG, không phải của
  người xem. Kinh nghiệm giết quái do **chủ phòng** chia (gói `LoaiKinhNghiem`, 4 byte) vì chỉ nó chạy AI quái;
  kinh nghiệm giết người đi theo gói `LoaiChet` mà máy nạn nhân đã gửi.
- ⚠️ **Mọi đường gây sát thương của người chơi phải gọi `GhiKeDanh` trước `TakeDamage`** — kinh nghiệm, bảng điểm
  và dòng "Bị … hạ" chỉ đọc `Damageable.keDanhCuoi`. 13/09/2026 sót ở 6 chỗ (Mưa băng, Sấm sét, lốc cuốn, cháy, vũng lửa
  Thiên thạch, cây cháy) → giết bằng chúng không ai được gì. Thêm kỹ năng / hiệu ứng gây sát thương mới thì **chạy menu 61**.
- **SÁCH PHÉP** (`SachPhep.cs` + `CuaSoSachPhep.cs`, 12/09/2026): người chơi tự kéo thả kỹ năng vào các ô.
  ⚠️ **Kỹ năng CÒN KHOÁ vẫn kéo thả vào ô được** (người dùng 26/09/2026, `CuaSoSachPhep.KeoDuocTuKho`; trước đó bị chặn) — ô hiện
  tối + ổ khoá, chạm không tung, có điểm thì dấu "+" trên ô mở khoá ngay; chỉ nhóm BỊ ĐỘNG không kéo được. Menu 85 mục D kiểm.
  `SachPhep` chỉ giữ **bản đồ ô → số hiệu kỹ năng**; số hiệu KHÔNG đổi (nó là thứ đi qua mạng và vào
  `PlayerController.CastAt`), chỉ chỗ ngồi đổi. **Hai bộ ô riêng**: `OTron` (cảm ứng) và `OVuong` (máy tính) —
  bảy nút tròn xếp hai cung không có "thứ tự trái sang phải" dùng chung được với hàng ô vuông. Lưu trong
  PlayerPrefs (`diablo25d.sachphep.*`). Ô trong bảng dùng **chính** `GameHUD.LechNut` nên hình cụm nút trong
  bảng khớp cụm nút ngoài trận (menu 59 đo: lệch 0,00%).
  ⚠️ **Cột danh sách xếp theo NHÓM HỆ** (người dùng chốt 18/09/2026), không theo số hiệu nữa: LỬA (Cầu lửa · Thiên thạch ·
  Lửa địa ngục) → BĂNG (Quả cầu băng · Mưa băng · Tàng hình) → SÉT (Giựt sét · Sấm sét · Quả cầu điện) → PHONG (Gió lốc ·
  Lốc xoáy · Hoá lốc xoáy · Mây giông) → HỖ TRỢ (Bình máu · Bình mana · Khiên · Tốc biến) → **BỊ ĐỘNG** (Kháng Lửa · Kháng Băng ·
  Kháng Sét · Kháng Phong, 19/09/2026). Bảng ở `SachPhep.KyNangTheoNhom` / `TenNhom` / `MauNhom`; mỗi nhóm có một
  dòng tiêu đề thấp hơn hàng thường, nên **mọi chỗ tính vị trí phải đi qua `CuaSoSachPhep.YCuaDong` / `CaoDong`**, không được
  nhân "chỉ số × chiều cao hàng" (phép thử menu 59 hỏi `CuaSoSachPhep.VungHangKyNang`). Số hiệu kỹ năng KHÔNG đổi.
  ⚠️ Thứ tự ô ghi **thẳng localStorage** trên WebGL (`CD_DocChuoi/CD_GhiChuoi` trong `CauNoiCaiDat.jslib`, 14/09/2026) — PlayerPrefs
  WebGL ghi không đồng bộ, đóng tab ngay là mất; ngoài WebGL vẫn PlayerPrefs.
- ⚠️ **ACT2: VÒNG ĐÊM 4 PHÚT → NGÀY 2 PHÚT → CHIỀU 2 PHÚT, LẶP LẠI TỚI HẾT TRẬN** (người dùng 25/09/2026; trước đó
  19/09 và 24/09 là ngày → xế chiều → đêm rồi dừng). `Art/ChuyenChieuSangDem.cs`, gắn từ `GameBootstrap.BuildSun` khi
  `WorldFactory.LaAct2()`. Vòng `ChuKy` 480 s: đêm 0–210 · **bình minh** 210–240 · ngày 240–330 · **chiều xuống** 330–360 ·
  chiều 360–450 · **hoàng hôn** 450–480 (người dùng chọn "chuyển mượt 30 giây": mỗi buổi giữ nguyên, 30 giây CUỐI mới
  chuyển). Act2 **không nướng lightmap** nên mọi ánh sáng là thời gian thực. Ba bộ `BoAnhSang`: **Ngày** (đèn
  `(1,00 0,95 0,86)` 1,35 ở **55°**) và **Chiều** (`XeChieu`, đèn `(1,00 0,64 0,34)` 1,45 ở **13°**, trời cam) là hằng;
  ⚠️ **Đêm ĐỌC THẲNG TỪ CẢNH** lúc Start, không chép lại. `ApGiay(giay)` / `TinhLuc`; góc đèn nội suy bằng **Slerp
  quaternion** (Lerp Euler có thể quay ngược cả vòng); sao tắt ở nửa đầu bình minh, hiện ở nửa sau hoàng hôn; buổi
  đứng yên thì không ghi lại mỗi khung; đồng hồ `Time.timeSinceLevelLoad`, không tốn gói tin.
  ⚠️ **Đang chạy phép thử thì ĐỨNG YÊN Ở ĐÊM** (`GiayGiuaDem`): nhận ra bằng sự có mặt của `ChayThuMang`, kiểm ở CẢ
  `Start` lẫn `ChayThuMang.Awake` — phép thử dài quá 3,5 phút mà vòng chạy thì sang ngày giữa chừng. Phép thử muốn một
  buổi cụ thể: tắt component rồi `ApGiay(GiayGiuaNgay / GiayGiuaChieu / GiayGiuaDem)`. Menu 79 bật `ChoPhepChuyenTrongPhepThu`.
  ⚠️ **Mười lò lửa chỉ cháy khi trời tối**: vào trận là đêm nên lò cháy ngay (`Gan` đặt `LoLuaDa.ChoPhepNhomLua = true`
  trong **Awake** của GameBootstrap); độ tối qua `MucNhomLua` 0,70 thì đổi trạng thái **một lần**: bình minh giây
  **220,9** (`GiayTatLua()`) `DapTat` cả mười (còn làn khói), hoàng hôn giây **469,1** (`GiayNhomLua()`) nhóm lại. Cờ tắt
  thì `Chay()` từ chối — kể cả lần hẹn 30 giây của Gió lốc (`DapTatRoiChayLai`), nên lò bị dập ban đêm không tự cháy
  giữa ban ngày. Menu 79 kiểm (31 mốc cả hai vòng, lò, Gió lốc, đồng hồ, đêm khớp mốc cũ, chỉ Act2).
- ⚠️ **Phép thử đo "sát thương nhân đôi" phải đọc SỐ GHI TRÊN TỪNG ĐÒN**, không đếm tổng máu bia mất: tổng ấy
  phụ thuộc bao nhiêu quả trúng và trúng chỗ nào (sát thương vùng giảm từ tâm ra rìa) — menu 73 ra 1,35 rồi 1,73
  ở hai lần chạy cùng một bản code (19/09/2026). Đọc `impactDamage` từng quả thì ra đúng ×2,00 mọi lần; và phải
  **lọc theo người tung** (`f.boQua`) vì quái trong cảnh cũng bắn quả cầu lửa.
- ⚠️ **CỤM 7 NÚT TRÒN (cảm ứng) ×1,08 và CẦN ĐIỀU KHIỂN ×1,10** (người dùng 19/09/2026, xin hai lần: to 20%
  rồi thấy to quá nên nhỏ bớt 10%): nút **65,578**, lề **109,296**, cung trong **252,75** · ngoài **423,52** ·
  nút thứ bảy **99,61**; cần điều khiển bán kính **189,75**, tâm **255,75**, lề 66. Góc GIỮ NGUYÊN. Đổi cỡ thì phải
  nhân **tất cả** cho cùng hệ số kể cả LỀ (và với cần điều khiển là cả TÂM — giữ tâm thì lề tụt 60 → 42,75 và cần bị
  cắt ở mép); chỉ nút to mà cung giữ nguyên là các nút chồng nhau. Cả cụm 567,6 × 561,8; hở hẹp nhất 16,6 / đường
  kính 131,2; cụm cách cần điều khiển ≥ 303,7 px. Màn NGANG không tràn mép nào (hẹp nhất 844×390: dư 34,8 phải ·
  31,6 dưới); màn DỌC vượt mép trái 53,5 (bản gốc cũng vượt 20,7 — bản cảm ứng chơi ngang).
  ⚠️ **Đối chứng của menu 78 phải độc lập với hệ số đang chỉnh**: bản đầu là "thu cụm về 1/hệ số" nên khi hệ số
  đổi 1,2 → 1,08 thì nó hết bắt được lỗi mà vẫn im lặng. Nay là mức cố định "phóng riêng nút thêm 20%" (ra −9,6).
  ⚠️ Chụp ảnh bản cảm ứng phải đặt **`hud.epCamUng`**, không phải `CamUng.EpBat`: `GameHUD.Update` ghi đè
  `CamUng.EpBat = epCamUng` mỗi khung nên ảnh ra bản máy tính. Menu 78 kiểm.
- ⚠️ **BỘ BIỂU TƯỢNG KỸ NĂNG chỉ có MỘT BẢNG**: `IconKyNang.BoDayDu()` (xếp theo số hiệu, dài `CapDo.SoKyNang`).
  `GameHUD.BoIcon` và `ManSanh.IconXemTruoc` đều gọi nó. 19/09/2026 hai nơi còn giữ hai bản riêng, thêm nhóm BỊ ĐỘNG
  chỉ sửa bản của HUD → Sách phép ở **sảnh** hiện bốn ô TRỐNG TRƠN (người dùng báo). Thêm kỹ năng mới = thêm một dòng
  ở `BoDayDu`. Menu 66 mục B1b kiểm (cuộn hết cột rồi đo, vì phép đo cũ bỏ qua mọi hàng bị khuất — chính chỗ bốn kỹ
  năng Kháng nằm); ⚠️ 29/09/2026 (icon bị động vẽ lại, dồn nội dung vào giữa) mục B1b2 so với **Ô TRỐNG THẬT chụp cùng lượt**
  (tháo icon qua `ManSanh.DatIconXemTruocChoPhepThu`, đo cả hình): ô trống 0,117–0,123, icon ×1,85–2,46, ngưỡng ×1,3 — không so
  với độ sáng icon KHÁC nữa (icon hợp lệ có bố cục khác nhau).
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
- ⚠️ **Thiên thạch CẤP 5 rơi 5 quả** thay vì 3 (người dùng 19/09/2026, `ThienThach.SoQuaTheoCap`, cấp của NGƯỜI TUNG
  qua `capPhepDangTung` nên bản sao mạng cũng đúng). Menu 62 mục G.
  ⚠️ **Thiên thạch ĐỐT CHÁY BIA MỘ VÀ NHÀ MỒ Y HỆT CÂY** (người dùng 25/09/2026, chọn hai nhóm này — đá, hàng rào không): cùng
  `CayChay` (lửa lan theo điểm mồi, đen dần, cháy rụi mất hình + va chạm, 30 s mọc lại). Nhận ra bằng NHÓM CHA `BiaMo` / `NhaMo`
  (`CayChay.LaBiaNha`, `ChayDuoc`), không theo tên (452 bia có 40 kiểu lưới). Lưới Read/Write TẮT như cây → **điểm mồi lửa nướng
  sẵn** (menu 20 nay nướng 51 bộ: 9 cây + 40 bia + 2 nhà; bộ của cây giữ nguyên từng byte). Chung trần `ToiDaCungLuc` 4 với cây.
  Bia thấp nên `cao` kẹp tối thiểu 0,8 m thay vì 2,5. Menu 81 kiểm (tung thật: bia và nhà bắt lửa, 340 điểm, đen ×0,23,
  mất hình, mọc lại nguyên màu; đối chứng đá: 0 vật chạy được, 0 bộ điểm).
  ⚠️ **Khoảng chờ giữa hai quả 0,35 giây** (`ThienThach.GiayCachNhau`, người dùng chọn 25/09/2026, trước 0,7): 3 quả rơi xong trong
  0,7 s, cấp 5 trong 1,4 s. Chỉ kỹ năng người chơi (Quỷ dữ có nhịp riêng). Menu 62 mục G3 đo thời điểm chạm đất thật: TB 0,354 s.
- **Thiên thạch của người chơi đánh ngã 40% · 1,5 s** (`BiDanhNga.cs`): lật **model con** (va chạm ở gốc vẫn đứng), xoay
  quanh trục của GỐC (model Meshy xoay sẵn 180° — xoay trục riêng là ngã úp). Thiên thạch Quỷ dữ mặc định 0%. Bit
  `CoNga` trong byte hiệu ứng mạng — ⚠️ byte cờ gói tin nay dành **6 bit** (0x3F, từ Tàng hình 18/09/2026); gói người chơi đã dùng hết 8 bit (2 + 6) — thêm hiệu ứng thứ 7 phải nới byte hoặc dùng byte khác, ở CẢ gói người chơi (`VietTrangThai`/`DocTrangThai`) lẫn gói quái, không thì bit mới bị cắt im lặng (menu 63). Chữ nổi trên đầu dùng font Inter (`VeSoSatThuong`) — trước đây font mặc định, mất dấu trên web.
- **`FrozenEffect` có HAI đồng hồ**: `remaining` (vỏ băng + lớp chậm `slow`) và `dongCungConLai` (đóng cứng
  hoàn toàn). Mưa băng trúng là chắc chắn chậm 50% trong 2 s, và 35% số lần đóng cứng 1,5 s. Sấm sét: 35% choáng.
  Đừng suy "đóng cứng" từ `slow > 0,85` như bản cũ — hai tầng chồng nhau thì công thức ấy sai.
  Bắt đầu đóng cứng thì chữ **"ĐÓNG BĂNG!"** bay lên (`FrozenEffect.BaoChuDongBang`, gọi ở `Apply` và `HieuUngQuaMang.ApCo`); vỏ băng
  phủ cả **SkinnedMeshRenderer** (14/09/2026 — trước chỉ `MeshRenderer`, model có xương không có vỏ), dày khi đóng cứng, mỏng khi
  chỉ chậm. Menu 67 kiểm qua đường mạng thật.
- ⚠️ **CHỖ SÉT CHẠM ĐẤT CỦA SẤM SÉT + MÂY GIÔNG (+ LỐC XOÁY): TIA ĐIỆN BÒ TRÊN ĐẤT + VẾT NỨT PHÁT SÁNG** (người dùng 05/10/2026: "vỡ ra các hạt / dấu
  gạch sáng rất không tự nhiên"; duyệt mẫu A + vết nứt của mẫu B, chỉ hai kỹ năng này). `VfxFactory.LightningImpact` (CHỈ Sấm sét + Mây giông
  gọi — Lốc xoáy / Gió lốc gọi thẳng `LoeSetChamDat`, không đổi) tắt hai lớp hạt kéo dài `Sparks` / `Jet` của `Vfx_SetChamDat` (`LopVetGach`),
  thêm `Vfx/TiaBoDat.cs` (6 tia sét con kiểu `LightningArc` mặc định bò ngang mặt đất 0,6–1,05 bán kính + 3 tia đợt 2 sau 0,09 s bò tiếp,
  hai đầu cách mặt đất Ground 0,12 m) và `Vfx/VetNutSet.cs` (thay vết cháy xém 15% cũ: đĩa bám đất, ảnh Blender MCP
  `CongCu/Blender/vet_nut_set.blend` scene `VetNutSet` → `Resources/KyNang/SamSet/VetNutSet.png` + `VetNutSetQuang.png` 2×2; quầng trắng nóng →
  cam 0,5 s → đỏ sẫm 1,4 s → tắt 2,2 s, vết đen mờ 4–6 s; trần 12 vết, cách < 0,8 m thì làm mới vết cũ). Vòng sáng, quầng, cột sáng, bụi, đèn của
  prefab giữ. Menu 106 (0 lỗi): Sấm sét 19 chỗ chạm / Mây giông 26 — 0 lớp vệt gạch bật, 9 tia con mỗi chỗ, đầu tia lệch đất 0, xa nhất 1,72 m;
  màu vết nứt theo tuổi đúng; đối chứng `LoeSetChamDat` (đường Lốc xoáy) vẫn 2 lớp. Menu 58, 83: 0 lỗi.
  ⚠️ **Cùng ngày người dùng xin thêm cho LỐC XOÁY**: `TornadoBolt` (nhánh có lóe — Gió lốc `coLoe: false` không đổi, không tia con) gọi
  `TatVetGach` + `TiaBoDat.Tao(..., loc)` (tia con **BÁM THEO lốc** — `LightningArc.BamTheo`, đợt 2 dời theo quãng lốc đã đi) + `VetNutSet`
  (vết nứt nằm yên trên đất — ngoại lệ có chủ ý của luật "Lốc xoáy không để dấu vết trên mặt đất"); Hoá lốc xoáy cũng có. Tia con tên
  `TiaBoDat.TenTia` — phép thử đếm tia chính phải bỏ qua (menu 82 E1, menu 71 C5 đã sửa; C5 đếm lóe không cần lớp `Sparks`). Menu 106 (0 lỗi,
  cửa sổ đo từng kỹ năng tách riêng): Sấm sét 19 / Mây giông 36 / Lốc xoáy 14 chỗ chạm, đúng 9 tia con mỗi chỗ, 0 vệt gạch; tia con lệch so với
  lốc đổi 0,000 m khi lốc đi 0,56 m (đọc cuối khung); Gió lốc 0 tia con. Menu 71, 82: 0 lỗi.
- ⚠️ **Sấm sét hồi chiêu 4 giây** (28/09/2026 người dùng, trước 8): `PlayerController.boltCooldown` — số nằm ở BA chỗ: code,
  `Player_Sorceress.prefab`, nhân vật đặt sẵn trong Act2. **Mảnh vỡ khi hạt băng Mưa băng chạm đất to hơn 10%**
  (`VfxFactory.HeSoManhVoMuaBang` 1,1, `PhongManhVo` trong `FallingShard` — Quả cầu băng giữ cỡ cũ).
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
- ⚠️ **MẶT ĐẤT DÙNG ĐÈN SINH ĐÔI** (`Vfx/DenMatDat.cs`, 28/09/2026 — người dùng: "ô vuông ô chữ nhật chắp vá dưới đất mỗi khi
  kỹ năng phát sáng, khắp bản đồ"): Terrain vẽ thành nhiều MẢNH, Unity chia đèn theo từng mảnh; đèn vượt `pixelLightCount`
  (Cao 2 · TB 1 · **Yếu/Rất yếu 0**) bị hạ xuống đèn đỉnh/SH tính MỘT LẦN cho cả mảnh → mảnh sáng đều = ô vuông. Nay mọi đèn
  điểm/chiếu có một con `DenMatDat` **ForcePixel** chỉ chiếu lớp **Ground** (chỉ có Terrain), đèn gốc bỏ Ground; tạo ngay trong
  `Camera.onPreCull` nên vụ nổ không lộ ô khung nào; chép màu/độ sáng/tầm mỗi khung. ⚠️ Đã thử nâng `pixelLightCount` lên 8:
  hết ô nhưng mức Yếu 455 → 922 SetPass (mọi bia/cây thành đèn điểm ảnh) — KHÔNG dùng; đèn sinh đôi: 457 → 607. Số suất mỗi mức
  nay đặt rõ ở `CaiDatDoHoa.DenDiemAnhTheoMuc`. Đếm đèn con trong phép thử nhớ là có thêm đèn `DenMatDat`. Menu 89 kiểm (3 điểm
  cố định × 4 mức × 1/6 đèn, đối chứng tắt đèn sinh đôi, đo "đường nối" trên D/A để khử vân đất).
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
  `firebase hosting:clone acquytrolai@<version> acquytrolai:live` (trang game từ 10/10/2026; trước là `diablo25d-game`).
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
  ⚠️ **06/10/2026: CHỮ MÁU ĐẶC 3D** (người dùng duyệt sau 7 bản: "mềm mại, ghê rợn, máu me hơn", "3D thêm", giọt máu ngắn đều cả 4
  chữ + giọt rơi lơ lửng, máu bắn trên QUỶ và TRỞ) — dựng bằng **Blender MCP** `CongCu/Blender/tieu_de_2_ban.blend` (scene `TieuDe2B`,
  bộ sưu tập `TD2_B`; hàm chung `CongCu/Blender/tieu_de_mau_chung.py`), font **Playfair Display SC Black** (`CongCu/Fonts`, OFL): chữ
  dày 0,13 m + giọt máu NURBS gộp bằng voxel remesh + smooth, vật liệu máu bóng có cục đông, máy quay phối cảnh nghiêng xuống 10,4°
  (thấy mặt trên khối chữ), compositor: quầng đỏ + **bóng tối ôm sát viền chữ** (KHÔNG mảng khói phía sau — người dùng: che nền trong
  game). Ảnh cắt sát nội dung 1620×443. ⚠️ Nhìn chếch xuống làm dấu chấm dưới Ạ dính chân chữ A ("LAI") → đã dời 3 mảnh dấu xuống
  0,08 m. Bản cũ Grenze Gotisch `CongCu/TieuDe/sinh_tieu_de.py` **tự chặn** (chỉ chạy với `--cu`) vì chạy là đè ảnh mới. Gắn `?v=<md5>`
  vào trang bằng tay khi đổi ảnh. **Trang Loading có nền địa ngục**: `TemplateData/nen_dianguc.jpg` = ảnh chụp vực dung nham Act2 trong
  game (máy quay (−25, −8, −88) nhìn (30, −24, −86)); CSS phủ kín + tối vừa ở giữa; **quầng tối sau chữ là NỀN của thẻ `#ten-game`**
  (đi theo chữ ở mọi cỡ màn — đặt theo % trang thì màn dọc điện thoại lệch khỏi chữ). ⚠️ 09/10/2026 chữ trên trang Loading **lên 4vh**
  (`#ten-game { position: relative; top: -4vh }` — chỉ chữ dời, thanh tải giữ chỗ; người dùng "lên phía trên 1 chút", chọn ~4%): đo 1000×462 khung
  chữ y 54 → 36, thanh tải giữ y 284; 844×390 ảnh chữ từ y 21, 390×844 từ y 270 — không tràn mép trên.
  ⚠️ **09/10/2026: THANH TẢI = "VÒNG TRIỆU HỒI"** (người dùng chê thanh cũ "quá cứng"; chọn qua 3 vòng ảnh xem trước trong `CongCu/MauThanhTai/`:
  mẫu 4 vòng triệu hồi → biến thể **4A Cổ tự khắc đá** → chữ giữa **"Hồn ma thì thầm"** nhưng **màu đá bia mộ + vết máu**): SVG vẽ bằng JS trong
  template (`#vong`, `veVong`, gọi từ `datTienDo`) — vòng máu cháy theo %, 24 rune khắc nét kép sáng dần + quay chậm, 72 vạch chia độ quay ngược,
  ngôi sao sáng từng cạnh, 5 chấm phép; số % + "ĐANG TRIỆU HỒI" font **Cormorant SC Bold** (`TemplateData/CormorantSC-Bold.ttf`, đủ 134 chữ có dấu, OFL
  kèm), mặt chữ màu đá lấy mẫu từ ảnh tên game (#6e695e / #85836f / #a09d86) + hạt sần (cùng ngày người dùng **bỏ vết máu loang trên chữ + số**,
  đã xoá các lớp lọc ấy; sau đó bỏ luôn 3 vệt máu nhỏ giọt dưới chân số); **vòng canh GIỮA đáy ảnh tên game và dòng tác giả** bằng JS
  (`canhVong`: đo lại khi ảnh / font nạp và khi đổi cỡ màn, dịch `transform`) — đo 932×430 / 390×844 / 1000×640: khoảng trên/dưới 23/23, 30/31, 31/32;
  số % canh vào TÂM KHUNG NGÔI SAO (10/10/2026 người dùng: cỡ 42 → 36, dòng chân 10 → 3,2 — `SO_CO`/`SO_Y`; tâm chữ số đo lệch tâm sao 0,17 đơn vị, cũ 5,35; theo chiều ngang CHỈ PHẦN CHỮ SỐ nằm đúng trục sao, dấu % tràn sang phải — người dùng chọn, `canhChuSo` đo mực chữ số bằng canvas, đo ảnh 0 / 18 / 100%: lệch ≤ 0,5 px); 5 nét sao sáng ở 0 / 20 / 40 / 60 / 80% (nét ngang trước chỉ sáng khi qua 90%, người dùng chọn "đủ 5 nét sớm hơn"), hai bóng ma lệch rung, dòng chữ vặn như khói. ⚠️ Bộ lọc SVG phải `color-interpolation-filters="sRGB"` — mặc định
  linearRGB đẩy đỏ máu thành HỒNG. Màn thấp (≤ 520) tên game co về 105vh rộng cho vòng vừa màn. Đo (máy chủ tĩnh `web/`): 1000×640 vòng y 273–555,
  844×390 vòng 172 px y 162–333 + tác giả ≤ 360, 390×844 vòng 289 px — không tràn; console 0 lỗi; font nạp được.
  ⚠️ **Cùng ngày (sau): MẶT CHỮ = ĐÁ BIA MỘ** cho CẢ trang Loading lẫn màn trong game (người dùng: "màu giống bia mộ trong game, sần sùi,
  nhiều vết nứt, nhiều vết máu; giọt máu / giọt rơi / cụm máu giữ nguyên"; rồi "sáng hơn một chút"; rồi "giống vậy cho màn trong game"):
  vật liệu `TD2C_DaMo` (`CongCu/Blender/tieu_de_vat_lieu_bia.py`) dùng CHÍNH ảnh `DaMo_Mau/Gan/MatNa.png` của vật liệu `Act2_DaBia`
  (chiếu hộp, sắc ×0,86), rêu / mốc nhẹ, vết nứt Voronoi đen, vết máu khô (kênh B mặt nạ + vệt chảy từ đỉnh + máu đọng trong vết nứt);
  thuộc tính đỉnh `LaDa` (khoảng cách tới lưới chữ gốc < 6–16 mm) tách đá / giọt máu → giọt vẫn là máu bóng. Đèn đỏ hạ (viền 260, đỏ
  50), đèn chính 1350; ⚠️ thêm đèn chính diện làm hạt máu bắn lóe trắng như kim tuyến — bỏ. Độ sáng TB phần chữ 63 → 82. ⚠️ **Mảng TRẮNG loang lổ** trên chữ
  (người dùng khoanh): KHÔNG phải do sáng / bóng / phân loại đá-máu (đã thử cả ba, số điểm trắng không đổi) mà do **ALPHA của `DaMo_Mau.png`**
  — 92% điểm ảnh alpha 0 (dữ liệu riêng của shader game), Blender coi là trong suốt và làm hỏng màu ở đó → ảnh phải `alpha_mode =
  'CHANNEL_PACKED'`. Đọc đúng thì đá bia thật SÁNG (màu xám xanh trước đó là màu hỏng) → sắc 0,27 + chặn trần 0,66, rêu 0,18, mốc 0. Điểm
  trắng 2,9% → 1,6% (còn lại là ánh trên cạnh vát), sáng TB 97. Chữ trên trang Loading dùng Inter nhúng kèm (`TemplateData/Inter-Regular.ttf`).
  **Không đổi `productName`** trong Unity (vẫn "Diablo 2.5D"): cache dữ liệu và chỗ lưu của người chơi tính theo nó.
- ⚠️ **08/10/2026 MÀN ĐĂNG NHẬP / SẢNH / PHÒNG** (người dùng, ảnh đánh dấu): ảnh tên game lên gần mép trên (`ManDangNhap.TamTieuDe`
  0,153 — 0,11 người dùng thấy "hơi cao quá", khung đăng nhập giữ chỗ); lòng khung + hàng + ghế trống + nền ô nhập **đục bớt 40%** (`GiaoDien.DoDucKhungSanh` 0,33 /
  `DoDucHangSanh` 0,27 — chỉ ba màn này, Sách phép / Cài đặt / HUD giữ hằng cũ); đếm ngược 5 giây, vòng + số **×0,6292** (0,52 → ×1,1 = 0,572 →
  09/10/2026 ×1,1 nữa) ở 0,204 chiều cao (`ManSanh.TamDemNguoc` — sát gạch đỏ nên không lên được nữa); ⚠️ đứng yên mép dưới CHẠM chóp mũ ~10 điểm
  (người dùng xem ảnh, chọn "giữ như ảnh"), đập nhịp to nhất vẫn trên trán. Menu 50 đo trên ảnh: cảnh lọt qua 0,66–0,72 (cũ ~0,45), tên game tâm 0,164.
- ⚠️ **MÁY BOT — ĐANG LÀM TỪNG BƯỚC** (người dùng 08/10/2026; chọn độ khó khi thêm · mỗi BOT một hệ chính ngẫu nhiên · tính như người
  khi kết trận, không lưu thành tích · báo sau mỗi bước): **bước 1 XONG** — chủ phòng bấm ghế trống → bảng DỄ / THƯỜNG / KHÓ; BOT là ghế
  `phong/{mã}/nguoiChoi/bot_{d|t|k}_xxxxxxxx` (`Mang/MayBot.cs`, `PhongMang.ThemBot`; độ khó nằm trong uid vì luật chỉ cho 5 trường — không đổi
  luật). Menu 112 (Firebase thật): 0 lỗi. **Bước 2 XONG** — BOT là nhân vật THẬT trên máy CHỦ PHÒNG (`KhoiDongTranMang.SinhBot`,
  `BotDieuKhien` đặt `pc.input`), máy khách thấy như một ghế (trạng thái chung gói chủ phòng, phép `chiSo` = ghế BOT, chủ phòng gửi gói chết thay
  BOT); quái tính BOT như người (người dùng chọn). ⚠️ **`CapDo` đã TÁCH**: trạng thái ở lớp `BangCap`, `CapDo.CuaMay` = nhân vật máy này (hàm
  tĩnh cũ vẫn dùng được); chỗ nào có thể là BOT phải đọc **`PlayerController.Cap`** (kể cả kỹ năng / hiệu ứng mới). Chỗ xuất phát tính MỘT LẦN
  (`ViTriXuatPhat` — `ChoXuatPhat` kiểm vật cản bằng vật lý, tính lại thì trượt). Menu 113: 0 lỗi. **Bước 3 XONG** — `Mang/BanDoBot.cs`: lưới ô 0,8 m
  (đất Ground ≥ −4 m, không chạm vật cản Default từ 0,6 m, cấm vành 1,6 m sát vực) dựng một lần lúc vào trận + A\* + làm thẳng; `BotDieuKhien`:
  mục tiêu gần nhất trong 30 m / săn xa / đi tuần, dừng cách 9 m, dò vực phía trước, chống kẹt (cấm tạm ô). Menu 114: 0 lỗi (nay TẮT đánh — trận 60 s có
  BOT tung phép từng làm GPU timeout, Unity tắt). **Bước 4 XONG** (09/10/2026) — `MayBot.TieuDiem` cộng điểm theo hệ đúng đường mở khoá;
  `BotDieuKhien.ChonKyNang`: Khiên khi máu < 60%, liên hoàn theo hệ (Dễ 30 / Thường 75 / Khó 100%), chiêu cơ bản cần thấy mục tiêu; ngắm
  lệch 2,5 / 1,0 / 0,3 m + đón đầu (bỏ vận tốc > 15 m/s); uống bình máu / mana, đi nhặt bình (`QuanLyBinhRoi.XetBotNhat`), lùi khi bị áp sát.
  Menu 115: 0 lỗi. ⚠️ BOT trên máy khách CHƯA thử hai máy thật (cần bản web).
  ⚠️ **09/10/2026: BOT ĐẦU TRẬN ĐI GIẾT QUÁI, CẤP 7 MỚI SĂN NGƯỜI** (người dùng; chọn 20 m · đánh trả · cấp 7 quái áp sát thì giết trước · hết quái
  thì đi tuần): `BotDieuKhien.ChonMucTieu` — đối thủ (người / BOT khác) trong `TamGiaoTranh` **20 m** thì giao tranh ngay (mọi cấp); dưới cấp
  `CapSanNguoi` 7: bị người chơi đánh trúng trong `GiayDanhTra` 5 s thì đánh trả kẻ ấy dù xa (`Damageable.nguoiChoiDanhCuoi` / `lucNguoiChoiDanh`,
  ghi trong `TakeDamage` sau phần bỏ đồng đội), không thì quái gần nhất cả bản đồ, hết quái đi tuần; từ cấp 7: quái trong 20 m giết trước, rồi săn đối
  thủ gần nhất cả bản đồ, không còn đối thủ thì quái. `TamPhatHien` 30 m đã bỏ. Menu 114 mục D (0 lỗi): cấp 1 đối thủ 44,7 m không nhắm, đứng yên
  6 s; bị bắn từ 44,9 m → nhắm sau 0,27 s, 6 s sau còn 19,9 m; cấp 7 → còn 7,1 m; cấp 1 quái 33 m vs người 45 m → quái; cấp 7 quái 10 m → quái,
  quái 33 m → người. Menu 115: 0 lỗi.
- ⚠️ **TẦM NHÌN 25 m + SƯƠNG CHIẾN TRANH KIỂU STARCRAFT 2** (10/10/2026, người dùng: "kéo camera xa nhìn từ đầu map tới cuối map — chỉ
  tầm 25 m, vượt quá có sương mù bao phủ"; chọn kiểu SC2 = cảnh vật ngoài tầm vẫn thấy tối mờ, KHÔNG thấy quái / người / BOT / hiệu ứng; Đôi
  chia sẻ tầm nhìn đồng đội; chết giữ 25 m quanh xác; BOT cũng chỉ thấy 25 m, không thấy ai thì đi tuần khắp bản đồ). `Art/TamNhin.cs` (gắn
  trong `GameBootstrap.Awake`): nguồn nhìn = nhân vật máy này + đồng đội còn sống → `_TN_Tam[4]` toàn cục; hàm `Assets/Shaders/SuongChienTranh.cginc`
  (`TN_Ap`: tối ×0,32, nhạt màu 75%, mây xám xanh trôi, mép mờ 3 m) gắn vào DaMo / vỏ cây / nước / vực địa ngục / dung nham / khói vực +
  shader MỚI `ChuanSuong` (bản sao vật liệu Standard của cảnh lúc chạy — cỏ, lá, lò, sắt) + `SuongDat` (tấm lưới bám địa hình, KHÔNG sửa shader
  địa hình Unity). Không thêm lượt vẽ nào (không có ảnh độ sâu — thêm vào là vẽ lại cả cảnh). Ẩn ngoài tầm bằng `Renderer.forceRenderingOff`:
  mọi renderer sinh sau khi vào trận + renderer thuộc Damageable (trừ mình / đồng đội) + hạt / vệt / đồ trong suốt của cảnh (lửa lò, tàn lửa);
  đèn → `cullingMask = 0`; tên trên đầu / số sát thương / dấu ngã hỏi `TamNhin.ThayDuoc`. Có `ChayThuMang` thì TẮT phần hình (ảnh menu cũ không
  đổi) trừ `TamNhin.BatTrongPhepThu`; logic BOT (`TamNhin.NhinThay`) luôn bật. ⚠️ **Surface shader có `finalcolor` thì Unity KHÔNG tự sinh sương
  khoảng cách** — phải `#pragma multi_compile_fog` + `TN_SuongUnity` (bản đầu thiếu: DaMo mất hẳn sương cũ, menu 117 C bắt được). Thêm shader cảnh
  vật mới thì gọi `TN_SuongUnity` + `TN_Ap`. Menu 117 (0 lỗi): ảnh trên xuống — trong 6–20 m ×1,00, ngoài 31–45 m địa hình ×0,28 / vật ×0,33; sương
  cũ k 0,860 (Standard 0,843, lý thuyết 0,838); quái 20 / 30 / 60 m hiện / ẩn / ẩn, lửa lò 74 m ẩn; đồng đội 40 m → thấy quái cạnh đó; chết giữ
  quanh xác; một lượt quét 0,40 ms / 1 182 renderer (5 lượt/giây). Menu 114 D (0 lỗi): đối thủ 45 m không biết, bắn từ 23 m → đánh trả, cấp 7 săn
  23 m; đi tuần 45 s: 230 m, +15/64 ô. Menu 113, 115: 0 lỗi.
- ⚠️ **NHÀO LỘN — KỸ NĂNG 22, NHÓM HỖ TRỢ** (10/10/2026, người dùng; `Skills/NhaoLon.cs`): **có sẵn cấp 1 từ đầu trận như hai bình, chỉ MỘT cấp**
  (`CapDo.LaKyCoSan`, `CapToiDaCua` = 1; Sách phép ghi "CÓ SẴN — chỉ một cấp"). Người dùng chọn: **0 năng lượng**, hồi chiêu **4 s**, lăn **TỚI chỗ ngắm
  tối đa 5 m** (tối thiểu 0,8; chạm nhanh trên cảm ứng = đủ 5 m về hướng cần / mặt), **KHÔNG miễn sát thương**, vật cản chặn lại (không xuyên như Tốc
  biến; lăn qua mép vực là rơi), **BOT dùng** khi bị áp sát < 4 m (mỗi giây xét một lần, Dễ 20% / Thường 50% / Khó 85%, ô 2,5 + 5 m phải đi được —
  `BotDieuKhien.NhaoLonNeuApSat`). Đang lăn: không tung phép khác ("Đang nhào lộn!"), bình vẫn uống; bị choáng / đóng băng / ngã / hất tung / lốc xoáy
  thì không lăn và đang lăn thì dừng. Di chuyển trong `PlayerController.HandleMovement` (`NhaoLon.BuocDi`: quãng theo `TiLeQuang` lao nhanh đầu,
  chậm dần cuối; thời gian 0,34 + 0,052 × m — 5 m = 0,6 s). HÌNH (mọi máy, bản sao chỉ hình — vị trí từ gói tin): cuộn tròn thân (ngắm hướng xương
  trong khung GỐC như Giựt sét), lật model con 360° quanh tâm quả bóng (smootherstep), chạm đất theo **bảng khe đo bằng BakeMesh** `NhaoLon.BangKhe`
  (áo choàng thò dưới xương 7–29 cm tuỳ góc — khe cố định thì lún 9 cm / hở 14 cm). ⚠️ BiDanhNga / BiHatTung / XacNam gắn GIỮA vòng lăn phải lấy tư
  thế gốc qua `NhaoLon.LayGoc` (không thì chụp nhầm tư thế đang lộn ngược). Icon Blender MCP `CongCu/Blender/nhao_lon.blend` (scene `NhaoLon`, chính
  model phù thuỷ tạo dáng bằng bpy + 3 bóng ma tím + mũi tên máu cong) → `Resources/Icons/NhaoLon.png`. Menu 119 (0 lỗi): lăn 5,00 / 2,00 / 5,00 / 0,80
  m khi ngắm 5 / 2 / 9 / 0,3 m, 0,60 s, mana không giảm, bấm lại bị hồi chiêu chặn; 35 khung (60 khung/giây cố định) góc lật 358° bước lớn nhất
  22° / TB 10,5°; chạm đất −0,011 … +0,024 m; hộp chặn ở 2,5 m → dừng 1,80 m; bị ngã giữa vòng → model về đúng gốc; bản sao lăn hình, tự đi 0 m.
  Menu 115 F2: BOT Khó bị áp sát 2,4 m → lăn sau 1,2 s ra 7,4 m. Menu 59, 60, 66, 77, 80: 0 lỗi (60 / 66 nay đếm `LaKyCoSan`).
- ⚠️ **NÚT THOÁT TRẬN + BẢNG XÁC NHẬN** (10/10/2026, người dùng; `UI/GameHUDThoatTran.cs`): cột góc phải trên = **THOÁT TRẬN ở đúng góc**
  (62, 62), con mắt + Sách phép dời xuống mỗi nút 88 đơn vị (`GameHUD.KhoangCotGoc`; bản máy tính: thoát → Sách phép). Icon Blender MCP
  `CongCu/Blender/nut_thoat_tran.blend` (scene `NutThoat`) → `Resources/GiaoDien/ThoatTran.png`: vòm đá nứt rêu + vết máu, cánh cửa gỗ mục đai sắt
  đinh tán hé mở, lòng hầm ánh đỏ + đôi mắt vàng, sương tràn, mũi tên máu nhỏ giọt chỉ ra ngoài. Bấm → bảng "THOÁT TRẬN?" (THOÁT TRẬN / Ở LẠI),
  đồng ý = `GameDirector.BackToMenu` (như ESC cũ). Người dùng chọn: máy tính cũng có nút + **ESC cũng hỏi** (ESC lần nữa = ở lại; đã chết / hết trận thì
  ESC về sảnh ngay như cũ — `GameHUD.XuLyEsc`, GameDirector chỉ tự xử ESC khi không có HUD); **chủ phòng** còn người khác trong trận thì bảng thêm
  dòng "thoát sẽ kết thúc trận của mọi người". Bảng mở = `GameHUD.KhoaInputTran` (như Sách phép: DocInput gói rỗng, nút / thanh kỹ năng / "+"
  thôi nhận bấm, `GUI.depth` −50). Menu 118 (0 lỗi): cột nút cách tâm 88 đơn vị > 2r, ảnh cửa hầm 8,6% điểm đỏ-cam trong vòng nút (đối chứng 0),
  bảng mở: nền ×0,38, chữ đỏ giữa màn 9 → 1 385 điểm, cần đẩy mà hướng đi 0 (đối chứng 1,00), 0 chữ bị cắt; Ở LẠI → chạy lại; THOÁT → MainMenu.
  Menu 59, 66 (con mắt đo ở chỗ mới): 0 lỗi. ⚠️ Cảnh báo chủ phòng CHƯA thử hai máy thật.
- ⚠️ **MÀU ÁO TỪNG NGƯỜI CHƠI** (09/10/2026, người dùng): `Player_PhuThuy.mat` dùng shader `Diablo25D/PhuThuyDoiMau` (thay Standard) —
  vùng áo TÍM của ảnh (sắc độ 277–324°) thay bằng `_MauAo` giữ sáng tối, `_DoSangAo` 0,72 (người dùng chọn sáng hơn ~20%); `_MauAo.a` 0
  = áo gốc. Màu qua MaterialPropertyBlock (`MauAoNhanVat`), gắn ở `KhoiDongTranMang.GanDoi(..., ghế)`: Đơn theo ghế (6 màu), Đôi A lam / B đỏ;
  0 byte mạng. Menu 116: 0 lỗi. ⚠️ Thêm hiệu ứng dùng `SetPropertyBlock` lên nhân vật thì GET trước rồi mới SET, không thì xoá mất màu áo.
- **Màn chính (MainMenu.unity) dựng từ cảnh Act2** bằng menu 51 — đừng sửa tay trong scene, chạy lại menu.
  ⚠️ **04/10/2026: PHÙ THUỶ TRƯNG BÀY NGỬA HAI TAY NÂNG QUẢ CẦU LỬA + QUẢ CẦU BĂNG** (người dùng; chọn lửa bên PHẢI màn hình lúc quay mặt về
  máy quay, quả cầu y trong trận thu nhỏ, bàn tay ngang bụng chìa sang hai bên): `UI/TuTheTrungBay.cs` gắn LÚC CHẠY từ `MainMenuUI.Start`
  (không sửa scene), chạy sau `NguoiChoiHoatHinh` (thứ tự 10020): ngắm hướng cánh tay bằng `NguoiChoiHoatHinh.NgamHuongKhop` (như Giựt sét),
  xoắn cẳng tay + bẻ cổ tay cho lòng bàn tay ngửa. ⚠️ Khung xương Meshy KHÔNG có xương ngón, lưới Read/Write tắt: pháp tuyến lòng bàn tay
  đo bằng **menu 101** (xoay xương Hand, BakeMesh hai lần, PCA các đỉnh đi theo — ngón tay = +Y cục bộ) và chọn dấu bằng ảnh **101b** (phía
  có NHẪN là mu tay): `LongTayTrai (0,72 0,14 0,68)`, `LongTayPhai (−0,75 0,20 0,62)`. Quả cầu: `BuildFireballVisual` / `BuildQuaCauBangVisual`
  r = 0,13, đèn kẹp 2,6 m / 2,2 (để nguyên 6 / 12 m thì nhuộm cam cả nền); tàn lửa + lửa lõi = NGỌN LỬA THẬT (xem mục Quả cầu lửa).
  ⚠️ **Lửa trên tay BÁM QUANH quả cầu** (người dùng 04/10/2026: "vệt lửa phải bao quanh quả cầu, không bay lơ lửng"; chọn chỉ màn chính — trong
  trận giữ đuôi lửa khi bay): `Sparks` + `Flames` của quả cầu trên tay mô phỏng CỤC BỘ, không vận tốc / bốc lên / nhiễu, gốc lửa trên mặt cầu
  0,85 r (`BanKinhGocLua`). Menu 101c: 19 ngọn lửa, gốc xa tâm nhất 0,85 r, tốc độ 0.
  ⚠️ **+ LỬA GIỮA TÂM + KHÓI ĐEN** (người dùng 04/10/2026: "lửa cháy qua 2 bên nhiều quá — giữ lửa 2 bên, tăng lửa ở giữa tâm"; "thêm 1 ít khói
  đen bay lên"): lớp `LuaGiua` nhân bản lớp `Flames` đã chỉnh, gốc lửa trong lòng cầu 0,3 r (`BanKinhLuaGiua`), 26 ngọn/giây; `KhoiDenBocLen`
  (`TuTheTrungBay.TaoKhoiDen`: flipbook KhoiCuon xám đen, sinh ở đỉnh cầu, THẾ GIỚI, bốc 0,44–0,69 m/s, 9 hạt/giây, nở ×3,2). Menu 101c: 8 ngọn
  giữa gốc ≤ 0,30 r; khói 15 làn, 15/15 trên quả cầu và đang bốc lên, cao tới 0,87 m, độ sáng màu 0,10 — 0 lỗi.
  ⚠️ **05/10/2026: ĐỨNG YÊN, NHÌN THẲNG MÁY QUAY, HAI CHÂN BẰNG NHAU CHẠM ĐẤT** (người dùng: "không lơ lửng", "không cần tự quay"): bỏ xoay
  18°/s (`MainMenuUI.spinSpeed` đã xoá); `TuTheTrungBay` quay mặt vào `Camera.main`, mỗi khung đặt xương chân về góc **bind pose** của model
  (`GocChanBind` — ghi CỨNG, lưới Read/Write tắt nên bản build có thể không đọc được `bindposes`), xoay CẢ CHÂN quanh hông khép bớt chữ A
  (`HeSoKhepChan` 0,4 → cổ chân cách 0,33 m như bản cũ 0,34), bàn chân phẳng; khung đầu BakeMesh một lần rồi hạ gốc cho bên hở nhất vừa chạm đất
  (đất dốc: bên kia lún 2,8 cm). ⚠️ **ĐỪNG ngắm riêng đùi + cẳng chân thẳng xuống**: xương Meshy ở bind pose đã GẬP GỐI 48,6° / 46,1° trong khi
  LƯỚI chân thẳng — bẻ xương thẳng thì lưới chân CONG ra hai bên (bản đầu cùng ngày, người dùng: "2 chân quá cong"; menu 101c khi ấy kiểm "gối 0°"
  nên vẫn xanh). Ảnh so 5 cách: `PlayTestShots/manchinh_chan_cac_muc.png` (vẽ từ lưới BakeMesh — SkinnedMesh vẽ nhiều tư thế trong CÙNG lệnh ra
  y hệt nhau). Trước: menu 51 đặt gốc cao 0,2 m, lệch 20°, tư thế bước dở → đế giày hở 0,15–0,16 m. Menu 51 nay đặt sát đất, nhìn thẳng. Menu 101c
  đo (đối chứng = tư thế cũ trong scene): lệch 0,0° (cũ 20), hở đất 0,000 / −0,028 m (cũ 0,163 / 0,152), cổ chân lệch cao 0,006 (cũ 0,079), gối =
  góc bind đọc từ `Mesh.bindposes` (48,6 / 46,1; cũ 75 / 53), đế giày chạm đất dài 0,33 / 0,33 m (cũ 0,00 / 0,33), 2 s sau quay thêm 0,00°.
  Menu 101c (0 lỗi): lòng tay · lên = 1,000 cả hai,
  tay cao 1,06–1,07 m (hông 0,90, ngực 1,22), ra trước 0,41–0,43, ra ngoài 0,37; lửa x 0,56 / băng x 0,44 màn hình; cầu cách lòng tay 0,19–0,20 m.
  ⚠️ Mặt đất Màn chính là file RIÊNG `Terrain/ManChinh_MatDat.asset` (bản Act2 trước khi mở rộng, 27/09/2026) — trước đó dùng chung
  `Act2_MatDat.asset` nên menu 86 làm Màn chính mất đất. Chạy lại menu 51 thì nó lại dùng chung — sửa Act2 phải kiểm cả Màn chính.
  ⚠️ Dự án TẮT Domain Reload khi vào Play: biến tĩnh giữ giá trị lần Play trước. Cờ lửa `LoLuaDa.ChoPhepNhomLua` nay đặt lại khi
  vào Play và khi rời Act2 (`ChuyenChieuSangDem.OnDestroy`) — trước đó chơi Act2 sang ngày rồi về Màn chính là hai lò nguội.
  Lò đá: `Assets/Models/LoLuaDa` (FBX + texture nướng từ `CongCu/Blender/lo_lua_da.py`, chạy nền — từ nay
  dựng/thiết kế phải qua Blender MCP). Lửa + khói đen: mô phỏng Mantaflow trong Blender qua MCP
  (`CongCu/Blender/lua_lo_da.blend`) → flipbook `LuaLo` / `KhoiDen`, ba tấm đứng yên mỗi lò, dựng lúc chạy
  bởi `LoLuaDa` → `VfxFactory.LuaLoDa`. ⚠️ `VerticalBillboard` vẽ tứ giác **0,707×** kích thước đặt — đã bù √2.
- **Act2 có 10 lò lửa đá** (+4 ở vành mới do menu 86 = 14) (cùng prefab `LoLuaDa`, nhóm gốc `LoLua_Act2`, có va chạm) do **menu 54** đặt: lò giữa = chỗ
  đất khô gần tâm nhất (tâm là hồ nước), không dưới nước / trong nhà mồ / sát bia — đổi luật hay dựng lại Act2 thì chạy
  lại menu 54 rồi 54b, đừng kéo tay. **Lốc xoáy dập tắt lửa rồi cuốn cả lò đi, 30 giây sau lò mọc lại và cháy
  tiếp** (menu 54c) — `LoLuaDa.DapTat/Chay`, ngoại lệ duy nhất của luật "vật có hệ hạt thì không cuốn".
- Việc gần đây nhất (xem mục tương ứng trong `HUONG-DAN.md`):
  Thiên thạch **đốt cháy cả cái cây** (lửa lan theo bề mặt thật, cây rụi rồi mọc lại sau 30 s) ·
  Lốc xoáy trả cảnh vật về sau **30 s** thay vì 60 s ·
  **nút con mắt** khoá góc nhìn trên bản cảm ứng.
- **Nhiều người chơi — giai đoạn 1 xong** (mục "Nhiều người chơi, giai đoạn 1" trong
  `HUONG-DAN.md`). Đăng ký, đăng nhập, tạo/vào phòng, sẵn sàng, đếm ngược **5 giây** (08/10/2026, trước 10) — **tất cả
  nằm trong game Unity**, vẽ bằng OnGUI, gọi Firebase qua **REST** (Firebase Unity SDK không
  chạy trên WebGL). Trang web **chỉ để admin quản lý tài khoản**.
  Dự án Firebase `diablo25d-game` (asia-southeast1). Menu 26, 27, 28 chạy thật, **0 lỗi**.
- **Chơi được trên trình duyệt**: ⚠️ **https://acquytrolai.web.app** (10/10/2026 người dùng đổi địa chỉ; trước https://diablo25d-game.web.app) (bản WebGL, menu 29 —
  build 3–11 phút, lần đầu người chơi tải **172,5 MB**, các lần sau 0 byte và vào game ~10 s, 0 lỗi console). Trang quản trị chuyển
  sang https://acquytrolai.web.app/quantri/ . ⚠️ **HAI TRANG HOSTING trong cùng dự án `diablo25d-game`** (`firebase.json` là MẢNG, `.firebaserc`
  targets): `game` = site `acquytrolai` (thư mục `web/`, mọi header cache cũ), `cu` = site `diablo25d-game` (thư mục `web-chuyen-huong/`) **chuyển
  hướng 301 mọi đường dẫn + tham số** sang địa chỉ mới (luật riêng cho "/" — mẫu `/:duong*` không khớp gốc). `firebase deploy --only hosting`
  đẩy CẢ HAI; chỉ game: `--only hosting:game`. Đổi tên miền = khác origin → người chơi tải lại 172,5 MB một lần, đăng nhập lại, mất cài đặt
  localStorage (đồ hoạ, ô kỹ năng). Firebase Auth / Database dùng chung (cùng dự án). Mã nguồn ở
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
