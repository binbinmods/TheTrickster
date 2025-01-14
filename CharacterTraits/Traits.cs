using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using Obeliskial_Content;
using static TheMagician.TraitFunctions;

namespace TheMagician
{

    
    [HarmonyPatch]
    internal class Traits
    {
        // list of your trait IDs
        public static string heroName = "jack";

        public static string subclassname = "magician";

        public static string debugBase = "Binbin - Testing " + heroName + " ";

        public static string[] simpleTraitList = ["trickery","practice","study","trickupyoursleeves","learnrealmagic","secretpocket","lightningfast","distractingact","drawpower"];
        public static string[] myTraitList = (string[])simpleTraitList.Select(trait=>heroName+trait); // Needs testing

        public static int cardsPlayedPerTurn = 0;
        //public static int level5MaxActivations = 3;

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
            
            
            string trait0 = heroName+simpleTraitList[0];
            string trait2a = heroName+simpleTraitList[3];
            string trait2b = heroName+simpleTraitList[4];
            string trait4a = heroName+simpleTraitList[7];
            string trait4b = heroName+simpleTraitList[8];

            // activate traits
            // I don't know how to set the combatLog text I need to do that for all of the traits
            if (_trait == trait0)
            { // TODO  Front hero starts with 1 Evasion
                string traitName = _trait;
                Character frontHero = GetFrontCharacter(teamHero);
                frontHero.SetAuraTrait(_character, "evasion", 1);
                _character.HeroItem.ScrollCombatText(Texts.Instance.GetText("traits_" + traitName), Enums.CombatScrollEffectType.Trait);
            }

                    
            else if (_trait == trait2a)
            { // After you play 4 cards, Gain 1 energy and draw 1 card. [3x/turn]
                string traitName = _trait;
                cardsPlayedPerTurn +=1;
                if (cardsPlayedPerTurn >=3 && CanIncrementActivations(traitName,traitData)){
                    IncrementActivations(traitName);
                    _character.ModifyEnergy(1, true);
                    DrawCards(1);
                    DisplayRemainingCharges(ref _character,traitName,traitData,"energy");
                    PlaySoundEffect(_character,"energy");
                    cardsPlayedPerTurn=0;
                }                
            }

            else if (_trait == trait2b)
            { // TODO The first time you play a Lightning spell each turn, gain 1 Inspire 1 Evasion.
                string traitName = _trait;
                if (_castedCard!=null&&_castedCard){

                    if (!((UnityEngine.Object)MatchManager.Instance != (UnityEngine.Object)null) || !((UnityEngine.Object)_castedCard != (UnityEngine.Object)null))
                        return;

                    if (CanIncrementActivations(traitName,traitData)){
                        WhenYouPlayXGainY(Enums.CardType.Lightning_Spell,"inspire",1, _castedCard,ref _character,traitName);
                        WhenYouPlayXGainY(Enums.CardType.Lightning_Spell,"evasion",1, _castedCard,ref _character,traitName);
                        IncrementActivations(traitName);
                    }
                }
            }

            else if (_trait == trait4a)
            { // TODO After you play a card, gain 2 Stealth if you had none.
                string traitName = _trait;
                if (_castedCard!=null&&  !_character.HasEffect("stealth")){
                    _character.SetAuraTrait(_character, "stealth", 2);
                    _character.HeroItem.ScrollCombatText(Texts.Instance.GetText("traits_" + traitName), Enums.CombatScrollEffectType.Trait);
                }   
            }

            else if (_trait == trait4b)
            { // TODO when you draw a card, gain 1 Powerful, +5 Max powerful charges on Magician, lose all powerful charges at end of turn
                string traitName = _trait;
                if (_character.IsHero && _character != null && _character.Alive){
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

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Character), "SetEvent")]
        public static void SetEventPrefix(ref Character __instance, ref Enums.EventActivation theEvent, Character target = null)
        {
            string traitOfInterest = myTraitList[3]; //trickupyoursleeves
            if (theEvent == Enums.EventActivation.BeginTurn && __instance.IsHero && __instance.HaveTrait(traitOfInterest)){
                cardsPlayedPerTurn=0;
                // Plugin.Log.LogInfo("Binbin - PestilyBiohealer - Reset Activation Counter: "+ level5ActivationCounter);
            }
            

            
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(AtOManager), "HeroLevelUp")]
        public static bool HeroLevelUpPrefix(ref AtOManager __instance, Hero[] ___teamAtO, int heroIndex, string traitId)
        {
            Hero hero = ___teamAtO[heroIndex];
            Plugin.Log.LogDebug(debugBase + "Level up before conditions for subclass "+ hero.SubclassName + " trait id " + traitId);
            
            string traitOfInterest = myTraitList[4]; //Learn real magic
            if (hero.AssignTrait(traitId))
            {
                TraitData traitData = Globals.Instance.GetTraitData(traitId);
                if ((UnityEngine.Object) traitData != (UnityEngine.Object) null && traitId==traitOfInterest)
                {
                    Plugin.Log.LogDebug(debugBase + "Learn Real Magic inside conditions");
                    Globals.Instance.SubClass[hero.SubclassName].HeroClassSecondary=Enums.HeroClass.Mage;
                }
                
            }
            return true;
        }


        [HarmonyPostfix]
        [HarmonyPatch(typeof(AtOManager),"GlobalAuraCurseModificationByTraitsAndItems")]
        public static void GlobalAuraCurseModificationByTraitsAndItemsPostfix(ref AtOManager __instance, ref AuraCurseData __result, string _type, string _acId, Character _characterCaster, Character _characterTarget){
            //Draw Power increases max powerful charges by 5 lose an additional 3 charges per turn         

            string traitToUpdate =myTraitList[8]; 
            if(_acId=="powerful")
            {
                if(_type=="set")
                {
                    if (_characterTarget != null && __instance.CharacterHaveTrait(_characterTarget.SubclassName, traitToUpdate))
                    {   
                        Plugin.Log.LogInfo(debugBase + " Set Powerful with " + traitToUpdate);
                        __result.MaxCharges += 5;
                        __result.MaxMadnessCharges += 5;
                        __result.AuraConsumed+=3;
                        // __result.ConsumeAll=true;
                    }
                }                
            }
        }
    }
}
