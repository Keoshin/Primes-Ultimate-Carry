using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

namespace Primes_Ultimate_Carry
{
	// ReSharper disable once InconsistentNaming
	class Champion_Draven :Champion 
	{
		public Champion_Draven()
		{
			SetSpells();
			LoadMenu();

			Game.OnGameUpdate += OnUpdate;
			Orbwalker.BeforeAttack += BeforeAttach;
			//Drawing.OnDraw += OnDraw;
			//Interrupter.OnPossibleToInterrupt += Interrupter_OnPosibleToInterrupt;
			//AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
			PluginLoaded();
		}

		private void SetSpells()
		{
			Q = new Spell(SpellSlot.Q);

			W = new Spell(SpellSlot.W);

			E = new Spell(SpellSlot.E, 1100);
			E.SetSkillshot(250f, 130f, 1400f, false, SkillshotType.SkillshotLine);

			R = new Spell(SpellSlot.R, 20000);
			R.SetSkillshot(400f, 160f, 2000f, false, SkillshotType.SkillshotLine);
		}

		private void LoadMenu()
		{
			ChampionMenu.AddSubMenu(new Menu("Combo", PUC.Player.ChampionName + "Combo"));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Combo").AddItem(new MenuItem("sep0", "====== Combo"));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Combo").AddItem(new MenuItem("useQ_Combo", "= Use Q").SetValue(true));
			//ChampionMenu.SubMenu(PUC.Player.ChampionName + "Combo").AddItem(new MenuItem("useW_Combo", "= Use W").SetValue(true));
			//ChampionMenu.SubMenu(PUC.Player.ChampionName + "Combo").AddItem(new MenuItem("useE_Combo", "= Use E").SetValue(true));
			//ChampionMenu.SubMenu(PUC.Player.ChampionName + "Combo").AddItem(new MenuItem("useR_Combo", "= Use R").SetValue(false));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Combo").AddItem(new MenuItem("sep1", "========="));

			//ChampionMenu.AddSubMenu(new Menu("Harass", PUC.Player.ChampionName + "Harass"));
			//ChampionMenu.SubMenu(PUC.Player.ChampionName + "Harass").AddItem(new MenuItem("sep0", "====== Harass"));
			//AddManaManager(ChampionMenu.SubMenu(PUC.Player.ChampionName + "Harass"), "ManaManager_Harass", 40);
			//ChampionMenu.SubMenu(PUC.Player.ChampionName + "Harass").AddItem(new MenuItem("useQ_Harass", "= Use Q").SetValue(true));
			//ChampionMenu.SubMenu(PUC.Player.ChampionName + "Harass").AddItem(new MenuItem("useW_Harass", "= Use W").SetValue(true));
			//ChampionMenu.SubMenu(PUC.Player.ChampionName + "Harass").AddItem(new MenuItem("useHarass_Auto", "= AutoHarras").SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Toggle)));
			//ChampionMenu.SubMenu(PUC.Player.ChampionName + "Harass").AddItem(new MenuItem("sep1", "========="));

			//ChampionMenu.AddSubMenu(new Menu("LaneClear", PUC.Player.ChampionName + "LaneClear"));
			//ChampionMenu.SubMenu(PUC.Player.ChampionName + "LaneClear").AddItem(new MenuItem("sep0", "====== LaneClear"));
			//AddManaManager(ChampionMenu.SubMenu(PUC.Player.ChampionName + "LaneClear"), "ManaManager_LaneClear", 20);
			//ChampionMenu.SubMenu(PUC.Player.ChampionName + "LaneClear").AddItem(new MenuItem("useQ_LaneClear", "= Use Q").SetValue(true));
			//ChampionMenu.SubMenu(PUC.Player.ChampionName + "LaneClear").AddItem(new MenuItem("useW_LaneClear", "= Use W").SetValue(true));
			//ChampionMenu.SubMenu(PUC.Player.ChampionName + "LaneClear").AddItem(new MenuItem("sep1", "========="));

			//ChampionMenu.AddSubMenu(new Menu("Lasthit", PUC.Player.ChampionName + "Lasthit"));
			//ChampionMenu.SubMenu(PUC.Player.ChampionName + "Lasthit").AddItem(new MenuItem("sep0", "====== Lasthit"));
			//ChampionMenu.SubMenu(PUC.Player.ChampionName + "Lasthit").AddItem(new MenuItem("useQ_Lasthit", "= Use Q").SetValue(true));
			//ChampionMenu.SubMenu(PUC.Player.ChampionName + "Lasthit").AddItem(new MenuItem("sep1", "========="));

			//ChampionMenu.AddSubMenu(new Menu("RunLikeHell", PUC.Player.ChampionName + "RunLikeHell"));
			//ChampionMenu.SubMenu(PUC.Player.ChampionName + "RunLikeHell").AddItem(new MenuItem("sep0", "====== RunLikeHell"));
			//ChampionMenu.SubMenu(PUC.Player.ChampionName + "RunLikeHell").AddItem(new MenuItem("useW_RunLikeHell", "= W to speed up").SetValue(true));
			//ChampionMenu.SubMenu(PUC.Player.ChampionName + "RunLikeHell").AddItem(new MenuItem("sep1", "========="));

			//ChampionMenu.AddSubMenu(new Menu("Misc", PUC.Player.ChampionName + "Misc"));
			//ChampionMenu.SubMenu(PUC.Player.ChampionName + "Misc").AddItem(new MenuItem("sep0", "====== Misc"));
			//ChampionMenu.SubMenu(PUC.Player.ChampionName + "Misc").AddItem(new MenuItem("useR_Interrupt", "= R to Interrupt").SetValue(true));
			//ChampionMenu.SubMenu(PUC.Player.ChampionName + "Misc").AddItem(new MenuItem("useW_Auto", "= Auto W if hit").SetValue(new Slider(2, 0, 5)));
			//ChampionMenu.SubMenu(PUC.Player.ChampionName + "Misc").AddItem(new MenuItem("useR_Auto", "= Auto R if hit").SetValue(new Slider(3, 0, 5)));
			//ChampionMenu.SubMenu(PUC.Player.ChampionName + "Misc").AddItem(new MenuItem("useE_Auto", "= Shield at % health").SetValue(new Slider(40, 100, 0)));
			//ChampionMenu.SubMenu(PUC.Player.ChampionName + "Misc").AddItem(new MenuItem("sep1", "========="));

			ChampionMenu.AddSubMenu(new Menu("Drawing", PUC.Player.ChampionName + "Drawing"));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Drawing").AddItem(new MenuItem("sep0", "====== Drawing"));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Drawing").AddItem(new MenuItem("Draw_Disabled", "Disable All").SetValue(false));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Drawing").AddItem(new MenuItem("Draw_E", "Draw E").SetValue(true));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Drawing").AddItem(new MenuItem("Draw_R", "Draw R").SetValue(true));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Drawing").AddItem(new MenuItem("Draw_CatchRange", "Draw CatchRange").SetValue(true));


			var dmgAfterComboItem = new MenuItem("DamageAfterCombo", "Draw damage").SetValue(true);
			Utility.HpBarDamageIndicator.DamageToUnit = GetComboDamage;
			Utility.HpBarDamageIndicator.Enabled = dmgAfterComboItem.GetValue<bool>();
			dmgAfterComboItem.ValueChanged += delegate(object sender, OnValueChangeEventArgs eventArgs)
			{
				Utility.HpBarDamageIndicator.Enabled = eventArgs.GetNewValue<bool>();
			};
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Drawing").AddItem(dmgAfterComboItem);
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Drawing").AddItem(new MenuItem("sep1", "========="));

			PUC.Menu.AddSubMenu(ChampionMenu);
		}

		private float GetComboDamage(Obj_AI_Base enemy)
		{
			var damage = 0d;
			if(Q.IsReady())
				damage += PUC.Player.GetSpellDamage(enemy, SpellSlot.Q);
			if(W.IsReady())
				damage += PUC.Player.GetSpellDamage(enemy, SpellSlot.W);
			if(E.IsReady())
				damage += PUC.Player.GetSpellDamage(enemy, SpellSlot.E);
			if(R.IsReady())
				damage += PUC.Player.GetSpellDamage(enemy, SpellSlot.R);
			damage += PUC.Player.GetAutoAttackDamage(enemy)*2;
			return (float)damage;
		}

		private void OnUpdate(EventArgs args)
		{
			switch(Orbwalker.CurrentMode)
			{
				case Orbwalker.Mode.Combo:
					if(ChampionMenu.Item("useQ_Combo").GetValue<bool>())
						CastQ();
					break;
			}
		}

		private void BeforeAttach(Orbwalker.BeforeAttackEventArgs args)
		{
			
		}

		private void CastQ()
		{
			if(!Q.IsReady())
				return;

			var target = TargetSelector.GetAATarget();
			if(target != null)
				Q.Cast();

			if(Orbwalker.CurrentMode != Orbwalker.Mode.LaneClear)
				return;
			var allMinion = MinionManager.GetMinions(PUC.Player.Position,
				Orbwalker.GetAutoAttackRangeto(), MinionTypes.All, MinionTeam.NotAlly);
			if(!allMinion.Any(minion => minion.IsValidTarget(Orbwalker.GetAutoAttackRangeto(minion))))
				return;
			Q.Cast();
		}
	}
}
