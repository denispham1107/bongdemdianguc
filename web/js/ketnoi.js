// KHOI TAO FIREBASE MOT LAN CHO CA TRANG WEB.
//
// Moi trang (dang nhap, sanh, quan tri) deu nap file nay chu khong tu goi
// initializeApp - goi hai lan tren cung mot trang thi Firebase nem loi
// "Firebase App named '[DEFAULT]' already exists".

import { initializeApp } from "https://www.gstatic.com/firebasejs/10.12.0/firebase-app.js";
import { getAuth } from "https://www.gstatic.com/firebasejs/10.12.0/firebase-auth.js";
import { getFirestore } from "https://www.gstatic.com/firebasejs/10.12.0/firebase-firestore.js";
import { getDatabase } from "https://www.gstatic.com/firebasejs/10.12.0/firebase-database.js";
import { caiHinh } from "./caihinh.js";

export const app = initializeApp(caiHinh);
export const auth = getAuth(app);
export const kho = getFirestore(app);     // Firestore: ho so, lich su tran
export const rtdb = getDatabase(app);     // Realtime Database: phong, tran dau

/// Bao loi ra man hinh bang tieng Viet thay vi ma loi cua Firebase.
export function dichLoi(loi) {
  const ma = (loi && loi.code) ? loi.code : "";
  switch (ma) {
    case "auth/invalid-email":          return "Email khong hop le.";
    case "auth/email-already-in-use":   return "Email nay da co nguoi dung roi.";
    case "auth/weak-password":          return "Mat khau qua ngan - phai tu 6 ky tu tro len.";
    case "auth/user-not-found":
    case "auth/wrong-password":
    case "auth/invalid-credential":     return "Sai email hoac mat khau.";
    case "auth/too-many-requests":      return "Thu qua nhieu lan. Doi mot lat roi thu lai.";
    case "auth/network-request-failed": return "Mat ket noi mang.";
    case "auth/operation-not-allowed":
      return "Dang nhap bang email chua duoc bat trong Firebase Console.";
    case "auth/configuration-not-found":
      return "Firebase Authentication chua duoc khoi tao cho du an nay. "
           + "Vao Firebase Console > Authentication > Get started, "
           + "roi bat Email/Password.";
    case "auth/unauthorized-domain":
      return "Ten mien nay chua duoc cap phep. Vao Firebase Console > "
           + "Authentication > Settings > Authorized domains de them.";
    case "permission-denied":
    case "PERMISSION_DENIED":           return "Khong du quyen. Tai khoan cua ban co the da bi khoa.";
    default:
      return (loi && loi.message) ? loi.message : "Co loi khong ro.";
  }
}
