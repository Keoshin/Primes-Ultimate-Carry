using System;
using System.Collections.Generic;
using Color = System.Drawing.Color;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
namespace Primes_Ultimate_Carry
{
	// ReSharper disable once InconsistentNaming
	class Champion_Draven :Champion 
	{
		public static List<Axe> AxeList = new List<Axe>();
		public static bool IsCatching;
		public Champion_Draven()
		{
			SetSpells();
			LoadMenu();

			GameObject.OnCreate += OnCreateObject;
			GameObject.OnDelete += OnDeleteObject;
			Game.OnGameUpdate += OnUpdate;
			Orbwalker.BeforeAttack += BeforeAttach;
			Drawing.OnDraw += OnDraw;
			Interrupter.OnPossibleToInterrupt += Interrupter_OnPosibleToInterrupt;
			AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
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
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Combo").AddItem(new MenuItem("useW_Combo", "= Use W").SetValue(false));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Combo").AddItem(new MenuItem("useE_Combo", "= Use E").SetValue(false));
			//ChampionMenu.SubMenu(PUC.Player.ChampionName + "Combo").AddItem(new MenuItem("useR_Combo", "= Use R if Hit").SetValue(new Slider(3, 0, 5)));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Combo").AddItem(new MenuItem("sep1", "========="));

			ChampionMenu.AddSubMenu(new Menu("Harass", PUC.Player.ChampionName + "Harass"));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Harass").AddItem(new MenuItem("sep0", "====== Harass"));
			AddManaManager(ChampionMenu.SubMenu(PUC.Player.ChampionName + "Harass"), "ManaManager_Harass", 40);
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Harass").AddItem(new MenuItem("useQ_Harass", "= Use Q").SetValue(true));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Harass").AddItem(new MenuItem("useW_Harass", "= Use W").SetValue(false));
			//ChampionMenu.SubMenu(PUC.Player.ChampionName + "Harass").AddItem(new MenuItem("useHarass_Auto", "= AutoHarras").SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Toggle)));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Harass").AddItem(new MenuItem("sep1", "========="));

			ChampionMenu.AddSubMenu(new Menu("LaneClear", PUC.Player.ChampionName + "LaneClear"));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "LaneClear").AddItem(new MenuItem("sep0", "====== LaneClear"));
			AddManaManager(ChampionMenu.SubMenu(PUC.Player.ChampionName + "LaneClear"), "ManaManager_LaneClear", 20);
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "LaneClear").AddItem(new MenuItem("useQ_LaneClear", "= Use Q").SetValue(true));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "LaneClear").AddItem(new MenuItem("useW_LaneClear", "= Use W").SetValue(false));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "LaneClear").AddItem(new MenuItem("useE_LaneClear", "= Use E").SetValue(true));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "LaneClear").AddItem(new MenuItem("sep1", "========="));

			//ChampionMenu.AddSubMenu(new Menu("Lasthit", PUC.Player.ChampionName + "Lasthit"));
			//ChampionMenu.SubMenu(PUC.Player.ChampionName + "Lasthit").AddItem(new MenuItem("sep0", "====== Lasthit"));
			//ChampionMenu.SubMenu(PUC.Player.ChampionName + "Lasthit").AddItem(new MenuItem("useQ_Lasthit", "= Use Q").SetValue(true));
			//ChampionMenu.SubMenu(PUC.Player.ChampionName + "Lasthit").AddItem(new MenuItem("sep1", "========="));

			//ChampionMenu.AddSubMenu(new Menu("RunLikeHell", PUC.Player.ChampionName + "RunLikeHell"));
			//ChampionMenu.SubMenu(PUC.Player.ChampionName + "RunLikeHell").AddItem(new MenuItem("sep0", "====== RunLikeHell"));
			//ChampionMenu.SubMenu(PUC.Player.ChampionName + "RunLikeHell").AddItem(new MenuItem("useW_RunLikeHell", "= W to speed up").SetValue(true));
			//ChampionMenu.SubMenu(PUC.Player.ChampionName + "RunLikeHell").AddItem(new MenuItem("sep1", "========="));

			ChampionMenu.AddSubMenu(new Menu("Misc", PUC.Player.ChampionName + "Misc"));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Misc").AddItem(new MenuItem("sep0", "====== Misc"));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Misc").AddItem(new MenuItem("useE_Interrupt", "= E to Interrupt").SetValue(true));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Misc").AddItem(new MenuItem("useE_GapCloser", "= E Anti Gapclose").SetValue(true));

			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Misc").AddItem(new MenuItem("useCatchAxe_Combo", "= Combo CatchAxeRange").SetValue(new Slider(300, 0, 1000)));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Misc").AddItem(new MenuItem("useCatchAxe_Harass", "= Harass CatchAxeRange").SetValue(new Slider(400, 0, 1000)));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Misc").AddItem(new MenuItem("useCatchAxe_LaneClear", "= LaneClear CatchAxeRange").SetValue(new Slider(700, 0, 1000)));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Misc").AddItem(new MenuItem("useCatchAxe_Lasthit", "= Lasthit CatchAxeRange").SetValue(new Slider(500, 0, 1000)));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Misc").AddItem(new MenuItem("useW_SpeecBuffCatch", "= Use W to Catch Axes").SetValue(true));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Misc").AddItem(new MenuItem("useQ_Stacks", "= Max Q Stacks").SetValue(new Slider(2, 1, 2)));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Misc").AddItem(new MenuItem("sep1", "========="));

			ChampionMenu.AddSubMenu(new Menu("Drawing", PUC.Player.ChampionName + "Drawing"));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Drawing").AddItem(new MenuItem("sep0", "====== Drawing"));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Drawing").AddItem(new MenuItem("Draw_Disabled", "Disable All").SetValue(false));
			ChampionMenu.SubMenu(PUC.Player.ChampionName + "Drawing").AddItem(new MenuItem("Draw_E", "Draw E").SetValue(true));
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

		private void OnDraw(EventArgs args)
		{
			Orbwalker.AllowDrawing = !ChampionMenu.Item("Draw_Disabled").GetValue<bool>();

			if(ChampionMenu.Item("Draw_Disabled").GetValue<bool>())
				return;

			if(ChampionMenu.Item("Draw_E").GetValue<bool>())
				if(E.Level > 0)
					Utility.DrawCircle(ObjectManager.Player.Position, E.Range, E.IsReady() ? Color.Green : Color.Red);

			if(ChampionMenu.Item("Draw_CatchRange").GetValue<bool>())
				if (Q.Level > 0)
				{
					switch(Orbwalker.CurrentMode)
					{
						case Orbwalker.Mode.Combo:
							Utility.DrawCircle(Game.CursorPos, ChampionMenu.Item("useCatchAxe_Combo").GetValue<Slider>().Value, Color.Blue);	
							break;
						case Orbwalker.Mode.Harass:
							Utility.DrawCircle(Game.CursorPos, ChampionMenu.Item("useCatchAxe_Harass").GetValue<Slider>().Value, Color.Blue);
							break;
						case Orbwalker.Mode.LaneClear:
							Utility.DrawCircle(Game.CursorPos, ChampionMenu.Item("useCatchAxe_LaneClear").GetValue<Slider>().Value, Color.Blue);
							break;
						case Orbwalker.Mode.Lasthit:
							Utility.DrawCircle(Game.CursorPos, ChampionMenu.Item("useCatchAxe_Lasthit").GetValue<Slider>().Value, Color.Blue);
							break;
					}
				}
					
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
			CatchAxe();
			switch(Orbwalker.CurrentMode)
			{
				case Orbwalker.Mode.Combo:
					if(ChampionMenu.Item("useQ_Combo").GetValue<bool>())
						CastQ();
					if(ChampionMenu.Item("useW_Combo").GetValue<bool>())
						CastW();
					if (ChampionMenu.Item("useE_Combo").GetValue<bool>())
						Cast_BasicSkillshot_Enemy(E);
					break;
				case Orbwalker.Mode.Harass:
					if(ChampionMenu.Item("useQ_Harass").GetValue<bool>() && ManamanagerAllowCast("ManaManager_Harass"))
						CastQ();
					if(ChampionMenu.Item("useW_Harass").GetValue<bool>() && ManamanagerAllowCast("ManaManager_Harass"))
						CastW();
					break;
				case Orbwalker.Mode.LaneClear:
					if(ChampionMenu.Item("useQ_LaneClear").GetValue<bool>() && ManamanagerAllowCast("ManaManager_LaneClear"))
						CastQ();
					if(ChampionMenu.Item("useW_LaneClear").GetValue<bool>() && ManamanagerAllowCast("ManaManager_LaneClear"))
						CastW();
					if(ChampionMenu.Item("useE_LaneClear").GetValue<bool>() && ManamanagerAllowCast("ManaManager_LaneClear"))
						Cast_BasicSkillshot_Enemy(E);
					break;
			}
		}

		private void CatchAxe()
		{
			if (AxeList.Count > 0)
			{
				Axe axe = null;
				foreach (var obj in AxeList.Where(obj => axe == null || obj.CreationTime < axe.CreationTime))
					axe = obj;
				if (axe != null)
				{
					var distanceNorm = Vector2.Distance(axe.Position.To2D(), PUC.Player.ServerPosition.To2D()) - PUC.Player.BoundingRadius;
					var distanceBuffed = PUC.Player.GetPath(axe.Position).ToList().To2D().PathLength();
					var canCatchAxeNorm = distanceNorm / PUC.Player.MoveSpeed + Game.Time < axe.EndTime;
					var canCatchAxeBuffed = distanceBuffed / (PUC.Player.MoveSpeed + (5 * W.Level + 35) * 0.01 * PUC.Player.MoveSpeed + Game.Time) < axe.EndTime;

					if (!ChampionMenu.Item("useW_SpeecBuffCatch").GetValue<bool>())
						if (!canCatchAxeNorm)
						{
							AxeList.Remove(axe);
							return;
						}

					if ((axe.Position.Distance(Game.CursorPos) < ChampionMenu.Item("useCatchAxe_Combo").GetValue<Slider>().Value &&
						Orbwalker.CurrentMode == Orbwalker.Mode.Combo) ||
						(axe.Position.Distance(Game.CursorPos) < ChampionMenu.Item("useCatchAxe_Harass").GetValue<Slider>().Value &&
						Orbwalker.CurrentMode == Orbwalker.Mode.Harass) ||
						(axe.Position.Distance(Game.CursorPos) < ChampionMenu.Item("useCatchAxe_LaneClear").GetValue<Slider>().Value &&
						Orbwalker.CurrentMode == Orbwalker.Mode.LaneClear ) ||
						(axe.Position.Distance(Game.CursorPos) < ChampionMenu.Item("useCatchAxe_Lasthit").GetValue<Slider>().Value &&
						Orbwalker.CurrentMode == Orbwalker.Mode.Lasthit))
					{
						if(canCatchAxeBuffed && !canCatchAxeNorm && W.IsReady())
							W.Cast();
						Orbwalker.CustomOrbwalkMode = true;
						Orbwalker.Orbwalk(GetModifiedPosition(axe.Position, Game.CursorPos, 49), Orbwalker.GetPossibleTarget());
					}



				}
				
			}
			else
				Orbwalker.CustomOrbwalkMode = false;
		}

		private void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
		{
			if(!ChampionMenu.Item("useE_GapCloser").GetValue<bool>())
				return;
			if (!(gapcloser.End.Distance(PUC.Player.ServerPosition) <= 100)) 
				return;
			E.CastIfHitchanceEquals(gapcloser.Sender, HitChance.Medium, UsePackets());
		}

		private void Interrupter_OnPosibleToInterrupt(Obj_AI_Base unit, InterruptableSpell spell)
		{
			if (!ChampionMenu.Item("useE_Interrupt").GetValue<bool>())
				return;
			E.CastIfHitchanceEquals(unit,HitChance.Medium,UsePackets());			
		}

		private static void OnCreateObject(GameObject sender, EventArgs args)
		{
			if (!sender.Name.Contains("Q_reticle_self"))
				return;
			AxeList.Add(new Axe(sender));
		}

		private static void OnDeleteObject(GameObject sender, EventArgs args)
		{
			if(!sender.Name.Contains("Q_reticle_self"))
			{
				return;
			}
			foreach (var axe in AxeList.Where(axe => axe.NetworkId == sender.NetworkId))
			{
				AxeList.Remove(axe);
				if(AxeList.Count == 0)
					IsCatching = false;
				return;
			}
		}

		private void BeforeAttach(Orbwalker.BeforeAttackEventArgs args)
		{
			
		}

		private void CastQ()
		{
			if(!Q.IsReady())
				return;
			if(ChampionMenu.Item("useQ_Stacks").GetValue<Slider>().Value < GetQStacks())
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

		private void CastW()
		{
			if(!W.IsReady())
				return;

			var target = TargetSelector.GetAATarget();
			if(target != null)
				W.Cast();

			if(Orbwalker.CurrentMode != Orbwalker.Mode.LaneClear)
				return;
			var allMinion = MinionManager.GetMinions(PUC.Player.Position,
				Orbwalker.GetAutoAttackRangeto(), MinionTypes.All, MinionTeam.NotAlly);
			if(!allMinion.Any(minion => minion.IsValidTarget(Orbwalker.GetAutoAttackRangeto(minion))))
				return;
			W.Cast();
		}

		public static int GetQStacks()
		{
			var buff = ObjectManager.Player.Buffs.FirstOrDefault(buff1 => buff1.Name.Equals("dravenspinningattack"));
			return buff != null ? buff.Count : 0;
		}

		internal class Axe
		{
			public GameObject AxeObject;
			public double CreationTime;
			public double EndTime;
			public int NetworkId;
			public Vector3 Position;

			public Axe(GameObject axeObject)
			{
				AxeObject = axeObject;
				NetworkId = axeObject.NetworkId;
				Position = axeObject.Position;
				CreationTime = Game.Time;
				EndTime = CreationTime + 1.2;
			}

			public float DistanceToPlayer()
			{
				return ObjectManager.Player.Distance(Position);
			}
		}
	}
}
