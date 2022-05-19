using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Audio;
using Terraria.UI.Chat;
using Terraria.GameContent;
using System.Collections.Generic;
using Terraria.GameInput;
using Terraria.UI.Gamepad;
using Terraria.Localization;
using Terraria.DataStructures;
using System;
using System.Reflection;

namespace CustomBanksLib
{
    public abstract class ModBank : ModType
    {
        public int Type { get; internal set; }
        public abstract string BankName { get; }

        /// <summary>
        /// Whether or not this bank will have its items checked for removing certain angler rewards. Weird vanilla feature I know.
        /// </summary>
        public virtual bool CheckForAnglerOptionsRemoval => true;
        /// <summary>
        /// What slot context this bank uses. Defaults to <see cref="ItemSlot.Context.BankItem"/>
        /// </summary>
        public virtual int ItemSlotContext => ItemSlot.Context.BankItem;
        public virtual bool DisableVanillaChestUI => true;
        public virtual bool CloseWhenVanillaChestIsOpened => true;

        protected sealed override void Register()
        {
        }

        public sealed override void SetupContent()
        {
            CustomBanksLib.RegisterBank(this);
            SetStaticDefaults();
        }

        /// <summary>
        /// Use this method to return a saved chest. Most likely usage: player.GetModPlayer{MyPlayer}().myBank where myBank is <see cref="Terraria.Chest"/>
        /// </summary>
        /// <param name="player"></param>
        /// <returns>A chest instance</returns>
        public abstract Item[] GetBank(Player player);

        public virtual void OnOpen(Player player)
        {

        }
        public virtual void OnClose(Player player)
        {
            if (Main.myPlayer == player.whoAmI)
            {
				Main.trashSlotOffset = Point16.Zero;
				SoundEngine.PlaySound(SoundID.MenuClose);
			}
		}

        public virtual void DrawUI(SpriteBatch spriteBatch)
        {
            var textColor = Color.White * (1f - (255f - Main.mouseTextColor) / 255f * 0.5f);
            textColor.A = 255;
            DrawName(spriteBatch, BankName, textColor);
			DrawButtons(spriteBatch);
			int length = GetBank(Main.LocalPlayer).Length;

			var player = Main.LocalPlayer;
			Item[] inv = GetBank(player);
			Main.inventoryScale = 0.755f;
			if (Utils.FloatIntersect(Main.mouseX, Main.mouseY, 0f, 0f, 73f, Main.instance.invBottom, 560f * Main.inventoryScale, 224f * Main.inventoryScale) && !PlayerInput.IgnoreMouseInterface)
			{
				player.mouseInterface = true;
			}
			for (int i = 0; i < length; i++)
            {
                DrawSlot(spriteBatch, player, inv, ItemSlot.Context.BankItem, i);
            }
        }

        public virtual void DrawName(SpriteBatch spriteBatch, string text, Color color)
        {
            ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.MouseText.Value, text, 
                new Vector2(504f, (float)(Main.instance.invBottom)), color, 0f, Vector2.Zero, Vector2.One, -1f, 1.5f);
        }

        public virtual void DrawButtons(SpriteBatch spriteBatch)
        {
			for (int i = 0; i < ChestUI.ButtonID.Count; i++)
            {
				DrawButton(spriteBatch, i, 506, Main.instance.invBottom + 40);
            }
		}
		public virtual void DrawButton(SpriteBatch spriteBatch, int ID, float X, float Y)
        {
			Player player = Main.player[Main.myPlayer];
			if ((ID == ChestUI.ButtonID.RenameChest) || (ID == ChestUI.ButtonID.RenameChestCancel && !Main.editChest))
			{
				ChestUI.UpdateHover(ID, hovering: false);
				return;
			}
			if (ID == ChestUI.ButtonID.ToggleVacuum && player.chest != -5)
			{
				ChestUI.UpdateHover(ID, hovering: false);
				return;
			}
			int num = ID;
			if (ID == 7)
			{
				num = 5;
			}
			Y += num * 26;
			float num2 = ChestUI.ButtonScale[ID];
			string text = "";
			switch (ID)
			{
				case 0:
					text = Lang.inter[29].Value;
					break;
				case 1:
					text = Lang.inter[30].Value;
					break;
				case 2:
					text = Lang.inter[31].Value;
					break;
				case 3:
					text = Lang.inter[82].Value;
					break;
				case 5:
					text = Lang.inter[Main.editChest ? 47 : 61].Value;
					break;
				case 6:
					text = Lang.inter[63].Value;
					break;
				case 4:
					text = Lang.inter[122].Value;
					break;
				//case 7:
				//	text = ((!player.IsVoidVaultEnabled) ? Language.GetTextValue("UI.ToggleBank4VacuumIsOff") : Language.GetTextValue("UI.ToggleBank4VacuumIsOn"));
				//	break;
			}
			Vector2 value = FontAssets.MouseText.Value.MeasureString(text);
			Color color = new Color((int)Main.mouseTextColor, Main.mouseTextColor, (int)Main.mouseTextColor, (int)Main.mouseTextColor) * num2;
			color = Color.White * 0.97f * (1f - (255f - Main.mouseTextColor) / 255f * 0.5f);
			color.A = byte.MaxValue;
			X += (int)(value.X * num2 / 2f);
			bool flag = Utils.FloatIntersect(Main.mouseX, Main.mouseY, 0f, 0f, (float)X - value.X / 2f, Y - 12, value.X, 24f);
			if (ChestUI.ButtonHovered[ID])
			{
				flag = Utils.FloatIntersect(Main.mouseX, Main.mouseY, 0f, 0f, (float)X - value.X / 2f - 10f, Y - 12, value.X + 16f, 24f);
			}
			if (flag)
			{
				color = Main.OurFavoriteColor;
			}
			ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.MouseText.Value, text, new Vector2((float)X, (float)Y), color, 0f, value / 2f, new Vector2(num2), -1f, 1.5f);
			value *= num2;
			switch (ID)
			{
				case 0:
					UILinkPointNavigator.SetPosition(500, new Vector2((float)X - value.X * num2 / 2f * 0.8f, (float)Y));
					break;
				case 1:
					UILinkPointNavigator.SetPosition(501, new Vector2((float)X - value.X * num2 / 2f * 0.8f, (float)Y));
					break;
				case 2:
					UILinkPointNavigator.SetPosition(502, new Vector2((float)X - value.X * num2 / 2f * 0.8f, (float)Y));
					break;
				case 5:
					UILinkPointNavigator.SetPosition(504, new Vector2((float)X, (float)Y));
					break;
				case 6:
					UILinkPointNavigator.SetPosition(504, new Vector2((float)X, (float)Y));
					break;
				case 3:
					UILinkPointNavigator.SetPosition(503, new Vector2((float)X - value.X * num2 / 2f * 0.8f, (float)Y));
					break;
				case 4:
					UILinkPointNavigator.SetPosition(505, new Vector2((float)X - value.X * num2 / 2f * 0.8f, (float)Y));
					break;
				case 7:
					UILinkPointNavigator.SetPosition(506, new Vector2((float)X - value.X * num2 / 2f * 0.8f, (float)Y));
					break;
			}
			if (!flag)
			{
				ChestUI.UpdateHover(ID, hovering: false);
				return;
			}
			ChestUI.UpdateHover(ID, hovering: true);
			if (PlayerInput.IgnoreMouseInterface)
			{
				return;
			}
			player.mouseInterface = true;
			if (Main.mouseLeft && Main.mouseLeftRelease)
			{
				switch (ID)
				{
					case 0:
						LootAll(Main.LocalPlayer);
						break;
					case 1:
						DepositAll(Main.LocalPlayer);
						break;
					case 2:
						QuickStack(Main.LocalPlayer);
						break;
					//case 5:
					//	RenameChest();
					//	break;
					//case 6:
					//	RenameChestCancel();
					//	break;
					case 3:
						Restock(Main.LocalPlayer);
						break;
					case 4:
						SortChest(Main.LocalPlayer);
						break;
					//case 7:
					//	Main.LocalPlayer.IsVoidVaultEnabled = !Main.LocalPlayer.IsVoidVaultEnabled;
					//	break;
				}
				Recipe.FindRecipes();
			}
		}
		public virtual void LootAll(Player player)
        {
			var lootAllSettings = GetItemSettings.LootAllSettings;
			var bank = GetBank(player);
			for (int i = 0; i < bank.Length; i++)
			{
				if (bank[i].type > ItemID.None)
				{
					bank[i].position = player.Center;
					bank[i] = player.GetItem(Main.myPlayer, bank[i], lootAllSettings);
					if (Main.netMode == NetmodeID.MultiplayerClient)
					{
						//NetMessage.SendData(MessageID.SyncChestItem, -1, -1, null, player.chest, i);
					}
				}
			}
		}
		public virtual void DepositAll(Player player)
        {
			var inv = player.inventory;
			var bank = GetBank(player);
			for (int i = 49; i >= 10; i--)
			{
				if (inv[i].stack > 0 && inv[i].type > ItemID.None && !inv[i].favorited)
				{
					if (inv[i].maxStack > 1)
					{
						for (int j = 0; j < bank.Length; j++)
						{
							if (bank[j].stack >= bank[j].maxStack || !AreTheSame(inv[i], bank[j]) || !ItemLoader.CanStack(bank[j], inv[i]))
							{
								continue;
							}
							int num2 = inv[i].stack;
							if (inv[i].stack + bank[j].stack > bank[j].maxStack)
							{
								num2 = bank[j].maxStack - bank[j].stack;
							}
							inv[i].stack -= num2;
							bank[j].stack += num2;
							SoundEngine.PlaySound(SoundID.Grab);
							if (inv[i].stack <= 0)
							{
								inv[i].SetDefaults();
								if (Main.netMode == NetmodeID.MultiplayerClient)
								{
									//NetMessage.SendData(MessageID.SyncChestItem, -1, -1, null, player.chest, j);
								}
								break;
							}
							if (bank[j].type == ItemID.None)
							{
								bank[j] = inv[i].Clone();
								inv[i].SetDefaults();
							}
							if (Main.netMode == NetmodeID.MultiplayerClient)
							{
								//NetMessage.SendData(MessageID.SyncChestItem, -1, -1, null, player.chest, j);
							}
						}
					}
					if (inv[i].stack > 0)
					{
						for (int j = 0; j < bank.Length; j++)
						{
							if (bank[j].stack == 0)
							{
								SoundEngine.PlaySound(SoundID.Grab);
								bank[j] = inv[i].Clone();
								inv[i].SetDefaults();
								if (Main.netMode == NetmodeID.MultiplayerClient)
								{
									//NetMessage.SendData(MessageID.SyncChestItem, -1, -1, null, player.chest, j);
								}
								break;
							}
						}
					}
				}
			}
		}
		public virtual void QuickStack(Player player)
        {
			var inventory = player.inventory;
			var item = GetBank(player);
			ChestUI.MoveCoins(inventory, item);
			List<int> list = new List<int>();
			List<int> list2 = new List<int>();
			List<int> list3 = new List<int>();
			Dictionary<int, int> dictionary = new Dictionary<int, int>();
			List<int> list4 = new List<int>();
			bool[] array = new bool[item.Length];
			for (int i = 0; i < 40; i++)
			{
				if (item[i].type > ItemID.None && item[i].stack > 0 && (item[i].type < ItemID.CopperCoin || item[i].type > ItemID.PlatinumCoin))
				{
					list2.Add(i);
					list.Add(item[i].netID);
				}
				if (item[i].type == ItemID.None || item[i].stack <= 0)
				{
					list3.Add(i);
				}
			}
			int num = 50;
			if (player.chest <= -2)
			{
				num += 4;
			}
			for (int j = 10; j < num; j++)
			{
				if (list.Contains(inventory[j].netID) && !inventory[j].favorited)
				{
					dictionary.Add(j, inventory[j].netID);
				}
			}
			for (int k = 0; k < list2.Count; k++)
			{
				int num2 = list2[k];
				int netID = item[num2].netID;
				foreach (KeyValuePair<int, int> item2 in dictionary)
				{
					if (item2.Value == netID && inventory[item2.Key].netID == netID && ItemLoader.CanStack(item[num2], inventory[item2.Key]))
					{
						int num3 = inventory[item2.Key].stack;
						int num4 = item[num2].maxStack - item[num2].stack;
						if (num4 == 0)
						{
							break;
						}
						if (num3 > num4)
						{
							num3 = num4;
						}
						SoundEngine.PlaySound(SoundID.Grab);
						item[num2].stack += num3;
						inventory[item2.Key].stack -= num3;
						if (inventory[item2.Key].stack == 0)
						{
							inventory[item2.Key].SetDefaults();
						}
						array[num2] = true;
					}
				}
			}
			foreach (KeyValuePair<int, int> item3 in dictionary)
			{
				if (inventory[item3.Key].stack == 0)
				{
					list4.Add(item3.Key);
				}
			}
			foreach (int item4 in list4)
			{
				dictionary.Remove(item4);
			}
			for (int l = 0; l < list3.Count; l++)
			{
				int num5 = list3[l];
				bool flag = true;
				int num6 = item[num5].netID;
				if (num6 >= 71 && num6 <= 74)
				{
					continue;
				}
				foreach (KeyValuePair<int, int> item5 in dictionary)
				{
					if (((item5.Value != num6 || inventory[item5.Key].netID != num6) && (!flag || inventory[item5.Key].stack <= 0)) || !ItemLoader.CanStack(item[num5], inventory[item5.Key]))
					{
						continue;
					}
					SoundEngine.PlaySound(SoundID.Grab);
					if (flag)
					{
						num6 = item5.Value;
						item[num5] = inventory[item5.Key];
						inventory[item5.Key] = new Item();
					}
					else
					{
						int num7 = inventory[item5.Key].stack;
						int num8 = item[num5].maxStack - item[num5].stack;
						if (num8 == 0)
						{
							break;
						}
						if (num7 > num8)
						{
							num7 = num8;
						}
						item[num5].stack += num7;
						inventory[item5.Key].stack -= num7;
						if (inventory[item5.Key].stack == 0)
						{
							inventory[item5.Key] = new Item();
						}
					}
					array[num5] = true;
					flag = false;
				}
			}
			if (Main.netMode == NetmodeID.MultiplayerClient && player.chest >= 0)
			{
				for (int m = 0; m < array.Length; m++)
				{
					//NetMessage.SendData(MessageID.SyncChestItem, -1, -1, null, player.chest, m);
				}
			}
			list.Clear();
			list2.Clear();
			list3.Clear();
			dictionary.Clear();
			list4.Clear();
		}
		public virtual void Restock(Player player)
        {
			var inventory = player.inventory;
			var item = GetBank(player);

			var hashSet = new HashSet<int>();
			var list = new List<int>();
			var list2 = new List<int>();
			for (int i = 57; i >= 0; i--)
			{
				if ((i < 50 || i >= 54) && (inventory[i].type < ItemID.CopperCoin || inventory[i].type > ItemID.PlatinumCoin))
				{
					if (inventory[i].stack > 0 && inventory[i].maxStack > 1)
					{
						hashSet.Add(inventory[i].netID);
						if (inventory[i].stack < inventory[i].maxStack)
						{
							list.Add(i);
						}
					}
					else if (inventory[i].stack == 0 || inventory[i].netID == 0 || inventory[i].type == ItemID.None)
					{
						list2.Add(i);
					}
				}
			}
			bool flag = false;
			for (int i = 0; i < item.Length; i++)
			{
				if (item[i].stack < 1 || !hashSet.Contains(item[i].netID))
				{
					continue;
				}
				bool flag2 = false;
				for (int j = 0; j < list.Count; j++)
				{
					int num2 = list[j];
					int context = 0;
					if (num2 >= 50)
					{
						context = 2;
					}
					if (inventory[num2].netID != item[i].netID || ItemSlot.PickItemMovementAction(inventory, context, num2, item[i]) == -1 || !ItemLoader.CanStack(inventory[num2], item[i]))
					{
						continue;
					}
					int num3 = item[i].stack;
					if (inventory[num2].maxStack - inventory[num2].stack < num3)
					{
						num3 = inventory[num2].maxStack - inventory[num2].stack;
					}
					inventory[num2].stack += num3;
					item[i].stack -= num3;
					flag = true;
					if (inventory[num2].stack == inventory[num2].maxStack)
					{
						if (Main.netMode == NetmodeID.MultiplayerClient && Main.player[Main.myPlayer].chest > -1)
						{
							//NetMessage.SendData(MessageID.SyncChestItem, -1, -1, null, Main.player[Main.myPlayer].chest, i);
						}
						list.RemoveAt(j);
						j--;
					}
					if (item[i].stack == 0)
					{
						item[i] = new Item();
						flag2 = true;
						if (Main.netMode == NetmodeID.MultiplayerClient && Main.player[Main.myPlayer].chest > -1)
						{
							//NetMessage.SendData(MessageID.SyncChestItem, -1, -1, null, Main.player[Main.myPlayer].chest, i);
						}
						break;
					}
				}
				if (flag2 || list2.Count <= 0 || item[i].ammo == 0)
				{
					continue;
				}
				for (int k = 0; k < list2.Count; k++)
				{
					int context2 = 0;
					if (list2[k] >= 50)
					{
						context2 = 2;
					}
					if (ItemSlot.PickItemMovementAction(inventory, context2, list2[k], item[i]) != -1)
					{
						Utils.Swap(ref inventory[list2[k]], ref item[i]);
						if (Main.netMode == NetmodeID.MultiplayerClient && Main.player[Main.myPlayer].chest > -1)
						{
							NetMessage.SendData(MessageID.SyncChestItem, -1, -1, null, Main.player[Main.myPlayer].chest, i);
						}
						list.Add(list2[k]);
						list2.RemoveAt(k);
						flag = true;
						break;
					}
				}
			}
			if (flag)
			{
				SoundEngine.PlaySound(SoundID.Grab);
			}
		}
		public virtual void SortChest(Player player)
        {
			var bank = GetBank(player);

			var preSort = new Tuple<int, int, int>[bank.Length];
			for (int j = 0; j < 40; j++)
			{
				preSort[j] = Tuple.Create(bank[j].netID, bank[j].stack, bank[j].prefix);
			}

			int oldChest = Main.LocalPlayer.chest;
			Main.LocalPlayer.chest = 0;
			typeof(ItemSorting).GetMethod("Sort", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, new object[] { bank, new int[0], }); // zzz
			Main.LocalPlayer.chest = oldChest;
			var afterSort = new Tuple<int, int, int>[bank.Length];
			for (int j = 0; j < 40; j++)
			{
				afterSort[j] = Tuple.Create(bank[j].netID, bank[j].stack, bank[j].prefix);
			}

			if (Main.netMode != NetmodeID.MultiplayerClient)
			{
				return;
			}

			for (int k = 0; k < 40; k++)
			{
				if (afterSort[k] != preSort[k])
				{
					//NetMessage.SendData(32, -1, -1, null, Main.player[Main.myPlayer].chest, k);
				}
			}
		}

		public virtual bool AreTheSame(Item item, Item comparisonItem)
        {
			if (item.netID == comparisonItem.netID)
			{
				return item.type == comparisonItem.type;
			}
			return false;
		}

		public virtual void DrawSlot(SpriteBatch spriteBatch, Player player, Item[] inv, int context, int i)
        {
			int x = i % 10;
			int y = i / 10;
			int num = (int)(73f + (float)(x * 56) * Main.inventoryScale);
			int num2 = (int)(Main.instance.invBottom + (float)(y * 56) * Main.inventoryScale);
			int slot = x + y * 10;
			new Color(100, 100, 100, 100);
			if (Utils.FloatIntersect(Main.mouseX, Main.mouseY, 0f, 0f, num, num2, TextureAssets.InventoryBack.Width() * Main.inventoryScale, (float)TextureAssets.InventoryBack.Height() * Main.inventoryScale) && !PlayerInput.IgnoreMouseInterface)
			{
				player.mouseInterface = true;
				ItemSlot.Handle(inv, context, slot);
			}
			ItemSlot.Draw(spriteBatch, inv, context, slot, new Vector2(num, num2));
		}
		public virtual void HandleSlot(Rectangle hoverRect)
        {

        }

        /// <summary>
        /// Whether or not this bank will contribute to purchases if there is money in it. This feature currently doesn't work, and ModBanks will not contribute money. Defaults to true
        /// </summary>
        public virtual bool ContributesMoney(Player player, int price, int customCurrency) => true;

        /// <summary>
        /// Purges all items of type <see cref="ItemID.DD2EnergyCrystal"/>. Can be overridden to prevent this, or provide a different function
        /// </summary>
        /// <param name="player"></param>
        public virtual void PurgeDD2EnergyCrystals(Player player)
        {
            var chest = GetBank(player);
            for (int j = 0; j < 40; j++)
            {
                if (chest[j].stack > 0 && chest[j].type == ItemID.DD2EnergyCrystal)
                {
                    chest[j].TurnToAir();
                }
            }
        }

        /// <summary>
        /// See <see cref="ChestUI.GetContainerUsageInfo(out bool, out Item[])"/>
        /// </summary>
        /// <param name="sync">Seeminly only used for real chests, not banks. So do not use.</param>
        /// <param name="chestInv"></param>
        public virtual void GetContainerUsageInfo(ref bool sync, ref Item[] chestInv)
        {
            chestInv = GetBank(Main.LocalPlayer);
        }
    }
}