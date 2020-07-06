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
            ModSettings settings = mod.GetSettings();
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
            }

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
			
		}
	}
}