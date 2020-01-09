using System;

namespace AirjME
{
    public class GameObject
    {
        public IntPtr address;
        public WowProcess process;
        public GameObject(WowProcess process, IntPtr address)
        {
            this.process = process;
            this.address = address;
        }

        public Guid GetGuid()
        {
            return process.Read<Guid>(process.Read < IntPtr > (address + 0x10));
        }

        public byte GetObjectType()
        {
            return process.Read<byte>(address + 0x20);
        }

        public int GetObjectFlag()
        {
            var objectTypeToFlagAddress = process.BaseAddress + process.GetOffset("objectTypeToFlag");
            return process.Read<int>(objectTypeToFlagAddress + GetObjectType() * 4);
        }

        public void Interact()
        {
            process.CallExecutor.CallFunction(process.GetOffset("interact"), (address+0x10).ToInt64());
        }

        public UnitObject ToUnit()
        {
            if ((GetObjectFlag() & 0x20) != 0)
            {
                return new UnitObject(process, address);
            }

            return null;
        }
        public PlayerObject ToPlayer()
        {
            if ((GetObjectFlag() & 0x40) != 0)
            {
                return new PlayerObject(process, address);
            }

            return null;
        }
    }

    public class UnitObject : GameObject
    {
        public UnitObject(WowProcess process, IntPtr address) : base(process, address)
        {

            // var name = FindPlayerNameByGuid(guid);
            // var harm = CallFunctionWithTainted(GetOffset("isHarm"), playerObject.ToInt64(), obj.ToInt64());
            // var help = CallFunction(GetOffset("isHelp"), playerObject.ToInt64(), obj.ToInt64());
            //var testvalue = CallFunction(0x8c0500, playerObject.ToInt64(), obj.ToInt64());
            // guid = *(obj + 0x10)
            // position = *(*(obj + 0x198) + 0x20)
            // powerOffset = *(2316420 + ((*(obj+0x188) + 0xb5 - 1) * 0x13) + powerType) * 4)
            // powerType = *(*(obj+0x188) + 0xb8)
            // power = *(obj+0x8A10+4*powerOffset)
            // power = *(*(obj+0x188) + 0xC8 + 4*powerOffset)
            // powerMax = *(*(obj+0x188) + 0xe8 + 4*powerOffset)
            // health = *(*(obj+0x188) + 0xC0)
            // healthMax = *(*(obj+0x188) + 0xE0)
            // mount = *(obj+0x1850)
            // aura size = *(obj + 0x1ac8)
            // aura[n] = *(obj + 0x1ad0 + n * 0xa8)
            // aura_spellId[n] = *(obj + 0x1B58 + n * 0xa8)

            // AuraSize = 0xA8;
            // AuraTable1 = 0x1AD0;
            // AuraOwnerGuid = 0x68;
            // AuraSpellId = 0x88;
            // AuraFlags = 0x90;
            // AuraStack = 0x91;
            // AuraLevel = 0x92;
            // AuraDuration = 0x94;
            // AuraEndTime = 0x98;
        }

        public Position GetPosition()
        {
            var x = process.Read<float>(process.Read<IntPtr>(address + 0x198) + 0x20);
            var y = process.Read<float>(process.Read<IntPtr>(address + 0x198) + 0x24);
            var z = process.Read<float>(process.Read<IntPtr>(address + 0x198) + 0x28);
            var facing = process.Read<float>(process.Read<IntPtr>(address + 0x198) + 0x30);
            return new Position(x, y, z, facing);
        }

        public bool IsCasting()
        {
            return process.Read<int>(address + 0x1990) != 0;
        }

        public bool IsHelpTo(UnitObject other)
        {
            var help = process.CallExecutor.CallFunction(
                process.GetOffset("isHelp"), other.address.ToInt64(), address.ToInt64());
            return help != 0;
        }

        public Guid GetTargetGuid()
        {
            var targetGuid = process.Read<Guid>(process.Read<IntPtr>(address + 0x188) + 0x80);
            return targetGuid;
        }

    }

    public class PlayerObject : UnitObject
    {
        public PlayerObject(WowProcess process, IntPtr address) : base(process, address)
        {
        }
        public bool IsGhost()
        {
            return (process.Read<int>(process.Read<IntPtr>(address + 0x8A80) + 0x30) & 0x10) != 0;
        }

        public bool IsOnMount()
        {
            return process.Read<int>(address + 0x1850) != 0;
        }
    }


}