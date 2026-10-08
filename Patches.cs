using HarmonyLib;
using Il2Cpp;

namespace NullList
{
    [HarmonyPatch(typeof(WhoKilledWho), "OnPlayerAddedHandler", [typeof(PlayerInfo)])]
    public static class OnPlayerJoinedRoomPatch
    {
        [HarmonyPrefix]
        public static void Prefix(PlayerInfo otherPlayer)
        {
            string nickName = otherPlayer.EKINODNOMKE;
            bool playerInBlacklist = Core.CheckPlayer(nickName);

            if (playerInBlacklist && Core.IsHost)
            {
                Core.Kick(otherPlayer);
            }
        }
    }
}
