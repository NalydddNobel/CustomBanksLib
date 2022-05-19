using Terraria;
using Terraria.ModLoader;

namespace CustomBanksLib
{
    public class CustomBanksPlayer : ModPlayer
    {
        internal int customBank = -1;

        public int CustomBank => customBank;

        public void SetBank(int bank, bool closeVanillaChest = true)
        {
            if (customBank == bank)
                return;
            if (closeVanillaChest)
            {
                Player.chest = -1;
            }
            if (customBank > -1)
            {
                CustomBanksLib.registeredBanks[customBank].OnClose(Player);
            }
            customBank = bank;
            if (customBank > -1)
            {
                CustomBanksLib.registeredBanks[customBank].OnOpen(Player);
            }
        }

        public override void Initialize()
        {
            customBank = -1;
        }

        public override void SetControls()
        {
            if (Main.myPlayer != Player.whoAmI)
            {
                return;
            }
            bool resetBank = !Main.playerInventory;
            if ((Player.controlInv && customBank > -1))
            {
                resetBank = true;
                Player.releaseInventory = false;
            }
            if (resetBank)
                SetBank(-1, closeVanillaChest: false);
        }

        public override void PostUpdate()
        {
            if (Player.controlHook)
            {
                SetBank(0);
            }
        }
    }
}