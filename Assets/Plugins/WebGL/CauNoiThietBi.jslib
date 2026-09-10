// HOI TRINH DUYET: DAY CO PHAI MAY DI DONG KHONG.
//
// Vi sao khong hoi Unity: tren WebGL, Application.isMobilePlatform luon tra ve
// FALSE ke ca khi dang chay tren dien thoai that. Con Input.touchSupported thi
// nguoc lai - no tra ve TRUE tren gan nhu moi may tinh Windows doi moi, vi
// Chrome bao co ho tro cam ung theo API cua he dieu hanh chu khong theo viec
// may co man hinh cam ung hay khong. Dung no lam thuoc do thi nguoi choi ngoi
// truoc man hinh 27 inch va ban phim co bi bat vao che do joystick ao.
//
// Nen phai hoi thang trinh duyet.

var CauNoiThietBi = {

  // 1 = dien thoai / may tinh bang, 0 = may tinh de ban hoac xach tay
  TB_LaDiDong: function () {
    try {
      var nav = window.navigator;

      // Cach dang tin nhat khi co: Chrome/Edge khai bao thang.
      if (nav.userAgentData && typeof nav.userAgentData.mobile === 'boolean') {
        if (nav.userAgentData.mobile) return 1;
        // Khong ket luan "khong phai" o day: userAgentData.mobile bao FALSE cho
        // ca may tinh bang, ma may tinh bang thi van phai choi bang cam ung.
      }

      var ua = nav.userAgent || '';

      // Dien thoai va may tinh bang Android. KHONG doi hoi chu "Mobile":
      // dien thoai Android co chu ay, may tinh bang Android thi KHONG - loc
      // theo "Mobile" la bo sot toan bo may tinh bang Android.
      if (/Android/i.test(ua)) return 1;

      // iPhone, iPod, va iPad doi cu
      if (/iPhone|iPod|iPad/i.test(ua)) return 1;

      // IPAD DOI MOI TU CHO LA MAY MAC.
      //
      // Tu iPadOS 13, Safari tren iPad khai bao user agent y het macOS -
      // "Macintosh; Intel Mac OS X" - va khong con chu "iPad" nao. Loc bang
      // ten thi ca dong iPad hien nay deu lot qua thanh may tinh de ban.
      //
      // Dau hieu phan biet: may Mac that KHONG co diem cham nao
      // (maxTouchPoints = 0), con iPad thi co it nhat nam.
      if (/Macintosh/i.test(ua) && (nav.maxTouchPoints || 0) > 1) return 1;

      // Vai dong may khac: Windows Phone, BlackBerry, Kindle, Opera Mini
      if (/webOS|BlackBerry|IEMobile|Opera Mini|Windows Phone|Silk/i.test(ua)) return 1;

      return 0;
    } catch (e) {
      // Hoi khong duoc thi coi la may tinh de ban: doan nham ve phia ay chi
      // lam nguoi dung dien thoai thay thanh ky nang kieu PC, con doan nham
      // phia kia thi nguoi dung may tinh bi mot cai joystick ao che mat man
      // hinh va khong go duoc.
      return 0;
    }
  }
};

mergeInto(LibraryManager.library, CauNoiThietBi);
