// Project:         BasicMagicRegen mod for Daggerfall Unity (http://www.dfworkshop.net)
// Copyright:       Copyright (C) 2022 Kirk.O
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Author:          Kirk.O
// Created On: 	    7/5/2020, 6:00 PM
// Last Edit:		4/13/2020, 5:30 PM
// Version:			1.20
// Special Thanks:  Hazelnut, Ralzar, Jefetienne, Kab the Bird Ranger, and Interkarma
// Modifier:

using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop.Game.Utility.ModSupport;
using DaggerfallWorkshop.Game.Utility.ModSupport.ModSettings;
using DaggerfallWorkshop.Game.Utility;
using DaggerfallWorkshop.Game.MagicAndEffects;
using UnityEngine;
using System;
using DaggerfallWorkshop.Game.Serialization;
using DaggerfallWorkshop.Game.MagicAndEffects.MagicEffects;
using DaggerfallWorkshop.Game.Formulas;
using DaggerfallWorkshop;

namespace BasicMagicRegen
{
    public class BasicMagicRegenMain : MonoBehaviour
	{
        static BasicMagicRegenMain instance;

        public static BasicMagicRegenMain Instance
        {
            get { return instance ?? (instance = FindObjectOfType<BasicMagicRegenMain>()); }
        }

        static Mod mod;

        // Options
        public static int MagicRegenType { get; set; }
        public static int FlatOrPercent { get; set; }
        public static int TickRegenFrequency { get; set; }
        public static float RegenAmountModifier { get; set; }
        public static float RestRegenModifier { get; set; }

        static PlayerEntity player = GameManager.Instance.PlayerEntity;
        static int regenAmount = 0;
        public static int OptionsMagicRoundTracker { get; set; }

        public static float ManaCounter { get; set; }
        public static int FixedUpdateCounter { get; set; }

        [Invoke(StateManager.StateTypes.Start, 0)]
        public static void Init(InitParams initParams)
        {
            mod = initParams.Mod;
            instance = new GameObject("BasicMagicRegen").AddComponent<BasicMagicRegenMain>(); // Add script to the scene.

            mod.LoadSettingsCallback = LoadSettings; // To enable use of the "live settings changes" feature in-game.

            mod.IsReady = true;
        }

        private void Start()
        {
            Debug.Log("Begin mod init: Basic Magic Regen");

            mod.LoadSettings();

            WorldTime.OnNewHour += IncreaseManaLoitering_OnNewHour;

            FormulaHelper.RegisterOverride(mod, "CalculateSpellPointRecoveryRate", (Func<PlayerEntity, int>)CalculateSpellPointRecoveryRate);

            Debug.Log("Finished mod init: Basic Magic Regen");
        }

        #region Settings

        static void LoadSettings(ModSettings modSettings, ModSettingsChange change)
        {
            MagicRegenType = mod.GetSettings().GetValue<int>("Options", "RegenType");
            FlatOrPercent = mod.GetSettings().GetValue<int>("Options", "FlatOrPercentageBased");
            TickRegenFrequency = mod.GetSettings().GetValue<int>("Options", "TickFrequency");
            RegenAmountModifier = mod.GetSettings().GetValue<float>("Options", "RegenMulti");
            RestRegenModifier = mod.GetSettings().GetValue<float>("Options", "RestMulti");

            EntityEffectBroker.OnNewMagicRound -= MagicRegenFlat_OnNewMagicRound;
            EntityEffectBroker.OnNewMagicRound -= MagicRegenPercent_OnNewMagicRound;

            if (MagicRegenType == 1 && FlatOrPercent == 0)
                EntityEffectBroker.OnNewMagicRound += MagicRegenFlat_OnNewMagicRound;
            else if (MagicRegenType == 1 && FlatOrPercent == 1)
                EntityEffectBroker.OnNewMagicRound += MagicRegenPercent_OnNewMagicRound;
        }

        #endregion

        public static int GetMagicRegenType()
        {
            return MagicRegenType;
        }

        public static int GetFlatOrPercent()
        {
            return FlatOrPercent;
        }

        public static int GetTickRegenFrequency()
        {
            return TickRegenFrequency;
        }

        public static float GetRegenAmountModifier()
        {
            return RegenAmountModifier;
        }

        public static float GetRestRegenModifier()
        {
            return RestRegenModifier;
        }

        private void FixedUpdate()
        {
            if (MagicRegenType == 1)
                return;
				
			if (player == null)
				return;

            if (SaveLoadManager.Instance.LoadInProgress)
                return;

            if (GameManager.IsGamePaused) // This also counts the rest interface.
                return;

            if (player.Career.NoRegenSpellPoints) // If player character has the trait to prevent magic regen, then stop here.
                return;

            FixedUpdateCounter++; // Increments the FixedUpdateCounter by 1 every FixedUpdate.

            if (FlatOrPercent == 0)
                FlatManaRegenRealtime();
            else
                PercentManaRegenRealtime();
        }

        public static void IncreaseManaLoitering_OnNewHour()
        {
            if (GameManager.IsGamePaused)
            {
                if (player.IsLoitering)
                {
                    //Debug.Log("Basic Magic Regen, Just Increased Your Mana Through Loitering An Hour.");
                    player.IncreaseMagicka(FindManaRecoveryRateResting(true));
                }
            }
        }

        #region Methods and Functions

        public static int CalculateSpellPointRecoveryRate(PlayerEntity playerEnt)
        {
            return FindManaRecoveryRateResting(false);
        }

        public static int FindManaRecoveryRateResting(bool loitering = false)
        {
            int minPerHour = Mathf.Max((int)Mathf.Round(player.MaxMagicka / 11), 1);
            int maxPerHour = (int)Mathf.Ceil(player.MaxMagicka / 4);
            float estSecondsPassed = 2000f;
            if (loitering)
            {
                estSecondsPassed = 600f; // Loitering gets a penalty for mana regen per time interval compared to normal resting.
                minPerHour = Mathf.Max((int)Mathf.Round(player.MaxMagicka / 18), 1);
                maxPerHour = (int)Mathf.Ceil(player.MaxMagicka / 7);
            }

            if (player.Career.NoRegenSpellPoints)
                return 0;

            float playerWillMod = player.Stats.LiveWillpower * 0.00005f * RestRegenModifier;
            float playerLuckMod = player.Stats.LiveLuck * 0.00001f * RestRegenModifier;

            //Debug.Log("Basic Magic Regen, Just Increased Your Mana Through Resting An Hour.");
            int resultValue = Mathf.Clamp(minPerHour + (int)Mathf.Round(estSecondsPassed / (1f / (playerWillMod + playerLuckMod))), minPerHour, maxPerHour);
            return (int)Mathf.Max(resultValue, 1);
        }

        private static void FlatManaRegenRealtime()
        {
            if (FixedUpdateCounter >= 80) // 50 FixedUpdates is approximately equal to 1 second since each FixedUpdate happens every 0.02 seconds, that's what Unity docs say at least.
            {
                FixedUpdateCounter = 0;

                float playerWillMod = (Mathf.Pow(1.019f, player.Stats.LiveWillpower) - 1) * RegenAmountModifier;

                if (GameManager.Instance.PlayerEffectManager.HasVampirism() && !(GameManager.Instance.PlayerEffectManager.GetRacialOverrideEffect() as VampirismEffect).IsSatiated()) // If player is a vampire and currently does NOT have their thirst sated, they will regen magic much slower.
                    playerWillMod *= 0.2f; // Vampires with unsated thirst will only regen mana at 20% of their normal amount.

                if (GameManager.Instance.PlayerEffectManager.HasReadySpell || player.CurrentMagicka >= player.MaxMagicka) // Don't allow regen "tick" to occur while player has spell ready to fire, nor if they currently have full mana.
                    return;

                ManaCounter += playerWillMod;
                if (Dice100.SuccessRoll((int)Mathf.Floor(player.Stats.LiveLuck / 2)))
                    ManaCounter *= 1.3f;

                if (ManaCounter >= 1)
                {
                    int quotient = (int)Math.Truncate(ManaCounter);

                    player.IncreaseMagicka(Mathf.Max((int)Mathf.Round(quotient), 1));
                    //Debug.LogFormat("Basic Magic Regen, Just Increased Your Mana By {0} Points. Remainder Is {1}", Mathf.Max((int)Mathf.Round(quotient), 1), ManaCounter - quotient);
                    ManaCounter -= quotient;
                }
            }
        }

        private static void PercentManaRegenRealtime()
        {
            if (FixedUpdateCounter >= 80) // 50 FixedUpdates is approximately equal to 1 second since each FixedUpdate happens every 0.02 seconds, that's what Unity docs say at least.
            {
                FixedUpdateCounter = 0;

                float playerWillMod = (Mathf.Pow(1.019f, player.Stats.LiveWillpower) - 1) * RegenAmountModifier;
                //float playerWillMod = player.Stats.LiveWillpower * 0.05f * RegenAmountModifier;

                if (GameManager.Instance.PlayerEffectManager.HasVampirism() && !(GameManager.Instance.PlayerEffectManager.GetRacialOverrideEffect() as VampirismEffect).IsSatiated()) // If player is a vampire and currently does NOT have their thirst sated, they will regen magic much slower.
                    playerWillMod *= 0.2f; // Vampires with unsated thirst will only regen mana at 20% of their normal amount.

                if (GameManager.Instance.PlayerEffectManager.HasReadySpell || player.CurrentMagicka >= player.MaxMagicka) // Don't allow regen "tick" to occur while player has spell ready to fire, nor if they currently have full mana.
                    return;

                ManaCounter += playerWillMod;
                if (Dice100.SuccessRoll((int)Mathf.Floor(player.Stats.LiveLuck / 2)))
                    ManaCounter *= 1.3f;

                if (ManaCounter >= 1)
                {
                    int quotient = (int)Math.Truncate(ManaCounter);

                    player.IncreaseMagicka(Mathf.Max((int)Mathf.Round(player.MaxMagicka * (quotient / 100f)), 1));
                    //Debug.LogFormat("Basic Magic Regen, Just Increased Your Mana By {0} Points. Remainder Is {1}", Mathf.Max((int)Mathf.Round(player.MaxMagicka * (quotient / 100f)), 1), ManaCounter - quotient);
                    ManaCounter -= quotient;
                }
            }
        }

        private static void MagicRegenFlat_OnNewMagicRound()
        {
            float playerWillMod = (player.Stats.LiveWillpower / 10f);
            int playerLuck = player.Stats.LiveLuck - 50;
            float playerLuckMod = (playerLuck * .015f) + 1;
            int willModRemain = (int)Mathf.Clamp(Mathf.Floor((playerWillMod - (float)Math.Truncate(playerWillMod)) * 100 * playerLuckMod), 0, 100);

            if (GameManager.Instance.PlayerEffectManager.HasVampirism() && !(GameManager.Instance.PlayerEffectManager.GetRacialOverrideEffect() as VampirismEffect).IsSatiated()) // If player is a vampire and currently does NOT have their thirst sated, they will regen magic much slower.
                playerWillMod *= 0.2f; // Vampires with unsated thirst will only regen mana at 20% of their normal amount.

            if (!player.Career.NoRegenSpellPoints && !GameManager.Instance.PlayerEffectManager.HasReadySpell) // Keeps magic from regenerating while player is in "Ready to cast" state, as to prevent overflow if they refund the spell.
            {
                if (player.CurrentMagicka < player.MaxMagicka) // Only allows magic regeneration to occur when the player is below their maximum mana amount.
                {
                    if (TickRegenFrequency == 1)
                    {
                        regenAmount = (int)Mathf.Floor(playerWillMod);
                        if (Dice100.SuccessRoll(willModRemain)) // Rolls the remainder of the Willpower mod value to see if "rounds" up or not, has a luck modifier.
                            regenAmount += 1;
                        regenAmount = (int)Mathf.Round(regenAmount * RegenAmountModifier);

                        if (player.MaxMagicka < (player.CurrentMagicka + regenAmount)) // Checks if amount about to be regenerated would go over their players maximum mana pool amount.
                        {
                            regenAmount -= player.CurrentMagicka + regenAmount - player.MaxMagicka; // If true, regen amount will be limited as to only regen what space the max mana pool has left to fill.
                        }
                        //Debug.LogFormat("Regenerating Mana Amount of, {0}", regenAmount);
                        player.IncreaseMagicka(regenAmount); // Actual part where mana is regenerated into the player's current mana pool amount.
                        return;
                    }
                    else
                    {
                        OptionsMagicRoundTracker++;
                        //Debug.LogFormat("OptionsMagicRoundTracker = {0}", OptionsMagicRoundTracker);
                        if (OptionsMagicRoundTracker < TickRegenFrequency)
                            return;
                        else
                            OptionsMagicRoundTracker = 0;

                        regenAmount = (int)Mathf.Floor(playerWillMod);
                        if (Dice100.SuccessRoll(willModRemain)) // Rolls the remainder of the Willpower mod value to see if "rounds" up or not, has a luck modifier.
                            regenAmount += 1;
                        regenAmount = (int)Mathf.Round(regenAmount * RegenAmountModifier);

                        if (player.MaxMagicka < (player.CurrentMagicka + regenAmount)) // Checks if amount about to be regenerated would go over their players maximum mana pool amount.
                        {
                            regenAmount -= player.CurrentMagicka + regenAmount - player.MaxMagicka; // If true, regen amount will be limited as to only regen what space the max mana pool has left to fill.
                        }
                        //Debug.LogFormat("Regenerating Mana Amount of, {0}", regenAmount);
                        player.IncreaseMagicka(regenAmount); // Actual part where mana is regenerated into the player's current mana pool amount.
                        return;
                    }
                }
            }
        }

        private static void MagicRegenPercent_OnNewMagicRound()
        {
            float playerWillMod = (player.Stats.LiveWillpower / 10f);
            int playerLuck = player.Stats.LiveLuck - 50;
            float playerLuckMod = (playerLuck * .015f) + 1;
            int willModRemain = (int)Mathf.Clamp(Mathf.Floor((playerWillMod - (float)Math.Truncate(playerWillMod)) * 100 * playerLuckMod), 0, 100);

            if (GameManager.Instance.PlayerEffectManager.HasVampirism() && !(GameManager.Instance.PlayerEffectManager.GetRacialOverrideEffect() as VampirismEffect).IsSatiated()) // If player is a vampire and currently does NOT have their thirst sated, they will regen magic much slower.
                playerWillMod *= 0.2f; // Vampires with unsated thirst will only regen mana at 20% of their normal amount.

            if (!player.Career.NoRegenSpellPoints && !GameManager.Instance.PlayerEffectManager.HasReadySpell) // Keeps magic from regenerating while player is in "Ready to cast" state, as to prevent overflow if they refund the spell.
            {
                if (player.CurrentMagicka < player.MaxMagicka) // Only allows magic regeneration to occur when the player is below their maximum mana amount.
                {
                    if (TickRegenFrequency == 1)
                    {
                        int addRemainder = 0;
                        if (Dice100.SuccessRoll(willModRemain)) // Rolls the remainder of the Willpower mod value to see if "rounds" up or not, has a luck modifier.
                            addRemainder = 1;
                        regenAmount = Mathf.Max((int)Mathf.Round(((int)playerWillMod + addRemainder) * (player.MaxMagicka / 100f)), 1);
                        regenAmount = (int)Mathf.Round(regenAmount * RegenAmountModifier);

                        if (player.MaxMagicka < (player.CurrentMagicka + regenAmount)) // Checks if amount about to be regenerated would go over their players maximum mana pool amount.
                        {
                            regenAmount -= player.CurrentMagicka + regenAmount - player.MaxMagicka; // If true, regen amount will be limited as to only regen what space the max mana pool has left to fill.
                        }
                        //Debug.LogFormat("Regenerating Mana Amount of, {0}", regenAmount);
                        player.IncreaseMagicka(regenAmount); // Actual part where mana is regenerated into the player's current mana pool amount.
                        return;
                    }
                    else
                    {
                        OptionsMagicRoundTracker++;
                        //Debug.LogFormat("OptionsMagicRoundTracker = {0}", OptionsMagicRoundTracker);
                        if (OptionsMagicRoundTracker < TickRegenFrequency)
                            return;
                        else
                            OptionsMagicRoundTracker = 0;

                        int addRemainder = 0;
                        if (Dice100.SuccessRoll(willModRemain)) // Rolls the remainder of the Willpower mod value to see if "rounds" up or not, has a luck modifier.
                            addRemainder = 1;
                        regenAmount = Mathf.Max((int)Mathf.Round(((int)playerWillMod + addRemainder) * (player.MaxMagicka / 100f)), 1);
                        regenAmount = (int)Mathf.Round(regenAmount * RegenAmountModifier);

                        if (player.MaxMagicka < (player.CurrentMagicka + regenAmount)) // Checks if amount about to be regenerated would go over their players maximum mana pool amount.
                        {
                            regenAmount -= player.CurrentMagicka + regenAmount - player.MaxMagicka; // If true, regen amount will be limited as to only regen what space the max mana pool has left to fill.
                        }
                        //Debug.LogFormat("Regenerating Mana Amount of, {0}", regenAmount);
                        player.IncreaseMagicka(regenAmount); // Actual part where mana is regenerated into the player's current mana pool amount.
                        return;
                    }
                }
            }
        }

        #endregion

    }
}