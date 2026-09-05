using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// TU KIEM TRA: dung thu tat ca nhan vat, quai vat, hieu ung lua/bang va shader
/// de chac chan khong co gi bi loi truoc khi ban bam Play.
/// Vao menu "Diablo 2.5D > 3. Tu kiem tra".
/// </summary>
public static class SelfTest
{
    static int errors;

    [MenuItem("Diablo 2.5D/3. Tu kiem tra (Self Test)", false, 20)]
    public static void Run()
    {
        errors = 0;
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        CheckShaders();
        CheckArt();
        CheckVfx();
        CheckGameplay();

        if (errors == 0) Debug.Log("[SelfTest] TAT CA DEU TOT - khong phat hien loi nao.");
        else Debug.LogError("[SelfTest] Phat hien " + errors + " van de.");

        if (!Application.isBatchMode)
            EditorUtility.DisplayDialog("Tu kiem tra",
                errors == 0 ? "Tat ca deu tot!" : "Phat hien " + errors + " van de - xem cua so Console.",
                "OK");

        if (Application.isBatchMode && errors > 0)
            EditorApplication.Exit(1);
    }

    static void Fail(string msg)
    {
        errors++;
        Debug.LogError("[SelfTest] " + msg);
    }

    // ---------------- Shader ----------------
    static void CheckShaders()
    {
        string[] names =
        {
            "Diablo25D/Fire", "Diablo25D/Ice", "Diablo25D/FrozenShell",
            "Diablo25D/ParticleAdditive", "Diablo25D/ParticleAlpha",
            "Diablo25D/SkyGradient", "Diablo25D/Bloom", "Diablo25D/ClothTwoSided",
            "Diablo25D/Terrain", "Diablo25D/Foliage"
        };

        for (int i = 0; i < names.Length; i++)
        {
            var sh = Shader.Find(names[i]);
            if (sh == null) { Fail("Khong tim thay shader: " + names[i]); continue; }
            if (ShaderUtil.ShaderHasError(sh)) Fail("Shader bi loi: " + names[i]);
            if (!sh.isSupported) Fail("May nay khong chay duoc shader: " + names[i]);
        }
    }

    // ---------------- Hinh nhan vat ----------------
    static void CheckArt()
    {
        try
        {
            var holder = new GameObject("TEST_Art");

            var wiz = WizardFactory.Build(holder.transform);
            if (wiz == null || wiz.castPoint == null || wiz.head == null)
                Fail("Phu thuy thieu khop xuong.");

            var types = new[] { MonsterType.Fallen, MonsterType.Skeleton, MonsterType.Ghoul, MonsterType.Brute };
            for (int i = 0; i < types.Length; i++)
            {
                var rig = MonsterFactory.Build(types[i], holder.transform);
                if (rig == null || rig.head == null || rig.handR == null)
                    Fail("Quai " + types[i] + " thieu khop xuong.");

                int meshes = rig.GetComponentsInChildren<MeshFilter>().Length;
                if (meshes < 8) Fail("Quai " + types[i] + " chi co " + meshes + " mieng hinh - qua it.");
            }

            var world = new GameObject("TEST_World");
            WorldFactory.Build(world.transform, 20f, 7);

            // Tham co phai thuc su moc ra chu khong duoc rong
            var field = world.transform.Find("ThamCo");
            if (field == null || field.childCount == 0)
                Fail("Khong dung duoc tham co.");
            else
            {
                int tufts = 0;
                var filters = field.GetComponentsInChildren<MeshFilter>();
                for (int i = 0; i < filters.Length; i++)
                    if (filters[i].sharedMesh != null) tufts += filters[i].sharedMesh.vertexCount;
                if (tufts < 2000) Fail("Tham co qua thua: chi co " + tufts + " dinh.");
            }

            var bush = WorldFactory.BuildBush(world.transform, Vector3.zero, 1);
            if (bush.GetComponentInChildren<MeshFilter>() == null) Fail("Bui ram khong co hinh.");

            var leafy = WorldFactory.BuildLeafyTree(world.transform, Vector3.zero, 1);
            if (leafy.GetComponentsInChildren<MeshFilter>().Length < 3) Fail("Cay xanh thieu tan la.");

            Object.DestroyImmediate(holder);
            Object.DestroyImmediate(world);
        }
        catch (System.Exception e)
        {
            Fail("Loi khi dung hinh: " + e);
        }
    }

    // ---------------- Hieu ung ----------------
    static void CheckVfx()
    {
        try
        {
            var holder = new GameObject("TEST_Vfx");

            VfxFactory.BuildFireballVisual(holder.transform, 0.3f);
            VfxFactory.FireExplosion(Vector3.zero, 3f);
            VfxFactory.IceImpact(new Vector3(3f, 0f, 0f), 2f);
            VfxFactory.IceStormField(new Vector3(6f, 0f, 0f), 5f, 4f);
            VfxFactory.IceShardFalling(new Vector3(6f, 0f, 0f), 10f, 0.4f);
            VfxFactory.FrozenShatter(new Vector3(9f, 0f, 0f), 1f);
            VfxFactory.AttachBurning(holder.transform, 1.8f, 0.4f);
            VfxFactory.Brazier(new Vector3(-4f, 0f, 0f), holder.transform, 1f);
            VfxFactory.CastCharge(holder.transform, 0, 0.5f);
            VfxFactory.CastCharge(holder.transform, 1, 0.5f);
            VfxFactory.CastCharge(holder.transform, 2, 0.5f);

            // Sam set
            VfxFactory.LightningStormField(new Vector3(12f, 0f, 0f), 6f, 2.4f);
            VfxFactory.LightningWarning(new Vector3(12f, 0f, 0f), 2.1f, 0.1f);
            VfxFactory.LightningImpact(new Vector3(12f, 0f, 0f), 2.1f);
            VfxFactory.AttachStunned(holder.transform, 1.8f, 0.4f);
            LightningArc.Create(new Vector3(12f, 14f, 0f), new Vector3(12f, 0f, 0f), 1f, 0.26f);
            LightningStrike.Spawn(new Vector3(12f, 0f, 0f), 18f, 0.05f);

            // Loc xoay
            var funnel = VfxFactory.BuildTornado(1f);
            if (funnel.GetComponentsInChildren<MeshFilter>().Length < 3)
                Fail("Con loc thieu lop vo.");
            if (funnel.GetComponentsInChildren<ParticleSystem>().Length < 3)
                Fail("Con loc thieu bui / manh vun.");
            VfxFactory.TornadoBolt(new Vector3(16f, 0f, 0f), 1f);
            VfxFactory.HitBurst(Vector3.zero, Color.red);
            DamagePopup.Spawn(Vector3.up, 42, DamageType.Fire, false);

            // Kiem tra vat lieu hat khong bi thieu shader
            var systems = Object.FindObjectsByType<ParticleSystem>(FindObjectsInactive.Include);
            for (int i = 0; i < systems.Length; i++)
            {
                var r = systems[i].GetComponent<ParticleSystemRenderer>();
                if (r == null || r.sharedMaterial == null || r.sharedMaterial.shader == null)
                    Fail("He hat thieu vat lieu: " + systems[i].name);
            }
        }
        catch (System.Exception e)
        {
            Fail("Loi khi tao hieu ung: " + e);
        }
    }

    // ---------------- Gameplay ----------------
    static void CheckGameplay()
    {
        try
        {
            if (LayerMask.NameToLayer("Enemy") < 0) Fail("Chua co layer 'Enemy'.");
            if (LayerMask.NameToLayer("Ground") < 0) Fail("Chua co layer 'Ground'.");

            var enemy = EnemyFactory.Spawn(MonsterType.Skeleton, new Vector3(2f, 0f, 2f), null, null);
            var dmg = enemy.GetComponent<Damageable>();
            if (dmg == null) { Fail("Quai khong co thanh mau."); return; }

            float before = dmg.health;
            dmg.TakeDamage(15f, DamageType.Fire, enemy.transform.position);
            if (dmg.health >= before) Fail("Nhan sat thuong khong tru mau.");

            BurningEffect.Apply(dmg, 5f, 2f);
            FrozenEffect.Apply(dmg, 2f);
            StunnedEffect.Apply(dmg, 2f);
            if (enemy.GetComponent<BurningEffect>() == null) Fail("Khong gan duoc trang thai bong chay.");
            if (enemy.GetComponent<StunnedEffect>() == null) Fail("Khong gan duoc trang thai choang.");

            float beforeShock = dmg.health;
            dmg.TakeDamage(12f, DamageType.Lightning, enemy.transform.position);
            if (dmg.health >= beforeShock) Fail("Sat thuong set khong tru mau.");

            Physics.SyncTransforms();
            CombatUtil.AreaShock(enemy.transform.position, 3f, 10f, LayerMask.GetMask("Enemy"), 1f, 2f);

            Fireball.Spawn(new Vector3(0f, 1.4f, 0f), Vector3.forward,
                           LayerMask.GetMask("Default"), LayerMask.GetMask("Enemy"));
            IceStorm.Spawn(new Vector3(4f, 0f, 4f), LayerMask.GetMask("Enemy"));
            LightningStorm.Spawn(new Vector3(-4f, 0f, 4f), LayerMask.GetMask("Enemy"));

            var twister = Tornado.Spawn(new Vector3(6f, 0f, -4f), Vector3.forward, LayerMask.GetMask("Enemy"));
            if (twister == null) Fail("Khong tao duoc con loc.");
            if (WhirledEffect.Catch(dmg, twister) == null) Fail("Khong cuon duoc quai vao loc.");
            if (enemy.GetComponent<WhirledEffect>() == null) Fail("Khong gan duoc trang thai bi cuon.");

            var scenes = EditorBuildSettings.scenes;
            if (scenes == null || scenes.Length == 0)
                Debug.LogWarning("[SelfTest] Chua co scene nao trong Build Settings.");
        }
        catch (System.Exception e)
        {
            Fail("Loi khi thu danh nhau: " + e);
        }
    }
}
