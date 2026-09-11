using System.Collections.Generic;
using System.IO;

/// <summary>
/// DOC BANG KY TU (cmap) CUA MOT FILE FONT TTF/OTF - khong qua Unity.
///
/// Vi sao khong hoi Font.HasCharacter: trong Editor, font dong (dynamic) cua
/// Unity tu MUON font cua Windows de ve nhung chu no thieu, va HasCharacter
/// tra loi theo ca phan muon ay. Phep thu menu 48 cu hoi font mac dinh bang
/// cach do va bao "thieu: khong" - trong khi len ban web (khong co gi de muon)
/// thi "CAI DAT" hien thanh "CAI D T". Doc thang file font thi khong ai noi
/// doi duoc.
///
/// Doc bang con dang 4 (BMP) va dang 12 (toan bo Unicode).
/// </summary>
public static class BangKyTuFont
{
    /// <summary>Tap ma Unicode font ve duoc. Null neu khong doc duoc file.</summary>
    public static HashSet<int> Doc(string duong)
    {
        if (!File.Exists(duong)) return null;
        byte[] b = File.ReadAllBytes(duong);
        var ma = new HashSet<int>();

        int soBang = U16(b, 4);
        int cmap = -1;
        for (int i = 0; i < soBang; i++)
        {
            int o = 12 + 16 * i;
            string tag = "" + (char)b[o] + (char)b[o + 1] + (char)b[o + 2] + (char)b[o + 3];
            if (tag == "cmap") { cmap = (int)U32(b, o + 8); break; }
        }
        if (cmap < 0) return null;

        int soBangCon = U16(b, cmap + 2);
        for (int i = 0; i < soBangCon; i++)
        {
            int st = cmap + (int)U32(b, cmap + 4 + 8 * i + 4);
            int dang = U16(b, st);
            if (dang == 4)
            {
                int segX2 = U16(b, st + 6), seg = segX2 / 2;
                int dauEnd = st + 14, dauStart = dauEnd + segX2 + 2;
                int dauDelta = dauStart + segX2, dauOff = dauDelta + segX2;
                for (int k = 0; k < seg; k++)
                {
                    int cuoi = U16(b, dauEnd + 2 * k), dau = U16(b, dauStart + 2 * k);
                    int delta = (short)U16(b, dauDelta + 2 * k);
                    int roff = U16(b, dauOff + 2 * k);
                    for (int c = dau; c <= cuoi && c != 0xFFFF; c++)
                    {
                        int g;
                        if (roff == 0) g = (c + delta) & 0xFFFF;
                        else
                        {
                            int gi = dauOff + 2 * k + roff + 2 * (c - dau);
                            g = U16(b, gi);
                            if (g != 0) g = (g + delta) & 0xFFFF;
                        }
                        if (g != 0) ma.Add(c);
                    }
                }
            }
            else if (dang == 12)
            {
                long nhom = U32(b, st + 12);
                for (long k = 0; k < nhom; k++)
                {
                    int o = st + 16 + (int)(12 * k);
                    long dau = U32(b, o), cuoi = U32(b, o + 4);
                    for (long c = dau; c <= cuoi; c++) ma.Add((int)c);
                }
            }
        }
        return ma;
    }

    static int U16(byte[] b, int o) { return (b[o] << 8) | b[o + 1]; }
    static long U32(byte[] b, int o) { return ((long)b[o] << 24) | ((long)b[o + 1] << 16) | ((long)b[o + 2] << 8) | b[o + 3]; }

    /// <summary>Du 134 chu co dau cua tieng Viet (thuong va hoa).</summary>
    public const string ChuVietCoDau =
        "àáảãạăằắẳẵặâầấẩẫậèéẻẽẹêềếểễệìíỉĩịòóỏõọôồốổỗộơờớởỡợùúủũụưừứửữựỳýỷỹỵđ" +
        "ÀÁẢÃẠĂẰẮẲẴẶÂẦẤẨẪẬÈÉẺẼẸÊỀẾỂỄỆÌÍỈĨỊÒÓỎÕỌÔỒỐỔỖỘƠỜỚỞỠỢÙÚỦŨỤƯỪỨỬỮỰỲÝỶỸỴĐ";
}
