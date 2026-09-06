// PHONG CHO: tao, vao, ra, san sang, bat dau.
//
// Tat ca nam tren Realtime Database chu khong phai Firestore, vi mot ly do
// duy nhat nhung quyet dinh: onDisconnect(). Nguoi choi dong tab thi Firebase
// tu xoa ho khoi phong - khong co no thi phong day nguoi ma va tran khong bao
// gio ket thuc.

import {
  ref, get, set, update, remove, push, onValue, off,
  runTransaction, onDisconnect, serverTimestamp, query, orderByChild
} from "https://www.gstatic.com/firebasejs/10.12.0/firebase-database.js";
import { rtdb } from "./ketnoi.js";
import { SO_NGUOI_TOI_DA, GIAY_DEM_NGUOC } from "./caihinh.js";

// ================================================================
//  DONG HO MAY CHU
// ================================================================
//
// Dem nguoc phai khop tren ca bon may. Neu moi may dem bang dong ho cua
// chinh no thi may nao lech gio la vao tran som hoac muon vai giay.
//
// Cach lam: host ghi mot MOC THOI GIAN may chu (batDauLuc), moi may doc
// do lech giua dong ho cua minh va dong ho Firebase roi tu tinh con bao
// nhieu giay. Khong ai phai gui tin moi giay.

let doLechDongHo = 0;

export function batDauDoDongHo() {
  onValue(ref(rtdb, ".info/serverTimeOffset"), (o) => {
    doLechDongHo = o.val() || 0;
  });
}

export function gioMayChu() {
  return Date.now() + doLechDongHo;
}

// ================================================================
//  TAO / VAO / RA PHONG
// ================================================================

/// Tao phong moi. Nguoi tao thanh Host.
export async function taoPhong(nguoiDung, ten, tenPhong, manChoi) {
  const o = push(ref(rtdb, "phong"));
  const maPhong = o.key;

  await set(o, {
    ten: (tenPhong || ("Phong cua " + ten)).slice(0, 24),
    hostUid: nguoiDung.uid,
    hostTen: ten.slice(0, 16),
    manChoi: manChoi,
    trangThai: "cho",
    toiDa: SO_NGUOI_TOI_DA,
    soNguoi: 1,
    taoLuc: serverTimestamp(),
    nguoiChoi: {
      [nguoiDung.uid]: { ten: ten.slice(0, 16), sanSang: true, cho: 0, vaoLuc: Date.now() }
    }
  });

  await gaiTuRoiKhiMatKetNoi(maPhong, nguoiDung.uid, true);
  return maPhong;
}

/// Vao mot phong da co. Nem loi neu phong day, dang choi, hoac khong con.
export async function vaoPhong(maPhong, nguoiDung, ten) {
  const oPhong = ref(rtdb, "phong/" + maPhong);
  const anh = await get(oPhong);
  if (!anh.exists()) throw new Error("Phòng này không còn nữa.");

  const p = anh.val();
  if (p.trangThai !== "cho") throw new Error("Phòng này đã bắt đầu chơi rồi.");

  // Da o trong phong roi thi khong lam gi them
  if (p.nguoiChoi && p.nguoiChoi[nguoiDung.uid]) {
    await gaiTuRoiKhiMatKetNoi(maPhong, nguoiDung.uid, false);
    return true;
  }

  // GIAO DICH de hai nguoi bam cung luc thi chi mot nguoi lot vao cho cuoi.
  // Luat cua Realtime Database khong dem duoc so con (khong co numChildren),
  // nen suc chua phai giu bang chinh truong soNguoi nay.
  const kq = await runTransaction(ref(rtdb, "phong/" + maPhong + "/soNguoi"), (n) => {
    if (n === null) return 0;
    if (n >= SO_NGUOI_TOI_DA) return;      // tra ve undefined = huy giao dich
    return n + 1;
  });

  if (!kq.committed) throw new Error("Phòng đã đủ " + SO_NGUOI_TOI_DA + " người.");

  // Chon cho ngoi con trong (0..3) de nhan vat khong de chong len nhau
  const daDung = Object.values(p.nguoiChoi || {}).map(x => x.cho);
  let cho = 0;
  while (daDung.includes(cho) && cho < SO_NGUOI_TOI_DA - 1) cho++;

  await set(ref(rtdb, "phong/" + maPhong + "/nguoiChoi/" + nguoiDung.uid), {
    ten: ten.slice(0, 16), sanSang: false, cho: cho, vaoLuc: Date.now()
  });

  await gaiTuRoiKhiMatKetNoi(maPhong, nguoiDung.uid, false);
  return true;
}

/// VAO PHONG NHANH: chon phong dang cho con cho trong, uu tien phong dong
/// nguoi nhat (de tran bat dau som), khong co thi tu tao mot phong moi.
export async function vaoPhongNhanh(nguoiDung, ten) {
  const ds = await layDanhSachPhong();
  const conCho = ds
    .filter(p => p.trangThai === "cho" && (p.soNguoi || 0) < (p.toiDa || SO_NGUOI_TOI_DA))
    .sort((a, b) => (b.soNguoi || 0) - (a.soNguoi || 0));

  for (const p of conCho) {
    try {
      await vaoPhong(p.ma, nguoiDung, ten);
      return p.ma;
    } catch (e) {
      // Phong vua day trong luc minh dang doc - thu phong ke tiep
    }
  }

  const man = Math.random() < 0.5 ? "Act1" : "Act2";
  return await taoPhong(nguoiDung, ten, "Phong cua " + ten, man);
}

/// Roi phong. Host roi thi phong bi xoa han.
export async function roiPhong(maPhong, nguoiDung) {
  const anh = await get(ref(rtdb, "phong/" + maPhong));
  if (!anh.exists()) return;

  if (anh.val().hostUid === nguoiDung.uid) {
    await remove(ref(rtdb, "phong/" + maPhong));
    await remove(ref(rtdb, "tran/" + maPhong));
    return;
  }

  await remove(ref(rtdb, "phong/" + maPhong + "/nguoiChoi/" + nguoiDung.uid));
  await runTransaction(ref(rtdb, "phong/" + maPhong + "/soNguoi"),
                       (n) => (n === null ? 0 : Math.max(0, n - 1)));
}

/// Gai san lenh "khi mat ket noi thi tu xoa". Firebase giu lenh nay o phia
/// may chu, nen no chay ca khi trinh duyet bi tat dot ngot hay mat dien.
async function gaiTuRoiKhiMatKetNoi(maPhong, uid, laHost) {
  if (laHost) {
    // Host bien mat thi ca phong bien theo - khong de lai phong ma
    await onDisconnect(ref(rtdb, "phong/" + maPhong)).remove();
    await onDisconnect(ref(rtdb, "tran/" + maPhong)).remove();
  } else {
    await onDisconnect(ref(rtdb, "phong/" + maPhong + "/nguoiChoi/" + uid)).remove();
  }
  await onDisconnect(ref(rtdb, "hienDien/" + uid)).remove();
  await set(ref(rtdb, "hienDien/" + uid), { truc: true, phong: maPhong });
}

// ================================================================
//  TRONG PHONG
// ================================================================

export function datSanSang(maPhong, uid, sanSang) {
  return set(ref(rtdb, "phong/" + maPhong + "/nguoiChoi/" + uid + "/sanSang"), !!sanSang);
}

export function doiManChoi(maPhong, manChoi) {
  return update(ref(rtdb, "phong/" + maPhong), { manChoi: manChoi });
}

/// Host bam bat dau. Host duoc bat dau KE CA khi chi co mot minh.
///
/// Ghi mot moc thoi gian tuong lai roi de moi may tu dem - khong ai phai
/// gui tin tung giay, va bon may dem khop nhau trong vong vai chuc mili giay.
export function batDauDemNguoc(maPhong) {
  return update(ref(rtdb, "phong/" + maPhong), {
    trangThai: "demNguoc",
    batDauLuc: gioMayChu() + GIAY_DEM_NGUOC * 1000
  });
}

export function danhDauDangChoi(maPhong) {
  return update(ref(rtdb, "phong/" + maPhong), { trangThai: "dangChoi" });
}

/// Host duoi mot nguoi ra khoi phong
export async function duoiNguoi(maPhong, uid) {
  await remove(ref(rtdb, "phong/" + maPhong + "/nguoiChoi/" + uid));
  await runTransaction(ref(rtdb, "phong/" + maPhong + "/soNguoi"),
                       (n) => (n === null ? 0 : Math.max(0, n - 1)));
}

// ================================================================
//  DOC
// ================================================================

export async function layDanhSachPhong() {
  const anh = await get(query(ref(rtdb, "phong"), orderByChild("taoLuc")));
  if (!anh.exists()) return [];
  const ra = [];
  anh.forEach((con) => { ra.push({ ma: con.key, ...con.val() }); });
  return ra.reverse();     // phong moi nhat len dau
}

/// Theo doi danh sach phong. Tra ve ham de thoi theo doi.
export function theoDoiDanhSachPhong(ham) {
  const o = ref(rtdb, "phong");
  const huy = onValue(o, (anh) => {
    const ra = [];
    if (anh.exists()) anh.forEach((con) => { ra.push({ ma: con.key, ...con.val() }); });
    ra.sort((a, b) => (b.taoLuc || 0) - (a.taoLuc || 0));
    ham(ra);
  });
  return () => off(o, "value", huy);
}

/// Theo doi mot phong. Tra ve ham de thoi theo doi.
export function theoDoiPhong(maPhong, ham) {
  const o = ref(rtdb, "phong/" + maPhong);
  const huy = onValue(o, (anh) => ham(anh.exists() ? { ma: maPhong, ...anh.val() } : null));
  return () => off(o, "value", huy);
}
