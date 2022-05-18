using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria.UI;

namespace CustomBanksLib
{
    public abstract class ModBank : ModType
    {
        public int Chest { get; internal set; }
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
        public abstract Chest GetBank(Player player);

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
            Chest chest = GetBank(player);
            for (int j = 0; j < 40; j++)
            {
                if (chest.item[j].stack > 0 && chest.item[j].type == ItemID.DD2EnergyCrystal)
                {
                    chest.item[j].TurnToAir();
                }
            }
        }
    }
}