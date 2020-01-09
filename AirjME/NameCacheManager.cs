using System;

namespace AirjME
{
    public class NameCacheManager
    {
        public WowProcess process;
        public NameCacheManager(WowProcess process)
        {
            this.process = process;
        }
        private IntPtr FindInList(IntPtr startPointer, int nextOffset, Func<IntPtr, bool> checkFunction)
        {
            var currentPointer = process.Read<IntPtr>(startPointer);
            while (currentPointer != startPointer && currentPointer != IntPtr.Zero)
            {
                if (checkFunction(currentPointer))
                {
                    return currentPointer;
                }

                currentPointer = process.Read<IntPtr>(currentPointer + nextOffset);
            }

            return IntPtr.Zero;
        }

        public string FindPlayerNameByGuid(Guid guid)
        {
            var nameBaseAddress = process.BaseAddress + process.GetOffset("nameCache"); // ;
            var playerNameObject = FindInList(nameBaseAddress, 0, p => process.Read<Guid>(p + 0x20) == guid);
            if (playerNameObject != IntPtr.Zero)
            {
                return process.ReadString(playerNameObject + 0x31);
            }

            return "";
        }
    }
}