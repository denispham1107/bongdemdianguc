// TAI KHOAN NGUOI CHOI: dang ky, dang nhap, ho so.
//
// Firebase Auth giu email va mat khau; nhung ho so nguoi choi (ten, thanh tich,
// co bi khoa) thi nam trong Firestore o collection "nguoichoi".
//
// VI SAO PHAI CO BAN SAO TRONG FIRESTORE: Firebase Auth khong cho phia trinh
// duyet liet ke toan bo tai khoan - chi Admin SDK chay tren may chu moi lam
// duoc. Nen trang quan tri doc collection "nguoichoi" chu khong doc Auth.

import {
  createUserWithEmailAndPassword, signInWithEmailAndPassword,
  signOut, onAuthStateChanged, sendPasswordResetEmail
} from "https://www.gstatic.com/firebasejs/10.12.0/firebase-auth.js";
import {
  doc, getDoc, setDoc, updateDoc, serverTimestamp
} from "https://www.gstatic.com/firebasejs/10.12.0/firebase-firestore.js";
import { auth, kho } from "./ketnoi.js";

/// Dang ky roi tao luon ho so. Hai viec nay phai di lien nhau.
export async function dangKy(email, matKhau, ten) {
  const kq = await createUserWithEmailAndPassword(auth, email, matKhau);
  await taoHoSoNeuThieu(kq.user, ten);
  return kq.user;
}

export async function dangNhap(email, matKhau) {
  const kq = await signInWithEmailAndPassword(auth, email, matKhau);

  // Nguoi dung co the da dang ky xong roi dong tab truoc khi ho so kip tao -
  // luc do co tai khoan Auth "mo coi" ma trang quan tri khong nhin thay. Tu
  // chua o day: lan dang nhap sau la ho so duoc tao lai.
  await taoHoSoNeuThieu(kq.user, null);
  await updateDoc(doc(kho, "nguoichoi", kq.user.uid),
                  { lanDangNhapCuoi: serverTimestamp() });
  return kq.user;
}

export function dangXuat() {
  return signOut(auth);
}

export function guiEmailDatLaiMatKhau(email) {
  return sendPasswordResetEmail(auth, email);
}

/// Tao ho so voi moi con so thanh tich bang 0. Security Rules bat buoc dung
/// nhu vay - khong ai tu dat soTranThang = 99 luc dang ky duoc.
export async function taoHoSoNeuThieu(nguoiDung, ten) {
  const o = doc(kho, "nguoichoi", nguoiDung.uid);
  const dsHienCo = await getDoc(o);
  if (dsHienCo.exists()) return dsHienCo.data();

  const tenDung = (ten && ten.trim())
    ? ten.trim().slice(0, 16)
    : (nguoiDung.email || "NguoiChoi").split("@")[0].slice(0, 16);

  const hoSo = {
    ten: tenDung,
    email: nguoiDung.email || "",
    ngayTao: serverTimestamp(),
    lanDangNhapCuoi: serverTimestamp(),
    soTranChoi: 0,
    soTranThang: 0,
    soQuaiDaDiet: 0,
    soNguoiDaHa: 0,
    biKhoa: false,
    ghiChuAdmin: ""
  };
  await setDoc(o, hoSo);
  return hoSo;
}

export async function layHoSo(uid) {
  const d = await getDoc(doc(kho, "nguoichoi", uid));
  return d.exists() ? d.data() : null;
}

/// Co phai admin khong - hoi bang cach xem co document trong "quantri" khong.
export async function laAdmin(uid) {
  try {
    const d = await getDoc(doc(kho, "quantri", uid));
    return d.exists();
  } catch (e) {
    return false;
  }
}

/// Goi ham khi trang thai dang nhap doi. Tra ve ham de huy dang ky.
export function theoDoiDangNhap(ham) {
  return onAuthStateChanged(auth, ham);
}

/// Bat moi trang phai co nguoi dang nhap; chua thi day ve trang dau.
/// Tra ve { nguoiDung, hoSo } khi da dang nhap va KHONG bi khoa.
export function batBuocDangNhap(duongDanVe = "index.html") {
  return new Promise((xong) => {
    theoDoiDangNhap(async (nguoiDung) => {
      if (!nguoiDung) { location.href = duongDanVe; return; }

      const hoSo = await taoHoSoNeuThieu(nguoiDung, null);
      if (hoSo && hoSo.biKhoa === true) {
        await dangXuat();
        location.href = duongDanVe + "?khoa=1";
        return;
      }
      xong({ nguoiDung, hoSo });
    });
  });
}
