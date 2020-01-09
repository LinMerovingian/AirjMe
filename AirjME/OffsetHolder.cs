using System;
using System.Collections.Generic;

namespace AirjME
{
    public class OffsetHolder
    {
        public WowProcess process;
        public OffsetHolder(WowProcess process)
        {
            this.process = process;
        }
        private readonly Dictionary<string, int> offsetDict = new Dictionary<string, int>();

        public void AddOffsetIfNotExists(string name, Func<int> function)
        {
            if (!offsetDict.ContainsKey(name))
            {
                var offset = function();
                if (offset != 0)
                {
                    offsetDict.Add(name, offset);
                }
            }
        }

        public void InitializeOffsets()
        {
            // 32877

            offsetDict.Add("tainted", 0x26C7C28);
            offsetDict.Add("hardware", 0x22BCD20);
            offsetDict.Add("inGame", 0x259ECD8);
            offsetDict.Add("objectManager", 0x0237cb28);
            offsetDict.Add("nameCache", 0x1FA5B18);
            offsetDict.Add("playerGuid", 0x267D640);
            offsetDict.Add("currentTime", 0xEA1AC1);
            offsetDict.Add("mouseoverGuid", 0x259ECE0);
            offsetDict.Add("objectTypeToFlag", 0x1C6D4C0);
            offsetDict.Add("isHarm", 0xAF123D);
            offsetDict.Add("isHelp", 0x8ACDF0);
            offsetDict.Add("faction", 0x8C9290);
            offsetDict.Add("interact", 0xD68A60);

            offsetDict.Add("moveStart_4", 0x10A5A10);
            offsetDict.Add("moveStart_5", 0x10A5B20);
            offsetDict.Add("moveStart_6", 0x10A5DF0);
            offsetDict.Add("moveStart_7", 0x10A5F00);
            offsetDict.Add("moveStart_8", 0x10A5C30);
            offsetDict.Add("moveStart_9", 0x10A5D10);

            offsetDict.Add("moveStop_4", 0x10A5AB0);
            offsetDict.Add("moveStop_5", 0x10A5BC0);
            offsetDict.Add("moveStop_6", 0x10A5E90);
            offsetDict.Add("moveStop_7", 0x10A5FA0);
            offsetDict.Add("moveStop_8", 0x10A5CA0);
            offsetDict.Add("moveStop_9", 0x10A5D80);

            offsetDict.Add("moveBase", 0x25EED98);
            offsetDict.Add("moveExecute", 0x025ac690);
            offsetDict.Add("updateMove", 0xE935C0);

            offsetDict.Add("runScript", 0x329A10);

            // 32836
            // offsetDict.Add("tainted", 40610888);
            // offsetDict.Add("hardware", 36371760);
            // offsetDict.Add("inGame", 39394552);
            // offsetDict.Add("nameCache", 33131288);
            // offsetDict.Add("playerGuid", 40306272);
            // offsetDict.Add("currentTime", 13155201);
            // offsetDict.Add("runScript", 3271376);
            // offsetDict.Add("interact", 13994880);
            // offsetDict.Add("mouseoverGuid", 39394560);
            // offsetDict.Add("objectTypeToFlag", 29716576);
            // offsetDict.Add("isHarm", 8938896);
            // offsetDict.Add("isHelp", 9061056);
            // offsetDict.Add("faction", 29716576);
            // offsetDict.Add("getCurrentTime", 2773968);
            //
            // offsetDict.TryAdd("tainted", GetGlobalOffsetByPattern("4C 8B 0D ?? ?? ?? ?? 45 33 C0 48 8B CE", 3));
            // offsetDict.TryAdd("hardware", GetGlobalOffsetByPattern("BA 00 20 00 00 8B 35 ? ? ? ? 48 8B CF 44 8B C6", 7));
            // offsetDict.TryAdd("objectManager", GetGlobalOffsetByPattern("48 89 5C 24 08 48 89 6C 24 10 48 89 74 24 18 57 41 56 41 57 48 83 EC 30 4C 8B 05 ? ? ? ? 45", 27));
            // offsetDict.TryAdd("inGame", GetGlobalOffsetByPattern("48 83 EC 28 0F B6 15 ? ? ? ? C1 EA 02 83 E2 01", 7));
            // offsetDict.TryAdd("nameCache",
            //     GetGlobalOffsetByPattern("48 8D 3D ? ? ? ? 48 8B DF 48 8D 0D ? ? ? ? 48 83 CB 01 48 89 1D ? ? ? ? E8 ? ? ? ? 33 C9 48 89 1D ? ? ? ?", 3));
            // offsetDict.TryAdd("playerGuid", GetGlobalOffsetByPattern("48 8D 05 ? ? ? ? 41 B8 03 00 00 00 0F 1F 00", 3));
            // offsetDict.TryAdd("currentTime", GetGlobalOffsetByPattern("48 83 EC 28 FF 05 ? ? ? ? 0F 57 C0 8B C1 89 ? ? ? ? 02 C7 05", 16));
            // // offsetDict.TryAdd("runScript",
            // //     GetFunctionOffsetByPattern("48 89 5C 24 08 48 89 6C 24 10 48 89 74 24 18 48 89 7C 24 20 41 56 48 83 EC 70 83 05 5F ? ? ? ? 48 8B EA"));
            // offsetDict.TryAdd("interact", GetFunctionOffsetByPattern("40 57 48 83 EC 20 48 8B F9 E8 ? ? ? ? 48 85 C0 75 0B"));
            // offsetDict.TryAdd("mouseoverGuid", GetGlobalOffsetByPattern("00 85 C0 75 09 0F 10 05 ? ? ? ? EB 40 BA 02 00 00 00 48 8B CB E8 ? ? ? ? F3 0F 10 05", 8));
            // offsetDict.TryAdd("objectTypeToFlag", GetGlobalOffsetByPattern("48 8D 05 ? ? ? ? 8B 14 88 C1 EA 05 40 84 D5 74", 3));
            // offsetDict.TryAdd("isHarm", GetFunctionOffsetByPattern("E8 ? ? ? FF 84 C0 74 9E F7 C3 00 00 30 00", 1));
            // offsetDict.TryAdd("isHelp", GetFunctionOffsetByPattern("48 89 5C 24 08 57 48 83 EC 20 48 8B DA 48 8B F9 E8 ? ? ? ? 83 F8 04 7D ? 48 8B D7 48 8B CB"));
            // offsetDict.TryAdd("faction", GetFunctionOffsetByPattern("48 89 5C 24 10 48 89 6C 24 18 48 89 74 24 20 57 41 56 41 57 48 83 EC 40 48 8B FA 48 8B D9 48 3B"));
            // offsetDict.TryAdd("updateMove", GetFunctionOffsetByPattern("48 89 5C 24 10 48 89 6C 24 18 48 89 7C 24 20 41 56 48 83 EC 50 45 8B F0 8B FA 48 8B D9 E8"));
            // offsetDict.TryAdd("moveBase", GetGlobalOffsetByPattern("40 53 48 83 EC 20 48 8B 1D ? ? ? ? 33 C9 E8 ? ? ? ? 84 C0 74 ? 48 89 7C 24 30 BA", 9));
            // offsetDict.TryAdd("moveExecute", GetGlobalOffsetByPattern("48 8D 0D ? ? ? ? 45 8B C6 E8 ? ? ? ? E9", 3));
            // int offset;
            for (int i = 4; i < 10; i++)
            {
                var valueString = $"{1 << i:X4}";
                //     // offsetDict.TryAdd($"moveStart_{(i)}", GetFunctionOffsetByPattern($"40 53 48 83 EC 20 48 8B 1D ? ? ? ? 33 C9 E8 ? ? ? ? 84 C0 74 ? 48 89 7C 24 30 BA " +
                //     //                                                                   $"{valueString.Substring(2,2)} {valueString.Substring(0, 2)} 00 00"));
                //     offset = GetFunctionOffsetByPattern(
                //         $"40 53 48 83 EC 30 48 8B 1D ? ? ? ? 33 C9 E8 ? ? ? ? 84 C0 74 ? 48 89 7C 24 40 45 33 C9 8B 3D ? ? ? ? BA " +
                //         $"{valueString.Substring(2, 2)} {valueString.Substring(0, 2)} 00 00");
                if (i < 8)
                {
                    AddOffsetIfNotExists($"moveStop_{i}",
                        () => process.GetFunctionOffsetByPattern(
                            "40 53 48 83 EC 30 48 8B 1D ? ? ? ? 33 C9 E8 ? ? ? ? " +
                            "84 C0 74 46 45 33 C9 48 89 7C 24 40 8B 3D ? ? ? ? " +
                            $"48 8B CB 44 8B C7 C6 44 24 20 00 41 8D 51 {valueString.Substring(2, 2)}"));
                }
            }

            // "40 53 48 83 EC 30 48 8B 1D ? ? ? ? 33 C9 E8 ? ? ? ? 84 C0 74 46 45 33 C9 48 89 7C 24 40 8B 3D ? ? ? ? 48 8B CB 44 8B C7 C6 44 24 20 00 41 8D 51 10"
            // "40 53 48 83 EC 30 48 8B 1D ? ? ? ? 33 C9 E8 ? ? ? ? 84 C0 74 46 45 33 C9 48 89 7C 24 40 8B 3D ? ? ? ? BA 80 00 00 00"
            // "40 53 48 83 EC 30 48 8B 1D ? ? ? ? 33 C9 E8 ? ? ? ? 84 C0 74 ? 48 89 7C 24 40 45 33 C9 8B 3D ? ? ? ? BA"

            foreach (var entry in offsetDict)
            {
                Console.WriteLine($"offsetDict.Add(\"{entry.Key}\", 0x{entry.Value:X}); ");

                // do something with entry.Value or entry.Key
            }
        }

        private int FindOffset(string name)
        {
            return 0;
        }


        public int GetOffset(string name)
        {
            int result = 0;
            if (!offsetDict.TryGetValue(name, out result))
            {
                result = FindOffset(name);
                offsetDict.Add(name, result);
            }

            return result;
        }
    }
}