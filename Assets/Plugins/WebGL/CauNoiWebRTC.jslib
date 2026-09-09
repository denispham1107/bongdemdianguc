// CAU NOI WEBRTC CHO BAN WEBGL.
//
// Unity WebGL khong co WebRTC san (goi com.unity.webrtc khong chay tren nen
// nay). Nhung TRINH DUYET thi co san RTCPeerConnection - file nay goi thang
// API do roi bao ket qua nguoc len C#.
//
// VI SAO KHONG DUNG REALTIME DATABASE CHO VI TRI NHAN VAT: moi goi phai bay
// sang Singapore roi bay ve, do duoc 49 ms chi rieng duong truyen. Noi thang
// may voi may trong nuoc thi con 10-30 ms - do la ca khoang cach giua "dat"
// va "khong dat" muc tieu 50-80 ms.
//
// Chu y ve chuoi: chuoi tu C# sang la con tro byte UTF8, phai doi bang
// UTF8ToString; chuoi tra ve C# phai cap phat trong heap cua Unity roi C# tu
// giai phong - khong thi ro ri bo nho, ma mot tran dau co hang nghin goi.

var CauNoiWebRTC = {

  $trangThaiRTC: {
    pc: null,
    kenh: null,
    daMo: false,
    hangCho: [],          // tin nhan da nhan, cho C# lay ra
    ungVienCuaToi: [],    // ICE candidate cua may nay, cho C# gui di

    // UNG VIEN CUA BEN KIA DEN SOM THI PHAI XEP HANG, KHONG DUOC VUT DI.
    //
    // Firebase phat lai toan bo ung vien da co ngay khi minh bat dau nghe, nen
    // ung vien dau tien cua ben kia thuong den TRUOC khi minh kip
    // setRemoteDescription. Goi addIceCandidate luc do la nem loi. Ban dau toi
    // nuot loi do bang mot catch rong - ket qua la mat sach ung vien cua ben
    // kia va ICE khong co duong nao de thu. Cung mang LAN thi van noi duoc nho
    // ung vien noi bo sinh sau, nen loi bi che kin.
    hangUngVienCho: [],
    daCoMoTaBenKia: false,
    moTa: "",             // offer/answer vua tao ra
    loi: "",
    kieuKetNoi: "",

    themUngVien: function (uv) {
      var t = trangThaiRTC;
      if (!t.daCoMoTaBenKia) { t.hangUngVienCho.push(uv); return; }
      try {
        t.pc.addIceCandidate(new RTCIceCandidate(uv))
          .catch(function (e) { t.loi = "them ung vien hong: " + e; });
      } catch (e) { t.loi = "them ung vien hong: " + e; }
    },

    /// Goi NGAY SAU moi lan setRemoteDescription.
    xaHangUngVien: function () {
      var t = trangThaiRTC;
      t.daCoMoTaBenKia = true;
      while (t.hangUngVienCho.length > 0) t.themUngVien(t.hangUngVienCho.shift());
    },

    // Chuoi tra ve C# phai nam trong heap cua Unity. C# goi Marshal.FreeHGlobal
    // de tra lai - xem KenhTrucTiep.LayChuoi.
    traChuoi: function (s) {
      var n = lengthBytesUTF8(s) + 1;
      var p = _malloc(n);
      stringToUTF8(s, p, n);
      return p;
    }
  },

  /// Tao ket noi. mayChu = danh sach STUN, ngan cach bang dau phay.
  RTC_Tao: function (dsStun) {
    var ds = UTF8ToString(dsStun).split(",").filter(function (x) { return x.length > 0; });
    var t = trangThaiRTC;
    t.daMo = false; t.hangCho = []; t.ungVienCuaToi = [];
    t.moTa = ""; t.loi = ""; t.kieuKetNoi = "";
    t.hangUngVienCho = []; t.daCoMoTaBenKia = false;

    try {
      t.pc = new RTCPeerConnection({ iceServers: [{ urls: ds }] });
    } catch (e) {
      t.loi = "khong tao duoc RTCPeerConnection: " + e;
      return 0;
    }

    t.pc.onicecandidate = function (su) {
      if (su.candidate) t.ungVienCuaToi.push(JSON.stringify(su.candidate.toJSON()));
    };

    t.pc.oniceconnectionstatechange = function () {
      if (t.pc.iceConnectionState === "failed") t.loi = "ICE that bai";
    };

    // May tra loi khong tu tao kenh - no nhan kenh do may moi tao ra
    t.pc.ondatachannel = function (su) {
      t.kenh = su.channel;
      trangThaiRTC.ganKenh(t.kenh);
    };

    t.ganKenh = function (k) {
      k.binaryType = "arraybuffer";
      k.onopen = function () { t.daMo = true; };
      k.onclose = function () { t.daMo = false; };
      k.onmessage = function (su) { t.hangCho.push(su.data); };
    };

    return 1;
  },

  /// May MOI: tao kenh khong tin cay roi sinh loi moi (offer).
  RTC_TaoLoiMoi: function () {
    var t = trangThaiRTC;
    if (!t.pc) { t.loi = "chua tao ket noi"; return; }

    // ordered:false + maxRetransmits:0 = hanh xu nhu UDP.
    // De mac dinh (tin cay, dung thu tu) thi mot goi rot se CHAN moi goi sau
    // no - do tre vot len 300 ms dung luc dang danh nhau. Vi tri cu 20 ms
    // truoc thi gui lai cung vo ich, thà bỏ.
    t.kenh = t.pc.createDataChannel("tran", { ordered: false, maxRetransmits: 0 });
    t.ganKenh(t.kenh);

    t.pc.createOffer().then(function (moi) {
      return t.pc.setLocalDescription(moi).then(function () {
        t.moTa = JSON.stringify({ type: moi.type, sdp: moi.sdp });
      });
    }).catch(function (e) { t.loi = "tao loi moi that bai: " + e; });
  },

  /// May VAO: nhan loi moi cua may kia roi sinh cau tra loi (answer).
  RTC_TraLoi: function (jsonMoi) {
    var t = trangThaiRTC;
    if (!t.pc) { t.loi = "chua tao ket noi"; return; }
    var moi = JSON.parse(UTF8ToString(jsonMoi));

    t.pc.setRemoteDescription(new RTCSessionDescription(moi)).then(function () {
      t.xaHangUngVien();
      return t.pc.createAnswer();
    }).then(function (tra) {
      return t.pc.setLocalDescription(tra).then(function () {
        t.moTa = JSON.stringify({ type: tra.type, sdp: tra.sdp });
      });
    }).catch(function (e) { t.loi = "tra loi that bai: " + e; });
  },

  /// May MOI: nhan cau tra loi cua may kia.
  RTC_NhanTraLoi: function (jsonTra) {
    var t = trangThaiRTC;
    if (!t.pc) return;
    var tra = JSON.parse(UTF8ToString(jsonTra));
    t.pc.setRemoteDescription(new RTCSessionDescription(tra))
      .then(function () { t.xaHangUngVien(); })
      .catch(function (e) { t.loi = "nhan tra loi that bai: " + e; });
  },

  RTC_ThemUngVien: function (jsonUv) {
    var t = trangThaiRTC;
    if (!t.pc) return;
    try { t.themUngVien(JSON.parse(UTF8ToString(jsonUv))); }
    catch (e) { t.loi = "ung vien khong doc duoc: " + e; }
  },

  /// Lay offer/answer vua tao. Chua co thi tra ve chuoi rong.
  RTC_LayMoTa: function () {
    var t = trangThaiRTC;
    var s = t.moTa; t.moTa = "";
    return t.traChuoi(s);
  },

  /// Lay mot ung vien ICE cua may nay de gui sang may kia. Het thi chuoi rong.
  RTC_LayUngVien: function () {
    var t = trangThaiRTC;
    var s = t.ungVienCuaToi.length > 0 ? t.ungVienCuaToi.shift() : "";
    return t.traChuoi(s);
  },

  RTC_DaMo: function () { return trangThaiRTC.daMo ? 1 : 0; },

  RTC_LayLoi: function () {
    var t = trangThaiRTC;
    var s = t.loi; t.loi = "";
    return t.traChuoi(s);
  },

  RTC_Gui: function (tin) {
    var t = trangThaiRTC;
    if (!t.daMo || !t.kenh) return 0;
    try { t.kenh.send(UTF8ToString(tin)); return 1; }
    catch (e) { return 0; }
  },

  /// Lay mot tin nhan da nhan. Het thi chuoi rong.
  RTC_Nhan: function () {
    var t = trangThaiRTC;
    var s = t.hangCho.length > 0 ? t.hangCho.shift() : "";
    return t.traChuoi(typeof s === "string" ? s : "");
  },

  /// Noi thang hay phai nho nguoi khac tiep suc. Doc luc DANG NOI, khong doi
  /// den luc dong - dong roi thi khong con cap ung vien nao de xem.
  RTC_CapNhatKieuKetNoi: function () {
    var t = trangThaiRTC;
    if (!t.pc || !t.pc.getStats) return;
    t.pc.getStats().then(function (so) {
      var cap = null;
      so.forEach(function (b) {
        if (b.type === "candidate-pair" && b.state === "succeeded" && b.nominated) cap = b;
      });
      if (!cap) return;
      var toi = "?", kia = "?";
      so.forEach(function (b) {
        if (b.id === cap.localCandidateId) toi = b.candidateType;
        if (b.id === cap.remoteCandidateId) kia = b.candidateType;
      });
      t.kieuKetNoi = toi + "/" + kia;
    }).catch(function () {});
  },

  RTC_LayKieuKetNoi: function () {
    return trangThaiRTC.traChuoi(trangThaiRTC.kieuKetNoi);
  },

  RTC_Dong: function () {
    var t = trangThaiRTC;
    if (t.kenh) { try { t.kenh.close(); } catch (e) {} }
    if (t.pc) { try { t.pc.close(); } catch (e) {} }
    t.pc = null; t.kenh = null; t.daMo = false;
    t.hangCho = []; t.ungVienCuaToi = [];
  }
};

autoAddDeps(CauNoiWebRTC, '$trangThaiRTC');
mergeInto(LibraryManager.library, CauNoiWebRTC);
