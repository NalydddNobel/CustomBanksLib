using System.Collections.Generic;
using System.Reflection;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace CustomBanksLib
{
	public class CustomBanksLib : Mod
	{
		public static int ModBanksCount { get; internal set; }
		internal static List<ModBank> registeredBanks;

		private static MethodInfo Player_RemoveAnglerAccOptionsFromRewardPool;

		public override void Load()
        {
			ModBanksCount = 0;
			registeredBanks = new List<ModBank>();
			Player_RemoveAnglerAccOptionsFromRewardPool = typeof(Player).GetMethod("RemoveAnglerAccOptionsFromRewardPool", BindingFlags.NonPublic | BindingFlags.Instance);
			On.Terraria.Player.PurgeDD2EnergyCrystals += Player_PurgeDD2EnergyCrystals;
            On.Terraria.Player.DropAnglerAccByMissing += Player_DropAnglerAccByMissing;
            On.Terraria.UI.ChestUI.Draw += ChestUI_Draw;
            On.Terraria.Main.DrawTrashItemSlot += Main_DrawTrashItemSlot;
            On.Terraria.Main.DrawBestiaryIcon += Main_DrawBestiaryIcon;
            On.Terraria.Main.DrawEmoteBubblesButton += Main_DrawEmoteBubblesButton;
            On.Terraria.Main.DrawInventory += Main_DrawInventory;
            //IL.Terraria.Player.BuyItem += Player_BuyItem;
        }

        private void Main_DrawInventory(On.Terraria.Main.orig_DrawInventory orig, Main self)
		{
			int oldChest = Main.LocalPlayer.chest;
			orig(self);
			Main.LocalPlayer.chest = oldChest;
        }

        private void Main_DrawEmoteBubblesButton(On.Terraria.Main.orig_DrawEmoteBubblesButton orig, int pivotTopLeftX, int pivotTopLeftY)
        {
			int oldChest = Main.LocalPlayer.chest;
			if (Main.LocalPlayer.GetModPlayer<CustomBanksPlayer>().customBank > -1)
			{
				Main.LocalPlayer.chest = 0;
			}
			orig(pivotTopLeftX, pivotTopLeftY);
			Main.LocalPlayer.chest = oldChest;
		}

		private void Main_DrawBestiaryIcon(On.Terraria.Main.orig_DrawBestiaryIcon orig, int pivotTopLeftX, int pivotTopLeftY)
        {
			int oldChest = Main.LocalPlayer.chest;
			if (Main.LocalPlayer.GetModPlayer<CustomBanksPlayer>().customBank > -1)
            {
				Main.LocalPlayer.chest = 0;
            }
			orig(pivotTopLeftX, pivotTopLeftY);
			Main.LocalPlayer.chest = oldChest;
		}

		private void Main_DrawTrashItemSlot(On.Terraria.Main.orig_DrawTrashItemSlot orig, int pivotTopLeftX, int pivotTopLeftY)
        {
			if (Main.LocalPlayer.GetModPlayer<CustomBanksPlayer>().customBank > -1)
            {
				Main.trashSlotOffset = new Point16(5, 168);
			}
			orig(pivotTopLeftX, pivotTopLeftY);
        }

        private void ChestUI_Draw(On.Terraria.UI.ChestUI.orig_Draw orig, Microsoft.Xna.Framework.Graphics.SpriteBatch spritebatch)
        {
			if (!Main.recBigList)
            {
				int customBank = Main.LocalPlayer.GetModPlayer<CustomBanksPlayer>().customBank;
				if (customBank != -1)
                {
					var b = registeredBanks[customBank];

					if (b.CloseWhenVanillaChestIsOpened && Main.LocalPlayer.chest != -1)
                    {
						b.OnClose(Main.LocalPlayer);
						goto Origin;
                    }

					b.DrawUI(spritebatch);
					if (b.DisableVanillaChestUI)
                    {
						if (Main.LocalPlayer.GetModPlayer<CustomBanksPlayer>().customBank > -1)
						{
							Main.LocalPlayer.chest = 0;
						}
						return;
                    }
                }
            }
		Origin:
			orig(spritebatch);
        }

        private bool Player_DropAnglerAccByMissing(On.Terraria.Player.orig_DropAnglerAccByMissing orig, Player self, List<int> itemIdsOfAccsWeWant, int randomChanceForASingleAcc, out bool botheredRollingForADrop, out int itemIdToDrop)
        {
			foreach (var b in registeredBanks)
            {
				if (b.CheckForAnglerOptionsRemoval)
                {
					var chest = b.GetBank(self);
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
			bank.Type = ModBanksCount;
			registeredBanks.Add(bank);
			ModBanksCount++;
			return ModBanksCount - 1;
        }
		/// <summary>
		/// 
		/// </summary>
		/// <param name="player"></param>
		/// <param name="type">The chest 'ID'</param>
		/// <returns>A ModBank using a chest. Returns null if one doesn't exist</returns>
		public static ModBank GetBank(Player player, int type)
		{
			return (type > 0 && type < ModBanksCount) ? registeredBanks[type] : null;
		}

		/// <summary>
		/// Sets the player's bank to a ModBank chest 'ID'
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="player"></param>
		public static void SetBank<T>(Player player) where T : ModBank
        {
			player.GetModPlayer<CustomBanksPlayer>().customBank = ModContent.GetInstance<ModBank>().Type;
        }

		public class TestModBank : ModBank
        {
			public override string BankName => "Balls";
            public override Item[] GetBank(Player player)
            {
				return player.bank4.item;
            }
        }
    }
}