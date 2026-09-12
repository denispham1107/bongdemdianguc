# Cấp độ, kinh nghiệm và điểm kỹ năng

Tài liệu này ghi lại **những con số** của hệ thống cấp độ, đúng như anh yêu cầu ngày 13/09/2026.
Mọi con số ở đây đều nằm trong một chỗ duy nhất trong mã nguồn: `Assets/Scripts/Player/CapDo.cs`.

---

## 1. Nguyên tắc chung

- Vào trận, **ai cũng cấp 1**. Cấp tối đa là **cấp 10**.
- Cấp **tính theo từng trận**, không cất lại giữa các trận. Mỗi trận là một ván đấu riêng
  (người sống sót cuối cùng thắng) — giữ cấp giữa các trận thì người chơi lâu năm vào trận với
  cấp 10 trong khi người mới cấp 1, và không còn là một ván đấu nữa.
- Cấp hiện ra **ngay sau tên** trên bảng góc trái: `PHÙ THỦY · Cấp 3`, kèm một thanh kinh nghiệm
  mảnh dưới thanh năng lượng.

---

## 2. Cần bao nhiêu kinh nghiệm để lên cấp

Mỗi bậc nặng hơn bậc trước khoảng **1,35 lần** (làm tròn cho dễ nhìn).

| Từ cấp → cấp | Cần | Cộng dồn từ đầu |
|---|---:|---:|
| 1 → 2 | 100 | 100 |
| 2 → 3 | 135 | 235 |
| 3 → 4 | 180 | 415 |
| 4 → 5 | 245 | 660 |
| 5 → 6 | 330 | 990 |
| 6 → 7 | 445 | 1 435 |
| 7 → 8 | 600 | 2 035 |
| 8 → 9 | 810 | 2 845 |
| 9 → 10 | 1 090 | **3 935** |

Giết một con quái to đúng lúc sắp lên cấp thì **nhảy được hai bậc** — phần kinh nghiệm thừa
được giữ lại, không bỏ phí. Đến cấp 10 thì thôi đếm.

---

## 3. Giết gì được bao nhiêu

| Đối tượng | Kinh nghiệm | Vì sao |
|---|---:|---|
| Bộ xương | 18 | quái thường, đông nhất |
| Xác sống | 20 | |
| Quỷ lùn | 22 | |
| Quỷ cây | 30 | chạy rất nhanh, bắn tia sét |
| Mụ phù thủy | 32 | đánh từ xa, khó tới gần |
| Quỷ dữ | 40 | gọi thiên thạch từ trên trời |
| Quỷ khổng lồ | 70 | to và khoẻ nhất |
| **Người chơi khác** | **250** | một mạng người đáng giá hơn cả một đợt quái |

### Nhịp lên cấp trong thực tế (Act2)

Mỗi đợt ở Act2 sinh quanh **từng người chơi** bốn con: bộ xương + phù thủy + quỷ cây + quỷ dữ
= **120 kinh nghiệm** một đợt, và từ đợt hai trở đi mỗi đợt cộng dồn thêm quái.

| Sau đợt | Kinh nghiệm gom được (tự giết hết phần của mình) | Cấp đạt |
|---|---:|---|
| 1 | ~120 | 2 |
| 2 | ~270 | 3 |
| 3 | ~480 | 4 |
| 5 | ~1 000 | 6 |
| 7 | ~1 700 | 7 |
| 10 | ~3 100 | 9 |
| 12 | ~4 000 | **10** |

Nghĩa là: chơi một mình, chăm giết quái thì khoảng **12 đợt** là chạm cấp 10. Hạ được một người
chơi khác thì rút ngắn quãng đường bằng hơn hai đợt quái.

---

## 4. Lên cấp được gì

Mỗi cấp, chỉ số **nhân thêm** (không phải cộng tuyến tính):

| Chỉ số | Mỗi cấp | Cấp 10 so với cấp 1 |
|---|---|---:|
| Máu | +15% | ×3,52 (600 → 2 113) |
| Năng lượng | +10% | ×2,36 (250 → 590) |
| Tốc độ di chuyển | +3,5% | ×1,36 |

Máu và năng lượng **đang có** cũng được cộng đúng phần vừa tăng thêm — không hồi đầy (lên cấp
thành một bình máu miễn phí), nhưng cũng không để người chơi tụt tỉ lệ: đang 50% máu mà chỉ kéo
trần lên thì tự nhiên còn 43%.

Ngoài ra **mỗi lần lên cấp được 1 điểm kỹ năng**.

---

## 5. Điểm kỹ năng và Sách phép

- Vào trận ở cấp 1: **cả bảy kỹ năng đều khoá**, và anh có sẵn **1 điểm** để mở một cái mình muốn.
- Mỗi lần lên cấp: **+1 điểm**. Dùng để **mở khoá** một kỹ năng mới, hoặc **nâng cấp** kỹ năng
  đang dùng — anh tự chọn.
- Cả trận có **10 điểm** (1 lúc đầu + 9 lần lên cấp). Không đủ để vừa mở cả bảy vừa nâng hết:
  mở hết bảy kỹ năng thì chỉ còn 3 điểm để nâng; dồn vào ba kỹ năng thì có thể đưa chúng lên rất cao.
  Đó chính là chỗ để anh chọn lối chơi.
- Chỗ mở khoá và nâng cấp nằm trong cửa sổ **Sách phép** (nút hình cuốn sách, hoặc phím **P**).
  Kỹ năng chưa mở hiện ra xám cả trên bảng lẫn trên nút ngoài trận, có hình ổ khoá, và bấm vào
  thì báo *"… chưa mở khoá — vào SÁCH PHÉP để mở"*.

---

## 6. Nâng cấp kỹ năng được gì

Kỹ năng có **cấp tối đa 5**. Mỗi cấp:

| | Mỗi cấp | Cấp 5 so với cấp 1 |
|---|---|---:|
| Sát thương | +20% | ×2,07 |
| Năng lượng tiêu tốn | +10% | ×1,46 |
| Hiệu ứng kéo dài thêm | +0,15 giây | +0,60 giây |
| **Riêng Khiên**: máu khiên | +15% | ×1,75 (150 → 262) |

"Hiệu ứng" là thứ mỗi kỹ năng tự có:

| Kỹ năng | Hiệu ứng được kéo dài |
|---|---|
| Quả cầu lửa | thời gian cháy |
| Mưa băng | đóng cứng **và** làm chậm |
| Sấm sét | choáng |
| Lốc xoáy | cơn lốc sống lâu hơn (cuốn được lâu hơn) |
| Thiên thạch | vũng lửa cháy trên mặt đất |
| Khiên | *(thay bằng +15% máu khiên)* |
| Giựt sét | *(không có hiệu ứng kéo dài)* |

Sát thương tăng **nhân dồn** chứ không cộng tuyến tính, giống cách máu nhân vật tăng — cộng
tuyến tính thì những cấp cuối gần như không thấy khác gì.

---

## 7. Khi chơi nhiều người

- **Cấp kỹ năng đi kèm từng lần tung phép** qua mạng. Người cấp 5 bắn sang máy anh thì đòn đó
  mạnh đúng bằng cấp 5 — không bị tính lại theo cấp của người xem. (Thiếu chỗ này thì hai màn
  hình hiện hai con số sát thương khác nhau.)
- **Kinh nghiệm giết quái** do máy chủ phòng chia: chỉ nó chạy trí tuệ quái nên chỉ nó biết con
  vừa chết là do tay ai. Nó gửi một gói nhỏ 4 byte sang máy người đó.
- **Kinh nghiệm giết người** đi theo gói báo tử mà chính máy nạn nhân gửi (nó là trọng tài cái
  chết của mình), trong đó đã có sẵn ghế của kẻ hạ.
- Máu của người khác vẫn truyền dưới dạng **tỉ lệ phần trăm**, nên thanh máu trên đầu họ vẫn
  đúng dù cấp của họ cao tới đâu.

---

## 8. Muốn chỉnh số thì sửa ở đâu

| Muốn đổi | Sửa |
|---|---|
| Bảng kinh nghiệm mỗi cấp | `CapDo.canDeLen` |
| Kinh nghiệm mỗi loại quái | `CapDo.KnCuaQuai` |
| Kinh nghiệm giết người | `CapDo.KnGietNguoi` |
| Máu / năng lượng / tốc độ mỗi cấp | `CapDo.HeSoMauTheoCap`, `HeSoManaTheoCap`, `HeSoTocTheoCap` và `PlayerController.LenCap` |
| Sức mạnh kỹ năng mỗi cấp | `CapDo.SatThuongTheoCap`, `ManaTheoCap`, `ThemGiayHieuUngTheoCap`, `MauKhiengTheoCap` |
| Cấp tối đa | `CapDo.CapToiDa`, `CapDo.CapKyNangToiDa` |

Sửa xong chạy **menu 60** (`Diablo 2.5D/60. Chay thu CAP DO`) để đo lại — nó kiểm cả bảng kinh
nghiệm lẫn chỉ số thật của nhân vật trong Play.
