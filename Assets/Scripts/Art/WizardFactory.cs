using UnityEngine;

/// <summary>
/// DUNG HINH NHAN VAT PHU THUY (Archmage) hoan toan bang code.
///
/// Tao hinh theo mau: mot lao phap su gia, mat gan guoc xanh xao, TOC VA RAU BAC
/// dai buong xuong nguc, ao choang den tham anh tim co lop lot mau tim, ho a tiet
/// vang chay doc than truoc, giap vai co gai vang vuot len, vien da quy tim tren
/// nguc, mot cai so nho deo o that lung, va CAY GAY cao om vien pha le tim khong lo.
///
/// Bo xuong (CharacterRig) giu NGUYEN nhu cu de he hoat anh khong phai sua gi.
/// </summary>
public static class WizardFactory
{
    public static CharacterRig Build(Transform parent)
    {
        var go = new GameObject("Sorceress_Visual");
        go.transform.SetParent(parent, false);
        var rig = go.AddComponent<CharacterRig>();
        rig.bodyHeight = 1.8f;
        rig.bodyRadius = 0.32f;

        // ---------------- XUONG (giu nguyen de hoat anh chay dung) ----------------
        rig.root = rig.Bone("Root", go.transform, Vector3.zero);
        rig.hips = rig.Bone("Hips", rig.root, new Vector3(0f, 0.92f, 0f));
        rig.spine = rig.Bone("Spine", rig.hips, new Vector3(0f, 0.14f, 0f));
        rig.chest = rig.Bone("Chest", rig.spine, new Vector3(0f, 0.20f, 0f));
        rig.neck = rig.Bone("Neck", rig.chest, new Vector3(0f, 0.17f, 0f));
        rig.head = rig.Bone("Head", rig.neck, new Vector3(0f, 0.10f, 0f));

        rig.armL = rig.Bone("ArmL", rig.chest, new Vector3(0.17f, 0.11f, 0f));
        rig.armR = rig.Bone("ArmR", rig.chest, new Vector3(-0.17f, 0.11f, 0f));
        rig.foreArmL = rig.Bone("ForeArmL", rig.armL, new Vector3(0f, -0.27f, 0f));
        rig.foreArmR = rig.Bone("ForeArmR", rig.armR, new Vector3(0f, -0.27f, 0f));
        rig.handL = rig.Bone("HandL", rig.foreArmL, new Vector3(0f, -0.25f, 0f));
        rig.handR = rig.Bone("HandR", rig.foreArmR, new Vector3(0f, -0.25f, 0f));

        rig.legL = rig.Bone("LegL", rig.hips, new Vector3(0.10f, -0.03f, 0f));
        rig.legR = rig.Bone("LegR", rig.hips, new Vector3(-0.10f, -0.03f, 0f));
        rig.shinL = rig.Bone("ShinL", rig.legL, new Vector3(0f, -0.43f, 0f));
        rig.shinR = rig.Bone("ShinR", rig.legR, new Vector3(0f, -0.43f, 0f));
        rig.footL = rig.Bone("FootL", rig.shinL, new Vector3(0f, -0.40f, 0f));
        rig.footR = rig.Bone("FootR", rig.shinR, new Vector3(0f, -0.40f, 0f));

        rig.hitPoint = rig.Bone("HitPoint", rig.chest, new Vector3(0f, 0.05f, 0f));

        BuildLegs(rig);
        BuildBody(rig);
        BuildArms(rig);
        BuildHead(rig);

        // ---------------- CAY GAY PHEP ----------------
        rig.offHandAnchor = rig.Bone("StaffAnchor", rig.handL, Vector3.zero);
        // Bu lai do nghieng cua canh tay de cay gay dung THANG DUNG
        rig.offHandAnchor.localRotation = Quaternion.Euler(72f, 0f, 10f);
        rig.castPoint = BuildStaff(rig, rig.offHandAnchor);

        rig.weaponAnchor = rig.Bone("WeaponAnchor", rig.handR, Vector3.zero);

        return rig;
    }

    // ================================================================
    //  CHAN
    // ================================================================

    static void BuildLegs(CharacterRig rig)
    {
        var legMesh = ProcMesh.Taper(0.070f, 0.056f, 0.43f, 12, true, Color.white);
        var shinMesh = ProcMesh.Taper(0.056f, 0.046f, 0.40f, 12, true, Color.white);
        var bootMesh = BuildShoe(0.060f, 0.21f, 0.078f);

        AttachDown(rig, "ThighL", rig.legL, legMesh, Mats.RobeDark, 0.43f);
        AttachDown(rig, "ThighR", rig.legR, legMesh, Mats.RobeDark, 0.43f);
        AttachDown(rig, "ShinL", rig.shinL, shinMesh, Mats.RobeDark, 0.40f);
        AttachDown(rig, "ShinR", rig.shinR, shinMesh, Mats.RobeDark, 0.40f);

        rig.Attach("BootL", rig.footL, bootMesh, Mats.Leather, new Vector3(0f, -0.035f, 0.035f),
                   Quaternion.identity, Vector3.one, true);
        rig.Attach("BootR", rig.footR, bootMesh, Mats.Leather, new Vector3(0f, -0.035f, 0.035f),
                   Quaternion.identity, Vector3.one, true);
    }

    // ================================================================
    //  THAN NGUOI
    // ================================================================

    static void BuildBody(CharacterRig rig)
    {
        // Than gay guoc: nguc hep, eo thon
        var torso = ProcMesh.Revolve(new Vector2[]
        {
            new Vector2(0.138f, -0.14f),
            new Vector2(0.134f, -0.05f),
            new Vector2(0.150f,  0.05f),
            new Vector2(0.172f,  0.13f),
            new Vector2(0.182f,  0.20f),
            new Vector2(0.140f,  0.26f),
            new Vector2(0.066f,  0.30f),
        }, 20, true, true, Color.white, "torso");
        ProcMesh.ScaleMesh(torso, new Vector3(1f, 1f, 0.72f));
        rig.Attach("Torso", rig.spine, torso, Mats.Robe, Vector3.zero);

        // Vay ao dai, gau RACH THANH NHUNG MUI NHON
        var skirt = BuildSkirt(0.18f, 0.36f, 0.86f, 28, 12, 0.20f);
        rig.Attach("Skirt", rig.hips, skirt, Mats.RobeDark, new Vector3(0f, 0.02f, 0f));

        // Lop lot tim lo ra ben trong vay
        var lining = BuildSkirt(0.15f, 0.28f, 0.72f, 22, 8, 0.12f);
        var liningGo = rig.Attach("SkirtLining", rig.hips, lining, Mats.RobeLining,
                                  new Vector3(0f, 0.02f, 0f));
        SetTwoSided(liningGo);

        // Ho a tiet vang chay doc than truoc ao
        BuildFiligree(rig);

        // Ao khoac vai (mantle) - do xuong hai ben nhu trong hinh
        // Cung 145 -> 395 do: kin lung va hai ben, HO PHIA TRUOC cho thay nguc
        var mantle = ProcMesh.RevolveArc(new Vector2[]
        {
            new Vector2(0.080f,  0.21f),
            new Vector2(0.160f,  0.13f),
            new Vector2(0.202f,  0.03f),
            new Vector2(0.220f, -0.06f),
            new Vector2(0.212f, -0.13f),
        }, 22, 145f, 250f, Color.white, "mantle");
        ProcMesh.ScaleMesh(mantle, new Vector3(1f, 1f, 0.86f));
        var mantleGo = rig.Attach("Mantle", rig.chest, mantle, Mats.RobeDark, new Vector3(0f, 0.06f, -0.01f));
        SetTwoSided(mantleGo);

        // Ao choang dai sau lung: mat ngoai den, mat trong tim
        var cape = BuildCape(0.235f, 0.395f, 1.10f, 16, 12);
        var capeGo = rig.Attach("Cape", rig.chest, cape, Mats.RobeDark,
                                new Vector3(0f, 0.14f, -0.10f), Quaternion.Euler(6f, 0f, 0f), Vector3.one, false);
        SetTwoSided(capeGo);

        var capeIn = BuildCape(0.215f, 0.365f, 1.06f, 16, 12);
        var capeInGo = rig.Attach("CapeLining", rig.chest, capeIn, Mats.RobeLining,
                                  new Vector3(0f, 0.14f, -0.075f), Quaternion.Euler(6f, 0f, 0f), Vector3.one, false);
        SetTwoSided(capeInGo);

        // Co ao dung cao om lay gay
        var collar = ProcMesh.RevolveArc(new Vector2[]
        {
            new Vector2(0.094f, 0.170f),
            new Vector2(0.112f, 0.215f),
            new Vector2(0.124f, 0.262f),
        }, 18, 150f, 240f, Color.white, "collar");
        var collarGo = rig.Attach("Collar", rig.chest, collar, Mats.RobeDark,
                                  new Vector3(0f, 0.06f, -0.02f), Quaternion.identity, Vector3.one, false);
        SetTwoSided(collarGo);

        // Vien vang quanh co ao
        var collarTrim = ProcMesh.RevolveArc(new Vector2[]
        {
            new Vector2(0.122f, 0.250f),
            new Vector2(0.133f, 0.264f),
            new Vector2(0.122f, 0.278f),
        }, 18, 150f, 240f, Color.white, "collartrim");
        var ctGo = rig.Attach("CollarTrim", rig.chest, collarTrim, Mats.Trim,
                              new Vector3(0f, 0.06f, -0.02f), Quaternion.identity, Vector3.one, false);
        SetTwoSided(ctGo);

        // Vien da quy tim tren nguc, dat trong o vang
        rig.Attach("GemSetting", rig.chest, ProcMesh.Sphere(0.040f, 12, 8, 0.8f, Color.white), Mats.Trim,
                   new Vector3(0f, 0.10f, 0.118f), Quaternion.identity, new Vector3(1f, 1.35f, 0.5f), false);
        var gem = rig.Attach("ChestGem", rig.chest, BuildCrystal(0.030f, 0.075f), Mats.GemPurple,
                             new Vector3(0f, 0.10f, 0.132f), Quaternion.Euler(90f, 0f, 0f), Vector3.one, false);
        rig.eyeRenderers.Add(gem.GetComponent<Renderer>());

        // Giap vai co gai vang vuot len
        BuildPauldron(rig, rig.armL, 1f);
        BuildPauldron(rig, rig.armR, -1f);

        // Day lung vang + cai so nho deo phia truoc
        var belt = ProcMesh.Revolve(new Vector2[]
        {
            new Vector2(0.148f, -0.035f),
            new Vector2(0.162f,  0.000f),
            new Vector2(0.148f,  0.035f),
        }, 20, false, false, Color.white, "belt");
        ProcMesh.ScaleMesh(belt, new Vector3(1f, 1f, 0.76f));
        rig.Attach("Belt", rig.hips, belt, Mats.Trim, new Vector3(0f, 0.10f, 0f));

        BuildSkullCharm(rig, rig.hips, new Vector3(0f, 0.075f, 0.115f));
    }

    /// <summary>Hai dai vang manh chay doc than truoc ao choang.</summary>
    static void BuildFiligree(CharacterRig rig)
    {
        // Hai dai chum lai thanh chu V (khong con giong cai thang)
        var strip = ProcMesh.Box(new Vector3(0.017f, 0.32f, 0.012f), Color.white);
        rig.Attach("FiligreeL", rig.spine, strip, Mats.Trim,
                   new Vector3(0.050f, 0.06f, 0.118f), Quaternion.Euler(4f, 0f, -13f), Vector3.one, false);
        rig.Attach("FiligreeR", rig.spine, strip, Mats.Trim,
                   new Vector3(-0.050f, 0.06f, 0.118f), Quaternion.Euler(4f, 0f, 13f), Vector3.one, false);

        // Mot hat vang nho o dinh chu V, ngay duoi vien co
        var stud = ProcMesh.Sphere(0.019f, 8, 6, 1.1f, Color.white);
        rig.Attach("FiligreeStud", rig.spine, stud, Mats.Trim,
                   new Vector3(0f, 0.212f, 0.116f), Quaternion.identity, new Vector3(1f, 1f, 0.6f), false);
    }

    /// <summary>Mieng giap vai: mot mang cong om vai, tren co ba cai gai vang vuot len.</summary>
    static void BuildPauldron(CharacterRig rig, Transform arm, float side)
    {
        var pad = ProcMesh.Revolve(new Vector2[]
        {
            new Vector2(0.030f,  0.055f),
            new Vector2(0.080f,  0.020f),
            new Vector2(0.098f, -0.030f),
            new Vector2(0.092f, -0.075f),
        }, 14, false, false, Color.white, "pauldron");
        var padGo = rig.Attach("Pauldron", arm, pad, Mats.RobeDark,
                               new Vector3(0.012f * side, 0.020f, 0f), Quaternion.identity, Vector3.one, false);
        SetTwoSided(padGo);

        // Ba cai gai vang: cang ra ngoai cang dai va nga ra sau
        for (int i = 0; i < 3; i++)
        {
            float t = i / 2f;
            var spike = ProcMesh.Taper(0.016f - t * 0.004f, 0.001f, 0.10f + t * 0.09f, 6, false, Color.white);
            ProcMesh.Bend(spike, -14f, Vector3.right);

            rig.Attach("Spike" + i, arm, spike, Mats.Trim,
                       new Vector3((0.045f + t * 0.035f) * side, 0.020f, -0.02f - t * 0.02f),
                       Quaternion.Euler(-26f - t * 10f, 0f, (18f + t * 16f) * side),
                       Vector3.one, true);
        }
    }

    /// <summary>Cai so nho deo o that lung.</summary>
    static void BuildSkullCharm(CharacterRig rig, Transform bone, Vector3 at)
    {
        var skull = ProcMesh.Sphere(0.034f, 12, 9, 1.05f, Color.white);
        ProcMesh.ScaleMesh(skull, new Vector3(0.85f, 1f, 0.9f));
        rig.Attach("CharmSkull", bone, skull, Mats.BoneMat, at, Quaternion.identity, Vector3.one, false);

        // Ham duoi
        rig.Attach("CharmJaw", bone, ProcMesh.Box(new Vector3(0.040f, 0.016f, 0.030f), Color.white),
                   Mats.BoneMat, at + new Vector3(0f, -0.030f, 0.004f), Quaternion.identity, Vector3.one, false);

        // Hai hoc mat den
        var socketMat = Mats.Solid("CharmSocket", new Color(0.05f, 0.04f, 0.05f), 0.1f, 0f);
        var socket = ProcMesh.Sphere(0.011f, 8, 6, 0.8f, Color.white);
        rig.Attach("CharmEyeL", bone, socket, socketMat, at + new Vector3(0.013f, 0.006f, 0.026f),
                   Quaternion.identity, Vector3.one, false);
        rig.Attach("CharmEyeR", bone, socket, socketMat, at + new Vector3(-0.013f, 0.006f, 0.026f),
                   Quaternion.identity, Vector3.one, false);
    }

    // ================================================================
    //  TAY
    // ================================================================

    static void BuildArms(CharacterRig rig)
    {
        var upperArm = ProcMesh.Taper(0.054f, 0.044f, 0.27f, 10, true, Color.white);
        var foreArm = ProcMesh.Taper(0.042f, 0.032f, 0.25f, 10, true, Color.white);

        AttachDown(rig, "UpperArmL", rig.armL, upperArm, Mats.Robe, 0.27f);
        AttachDown(rig, "UpperArmR", rig.armR, upperArm, Mats.Robe, 0.27f);
        AttachDown(rig, "ForeArmL", rig.foreArmL, foreArm, Mats.PaleSkin, 0.25f);
        AttachDown(rig, "ForeArmR", rig.foreArmR, foreArm, Mats.PaleSkin, 0.25f);

        // Tay ao rong xoe ra, gau nhon - dac trung ao phap su
        var sleeve = BuildSleeve(0.062f, 0.135f, 0.34f, 16, 8);
        for (int i = 0; i < 2; i++)
        {
            var bone = i == 0 ? rig.armL : rig.armR;
            var g = rig.Attach("Sleeve" + i, bone, sleeve, Mats.RobeDark,
                               new Vector3(0f, -0.05f, 0f), Quaternion.identity, Vector3.one, false);
            SetTwoSided(g);

            var inner = BuildSleeve(0.052f, 0.118f, 0.31f, 14, 6);
            var gi = rig.Attach("SleeveLining" + i, bone, inner, Mats.RobeLining,
                                new Vector3(0f, -0.055f, 0f), Quaternion.identity, Vector3.one, false);
            SetTwoSided(gi);
        }

        // Ban tay gan guoc
        var hand = ProcMesh.Sphere(0.042f, 12, 8, 1.15f, Color.white);
        rig.Attach("HandL", rig.handL, hand, Mats.PaleSkin, Vector3.zero, Quaternion.identity,
                   new Vector3(0.82f, 1f, 1.15f), true);
        rig.Attach("HandR", rig.handR, hand, Mats.PaleSkin, Vector3.zero, Quaternion.identity,
                   new Vector3(0.82f, 1f, 1.15f), true);
    }

    // ================================================================
    //  DAU
    // ================================================================

    static void BuildHead(CharacterRig rig)
    {
        var neckMesh = ProcMesh.Taper(0.048f, 0.044f, 0.09f, 10, true, Color.white);
        rig.Attach("Neck", rig.neck, neckMesh, Mats.PaleSkin, Vector3.zero);

        // So dau hoi dai va hep - dang nguoi gia gay guoc
        var skull = ProcMesh.Sphere(0.104f, 20, 14, 1.14f, Color.white);
        ProcMesh.ScaleMesh(skull, new Vector3(0.88f, 1.04f, 0.98f));
        rig.Attach("Head", rig.head, skull, Mats.PaleSkin, new Vector3(0f, 0.06f, 0f));

        // Go ma nho cao, mui khoam
        rig.Attach("CheekL", rig.head, ProcMesh.Sphere(0.024f, 8, 6, 0.9f, Color.white), Mats.PaleSkin,
                   new Vector3(0.036f, 0.030f, 0.058f), Quaternion.identity, new Vector3(0.9f, 0.6f, 0.7f), false);
        rig.Attach("CheekR", rig.head, ProcMesh.Sphere(0.024f, 8, 6, 0.9f, Color.white), Mats.PaleSkin,
                   new Vector3(-0.036f, 0.030f, 0.058f), Quaternion.identity, new Vector3(0.9f, 0.6f, 0.7f), false);
        rig.Attach("Nose", rig.head, ProcMesh.Sphere(0.018f, 8, 6, 1.5f, Color.white), Mats.PaleSkin,
                   new Vector3(0f, 0.046f, 0.092f), Quaternion.Euler(24f, 0f, 0f), new Vector3(0.60f, 0.78f, 1.45f), true);

        // Hoc mat sau, mat sang lanh
        var socket = ProcMesh.Sphere(0.027f, 8, 6, 0.7f, Color.white);
        var socketMat = Mats.Solid("EyeSocket", new Color(0.045f, 0.035f, 0.050f), 0.1f, 0f);
        rig.Attach("SocketL", rig.head, socket, socketMat, new Vector3(0.040f, 0.068f, 0.079f),
                   Quaternion.Euler(0f, 0f, -8f), new Vector3(1.15f, 0.78f, 0.6f), false);
        rig.Attach("SocketR", rig.head, socket, socketMat, new Vector3(-0.040f, 0.068f, 0.079f),
                   Quaternion.Euler(0f, 0f, 8f), new Vector3(1.15f, 0.78f, 0.6f), false);

        var eye = ProcMesh.Sphere(0.0115f, 8, 6, 1f, Color.white);
        rig.AttachEye("EyeL", rig.head, eye, Mats.EyeIce, new Vector3(0.039f, 0.066f, 0.090f));
        rig.AttachEye("EyeR", rig.head, eye, Mats.EyeIce, new Vector3(-0.039f, 0.066f, 0.090f));

        // ---- TOC BAC dai buong ra sau ----
        var hair = ProcMesh.Revolve(new Vector2[]
        {
            new Vector2(0.026f,  0.170f),
            new Vector2(0.096f,  0.105f),
            new Vector2(0.112f,  0.020f),
            new Vector2(0.108f, -0.090f),
            new Vector2(0.090f, -0.200f),
            new Vector2(0.062f, -0.300f),
            new Vector2(0.028f, -0.370f),
        }, 18, false, false, Color.white, "hair");
        ProcMesh.ScaleMesh(hair, new Vector3(1f, 1f, 0.88f));
        var hairGo = rig.Attach("Hair", rig.head, hair, Mats.WhiteHair, new Vector3(0f, 0.062f, -0.026f));
        SetTwoSided(hairGo);

        // Hai lon toc vuot cao ra sau nhu hai cai sung
        for (int i = 0; i < 2; i++)
        {
            float side = i == 0 ? 1f : -1f;
            var lock1 = ProcMesh.Taper(0.034f, 0.004f, 0.26f, 7, false, Color.white);
            ProcMesh.Bend(lock1, 26f, Vector3.right);
            rig.Attach("HairSpike" + i, rig.head, lock1, Mats.WhiteHair,
                       new Vector3(0.070f * side, 0.098f, -0.048f),
                       Quaternion.Euler(-74f, 20f * side, 12f * side), Vector3.one, false);
        }

        // ---- RAU BAC dai buong xuong nguc ----
        // Cung 30 -> 150 do: om lay PHIA TRUOC cam (huong +Z ung voi 90 do)
        var beard = ProcMesh.RevolveArc(new Vector2[]
        {
            new Vector2(0.086f,  0.008f),   // om lay ham (so rong 0.079 o day)
            new Vector2(0.072f, -0.030f),   // duoi cam
            new Vector2(0.056f, -0.105f),
            new Vector2(0.040f, -0.195f),
            new Vector2(0.022f, -0.285f),
            new Vector2(0.007f, -0.362f),
        }, 18, 40f, 120f, Color.white, "beard");
        ProcMesh.ScaleMesh(beard, new Vector3(1f, 1f, 1.04f));
        var beardGo = rig.Attach("Beard", rig.head, beard, Mats.WhiteHair,
                                 new Vector3(0f, 0f, 0.004f), Quaternion.identity, Vector3.one, false);
        SetTwoSided(beardGo);

        // Long may bac
        var brow = ProcMesh.Box(new Vector3(0.034f, 0.0075f, 0.011f), Color.white);
        rig.Attach("BrowL", rig.head, brow, Mats.WhiteHair,
                   new Vector3(0.041f, 0.092f, 0.082f), Quaternion.Euler(-14f, 0f, -20f), Vector3.one, false);
        rig.Attach("BrowR", rig.head, brow, Mats.WhiteHair,
                   new Vector3(-0.041f, 0.092f, 0.082f), Quaternion.Euler(-14f, 0f, 20f), Vector3.one, false);
    }

    // ================================================================
    //  CAY GAY PHEP
    // ================================================================

    /// <summary>Cay gay cao, dinh la mot cai vuong mien vang om vien pha le tim.</summary>
    static Transform BuildStaff(CharacterRig rig, Transform anchor)
    {
        var staff = new GameObject("Staff");
        staff.transform.SetParent(anchor, false);
        staff.transform.localPosition = new Vector3(0.05f, 0f, 0.06f);
        staff.transform.localRotation = Quaternion.Euler(0f, 0f, -6f);

        // Than gay den, hoi thon len tren
        var shaft = ProcMesh.Taper(0.024f, 0.016f, 1.78f, 10, true, Color.white);
        ProcMesh.Part("Shaft", staff.transform, shaft, Mats.Bark, new Vector3(0f, -0.86f, 0f));

        // Vong quan vang tren than gay
        var wrap = ProcMesh.Revolve(new Vector2[]
        {
            new Vector2(0.024f, -0.018f),
            new Vector2(0.031f,  0.000f),
            new Vector2(0.024f,  0.018f),
        }, 12, false, false, Color.white, "wrap");
        ProcMesh.Part("Grip1", staff.transform, wrap, Mats.Trim, new Vector3(0f, 0.06f, 0f));
        ProcMesh.Part("Grip2", staff.transform, wrap, Mats.Trim, new Vector3(0f, -0.16f, 0f));
        ProcMesh.Part("Grip3", staff.transform, wrap, Mats.Trim, new Vector3(0f, -0.60f, 0f));

        // De vuong mien
        var crownBase = ProcMesh.Revolve(new Vector2[]
        {
            new Vector2(0.020f, 0.72f),
            new Vector2(0.052f, 0.76f),
            new Vector2(0.044f, 0.81f),
        }, 14, false, false, Color.white, "crownbase");
        ProcMesh.Part("CrownBase", staff.transform, crownBase, Mats.Trim, Vector3.zero);

        // Bon mong vang cong len om lay vien pha le
        var claw = ProcMesh.Taper(0.016f, 0.003f, 0.30f, 7, false, Color.white);
        ProcMesh.Bend(claw, -46f, Vector3.right);
        for (int i = 0; i < 4; i++)
        {
            float ang = i * 90f + 45f;
            var g = ProcMesh.Part("Claw" + i, staff.transform, claw, Mats.Trim,
                                  Vector3.zero,
                                  Quaternion.Euler(0f, ang, 0f) * Quaternion.Euler(-16f, 0f, 0f),
                                  Vector3.one, true);
            g.transform.localPosition = Quaternion.Euler(0f, ang, 0f) * new Vector3(0f, 0.79f, 0.042f);
        }

        // VIEN PHA LE TIM khong lo o giua
        var crystal = ProcMesh.Part("Crystal", staff.transform, BuildCrystal(0.072f, 0.30f), Mats.GemPurple,
                                    new Vector3(0f, 0.92f, 0f), Quaternion.identity, Vector3.one, false);

        // Vai manh pha le nho bay quanh
        for (int i = 0; i < 3; i++)
        {
            float ang = i * 120f + 30f;
            var shard = ProcMesh.Part("Shard" + i, staff.transform, BuildCrystal(0.024f, 0.11f), Mats.GemPurple,
                                      Quaternion.Euler(0f, ang, 0f) * new Vector3(0f, 0.90f, 0.085f),
                                      Quaternion.Euler(0f, ang, 0f) * Quaternion.Euler(0f, 0f, 22f),
                                      Vector3.one, false);
            rig.eyeRenderers.Add(shard.GetComponent<Renderer>());
        }

        // Anh sang tim toa ra tu vien pha le
        var lightGo = new GameObject("OrbLight");
        lightGo.transform.SetParent(crystal.transform, false);
        var lt = lightGo.AddComponent<Light>();
        lt.type = LightType.Point;
        lt.color = new Color(0.72f, 0.42f, 1f);
        lt.intensity = 2.4f;
        lt.range = 7.5f;
        lt.shadows = LightShadows.None;
        var flicker = lightGo.AddComponent<LightFlicker>();
        flicker.baseIntensity = 2.4f;
        flicker.amount = 0.32f;
        flicker.speed = 3.2f;

        // Diem phep bay ra (ngay tren vien pha le)
        var cast = ProcMesh.Joint("CastPoint", staff.transform, new Vector3(0f, 1.12f, 0f));
        rig.eyeRenderers.Add(crystal.GetComponent<Renderer>());
        return cast;
    }

    /// <summary>Mot vien pha le hai dau nhon (hinh thoi dung).</summary>
    static Mesh BuildCrystal(float radius, float height)
    {
        return ProcMesh.Revolve(new Vector2[]
        {
            new Vector2(0.000f, -height * 0.5f),
            new Vector2(radius * 0.62f, -height * 0.26f),
            new Vector2(radius, -height * 0.02f),
            new Vector2(radius * 0.90f, height * 0.20f),
            new Vector2(0.000f, height * 0.5f),
        }, 6, false, false, Color.white, "crystal");
    }

    // ================================================================
    //  CHI TIET
    // ================================================================

    /// <summary>Gan mieng hinh keo XUONG duoi tu khop (tay/chan deu moc xuong).</summary>
    static void AttachDown(CharacterRig rig, string name, Transform bone, Mesh mesh, Material mat, float length)
    {
        rig.Attach(name, bone, mesh, mat, new Vector3(0f, -length, 0f),
                   Quaternion.identity, Vector3.one, true);
    }

    /// <summary>Chiec giay: mui thuon ve truoc, got hoi cao.</summary>
    static Mesh BuildShoe(float width, float length, float height)
    {
        var b = new ProcMesh.Builder();
        b.Append(ProcMesh.Sphere(1f, 14, 10, 1f, Color.white),
                 Matrix4x4.TRS(new Vector3(0f, height * 0.5f, length * 0.10f), Quaternion.identity,
                               new Vector3(width, height * 0.55f, length * 0.55f)), Color.white);
        b.Append(ProcMesh.Sphere(1f, 12, 8, 1f, Color.white),
                 Matrix4x4.TRS(new Vector3(0f, height * 0.42f, length * 0.38f), Quaternion.identity,
                               new Vector3(width * 0.72f, height * 0.38f, length * 0.32f)), Color.white);
        b.Append(ProcMesh.Sphere(1f, 12, 8, 1f, Color.white),
                 Matrix4x4.TRS(new Vector3(0f, height * 1.05f, -length * 0.12f), Quaternion.identity,
                               new Vector3(width * 0.85f, height * 0.75f, width * 0.95f)), Color.white);
        var m = b.Build("shoe");
        m.RecalculateNormals();
        return m;
    }

    /// <summary>Tay ao rong xoe ra, gau cat thanh nhung mui nhon.</summary>
    static Mesh BuildSleeve(float topRadius, float botRadius, float height, int segments, int rings)
    {
        var b = new ProcMesh.Builder();
        int cols = segments + 1;
        const int points = 6;

        for (int r = 0; r < rings; r++)
        {
            float t = (float)r / (rings - 1);

            for (int c = 0; c < cols; c++)
            {
                float u = (float)c / segments;
                float ang = u * Mathf.PI * 2f;

                // Gau cat thanh mui nhon: chieu dai thay doi theo goc
                float tip = Mathf.Abs(Mathf.Cos(ang * points * 0.5f));
                float len = height * Mathf.Lerp(0.72f, 1f, tip);

                float y = -len * t;
                float rad = Mathf.Lerp(topRadius, botRadius, Mathf.Pow(t, 0.8f));

                Vector3 p = new Vector3(Mathf.Cos(ang) * rad, y, Mathf.Sin(ang) * rad);
                Vector3 n = new Vector3(Mathf.Cos(ang), 0.3f, Mathf.Sin(ang)).normalized;
                b.AddVertex(p, n, new Vector2(u * 2f, t * 1.4f), Color.white);
            }
        }

        for (int r = 0; r < rings - 1; r++)
            for (int c = 0; c < segments; c++)
            {
                int i0 = r * cols + c;
                b.AddQuad(i0, i0 + 1, i0 + cols + 1, i0 + cols);
            }

        var m = b.Build("sleeve");
        m.RecalculateNormals();
        return m;
    }

    /// <summary>Ao choang sau lung: mot tam vai cong, rong dan xuong duoi, co gon song.</summary>
    static Mesh BuildCape(float topWidth, float botWidth, float height, int cols, int rows)
    {
        var b = new ProcMesh.Builder();
        int cx = cols + 1;

        for (int r = 0; r <= rows; r++)
        {
            float v = (float)r / rows;
            float w = Mathf.Lerp(topWidth, botWidth, Mathf.Pow(v, 0.85f));

            for (int c = 0; c <= cols; c++)
            {
                float u = (float)c / cols;

                // Gau ao cat thanh mui nhon
                float tip = Mathf.Abs(Mathf.Cos((u - 0.5f) * Mathf.PI * 5f));
                float y = -height * v * Mathf.Lerp(0.80f, 1f, tip);

                float x = (u - 0.5f) * 2f * w;
                float z = -(0.10f + 0.18f * v) * Mathf.Cos((u - 0.5f) * Mathf.PI)
                          + Mathf.Sin(u * Mathf.PI * 3f) * 0.022f * v;
                b.AddVertex(new Vector3(x, y, z), Vector3.back, new Vector2(u, v), Color.white);
            }
        }

        for (int r = 0; r < rows; r++)
            for (int c = 0; c < cols; c++)
            {
                int i0 = r * cx + c;
                b.AddQuad(i0, i0 + 1, i0 + cx + 1, i0 + cx);
            }

        var m = b.Build("cape");
        m.RecalculateNormals();
        return m;
    }

    /// <summary>
    /// Vay ao xoe co nep gap.
    /// <paramref name="tatter"/> lam gau vay rach thanh nhung mui nhon dai ngan khac nhau.
    /// </summary>
    static Mesh BuildSkirt(float topRadius, float botRadius, float height, int segments, int rings, float tatter)
    {
        var b = new ProcMesh.Builder();
        int cols = segments + 1;
        const int folds = 11;                // so nep gap quanh vay
        const int points = 7;                // so mui nhon o gau

        for (int r = 0; r < rings; r++)
        {
            float t = (float)r / (rings - 1);

            for (int c = 0; c < cols; c++)
            {
                float u = (float)c / segments;
                float ang = u * Mathf.PI * 2f;

                // Gau rach: chieu dai vay thay doi theo goc
                float tip = Mathf.Abs(Mathf.Cos(ang * points * 0.5f));
                float len = height * (1f - tatter * (1f - tip));

                float y = -len * t;
                float baseR = Mathf.Lerp(topRadius, botRadius, Mathf.Pow(t, 1.3f));
                float fold = Mathf.Cos(ang * folds) * 0.020f * t;
                float rad = baseR + fold;

                Vector3 p = new Vector3(Mathf.Cos(ang) * rad, y, Mathf.Sin(ang) * rad * 0.92f);
                Vector3 n = new Vector3(Mathf.Cos(ang), 0.35f, Mathf.Sin(ang)).normalized;
                b.AddVertex(p, n, new Vector2(u * 2f, t * 1.6f), Color.white);
            }
        }

        for (int r = 0; r < rings - 1; r++)
            for (int c = 0; c < segments; c++)
            {
                int i0 = r * cols + c;
                b.AddQuad(i0, i0 + 1, i0 + cols + 1, i0 + cols);
            }

        var m = b.Build("skirt");
        m.RecalculateNormals();
        return m;
    }

    /// <summary>Bat ve ca hai mat (ao choang mong nhin tu trong ra van thay).</summary>
    public static void SetTwoSided(GameObject go)
    {
        var r = go.GetComponent<Renderer>();
        if (r == null) return;
        r.sharedMaterial = Mats.TwoSided(r.sharedMaterial);
    }
}
