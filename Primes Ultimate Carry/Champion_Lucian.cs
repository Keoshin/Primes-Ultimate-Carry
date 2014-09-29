using System;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;
using Menu = LeagueSharp.Common.Menu;
using MenuItem = LeagueSharp.Common.MenuItem;

namespace Primes_Ultimate_Carry
{
	// ReSharper disable once InconsistentNaming
	class Champion_Lucian : Champion
	{
		public bool HavePassiveUp;
		public int Delay = 150;
		public int DelayTick = 0;

		public Champion_Lucian()
		{
			SetSpells();
			LoadMenu();

			Game.OnGameUpdate += OnUpdate;
			Drawing.OnDraw += OnDraw;
			Obj_AI_Base.OnProcessSpellCast += OnProcessSpell;

			PluginLoaded();
		}

		private void SetSpells()
		{
			Q = new Spell(SpellSlot.Q, 675);
			Q.SetTargetted(0.5f,float.MaxValue);

			W = new Spell(SpellSlot.W, 1000);
			W.SetSkillshot(0.3f, 80f, 1600, true, SkillshotType.SkillshotLine);

			E = new Spell(SpellSlot.E, 475);

			R = new Spell(SpellSlot.R, 1400);
			R.SetSkillshot(0.01f, 110, 2800f, true, SkillshotType.SkillshotLine);
		}

		private void LoadMenu()
		{
			ChampionMenu.AddSubMenu(new Menu("Combo", PUC.Player.ChampionName + "Combo"));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Combo").AddItem(new MenuItem("sep0", "====== Combo"));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Combo").AddItem(new MenuItem("useQ_Combo", "= Use Q").SetValue(true));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Combo").AddItem(new MenuItem("useW_Combo", "= Use W").SetValue(true));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Combo").AddItem(new MenuItem("useE_Combo", "= Use E").SetValue(true));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Combo").AddItem(new MenuItem("useE_Combo_maxrange", "= E MaxRange to Enemy").SetValue(new Slider(1100, 2000, 500)));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Combo").AddItem(new MenuItem("useR_Combo_Filler", "= R if rest on CD").SetValue(true));
			// R on Groups todo
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Combo").AddItem(new MenuItem("sep1", "========="));

			ChampionMenu.AddSubMenu(new Menu("Harass", PUC.Player.ChampionName + "Harass"));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Harass").AddItem(new MenuItem("sep0", "====== Harass"));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Harass").AddItem(new MenuItem("useQ_Harass", "= Use Q").SetValue(true));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Harass").AddItem(new MenuItem("useW_Harass", "= Use W").SetValue(true));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Harass").AddItem(new MenuItem("sep1", "========="));

			ChampionMenu.AddSubMenu(new Menu("LaneClear", PUC.Player.ChampionName + "LaneClear"));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "LaneClear").AddItem(new MenuItem("sep0", "====== LaneClear"));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "LaneClear").AddItem(new MenuItem("useQ_LaneClear", "= Use Q").SetValue(true));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "LaneClear").AddItem(new MenuItem("useW_LaneClear", "= Use W").SetValue(true));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "LaneClear").AddItem(new MenuItem("useE_LaneClear", "= Use E").SetValue(true));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "LaneClear").AddItem(new MenuItem("useE_LaneClear_maxrange", "= E MaxRange to Minion").SetValue(new Slider(1100, 2000, 500)));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "LaneClear").AddItem(new MenuItem("useE_LaneClear_dangerzones", "= E inside Dangerzones ?").SetValue(false));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "LaneClear").AddItem(new MenuItem("sep1", "========="));

			ChampionMenu.AddSubMenu(new Menu("RunLikeHell", PUC.Player.ChampionName + "RunLikeHell"));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "RunLikeHell").AddItem(new MenuItem("sep0", "====== RunLikeHell"));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "RunLikeHell").AddItem(new MenuItem("useE_RunLikeHell", "= E to Mouse").SetValue(true));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "RunLikeHell").AddItem(new MenuItem("useE_RunLikeHell_passive", "= Ignore Passive").SetValue(true));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "RunLikeHell").AddItem(new MenuItem("sep1", "========="));

			ChampionMenu.AddSubMenu(new Menu("Misc", PUC.Player.ChampionName + "Misc"));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Misc").AddItem(new MenuItem("sep0", "====== Misc"));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Misc").AddItem(new MenuItem("usePassive_care", "= Take Care of Passive").SetValue(true));
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

		private void OnProcessSpell(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
		{
			if (!sender.IsMe) 
				return;
			switch (args.SData.Name)
			{
				case "LucianQ":
					HavePassiveUp = true;
					Utility.DelayAction.Add(500 , SetPassive);
					break;
				case "LucianW":
					HavePassiveUp = true;
					Utility.DelayAction.Add(500, SetPassive);
					break;
				case "LucianE":
					HavePassiveUp = true;
					Utility.DelayAction.Add(500, SetPassive);
					break;
				case "LucianR":
					HavePassiveUp = true;
					Utility.DelayAction.Add(500, SetPassive);
					break;
				case "LucianPassiveAttack":
					Utility.DelayAction.Add( 50, SetPassive);
					break;
			}
		}

		private void SetPassive()
		{
			if(PUC.Player.Buffs.Any(buff => buff.Name == "LucianR"))
			{
				Utility.DelayAction.Add(100, SetPassive);
				return;
			}
			if(PUC.Player.Buffs.All(buff => buff.Name != "lucianpassivebuff"))
				HavePassiveUp = false;
			else
				Utility.DelayAction.Add(100, SetPassive);
		}

		public Vector3 runwardposition(Vector3  unit, int range)
		{
			var me = ObjectManager.Player.Position;
			var mouse = unit;

			var newpos = mouse - me;
			newpos.Normalize();
			return unit + (newpos * range);
		}

		private void OnDraw(EventArgs args)
		{			

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
			switch(Orbwalker.CurrentMode)
			{
				case Orbwalker.Mode.Combo:
					Combo();
					break;
				case Orbwalker.Mode.Harass:
					Harass();
					break;
				case Orbwalker.Mode.LaneClear:
					LaneClear();
					break;
				case Orbwalker.Mode.RunlikeHell:
					RunlikeHell();
					break;
			}
		}

		private void Combo()
		{
			if(ChampionMenu.Item("useE_Combo").GetValue<bool>())
				CastE(!ChampionMenu.Item("usePassive_care").GetValue<bool>());
			if(ChampionMenu.Item("useW_Combo").GetValue<bool>())
				CastW(!ChampionMenu.Item("usePassive_care").GetValue<bool>());
			if(ChampionMenu.Item("useQ_Combo").GetValue<bool>())
				CastQ(!ChampionMenu.Item("usePassive_care").GetValue<bool>());
			//CastR();
		}

		private void Harass()
		{
			if(ChampionMenu.Item("useW_Harass").GetValue<bool>())
				CastW();
			if(ChampionMenu.Item("useQ_Harass").GetValue<bool>())
				CastQ(!ChampionMenu.Item("usePassive_care").GetValue<bool>());
		}

		private void LaneClear()
		{
			if(ChampionMenu.Item("useE_LaneClear").GetValue<bool>())
				CastE(!ChampionMenu.Item("usePassive_care").GetValue<bool>());
			if(ChampionMenu.Item("useW_LaneClear").GetValue<bool>())
				CastW(!ChampionMenu.Item("usePassive_care").GetValue<bool>());
			if(ChampionMenu.Item("useQ_LaneClear").GetValue<bool>())
				CastQ(!ChampionMenu.Item("usePassive_care").GetValue<bool>());
		}

		private void RunlikeHell()
		{
			if(ChampionMenu.Item("useE_RunLikeHell").GetValue<bool>())
				CastE(ChampionMenu.Item("useE_RunLikeHell_passive").GetValue<bool>());
		}

		private void CastQ(bool iggnorePassive = false)
		{
			if(!Q.IsReady() || (HavePassiveUp && !iggnorePassive))
				return;
			if(Delay >= Environment.TickCount - DelayTick)
				return;
			var targetNormal = TargetSelector.GetTarget(Q.Range);
			var targetExtended = TargetSelector.GetTarget(1100);
			switch(Orbwalker.CurrentMode)
			{
				case Orbwalker.Mode.Combo:
					foreach(var enemy in PUC.AllHerosEnemy.Where(hero => hero.IsValidTarget(1100)))
					{
						foreach(var obj in ObjectManager.Get<Obj_AI_Base>().Where(obj => obj.IsValidTarget(Q.Range)))
						{
							if(obj is Obj_AI_Hero)
							{
								Q.Cast(obj);
								DelayTick = Environment.TickCount;
								return;
							}
							for(var i = 10; i < 1070 - Q.Range; i = i + 10)
							{
								if(!(runwardposition(Q.GetPrediction(obj).UnitPosition, i).Distance(Q.GetPrediction(enemy).UnitPosition) < 35))
									continue;
								Q.Cast(obj);
								DelayTick = Environment.TickCount;
								return;
							}
						}
					}
					break;
				case Orbwalker.Mode.Harass:
					foreach(var enemy in PUC.AllHerosEnemy.Where(hero => hero.IsValidTarget(1100)))
					{
						foreach(var obj in ObjectManager.Get<Obj_AI_Base>().Where(obj => obj.IsValidTarget(Q.Range)))
						{
							if (obj is Obj_AI_Hero )
							{
								Q.Cast(obj);
								DelayTick = Environment.TickCount;
								return;
							}
							for(var i = 10; i < 1070 - Q.Range; i = i + 10)
							{
								if (!(runwardposition(Q.GetPrediction(obj).UnitPosition, i).Distance(Q.GetPrediction(enemy).UnitPosition) < 35))
									continue;
								Q.Cast(obj);
								DelayTick = Environment.TickCount;
								return;
							}
						}
					}
					break;
				case Orbwalker.Mode.LaneClear:
					var allMinions = MinionManager.GetMinions(ObjectManager.Player.Position, Q.Range, MinionTypes.All, MinionTeam.NotAlly);
					var minion = allMinions.FirstOrDefault(minionn => minionn.Distance(ObjectManager.Player) <= Q.Range && HealthPrediction.LaneClearHealthPrediction(minionn,500) > 0);
					if(minion == null)
						return;
					Q.CastOnUnit(minion, UsePackets());
					DelayTick = Environment.TickCount;
					break;
			}
		}

		private void CastW(bool iggnorePassive = false)
		{
			if(!W.IsReady() || (HavePassiveUp && !iggnorePassive))
				return;
			if(Delay >= Environment.TickCount - DelayTick)
				return;
		
			var target =TargetSelector.GetTarget(W.Range);
			switch(Orbwalker.CurrentMode)
			{
				case Orbwalker.Mode.Combo:
					if (target.IsValidTarget(W.Range) && W.GetPrediction(target).Hitchance >= HitChance.High)
					{
						W.Cast(target, UsePackets());
						DelayTick = Environment.TickCount;
					}
					else if(W.GetPrediction(target).Hitchance == HitChance.Collision)
					{
						var wCollision = W.GetPrediction(target).CollisionObjects;
						if(!wCollision.Any(wCollisionChar => wCollisionChar.Distance(target) >= 100))
						{
							W.Cast(target, UsePackets());
							DelayTick = Environment.TickCount;
						}
					}				
					break;
				case Orbwalker.Mode.Harass:
					if(target.IsValidTarget(W.Range) && W.GetPrediction(target).Hitchance >= HitChance.High)
					{
						W.Cast(target, UsePackets());
						DelayTick = Environment.TickCount;
					}
					else if(W.GetPrediction(target).Hitchance == HitChance.Collision)
					{
						var wCollision = W.GetPrediction(target).CollisionObjects;
						if(!wCollision.Any(wCollisionChar => wCollisionChar.Distance(target) >= 100))
						{
							W.Cast(target, UsePackets());
							DelayTick = Environment.TickCount;
						}
					}
					break;
				case Orbwalker.Mode.LaneClear:
					var allMinions = MinionManager.GetMinions(ObjectManager.Player.Position, W.Range - 100, MinionTypes.All, MinionTeam.NotAlly);
					var minion = allMinions.FirstOrDefault(minionn => minionn.IsValidTarget(W.Range) && HealthPrediction.LaneClearHealthPrediction(minionn, 500) > 0);
					if(minion != null)
					{
						W.Cast(minion, UsePackets());
						DelayTick = Environment.TickCount;
					}
					break;
			}
		}

		private void CastE(bool iggnorePassive = false)
		{
			if(!E.IsReady() || ( HavePassiveUp && !iggnorePassive))
				return;
			if(Delay >= Environment.TickCount - DelayTick)
				return;
			DelayTick = Environment.TickCount;
			switch(Orbwalker.CurrentMode)
			{
				case Orbwalker.Mode.Combo:
					if (TargetSelector.GetTarget(PUC.Menu.Item("useE_Combo_maxrange").GetValue<Slider>().Value) != null)
					{
						E.Cast(Game.CursorPos, UsePackets());
						DelayTick = Environment.TickCount;
					}
					break;
				case Orbwalker.Mode.LaneClear:
					var allMinions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, PUC.Menu.Item("useE_LaneClear_maxrange").GetValue<Slider>().Value, MinionTypes.All,
						MinionTeam.NotAlly);
					if(allMinions.Where(minion => minion != null).Any(minion => minion.IsValidTarget(PUC.Menu.Item("useE_LaneClear_maxrange").GetValue<Slider>().Value) && E.IsReady()))
					{
						E.Cast(Game.CursorPos, UsePackets());
						DelayTick = Environment.TickCount;
					}
					break;
				case Orbwalker.Mode.RunlikeHell:
					if(Game.CursorPos.Distance(PUC.Player.Position) > E.Range && E.IsReady())
					{
						E.Cast(Game.CursorPos, UsePackets());
						DelayTick = Environment.TickCount;
					}
					break;
			}
		}
	}
}
