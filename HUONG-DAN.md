# Diablo 2.5D — Game nhập vai hành động kiểu Diablo II

Một game action-RPG **3D** chơi kiểu Diablo II, nhưng bạn có thể **đổi góc nhìn tự do
giữa 3D → 2.5D → 2D** ngay trong lúc chơi chỉ bằng một phím.

Nhân vật là một **đại phù thủy** (áo choàng tím than viền vàng, tóc và râu bạc,
cây trượng gắn viên pha lê tím) với bốn phép:

| Phím | Phép | Hiệu ứng |
|------|------|----------|
| **1** | 🔥 **Bắn quả cầu lửa** | **Ba** quả cầu lửa cuộn xoáy bay đi cùng lúc thành hình quạt, kéo theo đuôi lửa, tàn lửa và khói. Chạm mục tiêu thì **nổ bung**, hắt ánh sáng cam ra cả khung cảnh, để lại vệt cháy đen và làm quái **bốc cháy** mất máu dần. |
| **2** | ❄️ **Mưa băng** | Một vùng bão lạnh phủ xuống: tuyết rơi, sương lạnh là mặt đất, vòng phép xoay tròn. Từng tảng băng lớn liên tục rơi xuống, **nổ tung** thành mảnh băng, mọc gai băng từ dưới đất lên và **đóng băng** quái vật. Quái đang đóng băng mà chết thì **vỡ tan** thành mảnh. |
| **3** | ⚡ **Sấm sét** | Nhiều tia sét liên tiếp đánh thẳng từ trên trời xuống một vùng tròn. Mỗi tia có **40% gây choáng** 2,2 giây — quái bị choáng đứng im, hiện chữ "CHOANG!" và một vòng tia lửa xoay quanh đầu. |
| **4** | 🌪️ **Lốc xoáy** | Một cơn lốc kèm tia sét tự chạy tới trước. Quái dính vào bị **cuốn bổng lên cao**, xoay theo chiều lốc và **đi theo lốc**, vừa bay vừa mất máu; lúc lốc tan thì rơi xuống chịu thêm sát thương. Đất cát li ti dưới chân bị hút lên và **đổi màu theo màu đất** mà lốc đi qua, phía sau kéo một **dải khói bụi đen mỗi lúc một dày**. |

Dự án được tổ chức **giống hệt một dự án Unity bình thường** (như game Mario của bạn):
có đủ **Scenes, Prefabs, Models, Materials, Textures** là file thật, bấm vào xem và sửa được.

---

## Phần 1 — Mở game lên chơi (5 phút)

### Bước 1. Mở Unity Hub
Tìm biểu tượng **Unity Hub** trong Start Menu và mở lên.

### Bước 2. Thêm project

- Ở màn hình **Projects**, bấm **mũi tên nhỏ ▼** cạnh nút **Add**
- Chọn **"Add project from disk"**
- Chọn thư mục: `C:\Users\HP\Documents\GameUnity\Diablo25D`

> ⚠️ Phải chọn **đúng thư mục `Diablo25D`** (bên trong nó có `Assets`, `Packages`, `ProjectSettings`).

### Bước 3. Mở project
Bấm vào tên project `Diablo25D`. Lần đầu mở hơi lâu (1–3 phút), cứ để yên.
Game dùng đúng bản Unity **6000.5.6f1** bạn đã cài.

### Bước 4. Mở màn chơi và bấm Play

Khung dưới bên trái → tab **Project** → **Assets** → **Scenes** → bấm đúp một trong ba scene:

| Scene | Nội dung |
|-------|----------|
| **MainMenu** | Màn hình chính: tên game, nút vào chơi, bảng hướng dẫn phím, phù thủy đứng xoay giữa hai lò lửa |
| **Act1** | Màn chơi — đấu trường bán kính 42m; đợt đầu 6 quái (5 bốc ngẫu nhiên + 1 phù thủy), các đợt sau đông dần |
| **Act2** | Nghĩa địa vẽ tay trong Blender, 109,2 × 109,2 m; **mỗi đợt gấp đôi Act1** (đợt đầu 11 quái) |

Rồi bấm nút **▶ Play** ở giữa phía trên.

---

## Phần 2 — Cách chơi

| Thao tác | Tác dụng |
|----------|----------|
| **Chuột trái** (bấm/giữ xuống đất) | Đi tới chỗ đó (kiểu Diablo) |
| **Phím 1 → 7** | **Đánh ra ngay** về phía con trỏ chuột: 1 = Quả cầu lửa, 2 = Mưa băng, 3 = Sấm sét, 4 = **Lốc xoáy**, 5 = **Thiên thạch**, 6 = **Khiêng bảo vệ**, 7 = **Giựt sét** |
| **Phím Z / X / V / B / N / M / G** | Hàng phím **dự phòng** cho bảy phép trên. Dùng khi hàng số bị bộ gõ tiếng Việt ăn mất |
| **Bấm vào ô phép** dưới màn hình | Đánh ra ngay, thẳng về phía trước mặt |
| **Giữ chuột phải rồi kéo** | **Tự xoay camera** mọi hướng. Thả tay ra thì camera đứng yên ở đúng góc đó |
| **W A S D** / mũi tên | Đi chuyển trực tiếp (hướng theo camera) |
| **Phím C** | **Đổi góc nhìn** (3D → 2.5D → 2D trên xuống → 2D nhìn ngang), đồng thời trả camera về góc chuẩn |
| **F1–F4** | Nhảy thẳng tới một góc nhìn cụ thể |
| **Q / E**, giữ chuột giữa | Xoay camera quanh nhân vật |
| **Con lăn chuột** | Phóng to / thu nhỏ |
| **R** | Chơi lại màn hiện tại |
| **ESC** | Về màn hình chính |
| **F12** | Bật/tắt bảng chẩn đoán ở góc trái trên |

### Khi thấy "bấm phím mà không ra phép"

Bảng chẩn đoán (góc trái trên, tắt bằng **F12**) trả lời ngay hỏng ở đâu:

| Dòng trên bảng | Nghĩa là |
|----------------|----------|
| `Nhân vật: DA CHET` | Đã gục — bấm **R** chơi lại. Nhân vật chết thì mọi phím đều vô hiệu, đây là nguyên nhân hay gặp nhất |
| `Số vòng Update` đứng im | Nhân vật đã chết, hoặc trò chơi đang tạm dừng |
| `Phím bất kỳ: 0` mà bạn có bấm | Bàn phím không tới được game: bấm chuột vào trong khung Game một cái, hoặc tắt bộ gõ tiếng Việt (kiểu VNI lấy phím số 1-9 làm dấu thanh), hoặc dùng hàng **Z X V B** |
| `Phím kỹ năng` có tăng mà không ra phép | Đang hồi chiêu hoặc thiếu năng lượng — giữa màn hình sẽ hiện chữ báo lý do |

### Máu và mana của nhân vật

**400 máu, 250 mana**, hồi 9 mana mỗi giây (đầy bình từ 0 mất 27,8 giây).

Với 250 mana, đánh liên tiếp được:

```
Qua cau lua (10 mana) : 25 phat
Giut set    (14 mana) : 17 phat
Mua bang    (34 mana) :  7 phat
Sam set     (42 mana) :  5 phat
```

> ⚠️ Hai con số này nằm ở **bốn chỗ**, sửa thiếu một chỗ là không ăn:
>
> | Chỗ | Trường |
> |---|---|
> | `GameBootstrap.cs` | `playerMaxHealth`, `playerMaxMana` |
> | `PlayerController.cs` | `maxMana`, `mana` |
> | prefab `Player_Sorceress` | `Damageable.maxHealth`, `PlayerController.maxMana` |
> | `Act1.unity` và `Act2.unity` | cả `GameBootstrap` lẫn bản sao `Player` trong cảnh |
>
> `GameBootstrap` là chỗ dễ quên nhất: nó **ghi đè** máu và mana lúc chạy
> (`hp.maxHealth = playerMaxHealth`), nên dù prefab và cảnh đã đúng mà nó còn số cũ thì vào
> game vẫn ra số cũ. Tôi đã vấp: sửa đủ ba chỗ kia, chạy thử thấy máu 400 đúng nhưng mana
> vẫn 130 — vì `playerMaxMana` trong `GameBootstrap` còn nguyên.

Vài phép đo cũ trong tài liệu này ghi "170 máu" — đó là số **lúc đo**, không phải số hiện
tại. Giữ nguyên để con số đo và kết luận còn khớp nhau.

### Ba phép của phù thủy

| Phím | Phép | Năng lượng | Hồi chiêu | Tầm | Tác dụng |
|------|------|-----------|-----------|-----|----------|
| **1** | Quả cầu lửa | 10 | 0.55 giây | bay tự do | Bắn **ba quả cùng lúc** toè hình quạt (−11° / 0° / +11°), mỗi quả **55 sát thương** khi chạm, nổ ra vùng lửa bán kính 3,4 m và gây **bỏng cháy** 3,5 giây |
| **2** | Mưa băng | 34 | 6 giây | **12 m** | Bão tuyết phủ một vùng, tảng băng rơi liên tục từ độ cao **20 m**. Mỗi tảng gây **29 sát thương** trong bán kính 1.7m và có **30% khả năng ĐÓNG BĂNG** quái 2.6 giây (không đi được, không đánh được). Tảng băng **ưu tiên nhắm vào quái** đang đứng trong vùng. |
| **3** | **Sấm sét** | 42 | 8 giây | **12 m** | Giông kéo tới, **hàng loạt tia sét giáng từ trên cao xuống**. Mỗi tia gây 26 sát thương trong bán kính 2.1m và có **40% khả năng làm quái bị CHOÁNG 2.2 giây** (đứng im, không đánh được). Sét ưu tiên nhắm vào quái đang đứng trong vùng. |

| **5** | **Thiên thạch** | 60 | **2 giây** | Gọi **ba** khối đá rực lửa lao từ trên trời xuống, nối đuôi nhau **cách nhau 0,7 giây**. Chạm đất hoặc chạm kẻ địch là **nổ tung**: 85 sát thương cả vùng bán kính 4,2 m, rồi **để lại một bãi đất cháy âm ỉ 5 giây** trong bán kính **4,5 m** — ai đứng trong đó ăn 26 sát thương mỗi giây và bị bắt lửa. |

| **7** | ⚡ **Giựt sét** | **14** | **0,55 giây** | Một tia sét phóng sang kẻ địch gần nhất trong tầm **19,5 m**, rồi **lan tiếp** từ con đó sang con khác trong bán kính **5,5 m** — tối đa **6 mục tiêu**. Mỗi nhịp lan cách nhau 0,07 giây và sát thương còn **85%** của nhịp trước: 30 → 25,5 → 21,7 → 18,4 → 15,7 → 13,3. |

| **6** | **Khiêng bảo vệ** | 45 | 12 giây | Mở một quả cầu năng lượng bao quanh nhân vật. Khiêng có **150 máu riêng**; mọi đòn đánh trúng người chơi đều trừ vào khiêng trước. Hết máu thì khiêng **nổ tung**, và từ lúc đó nhân vật mới chịu sát thương. Quái không lại gần được, đạn từ xa nổ ngay trên mặt cầu. |

#### Khiêng bảo vệ — ba việc phải làm cùng nhau

Thiếu một cái là nó không còn là cái khiên nữa:

| Việc | Làm ở đâu |
|---|---|
| **Hấp thụ sát thương** | Móc vào đầu `Damageable.TakeDamage` |
| **Chặn quái lại gần** | `SphereCollider` đặc ở lớp riêng `Khieng` |
| **Chặn đạn từ xa** | `EnemyAI` thêm lớp `Khieng` vào mask của đạn |

**Vì sao phải dùng một lớp riêng.** Nếu để khiêng ở lớp `Default` thì đạn của **chính
người chơi** cũng nổ vào nó: đạn sinh ra ở tay, tức **bên trong** quả cầu, nên nó đâm
vào mặt trong và nổ ngay — bật khiêng lên là không bắn ra được phát nào. Lớp riêng cho
phép nói "đạn địch thì chặn, đạn mình thì cho qua". Bảng va chạm được đặt bằng code
trong `Khieng.DonBangVaCham` vì bảng trong Project Settings không tự có dòng nào cho
một lớp vừa thêm.

**Đòn phá vỡ khiêng không tràn sang người chơi.** Đo lại toàn bộ chuỗi:

| Đòn | Khiêng | Máu người |
|---|---|---|
| 40 | 150 → 110 | 170 |
| 40 | 110 → 70 | 170 |
| 40 | 70 → 30 | 170 |
| **500** | 30 → **0** (vỡ) | **170** — không mất máu nào |
| 25 | — | 170 → **145** |

#### Vẻ ngoài quả cầu: gân sáng, không phải lưới

Bản vẽ đầu tiên của tôi **sai hẳn so với mẫu**: nền đục mờ phủ kín, và vân là một
**lưới lục giác đều tăm tắp** trải khắp quả cầu. Nhìn ra quả bóng nhựa chứ không ra
trường năng lượng, lại che mất cả nhân vật bên trong.

Mẫu thật thì ngược lại — quả cầu **trong suốt gần như hoàn toàn**, trên mặt chỉ có
những **đường gân sáng mảnh, cong ngoằn ngoèo**, dày ở rìa và thưa dần vào giữa.

Ba điểm phải sửa:

| Sai | Đúng |
|---|---|
| Nền mờ đục phủ kín | **Nền trong hoàn toàn** — độ đặc chỉ đến từ gân và viền |
| Lưới lục giác đều | **Đường đồng mức của nhiễu** — cong ngoằn ngoèo, mỗi chỗ một khác |
| Vân trải đều khắp cầu | **Dồn ra rìa**: ở chính giữa hệ số chỉ 0,02 |

**Là VÒM BÁN CẦU, không phải quả cầu.** Chỉ có nửa trên, đáy cắt phẳng nằm sát mặt
đất — như cái mái vòm chụp lên nhân vật. Đo lại: rộng **6,07 m**, cao **3,33 m**, chân vòm
đúng **0,00 m** so với chân nhân vật.

> Tôi mất **ba vòng sửa** mới ra: cầu tròn → trứng thon (đỉnh nhọn hoắt) → lại cầu tròn.
> Cả ba đều sai vì tôi đọc hình mẫu ra "quả cầu" trong khi nó là "vòm". Đáng ra nên hỏi
> ngay từ lần sửa thứ hai thay vì đoán tiếp.

Lưới vòm chia đều theo **góc**, không theo chiều cao. Chia theo chiều cao thì sát đỉnh các
vòng thưa ra và chỗ chụm lại bị gãy khúc — nhìn y như đỉnh nhọn, **dù đo bán kính hai bên
lệch nhau đúng 0,000 m**. Đây là cái bẫy làm tôi tưởng hình sai trong khi hình đúng.

**Không ám xanh.** Gân và viền phải **trắng thuần**: chênh lệch xanh−đỏ đo được **−0,003**
(0 = trắng hoàn toàn). Màu xanh dương làm cảnh vật nhìn xuyên qua bị nhuộm màu; mẫu chuẩn
thì trong veo, chỉ sáng lên chứ không đổi màu thứ gì phía sau. Đèn và đốm sáng bên trong
cũng phải trắng theo, không thì chúng kéo cả vòm về xanh.

Màu xanh dương làm cảnh vật nhìn xuyên qua bị đổi màu. Mẫu chuẩn thì trong veo: **chỉ sáng
lên chứ không nhuộm màu thứ gì phía sau**. Đèn và đốm sáng bên trong cũng phải đổi sang
trắng theo, không thì chúng kéo cả quả cầu về xanh.

Vách chặn dùng `SphereCollider` tâm đặt ngay **chân nhân vật**, bán kính bằng bán kính
ngang của vòm. Nửa dưới hình cầu chìm xuống dưới đất nên không ảnh hưởng gì — quái vẫn bị
chặn đúng ở mép vòm theo phương ngang.

#### Lỗi thứ tư: quay camera thì khiêng biến mất

Triệu chứng: ở một số góc camera khiêng **mất hẳn** (chỉ còn đốm hạt), ở góc khác thì
**chỉ hiện một mảng** — luôn là nửa xa của vòm, ranh giới là đường chân trời của nó.

Nguyên nhân là hậu quả muộn của lỗi thứ hai. Sửa đúng công thức fresnel (so với
`(0,0,1)`) vẫn chưa đủ: công thức ấy **chỉ hợp lệ khi lưới có tangent**, vì `IN.viewDir`
được chuyển sang không gian tiếp tuyến bằng ma trận TBN dựng từ `v.tangent`.

`ProcMesh.Builder.Build` **không sinh tangent**. Tangent bằng 0 thì hai hàng đầu của ma
trận bằng 0, `viewDir` co lại thành `(0, 0, ±1)`, và fresnel chỉ còn **nhận hai giá
trị**: 0 hoặc 1, tùy mặt tam giác quay về hay quay lưng lại camera. Mà toàn bộ hình
khiêng — cả vòm lẫn tia sét — đều nhân với fresnel, nên fresnel tắt là tắt hết.

**Cách chữa**: bỏ hẳn `viewDir`, tính fresnel trong **không gian thế giới** với pháp
tuyến và vị trí **tự truyền** từ vertex shader — không đụng đến tangent một chút nào.
Lấy `abs()` của tích vô hướng vì `Cull Off` vẽ cả hai mặt.

Đo ở 8 góc (4 phương vị × 2 góc nghiêng), diện tích vòm nhìn thấy: **48,1% – 57,2%**
— chính là chênh lệch hình chiếu giữa hai góc nghiêng, không còn góc nào tắt ngóm.

#### Sửa fresnel xong lại lộ ra hai lỗi nữa

**Màu được cộng thẳng thay vì lấy trung bình.** Vùng chỉ có màng vòm (không tia,
không viền) ra màu tối ~0,2 nhưng vẫn có độ đặc, nên nó làm cảnh phía sau **xám đi**
thay vì sáng lên — cả cái vòm trông như một vết bẩn mờ. Chữa bằng cách chia tổng màu
cho **tổng độ đặc**, tức lấy trung bình có trọng số của ba lớp. Đo số điểm bị vòm
làm tối đi: **0,0%** ở cả 8 góc.

**Nửa vòm trước che mất nhân vật.** `Cull Off` vẽ cả hai mặt, mà nhân vật đứng
**giữa** hai lớp vòm — nên nhìn vào nhân vật là nhìn xuyên qua lớp trước. Bản cũ
trông rõ chỉ vì fresnel hỏng vô tình làm nửa trước tàng hình hẳn.

Chữa bằng `_MatTruoc` (0,35): nửa trước chỉ còn 35% độ đặc. Nhận biết hai nửa bằng
**dấu của tích vô hướng** chứ không bằng `VFACE` hay culling — cách này không phụ thuộc
chiều cuốn tam giác nên lưới nào cũng đúng. Chuyển tiếp phải **mượt** (`smoothstep`):
dùng `step()` thì chỗ giáp ranh nhảy một phát từ đậm sang trong, thành một đường cắt
ngang vòm rất gắt.

### FLIPBOOK — ảnh động cho từng hạt

Mọi texture hiệu ứng trước đây đều **sinh bằng code và đứng yên**: `TextureFactory` có
29 hàm, mỗi hàm trả về một ảnh tĩnh. Hạt chỉ biết to lên, nhỏ đi, mờ đi — nên đám khói
trông như một vệt đen phẳng.

**Flipbook** là cách chữa: texture là một **lưới ô** (6×6 = 36 khung hình) chụp một đám
khói đang cuộn. Hệ hạt chạy lần lượt qua các ô theo đời sống của hạt, nên **bên trong
từng hạt cũng có chuyển động**.

| | Tập tin |
|---|---|
| Shader trộn hai khung | [S_ParticleFlipbookAdd.shader](Assets/Shaders/S_ParticleFlipbookAdd.shader), [S_ParticleFlipbookAlpha.shader](Assets/Shaders/S_ParticleFlipbookAlpha.shader) |
| Bật cho một hệ hạt | `VfxFactory.BatFlipbook` |
| Nạp lưới ảnh | `VfxFactory.NapFlipbook` — `Assets/Resources/Flipbooks/` |

**Vì sao phải có shader riêng.** Unity tự cắt ô giúp, nhưng khi nhảy từ khung này sang
khung kế tiếp thì ảnh **đổi đột ngột** — khói bị giật. Muốn mượt thì phải lấy mẫu **cả
hai khung** rồi trộn. Ba thứ cần cho việc đó (uv khung hiện tại, uv khung kế tiếp, hệ số
trộn) chỉ đến khi Custom Vertex Streams được khai báo đúng **thứ tự**
`Position, Color, UV, UV2, AnimBlend`. Sai thứ tự thì **không báo lỗi gì cả** — hạt chỉ ra
sai màu hoặc đứng im ở một khung.

#### Ba flipbook đã dựng

| Lưới ảnh | Dùng cho | Màu |
|---|---|---|
| `KhoiCuon` | khói bụi sau Lốc xoáy | **trắng** — Unity tô màu sau |
| `LuaCuon` | vùng đất cháy của Thiên thạch | **giữ nguyên màu** |
| `VuNo` | Thiên thạch chạm đất | **giữ nguyên màu** |

**Khói render trắng, lửa thì không.** Khói trắng cho phép một lưới ảnh dùng được cho cả
khói đen của Lốc xoáy lẫn khói tím của Khiêng vỡ. Nhưng màu lửa là **màu của vật đen ở
các mức nhiệt khác nhau** và nó đổi theo **từng điểm** trong ngọn lửa — tô một màu duy
nhất lên ảnh trắng là mất hết chuyển màu vàng → cam → đỏ. Đo màu lưới lửa: **R 207 /
G 118 / B 98**.

Ba cái bẫy riêng của lửa và nổ:

**Nhiệt độ đỉnh quá thấp.** Attribute `flame` của Mantaflow chạy 0..1 rồi được **nhân**
với ô `Temperature`. Để 1400K thì đỉnh ngọn lửa mới chỉ là đỏ sẫm — ra màu nâu xỉn.
Lửa vàng cam cần quanh **2800K**.

**`burning_rate` cao là lửa tắt ngay.** Nhiên liệu cháy hết ở gốc, phần trên chỉ còn khói
xám. Hạ xuống **0,25–0,32** thì lửa cháy lâu và theo dòng khí lên cao.

**Nội dung chỉ chiếm ~40% ô.** Quanh quả cầu nổ là viền trống, nên một hạt khai báo "1
mét" thì ngọn lửa nhìn thấy chỉ khoảng 0,4 mét — đổi sang flipbook là cả vụ nổ teo đi.
Phải **phóng to hạt 1,7 lần** bù lại.

#### Hai tia sét trong lốc

Mỗi nhịp (`boltInterval` = 0,45 giây) `VfxFactory.TornadoBolt` phát **hai** tia khác kiểu
nhau, không phải hai bản sao của cùng một tia:

| | Tia 1 | Tia 2 |
|---|---|---|
| Nằm ở đâu | bám trên **vỏ** lốc | xuyên dọc **trong lòng** |
| Bán kính hai đầu | 0,85 – 3,10 m | **0,30** – 2,17 m |
| Độ dài | 2 – 7,5 m | **9,5 – 12,8 m** |
| Số đoạn | 16 | 22 |
| Độ giật | 1,5 | 1,05 |

Tia 2 lấy một điểm ở **phần ba dưới** và một điểm ở **phần ba trên** của thân lốc. Chọn
ngẫu nhiên trong cả thân thì nhiều lần hai điểm rơi sát nhau, ra một đoạn cụt ngắn
chẳng thấy gì. Hệ số `TrongLong = 0,35` kéo hai đầu vào **gần trục** để tia không bám vỏ.

**Độ giật phải NHẸ hơn tia ngắn.** Tia 2 dài gấp 3–5 lần tia 1; để nguyên `jitter = 1,5`
thì các đoạn lệch quá nhiều và cả tia **tan thành một đám bụi sáng** thay vì ra đường sét.

> **Không chụp được ngoài Play mode.** `LightningArc` dựng lưới trong `Start()`, mà
> `Start()` không chạy trong Editor — nên mọi ảnh render lốc xoáy ở đây đều **không có
> tia sét**, dù tia vẫn chạy bình thường trong game. Kiểm bằng cách đếm số `LightningArc`
> sinh ra và đo toạ độ hai đầu, chứ đừng tưởng lốc mất tia sét.

#### Chùm tia xòe như nan quạt ở chân lốc

Sau khi đổi mảnh vụn sang khối đá nhỏ, chân lốc lộ ra một **chùm tia trắng xòe ngang
như nan quạt** — trông rất kỳ. Đây không phải lỗi mới: nó **vọn đã có sẵn**, chỉ là
trước đó bị đám mảnh hình thoi to che mất.

Nguyên nhân ở hệ hạt `Vut`, chế độ `Stretch`. `Stretch` kéo mỗi hạt dài ra theo **vectơ
vận tốc tổng**. Ở chân lốc, vận tốc tiếp tuyến (orbital 9–14, tức 9–14 m/s ở bán kính
1 m) **lớn hơn** vận tốc leo lên (5,5–8,5), cộng thêm radial 0,9–2,0 loe ra — nên hướng
vận tốc gần như **nằm ngang**, và tia bị kéo thành vạch ngang.

Chữa bằng cách đảo lại tỉ lệ:

| | Cũ | Mới |
|---|---|---|
| `y` (leo lên) | 5,5–8,5 | **8–12** |
| `orbitalY` (tiếp tuyến) | 9–14 | **5,5–8** |
| `radial` (loe ngang) | 0,9–2,0 | **0,15–0,5** |
| `lengthScale` | 2,4 | **1,5** |
| `velocityScale` | 0,16 | **0,06** |

Leo lên mạnh hơn tiếp tuyến thì hướng kéo thiên về **dọc**, tia bám theo thân lốc. Cũng
nới `radiusThickness` từ 0 lên 0,35 để hạt rải quanh vành thay vì đứng đúng một vòng.

> **Bài học:** sửa một thứ có thể **lộ ra** lỗi cũ vọn bị thứ đó che. Khi người dùng báo
> "không giống bản cũ" sau một thay đổi, đừng mặc định là thay đổi ấy làm sai — ở đây
> velocity của `Vut` được đo lại và **vẫn đúng nguyên như code gốc**.

#### Mảnh đất đá bị lốc cuốn — dựng khối thật trong Blender

Hệ hạt `Grit` trong prefab lốc xoáy dùng `DebrisMat`, mà `DebrisMat` lại dùng
`TextureFactory.IceShard()` — **một hình tam giác nhọn duy nhất**. Cả đám mảnh bị cuốn
lên vì thế đều là những hình thoi giống hệt nhau, đều tăm tắp.

Thay bằng **16 khối đá dựng trong Blender**: mỗi khối là **bao lồi của một chùm điểm
ngẫu nhiên** (`bmesh.ops.convex_hull`), có chiếu sáng nên mỗi mặt một độ sáng khác nhau.
Đo tương phản sáng–tối trong từng mảnh: **trung bình 93**. Vẽ một đa giác phẳng bằng code
thì không bao giờ có được chuyển sáng giữa các mặt như vậy.

**Một lỗi hình học đáng ghi lại.** Lần đầu tôi làm mảnh **dệt theo trục Z**, mà máy quay
lại đặt ở `(0,-6,0)` **nhìn theo trục Y** — nên mặt phẳng của mảnh (XY) vuông góc với
tầm nhìn: khung nào cũng thấy mảnh **từ cạnh**, dệt thành vệt mỏng chiếm có 8% ô. Đổi
chiều dệt sang trục Y (đúng trục đang nhìn theo) thì diện tích lên **17%**.

Cũng đổi `Stretch` → `Billboard` kèm tự xoay: `Stretch` kéo mảnh dài ra theo hướng bay
thành vệt thuôn, mà mảnh đất bị lốc cuốn thì **xoay lộn**.

> **Lỗi console đi thành hai nhóm.** Unity đòi cả ba trục của **mỗi nhóm** đường cong
> vận tốc phải cùng kiểu, và có **hai nhóm riêng biệt**: linear (`x/y/z`) và orbital
> (`orbitalX/Y/Z`). Sửa nhóm orbital xong vẫn còn nhóm linear — mỗi khung hình lại một
> dòng `"Particle Velocity curves must all be in the same mode"`. Và phải sửa ở **cả
> code lẫn prefab**: hai chỗ khác nhau.

#### Chọn ĐÚNG lưới ảnh cho từng việc

Lửa bám người lúc đầu dùng `LuaCuon` — và **vẫn ra hình tam giác**. Lý do: mọi khung
trong `LuaCuon` đều là một **ngọn lửa hình nón** (lửa bốc lên từ một điểm). Dán vào hạt
nhỏ và thưa thì mỗi hạt lộ nguyên cái hình nón ấy ra.

Vùng đất cháy không bị vấn đề này vì ở đó hạt **rất dày** (900 hạt) nên chúng chồng
lên nhau, che hết hình từng hạt. Tăng mật độ lên cho lửa bám người (45 → 135 hạt/giây)
vẫn chưa đủ — vẫn nhìn ra hình nón.

Cách chữa thật: **đổi sang lưới `VuNo`**. Đó là quả cầu lửa **cuộn tròn**, không có
hướng bốc lên, không có đỉnh nón — đúng thứ cần cho lửa bám quanh người. Bài học:
**lưới ảnh phải khớp với cách hạt được dùng**, không phải cứ "lửa" là dùng ảnh lửa.

#### Mảnh vỡ: một ảnh thì ra khuôn, không ra vỡ

Mảnh khiêng trước đây dùng `IceShard()` — **một** hình tam giác duy nhất cho cả trăm
hạt, nên mọi mảnh giống hệt nhau như cắt bằng khuôn. Thay bằng `ManhVoSheet()`:
**16 hình mảnh khác nhau** trong một lưới 4×4, mỗi hạt lấy **một ô cố định**
(`frameOverTime = 0`, `startFrame` ngẫu nhiên) — khác flipbook khói/lửa là chạy lần lượt
qua các ô.

Ba lần sửa mới ra hình mảnh vỡ thật:

**Viền sáng đều quanh chu vi ra viên đá quý.** Phải có **mặt vát** mạnh (một bên hứng
sáng rõ, bên kia tối hẳn) — đó mới là thứ cho mảnh có **bề dày**. Và chỉ **vài cạnh**
bắt sáng, cạnh nào hướng về phía sáng thì rõ, còn lại chìm. Cộng thêm vài **vết nứt**
bên trong.

**Alpha 0,30 cho "trong như mảnh năng lượng" là quá tay** — cả đám mảnh tan thành những
đốm mờ, mất hẳn hình. Kéo lên 0,62.

**Kích thước phải chênh nhau nhiều.** Một cái vỡ thật cho ra vài mảnh lớn lẫn rất nhiều
mảnh vụn. Để khoảng họp (0,12–0,42) thì mảnh nào cũng xấp xỉ nhau. Nới ra 0,07–0,52:
đo được chênh **7,1 lần** giữa mảnh lớn nhất và nhỏ nhất.

Cũng đổi `Stretch` → `Billboard` kèm tự xoay: `Stretch` kéo mảnh dài ra theo hướng bay
thành những vệt thuôn, mà mảnh vỡ thật thì **xoay lộn** trong không khí.

> **Lại vấp bẫy prefab.** Sửa `BuildBurning` trong code là vô ích: có `burningPrefab`
> (`Assets/Prefabs/Vfx_BongChay.prefab`) gán trong **cả hai scene**, nên `AttachBurning`
> luôn dùng prefab và không bao giờ gọi `BuildBurning`. Phải sửa prefab tại chỗ — và
> prefab cần một **Material asset có thật trên đĩa** (`M_LuaBamNguoi.mat`), vì material
> tạo bằng `new Material(...)` lúc chạy sẽ biến mất khi lưu prefab.

#### Vùng lửa nhìn "mờ" — không phải vì thiếu điểm ảnh

Người dùng báo ngọn lửa của Thiên thạch nhìn mờ, **mờ như nhau ở cả camera xa lẫn gần**.
Chi tiết "như nhau ở mọi khoảng cách" là thứ loại trừ giả thuyết độ phân giải ngay từ đầu:
nếu do texture bị kéo giãn thì nhìn xa phải nét.

Đo ở góc camera thật:

```
39,8 px cho moi met o mat dat
hat lua to 0,99 - 2,48 m  ->  chiem 39 - 98 px tren man hinh
texture co 128 px/o       ->  dang bi THU NHO 0,31x .. 0,77x
```

Texture còn **dư** điểm ảnh. Nguyên nhân thật nằm ở chỗ khác — đo trên ảnh chụp vùng lửa:

```
205 hat song cung luc, moi hat to toi 2,48 m, blend cong sang
   vung sang chiem 57,6% khung hinh
   28,6% so diem anh sang co kenh DO KEP o 255
   do net (chenh lech trung binh giua hai diem canh nhau): 3,31
```

Gần một phần ba vùng lửa có kênh đỏ **kẹp ở 255**. Mọi biến thiên trong vùng đó bị cắt
mất — đó là "mờ": không phải thiếu điểm ảnh mà là **thông tin bị bão hoà**.

Cách chữa: hạt **nhỏ hơn** (0,22–0,55 → 0,17–0,42 lần bán kính), **nhiều hơn** để bù độ
phủ (88 → 150 mỗi mét bán kính), và **nhạt hơn** (đỉnh alpha 0,95 → 0,72).

```
                        TRUOC    SAU
kenh DO bao hoa         28,6%   11,4%
do net                   3,31    3,76
```

##### Hai hướng đã thử và bỏ

**Texture 3× từ mô phỏng mới.** Đã dựng lại mô phỏng Mantaflow và render 36 khung ở
384 px/ô (2304×2304, gấp ba). Nhưng ngọn lửa mô phỏng mới là một **cột hẹp**, không phải
khối lửa cuộn rộng như bản gốc — đo độ sáng trung bình của lưới: bản gốc **88,6** với 74%
điểm sáng hơn 2, bản mới chỉ **16,1** và 16%. Nội dung không lấp đầy ô, nên dán vào hạt thì
mỗi hạt ra một mảnh nhỏ rời rạc và **thấy rõ từng billboard hình vuông** — tệ hơn hẳn.
Đã khôi phục bản gốc.

> File Blender gốc của flipbook lửa **không còn** trên máy, nên không render lại đúng ngọn
> lửa cũ ở độ phân giải cao được. Muốn làm lại phải dựng mô phỏng cho ra **khối lửa cuộn
> rộng** chứ không phải cột lửa — nguồn rộng hơn, lực đẩy nhiệt (`beta`) thấp hơn.

**Bật noise cho hệ hạt.** Cái lư lửa trong cảnh nhìn nét hơn hẳn vùng lửa, và nó có bật
`noise` (strength 0,35) trong khi vùng lửa thì không — nên tưởng đó là chỗ khác nhau. Bật
thử: bão hoà **vọt lên 38,1%**, tệ hơn cả bản gốc, độ nét tụt về 3,37. Noise gom hạt lại
thành cụm nên chúng chồng nhau dày hơn. Lư lửa thoát được vì nó chỉ có 150 hạt trong một
cột hẹp, còn vùng lửa có 350 hạt trải trên bán kính 4,5 m.

##### Còn lại chưa chữa

`S_ParticleFlipbookAdd` **không lấy mẫu depth**, nên chỗ billboard cắt mặt đất tạo đường
thẳng cứng — thấy rõ ở đáy đám lửa. Chữa được bằng soft particles, nhưng phải bật
`depthTextureMode` cho camera (đang là `None`), tức thêm một pass render depth.

#### Nơi nào đang dùng flipbook

| Hiệu ứng | Lưới ảnh | Hệ số bù |
|---|---|---|
| Khói bụi sau Lốc xoáy | `KhoiCuon` | 1,0 |
| Vùng đất cháy Thiên thạch | `LuaCuon` | 1,0 |
| Thiên thạch chạm đất | `VuNo` | **1,20** |
| Đuôi lửa + lửa viền thiên thạch | `LuaCuon` | **0,90** |
| Nhân vật bị thiêu đốt | `LuaCuon` | **0,90** |
| Khói tím lúc Khiêng vỡ | `KhoiCuon` tô tím | **0,75** |

**Một lưới ảnh dùng được hai màu.** Khói tím lúc Khiêng vỡ dùng lại **đúng** lưới
`KhoiCuon` của Lốc xoáy, chỉ đổi tint. Được vì lưới ấy render **trắng**. Nếu hồi ấy
render khói màu đen thì bây giờ đã phải mô phỏng lại cả 36 khung chỉ để đổi màu.

#### Hệ số bù phải ĐO, không đoán

Nội dung trong mỗi ô flipbook không lấp kín ô như texture cũ, nên một hạt khai báo
"1 mét" vẽ ra không còn đúng 1 mét. Phải bù lại — nhưng bằng số đo.

Cách đo: **bán kính hiệu dụng** = bán kính hình tròn có cùng **diện tích** với phần nhìn
thấy được của ảnh (cộng dồn alpha), chia cho nửa bề rộng ô. Hệ số bù = bán kính của
ảnh cũ chia cho bán kính của khung flipbook.

Tôi đã đoán nhầm một lần: nhìn lưới ảnh thấy quả cầu nổ nằm gọn giữa ô nên đặt
1,7 — đo ra chỉ **1,18**. Ngược lại, `LuaCuon` và `KhoiCuon` thực ra **to hơn** texture
cũ (0,88 và 0,71), tức phải thu nhỏ chứ không phóng to.

Một lưu ý nữa cho khói tím: để hạt 0,55–1,20 lần bán kính khiêng (1,1–2,4 mét) cộng 38
hạt bụng ra cùng lúc thì đám khói **trùm kín cả khung hình**, không còn thấy mảnh vỡ lẫn
nhân vật. Kéo về 0,26–0,58 và 20 hạt: độ che kín **13,1% → 0,4%**.

#### Làm mờ mép từng ô

Khung nào có lửa/khói tràn sát mép ô thì khi dán vào hạt sẽ thấy một **cạnh thẳng cắt
ngang** — lộ ngay ra là ảnh vuông. Thay vì render lại cả loạt, nhân alpha với một mặt nạ
mờ dần ở **7 điểm ảnh ngoài cùng** của mỗi ô: cạnh biến mất mà phần giữa ô không bị
động đến. Đo số điểm sát mép: `LuaCuon` **22 → 0**, `VuNo` **506 → 0**.

#### Dựng flipbook trong Blender (qua MCP)

Mô phỏng khói Mantaflow, camera **trực giao** (không phối cảnh — mọi ô phải nhìn từ đúng
một góc), nền trong suốt, khói **trắng** để Unity tô màu sau.

Bốn cái bẫy đã vấp:

**Tên bị dịch theo ngôn ngữ giao diện.** Blender này chạy tiếng Việt: modifier tên
"Chất Lỏng" chứ không phải "Fluid", node là "Thể Tích Nguyên Tắc" chứ không phải
"Principled Volume". Phải tra theo `type` / `bl_idname`. Riêng tên **socket** thì Blender
giữ nguyên tiếng Anh.

**Nguồn phun lọt vào khung hình.** Quả cầu emitter hiện ra thành chấm sáng giữa ảnh —
phải `hide_render = True`.

**Khói tự phát sáng thì mất khối.** Để `Emission Strength` cao là khói sáng đều, xóa sạch
bóng đổ **bên trong** khối khói — mà chính bóng ấy mới cho đám khói có khối. Phải tắt
emission, bật `use_volumetric_shadows`, và để đèn làm việc. Đo tương phản trước/sau:
**0,0 → 0,592**.

**Khói nở mãi không co lại.** Đến khung 72 nó trùm kín ô và bị cắt vuông ở mép — dán
vào hạt là lộ ngay ra ô vuông. Chữa bằng cách lấy 36 khung **liên tiếp** kết thúc ở khung
40 (lúc đám khói chiếm hơn nửa ô mà vẫn nằm gọn), thay vì trải đều tới khung 72.

Hai chi tiết khi nhập vào Unity: **tắt mipmap** (mức mip thấp trộn các ô kề nhau vào
nhau), và **tắt `npotScale`** (Unity mặc định kéo 768 lên 1024, làm mỗi ô thành 170,67
điểm ảnh — không tròn).

> **Giới hạn của máy này.** Dự án là **Built-in RP** (không có URP/HDRP trong
> `Packages/manifest.json`) nên **VFX Graph và Shader Graph không dùng được** — cả hai đòi
> Scriptable Render Pipeline. Blender ở đây cũng chỉ có **EEVEE** (không có Cycles) và
> chạy trên GPU tích hợp, nên render volume khá chậm. Đó là lý do chọn flipbook thay vì
> đổi pipeline.

#### Khiêng bất tử — chặn được hết nhưng không bao giờ mất máu

Khiêng chặn được mọi đòn nhưng máu khiêng **không hề giảm**. Nguyên nhân là hai con
số đứng cạnh nhau mà không ai đối chiếu:

| | Tầm với | So với bán kính khiêng 3,04 m |
|---|---|---|
| Đạn phù thủy nổ | bán kính **2,6 m** | **nhỏ hơn** → không với tới người chơi |
| Bộ xương đánh gần | 2,4 × 1,35 = **3,24 m** | quái bị vách đẩy ra 3,44 m → không tới |
| Quỷ lùn | 1,5 × 1,35 = **2,03 m** | không tới |

Đạn nổ **trên mặt vòm**, mà bán kính nổ nhỏ hơn bán kính khiêng — nên người chơi đứng
giữa **không bao giờ nằm trong tầm sát thương**, `TakeDamage` không hề được gọi, nên
`Khieng.HapThu` cũng không. Quái đánh gần thì bị chính cái vách chặn đẩy ra ngoài tầm tay.

**Chữa ở hai đường:**

`Khieng.NoTrungKhieng` — vụ nổ chạm tới mặt vòm thì trừ thẳng máu khiêng. Bỏ qua khi chủ
khiêng **đã nằm trong tầm nổ** — lúc đó đòn đã đi qua `TakeDamage` rồi, tính thêm lần nữa
là trừ hai lần. Giữ sẵn một **danh sách tĩnh** các khiêng đang bật thay vì quét cả cảnh mỗi
lần nổ.

`EnemyAI.TamDanhHieuDung` — nới tầm đánh thêm đúng bán kính khiêng để quái đánh **vào
vòm**. Điều kiện phải là `dist − bán_kính > attackRange × 1,35`, không phải
`dist > (attackRange + bán_kính) × 1,35` — cách sau nhân 1,35 vào cả bán kính, cho quái
thừa ra **1,06 m** tầm với.

Đo lại toàn chuỗi:

| Phép thử | Kết quả |
|---|---|
| Đạn nổ trên vòm (16 sát thương) | khiêng 150 → **134**, người chơi **170 không đổi** |
| Bộ xương đánh (14 sát thương) | khiêng 134 → **120**, người chơi **170 không đổi** |
| Đánh liên tiếp | **8 đòn** thì khiêng vỡ, người chơi **vẫn 170** |
| Đánh thêm sau khi vỡ | **0** khiêng trúng (đã rời khỏi danh sách) |

#### Viền vòm phát sáng

Số mũ fresnel **nhỏ = viền DÀY hơn**. Để 2,4 thì viền teo lại thành một sợi chỉ ở mép,
gần như không thấy.

| | Cũ | Mới |
|---|---|---|
| `_VienManh` | 2,4 | **1,6** |
| `_VienDam` | 0,30 | **0,50** |
| `_VienChoi` (độ chói riêng) | 2,2 | **3,0** |

Độ sáng vành ngoài đo được: **61 → 88 (+44%)**. Tách `_VienChoi` ra riêng để chỉnh **độ
chói** của viền mà không phải tăng độ đặc — tăng độ đặc thì viền che mất nhân vật.

#### Sáng hơn mà không bớt trong

Độ **trong suốt** do `o.Alpha` quyết định, độ **sáng** do `o.Albedo` — hai thứ độc lập.
Muốn sáng hơn mà vẫn trong như cũ thì **chỉ được động vào màu**: `_PhatSang` nhân thẳng
vào `o.Albedo`, công thức alpha không bị đụng tới.

Tăng alpha cũng làm sáng lên thật, nhưng đồng thời **che cảnh phía sau nhiều hơn** —
tức không còn trong như cũ.

Đo lại (tắt đèn khiêng để con số chỉ phản ánh riêng cái vòm):

| | Giá trị |
|---|---|
| Lượng sáng vòm thêm vào cảnh | **+35,1%** |
| Diện tích vùng khiêng che | **+0,04%** (không đổi) |

Diện tích che không đổi là bằng chứng độ trong giữ nguyên: khiêng sáng hơn nhưng không
trùm rộng hơn, không lấn thêm cảnh vật nào.

Đèn hắt ra chung quanh cũng lên **1,20 → 1,62** (×1,35) để "toàn bộ khiêng" sáng đều.

#### Nới bề ngang mà giữ nguyên chiều cao

Chiều cao vòm = **bán kính × `CaoVomKhieng`**. Nên nới bán kính 15% mà để nguyên tỉ lệ
thì vòm **cao lên theo 15%** — không còn là "giữ nguyên chiều cao" nữa. Phải chia tỉ lệ
cho đúng cùng hệ số:

| | Cũ | Mới |
|---|---|---|
| `khiengBanKinh` | 2,64 m | **3,036 m** (×1,15) |
| `CaoVomKhieng` | 1,26 | **1,0957** (÷1,15) |
| Bề ngang đo được | 5,28 m | **6,07 m** (+15,0%) |
| Chiều cao đo được | 3,33 m | **3,33 m** (−0,1%) |

**Vách chặn quái tự nới theo** — `SphereCollider` lấy thẳng `banKinh`, đo lại đúng
3,036 m. Không phải chỉnh tay.

> Đổi bán kính thì phải sửa **bốn chỗ**: code, prefab `Player_Sorceress`, `Act2.unity`,
> và tỉ lệ trong `GameHUD.KhiengIcon`. Ba chỗ đầu vì giá trị đã serialize đè lên code;
> chỗ cuối vì biểu tượng vẽ vòm theo tỉ lệ cao/rộng của vòm thật (nay là **0,55**).

#### Khiêng màu tím — tách lõi và quầng

Tia sét thật **không một màu**: lõi cháy trắng gần như thuần, chỉ cái quầng bao
quanh nó mới ăn màu. Tô cả sợi một màu tím thì ra sợi dây nhựa tím, không ra tia
điện. Nên hàm vẽ tia trả về **hai giá trị riêng** — lõi và quầng — rồi mỗi phần ăn
một màu:

| Lớp | Màu | Vai trò |
|---|---|---|
| `_LoiColor` | `(1,00 · 0,93 · 1,00)` gần trắng | sợi cháy ở giữa tia |
| `_GanColor` | `(0,62 · 0,24 · 0,98)` tím | quầng bao quanh tia |
| `_VienColor` | `(0,58 · 0,28 · 0,95)` tím | viền vòm |
| `_MangColor` | `(0,85 · 0,78 · 1,00)` tím rất nhạt | màng phủ cả vòm |

Màng phải **nhạt hơn hẳn** các lớp kia: nó phủ kín vòm nên tô đậm màu là cảnh vật
nhìn xuyên qua bị nhuộm tím hết.

**Tia phải thẳng hơn.** `_GanNhip` hạ từ 0,72 xuống **0,30** — mẫu chuẩn là những tia
chạy dọc gần như thẳng đứng, chỉ gãy khúc vụn chứ không lượn sóng. Bù lại, biên độ
giữa các tầng nhiễu giảm **chậm** hơn (0,66 thay vì 0,52) để các tầng mịn còn đủ sức
nặng — chính chúng tạo ra những nét gấp vụn sắc cạnh.

**Nhánh rẽ** (`_SetNhanh`): một lớp tia thứ hai dày gấp đôi, lệch nửa bước, và chỉ
hiện trong **từng đoạn ngắn** theo độ cao — đoạn nào hiện thì một hàm nhiễu quyết định.

Màu tím cũng được áp cho **đốm sáng trong vòm, đèn hắt ra chung quanh, mảnh vỡ khi
khiêng nổ**, và **biểu tượng kỹ năng**. Biểu tượng trước đó vẫn là quả cầu xanh dương
phủ lưới lục giác — sai cả hình dạng lẫn màu. Vẽ lại thành vòm tím có tia sét, tỉ lệ
cao/rộng lấy theo vòm thật (**0,63**). Mỗi lần đổi chiều cao vòm thì phải đổi cả ở đây,
không thì biểu tượng vẫn là cái vòm dẹt cũ.

#### Làm tia sáng hơn: hai ngõ cụt trước khi tìm ra chỗ nghẽn

Yêu cầu là tia sáng hơn 30%. Hai cách hiển nhiên đều không ăn thua:

**Tăng độ chói màu (`_TiaChoi`)** — đẩy từ 3,2 lên 5,2 chỉ được **+17,8%**. Vì
`o.Albedo` là `fixed3`, giá trị vượt 1 bị **kẹp**; lõi tia đã trắng bão hòa nên tô sáng
thêm bao nhiêu cũng vậy.

**Tạo `_TiaSang` để tia không chịu hệ số trong suốt của màng** — chỉ **+6,7%**. Đúng
hướng nhưng không phải chỗ nghẽn.

Con số chỉ ra chỗ nghẽn thật: **chỉ 125 trên 22 000 điểm tia** đạt gần trắng. Lõi đã
bão hòa nhưng **quá mảnh** (bề dày 0,014), còn phần lớn diện tích là quầng mờ. "Sáng
hơn" ở đây phải là **lõi dày hơn và quầng rộng hơn**, không phải màu chói hơn.

| | Cũ | Mới |
|---|---|---|
| `_GanDay` (bề dày lõi) | 0,014 | **0,024** |
| `_SetQuang` (bề rộng quầng) | 4,2 | **5,0** |
| `_TiaSang` (độ đặc riêng của tia) | 0,519 | **0,74** |

Đo trên hai góc camera: **+31,5%** và **+29,5%**, trung bình **+30,5%**. Độ đục của màng
hầu như không đổi (26,0 → 26,1) — tức chỉ tia sáng lên, khiêng vẫn trong như cũ.

#### Mảnh vỡ phải có vật liệu riêng

Đổi `startColor` của hạt sang tím mà mảnh vỡ vẫn xanh. Vì `ShardMat` là vật liệu
**dùng chung với MƯA BĂNG**, đã tô sẵn màu xanh băng `(0,70 · 0,90 · 1,00)` — mà vật liệu
hạt **nhân** với màu hạt, nên tím × xanh băng ra xanh nhạt. `RingIceMat` và ánh nổ cũng
vậy.

Phải tạo vật liệu **riêng** cho khiêng: `ShardKhiengMat` và `RingKhiengMat`. Đo màu mà
mảnh vỡ thêm vào khung hình: **(105,4 · 43,7 · 122,4)** — xanh lam và đỏ vượt xanh lá rõ
rệt, đúng là tím.

Cả hai đầu gradient của mảnh vỡ đều phải ngả tím. Để một đầu trắng thì một nửa số
mảnh bay ra màu trắng, nhìn ra mảnh thủy tinh chứ không ra mảnh của cái khiêng tím.

#### Vân là TIA SÉT, không phải đường cong trơn

Đường đi của mỗi tia được dựng bằng **nhiễu gãy khúc bốn tầng**: tần số gấp đôi và biên độ
giảm một nửa mỗi tầng. Tầng thô cho hướng đi chung, các tầng mịn thêm những nếp gấp nhỏ —
đúng cách một tia sét thật phân nhánh.

Điểm mấu chốt: hàm nhiễu dùng ở đây **bỏ bước làm mượt** (`smoothstep`). Nhiễu mượt cho ra
đường cong tròn trịa như sợi tơ; bỏ làm mượt đi thì đường nối giữa các điểm thành đoạn
thẳng, gấp khúc ở mỗi điểm — mới ra dáng tia điện.

Mỗi tia có **lõi chói** rất mảnh bọc trong một **quầng mờ** rộng gấp 3,4 lần. Chỉ vẽ lõi
thôi thì ra sợi chỉ, không ra tia điện.

**Chớp tắt riêng từng tia**: nhịp nháy lấy theo số thứ tự tia, nên chúng không nhấp nháy
đồng loạt.

#### Trong suốt hơn nhưng vẫn sáng như cũ

Với phép trộn thường, màu nhìn thấy = `màu × độ_đặc`. Hạ độ đặc xuống 70% mà không bù thì
khiêng mờ hẳn đi. Nên màu được **chia cho `_TrongSuot`**: tích `màu × độ_đặc` giữ nguyên —
tức vẫn sáng y như cũ — trong khi phần cảnh vật phía sau lọt qua được nhiều hơn 30%.

Đo lại độ lệch màu mà vòm gây ra cho cảnh phía sau: **0,309 → 0,157**.

#### Shader khiêng — ba lỗi tôi vấp liên tiếp

Cả ba đều cho ra **cùng một triệu chứng**: quả cầu trắng đục kín mít, không thấy vân,
không thấy viền, không thấy nhân vật bên trong.

1. **Khai báo `Blend SrcAlpha One` cùng lúc với `alpha:fade`.** Chỉ thị `alpha:fade` đã
   tự đặt blend thường rồi; đặt thêm blend cộng sáng thì hai cái đá nhau, mà cộng sáng
   lại nhân đôi vì `Cull Off` vẽ cả hai mặt.

2. **Tính fresnel bằng `dot(viewDir, o.Normal)`.** Trong surface shader, `IN.viewDir` nằm
   ở **không gian tiếp tuyến**, mà trong không gian đó pháp tuyến bề mặt luôn là
   `(0,0,1)`. Còn `o.Normal` chỉ có giá trị nếu mình tự ghi vào — không ghi thì nó bằng 0,
   `dot()` ra 0, viền sáng phủ **toàn bộ** quả cầu.

3. **Quên `noforwardadd`.** Hàm chiếu sáng tuỳ biến trả thẳng `Albedo` ra, mà Unity chạy
   thêm một pass ForwardAdd cho **mỗi ngọn đèn** — cảnh này có ba ngọn (Moonlight,
   HeroLight, và đèn của chính cái khiêng), nên màu bị cộng ba lần.

Đo bằng số điểm gần trắng trên ảnh: **27,2% → 20,5% → 0,1%**.

#### Khiêng tự bật dù không bấm nút

Hai triệu chứng đi cùng nhau: vào game là khiêng đã bật sẵn, và khiêng vỡ rồi lại hiện
ra như chưa hề vỡ. Ba nguyên nhân độc lập, đã sửa cả ba:

**1. `mau` mặc định là 150.** `DangBat` đọc `mau > 0`, nên hễ component `Khieng` có mặt
trên nhân vật là khiêng bật — kể cả khi chưa ai gọi `Bat`. Đổi mặc định về **0**: có
component không có nghĩa là khiêng đang bật.

**2. `hinh` không được lưu vào cảnh.** Biến private này luôn bằng `null` sau khi vào
Play mode, trong khi GameObject con `"Khieng"` từ lần trước **vẫn còn đó**. Mỗi lần bật
lại là chồng thêm một vòm nữa, và vòm cũ không còn ai tắt được. `Dung()` giờ dọn sạch
mọi con tên `"Khieng"` / `"VachChanKhieng"` trước khi dựng vòm mới.

**3. Chuỗi `if/else if` kết thúc bằng `else` trơn.** Cả `CastAt` lẫn `Release` đều để
nhánh khiêng ở `else` cuối, nên **mọi số hiệu kỹ năng lạ đều bật khiêng**. `castingSkill`
khởi tạo bằng `-1`, và `-1` rơi đúng vào nhánh đó. Giờ kiểm tra hẳn `== 5`, còn giá
trị lạ thì ghi cảnh báo ra console thay vì âm thầm bật một kỹ năng.

> **Cảnh báo cho lần sau.** Nguyên nhân gốc của lần này là các **lệnh đo chạy trong
> Editor**: chúng gọi `Khieng.Bat` trên **cảnh đang mở**, không phải một bản sao. File
> `.unity` trên đĩa vẫn sạch vì chưa lưu, nhưng **Play mode chạy bản trong bộ nhớ**. Kiểm
> tra bằng `FindObjectsByType` chứ đừng chỉ grep file cảnh.

#### Hồi chiêu tính từ lúc bấm

Đồng hồ hồi chiêu được đặt **ngay lúc bấm phím**, trước cả lúc bắt đầu niệm chú, và
chạy độc lập hoàn toàn với vòng đời của thứ mà phép sinh ra. Cơn lốc sống 6 giây hay
60 giây cũng không dính gì tới lúc bạn được tung con tiếp theo.

> ⚠️ **Sửa con số hồi chiêu trong code là chưa đủ.** Các con số này đã được lưu vào
> prefab `Player_Sorceress` **và** vào cả hai cảnh; giá trị lưu đó **đè lên** giá trị
> mặc định trong code. Tôi đã vấp đúng một lần: đổi `tornadoCooldown` từ 13 xuống 2
> trong code, chạy game vẫn thấy 13 giây, tưởng là lỗi cách tính hồi chiêu. Đọc giá trị
> thật lúc đang chơi mới ra: **lốc xoáy 13 giây, thiên thạch 2 giây** — thiên thạch đúng
> vì nó là trường mới, prefab chưa kịp lưu nên lấy giá trị code.
>
> Sửa số thì phải sửa **cả ba chỗ**: code, prefab, và cả hai cảnh.

> Hồi chiêu 2 giây nhưng **năng lượng mới là thứ giữ nhịp**: 60 mỗi lần trên tổng 130,
> hồi 9/giây — bắn hai phát liên tiếp là cạn, phải chờ khoảng 7 giây mới đủ phát thứ ba.
> Lốc xoáy cũng vậy, và vì mỗi cơn lốc sống 6 giây nên với hồi chiêu 2 giây bạn có thể
> có **ba cơn cùng lúc** trên sân nếu đủ năng lượng.

#### Một lần gọi là BA quả

`ThienThach.SpawnLoat` sinh cả loạt một lúc, mỗi quả mang một **độ trễ** khác nhau:
0,00 / 0,70 / 1,40 giây.

**Quả đang chờ lượt phải VÔ HÌNH.** Khối đá và đuôi lửa chỉ được dựng khi đến lượt rơi
(`daHienHinh`). Dựng ngay từ đầu thì hai quả sau **treo lơ lửng trên trời chờ sẵn**, người
chơi ngửa lên là thấy.

**Không tính tuổi trong lúc chờ.** `age` chỉ tăng sau khi hết trễ — trừ ngay từ đầu thì
quả cuối có thể chạm trần `SongToiDa` và tự nổ trên không ngay khi vừa bắt đầu rơi.

**Ba quả không rơi trùng một chỗ.** Quả đầu đúng điểm ngắm, hai quả sau lệch ra chung
quanh trong bán kính `tanRong` = 2,8 m. Đo điểm đập thực tế: cách điểm ngắm **0 / 1,97 /
2,22 m**, cách nhau từng đôi **2,07 / 2,30 / 2,99 m**. Rơi chồng lên nhau thì ba vụ nổ gộp
thành một, nhìn ra một quả to chứ không ra một loạt.

> **Cẩn thận khi đo.** Lần đầu tôi đo "lệch ngang" bằng `transform.position` và ra 5,7–7,5 m
> — nhưng đó là vị trí **sinh trên cao**, lệch vì đường rơi xiên (`doNghieng` = 7 m), không
> phải điểm chạm đất. Phải đo `DiemDich` mới ra con số đúng.

### ĐIỀU KHIỂN CẢM ỨNG (web / iOS / Android)

Máy cảm ứng dùng bố cục riêng; **PC không đổi gì**. Cả hai cùng một `GameHUD`, rẽ nhánh ở
`CamUng.DangDung`.

| | PC | Cảm ứng |
|---|---|---|
| Di chuyển | WASD + bấm chuột để đi | **chỉ cần joystick** (góc dưới trái) |
| Kỹ năng | thanh ô vuông giữa dưới | **7 nút tròn** góc dưới phải, chỉ hình không tên |
| Ngắm | theo con trỏ chuột | **giữ nút rồi kéo** để ngắm, hoặc chạm-thả để tự tìm |

#### Giữ — kéo — thả: ngắm có định hướng

Chạm vào một nút kỹ năng **không đánh ngay nữa**. Nút chỉ ghi nhận ngón tay, rồi chờ xem
ngón đó làm gì:

| Người chơi làm gì | Game làm gì |
|---|---|
| Chạm rồi thả ngay tại chỗ | đánh **kẻ địch gần người chơi nhất** |
| Giữ, kéo ra khỏi ô nút, rồi thả | đánh **đúng hướng và đúng cự ly** đã kéo |

Trong lúc kéo, một **đường ngắm vẽ thẳng trên mặt đất**: hướng đi, tầm xa và vòng gây sát
thương. Mỗi kỹ năng vẽ một kiểu riêng — xem bảng ở mục dưới.

**Ngưỡng "đã kéo ra" là bán kính nút.** Ngón tay ai chẳng nhích vài điểm ảnh khi nhấc lên;
lấy ngưỡng nhỏ hơn thì mọi cú chạm nhanh đều hoá thành "ngắm về một hướng ngẫu nhiên".

> Và **một khi đã ra khỏi nút thì không quay lại được**. Kéo ngược ngón tay về gần nút để
> ngắm một mục tiêu đang đứng sát bên là chuyện bình thường; cho cờ ấy tắt đi thì cú đó hoá
> ra "chạm rồi thả" và kỹ năng bay vào một con hoàn toàn khác.

Cự ly quy về 0–1 bằng `(độ dài kéo − bán kính nút) / (300·s − bán kính nút)`. **Trừ bán kính
nút trước khi chia**: không trừ thì vừa nhấc ngón ra khỏi mép nút đã được 0,35 rồi, mất sạch
một phần ba đoạn dùng để chỉnh cự ly. Con số 300 chọn theo **tầm với của ngón cái**, không
theo kích thước màn hình.

**Đòn thẳng thì kéo dài ngắn không đổi tầm.** Quả cầu lửa, Lốc xoáy, Giựt sét luôn bay hết
tầm — kéo chỉ để chọn *hướng*. Chỉ Mưa băng, Sấm sét, Thiên thạch mới lấy độ dài kéo làm
khoảng cách; đó là ba phép rơi xuống một điểm.

#### Mỗi kỹ năng một đường ngắm riêng

`ChiBaoNgam` dựng một lưới **bám theo mặt đất** — mỗi đỉnh bắn một tia xuống đất lấy độ cao,
giống `GroundRing`. Vẽ trong thế giới chứ không phải bằng `OnGUI`: một làn đường 14 m vạch
trên đất gồ ghề thì phải bám theo đất mới đọc được nó tới đâu.

| Kỹ năng | Kiểu | Vẽ gì | Tầm | Bán kính sát thương |
|---|---|---|---|---|
| Quả cầu lửa | thẳng | làn + trục + **hai tia phụ lệch 11°** + ba vòng nổ | 14 m | 3,40 m |
| Mưa băng | theo điểm | vành tầm + vòng bão + dấu cộng | 12 m | 5,50 m |
| Sấm sét | theo điểm | vành tầm + vòng giông + dấu cộng | 12 m | 6,00 m |
| Lốc xoáy | thẳng | làn + trục + vòng bắt ở cuối | 12 m | 3,60 m |
| Thiên thạch | theo điểm | vành tầm + vòng nổ + dấu cộng | 18 m | 4,20 m |
| Khiêng | quanh mình | một vòng quanh nhân vật, **không có hướng** | 3,04 m | 3,04 m |
| Giựt sét | thẳng | làn + trục + vòng lan ở cuối | 19,5 m | 2,20 m |

Hai tia phụ của Quả cầu lửa không phải trang trí: `Fireball.SpawnChum` bắn **ba quả toả 11°**.
Vẽ mỗi một đường thẳng thì người chơi tưởng nó chỉ bắn một quả.

> **Bán kính đọc từ PREFAB, không lấy hằng số trong code.** Giá trị lưu trong prefab đè lên
> giá trị mặc định của script. Lấy hằng số thì đường ngắm vẽ vòng 5,5 m trong khi phép thật
> nổ 7 m — người chơi ngắm theo vòng vẽ rồi cứ trượt hoài mà không hiểu vì sao.

#### Đường ngắm gãy vụn vì bám nhầm vào cây và đá

Ảnh người chơi gửi: đường ngắm **bẻ gãy, leo lên thân cây, xoè ngang ra** giữa chừng, và
từng khúc bị vật cản che mất.

Hai lỗi khác nhau chồng lên nhau.

**Một — tia lấy độ cao bắn trúng nóc cây.** Mỗi đỉnh của lưới bắn một tia xuống đất để lấy
độ cao. Mặt nạ ban đầu là `Ground` **cộng `Default`**. Đếm lại trong màn:

```
Terrain "MatDat"   lop 8 (Ground)     1 collider
cay, da, bia mo    lop 0 (Default)  154 collider
```

Tia trúng nóc tảng đá hay thân cây thì đỉnh ấy **nhảy vọt lên**, còn đỉnh bên cạnh vẫn nằm
dưới đất — mặt lưới xé toạc ra. Đo bằng chiều cao khối bao của lưới:

```
                   truoc        sau
Qua cau lua       5,80 m      0,72 m     <- 5,8 m la chieu cao mot goc cay
Mua bang          1,50 m      1,46 m
Thien thach       1,80 m      1,81 m
```

Sửa: mặt nạ chỉ còn `Ground`. Địa hình thật chỉ có **một** collider và nó nằm ở lớp ấy.

**Hai — vật cản che khuất đường ngắm.** Đường ngắm là một thứ của **giao diện**, không phải
vật thể trong thế giới: nó phải đọc được kể cả khi chạy xuyên gốc cây hay khi người chơi
đứng sau tảng đá. Shader hạt thường dùng `ZTest LEqual` nên cây và đá ăn mất từng khúc.

Thêm `S_ChiBaoNgam.shader` — y hệt `ParticleAdditive` chỉ khác **một dòng `ZTest Always`**,
và `Queue` đẩy lên `Transparent+100` để lửa khói không phủ lên nó.

> Shader này nạp bằng `Shader.Find` lúc chạy nên **bắt buộc phải có trong Always Included
> Shaders** — đã thêm, danh sách từ 22 lên **23**. Thiếu nó thì Editor vẫn đẹp còn bản build
> mất ZTest, và đường ngắm lại bị che khúc — nên đường lui có kèm một dòng `LogError` thật to.

Bỏ 154 collider khỏi mặt nạ còn làm tia dò nhanh hẳn lên:

```
                truoc toi uu   sau gop dinh   sau doi mat na
Qua cau lua       0,601 ms       0,428 ms        0,232 ms
trung binh ca 7      -              -            0,143 ms
```

#### Vừa chạy vừa ngắm — và camera phải đứng yên

Hai ngón cùng chạm màn hình là chuyện bình thường: tay trái giữ joystick, tay phải kéo kỹ
năng. Mỗi ngón **nhận vai đúng một lần**, ngay lúc nó chạm xuống, và giữ vai đó cho tới khi
nhấc lên — cần trước, nút sau, còn lại mới là vùng trống.

Ngón đang ngắm được ghi vào danh sách `ngonNut`, nên nó không bao giờ bị tính là cử chỉ xoay
góc nhìn. Nhưng như thế **vẫn chưa đủ**: người chơi chạm thêm ngón thứ ba vào vùng trống —
rất dễ xảy ra khi hai tay đang bận — thì ngón đó vẫn xoay được camera, và cả đường ngắm đang
vẽ quay theo một cái. Nên khi `mayNgam.DangNgam` thì **khoá hẳn** cả xoay lẫn phóng to, bằng
bất cứ ngón nào. Chặn đặt **sau** đoạn đọc joystick, nên người chơi vẫn di chuyển được.

#### Máy trạng thái tách riêng để kiểm thử được

`MayNgam` không đụng tới `Input`, `Screen` hay `Time` — chỉ nhận toạ độ. Nhờ vậy bộ tự kiểm
gọi thẳng nó bằng toạ độ bịa ra; không thể kiểm thử một hàm đọc `Input.GetTouch`.

Nó cũng là chỗ **duy nhất** giữ logic ấy: HUD có hai đường vào giống hệt nhau về ý nghĩa —
ngón tay thật, và con chuột để thử trên PC bằng F9. Viết hai lần thì sửa một bên quên bên
kia, mà lỗi kiểu đó chỉ lộ ra trên điện thoại thật.

```
12/12 phep thu DAT:
  cham-tha tai cho -> TuDong          run tay 22px van TuDong
  keo 200px -> TheoHuong              ra roi keo ve gan van TheoHuong
  Xa01 o mep nut = 0                  Xa01 het tam = 1
  Xa01 keo qua van kep 1              Xa01 giua = 0,5
  Huy -> khong con ngam               Tha khi khong ngam -> Khong
  Tha xong Lech van doc duoc          nho dung so ky nang
```

#### Số đo trong Play mode

```
BAN THEO HUONG DA NGAM
  Mua bang ngam +X het tam (12 m), bia dat dung do
     bia XA  (12 m theo +X)  mat 559,6 mau
     bia GAN (3 m, nguoc huong) mat 0,0 mau

CHAM ROI THA -> DANH CON GAN NHAT
  nhan vat dang quay mat ve +X, bia XA nam dung huong do
     bia XA  (12 m, dung huong mat)  mat   0,0 mau
     bia GAN (3 m, lech 90 do)       mat 179,7 mau
  -> chon dung con GAN NHAT, khong theo huong dang nhin

GIA THANH DUNG LUOI, moi khung hinh (PC)
                 truoc toi uu    sau toi uu
  Qua cau lua      0,601 ms        0,428 ms   (602 -> 344 dinh)
  Mua bang         0,422 ms        0,170 ms
  Thien thach      0,443 ms        0,177 ms
  Khieng           0,180 ms        0,089 ms
  (mot khung hinh 60 hinh/giay = 16,7 ms)
```

> **`MeshFilter.sharedMesh`, KHÔNG phải `.mesh`.** Getter của `.mesh` **nhân bản** lưới ra
> một bản riêng rồi gán bản đó vào `MeshFilter`. Chỉ cần một ai đó đọc `mf.mesh` một lần —
> một đoạn đo, một dòng gỡ lỗi — là từ đó `MeshFilter` vẽ bản sao, còn code vẫn sửa tiếp lưới
> gốc. Tôi vấp đúng bẫy này: phép đo đầu tiên cho **cả bảy kỹ năng ra y hệt một hình**.

> ⚠️ Đo phép thuật trong Play mode thì **đừng tắt `PlayerController`** để cố định nhân vật:
> `HandleCasting` nằm trong `Update` của chính nó, tắt đi thì `castTimer` không giảm,
> `Release()` không bao giờ chạy, và phép "đã đánh" mà không có gì bay ra. Và **bia phải bật
> `CharacterController`** — đó chính là collider của con quái, tắt đi thì `AreaDamage` nổ
> ngay trên đầu bia cũng không trúng ai.

**Khóa chạm-để-đi.**

**Nhận biết nền tảng.** `Application.isMobilePlatform` trả về **false cho WebGL dù đang
chạy trên điện thoại** — nên phải hỏi thêm `Input.touchSupported` cho trường hợp WebGL.
Có cờ `epCamUng` để thử ngay trên PC.

**Khóa chạm-để-đi.** Trên cảm ứng, đoạn "bấm chuột để đi tới đó" bị tắt hẳn. Để nguyên
thì mỗi lần chạm vào sân để bấm nút hay xoay máy quay, nhân vật lại lao về phía đó. Trên
WebGL, trình duyệt báo **mọi cú chạm màn hình thành một cú bấm chuột trái**, nên không
chặn ở đây thì chạm vào đâu nhân vật cũng chạy đến đó.

Khóa bằng **hai lớp**, không phải một:

```csharp
// Lop 1 - dau HandleMovement, vo dieu kien, truoc khi doc bat cu thu gi
if (CamUng.DangDung) hasMoveTarget = false;

// Lop 2 - ngay tai cho dat diem den
if (!CamUng.DangDung && Input.GetMouseButton(0) && !IsPointerOverSkillBar())
```

Lớp 2 là chỗ sửa hiển nhiên. Lớp 1 mới là chốt chặn: dù sau này có chỗ nào khác đặt
`moveTarget` — AI dẫn đường, một nút trong menu, hay chính lớp 2 sót lại — thì khung hình
sau nó cũng bị xoá, nhân vật không thể tự chạy đi. Hiện toàn dự án chỉ có **đúng một** chỗ
đặt `hasMoveTarget = true`, nhưng chốt chặn không phụ thuộc vào điều đó còn đúng mãi.

#### Camera trên cảm ứng: chặn cửa giả, mở cửa thật

Lỗi người chơi gặp: đang **giữ joystick** mà bấm nút kỹ năng thì góc nhìn tự nhảy. Người
chơi trên điện thoại làm gì có chuột phải hay con lăn — nhưng **trình duyệt vẫn gửi chúng
vào**. Đặt ngón thứ hai lên màn hình trong khi ngón thứ nhất còn giữ, Safari/Chrome hiểu
đó là cử chỉ **cuộn hoặc pinch của trang web**, rồi báo lại cho Unity thành
`Mouse ScrollWheel`. Giữ lâu một chỗ có thể thành chuột phải.

Bản sửa đầu tiên của tôi **tắt hẳn camera trên cảm ứng** — hết lỗi, nhưng người chơi cũng
mất luôn quyền xoay. Sai ở chỗ tôi bịt cả **cửa giả** (trình duyệt tự dịch cử chỉ) lẫn
**cửa thật** (người chơi cố ý xoay), trong khi chỉ cửa giả mới cần bịt.

Nguyên tắc: **tự đọc `Input.touches`, không nhờ trình duyệt phiên dịch.**

| | cảm ứng | PC |
|---|---|---|
| Xoay quanh nhân vật | **kéo một ngón ở vùng trống** | giữ chuột phải |
| Phóng to / thu nhỏ | **chụm hai ngón** | con lăn |
| Đổi góc nhìn | **nút máy quay góc phải trên** | phím C / F1–F4 |
| Khoá góc nhìn | **nút con mắt, ngay dưới nút máy quay** | (không có — PC không cần) |

Không có dòng chữ nào báo đang ở góc nhìn nào — người chơi nhìn thấy nó rồi.

`CameraRig` vẫn chặn sạch mọi đường chuột, con lăn và phím khi đang ở cảm ứng; nó chỉ nhận
`CamUng.XoayCam` và `CamUng.ChumZoom` — hai biến do HUD tự đọc từ `Input.touches`.

**Mỗi ngón nhận vai đúng một lần, lúc nó chạm xuống**, và giữ vai đó đến khi nhấc lên:
cần → nút → còn lại là vùng trống. Phân vai lại mỗi khung thì ngón đang kéo camera đi
ngang qua cụm nút sẽ bấm nhầm kỹ năng, còn ngón bấm kỹ năng — bao giờ cũng kéo lê một
chút — sẽ thành kéo camera. Chính vì thế `ngonNut` phải **nhớ** ngón nào đang bấm nút.

Hai ngón thì **chỉ chụm, không xoay**. Làm cả hai cùng lúc thì mỗi cú chụm đều kéo góc
nhìn lệch đi một ít. Khung đầu tiên của cú chụm chỉ để ghi nhớ khoảng cách — lấy hiệu
ngay khung đó là nhảy một phát bằng cả khoảng cách hai ngón.

Độ nhạy `nhayCamUng = 0,16` độ mỗi điểm ảnh: kéo ngang 400 điểm ảnh xoay được 64°, vừa
hết một lần quét ngón cái.

**Đo** bằng chính `Orbit()` và hai biến cử chỉ (đều public), trong Play mode:

| phép đo | kết quả |
|---|---|
| PC — `Orbit(50,20)` rồi đợi nhiều khung | yaw=50,00 chuc=−18,00 — **giữ nguyên**, PC không bị đụng |
| Cảm ứng — đặt `XoayCam=(100,0)` | yaw đổi liên tục — camera **nhận** cử chỉ kéo |
| Cảm ứng — đặt `ChumZoom=60` | xa=−3,38 — **âm là lại gần**, toè ra đúng chiều phóng to |
| Cảm ứng — mọi đường chuột/con lăn | vẫn bị chặn sạch |

Cột PC quan trọng ngang cột cảm ứng: nó chứng minh bản sửa không đụng gì đến PC.

**Trên máy thật, bấm F12** để xem hai dòng camera: dòng `trinh duyet gui:` là thứ trình
duyệt tự dịch (phải kệ nó), dòng `cu chi cua ta:` là thứ HUD thật sự đọc được.

#### Nút con mắt: khoá góc nhìn

Bấm một cái là camera **đứng im ở đúng góc đang có**: không xoay, không phóng to, không đổi
chế độ nhìn — không bằng bất cứ thao tác nào. Bấm lại là mở. Đang khoá thì trên nút có một
**gạch chéo đỏ**, và nút máy quay ở trên mờ hẳn đi — để người chơi khỏi bấm mãi mà không
hiểu sao không đổi được.

Camera **vẫn bám theo nhân vật**. Khoá là khoá *góc nhìn*, không phải đóng băng camera lại
một chỗ; đóng băng hẳn thì đi vài bước là nhân vật ra khỏi khung hình.

**Chặn ở ngay đầu `CameraRig.Update`, không lọc từng thao tác một.** Lọc từng cái thì mỗi
đường vào mới lại phải nhớ chặn lại — phím, chuột, con lăn, cử chỉ cảm ứng, nút đổi góc
nhìn — và chỉ cần bỏ sót một cái là góc nhìn vẫn nhảy được dù đang khoá.

Nhưng chặn ở `Update` thôi vẫn chưa đủ: `Orbit()` **cộng dồn** độ lệch vào `yawOffset`, nên
độ lệch vẫn tích luỹ trong lúc khoá và đến lúc mở khoá camera nhảy một phát bằng tất cả
những gì đã tích. Nên `Orbit()` có lớp chặn thứ hai của riêng nó.

Cờ `CamUng.KhoaCam` đặt ở `CamUng` chứ không ở `CameraRig` vì **cả hai bên đều cần**:
`CameraRig` đọc để bỏ qua mọi thao tác, HUD đọc để vẽ gạch chéo và làm mờ nút máy quay.

Hai nút cách nhau **88·s** từ tâm đến tâm, mỗi nút bán kính **40·s** — còn 8·s hở giữa hai
vành, đủ để không bấm nhầm cái này ra cái kia.

> Con mắt vẽ bằng **17 dải dọc** chứ không phải một hình bầu dục kéo giãn: chiều cao mỗi dải
> theo `(1 − d²)^0,62`, số mũ nhỏ hơn 0,5 của đường tròn nên hai đầu **thon nhọn** lại thành
> hình quả hạnh. Bầu dục thì ra quả trứng, không ai đọc ra là con mắt.

```
DO TRONG PLAY (menu 21)
   bam vao dung tam nut          -> nut nhan cham = True, khoa = True
   DANG KHOA, day camera bang moi duong (Orbit + CycleView + XoayCam + ChumZoom):
       lech goc = 0,00 do   lech khoang cach = 0,00 m   lech fov = 0,00
       doi che do nhin = False
   bam lan hai                   -> khoa = False
   DA MO KHOA, day lai:
       lech goc = 28,87 do  lech khoang cach = 18,03 m  doi che do = True
   so loi = 0
```

> Ảnh chụp phải bằng `ScreenCapture`, **không** bằng `cam.Render()`: HUD vẽ bằng `OnGUI`, mà
> `OnGUI` không đi vào RenderTexture của một lần Render thủ công — chụp kiểu đó ra một cảnh
> game không có lấy một cái nút nào.
>
> Và bật giao diện cảm ứng để thử thì phải đặt qua **ô `epCamUng` của HUD**, đừng đặt thẳng
> `CamUng.EpBat`: `GameHUD.Update` ghi đè biến đó mỗi khung, nên đặt thẳng vào là khung sau
> mất sạch. Lần đo đầu vấp đúng cái đó — ảnh chụp ra thanh kỹ năng của PC.

#### Bài học: thông báo "đã sửa" của script KHÔNG chứng minh file đã ghi

Lần sửa đầu tiên **không hề vào được file**, mà tôi vẫn báo là xong. Script Python làm hai
bước rồi mới ghi file **một lần ở cuối**; bước 2 ném `AssertionError` vì mốc chú thích
không khớp, nên bước 1 — đã in ra "sua: di chuyen bang joystick va khoa cham-de-di" — cũng
mất theo. Tôi tin vào dòng in đó. Người dùng build ra WebGL mới phát hiện lỗi còn nguyên.

Từ đó mọi script sửa file đều **đọc lại file từ đĩa** và grep lại từng mẫu đã thay, in ra
CÓ/KHÔNG cho từng cái. Dòng `print` ở giữa chương trình chỉ nói ý định, không nói kết quả.

**Trên điện thoại không có Console.** Nên bảng chẩn đoán F12 hiện thẳng lên màn hình:
chế độ đang chạy, nền tảng + `isMobilePlatform` + `touchSupported`, số ngón đang chạm, hướng
cần joystick, và quan trọng nhất — `dang chay toi diem bam: khong`. Dòng cuối đọc thẳng
`hasMoveTarget` qua thuộc tính `PlayerController.DangCoDiemDen`, nên nếu khóa hỏng thì nó
hiện **"CO - VAN CHUA KHOA!"** ngay lúc chạm, không cần đoán.

**Cần đẩy nửa chừng thì đi chậm.** Vectơ cần **không chuẩn hoá thẳng** — chuẩn hoá là mọi
cái chạm tay đều thành chạy hết tốc.

**Chọn mục tiêu chấm điểm theo hai thứ**: lệch hướng bao nhiêu và ở xa bao nhiêu, với
**lệch hướng nặng gấp ba**. Chỉ lấy con gần nhất thì đẩy cần sang phải vẫn bắn vào con
sau lưng; chỉ lấy con đúng hướng nhất thì một con ở tận cuối bản đồ cũng được chọn chỉ vì
nó thẳng hàng.

#### Đổi qua lại giữa hai chế độ

| Cách | Đổi cái gì | Dùng khi |
|---|---|---|
| **Phím F9** (trong lúc chơi) | chỉ phiên đang chạy | xem nhanh, không phải thoát Play |
| **Menu `Diablo25D ▸ Giao dien cam ung`** | ghi vào cảnh | đặt mặc định, giữ qua các lần mở Unity và đi theo bản build |
| Ô `epCamUng` trong Inspector của `GameHUD` | ghi vào cảnh | giống menu, chỉ là làm bằng tay |

F9 đổi ngay giữa trận và hiện một dòng báo chế độ mới, nên xem được cả hai bố cục trong
cùng một lần chạy. Nhưng nó **không lưu** — thoát Play là trở lại như cũ. Muốn giữ thì
dùng menu.

Cả ba chỉ để **thử trên PC**. Máy cảm ứng thật tự nhận ra mình qua `CamUng.DangDung`,
không cần ai bật gì.

#### Ba lỗi bị bắt khi chụp trong Play mode

**Joystick nhảy lên góc TRÊN.** Toạ độ chạm đếm **từ dưới lên** (như `Input.mousePosition`),
nên góc dưới trái là **y nhỏ**. Tôi viết `Screen.height - 150s` và cần dán lên sát bảng
trạng thái.

**Sáu nút chồng lên nhau.** Khoảng cách hai nút liền kề trên một cung là `2R·sin(bước/2)`,
phải lớn hơn **đường kính** nút. Cung R=170·s bước 29° cho ra 85·s trong khi đường kính là
124·s.

#### Phóng to nút thì phải phóng to cả cụm

Số hiện tại: nút bán kính **60,72·s**, lề **101,2·s**, cung trong **234,03·s** bước 34°,
cung ngoài **392,15·s** bước 22°, nút thứ bảy ở cung riêng **92,23·s** góc 26,1°.
Cần joystick bán kính **172,5·s**, tâm **(232,5·s, 232,5·s)**.

Nút to thêm mà **giữ nguyên hai cung** thì khe hở giữa hai nút liền kề tụt gần về 0 — chúng
dính vào nhau. Cách đúng là nhân **tất cả** cho cùng một hệ số: hai bán kính cung, cung riêng
của nút thứ bảy, và cả lề tính từ góc màn hình. **Góc thì giữ nguyên** — chỉ bán kính đổi.
Khi đó mọi khoảng cách cùng lớn lên đúng tỉ lệ đó và tỉ lệ hở giữ nguyên.

Đã nhân hai đợt: **+10% rồi +15%**, tức 48 → 52,8 → **60,72**; lề 80 → 88 → **101,2**.

Đo lại toàn bộ bố cục sau đợt +15%, trên sáu tỉ lệ màn hình:

```
ti le man hinh      tran man hinh   cho sat mep nhat   cap sat nhau nhat
16:9  1920x1080     khong co             81,1 px            ho 15,4 px
20:9  2400x1080     khong co             81,1 px            ho 15,4 px
4:3   1440x1080     khong co             81,1 px            ho 15,4 px
16:9  1280x720      khong co             54,0 px            ho 10,3 px
19,5:9 2340x1080    khong co             81,1 px            ho 15,4 px
iPhone 8 1334x750   khong co             56,3 px            ho 10,7 px
```

Không tỉ lệ nào có nút tràn mép, không cặp nào đè nhau, và không nút nào chạm vùng cần
joystick. Chỗ sát mép nhất luôn là **nút thứ bảy** (Giựt sét) vì nó nằm trong cùng, gần góc
màn hình nhất.

Với cần joystick cũng vậy: bán kính lên **hai đợt**, +25% rồi +20% nữa, tức
115 → 143,75 → **172,5** (tổng +50% so với ban đầu). Mỗi lần **tâm phải đẩy ra theo**. Giữ
tâm ở 175 như lúc đầu thì cần chạm mép màn hình — 175 − 172,5 = 2,5 điểm ảnh — và nửa vòng
bên trái nằm ngoài viền. Công thức giữ lề: **tâm = bán kính + 60**, nên tâm là **232,5**.

Đo lại sau đợt hai: lề trái 60,0, lề dưới 60,0, đỉnh vòng tròn ở y = **405** trên 1080 nên
không chạm bảng trạng thái; vùng chạm cần (1,35 × bán kính) lan tới x = **465**, còn nút kỹ
năng gần nhất bắt đầu ở x ≈ 1579 trên màn 16:9 — hai vùng không tranh nhau ngón tay.

**Cờ `EpBat` kẹt lại ở giá trị cũ.** Đặt nó trong `OnGUI` nhưng đọc trong `Update` — mà
`Update` chạy **trước**. Tắt cờ thử xong mà `CamUng.DangDung` vẫn trả về true, PC vẫn hiện
joystick. Đưa về đầu `Update`.

#### Màn hình thua trên cảm ứng: bỏ dòng nhắc phím, thêm nút

Màn hình thua vốn viết `"Bam R de choi lai, ESC de ve man hinh chinh."` — trên máy cảm ứng
đó là **chỉ ra hai cái phím mà người chơi không có**. Nay dòng đó chỉ còn hiện khi
`!CamUng.DangDung`; ở chế độ cảm ứng thay bằng một nút **TRO VE** gọi
`GameDirector.BackToMenu()`.

**Ô nút có đúng MỘT công thức** — [`ONutTroVe`](Assets/Scripts/UI/GameHUD.cs) — dùng chung
cho cả lúc vẽ lẫn lúc đọc chạm. Tách thành hai chỗ thì sửa một bên quên bên kia, nút hiện
một nơi mà vùng bấm nằm nơi khác.

Vị trí: rộng **280·s**, cao **76·s**, giữa ngang, đỉnh ở **0,66·H**. Cao độ 0,66 chứ không
phải 0,58: dòng thông báo ngắn ("het nang luong"...) vẽ ở **0,62·H** và còn sống thêm 1,6
giây sau khi gục ngã, đặt nút cao hơn là nó đè lên chữ.

Kiểm bố cục bằng chính công thức trong code:

```
ti le man hinh      nut x        nut y        tran man hinh   de cum skill
16:9  1920x1080     820-1100     713-789      khong           khong
16:9  1280x720      547-733      475-526      khong           khong
4:3   1440x1080     580-860      713-789      khong           khong
20:9  2400x1080     1060-1340    713-789      khong           khong
iPhone 8 1334x750   570-764      495-548      khong           khong
doc   1080x1920     291-789      1267-1402    khong           DE
```

Màn hình **dọc** thì ô nút phủ lên vùng cụm kỹ năng — nhưng vô hại: lúc đã chết,
`DocCamUng` thoát sớm nên cụm kỹ năng **không nhận chạm nữa**, còn về hình thì lớp phủ đen
0,72 và nền nút (alpha 0,92) đều vẽ **sau** cụm nút.

**Chạm: bấm xuống chỉ ghi nhớ ngón, nhấc lên còn trong ô mới thật sự về menu.** Đặt nhầm
ngón thì kéo ra ngoài rồi thả là thoát, giống nút của hệ điều hành. Màn hình thua nhảy ra
đúng lúc đang loạn tay — không nên để một cái chạm vô tình ném thẳng người chơi về menu.

> Nút này **không** dùng `GUI.Button`. Cả HUD đọc chạm thẳng từ `Input.touches` như các nút
> kỹ năng, và ở đây thêm một lý do: `GUI.Button` bắn ngay khi ngón chạm xuống chứ không đợi
> nhấc lên.

**Cảm ứng không còn cách chơi lại tại chỗ** — phím R không bấm được và chưa có nút cho nó.
Đường về là TRO VE → Main Menu → chọn màn.

### Ô vuông đen loang lổ dưới mặt đất Act2

Người chơi báo: bản WebGL trên điện thoại **thường xuyên** hiện các ô vuông trong suốt màu
đen dưới mặt đất, "giống như đang ghép từng mảnh đất lại"; trên máy bàn thì **thỉnh thoảng**.
Xuất hiện **sau khi vẽ lại địa hình cho gồ ghề**.

#### Không phải terrain — mà là mặt nước

Đã loại từng cái một trước khi tìm ra:

```
lo thung terrain (holes)   : 0 / 262144 o  -> khong phai
nhieu terrain ghep lai     : chi co 1 Terrain, khong co neighbor
LOD crack giua cac patch   : dung nen DO CHOI roi dem diem lot qua, ca 3 muc
                             pixelError (3 / 1 / 0) x 2 co man hinh -> 0 diem
ghi de terrain theo muc
chat luong (basemap...)    : terrainQualityOverrides = None o ca 6 muc
```

Nguyên nhân thật: **mặt nước và mặt đất nằm chồng lên nhau**. Act2 có 11 mặt nước phẳng
(shader `Diablo25D/NuocDam`). Trước đây địa hình phẳng nên chúng nằm yên bên dưới; sau khi
vẽ lại cho gồ ghề, `KeoVeTam` kéo địa hình quanh mỗi vũng về **cao độ mới tại tâm vũng**,
còn mặt nước vẫn ở cao độ cũ. Đo được:

```
11 / 11 vung nuoc bi dat cat qua
moi vung co 3 - 19 diem hai mat cach nhau DUOI 2 CM
```

Hai bề mặt sát nhau như vậy thì cái nào vẽ trước phụ thuộc vào **độ chính xác của bộ đệm
chiều sâu**. Trên WebGL điện thoại bộ đệm thường chỉ **16 bit**, máy bàn 24 bit — đúng là lý
do "mobile bị nhiều hơn desktop".

#### Chữa 1: khoét lòng vũng xuống dưới mặt nước

[Act2LongVungNuoc.cs](Assets/Editor/Act2LongVungNuoc.cs) — vũng nước vốn phải nằm trong một
chỗ trũng, địa hình cũ vô tình dâng ngang mặt nước.

```
tu tam den mep   : sau 0,42 m -> 0,25 m duoi mat nuoc
tu mep ra 1,7 R  : vuot dan len khop dia hinh xung quanh
ban kinh khoet   : ban kinh hop bao x 1,08
```

Lấy **cái thấp nhất** trong các vùng ảnh hưởng (`Mathf.Min`), không phải cái cuối cùng: hai
vũng kề nhau thì chỗ giao phải theo cái sâu hơn, không thì giữa chúng nổi lên một gò nằm
đúng ngay dưới mặt nước.

```
                                   truoc    sau
vung bi dat cat qua                11/11    0/11
diem hai mat sat nhau duoi 2 cm      90        0
khoang cach nho nhat dat-nuoc      0,00 m   0,24 m
```

> Bán kính khoét phải **nới thêm 8%**. Để đúng bán kính hộp bao thì còn 2/11 vũng dính:
> mép mặt nước nhô ra ngoài hộp một chút vì mesh không tròn đều, và vài centimet hụt ở vành
> là đủ sinh z-fighting trở lại.

> ⚠️ **Phạm vi đo phải là hình TRÒN nội tiếp, không phải cả hộp bao.** Đo cả hộp thì bốn góc
> — vốn nằm **ngoài** mặt nước, là bờ, đất cao hơn nước ở đó là đúng ý muốn — bị tính thành
> lỗi, và kết quả ra "11/11 vẫn hỏng" trong khi thật ra đã sạch.

Kiểm bờ vũng không thành bẫy: dốc lớn nhất **42,5°**, không điểm nào vượt 55° — đúng bằng
`slopeLimit` của người chơi và quái, nên không ai bị kẹt dưới vũng.

#### Chữa 2: tỉ lệ far/near của máy quay

Độ chính xác bộ đệm chiều sâu phụ thuộc **tỉ lệ** far/near chứ không phải hiệu của chúng.

```
        truoc      sau
near    0,15       0,5
far     400        220
ti le   2667       440      -> tot hon 6 lan
```

Cắt gì không: máy quay gần nhất là **4,1 m** (zoom hết cỡ ở góc nhìn "3D tự do":
`zoomOffset` chặn ở `-distance × 0,45`), còn nhìn xa nhất qua bản đồ 109 m là khoảng 150 m —
mà ở 200 m sương mù `ExponentialSquared` mật độ 0,0105 đã che **98,8%**.

Sửa ở **sáu chỗ**: `Act2Baker`, `AssetBaker`, `WorldBuilder`, `GameBootstrap`, và cả
`Act1.unity` lẫn `Act2.unity`.

> Sửa file `.unity` thẳng trên đĩa trong khi Unity **đang mở cảnh đó** thì phải
> `OpenScene` lại ngay — bản trong bộ nhớ vẫn là bản cũ, và lần lưu cảnh kế tiếp sẽ đè
> lên phần vừa sửa.

#### Chưa xác nhận trên máy thật

Chẩn đoán này dựa trên đo đạc trong Editor, **chưa chạy thử trên WebGL điện thoại**. Editor
dùng D3D11 với bộ đệm chiều sâu 24 bit nên không tái hiện được lỗi. Nếu build lại mà vẫn
còn, thứ cần soi tiếp là bóng đổ: mức chất lượng Medium trở xuống chỉ có **1 cascade**,
`shadowResolution` Low, `shadowDistance` 20 m.

### Giựt sét bắn ba mạch cùng lúc

Người chơi phóng **tối đa 3 mạch** tới **3 mục tiêu riêng** trong tầm, mỗi mạch vẫn lan
tiếp sang con xung quanh như cũ. Quỷ cây **không đổi** — vẫn một mạch, không lan.

```
                    soTiaDau   maxChains   toi da bao nhieu con trung
nguoi choi              3          5            3 x 6 = 18
Quy cay                 1          0                 1
```

Chỉnh bằng `soTiaDau` trong [GiatSet.cs](Assets/Scripts/Skills/GiatSet.cs); `PhongCuaQuai`
đặt lại về 1 nên quái không ăn theo.

#### Ba mạch phải dùng CHUNG danh sách "đã trúng"

Cho mỗi mạch một danh sách riêng thì đám quái đứng sát nhau **ăn ba lần sát thương**, và
mạch này lan sang đúng con mà mạch kia vừa đánh — nhìn ra một mớ tia chồng lên nhau.

Và phải **đánh dấu trước cả ba mục tiêu đầu** ngay khi chọn xong, trước khi mạch nào chạy.
Không thì mạch thứ nhất lan sang đúng con mà mạch thứ hai sắp đi tới, và mạch hai thành ra
không còn gì để đánh. Trong vòng lặp cũng vậy: chọn được con tiếp theo là đánh dấu ngay,
đừng đợi tới lượt đánh — ba mạch chạy song song, chờ thì cả ba cùng nhắm một con.

#### OverlapSphere trả về từng COLLIDER, không phải từng con quái

Một con quái có mấy collider thì nó vào danh sách ứng viên mấy lần, và **cả ba tia dồn hết
vào một con**. Phải lọc trùng theo `Damageable` trước khi xếp hạng.

#### Hồi chiêu 0,4 giây

`giatSetCooldown` 0,55 → **0,4**. Con số thật nằm ở **ba chỗ**: mặc định trong
[PlayerController.cs](Assets/Scripts/Player/PlayerController.cs), prefab
`Player_Sorceress`, và `Act2.unity`. Act1 không ghi đè nên nó ăn theo prefab.

> ⚠️ **Kỹ năng này giờ mạnh hơn nhiều.** Trước: 1 mạch, tối đa 6 con, một đòn mỗi 0,55 giây.
> Nay: 3 mạch, tối đa 18 con, một đòn mỗi 0,4 giây — sát thương mỗi giây tăng khoảng **4
> lần**. Mỗi mạch đều bắt đầu ở sát thương đầy đủ (30) rồi mới giảm dần theo `damageFalloff`.
> Muốn hãm bớt mà vẫn giữ ba tia thì hạ `maxChains` hoặc `damage`.

Đo trong Play mode, đặt 5 con dàn hàng ngang trước mặt:

```
tia TU TAY   dai 8.0 m  glow 0.483
tia TU TAY   dai 7.6 m  glow 0.483
tia TU TAY   dai 8.3 m  glow 0.483
TONG: 3 tia, trong do 3 tia phong tu tay
hoi chieu cua nguoi choi = 0.4 s
tia QUY CAY: soTiaDau = 1  maxChains = 0  tam = 8  sat thuong = 14
```

> Dời quái bằng `transform.position` thì phải gọi **`Physics.SyncTransforms()`** ngay sau
> đó. Không gọi thì `OverlapSphere` vẫn thấy chỗ **cũ**, và phép đo báo "không tìm thấy mục
> tiêu nào" dù quái đang đứng ngay trước mặt.

### Tia sét: "một lớp hình mỏng dán vào tay"

Người chơi mô tả hai lỗi ở chỗ tay giáp tia Giựt sét: **thấy một lớp hình mỏng như đang
ghép tia sét vào**, và **tia quá lớn, che hết mặt và đầu nhân vật**. Hai lỗi này chung một
gốc.

#### Dải chữ X luôn có một dải quay lưng về người xem

`LightningArc` dựng mỗi đoạn bằng **hai dải vải cắt chéo nhau hình chữ X**. Ý đồ nghe hợp
lý — "nhìn góc nào cũng thấy tia dày dặn" — nhưng thực tế bao giờ cũng có **một dải gần như
vuông góc với hướng nhìn**, và nó hiện ra nguyên một **mảng chữ nhật mờ**. Chụp cận cảnh
thấy rất rõ: những hình chữ nhật nhạt nằm quanh thân tia, cạnh thẳng băng.

Dải càng rộng thì mảng đó càng to — mà quầng sáng để **1,05** tức dải rộng **2,1 m**, to hơn
cả chiều ngang thân nhân vật.

Chữa bằng **một dải billboard** — mặt dải luôn quay về máy quay:

```csharp
Vector3 s = Vector3.Cross(t, mat - pts[i]);   // t = huong di, mat = cho may quay
```

Bỏ dải thứ hai **không làm tia hẹp đi**: dải chéo chỉ chồng thêm một lớp sáng ở giữa chứ
không nới rộng ra hai bên. Nên Sấm sét, Lốc và tia choáng giữ nguyên bề ngang cũ.

> `Camera.main` phải **cache lại**. Nó quét cả cảnh tìm theo tag, mà hàm này bị gọi cho
> **từng điểm** trên **từng tia** — một mạch Giựt sét 6 nhịp là hàng trăm lần mỗi lần dựng.

#### Đầu dải cắt phẳng nằm ngay trên tay

Bề ngang cũ là `Lerp(1 → 0,5)` dọc theo tia: **rộng nhất ngay tại gốc**. Đầu dải bị cắt
phẳng, thành một cạnh ngang rộng bằng cả bề ngang tia, đặt đúng chỗ bàn tay — nhìn ra
"miếng vải dán vào tay". Nay vuốt nhọn **14% ở mỗi đầu**:

```csharp
float vuot = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(Mathf.Min(k, 1f - k) / 0.14f));
float w = width * Mathf.Lerp(1f, 0.62f, k) * vuot;
```

Tia **mọc ra** từ tay thay vì được dán vào.

#### Bề ngang riêng cho Giựt sét

Giựt sét là tia **duy nhất phóng ra từ tay người chơi** — các tia khác giáng từ trời xuống
hoặc nổ quanh quái, cách xa nhân vật nên bề ngang lớn không phiền. Nên đặt riêng trong
`GiatSet.VeTia` thay vì hạ giá trị mặc định:

```
                    quang    dai rong
Sam set, Loc, choang 1,050     2,10 m     (giu nguyen)
Giut set             0,483     0,97 m     (0,42 x he so 1,15 cua nhip dau)
```

Đo trong Play mode, hai tia sống cùng lúc:

```
tia dai 7.3 m   core 0.184   glow 0.483  -> dai rong 0.97 m    <- Giut set
tia dai 9.0 m   core 0.220   glow 1.050  -> dai rong 2.10 m    <- kieu Sam set
```

#### Cách chụp được tia sét để soi

Tia sống **0,3 giây** — ngắn hơn khoảng cách giữa hai lệnh MCP. Hạ `Time.timeScale`:

```
timeScale 0,015  ->  0,3 s thanh 20 s that   -> VAN KHONG KIP, tia da tat
timeScale 0,002  ->  0,3 s thanh 150 s that  -> kip
```

> ⚠️ **Phải chụp bằng CHÍNH máy quay của game.** Dải quay mặt về `Camera.main`; dựng một
> máy quay phụ ở góc khác rồi chụp thì thấy dải nghiêng — đó là lỗi của **phép đo**, không
> phải của hiệu ứng, và nó khiến bản đã sửa trông y như bản chưa sửa. Muốn nhìn cận thì
> **giảm `fieldOfView`**, đừng dời máy quay: dời chỗ thì lưới đã dựng theo vị trí cũ vẫn
> giữ nguyên.

### CHỤP ẢNH TRONG PLAY MODE

Những thứ **chỉ sống trong Play mode** — `OnGUI`, `LightningArc`, hệ hạt đang chạy thật —
không chụp được bằng `Camera.Render` từ Editor. Bộ đồ nghề để chụp chúng:

| Tập tin | Vai trò |
|---|---|
| [`Scripts/Debug/ChupRoiThoat.cs`](Assets/Scripts/Debug/ChupRoiThoat.cs) | chờ N giây → `ScreenCapture.CaptureScreenshot` → tự thoát Play |
| [`Editor/ChupTrongPlayMode.cs`](Assets/Editor/ChupTrongPlayMode.cs) | đặt lịch qua `EditorPrefs` để lịch **sống sót qua lần nạp lại domain** khi bấm Play |

Trình tự: đặt lịch → `EditorApplication.isPlaying = true` → chờ file PNG hiện ra → đọc.

> **Lệnh bật Play báo lỗi nhưng vẫn chạy.** Nó trả về *"User interactions are not
> supported"*, mà Unity **vẫn vào Play mode**. Kiểm bằng `EditorApplication.isPlaying` ở
> lệnh sau chứ đừng tin thông báo lỗi. Trong Play mode các lệnh thường vẫn chạy được;
> riêng `AssetDatabase.Refresh()` thì bị chặn.

**Vì sao đáng làm:** một bản dựng lại bằng đúng công thức trông rất đúng, nhưng vẫn che
mất lỗi thật. Bản dựng lại của HUD không hề cho thấy dòng **"GOC NHIN" đè chồng lên tên
nhân vật** — cả hai đều vẽ ở góc trái trên, một cái ở `(12·s, 10·s)` và một cái ở
`(14·s, 12·s)`. Ảnh chụp thật đầu tiên lộ ra ngay.

Chữa bằng cách đưa kích thước bảng về **một chỗ duy nhất** (`BangCao*`) và mở
`DayBangTrangThai(s)` cho chỗ khác biết mà né ra.

Dòng đó về sau **bỏ hẳn**. Né ra rồi nó vẫn gây chuyện lần thứ hai: trên màn hình hẹp,
chữ dài xuống hai dòng và tràn vào thanh năng lượng. Người chơi đang nhìn thẳng vào góc
nhìn của mình thì không cần ai viết tên nó ra — trên cảm ứng đã có nút máy quay ở góc phải
trên, trên PC vẫn là phím C. `DayBangTrangThai(s)` giữ lại cho thứ khác dùng.

### MƯA BĂNG: TẢNG BĂNG DỰNG TRONG BLENDER

Sáu tảng băng lớn nhỏ hình dạng khác nhau, cộng bốn mảnh vụn cho lúc vỡ — nằm ở
`Assets/Resources/BangRoi`, dựng bằng **bao lồi của điểm ngẫu nhiên** như mảnh băng của
biểu tượng. Trục dài theo Y, đầu nhọn quay xuống.

Trước đó tất cả đều là `ProcMesh.Crystal` — một hình nón sáu cạnh **đều**, cùng một cỡ: cả
trận mưa trông như đổ từ một cái khuôn duy nhất ra.

Thêm **khí lạnh** toả quanh tảng trên đường rơi, và lúc chạm đất thì **mảnh băng 3D thật**
văng ra — hệ hạt kiểu `Mesh` với `SetMeshes(...)`, mỗi hạt bốc một trong bốn mảnh vụn. Dùng
hệ hạt chứ không tạo từng GameObject: hệ hạt lo sẵn chuyển động, trọng lực, xoay lộn và tự
dọn dẹp.

**Sát thương 16 → 26,4** (tăng 65%). Thả **39 tảng** trong 5 giây (`shardInterval` 0,16 →
0,128) và rơi **nhanh hơn 25%** (`fallTime` 0,42 → 0,336).

#### Đóng băng 30%: xác suất phải gieo RIÊNG từng mục tiêu

`CombatUtil.AreaDamage` áp `statusSeconds` cho **tất cả** mục tiêu trúng đòn — cứ chạm là
đóng băng hết, không còn là xác suất. Nên có `AreaFreeze` riêng, gieo một lần cho mỗi mục
tiêu, giống cách `AreaShock` làm với choáng của sấm sét.

**`FrozenEffect` trước đây chỉ làm CHẬM, không chặn tấn công.** `EnemyAI` chỉ đọc
`frozen.slow` để giảm tốc — con quái đứng yên mà vẫn vung tay đánh. "Đóng băng" mà vẫn ra
sát thương thì không ra đóng băng. Giờ `IsFullyFrozen` được xử lý giống hệt bị sét choáng:
huỷ đòn đang đánh, không bắt đầu đòn mới, đứng im.

Đo bằng hai phép:

| phép đo | kết quả |
|---|---|
| Gieo 400 lần `AreaFreeze` với xác suất 0,30 | đóng băng **28,8%** |
| Đóng băng một con 60 giây, để một con tự do làm đối chứng | con đóng băng đi **0,01 m**, con tự do đi **3,42 m** |

Cột đối chứng quan trọng: nếu cả hai cùng đứng im thì phép đo không nói lên điều gì — có
thể chỉ vì game đang dừng.

**Cụm gai băng mọc lên sau khi tảng vỡ.** Bốn cụm dựng trong Blender
(`Resources/BangRoi/CumGai0..3`), mỗi cụm là **một mesh duy nhất** gồm tám đến mười bảy
tinh thể chụm lại — Unity chỉ phải vẽ một lần thay vì mười bảy lần.

**Tỉ lệ cao/rộng quyết định nó ra băng hay ra cỏ.** Bản đầu để bán kính tinh thể
0,055–0,115 trên chiều cao tới 1,15 — mảnh **1:10**, tức đúng tỉ lệ một cọng cỏ, và cả cụm
nhìn ra bụi cỏ chứ không ra băng. Giờ tỉ lệ cả cụm quanh **1,1–1,3**.

Hệ số cỡ cụm là **1,0925** (= 0,95 × 1,15, sau lần tăng thêm 15%), nhân với chiều cao
ngẫu nhiên 0,5–1,3.

Ba thứ làm cụm "dày đặc" chứ không "thưa":

- mỗi tinh thể **mập hẳn** (bán kính 0,12–0,28 thay vì 0,055–0,115)
- **nhiều tinh thể hơn** và bán kính cụm **nhỏ lại**, nên chúng chụm sát, cái nọ chồng cái kia
- thêm một lớp **khối thấp ở chân** làm đế — không có thì nhìn ra mấy cái que cắm xuống đất Bản trước dùng `ProcMesh.Crystal`: một hình nón năm cạnh **đều**, mỗi chỗ mọc lên
đúng một cái, nhìn ra cái cọc chứ không ra cụm băng. Mỗi vụ nổ giờ mọc 7–10 cụm.

Hai chi tiết dễ sai ở đây:

- **Đáy cụm phải nằm ở y = 0.** `ExpandFade` kéo chiều cao từ 0 lên 1 để cụm trông như đâm
  từ dưới đất trồi lên; đáy lệch khỏi 0 là cụm nhô lên giữa không trung.
- **Chỉ xoay quanh trục đứng.** Nghiêng nguyên cả cụm thì chân cụm nhấc khỏi mặt đất.
- `ExpandFade.endScale` phải là **localScale đã đặt**, không phải `Vector3.one` — đặt
  `one` là cụm bị ép về cỡ 1,0 và mất hết cỡ ngẫu nhiên vừa gán.

**Cỡ tảng và cỡ mảnh vỡ phải RỜI HẲN NHAU.** Lúc đầu tảng cao 0,23–1,12 m còn mảnh vỡ
0,08–0,47 m: hai dải chồng lên nhau, tảng nhỏ nhất còn bé hơn mảnh to nhất — rơi xuống đất
thì không đọc ra là *vỡ*, chỉ thấy một đám mảnh y hệt cái vừa rơi. Giờ tảng **0,72–2,54 m**
(sau lần tăng thêm 30%), mảnh vỡ **0,07–0,32 m**, và bắn ra 22 mảnh thay vì 14.

#### Tảng băng nhắm vào kẻ địch

`ChonDiemRoi()` ưu tiên đầu một kẻ địch còn sống trong vùng; không có con nào thì mới rơi
bừa. Nhiều mục tiêu thì chọn ngẫu nhiên một con đang sống bằng **lấy mẫu kiểu hồ chứa**
(không gom thành danh sách rồi mới bốc), nhờ vậy tảng rải đều giữa các mục tiêu chứ không
dồn hết vào một con. Cùng cách làm với `PickTarget()` của Sấm sét.

**Vẫn phải tản ra quanh mục tiêu.** Ba mươi chín tảng rơi vào đúng một toạ độ thì chúng
chồng khít lên nhau, hết ra "mưa" mà trông như một cái cột băng. `aimSpread = 0,55 m` vẫn
nằm gọn trong bán kính nổ 1,7 m nên không mất sát thương.

Đo bằng một **bia bất tử** đặt lệch tâm bão 3 m — đo bằng quái thật thì không ra kết quả:
29 sát thương mỗi tảng giết nó trong chưa đầy một giây, quái chết là hàm quay về rơi bừa và
số liệu đo được là của trường hợp khác hẳn.

```
cach BIA     : trung binh 0.37 m, xa nhat 0.54 m
cach TAM BAO : trung binh 3.10 m
nam trong ban kinh sat thuong 1,7 m: 23/23
(roi bua thi cach bia trung binh ~3,9 m)
```

#### Độ cao thả và tốc độ rơi đi liền nhau

`fallHeight` lên **20 m** cho bằng `boltHeight` của Sấm sét, thì `fallTime` phải lên
**0,96 giây** — giữ nguyên tốc độ 20,8 m/s đã chỉnh trước đó (7 / 0,336). Để nguyên 0,336
giây cho quãng đường gấp gần ba lần thì tảng lao xuống với **59 m/s**, nhanh đến mức gần
như không kịp nhìn thấy gì.

Nỗi lo cũ — tảng rơi xuyên qua ngay trước ống kính — **không thành**: máy quay đứng ở độ
cao 3,9 m và lùi ra sau người chơi, đo trong Play mode thì tảng gần máy quay nhất là
**9,75 m**. Hệ quả phụ: thời gian rơi dài gấp ba nên cùng lúc có **5–6 tảng** đang bay thay
vì 2–3.

#### Cụm băng phải có VẬT LIỆU RIÊNG, không dùng chung với chớp sáng

Dùng chung `Mats.Ice` thì cụm băng ra **trắng cháy**. Đo trên ảnh chụp thật:
R = 0,879 · G = 0,890 · B = 0,890 — hiệu B − R chỉ **0,011**, tức gần như không còn màu
xanh nào.

Ba thứ trong `S_Ice.shader` cộng **thẳng màu trắng** vào, không qua màu nào cả:

```hlsl
col += _RimColor.rgb * rim * _Glow;   // _RimColor cũ là (0,85 0,97 1,0)
col += sparkle;                        // số vô hướng — cộng đều cả ba kênh
col += spec * 0.9;
```

Lưới cụm gai có rất nhiều mặt nghiêng nên `rim` lớn ở gần như **mọi điểm** — cộng dồn ba
thứ lại thì cả cụm trắng xoá. Trên một khối băng phẳng thì không lộ, trên cụm gai thì lộ
hết.

`CumBangMat` chữa bằng bốn con số: viền đổi sang **xanh** (0,52 0,76 1,0), `_RimPower` lên
**3,6** cho viền hẹp lại, `_Glow` xuống **0,75**, `_Sparkle` xuống **0,35**. Đo lại sau khi
sửa: **B − R = 0,301**.

**Phần "phát sáng" không nằm ở viền fresnel.** Viền chỉ sáng ở mép nhìn gần tiếp tuyến, nên
từ xa cụm băng chẳng phát sáng chút nào. Thứ nhìn ra được là một **đĩa sáng nằm ngang dưới
chân cụm** (`HaoQuangBangMat`, ảnh `GlowPool`) — góc nhìn nào cũng thấy, và nó hắt màu xanh
lên phần dưới của cụm.

#### Nước bắn ra và gợn sóng

| Bộ phận | Kiểu vẽ | Vì sao |
|---|---|---|
| `GiotNuoc` | **Stretch** | giọt tròn nhìn ra hạt tuyết, không ra nước; kéo dài theo hướng bay mới đọc được là giọt |
| `GonSongNuoc` | **HorizontalBillboard** | billboard thường thì vòng quay theo camera và dựng đứng lên như cái vòng cổ |
| `NuocVan` (lúc rơi) | Stretch, thưa | tảng rơi hết 0,336 giây nên chỉ kịp nhả vài chục giọt — một vệt mờ theo sau |

Giọt nước dùng **alpha** chứ không additive: nước thật che bớt nền phía sau, additive thì
giọt nước sáng lên như tàn lửa.

Gợn sóng phát **ba nhịp** cách nhau 0 / 0,16 / 0,34 giây. Một vòng đơn chỉ ra "một cái vòng
lan ra"; ba vòng nối nhau mới thành gợn sóng.

**Bản đầu quá mờ để nhìn thấy.** Đếm được 34 giọt và 3 gợn đang sống mà trên ảnh chụp gần
như không thấy gì — trong cảnh đêm, giọt 0,05–0,16 với alpha 0,75 biến mất hoàn toàn. Phải
nâng: giọt **0,10–0,28**, ảnh `SoftDot(1.2)` thay vì `1.9` (luỹ thừa càng cao chấm càng
loãng ra rìa, giọt nước cần có lõi đặc), vành gợn sóng **0,10** thay vì 0,06, độ sáng
**1,25**. Đếm số hạt sống chứng minh hệ hạt *có chạy*, nhưng không chứng minh nhìn thấy
được — phải mở ảnh ra xem.

#### Bốn chỗ vấp

**Prefab đè lên code — hai lần trong cùng một việc.** `Skill_MuaBang.prefab` lưu sẵn
`shardDamage = 16`, và `iceShardPrefab` trỏ tới `Vfx_TangBangRoi.prefab` nên bản dựng bằng
code không hề chạy. Sửa code là chưa đủ; phải dựng lại cả prefab.

**Vật liệu tạo lúc chạy không lưu được vào prefab.** `Mats.Ice`, `MistMat`, `ShardMat` nằm
trong bộ nhớ chứ không trên đĩa — lưu prefab xong thì tham chiếu ra `null` và Unity vẽ mảng
**hồng magenta** kín màn hình. Phải kết tinh chúng thành file `.mat` trước
(`Assets/Materials/BangRoi`).

**Kết tinh phải kéo theo cả TEXTURE — đây mới là gốc của đám hình vuông.**

Vật liệu chạy bằng code đeo một texture **cũng tạo bằng code**. Ghi vật liệu thành `.mat`
thì tham chiếu texture rơi mất, `_MainTex` thành `null`, shader lấy mẫu **texture trắng mặc
định** — và mỗi hạt billboard biến thành một **ô vuông đặc**. Đúng cái người dùng thấy: ô
vuông nhỏ rơi theo tảng băng, ô vuông lớn khi nổ.

Phép đo tìm ra nó: in `_MainTex` của từng renderer trong prefab. Cả mười ba dòng đều
`tex = KHONG CO`. Trước đó tôi cứ đoán quanh chuyện vật liệu gán nhầm.

Cách chữa: nhúng texture vào chính file `.mat` bằng `AssetDatabase.AddObjectToAsset`.
Kiểm lại bằng cách **xuất texture ra PNG mà nhìn** — phải thấy đám sương tròn, mảnh băng
hình giọt, vòng tròn; nếu thấy ô vuông đặc là hỏng.

**Kết tinh phải theo VẬT LIỆU GỐC, đừng theo tên renderer.** Lần đầu tôi viết
`if (tên == "KhiLanh") … else vật-liệu-băng` — thế là mọi hệ hạt còn lại (chớp sáng, vòng
sóng, sương, bụi vụn) đều nhận vật liệu băng. Chúng là **billboard**, nên mỗi hạt thành một
tấm vuông tô vân băng: cả trận mưa lẫn vụ nổ ra một đám **hình vuông xanh** to tướng. Cách
đúng là duyệt từng renderer, lấy chính `sharedMaterial` nó đang đeo rồi ghi cái đó thành
asset — mỗi renderer giữ nguyên vật liệu của mình.

**Prefab chỉ giữ được MỘT hình và MỘT cỡ** — nó là ảnh chụp một thể hiện. Không bốc lại
mesh và scale ngay lúc sinh thì cả trận mưa chỉ có đúng một tảng lặp đi lặp lại, tức mất
sạch cái "lớn nhỏ hình dạng khác nhau" mà công dựng sáu mesh để làm.

**Tảng sinh ở độ cao 12 m là cao hơn cả máy quay** (máy quay ở khoảng 9–10 m): nó rơi xuyên
qua ngay trước ống kính và che kín màn hình. Bản cũ không bị vì tảng chỉ cao 0,8 m. Hạ
`fallHeight` xuống 7 và giới hạn cỡ tảng ở 0,34–0,92.

### KHIÊNG: QUẢ BONG BÓNG

Khiêng là một **bong bóng xà phòng** vàng kim, gần như vô hình — nhìn xuyên qua thấy rõ
mặt đất và cả quái đứng bên trong. Cái làm người ta "thấy" được nó chỉ gồm bốn thứ:

1. **Viền** mảnh ở mép, chỗ nhìn gần tiếp tuyến — thứ nhận ra đầu tiên
2. **Vân xà cừ** loang nhẹ, dày dần về phía rìa
3. **Đốm lấp lánh** rải rác, mỗi đốm nhấp nháy theo nhịp riêng
4. **Vành sáng ở chân vòm**, ngay trên mặt đất

Bản trước là một thứ khác hẳn: vòm vàng đục với những tia sét to chạy ngoằn ngoèo khắp mặt
cầu. Nhìn ra quả cầu năng lượng chứ không ra bong bóng, và tia sét thì che mất nhân vật.

#### Dựng mẫu trong Blender trước, rồi mới viết shader

Blender **không render được hiệu ứng chạy thời gian thực** — nó chỉ ra ảnh tĩnh. Nên quy
trình là: dựng bong bóng trong Blender để chốt diện mạo và các thông số, rồi tái tạo lại
bằng shader trong Unity.

Việc dựng mẫu đó đáng làm, vì nó bắt ngay hai thái cực:

| `Blend` của Layer Weight | kết quả |
|---|---|
| 0,08 (cộng mũ 4) | **vô hình hoàn toàn** — ở 512 điểm ảnh không còn điểm nào vẽ ra |
| 0,34 | **đục như quả trứng sữa** |
| 0,17 (mũ 3) | vừa: trong veo ở giữa, chỉ dày lên ở mép |

Không có mặt đất trong cảnh Blender thì không đánh giá được "trong đến mức nào" — phải có
gì đó ở phía sau để nhìn xuyên qua.

#### Giới hạn interpolator

Shader mới lúc đầu truyền **ba** `float3` xuống fragment (pháp tuyến đối tượng, pháp tuyến
thế giới, vị trí thế giới) và Unity từ chối: *"Too many texture interpolators would be used
for ForwardBase pass (11 out of max 10)"*.

Bỏ pháp tuyến đối tượng, dùng luôn **pháp tuyến thế giới** cho vân và đốm. Vòm không bao
giờ nghiêng nên `n.y` vẫn đúng là "cao thấp trên vòm".

#### Màu

Vàng kim, và ba nơi phải đổi cùng lúc — xem mục màu ở phần biểu tượng bên dưới. Lúc khiêng
**sắp vỡ** thì lệch hẳn sang đỏ sẫm: vòm đã vàng kim rồi thì đỏ cam trông y hệt lúc còn
nguyên.

**Chiều cao vòm giữ nguyên** (`CaoVomKhieng = 1,0957`, cao hơn bán cầu 20% theo yêu cầu
trước đó). Ảnh mẫu có vòm dẹt hơn — đổi thì phải sửa hằng số đó.

### SÁU BIỂU TƯỢNG KỸ NĂNG

Ruột mỗi biểu tượng là **ảnh render trong Blender**, nằm ở `Assets/Resources/Icons`.
`IconKyNang.cs` lo phần còn lại: cái đĩa nút — mặt lồi, đèn từ trên-trái, vành viền — rồi
**cộng** ảnh Blender vào.

Cộng chứ không trộn: ảnh Blender nền đen, nên chỗ đen không ảnh hưởng gì, chỗ sáng thì loé
lên — đúng kiểu một hiệu ứng phát sáng.

| | |
|---|---|
| Quả cầu lửa | khối cầu vân cuộn, vòng corona không đều, đốm lửa bay quanh |
| Mưa băng | hơn hai mươi mảnh băng vỡ, mỗi mảnh là bao lồi của một đám điểm ngẫu nhiên |
| Sấm sét | sợi lõi trắng chạy trong một luồng điện tím, thon dần về ngọn |
| Lốc xoáy | ba sợi khí cuộn quanh trục, mảnh vụn bị cuốn theo |
| Thiên thạch | tảng đá gồ ghề tối màu, khe nứt cháy đỏ, đuôi lửa |
| Khiêng | khiên heater vàng kim có thánh giá, viền chói loá, hào quang và tia toả quanh |

Trước đó tôi vẽ tay từng điểm ảnh. Ra hình phẳng, cạnh cứng đều — người dùng gọi là *"như
hình vẽ trẻ con"*, và đúng vậy: vẽ tay chỉ hợp cho những miếng đơn giản như vành nút hay
thanh máu.

#### Mảnh băng: ba thứ quyết định nó có ra "băng" hay không

Bản đầu là **lăng trụ sáu cạnh cộng chóp nón** — thân đều tăm tắp, mũi gọn đẹp, đối xứng
hoàn hảo. Đó đúng là hình một cây bút chì. Băng vỡ thì gãy theo những mặt phẳng ngẫu
nhiên, không mặt nào giống mặt nào. **Bao lồi (`bmesh.ops.convex_hull`) của một đám điểm
ngẫu nhiên** cho ra đúng cái hình đó, và cạnh sắc tự nhiên.

**Tiết diện phải gần tròn.** Bóp dẹt theo một trục thì khối nào quay mặt dẹt về máy quay
sẽ thành một lát kính mỏng.

**Phải đủ nhiều mặt.** Chín điểm cho ra vài mặt tam giác to bằng cả thân mảnh, bắt sáng
đều một màu — trông như miếng giấy gấp. Mười lăm đến hai mươi điểm thì các mặt nhỏ lại,
mỗi mặt một độ sáng, ra được chất băng lấp lánh.

**Trộn cả hai kiểu**: mảnh nhọn dài (tỉ lệ dài/ngang ~3,2) và khối gãy ngắn (~1,7). Chỉ
làm một kiểu thì hoặc ra một rổ bút chì, hoặc ra một đống đá cuội.

Mọi mảnh rơi theo **cùng một chiều**, chỉ lệch đi một chút. Cho mỗi mảnh một hướng ngẫu
nhiên thì ra đám mảnh vụn bay lung tung, không đọc ra là mưa băng đang đổ xuống.

#### Ánh sáng toả quanh: để compositor lo, đừng dựng bằng hình khối

Bản đầu tôi dựng các tia sáng bằng hình nón đặt quanh khiên. Máy quay trực giao nhìn thẳng
nên lần nào cũng thấy chúng **từ cạnh** — ra những thanh trắng dẹt cứng đơ, không phải tia
sáng. Đây là lần thứ ba cùng một cái bẫy trong phiên này.

Cách đúng: **Glare kiểu `Streaks`** kéo tia ra từ chính những chỗ sáng của khiên, rồi nối
tiếp một **`Fog Glow`** cho quầng mềm. Hai node Glare mắc nối tiếp trong compositor.

Khiên dựng bằng bao đa giác kiểu *heater* (cạnh trên thẳng, hai cạnh bên cong tụ về mũi
nhọn) rồi extrude cho dày; một bản nhỏ hơn 0,82 lồng vào trong làm mặt khiên, nên viền dày
lên và bắt sáng riêng.

**Màu: VÀNG KIM.** Đổi cả ba nơi cùng lúc, nếu không icon một màu mà hiệu ứng một màu:

| | ở đâu |
|---|---|
| Vòm khiên trong game | bốn màu trong `S_Khieng.shader` (`_LoiColor`, `_GanColor`, `_VienColor`, `_MangColor`) |
| Đốm sáng bay trong vòm | `startColor` của hệ hạt trong `VfxFactory` |
| Mảnh vỡ, khói, vòng sóng | `ShardKhiengMat`, `KhoiKhiengMat`, `RingKhiengMat` và các gradient của chúng |
| Nền nút | `nenHe` của `IconKyNang.Khieng()` |

`KhiengMat` dựng thẳng từ shader nên **không có material asset đè lên** — sửa giá trị mặc
định trong shader là đủ. Đây là ngoại lệ so với thói quen: chỗ khác trong dự án thì prefab
và material đã lưu luôn thắng giá trị trong code.

Chói loá hơn: `_PhatSang` 1,35 → 1,85 và `_VienChoi` 3,0 → 4,2.

Màu lúc khiên **sắp vỡ** cũng phải đổi theo. Vòm lạnh thì đỏ cam là đủ báo động, nhưng vòm
đã vàng kim rồi thì đỏ cam trông y hệt lúc còn nguyên — phải lệch hẳn sang **đỏ sẫm**.

#### Bảy chỗ vấp khi render trong Blender

**Đổi view transform sang `Standard`.** Mặc định là **AgX**, kéo mọi vùng sáng về trắng —
đẹp cho ảnh phim, nhưng biểu tượng cần đúng cái màu cam/tím mà nó được tô. Vòng corona đầu
tiên ra một vòng trắng bệt.

**Emission phải vừa.** Với `Standard`, cường độ 1,0 đã là trắng. Màu `(0.42, 0.36, 1.0)`
nhân 3,4 ra `(1.43, 1.22, 3.40)`, kẹp về `[0,1]` thành **trắng** — mất sạch sắc tím. Giữ
quanh 1,1–1,3 thì màu còn mà vẫn vượt ngưỡng Glare để loé.

**Nền phải ĐEN, không trong suốt.** Với `film_transparent`, hào quang của Glare lan ra vùng
alpha = 0: màu thì có mà alpha vẫn bằng không, nên đắp lên nền tối là mất sạch.

**Vật nằm đúng trong mặt phẳng ảnh thì ra miếng giấy.** Máy quay trực giao nhìn thẳng
xuống, nên hình nón đặt nằm ngang lần nào cũng thấy từ cạnh — ra tam giác dẹt trắng, đúng
cái lỗi người dùng đã than phiền hai lần. Tinh thể băng phải **chĩa một phần về phía máy
quay** mới thấy được ba mặt và ra khối.

**Lõi phải mảnh hơn hẳn quầng.** Tia sét với lõi bằng 1/3 bề rộng vẫn đọc ra "ống trắng có
viền tím". Một phần năm mới ra "sợi chỉ trắng chạy trong luồng điện".

**Quả cầu đặc đặt sau lưng không thành quầng sáng** — nó thành một cái đĩa trắng như mặt
trăng. Hào quang phải toả ra từ chính chủ thể.

**Blender 5 dời API.** Compositor giờ là `scene.compositing_node_group` (một node group,
không còn `scene.node_tree`); node `Composite` bỏ hẳn, thay bằng `NodeGroupOutput`; mọi tuỳ
chọn của Glare thành **socket**, và menu nhận nhãn hiển thị (`'Fog Glow'`, `'High'`) chứ
không phải hằng (`'FOG_GLOW'`).

### HỒI CHIÊU: SỐ GIÂY VÀ SÁNG DẦN

Kỹ năng vừa dùng thì nút **tối đi rồi sáng dần lên**, kèm **số giây còn lại** ở giữa và một
**cung tiến trình** chạy quanh viền (phần đã hồi xong hiện sáng).

`PlayerController.HoiChieuGiay(skill)` trả về số giây. Tỉ lệ 0..1 có sẵn không đủ: 0,5 của
Quả cầu lửa là một phần tư giây, còn 0,5 của Khiêng là sáu giây — người chơi cần biết còn
phải chờ bao lâu chứ không phải còn bao nhiêu phần trăm. Dưới 10 giây hiện một chữ số lẻ
(`2.7`), từ 10 trở lên thì làm tròn.

Cung tiến trình ghép từ 44 chấm nhỏ chạy dọc vòng tròn — OnGUI không vẽ được hình quạt.

**Style GUI phải đặt lại mỗi lần vẽ, không chỉ lúc tạo.** Đặt `alignment = MiddleCenter`
trong khối `if (style == null)` thì số giây vẫn bám góc trên-trái ô kỹ năng; có cái gì đó
ghi đè lên style sau đó, và sửa ở chỗ tạo thì không ăn.

**Nhập lại script rồi vào Play trong CÙNG một lệnh thì Unity chưa kịp biên dịch** — ảnh
chụp ra vẫn là bản cũ, và tôi đã tưởng bản sửa không có tác dụng. Phải tách: một lệnh nhập
lại, một lệnh vào Play, một lệnh chụp.

### BẢNG TRẠNG THÁI GÓC TRÁI TRÊN

Hai quả cầu máu/năng lượng ở hai góc dưới **đã bỏ**. Thay bằng một bảng xếp dọc ở góc
trái trên, từ trên xuống:

| Dải | Cao | Nội dung |
|---|---|---|
| Tên nhân vật | 26 | `tenNhanVat` (mặc định "PHU THUY"), có bóng chữ |
| Thanh máu | 30 | đỏ, kèm số máu / máu tối đa |
| Thanh khiêng | **9** | tím, **mỏng**, chỉ hiện khi đang có khiêng |
| Thanh năng lượng | 22 | xanh, kèm số |

(đơn vị là điểm ảnh trên màn hình cao 1080, nhân với hệ số `s` cho màn khác)

**Mỗi thứ chiếm một dải cao riêng, cộng dồn xuống bằng biến `y`** — không đặt toạ độ
tay cho từng thanh. Đặt tay thì đổi chiều cao một thanh là phải sửa toạ độ tất cả những
thanh dưới nó, sớm muộn cũng chồng lên nhau.

**Không có khiêng thì để TRỐNG dải đó**, không kéo thanh năng lượng lên lấp chỗ. Kéo lên
thì thanh năng lượng nhảy lên nhảy xuống mỗi lần bật/tắt khiêng, rối mắt.

#### Bốn lớp làm nên hiệu ứng nổi khối

`VeThanh3D` xếp bốn lớp từ dưới lên, thiếu lớp nào là thanh xẹp lại thành hình chữ nhật
phẳng:

1. **Bóng đổ** — hình chữ nhật đen mờ, lệch xuống dưới bên phải 3 điểm. Đây là thứ
   tách thanh ra khỏi nền.
2. **Rãnh chìm** — nền tối, cạnh **trên tối** và cạnh **dưới sáng**. Ngược với một khối
   nổi (trên sáng dưới tối) — nhờ vậy nó chìm xuống.
3. **Phần đầy** — tô bằng **dải màu dọc ba chặng**: sáng trên, đậm giữa, tối dưới.
   Ba chặng chứ không hai: chuyển thẳng sáng → tối chỉ ra một mặt phẳng nghiêng, còn
   sáng – đậm – tối làm mặt thanh **cong** như nửa ống trụ.
4. **Vệt bóng** — dải trắng mờ ở một phần ba trên, giống ánh sáng hắt trên mặt kính cong.

Thêm một vạch sáng ở đầu mút phần đầy để thấy rõ thanh đang đầy tới đâu.

> Máu khiêng trước đây còn được vẽ **lần nữa** ngay trên thanh kỹ năng. Đã bỏ chỗ đó —
> cùng một con số hiện hai nơi là thừa, mà chỗ đó lại chèn ngay trên hàng kỹ năng.

### HAI BẢNG CHỮ TRÊN MÀN ĐÃ TẮT

| Bảng | Trước | Sau |
|---|---|---|
| **Chẩn đoán** (góc trái trên) | `hienChanDoan = true` | **false** — vẫn bật lại được bằng **F12** |
| **Hướng dẫn phím** (giữa màn) | `hintDuration = 14` giây | **0** — không hiện |

Bảng chẩn đoán là công cụ gỡ lỗi (đếm vòng `Update`, xem phím có tới được game không),
nên **tắt mặc định nhưng giữ phím F12** để bật lại khi cần — xóa hẳn thì lần sau game
"không ăn phím" lại phải đoán mò từ đầu.

> Hai giá trị này đã serialize trong **cả hai cảnh**, nên phải sửa **ba nơi**: code,
> `Act1.unity`, `Act2.unity`. Sửa mỗi code thì cảnh vẫn đè giá trị cũ lên.

### NHÂN VẬT CHÍNH HƠI PHÁT SÁNG

Bật emission cho vật liệu `Assets/Materials/Player_PhuThuy.mat` (shader Standard) ở mức
**0,06**, màu hơi ngả xanh lam theo tông màu đêm — nâng độ sáng lên mà không làm nhân
vật đổi màu.

Đo độ sáng trên những điểm thuộc về nhân vật (tách bằng cách chụp một lần tắt hẳn
renderer làm nền):

| Mức emission | Độ sáng nhân vật |
|---|---|
| 0 (tắt) | 152,9 |
| **0,06** | **198,5 (+29,8%)** |
| 0,12 | 237,7 (+55,5%) |
| 0,20 | 292,3 (+91,2%) |

Chọn 0,06 vì từ 0,12 trở lên màu bắt đầu **bợt** — sắc đỏ của áo và chi tiết giáp nhạt
dần, 0,20 thì mất hẳn chiều sâu. Nền chung quanh đo được **119,9 → 120,0**, tức ánh sáng
này **không lan ra cảnh** — chỉ chính nhân vật sáng lên.

> **Cờ GI làm mất emission khi lưu.** Đặt `globalIlluminationFlags = EmissiveIsBlack` thì
> Unity hiểu là "coi emission như màu đen" và **tự tắt từ khoá `_EMISSION`** khi ghi asset
> — render trong phiên này vẫn sáng, nhưng mở Unity lần sau là mất. Phải dùng `None`:
> không đóng góp vào GI nhưng vẫn render bình thường. Kiểm bằng cách **nạp lại asset từ
> đĩa** sau khi lưu, chứ đừng tin bản đang nằm trong bộ nhớ.

### QUÁI RẢI SẴN KHẮP BẢN ĐỒ (Act2)

Vào Act2 là trên bản đồ đã có sẵn **10 mụ phù thủy + 10 bộ xương** đứng rải rác, độc
lập với nhịp thả quái theo đợt. Xem `GameDirector.RaiQuaiKhapBanDo`.

**Khác hẳn `SpawnOne`.** Hàm kia thả quái **quanh người chơi** trong vành đai 14–26 m,
dùng cho từng đợt. Hàm này rải đều khắp đấu trường (bán kính **50 m** trong Act2) để
đi đâu cũng gặp.

**Dùng xoắn ốc vàng, không rải ngẫu nhiên.** Rải ngẫu nhiên thì chỗ túm ba bốn con vào
một góc, chỗ khác trống hoác cả một vùng rộng. Còn chia đều theo góc thì ra một vòng
tròn điều đặn, lộ ngay ra là đặt tay. Góc vàng (137,508°) cho các điểm trải đều mà
vẫn không theo quy luật nhìn thấy được.

**Bán kính phải lấy CĂN BẬC HAI.** Mật độ đều trên **diện tích** chứ không phải trên bán
kính — không lấy căn thì quái dồn hết vào giữa, vì vòng ngoài có diện tích lớn hơn nhiều.

**Xen kẽ hai loại.** Xếp liền khối (10 phù thủy rồi 10 xương) thì phù thủy dồn hết vào
giữa còn xương dạt hết ra rìa, vì bán kính tăng dần theo số thứ tự.

Đo trên 20 điểm thực tế: hai con gần nhau nhất **9,4 m**, cách tâm từ **14,9 đến 45,7 m**,
chia theo 4 góc phần tư ra **5/5/6/4**. Đặt được **20/20** con, không con nào vướng vật cản.

> **Chỉ bật cho Act2.** `GameDirector` dùng cho **cả hai màn**, nên hai số này để **mặc
> định 0** trong code và chỉ đặt 10/10 trong `Act2.unity`. Act1 không có hai dòng đó nên
> dùng mặc định, tức không rải gì.

#### Chùm ba quả cầu lửa

`Fireball.SpawnChum` bắn ba quả cùng lúc, lệch **−11° / 0° / +11°**.

**Xoay quanh trục DỨNG, không quanh trục của người bắn.** Nếu quay quanh trục ngang của
nhân vật thì khi họ nhắm chếch lên hay chếch xuống, ba quả sẽ toè theo một **mặt phẳng
nghiêng** — một quả chúi xuống đất, một quả bay lên trời. Kiểm bằng cách nhắm chếch
xuống rồi đo thành phần dọc của cả ba: **−0,243 cho cả ba** — chùm phẳng, không nghiêng.

> **Sát thương gấp ba nếu trúng hết.** Mỗi quả mang trọn `impactDamage`, nên bắn trúng
> một mục tiêu ở cự ly gần (lúc ba quả chưa kịp toè xa) là ăn đủ ba. Ở xa thì chùm toè
> rộng, thường chỉ trúng một.
>
> Hai quả biên cách nhau `0,389 × khoảng cách`; bán kính nổ là 3,4 m, nên **dưới 9 m thì
> cả ba quả còn phủ chồng lên nhau**. Với `impactDamage` = 55, một lần bấm ở cự ly gần là
> **165 sát thương chạm** cộng bỏng cháy — đọc con số 55 mà quên nhân ba là hiểu sai hẳn
> sức mạnh của kỹ năng này.

#### Mưa băng và Sấm sét có tầm thả 12 m

Hai kỹ năng vùng này **trước đây không có giới hạn tầm nào cả** — chúng rơi đúng chỗ con
trỏ chạm mặt đất, mà tia ngắm bắn xa 300 m: ở màn 16:9 người chơi thả được tới **45 m**,
tức tận mép màn hình, xa hơn cả tầm để ý của mọi loại quái.

`iceRange` và `boltRange` trong [PlayerController.cs](Assets/Scripts/Player/PlayerController.cs)
giới hạn cả hai ở **12 m**. Ngắm xa hơn thì phép **rơi ở mép tầm**, giữ nguyên hướng đã
ngắm — chứ không phải từ chối đánh. Bấm kỹ năng mà không có gì xảy ra thì người chơi tưởng
nút hỏng; rơi ở mép tầm thì ít ra còn thấy phép bay ra và tự hiểu là mình ngắm quá xa.

Việc kẹp nằm ngay đầu `CastAt`, tức áp cho **mọi đường vào**: phím tắt, chuột, và nút tròn
cảm ứng. Sau khi kẹp ngang thì độ cao được lấy lại từ mặt đất — giữ nguyên `y` cũ thì phép
treo lơ lửng trên sườn đồi hoặc thụt xuống dưới đất.

**Quả cầu lửa KHÔNG bị kẹp** (`TamCuaKyNang` trả 0 cho nó). Nó *bay* đi chứ không rơi
xuống một điểm; kẹp điểm ngắm chỉ làm đổi hướng bắn chứ không làm nó bay ngắn lại — tức
chẳng được gì mà còn bắn lệch.

Đo khi chạy thật, ngắm vào điểm cách 30 m:

```
Mua bang : tam vung roi cach nguoi choi 12,00 m
Sam set  : tam vung roi cach nguoi choi 12,00 m
Qua cau lua : van bay tiep, khong bi kep
```

#### Thiên thạch — hai phần tách bạch

Kỹ năng này gồm **hai lớp riêng**, cố ý tách ra thành hai script:

| Script | Lo việc gì |
|---|---|
| [ThienThach.cs](Assets/Scripts/Skills/ThienThach.cs) | Khối đá rơi, va chạm, vụ nổ |
| [VungLua.cs](Assets/Scripts/Skills/VungLua.cs) | Bãi đất cháy còn lại, sát thương liên tục |

**Sinh ra trên trời, không phải từ tay phù thủy.** Bốn phép kia đều bay ra từ
`castPoint` ở tay; thiên thạch thì xuất hiện thẳng trên đầu điểm ngắm, cách 26 m,
lệch ngang 7 m cho đường rơi hơi xiên. Rơi thẳng đứng thì nhìn như thả thang máy.

**Va chạm phải quét cả đoạn vừa đi.** Thiên thạch to và rơi 34 m/giây — mỗi khung
hình đi hơn một mét. Kiểm tra từng điểm thì nó xuyên thẳng qua mặt đất; phải
`SphereCast` đúng đoạn vừa trôi qua trong khung hình đó.

**Sát thương của bãi lửa tính theo GIÂY**, không theo lần đánh: `damagePerSecond`
được chia đều ra từng nhịp 0,25 giây. Đổi nhịp nhanh chậm bao nhiêu cũng không làm
bãi lửa mạnh lên hay yếu đi — chỉ đổi cảm giác mượt hay giật.

Hai giây cuối lửa **yếu dần** rồi mới tắt, và hệ hạt **ngừng phun trước** chứ không
biến mất một cái — xem [TatDanVungLua.cs](Assets/Scripts/Vfx/TatDanVungLua.cs).

**Ngọn lửa phải thấp mà dày.** Hai lần tôi chỉnh hụt, đều đo được:

| Lần | Số hạt cùng lúc | Ngọn cao nhất | Nhìn ra |
|---|---|---|---|
| Đầu | ~500 | **5–6 m** | cột lửa cao gấp mấy lần bia mộ, che kín khung hình |
| Sau khi giảm | **48** | 3,9 m | vài mảnh lửa rời trôi lơ lửng, thấy rõ cạnh tam giác của ảnh ngọn lửa |
| Chốt | **174** | 4,2 m | cả vùng đất cháy liên tục |

(Với bán kính 4,5 m hiện tại thì con số là **206 hạt** — mật độ tính theo bán kính nên
vùng rộng ra bao nhiêu, lửa vẫn dày y như vậy.)

#### Khối đá phải nhìn thấy được

Muốn thấy tảng thiên thạch bên trong thì **không được bọc nó bằng vỏ hình cầu**. Lúc đầu
tôi bọc hai lớp vỏ cầu bằng vật liệu lửa phát sáng — vỏ cầu bao kín khối đá, hai lớp cộng
dồn thành một cục trắng xoá. Đo trên ảnh: **19.614 điểm cháy trắng**.

Thay hai lớp vỏ đó bằng **lửa dạng hạt** phun từ một vỏ cầu mỏng (`radiusThickness` = 0,12)
ngay sát rìa khối đá: hạt rời rạc, luôn có khe hở giữa chúng, nên mặt đá vẫn lộ ra. Đo lại:
còn **4 điểm** cháy trắng.

Hai chi tiết nữa phải đi kèm:

- **Khối đá dùng `ProcMesh.Rock`**, không dùng hình cầu trơn — cầu trơn nằm trong bụng lửa
  chỉ còn là một vệt sáng tròn, không ai nhận ra đó là tảng đá.
- **Đuôi lửa phun lùi về phía sau** (`z` âm) chứ không phun ngay tại tâm. Phun tại tâm thì
  hạt lửa sinh ra trong lòng khối đá và phủ kín lên nó.

Độ phát sáng của đá để **0,60**: để 1,15 thì cả khối sáng đều một màu cam, mất hết cảm giác
đá; 0,60 thì mặt hướng về phía lửa sáng lên còn mặt khuất tối đi, nhìn ra khối đá đang nung đỏ.

Điểm mấu chốt là **hạt nhỏ nhưng thật nhiều** để chúng chồng lên nhau thành mảng
lửa liền; hạt nhỏ mà thưa thì lộ ngay từng miếng ảnh rời.

#### Lốc xoáy — khói bụi thay cho mảnh vỡ

Trước đây lốc cuốn theo **hai** bầy mảnh: `Debris` cỡ 0,22–0,95 m và `Grit` cỡ
0,05–0,20 m. Bầy `Debris` đã bị bỏ hẳn — những tảng to bằng cả cái bia mộ bay lơ
lửng quanh thân lốc nhìn ra mảnh giấy đen cắt rời chứ không ra đất đá. Giờ chỉ còn
`Grit` li ti, còn phần "lốc bốc thứ gì lên" được thể hiện bằng **khói bụi đen kéo
dài phía sau**.

Hệ hạt `KhoiBui` phun ở **không gian thế giới** và hạt gần như đứng yên tại chỗ sau
khi sinh ra — lốc chạy tới trước, đám khói ở lại đằng sau thành một vệt dài đánh dấu
đường nó vừa đi. Phun theo vật thể thì cả đám khói đi theo lốc và không bao giờ thấy
được cái vệt đó. Hạt sống 2,5–5 giây và **nở rộng dần** theo đời, nên vệt càng về sau
càng bung to và tan loãng.

**Càng đi càng nhiều khói.** `Tornado.CangDiCangNhieuKhoi` tăng lượng phun từ
`khoiBuiDau` = 30 hạt/giây lên `khoiBuiCuoi` = 210 theo bình phương thời gian sống,
nên lúc mới ra chỉ lấm tấm bụi, đi một lát là cả dải khói đen un un. Hết giờ thì
**ngừng phun** chứ không tắt ngay — đám khói đã phun ra vẫn tự tan, nên vệt mờ dần
chứ không biến mất một cái.

> ⚠️ Sửa `VfxFactory` **không đủ**: lốc lúc chơi được dựng từ `Assets/Prefabs/Skill_LocXoay.prefab`
> đã nướng sẵn. Phải sửa cả prefab, và khi thêm hệ hạt vào prefab thì phải gán vật liệu
> **là tài sản** — vật liệu do `NewPS` tạo lúc chạy sẽ mất sạch khi lưu prefab.

| **4** | **Lốc xoáy** | 55 | **2 giây** | Ném ra một cơn lốc cao 13m kèm tia sét, **tự trườn đi** trong 6 giây. Quái lọt vào bị **nhấc bổng khỏi mặt đất**, bay vòng quanh trục lốc theo đúng chiều xoay, bị kéo dần lên cao và **đi theo lốc**. Mất 20 máu/giây khi đang bị cuốn, thỉnh thoảng ăn thêm một tia sét. Lúc lốc tan thì rơi xuống và chịu thêm sát thương theo độ cao. |

> Mẹo: Sấm sét kéo dài 2.4 giây và thả khoảng **22 tia** — nhiều quái sẽ dính choáng
> liên tiếp, rất hợp để chặn một đợt quái đông đang lao tới.

### Luật chơi
- Quái tràn vào theo từng **đợt**; hết một đợt thì vài giây sau có đợt mới, đông hơn.
- **4 loại quái**: **Bộ xương** (to cao đầy gai, chịu lạnh tốt), **Phù thủy** (đứng xa ném
  quả cầu lửa), **Quỷ dữ** (gọi thiên thạch) và **Quỷ cây** (tia sét xanh, chạy nhanh hơn
  người chơi). Hai loại cuối có dòng riêng, không theo nhịp đợt.
- Hết máu là thua → bấm **R** chơi lại hoặc **ESC** về menu.

### Ba loại quái đã bị bỏ khỏi game

**Quỷ lùn**, **Xác sống** và **Quỷ khổng lồ** không còn xuất hiện nữa. `GameDirector.PickType`
— đường duy nhất sinh quái theo đợt — giờ chỉ trả về Bộ xương và Phù thủy; quái rải sẵn khắp
bản đồ ở Act2 vốn đã chỉ có hai loại đó, và Quỷ dữ đi theo dòng riêng.

Tỉ lệ mới: **16% Phù thủy, 84% Bộ xương**, không đổi theo đợt.

Đo khi chạy thật, đợt 1 với 22 quái mỗi đợt:

```
DOT 1: 23 quai
   Enemy_BoXuong   19   (83%)
   Enemy_PhuThuy    4   (17%)
```

Không có con nào thuộc ba loại đã bỏ. Trước khi sửa, **Quỷ lùn chiếm khoảng 38% ngay ở đợt
1** (nó là nhánh `return` mặc định) — xác suất không gặp con nào trong 23 lần bốc là chừng 5
phần triệu, nên phép đo này đủ để kết luận.

> ⚠️ **Nhịp đợt mất phần leo thang.** Trước đây Xác sống bắt đầu ra từ đợt 2 và Quỷ khổng lồ
> từ đợt 3, nên đợt càng về sau quái càng mạnh chứ không chỉ càng đông. Giờ mọi đợt đều là
> một rổ Phù thủy và Bộ xương như nhau — chỉ số lượng tăng. Muốn khó dần trở lại thì nâng tỉ
> lệ Phù thủy theo `Wave` trong `PickType`.

**File và code của ba loại đó vẫn còn nguyên** — prefab `Enemy_QuyLun`, `Enemy_XacSong`,
`Enemy_QuyKhongLo` trong `Assets/Prefabs`, hình dạng trong `MonsterFactory`, chỉ số trong
`EnemyFactory.BuildFromCode`. Chúng chỉ không còn ai gọi tới. Muốn bật lại thì thêm vào
`PickType` là xong; muốn xoá hẳn thì xoá file, nhưng đó là việc không lùi lại được.

> ⚠️ **Đừng xoá phần tử khỏi `MonsterType`.** Số thứ tự của enum được lưu thẳng vào mảng
> `enemyPrefabs` trong cảnh; bỏ một cái ở giữa là mọi con quái còn lại đổi thành loại khác.
> Ba loại đã bỏ được đánh dấu `(da bo)` trong comment và giữ nguyên vị trí.

### Phù thủy — quái đánh từ xa

Bộ xương lao vào đánh giáp lá cà; riêng mụ phù thủy **đứng xa ném quả cầu lửa**,
nên phải xử lý khác: hoặc lao vào ép sát, hoặc nấp sau vật cản, hoặc bắn hạ trước.

| | |
|---|---|
| Máu | 95 |
| Kháng | Chịu lửa **+35%** (bắn lửa vào ít ăn thua), sợ lạnh **−20%** (mưa băng rất hiệu quả) |
| Tầm ném | 12 m |
| Lùi ra khi | Người chơi vào gần hơn **6,5 m** — vừa lùi vừa ném |
| Hồi chiêu | 3,2 giây |
| Quả cầu | 16 sát thương, nổ bán kính 2,6 m, bay 13 m/s |
| Xuất hiện | **Ngay từ đợt 1**, chiếm khoảng 16% số quái mỗi đợt. Riêng đợt 1 được **cộng thêm 1 mụ chắc chắn có**, nên đợt đầu là 6 quái và luôn gặp ít nhất một mụ |

Quả cầu của mụ ta **chỉ gây sát thương cho người chơi**, không bao giờ đánh trúng đồng bọn.

### Nhân vật và quái dùng model dựng sẵn

Ba nhân vật **không vẽ bằng code**, mà lấy model đưa từ Meshy vào:

| Nhân vật | Model | Lối đánh |
|---|---|---|
| **Người chơi** | `Voidborn Lich King` (ảnh màu 8K) | Bốn phép, mỗi phép một tư thế riêng |
| **Phù thủy** (quái) | `Veil of the Abyss` | Đứng xa ném cầu lửa |
| **Bộ xương** (quái) | `Crimson Bone Warlord` | Lao vào chém cận chiến |

Cả ba nằm trong `Assets/MeshyImports/` — thư mục này **không bị xoá** khi bake.

#### Bốn tư thế niệm chú của người chơi

Model chỉ kèm một hoạt hình đi bộ, nên bốn động tác phép được dựng tại chỗ trong
[NguoiChoiHoatHinh.cs](Assets/Scripts/Player/NguoiChoiHoatHinh.cs):

| Phím | Phép | Động tác |
|---|---|---|
| 1 | Quả cầu lửa | Rút tay ra sau vai rồi **đâm thẳng** ra trước |
| 2 | Mưa băng | Giơ **cả hai tay** lên trời rồi bổ rộng sang hai bên |
| 3 | Sấm sét | Ngửa người, giơ tay vút lên trời rồi **bổ mạnh** xuống đất |
| 4 | Lốc xoáy | **Quạt một vòng** quanh người, xoay cả thân lấy đà |
| 5 | Thiên thạch | **Ngửa hẳn người ra sau**, vút cả hai tay lên trời gọi đá rơi, rồi bổ mạnh về trước |
| 6 | Khiêng bảo vệ | **Thu người lại**, hai tay khoanh chéo trước ngực rồi **bung mạnh ra hai bên** — phép duy nhất không vươn về phía trước |

Thời điểm phép bay ra khớp với đúng lúc tay vung tới — chỉnh bằng `mocPhepBayRa`
(đang để 0,41). Đặt sai số này thì tay vung một đằng mà phép bay ra một nẻo.

**Hoà về tư thế đứng yên.** Mỗi lần rời khỏi một tư thế — dừng bước đột ngột, hay niệm
chú xong — nhân vật hoà dần về tư thế đứng yên trong `thoiGianVeChuan` (đang để 0,26 giây)
thay vì đứng chết ở tư thế cuối. Muốn mượt hơn nữa thì tăng số đó lên, nhưng quá 0,4 giây
là bắt đầu thấy nhân vật uể oải.

#### Hai con quái cũng hoà về tư thế đứng yên

[ModelHoatHinh.cs](Assets/Scripts/Enemies/ModelHoatHinh.cs) dùng chung cách làm với nhân
vật chính. Trước đây nó dính đúng hai lỗi mà người chơi đã được sửa:

**Lỗi 1 — ra đòn xong kết cứng ở tư thế gồng.** Công thức cũ là `go = 1 - bung * 0.85`.
Cuối đòn thì `bung` về 0, nên `go` hoá ra bằng **1** — tức phần gồng sức đang ở mức mạnh
nhất đúng lúc đồng hồ hết giờ, rồi giật phắt về tư thế thường. Đo lại tại `t = 1,000`:

| | `bung` | `go` |
|---|---|---|
| Cũ | 0,000 | **1,000** ← gồng hết cỡ rồi giật |
| Mới | 0,000 | **0,000** ← kết thúc đúng tư thế chuẩn |

**Lỗi 2 — dừng bước là đông cứng giữa bước chân.** `Animation.Stop()` của hệ hoạt hình cũ
để nguyên khung xương ở đúng khung hình cuối của clip. Code cũ chỉ trả về tư thế chuẩn cho
**3 khớp** (spine, head, hips) — mà dưới `Hips` có **24 khớp**. Hai mươi mốt khớp còn lại
(cả hai chân, hai tay, chuỗi sống lưng) kẹt nguyên ở giữa bước.

Nay trước khi `Stop()` thì ghi lại tư thế đang có, rồi hoà dần cả 24 khớp về đích thật sự
trong `thoiGianVeChuan` (0,26 giây) — y hệt người chơi. Lúc gục ngã cũng trả về tư thế
chuẩn trước rồi mới đổ người, không thì cái xác nằm đó với một chân vẫn đang bước.

**Hai điểm phụ phải sửa kèm:**

- `Animation` của model để `playAutomatically = true`, tức clip đi bộ tự chạy và lấy mẫu
  khung xương ngay từ khung hình đầu. Đọc tư thế chuẩn sau đó sẽ ra tư thế **đang bước**
  chứ không phải đứng yên, và mọi lần hoà về đều về nhầm chỗ. Nay tắt tự phát trong `Awake`.
- `EnemyFactory` gọi `AddComponent<ModelHoatHinh>()` **rồi mới** gán `hips`, `spine`… mà
  `AddComponent` trên vật đang bật thì chạy `Awake` ngay lập tức — lúc đó mọi khớp còn null.
  Nay tư thế chuẩn được đọc trễ, ở lần `Update` đầu tiên chạy được.

> Điểm này **không ảnh hưởng game đang chơi**: quái lúc chơi được dựng từ prefab trong
> `Assets/Prefabs`, mà prefab đã lưu sẵn các khớp nên `Awake` thấy đủ. Nó chỉ sai trên
> đường `BuildFromCode` (khi thiếu prefab). Sửa cho chắc, không phải chữa cháy.

#### Độ nét của model

Ảnh vân của mọi model Meshy được `NangChatLuongAnh` trong `AssetBaker` tự chỉnh mỗi lần bake:

| Thiết lập | Giá trị | Vì sao |
|---|---|---|
| Kiểu nén | **BC7** (thay DXT1/DXT5) | DXT chia ảnh thành ô 4×4 rồi rút mỗi ô còn hai màu — giáp nhiều chi tiết sẽ lộ ô vuông. BC7 tốn bộ nhớ như nhau nhưng giữ nhiều màu hơn hẳn |
| Lọc xiên | **4** | Camera nhìn chéo xuống 48°, để mức 1 là mặt đất càng xa càng nhoè và rung rám |
| Trần kích thước | **8192** | Chỉ là trần, không ép: ảnh gốc 2048 vẫn giữ 2048. Nhưng ảnh 8K thì được dùng trọn |
| Khử răng cưa | **8×** (thiết lập dự án) | Cạnh giáp và vai gai là những đường chéo mảnh; mức 2× cũ làm chúng thành bậc thang lởm chởm khi zoom gần |

> **Zoom vào thấy vỡ thì hãy xem lại độ nét của khung nhìn trước.** Nhân vật vẽ vào
> khoảng 300 điểm ảnh chiều cao thì không thể hiện ra nhiều chi tiết hơn thế, dù ảnh
> vân có sắc đến đâu. Cùng một camera, dựng ở 424 px thì vỡ, dựng ở 1272 px thì thấy
> rõ từng mảnh giáp và hình thêu trên vạt áo. Kéo khung Scene/Game rộng hết cỡ trước
> khi kết luận là model hỏng.

**Cái giá của ảnh 8K.** Nhân vật hiện dùng ảnh màu 8192×8192, ảnh gân và ảnh kim loại
4096. Đo bằng Profiler:

| Ảnh | Kích thước | Bộ nhớ card đồ hoạ |
|---|---|---|
| `meshy_basecolor` | 8192² | **170 MB** |
| `meshy_normal` | 4096² | 42 MB |
| `meshy_metallic_smoothness` | 4096² | 42 MB |
| | | **256 MB cho một nhân vật** |

Chấp nhận được vì đây là nhân vật chính, chỉ có **một** trên màn hình. **Đừng dùng ảnh 8K
cho quái** — mỗi đợt có tới hơn chục con, nhân lên là hết sạch bộ nhớ card đồ hoạ.

##### Quỷ dữ và Quỷ cây đã lỡ dùng 8K — đã hạ về 2048

Lời dặn ngay trên bị chính hai con quái Meshy vi phạm: cả `Enemy_QuyDu` lẫn `Enemy_QuyCay`
đều nhập vào với trần **8192**, mà chúng có **7 con và 4 con** cùng lúc trên sân.

```
                    truoc                sau
basecolor           8192²  170,7 MB      2048²  10,7 MB
normal              4096²   42,7 MB      2048²  10,7 MB
metallic_smoothness 4096²   42,7 MB      2048²  10,7 MB
moi con              256,0 MB             32,0 MB
hai con              512,0 MB             64,0 MB
```

**Đây là con số đúng cho cả bản WebGL.** `maxTextureSize` khối chung là 8192 và khối ghi đè
riêng cho WebGL **chưa bật** (`overridden = false`) — cái số 2048 nằm sẵn trong khối đó chỉ
là giá trị mặc định chưa có hiệu lực, nhìn qua rất dễ tưởng WebGL đã tự thu nhỏ rồi.

**Nhìn không khác.** Chụp cùng góc, cùng đèn, trước và sau:

```
                     lech trung binh   max   >2      >8
Quy du   góc cận         0,48/255       34   6,4%   0,4%
Quy du   góc xa          0,03/255       24   0,3%   0,0%
Quy cay  góc cận         0,54/255       47   7,0%   1,2%
Quy cay  góc xa          0,05/255       17   0,6%   0,0%
```

Góc "cận" ở đây là mặt chiếm gần hết khung 700 px — **gần hơn nhiều** so với góc chơi thật,
nơi một con quái cao 2 m chỉ chiếm 80–150 điểm ảnh. Ở khoảng cách đó mipmap lấy mẫu ở mức
128–256 px, tức phần lớn 8192 pixel kia chưa bao giờ được nhìn thấy.

Bộ xương và Phù thuỷ không dính lỗi này: chúng **không có ảnh màu nào cả** — vật liệu chỉ
là Standard shader tô một màu phẳng, mượn thêm `meshy_normal` 2048². Mỗi con 10,7 MB.

> ⚠️ Bản có rig của model người chơi **bị mất cây gậy** — bước dựng xương của Meshy
> chỉ giữ lại phần cơ thể. Hiện nhân vật niệm chú bằng tay không. Muốn có gậy thì
> phải dựng riêng rồi gắn vào khớp `RightHand`.

Mỗi model Meshy chỉ kèm **đúng một** hoạt hình đi bộ. Ba tư thế còn thiếu (đứng yên, ra đòn,
gục ngã) được dựng tại chỗ bằng cách xoay thẳng khớp xương trong
[ModelHoatHinh.cs](Assets/Scripts/Enemies/ModelHoatHinh.cs). Có hai kiểu ra đòn:

- `NemPhep` — ngửa người gồng sức rồi hất **cả hai tay** về trước (phù thủy)
- `ChemToi` — vung tay phải lên cao ra sau, xoay người lấy đà rồi **bổ mạnh** xuống,
  tay trái đưa khiên lên che (bộ xương)

**Thêm hoặc thay model khác:** bỏ file vào `Assets/MeshyImports` rồi bake lại. Trong
`AssetBaker` gọi thêm một dòng, đưa vào **mẩu tên riêng** của model:

```csharp
lib.enemies[(int)MonsterType.Skeleton] = NuongQuaiTuModel(
    "Bone_Warlord", "Enemy_BoXuong", new Color(0.80f, 0.76f, 0.68f),
    EnemyFactory.LapRapBoXuong);
```

Mẩu tên riêng đó là bắt buộc. Trước đây tôi tìm file đầu tiên có chữ *Walking* trong cả thư mục;
đến khi có hai model thì phù thủy suýt bị dựng bằng đúng cái model bộ xương.

Khớp xương phải đặt tên theo lối thông dụng: `Hips`, `Spine`, `Head`, `LeftArm`,
`LeftForeArm`, `RightArm`, `RightForeArm`, `RightHand`. Lò nướng tự đổi cách nhập sang
Legacy và bật lặp cho clip đi bộ.

Cả hai model Meshy hiện tại **không kèm ảnh màu**, chỉ có ảnh gân, nên mang màu phẳng.
Nếu sau này bạn xuất lại **có kèm ảnh màu**, cứ để file ảnh cạnh file FBX — hàm
`MacAoChoModel` trong `AssetBaker` tự tìm và dùng, không phải sửa code.

---

## Phần 3 — Trong Project có những gì

```
Assets/
  Scenes/       MainMenu.unity, Act1.unity                 (2 màn hình, đã đặt sẵn đầy đủ vật thể)
  Prefabs/      30 prefab - bấm vào là xem/sửa được ngay
  Models/       27 file .asset chứa ~150 hình khối 3D (mỗi nhân vật một file, như file .fbx)
  Terrain/      địa hình mặt đất (.asset) và 4 lớp vật liệu (.terrainlayer)
  Materials/    52 vật liệu .mat (áo choàng, da quái, xương, đá, lửa, băng, bầu trời...)
  Textures/     25 file .png hoa tiết (vải, da, xương, đá, ngọn lửa, bông tuyết...)
  Shaders/      8 shader tự viết (lửa, băng, vỏ băng, hạt, bầu trời, bloom, vải hai mặt)
  Scripts/      Toàn bộ mã nguồn, chú thích tiếng Việt
  Editor/       Công cụ trong menu "Diablo 2.5D"
```

### Danh sách Prefabs

**Nhân vật & quái vật**
- `Player_Sorceress` — đại phù thủy (bộ xương khớp, hoạt hình, điều khiển, máu/năng lượng, đèn theo người)
- `Enemy_QuyLun`, `Enemy_BoXuong`, `Enemy_XacSong`, `Enemy_QuyKhongLo` — 4 loại quái, mỗi con có sẵn máu, sát thương, tầm đánh, AI

**Kỹ năng**
- `Skill_QuaCauLua` — quả cầu lửa (lõi lửa, vỏ lửa, 3 hệ hạt, đèn nhấp nháy, script `Fireball`)
- `Skill_MuaBang` — bộ điều khiển phép Mưa băng (bán kính, thời gian, sát thương, độ đóng băng)
- `Skill_SamSet` — bộ điều khiển phép Sấm sét (bán kính, nhịp thả sét, sát thương, **tỉ lệ choáng**)
- `Skill_LocXoay` — cơn lốc: 3 lớp vỏ xoay + bụi + hạt cát + khói bụi đen phía sau + mây đỉnh, kèm script `Tornado`

**Hiệu ứng**
- `Vfx_NoLua`, `Vfx_NoBang`, `Vfx_VungBaoTuyet`, `Vfx_TangBangRoi`,
  `Vfx_BongChay`, `Vfx_VoBang`, `Vfx_TichTuLua`, `Vfx_TichTuBang`, `Vfx_TrungDon`
- `Vfx_VungGiong`, `Vfx_SetChamDat`, `Vfx_Choang`, `Vfx_TichTuSet`

> Tia sét không có prefab riêng vì đường gấp khúc của nó được **tính lại mỗi vài phần
> trăm giây** ngay trong lúc chơi ([`Vfx/LightningArc.cs`](Assets/Scripts/Vfx/LightningArc.cs)),
> nên không tia nào giống tia nào.

**Cảnh vật**
- `Prop_LoLua` (lò lửa cháy), `Prop_DaTang_1..6`, `Prop_CotDa_1..3`, `Prop_KhucXuong`
- `Prop_CayChet_1..3` (cây khô), `Prop_CayXanh_1..4` (cây còn lá), `Prop_BuiRam_1..4` (bụi rậm)

### Cấu trúc một màn chơi (Hierarchy)

```
Act1
├── World
│   ├── MatDat          Unity Terrain 4 lớp vật liệu, tô tay được
│   ├── ThamCo          ~45 mảng cỏ, mỗi mảng gộp hàng trăm bụi vào 1 mesh
│   ├── VachDa          76 tảng đá bao quanh đấu trường
│   ├── TrangTri        đá phủ rêu, bụi rậm, cây, cột đổ nát, xương
│   ├── NghiaDia        các cụm mộ và hòm mộ hé nắp
│   ├── LeuThayMo       4 căn lều xương, mỗi căn 4-5 sọ cắm trên đầu giáo
│   └── LoLua           3 lò lửa cháy bập bùng
├── Player              prefab phù thủy
├── Main Camera         CameraRig (4 góc nhìn) + SimpleBloom
├── Moonlight           ánh trăng + đổ bóng
├── GAME                GameBootstrap + GameAssets (thư viện prefab)
├── GameDirector        thả quái theo đợt
└── HUD                 hai quả cầu máu/năng lượng, thanh kỹ năng
```

### Lều thầy mo xương

Bốn căn rải quanh bản đồ, dựng bằng `Assets/Scripts/Art/LeuFactory.cs`.

| Bộ phận | Chi tiết |
|---------|----------|
| Khung | 7 cây — xen kẽ xương đùi và cán giáo gỗ mục — tựa vào nhau ở những độ cao khác nhau, ngọn chọc vượt lên trời |
| Sọ người | 4–5 cái cắm xiên trên đầu giáo, quay mặt ra ngoài, cách nhau ít nhất 0,52 m. Mỗi cái **hạ hàm la hét** được như xương treo trên cây |
| Vách | Giẻ rách phủ nửa dưới, mép trên xé nham nhở, thủng vài lỗ nhìn thấu vào khung. Vải có **cả hai mặt** nên đứng trong lều nhìn ra vẫn thấy |
| Cửa | Một khe hở không phủ vải, quay hướng ngẫu nhiên |
| Bên trong | Một đốm lửa ma xanh hắt sáng ngược lên đám sọ |
| Treo lủng lẳng | 3–4 chùm bùa: xích sắt, sọ nhỏ, xương vụn, tấm gỗ mục buộc giẻ — tất cả đều lắc lư |

Muốn đổi số lượng hay chỗ đặt thì sửa `DatLeuThayMo` trong `Assets/Editor/AssetBaker.cs`:
`ChuaDuong` là khoảng chừa ra hai bên mép đường (đang để 2,6 m), `CachNhau` là khoảng cách tối thiểu giữa hai căn (13 m).

---

## Vòng phép Mưa băng / Sấm sét méo mó quanh bia mộ

Vòng sáng dưới chân hai skill này gãy khúc thành những hình thù kỳ quặc quanh mỗi tấm bia,
mỗi gốc cây. Hai nguyên nhân độc lập, phải chữa cả hai.

### 1. Vòng bám nhầm vào đồ đạc thay vì bám địa hình

`GroundRing` dựng vòng thành một cái đĩa nhiều mảnh, mỗi đỉnh **bắn một tia xuống** để lấy độ
cao. Mặt nạ của tia là `Ground + Default` — mà lớp `Default` chính là nơi của **458 bia mộ,
229 tảng đá và 58 gốc cây**. Tia trúng nóc chúng thì đỉnh vòng bị kéo lên theo.

Đo trên một vùng bán kính 5,8 m có 10 bia mộ bên trong:

```
                              diem leo len vat can    cao nhat
mat na CU (Ground+Default)         45 / 205            1,52 m
mat na MOI (chi Ground)             0 / 205                 -
```

Đúng cái bẫy đã vấp ở chỉ báo ngắm kỹ năng. Vòng phép phải bám theo **địa hình**, không bám
theo đồ đạc đứng trên địa hình.

### 2. Vòng bị vật cản che khuất

Sửa xong mặt nạ thì vòng tròn đều trở lại, nhưng vẫn bị thân cây và bia mộ **che mất** từng
đoạn. Người chơi cần nhìn thấy trọn vẹn cái vòng để biết phạm vi phép tới đâu.

`ZTest` được đưa ra thành **thuộc tính** của `Diablo25D/ParticleAdditive` chứ không viết cứng
`ZTest Always`: cả trăm hiệu ứng dùng chung shader này, và gần hết trong số đó **phải** bị vật
cản che mới đúng. Chỉ vài cái mới cần vẽ xuyên.

`GroundRing.xuyenVatCan` bật cho đúng ba chỗ — vòng vùng Mưa băng, vòng vùng Sấm sét, và
khoanh báo trước của tia sét (bị che mất thì người chơi không kịp tránh). Các vòng khác giữ
nguyên: chúng là vật trong cảnh, bị bia mộ che là đúng.

> ⚠️ **Và phải đặt lại sau khi tạo từ prefab.** `Vfx_VungBaoTuyet` / `Vfx_VungGiong` đã nướng
> từ trước, trong đó `GroundRing` vẫn giữ `xuyenVatCan = false`. Sửa trong `BuildXxxField` thì
> chỉ ăn khi **không** có prefab — tức là không bao giờ. Cùng cái bẫy đã vấp với `catchRadius`
> của Lốc xoáy.

### Tia sét và tảng băng vẫn bị cản — đúng như thế

Chỉ **vùng ánh sáng** mới vẽ xuyên. Tia sét giáng xuống và tảng băng rơi vẫn va vào vật cản
như cũ; không đụng gì tới phần đó.

```
sau khi sua, do tren dia 205 dinh trong vung 10 bia mo:
   leo len vat can = 0
   lech khoi mat dat = 0,0000 m   (dung bang lift 0,14 o moi dinh)
   _ZTest = 8 (Always), renderQueue = 3100
```

### `hold` của vòng: một báo động giả, và vì sao vẫn nên sửa

Tôi đã báo rằng prefab lưu `hold` cứng **5,4 s** / **2,8 s** nên vòng tắt sớm hơn thời gian
phép. **Sai.** Đo ra thì hai con số đó khớp chính xác:

```
Skill_MuaBang.duration = 5,00   ->  can hold 5,40   prefab luu 5,40   KHOP
Skill_SamSet.duration  = 2,40   ->  can hold 2,80   prefab luu 2,80   KHOP
```

Sai ở phép đo, không ở game: lúc thử tôi tự truyền `duration = 300` để vòng sống lâu mà chụp,
rồi thấy `hold` là 5,4 và tưởng nó ngắn hơn thời gian phép.

Vẫn giữ thay đổi, nhưng vì lý do khác: `hold` giờ **tính từ `duration` được truyền vào** thay
vì đọc con số nướng sẵn. Hai chỗ đang trùng nhau chỉ vì cùng lấy giá trị mặc định; ngày nào ai
đó sửa `duration` trong `Skill_MuaBang.prefab` thì vòng vẫn tắt theo con số cũ. Bỏ bản chép
tay thứ hai đi thì không còn cửa cho chúng lệch nhau.

```csharp
gr.hold = duration + 0.4f;      // mot nguon su that duy nhat
```

Hệ hạt trong hai prefab cũng đã đúng: `duration` 5,00 / 2,40 và `loop = true`, gốc bị
`AutoDestroy` sau `duration + 3` nên chúng tan dần chứ không tắt phụt.

## Bia mộ Act2: mục nát, sứt mẻ, rêu bám, mốc và vết máu

458 bia mộ, 229 tảng đá và 5 nhà mồ trước đây đều dùng `Standard` một màu, nhìn ra khối nhựa
trơn. Nay có hạt đá, khe nứt, mảng bong, rêu bám mặt trên, mốc và vết máu chảy — và **119 tấm
bị sứt mẻ thật ở mức lưới**.

### Một shader mới, ba mặt nạ dồn vào một ảnh

`Diablo25D/DaMoTriplanar`. Lại là triplanar vì cùng lý do với vỏ cây: cả 36 lưới đều
`uv.Length = 0`. Bộ ảnh sinh bằng `Act2DaMo.Dung()` (menu **16**):

```
DaMo_Mau.png    RGB = da muc,  A = VET VO
DaMo_Gan.png    anh gan
DaMo_MatNa.png  R = reu,  G = moc,  B = vet mau
```

Ba mặt nạ dồn vào **một** ảnh chứ không ba ảnh riêng: shader phải lấy mẫu ba lần cho ba hướng
chiếu, nên mỗi ảnh thêm là thêm **chín** lần lấy mẫu cho một điểm ảnh.

> `DaMo_MatNa.png` phải **tắt sRGB**. Nó không phải màu mà là ba con số 0..1; để sRGB thì Unity
> áp đường cong gamma lên chúng, và mức rêu 0,5 đọc ra thành 0,73 — rêu dày gấp rưỡi ý đồ.

### Rêu mọc ở đâu là có lý do

Rêu cần ẩm, và chỗ ẩm là chỗ nước đọng lại. Nên lượng rêu = mặt nạ **nhân** với hai hệ số
hình học: `saturate(pháp tuyến thế giới · trục Y)` và độ lõm của bề mặt. Thiếu hai hệ số đó
thì rêu phủ đều cả mặt dưới lẫn mặt đứng — ra một lớp sơn xanh chứ không ra rêu.

Máu thì ngược lại: **không** phụ thuộc hướng, và hình dạng vệt chảy nằm sẵn trong ảnh mặt nạ
(nhiễu kéo dãn `13 × 3`, tỉ lệ hơn bốn lần). Để máu là đốm tròn thì nhìn ra vệt sơn đỏ chấm
chấm, không ra máu chảy.

### Ba thứ phải sửa mới ra được kết quả

**1. Vết vỡ tròn đều như lỗ bọt.** Một lớp fBm rồi lấy ngưỡng cho ra những lỗ tròn trơn — đá
mẻ không bao giờ tròn như vậy. Cộng thêm một lớp nhiễu tần số cao biên độ nhỏ **trước** khi
lấy ngưỡng thì đường biên gãy khúc, và ngưỡng hẹp (0,660 .. 0,700) giữ cho mép sắc.

**2. Mặt nạ phải có tỉ lệ RIÊNG, to hơn hẳn hạt đá.** Dùng chung một tỉ lệ thì hạt đá và mảng
rêu buộc phải cùng cỡ: để hạt đá vừa mắt thì mảng rêu chỉ còn hai chục phân, và cả viên đá
nhìn ra một tấm vải hoa lấm tấm. Tách ra (`_TiLe` cho đá, `_TiLeMatNa` cho mặt nạ) thì đá giữ
được chi tiết nhỏ mà rêu, mốc, máu vẫn là những mảng lớn trội hẳn lên.

**3. Cả hàng bia có vết máu ở cùng một chỗ.** Lấy mặt nạ thẳng theo toạ độ thế giới thì hai
tấm bia đứng cạnh nhau — cách một mét, mà mặt nạ lặp mỗi 2,4 m — rơi vào gần đúng một chỗ trên
mảng: nhìn ra một đường viền đỏ kẻ sẵn chứ không ra máu vảy. Gốc vật thể đổi thành một độ lệch
giả ngẫu nhiên nhưng **tất định**, nên mỗi viên đá lấy một chỗ khác nhau:

```hlsl
float3 gocVat = mul(unity_ObjectToWorld, float4(0, 0, 0, 1)).xyz;
float3 lech = frac(sin(gocVat * 12.9898 + gocVat.yzx * 78.233) * 43758.5453) * 41.0;
float3 q = (IN.worldPos + lech) * _TiLeMatNa;
```

### Sứt mẻ thật: cắt bằng mặt phẳng, dùng chung bản vỡ

Vết vỡ trên ảnh chỉ làm bề mặt lõm; nhìn nghiêng thì đường viền tấm bia vẫn thẳng băng tắp.
`Act2VoBia.Chay()` (menu **17**) cắt vào chính cái lưới.

**Cắt bằng mặt phẳng, không đẩy từng đỉnh.** Lưới bia rất thưa — `TS_gothic` chỉ 350 đỉnh,
khoảng cách hai đỉnh kề nhau đã vài centimet. Đẩy từng đỉnh vào trong theo một quả cầu thì vết
lõm ra lồi nhõm như bị bóp. Cắt bằng mặt phẳng thì mỗi đỉnh nằm ngoài được chiếu vuông góc lên
chính nó, nên chỗ cắt thành một mặt **vạt phẳng** — đúng như đá vỡ theo thớ.

**Bản vỡ dùng chung, không phải mỗi bia một lưới.** 458 tấm mà mỗi tấm một lưới riêng thì mất
sạch khả năng gộp lệnh vẽ. Chỉ dựng **3 bản vỡ cho mỗi loại lưới** rồi chia nhau dùng. Chọn
bia nào bị vỡ là tất định — băm từ tên vật thể — nên chạy lại bao nhiêu lần cũng ra đúng một
nghĩa địa.

Mặt phẳng cắt đi qua phần trên khối bao (60..90% chiều cao) và không được ăn quá 22% số đỉnh —
quá thì kéo lui, không thì cả tấm bia biến thành một cái nêm.

> Menu **17b** trả mọi bia về lưới gốc. Cần nó vì menu 17 bỏ qua những lưới đã tên `_voN`, nên
> muốn đổi tỉ lệ sứt mẻ thì phải trả về trước rồi mới cắt lại.

### Số đo

```
Act2_DaBia   458 renderer  ->  Diablo25D/DaMoTriplanar
Act2_DaTang  229 renderer  ->  Diablo25D/DaMoTriplanar
Act2_LongMo    5 renderer  ->  Diablo25D/DaMoTriplanar
Act2_Sat       6 renderer  ->  GIU Standard (hang rao sat, khong phai da)

sut me hinh hoc   119 / 452 bia TS_* (26%), bang 3 ban vo moi loai luoi
vat the Act2      1011 (khong doi)   0 o vat lieu hong
mang dong phang   2 -> 0   (cat luoi lam doi khoi bao, phai chay lai menu 14)
```

## Vỏ cây Act2: sần sùi, nám và tróc từng mảng

Thân và cành cây trước đây gần như **trơn một màu** — nhìn ra một khối tối, không ra vỏ cây.
Nay có vân chạy dọc, khe nứt sâu, đốm nám mốc, và từng mảng vỏ bong ra lộ gỗ sáng bên dưới.

Bộ ảnh sinh bằng `Act2VoCay.Dung()` (menu **15**), 1024×1024, gạch được theo cả hai chiều.

### Vì sao sinh bằng code chứ không vẽ tay

Lưới cây đưa từ Blender **không có toạ độ ảnh** — kiểm cả chín lưới: `uv.Length = 0`. Nên
shader phải chiếu ảnh từ ba hướng rồi trộn lại (triplanar), và ảnh **bắt buộc phải gạch được**
theo cả hai chiều, không được có đường nối. Sinh bằng nhiễu tuần hoàn thì liền mạch sẵn: mỗi
lớp lấy chu kì bằng đúng số ô của nó (`Mod(xi, perX)`), nên mảnh nào cũng khớp mảnh bên cạnh.

### Bốn lớp làm nên vẻ ngoài

```
VAN DOC    nhieu day theo chieu ngang, thua theo chieu doc (26 x 5)
           -> van chay DOC than, dung nhu vo cay that
RANH NUT   nhieu "song nui" - vach sac, khong tron - lam khe nut sau
MANG TROC  nhieu tan so THAP (5 x 4) roi lay nguong -> tung mang vo bong ra
NAM MOC    dom xam xanh, chi bam vao cho vo con nguyen
```

Mép mảng tróc được làm **thẫm lại và gồ lên một vành hẹp**: chỗ vỏ còn dính đang nhểu vào gỗ.
Thiếu cái vành đó thì mảng tróc nhìn như một vệt sơn dán lên, không ra vết bong.

### Bẫy: không có UV thì tangent vô nghĩa

Ảnh gân trước đây **hoàn toàn không có tác dụng**, dù đã gán đúng và nhập đúng kiểu NormalMap.

Không có UV thì tangent Unity sinh ra là một hướng **tuỳ tiện**, không dính gì đến hướng chiếu
của triplanar. Cộng thẳng ba ảnh gân rồi gán vào `o.Normal` — vốn là không gian tangent — thì
chỗ sần sùi bị xoay lung tung theo từng tam giác, và kết quả nhìn ra một thân cây nhẵn thín.

Nay trộn ba ảnh gân trong **không gian thế giới** bằng phép "whiteout", rồi mới đổi ngược về
không gian tangent bằng chính `WorldNormalVector`:

```hlsl
float3 tX = float3(gX.xy + n.zy, abs(gX.z) * n.x);
float3 tY = float3(gY.xy + n.xz, abs(gY.z) * n.y);
float3 tZ = float3(gZ.xy + n.xy, abs(gZ.z) * n.z);
float3 gThegioi = normalize(tX.zyx * w.x + tY.xzy * w.y + tZ.xyz * w.z);

float3 truc0 = WorldNormalVector(IN, float3(1, 0, 0));
float3 truc1 = WorldNormalVector(IN, float3(0, 1, 0));
o.Normal = normalize(float3(dot(gThegioi, truc0), dot(gThegioi, truc1), dot(gThegioi, n)));
```

`_DoSanSui` cũng đổi cách dùng: **nhân vào phần nghiêng** thay vì `lerp` với `(0,0,1)`. Lerp
với hệ số lớn hơn 1 là ngoại suy — pháp tuyến dài ra rồi chuẩn hoá lại, nên để 2,6 hay 1,0
cũng ra gần đúng một kết quả.

### Bẫy đo: mọi thay đổi trong CÙNG một lệnh đều không kịp có tác dụng

Phép kiểm "bật/tắt ảnh gân rồi so hai ảnh" ra **0 điểm khác nhau trên 810 000** — ba lần liền,
và suýt nữa thì tôi kết luận shader hỏng.

Thủ phạm là phép đo. Trong một lệnh MCP, nếu đổi đèn / đổi tham số vật liệu **rồi gọi
`cam.Render()` ngay trong cùng lệnh đó** thì bản dựng vẫn dùng trạng thái cũ. Đo được:

```
                                     do sang trung binh
khong den phu                              46,0
tao den moi 1,9  + Render cung lenh        46,0     <- den khong an
sua den CO SAN   + Render cung lenh        46,0     <- van khong an
sua den o lenh TRUOC, Render o lenh sau    52,3     <- an
```

Tách ra hai lệnh thì phép đo cho kết quả thật:

```
_DoSanSui = 0  so voi  2,8 :  22 516 / 682 000 diem khac (3,30%), lech toi da 142/255
```

> Và một cái bẫy nhỏ hơn nhưng cùng loại: lần đầu tôi để đèn ở `yaw = 150°`, tức chiếu vào
> **mặt sau** thân cây. Mặt đang chụp nằm trong tối, nên đổi gì cũng "không khác gì".

### Tham số

| | |
|---|---|
| `_TiLe` | 1,90 — ảnh lặp mỗi 0,53 m; thấp hơn thì mảng tróc to như vệt loang |
| `_KeoDoc` | 2,30 — kéo vân theo chiều dọc thân |
| `_DoSanSui` | 2,80 |
| `_XanhReu` | 0,20 — hạ từ 0,45: rêu xanh phủ lên cả chỗ gỗ mới tróc |
| `_Color` | (0,88 0,86 0,82) — gần trắng; bản cũ để 0,115 làm dập hết tương phản |

## Vũng nước lơ lửng trên không trung

Người chơi gửi ảnh: một vũng nước ở góc phải bản đồ **bay lên khỏi mặt đất**, thấy rõ cạnh
đĩa. Đo ra thì không phải một, mà **cả 11/11 vũng nước** đều thế — **100% số đỉnh** nằm trên
mặt đất, trung bình cao 0,32 m, cao nhất 0,87 m.

### Lỗi này do chính bản vá trước gây ra

`Act2LongVungNuoc` ra đời để chữa z-fighting giữa mặt nước và mặt đất, bằng cách khoét lòng
xuống dưới mặt nước. Nó khoét cả **đường bờ** xuống thêm 25 cm (`SauMep`) — ghi chú trong
code lúc đó nói rõ lý do: *"để 0,16 thì còn 2/11 vùng dính"*.

Hết z-fighting thật. Nhưng cái giá là **mép mặt nước hở hẳn ra khỏi mặt đất 25 cm**, và cả
vũng nước biến thành một cái đĩa bay giữa nghĩa địa — tệ hơn hẳn lỗi ban đầu.

> **Bài học:** chữa một bề mặt khỏi z-fighting thì phải hỏi **nó sẽ nằm ở đâu sau khi chữa**,
> chứ không chỉ hỏi "hai mặt đã đủ xa nhau chưa". Con số 0 chỗ dính là một con số đẹp, nhưng
> nó không phải là cái người chơi nhìn thấy.

### Cách chữa: đặt mặt nước vào lòng, rồi mới khoét

Hai việc, và **không đổi thứ tự được** — khoét dựa theo cao độ mặt nước, mà cao độ mặt nước
lại dựa theo đất ở đường bờ:

1. **Hạ mặt nước** xuống dưới chỗ thấp nhất của đường bờ 8 cm → mọi điểm trên mép lưới chìm
   vào đất, không còn cạnh nào lộ.
2. **Khoét lòng** bên trong đường bờ, sâu 0,55 m ở tâm, thoải dần về 0 ở `d = 0,80`.

```
  d = 0        tam vung, dat thap hon nuoc 0,55 m
  d -> 0,80    dat nong dan len, vua bang mat nuoc
  d = 0,88..1  DUONG BO - khong dong den, mep luoi chim vao day
```

Chỉ **hạ** đất, không bao giờ nâng — nâng lên thì bia mộ quanh bờ bị chôn.

### Ba cái bẫy khi làm cho nó chạy lại được nhiều lần

Một công cụ dựng cảnh mà chạy hai lần ra hai kết quả thì không dùng được. Cả ba lần đo đều
bằng cách chạy `Khoet()` liên tiếp rồi so cao độ mặt nước.

**1. Hộp bao là hình VUÔNG, mặt nước là hình TRÒN.** Lấy nửa cạnh hộp làm bán kính thì vành
khoét hụt mất một phần mép; lấy nửa đường chéo thì trùm ra quá xa 41%. Phải đo bán kính từ
**đỉnh lưới**, trong **toạ độ thế giới**.

**2. Hai chỗ đo "đường bờ" phải dùng CHUNG một công thức.** Chỗ hạ mặt nước đo theo toạ độ
lưới, chỗ khoét đo theo toạ độ thế giới — hai cái lệch nhau vài phần trăm là đủ để vành khoét
ăn vào đúng những đỉnh đang được dùng làm mốc.

```
chay lan hai, DamLay_7 tut them:  0,5397 m
```

**3. `SampleHeight` nội suy, nên hai vành phải cách nhau một khoảng đệm.** Sau khi sửa lỗi 2
thì vẫn còn tụt 4,4 cm mỗi lần. Lưới độ cao là 513 điểm trên 109 m — **21 cm một ô** — và
`SampleHeight` nội suy từ bốn ô quanh điểm hỏi. Hỏi ở `d = 0,86` mà khoét tới `0,85`, trên
bán kính 7 m thì hai chỗ chỉ cách nhau **7 cm**, chưa bằng một phần ba ô lưới: cái hỏi **rò
rỉ** sang cái khoét.

Tách ra thành `MepLong = 0,80` (khoét đến đây) và `BoDo = 0,88` (đo từ đây) — trên bán kính
7 m là cách nhau 56 cm, hơn hai ô lưới.

```
                         lech giua hai lan chay
ban dau                        0,5397 m
chung cong thuc "d"            0,0440 m
them vanh dem                  0,0000 m   <- on dinh han
```

### Lần thứ hai: phép đo của tôi che mất lỗi

Sau bản trên, phép đo báo **0 đỉnh mép lộ** ở cả 11 vũng. Người chơi vẫn gửi ảnh: một vũng ở
góc phải bản đồ còn bay. Đo cắt ngang `DamLay_10` theo tám hướng thì ra ngay:

```
huong    d=0,9    1,0    1,2    1,5    2,0     (+ = dat cao hon mat nuoc)
  45°    -0,43  -0,44  -0,44  -0,40  -0,40
  90°    -0,52  -0,52  -0,56  -0,72  -0,96     <- mep nuoc treo nua met
 135°    -0,61  -0,64  -0,73  -0,90  -1,06
 270°    +1,28  +1,45  +1,73  +2,05  +2,21     <- phia kia dat cao hon 1,3 m
```

Địa hình quanh vũng này **dốc 2,69 m** — gấp bốn lần mọi vũng khác.

Nhưng lỗi thật nằm ở **phép đo**. Lưới đầm lầy là hình bất quy tắc, không phải hình tròn:
cùng một vũng mà hướng này dài 7 m, hướng kia chỉ 3,5 m. Cả `d = khoảng cách / bán kính` lẫn
bộ lọc "chỉ xét đỉnh ở `d ≥ 0,88`" đều lấy **một** bán kính chung — cái lớn nhất. Ở những
hướng lưới ngắn, mép thật rơi vào `d ≈ 0,5`: nằm gọn trong vùng khoét, và **không bao giờ
được đếm là mép**.

Nay bán kính đo **riêng cho từng hướng**: chia 72 cung, mỗi cung lấy đỉnh xa tâm nhất, cung
rỗng mượn của cung gần nhất, rồi làm tròn bằng trung bình ba cung liền nhau (không làm tròn
thì `d` nhảy bậc và vành khoét có răng cưa).

> **Và phép kiểm phải độc lập với thứ đang kiểm.** Đo bằng `d` là dùng lại đúng cái giả thiết
> đang sai. Phép đo đúng không cần bán kính nào cả: tìm **cạnh biên thật** của lưới — cạnh chỉ
> thuộc **một** tam giác — rồi hỏi từng đỉnh biên có nằm dưới mặt đất không.
>
> Cũng đừng đếm "đỉnh nào nổi trên mặt đất": ~465/721 đỉnh của mọi vũng đều nổi, và đó là
> **đúng** — mặt nước phải nổi trên đáy chảo. Chỉ có đỉnh **biên** mới bắt buộc phải chìm.

```
                            phep do cu (d >= 0,88)   phep do dung (canh bien)
DamLay_10 truoc khi sua              0 dinh lo              nhieu dinh lo
sau khi sua                          0                      0 / 880 dinh bien
```

### Và bộ chống đồng phẳng không được đụng vào mặt nước

Chạy `Act2ChongDongPhang` sau khi đã đặt nước xong thì **258 đỉnh mép lại lộ ra**. Mặt nước
dày đúng 0 m, nên hàm tính dịch không còn đường "dìm xuống" (dìm tối đa là 45% của 0) — chỉ
còn đường **nâng**, mà nâng thì lấy theo chỗ đất **cao nhất** trong hộp bao, tức là lên trên
cả mép bờ. Nó dựng lại chính xác cái lỗi vừa chữa xong.

Nay `BoQua` loại mọi renderer có `bounds.size.y < 0.02` — chúng có cơ chế riêng.

### Thứ tự chạy cho Act2

```
1. Act2GoGhe.VeLai(false)        dia hinh sach, khong dich do dac
2. Act2LongVungNuoc.Khoet()      dat nuoc vao long roi khoet
3. Act2ChongDongPhang.Sua()      tach cac mat con lai khoi dat
```

```
KET QUA  (do bang CANH BIEN cua luoi, khong dung ban kinh chuan hoa)
   mep nuoc con lo        0 / 880 dinh bien
   vung nong nhat         DamLay_2, sau 0,16 m
   mang dong phang        0
   vat the Act2           1011 (khong doi)   0 o vat lieu hong
   chay lai lan hai       lech 0,0000 m
```

## Ô vuông đen dưới mặt đất: z-fighting, và ba lần đi sai đường

Người chơi thấy **hình vuông tối, hơi trong suốt, nằm la liệt dưới mặt đất** — trên điện
thoại thì nhiều, trên máy bàn thì không.

### Vì sao là z-fighting, dù trên máy bàn không tài nào chụp lại được

Không dựng lại được hiện tượng bằng mắt trên máy bàn. Nhưng nó **tính ra được**. Độ chính
xác của bộ đệm chiều sâu phối cảnh giảm theo **bình phương** khoảng cách:

```
    khe nho nhat phan biet duoc  ~  z² / (near × 2^bit)
```

Với `near = 0,5` và vật cách máy quay 15 m:

```
    16 bit (WebGL dien thoai)  ->  6,87 mm
    24 bit (may ban)           ->  0,027 mm
```

**Chênh nhau 256 lần.** Đó chính là "điện thoại bị mà máy bàn không". Đo trong cảnh Act2:
chỗ sát nhất — đế một tấm bia — cách mặt đất **1,3 mm**.

> Thử ép Unity render vào một `RenderTexture` khai báo depth 16 bit để dựng lại hiện tượng:
> **không được**, driver D3D11 vẫn cấp cho 24 bit (`rt.depth` đọc ra 32). Hai ảnh 16/24 bit
> khác nhau **0 điểm ảnh trên 450 000**. Không phải cái gì cũng chụp lại được — có lúc phép
> tính là bằng chứng chắc hơn con mắt.

### Sai lầm 1: đếm "có điểm nào gần đất không"

Phép đếm đầu tiên ra **247 vật** và tôi tưởng đó là quy mô của lỗi. Con số đó vô nghĩa.

Một phiến đá nằm **vắt qua** một gò đất thì nó **cắt** mặt đất theo một đường, và ở chỗ cắt
khoảng cách đúng bằng 0. Nhưng giao cắt **không gây z-fighting**: hai mặt cắt nhau một góc
rõ ràng thì bộ đệm chiều sâu phân biệt được thoải mái.

z-fighting chỉ xảy ra khi hai mặt **gần song song** và gần nhau **trên một mảng rộng**. Nên
phép đếm đúng là: quét lưới 9×9, và chỉ tính khi có **từ 3 điểm trở lên** cùng nằm trong khe
nguy hiểm.

```
                          nguong cu (1 diem, 2 cm)      nguong dung (3 diem, 7 mm)
Act2                              247 vat                        66 vat
```

### Sai lầm 2: nâng mặt đất lên ôm lấy chân vật

Dịch riêng vật chỉ chữa được một nửa (247 → 102) rồi đứng im, nên tôi thêm một bước "nâng
địa hình trong vùng ngay dưới vật cho ngập hẳn mặt đáy". Sai ở hai chỗ:

1. **Nó chỉ nâng, không bao giờ hạ** (`Mathf.Max`, để hai vật cạnh nhau không đào hố dưới
   chân nhau). Chạy bao nhiêu vòng thì đất leo lên bấy nhiêu — không hội tụ: 102 → 34 → 32
   rồi dừng ở đó mãi.
2. Cái giá phải trả đo được: **31 vật bị chôn HẲN** dưới đất, 70 cái chôn quá 70%, 92 cái
   quá nửa. Đổi một lỗi nhấp nháy lấy một lỗi mất hẳn đồ đạc thì không phải là chữa.

Đã bỏ hẳn bước đó. Địa hình dựng lại nguyên trạng bằng `Act2GoGhe.VeLai(false)` +
`Act2LongVungNuoc.Khoet()`.

> ⚠️ Tham số `false` đó là cả vấn đề. `VeLai` bình thường **dịch đồ đạc theo mặt đất mới** —
> đúng khi địa hình đổi thật. Nhưng khi đang **hoàn nguyên** thì "đất cũ" chính là cái đất đã
> bị làm sai, nên dịch theo nó là **chép lại cái sai**: vật đang chôn nửa người thì sau khi
> dịch vẫn chôn nửa người, chỉ khác là cả hai cùng tụt xuống. Bỏ bước đó thì đất hạ về đúng
> chỗ cũ còn đồ đạc đứng yên — đúng là thứ cần.

### Sai lầm 3: dịch một bộ phận thay vì dịch cả đồ vật

Công cụ dịch thẳng `r.transform` của từng `MeshRenderer`. Một bụi cỏ là **hai** renderer rời:
`MatCo` nằm bẹt dưới đất và `MatHoa` nhô lên trên. Chỉ `MatCo` bị bắt là đồng phẳng, nên nó
bị dịch **một mình** — mặt cỏ trượt ra khỏi bụi cỏ của chính nó, lệch tới 13 cm.

Nhận ra được là nhờ `MatHoa` — bộ phận kia của **chính những bụi cỏ đó** — vẫn còn nguyên
`localPosition.y = 0` hết. Cùng tên mà cái có cái không thì cái "có" là cái bị dịch.

```
BuiCo3_Co_MatCo    n=19   so cai khac 0 = 12   lech xa nhat = -0,1194 m
BuiCo3_Hoa_MatHoa  n=19   so cai khac 0 =  0
```

Cái bẫy này còn to hơn ở Act1, nơi thân cây và tán lá là hai vật thể riêng tên `Trunk` và
`Leaves` — dịch riêng một cái là xé đôi cái cây.

Nay dịch **gốc cụm**. Phân biệt cụm với thùng chứa bằng số con:

| | ví dụ | số con | dịch cái gì |
|---|---|---|---|
| thùng chứa | `BiaMo` ở Act2 | 451 | dịch **từng con** — mỗi con là một ngôi mộ riêng |
| cụm bộ phận | `BuiCo_34` | 2 | dịch **cả cụm** — hai con là hai nửa của một bụi cỏ |

Mốc: **từ 5 con trở lên thì là thùng chứa**. Chưa gặp đồ vật nào ghép từ quá bốn mảnh, còn
thùng thì ít nhất cũng vài chục.

`TraLaiCumBiTach()` trả 45 mặt cỏ đã bị xé về đúng chỗ trước khi chạy lại.

### Cách chữa: hai đòn bẩy

**1. Nâng `nearClipPlane` từ 0,5 lên 1,5.** Rẻ nhất, không động đến một mô hình nào, và ăn
cả ba màn cùng lúc:

```
    near 0,15  ->  22,9 mm      (ban rat cu)
    near 0,5   ->   6,9 mm
    near 1,5   ->   2,3 mm      <- dang dung
```

Cắt gì không: máy quay gần nhất là 4,1 m (zoom hết cỡ ở góc nhìn 3D tự do), và chỗ gần nhất
của nhân vật lúc đó vẫn còn cách 3,5 m.

**2. Dịch vật cho khe ≥ 25 mm.** Không lấy 7 mm: 7 mm chỉ đủ cho vật ở 15 m; ở 30 m cần
9,2 mm, ở 40 m cần 16,3 mm. Lấy 25 mm thì phủ hết vùng nhìn thấy, mà dịch 2,5 cm thì mắt
không nhận ra.

Mỗi vật chọn đường **ngắn hơn** trong hai đường — nâng hẳn lên trên đất, hoặc dìm hẳn xuống
dưới — với trần "không nâng quá 15 cm" (quá thì hở chân) và "không dìm quá 45% chiều cao"
(quá thì mất vật).

### Số đo

```
                        truoc      sau
Act2  mang dong phang     66         0     (mot vong la xong, khong lap)
Act1  mang dong phang    179         0
Act2  vat the           1011      1011     anh sang / suong mu khop tung con so
Act1  vat the           4436      4436     0 o vat lieu hong
Act2  vat bi chon HAN     25        19     (giam - khong tao them cai nao)
bo phan bi xe             45         0
```

## Cây Act2: đi xuyên qua được, và lốc không cuốn nổi

Hai lỗi người chơi thấy hoá ra **cùng một gốc**: ống trụ va chạm của cả 58 cây đặt sai chỗ.

```
TREE_oakA_704   HINH  tam y=8,79   co (22,05 / 14,95 / 23,14)   <- cay cao 15 m
                CHAN  tam y=8,79   co ( 8,05 /  3,05 / 17,88)   <- vien nang NAM NGANG
```

Viên nang chỉ cao 3 m mà **dài 17,9 m theo trục Z**, và tâm nó ở **y = 8,79** — tức nằm
ngang lơ lửng ngang tán cây. Người chơi đi dưới gốc không đụng phải gì; con lốc quét ở tầm
1,2 m cũng không thấy gì mà bốc.

### Bẫy thứ nhất: đoán thân cây dọc trục Y

```csharp
c.direction = 1;                 // doc theo truc Y
c.height = bb.size.y;
```

Blender dựng trục Z hướng lên, và sau khi qua FBX thì vật thể bị **xoay −90° quanh X** —
thân cây vẫn nằm dọc **Z của lưới**. Đặt cứng `direction = 1` thì được một viên nang nằm
ngang. Đúng cái bẫy đã vấp một lần ở Act3.

### Bẫy thứ hai: "trục dài nhất" cũng sai

Bản sửa đầu của tôi dò **trục dài nhất của bounds**. Ở Act3 cách đó đúng vì cây gầy; ở Act2
thì sai — cây sồi có **tán rộng hơn thân cao**:

```
TREE_oakC bounds cuc bo = 0,122 x 0,077 x 0,086
                          ^^^^^ dai nhat la X, ma X chinh la BE RONG TAN
```

Kết quả: 50 trong 58 cây vẫn đặt ống trụ nằm ngang. Chỉ **8/58** chặn được tia.

Quy tắc đúng không nằm ở hình học mà ở thực tế: **cây mọc thẳng lên trời**. Hỏi "trục cục bộ
nào đang hướng lên" thay vì "trục nào dài nhất":

```csharp
float dX = Mathf.Abs(Vector3.Dot(t.right,   Vector3.up));
float dY = Mathf.Abs(Vector3.Dot(t.up,      Vector3.up));
float dZ = Mathf.Abs(Vector3.Dot(t.forward, Vector3.up));
```

### Bẫy thứ ba: một cành rủ làm ống trụ phình lên 6 m

Bán kính đo từ các đỉnh ở **15% dưới cùng** của thân, nhưng lấy **đỉnh xa nhất** thì ba cây
`TREE_oakC` ra bán kính **5,4 – 6,3 m** — vì vài đỉnh lẻ nằm tận ngoài rìa (một cành rủ
xuống, một mảng cỏ mọc ở gốc). Người chơi sẽ đâm vào một bức tường vô hình cách gốc cây sáu
bước chân.

Lấy **phân vị 90%** thay cho đỉnh xa nhất, cộng thêm trần "thân cây không bao giờ dày bằng
18% chiều cao của nó".

```
                  dinh xa nhat    phan vi 90% + tran
ban kinh ong tru   0,27 - 6,33 m    0,27 - 2,01 m
```

### Số đo sau khi sửa

```
BAN TIA NGANG qua tam than, o do cao 1,1 m tren chan cay
   truoc khi sua        8/58 chan duoc
   doan truc dai nhat   8/58 chan duoc   (van sai)
   doan truc huong len 50/58 chan duoc, 0 TRUOT HAN
   (8 cai con lai: tia trung mot bia mo hay tang da dung gan hon, khong phai truot)

DAY NHAN VAT 8 m thang vao goc cay TREE_dead_738 (ban kinh ong tru 0,54 m)
   bat dau cach tam than 5,00 m  ->  dung lai cach 0,94 m
   BI CHAN, khong xuyen qua

LOC XOAY tha vao nghia dia
   dang cuon 1 CAY (TREE_oakB_bare_732) va 13 bia mo / tang da
   cay ke ben cach 8,1 m: be ngang va cham 1,3 m, cuon duoc = True
```

### Lốc đo bề ngang theo VA CHẠM, không theo hình vẽ

`VatTheBiCuon.CuonDuoc` loại vật rộng hơn 6 m. Đo theo bounds của **hình** thì mọi cây Act2
đều bị loại — một cây sồi rộng **22 m** theo tán lá trong khi thân chỉ hơn một mét. Con lốc
đi qua cả khu rừng mà không bốc nổi cái gì.

Nay đo theo **collider** — cái mà con lốc thực sự chạm tới. Sau khi sửa: **58/58 cây** đủ
nhỏ để cuốn, còn vách đá và nhà mồ vẫn bị loại vì `MeshCollider` của chúng to thật.

> Sửa ở `Act2Baker.OngTruOmThan` nên **lần dựng lại Act2 sau này cũng đúng**, và vá luôn 58
> cây trong cảnh đang có bằng chính hàm đó — một nguồn sự thật duy nhất, không có bản chép tay
> thứ hai để lệch nhau.

## Thiên thạch đốt cháy cả cái cây

Thiên thạch rơi trúng gốc cây thì cây bắt lửa ở gốc, rồi **lửa lan dần ra khắp cây** — thân,
cành lớn, cành nhỏ, chùm lá — cho tới khi không còn bộ phận nào chưa cháy. Khói đen cuộn lên,
từng mảng cây cháy rớt xuống đám lửa dưới gốc, thân cây đen dần theo đúng bước lửa lan. **Lan
hết rồi ngọn lửa mới tắt dần**, cây biến mất, và **30 giây sau nó mọc lại ở đúng chỗ cũ**.

Kẻ địch đứng dưới gốc cây đang cháy ăn **hai** nguồn sát thương cùng lúc: lửa của cái cây và
vùng lửa mà thiên thạch để lại. Hai vùng đốt hoàn toàn độc lập nên chúng tự cộng vào nhau —
không cần một dòng nào để làm chuyện đó.

### Lửa bám bề mặt thật, không phải một khối hình học quanh thân

Bản dựng đầu tiên rải lửa bằng một cái **hộp** quanh thân cộng một **quả cầu** ở tán. Kết quả
đúng như người dùng nói: *lửa chỉ hiện ra một phần trên cây*. Tán cây Act2 toè rộng tới
**23,6 m** mà quả cầu bị chặn ở 5,5 m — phần lớn cành nằm ngoài lửa, và cái nhìn thấy được là
"có đám lửa ở chỗ cái cây" chứ không phải "cái cây đang cháy".

Nay mỗi cây có một bộ **điểm mồi lửa** rải đều trên chính bề mặt lưới của nó. Hai lớp
`LuaCanh` và `KhoiCanh` **không tự phun** (`rateOverTime = 0`); `CayChay` tự bắn hạt vào đúng
những điểm ấy:

```
nguongLan = xaNhat * (age / thoiGianLan)      lua an ra bao xa roi
diem nao co xa[i] <= nguongLan  ->  dang chay
```

Mảng khoảng cách được **sắp tăng dần một lần** lúc bắt lửa, nên mỗi khung hình chỉ cần một
phép tìm cung trái — không phải duyệt ba trăm điểm.

Một phần ba số hạt dành cho **rìa lửa** (vành ngoài của vùng đã cháy). Đó là chỗ lửa đang ăn
tới, phải sáng hơn và đông hơn phần phía trong, không thì không nhìn ra lửa đang lan mà chỉ
thấy nó đột nhiên to ra.

### Điểm mồi lửa phải nướng sẵn — 46/58 cây không đọc được lưới lúc chạy

Muốn lửa bám theo từng cành thì phải biết bề mặt cây nằm ở đâu, tức phải đọc `mesh.vertices`.
Nhưng **46 trong 58 cây Act2 nằm trong `map_luoi.fbx` với Read/Write TẮT** — đọc lúc chạy sẽ
ném lỗi. Trong Editor thì đọc được, nên **lỗi này chỉ lộ ra ở bản build**.

Bật Read/Write cho file đó thì Unity giữ một bản sao CPU của **cả tấm bản đồ** (12,7 MB), mà
cây chỉ là một phần nhỏ trong đó. Trên điện thoại đó là cái giá rất đắt cho một hiệu ứng.

`Act2DiemLua` (menu **20**) nướng sẵn: rải 340 điểm trên mỗi loại lưới rồi lưu thành
`Assets/Resources/DiemLua/<tên lưới>.asset`. Chín loại lưới cây Act2 gộp lại chưa tới một phần
nghìn số đỉnh thật.

> **Rải theo diện tích tam giác, không lấy đỉnh cách quãng.** Lấy đỉnh cách quãng thì điểm dồn
> về chỗ lưới chia dày — chỗ nối cành, những mấu u nần — còn một khúc cành thẳng dài hai mét
> chỉ có vài chục đỉnh thì gần như không được điểm nào. Lửa sẽ rõ từng cụm ở chỗ nối cành và
> bỏ trống cả đoạn giữa.

Cây Act1 thì không cần asset: lưới của nó dựng bằng code lúc chạy nên đọc thẳng được. Nhưng
ngân sách điểm phải chia theo **số đỉnh của từng mảnh**, không chia đều: cây Act1 có 88 khúc
cành nhỏ và **duy nhất một** mảnh `Leaves` chứa cả tán lá. Chia đều thì cả tán lá chỉ được 3
chỗ bắt lửa — đo lần đầu ra đúng như vậy, tán lá gần như không cháy.

### Hạt nhỏ mà nhiều, không to mà thưa

Lần dựng đầu, hạt lửa trên tán to 1,5 .. 4,0 m: trên ảnh nhìn ra từng cánh lửa rời rạc trôi
lơ lửng, thấy rõ từng miếng ảnh. Kéo cỡ hạt xuống theo chiều cao cây (0,5 .. 1,4 m cho cây 9 m)
và tăng lượng phun thì chúng chồng lên nhau thành một mảng lửa liền. Đúng cái bẫy đã gặp ở
vùng lửa của thiên thạch.

### Ba cái trần, và cái bẫy `Start`

Mỗi cây cháy hết cỡ là hơn một nghìn hạt mỗi giây cộng một ngọn đèn điểm. Ba thứ chặn lại:

| Trần | Giá trị | Vì sao |
|---|---|---|
| `ToiDaCungLuc` | 4 cây | sáu cái cùng lúc làm khung hình tụt khoảng một nửa |
| `NganSachCay` | 2,5 cây | lượng hạt của **mỗi** cây bị chia lại khi nhiều cây cùng cháy, nên tổng gần như không đổi |
| `DenToiDa` | 2 đèn | đèn điểm thời gian thực tầm 14 m giữa nghĩa địa dày đặc bia mộ — đường vẽ tiến vẽ **lại** mọi vật trong tầm đèn một lần nữa cho mỗi ngọn |

> **Đếm ngay lúc gắn component, đừng đợi `Start`.** `Start` chỉ chạy ở khung hình **sau**. Mà
> một quả thiên thạch đốt cả chùm cây trong **cùng một khung hình** — nên lúc mỗi cái kiểm
> trần, biến đếm vẫn bằng 0 cho cả chùm. Đo thật: châm lửa 12 cây với trần 6 thì **cả 12** đều
> bắt lửa, và khung hình tụt 13,6 → 7,3 fps.

```
DO TRONG PLAY (menu 18c) - cham lua vao 12 cay mot luc
   truoc khi sua bay dem:  12 cai bat lua   7 154 hat
   tran 6, chia ngan sach:  6 cai bat lua   3 726 hat
   tran 4, gioi han den:    4 cai bat lua   2 558 hat
```

> Con số fps trong Editor **không đáng tin để so hai lần chạy khác nhau**: nền đo được nhảy từ
> 15,0 xuống 6,7 fps giữa các lần dù cảnh y hệt (Scene view, nạp asset, việc nền của Editor).
> Số **hạt** thì ổn định, nên đó mới là thứ dùng để so. Trong một lần chạy, tỉ lệ nền → lúc
> cháy vẫn đọc được: khoảng 17 % với trần 4.

### Thiên thạch của Quỷ dữ thì không đốt cây

Việc đốt cây nằm **trong đúng cái nhánh `if`** đã bọc vùng lửa, chứ không phải một công tắc
riêng. Quỷ dữ đã tắt vùng lửa đi rồi (mười con cùng ném thì cả sân thành biển lửa và người
chơi không còn chỗ đặt chân) — nếu nó vẫn đốt được cây thì cả khu rừng cháy sạch trong một
đợt bắn.

### Cây đang cháy thì lốc không cuốn

Hai cơ chế cùng muốn ẩn vật thể đi rồi trả lại sau một khoảng thời gian, mà **mỗi cái nhớ một
chỗ cũ và một cái đồng hồ riêng**. Để cả hai cùng chạy trên một cái cây thì cái nào hết giờ
sau sẽ bật lại một cái cây mà cái kia đã tắt, hoặc ngược lại. `VatTheBiCuon.CuonDuoc` loại
thẳng cây đang cháy, và `CayChay.Dot` loại thẳng cây đang bị cuốn.

### Cháy đen bằng MaterialPropertyBlock, theo đúng bước lửa lan

Cả 58 cây Act2 dùng **chung** một vật liệu `Act2_VoCay_SanSui`. Tô đen vật liệu đó là cả rừng
đen theo. Còn tạo bản sao vật liệu cho từng cây thì mỗi lần cháy lại sinh thêm một đống vật
liệu rác. `MaterialPropertyBlock` đổi màu cho **riêng một renderer** mà không đụng tới vật liệu
gốc, và gỡ ra chỉ bằng `SetPropertyBlock(null)` — đúng cái cần khi cây mọc lại.

Cây nhiều bộ phận (Act1: 88 renderer) thì **mỗi bộ phận đen theo lúc lửa lan tới nó**, đen hết
trong khoảng 1,2 m kể từ khi bắt lửa — nhìn ra từng cành đen lại lần lượt. Cây một bộ phận
(Act2) thì cả cây đen theo tiến độ chung.

> Đổi màu theo **nhịp 0,12 giây**, không phải mỗi khung hình. Sáu cây Act1 cùng cháy là 528
> lần `SetPropertyBlock` mỗi khung. Mà thân cây đen đi trong mấy giây — mắt không phân biệt nổi
> bước nhảy 0,12 giây với đổi màu liên tục.

### Số đo

```
DO TRONG PLAY (menu 18 / 18b)

                     Act2 TREE_oakC_bare_724     Act1 CayXanh_16
bao cay              8,9 x 9,1 x 13,4 m          3,2 x 6,1 x 4,2 m
so renderer          1                           91
diem moi lua         340 (asset nuong san)       353 (doc thang luoi)
bat lua sau          0,70 s ke tu luc tha        0,80 s
lan het ca cay       3,7 s  (du tinh 3,7 s)      2,5 s  (du tinh 2,5 s)
   luc do            340/340 diem dang chay      353/353 diem dang chay
   cay VAN CON       renderer 1/1                renderer 91/91
   hat lua bam canh  582                         537
than cay             con 60 % do sang            con 35 %
quai duoi goc        70 -> 0 mau (chet)          70 -> 0
cay bien mat sau     7,5 s                       6,3 s
sau 20,9 / 19,7 s    van an (chua moc lai)       van an
moc lai sau          37,4 s, mau da tra ve       36,3 s, mau da tra ve
so loi               0                           0
```

Và một lần nữa qua **đúng đường người chơi đi** — bấm phép chứ không gọi thẳng
`ThienThach.Spawn` (menu 18d): cây bắt lửa sau 1,13 s (lâu hơn vì còn động tác niệm chú), lan
hết 340/340 điểm sau 3,7 s, rụi ở 7,5 s, mọc lại ở 37,5 s. 0 lỗi.

Phải chạy cả đường này: gọi thẳng `Spawn` là kiểm `ThienThach.No`, còn cái người chơi thực sự
làm là `CastAt(4)` → `SpawnLoat` → ba quả → `No`, với **mặt nạ và điểm ngắm của chính người
chơi**. Một khác biệt ở mặt nạ thôi cũng đủ làm cả tính năng không bao giờ chạy trong lúc chơi
thật.

> Trong kịch bản đó, người chơi phải đứng **xa** và được bơm máu. Lần đầu để người chơi cách
> 6 m: quả đá rơi ngay cạnh, người chơi ăn vùng lửa rồi chết, `GameDirector` nạp lại cảnh — và
> kịch bản mất sạch mọi tham chiếu giữa chừng. Phép đo phải không dính gì tới cái nó đo.

### Cây nào là cây: bốn kiểu tên

```
Act2   TREE_oakA_bare_701, TREE_dead_733, TREE_thin_742   (dua tu Blender)
Act1   CayXanh_19, CayChet_11                             (ten trong CANH da bake)
Act1   Tree7, DeadTree12                                  (ten khi dung bang CODE)
```

Hai dòng Act1 là **cùng một thứ ở hai thời điểm khác nhau**: `WorldFactory.BuildLeafyTree`
đặt tên `Tree7`, còn `AssetBaker` khi nướng cảnh vào `Act1.unity` thì đổi thành `CayXanh_19`.
Lần đầu chỉ nhớ tên trong code, kịch bản thử báo *"khong tim thay cay nao hop le"* trong khi
màn có 22 cái cây đứng sờ sờ.

Phần đuôi bắt buộc phải là **số**, không thì một vật tên `Treasure` hay `TreeStump` cũng lọt
vào danh sách cây.

### Trúng cây nghĩa là trúng cái thân

Quét bằng `Physics.OverlapSphere` ở lớp `Default`. Collider của cây là một **bao nang ôm lấy
thân** (bán kính 0,33 .. 0,74 m), nên "trúng cây" đúng nghĩa là quả đá nổ gần **cái thân** —
chứ không phải chạm vào mép tán ngoài 11 m.

Từ chỗ trúng còn phải **leo lên gốc cụm**: cây Act1 dựng thành 88 mảnh con (`Trunk`,
`Branch0`, `Twig0_1`, `Leaves`...), tia quét trúng mảnh nào cũng được, mà đốt một mảnh thì
chỉ cháy đúng cái cành đó.

### Đo trong Play phải dùng đồng hồ CỦA GAME

Lần đo đầu dùng `Time.realtimeSinceStartup` và báo *"cay khong bat lua"* — trong khi nó có
bắt lửa thật. Lúc Act2 vừa vào Play, mấy khung hình đầu nặng tới mức `Time.deltaTime` bị kẹp
ở trần 0,333 s: đồng hồ thật đã chạy 4 giây mà trong game mới trôi chưa được một giây, quả
thiên thạch còn đang lơ lửng trên không.

Và mốc đo không đếm từ lúc thả quả đá mà đếm từ lúc **cây bắt lửa**: quả đá rơi bao lâu là
chuyện của quả đá, còn "lan hết rồi mới tắt dần" là chuyện của cái cây.

## Lốc xoáy — phình to, cuốn cả cảnh vật, và xác thì phải rơi

### Bán kính rộng thêm 20% — nhưng chiều cao giữ nguyên

Cả thân lốc nở ngang **1,20 lần**, từ chân lên tới đỉnh. Hệ số nằm ở một chỗ duy nhất:
`VfxFactory.LocNoNgang`.

**Nhân riêng vào bán kính, không nhân vào `scale`.** `FunnelProfile` dùng `scale` cho *cả
hai* trục — nhân vào đó thì con lốc cao thêm 20% nữa, và nó đâm thủng đám mây ở đỉnh.

Bảy chỗ phải nở cùng một hệ số, thiếu một là phần đó teo lại giữa thân lốc:

```
FunnelProfile.x        ba lop vo + ba dai xoan quan quanh
Dust.shape.radius      bui cuon o chan
Vut.shape.radius       tia bui vut tu chan len ngon
BuildDebrisSwarm       dam manh vun bay quanh
Cloud.shape.radius     may den o dinh
Tornado.FunnelRadiusAt quy dao cua thu bi cuon
Tornado.catchRadius    vung hut
```

Hai cái cuối là **logic**, không phải hình vẽ — bỏ sót thì quái bay theo một đường nằm lọt
hẳn trong vỏ, nhìn như nó xuyên qua thân lốc; còn vùng hút hẹp hơn cái vỏ nhìn thấy thì quái
đứng ngay trong lòng lốc mà không bị cuốn.

```
                       truoc      sau
be ngang vo ngoai    15,13 m    18,15 m
catchRadius           3,60 m     4,32 m
FunnelRadiusAt(0)     0,90 m     1,08 m
FunnelRadiusAt(12,6)  4,40 m     5,28 m
chieu cao            GIU NGUYEN
```

> ⚠️ **Prefab có sẵn hình dựng sẵn — phải nướng lại.** `Skill_LocXoay.prefab` chứa 12 vật thể
> con, và `Tornado.Start` chỉ dựng hình bằng code khi prefab **không có con nào**. Sửa
> `BuildTornado` mà không nướng lại thì trong game vẫn là con lốc cũ, y nguyên kích thước cũ.
> `catchRadius` cũng vậy: prefab lưu 3,6 và **đè lên** giá trị mặc định trong code.
> Đã thêm menu **"13. Nuong rieng prefab Loc xoay"** cho lần sau.

### Xác chết giữa không trung thì phải rơi xuống

`Damageable.Die` tắt `CharacterController` và `Collider` của con quái. Nên khi nó chết lúc
đang bị cuốn lên cao, **không còn gì kéo nó xuống**: cái xác treo nguyên giữa trời cho tới
lúc hết `corpseSeconds` rồi biến mất tại chỗ.

`RoiXuongDat` gắn vào lúc `WhirledEffect` nhả ra một cái xác còn ở trên cao hơn 0,6 m.

**Không dùng Rigidbody.** Cái xác không còn collider nào để va chạm, mà thêm `Rigidbody` vào
một vật vừa chết thì nó lọt thẳng qua địa hình. Đây chỉ là rơi tự do rất đơn giản, dừng lại
khi chạm cao độ mặt đất, rồi **tự huỷ chính nó**.

Cao độ mặt đất đo **một lần** rồi nhớ lại: cái xác rơi thẳng đứng, đất dưới chân nó không
đổi — bắn tia mỗi khung vừa tốn vừa có thể trúng một cái xác khác đang rơi bên cạnh.

```
DO TRONG PLAY
   truoc khi giet:  y = 5,14   mat dat = -0,16   cach dat 5,30 m
   sau khi giet:    y = -0,16  mat dat = -0,16   cach dat 0,00 m
   goc nghieng cua xac = 78 do  (nam sap, khong dung thang)
   RoiXuongDat da tu huy sau khi cham dat
```

### Lốc cuốn luôn cây, đá, bia mộ

Con lốc đi qua thì bốc cả cảnh vật lên. Vật thể bay quanh thân lốc y như kẻ địch bị cuốn,
rồi **tan biến cùng con lốc** — không rơi xuống — và **mọc lại ở đúng chỗ cũ sau 30 giây**.

#### Vì sao không xoá hẳn rồi tạo lại

Hai lý do, cái thứ hai mới là cái khó:

1. Cảnh vật được dựng khi vào màn, **không có prefab để tạo lại**. Xoá là mất vĩnh viễn.
2. Cái đồng hồ 30 giây phải chạy ở đâu đó. `SetActive(false)` thì `Update` của chính nó
   ngừng chạy và **không bao giờ có ai đánh thức nó dậy**.

Nên "biến mất" ở đây là **tắt `Renderer` và `Collider`**; GameObject vẫn sống và vẫn đếm giờ.

> Và ngay lúc tan biến, vật thể được **đưa về toạ độ cũ luôn**, không đợi tới lúc mọc lại.
> Để nó nằm trên trời suốt 30 giây thì mọi truy vấn vật lý, mọi phép tính khoảng cách đều
> lấy phải cái toạ độ lơ lửng đó.

#### Cuốn cái gì, và không cuốn cái gì

Quét ở **lớp `Default`** — toàn bộ cảnh vật của màn nằm ở đó, còn địa hình ở lớp `Ground`
riêng nên không bao giờ bị hút theo. Danh sách loại trừ nằm trong `VatTheBiCuon.CuonDuoc`:

| Bỏ qua | Vì sao |
|---|---|
| có `Damageable` | quái và người chơi đã có `WhirledEffect` lo; hai cơ chế cùng đặt vị trí thì nó giật lia lịa giữa hai quỹ đạo |
| là `Terrain` | cuốn cả quả đồi lên trời |
| là một phần của lốc | tự cuốn chính mình |
| có `ParticleSystem` | lửa, khói, vòng phép — vừa vô lý vừa hỏng vòng đời của chúng |
| không có `Renderer` | cuốn cũng chẳng ai thấy |
| bề **ngang** > 6 m | vách đá bao quanh đấu trường, nền nhà |

Đo theo bề **ngang** chứ không theo chiều cao: cây cao 8 m vẫn cuốn được — mà cuốn được cây
mới đúng là một con lốc.

Trần **14 vật cùng lúc**: lốc đi qua một bụi cây rậm có thể ôm một lúc ba bốn chục vật, mỗi
cái một `Update` riêng, và khung hình tụt thấy rõ trên điện thoại.

Va chạm tắt **ngay** lúc bị cuốn: một gốc cây đang bay ngang tầm người mà vẫn chặn đường thì
người chơi đâm vào không khí.

```
DO TRONG PLAY
   loc di qua      -> cuon duoc CotDa_0, CotDa_7, DaTang_24, DaTang_32
   bi loai         -> DaTang_3 (rong 7,8 m), DaTang_15 (rong 6,7 m)  qua to
   loc tan         -> ca bon: hien=False, da ve dung toa do cu
   sau 30 giay     -> ca bon: hien=True, va cham bat lai, component tu huy
```

#### Thời gian mọc lại: 30 giây

`VatTheBiCuon.GiayHoiSinh`. Trước đây là 60 giây — cả một phút nhìn vào chỗ trống ở giữa
nghĩa địa thì thấy rõ là "có cái gì đó biến mất", chứ không ra một cơn lốc quét qua.

Con số nằm ở **một chỗ duy nhất** và mọi thứ khác đọc từ đó. Ghi chú trong `Tornado.Dissipate`
cũng trỏ về hằng số ấy chứ không viết lại con số — hai bản chép tay của cùng một con số thì
sớm muộn cũng lệch nhau, đúng cái bẫy đã vấp với `hold` của vòng phép.

```
DO TRONG PLAY (menu 19)
   loc tha luc t = 1,01s, song 6,0s
   t = 4,08s   loc dang cuon 7 vat the
   t = 7,10s   ca 7 vat TAN BIEN cung con loc
   sau 15,0s   dang hien = 0/7      (chua duoc moc lai)
   sau 31,5s   dang hien = 7/7, component VatTheBiCuon da tu go ra
```

## Giựt sét — mạch điện chạy qua cả đám quái

Khác hẳn Sấm sét: Sấm sét giáng tia từ trên trời xuống những điểm rời rạc, còn Giựt sét là
**một mạch** chạy từ người chơi xuyên qua đám quái. Ba điều làm nó đọc ra là "lan" chứ
không phải "đánh nhiều con cùng lúc":

- **Mỗi nhịp cách nhau 0,07 giây.** Vẽ hết một lượt trong cùng khung hình thì người xem
  thấy một mảng sét hiện ra đồng loạt, không đọc được thứ tự lan.
- **Tia mới vẽ từ con VỪA TRÚNG**, không phải từ tay phù thuỷ — đó là thứ nói lên mạch điện
  đang di chuyển.
- **Sát thương giảm dần** 85% mỗi nhịp. Không giảm thì kỹ năng này mạnh hơn mọi kỹ năng
  khác trong game.

Một con chỉ ăn một nhịp (`HashSet` đánh dấu), không thì mạch nhảy qua nhảy lại giữa hai con
đứng cạnh nhau cho hết lượt.

Đo bằng **bảy bia bất tử** xếp cách nhau 3,5 m — đo bằng quái thật thì 30 sát thương giết
nó ngay nhịp đầu, con chết rồi thì không đếm được nó có bị trúng hay không:

```
bia 0  cach  4.2 m : mat 30.00 mau
bia 1  cach  7.6 m : mat 25.50 mau  (85% cua nhip truoc)
bia 2  cach 11.1 m : mat 21.67 mau  (85%)
bia 3  cach 14.6 m : mat 18.42 mau  (85%)
bia 4  cach 18.0 m : mat 15.66 mau  (85%)
bia 5  cach 21.5 m : mat 13.31 mau  (85%)
bia 6  cach 25.0 m : mat  0.00 mau   <- dung gioi han 6 muc tieu
```

## Sét đánh trúng gì thì cháy chỗ đó

Cả **Sấm sét** lẫn **Giựt sét** giờ để lại dấu ở nơi tia chạm: khói bốc lên, và **vết nám
đen dán lên chính bề mặt** của chướng ngại vật bị trúng — bia mộ, thân cây, tảng đá.

`GroundDecal.Spawn` không dùng được cho việc này: nó dựng một cái **đĩa ôm theo mặt đất**,
nên dán lên thân cây hay bia mộ dựng đứng thì vết nám nằm ngang lơ lửng trong không.
`NamDen` là một miếng vuông xoay theo **pháp tuyến bề mặt**, nhấc ra 2 cm để hai mặt phẳng
không trùng nhau và nhấp nháy theo góc nhìn.

Miếng vuông ấy phải là mesh **mới mỗi lần**. `GroundDecal.OnDestroy` huỷ luôn mesh của nó,
mà `GroundDecal.QuadMesh()` là mesh **tĩnh dùng chung** — huỷ một lần là mọi thứ khác đang
dùng nó cùng mất hình.

#### Raycast từ tâm nổ ra ngoài không đủ

`NamChuongNgai` quét các chướng ngại vật quanh chỗ sét đánh rồi raycast từ tâm nổ tới từng
cái, lấy **điểm và pháp tuyến thật** của bề mặt. Dùng `Collider.ClosestPoint` thì chỉ có
điểm, không có pháp tuyến, và nó còn ném lỗi trên `MeshCollider` không lồi.

Nhưng khi **tâm nổ nằm ngay bên trong vật** — sét đánh trúng chân một cái lăng mộ chẳng hạn
— thì raycast từ trong ra **không trúng gì cả**: Physics bỏ qua chính cái collider mà tia
bắt đầu bên trong nó. Đo được:

```
danh vao manh nam sat dat        : 2 vet nam
danh NGAY TRONG lang mo cao 4,68 m : 0 vet nam   <- truoc khi sua
                                     1 vet nam   <- sau khi ban nguoc tu ngoai vao
```

Cách chữa: khi lượt raycast đầu trượt, bắn **ngược từ ngoài vào** — lấy một hướng ngang
ngẫu nhiên, lùi ra ngoài bán kính bao của vật rồi bắn về phía nó.

Bỏ qua `TerrainCollider`: mặt đất đã có vệt cháy riêng, dán thêm một miếng vuông dựng lên
giữa đất thì nhìn ra một tấm bia.

**Sấm sét chỉ dùng 45% lượng khói.** Nó thả 22 tia trong 2,4 giây; mỗi tia một đám khói dày
như Giựt sét thì cả vùng chìm trong khói và không còn nhìn ra sân chơi.

#### Đo trong Play mode thì phải KIỂM TRA là đang ở Play mode

Nâng tầm 13 → **19,5 m** (+50%) và hạ năng lượng 28 → **14** (−50%). Kiểm bằng một bia bất
tử đặt ở **18 m** — ngoài tầm cũ, trong tầm mới:

```
tam cu  13,0 m : bia mat  0.00 mau -> TRUOT
tam moi 19,5 m : bia mat 30.00 mau -> TRUNG
```

Trước khi ra được hai dòng ấy tôi đã mất nhiều lượt vì một chuyện khác hẳn: **Editor đã tự
thoát Play mode** mà tôi không biết. Trong Edit mode thì `Start()` không chạy, coroutine
không chạy, nên `GiatSet` chỉ nằm im — bia không mất máu, và tôi đi tìm lỗi trong thuật
toán chọn mục tiêu. Dấu hiệu lộ ra là **bốn đối tượng `GiatSet` vẫn còn sống**: chúng phải
tự huỷ sau 0,6 giây, còn sống nghĩa là coroutine chưa hề chạy.

Mọi lệnh đo hành vi nên mở đầu bằng:

```csharp
if (!Application.isPlaying)
{ result.LogError("KHONG o Play mode - coroutine se khong chay, do la vo nghia"); return; }
```

**Bia đo phải nằm ở một layer TRỐNG.** Đo bằng layer `Enemy` thì quái thật trong cảnh cũng
lọt vào phép quét: con nào gần hơn ăn điểm cao hơn, mạch sét đi về phía nó, và số liệu đo
được là của một trường hợp khác hẳn.

Và bia tạo trong Edit mode nằm thẳng trong **cảnh thật** — phải xoá trước khi cảnh được
lưu, không thì chúng đi luôn vào bản build.

#### Chỗ vừa bị sét đánh phải trông như vừa bị CHÁY

Ban đầu chỗ trúng dùng `LightningImpact` — cùng hàm với Sấm sét. Nó toè ra một đám **tàn
điện xanh** cùng màu với tia sét: nhìn ra "có điện ở đây", không ra "chỗ này vừa bị đánh
cháy". `SetChayDen` thay bằng ba lớp:

1. **Khói bốc lên và NỞ RA khi lên cao.** Trọng lực để **âm** (−0,16) nên khói nhẹ hơn
   không khí và bay lên. `sizeOverLifetime` là phần quan trọng nhất — thiếu nó thì ra một
   cột xám thẳng đuỗn chứ không ra khói.
2. **Than đỏ li ti** bay lên rồi tắt. Không có nó thì khói trên nền đêm gần như vô hình.
3. **Vệt cháy trên mặt đất**, ở lại 4 giây sau khi khói tan hết.

**Khói đen thật thì không nhìn thấy được.** Lần đầu tôi tô đúng màu khói đen
(0,12 0,10 0,09, độ đục 0,75 — chính là `SmokeMat` dùng chung). Hạt **có thật**, đúng chỗ,
bốc lên tới y = 2,83 — mà trên ảnh chụp **không thấy gì cả**.

Phép đo trả lời được câu này: chụp hai ảnh cùng góc, một có khói một không, rồi trừ nhau.

```
SmokeMat (0,12 0,10 0,09):  8,0% diem anh doi khac, do lech tb 0,0128
KhoiSetMat (0,42 0,39 0,36): 34,0% diem anh doi khac, do lech tb 0,0625
```

Đếm số hạt sống chỉ chứng minh hệ hạt *có chạy*. Nó không nói được người chơi có **nhìn
thấy** hay không — hai ảnh trừ nhau mới nói được.

Màu cuối cùng là tint vật liệu (0,42 0,39 0,36) nhân với màu hạt (0,62 0,58 0,54), ra
khoảng **(0,26 0,23 0,19)** — xám rất đậm. Tôi có thử hạ thêm một bậc cho "đen" hơn: khói
mờ hẳn đi, trên ảnh chỉ còn một vệt xám nhạt. Muốn khói đọc ra là khói trên nền đêm thì
phải để nó sáng hơn nền — không hạ thêm được nữa.

#### Ảnh mặt cắt tia sét dựng trong Blender

`LightningArc` không vẽ hình tia sét bằng ảnh — nó dựng hình khối theo đường gấp khúc rồi
dán **mặt cắt ngang** lên. Mặt cắt ấy quyết định tia trông dày hay mỏng, sáng hay đục.

Bản vẽ bằng `Mathf.Pow` cho ra một viền gradient đều tăm tắp, nhìn ra ống nhựa chứ không ra
tia điện. Bản Blender (Cycles + Glare Fog Glow) có quầng mềm và loang không đều như quầng
thật: lõi trắng đặc 0–10 px, chuyển dần sang xanh, tắt hẳn ở 62 px.

**Ba cái bẫy khi render ảnh này:**

1. **MCP không render được.** Cả Cycles lẫn EEVEE đều trả về ảnh **rỗng tuyệt đối** khi
   lệnh đi qua MCP — `Render Result` có kích thước (0, 0). Chỉ `blender -b -P script.py`
   mới chạy thật. (`bpy.ops.render.opengl` thì chạy được qua MCP, nhưng đó là render
   viewport: không có Glare, không có emission đúng.)

2. **Compositor phải lấy ảnh từ `CompositorNodeRLayers`, không phải `NodeGroupInput`.** Nối
   từ Group Input thì luồng ảnh đứt hẳn — file PNG ra 7783 byte và tổng RGB lớn nhất trên
   toàn ảnh là **3/765**, trong khi tắt compositor đi thì file ra 27096 byte. Group Input
   chỉ có nghĩa khi node group được dùng **lồng** trong cây khác; đây nó là cây compositor
   gốc của cảnh.

3. **Gradient phải nằm trong VẬT LIỆU.** Lần đầu tôi chồng ba mặt phẳng (lõi / quầng trong
   / quầng ngoài) — ra ba **dải cạnh sắc**, không ra quầng. Glare không cứu được. Phải dùng
   `TexCoord → SeparateXYZ → Abs → ColorRamp → Emission`.

Và với icon có hai lớp ống lồng nhau thì **lõi phải nằm gần camera hơn quầng**: để cả hai ở
z = 0 thì ống lõi (bán kính 0,019) nằm lọt hẳn trong ống quầng (0,058), lõi bị che kín và
cả cái icon ra một nét xanh phẳng như vẽ bằng bút chì màu.

Ảnh Blender trả về **thẳng**, không đi qua `TextureFactory.Get`: `Get` đặt lại
`t.name = "Tex_..."` cho mọi thứ nó trả về, mà đây là một **asset thật** trong `Resources` —
đổi tên nó là sửa asset của dự án từ trong lúc chạy.

#### Nút thứ bảy nằm ở cung riêng

Sáu nút cũ xếp thành hai cung ba nút. Nhét nút thứ bảy vào một trong hai cung ấy thì bước
góc hẹp lại và các nút dính nhau. Nút mới nằm ở **cung thứ ba** bán kính **80,2·s**, góc
**26,1°** — giữa nút Sấm sét và góc màn hình, đúng chỗ người dùng khoanh tròn.

Đo lại khoảng cách: tới nút Sấm sét **128,4**, tới nút Mưa băng **129,6**, đường kính nút
**105,6**. Chỗ hẹp nhất còn hở **22,8**.

`HoiChieu01` bỏ hẳn nhánh `default: return KhiengCooldown01`. Thêm kỹ năng mới mà quên sửa
chỗ ấy thì nút mới lấy nhầm vòng hồi chiêu của Khiêng — một lỗi nhìn vào không đọc ra được.

## Quỷ cây — con quái chạy nhanh hơn bạn

Quái thứ bảy, dựng từ model **Horned Shadowfiend**. Nó bắn **tia sét xanh lá** — cùng khối
`GiatSet` với kỹ năng số 7 của người chơi nên hiệu ứng y hệt, chỉ đổi màu và tắt phần lan.

| | Quỷ cây | Quỷ dữ | Bộ xương |
|---|---|---|---|
| tốc độ | **5,98 m/s** | 2,4 | 2,7 |
| tầm đánh | 8 m | 10 m | 2,4 m |
| tầm để ý | 20 m | 24 m | 15 m |
| lùi ra khi gần hơn | 4,5 m | 7 m | — |
| hồi chiêu | 1,5 s | 2 s | 1,7 s |
| máu | 110 | 130 | 70 |
| kháng lửa | **−0,25** | +0,40 | 0 |
| kháng sét | +0,35 | 0 | 0 |
| sát thương mỗi đòn | 14 | 22 | 12 |

**Người chơi đi 5,2 m/s, con này 5,98 — nhanh hơn 15%.** Đây là con quái duy nhất trong
game **không thể chạy thoát**: cứ quay lưng bỏ chạy là nó bám theo và rút ngắn khoảng cách.
Muốn thoát phải giết, phải đóng băng, hoặc phải nấp sau cái gì đó.

Vì thế tầm bắn chỉ 8 m và nó **chịu đòn kém** — 110 máu và **sợ lửa 25%**. Một con vừa
nhanh vừa bắn xa vừa trâu thì không còn cách nào chơi lại.

Khoảng lùi để 4,5 m chứ không 7 như Quỷ dữ. Nó chạy nhanh gấp đôi bọn kia, để ngưỡng lùi xa
thì cả đàn cứ giật lùi ra rồi lại lao vào — nhìn ra một đám giãn gió chứ không ra một đàn
quái vây đánh.

### Màu nằm ở quầng, không ở lõi

`GiatSet.PhongCuaQuai` đổi `glowColor` sang xanh lá **(0,25 / 1 / 0,30)** nhưng giữ
`coreColor` gần như trắng **(0,85 / 1 / 0,85)**. Đó là cách tia sét thật trông: cái người
chơi đọc ra màu là vùng sáng toả quanh nó. Nhuộm cả lõi thành xanh lá thì tia mất hết vẻ
cháy bỏng, nhìn ra một cọng nhựa phát sáng.

Phần **lan sang con khác bị tắt** (`maxChains = 0`): quái chỉ có một mục tiêu là người
chơi, không có gì để mạch điện nhảy tiếp sang.

#### Đổi màu thôi chưa đủ — phải tô ngay trong Start

Lần đầu làm, tia của Quỷ cây **loé lên xanh dương đúng một khung hình** rồi mới sang xanh
lá — tức nó nháy đúng màu tia của người chơi trước mỗi phát bắn.

`LightningArc.Start` tạo material bằng `new Material(VfxFactory.BoltGlowMat)`, mà vật liệu
gốc ấy mang sẵn màu **(0,45 / 0,70 / 1)** — xanh dương của người chơi. Màu riêng chỉ được
áp trong `Update`, qua `SetFade`.

Chỗ chết người nằm ở **vòng đời Unity**: một object tạo ra giữa khung N thì `Start` của nó
chạy **cuối khung N**, còn `Update` đầu tiên mãi khung N+1. Giữa hai mốc đó có **một lần
vẽ**. Nên khung render đầu tiên của tia dùng đúng màu gốc của vật liệu.

Một khung trên mười tám (0,3 giây ở 60 fps) chỉ là 5% thời gian, nhưng nó rơi trúng lúc tia
**sáng nhất**, nên nhìn ra rất rõ. Cách chữa: gọi `SetFade` ngay trong `Start`, sau
`MakeRenderer` và trước `Rebuild`.

> Lỗi một-khung-hình kiểu này **không đo được bằng ảnh chụp rời rạc** — mỗi lệnh chỉ chụp
> được một khung, mà tia chỉ sống 0,3 giây. Chỗ dựa để kết luận là vòng đời Unity, không
> phải phép đo.

Cùng cách chữa này bảo vệ mọi chỗ khác dùng `LightningArc` (`StunnedEffect`, `Tornado`,
`LightningStrike`, `CastCrackle`): chúng để `coreColor`/`glowColor` mặc định, đúng bằng màu
vật liệu gốc, nên `SetFade` trong `Start` không đổi gì với chúng.

`GiatSet.PhongCuaQuai` đổi `glowColor` sang xanh lá **(0,25 / 1 / 0,30)** nhưng giữ
`coreColor` gần như trắng **(0,85 / 1 / 0,85)**. Đó là cách tia sét thật trông: cái người
chơi đọc ra màu là vùng sáng toả quanh nó. Nhuộm cả lõi thành xanh lá thì tia mất hết vẻ
cháy bỏng, nhìn ra một cọng nhựa phát sáng.

Phần **lan sang con khác bị tắt** (`maxChains = 0`): quái chỉ có một mục tiêu là người
chơi, không có gì để mạch điện nhảy tiếp sang.

**Khác thiên thạch ở chỗ đòn này trúng ngay** — không có 0,8 giây rơi để né. Bù lại tầm bắn
ngắn hơn nhiều (8 m so với 10 m) và sát thương thấp hơn (14 so với 22).

### Dòng quái riêng thứ hai

Vào màn có **4 con**, cứ **4 phút thêm 10 con**, trần 40 con sống cùng lúc. Bốn số chỉnh
trong `GameDirector`: `soQuyCayBanDau`, `chuKyThemQuyCay`, `soQuyCayMoiDot`, `quyCayToiDa`.

Giờ có **hai** dòng quái riêng nên phần logic được gộp thành hai hàm dùng chung —
`RaiDongRieng` và `DemGioDongRieng` — còn các trường vẫn để phẳng cho từng dòng. Gộp cả
trường vào một class serializable thì sạch hơn về code, nhưng đổi kiểu dữ liệu là **mất
sạch giá trị đã lưu trong hai cảnh**, phải đặt lại bằng tay từng con số.

Hai dòng được rải bằng cùng xoắn ốc vàng nhưng **lệch pha khác nhau** (0,5 và 0,25 lần góc
vàng), để chúng không dồn vào cùng những điểm đó.

### ⚠️ Từ khoá tìm model phải là mảnh tên RIÊNG

Thêm con này làm **hỏng từ khoá của Quỷ dữ**. Trong `MeshyImports` giờ có ba con dính nhau:

```
Abyssal Horned Demon   -> Quy du
Horned Shadowfiend     -> Quy cay
Veil of the Abyss      -> Phu thuy
```

Từ khoá cũ của Quỷ dữ là `"Horned"` — nó khớp **cả bốn file** của hai con đầu, và
`TimFbxTheoTen` trả về file đầu tiên nó gặp. Tức Quỷ dữ có thể bị dựng bằng model Quỷ cây
mà **không báo lỗi gì cả**. Đã đổi sang `"Abyssal"`; `"Abyss"` cũng không dùng được vì trúng
luôn con phù thuỷ.

Mỗi lần đưa model mới vào, kiểm lại xem từ khoá của những con cũ có còn duy nhất không.

### Act2Baker từng làm mất prefab của chính nó

`Act2Baker.Kho.DienVao` vốn **chỉ điền năm ô** của `enemyPrefabs`, thiếu sẵn Quỷ dữ và Quỷ
cây từ trước. Nướng lại Act2 là hai con ấy mất sạch prefab, và game **lặng lẽ** dựng hình
chung bằng code thay vì model — không một dòng cảnh báo nào.

Nay điền đủ **bảy ô đúng thứ tự `MonsterType`**. Thêm loại quái mới thì phải sửa bốn chỗ
cùng lúc, thiếu một là dính lại đúng lỗi này:

```
GameAssets.enemyPrefabs        so o
AssetBaker.Library.enemies     so o
Act1.unity, Act2.unity         mang trong canh
Act2Baker.Kho.DienVao          danh sach Lay(...)
```

## Quỷ dữ — con quái gọi thiên thạch

Quái thứ sáu, dựng từ model **Abyssal Horned Demon** đưa từ Meshy vào. Nó **đứng xa gọi
thiên thạch** xuống chỗ người chơi đang đứng — cùng khối `ThienThach` với kỹ năng số 5 của
người chơi nên hiệu ứng y hệt, chỉ khác **mỗi lần đánh chỉ một quả** (`Spawn` chứ không
phải `SpawnLoat`).

| | Quỷ dữ | Mụ phù thuỷ | Người chơi (Thiên thạch) |
|---|---|---|---|
| tầm đánh | **10 m** | 12 m | — |
| tầm để ý | 24 m | 22 m | — |
| lùi ra khi gần hơn | 7 m | 6,5 m | — |
| hồi chiêu | **2 s** | **2 s** | — |
| máu | 130 | 95 | — |
| kháng lửa | 0,40 | 0,35 | — |
| sát thương mỗi quả | 22 | 16 | 85 |
| bán kính nổ | 2,8 m | 2,6 m | 4,2 m |
| vùng lửa để lại | **không có** | không có | 4,5 m / 5 s / 26 mỗi giây |

**Tầm để ý phải lớn hơn tầm đánh.** Để nhỏ hơn thì nó chỉ bắt đầu chú ý đến người chơi khi
đã vào trong tầm bắn — tức không bao giờ đứng ở rìa tầm mà đánh, phí cả cái tầm 15 m.

**Thiên thạch của Quỷ dữ chỉ gây sát thương — không để lại vùng lửa.** Lúc đầu nó để lại
một vùng cháy nhỏ (2,2 m / 2,5 s / 8 mỗi giây), nhưng hồi chiêu 2 giây ngắn hơn thời gian
cháy nên các vùng lửa **chồng lên nhau liên tục**: đo được 3–4 vùng cùng cháy một chỗ, và
chỗ người chơi đứng gần như lúc nào cũng có lửa. Nay tắt hẳn.

Tắt bằng cách gán **rõ ràng** `chayBanKinh / chayThoiGian / chaySatThuongMoiGiay = 0` trong
[`EnemyAI.GoiThienThach`](Assets/Scripts/Enemies/EnemyAI.cs) — `ThienThach` mặc định cháy
**5 giây**, không gán gì thì mỗi quả đá của quái còn đốt sân lâu hơn cả lúc trước. Bên
`ThienThach` có thêm chốt chặn: thời gian hoặc bán kính bằng 0 thì không gọi `VungLua.Spawn`
nữa.

> Ba trường `luaBanKinh / luaThoiGian / luaSatThuongMoiGiay` đã **xoá khỏi `EnemyAI`**, chứ
> không phải chỉ đặt về 0. Còn để đó thì prefab `Enemy_QuyDu` — nơi ba con số cũ vẫn nằm —
> có đường bật lại vùng lửa mà không ai để ý.

**Đòn này né được.** Thiên thạch rơi từ trên trời xuống chỗ người chơi *đang đứng*, mất
khoảng 0,8 giây; bước ra là thoát. Khác hẳn quả cầu lửa của mụ phù thuỷ, bay thẳng theo
đường ngắm.

**Khoảng đánh hiệu dụng là một dải hẹp 7–10 m** (`khoangLui` 7, tầm 10). Gần hơn 7 m thì nó
lùi ra — nhưng vừa lùi vừa bắn được, nên không phí nhịp nào.

Đo trong Play mode, 7 con Quỷ dữ đánh người chơi trong 20 giây:

```
[DO] SAU 20 GIAY: thien thach da roi=11  vung lua da tao=0
```

Đếm bằng **tập hợp các đối tượng đã từng thấy**, không phải số đang sống: một quả thiên
thạch chỉ tồn tại khoảng một giây, hai lần đọc cách nhau là trượt mất. Con số 11 quả đã rơi
là thứ làm phép đo có nghĩa — nếu nó bằng 0 thì "0 vùng lửa" chẳng chứng minh được gì.

**Bỏ vùng lửa làm đòn này nhẹ đi đáng kể.** Trước đây đứng sai chỗ là ăn thêm 8 sát
thương mỗi giây suốt 2,5 giây, mà các vùng còn chồng nhau. Giờ né được quả đá là không mất
gì thêm. Muốn gắt lại thì nâng `satThuongCau` (đang 22) chứ đừng bật lại vùng lửa.

### Dòng quái riêng, không theo nhịp đợt

Vào màn là có **7 con**, và **cứ 2 phút thêm 5 con**. Bốn số chỉnh trong `GameDirector`:
`soQuyDuBanDau`, `chuKyThemQuyDu`, `soQuyDuMoiDot`, `quyDuToiDa`.

Quỷ dữ được đếm trong **một danh sách riêng**, không nằm chung `alive`. Nhịp đợt quái thường
dựa vào `alive.Count == 0` để biết khi nào thả đợt mới; cho Quỷ dữ vào chung thì con số đó
không bao giờ về 0 nữa và **các đợt quái thường dừng hẳn**.

Có **trần 40 con** sống cùng lúc (`quyDuToiDa`; đặt 0 là bỏ trần). Không có trần thì sau
mười phút là ba mươi con, sau nửa tiếng là tám mươi — máy không kéo nổi mà người chơi cũng
không còn chỗ mà đứng.

Đo được khi chạy thật:

```
MOT con Quy du, nguoi choi bat tu dung yen trong tam
  tam 15 m / hoi 4,5 s :  12,0 sat thuong moi giay
  tam 10 m / hoi 2   s :  22,4 sat thuong moi giay   <- gap 1,87 lan

MUOI con Quy du, nguoi choi 170 mau dung yen (Act1)
  tam 15 m / hoi 4,5 s : chet sau 13 - 17 giay
  tam 10 m / hoi 2   s : chet truoc giay thu 9,5

nhip tang vien (chu ky rut xuong 15 giay de do cho nhanh)
  t= 6,3s : 2 con      t=19,3s : 7 con      t=36,5s : 12 con
```

Tầm bắn giảm từ 15 xuống 10 m làm ít con vào tầm hơn cùng lúc (đo được 2/10 con trong tầm),
nhưng hồi chiêu 2 giây thay cho 4,5 vẫn đẩy sát thương lên gần gấp đôi.

> ⚠️ **Mười con giết người chơi đứng yên trong chưa tới 9,5 giây** (Act1, 170 máu). Người
> chơi thật thì chạy và né được, nhưng nếu thấy quá gắt thì hạ `soQuyDuBanDau` hoặc nới
> `attackCooldown` trong [EnemyFactory.cs](Assets/Scripts/Enemies/EnemyFactory.cs) — **và
> nhớ sửa cả prefab `Enemy_QuyDu`**, xem bên dưới.

### Giảm số quái lúc bắt đầu 30%

Giảm **mỗi loại** 30%, chỉ số lượng lúc vào màn:

```
                            truoc   sau
quai thuong moi dot Act1      5      4     (3,5 lam tron len)
quai thuong moi dot Act2     10      7
phu thuy rai san Act2        10      7
bo xuong rai san Act2        10      7
Quy du                       10      7
Quy cay                       5      4     (3,5 lam tron len)
```

Hai chỗ ra **3,5** đã làm tròn **lên** — 3 là giảm 40%, 4 là giảm 20%, mà 4 thì gần 3,5
đúng bằng 3.

> **Hai đường sinh quái khác nhau, dễ sót một.** `startingCount` là quái **mỗi đợt**, còn
> `soPhuThuyRaiSan` / `soBoXuongRaiSan` là quái **rải sẵn khắp bản đồ** ngay lúc `Start`
> (`RaiQuaiKhapBanDo`). Act1 để cả hai bằng 0 nên nhìn Act1 thì tưởng không có đường này;
> Act2 thì mỗi loại 10 con. Sửa mỗi `startingCount` là Act2 vẫn còn nguyên hai chục con.

Đo trong Play mode ở Act1:

```
[GameDirector] Da rai 7/7 Quy du khap ban do.
[GameDirector] Da rai 4/4 Quy cay khap ban do.
[DO] SO QUAI LUC BAT DAU: thuong=5  QuyDu=7  QuyCay=4
```

**Quái thường ra 5 chứ không phải 4 — đúng như thiết kế.** Đợt 1 thả `startingCount` con
rồi **cộng thêm một mụ phù thuỷ chắc chắn có** (xem `SpawnWave`): chỉ bốc ngẫu nhiên với tỉ
lệ 16% thì nhiều ván người chơi không gặp mụ nào, vào trận ba lần vẫn chưa biết trong game
có loại quái đánh từ xa. Trước khi giảm là 5 + 1 = 6.

> Đếm quái phải đợi **sau giây 2,5**. Đợt quái thường đầu tiên không thả ngay lúc `Start`
> mà hẹn `waveTimer = 2.5f`. Lần đo đầu tôi đếm ở giây 2 và ra `thuong=0` — tưởng
> `startingCount` không ăn.

**Con số thật nằm trong CẢNH, không phải trong code.** `startingCount`, `soQuyDuBanDau`,
`soQuyCayBanDau` là trường public của `GameDirector`, nên `Act1.unity` và `Act2.unity` mỗi
cảnh giữ một bản sao riêng — sửa giá trị mặc định trong
[GameDirector.cs](Assets/Scripts/GameDirector.cs) là **không ăn gì cả**. Đã sửa cả bốn chỗ:
code, hai cảnh, và hai hằng của baker (`AssetBaker.StartingEnemies`,
`Act2Baker.SoQuaiDotDau`) để lần nướng lại cảnh sau không kéo số cũ về.

> Cảnh đang mở trong Editor giữ **bản trong bộ nhớ**. Sửa file `.unity` trên đĩa xong phải
> `EditorSceneManager.OpenScene` lại, không thì bấm Play vẫn chạy số cũ — và lần lưu cảnh
> kế tiếp sẽ ghi đè mất phần vừa sửa.

**`startingCount` không chỉ là đợt đầu.** Số quái mỗi đợt là
`startingCount + round((Wave-1) × 1,8)`, nên hạ nó xuống là **mọi đợt về sau đều ít đi đúng
chừng ấy con**, không riêng lúc vào màn.

### Ba chỗ dễ sai khi thêm một loại quái mới

**`MonsterType` phải thêm ở CUỐI danh sách.** Số thứ tự của enum được lưu thẳng vào mảng
`enemyPrefabs` trong cảnh; chèn vào giữa là mọi con quái trong cảnh đổi thành một loại khác.

**Chỉnh số của quái thì phải sửa CẢ HAI chỗ.** `EnemyFactory` chỉ chạy lúc *nướng* prefab;
trong game, quái được sinh ra từ prefab trong `Assets/Prefabs` với những con số đã đóng băng
ở đó. Sửa mỗi code là không ăn gì cả — cùng cái bẫy với `Player_Sorceress`.

**Mảng `enemyPrefabs` nằm trong CẢNH, không phải trong code.** Sửa `new GameObject[5]` thành
`[6]` trong `GameAssets.cs` không làm mảng đã lưu trong `Act1.unity` / `Act2.unity` dài ra —
phải mở rộng và gán prefab bằng tay cho từng cảnh. Cùng một cái bẫy với prefab đè lên giá
trị code.

**Nướng lẻ một prefab thì phải tự dựng ba bộ đệm.** `texMap`, `matMap`, `usedNames` chỉ được
khởi tạo ở đầu mục 1 (`BakeSilent`). Gọi thẳng `NuongQuaiTuModel` mà không khởi tạo thì
`BakeMaterial` ném `NullReferenceException` ngay dòng đầu — và lúc đó model đã được dựng
trong cảnh đang mở rồi, nên **cảnh dính ba vật thể rác**. Tôi đã vấp đúng thế. `DungBoDem`
trong [AssetBaker.cs](Assets/Editor/AssetBaker.cs) làm việc đó, và còn nạp sẵn mọi tên file
đang có vào `usedNames` để `AssetDatabase.CreateAsset` không ghi đè lên vật liệu của con
quái khác.

Từ khoá tìm model phải là **"Horned"**, không phải "Abyss": trong `MeshyImports` đang có cả
*Abyssal Horned Demon* lẫn *Veil of the Abyss*, lấy "Abyss" là trúng cả hai và Quỷ dữ có thể
bị dựng bằng model của mụ phù thuỷ.

> ⚠️ Đừng chạy **mục 1** chỉ để thêm một con quái — nó nướng lại tất cả và dựng lại cả ba
> cảnh, tức xoá sạch địa hình Act2 đã vẽ lại và mọi thứ đặt tay khác. Dùng **mục 11** rồi
> gán prefab vào cảnh bằng tay.

## Act2 — bụi cỏ

Bảy mươi bụi rải khắp bản đồ, mẫu ở `Assets/Models/BuiCo` (bốn biến thể). Mỗi bụi
**10 cây cỏ + 2 bông trắng**, tổng 288 đỉnh cỏ và 54 đỉnh hoa.

**KHÔNG để trong `Resources`.** Mọi thứ nằm trong `Resources` đều bị nhồi vào bản build dù
có dùng hay không, và không cách nào loại bớt. Chỉ những thứ được nạp bằng
`Resources.Load` lúc chạy mới được ở đó — trong dự án này là `BangRoi` (mesh băng, do
`VfxFactory` bốc ngẫu nhiên lúc chạy), `Flipbooks` và `Icons`. Bụi cỏ thì đặt sẵn vào cảnh
từ Editor, tham chiếu trực tiếp, nên thuộc `Models`.

Tôi đã đặt nhầm nó vào `Resources` vì làm ngay sau mesh băng và đi theo quán tính — mesh
băng *phải* ở đó, bụi cỏ thì không.

**Mỗi cây cỏ là HAI DẢI GIAO NHAU hình chữ thập.** Một dải phẳng thì ở góc nhìn nào đó nó
gần như song song với hướng nhìn và teo lại thành một đường kẻ — cả bụi 10 lá mà chỉ còn
thấy năm sáu cái. Hai dải vuông góc thì luôn có một cái quay mặt về phía người xem. Cùng
một cái bẫy với mảnh băng và tia lửa trước đây.

**Bông hoa có nhụy tròn.** Cánh nằm ngang thì nhìn từ ngang bông hoa thành một vạch mờ,
gần như biến mất. Nhụy khối tám mặt thì góc nào cũng thấy được, nên bông hoa không bao giờ
mất hút.

#### Cỏ và hoa phải là HAI OBJECT, không phải hai nhóm vật liệu

Thử xuất một object với hai nhóm `usemtl` — Unity gộp hết lại thành **một submesh**, mất
luôn chỗ phân tách cỏ với hoa, kể cả khi đã cho hai vật liệu màu khác nhau trong file
`.mtl`. Hai object riêng trong cùng file OBJ thì Unity tạo hai node con, mỗi node một mesh,
và gán vật liệu cho từng cái là chuyện dễ.

(Blender chỉ ghi màu vào `.mtl` khi vật liệu **có nodes**. Vật liệu tạo bằng
`bpy.data.materials.new` không có nodes nên cả hai đều ra `Kd 0.8 0.8 0.8` — giống hệt
nhau.)

#### Ràng buộc chỗ đặt

Mỗi bụi phải nằm trên mặt đất trống: không dưới nước, không chồng bia mộ hay nhà cửa,
không trên sườn dốc, và cách bụi khác ít nhất 3,5 m.

Khoảng cách tối thiểu hạ từ **5 m xuống 3,5 m** khi lên 70 bụi. Giữ 5 m thì phần đất
trống hợp lệ không đủ chỗ cho 70 điểm — vòng tìm chạy hết lượt mà vẫn thiếu, và những
bụi đặt được sau cùng bị đẩy hết ra rìa bản đồ.

**Phép kiểm vật cản phải LOẠI BỎ mặt đất.** `Physics.CheckSphere` với mask `~0` bắt trúng
chính cái terrain ngay dưới chân, nên điểm nào cũng bị kể là "có vật cản" — lần đầu
**3787/4000** điểm bị loại vì lý do đó và không đặt được bụi nào.

**Bán kính phải tăng theo số bụi ĐÃ ĐẶT, không theo số lần THỬ.** Lần đầu tôi tăng theo số
lần thử; vòng lặp dừng ở lần thứ 75 nên cả 20 bụi nằm gọn trong bán kính 23 m, tụm giữa
bản đồ. Giờ sinh trước N điểm mục tiêu trải đều (góc vàng 137,508° + căn bậc hai bán
kính) rồi tìm chỗ hợp lệ quanh từng điểm.

Đo lại sau khi lên **70 bụi**: bán kính từ tâm trải **13,3 đến 46,4 m**, hai bụi gần nhau
nhất cách **3,6 m**, 0 bụi dưới nước, 0 bụi chạm bia mộ hay nhà, 0 bụi lơ lửng, 0 bụi
thiếu vật liệu. Tổng **23.940 đỉnh** — con số đáng để mắt tới khi tính hiệu năng WebGL.

## Act2 — tông màu và ánh trăng

Act2 dùng **bộ màu riêng**: xanh lam lạnh, có mặt trăng trên trời. Act1 giữ nguyên tông cũ.

| | Act1 | Act2 |
|---|---|---|
| Ambient equator | (0,14 0,12 0,11) — ngả nâu | **(0,085 0,155 0,190)** — xanh lam |
| Sương mù | (0,08 0,08 0,11), dày 0,014 | **(0,115 0,175 0,215)**, dày 0,0105 |
| Ánh trăng | (0,72 0,74 0,88) × 0,62 | **(0,52 0,72 0,98) × 0,88** |
| Đĩa trăng trên trời | không | **có**, kèm quầng sáng |

**Thành phần ĐỎ trong nguồn sáng mới là thứ quyết định tông màu.** Mặt đất Act2 vốn là đất
nâu — đỏ cao trong albedo. Ánh sáng chỉ **nhân** vào albedo, nên nâng xanh lên bao nhiêu
thì đất vẫn cứ nâu bấy nhiêu; phải **hạ đỏ xuống** thì cả màn chơi mới ngả xanh. Đo trên
ảnh chụp thật, hiệu B − R của toàn khung: **1,1 → 9,7**.

Và thứ kéo tông màu không phải bầu trời mà là **ánh sáng môi trường tầng equator/ground** —
nó chạm vào mọi vật trong cảnh. Bầu trời lúc đầu đã xanh sẵn rồi mà cảnh vẫn nâu.

**Mặt trăng** vẽ trong `S_Sky.shader`: một quầng rộng (`pow(cos)`) cộng một đĩa nhỏ rành
nét. Chỉ vẽ đĩa thì mặt trăng trông như chấm sơn dán lên trời; chỉ vẽ quầng thì thành vệt
mờ không ra hình. `_MoonDir` là hướng **nhìn tới** mặt trăng, tức **ngược** với hướng chiếu
của đèn — đặt sai dấu là trăng mọc sau lưng người chơi trong khi bóng đổ ngả về phía trước.

#### Vì sao phải tách hai bộ màu

`GameBootstrap` gọi `BuildSkyAndFog()` ở **mọi màn** lúc chạy. Đổi màu cho Act2 mà không
tách thì Act1 đổi theo — dù file `.unity` của Act1 vẫn giữ giá trị cũ, vì bản chạy ghi đè
lên. `LaAct2()` đọc tên cảnh đang mở để chọn bộ màu.

## Act2 — bản đồ vẽ tay trong Blender

Act1 sinh **hoàn toàn bằng code**. Act2 thì ngược lại: lấy nguyên bản đồ dựng tay
trong Blender, từng ngôi mộ từng gốc cây đều đúng chỗ người vẽ đã đặt.

| | |
|---|---|
| File gốc | `C:\Users\HP\Documents\Blender\graveyard_map_109x109.blend` |
| Kích thước | 109,2 × 109,2 m, chênh cao 3,83 m |
| Số vật thể | 746 (452 bia mộ, 229 đá, 58 cây, 5 nhà mồ, 1 hàng rào, 1 mặt đất) |
| Quái mỗi đợt | **Gấp đôi Act1** — đợt đầu 10 + 1 phù thủy bảo đảm = 11 con |
| Dựng lại | Menu **Diablo 2.5D ▸ 9. Dung Act2 tu ban do Blender** |

### Bản đồ được đưa vào qua HAI file, không phải một

```
Assets/BlenderMaps/GraveyardAct2/
    map_luoi.fbx     27 lưới gốc, mỗi lưới đúng một bản      (12,2 MB)
    map_vitri.fbx    746 cái "mốc" rỗng, mỗi cái một chỗ đặt (0,6 MB)
    bang_tra.txt     bảng đối chiếu, để tra khi cần
```

Tách làm hai là **có chủ ý**. Xuất thẳng cả 746 vật thể thì FBX phồng lên hơn 150 MB
và Unity phải giữ 2,22 triệu đỉnh, vì mỗi bản sao mang một bản lưới riêng. Tách ra
thì 746 vật thể cùng trỏ vào 27 lưới dùng chung — **nhẹ hơn hơn bốn lần**.

Phần "mốc rỗng" còn giải được một việc nữa: Blender dùng trục Z hướng lên, Unity dùng
Y hướng lên. Để Unity tự đọc transform từ file FBX thì nó tự lo phần đổi trục.

### Mặt đất là Unity Terrain, không phải lưới Blender

Tấm đất từ Blender chỉ là **một lưới tấm, đắp được đúng một vật liệu** — cả bản đồ rộng
109 m chỉ một màu, nhìn ra tấm bìa phẳng chứ không ra mặt đất. Nên nó được
[Act2Terrain.cs](Assets/Editor/Act2Terrain.cs) đổi thành **Unity Terrain thật**, trộn được
bốn lớp vật liệu chồng lên nhau:

| Lớp | Nhịp lặp | Tô ở đâu |
|---|---|---|
| `Act2_CoChet` | 2,4 m | nền, xen mảng theo một lớp nhiễu lớn |
| `Act2_DatTroc` | 3,7 m | nền còn lại, **và quanh mỗi ngôi mộ** (chân người viếng giẫm nát) |
| `Act2_SoiDa` | 1,9 m | sườn dốc, nơi đất không bám được |
| `Act2_Bun` | 5,1 m | chỗ trũng, nơi nước mưa đọng lại |

Mỗi lớp một **nhịp lặp khác nhau** — để hai lớp cùng nhịp thì chỗ nào trộn hai lớp sẽ lộ
ra ô vuông lặp lại đều tăm tắp.

**Hình dáng được giữ nguyên tuyệt đối.** Độ cao đọc lại từ chính lưới Blender bằng cách
rải từng tam giác lên lưới ô vuông 513×513 (không dùng bắn tia — va chạm của lưới vừa tạo
chưa kịp vào hệ vật lý thì tia sẽ trượt hết). Đo lại sau khi đổi:

| Nhóm | Số vật | Lệch trung bình |
|---|---|---|
| Bia mộ | 452 | 0,089 m |
| Đá | 229 | 0,086 m |
| Nhà mồ | 5 | 0,100 m |
| Cây | 58 | 0,250 m — đúng bằng độ chôn gốc bạn đặt trong Blender |

Ba hàm đo độ cao của game (`VfxFactory.GroundY`, `Terrain.SampleHeight`,
`WorldFactory.GroundHeight`) đều trả về cùng một giá trị.

**Độ bóng nằm trong kênh alpha, không phải trong ô `Smoothness`.** Với Unity Terrain,
kênh alpha của ảnh vật liệu **chính là độ bóng**. Các hàm vẽ đất sinh ra alpha loang lổ
0,5–1,0 — tức mặt đất bóng gần như gương: đèn của người chơi quét tới đâu là nổi lên một
mạng vân trắng loe loét như bọt nước, nhìn không ra đất. Đo được: điểm sáng nhất của mặt
đất là **1,00** (cháy trắng); sau khi ép alpha về **0,04** thì còn **0,37** và không còn
điểm trắng nào. Vặn ô `smoothness` của `TerrainLayer` **không ăn thua gì** — khi lớp không
có ảnh mask thì shader lấy thẳng từ alpha này. Hằng số `DoBongMatDat` trong
[Act2Terrain.cs](Assets/Editor/Act2Terrain.cs) giữ mức đó.

**Bạn tô thêm được bằng tay.** Chọn `MatDat_Terrain` trong Hierarchy, dùng bảng công cụ
Terrain trong Inspector — Paint Texture để quét thêm lối mòn, Raise/Lower để nắn gò đất.
Nét tô tay nằm trong `Assets/Terrain/Act2_MatDat.asset`. ⚠️ Chạy lại mục 9 sẽ **ghi đè**
nét tô tay đó, vì nó dựng lại địa hình từ đầu.

### Vẽ lại mặt đất cho gồ ghề như Act1

Bản đồ Blender vẽ ra một mặt đất **phẳng lì**. Đo trong vùng lõi bán kính 30 m:

| | Act1 | Act2 lúc đầu | Act2 bây giờ |
|---|---|---|---|
| chênh cao | 2,35 m | 1,44 m | 2,25 m |
| độ lệch chuẩn độ cao | 0,43 m | 0,22 m | 0,45 m |
| độ dốc trung bình trên 2 m | 4,3° | 1,9° | 4,2° |

Cắt ngang bản đồ ở `z = 0`, từ `x = −36` đến `x = +42` độ cao chỉ dao động trong **0,22 m
trên suốt 78 m** — đi trong Act2 như đi trên mặt bàn.

[Act2GoGhe.cs](Assets/Editor/Act2GoGhe.cs) **cộng thêm** một lớp Perlin ba tầng lên nền
sẵn có, dùng đúng tần số của Act1 (`WorldFactory.AnalyticHeight`): đồi thoải bước sóng
28 m, gò nhỏ 9 m, gợn lăn tăn 2,6 m. Lệch hạt phải khác Act1, không thì hai màn chơi ra
đúng một hình dạng đồi núi.

**Chạy lại được.** Lần đầu chạy, nền phẳng gốc được lưu ra `Act2_DoCaoPhang.bytes` và nét
tô gốc ra `Act2_ToMauGoc.bytes`; những lần sau đọc lại từ đó chứ không cộng dồn. Muốn đậm
nhạt khác thì sửa `BienDo` rồi chạy lại — 1,0 là biên độ của Act1, hiện để **0,80** vì nền
Act2 vốn đã có sẵn một phần gồ ghề.

#### Giữ vùng phẳng: kéo về giá trị tại tâm, đừng nhân về 0

Ba chỗ không được xê dịch: **11 vũng nước**, **dải sát rìa** (hàng rào là một lưới liền,
chân nó ở `y = 0,01` còn đất quanh đó cao 0,58–2,77 m — nó vốn được chôn vào vành đất), và
**nền dưới năm nhà mồ** (khối đá 4–6 m, đặt trên sườn dốc thì hở chân một bên).

Cách nghĩ đầu tiên — nhân mặt nạ về 0 trong vùng cần giữ — **sai**. Độ cao thêm ở ngoài
vành lên tới ±3,4 m mà trong vành là 0, nên cả vành chuyển tiếp biến thành một cái **gờ
bao quanh mỗi vũng nước**. Đo được: **63% số ô dốc hơn 18° nằm đúng trong vành này**, chỗ
dốc nhất **35,8°** trong khi Act1 chỉ có 12,6°.

Cách đúng là **kéo về đúng giá trị địa hình tại tâm vùng**. Lòng chảo được nâng hạ nguyên
khối, nên hình dạng chảo và độ sâu nước giữ y nguyên, mà hai đầu vành chuyển tiếp chỉ lệch
nhau đúng phần địa hình biến thiên trong vài mét — tầng đồi thoải có bước sóng 28 m nên
phần đó rất nhỏ. Sau khi đổi: dốc trung bình 4,2°, chỉ **0,25%** số ô vượt 18°.

Nhờ nâng hạ nguyên khối mà **mặt nước chỉ cần dịch theo đúng một đoạn** — nó nằm chung
danh sách với đồ đạc. Độ sâu lưu trong màu đỉnh của lưới nước vẫn khớp: đo 11 vũng, lệch
lớn nhất **0,0000 m**.

#### Đồ đạc phải đi theo đất

452 bia mộ, 229 tảng đá, 58 cây, 5 nhà mồ, 70 bụi cỏ và người chơi đang cắm sẵn vào mặt
đất cũ. Mỗi vật được dời đúng bằng phần đất dưới chân nó đã dịch, nên độ chôn xuống đất
giữ nguyên từng con số:

| Nhóm | Trước | Sau |
|---|---|---|
| Bia mộ | −0,168 / −0,089 / −0,021 | y hệt |
| Đá | −0,141 / −0,086 / −0,030 | y hệt |
| Cây | −0,252 / −0,250 / −0,248 | y hệt |
| Nhà mồ | −0,101 / −0,100 / −0,100 | y hệt |

Chạy thử 15 giây trong Play mode: người chơi `isGrounded = True`, cách đất 0,085 m; 32
sinh vật, không con nào cách đất quá 0,5 m.

#### Sửa độ cao thôi là chưa đủ — vật liệu mới là thứ mắt đọc được

Sau khi hình học đã khớp Act1, chụp lại cùng một góc máy thì độ tương phản của mặt đất
**gần như không đổi** (5,92 → 6,43). Dưới ánh trăng, một sườn dốc 4° chỉ đổi được chừng
**3 mức xám** — mắt không đọc ra nổi.

Thứ làm Act1 nhìn ra địa hình là **vật liệu đổi màu theo chỗ cao chỗ thấp**. Act2 thì 81%
bản đồ là một lớp cỏ chết duy nhất, nên gò nào cũng đúng một màu. `ToLaiTheoDiaHinh` sửa
hai việc:

- **Sườn dốc lộ sỏi đá** — ngưỡng 5° chứ không phải 9°: địa hình này dốc trung bình có
  4,2°, để ngưỡng 9 thì gần như không chỗ nào được tô. Tương quan sỏi ↔ độ dốc: **0,956**.
- **Đỉnh gò trơ rơm khô sáng, lòng trũng đọng đất ẩm tối** — đo "cao hơn hay thấp hơn *vùng
  xung quanh* trong bán kính 7 m", không phải so với độ cao tuyệt đối: một chỗ ở lưng chừng
  sườn đồi vẫn có thể là đỉnh gò của riêng nó. Lớp cỏ chết ở đỉnh gò **0,798**, ở lòng
  trũng **0,447**.

Hai lớp cỏ chết và đất trọc chỉ **đổi chỗ cho nhau**, giữ nguyên tổng của chúng — nên vùng
đất trọc quanh 452 ngôi mộ và lớp bùn của đầm lầy không bị vẽ đè lên.

> ⚠️ Đừng so độ tương phản ảnh giữa Act1 và Act2 để kết luận. Con số 10,10 của Act1 ở góc
> chơi là do **lối mòn sáng màu và mấy ngọn đèn lửa**, không phải do đất gồ ghề. Phép đo
> hợp lệ là hình học, và dải chân trời ở góc máy thấp — chỗ chỉ có đường viền đất: Act2 đi
> từ 9,99 lên **10,33**, Act1 là 9,95.

### Đầm lầy và hồ nước

[Act2DamLay.cs](Assets/Editor/Act2DamLay.cs) đào **1 hồ ở trung tâm** (bán kính 5,4 m, sâu
0,92 m) và **10 bãi đầm lầy** rải quanh (bán kính 4,2–7,6 m, sâu 0,26–0,44 m). Chạy sau khi
địa hình đã dựng và đồ đạc đã đặt, vì nó cần biết cây và nhà mồ đang ở đâu để khỏi đào vào.

Bia mộ và đá thì **không tránh** — nghĩa địa ngập nước, bia chìm nửa thân trong bùn là đúng
không khí. Chúng được hạ xuống theo lòng chảo ở bước `HaDoDac`.

**Người chơi lội qua được.** Mặt nước không có va chạm nào; đáy chảo vẫn là terrain collider
như cũ. Chỗ đứng đầu màn được dời ra bờ nam `(0, 3, −10,5)` — trước đó là `(0, 3, 0)`, tức
đúng giữa hồ, vào game là thấy mình đang dầm dưới nước.

#### Ba chỗ dễ sai, đều đã vấp

**1. Không còn chỗ để đào xuống.** Điểm thấp nhất của địa hình đã là 0,0005 — sát đáy tuyệt
đối của dải cao độ. Phải nới `size.y` ra rồi **hạ cả terrain xuống đúng bằng phần vừa nới**,
để bề mặt không xê dịch một ly:

```
Điểm thế giới cũ:   y = viTri.y + h × cao
Nới thêm d:         h' = (h×cao + d) / (cao+d),  viTri.y' = viTri.y − d
                    y' = viTri.y − d + h×cao + d = y   ✓
```

Đo lại sau khi nới: chỗ không đào lệch **0,0000 m**, và 452 bia mộ vẫn lệch trung bình
**0,089 m** — đúng bằng con số trước khi có đầm lầy.

**2. `TerrainData` là file, `transform.position` nằm trong cảnh.** Lần đầu tôi chạy đào trực
tiếp trên cảnh đang mở rồi lưu. Heightmap (file tài sản) lưu được, nhưng vị trí terrain và
các vật mặt nước (nằm trong cảnh) thì không — kết quả là **mặt đất cao hơn đồ đạc đúng
1,27 m**, cả bản đồ bia mộ chìm nghỉm. Nên `Act2DamLay` được gọi từ trong `Act2Baker`, dựng
một lượt cùng cảnh, chứ không sửa cảnh có sẵn.

**3. Mép nước phải cắt theo BỜ THẬT, không theo bán kính.** Mực nước thấp hơn mép chảo, nên
bờ thật luôn nằm bên trong mép chảo. Cắt theo bán kính hình học thì rìa nước thò ra ngoài,
nhìn như tấm kính đặt đè lên mặt đất. Nay với mỗi trong 64 hướng, code dò dần ra từ tâm bằng
`SampleHeight` cho tới khi đáy chạm mực nước — đó mới là bờ.

#### Nước: độ sâu nướng vào màu đỉnh

Shader [`Diablo25D/NuocDam`](Assets/Shaders/S_NuocDam.shader) làm nước đục dần theo độ sâu:
chỗ nông lộ đáy bùn, chỗ sâu đục hẳn. Độ sâu **được nướng sẵn vào kênh đỏ của màu đỉnh** lúc
dựng lưới, đo bằng `SampleHeight` trên terrain đã đào xong.

Ban đầu tôi đọc `_CameraDepthTexture` cho tiện — nhưng trong Built-in RP camera **không tự
sinh** bộ đệm độ sâu, thiếu nó thì độ đục ra 0 và cả mặt nước trong suốt như không có gì.
Nướng sẵn vào lưới thì chạy ở đâu cũng đúng, không phụ thuộc thiết lập camera.

#### Sóng, óng ánh, bọt — và lội nước

Mặt nước làm bằng năm lớp, tất cả nằm trong
[`Diablo25D/NuocDam`](Assets/Shaders/S_NuocDam.shader):

| Lớp | Làm gì |
|---|---|
| **Sóng** | Dịch hẳn đỉnh lưới lên xuống, ba nhịp sin chồng nhau |
| **Gợn** | Hai lớp nhiễu trôi **ngược chiều** nhau, khác nhịp |
| **Óng ánh** | Nhiễu tần số cao nâng luỹ thừa 14 → còn vài đốm, trượt theo mặt sóng |
| **Bờ động** | Mép nước tràn ra rút vào theo sóng, bọt bám theo |
| **Đục theo sâu** | Chỗ nông lộ đáy bùn, chỗ sâu đục hẳn |

#### Mép nước không được đứng yên

Lần đầu tôi cắt lưới đúng ở bờ tĩnh và lấy độ sâu cố định. Kết quả là quanh mỗi
vũng nước luôn có **một vòng viền sáng đứng im như nét kẻ bằng bút** — nhìn ra
miếng dán chứ không ra nước.

Cách chữa gồm ba phần ăn khớp nhau:

1. **Lưới thò ra ngoài bờ tĩnh.** Cắt tại chỗ đất cao hơn mặt nước `TranToiDa`
   = 0,14 m, chứ không cắt đúng ở mặt nước. Đo lại: lưới rộng 6,47 m trong khi bờ
   tĩnh chỉ 4,30 m — thò ra **2,17 m** để lấy chỗ cho sóng tràn.

2. **Độ sâu lưu trong màu đỉnh có cả số âm.** Chỗ đất cao hơn mặt nước thì độ sâu
   âm. Màu đỉnh chỉ chứa được 0..1 nên phải đẩy lên `LechAm` = 0,25 m rồi chia
   `ThangSau` = 1,5 m; shader trừ và nhân lại. Đo lại: độ sâu giải mã chạy từ
   **−0,140 đến 0,745 m**, có **217/721 đỉnh mang số âm**.

3. **Shader cộng sóng vào độ sâu đó.** Sóng lên thì chỗ đất hơi cao cũng thành có
   nước (tràn ra), sóng rút thì lui vào. Mép được cắt **mềm** (`_MemBo`) chứ không
   cắt thẳng, không thì lại ra đúng cái viền sắc cạnh như cũ.

> ⚠️ `TranToiDa`, `LechAm`, `ThangSau` trong `Act2DamLay` **phải khớp** với
> `_LechAm` / `_ThangSau` trong shader. Lệch là cả đường bờ xê dịch đi một đoạn.

Vì lưới thò ra ngoài, phải giữ **hai mảng bờ khác nhau**: `boVe` (rộng, cho lưới)
và `boLoi` (bờ tĩnh, cho `VungNuoc`). Lấy `boVe` cho `VungNuoc` thì người chơi sẽ
"lội nước" ở cả những chỗ đang khô chân.

**Rìa ngoài cùng của lưới không bao giờ lộ ra**, vì `TranToiDa` (0,14 m) lớn hơn
biên độ sóng (0,085 m) — dù sóng đang ở đỉnh thì rìa lưới vẫn nằm dưới mặt đất.

**Biên độ sóng thực nhỏ hơn `_CaoSong`.** Ba nhịp sin triệt tiêu bớt nhau: để
`_CaoSong` 8,5 cm thì đo tại một điểm bất kỳ chỉ thấy lên xuống khoảng ±2 cm.

#### Lội nước

[LoiNuoc.cs](Assets/Scripts/Vfx/LoiNuoc.cs) tự gắn vào người chơi và mọi con quái
trong `Awake`. Mỗi khung hình nó hỏi [VungNuoc.cs](Assets/Scripts/Vfx/VungNuoc.cs)
xem chân đang ngập bao nhiêu, rồi lo bốn việc:

1. **Đi chậm lại** — ngập sâu nhất thì còn 55% tốc độ
2. **Toé nước** — lúc vừa bước xuống, bọt bay tung một cái
3. **Gợn sóng** — đang lội mà còn đi thì chân để lại vòng gợn lan ra
4. **Bọt quanh chân** — bọt trắng li ti chỗ chân khuấy nước

Ba thứ 2–4 chỉ sinh ra khi **có di chuyển thật sự**; đứng yên mà vẫn phun bọt thì
nhìn như nhân vật đang sôi nước. Vòng gợn phun ở **không gian thế giới** — phun
theo vật thể thì vòng gợn đi theo chân người chơi, mà gợn thật phải đứng yên tại
chỗ rồi loang ra.

**Không có collider trên mặt nước**, nên `VungNuoc` giữ sẵn mực nước và mảng bờ
rồi trả lời thẳng "ngập bao nhiêu mét". Mảng bờ đó **dùng chung** với lưới vẽ —
hai cái lệch nhau thì người chơi lội ở chỗ không có nước, hoặc đi khô chân ngay
giữa hồ.

Đo lại sau khi dựng:

| Chỗ đứng | Chân ở | Ngập |
|---|---|---|
| Giữa hồ | −1,08 m | **0,74 m** |
| Trong hồ, cách tâm 2,5 m | −0,69 m | 0,35 m |
| Sát bờ (3,6 m) | −0,31 m | 0 |
| Đất khô | −0,30 m | 0 |
| Một bãi đầm lầy | −0,56 m | 0,20 m |

> ⚠️ **Hai thứ không kiểm được ngoài Play mode.** `_Time` của shader không tăng khi
> Editor không chạy, nên chụp hai ảnh cách nhau vẫn ra **giống hệt nhau** — đừng vội
> kết luận nước đứng yên. Và `OnEnable` không chạy, nên danh sách vùng nước rỗng và
> `VungNuoc.DoNgap` trả 0 ở mọi điểm, nhìn y như code hỏng. Muốn thử ngoài Play mode
> thì gọi `NgapTai` trên từng vùng, đừng gọi bản `DoNgap` static.

> ⚠️ **Chụp ảnh thì dùng đúng ánh sáng của cảnh.** Tôi thêm đèn phụ cường độ 1,0
> cho dễ nhìn, rồi thấy một vệt trắng chói trên mặt nước và tưởng óng ánh quá mạnh —
> chỉnh mãi không hết. Chụp lại bằng ánh sáng thật của cảnh (Moonlight 0,62) thì đếm
> được **0 điểm gần trắng**: vệt đó là phản chiếu của chính cái đèn tôi thêm vào.

> ⚠️ **Đừng kiểm tra vật thể tĩnh bằng cách đắp vật liệu màu vào.** Mọi thứ dưới `World` đều
> là static nên Unity đã gộp lưới thành một mẻ chung; thay `sharedMaterial` trên một renderer
> đã gộp mẻ thì nó vẽ sai hoặc không vẽ. Tôi đếm được **90 điểm đỏ** và tưởng mặt nước hỏng,
> trong khi ảnh chụp bình thường cho thấy hồ nước hiện ra đầy đủ. Cứ nhìn ảnh thật.

### Cây — hai lỗi phải chữa ở Unity, không sửa file Blender

[Act2Cay.cs](Assets/Editor/Act2Cay.cs) chữa hai lỗi của cây trong bản đồ. Chữa bên Unity
chứ không sửa file Blender của bạn, để bạn xuất lại bản đồ lúc nào cũng được.

**Lỗi 1 — lá lơ lửng không có cành để bám.** Đo được: 47% số đỉnh lá nằm cách miệng vỏ cây
gần nhất **hơn 0,8 m**, xa nhất tới **2,60 m**. Cây trong Blender chỉ có những cành to; nhánh
con — thứ thật sự giữ chùm lá — không được sinh ra.

Cách chữa: gom lá thành từng chùm (ô 1,5 m), rồi mọc một nhánh con thon từ vỏ cây ra tới
chùm lá đó. Nhánh **không nối thẳng về thân** mà dùng cây khung nhỏ nhất: mỗi vòng chọn chùm
nào gần chỗ đã có nhánh nhất rồi nối vào đúng chỗ đó — có thể là vỏ cây, cũng có thể là một
chùm đã mọc trước. Nhánh to đẻ nhánh nhỏ, đúng như cây thật.

> Lần đầu tôi nối thẳng từng chùm về vỏ cây gần nhất, ngưỡng 0,35 m, ô chùm 0,7 m. Kết quả:
> một cây ra 1000–2000 nhánh, mỗi chùm ở rìa tán thành một cái que dài đâm thẳng từ thân ra —
> cả cây hóa **con nhím**. Nới ngưỡng lên 0,55 m và ô chùm lên 1,5 m thì còn 941 nhánh cho cả
> bốn loại cây, dài trung bình 0,50 m, dài nhất 1,18 m.

| Loại cây | Số nhánh mọc thêm | Đỉnh sau khi mọc |
|---|---|---|
| `TREE_oakA` | 234 | 103.596 |
| `TREE_oakB` | 365 | 128.190 |
| `TREE_oakC` | 211 | 61.836 |
| `TREE_thin` | 131 | 49.763 |

Năm lưới `_bare` và `_dead` **không có miếng lá** nên không mọc nhánh nào — đúng, vì chúng
vốn là cây trụi.

**Lỗi 2 — thân cây trơn lu một màu.** Cả chín lưới cây đều **không có toạ độ ảnh (UV)**, nên
mọi ảnh vỏ cây đắp vào đều chỉ lấy đúng một điểm màu. Vì không có UV nên cũng **không gọi được
`RecalculateTangents`**.

Cách chữa là shader [`Diablo25D/BarkTriplanar`](Assets/Shaders/S_BarkTriplanar.shader): nó
không hỏi lưới xem lấy ảnh ở đâu mà tự tính theo vị trí trong không gian, chiếu ảnh từ ba
hướng X/Y/Z rồi trộn theo hướng mặt phẳng — chạy được trên mọi lưới, kể cả lưới không có UV.
Chiều dọc được kéo dãn (`_KeoDoc`) vì vân vỏ cây thật chạy dọc thân, không phải ô vuông.

Ảnh vỏ cây được vẽ bằng code: vân dọc, rãnh nứt sâu, nứt ngang cắt qua, đốm rêu. **Kênh alpha
đánh dấu chỗ có nhựa cây**, shader đọc kênh đó để bôi màu nhựa hổ phách và làm bóng hẳn lên —
nhựa chỉ rỉ trong rãnh nứt, không bao giờ bám trên chỗ lồi. Màu nền `(0,115 · 0,135 · 0,100)`
là đen ngả xanh rêu.

> Ba cái mốc lọc nhựa lúc đầu tôi để 0,70 và 0,42 rồi còn nhân thêm với vết nứt. Nhiễu Perlin
> của Unity thực tế chỉ chạy quanh 0,25–0,75 chứ không trải đều 0–1, nên ba cái cổng nhân vào
> nhau làm cả tấm ảnh 512×512 có alpha cao nhất **0,055** — tức **không có giọt nhựa nào**.
> Hạ mốc xuống 0,48 và 0,34 thì độ phủ lên **1,71% đậm / 5,78% nhạt**, đúng nghĩa "có ít nhựa".

**Ghi lưới: phải ghi thẳng vào chính đối tượng cũ.** Hai cách làm sai:

1. *Xoá file rồi tạo lại* — Unity cấp mã GUID mới, mà từng gốc cây ngoài cảnh chỉ nhớ lưới
   theo GUID, nên cả 58 gốc cây mất lưới.
2. *Tạo lưới mới rồi `EditorUtility.CopySerialized` đè lên file cũ* — đọc lại bằng code thì
   số liệu **đúng y hệt** (đỉnh, tam giác, miếng, hộp bao đều khớp tuyệt đối), nhưng bản nằm
   trên card đồ hoạ không được dựng lại, nên vẽ ra **mất trắng miếng lá** — cả cây trụi lủi.
   Mọi phép đo đều báo "giống hệt" nên rất khó lần ra.

Cách đúng: lấy đúng đối tượng cũ ra, `Clear()` rồi đổ dữ liệu mới vào, và nhớ chép lại
**tiếp tuyến** của lưới gốc (không có UV thì không tính lại được).

### Va chạm — mỗi loại một kiểu

| Loại | Va chạm | Vì sao |
|---|---|---|
| Mặt đất | **Terrain collider**, lớp `Ground` | `VfxFactory.GroundY` bắn tia xuống hỏi độ cao; sai lớp là cả đàn quái rơi tự do |
| Hàng rào, nhà mồ | MeshCollider | ít vật, cần chặn chính xác |
| Bia mộ, đá | BoxCollider ôm khít | rẻ hơn nhiều, người chơi không phân biệt được |
| Cây | CapsuleCollider ôm thân | một cây hơn 100 nghìn đỉnh — MeshCollider cho 58 cây sẽ nặng hơn cả màn chơi |

### Vật liệu

Blender dùng vật liệu thủ tục (node), mà FBX **không mang được node nào** — nhập sang
Unity thì tất cả về xám trơn. Nên vật liệu được dựng lại bằng tay trong `Act2Baker`,
khớp tông với Act1: đất nâu tối, đá xám lạnh, lá nâu úa. Muốn đổi màu thì sửa trong
`LamVatLieu()` rồi chạy lại mục 9.

### Muốn đưa bản đồ Blender khác vào

Xuất theo đúng hai file như trên (script xuất nằm trong lịch sử phiên làm việc), bỏ vào
`Assets/BlenderMaps/<tên>`, rồi sửa `ThuMucBanDo` trong
[Act2Baker.cs](Assets/Editor/Act2Baker.cs). Điều kiện: mặt đất phải là một vật thể riêng,
và các bản sao phải **dùng chung lưới** (trong Blender là Alt+D chứ không phải Shift+D).

---

## Ô vật liệu của hệ hạt: đừng bao giờ gán cả mảng `sharedMaterials`

Bốn prefab hiệu ứng vẽ ra một **mảng hồng cánh sen** thay vì khói. Lỗi nằm im từ **22/08**
đến khi tình cờ lọt vào một ảnh chụp kiểm chứng — vì nó chỉ loè lên trong khoảnh khắc nổ,
và trong lúc chơi thì mắt còn đang nhìn chỗ khác.

| Prefab | Hệ hạt hỏng |
|---|---|
| `Vfx_NoLua` | `Smoke` |
| `Vfx_SetChamDat` | `Dust` |
| `Skill_QuaCauLua` | `Smoke` |
| `Vfx_VungGiong` | `Cloud` |

### Vì sao tìm mãi không ra

Mọi dấu hiệu đều nói "không có gì sai":

```
file .prefab       ghi DUNG guid cua M_P_Smoke.mat, dung 1 phan tu
M_P_Smoke.mat      nap duoc, shader Diablo25D/ParticleAlpha, hop le
ban dung TUOI      bang code: 'Smoke' -> P_Smoke, 1 o vat lieu
prefab doc ra      'Smoke' -> o thu 0 = NULL
```

Cái bẫy nằm ở `AssetBaker.RemapMaterials`:

```csharp
var mats = r.sharedMaterials;      // <- voi he hat, mang nay CO CA trailMaterial
...
r.sharedMaterials = mats;          // <- gan nguyen mang tro lai
```

Với `ParticleSystemRenderer`, `sharedMaterials` **không phải** danh sách ô vật liệu bình
thường: Unity nhét cả `trailMaterial` vào trong đó. Gán nguyên mảng trở lại thì hai ô bị
trộn lên nhau, và cái rơi ra ngoài là **ô chính**.

Nay hệ hạt đi một nhánh riêng, đặt **từng ô một**:

```csharp
psr.sharedMaterial = BakeMaterial(psr.sharedMaterial);
if (psr.trailMaterial != null) psr.trailMaterial = BakeMaterial(psr.trailMaterial);
```

### Vá bốn prefab: không đoán, mà đối chiếu với bản dựng tươi

Cách vá không phải là "chắc là `M_P_Smoke`". Với mỗi prefab, dựng lại **bản tươi bằng chính
code đã sinh ra nó** (`BuildFireExplosion`, `BuildLightningImpact`, `BuildFireballVisual`,
`BuildLightningStormField`), đọc tên vật liệu **theo tên vật thể**, rồi gán vào ô rỗng.
Trùng tên thì mới gán; không trùng thì báo lỗi chứ không đoán bừa.

```
Vfx_NoLua        / 'Smoke' <- M_P_Smoke.mat
Vfx_SetChamDat   / 'Dust'  <- M_P_Smoke.mat
Skill_QuaCauLua  / 'Smoke' <- M_P_Smoke.mat
Vfx_VungGiong    / 'Cloud' <- M_P_StormCloud.mat

quet lai 62 prefab, 1233 renderer  ->  0 o vat lieu hong
chup lai vu no lua trong Play      ->  0 diem anh magenta
```

> Phép chụp phải **tua hệ hạt** bằng `ps.Simulate(0.8f, ...)` rồi `cam.Render()` **trong
> cùng một lệnh**. Cho nổ ở lệnh này rồi chụp ở lệnh sau thì hiệu ứng đã tan từ lâu — mỗi
> lệnh MCP cách nhau vài giây, mà vụ nổ chỉ sống hơn một giây.

## Shader chỉ nạp lúc chạy phải khai vào Always Included

Trong Editor mọi shader đều tìm được, nên **lỗi này không bao giờ lộ ra ở Play Mode.** Nó
chỉ hiện trên bản build.

Unity đưa một shader vào build khi có **material asset** trỏ tới nó, hoặc nó nằm trong
**Resources**, hoặc nó được khai trong **Project Settings › Graphics › Always Included
Shaders**. Dự án này tạo gần như toàn bộ material lúc chạy (`MaterialLibrary`,
`VfxFactory`), nên phần lớn shader không có material asset nào trỏ tới — Unity loại chúng
khỏi build và `Shader.Find` trả `null`.

Rà lại thì có **bốn** shader không được asset nào tham chiếu, tức bốn shader hỏng trên
WebGL:

| Shader | Hậu quả trên bản build |
|---|---|
| `Diablo25D/Khieng` | vòm khiêng ra **quả cầu lửa** |
| `Diablo25D/FrozenShell` | vỏ băng lúc đóng băng kẻ địch |
| `Diablo25D/Bloom` | hậu kỳ loé sáng |
| `Diablo25D/ParticleFlipbookAlpha` | khói flipbook |

Giờ **cả 15 shader tuỳ biến** đều nằm trong Always Included. Không chừa mấy cái "đang có
material asset trỏ tới" — cái đó là ngẫu nhiên, xoá một material là shader lại rơi khỏi
build.

#### Fallback không được là một hiệu ứng trông có thật

Chỗ này ban đầu viết:

```csharp
if (sh == null) { Debug.LogError(...); return Mats.FireSoft; }
```

`Mats.FireSoft` là vật liệu **lửa**. Nên khi shader biến mất, cái vòm khiêng không vỡ,
không magenta, không báo gì trên màn hình — nó ra một **quả cầu lửa vàng cam cuộn xoáy**,
trông y như một hiệu ứng được thiết kế đàng hoàng. Đi tìm nguyên nhân thì soi vào màu sắc,
vào số liệu, vào shader Khiêng — trong khi gốc rễ là shader **không hề có trong build**.

Đã đổi sang **magenta**. Nhìn một cái là biết phải đi tìm shader.

#### Cách kiểm chứng

Đoán "chắc là thiếu shader" thì chưa đủ. Phép kiểm là **dựng lại đúng tình huống lỗi**:
vào Play Mode, dựng vòm khiêng thật rồi ép `sharedMaterial = Mats.FireSoft`, chụp ảnh, đặt
cạnh ảnh chụp WebGL. Hai ảnh trùng nhau tới từng chi tiết — mép dưới cắt phẳng theo mặt
đất, quầng trắng ở đỉnh vòm, chấm sáng lấm tấm. Lúc đó mới gọi là xong.

Và kiểm tra thêm rằng shader **biên dịch được** cho nền đích, vì "có trong build" với "chạy
được" là hai chuyện:

```csharp
var pass = ShaderUtil.GetShaderData(sh).GetSubshader(0).GetPass(0);
var info = pass.CompileVariant(ShaderType.Fragment, new string[0],
                               ShaderCompilerPlatform.GLES3x, BuildTarget.WebGL);
// info.Success, info.Messages
```

Cả bốn shader đều `Success = true`, không một cảnh báo — nên vấn đề đúng là ở chỗ vào
build, không phải ở chỗ biên dịch.

## Thanh kỹ năng trên PC: bỏ tên kỹ năng, và hai lần đo sai đường

**Hiện tượng.** Dưới bảy ô kỹ năng ở đáy màn hình có bảy dòng chữ
`[1/Z] QUA CAU LUA`, `[2/X] MUA BANG`… Ô kỹ năng rộng 44 px (màn 571 px cao),
mà dòng chữ đầy đủ cần 80 px — nên `GUI.Label` tự ngắt dòng, dòng thứ hai rơi
khỏi mép dưới màn hình và bị cắt cụt. Người chơi nhìn thấy `[1/Z] QUA`,
`[2/X] MUA` — chữ vừa thừa vừa không đọc được.

**Cách sửa.** Trong [`UI/GameHUD.cs`](Assets/Scripts/UI/GameHUD.cs) hàm
`DrawSkillSlot` chỉ vẽ phím tắt, bỏ hẳn tham số `label`:

```csharp
GUI.Label(new Rect(r.x, r.y + r.height + 2f * s, r.width, 22f * s),
          "[" + key + "]", keyStyle);
```

Chỉ ảnh hưởng bản PC: `GameHUD.OnGUI` rẽ nhánh `if (CamUng.DangDung)
{ VeNutKyNangTron(s); … } else DrawSkillBar(s);` — bản cảm ứng đi lối khác, và
lối đó vốn chỉ vẽ biểu tượng, chưa bao giờ vẽ chữ.

**Số đo chứng minh.** Kịch bản mới **menu 22** chụp cả HUD bằng `ScreenCapture`
(không dùng `cam.Render()` — OnGUI không đi vào RenderTexture), rồi đếm pixel màu
chữ trong dải ngay dưới hàng ô. Bản trước khi sửa được giữ lại làm đối chứng, đo
đúng cùng một cách, trên cùng màn Act2:

| Ô | Bản cũ — cụm chữ rộng | Bản mới — cụm chữ rộng |
|---|---|---|
| 1 | 38 px | **14 px** |
| 2 | 37 px | **15 px** |
| 3 | 36 px | **15 px** |
| 4 | 38 px | **15 px** |
| 5 | 18 px | **16 px** |
| 6 | 16 px | **19 px** |
| 7 | 36 px | **19 px** |
| **Tổng pixel chữ** | **302** | **175** |

Bề rộng lý thuyết do chính `keyStyle` của HUD trả về: `"[1/Z]"` = **19 px**,
`"[1/Z] QUA CAU LUA"` = **80 px**. Bản mới có **cả bảy** cụm ≤ 19 px — đúng bằng
phím tắt, không hơn một chữ nào. Bản cũ có bốn ô 36–38 px, gần gấp đôi.
Ảnh: `PlayTestShots/thanh_ky_nang_pc.png` (mới) và `thanh_ky_nang_pc_CU.png` (cũ).

**Hai lần đi sai đường — đều ở phép đo, không ở code.**

*Lần một: chép hằng số thay vì hỏi.* Kịch bản đo tự đặt `const float Ref = 720f`
để tính vị trí ô, trong khi `GameHUD` dùng `Ref = 1080f`. Mỗi ô bị tính rộng
66 px thay vì 44 px, cả bảy vùng quét trượt sang trái, dải quét lấn lên viền ô —
và viền vàng của ô đang chọn lọt vào bộ lọc "màu chữ". Kết quả: ô 1 và ô 7 báo
**0 pixel chữ** trong khi ảnh rành rành có `[1/Z]` và `[7/G]`, còn ô 3 báo cụm
rộng 63 px — rộng hơn cả chuỗi cũ. Suýt nữa thì báo là code hỏng. Nay kịch bản
đọc `Ref` thẳng từ `GameHUD` bằng reflection: nó sai thì mình sai theo, không
lệch được.

*Lần hai: đo nhiễu rồi tưởng là đo chữ.* Phép kiểm "bản cảm ứng không có chữ"
đếm pixel màu chữ trên toàn bề ngang, ra **87** ở lần chạy này và **4176** ở lần
chạy sau — trên đúng một thứ. Nhìn ảnh mới thấy: lần sau có một quả thiên thạch
nổ sáng rực giữa màn hình, và bộ lọc "vàng nhạt" đếm luôn đám lửa. Phép đo ấy đã
bị bỏ: điều cần biết đã nằm sẵn ở nhánh `if (CamUng.DangDung)`, còn ảnh chụp thì
để người đọc tự nhìn.

Bài học chung của cả hai: **một con số không có đối chứng thì không nói được gì.**
Chỉ khi đặt bản cũ và bản mới cạnh nhau, đo cùng một dải bằng cùng một bộ lọc,
phần nhiễu mới triệt tiêu và phần chênh lệch mới là thật.

## Đưa dự án vào git — và vì sao 920 MB model bị bỏ lại ngoài

**Hiện tượng.** Suốt từ đầu dự án không có chỗ nào để lùi lại. Sửa hỏng một file
`.cs` hay lỡ bấm nhầm "1. Nuong Asset" (mục này *xóa và tạo lại* cả bốn thư mục
Textures / Materials / Models / Prefabs) thì không có cách nào lấy lại bản cũ —
`HUONG-DAN.md` kể được **vì sao** đã làm, nhưng không giữ được **chính cái file** đó.

**Cách làm.** `git init -b main`, nhánh `main`, ảnh chụp đầu tiên `0ef3e18`
(05/09/2026). Người ghi commit đặt ở mức repo (`git config user.email`), không đụng
tới cấu hình chung của máy. `core.autocrlf = false` — Unity ghi file gì thì git giữ
đúng byte đó, không tự đổi xuống dòng, tránh cảnh cả nghìn file `.meta` "thay đổi"
giả sau một lần checkout.

**Cái gì không vào git.** File Unity tự sinh lại được thì không giữ:
`Library/` (bộ nhớ đệm nhập asset), `Temp/`, `Logs/`, `build/`, `UserSettings/`,
`.utmp/`, cùng file dự án của Visual Studio / Rider (`*.csproj`, `*.sln` — Unity tự
sinh lại mỗi lần mở).

Riêng **`Assets/MeshyImports/` (920 MB) là quyết định có cân nhắc**, không phải bỏ sót:

| | Vào git | Để ngoài (đã chọn) |
|---|---|---|
| `.git` phình thêm | ~0,9 GB (FBX/PNG gần như không nén được) | 0 |
| Lỡ xoá thư mục | git lấy lại được | phải tải lại từ Meshy |
| Unity dùng model | bình thường | **bình thường** — file vẫn nằm nguyên trên đĩa |

Đó là 5 model nhân vật tải về từ Meshy, mỗi bộ 164–212 MB, **không bao giờ sửa** —
chúng chỉ là nguyên liệu cho menu 11 / 12 nướng ra prefab `Enemy_QuyDu` /
`Enemy_QuyCay`. Trả giá 0,9 GB dung lượng để giữ lịch sử cho những file không có
lịch sử là không đáng. Điều phải nhớ: **git không cứu được thư mục này** — lỡ xoá thì
tải lại từ Meshy.

**Số đo chứng minh.**

| Đo | Số |
|---|---|
| File được theo dõi | **1175** |
| Dung lượng `.git` | **78 MB** (so với 1,1 GB của `Assets/`) |
| File lớn hơn 20 MB lọt vào commit | **0** |
| `git status` sau khi commit | **sạch** — 0 file thay đổi |
| `git fsck` | **0 lỗi**, không một cảnh báo |
| Lọt `Library` / `Temp` / `MeshyImports` vào commit | **0 file** (lọc lại bằng grep sau khi stage) |

Phân bố 1175 file đó: Materials 273, Scripts 145, Prefabs 124, Models 120,
Textures 78, Resources 75, Terrain 66, Meshes 62, Editor 44, Shaders 34,
PlayTestShots 34, BlenderMaps 32, ProjectSettings 25 — tức là **toàn bộ phần dựng
nên game**, chỉ thiếu nguyên liệu thô của Meshy.

**Muốn lùi lại thì làm gì.**

```bash
git status              # dang sua nhung gi
git diff                # xem tung dong da doi
git checkout -- <file>  # tra rieng mot file ve ban da commit
git log --oneline       # danh sach cac lan chup
```

## Nhiều người chơi, giai đoạn 1: tài khoản, sảnh phòng, trang quản trị

### Việc muốn làm

Nhiều người tạo tài khoản, đăng nhập, một người lập phòng (tối đa 4) và chọn Act1 hoặc Act2,
người khác vào phòng bấm sẵn sàng, chủ phòng bấm bắt đầu, cả phòng cùng đếm ngược 10 giây rồi
cùng nhảy vào màn. Kèm một chỗ cho admin quản lý toàn bộ tài khoản người chơi.

Giai đoạn 1 chỉ làm **phần nền**: mỗi người vẫn chơi bản riêng của mình sau khi đếm ngược xong —
chưa nhìn thấy nhau trong trận. Phần nhìn thấy nhau là giai đoạn 2.

### Lần đi sai đường lớn nhất: làm sảnh phòng trên trang web

Tôi hiểu sai và đã làm xong một trang web đầy đủ: `index.html` đăng nhập, `sanh.html` danh sách
phòng, `choi.html` nhúng game, `js/phong.js` lo phòng và đếm ngược. Nó **chạy được** — hai phiên
Firebase độc lập trong cùng một tab đếm ngược khớp nhau **lệch 0 giây** dù đồng hồ hai bên lệch
545 ms.

Nhưng người dùng muốn khác: đăng ký, đăng nhập, tạo phòng, vào phòng **đều ở trong game Unity**,
màn hình MainMenu có sẵn rồi. Trang web **chỉ còn một việc**: cho admin quản lý tài khoản.

Kết quả: xoá `sanh.html`, `choi.html`, `js/phong.js`; `index.html` viết lại thành trang đăng nhập
**cho admin**; toàn bộ luồng người chơi chuyển sang OnGUI trong Unity.

Bài học: 745 dòng JavaScript chạy đúng vẫn là 745 dòng bỏ đi, nếu nó nằm sai chỗ.

### Vì sao gọi Firebase bằng REST chứ không dùng Firebase Unity SDK

**Firebase Unity SDK không hỗ trợ WebGL.** Mà WebGL là nơi game sẽ chạy. Nên toàn bộ đi qua
**REST API + `UnityWebRequest`** (`Assets/Scripts/Mang/FirebaseMang.cs`): chạy được trong Editor,
bản PC lẫn bản WebGL, không thêm một thư viện nào vào bản build.

Cái mất: REST **không có luồng đẩy (streaming)**, nên sảnh phòng phải **hỏi lại theo nhịp** —
2,0 giây một lần khi xem danh sách, 1,0 giây một lần khi đang ở trong phòng.

Cũng vì lý do "không kéo thư viện vào bản build" mà JSON của Firestore được **tự tách bằng tay**
(`stringValue`, `integerValue`, `booleanValue`) thay vì nạp một thư viện JSON: `JsonUtility` của
Unity không đọc nổi kiểu lồng nhau đó, mà khoá phòng lại do máy chủ tự sinh (`-P0pIVxg...`) nên
cũng không khai trước thành trường được.

### Vì sao phòng ở Realtime Database còn hồ sơ ở Firestore

**Realtime Database có `onDisconnect()`, Firestore không có.** Người chơi tắt trình duyệt giữa
chừng thì phòng phải tự dọn — chỉ RTDB làm được. Ngược lại, trang quản trị cần **sắp xếp, lọc,
phân trang** hàng trăm tài khoản — đó là việc của Firestore.

Nên: `phong/`, `tran/` ở RTDB; `nguoichoi/`, `quantri/` ở Firestore. Cả hai đặt tại
`asia-southeast1`.

### Bốn lần vấp ở luật bảo mật

**1. `numChildren()` không tồn tại trong luật RTDB.** Chỉ Firestore mới có `size()`. Muốn chặn
phòng quá 4 người thì phải tự giữ một trường `soNguoi` và cộng trừ bằng giao dịch.

**2. Người vào phòng không tăng được `soNguoi`.** Luật ban đầu chỉ cho chủ phòng ghi vào node
phòng — thành ra người thứ hai vào phòng không sửa được số đếm. Phải mở riêng một `.write` cho
đúng trường `soNguoi`, và chỉ cho phép đổi **±1** (chủ phòng thì được đặt tuỳ ý).

**3. Tạo phòng bị từ chối mà không rõ vì sao.** Luật có `.write` cho *các con* của `tran/$maPhong`
nhưng **không có cho chính node đó**. Lúc tạo phòng, lệnh `onDisconnect().remove()` đặt lên node
`tran/$maPhong` bị từ chối, và Firebase đánh trượt **cả thao tác tạo phòng**. Thêm `.write` cho
chủ phòng ở node đó là xong.

**4. Không thể tự cấp quyền admin từ trình duyệt.** `quantri` để `allow write: if false` — đúng
như thiết kế. Cấp quyền phải bằng token chủ dự án (`gcloud auth print-access-token`) hoặc bằng tay
trong Firebase Console.

### Hai lần vấp khác

**Đua nhau tạo hồ sơ.** Lúc còn dùng web: luồng đăng ký và trang sảnh **cùng tạo** hồ sơ
`nguoichoi/{uid}`. Ai thua thì lệnh `setDoc` biến thành lệnh sửa và bị luật từ chối — trang treo
ở dấu "…" không báo gì. Sửa: bắt lỗi rồi đọc lại hồ sơ mà bên kia vừa tạo.

**Tên hiển thị biến mất** — đăng ký tên `ChienBinhA` mà vào sảnh lại thành `thunghiem.a.diab`.
`onAuthStateChanged` bắn ngay khi tài khoản vừa lập, chuyển trang **trước khi** tên kịp ghi. Sửa
bằng một cờ `dangTuXuLy` để lần chuyển trang tự động ngồi im khi luồng đăng ký đang chạy dở.

### Đếm ngược 10 giây khớp nhau dù đồng hồ hai máy lệch nhau

Không ai đếm bằng đồng hồ của mình. Chủ phòng ghi lên máy chủ **một mốc thời gian trong tương lai**;
mỗi máy tự đo **độ lệch đồng hồ của mình so với máy chủ** rồi trừ đi. Đo được:

- độ lệch đồng hồ giữa hai phiên: **545 ms** — mà đếm ngược hiện ra **lệch 0 giây**;
- đo lại trong Unity: sau **3 giây thật**, đồng hồ đếm ngược tụt **3,05 giây**.

### Kết quả đo — menu 26, chạy thật trên Firebase

Không giả lập: đúng tài khoản, đúng cơ sở dữ liệu mà người chơi sẽ dùng.

```
dang nhap B: OK (842 ms)
tai ho so: OK, ten = ChienBinhB, 0 thang / 0 tran (841 ms)
do dong ho may chu: 1600 ms
tao phong: OK (291 ms)
  ma phong = -P0pIVxgGbKIwFFswgBj, man = Act2, host = ChienBinhB, la host = True, so nguoi = 1
doc danh sach: 1 phong, co phong vua tao = True (135 ms)
doi man sang Act1 -> doc lai duoc: Act1
bat dau dem nguoc: trang thai = demNguoc, con lai 9.55 giay (461 ms)
sau 3 giay thuc: dem nguoc tut 3.05 giay (dung ra phai ~3,00)
dang nhap A: OK, ten = thunghiem.a.diab (1243 ms)
vao phong dang dem nguoc: bi tu choi - Phong nay da bat dau choi roi.
don phong chay thu: sach
tong thoi gian: 10.1 giay
so loi ghi nhan = 0
```

### Trang quản trị — và cái bảng rỗng không báo gì

Trang `admin/` liệt kê tài khoản (tên, email, ngày tạo, số trận, số thắng, số quái, số người đã
hạ), tìm theo email hoặc tên, **khoá / mở khoá**, xem phòng đang mở và giải tán phòng.

Lần đầu mở thì bảng **rỗng hoàn toàn**, ô đếm cũng trống, không một chữ báo lỗi. Nguyên nhân nằm
ở JavaScript: `await taiTrangDau()` được gọi **trước** dòng khai báo `let mocCuoi = null;`. Biến
khai bằng `let` rơi vào "vùng chết tạm thời", trình duyệt ném `ReferenceError: Cannot access
mocCuoi before initialization` — nhưng vì lỗi xảy ra trong một module nạp bất đồng bộ, **trang vẫn
hiện ra bình thường**, chỉ là không có dữ liệu. Chuyển hai dòng khai báo lên trước là xong.

Đây là kiểu lỗi khó thấy nhất: giao diện trông đúng, chỉ có nội dung là không có. Nếu chỉ nhìn ảnh
chụp mà không mở console thì sẽ tưởng "chưa có tài khoản nào".

Còn một cái bẫy nữa: sau khi sửa và `firebase deploy`, mở lại trang **vẫn rỗng**. Máy chủ đã có bản
mới (kiểm bằng `curl` thấy đúng thứ tự dòng) nhưng trình duyệt còn giữ bản cũ trong bộ nhớ đệm.
Thêm `?v=2` vào địa chỉ mới thấy kết quả thật.

Sau khi sửa, đo được: bảng hiện **2 tài khoản**; gõ `ChienBinhB` rồi bấm Tìm còn **1**.

### Kết quả đo — menu 27: khoá tài khoản trên web thì trong game có chặn không

Cái nút "Khoá" chỉ có nghĩa nếu game thật sự không cho vào. Khoá nằm ở Firestore
(`nguoichoi/{uid}.biKhoa`) chứ **không** ở Firebase Auth, nên bước đăng nhập vẫn qua — phải đến
bước tải hồ sơ mới bị chặn.

**Bản đầu của phép thử này sai, và sai theo kiểu khó thấy nhất.** Nó *giả định* tài khoản A đang
bị khoá sẵn (vì tôi vừa bấm khoá bằng tay trên trang admin). Chạy lần đầu: 0 lỗi, trông rất đẹp.
Sau khi mở khoá A rồi chạy lại, kết quả in ra **giống hệt từng chữ** — mà đúng ra phải khác. Hoá
ra Unity chưa biên dịch lại nên vẫn chạy assembly cũ. Thêm dấu `[ban N]` vào báo cáo mới lòi ra.

Sửa hai lần:
- Báo cáo luôn in `[ban N]`, đổi số mỗi lần sửa. Hai lần chạy ra số giống hệt nhau mà số bản
  không đổi thì **chưa chắc code đã chạy**.
- Phép thử **tự khoá rồi tự mở lại**, không đợi ai đặt sẵn trạng thái. Nó đo **ba lần** chứ không
  một: trước khi khoá phải vào được, sau khi khoá phải bị chặn, mở khoá xong phải vào lại được.
  Chỉ đo lần giữa thì không phân biệt được "khoá có tác dụng" với "tài khoản này vốn không vào
  được".

```
[ban 6] tu khoa roi tu mo lai; kiem ca nguoi thuong khong khoa duoc ai
1. truoc khi khoa, A vao game: VAO DUOC
1b. nguoi choi thuong (B) thu khoa A: bi tu choi - dung nhu mong doi
2. admin khoa A: OK
3. sau khi khoa, A vao game: bi chan - Tai khoan cua ban da bi khoa. Hay lien he quan tri vien.
   phien cua A con giu lai khong: da bo
4. admin mo khoa A: OK
5. sau khi mo khoa, A vao game: VAO DUOC
so loi ghi nhan = 0
```

### Quyền admin không được mượn của tài khoản người chơi

Lúc đầu tôi tiện tay cấp quyền admin cho **tài khoản thử nghiệm B**, vì phép thử cần một tài
khoản có quyền khoá người khác. Tiện, nhưng sai: một tài khoản chạy thử — mật khẩu yếu, nằm
trong file cấu hình, dùng chung cho mọi kịch bản — mà cầm quyền quản trị thì mất nó là mất cả
trang quản trị.

Giờ admin là **tài khoản riêng của chủ game**, khai ở dòng 4–5 của `chay-thu-mang.txt`. Chưa
khai thì menu 27 chạy phần đầu rồi dừng và nói rõ "BO QUA", chứ không báo lỗi giả.

Và phép thử có thêm **bước 1b**: một tài khoản người chơi bình thường thử khoá người khác —
phải **bị từ chối**. Bước này đo đúng cái ranh giới: nếu người chơi thường cũng khoá được người
khác thì luật bảo mật hỏng và cả trang quản trị thành vô nghĩa. Nó cũng chính là bằng chứng
rằng quyền admin đã thu hồi khỏi tài khoản B thật sự — chứ không phải chỉ xoá một dòng trong
cơ sở dữ liệu rồi tin là xong.

Bản tự chứa này lại lòi thêm một lỗi thật mà bản cũ giấu mất: **phiên của tài khoản bị khoá vẫn
được giữ lại**. Việc bỏ phiên khi đó nằm ở `ManDangNhap` — tức ở tầng giao diện — nên chỗ nào gọi
thẳng `HoSoMang` là lọt, và lần sau mở game lên là tự vào thẳng dù đang bị khoá. Chuyển
`FirebaseMang.Quen()` xuống chính chỗ phát hiện `biKhoa`.

Đọc thẳng Firestore bằng token chủ dự án (không qua trang web) để chắc nút Khoá ghi thật:
`biKhoa` của A đổi `false → true`, rồi `true → false` sau khi bấm Mở khoá.

**Mật khẩu tài khoản chạy thử không nằm trong mã nguồn.** Repo để công khai, mà trước đó ba kịch
bản viết thẳng mật khẩu vào code — ai cũng đăng nhập được vào hai tài khoản đó và làm bẩn cơ sở
dữ liệu thật. Giờ đọc từ `chay-thu-mang.txt` ở gốc dự án, và file đó nằm trong `.gitignore`.
Thiếu file thì menu dừng ngay **trước khi** vào Play và báo rõ — vào Play rồi mới hỏng thì nhìn
hệt lỗi mạng.

### Ảnh chụp tìm ra ba lỗi mà số đo không thấy

Menu 26 và 27 báo **0 lỗi**, nhưng cả hai chỉ đo tầng REST. Chúng không trả lời được câu hỏi
đơn giản nhất: *giao diện có vẽ ra không*. Nên thêm menu **28** chụp ba màn hình thật (đăng nhập,
sảnh, trong phòng) bằng Game view — không dùng camera phụ, vì camera phụ không vẽ OnGUI.

Ảnh đầu tiên lộ ngay ba chuyện:

**1. Thông báo lỗi hiện nguyên khối JSON.** Giữa màn đăng nhập là một đoạn chữ đỏ
`"code": 403, "message": "Missing or insufficient permissions."`. Hoá ra `DichLoi` chỉ bắt chữ
`Permission denied` — đó là cách nói của **Realtime Database**. **Firestore** nói
`Missing or insufficient permissions.` Hai câu khác hẳn nhau, và câu của Firestore rơi thẳng
xuống nhánh cuối "trả về 120 ký tự đầu của thân lỗi".

**2. Lỗi 403 bị hiểu nhầm thành "chưa có hồ sơ".** `TaiHoacTao` đọc hồ sơ, và **hễ không thành
công** là đi tạo hồ sơ mới. Nhưng chỉ **404** mới có nghĩa "chưa có". Với 403 hay mất mạng thì
lần tạo cũng hỏng, và người chơi đọc được "Khong tao duoc ho so" — sai hẳn nguyên nhân. Sửa: chỉ
404 mới đi tạo; mã khác thì báo đúng lỗi đó.

**3. Nút chọn màn bị cắt chữ.** Nút rộng `150 * s`, chữ "MAN: NGHIA DIA" dài 14 ký tự. Ở màn hình
thấp (`s` nhỏ) chữ mất cả đầu lẫn đuôi, đọc thành **"IAN: NGHIA DI"**. Nới nút lên `230 * s`.

Còn bản thân cái 403 kia thì **không phải lỗi của game**: chính kịch bản chụp gọi `FirebaseMang.Quen()`
giữa lúc màn đăng nhập đang khôi phục phiên cũ, giật mất token đúng lúc nó sắp dùng. Sửa kịch bản:
bỏ phiên cũ **trước khi vào Play**, chứ không cắt ngang. Bài học quen thuộc — phép đo làm hỏng
chính cái nó đang đo.

### Hai cái bẫy về công cụ, không liên quan Firebase

**`ChayThuMang` là MonoBehaviour nên không được nằm trong `Assets/Editor`.** Đặt nhầm vào đó thì
`AddComponent` chỉ ghi một dòng Log ("it is an editor script") rồi trả về `null`, và lỗi thật hiện
ra ở chỗ khác: `NullReferenceException` tại dòng gán `batDau`. Chuyển sang `Assets/Scripts/Mang/`
là hết.

**`AssetDatabase.Refresh()` gọi qua MCP không biên dịch lại.** Cửa sổ Unity không được focus thì
`IsCompiling` trả về False ngay lập tức, Unity vẫn chạy assembly cũ — sửa code ba lần liên tiếp mà
số đo không đổi một chữ. Phải dùng `ImportAsset(path, ImportAssetOptions.ForceUpdate)` rồi đợi
`IsCompiling` lên True rồi xuống False. Từ đó mọi báo cáo chạy thử đều in một dòng `[ban N]` để
biết ngay mình đang đọc kết quả của bản nào.

### Những file mới

| File | Việc |
|---|---|
| `Assets/Scripts/Mang/FirebaseMang.cs` | Cầu REST: đăng ký, đăng nhập, tự đăng nhập lại, giữ hạn token, đọc/ghi/xoá RTDB, dịch mã lỗi sang tiếng Việt |
| `Assets/Scripts/Mang/HoSoMang.cs` | Hồ sơ người chơi trên Firestore; chặn tài khoản bị khoá; cộng thành tích |
| `Assets/Scripts/Mang/PhongMang.cs` | Tạo/vào/rời phòng, vào phòng nhanh, sẵn sàng, đổi màn, đếm ngược, đuổi người |
| `Assets/Scripts/Mang/TranHienTai.cs` | Ba biến static để màn chơi biết mình đang ở phòng nào |
| `Assets/Scripts/Mang/ChayThuMang.cs` | Một MonoBehaviour bé xíu để chạy coroutine trong Play mode |
| `Assets/Scripts/UI/ManDangNhap.cs` | Màn đăng nhập / đăng ký vẽ bằng OnGUI ngay trong game |
| `Assets/Scripts/UI/ManSanh.cs` | Sảnh phòng, danh sách phòng, đếm ngược toàn màn hình |
| `web/index.html`, `web/admin/index.html` | Trang **chỉ dành cho admin** |
| `database.rules.json`, `firestore.rules` | Luật bảo mật, đã triển khai |

Cờ `batChoiMang` trong `MainMenuUI` để tắt toàn bộ phần mạng nếu muốn quay lại kiểu chơi một mình.

---

## Bản chơi trên trình duyệt (WebGL)

### Đẩy mã nguồn lên GitHub không làm ra bản chơi được

Sau khi đẩy repo lên GitHub, câu hỏi đầu tiên là: sao vào `denispham1107.github.io/...` không chơi
được? Vì GitHub chỉ giữ **mã nguồn** — 90 file C#, ba scene, 17 shader. Trình duyệt không chạy
được thứ đó. Phải qua Unity xuất ra bản **WebGL**: toàn bộ C# dịch sang WebAssembly, tài nguyên
gói thành một file dữ liệu.

Và nơi đặt là **Firebase Hosting**, không phải GitHub Pages: game gọi Firebase Auth/Firestore/RTDB,
để cùng một tên miền thì đỡ hẳn một lớp cấu hình.

### Số đo lần xuất bản đầu tiên

```
[ban 1] xuat ban WebGL
ket qua: Succeeded
thoi gian: 11.3 phut
tong dung luong: 161.0 MB
so loi: 0 | so canh bao: 348
  WebGL.data.unityweb          155.7 MB
  WebGL.wasm.unityweb            5.2 MB
  WebGL.framework.js.unityweb    0.1 MB
  WebGL.loader.js                0.0 MB
```

Đo lại từ phía người chơi (`curl -I` lên hosting): **169 MB** phải tải về cho một lần vào.
Đó là con số **quá lớn** — xem mục "Việc còn phải làm" ở cuối.

### Cấu trúc trang sau khi có game

Trang gốc trước đây là trang đăng nhập của admin. Giờ:

| Đường dẫn | Nội dung |
|---|---|
| `/` | **Game** — vào là chơi |
| `/quantri/` | Đăng nhập quản trị |
| `/quantri/admin/` | Bảng quản lý tài khoản |

Chuyển thư mục thì phải sửa lại đường dẫn tương đối trong hai trang admin (`./js/` → `../js/`,
`../js/` → `../../js/`) — quên một chỗ là trang trắng không báo gì.

### Nén: vì sao chọn Gzip kèm "tự giải nén"

Unity WebGL có ba kiểu nén. Cách nhanh nhất là Gzip/Brotli **không** kèm tự giải nén — nhưng khi
đó máy chủ **bắt buộc** phải gắn `Content-Encoding` đúng, sai một dòng cấu hình là trang trắng
không một lời báo lỗi.

Chọn **Gzip + `decompressionFallback = true`**: Unity đặt đuôi `.unityweb` và tự giải nén bằng
JavaScript. Chậm hơn một chút nhưng chạy ở mọi nơi. Cấu hình header trong `firebase.json` vẫn
giữ (khớp `.gz` và `.br`) để dùng khi nào tắt fallback.

### Hai cái bẫy ở Firebase Hosting

**1. `Cache-Control: max-age=3600` cho trang gốc.** Deploy bản mới xong mở trình duyệt vẫn thấy
trang cũ — người chơi phải đợi một tiếng mới thấy bản cập nhật. Thêm quy tắc `no-cache` cho HTML.

**2. Quy tắc `**/*.html` không khớp trang gốc.** Firebase so header theo **đường dẫn URL**, không
theo tên file trên đĩa. Địa chỉ `/` không kết thúc bằng `.html` nên quy tắc trượt — sửa xong deploy
mà `curl -I` vẫn trả `max-age=3600`. Phải khai thêm đúng nguồn `"/"`.

Đây là chỗ dễ tưởng là "đã sửa rồi": file cấu hình trông rất hợp lý, chỉ có máy chủ là không nghĩ vậy.

### Trang mẫu của Unity đặt game vào một ô 960×600

Bản mẫu Unity sinh ra để canvas cố định 960×600 ở góc trái màn hình. Với dự án lấy **điện thoại
làm nền tảng chính** thì không dùng được. Viết template riêng ở
`Assets/WebGLTemplates/Diablo25D/index.html`:

- canvas lấp đầy màn hình trên cả máy tính lẫn điện thoại;
- `touch-action: none` và chặn kéo-để-tải-lại — hai thao tác này phá hỏng điều khiển cảm ứng;
- màn chờ có tên game và **phần trăm tải**: bản nặng 169 MB thì người chơi phải đợi thật, và
  không gì tệ hơn một màn hình đen không nói gì.

Đặt `PlayerSettings.WebGL.template = "PROJECT:Diablo25D"` để mọi lần build sau tự dùng.

### Kiểm chứng

Mở thẳng trang trong trình duyệt, không tin vào việc "deploy xong là xong":

- `curl` bảy đường dẫn (`/`, bốn file trong `Build/`, `/quantri/`, `/quantri/admin/`) — **tất cả 200**;
- tải hết 169 MB trong trình duyệt: tiến trình chạy tới **100%**, màn chờ tự ẩn, **màn đăng nhập
  của game hiện ra**;
- console: **0 lỗi**.

### Chỉ mình chủ phòng vào được trận, người kia kẹt lại ở MainMenu

Lần thử thật đầu tiên với hai máy khác nhau: tạo phòng được, thấy nhau được, bấm sẵn sàng được,
đếm ngược chạy khớp trên cả hai màn hình — rồi hết 10 giây thì **chỉ chủ phòng vào màn chơi**,
người kia đứng nguyên ở MainMenu.

**Nguyên nhân là một cuộc đua.** Điều kiện để nhảy vào trận trước đây là:

```
trangThai == "demNguoc"  VÀ  còn lại <= 0 giây
```

Nhưng ngay khi vào trận, chủ phòng ghi `trangThai = "dangChoi"` lên máy chủ. Máy khách hỏi lại
phòng **mỗi 1 giây**, nên lần hỏi kế tiếp đè `trangThai` thành `"dangChoi"` — và điều kiện
`== "demNguoc"` sai **vĩnh viễn**. Người đó đứng ở sảnh mãi mãi.

Cửa sổ để máy khách kịp nhận ra "hết giờ" trước khi bị đè chỉ rộng vài trăm mili giây (đo được:
ghi mất ~300 ms, đọc ~135 ms). Nên hầu như lần nào cũng hỏng — đúng như người dùng thấy.

**Cách sửa:** `"dangChoi"` cũng phải là một lý do để vào. Nó có nghĩa là trận đã bắt đầu rồi, ai
còn trong phòng thì vào ngay:

```csharp
public static bool DenGioVaoTran(Phong p, double conLaiGiay)
{
    if (p == null) return false;
    if (p.trangThai == "dangChoi") return true;     // host da vao truoc
    return p.trangThai == "demNguoc" && conLaiGiay <= 0;
}
```

Tách thành **hàm thuần** (không đụng biến toàn cục) chính là để kiểm được bằng số. Menu 26 giờ
dựng đúng tình huống đó trên Firebase thật rồi hỏi lại y hệt cách máy khách hỏi:

```
con 6.7 giay, dang dem nguoc -> khach vao tran = False (phai la False)
het gio dem nguoc -> khach vao tran = True
host da vao tran -> trang thai doc duoc = dangChoi, khach vao tran = True
so loi ghi nhan = 0
```

Ba dòng, không phải một. Chỉ đo dòng giữa thì không phân biệt được "sửa đúng" với "hàm luôn trả
về True" — lúc đó người chơi sẽ bị ném vào màn chơi ngay khi vừa vào phòng.

Xuất bản lại: **7,2 phút**, 0 lỗi, 161 MB (lần này nhanh hơn lần đầu 11,3 phút vì Unity còn giữ
kết quả biên dịch cũ).

### Phòng ma: REST không có `onDisconnect`

Xem cơ sở dữ liệu sau buổi thử của người dùng thì thấy **4 phòng còn nằm lại**, chủ phòng đã đóng
tab từ lâu. Chúng không tự mất, và sẽ không bao giờ mất.

Lý do nằm ngay ở lựa chọn nền tảng: tầng mạng đi qua **REST**, mà **REST không có
`onDisconnect()`** — thứ duy nhất của Realtime Database có thể tự dọn khi người ta biến mất.
(Chính vì `onDisconnect` mà phòng được đặt ở RTDB thay vì Firestore — nhưng nó chỉ có trong SDK
thời gian thực, không có trong REST. Một cái bẫy dễ vấp: chọn đúng cơ sở dữ liệu vì một tính
năng, rồi lại truy cập nó bằng con đường không có tính năng đó.)

Bốn phòng kia đều ở trạng thái `dangChoi` nên không hiện trong sảnh. Nhưng nếu ai thoát lúc phòng
còn **đang chờ**, phòng chết sẽ nằm chình ình trong danh sách và người khác bấm vào rồi ngồi đợi
một chủ phòng không bao giờ quay lại.

**Cách làm:** chủ phòng ghi một mốc thời gian mỗi nhịp (`capNhatLuc`), phòng nào quá **60 giây**
không nhúc nhích thì bị lọc khỏi sảnh. Chỉ chủ phòng dập nhịp, không phải mọi người — phòng là
của chủ phòng, chủ phòng bỏ đi thì phòng không còn ý nghĩa.

Hai chi tiết nhỏ mà thiếu là hỏng:
- Phòng vừa tạo chưa kịp dập nhịp lần nào, nên khi không có `capNhatLuc` thì lấy `taoLuc` thay —
  không thì phòng nào cũng chết ngay giây đầu tiên.
- **Lọc chứ không xoá.** Luật chỉ cho chủ phòng xoá phòng của mình, mà người đang xem sảnh thì
  không phải chủ phòng đó. Rác vẫn còn trong cơ sở dữ liệu, admin dọn được trong trang quản trị.

Đo ở **hai phía** — chỉ đo một phía thì không phân biệt được "lọc đúng" với "lọc sạch trơn":

```
phong vua dap nhip -> con song = True (phai la True)
cung phong do, 65 giay sau -> con song = False (phai la False)
danh sach sanh: 1 phong, trong do phong ma = 0 (phai la 0)
so loi ghi nhan = 0
```

Con số nói lên tất cả: lần chạy trước danh sách trả về **5 phòng**, lần này còn **1** — đúng cái
phòng vừa tạo. Bốn phòng ma đã bị lọc sạch.

### Nhưng lọc chưa phải là sửa — và cái nút Giải tán chưa bao giờ dùng được

Người dùng mở trang quản trị và thấy **ba phòng vẫn nằm đó**, ghi "đang mở", dù đã đóng game từ
lâu. Bấm **Giải tán** thì không có gì xảy ra.

Hai chuyện, một gốc: **luật chỉ cho chủ phòng ghi vào phòng của mình.**

- Tôi mới chỉ *lọc* phòng ma khỏi sảnh trong game, chứ không *xoá*, vì người đang xem sảnh không
  phải chủ phòng nên không có quyền. Rác vẫn nằm nguyên trong cơ sở dữ liệu — và trang quản trị
  thì hiện tất cả.
- Tài khoản admin cũng không phải chủ ba phòng đó. Luật từ chối **im lặng**: trang web gọi
  `await remove(...)` trần, lỗi bay lên console, còn người dùng chỉ thấy một cái nút bấm không ăn.

Sửa ba chỗ:

**1. Luật biết ai là admin.** Danh sách admin thật nằm ở Firestore, nhưng **luật của Realtime
Database không đọc được Firestore** — hai cơ sở dữ liệu khác hẳn nhau. Phải giữ thêm một bản chỉ
chứa uid ở `quantri/` bên RTDB. (Một cái bẫy đáng nhớ: chọn RTDB *vì* `onDisconnect`, rồi truy cập
nó bằng REST — con đường không có `onDisconnect`; rồi lại đặt danh sách admin ở cơ sở dữ liệu kia.)

**2. Ai cũng được dọn xác.** Luật cho phép **xoá** — và chỉ xoá, không cho sửa — một phòng đã quá
60 giây không dập nhịp:

```
!newData.exists() && ( !data.hasChild('capNhatLuc') || data.child('capNhatLuc').val() < now - 60000 )
```

Chỉ cho xoá chứ không cho sửa: không ai muốn người lạ vào sửa phòng của mình chỉ vì mình rớt mạng
một phút. Game giờ vừa đọc sảnh vừa dọn xác — trả danh sách cho người gọi **trước**, rồi mới xoá,
để sảnh hiện ra ngay chứ không đợi mấy lệnh xoá.

**3. Nút bấm phải nói khi nó hỏng.** `giaiTan()` bắt lỗi và hiện ra; trang quản trị thêm cột
"⚠ chủ phòng đã thoát" và một nút **Dọn N phòng đã chết**.

Đo, và phải đo **cả hai phía** — chỉ xem "phòng ma có mất không" thì không phân biệt được với
"xoá sạch mọi thứ":

```
dung mot phong ma (im lang 600 giay): -P0u7pOFlynEDX1ULa2w
sau khi doc sanh, phong ma con khong: da bi xoa
phong dang mo cua minh con nguyen: True (phai la True)
so loi ghi nhan = 0
```

Và bằng chứng cuối cùng, đọc thẳng cơ sở dữ liệu bằng token chủ dự án: trước đó có **4 phòng**,
sau một lần đọc sảnh còn **0**. Ba phòng trong ảnh chụp của người dùng biến mất mà không phải
bấm gì.

### Giai đoạn 2, bước 0: đường truyền nối thẳng máy với máy

Trong trận đấu, vị trí nhân vật **không đi qua Firebase**. Lý do là một con số đo được: một vòng
khứ hồi tới Firebase Singapore mất **49 ms**, và đó là *sàn cứng* — cộng thêm nhịp gửi, đệm nội
suy và khung hình thì vượt 80 ms. Nối thẳng máy với máy trong nước thì còn 10–30 ms. Đó là cả
khoảng cách giữa "đạt" và "không đạt" mục tiêu 50–80 ms.

Firebase vẫn còn việc của nó: hai máy dùng nó để **tìm thấy nhau**. Bắt tay xong thì nó đứng sang
một bên — trong suốt trận đấu không còn gói tin nào đi qua Google, và nếu Firebase sập giữa trận
thì trận vẫn chạy.

**Bốn file:**

| File | Việc |
|---|---|
| `Assets/Plugins/WebGL/CauNoiWebRTC.jslib` | gọi thẳng `RTCPeerConnection` của trình duyệt |
| `Assets/Scripts/Mang/KenhTrucTiep.cs` | một cửa duy nhất cho cả hai nền tảng |
| `Assets/Scripts/Mang/BatTay.cs` | hai máy trao đổi địa chỉ qua Realtime Database |
| `web/dothu/index.html` | công cụ đo, dùng đúng cơ chế đó nhưng chạy ngoài game |

**Kênh phải là kênh không tin cậy.** `ordered: false, maxRetransmits: 0` khiến nó hành xử như
UDP: gói nào rớt thì bỏ luôn. Để mặc định (tin cậy, đúng thứ tự) thì một gói rớt sẽ **chặn mọi
gói sau nó** và độ trễ vọt từ 50 lên 300 ms đúng lúc đang đánh nhau — mà một vị trí cũ 20 ms
trước thì gửi lại cũng vô ích.

**Chuỗi trả về từ JavaScript phải được giải phóng.** Bên `.jslib` cấp phát bằng `_malloc` trong
heap của Unity; C# không gọi `FreeHGlobal` thì mỗi tin nhắn để lại một mảnh rác — một trận đấu có
hàng chục nghìn tin nhắn.

**Editor và PC không có WebRTC** (gói `com.unity.webrtc` không chạy trên WebGL, nên không dùng
chung được). Ở đó `KenhTrucTiep` chạy một kênh **giả lập trong bộ nhớ**: đủ để kiểm phần logic mà
không phải build WebGL 7 phút mỗi lần, nhưng **không đo được độ trễ thật**. Cờ `LaGiaLap` có ở đó
để không ai lỡ báo cáo một con số giả lập như thể nó là số đo thật.

#### Công cụ đo, và hai lỗi của chính nó

Lần đo đầu tiên của người dùng trả về: trung vị **5,7 ms**, mất gói **9%**, kiểu kết nối
**"không xác định"**.

Cả ba dòng đều có vấn đề, và hai trong số đó là lỗi của công cụ:

**1. "Không xác định" — tôi đọc quá muộn.** Dòng cuối nhật ký là
`trang thai ICE: disconnected`: kết nối đã rớt trước khi tôi gọi `getStats()`, nên không còn cặp
ứng viên nào để xem. Mà đó lại chính là dòng quan trọng nhất — không biết nối thẳng hay phải
tiếp sức thì mọi con số phía trên đều lơ lửng. Sửa: đọc **ngay khi kênh mở**.

**2. Mất gói 9% khớp đáng ngờ với việc rớt kết nối.** 200 gói, mất 18 — bằng đúng phần cuối, tức
mất dồn một cục lúc rớt chứ không rải đều. Sửa: ghi nhận nếu ICE rớt *trong* lúc đo và nói thẳng
là tỉ lệ mất gói không còn đáng tin.

**3. Còn 5,7 ms thì không phải lỗi, mà là số đúng của một tình huống sai.** Hai máy ở **cùng mạng
nội bộ**: gói tin chạy qua cái router trong nhà chứ không hề ra Internet. Ra 2–6 ms là chuyện
bình thường, và nó không nói lên được gì về lúc hai người ở hai nơi khác nhau. Công cụ giờ nhận
ra trường hợp `host ↔ host` và nói thẳng: *"Hai máy đang ở CÙNG MẠNG NỘI BỘ — con số này không
đại diện cho lúc chơi thật."*

Bài học lặp lại lần nữa: một phép đo cho ra số đẹp không có nghĩa là nó đang đo đúng thứ mình
tưởng.

#### Wi-Fi ↔ 4G treo cứng, và cái `catch` rỗng đã giấu lỗi suốt

Đo lần hai với một máy Wi-Fi nhà, một máy 4G: **treo luôn**. Máy 1 báo ICE `checking` →
`disconnected`, máy 2 đứng mãi ở "Đang nối…", không bên nào nói gì thêm.

Nhật ký cho thấy phần bắt tay đã xong **cả hai chiều** ("may 2 da tra loi" ở máy 1, "da tra loi
may 1" ở máy 2). Vậy hai máy đã tìm thấy nhau; chỉ khâu nối thẳng là hỏng. Rất dễ đổ ngay cho
NAT của nhà mạng — nhưng lỗi nằm trong code của tôi:

```js
onChildAdded(... ice1 ..., (anh) => {
  pc.addIceCandidate(...).catch(() => {});   // ← nuốt lỗi im lặng
});
```

Firebase **phát lại toàn bộ** ứng viên đã có ngay khi mình bắt đầu nghe. Máy 2 nghe trước, rồi
mới đọc lời mời và `setRemoteDescription` — nên những ứng viên đầu tiên của máy 1 đến lúc chưa có
mô tả của bên kia. Gọi `addIceCandidate` lúc đó là ném lỗi, và cái `.catch(() => {})` vứt lỗi đi
không một tiếng động. Kết quả: máy 2 **mất sạch địa chỉ của máy 1**, ICE không có đường nào để thử.

**Vì sao lần đo trước vẫn chạy:** hai máy cùng mạng nội bộ nên còn ứng viên `host` sinh ra sau,
đủ để nối. Môi trường thuận lợi đã che kín lỗi — đúng kiểu lỗi chỉ lộ ra khi ra đời thật.

Sửa ba chỗ:

1. **Xếp hàng ứng viên** cho tới khi có mô tả của bên kia, rồi xả một lượt — không vứt đi nữa.
2. **Không nuốt lỗi**: `catch` giờ ghi thẳng ra nhật ký.
3. **Không treo im lặng**: sau 25 giây chưa nối được thì báo rõ, kèm chẩn đoán — nhận được bao
   nhiêu địa chỉ của máy kia. Nhận 0 nghĩa là hai máy chưa gặp nhau; nhận đủ mà vẫn không nối
   được thì gần như chắc chắn là NAT hai bên chặn nhau (mạng 4G Việt Nam thường dùng CGNAT, loại
   NAT mà chỉ STUN không xuyên qua được) — và trường hợp đó cần máy chủ tiếp sức.

Cùng một lỗi có ở `CauNoiWebRTC.jslib` vì tôi chép nguyên cách làm sang; đã sửa cả hai.

#### Số đo thật: Wi-Fi ↔ 4G, và cái đuôi của phân bố

Sau khi sửa, đo lại với một máy Wi-Fi nhà và một máy 4G:

```
Nhỏ nhất            18.9 ms
Trung vị            28.1 ms
95% số lần dưới     72.6 ms
Lớn nhất           138.6 ms
Dao động (jitter)   53.7 ms
Gói gửi / nhận về   200 / 200
Tỉ lệ mất gói       0.0%
Kiểu kết nối        host ↔ prflx      ← KHÔNG có relay
Ước tính nhìn thấy nhau   ~61 ms
```

**Dòng quan trọng nhất là `host ↔ prflx`**: hai máy nối **thẳng** với nhau, không qua máy chủ tiếp
sức nào. Lo ngại rằng CGNAT của mạng 4G Việt Nam sẽ chặn — đã không xảy ra. Nghĩa là không cần
TURN, và hạ tầng vẫn thuần Google Cloud.

Mất gói **0%** so với 9% lần trước, xác nhận thêm rằng 9% kia là do rớt kết nối chứ không phải
đường truyền kém.

**Nhưng cái đuôi phân bố mới là thứ quyết định cảm giác chơi:**

| | Độ trễ đường truyền | Cộng 33 ms xử lý | Mục tiêu 50–80 ms |
|---|---|---|---|
| Trung vị | 28,1 ms | ~61 ms | đạt |
| 95% số lần dưới | 72,6 ms | ~106 ms | **vượt** |

Cứ 20 gói thì có 1 gói mất hơn 72 ms — bản chất của mạng di động. Phần lớn thời gian mượt, thỉnh
thoảng giật một nhịp.

Điều này **đổi một quyết định ở bước 4**: đệm nội suy 1 nhịp cố định (17 ms) mà kế hoạch ban đầu
dự tính sẽ không đủ che dao động 53,7 ms. Phải làm **đệm co giãn theo mạng** — dày lên khi đường
truyền chập chờn, mỏng lại khi ổn định. Biết điều này từ bước 0 thì bước 4 làm đúng ngay, thay vì
làm xong rồi mới phát hiện game giật trên 4G.

### Giai đoạn 2, bước 1: tách ý muốn ra khỏi việc thi hành

`PlayerController` 940 dòng vừa *đọc phím* vừa *thi hành phép*. Chơi một mình thì không sao,
nhưng chơi mạng thì cùng một đoạn logic phải chạy được ở **hai nơi**:

- máy người chơi — chạy ngay khi bấm, không đợi mạng (dự đoán);
- máy trọng tài — chạy lại chính chuỗi thao tác đó để phán xử.

Bàn phím chỉ có ở nơi thứ nhất. Nên phải tách.

**Ba file:**

| File | Việc |
|---|---|
| `GoiInput.cs` | struct: một khung hình ý muốn của người chơi |
| `DocInput.cs` | nơi **duy nhất** trong đường điều khiển được phép gọi `Input.*` hay đọc `CamUng` |
| `PlayerController.cs` | chỉ còn thi hành; nhận ý muốn từ `input` |

**Vì sao mọi trường trong gói đều là toạ độ thế giới**, không phải toạ độ màn hình: mỗi người xoay
camera một kiểu, và máy trọng tài thì không có camera của ai cả. Gửi "nghiêng cần sang trái" thì
bên kia không dịch được ra hướng nào; gửi thẳng "đi về hướng (0.7, 0, 0.7)" thì ai cũng hiểu giống
nhau. Nên phép đổi trục theo camera được chuyển hẳn sang `DocInput`.

Ngoại lệ có chủ ý: `GetAimPoint` vẫn nằm ở `PlayerController` và vẫn đọc `Input.mousePosition` —
nhưng **chỉ máy người chơi gọi nó**. Nó bắn tia từ camera của mình rồi nhét *kết quả* (một điểm
trong thế giới) vào gói. Trọng tài chỉ nhận điểm đã ngắm rồi kẹp lại trong tầm cho phép.

Cờ `tuDocInput` giữ hành vi cũ nguyên vẹn: bật (mặc định) thì nhân vật tự đọc bàn phím như trước;
tắt thì chờ ngoài bơm vào — dùng cho nhân vật của người khác, và cho việc chạy lại input khi
hiệu chỉnh.

**Đo (menu 30), hai chiều chứ không một:**

```
[ban 1] buoc 1 - tach y muon ra khoi viec thi hanh
tim thay nhan vat: Player, moveSpeed = 5.2
co bo doc input gan kem: True (phai la True)
bom huong (1,0,0) trong 1.2s -> di duoc 6.27 m, lech truc X 6.27 m (toi thieu can 3.12 m)
bom huong (0,0,1) trong 1.2s -> di duoc 6.25 m, lech truc Z 6.24 m
tra ve tu doc phim, khong bam gi -> troi 0.000 m (phai gan 0)
bom ky nang 0 -> so lan bam ky nang tang 1 (phai la 1), nang luong 250 -> 247
so loi ghi nhan = 0
```

Con số đắt nhất là **6,27 m**: lý thuyết `5,2 × 1,2 = 6,24 m`. Lệch 0,5% — logic di chuyển còn
nguyên vẹn sau khi tách, chứ không phải "chạy được là xong".

Chiều thứ hai (**trôi 0,000 m**) mới là chiều bắt được lỗi nguy hiểm nhất: gói input cũ kẹt lại và
nhân vật cứ chạy mãi. Chỉ đo chiều "bơm vào có đi không" thì lỗi đó không bao giờ lộ ra.

**Phải đo cả hai màn.** Lần đầu tôi chỉ chạy trên Act2 rồi báo là xong — thiếu, vì Act1 dựng bằng
code lúc chạy còn Act2 là scene đã nướng sẵn, hai đường khác hẳn nhau. Chạy lại trên Act1 (menu
30b): đi được **6,34 m**, trôi **0,000 m**, năng lượng 250 → 246, **0 lỗi**. Hai màn cùng đạt thì
mới kết luận được.

**Và một mảnh mà máy không tự đo được.** Phép thử bơm ý muốn thẳng vào bằng code, nên nó chứng
minh đường *"ý muốn → thi hành"* còn nguyên. Nhưng đường *"bàn phím → `DocInput` → thi hành"* thì
chỉ kiểm được một nửa: không bấm gì thì đứng yên. Bàn phím không giả lập được bằng code, nên phần
này phải do người thật bấm: **WASD, chuột trái để đi, phím 1–7 tung phép, và bản cảm ứng với cần
joystick lẫn nút kỹ năng — tất cả chạy đúng như trước.** Đó mới là dòng khép lại bước 1: sáu chỗ
đọc phím đã dời sang file khác mà người chơi không nhận ra gì thay đổi.

Và bơm kỹ năng thì **năng lượng tụt 250 → 247** — có tụt mới là đã thực sự tung phép, chứ đếm số
lần bấm thì chỉ chứng minh con số tăng.

### Giai đoạn 2, bước 2: nhân vật của mình phản hồi tức thì

Với 28 ms độ trễ đo được ở bước 0, nếu chờ trọng tài xác nhận rồi mới cho nhân vật nhúc nhích thì
người chơi bấm một cái phải đợi **56 ms** mới thấy chính nhân vật mình phản ứng. Cảm giác "nặng
tay" xuất hiện ngay lập tức.

Nên máy người chơi **thi hành ngay**, đồng thời nhớ lại những gói input chưa được xác nhận. Khi
trọng tài báo *"tôi đã xử lý tới gói số N, kết quả là đây"*:

1. đặt nhân vật về đúng trạng thái đó,
2. **chạy lại** các gói từ N+1 tới hiện tại,
3. nếu ra đúng chỗ cũ thì người chơi không thấy gì cả.

Chỉ khi trọng tài bắt được điều gì khác — va chạm, bị đẩy lùi, ăn đòn — thì vị trí mới lệch, và
lúc đó mới kéo nhân vật về.

**Ba file mới, một hàm tách ra:**

| | |
|---|---|
| `TrangThaiNhanVat.cs` | ảnh chụp một thời điểm |
| `DuDoan.cs` | vòng đệm input chưa xác nhận + hiệu chỉnh |
| `PlayerController.ThiHanhMotKhung(GoiInput)` | tách khỏi `Update` để **gọi lại được** |
| `ChupTrangThai` / `DatTrangThai` | chụp và đặt lại |

**Dùng `g.dt` chứ không phải `Time.deltaTime`.** Lúc chạy lại, một khung hình cũ 16,7 ms phải được
thi hành đúng 16,7 ms chứ không theo nhịp khung hình hiện tại. Sai chỗ này thì mỗi lần hiệu chỉnh
nhân vật lại nhảy một đoạn — mà hiệu chỉnh xảy ra vài chục lần mỗi giây.

**Hộ chiếu hồi chiêu cũng phải nằm trong ảnh chụp.** Thiếu nó thì lúc chạy lại, một kỹ năng đang
đợi hồi sẽ được coi là sẵn sàng và bắn ra lần nữa — tự nhiên nhân vật tung hai quả cầu lửa từ một
lần bấm.

**Phải tắt `CharacterController` trước khi đổi vị trí.** Nó giữ một bản sao vị trí ở tầng dưới;
gán thẳng `transform.position` trong khi nó đang bật thì khung hình sau nó kéo nhân vật về chỗ cũ,
và hiệu chỉnh nhìn như không ăn gì.

**Hai ngưỡng, và lý do có chúng:**

- **5 cm** — dưới mức này coi như khớp. Không thể đòi khớp tuyệt đối: phép tính dấu phẩy động trên
  hai máy không bao giờ ra số giống hệt tới chữ số cuối. Đòi bằng nhau tuyệt đối thì khung hình
  nào cũng "lệch" và nhân vật rung liên tục.
- **4 m** — quá mức này thì nhảy thẳng về, không kéo từ từ nữa; kéo một quãng xa quá thì nhân vật
  trượt dài như đi băng.

#### Đo (menu 31)

```
[ban 2] buoc 2 - chuoi input di xa hon de phep kiem co suc nang
1. tat dinh: chay 120 goi hai lan -> lech 0.0000 m (di duoc 4.84 m)
2. hieu chinh khi KHOP -> nhan vat xe dich 0.0000 m (phai gan 0), lech phat hien 0.0000 m
3. hieu chinh khi LECH 2 m -> phat hien lech 2.00 m, chay lai 80 goi
4. sau hieu chinh con giu 80 goi (phai la 80)
so loi ghi nhan = 0
```

**Phép đo số 1 là điều kiện sống còn của cả bước 2.** Cùng một chuỗi input, chạy lại phải ra cùng
một kết quả — không tất định thì mỗi lần hiệu chỉnh nhân vật lại nhảy một đoạn.

Bản đầu của phép thử này **yếu**: chuỗi input đổi hướng mỗi 12 khung với bước 1,7 rad khiến nhân
vật xoay vòng tại chỗ, 60 gói mà chỉ dịch được **0,94 m**. Kiểm tính tất định với một nhân vật gần
như đứng yên thì chẳng chứng minh được gì. Sửa thành 120 gói, đổi hướng thoải hơn — nhân vật đi
**4,84 m** và vẫn lệch **0,0000 m**.

**Phép đo số 2 mới là cái xảy ra 99% thời gian:** trọng tài xác nhận đúng y điều mình đã đoán.
Nhân vật xê dịch **0,0000 m** — không nhúc nhích. Nếu chỗ này mà giật thì game không chơi được,
vì nó lặp lại vài chục lần mỗi giây.

Chuỗi input dùng bộ sinh số có hạt giống cố định chứ không dùng `Random` của Unity: `Random` đó
dùng chung trạng thái với cả game, hạt lửa hay quái vật nhúc nhích một cái là chuỗi đổi và hai lần
chạy không còn so sánh được.

Chạy lại menu 30 sau khi sửa: bước 1 vẫn nguyên vẹn (6,34 m và 6,20 m, lý thuyết 6,24), 0 lỗi.

### Giai đoạn 2, bước 3: một cảnh chứa nhiều nhân vật

Cả game được viết với một giả định ngầm: **có đúng một người chơi**. `GameDirector.player` là một
`Transform` duy nhất; quái được gắn thẳng người đó làm mục tiêu ngay lúc sinh ra.

Điều đó nghĩa là: đặt thêm một người nữa vào cảnh thì **cả bầy quái vẫn chỉ đuổi một người**.
Người thứ hai đứng giữa đám quái mà không con nào thèm ngó, còn người thứ nhất bị cả bản đồ dí.

**Ba thay đổi:**

| Chỗ | Trước | Sau |
|---|---|---|
| `GameDirector` | `player` — một `Transform` | thêm `moiNguoi` — danh sách; `player` vẫn còn, nay chỉ có nghĩa "người chơi **của máy này**" (camera bám, HUD hiện máu) |
| `EnemyAI` | gắn mục tiêu một lần lúc sinh | hỏi lại `GanNhat()` mỗi **0,7 giây** |
| — | — | `NguoiChoiKhac.Sinh()` dựng nhân vật cho người khác |

**Vì sao hỏi lại theo nhịp chứ không mỗi khung hình:** một bản đồ có hàng trăm con quái; mỗi con
quét danh sách 60 lần mỗi giây là phí không. 0,7 giây đủ nhanh để bám theo người đang chạy.

**Nhân vật của người khác dùng y hệt nhân vật của mình** — cùng prefab, cùng bộ kỹ năng, cùng máu.
Chỉ khác đúng một điều: `tuDocInput = false`, ý muốn đến qua đường truyền. Không làm một prefab
riêng kiểu "hình bóng người khác", vì đến bước đánh nhau thì nhân vật đó phải chịu sát thương,
phải có khiên, phải chết — tức phải là một nhân vật thật sự. Một cái vỏ rỗng nhìn giống người chơi
thì sẽ phải đi bổ sung từng thứ một, và mỗi lần quên một thứ là một lỗ hổng không ai thấy cho đến
lúc đang đánh nhau.

**Hai thứ bắt buộc phải gỡ khỏi nhân vật người khác:**

- **Camera.** Hai camera trong một cảnh thì Unity chọn bừa một cái, và người chơi đột nhiên nhìn
  thế giới bằng mắt người khác.
- **Tag `"Player"`.** Nhiều chỗ trong game vẫn gọi `FindGameObjectWithTag("Player")` và chỉ lấy
  **cái đầu tiên tìm thấy** — để nguyên thì camera hoặc quái có thể vớ phải người khác thay vì
  người chơi của máy này.

#### Đo (menu 32)

```
[ban 1] buoc 3 - mot canh chua nhieu nhan vat
truoc khi them: 1 nguoi trong danh sach
1. sinh nhan vat cho nguoi khac: OK - NguoiChoi_BanThu
   danh sach gio co 2 nguoi (phai la 2)
   nguoi khac tu doc ban phim khong: False (phai la False)
   so camera: 1 -> 1 (khong duoc tang)
2. 33 con quai -> nham minh 21, nham nguoi kia 12, khac 0
3. truoc khi chet, gan (22.00, 0.21, -10.50) nhat la: nguoi kia (dung)
   sau khi nguoi kia guc -> GanNhat tra ve nguoi khac: True (phai la True)
   so nguoi con song: 1 (phai la 1)
4. sau khi bo: danh sach con 1 nguoi (phai la 1)
so loi ghi nhan = 0
```

**Dòng 2 là phép đo có sức nặng nhất.** Đặt một người cách 22 m rồi đếm xem quái nhắm ai: **21
nhắm mình, 12 nhắm người kia**. Trước khi sửa, con số đó sẽ là 33 và 0 — và đó chính là thứ phải
chứng minh, chứ không phải "sinh được nhân vật thứ hai".

Phép đo số 3 kiểm chiều ngược lại: giết người kia đi thì `GanNhat` phải **thôi** trả về họ. Thiếu
điều này thì quái sẽ xúm quanh một cái xác trong khi người còn sống đi lại thoải mái.

Chạy lại menu 30 và 31 sau khi sửa: bước 1 và 2 vẫn nguyên vẹn, 0 lỗi.

### Giai đoạn 2, bước 4: thấy nhau di chuyển mượt

Vị trí người khác đến 60 lần mỗi giây, còn màn hình vẽ 60 lần. Đặt thẳng vị trí vừa nhận thì
nhân vật nhảy từng nấc — nhìn như tua hình. Phải **vẽ ở giữa hai mốc đã nhận**. Mà muốn vẽ ở giữa
thì phải chấp nhận **tụt lại phía sau một chút**: luôn hiện nhân vật ở thời điểm *"bây giờ trừ đi
độ dày đệm"*. Độ dày đệm chính là cái giá phải trả để đổi lấy sự mượt mà.

**Ba file:**

| | |
|---|---|
| `GoiTin.cs` | đóng gói trạng thái thành byte, và mở ra |
| `NoiSuy.cs` | đệm co giãn + vẽ ở giữa hai mốc |
| `DongBoTran.cs` | gửi trạng thái mình, nhận và vẽ người khác |

#### Gói tin: 54 byte thay vì 260

Vị trí nén xuống số nguyên 16 bit, độ phân giải 1 cm — bản đồ rộng chừng 70 m, mà 16 bit phủ được
±327 m. Góc chỉ cần một trục vì nhân vật luôn đứng thẳng; gửi cả bốn số của quaternion là phí ba
phần tư.

```
1. goi tin 4 nguoi: nhi phan 54 byte, JSON 260 byte (nho hon 4.8 lan)
   doc lai: lech vi tri 0.0000 m, lech goc 0.003 do
   goi cut bi tu choi dung cach
```

Gói cụt **phải bị từ chối** chứ không được đọc bừa: gói đến từ máy khác qua kênh không tin cậy, và
một gói bị cắt đôi mà cứ đọc thì ném lỗi giữa trận đấu.

#### Nhịp gửi 20 lần/giây là một sai lầm, và số đo đã chỉ ra

Ban đầu tôi chọn 20 lần/giây cho tiết kiệm băng thông. Đo ra: **đệm phình lên 132 ms**.

Lý do là một ràng buộc tôi đã bỏ qua: **đệm không thể mỏng hơn khoảng cách giữa hai mốc**. Gửi 20
lần/giây nghĩa là hai mốc cách nhau 50 ms, nên đệm phải chứa được ít nhất chừng ấy mới luôn có mốc
sau để vẽ ở giữa. Cộng cả đường truyền thì độ trễ nhìn thấy nhau lên **~154 ms** — gấp đôi mục
tiêu 50–80 ms.

Chuyển sang 60 lần/giây: mạng tốt thì đệm còn **49 ms**. Cái giá là băng thông 54 × 60 = **3,2 KB/giây**
mỗi người — trên 4G là con số không đáng kể, rẻ hơn nhiều so với 80 ms độ trễ.

Bài học: tiết kiệm băng thông ở chỗ này **không đổi lấy được gì**, mà còn làm hỏng đúng cái đang cố
gắng đạt được.

#### Nhưng trên mạng 4G thật thì đệm vẫn dày, và đó là vật lý

```
2. mang ly tuong          : nhay 0 lan | lech 0.002 m | dem  49 ms
3. mang 4G that (jitter 54): nhay 0 lan | lech 0.102 m | dem 131 ms
4. mat 10% goi            : nhay 1 lan | lech 0.029 m | dem 125 ms
5. dem co gian: mang tot 49 ms -> mang xau 131 ms
```

Tăng nhịp gửi giúp mạng tốt (101 → 49 ms) nhưng **không cứu được mạng 4G**: khi jitter 54 ms thì
đệm bị chi phối bởi dao động chứ không phải nhịp gửi. Đây là đánh đổi vật lý:

- đệm mỏng → độ trễ thấp nhưng gói đến muộn sẽ không còn gì để nội suy, nhân vật khựng lại;
- đệm dày → mượt nhưng trễ.

Nên phải **đo cả cái giá của đệm mỏng** rồi mới chọn, chứ không đoán:

```
--- Tren mang 4G that (jitter 54 ms), chay 6 giay ---
   dem tu co gian 131 ms -> nhay 0 lan
   ep 90 ms              -> nhay 1 lan
   ep 60 ms              -> nhay 1 lan
```

Ép đệm xuống 60 ms chỉ giật **1 lần trong 6 giây**. Quy ra độ trễ nhìn thấy nhau:

| Đệm | Tổng độ trễ | Giật (6 giây) |
|---|---|---|
| Tự co giãn (131 ms) | ~153 ms | 0 lần |
| Ép 90 ms | ~112 ms | 1 lần |
| **Ép 60 ms** | **~82 ms** | 1 lần |

Cột giữa cộng: một chiều đường truyền 14 ms + đệm + khung hình 8 ms.

Mặc định để **tự co giãn** (mượt nhất). Muốn bám sát mục tiêu 50–80 ms thì đặt
`NoiSuy.demToiDaEp = 0.06f` — đổi lấy khoảng mười cú giật nhẹ mỗi phút.

#### Hai chi tiết nhỏ mà thiếu là hỏng

**Gói đến không đúng thứ tự là chuyện bình thường** trên kênh không tin cậy — phải chèn vào đúng
chỗ theo mốc thời gian chứ không vứt đi.

**Không đặt vị trí người khác qua `CharacterController`.** Vị trí đó đến từ máy kia, va chạm đã
được tính ở bên đó rồi; cho `CharacterController` xen vào lần nữa thì nhân vật bị kẹt vào tường hai
lần.

### Giai đoạn 2, bước 5: đánh giết lẫn nhau

#### Bật PvP chỉ bằng một dòng — và đó là nhờ thiết kế cũ

Mọi kỹ năng đều nhận `damageMask` từ bên ngoài, và `PlayerController.enemyMask` là **nơi duy nhất**
cấp mask đó. Nên thêm lớp `Player` vào đấy là cả bảy phép đều đánh được người chơi khác, không
phải sửa từng phép một:

```csharp
enemyMask = TranHienTai.DangChoiMang
    ? LayerMask.GetMask("Enemy", "Player")
    : LayerMask.GetMask("Enemy");
```

Nhưng đổi lại, phép cũng đánh được **chính mình**: quả cầu lửa nổ ngay dưới chân sẽ giết người vừa
bấm phím. Nên phải thêm "bỏ qua ai" xuống tận nơi tính sát thương.

#### Truyền "bỏ qua" qua mười một chỗ

| Nơi | Việc |
|---|---|
| `CombatUtil.AreaDamage / AreaFreeze / AreaShock` | thêm tham số `boQua` |
| `Fireball`, `ThienThach`, `VungLua`, `FallingShard`, `LightningStrike` | thêm trường `boQua`, truyền xuống |
| `IceStorm`, `LightningStorm` | **giữ** `boQua` rồi giao lại cho từng mảnh sinh sau |
| `Tornado`, `GiatSet` | tự lọc trong vòng lặp tìm mục tiêu |
| `PlayerController.Release()` | giao `boQua = health` cho mọi phép vừa tung |

Hai kỹ năng cần cách khác: **mưa băng và sấm sét sinh từng mảnh trễ vài giây**, nên không thể đọc
một biến tạm lúc tung — phải giữ trong bản thân cơn mưa rồi giao lại cho mỗi mảnh khi nó rơi.

`AreaFreeze` có tham số `out` ở cuối nên `boQua` phải đặt trước nó, không dùng được giá trị mặc
định — đây là chỗ duy nhất phải sửa lời gọi cũ.

#### Đo (menu 34), bốn chiều

```
[ban 1] buoc 5 - danh giet lan nhau
lop Player co so hieu: 9
1. choi mot minh -> mask co lop Player: False (phai la False)
2. choi doi khang -> mask co lop Player: True (phai la True)
3. no ngay tren dau nguoi kia -> ho mat 37 mau (400 -> 363)
4. no ngay duoi chan MINH -> minh mat 0 mau (phai la 0)
4b. cung cu no do nhung KHONG bo qua -> minh mat 40 mau (phai lon hon 0)
so loi ghi nhan = 0
```

**Phép 4b mới là phép quan trọng nhất.** Chỉ đo "nổ dưới chân mình mà không mất máu" thì không
phân biệt được *"bỏ qua đúng"* với *"phép này chẳng trúng ai cả"*. Phải chứng minh rằng cùng cú nổ
ấy, khi **không** bỏ qua, thì máu **có** tụt — 40 điểm.

Và phép 1 giữ cho chế độ chơi một mình không bị vạ lây: mask không được chứa lớp `Player`, nếu
không người chơi đơn sẽ tự thiêu mình mà không hiểu vì sao.

#### Một sai lầm về công cụ, tốn khá nhiều thời gian

Sau khi viết xong phép thử, menu 34 **không xuất hiện** — mà console qua MCP trả về **rỗng hoàn
toàn**, kể cả `Debug.Log` tôi vừa in, và `IsCompiling` báo `False`. Mọi dấu hiệu đều nói "đã xong".

Thật ra biên dịch đã thất bại, assembly cũ vẫn chạy nên các menu cũ vẫn còn. Lỗi chỉ hiện ra khi
đọc thẳng nhật ký của Unity:

```bash
grep -n "error CS" Logs/Editor.log | tail -8
```

Lỗi: `LayerMask.GetMask()` trả về `int` chứ không phải `LayerMask`, nên `.value` không tồn tại.

Chú ý chỗ tìm: `%LOCALAPPDATA%\Unity\Editor\Editor.log` chỉ có phần khởi động rồi ghi *"Logs moved
to project-relative Editor.log file"* — nhật ký thật nằm ở **`Logs/Editor.log` trong thư mục dự
án**. Từ nay, sau khi thêm một script Editor mới thì hỏi thẳng xem menu đã tồn tại chưa, đừng chỉ
tin `IsCompiling == False`.

#### Còn thiếu trong bước 5

Hai phần chưa làm, và phải nói rõ chứ không lặng lẽ bỏ qua:

- **Trọng tài phán xử máu.** Hiện mỗi máy tự tính sát thương, nên hai máy có thể thấy máu lệch
  nhau — nhất là `AreaFreeze` và `AreaShock` dùng `Random.value`, mỗi máy gieo riêng thì người này
  thấy địch đóng băng, người kia thấy không.
- **Bù trễ khi tính trúng.** Ở 28 ms độ trễ, người chạy ngang sẽ né được đòn dù trên màn hình
  người bắn thấy trúng rõ ràng.

Cả hai chỉ có nghĩa khi đã có hai máy thật nối vào nhau, nên để làm cùng lúc với việc ghép trận.

### Ghép hai máy: lần đầu mọi thứ gặp nhau trên một màn hình

Từng mảnh của giai đoạn 2 đều đã được đo riêng, nhưng đo riêng **không chứng minh được rằng ghép
lại thì chạy**. `KhoiDongTranMang` là chỗ chúng gặp nhau:

1. Đọc lại phòng từ Firebase — biết ai là chủ phòng, ai là khách.
2. Chủ phòng **mời**, khách **nhận**. Bắt tay qua Realtime Database, một lần, 1–2 giây.
3. Sinh nhân vật cho người kia (bước 3).
4. Gắn `DongBoTran` — gửi và nhận vị trí 60 lần mỗi giây (bước 4).

Xong bước 2 thì **Firebase đứng sang một bên**: từ đó tới hết trận không còn gói tin nào đi qua
Google.

**Tự gắn vào màn chơi bằng `[RuntimeInitializeOnLoadMethod]`**, không kéo tay vào scene. Lý do rất
cụ thể: Act1 dựng bằng code lúc chạy còn Act2 là scene đã nướng sẵn — gắn tay thì phải nhớ cả hai,
và quên một cái là một màn không nối mạng được mà chẳng báo gì.

**Màn hình phải nói nó đang làm gì.** Bắt tay mất 1–2 giây, và nếu hỏng thì phải hỏng ra tiếng:
*"Đang tìm người chơi khác…"* → *"Đang mời người kia nối vào…"* → *"Đã nối! (bắt tay mất 1,4 giây)"*,
hoặc một dòng đỏ nói rõ hỏng ở đâu. Ba giây sau khi nối được thì dòng chữ tự tắt, trả màn hình lại
cho game.

#### Hai giới hạn phải nói trước

**Chỉ hai người.** `KenhTrucTiep` giữ đúng một kết nối. Bốn người thì phải nối hình sao qua chủ
phòng và chủ phòng chuyển tiếp — chưa làm. Nói rõ ở đây chứ không để người ta vào phòng bốn người
rồi ngơ ngác vì chỉ thấy một.

**Bản Editor không nối mạng thật được.** WebRTC chỉ có trên WebGL; trong Unity thì `KenhTrucTiep`
chạy kênh giả lập. Nên bản Editor hiện thẳng một dòng nói điều đó thay vì quay vòng bất tận — một
vòng xoay không bao giờ dừng thì nhìn như treo máy.

**Máu chưa được đồng bộ.** Hai máy thấy nhau chạy, nhưng đánh nhau thì mỗi máy tự tính sát thương
nên máu có thể lệch. Đó là phần còn thiếu của bước 5, để làm sau khi việc nhìn thấy nhau đã chắc.

### Hai người vào hai bản đồ khác nhau

Anh báo: *"Khi đếm ngược 10 giây xong rồi vào game, 2 người chơi vẫn vào 2 map khác nhau chứ không
cùng 1 map."*

#### Nghi sai hai lần trước khi đo

Chỗ nạp màn chỉ có một dòng, và trông rất đáng ngờ:

```csharp
SceneManager.LoadScene(p.manChoi);      // p là bản sao cục bộ, có thể cũ 1 giây
```

Nên tôi nghi chủ phòng đổi màn ngay trước khi bấm bắt đầu, máy khách chưa kịp hỏi lại. Nghi thứ
hai: Act1 dựng bằng code nên hai máy có thể ra hai địa hình khác nhau.

Cả hai đều sai, và **phép đo nói ra điều đó chứ không phải tôi**. Tôi viết một kịch bản đi thẳng
vào Firebase bằng REST — không qua một dòng C# nào — dựng lại đúng tình huống xấu nhất: A tạo
phòng Act2, B vào, A **đổi sang Act1 rồi bấm bắt đầu ngay lập tức**:

```
1. A tạo phòng, manChoi=Act2
2. B vào phòng, B đọc thấy manChoi=Act2
3. A đổi màn sang Act1
4. A bấm bắt đầu, mốc = +10s
   t= 1s  A[màn=Act1 …]  B[màn=Act1 …]
   …
   t= 8s  A[màn=Act1 còn=-0.9 đến=True]  B[màn=Act1 còn=-0.9 đến=True]

KẾT QUẢ: A nạp "Act1" | B nạp "Act1" -> CÙNG MỘT MÀN
```

Đường dữ liệu sạch. (Còn Act1 thì `worldSeed = 12345` cố định, và cảnh đã bake sẵn nên không dựng
lại lúc chạy.)

#### Lỗi thật nằm ở nút "Vào phòng nhanh"

Phép đo thứ hai: cho hai tài khoản bấm nút đó **cùng một lúc** — đúng như hai người ngồi hai máy
đếm "một, hai, ba" rồi cùng bấm. Ba lần liên tiếp:

```
lần 1: A TỰ TẠO phòng -P1Al4K3t9 [Act1] | B TỰ TẠO phòng -P1Al4K6Bb [Act2] -> KHÁC PHÒNG, KHÁC MÀN
lần 2: A TỰ TẠO phòng -P1Al4nxZX [Act2] | B TỰ TẠO phòng -P1Al4n-nk [Act1] -> KHÁC PHÒNG, KHÁC MÀN
lần 3: A TỰ TẠO phòng -P1Al5Hgi3 [Act1] | B TỰ TẠO phòng -P1Al5FpqG [Act1] -> KHÁC PHÒNG, cùng màn
```

**3/3 lần khác phòng.** Hai người không hề ở chung một phòng — mỗi người là chủ phòng của mình,
nên ai cũng có nút bắt đầu, ai cũng đếm ngược, ai cũng vào được màn chơi. Nhìn từ ngoài thì y hệt
"cùng vào một trận mà lạc nhau".

Nguyên nhân là một cuộc đua, và nó xảy ra gần như mọi lần:

1. A bấm → đọc danh sách → rỗng (hoặc chưa thấy phòng của B).
2. B bấm sau 200 ms → cũng đọc danh sách → **vẫn rỗng**, vì phòng của A chưa kịp hiện ra.
3. Cả hai cùng tạo phòng.

Và vì màn khi ấy được **bốc ngẫu nhiên**:

```csharp
string man = UnityEngine.Random.value < 0.5f ? "Act1" : "Act2";
```

nên hai phòng còn ra hai màn khác nhau — 2/3 lần trong phép đo.

#### Cách sửa: hai máy phải tự ra cùng một đáp án

Tạo phòng xong thì **ngó lại một lần** sau 1,2 giây. Nếu thấy phòng khác cũng đang chờ và cũng chỉ
có một người, hai bên cùng tính:

```csharp
public static string PhongDuocGiu(string maA, string maB)
{
    if (string.IsNullOrEmpty(maA)) return maB;
    if (string.IsNullOrEmpty(maB)) return maA;
    return string.CompareOrdinal(maA, maB) <= 0 ? maA : maB;
}
```

Điều quan trọng không phải là *chọn ai*, mà là **hai máy phải ra cùng một đáp án mà không hỏi nhau
câu nào**. Nếu mỗi máy tự quyết theo ý mình thì hoặc cả hai cùng nhường (không ai ở đâu cả), hoặc
cả hai cùng giữ (vẫn hai phòng).

Vì sao so chuỗi là đủ: khoá mà Realtime Database sinh ra (`-P1Al4K3t9…`) có **tám chữ đầu là mốc
thời gian**, viết theo một bảng 64 ký tự mà thứ tự của nó *trùng khớp* với thứ tự mã ASCII. Nên
phòng nào tạo trước thì chuỗi cũng nhỏ hơn — người đang ngồi chờ được giữ phòng, người đến sau
nhường.

Kèm theo ba chỗ nữa:

| Sửa | Vì sao |
|---|---|
| `ManMacDinh = "Act2"` thay cho `Random` | một cái nút mà mỗi lần bấm ra một màn khác thì không ai hiểu chuyện gì đang xảy ra |
| `DoiManChoi` chỉ chạy khi phòng còn `DangCho` | bấm bắt đầu rồi mà còn đổi được thì cửa sổ vài trăm mili giây kia vẫn mở |
| `VaoTran()` đọc lại phòng **một lần nữa** ngay trước `LoadScene` | bản sao trong tay có thể đã một giây tuổi; đọc lại tốn ~50 ms, đổi lại chắc chắn cùng màn |

Và `LoadScene` không còn nhận thẳng `p.manChoi` nữa: đọc ra thứ gì không phải `Act1`/`Act2` thì
về màn mặc định, chứ `LoadScene(null)` ném lỗi và người chơi kẹt lại ở MainMenu không hiểu vì sao.

#### Một dòng chữ nhỏ ở góc màn hình

Câu hỏi đầu tiên khi ai đó nói "tôi vào bản đồ khác" là: *hai máy có đang ở cùng một phòng không?*
Không có gì trả lời được câu ấy — kể cả tôi, nên tôi đã phải đoán hai lần. Giờ màn chơi mạng luôn
hiện ở góc dưới trái:

```
phòng …92Ju · Act2 · chủ phòng · 2 người
```

Chụp hai màn hình là đọc ra ngay: khác mã phòng, hay cùng phòng mà khác màn.

#### Số đo sau khi sửa

Chạy lại đúng cuộc đua ấy, năm lần:

```
lần 1: A NHƯỜNG, sang  xk6nHhi [Act2] | B GIỮ phòng mình xk6nHhi [Act2] -> CÙNG PHÒNG, cùng màn
lần 2: A NHƯỜNG, sang  GGf_Z-T [Act2] | B GIỮ phòng mình GGf_Z-T [Act2] -> CÙNG PHÒNG, cùng màn
lần 3: A GIỮ phòng mình UBo68Pr [Act2] | B NHƯỜNG, sang  UBo68Pr [Act2] -> CÙNG PHÒNG, cùng màn
lần 4: A NHƯỜNG, sang  j0l6Vkz [Act2] | B GIỮ phòng mình j0l6Vkz [Act2] -> CÙNG PHÒNG, cùng màn
lần 5: A GIỮ phòng mình n-CGLv8 [Act2] | B VÀO SẴN       n-CGLv8 [Act2] -> CÙNG PHÒNG, cùng màn

cùng phòng: 5/5
```

**0/3 → 5/5.** Và luôn đúng *một* bên nhường, không bao giờ cả hai.

#### Phép thử trong Unity, và lần nó bắt được chính tôi

Phép đo trên chạy bằng Python nói chuyện thẳng với Firebase — nó chứng minh *thuật toán* đúng,
nhưng không chứng minh *code C#* đúng. Nên có thêm **menu 35**, đo ba tính chất của `PhongDuocGiu`:

```
1. đối xứng trên 2000 cặp -> số cặp lệch: 0
2. đáp án luôn là một trong hai -> số lần ra ngoài: 0
3. phòng tạo trước luôn thắng (515 cặp, mốc đọc thẳng từ mã) -> số cặp sai: 0
4. màn mặc định của 'vào phòng nhanh': Act2
5. phòng đang đếm ngược -> DangCho = False
số lỗi ghi nhận = 0
```

Lần chạy **đầu tiên** phép 3 báo sai một cặp. Tôi đã liệt kê sáu mã phòng thật rồi tự ghi thứ tự
thời gian của chúng theo trí nhớ — và chính thứ tự tôi ghi mới là cái sai, hàm thì đúng. Nên phép
thử được viết lại: nó **tự giải mã mốc thời gian ra khỏi mã phòng** rồi mới đối chiếu, cộng thêm
500 cặp tự chế với mốc biết trước, cách nhau từ 1 ms đến một phút. Không còn chỗ nào để tôi nhét
giả thiết của mình vào.

### Cùng phòng, cùng màn, mà vẫn không thấy nhau

Sửa xong phần trên, anh thử lại: hai người **đã vào đúng một phòng**, và ảnh chụp cho thấy cả hai
đều đang ở nghĩa địa — **đúng một màn**. Nhưng vẫn hai trận riêng biệt, không thấy nhau.

Ảnh chụp nói ra chỗ hỏng, không phải bằng cái nó cho thấy mà bằng cái nó **không** cho thấy: góc
dưới trái **trống trơn**. Dòng nhận dạng vừa thêm ở mục trước không hề xuất hiện — nghĩa là
`KhoiDongTranMang` chưa bao giờ được dựng dậy.

#### `[RuntimeInitializeOnLoadMethod]` chạy đúng một lần

Bộ nối mạng tự đặt mình vào màn chơi thay vì bắt kéo tay vào scene — vì Act1 dựng bằng code còn
Act2 là scene nướng sẵn, gắn tay thì phải nhớ cả hai. Cách gắn là:

```csharp
[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
static void TuGan()
{
    if (!TranHienTai.DangChoiMang) return;
    …
}
```

Cái tên `AfterSceneLoad` đọc lên nghe như "sau **mỗi** lần nạp màn". Không phải: nó chạy **đúng một
lần**, lúc game vừa khởi động, sau khi scene **đầu tiên** nạp xong. Lúc ấy người chơi còn đang ở
MainMenu, `DangChoiMang` vẫn là `false`, nên hàm thoát ra ngay dòng đầu — **và không bao giờ quay
lại**. Nạp Act2 sau đó thì không ai dựng bộ nối mạng cả.

Sửa: phần việc của nó chỉ là **đăng ký một lần**, rồi để `sceneLoaded` gọi lại sau mỗi lần nạp màn.

```csharp
[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
static void DangKyNgheNapMan()
{
    SceneManager.sceneLoaded -= KhiNapXongMan;
    SceneManager.sceneLoaded += KhiNapXongMan;
    KhiNapXongMan(SceneManager.GetActiveScene(), LoadSceneMode.Single);   // màn đang mở
}
```

Dòng cuối là cho trường hợp ai đó vào thẳng Act2 mà không qua MainMenu (chạy thử trong Editor
chẳng hạn) — khi ấy không có lần nạp nào để mà nghe.

#### Vì sao lỗi này sống sót qua cả năm bước đo

Năm bước của giai đoạn 2 đều được đo riêng, và đều đạt. Nhưng **không phép đo nào đi qua đường
người chơi thật đi**: từ MainMenu, bấm bắt đầu, nạp màn chơi. Các phép thử trước đều mở thẳng Act2
rồi đo — mà mở thẳng Act2 thì `RuntimeInitializeOnLoadMethod` chạy đúng lúc Act2 vừa nạp, nên nó
hoạt động. Chỉ khi **đi từ MainMenu sang** lỗi mới hiện ra.

Nên **menu 36** đi đúng con đường đó: vào Play ở MainMenu, bật `DangChoiMang`, `LoadScene`, rồi
**đếm** xem trong cảnh có bộ nối mạng không.

```
--- màn Act2 ---
1. KhoiDongTranMang trong cảnh: KHÔNG CÓ
[LỖI] không ai dựng dậy bộ nối mạng - hai máy không thể tìm thấy nhau
2. dòng nhận dạng trên màn hình: "(rỗng)"
[LỖI] màn hình không nói gì cả - đúng như ảnh chụp của người chơi
số lỗi ghi nhận = 2
```

Sau khi sửa, chạy lại **cả hai màn**:

```
--- màn Act2 ---
1. KhoiDongTranMang trong cảnh: có
2. dòng nhận dạng: "phòng …tu-gan · Act2 · chủ phòng"
3. bước tiếp theo: "Bản này chạy trong Unity Editor nên chưa nối mạng thật được…"
--- màn Act1 ---
1. KhoiDongTranMang trong cảnh: có
2. dòng nhận dạng: "phòng …tu-gan · Act1 · chủ phòng"
3. bước tiếp theo: "Bản này chạy trong Unity Editor nên chưa nối mạng thật được…"
số lỗi ghi nhận = 0
```

Chiều thứ 3 không thừa: chỉ đếm "có bộ nối mạng" thì chưa biết nó có **chạy được** không. Dòng nói
"bản Editor chưa nối mạng thật được" chứng minh nó đã đi qua đoạn tìm nhân vật của mình — chỗ dễ
hỏng ngay sau đó.

Và vì vật thể chạy phép thử phải sống qua lần nạp màn, nó cần `DontDestroyOnLoad`; không thì
coroutine đứt ngang giữa chừng và phép thử im lặng không ghi gì, nhìn hệt như "chạy xong mà không
có kết quả".

#### Một chỗ nghi oan

Trước khi build lại, tôi ngờ cầu nối WebRTC (`CauNoiWebRTC.jslib`) không được nhúng vào bản web —
file `.meta` của nó chỉ có hai dòng, không có phần `PluginImporter` như plugin thường thấy. Kiểm
bằng cách giải nén thẳng bản build ra mà tìm:

```
RTC_Tao              CÓ
RTC_TaoLoiMoi        CÓ
RTC_Gui              CÓ
themUngVien          CÓ
xaHangUngVien        CÓ
RTCPeerConnection    CÓ
```

Đủ cả. Meta hai dòng là chuyện bình thường của Unity 6 với `.jslib`.

### Thấy nhau chạy, nhưng đánh nhau thì không

Anh thử lại: **hai người đã thấy nhau, thấy nhau chạy, thấy nhau bị quái giết**. Nhưng một bên
tung phép thì bên kia không thấy gì, không mất máu, không dính hiệu ứng nào — bật khiên lên người
kia cũng không thấy cái khiên.

Đây đúng là phần tôi đã ghi là *"còn thiếu trong bước 5"*, và lý do rất đơn giản: **gói tin không
chở kỹ năng**. Nó chỉ có vị trí, góc, máu, đang chạy, đã chết. Không có lấy một bit nào nói rằng
"tôi vừa tung phép". Còn máu thì có gửi, nhưng bên nhận chưa bao giờ đọc đến.

Gỡ ra được **bốn lỗi chồng lên nhau** — và ba trong số đó chỉ lộ ra sau khi sửa lỗi trước.

#### Lỗi 1: không có gói kỹ năng

Thêm loại gói thứ ba, 14 byte: ai tung, phép nào, số thứ tự, ngắm vào đâu.

Vì sao phải là gói **riêng** chứ không nhét vào gói trạng thái: trạng thái gửi 60 lần mỗi giây và
**được phép mất** — mất một gói thì 17 ms sau đã có gói mới. Tung phép thì khác: nó xảy ra đúng
một lần, mất là mất hẳn, người kia sẽ thấy đám lửa nổ mà không hiểu từ đâu. Nên gói kỹ năng được
gửi **lặp ba lần**, và bên nhận bỏ bản sao theo số thứ tự.

#### Lỗi 2: bản sao người khác đứng niệm chú vĩnh viễn

Sửa xong lỗi 1, gói đã sang tới nơi, nhưng vẫn không có gì xảy ra. Bản sao nhận lệnh, vào thế niệm
chú — rồi đứng im mãi mãi.

```csharp
if (tuDocInput && boDoc != null) input = boDoc.Doc(dt);
ThiHanhMotKhung(input);        // input.dt van la 0
```

Bản sao của người khác không đọc bàn phím (`tuDocInput = false`), nên nó giữ nguyên gói ý muốn cũ
— mà gói ấy có `dt = 0`. Cả `ThiHanhMotKhung` chạy với `dt = 0`: đồng hồ niệm chú không giảm, hồi
chiêu không chạy, **phép không bao giờ bay ra**. Thêm một dòng `else input.dt = dt;` là xong.

#### Lỗi 3: quả cầu lửa bay xuyên qua người

Giờ ba quả cầu thật sự bay ra — đếm được trên cảnh — mà đối phương vẫn mất **đúng 0 máu**.

Chia đôi bài toán bằng hai phép đo:

```
3b. nổ thẳng một cú ngay trên đầu mình với mask của người kia -> mình mất 37 máu
3c. người kia tung THIÊN THẠCH vào chỗ mình              -> mình mất 83 máu
```

Sát thương ăn được, và đường mạng thông hoàn toàn — thiên thạch từ máy kia gây 83 máu. Vậy lỗi
nằm riêng ở **đường bay của quả cầu**:

```csharp
Physics.SphereCast(from, bodyRadius, dir, out hit, step + 0.05f, hitMask, …)
```

`hitMask` là `obstacleMask` — gồm `Enemy`, `Ground`, `Default`. **Không có `Player`.** Nên quả cầu
bay xuyên thẳng qua người rồi nổ ở đâu đó phía sau. Đo được: quả gần nhất chỉ cách người 1,67 m.

Cách sửa đầu tiên của tôi — nhét `Player` vào `obstacleMask` — **hỏng**: quả cầu sinh ra ngay bên
trong collider của chính người tung, nên nó nổ trên đầu họ. Cách đúng là để quả cầu tự hỏi một câu
khác: *trong tầm ăn đòn có ai không phải người tung không?* Câu hỏi ấy đúng cho cả quái lẫn người.

#### Lỗi 4: quả cầu nhảy qua người khi khung hình tụt

Sửa xong lỗi 3, chạy lần một: **46 máu**. Chạy lần hai: **0 máu**.

Con số nhảy như thế không phải ngẫu nhiên của chùm ba quả — đó là dấu hiệu của **tunneling**. Quả
cầu đi `speed × dt` mỗi khung; khung hình tụt một cái là nó nhảy qua người mà không chạm vào đâu,
vì tôi đang hỏi tại **một điểm** chứ không quét **cả đoạn đường**. Đổi `OverlapSphere` thành
`SphereCastAll` trên đoạn vừa bay.

Ba lần chạy liên tiếp sau khi sửa: **48, 48, 48 máu**.

#### Trọng tài phán xử máu: mỗi máy tự xử chính mình

Quy ước đã chọn: **mỗi máy là trọng tài của chính nhân vật mình**. Phép của người kia bay sang đây,
trúng nhân vật của tôi, thì **máy tôi** trừ máu rồi báo sang — máy kia chỉ hiển thị con số ấy.
Ngược lại cũng vậy.

Vì sao không cho mỗi máy tự tính cả hai bên: `CombatUtil.AreaFreeze` và `AreaShock` dùng
`Random.value`, mỗi máy gieo riêng — người này thấy địch đóng băng, người kia thấy không. Và sát
thương tính theo vị trí, mà vị trí của người kia ở đây luôn trễ hơn bên đó vài chục mili giây.

Máu nhận được đặt thẳng vào `health`, không gọi `TakeDamage`: `TakeDamage` còn bắn ra hiệu ứng
trúng đòn và tự tính lại sống chết — làm hai lần thì giật cả hai đầu.

#### Đo (menu 37), tám chiều

```
1. lớp của nhân vật mình: Player
2. gói kỹ năng 14 byte, đọc lại khớp, lệch điểm ngắm = 0,0000 m
3. người kia tung cầu lửa vào chỗ mình -> mình mất 48 máu (400 -> 352)
4. người tung phép mất 0 máu
5. gửi lại đúng gói ấy hai lần nữa -> mất thêm 0 máu, số gói bỏ vì trùng = 2
6. máy kia báo còn 50% máu -> bản sao bên này: 50%
7. mình tung sấm sét -> số gói kỹ năng gửi đi: 3
8. người kia bật khiên -> số khiên trên cảnh: 0 -> 1
số lỗi ghi nhận = 0
```

Phép thử đi **đúng đường thật**: nó nhét gói vào hàng nhận của kênh (`GiaLapNhan`) rồi để chính
`DongBoTran` đọc ra, chứ không gọi tắt vào hàm bên trong — gọi tắt thì không kiểm được phần phân
loại gói, chỗ dễ hỏng nhất khi thêm một loại gói thứ hai.

Chiều 7 không thừa: chiều 3 chỉ chứng minh **nhận** được. Nhận được mà không **gửi** được thì
người kia vẫn không thấy gì.

#### Và một lần phép đo tự nói dối

Lần chạy đầu tiên báo hai lỗi: "gửi lại gói cũ mà vẫn mất thêm 81 máu" và "máu áp vào ra 42% thay
vì 50%". Cả hai đều là **lỗi giả**: Act2 có 25–33 con quái, và chúng vẫn cắn người chơi trong lúc
phép thử đang chờ. Máu tụt vì bị quái cắn thì không nói lên điều gì về mạng cả.

Dọn sạch quái trước khi đo, và chờ hết hiệu ứng cháy trước khi chốt mốc — hai "lỗi" ấy biến mất,
để lộ ra lỗi thật ở mục 3 mà chúng đang che.

#### Còn thiếu

- **Quái chưa đồng bộ.** Mỗi máy tự rải quái của mình, nên hai người đang đánh hai đàn quái khác
  nhau. Việc này nằm ngoài phần "đánh nhau" và chưa làm.
- **Bù trễ khi tính trúng.** Ở 28 ms, người chạy ngang vẫn né được đòn mà trên màn hình người bắn
  thấy trúng rõ ràng.

### Chung một đàn quái, và đòn trúng theo cái người bắn nhìn thấy

Hai việc còn lại của giai đoạn 2, làm cùng một lượt vì cả hai đều là chuyện "ai là trọng tài".

#### Đàn quái: chủ phòng kể, khách nghe

Trước đây mỗi máy tự rải quái của mình. Hai người đứng cạnh nhau mà đánh hai đàn hoàn toàn khác
nhau — đó chính là lý do **cả hai màn hình cùng ghi "Quái còn lại: 33" một cách độc lập**, và giết
một con thì chỉ mình mình thấy nó ngã.

Quy ước mới:

| | Chủ phòng | Máy khách |
|---|---|---|
| Rải quái | có | **không** |
| Chạy AI | có | **không** (tắt hẳn `EnemyAI`) |
| Tính máu quái | có | không — lấy con số nghe được |
| Nhịp đếm sinh thêm quái | có | không |

Vì sao không cho mỗi máy tự tính: AI quái chọn mục tiêu, đổi hướng, ra đòn theo đồng hồ riêng của
từng máy — chỉ lệch vài mili giây là hai bên rẽ hai hướng khác nhau, và càng chạy càng xa nhau.

Mỗi con quái được cấp một **số hiệu** (`NhanDangQuai`) để hai máy gọi cùng một tên; thiếu nó thì
không cách nào nói "con ở góc kia vừa mất 40 máu", vì thứ tự trong danh sách đổi mỗi khi có con
chết. Số hiệu phải gán ở **mọi** chỗ sinh quái — bỏ sót một chỗ là những con sinh ra ở đó vĩnh viễn
vô hình trên máy khách mà không ai báo gì.

**Sát thương của máy khách vẫn vào được quái**, và nó đi đường vòng: gói kỹ năng sang chủ phòng,
bản sao của người khách bên đó tung đúng phép ấy, phép trúng con quái **thật**. Nên không cần gửi
riêng một gói "tôi vừa đánh con số 7".

Con nào lâu không được nhắc đến thì máy khách gỡ đi. Im lặng là cách rẻ nhất để nói một con quái
không còn nữa, và nó chịu được cả trường hợp mất gói.

#### Bù trễ: lùi nạn nhân về đúng khoảnh khắc người kia bấm

Vấn đề: A nhìn thấy B **trễ** một khoảng — độ trễ đường truyền cộng đệm nội suy, đo được 82–131 ms.
A ngắm vào chỗ A *thấy* B đang đứng rồi bấm. Gói tin bay sang máy B mất thêm một lát nữa. Đến lúc
máy B tính trúng thì B đã chạy tiếp hơn một mét — đòn trượt, dù trên màn hình của A nó trúng rõ
ràng.

Cách chữa, giống hệt các game bắn súng: máy B **lùi B về quá khứ** đúng bằng khoảng A đã trễ, tính
trúng ở đó, rồi trả B về chỗ cũ. Tất cả gọn trong một khung hình, không render ở giữa, nên không ai
thấy nhân vật nhảy.

Ba mảnh:

- `LichSuViTri` — vòng đệm cố định, nhớ một giây vừa qua. Ghi ở `LateUpdate` (lúc nhân vật đã đi
  xong trong khung ấy); ghi ở `Update` thì mốc luôn chậm một khung so với cái mắt nhìn thấy.
- `BuTre` — mở cửa sổ: lùi mọi nhân vật có lịch sử, **bỏ qua người tung** (họ ở đúng chỗ họ muốn
  rồi), gọi `Physics.SyncTransforms()`, tính trúng, rồi trả lại. Thiếu `SyncTransforms` thì
  `OverlapSphere` vẫn thấy nhân vật ở chỗ cũ và cả việc lùi thành vô nghĩa mà không báo gì.
- **Gói nhịp** 6 byte để đo vòng đi-về thật. Mốc thời gian được ném trả lại *nguyên vẹn*, nên hai
  đầu trừ đều là giờ của **cùng một máy** — đồng hồ hai bên lệch bao nhiêu cũng không ảnh hưởng.
  Làm mịn bằng trung bình trượt: một gói kẹt mạng đẩy RTT lên 300 ms trong đúng một nhịp, tin ngay
  con số ấy thì cú đòn kế tiếp bù gấp ba.

Đạn bay cần cách riêng. Lùi nạn nhân chỉ cứu được đòn tính trúng **ngay**; quả cầu lửa còn phải bay
một đoạn, lúc nó tới nơi thì cửa sổ lùi đã đóng từ lâu. Nên quả cầu sinh ra từ một phép đến từ mạng
được **tua tới trước** đúng bằng thời gian nó đã mất để bay sang đây — chia nhỏ từng bước, vì nhảy
một phát thì nó xuyên qua cả tường lẫn người.

**Giới hạn cứng 300 ms.** Cái giá của mọi hệ bù trễ là "chết sau góc tường": người đã nấp sau vật
cản rồi vẫn có thể ăn một đòn bắn từ 100 ms trước. Đó là đánh đổi giữa thưởng cho người bắn hay
thưởng cho người chạy; các game bắn súng đều chọn người bắn, vì họ là người vừa thao tác và sẽ thấy
vô lý nếu đòn của mình không ăn. Nhưng bù quá nửa giây thì sinh ra những cảnh không ai giải thích
nổi — mạng tệ đến thế thì thà chịu trượt.

#### Đo (menu 38), mười hai chiều

```
 1. được tự rải quái? chơi đơn=True chủ phòng=True khách=False
 2. quái trong cảnh: 25 con, có số hiệu: 25, số hiệu trùng nhau: 0
 3. gói 3 con = 45 byte (13 byte/con), đọc lại khớp, lệch vị trí 0,0000 m
 4. cả đàn 25 con = 337 byte mỗi lượt, 10 lượt/giây -> 3,3 KB/giây
 5. đi 6,00 m trong 0,4 giây, hỏi lại chỗ 0,4 giây trước -> lệch 62 ms (trần: hai khung = 95 ms)
 6. mở cửa sổ bù trễ -> người bị lùi 3,56 m; đóng lại -> sai lệch 0,0000 m
 7. bù trễ nhưng BỎ QUA chính người này -> họ bị lùi 0,0000 m
 8. bảo lùi 5 giây -> thực tế lùi 0,30 giây (trần)
 9. gói kỹ năng 16 byte, độ trễ viết 137 ms -> đọc lại 137 ms
10. gói nhịp 6 byte: hỏi(4242) -> đáp(4242)
11. chạy ngang 3,00 m rồi nổ vào CHỖ CŨ (lùi 344 ms):
    a) không bù trễ -> mất 0 máu (đòn trượt)
    b) CÓ bù trễ    -> mất 32 máu (đòn trúng)
12. máy khách nghe gói 2 con -> quái trong cảnh 33 -> 35, AI đã tắt: True, máu áp vào: 50%
số lỗi ghi nhận = 0
```

**Mục 11 là bằng chứng chính**, và nó phải có *cả hai nửa*. Chỉ đo "có bù thì trúng" thì không phân
biệt được *bù đúng* với *đòn này vốn trúng sẵn*. Phải chứng minh rằng cùng cú nổ ấy, ở cùng chỗ ấy,
**không** bù thì **trượt**.

#### Hai lần phép đo tự nói dối, và cách chữa

Lần chạy đầu báo hai lỗi, cả hai đều nằm ở phép đo chứ không ở code:

**Mục 5** báo "lệch 1,48 m, quá ngưỡng 1,0 m". Nhưng 1,48 m ở tốc độ 15 m/giây đúng bằng **một
khung hình** khi Editor tụt xuống 10 khung/giây — lịch sử không thể nhớ mịn hơn nhịp nó được ghi.
Ngưỡng tính bằng mét là sai từ gốc; đổi sang quy sai lệch ra **giây** và đòi dưới hai khung hình.

**Mục 11** báo "bù trễ không biến được đòn trượt thành đòn trúng". Hoá ra tôi viết cứng "lùi 0,25
giây" cho cả hai phép, rồi lại chờ thêm 0,2 giây giữa chúng — nên lùi 0,25 giây chỉ về được đến lúc
đã đứng ở chỗ **mới**. Bù trễ chưa hề được thử. Sửa: nhớ mốc thời gian lúc còn ở chỗ cũ, lùi đúng
bằng khoảng đã trôi kể từ mốc ấy, và đo hai phép liền nhau không chờ ở giữa (`AreaDamage` trừ máu
ngay trong lời gọi).

#### Còn thiếu

Quái được đồng bộ **vị trí và máu**, nhưng hiệu ứng hình ảnh của chúng thì chưa: máy khách thấy con
quái đứng đúng chỗ và đúng máu, còn cú vung kiếm hay quả thiên thạch mà quỷ dữ gọi xuống thì vẫn do
máy khách tự đoán. Chưa ai báo là khó chịu nên để nguyên.

### Đòn của quái — và một lỗ hổng chính tôi vừa tạo ra

Anh bảo làm nốt hiệu ứng hình ảnh của quái. Nhưng khi mở code ra thì hoá ra việc đó không phải
chuyện trang trí: **chính bản sửa "chung một đàn quái" của tôi đã làm người khách bất tử**.

Lý do nằm gọn trong hai câu:

- Trên máy **chủ phòng**, con quái đánh vào *bản sao* của người khách. Bản sao ấy có mất máu, nhưng
  máu của nó bị gói tin từ máy khách đè lên **60 lần mỗi giây** — sát thương biến mất không dấu vết.
- Trên máy **khách**, quái chỉ là bản sao và AI đã tắt, nên nó không đánh ai cả.

Đo lại cho chắc trước khi sửa, dựng đúng tình huống ấy:

```
1. quái đánh bản sao: 400 -> 360 máu, rồi một gói tin từ máy kia đến -> 400 máu
   => sát thương bị xoá sạch: True
```

Không ai báo gì. Không lỗi, không cảnh báo — chỉ là người khách không bao giờ chết vì quái nữa.

#### Một gói lo cả hai việc

Gói "quái ra đòn", 15 byte: con nào, kiểu đòn (đánh gần / ném phép / thiên thạch / tia sét), nhắm
vào ai, số thứ tự, điểm ngắm. Chủ phòng kể; máy khách **diễn lại** — và diễn lại thì có luôn cả
hình ảnh lẫn sát thương, đúng quy ước *mỗi máy là trọng tài của chính nhân vật mình*:

- Đòn nhắm vào nhân vật của máy này → trừ máu **thật**.
- Đòn nhắm người khác → chỉ hình ảnh, vì máu của họ do máy họ quyết.

Ba kiểu đánh xa vốn đọc thẳng `target.position`, mà bản sao bên khách thì không theo dõi ai cả —
nên mỗi hàm được tách làm hai: bản cũ dùng `target`, bản mới nhắm vào **một điểm**. Cả hai đường
dùng chung một khối code, nên sửa sát thương hay tốc độ ở một chỗ là hai máy cùng đổi, không thể
lệch nhau.

Gói được gửi lặp ba lần như gói kỹ năng — ra đòn xảy ra đúng một lần, mất là mất hẳn — và bên nhận
bỏ bản sao theo số thứ tự.

#### Hai chỗ tôi làm cẩu thả, và cái giá của chúng

**Sự kiện không mang theo người phát.** Ban đầu tôi để `DaRaDon(kiểu, mục tiêu, điểm ngắm)` rồi bên
nghe đi **quét cả cảnh** đoán xem "con nào vừa đánh". Vừa chậm vừa sai: hai con đánh trong cùng một
khung hình là đoán nhầm ngay. Sự kiện phải mang theo chính con quái phát ra nó.

**Ghi tên nghe theo nhịp thay vì ngay lúc sinh.** Tôi cho quét mỗi nửa giây để đăng ký nghe những
con mới. Phép thử bắt ngay:

```
3. chủ phòng: quái ra đòn -> số gói đòn quái gửi đi: 0 (phải ít nhất 1)
```

Con quái sinh giữa trận sẽ **im lặng suốt nửa giây đầu** — nó vung kiếm mà máy kia không nghe thấy
gì. Sửa: `GameDirector.DanhSo` là đường mà mọi con quái đều đi qua, ghi tên nghe ngay tại đó; nhịp
quét giữ lại làm lưới an toàn.

#### Đo (menu 39), sáu chiều

```
1. quái đánh bản sao: 400 -> 360 -> 400  => sát thương bị xoá sạch: True
2. gói đòn quái 15 byte, đọc lại khớp, lệch điểm ngắm 0,0000 m
3. chủ phòng: quái ra đòn -> số gói gửi đi: 6
4. máy khách nghe "quái ra đòn" -> mình mất 12 máu
5. cùng gói ấy đến thêm hai lần -> mất thêm 0 máu
6. đòn nhắm NGƯỜI KIA -> mình mất 0 máu
số lỗi ghi nhận = 0
```

Chiều 1 giữ nguyên trong phép thử dù lỗi đã sửa — nó không kiểm cái gì hỏng, nó **ghi lại vì sao
cần gói này**. Bỏ nó đi thì người đọc sau sẽ tưởng đây chỉ là chuyện hiệu ứng cho đẹp.

#### Lại một lần phép đo nhiễu

Sau khi sửa, menu 37 (kỹ năng qua mạng) đang 0 lỗi bỗng báo hỏng: cầu lửa gây 0 máu. Nhưng con số
chẩn đoán chỉ thẳng chỗ — quả cầu dừng ở **4,33 m**, tức chỉ bay được 0,7 m khỏi tay người tung.

Nó nổ vào một con quái đứng chắn giữa. Phép thử dọn quái **ngay khi vừa tìm thấy nhân vật**, mà lúc
ấy `GameDirector` chưa rải xong hai dòng quái riêng — chúng ra đời sau đó. Sửa: đợi 2 giây cho rải
xong rồi mới dọn, dọn thêm một lượt nữa, và dọn lần cuối ngay trước phép đo. Sau đó: **48 máu**,
0 lỗi.

### Máu 30 000 để chạy thử — và con số ấy nằm ở bốn chỗ

> **Cập nhật 12/09/2026: máu chính thức là 600.** Anh chốt mức 600 thay cho 30 000 của lúc chạy thử. Vẫn đúng bốn chỗ
> ấy, sửa bằng đúng cách kể dưới đây; menu 40 đo lại ở **cả hai màn**: nhân vật của mình 600/600, bản sao người chơi
> khác 600/600 (lấy thẳng từ prefab), ăn một đòn 250 còn 350, chữ "600 / 600" rộng 46 điểm trong thanh máu rộng 158
> điểm — **0 lỗi**. Hai scene ghi `maxHealth` là 400 nhưng `GameBootstrap` đè 30 000 lúc chạy: đúng cái bẫy mục này kể.

Anh muốn nhân vật vào màn có 30 000 máu thay vì 400, để thử game cho lâu.

Nghe như sửa một dòng. Thật ra con số ấy nằm ở **bốn chỗ**, và sửa thiếu một chỗ thì hoặc không đổi
gì cả, hoặc đổi nửa vời theo kiểu khó hiểu:

| Chỗ | Ảnh hưởng ai |
|---|---|
| `GameBootstrap.playerMaxHealth` | mặc định trong code |
| `Act1.unity` và `Act2.unity` | **đè lên** mặc định trên — Unity lưu giá trị component vào scene |
| `Player_Sorceress.prefab` | bản sao của người chơi khác, nó không đi qua `GameBootstrap` |
| `GameBootstrap.EnsurePlayer` | máu **hiện tại** lúc vào màn |

Chỗ thứ tư là chỗ phép thử bắt được, và nó là chỗ khó đoán nhất:

```
1. nhân vật của mình: 400 / 30000
[LỖI] vào màn mà không đầy máu
```

Dòng cũ chỉ vá khi giá trị **bất thường**:

```csharp
if (hp.health <= 0f || hp.health > playerMaxHealth) hp.health = playerMaxHealth;
```

400 thì không âm, cũng không vượt 30 000 — nên nó được giữ nguyên. Kết quả: thanh máu tối đa
30 000 mà máu hiện tại 400, tức một vạch đỏ bằng đầu đũa. Đổi thành *vào màn là đầy máu, không hỏi
han*.

#### Đo (menu 40), bốn chiều, cả hai màn

```
1. nhân vật của mình: 30000 / 30000
2. bản sao người chơi khác: 30000 / 30000 (lấy thẳng từ prefab)
3. ăn một đòn 250 -> mất 250 máu, còn 29750
4. chữ "30000 / 30000" rộng 56 điểm, thanh máu rộng 172 điểm
số lỗi ghi nhận = 0
```

Chiều 2 không thừa: bản sao người chơi khác lấy máu **thẳng từ prefab**, không qua `GameBootstrap`
— thiếu chiều này thì sửa prefab hay không cũng chẳng ai biết. Chiều 3 giữ cho "máu lớn" không
biến thành "máu không trừ được". Chiều 4 hỏi cái mắt sẽ nhìn thấy: `30000 / 30000` dài gần gấp rưỡi
`400 / 400`, nó có tràn ra ngoài thanh không.

> **30 000 là con số CHẠY THỬ, không phải cân bằng game.** Mức thật là 400. Đổi lại trước khi phát
> hành, không thì quái đánh cả buổi không hết một thanh máu.

#### Ba lần tự làm khó mình

**`GUI.skin` ngoài `OnGUI`.** Chiều 4 lặng lẽ biến mất khỏi báo cáo — không lỗi, không dòng nào.
`GUI.skin` chỉ sống trong `OnGUI`; gọi ngoài đó thì ném, mà ngoại lệ trong coroutine bị Unity cắt
im lặng phần còn lại. Thay bằng font dựng sẵn (`LegacyRuntime.ttf`) và bọc `try/catch` để nó **tự
khai lỗi ra giấy** thay vì biến mất.

**Console đầy lỗi hạt che mất.** Tôi đọc console để tìm ngoại lệ, nhưng tám dòng
*"Particle Velocity curves must all be in the same mode"* chiếm hết chỗ. Đọc console mà không lọc
thì dễ kết luận "không có lỗi nào".

**Và lỗi ngớ ngẩn nhất: tôi đọc báo cáo của lần chạy trước.** Sau khi đổi tên menu để kiểm tra
assembly, tôi đọc `mau_khoi_dau.txt` mà **quên chạy lại** — file vẫn là kết quả cũ, thiếu chiều 4,
và tôi suýt kết luận Unity chạy assembly cũ. Bài học: sau khi sửa phép thử, kiểm dấu thời gian của
file kết quả, đừng tin nội dung của nó.

### Người khác và cả đàn quái đều "đang bay"

Anh báo, và mô tả rất chính xác:

- **Chủ phòng** thấy quái đi lại bình thường, nhưng người chơi kia thì **trượt trên mặt đất** như
  đang bay — chân không nhúc nhích.
- **Khách** thấy *cả đàn quái lẫn người kia* đều bay.

#### Tôi đoán sai lần đầu, và phép thử bác lại

Giả thiết đầu của tôi: bản sao được đặt thẳng vị trí, không đi qua `CharacterController`, nên
`cc.velocity` luôn bằng 0 — mà **mọi** bộ hoạt hình trong game đều đọc tốc độ từ đó.

Nghe rất hợp lý. Nên tôi mở một đường "ép tốc độ từ bên ngoài" cho hai bộ hoạt hình, rồi tính tốc
độ thật bằng quãng đường chia thời gian và đẩy vào. Bốn chiều đo đều đạt.

Rồi tôi thêm chiều thứ năm để phép thử **không tự lừa mình** — chứng minh rằng nguồn cũ đúng là đã
chết:

```
5. trong lúc bản sao đang chạy: cc.velocity = 1.35 m/giây (nguồn cũ, phải là 0)
[chú ý] cc.velocity khác 0 - phép thử này không còn nói lên điều gì
```

**1,35 chứ không phải 0.** Giả thiết của tôi sai, và nếu không có chiều 5 thì tôi đã báo xong với
một bản sửa vá đúng chỗ nhưng vì lý do sai — thứ sẽ vỡ lại ở lần đổi tiếp theo.

#### Nguyên nhân thật: bản sao vẫn tự chạy vòng di chuyển

`PlayerController.HandleMovement` chạy cho **mọi** nhân vật, kể cả bản sao của người khác — với ý
muốn rỗng. Nó làm hai việc hại, cả hai đều im lặng:

1. `cc.Move(velocity * dt)` kéo bản sao theo trọng lực và va chạm của **máy này**, đánh nhau với vị
   trí vừa đặt từ gói tin.
2. Dòng cuối hàm đặt nhịp bước bằng `velocity.xz / moveSpeed` — mà `velocity.xz` của bản sao luôn
   bằng 0. Nó **ghi đè con số vừa tính, mỗi khung hình**.

Cái thứ hai mới là thủ phạm: dù `DongBoTran` có tính đúng tốc độ đến đâu, `HandleMovement` cũng đặt
lại về 0 ngay sau đó. Sửa một dòng:

```csharp
void HandleMovement(float dt)
{
    if (!tuDocInput) return;   // bản sao không tự đi
    ...
```

Niệm chú, hồi chiêu và mọi thứ khác vẫn chạy — chỉ riêng phần tự đi là không.

#### Vẫn cần đường ép tốc độ

Sửa trên mới chỉ *ngừng phá*. Bản sao vẫn không có `cc.velocity` đáng tin, nên vẫn phải nói cho bộ
hoạt hình biết nó đang đi nhanh bao nhiêu — bằng chính quãng đường nội suy vừa đi chia cho thời
gian. Không dùng cờ `dangChay` trong gói tin làm nguồn: cờ ấy chỉ nói *có/không*, mà bước chân cần
biết **nhanh bao nhiêu** thì mới khớp nhịp.

Đường ép phải mở cho **cả hai kiểu** bộ hoạt hình — nhân vật dựng bằng code (`ProceduralAnimator`)
và nhân vật/quái dựng từ model sẵn (`NguoiChoiHoatHinh`, `ModelHoatHinh`). Bỏ sót một kiểu thì một
nửa đàn quái vẫn bay.

#### Đo (menu 41), năm chiều

```
1. người kia chạy (30 gói) -> nhịp bước bộ hoạt hình nhận được: 0.54
2. người kia dừng lại -> nhịp bước: 0.00
3. quái đi bên máy khách -> nhịp bước: 0.30
4. quái dừng lại -> nhịp bước: 0.00
5. trong lúc bản sao đang chạy: nhịp bước giữ được = 0.14
   (cc.velocity của bản sao: 0.00 m/giây - chỉ để tham khảo, nó không còn là nguồn nữa)
số lỗi ghi nhận = 0
```

Chiều 2 và 4 không thừa: chỉ đo "đi thì có bước" mà không đo "dừng thì hết bước" là để lọt cái lỗi
ngược lại — nhân vật đứng yên mà chân vẫn đạp mãi.

#### Ba lần phép đo nhiễu trong cùng một buổi

Sửa xong, ba phép thử cũ báo hỏng — và cả ba đều là nhiễu, không phải hỏng thật:

**Menu 37, chiều 5** báo "một cú bấm ra nhiều lần sát thương". Thật ra vũng lửa của thiên thạch ở
phép 3c vẫn đang cháy. Tôi từng vá bằng "chờ 6 giây cho DOT tan" — vẫn sai. Sửa đúng: **chờ đến khi
máu ngừng tụt** (đứng yên trọn một giây), không đoán một con số giây.

**Menu 38, chiều 5** báo lệch 41 ms với trần 40 ms — sát nút, và cái sát nút ấy chỉ nói rằng tôi
đếm thiếu một nguồn sai số. Có **ba** nguồn, mỗi cái một khung hình: khung lúc *ghi* mốc, khung lúc
*đọc* lại, và dao động của chính nhịp khung hình trong quãng đo. Trần đúng là ba khung.

**Menu 37, chiều 3** dao động 48 → 0 → 48 giữa các lần chạy. Số đo chẩn đoán chỉ thẳng: quả cầu
dừng cách người tung 0,7 m — nó nổ vào một con quái sinh sau lúc phép thử dọn. Đã sửa ở lần trước
bằng cách đợi `GameDirector` rải xong rồi mới dọn.

### Máy tính bị bắt chơi bằng joystick ảo

Anh báo: mở game bằng trình duyệt trên máy tính để bàn mà vẫn bị đưa vào chế độ cảm ứng — joystick
ảo che mất màn hình, chuột và bàn phím thành vô dụng.

Thủ phạm là một dòng trông rất hợp lý:

```csharp
if (Application.platform == RuntimePlatform.WebGLPlayer && Input.touchSupported)
    return true;
```

`Input.touchSupported` trả về **TRUE trên gần như mọi máy tính Windows đời mới**. Chrome khai báo
theo API của hệ điều hành chứ không theo việc máy có màn hình cảm ứng hay không. Nên câu hỏi ấy
không phân biệt được gì cả.

Cũng không hỏi được `Application.isMobilePlatform`: trên WebGL nó trả về **FALSE kể cả khi đang
chạy trên điện thoại thật**. Hai câu hỏi có sẵn của Unity, một cái luôn đúng, một cái luôn sai.

#### Hỏi thẳng trình duyệt

Thêm `CauNoiThietBi.jslib` với bốn mảnh lọc, xếp theo thứ tự tin cậy giảm dần:

1. `navigator.userAgentData.mobile` — Chrome/Edge khai báo thẳng. Nhưng **chỉ dùng khi nó nói
   "có"**: nó báo `false` cho cả máy tính bảng, mà máy tính bảng thì vẫn phải chơi bằng cảm ứng.
2. `Android` trong user agent. **Không đòi thêm chữ "Mobile"** — điện thoại Android có chữ ấy, máy
   tính bảng Android thì không; lọc theo "Mobile" là bỏ sót toàn bộ dòng máy tính bảng.
3. `iPhone|iPod|iPad`.
4. `Macintosh` **kèm** `maxTouchPoints > 1`.

Mảnh thứ tư là cái bẫy lớn nhất: **từ iPadOS 13, Safari trên iPad khai báo user agent y hệt macOS**
— `"Macintosh; Intel Mac OS X"`, không còn chữ "iPad" nào. Lọc bằng tên thì cả dòng iPad hiện nay
lọt qua thành máy tính để bàn. Dấu hiệu phân biệt: máy Mac thật có `maxTouchPoints = 0`, iPad thì
ít nhất năm.

Hỏi hỏng thì mặc định là **máy tính**. Đoán nhầm về phía ấy chỉ khiến người dùng điện thoại thấy
thanh kỹ năng kiểu PC; đoán nhầm phía kia thì người dùng máy tính bị một cái joystick ảo che mất
màn hình và không gỡ được — đúng cái vừa xảy ra.

Câu trả lời được **nhớ lại**, vì nó không bao giờ đổi trong một phiên chơi mà `CamUng.DangDung` thì
bị hỏi nhiều lần mỗi khung hình.

#### Đo (menu 42), chín loại máy thật

```
dat  Windows 11 + Chrome (màn hình thường)            chạm=0   -> PC
dat  Windows 11 + Chrome (laptop CÓ màn hình cảm ứng) chạm=10  -> PC
dat  MacBook + Safari                                 chạm=0   -> PC
dat  Linux + Firefox                                  chạm=0   -> PC
dat  iPhone + Safari                                  chạm=5   -> CẢM ỨNG
dat  Điện thoại Android + Chrome                      chạm=5   -> CẢM ỨNG
dat  Máy tính bảng Android (KHÔNG có chữ Mobile)      chạm=5   -> CẢM ỨNG
dat  iPad đời cũ (còn khai là iPad)                   chạm=5   -> CẢM ỨNG
dat  iPad đời mới (tự nhận là máy Mac)                chạm=5   -> CẢM ỨNG
1. 9 loại máy, số kết luận sai: 0
2. file jslib có đủ 4/4 mảnh lọc
3. CamUng.DangDung còn hỏi Input.touchSupported: False
số lỗi ghi nhận = 0
```

Ba dòng máy trong bảng là ba cái bẫy riêng, và mỗi cái từng làm hỏng một cách khác nhau: **laptop
Windows có màn hình cảm ứng** (đúng ca của anh), **máy tính bảng Android** không có chữ "Mobile",
và **iPad đời mới** tự nhận là máy Mac.

Phép thử chạy lại luật lọc bằng một bản chép C# — vì `.jslib` chỉ sống trong trình duyệt, không vào
Play mà thử được. Nên có thêm chiều 2: đọc lại chính file `.jslib` để chắc bản thật có đủ bốn mảnh,
và chiều 3: chắc rằng `Input.touchSupported` đã bị cắt khỏi đường quyết định — còn sót một dòng là
máy tính lại bị bắt cầm joystick.

Bảng chẩn đoán (F9) giờ in thẳng câu trả lời của trình duyệt, còn `touchSupported` vẫn hiện nhưng
được ghi rõ là *chỉ để tham khảo*.

### "Nhanh hơn thật" — thật ra là đứng im rồi nhảy

Chân đã bước rồi, nhưng anh báo tiếp: người kia và cả đàn quái **di chuyển nhanh hơn tốc độ thật**.
Guest thấy quái nhanh hơn hẳn so với Host thấy chính đàn quái ấy.

#### Tách bạch hai khả năng trước khi sửa

"Nhanh hơn" có thể là hai thứ hoàn toàn khác nhau, và đo chung thì không biết sửa chỗ nào:

- **Vị trí** đi nhanh hơn — nội suy phát lại quá nhanh.
- **Chân** quay nhanh hơn — vị trí đúng nhưng nhịp bước bị thổi phồng.

Nên hai chiều đo riêng, với một con số biết trước (3,0 m/giây). Kết quả trong Editor: **3,08 m/giây
và nhịp bước 0,59** (đúng ra 0,58). Cả hai đều đúng.

Vậy lỗi không nằm ở đó — nó chỉ hiện ra khi gói tin đến **thưa**.

#### Đệm mỏng hơn khoảng cách hai mốc

```csharp
float mocCachNhau = 1f / DongBoTran.NhipGui;   // 1/60 = 17 ms
```

Con số ấy chỉ đúng cho người chơi. **Đàn quái gửi 10 lần mỗi giây** — mốc cách nhau 100 ms — mà
đệm vẫn được tính theo 17 ms, tức **mỏng hơn khoảng cách hai mốc sáu lần**. Khi ấy nội suy luôn hết
mốc để vẽ ở giữa: con quái đứng im một lát rồi **nhảy** một cái tới mốc mới. Mắt người đọc cú nhảy
ấy thành *"nó chạy nhanh hơn"*.

Đây đúng là bài học đã ghi ở bước 4 — *"đệm không thể mỏng hơn khoảng cách hai mốc"* — nhưng lần ấy
tôi sửa bằng cách nâng nhịp gửi lên 60 và viết hằng số vào công thức. Hằng số ấy sai ngay khi có
nguồn thứ hai gửi ở nhịp khác.

Và nó còn sai cả với người chơi: gói được gửi trong `Update`, nên máy chạy 30 khung/giây thì chỉ
gửi được 30 gói/giây dù có khai 60.

#### Cách sửa: đo lấy, đừng tin hằng số

`NoiSuy` giờ **tự đo khoảng cách thật** giữa hai gói đến, trung bình trượt, bỏ qua những khoảng quá
dài (gói đến cụm sau một lần nghẽn — chúng làm con số phình ra rồi đệm dày lên mãi không rút được).

Jitter cũng phải đo so với khoảng cách **thật** chứ không so với hằng số: so với 17 ms trong khi
gói thật về 100 ms một lần thì *mọi* gói đều bị tính là "lệch 83 ms", và đệm phình lên vì một dao
động không hề tồn tại.

#### Đo (menu 41, tám chiều)

```
6. đặt bản sao đi 3.0 m/giây -> đo được 3.25 m/giây (lệch 8%)
7. cùng lúc đó, nhịp bước trung bình = 0.63 (đúng ra 0.58 = 3.0/5.2)
8. gói về 10 lần/giây (như đàn quái), đặt 3.0 m/giây:
    tốc độ đo được: 2.98 m/giây
    độ giật (khung dài nhất / khung trung bình): 1.4  (đi đều thì gần 1)
    đệm nội suy: 202 ms, khoảng cách hai mốc đo được: 138 ms
số lỗi ghi nhận = 0
```

**Độ giật** là con số nói lên đúng cái mắt nhìn thấy: quãng đường mỗi khung hình, lần lớn nhất chia
cho lần trung bình. Đi đều thì gần 1; đứng im rồi nhảy thì vọt lên mấy lần. Đo tốc độ trung bình
thôi là **không đủ** — một nhân vật đứng im nửa giây rồi nhảy một mét vẫn cho ra tốc độ trung bình
đúng y như một nhân vật đi đều.

Chiều 8 còn kiểm thẳng cái điều kiện đã hỏng: đệm phải **dày hơn** khoảng cách hai mốc.

#### Và một lần phép đo tự nói dối, lần thứ ba trong tuần

Lần chạy đầu, chiều 6 báo "sai tốc độ 85%". Tôi viết chuỗi mốc bằng `i * TocDoDat / 60f` — tức giả
định 60 khung/giây. Editor chạy khoảng 20, nên chuỗi ấy mô tả một người đi 1 m/giây chứ không phải
3. Mốc thời gian trong gói tin đi theo **giây thật**, nên vị trí cũng phải thế.

### Kiểm toán bước 5, và bốn chỗ hở đã vá

Anh hỏi bước 5 đã xong hết chưa. Trả lời bằng trí nhớ thì dễ nói "xong rồi", nên tôi viết **menu 43**
— một phép đo *chỉ để báo cáo*, dựng lại đúng tình huống trên máy khách rồi ghi ra số. Nó bắt được
hai lỗi thật; đọc code thấy thêm hai chỗ. Anh chọn sửa theo thứ tự 2 → 1 → 4 → 5.

#### 2. Quái chết giả bên máy khách

```
phép của khách trúng con quái -> IsDead = True
1 giây sau, chủ phòng vẫn báo "sống, 100% máu":
     IsDead = True, máu = 70
```

Phép của người khách vẫn trừ máu con quái bản sao **ngay trên máy khách**. Nếu nó về 0 trước khi
gói tin kế tiếp đến thì con quái chết ở bên khách — và không bao giờ sống lại, vì chỉ có đường "chủ
phòng bảo chết thì chết", không có đường ngược lại. Trong khi ấy bên chủ phòng nó vẫn sống và vẫn
đánh chủ phòng.

Và **bản sao người chơi dính đúng lỗi y hệt**: một đòn mà bên họ né được (vì trễ) sẽ để lại một cái
xác vĩnh viễn bên này, trong khi họ vẫn chạy nhảy bên kia.

Sửa ở gốc, trong `Damageable`: cờ `mauDoMayKhacQuyet`, bật cho mọi bản sao — quái bên khách và
người chơi khác. Đòn đánh vẫn hiện đầy đủ (nhấp nháy, số sát thương, tia hạt), nhưng **không trừ máu
và không giết được**. Máu thật đến từ gói tin; cái chết thật đến từ một gói tin nói "đã chết".

Kèm hai chỗ nhỏ:
- Cái xác không được ra đòn — gói "ra đòn" có thể đến muộn hơn gói "đã chết" một chút.
- Chủ phòng vẫn nhắc tên con quái trong lúc nó nằm gục; máy khách chưa thấy nó bao giờ thì **không
  dựng lại một cái xác** chỉ để nó chết thêm lần nữa.

#### 1. HUD máy khách ghi "Quái còn lại: 0" giữa một bầy quái

`GameHUD` đọc `director.Alive` và `director.Kills`, mà hai con số ấy đếm từ danh sách quái riêng của
máy — rỗng ở máy khách, vì khách không rải quái.

Thêm gói **bảng số**, 9 byte, hai lần mỗi giây: đợt mấy, đã diệt bao nhiêu, còn bao nhiêu, bao lâu
nữa đến đợt mới. Chỉ chủ phòng đếm được bốn con số này — chỉ nó rải quái và chỉ nó thấy con nào
chết *thật*.

Và một đường dự phòng: nửa giây đầu trận, trước khi bảng số đầu tiên đến, HUD khách đếm chính đàn
bản sao nó đang giữ thay vì ghi 0. Chính phép kiểm toán chỉ ra chỗ này — lần chạy lại đầu tiên sau
khi sửa nó *vẫn* báo 0, vì nó chỉ gửi gói vị trí quái, không gửi bảng số. Đó không phải lỗi của phép
đo mà là một khoảng hở thật.

#### 4. Phím R nạp lại màn giữa trận mạng

`GameDirector` nạp lại màn khi bấm R — kể cả khi đang sống, kể cả trong trận mạng. Chủ phòng lỡ bấm
thì cả đàn quái bị rải lại từ đầu, hai máy lệch nhau hoàn toàn; khách bấm thì phải bắt tay lại
trong khi chủ phòng vẫn chờ bản sao cũ. Và R nằm ngay cạnh WASD.

Giờ `GameDirector.DuocChoiLai` là `false` trong trận mạng. Màn hình gục ngã cũng thôi bảo "bấm R để
chơi lại" — chỉ còn "bấm ESC để về sảnh".

#### 5. Mất kết nối giữa trận

Trước đây không có gì cả: người kia đóng tab thì bản sao của họ đứng im mãi mãi; chủ phòng thoát thì
5 giây sau cả đàn quái bên khách biến mất — không một lời nào.

Hai ngưỡng, vì **trình duyệt dừng hẳn game khi tab bị ẩn**: người kia chuyển tab một lát, hay khoá
màn hình điện thoại, là gói tin ngừng đến. Kết luận "đã rời trận" ngay thì một cú liếc sang tin nhắn
cũng làm vỡ trận.

| Im lặng | Màn hình |
|---|---|
| 3 giây | *"Đang chờ tín hiệu từ người chơi kia... (5 giây)"* — chưa kết luận gì |
| tín hiệu quay lại | dòng chữ tắt, trận chạy tiếp như cũ |
| 10 giây, **hoặc kênh đóng hẳn** | kết luận đã rời trận |

Kênh **đóng hẳn** là dấu hiệu chắc nhất — người kia đóng tab hoặc bấm về sảnh (`BackToMenu` giờ đóng
kênh trước khi đi) — nên kết luận ngay, không đợi đủ 10 giây.

Hai phía mất hai thứ khác nhau, nên xử lý khác nhau:

- **Chủ phòng mất người khách**: trận vẫn chạy — quái, nhịp đợt đều nằm ở máy này. Gỡ bản sao của họ
  để quái thôi đuổi theo một cái bóng, báo *"Người chơi kia đã rời trận. Bạn chơi tiếp một mình."*
- **Người khách mất chủ phòng**: mất luôn cả trận, vì quái do chủ phòng điều khiển. Báo thẳng
  *"Chủ phòng đã rời trận — trận đấu dừng tại đây"*, kèm nút **TRỞ VỀ** trên máy cảm ứng (máy đó không
  có phím ESC).

Và đàn quái bên khách **không còn biến mất** khi đường truyền im lặng. Việc gỡ con quái "lâu không
được nhắc đến" chỉ chạy khi đường truyền còn tốt: im lặng chung của cả kênh không nói lên rằng con
quái nào đã chết.

#### Đo (menu 44), mười lăm chiều

```
2a. phép của khách trúng bản sao quái (gấp 3 lần máu) -> IsDead = False, máu 70 -> 70
2b. chủ phòng báo "đã chết" -> bản sao chết: True
2c. một gói "ra đòn" đến muộn từ CÁI XÁC -> mình mất 0 máu
2d. chủ phòng vẫn nhắc tên con đã chết -> số vật thể mang số 8000: 1 (không dựng thêm xác)
2e. phép của mình trúng BẢN SAO NGƯỜI CHƠI -> IsDead = False, máu không đổi
1b. chủ phòng báo đợt 3, đã diệt 27, còn 19 -> HUD khách: "Đợt 3  Quái còn lại: 19  Đã diệt: 27"
1c. hết đợt, đợt mới sau 4,2 giây -> HUD khách: "Đợt mới sau 5 giây"
4.  phím R: chơi một mình = True, trong trận mạng = False
5a. im lặng 3,5 giây -> DangChoTinHieu
5b. im lặng 6 giây (qua ngưỡng gỡ quái 5 giây) -> đàn quái còn 8/8 con
5c. tín hiệu quay lại ở giây thứ 6 -> Tot
5d. im lặng 10,5 giây -> DaMat, sự kiện mất kết nối đã bắn
5e. người kia ĐÓNG kênh -> kết luận sau 46 ms
số lỗi ghi nhận = 0
```

Chiều 2b không thừa: chỉ đo "bản sao không chết cục bộ" thì không phân biệt được *sửa đúng* với
*bản sao không bao giờ chết được nữa*. Phải chứng minh rằng khi chủ phòng bảo chết thì nó chết.
Chiều 5c cũng vậy: nó giữ cho ngưỡng 10 giây không trở thành "cứ im lặng là vỡ trận".

Và **menu 43 chạy lại đổi kết luận cả hai mục** thành *"khớp"* và *"không hở"*.

#### Ba phép thử cũ phải nói rõ vai của bia

Menu 34, 37, 39 dùng `NguoiChoiKhac.Sinh` làm bia để đo sát thương. Giờ bản sao mặc định không mất
máu cục bộ, nên nếu để nguyên thì phép đo sát thương luôn ra 0 — và tệ hơn, chiều "người tung không
tự thiêu" của menu 37 sẽ **luôn đạt** dù `boQua` có hỏng. Một phép đo không thể sai thì không đo gì
cả. Nên cả ba phải tắt cờ trên bia, kèm lời giải thích: bia đóng vai *người chơi trên máy của họ*.

#### Còn lại sau lượt sửa này

- **Chỉ 2 người** (mục 3 của báo cáo) — phòng cho 4, trận chỉ nối với người đầu tiên.
- **Đóng băng, choáng, máu khiên** không qua mạng (mục 6) — chỉ lệch về hình ảnh.
- **Chưa đo trên mạng thật**: mọi số đo trên chạy trong Editor với kênh giả lập. Hai ngưỡng 3 và 10
  giây là con số chọn trên giấy — trên điện thoại thật, khoá màn hình quá 10 giây sẽ làm vỡ trận.

### Bốn người trong một trận, và hiệu ứng giống nhau trên mọi máy

Hai mục cuối của báo cáo kiểm toán: **mục 3** (phòng cho 4 người mà trận chỉ nối được 2) và **mục 6**
(đóng băng, choáng, máu khiên lệch nhau giữa các máy).

#### Mục 3: nối hình sao qua chủ phòng

`KenhTrucTiep` giữ **đúng một** kết nối, cả phía C# lẫn cầu nối JavaScript, và `KhoiDongTranMang`
dừng ở người đầu tiên nó tìm thấy (`break`). Người thứ ba vào trận không thấy ai.

Viết lại thành **nhiều kênh**, nối hình sao:
- chủ phòng giữ một kênh cho mỗi người khách;
- khách chỉ giữ một kênh, tới chủ phòng;
- chủ phòng **chuyển tiếp** trạng thái và kỹ năng của khách này sang các khách còn lại.

Mẹo giữ mọi thứ đơn giản: **số kênh = số ghế của người ở đầu bên kia**. Trên máy chủ phòng, kênh 2
là khách ghế 2; trên máy khách, kênh 0 luôn là chủ phòng. Không cần bảng tra nào.

Năm chỗ phải làm cho đúng, mỗi chỗ một cách hỏng riêng:

| Chỗ | Làm sai thì |
|---|---|
| Chuyển tiếp **trừ kênh vừa gửi** | người gửi nhận lại chính mình từ 30 ms trước — một bản sao "chính mình" đứng sau lưng |
| Gói nhịp **trả lời đúng kênh vừa hỏi**, không phát cả phòng | cả phòng nhận câu trả lời của một câu họ không hỏi, con số độ trễ vô nghĩa |
| **Xếp ghế tất định** từ danh sách phòng, không đọc thẳng ô "chỗ" | hai người vào cách nhau vài trăm mili giây cùng ngồi ghế 1 (lại cuộc đua ấy), gói tin đè lên nhau, một người biến mất |
| **Sinh bản sao khi gói đầu tiên đến**, không sinh sẵn | một khách bắt tay hỏng thì các máy khác giữ một bức tượng đứng im mãi mãi |
| Chủ phòng **báo cho khách còn lại** khi một khách đi | khách không nối thẳng với nhau nên không tự biết — bản sao người vừa đi đứng im trên máy họ |

Ngoài ra: bắt tay chạy **song song** (lần lượt thì khách thứ ba phải chờ hai lần bắt tay trước, và một
lần hỏng 20 giây kéo cả người sau hết giờ theo); mất kết nối theo dõi **từng kênh** (một khách đi thì
trận vẫn còn, chỉ khi tất cả đi chủ phòng mới coi là hết); và bốn người không còn sinh ra chồng lên
nhau ở một điểm — mỗi ghế đứng một góc quanh điểm xuất phát.

Còn kết nối nào bắt tay hỏng thì màn hình nói thẳng: *"Đã nối 2/3 người. Không nối được với: Dũng"*,
thay vì im lặng để người ta tự hỏi sao bạn mình không thấy đâu.

#### Đo (menu 45), mười một chiều

```
1a. phòng có hai người cùng ghi "ghế 1" -> xếp: An=0(chủ phòng) Bình=1 Dũng=2 Chi=3
1b. máy khác đọc cùng phòng theo thứ tự ngược -> cùng bảng ghế: True
2a. gói trạng thái của ghế 2 đến trên kênh 2 -> chủ phòng chuyển sang kênh: 1,3 (KHÔNG vòng về 2)
2b. chủ phòng sinh bản sao cho ghế 2 khi gói đầu tiên đến: True
2c. ghế 1 tung phép -> chủ phòng chuyển sang kênh: 2,3
2d. ghế 3 hỏi nhịp -> chủ phòng trả lời trên kênh: 3 (CHỈ 3)
2e. chủ phòng kể lại đàn quái -> tới kênh: 1,2,3
3a. ghế 3 đóng kênh -> chủ phòng kết luận ai đi: [3], báo cho kênh: 1,2
3b. tình trạng chung của chủ phòng: Tot (không vỡ trận - còn hai người)
4a. khách nhận trên một kênh duy nhất -> có bản sao cho ghế 0/1/3: True/True/True, cho chính mình: False
4b. chủ phòng báo "ghế 3 đã rời trận" -> sự kiện: [3]; gói trễ của ghế 3 -> sinh lại: False
số lỗi ghi nhận = 0
```

Không bắt tay WebRTC thật được trong Editor, nên phép thử mở thẳng các kênh giả lập và đo đúng những
chỗ dễ sai kể trên. **Bắt tay thật với ba máy chưa từng chạy** — chỉ đo được khi anh mở trên ba, bốn
máy.

#### Mục 6: máy chủ sở hữu kể luôn hiệu ứng

Mỗi máy tự gieo hiệu ứng cho bản sao của người khác bằng `Random.value` riêng: A thấy B bị đóng băng,
B thì vẫn chạy nhảy; khiên của B vỡ trên máy A trong khi trên máy B còn nửa máu.

Quy ước giữ nguyên: **máy chủ sở hữu là trọng tài**. Người chơi kể hiệu ứng và khiên của chính mình
kèm gói trạng thái; chủ phòng kể hiệu ứng của quái kèm gói quái. Bản sao chỉ vẽ lại, và **không tự
gieo nữa** — gác ở `FrozenEffect.Apply`, `StunnedEffect.Apply`, chỗ `IceStorm` gắn thẳng lớp băng, và
`Khieng.HapThu`.

Gói tin **không to thêm một byte nào**: gói trạng thái cấp 12 byte mỗi người mà trước đây chỉ viết 11
— byte thứ 12 giờ chở máu khiên; ba bit hiệu ứng nhét vào byte cờ cùng hai bit cũ. Gói quái cũng vậy:
byte "đã chết" (0/1) giờ chở thêm ba bit.

Hiệu ứng áp từ mạng được **giữ sống 0,35 giây** và làm mới mỗi gói. Gói ngừng đến thì nó tự tan —
không cần một gói "hết đóng băng" riêng, và mất gói không để lại khối băng vĩnh viễn.

Khiên của bản sao chỉ có **một nguồn**: gói trạng thái. Phát lại phép Khiên trên bản sao giờ chỉ còn
động tác. Để cả hai đường thì chúng lệch nhau vài chục mili giây — một gói "chưa có khiên" đến sau là
đập vỡ cái khiên vừa dựng, rồi dựng lại: nhấp nháy, kèm một lần vỡ giả.

#### Đo (menu 46), mười hai chiều

```
1a. gieo đóng băng + choáng: lên BẢN SAO -> False/False; lên NHÂN VẬT THẬT -> True/True
1b. AreaFreeze tỉ lệ 100% trúng bản sao -> bản sao có băng: False
2a. nhân vật thật đang bị đóng băng + choáng -> cờ gửi đi: True/True/True
2b. khiên 150 máu ăn 75 -> gửi đi 0,50
3a. gói trạng thái 18 byte (không to thêm), đọc lại khớp
3b. gói quái: (chết, băng) -> (True, 1); (sống, choáng) -> (False, 4)
4a. máy kia kể "đông cứng + choáng + khiên 60%" -> bản sao: băng / choáng / khiên 0,60
4b. đòn 100 trúng bản sao trên máy này -> khiên bản sao giữ 0,60
4c. máy kia kể "hết cả" -> bản sao còn băng/choáng/khiên: False/False/False
4d. một gói "đóng băng" rồi gói tin ngừng hẳn -> sau 0,95 giây còn băng: False
4e. phát lại phép Khiên trên bản sao, chưa có gói trạng thái -> có khiên: False
5a/5b. chủ phòng kể quái choáng rồi hết choáng -> bản sao quái theo đúng
số lỗi ghi nhận = 0
```

Chiều 1a đo **cả hai chiều** của chỗ gác: bản sao không tự gieo được, *và* nhân vật thật vẫn gieo được
bình thường. Thiếu nửa sau thì không phân biệt được "gác đúng" với "hiệu ứng hỏng hẳn".

Chiều **4d bắt được một lỗi của tôi**: lần chạy đầu bản sao còn đóng băng sau khi gói tin đã ngừng.
`FrozenEffect` vừa gắn mới có sẵn `remaining = 3f`, nên `Max(3; 0,35)` giữ khối băng **3 giây**. Choáng
cũng vậy (mặc định 2 giây). Sửa: vừa gắn thì đặt thẳng thời gian sống.

#### Ba phép thử cũ phải sửa lại, và vì sao

**Menu 37** hỏng ba chiều sau khi đổi đường truyền. Nó có những quãng chờ dài (đợi máu ngừng tụt, đợi
thiên thạch rơi) mà không có gói tin nào — quá 10 giây là bộ đồng bộ kết luận "người kia đã rời trận"
và thôi đọc kênh ấy. Trận thật không bao giờ im lặng thế (60 gói mỗi giây). Sửa phép thử: một nhịp
giữ sống 0,5 giây, giống một người thật ở đầu bên kia. Và chiều khiên của nó giờ phải gửi cả gói trạng
thái có khiên — vì đó là con đường thật.

**Menu 38, chiều 5** — lần sửa thứ ba, lần này đúng chỗ. Hai lần trước tôi chỉnh *ngưỡng* (mét → giây,
hai khung → ba khung), trong khi sai lệch đo được gần như không đổi: **62, 41, 60 ms**, bất kể khung hình
nhanh hay chậm. Một sai số không co giãn theo khung hình thì không phải sai số của nhịp ghi — nó là sai
số của **chính phép đo**: tôi so lịch sử với "chỗ xuất phát", nhưng lúc hỏi thì đồng hồ đã trôi thêm
một, hai khung sau khi phép thử dừng di chuyển. Giờ phép thử tự ghi nhật ký (thời điểm, vị trí) của
chính nó và đối chiếu đúng thời điểm được hỏi: **lệch 0,00 m**.

**Menu 41** có một lần độ giật 2,1 thay vì 1,3 — chạy lại ra 1,3. Một khung hình Editor chậm.

Và mười phép thử mạng chạy lại đều **0 lỗi**: 34, 36, 37, 38, 39, 41, 43, 44, 45, 46.

### Chế độ chạy thử: bốn bộ xương một đợt

Anh báo vào trận quá nhiều quái, giật đến mức không thử được gì. Đúng vậy: Act2 mở màn là **25 con**
(7 phù thuỷ + 7 bộ xương rải sẵn, 7 quỷ dữ, 4 quỷ cây) cộng một đợt 7 con, và hai dòng quỷ còn sinh
thêm theo đồng hồ riêng.

Anh xin: vào màn chỉ có **4 bộ xương**, không loại nào khác; giết hết thì đợi **30 giây** ra 4 con mới;
lặp lại mãi.

#### Viết cứng trong code, không sửa scene

Hai scene ghi đè gần hết các con số rải quái — Act2 đặt `startingCount = 7`, `soPhuThuyRaiSan = 7`,
`soBoXuongRaiSan = 7`; Act1 lại đặt khác. Sửa từng ô trong scene thì sót một ô là vẫn giật, đúng cái
bẫy đã vấp với máu 30 000.

Nên `GameDirector.CheDoBonBoXuong` là **hằng số** trong code, scene không đè được. Bật lên thì bỏ qua
toàn bộ: rải sẵn khắp bản đồ, hai dòng quỷ có đồng hồ riêng, và nhịp đợt tăng dần. Chỉ còn một vòng:
bốn con bộ xương ra ngay lúc vào màn → giết hết → đếm ngược 30 giây (HUD hiện *"Đợt mới sau … giây"*)
→ bốn con mới.

Chơi mạng vẫn đúng: chủ phòng là trọng tài của quái nên chỉ chủ phòng chạy vòng này, khách nhận bốn
con qua mạng như cũ.

#### Đo (menu 47), cả hai màn, hai vòng mỗi màn

```
--- màn Act2 ---
1.  vào màn 2 giây: quái đang sống = 4 [Skeleton=4]
2a. vòng 1: giết hết -> còn 0 con, HUD đếm ngược: 29,9 giây
2b. đợt mới ra sau 29,9 giây game -> 4 con [Skeleton=4]
3b. đợt mới ra sau 30,3 giây game -> 4 con [Skeleton=4]
4.  để nguyên 260 giây game (qua cả đồng hồ quỷ dữ 120 và quỷ cây 240) -> nhiều nhất 4 con
--- màn Act1 ---
1.  vào màn 2 giây: quái đang sống = 4 [Skeleton=4]
2b. đợt mới ra sau 30,1 giây game -> 4 con
3b. đợt mới ra sau 30,1 giây game -> 4 con
4.  để nguyên 260 giây game -> nhiều nhất 4 con
số lỗi ghi nhận = 0
```

Phép thử đếm quái **trên cảnh, theo loại** — không tin con số HUD, vì HUD đếm từ danh sách của
`GameDirector`, tức chính cái đang cần kiểm. Chờ 30 giây bằng cách tăng tốc thời gian game và đo bằng
đồng hồ game (`Time.time`): 30 giây game mới là con số người chơi cảm thấy.

Chiều 4 là chiều chống lọt: nó để yên đủ lâu để **cả hai đồng hồ riêng** (quỷ dữ 120 giây, quỷ cây 240
giây) phải điểm. Còn sót đường nào thì đây là lúc quái lạ hiện ra.

Và băng thông đàn quái tụt theo: 4 con là **0,6 KB/giây** thay vì 3,3.

> ⚠️ **Đây là chế độ chạy thử.** Tắt (`CheDoBonBoXuong = false`) trước khi phát hành — không thì game
> chỉ còn bốn con bộ xương.

### Nút "Cài đặt" và ba mức đồ hoạ

Anh xin một nút **Cài đặt** ở sảnh, ngay bên trái *VAO PHONG NHANH*; bấm vào có nhiều tab (hiện
một tab **Giao diện**), trong đó chọn độ phân giải **Cao (hiện giờ) / Trung bình / Yếu**, bấm **OK**
thì game tải lại và chạy đúng mức đã chọn.

#### Mỗi mức đổi hai thứ

| Mức | Độ phân giải vẽ | Mức chất lượng Unity | Bóng |
|---|---|---|---|
| **Cao** (như trước giờ) | 100 % | High — khử răng cưa 8×, 2 đèn tính từng điểm ảnh | mềm, xa 60 m |
| **Trung bình** | 75 % mỗi chiều (≈ 56 % số điểm ảnh) | Medium — tắt khử răng cưa, 1 đèn | cứng, xa 40 m |
| **Yếu** | 50 % mỗi chiều (25 % số điểm ảnh) | Low — tắt khử răng cưa, 0 đèn | tắt |

Độ phân giải là thứ đáng tiền nhất trên điện thoại: iPhone có `devicePixelRatio = 3`, tức game vẽ
gấp 9 lần số điểm ảnh của một màn hình thường. Mức Yếu cắt còn một phần tư.

#### Vì sao phải tải lại cả trang

Trên WebGL, **độ phân giải chỉ đặt được một lần, trước khi Unity khởi động** — nó là
`config.devicePixelRatio` mà trang web đưa cho `createUnityInstance`. Nên lựa chọn phải nằm ở chỗ
**trang web đọc được trước khi Unity chạy**, rồi tải lại trang.

Chỗ đó là `localStorage` của trình duyệt, không phải `PlayerPrefs`:

- `PlayerPrefs` trên WebGL ghi xuống IndexedDB **không đồng bộ**. Bấm OK là trang tải lại ngay — ghi
  chưa kịp xong thì lựa chọn mất, người chơi thấy game "không nghe lời".
- `PlayerPrefs` nằm trong file của Unity; trang web không đọc được. `localStorage` thì cả hai phía
  cùng đọc.

Nên có thêm cầu nối `CauNoiCaiDat.jslib` (đọc, ghi, tải lại trang), và `index.html` đọc khoá
`diablo25d.mucDoHoa` rồi nhân `devicePixelRatio` với hệ số **trước** `createUnityInstance`.
Mức chất lượng Unity thì đặt trong `CaiDatDoHoa.cs`, **trước khi nạp scene đầu tiên**.

Người chơi **không phải đăng nhập lại**: phiên đăng nhập đã nằm trong kho trình duyệt từ lúc đăng
nhập, tải lại trang là tự vào sảnh như mọi lần mở game.

Ba chi tiết nhỏ:

- Bấm OK mà **không đổi gì** thì chỉ đóng bảng — tải lại cả game để vẽ y hệt là bắt người chơi đợi
  vô ích.
- Trình duyệt **cấm lưu** (vài chế độ ẩn danh) thì báo rõ trên bảng thay vì tải lại — tải lại lúc ấy
  game khởi động với mức cũ và người chơi tưởng nút OK hỏng.
- **Giữ nhịp khung hình như cũ.** Mức Low của Unity tắt vSync; tắt vSync thì trên WebGL dòng
  `Application.targetFrameRate = 120` (GameBootstrap) bắt đầu có hiệu lực — Unity bỏ
  `requestAnimationFrame` sang `setTimeout` và cố chạy 120 khung. Máy yếu chọn "Yếu" để nhẹ đi lại bị
  ép vẽ **nhiều khung hơn**. Nên cả ba mức đều giữ `vSyncCount = 1` như mức Cao.

Trong lúc bảng mở, cả sảnh bên dưới bị khoá: IMGUI trao cú bấm cho nút nào **vẽ trước**, nên không
khoá thì bấm vào bảng lại trúng nút *TAO PHONG* nằm ngay bên dưới.

#### Đo (menu 48)

**Ngoài Play:**

```
1. font: 14 chuỗi có dấu trong ManSanh.cs + CaiDatDoHoa.cs, 32 ký tự khác nhau, thiếu: không
   ⚠️ SAI — xem mục "Giao diện đăng nhập và sảnh chờ, làm lại": trên bản web chữ vẫn mất dấu.
2. vị trí nút CÀI ĐẶT:
   1920x1080: nút 1018..1178, VAO NHANH từ 1190, tiêu đề hết ở 759, chữ 76/160 điểm -> ổn
   2532x1170 (iPhone ngang): nút 1329..1502, VAO NHANH từ 1515, tiêu đề hết ở 1041 -> ổn
   ... (8 cỡ màn hình, 7 cỡ nằm ngang đều ổn)
3. trang web - chạy THẬT đoạn mã trong index.html bằng node:
   localStorage=null  dpr=2 -> để nguyên      localStorage="1" dpr=3 -> 2.25
   localStorage="0"   dpr=2 -> để nguyên      localStorage="2" dpr=3 -> 1.5
   localStorage="1"   dpr=2 -> 1.5            localStorage="3"/"abc" -> để nguyên
   localStorage="2"   dpr=2 -> 1              trình duyệt cấm lưu    -> để nguyên
```

Phép 1 quét thẳng file nguồn lấy mọi chuỗi có dấu, không chép lại danh sách chữ — chép lại thì thêm
một chữ mới là phép thử không biết. Phép 3 là đoạn mã **chỉ chạy trong trình duyệt**, Editor không bao
giờ chạm tới; nên phải chạy nó thật bằng node, và so với bảng hệ số lấy từ `CaiDatDoHoa.cs` chứ không
chép tay.

**Trong Play**, đăng nhập thật bằng tài khoản chạy thử, mở bảng, bấm OK từng mức; sau mỗi lần tải lại
thì đọc lại **từ kho lưu** (không tin biến trong bộ nhớ) rồi vào Act2 đếm những gì Unity thật sự vẽ:

```
5. OK khi không đổi gì: bảng đóng, game không tải lại
6. Trung bình: tải lại có, kho = 1, Unity Medium, bóng cứng, khử răng cưa 0x, vSync 1
   Act2: bóng 40 m, vật đổ bóng 205, tam giác 4 276k
   Yếu:       tải lại có, kho = 2, Unity Low, tắt bóng, khử răng cưa 0x, vSync 1
   Act2: vật đổ bóng 0, tam giác 1 561k
   Cao:       tải lại có, kho = 0, Unity High, bóng mềm, khử răng cưa 8x, vSync 1
   Act2: bóng 60 m, vật đổ bóng 418, tam giác 5 254k
số lỗi ghi nhận = 0
```

Số tam giác gồm cả lượt vẽ bóng: mức Yếu vẽ **ít hơn 3,4 lần** mức Cao, chưa tính việc mỗi khung
chỉ tô một phần tư số điểm ảnh.

**Trên bản web thật** (https://diablo25d-game.web.app, khung 1280×720):

| Kho lưu | `devicePixelRatio` Unity nhận | Canvas vẽ | Unity báo |
|---|---|---|---|
| (chưa có) | mặc định 1,25 | 1600 × 900 | `mức Cao, Unity High` |
| `1` | 1,25 × 0,75 = 0,9375 | 1200 × 675 | |
| `2` | 1 × 0,5 = 0,5 | 640 × 360 | `mức Yếu, Unity Low` |

(Lần đo mức 2, trang được nạp lúc khung trình duyệt thử đang ẩn nên nó báo tỉ lệ 1 thay vì 1,25 —
con số canvas vẫn khớp đúng tỉ lệ trang nhận được.)

#### Chưa làm / cần anh biết

- Màn hình **dựng đứng** (điện thoại cầm dọc): nút đè lên chữ *"Phong dang cho"*. Sảnh chưa từng
  được xếp cho màn hình dọc — ở cỡ ấy chữ tiêu đề đã sát nút VAO PHONG NHANH từ trước.
- Trong Editor, "tải lại" là nạp lại scene MainMenu, và lúc ấy game hiện **menu chơi đơn cũ** chứ
  không hiện sảnh. Đó là lỗi có sẵn: **mọi lần quay về MainMenu khi đang đăng nhập** (kể cả bấm
  *TRỞ VỀ* sau trận) đều rơi vào menu cũ, vì sảnh chỉ bật khi đăng nhập *xảy ra* trong lúc MainMenu
  đang mở. Bản web không vấp chỗ này khi đổi cài đặt, vì tải lại trang là khởi động lại từ đầu và tự
  đăng nhập.

### Cầu lửa trúng người chơi khác và trúng khiên

Anh báo hai lỗi của Quả cầu lửa khi đánh nhau:

1. Trúng người chơi khác thì có nổ, nhưng **không thấy sát thương của cú nổ**, chỉ thấy sát thương
   cháy liên tục.
2. Trúng **khiên** của người chơi khác thì không nổ mà **bay xuyên qua**.

Menu 37 trước giờ chỉ hỏi "người trúng đòn có mất máu không" — mất máu vì *cháy* cũng tính là đạt,
nên nó không bao giờ thấy được lỗi 1. Menu 49 mới ghi **từng cú mất máu** kèm thời điểm và tách hai
loại: cú nổ (55 × 0,55…1 = 30–55 máu, đúng khung hình quả cầu nổ) và cú cháy (~5 máu mỗi nửa giây).
Nó cũng ghi **chỗ quả cầu nổ** để biết nổ ở mặt khiên hay đã lọt vào trong. Đo cả hai chiều mạng:
người khác bắn mình (gói phép qua mạng, máy này phát lại) và mình bắn người khác (quả cầu trúng bản
sao).

#### Lỗi 1: cú nổ CÓ trừ máu — nhưng con số bị chính vụ nổ che

Đo trên code cũ, với một hướng bắn trống:

```
người kia bắn mình:  +0,00s: 48   +0,54s: 5   +1,06s: 5 ... (7 cú cháy)   -> mất 85 máu
mình bắn người kia:  +0,00s: 48   +0,53s: 5 ...
```

Cú nổ **có** — 48 máu, đúng khung hình quả cầu nổ, trên cả hai máy. Vậy "không thấy" là **không
nhìn thấy**. Chụp màn hình 0,15 và 0,4 giây sau cú nổ: thanh máu đã tụt (29 953 / 30 000) nhưng con
số 48 **không hề có trên ảnh**.

Nguyên nhân: con số là chữ 3D (`TextMesh`) nằm **trong cảnh**, ngay giữa quả cầu lửa bán kính 3,4 m.
Hạt lửa vẽ sau nó và cộng sáng lên, rồi hiệu ứng loé (`SimpleBloom`, chạy **sau** khi cả cảnh đã vẽ)
phủ trắng thêm một lần nữa. Con số sống 0,9 giây — đúng bằng lúc vụ nổ sáng nhất — nên biến mất trọn.
Các cú cháy 5 máu đến sau, lúc lửa đã tàn, nên vẫn thấy: đúng y hiện tượng anh tả.

Đổi hàng vẽ (`renderQueue`) không đủ: hiệu ứng loé cộng ánh sáng lên *cả màn hình* sau cùng. Nên số
sát thương giờ **vẽ trên màn hình bằng OnGUI** — cùng lớp với thanh máu, thanh kỹ năng — qua một đối
tượng ẩn duy nhất `VeSoSatThuong`, vẽ tất cả trong một lượt. Cỡ chữ tính từ đúng chiều cao 0,35 m tại
chỗ ấy (bằng chữ 3D cũ), thêm một viền tối để chữ đỏ đọc được trên nền lửa vàng. Hai chi tiết để khỏi
hại hiệu năng trên điện thoại: cỡ chữ **làm tròn số chẵn** (font động phải vẽ lại bảng chữ cho mỗi cỡ
mới), và hiệu ứng phồng lên dùng **ma trận** chứ không đổi cỡ chữ.

Sau khi sửa, ảnh chụp ở 0,15 s, 0,4 s và 0,54 s đều thấy rõ số **48** ngay giữa vụ nổ
(`PlayTestShots/caulua_no_*.png`).

#### Lỗi 2: khiên không chặn cầu lửa của người chơi

Mặt nạ va chạm của cầu lửa người chơi là `Enemy, Ground, Default` — **không có lớp `Khieng`**. Cầu
lửa của *quái* thì có (xem `EnemyAI`), nên khiên chặn được quái mà không chặn được người. Quả cầu bay
thẳng vào trong vòm và nổ ngay trên người chủ khiên. Đo được trên code cũ: nổ cách tâm khiên **1,00 m**,
trong khi khiên bán kính 3,04 m.

Không sửa bằng cách nhét lớp `Khieng` vào mặt nạ va chạm, vì có ba luật phải giữ:

- khiên **của người bắn** thì bỏ qua — quả cầu sinh ra trong khiên của chính mình;
- chỉ khiên của những ai nằm trong mặt nạ sát thương của quả cầu;
- quả cầu xuất phát **bên trong** vòm của ai thì vòm ấy không chặn nó (đứng trong khiên người khác mà
  bắn ra vẫn ra được).

Nhét vào mặt nạ thì cả ba phụ thuộc vào cách Unity xử lý một khối cầu xuất phát bên trong collider —
đúng, nhưng không ai đọc code mà biết. Nên `Khieng.DanChamVom` hỏi thẳng bằng hình học: đoạn đường quả
cầu đi trong khung hình này có cắt vòm nào (bán kính khiên + bán kính quả cầu) không, theo đúng ba luật
trên. `Fireball.Update` giờ lấy thứ **gần nhất** trong ba: vật cản, mặt khiên, người/quái — trước đây
hỏi lần lượt và nổ ở cái đầu tiên, nên một bia mộ nằm *sau* vòm vẫn có thể thắng.

Thêm hai chỗ, lộ ra khi đo:

- **Cầu lửa của mình trừ máu khiên của mình.** `Khieng.NoTrungKhieng` (trừ máu khiên khi vụ nổ chạm
  mặt vòm) không có tham số "người bắn": nổ trong vòng 6,4 m quanh mình là khiên mình mất 55. Đo được:
  khiên của người bắn mất **55** dù quả cầu nổ tận chỗ người kia. Giờ bỏ qua khiên của người bắn.
- **Khiên đỡ trọn đòn thì đỡ luôn hiệu ứng.** Nổ ở mặt vòm thì người bên trong vẫn nằm trong bán kính
  nổ, và `AreaDamage` sau khi khiên hút hết sát thương vẫn châm lửa cho họ — nhìn y như lửa lọt qua
  khiên. Giờ khiên đang bật lúc trúng đòn thì không dính cháy / đóng băng từ đòn đó.

#### Đo (menu 49), sau khi sửa

```
A1. người kia bắn mình:           nổ cách mình 0,26 m, cú nổ 48 + 7 cú cháy
A2. người kia bắn, mình BẬT KHIÊN: cả 3 quả nổ cách tâm khiên 3,34 m (mặt vòm),
                                   khiên mất 92, mình mất 0 máu, không cháy
B1. mình bắn người kia:           cú nổ 48 + 7 cú cháy hiện trên bản sao
B2. mình bắn, người kia BẬT KHIÊN: cả 3 quả nổ cách tâm khiên họ 3,34 m, không cháy
B3. mình BẬT KHIÊN rồi bắn ra:     quả cầu tới được người kia (cú nổ 48), khiên mình mất 0
số lỗi ghi nhận = 0
```

Trước khi sửa, cùng phép thử ra 3 lỗi: A2 và B2 nổ cách tâm 1,00 m, B3 khiên mình mất 55.

**Một phép thử cũ bị sửa theo.** Menu 37 đặt người kia cố định ở +5 m trục x — mà ngay cạnh đường bay
ấy có tấm bia `TS_round_50`, quả cầu giữa chỉ hở **3 cm**. Tuỳ tư thế tay lúc tung, có lần lọt qua, có
lần nổ vào bia cách người tung 1,5 m và phép thử báo "phép không gây sát thương". Lần chạy menu 49 đầu
tiên (code cũ) cũng nổ đúng vào bia ấy. Giờ cả menu 37 và 49 **tự chọn một hướng trống** — cả ba
đường bay của chùm không vướng gì trong 9 m — rồi mới đặt người. Chạy lại menu 34, 37, 38, 46: đều
**0 lỗi**.

### Giao diện đăng nhập và sảnh chờ, làm lại

Anh gửi bốn ảnh và báo ba việc:

1. Vào game vẫn hiện **menu chơi đơn cũ** (MÀN 1 – ĐẤU TRƯỜNG / MÀN 2 – NGHĨA ĐỊA / THOÁT) — chỉ nên có sảnh.
2. Màn đăng nhập và sảnh **sơ sài**, **nhiều chỗ chữ tràn, bị che** — làm lại cho đẹp, đúng chất game kinh dị.
3. Tiếng Việt phải **có dấu đầy đủ**; bảng Cài đặt đang mất chữ: "CÀI Đ T", "Giao di n", "Y u", "H Y".

#### Lỗi 3: font mặc định của Unity thiếu chữ tiếng Việt — và phép thử cũ đã nói dối

Font mà OnGUI dùng khi không chỉ định gì (`LegacyRuntime`) **thiếu** các chữ ạ ả ấ ầ ẩ ẫ ậ ắ ặ ẹ ẻ ẽ ế ệ ỉ ị ọ ỏ
ố ộ ớ ợ ụ ủ ứ ự ỳ ỷ ỹ ỵ… (dải U+1EA0–U+1EF9) và cả ơ, ư. Còn à á â ã è é ê ì í ò ó ô õ ù ú ý đ ă thì có, nên
"Trung bình" hiện đúng mà "Yếu" thành "Y u".

Trong Editor **không ai thấy**: font động của Unity mượn Arial của Windows để vẽ bù những chữ nó thiếu. Mục
"Nút Cài đặt" ở trên ghi *"font: … thiếu: không"* — con số đó **sai**. Nó hỏi `Font.HasCharacter`, mà trong
Editor hàm ấy trả lời luôn cả phần mượn của Windows. Lên trình duyệt thì chẳng có gì để mượn. Phép kiểm dùng
chính cơ chế đang che lỗi nên nó luôn báo đúng.

Cách sửa: dùng font **Inter** (bộ cài Unity có sẵn, giấy phép **SIL OFL** cho phép đóng gói kèm game; file
giấy phép đi theo trong `Assets/Resources/Fonts/Inter-LICENSE.txt`). Mọi chữ trên đăng nhập, sảnh, phòng,
đếm ngược, Cài đặt, và thông báo mạng trong trận đều dùng Inter.

Phép kiểm mới **đọc thẳng bảng ký tự (cmap) trong file font**, không hỏi Unity:

```
font Inter: Regular 2519 ký tự, SemiBold 2519 ký tự; 134 chữ có dấu tiếng Việt thiếu: không
đối chứng — Lato đi kèm Unity: thiếu 102/134 chữ (phải > 0)
137 chuỗi có dấu trong 8 file giao diện, 84 ký tự khác nhau, font thiếu: không
```

Dòng đối chứng là để chứng minh bộ đọc không "cái gì cũng đủ": một font biết chắc thiếu tiếng Việt thì nó
phải báo thiếu, và nó báo thiếu 102 chữ.

#### Lỗi 1: menu chơi đơn cũ hiện ra khi đã đăng nhập

Sảnh chỉ được bật khi việc đăng nhập **xảy ra** trong lúc MainMenu đang mở. Quay về MainMenu mà đã đăng nhập
từ trước (bấm TRỞ VỀ sau trận, hay tải lại màn) thì không ai bật sảnh, và menu chơi đơn cũ lộ ra. Đây là
lỗi đã ghi ở mục "Nút Cài đặt". Giờ khi chơi mạng, màn này **chỉ có** đăng nhập và sảnh: đã đăng nhập thì
vào thẳng sảnh, menu cũ và phím Enter-vào-Act1 không còn nữa (vẫn còn nếu tắt hẳn chơi mạng,
`batChoiMang = false`).

#### Lỗi 2: làm lại giao diện

Tất cả nằm trong `GiaoDien.cs` — một bộ dùng chung, ba màn chỉ việc gọi:

- **Không khí:** bốn góc màn hình tối lại, sương đỏ bốc lên từ đáy; khung gần như đen, viền đỏ sẫm, **móc
  sắt ở bốn góc**, hình thoi đỏ giữa mép trên; **máu nhỏ giọt** từ mép trên khung và dưới tên game (ảnh vẽ
  bằng code, số ngẫu nhiên cố định nên lần nào mở cũng một hình). Tên game **chập chờn như ánh nến** (độ
  sáng theo nhiễu Perlin), có quầng đỏ và bóng đổ.
- **Nút:** hai loại — đỏ máu cho việc chính (VÀO GAME, TẠO PHÒNG, VÀO PHÒNG NHANH, BẮT ĐẦU TRẬN, OK) và đá
  tối cho việc phụ; sáng lên khi rê chuột.
- **Sảnh:** tên + thành tích ở đầu, khung "TẠO PHÒNG MỚI" (chọn màn bằng hai nút Đấu trường / Nghĩa địa thay
  cho một nút bấm xoay vòng khó hiểu), khung "PHÒNG ĐANG CHỜ" có CÀI ĐẶT và VÀO PHÒNG NHANH cùng hàng. Mỗi
  phòng một hàng: tên, màn, chủ phòng, **bốn ô người** tô đỏ theo số người, nút VÀO / ĐẦY.
- **Trong phòng:** bốn thẻ ghế — vai trò (CHỦ PHÒNG màu vàng · BẠN), tên, SẴN SÀNG màu xanh hay "Đang chờ…",
  nút ĐUỔI cho chủ phòng; ghế trống ghi "Ghế trống".
- **Đếm ngược:** con số lớn đỏ, mỗi giây đập một nhịp.
- **Thông báo lỗi** từ Firebase và phòng cũng viết lại có dấu ("Phòng này đã bắt đầu chơi rồi.", "Tài khoản
  của bạn đã bị khoá…").

**Chữ không bao giờ tràn.** Mọi nhãn và nút đi qua `GiaoDien.Chu` / `GiaoDien.Nut`: dài hơn chỗ thì tự thu
nhỏ cỡ chữ (tới 62 %), vẫn dài thì cắt và thêm "…". Và mọi thứ xếp trong một **khung ảo rộng 1000 đơn vị**
nhân với tỉ lệ = nhỏ hơn giữa (cao / 1080) và (rộng / 1180) — nên điện thoại cầm dọc cũng không còn đè chữ
như trước.

#### Đo (menu 50)

Phép đo **đếm ngay trong hàm vẽ**, nên mọi nhãn mọi nút đều được tính, không phải chọn mẫu:

```
man hinh Game: 1568x581, tỉ lệ giao diện 0,54
1. màn đăng nhập:                4 lượt vẽ, chữ bị cắt 0, phải thu nhỏ 0
   font đang dùng: Inter-Regular / Inter-SemiBold
2. tab tạo tài khoản:            3 lượt vẽ, cắt 0, thu nhỏ 0
3. sảnh chờ:                     4 lượt vẽ, cắt 0, thu nhỏ 0
3b. sảnh có hai phòng (giả):     4 lượt vẽ, cắt 0, thu nhỏ 0
4. bảng Cài đặt:                 3 lượt vẽ, cắt 0, thu nhỏ 0
5. nạp lại MainMenu khi đã đăng nhập -> sảnh hiện ngay: có
6. trong phòng (chủ phòng):      4 lượt vẽ, cắt 0, thu nhỏ 0
6b. phòng đủ 4 người (giả):      4 lượt vẽ, cắt 0, thu nhỏ 4 ("Nguyễn Thị Hằng Nga" — tên dài, co chữ cho vừa)
7. đếm ngược:                    3 lượt vẽ, cắt 0, thu nhỏ 0
số lỗi ghi nhận = 0
```

Ba lần phép đo tự sửa mình:

- **"0 chữ bị cắt" khi chẳng vẽ gì.** Lần chạy đầu, ảnh sảnh **trống trơn** mà phép đo vẫn báo 0 lần cắt —
  không vẽ thì làm gì có chữ để cắt. Giờ nó đếm cả **số lượt vẽ** và báo lỗi nếu bằng 0.
- Lần chạy thứ hai bắt được "VÀO PHÒNG NHANH" bị cắt thành "VÀO PHÒNG NHA…" ở tỉ lệ 0,54 → nút rộng thêm.
  Dòng gợi ý tên phòng quá dài và nút "Đấu trường" hơi chật → rút gọn, nới rộng.
- Ảnh "phòng 4 người" vẫn ra 1/4: sảnh tự tải lại phòng từ máy chủ mỗi giây và ghi đè dữ liệu giả trước lúc
  chụp. Phép đo tạm hoãn nhịp tải lại trong lúc chụp.

Menu 48 kiểm hàng "PHÒNG ĐANG CHỜ" ở **10 cỡ màn hình** bằng cỡ chữ thật của Inter — kể cả hai điện thoại
cầm dọc (1080×1920, 1170×2532), trước đây bị đè chữ: giờ đều "ổn".

#### Hai phép thử cũ bị sửa theo

Menu 27 (khoá tài khoản) báo **"tài khoản bị khoá mà vẫn vào được game"**. Không phải lỗ hổng: trong Editor
đang lưu sẵn một phiên đăng nhập, lúc vào Play màn đăng nhập **tự đăng nhập lại song song** với phép thử và
đổi tài khoản giữa chừng — bước 3 đọc hồ sơ của một tài khoản khác. Menu 28 đã tránh việc này từ lâu (cất
phiên đi trước khi vào Play); menu 26 và 27 thì chưa. Cho cả hai cất phiên rồi trả lại: menu 27 **0 lỗi**,
tài khoản bị khoá bị chặn với câu *"Tài khoản của bạn đã bị khoá. Hãy liên hệ quản trị viên."*; menu 26
**0 lỗi**.

### Nền khung trong suốt hơn

Anh xin: các khung (đăng nhập, sảnh, phòng, Cài đặt) có **nền trong suốt hơn** để thấy cảnh phía sau; nút
bấm và chữ giữ nguyên độ đặc.

Lòng khung trước đặc ~0,94 — gần như che kín cảnh. Giờ ảnh lòng khung vẽ đặc hoàn toàn và độ trong suốt đặt
lúc vẽ, ở ba hằng số trong `GiaoDien`: lòng khung **0,55**, hàng trong danh sách / thẻ ghế **0,45**, bảng nổi
(Cài đặt) **0,66** — bảng nổi đục hơn một chút vì phía sau nó là cả sảnh, trong quá thì chữ hai lớp chồng lên
nhau. Bốn góc tối và sương đỏ cũng nhạt bớt (0,92 → 0,72 và 0,55 → 0,45). Viền, móc sắt, nút, ô nhập, chữ:
không đổi.

Đo trên ảnh chụp (menu 50), ở một vùng lòng khung **không có chữ**: độ lệch chuẩn độ sáng — nền càng lộ cảnh
phía sau thì càng lệch — và độ tương phản của chữ với chỗ nền **sáng nhất** (phân vị 95):

| Vùng | Lệch chuẩn trước → sau | Tương phản chữ chính / chữ phụ (sau) |
|---|---|---|
| Sảnh, lòng danh sách | 0,0005 → 0,0043 (×8,6) | 11,5 : 1 / 5,2 : 1 |
| Khung đăng nhập | 0,0001 → 0,0017 (×17) | 12,4 : 1 / 5,7 : 1 |
| Bảng Cài đặt | 0,0000 → 0,0003 | 14,0 : 1 / 6,4 : 1 |

Chữ phụ (màu nhạt) thấp nhất là 5,2 : 1 — vẫn trên mức 4,5 : 1 mà chuẩn WCAG coi là dễ đọc. Menu 50 vẫn
**0 chữ bị cắt** ở mọi màn.

### Người chơi trong phòng xếp theo hàng

Anh xin: thay vì bốn thẻ người chơi xếp thành **bốn cột**, mỗi người một **hàng**. Giờ mỗi hàng rộng cả
khung: số ghế · tên (chữ lớn) và vai trò (CHỦ PHÒNG màu vàng / NGƯỜI CHƠI, kèm "BẠN") ở dòng dưới · trạng
thái SẴN SÀNG / Đang chờ… · nút ĐUỔI sát lề phải (chỉ chủ phòng thấy, không có ở hàng của chính mình). Ghế
trống là một hàng mờ ghi "Ghế trống".

Lợi thêm một điều: cột cũ chỉ rộng ~220 đơn vị, nên tên dài phải co chữ — menu 50 lần trước ghi
*"Nguyễn Thị Hằng Nga" phải thu nhỏ 4 lần*. Xếp theo hàng thì tên có ~560 đơn vị: **0 chữ phải thu nhỏ, 0 chữ
bị cắt** ở cả phòng 1 người lẫn phòng đủ 4 người.

### Nền màn chính: nghĩa địa Act2, lò lửa bằng đá, ngọn lửa thật

Anh xin: nền phía sau màn đăng nhập / sảnh dùng **cảnh quan Act2**, vẫn hai lò lửa hai bên, nhưng lò
**dựng lại bằng đá trong Blender** — bề mặt sần sùi, vết nứt thấm máu — và **ngọn lửa thật hơn** (lửa cũ
trông như các hình tam giác).

#### Vì sao lửa cũ ra tam giác

Mỗi hạt lửa cũ dùng **một ảnh tĩnh** (`TextureFactory.FlameLick`): hạt chỉ biết to lên, nhỏ đi, mờ đi.
Bốn chục mảnh hình giọt nước giống hệt nhau chồng lên nhau thì mắt đọc ra ngay từng mảnh.

#### Ngọn lửa mới: lưới 64 khung, mỗi khung một khoảnh khắc của MỘT lưỡi lửa

> **Đã thay** bằng lửa mô phỏng thật trong Blender — xem "Lần 3" ngay dưới mục này. `LuaNgon.png` và
> `sinh_lua_ngon.py` đã xoá (còn trong git, commit `f69da60`).

`Assets/Resources/Flipbooks/LuaNgon.png` (8×8, 1024×1024), sinh bằng `CongCu/Blender/sinh_lua_ngon.py`
(chạy bằng Python đi kèm Blender vì Python của máy không có numpy). Mỗi ô là một thời điểm trong đời một
lưỡi lửa: nhen lên → vươn cao, lắc lư → thân tách thành 2–3 lưỡi, mép trên xé sợi theo nhiễu Perlin cuộn
→ gốc rút lên, ngọn đứt ra → tàn. Màu theo **nhiệt độ từng điểm** (vật đen: trắng vàng ở lõi → cam → đỏ ở
mép), trung bình R 236 / G 121 / B 41.

Lần đầu ra **ngọn nến** (hẹp, tròn, gai nhọn ở đỉnh) — nới đáy, tách thân thành nhiều lưỡi, tắt hẳn ở đỉnh.

Trong game (`VfxFactory.LuaLoDa`) có năm lớp: lưỡi lửa (mỗi hạt sống **trọn một vòng đời**, bắt đầu từ khung
đầu chứ không ngẫu nhiên như khói, lật ngang ngẫu nhiên một nửa cho khỏi lặp hình) · quầng sáng ở miệng chậu
· khói xám (lưới `KhoiCuon` có sẵn) · tàn lửa · đèn chập chờn.

Hai lỗi thấy trên ảnh chụp và đã chỉnh:

| | Lần đầu | Giờ |
|---|---|---|
| Lửa cháy trắng loá | 26 hạt/giây × độ sáng 1,25 cộng chồng lên nhau | 18 hạt/giây × 0,9, tint ngả cam |
| Lưỡi lửa lơ lửng trên lò | sống 1,35 s, bốc 0,45 m/s | sống 0,7–1,1 s, bốc 0,08–0,25 m/s |

#### Lò đá: dựng và nướng trong Blender

`CongCu/Blender/lo_lua_da.py`, chạy nền `blender -b --factory-startup -P …`:

1. **Lưới chi tiết** — chân đế bát giác bậc thang, trụ thon, đầu trụ loe, chậu đá dày; voxel remesh
   (140 nghìn đỉnh) rồi **đục** bằng nhiễu: khối u không đều, các ô lồi lõm như đá đẽo tay (nhiễu tế bào),
   hạt sần, miệng chậu sứt mẻ.
2. **Vật liệu nguồn** — đá xám nâu, rêu ẩm ở chân, muội đen gần miệng lửa. **Vết nứt** = Voronoi "khoảng
   cách tới cạnh" trên toạ độ bị bóp méo, chỉ bật ở vài vùng. **Máu** đọng trong nứt và **chảy xuống**:
   lấy mặt nạ nứt ở toạ độ dịch lên 2,5 / 5,5 / 9 / 14 / 20 cm, nhân với nhiễu sọc dọc — thành từng vệt nhỏ
   giọt bên dưới khe. Máu ướt nên bóng (độ nhám 0,22), đá khô 0,92.
3. **Lưới gọn cho game** — decimate xuống **8 000 tam giác** (+ 2 720 tam giác than hồng), UV tự động.
4. **Nướng** từ lưới chi tiết sang lưới gọn (Cycles, 1024²): màu (nhân AO 60%), pháp tuyến (khe nứt có
   chiều sâu thật), độ nhám → kênh alpha của texture kim loại/bóng cho shader Standard.

Hai cái bẫy trong Blender:

- **Cycles vẫn dùng được ở chế độ nền** dù không có trong danh sách engine (ghi chú cũ nói máy chỉ có
  EEVEE — đó là cài đặt người dùng). `--factory-startup` bật lại add-on Cycles.
- **Đục mặt đá bằng `mathutils.noise` từng đỉnh mất 1 403 giây** cho 140 nghìn đỉnh (`voronoi` rất chậm).
  Viết lại bằng numpy (Perlin + nhiễu tế bào tính cả khối) → **1 giây**. Cả quy trình giờ 125 giây.

Máu lần đầu đỏ tươi như sơn (0,30 / 0,015 / 0,01 tuyến tính) — hạ xuống đỏ nâu sẫm như máu khô.

#### Cảnh: chép một phần Act2

`DungManChinh` (menu 51) mở Act2, lấy ánh sáng / sương / bầu trời của nó, rồi chép sang cảnh mới **phần quanh
chỗ đứng** (địa hình, hàng rào, nhà mồ, bia, đá, cây, nước, cỏ — trong vòng 45 m và phía trước camera):
**287 vật** được chép, 539 vật bỏ. Tự dọn vật đứng ở chỗ nhân vật / lò lửa / chắn giữa camera và nhân vật
(bỏ 1 cây chết và 1 bụi cỏ). Menu 1 (dựng lại toàn bộ) cũng gọi hàm này.

Chỗ đứng chọn bằng menu 51b: đặt nhân vật trước từng nhà mồ theo bốn hướng, bỏ chỗ vướng bia/đá hoặc giữa
vũng nước, chụp bằng đúng khung camera màn chính. Chọn **trước nhà mồ MAUS_A** (cửa song sắt loang máu), cây
trụi lá phía sau.

Đo (menu 51c, Play, không có giao diện):

```
2 lò đá: mỗi lò 13–17 lưỡi lửa đang sống, vật liệu lửa P_LuaNgon, vật liệu đá đủ màu / pháp tuyến / bóng
khung hình màn chính: 2 326 nghìn tam giác, 175 vật đổ bóng, 4 đèn, 326 renderer
MainMenu.unity: 393 vật thể
```

(Cảnh chơi Act2 ở mức Cao là ~5 300 nghìn tam giác.) Menu 50 trên nền mới: **0 chữ bị cắt, 0 chữ phải thu
nhỏ** ở mọi màn; menu 48 (Cài đặt, tải lại màn chính ba lần): **0 lỗi**.

#### Lần 3: lửa và khói đen mô phỏng thật trong Blender (qua Blender MCP)

Anh xem bản trên: lò đá đẹp, nhưng lửa **vẫn ra hình tam giác và bay lơ lửng trên không** — nhờ dựng lại
lửa kèm khói đen **bằng Blender MCP**, trong Blender anh đang mở.

Lửa Perlin lần 2 hỏng từ gốc: mỗi hạt là **một lưỡi lửa rời** bốc lên rồi tan, nên lưỡi lửa tách khỏi
miệng chậu là đúng cái "lơ lửng". Lần này không vẽ lưỡi lửa bằng tay nữa mà **mô phỏng khí cháy**
(Mantaflow) ngay trong cảnh Blender của anh:

| | |
|---|---|
| Miền mô phỏng | 1,2 × 1,2 × 2,6 m, độ phân giải **112**, mở biên trên cho khói thoát ra |
| Nguồn cháy | đĩa bán kính 0,36 m, nhiên liệu 0,8, nhiễu mây trên nguồn trôi theo thời gian (lửa không đều) |
| Cháy | tốc độ cháy 0,62 (lửa cao ~1,3 m), khói sinh ra ×3, khói tan dần 40 khung |
| Nướng | 134 khung, **9,5 phút** (chạy bằng `bpy.app.timers` để Blender không treo lệnh MCP) |
| Render | EEVEE, camera trực giao 320 × 640, **hai lượt** bằng cách đổi vật liệu miền: lửa (phát sáng vật đen 1 150 + 1 600 × flame K) và khói (mật độ × 18) |

Chỉ lấy khung 70–133 (lửa đã cháy đều, bỏ đợt bùng lúc mới nhen), rồi **ghép vòng lặp 48 khung**: 16 khung
đầu trộn dần với 16 khung cuối. Đo độ liền mạch: khung cuối → khung đầu khác nhau **0,00526**, hai khung
liền nhau bình thường khác **0,00534** — mắt không thấy chỗ nối.

Mã hoá ảnh cho đúng shader trong game:

- **Lửa** (`Flipbooks/LuaLo.png`, 2048 × 2048, 8 × 6 ô): shader cộng sáng tính `màu × alpha`, dự án ở không
  gian màu Gamma, nên alpha = độ sáng lớn nhất của điểm, màu = màu ÷ alpha → cộng lên ra đúng màu render.
  Dọn nhiễu hạt của render (giá trị ~1/255 chia cho alpha gần 0 thành màu loạn): alpha < 0,015 thì bỏ —
  **79%** điểm ảnh là nền trống.
- **Khói** (`Flipbooks/KhoiDen.png`, 1024 × 2048): alpha = độ che phủ, mờ dần ở đáy (lửa đã che), ở đỉnh
  và hai bên (không có mép cắt). Màu **nướng sẵn**: chân khói hắt ánh cam của lửa, lên cao đen kịt ~0,05.

Trong game (`VfxFactory.LuaLoDa`) mỗi lò chỉ có **ba tấm đứng yên**, mỗi tấm đúng **một hạt** sống mãi, chạy
ảnh 20 khung/giây: khói phía sau, hai tấm lửa lệch pha 24 khung (một tấm lật ngang). Không còn hạt lửa nào
bay lên — chuyển động lưỡi lửa, xoắn, tách, tan thành khói đã nằm sẵn trong ảnh. Còn lại tàn lửa và đèn
chập chờn như cũ.

Những cái bẫy đã vấp, theo thứ tự:

| Hiện tượng | Nguyên nhân thật | Cách sửa |
|---|---|---|
| Render ra ảnh rỗng hoàn toàn — nhân mật độ khói ×80 vẫn alpha 0 | Compositor của cảnh đang bật (`render.use_compositing`) | Tắt compositor |
| Lượt lửa báo "alpha 0" | Volume chỉ **phát sáng**, không che gì → alpha 0 là đúng; phải xét kênh màu | Đo bằng RGB (lửa chiếm 22% khung, đúng nửa dưới) |
| Noise upres (tăng chi tiết ×2) ra rỗng: 140 file chỉ 0–3,4 KB | Chưa rõ; nướng lại lần hai vẫn rỗng | Bỏ noise, tăng thẳng độ phân giải gốc 80 → 112 |
| **Lửa vẫn lơ lửng ~0,4 m trên chậu** dù toạ độ tính đúng (đáy lửa 0,94 m, miệng chậu 1,07 m) | Chế độ `VerticalBillboard` của Unity **tự vẽ tứ giác bằng 0,707 lần kích thước đặt**. Đo bằng `BakeMesh`: đặt cao 3,033 m ra **2,145 m** — ở cả 1,5 m, 6 m, 30 m; chuyển sang `Billboard` thường thì ra đúng 3,033 m. Tấm co quanh **tâm** nên đáy lửa bị kéo lên | Nhân kích thước với √2 |
| Khói đen gần như vô hình | Màu khói 0,10–0,16 ≈ màu trời đêm ~0,15 | Nướng khói tối hơn trời (0,05) + ánh cam ở chân |
| Lõi lửa trắng loá | Hai tấm cộng sáng chồng nhau + bloom | Mỗi tấm 0,6 thay vì 1,0 |
| Lửa hơi nhoè khi nhìn gần | Lưới 1280 × 1920 bị Unity co về 1024 → ô còn 128 điểm ngang | Ghép thẳng ra 2048 × 2048 (ô 256 × 341) |
| Lửa cao 1,4 m trên lò cao 1,3 m, cột khói nằm ngoài khung | Tỉ lệ 1:1 với mô phỏng | Thu cả khối 0,82 |

Tôi đã **đoán sai một lần**: tưởng lửa lơ lửng vì Unity kẹp hạt ≤ 0,5 màn hình (`maxParticleSize`), sửa xong
chụp lại thấy y hệt. `BakeMesh` mới ra con số thật. Vẫn giữ `maxParticleSize = 100` vì khi camera nhìn cận,
tấm cao 3 m sẽ chạm trần đó thật.

Game view trong Editor đang là 1568 × 505 (lệch xa 16:9), nên menu 51c giờ render thêm `Camera.main` ở
**1920 × 1080** (`nen_0_1080p.png`) — đúng khung người chơi thấy.

Đo (menu 51c, Play):

```
mỗi lò: KhoiDen 1 hạt (KhoiDen 1024x2048) · LuaA 1 hạt · LuaB 1 hạt (LuaLo 2048x2048) · tàn lửa 10–11 hạt
khung hình màn chính: 2 325 nghìn tam giác, 175 vật đổ bóng, 637 lần gọi vẽ, 4 đèn, 326 renderer
```

Menu 50: **0 lỗi**, 0 chữ bị cắt ở mọi màn. Menu 48 (tải lại màn chính ba lần): **0 lỗi**. Bản WebGL build
6,4 phút, 163,8 MB, 0 lỗi; trên trang thật trình duyệt tải mới đủ bốn file theo mã phiên bản. Lần tải
đầu (thay trang game cũ đang chạy) console có **một** lỗi `RangeError: Maximum call stack size exceeded`
trong wasm; hai lần tải sau, mỗi lần theo dõi 50–70 giây: **0 lỗi**.

**Truy nguồn lỗi đó — không phải bản mới.** Chrome ghi khung wasm dạng `wasm://wasm/<mã>`; lỗi mang mã
`03e262ca`. Với file wasm dài như của game, V8 tính mã chỉ từ **độ dài**: mã = độ dài × 4 + 2 (hex). Kiểm
trên hai bản thật, giải nén file wasm ra đếm byte:

| Bản | wasm giải nén (byte) | Mã dự đoán | Mã Chrome ghi |
|---|---|---|---|
| Bản mới (20:59) | 16 317 709 | `03e3f436` | `03e3f436` (hai lần tải, như nhau) |
| Bản 19:16 | 16 315 926 | `03e3d85a` | `03e3d85a` |
| Bản 10:49 | 16 309 443 | `03e3730e` | — |
| Bản 09:51 (bản giao diện mới gây sập vì cache) | 16 308 742 | `03e3681a` | — |
| Bản 08:45 | 16 283 560 | `03e1dea2` | — |
| **Module gây lỗi** | **16 292 018** | | `03e262ca` |

Module gây lỗi nằm lọt giữa bản 08:45 và bản 09:51, mà giữa hai bản đó **không có lần triển khai nào** — nó
là một bản build từng được mở trong chính tab trình duyệt ấy ở phiên trước, lỗi còn đọng trong bộ đệm console
của tab (bộ đệm này giữ qua các lần chuyển trang). Các bản cũ được mở lại để đo bằng một kênh xem thử tạm của
Firebase Hosting (`hosting:clone … :thuloi`), đo xong đã xoá; trang live không bị đụng tới.

Thử tái hiện trên bản mới trong một tab mới tinh (bộ đệm console sạch): tải lạnh (xoá cache dữ liệu Unity),
tải lại, chuyển trang giữa lúc đang tải, chuyển trang đè lên game đang chạy — **cả bốn: 0 lỗi**.

Tệp Blender: `CongCu/Blender/lua_lo_da.blend` (bản sao cảnh mô phỏng, **không kèm** bộ nhớ nướng 654 MB —
mở ra phải nướng lại). Render qua Blender MCP **chạy được**: `bpy.ops.render.render(write_still=True)` ghi
ảnh đúng, chỉ có `Render Result` báo kích thước (0, 0) nên dễ tưởng là rỗng.

### Game sập ngay lúc tải sau mỗi lần cập nhật — cache giữ mã cũ

Đưa bản giao diện mới lên, mở trang thì game **sập ở 90 %**: lần đầu *"memory access out of bounds"*, lần
sau *"Maximum call stack size exceeded"*. Cùng bộ file ấy chạy trên máy (server cục bộ) thì tải bình thường
ở cả ba mức đồ hoạ, tải đi tải lại bốn lần.

Đã đoán sai hai lần trước khi đo đúng chỗ:

1. *"Tab trình duyệt cạn bộ nhớ vì đã tải game 160 MB bảy tám lần."* — mở một tab sạch, vẫn sập.
2. *"Unity nạp mức chất lượng thấp đã lưu, rồi code chuyển lên High (bật khử răng cưa 8×) lúc khởi động."*
   — dựng đúng điều kiện ấy trên máy: không sập.

Khác biệt thật giữa hai nơi nằm ở header. `firebase.json` đặt cho mọi file trong `Build/`:

```
Cache-Control: public, max-age=31536000, immutable
```

tức *"giữ một năm, đừng bao giờ hỏi lại"* — trong khi **tên file không đổi** giữa các bản
(`WebGL.wasm.unityweb`…). Trình duyệt từng vào trang giữ nguyên mã game (.wasm, .framework.js) của bản cũ.
File dữ liệu (.data) thì Unity tự hỏi lại máy chủ nên luôn là bản mới. **Dữ liệu mới + mã cũ = sập.**

Đo được ngay trong trình duyệt bị sập:

```
WebGL.wasm.unityweb   transfer = 0 (lấy từ cache)   dài 5 472 605 byte   <- bản 14:32
máy chủ đang có                                      dài 5 482 538 byte   <- bản mới
```

Lỗi này **có từ trước**, không phải do giao diện mới — chỉ là lần này dữ liệu thay đổi đủ nhiều (thêm font,
thêm kiểu dữ liệu) để lệch là sập. Người chơi nào không bấm Ctrl+F5 sau mỗi lần cập nhật đều có thể gặp.
Lúc phát hiện, tôi đã đưa trang về bản trước (bằng `firebase hosting:clone`) trong lúc tìm lỗi.

**Cách sửa** — hai chỗ, vì chỉ đổi header thì không cứu được ai: trình duyệt đã giữ bản "immutable" sẽ
không bao giờ hỏi lại để biết header đã đổi.

- `index.html` (luôn được hỏi lại) gắn **mã phiên bản** vào đường dẫn các file mã:
  `WebGL.wasm.unityweb?v=50cbd283e9`. Mã lấy từ nội dung file (MD5), nên mỗi bản một đường dẫn mới và
  cache cũ không còn khớp. Menu 29 tự gắn sau khi build (`XuatBanWebGL.GanPhienBanChoTrang`). File .data
  **không** gắn: Unity tự hỏi lại nó, và gắn vào thì mỗi bản để lại thêm 160 MB trong trình duyệt người chơi.
- `Build/**` đổi thành `Cache-Control: no-cache` — vẫn giữ trong cache, nhưng mỗi lần mở đều hỏi lại; không
  đổi thì máy chủ trả 304, gần như không tốn gì.

> ⚠️ **Hai câu trên sai — đo lại ngày 11/09/2026.** Firebase Hosting **không bao giờ trả 304** cho file
> `no-cache`, nên mỗi lần vào trang người chơi tải lại đủ 166 MB. `.data` giờ **có** gắn mã, và bản cũ
> không dồn lại trong máy người chơi. Xem mục ngay dưới.

### Mỗi lần vào trang lại tải 166 MB — Firebase không trả 304 cho file `no-cache`

Trình duyệt vừa tải lạnh xong, Cache Storage `UnityCache_DefaultCompany_Diablo 2.5D` đã có đúng bản
`.data` (166 129 904 byte, `Last-Modified` khớp máy chủ). Vậy mà tải lại trang vẫn thấy thanh tiến độ chạy
từ đầu. Đo trong trình duyệt ấy, hai lần tải lại liền nhau:

| Lần | `.data` | transferSize | thời gian | Unity tự ghi |
|---|---|---|---|---|
| Tải ấm 1 | 200 | 165 921 735 | 59,9 s | *successfully downloaded and stored* |
| Tải ấm 2 | 200 | 165 921 735 | 6,5 s | *successfully downloaded and stored* |

(Lần quan sát đầu tiên mất 103 s.) wasm (5 454 506) và framework (76 732) cũng tải lại đủ mỗi lần:
**171 468 063 byte mỗi lần vào trang**.

**Unity có hỏi lại đúng cách.** Đọc `WebGL.loader.js`: mặc định `cacheControl` cho `.data` là
`"must-revalidate"`. Có sẵn trong cache thì nó gửi `If-Modified-Since: <Last-Modified đã lưu>` kèm
`Cache-Control: no-cache`, được **304** thì dùng bản trong cache, được **200** thì tải lại hết rồi ghi đè.
Dòng *"downloaded and stored"* (thay vì *"revalidated"*) cho biết nó đi nhánh 200.

**Máy chủ mới là chỗ hỏng.** Kiểm độc lập bằng `curl`, gửi nguyên ETag máy chủ vừa đưa:

```
WebGL.data.unityweb (no-cache)  If-None-Match: <đúng ETag>   -> 200, 165 921 435 byte
/  (index.html, no-cache)       If-None-Match: <đúng ETag>   -> 200, đủ trang
/css/chung.css (max-age=3600)   lần 1 X-Cache: MISS, lần 2-3: HIT
                                If-None-Match: <đúng ETag>   -> 304, 0 byte
                                + Cache-Control: no-cache    -> vẫn 304 (X-Cache: HIT)
```

Firebase chỉ trả 304 khi **CDN của nó đang giữ file**. File `no-cache` thì CDN không giữ → mọi yêu cầu
đều `X-Cache: MISS`, đi về máy gốc, mà máy gốc thì trả 200 kèm cả file bất kể điều kiện. Tức là cái giả
thiết *"no-cache thì 304, gần như không tốn gì"* ở mục trên chưa từng được đo — và sai.

**Cách sửa: đừng hỏi mạng nữa.** Không đổi header để mong 304 (phải chờ CDN, mà CDN có giữ file 166 MB
của một game ít người chơi hay không thì không chắc) — mà để Unity không hỏi gì cả:

- Menu 29 gắn `?v=<MD5>` cho **cả `.data`** (`WebGL.data.unityweb?v=3c3de4e419`).
- `index.html` đặt `cacheControl`: đường dẫn nào có `?v=` thì trả `"immutable"` → Unity lấy thẳng từ
  Cache Storage, không một yêu cầu mạng nào. Mã nằm trong đường dẫn nên nội dung dưới một đường dẫn không
  bao giờ đổi; bản mới là đường dẫn mới — mã game và dữ liệu vẫn luôn cùng một bản, lỗi sập ở mục trên
  không thể quay lại.
- Menu 29 gắn mã `.data` vào `productVersion` (`"1.0+3c3de4e419"`). Unity có sẵn `cleanUpCache`: lúc khởi
  động, **trước khi tải**, nó xoá mọi mục có `productVersion` khác. Nhờ vậy gắn mã cho `.data` không làm dồn
  166 MB mỗi bản trong máy người chơi — nỗi lo đã khiến mục trên để trần `.data`.

Thử trên máy (server Python, nó ghi mọi yêu cầu nhận được):

```
tải lạnh      : GET /, loader, framework, wasm, data            -> cả ba file vào Cache Storage
tải lại       : GET /, loader.js                                -> hết. Unity: "served from the browser
                                                                   cache without revalidation" x3
giả bản mới   : .data và wasm đổi mã, productVersion đổi       -> tải mới đúng hai file đó
                Cache Storage trước 171 691 470 byte, sau 171 691 470 byte (bản cũ đã bị xoá)
```

Trên trang thật (deploy chỉ đưa lên **một** file mới là `index.html` — mã game và dữ liệu giữ nguyên),
trong chính trình duyệt đang giữ bản `.data` cũ không mã:

| Lần | Byte tải từ `Build/` | Ghi chú |
|---|---|---|
| Trước khi sửa, mỗi lần | 171 468 063 | |
| Lần đầu sau cập nhật | 171 468 063 | một lần duy nhất; bản cũ `version 1.0` bị xoá, cache còn 171 692 656 byte |
| Tải lại lần 2 | 15 090 (chỉ `loader.js`) | + trang 2 541 byte |
| Tải lại lần 3 | 15 090 (chỉ `loader.js`) | màn tải biến mất ở giây 22,6; 0 lỗi console |

(Con số 22,6 s trong bảng bị nhiễu: lúc đo còn mấy tab khác chạy game ở nền. Đo lại sạch, một tab: 14,3 s.)

Bản trước lần deploy này: `57cf551200457683`.

### Vào game chậm 4 giây vì giải nén 166 MB bằng JavaScript — build không nén, để Firebase nén

Hết tải lại rồi mà vào game vẫn mất ~14 s dù không đi mạng một byte. Console nói thẳng lý do:
*"You can reduce startup time if you configure your web server to add Content-Encoding: gzip"* — bản build
nén Gzip sẵn (`.unityweb`) mà máy chủ không báo `Content-Encoding`, nên Unity giải nén cả ba file (166 MB
`.data`, 5,5 MB wasm, framework) bằng JavaScript **mỗi lần vào game**, kể cả khi lấy từ cache.

**Đo trước ở máy**, hai máy chủ Python giống hệt nhau, chỉ khác header, tải ấm xen kẽ trong một tab:

| | lần 1 | lần 2 | lần 3 | TB |
|---|---|---|---|---|
| A — không `Content-Encoding` (như Firebase) | 15,9 s | 13,7 s | 13,8 s | 14,5 s |
| B — có `Content-Encoding: gzip` | 9,5 s | 9,8 s | 10,1 s | 9,8 s |

**Cách đầu tiên thử — thất bại:** thêm quy tắc `"**/*.unityweb"` → `Content-Encoding: gzip` vào
`firebase.json`, đưa lên một kênh xem thử. Đọc cấu hình phiên bản qua API: quy tắc **có** trong đó. Nhưng
phản hồi thật thì **không có** header ấy — Firebase gạt bỏ `Content-Encoding` tự đặt, rồi tự nén thêm lần
nữa theo ý nó (wasm tải về khác MD5 file gốc). Nghĩa là sáu quy tắc `.gz`/`.br` có sẵn trong `firebase.json`
từ trước (bản mẫu của Unity) chưa bao giờ có tác dụng, và câu *"Gzip + header Content-Encoding trên Firebase
Hosting"* ở đầu `XuatBanWebGL.cs` chưa bao giờ đúng. Đã xoá cả sáu.

**Cách làm được: build không nén sẵn**, để Firebase tự nén khi gửi — trình duyệt giải nén bằng mã máy.
Thử trước khi build, bằng cách giải nén tay các file hiện có (nội dung y hệt bản build không nén):

```
Firebase gửi            WebGL.wasm         br    4 236 596 byte  (bản nén sẵn: 5 454 506)
                        WebGL.framework.js br       67 813
                        WebGL.data         gzip 168 198 261     (bản nén sẵn: 165 921 735)
giải nén ra             MD5 khớp từng file gốc
tải ấm, trang thật      14,64 · 14,07 · 14,15 s  (TB 14,3)
tải ấm, kênh thử        10,90 · 10,46 · 10,48 s  (TB 10,6)
```

Lần tải đầu chỉ nặng hơn 0,6 % (172,5 MB so với 171,5 MB): wasm nén brotli còn nhỏ hơn bản Gzip của
Unity, còn `.data` gần như không nén được (texture đã nén sẵn). Firebase gọi `.data` là `text/html` vì không
biết đuôi ấy — `firebase.json` đặt `application/octet-stream` (đã thử: vẫn được nén).

**Cái bẫy thứ hai:** build không nén thật thì loader **khác**. Chỉ `.data` còn đi qua Cache Storage của Unity;
framework nạp bằng thẻ `<script>`, wasm do framework tự tải — cả hai đi qua **bộ đệm HTTP** của trình duyệt.
Với `Build/**` là `no-cache` (và Firebase không trả 304), mỗi lần vào vẫn tải lại 4,3 MB. Muốn bộ đệm HTTP giữ
được thì phải `immutable` — mà tên cố định + `immutable` chính là lỗi sập lúc tải trước đây. Nên bật
`nameFilesAsHashes`: **tên file là MD5 nội dung** (`bb0443598881fd7827ba57b633327ce6.wasm`), đổi nội dung là đổi
tên, không cách nào ghép bản cũ với bản mới — kể cả khi ai đó quên bước gắn `?v=`. Giờ `Build/**` là
`public, max-age=31536000, immutable`.

Kiểm thêm: `PlayerPrefs` (cài đặt, phiên đăng nhập) nằm ở `/idbfs/<mã>`. Mã đang có trên trang thật là
`0382ed2a…` = MD5 của `https://diablo25d-game.web.app` — địa chỉ **trang**, không dính gì tới tên file, nên đổi
tên file không làm ai mất cài đặt.

Trên trang thật (bản trước: `f7f89b60cb08a187`), trình duyệt đang giữ bản cũ:

| Lần | Mạng | Vào game |
|---|---|---|
| Đầu sau cập nhật | 172 511 486 byte; 3 mục cũ trong Cache Storage bị xoá | 16,6 s |
| Tải ấm 1 · 2 · 3 | **2 611 byte** (chỉ trang); `Build/` 0 byte | **10,35 · 9,68 · 9,45 s** |

0 lỗi console, không còn dòng *"reduce startup time"*. Còn lại cảnh báo *"[UnityCache] Response is served
without Content-Length header"*: Firebase luôn gửi kiểu chunked khi tự nén, phía ta không chỉnh được, và nó chỉ
là cảnh báo.

⚠️ Deploy giờ phải **xoá sạch `web/Build/` trước khi chép** — tên file đổi mỗi bản, chép đè sẽ để lại file
~200 MB của bản cũ trên máy chủ.

Kiểm lại trên **chính trình duyệt đã sập**:

```
WebGL.loader.js?v=23b18e8fa0            tải mới
WebGL.framework.js.unityweb?v=eb5475452d  tải mới
WebGL.wasm.unityweb?v=50cbd283e9          tải mới, 5 482 538 byte
đang tải... 100%, không lỗi, màn đăng nhập hiện đủ dấu tiếng Việt
```

### Tên người chơi trên đầu nhân vật

Anh xin: vào map thì trên đầu mỗi nhân vật có tên, để nhận ra ai với ai — bốn người cùng một bộ quần áo
phù thuỷ, không có tên thì không ai biết con nào là mình, con nào là người khác.

`BangTen` (Assets/Scripts/UI) gắn vào **mọi nhân vật trong trận mạng**: nhân vật của mình (`KhoiDongTranMang`,
ngay sau khi xếp ghế, nếu phòng có từ hai người) và bản sao của người khác (`NguoiChoiKhac.Sinh`). Chơi một
mình thì không có tên.

- Vẽ bằng **OnGUI** như số sát thương — chữ 3D trong cảnh bị hạt lửa và bloom phủ trắng. Nằm dưới HUD.
- Font **Inter** (`GiaoDien.ChuDam`) — font mặc định của Unity thiếu chữ có dấu, lên web là mất chữ.
- Cỡ chữ 19 ở màn hình cao 1080 (cùng thước với HUD), tối thiểu 12 điểm ảnh.
- Tên **của mình màu vàng**, người khác trắng ngà; viền tối bốn phía và nền mờ cho đọc được trên nền lửa.
- Người đã gục: tên **mờ đi** (độ đục 0,45) và **hạ xuống theo đầu**.

#### Cái bẫy: khung bao lưới không phải đỉnh đầu

Lần đầu đặt tên theo **khung bao của lưới** — ảnh chụp thấy tên lơ lửng cao hơn đầu cả một khoảng. Đo lại
prefab phù thuỷ bằng `BakeMesh` (lấy từng đỉnh thật):

| | Cao (m) |
|---|---|
| Chóp mũ thật (đỉnh lưới đã bake) | **1,61** |
| Xương `head_end` | 1,58 |
| Khung bao `SkinnedMeshRenderer` | 1,88 — khung "rộng rãi" Unity dùng để cắt hình |
| Con nhộng va chạm (`CharacterController`) | 2,06 |

Giờ tên **neo vào xương `head_end`**, đọc mỗi khung hình (đi theo đầu khi chạy, khi ngã). Nhân vật dựng bằng
code không có xương thì mới dùng khung bao.

Đo (menu 52, Play ở Act2, 1 nhân vật của mình + 3 bản sao tên có dấu "Ác Quỷ Bóng Đêm", "Kẻ Săn Hồn",
"Người chơi 4"; chóp mũ đo **độc lập** bằng lưới bake ở tư thế đang diễn):

```
4 bảng tên, cả 4 được vẽ trong khung hình vừa rồi, font Inter-SemiBold
đáy chữ trên chóp mũ 7–9 điểm ảnh (0,30 m = 15–19 điểm ảnh ở góc camera này), không cái nào đè lên mũ
đúng màu (mình vàng, người khác trắng ngà), 0 cặp bảng tên đè lên nhau
người gục: độ đục tên 0,45
số lỗi = 0
```

Bản web: build 5,7 phút, 0 lỗi; trình duyệt đã từng vào trang tải đúng bản mới, console 0 lỗi. **Chưa thử được
trận mạng thật hai máy** — WebRTC chỉ chạy trên bản web, và đăng nhập tài khoản thử phải do người làm.

#### Bỏ nền đen, và chắc chắn tên có dấu không mất chữ

Anh thử trận thật: tên hiện đúng nhưng **có ô nền đen phía sau** — xin nền trong suốt, và tên tiếng Việt phải
đủ dấu, không lỗi font.

**Nền đen mà phép thử không hề thấy.** Ô nền vẽ bằng `GiaoDien.To`, mà hàm này dùng texture `trang` — chỉ được
tạo trong `GiaoDien.ChuanBi()`, tức lúc màn đăng nhập / sảnh chạy. Người chơi đi qua sảnh nên có texture và
thấy ô đen; phép thử menu 52 vào **thẳng Act2** nên texture chưa có, ô nền lặng lẽ không vẽ gì — mọi ảnh chụp
trước đó của tôi đều không có thứ anh nhìn thấy. Giờ:

- `BangTen` **không còn nền**, chỉ còn viền tối mảnh bốn phía (1,5 điểm ảnh ở 1080) cho đọc được trên nền lửa.
- Phép thử gắn `VeThuOnGUI` gọi `GiaoDien.ChuanBi()` mỗi lượt OnGUI như màn sảnh — dựng lại đúng trạng thái
  người chơi có.

**Đo nền trong suốt trên ảnh chụp** — và lần đầu đo sai: so độ sáng trung vị *cả ô chữ* với nền quanh thì
**không nhạy** (ở cỡ chữ 12 điểm ảnh, nét chữ + viền chiếm quá nửa ô; thử trên ảnh cũ vẫn ra tỉ số ≥ 1). Đổi
sang đo **dải sát hai bên ô chữ** — nơi nền cũ tràn ra, không có nét chữ — so với nền xa hơn một chút, và thêm
**mẫu đối chứng**: vẽ lại đúng nền đen cũ quanh từng bảng tên, chụp, đo bằng cùng phép đo:

| | Tỉ số độ sáng dải sát mép / nền xa hơn |
|---|---|
| Bảng tên bây giờ (4 cái) | 0,96 · 1,07 · 1,07 · 1,14 |
| Đối chứng: vẽ lại nền đen cũ | 0,71 · 0,78 · 0,79 · 0,88 — bắt được 4/4 |

Ngưỡng 0,90 đặt giữa hai nhóm (dải đo chỉ rộng 1–2 điểm ảnh ở Game view 1568 × 505 nên dao động).

**Font.** Tên dùng Inter-SemiBold, nhúng cả dữ liệu font vào bản build (`includeFontData`) — không phụ thuộc font
của máy. Đọc **thẳng bảng ký tự (cmap) của file** (không tin `Font.HasCharacter` — trong Editor Windows vẽ bù):
2 519 ký tự, **đủ cả 200 chữ cần có** (134 chữ có dấu hoa + thường, chữ cái, chữ số, dấu cách, `_ - .`).

**Dấu rời.** Tên do người chơi tự gõ; bộ gõ "Unicode tổ hợp" (Unikey), máy Mac, điện thoại có thể lưu `a` + dấu
mũ rời + dấu hỏi rời thay vì một chữ `ẩ`. Bộ vẽ chữ của Unity không đặt được dấu rời lên đúng chữ cái — dấu lệch
hoặc mất. `GhepDauTiengViet.Ghep` ghép lại trước khi vẽ: lưới 12 nguyên âm × 6 thanh + 6 quy tắc mũ/trăng/móc
(không dùng `string.Normalize` của .NET — chưa chắc chạy trên WebGL). Đối chiếu với chuẩn Unicode của .NET
trong Editor: **252 cách gõ** của 134 chữ (tách hết, dấu sai thứ tự, nửa tách kiểu Unikey) — **sai 0**; bản sao
đặt tên ở dạng dấu rời "Ác Quỷ Bóng Đêm" hiện ra đúng chữ dựng sẵn, không còn ký tự dấu rời nào.

Menu 52: **0 lỗi**. Bản web build 5,4 phút, 0 lỗi, trình duyệt đã từng vào trang tải đúng bản mới, console 0 lỗi.

### Tên game mới "ÁC QUỶ TRỞ LẠI", trang Loading đủ dấu, bỏ lớp phủ mờ

Anh xin, ở **màn Loading**: font hiện đúng tiếng Việt có dấu; đổi "DIABLO 2.5D" thành **"ÁC QUỶ TRỞ LẠI"** kiểu chữ
đáng sợ; "dang tai..." viết hoa chữ đầu; thêm dòng **"Tác giả: PHẠM MINH QUÂN"**. Ở **màn trong game**: cũng đủ dấu,
tên mới **giống y** màn Loading, và **cảnh phía sau không bị phủ mờ**.

#### Tiêu đề là MỘT ẢNH dùng chung cho cả hai nơi

Màn Loading là trang HTML (chữ vẽ bằng font của trình duyệt), màn trong game là OnGUI (font của Unity) — hai bộ
vẽ chữ khác nhau thì không thể "giống y". Nên tên game được dựng thành **một ảnh PNG** (1632 × 478, nền trong):
dấu tiếng Việt nằm sẵn trong ảnh, không còn phụ thuộc font ở máy nào; trang Loading (`<img>`) và màn đăng nhập
(`GiaoDien.TieuDeGame` → `GUI.DrawTexture`) hiện **cùng một file**, cùng hiệu ứng chập chờn độ sáng 0,62–1,0.

- **Font:** phần lớn font kinh dị (Nosifer, Creepster, Metal Mania…) không có chữ Việt. Chọn **Grenze Gotisch**
  (chữ Gothic kiểu Diablo, Google Fonts, SIL OFL — anh đồng ý tải), `CongCu/Fonts/`. Đọc cmap: 643 ký tự, **đủ 134
  chữ có dấu** và mọi chữ trong tên. Chỉ dùng để dựng ảnh, không vào game.
- **Dựng:** `CongCu/TieuDe/sinh_tieu_de.py` (Pillow, không cần numpy): chữ Black 900, đỏ máu chuyển từ đỏ tươi
  xuống đỏ sẫm, vân máu chảy dọc + vết nứt, viền đen đỏ, cạnh trên sáng như lưỡi dao, **15 giọt máu** chảy từ
  chân chữ (cứ ba giọt một giọt dài, rải đều; không đặt dưới chữ Ạ để khỏi nhầm với dấu nặng), quầng đỏ và bóng đổ.
  Lần đầu vân chữ lấm tấm như bọt biển và giọt máu dài đều như que — đã sửa. Script tự gắn `tieude.png?v=<mã băm>`
  vào trang Loading.

#### Trang Loading

- Chữ dùng **Inter nhúng kèm trang** (`TemplateData/Inter-Regular.ttf`, `font-display: block` — chưa nạp xong thì
  ẩn chữ chứ không vẽ bằng font khác rồi nhảy). Đọc cmap: đủ mọi chữ trên trang.
- "Đang tải... 42%", dòng "Tác giả: PHẠM MINH QUÂN", lỗi "Không chạy được: …" — đều có dấu.
- Tên trên tab trình duyệt: "Ác Quỷ Trở Lại". **Không đổi `productName`** của Unity: cache dữ liệu (`UnityCache_…`) và
  chỗ lưu cài đặt / phiên đăng nhập của người chơi tính theo tên ấy — đổi là mất hết.
- ⚠️ Bản build giờ có thư mục `TemplateData/` — **phải chép sang `web/`** khi triển khai (đã thêm vào quy trình).

#### Bỏ lớp phủ mờ

`GiaoDien.VeNen` (tối bốn góc 0,72 + sương đỏ dưới đáy) không còn được gọi ở màn đăng nhập, sảnh và màn đếm ngược
(đếm ngược trước còn phủ đen 72%). Con số đếm ngược vẫn đọc rõ nhờ bóng đen và quầng đỏ riêng.

Đo:

```
Menu 50 (giao diện): 0 chữ bị cắt ở mọi màn, 0 lỗi (bảng Cài đặt thu nhỏ "Trung bình (hiện giờ)" 3 lần - vì mức
đang lưu là Trung bình, không liên quan)
Bản web: build 6,7 phút, 0 lỗi; trang thật: tab "Ác Quỷ Trở Lại", font InterViet "loaded", ảnh tiêu đề 1632x478,
"Đang tải... 0% | Tác giả: PHẠM MINH QUÂN", wasm mới; màn đăng nhập hiện tiêu đề mới, console 0 lỗi
```

#### Tiêu đề sáng hơn, bỏ dòng phụ, thanh tải "vạch máu"

Anh xem bản trên: tiêu đề **hơi tối** ở cả hai màn; bỏ dòng "Kẻ sống sót cuối cùng sẽ chiến thắng"; thanh tải
phải **rùng rợn** hơn.

**Tối vì hai lẽ**, đo trên phần chữ đặc của ảnh (alpha > 200): độ sáng trung bình chỉ **32/255** (kênh đỏ 83) — đỏ
sẫm, vân máu và vết nứt còn làm tối thêm — rồi hiệu ứng chập chờn lại nhân thêm xuống tới **0,62**. Sửa cả hai:

| | Trước | Giờ |
|---|---|---|
| Độ sáng trung bình phần chữ | 32 / 255 (đỏ 83) | **56 / 255 (đỏ 130)** |
| Màu chữ trên → dưới | 0,93/0,16/0,08 → 0,36/0,015/0,012 | 1,0/0,30/0,15 → 0,66/0,05/0,03 |
| Vân máu / vết nứt | nhân tối tới ~0,8 / vết 60–120 | ~0,9 / vết 110–165 |
| Quầng đỏ · cạnh sáng | 0,55 · 0,75 | 0,80 · 0,95 |
| Chập chờn (game và trang Loading) | 0,62 – 1,0 | **0,85 – 1,0** |

**Dòng phụ** và đường kẻ dưới nó bỏ khỏi màn đăng nhập; khung đăng nhập nhích lên ngay dưới tiêu đề.

**Thanh tải "vạch máu"** (trang Loading, thuần CSS):

- khung sắt đen viền đỏ, hai mép mọc **răng nanh** dài ngắn, nghiêng khác nhau (SVG nhúng thẳng trong CSS);
- ruột là **máu chảy**: đỏ tươi → sẫm, hai lớp vệt lệch nhịp (46 / 62 px) trôi liên tục — một lớp đều thì nhìn ra
  kẹo sọc;
- đầu thanh có **đốm sáng đập như tim** (nhịp đôi 1,1 s), cả thanh phập phồng ánh đỏ cùng nhịp. Đốm tim là phần
  tử riêng: đặt trong ruột thanh thì bị khung cắt (`overflow: hidden`) xén còn 10 px;
- **7 giọt máu** nhỏ xuống từ phần đã đầy — mỗi giọt chỉ hiện khi máu đã chảy tới chỗ nó, mỗi giọt một nhịp riêng;
  quãng rơi ngắn (28 px) để không rơi xuyên qua dòng "Đang tải…".

Xem trước bằng một bản sao trong thư mục nháp (thay các chỗ `{{{ … }}}` của Unity bằng giá trị giả — mở thẳng bản
mẫu thì khối JavaScript lỗi cú pháp và thanh không chạy).

Đo: menu 50 **0 lỗi**, 0 chữ bị cắt. Bản web build 6,5 phút, 0 lỗi; trang thật: đủ răng nanh / đốm tim / 7 giọt
(hiện đủ 7 khi tới 100%), font InterViet "loaded", ảnh tiêu đề mới `?v=46475ea594`, màn đăng nhập không còn dòng
phụ, console 0 lỗi.

### Cài đặt đồ hoạ: mức thấp chỉ thu nhỏ cảnh 3D, chữ và nút luôn nét — thêm mức "Rất yếu"

Anh báo: để mức **Yếu** thì các ô Đăng nhập, Tạo tài khoản, Tạo phòng, các nút… **quá mờ và nhoè**. Anh muốn: đổi mức
chỉ đổi **cảnh trong game và nền màn menu**, còn chữ, khung, nút giữ **y hệt mức Cao**. Đổi tên "Yếu" thành
**"Rất yếu"** và thêm một mức **"Yếu"** ở giữa Trung bình và Rất yếu.

**Nguyên nhân mờ:** mức thấp được làm bằng cách hạ `devicePixelRatio` của **cả khung game** ngay trên trang web, trước
khi Unity khởi động — mức Yếu cũ còn 50% mỗi chiều, và mọi thứ vẽ vào khung ấy (kể cả chữ và nút OnGUI) bị phóng to.

**Cách làm mới:** khung game luôn đủ độ phân giải (`index.html` không còn đặt `devicePixelRatio`, không đọc cài đặt).
Chỉ **camera chính** vẽ vào một ảnh đệm nhỏ (`KetXuatThuNho`), bloom cũng chạy trên ảnh nhỏ, rồi phóng lên màn hình ở
cuối chuỗi hậu kỳ; OnGUI vẽ **sau** mọi camera, lên thẳng màn hình đủ độ phân giải. `TheoDoiCamera` gắn nó vào camera
chính của mọi màn (kiểm mỗi khung — Act1 dựng camera sau khi scene nạp).

Cái bẫy phải tránh: để camera **luôn** vẽ vào ảnh đệm thì `pixelWidth` của nó là kích thước ảnh nhỏ — mọi
`WorldToScreenPoint` / `ScreenPointToRay` (tên trên đầu, số sát thương, chuột nhắm) lệch hết. Nên ảnh đệm chỉ gắn
**trong lúc camera vẽ** (`OnPreCull`) và gỡ ra ngay khi vẽ xong (`OnRenderImage`).

| Mức | Cảnh 3D | Unity | Bóng | Chi tiết xa |
|---|---|---|---|---|
| Cao | 100% | High | mềm, 2 tầng, 60 m | 1,0 |
| Trung bình | 75% | Medium | cứng, 40 m | 0,7 |
| **Yếu (mới)** | **62%** | Low + bóng | cứng, phân giải thấp, 25 m | 0,55 |
| **Rất yếu** (= Yếu cũ) | 50% | Low | tắt | 0,4 |

Lưu ở khoá mới `diablo25d.mucDoHoa2`: khoá cũ lưu 0/1/2 với 2 = Yếu cũ (50%) — đọc bằng nghĩa mới thì người đang
chọn 50% bị đẩy lên 62%. Lần đầu đọc thì chuyển: 0 → Cao, 1 → Trung bình, **2 → Rất yếu**.

**Cái bẫy menu 48 bắt được:** lần đầu "Rất yếu" **vẫn vẽ bóng** (51 vật đổ bóng). Gán `QualitySettings.shadows` là **ghi
thẳng vào bộ thông số của mức Unity đang dùng**: "Yếu" bật bóng cứng trên Low, rồi "Rất yếu" đặt lại Low vẫn nhận bóng
cứng. Giờ mỗi mức tự đặt đủ bốn giá trị (kiểu bóng, phân giải bóng, số tầng, chi tiết xa) bằng bộ gốc của mức Unity
tương ứng.

Đo (menu 48, Play thật, đăng nhập tài khoản chạy thử, Game view 1568 × 505):

```
trang web: không đặt devicePixelRatio, không đọc khoá cài đặt
chuyển khoá cũ: không có -> Cao, 0 -> Cao, 1 -> Trung bình, 2 -> Rất yếu (ghi sang khoá mới 3)
Trung bình: ảnh đệm 1176x379 (75%), bóng cứng 40 m, 213 vật đổ bóng, 4 313k tam giác
Yếu:        ảnh đệm  972x313 (62%), bóng cứng 25 m,  80 vật đổ bóng, 3 759k tam giác
Rất yếu:    ảnh đệm  784x252 (50%), tắt bóng,         0 vật đổ bóng, 1 579k tam giác
Cao:        không có ảnh đệm, bóng mềm 60 m, 438 vật đổ bóng, 5 428k tam giác
mọi mức thấp: ảnh đệm được phóng lên màn hình mỗi khung; ngoài lúc vẽ camera không gắn ảnh đệm, pixelWidth 1568 =
màn hình; KetXuatThuNho đứng sau SimpleBloom; ảnh chụp đủ sáng như mức Cao (0,163-0,165)
số lỗi = 0
```

Bảng Cài đặt giờ bốn hàng, dòng phụ "Chất lượng cảnh 3D · chữ và nút luôn giữ nguyên độ nét"; cột tên nới rộng —
menu 50: 0 chữ bị cắt, **0 chữ phải thu nhỏ** (trước đó "Trung bình (hiện giờ)" phải thu nhỏ).

Trên trang thật, mức Rất yếu: khung game **385 × 703 điểm ảnh = kích thước hiển thị × tỉ lệ điểm ảnh** (đủ độ phân
giải; bản cũ ở mức thấp nhất chỉ còn 50%), nhật ký game "Rất yếu … cảnh 3D = 0.5", nền phía sau thô đi nhưng chữ và
nút sắc nét, console 0 lỗi.

### HUD trong trận kiểu kinh dị: máu, mana, thông báo — có dấu, không đè nhau

Anh gửi ảnh: khung "DOT 1 - Quai con lai: 1 / Da diet: 3" bị dòng "Chơi một mình trong phòng…" **đè lên**, bảng
"PHU THUY" + thanh máu/mana đỏ tươi xanh trời trông lạc giữa game. Xin: thiết kế lại phần thông báo và máu/mana cho
**rùng rợn, âm u**, gõ đúng **tiếng Việt có dấu**, **không tràn che lẫn nhau**.

**Nguyên nhân đè nhau:** HUD (`GameHUD`) vẽ khung đợt ở giữa mép trên, còn `KhoiDongTranMang` tự vẽ dòng trạng thái mạng
ở **đúng chỗ ấy** (y = 24) — hai file tự chọn chỗ, không ai biết ai. Chữ không dấu vì cả HUD vẽ bằng font mặc định.

**Làm lại** (`GameHUDKinhDi.cs`, cùng lớp `GameHUD` — partial):

- **Bảng máu/mana**: khung đá tối, móc sắt đỏ ở góc (như sảnh); tên "PHÙ THỦY" (chơi mạng: tên người chơi, đã ghép
  dấu rời) + đường kẻ đỏ; thanh kiểu **ống chất lỏng** — máu đỏ bầm, mana xanh tím "linh hồn", vạch chia 10%, một vệt
  sáng trôi ngang như chất lỏng sánh, viền sắt + chỉ đỏ; **máu dưới 30%** thì viền bùng đỏ theo **nhịp tim**.
- **Khung đợt**: "ĐỢT 1" to đỏ máu, dưới là "Quái còn lại: 4 · Đã diệt: 0" / "Đợt mới sau 5 giây…"; khung tự nới theo chữ.
- **Thông báo mạng, mất kết nối**: HUD vẽ (KhoiDongTranMang nhường khi có HUD), trong khung đá riêng, tự xuống dòng.
- **Báo ngắn / báo của nhân vật / màn thua**: chữ Inter có viền tối; "BẠN ĐÃ GỤC NGÃ" chập chờn đỏ; mọi câu
  `PlayerController.Say` ("Không đủ năng lượng!", "QUẢ CẦU LỬA đang hồi chiêu"…), nút "TRỞ VỀ", phím F9 — **có dấu**.
- **Một hàm bố cục thuần** `GameHUD.TinhBoCuc` xếp mọi khung theo thứ tự: bảng máu → khung đợt → thông báo mạng → mất
  kết nối → báo ngắn → báo nhân vật; khung nào vướng khung trước **hoặc** vướng nút điều khiển (thanh kỹ năng PC; cần
  joystick, cụm nút kỹ năng, cột nút góc nhìn trên bản cảm ứng) thì đẩy xuống (báo nhân vật: đẩy lên). Hết chỗ thì dò
  khoảng trống gần nhất; vẫn không có (điện thoại dựng dọc, 4 thông báo cùng lúc — cụm nút chiếm hết bề ngang) thì hai
  dòng báo **ngắn hạn** dùng chung một chỗ — không bao giờ để chữ đè chữ.

Tên "PHU THUY" cũ lưu **trong hai scene** (trường `tenNhanVat`) — sửa mặc định trong code thì scene vẫn đè; trường ấy đã
bỏ, tên lấy từ code.

Đo (menu 53 mới):

```
A. hàm bố cục thật, chữ dài nhất, mọi thông báo cùng hiện: 11 cỡ màn hình (1920x1080 … 800x360, 2732x2048, dọc
   1080x1920, 1170x2532) x PC / cảm ứng x 4 bộ chữ -> 0 khung chữ ra ngoài / đè nhau / đè nút;
   số máu/mana lọt thanh, chữ lọt khung đợt  (lần đầu: 1170x2532 cảm ứng, báo nhân vật văng ra y = -78 -> đã sửa)
B. 212 chuỗi trong 4 file HUD: mọi ký tự có trong cmap Inter; 0 cụm chữ không dấu cũ còn sót
C. Play Act2, 6 khung chữ cùng hiện + máu 20%: 0 đè nhau / ra ngoài; font Inter-SemiBold / Inter-Regular
```

Menu 40: "30000 / 30000" rộng 72 điểm trong thanh 148 điểm (đo bằng Inter), 0 lỗi; menu 22: 0 lỗi. Bản web build 5,6
phút, 0 lỗi, trang thật tải đúng bản mới, console 0 lỗi. Còn lại không dấu: bảng chẩn đoán F12 (công cụ gỡ lỗi, mặc
định tắt).

Nhân tiện: công cụ ghi file của tôi biến chuỗi `\u0300` trong mã nguồn thành **ký tự dấu rời thật** (vô hình khi đọc) —
`GhepDauTiengViet.cs` và `ThuBangTen.cs` đã được đổi lại thành dạng `\u0300` nhìn thấy được; phép thử 52 vẫn 0 lỗi.

### Mười lò lửa đá trong Act2

Anh xin: đưa cái lò lửa đá của màn chính vào Act2, **10 lò**, một lò **ngay chính giữa bản đồ**; lò nào cũng **không
dưới nước, không trong nhà mồ, không trên bia mộ — chỉ trên mặt đất**.

**Chính giữa bản đồ (0, 0) là hồ nước trung tâm.** Hai yêu cầu chọi nhau, và luật "không dưới nước" thắng: lò giữa là
**chỗ đất khô gần tâm nhất** — dò từ tâm ra, bước 0,25 m — ra bờ nam hồ, **cách tâm 4,8 m**, mép lò cách mép nước 2 m.

**Luật một chỗ hợp lệ** (menu 54 mới, `DatLoLuaAct2.cs`):

- **Mặt đất**: tia chiếu từ trên xuống ở tâm và 8 điểm quanh chân lò đều chạm **địa hình** trước tiên (không phải mái
  nhà, đá, bia); chênh cao dưới chân lò ≤ 0,22 m; chân lò đặt ở điểm **thấp nhất** nên không hở.
- **Nước**: mọi điểm trong vòng 2,05 m quanh tâm lò không nằm **dưới mặt nước** — tức nằm trong tam giác lưới nước
  **và** địa hình thấp hơn mặt nước (mép lưới nước chìm vào đất, xét lưới thôi thì bờ hồ phình ra).
- **Nhà mồ**: ngoài hộp bao nhà (tính cả mái) nới rộng 2 m. **Bia, đá, hàng rào**: không va chạm nào trong ống trụ
  bán kính 1,45 m quanh lò. **Cây**: cách thân 3 m (lửa + khói cao ~3 m). **Bụi cỏ**: cách 1,15 m.
- **Bia cao** (tháp, cột > 1,5 m): cách 3 m — xem lỗi thứ hai bên dưới.

Chín lò còn lại **rải đều trên phần đất hợp lệ**: chia vùng kiểu k-means (Lloyd) với lò giữa cố định, rồi đặt mỗi lò vào
điểm hợp lệ gần tâm vùng nhất, cách nhau ≥ 10 m. (Chọn kiểu "xa nhất" đơn giản thì cả chín lò dạt ra sát hàng rào.)
Chỉ **773 / 6625** điểm lưới 1 m trong vòng 46 m là hợp lệ — nghĩa địa rất dày: 2179 điểm vướng bia/đá, 1048 nước,
1031 tia không chạm đất trước, 489 gần bia cao, 399 gần cây, 372 nhà mồ.

Lò nằm trong nhóm gốc riêng `LoLua_Act2`, **không** trong `World` — menu 51 chép `World` sang màn chính, không được kéo
lò Act2 theo. Mỗi lò có **va chạm** (người chơi, quái không đi xuyên; GameDirector không thả quái vào lò) và vẫn là
prefab `Assets/Models/LoLuaDa` — sửa lò một chỗ, cả màn chính lẫn Act2 đổi theo. Lốc xoáy **không** cuốn lò (nó bỏ qua
vật có hệ hạt con). Chạy lại menu 54 ra y hệt (không ngẫu nhiên), xoá lò cũ trước.

**Lỗi thứ nhất — nhân vật đi xuyên 7/10 lò.** Va chạm đầu tiên là con nhộng cao bằng lò (1,33 m), bán kính 0,65 m —
tức gần như **một quả cầu**. Nhân vật bước được bậc 0,55 m, dốc 55°: nó trượt lên mặt cầu rồi **đứng trên đỉnh lò**
(cách tâm 0,00–0,08 m). Giờ là cột đứng bán kính 0,50 m, **cao hơn lò 1 m** (vùng ngọn lửa) — thành đứng từ 0,5 m trở
lên, quá bậc bước được. Sau khi sửa: **bị chặn 10/10**.

**Lỗi thứ hai — ảnh chụp, không phải số đo.** Lần đầu lò giữa đứng cách một cột tháp 1,45 m — đúng luật "không trên
bia". Nhưng nhìn từ camera game (phía nam), cột tháp nằm **thẳng dưới miệng lò**, trông y như lò **đặt trên đỉnh bia**.
Thêm luật bia cao cách 3 m; lò giữa dời sang bờ nam hồ.

Đo (menu 54b mới — kiểm bằng cách **khác** lúc đặt: nước bằng va chạm tạm gắn vào lưới nước + tia chiếu xuống; nhà
bằng tia chiếu **lên** từ miệng lò; bia bằng hộp bao **hình**; mặt đất bằng độ cao địa hình ở 16 điểm quanh chân):

```
10 lò; lò giữa cách tâm 4,8 m; hai lò gần nhau nhất 17,0 m
mép nước gần nhất 2,00 .. >12 m; nhà mồ gần nhất 9,7 m; bia/đá gần nhất 0,53 m; bia cao gần nhất 2,1 m
chân lò: hở 0,00 m, chôn tối đa 0,11 m (cả 10 lò)
Play: lửa chạy 10/10, đèn 10/10; nhân vật đi thẳng vào lò: bị chặn 10/10; 0 lỗi console
```

Ảnh: `lolua_bando.png` (nhìn từ trên xuống, vòng vàng = lò giữa), `lolua_1/2/3_*.png` (camera game).
Cái giá: mỗi lò 10 720 tam giác + một đèn điểm (tầm 9 m) — 10 lò thêm 107 nghìn tam giác vào 2,69 triệu của Act2 (+4%);
mức Yếu/Rất yếu của Unity cho 0 đèn điểm tính theo điểm ảnh nên đèn lò chỉ còn tính theo đỉnh, rẻ.

### Lốc xoáy dập tắt lò lửa rồi cuốn cả cái lò đi

Anh xin: lốc xoáy **của mọi người chơi** khi trúng lò lửa thì **dập tắt lửa trước**, rồi **cuốn cái lò bay theo** như
cây và bia mộ, **30 giây sau** lò hiện lại và **cháy tiếp**.

Trước đó lốc xoáy **không đụng được** vào lò: `VatTheBiCuon.CuonDuoc` loại mọi vật **có hệ hạt con** (lửa, khói, vòng
phép) — cuốn một cái đang phát hiệu ứng lên trời thì vừa vô lý vừa hỏng vòng đời của hiệu ứng. Lò lửa đúng là một vật
như vậy. Nên luật mới: **lò là ngoại lệ, nhưng phải tắt lửa đã** — tắt xong thì nó không còn hệ hạt nào nữa, và cái
luật cũ tự nhiên lại đúng.

Trình tự (`Tornado.CuonVatThe` → `LoLuaDa.DapTat` → `VatTheBiCuon`):

1. Gió quạt tới lò → `DapTat`: xoá cụm lửa (lửa, khói, tàn, đèn), để lại **một cuộn khói xám** bốc lên. Cuộn khói đặt
   ở **thế giới**, không làm con của lò — lát nữa lò bay đi, khói phải ở lại chỗ cũ.
2. Chờ **0,35 giây** rồi mới bốc lò lên, để mắt kịp thấy lửa tắt trước khi cái lò rời đất.
3. Lò bay quanh thân lốc, tan biến cùng lốc, **30 giây** sau mọc lại đúng chỗ cũ và `Chay()` nhóm lửa lại — dùng
   nguyên bộ máy có sẵn của cây và bia mộ, không viết đồng hồ riêng.

**Than trong chậu phải tắt theo.** Lần đầu lửa tắt nhưng ảnh chụp lúc lò bay vẫn thấy **đống than đỏ rực**: vật liệu
`M_Coals` **tự phát sáng** (emission 2,5), nó không liên quan gì tới hệ hạt. Giờ `DapTat` phủ `MaterialPropertyBlock`
lên riêng lưới "Than" cho nó thành tro xám, `Chay()` gỡ ra.

Lốc của người chơi khác cũng là một `Tornado` chạy trên máy mình, nên luật này áp cho **mọi người chơi** mà không cần
gói tin mới — giống hệt cách cây và bia mộ vẫn bị cuốn từ trước.

Đo (menu 54c mới — đọc trạng thái thật từng khung hình, không hỏi lại chính cái luật vừa sửa):

```
lửa tắt ở giây 0,83  →  lò nhấc khỏi đất ở giây 1,41   (đúng thứ tự, cách nhau 0,58 giây)
lò bay cao nhất 3,76 m; biến mất ở giây 6,02 cùng lúc lốc tan; lúc biến mất: hình tắt, va chạm tắt, đã về chỗ cũ
sau 30 giây: hình bật, va chạm bật, lửa cháy lại (12 hạt, có đèn), lệch chỗ cũ 0,00 m, không còn VatTheBiCuon
độ sáng vùng miệng chậu: đang cháy 147,9 → vừa tắt 45,5 (giảm 69%)
vật khác có hệ hạt: vẫn KHÔNG cuốn được; lò đang cháy: KHÔNG cuốn được
0 lỗi console
```

Ô đo độ sáng lúc đầu rộng quá (gồm cả mặt đất chung quanh) nên chỉ thấy giảm 42% và phép thử báo lỗi — tôi **thu ô đo
vào đúng miệng chậu**, không nới ngưỡng.

### Giai đoạn 2, bước 6: trận đấu kết thúc — người sống sót cuối cùng thắng

Đến trước bước này, trận mạng **không bao giờ kết thúc**: ai chết thì nằm đó, ai sống thì đánh quái mãi, và không ai
được ghi một trận thắng nào — `HoSoMang.CongThanhTich` viết xong từ giai đoạn 1 mà **chưa hề có ai gọi**.

**Ai phán quyết: chủ phòng, một mình.** Để mỗi máy tự kết luận thì hai màn hình có thể báo hai người thắng khác nhau —
gói tin không đến cùng lúc, máy này thấy đối phương chết trước khi máy kia thấy mình chết. Chủ phòng đếm, rồi phát
`LoaiKetTran` **kèm cả bảng điểm**, nên mọi máy hiện đúng một kết quả.

**Ai chết thì máy của chính họ biết** (quy ước từ bước 5: mỗi máy là trọng tài của nhân vật mình). Cái chết đã đi theo
cờ `daChet` trong gói trạng thái — nhưng cờ ấy **không nói ai hạ**. Nên nạn nhân gửi thêm một gói `LoaiChet` 3 byte:
"tôi chết, kẻ hạ tôi ngồi ghế kia", đọc từ `Damageable.keDanhCuoi`.

**Ghi công mà không phải sửa mười một chỗ.** Mọi phép đã mang sẵn `boQua` — chính là người tung nó (để không tự thiêu
mình, làm ở bước 5). Nên chỉ cần ở ba chỗ tính sát thương (`Damageable.AreaDamage`, `Tornado`, `GiatSet`) ghi
`d.GhiKeDanh(boQua)` trước khi trừ máu. Đòn của quái không ghi gì, nên chết vì quái thì không ai được tính công.
Quái chết thì `GameDirector` cộng cho ghế của kẻ hạ nó — **chỉ ở máy chủ phòng**, vì máy khách chỉ vẽ lại đàn quái
nghe được, đếm ở đó là đếm cả những con mình không hề giết.

**Rời trận cũng là ra khỏi trận.** Người đóng tab không còn là "người sống sót". Không tính thế thì hai người đánh
nhau, một người thoát, người còn lại đứng giữa nghĩa địa đợi một kết quả không bao giờ đến.

Còn lại:

- **Chết rồi thì ngồi xem**: camera chuyển sang bám một người **còn sống** (phím R chơi lại đã bị chặn từ bước 5), HUD
  ghi "Đang xem trận đấu — chờ người sống sót cuối cùng."
- **Màn kết trận**: "BẠN SỐNG SÓT CUỐI CÙNG" (vàng) hoặc "<TÊN> ĐÃ THẮNG" (đỏ), dưới là bảng điểm ba cột — người chơi ·
  quái đã diệt · đã hạ — dòng của người thắng màu vàng, dòng của mình ghi "(bạn)". Cỡ chữ tiêu đề **tự thu nhỏ** cho
  vừa bề ngang màn hình.
- **Ghi thành tích**: mỗi máy tự ghi hồ sơ **của mình** (luật bảo mật Firestore chỉ cho sửa document của chính mình, và
  cũng không ai muốn điểm của mình do máy người khác quyết).
- `TranHienTai.Xoa()` xoá luôn kết quả — không thì vào trận sau là hiện ngay bảng điểm trận trước.

Đo (menu 55 mới — mở thẳng các kênh **giả lập** như menu 45, vì Editor không bắt tay WebRTC thật được):

```
A. gói tin: gói kết trận 10 byte, viết ra đọc lại nguyên vẹn; gói cụt đuôi bị từ chối
B. máy chủ phòng (3 người): giết 1 quái -> cộng đúng ghế; ghế 1 chết -> CHƯA xong; ghế 2 chết -> xong,
   ghế thắng 0, gói kết trận phát 4 lần tới cả kênh 1 và 2, bảng điểm "hạ 2 người / diệt 1 quái",
   nhân vật hết điều khiển được
C. máy khách: mình chết -> gửi gói chết đúng "ghế 1, kẻ hạ ghế 0"; CẢ BA cùng chết mà khách vẫn KHÔNG tự
   kết luận; nghe chủ phòng báo -> hiện đúng ghế thắng, tên, bảng điểm
D. chết rồi camera bám nhân vật người khác (ngồi xem)
E. màn kết trận: chụp ảnh, chữ đủ dấu, bảng điểm không đè nhau
0 lỗi. Không sót lại gì: về sảnh thì KetTran.DaXong = False
```

Lần chạy đầu báo một lỗi ở phần D — nhưng là **kịch bản thử sai**, không phải mã sai: tôi cho hai người kia chết
trước rồi mới đo camera, nên không còn ai để bám. Đảo lại thứ tự (mình chết trước, hai người kia còn sống) thì đúng.

Chạy lại các phép thử cũ sau khi sửa: menu 53 (HUD, 224 chuỗi, 0 lỗi), 45 (bốn người, 0), 44 (bước 5, 0),
49 (cầu lửa trúng người · khiên, 0).

**Chưa làm, và phải nói rõ**: mới chạy trên kênh giả lập trong Editor — **chưa thử trên hai máy thật**. Hai phần
"còn thiếu" của bước 5 (trọng tài phán xử máu, bù trễ khi tính trúng) vẫn còn nguyên.

#### Hai chỗ anh nhìn ảnh là thấy ngay

**"Ai vừa giết tôi?"** — câu hỏi đầu tiên khi gục ngã, mà màn hình thua không trả lời. Giờ ngay dưới "BẠN ĐÃ GỤC NGÃ"
có một dòng: **"Bị Người 1 hạ."**, **"Bị Bộ xương hạ."**, hay "Không rõ ai đã hạ bạn." Màn kết trận cũng có dòng ấy nếu
mình không phải người thắng. Tên quái viết tiếng Việt theo loại (Bộ xương, Mụ phù thủy, Quỷ dữ, Quỷ cây…).

Đòn của quái trước đây **không ghi công gì cả** — `GhiKeDanh` chỉ được gọi từ phép của người chơi. Thêm hai dòng trong
`EnemyAI` (đánh gần và đánh tầm xa) là xong: con quái ghi chính nó vào `keDanhCuoi` của nạn nhân.

**Nút "TRỞ VỀ" trên cảm ứng có vẽ mà bấm không ăn** — anh hỏi đúng chỗ hỏng. HUD chỉ đọc cú chạm vào nút khi
`director.PlayerDead` hoặc chủ phòng rời trận. Mà **người THẮNG thì còn sống**, nên cả hai đều sai: nút hiện ra trên màn
kết trận, bấm không có gì xảy ra, và trên điện thoại không có phím ESC để thoát — kẹt luôn trong màn kết quả. Giờ nhánh
ấy xét thêm `KetTran.DaXong`.

Đo thêm (menu 55, bản 5): kẻ hạ là người chơi → "Người 1"; kẻ hạ là quái → "Bộ xương" (loại Skeleton); trận xong thì
cần điều khiển bị khoá (`CamUng.Huong = (0,0)`, `DangKeo = False`) — chính là nhánh đọc nút TRỞ VỀ. Ảnh
`kettran_3_vua_bi_ha.png`. 0 lỗi.

Báo cáo lần chạy trước **in đôi từng dòng**: một lần chạy dở dang để lại đăng ký `EditorApplication.update`, lần sau
kịch bản chạy hai lượt chồng lên nhau. Gỡ đăng ký trước khi gắn, và bỏ qua nếu lượt cũ còn vật thể trong cảnh.

### Mỗi người một góc bản đồ, và luật đợt quái mới của Act2

Anh xin hai việc: hết đếm ngược 10 giây thì **mỗi người hiện ra một chỗ ngẫu nhiên**, không ai gần ai; và Act2 bỏ hết
luật quái cũ, thay bằng: **quanh mỗi người bốn con** (bộ xương, mụ phù thủy, quỷ cây, quỷ dữ), giết hết đợi **30 giây**
ra đợt sau **giống hệt + thêm quái bất kì cộng dồn**, mỗi đợt quái **mạnh hơn 5% máu và 5% sát thương**. Act1 giữ nguyên.

**Chỗ xuất phát: cái khó không nằm ở "ngẫu nhiên" mà ở "không gần nhau".** Muốn tránh nhau thì các máy phải **biết chỗ
của nhau** — mà lúc vừa vào màn chưa ai gửi gói tin nào. Mỗi máy tự bốc riêng thì hai người có thể rơi vào cùng một góc
nghĩa địa.

Cách làm (`ChoXuatPhat.cs`): **gieo hạt ngẫu nhiên từ MÃ PHÒNG** — con số mọi máy đều có sẵn và đều giống nhau. Cùng
một hạt, cùng bản đồ, cùng đoạn mã thì mọi máy tính ra **cùng một danh sách chỗ**; mỗi người lấy chỗ theo ghế của mình.
Không tốn một gói tin nào, không có cuộc đua nào. Chỗ hợp lệ: đứng trên đất (tia chiếu xuống chạm địa hình trước), không
dưới nước (so với hộp bao lưới nước), không vướng bia/đá/nhà (`CheckCapsule` lớp Default), và cách người trước **≥ 22 m**.

**Luật đợt quái Act2** (`GameDirector.SinhDotQuanhNguoi`): bật theo **tên scene** (`Act2`), nên Act1 không phải sửa gì —
kể cả chế độ chạy thử bốn bộ xương vẫn còn nguyên ở đó. Ở Act2, chế độ mới **tắt hẳn** rải quái khắp bản đồ, dòng quỷ dữ
và quỷ cây theo đồng hồ riêng, và chế độ bốn bộ xương.

- Quanh **từng** người chơi, không phải quanh một người: bốn người đứng bốn góc mà chỉ một người bị vây thì ba người kia
  đứng không. Quái rơi trong vành đai **7–13 m**.
- Quái cộng thêm **cộng dồn**: đợt 2 thêm 1, đợt 3 thêm 2 (thành 3), đợt 4 thêm 3 (thành 6) — đúng câu "y chang lần vừa
  rồi và cộng thêm". Hai người chơi: 8 → 9 → 11 → 14 con.
- Mạnh thêm 5% mỗi đợt: nhân **thẳng vào con vừa sinh** (`maxHealth`, `attackDamage`, `satThuongCau`), không đụng prefab
  — nếu sửa prefab thì đợt sau nhân chồng lên đợt trước và số sẽ phình theo cấp số nhân.
- Đợt đầu chờ 6 giây khi chơi mạng: bản sao của những người kia chỉ hiện ra sau khi bắt tay xong, sinh ngay thì quanh
  họ không có con nào.

Đo (menu 56 mới):

```
A. cùng mã phòng -> hai máy ra cùng danh sách chỗ; phòng khác -> chỗ khác;
   bốn chỗ cách nhau gần nhất 22,2 m (cần ≥ 22); 0 chỗ lơ lửng / dưới nước / vướng vật cản
B. Act2 đợt 1, hai người: 8 con = Skeleton 2, Witch 2, QuyCay 2, QuyDu 2; con xa người chơi nhất 11,9 m
C. đợt 2 = 9 con, máu và sát thương x1,050 | đợt 3 = 11 con, x1,103 | đợt 4 = 14 con, x1,158
D. Act1: chế độ đợt quanh người chơi = tắt, vẫn 4 bộ xương như cũ
0 lỗi
```

Hai lần phép thử tự báo lỗi oan, và cả hai đều là lỗi của **phép thử**, không phải của mã:

- Kịch bản chết giữa chừng ở phần D vì vật thể chạy thử **không được đánh dấu giữ qua lần nạp cảnh** — nạp Act1 là nó
  biến mất, báo cáo không bao giờ được ghi. (`Object.DontDestroyOnLoad`.)
- Bốn lỗi "máu không tăng 5%": tôi lấy con bộ xương **đầu tiên tìm thấy**, mà xác quái đợt trước còn nằm lại 6 giây —
  vớ phải xác đợt 1. Lọc `IsDead` là đúng ngay.

Menu 47 (chế độ bốn bộ xương) từ nay **chỉ đo Act1** — đo nó trên Act2 là đo một thứ không còn tồn tại ở đó.
Chạy lại menu 45 (bốn người) và 55 (kết trận): 0 lỗi.

### Kết trận xong thì game đứng hình — một hàm gọi lại rỗng làm sập cả vòng lặp

Trận đấu thật đầu tiên có người thắng: cả hai máy hiện đúng màn kết trận và bảng điểm, rồi **hiện một hộp lỗi
JavaScript** đầy `wasm-function[...]`, và từ đó **bấm ESC trên máy tính hay chạm nút TRỞ VỀ trên điện thoại đều không
có tác dụng** — không ai về sảnh được.

**Nguyên nhân, một dòng:** `KetTran.GhiThanhTich` gọi `HoSoMang.CongThanhTich(..., null)` — không cần biết kết quả nên
tôi truyền `null` làm hàm gọi lại. Mà cuối hàm ấy là `xong(ok);`, **không kiểm null** → `NullReferenceException`.

**Vì sao một ngoại lệ lại làm kẹt cả nút bấm:** trong vệt lỗi có `_JS_CallAsLongAsNoExceptionsSeen` — đúng như tên nó,
bản WebGL **ngừng hẳn việc gọi vòng lặp game khi thấy một ngoại lệ không ai bắt**. Game đứng hình: `Update` không chạy
nữa thì phím ESC không ai đọc, cú chạm nút TRỞ VỀ cũng không ai đọc. Một hộp lỗi = mất luôn đường về sảnh.

Sửa: `if (xong != null) xong(ok);` trong `HoSoMang`, và `KetTran` truyền một hàm gọi lại ghi nhật ký thật.

**Vì sao menu 55 không bắt được:** trong Editor không có `FirebaseMang.Uid`, nên `GhiThanhTich` thoát ngay ở dòng đầu —
đường đi hỏng **chưa bao giờ được chạy**. Giờ phép thử gọi **thẳng** `CongThanhTich` với hàm gọi lại rỗng, và **đếm
ngoại lệ** qua `Application.logMessageReceived` suốt cả bài.

Kiểm rằng phép thử mới thật sự bắt được lỗi (bỏ chỗ vá ra rồi chạy lại):

```
bỏ vá   -> [NGOAI LE] NullReferenceException ; so ngoai le moi: 1 ; so loi = 1
vá lại  -> so ngoai le moi: 0 ; tong so ngoai le ca phep thu: 0 ; so loi = 0
```

Bài học rộng hơn cho bản WebGL: **mọi ngoại lệ đều là lỗi chí mạng**, không phải "một dòng đỏ trong console". Chỗ nào
nhận hàm gọi lại từ ngoài cũng phải kiểm null trước khi gọi.

### Bàn phím điện thoại che kín ô nhập

Anh gửi ảnh chụp trên điện thoại: chạm vào ô nhập ở màn đăng nhập thì bàn phím trượt lên **che kín cả khung** — gõ mà
không thấy mình đang gõ gì.

**Unity không hề biết có bàn phím.** Bàn phím là của hệ điều hành, nó không làm đổi kích thước khung game (canvas), nên
`Screen.height` giữ nguyên. Chỉ **trang web** biết: trình duyệt có `window.visualViewport` — phần màn hình đang thực sự
nhìn thấy; bàn phím lên thì vùng ấy thấp đi.

Nên `index.html` đo rồi gọi sang game: `SendMessage("BanPhimAo", "DatChe", "<tỉ lệ>")`. **Gửi tỉ lệ chứ không gửi số
điểm ảnh**: trang đo bằng CSS pixel, game đo bằng pixel thật của canvas — hai hệ đơn vị khác nhau, nhân với
`Screen.height` ở phía game mới đúng. Bên game, `BanPhimAo.cs` là một vật thể sống qua mọi lần nạp cảnh và **phải tên
đúng "BanPhimAo"** vì `SendMessage` tìm theo tên.

Màn đăng nhập (`ManDangNhap.TinhBoCuc`, hàm thuần để đo được):

- Bàn phím lên thì **bỏ ảnh tiêu đề** — nó chiếm gần một phần tư màn hình, giữ lại thì không còn chỗ cho khung.
- Đẩy khung lên sao cho **đáy khung** nằm trên mép bàn phím.
- Không đủ chỗ thì **ưu tiên ô nhập**: đẩy tiếp cho **đáy ô Mật khẩu** nằm trên mép bàn phím, chấp nhận hai cái tab
  tràn lên khỏi mép trên. Mất tab thì còn đóng bàn phím rồi kéo lại được; mất ô nhập là gõ mù.

Đo (menu 57 mới, chạy thẳng trên hàm thuần — không cần Play, không cần điện thoại):

```
A. không bàn phím: 8 cỡ màn hình x 2 trang -> 0 lỗi, vẫn có ảnh tiêu đề, khung không tràn
B. bàn phím che 35% / 45% / 55%: 48 trường hợp -> 0 lỗi;
   chỗ chật nhất (844x390, che 55%, trang tạo tài khoản) ô cuối còn cách mép bàn phím 74 điểm
E. che càng cao khung càng lên: y = 319 -> 152 -> 60 -> -32 (âm = hai tab bị che, ô nhập vẫn nguyên)
```

Lần chạy đầu **phép thử bắt được lỗi thật**: trang "Tạo tài khoản" cao 616 đơn vị, bàn phím che 55% thì không đủ chỗ —
5/48 trường hợp ô mật khẩu vẫn bị che 14–40 điểm. Đó là lúc thêm luật "ưu tiên ô nhập, cho tab tràn lên".

Sảnh phòng không cần sửa: ô "Tên phòng" nằm ở phần trên màn hình (đáy ở 284 đơn vị), bàn phím che nửa dưới không tới.

### Cài được như một ứng dụng: iOS, Android và máy tính

Anh xin: trên iOS thêm vào Màn hình chính, trên Android và máy tính thì cài đặt / tải về như một ứng dụng, và dùng
**ảnh đầu ác quỷ anh gửi** làm bộ icon.

**Ba nền tảng, ba đường khác nhau** — đây là chỗ dễ tưởng làm một lần là xong:

| Nền tảng | Cần gì | Ai mở hộp thoại |
|---|---|---|
| Android, Chrome, Edge (máy tính) | manifest + **bộ chạy nền** (service worker có xử lý `fetch`) + HTTPS | Trình duyệt bắn `beforeinstallprompt`; ta **giữ lại** rồi gọi khi người chơi bấm nút |
| iOS Safari | các thẻ `apple-*` + `apple-touch-icon` | **Không có hộp thoại nào cả** — người dùng phải tự bấm Chia sẻ → "Thêm vào MH chính". Ta chỉ nhắc được |

Nên trang có một nút **"CÀI ĐẶT ỨNG DỤNG"** (Android/máy tính) và một dòng nhắc riêng cho iOS Safari; cả hai nằm **trong
màn chờ tải**, không phải nút nổi trên khung game — trên điện thoại, cạnh phải và cạnh dưới là chỗ của cần điều khiển
và cụm nút kỹ năng, để một nút HTML đè lên đó là cướp mất cú chạm của người chơi. Đã cài rồi (mở từ biểu tượng) thì
không hiện gì.

**Bộ chạy nền cố ý KHÔNG lưu cache gì.** Nó chỉ tồn tại để trình duyệt cho phép cài đặt. Bản game nặng 174 MB và Unity
**đã** tự quản lý cache riêng theo mã phiên bản; lưu thêm một bản nữa là tốn gấp đôi chỗ trên máy người chơi. Nguy hiểm
hơn: một bộ chạy nền giữ bản `index.html` cũ sẽ ghép **mã game cũ với dữ liệu mới** sau mỗi lần cập nhật — đúng cái lỗi
đã làm game sập lúc tải (mục "Game sập ngay lúc tải"). `sw.js` và `manifest.webmanifest` đều để `no-cache`.

**Icon** sinh từ ảnh anh gửi bằng `CongCu/Icon/sinh_icon.py`, ba kiểu vì ba luật khác nhau:

- **icon thường**: nền tối đặc. iOS và Windows không chấp nhận nền trong — chúng ghép lên nền trắng, ảnh đỏ trên trắng
  thì nhạt và viền đen bị chìm.
- **maskable**: Android **cắt** icon theo hình của máy (tròn, vuông bo góc, giọt nước). Phần chắc chắn không bị cắt chỉ
  là vòng tròn đường kính 80%, nên hình phải thu còn **68%** và đặt giữa.
- **apple-touch-icon** 180×180, nền đặc, không bo góc (iOS tự bo).

Đo trên trang thật (`PlayTestShots/pwa.txt`):

```
manifest: 200, Content-Type application/manifest+json, display=standalone, 8 icon (có 192, 512, maskable)
bộ chạy nền: đã đăng ký, trạng thái "active", phạm vi / ; sw.js 200, Cache-Control no-cache
8 icon tải thật: 192x192, 512x512, 1024x1024, maskable 192/512, apple 180x180, favicon 32, ảnh chia sẻ 1200x630
nút cài đặt: giả lập beforeinstallprompt -> nút hiện, bấm thì gọi prompt() của trình duyệt, xong thì tự ẩn
0 lỗi
```

### Việc còn phải làm

**169 MB là quá nặng**, nhất là trên điện thoại — nền tảng chính của game. Gần như toàn bộ nằm ở
`WebGL.data.unityweb` (155,7 MB), tức tài nguyên chứ không phải mã.

Console còn lặp lại một cảnh báo đáng chú ý:

```
WARNING: RGBA Compressed ASTC6X6 UNorm format is not supported, decompressing texture
```

Texture đang nén theo **ASTC** (định dạng của Android). WebGL trên máy tính không đọc được nên
phải **giải nén ra bộ nhớ** — vừa tốn RAM vừa mất thời gian mỗi lần nạp. Chọn lại định dạng nén
cho riêng nền tảng WebGL sẽ ăn cả hai đầu: file nhỏ hơn và không phải giải nén.

---

## Mưa băng và Sấm sét: người chơi bị đóng băng thì phải đứng yên thật

### Hiện tượng

Anh gửi hai ảnh chụp trên điện thoại: đứng giữa Act2, tung Mưa băng, và **tảng băng cứ dội
xuống đúng đầu mình**, hết tảng này đến tảng khác, trong khi chung quanh chẳng có gì. Kèm theo
đó là một nhận xét quan trọng hơn nhiều:

> "Hai skill ở trên, tôi cảm thấy đánh vào người chơi khác chỉ hiện lên hiệu ứng hình ảnh chứ
> không làm cho người chơi khác bị đứng yên và không thể sử dụng skill."

Anh nói đúng. Cả hai đều là lỗi thật, và lỗi thứ hai là lỗi nặng.

### Nguyên nhân 1 — người chơi miễn nhiễm với chính hệ trạng thái của game

Game có sẵn hai trạng thái `FrozenEffect` (đóng băng) và `StunnedEffect` (choáng). **Chỉ quái
đọc chúng**: `EnemyAI` mỗi khung hình đều hỏi "tôi có đang bị đóng băng không" rồi đứng im.
`PlayerController` thì **không hỏi một câu nào** — không có lấy một dòng nào nhắc đến hai
trạng thái đó.

Hậu quả: đắp lên người chơi lớp vỏ băng dày đến đâu thì đó cũng chỉ là **một lớp vật liệu phủ
lên hình**. Người bên trong vẫn chạy đủ tốc độ và vẫn bấm được cả bảy kỹ năng. Nhìn màn hình
thì thấy đối thủ đóng băng, nhưng đối thủ chẳng hề hấn gì — đúng như anh mô tả.

Sửa: `PlayerController` nay có `HeSoTocBang` và `DangBiKhoaCung`, đọc y hệt cách quái đọc.
Bị đóng cứng hoặc bị choáng thì tốc độ về 0 và `CastAt` từ chối mọi phép, kèm câu nhắc
"BẠN ĐANG BỊ ĐÓNG BĂNG!" / "BẠN ĐANG BỊ CHOÁNG!" — im lặng thì người chơi tưởng nút hỏng.

Chặn ở lúc **bắt đầu** niệm chú chứ không ngắt phép đang niệm dở: gói tin "tôi vừa tung phép"
đã gửi đi từ lúc bắt đầu, ngắt giữa chừng thì máy bên kia vẫn vẽ ra quả phép đó và vẫn tính
sát thương của nó — hai máy kể hai câu chuyện khác nhau.

### Nguyên nhân 2 — mưa băng coi chính người tung là một mục tiêu ngon

Mưa băng không rơi bừa: nó **ưu tiên nhắm vào kẻ địch** đang đứng trong vùng (quét
`OverlapSphere` theo `damageMask`). Khi chơi mạng, `damageMask` có cả lớp `Player` — vì phải
đánh được người chơi khác. Mà người tung thì gần như luôn đứng giữa vùng mình vừa nhắm, nên
**gần như tảng nào cũng chọn chính anh ta**.

Không mất máu (`AreaFreeze` có bỏ qua người tung), nhưng cả trận mưa đổ xuống đầu mình trong
khi kẻ địch bên cạnh không dính giọt nào. Sấm sét có đúng lỗi ấy ở `PickTarget`.

Sửa: một dòng `if (boQua != null && d == boQua) continue;` trong cả hai chỗ chọn mục tiêu, và
thêm một dòng nữa cho làn khí lạnh trong vùng — trước đây phù thủy đứng trong cơn bão của
chính mình cũng bị ướp lạnh chậm 55%.

### Hai lớp băng chồng nhau, đếm giờ riêng

Luật anh đặt cho Mưa băng có hai tầng: **chắc chắn chậm 50% trong 2 giây**, và **35% số lần
đóng cứng hoàn toàn 1,5 giây**. `FrozenEffect` cũ không diễn tả nổi: nó chỉ có một cặp
(`slow`, `remaining`), và "đóng cứng" được suy ra từ `slow > 0,85`. Hạ `slow` xuống 0,5 là mất
sạch dấu vết đóng cứng; giữ `slow = 1` là đứng im suốt cả 2 giây.

Nay tách hẳn: `dongCungConLai` là **một đồng hồ riêng**. Vỏ băng và lớp chậm chạy theo
`remaining`, đóng cứng chạy theo đồng hồ của nó. Hết 1,5 giây đóng cứng thì còn nửa giây lê
bước chậm — đúng như anh hình dung.

### Số đo (`PlayTestShots/bang_set.txt`, menu 58, 0 lỗi)

| Đo | Kết quả |
|---|---|
| Tảng băng rơi | cao 0,82–2,90 m, trung bình 1,601 m — **to thêm 15,6%** (trước: 1,385 m) |
| Cụm băng dưới đất | tổng cỡ 8 cụm 10,0901 (trước 9,1729) — **to thêm 10,0%** |
| Mưa băng, 1000 lần gieo | đóng cứng **35,5%**, làm chậm **100,0%** |
| Sấm sét, 1000 lần gieo | choáng **34,9%** |
| Người chơi không bị gì | đi 3,18 m trong 0,6 s, bấm phép → bay ra 1 |
| Người chơi bị **đóng cứng** | đi **0,00 m**, bấm 2 phép → bay ra **0** |
| Người chơi chỉ bị **chậm 50%** | đi 1,64 m = **51%** quãng đường bình thường |
| Người chơi bị **choáng** | đi **0,00 m**, bấm phép → bay ra **0** |
| Tan băng xong | đi lại 3,19 m — không bị giữ chân oan |
| Mưa băng của mình, 43 tảng | rơi vào mình **0**, rơi vào kẻ địch **36** |
| *Mẫu đối chứng*: bỏ dòng vừa vá | rơi vào mình **10/22** — phép đo này bắt được đúng lỗi cũ |
| Mưa băng **của người khác** rơi trúng mình | bị chậm ✔, có lúc bị đóng cứng ✔ |

Hàng áp chót là hàng quan trọng nhất của bảng: nó chứng minh phép thử **không phải lúc nào
cũng báo xanh**. Bỏ đúng một dòng vừa vá thì nó lập tức đỏ lên với đúng hiện tượng trong ảnh
anh gửi.

### Một lỗi ngầm lộ ra trong lúc đo

Phép đo di chuyển đầu tiên cho **0,00 m ngay cả khi nhân vật không bị làm sao**. Thủ phạm nằm
trong `HandleMovement`: dòng chặn bản sao mạng tự đi được viết là `if (!tuDocInput) return;`.
Bản sao mạng đúng là có cờ đó, nhưng **mọi phép thử bơm input cũng đặt đúng cờ đó** — nên từ
lúc dòng ấy ra đời, menu 30 và mọi phép đo di chuyển đều đo một nhân vật bị khoá chân mà không
ai biết. Nay điều kiện là "không đọc phím **và** máu do máy khác quyết", tức đúng nghĩa "là bản
sao mạng". Menu 30 chạy lại: đi 6,37 m, 0 lỗi.

### Đường đi của hiệu ứng khi chơi mạng (không đổi)

Vẫn theo quy ước cũ: **máy của ai quyết trạng thái người ấy**. A bắn B thì trên máy B phép ấy
được phát lại, B tự gieo xác suất cho chính mình, rồi B kể lại "tôi đang đóng cứng" trong gói
trạng thái 60 lần/giây để A vẽ theo. Menu 46 chạy lại sau khi đổi `FrozenEffect`: 0 lỗi.

---

## Sách phép: người chơi tự sắp xếp các ô kỹ năng

### Anh xin gì

Hai việc, trong cùng một tin nhắn kèm bản vẽ tay `cuasokynang.png`:

1. Bản cảm ứng bỏ hẳn nút tròn **đổi góc nhìn** (hình máy quay) ở góc phải trên; nút **con mắt**
   dọn lên đúng chỗ đó.
2. Thêm nút hình cuốn sách tên **"Sách phép"**. Bấm vào mở một bảng: cột trái là kho kỹ năng
   (cuộn được), phải trên là lời kể chi tiết của kỹ năng đang chọn (cuộn được), phải dưới là các
   **ô** — kéo thả từ cột trái vào. *"Trong này sắp xếp thế nào thì ra ngoài màn hình game sắp
   xếp giống như vậy."* Bản máy tính thì các ô tròn đổi thành **một hàng ô vuông nằm ngang**.

### Cái khó không nằm ở chỗ vẽ bảng

Bảy kỹ năng trước đây nằm cứng: nút số 1 luôn là Quả cầu lửa, ô vuông thứ ba luôn là Sấm sét.
Số hiệu kỹ năng vừa là **chỗ ngồi trên màn hình**, vừa là **thứ đi qua mạng** (gói "tôi vừa tung
phép số 2"). Cho người chơi xếp lại mà không tách hai thứ ấy ra thì hai máy sắp xếp khác nhau sẽ
bắn ra hai phép khác nhau.

Nên `SachPhep.cs` chỉ giữ **bản đồ ô → số hiệu kỹ năng**. Số hiệu không bao giờ đổi; chỉ chỗ ngồi
đổi. Mọi thứ đi ra ngoài (gói tin, `PlayerController.CastAt`, `MayNgam`) vẫn nhận số hiệu.

**Hai bộ ô riêng** — một cho bản cảm ứng, một cho bản máy tính. Một người chơi WebGL đổi qua đổi
lại giữa hai bản (trình duyệt trả lời có cảm ứng hay không), và bảy nút tròn xếp thành hai cung
thì không có "thứ tự trái sang phải" để mà dùng chung với hàng ô vuông.

Cất trong `PlayerPrefs` — trên WebGL chính là `localStorage`. Bản ghi hỏng hoặc số ô không khớp
thì bỏ hẳn, quay về thứ tự gốc: thà xếp lại từ đầu còn hơn bấm ô này ra phép kia.

### Ô trong bảng xếp đúng hình cụm nút ngoài trận

Chỗ này là ý anh nhấn mạnh nên tôi không vẽ một hàng ngang cho dễ: bảng dùng **chính hàm bố cục
của cụm nút thật** (`GameHUD.LechNut`), thu nhỏ cả cụm cho vừa khung. Ô thứ tư trong bảng nằm
đúng vị trí tương đối của nút thứ tư ngoài trận — không phải đoán.

Đo: 21 cặp ô, tỉ lệ thu nhỏ của mọi cặp đều là **0,6851**, lệch **0,00%**. Cụm không bị kéo méo.

### Những chỗ phải chặn, nếu không sẽ vừa sắp xếp vừa đánh nhau

| Chỗ | Nếu không chặn |
|---|---|
| `DocInput.Doc` | Mỗi cú chạm để kéo thả bị hiểu thành "bấm chuột xuống sân" — nhân vật chạy đi trong lúc đang sắp xếp |
| `GUI.Button` của thanh kỹ năng (máy tính) | Thanh vẫn nằm dưới bảng, mà IMGUI cho cái vẽ trước giành sự kiện — kéo ngang qua đó là một phát bắn ra |
| `GUI.Button` của chính nút Sách phép | Bấm vào vùng đó trong bảng sẽ đóng bảng giữa chừng |
| Ô trống trên cụm nút tròn | Không vẽ gì mà vẫn ăn cú chạm, thành một vùng bấm vô hình |

### Kéo dọc là cuộn, kéo ngang là mang kỹ năng đi

Trên điện thoại, một ngón tay phải làm hai việc trong cùng một cột. Phân biệt theo hướng: đi
dọc nhiều hơn ngang (gấp 1,2 lần) thì là **cuộn danh sách**; còn lại là **kéo thả**. Không tách
thì mỗi lần vuốt để xem kỹ năng phía dưới là một lần kéo thả hụt.

Thả vào ô đã có kỹ năng thì **hai ô đổi chỗ cho nhau**, không nhân bản — kéo Sấm sét từ ô 3 sang
ô 1 mà để nguyên ô 3 thì người chơi có hai nút Sấm sét và mất một kỹ năng khác.

### Số đo (`PlayTestShots/sachphep.txt`, menu 59, 0 lỗi)

| Đo | Kết quả |
|---|---|
| Bố cục bảng, 8 cỡ màn hình × 2 bản | 16 trường hợp, **0 lỗi** — không khung nào đè khung nào, không ô nào tràn |
| Hai ô gần nhau nhất | màn 844×390 (điện thoại nhỏ nhất), còn hở **3,5 điểm** |
| Ô tròn so với cụm nút thật | 21 cặp, tỉ lệ 0,6851 → 0,6851, lệch **0,00%** |
| Đặt kỹ năng 5 vào ô 0 | `5 1 2 3 4 0 6` — đổi chỗ, không nhân bản |
| Lưu rồi nạp lại | y nguyên |
| Sửa bản cảm ứng | bản máy tính vẫn `0 1 2 3 4 5 6` |
| Đang mở bảng | gói input = hướng (0,0,0), kỹ năng −1, không đòi đi — trận đấu bị khoá hẳn |
| Kéo Thiên thạch vào ô 1 | cụm nút ngoài trận đổi theo ngay (ảnh `sachphep_4`) |

Ảnh: `sachphep_1_hud_camung`, `sachphep_2_bang_camung`, `sachphep_3_bang_maytinh`,
`sachphep_4_hud_da_doi_cho`.

---

## Cấp độ, kinh nghiệm và điểm kỹ năng

> **Mọi con số nằm trong `kinhnghiem.md`** — bảng kinh nghiệm từng cấp, giá của từng loại quái,
> lên cấp được gì, nâng kỹ năng được gì. Mục này chỉ kể những chỗ **khó** khi làm.

### Cấp tính theo trận, không cất lại

Mỗi trận là một ván đấu riêng (người sống sót cuối cùng thắng). Giữ cấp giữa các trận thì người
chơi lâu năm vào trận với cấp 10 trong khi người mới cấp 1 — không còn là một ván đấu nữa. Nên
`CapDo.BatDauTranMoi()` được gọi trong `GameDirector.Awake`, tức mỗi lần vào màn.

Đặt ở `GameDirector` chứ không ở `PlayerController`: **bản sao của người chơi khác cũng là một
`PlayerController`**, mà chúng được dựng lên giữa trận — xoá sạch ở đó thì mỗi lần có người vào
là cả phòng tụt về cấp 1.

### Chỗ khó nhất: sức mạnh kỹ năng khi chơi mạng

Sát thương của phép **không được tính theo cấp của người xem**. A nâng Mưa băng lên cấp 5 rồi bắn
sang B: trên máy B phép ấy được phát lại để tính trúng, và nếu máy B lấy cấp Mưa băng *của B* thì
đòn đó yếu đi — hai màn hình hiện hai con số sát thương khác nhau cho cùng một cú đánh.

Nên **cấp kỹ năng đi kèm từng lần tung phép**: gói kỹ năng thêm một byte, từ 16 lên 17 byte.
`PlayerController.capPhepDangTung` giữ cấp ấy tới lúc phép thật sự bay ra (`Release`), rồi nhân
vào sát thương và cộng vào thời gian hiệu ứng.

### Ai chia kinh nghiệm cho ai

| Việc | Ai biết | Đi đường nào |
|---|---|---|
| Giết quái | **chủ phòng** — chỉ nó chạy trí tuệ quái, máy khách chỉ vẽ lại đàn quái nghe được | chủ phòng gửi gói `LoaiKinhNghiem` 4 byte (ghế + điểm) tới máy người hạ |
| Giết người chơi | **máy nạn nhân** — nó là trọng tài cái chết của mình | đã có sẵn: gói `LoaiChet` mang theo ghế của kẻ hạ, máy kẻ hạ nghe thấy thì tự cộng |

Gói chết được gửi lại vài lần cho chắc, nên chỗ cộng kinh nghiệm phải kiểm "đây có phải lần đầu
nghe không" — không thì hạ một người được cộng ba bốn lần.

### Kỹ năng khoá phải khoá ở cả ba nơi

Một kỹ năng chưa mở mà chỉ chặn ở một chỗ là lọt: người chơi vẫn bấm được từ chỗ còn lại.

1. `PlayerController.CastAt` — từ chối và **nói ra lý do** ("… chưa mở khoá — vào SÁCH PHÉP để mở").
2. Cụm nút tròn / thanh ô vuông — vẽ xám kèm hình ổ khoá, và `NutTaiDiem` không nhận ngón tay ở đó.
3. Sách phép — không kéo thả được kỹ năng còn khoá (kéo được thì nó nằm trên thanh như một nút
   thật, bấm vào chỉ hiện ra lời từ chối).

### Lên cấp thì cộng máu thế nào

Máu tối đa +15%, và **máu đang có cũng được cộng đúng phần vừa tăng thêm**. Không hồi đầy (lên
cấp thành một bình máu miễn phí), nhưng cũng không để người chơi tụt tỉ lệ: đang 50% máu mà chỉ
kéo trần lên thì tự nhiên còn 43%.

### Số đo (`PlayTestShots/capdo.txt`, menu 60, 0 lỗi)

| Đo | Kết quả |
|---|---|
| Bảng kinh nghiệm | 100 · 135 · 180 · 245 · 330 · 445 · 600 · 810 · 1090 — **tổng 3 935** để đạt cấp 10 |
| Một đợt Act2 (4 con quanh mình) | 120 kinh nghiệm — đủ lên cấp 2 ngay đợt đầu |
| Cộng một phát 500 từ cấp 1 | lên cấp 4, còn dư 85 — nhảy nhiều bậc và giữ phần thừa |
| Cả trận | 10 điểm kỹ năng (1 lúc đầu + 9 lần lên cấp) |
| Nhân vật cấp 10 | máu ×3,518 · năng lượng ×2,358 · tốc độ ×1,363 |
| Kỹ năng cấp 5 | sát thương ×2,074 · năng lượng ×1,464 · hiệu ứng +0,60 s · máu khiên ×1,749 |
| Vào trận | cấp 1, máu 600, năng lượng 250, tốc độ 5,20, **0 kỹ năng mở** |
| Bấm kỹ năng chưa mở | **0 phép bay ra**, báo "MƯA BĂNG chưa mở khoá — vào SÁCH PHÉP để mở" |
| Lên cấp 2 (đo thật trên nhân vật) | máu 600 → 690 (×1,150) · năng lượng 250 → 275 (×1,100) · tốc độ 5,20 → 5,38 (×1,035) |
| Mưa băng cấp 1 → cấp 5 (đo trên cơn bão thật) | sát thương 29,04 → **60,22** (×2,074) · đóng cứng 1,50 s → **2,10 s** |
| Giết một con quỷ cây | **+30** kinh nghiệm, đúng bảng giá |
| Gói mạng | kỹ năng 17 byte giữ đúng cấp 5; gói kinh nghiệm 4 byte đúng ghế và điểm |

### Ổ khoá vẽ lại, và ba lỗi hiển thị anh chụp được (13/09/2026)

| Anh thấy | Nguyên nhân | Sửa |
|---|---|---|
| Ổ khoá quá đơn giản, không hợp không khí game | ghép từ 4 hình chữ nhật trắng | `IconKhoa.cs`: **một ảnh sinh bằng code** (trường khoảng cách, mép mềm) — quai sắt tròn có ánh sáng dọc thân, thân sắt gỉ viền đỏ, 4 đinh tán, lỗ khoá hình giọt hắt ánh đỏ, viền đen + bóng đổ |
| Bản máy tính: ổ khoá chỉ là **hình vuông trắng** | hàm của bản máy tính chỉ vẽ **một thanh ngang**, quên mất cái quai | cả hai bản gọi **chung một hàm** `IconKhoa.Ve` — không thể lệch nhau nữa |
| Sách phép: ô tròn đè lên dòng chữ hướng dẫn | cụm nút được căn giữa **cả** vùng ô, gồm cả dải chữ | chừa dải chữ (`DongNhacO`, dùng chung lúc vẽ và lúc xếp ô); căn giữa theo **hộp bao thật** của bảy nút thay vì hộp tính từ góc màn hình (hộp cũ còn cả lề 101 điểm nên cụm bị đẩy lệch, chừa khoảng trống lớn bên dưới) |
| *(thấy thêm khi sửa)* sau mỗi ô tròn lộ ra một ô vuông xám | nền ô dùng `GiaoDien.Trang` — một ảnh vuông | nền tròn mép mềm |

**Số đo** (menu 59, 0 lỗi): ổ khoá rộng **×0,800** so với bản cũ trên cả nút tròn (31,6 → 25,3) lẫn ô vuông (28,6 → 22,8);
đọc thẳng điểm ảnh: lòng chữ U của quai alpha **0,03**, hai chân quai / đỉnh quai / thân **1,00** — ảnh có quai thật.
Ô tròn gần dòng chữ nhất trên 16 trường hợp vẫn còn hở; cụm ô to thêm **18%** (tỉ lệ thu nhỏ 0,5133 → 0,6080) nhờ căn theo hộp bao thật, hình cụm vẫn lệch **0,00%**.

### Lỗi nghiêm trọng: giết bằng Mưa băng, Sấm sét… không được kinh nghiệm (13/09/2026)

**Hiện tượng anh báo:** giết quái hoặc người chơi khác bằng Mưa băng và Sấm sét thì không nhận được
kinh nghiệm.

**Nguyên nhân:** kinh nghiệm, bảng điểm cuối trận và dòng "Bị … hạ" đều đọc một trường duy nhất —
`Damageable.keDanhCuoi` ("ai đánh đòn cuối"). Trường ấy chỉ được ghi ở chỗ nào **nhớ gọi**
`GhiKeDanh`. Đường gây sát thương nào quên, kẻ giết thành **vô danh** và không ai được gì.

Tôi không đoán từ code mà viết **menu 61** chạy trên code chưa sửa trước: tung từng kỹ năng thật vào
một con quái máu 1. Kết quả: **11 lỗi** — rộng hơn anh thấy nhiều.

| Đường gây sát thương | Trước | Sau |
|---|---|---|
| Quả cầu lửa (nổ) | ✔ | ✔ |
| Thiên thạch (nổ) | ✔ | ✔ |
| **Mưa băng** (`AreaFreeze`) | ✘ không ghi | ✔ |
| **Sấm sét** (`AreaShock`) | ✘ không ghi | ✔ |
| **Lốc xoáy** — bị cuốn, bị ném xuống đất (`WhirledEffect`) | ✘ không ghi | ✔ nhớ người tung ngay lúc hút vào, kể cả khi cơn lốc đã tan |
| **Cháy theo thời gian** (`BurningEffect`) | ✘ không biết ai gây cháy | ✔ `keGayChay` |
| **Vũng lửa của Thiên thạch** (`VungLua`) | ✘ không được giao người tung — còn **đốt cả chính người tung** khi chơi mạng | ✔ |
| **Cây bị đốt cháy** (`CayChay`) | ✘ không biết ai đốt | ✔ `keDot` |
| Giựt sét | ✔ (lần đầu phép thử báo sai: con quái đứng đúng chỗ vũng lửa Thiên thạch còn cháy — nay dọn sạch phép cũ giữa các lần) | ✔ |

**Số đo** (`PlayTestShots/kinhnghiem_kynang.txt`, menu 61): **11 lỗi → 0**. Sáu kỹ năng gây sát thương
đều giết được, "kẻ đánh cuối" đúng là người tung, kinh nghiệm cộng đúng giá con quái (+18, +30, +32,
+40). Tách riêng các đường chết chậm (cháy, bị cuốn, vũng lửa, cây cháy) để đòn trực tiếp không che
mất lỗi. Phần **người chơi khác làm nạn nhân**: Mưa băng, Sấm sét, vụ nổ và lửa cháy trúng nhân vật
đều ghi đúng người kia — đó là trường máy nạn nhân gửi đi trong gói báo tử. Menu 55 (kết trận) chạy
lại vẫn 0 lỗi.

### Hai lần phép thử báo đỏ oan

Lần đầu nó báo "sát thương không tăng theo cấp" vì tôi tung Mưa băng lần hai chỉ **0,9 giây** sau
lần đầu, trong khi kỹ năng này hồi chiêu **6 giây** — phép bị từ chối, không có cơn bão nào trong
cảnh, và số đo ra 0.

Sửa xong vẫn đỏ: chờ 6,6 giây đứng giữa Act2 thì **nhân vật bị đàn quái giết**, mà người chết thì
không niệm chú được nữa. Nay phép thử cho máu thật lớn trong lúc chờ, và in thêm dòng "nhân vật
còn sống: True" để lần sau đọc số đo là biết ngay.

---

## Thiên thạch đánh ngã, và luật đợt quái mới của Act2

### Thiên thạch: 40% đánh ngã kẻ địch 1,5 giây

Anh xin: quả thiên thạch rơi xuống có 40% đánh ngã kẻ địch; kẻ bị ngã bị **hất nhẹ lên khỏi mặt đất rồi rơi
xuống nằm ngửa**, không đi được, không đánh được kỹ năng.

`BiDanhNga.cs` là một trạng thái mới, đứng cạnh đóng băng và choáng. Chỗ khó là cái hình:

- **Lật hình, không lật nhân vật.** Mọi nhân vật có con nhộng va chạm (`CharacterController`) ở gốc và model
  ở con đầu tiên. Xoay gốc thì va chạm xoay theo, lại bị trí tuệ quái / điều khiển ghi đè hướng mỗi khung
  hình. Xoay **model con** thì va chạm vẫn đứng yên dưới đất, chỉ hình là nằm xuống — và hoạt hình của quái
  chỉ ghi các khớp xương bên trong, không đụng tới transform này.
- Xoay quanh **trục của gốc nhân vật** chứ không quanh trục riêng của model: model Meshy nhập vào hay xoay sẵn
  180 độ, xoay theo trục riêng là nhân vật ngã **úp mặt**.
- Nhịp: 0,38 giây đầu bị hất lên theo đường parabol (cao 0,9 m) và lật ngửa dần → nằm → 0,28 giây cuối chống
  đứng dậy. Khoá di chuyển và kỹ năng trong suốt 1,5 giây. Chết trong lúc nằm thì giữ nguyên tư thế nằm.
- Chỉ **thiên thạch của người chơi** đánh ngã. Quỷ dữ cũng ném thiên thạch bằng cùng mã, nhưng anh chỉ xin cho
  kỹ năng — nên mặc định là 0%, người chơi tung thì mới đặt 40%. Cấp kỹ năng cao kéo dài thêm 0,15 giây mỗi cấp
  như mọi hiệu ứng khác. Người đang bật Khiên thì không bị ngã (khiên đỡ trọn đòn thì đỡ luôn hiệu ứng).
- **Qua mạng**: thêm một bit "đang ngã" vào byte hiệu ứng có sẵn (không thêm byte nào). Máy của ai quyết người
  đó có bị ngã không; các máy khác chỉ vẽ lại, gói ngừng đến thì bản sao tự đứng dậy.

**Chữ nổi trên đầu mất dấu** — tìm thấy khi làm chữ "NGÃ!": hệ vẽ chữ nổi dùng font mặc định, và hai chữ cũ viết
không dấu ("CHOANG!", "BI CUON!"). Editor vẫn hiện đúng vì Windows vẽ bù, lên web thì mất dấu. Nay dùng font
Inter, chữ thành "CHOÁNG!", "BỊ CUỐN!", "NGÃ!".

**Số đo** (`PlayTestShots/danhnga.txt`, menu 62, 0 lỗi):

| Đo | Kết quả |
|---|---|
| 1000 lần gieo | đánh ngã **38,7%** (lần chạy trước 41,9%) — mong đợi 40% |
| Thiên thạch của Quỷ dữ | 0% |
| Quả thiên thạch nhân vật tung thật | mang đúng **40% · 1,5 giây** |
| Bị hất lên | hình lên cao tối đa **0,98 m** |
| Lúc nằm (đo **vị trí xương đầu thật**, không đọc lại góc vừa đặt) | đầu cách đất **0,21 m** (đứng thẳng 1,36 m), đầu nằm **1,42 m về phía sau lưng** → nằm ngửa |
| Trong 1,35 giây | vẫn đang ngã, quái di chuyển **0,00 m** |
| Hết 1,5 giây | hết ngã, đầu về lại **1,42 m** — đã đứng dậy |
| Người chơi bị ngã | đẩy cần 0,6 giây đi **0,00 m**, bấm phép ra **0**, báo "BẠN ĐANG BỊ HẤT NGÃ!" |
| Qua mạng | bit "đang ngã" được gửi; bản sao không tự gieo; gói tới thì ngã, gói ngừng thì tự đứng dậy |

### "Đánh rất nhiều lần mà không thấy đánh ngã" (13/09/2026)

**Anh báo:** tự thử Thiên thạch vào quái và người chơi khác rất nhiều lần, không lần nào thấy hiệu ứng đánh ngã.

**Phép thử lần trước có lỗ hổng:** nó gọi thẳng hiệu ứng ngã và chỉ đọc tham số của quả thiên thạch — chưa lần
nào cho **một quả thật rơi trúng một con quái thật** rồi đếm. Phần F mới làm đúng việc anh làm: nhân vật tung
Thiên thạch 10 lần vào một con quái. Kết quả: **10/10 trúng, 8/10 bị ngã** — đúng mức ~78% (mỗi lần 3 quả, mỗi quả
gieo riêng 40%). Hiệu ứng **có xảy ra**.

**Nguyên nhân thật — không nhìn thấy:** chụp 5 ảnh trong suốt 1,5 giây nằm (0,25 · 0,60 · 0,95 · 1,20 · 1,40 s):
**ba quả thiên thạch nổ nối nhau** phủ lửa kín chỗ con quái ở mọi thời điểm, còn chữ "NGÃ!" trôi lên mất sau nửa
giây giữa cả chục con số sát thương. Anh không thấy là đúng.

**Sửa** (giữ nguyên 40% và 1,5 giây anh chốt): suốt lúc bị ngã, trên đầu kẻ bị ngã có **chữ NGÃ viền đen và vòng
sao vàng xoay**, vẽ ở **lớp giao diện** (OnGUI) nên lửa 3D không che được, đặt ở độ cao 2,7 m — **cao hơn chỗ số
sát thương hiện ra** (2,1 m) để số không đè lên chữ. Bỏ chữ "NGÃ!" trôi cũ.

**Số đo** (menu 62, 0 lỗi): tung thật 10 lần → trúng 10, ngã 7; dấu hiệu hiện **376/383 khung hình đang ngã (98%)**;
chuỗi ảnh mới thấy chữ NGÃ rõ ở cả 5 thời điểm, đè lên lửa.

### Kỹ năng mới: Tốc biến — dịch chuyển tức thời 15 m (18/09/2026)

Anh xin kỹ năng **"Tốc biến"** xếp vào nhóm **Hỗ trợ**: bấm là nhân vật dịch chuyển tức thời tới chỗ khác trong
**15 m**, có hiệu ứng **biến mất** ở chỗ đang đứng và **hiện ra** ở chỗ mới; hồi chiêu **5 giây**, **mỗi cấp giảm
0,25 giây**. Tôi hỏi thêm, anh chốt: **40 năng lượng**; ngắm vào chỗ không đứng được thì **vẫn nháy nhưng lùi về
điểm trống gần chỗ ngắm nhất** (tức đi xuyên được tường, bia mộ, miễn điểm đến đứng được); **không niệm chú**.

**Cách làm:**
- `TocBien.TimChoDen`: đi từ điểm ngắm (đã kẹp 15 m) **lùi dần 0,5 m** về phía người chơi cho tới khi gặp chỗ đứng
  được (`Physics.CheckCapsule` cao 1,8 m bán kính 0,35 không chạm vật cản). Tìm **trước khi trừ mana**, không có chỗ
  nào thì từ chối và không mất gì.
- `TocBien.Nhay`: **tắt `CharacterController` trước khi đổi `transform.position`** rồi bật lại — Unity giữ vị trí
  riêng trong đó, đổi transform mà không tắt thì khung sau nó kéo người về chỗ cũ.
- "Không niệm chú" làm bằng `castTime = 0,01 giây`: vừa đủ một khung hình để gói tin *"tôi vừa tung phép"* bay sang
  máy khác (không thì người ta chỉ thấy nhân vật **trượt** 15 m), mà người chơi vẫn thấy là tức thời — đo được
  **0,025 giây** từ lúc bấm đến lúc đổi chỗ. Bản sao trên máy người khác **chỉ chạy hiệu ứng**, không tự kéo mình đi:
  vị trí của họ do gói trạng thái quyết định.
- Hiệu ứng (`VfxTocBien.cs`, dựng bằng code): chỗ cũ vòng sáng **thu vào** + khói tím bay lên; chỗ mới vòng sáng
  **nở ra** + cột sáng ngắn + hạt toé, kèm một ngọn đèn tím loé.

**Một lỗi thật, phép thử bắt được ngay:** mục D ngắm thẳng vào giữa một khối đá đặc mà kết quả báo *"chỗ đó đứng
được"*. Nguyên nhân: tôi dùng `VfxFactory.GroundY`, mà hàm ấy tính **cả lớp Default** — nó trả về **nóc khối đá**,
nên "trong lòng đá" biến thành "trên nóc đá" và hợp lệ. Người chơi sẽ nháy tót lên mái nhà mồ. Sửa: `TocBien.MatDatY`
chỉ bắn tia vào **lớp Ground**, đúng như `GioLoc.MatDatY` đã phải làm khi lốc trèo lên mái nhà.

**Số đo** (menu 76, `tocbien.txt`, **0 lỗi**; ảnh `tocbien_1_vua_nhay.png`):

| Đo | Kết quả |
|---|---|
| Thông số | số hiệu 15, 16 kỹ năng; **40 năng lượng** (trừ đúng 40), hồi chiêu **5,00 s** ở cấp 1, tầm 15 m; Sách phép xếp vào nhóm **HỖ TRỢ** |
| Hồi chiêu theo cấp | 5,00 / 4,75 / 4,50 / 4,25 / **4,00** giây |
| Nháy | ngắm 10 m → đi đúng **10,00 m**, lệch chỗ ngắm **0,00 m**; có cả hai hiệu ứng trên cảnh |
| Không niệm | từ lúc bấm đến lúc đổi chỗ **0,025 giây** |
| Kẹp tầm | ngắm ra 30 m → chỉ nháy **15,01 m** |
| Chỗ không đứng được | ngắm vào giữa khối đá (đứng được: **False**) → vẫn nháy **5,98 m** và dừng **cách khối đá 2,02 m**, chỗ đến đứng được |
| Hồi quy | menu 59 (Sách phép, nay 16 kỹ năng): **0 lỗi** |

**Cấp 5: gỡ trói (18/09/2026, anh xin thêm ngay sau đó).** *"Khi đạt cấp 5, sẽ có thể xoá bỏ tất cả mọi trạng thái
bất lợi, có thể dùng ngay khi bị choáng, đánh ngã, đóng băng, hất bay. Chỉ có duy nhất bị cuốn vào Lốc xoáy thì
không thể Tốc biến được."*

- `PlayerController.CastAt` vốn chặn cứng: bị đóng băng / choáng / ngã / hất tung thì **không phép nào** tung được.
  Nay Tốc biến cấp 5 là **ngoại lệ duy nhất** — `TocBien.CapNamGoTroiDuoc` cho đi qua cái chặn ấy.
- Nhảy xong thì `TocBien.GoSachTrangThai` xoá sạch `FrozenEffect` (gọi `Thaw()` để trả lại vật liệu gốc, không thì
  vỏ băng dính lại trên người), `StunnedEffect`, `BiDanhNga`, `BiHatTung`, `BurningEffect`. Cả `BiDanhNga` lẫn
  `BiHatTung` đều khôi phục tư thế hình trong `OnDestroy` nên xoá thẳng là an toàn.
- Ngoại lệ: đang bị **Lốc xoáy** cuốn (`WhirledEffect`) thì chịu — lúc ấy người chơi đang bay vòng quanh trục lốc,
  không còn đứng trên mặt đất. Bấm thì hiện chữ **"BẠN ĐANG BỊ LỐC XOÁY CUỐN!"**.

**Một cái bẫy phép thử suýt cho tôi màu xanh giả.** Ca đo đầu tiên tôi dính **năm** trạng thái cùng lúc — đóng băng,
choáng, ngã, hất tung, **và cháy** — rồi kiểm "dính ≥ 4". Số đo trả về đúng **4**, phép thử vẫn xanh. Nhìn kỹ mới
thấy `BurningEffect.Start` có dòng *"lửa thiêu làm tan băng"*: nó gọi `frozen.Thaw()`, nên **cái đóng băng đã bị lửa
gỡ trước khi tôi kịp đo** — tức là thứ anh gọi tên đầu tiên lại là thứ phép thử không hề kiểm. Sửa: ca chính chỉ
bốn trạng thái anh nêu và bắt **đúng 4** (đóng băng kiểm bằng `IsFullyFrozen`, không phải "có component"), còn cháy
đo thành một ca riêng.

**Số đo cấp 5** (menu 76 mục G, **0 lỗi**):

| Đo | Kết quả |
|---|---|
| Đối chứng cấp 4 | đang choáng mà bấm → **bị chặn**, và vẫn còn choáng |
| Cấp 5 | dính **4/4** trạng thái (đóng băng + choáng + ngã + hất tung) → **bấm được**, nháy **8,00 m**, còn lại **0** trạng thái |
| Cháy (đo riêng) | đang cháy → nháy xong **hết cháy** |
| Lốc xoáy | bị `WhirledEffect` cuốn thật → bấm Tốc biến **bị chặn** (ngoại lệ duy nhất) |

### Gió lốc hồi mana, và kỹ năng mới "Hoá Lốc Xoáy" (18/09/2026)

Anh xin hai việc cùng lúc. **Gió lốc**: mỗi lần đánh trúng kẻ địch thì hồi **10 mana cố định cho cả 5 cấp**, và
**cấp 5 chỉ tốn 25 năng lượng**. **Kỹ năng mới "Hoá Lốc Xoáy"**: bấm là cơn Gió lốc đang bay của lần tung gần nhất
**phình to thành Lốc xoáy**, giữ tốc độ bay của Gió lốc và mọi hiệu ứng cuốn bay lên của Lốc xoáy; sát thương tổng =
cấp hiện tại của Gió lốc + cấp hiện tại của Lốc xoáy; 45 năng lượng, hồi chiêu 0,5 giây.

Tôi hỏi trước, anh chốt: **giữ cả hai kiểu đòn** (chạm vào ăn ngay 75 của Gió lốc, rồi bị cuốn lên và chịu tiếp
20/giây + tia sét của Lốc xoáy); cơn lốc mới sống **đủ 6 giây** của Lốc xoáy; **không có** cơn Gió lốc nào đang bay
thì **từ chối, không tốn mana**; Gió lốc cấp 5 ra hai cơn thì **một lần bấm hoá cả hai**.

**Cách làm:**
- `GioLoc.ManaHoiMoiLanTrung` = 10, cộng trong `TrungMot` qua `HoiManaChoNguoiTung`. ⚠️ Chỉ cộng trên **máy của
  chính người ấy** (`pc.tuDocInput`): bản sao mạng cũng chạy hàm này, cộng vào đó là hai máy kể hai con số khác nhau.
- `GioLoc.NangLuongCan(cấp, gốc, hệ số cấp)`: cấp 1–4 giữ công thức cũ, **cấp 5 trả thẳng 25**.
- Kỹ năng **14** `HoaLocXoay` (static, không phải MonoBehaviour): mỗi cơn Gió lốc nay mang `lucTung` (Time.time lúc
  bấm) nên tìm được "lần tung gần nhất"; cấp 5 hai cơn cùng một mốc nên hoá cả hai. Hoá = sinh `Tornado` ngay chỗ
  cơn Gió lốc, đặt `moveSpeed = GioLoc.TocDo` (9,5 thay vì 3,4), nhân sát thương mỗi giây / tia sét theo cấp Lốc xoáy,
  đặt `Tornado.donChamMotLan` = 75 × cấp Gió lốc (trường mới, đánh **một lần mỗi kẻ** lúc bị cuốn), rồi huỷ cơn Gió lốc.
- Hiệu ứng **to dần**: `PhinhToThanhLoc` chỉ đổi **tỉ lệ hình** từ 0,42 lên 1,00 trong 0,55 giây — không đụng vào sức
  hút hay sát thương, không thì kẻ đứng sát cơn lốc đang phình sẽ không bị cuốn và nhìn như lốc hỏng.
- `CastAt` hỏi `HoaLocXoay.CoLocDeHoa` **trước khi trừ mana**: bấm hụt thì không mất gì.

**Số đo** (menu 75, `hoalocxoay.txt`, **0 lỗi**):

| Đo | Kết quả |
|---|---|
| Hồi mana | một cơn Gió lốc quét 3 bia → trúng 3 lần, hồi đúng **30 mana**; ĐỐI CHỨNG bắn vào chỗ trống → **0** |
| Năng lượng Gió lốc | cấp 1–4: 20,0 / 22,0 / 24,2 / 26,6; **cấp 5: 25,0** (bản cũ 58,6) |
| Hoá | trước khi hoá 1 cơn Gió lốc → sau khi hoá **0 Gió lốc, có Lốc xoáy**, lệch chỗ **0,00 m** |
| Giữ tốc độ | Lốc xoáy hoá ra bay **9,5 m/s** (Lốc xoáy gốc chỉ 3,4), sống **6,0 giây** |
| Sát thương | đòn chạm một lần **75,0** = Gió lốc cấp hiện tại; sát thương mỗi giây **20,0** của Lốc xoáy; bia đứng yên mất **388 máu** |
| To dần | tỉ lệ hình ngay khi hoá **0,42** → sau 0,55 giây **1,00** |
| Bấm hụt | không có Gió lốc nào đang bay → **từ chối**, mana 302,5 → 302,5, hồi chiêu **0,00 s** |
| Cấp 5 | Gió lốc cấp 5 ra 2 cơn → một lần bấm hoá **cả hai**, còn 0 Gió lốc |
| Hồi quy | menu 71 (Gió lốc) và 59 (Sách phép, nay 15 kỹ năng): **0 lỗi** |

Hai lần phép thử tự báo oan, cùng một kiểu và đáng ghi: **đo vị trí một vật đang bay thì phải đo đúng khoảnh khắc**.
Lần đầu tôi đọc chỗ cơn Gió lốc *trước khi niệm* rồi so với Lốc xoáy sau khi hoá → lệch 5,12 m, đúng bằng quãng đường
9,5 m/s × 0,55 giây niệm chiêu. Sửa xong lại lệch 9,35 m vì lần này chính **cơn Lốc xoáy** đã bay tiếp trước lúc tôi
đọc. Chỉ khi ghi cả hai vị trí **ngay khung hình đầu tiên thấy cơn lốc mới** thì số đo mới ra 0,00 m. Tỉ lệ "to dần"
cũng vậy: đọc trễ 0,5 giây thì thấy 0,91 thay vì 0,42 — phải theo dõi suốt trong vòng lặp.

### Giựt sét vẽ lại theo ảnh mẫu, phù thủy đẩy hai tay (25/09/2026)

Anh gửi một ảnh mẫu (pháp sư đứng bên phải phóng một luồng điện sang bầy xương bên trái) và xin ba điều:
dựng lại tia **bằng Blender MCP** giống ảnh; tia có **lõi trắng mảnh, bao quanh là ánh sáng xanh**; và nhân vật
**đưa cả hai tay** ra phóng như trong ảnh. Tôi hỏi lại ba chỗ chưa chắc, anh chốt: tia **hiện ~0,6 giây**
(trước 0,3) nhưng luật chơi giữ nguyên; động tác **dựng bằng code** như mọi tư thế khác; tia mới áp cho
**cả Giựt sét của quái** (Quỷ cây giữ màu xanh lá).

**Hình tia — dựng trong Blender qua MCP** (`CongCu/Blender/giat_set.blend`, cảnh riêng `GiatSet`, không đụng cảnh
của anh). Sinh đường sét bằng cách chia đôi đoạn thẳng rồi đẩy điểm giữa lệch sang bên, biên độ giảm dần qua
9 tầng — giảm **chậm** (×0,66 mỗi tầng) để khúc nhỏ vẫn gãy sắc như ảnh mẫu; lần đầu giảm ×0,56 ra một đường
uốn lượn mềm, không giống sét. Thêm 4 sợi phụ tách ra rồi nhập lại thân chính, 26 nhánh rẽ (có nhánh con,
nhánh cháu). Render **hai lớp riêng**: *lõi* (ống mảnh, phát sáng trắng) và *quầng* (cùng hình nhưng ống dày
hơn, qua compositor làm mờ hai cỡ 8 px + 42 px rồi cộng lại). Hai ảnh đều **xám** nên Unity tô màu lúc chạy được
— nhờ vậy Quỷ cây dùng chung mà vẫn xanh lá. Mỗi ảnh 1024 × 2048: nửa dưới là **4 dải thân tia** lặp liền mạch
theo chiều dài (thân chính vào và ra đúng giữa mép, các nhánh chép sang hai bên ±4 m), nửa trên là **4 cụm điện
bùng** 2 × 2 cho hai đầu tia. Nằm ở `Resources/KyNang/GiatSet/GiatSetLoi.png`, `GiatSetQuang.png`.

**Trong Unity** (`LightningArc.anhBlender`): đường đi vẫn gãy khúc bằng code nhưng chỉ khúc lớn (khúc nhỏ đã có
trong ảnh), dải ảnh trải dọc theo đường, quay mặt về máy quay; ảnh **lặp lại** chứ không kéo giãn (một chu kỳ =
4 lần bề ngang), nên tia 3 m hay 20 m đều giữ đúng tỉ lệ. Cứ 0,045 giây đổi dải khác + trượt ngẫu nhiên + lật dọc
→ 4 dải ra hàng chục hình, tia "sống" như ảnh mẫu. Ở tay và ở chỗ trúng có cụm điện bùng. Tia **sáng nguyên 65%
đời rồi mới tắt** (tia duy trì). Dưới cùng còn một lớp hào quang xanh rộng — lần chụp đầu chỉ có lõi và quầng
trong ảnh, **nhìn ra toàn trắng**, mất hẳn cái màu xanh đậm bao quanh của ảnh mẫu.

Tia **bám theo**: đầu tia bám điểm giữa hai bàn tay, cuối tia bám thân kẻ địch (quái chạy 1,5 m thì cuối tia đi
đúng 1,50 m). Tia hiện 0,6 s mà đứng yên thì chỉ cần quái bước một bước là tia treo lơ lửng giữa không trung.

**Động tác hai tay** (`NguoiChoiHoatHinh.TuTheGiatSet`): lúc niệm **gom hai tay về trước ngực**, tới lúc phép bay
ra thì **đẩy thẳng cả hai tay** về phía trước, hai bàn tay chụm vào nhau (cách 0,2 m), tay phải hơi cao hơn như ảnh
mẫu, người hơi chúi theo — và **giữ** suốt lúc tia còn hiện, tay rung nhẹ theo dòng điện. Khác các tư thế cũ
(xoay thêm một góc Euler vào khớp), ở đây tôi **ngắm hướng xương**: xoay cánh tay sao cho đoạn vai → khuỷu chỉ
đúng hướng muốn trong thế giới. Trục riêng của khớp model Meshy không rõ ràng, đoán góc Euler để "chỉ thẳng ra
trước" rất dễ lệch; ngắm hướng thì đúng bất kể trục khớp. Đang giữ tay mà bước đi thì hạ tay xuống, không trượt
như tượng. Tia mọc ra **giữa hai tay** (trước kia là đầu gậy tay phải, lệch 0,30 m).

**Một lỗi thật lộ ra trong lúc đo.** Vật liệu của tia được cất trong biến static kèm cờ "đã nạp". Vật liệu tạo
lúc Play **bị Unity xoá khi thoát Play**, nhưng biến static và cờ vẫn còn (các phép thử tắt nạp lại mã) — lần Play
sau cờ bảo "đã nạp", vật liệu thì đã chết, tia **âm thầm quay về kiểu cũ**. Sửa bằng cách kiểm null của Unity như
`VfxFactory` vẫn làm.

**Số đo** (menu 69, mục G mới — **0 lỗi**; các mục A–F cũ vẫn đạt, menu 37 và 61 cũng 0 lỗi):

| Đo | Kết quả | Đối chứng |
|---|---|---|
| G1 ảnh của tia | `GiatSetLoi` / `GiatSetQuang`, sống **0,60 s**, ở giây 0,32 còn sáng **1,00** | tia mặc định: ảnh `BoltStrip`, sống 0,26 |
| G2 hai tay | cánh tay chỉ ra trước **0,99 / 0,98** (tích vô hướng), giữa hai tay trước thân 0,63 m, cách nhau 0,22 m | lúc đứng yên: 0,16 / 0,04 |
| G3 chỗ mọc tia | cách điểm giữa hai tay **0,000 m** | đầu gậy cũ cách 0,30 m |
| G4 bám kẻ địch | bia chạy 1,5 m → cuối tia đi **1,50 m** | tia không bám: 0,00 m |
| G5 bước đi khi giữ tay | tay hạ ngay (1,00 → 0,00) | — |
| G6 Giựt sét của quái | dùng ảnh mới, quầng giữ **xanh lá** | — |

Ảnh: `PlayTestShots/giatset_3_hai_tay.png` (máy quay game), `giatset_4_can_canh.png` (cận tia),
`giatset_5_tu_the.png` (cận tư thế).

**Cùng ngày, anh chơi thử: "tia còn quá sáng và quá dày"** — trong khi ảnh cận tôi gửi thì anh ưng. Hai chỗ khác
nhau: góc máy mặc định khi chơi là **"3D tự do"** (đứng sau lưng, nhìn dọc theo hướng tia) chứ không phải góc
chéo tôi đặt để chụp; và lúc chơi thật trời đang **ban ngày**, còn phép thử luôn nhảy thẳng sang đêm. Nhìn dọc
theo tia thì ba bốn tia cùng mọc từ một điểm giữa hai tay **chồng kín lên nhau** ngay trước mặt máy quay thành một
mảng trắng. Menu 69 nay có **mục H** chụp đúng góc chơi thật, cả ngày lẫn đêm, và đếm điểm ảnh chói trắng (cả ba
kênh > 0,94) trong vùng tia. Sửa: bề ngang dải ảnh **1,35 → 0,70 m**, độ sáng lõi 1,8 → 1,2, quầng 2,8 → 1,5, hào
quang rộng 0,50 → 0,35 và đặc 0,85 → 0,45, cụm bùng ở tay 0,95 → 0,45 m, ở chỗ trúng 2,0 → 1,2 m, và đầu tia
vuốt nhọn dài hơn (5% → 12% chiều dài) để các tia tách nhau ngay khỏi tay.

| Góc chơi thật | Chói trắng trước | Chói trắng sau |
|---|---|---|
| Ban ngày | 16,11% | **5,63%** |
| Ban đêm | 11,80% | **3,70%** |

Ảnh: `PlayTestShots/giatset_6_goc_choi_ngay.png`, `giatset_6_goc_choi_dem.png`.

**Lần ba: "tia trắng mảnh thanh hơn 50%, tăng ánh sáng xanh bọc ngoài".** Trước khi sửa, tôi đo xem cái "trắng"
anh thấy thực ra là gì. Menu 69 thêm phép **cắt ngang tia** trên ảnh chụp: quét một đoạn vuông góc với tia, đếm
bao nhiêu điểm ảnh trắng (cả ba kênh > 0,85) và bao nhiêu điểm ảnh xanh. Ở ảnh cận, lõi trắng chỉ rộng **0,6 điểm
ảnh**, tức lõi trong ảnh vốn đã mảnh. Cái trắng dày ở góc chơi là **tâm quầng xanh cháy sáng**: đo trên ảnh
quầng, dải > 0,9 rộng 21,8/256 điểm ảnh; tô màu (0,30 0,52 1) × 2 lần độ sáng rồi cộng hào quang, cộng hậu kỳ phát sáng
thì kênh đỏ và lục cũng vượt ngưỡng, thành trắng.

Lần đo đầu cũng bị nhiễu: bia thử màu trắng dưới nắng và tia bên cạnh lọt vào lát cắt. Nay bia sơn tối, mục H chỉ bắn một tia.

Sửa ở hai nơi. **Blender MCP** vẽ lại cả hai ảnh, cùng hạt giống nên hình tia y hệt: lõi dùng ống **nhỏ một nửa**
(bề rộng > 0,5 từ 5,5 còn 3,4 điểm ảnh; thân 0,0055 → 0,00275), quầng dùng ống mảnh hơn và phát sáng yếu hơn ở tâm
(dải cháy sáng 21,8 → 6,1), còn phần loang rộng thì làm mờ rộng hơn (42 → 70 điểm ảnh) và cộng mạnh gấp 1,8 lần
(dải > 0,2 từ 81,7 → 88,0). **Unity**: quầng sáng 1,5 → 2,2, hào quang rộng 0,35 → 0,55 và đặc 0,45 → 0,60, và quan
trọng nhất là **màu quầng xanh đậm (0,14 0,34 1)**. Với màu (0,25 0,45 1), quầng dày lên thì tâm lại ngả trắng: phần
trắng ban đêm **tăng** 7,0 → 9,6. Cuối cùng hạ lõi 1,3 → 1,0.

| Góc chơi thật, một tia | Phần trắng trước | sau | Phần xanh trước | sau |
|---|---|---|---|---|
| Ban đêm | 7,0 điểm ảnh | **1,6** | 24,0 | **47,6** |
| Ban ngày | 13,8 | **9,6** | 14,4 | **29,8** |

Ban ngày, phần trắng giảm ít hơn vì mặt đất sáng cộng với quầng xanh cũng vượt ngưỡng "trắng" của phép đo,
nhưng tỉ lệ điểm ảnh chói trong cả vùng tia vẫn giảm 5,63% → 0,80%.

**Lần bốn: "tia trắng đã ổn, cho viền xanh sáng và dày hơn 10%".** Trước đó lõi và quầng dùng **chung một dải
hình**, nới viền là nới cả lõi. Nay tách hai dải (`LightningArc.DungLuoiAnh`): dải quầng rộng hơn
**`HeSoVienXanh` = 1,10** lần. Hai dải dùng chung đường đi, dải ảnh, độ trượt, nhánh và cụm bùng (gieo ngẫu nhiên một
lần). Chu kỳ lặp ảnh tính theo bề ngang gốc, nên hai lớp trùng nhau theo chiều dọc tia, chỉ viền phình ra hai
bên. Quầng sáng 2,2 → 2,42, hào quang rộng 0,55 → 0,605 và đặc 0,60 → 0,66, cả ba đều ×1,10.

Thay đổi 10% nhỏ hơn độ dao động của một ảnh chụp, vì tia đổi hình ngẫu nhiên mỗi 0,045 s. Nên mục H nay **tung
5 lần** mỗi điều kiện rồi lấy trung bình, và có thêm "tổng độ sáng xanh" (cộng kênh xanh lam của các điểm ảnh
xanh trên lát cắt):

| Góc chơi thật (TB 5 lần) | Viền xanh rộng | Tổng độ sáng xanh | Lõi trắng rộng |
|---|---|---|---|
| Ban ngày | 31,5 → **35,8** (+14%) | 28,8 → **32,1** (+11%) | 7,8 → 8,9 |
| Ban đêm | 45,6 → **55,0** (+21%) | 38,1 → **45,1** (+18%) | 3,7 → 4,8 |

Bề rộng đo được tăng hơn 10% vì viền sáng hơn nên có thêm điểm ảnh vượt ngưỡng "xanh". Lõi trắng rộng thêm khoảng
1 điểm ảnh, vì quầng sáng hơn cộng vào tâm.

**Quả cầu điện dùng chung kiểu tia (25/09/2026).** Anh xin các tia điện của Quả cầu điện "cũng có hiệu ứng như tia
sét của Giựt sét". Phần dựng kiểu tia (dải ảnh Blender, cụm bùng hai đầu, bám hai đầu, gãy khúc theo độ dài) tách
ra thành `GiatSet.KieuTia`, và `VfxFactory.TiaCauDien` gọi nó. Màu viền là `GiatSet.MauQuangNguoiChoi`, bề ngang
bằng tia đầu của Giựt sét (×1,15). Đầu tia bám quả cầu, cuối tia bám kẻ địch. Từ nay **sửa kiểu tia ở một chỗ là cả
hai kỹ năng cùng đổi**. Thời gian sống giữ **0,22 s** như cũ (`GiaySongTiaCauDien`): cầu bắn mỗi 0,4 s, sống 0,6 s như
Giựt sét thì lượt sau chồng lên lượt trước.

Menu 74 thêm mục K, đo ở lượt thứ ba: **5/5 tia** quanh cầu dùng ảnh `GiatSetLoi`, đúng màu viền của Giựt sét, đúng bề
ngang (đối chứng: tia mặc định không dùng ảnh). Menu 74 cả bài **0 lỗi**. Ảnh `quacaudien_2_ban_tia.png`: trước đây năm
tia là một mảng trắng loá, nay thấy rõ từng tia lõi trắng viền xanh.

### Kỹ năng mới MÂY GIÔNG — nhóm Phong (25/09/2026)

Anh gửi ảnh một đám mây trắng sáng từ bên trong với nhiều tia sét trắng xanh rẽ nhánh, xin kỹ năng "Mây giông": vùng mây ngay
chỗ chọn, tầm bằng Sấm sét, 5 giây 20 tia, mỗi tia 125 ở cấp 1, 45% hất ngã 0,85 giây, kẻ trúng cháy đen toàn thân, hồi chiêu 7
giây, mọi hình dựng bằng Blender. Tôi hỏi thêm bốn điều, anh chọn: **50 năng lượng, niệm 0,5 giây**; **vùng 6 m, ưu tiên kẻ
địch** như Sấm sét; **không cần điều kiện mở khoá**; **cháy đen chỉ là hình, 3 giây**.

**Hình (Blender MCP, `CongCu/Blender/may_giong.blend`).** Bốn đám mây bồng (lưới 2×2) — bản đầu tròn nhoè như quả bóng, đổi
sang "nhiễu trừ khoảng cách rồi cắt mềm" thì mép mây lồi lõm từng cụm, lõi sáng trắng xanh; ảnh loé chạm đất hình sao; lớp
than đen liền mạch (Voronoi 4D trên hình xuyến) với vết nứt âm ỉ màu lửa — bản đầu nứt dày và cam như gạch lát, chỉnh cho gần
đen tuyền; icon mây + tia sét trên nền đen. Tia sét dùng lại ảnh Blender của Giựt sét, 4–6 nhánh. Trong game: hai tầng mây
(sáng trên, xám dưới), loé sáng và đèn nhấp nháy trong mây, tia ngang lách tách trong lòng mây, khói đen bốc từ người cháy.

**Cháy đen** phủ thêm một lớp vật liệu lên MỌI renderer kể cả `SkinnedMeshRenderer` (bài học vỏ băng), và lúc gỡ chỉ bỏ đúng
lớp của mình — không trả nguyên mảng cũ như vỏ băng — nên chồng với đóng băng vẫn giữ vỏ băng.

**Độ cao mây: 7 m chứ không 10 m** (con số tôi tự chọn): ảnh ở góc chơi thật cho thấy máy quay "3D tự do" chỉ nhìn tới
~5,5 m ở chỗ cách 10–12 m, mây 10 m nằm hẳn trên mép màn hình.

**Số đo** (menu 83 mới — **0 lỗi**):

| Đo | Kết quả |
|---|---|
| Thông số | số hiệu 21, 22 kỹ năng; nhóm PHONG; 50 / 7 s / 0,5 s; tầm 12 m = Sấm sét; vùng 6 m; mở khoá không điều kiện |
| Tung thật | tốn 50, hồi chiêu 7,00 s; mây hiện sau 0,33 s đúng chỗ ngắm (lệch 0,00 m); bấm lại lúc hồi chiêu → vẫn 1 đám mây |
| Nhịp | 20 tia, tia đầu 0,36 s, cuối 5,11 s, cách nhau TB 0,250 s; 20/20 tia ảnh Blender ≥ 4 nhánh |
| Sát thương | 81/81 lần bia mất máu là bội số 125; nhắm kẻ địch 65–75%; **đối chứng** vùng trống: 20 tia, 0 nhắm |
| Hất ngã | 269–278 lần trúng → 41,6–45,0% (hai lần chạy); dài 0,85 s |
| Cháy đen | bộ xương (SkinnedMesh) phủ 1/1, đậm 1,00, tự gỡ sau 3,03 s, trả đúng; chồng vỏ băng → gỡ xong vỏ băng còn |
| Qua mạng · hình | phát lại được; mây 26 + 16 đám nằm ngang (lệch dọc 0,00 m), rộng 5,7 m; có đèn |

Chạy lại các phép thử có đếm kỹ năng / icon: 60, 66 (bảng biểu tượng đủ 22 ô), 77, 80, 61 — đều **0 lỗi**. Menu 61 thêm Mây
giông (giết quái, ghi đúng kẻ đánh, đủ kinh nghiệm). Mục B4 của menu 61 (châm cây) từng báo sai: loạt Thiên thạch ở B3 nay
châm cả bia mộ / nhà mồ (tính năng sáng nay), đầy trần "4 vật cháy cùng lúc" nên cây không bắt lửa — phép thử nay chờ lửa tàn.

Ảnh: `PlayTestShots/maygiong_1_dem_can.png`, `maygiong_2_chay_den.png`, `maygiong_3_ngay_goc_choi.png`.

### Tàng hình 90 giây, vòng phép nổ lúc hiện hình (26/09/2026)

Anh gửi ảnh một vòng phép xanh dưới chân (hai vòng tròn, sao năm cánh, ký hiệu, đuôi mũi tên) và xin: tàng hình **90 giây**; **lúc kết
thúc** phát vòng sáng ấy (dựng bằng Blender MCP) và gây sát thương **một lần** cho kẻ địch: **100 + 10% máu** ở cấp 1, **mỗi cấp +4%**.
Anh chọn: nổ khi kết thúc **bằng cả hai cách** (hết giờ, hoặc tan do đòn đầu); **máu tối đa**; vòng **5 m**; hồi chiêu **đang tàng hình
không bấm lại được, hiện hình xong đếm 10 giây** (trước 30 giây từ lúc bấm).

**Ảnh (Blender MCP, `tang_hinh_vong_phep.blend`).** 54 đường cong phát sáng (vòng đôi, sao, vòng nhỏ ở đỉnh, trăng khuyết, ký hiệu chạc
ba, đuôi mũi tên) + đĩa sương mờ xanh; compositor **Fog Glow** (Blender 5.2: `scene.compositing_node_group`, Render Layers → Glare →
Group Output). Nền **đen** để dùng cộng sáng. Quầng sáng loang tới tận mép ảnh (3–17/255) nên trong game hiện một **ô vuông mờ** quanh
vòng — trừ mức nền và cho mờ dần về 0 ngoài bán kính vòng (xử lý ngay trong Blender).

**Vòng phép phải BÁM ĐẤT.** Bản đầu là một tấm phẳng ở độ cao chân nhân vật; ảnh ở góc chơi thật cho thấy chỗ đất Act2 cao hơn **che
mất cả nửa vòng** (ảnh chụp sau nhiều mục thử còn trông như "mảng đen"). Nay là lưới 24 × 24 ô, mỗi đỉnh bắn tia **chỉ lớp Ground**
rồi nâng 6 cm; xoay và phóng to làm trên toạ độ ảnh (đổi UV mỗi khung) để lưới đứng yên trên đất.

**Sát thương** là loại "vật lý" nhưng ghi hệ **BĂNG** cho Kháng Băng (Tàng hình thuộc nhóm Băng): quái có kháng băng ±25%, dùng loại
băng thì con số "100 + 10%" lệch theo từng con.

**Hai lỗi thật phép thử bắt được:**
1. **Người khác hiện hình rồi mà trên máy mình vẫn tàng hình thêm cả chục giây.** Bản sao nhận bit "đang tàng hình" thì tạo trạng thái
   mới — trường thời gian còn lại khai báo sẵn bằng `ThoiGian` — rồi lấy `Max` với 0,35 s: ra **20 s** (nay sẽ là 90 s). Cùng loại bẫy
   "AddComponent mang giá trị mặc định" đã ghi. Sửa: trạng thái mới đặt đúng 0,35 s.
2. **Vòng phép thứ hai:** ngay sau khi vòng nổ trên bản sao, gói tin trễ vẫn còn bit "đang tàng hình" → tạo lại tàng hình → hết giờ →
   nổ thêm một vòng. Nay bỏ qua bit ấy 1,5 s sau khi nổ.

| Đo (menu 73, 0 lỗi) | Kết quả |
|---|---|
| Thời gian · hồi chiêu | tàng hình 90 s; đang tàng hình bấm lại → từ chối, ô giữ **10,00 s**; hiện hình → **9,93 s** rồi 8,93 s sau 1 giây |
| Cấp 1 | bia tối đa 600 / 1000 / 300 (máu hiện tại còn một nửa) mất **160 / 200 / 130** = 100 + 10% máu TỐI ĐA |
| Cấp 5 (tung thật) | **256 / 360 / 178** = 100 + 26% |
| Ngoài vùng · chính mình | bia 5,6 m mất 0; người tung mất 0 |
| Tan do đòn đầu (Quả cầu lửa) | nổ thêm đúng 1 vòng |
| Hình | vòng ngoài vẽ thật bán kính **5,00 m**; 625 đỉnh lưới lệch khỏi (mặt đất thật + 6 cm) **0,000 m** |
| Bản sao | hết tàng hình → nổ 1 vòng trên máy này; bit cũ trễ → không tạo lại, không vòng thứ hai |

Ảnh: `PlayTestShots/tanghinh_6_vong_phep_goc_choi.png` (góc chơi, cảnh sạch).

### Mây giông: hồi chiêu 5,5 s, bỏ cột khói, mây to hơn, mưa dập lò, mây tự bay 4 giây (26/09/2026)

Anh chơi thử rồi xin: hồi chiêu **5,5 giây**; **bỏ cột mây và quầng mây dưới đất**; đám mây **to hơn 15%**; **mưa rơi trúng lò lửa
thì tắt lửa**; và **hết 5 giây mây không tan mà tự bay theo hướng ngẫu nhiên 4 giây nữa, vừa bay vừa mưa vừa giựt sét**. Anh chọn:
to 15% **chỉ hình** (vùng mưa / ướt / sét giữ 6 m), bay **1,5 m/s** (~6 m), sét khi bay **như cũ 4 tia/giây**, lò **cháy lại sau 30 giây**
như Gió lốc.

**Hướng bay "ngẫu nhiên" mà mọi máy phải thấy như nhau.** Mây được phát lại trên từng máy; mỗi máy tự `Random` thì mỗi người thấy mây
bay một ngả và sét đánh một nơi. Em băm hướng từ **toạ độ ngắm đã nén đúng như gói tin** (`GoiTin.NenToaDo`, đơn vị 0,01 m): máy tung
dùng điểm thật, máy nhận dùng điểm đã nén, cả hai ra cùng một số nguyên — không thêm byte nào. Đo: 40 chỗ ngắm ngẫu nhiên, **40/40
khớp** giữa điểm thật và điểm qua gói tin; hướng chia bốn góc 14/7/12/7.

**Mây bay được** thì các lớp mây phải mô phỏng **Local** (World thì đám mây ở lại chỗ cũ, chỉ mưa đi theo) và luôn mô phỏng
(Local + ngoài khung thì Unity dừng mô phỏng). Mây bám đất bằng tia **chỉ lớp Ground** (bài học Gió lốc trèo mái nhà).

**Bẫy: hồi chiêu vẫn 7 giây.** Code đổi hằng 7 → 5,5 mà Play vẫn ra 7, dù prefab và scene trên đĩa **không có** trường này: bản prefab
Unity giữ trong bộ nhớ đã nạp lúc trường còn mặc định 7 và giữ nguyên qua mọi lần biên dịch — bản build WebGL cũng lấy từ đó.
Ba thông số của Mây giông nay là **thuộc tính đọc thẳng hằng số**, như bình máu.

**Phép thử bắt thêm:** mục K xoá mây sớm nhưng phần hình (mây, mưa) là vật riêng nên vẫn mưa tiếp ở chỗ cũ tới hết giờ — nay mây bị
xoá giữa chừng thì xoá luôn phần hình.

| Đo (menu 83, 0 lỗi) | Kết quả |
|---|---|
| Hồi chiêu | **5,50 s** (Sách phép 5,5) |
| Tia | **36** (20 đứng yên + 16 khi bay), cách nhau TB 0,250–0,253 s, tia cuối 9,12–9,22 s (hai lần chạy) |
| Bay | 16 tia khi bay, **0** tia rơi ngoài vùng 6 m quanh chỗ mây ĐANG Ở; đi **5,79–5,97 m**, lệch hướng 0,0°; lệch mặt đất 0,000 m |
| Mây to | cỡ đám + bán kính rải **×1,150** cả hai tầng; vùng mưa vẫn 6 m; mô phỏng Local |
| Bỏ cột / quầng | còn **0** lớp (CotMay, KhoiCot, SuongDat, MayThap) |
| Mưa dập lò | lò cách mây 3 m tắt sau **0,36 s** (mưa bắt đầu 0,35 s), hẹn cháy lại **30,0 s**; lò đối chứng cách 22 m vẫn cháy |
| Các mục cũ | ướt / +50% (×1,500 cả bốn) / cháy xém 36 vết / cháy đen cả khi ướt — đều đạt |

### Mây giông: mưa làm "Bị ướt", sét +50% lên kẻ ướt, cháy xém mặt đất; cột khói chạm đất THẬT (25/09/2026, khuya)

Anh xin bốn điều: cột mây **rộng thêm 20%**; **mưa rơi liên tục** từ mây xuống đất; mưa không gây sát thương nhưng **100% làm
"Bị ướt" 5 giây** mọi đối thủ, kẻ ướt chịu **+50% sát thương từ nhóm sét điện**; tia chạm đất **cháy xém và bốc khói như Sấm sét**.
Anh chọn: ướt khi **đứng trong cả vùng 6 m** (còn đứng là còn làm mới, ra khỏi thì ướt thêm 5 giây); +50% cho **Giựt sét, Sấm sét,
Quả cầu điện và chính tia Mây giông**; hình ướt là **nước nhỏ giọt + thân bóng ướt + chữ**.

**Ảnh Blender** (`CongCu/Blender/may_giong_mua.blend` — lần này Blender mở cảnh trống, em dựng mặt phẳng + máy quay mới và lưu file
riêng để không đè `may_giong.blend`): vệt giọt mưa mảnh sáng đầu dưới, vòng nước bắn hai vòng đứt quãng, giọt nước hình quả lê
(bản đầu ra hình "ngôi nhà", vẽ lại), lớp bóng ướt liền mạch (nền tối xanh + vệt nước chảy sáng).

**Mưa** là vệt dựng đứng rơi 16 m/s, 160 vệt/giây. Đất Act2 gồ ghề nên không đặt vòng nước ở một độ cao cố định: vệt mưa **va chạm
với lớp Ground** rồi chết và bật vòng nước đúng chỗ chạm (sub-emitter). Phép thử bắn tia xuống đất tại từng vòng nước (độc lập với
hệ va chạm): lệch trung bình **0,04 m**.

**Bị ướt** (`BiUot`) không cần gói tin: đám mây được phát lại trên mọi máy, nên mỗi máy tự thấy ai đứng trong mưa; sát thương lên
người chơi do máy nạn nhân tính, lên quái do chủ phòng tính — cả hai đọc trạng thái ướt của chính mình.

**Hai lỗi thật phép thử bắt được:**

1. **Cột khói chưa bao giờ chạm đất.** Hạt dựng đứng / nằm phẳng vẽ ra chỉ **0,707×** kích thước đặt (đo bằng BakeMesh: đặt 2 × 8 → vẽ
   1,414 × 5,657 — CLAUDE.md đã ghi cho lò lửa mà em quên), và Unity còn **ép hạt to không quá nửa màn hình** (`maxParticleSize` 0,5):
   ở góc đo, cột 8,5 m vẽ ra chỉ còn **3,77 m, lơ lửng từ 2,04 m**. Phép thử cũ đọc cỡ ĐẶT nên vẫn báo "chạm đất". Sửa: bù chiều cao
   /0,7071, `maxParticleSize` 10 cho mọi lớp mây, và đo bằng **hình vẽ thật** (BakeMesh trong Play) → cột **−0,35 → 8,20 m**.
   Quầng mây sát đất đo lại theo hình vẽ thật: lan **3,18 m** (con số 3,45 m em báo lần trước là tính theo cỡ đặt, sai).
2. **Kẻ bị ướt không bao giờ cháy đen.** Mưa làm ướt trước tia đầu; lớp bóng ướt là vật liệu trong suốt; lớp cháy đen bỏ qua
   "renderer có vật liệu trong suốt" (vòm khiên, hạt) — xét cả mảng nên thấy lớp ướt và bỏ luôn cả thân. Nay chỉ xét **vật liệu gốc**.

| Đo (menu 83, 0 lỗi) | Kết quả |
|---|---|
| Ướt sau 0,9 s mưa | trong vùng 2 / 4 / 5,5 m, bia rồi đi ra, quái: **đều ướt**; ngoài vùng 7,5 m: không; người tung: không; chữ "BỊ ƯỚT" đúng 5 lần |
| Hết ướt | sau khi mây tan **4,85 s**, sau khi ra khỏi vùng **4,93 s** (lần làm mới cuối trễ tới 0,2 s) |
| +50% | Giựt sét 112,50 / 75 · Sấm sét 39 / 26 · Quả cầu điện 233,25 / 155,50 — đều **×1,500**; tia Mây giông lên cụm bia ướt: mọi lần là bội số 187,5 |
| Đối chứng | Giựt sét của **quái** 40 / 40 và sát thương vùng thường 50 / 50 — **×1,000** |
| Cháy xém | 20 tia → **20** vết `SetChayDen`; một tia Sấm sét thật → 1 (cùng hàm) |
| Mưa | 79 vệt đang rơi, trải tới 5,99 m; 37 vòng nước, lệch mặt đất thật 0,04 m |
| Hình ướt | quái có xương: lớp bóng ướt 1/1, đậm 1,00, có nước nhỏ; có lúc mang **cả hai lớp** ướt + cháy đen; hết cả hai → vật liệu về đúng |
| Cột khói | vẽ thật **−0,35 → 8,20 m**, rộng TB **4,36 m** (trước vẽ 3,54 → ×1,23; cỡ đặt ×1,200) |

Chạy lại menu 58 (Sấm sét), 69 (Giựt sét), 74 (Quả cầu điện): **0 lỗi**. Menu 69 lần đầu báo 1 lỗi ở mục tư thế tay (lúc đo tay
chưa kịp đẩy ra, 0,00 thay vì 1,00 — không liên quan), chạy lại thì đạt.

Ảnh: `PlayTestShots/maygiong_5_mua.png` (mưa, số 188 trên kẻ ướt, đất cháy xém), `maygiong_3_ngay_goc_choi.png` (cột đen chạm đất).

### Mây giông đen thật, sét rọi sáng từng mảng; quầng sát đất to hơn (25/09/2026, khuya)

Anh chơi thử trên điện thoại rồi xin: quầng mây sát đất **to thêm 15%, dày thêm 10%**, và **toàn bộ mây tối đen hơn cho giống
mây giông thật**. Anh chọn "dày" là **cả đặc hơn lẫn cao hơn**, và độ tối "**xám đen, sét rọi sáng**".

**Màu.** Mọi lớp mây (mây trên cao hai tầng, cột khói, khói cuộn, quầng sát đất, cụm mây thấp) nhân màu cũ với **0,26**
(`HeSoToiMay`); shader hạt là ảnh × màu, nên độ sáng hiện ra giảm đúng chừng ấy. Tầng xám dưới đáy mây vốn tối hơn nên vẫn tối hơn đỉnh.

**Sét rọi sáng.** Bản đầu em cho cả đám mây sáng bừng ×2,8 mỗi tia và mỗi tia ngang, tắt trong 0,22 s — nhưng 4 tia/giây cộng tia
ngang thì mây sáng gần như liên tục, mất cái tối vừa làm. Đổi lại cho đúng ý "rọi sáng **từng mảng**": mỗi tia bật một **mảng sáng**
(2 đám mây Blender cộng sáng, 3,5–5,5 m, 0,18–0,28 s) ngay chỗ tia phát ra; cả đám chỉ loé nhẹ ×1,8 và tắt trong 0,15 s. Loé ngẫu
nhiên trong mây trước dùng ảnh ngôi sao (hợp với mây trắng) — trên mây đen nó thành những ngôi sao lấp lánh, nên đổi sang ảnh
đám mây cộng sáng màu xanh.

**Quầng sát đất.** Cỡ đám và vùng rải ×1,15, độ đặc ×1,10, cụm mây thấp to và cao ×1,10.

| Đo (menu 83, 0 lỗi) | Kết quả |
|---|---|
| Độ sáng 6 lớp mây so với màu cũ | cả 6 lớp ×0,26; đáy mây 0,138 < đỉnh 0,254 |
| Sét rọi sáng | loé cả đám cao nhất ×1,77; **48%** khung hình mây ở đúng màu tối gốc; 8 tia → 8 mảng sáng |
| Quầng sát đất so với bản trước | cỡ ×1,150, rải ×1,150, cụm thấp rải ×1,150 · đặc ×1,100 · cụm thấp cỡ/cao ×1,100 |
| Quầng lan xa | **3,45 m** (bản trước 3,13) |
| Các mục cũ | 20 tia · 0,249 s/tia · bội số 125 · hất ngã 47,3% · cột chạm đất — không đổi |

⚠️ Ban đêm mây xám đen lẫn nhiều vào nền tối (`maygiong_4_dem_goc_choi.png`): thấy chủ yếu nhờ tia và mảng sáng. Ban ngày
cột mây đen nổi rõ (`maygiong_3_ngay_goc_choi.png`).

### Mây giông: tia xanh sẫm hơn, quầng mây mỏng sát đất (25/09/2026, tối)

Anh gửi lại ảnh mẫu, xin **tia xanh dương đậm hơn** và **một quầng mây mỏng trên mặt đất** như dưới chân cột trong ảnh. Anh
chọn "xanh đậm hơn, vẫn lõi trắng" (chỉ Mây giông, Giựt sét giữ nguyên) và "quanh chân cột, ~3 m".

**Tia.** Quầng xanh từ (0,14 0,34 1) của Giựt sét xuống **(0,05 0,16 1)** — hạ kênh đỏ/lục chứ không tăng độ sáng, vì bài
học cũ: quầng sáng lên mà đỏ/lục còn cao thì tâm tia ngả trắng. Viền xanh và hào quang **dày ×1,2** để màu thấy rõ. Trước đây
viền xanh là một hằng số dùng chung mọi tia, nên em thêm trường `heSoVien` cho từng tia (mặc định vẫn 1,10) — Giựt sét và Quả
cầu điện không bị kéo theo.

**Quầng mây sát đất (Blender, vật liệu `MG_SuongDat`).** Bốn đám sương nhìn từ trên xuống: tròn mềm dần ra mép, mép lồi lõm,
bên trong là sợi xoáy và lỗ thủng (nhiễu 4D có vặn méo). Trong game 5 đám **nằm phẳng** (HorizontalBillboard) ở 0,3 m trên đất
— thấp hơn thì chỗ đất gồ ghề của Act2 xuyên qua đám sương — xoay chậm, cộng 6 cụm mây thấp đứng để lớp sương có độ dày khi nhìn xiên.
Độ lan đo bằng cách đọc thẳng file PNG (sương thấy được tới 77% nửa ô) nhân với cỡ hạt thật: lần đầu 2,62 m, nới hai lần (cỡ
3,2–4,4 → 3,8–5,0 m, rải 1,1 → 1,6 m) được **3,13 m**.

| Đo (menu 83, 0 lỗi) | Kết quả |
|---|---|
| Quầng Giựt sét thật | (0,14 0,34 1), viền 1,100, hào quang 0,605 — **không đổi** |
| Quầng Mây giông | (0,05 0,16 1), viền 1,320, hào quang 0,726 → **×1,20 / ×1,20** so với tia Giựt sét thật |
| Hình dạng tia | vẫn y hệt Giựt sét (bề ngang, lõi, thời gian sống, nhánh, cụm bùng) |
| Quầng mây sát đất | 5 đám nằm phẳng, cao 0,30 m, lan 3,13 m; 6 cụm mây thấp |
| Các mục cũ | 20 tia · 0,250 s · mọi lần trúng bội số 125 · hất ngã 48,3% · cột khói chạm đất (−0,12 m) — không đổi |

### Mây giông: cột khói xuống tận mặt đất, tia sét y như Giựt sét (25/09/2026, chiều)

Anh gửi thêm một ảnh: đám mây có **cột khói rủ thẳng xuống tận mặt đất**, tia sét đan quanh cột; và xin **tia giống hệt tia
Giựt sét**. Anh chọn "một cột khói giữa vùng" và "đúng y Giựt sét".

**Cột khói (Blender MCP, `may_giong.blend`, vật liệu `MG_Cot`).** Bốn cột trong một ảnh 2×2 (`CotMay.png`, mỗi ô 512×1024):
đường tâm uốn chữ S, thân rộng dần lên chỗ nhập vào mây, chân loe nhẹ; mép lồi lõm theo nhiễu 4D, bên trong có sợi dọc và mảng
sáng tối. Bản đầu mép thẳng như cái đồng hồ cát; bản hai bồng bềnh nhưng vào game **gần như không thấy** (ở góc chơi chỉ là
một vệt xám nhạt sau lò lửa) — bản ba đặc hơn, thân rộng hơn, màu sáng hơn thì hiện rõ cả ngày lẫn đêm. Trong game: 3 lớp hạt
**dựng đứng** (VerticalBillboard — luôn quay về máy quay nhưng không nghiêng), rộng 5 m, từ 0,35 m dưới đất tới 1,2 m trong
mây, cộng khói cuộn nhỏ trôi dọc cột.

**Tia sét.** Bản trước dùng chung hàm vẽ tia của Giựt sét nhưng tự đặt thông số riêng: dày ×1,7, quầng xanh nhạt mặc định,
sống 0,30–0,42 s, 4–6 nhánh, thêm loé hình sao chạm đất. Nay gọi đúng như Giựt sét: bề ngang ×1, quầng xanh đậm của người
chơi, sống 0,6 s, 2 nhánh; bỏ loé hình sao (Giựt sét đã có cụm điện bùng ở đầu tia).

**Phép kiểm độc lập.** Không so tia Mây giông với các hằng số trong code (sai chung thì vẫn "khớp"). Menu 83 mục I **tung
Giựt sét thật** từ nhân vật rồi chụp thông số tia bay ra. Lần đầu nó bắt nhầm **tia loé phụ lúc niệm** ở tay (sống 0,08–0,11 s,
không có ảnh Blender) — nên phép thử nay ghi hết mọi kiểu tia bay ra và chỉ so với tia chính.

| Đo (menu 83, 0 lỗi) | Kết quả |
|---|---|
| Tia chính Giựt sét thật | ảnh Blender · bề ngang 0,700 · lõi 0,160 · quầng 0,420 · quầng (0,14 0,34 1) · 0,60 s · 2 nhánh · bùng 0,45/1,20 |
| Tia Mây giông | **y hệt từng số**; 20/20 tia cùng một kiểu |
| Cột khói | 3 lớp dựng đứng, rộng 5,3 m; hạt từ −0,35 m tới 8,20 m (đáy mây 6,5 m) |
| Chân khói thấy được | đọc thẳng file PNG: khói bắt đầu ở 2,7% chiều cao ô → **−0,12 m**, tức chạm đất |
| Khói cuộn dọc cột | 13 hạt |
| Các mục cũ | 20 tia · 0,250 s/tia · mọi lần trúng là bội số 125 · hất ngã 39,8–49,4% qua ba lần chạy (~260 lần trúng mỗi lần; độ lệch chuẩn ≈ 3,1% quanh 45%) · cháy đen 3,02 s — không đổi |

Ảnh: `PlayTestShots/maygiong_3_ngay_goc_choi.png`, `maygiong_4_dem_goc_choi.png` (mới — ban đêm ở góc chơi thật).

### Lốc xoáy dựng lại bằng Blender theo ảnh mẫu (25/09/2026)

Anh gửi hai ảnh cơn lốc xám trắng với nhiều tia sét trắng xanh và xin "dựng lại giống như trên hình 100%, lốc cuốn lên chỉ
quay theo trục một chiều". Tôi hỏi lại ba điều, anh chọn: **cao 15,4 m, dáng theo ảnh** (chân hẹp, loe dần lên miệng),
**bỏ mây giông đen và vệt khói đen** (ảnh chỉ có quầng sáng trên miệng và bụi xám nhạt dưới chân), **tia sét kiểu Giựt sét
nhiều nhánh**. Vùng hút và sát thương giữ nguyên. Mục "thân nở 20%" ngay dưới đây vì thế không còn áp dụng — dáng mới
theo ảnh.

**Dựng trong Blender (qua MCP, `CongCu/Blender/loc_xoay.blend`).** Bốn lớp vỏ phễu lồng nhau (bán kính ×0,70 / 0,84 / 1,00 /
1,13; vỏ chính r = 1,3 + 5,5·t^1,9 m, miệng 6,8 m) và một vành cuộn ở miệng. Ảnh gió làm bằng shader node: dải khói xoắn ốc,
mỗi vòng quanh thân lệch đúng một dải nên **ảnh khép vòng**, quay mãi không lộ đường nối; sợi mảnh chạy dọc dải; lõi xám đậm,
lớp ngoài mảnh và mờ. Tôi render thử cả cơn lốc bảy lần để chỉnh (bản đầu là hình nón thẳng, mép lởm chởm như bậc thang;
bản sau như phễu thuỷ tinh đục; vành miệng từng phẳng như cái đĩa). Xuất ảnh bằng cách render mặt phẳng UV qua máy quay trực
giao. Hai lỗi bắt được bằng số: bộ lọc điểm ảnh 1,5 px của Cycles trộn nền trong suốt vào cột mép (mép trái–phải lệch 0,04–0,05
so với 0,003 giữa ảnh); và nhiễu sợi gió lấy toạ độ theo dải nghiêng nên không khép vòng. Sửa cả hai: mép còn lệch 0,002–0,004.

**Một chiều mà vẫn cuốn lên.** Mọi lớp quay cùng chiều vật bị cuốn bay. Đo trên lưới đã nhập vào Unity: u tăng thì góc quanh
trục tăng; đo trên ảnh: đi lên thì dải lệch về phía u tăng — nên quay như thế thì dải **trôi xuống**. Lật u của ảnh (tiling −1)
thì dải cuốn lên mà chiều quay không đổi.

**Số đo** (menu 82 viết lại — **0 lỗi**):

| Đo | Kết quả |
|---|---|
| Cấu trúc | một con `LocXoayHinh`: 5 lớp (lưới từ FBX Blender 5/5), quầng sáng, bụi chân, đèn; 0 thứ cũ còn sót (mây, khói, dải xoắn, hạt cát) |
| Kích thước | cao 15,72 m (thân 15 + vành), miệng vỏ chính 6,80 m, chân 1,30 m; bề ngang miệng / cao 0,87 (ảnh mẫu ~0,9) |
| Một chiều | 6/6 mục cùng chiều bia bị cuốn (5 lớp +210…+100 °/s, bụi 100% hạt); **đối chứng** đảo một lớp → bắt đúng 1 lớp ngược |
| Cuốn lên | ảnh: tần số dải mạnh nhất N = 8, hết một vòng u pha dải đổi **+1,00 vòng** (đúng công thức); lưới: 2520/2520 cặp u tăng → góc tăng; tiling −1 → **cuốn lên**; **đối chứng** tiling +1 → trôi xuống |
| Tia sét | kiểu ảnh Blender, 3–5 nhánh (TB 4,0); dời lốc 2,00 m → đầu tia dời 2,00 m |
| Vùng hút | giữ 5,184; quỹ đạo kẻ bị cuốn = 0,9 × vỏ chính |

Bẫy khi đo độ nghiêng dải: bản đầu dùng tương quan hai hàng ảnh, ra "lên 3 px dịch 3 px" — sợi gió nằm ngang lấn át dải
nghiêng (công thức cho 48 px). Đúng dấu nhưng là may; đổi sang đo **pha** dải theo từng cột thì ra đúng +1,00 vòng.

Hai sửa phụ: `AssetBaker.SaveMeshes` bỏ qua lưới đã là asset (lưới FBX); lúc lốc tan, tắt **mọi** hệ hạt và đèn (trước chỉ
tắt những gì trong `visual`, mà lốc từ prefab thì `visual` rỗng — quầng sáng lơ lửng thêm 2,5 giây). Hoá lốc xoáy giờ phình cả
cơn lốc (trước chỉ phình lớp vỏ đầu tiên).

Chạy lại, đều **0 lỗi**: menu 75 (hình phình 0,42 → 1,00), 54c (lửa tắt 0,60 s, lò mọc lại), 19 (9/9 vật hiện lại sau 30 s).

Ảnh: `PlayTestShots/thanlocxoay_0_dem_can.png` (đêm, nhìn từ dưới lên như ảnh mẫu), `thanlocxoay_1_ngay_goc_choi.png`;
ảnh render Blender: `CongCu/Blender/loc_xoay_render/xem_truoc_1…8.png`.

**Sau đó cùng ngày — dải khói trôi lên thật.** Anh khen hình lốc và xin "hiệu ứng cuộn từ dưới lên nhiều và chi tiết
hơn", bảo tôi đề xuất để duyệt. Tôi đưa bốn phương án (dải khói trôi lên thật · vệt gió xoắn ốc bay lên · mảnh vụn bị hút
xoáy lên · khói bụi cuộn ở chân bốc lên), anh chọn **dải khói trôi lên thật** — không thêm hạt nào.

Trước đó dải xoắn chỉ QUAY (trông như đi lên nhờ ảo giác cột đèn cắt tóc). Để ảnh trượt được theo chiều dọc mà không lộ
đường nối, tôi vẽ lại bốn ảnh gió trong Blender với nhiễu 4D trải trên **hình xuyến** (cos/sin theo u và theo v), dải xoắn
N dải nguyên nên khép vòng cả hai chiều; phần mờ ở chân / miệng và phần xám đậm dưới chân không thể nằm trong ảnh nữa (nó
sẽ trôi theo) nên chuyển sang **màu đỉnh** của lưới FBX. Bản render Blender bằng chính ảnh PNG × màu đỉnh giống hệt bản cũ.

**Số đo** (menu 82, dấu vết `[ban 3]` — **0 lỗi**):

| Đo | Kết quả |
|---|---|
| Ảnh liền mạch theo chiều dọc | mép trên–dưới lệch 0,008 / 0,015 / 0,017 / 0,011, ngang hai hàng cạnh nhau giữa ảnh (0,009 / 0,017 / 0,026 / 0,012) |
| Màu đỉnh trên lưới đã nhập | alpha chân 0,00, giữa 1,00; xám chân 0,40–0,65 → giữa 0,90–0,94; v từ chân 0,00 lên đỉnh 1,00 |
| Tốc độ dải khói đi lên (đo trong Play) | **+3,00 / +2,40 / +1,95 / +1,50 m/s** (lớp trong nhanh hơn); vành 0 |
| **Đối chứng** đảo chiều trượt một lớp | lớp ấy đo ra −1,95 m/s (trôi xuống), lớp bên cạnh vẫn +2,40 |


### Thân Lốc xoáy nở 20% ở nửa dưới, mọi vệt trắng cuốn một chiều; Gió lốc hất tung 80% (25/09/2026)

Anh khoanh phần thân thắt hẹp của Lốc xoáy (từ mặt đất lên giữa thân) và xin **rộng thêm 20%**, và các **đường viền trắng
đậm chỉ cuốn đúng một hướng** — lúc đó nhiều vệt cuốn theo các chiều khác nhau. Tôi hỏi lại hai điều, anh chọn: **nửa dưới
+20% rồi nhạt dần lên** (miệng loe trên cùng và mây giông giữ nguyên cỡ) và **vùng hút nở theo hình**. Kèm Gió lốc: hất
tung **55% → 80%**.

**Vì sao vệt trắng xoáy hai chiều.** Thân lốc có ba lớp vỏ và ba dải xoắn. Lớp vỏ giữa quay −210°/s, hai lớp kia +320 và
+140, ba dải xoắn +230…+340. Trong Unity, xoay dương quanh trục đứng làm góc nhìn từ trên xuống **giảm** — ngược với chiều
quái bị cuốn bay (góc **tăng**). Nên vỏ giữa quay đúng chiều cuốn, còn năm lớp kia quay ngược. Comment cũ ở dải xoắn ghi
"quay dương để vân xoắn chạy lên", nhưng tính ra với chiều thật thì vân xoắn đang **tụt xuống**. Nay mọi lớp quay theo
đúng chiều của Gió lốc (đã đo ở menu 71). Trượt ảnh ngang trên vỏ cùng dấu với chiều quay, các hệ hạt (bụi chân, vệt vút
lên, khói) cũng quỹ đạo cùng chiều.

**Nở thân.** Một hàm `HeSoNoThanLoc`: ×1,20 từ chân tới nửa chiều cao, giảm mượt về ×1,00 ở miệng loe. Hàm được nhân
thẳng vào đường viền phễu nên vỏ, dải xoắn, tia sét trong lòng lốc đều theo; quỹ đạo kẻ bị cuốn và vùng hút
(`catchRadius` 4,32 → 5,184) cũng theo. Số này nằm trong **prefab** `Skill_LocXoay` nên phải nướng lại bằng menu 13. Lần
nướng ấy sinh thêm bốn ảnh `Tex_*_3.png` giống hệt từng byte ảnh `_2` đang dùng và trỏ vật liệu sang đó — tôi trả vật liệu
về và xoá ảnh trùng, để bản web không nặng thêm vô ích.

**Phát hiện thêm, chưa sửa:** hạt cát `Grit` quanh chân lốc (cả Lốc xoáy lẫn Gió lốc) **không quay chút nào**. Hàm dựng
nó đặt trục đứng bằng hai hằng số còn hai trục kia một hằng số; Unity báo "velocity curves must all be in the same mode"
và bỏ qua cả mô-đun vận tốc, nên hạt chỉ văng ra theo tốc độ ban đầu. Nó là hạt đất nhỏ màu sẫm, không phải vệt trắng; sửa
thì đổi luôn hình Gió lốc nên để anh quyết.

**Số đo** (menu 82 mới — **0 lỗi**):

| Đo | Kết quả |
|---|---|
| Bán kính thật trên lưới 3 lớp vỏ, so với đường viền CŨ chép tay trong phép thử | 4 vòng dưới ×1,200; rồi ×1,184 · ×1,102 · ×1,025; miệng loe ×1,000 (vỏ ngoài 9,08 m như cũ) |
| Vùng hút | `catchRadius` 5,184; bia cách 4,8 m **bị cuốn**, đối chứng 5,9 m không |
| Chiều cuốn thật (bia đang bay quanh lốc) | góc tăng |
| Lốc xoáy mới | **9/9** mục cùng chiều: vỏ +455 · +342 · +255 °/s (quay + trượt), dải xoắn +230 · +285 · +340, bụi 99–100%, vệt vút 100%, khói 100% hạt |
| **Đối chứng** dựng lại đúng cấu hình cũ | 1/9 mục cùng chiều — phép đo bắt được đúng cái anh thấy |
| Vân dải xoắn | lên cao 1,87 m thì góc −117°, quay theo chiều cuốn → vân **chạy lên** như Gió lốc |
| Gió lốc hất tung (menu 71, 190 lần trúng) | **80,5%** (153 lần), đếm độc lập khớp bộ đếm trong code |

Chạy lại, đều **0 lỗi**: menu 54c (lửa tắt sau 0,59 s thay vì 0,80 — vùng hút rộng hơn nên chạm lò sớm hơn), 75, 19 (cuốn
9 vật thay vì 8, 30 s sau hiện lại 9/9).

Ảnh: `PlayTestShots/thanlocxoay_1_ngay_goc_choi.png`, `thanlocxoay_2_ngay_goc_choi_sau.png`, `thanlocxoay_3_can_than.png`.

**Sau đó cùng ngày — bỏ hẳn ba dải xoắn.** Anh chơi thử, vẽ nét đỏ và xanh lên ảnh chụp những "đường viền trắng uốn éo"
quấn quanh thân lốc và xin bỏ đi. Đó là ba dải `DaiXoan0–2` (dựng bằng `BuildSpiralRibbons`, quấn 2,35 vòng từ chân lên
miệng) — thứ duy nhất trên thân lốc có dạng một đường xoắn liền. Nay không dựng nữa (hàm vẫn giữ). Menu 82 đếm cả theo tên
vật lẫn tên lưới: **0 dải xoắn**; chiều quay còn 6/6 mục cùng chiều cuốn (3 vỏ, bụi, vệt vút, khói), đối chứng cấu hình cũ
1/6. Menu 75 vẫn 0 lỗi. Ảnh ban đêm nhìn từ dưới lên như ảnh anh gửi: `PlayTestShots/thanlocxoay_0_dem_can.png`.

**Và một lỗi tôi gây ra giữa chừng: khói nổ, quả cầu lửa MÀU HỒNG.** Anh chơi thấy những khối hồng khổng lồ. Menu 13 (nướng
lại prefab Lốc xoáy) sinh thêm ảnh trùng và trỏ 9 vật liệu sang đó; tôi trả vật liệu về bằng git và xoá ảnh trùng — file trên
đĩa đúng hoàn toàn, nhưng Unity nạp lại vật liệu thành đối tượng MỚI trong khi các prefab đang trong bộ nhớ (`Vfx_NoLua`,
`Skill_QuaCauLua`, `Vfx_SetChamDat`, `Vfx_VungGiong`, `Skill_LocXoay`) vẫn giữ con trỏ tới đối tượng cũ đã bị xoá → vật liệu
null → hồng. Sửa: nạp lại (ImportAsset ForceUpdate) các vật liệu và cả năm prefab; quét 62 prefab + cảnh: 0 ô hỏng; hai lần
Play liên tiếp sinh vụ nổ + quả cầu lửa + Lốc xoáy: 0/21 hỏng, cả cảnh 0/1002. Bản web không dính (build từ commit trước).

### Thiên thạch đốt cháy bia mộ và nhà mồ y hệt cây (25/09/2026)

Anh xin Thiên thạch "đốt cháy mọi thứ như đốt cháy cây, hiệu ứng giống y chang". Tôi hỏi lại vật nào và cháy xong ra
sao. Anh chọn **bia mộ (452) và nhà mồ (5)**; đá và hàng rào không cháy. Cháy xong **y hệt cây**: lửa lan khắp bề mặt,
vật đen dần, cháy rụi thì biến mất, 30 giây sau mọc lại.

Dùng lại nguyên `CayChay`, không viết hiệu ứng mới. Hai việc phải làm thêm:

- **Nhận ra bia và nhà.** Cây nhận ra theo tên (`TREE_...`), nhưng 452 tấm bia có tên đủ kiểu (`TS_round_0`,
  `TS_shoulder_12`...) và 40 kiểu lưới khác nhau. Cái chung duy nhất là **nhóm cha** `BiaMo` / `NhaMo`, đúng nhóm mà
  menu 72 vẫn dùng để phân loại vật cản.
- **Điểm mồi lửa.** Lưới trong `map_luoi.fbx` tắt Read/Write, nên lúc chạy không đọc được bề mặt; không có điểm mồi
  thì lửa chỉ rải lung tung trong khối bao. Menu 20 nay nướng luôn 40 lưới bia và 2 lưới nhà (51 bộ, 340 điểm mỗi bộ).
  Chín bộ của cây nướng lại vẫn trùng từng byte, vì hạt ngẫu nhiên lấy từ tên lưới.

Bia rất thấp (0,16–3,18 m), mà cây thì chiều cao bị kẹp tối thiểu 2,5 m, nên với bia hạ ngưỡng này xuống 0,8 m để
đám lửa không to gấp mấy lần tấm bia. Bia và nhà dùng chung trần "tối đa 4 vật cháy cùng lúc" với cây, để máy
điện thoại không bị quá tải hạt lửa.

**Số đo** (menu 81 mới — **0 lỗi**; menu 18d cây cháy phép thật — **0 lỗi**):

| Đo | Kết quả |
|---|---|
| Nhận diện | 452/452 bia, 5/5 nhà "cháy được"; 40 + 2 kiểu lưới đều có bộ điểm 340 điểm; **đối chứng đá**: 0/229 cháy được, 0 bộ điểm |
| Tung Thiên thạch thật vào bia `TS_round_0` | bắt lửa, 340 điểm mồi, cháy 4,7 s, lửa lan 100%, độ sáng 0,697 → 0,162 (**×0,23**), mất hình + tắt va chạm, 30 s sau mọc lại đúng 0,697 |
| Tung vào nhà mồ `MAUS_A_001_682` | như trên: đen ×0,23, biến mất, mọc lại nguyên màu |
| Cùng lúc | nhiều nhất 3 vật cháy (trần 4); vật không phải cây / bia / nhà bị đốt: 0 |

Ảnh: `PlayTestShots/thienthach_bia_1_BiaMo.png`, `thienthach_bia_2_NhaMo.png`.

### Tầm Giựt sét bằng Sấm sét, Quỷ cây đánh xa hơn, Thiên thạch rơi dồn hơn (25/09/2026)

Anh xin ba việc. Tôi gửi số hiện tại và hỏi lại trước khi làm:
- Thiên thạch: khoảng chờ giữa hai quả là 0,7 giây, **anh chọn 0,35 giây**.
- Giựt sét 20 m, Sấm sét 12 m: "bằng nhau" theo hướng nào, **anh chọn Giựt sét giảm còn 12 m**.
- Tia của Quỷ cây vốn đã cùng kiểu Giựt sét từ lần vẽ lại, **anh chọn giữ màu xanh lá**.

**Quỷ cây đánh xa thêm 20%.** Nó dừng lại phóng sét ở 9,6 m (trước 8 m), tia bay tới 12 m (trước 10 m, thêm trường
`EnemyAI.tamTiaSet`). Chỗ dễ sai: Quỷ cây trong game sinh ra từ **prefab `Enemy_QuyCay`** do AssetBaker nướng, và prefab
ghi sẵn `attackRange: 8`. Chỉ sửa trong `EnemyFactory` thì con quái thật vẫn đánh ở 8 m (đúng cái bẫy "prefab đè giá trị
code"). Nên phải sửa cả prefab, và phép thử sinh **Quỷ cây thật từ kho quái** chứ không dựng tay.

**Số đo** (menu 69 và menu 62 — **0 lỗi**):

| Đo | Kết quả |
|---|---|
| Tầm Giựt sét | 12 m, bằng `boltRange` của Sấm sét đọc từ nhân vật; bia mặt ở 11,9 m trúng, mặt ở 12,4 m không trúng |
| Bắn 4 tia / tia lan / choáng | vẫn 4 tia × 75, tia lan 64, choáng 14,2% / 12,8% (160 lần) — các bia xếp lại trong tầm 12 m |
| Quỷ cây thật | tầm đánh 9,60, tầm tia 12,00; tự đi tới rồi dừng phóng ở **9,53 m**; tia màu xanh lá, dùng ảnh Giựt sét, sống 0,6 s |
| Thiên thạch cấp 5 | 5 quả chạm đất cách nhau 0,38 · 0,34 · 0,29 · 0,40 → trung bình **0,354 s**; cả loạt 1,42 s (trước 2,8 s) |

Ảnh: `PlayTestShots/giatset_7_quy_cay.png`.

### Sách phép: bình có sẵn và nâng cấp, quả cầu xuyên bia, băng dập lò, bị động Tốc độ (25/09/2026)

Anh xin bốn việc. Tôi hỏi lại ba chỗ, anh chọn: bình **tối đa cấp 3**; **Lửa địa ngục cũng va vào lò** (một luật
chung cho cả ba kỹ năng bay); icon Tốc độ **làm bằng script** như bốn icon Kháng.

**1. Bình máu / Bình mana có sẵn cấp 1.** Vào trận là cả hai đã ở cấp 1, không tốn điểm. Mỗi lần nâng cấp tốn
1 điểm: bình máu hồi thêm 75 (200 → 275 → 350), bình mana thêm 45 (75 → 120 → 165). Sách phép ghi "Cấp 2/3 · còn N
bình" ở góc hàng, và nút ở dưới giờ là "NÂNG LÊN CẤP 3". Có một chỗ phải rút gọn: dòng tóm tắt cũ "Uống một bình,
hồi tối đa 75 năng lượng" đè lên nhãn góc (nay dài hơn trước), nên đổi thành "Hồi tối đa 75 năng lượng".

**2. Quả cầu lửa, Quả cầu băng bay xuyên bia mộ.** Dùng lại đúng luật "vật nhỏ" của Lửa địa ngục (cao < 4 m và
ngang < 4 m). Có một điểm vướng: **lò lửa cao 2,33 m**, nên theo kích thước nó là vật nhỏ, và Lửa địa ngục vẫn bay
xuyên lò từ trước tới giờ. Nay mọi thứ có `LoLuaDa` đều chặn. Đoạn tìm vật chặn đường gom thành
`Fireball.VatCanChan`, dùng chung cho cả ba kỹ năng. Quả cầu lửa **của quái** không xuyên: chỉ đường tung của
người chơi bật cờ.

**3. Quả cầu băng, Mưa băng dập lửa lò.** Lò nằm trong vùng nổ của Quả cầu băng, hoặc trong vùng sát thương của tảng
băng rơi, thì bị dập như khi Gió lốc lướt qua, 30 giây sau cháy lại.

**4. Bị động mới "Tốc độ di chuyển"** (số hiệu 20, thêm ở cuối). Mở khoá cộng 10% **tốc độ gốc**, tức tốc độ lúc
vào trận, chưa tính phần tăng khi lên cấp nhân vật. Mỗi cấp sau cộng thêm 2,5%, cấp 5 là 20%. Có một cái bẫy
suýt dính: định chỉ cộng cho "nhân vật tự đọc phím" (`tuDocInput`), nhưng phép thử bơm input lại tắt đúng cờ ấy, nên
sẽ đo trúng một nhân vật không bao giờ được cộng. Nay điều kiện là "không phải bản sao của máy khác".

**Số đo** (menu 80 mới — **0 lỗi**; menu 65, 72, 68, 77, 66, 59, 60, 70 chạy lại cũng **0 lỗi**, sau khi sửa các
phép kiểm còn theo luật cũ: "đầu trận không kỹ năng nào mở", "nhóm Bị động có 4", "20 icon", "lò phải xuyên hết"):

| Đo | Kết quả | Đối chứng |
|---|---|---|
| Tốc độ, bơm input đi thẳng cùng một đoạn | cấp 0: 5,200 m/s · cấp 1: 5,720 (**×1,100**) · cấp 5: 6,214 (**×1,195**) | cấp 0 |
| Bấm kỹ năng bị động | không niệm, năng lượng 250 → 250 | — |
| Bình | đầu trận cấp 1/3, vẫn còn 1 điểm; uống hồi 200; nâng lên 3, lên 4 bị từ chối; cấp 3 hồi **350**, mana cấp 2 hồi **120** | — |
| Đường tung thật | Quả cầu lửa và Quả cầu băng đều bật cờ xuyên | quả cầu lửa của quái: không |
| Bắn vào bia 3 m | cả hai loại bay qua, đi 22 m | không bật cờ: nổ ở 8,4 m |
| Bắn vào cây 8 m / lò lửa | nổ ở 8,3 / 8,4 m (vật cách 9 m) | — |
| 10 lò thật trong Act2 | 0/10 collider bị coi là vật nhỏ | — |
| Quả cầu băng nổ cạnh lò | lò tắt | nổ cách lò khác 6 m: vẫn cháy |
| Tảng băng Mưa băng rơi cạnh lò | lò tắt | rơi cách lò khác 5 m: vẫn cháy |
| 31,5 giây sau | hai lò cháy lại | — |

Ảnh: `PlayTestShots/bonviec_1_binh_mau.png`, `bonviec_2_toc_do.png`.

### Act2: vòng đêm 4 phút → ngày 2 phút → chiều 2 phút, lặp tới hết trận (25/09/2026)

Anh xin: vừa vào game là **ban đêm 4 phút**, rồi **ban ngày 2 phút**, rồi **buổi chiều 2 phút**, rồi lặp lại như
ban đầu cho tới khi hết trận. Tôi hỏi cách chuyển giữa hai buổi, anh chọn **chuyển mượt 30 giây**: mỗi buổi giữ
nguyên ánh sáng của nó, chỉ 30 giây cuối mới chuyển dần sang buổi sau.

Một vòng 8 phút: đêm 0–210 s · bình minh 210–240 · ngày 240–330 · chiều xuống 330–360 · chiều 360–450 ·
hoàng hôn 450–480, rồi quay lại. Vẫn ba bộ ánh sáng cũ (ngày, chiều là hằng; đêm đọc thẳng từ cảnh). Có thêm
hai lần chuyển mới:
- **Bình minh** (đêm → ngày): sao tắt trong nửa đầu.
- Góc đèn đổi sang **Slerp quaternion**: đêm đọc từ cảnh ghi góc dạng 0..360 độ, nên Lerp từng góc Euler có thể làm
  mặt trời quay ngược cả vòng trong 30 giây.

**Lò lửa giờ phải tắt được.** Trước đây lò chỉ nhóm một lần rồi thôi, vì đêm là trạng thái cuối. Nay vào trận là
đêm nên lò cháy ngay từ đầu. Bình minh sáng quá mức 0,70 (giây 220,9) thì dập cả mười lò (còn làn khói), hoàng hôn
tối quá mức ấy (giây 469,1) thì nhóm lại; mỗi lần qua mức chỉ làm một lần. Có một kẽ hở tôi chặn luôn: lò bị Gió
lốc dập ban đêm được hẹn 30 giây sau tự cháy lại, nếu trời sáng trước giờ hẹn thì lò bùng lên giữa ban ngày. Nay cờ
đang tắt thì `Chay()` từ chối, kể cả lần hẹn ấy.

**Số đo** (menu 79 viết lại — **0 lỗi**):

| Đo | Kết quả |
|---|---|
| Vừa vào trận | giây 1,15, buổi "đêm", đèn `(0,520 0,720 0,980)` 0,88 ở 42°, sao 1, lò **10/10** cháy |
| Giữ nguyên trong buổi | đêm (0/100/205) · ngày (245/300/325) · chiều (365/420/445) đều không đổi một số nào; đối chứng: môi trường ngày 0,516 · chiều 0,295 · đêm 0,143 |
| Ba lần chuyển | bình minh sáng dần, sao 1,00 → 0 · chiều xuống đèn ấm dần 0,140 → 0,660 · hoàng hôn tối dần, sao 0 → 1,00 — không mốc nào đi ngược; cuối mỗi lần chuyển trùng buổi sau |
| Lặp lại | giây 480 đêm, 780 ngày, 900 chiều, 1060 đêm — trùng vòng đầu |
| Lò lửa | đêm 10 · ngày + chiều 0; giây 220,6 → 10, 221,2 → 0; giây 468,8 → 0, 469,4 → 10 |
| Gió lốc dập lò | vẫn đêm → cháy lại (đối chứng); trời đã sáng → **không** cháy lại |
| Đồng hồ | 6,02 s thật → vòng tăng 6,02 s |
| Chỉ Act2 | Act1 không có component, đèn giữ nguyên |

Ảnh: `PlayTestShots/anhsang_1_dem_dau_tran.png` … `anhsang_6_hoang_hon.png`.

### Act2: ban ngày → xế chiều → đêm, mỗi chặng 2 phút (24/09/2026)

Anh xin thêm một chặng **ban ngày** ở đầu: vào trận là ban ngày, 2 phút chuyển dần sang xế chiều. Tôi hỏi lại
chỗ duy nhất chưa rõ — *sau xế chiều thì sao* — và anh chốt: xế chiều **vẫn tối dần thành đêm trong 2 phút nữa**.
Tổng **4 phút**: ngày (giây 0) → xế chiều (giây 120) → đêm (giây 240). Lò lửa tắt suốt ban ngày và xế chiều.

`ChuyenChieuSangDem` viết lại quanh **một kiểu dữ liệu chung** `BoAnhSang` (đèn · môi trường · sương · bầu trời ·
đĩa sáng · đèn nhân vật). Ba bộ: **Ngày** và **Xế chiều** là hằng số, **Đêm** vẫn đọc thẳng từ cảnh như trước.
Hai chặng nội suy bằng **cùng một hàm** — thêm chặng thứ tư sau này chỉ là thêm một bộ số.

Ban ngày: mặt trời **cao 55°**, đèn trắng hơi ấm `(1,00 0,95 0,86)`, trời xanh nhạt, mây trắng, sương xám xanh thưa
0,0060 — vẫn hơi u ám chứ không rực rỡ như đồng cỏ. Mặt trời **hạ dần** 55° → 13° trong chặng đầu (bóng dài dần),
rồi ánh trăng lên 42° ở chặng sau. Mỗi chặng có đường cong smoothstep riêng nên đúng lúc xế chiều ánh sáng "dừng
lại một nhịp" chứ không bị gãy.

**Số đo** (menu 79, **0 lỗi**):

| Đo | Kết quả |
|---|---|
| Vừa vào trận | giây 1,05 · tiến độ thật 0,0044 · đèn ấm **0,140** · độ sáng đèn **1,288** · góc **55°** · môi trường **0,516** · sao 0 |
| Giữa đường (giây 120) | xế chiều đúng như trước: ấm **0,660** · độ sáng 1,008 · góc **13°** · môi trường 0,295 · sao 0 |
| Chín mốc 0 → 1 | đèn tối dần 1,288 → 0,613; môi trường 0,516 → 0,143; sao 0 → 1 không bao giờ bớt; màu đèn **ấm dần** ở chặng đầu (0,14 → 0,66) rồi **nguội dần** (→ −0,46) — không mốc nào đi ngược |
| Đồng hồ | 6,02 giây thật → tiến độ +0,0251 (đúng 6/240) |
| Hết 4 phút | trùng khít đêm cũ từng con số |
| Lò lửa | giây 0 · 60 · 120 · 156 · 192 → **0 lò**; giây 201 và 240 → **10 lò** (nhóm tại giây 196) |
| Chỉ Act2 | nạp Act1: không có component nào |

Một cái bẫy trong ảnh chụp: lần đầu, ảnh "giữa ngày và chiều" lại thấy **lò đang cháy** — không phải lỗi game, mà vì
ảnh ấy chụp sau khi mục đo lò lửa đã nhóm lửa (nhóm rồi thì không tắt lại). Mọi ảnh trước giờ nhóm lửa nay chụp
**trước** mục đo lò lửa; ảnh giây 60 và giây 180 đều thấy lò tắt, đúng như trong game.

### Act2: vào trận lúc xế chiều, tối dần thành đêm trong 2 phút (19/09/2026)

Anh xin: mới vào game thì Act2 sáng như **buổi xế chiều**, rồi trong **2 phút** chuyển dần sang đêm tối
**giống hiện giờ**. Chỉ Act2.

Thuận lợi là Act2 **không nướng lightmap nào** (đo được 0), nên mọi ánh sáng đều là thời gian thực và đổi
lúc chạy là ăn ngay. `Art/ChuyenChieuSangDem.cs` nội suy bảy nhóm cùng lúc: đèn hướng (màu, độ mạnh, độ đậm
bóng, **và góc chiếu**), ba tầng ánh sáng môi trường, màu và độ đậm sương mù, bầu trời (đỉnh trời · chân
trời · mây · sao), đĩa sáng trên trời, và đèn điểm bám theo nhân vật.

**Trạng thái đêm không viết tay trong file mới.** Nó **đọc thẳng từ cảnh** ngay lúc bắt đầu, sau khi
`WorldFactory` đã đặt xong — nên sau này ai đổi màu đêm thì cái đích tự đi theo. Đây chính là bài học vừa
phải trả giá ở [bốn ô trống trong Sách phép](#): hai nơi giữ hai bảng rồi lệch nhau.

Vài lựa chọn nhỏ trong đó:

- **Mặt trời thấp rồi lên cao.** Xế chiều đèn ở **13°** nên bóng cây đổ dài và xiên; tới đêm về **42°** như
  ánh trăng cũ. Đĩa sáng trên trời đi theo đúng hướng đèn (cam to → trắng nhỏ).
- **Sao chỉ hiện ở nửa sau.** Trời còn quầng đỏ ở chân trời mà đã đầy sao thì trông rất giả.
- **Đường cong smoothstep**: mấy giây đầu và mấy giây cuối đổi chậm, khúc giữa đổi nhanh — ra cảm giác
  "trời sập tối" chứ không phải một cái vặn đều đều.
- Đồng hồ là `Time.timeSinceLevelLoad`, mỗi máy tự chạy. Cả phòng vào trận cùng lúc (đếm ngược 10 giây ở
  sảnh) nên mọi người thấy gần như cùng một khung trời, và **không tốn gói tin nào**.

**Số đo** (menu 79 — mới, **0 lỗi**):

| Đo | Kết quả |
|---|---|
| Vừa vào trận | đèn `(1,00 0,64 0,34)` ấm 0,660 · mạnh **1,45** · góc **13°** · sao **0** · sương ấm 0,360 |
| Năm mốc 0 → 1 | đèn 1,45 → 0,88; ấm 0,660 → **−0,460**; môi trường 0,295 → 0,143; sao 0 → 1; góc 13° → 42° — **không mốc nào đi ngược** |
| Đồng hồ | sau 6,00 giây thật, tiến độ tăng 0,0500 (đúng 6/120) |
| Hết 2 phút | **trùng khít** đêm hiện giờ: đèn `(0,520 0,720 0,980)` 0,88 / bóng 0,62 / 42°, ba tầng môi trường, sương `(0,115 0,175 0,215)` 0,0105, sao 1, quầng đĩa 0,9 |
| Đối chứng | đêm thì đèn **lạnh** (ấm −0,460), ngược hẳn lúc xế chiều (+0,660) |
| Chỉ Act2 | nạp Act1: không có component nào, đèn Act1 giữ nguyên |


**Mười lò lửa chỉ cháy khi trời đã tối** (anh xin ngay sau đó). Lò **không nhóm lửa từ đầu** chứ không phải
nhóm rồi dập — dập thì mỗi lò phun một cuộn khói lúc vừa vào trận, mười cuộn cùng lúc trông rất lạ. Cờ
`LoLuaDa.ChoPhepNhomLua` đặt trong **Awake** của `GameBootstrap`, trước mọi `Start`, vì thứ tự `Start` giữa các
vật thể không cố định. Tới mức **0,70** trên đường cong (khoảng giây thứ 78) thì bật cờ và nhóm cả mười lò —
**một lần duy nhất**, không gọi mỗi khung, nếu không sẽ nhóm lại chính cái lò mà Gió lốc vừa dập tắt và hẹn
30 giây sau mới cháy lại. Số đo (menu 79 mục G): mức 0,00 · 0,30 · 0,65 → **0 lò**; mức 0,75 và 1,00 → **10 lò**.

Chỗ này làm tôi vấp hai lần liền, cùng một kiểu: mục G ban đầu nằm cuối kịch bản, mà các mục trước đã gọi
`Ap(1f)` — lửa nhóm rồi thì không tắt lại nữa, nên đo mốc nào cũng thấy đủ mười lò đang cháy. Phải đưa nó lên
**ngay đầu**, trước mọi mục khác.

**Việc này làm lộ một vấn đề lớn hơn với phép thử.** Hàng chục phép thử chụp ảnh trong Act2 ở những giây đầu
trận — giờ nền trời cam thì ảnh đối chứng nào cũng lệch. Cách xử: **sự có mặt của `ChayThuMang`** (vật thể mà
mọi kịch bản chạy thử đều dựng) là dấu hiệu "đang chạy thử" → nhảy thẳng tới đêm, ánh sáng đứng yên như trước.
Phải kiểm ở **cả hai đầu** vì thứ tự không cố định: vật thể ấy tạo từ `EditorApplication.update`, có thể trước
hoặc sau `Start`. Riêng menu 79 bật cờ cho phép và tự trả lại khi xong.

**Và ba phép đo mong manh lộ ra.** Menu 73 (Tàng hình) kiểm "đòn đầu nhân đôi" bằng cách đếm **tổng máu bia
mất**. Con số ấy phụ thuộc bao nhiêu quả trúng được bia và trúng chỗ nào (sát thương vùng giảm dần từ tâm ra
rìa), nên hai lần chạy **cùng một bản code** cho 1,35 và 1,73. Nay cả ba đọc **số ghi trên từng đòn** — đúng
cách mà một ca khác trong chính phép thử ấy đã làm — và ra đúng **×2,00** mọi lần. Cùng lúc, một ca bắt nhầm
quả cầu lửa **của quái** (16,80 sát thương) nên ra tỉ lệ 0,20 rồi 10,12; nay lọc theo người tung.

Còn một chỗ thật sự thay đổi hành vi: Quả cầu băng cấp 5 nay để lại tảng băng **nổ +100 cố định**, mà số cố
định thì không nhân đôi theo Tàng hình. Phép thử dọn tảng băng trước khi đếm và đo riêng phần đòn trực tiếp.

### Bảy nút kỹ năng tròn to thêm 20% (19/09/2026)

Anh xin cụm nút cảm ứng **to thêm 20%**, và dặn: đừng để chúng chồng lên nhau, đừng tràn qua mép phải hay
tràn xuống dưới màn hình.

Cách làm đã có sẵn trong chính comment của `GameHUD` từ hai lần phóng to trước (1,1 rồi 1,15): **nhân tất cả
cho cùng một hệ số** — bán kính nút, bán kính hai cung, bán kính cung riêng của nút thứ bảy, **và cả lề từ góc
màn hình** — trong khi giữ nguyên mọi góc. Chỉ phóng to cái nút mà giữ nguyên hai cung thì các nút xúm vào nhau.

| | Trước | Sau |
|---|---|---|
| Bán kính nút | 60,72 | **72,864** |
| Lề từ góc màn hình | 101,2 | **121,44** |
| Cung trong · cung ngoài · cung nút thứ 7 | 234,03 · 392,15 · 92,23 | **280,84 · 470,58 · 110,68** |
| Cả cụm chiếm chỗ | 525,5 × 520,2 | **630,6 × 624,2** |

**Số đo** (menu 78 — mới, `cumnut.txt`, **0 lỗi**):

| Đo | Kết quả |
|---|---|
| To đúng 20% | bán kính ×1,200; bề ngang ×1,200; chiều cao ×1,200 (cụm không bị kéo méo) |
| 21 cặp nút | chỗ hở hẹp nhất **18,5** trên đường kính 145,7 — trước là 15,4 trên 121,4, **đúng cùng tỉ lệ** |
| Mép phải · mép dưới | còn dư ở **mọi** độ phân giải ngang, hẹp nhất là điện thoại 844×390: **38,6** và **35,1 px** |
| Cần điều khiển | không nút nào chạm, chỗ hở gần nhất 289,9 px (máy 4:3 — trường hợp xấu nhất) |

**Phép đo "không chồng nhau" tự nó vô nghĩa nếu không có đối chứng**, nên menu 78 tính thêm phương án sai —
*chỉ nút to lên, hai cung giữ nguyên* — và bắt nó phải hỏng: chỗ hở ra **−8,9** (tức chồng nhau). Có vậy con
số 18,5 ở trên mới nói lên điều gì.

**Một chỗ màn hình dọc.** Ở 390×844 cụm vượt mép **trái** 102,8 px — nhưng bản cũ cũng đã vượt 20,7 px, và
bản cảm ứng vốn chơi ngang (cần điều khiển góc trái dưới, cụm nút góc phải dưới, hai cái chỉ vừa nhau khi màn
nằm ngang). Phép thử **ghi lại cả hai con số** thay vì tính là lỗi. Nếu anh muốn chơi dọc được thì bảo tôi.

**Và một cái bẫy khi chụp ảnh đối chứng.** Đặt `CamUng.EpBat = true` rồi chụp thì ra ảnh **bản máy tính** với
thanh ô vuông — vì `GameHUD.Update` ghi đè `CamUng.EpBat = epCamUng` mỗi khung hình. Phải đặt `hud.epCamUng`.
Nay phép thử kiểm luôn `CamUng.DangDung` trước khi tin vào ảnh.

**Sửa lại ngay sau đó**: anh thấy cụm nút *"hơi bị to quá"* nên xin **nhỏ lại 10%** (tổng cộng còn **×1,08** so
với trước khi phóng to), và xin **cần điều khiển to thêm 10%**. Cùng một cách làm: nhân đều, giữ nguyên góc.

| | Trước mọi thay đổi | Cuối cùng |
|---|---|---|
| Bán kính nút · lề | 60,72 · 101,2 | **65,578 · 109,296** |
| Ba cung | 234,03 · 392,15 · 92,23 | **252,75 · 423,52 · 99,61** |
| Cả cụm | 525,5 × 520,2 | **567,6 × 561,8** |
| Cần điều khiển: bán kính · tâm · lề | 172,5 · 232,5 · 60 | **189,75 · 255,75 · 66** |

Cần điều khiển cũng phải nhân **cả tâm**, không chỉ bán kính — giữ nguyên tâm mà bán kính to lên thì lề tụt từ
60 xuống 42,75 và cần bị cắt ở mép. Số đo sau khi sửa (menu 78, **0 lỗi**): nút ×1,080 cả ba chiều; cần ×1,100
cả bán kính lẫn tâm, lề 60 → 66; chỗ hở hẹp nhất giữa hai nút **16,6** trên đường kính 131,2; mép phải · mép
dưới còn dư ít nhất **34,8 · 31,6 px** (điện thoại 844×390); cụm nút cách cần điều khiển gần nhất 303,7 px.

**Đối chứng phải độc lập với con số đang chỉnh.** Đối chứng cũ là *"thu cụm về 1/hệ số, giữ nút"* — đúng khi hệ
số là 1,2, nhưng khi nó thành 1,08 thì phép ấy chỉ phóng nút lên 8%, không đủ để chồng nhau, và đối chứng tự báo
"vẫn còn hở" — tức phép đo B mất giá trị mà không ai biết. Nay đối chứng dùng **một mức cố định**: phóng riêng
cái nút thêm 20% trong khi hai cung giữ nguyên, và nó phải cho chồng nhau (**−9,6**).

### Bốn ô trống trong Sách phép ở sảnh — một bản sao bị bỏ quên (19/09/2026)

Anh bấm **KỸ NĂNG** ở phòng chờ và thấy bốn kỹ năng Kháng hiện ra **bốn ô trống trơn** — không có cả cái đĩa
nút, trong khi mọi kỹ năng khác vẫn đủ hình.

Nguyên nhân không nằm ở ảnh icon (chúng vẫn đúng trong trận): bộ biểu tượng có **hai bản**. `GameHUD` giữ một
bản, `ManSanh` giữ một bản riêng cho Sách phép xem trước. Khi thêm nhóm BỊ ĐỘNG tôi sửa bản của `GameHUD`
thành 20 ô mà **không biết có bản thứ hai** — nó vẫn dài 16, nên kỹ năng số 16–19 không có gì để vẽ.

Nay chỉ còn **một bảng duy nhất**: `IconKyNang.BoDayDu()`, xếp theo số hiệu kỹ năng và dựng theo
`CapDo.SoKyNang`; cả HUD lẫn sảnh đều gọi nó. Thêm kỹ năng mới thì thêm đúng một dòng ở một chỗ.

**Vì sao menu 66 vẫn báo xanh suốt thời gian ấy.** Phép đo độ sáng của nó **bỏ qua mọi hàng bị khuất dưới đáy
cột** ("phải cuộn mới thấy") — mà bốn kỹ năng Kháng nằm **cuối** danh sách, nên chúng chưa bao giờ được đo.
Mục **B1b** mới: đặt `CuaSoSachPhep.CuonKho` cho cuộn hết cột rồi đo đúng bốn hàng ấy, cộng một phép kiểm rẻ
là bảng biểu tượng phải đủ `CapDo.SoKyNang` ô và không ô nào rỗng.

Và phép thử ấy suýt nữa cũng vô dụng. Tôi **tháo biểu tượng ra để đối chứng**, và bản hỏng **vẫn lọt**:

| | Độ sáng vành hình bốn ô | So với các kỹ năng khác cùng khung hình (0,227) |
|---|---|---|
| Có biểu tượng | 0,288 – 0,352 | ×1,27 – 1,55 |
| Tháo biểu tượng ra | 0,115 – 0,127 | ×0,51 – 0,56 |

Ngưỡng đầu tiên tôi đặt là ×0,5 — bản thiếu biểu tượng đo được 0,115, mà ngưỡng là 0,1135. Lọt qua trong gang
tấc, và phép thử báo "0 lỗi" cho đúng cái lỗi anh vừa nhìn thấy. Nay ngưỡng là **×0,85**, nằm giữa hai cụm số:
bản đúng xanh, bản tháo biểu tượng **đỏ**.

**Một lỗi nữa lộ ra cùng lúc.** Mục B1 cũ tìm hàng bằng phép nhân `chỉ số × chiều cao hàng` — đúng cái bẫy
CLAUDE.md đã ghi từ khi cột xếp theo nhóm hệ (mỗi nhóm có một dòng tiêu đề). Nó đo trúng khoảng trống giữa các
nhóm nên cho tỉ lệ lung tung; sau khi hỏi `CuaSoSachPhep.VungHangKyNang`, tỉ lệ thấp nhất nhảy từ **×1,35 lên
×3,22**.

### Lửa địa ngục xuyên bia mộ; Quả cầu lửa cấp 5 đánh ngã; Thiên thạch cấp 5 năm quả (19/09/2026)

Ba việc nhỏ anh giao cùng lúc.

**1. Lửa địa ngục bay xuyên vật nhỏ.** Anh xin: xuyên được bia và mộ, *không* xuyên được cây cối và nhà.
Câu hỏi thật sự là "làm sao biết cái này nhỏ" — và tôi không muốn đoán theo tên, vì Act1 dựng bằng code còn
Act2 nhập từ Blender, tên hai bên chẳng liên quan gì nhau. Nên tôi **đo cả hai màn trước khi viết một dòng nào**:

| | Act2 | Act1 |
|---|---|---|
| Nhỏ | 452 bia mộ cao **0,16–3,18 m**, 229 tảng đá 0,12–1,31 m, 10 lò lửa 2,33 m | — |
| To | 58 cây **6,54–17,47 m**, 5 nhà mồ 4,22–5,15 m (rộng 4,37–6,16), hàng rào 4,77 m rộng 107 m | 76 vách đá 4,53–14,55 m |

Giữa "bia cao nhất" 3,18 m và "vật to thấp nhất" 4,22 m có một khe trống hơn **1 mét** — ngưỡng **4,00 m**
nằm đúng giữa. Luật thành: vật cản **cao dưới 4 m và ngang dưới 4 m** thì quả bay xuyên; mặt đất và
người/quái thì không bao giờ (dẫm đất phải nổ, trúng người phải nổ). `Fireball.LaVatNho` + `xuyenVatNho`,
chỉ Lửa địa ngục bật.

Một chỗ dễ sai: không được chỉ hỏi "vật gần nhất rồi bỏ qua nếu nó nhỏ". `SphereCast` trả về **một** vật, mà
vật ấy rất hay là cái bia đang đứng chắn trước gốc cây — bỏ riêng nó đi thì quả xuyên luôn qua cây phía sau.
Phải quét cả đoạn (`SphereCastAll`) rồi lấy vật **to** gần nhất.

**2. Quả cầu lửa cấp 5: 30% đánh ngã**, nằm 1,5 giây đúng như Thiên thạch (dùng luôn `ThienThach.GieoDanhNga`
nên tự động bỏ qua người tung, kẻ đã chết và kẻ đang có khiên). Mỗi quả trong chùm ba quả gieo riêng —
giống hệt loạt ba quả Thiên thạch vẫn làm.

**3. Thiên thạch cấp 5 rơi 5 quả** thay vì 3 (`ThienThach.SoQuaTheoCap`, đọc cấp của **người tung** nên qua
mạng cũng đúng).

**Số đo** (ba menu, **0 lỗi**):

| Menu | Đo | Kết quả |
|---|---|---|
| 72 K1 | phân loại **755 vật cản thật** của Act2 | bia mộ 452/452 xuyên · đá 229/229 xuyên · lò lửa 10/10 xuyên · cây **0/58** · nhà mồ **0/5** · hàng rào **0/1** · mặt đất không xuyên |
| 72 K2 | bắn quả thật qua bia thử cao 3,0 m | Lửa địa ngục đi tiếp **20,3 m**; đối chứng Quả cầu lửa thường **nổ ngay ở bia** (8,4 m ≈ đúng chỗ bia) |
| 72 K3 | cây thử 8 m · nhà thử 5×4,5×5 m | nổ ở 8,3 m và 6,4 m — đúng mặt trước vật, không qua được |
| 72 K4/K5 | bia THẬT và cây THẬT trong nghĩa địa | qua bia thật đi tiếp 16,9 m; tới cây thật thì nổ ở 8,3 m |
| 72 K6 | đường tung thật | 5/5 quả Lửa địa ngục bật cờ xuyên; 0/3 quả Quả cầu lửa thường |
| 70 D1 | xác suất ngã trên quả cầu THẬT, cấp 1→5 | 0 · 0 · 0 · 0 · **0,30**, nằm 1,5 giây |
| 70 D2/D3 | một quả cấp 5 nổ giữa 400 bia | ngã **119 → 29,8%**; đối chứng cấp dưới 5: **0** |
| 62 G | số quả mỗi loạt, cấp 1→5 | **3 · 3 · 3 · 3 · 5** |

**Ba lần phép thử tự lừa mình.** Cả ba đều cùng một kiểu: *bấm phép xong đọc kết quả ngay ở khung hình sau*.
Nhưng phép có **thời gian niệm**, nên chưa có gì để đọc — mà mảng số đo thì mặc định bằng 0, và số 0 ấy
trông y hệt một lỗi thật ("cấp 5 không mang 30%", "mọi cấp đều rơi 0 quả"). Lần thứ ba là **hồi chiêu**:
chờ 1,2 giây giữa hai lần tung thì cấp 2 và cấp 4 bị từ chối, bảng số đo ra `3, 0, 3, 0, 5`. Nay cả hai chỗ
đều **đọc hồi chiêu thật từ nhân vật** (`SachPhep.ThongSo`) rồi mới tung tiếp — không chép tay con số nào.

**Và một dư chấn của luật mở khoá theo bậc.** Menu 18d (cây cháy bằng phép thật) báo *"thả 8 giây mà cây
không bắt lửa"*. Không phải lỗi mới: kịch bản ấy gọi thẳng `CastAt(4)` mà chưa bao giờ mở khoá kỹ năng, nên
từ hôm qua Thiên thạch bị khoá sau Quả cầu lửa cấp 2 là nó **bị từ chối trong im lặng** — không ngoại lệ,
không lời nhắc trong log, chỉ có "cây không cháy". Thêm `MoCaDuongChoPhepThu(4)` là xanh lại, và cây vẫn
cháy rụi rồi mọc lại sau 37,4 giây như cũ. Quét lại toàn bộ `Assets/Editor` còn tìm thấy hai kịch bản cũ nữa
mắc đúng lỗi này (menu 4 chụp hình, menu 7 kiểm hiệu ứng sót) — chúng chỉ chụp ảnh nên không ai thấy;
đã sửa luôn.

### Mở khoá theo bậc, và bình máu 200 / bình mana 75 (19/09/2026)

Anh xin mỗi nhóm hệ một **đường lên**: kỹ năng rẻ mở trước, lên đủ cấp mới mở được cái sau.

| Nhóm | Bậc 1 → bậc 2 | Bậc 2 → bậc 3 |
|---|---|---|
| LỬA | Quả cầu lửa **cấp 2** → Thiên thạch | Thiên thạch **cấp 5** → Lửa địa ngục |
| BĂNG | Quả cầu băng **cấp 2** → Mưa băng | Mưa băng **cấp 5** → Tàng hình |
| SÉT | Giựt sét **cấp 2** → Sấm sét | Sấm sét **cấp 5** → Quả cầu điện |
| PHONG | Gió lốc **cấp 2** → Lốc xoáy | Lốc xoáy **cấp 5** → Hoá lốc xoáy |

Chỉ chặn **lúc mở khoá** — mở rồi thì nâng cấp tự do, đúng chữ anh viết. Nhóm HỖ TRỢ và BỊ ĐỘNG không có điều kiện.
Sách phép nói rõ còn thiếu gì: *"Cần QUẢ CẦU LỬA cấp 2 mới mở được (đang cấp 0)"*, hiện cả trên dòng cấp lẫn trên
chính cái nút mở khoá. Cùng lúc, **bình máu hồi 200** (trước 100) và **bình mana hồi 75** (trước 50).

**Số đo** (menu 60 mục F và G, `capdo.txt`, **0 lỗi**):

| Đo | Kết quả |
|---|---|
| Tám luật | với mỗi luật: chưa có kỹ năng trước → **trượt**; có nhưng **thiếu một cấp** → vẫn **trượt**; đủ cấp → **mở được** |
| Đối chứng | Khiên, Giựt sét, Cầu lửa, Cầu băng, Gió lốc, Kháng Lửa, Tốc biến — **mở được ngay**, và không kỹ năng nào trong số đó có câu nhắc |
| Bình | uống thật: hồi đúng **200 máu** và **75 năng lượng** |
| Hồi quy | menu 59 Sách phép · 61 kinh nghiệm · 65 bình · 68 Quả cầu băng · 77 kháng hệ: **0 lỗi** |

**Luật mới làm gãy hàng loạt phép thử cũ**, vì chúng quen gọi `CapDo.MoKhoa(X)` thẳng cho bất kỳ kỹ năng nào.
Thêm `CapDo.MoCaDuongChoPhepThu(ky)` (mở cả đường dẫn tới nó) và thay ở 18 file. Ba cái bẫy gặp trên đường:

- **`MoCaDuongChoPhepThu` ban đầu bơm KINH NGHIỆM để lấy điểm** → nhân vật nhảy thẳng lên **cấp 20**, mà ở cấp tối
  đa thì giết quái không còn được kinh nghiệm: menu 61 đo ra "+0 kinh nghiệm" ở **mọi** kỹ năng. Nay có
  `CapDo.ThemDiemChoPhepThu` cho điểm thẳng, không đụng tới cấp.
- Cùng lý do đó, menu 60 mục D3 mở Mưa băng làm nhân vật lên cấp 20 trước khi D4 kịp đo *"lên cấp 2 thì máu tăng
  15%"* — D4 đo được ×1,000. Nay D3 dùng một kỹ năng không có điều kiện và tốn đúng một điểm.
- Menu 65 đo bề ngang nét chữ ở **hàng số 1** của cột trái bằng phép nhân `chỉ số × chiều cao hàng` — đúng cái bẫy
  `CuaSoSachPhep.YCuaDong` đã ghi trong CLAUDE.md từ khi cột xếp theo nhóm. Nay hỏi `VungHangKyNang`.

**Và một lần Unity treo cứng.** Đang chạy menu 60 thì Editor đứng hình hẳn, không ngoại lệ, không crash dump, log
chỉ còn một file đầy ký tự null vì chưa kịp ghi — anh phải tắt bằng tay. Thủ phạm là dòng tôi vừa thêm:
`while (CapDo.CapCuaKyNang(can) < capCan) CapDo.NangCap(can);`. `NangCap` trả `false` khi hết điểm và **không đổi
gì**, nên vòng quay vô tận ngay trong một khung hình. Mọi vòng chờ trong kịch bản chạy thử nay đều có trần
(`ThuCapDo.NangToiCap`). Khác với coroutine chết (Play kẹt nhưng Editor vẫn dùng được), kiểu treo này làm chết cả
Editor — và **log sẽ không có gì để đọc**, nên đừng mất công tìm exception.

### Nhóm BỊ ĐỘNG và bốn kỹ năng Kháng (19/09/2026)

Anh xin thêm vào Sách phép **một nhóm mới tên BỊ ĐỘNG**: *"các skill nằm trong nhóm này mỗi khi mở khoá hoặc
nâng cấp sẽ tăng vĩnh viễn thuộc tính của nhân vật"*. Bốn kỹ năng đầu tiên của nhóm là **Kháng Lửa, Kháng Băng,
Kháng Sét, Kháng Phong**: nhận sát thương từ kỹ năng hệ tương ứng **của người chơi khác** giảm **25% ở cấp đầu,
mỗi cấp thêm 5%** (cấp 5 = 45%). Icon vẽ bằng script Python, không dùng Blender (anh chốt).

Hỏi lại, anh chốt thêm hai điều: kỹ năng bị động **không kéo được vào ô kỹ năng** (nó tự chạy, không bấm được nên
không được chiếm một trong bảy ô), và **tia sét đánh trong lòng Lốc xoáy / Gió lốc tính hệ PHONG** — theo nhóm kỹ
năng, không theo loại sát thương.

**Chỗ khó: làm sao biết đòn này đến từ người chơi khác.** `Damageable.keDanhCuoi` không dùng được —
đòn của quái **không ghi gì cả**, nên nó vẫn giữ tên người chơi của đòn trước đó; hỏi nó thì đòn của quái cũng hoá
ra "đòn của người chơi" và bị Kháng chặn oan. Nên `GhiKeDanh` nay đặt thêm một **cái thẻ dùng một lần**: chỉ tin
khi được ghi **trong cùng khung hình**, và `TakeDamage` xoá nó ngay sau khi đọc. Mọi đường gây sát thương của
người chơi đều đã gọi `GhiKeDanh` ngay trước `TakeDamage` (luật cũ của dự án, menu 61 canh), nên không phải sửa
mười một chỗ để truyền thêm tham số.

**Hệ của một đòn** suy thẳng từ `DamageType` vì nó khớp sẵn với nhóm trong Sách phép: Fire→LỬA, Ice→BĂNG,
Lightning→SÉT, Physical→PHONG. Ngoại lệ duy nhất là tia sét của lốc — `Tornado.Bolt` nay gọi
`GhiKeDanh(boQua, HeSat.Phong)` để nói rõ hệ.

⚠️ Kháng đọc `CapDo` — cấp của nhân vật **máy này** — nên chỉ áp cho chính mình, không áp cho bản sao của người
chơi khác (`mauDoMayKhacQuyet`). Điều đó đúng với cách chơi mạng hiện tại: máu của mỗi người do chính máy họ
quyết, nên kháng của nạn nhân được tính ở đúng nơi biết cấp của nạn nhân.

**Số đo** (menu 77, `khanghe.txt`, **0 lỗi**, chạy hai lần liên tiếp đều giống nhau):

| Đo | Kết quả |
|---|---|
| Bảng tỉ lệ | cấp 0/1/2/3/4/5 = **0 / 25 / 30 / 35 / 40 / 45 %** |
| Giảm thật trên máu (cả bốn hệ) | đòn 400 của người chơi khác → chưa mở **400,0**; cấp 1 **300,0**; cấp 5 **220,0** |
| Đối chứng: đòn của QUÁI | **400,0** — không bị chặn, dù đang có đủ bốn kháng cấp 5 |
| Đối chứng: đòn không rõ nguồn | **400,0** |
| Đối chứng: kháng này không chặn hệ khác | chỉ mở Kháng Lửa cấp 5 → đòn Lửa **220,0**, đòn Sét **400,0** |
| Tia sét trong lòng lốc | chỉ có Kháng Sét cấp 5 → **400,0** (không chặn); chỉ có Kháng Phong cấp 5 → **220,0** |
| Đối chứng cho ca trên | Giựt sét thật vẫn bị Kháng Sét chặn: **220,0** |
| Không tung được | bấm cả bốn → **0** phép bay ra, **0** mana, hồi chiêu **0,00 s** |
| Cột Sách phép | tiêu đề "BỊ ĐỘNG" ở hàng 21, Kháng Lửa ngay sau ở hàng 22, tổng 26 hàng |
| Hồi quy | menu 59 (Sách phép, nay 20 kỹ năng): **0 lỗi** |

**Một lần chạy đầu ra số lệch mà tôi chưa giải thích được.** Lần đầu phép thử cho 224 thay vì 220, 304 thay vì
300, 384 thay vì 400 — lệch vài đơn vị theo cả hai chiều. Tôi nghi độ chính xác `float` (thanh máu để 10 triệu) và
đã **kiểm chứng riêng**: `1e7f - 220f` ra đúng `9999780`, tức float **không phải** nguyên nhân. Sau khi hạ máu thử
xuống 100 000 và thêm phép kiểm cấp kỹ năng ngay trước mỗi lần đo, mọi con số tròn trịa và ổn định qua hai lần
chạy. Tôi ghi lại đây vì chưa chứng minh được nguyên nhân thật — nếu số lệch quay lại thì chỗ này là manh mối.

### Tảng băng: chỉ mọc khi đóng băng được, và cấp 5 thì nổ tung (19/09/2026)

Anh xin hai việc. **Mưa băng**: *"Chỉ khi nào làm Đóng Băng đối thủ thành công thì mới cho xuất hiện tảng băng ở
dưới đất (mỗi đối thủ bị đóng băng thành công sẽ cho xuất hiện 1 tảng băng)"*. Và cho **cả Mưa băng lẫn Quả cầu
băng**: *"Khi skill đạt cấp 5, tảng băng đến thời gian biến mất thay vì biến mất như bình thường, thì sẽ nổ tung
gây thêm 100 sát thương cho đối thủ và các đối thủ ở gần đó"*. Hỏi lại, anh chốt: bán kính nổ **3,4 m** (bằng vùng
nổ của Quả cầu băng), sát thương **đúng 100 cố định**, và **Quả cầu băng giữ nguyên** cách mọc tảng băng như cũ —
luật "chỉ đóng băng mới có tảng băng" chỉ áp cho Mưa băng.

**Luật cũ so với luật mới.** Trước đây `FallingShard.TrungKeDich` hỏi *"trong 1,7 m có kẻ địch nào còn sống
không"* — có là mọc tảng băng, dù đóng băng được hay không (đóng băng chỉ là 35% số lần). Nay tảng băng mọc
**dưới chân từng kẻ bị đóng cứng**, mỗi kẻ một tảng; không đóng được ai thì vụ nổ vẫn có chớp, vòng lạnh, sương,
mảnh băng — chỉ không có tảng băng.

Việc này buộc phải **đảo thứ tự trong `FallingShard`**: bản cũ vẽ hình trước rồi mới gây sát thương. Muốn biết ai
bị đóng băng thì phải gieo xác suất xong đã, nên nay sát thương chạy trước, hình dựng sau.
`CombatUtil.AreaFreeze` có thêm một chỗ nhận danh sách kẻ bị đóng cứng để trả về.

**Vụ nổ** là `TangBangNo` gắn lên chính tảng băng: nó đọc tuổi thọ từ `AutoDestroy` của tảng (4 giây trong
`Vfx_NoBang.prefab`) và nổ sớm hơn **0,12 giây** — nếu để đúng bằng nhau thì có lần `AutoDestroy` xoá vật trước khi
`Update` kịp chạy và cả vụ nổ biến mất im lặng. Hình vụ nổ (`VfxFactory.NoTangBang`) dựng từ những mảnh có sẵn:
mảnh băng vỡ tung, vòng lạnh lan trên đất, sương bung, một ngọn đèn loé. ⚠️ **Không được dùng lại
`VfxFactory.NoQuaCauBang`** cho vụ nổ này — hàm ấy gọi `IceImpact`, tức lại mọc thêm một tảng băng mới: tảng băng
nổ ra tảng băng, không bao giờ dứt.

Cấp kỹ năng đi theo **cấp của NGƯỜI TUNG** (`capPhepDangTung`, đi kèm gói tin) chứ không phải cấp của người xem:
`IceStorm.capKyNang` giữ suốt 5 giây con bão rồi giao cho từng mảnh rơi, `QuaCauBang.capKyNang` đi cùng mỗi quả.

**Số đo** (menu 68 mục L và N, `quacaubang.txt`, **0 lỗi**):

| Đo | Kết quả |
|---|---|
| Đóng băng chắc chắn (quái) | **8 cụm gai**, đúng **1** tảng băng |
| Đóng băng chắc chắn (người chơi khác) | **8 cụm gai**, đúng **1** tảng băng |
| **Trúng quái mà KHÔNG đóng băng** | **0** tảng băng — điều anh xin, và là điều bản cũ làm sai |
| Đất trống / bia mộ / chính người tung | **0** tảng băng (đối chứng, giữ nguyên như trước) |
| Mưa băng thật (có bia đỡ đòn) | 36 quả chạm đất, **10 quả** có tảng băng ≈ 28% — khớp với xác suất đóng băng 35% |
| Mưa băng thật (không có kẻ địch) | 36 quả chạm đất, **0** tảng băng |
| **Cấp 5, vụ nổ** | bia tại chỗ mất đúng **100,0**; bia cách **3,00 m** mất **60,0** (giảm dần); bia cách **4,00 m** mất **0,0** |
| Đối chứng cấp 4 | **0** vụ nổ, không ai mất máu |
| Quả cầu băng cấp 5 | tảng băng sau vụ nổ cũng nổ, bia mất thêm **86,0** |
| Hồi quy | menu 58 (Mưa băng · Sấm sét): **0 lỗi** · menu 61 (kinh nghiệm, thêm ca B2b cho vụ nổ tảng băng): **0 lỗi** |

**Hai lỗi của phép thử, không phải của game.** Lần chạy đầu menu 68 báo *"đối chứng hỏng: cấp 4 mà tảng băng vẫn
nổ"* — 2 vụ nổ, bia mất đúng 55 máu. Hoá ra mục M ngay trước đó tung **Quả cầu băng thật ở cấp 5**, để lại mấy
tảng băng mang luật nổ, và chúng phát nổ đúng vào giữa lúc mục N đang đo đối chứng. Nay mục N dọn sạch
`TangBangNo` còn sống trước khi đo. Lỗi thứ hai: mục K báo *"ảnh đốm tròn cũ cũng đếm ra răng"* — hai ảnh đối
chứng để trong `Temp/`, mà Unity **xoá sạch thư mục đó mỗi lần khởi động lại**, nên sau lần Unity sập chúng không
còn. Đã chuyển sang `PlayTestShots/doichung/` và commit vào git.

### Quả cầu điện: đổi đường gân liền thành tia điện mỏng đứt quãng, chớp tắt (19/09/2026)

Anh gửi ảnh quả cầu trong game và xin: *"Thay các đường vân trắng thành các tia điện mỏng bao bọc xung quanh
quả cầu giống như các đường vân trắng hiện giờ (không vẽ liền mạch nhé)"*. Tôi hỏi lại hai điều và anh chốt:
tia **chớp tắt liên tục** (không đứng yên), và **mỗi đường vòng đứt thành nhiều đoạn** chứ không rải lung tung.

**Cách làm** — Blender MCP (`CongCu/Blender/tia_dien_qua_cau.blend` → `Resources/KyNang/QuaCauDien/VoTiaDien.fbx`):
- Vẫn **đúng năm đường vòng** của bản gân cũ (trục nghiêng cố định theo số ngẫu nhiên có hạt giống), nhưng mỗi
  đường bị **ngắt thành nhiều đoạn** có khoảng hở; hai đầu mỗi đoạn thuôn nhỏ nên tia tắt dần chứ không cụt.
- Xuất **bốn khung** (`VoTia0..3`): cùng năm đường ấy, các đoạn nằm chỗ khác nhau. Lúc chạy,
  `ChopTiaDien` chỉ **đổi `sharedMesh`** mỗi 0,045–0,105 giây (ngẫu nhiên, không bao giờ lặp lại đúng khung đang
  hiện) — một renderer duy nhất, không sinh rác, không bật/tắt GameObject.

**Hai lần đo lại vì mắt, không vì phép thử.** Bản đầu tôi để ống dày 0,011 (gân cũ 0,019) cho thật "mỏng".
Render trong Blender rất đẹp, nhưng **chụp trong game ở cự ly chơi thật** thì tia vỡ thành một đám lấm tấm —
mất hẳn nét tia. Lần hai: dày **0,014** và **zigzag thưa gấp đôi** (bước 0,115 rad thay vì 0,055) — ở Blender
trông thưa hơn nhưng trong game mới ra đúng hình tia. Ngưỡng của phép thử vì thế hạ từ "mỏng hơn 40%" xuống
**"mỏng hơn 20%"**, và tôi ghi rõ lý do ngay cạnh dòng ấy để lần sau không ai chỉnh ngược lên.

**Số đo** (menu 74 mục J, `quacaudien.txt`, **0 lỗi**):

| Đo | Kết quả |
|---|---|
| Đứt quãng | khung 0 có **35 cụm rời** — đối chứng vỏ gân cũ chỉ **4 cụm** (mỗi đường một nét liền) |
| Mỏng | bề dày ống **0,013** so với gân cũ **0,019** (đo bằng trung vị cạnh ngắn nhất của tam giác, chia √3) |
| Chớp tắt | trong 1,5 giây đổi **12 lần**, thấy đủ **4 khung khác nhau** |
| Trên cảnh | có `TiaDienBoc` + `ChopTiaDien`, **không còn** lớp `VoDien` liền mạch |
| Hồi quy | toàn bộ menu 74 (thông số, chỗ đặt, 10 lượt, 5 tia mỗi lượt, sát thương 155,52, choáng 30,8%, qua mạng): **0 lỗi** |

⚠️ Hai lưới FBX này để **Read/Write TẮT**, nên trong Play `mesh.vertices` trả về mảng rỗng — lần chạy đầu đo ra
"−1 cụm" và báo oan ba lỗi. Phép đo hình lưới vì thế chạy **ở Editor, trước khi vào Play**
(`ThuQuaCauDien.DoHinhLuoiTruocKhiChay`): bật Read/Write tạm, đo, rồi trả lại đúng giá trị cũ.

**Và một lần Unity sập thật.** Giữa lúc chạy menu 74, Unity của anh tắt hẳn. Không có crash dump; dấu vết nằm ở
cuối `Logs/Editor.log`:

> Failed to present D3D11 swapchain due to device reset/removed … This is an unrecoverable error and the
> editor will shut down.

Dòng ngay trước nó: `[GameDirector] Dot 1: 1 nguoi x 4 con + 0 con bat ki + 20 con xa`. Phép thử chạy lâu (riêng
mục G bắn **400 tia sét thật** để đo tỉ lệ choáng), và giữa chừng `GameDirector` đếm đủ 30 giây rồi thả **24 con
quái** vào đúng lúc đang vẽ hàng trăm tia — Windows phát hiện GPU timeout, reset driver, Unity buộc phải tắt.
Phép thử tự dựng bia riêng nên **không cần con quái nào**: nay menu 74 tắt `GameDirector` suốt phép thử rồi trả
lại. Vừa cứu GPU vừa làm số đo sạch hơn (quái thật cũng ăn tia và cũng bị nhắm).

### Kỹ năng mới: Quả cầu điện — quả cầu lơ lửng bắn tia vào mọi kẻ quanh nó (18/09/2026)

Anh gửi hai ảnh Diablo III và xin kỹ năng **"Quả cầu điện"**: dựng quả cầu **bằng Blender MCP** như trong ảnh,
kèm hiệu ứng **bắn các tia điện ra xung quanh**; hồi chiêu **5 giây**; quả cầu hiện ra **ngay gần đối thủ**;
tầm đánh **bằng Thiên thạch**; mỗi lần phóng **5 tia** tới quái và người chơi khác ở gần, **tối đa 10 lần**, cách
nhau **0,4 giây**; mỗi tia sát thương bằng **Giựt sét ở cấp 5** và **30% gây choáng**.
Tôi hỏi trước, anh chốt: "Giựt điện" là **Giựt sét** → mỗi tia **155,5** (75 × 1,2⁴); quả cầu **bám kẻ địch gần chỗ
ngắm nhất**, bắn trong **9 m**; **55 năng lượng**, choáng **1,5 giây**; mỗi lượt **mỗi kẻ một tia**, dư thì bỏ.

**Hình dựng trong Blender của anh qua MCP** (`CongCu/Blender/qua_cau_dien.blend` → `Resources/KyNang/QuaCauDien/`):
`CauDien.fbx` gồm **LoiCauDien** (khối cầu gồ ghề, đỉnh bị nhiễu noise) và **VoDienCauDien** (5 cung điện cuốn quanh
+ 14 gai điện toé ra); ba ảnh vẽ bằng numpy trong Blender: `HaoQuangDien.png` (vầng sáng có vân điện xoè),
`TiaDien.png` (sợi tia lượn sóng), `HatDien.png` (đốm bốn cánh). Icon `Resources/Icons/CauDien.png` render từ chính
cảnh ấy. Lúc chạy `VfxFactory.QuaCauDienHinh` cho lõi và vỏ **quay ngược chiều nhau** (55 và −120 °/s) nên nhìn như
điện đang cuộn, thêm hào quang billboard, hạt điện bay quanh và một ngọn đèn xanh nhấp nháy.

**Một lỗi thật tìm ra nhờ kỹ năng này:** đo choáng ra **2,00 giây** trong khi phải là 1,5. `StunnedEffect.remaining`
khai báo sẵn **2 giây**, mà `Apply` lấy `Max(remaining, seconds)` ngay từ lần đầu — nên **mọi cú choáng ngắn hơn 2
giây đều bị kéo dài thành 2**, kể cả **Giựt sét (1,5 s)** đã có từ trước. Sửa: component **mới** nhận đúng số giây
được giao, chỉ khi đã dính sẵn một lần choáng mới lấy lần dài hơn (cùng cái bẫy `AddComponent` mang giá trị mặc định
đã gặp ở `FrozenEffect`).

**Số đo** (menu 74, `quacaudien.txt`, **0 lỗi**; ảnh `quacaudien_1_cau.png`, `quacaudien_2_ban_tia.png`):

| Đo | Kết quả |
|---|---|
| Thông số | số hiệu 13, 14 kỹ năng; 55 năng lượng (trừ đúng 55), hồi chiêu 5 s, niệm 0,62; tầm ngắm 18 m |
| Tài nguyên Blender | lõi cầu khung bao 2,00×2,00×2,03; vỏ điện 2,92×2,96×2,81; ba ảnh 256×256; icon 256×256 |
| Chỗ đặt | có kẻ địch cách chỗ ngắm 3,5 m → quả cầu cách **kẻ địch 1,40 m**, cách chỗ ngắm 2,8–3,7 m; ĐỐI CHỨNG không có ai → cách chỗ ngắm **0,00 m** |
| Nhịp bắn | **10 lượt**, từ lượt đầu đến lượt cuối **3,71–3,80 giây** (9 × 0,4 = 3,6) |
| Chia tia | 7 bia trong 9 m → **50 tia** (10 × 5); đúng **5 kẻ gần nhất** ăn mỗi lượt một tia, kẻ thứ 6 và kẻ ngoài 9 m **0 đòn** |
| Sát thương | **155,52 mỗi tia** = Giựt sét cấp 5 (75 × 1,2⁴), đọc thẳng từ hằng của Giựt sét chứ không chép tay |
| Choáng | 400 tia thật → **30,0%** (120 lần), thời gian choáng **1,50 giây** |
| Qua mạng | gói phép số 13 từ máy kia → máy này phát lại ra quả cầu; mình tung → gói gửi đi mang số 13 |
| Hồi quy | menu 69 (Giựt sét, cùng đường choáng), 61 (kinh nghiệm — nay gồm kỹ năng 13), 59 (Sách phép 14 kỹ năng): **0 lỗi** |

**Anh xem xong bản đầu và xin sửa hai điều (cùng ngày):**

1. **Chưa bắn đủ 10 lượt thì quả cầu phải đứng nguyên chỗ đó**, đừng tự biến mất. Bản đầu cho đồng hồ chạy đều
   0,4 giây một lượt **bất kể có bắn được hay không**, nên thả quả cầu ở chỗ vắng là nó tan sau 4 giây mà **chưa đánh
   lần nào**. Nay chỉ lượt **thật sự bắn ra tia** mới được tính; không có ai trong 9 m thì quả cầu ngó lại sau mỗi
   0,15 giây và cứ đứng đó. Để không có quả cầu nằm lại mãi trên bản đồ, anh chốt hạn chờ **tối đa 20 giây**.
2. **Vẽ lại cho giống ảnh hơn.** Dựng lại trong Blender của anh qua MCP: lõi cầu **tối màu** (xanh sẫm) thay vì đốm
   sáng, thêm hẳn một **vành sáng** bọc ngoài (vỏ cầu mỏng), cung điện dày và dài hơn, và **10 tia toé ra ngoài**.
   Ảnh hào quang vẽ lại thành **vòng khuyên sáng** (tâm mờ, rìa rực) chứ không phải vầng sáng đặc. Trong game thêm
   `ToeTiaDien`: cứ ~0,13 giây phóng 2 tia điện ngắn ra chung quanh, **chúc xuống đất** như trong ảnh — tia này chỉ
   để nhìn, không gây sát thương. Ba lớp (lõi, vành, vỏ điện) quay ngược chiều nhau.

**Anh xem tiếp trong game rồi chốt lần ba:** bỏ hẳn **tia sét nhỏ bắn ra liên tục** (đẹp trong ảnh tĩnh nhưng rối khi
chơi); **giữ các đường viền trắng lớn vòng quanh và vẫn để chúng nằm NGOÀI mặt cầu** — thứ anh khoanh đỏ chê "răng cưa
rất xấu" là mấy **gai thẳng ngắn chĩa ra** của bản đầu, không phải các đường viền; và quả cầu tồn tại **30 giây** thay
vì 20. Nay vỏ điện là **năm đường viền lớn** (bán kính 1,20–1,31 so với mặt cầu 1,00), uốn mềm, không còn gai nào.

**Số đo của lần sửa** (menu 74 mục I, **0 lỗi**): thả quả cầu ở chỗ không có kẻ địch → sau **6 giây** (bản cũ đã tan
từ giây thứ 4) quả cầu **vẫn còn**, đã bắn **0 lượt**, xê dịch khỏi chỗ đặt **0,00 m**; hình có vành sáng, viền trắng
xa tâm **1,243** (vành sáng 1,100 → viền đúng là nằm ngoài như ảnh anh gửi), và **0 khung hình** có tia sét quanh quả
cầu khi không có kẻ địch (đã bỏ hẳn tia bắn liên tục). Đặt một bia cạnh nó → bắn **đủ 10 lượt** rồi mới tan.

Ba lần phép thử tự báo oan, đều là lỗi của **kịch bản đo** chứ không phải của kỹ năng — và đáng ghi lại vì cùng một
kiểu: (1) đoán "5 bia gần" theo **thứ tự chỉ số** trong khi đặt bia theo vòng tròn nên thứ tự thật khác → nay sắp
theo khoảng cách thật rồi mới kết luận; (2) đọc `transform` của quả cầu **sau khi nó tự tan** → `MissingReference`
làm coroutine chết, Play mode kẹt lại; (3) trong menu 61, thêm kỹ năng vào danh sách thử mà **quên cấp thêm một
điểm kỹ năng**, nên `MoKhoa` thất bại và `CastAt` từ chối im lặng — báo thành "phép không giết được quái".

### Tàng hình: nhấp nháy hai giây cuối, và đòn đầu nhân đôi TOÀN BỘ sát thương của kỹ năng (18/09/2026)

Anh xin thêm hai điều cho **Tàng hình**:

1. **Sắp hết tàng hình thì báo trước**: hai giây cuối, nhân vật **nhấp nháy liên tục** — và **chỉ mình người điều khiển
   thấy**, người chơi khác không thấy dấu hiệu này.
2. **Đòn đầu tiên phải nhân đôi TẤT CẢ sát thương của kỹ năng ấy**, không phải chỉ cú trúng đầu: tung Mưa băng thì
   **mọi vệt băng rơi xuống** đều 200%, tung Quả cầu băng thì **mỗi quả trong chùm** đều 200%.

**Nhấp nháy.** `TangHinh.Update` đã tự tính "độ hiện" mỗi khung (`_Amount` của shader). Nay khi còn ≤ 2 giây
(`GiayNhapNhay`), độ hiện đổi qua lại giữa **1,00 và 0,12** theo nhịp **0,22 giây** — nhấp nháy bằng độ sáng chứ không
tắt hẳn renderer (tắt renderer mỗi 0,1 giây vừa tốn vừa giật). Điều kiện `laCuaMinh && !tuMang` chặn hẳn bản sao của
người khác: bản sao **không đếm được giờ thật** (mỗi gói tin lại đẩy `conLai` lên), nên nếu không chặn thì nó sẽ nhấp
nháy suốt — và anh muốn chỉ mình mình thấy.

**Đòn đầu ×2.** Chỗ nhân đôi vốn nằm trong `Release` và nhân vào `manhHon` — hệ số sát thương của **cả kỹ năng**, nó
đi vào từng quả cầu, từng vệt băng, từng nhịp cháy. Đo lại thì phần này **đã đúng sẵn**: mỗi vệt Mưa băng ghi 29,04 →
**58,08**, mỗi quả trong chùm năm quả đều gấp đôi. Nhưng có **một lỗ thật, chỉ lộ ra khi chơi mạng**:

> Gói tin "tôi vừa tung phép" bay đi từ lúc **BẮT ĐẦU NIỆM**, còn việc nhân đôi lại quyết định ở **lúc phép bay ra**.
> Máy bên kia phát lại phép ấy để tính trúng, mà trong gói **không có một bit nào** nói "đòn này gấp đôi" — nên máy
> mình tính 249 còn máy họ tính 124,5. Hai máy kể hai chuyện khác nhau, đúng cái bẫy mà `capKyNang` đã từng vá.

**Sửa:** quyết định "đòn này là đòn đầu của Tàng hình" ngay trong `BeginCast` (lúc bắt đầu niệm), giữ trong
`PlayerController.donTangHinhDangTung`, và **gửi kèm gói phép**: cờ nằm ở **bit cao của byte cấp kỹ năng**
(cấp chỉ 1..5 nên bảy bit dưới thừa chỗ — không phải nới gói 17 byte). Bên nhận đọc `donTangHinh` rồi truyền vào
`TungPhepTheoMang`. Chiêu bị **ngắt giữa chừng** thì trả lại quyền đòn đầu cho Tàng hình (đòn chưa bay ra thì chưa tiêu).

**Số đo** (menu 73, `tanghinh.txt`, **0 lỗi**; ảnh `tanghinh_3_nhapnhay_sang.png` / `tanghinh_4_nhapnhay_mo.png` là hai pha
của cùng một nhịp):

| Đo | Kết quả |
|---|---|
| Nhấp nháy | lúc còn > 2 giây: độ hiện **1,00..1,00** (đứng yên, không nháy); trong 2 giây cuối: **0,12..1,00**, đổi sáng/tối **18 lần** trong 63 khung |
| Chỉ mình thấy | bản sao của người khác (ép `conLai` = 1 giây): **không nhấp nháy** |
| Mưa băng | sát thương ghi trên **từng vệt**: thường 33–35 vệt đều **29,04**; đang tàng hình 35 vệt đều **58,08** → **×2,00** (nếu chỉ vệt đầu ×2 thì các vệt sau vẫn 29,04) |
| Mưa băng, máu bia thật | 26,5/lần trúng → **48,6/lần** = ×1,84 (tổng không tròn 2,00 vì mỗi cơn bão số vệt **trúng** bia khác nhau và sát thương vùng giảm dần từ tâm ra rìa) |
| Quả cầu băng cấp 5 | 5 quả: **442,4 → 884,7** = **×2,00** (nếu chỉ quả đầu ×2 thì 1,20) |
| Qua mạng | gói phép viết–đọc lại: cấp 5 + cờ đòn đầu đều đúng; **người kia** tung Quả cầu lửa qua mạng: gói không cờ 137,3, gói có cờ **273,2** = ×1,99; mình đánh đòn đầu → gói gửi đi **có mang cờ** |
| Hồi quy | menu 37 (kỹ năng qua mạng), 63, 68, 71, 72, 61 — đều **0 lỗi** |

Hai phép thử cũ sai lộ ra nhân dịp này, đã sửa: menu 37 mục 7 tung Sấm sét mà **không mở khoá kỹ năng** nên CastAt từ
chối (báo "không có gói nào đi ra" — lỗi của phép thử, không phải của đường mạng); menu 72 còn kiểm `SoKyNang == 12`
trong khi nay là 13. Menu 68 mục L thì **chập chờn**: quái thật lang thang tới gần chỗ thử là trong 1,7 m có kẻ địch
sống nên cụm gai mọc lên — nay dọn quái quanh chỗ thử trước khi đo (cùng một cái bẫy với `TAM_` sót lại).

### Kỹ năng mới: Tàng hình — thân trong suốt, quái không thấy, đòn sau gấp đôi (18/09/2026)

Anh gửi ảnh Dark Templar và xin kỹ năng **"Tàng hình"**: thân người trong suốt như trong ảnh; quái **không thấy và không đánh**;
người chơi khác chỉ thấy **đường nét khi mình di chuyển**, mình đứng yên thì họ **không thấy gì**; **miễn toàn bộ hiệu ứng**;
đi **nhanh hơn 20%**; kéo **20 giây**; **đòn đầu tiên** bằng một kỹ năng mạnh thêm 200% và làm tan tàng hình; hồi chiêu **30 giây**.
Tôi hỏi trước, anh chọn: "tăng 200%" = **gấp 2**; chỉ **kỹ năng gây sát thương** mới tính (Khiên, bình không); miễn hiệu ứng nhưng
**vẫn ăn sát thương**; **30 năng lượng**, niệm 0,38; đứng yên **mờ dần 0,4 giây**; **chính mình vẫn thấy thân mình mờ mờ**;
quái đang đuổi thì **mất dấu ngay**; icon dựng bằng **Blender MCP**.

**Cách làm:**
- Số hiệu **12** (`CapDo.KyTangHinh`; `SoKyNang` 13), nối đủ các chỗ như những kỹ năng trước (HUD, sảnh, Sách phép, chỉ báo ngắm,
  tư thế niệm như Khiên, trạng thái `hoiTangHinh`).
- `Combat/TangHinh.cs`: **thay toàn bộ vật liệu** của model bằng shader mới `Diablo25D/TangHinh` (giữ bản gốc để trả lại) — khác
  `FrozenEffect` chỉ *phủ thêm* một lớp. Shader: gần như trong suốt, chỉ còn **viền fresnel** xanh lơ + vài vệt sáng trôi dọc thân.
  Shader đã thêm vào **Always Included** (bài học cũ: shader chỉ nạp lúc chạy thì bản build không có).
- Người khác thấy gì: component tự đo tốc độ của chính nó. Của mình → luôn hiện mờ; bản sao người khác → đang đi thì hiện đường nét,
  đứng yên thì `_Amount` về 0 trong 0,4 giây rồi **tắt hẳn renderer**.
- Miễn hiệu ứng: `TangHinh.ChanHieuUng` chặn ngay đầu `FrozenEffect.Apply/ApCham`, `StunnedEffect.Apply`, `BiDanhNga.Apply`,
  `BiHatTung.Apply`, `BurningEffect.Apply`; lúc bật thì xoá sạch hiệu ứng đang dính.
- Quái không thấy: `GameDirector.GanNhat` bỏ qua người đang tàng hình, và `EnemyAI` chọn lại mục tiêu ngay khi mục tiêu tàng hình.
- Đòn đầu: trong `Release`, nếu đang tàng hình và kỹ năng **gây sát thương** (`PlayerController.KyGaySatThuong`) thì nhân đôi hệ số
  sát thương rồi tắt tàng hình.
- Qua mạng: bit thứ sáu `CoTangHinh` (1<<5) — **mặt nạ byte cờ nới lên 0x3F** ở cả gói người chơi (dùng hết 8 bit: 2 + 6) và gói quái.

**Hai lỗi thật, tìm ra nhờ đo:**
1. Bật lại tàng hình ngay sau khi vừa tắt thì `GetComponent` trả về component **đang chờ huỷ** — đặt thời gian cho nó vô ích.
2. Component mới khai báo sẵn `conLai = 20 s`, mà `Bat` lấy `Max(conLai, giây)` → đặt 1,2 giây không ăn gì (đo ra "tan sau 4 giây").
   Sửa: component mới đặt đúng số giây, component cũ mới lấy Max.

**Số đo** (menu 73, `tanghinh.txt`, **0 lỗi**; ảnh `tanghinh_1_dang_tang_hinh.png`, `tanghinh_2_quai_mat_dau.png`):

| Đo | Kết quả |
|---|---|
| Thông số | số hiệu 12, 13 kỹ năng; 30 năng lượng (trừ đúng 30), hồi chiêu 30 s, niệm 0,38; trạng thái 20 s |
| Hình | trước khi bật: 0 renderer dùng shader tàng hình, sau khi bật: có; tắt thì trả lại vật liệu cũ |
| Miễn hiệu ứng | áp 6 hiệu ứng lúc đang tàng hình → **dính 0**; ĐỐI CHỨNG lúc không tàng hình → dính 3/3; bật lúc đang dính 3 hiệu ứng → còn 0; **vẫn mất 100 máu** khi bị đánh |
| Quái | 24 quái thật: trước khi tàng hình **24/24** nhắm mình, sau khi tàng hình **0**; `GanNhat` trả null |
| Tốc độ | đo quãng đường thật: 6,24 m/s so với 5,20 m/s → **×1,200** |
| Đòn đầu | một chùm Quả cầu lửa: thường 124,5 máu, lúc tàng hình **249,0** → ×2,00; sau đòn đó tàng hình **tan**; tung Khiên thì **không** tan |
| Hết giờ | đặt 1,2 giây → tự tan sau 1,23 giây |
| Qua mạng | gói trạng thái mang bit `CoTangHinh` (byte cờ = 32); bản sao nhận bit: đứng yên **0 renderer hiện**, di chuyển thì hiện lại |
| Hồi quy | menu 63 (đánh ngã qua mạng, cùng byte cờ) và menu 59 (Sách phép, nay 13 kỹ năng): 0 lỗi |

Một chỗ suýt báo oan: đối chứng "không tàng hình thì dính đủ" lúc đầu ra 2/3 — vì `BurningEffect` **làm tan băng** (luật cũ của game),
áp cùng lúc thì đóng băng bị xoá ngay. Đo tách làm hai đợt thì ra 3/3.

### Lửa địa ngục năm quả màu như Quả cầu lửa; Quả cầu băng đóng băng 40%, cấp 5 năm quả (18/09/2026)

Anh xin bốn việc: **Lửa địa ngục** thêm một quả (thành 5) và **màu quả + vụ nổ giống hệt Quả cầu lửa**; **Quả cầu băng** mỗi quả **40%
đóng băng** đối thủ (không đi, không dùng kỹ năng) và **cấp 5 ra 5 quả**. Tôi hỏi trước, anh chọn: trúng vẫn **làm chậm như cũ** rồi cộng
thêm 40% đóng băng; đóng băng **1,5 giây** (như Mưa băng, +0,15 s mỗi cấp kỹ năng); cấp 5 **không tốn thêm** năng lượng; Lửa địa ngục
**31 năng lượng** (25 × 5/4) và chia cho tối đa 5 mục tiêu.

**Sửa:**
- `LuaDiaNguc.SoQua` 4 → 5, `PlayerController.luaDiaNgucCost` 25 → 31; bỏ hẳn lớp nhuộm đỏ sẫm (`VfxLuaDiaNguc.cs`, `XoaVatLieuRieng.cs`
  xoá; `FireExplosion` vẫn trả về vật nổ). Icon vẫn nền đỏ sẫm để phân biệt nút.
- `QuaCauBang`: `XacSuatCham` 0,40 → **1** (trúng là chậm), thêm `XacSuatDongBang` 0,40 và `GiayDongBang` 1,5 (cộng theo cấp như lớp chậm);
  `NoBang` nhận thêm tham số thời gian đóng băng. Cấp 5: `SoQuaTheoCap` → 5 quả (cấp của **người tung**, đi kèm gói tin).
- Đóng băng dùng đúng `FrozenEffect.Apply` của Mưa băng: bản sao mạng không tự gieo, trạng thái sang máy kia bằng bit `CoBangHoanToan`.

**Bẫy cũ vấp lại:** menu 72 báo "năng lượng không phải 31" dù code ghi 31 — **prefab `Player_Sorceress` lưu sẵn 25** và đè lên code (đúng
cái bẫy đã ghi trong bộ nhớ). Sửa giá trị trong prefab rồi mới đúng.

**Lỗi thật thứ hai:** 1/5 quả Lửa địa ngục vẫn đâm đất khi vòng ra sau. Trước đó chỉ đo mặt đất **phía mục tiêu**; lúc quẹo gấp, hướng bay
và hướng mục tiêu khác hẳn nhau. Nay đo **ba chỗ** (dưới quả, 1,5 m theo hướng bay, 1,5 m về phía mục tiêu) và giữ cao 1,3 m.

**Số đo** (menu 72 và 68, **0 lỗi** cả hai):

| Đo | Kết quả |
|---|---|
| Lửa địa ngục | 5 quả mỗi lần tung, tốn đúng 31; 5 bia quanh người (ngoài hình quạt) → mỗi quả một bia, **5/5 bia trúng**, bia 25 m không bị gì (đối chứng chùm Quả cầu lửa thường: 0 bia) |
| Màu | tỉ lệ xanh lá/đỏ của hạt: Quả cầu lửa 1,000, Lửa địa ngục **1,000** (×1,000 — giống hệt) |
| Còn lại của Lửa địa ngục | một bia → 5/5 quả; bia chạy ngang 5 m/s mất 758; mục tiêu chết giữa đường → 5/5 quả sang bia khác; không có ai → 5 quả thẳng, lệch 18°; sát thương một quả = Quả cầu lửa cấp 5 (×1,02); qua mạng 5 quả |
| Quả cầu băng, 1000 lần nổ | làm chậm **100,0%**, đóng băng **40,7%** (mong 40%), chậm 0,5 trong 2,00 s, đóng cứng **1,50 s**; cấp 3: chậm 2,30 s, đóng băng 1,80 s |
| Cấp 5 | cấp 4 (đối chứng) 3 quả, cấp 5 **5 quả** |
| Người chơi bị đóng băng | hệ số tốc độ 1,00 → **0,00**, bị khoá cứng; bấm Quả cầu băng lúc đó ra **0** quả, hiện "BẠN ĐANG BỊ ĐÓNG BĂNG!" |

### Kỹ năng mới: Lửa địa ngục — bốn quả cầu lửa đỏ sẫm tự đuổi kẻ địch (17/09/2026)

> ⚠️ **Đã đổi ngày 18/09/2026** (mục trên): 5 quả, 31 năng lượng, màu giống hệt Quả cầu lửa (bỏ lớp nhuộm đỏ sẫm).

Anh xin: kỹ năng **"Lửa địa ngục"** — sát thương ban đầu bằng **Quả cầu lửa cấp 5**, hồi chiêu **0,5 s**; phóng **4 quả cầu lửa** giống Quả cầu
lửa nhưng **tự dí đúng vào** người chơi / quái ở gần; vẫn **thiêu đốt**. Tôi hỏi trước, anh chọn:
- cấp 1 = **176** (85 × 1,2⁴), vẫn +20% mỗi cấp; **25 năng lượng**, niệm **0,38 s**;
- chia 4 quả cho tối đa **4 kẻ gần người tung nhất trong 20 m**; ít hơn 4 thì quả dư nhắm kẻ gần nhất; mục tiêu chết giữa đường thì đổi sang kẻ
  còn sống gần quả nhất; **bám chắc** (quẹo ~360°/s); không có ai thì bay thẳng;
- hình **lửa đỏ sẫm địa ngục**; **toả quạt rồi uốn cong**; icon dựng bằng **Blender MCP**.

**Code:**
- Số hiệu **11** (`CapDo.KyLuaDiaNguc`, thêm ở cuối; `SoKyNang` 12). Nối đủ các chỗ như Gió lốc: `PlayerController` (năng lượng / hồi chiêu /
  niệm, `CastAt`, `Release`, `TungPhepTheoMang`, `ThoiGianNiem`, nguyên tố lửa, trạng thái `hoiLuaDiaNguc`), HUD, sảnh, Sách phép, chỉ báo
  ngắm (đỏ sẫm), tư thế niệm (như Quả cầu lửa).
- `Skills/LuaDiaNguc.cs`: `SatThuongGoc` đọc **impactDamage của prefab** Quả cầu lửa × 1,2⁴ (prefab đè code); `TimGanNhat`; `SpawnChum` toả
  4 quả 18° và gán mục tiêu.
- Đường bay / va chạm / nổ / thiêu đốt **dùng lại `Fireball`**, chỉ thêm phần dí (`tocQueo`, `mucTieu`, `DiMucTieu`): bay thẳng 0,1 s cho thấy
  hình quạt rồi quẹo ≤ 360°/s (trong 4 m quẹo gấp đôi để không quay vòng quanh mục tiêu).
- Hình: `VfxFactory.NhuomLuaDiaNguc` nhân màu (1; 0,28; 0,16) vào hạt, vật liệu (bản sao riêng, xoá khi huỷ — `XoaVatLieuRieng`), vệt,
  đèn của chính quả cầu lửa và vụ nổ (`FireExplosion` nay trả về vật nổ).
- Icon `Resources/Icons/LuaDiaNguc.png` từ `CongCu/Blender/lua_dia_nguc.blend`: bốn quả lửa đỏ sẫm từ bốn phía uốn vào một điểm sáng. Lần
  render đầu 4 quả dồn cục và màu cam — ảnh PNG sRGB làm màu tuyến tính sáng lên, phải hạ kênh xanh lá rất thấp mới ra đỏ sẫm; Glare
  compositor không ăn nên thay bằng đĩa hào quang.

**Lỗi thật tìm ra khi đo:** lần chạy đầu phần lớn quả **không trúng** — chẩn đoán in chỗ nổ: cách mặt đất 0,04–0,5 m. Quả bay ngang ở độ cao
lúc phóng, uốn cong trên đất Act2 gồ ghề thì **đâm xuống đất**. Sửa: khi còn xa (> 2,5 m ngang) quả giữ cao **≥ 1,2 m trên mặt đất phía trước
1,5 m**, tới gần mới hạ vào ngực mục tiêu. Sau đó vẫn ~1/4 quả nổ sớm — vào **lò lửa giữa map** cạnh chỗ đứng: đo cả Act2 **không có chỗ trống
bán kính 11 m** nào (bia, đá, cây dày đặc). Đó là luật cũ của Quả cầu lửa (đâm vật cản thì nổ), nên phép thử tạm **tắt 143 va chạm đồ vật
trong 26 m** (chỉ trong Play) để đo riêng khả năng dí. Trong trận thật, quả lửa vòng cung rộng (bán kính quẹo ~2,7 m) giữa bãi mộ sẽ có quả
nổ vào vật cản.

**Số đo** (menu 72, `luadianguc.txt`, **0 lỗi**; ảnh `luadianguc_1_uon_cong.png`):

| Đo | Kết quả |
|---|---|
| Thông số | số hiệu 11, 12 kỹ năng; 25 năng lượng, hồi chiêu 0,5 (bấm mỗi khung: nhận lại sau 0,52 s), niệm 0,38; sát thương gốc **176,26** = prefab 85 × 1,2⁴ |
| Tung thật | khoá → từ chối; mở → **4 quả**, trừ 25 |
| 4 bia ngoài hình quạt (±70°, ±150°, 9–12 m) + bia 25 m phía sau | 4 quả gán **4 mục tiêu khác nhau**, mỗi bia trúng 1 quả, 4/4 bia mất máu, bia 25 m mất 0; ĐỐI CHỨNG chùm Quả cầu lửa thường cùng chỗ: **0** bia trúng |
| Một bia | 4/4 quả gán vào nó, 4/4 trúng |
| Bia chạy ngang 5 m/s | mất 593 máu (4 quả) |
| Mục tiêu bị giết sau 0,15 s | 4/4 quả chuyển sang bia còn lại, bia đó mất 612 |
| Không có ai | 4 quả, hướng đổi 0,00° sau 0,4 s; lệch nhau đúng 18,0° |
| Sát thương | một quả Lửa địa ngục cấp 1 mất 142,35 / một quả Quả cầu lửa **cấp 5 thật** cùng chỗ 137,09 → ×1,038 (cả hai giảm theo khoảng cách nổ); bia bị thiêu đốt |
| Màu | tỉ lệ xanh lá/đỏ của hạt: thường 1,000, địa ngục **0,280**; nhuộm 4 quả + 4 vụ nổ |
| Qua mạng | gói kỹ năng 11 của người kia → máy mình ra 4 quả; mình tung → gói mang số 11; giết bia → kẻ đánh cuối là người tung |
| Menu 61 | Lửa địa ngục giết quái → +18 kinh nghiệm đúng giá; menu 70 (Quả cầu lửa) vẫn 0 lỗi sau khi sửa `Fireball` |

### Gió lốc: hai tia sét cách nhau 1,2 m; cấp 5 phóng hai lốc song song (17/09/2026)

Anh báo: hai tia sét trong lốc **gần sát nhau quá** — cho xa ra một chút nhưng vẫn trong lốc và bám lốc; và **nâng lên cấp 5 thì đánh ra
2 lốc cùng lúc, cách nhau một khoảng, song song**. Tôi hỏi trước, anh chọn: tia cách nhau **~1,2 m**; hai lốc cách **4 m**; kẻ đứng giữa
bị cả hai quét **trúng hai lần**; cấp 5 **tốn gấp đôi năng lượng**.

**Hai tia sét** (`VfxFactory.GioLocSetTrongLoc`): hai tia ở hai phía **đối diện** trục, cùng góc xoắn nên luôn đối diện. Mỗi đầu tia lệch trục
`LechTiaSetGioLoc` = 0,6 m nhưng không quá 0,85 × bán kính **vỏ trong cùng** ở đúng độ cao đó (`BanKinhVoTrongGioLoc`: bảng bán kính Vo0 đo
trên lưới mỗi 0,5 m × 1,1). Đo trên lưới: chân lốc vỏ trong chỉ 0,46 m, gần miệng 1,9 m — nên đỉnh hai tia cách 1,2 m, còn đoạn dưới tự thu
vào (~0,85 m) để không lòi ra ngoài.

**Cấp 5** (`GioLoc.SoLocTheoCap`, `SpawnSongSong`): ≥ `CapHaiLoc` (5) thì 2 lốc cùng hướng, xếp ngang (vuông góc hướng bay) cách
`KhoangCachHaiLoc` 4 m; năng lượng nhân `HeSoNangLuongTheoCap` (×2) sau hệ số cấp chung — Sách phép hiển thị đúng hệ số ×2,93 ở cấp 5.
Số lốc lấy theo cấp **của người tung** (`capPhep`, đi kèm gói kỹ năng) nên máy nào cũng ra 2 lốc. Mô tả Sách phép thêm câu cấp 5.

**Số đo** (menu 71, `gioloc.txt`, **0 lỗi**; ảnh `gioloc_4_cap5_hai_loc.png`, `gioloc_can_1.png`):

| Đo | Kết quả |
|---|---|
| Khoảng cách đỉnh hai tia (6 nhịp) | nhỏ nhất **1,20 m**, trung bình 1,20 m; đuôi 0,83–0,88 m |
| Nằm trong lốc | 12/12 đầu tia nằm trong vỏ **trong cùng** — bán kính vỏ đọc thẳng từ lưới FBX ngoài Play, không dùng bảng trong code; sát vỏ nhất còn cách 0,06 m |
| Bám lốc | độ trôi của tia so với lốc suốt đời **0,000 m** (12 tia, mọi khung); ĐỐI CHỨNG tia không bám: bị bỏ lại 2,74 m |
| Cấp 4 (đối chứng) | 1 lốc, tốn **26,62** (20 × 1,1³) |
| Cấp 5 | **2 lốc**, tốn **58,56** (20 × 1,1⁴ × 2); tâm cách nhau **4,00 m** lúc sinh và sau 1 s; lệch theo hướng bay 0,000 m; cùng hướng (cos 1,0000) |
| Bia đứng giữa hai đường bay | mất 312 = trúng cả hai lốc (mỗi lốc 75 × 1,2⁴ = 155,52) |
| Qua mạng | gói kỹ năng cấp 5 của người kia → máy mình ra **2** lốc (gói cấp 1 → 1) |

Hai lần báo lỗi dọc đường, đều không phải lỗi game: (1) bia giữa mất 312 thay vì 311,04 — bia thử có **10 000 000 máu**, số thực `float`
ở cỡ đó chỉ chính xác tới 1 đơn vị nên mỗi cú 155,52 thành 156; (2) một lần đo "đầu tia cách trục" ra 0,82 m (> 0,6 m đặt) — không lặp
lại ở hai lần chạy sau; đổi sang đo **độ trôi** của từng tia so với chính chỗ của nó lúc sinh (kiểm thẳng "bám theo", không phụ thuộc bán kính
đặt): 0,000 m cả hai lần. Nguyên nhân lần 0,82 m tôi chưa xác định được.

### Gió lốc: hai tia sét bị bỏ lại phía sau lốc — cho tia bám theo lốc (17/09/2026)

Anh báo: 2 tia sét **luôn bị bỏ lại phía sau** cơn lốc.

**Nguyên nhân:** `LightningArc` ghim hai đầu tia ở **toạ độ thế giới** lúc sinh. Tia sống 0,13–0,28 s, lốc bay 9,5 m/s → lốc đi mất
**1,2–2,7 m** trước khi tia tắt. Phép thử hôm trước chỉ đo tia **lúc vừa sinh** (lệch trục ≤ 0,31 m) nên không thấy.

**Sửa:** `LightningArc.BamTheo(Transform)` — nhớ khoảng lệch hai đầu so với vật, mỗi `LateUpdate` dời `start`/`end` và transform theo vật
(lưới tia dựng trong không gian cục bộ của transform nên cả tia dời theo; lần giật hình sau vẫn đúng chỗ). `GioLocSetTrongLoc` nhận
transform của lốc và gọi `BamTheo`. Các tia khác (Sấm sét, Lốc xoáy, Giựt sét…) không gọi nên không đổi.

**Số đo** (menu 71, `gioloc.txt`, **0 lỗi**; ảnh `gioloc_can_1.png`): đo **mọi khung** suốt đời mỗi tia, ở **cuối khung** —

| Đo | Kết quả |
|---|---|
| Tâm hình vẽ thật (bounds lưới lõi) cách trục lốc | lớn nhất **0,45 m** |
| Hai đầu tia cách trục lốc | lớn nhất **0,35 m** (đúng bán kính đặt khi sinh) |
| ĐỐI CHỨNG: tia cùng kiểu không bám theo, sinh cùng lúc | bị bỏ lại **2,38 m** |
| Phần còn lại | 6 nhịp / 12 tia, cỡ tia ×0,325, bề ngang ×1,100, 9,50 m/s — như lần trước |

Lần đo đầu ra lệch 0,68 m và báo lỗi — **do phép đo**: coroutine `yield return null` chạy **giữa Update và LateUpdate**, lúc lốc đã đi
bước mới mà tia chưa kịp bám (9,5 m/s × dt ≈ 0,35–0,5 m). Đổi sang `WaitForEndOfFrame` (đo đúng lúc đã vẽ) thì ra 0,35 m.

### Gió lốc: rộng thêm 10%, bay 9,5 m/s, bỏ sét khi trúng, hai tia sét luôn đánh trong lòng lốc (17/09/2026)

Anh báo: toàn bộ bán kính lốc **to thêm 10%**; **bỏ** hiệu ứng tia sét khi lốc trúng quái / người chơi khác; tốc độ **9,5**; **luôn có
2 tia sét trong lốc**, đánh từ đỉnh xuống giống Lốc xoáy nhưng **cỡ tia phải hợp với lốc nhỏ**. Tôi hỏi trước, anh chọn: 10% gồm **cả
hình lẫn vùng trúng**; bỏ **hết** gói hiệu ứng khi trúng (tia sét, chớp, cháy sém bốc khói); hai tia **theo nhịp 0,45 s như Lốc xoáy, chỉ hình**.

**Sửa:**
- `VfxFactory.HeSoBanKinhGioLoc = 1,1`: bốn lớp lưới nhân 1,1 theo x/z (chiều cao giữ 5 m), vòng bụi chân 0,45 → 0,495 m, vệt bụi và hạt đất
  nhân theo. `CoLaiRoiTat` (co lốc lúc tan) giờ co từ cỡ gốc của từng lớp — trước nó gán thẳng `(k,k,k)` sẽ xoá mất hệ số 1,1.
- `GioLoc.BanKinhTrung` 2,2 → **2,42**; `GioLoc.TocDo` 8 → **9,5**.
- Bỏ `VfxFactory.GioLocGiatSet` (tia từ thân lốc sang đối thủ + chớp Sấm sét + cháy sém).
- `VfxFactory.GioLocSetTrongLoc` (gọi mỗi `GioLoc.NhipSetTrongLoc` = 0,45 s): chép cách dựng hai tia của `TornadoBolt` (độ dày 0,85 / 0,70,
  số đoạn, độ giật, nhánh, thời gian sống) nhưng **cả hai tia từ gần đỉnh (86–96% chiều cao) đánh xuống** gần trục lốc, đuôi xuống thấp
  0,3–2,2 m; **bề dày nhân 5 / 15,37** (chiều cao Gió lốc / Lốc xoáy đo được). Độ giật và nhánh tính theo tỉ lệ độ dài tia nên tự nhỏ theo.
- Sách phép: bỏ câu cháy sém, thêm "Trong lòng lốc luôn lóe hai tia sét đánh từ đỉnh xuống".

**Số đo** (menu 71, `gioloc.txt`, **0 lỗi**; ảnh `gioloc_can_1.png` thấy hai tia trong lốc):

| Đo | Kết quả |
|---|---|
| Bề ngang thật vỏ Vo1 / lưới | **×1,100**, chiều cao ×1,000; vòng phun bụi 0,491 m |
| Vùng trúng | bia lệch 2,72 m mất 75, 2,92 m mất 0 (với 2,2 cũ thì 2,72 m đã trượt) |
| Tốc độ | **9,50 m/s**; ngừng đi sau 4,54 s |
| Tia sét trong lốc | 6 nhịp → **12 tia**, 6/6 nhịp đúng 2 tia; đầu tia cao ≥ 4,32 m, đuôi thấp hơn đầu ≥ 2 m, lệch khỏi trục ≤ 0,31 m |
| Cỡ tia | lõi 0,0608 m / tia THẬT của Lốc xoáy đo cùng lúc 0,1870 m = **×0,325**; tỉ lệ chiều cao đo được ×0,325 |
| Khi trúng 5 bia | tia từ thân lốc sang bia **0**, chớp **0**, cháy sém **0**; mỗi bia mất đúng 75 |
| Phần khác | 1 lốc, 20 năng lượng, hồi chiêu 0,4; hất tung 60,0% (190 lần), khiên chặn; ngắt chiêu; qua mạng; lò lửa; không trèo mái nhà — như cũ |

Phép thử sửa 2 lần, đều do chính nó: tỉ lệ bề ngang lấy từ `Renderer.bounds` lúc vỏ đang quay ra **×1,319** (hộp trục thế giới bao hộp cục
bộ đã xoay — cùng cái bẫy với phép đo quả cầu băng hôm nay) → đo lúc vỏ ở góc 0; đếm tia từ lúc sinh lốc thì cộng cả tia đã tắt trong lúc
chờ → chụp lại mốc đếm ngay trước vòng đếm.

### Mưa băng: đầu vệt nhọn răng cưa hướng xuống; chỉ mọc băng khi trúng quái / người chơi khác (17/09/2026)

Anh gửi ảnh khoanh đầu vệt: cho **hơi nhọn và có răng cưa một chút** như sao băng thật; và **chỉ tạo băng trên mặt đất khi đánh trúng
người chơi khác hoặc quái vật**. Tôi hỏi trước, anh chọn: **mũi nhọn chĩa xuống đất** (hướng rơi); rơi trúng đồ vật mà không có ai thì
**không mọc băng**; các phần nổ khác (chớp, vòng lạnh, sương, mảnh băng, vết sương giá) **giữ như cũ**.

**Đầu vệt** — vẽ lại `DauSaoBang.png` trong Blender (128×256, `CongCu/Blender/vet_sao_bang.blend`, ảnh `vet_sao_bang_render/xem_dau.png`):
giọt sáng, phía sau tròn, phía trước thuôn thành mũi, hai sườn có vài răng cưa hướng ra sau (so le hai bên). Lần 1 răng cưa dồn về phía
sau và mũi mềm như ngọn nến; lần 2 thành cây thông (quá nhiều răng) → giảm còn vài răng, mũi ngắn lại.
Mũi phải chĩa theo hướng rơi, mà hạt billboard xoay tự do không giữ được hướng → đầu là **một tấm tứ giác** (`DauSaoBangHuong.cs`):
trục lên của tấm luôn trùng hướng rơi, mặt tấm quay về camera quanh trục đó; tâm đốm sáng đặt đúng đầu vệt.

**Mọc băng** (`FallingShard.TrungKeDich`): bỏ hẳn phép thử va chạm lớp `Default`; chỉ còn Damageable còn sống trên `damageMask`
(quái; người chơi khác khi chơi mạng) trừ người tung.

**Số đo** (menu 68, `quacaubang.txt`, **0 lỗi**; ảnh `quacaubang_5b_muabang_roi_can.png`):

| Đo | Kết quả |
|---|---|
| Tấm đầu vệt | 32/32 vệt: đúng cỡ, mũi trùng hướng rơi (cos nhỏ nhất **1,0000**), mặt quay về camera (**1,0000**) |
| Hình đầu (đọc thẳng PNG) | bề ngang ở đốm **39** điểm ảnh, gần mũi **4** → nhọn; **3 răng cưa** (ĐỐI CHỨNG đốm tròn cũ trong git: 0 răng); mép 0,004 |
| Rơi cạnh bia mộ 1 m (có va chạm Default trong 1,7 m) | **0** gai (trước: 8) |
| Quái 1 m / người chơi khác 1 m | 8 gai / 8 gai |
| Chính người tung đứng trong vùng | 0 gai |
| Mưa băng thật không có ai | **0/37** quả có gai, trong khi **14/37** quả rơi gần đồ vật |
| Còn lại | vệt 0,463–0,652 m, dài trung bình 6,87 m; rơi thẳng 32/32; bia mất 788 máu, bị chậm; các phần nổ khác vẫn 8/8 |

**Lỗi của chính phép thử, tìm ra nhờ dòng chẩn đoán thêm hôm nay:** lần chạy đầu, cơn Mưa băng "không có ai" cho 37/37 quả có gai — chẩn
đoán in ra `TAM_BiaDuong@Enemy`: bia thử của mục C vẫn còn sống gần đó, và Mưa băng nhắm kẻ địch 100% nên cả cơn đổ vào nó. Đây cũng là
nguyên nhân lần báo lỗi "chập chờn" ở mục trước (chỗ thử `TS_obelisk_369`). Sửa: dọn mọi bia `TAM_` còn sống trước cơn đó.

### Mưa băng: vệt sáng to thêm 20%, đầu tròn phát sáng, to nhỏ ngẫu nhiên (17/09/2026)

> ⚠️ Đầu tròn trong mục này **đã đổi** thành giọt sáng mũi nhọn răng cưa (mục trên).

Anh báo: vệt sáng băng **to thêm 20%**; **đầu vệt như bị cắt mất ngang** — thiết kế lại; mỗi vệt **to nhỏ ngẫu nhiên** nhưng không bé hơn
70% cỡ gốc. Tôi hỏi trước, anh chọn: 20% **chỉ bề ngang**; ngẫu nhiên **70%–100% của cỡ mới**; đầu vệt là **đốm sáng tròn** phát sáng.

**Vì sao đầu bị cắt:** ảnh vệt cũ sáng nhất đúng ở cột u = 0 (độ đục 1,0) và thân vệt rộng đủ 0,55 m ngay tại đó, nên đầu `TrailRenderer`
là một cạnh thẳng ngang, sáng rực.

**Vẽ lại trong Blender** (`CongCu/Blender/vet_sao_bang.blend`, ảnh xem `vet_sao_bang_render/xem_dau.png`):
- `VetSaoBang.png`: thân vệt **mờ vào từ u = 0** (độ đục cột đầu 0) và lõi **thuôn nhọn** về đầu, không còn cạnh.
- `DauSaoBang.png` 128×128 (mới): đốm sáng tròn trắng xanh, quầng tròn, 4 tia lấp lánh thẳng + 2 tia chéo mờ, tắt hẳn ở mép.

**Unity** (`VfxFactory.DungVetSaoBang`): bề rộng gốc `RongDauVetSaoBang` 0,66 m / `RongDuoiVetSaoBang` 0,36 m (trước 0,55 / 0,30); mỗi vệt
`Random.Range(0,7; 1)` nhân vào `widthMultiplier`; đốm `DauSaoBang` (hạt billboard cộng sáng, đường kính 1,6 × bề rộng đầu vệt, cùng tỉ lệ
ngẫu nhiên) đặt đúng đầu vệt. Chiều dài không đổi.

**Số đo** (menu 68 mục K, `quacaubang.txt`, **0 lỗi**; ảnh `quacaubang_5b_muabang_roi_can.png`):

| Đo | Kết quả |
|---|---|
| Bề rộng đầu vệt thật của 33 vệt | nhỏ nhất **0,482 m**, lớn nhất **0,659 m** (cho phép 0,462–0,660) |
| Đốm sáng tròn ở đầu | 33/33 đang phát, 33/33 đúng cỡ theo vệt của nó |
| Ảnh đọc thẳng từ file PNG trên đĩa | cột đầu vệt độ đục **0,000** (ĐỐI CHỨNG ảnh cũ trong git: **1,000**), cột u = 0,1: 1,000; đốm tròn: tâm 1,000, mép 0,000 |
| Chiều dài | trung bình 6,98 m, ngắn nhất 3,34 m (không đổi) |
| Phần còn lại | không lưới 33/33, quầng sáng, mảnh băng, khí lạnh 33/33; rơi thẳng 33/33; bia mất 815 máu, bị chậm; cụm gai chạm đất đạt đủ |

### Mưa băng: bỏ khối cầu, chỉ còn vệt sáng băng rơi như sao băng (17/09/2026)

Anh báo: **bỏ khối cầu**, cho các **vệt sáng băng rớt từ trên trời xuống**. Tôi hỏi trước, anh chọn: **chỉ bỏ khối cầu** (giữ quầng sáng
nhỏ ở đầu vệt, luồng khí lạnh, mảnh băng lấp lánh); vệt **dài, mảnh như sao băng**; vẽ bằng **Blender MCP**; chạm đất **giữ nguyên**.
Quả cầu tròn bọc khí lạnh làm cùng ngày (mục ngay dưới) bị bỏ: xoá `CauBangTron.fbx` và `KhiLanhBoc.png` khỏi `Resources` (file Blender
`cau_bang_tron.blend` vẫn giữ trong `CongCu/Blender`).

**Vẽ trong Blender** (`CongCu/Blender/vet_sao_bang.blend`, ảnh thử `CongCu/Blender/vet_sao_bang_render/xem.png`): ảnh `VetSaoBang.png` 512×64,
u = 0 là đầu vệt (nút sáng trắng), lõi mảnh nhỏ dần và nhạt dần về đuôi, quầng xanh lạnh hai bên, vài đốm lấp lánh. Lần đầu đuôi tắt quá
sớm (giữa vệt độ sáng 0,14) và đốm lấm tấm như bụi → kéo dài độ sáng (giữa vệt 0,46), bớt đốm.

**Unity** (`Vfx/VfxQuaCauBang.cs`): bản rơi (`banRoi`) không dựng lõi; vệt là `DungVetSaoBang` — `TrailRenderer` kéo giãn ảnh (Stretch),
sống `GiayVetSaoBang` = 0,17 s, rộng 0,55 m ở đầu → 0,30 m ở đuôi, cộng sáng. Quả rơi 20 m trong 0,96 s và nhanh dần (cuối ~42 m/s) nên
vệt tự dài ra khi xuống thấp. Tên vẫn `VetBang` để `ThaDuoiQuaCauBang` thả nó tan dần khi chạm đất như cũ.

**Số đo** (menu 68 mục K, `quacaubang.txt`, **0 lỗi**; ảnh `quacaubang_5_muabang_roi.png`, `quacaubang_5b_muabang_roi_can.png`):

| Đo | Kết quả |
|---|---|
| Khối cầu | 33/33 quả rơi **không có lưới nào**; ĐỐI CHỨNG quả của kỹ năng Quả cầu băng dựng bằng cùng hàm: 1 lưới |
| Vệt sao băng | 33/33 dùng ảnh `VetSaoBang` kéo giãn; độ dài thật (cộng các đoạn của vệt) lớn nhất mỗi quả: ngắn nhất **3,76 m**, trung bình **7,14 m**, dài nhất 12,80 m; dài / rộng ×13 |
| Phần giữ lại | quầng sáng 33/33, mảnh băng 33/33, luồng khí lạnh 33/33, 0 đèn riêng; rơi thẳng đứng 33/33; bia mất 812 máu và bị chậm |
| Chạm đất | cụm gai chỉ khi trúng đồ vật / kẻ địch: như trước (mục L đạt đủ) |
| Kỹ năng Quả cầu băng | vẫn lưới có gai, đuôi phía sau |

Trong ảnh chụp gần: đầu vệt sáng trắng nằm dưới, đuôi mảnh nhạt dần lên trời — đúng chiều. Vệt dài nhất 12,8 m vượt mức tính (~7 m ở
42 m/s × 0,17 s), nhiều khả năng do Editor tụt khung làm các điểm của vệt giãn thưa; chưa đo riêng.

### Mưa băng: quả cầu rơi thành cầu tròn bọc khí lạnh phát sáng, bỏ gai (17/09/2026)

> ⚠️ **Đã thay ngay trong ngày** (mục trên): bỏ hẳn khối cầu, chỉ còn vệt sáng băng như sao băng.

Anh báo: quả cầu băng của Mưa băng **không cần các gai xung quanh**, chỉ cần quả cầu có **khí lạnh bọc quanh, phát sáng**; hiệu ứng
rơi giữ như cũ. Tôi hỏi trước, anh chọn: bỏ **gai trên thân quả cầu** (cụm gai mọc dưới đất khi trúng đồ vật / kẻ địch giữ nguyên);
**chỉ Mưa băng** (kỹ năng Quả cầu băng vẫn có gai); dựng bằng **Blender MCP**.

**Dựng trong Blender** (`CongCu/Blender/cau_bang_tron.blend`, scene `CauBangTron`; ảnh thử ở `CongCu/Blender/cau_bang_tron_render/`):
- `LoiTron`: cầu ico bán kính 0,5, mặt hơi gợn bằng nhiễu nhỏ (không gai).
- `KhiLanh0` (r 0,66) và `KhiLanh1` (r 0,84): hai vỏ cầu có UV quấn quanh thân, trải ảnh **`KhiLanhBoc.png`** 512×256 — sợi khí lạnh xoắn
  nghiêng quanh cầu, mờ dần về hai cực, **liền mạch theo chiều quấn** (nhiễu lấy trên vòng tròn cos/sin: chênh lệch cột đầu–cột cuối 0,0064,
  bằng chênh lệch hai cột kề nhau 0,0075).
- Lần render đầu ảnh khí quá đặc, quả cầu thành viên bi trắng → làm sợi mảnh và thưa hơn (độ đục trung bình 0,20 → 0,04), lõi xanh lộ ra.

**Unity** (`Vfx/VfxQuaCauBang.cs`): `BuildQuaCauBangVisual(..., banRoi: true)` dùng `DungCauBangTron` — lõi dùng vật liệu pha lê cũ
(`LoiCauBangMat`), hai vỏ **cộng sáng** (additive) quay ngược chiều nhau trên hai trục khác nhau và trượt ảnh, nên sợi khí cuộn quanh lõi.
Phần rơi dùng chung như cũ: hào quang, luồng khí lạnh, vệt băng, mảnh băng, rơi thẳng đứng, `FallingShard` không đổi. Bản ném của kỹ năng
Quả cầu băng vẫn đi `DungLoiCoGai`. Nạp không được lưới tròn thì tự quay về lõi có gai.

**Số đo** (menu 68, `quacaubang.txt`, **0 lỗi**; ảnh so sánh `quacaubang_8_tron_vs_gai.png`):

| Đo | Kết quả |
|---|---|
| Lưới của 32 quả Mưa băng thật | lưới tròn **32**, lưới có gai **0**; hai vỏ khí lạnh cộng sáng **32**, đang quay **32**, hào quang đang phát **32** |
| Không còn đuôi gai | lệch tâm lõi lớn nhất **0,001 m**, tỉ lệ cạnh hộp bao lưới **1,010**; ĐỐI CHỨNG lưới có gai cùng phép đo lệch **0,648 m** |
| Hiệu ứng rơi giữ như cũ | luồng khí lạnh 32/32, vệt băng 32/32, rơi thẳng đứng 32/32 (lệch ngang 0,000 m), 0 đèn riêng; bia vẫn mất máu và bị chậm |
| Quả cầu băng (kỹ năng) | vẫn lưới có gai, đuôi phía sau (-0,451 m) |

Phép thử sửa 2 lần: tỉ lệ cạnh lấy từ `Renderer.bounds` ra **1,395** cho cầu tròn — đó là hộp trục thế giới bao quanh hộp đã xoay, nên
vật đang quay cũng ra ~√2 → đổi sang hộp bao cục bộ của lưới (1,010). Lần chạy đầu mục L báo 4 lỗi (quả rơi chỗ trống vẫn có gai) ở một chỗ
thử khác (`TS_obelisk_369`); hai lần chạy sau ở chỗ khác đều đạt. Luồng gai không bị động đến trong lần sửa này; đã thêm dòng chẩn đoán liệt kê
kẻ địch còn sống quanh điểm rơi để lần sau gặp lại biết ngay nguyên nhân. (Sau đó tìm ra: bia thử `TAM_BiaDuong` của mục C còn sót — xem mục "đầu vệt nhọn răng cưa".)

### Gió lốc: chân to thêm 20% nữa, tia sét hiệu ứng khi trúng đối thủ (17/09/2026)

> ⚠️ **Tia sét khi trúng đối thủ đã bỏ** cùng ngày, thay bằng hai tia sét luôn đánh trong lòng lốc (mục "Gió lốc: rộng thêm 10%...").

Anh xin: thân dưới **to thêm 20%**; lốc **trúng quái / người chơi khác** thì xuất hiện **tia sét tương ứng số đối thủ bị trúng**, chỗ
bị đánh **bốc khói như Sấm sét đánh trúng**; tia sét **chỉ là hiệu ứng, không gây sát thương**.
Tôi hỏi trước (nếu tính 20% so với gốc như các lần trước thì chân lại **nhỏ đi**), anh chọn: **thêm 20% trên bản hiện tại** (×1,68 so gốc);
tia sét **giật từ thân lốc sang đối thủ**; khói **như Sấm sét: cháy sém + khói bốc trên đất chỗ đối thủ đứng**.

**Làm**: Blender nhân thêm `1 + 0,2·s(z, 2,3 m)`, xuất lại FBX. `VfxFactory.GioLocGiatSet` gọi trong `GioLoc.TrungMot` (trước cú
75): `LightningArc` từ giữa thân lốc (2,2 m) tới ngực đối thủ + chớp Sấm sét (prefab `Vfx_SetChamDat`, bán kính 2,1 như `Skill_SamSet`)
+ `SetChayDen` (lượng khói 0,45 như `LightningStrike.Strike`). **Tắt hai cột sáng đứng `Column0/1`** của prefab chớp: ảnh chụp đầu có một
cột sáng chọc thẳng lên trời — đó là lớp bọc tia sét **từ trời đánh xuống** của Sấm sét, trái với tia giật ngang anh chọn.
Mỗi máy tự vẽ tia khi bản lốc trên máy đó trúng đối thủ (bản lốc chạy trên mọi máy), không thêm gói tin.

**Số đo** (menu 71 mục **L** mới, **0 lỗi**):

| Đo | Kết quả |
|---|---|
| Chân lốc | Vo1 **0,504 m (×1,680 so gốc = ×1,2 bản trước)**; ở 2,5 m **0,822 m (×1,000)** |
| 5 bia trên đường bay | **5** tia sét mới, **5/5** đầu tia ở thân lốc, **5/5** cuối tia ở bia; **5** chỗ cháy sém bốc khói; **5** chớp Sấm sét, cột sáng đứng còn bật **0** |
| Không gây sát thương | 5/5 bia mất **đúng 75** |
| Lốc không trúng ai | **0** tia sét (mục C) |
| Còn lại | 1 lốc, 8 m/s, 4,52 s, không trèo mái (tia cũ chạm mái 4,57 m, lốc lệch 0,000 m), hất tung 55,3%, ngắt chiêu, mạng, lò lửa đạt |

Lần chạy trước khi sửa ra 1 lỗi ở chính phép thử: điều kiện "mô tả Sách phép không nhắc tia sét" (đặt lúc anh bỏ tia sét) đã lỗi thời —
đổi thành phải nói rõ tia sét **không gây thêm sát thương** và không còn số 15. Ảnh `gioloc_3_tia_set.png`; so Blender `so_chan_40_68.png`.

### Gió lốc: một lốc, tan sau 4,5 giây, chân to 40%, sửa lỗi lốc trèo lên mái nhà (17/09/2026)

Anh xin: thân dưới **to thêm 40%**; tung kỹ năng chỉ ra **1 lốc** thay vì 3, **tan sau 4,5 giây**; và báo lỗi: lốc đánh trúng một **căn
nhà nhỏ thì tự trèo lên mái nhà**. Tôi hỏi trước, anh chọn: 40% **so với bản gốc**; đoạn nới **theo khung đỏ mới, về 0 ở ~2,3 m**; giữ
**20 năng lượng, 75 sát thương**; **chỉ sửa Gió lốc** (Lốc xoáy lớn tôi đo riêng rồi báo).

**Nguyên nhân trèo mái**: lốc bám mặt đất bằng `VfxFactory.GroundY` — tia chiếu xuống **cả lớp `Default`** (nhà mồ, bia, đá, hàng rào).
Lốc đi xuyên vật cản nên vào tới nhà là tia chạm mái trước, lốc "đứng" lên mái. **Sửa**: `GioLoc.MatDatY` chỉ chiếu lớp `Ground`
(tia không chạm thì giữ độ cao cũ, không rơi về y = 0).

**Làm**: Blender nhân bán kính `(1 + 0,4·s(z, 2,3 m)) / (1 + 0,3·s(z, 2,0 m))` (đổi từ bản +30% sang +40% so gốc), xuất lại FBX;
`GioLoc.SoLocMoiLan = 1`, `ThoiGianSong = 4,5`; Sách phép đổi chữ ("Một cơn lốc, 55% hất tung").

**Số đo** (menu 71, **0 lỗi**, mục **K** mới):

| Đo | Kết quả |
|---|---|
| Chân lốc (so công thức gốc) | Vo1 **0,300 → 0,420 m (×1,400)**; ở 2,5 m **0,822 m (×1,000)** |
| Số lốc / thời gian sống | tung thật ra **1** lốc; qua mạng phát lại **1**; ngừng đi sau **4,50 s**; bia trước mặt mất 75 |
| Không trèo mái (nhà mồ `MAUS_A_001_682`) | **đối chứng**: tia cũ (Ground + Default) trên đường bay chạm mái cao **4,57 m**; Gió lốc đi 98 khung, **28 khung trong nhà**, lệch khỏi mặt đất **0,000 m** |
| Lốc xoáy lớn cùng đường (chỉ báo) | 0,00 m — **nhưng không kết luận được**: nhà rộng 5,6 m < `VatTheBiCuon.RongToiDa` 6 m nên Lốc xoáy lớn **cuốn luôn căn nhà** (tắt va chạm) trước khi tới; vật > 6 m (hàng rào quanh map) chưa đo |
| Còn lại | 8 m/s, xoáy một chiều 382/0 · 4/4, bụi 23/23, vùng 2,2 m, hất tung 58,4%, khiên, ngắt chiêu, mạng, lò lửa đều đạt |

Ảnh `gioloc_can_*.png`; so Blender gốc / +40%: `CongCu/Blender/gio_loc_render/so_chan_goc_40.png`.

### Gió lốc: thân dưới to thêm 30% so với bản gốc, bay 8 m/s (17/09/2026)

Anh khoanh lại phần thân dưới: vẫn còn nhỏ, **to thêm 30%**; tốc độ **8 m/s**. Tôi hỏi mốc 30%, anh chọn **30% so với bản gốc (thay
cho 20%)**, không cộng dồn lên bản vừa nới. Vẫn nhỏ dần mượt về 0 ở ~2 m, phần trên giữ nguyên, vùng trúng giữ 2,2 m.

**Sửa trong Blender**: nhân thêm `(1 + 0,3·s) / (1 + 0,2·s)` (s = 1 − smoothstep(z / 2 m)) để đổi từ +20% sang +30% so với gốc, xuất lại
`LocNho.fbx`. `GioLoc.TocDo = 8`. Phép thử chỉnh theo tầm bay mới 28 m (hàng bia hất tung cách 1,3 m; chờ xuyên bia 1,9 s).

**Số đo** (menu 71, **0 lỗi**): Vo1 chân **0,300 → 0,390 m (×1,300)**, ở 2,5 m **0,822 m (×1,000)** — so với công thức gốc; tốc độ
**8,00 m/s**, tan sau 3,51 s; còn lại vẫn đạt (75 / 225, vùng 2,2 m, xoáy một chiều 382/0 · 4/4, bụi 24/24, hất tung 53,7%, ngắt chiêu,
mạng, lò lửa). So ảnh Blender gốc / +30%: `CongCu/Blender/gio_loc_render/so_chan_goc_30.png`.

### Gió lốc: thân dưới to thêm 20%, bay 10 m/s (17/09/2026)

Anh khoanh đỏ phần thân dưới lốc (trên ảnh render Blender): còn quá nhỏ, **bán kính to thêm 20%**, phần trên giữ nguyên; tốc độ **10 m/s**.
Tôi hỏi trước, anh chọn: **+20% ở chân, nhỏ dần mượt về 0 ở ~2 m** (mép trên khung đỏ, 40% chiều cao); vùng trúng **giữ 2,2 m**.

**Sửa trong Blender** (`gio_loc.blend`): nhân bán kính mọi đỉnh của 3 vỏ + dải gió với `1 + 0,2 × (1 − smoothstep(z / 2 m))` rồi xuất
lại `LocNho.fbx`. `GioLoc.TocDo = 10`.

**Số đo** (menu 71, **0 lỗi**): đo trên lưới đã nhập, so với **công thức gốc** R(t) = 0,30 + 1,95·t^1,9 (không đọc lại hằng trong code):
vỏ Vo1 chân **0,300 → 0,360 m (×1,200)**, ở 2,5 m **0,822 m (×1,000)**. Tốc độ đo **10,00 m/s**, tan sau 3,51 s; các phần còn lại
(75 / 225, vùng 2,2 m, xoáy một chiều 382/0 · 4/4 lớp, bụi 24/24 bay lên cùng chiều, hất tung 47,9% lần này, ngắt chiêu, mạng, lò lửa)
đều đạt. Ảnh `gioloc_can_*.png`, so ảnh Blender trước/sau `CongCu/Blender/gio_loc_render/so_chan_truoc_sau.png`.

### Gió lốc dựng lại bằng Blender: xoáy một chiều từ dưới lên, khói bụi đen, bỏ tia sét, chậm 25% (17/09/2026)

Anh báo: 3 cơn lốc nhỏ **quá xấu** — xin dựng lại bằng Blender MCP cho thấy gió lốc xoáy thật chi tiết, **xoáy một chiều từ dưới
lên**, kèm **khói bụi đen cuộn bay lên**, màu như Lốc xoáy; **bỏ tia sét**; tốc độ bay **giảm 25%**.
Tôi hỏi trước, anh chọn: **xám trắng như Lốc xoáy** (bỏ nâu); bỏ tia sét **cả hình lẫn sát thương 15** (còn 75); khói bụi **vừa cuộn
quanh thân vừa để lại vệt phía sau**; cao **~5 m**.

⚠️ **Sự cố**: lệnh Blender đầu tiên của tôi có `read_factory_settings` (để có cảnh trống) — nó gỡ luôn addon MCP, anh phải **tắt hẳn
Blender và mở lại**. Đã ghi vào bộ nhớ thành quy tắc: không dùng lệnh nào làm tắt Blender MCP; cảnh sạch thì tạo scene mới.

**Dựng trong Blender** (`CongCu/Blender/gio_loc.blend`, scene `GioLoc`; ảnh gốc ở `CongCu/Blender/gio_loc_render/`):
- **Ảnh gió** `GioDai.png` (dải mềm) và `GioSoi.png` (sợi mảnh thưa): nhiễu 4D quấn trên **mặt xuyến** (cos/sin của cả u và v) nên
  **liền mạch cả hai chiều** — trượt mãi không thấy đường nối; vệt nghiêng quấn đúng một vòng (`u + v = hằng số`). Thử 6 lần ngưỡng: dày
  kín mặt → mảnh như mưa → dải mềm có khoảng hở.
- **Flipbook** `BuiDenCuon.png` 6×6: đám bụi méo, mép rách, có lỗ, nở dần (lần đầu tròn như cục bông → tăng méo + ngưỡng).
- **Lưới** `LocNho.fbx`: 3 vỏ phễu cao 5 m, loe miệng ~2,6 m (cùng tỉ lệ Lốc xoáy), UV quấn quanh thân, màu đỉnh xám trắng mờ ở chân và
  miệng; 5 **dải gió** xoắn 1,6 vòng, hai miếng bắt chéo.
- Render thử lần đầu thấy **dải gió xoắn ngược tay với vệt trên ảnh** (dải: góc tăng theo độ cao; ảnh: giảm) — quay lên thì một cái đi
  lên, một cái đi xuống. Đảo dải cho cùng tay.

**Unity** (`Vfx/VfxGioLoc.cs` viết lại): 4 lớp lưới Blender, **cùng một chiều quay**, ảnh trượt cùng chiều; khói bụi đen `BuiCuon` phun
vòng ngang ở chân, bay lên 2,6–3,6 m/s, quỹ đạo cùng chiều thân, nở to; vệt `KhoiBui` không gian thế giới ở lại phía sau (hàm của Lốc xoáy,
đổi sang flipbook mới); hạt đất `Grit` cùng chiều. Không đèn, không tia sét. `GioLoc.TocDo = QuaCauBang.TocDoBay × 0,75`.
**Chiều quay chọn bằng số đo, không đoán**: đo trên lưới đã nhập — dải gió 930/930 cặp đỉnh "góc atan2(z,x) giảm khi lên cao", vỏ 545/545
"góc tăng theo uv.x" (tức vệt ảnh cũng giảm khi lên cao) → quay làm góc **tăng** thì vệt chạy lên → `Transform.Rotate` quanh +Y góc
**âm** (góc dương làm atan2(z,x) giảm), UV trượt v âm.
Ảnh đầu trong game: ba vỏ chồng nhau thành màn sương trắng đều, bụi đen chỉ còn vệt xám → giảm độ đục hai vỏ ngoài (0,55/0,42 → 0,34/0,26),
dải gió 0,9, bụi đen đậm và to hơn (`gioloc_can_zoom.png`).

**Số đo** (menu 71, `gioloc.txt`, **0 lỗi**):

| Đo | Kết quả |
|---|---|
| Lưới | Vo0, Vo1, Vo2, DaiGio từ FBX Blender; cao **5,00 m** (Lốc xoáy 15,37 m) |
| Màu / đèn / sét | vỏ (0,89; 0,91; 0,94) xám trắng; **0** đèn; **0** tia sét suốt 3,5 giây |
| Xoáy một chiều từ dưới lên | độ xoắn dải gió đo ngoài Play: **382 giảm / 0 tăng**; quay thật sau 0,1 s: **4/4 lớp** làm góc tăng; ảnh trượt lên **4/4** |
| Khói bụi đen cuộn | 23 hạt theo dõi: **23 bay lên / 0 xuống**, **23 cùng chiều / 0 ngược**; vòng phun 200 hạt thử: lệch dọc **0,000 m**, bán kính 0,45 m; vệt phía sau không gian World |
| Tốc độ / sống | **12,75 m/s** (quả cầu băng 17); ngừng đi sau **3,50 s** |
| Sát thương | một lốc **75**; ba lốc cùng qua **225**; vùng 2,2 m (2,5 m trúng / 2,7 m trượt) |
| Hất tung, ngắt chiêu, mạng, lò lửa | như trước: 56,8% (190 lần), cao 1,50 m, 0,51 s; khiên 0; ngắt chiêu 3/3 (đối chứng 3/3); quái 0/5 (đối chứng 5/5); bản sao 3/3; lò tắt, 31 s cháy lại |

Phép thử phải sửa 3 lần, đều do chính nó: trong Play lưới FBX **không đọc được đỉnh** (đếm độ xoắn ra 0/0 — bài học cũ) → đo ngoài Play;
ở chân lốc năm dải chỉ cách nhau ~0,37 m nên "đỉnh kế tiếp" bắt nhầm dải bên (51, rồi 7 cặp "tăng") → bỏ đoạn < 1,5 m và lấy đỉnh **gần
nhất**; kiểm vòng phun bằng 1–2 hạt vừa sinh lẫn vận tốc bay lên (0,05–0,06 m quanh ngưỡng) → nhân bản hệ hạt, tắt vận tốc, phun 200 hạt.

### Kỹ năng mới: Gió lốc — ba cơn lốc nhỏ màu nâu, hất tung và ngắt chiêu (16/09/2026)

> ⚠️ **Hình, tia sét và tốc độ trong mục này đã đổi ngày 17/09/2026** (mục ngay trên): lốc dựng lại bằng Blender, xám trắng, cao 5 m,
> không còn tia sét 15, bay 12,75 m/s. Phần hất tung / ngắt chiêu / mạng / lò lửa bên dưới vẫn đúng.

Anh xin: phóng ra **3 cơn lốc nhỏ** có hiệu ứng như Lốc xoáy nhưng **thấp hơn nửa chiều cao**, **màu nâu**, vẫn có tia sét
bên trong; bay nhanh bằng quả cầu băng, **tự tan sau 3,5 giây**; sát thương 75, hồi chiêu 0,4 giây; **55% hất tung** đối thủ
và người chơi khác 0,5 giây (không cuốn lên như Lốc xoáy), đang dùng kỹ năng mà bị hất thì **bị ngắt chiêu ngay**; lốc
**đi xuyên** mọi vật cản và người chơi.

Những chỗ anh chưa nói, tôi **hỏi trước khi làm** (ba lượt câu hỏi), anh chọn:

| Hỏi | Anh chọn |
|---|---|
| 75 tính thế nào | **một lần mỗi mục tiêu** cho mỗi cơn lốc (ba cơn cùng trúng = 225) |
| Tia sét có gây sát thương không | **có**: mỗi lần lốc trúng, **một tia 15** giật vào mục tiêu |
| 55% gieo thế nào | **mỗi cơn lốc gieo riêng** |
| Năng lượng | **20** |
| Hình phóng ra | **toả quạt 11°** như Quả cầu băng |
| Ngắt chiêu áp cho ai | **cả người chơi và quái** |
| Hình ảnh / icon | **dùng hình Lốc xoáy nhuộm nâu**, không Blender |
| Vùng trúng | **bán kính 2,2 m** |
| Bị hất cao bao nhiêu | **1,5 m** |
| Cảnh vật | **dập tắt lò lửa** khi lướt qua, 30 giây sau cháy lại (không cuốn cây / bia) |
| Thời gian niệm | **0,38 s** như Quả cầu băng |

Tôi tự theo **luật sẵn có** của dự án (không hỏi): số hiệu **10** thêm ở cuối; cấp kỹ năng +20% sát thương (cả tia sét) và
+0,15 s thời gian hất mỗi cấp; **khiên đang bật thì không bị hất** (luật chung như choáng / đóng băng / ngã); chữ nổi
**"HẤT TUNG!"** như "CHOÁNG!".

**Làm:**
- `Skills/GioLoc.cs`: lốc bám mặt đất, bay thẳng **17 m/s** (`QuaCauBang.TocDoBay` — hằng dùng chung, không chép số), **không hỏi va
  chạm gì** nên xuyên hết; mỗi khung quét **cả đoạn vừa đi** bằng `OverlapCapsule` (máy yếu 10 khung/giây là 1,7 m một khung — quét
  điểm cuối thì lọt người); mỗi mục tiêu ghi vào danh sách đã trúng → đúng một lần. Gọi `GhiKeDanh` trước cả hai cú sát thương.
- `Vfx/VfxGioLoc.cs`: **chính** `BuildTornado` (prefab `Skill_LocXoay` có cùng bộ con) ở tỉ lệ 0,45 → cao 6,9 m so với 15,4 m.
  Nhuộm nâu vỏ lốc / dải xoắn (vật liệu riêng từng cái) và **màu hạt** (vật liệu hạt **dùng chung** với Lốc xoáy — sửa vật liệu là
  Lốc xoáy cũng nâu theo). **Bản nhẹ**: bỏ đèn điểm (ba lốc = ba đèn), nửa lượng hạt. Tia sét: `TornadoBolt` cùng tỉ lệ.
- `Combat/BiHatTung.cs`: nhấc **model con** theo parabol 1,5 m trong 0,5 giây (va chạm ở gốc đứng yên — cùng cách `BiDanhNga`),
  khoá đi / đánh / tung phép, gọi **`PlayerController.NgatChieu`** (phép chưa phóng thì không bao giờ phóng, tắt hiệu ứng tích tụ,
  bỏ tư thế niệm) và **`EnemyAI.NgatDon`**. Đang bay mà bị hất tiếp thì tính lại 0,5 giây từ **độ cao đang có**.
- **Qua mạng**: bit hiệu ứng thứ **năm** `CoHatTung` → nới mặt nạ **0x0F → 0x1F** ở **cả** gói người chơi lẫn gói quái (bài học
  menu 63). Máy chủ sở hữu gieo 55%; bản sao nhận bit thì bay một cú 0,5 giây **và ngắt chiêu bản sao** — bản sao bắt đầu niệm trễ
  đúng bằng độ trễ gói tin, bit hất tung đi cùng đường nên luôn tới **trước** lúc bản sao phóng phép. Gói "đang bay" cuối cùng tới
  trễ sau khi hình đã rơi xuống **không** hất lần hai (nhớ lúc kết thúc từng mục tiêu, bỏ qua trong 0,35 s).
- `LoLuaDa.DapTatRoiChayLai(30)`: Gió lốc không cuốn lò nên không có `VatTheBiCuon` nào nhóm lửa lại — lò tự hẹn giờ; nếu Lốc xoáy
  lớn đang cuốn lò lúc hết giờ thì nhường cho `VatTheBiCuon.MocLai`.
- Nối vào `CapDo` (`KyGioLoc = 10`, `SoKyNang = 11`), `PlayerController` (20 · 0,4 · 0,38, hồi chiêu vào trạng thái dự đoán),
  Sách phép (tên, tóm tắt, mô tả có dấu), HUD / sảnh (icon thứ 11), màu chỉ báo ngắm nâu, tư thế niệm của Lốc xoáy.
  Icon: `python CongCu/Icon/sinh_gio_loc.py` — ba bản thu nhỏ icon Lốc xoáy nhuộm nâu.

**Số đo** (menu **71** mới, `gioloc.txt`, **0 lỗi**):

| Đo | Kết quả |
|---|---|
| Thông số nhân vật / Sách phép | 20 năng lượng · hồi chiêu 0,4 s · niệm 0,38 s; HUD 11 icon |
| Tung thật `CastAt(10)` | khoá thì từ chối; mở khoá → **3 lốc**, tốn **20**; bấm lại mỗi khung → được nhận sau **0,418 s** (thấy "GIÓ LỐC đang hồi chiêu" trước đó); kẻ đánh cuối = người tung |
| Chiều cao hình | lốc nhỏ **6,92 m**, Lốc xoáy thật **15,37 m** → **0,45** |
| Màu vỏ lốc | (0,75; 0,53; 0,31) — nâu; **0** đèn; **12–15** tia sét xuất hiện khi bay không có mục tiêu nào |
| Tốc độ / thời gian sống | **17,00 m/s** (quả cầu băng thật: 17); ngừng đi sau **3,50–3,51 s**, rồi vật thể bị xoá |
| Xuyên vật cản | đi thẳng qua bia mộ (tia đối chứng cùng đường **chạm** bia) tới 16,4 m trong 1 giây |
| Sát thương | một lốc qua bia đứng yên: **90** (75 + 15) đúng một lần; ba lốc cùng qua: **270**; bia lệch 2,5 m (mép thân 2,1 m) mất 90, lệch 2,7 m (mép 2,3 m) **0**; lốc đi tiếp **33,8 m** sau khi trúng |
| Hất tung (19 bia × 10 lốc) | 190 lần trúng → **57,9% · 51,6% · 50,0%** ở ba lần chạy (gộp 570 lần: **53,2%**, mong 55%); đếm độc lập bằng component = bộ đếm trong code; cao nhất **1,50 m**; bay **0,51 s** |
| Khiên | 20 lốc với tỉ lệ 100%: hất **0**, máu mất 0 |
| Ngắt chiêu người chơi | đang niệm Quả cầu lửa bị hất: **3/3** lần không quả nào (đối chứng không hất: 3/3 ra đủ 3 quả); lốc thật trúng người đang niệm: 0 quả; bấm kỹ năng lúc đang bay → "BẠN ĐANG BỊ HẤT TUNG!" |
| Ngắt đòn quái | bộ xương đang vung tay bị hất: **0/5** đòn trúng; đối chứng: **5/5** trúng |
| Qua mạng | gói kỹ năng số 10 → máy mình phát lại 3 lốc; gói trạng thái mang bit hất tung; mặt nạ 0x1F qua được cả hai gói; bản sao nhận bit bay cao **1,50 m**, gói trễ **không** hất lần hai; bản sao đang niệm nhận bit: **3/3** không phóng (đối chứng 3/3 phóng đủ) |
| Lò lửa | lốc lướt qua → tắt, lò đối chứng ở xa vẫn cháy; **25 s** vẫn tắt, **31 s** cháy lại |

Lần chạy đầu ra 4 lỗi, **cả bốn ở phép thử**: ba lốc đo sau bay đúng đường các bia trước (trúng thêm); cấp 1 chỉ có **1 điểm kỹ
năng** nên Quả cầu lửa (để có phép "đang niệm") vẫn khoá — đối chứng ra 0/3 mới lộ; kiểm bản sao ngay khung đầu, trước khi gói
tin được xử lý. Lần hai: hồi chiêu đo bằng `WaitForSeconds(0.3)` trượt quá 0,4 s vì Editor giật khung → đổi sang bấm mỗi khung đo
lúc được nhận; bia thấp làm tia đối chứng ở độ cao 1 m bay qua đầu → bắn ngang tâm bia.
Chạy lại menu **61** (thêm Gió lốc: giết quỷ dữ được đủ 40 kinh nghiệm), **63** (mặt nạ 5 bit không phá bit ngã), **66**
(xem trước 11/11 kỹ năng), **59**: đều **0 lỗi**.

Ảnh `gioloc_can_*.png` (menu 71b, cạnh `gioloc_locxoay_*.png`): lốc nâu xoắn, tia sét lách tách bên trong — độ sáng tia **bằng**
Lốc xoáy lớn vì dùng chung hàm; ở một số khung tia sét trắng xanh át phần nâu.

### Mưa băng: cụm gai băng chỉ mọc khi trúng đồ vật hoặc kẻ địch (16/09/2026)

Anh xin: quả cầu băng chỉ trúng mặt đất thì nổ bình thường, **không tạo khối băng**; trúng đồ vật / người chơi khác / quái thì vừa
nổ vừa tạo băng. Ba chỗ chưa rõ tôi **hỏi lại trước khi làm** (quy tắc mới: không chắc 95% thì hỏi), anh chọn:
- "Nổ bình thường" = **bỏ cụm gai băng**, giữ chớp sáng, vòng lạnh, sương, giọt nước, mảnh băng văng, vết sương giá.
- "Trúng người / quái" = có kẻ địch còn sống **trong vùng sát thương 1,7 m** của quả đó.
- "Đồ vật" = **mọi vật cản** trong 1,7 m.

**Đo trước khi làm** (Act2): mặt đất là **một** `TerrainCollider` lớp `Ground`; đồ vật đều lớp `Default` — 681 BoxCollider (bia, đá),
68 CapsuleCollider (cây), 6 MeshCollider (nhà mồ, hàng rào), lò lửa (prefab lớp 0). Act1: mặt đất `WorldFactory` cũng lớp `Ground`.
Các hiệu ứng (`Vfx_NoBang`, vùng bão tuyết, `Skill_MuaBang`) **0 va chạm** nên không tự làm quả sau "trúng đồ vật".

**Sửa**: `FallingShard.TrungDoVatHoacKeDich` hỏi **trước khi** gây sát thương (con chết vì cú này vẫn tính): `CheckSphere` lớp `Default`
trong `impactRadius`, hoặc `Damageable` còn sống trên `damageMask` (trừ người tung). `VfxFactory.IceImpact(pos, r, coGai)`: không gai thì tắt
8 `CumGai*` cùng quầng sáng chân `HaoQuang*` của chúng (cả bản dựng bằng code cùng tên).

**Số đo** (menu 68 mục **L** mới, quả cầu rơi thật, **0 lỗi**):

| Trường hợp | Cụm gai đang bật | Phần nổ khác đang bật |
|---|---|---|
| Đất trống (không vật, không kẻ địch) | **0** | 8 |
| Bia mộ cách 1,0 m | **8** | 8 |
| Kẻ địch (bia đỡ đòn) cách 1,0 m | **8** | 8 |
| Đối chứng: bia mộ cách 2,6 m (ngoài 1,7 m) | **0** | 8 |

Mưa băng thật có bia đỡ đòn giữa vùng: **36/36** quả có gai — đúng, vì Mưa băng nhắm kẻ địch 100% (`aimAtEnemyChance = 1`, lệch ≤ 0,55 m).
Mưa băng thật **không** có kẻ địch (quả rơi ngẫu nhiên): **10/34** có gai, 24 không; đếm độc lập theo điểm rơi: **10/34** quả có đồ vật
trong 1,7 m — khớp đúng.

### Mưa băng rơi quả cầu băng thay tảng băng (16/09/2026)

Anh xin: thay vì rơi các tảng băng thì rơi các **quả cầu băng**, có luồng không khí lạnh phía sau giống y hệt kỹ năng Quả cầu băng.

**Sửa:** `IceStorm` gọi `VfxFactory.QuaCauBangRoi` thay `IceShardFalling`. Quả rơi dùng **chung** `BuildQuaCauBangVisual` (lõi pha lê
Blender, hào quang, luồng khí lạnh, vệt băng, mảnh băng lấp lánh) ở **bản nhẹ**: nửa lượng hạt, **không đèn riêng** — cứ 0,128 giây
một quả, mỗi quả rơi 0,96 giây, tức 7–8 quả cùng lúc trên không; 7–8 đèn điểm quá nặng cho điện thoại (vùng bão tuyết đã có ánh sáng).
Phần rơi / sát thương / làm chậm / đóng cứng vẫn là `FallingShard` cũ, không đổi; chạm đất thì **thả** luồng khí lạnh + vệt băng
ra tan dần. Bán kính quả 0,36–0,50 m.

**Hướng rơi**: tôi tự cho rơi **chéo ngang theo màn hình** (gió ngẫu nhiên trùng hướng nhìn làm vệt băng thành cột sáng đứng) —
anh **không muốn nghiêng**, xin rơi **thẳng đứng** từ trên cao. Đã bỏ phần gió: quả sinh ngay trên điểm rơi 20 m, trục bay chỉ thẳng
xuống (`LookRotation(xuống, trước)` — dùng `up` mặc định thì hai vector song song, hướng quay không xác định). Menu 68 mục K đo thêm:
**34/34** quả rơi thẳng đứng, lệch ngang lớn nhất **0,000 m**, vẫn đủ lưới Blender / luồng khí lạnh / vệt băng / đuôi gai phía sau,
bia vẫn mất 806 máu và bị làm chậm — **0 lỗi**.

**Số đo** (menu 68 mục **K** mới, Mưa băng **thật**, **0 lỗi**): tối đa **8** quả rơi cùng lúc; xét **33** quả: 33 dùng lưới Blender,
33 có luồng khí lạnh đang phát, 33 có vệt băng, 33 có đuôi gai phía **sau** hướng rơi, **0** gắn đèn riêng; **0** tảng băng cũ;
bia giữa vùng mất 815 máu và bị làm chậm/đóng cứng; tổng số hạt cùng lúc tối đa **1 922** (cả vùng bão tuyết + cụm băng nổ).
Lần đo đầu 33/34: phép đo bắt trúng đúng khung chạm đất (vệt vừa thả ra, vị trí trùng đích) — nay bỏ qua quả còn cách đích < 0,6 m.
Ảnh cận cảnh: ở góc camera 2.5D quả cầu chỉ lọt khung ở 3–4 m cuối trước khi chạm đất (`quacaubang_5b_muabang_roi_can.png`).
Chạy lại: menu 58 **0 lỗi** (E1: 2/42 quả rơi trong 1,2 m quanh người tung — là quả thả ngẫu nhiên khi không nhắm được kẻ địch,
`ChonDiemRoi` không đổi, ngưỡng ≤ 10%); menu 67 **0 lỗi** (Mưa băng qua mạng vẫn đóng cứng người chơi khác 6/6).

### Quả cầu lửa: sát thương 85, vệt lửa vẽ lại bằng Blender (16/09/2026)

Anh xin: sát thương ban đầu **85**; vẽ lại vệt lửa phía sau quả cầu khi bay bằng Blender MCP — vệt cũ "dạng hình tam giác không thật".

**Nguyên nhân vệt tam giác** (đo được): hạt `Flames` của prefab `Skill_QuaCauLua` dùng `Tex_flame.png` — bản thân ảnh là **một hình
tam giác mờ**; mỗi hạt là một tam giác đứng quay mặt về máy quay, nối nhau thành vệt răng cưa.

**Dựng trong Blender** (qua MCP; `CongCu/Blender/vet_lua_qua_cau_lua.blend`; trong lúc làm Blender mất kết nối một lần, gọi lại thì nối được):
- **Flipbook `Flipbooks/LuaDuoi.png`** (4×4, 256² mỗi ô): một **đám lửa cuộn** từ lúc bùng sáng đến lúc vỡ vụn thành than đỏ — nhiễu 4D
  chạy theo pha (bên trong từng hạt lửa cũng cuộn), bóp méo toạ độ thành lưỡi lửa, mép mềm. Bản thử đầu tròn đặc như quả bóng → tăng
  biên độ méo, nâng ngưỡng màu cho vùng mờ thủng thành lỗ, mép mũ 1,7.
- **`KyNang/QuaCauLua/VetLuaDai.png`** (512×128): vệt lửa dài liền mạch — đầu trắng vàng, thân cam có vân lửa chạy dọc, mép xé bởi nhiễu,
  đuôi thon đỏ thẫm; dùng cho `TrailRenderer` nối các hạt thành đuôi sao chổi.

**Trong Unity** (`VfxDuoiLua.cs`, sửa lúc chạy trên bản sinh từ prefab — không đụng prefab/scene): `Flames` → flipbook cộng sáng, xoay
ngẫu nhiên, to dần; thêm `VetLua`; nổ thì **thả** hạt và vệt ra tan dần tại chỗ (trước đây xoá quả cầu là cả đuôi lửa biến mất trong một
khung). Ảnh đầu tiên cả chùm ba quả + bloom nhoè thành một khối trắng → hạ độ sáng flipbook 1,35 → 0,75, vệt 1,25 → 0,8, hạt và vệt nhỏ lại.
Quả cầu lửa của **quái Phù thuỷ** đi cùng `Fireball.Spawn` nên cũng có vệt mới (sát thương riêng của quái giữ nguyên).

**Sát thương**: `impactDamage` 55 → **85** ở **prefab** (prefab đè code) và mặc định trong code.

**Số đo** (menu **70** mới; chạy **trước khi sửa làm đối chứng: 7 lỗi** — prefab 55, ảnh `Tex_flame`, không flipbook, không vệt dài, bia
mất 52, không thả vệt; **sau khi sửa: 0 lỗi**): prefab 85, quả cầu thật khi tung 85 (3 quả); bia 8 m mất **80** (nổ cách tâm 0,40 m →
85 × 0,947); `Flames` dùng `LuaDuoi` 1024² flipbook 4×4; có `TrailRenderer` `VetLuaDai`; nổ xong 2 vệt được thả ra tan dần.

**Menu 49 hoá ra đo rỗng một nửa** (như menu 58 hôm trước): lần chạy cuối 12/09, trước hệ cấp độ — phần B `CastAt(0)` bị từ chối vì
kỹ năng khoá, "không có cú nổ nào". Nay mở khoá trước khi tung: **0 lỗi**; khiên đỡ ba quả 85 mất 142/150 máu, không vỡ.

### Giựt sét: tầm 20 m, sát thương 75, 4 tia cùng lúc, 15% choáng (16/09/2026)

Anh xin: tầm bằng Sấm sét, sát thương ban đầu 75, đánh cùng lúc 4 tia nếu trước mặt có 4 kẻ địch trong tầm (mỗi tia vẫn lan như
cũ), mỗi tia trúng — kể cả tia lan — 15% choáng. Về tầm: trong code Giựt sét **đã** xa 19,5 m, còn Sấm sét tới 18 m (thả giông xa
12 m + bán kính 6 m), "bằng Sấm sét" sẽ là ngắn đi — tôi hỏi lại, anh chốt **20 m**.

**Sửa** (`GiatSet.cs`, không có prefab nên hằng số trong code có hiệu lực): tầm 19,5 → **20** (vạch ngắm `TamGiatSet` đọc cùng hằng),
sát thương 30 → **75**, số tia 3 → **4** (chọn kẻ địch theo cách chấm điểm cũ: đúng hướng + gần), lan 5 lần giảm ×0,85 như cũ.
Choáng: mỗi cú trúng gieo **riêng** 15% → `StunnedEffect.Apply` **1,5 giây** (+0,15 giây mỗi cấp kỹ năng như mọi hiệu ứng). Thời
gian choáng anh không nói — tôi chọn 1,5 giây (Sấm sét 2,2 giây): Giựt sét hồi chiêu 0,4 giây, 4 tia × 6 cú mỗi lần, choáng dài
thì gần như khoá chết cả đám. Khiên đỡ trọn đòn thì không dính choáng. Giựt sét của **quái** giữ nguyên, không choáng.
Mô tả trong Sách phép viết lại (bản cũ còn nói "một tia").

**Số đo** (menu **69** mới, **0 lỗi**):

| Đo | Kết quả |
|---|---|
| Thông số | tầm 20, sát thương 75, 4 tia, choáng 15% / 1,5 giây; vạch ngắm 20; Giựt sét của quái choáng 0% |
| Tung thật vào 5 bia (vòng cung 15 m, cách nhau 6,5 m > bán kính lan) | **4** bia mất đúng **75**, 1 bia không mất; cả 4 trúng **trong cùng một khung hình** |
| Tầm | mặt bia 19,9 m → trúng; 20,4 m → không (tầm cũ 19,5 không trúng cả hai) |
| Lan | bia 18 m mất 75; bia 22 m ngoài tầm, cách bia trước 4 m, mất 64 (= 75 × 0,85 làm tròn) |
| 160 lần phóng (4 tia đầu + 4 tia lan mỗi lần) | tia đầu choáng **18,3%** (lần chạy trước 14,4%), tia lan **15,5%** (trước 13,4%) — gộp hai lần: 16,3% / 14,5% |
| Cấp kỹ năng 3 | choáng 1,80 giây, sát thương 108 (= 75 × 1,44) |

Hai lỗi đo lần đầu: máu bị làm tròn khi trừ (63,75 → 64); và phép đo cấp 3 bắt nhầm Giựt sét của con phù thủy trong đợt quái vừa ra
(sát thương 14) — nay lọc đúng vật do người chơi tung. Menu 61: giết quái bằng Giựt sét vẫn được đủ kinh nghiệm. Ảnh `giatset_1_bon_tia.png`.

### Mưa băng: cụm băng trên mặt đất to bằng của Quả cầu băng (16/09/2026)

Anh gửi ảnh vụ nổ Quả cầu băng, xin cụm băng Mưa băng tạo trên mặt đất cũng to như vậy. Cả hai dùng **chung** hiệu ứng
`IceImpact` (prefab `Vfx_NoBang`, nướng ở bán kính 1,7 m, phóng theo bán kính truyền vào): Mưa băng truyền 1,7 m (×1), Quả cầu băng
2,55 m (×1,5). Sửa: hằng `QuaCauBang.BanKinhHinhBang` (= 3,4 × 0,75 = 2,55 m) dùng chung; `FallingShard` gọi
`IceImpact(max(impactRadius, 2,55))`. **Chỉ hình to ra** — vùng gây sát thương / đóng băng của mỗi tảng vẫn 1,7 m.

**Số đo** (menu 68 mục J mới — bắt vật hiệu ứng vừa sinh, hộp bao các gai bỏ vòng phẳng, lớn nhất trong 0,8 giây): tảng Mưa băng
**thật** rộng **5,59 m**, cao 2,79 m; vụ nổ Quả cầu băng thật 5,61 m / 2,81 m → tỉ lệ **×1,00 / ×1,00**. Đối chứng cỡ cũ 1,7 m: 3,74 m
(×0,67) — phép đo phân biệt được. Menu 58 chạy lại **0 lỗi**; menu 68 **0 lỗi**.
⚠️ Mỗi cơn Mưa băng rơi ~40 tảng, mỗi tảng giờ phủ diện tích gấp 2,25 lần — cần để ý tốc độ khung hình trên điện thoại yếu.

### Kỹ năng mới: Quả cầu băng (16/09/2026)

Anh xin: phóng 3 quả cầu băng như Quả cầu lửa nhưng **có luồng không khí lạnh và vệt băng phía sau**; sát thương ban đầu **65**;
hồi chiêu **0,55 giây**; trúng thì **40% làm chậm 50%** tốc độ trong **2 giây**; chạm là nổ như quả cầu lửa, sát thương mọi kẻ địch
trong vùng nổ. Dựng bằng **Blender MCP**.

**Dựng trong Blender** (qua MCP, trong Blender đang mở của anh; lưu `CongCu/Blender/qua_cau_bang.blend`):
- **Lõi pha lê** `QuaCauBang.fbx` (236 tam giác): icosphere méo nhiều mặt, 7 tinh thể nhỏ quanh thân, 6 gai dài chụm ra sau như đuôi
  sao chổi. Lần đầu 20 gai toả đều trông như con nhím biển → dựng lại ít gai, gai sau dài.
- **Ảnh hạt**: `SuongLanh.png` (sương lạnh cuộn — nhiễu fBm tắt dần ra mép; lần đầu mép tròn cứng → mũ 2,4 cho mép mềm),
  `ManhBang.png` (tinh thể nhỏ + tia sáng chéo từ Glare Streaks).
- **Icon** `Icons/CauBang.png` 256², nền đen như các icon cũ: quả cầu bay chéo, quầng lạnh, luồng sương và vệt băng. Năm lần render
  mới đạt: lật trái/phải (máy quay nhìn từ +Y), sương đục như mây (vật phát sáng che nhau → chuyển sang cộng sáng trong suốt),
  quầng sáng làm quả cầu thành bóng đen.

**Trong Unity**: `QuaCauBang.cs` chép nguyên đường bay/va chạm của `Fireball.Update` (vật cản, mặt khiên, quét cả đoạn đường, tua
trước bù trễ mạng) — mỗi cái bẫy ấy đã vấp và có ghi chú ở Fireball. Vụ nổ: gai băng từ đất + vòng lạnh (`IceImpact`), bung sương +
mảnh băng, vết sương giá trên đất; sát thương giảm dần từ tâm ra rìa như quả cầu lửa, **gọi `GhiKeDanh` trước**, rồi mỗi mục tiêu gieo
**riêng** 40% → `FrozenEffect.ApCham(0,5; 2 s + 0,15 s/cấp)`. Làm chậm đi đường mạng sẵn có (máy chủ sở hữu gieo, bit `CoBang`).
Số hiệu **9** (thêm ở cuối, không chèn sau Giựt sét: số hiệu đi qua gói tin và nằm trong thứ tự ô đã lưu). Luồng khí lạnh + vệt
băng được **thả ra** khi quả cầu nổ để tan dần tại chỗ. Lõi dùng bản sao vật liệu băng **phát sáng thấp** (0,55): dùng thẳng
`Mats.Ice` (1,35) thì ba quả + bloom nhoè thành một đốm trắng — ảnh chụp lần đầu không đọc ra hình pha lê.

**Số đo** (menu **68** mới, **0 lỗi**):

| Đo | Kết quả |
|---|---|
| Lưới Blender / ảnh hạt / icon | 236 tam giác · 256² và 128² có kênh trong suốt · 256² |
| Thông số trên nhân vật | hồi chiêu 0,55 giây, 12 năng lượng, niệm 0,38 — Sách phép đọc ra khớp |
| Tung thật `CastAt(9)` | còn khoá → từ chối (có lời nhắc); mở khoá → 3 quả, tốn 12; bấm lại sau 0,45 s → "đang hồi chiêu"; sau 0,6 s tung được |
| Bia trên đường bay 8 m / bia lệch 7 m | mất 96 máu / 0 |
| Nổ ngay tâm | mất **65,00**; bia 1 m mất 56, 2,5 m mất 43, 5 m (ngoài 3,4 m) mất 0 |
| 1000 lần nổ | chậm **40,6%**, đóng cứng 0, slow 0,5, kéo dài 2,00 s; cấp 3 kéo dài 2,30 s |
| Hình lúc bay | lưới Blender, đuôi gai phía **sau** (tâm hộp bao lệch −0,45 m theo hướng bay), luồng khí lạnh đang phát, vệt băng, ánh sáng; nổ xong 3 luồng khí lạnh còn lại tan dần |
| Qua mạng | gói kỹ năng số 9 của người kia → máy mình phát lại 3 quả; mình tung → gói gửi đi mang số 9 |
| HUD / Sách phép | 10 icon, tên "QUẢ CẦU BĂNG", mô tả 511 ký tự |

Chạy lại: menu 61 thêm Quả cầu băng — giết quái được đúng **+32** kinh nghiệm, kẻ đánh cuối là mình; menu 60, 59, 66 (10/10 kỹ năng
xem trước) **0 lỗi**. Ảnh `quacaubang_1b_bay_can.png` (cận cảnh ba quả đang bay), `quacaubang_2_no.png`.

### Mưa băng lên người chơi khác: có tác dụng, nhưng không chữ và không thấy vỏ băng (14/09/2026)

Anh hỏi: Mưa băng đã thật sự đóng băng người chơi khác chưa, mà không thấy chữ "Đóng băng" như "Choáng", "Ngã".

**Kiểm tra trước** (menu **67** mới — bộ đồng bộ thật + kênh giả lập, người kia tung Mưa băng bằng **gói kỹ năng thật**):
- Có tác dụng thật: trúng **6/6** cơn bão, chậm 6/6, **đóng cứng 6/6**, chân và phép bị khoá (tốc độ ×0,00), gói trạng thái gửi đi
  mang bit đóng cứng 6/6; phía người xem, bản sao đứng im và tan khi gói hết.
- **Chữ nổi: 0.** Không có dòng code nào sinh chữ cho đóng băng — kể cả với quái. Đối chứng: gói "choáng" trên cùng bản sao ra
  "CHOÁNG!" (bộ đếm chạy đúng).
- **Vỏ băng phủ lên 0 renderer** — cả nhân vật mình, bản sao người kia lẫn con bộ xương đối chứng. `FrozenEffect` chỉ lấy
  `MeshRenderer` (hợp với quái dựng bằng code thời đầu); nhân vật và quái bây giờ là model có xương (`SkinnedMeshRenderer`).
  Người bị đóng cứng trông y hệt bình thường, chỉ đứng im.
- Lỗi phụ: đường **máy tự tính** của choáng in "CHOANG!" không dấu (chỉ đường qua mạng có dấu).

**Sửa:**
- `FrozenEffect.BaoChuDongBang`: chữ **"ĐÓNG BĂNG!"** xanh băng bay lên **lúc bắt đầu** đóng cứng — gọi ở `FrozenEffect.Apply` (máy tự
  tính) và `HieuUngQuaMang.ApCo` (bản sao, khi bit đóng cứng vừa bật). Bản sao không đi qua `Apply` nên không in hai lần; gói đến 60
  lần/giây cũng không lặp chữ.
- Vỏ băng phủ cả `SkinnedMeshRenderer`, bỏ qua renderer trong suốt (vòm khiên, hiệu ứng). **Dày khi đóng cứng, mỏng (0,5) khi chỉ bị
  chậm** — nhìn là phân biệt được. Ngưỡng hiện vỏ hạ 0,4 → 0,2 giây: bản sao chỉ được giữ sống 0,35 giây mỗi gói, ngưỡng cũ thì vỏ
  trên bản sao không bao giờ hiện.
- Shader `S_FrozenShell` phồng theo **mét thế giới** (trước: toạ độ vật — model Meshy mang tỉ lệ riêng).
- `StunnedEffect.Apply`: "CHOÁNG!" có dấu.

**Số đo sau khi sửa** (menu 67, **0 lỗi**): chữ "ĐÓNG BĂNG!" trên máy nạn nhân **7** = số lần bắt đầu đóng cứng đếm từng khung **7**;
trên bản sao **đúng 1** trong 1,5 giây nhận gói; vỏ băng phủ 1/1 renderer (mình, bản sao, quái); độ xanh (b − r) vùng thân bản sao trên
ảnh chụp: trước **0,014** → đang đóng cứng **0,123** → sau khi tan **0,009**; choáng máy tự tính ra "CHOÁNG!". Lần đo ảnh đầu chỉ đợi 1
giây sau cơn bão: vùng băng dưới đất (vật riêng, tắt chậm hơn cơn bão) nhuộm ảnh "trước" thành 0,129 — đợi 6 giây thì sạch.

**Menu 58 hoá ra đang đo rỗng**: lần chạy cuối là 12/09, trước hệ cấp độ. Từ 13/09 kỹ năng khoá lúc vào trận nên mẫu đối chứng tung
phép ra 0 lần, và "đóng cứng / choáng thì không tung được phép" ra 0 **chỉ vì kỹ năng khoá**. Nay phần D tự lên cấp 4 và mở khoá kỹ năng
1–4 trước khi đo: đối chứng tung 1 lần, đóng cứng 0, choáng 0 — **0 lỗi**. Menu 46 chạy lại **0 lỗi**.

### Sảnh: nút CÀI ĐẶT lên đầu trang, nút KỸ NĂNG mở Sách phép xem trước; con mắt khoá góc nhìn vẽ lại (14/09/2026)

Anh gửi ảnh sảnh (khung xanh bên trái ĐĂNG XUẤT) và ảnh nút con mắt, xin ba việc:
1. Nút **CÀI ĐẶT** lên đầu trang, ngay bên trái **ĐĂNG XUẤT**.
2. Chỗ cũ thành nút **KỸ NĂNG**: mở **nguyên cửa sổ Sách phép** trong trận, hiện đủ mọi kỹ năng **không ổ khoá**, để người
   chơi đọc hết và kéo thả sẵn vào ô trước khi vào trận; **tắt game mở lại vẫn giữ** thứ tự ô.
3. Con mắt khoá góc nhìn "quá sơ sài và còn bị bể hình" → vẽ lại kiểu kinh dị.

**Bố cục** (`ManSanh.TinhBoCucSanh`): thêm `nutDangXuat`, `nutCaiDat` (đầu trang, cách 14s), `nutKyNang` (chỗ cũ). Tên người
chơi + thành tích chừa chỗ cho hai nút (`nutCaiDat.x − 20s`).

**Sách phép xem trước** (`CuaSoSachPhep.MoXemTruoc`, `XemTruoc`, `HienDaMo`): cùng một cửa sổ, cùng hàm vẽ với trong trận,
chỉ khác: mọi kỹ năng hiện đủ màu và **kéo thả được** (trong trận kỹ năng còn khoá không kéo được); không có nút mở khoá /
nâng cấp (thay bằng dòng "Mở khoá và nâng cấp trong trận…"); không hiện số bình; dòng cấp kể **sức mạnh ở cấp tối đa**
(sát thương ×2,07, năng lượng ×1,46, hiệu ứng +0,60 giây). Năng lượng / hồi chiêu / niệm đọc thẳng từ nhân vật trưng bày
`PhuThuy_TrungBay` của màn chính (cùng prefab với trong trận). Sảnh gọi `CapNhat` trong `Update` và `Ve` cuối `OnGUI`;
sảnh bị khoá (`GUI.enabled`) khi sách mở **và tới lúc nhấc ngón tay** sau khi sách đóng — chạm ra ngoài cửa sổ đóng sách
lúc ngón tay đặt xuống, không khoá thì nút sảnh nằm dưới (VÀO PHÒNG NHANH) ăn cú thả tay. Esc đóng. Sách phép là `static`:
đăng xuất, vào phòng hay rời màn (`OnDestroy`) đều đóng, không thì vào trận nó vẫn phủ kín màn hình.

**Giữ thứ tự khi tắt game**: `SachPhep` trước ghi PlayerPrefs — trên WebGL PlayerPrefs xuống IndexedDB **không đồng bộ**, xếp ô
xong đóng tab ngay là mất. Nay ghi **thẳng localStorage** như cài đặt đồ hoạ: thêm `CD_DocChuoi` / `CD_GhiChuoi` vào
`CauNoiCaiDat.jslib` (chuỗi trả về cấp phát bằng `_malloc`, Unity tự giải phóng); đọc localStorage trước, không có thì đọc
PlayerPrefs (bản ghi cũ vẫn dùng được). Hai bộ ô (cảm ứng / máy tính) vẫn riêng.

**Con mắt quỷ** (`python CongCu/Icon/sinh_mat_quy.py` → `Resources/GiaoDien/MatQuy.png`, `MatQuyKhoa.png`, 256², có mipmap):
mi mắt bằng thịt nhăn màu huyết dụ, lòng trắng ngả vàng đầy gân máu, tròng lửa cam vân tia, con ngươi **khe dọc**, máu nhỏ
giọt từ mi dưới. **Đang khoá**: tròng tắt lửa đỏ sẫm, khe ngươi hẹp lại và một **vết chém máu chéo** trên-trái → dưới-phải
(giữ nghĩa "gạch chéo" của bản cũ). Hai ảnh cắt cùng khung nên đổi trạng thái con mắt không nhảy. Bản cũ ghép 17 dải dọc —
ở nút ~43 điểm các dải lệch nửa điểm ảnh nên mép răng cưa; không nạp được ảnh thì vẫn vẽ kiểu cũ.

**Số đo** (menu **66** mới, **0 lỗi**; màn Game 1568×581):

| Đo | Kết quả |
|---|---|
| Chữ có dấu (ManSanh, CuaSoSachPhep, SachPhep — 245 chuỗi) | đủ trong cmap Inter |
| Cầu nối localStorage **chạy thật bằng node** (localStorage giả) | khoá chưa có → rỗng; ghi → 1, đọc lại đúng; chữ có dấu đúng; trình duyệt cấm lưu → đọc rỗng, ghi 0 |
| Kỹ năng hiện đã mở / kéo được | xem trước **9/9**; đối chứng trong trận lúc mới vào **0/9** |
| Độ sáng vành hình (né ổ khoá giữa hình), xem trước ÷ đối chứng khoá | cột trái thấp nhất **×2,66**, ô thấp nhất **×1,94** |
| Thông số ở sảnh | cầu lửa 10 năng lượng / 0,55 giây, thiên thạch 60 = đọc thẳng prefab |
| Xếp ô ở sảnh → kho lưu | máy tính `8,7,3,2,4,5,6`, cảm ứng `0,1,2,3,4,5,8` = tính tay |
| Sách đang mở mà vào Act2 | sách đóng; thanh kỹ năng trong trận `8,7,3,2,4,5,6` |
| Thoát Play, nạp lại từ kho (như mở game lần sau) | vẫn đúng hai bộ ô |
| Con mắt (nút bán kính 21,5 điểm) | mở: tròng cam **59%** (ngoài nút 0%); đường chéo đỏ mở 0% → khoá **42%** |

Lần đo đầu độ sáng hình đo **giữa** hình: ổ khoá sáng của bản đối chứng làm hình tối (Sấm sét, Bình máu) ra ×1,04 dù ảnh
chụp cho thấy rõ bản xem trước đủ màu — đổi sang đo vành 62–92%. Menu 48 cập nhật kiểm vị trí **cả đầu trang** (10 cỡ màn
hình: không đè nhau, chữ vừa nút, tên 16 chữ + thành tích không bị cắt, nút không đè khung tạo phòng) — **0 lỗi**; menu 59
và 21 chạy lại **0 lỗi**. Ảnh `sachphep_sanh_*.png`.

### Màn đếm ngược vào trận: con số kinh dị, bỏ tên màn, dòng chữ lên trên (13/09/2026)

Anh gửi ảnh màn đếm ngược: con số "quá đơn giản" (chữ đỏ phẳng có bóng), không cần tên màn "NGHĨA ĐỊA", và dòng
"TRẬN ĐẤU BẮT ĐẦU SAU" cùng gạch đỏ đè xuống che mất nhân vật.

**Sửa** (`ManSanh.VeDemNguoc`):
- **Con số là ảnh vẽ sẵn** `Resources/GiaoDien/DemNguoc/So0..So9.png` (`python CongCu/DemNguoc/sinh_so_dem_nguoc.py`), cùng
  ngôn ngữ hình với ảnh tên game: font Grenze Gotisch Black, lòng đỏ máu có vân chảy dọc và vết nứt, máu nhỏ giọt từ điểm
  thấp nhất của nét, cạnh trên sáng như lưỡi dao, viền đen, quầng đỏ. Mọi chữ số cùng khung và cùng đường chân nên "10"
  ghép từ "1" và "0" vẫn thẳng hàng. Không nạp được ảnh thì vẫn vẽ số bằng chữ như bản cũ.
- Phía sau số: **vòng phù chú** đỏ (chữ rune + ngôi sao ngược) xoay chậm. Mỗi giây số **đập to rồi co lại và rung**, bốn góc
  màn hình **tối đỏ lại theo nhịp** như tim đập — giữa màn hình vẫn trong, không phủ tối lên cảnh (luật 12/09/2026).
- **Bỏ tên màn.**
- Dòng chữ + gạch đỏ lên **sát mép trên** (`KhungTieuDeDemNguoc`: 4,5% chiều cao màn hình).

**Số đo** (menu 50 thêm mục 7b, **0 lỗi**; màn Game 1568×581): dòng chữ y 26–48, gạch đỏ y 53; đỉnh khung bao nhân vật
(SkinnedMeshRenderer chiếu lên màn hình — khung bao rộng hơn đỉnh đầu thật nên phép so là khắt khe) y 74 → không đè. Trước
đây dòng chữ ở `giữa − 230s` = ngang đầu nhân vật. Ảnh `gd_7_demnguoc.png`.

**Nhỏ bớt 15%** (anh xin sau khi xem bản web): chữ số `CaoSoDemNguoc` 360s → **306s**, vòng phù chú `CoVongDemNguoc`
470s → **400s**. Menu 50 mục 7c đo bề ngang vòng **trên ảnh chụp** (quét 5 hàng giữa, điểm đỏ trội): **204 điểm** trên màn
581 cao — khớp cỡ mới (phần có hình 94% × 215 = 202–216 điểm), cỡ cũ sẽ ra ~238. Lần đo đầu dùng ngưỡng "đỏ rực" thì vòng
vẽ mờ theo nhịp không qua được, chỉ đo trúng con số (72 điểm).

### Bình máu, bình mana, bình rơi chung cả phòng; Sách phép chữ to và cuộn (13/09/2026)

**Anh xin:** chữ cột trái Sách phép to thêm 15%, chữ phần chi tiết to thêm 20%, hai vùng cuộn được (sau này nhiều kỹ năng,
lời kể dài). Thêm hai kỹ năng **Bình máu** và **Bình mana** (vẽ bình hợp phong cách kinh dị): mở khoá 1 điểm, cấp tối đa 1;
chỉ dùng khi đã nhặt bình; giết quái 10% ra bình máu, 10% ra bình mana; tới gần là bình tự bay vào người; số bình hiện trên
ô kỹ năng; một bình hồi tối đa 100 máu / 50 năng lượng; chờ 0,5 giây. Hai câu tôi hỏi lại: **giữ 7 ô** (bình phải kéo vào
ô mới dùng) và **bình của chung cả phòng**.

**Hình vẽ:** `python CongCu/Icon/sinh_binh.py` — bình giả kim bụng tròn, nút bịt là một chiếc sọ nhỏ (hốc mắt cháy đỏ / xanh),
đai sắt gỉ đinh tán; bình máu máu đỏ phát sáng có bọt và máu rỉ ngoài thân; bình mana nước xanh xoáy, khắc ngôi sao ngược.
Hai loại ảnh: `Resources/Icons/BinhMau.png`, `BinhMana.png` (nền đen, cộng lên đĩa nút như các icon khác) và `Resources/VatPham/BinhMau.png`, `BinhMana.png`
(nền trong — tấm phẳng quay theo camera, nhấp nhô, quầng sáng, trên mặt đất).

**Chia bình khi chơi nhiều người** (`QuanLyBinhRoi`): máy trọng tài quái gieo khi quái chết → gói `LoaiBinhRoi` (10 byte, số
hiệu + loại + vị trí) → máy nào có nhân vật tới gần 3,5 m thì xin (`LoaiXinBinh`) → **chủ phòng giao cho người xin TRƯỚC** và
bỏ mọi lời xin sau (`LoaiBinhThuoc`) → mọi máy thấy bình bay vào đúng người, chỉ máy người ấy cộng. Không để mỗi máy tự thấy
mình gần là tự nhặt: hai người đứng hai bên một bình sẽ cùng nhặt — một bình thành hai. Kênh chạy như UDP nên cả ba gói gửi
lặp 3 lần; gói "thuộc về" đến trước gói "rơi ra" thì ghi nhớ, bình hiện ra là giao ngay.

**Uống** (`PlayerController.UongBinh`): không niệm, không ngắm, không đi qua gói kỹ năng (máu của mình do máy mình quyết, gói
trạng thái mang sang); uống được cả khi đang niệm phép khác. Chưa mở / hết bình / máu đầy / đang hồi chiêu / bị đóng băng,
choáng, ngã → không uống và nói rõ vì sao — uống mất bình mà không hồi được gì thì người chơi mất trắng. Số bình vẽ thành huy
hiệu góc phải dưới ô (nút tròn, ô vuông, ô trong Sách phép), hết bình thì ô tối đi.

**Sách phép:** hệ số `HeSoChuKho = 1,15` (hàng cao theo), `HeSoChuThan = 1,20`. Chi tiết tách **đầu mục đứng yên** (hình, tên,
thông số) và **thân cuộn** (dòng cấp + lời kể) — cuộn bằng con lăn chuột và vuốt dọc ngón tay; hai vùng có thanh cuộn cam khi
nội dung dài hơn vùng. Trước đây cả khung chi tiết cuộn chung (tên cũng trôi đi) và chỉ bằng con lăn — điện thoại không cuộn được.

**Số đo** (menu 65, **0 lỗi**; menu 59, 60, 61 chạy lại vẫn 0 lỗi):

| | Kết quả |
|---|---|
| 3000 con quái chết | 282 bình máu (9,4%), 290 bình mana (9,7%) |
| Giết thật 24 con qua `GameDirector` | rơi 6 bình, 6/6 nằm đúng chỗ quái chết (≤ 1 m) |
| Bình cách 6 m / đưa vào 2,5 m | nằm yên / bay vào người sau 0,33 giây, số bình +1 |
| Chưa mở khoá, có bình | không uống được |
| Mở khoá | cấp 1/1, không nâng cấp được |
| Thiếu 250 máu / thiếu 30 / đầy | hồi 100 / hồi 30 / không uống, không mất bình |
| Uống liền / sau 0,3 s / sau 0,6 s | từ chối / từ chối / uống được |
| `CastAt(bình mana)` thiếu 120 · thiếu 20 · hết bình | +50 · +20 · từ chối |
| Số bình trên ô (3 → 12), ô bình / ô đối chứng | máy tính 0,051 / 0,000; cảm ứng 0,043 / 0,000 |
| Chữ cột trái (bề ngang nét) | 65 điểm = chữ đối chứng cỡ mới 65 (cỡ cũ 54) |
| Chữ thân chi tiết | 184 điểm ≈ đối chứng cỡ mới 185 (cỡ cũ 167) |
| Cuộn thân chi tiết (cảm ứng) | nội dung 128 / vùng 112 → kẹp 20; ảnh thân đổi 0,104, đầu mục 0,000 |
| Mạng — máy khách | gói rơi lặp 3 lần → 1 bình; tới gần gửi xin, **chưa bay** tới khi chủ phòng giao; giao cho mình +1 |
| Mạng — bình giao cho người khác | bay vào họ (cách 0,31 m lúc biến mất), mình không cộng |
| Mạng — gói "thuộc về" đến trước "rơi ra" | vẫn nhận đúng bình |
| Mạng — chủ phòng | gửi 3 gói rơi; ghế 1 xin trước, ghế 2 xin sau → giao ghế 1 (3 gói), ghế khác 0 |
| Mạng — chủ phòng tự tới gần | tự nhặt +1, báo cả phòng, không gửi lời xin |

**Những lần phép thử sai trước khi đúng:** giữ trần máu từ *trước* khi lên cấp (600 thay vì 793,5) nên "thiếu 30" thật ra
thiếu 223; hộp chữ đối chứng đè lên viền giọt máu của cửa sổ; đặt bình vào ô ngay khung hình vừa bật chế độ cảm ứng (vào nhầm
bộ ô vuông); số bình đã là 12 từ lượt trước nên hai ảnh giống hệt; huy hiệu số dùng kiểu chữ có lề → "12" tràn khung (sửa ở
game). Menu 59 phần F2 (vành ô đang chọn) nay lấy **trung bình 4 khung** — quầng sáng đập nhịp, chụp một khung thì ra 1,65 lần
rồi 1,44 lần tuỳ pha.

⚠️ Còn thiếu: màn Editor cao, cột trái 9 hàng vẫn vừa (cả màn 640×300) nên **thanh cuộn cột trái chưa được đo bằng ảnh** — mã
cuộn của nó có từ trước và dùng chung hàm thanh cuộn với thân chi tiết (đã đo). Mạng mới thử trên kênh giả lập.

### Sách phép: cửa sổ to hơn, dòng / ô đang chọn sáng lên, ổ khoá trong ô (13/09/2026)

Anh vẽ một khung xanh trên ảnh chụp (màn 1560×572) và xin: cửa sổ to bằng khung ấy; cột trái — dòng đang chọn phải
**sáng hẳn**, kỹ năng đã mở / nâng cấp nổi bật hơn kỹ năng còn khoá; vùng ô — chạm ô nào thì ô ấy hiện là đang chọn, ô giữ
kỹ năng chưa mở vẫn có **ổ khoá như ngoài trận**.

**Sửa:**

- **Kích thước** `1180s × 720s → 1640s × 836s` (vẫn giữ trần 94% × 90% màn hình để thấy trận đấu phía sau). Khung xanh anh vẽ
  đo được ~865 × 442 điểm = 1640s × 836s với s = 572/1080.
- **Cột trái, ba mức sáng:** đang chọn = lòng đỏ rực (đập nhẹ), viền sáng hai lớp, chữ gần trắng; đã mở = lòng ấm vàng đồng,
  chữ vàng, vạch trái vàng; còn khoá = lòng tối hẳn, chữ mờ, hình tối kèm ổ khoá. Trước đây dòng đang chọn chỉ thêm một lớp
  đỏ 28% và dòng khoá bị phủ tối 45% — chọn một kỹ năng còn khoá thì dòng ấy tối y như các dòng khác.
- **Một lỗi cũ lộ ra:** khung quanh hình ở cột trái gọi `GiaoDien.DuongKe` — hàm vẽ *đường kẻ ngang mờ hai đầu* — bị kéo giãn
  phủ kín cả hình thành **một dải nâu cam dọc giữa biểu tượng**, và đè mất luôn ổ khoá. Thay bằng viền mảnh, ổ khoá vẽ sau cùng.
- **Vùng ô:** ô có kỹ năng sáng theo *kỹ năng đang xem* — chọn ở cột trái cũng sáng luôn ô đang giữ nó, kéo đổi chỗ thì quầng
  sáng đi theo; chạm vào ô *trống* thì chính ô trống ấy sáng. Quầng sáng cam đập nhẹ phía sau + viền vàng dày.
  Kỹ năng chưa mở trong ô: nhân màu (0,38; 0,36; 0,40) và ổ khoá — **đúng màu, đúng tỉ lệ** với cụm nút / thanh ô ngoài trận
  (`GameHUD.RongKhoaTron`, `RongKhoaVuong`).

**Số đo** (menu 59 phần F, đo độ sáng điểm ảnh trên **ảnh chụp**, cả bản cảm ứng lẫn máy tính, **0 lỗi**; phần A–E cũ vẫn 0 lỗi):

| | Cảm ứng | Máy tính |
|---|---|---|
| Cửa sổ trên màn 1560×572 | 869 × 443 (bản cũ 625 × 381) | — |
| Chọn kỹ năng **còn khoá**: dòng chọn / dòng khác sáng nhất | 0,314 / 0,182 (×1,73) | 0,291 / 0,182 (×1,60) |
| Chọn kỹ năng **đã mở** | 0,357 / 0,163 (×2,19) | 0,344 / 0,163 (×2,11) |
| Dòng đã mở / dòng khoá | 0,163–0,182 / 0,054–0,060 (~×3) | như bên trái |
| Ổ khoá ở dòng còn khoá (điểm sáng > 0,5 trong hình) | 18 | 18 |
| Vành ô đang chọn / vành ô khác sáng nhất | 0,167 / 0,101 | 0,315 / 0,128 |
| Chạm ô trống: vành ô trống / ô vừa chọn trước | 0,176 / 0,107 | 0,330 / 0,132 |
| Ô giữ Khiên: độ sáng lúc khoá / sau khi mở khoá | 0,219 / 0,796 | 0,243 / 0,828 |
| Ổ khoá trong ô (điểm sáng > 0,5) | 8 | 23 |

Hai phép đo lần đầu sai và đã sửa: "xám đi" ngoài trận là **nhân màu** nên làm *tối* chứ không giảm độ bão hoà (đo bão hoà
ra 0,39 vs 0,40, báo nhầm "không xám") → đo độ sáng; tìm ổ khoá bằng màu đỏ của lỗ khoá thì lỗ quá nhỏ (bản cảm ứng ra 0) →
đếm điểm sáng > 0,5: hình kỹ năng đã bị nhân ≤ 0,40 nên điểm sáng ấy chỉ có thể là quai / đinh tán của ổ khoá.

### Android: ứng dụng đã cài vẫn không kín màn hình (13/09/2026)

Anh chụp ba ảnh trên điện thoại Android (đã cài game thành ứng dụng web): còn **dải đỏ có đồng hồ** (thanh trạng thái),
**dải trắng** (thanh điều hướng) và **dải đen** ở chỗ camera — trong khi iPhone và iPad kín màn hình.

**Nguyên nhân:** `web/manifest.webmanifest` xin `"display": "standalone"`, và `display_override` cũng xếp `standalone`
lên đầu. Ở chế độ standalone Android **luôn giữ** hai thanh hệ thống. iOS kín màn hình là nhờ thẻ riêng của Apple
(`apple-mobile-web-app-status-bar-style = black-translucent` + `viewport-fit=cover`) — Android không đọc thẻ đó, nên
thử trên iPhone thì không bao giờ thấy lỗi này.

**Sửa hai lớp:**

1. Manifest: `"display": "fullscreen"`, `display_override: ["fullscreen", "standalone", "minimal-ui"]` — đường chính thức,
   Android ẩn cả hai thanh và dùng luôn chỗ camera (trang đã có `viewport-fit=cover`).
2. Dự phòng trong trang bao game (`WebGLTemplates/Diablo25D/index.html`): **Android + đang chạy như ứng dụng đã cài +
   chưa toàn màn hình** → cú chạm đầu tiên gọi Fullscreen API với `navigationUI: "hide"`. Cần lớp này vì ứng dụng
   **đã cài** chỉ nhận manifest mới khi Chrome tự kiểm lại (thường tới một ngày). Tab trình duyệt thường và iOS không bị
   đụng tới. Nhận biết "đã cài" nay tính cả `display-mode: fullscreen` (trước chỉ `standalone` → sau khi đổi manifest,
   nút "CÀI ĐẶT ỨNG DỤNG" sẽ hiện nhầm trong chính ứng dụng đã cài).

**Số đo** (bản build chạy trên máy, không có máy Android để thử thật):

| | Kết quả |
|---|---|
| Hàm quyết định, 6 tổ hợp (Android đã cài / chưa cài / đã toàn màn hình / không có API, iPhone, máy tính) | 6/6 đúng |
| Chạm thật khi **chưa cài** | gọi Fullscreen API **0** lần |
| Giả lập Android đã cài, chạm thật | gọi **1** lần, tuỳ chọn `{"navigationUI":"hide"}` |
| Một cú chạm điện thoại (`pointerup` + `touchend` liền nhau) | 1 lần (khoá 1 giây — không có khoá thì 2 lần) |
| Game vẫn vào màn đăng nhập | có |

Khung trình duyệt dùng để thử không cho vào toàn màn hình thật, và chặn bộ chạy nền trên địa chỉ cục bộ (3–4 dòng lỗi
"fetching the script" kể cả khi đã chép `sw.js` vào — máy chủ không nhận yêu cầu nào); trên trang thật https cùng mã
đăng ký ấy vẫn 0 lỗi. **Phải thử lại trên chính điện thoại Android của anh.**

### Quái vòng ngoài tự truy lùng sau 60 giây, và cấp tối đa 20 (13/09/2026)

**Anh xin:** 20 con quái vòng ngoài của mỗi đợt, nếu sau 60 giây vẫn chưa tìm thấy người chơi, thì tự biết người chơi
gần nhất ở đâu và chạy tới đánh — tránh cảnh còn vài con trên bản đồ mà tìm mãi không thấy. Và nâng cấp tối đa lên 20.

**Vì sao trước đây chúng không tới:** quái chỉ đuổi khi người chơi vào trong `aggroRange` = **14 m**, mà quái vòng ngoài
được thả ở **20–25 m** — nên đứng lang thang mãi. Sửa: lúc thả, `GameDirector.SinhQuaiXa` hẹn giờ cho từng con
(`EnemyAI.HenTruyLung(60)`); hết giờ con đó bỏ giới hạn 14 m và đuổi người **gần nhất còn sống**. Áp cho mọi con vòng
ngoài còn sống, kể cả con đã gặp người chơi rồi bị bỏ lại xa — nó cũng là con "tìm mãi không thấy". Chỉ máy trọng tài
quái chạy AI nên không cần gói tin mới.

**Chỗ tôi suýt báo xong quá sớm.** Lần chạy thử đầu 20/20 con tới được. Lần thứ hai, với vị trí thả ngẫu nhiên khác,
**4/20 con đứng kẹt 0,0 m suốt 5 giây** cách người chơi 8–17 m: quái đi **đường thẳng** (không có NavMesh), vướng cây,
nhà mồ, hoặc bị chính đám quái khác chắn. Chính là cái lỗi anh muốn tránh. Nên thêm hai tầng cứu, **chỉ cho con đang
truy lùng**:

1. Mỗi giây so quãng **thật** đã đi với quãng **muốn** đi; đi chưa tới 35% là kẹt → quét 7 hướng, lấy hướng thông 2,5 m
   (còn đất, không hụt độ cao) gần hướng người chơi nhất, đi theo 1,5 giây.
2. Kẹt 4 lần mà khoảng cách không giảm được 1 m nào → **đổi chỗ** sang chỗ trống cách người chơi 10–14 m, cùng độ cao,
   nhìn thẳng thấy người chơi. Người chơi chạy trốn thì quái vẫn đi được (không tính kẹt) nên không bị dịch chỗ theo.

Lần chạy thứ ba lộ thêm một chỗ: **quỷ cây 10,6 m và phù thủy 13,8 m** đứng sau đám đông, không bắn phát nào — điều
kiện "sát người chơi thì không tính kẹt" cộng thêm 3 m cho cả quái đánh xa. Sửa: quái đánh xa kẹt mà đã trong 1,5 lần
tầm đánh thì **bắn tại chỗ**.

**Số đo** (menu 64, **0 lỗi**, dùng đợt quái THẬT director tự thả ở giây 30):

| Phần | Kết quả |
|---|---|
| Hẹn giờ | 20/20 con hẹn đúng 60,000 giây từ lúc sinh |
| Trước 60 giây | các con chưa gặp người chơi: trung bình tiến lại **−1,1 m** (tức là không đuổi) |
| Sau 60 giây | **20/20** con vào tầm 14 m (chậm nhất 6,1 giây, trung vị 1,6 giây); **20/20 con ra đòn thật** (sự kiện `DaRaDon`) |
| Đối chứng | 2 bộ xương thường đặt 41–43 m: 90 giây vẫn cách 32–41 m, không ra đòn |
| Kẹt sau cây / nhà mồ (tia thẳng bị chặn) | vòng 1 lần, tới sát 2,4 m |
| Nhốt trong 4 bức tường tạm | vòng 6 lần không thoát → **đổi chỗ 1 lần** → tới sát 2,4 m |

Menu 56 chạy lại vẫn 0 lỗi (28 con đợt đầu, 20/20 con vòng ngoài đúng 20–25 m).

**Cấp tối đa 20.** `CapDo.CapToiDa = 20`, bảng kinh nghiệm thêm 10 bậc. Giữ 1,35 lần mỗi bậc thì lên cấp 20 cần ~62 000
kinh nghiệm (chơi một mình ~22 đợt); tôi cho cấp 10–20 chỉ nặng thêm **1,15 lần** mỗi bậc → tổng **29 465**, chơi một mình
khoảng **16 đợt**. Cả trận có **20 điểm kỹ năng** (mở hết 7 kỹ năng và nâng hết lên cấp 5 cần 35 — vẫn phải chọn).
Chỉ số mỗi cấp giữ nguyên nên cấp 20: máu ×14,2, năng lượng ×6,1, **tốc độ ×1,92** (~10 m/giây, quái nhanh nhất 5,98) —
đã ghi cảnh báo trong `kinhnghiem.md`. **Số đo** (menu 60, **0 lỗi**): bảng đủ 19 bậc tăng dần, cộng 999 999 → cấp 20 và 20
điểm, hệ số máu cấp 20 = 1,15¹⁹.

**Tốc độ chỉ tăng tới cấp 10** (anh chốt sau khi đọc cảnh báo trên). Tốc độ được nhân ở **hai** chỗ — công thức
`CapDo.HeSoTocTheoCap` và `PlayerController.LenCap` (nhân thẳng `moveSpeed` mỗi lần lên cấp) — nên chặn cả hai bằng một
hằng `CapDo.CapTangTocToiDa = 10`; chặn một chỗ thì chỗ kia vẫn tăng tiếp mà phép thử công thức vẫn xanh. **Số đo** (menu 60,
**0 lỗi**) trên nhân vật THẬT lên thẳng cấp 20: tốc độ 5,20 → **7,09** m/giây (×1,363 = 1,035⁹), máu vẫn ×14,23 (= 1,15¹⁹).

### Người chơi mới khởi động ở mức đồ hoạ "Yếu" (13/09/2026)

Anh xin: vào game thì mặc định ở **Cài đặt › Giao diện: Yếu**. Tôi hỏi lại vì có hai cách hiểu, anh chọn
**chỉ người chưa từng chọn**: ai chưa bao giờ bấm OK trong Cài đặt thì khởi động ở Yếu (cảnh 62%, bóng cứng gần,
giảm chi tiết xa); ai đã tự chọn mức khác thì lần sau vào vẫn giữ mức đó. (Cách còn lại — lần mở game nào cũng về
Yếu — sẽ phá chính nút Cài đặt, vì bấm OK là tải lại trang.)

Sửa một dòng trong `CaiDatDoHoa.DocTuKho`: không có khoá mới lẫn khoá cũ thì trả `MacDinh = Yeu` thay vì Cao. Mức mặc
định **không** ghi xuống kho — "chưa chọn" vẫn là chưa chọn. Người có khoá cũ ba mức vẫn chuyển như trước.

Menu 48 cũng phải sửa: trước đây nó **đặt khoá = Cao rồi gọi đó là "người chơi mới"**, tức không hề đi qua nhánh
"chưa từng chọn". Nay nó xoá cả hai khoá thật. **Số đo** (menu 48, **0 lỗi**): kho trống → vào sảnh ở **Yếu**, mức
Unity **Low**, bóng HardOnly, chi tiết xa 0,55, kho vẫn không có khoá; khoá cũ 0 / 1 / 2 → Cao / Trung bình / Rất yếu
như cũ; bốn lần chọn Trung bình → Yếu → Rất yếu → Cao vẫn tải lại và áp đúng từng mức. Ảnh `caidat_2_bang.png`
hiện "Yếu (hiện giờ)".

### Nút Sách phép: chữ bị cắt, và quyển sách vẽ lại (13/09/2026)

Anh gửi hai ảnh: chữ **"Sách phép"** dưới nút bị cụt mất nửa dưới (chữ "p" chỉ còn phần trên), và quyển sách
"vẽ rất sơ sài" — bản cũ chỉ là 4 hình chữ nhật (bìa, gáy, mép giấy, một hình thoi đỏ).

**Chữ bị cắt — nguyên nhân thật:** khung chữ cao `22s` cho chữ cỡ `15s`, dùng kiểu `GUI.skin.label`. Kiểu này có
**lề trên/dưới tính bằng điểm ảnh cố định**, không co theo màn hình. Màn 1080 thì lề nhỏ so với khung, chữ vừa đủ;
màn càng thấp thì lề ăn càng nhiều phần khung. Điện thoại nằm ngang chơi trên web chỉ cao khoảng **390 điểm ảnh**
(trang không đặt `devicePixelRatio`), đúng trường hợp của anh. **Sửa:** bỏ lề, khung cao 1,9 lần cỡ chữ,
`TextClipping.Overflow`, chữ tối thiểu 8 điểm, thêm bóng đen để đọc được trên nền đất sáng.

**Quyển sách:** ảnh vẽ sẵn `Assets/Resources/GiaoDien/SachPhep.png` (256×256, không nén, có mipmap), sinh bằng
`python CongCu/Icon/sinh_sach_phep.py`: bìa da thuộc màu huyết dụ nứt nẻ, gáy có đai nổi, bọc góc đồng đen đinh tán
gỉ xanh, vòng phù chú khắc chữ rune bao ngôi sao năm cánh ngược phát sáng đỏ, đầu lâu ở giữa với hai hốc mắt
cháy đỏ, dây da khoá bên phải, mép giấy ngả vàng và vệt máu rỉ từ mép trên. Không nạp được ảnh thì nút vẫn vẽ
bản hình chữ nhật cũ, không biến mất.

**Phép đo** (menu 59 thêm phần E, **0 lỗi**). Đo trên **ảnh chụp màn hình**, không tin khung của HUD:

| Cỡ màn (hệ số s) | Chữ kiểu CŨ: nét thật / nhãn chuẩn cùng cỡ | Chữ kiểu MỚI: nét thật / nhãn chuẩn |
|---|---|---|
| 0,36 — điện thoại ngang trên web | **3 / 5** — mất gần nửa | 9 / 9 |
| 0,53 — màn Game của Editor | **7 / 9** | 9 / 9 |
| 1,00 | 16 / 16 | 16 / 16 |
| 2,00 | 31 / 31 | 31 / 31 |

"Nhãn chuẩn" là cùng chữ, cùng cỡ, vẽ trong khung cao gấp 10 lần — không thể bị cắt. Lần chạy đầu tôi so với
chiều cao tính từ **bảng glyph** của font và phép thử báo nhầm kiểu mới "bị cắt": bảng glyph lệch nét render thật
khoảng 2 điểm ảnh (làm tròn + khử răng cưa). Nó cũng cho thấy vì sao lỗi không lộ khi thử trên màn to: ở s = 1 kiểu
cũ vẫn đủ nét.

Trong trận (cả bản cảm ứng lẫn máy tính): chữ dưới nút **9 / 9** điểm, lòng nút có **59 màu** khác nhau trong ô
15×15 điểm (quyển sách có chi tiết thật, không còn là khối màu trơn). Ảnh cận cảnh: `PlayTestShots/sachphep_nut_moi.png`.

### Quái vòng ngoài: 20 con ở 20–25 m (13/09/2026)

Anh đổi luật: số quái thêm mỗi đợt thành **20 con** (thay vì 10), ở **20–25 m** (thay vì 55–65 m); quái sát bên người
chơi giữ nguyên. Chỉ đổi ba hằng số trong `GameDirector` (`SoQuaiXaMoiDot`, `QuaiXaGanNhat`, `QuaiXaXaNhat`) — cách
chọn chỗ vẫn như cũ: tính theo người chơi **gần nhất**, trên mặt đất, ngoài nước, không vướng vật cản.

Phép thử (menu 56) phải sửa một chỗ trước khi đo: ở 20–25 m quái **phát hiện người chơi tức thì** và chạy tới; phép thử
đợi nửa giây mới đo nên đo nhầm chỗ quái *đã chạy tới* chứ không phải chỗ nó được thả. Nay khoá chân quái ngay khung
hình vừa thả.

**Số đo** (menu 56, 0 lỗi, hai người chơi): đợt 1 **28 con** = 2 × 4 + 20; đợt 2 · 3 · 4 = 29 · 31 · 34 con; cả 4 đợt
**20/20 con vòng ngoài đúng 20–25 m** (đo 20,0–25,0 m từ vị trí thật), 0 dưới nước, chênh mặt đất tối đa 0,15 m; quái
quanh người vẫn 7–13 m; chờ đợt đầu vẫn 30,0 giây; Act1 không đổi.

⚠️ Nhịp lên cấp đổi nhiều: chơi một mình giết hết thì **~5 đợt** chạm cấp 10 (trước ~12). Bảng mới trong `kinhnghiem.md`.

### "Bạn chưa thử với nhân vật khác, mới chỉ thử với quái" — và đó đúng là chỗ hỏng (13/09/2026)

Anh chỉ ra đúng lỗ hổng: menu 62 thử người chơi bằng cách **gọi thẳng** hiệu ứng ngã và hàm vẽ lại trên bản sao —
bỏ qua đúng hai bước dễ hỏng nhất khi chơi mạng: quả thiên thạch đi qua **gói kỹ năng**, và cờ "đang ngã" đi qua
**gói trạng thái** (đóng gói → mở gói).

**Menu 63** dùng bộ đồng bộ thật (`DongBoTran`) và kênh giả lập như menu 37, chạy trên code chưa sửa trước:

| | Kết quả trên code cũ |
|---|---|
| **Mình là nạn nhân** — người kia tung Thiên thạch bằng gói kỹ năng thật | ✔ trúng 10/10, nhân vật mình ngã 8/10, hình nằm ngửa thật (đo xương đầu), có dấu hiệu |
| **Mình là người xem** — máy người kia báo "tôi đang ngã" bằng gói trạng thái thật | ✘ **bản sao không hề nhận được** — không nằm, không dấu hiệu |
| Đóng gói rồi mở gói | ✘ bit "đang ngã" **mất** ở cả gói người chơi lẫn gói quái |

**Nguyên nhân:** byte cờ trong gói tin chỉ dành **3 bit** cho hiệu ứng (`coHieuUng & 0x07`) — đóng băng, đóng cứng,
choáng. Cờ "đang ngã" tôi thêm là **bit thứ tư**, và mặt nạ 0x07 cắt bỏ nó mỗi lần đóng gói. Người bị ngã nằm trên
máy *của họ*, còn máy của những người khác không bao giờ biết. Gói quái cũng vậy: chơi với tư cách **khách** (chủ phòng
quyết định quái) thì đánh ngã quái cũng không thấy — giải thích vì sao phép thử một máy của tôi thấy ngã mà anh thì không.

**Sửa:** nới mặt nạ lên **4 bit** (0x0F) ở cả hai loại gói. Các bit ấy vốn còn trống nên **gói không to thêm** (vẫn 18 byte).

**Số đo** (menu 63, **6 lỗi → 0**): bit ngã qua được cả hai gói, đủ 4 hiệu ứng cùng lúc (15/15) mà "đang chạy / đã
chết" vẫn nguyên; mình là nạn nhân ngã 9/10, nằm ngửa thật 9/9, có dấu hiệu 9/9; **bản sao người kia nhận ngã, nằm ngửa
thật, có dấu hiệu, và đứng dậy khi gói ngừng**. Menu 46 chạy lại vẫn 0 lỗi.

### Luật đợt quái Act2: chờ 30 giây, và thêm 10 con ở xa

- **Vào trận chờ đúng 30 giây** mới ra đợt đầu (trước đây 1,5 giây chơi một mình / 6 giây chơi mạng).
- **Mỗi đợt thêm cố định 10 con**, loại ngẫu nhiên, cách **người chơi gần nhất** 55–65 m. Tính theo người
  gần nhất: thả cách người A 60 m mà ngay cạnh người B thì với B đó là quái sát bên. 10 con này vẫn mạnh thêm
  5% mỗi đợt, vẫn tính vào "giết hết mới sang đợt sau", và nhận người gần nhất làm mục tiêu nên tự chạy tới.
- Chỉ Act2. Act1 giữ nguyên như anh dặn lần trước.

⚠️ **Bản đồ Act2 chỉ rộng 109 × 109 m** (đo từ va chạm mặt đất). Người đứng gần tâm thì vành 55–65 m chỉ còn
ở bốn góc bản đồ; bốn người tản khắp nơi thì có khi không còn chỗ nào cách **tất cả** họ đủ 55 m. Lúc ấy vẫn thả
đủ 10 con, ở chỗ hợp lệ **gần khoảng 55–65 m nhất có thể**, và đếm số con đạt đúng khoảng để phép thử đọc — không
im lặng giả vờ là đạt. Chỗ thả luôn trên mặt đất, ngoài nước, không vướng vật cản.

**Số đo** (`PlayTestShots/dotquai_act2.txt`, menu 56, 0 lỗi, hai người chơi):

| Đo | Kết quả |
|---|---|
| Chờ đợt đầu | đồng hồ đếm ngược + thời gian đã trôi = **30,0 giây**, lúc ấy chưa có đợt nào |
| Đợt 1 | **18 con** = 2 người × 4 loại + 10 con xa |
| Đợt 2 · 3 · 4 | 19 · 21 · 24 con (cộng dồn +1, +3, +6, cộng 10 con xa) — quái mạnh ×1,050 · ×1,103 · ×1,158 |
| 10 con xa, cả 4 đợt | **10/10 đúng khoảng 55–65 m** (54,7–64,8 m đo từ vị trí thật), 0 dưới nước, chênh với mặt đất tối đa 0,18 m |
| Act1 | vẫn luật cũ (4 bộ xương) |

Lần chạy đầu phép thử báo cả 10 con "lơ lửng": tia chiếu từ trên xuống chạm **đỉnh đầu con quái** trước khi chạm
đất. Nay tia bỏ qua chính con quái.

---

## Phần 4 — Menu công cụ "Diablo 2.5D"

| Mục | Tác dụng |
|-----|----------|
| **1. Nuong Asset + Dung Man Choi** | Tạo lại **toàn bộ** Textures / Materials / Models / Prefabs và dựng lại 3 scene. Dùng khi bạn sửa code tạo hình và muốn cập nhật prefab. |
| **2. Mo Man Choi (Open Scene)** | Mở nhanh scene Act1 |
| **3. Tu kiem tra (Self Test)** | Dựng thử mọi thứ + kiểm tra shader, báo lỗi nếu có |
| **4. Chay thu va chup hinh (Play Test)** | Tự chạy game, tự tung phép, chụp ảnh vào thư mục `PlayTestShots` |
| **5. Dung Scene Trong** | Tạo scene rỗng chỉ có vật thể `GAME` (kiểu cũ: mọi thứ sinh bằng code lúc chạy) |
| **6. Chup man hinh chinh** | Chụp riêng màn hình MainMenu |
| **7. Kiem tra hieu ung con sot** | Tự bắn phép rồi đếm xem có hiệu ứng nào không tự biến mất |
| **8. Dung lai dia hinh** | Xoá hết nét tô tay trên mặt đất rồi sinh lại từ công thức |
| **10. Ve lai dia hinh Act2** | Cộng lớp gồ ghề Perlin lên mặt đất Act2 và dời toàn bộ đồ đạc theo. Mở scene Act2 trước khi bấm. |
| **11. Nuong rieng prefab Quy du** | Nướng lại riêng `Enemy_QuyDu` từ model Meshy, không đụng gì khác. |
| **12. Nuong rieng prefab Quy cay** | Nướng lại riêng `Enemy_QuyCay` từ model Meshy. |
| **13. Nuong rieng prefab Loc xoay** | Nướng lại riêng `Skill_LocXoay`. |
| **14. Chong o vuong den - Act2** | Tách những mặt phẳng nằm sát đất ra khỏi mặt đất, hết ô vuông đen. |
| **15. Dung lai anh vo cay Act2** | Sinh lại bộ ảnh vỏ cây sần sùi và gán vào vật liệu. |
| **16. Dung lai anh da mo Act2** | Sinh lại bộ ảnh đá mộ (rêu, mốc, máu) và gán vào ba vật liệu đá. |
| **17. Sut me mot phan bia mo - Act2** | Cắt sứt mẻ thật ở mức lưới cho 28% số bia. |
| **17b. Tra bia mo ve nguyen ven** | Gỡ mọi bản vỡ, trả bia về lưới gốc. |
| **18. Chay thu CAY CHAY - Act2** | Thả một quả thiên thạch vào một cái cây rồi đo cả vòng đời cháy → biến mất → mọc lại. Kết quả ghi ra `PlayTestShots/cay_chay_Act2.txt`. |
| **18b. Chay thu CAY CHAY - Act1** | Như trên nhưng trên Act1 — cây Act1 dựng bằng code thành 88 mảnh con, khác hẳn cây Act2. |
| **18c. Chay thu TRAN so cay chay cung luc** | Châm lửa 12 cây một lúc để kiểm cái trần 6 cây và đo khung hình. |
| **18d. Chay thu CAY CHAY bang PHEP THAT** | Như 18 nhưng người chơi tự bấm phép Thiên thạch — chạy đúng đường mà người chơi đi. |
| **19. Chay thu HOI SINH sau Loc xoay** | Thả một con lốc, đo xem cảnh vật bị cuốn có mọc lại đúng 30 giây không. |
| **20. Nuong diem moi lua cho cay** | Rải 340 điểm mồi lửa trên mỗi loại lưới cây và lưu vào `Resources/DiemLua`. Chạy lại nếu đổi mẫu cây Act2. |
| **21. Chay thu NUT KHOA GOC NHIN** | Bấm nút con mắt rồi thử đẩy camera bằng mọi đường, đo xem góc nhìn có nhúc nhích không. |
| **22. Chay thu THANH KY NANG (PC)** | Chụp thanh kỹ năng ở chế độ PC rồi đếm pixel chữ dưới từng ô — dùng để kiểm rằng dưới ô chỉ còn phím tắt. Trả lại scene đang mở khi xong. |
| **26. Chay thu MANG - dang nhap va phong cho** | Chạy thật trên Firebase: đăng nhập, tạo phòng, đọc danh sách, đổi màn, đếm ngược, người thứ hai bị từ chối vào phòng đang đếm. Luôn dọn phòng đã tạo. Kết quả ra `PlayTestShots/mang_sanh.txt`. |
| **27. Chay thu MANG - khoa tai khoan** | Kiểm rằng tài khoản bị admin khoá trên web thì không vào được game, còn tài khoản bình thường vẫn vào được. Kết quả ra `PlayTestShots/mang_khoa.txt`. |
| **28. Chup man DANG NHAP va SANH PHONG** | Chụp ba màn hình thật của phần mạng ra `PlayTestShots/mang_man_*.png`. Xoá phiên đăng nhập cũ trên máy này (lần sau phải gõ lại mật khẩu) và luôn dọn phòng đã tạo. |
| **29. Xuat ban WEBGL** | Xuất bản bản chơi trên trình duyệt ra `Build/WebGL` (khoảng 11 phút). Kết quả và dung lượng từng file ghi ra `PlayTestShots/build_webgl.txt`. **Xoá sạch `web/Build/`**, chép `Build/WebGL/Build/*`, `Build/WebGL/TemplateData/*` và `Build/WebGL/index.html` sang `web/`, rồi `firebase deploy --only hosting` là lên mạng. |
| **30 / 30b. Bom input Act2 · Act1** | Bước 1 giai đoạn 2: bơm một chuỗi ý muốn vào `PlayerController` rồi đo quãng đường đi được, đối chiếu với lý thuyết. Phải chạy cả hai vì hai màn dựng khác hẳn nhau. |
| **31. Chay thu DU DOAN va HIEU CHINH** | Bước 2: chạy 120 gói input, đặt lại trạng thái rồi chạy lại từ gói N+1, đo độ lệch giữa hai lần — phải là 0. |
| **32. Chay thu NHIEU NGUOI mot canh** | Bước 3: sinh thêm nhân vật thứ hai rồi đếm xem quái chia nhau ra nhắm hai người hay dồn cả vào một. |
| **33. Chay thu NOI SUY** | Bước 4: đo độ dày của đệm co giãn ở mạng tốt và mạng 4G, và số lần giật khi ép đệm mỏng. |
| **34. Chay thu PVP** | Bước 5: bốn chiều — chơi đơn không tự thiêu, chơi đối kháng đánh được nhau, đánh người khác họ mất máu, đánh chính mình thì không. |
| **35. Chay thu GHEP PHONG (cung man)** | Kiểm rằng hai người bấm "Vào phòng nhanh" cùng lúc thì vẫn về chung một phòng và chung một màn. Không nối mạng — đo tính chất của `PhongMang.PhongDuocGiu` (đối xứng, luôn là một trong hai, phòng tạo trước thắng) trên 2 515 cặp. Kết quả ra `PlayTestShots/ghepphong.txt`. |
| **36. Chay thu TU GAN bo noi mang** | Đi đúng đường người chơi đi: vào Play ở MainMenu, bật `DangChoiMang`, nạp màn chơi, rồi **đếm** xem bộ nối mạng có được dựng dậy không — trên **cả hai màn**. Kết quả ra `PlayTestShots/tugan.txt`. |
| **37. Chay thu KY NANG qua mang** | Tám chiều: gói kỹ năng khứ hồi, người kia tung phép thì mình mất máu, người tung không tự thiêu, gửi lại gói cũ không nổ lần hai, máu nhận từ mạng được áp đúng, mình tung thì có gói đi ra, và khiên của người kia hiện ra bên này. Dọn sạch quái trước khi đo. Kết quả ra `PlayTestShots/kynang_mang.txt`. |
| **38. Chay thu QUAI CHUNG va BU TRE** | Mười hai chiều: ai được rải quái, mọi con đều có số hiệu, gói quái khứ hồi, băng thông cả đàn, lịch sử vị trí nhớ đúng, cửa sổ bù trễ lùi rồi trả về đúng chỗ, bỏ qua người tung, trần 300 ms, và phép đo chính — cùng cú nổ ấy: không bù thì trượt, có bù thì trúng. Kết quả ra `PlayTestShots/quai_butre.txt`. |
| **39. Chay thu DON CUA QUAI qua mang** | Sáu chiều: dựng lại lỗ hổng "người khách bất tử trước quái", gói đòn quái khứ hồi, chủ phòng ra đòn thì có gói đi ra, máy khách nghe thì mất máu thật, gói lặp không ăn máu hai lần, và đòn nhắm người khác thì mình không mất máu. Kết quả ra `PlayTestShots/donquai.txt`. |
| **40. Chay thu MAU KHOI DAU** | Vào Play thật ở **cả hai màn** rồi đọc máu từ `Damageable`: nhân vật mình đầy máu, bản sao người chơi khác cũng đúng mức (nó lấy thẳng từ prefab), máu vẫn trừ được, và con số không tràn ra ngoài thanh máu. Đổi mức máu thì sửa `MauMongDoi` trong phép thử cho khớp. Kết quả ra `PlayTestShots/mau_khoi_dau.txt`. |
| **41. Chay thu NHIP BUOC qua mang** | Tám chiều: người chơi khác và đàn quái bên máy khách phải **bước chân** khi di chuyển và **dừng chân** khi đứng yên; nhịp bước không bị vòng di chuyển đặt lại về 0; vị trí và nhịp chân khớp với một tốc độ biết trước; và gói về thưa (10 lần/giây như đàn quái) không được làm nhân vật đứng im rồi nhảy — đo bằng **độ giật**, không chỉ tốc độ trung bình. Đo con số bộ hoạt hình nhận được, không chụp ảnh. Kết quả ra `PlayTestShots/nhipbuoc.txt`. |
| **42. Chay thu CHE DO DIEU KHIEN** | Chạy luật nhận diện thiết bị trên chín loại máy thật (kèm ba cái bẫy: laptop Windows có màn cảm ứng, máy tính bảng Android, iPad đời mới tự nhận là Mac), đối chiếu lại file `.jslib`, và kiểm rằng `Input.touchSupported` đã bị cắt khỏi đường quyết định. Không vào Play. Kết quả ra `PlayTestShots/chedodieukhien.txt`. |
| **43. Kiem toan buoc 5 (chi do, khong sua)** | Dựng lại tình huống máy khách rồi ghi ra số, không sửa gì: HUD khách có đếm đúng số quái không, và phép của khách có giết giả được bản sao quái không. Dùng để trả lời "bước 5 xong chưa" bằng số thay vì trí nhớ. Kết quả ra `PlayTestShots/kiemtoan_buoc5.txt`. |
| **44. Chay thu SUA BUOC 5 (2-1-4-5)** | Mười lăm chiều cho bốn chỗ hở đã vá: bản sao không chết cục bộ nhưng chết khi chủ phòng bảo, xác không ra đòn, HUD khách lấy bảng số của chủ phòng, phím R bị chặn trong trận mạng, và hai ngưỡng mất kết nối (chờ tín hiệu 3 giây — hồi phục được — rồi rời trận sau 10 giây hoặc ngay khi kênh đóng). Kết quả ra `PlayTestShots/sua_buoc5.txt`. |
| **45. Chay thu BON NGUOI (noi hinh sao)** | Mười một chiều cho trận bốn người: xếp ghế tất định (kể cả khi hai người trùng ghế), chủ phòng chuyển tiếp trạng thái và kỹ năng sang đúng những người còn lại và **không vòng về người gửi**, trả lời nhịp đúng kênh, sinh bản sao khi gói đầu tiên đến, một khách rời trận thì những người còn lại đều biết và gói trễ không làm người đó hiện lại. Kết quả ra `PlayTestShots/bonnguoi.txt`. |
| **46. Chay thu HIEU UNG qua mang** | Mười hai chiều: bản sao không tự gieo đóng băng/choáng (và nhân vật thật vẫn gieo được), cờ và máu khiên đọc đúng rồi đi qua gói tin không to thêm, bản sao vẽ lại theo lời kể, khiên bản sao không bị trừ cục bộ, mất gói thì hiệu ứng tự tan, và quái bên khách choáng theo chủ phòng. Kết quả ra `PlayTestShots/hieuung_mang.txt`. |
| **47. Chay thu CHE DO BON BO XUONG** | Vào Play thật ở **cả hai màn**, đếm quái trên cảnh theo loại: vào màn đúng 4 bộ xương, giết hết thì đợt mới ra đúng 30 giây game (hai vòng), và để yên 260 giây không sinh thêm con nào. Kết quả ra `PlayTestShots/bonboxuong.txt`. |
| **48. Chay thu CAI DAT do hoa** | Ngoài Play: font đủ chữ có dấu, vị trí nút ở nhiều cỡ màn hình, `index.html` không hạ `devicePixelRatio`, chuyển khoá cũ 3 mức sang khoá mới 4 mức. Trong Play: đăng nhập thật, bấm OK lần lượt 4 mức, đọc lại từ kho lưu, kiểu bóng / chi tiết xa, vào Act2 đếm vật đổ bóng, đo ảnh đệm cảnh 3D (kích thước, có phóng lên màn hình, gỡ ra ngoài lúc vẽ, đứng sau bloom), độ sáng ảnh chụp. Trả lại mức cũ, phiên đăng nhập và mức chất lượng của Editor. Kết quả ra `PlayTestShots/caidat.txt`. |
| **49. Chay thu CAU LUA trung nguoi va khieng** | Tự chọn hướng bắn trống, rồi đo hai chiều mạng: người khác bắn mình / mình bắn người khác, có và không có khiên, và khiên của chính người bắn. Ghi từng cú mất máu (cú nổ hay cú cháy), chỗ quả cầu nổ so với mặt vòm, máu khiên; chụp màn hình lúc nổ để xem con số sát thương có đọc được không. Kết quả ra `PlayTestShots/cauluapvp.txt`, ảnh `caulua_no_*.png`. |
| **50. Chay thu GIAO DIEN dang nhap - sanh - phong** | Đi hết các màn (đăng nhập, tạo tài khoản, sảnh trống, sảnh có phòng, Cài đặt, trong phòng, phòng đủ 4 người, đếm ngược); ở mỗi màn đếm số lượt vẽ, số chữ bị cắt, số chữ phải thu nhỏ — đếm ngay trong hàm vẽ nên không sót nhãn nào. Kiểm font đang dùng là Inter, và quay về MainMenu khi đã đăng nhập thì vào thẳng sảnh. Ảnh `gd_*.png`, kết quả `PlayTestShots/giaodien.txt`. |
| **51. Dung man chinh tu canh Act2** | Chép phần cảnh Act2 quanh chỗ đứng (45 m, phía trước camera) sang MainMenu.unity cùng ánh sáng / sương / bầu trời; đặt phù thuỷ, camera, hai lò đá; dọn vật vướng. Tạo luôn prefab lò đá từ FBX + texture Blender. Báo cáo `PlayTestShots/dungmanchinh.txt`. |
| **51b. Chup thu goc nhin man chinh (Act2)** | Đặt nhân vật trước từng nhà mồ theo bốn hướng, bỏ chỗ vướng vật / giữa nước, chụp bằng khung camera màn chính — để chọn chỗ đứng. Ảnh `PlayTestShots/goc/`. |
| **54. Dat 10 lo lua vao Act2** | Đặt 10 lò đá (prefab `Assets/Models/LoLuaDa`) vào Act2: lò giữa = chỗ đất khô gần tâm bản đồ nhất, 9 lò rải đều; không dưới nước, trong nhà mồ, trên/sát bia, chỉ trên mặt đất. Xoá lò cũ trước, chạy lại ra y hệt. Lưu Act2. Số đo `lolua_act2_dat.txt`. |
| **57. Chay thu BAN PHIM AO (o nhap khong bi che)** | Chạy thẳng trên hàm bố cục màn đăng nhập với 8 cỡ màn hình × 3 mức bàn phím che (35/45/55%) × 2 trang: khung và ô nhập cuối phải nằm trên mép bàn phím, ô nhập đầu không tràn lên khỏi mép trên. Số đo `banphimao.txt`. |
| **58. Chay thu MUA BANG + SAM SET (dong bang, choang)** | Đo kích thước tảng băng và cụm băng (đối chiếu mốc lấy từ git), xác suất đóng cứng/choáng trên 1000 lần gieo, người chơi bị đóng băng·choáng có thực sự đứng yên và không tung được phép (có mẫu đối chứng), mưa băng không nhắm vào chính người tung, và phép của người khác rơi trúng mình thì mình có dính. Số đo `bang_set.txt`. |
| **59. Chay thu SACH PHEP (keo tha o ky nang)** | Đo bố cục bảng trên 8 cỡ màn hình × 2 bản, kiểm ô tròn trong bảng xếp đúng hình cụm nút thật, kho kỹ năng (đổi chỗ · bỏ khỏi ô · lưu/nạp · hai bản riêng), và trong trận: nút con mắt ở góc phải trên, mở bảng thì input trận đấu bị khoá. Phần E: chữ "Sách phép" dưới nút không bị cắt — đo nét chữ trên ảnh chụp so với nhãn chuẩn không cắt, có mẫu đối chứng kiểu chữ cũ ở 4 cỡ màn (phải cắt ở màn điện thoại) — và quyển sách vẽ bằng ảnh có chi tiết. Phần F: cửa sổ to bằng khung xanh người dùng vẽ, độ sáng dòng đang chọn / đã mở / còn khoá, vành ô đang chọn (cả ô trống), ổ khoá trong ô — đo trên ảnh chụp, cả hai bản. Chụp 8 ảnh. Số đo `sachphep.txt`. |
| **60. Chay thu CAP DO (kinh nghiem, diem ky nang)** | Đo bảng kinh nghiệm và cách cộng dồn, hệ số chỉ số và hệ số kỹ năng, điểm kỹ năng (mở khoá · nâng cấp · hết điểm), gói mạng mang cấp kỹ năng; trong Play đo chỉ số **thật** trước/sau khi lên cấp, kỹ năng chưa mở không tung được, nâng cấp xong phép mạnh lên thật, giết quái được đúng số điểm. Số đo `capdo.txt`. |
| **61. Chay thu KINH NGHIEM theo tung ky nang** | Tung từng kỹ năng thật vào một con quái máu 1: phải chết, "kẻ đánh cuối" phải là người tung, kinh nghiệm phải cộng đúng giá. Tách riêng các đường chết chậm (cháy, bị lốc cuốn, vũng lửa Thiên thạch, cây cháy) và trường hợp nạn nhân là người chơi. Số đo `kinhnghiem_kynang.txt`. |
| **62. Chay thu THIEN THACH DANH NGA** | Xác suất đánh ngã trên 1000 lần gieo; thiên thạch của Quỷ dữ không đánh ngã; quả nhân vật tung mang đúng 40%·1,5 giây; trên quái thật đo độ cao bị hất, vị trí xương đầu lúc nằm (ngửa, sát đất), đứng im, đứng dậy; người chơi bị ngã không đi / không tung phép; bit "đang ngã" qua mạng. Số đo `danhnga.txt`. |
| **63. Chay thu DANH NGA NGUOI CHOI KHAC (qua mang)** | Bộ đồng bộ thật + kênh giả lập: người kia tung Thiên thạch bằng gói kỹ năng thật vào mình (đếm ngã, đo hình nằm ngửa từ xương đầu, dấu hiệu); máy người kia báo "đang ngã" bằng gói trạng thái thật → bản sao phải nằm, có dấu hiệu, đứng dậy; bit ngã qua được gói người chơi và gói quái. Số đo `danhnga_nguoichoi.txt`. |
| **64. Chay thu QUAI VONG NGOAI TRUY LUNG (60 giay)** | Dùng đợt quái thật: hẹn giờ 60 giây từng con; trước 60 giây quái chưa gặp không tiến lại (trung bình); sau 60 giây đếm con vào 14 m và con ra đòn thật; đối chứng hai con quái thường; dựng ca kẹt sau vật cản thật và ca nhốt trong bốn bức tường (phải vòng / đổi chỗ). Số đo `quai_truylung.txt`. |
| **65. Chay thu BINH MAU - BINH MANA (roi, nhat, uong, mang)** | Tỉ lệ rơi (3000 lần + giết quái thật), nhặt trong / ngoài bán kính, uống (khoá, hết, đầy, hồi chiêu, 100 / 50), số bình trên ô (so ảnh với ô đối chứng), chữ Sách phép to (so bề ngang nét với chữ đối chứng cỡ cũ / mới), cuộn thân chi tiết, 5 ca mạng qua bộ đồng bộ thật + kênh giả lập. Số đo `binh_mau_mana.txt`. |
| **66. Chay thu NUT KY NANG o sanh (sach phep xem truoc) + con mat** | Ngoài Play: quét chữ có dấu 3 file, chạy thật cầu nối localStorage bằng node. Trong Play: đăng nhập, mở Sách phép xem trước (tất cả kỹ năng mở — nay 11/11, so độ sáng hình với đối chứng khoá), thông số đọc từ nhân vật trưng bày = prefab, xếp ô → kho lưu, vào Act2 lúc sách mở (sách đóng, thanh kỹ năng đúng thứ tự), đo con mắt quỷ hai trạng thái; thoát Play nạp lại từ kho. Số đo `sachphep_sanh.txt`. |
| **67. Chay thu MUA BANG dong bang NGUOI CHOI KHAC (qua mang)** | Bộ đồng bộ thật + kênh giả lập: người kia tung Mưa băng vào mình bằng gói kỹ năng thật (6 cơn) — trúng, chậm, đóng cứng, khoá chân/phép, gói trạng thái mang bit đóng cứng; bản sao nhận gói đóng cứng thì đứng im và tan đúng lúc; đếm chữ nổi "ĐÓNG BĂNG!" (khớp số lần bắt đầu đóng cứng, bản sao đúng 1 lần) có đối chứng "CHOÁNG!"; vỏ băng phủ lên model có xương và làm vùng thân xanh lên trên ảnh chụp. Số đo `bang_nguoichoi.txt`. |
| **68. Chay thu QUA CAU BANG (ky nang moi)** | Tài nguyên Blender nạp được; thông số thật trên nhân vật (hồi chiêu 0,55); tung thật `CastAt(9)` (khoá, 3 quả, năng lượng, hồi chiêu); bia trên / lệch đường bay; sát thương 65 ở tâm và vùng nổ 3,4 m; 1000 lần gieo làm chậm (40%, 50%, 2 giây, cấp kéo dài); hình lúc bay (lưới, đuôi gai phía sau, luồng khí lạnh, vệt băng) và sau khi nổ; gói kỹ năng số 9 qua mạng; icon HUD, chữ Sách phép; kích thước thật cụm băng Mưa băng = Quả cầu băng (đối chứng cỡ cũ); Mưa băng thật rơi **vệt sáng băng như sao băng, không khối cầu** (0 lưới, đối chứng quả của kỹ năng có lưới; ảnh vệt Blender, độ dài thật của vệt, hào quang, mảnh băng, luồng khí lạnh, không đèn, không còn tảng cũ, vẫn gây sát thương, rơi thẳng đứng); cụm gai chỉ khi trúng đồ vật / kẻ địch (4 trường hợp + đối chứng + 2 cơn thật). Số đo `quacaubang.txt`. |
| **69. Chay thu GIUT SET (20 m, 75, 4 tia, 15% choang)** | Thông số; tung thật `CastAt(6)` vào 5 bia (đúng 4 bia mất 75 cùng một khung hình); tầm 20 m bằng bia 19,9 / 20,4 m; tia lan ra ngoài tầm vẫn trúng ×0,85; 160 lần phóng đếm tỉ lệ choáng tia đầu và tia lan riêng (bằng StunnedEffect trên bia); cấp kỹ năng kéo dài choáng. Số đo `giatset.txt`. |
| **71. Chay thu GIO LOC (ky nang moi)** | Thông số; tung thật `CastAt(10)` (khoá, 3 lốc, 20 năng lượng, hồi chiêu đo bằng bấm mỗi khung); lưới Blender, cao 5 m so Lốc xoáy thật, xám trắng, không đèn, không tia sét; **xoáy một chiều đi lên** (độ xoắn dải gió đo ngoài Play + chiều quay thật từng lớp + chiều trượt ảnh); khói bụi đen bay lên và cuộn cùng chiều (theo dõi từng hạt), vòng phun nằm ngang (phun thử 200 hạt), vệt phía sau; bán kính chân ×1,68 / phần trên ×1,0 (so công thức gốc); tia sét hiệu ứng khi trúng (5 bia → 5 tia từ thân lốc, 5 cháy sém, 0 cột sáng đứng, mất đúng 75); 1 lốc, sống 4,5 s; không trèo mái nhà mồ (đối chứng tia cũ chạm mái); tốc độ 8 m/s và thời gian sống; xuyên bia mộ (tia đối chứng); 75 một lần, 225 ba lốc, vùng 2,2 m (2,5 / 2,7 m); 190 lần trúng đếm hất tung độc lập, độ cao, thời gian bay; khiên chặn hất; ngắt chiêu người chơi (đối chứng) và đòn quái (đối chứng); qua mạng: gói số 10, bit hất tung, mặt nạ 5 bit, bản sao bay / ngắt chiêu / không hất lần hai; lò lửa tắt rồi cháy lại sau 30 s. Số đo `gioloc.txt`. |
| **71b. Chup anh GIO LOC (so voi Loc xoay)** | Chỉ chụp: hai lốc bay ngang màn hình 5 khung liên tiếp + Lốc xoáy lớn để so. Ảnh `gioloc_can_*.png`, `gioloc_locxoay_*.png`. |
| **76. Chay thu TOC BIEN (ky nang moi)** | Thông số (số hiệu 15, 40 năng lượng, tầm 15 m, nhóm HỖ TRỢ); hồi chiêu theo cấp 5 / 4,75 / 4,5 / 4,25 / 4; nháy đúng chỗ ngắm (lệch 0,00 m), có cả hai hiệu ứng; không niệm (0,029 s); ngắm 30 m chỉ nháy 15,01 m; ngắm vào giữa khối đá thì lùi về chỗ trống cách đá 2,02 m; **cấp 5 gỡ trói**: đối chứng cấp 4 bị chặn, cấp 5 dính 4 trạng thái vẫn bấm được và sạch còn 0, cháy hết, bị Lốc xoáy cuốn thì bị chặn. Số đo `tocbien.txt`. |
| **75. Chay thu HOA LOC XOAY + Gio loc hoi mana** | Thông số (số hiệu 14, 45 năng lượng, hồi chiêu 0,5); Gió lốc quét 3 bia hồi đúng 30 mana (đối chứng bắn chỗ trống: 0); năng lượng Gió lốc cấp 5 = 25 (bản cũ 58,6); hoá xong 0 Gió lốc / có Lốc xoáy lệch 0,00 m, bay 9,5 m/s, sống 6 s, đòn chạm 75 + 20/giây; hình to dần 0,42 → 1,00; bấm hụt không tốn mana và không vào hồi chiêu; cấp 5 hoá cả hai cơn. Số đo `hoalocxoay.txt`. |
| **74. Chay thu QUA CAU DIEN (ky nang moi)** | Thông số (số hiệu 13, 55 năng lượng, hồi chiêu 5 s, tầm 18 m) + tài nguyên Blender; quả cầu bám kẻ địch gần chỗ ngắm (đối chứng: không có ai thì đứng đúng chỗ ngắm); 10 lượt cách nhau 0,4 s; 50 tia, đúng 5 kẻ gần nhất mỗi kẻ một tia; 155,52 mỗi tia = Giựt sét cấp 5; 400 tia → choáng 30,0% × 1,50 s; gói phép số 13 qua mạng. Số đo `quacaudien.txt`. |
| **73. Chay thu TANG HINH (ky nang moi)** | Thông số (số hiệu 12, 30 năng lượng, hồi chiêu 30 s, 20 giây); thân đổi sang shader tàng hình rồi trả lại; miễn 6 hiệu ứng (đối chứng lúc thường dính đủ), xoá hiệu ứng đang dính, vẫn ăn sát thương; 24 quái thật mất dấu; tốc độ ×1,20 đo bằng quãng đường; đòn đầu ×2,00 rồi tan, Khiên không làm tan; hết giờ tự tan; qua mạng bit `CoTangHinh`, bản sao đứng yên tắt renderer; **nhấp nháy 2 giây cuối** (18 lần đổi sáng/tối, bản sao người khác không nháy) và hai ảnh hai pha; **đòn đầu ×2 cho TOÀN BỘ sát thương**: từng vệt Mưa băng 29,04 → 58,08, Quả cầu băng cấp 5 442,4 → 884,7; cờ đòn đầu đi qua gói phép (người kia tung qua mạng cũng ×2). Số đo `tanghinh.txt`. |
| **72. Chay thu LUA DIA NGUC (ky nang moi)** | Thông số (số hiệu 11, năng lượng 31/hồi chiêu/niệm, sát thương gốc = prefab Quả cầu lửa × 1,2⁴, chữ Sách phép, icon); tung thật 5 quả; tự dí 5 bia ngoài hình quạt (đối chứng Quả cầu lửa thường 0 bia), 1 bia, bia chạy ngang, mục tiêu chết giữa đường, không có ai bay thẳng 18°; sát thương so với Quả cầu lửa cấp 5 thật + thiêu đốt; màu giống hệt Quả cầu lửa; qua mạng + kẻ đánh. Tạm tắt va chạm đồ vật 26 m (Act2 không có chỗ trống). Số đo `luadianguc.txt`. |
| **70. Chay thu QUA CAU LUA (85 sat thuong, vet lua moi)** | Sát thương đọc thẳng prefab và trên quả cầu thật khi tung; quả cầu sinh từ prefab bay vào bia (mất 85 × giảm theo khoảng cách); hạt `Flames` không còn ảnh tam giác mà là flipbook Blender, có vệt lửa dài `TrailRenderer`; nổ xong vệt được thả ra; chụp cận cảnh lúc bay. Chạy trên bản cũ ra 7 lỗi (đối chứng). Số đo `quacaulua.txt`. |
| **56. Chay thu DOT QUAI Act2 + cho xuat phat** | *(13/09/2026: thêm đo chờ 30 giây và 10 con xa 55–65 m)*  Kiểm chỗ xuất phát ngẫu nhiên (hai máy cùng mã phòng ra cùng danh sách, cách nhau ≥ 22 m, trên đất, ngoài nước, không vướng vật cản) và luật đợt quái Act2 (đợt 1 bốn con quanh mỗi người; đợt sau cộng dồn quái và mạnh thêm 5% máu · sát thương); kiểm Act1 không bị đổi. Số đo `dotquai_act2.txt`. |
| **55. Chay thu KET TRAN (nguoi song sot cuoi cung)** | Mở kênh giả lập như menu 45: kiểm gói tin kết trận/chết, máy chủ phòng phán quyết đúng lúc còn một người, bảng điểm cộng đúng người, máy khách không tự kết luận và hiện đúng kết quả nghe được, chết rồi camera chuyển sang người còn sống, chụp màn kết trận. Số đo `kettran.txt`, ảnh `kettran_*.png`. |
| **54c. Chay thu LOC XOAY cuon lo lua** | Vào Play Act2, thả một cơn lốc đi thẳng vào lò: đo mốc thời gian lửa tắt / lò nhấc lên / lò biến mất / lò mọc lại, kiểm than trong chậu tắt bằng độ sáng trên ảnh, và kiểm vật có hệ hạt khác vẫn không bị cuốn. Ảnh `locxoay_*.png`, số đo `locxoay_lolua.txt`. |
| **54b. Chay thu lo lua Act2** | Kiểm 10 lò bằng cách khác lúc đặt (va chạm tạm cho lưới nước, tia chiếu lên tìm mái nhà, hộp bao bia, độ cao địa hình quanh chân); trong Play: lửa + đèn bật, nhân vật đi thẳng vào lò bị chặn; chụp `lolua_*.png` + bản đồ. Số đo `lolua_act2.txt`. |
| **53. Chay thu HUD KINH DI (mau, mana, thong bao)** | Ngoài Play: chạy hàm bố cục HUD với chữ dài nhất ở 11 cỡ màn hình × PC/cảm ứng — khung chữ không ra ngoài, không đè nhau hay đè nút; số máu/mana lọt thanh. Quét chuỗi 4 file HUD: đủ ký tự trong cmap Inter, không còn chữ không dấu cũ. Trong Play (Act2): bật cùng lúc mọi thông báo + máu thấp, đọc bố cục thật, chụp `hud_*.png`. Số đo `hudkinhdi.txt`. |
| **52. Chay thu TEN TREN DAU nhan vat** | Vào Play ở Act2, gắn tên cho nhân vật của mình, sinh ba bản sao tên có dấu quanh mình; đo từng bảng tên: có vẽ, trong màn hình, ngay trên chóp mũ (đo độc lập bằng lưới bake) không quá 0,30 m, đúng màu, font Inter đủ 134 chữ có dấu (đọc cmap), ghép dấu rời đúng (252 cách gõ), nền trong suốt (đo trên ảnh chụp, có mẫu đối chứng nền đen), không đè nhau, người gục thì tên mờ. Gọi `GiaoDien.ChuanBi` như màn sảnh. Ảnh `bangten_*.png`, số đo `bangten.txt`. |
| **51c. Chup nen man chinh (lo da, ngon lua)** | Vào Play, tắt giao diện, chụp toàn cảnh (thêm một ảnh `Camera.main` đúng 1920 × 1080), cận lò đá, cận ngọn lửa; đo từng tấm flipbook (khói đen, hai tấm lửa: số hạt, vật liệu, texture), tam giác, vật đổ bóng. Ảnh `nen_*.png`, số đo `nenmanchinh.txt`. |

> ⚠️ Mục **1** sẽ **xóa và tạo lại** các thư mục Textures / Materials / Models / Prefabs.
> Nếu bạn tự sửa tay trong đó thì hãy sao lưu trước.
> Riêng thư mục **Terrain** thì **không bị xoá** — nét tô tay trên mặt đất được giữ nguyên.

---

## Tô vẽ mặt đất bằng tay

Mặt đất giờ là một **Unity Terrain** thật, không còn là khối hình sinh bằng code nữa.
Bạn tô trực tiếp trong cửa sổ Scene, không cần đụng tới dòng code nào.

**Cách làm:**

1. Mở scene `Act1`, bấm vào đối tượng **World → MatDat** trong cửa sổ Hierarchy
2. Trong cửa sổ Inspector hiện ra một hàng nút cọ. Chọn **Paint Terrain**
3. Ở ô danh sách phía dưới chọn việc muốn làm:
   - **Raise or Lower Terrain** — nâng / hạ đồi núi (giữ Shift để hạ)
   - **Paint Texture** — tô lớp vật liệu lên mặt đất
   - **Smooth Height** — làm mượt chỗ vừa nâng
4. Với **Paint Texture**, chọn một trong bốn lớp rồi quét chuột lên mặt đất:

| Lớp | Là gì | Ban đầu |
|-----|-------|---------|
| `Lop_Co` | đất có cỏ mọc, sáng màu hơn | đã trải sẵn ở vùng ẩm |
| `Lop_DatKho` | đất cát khô trơ | đã trải sẵn ở vùng khô |
| `Lop_SoiDa` | sỏi đá vụn | **để trống** — dành cho bạn tô |
| `Lop_Bun` | bùn ướt sẫm màu | **để trống** — dành cho bạn tô |

5. Chỉnh **Brush Size** (cỡ cọ) và **Opacity** (độ đậm) ở dưới cho vừa ý

**Nét tô của bạn được giữ lại.** Bấm nút "1. Nuong Asset" những lần sau sẽ *không*
ghi đè lên địa hình — chỉ tạo mới khi file chưa có. Muốn xoá hết làm lại từ đầu thì
bấm **"Diablo 2.5D → 8. Dung lai dia hinh (xoa net to tay)"**.

**Cỏ, cây, đá vẫn tự bám theo.** Nếu bạn nâng một quả đồi lên, những thứ đã đặt sẵn
không tự di chuyển — nhưng lần dựng scene sau chúng sẽ đọc độ cao mới từ terrain
(hàm `WorldFactory.GroundHeight` giờ lấy số trực tiếp từ terrain). Vòng tròn phép
của các skill cũng bám theo mặt đất mới ngay lập tức vì chúng bắn tia dò xuống đất.

---

## Phần 5 — Muốn chỉnh gì thì sửa ở đâu

Có **hai cách sửa**, cách nào cũng được:

**Cách A — sửa trực tiếp trên prefab (dễ, không cần biết code):**
mở `Assets/Prefabs`, bấm đúp một prefab, sửa số trong khung Inspector, bấm lưu.
Ví dụ: bấm `Enemy_QuyKhongLo` → mục `Damageable` đổi `Max Health` từ 240 thành 400.

**Cách B — sửa code rồi bấm "1. Nuong Asset" để tạo lại:**

| Muốn đổi | Sửa ở đâu |
|----------|-----------|
| Sát thương / tầm nổ quả cầu lửa | Prefab `Skill_QuaCauLua` hoặc [`Skills/Fireball.cs`](Assets/Scripts/Skills/Fireball.cs) |
| Sức mạnh và thời gian Mưa băng | Prefab `Skill_MuaBang` hoặc [`Skills/IceStorm.cs`](Assets/Scripts/Skills/IceStorm.cs) |
| **Sức mạnh Thiên thạch** | [`Skills/ThienThach.cs`](Assets/Scripts/Skills/ThienThach.cs) — `impactDamage`, `blastRadius`, `doCaoRoi`, `speed` |
| **Bãi đất cháy sau Thiên thạch** | [`Skills/VungLua.cs`](Assets/Scripts/Skills/VungLua.cs) — `radius`, `duration`, `damagePerSecond`; hoặc `chayBanKinh` / `chayThoiGian` trong `ThienThach.cs` |
| **Máu khiêng, bán kính khiêng** | [`Player/PlayerController.cs`](Assets/Scripts/Player/PlayerController.cs) — `khiengMau` (150), `khiengBanKinh` (**3,036 m**). **Nhớ sửa cả prefab `Player_Sorceress` và hai scene** — giá trị đã serialize đè lên code |
| **Độ cao vòm khiêng** | [`Vfx/VfxFactory.cs`](Assets/Scripts/Vfx/VfxFactory.cs) — `CaoVomKhieng` (**1,0957** lần bán kính). Đây là **tỉ lệ** so với bán kính ngang, nên đổi nó chỉ đổi chiều cao, bề ngang giữ nguyên. Nhớ sửa tỉ lệ trong `GameHUD.KhiengIcon` cho khớp |
| **Tia sét trên khiêng** | [S_Khieng.shader](Assets/Shaders/S_Khieng.shader) — `_GanSoDuong` (số tia), `_GanNhip` (độ gãy khúc), `_GanDay` (bề dày lõi), `_SetQuang` (bề rộng quầng), `_SetChop` (độ chớp tắt), `_TiaChoi` (độ chói) |
| **Tia trải đều hay dồn ra rìa** | [S_Khieng.shader](Assets/Shaders/S_Khieng.shader) — `_GanRia` (0,55; **phải nhỏ**, để cao là tia teo thành vòng chỉ ở mép), `_GanNen` (lượng tia giữa vòm) |
| **Độ đục của màng vòm** | [S_Khieng.shader](Assets/Shaders/S_Khieng.shader) — `_DamNen` (0,18). Đặt 0 là vòm chỉ còn viền và tia, mất hết hình khối |
| **Nhân vật bị vòm che mờ** | [S_Khieng.shader](Assets/Shaders/S_Khieng.shader) — `_MatTruoc` (0,35). Nhỏ hơn = nửa vòm trước mặt nhân vật trong hơn |
| **Độ trong của khiêng** | [S_Khieng.shader](Assets/Shaders/S_Khieng.shader) — `_TrongSuot` (0,70). Nhỏ hơn = trong hơn; độ sáng tự được bù lại |
| **Độ nung đỏ của khối đá** | `VfxFactory.DaThienThachMat` — số cuối trong `Mats.Glow` |
| **Lượng khói bụi đen phía sau Lốc xoáy** | [`Skills/Tornado.cs`](Assets/Scripts/Skills/Tornado.cs) — `khoiBuiDau` (30 hạt/giây lúc mới ra) và `khoiBuiCuoi` (210 lúc sắp tan) |
| **Sức hút, tốc độ trườn, độ cao nhấc bổng của Lốc xoáy** | Prefab `Skill_LocXoay` hoặc [`Skills/Tornado.cs`](Assets/Scripts/Skills/Tornado.cs) — `catchRadius`, `moveSpeed`, `liftHeight`, `spinDegreesPerSecond` |
| **Tỉ lệ choáng / sát thương Sấm sét** | Prefab `Skill_SamSet` hoặc [`Skills/LightningStorm.cs`](Assets/Scripts/Skills/LightningStorm.cs) — ô `Stun Chance` (0.4 = 40%), `Stun Seconds`, `Strike Interval` |
| Độ dày, độ sáng, số nhánh của tia sét | [`Vfx/LightningArc.cs`](Assets/Scripts/Vfx/LightningArc.cs) — `coreWidth`, `glowWidth`, `branches` |
| Máu, năng lượng, tốc chạy phù thủy | Prefab `Player_Sorceress` hoặc [`Player/PlayerController.cs`](Assets/Scripts/Player/PlayerController.cs) |
| Máu / sát thương từng loại quái | 4 prefab `Enemy_*` hoặc [`Enemies/EnemyFactory.cs`](Assets/Scripts/Enemies/EnemyFactory.cs) |
| Số quái mỗi đợt, nhịp ra quái | Vật thể `GameDirector` trong scene |
| Góc nhìn camera | `Main Camera` → `CameraRig` → mảng `Views` |
| Độ chói của hiệu ứng sáng | `Main Camera` → `SimpleBloom` |
| Màu áo choàng, màu da quái | `Assets/Materials` (bấm vào file .mat đổi màu ngay) |
| **Cỏ dày hay thưa** | [`Art/WorldFactory.cs`](Assets/Scripts/Art/WorldFactory.cs) → `grassDensity` (số bụi cỏ trên mỗi m², đang để 0.36). Muốn cỏ rậm như đồng hoang thì nâng lên 5-6 |
| **Tỉ lệ đất thịt / đất cát khô** | [`Art/WorldFactory.cs`](Assets/Scripts/Art/WorldFactory.cs) → hàm `GrassAmount` |
| **Chiều cao thân cỏ** | [`Art/WorldFactory.cs`](Assets/Scripts/Art/WorldFactory.cs) → `BuildGrassField`, biến `h` (bề ngang `w` tính theo `h`) |
| **Độ gồ ghề, đồi dốc của mặt đất** | [`Art/WorldFactory.cs`](Assets/Scripts/Art/WorldFactory.cs) → hàm `GroundHeight` |
| **Hình dáng cục đá / núi đá** | [`Art/WorldFactory.cs`](Assets/Scripts/Art/WorldFactory.cs) → `RockClusterMesh` (3 kiểu: `RockStyleSpire` ngọn nhọn, `RockStyleCliff` khối vuông, `RockStyleRubble` đá nằm bẹt) |
| **Hình dáng thân cây** | [`Art/WorldFactory.cs`](Assets/Scripts/Art/WorldFactory.cs) → `TrunkMesh` (số cạnh, số tầng, độ nghiêng) và `BuildLeafyTree` / `BuildDeadTree` |
| **Vân vỏ cây** | `M_Bark.mat`, hoặc hàm `Bark()` trong [`Art/TextureFactory.cs`](Assets/Scripts/Art/TextureFactory.cs) |
| **Màu / vân đá** | `M_Rock.mat`, hoặc hàm `CliffRock()` trong [`Art/TextureFactory.cs`](Assets/Scripts/Art/TextureFactory.cs) |
| **Màu mặt đất** | `M_Ground.mat`, hoặc hàm `DarkSoil()` / `MudGround()` trong [`Art/TextureFactory.cs`](Assets/Scripts/Art/TextureFactory.cs) |
| **Độ lắc của cỏ theo gió** | [`Shaders/S_Foliage.shader`](Assets/Shaders/S_Foliage.shader) → `_WindSpeed`, `_WindBend` |
| **Ánh trăng, bóng đổ, sương đêm** | [`Art/WorldFactory.cs`](Assets/Scripts/Art/WorldFactory.cs) → `SetupMoonlight` và `BuildSkyAndFog` |
| **Tông màu cỏ và lá cây** | 4 biến ở đầu [`Art/TextureFactory.cs`](Assets/Scripts/Art/TextureFactory.cs): `GrassRoot`, `GrassTip`, `LeafLight`, `LeafDark` → rồi chạy lại "1. Nuong Asset" |
| Hình dáng nhân vật / quái | [`Art/WizardFactory.cs`](Assets/Scripts/Art/WizardFactory.cs), [`Art/MonsterFactory.cs`](Assets/Scripts/Art/MonsterFactory.cs) → rồi chạy lại "1. Nuong Asset" |
| Bố cục cảnh vật trong màn | Kéo thả trực tiếp các prefab trong scene, hoặc sửa `AssetBaker.BuildScene` |

---

## Phần 6 — Một quy tắc quan trọng khi sửa code

> ⚠️ **Mỗi class `MonoBehaviour` phải nằm trong file `.cs` TRÙNG TÊN với class.**
>
> Ví dụ class `ExpandFade` phải nằm trong `ExpandFade.cs`. Nếu nhét nhiều class vào
> một file, Unity vẫn biên dịch được, nhưng khi lưu vào **prefab** thì tham chiếu
> script bị mất (`m_Script: {fileID: 0}`) — script không chạy nữa. Đó chính là lý do
> lúc đầu hiệu ứng lửa/băng dính mãi trên màn hình: script tự huỷ đã bị mất.
>
> Công cụ **1. Nuong Asset** nay tự kiểm tra và báo lỗi đỏ nếu gặp trường hợp này.

## Phần 7 — Cách game hoạt động (ngắn gọn)

- Vật thể **`GAME`** trong scene có 2 script:
  - `GameAssets` — **thư viện prefab**: mọi phép thuật, quái vật, hiệu ứng đều lấy từ đây.
  - `GameBootstrap` — nối các thành phần lại với nhau khi bấm Play.
- Nếu một ô prefab bị bỏ trống, game **vẫn chạy**: nó tự dựng hình bằng code như phiên bản đầu.
  Nhờ vậy lỡ xóa prefab nào thì game cũng không vỡ.
- Bốn góc nhìn dùng chung **một camera phối cảnh**; chế độ "2D" đạt được bằng cách lùi camera
  thật xa + thu góc nhìn còn 13–15° nên hình gần như phẳng, và chuyển cảnh vẫn mượt.

**Về độ "giống thật".** Mọi hình khối vẫn được sinh từ code rồi nướng ra file, nên nhân vật ở
mức **tả thực vừa phải** (đúng tỉ lệ người, mũ trùm hở mặt, áo choàng, xương sườn, sừng, vũ khí)
chứ chưa chi tiết như model do họa sĩ nặn. Muốn thật hơn: tải model Mixamo, kéo vào `Assets`,
rồi thay phần thân của prefab `Player_Sorceress` — phần phép thuật, quái, camera, HUD giữ nguyên.

**Xuất bản thành .exe:** menu **File → Build Settings → Build**. Hai scene đã được thêm sẵn vào
danh sách build (MainMenu trước, rồi Act1) và các shader tự viết đã khai báo "luôn đưa vào
bản build" nên hiệu ứng lửa/băng không bị mất.

Chúc bạn chơi vui! 🔥❄️
