// CAI DAT CUA NGUOI CHOI - LUU THANG VAO localStorage CUA TRINH DUYET.
//
// Vi sao khong dung PlayerPrefs: tren WebGL, PlayerPrefs ghi xuong IndexedDB
// KHONG DONG BO. Nguoi choi bam OK la trang tai lai ngay - ghi chua kip xong
// thi lua chon mat, nguoi choi thay game "khong nghe loi". localStorage thi
// ghi xong ngay truoc khi ham tra ve.
//
// Va trang bao quanh game (index.html) phai DOC DUOC lua chon nay TRUOC khi
// Unity khoi dong - de dat do phan giai cua canvas. Trang web doc duoc
// localStorage, con PlayerPrefs thi nam trong file cua Unity, khong doc duoc.

var CauNoiCaiDat = {

  // Doc mot so nguyen; khong co (hoac trinh duyet cam luu) thi tra mac dinh
  CD_DocSo: function (khoa, macDinh) {
    try {
      var v = window.localStorage.getItem(UTF8ToString(khoa));
      if (v === null) return macDinh;
      var n = parseInt(v, 10);
      return isNaN(n) ? macDinh : n;
    } catch (e) {
      return macDinh;
    }
  },

  // Ghi mot so nguyen. Tra 1 neu ghi duoc, 0 neu trinh duyet cam luu
  // (che do an danh cua mot so trinh duyet, hoac het cho).
  CD_GhiSo: function (khoa, giaTri) {
    try {
      window.localStorage.setItem(UTF8ToString(khoa), String(giaTri));
      return 1;
    } catch (e) {
      return 0;
    }
  },

  // Tai lai ca trang - Unity khoi dong lai tu dau voi cai dat moi
  CD_NapLai: function () {
    window.location.reload();
  }
};

mergeInto(LibraryManager.library, CauNoiCaiDat);
