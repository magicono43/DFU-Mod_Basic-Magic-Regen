using DaggerfallConnect;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Formulas;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop.Game.Items;
using DaggerfallWorkshop.Game.Utility.ModSupport;
using DaggerfallWorkshop.Game.Utility.ModSupport.ModSettings;
using DaggerfallWorkshop.Game.Utility;
using DaggerfallWorkshop.Utility;
using DaggerfallWorkshop.Game.UserInterfaceWindows;
using DaggerfallWorkshop.Game.MagicAndEffects;
using DaggerfallWorkshop.Game.MagicAndEffects.MagicEffects;
using UnityEngine;
using System;
using System.Collections.Generic;

namespace BasicMagicRegen
{
	public class BasicMagicRegen : MonoBehaviour
	{
		static Mod mod;

        static PlayerEntity player = GameManager.Instance.PlayerEntity;
        static int regenAmount = 0;

        [Invoke(StateManager.StateTypes.Start, 0)]
        public static void Init(InitParams initParams)
        {
            mod = initParams.Mod;
            var go = new GameObject("BasicMagicRegen");
            go.AddComponent<BasicMagicRegen>();
			EntityEffectBroker.OnNewMagicRound += MagicRegen_OnNewMagicRound;
        }
		
		void Awake() // Needs to be trimmed down, but should be ok for testing phase.
        {	
            /*ModSettings settings = mod.GetSettings();
            Mod roleplayRealism = ModManager.Instance.GetMod("RoleplayRealism");
            Mod meanerMonsters = ModManager.Instance.GetMod("Meaner Monsters");
            bool equipmentDamageEnhanced = settings.GetBool("Modules", "equipmentDamageEnhanced");
			bool fixedStrengthDamageModifier = settings.GetBool("Modules", "fixedStrengthDamageModifier");
			bool armorHitFormulaRedone = settings.GetBool("Modules", "armorHitFormulaRedone");
			bool criticalStrikesIncreaseDamage = settings.GetBool("Modules", "criticalStrikesIncreaseDamage");
            bool rolePlayRealismArcheryModule = false;
            bool ralzarMeanerMonstersEdit = false;
            if (roleplayRealism != null)
            {
                ModSettings rolePlayRealismSettings = roleplayRealism.GetSettings();
                rolePlayRealismArcheryModule = rolePlayRealismSettings.GetBool("Modules", "advancedArchery");
            }
            if (meanerMonsters != null)
            {
                ralzarMeanerMonstersEdit = true;
            }*/

            InitMod();

            mod.IsReady = true;
        }
		
		#region InitMod and Settings
		
		private static void InitMod()
        {
            Debug.Log("Begin mod init: BasicMagicRegen");
			
			Debug.Log("Finished mod init: BasicMagicRegen");
		}
		
		#endregion
		
		// Look into making a super basic "OnNewMagicRound" based "live" magic regeneration function and see if it works out.
		
		private static void MagicRegen_OnNewMagicRound()
		{
            float playerWillMod = (player.Stats.LiveWillpower / 10f);
            int playerLuck = player.Stats.LiveLuck - 50;
            float playerLuckMod = (playerLuck * .015f) + 1;
            int willModRemain = (int)Mathf.Clamp(Mathf.Floor((playerWillMod - (float)Math.Truncate(playerWillMod)) * 100 * playerLuckMod), 0, 100);

            if (!player.Career.NoRegenSpellPoints && !GameManager.Instance.PlayerEffectManager.HasReadySpell) // Keeps magic from regenerating while player is in "Ready to cast" state, as to prevent overflow if they refund the spell.
            {
                if (player.CurrentMagicka < player.MaxMagicka) // Only allows magic regeneration to occur when the player is below their maximum mana amount.
                {
                    regenAmount = (int)Mathf.Floor(playerWillMod);
                    if (Dice100.SuccessRoll(willModRemain)) // Rolls the remainder of the Willpower mod value to see if "rounds" up or not, has a luck modifier.
                        regenAmount += 1;

                    if (player.MaxMagicka < (player.CurrentMagicka + regenAmount)) // Checks if amount about to be regenerated would go over their players maximum mana pool amount.
                    {
                        regenAmount -= player.CurrentMagicka + regenAmount - player.MaxMagicka; // If true, regen amount will be limited as to only regen what space the max mana pool has left to fill.
                    }
                    Debug.LogFormat("Regenerating Mana Amount of, {0}", regenAmount);
                    player.IncreaseMagicka(regenAmount); // Actual part where mana is regenerated into the player's current mana pool amount.
                }
            }
        }
	}
}