// =====================================================================
//  TRANG CHU - DANG KY / DANG NHAP NGAY TREN WEB (29/10/2026)
// =====================================================================
//
// Goi Firebase bang REST nhu chinh game (Assets/Scripts/Mang/FirebaseMang.cs + HoSoMang.cs) - khong nap
// Firebase JS SDK (~200 KB) chi de dang nhap. Ho so tao y het game: collection "nguoichoi", moi thanh tich
// bang 0 (firestore.rules bat buoc vay).
//
// PHIEN DUNG CHUNG VOI GAME: refresh token giu o localStorage khoa "diablo25d_refresh" - game o /choi/ cung
// goc (origin) nen doc duoc qua CauNoiCaiDat.jslib. Dang nhap o day roi bam "CHOI NGAY" la vao thang sanh;
// dang xuat o day (hoac trong game) thi ben kia cung ra.

(function () {
  "use strict";

  var KHOA_API = "AIzaSyDeC_T3VxJc-tc-EeX7Vl_ajGU28ZNeCZA";
  var XAC_THUC = "https://identitytoolkit.googleapis.com/v1/accounts";
  var LAM_MOI = "https://securetoken.googleapis.com/v1/token";
  var FIRESTORE = "https://firestore.googleapis.com/v1/projects/diablo25d-game/databases/(default)/documents";
  var KHOA_PHIEN = "diablo25d_refresh";

  // ---------------------------------------------------------------
  //  Kho phien
  // ---------------------------------------------------------------
  function docPhien() { try { return localStorage.getItem(KHOA_PHIEN) || ""; } catch (e) { return ""; } }
  function ghiPhien(t) { try { localStorage.setItem(KHOA_PHIEN, t || ""); } catch (e) { } }

  // ---------------------------------------------------------------
  //  Loi -> tieng Viet (chep tu FirebaseMang.DichLoi)
  // ---------------------------------------------------------------
  function dichLoi(t) {
    if (!t) return "Mất kết nối mạng.";
    if (t.indexOf("EMAIL_EXISTS") >= 0) return "Email này đã có người dùng rồi.";
    if (t.indexOf("INVALID_EMAIL") >= 0) return "Email không hợp lệ.";
    if (t.indexOf("WEAK_PASSWORD") >= 0) return "Mật khẩu phải từ 6 ký tự trở lên.";
    if (t.indexOf("EMAIL_NOT_FOUND") >= 0) return "Không có tài khoản nào dùng email này.";
    if (t.indexOf("INVALID_PASSWORD") >= 0) return "Sai mật khẩu.";
    if (t.indexOf("INVALID_LOGIN_CREDENTIALS") >= 0) return "Sai email hoặc mật khẩu.";
    if (t.indexOf("USER_DISABLED") >= 0) return "Tài khoản đã bị khoá.";
    if (t.indexOf("TOO_MANY_ATTEMPTS") >= 0) return "Thử quá nhiều lần. Đợi một lát rồi thử lại.";
    if (t.indexOf("PERMISSION_DENIED") >= 0 || t.indexOf("Missing or insufficient permissions") >= 0)
      return "Không đủ quyền. Tài khoản của bạn có thể đã bị khoá.";
    if (t.indexOf("TOKEN_EXPIRED") >= 0 || t.indexOf("INVALID_REFRESH_TOKEN") >= 0 || t.indexOf("UNAUTHENTICATED") >= 0)
      return "Phiên đăng nhập đã hết hạn, hãy đăng nhập lại.";
    return "Có lỗi xảy ra, hãy thử lại sau.";
  }

  function guiJson(url, cach, than, token) {
    var tieuDe = { "Content-Type": "application/json" };
    if (token) tieuDe.Authorization = "Bearer " + token;
    return fetch(url, { method: cach, headers: tieuDe, body: than ? JSON.stringify(than) : undefined })
      .then(function (r) {
        return r.text().then(function (t) { return { ok: r.ok, ma: r.status, chu: t }; });
      }, function () { return { ok: false, ma: 0, chu: "" }; });
  }

  // ---------------------------------------------------------------
  //  Firebase Auth + ho so
  // ---------------------------------------------------------------
  var phien = null;   // { uid, email, idToken }
  var hoSo = null;    // { ten, soTranChoi, soTranThang, ... }

  function xacThuc(hanh, email, matKhau) {
    return guiJson(XAC_THUC + hanh + "?key=" + KHOA_API, "POST",
      { email: email, password: matKhau, returnSecureToken: true }).then(function (k) {
      if (!k.ok) throw new Error(dichLoi(k.chu));
      var d = JSON.parse(k.chu);
      phien = { uid: d.localId, email: d.email, idToken: d.idToken };
      ghiPhien(d.refreshToken);
      return phien;
    });
  }

  function lamMoi(refresh) {
    return fetch(LAM_MOI + "?key=" + KHOA_API, {
      method: "POST",
      headers: { "Content-Type": "application/x-www-form-urlencoded" },
      body: "grant_type=refresh_token&refresh_token=" + encodeURIComponent(refresh)
    }).then(function (r) {
      return r.json().then(function (d) {
        if (!r.ok) throw new Error(dichLoi(JSON.stringify(d)));
        phien = { uid: d.user_id, email: "", idToken: d.id_token };
        ghiPhien(d.refresh_token);
        return phien;
      });
    });
  }

  function giaTri(f) {
    if (!f) return null;
    if ("stringValue" in f) return f.stringValue;
    if ("integerValue" in f) return parseInt(f.integerValue, 10);
    if ("booleanValue" in f) return f.booleanValue;
    return null;
  }

  function docHoSo(f) {
    return {
      ten: giaTri(f.ten) || "", email: giaTri(f.email) || "",
      soTranChoi: giaTri(f.soTranChoi) || 0, soTranThang: giaTri(f.soTranThang) || 0,
      soQuaiDaDiet: giaTri(f.soQuaiDaDiet) || 0, soNguoiDaHa: giaTri(f.soNguoiDaHa) || 0,
      biKhoa: giaTri(f.biKhoa) === true
    };
  }

  // Doc ho so; chua co (404) thi tao voi moi thanh tich bang 0 - y het HoSoMang.TaiHoacTao.
  function taiHoacTao(tenNeuTao) {
    var duong = FIRESTORE + "/nguoichoi/" + phien.uid;
    return guiJson(duong, "GET", null, phien.idToken).then(function (k) {
      if (k.ok) {
        hoSo = docHoSo(JSON.parse(k.chu).fields || {});
        if (hoSo.biKhoa) { dangXuat(); throw new Error("Tài khoản của bạn đã bị khoá. Hãy liên hệ quản trị viên."); }
        return hoSo;
      }
      if (k.ma !== 404) throw new Error(dichLoi(k.chu));
      var ten = (tenNeuTao || (phien.email || "NguoiChoi").split("@")[0]).slice(0, 16);
      if (ten.length < 2) ten = (ten + "__").slice(0, 2);
      var bayGio = new Date().toISOString().replace(/\.\d+Z$/, "Z");
      var than = { fields: {
        ten: { stringValue: ten }, email: { stringValue: phien.email || "" },
        ngayTao: { timestampValue: bayGio }, lanDangNhapCuoi: { timestampValue: bayGio },
        soTranChoi: { integerValue: "0" }, soTranThang: { integerValue: "0" },
        soQuaiDaDiet: { integerValue: "0" }, soNguoiDaHa: { integerValue: "0" },
        biKhoa: { booleanValue: false }, ghiChuAdmin: { stringValue: "" }
      } };
      return guiJson(FIRESTORE + "/nguoichoi?documentId=" + phien.uid, "POST", than, phien.idToken).then(function (k2) {
        if (!k2.ok) throw new Error("Không tạo được hồ sơ: " + dichLoi(k2.chu));
        hoSo = { ten: ten, email: phien.email || "", soTranChoi: 0, soTranThang: 0, soQuaiDaDiet: 0, soNguoiDaHa: 0, biKhoa: false };
        return hoSo;
      });
    });
  }

  function dangXuat() {
    phien = null; hoSo = null;
    ghiPhien("");
    veTaiKhoan();
  }

  // ---------------------------------------------------------------
  //  Giao dien tai khoan
  // ---------------------------------------------------------------
  var $ = function (id) { return document.getElementById(id); };

  function veTaiKhoan() {
    var co = !!hoSo;
    document.body.classList.toggle("da-dang-nhap", co);
    if (co) {
      $("ten-nguoi-choi").textContent = hoSo.ten;
      $("ten-chao").textContent = hoSo.ten;
      $("so-tran").textContent = hoSo.soTranChoi + " trận đã chơi · " + hoSo.soTranThang + " trận thắng";
    }
  }

  var bang = $("bang-tai-khoan");
  var cheDo = "dangnhap";

  function moBang(c) {
    cheDo = c;
    bang.classList.toggle("dang-ky", c === "dangky");
    $("tab-dang-nhap").classList.toggle("chon", c !== "dangky");
    $("tab-dang-ky").classList.toggle("chon", c === "dangky");
    $("tab-dang-nhap").setAttribute("aria-selected", c !== "dangky");
    $("tab-dang-ky").setAttribute("aria-selected", c === "dangky");
    $("nut-gui").textContent = c === "dangky" ? "TẠO TÀI KHOẢN" : "ĐĂNG NHẬP";
    $("o-mat-khau").setAttribute("autocomplete", c === "dangky" ? "new-password" : "current-password");
    bao("", false);
    if (!bang.open) bang.showModal();
    setTimeout(function () { (c === "dangky" ? $("o-ten") : $("o-email")).focus(); }, 30);
  }

  function bao(chu, loi) {
    var b = $("bao-loi");
    b.textContent = chu || "";
    b.classList.toggle("loi", !!loi);
  }

  var dangGui = false;
  function gui(e) {
    e.preventDefault();
    if (dangGui) return;
    var ten = $("o-ten").value.trim(), email = $("o-email").value.trim(), mk = $("o-mat-khau").value;
    if (cheDo === "dangky" && ten.length < 2) return bao("Tên phải từ 2 ký tự trở lên.", true);
    if (!email || email.indexOf("@") < 0) return bao("Email không hợp lệ.", true);
    if (mk.length < 6) return bao("Mật khẩu phải từ 6 ký tự trở lên.", true);
    dangGui = true;
    $("nut-gui").disabled = true;
    bao(cheDo === "dangky" ? "Đang tạo tài khoản..." : "Đang đăng nhập...", false);
    xacThuc(cheDo === "dangky" ? ":signUp" : ":signInWithPassword", email, mk)
      .then(function () { return taiHoacTao(cheDo === "dangky" ? ten : null); })
      .then(function () {
        $("o-mat-khau").value = "";
        bang.close();
        veTaiKhoan();
      })
      .catch(function (err) {
        if (!hoSo) { phien = null; ghiPhien(""); }
        bao(err.message || "Không đăng nhập được.", true);
      })
      .then(function () { dangGui = false; $("nut-gui").disabled = false; });
  }

  // ---------------------------------------------------------------
  //  Gan su kien
  // ---------------------------------------------------------------
  document.querySelectorAll("[data-mo]").forEach(function (n) {
    n.addEventListener("click", function () { moBang(n.getAttribute("data-mo")); });
  });
  $("tab-dang-nhap").addEventListener("click", function () { moBang("dangnhap"); });
  $("tab-dang-ky").addEventListener("click", function () { moBang("dangky"); });
  $("dong-bang").addEventListener("click", function () { bang.close(); });
  bang.addEventListener("click", function (e) { if (e.target === bang) bang.close(); });
  $("form-tai-khoan").addEventListener("submit", gui);
  document.querySelectorAll("[data-dang-xuat]").forEach(function (n) { n.addEventListener("click", dangXuat); });

  // Thanh tren cung hien nut CHOI NGAY nho khi nut lon da cuon khoi man hinh
  var nutLon = $("choi-ngay");
  if ("IntersectionObserver" in window && nutLon) {
    new IntersectionObserver(function (ds) {
      document.body.classList.toggle("cuon-qua-nut", !ds[0].isIntersecting);
    }).observe(nutLon);
  }

  // Phong to anh ky nang khi bam
  var xem = $("xem-anh");
  document.querySelectorAll(".anh-ky-nang img").forEach(function (im) {
    im.addEventListener("click", function () {
      $("xem-anh-img").src = im.currentSrc || im.src;
      $("xem-anh-img").alt = im.alt;
      xem.showModal();
    });
  });
  xem.addEventListener("click", function () { xem.close(); });

  // Phien con tu lan truoc (o day hoac trong game)
  var luu = docPhien();
  if (luu) {
    lamMoi(luu).then(function () { return taiHoacTao(null); })
      .then(veTaiKhoan)
      .catch(function () { phien = null; hoSo = null; ghiPhien(""); veTaiKhoan(); });
  }

  // Tan lua bay len (canvas nhe, dung khi nguoi dung xin giam chuyen dong)
  var cv = $("tan-lua");
  var giam = window.matchMedia && window.matchMedia("(prefers-reduced-motion: reduce)").matches;
  if (cv && cv.getContext && !giam) {
    var g = cv.getContext("2d"), hat = [], W = 0, H = 0;
    function doi() { W = cv.width = cv.clientWidth; H = cv.height = cv.clientHeight; }
    window.addEventListener("resize", doi); doi();
    function moi(dau) {
      return { x: Math.random() * W, y: dau ? Math.random() * H : H + 10, v: 0.35 + Math.random() * 0.9,
               r: 0.6 + Math.random() * 1.8, l: Math.random() * 6.28, a: 0.35 + Math.random() * 0.6 };
    }
    for (var i = 0; i < 70; i++) hat.push(moi(true));
    (function ve() {
      g.clearRect(0, 0, W, H);
      for (var i = 0; i < hat.length; i++) {
        var h = hat[i];
        h.y -= h.v; h.l += 0.02; h.x += Math.sin(h.l) * 0.4;
        if (h.y < -10) hat[i] = h = moi(false);
        var t = Math.max(0, Math.min(1, h.y / H));
        g.fillStyle = "rgba(255," + (90 + 90 * t | 0) + ",40," + (h.a * t).toFixed(3) + ")";
        g.beginPath(); g.arc(h.x, h.y, h.r, 0, 6.283); g.fill();
      }
      requestAnimationFrame(ve);
    })();
  }
})();
