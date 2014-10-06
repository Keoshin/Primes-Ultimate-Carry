﻿using System;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

namespace Primes_Ultimate_Carry
{
	// ReSharper disable once InconsistentNaming
	class Champion_Ezreal :Champion 
	{
		public Champion_Ezreal()
		{
			SetSpells();
			LoadMenu();

			Game.OnGameUpdate += OnUpdate;
			Drawing.OnDraw += OnDraw;

			PluginLoaded();
		}

		private void SetSpells()
		{
			Q = new Spell(SpellSlot.Q, 1200);
			Q.SetSkillshot(0.25f, 70f, 2000f, true, SkillshotType.SkillshotLine);

			W = new Spell(SpellSlot.W, 1000);
			W.SetSkillshot(0.25f, 80f, 2000f, false, SkillshotType.SkillshotLine);

			E = new Spell(SpellSlot.E, 475);
			E.SetSkillshot(0.25f, 80f, 1600f, false, SkillshotType.SkillshotCircle);

			R = new Spell(SpellSlot.R, 3000);
			R.SetSkillshot(1f, 160f, 2000f, false, SkillshotType.SkillshotLine);
		}

		private void LoadMenu()
		{
			ChampionMenu.AddSubMenu(new Menu("Combo", PUC.Player.ChampionName + "Combo"));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Combo").AddItem(new MenuItem("sep0", "====== Combo"));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Combo").AddItem(new MenuItem("useQ_Combo", "= Use Q").SetValue(true));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Combo").AddItem(new MenuItem("useW_Combo", "= Use W").SetValue(true));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Combo").AddItem(new MenuItem("useR_Combo", "= Use R").SetValue(true));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Combo").AddItem(new MenuItem("useR_Combo_minRange", "= R min Range").SetValue(new Slider(500, 900, 0)));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Combo").AddItem(new MenuItem("useR_Combo_maxRange", "= R max Range").SetValue(new Slider(1500, 2000, 0)));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Combo").AddItem(new MenuItem("useR_Combo_minHit", "= R min Hit").SetValue(new Slider(2, 5, 1)));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Combo").AddItem(new MenuItem("sep1", "========="));

			ChampionMenu.AddSubMenu(new Menu("Harass", PUC.Player.ChampionName + "Harass"));
			AddManaManager(ChampionMenu.SubMenu(PUC.Player.ChampionName + "Harass"), "ManaManager_Harass", 40);
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Harass").AddItem(new MenuItem("sep0", "====== Harass"));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Harass").AddItem(new MenuItem("useQ_Harass", "= Use Q").SetValue(true));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Harass").AddItem(new MenuItem("useW_Harass", "= Use W").SetValue(true));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Harass").AddItem(new MenuItem("sep1", "========="));

			ChampionMenu.AddSubMenu(new Menu("LaneClear", PUC.Player.ChampionName + "LaneClear"));
			AddManaManager(ChampionMenu.SubMenu(PUC.Player.ChampionName + "LaneClear"), "ManaManager_LaneClear", 20);
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "LaneClear").AddItem(new MenuItem("sep0", "====== LaneClear"));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "LaneClear").AddItem(new MenuItem("useQ_LaneClear_Minion", "= Use Q Minion").SetValue(true));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "LaneClear").AddItem(new MenuItem("useQ_LaneClear_Enemy", "= Use Q Enemy").SetValue(true));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "LaneClear").AddItem(new MenuItem("useE_LaneClear", "= Use E").SetValue(true));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "LaneClear").AddItem(new MenuItem("sep1", "========="));

			ChampionMenu.AddSubMenu(new Menu("Lasthit", PUC.Player.ChampionName + "Lasthit"));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Lasthit").AddItem(new MenuItem("sep0", "====== Lasthit"));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Lasthit").AddItem(new MenuItem("useQ_Lasthit", "= Use Q").SetValue(true));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Lasthit").AddItem(new MenuItem("sep1", "========="));

			ChampionMenu.AddSubMenu(new Menu("RunLikeHell", PUC.Player.ChampionName + "RunLikeHell"));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "RunLikeHell").AddItem(new MenuItem("sep0", "====== RunLikeHell"));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "RunLikeHell").AddItem(new MenuItem("useE_RunLikeHell", "= E to Mouse").SetValue(true));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "RunLikeHell").AddItem(new MenuItem("sep1", "========="));

			ChampionMenu.AddSubMenu(new Menu("Misc", PUC.Player.ChampionName + "Misc"));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Misc").AddItem(new MenuItem("sep0", "====== Misc"));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Misc").AddItem(new MenuItem("useR_KS", "= R to KS").SetValue(true));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Misc").AddItem(new MenuItem("useR_KS_minRange", "= R min Range").SetValue(new Slider(500, 900, 0)));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Misc").AddItem(new MenuItem("useR_KS_maxRange", "= R max Range").SetValue(new Slider(1500, 2000, 0)));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Misc").AddItem(new MenuItem("sep1", "========="));

			ChampionMenu.AddSubMenu(new Menu("Drawing", PUC.Player.ChampionName + "Drawing"));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Drawing").AddItem(new MenuItem("sep0", "====== Drawing"));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Drawing").AddItem(new MenuItem("Draw_Disabled", "Disable All").SetValue(false));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Drawing").AddItem(new MenuItem("Draw_Q", "Draw Q").SetValue(true));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Drawing").AddItem(new MenuItem("Draw_W", "Draw W").SetValue(true));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Drawing").AddItem(new MenuItem("Draw_E", "Draw E").SetValue(true));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Drawing").AddItem(new MenuItem("Draw_R", "Draw R").SetValue(true));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Drawing").AddItem(new MenuItem("sep1", "========="));

			PUC.Menu.AddSubMenu(ChampionMenu);
		}

		private void OnDraw(EventArgs args)
		{
			Orbwalker.AllowDrawing = !ChampionMenu.Item("Draw_Disabled").GetValue<bool>();

			if(ChampionMenu.Item("Draw_Disabled").GetValue<bool>())
				return;

			if(ChampionMenu.Item("Draw_Q").GetValue<bool>())
				if(Q.Level > 0)
					Utility.DrawCircle(ObjectManager.Player.Position, Q.Range, Q.IsReady() ? Color.Green : Color.Red);

			if(ChampionMenu.Item("Draw_W").GetValue<bool>())
				if(W.Level > 0)
					Utility.DrawCircle(ObjectManager.Player.Position, W.Range, W.IsReady() ? Color.Green : Color.Red);

			if(ChampionMenu.Item("Draw_E").GetValue<bool>())
				if(E.Level > 0)
					Utility.DrawCircle(ObjectManager.Player.Position, E.Range, E.IsReady() ? Color.Green : Color.Red);

			if(ChampionMenu.Item("Draw_R").GetValue<bool>())
				if(R.Level > 0)
					Utility.DrawCircle(ObjectManager.Player.Position, R.Range, R.IsReady() ? Color.Green : Color.Red);
		}

		private void OnUpdate(EventArgs args)
		{
			if(ChampionMenu.Item("useR_KS").GetValue<bool>())
			CastRks();

			switch(Orbwalker.CurrentMode)
			{
				case Orbwalker.Mode.Combo:
					if(ChampionMenu.Item("useQ_Combo").GetValue<bool>())
						Cast_BasicSkillshot_Enemy(Q);
					if(ChampionMenu.Item("useW_Combo").GetValue<bool>())
						Cast_BasicSkillshot_Enemy(W);
					if(ChampionMenu.Item("useR_Combo").GetValue<bool>())
						CastREnemy();
					break;
				case Orbwalker.Mode.Harass:
					if(ChampionMenu.Item("useQ_Harass").GetValue<bool>() && ManamanagerAllowCast("ManaManager_Harass"))
						Cast_BasicSkillshot_Enemy(Q);
					if(ChampionMenu.Item("useW_Harass").GetValue<bool>() && ManamanagerAllowCast("ManaManager_Harass"))
						Cast_BasicSkillshot_Enemy(W);
					break;
				case Orbwalker.Mode.LaneClear:
					if(ChampionMenu.Item("useQ_LaneClear_Enemy").GetValue<bool>() && ManamanagerAllowCast("ManaManager_LaneClear"))
						Cast_BasicSkillshot_Enemy(Q);
					if(ChampionMenu.Item("useQ_LaneClear_Minion").GetValue<bool>() && ManamanagerAllowCast("ManaManager_LaneClear"))
						Cast_Basic_Farm(Q, true);
					break;
				case Orbwalker.Mode.Lasthit:
					if(ChampionMenu.Item("useQ_Lasthit").GetValue<bool>())
						Cast_Basic_Farm(Q, true);
					break;
				case Orbwalker.Mode.RunlikeHell  :
					if (ChampionMenu.Item("useE_RunLikeHell").GetValue<bool>() && E.IsReady())
						E.Cast(GetModifiedPosition(PUC.Player.Position, Game.CursorPos, E.Range),UsePackets() );
					break;
			}
		}

		private void CastREnemy()
		{
			if(!R.IsReady())
				return;
			var minRange = ChampionMenu.Item("useR_Combo_minRange").GetValue<Slider>().Value;
			var maxRange = ChampionMenu.Item("useR_Combo_maxRange").GetValue<Slider>().Value;
			var minHit = ChampionMenu.Item("useR_Combo_minHit").GetValue<Slider>().Value;

			var target = TargetSelector.GetTarget(maxRange);
			if(target == null)
				return;
			if(target.Distance(PUC.Player) >= minRange)
				R.CastIfWillHit(target, minHit -1, UsePackets());
		}

		private void CastRks()
		{
			if(!R.IsReady())
				return;
			var minRange = ChampionMenu.Item("useR_KS_minRange").GetValue<Slider>().Value;
			var maxRange = ChampionMenu.Item("useR_KS_maxRange").GetValue<Slider>().Value;

			var killableEnemy =
				PUC.AllHerosEnemy.FirstOrDefault(
					hero =>
						hero.IsValidTarget(maxRange) && hero.Distance(PUC.Player) >= minRange &&
						hero.GetSpellDamage(hero, R.Slot) * 0.9 > hero.Health);
			if (killableEnemy != null)
				R.Cast(killableEnemy, UsePackets());
		}
	}
}
