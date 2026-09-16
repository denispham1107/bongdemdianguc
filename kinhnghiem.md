# Cấp độ, kinh nghiệm và điểm kỹ năng

Tài liệu này ghi lại **những con số** của hệ thống cấp độ, đúng như anh yêu cầu ngày 13/09/2026
(cùng ngày anh nâng cấp tối đa từ **10 lên 20**).
Mọi con số ở đây đều nằm trong một chỗ duy nhất trong mã nguồn: `Assets/Scripts/Player/CapDo.cs`.

---

## 1. Nguyên tắc chung

- Vào trận, **ai cũng cấp 1**. Cấp tối đa là **cấp 20**.
- Cấp **tính theo từng trận**, không cất lại giữa các trận. Mỗi trận là một ván đấu riêng
  (người sống sót cuối cùng thắng) — giữ cấp giữa các trận thì người chơi lâu năm vào trận với
  cấp 20 trong khi người mới cấp 1, và không còn là một ván đấu nữa.
- Cấp hiện ra **ngay sau tên** trên bảng góc trái: `PHÙ THỦY · Cấp 3`, kèm một thanh kinh nghiệm
  mảnh dưới thanh năng lượng.

---

## 2. Cần bao nhiêu kinh nghiệm để lên cấp

Cấp 1 → 10: mỗi bậc nặng hơn bậc trước khoảng **1,35 lần**. Cấp 10 → 20: mỗi bậc chỉ nặng hơn khoảng
**1,15 lần** (làm tròn cho dễ nhìn).

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
| 9 → 10 | 1 090 | 3 935 |
| 10 → 11 | 1 250 | 5 185 |
| 11 → 12 | 1 440 | 6 625 |
| 12 → 13 | 1 660 | 8 285 |
| 13 → 14 | 1 910 | 10 195 |
| 14 → 15 | 2 200 | 12 395 |
| 15 → 16 | 2 530 | 14 925 |
| 16 → 17 | 2 910 | 17 835 |
| 17 → 18 | 3 350 | 21 185 |
| 18 → 19 | 3 850 | 25 035 |
| 19 → 20 | 4 430 | **29 465** |

**Vì sao sau cấp 10 lại tăng chậm hơn:** giữ nguyên 1,35 lần thì riêng bậc 19 → 20 đã cần 16 000, tổng lên cấp 20 là
~62 000 kinh nghiệm — chơi một mình phải qua ~22 đợt, một trận gần như không ai chạm nổi cấp 20. Với 1,15 lần thì
khoảng **16 đợt** (xem bảng ở mục 3). Muốn lên cấp nhanh hay chậm hơn thì sửa đúng bảng này.

Giết một con quái to đúng lúc sắp lên cấp thì **nhảy được hai bậc** — phần kinh nghiệm thừa
được giữ lại, không bỏ phí. Đến cấp 20 thì thôi đếm.

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

Mỗi đợt ở Act2 gồm (luật 13/09/2026):

- quanh **từng người chơi** bốn con: bộ xương + phù thủy + quỷ cây + quỷ dữ = **120 kinh nghiệm**;
- từ đợt hai trở đi cộng dồn thêm quái bất kì (+1, +3, +6, +10…);
- **cố định 20 con** loại ngẫu nhiên ở vòng ngoài, cách người chơi gần nhất 20–25 m — trung bình ~30 kinh nghiệm
  một con, tức **~600 kinh nghiệm** nếu giết hết. 20 con này là của **cả phòng**, không nhân theo số người.

Chơi **một mình**, giết hết cả đợt:

| Sau đợt | Kinh nghiệm đợt ấy | Cộng dồn | Cấp đạt |
|---|---:|---:|---|
| 1 | 120 + 600 = 720 | 720 | 5 |
| 2 | 120 + 30 + 600 = 750 | 1 470 | 7 |
| 3 | 810 | 2 280 | 8 |
| 4 | 900 | 3 180 | 9 |
| 5 | 1 020 | 4 200 | 10 |
| 6 | 1 170 | 5 370 | 11 |
| 7 | 1 350 | 6 720 | 12 |
| 8 | 1 560 | 8 280 | 12 (thiếu 5 điểm) |
| 9 | 1 800 | 10 080 | 13 |
| 10 | 2 070 | 12 150 | 14 |
| 11 | 2 370 | 14 520 | 15 |
| 12 | 2 700 | 17 220 | 16 |
| 13 | 3 060 | 20 280 | 17 |
| 14 | 3 450 | 23 730 | 18 |
| 15 | 3 870 | 27 600 | 19 |
| 16 | 4 320 | 31 920 | **20** |

(Mỗi đợt = 120 quanh mình + 30 × số quái cộng dồn + 600 của 20 con vòng ngoài.)

Nghĩa là: chơi một mình, giết hết thì khoảng **5 đợt** chạm cấp 10 và khoảng **16 đợt** chạm cấp 20.
Chơi **bốn người** thì 20 con vòng ngoài chia nhau — mỗi người chỉ còn khoảng 5 con (~150 kinh nghiệm) cộng phần 120
quanh mình, nên lên cấp chậm hơn hẳn. Hạ được một người chơi khác (250) vẫn đáng giá hơn cả phần quái quanh mình
trong một đợt.

⚠️ Nếu thấy lên cấp **quá nhanh** khi chơi một mình, chỗ chỉnh là bảng kinh nghiệm ở mục 2 hoặc giá từng loại quái
ở trên — số quái mỗi đợt là luật anh đã chốt nên tôi không đụng vào.

---

## 4. Lên cấp được gì

Mỗi cấp, chỉ số **nhân thêm** (không phải cộng tuyến tính):

| Chỉ số | Mỗi cấp | Cấp 10 so với cấp 1 |
|---|---|---:|
| Máu | +15% | ×3,52 (600 → 2 113) |
| Năng lượng | +10% | ×2,36 (250 → 590) |
| Tốc độ di chuyển | +3,5% **(chỉ tới cấp 10)** | ×1,36 |

Cấp 20 so với cấp 1: **máu ×14,2** (600 → 8 539), **năng lượng ×6,1** (250 → 1 529), **tốc độ di chuyển ×1,36**
(5,2 → 7,1 m/giây).

**Tốc độ chỉ tăng tới cấp 10** (anh chốt 13/09/2026). Tăng đều tới cấp 20 thì tốc độ ×1,92, tức ~10 m/giây — chạy vượt
mọi loại quái (nhanh nhất 5,98 m/giây). Từ cấp 11 trở đi máu và năng lượng vẫn tăng, tốc độ giữ nguyên. Chỗ sửa:
`CapDo.CapTangTocToiDa` (công thức `HeSoTocTheoCap` và `PlayerController.LenCap` đều đọc hằng này).

Máu và năng lượng **đang có** cũng được cộng đúng phần vừa tăng thêm — không hồi đầy (lên cấp
thành một bình máu miễn phí), nhưng cũng không để người chơi tụt tỉ lệ: đang 50% máu mà chỉ kéo
trần lên thì tự nhiên còn 43%.

Ngoài ra **mỗi lần lên cấp được 1 điểm kỹ năng**.

---

## 5. Điểm kỹ năng và Sách phép

- Vào trận ở cấp 1: **cả mười kỹ năng đều khoá** (8 phép — thêm Quả cầu băng 16/09/2026 — + Bình máu + Bình mana), và anh có sẵn **1 điểm** để mở một cái mình muốn.
- Mỗi lần lên cấp: **+1 điểm**. Dùng để **mở khoá** một kỹ năng mới, hoặc **nâng cấp** kỹ năng
  đang dùng — anh tự chọn.
- Cả trận có **20 điểm** (1 lúc đầu + 19 lần lên cấp). Mở cả tám phép và nâng hết lên cấp 5 cần 40 điểm, cộng
  2 điểm mở hai bình là 42 — không bao giờ đủ: mở hết mười kỹ năng thì còn 10 điểm để nâng.
- **Bình máu / Bình mana** (13/09/2026): mở khoá 1 điểm, **cấp tối đa 1** (không nâng cấp). Chỉ dùng được khi đã
  **nhặt** bình: mỗi con quái bị hạ rơi bình máu 10%, bình mana 10% (độc lập); tới gần 3,5 m là bình tự bay vào
  người. Một bình hồi **tối đa 100 máu / 50 năng lượng**, hồi chiêu 0,5 giây; đầy thì không uống (không mất bình).
  Số bình tính theo trận, như cấp. Chơi nhiều người thì bình rơi là **của chung cả phòng** — ai tới trước người ấy được.
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
| Giựt sét | choáng (1,5 giây → cấp 5: 2,10 giây) — từ 16/09/2026 |
| Quả cầu băng | làm chậm (2 giây → cấp 5: 2,60 giây) |
| Bình máu / Bình mana | *(không nâng cấp được — cấp tối đa 1)* |

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
