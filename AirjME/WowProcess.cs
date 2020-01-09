using System;
using System.Collections;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using System.Linq;

namespace AirjME
{
    public class WowProcess : BasicProcess
    {
        public ScriptExecutor ScriptExecutor;
        public CallExecutor CallExecutor;
        public MoveExecutor MoveExecutor;
        public OffsetHolder OffsetHolder;
        public GameObjectManager GameObjectManager;
        public NameCacheManager NameCacheManager;


        public WowProcess(System.Diagnostics.Process process) : base(process)
        {
            ScriptExecutor = new ScriptExecutor(this);
            OffsetHolder = new OffsetHolder(this);
            CallExecutor = new CallExecutor(this);
            MoveExecutor = new MoveExecutor(this);
            GameObjectManager = new GameObjectManager(this);
            NameCacheManager = new NameCacheManager(this);
        }

        public int GetOffset(string name)
        {
            return OffsetHolder.GetOffset(name);
        }


        public bool AllowCreateRemoteThread(bool status)
        {
            byte[] Patch = {0xFF, 0xE0, 0xCC, 0xCC, 0xCC}; //JMP RAX
            byte[] Patch2 = {0x48, 0xFF, 0xC0, 0xFF, 0xE0}; //INC RAX, JMP RAX
            var CreateRemoteThreadPatchOffset =
                (long) Kernel32.GetProcAddress(Kernel32.GetModuleHandle("kernel32.dll"),
                    "BaseDumpAppcompatCacheWorker") + 0x220;

            int bytesRead = 0;
            byte[] buffer = new byte[5];
            Kernel32.ReadProcessMemory(handle, CreateRemoteThreadPatchOffset, buffer, buffer.Length, ref bytesRead);

            if (status)
            {
                Patch = Patch2;
            }

            _ = Kernel32.WriteProcessMemory(handle, CreateRemoteThreadPatchOffset, Patch, Patch.Length, out _);
            return true;
        }

        public long GetTaintedValue()
        {
            return Read<long>(BaseAddress + GetOffset("tainted"));
        }

        public void SetTaintedValue(long value)
        {
            Write<long>(BaseAddress + GetOffset("tainted"), value);
        }

        /** custom function */
        public bool IsInGame()
        {
            var isInGame = Read<byte>(BaseAddress + GetOffset("inGame")); //
            return isInGame != 0;
        }

        public string GetPlayerName()
        {
            return NameCacheManager.FindPlayerNameByGuid(GetPlayerGuid());
        }

        public Guid GetPlayerGuid()
        {
            return Read<Guid>(BaseAddress + GetOffset("playerGuid"));
        }

        /*public int Test()
        {
            var objectTypeToFlagAddress = BaseAddress + GetOffset("objectTypeToFlag");
            int objectAmount = 0;
            var playerGuid = GetPlayerGuid();
            var playerObject = FindObjectByGuid(playerGuid);
            IterateObject(obj =>
            {
                var type = Read<byte>(obj + 0x20);
                var flag = Read<int>(objectTypeToFlagAddress + type * 4);
                if ((flag & 0x40) != 0)
                {
                    var guid = Read<Guid>(Read<IntPtr>(obj + 0x10));
                    var name = FindPlayerNameByGuid(guid);
                    // var harm = CallFunctionWithTainted(GetOffset("isHarm"), playerObject.ToInt64(), obj.ToInt64());
                    var help = CallFunction(GetOffset("isHelp"), playerObject.ToInt64(), obj.ToInt64());
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

                    Console.WriteLine($"{name} {help}");


                    objectAmount++;
                }

                return false;
            });
            // ResumeAllThread();
            return objectAmount;
        }*/

        /*public void Test2()
        {
            // var moveExecuteAddress = BaseAddress + GetOffset("moveExecute");

            // Write<float>(moveExecuteAddress + 0x3c, 1f);

            // CallFunctionWithTainted(GetOffset("updateMove"), (BaseAddress + GetOffset("moveExecute")).ToInt64(), 0x00010010, 1);
            // CallFunctionWithTainted(GetOffset("moveStart_4"));
            // CallFunction(0x2B0310);
            // var moveAddress = BaseAddress + 0x25E1DB8;
            // var currentTime = Read<int>(BaseAddress + FindOffset("currentTime"));

            // CallFunction(0x0DAC590, 10);

            // var moveBase = Read<IntPtr>(BaseAddress + GetOffset("moveBase"));
            // var moveStatus = Read<int>(moveBase + 4);
            // var moveStatus2 = Read<int>(moveBase + 0xac);
            // Console.WriteLine($"{moveStatus:X8} {moveStatus2:X8}");

            // Write<int>(moveBase + 4, 0x00010010);
            // moveStatus = Read<int>(moveBase + 4);


            // Console.WriteLine($"{moveStatus:X8} {moveStatus2:X8}");
            // moveStatus = moveStatus;

            // CallFunctionWithTainted(0x10A5FA0);

            var playerObject = FindObjectByGuid(GetPlayerGuid());
            //var targetGuid = Read<Guid>(Read<IntPtr>(playerObject + 0x188) + 0x80);
            var playerPosition = GetUnitPosition(playerObject);
            //// var movePosition = playerPosition;
            //// if (!targetGuid.IsZero())
            //// {
            ////     var targetObject = FindObjectByGuid(targetGuid);
            ////     if (targetObject != IntPtr.Zero)
            ////     {
            ////         movePosition = GetUnitPosition(targetObject);
            ////     }
            //// }

            var movePosition = GetPositionByNodeList(playerPosition, nodeList);


            //var faceYaw = 0f;
            var moveYaw = playerPosition.FaceTo(movePosition);

            var flag = playerPosition.DistanceTo2d(movePosition) < ((moveFlag & 0xf0) == 0 ? 3 : 0.2)
                ? 0
                : GetMoveFlag(playerPosition.f, moveYaw, moveYaw);

            //GetMoveFlagForStopMoving(playerPosition.f, moveYaw)
            Move(flag);
            // if (flag != moveFlag)
            // {
            //     Move(flag);
            // }
        }
        */

        /*
        public void Test4()
        {
            var dataAddress = WriteScriptBytes(Encoding.UTF8.GetBytes("print(1) return 1\0"));
            var offset = GetOffset("runScript");
            var result = CallFunctionWithTainted(offset, dataAddress.ToInt64(), dataAddress.ToInt64(), 0);
        }

        public void Test3()
        {
            var guidAddress = BaseAddress + GetOffset("mouseoverGuid");
            if (!Read<Guid>(guidAddress).IsZero())
            {
                InteractWithGuid(guidAddress);
            }

            // CallFunction(0x01091ad0);
            // CallFunction2(0x01091ad0);
            // CallFunction(0x1095160, moveAddress.ToInt64(), 0x10, currentTime);
        }


        public void Test5()
        {
            CallFunctionWithTainted(GetOffset("moveStart_4"));
        }
        */
        public void Test5()
        {
            CallExecutor.CallFunction(GetOffset("moveStart_4"));
        }


        public PlayerObject GetActivePlayerObject()
        {
            var playerGuid = GetPlayerGuid();
            var activePlayerObject = GameObjectManager.FindObjectByGuid(playerGuid).ToPlayer();
            return activePlayerObject;
        }

        public void AVPrint()
        {
            var playerPosition = GetActivePlayerObject().GetPosition();
            Console.WriteLine($"{playerPosition.x},{playerPosition.y},{playerPosition.z}");
        }

        private bool GetNearFriendMeanPosition(out Position result, int playerLimit = 15)
        {
            int objectAmount = 0;
            var playerGuid = GetPlayerGuid();
            var activePlayerObject = GameObjectManager.FindObjectByGuid(playerGuid).ToPlayer();
            var positionList = new List<Position>();
            GameObjectManager.IterateObject(gameObject =>
            {
                var playerObject = gameObject.ToPlayer();
                if (playerObject?.IsHelpTo(activePlayerObject) == true)
                {
                    positionList.Add(playerObject.GetPosition());
                }

                return false;
            });
            if (objectAmount >= playerLimit)
            {
                var mean = new Position(positionList.Average(p => p.x), positionList.Average(p => p.y));
                var nearPositionList = new List<Position>();
                foreach (var position in positionList)
                {
                    if (mean.DistanceTo2d(position) < 20)
                    {
                        nearPositionList.Add(position);
                    }
                }

                if (nearPositionList.Count >= playerLimit / 2)
                {
                    result = new Position(nearPositionList.Average(p => p.x), nearPositionList.Average(p => p.y));
                    return true;
                }
            }

            result = new Position();
            return false;
        }

        private long lastCastingTimestamp = 0;

        public void CastMount()
        {
            ScriptExecutor.RunMacroText("/use 召唤军马");
        }

        public void UpdateCastingInfo()
        {
            var playerObject = GetActivePlayerObject();
            var now = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            if (playerObject.IsCasting())
            {
                lastCastingTimestamp = now;
            }
        }

        public bool JustCasting(long timeLimit = 200)
        {
            var now = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            return now < lastCastingTimestamp + timeLimit;
        }

        private Dictionary<string, long> lastRunTimestampDict = new Dictionary<string, long>();

        public bool RunWithInterval(string key, int interval, long now, Action function)
        {
            long lastTimestamp = 0;
            lastRunTimestampDict.TryGetValue(key, out lastTimestamp);
            if (now >= lastTimestamp + interval)
            {
                function();
                lastRunTimestampDict[key] = now;
                return true;
            }

            return false;
        }


        public void AV()
        {
            var now = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            ;
            var playerObject = GetActivePlayerObject();
            var playerPosition = playerObject.GetPosition();
            var targetGuid = playerObject.GetTargetGuid();
            //GetMoveFlagForStopMoving(playerPosition.f, moveYaw)
            if (AvData.IsInBattle(playerPosition))
            {
                UpdateCastingInfo();
                var movePosition = playerPosition;
                // var friendMeanPosition = new Position();
                var doMove = false;


                // if (!AvData.IsInCave(playerPosition) && !playerObject.IsGhost() && !playerObject.IsOnMount() && !JustCasting())
                // {
                //     CastMount();
                //     MoveExecutor.Move(0);
                // }
                // else if(!JustCasting())
                // {
                movePosition = MoveExecutor.GetPositionByNodeList(playerPosition, AvData.GetNodeList());

                var moveYaw = playerPosition.FaceTo(movePosition);

                var flag = playerPosition.DistanceTo2d(movePosition) < ((MoveExecutor.moveFlag & 0xf0) == 0 ? 3 : 0.5)
                    ? 0
                    : MoveExecutor.GetMoveFlag(playerPosition.f, moveYaw, moveYaw);


                Console.WriteLine($"=====");
                Console.WriteLine($"Tainted = {GetTaintedValue()}");
                MoveExecutor.Move(flag);
                Console.WriteLine($"Tainted = {GetTaintedValue()}");
                // }
            }


            else
            {
                MoveExecutor.Move(0);

                RunWithInterval("queue", 1000, now, () =>
                {
                    Console.WriteLine($"=====");
                    Console.WriteLine($"Tainted = {GetTaintedValue()}");
                    ScriptExecutor.RunMacroText("/target 格罗杜姆·钢须");
                    Console.WriteLine($"Tainted = {GetTaintedValue()}");
                    ScriptExecutor.RunMacroText("/click BattlefieldFrameJoinButton");
                    Console.WriteLine($"Tainted = {GetTaintedValue()}");
                    ScriptExecutor.RunMacroText("/click GossipTitleButton1");
                    Console.WriteLine($"Tainted = {GetTaintedValue()}");
                    ScriptExecutor.RunScript("InteractUnit(\"target\")");
                    Console.WriteLine($"Tainted = {GetTaintedValue()}");
                });

                // queue
                // RunScript("UseAction(1)");
                // RunScript("RunMacroText(\"/click AirjRemoteButton\")");
                // RunScript("InteractUnit(\"target\")");
                // RunScript("RunMacroText(\"/click AirjRemoteButton\") InteractUnit(\"target\")");
            }
        }

        //0x22D8540 CGObjectData::m_guid
        //0x22D8600 CGItemData::m_owner
        //0x22D8D20 CGContainerData::m_slots
        //0x22D9AC0 CGAzeriteEmpoweredItemData::m_selections
        //0x22D9B20 CGAzeriteItemData::m_xp
        //0x22D9BB0 CGUnitData::charm
        //0x22DAFB0 CGPlayerData::duelArbiter
        //0x22DD480 CGActivePlayerData::invSlots
        //0x22F3200 CGGameObjectData::m_createdBy
        //0x22F3480 CGDynamicObjectData::m_caster
        //0x22F3560 CGCorpseData::m_owner
        //0x22F38F0 CGAreaTriggerData::m_overrideScaleCurve
        //0x22F3BE0 CGSceneObjectData::m_scriptPackageID
        //0x22D85E8 CGConversationData::m_lastLineEndTime
        //0x22D8CE0 CGItemDynamicData::m_modifiers
        //0x22DAF78 CGUnitDynamicData::passiveSpells
        //0x22DD470 CGPlayerDynamicData::arenaCooldowns
        //0x22F3110 CGActivePlayerDynamicData::researchSites
        //0x22F3470 CGGameObjectDynamicData::enableDoodadSets
        //0x22F3C88 CGConversationDynamicData::m_actors

        //ActionBarFirstSlot = 0x0
        //ActiveTerrainSpell = 0x22CA080
        //CameraBase = 0x2592818
        //GameBuild = 0x1C261CC
        //GameReleaseDate = 0x1C261D8
        //GameVersion = 0x1C261C4
        //InGameFlag = 0x0
        //InGameFlag_classic = 0x2591CF8
        //IsLoadingOrConnecting = 0x225DBD0
        //LastHardwareAction = 0x22AFD30
        //ObjectMgrPtr = 0x0
        //ObjectMgrPtr_classic = 0x236FB48
        //RuneReady = 0x0

        //CheckSpellAttribute = 0x0
        //CheckSpellAttribute_classic = 0x18ED120
        //FrameScript_ExecuteBuffer = 0x31EAD0
        //FrameScript_GetLocalizedText = 0x0
        //FrameScript_GetText = 0x31B3D0
        //FrameTime_GetCurTimeMs = 0x2A53D0
        //Item_GetSpellIdById = 0x0
        //Item_GetSpellIdByObj = 0x0
        //Item_UseItem = 0x0
        //PartyInfo_GetActiveParty = 0xDE63A0
        //Party_FindMember = 0xDE6110
        //PetInfo_FindSpellById = 0xEF3EB0
        //PetInfo_SendPetAction = 0x0
        //Specialization_IsTalentSelectedById = 0xE37760
        //SpellBook_CastSpell = 0xDAC590
        //SpellBook_FindSlotBySpellId = 0xDAE8A0
        //SpellBook_FindSpellOverrideById = 0x0
        //SpellBook_FindSpellOverrideById_classic = 0xDB02F0
        //SpellBook_GetOverridenSpell = 0xDAEFD0
        //SpellDB_GetRow = 0x18ED180
        //Spell_ClickSpell = 0x771090
        //Spell_GetMinMaxRange = 0x774F60
        //Spell_GetSomeSpellInfo = 0x0
        //Spell_GetSomeSpellInfo_classic = 0x18EB770
        //Spell_GetSpellCharges = 0x0
        //Spell_GetSpellCharges_classic = 0x777570
        //Spell_GetSpellCooldown = 0x0
        //Spell_GetSpellType = 0x755B10
        //Spell_HandleTerrainClick = 0x77D450
        //Spell_IsInRange = 0x783A80
        //Spell_IsPlayerSpell = 0xDB66F0
        //Spell_IsSpellKnown = 0x8A5000
        //Spell_IsStealable = 0xDAC330
        //Unit_CanAttack = 0x886590
        //Unit_GetAuraByIndex = 0x765860
        //Unit_GetFacing = 0x898350
        //Unit_GetPosition = 0x16C2A0
        //Unit_GetPower = 0x110E3D3
        //Unit_GetPowerMax = 0x110E590
        //Unit_GetPower_classic = 0x110E3E0
        //Unit_Interact = 0xD58B80
        //Unit_IsFriendly = 0x8A42C0
        //WorldFrame_GetCurrent = 0x0
        //WorldFrame_Intersect = 0x0
        //WorldFrame_Intersect_classic = 0x113A880


        /*public void AntiAFK()
        {
            byte[] asm =
            {
                0x90, //nop
                0x48, 0x83, 0xEC, 0x20,
                0x48, 0xB9, 0xEF, 0xBE, 0xAD, 0xDE, 0xDE, 0xAD, 0xBE, 0xEF, //mov rcx, XX 
                0x48, 0xBA, 0xEF, 0xBE, 0xAD, 0xDE, 0xDE, 0xAD, 0xBE, 0xEF, //mov rdx, xx 
                0x48, 0x8B, 0x02, //mov rax, [rdx]
                0x48, 0x89, 0x01, //mov [rcx], rax
                0x48, 0xB8, 0x00, 0x80, 0xC6, 0xA4, 0x7E, 0x8D, 0x03, 0x00, //mov rax, sleepAddress 
                0x48, 0xB9, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, //mov rcx, 65536 
                0xFF, 0xD0, //call rax  || 0x90, 0x90,
                0xEB, 0xCE, //jmp (to mov)
                0x48, 0x83, 0xC4, 0x20,
                0xC3 //ret
            };

            SetAsmByteByOffset(asm, 7, GetOffset("hardware"));
            SetAsmByteByOffset(asm, 17, GetOffset("currentTime"));
            SetAsmByteByAddress(asm, 33, Kernel32.GetProcAddress(Kernel32.GetModuleHandle("kernel32.dll"), "Sleep"));

            var hAlloc = (long) Kernel32.VirtualAllocEx(handle, 0, (uint) asm.Length, Kernel32.AllocationType.Commit,
                Kernel32.MemoryProtection.ExecuteReadWrite);
            Kernel32.WriteProcessMemory(handle, hAlloc, asm, asm.Length, out _);

            Kernel32.CreateRemoteThread(handle, IntPtr.Zero, 0, (IntPtr) hAlloc, IntPtr.Zero, 0, out _);
        }*/
    }
}