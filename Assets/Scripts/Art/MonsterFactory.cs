using System.Collections.Generic;
using UnityEngine;

public enum MonsterType
{
    // BA LOAI DAU DA BI BO KHOI GAME. Khong con duong nao sinh ra chung:
    // GameDirector.PickType chi tra ve Skeleton va Witch, con Quy du thi co
    // dong rieng. Hinh dang, prefab va cac ham dung chung van con nguyen -
    // muon bat lai thi them chung vao PickType.
    //
    // KHONG duoc xoa khoi enum: so thu tu duoc luu thang vao mang enemyPrefabs
    // trong canh, bo mot cai o giua la moi con quai con lai doi thanh loai khac.
    Fallen,     // (da bo) quy lun do, cam chuy - dan thuong nhanh nhen
    Skeleton,   // bo xuong cam kiem va khien
    Ghoul,      // (da bo) xac song tay dai, mong vuot
    Brute,      // (da bo) quy khong lo co sung - manh va cham

    // Mu phu thuy trum khan. KHAC HAN bon tren: hinh khong dung bang code ma
    // dung MODEL SAN dua tu Meshy vao, nen MonsterFactory.Build KHONG dung cho
    // loai nay - xem EnemyFactory.LapRapPhuThuy.
    Witch,

    // Quy du co sung. Cung dung MODEL SAN nhu Witch. Danh tu xa bang THIEN
    // THACH goi tu tren troi xuong - xem EnemyFactory.LapRapQuyDu.
    //
    // PHAI dat o CUOI danh sach. So thu tu cua enum nay duoc luu thang vao
    // mang enemyPrefabs trong canh; chen vao giua la moi con quai trong canh
    // doi thanh mot loai khac.
    QuyDu,

    // Quy cay: model san, danh tu xa bang TIA SET MAU XANH LA va chay rat
    // nhanh - xem EnemyFactory.LapRapQuyCay.
    QuyCay
}

/// <summary>
/// DUNG HINH QUAI VAT bang code. Moi loai quai co ti le co the, mau da,
/// sung, vu khi rieng. Hinh (mesh) duoc dung MOT LAN roi dung lai cho
/// moi con cung loai nen sinh hang chuc con van muot.
/// </summary>
public static class MonsterFactory
{
    static readonly Dictionary<string, Mesh> meshCache = new Dictionary<string, Mesh>();
    static Material mGhoulEye, mBruteSkin, mFallenCloth;

    static Mesh Cached(string key, System.Func<Mesh> make)
    {
        Mesh m;
        if (meshCache.TryGetValue(key, out m) && m != null) return m;
        m = make();
        meshCache[key] = m;
        return m;
    }

    // ================================================================
    //  DIEM CHI SO HINH DANG CUA MOT CON QUAI
    // ================================================================
    class Spec
    {
        public float height = 1.8f;
        public float hipY = 0.9f;
        public float spineLen = 0.16f, chestLen = 0.20f, neckLen = 0.12f;
        public float shoulderX = 0.20f, shoulderY = 0.10f;
        public float armUpper = 0.28f, armFore = 0.26f;
        public float legUpper = 0.42f, legLower = 0.40f;
        public float hipX = 0.12f;
        public float limbThick = 1f;
        public float hunch = 10f;          // do gu lung (do)
        public float headRadius = 0.12f;
        public Material body, cloth, horn, eye;
        public float bodyRadius = 0.35f;
    }

    // ================================================================
    //  HAM CHINH
    // ================================================================
    public static CharacterRig Build(MonsterType type, Transform parent)
    {
        Spec s = SpecOf(type);

        var go = new GameObject(type + "_Visual");
        go.transform.SetParent(parent, false);
        var rig = go.AddComponent<CharacterRig>();
        rig.bodyHeight = s.height;
        rig.bodyRadius = s.bodyRadius;

        BuildSkeletonBones(rig, s);
        BuildLimbs(rig, s, type);

        switch (type)
        {
            case MonsterType.Fallen: DecorateFallen(rig, s); break;
            case MonsterType.Skeleton: DecorateSkeleton(rig, s); break;
            case MonsterType.Ghoul: DecorateGhoul(rig, s); break;
            case MonsterType.Brute: DecorateBrute(rig, s); break;
        }

        return rig;
    }

    static Spec SpecOf(MonsterType type)
    {
        switch (type)
        {
            case MonsterType.Fallen:
                return new Spec
                {
                    height = 1.15f, hipY = 0.58f,
                    spineLen = 0.11f, chestLen = 0.14f, neckLen = 0.07f,
                    shoulderX = 0.15f, shoulderY = 0.07f,
                    armUpper = 0.19f, armFore = 0.19f,
                    legUpper = 0.26f, legLower = 0.24f, hipX = 0.09f,
                    limbThick = 1.1f, hunch = 26f, headRadius = 0.115f,
                    bodyRadius = 0.28f,
                    body = Mats.DemonSkin, cloth = FallenCloth(), horn = Mats.DemonHorn, eye = Mats.EyeFire
                };

            case MonsterType.Skeleton:
                return new Spec
                {
                    height = 1.75f, hipY = 0.92f,
                    spineLen = 0.16f, chestLen = 0.20f, neckLen = 0.11f,
                    shoulderX = 0.19f, shoulderY = 0.10f,
                    armUpper = 0.28f, armFore = 0.26f,
                    legUpper = 0.43f, legLower = 0.41f, hipX = 0.11f,
                    limbThick = 0.62f, hunch = 6f, headRadius = 0.105f,
                    bodyRadius = 0.30f,
                    body = Mats.BoneMat, cloth = Mats.Rag, horn = Mats.BoneMat, eye = Mats.EyeRed
                };

            case MonsterType.Ghoul:
                return new Spec
                {
                    height = 1.85f, hipY = 0.92f,
                    spineLen = 0.16f, chestLen = 0.20f, neckLen = 0.12f,
                    shoulderX = 0.21f, shoulderY = 0.09f,
                    armUpper = 0.34f, armFore = 0.33f,
                    legUpper = 0.42f, legLower = 0.40f, hipX = 0.12f,
                    limbThick = 0.85f, hunch = 24f, headRadius = 0.115f,
                    bodyRadius = 0.34f,
                    body = Mats.ZombieFlesh, cloth = Mats.Rag, horn = Mats.BoneMat, eye = GhoulEye()
                };

            default: // Brute
                return new Spec
                {
                    height = 2.45f, hipY = 1.28f,
                    spineLen = 0.22f, chestLen = 0.28f, neckLen = 0.10f,
                    shoulderX = 0.34f, shoulderY = 0.14f,
                    armUpper = 0.40f, armFore = 0.38f,
                    legUpper = 0.56f, legLower = 0.52f, hipX = 0.18f,
                    limbThick = 1.7f, hunch = 16f, headRadius = 0.17f,
                    bodyRadius = 0.55f,
                    body = BruteSkin(), cloth = Mats.Leather, horn = Mats.DemonHorn, eye = Mats.EyeRed
                };
        }
    }

    static Material GhoulEye()
    {
        if (mGhoulEye == null)
            mGhoulEye = Mats.Glow("EyeGhoul", new Color(0.85f, 0.95f, 0.35f), new Color(0.7f, 0.9f, 0.2f), 3.5f);
        return mGhoulEye;
    }

    static Material BruteSkin()
    {
        if (mBruteSkin == null)
            mBruteSkin = Mats.Solid("BruteSkin", new Color(0.34f, 0.10f, 0.09f), 0.30f, 0f,
                                    TextureFactory.Skin(new Color(0.32f, 0.10f, 0.08f), 0.6f, 11), 2f);
        return mBruteSkin;
    }

    static Material FallenCloth()
    {
        if (mFallenCloth == null)
            mFallenCloth = Mats.Solid("FallenCloth", new Color(0.35f, 0.14f, 0.10f), 0.15f, 0f,
                                      TextureFactory.Cloth(new Color(0.22f, 0.08f, 0.06f), new Color(0.45f, 0.20f, 0.12f), 0.8f), 3f);
        return mFallenCloth;
    }

    // ================================================================
    //  XUONG + CHI
    // ================================================================

    static void BuildSkeletonBones(CharacterRig rig, Spec s)
    {
        var root = rig.transform;
        rig.root = rig.Bone("Root", root, Vector3.zero);
        rig.hips = rig.Bone("Hips", rig.root, new Vector3(0f, s.hipY, 0f));
        rig.spine = rig.Bone("Spine", rig.hips, new Vector3(0f, s.spineLen, 0f));
        rig.chest = rig.Bone("Chest", rig.spine, new Vector3(0f, s.chestLen, 0f));
        rig.neck = rig.Bone("Neck", rig.chest, new Vector3(0f, s.neckLen, 0f));
        rig.head = rig.Bone("Head", rig.neck, new Vector3(0f, s.headRadius * 0.55f, 0f));

        rig.armL = rig.Bone("ArmL", rig.chest, new Vector3(s.shoulderX, s.shoulderY, 0f));
        rig.armR = rig.Bone("ArmR", rig.chest, new Vector3(-s.shoulderX, s.shoulderY, 0f));
        rig.foreArmL = rig.Bone("ForeArmL", rig.armL, new Vector3(0f, -s.armUpper, 0f));
        rig.foreArmR = rig.Bone("ForeArmR", rig.armR, new Vector3(0f, -s.armUpper, 0f));
        rig.handL = rig.Bone("HandL", rig.foreArmL, new Vector3(0f, -s.armFore, 0f));
        rig.handR = rig.Bone("HandR", rig.foreArmR, new Vector3(0f, -s.armFore, 0f));

        rig.legL = rig.Bone("LegL", rig.hips, new Vector3(s.hipX, -0.02f, 0f));
        rig.legR = rig.Bone("LegR", rig.hips, new Vector3(-s.hipX, -0.02f, 0f));
        rig.shinL = rig.Bone("ShinL", rig.legL, new Vector3(0f, -s.legUpper, 0f));
        rig.shinR = rig.Bone("ShinR", rig.legR, new Vector3(0f, -s.legUpper, 0f));
        rig.footL = rig.Bone("FootL", rig.shinL, new Vector3(0f, -s.legLower, 0f));
        rig.footR = rig.Bone("FootR", rig.shinR, new Vector3(0f, -s.legLower, 0f));

        rig.weaponAnchor = rig.Bone("WeaponAnchor", rig.handR, Vector3.zero);
        rig.offHandAnchor = rig.Bone("OffHandAnchor", rig.handL, Vector3.zero);
        rig.hitPoint = rig.Bone("HitPoint", rig.chest, new Vector3(0f, 0.05f, 0f));
        rig.castPoint = rig.Bone("CastPoint", rig.chest, new Vector3(0f, 0.10f, 0.25f));

        // Gu lung: nghieng nguoi ve truoc cho dang di doa nat
        rig.spine.localRotation = Quaternion.Euler(s.hunch * 0.45f, 0f, 0f);
        rig.chest.localRotation = Quaternion.Euler(s.hunch * 0.55f, 0f, 0f);
        rig.neck.localRotation = Quaternion.Euler(-s.hunch * 0.7f, 0f, 0f);
    }

    static void BuildLimbs(CharacterRig rig, Spec s, MonsterType type)
    {
        string k = type.ToString();
        float th = s.limbThick;

        var upperArm = Cached(k + "_uarm", () => ProcMesh.Taper(0.062f * th, 0.050f * th, s.armUpper, 10, true, Color.white));
        var foreArm = Cached(k + "_farm", () => ProcMesh.Taper(0.052f * th, 0.042f * th, s.armFore, 10, true, Color.white));
        var thigh = Cached(k + "_thigh", () => ProcMesh.Taper(0.082f * th, 0.062f * th, s.legUpper, 10, true, Color.white));
        var shin = Cached(k + "_shin", () => ProcMesh.Taper(0.062f * th, 0.048f * th, s.legLower, 10, true, Color.white));
        var hand = Cached(k + "_hand", () => ProcMesh.Sphere(0.052f * th, 10, 8, 1.1f, Color.white));
        var foot = Cached(k + "_foot", () => ProcMesh.Taper(0.070f * th, 0.050f * th, 0.12f, 10, true, Color.white));

        Down(rig, "UpperArmL", rig.armL, upperArm, s.body, s.armUpper);
        Down(rig, "UpperArmR", rig.armR, upperArm, s.body, s.armUpper);
        Down(rig, "ForeArmL", rig.foreArmL, foreArm, s.body, s.armFore);
        Down(rig, "ForeArmR", rig.foreArmR, foreArm, s.body, s.armFore);
        Down(rig, "ThighL", rig.legL, thigh, s.body, s.legUpper);
        Down(rig, "ThighR", rig.legR, thigh, s.body, s.legUpper);
        Down(rig, "ShinL", rig.shinL, shin, s.body, s.legLower);
        Down(rig, "ShinR", rig.shinR, shin, s.body, s.legLower);

        rig.Attach("HandL", rig.handL, hand, s.body, Vector3.zero);
        rig.Attach("HandR", rig.handR, hand, s.body, Vector3.zero);

        rig.Attach("FootL", rig.footL, foot, s.body, new Vector3(0f, -0.02f, 0.03f),
                   Quaternion.Euler(88f, 0f, 0f), new Vector3(1f, 1f, 1.5f), true);
        rig.Attach("FootR", rig.footR, foot, s.body, new Vector3(0f, -0.02f, 0.03f),
                   Quaternion.Euler(88f, 0f, 0f), new Vector3(1f, 1f, 1.5f), true);

        // Khop vai / goi tron de che cho noi
        var joint = Cached(k + "_joint", () => ProcMesh.Sphere(0.058f * th, 10, 8, 1f, Color.white));
        rig.Attach("JointArmL", rig.armL, joint, s.body, Vector3.zero);
        rig.Attach("JointArmR", rig.armR, joint, s.body, Vector3.zero);
        rig.Attach("JointKneeL", rig.shinL, joint, s.body, Vector3.zero);
        rig.Attach("JointKneeR", rig.shinR, joint, s.body, Vector3.zero);
    }

    static void Down(CharacterRig rig, string name, Transform bone, Mesh mesh, Material mat, float len)
    {
        rig.Attach(name, bone, mesh, mat, new Vector3(0f, -len, 0f), Quaternion.identity, Vector3.one, true);
    }

    static Mesh TorsoMesh(string key, float waist, float chest, float shoulder, float len, float flatten)
    {
        return Cached(key, () =>
        {
            var m = ProcMesh.Revolve(new Vector2[]
            {
                new Vector2(waist * 0.92f, -len * 0.55f),
                new Vector2(waist,        -len * 0.20f),
                new Vector2(chest,         len * 0.20f),
                new Vector2(shoulder,      len * 0.55f),
                new Vector2(shoulder * 0.55f, len * 0.75f),
            }, 18, true, true, Color.white, "torso");
            ProcMesh.ScaleMesh(m, new Vector3(1f, 1f, flatten));
            return m;
        });
    }

    static Mesh SkullMesh(string key, float r, float jaw)
    {
        return Cached(key, () =>
        {
            var b = new ProcMesh.Builder();
            b.Append(ProcMesh.Sphere(r, 16, 12, 1.05f, Color.white), Matrix4x4.identity, Color.white);
            // Ham duoi nho ra truoc
            b.Append(ProcMesh.Sphere(r * 0.62f, 12, 8, 0.75f, Color.white),
                     Matrix4x4.TRS(new Vector3(0f, -r * 0.55f, r * jaw), Quaternion.identity,
                                   new Vector3(0.85f, 1f, 1.25f)), Color.white);
            var m = b.Build("skull");
            m.RecalculateNormals();
            return m;
        });
    }

    // ================================================================
    //  TRANG TRI TUNG LOAI QUAI
    // ================================================================

    static void DecorateFallen(CharacterRig rig, Spec s)
    {
        rig.Attach("Torso", rig.spine, TorsoMesh("fallen_torso", 0.125f, 0.155f, 0.145f, 0.30f, 0.85f), s.body, Vector3.zero);

        // Bung phe ra mot chut
        rig.Attach("Belly", rig.spine, Cached("fallen_belly", () => ProcMesh.Sphere(0.115f, 14, 10, 0.85f, Color.white)),
                   s.body, new Vector3(0f, -0.03f, 0.035f));

        // Khan quan hong
        var loin = Cached("fallen_loin", () => ProcMesh.Revolve(new Vector2[]
        {
            new Vector2(0.135f, 0.00f),
            new Vector2(0.150f, -0.10f),
            new Vector2(0.140f, -0.20f),
        }, 14, false, false, Color.white, "loin"));
        var loinGo = rig.Attach("Loin", rig.hips, loin, s.cloth, new Vector3(0f, 0.02f, 0f));
        WizardFactory.SetTwoSided(loinGo);

        // Dau to, mom nhon
        rig.Attach("Head", rig.head, SkullMesh("fallen_head", s.headRadius, 0.55f), s.body, Vector3.zero);

        // Tai doi nhon hai ben
        var ear = Cached("fallen_ear", () =>
        {
            var m = ProcMesh.Taper(0.030f, 0.004f, 0.14f, 6, false, Color.white);
            return ProcMesh.Bend(m, 25f, Vector3.forward);
        });
        rig.Attach("EarL", rig.head, ear, s.body, new Vector3(0.10f, 0.02f, -0.01f),
                   Quaternion.Euler(0f, 0f, -68f), Vector3.one, true);
        rig.Attach("EarR", rig.head, ear, s.body, new Vector3(-0.10f, 0.02f, -0.01f),
                   Quaternion.Euler(0f, 0f, 68f), Vector3.one, true);

        // Hai cai sung nho cong ve sau
        var horn = Cached("fallen_horn", () =>
        {
            var m = ProcMesh.Taper(0.024f, 0.003f, 0.13f, 7, false, Color.white);
            return ProcMesh.Bend(m, -60f, Vector3.right);
        });
        rig.Attach("HornL", rig.head, horn, s.horn, new Vector3(0.055f, 0.085f, -0.01f),
                   Quaternion.Euler(-18f, 0f, -14f), Vector3.one, true);
        rig.Attach("HornR", rig.head, horn, s.horn, new Vector3(-0.055f, 0.085f, -0.01f),
                   Quaternion.Euler(-18f, 0f, 14f), Vector3.one, true);

        // Mat vang ruc
        var eye = Cached("eye_s", () => ProcMesh.Sphere(0.019f, 8, 6, 1f, Color.white));
        rig.AttachEye("EyeL", rig.head, eye, s.eye, new Vector3(0.045f, 0.025f, 0.093f));
        rig.AttachEye("EyeR", rig.head, eye, s.eye, new Vector3(-0.045f, 0.025f, 0.093f));

        // Rang nanh
        var fang = Cached("fang", () => ProcMesh.Taper(0.010f, 0.001f, 0.035f, 5, false, Color.white));
        rig.Attach("FangL", rig.head, fang, Mats.BoneMat, new Vector3(0.030f, -0.055f, 0.085f),
                   Quaternion.Euler(180f, 0f, 0f), Vector3.one, false);
        rig.Attach("FangR", rig.head, fang, Mats.BoneMat, new Vector3(-0.030f, -0.055f, 0.085f),
                   Quaternion.Euler(180f, 0f, 0f), Vector3.one, false);

        // Vu khi: cai chuy go co dinh da
        BuildClub(rig.weaponAnchor);
    }

    static void DecorateSkeleton(CharacterRig rig, Spec s)
    {
        // Long nguc: cac dot suon rieng le
        var rib = Cached("skel_rib", () => RibMesh(0.135f, 0.016f));
        for (int i = 0; i < 5; i++)
        {
            float t = i / 4f;
            float y = Mathf.Lerp(0.12f, -0.10f, t);
            float sc = Mathf.Lerp(1.0f, 0.72f, t);
            rig.Attach("Rib" + i, rig.chest, rib, s.body, new Vector3(0f, y, 0f),
                       Quaternion.identity, new Vector3(sc, 1f, sc * 0.8f), true);
        }

        // Xuong song va xuong chau
        rig.Attach("Spine", rig.spine, Cached("skel_spine", () => ProcMesh.Taper(0.032f, 0.026f, 0.36f, 8, true, Color.white)),
                   s.body, new Vector3(0f, -0.06f, -0.02f));
        rig.Attach("Pelvis", rig.hips, Cached("skel_pelvis", () => ProcMesh.Revolve(new Vector2[]
        {
            new Vector2(0.070f, -0.09f),
            new Vector2(0.125f, -0.02f),
            new Vector2(0.115f,  0.05f),
            new Vector2(0.070f,  0.09f),
        }, 14, false, false, Color.white, "pelvis")), s.body, Vector3.zero);

        // Vai co mieng vai rach quang qua
        var cape = Cached("skel_cape", () => ProcMesh.RevolveArc(new Vector2[]
        {
            new Vector2(0.09f,  0.16f),
            new Vector2(0.15f,  0.06f),
            new Vector2(0.17f, -0.08f),
            new Vector2(0.15f, -0.22f),
        }, 14, 200f, 140f, Color.white, "cape"));
        var capeGo = rig.Attach("Cape", rig.chest, cape, s.cloth, new Vector3(0f, 0.06f, -0.02f));
        WizardFactory.SetTwoSided(capeGo);

        // So dau + ho mat sang do
        rig.Attach("Skull", rig.head, SkullMesh("skel_skull", s.headRadius, 0.45f), s.body, Vector3.zero);
        var socket = Cached("socket", () => ProcMesh.Sphere(0.030f, 8, 6, 1f, Color.white));
        rig.Attach("SocketL", rig.head, socket, Mats.Solid("Dark", new Color(0.05f, 0.04f, 0.04f), 0.1f, 0f),
                   new Vector3(0.043f, 0.020f, 0.082f), Quaternion.identity, new Vector3(1f, 0.8f, 0.6f), false);
        rig.Attach("SocketR", rig.head, socket, Mats.Solid("Dark", new Color(0.05f, 0.04f, 0.04f), 0.1f, 0f),
                   new Vector3(-0.043f, 0.020f, 0.082f), Quaternion.identity, new Vector3(1f, 0.8f, 0.6f), false);

        var eye = Cached("eye_xs", () => ProcMesh.Sphere(0.014f, 8, 6, 1f, Color.white));
        rig.AttachEye("EyeL", rig.head, eye, s.eye, new Vector3(0.043f, 0.020f, 0.094f));
        rig.AttachEye("EyeR", rig.head, eye, s.eye, new Vector3(-0.043f, 0.020f, 0.094f));

        BuildSword(rig.weaponAnchor);
        BuildShield(rig.offHandAnchor);
    }

    static void DecorateGhoul(CharacterRig rig, Spec s)
    {
        rig.Attach("Torso", rig.spine, TorsoMesh("ghoul_torso", 0.135f, 0.185f, 0.180f, 0.40f, 0.72f), s.body, Vector3.zero);

        // Suon lo ra duoi lop da
        var rib = Cached("ghoul_rib", () => RibMesh(0.150f, 0.012f));
        for (int i = 0; i < 3; i++)
            rig.Attach("Rib" + i, rig.chest, rib, Mats.BoneMat, new Vector3(0f, 0.02f - i * 0.055f, 0.01f),
                       Quaternion.identity, new Vector3(0.92f - i * 0.05f, 1f, 0.66f), true);

        // Gie rach quan quanh hong
        var rags = Cached("ghoul_rags", () => ProcMesh.Revolve(new Vector2[]
        {
            new Vector2(0.150f,  0.04f),
            new Vector2(0.165f, -0.08f),
            new Vector2(0.150f, -0.22f),
            new Vector2(0.120f, -0.32f),
        }, 16, false, false, Color.white, "rags"));
        var ragGo = rig.Attach("Rags", rig.hips, rags, s.cloth, Vector3.zero);
        WizardFactory.SetTwoSided(ragGo);

        // Dau cui, ham dai
        rig.Attach("Head", rig.head, SkullMesh("ghoul_head", s.headRadius, 0.62f), s.body, Vector3.zero);
        var eye = Cached("eye_s", () => ProcMesh.Sphere(0.019f, 8, 6, 1f, Color.white));
        rig.AttachEye("EyeL", rig.head, eye, s.eye, new Vector3(0.042f, 0.030f, 0.092f));
        rig.AttachEye("EyeR", rig.head, eye, s.eye, new Vector3(-0.042f, 0.030f, 0.092f));

        // Mong vuot dai o hai ban tay
        var claw = Cached("ghoul_claw", () =>
        {
            var m = ProcMesh.Taper(0.012f, 0.001f, 0.115f, 6, false, Color.white);
            return ProcMesh.Bend(m, 45f, Vector3.right);
        });
        for (int i = 0; i < 3; i++)
        {
            float x = (i - 1) * 0.028f;
            rig.Attach("ClawL" + i, rig.handL, claw, Mats.BoneMat, new Vector3(x, -0.03f, 0.02f),
                       Quaternion.Euler(150f, 0f, 0f), Vector3.one, false);
            rig.Attach("ClawR" + i, rig.handR, claw, Mats.BoneMat, new Vector3(x, -0.03f, 0.02f),
                       Quaternion.Euler(150f, 0f, 0f), Vector3.one, false);
        }
    }

    static void DecorateBrute(CharacterRig rig, Spec s)
    {
        rig.Attach("Torso", rig.spine, TorsoMesh("brute_torso", 0.240f, 0.340f, 0.330f, 0.55f, 0.80f), s.body, Vector3.zero);

        // Co bap nguc
        var pec = Cached("brute_pec", () => ProcMesh.Sphere(0.135f, 12, 8, 0.8f, Color.white));
        rig.Attach("PecL", rig.chest, pec, s.body, new Vector3(0.13f, 0.02f, 0.14f));
        rig.Attach("PecR", rig.chest, pec, s.body, new Vector3(-0.13f, 0.02f, 0.14f));

        // Gai xuong nho tren vai
        var spike = Cached("brute_spike", () => ProcMesh.Taper(0.055f, 0.004f, 0.24f, 7, false, Color.white));
        for (int i = 0; i < 3; i++)
        {
            float a = -25f + i * 25f;
            rig.Attach("SpikeL" + i, rig.armL, spike, s.horn, new Vector3(0.02f, 0.04f, 0f),
                       Quaternion.Euler(a, 0f, -20f - i * 8f), Vector3.one, true);
            rig.Attach("SpikeR" + i, rig.armR, spike, s.horn, new Vector3(-0.02f, 0.04f, 0f),
                       Quaternion.Euler(a, 0f, 20f + i * 8f), Vector3.one, true);
        }

        // Vay da quanh hong
        var kilt = Cached("brute_kilt", () => ProcMesh.Revolve(new Vector2[]
        {
            new Vector2(0.26f,  0.05f),
            new Vector2(0.30f, -0.12f),
            new Vector2(0.28f, -0.34f),
        }, 18, false, false, Color.white, "kilt"));
        var kiltGo = rig.Attach("Kilt", rig.hips, kilt, s.cloth, Vector3.zero);
        WizardFactory.SetTwoSided(kiltGo);

        // Dau quy: mom banh, sung to cong ra hai ben
        rig.Attach("Head", rig.head, SkullMesh("brute_head", s.headRadius, 0.55f), s.body, Vector3.zero);

        var horn = Cached("brute_horn", () =>
        {
            var m = ProcMesh.Taper(0.052f, 0.006f, 0.34f, 8, false, Color.white);
            return ProcMesh.Bend(m, 55f, Vector3.right);
        });
        rig.Attach("HornL", rig.head, horn, s.horn, new Vector3(0.10f, 0.10f, 0f),
                   Quaternion.Euler(10f, 0f, -58f), Vector3.one, true);
        rig.Attach("HornR", rig.head, horn, s.horn, new Vector3(-0.10f, 0.10f, 0f),
                   Quaternion.Euler(10f, 0f, 58f), Vector3.one, true);

        var eye = Cached("eye_m", () => ProcMesh.Sphere(0.026f, 8, 6, 1f, Color.white));
        rig.AttachEye("EyeL", rig.head, eye, s.eye, new Vector3(0.062f, 0.030f, 0.135f));
        rig.AttachEye("EyeR", rig.head, eye, s.eye, new Vector3(-0.062f, 0.030f, 0.135f));

        // Rang nanh lon
        var fang = Cached("brute_fang", () => ProcMesh.Taper(0.018f, 0.002f, 0.070f, 6, false, Color.white));
        rig.Attach("FangL", rig.head, fang, Mats.BoneMat, new Vector3(0.048f, -0.075f, 0.125f),
                   Quaternion.Euler(170f, 0f, 0f), Vector3.one, false);
        rig.Attach("FangR", rig.head, fang, Mats.BoneMat, new Vector3(-0.048f, -0.075f, 0.125f),
                   Quaternion.Euler(170f, 0f, 0f), Vector3.one, false);

        // Nam dam to nhu bua
        var fist = Cached("brute_fist", () => ProcMesh.Rock(0.13f, 21, 0.18f, 12, Color.white));
        rig.Attach("FistL", rig.handL, fist, s.body, Vector3.zero);
        rig.Attach("FistR", rig.handR, fist, s.body, Vector3.zero);
    }

    // ================================================================
    //  VU KHI
    // ================================================================

    static Mesh RibMesh(float radius, float thickness)
    {
        // Vong suon: khong khep kin phia truoc
        var b = new ProcMesh.Builder();
        int seg = 14, ring = 6;
        float sweep = 300f;

        for (int i = 0; i <= seg; i++)
        {
            float a = (-sweep * 0.5f + sweep * i / seg + 90f) * Mathf.Deg2Rad;
            Vector3 center = new Vector3(Mathf.Cos(a) * radius, 0f, Mathf.Sin(a) * radius);
            Vector3 outward = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a));
            for (int j = 0; j <= ring; j++)
            {
                float ra = (float)j / ring * Mathf.PI * 2f;
                Vector3 p = center + outward * (Mathf.Cos(ra) * thickness) + Vector3.up * (Mathf.Sin(ra) * thickness);
                Vector3 n = (outward * Mathf.Cos(ra) + Vector3.up * Mathf.Sin(ra)).normalized;
                b.AddVertex(p, n, new Vector2((float)i / seg, (float)j / ring), Color.white);
            }
        }

        int cols = ring + 1;
        for (int i = 0; i < seg; i++)
            for (int j = 0; j < ring; j++)
            {
                int i0 = i * cols + j;
                b.AddQuad(i0, i0 + cols, i0 + cols + 1, i0 + 1);
            }

        return b.Build("rib");
    }

    static void BuildClub(Transform anchor)
    {
        if (anchor == null) return;
        var club = new GameObject("Club");
        club.transform.SetParent(anchor, false);
        club.transform.localRotation = Quaternion.Euler(-70f, 0f, 0f);

        ProcMesh.Part("Handle", club.transform,
                      Cached("club_handle", () => ProcMesh.Taper(0.022f, 0.030f, 0.46f, 8, true, Color.white)),
                      Mats.Wood, new Vector3(0f, -0.06f, 0f));
        ProcMesh.Part("Head", club.transform,
                      Cached("club_head", () => ProcMesh.Rock(0.075f, 5, 0.25f, 10, Color.white)),
                      Mats.Rock, new Vector3(0f, 0.38f, 0f));
        for (int i = 0; i < 3; i++)
            ProcMesh.Part("Spike" + i, club.transform,
                          Cached("club_spike", () => ProcMesh.Taper(0.014f, 0.002f, 0.07f, 5, false, Color.white)),
                          Mats.BoneMat, new Vector3(0f, 0.38f, 0f),
                          Quaternion.Euler(0f, i * 120f, 70f), Vector3.one, false);
    }

    static void BuildSword(Transform anchor)
    {
        if (anchor == null) return;
        var sword = new GameObject("Sword");
        sword.transform.SetParent(anchor, false);
        sword.transform.localRotation = Quaternion.Euler(-75f, 0f, 0f);

        var blade = Cached("sword_blade", () =>
        {
            var b = new ProcMesh.Builder();
            // Luoi kiem det, vat nhon o dau
            ProcMesh.AppendBox(b, Matrix4x4.TRS(new Vector3(0f, 0.34f, 0f), Quaternion.identity, Vector3.one),
                               new Vector3(0.075f, 0.62f, 0.017f), Color.white);
            b.Append(ProcMesh.Taper(0.037f, 0.002f, 0.11f, 4, false, Color.white),
                     Matrix4x4.TRS(new Vector3(0f, 0.65f, 0f), Quaternion.identity, new Vector3(1f, 1f, 0.45f)),
                     Color.white);
            var m = b.Build("blade");
            m.RecalculateNormals();
            return m;
        });
        ProcMesh.Part("Blade", sword.transform, blade, Mats.Steel, Vector3.zero);
        ProcMesh.Part("Guard", sword.transform, Cached("sword_guard", () => ProcMesh.Box(new Vector3(0.20f, 0.028f, 0.035f), Color.white)),
                      Mats.Steel, new Vector3(0f, 0.02f, 0f));
        ProcMesh.Part("Grip", sword.transform, Cached("sword_grip", () => ProcMesh.Taper(0.020f, 0.018f, 0.14f, 8, true, Color.white)),
                      Mats.Leather, new Vector3(0f, -0.15f, 0f));
        ProcMesh.Part("Pommel", sword.transform, Cached("sword_pommel", () => ProcMesh.Sphere(0.028f, 10, 8, 0.8f, Color.white)),
                      Mats.Steel, new Vector3(0f, -0.16f, 0f));
    }

    static void BuildShield(Transform anchor)
    {
        if (anchor == null) return;
        var shield = new GameObject("Shield");
        shield.transform.SetParent(anchor, false);
        shield.transform.localPosition = new Vector3(0.06f, -0.02f, 0.10f);
        shield.transform.localRotation = Quaternion.Euler(0f, -22f, 12f);
        shield.transform.localScale = Vector3.one * 0.85f;

        var disc = Cached("shield_disc", () =>
        {
            var m = ProcMesh.Revolve(new Vector2[]
            {
                new Vector2(0.000f, -0.030f),
                new Vector2(0.120f, -0.018f),
                new Vector2(0.190f,  0.000f),
                new Vector2(0.196f,  0.022f),
                new Vector2(0.150f,  0.030f),
                new Vector2(0.000f,  0.036f),
            }, 18, false, false, Color.white, "shield");
            return m;
        });
        ProcMesh.Part("Plate", shield.transform, disc, Mats.Wood,
                      Vector3.zero, Quaternion.Euler(90f, 0f, 0f), Vector3.one, true);
        ProcMesh.Part("Boss", shield.transform, Cached("shield_boss", () => ProcMesh.Sphere(0.055f, 12, 8, 0.6f, Color.white)),
                      Mats.Steel, new Vector3(0f, 0f, -0.035f), Quaternion.Euler(90f, 0f, 0f), Vector3.one, true);
    }
}
