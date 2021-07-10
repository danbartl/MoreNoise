using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using HarmonyLib;
using System;
using System.Reflection;
using UnityEngine;

namespace MoreNoise
{
    [BepInPlugin("your.unique.mod.identifier", Plugin.ModName, Plugin.Version)]
    public class Plugin : BaseUnityPlugin
    {
        private static readonly bool isDebug = true;
        public const string Version = "0.1";
        public const string ModName = "MoreNoise";
        public static ConfigEntry<float> miningNoise;
        public static ConfigEntry<float> woodNoise;
        public static ConfigEntry<float> noiseMax;
        public static ConfigEntry<int> CaptureWidth;
        private static float noiseTracker =0f;
        private static float noiseTracker_x = 0f;
        private static float c_value= 0f;
        private static float k_value = 0.000004f;

        Harmony _Harmony;
        public static ManualLogSource Log;
        
        public static void Dbgl(string str = "", bool pref = true)
        {
            if (isDebug)
                Debug.Log((pref ? typeof(Plugin).Namespace + " " : "") + str);
        }

        private void Awake()
        {
            //miningNoise = Config.AddSetting("General", "miningNoise", 200f, new ConfigDescription("Description", new AcceptableValueRange<float>(100f, 300f)));
            //woodNoise = Config.AddSetting("General", "woodNoise", 50f, new ConfigDescription("Description", new AcceptableValueRange<float>(50f, 300f)));
            //noiseMax = Config.AddSetting("General", "noiseMax", 100f, new ConfigDescription("Description", new AcceptableValueRange<float>(301f, 500f)));
            miningNoise = Config.Bind("General", "miningNoise", 100f, new ConfigDescription("Description", new AcceptableValueRange<float>(100f, 300f)));
            woodNoise = Config.Bind("General", "woodNoise", 50f, new ConfigDescription("Description", new AcceptableValueRange<float>(50f, 300f)));
            noiseMax = Config.Bind("General", "noiseMax", 400f, new ConfigDescription("Description", new AcceptableValueRange<float>(301f, 500f)));

            //woodNoise = Config.Bind<float>("General", "woodNoise", 50f, "Noise multiplier to chopping wood");
            //noiseMax = Config.Bind<float>("General", "noiseMax", 371f, "Maximum Noise");

                        
#if DEBUG
			Log = Logger;
#else
            Log = new ManualLogSource(null);
#endif
            _Harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), null);
             
        }


        [HarmonyPatch(typeof(ZNetScene), "Awake")]
        public static class ZNetScene_Awake_Patch
        {
            public static void Postfix(ZNetScene __instance)
            {
                //var beehive = __instance.GetPrefab("Beehive");
                
                var Beech = ZNetScene.instance.GetPrefab("Beech_Stub");
                var BeechCom = Beech.GetComponent<Destructible>();
                
                //Destructible beehiveCom = beehive.GetComponent<Destructible>();
                BeechCom.m_hitNoise = woodNoise.Value;

                var Birch = ZNetScene.instance.GetPrefab("BirchStub");
                var BirchCom = Birch.GetComponent<Destructible>();

                //Destructible beehiveCom = beehive.GetComponent<Destructible>();
                BirchCom.m_hitNoise = woodNoise.Value;
                
                var FirTree = ZNetScene.instance.GetPrefab("FirTree_Stub");
                var FirTreeCom = FirTree.GetComponent<Destructible>();

                //Destructible beehiveCom = beehive.GetComponent<Destructible>();
                FirTreeCom.m_hitNoise = woodNoise.Value;

                var Oak = ZNetScene.instance.GetPrefab("OakStub");
                var OakCom = Oak.GetComponent<Destructible>();

                //Destructible beehiveCom = beehive.GetComponent<Destructible>();
                OakCom.m_hitNoise = woodNoise.Value;
           
                var Pinetree_01 = ZNetScene.instance.GetPrefab("Pinetree_01_Stub");
                var Pinetree_01Com = Pinetree_01.GetComponent<Destructible>();

                //Destructible beehiveCom = beehive.GetComponent<Destructible>();
                Pinetree_01Com.m_hitNoise = woodNoise.Value;


                var SwampTree1 = ZNetScene.instance.GetPrefab("SwampTree1_Stub");
                var SwampTree1Com = SwampTree1.GetComponent<Destructible>();

                //Destructible beehiveCom = beehive.GetComponent<Destructible>();
                SwampTree1Com.m_hitNoise = woodNoise.Value;
                

            }
        }

        [HarmonyPatch(typeof(MineRock5), "Damage")]
        static class MineRock5_Damage_Patch
        {
            static void Prefix(MineRock5 __instance, ref HitData hit)
            {

                Player closestPlayer = Player.GetClosestPlayer(hit.m_point, 10f);

                if ((bool)closestPlayer)
                {
                    noiseTracker = closestPlayer.GetNoiseRange();
                }
            }

            static void Postfix(MineRock5 __instance, ref HitData hit)
            {

                Player closestPlayer = Player.GetClosestPlayer(hit.m_point, 10f);

                if ((bool)closestPlayer)
                {
                    
                    double miningNoise_conv = System.Convert.ToDouble(miningNoise.Value);
                    double noiseMax_conv = System.Convert.ToDouble(noiseMax.Value);

                    c_value = (float)Math.Exp((float)k_value * (float)noiseMax.Value * (float)(miningNoise.Value)) * ((float)noiseMax.Value / 100f - 1f);

                    noiseTracker_x = (float)Math.Max(
                        -1f*Math.Log((float)noiseMax.Value / ((float)noiseTracker * (float)c_value) - 1 / (float)c_value) / ((float)k_value * (float)noiseMax.Value) + (float)miningNoise_conv
                        , (float)miningNoise_conv);
                    noiseTracker = (float)((float)noiseMax_conv / (1 + (float)c_value * Math.Exp((float)(k_value) * (float)(noiseMax_conv) * (float)noiseTracker_x * -1f)));
                    /*
                    Dbgl($"Noise was given back as {closestPlayer.m_noiseRange}");
                    Dbgl($"Noise value was {miningNoise.Value}");                   
                    Dbgl($"noiseTracker_x valuex was {noiseTracker_x}");
                    Dbgl($"c_value was {c_value}");
                    */
                    Dbgl($"Final Noise value was {noiseTracker}");
                    closestPlayer.m_noiseRange = noiseTracker;
                }
            }
        }

        [HarmonyPatch(typeof(MineRock), "Damage")]
        static class MineRock_Damage_Patch
        {
            static void Prefix(MineRock __instance, ref HitData hit)
            {
             
                Player closestPlayer = Player.GetClosestPlayer(hit.m_point, 10f);

                if ((bool)closestPlayer)
                {
                    //Dbgl($"Noise was{closestPlayer.GetNoiseRange()}");
                    noiseTracker = closestPlayer.GetNoiseRange();
                }
            }
            static void Postfix(MineRock __instance, ref HitData hit)
            {

                Player closestPlayer = Player.GetClosestPlayer(hit.m_point, 10f);
                if ((bool)closestPlayer)
                {


                    double miningNoise_conv = System.Convert.ToDouble(miningNoise.Value);
                    double noiseMax_conv = System.Convert.ToDouble(noiseMax.Value);

                    c_value = (float)Math.Exp((float)k_value * (float)noiseMax.Value * (float)(miningNoise.Value)) * ((float)noiseMax.Value / 100f - 1f);

                    noiseTracker_x = (float)Math.Max(
                        -1f * Math.Log((float)noiseMax.Value / ((float)noiseTracker * (float)c_value) - 1 / (float)c_value) / ((float)k_value * (float)noiseMax.Value) + (float)miningNoise_conv
                        , (float)miningNoise_conv);
                    noiseTracker = (float)((float)noiseMax_conv / (1 + (float)c_value * Math.Exp((float)(k_value) * (float)(noiseMax_conv) * (float)noiseTracker_x * -1f)));
                    /*
                    Dbgl($"Noise was given back as {closestPlayer.m_noiseRange}");
                    Dbgl($"Noise value was {miningNoise.Value}");
                    Dbgl($"noiseTracker_x value was {noiseTracker_x}");
                    Dbgl($"c_value was {c_value}");
                    */
                    Dbgl($"Final Noise value was {noiseTracker}");
                    closestPlayer.m_noiseRange = noiseTracker;

                    //closestPlayer.m_seman.ModifyNoise(noiseTracker, ref closestPlayer.m_noiseRange);
                    
                }
            }

        }
       
        [HarmonyPatch(typeof(TreeBase), "Damage")]
        static class TreeBase_Damage_Patch
        {
            static void Prefix(TreeBase __instance, ref HitData hit)
            {

                Player closestPlayer = Player.GetClosestPlayer(hit.m_point, 10f);

                if ((bool)closestPlayer)
                {
                    //Dbgl($"Noise was{closestPlayer.GetNoiseRange()}");
                    noiseTracker = closestPlayer.GetNoiseRange();
                }
            }
            static void Postfix(TreeBase __instance, ref HitData hit)
            {

                Player closestPlayer = Player.GetClosestPlayer(hit.m_point, 10f);
                if ((bool)closestPlayer)
                {
                    // closestPlayer.AddNoise(noiseTracker);
                    //Dbgl($"51Noise was really {noiseMax}");

                    double woodNoise_conv = System.Convert.ToDouble(woodNoise.Value);
                    //double noiseMax_conv = System.Convert.ToDouble(noiseMax.Value);

                    noiseTracker = (float)Math.Max((float)woodNoise_conv, (float)noiseTracker);

                    /*
                    Dbgl($"Noise was given back as {closestPlayer.m_noiseRange}");
                    Dbgl($"Noise value was {woodNoise.Value}");
                    noiseTracker = miningNoise - noiseMax;
                    */
                    Dbgl($"Final Noise value was {noiseTracker}");

                    closestPlayer.m_noiseRange = noiseTracker;
                                        

                }
            }

        }

        [HarmonyPatch(typeof(TreeLog), "Damage")]
        static class TreeLog_Damage_Patch
        {
            static void Prefix(TreeLog __instance, ref HitData hit)
            {

                Player closestPlayer = Player.GetClosestPlayer(hit.m_point, 10f);

                if ((bool)closestPlayer)
                {
                    Dbgl($"Noise was{closestPlayer.GetNoiseRange()}");
                    noiseTracker = closestPlayer.GetNoiseRange();
                }
            }
            static void Postfix(TreeLog __instance, ref HitData hit)
            {

                Player closestPlayer = Player.GetClosestPlayer(hit.m_point, 10f);
                if ((bool)closestPlayer)
                {

                    double woodNoise_conv = System.Convert.ToDouble(woodNoise.Value);
                    
                    noiseTracker = (float)Math.Max((float)woodNoise_conv, (float)noiseTracker);

                    /*
                    Dbgl($"Noise was given back as {closestPlayer.m_noiseRange}");
                    Dbgl($"Noise value was {woodNoise.Value}");   
                    */
                    Dbgl($"Final Noise value was {noiseTracker}");
                    closestPlayer.m_noiseRange = noiseTracker;
                    

                }
            }

        }


      


        private void OnDestroy()
        {
            if (_Harmony != null
                ) _Harmony.UnpatchSelf();
        }
    }
}
