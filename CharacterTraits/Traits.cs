using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using Obeliskial_Content;
using static TheMagician.CustomFunctions;
using static TheMagician.Plugin;
using UnityEngine;

namespace TheMagician
{


    [HarmonyPatch]
    internal class Traits
    {
        // list of your trait IDs
        public static string heroName = "jack";

        public static string subclassname = "magician";

        public static string debugBase = "Binbin - Testing " + heroName + " ";

        public static string[] myTraitList = ["trickstermagictrick",
                                                "tricksterpractice",
                                                "tricksterstudy",
                                                "trickstertrickupyoursleeve",
                                                "tricksterlearnrealmagic",
                                                "trickstersecretpocket",
                                                "tricksterimprovise",
                                                "tricksterdistractingact",
                                                "tricksterdrawpower"];

        public static int cardsPlayedPerTurn = 0;
        //public static int level5MaxActivations = 3;

        public static string trait0 = "trickstermagictrick";
        public static string trait2a = "tricksterstudy";
        public static string trait2b = "trickstertrickupyoursleeve";
        public static string trait4a = "tricksterdistractingact";
        public static string trait4b = "tricksterdrawpower";

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
            if (_trait == trait0)
            { // Front hero starts with 1 Evasion
                string traitName = _trait;
                Character frontHero = GetFrontCharacter(teamHero);
                frontHero.SetAuraTrait(_character, "evasion", 1);
                _character.HeroItem.ScrollCombatText(Texts.Instance.GetText("traits_" + traitName), Enums.CombatScrollEffectType.Trait);
            }


            else if (_trait == trait2a)
            { // When you play your first Small Weapon each turn, draw a card and reduce its cost by 2.
                string traitName = _trait;
                // cardsPlayedPerTurn += 1;
                if (CanIncrementTraitActivations(_trait) && _castedCard.HasCardType(Enums.CardType.Small_Weapon))
                {
                    DrawCards(1);
                    CardData cardToReduce = GetRightmostCard(heroHand);
                    if (cardToReduce == null)
                    {
                        return;
                    }
                    ReduceCardCost(ref cardToReduce, _character, 2,isPermanent:true);
                    IncrementTraitActivations(_trait);
                    _character.HeroItem.ScrollCombatText(Texts.Instance.GetText("traits_" + traitName), Enums.CombatScrollEffectType.Trait);

                }
            }

            else if (_trait == trait2b)
            { // Subclass Mage. At the start of your turn, reduce the cost of your highest cost Skill, Spell and Book by 1.
                string traitName = _trait;
                if (_castedCard != null && _castedCard)
                {

                    if (!((UnityEngine.Object)MatchManager.Instance != (UnityEngine.Object)null) || !((UnityEngine.Object)_castedCard != (UnityEngine.Object)null)) { return; }
                    CardData skill = GetRandomHighestCostCard(Enums.CardType.Skill, heroHand);
                    CardData spell = GetRandomHighestCostCard(Enums.CardType.Spell, heroHand);
                    CardData book = GetRandomHighestCostCard(Enums.CardType.Book, heroHand);
                    
                    ReduceCardCost(ref skill, _character, 1,isPermanent:false);
                    ReduceCardCost(ref spell, _character, 1,isPermanent:false);
                    ReduceCardCost(ref book, _character, 1,isPermanent:false);

                    _character.HeroItem.ScrollCombatText(Texts.Instance.GetText("traits_" + traitName), Enums.CombatScrollEffectType.Trait);

                }
            }

            else if (_trait == trait4a)
            { // +2 Stealth, +1 Evasion. After you play a card, gain 1 Stealth if you had none.
                string traitName = _trait;
                if (_castedCard != null && !_character.HasEffect("stealth"))
                {
                    _character.SetAuraTrait(_character, "stealth", 1);
                    _character.HeroItem.ScrollCombatText(Texts.Instance.GetText("traits_" + traitName), Enums.CombatScrollEffectType.Trait);
                }
            }

            else if (_trait == trait4b)
            { // +1 Inspire. At the start of your turn, you lose 2 more Powerful. When you draw a card, gain 1 Powerful. Max Powerful charges +5
                string traitName = _trait;
                if (IsLivingHero(_character))
                {
                    _character.SetAuraTrait(_character, "powerful", 1);
                    _character.HeroItem.ScrollCombatText(Texts.Instance.GetText("traits_" + traitName), Enums.CombatScrollEffectType.Trait);
                }
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

        // [HarmonyPrefix]
        // [HarmonyPatch(typeof(Character), "SetEvent")]
        // public static void SetEventPrefix(ref Character __instance, ref Enums.EventActivation theEvent, Character target = null)
        // {
        //     string traitOfInterest = myTraitList[3]; //trickupyoursleeves
        //     if (theEvent == Enums.EventActivation.BeginTurn && __instance.IsHero && __instance.HaveTrait(traitOfInterest)){
        //         cardsPlayedPerTurn=0;
        //         // Plugin.Log.LogInfo("Binbin - PestilyBiohealer - Reset Activation Counter: "+ level5ActivationCounter);
        //     }
        // }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(AtOManager), "HeroLevelUp")]
        public static bool HeroLevelUpPrefix(ref AtOManager __instance, Hero[] ___teamAtO, int heroIndex, string traitId)
        {
            Hero hero = ___teamAtO[heroIndex];
            LogDebug("Level up before conditions for subclass " + hero.SubclassName + " trait id " + traitId);

            string traitOfInterest = trait2b; // Gain Mage Subclass
            if (hero.AssignTrait(traitId))
            {
                TraitData traitData = Globals.Instance.GetTraitData(traitId);
                if ((UnityEngine.Object)traitData != (UnityEngine.Object)null && traitId == traitOfInterest)
                {
                    LogDebug("Setting mage subclass inside conditions");
                    Globals.Instance.SubClass[hero.SubclassName].HeroClassSecondary = Enums.HeroClass.Mage;
                }

            }
            LogDebug("Hopefully finished setting mage subclass - " + Globals.Instance.SubClass[hero.SubclassName].HeroClassSecondary);
            return true;
        }


        [HarmonyPostfix]
        [HarmonyPatch(typeof(AtOManager), "GlobalAuraCurseModificationByTraitsAndItems")]
        public static void GlobalAuraCurseModificationByTraitsAndItemsPostfix(ref AtOManager __instance, ref AuraCurseData __result, string _type, string _acId, Character _characterCaster, Character _characterTarget)
        {
            //Draw Power increases max powerful charges by 5 lose an additional 2 charges per turn         

            LogInfo("GACM");
            Character characterOfInterest = _type == "set" ? _characterTarget : _characterCaster;
            switch (_acId)
            {
                case "powerful":
                    if (IfCharacterHas(characterOfInterest, CharacterHas.Trait, trait4b, AppliesTo.ThisHero))
                    {
                        LogInfo($"Shaun has with {trait4b}");
                        __result.MaxCharges += 5;
                        __result.MaxMadnessCharges += 5;
                        __result.AuraConsumed += 2;
                        // __result.ConsumeAll=true;
                    }
                    break;
            }
        }
    }
}

