using System.Text;

/// <summary>
/// GHEP DAU ROI THANH CHU DUNG SAN (tieng Viet).
///
/// Mot so bo go (Unikey che do "Unicode to hop", may Mac, dien thoai) luu chu
/// co dau thanh CHU CAI + KY TU DAU ROI di sau: "a" + U+0302 + U+0309 thay vi
/// mot chu "ẩ". Bo ve chu cua Unity (OnGUI, font dong) khong biet dat dau roi
/// len dung chu cai - dau lech sang ben, chong len chu sau, hoac mat han. Ten
/// nguoi choi do nguoi ta tu go nen phai ghep lai truoc khi ve.
///
/// Khong dung string.Normalize cua .NET: chua chac chay tren ban WebGL
/// (IL2CPP). Tu ghep bang mot LUOI 12 nguyen am x 6 thanh + 6 quy tac mu /
/// trang / moc - du ca 134 chu co dau (tru d/D gach, khong co dang tach).
/// Phep thu menu 52 doi chieu voi chuan Unicode cua .NET trong Editor.
/// </summary>
public static class GhepDauTiengViet
{
    // Moi hang: nguyen am goc roi 5 thanh theo thu tu huyen, sac, hoi, nga, nang
    static readonly string[] HangThuong =
    {
        "aàáảãạ", "ăằắẳẵặ", "âầấẩẫậ", "eèéẻẽẹ", "êềếểễệ", "iìíỉĩị",
        "oòóỏõọ", "ôồốổỗộ", "ơờớởỡợ", "uùúủũụ", "ưừứửữự", "yỳýỷỹỵ",
    };
    static readonly string[] HangHoa =
    {
        "AÀÁẢÃẠ", "ĂẰẮẲẴẶ", "ÂẦẤẨẪẬ", "EÈÉẺẼẸ", "ÊỀẾỂỄỆ", "IÌÍỈĨỊ",
        "OÒÓỎÕỌ", "ÔỒỐỔỖỘ", "ƠỜỚỞỠỢ", "UÙÚỦŨỤ", "ƯỪỨỬỮỰ", "YỲÝỶỸỴ",
    };
    // Chi so hang
    const int A = 0, AW = 1, AA = 2, E = 3, EE = 4, O = 6, OO = 7, OW = 8, U = 9, UW = 10;

    /// <summary>Ma dau thanh -> cot (1..5); 0 neu khong phai dau thanh.</summary>
    static int CotThanh(char d)
    {
        switch (d)
        {
            case '̀': return 1;   // huyen
            case '́': return 2;   // sac
            case '̉': return 3;   // hoi
            case '̃': return 4;   // nga
            case '̣': return 5;   // nang
        }
        return 0;
    }

    /// <summary>Dau mu / trang / moc doi hang nguyen am; -1 neu khong ghep duoc.</summary>
    static int DoiHang(int hang, char d)
    {
        if (d == '̆') return hang == A ? AW : -1;                    // trang: ă
        if (d == '̂')                                                  // mu: â ê ô
            return hang == A ? AA : hang == E ? EE : hang == O ? OO : -1;
        if (d == '̛') return hang == O ? OW : hang == U ? UW : -1;   // moc: ơ ư
        return -1;
    }

    static bool LaDauRoi(char c) { return c >= '̀' && c <= 'ͯ'; }

    static bool TachChu(char c, out int hang, out int thanh, out bool hoa)
    {
        for (int h = 0; h < HangThuong.Length; h++)
        {
            int i = HangThuong[h].IndexOf(c);
            if (i >= 0) { hang = h; thanh = i; hoa = false; return true; }
            i = HangHoa[h].IndexOf(c);
            if (i >= 0) { hang = h; thanh = i; hoa = true; return true; }
        }
        hang = thanh = 0; hoa = false;
        return false;
    }

    /// <summary>
    /// Ghep moi cum "chu cai + dau roi" thanh chu dung san. Chuoi khong co dau
    /// roi nao thi tra ve y nguyen (khong cap phat gi). Cum nao khong ghep duoc
    /// (dau la, chu khong phai nguyen am) thi giu nguyen.
    /// </summary>
    public static string Ghep(string s)
    {
        if (string.IsNullOrEmpty(s)) return s;
        bool co = false;
        for (int k = 0; k < s.Length; k++) if (LaDauRoi(s[k])) { co = true; break; }
        if (!co) return s;

        var sb = new StringBuilder(s.Length);
        int i = 0;
        while (i < s.Length)
        {
            int j = i + 1;
            while (j < s.Length && LaDauRoi(s[j])) j++;

            int hang, thanh; bool hoa;
            if (j > i + 1 && TachChu(s[i], out hang, out thanh, out hoa))
            {
                bool duoc = true;
                for (int k = i + 1; k < j && duoc; k++)
                {
                    int cot = CotThanh(s[k]);
                    if (cot > 0) { if (thanh == 0) thanh = cot; else duoc = false; }
                    else { hang = DoiHang(hang, s[k]); duoc = hang >= 0; }
                }
                if (duoc)
                {
                    sb.Append((hoa ? HangHoa : HangThuong)[hang][thanh]);
                    i = j;
                    continue;
                }
            }
            sb.Append(s, i, j - i);
            i = j;
        }
        return sb.ToString();
    }
}
