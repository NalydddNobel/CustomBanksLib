using System.Collections.Generic;
using System.Reflection;
using Terraria;
using Terraria.ModLoader;

namespace CustomBanksLib
{
	public class CustomBanksLib : Mod
	{
		public const int FirstBank = -2;
		public const int VanillaBanks = -5;
		public const int VanillaBanksCount = 4;
		public static int ModBanks { get; internal set; }
		public static int ModBanksCount { get; internal set; }
		private static List<ModBank> registeredBanks;

		private static MethodInfo Player_RemoveAnglerAccOptionsFromRewardPool;

		public override void Load()
        {
			ModBanksCount = VanillaBanksCount;
			ModBanks = VanillaBanks;
			registeredBanks = new List<ModBank>();
			Player_RemoveAnglerAccOptionsFromRewardPool = typeof(Player).GetMethod("RemoveAnglerAccOptionsFromRewardPool", BindingFlags.NonPublic | BindingFlags.Instance);
			On.Terraria.Player.PurgeDD2EnergyCrystals += Player_PurgeDD2EnergyCrystals;
            On.Terraria.Player.DropAnglerAccByMissing += Player_DropAnglerAccByMissing;
            //IL.Terraria.Player.BuyItem += Player_BuyItem;
        }

        private bool Player_DropAnglerAccByMissing(On.Terraria.Player.orig_DropAnglerAccByMissing orig, Player self, List<int> itemIdsOfAccsWeWant, int randomChanceForASingleAcc, out bool botheredRollingForADrop, out int itemIdToDrop)
        {
			foreach (var b in registeredBanks)
            {
				if (b.CheckForAnglerOptionsRemoval)
                {
					var chest = b.GetBank(self).item;
					for (int i = 0; i < chest.Length; i++)
					{
						Player_RemoveAnglerAccOptionsFromRewardPool.Invoke(self, new object[] { itemIdsOfAccsWeWant, chest[i] });
					}
				}
			}
			return orig(self, itemIdsOfAccsWeWant, randomChanceForASingleAcc, out botheredRollingForADrop, out itemIdToDrop);
		}

		//private static void Player_BuyItem(MonoMod.Cil.ILContext il)
		//{
		//}

		private void Player_PurgeDD2EnergyCrystals(On.Terraria.Player.orig_PurgeDD2EnergyCrystals orig, Player self)
        {
			orig(self);
			foreach (var b in registeredBanks)
            {
				b.PurgeDD2EnergyCrystals(self);
            }
        }

        public static int RegisterBank(ModBank bank)
        {
			bank.Chest = ModBanks;
			bank.Type = ModBanksCount;
			registeredBanks.Add(bank);

			ModBanks--;
			ModBanksCount++;
			return ModBanksCount - 1;
        }
		/// <summary>
		/// 
		/// </summary>
		/// <param name="player"></param>
		/// <param name="chest">The chest 'ID'</param>
		/// <returns>A ModBank using a chest. Returns null if one doesn't exist</returns>
		public static ModBank GetBankFromChest(Player player, int chest)
		{
			return (chest > 0 && chest < ModBanksCount - VanillaBanksCount) ? registeredBanks[BankType(chest)] : null;
		}
		/// <summary>
		/// Expects a negative number below <see cref="VanillaBanks"/> (-5). Usually <see cref="Player.chest"/>
		/// </summary>
		/// <param name="chest"></param>
		/// <returns></returns>
		public static int BankType(int chest)
		{
			return -chest + (VanillaBanks - 1);
		}

		/// <summary>
		/// Sets the player's bank to a ModBank chest 'ID'
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="player"></param>
		public static void SetBank<T>(Player player) where T : ModBank
        {
			player.chest = ModContent.GetInstance<ModBank>().Chest;
        }

		public class TestModBank : ModBank
        {
			public override string BankName => "Balls";
            public override Chest GetBank(Player player)
            {
				return player.bank4;
            }
        }
    }
}