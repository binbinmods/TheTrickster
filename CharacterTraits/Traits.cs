using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using Obeliskial_Content;

namespace TheMagician
{
    [HarmonyPatch]
    internal class Traits
    {
        // list of your trait IDs
        public static string[] myTraitList = ["ulfvitrcalltherain","ulfvitrmagnet","ulfvitrregenerator","ulfvitrconductor","ulfvitrlifebloom"]; // TODO fill out traits

        public static int level5ActivationCounter = 0;
        public static int level5MaxActivations = 3;

        public static void DoCustomTrait(string _trait, ref Trait __instance)
        {
            // get info you may need
            Enums.EventActivation _theEvent = Traverse.Create(__instance).Field("theEvent").GetValue<Enums.EventActivation>();
            Character _character = Traverse.Create(__instance).Field("character").GetValue<Character>();
            Character _target = Traverse.Create(__instance).Field("target").GetValue<Character>();
            int _auxInt = Traverse.Create(__instance).Field("auxInt").GetValue<int>();
            string _auxString = Traverse.Create(__instance).Field("auxString").GetValue<string>();
            CardData _castedCard = Traverse.Create(__instance).Field("castedCard").GetValue<CardData>();
            Traverse.Create(__instance).Field("character").SetValue(_character);
            Traverse.Create(__instance).Field("target").SetValue(_target);
            Traverse.Create(__instance).Field("theEvent").SetValue(_theEvent);
            Traverse.Create(__instance).Field("auxInt").SetValue(_auxInt);
            Traverse.Create(__instance).Field("auxString").SetValue(_auxString);
            Traverse.Create(__instance).Field("castedCard").SetValue(_castedCard);
            TraitData traitData = Globals.Instance.GetTraitData(_trait);
            List<CardData> cardDataList = [];
            List<string> heroHand = MatchManager.Instance.GetHeroHand(_character.HeroIndex);
            Hero[] teamHero = MatchManager.Instance.GetTeamHero();
            NPC[] teamNpc = MatchManager.Instance.GetTeamNPC();

            // activate traits
            // I don't know how to set the combatLog text I need to do that for all of the traits
            if (_trait == "ulfvitrcalltherain")
            { // TODO trait 0
                string traitName = _trait;
                
            }

                    
            else if (_trait == "ulfvitrmagnet")
            { // TODO trait 2a
                string traitName = _trait;
                
            }

                
             
            else if (_trait == "ulfvitrregenerator")
            { // TODO trait 2b
                string traitName = _trait;
                
            }

            else if (_trait == "ulfvitrconductor")
            { // TODO trait 4a
                string traitName = _trait;
                
            }

            else if (_trait == "ulfvitrlifebloom")
            { // TODO trait 4b
                string traitName = _trait;
                
            }

        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Trait), "DoTrait")]
        public static bool DoTrait(Enums.EventActivation _theEvent, string _trait, Character _character, Character _target, int _auxInt, string _auxString, CardData _castedCard, ref Trait __instance)
        {
            if ((UnityEngine.Object)MatchManager.Instance == (UnityEngine.Object)null)
                return false;
            Traverse.Create(__instance).Field("character").SetValue(_character);
            Traverse.Create(__instance).Field("target").SetValue(_target);
            Traverse.Create(__instance).Field("theEvent").SetValue(_theEvent);
            Traverse.Create(__instance).Field("auxInt").SetValue(_auxInt);
            Traverse.Create(__instance).Field("auxString").SetValue(_auxString);
            Traverse.Create(__instance).Field("castedCard").SetValue(_castedCard);
            if (Content.medsCustomTraitsSource.Contains(_trait) && myTraitList.Contains(_trait))
            {
                DoCustomTrait(_trait, ref __instance);
                return false;
            }
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Character), "SetEvent")]
        public static void SetEventPrefix(ref Character __instance, ref Enums.EventActivation theEvent, Character target = null)
        {
            /*if (theEvent == Enums.EventActivation.AuraCurseSet && !__instance.IsHero && target != null && target.IsHero && target.HaveTrait("ulfvitrconductor") && __instance.HasEffect("spark"))
            { // if NPC has wet applied to them, deal 50% of their sparks as indirect lightning damage
                __instance.IndirectDamage(Enums.DamageType.Lightning, Functions.FuncRoundToInt((float)__instance.GetAuraCharges("spark") * 0.5f));
            }
            if (theEvent == Enums.EventActivation.BeginTurn && __instance.IsHero && (__instance.HaveTrait("pestilyhealingtoxins")||__instance.HaveTrait("pestilytoxichealing"))){
                level5ActivationCounter=0;
                // Plugin.Log.LogInfo("Binbin - PestilyBiohealer - Reset Activation Counter: "+ level5ActivationCounter);
            }
            
            */
        }



        [HarmonyPostfix]
        [HarmonyPatch(typeof(AtOManager),"GlobalAuraCurseModificationByTraitsAndItems")]
        public static void GlobalAuraCurseModificationByTraitsAndItemsPostfix(ref AtOManager __instance, ref AuraCurseData __result, string _type, string _acId, Character _characterCaster, Character _characterTarget){
            // Shadow Poison -  +1 Shadow Damage per 10 stacks of Poison on you. 
            // Antidote - You are immune to Poison damage, Poison stacks on you are limited to 300
            /*
            if(_acId=="poison")
            {
                if(_type=="set")
                {
                    if (_characterTarget != null && __instance.CharacterHaveTrait(_characterTarget.SubclassName, "pestilyshadowpoison"))
                    {   
                        __result.AuraDamageType = Enums.DamageType.Shadow;
                        int damageIncrease = FloorToInt((float )_characterTarget.GetAuraCharges("poison")/10.0f);
                        //__result.AuraDamageIncreasedPerStack=0.1f;
                        __result.AuraDamageIncreasedTotal = damageIncrease;
                    }
                    if (_characterTarget != null && __instance.CharacterHaveTrait(_characterTarget.SubclassName, "pestilyantidote"))
                    {   
                        __result.MaxCharges = 300;
                        __result.ProduceDamageWhenConsumed = false;
                        __result.DamageWhenConsumedPerCharge = 0.0f;
                        Plugin.Log.LogInfo("Binbin - PestilyBiohealer - Setting Poison: "+ __result.DamageWhenConsumedPerCharge);

                    }
                }
                if(_type=="consume")
                {
                    if (_characterCaster != null && __instance.CharacterHaveTrait(_characterCaster.SubclassName, "pestilyantidote"))
                    {   
                        Plugin.Log.LogInfo("Binbin - PestilyBiohealer - Consuming Poison");

                        __result.MaxCharges = 300;
                        __result.ProduceDamageWhenConsumed=false;
                        __result.DamageWhenConsumedPerCharge=0.0f;
                        Plugin.Log.LogInfo("Binbin - PestilyBiohealer - Consuming Poison: "+ __result.DamageWhenConsumedPerCharge);
                    }
                }
                */
            }


        
    }
}
