using LibDescent.Data;
using System;
using System.Text;

namespace LibDescent.Edit
{
    public class BitmapTableFile
    {
        public static string[] robotsbm = {"mech", "green", "spider", "josh", "violet",
                    "clkvulc", "clkmech", "brain", "onearm", "plasguy",
                    "toaster", "bird", "mislbird", "splitpod", "smspider",
                    "miniboss", "suprmech", "boss1", "cloakgrn", "vulcnguy", "rifleman",
                    "fourclaw", "quadlasr", "boss2", "babyplas1", "sloth", "icespidr",
                    "gaussguy", "fire", "spread", "sidearm", "xboss1", "newboss1",
                    "escort", "guard", "eviltwin", "sniper", "snipe", "frog", "minotaur",
                    "fourclaw2", "hornet", "bandit", "arnold", "sucker", "xboss3", "xboss2",
                    "boarshed", "spiderg", "omega", "smside", "toady", "xboss5",
                    "popcorn", "clkclaw", "clksloth", "guppy", "sloth2",
                    "omega2", "babyplas2", "spiderg2", "spawn", "xboss4", "spawn2",
                    "xboss6", "minireac"};

        public static string[] powerupsbm = {"Life", "Energy", "Shield", "Laser", "BlueKey", "RedKey", "YelKey",
                      "R_Pill", "P_Pill", "M_Pill", "Cmiss_1", "Cmiss_4", "QudLas",
                      "Vulcan", "Sprdfr", "Plasma", "Fusion", "Proxim", "Hmiss_1", "Hmiss_4",
                      "Smiss", "Mmiss", "V_Ammo", "Cloak", "Turbo", "Invuln",
                      "Headli", "Megwow", "Gauss", "Helix", "Phoenix", "Omega",
                      "SLaser", "Allmap", "Conv", "Ammork", "Burner", "Hlight", "Scmiss1",
                      "Scmiss4", "Shmiss1", "Shmiss4", "Sproxi", "Merc1",
                      "Merc4", "Eshkr", "BlueFlg", "RedFlag"};

        private static string[] AIBehaviors = { "STILL", "NORMAL", "BEHIND", "RUN_FROM", "SNIPE", "STATION", "FOLLOW" };

        public static string[] pofNames = { "robot09.pof", "robot09s.pof", "robot17.pof", "robot17s.pof", "robot22.pof", "robot22s.pof",
            "robot01.pof", "robot01s.pof", "robot23.pof", "robot23s.pof", "robot37.pof", "robot37s.pof","robot09.pof", "robot09s.pof",
            "robot26.pof", "robot27.pof", "robot27s.pof", "robot42.pof", "robot42s.pof", "robot08.pof", "robot16.pof", "robot16.pof",
            "robot31.pof", "robot32.pof", "robot32s.pof", "robot43.pof", "robot09.pof", "robot09s.pof", "boss01.pof", "robot35.pof",
            "robot35s.pof", "robot37.pof", "robot37s.pof", "robot38.pof", "robot38s.pof", "robot39.pof", "robot39s.pof", "robot40.pof",
            "robot40s.pof", "boss02.pof", "robot36.pof", "robot41.pof", "robot41s.pof", "robot44.pof", "robot45.pof", "robot46.pof",
            "robot47.pof", "robot48.pof", "robot48s.pof", "robot49.pof", "boss01.pof", "robot50.pof", "robot42.pof", "robot42s.pof",
            "robot50.pof", "robot51.pof", "robot53.pof", "robot53s.pof", "robot54.pof", "robot54s.pof", "robot56.pof", "robot56s.pof",
            "robot58.pof", "robot58s.pof", "robot57a.pof", "robot55.pof", "robot55s.pof", "robot59.pof", "robot60.pof", "robot52.pof",
            "robot61.pof", "robot62.pof", "robot63.pof", "robot64.pof", "robot65.pof", "robot66.pof", "boss5.pof", "robot49a.pof",
            "robot58.pof", "robot58.pof", "robot41.pof", "robot41.pof", "robot64.pof", "robot41.pof", "robot41s.pof", "robot64.pof",
            "robot36.pof", "robot63.pof", "robot57.pof", "Boss4.pof", "robot57.pof", "Boss06.pof", "reacbot.pof", "reactor.pof",
            "reactor2.pof", "reactor8.pof", "reactor9.pof", "newreac1.pof", "newreac2.pof", "newreac5.pof", "newreac6.pof", "newreac7.pof",
            "newreac8.pof", "newreac3.pof", "newreac4.pof", "newreac9.pof", "newreac0.pof", "marker.pof", "pship1.pof", "pship1s.pof",
            "pship1b.pof", "laser1-1.pof", "laser11s.pof", "laser12s.pof", "laser1-2.pof", "laser2-1.pof", "laser21s.pof", "laser22s.pof",
            "laser2-2.pof", "laser3-1.pof", "laser31s.pof", "laser32s.pof", "laser3-2.pof", "laser4-1.pof", "laser41s.pof", "laser42s.pof",
            "laser4-2.pof", "cmissile.pof", "flare.pof", "laser3-1.pof", "laser3-2.pof", "fusion1.pof", "fusion2.pof", "cmissile.pof",
            "smissile.pof", "mmissile.pof", "cmissile.pof", "cmissile.pof", "laser1-1.pof", "laser1-2.pof", "laser4-1.pof", "laser4-2.pof",
            "mmissile.pof", "laser5-1.pof", "laser51s.pof", "laser52s.pof", "laser5-2.pof", "laser6-1.pof", "laser61s.pof", "laser62s.pof",
            "laser6-2.pof", "cmissile.pof", "cmissile.pof", "mercmiss.pof", "erthshkr.pof", "tracer.pof", "laser6-1.pof", "laser6-2.pof",
            "cmissile.pof", "newbomb.pof", "erthbaby.pof", "mercmiss.pof", "smissile.pof", "erthshkr.pof", "erthbaby.pof", "cmissile.pof" };

        public static int[] pofIndicies = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 14, 15, 16, 17, 18, 19, 20, 22, 23, 24, 25, 28, 29, 30, 31, 32, 33,
            34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 48, 49, 51, 55, 56, 57, 58, 59, 60, 61, 62, 63, 64, 65, 66, 67, 68, 69, 70,
            71, 72, 73, 74, 75, 76, 77, 88, 89, 91, 92, 93, 94, 95, 96, 97, 98, 99, 100, 101, 102, 103, 104, 105, 106, 107, 108, 109, 110, 111, 112,
            113, 114, 115, 116, 117, 118, 119, 120, 121, 122, 123, 124, 125, 126, 127, 128, 129, 130, 131, 132, 134, 135, 143, 144, 145, 146, 147,
            148, 149, 150, 153, 154, 155, 159, 160 };

        //TODO: This isn't internationalization safe, because c# makes it more painful than it needs to be to format something specifically
        public static string GenerateBitmapsTable(EditorHAMFile datafile, PIGFile piggyFile, SNDFile sndFile)
        {
            StringBuilder stringBuilder = new StringBuilder();
            Robot robot; Weapon weapon; VClip vclip;
            int lastTexture = 0;
            for (int i = 0; i < datafile.Robots.Count; i++)
            {
                robot = datafile.Robots[i];
                TableWriteRobot(datafile, stringBuilder, robot, i);
            }
            for (int i = 0; i < datafile.Robots.Count; i++)
            {
                robot = datafile.Robots[i];
                TableWriteRobotAI(datafile, stringBuilder, robot, i);
            }
            foreach (Reactor reactor in datafile.Reactors)
            {
                TableWriteReactor(datafile, stringBuilder, reactor);
            }
            TableWritePlayerShip(datafile, stringBuilder, datafile.PlayerShip, piggyFile);
            for (int i = 0; i < datafile.Sounds.Count; i++)
                TableWriteSound(datafile, stringBuilder, i, sndFile);
            //stringBuilder.Append("\n");
            TableWriteCockpits(datafile, stringBuilder, piggyFile); //stringBuilder.Append("\n");
            TableWriteGauges(datafile, stringBuilder, piggyFile); //stringBuilder.Append("\n");
            TableWriteGaugesHires(datafile, stringBuilder, piggyFile); //stringBuilder.Append("\n");
            for (int i = 0; i < datafile.Weapons.Count; i++)
            {
                weapon = datafile.Weapons[i];
                TableWriteWeapon(datafile, stringBuilder, weapon, piggyFile, i);
            }
            TableWritePowerups(datafile, stringBuilder); //stringBuilder.Append("\n");
            for (int i = 0; i < datafile.VClips.Count; i++)
            {
                vclip = datafile.VClips[i];
                TableWriteVClip(datafile, stringBuilder, vclip, i, piggyFile);
            }
            //stringBuilder.Append("\n");
            lastTexture = TableWriteTextures(datafile, stringBuilder, piggyFile);
            //stringBuilder.Append("\n");
            TableWriteEClips(datafile, stringBuilder, piggyFile, lastTexture);
            //stringBuilder.Append("\n");
            TableWriteWalls(datafile, stringBuilder, piggyFile);

            return stringBuilder.ToString();
        }

        public static void TableWriteEClips(EditorHAMFile datafile, StringBuilder stringBuilder, PIGFile piggyFile, int start)
        {
            int eclipCount = CountValidEClips(datafile);
            bool extra;
            PIGImage img;
            TMAPInfo info = null;
            stringBuilder.Append("$EFFECTS\n");
            int endPoint = datafile.WClips[0].Frames[0];
            EClip clip;

            //Painful hack, needed for the file to parse properly.
            for (int i = start; i <= endPoint; i++)
            {
                if (datafile.TMapInfo[i].EClipNum != -1)
                {
                    clip = datafile.EClips[datafile.TMapInfo[i].EClipNum];
                    extra = false;
                    if (clip.Clip.PlayTime > 0)
                    {
                        stringBuilder.AppendFormat("$ECLIP clip_num={0} time={1} abm_flag=1 ", clip.ID, clip.Clip.PlayTime);
                        img = piggyFile.Bitmaps[clip.Clip.Frames[0]];
                        if (clip.ChangingWallTexture != -1)
                        {
                            if (clip.CriticalClip != -1)
                                stringBuilder.AppendFormat("crit_clip={0} ", clip.CriticalClip);
                            if (clip.DestroyedBitmapNum != -1)
                                stringBuilder.AppendFormat("dest_bm={0}.bbm ", piggyFile.Bitmaps[datafile.Textures[clip.DestroyedBitmapNum]].Name);
                            if (clip.ExplosionVClip != 0)
                                stringBuilder.AppendFormat("dest_vclip={0} ", clip.ExplosionVClip);
                            if (clip.ExplosionEClip != -1)
                                stringBuilder.AppendFormat("dest_eclip={0} ", clip.ExplosionEClip);
                            if (clip.SoundNum != -1)
                                stringBuilder.AppendFormat("sound_num={0} ", clip.SoundNum);
                            if (clip.ExplosionSize != 0)
                                stringBuilder.AppendFormat("dest_size={0} ", clip.ExplosionSize);
                            info = datafile.TMapInfo[clip.ChangingWallTexture];
                            extra = true;
                        }
                        else if (clip.ChangingObjectTexture != -1)
                            stringBuilder.Append("obj_eclip=1 ");
                        if ((img.Flags & PIGImage.BM_FLAG_NO_LIGHTING) != 0)
                            stringBuilder.Append("vlighting=-1 ");

                        stringBuilder.AppendFormat("\n{0}.abm ", img.Name);

                        if (extra)
                        {
                            if (info.Lighting > 0)
                                stringBuilder.AppendFormat("lighting={0} ", info.Lighting);
                            if (info.Damage > 0)
                                stringBuilder.AppendFormat("damage={0} ", info.Damage);
                            if ((info.Flags & TMAPInfo.TMI_VOLATILE) != 0)
                                stringBuilder.Append("volatile ");
                            if ((info.Flags & TMAPInfo.TMI_GOAL_RED) != 0)
                                stringBuilder.Append("goal_red ");
                            if ((info.Flags & TMAPInfo.TMI_GOAL_BLUE) != 0)
                                stringBuilder.Append("goal_blue ");
                            if ((info.Flags & TMAPInfo.TMI_WATER) != 0)
                                stringBuilder.Append("water ");
                            if ((info.Flags & TMAPInfo.TMI_FORCE_FIELD) != 0)
                                stringBuilder.Append("force_field ");
                            if ((img.Flags & PIGImage.BM_FLAG_SUPER_TRANSPARENT) != 0)
                                stringBuilder.Append("superx=254 ");
                            if (info.SlideU != 0 || info.SlideV != 0)
                                stringBuilder.AppendFormat("slide={0:F1} {1:F1} ", info.SlideU / 256.0f, info.SlideV / 256.0f);
                        }
                        stringBuilder.Append("\n");
                    }
                }
            }
            //Write the rest of the EClips
            for (int i = 0; i < datafile.EClips.Count; i++)
            {
                clip = datafile.EClips[i];
                if (clip.ChangingObjectTexture != -1)
                {
                    extra = false;
                    if (clip.Clip.PlayTime > 0)
                    {
                        stringBuilder.AppendFormat("$ECLIP clip_num={0} time={1} abm_flag=1 ", clip.ID, clip.Clip.PlayTime);
                        img = piggyFile.Bitmaps[clip.Clip.Frames[0]];
                        stringBuilder.Append("obj_eclip=1 ");
                        if ((img.Flags & PIGImage.BM_FLAG_NO_LIGHTING) != 0)
                            stringBuilder.Append("vlighting=-1 ");

                        stringBuilder.AppendFormat("\n{0}.abm ", img.Name);
                        stringBuilder.Append("\n");
                    }
                }
            }
        }

        public static void TableWriteWalls(EditorHAMFile datafile, StringBuilder stringBuilder, PIGFile piggyFile)
        {
            PIGImage img, frame;
            TMAPInfo info;
            WClip clip;
            bool superx;
            stringBuilder.AppendFormat("$WALL_ANIMS Num_wall_anims={0}\n", datafile.WClips.Count);
            for (int i = 0; i < datafile.WClips.Count; i++)
            {
                superx = false;
                clip = datafile.WClips[i];
                if (clip.PlayTime != 0)
                {
                    info = datafile.TMapInfo[clip.Frames[0]];
                    img = piggyFile.Bitmaps[datafile.Textures[clip.Frames[0]]];
                    stringBuilder.AppendFormat("$WCLIP clip_num={0} time={1} abm_flag=1 ", i, clip.PlayTime);
                    if (clip.OpenSound != -1)
                        stringBuilder.AppendFormat("open_sound={0} ", clip.OpenSound);
                    if (clip.CloseSound != -1)
                        stringBuilder.AppendFormat("close_sound={0} ", clip.CloseSound);
                    if ((img.Flags & PIGImage.BM_FLAG_NO_LIGHTING) != 0)
                        stringBuilder.Append("vlighting=-1 ");
                    else
                        stringBuilder.Append("vlighting=0 ");
                    if ((clip.Flags & WClip.WCF_TMAP1) != 0)
                        stringBuilder.Append("tmap1_flag=1 ");
                    if ((clip.Flags & WClip.WCF_BLASTABLE) != 0)
                        stringBuilder.Append("blastable=1 ");
                    if ((clip.Flags & WClip.WCF_BLASTABLE) != 0)
                        stringBuilder.Append("explodes=1 ");
                    if ((clip.Flags & WClip.WCF_HIDDEN) != 0)
                        stringBuilder.Append("hidden=1 ");
                    stringBuilder.AppendFormat("\n{0}.abm ", img.Name);
                    if (info.Lighting > 0)
                        stringBuilder.AppendFormat("lighting={0} ", info.Lighting);
                    if (info.Damage > 0)
                        stringBuilder.AppendFormat("damage={0} ", info.Damage);
                    if ((info.Flags & TMAPInfo.TMI_VOLATILE) != 0)
                        stringBuilder.Append("volatile ");
                    if ((info.Flags & TMAPInfo.TMI_GOAL_RED) != 0)
                        stringBuilder.Append("goal_red ");
                    if ((info.Flags & TMAPInfo.TMI_GOAL_BLUE) != 0)
                        stringBuilder.Append("goal_blue ");
                    if ((info.Flags & TMAPInfo.TMI_WATER) != 0)
                        stringBuilder.Append("water ");
                    if ((info.Flags & TMAPInfo.TMI_FORCE_FIELD) != 0)
                        stringBuilder.Append("force_field ");
                    for (int f = 0; f < clip.NumFrames; f++) //Need to scan all frames for superx
                    {
                        frame = piggyFile.Bitmaps[datafile.Textures[clip.Frames[f]]];
                        if ((frame.Flags & PIGImage.BM_FLAG_SUPER_TRANSPARENT) != 0)
                            superx = true;
                    }
                    if (superx)
                        stringBuilder.Append("superx=254 ");
                    if (info.SlideU != 0 || info.SlideV != 0)
                        stringBuilder.AppendFormat("slide={0:F1} {1:F1} ", info.SlideU / 256.0f, info.SlideV / 256.0f);
                    stringBuilder.Append("\n");
                }
            }
        }

        public static int TableWriteTextures(EditorHAMFile datafile, StringBuilder stringBuilder, PIGFile piggyFile)
        {
            PIGImage img;
            TMAPInfo info;
            EClip clip;
            int firstEClip = datafile.EClips[0].ChangingWallTexture;
            bool extra;
            stringBuilder.Append("$TEXTURES\n");
            for (int i = 0; i < firstEClip; i++)
            {
                extra = false;
                img = piggyFile.Bitmaps[datafile.Textures[i]];
                info = datafile.TMapInfo[i];
                if (img.IsAnimated && info.EClipNum == -1) //Probably a WClip so don't write yet. 
                    continue;
                if (info.EClipNum == -1 && i < firstEClip)
                {
                    stringBuilder.AppendFormat("{0}.bbm ", img.Name);
                    extra = true;
                }
                else if (info.EClipNum != -1)
                {
                    clip = datafile.EClips[info.EClipNum];
                }

                if (extra)
                {
                    if (info.Lighting > 0)
                        stringBuilder.AppendFormat("lighting={0} ", info.Lighting);
                    if (info.Damage > 0)
                        stringBuilder.AppendFormat("damage={0} ", info.Damage);
                    if ((info.Flags & TMAPInfo.TMI_VOLATILE) != 0)
                        stringBuilder.Append("volatile ");
                    if ((info.Flags & TMAPInfo.TMI_GOAL_RED) != 0)
                        stringBuilder.Append("goal_red ");
                    if ((info.Flags & TMAPInfo.TMI_GOAL_BLUE) != 0)
                        stringBuilder.Append("goal_blue ");
                    if ((info.Flags & TMAPInfo.TMI_WATER) != 0)
                        stringBuilder.Append("water ");
                    if ((info.Flags & TMAPInfo.TMI_FORCE_FIELD) != 0)
                        stringBuilder.Append("force_field ");
                    if ((img.Flags & PIGImage.BM_FLAG_SUPER_TRANSPARENT) != 0)
                        stringBuilder.Append("superx=254 ");
                    if (info.SlideU != 0 || info.SlideV != 0)
                        stringBuilder.AppendFormat("slide={0:F1} {1:F1} ", info.SlideU / 256.0f, info.SlideV / 256.0f);
                    if (info.DestroyedID != -1)
                    {
                        img = piggyFile.Bitmaps[datafile.Textures[info.DestroyedID]];
                        stringBuilder.AppendFormat("destroyed={0}.bbm ", img.Name);
                        i++;
                    }
                    stringBuilder.Append("\n");
                }
            }
            return firstEClip;
        }


        public static int CountValidEClips(EditorHAMFile datafile)
        {
            int count = 0;
            foreach (EClip clip in datafile.EClips)
            {
                if (clip.Clip.PlayTime > 0)
                    count++;
            }
            return count;
        }

        private static void TableWriteVClip(EditorHAMFile datafile, StringBuilder stringBuilder, VClip clip, int id, PIGFile piggyFile)
        {
            if (clip.PlayTime != 0)
            {
                stringBuilder.AppendFormat("$VCLIP clip_num={0} time={1} abm_flag=1 vlighting={2} sound_num={3} ", id, clip.PlayTime, clip.LightValue, clip.SoundNum);
                if ((clip.Flags & 1) != 0)
                    stringBuilder.Append("rod_flag=1");
                stringBuilder.AppendFormat("\n{0}.abm\n", piggyFile.Bitmaps[clip.Frames[0]].Name);
            }
        }

        private static void TableWriteWeapon(EditorHAMFile datafile, StringBuilder stringBuilder, Weapon weapon, PIGFile piggyFile, int id)
        {
            if (weapon.RenderType == 0)
                stringBuilder.Append("$WEAPON_UNUSED ");
            else
            {
                stringBuilder.Append("$WEAPON ");
                if (weapon.RenderType == WeaponRenderType.Sprite)
                {
                    stringBuilder.AppendFormat("blob_bmp={0}.bbm ", piggyFile.Bitmaps[weapon.Bitmap].Name);
                }
                else if (weapon.RenderType == WeaponRenderType.Object)
                {
                    stringBuilder.Append("weapon_pof=");
                    WriteModel(datafile, stringBuilder, weapon.ModelNum);
                    if (weapon.ModelNumInner != -1)
                    {
                        stringBuilder.Append("weapon_pof_inner=");
                        WriteModel(datafile, stringBuilder, weapon.ModelNumInner);
                    }
                    stringBuilder.AppendFormat("lw_ratio={0} ", weapon.POLenToWidthRatio);
                }
                else if (weapon.RenderType == WeaponRenderType.VClip)
                {
                    stringBuilder.AppendFormat("weapon_vclip={0} ", weapon.WeaponVClip);
                }
                else if (weapon.RenderType == WeaponRenderType.Invisible)
                {
                    stringBuilder.AppendFormat("none_bmp={0}.bbm ", piggyFile.Bitmaps[weapon.Bitmap].Name);
                }
                stringBuilder.AppendFormat("mass={0} ", weapon.Mass);
                stringBuilder.AppendFormat("drag={0} ", weapon.Drag);
                stringBuilder.AppendFormat("thrust={0} ", weapon.Thrust);
                if (weapon.Matter)
                    stringBuilder.Append("matter=1 ");
                if (weapon.Bounce != 0)
                    stringBuilder.AppendFormat("bounce={0} ", weapon.Bounce);
                if (weapon.Children != -1)
                    stringBuilder.AppendFormat("children={0} ", weapon.Children);
                stringBuilder.Append("strength=");
                for (int i = 0; i < 5; i++)
                    stringBuilder.AppendFormat("{0} ", weapon.Strength[i]);
                stringBuilder.Append("speed=");
                for (int i = 0; i < 5; i++)
                    stringBuilder.AppendFormat("{0} ", weapon.Speed[i]);
                if (weapon.SpeedVariance != 128)
                    stringBuilder.AppendFormat("speedvar={0} ", weapon.SpeedVariance);
                stringBuilder.AppendFormat("blob_size={0} ", weapon.BlobSize);
                stringBuilder.AppendFormat("flash_vclip={0} ", weapon.MuzzleFlashVClip);
                stringBuilder.AppendFormat("flash_size={0} ", weapon.FlashSize);
                if (weapon.FiringSound != 0)
                    stringBuilder.AppendFormat("flash_sound={0} ", weapon.FiringSound);
                stringBuilder.AppendFormat("robot_hit_vclip={0} ", weapon.RobotHitVClip);
                stringBuilder.AppendFormat("wall_hit_vclip={0} ", weapon.WallHitVClip);
                stringBuilder.AppendFormat("robot_hit_sound={0} ", weapon.RobotHitSound);
                stringBuilder.AppendFormat("wall_hit_sound={0} ", weapon.WallHitSound);
                stringBuilder.AppendFormat("impact_size={0} ", weapon.ImpactSize);
                if (weapon.AfterburnerSize != 0)
                    stringBuilder.AppendFormat("afterburner_size={0:F2} ", weapon.AfterburnerSize / 16.0f);
                stringBuilder.AppendFormat("energy_usage={0} ", weapon.EnergyUsage);
                stringBuilder.AppendFormat("ammo_usage={0} ", weapon.AmmoUsage);
                stringBuilder.AppendFormat("fire_wait={0} ", weapon.FireWait);
                stringBuilder.AppendFormat("lifetime={0} ", weapon.Lifetime);
                stringBuilder.AppendFormat("lightcast={0} ", weapon.Light);
                if (weapon.DamageRadius != 0)
                    stringBuilder.AppendFormat("damage_radius={0} ", weapon.DamageRadius);
                if (weapon.MultiDamageScale.Value != 65536)
                    stringBuilder.AppendFormat("multi_damage_scale={0} ", weapon.MultiDamageScale);
                stringBuilder.AppendFormat("fire_count={0} ", weapon.FireCount);
                stringBuilder.AppendFormat("flash_vclip={0} ", weapon.MuzzleFlashVClip);
                if (weapon.Persistent)
                    stringBuilder.Append("persistent=1");
                if (weapon.HomingFlag)
                    stringBuilder.Append("homing=1 ");
                if (weapon.Flags != 0)
                    stringBuilder.Append("placable=1 ");
                if (weapon.Flash != 0)
                    stringBuilder.AppendFormat("flash={0} ", weapon.Flash);
                if (weapon.CockpitPicture != 0)
                    stringBuilder.AppendFormat("picture={0}.bbm ", piggyFile.Bitmaps[weapon.CockpitPicture].Name);
                if (weapon.HiresCockpitPicture != 0)
                    stringBuilder.AppendFormat("hires_picture={0}.bbm ", piggyFile.Bitmaps[weapon.HiresCockpitPicture].Name);

                stringBuilder.AppendFormat(" ;{0}", datafile.WeaponNames[id]);

            }
            stringBuilder.Append("\n");
        }

        private static void TableWriteCockpits(EditorHAMFile datafile, StringBuilder stringBuilder, PIGFile piggyFile)
        {
            stringBuilder.Append("$COCKPIT\n");
            foreach (ushort index in datafile.Cockpits)
            {
                stringBuilder.AppendFormat("{0}.bbm\n", piggyFile.Bitmaps[index].Name);
            }
        }

        private static void TableWritePowerups(EditorHAMFile datafile, StringBuilder stringBuilder)
        {
            Powerup powerup;
            for (int i = 0; i < datafile.Powerups.Count; i++)
            {
                powerup = datafile.Powerups[i];
                if (powerup.VClipNum == 0)
                    stringBuilder.Append("$POWERUP_UNUSED\t");
                else
                    stringBuilder.Append("$POWERUP\t");
                stringBuilder.AppendFormat("name=\"{0}\"\t", powerupsbm[i]);
                stringBuilder.AppendFormat("vclip_num={0}\t", powerup.VClipNum);
                stringBuilder.AppendFormat("hit_sound={0}\t", powerup.HitSound);
                if (powerup.Size.Value != (3 * 65536))
                    stringBuilder.AppendFormat("size={0}\t", powerup.Size);
                if (powerup.Size.Value != 21845)
                    stringBuilder.AppendFormat("light={0}", powerup.Light);
                stringBuilder.Append("\n");
            }
        }

        private static void TableWriteGauges(EditorHAMFile datafile, StringBuilder stringBuilder, PIGFile piggyFile)
        {
            stringBuilder.Append("$GAUGES");
            string name = "", lastname;
            int id;
            for (int i = 0; i < datafile.Gauges.Count; i++)
            {
                id = datafile.Gauges[i];
                if (id != 0)
                {
                    PIGImage img = piggyFile.Bitmaps[id];
                    lastname = name;
                    name = img.Name;
                    if (lastname != name)
                    {
                        if (img.IsAnimated)
                        {
                            stringBuilder.AppendFormat(" abm_flag=1\n{0}.abm", name);
                        }
                        else
                        {
                            stringBuilder.AppendFormat("\n{0}.bbm", name);
                        }
                    }
                }
            }
            stringBuilder.Append("\n");
        }

        private static void TableWriteGaugesHires(EditorHAMFile datafile, StringBuilder stringBuilder, PIGFile piggyFile)
        {
            stringBuilder.Append("$GAUGES_HIRES");
            string name = "", lastname;
            int id;
            for (int i = 0; i < datafile.GaugesHires.Count; i++)
            {
                id = datafile.GaugesHires[i];
                if (id != 0)
                {
                    PIGImage img = piggyFile.Bitmaps[id];
                    lastname = name;
                    name = img.Name;
                    if (lastname != name)
                    {
                        if (img.IsAnimated)
                        {
                            stringBuilder.AppendFormat(" abm_flag=1\n{0}.abm", name);
                        }
                        else
                        {
                            stringBuilder.AppendFormat("\n{0}.bbm", name);
                        }
                    }
                }
            }
            stringBuilder.Append("\n");
        }

        private static void TableWriteSound(EditorHAMFile datafile, StringBuilder stringBuilder, int id, SNDFile sndFile)
        {
            int altID;
            if (datafile.Sounds[id] != 255)
            {
                if (datafile.AltSounds[id] == id)
                    altID = 0;
                else if (datafile.AltSounds[id] == 255)
                    altID = -1;
                else altID = datafile.AltSounds[id];
                stringBuilder.AppendFormat("$SOUND\t{0}\t{1}\t{2}.raw\t;{3}\n", id, altID, sndFile.Sounds[datafile.Sounds[id]].Name, ElementLists.GetSoundName(id));
            }
        }

        private static void TableWritePlayerShip(EditorHAMFile datafile, StringBuilder stringBuilder, Ship ship, PIGFile pigFile)
        {
            stringBuilder.Append("$MARKER ");
            WriteModel(datafile, stringBuilder, datafile.PlayerShip.MarkerModel);
            stringBuilder.Append("\n");
            stringBuilder.Append("$PLAYER_SHIP ");
            stringBuilder.AppendFormat("mass={0} ", ship.Mass);
            stringBuilder.AppendFormat("drag={0} ", ship.Drag );
            stringBuilder.AppendFormat("max_thrust={0} ", ship.MaxThrust);
            stringBuilder.AppendFormat("wiggle={0} ", ship.Wiggle);
            stringBuilder.AppendFormat("max_rotthrust={0} ", ship.MaxRotationThrust);
            stringBuilder.AppendFormat("expl_vclip_num={0} ", ship.DeathVClipNum);
            stringBuilder.Append("model=");
            WriteModel(datafile, stringBuilder, ship.ModelNum);
            stringBuilder.Append("multi_textures ");
            for (int i = 0; i < 14; i++)
            {
                int bitmapID = datafile.ObjBitmaps[datafile.ObjBitmapPointers[datafile.FirstMultiBitmapNum + i]];
                string name = pigFile.Bitmaps[bitmapID].Name;
                stringBuilder.AppendFormat("{0}.bbm ", name);
            }
            stringBuilder.Append("\n");
        }

        private static void TableWriteReactor(EditorHAMFile datafile, StringBuilder stringBuilder, Reactor reactor)
        {
            stringBuilder.Append("$REACTOR ");
            WriteModel(datafile, stringBuilder, reactor.ModelNum);
            stringBuilder.Append("\n");
        }

        private static void TableWriteRobotAI(EditorHAMFile datafile, StringBuilder stringBuilder, Robot robot, int id)
        {
            stringBuilder.AppendFormat("$ROBOT_AI {0} ", id);
            for (int i = 0; i < 5; i++)
            {
                stringBuilder.AppendFormat("{0} ", (int)(Math.Acos(robot.FieldOfView[i]) * 180.0d / Math.PI));
            }
            for (int i = 0; i < 5; i++)
            {
                stringBuilder.AppendFormat("{0} ", robot.FiringWait[i]);
            }
            for (int i = 0; i < 5; i++)
            {
                stringBuilder.AppendFormat("{0} ", robot.FiringWaitSecondary[i]);
            }
            for (int i = 0; i < 5; i++)
            {
                stringBuilder.AppendFormat("{0} ", robot.RapidfireCount[i]);
            }
            for (int i = 0; i < 5; i++)
            {
                stringBuilder.AppendFormat("{0} ", robot.TurnTime[i]);
            }
            for (int i = 0; i < 5; i++)
            {
                stringBuilder.AppendFormat("{0} ", robot.MaxSpeed[i]);
            }
            for (int i = 0; i < 5; i++)
            {
                stringBuilder.AppendFormat("{0} ", robot.CircleDistance[i]);
            }
            for (int i = 0; i < 5; i++)
            {
                stringBuilder.AppendFormat("{0} ", robot.EvadeSpeed[i]);
            }
            stringBuilder.AppendFormat(" ;{0}", robotsbm[id]);
            stringBuilder.Append("\n");
        }

        private static void TableWriteRobot(EditorHAMFile datafile, StringBuilder stringBuilder, Robot robot, int id)
        {
            stringBuilder.Append("$ROBOT ");
            WriteModel(datafile, stringBuilder, robot.ModelNum);
            stringBuilder.AppendFormat("name=\"{0}\" ", robotsbm[id]);
            stringBuilder.AppendFormat("score_value={0} ", robot.ScoreValue);
            stringBuilder.AppendFormat("mass={0} ", robot.Mass);
            stringBuilder.AppendFormat("drag={0} ", robot.Drag);
            stringBuilder.AppendFormat("exp1_vclip={0} ", robot.HitVClipNum);
            stringBuilder.AppendFormat("exp1_sound={0} ", robot.HitSoundNum);
            stringBuilder.AppendFormat("exp2_vclip={0} ", robot.DeathVClipNum);
            stringBuilder.AppendFormat("exp2_sound={0} ", robot.DeathSoundNum);
            stringBuilder.AppendFormat("lighting={0} ", robot.Lighting);
            stringBuilder.AppendFormat("weapon_type={0} ", robot.WeaponType);
            if (robot.WeaponTypeSecondary != -1)
                stringBuilder.AppendFormat("weapon_type2={0} ", robot.WeaponTypeSecondary);
            stringBuilder.AppendFormat("strength={0} ", (int)robot.Strength);
            if (robot.ContainsType == 2)
                stringBuilder.Append("contains_type=1 ");
            stringBuilder.AppendFormat("contains_id={0} ", robot.ContainsID);
            stringBuilder.AppendFormat("contains_count={0} ", robot.ContainsCount);
            stringBuilder.AppendFormat("contains_prob={0} ", robot.ContainsProbability);
            if (robot.AttackType != 0)
                stringBuilder.Append("attack_type=1 ");
            stringBuilder.AppendFormat("see_sound={0} ", robot.SeeSound);
            stringBuilder.AppendFormat("attack_sound={0} ", robot.AttackSound);
            if (robot.ClawSound != 190)
                stringBuilder.AppendFormat("claw_sound={0} ", robot.ClawSound);
            if (robot.CloakType != 0)
                stringBuilder.AppendFormat("cloak_type={0} ", robot.CloakType);
            if (robot.Glow != 0)
            {
                Fix glowval = robot.Glow / 16.0f;
                stringBuilder.AppendFormat("glow={0} ", glowval);
            }
            if (robot.LightCast != 0)
                stringBuilder.AppendFormat("lightcast={0} ", robot.LightCast);
            if (robot.DeathExplosionRadius != 0)
                stringBuilder.AppendFormat("badass={0} ", robot.DeathExplosionRadius);
            if (robot.DeathRollTime != 0)
                stringBuilder.AppendFormat("death_roll={0} ", robot.DeathRollTime);
            if (robot.DeathRollSound != 185)
                stringBuilder.AppendFormat("deathroll_sound={0} ", robot.DeathRollSound);
            if (robot.Thief)
                stringBuilder.Append("thief=1 ");
            if (robot.Kamikaze != 0)
                stringBuilder.Append("kamikaze=1 ");
            if (robot.Companion)
                stringBuilder.Append("companion=1 ");
            if (robot.Pursuit != 0)
                stringBuilder.AppendFormat("pursuit={0} ", robot.Pursuit);
            if (robot.SmartBlobsOnDeath != 0)
                stringBuilder.AppendFormat("smart_blobs={0} ", robot.SmartBlobsOnDeath);
            if (robot.SmartBlobsOnHit != 0)
                stringBuilder.AppendFormat("energy_blobs={0} ", robot.SmartBlobsOnHit);
            if (robot.EnergyDrain != 0)
                stringBuilder.AppendFormat("energy_drain={0} ", robot.EnergyDrain);
            if (robot.BossFlag != 0)
                stringBuilder.AppendFormat("boss={0} ", robot.BossFlag);
            if ((robot.Flags & 1) != 0)
                stringBuilder.Append("big_radius=1 ");
            if (robot.Aim != 255)
            {
                stringBuilder.AppendFormat("aim={0:F2} ", robot.Aim / 255.0f);
            }
            if (robot.Behavior >= RobotAIType.Still && robot.Behavior != RobotAIType.Normal)
                stringBuilder.AppendFormat("behavior={0} ", AIBehaviors[(int)robot.Behavior - 0x80]);
            stringBuilder.Append("\n");
        }

        private static void WriteModel(EditorHAMFile datafile, StringBuilder stringBuilder, int id, bool hack = false)
        {
            if (id < 0)
            {
                stringBuilder.Append("fixme.pof ");
                return;
            }
            Polymodel model = datafile.Models[id];
            //stringBuilder.AppendFormat("model{0}.pof ", id);
            stringBuilder.AppendFormat("{0} ", pofNames[id]);
            if (!hack)
            {
                foreach (string texture in model.TextureList)
                {
                    if (datafile.EClipNameMapping.ContainsKey(texture.ToLower()))
                    {
                        stringBuilder.AppendFormat("%{0} ", datafile.EClipNameMapping[texture.ToLower()].ID);
                    }
                    else
                    {
                        stringBuilder.AppendFormat("{0}.bbm ", texture);
                    }
                }
            }
            if (model.DyingModelnum != -1)
            {
                stringBuilder.Append("dying_pof=");
                WriteModel(datafile, stringBuilder, model.DyingModelnum, true);
            }
            if (model.SimplerModels != 0)
            {
                stringBuilder.Append("simple_model=");
                WriteModel(datafile, stringBuilder, model.SimplerModels - 1);
            }
            if (model.DeadModelnum != -1)
            {
                stringBuilder.Append("dead_pof=");
                WriteModel(datafile, stringBuilder, model.DeadModelnum);
            }
        }
    }
}
