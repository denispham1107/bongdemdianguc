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
   Combat/         Damageable + CombatUtil, cac hieu ung trang thai
   Vfx/            VfxFactory (3 500 dong, partial) + cac component hieu ung
   Art/            ProcMesh, WorldFactory, TextureFactory, MaterialLibrary (Mats)
   Cam/            CameraRig
   UI/             GameHUD (OnGUI), CamUng, ChiBaoNgam, MayNgam,
                   ManDangNhap, ManSanh (dang nhap + sanh phong, deu OnGUI)
   Mang/           FirebaseMang (cau REST), HoSoMang, PhongMang, TranHienTai,
                   KenhTrucTiep (WebRTC), BatTay, DuDoan, TrangThaiNhanVat
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
Blender MCP **không render được** (ảnh ra rỗng) — phải chạy `blender -b -P`.

> **Menu 3 (Self Test) THAY scene đang mở** bằng một scene thử đầy `TEST_Vfx`, `Fireball`,
> `Tornado`. Nó không lưu nên đĩa vẫn sạch, nhưng chạy xong phải
> `EditorSceneManager.OpenScene("Assets/Scenes/Act2.unity", Single)` trả lại ngay — không thì
> người dùng quay lại Unity thấy màn của mình biến mất.

**Menu `Diablo 2.5D`** trong Unity (29 mục): 1 nướng asset + dựng màn, 3 tự kiểm tra,
4 chạy thử & chụp hình, 14 chống ô vuông đen, 15–17 ảnh vỏ cây / đá mộ / sứt mẻ bia,
18–18d chạy thử cây cháy, 19 hồi sinh sau lốc, 20 nướng điểm mồi lửa, 21 nút khoá góc nhìn,
22 thanh kỹ năng bản PC, **26 chạy thử mạng (sảnh phòng), 27 chạy thử khoá tài khoản, 28 chụp màn đăng nhập·sảnh, 29 xuất bản WebGL, 30/30b bơm input Act2·Act1 (bước 1), 31 dự đoán & hiệu chỉnh (bước 2)**.
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
- Bảy kỹ năng chạy được: Cầu lửa, Mưa băng, Sấm sét, Lốc xoáy, Thiên thạch, Khiên, Giựt sét.
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
  build 11,3 phút, **169 MB** người chơi phải tải, 0 lỗi console). Trang quản trị chuyển
  sang https://diablo25d-game.web.app/quantri/ . Mã nguồn ở
  https://github.com/denispham1107/bongdemdianguc .
  **Cần giảm dung lượng**: 155,7 MB nằm ở tài nguyên, và texture đang nén ASTC nên WebGL
  phải giải nén ra RAM mỗi lần nạp.
  Giai đoạn 2 (chưa làm): thấy nhau trong trận, đánh nhau, người sống cuối cùng thắng.
- Dự án **đã là git repo** (nhánh `main`, ảnh chụp đầu tiên `0ef3e18`, 05/09/2026, 1175 file).
  `Assets/MeshyImports/` (920 MB model gốc Meshy) **nằm ngoài git** — file vẫn trên đĩa, Unity
  vẫn dùng bình thường, nhưng git không cứu được nếu lỡ xoá. `HUONG-DAN.md` vẫn là nơi kể
  **vì sao**, git chỉ giữ chỗ lùi lại.
