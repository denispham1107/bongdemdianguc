// ================================================================
//  BO CHAY NEN (service worker) - CHI DE CAI DUOC UNG DUNG
// ================================================================
//
// Chrome / Edge / Android chi cho "Cai dat ung dung" khi trang co manifest VA
// mot bo chay nen co xu ly su kien fetch. Day la tat ca viec cua file nay.
//
// CO Y KHONG LUU CACHE GI CA:
//   - Ban game nang 174 MB va Unity DA tu quan ly cache rieng cua no
//     (Cache Storage, theo ma phien ban trong duong dan "?v=..."). Luu them mot
//     ban nua o day la ton gap doi cho tren may nguoi choi.
//   - Va nguy hiem hon: mot bo chay nen giu ban cu cua index.html se ghep ma
//     game CU voi du lieu MOI sau moi lan cap nhat - dung cai loi da lam game
//     sap luc tai (xem HUONG-DAN, muc "Game sap ngay luc tai").
//
// Nen o day chi chuyen tiep thang ra mang. Muon choi ngoai mang thi phai tinh
// lai ca cach cache ban build - viec rieng, chua lam.

self.addEventListener("install", function (e) {
  self.skipWaiting();
});

self.addEventListener("activate", function (e) {
  e.waitUntil(self.clients.claim());
});

self.addEventListener("fetch", function (e) {
  // Khong goi e.respondWith() cho moi thu: de trinh duyet tu lay nhu binh
  // thuong. Chi "cham vao" cac yeu cau dieu huong de trinh duyet ghi nhan la
  // bo chay nen nay CO xu ly fetch.
  if (e.request.mode === "navigate") {
    e.respondWith(fetch(e.request));
  }
});
