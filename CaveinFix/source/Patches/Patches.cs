using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace CaveinFix.Patches;

public class CaveinFixPatches : ModSystem
{
    private Harmony _patcher;

    public static ICoreServerAPI _api;

    public override void StartServerSide(ICoreServerAPI api)
    {
        _api = api;

        _patcher = new Harmony(Mod.Info.ModID);
        _patcher.PatchAll();
    }

    public override void AssetsFinalize(ICoreAPI api)
    {
        if (api.Side != EnumAppSide.Server)
        {
            return;
        }
    }

    public override void Dispose()
    {
        _patcher?.UnpatchAll(Mod.Info.ModID);
        base.Dispose();
    }
}

internal static class Patches
{
    [HarmonyPatch(typeof(BlockBehaviorUnstableRock))]
    public static class UnstableRockPatch
    {
        [HarmonyPatch("searchCollapsible")]
        [HarmonyPostfix]
        public static void SearchCollapsiblePostfix(
            BlockBehaviorUnstableRock __instance,
            BlockPos startPos,
            ICoreAPI ___api,
            ref CollapsibleSearchResult __result
        )
        {
            BlockPos belowPos = startPos.DownCopy();
            Block blockBelow = ___api.World.BlockAccessor.GetBlock(belowPos, BlockLayersAccess.Solid);

            if (blockBelow.BlockId != 0 && blockBelow.SideIsSolid(___api.World.BlockAccessor, belowPos, BlockFacing.UP.Index))
            {
                bool foundVerticalSupport = false;
                double stabilityBelow = 0;

                if (blockBelow.HasBehavior<BlockBehaviorUnstableRock>())
                {
                    stabilityBelow = blockBelow.GetBehavior<BlockBehaviorUnstableRock>().getInstability(belowPos);

                    if (stabilityBelow < 1.0) foundVerticalSupport = true;
                }
                else
                {
                    foundVerticalSupport = true;
                    stabilityBelow = 0;
                }

                if (foundVerticalSupport)
                {
                    __result.Instability = Math.Min(__result.Instability, (float)stabilityBelow);

                    __result.Unconnected = false;
                }
            }
        }

        [HarmonyPatch("getInstability")]
        [HarmonyPrefix]
        public static bool GetInstabilityPrefix(
            BlockBehaviorUnstableRock __instance,
            BlockPos pos,
            ref double __result
        )
        {
            var method = AccessTools.Method(typeof(BlockBehaviorUnstableRock), "searchCollapsible");
            var res = (CollapsibleSearchResult)method.Invoke(__instance, new object[] { pos, false });

            __result = res.Instability;

            return false;
        }
    }
}
