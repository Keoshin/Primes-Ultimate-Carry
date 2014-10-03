using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace Primes_Ultimate_Carry
{
	class BaseUlt
	{

		private static bool _compatibleChamp;
		private static IEnumerable<Obj_AI_Hero> _ownTeam;
		private static IEnumerable<Obj_AI_Hero> _enemyTeam;
		private static Vector3 _enemySpawnPos;
		private static List<PlayerInfo> _playerInfo = new List<PlayerInfo>();
		private static int _ultCasted;
		private static Spell _ult;

		public static Utility.Map.MapType Map;
		public static Dictionary<int, int> RecallT = new Dictionary<int, int>();

		private static readonly Dictionary<String, UltData> UltInfo = new Dictionary<string, UltData> {
            {
                "Jinx",
                new UltData {
                    ManaCost = 100f,
                    DamageMultiplicator = 1f,
                    Width = 140f,
                    Delay = 600f/1000f,
                    Speed = 1700f,
                }
            }, {
                "Ashe",
                new UltData {
                    ManaCost = 100f,
                    DamageMultiplicator = 1f,
                    Width = 130f,
                    Delay = 250f/1000f,
                    Speed = 1600f,
                }
            }, {
                "Draven",
                new UltData {
                    ManaCost = 120f,
                    DamageMultiplicator = 0.7f,
                    Width = 160f,
                    Delay = 400f/1000f,
                    Speed = 2000f,
                }
            }, {
                "Ezreal",
                new UltData {
                    ManaCost = 100f,
                    DamageMultiplicator = 0.7f,
                    Width = 160f,
                    Delay = 1000f/1000f,
                    Speed = 2000f,
                }
            }
        };

		internal static void AddtoMenu(Menu menu)
		{
			menu.AddItem(new MenuItem("showRecalls", "Show Recalls").SetValue(true));
			menu.AddItem(new MenuItem("baseUlt", "Base Ult").SetValue(true));
			menu.AddItem(new MenuItem("extraDelay", "Extra Delay").SetValue(new Slider(0, -2000, 2000)));
			menu.AddItem(
				new MenuItem("panicKey", "Panic key (hold for disable)").SetValue(new KeyBind(32, KeyBindType.Press)));
			menu.AddItem(
				new MenuItem("regardlessKey", "No timelimit (hold)").SetValue(new KeyBind(17, KeyBindType.Press)));
			menu.AddItem(new MenuItem("debugMode", "Debug (developer only)").SetValue(false).DontSave());

			var teamUlt = menu.AddSubMenu(new Menu("Team Baseult Friends", "TeamUlt"));

			var champions = ObjectManager.Get<Obj_AI_Hero>().ToList();

			_ownTeam = champions.Where(x => x.IsAlly);
			_enemyTeam = champions.Where(x => x.IsEnemy);

			_compatibleChamp = Helper.IsCompatibleChamp(ObjectManager.Player.ChampionName);

			if(_compatibleChamp)
				foreach(Obj_AI_Hero champ in _ownTeam.Where(x => !x.IsMe && Helper.IsCompatibleChamp(x.ChampionName)))
					teamUlt.AddItem(
						new MenuItem(champ.ChampionName, champ.ChampionName + " friend with Baseult?").SetValue(false)
							.DontSave());

			_enemySpawnPos =
				ObjectManager.Get<GameObject>()
					.First(x => x.Type == GameObjectType.obj_SpawnPoint && x.Team != ObjectManager.Player.Team)
					.Position;

			Map = Utility.Map.GetMap()._MapType;

			_playerInfo = _enemyTeam.Select(x => new PlayerInfo(x)).ToList();
			_playerInfo.Add(new PlayerInfo(ObjectManager.Player));

			_ult = new Spell(SpellSlot.R, 20000f);

			PUC.Menu.AddSubMenu(menu);

			Game.OnGameProcessPacket += Game_OnGameProcessPacket;
			Drawing.OnDraw += Drawing_OnDraw;

			if(_compatibleChamp)
				Game.OnGameUpdate += Game_OnGameUpdate;


		}

		private static void Game_OnGameUpdate(EventArgs args)
		{
			int time = Environment.TickCount;

			foreach(PlayerInfo playerInfo in _playerInfo.Where(x => x.Champ.IsVisible))
				playerInfo.LastSeen = time;

			if(!PUC.Menu.Item("baseUlt").GetValue<bool>())
				return;

			foreach(PlayerInfo playerInfo in _playerInfo.Where(x =>
				x.Champ.IsValid &&
				!x.Champ.IsDead &&
				x.Champ.IsEnemy &&
				x.Recall.Status == Packet.S2C.Recall.RecallStatus.RecallStarted).OrderBy(x => x.GetRecallEnd()))
			{
				if(_ultCasted == 0 || Environment.TickCount - _ultCasted > 20000)
					//DONT change Environment.TickCount; check for draven ult return
					HandleRecallShot(playerInfo);
			}
		}

		private static float GetUltManaCost(Obj_AI_Hero source) //remove later when fixed
		{
			float manaCost = UltInfo[source.ChampionName].ManaCost;

			if(source.ChampionName == "Karthus")
			{
				if(source.Level >= 11)
					manaCost += 25;

				if(source.Level >= 16)
					manaCost += 25;
			}

			return manaCost;
		}

		private static void HandleRecallShot(PlayerInfo playerInfo)
		{
			bool shoot = false;

			foreach(Obj_AI_Hero champ in _ownTeam.Where(x =>
				x.IsValid && (x.IsMe || Helper.GetSafeMenuItem<bool>(PUC.Menu.Item(x.ChampionName))) &&
				!x.IsDead && !x.IsStunned &&
				(x.Spellbook.CanUseSpell(SpellSlot.R) == SpellState.Ready ||
				 (x.Spellbook.GetSpell(SpellSlot.R).Level > 0 &&
				  x.Spellbook.CanUseSpell(SpellSlot.R) == SpellState.Surpressed &&
				  x.Mana >= GetUltManaCost(x)))))
			//use when fixed: champ.Spellbook.GetSpell(SpellSlot.R) = Ready or champ.Spellbook.GetSpell(SpellSlot.R).ManaCost)
			{
				if(champ.ChampionName != "Ezreal" && champ.ChampionName != "Karthus" &&
					Helper.IsCollidingWithChamps(champ, _enemySpawnPos, UltInfo[champ.ChampionName].Width))
					continue;

				//increase timeneeded if it should arrive earlier, decrease if later
				float timeneeded =
					Helper.GetSpellTravelTime(champ, UltInfo[champ.ChampionName].Speed,
						UltInfo[champ.ChampionName].Delay, _enemySpawnPos) -
					(PUC.Menu.Item("extraDelay").GetValue<Slider>().Value + 65);

				if(timeneeded - playerInfo.GetRecallCountdown() > 60)
					continue;

				playerInfo.IncomingDamage[champ.NetworkId] = (float)Helper.GetUltDamage(champ, playerInfo.Champ) *
															 UltInfo[champ.ChampionName].DamageMultiplicator;

				if(playerInfo.GetRecallCountdown() <= timeneeded)
					if(champ.IsMe)
						shoot = true;
			}

			float totalUltDamage = playerInfo.IncomingDamage.Values.Sum();

			float targetHealth = Helper.GetTargetHealth(playerInfo);

			if(!shoot || PUC.Menu.Item("panicKey").GetValue<KeyBind>().Active)
			{
				if(PUC.Menu.Item("debugMode").GetValue<bool>())
					Game.PrintChat("!SHOOT/PANICKEY {0} (Health: {1} TOTAL-UltDamage: {2})",
						playerInfo.Champ.ChampionName, targetHealth, totalUltDamage);

				return;
			}

			playerInfo.IncomingDamage.Clear(); //wrong placement?

			int time = Environment.TickCount;

			if(time - playerInfo.LastSeen > 20000 && !PUC.Menu.Item("regardlessKey").GetValue<KeyBind>().Active)
			{
				if(totalUltDamage < playerInfo.Champ.MaxHealth)
				{
					if(PUC.Menu.Item("debugMode").GetValue<bool>())
						Game.PrintChat("DONT SHOOT, TOO LONG NO VISION {0} (Health: {1} TOTAL-UltDamage: {2})",
							playerInfo.Champ.ChampionName, targetHealth, totalUltDamage);

					return;
				}
			}
			else if(totalUltDamage < targetHealth)
			{
				if(PUC.Menu.Item("debugMode").GetValue<bool>())
					Game.PrintChat("DONT SHOOT {0} (Health: {1} TOTAL-UltDamage: {2})", playerInfo.Champ.ChampionName,
						targetHealth, totalUltDamage);

				return;
			}

			if(PUC.Menu.Item("debugMode").GetValue<bool>())
				Game.PrintChat("SHOOT {0} (Health: {1} TOTAL-UltDamage: {2})", playerInfo.Champ.ChampionName,
					targetHealth, totalUltDamage);

			_ult.Cast(_enemySpawnPos, true);
			_ultCasted = time;
		}

		private static void Drawing_OnDraw(EventArgs args)
		{
			if(!PUC.Menu.Item("showRecalls").GetValue<bool>())
				return;

			int index = -1;

			foreach(PlayerInfo playerInfo in _playerInfo.Where(x =>
				(x.Recall.Status == Packet.S2C.Recall.RecallStatus.RecallStarted ||
				 x.Recall.Status == Packet.S2C.Recall.RecallStatus.TeleportStart) &&
				x.Champ.IsValid &&
				!x.Champ.IsDead &&
				x.GetRecallCountdown() > 0 &&
				(x.Champ.IsEnemy || PUC.Menu.Item("debugMode").GetValue<bool>())).OrderBy(x => x.GetRecallEnd()))
			{
				index++;

				//draw progress bar
				//show circle on minimap on recall

				Drawing.DrawText(Drawing.Width * 0.73f, Drawing.Height * 0.88f + (index * 15f), Color.Red,
					playerInfo.ToString());
			}
		}

		private static void Game_OnGameProcessPacket(GamePacketEventArgs args)
		{
			if(args.PacketData[0] == Packet.S2C.Recall.Header)
			{
				Packet.S2C.Recall.Struct newRecall = Helper.RecallDecode(args.PacketData);

				PlayerInfo playerInfo =
					_playerInfo.Find(x => x.Champ.NetworkId == newRecall.UnitNetworkId).UpdateRecall(newRecall);
				//Packet.S2C.Recall.Decoded(args.PacketData)

				if(PUC.Menu.Item("debugMode").GetValue<bool>())
					Game.PrintChat(playerInfo.Champ.ChampionName + ": " + playerInfo.Recall.Status + " duration: " +
								   playerInfo.Recall.Duration + " guessed health: " + Helper.GetTargetHealth(playerInfo) +
								   " lastseen: " + playerInfo.LastSeen + " health: " + playerInfo.Champ.Health +
								   " own-ultdamage: " +
								   (float)Helper.GetUltDamage(ObjectManager.Player, playerInfo.Champ) *
								   UltInfo[ObjectManager.Player.ChampionName].DamageMultiplicator);
			}
		}

		private struct UltData
		{
			public float DamageMultiplicator;
			public float Delay;
			public float ManaCost;
			public float Speed;
			public float Width;
		}

		internal class PlayerInfo
		{
			public readonly Obj_AI_Hero Champ;
			public readonly Dictionary<int, float> IncomingDamage;
			public int LastSeen;
			public Packet.S2C.Recall.Struct Recall;

			public PlayerInfo(Obj_AI_Hero champ)
			{
				Champ = champ;
				Recall = new Packet.S2C.Recall.Struct(champ.NetworkId, Packet.S2C.Recall.RecallStatus.Unknown, Packet.S2C.Recall.ObjectType.Player, 0);
				IncomingDamage = new Dictionary<int, float>();
			}

			public PlayerInfo UpdateRecall(Packet.S2C.Recall.Struct newRecall)
			{
				Recall = newRecall;
				return this;
			}

			public int GetRecallStart()
			{
				switch((int)Recall.Status)
				{
					case (int)Packet.S2C.Recall.RecallStatus.RecallStarted:
					case (int)Packet.S2C.Recall.RecallStatus.TeleportStart:
						return RecallT[Recall.UnitNetworkId];

					default:
						return 0;
				}
			}

			public int GetRecallEnd()
			{
				return GetRecallStart() + Recall.Duration;
			}

			public int GetRecallCountdown()
			{
				int countdown = GetRecallEnd() - Environment.TickCount;
				return countdown < 0 ? 0 : countdown;
			}

			public override string ToString()
			{
				string drawtext = Champ.ChampionName + ": " + Recall.Status; //change to better string

				float countdown = GetRecallCountdown() / 1000f;

				if(countdown > 0)
					drawtext += " (" + countdown.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture) + "s)";

				return drawtext;
			}
		}

		internal class Helper
		{
			public static T GetSafeMenuItem<T>(MenuItem item)
			{
				if(item != null)
					return item.GetValue<T>();

				return default(T);
			}

			public static float GetTargetHealth(PlayerInfo playerInfo)
			{
				if(playerInfo.Champ.IsVisible)
					return playerInfo.Champ.Health;

				float predictedhealth = playerInfo.Champ.Health +
										playerInfo.Champ.HPRegenRate *
										((Environment.TickCount - playerInfo.LastSeen + playerInfo.GetRecallCountdown()) /
										 1000f);

				return predictedhealth > playerInfo.Champ.MaxHealth ? playerInfo.Champ.MaxHealth : predictedhealth;
			}

			public static float GetSpellTravelTime(Obj_AI_Hero source, float speed, float delay, Vector3 targetpos)
			{
				if(source.ChampionName == "Karthus")
					return delay * 1000;

				float distance = Vector3.Distance(source.ServerPosition, targetpos);

				float missilespeed = speed;

				if(source.ChampionName == "Jinx" && distance > 1350)
				//1700 = missilespeed, 2200 = missilespeed after acceleration, 1350 acceleration starts, 1500 = fully acceleration
				{
					const float accelerationrate = 0.3f; //= (1500f - 1350f) / (2200 - speed), 1 unit = 0.3units/second

					float acceldifference = distance - 1350f;

					if(acceldifference > 150f) //it only accelerates 150 units
						acceldifference = 150f;

					float difference = distance - 1500f;

					missilespeed = (1350f * speed + acceldifference * (speed + accelerationrate * acceldifference) +
									difference * 2200f) / distance;
				}

				return (distance / missilespeed + delay) * 1000;
			}

			public static bool IsCollidingWithChamps(Obj_AI_Hero source, Vector3 targetpos, float width)
			{
				var input = new PredictionInput
				{
					Radius = width,
					Unit = source,
				};

				input.CollisionObjects[0] = CollisionableObjects.Heroes;

				return LeagueSharp.Common.Collision.GetCollision(new List<Vector3> { targetpos }, input).Any();
				//x => x.NetworkId != targetnetid, bit harder to realize with teamult
			}

			public static Packet.S2C.Recall.Struct RecallDecode(byte[] data)
			{
				var reader = new BinaryReader(new MemoryStream(data));
				var recall = new Packet.S2C.Recall.Struct();

				reader.ReadByte(); //PacketId
				reader.ReadInt32();
				recall.UnitNetworkId = reader.ReadInt32();
				reader.ReadBytes(66);

				recall.Status = Packet.S2C.Recall.RecallStatus.Unknown;

				bool teleport = false;

				if(BitConverter.ToString(reader.ReadBytes(6)) != "00-00-00-00-00-00")
				{
					if(BitConverter.ToString(reader.ReadBytes(3)) != "00-00-00")
					{
						recall.Status = Packet.S2C.Recall.RecallStatus.TeleportStart;
						teleport = true;
					}
					else
						recall.Status = Packet.S2C.Recall.RecallStatus.RecallStarted;
				}

				reader.Close();

				var champ = ObjectManager.GetUnitByNetworkId<Obj_AI_Hero>(recall.UnitNetworkId);

				if(champ != null)
				{
					if(teleport)
						recall.Duration = 3500;
					else
					//use masteries to detect recall duration, because spelldata is not initialized yet when enemy has not been seen
					{
						recall.Duration = Map == Utility.Map.MapType.CrystalScar ? 4500 : 8000;

						if(champ.Masteries.Any(x => x.Page == MasteryPage.Utility && x.Id == 65 && x.Points == 1))
							recall.Duration -= Map == Utility.Map.MapType.CrystalScar ? 500 : 1000;
						//phasewalker mastery
					}

					int time = Environment.TickCount - Game.Ping;

					if(!RecallT.ContainsKey(recall.UnitNetworkId))
						RecallT.Add(recall.UnitNetworkId, time);
					//will result in status RecallStarted, which would be wrong if the assembly was to be loaded while somebody recalls
					else
					{
						if(RecallT[recall.UnitNetworkId] == 0)
							RecallT[recall.UnitNetworkId] = time;
						else
						{
							if(time - RecallT[recall.UnitNetworkId] > recall.Duration - 75)
								recall.Status = teleport
									? Packet.S2C.Recall.RecallStatus.TeleportEnd
									: Packet.S2C.Recall.RecallStatus.RecallFinished;
							else
								recall.Status = teleport
									? Packet.S2C.Recall.RecallStatus.TeleportAbort
									: Packet.S2C.Recall.RecallStatus.RecallAborted;

							RecallT[recall.UnitNetworkId] = 0; //recall aborted or finished, reset status
						}
					}
				}

				return recall;
			}

			public static bool IsCompatibleChamp(String championName)
			{
				switch(championName)
				{
					case "Ashe":
					case "Ezreal":
					case "Draven":
					case "Jinx":
						return true;

					default:
						return false;
				}
			}

			public static double GetUltDamage(Obj_AI_Hero source, Obj_AI_Hero enemy)
			{
				switch(source.ChampionName)
				{
					case "Ashe":
						return
							CalcMagicDmg(
								(75 + (source.Spellbook.GetSpell(SpellSlot.R).Level * 175)) + (1.0 * source.FlatMagicDamageMod),
								source, enemy);
					case "Draven":
						return
							CalcPhysicalDmg(
								(75 + (source.Spellbook.GetSpell(SpellSlot.R).Level * 100)) +
								(1.1 * source.FlatPhysicalDamageMod), source, enemy); // way to enemy
					case "Jinx":
						double percentage =
							CalcPhysicalDmg(
								((enemy.MaxHealth - enemy.Health) / 100) *
								(20 + (5 * source.Spellbook.GetSpell(SpellSlot.R).Level)), source, enemy);
						return percentage +
							   CalcPhysicalDmg(
								   (150 + (source.Spellbook.GetSpell(SpellSlot.R).Level * 100)) +
								   (1.0 * source.FlatPhysicalDamageMod), source, enemy);
					case "Ezreal":
						return CalcMagicDmg((200 + (source.Spellbook.GetSpell(SpellSlot.R).Level * 150)) +
											(1.0 * (source.FlatPhysicalDamageMod + source.BaseAttackDamage)) +
											(0.9 * source.FlatMagicDamageMod), source, enemy);
					case "Karthus":
						return CalcMagicDmg(
							(100 + (source.Spellbook.GetSpell(SpellSlot.R).Level * 150)) +
							(0.6 * source.FlatMagicDamageMod), source, enemy);
					default:
						return 0;
				}
			}

			public static double CalcPhysicalDmg(double dmg, Obj_AI_Hero source, Obj_AI_Base enemy)
			{
				bool doubleedgedsword = false, havoc = false;
				int executioner = 0;

				foreach(Mastery mastery in source.Masteries)
				{
					if(mastery.Page == MasteryPage.Offense)
					{
						switch(mastery.Id)
						{
							case 65:
								doubleedgedsword = (mastery.Points == 1);
								break;
							case 146:
								havoc = (mastery.Points == 1);
								break;
							case 132:
								break;
							case 100:
								executioner = mastery.Points;
								break;
							case 68:
								break;
						}
					}
				}

				double additionaldmg = 0;
				if(doubleedgedsword)
				{
					if(source.CombatType == GameObjectCombatType.Melee)
					{
						additionaldmg += dmg * 0.02;
					}
					else
					{
						additionaldmg += dmg * 0.015;
					}
				}

				if(havoc)
				{
					additionaldmg += dmg * 0.03;
				}

				if(executioner > 0)
				{
					if(executioner == 1)
					{
						if((enemy.Health / enemy.MaxHealth) * 100 < 20)
						{
							additionaldmg += dmg * 0.05;
						}
					}
					else if(executioner == 2)
					{
						if((enemy.Health / enemy.MaxHealth) * 100 < 35)
						{
							additionaldmg += dmg * 0.05;
						}
					}
					else if(executioner == 3)
					{
						if((enemy.Health / enemy.MaxHealth) * 100 < 50)
						{
							additionaldmg += dmg * 0.05;
						}
					}
				}

				double newarmor = enemy.Armor * source.PercentArmorPenetrationMod;
				double dmgreduction = 100 / (100 + newarmor - source.FlatArmorPenetrationMod);
				return (((dmg + additionaldmg) * dmgreduction));
			}

			public static double CalcMagicDmg(double dmg, Obj_AI_Hero source, Obj_AI_Base enemy)
			{
				bool doubleedgedsword = false, havoc = false;
				int executioner = 0;

				foreach(Mastery mastery in source.Masteries)
				{
					if(mastery.Page == MasteryPage.Offense)
					{
						switch(mastery.Id)
						{
							case 65:
								doubleedgedsword = (mastery.Points == 1);
								break;
							case 146:
								havoc = (mastery.Points == 1);
								break;
							case 132:
								break;
							case 100:
								executioner = mastery.Points;
								break;
							case 68:
								break;
						}
					}
				}

				double additionaldmg = 0;
				if(doubleedgedsword)
				{
					if(source.CombatType == GameObjectCombatType.Melee)
					{
						additionaldmg = dmg * 0.02;
					}
					else
					{
						additionaldmg = dmg * 0.015;
					}
				}
				if(havoc)
				{
					additionaldmg += dmg * 0.03;
				}
				if(executioner > 0)
				{
					if(executioner == 1)
					{
						if((enemy.Health / enemy.MaxHealth) * 100 < 20)
						{
							additionaldmg += dmg * 0.05;
						}
					}
					else if(executioner == 2)
					{
						if((enemy.Health / enemy.MaxHealth) * 100 < 35)
						{
							additionaldmg += dmg * 0.05;
						}
					}
					else if(executioner == 3)
					{
						if((enemy.Health / enemy.MaxHealth) * 100 < 50)
						{
							additionaldmg += dmg * 0.05;
						}
					}
				}

				double newspellblock = enemy.SpellBlock * source.PercentMagicPenetrationMod;
				double dmgreduction = 100 / (100 + newspellblock - source.FlatMagicPenetrationMod);
				return (((dmg + additionaldmg) * dmgreduction));
			}
		}
	}
}
