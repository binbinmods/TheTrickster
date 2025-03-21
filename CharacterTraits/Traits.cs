using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using Obeliskial_Content;
using static TheMagician.CustomFunctions;
using static TheMagician.Plugin;
using UnityEngine;
using System.Collections;

namespace TheMagician
{


    [HarmonyPatch]
    internal class Traits
    {
        // list of your trait IDs
        public static string heroName = "shaun eello";

        public static string subclassname = "trickster";

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
        public static string trait2a = "trickstertrickupyoursleeve";
        public static string trait2b = "tricksterlearnrealmagic";
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
                LogDebug(traitName);
                Character frontHero = teamHero.First();
                LogDebug($"Trait: {traitName} - Front hero: {frontHero.SourceName}");
                frontHero.SetAuraTrait(_character, "evasion", 1);
                LogDebug($"Trait: {traitName} - Evasion set");
                _character.HeroItem.ScrollCombatText(Texts.Instance.GetText("traits_" + traitName), Enums.CombatScrollEffectType.Trait);
            }


            else if (_trait == trait2a)
            { // When you play your first Small Weapon each turn, draw a card and reduce its cost by 2.
                string traitName = _trait;
                // cardsPlayedPerTurn += 1;
                LogDebug(traitName);
                if (CanIncrementTraitActivations(_trait) && _castedCard.HasCardType(Enums.CardType.Small_Weapon))
                {
                    LogDebug($"Trait: {traitName} Casted Card - {_castedCard.Id}. IsSmallWeap - {_castedCard.HasCardType(Enums.CardType.Small_Weapon)}");

                    PlayCardForFree("tricksterspecialdraw");

                    MatchManager matchManager = MatchManager.Instance;
                    if(matchManager!=null)
                    {
                        LogDebug("Decrement globalVanishCardsNum - draw");
                        int globalVanishCardsNum = Traverse.Create(matchManager).Field("GlobalVanishCardsNum").GetValue<int>();
                        globalVanishCardsNum -=1;
                        Traverse.Create(matchManager).Field("GlobalVanishCardsNum").SetValue(globalVanishCardsNum);
                    }

                    // DrawCards(1);
                    // Globals.Instance.WaitForSeconds(1.5f);

                    // CardData cardToReduce = GetRightmostCard(heroHand);
                    // if (cardToReduce == null)
                    // {
                    //     return;
                    // }
                    // LogDebug($"Trait: {traitName} Reducing Card Cost. Card to Reduce-{cardToReduce.Id}");

                    // ReduceCardCost(ref cardToReduce, _character, 2, isPermanent: true);
                    IncrementTraitActivations(_trait);
                    _character.HeroItem.ScrollCombatText(Texts.Instance.GetText("traits_" + traitName), Enums.CombatScrollEffectType.Trait);

                }
            }

            else if (_trait == trait2b)
            { // Subclass Mage. At the start of your turn, reduce the cost of your highest cost Skill, Spell and Book by 1.
                string traitName = _trait;
                LogDebug(traitName);

                CardData skill = GetRandomHighestCostCard(Enums.CardType.Skill, heroHand);
                CardData spell = GetRandomHighestCostCard(Enums.CardType.Spell, heroHand);
                CardData book = GetRandomHighestCostCard(Enums.CardType.Book);

                ReduceCardCost(ref skill, _character, 1, isPermanent: false);
                ReduceCardCost(ref spell, _character, 1, isPermanent: false);
                ReduceCardCost(ref book, _character, 1, isPermanent: false);

                _character.HeroItem.ScrollCombatText(Texts.Instance.GetText("traits_" + traitName), Enums.CombatScrollEffectType.Trait);


            }

            else if (_trait == trait4a)
            { // +2 Stealth, +1 Evasion. After you play a card, gain 1 Stealth if you had none.
                string traitName = _trait;
                LogDebug(traitName);
                LogDebug($"{traitName} nStealth = {_character.GetAuraCharges("stealth")}");
                if (_castedCard != null && _character.GetAuraCharges("stealth") <= 0 && _castedCard.Id!="tricksterspecialstealth"&& _castedCard.Id!="tricksterspecialdraw" )
                {
                    LogDebug($"Trait: {traitName} Gaining Stealth - {_character.GetAuraCharges("stealth")}");
                    
                    PlayCardForFree("tricksterspecialstealth");                        

                    MatchManager matchManager = MatchManager.Instance;
                    if(matchManager!=null)
                    {
                        LogDebug("Decrement globalVanishCardsNum - stealth");
                        int globalVanishCardsNum = Traverse.Create(matchManager).Field("GlobalVanishCardsNum").GetValue<int>();
                        globalVanishCardsNum -=1;
                        Traverse.Create(matchManager).Field("GlobalVanishCardsNum").SetValue(globalVanishCardsNum);
                    }
                    // _character.SetAuraTrait(_character, "stealth", 1);
                    _character.HeroItem.ScrollCombatText(Texts.Instance.GetText("traits_" + traitName), Enums.CombatScrollEffectType.Trait);
                }
            }

            else if (_trait == trait4b)
            { // +1 Inspire. At the start of your turn, you lose 2 more Powerful. When you draw a card, gain 1 Powerful. Max Powerful charges +5
                string traitName = _trait;
                // LogDebug(traitName);
                // if (IsLivingHero(_character))
                // {
                //     LogDebug($"Trait: {traitName} Gaining Powerful - {_character.GetAuraCharges("powerful")}");

                //     _character.SetAuraTrait(_character, "powerful", 1);
                //     _character.HeroItem.ScrollCombatText(Texts.Instance.GetText("traits_" + traitName), Enums.CombatScrollEffectType.Trait);
                // }
            }
        }


        [HarmonyPostfix]
        [HarmonyPatch(typeof(MatchManager), nameof(MatchManager.NewCard))]
        public static void NewCardPostfix(MatchManager __instance, int numCards, Enums.CardFrom fromPlace, string comingFromCardId = "")
        {
            Hero activeHero = __instance.GetHeroHeroActive();
            if (IsLivingHero(activeHero) && activeHero.HaveTrait(trait4b))
            {
                LogDebug($"Trait: {trait4a} Gaining Powerful - {activeHero.GetAuraCharges("powerful")}");

                activeHero.SetAura(activeHero, GetAuraCurseData("powerful"), 1, fromTrait: true, useCharacterMods: true);
                activeHero.HeroItem.ScrollCombatText(Texts.Instance.GetText("traits_" + trait4b), Enums.CombatScrollEffectType.Trait);
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

        public static void UnsetMage(AtOManager __instance)
        {
            if (__instance==null){return;}
            if (!Globals.Instance.SubClass.ContainsKey("trickster"))
            {
                return;
            }
            Hero[] teamAtO = Traverse.Create(__instance).Field("teamAtO").GetValue<Hero[]>();
            if (teamAtO == null) { LogError("null teamAtO"); return; }
            int tricksterIndex = -1;
            for (int _heroIndex = 0; _heroIndex < 4; ++_heroIndex)
            {
                if (teamAtO[_heroIndex].SubclassName == "trickster")
                {
                    tricksterIndex = _heroIndex;
                }
            }

            if (tricksterIndex == -1) { LogDebug("trickster not found"); return; }

            LogDebug("Unsetting mage subclass inside conditions");

            Globals.Instance.SubClass["trickster"].HeroClassSecondary = teamAtO[tricksterIndex].HaveTrait("tricksterlearnrealmagic") ? Enums.HeroClass.Mage : Enums.HeroClass.None;
        }


        [HarmonyPostfix]
        [HarmonyPatch(typeof(AtOManager), nameof(AtOManager.BeginAdventure))]
        public static void BeginAdventurePostfix(AtOManager __instance)
        {
            UnsetMage(__instance);
        }
        [HarmonyPostfix]
        // [HarmonyPatch(typeof(HeroSelectionManager), nameof(HeroSelectionManager.Instantiate))]
        [HarmonyPatch(typeof(HeroSelectionManager), "Awake")]
        public static void HeroSelectionManagerInstantiatePostfix(HeroSelectionManager __instance)
        {
            UnsetMage(AtOManager.Instance);
        }

        // [HarmonyPostfix]
        // [HarmonyPatch(typeof(AtOManager), nameof(AtOManager.SaveGame))]
        // public static void SaveGamePostfix(AtOManager __instance)
        // {
        //     UnsetMage(__instance);
        // }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(AtOManager), nameof(AtOManager.LoadGame))]
        public static void LoadGamePostfix(AtOManager __instance)
        {
            // Unsets the Subclass = mage when you reload the game and don't have the trait.

            UnsetMage(__instance);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(AtOManager), "HeroLevelUp")]
        public static bool HeroLevelUpPrefix(ref AtOManager __instance, int heroIndex, string traitId)
        {
            LogDebug("HeroLevelUpPrefix");
            Hero[] teamAtO = Traverse.Create(__instance).Field("teamAtO").GetValue<Hero[]>();
            if (teamAtO == null) { LogError("null teamAtO"); return true; }
            Hero hero = teamAtO[heroIndex];
            LogDebug("Level up before conditions for subclass " + hero.SubclassName + " trait id " + traitId);

            string traitOfInterest = trait2b; // Gain Mage Subclass
            if (hero.SubclassName == subclassname && traitId == trait2b)
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
                        LogInfo($"Shaun has {trait4b}");
                        __result.MaxCharges += 5;
                        __result.MaxMadnessCharges += 5;
                        __result.AuraConsumed += 2;
                        // __result.ConsumeAll=true;
                    }
                    break;
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(CharacterItem), nameof(CharacterItem.fOnMouseUp))]
        public static void fOnMouseUpPrefix()
        {
            LogDebug("fOnMouseUpPrefix - PRE");

            if(MatchManager.Instance.justCasted && MatchManager.Instance!=null)
            {
                LogDebug("fOnMouseUpPrefix - POST");
                Character _character = MatchManager.Instance.GetHeroHeroActive();

                if (_character.GetAuraCharges("stealth") <= 0)
                {
                    LogDebug($"Trait: {trait4a} Gaining Stealth - {_character.GetAuraCharges("stealth")}");
                    _character.SetAuraTrait(_character, "stealth", 1);
                    _character.HeroItem.ScrollCombatText(Texts.Instance.GetText("traits_" + trait4a), Enums.CombatScrollEffectType.Trait);
                }

                LogDebug("fOnMouseUpPrefix - POST Stealth");

            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MatchManager), nameof(MatchManager.JustCastedCo))]
        public static IEnumerator JustCastedCoWrapper(IEnumerator result)
        {
            LogDebug("JustCastedCoWrapper - PRE");

            // Run original enumerator code
            while (result.MoveNext())
            {   
                LogDebug("JustCastedCoWrapper - Inside1");         
                yield return result.Current;
                LogDebug("JustCastedCoWrapper - Inside2");

            }
            // Run your postfix

            LogDebug("JustCastedCoWrapper - POST");
            Character _character = MatchManager.Instance.GetHeroHeroActive();

            if (_character.GetAuraCharges("stealth") <= 0)
            {
                LogDebug($"Trait: {trait4a} Gaining Stealth - {_character.GetAuraCharges("stealth")}");
                _character.SetAuraTrait(_character, "stealth", 1);
                _character.HeroItem.ScrollCombatText(Texts.Instance.GetText("traits_" + trait4a), Enums.CombatScrollEffectType.Trait);
            }

            LogDebug("JustCastedCoWrapper - POST Stealth");


        }
    }
}

