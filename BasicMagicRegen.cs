// Project:         PhysicalCombatAndArmorOverhaul mod for Daggerfall Unity (http://www.dfworkshop.net)
// Copyright:       Copyright (C) 2020 Kirk.O
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Author:          Kirk.O
// Created On: 	    7/5/2020, 6:00 PM
// Last Edit:		7/8/2020, 1:05 AM
// Version:			1.00
// Special Thanks:  Hazelnut, Ralzar, Jefetienne, and Interkarma
// Modifier:

using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop.Game.Utility.ModSupport;
using DaggerfallWorkshop.Game.Utility.ModSupport.ModSettings;
using DaggerfallWorkshop.Game.Utility;
using DaggerfallWorkshop.Game.MagicAndEffects;
using UnityEngine;
using System;

namespace BasicMagicRegen
{
    public class BasicMagicRegen : MonoBehaviour
	{
		static Mod mod;

        // Options
        static int magicRegenType = 0;
        static int tickRegenFrequency = 1;
        static float regenAmountModifier = 1;

        static PlayerEntity player = GameManager.Instance.PlayerEntity;
        static int regenAmount = 0;
        public static int RestMagicRoundTracker { get; set; }
        public static int OptionsMagicRoundTracker { get; set; }

        [Invoke(StateManager.StateTypes.Start, 0)]
        public static void Init(InitParams initParams)
        {
            mod = initParams.Mod;
            var go = new GameObject("BasicMagicRegen");
            go.AddComponent<BasicMagicRegen>();
        }
		
		void Awake()
        {
            InitMod();

            mod.IsReady = true;
        }
		
		#region InitMod and Settings
		
		private static void InitMod()
        {
            Debug.Log("Begin mod init: BasicMagicRegen");

            ModSettings settings = mod.GetSettings();
            magicRegenType = settings.GetInt("Options", "MagicRegenType");
            tickRegenFrequency = settings.GetInt("Options", "TickRegenFrequency");
            regenAmountModifier = settings.GetFloat("Options", "RegenAmountModifier");

            if (magicRegenType == 0)
                EntityEffectBroker.OnNewMagicRound += MagicRegenFlat_OnNewMagicRound;
            else if (magicRegenType == 1)
                EntityEffectBroker.OnNewMagicRound += MagicRegenPercent_OnNewMagicRound;

            Debug.Log("Finished mod init: BasicMagicRegen");
		}

        #endregion

        #region Methods and Functions

        private static void MagicRegenFlat_OnNewMagicRound()
		{
            float playerWillMod = (player.Stats.LiveWillpower / 10f);
            int playerLuck = player.Stats.LiveLuck - 50;
            float playerLuckMod = (playerLuck * .015f) + 1;
            int willModRemain = (int)Mathf.Clamp(Mathf.Floor((playerWillMod - (float)Math.Truncate(playerWillMod)) * 100 * playerLuckMod), 0, 100);

            if (!player.Career.NoRegenSpellPoints && !GameManager.Instance.PlayerEffectManager.HasReadySpell) // Keeps magic from regenerating while player is in "Ready to cast" state, as to prevent overflow if they refund the spell.
            {
                if (player.CurrentMagicka < player.MaxMagicka) // Only allows magic regeneration to occur when the player is below their maximum mana amount.
                {
                    if (tickRegenFrequency == 1)
                    {
                        if (player.IsResting) // Changes behavior slightly when player is in "resting" mode of any kind.
                        {
                            RestMagicRoundTracker++;
                            //Debug.LogFormat("RestMagicRoundTracker = {0}", RestMagicRoundTracker);
                            if (RestMagicRoundTracker < 9) // Not the most elegant solution out there, but it seems to work for this purpose fairly well. While resting only ticks regen every 9 rounds counted.
                                return;
                            else
                                RestMagicRoundTracker = 0;
                        }

                        regenAmount = (int)Mathf.Floor(playerWillMod);
                        if (Dice100.SuccessRoll(willModRemain)) // Rolls the remainder of the Willpower mod value to see if "rounds" up or not, has a luck modifier.
                            regenAmount += 1;
                        regenAmount = (int)Mathf.Round(regenAmount * regenAmountModifier);

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
                        if (OptionsMagicRoundTracker < tickRegenFrequency)
                            return;
                        else
                            OptionsMagicRoundTracker = 0;

                        if (player.IsResting) // Changes behavior slightly when player is in "resting" mode of any kind.
                        {
                            RestMagicRoundTracker++;
                            //Debug.LogFormat("RestMagicRoundTracker = {0}", RestMagicRoundTracker);
                            if (RestMagicRoundTracker < 9) // Not the most elegant solution out there, but it seems to work for this purpose fairly well. While resting only ticks regen every 9 rounds counted.
                                return;
                            else
                                RestMagicRoundTracker = 0;
                        }

                        regenAmount = (int)Mathf.Floor(playerWillMod);
                        if (Dice100.SuccessRoll(willModRemain)) // Rolls the remainder of the Willpower mod value to see if "rounds" up or not, has a luck modifier.
                            regenAmount += 1;
                        regenAmount = (int)Mathf.Round(regenAmount * regenAmountModifier);

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

            if (!player.Career.NoRegenSpellPoints && !GameManager.Instance.PlayerEffectManager.HasReadySpell) // Keeps magic from regenerating while player is in "Ready to cast" state, as to prevent overflow if they refund the spell.
            {
                if (player.CurrentMagicka < player.MaxMagicka) // Only allows magic regeneration to occur when the player is below their maximum mana amount.
                {
                    if (tickRegenFrequency == 1)
                    {
                        if (player.IsResting) // Changes behavior slightly when player is in "resting" mode of any kind.
                        {
                            RestMagicRoundTracker++;
                            //Debug.LogFormat("RestMagicRoundTracker = {0}", RestMagicRoundTracker);
                            if (RestMagicRoundTracker < 17) // Not the most elegant solution out there, but it seems to work for this purpose fairly well. While resting only ticks regen every 17 rounds counted.
                                return;
                            else
                                RestMagicRoundTracker = 0;
                        }

                        int addRemainder = 0;
                        if (Dice100.SuccessRoll(willModRemain)) // Rolls the remainder of the Willpower mod value to see if "rounds" up or not, has a luck modifier.
                            addRemainder = 1;
                        regenAmount = Mathf.Max((int)Mathf.Round(((int)playerWillMod + addRemainder) * (player.MaxMagicka / 100f)), 1);
                        regenAmount = (int)Mathf.Round(regenAmount * regenAmountModifier);

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
                        if (OptionsMagicRoundTracker < tickRegenFrequency)
                            return;
                        else
                            OptionsMagicRoundTracker = 0;

                        if (player.IsResting) // Changes behavior slightly when player is in "resting" mode of any kind.
                        {
                            RestMagicRoundTracker++;
                            //Debug.LogFormat("RestMagicRoundTracker = {0}", RestMagicRoundTracker);
                            if (RestMagicRoundTracker < 17) // Not the most elegant solution out there, but it seems to work for this purpose fairly well. While resting only ticks regen every 17 rounds counted.
                                return;
                            else
                                RestMagicRoundTracker = 0;
                        }

                        int addRemainder = 0;
                        if (Dice100.SuccessRoll(willModRemain)) // Rolls the remainder of the Willpower mod value to see if "rounds" up or not, has a luck modifier.
                            addRemainder = 1;
                        regenAmount = Mathf.Max((int)Mathf.Round(((int)playerWillMod + addRemainder) * (player.MaxMagicka / 100f)), 1);
                        regenAmount = (int)Mathf.Round(regenAmount * regenAmountModifier);

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