using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Process.NET;
using Process.NET.Patterns;
using System.Collections.Generic;

namespace AirjME
{
    public struct Guid
    {
        int data0;
        int data1;
        int data2;
        int data3;

        public static bool operator ==(Guid a, Guid b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(Guid a, Guid b)
        {
            return !a.Equals(b);
        }
    }

    public class WowProcess
    {
        private PatternScanner PatternScanner;
        private ProcessSharp ProcessSharp;
        private IntPtr handle;

        private enum AllocationType
        {
            Commit = 0x1000,
            Reserve = 0x2000,
            Decommit = 0x4000,
            Release = 0x8000,
            Reset = 0x80000,
            Physical = 0x400000,
            TopDown = 0x100000,
            WriteWatch = 0x200000,
            LargePages = 0x20000000
        }

        private enum MemoryProtection
        {
            NoAccess = 0x0001,
            ReadOnly = 0x0002,
            ReadWrite = 0x0004,
            WriteCopy = 0x0008,
            Execute = 0x0010,
            ExecuteRead = 0x0020,
            ExecuteReadWrite = 0x0040,
            ExecuteWriteCopy = 0x0080,
            GuardModifierflag = 0x0100,
            NoCacheModifierflag = 0x0200,
            WriteCombineModifierflag = 0x0400,
            Proc_All_Access = 2035711
        }

        private enum ObjectTypeId
        {
            Object = 0,
            Item = 1,
            Container = 2,
            AzeriteEmpoweredItem = 3,
            AzeriteItem = 4,
            Unit = 5,
            Player = 6,
            ActivePlayer = 7,
            GameObject = 8,
            Dynamic = 9,
            Corpse = 10,
            AreaTrigger = 11,
            Scene = 12,
            Conversation = 13,
            AiGroup = 14,
            Scenario = 15,
            Loot = 16,
            Invalid = 17
        };

        [DllImport("Kernel32.dll")]
        public static extern bool ReadProcessMemory(IntPtr handle, long address, byte[] bytes, int nsize, ref int op);

        [DllImport("Kernel32.dll")]
        public static extern bool WriteProcessMemory(IntPtr hwind, long Address, byte[] bytes, int nsize,
            out int output);

        [DllImport("Kernel32.dll")]
        public static extern IntPtr OpenProcess(int Token, bool inheritH, int ProcID);

        [DllImport("Kernel32.dll")]
        public static extern IntPtr CloseHandle(IntPtr handle);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        private static extern IntPtr VirtualAllocEx(IntPtr hProcess, long lpAddress,
            uint dwSize, AllocationType flAllocationType, MemoryProtection flProtect);

        [DllImport("kernel32", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
        public static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

        [DllImport("kernel32")]
        public static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("kernel32.dll")]
        public static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes, uint dwStackSize,
            IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, out uint lpThreadId);

        public WowProcess(System.Diagnostics.Process process)
        {
            ProcessSharp = new ProcessSharp(process, Process.NET.Memory.MemoryType.Remote);
            PatternScanner = new PatternScanner(ProcessSharp[ProcessSharp.Native.MainModule.ModuleName]);
        }


        private IntPtr GetAddressFromPattern(string pattern, int offset, int size)
        {
            var scanResult = PatternScanner.Find(new DwordPattern(pattern));
            return IntPtr.Add(scanResult.ReadAddress, ProcessSharp.Memory.Read<int>(scanResult.ReadAddress + offset)) +
                   offset + size;
        }

        public static string ByteArrayToString(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2} ", b);
            return hex.ToString();
        }

        public bool AllowCreateRemoteThread(bool status)
        {
            byte[] Patch = {0xFF, 0xE0, 0xCC, 0xCC, 0xCC}; //JMP RAX
            byte[] Patch2 = {0x48, 0xFF, 0xC0, 0xFF, 0xE0}; //INC RAX, JMP RAX
            var CreateRemoteThreadPatchOffset =
                (long) GetProcAddress(GetModuleHandle("kernel32.dll"), "BaseDumpAppcompatCacheWorker") + 0x220;

            int bytesRead = 0;
            byte[] buffer = new byte[5];
            ReadProcessMemory(handle, CreateRemoteThreadPatchOffset, buffer, buffer.Length, ref bytesRead);

            if (status)
            {
                Patch = Patch2;
            }

            _ = WriteProcessMemory(handle, CreateRemoteThreadPatchOffset, Patch, Patch.Length, out _);
            return true;
        }

        private long LuaTaintedPtrOffset
        {
            get
            {
                var Lua_TaintedPtrOffset = GetAddressFromPattern("4C 8B 0D ?? ?? ?? ?? 45 33 C0 48 8B CE", 3, 4);
                return Lua_TaintedPtrOffset.ToInt64() - ProcessSharp.Native.MainModule.BaseAddress.ToInt64();
            }
        }

        private int GetOffsetByPattern(string pattern, int offset)
        {
            var Lua_TaintedPtrOffset = GetAddressFromPattern(pattern, offset, 4);
            return (int) (Lua_TaintedPtrOffset.ToInt64() - BaseAddress.ToInt64());
        }

        private ulong ReadMemory(IntPtr wHandle, long offset)
        {
            int bytesRead = 0;
            byte[] buffer = new byte[8];
            ReadProcessMemory(wHandle,
                (long) System.Diagnostics.Process.GetProcessById(ProcessSharp.Native.Id).MainModule.BaseAddress +
                offset, buffer, buffer.Length, ref bytesRead);
            return BitConverter.ToUInt64(buffer, 0);
        }

        public void KeepMemorySet(IntPtr wHandle, long offset, long value)
        {
            var sleepAddress = (long) GetProcAddress(GetModuleHandle("kernel32.dll"), "Sleep");
            var sleepAddressBytes = BitConverter.GetBytes(sleepAddress);
            byte[] asm =
            {
                0x90, //nop
                0x55, //push rbp
                0x48, 0x8B, 0xEC, //mov rbp, rsp
                0x48, 0xB9, 0xEF, 0xBE, 0xAD, 0xDE, 0xDE, 0xAD, 0xBE, 0xEF, //mov rcx, luaTaintedPtrOffset 
                0xC7, 0x01, 0x00, 0x00, 0x00, 0x00, //mov [rcx],00000000
                0xC7, 0x41, 0x04, 0x00, 0x00, 0x00, 0x00, //mov [rcx+04],00000000
                0xEB, 0xF1, //jmp (to mov)
                0x48, 0x8B, 0xE5, //mov rsp, rbp
                0x5D, //pop rbp
                0xC3 //ret
            };
            var offsetBytes =
                BitConverter.GetBytes((long) System.Diagnostics.Process.GetProcessById(ProcessSharp.Native.Id)
                                          .MainModule.BaseAddress + offset);
            var valueBytes = BitConverter.GetBytes(value);
            for (int i = 0; i < 8; i++)
            {
                asm[i + 7] = offsetBytes[i];
                //asm[i + 30] = sleepAddressBytes[i];
            }

            for (int i = 0; i < 4; i++)
            {
                asm[i + 17] = valueBytes[i];
                asm[i + 24] = valueBytes[i + 4];
            }

            var hAlloc = (long) VirtualAllocEx(wHandle, 0, (uint) asm.Length, AllocationType.Commit,
                MemoryProtection.ExecuteReadWrite);
            WriteProcessMemory(wHandle, hAlloc, asm, asm.Length, out _);

            CreateRemoteThread(wHandle, IntPtr.Zero, 0, (IntPtr) hAlloc, IntPtr.Zero, 0, out _);
        }

        public void KeepMemorySet2(IntPtr wHandle, long offset, long value)
        {
            var sleepAddress = (long) GetProcAddress(GetModuleHandle("kernel32.dll"), "Sleep");
            var sleepAddressBytes = BitConverter.GetBytes(sleepAddress);
            byte[] asm =
            {
                0x90, //nop
                0x55, //push rbp
                0x48, 0x8B, 0xEC, //mov rbp, rsp
                0x48, 0xB9, 0xEF, 0xBE, 0xAD, 0xDE, 0xDE, 0xAD, 0xBE, 0xEF, //mov rcx, XX 
                0xC7, 0x01, 0x00, 0x00, 0x00, 0x00, //mov [rcx],00000000
                0xC7, 0x41, 0x04, 0x00, 0x00, 0x00, 0x00, //mov [rcx+04],00000000
                0x48, 0xB8, 0x00, 0x80, 0xC6, 0xA4, 0x7E, 0x8D, 0x03, 0x00, //mov rax, sleepAddress 
                0x48, 0xB9, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, //mov rcx, 0000000000000001 
                0xFF, 0xD0, //call rax  || 0x90, 0x90,
                0xEB, 0xD1, //jmp (to mov)
                0x48, 0x8B, 0xE5, //mov rsp, rbp
                0x5D, //pop rbp
                0xC3 //ret
            };
            //byte[] asm =
            //{
            //    0x90,                                                       //nop
            //    0x55,                                                       //push rbp
            //    0x48, 0x8B, 0xEC,                                           //mov rbp, rsp
            //    0x48, 0xB9, 0xEF, 0xBE, 0xAD, 0xDE, 0xDE, 0xAD, 0xBE, 0xEF, //mov rcx, luaTaintedPtrOffset 
            //    0xC7, 0x01, 0x00, 0x00, 0x00, 0x00,                         //mov [rcx],00000000
            //    0xC7, 0x41, 0x04, 0x00, 0x00, 0x00, 0x00,                   //mov [rcx+04],00000000
            //    0xEB, 0xF1,                                                 //jmp (to mov)
            //    0x48, 0x8B, 0xE5,                                           //mov rsp, rbp
            //    0x5D,                                                       //pop rbp
            //    0xC3                                                        //ret
            //};
            var offsetBytes =
                BitConverter.GetBytes((long) System.Diagnostics.Process.GetProcessById(ProcessSharp.Native.Id)
                                          .MainModule.BaseAddress + offset);
            var valueBytes = BitConverter.GetBytes(value);
            for (int i = 0; i < 8; i++)
            {
                asm[i + 7] = offsetBytes[i];
                asm[i + 30] = sleepAddressBytes[i];
            }

            for (int i = 0; i < 4; i++)
            {
                asm[i + 17] = valueBytes[i];
                asm[i + 24] = valueBytes[i + 4];
            }

            var hAlloc = (long) VirtualAllocEx(wHandle, 0, (uint) asm.Length, AllocationType.Commit,
                MemoryProtection.ExecuteReadWrite);
            WriteProcessMemory(wHandle, hAlloc, asm, asm.Length, out _);

            CreateRemoteThread(wHandle, IntPtr.Zero, 0, (IntPtr) hAlloc, IntPtr.Zero, 0, out _);
        }

        public void WriteByte(IntPtr wHandle, long offset, byte value)
        {
            byte[] asm =
            {
                0x90, //nop
                0x55, //push rbp
                0x48, 0x8B, 0xEC, //mov rbp, rsp
                0x48, 0xB9, 0xEF, 0xBE, 0xAD, 0xDE, 0xDE, 0xAD, 0xBE, 0xEF, //mov rcx, XX 
                0xC6, 0x01, 0x00, //mov byte ptr [rcx], 0xff
                0x48, 0x8B, 0xE5, //mov rsp, rbp
                0x5D, //pop rbp
                0xC3 //ret
            };
            var offsetBytes =
                BitConverter.GetBytes((long) System.Diagnostics.Process.GetProcessById(ProcessSharp.Native.Id)
                                          .MainModule.BaseAddress + offset);
            var valueBytes = BitConverter.GetBytes(value);
            for (int i = 0; i < 8; i++)
            {
                asm[i + 7] = offsetBytes[i];
            }

            asm[17] = value;

            var hAlloc = (long) VirtualAllocEx(wHandle, 0, (uint) asm.Length, AllocationType.Commit,
                MemoryProtection.ExecuteReadWrite);
            WriteProcessMemory(wHandle, hAlloc, asm, asm.Length, out _);

            CreateRemoteThread(wHandle, IntPtr.Zero, 0, (IntPtr) hAlloc, IntPtr.Zero, 0, out _);
        }

        public void WriteByte2(IntPtr wHandle, long offset, byte value)
        {
            byte[] bytes = {value};

            _ = WriteProcessMemory(wHandle,
                (long) System.Diagnostics.Process.GetProcessById(ProcessSharp.Native.Id).MainModule.BaseAddress +
                offset, bytes, bytes.Length, out _);
        }


        public bool DoHack(IntPtr wHandle)
        {
            //var offset = GetOffsetByPattern("BA 00 20 00 00 8B 35 ? ? ? ? 48 8B CF 44 8B C6", 7);

            //int bytesRead = 0;
            //byte[] buffer = new byte[8];
            //ReadProcessMemory(wHandle, ProcessSharp.Native.MainModule.BaseAddress.ToInt64() + offset, buffer, buffer.Length, ref bytesRead);
            //Console.WriteLine($"{ByteArrayToString(buffer)}");


            var scanResult = PatternScanner.Find(new DwordPattern("48 8B CB 74 32 F3 0F 10 4C 24 20 0F 5A C9 E8 9D"));

            var before = ReadMemory(wHandle, scanResult.Offset);

            var offset = GetOffsetByPattern("4C 8B 0D ?? ?? ?? ?? 45 33 C0 48 8B CE", 3);

            //WriteByte(wHandle, offset, 0);
            WriteByte2(wHandle, scanResult.Offset + 4, 0x90);

            var after = ReadMemory(wHandle, scanResult.Offset);


            //var offset = GetOffsetByPattern("4C 8B 0D ?? ?? ?? ?? 45 33 C0 48 8B CE", 3);
            //Console.WriteLine($"{ReadMemory(wHandle, offset):X}");

            //KeepMemorySet(wHandle, offset, 0);
            //KeepMemorySet2(wHandle, GetOffsetByPattern("BA 00 20 00 00 8B 35 ? ? ? ? 48 8B CF 44 8B C6", 7), 0x70000000);
            Thread.Sleep(15);
            AllowCreateRemoteThread(false);

            return true;
        }

        public void Open()
        {
            handle = OpenProcess((int) MemoryProtection.Proc_All_Access, false, ProcessSharp.Native.Id);
        }

        public void Close()
        {
            CloseHandle(handle);
        }

        private IntPtr BaseAddress
        {
            get { return new IntPtr(ProcessSharp.Native.MainModule.BaseAddress.ToInt64()); }
        }

        private T Read<T>(IntPtr address) where T : struct
        {
            int bytesRead = 0;
            byte[] buffer = new byte[32];
            ReadProcessMemory(handle, address.ToInt64(), buffer, buffer.Length, ref bytesRead);
            T result;
            GCHandle gcHandle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            try
            {
                result = (T) Marshal.PtrToStructure(gcHandle.AddrOfPinnedObject(), typeof(T));
            }
            finally
            {
                gcHandle.Free();
            }

            return result;
        }

        private string ReadString(IntPtr address, int maxLenght = 128)
        {
            int bytesRead = 0;
            byte[] buffer = new byte[maxLenght];
            ReadProcessMemory(handle, address.ToInt64(), buffer, buffer.Length, ref bytesRead);
            return Encoding.UTF8.GetString(buffer).Split('\0')[0];
        }

        private IntPtr FindInList(IntPtr startPointer, int nextOffset, Func<IntPtr, bool> checkFunction)
        {
            var currentPointer = Read<IntPtr>(startPointer);
            while (currentPointer != startPointer && currentPointer != IntPtr.Zero)
            {
                if (checkFunction(currentPointer))
                {
                    return currentPointer;
                }

                currentPointer = Read<IntPtr>(currentPointer + nextOffset);
            }

            return IntPtr.Zero;
        }

        private void SetAsmAddressAbs(byte[] asmBytes, int asmOffset, IntPtr address)
        {
            var addressBytes = BitConverter.GetBytes(address.ToInt64());
            for (int i = 0; i < 8; i++)
            {
                asmBytes[i + asmOffset] = addressBytes[i];
            }
        }


        private void SetAsmAddress(byte[] asmBytes, int asmOffset, int addressOffset, int size = 8)
        {
            var addressBytes = BitConverter.GetBytes(BaseAddress.ToInt64() + addressOffset);
            for (int i = 0; i < size; i++)
            {
                asmBytes[i + asmOffset] = addressBytes[i];
            }
        }

        public bool IsInGame()
        {
            var isInGame =
                Read<byte>(BaseAddress +
                           0x02591cf8); //GetOffsetByPattern("48 83 EC 28 0F B6 15 ? ? ? ? C1 EA 02 83 E2 01", 7)
            return isInGame != 0;
        }

        public string GetPlayerName()
        {
            var nameBaseAddress =
                BaseAddress +
                0x01f98b18; // GetOffsetByPattern("48 8D 3D ? ? ? ? 48 8B DF 48 8D 0D ? ? ? ? 48 83 CB 01 48 89 1D ? ? ? ? E8 ? ? ? ? 33 C9 48 89 1D ? ? ? ?", 3);
            var playerGuid =
                Read<Guid>(BaseAddress + GetOffsetByPattern("48 8D 05 ? ? ? ? 41 B8 03 00 00 00 0F 1F 00", 3));
            var playerNameObject = FindInList(nameBaseAddress, 0, p => Read<Guid>(p + 0x20) == playerGuid);
            if (playerNameObject != IntPtr.Zero)
            {
                return ReadString(playerNameObject + 0x31);
            }

            return "";
        }

        public string GetPlayerNameByGuid(Guid guid)
        {
            var nameBaseAddress =
                BaseAddress +
                0x01f98b18; // GetOffsetByPattern("48 8D 3D ? ? ? ? 48 8B DF 48 8D 0D ? ? ? ? 48 83 CB 01 48 89 1D ? ? ? ? E8 ? ? ? ? 33 C9 48 89 1D ? ? ? ?", 3);
            var playerNameObject = FindInList(nameBaseAddress, 0, p => Read<Guid>(p + 0x20) == guid);
            if (playerNameObject != IntPtr.Zero)
            {
                return ReadString(playerNameObject + 0x31);
            }

            return "";
        }

        public float GetTime()
        {
            return Read<int>(BaseAddress + 0x22AD8DC + 0x1A00);
        }

        public void AntiAFK()
        {
            byte[] asm =
            {
                0x90, //nop
                0x55, //push rbp
                0x48, 0x8B, 0xEC, //mov rbp, rsp
                0x48, 0xB9, 0xEF, 0xBE, 0xAD, 0xDE, 0xDE, 0xAD, 0xBE, 0xEF, //mov rcx, XX 
                0x48, 0xBA, 0xEF, 0xBE, 0xAD, 0xDE, 0xDE, 0xAD, 0xBE, 0xEF, //mov rdx, xx 
                0x48, 0x8B, 0x02, //mov rax, [rdx]
                0x48, 0x89, 0x01, //mov [rcx], rax
                0x48, 0xB8, 0x00, 0x80, 0xC6, 0xA4, 0x7E, 0x8D, 0x03, 0x00, //mov rax, sleepAddress 
                0x48, 0xB9, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, //mov rcx, 65536 
                0xFF, 0xD0, //call rax  || 0x90, 0x90,
                0xEB, 0xCE, //jmp (to mov)
                0x48, 0x8B, 0xE5, //mov rsp, rbp
                0x5D, //pop rbp
                0xC3 //ret
            };

            SetAsmAddress(asm, 7, GetOffsetByPattern("BA 00 20 00 00 8B 35 ? ? ? ? 48 8B CF 44 8B C6", 7));
            SetAsmAddress(asm, 17,
                GetOffsetByPattern("48 83 EC 28 FF 05 ? ? ? ? 0F 57 C0 8B C1 89 ? ? ? ? 02 C7 05", 16));
            SetAsmAddressAbs(asm, 33, GetProcAddress(GetModuleHandle("kernel32.dll"), "Sleep"));

            var hAlloc = (long) VirtualAllocEx(handle, 0, (uint) asm.Length, AllocationType.Commit,
                MemoryProtection.ExecuteReadWrite);
            WriteProcessMemory(handle, hAlloc, asm, asm.Length, out _);

            CreateRemoteThread(handle, IntPtr.Zero, 0, (IntPtr) hAlloc, IntPtr.Zero, 0, out _);
        }
        //public bool HasAntiAFK()
        //{
        //    var scanResult = PatternScanner.Find(new DwordPattern(
        //        "90 55 48 8b ec " +
        //        "48 b9 ? ? ? ? ? ? ? ? " +
        //        "48 ba ? ? ? ? ? ? ? ? " +
        //        "48 8b 02 48 89 01" +
        //        //"48 b8 ? ? ? ? ? ? ? ? " +
        //        //"48 b9 ? ? ? ? ? ? ? ? " +
        //        //"ff d0 " +
        //        //"eb ce " +
        //        //"48 8b e5 5d c3" +
        //        ""));
        //    return scanResult.Found;
        //}

        public float[] GetPlayerPostition()
        {
            var result = new float[4];

            return result;
        }

        public void SetupMove()
        {
        }

        public void UpdataMove()
        {
        }


        private IntPtr moveTestAddress;


        public void MoveTest()
        {
            byte[] asm =
            {
                0x90, //nop
                0x55, //push rbp
                0x48, 0x8B, 0xEC, //mov rbp, rsp
                0x48, 0xB9, 0xEF, 0xBE, 0xAD, 0xDE, 0xDE, 0xAD, 0xBE, 0xEF, //mov rcx, xx 
                0xC7, 0x01, 0x00, 0x00, 0x00, 0x00, //mov [rcx],00000000
                0xC7, 0x41, 0x04, 0x00, 0x00, 0x00, 0x00, //mov [rcx+04],00000000
                0x48, 0xB8, 0x00, 0x80, 0xC6, 0xA4, 0x7E, 0x8D, 0x03, 0x00, //mov rax, xx 
                0xFF, 0xD0, //call rax  || 0x90, 0x90,
                0x31, 0xC0, 0xFF, 0xC0, //xor eax, eax   inc eax
                0x48, 0x8B, 0xE5, //mov rsp, rbp
                0x5D, //pop rbp
                0xC3 //ret
            };
            //var luaTaintedoffset = GetOffsetByPattern("4C 8B 0D ?? ?? ?? ?? 45 33 C0 48 8B CE", 3);
            //var scanResult = PatternScanner.Find(new DwordPattern("40 53 48 83 EC 20 48 8B 1D ? ? ? ? 33 C9 E8 ? ? ? ? 84 C0 74 73 48 89 7C 24 30 BA 10 00 00 00"));
            SetAsmAddress(asm, 7, 0x026bac48); // 0x026bac48
            SetAsmAddress(asm, 30, 0x01091ad0); // 0x01091ad0

            var hAlloc = (long) VirtualAllocEx(handle, 0, (uint) asm.Length, AllocationType.Commit,
                MemoryProtection.ExecuteReadWrite);
            WriteProcessMemory(handle, hAlloc, asm, asm.Length, out _);
            moveTestAddress = (IntPtr) hAlloc;
        }

        public void MoveTest2()
        {
            CreateRemoteThread(handle, IntPtr.Zero, 0, moveTestAddress, IntPtr.Zero, 0, out _);
        }

        private IntPtr WriteData(byte[] data)
        {
            var hAlloc = (long) VirtualAllocEx(handle, 0, (uint) data.Length, AllocationType.Commit,
                MemoryProtection.ReadWrite);
            WriteProcessMemory(handle, hAlloc, data, data.Length, out _);
            return new IntPtr(hAlloc);
        }

        public void RunScrit()
        {
            var scriptStringAddress = WriteData(Encoding.ASCII.GetBytes("print(1)\0"));

            AllowCreateRemoteThread(true);
            Thread.Sleep(15);
            byte[] asm =
            {
                0x90, //nop
                0x55, //push rbp
                0x48, 0x8B, 0xEC, //mov rbp, rsp
                0x48, 0xB9, 0xEF, 0xBE, 0xAD, 0xDE, 0xDE, 0xAD, 0xBE, 0xEF, //mov rcx, xx 
                0xC7, 0x01, 0x00, 0x00, 0x00, 0x00, //mov [rcx],00000000
                0xC7, 0x41, 0x04, 0x00, 0x00, 0x00, 0x00, //mov [rcx+04],00000000
                0x48, 0xB8, 0x00, 0x80, 0xC6, 0xA4, 0x7E, 0x8D, 0x03, 0x00, //mov rax, xx 
                0x48, 0xB9, 0xEF, 0xBE, 0xAD, 0xDE, 0xDE, 0xAD, 0xBE, 0xEF, //mov rcx, xx 
                0x48, 0xBA, 0xEF, 0xBE, 0xAD, 0xDE, 0xDE, 0xAD, 0xBE, 0xEF, //mov rdx, xx 
                0x4D, 0x31, 0xC0, //xor r8, r8
                0xFF, 0xD0, //call rax  || 0x90, 0x90,
                0x48, 0x8B, 0xE5, //mov rsp, rbp
                0x5D, //pop rbp
                0xC3 //ret
            };
            var luaTaintedoffset = GetOffsetByPattern("4C 8B 0D ?? ?? ?? ?? 45 33 C0 48 8B CE", 3);
            SetAsmAddress(asm, 7, luaTaintedoffset);

            var scanResult = PatternScanner.Find(new DwordPattern(
                "48 89 5C 24 08 48 89 6C 24 10 48 89 74 24 18 48 89 7C 24 20 41 56 48 83 EC 70 83 05 5F ? ? ? ? 48 8B EA"));

            SetAsmAddress(asm, 30, scanResult.Offset);
            SetAsmAddressAbs(asm, 40, scriptStringAddress);
            SetAsmAddressAbs(asm, 50, scriptStringAddress);

            var hAlloc = (long) VirtualAllocEx(handle, 0, (uint) asm.Length, AllocationType.Commit,
                MemoryProtection.ExecuteReadWrite);
            WriteProcessMemory(handle, hAlloc, asm, asm.Length, out _);
            CreateRemoteThread(handle, IntPtr.Zero, 0, (IntPtr) hAlloc, IntPtr.Zero, 0, out _);
            Thread.Sleep(15);
            AllowCreateRemoteThread(false);
        }

        public void InteractMouseover()
        {
            AllowCreateRemoteThread(true);
            Thread.Sleep(15);
            byte[] asm =
            {
                0x90, //nop
                0x55, //push rbp
                0x48, 0x8B, 0xEC, //mov rbp, rsp
                0x48, 0xB9, 0xEF, 0xBE, 0xAD, 0xDE, 0xDE, 0xAD, 0xBE, 0xEF, //mov rcx, xx 
                0xC7, 0x01, 0x00, 0x00, 0x00, 0x00, //mov [rcx],00000000
                0xC7, 0x41, 0x04, 0x00, 0x00, 0x00, 0x00, //mov [rcx+04],00000000
                0x48, 0xB8, 0x00, 0x80, 0xC6, 0xA4, 0x7E, 0x8D, 0x03, 0x00, //mov rax, xx 
                0x48, 0xB9, 0xEF, 0xBE, 0xAD, 0xDE, 0xDE, 0xAD, 0xBE, 0xEF, //mov rcx, xx 
                0xFF, 0xD0, //call rax  || 0x90, 0x90,
                0x31, 0xC0, 0xFF, 0xC0, //xor eax, eax   inc eax
                0x48, 0x8B, 0xE5, //mov rsp, rbp
                0x5D, //pop rbp
                0xC3 //ret
            };
            var luaTaintedoffset = GetOffsetByPattern("4C 8B 0D ?? ?? ?? ?? 45 33 C0 48 8B CE", 3);
            SetAsmAddress(asm, 7, luaTaintedoffset);
            var scanResult = PatternScanner.Find(new DwordPattern(
                "40 57 48 83 EC 20 48 8B F9 E8 72 27 41 FF 48 85 C0 75 0B B8 01 00 00 00 48 83 C4 20 5F C3 41 B9"));

            SetAsmAddress(asm, 30, scanResult.Offset);
            SetAsmAddress(asm, 40, 0x2591D00);

            var hAlloc = (long) VirtualAllocEx(handle, 0, (uint) asm.Length, AllocationType.Commit,
                MemoryProtection.ExecuteReadWrite);
            WriteProcessMemory(handle, hAlloc, asm, asm.Length, out _);
            CreateRemoteThread(handle, IntPtr.Zero, 0, (IntPtr) hAlloc, IntPtr.Zero, 0, out _);
            Thread.Sleep(15);
            AllowCreateRemoteThread(false);
        }

        private Dictionary<int, IntPtr> functionMemoryDict = new Dictionary<int, IntPtr>();

        public long CallFunction(int offsetAddress, long arg1 = 0, long arg2 = 0, long arg3 = 0, long arg4 = 0)
        {

            byte[] asm =
            {
                0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90,
                0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90,
                0x48, 0x83, 0xEC, 0x20,
                0x48, 0xB8, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, //mov rax, xx 
                0x48, 0xB9, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, //mov rcx, xx 
                0x48, 0xBA, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, //mov rdx, xx 
                0x49, 0xB8, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, //mov r8, xx 
                0x49, 0xB9, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, //mov r9, xx 
                0xFF, 0xD0, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, //call rax  || 0x90, 0x90,
                0x48, 0xB9, 0xEF, 0xBE, 0xAD, 0xDE, 0xDE, 0xAD, 0xBE, 0xEF, //mov rcx, xx 
                0x48, 0x89, 0x01, //mov [rcx], rax
                0x48, 0x83, 0xC1, 0x08, //add rcx, 8
                0x48, 0x31, 0xC0, //xor rax, rax
                0x48, 0x89, 0x01, //mov [rcx], rax
                0x48, 0x83, 0xC4, 0x20,
                0xC3 //ret
            };

            IntPtr threadAddress;
            if (!functionMemoryDict.TryGetValue(offsetAddress, out threadAddress))
            {
                threadAddress = VirtualAllocEx(handle, 0, (uint)asm.Length, AllocationType.Commit,
                    MemoryProtection.ExecuteReadWrite);
                functionMemoryDict.Add(offsetAddress, threadAddress);
            }
            SetAsmAddress(asm, 22, offsetAddress);
            SetAsmAddressAbs(asm, 32, new IntPtr(arg1));
            SetAsmAddressAbs(asm, 42, new IntPtr(arg2));
            SetAsmAddressAbs(asm, 52, new IntPtr(arg3));
            SetAsmAddressAbs(asm, 62, new IntPtr(arg4));
            SetAsmAddressAbs(asm, 82, threadAddress);
            WriteProcessMemory(handle, threadAddress.ToInt64(), asm, asm.Length, out _);
            CreateRemoteThread(handle, IntPtr.Zero, 0, threadAddress, IntPtr.Zero, 0, out _);
            for (int i = 0; i < 0x10000; i++)
            {
                if (Read<long>(threadAddress + 8) == 0)
                {
                    return Read<long>(threadAddress);
                }
            }

            return 0;
        }

        public IntPtr IterateObject(Func<IntPtr, bool> checkFunction)
        {
            var objectManagerAddress = Read<IntPtr>(BaseAddress + 0x236FB48);
            var obj = Read<IntPtr>(objectManagerAddress + 0x18);
            var offset = Read<int>(objectManagerAddress + 0x08) + 8;
            while (obj.ToInt64() != 0 && (obj.ToInt64() & 1) == 0)
            {
                if (checkFunction(obj))
                {
                    return obj;
                }

                obj = Read<IntPtr>(obj + offset);
            }

            return IntPtr.Zero;
        }

        public IntPtr FindObjcetByGuid(Guid guid)
        {
            return IterateObject(obj => guid == Read<Guid>(Read<IntPtr>(obj + 0x10)));
        }

        public int IterateObject()
        {
            var objectTypeToFlagAddress = BaseAddress + 0x1C57060;
            int objectAmount = 0;
            var playerGuid =
                Read<Guid>(BaseAddress + GetOffsetByPattern("48 8D 05 ? ? ? ? 41 B8 03 00 00 00 0F 1F 00", 3));
            var playerObject = FindObjcetByGuid(playerGuid);
            IterateObject(obj =>
            {
                var type = Read<byte>(obj + 0x20);
                var flag = Read<int>(objectTypeToFlagAddress + type * 4);
                if ((flag & 0x40) != 0)
                {
                    var guid = Read<Guid>(Read<IntPtr>(obj + 0x10));
                    var name = GetPlayerNameByGuid(guid);
                    var harm = CallFunction(0x886590, playerObject.ToInt64(), obj.ToInt64());
                    var help = CallFunction(0x8A42C0, playerObject.ToInt64(), obj.ToInt64());
                    var testvalue = CallFunction(0x8c0500, playerObject.ToInt64(), obj.ToInt64());
                    // guid = *(obj + 0x10)
                    // position = *(*(obj + 0x198) + 0x20)
                    // powerOffset = *(2316420 + ((*(obj+0x188) + 0xb5 - 1) * 0x13) + powerType) * 4)
                    // powerType = *(*(obj+0x188) + 0xb8)
                    // power = *(obj+0x8A10+4*powerOffset)
                    // power = *(*(obj+0x188) + 0xC8 + 4*powerOffset)
                    // powerMax = *(*(obj+0x188) + 0xe8 + 4*powerOffset)
                    // health = *(*(obj+0x188) + 0xC0)
                    // healthMax = *(*(obj+0x188) + 0xE0)
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


                        objectAmount++;
                }

                return false;
            });
            return objectAmount;
        }

        public void GetPlayerPosition()
        {
            var playerGuid =
                Read<Guid>(BaseAddress +
                           0x02670660); //GetOffsetByPattern("48 8D 05 ? ? ? ? 41 B8 03 00 00 00 0F 1F 00", 3)
            var playerObject = FindObjcetByGuid(playerGuid);
            var positionx = Read<float>(Read<IntPtr>(playerObject + 0x198) + 0x20);
            var positiony = Read<float>(Read<IntPtr>(playerObject + 0x198) + 0x24);
            var positionz = Read<float>(Read<IntPtr>(playerObject + 0x198) + 0x28);
            var facing = Read<float>(Read<IntPtr>(playerObject + 0x198) + 0x30);

            Console.WriteLine($"{positionx},{positiony},{positionz}, {facing}");
        }

        public bool Hack()
        {
            var wHandle = OpenProcess((int) MemoryProtection.Proc_All_Access, false, ProcessSharp.Native.Id);
            var result = DoHack(wHandle);
            CloseHandle(wHandle);
            return result;
        }


        private const int CGObjectDataEnd = (int)CGObjectData.CGObjectDataEnd;
        private const int CGItemDataEnd = (int)CGItemData.CGItemDataEnd;
        private const int CGUnitDataEnd = (int)CGUnitData.CGUnitDataEnd;
        private const int CGPlayerDataEnd = (int)CGPlayerData.CGPlayerDataEnd;

        enum CGObjectData
        {
            Guid = 0, // size 4
            EntryID = 4, // size 1
            DynamicFlags = 5, // size 1
            Scale = 6, // size 1
            CGObjectDataEnd = 7
        }


        enum CGItemData
        {
            Owner = CGObjectDataEnd + 0, // size 4 flags: MIRROR_ALL
            ContainedIn = CGObjectDataEnd + 4, // size 4 flags: MIRROR_ALL
            Creator = CGObjectDataEnd + 8, // size 4 flags: MIRROR_ALL
            GiftCreator = CGObjectDataEnd + 12, // size 4 flags: MIRROR_ALL
            StackCount = CGObjectDataEnd + 16, // size 1 flags: MIRROR_OWNER
            Expiration = CGObjectDataEnd + 17, // size 1 flags: MIRROR_OWNER
            SpellCharges = CGObjectDataEnd + 18, // size 5 flags: MIRROR_OWNER
            DynamicFlags = CGObjectDataEnd + 23, // size 1 flags: MIRROR_ALL
            Enchantment = CGObjectDataEnd + 24, // size 39 flags: MIRROR_ALL
            PropertySeed = CGObjectDataEnd + 63, // size 1 flags: MIRROR_ALL
            RandomPropertiesID = CGObjectDataEnd + 64, // size 1 flags: MIRROR_ALL
            Durability = CGObjectDataEnd + 65, // size 1 flags: MIRROR_OWNER
            MaxDurability = CGObjectDataEnd + 66, // size 1 flags: MIRROR_OWNER
            CreatePlayedTime = CGObjectDataEnd + 67, // size 1 flags: MIRROR_ALL
            ModifiersMask = CGObjectDataEnd + 68, // size 1 flags: MIRROR_OWNER
            Context = CGObjectDataEnd + 69, // size 1 flags: MIRROR_ALL
            ArtifactXP = CGObjectDataEnd + 70, // size 2 flags: MIRROR_OWNER
            ItemAppearanceModID = CGObjectDataEnd + 72, // size 1 flags: MIRROR_OWNER
            CGItemDataEnd = CGObjectDataEnd + 73
        }

        enum CGContainerData
        {
            Slots = CGItemDataEnd + 0, // size 144 flags: MIRROR_ALL
            NumSlots = CGItemDataEnd + 144, // size 1 flags: MIRROR_ALL
            CGContainerDataEnd = CGItemDataEnd + 145
        }


        enum CGAzeriteEmpoweredItemData
        {
            Selections = 0, // size 4
            CGAzeriteEmpoweredItemDataEnd = 4
        }

        enum CGAzeriteItemData
        {
            Xp = CGItemDataEnd + 0, // size 2 flags: MIRROR_ALL
            Level = CGItemDataEnd + 2, // size 1 flags: MIRROR_ALL
            AuraLevel = CGItemDataEnd + 3, // size 1 flags: MIRROR_ALL
            KnowledgeLevel = CGItemDataEnd + 4, // size 1 flags: MIRROR_OWNER
            DEBUGknowledgeWeek = CGItemDataEnd + 5, // size 1 flags: MIRROR_OWNER
            CGAzeriteItemDataEnd = CGItemDataEnd + 6
        }

        enum CGUnitData
        {
            Charm = CGObjectDataEnd + 0, // size 4 flags: MIRROR_ALL
            Summon = CGObjectDataEnd + 4, // size 4 flags: MIRROR_ALL
            Critter = CGObjectDataEnd + 8, // size 4 flags: MIRROR_SELF
            CharmedBy = CGObjectDataEnd + 12, // size 4 flags: MIRROR_ALL
            SummonedBy = CGObjectDataEnd + 16, // size 4 flags: MIRROR_ALL
            CreatedBy = CGObjectDataEnd + 20, // size 4 flags: MIRROR_ALL
            DemonCreator = CGObjectDataEnd + 24, // size 4 flags: MIRROR_ALL
            LookAtControllerTarget = CGObjectDataEnd + 28, // size 4 flags: MIRROR_ALL
            Target = CGObjectDataEnd + 32, // size 4 flags: MIRROR_ALL
            BattlePetCompanionGUID = CGObjectDataEnd + 36, // size 4 flags: MIRROR_ALL
            BattlePetDBID = CGObjectDataEnd + 40, // size 2 flags: MIRROR_ALL
            ChannelData = CGObjectDataEnd + 42, // size 2 flags: 
            SummonedByHomeRealm = CGObjectDataEnd + 44, // size 1 flags: MIRROR_ALL
            Sex = CGObjectDataEnd + 45, // size 1 flags: MIRROR_ALL
            DisplayPower = CGObjectDataEnd + 46, // size 1 flags: MIRROR_ALL
            OverrideDisplayPowerID = CGObjectDataEnd + 47, // size 1 flags: MIRROR_ALL
            Health = CGObjectDataEnd + 48, // size 2 flags: MIRROR_ALL
            Power = CGObjectDataEnd + 50, // size 6 flags: 
            MaxHealth = CGObjectDataEnd + 56, // size 2 flags: MIRROR_ALL
            MaxPower = CGObjectDataEnd + 58, // size 6 flags: MIRROR_ALL
            PowerRegenFlatModifier = CGObjectDataEnd + 64, // size 6 flags: 
            PowerRegenInterruptedFlatModifier = CGObjectDataEnd + 70, // size 6 flags: 
            Level = CGObjectDataEnd + 76, // size 1 flags: MIRROR_ALL
            EffectiveLevel = CGObjectDataEnd + 77, // size 1 flags: MIRROR_ALL
            ContentTuningID = CGObjectDataEnd + 78, // size 1 flags: MIRROR_ALL
            ScalingLevelMin = CGObjectDataEnd + 79, // size 1 flags: MIRROR_ALL
            ScalingLevelMax = CGObjectDataEnd + 80, // size 1 flags: MIRROR_ALL
            ScalingLevelDelta = CGObjectDataEnd + 81, // size 1 flags: MIRROR_ALL
            ScalingFactionGroup = CGObjectDataEnd + 82, // size 1 flags: MIRROR_ALL
            ScalingHealthItemLevelCurveID = CGObjectDataEnd + 83, // size 1 flags: MIRROR_ALL
            ScalingDamageItemLevelCurveID = CGObjectDataEnd + 84, // size 1 flags: MIRROR_ALL
            FactionTemplate = CGObjectDataEnd + 85, // size 1 flags: MIRROR_ALL
            VirtualItems = CGObjectDataEnd + 86, // size 6 flags: MIRROR_ALL
            Flags = CGObjectDataEnd + 92, // size 1 flags: 
            Flags2 = CGObjectDataEnd + 93, // size 1 flags: 
            Flags3 = CGObjectDataEnd + 94, // size 1 flags: 
            AuraState = CGObjectDataEnd + 95, // size 1 flags: MIRROR_ALL
            AttackRoundBaseTime = CGObjectDataEnd + 96, // size 2 flags: MIRROR_ALL
            RangedAttackRoundBaseTime = CGObjectDataEnd + 98, // size 1 flags: MIRROR_SELF
            BoundingRadius = CGObjectDataEnd + 99, // size 1 flags: MIRROR_ALL
            CombatReach = CGObjectDataEnd + 100, // size 1 flags: MIRROR_ALL
            DisplayID = CGObjectDataEnd + 101, // size 1 flags: 
            DisplayScale = CGObjectDataEnd + 102, // size 1 flags: 
            NativeDisplayID = CGObjectDataEnd + 103, // size 1 flags: 
            NativeXDisplayScale = CGObjectDataEnd + 104, // size 1 flags: 
            MountDisplayID = CGObjectDataEnd + 105, // size 1 flags: 
            MinDamage = CGObjectDataEnd + 106, // size 1 flags: 
            MaxDamage = CGObjectDataEnd + 107, // size 1 flags: 
            MinOffHandDamage = CGObjectDataEnd + 108, // size 1 flags: 
            MaxOffHandDamage = CGObjectDataEnd + 109, // size 1 flags: 
            AnimTier = CGObjectDataEnd + 110, // size 1 flags: MIRROR_ALL
            PetNumber = CGObjectDataEnd + 111, // size 1 flags: MIRROR_ALL
            PetNameTimestamp = CGObjectDataEnd + 112, // size 1 flags: MIRROR_ALL
            PetExperience = CGObjectDataEnd + 113, // size 1 flags: MIRROR_OWNER
            PetNextLevelExperience = CGObjectDataEnd + 114, // size 1 flags: MIRROR_OWNER
            ModCastingSpeed = CGObjectDataEnd + 115, // size 1 flags: MIRROR_ALL
            ModSpellHaste = CGObjectDataEnd + 116, // size 1 flags: MIRROR_ALL
            ModHaste = CGObjectDataEnd + 117, // size 1 flags: MIRROR_ALL
            ModRangedHaste = CGObjectDataEnd + 118, // size 1 flags: MIRROR_ALL
            ModHasteRegen = CGObjectDataEnd + 119, // size 1 flags: MIRROR_ALL
            ModTimeRate = CGObjectDataEnd + 120, // size 1 flags: MIRROR_ALL
            CreatedBySpell = CGObjectDataEnd + 121, // size 1 flags: MIRROR_ALL
            NpcFlags = CGObjectDataEnd + 122, // size 2 flags: 
            EmoteState = CGObjectDataEnd + 124, // size 1 flags: MIRROR_ALL
            Stats = CGObjectDataEnd + 125, // size 4 flags: 
            StatPosBuff = CGObjectDataEnd + 129, // size 4 flags: 
            StatNegBuff = CGObjectDataEnd + 133, // size 4 flags: 
            Resistances = CGObjectDataEnd + 137, // size 7 flags: 
            BonusResistanceMods = CGObjectDataEnd + 144, // size 7 flags: 
            BaseMana = CGObjectDataEnd + 151, // size 1 flags: MIRROR_ALL
            BaseHealth = CGObjectDataEnd + 152, // size 1 flags: 
            ShapeshiftForm = CGObjectDataEnd + 153, // size 1 flags: MIRROR_ALL
            AttackPower = CGObjectDataEnd + 154, // size 1 flags: 
            AttackPowerModPos = CGObjectDataEnd + 155, // size 1 flags: 
            AttackPowerModNeg = CGObjectDataEnd + 156, // size 1 flags: 
            AttackPowerMultiplier = CGObjectDataEnd + 157, // size 1 flags: 
            RangedAttackPower = CGObjectDataEnd + 158, // size 1 flags: 
            RangedAttackPowerModPos = CGObjectDataEnd + 159, // size 1 flags: 
            RangedAttackPowerModNeg = CGObjectDataEnd + 160, // size 1 flags: 
            RangedAttackPowerMultiplier = CGObjectDataEnd + 161, // size 1 flags: 
            MainHandWeaponAttackPower = CGObjectDataEnd + 162, // size 1 flags: 
            OffHandWeaponAttackPower = CGObjectDataEnd + 163, // size 1 flags: 
            RangedWeaponAttackPower = CGObjectDataEnd + 164, // size 1 flags: 
            SetAttackSpeedAura = CGObjectDataEnd + 165, // size 1 flags: 
            Lifesteal = CGObjectDataEnd + 166, // size 1 flags: 
            MinRangedDamage = CGObjectDataEnd + 167, // size 1 flags: 
            MaxRangedDamage = CGObjectDataEnd + 168, // size 1 flags: 
            PowerCostModifier = CGObjectDataEnd + 169, // size 7 flags: 
            PowerCostMultiplier = CGObjectDataEnd + 176, // size 7 flags: 
            MaxHealthModifier = CGObjectDataEnd + 183, // size 1 flags: 
            HoverHeight = CGObjectDataEnd + 184, // size 1 flags: MIRROR_ALL
            MinItemLevelCutoff = CGObjectDataEnd + 185, // size 1 flags: MIRROR_ALL
            MinItemLevel = CGObjectDataEnd + 186, // size 1 flags: MIRROR_ALL
            MaxItemLevel = CGObjectDataEnd + 187, // size 1 flags: MIRROR_ALL
            WildBattlePetLevel = CGObjectDataEnd + 188, // size 1 flags: MIRROR_ALL
            BattlePetCompanionNameTimestamp = CGObjectDataEnd + 189, // size 1 flags: MIRROR_ALL
            InteractSpellID = CGObjectDataEnd + 190, // size 1 flags: MIRROR_ALL
            StateSpellVisualID = CGObjectDataEnd + 191, // size 1 flags: 
            StateAnimID = CGObjectDataEnd + 192, // size 1 flags: 
            StateAnimKitID = CGObjectDataEnd + 193, // size 1 flags: 
            StateWorldEffectID = CGObjectDataEnd + 194, // size 4 flags: 
            ScaleDuration = CGObjectDataEnd + 198, // size 1 flags: MIRROR_ALL
            LooksLikeMountID = CGObjectDataEnd + 199, // size 1 flags: MIRROR_ALL
            LooksLikeCreatureID = CGObjectDataEnd + 200, // size 1 flags: MIRROR_ALL
            LookAtControllerID = CGObjectDataEnd + 201, // size 1 flags: MIRROR_ALL
            GuildGUID = CGObjectDataEnd + 202, // size 4 flags: MIRROR_ALL
            CGUnitDataEnd = CGObjectDataEnd + 206
        }

        enum CGPlayerData
        {
            DuelArbiter = CGUnitDataEnd + 0, // size 4 flags: MIRROR_ALL
            WowAccount = CGUnitDataEnd + 4, // size 4 flags: MIRROR_ALL
            LootTargetGUID = CGUnitDataEnd + 8, // size 4 flags: MIRROR_ALL
            PlayerFlags = CGUnitDataEnd + 12, // size 1 flags: MIRROR_ALL
            PlayerFlagsEx = CGUnitDataEnd + 13, // size 1 flags: MIRROR_ALL
            GuildRankID = CGUnitDataEnd + 14, // size 1 flags: MIRROR_ALL
            GuildDeleteDate = CGUnitDataEnd + 15, // size 1 flags: MIRROR_ALL
            GuildLevel = CGUnitDataEnd + 16, // size 1 flags: MIRROR_ALL
            HairColorID = CGUnitDataEnd + 17, // size 1 flags: MIRROR_ALL
            CustomDisplayOption = CGUnitDataEnd + 18, // size 1 flags: MIRROR_ALL
            Inebriation = CGUnitDataEnd + 19, // size 1 flags: MIRROR_ALL
            ArenaFaction = CGUnitDataEnd + 20, // size 1 flags: MIRROR_ALL
            DuelTeam = CGUnitDataEnd + 21, // size 1 flags: MIRROR_ALL
            GuildTimeStamp = CGUnitDataEnd + 22, // size 1 flags: MIRROR_ALL
            QuestLog = CGUnitDataEnd + 23, // size 1600 flags: MIRROR_PARTY
            VisibleItems = CGUnitDataEnd + 1623, // size 38 flags: MIRROR_ALL
            PlayerTitle = CGUnitDataEnd + 1661, // size 1 flags: MIRROR_ALL
            FakeInebriation = CGUnitDataEnd + 1662, // size 1 flags: MIRROR_ALL
            VirtualPlayerRealm = CGUnitDataEnd + 1663, // size 1 flags: MIRROR_ALL
            CurrentSpecID = CGUnitDataEnd + 1664, // size 1 flags: MIRROR_ALL
            TaxiMountAnimKitID = CGUnitDataEnd + 1665, // size 1 flags: MIRROR_ALL
            AvgItemLevel = CGUnitDataEnd + 1666, // size 4 flags: MIRROR_ALL
            CurrentBattlePetBreedQuality = CGUnitDataEnd + 1670, // size 1 flags: MIRROR_ALL
            HonorLevel = CGUnitDataEnd + 1671, // size 1 flags: MIRROR_ALL
            CGPlayerDataEnd = CGUnitDataEnd + 1672
        }

        enum CGActivePlayerData
        {
            InvSlots = CGPlayerDataEnd + 0, // size 780 flags: MIRROR_ALL
            FarsightObject = CGPlayerDataEnd + 780, // size 4 flags: MIRROR_ALL
            SummonedBattlePetGUID = CGPlayerDataEnd + 784, // size 4 flags: MIRROR_ALL
            KnownTitles = CGPlayerDataEnd + 788, // size 12 flags: MIRROR_ALL
            Coinage = CGPlayerDataEnd + 800, // size 2 flags: MIRROR_ALL
            XP = CGPlayerDataEnd + 802, // size 1 flags: MIRROR_ALL
            NextLevelXP = CGPlayerDataEnd + 803, // size 1 flags: MIRROR_ALL
            TrialXP = CGPlayerDataEnd + 804, // size 1 flags: MIRROR_ALL
            Skill = CGPlayerDataEnd + 805, // size 896 flags: MIRROR_ALL
            CharacterPoints = CGPlayerDataEnd + 1701, // size 1 flags: MIRROR_ALL
            MaxTalentTiers = CGPlayerDataEnd + 1702, // size 1 flags: MIRROR_ALL
            TrackCreatureMask = CGPlayerDataEnd + 1703, // size 1 flags: MIRROR_ALL
            TrackResourceMask = CGPlayerDataEnd + 1704, // size 2 flags: MIRROR_ALL
            MainhandExpertise = CGPlayerDataEnd + 1706, // size 1 flags: MIRROR_ALL
            OffhandExpertise = CGPlayerDataEnd + 1707, // size 1 flags: MIRROR_ALL
            RangedExpertise = CGPlayerDataEnd + 1708, // size 1 flags: MIRROR_ALL
            CombatRatingExpertise = CGPlayerDataEnd + 1709, // size 1 flags: MIRROR_ALL
            BlockPercentage = CGPlayerDataEnd + 1710, // size 1 flags: MIRROR_ALL
            DodgePercentage = CGPlayerDataEnd + 1711, // size 1 flags: MIRROR_ALL
            DodgePercentageFromAttribute = CGPlayerDataEnd + 1712, // size 1 flags: MIRROR_ALL
            ParryPercentage = CGPlayerDataEnd + 1713, // size 1 flags: MIRROR_ALL
            ParryPercentageFromAttribute = CGPlayerDataEnd + 1714, // size 1 flags: MIRROR_ALL
            CritPercentage = CGPlayerDataEnd + 1715, // size 1 flags: MIRROR_ALL
            RangedCritPercentage = CGPlayerDataEnd + 1716, // size 1 flags: MIRROR_ALL
            OffhandCritPercentage = CGPlayerDataEnd + 1717, // size 1 flags: MIRROR_ALL
            SpellCritPercentage = CGPlayerDataEnd + 1718, // size 1 flags: MIRROR_ALL
            ShieldBlock = CGPlayerDataEnd + 1719, // size 1 flags: MIRROR_ALL
            ShieldBlockCritPercentage = CGPlayerDataEnd + 1720, // size 1 flags: MIRROR_ALL
            Mastery = CGPlayerDataEnd + 1721, // size 1 flags: MIRROR_ALL
            Speed = CGPlayerDataEnd + 1722, // size 1 flags: MIRROR_ALL
            Avoidance = CGPlayerDataEnd + 1723, // size 1 flags: MIRROR_ALL
            Sturdiness = CGPlayerDataEnd + 1724, // size 1 flags: MIRROR_ALL
            Versatility = CGPlayerDataEnd + 1725, // size 1 flags: MIRROR_ALL
            VersatilityBonus = CGPlayerDataEnd + 1726, // size 1 flags: MIRROR_ALL
            PvpPowerDamage = CGPlayerDataEnd + 1727, // size 1 flags: MIRROR_ALL
            PvpPowerHealing = CGPlayerDataEnd + 1728, // size 1 flags: MIRROR_ALL
            ExploredZones = CGPlayerDataEnd + 1729, // size 320 flags: MIRROR_ALL
            RestInfo = CGPlayerDataEnd + 2049, // size 4 flags: MIRROR_ALL
            ModDamageDonePos = CGPlayerDataEnd + 2053, // size 7 flags: MIRROR_ALL
            ModDamageDoneNeg = CGPlayerDataEnd + 2060, // size 7 flags: MIRROR_ALL
            ModDamageDonePercent = CGPlayerDataEnd + 2067, // size 7 flags: MIRROR_ALL
            ModHealingDonePos = CGPlayerDataEnd + 2074, // size 1 flags: MIRROR_ALL
            ModHealingPercent = CGPlayerDataEnd + 2075, // size 1 flags: MIRROR_ALL
            ModHealingDonePercent = CGPlayerDataEnd + 2076, // size 1 flags: MIRROR_ALL
            ModPeriodicHealingDonePercent = CGPlayerDataEnd + 2077, // size 1 flags: MIRROR_ALL
            WeaponDmgMultipliers = CGPlayerDataEnd + 2078, // size 3 flags: MIRROR_ALL
            WeaponAtkSpeedMultipliers = CGPlayerDataEnd + 2081, // size 3 flags: MIRROR_ALL
            ModSpellPowerPercent = CGPlayerDataEnd + 2084, // size 1 flags: MIRROR_ALL
            ModResiliencePercent = CGPlayerDataEnd + 2085, // size 1 flags: MIRROR_ALL
            OverrideSpellPowerByAPPercent = CGPlayerDataEnd + 2086, // size 1 flags: MIRROR_ALL
            OverrideAPBySpellPowerPercent = CGPlayerDataEnd + 2087, // size 1 flags: MIRROR_ALL
            ModTargetResistance = CGPlayerDataEnd + 2088, // size 1 flags: MIRROR_ALL
            ModTargetPhysicalResistance = CGPlayerDataEnd + 2089, // size 1 flags: MIRROR_ALL
            LocalFlags = CGPlayerDataEnd + 2090, // size 1 flags: MIRROR_ALL
            PvpMedals = CGPlayerDataEnd + 2091, // size 1 flags: MIRROR_ALL
            BuybackPrice = CGPlayerDataEnd + 2092, // size 12 flags: MIRROR_ALL
            BuybackTimestamp = CGPlayerDataEnd + 2104, // size 12 flags: MIRROR_ALL
            YesterdayHonorableKills = CGPlayerDataEnd + 2116, // size 1 flags: MIRROR_ALL
            LifetimeHonorableKills = CGPlayerDataEnd + 2117, // size 1 flags: MIRROR_ALL
            WatchedFactionIndex = CGPlayerDataEnd + 2118, // size 1 flags: MIRROR_ALL
            CombatRatings = CGPlayerDataEnd + 2119, // size 32 flags: MIRROR_ALL
            PvpInfo = CGPlayerDataEnd + 2151, // size 54 flags: MIRROR_ALL
            MaxLevel = CGPlayerDataEnd + 2205, // size 1 flags: MIRROR_ALL
            ScalingPlayerLevelDelta = CGPlayerDataEnd + 2206, // size 1 flags: MIRROR_ALL
            MaxCreatureScalingLevel = CGPlayerDataEnd + 2207, // size 1 flags: MIRROR_ALL
            NoReagentCostMask = CGPlayerDataEnd + 2208, // size 4 flags: MIRROR_ALL
            PetSpellPower = CGPlayerDataEnd + 2212, // size 1 flags: MIRROR_ALL
            ProfessionSkillLine = CGPlayerDataEnd + 2213, // size 2 flags: MIRROR_ALL
            UiHitModifier = CGPlayerDataEnd + 2215, // size 1 flags: MIRROR_ALL
            UiSpellHitModifier = CGPlayerDataEnd + 2216, // size 1 flags: MIRROR_ALL
            HomeRealmTimeOffset = CGPlayerDataEnd + 2217, // size 1 flags: MIRROR_ALL
            ModPetHaste = CGPlayerDataEnd + 2218, // size 1 flags: MIRROR_ALL
            NumBackpackSlots = CGPlayerDataEnd + 2219, // size 1 flags: MIRROR_ALL
            OverrideSpellsID = CGPlayerDataEnd + 2220, // size 1 flags: 
            LfgBonusFactionID = CGPlayerDataEnd + 2221, // size 1 flags: MIRROR_ALL
            LootSpecID = CGPlayerDataEnd + 2222, // size 1 flags: MIRROR_ALL
            OverrideZonePVPType = CGPlayerDataEnd + 2223, // size 1 flags: 
            BagSlotFlags = CGPlayerDataEnd + 2224, // size 4 flags: MIRROR_ALL
            BankBagSlotFlags = CGPlayerDataEnd + 2228, // size 7 flags: MIRROR_ALL
            InsertItemsLeftToRight = CGPlayerDataEnd + 2235, // size 1 flags: MIRROR_ALL
            QuestCompleted = CGPlayerDataEnd + 2236, // size 1750 flags: MIRROR_ALL
            Honor = CGPlayerDataEnd + 3986, // size 1 flags: MIRROR_ALL
            HonorNextLevel = CGPlayerDataEnd + 3987, // size 1 flags: MIRROR_ALL
            PvpTierMaxFromWins = CGPlayerDataEnd + 3988, // size 1 flags: MIRROR_ALL
            PvpLastWeeksTierMaxFromWins = CGPlayerDataEnd + 3989, // size 1 flags: MIRROR_ALL
            CGActivePlayerDataEnd = CGPlayerDataEnd + 3990
        }

        enum CGGameObjectData
        {
            CreatedBy = CGObjectDataEnd + 0, // size 4 flags: MIRROR_ALL
            GuildGUID = CGObjectDataEnd + 4, // size 4 flags: MIRROR_ALL
            DisplayID = CGObjectDataEnd + 8, // size 1 flags: 
            Flags = CGObjectDataEnd + 9, // size 1 flags: 
            ParentRotation = CGObjectDataEnd + 10, // size 4 flags: MIRROR_ALL
            FactionTemplate = CGObjectDataEnd + 14, // size 1 flags: MIRROR_ALL
            Level = CGObjectDataEnd + 15, // size 1 flags: MIRROR_ALL
            PercentHealth = CGObjectDataEnd + 16, // size 1 flags: 
            SpellVisualID = CGObjectDataEnd + 17, // size 1 flags: 
            StateSpellVisualID = CGObjectDataEnd + 18, // size 1 flags: 
            SpawnTrackingStateAnimID = CGObjectDataEnd + 19, // size 1 flags: 
            SpawnTrackingStateAnimKitID = CGObjectDataEnd + 20, // size 1 flags: 
            StateWorldEffectID = CGObjectDataEnd + 21, // size 4 flags: 
            CustomParam = CGObjectDataEnd + 25, // size 1 flags: 
            CGGameObjectDataEnd = CGObjectDataEnd + 26
        }

        enum CGDynamicObjectData
        {
            Caster = CGObjectDataEnd + 0, // size 4 flags: MIRROR_ALL
            Type = CGObjectDataEnd + 4, // size 1 flags: MIRROR_ALL
            SpellXSpellVisualID = CGObjectDataEnd + 5, // size 1 flags: MIRROR_ALL
            SpellID = CGObjectDataEnd + 6, // size 1 flags: MIRROR_ALL
            Radius = CGObjectDataEnd + 7, // size 1 flags: MIRROR_ALL
            CastTime = CGObjectDataEnd + 8, // size 1 flags: MIRROR_ALL
            CGDynamicObjectDataEnd = CGObjectDataEnd + 9
        }

        enum CGCorpseData
        {
            Owner = CGObjectDataEnd + 0, // size 4 flags: MIRROR_ALL
            PartyGUID = CGObjectDataEnd + 4, // size 4 flags: MIRROR_ALL
            GuildGUID = CGObjectDataEnd + 8, // size 4 flags: MIRROR_ALL
            DisplayID = CGObjectDataEnd + 12, // size 1 flags: MIRROR_ALL
            Items = CGObjectDataEnd + 13, // size 19 flags: MIRROR_ALL
            SkinID = CGObjectDataEnd + 32, // size 1 flags: MIRROR_ALL
            FacialHairStyleID = CGObjectDataEnd + 33, // size 1 flags: MIRROR_ALL
            Flags = CGObjectDataEnd + 34, // size 1 flags: MIRROR_ALL
            DynamicFlags = CGObjectDataEnd + 35, // size 1 flags: MIRROR_VIEWER_DEPENDENT
            FactionTemplate = CGObjectDataEnd + 36, // size 1 flags: MIRROR_ALL
            CustomDisplayOption = CGObjectDataEnd + 37, // size 1 flags: MIRROR_ALL
            CGCorpseDataEnd = CGObjectDataEnd + 38
        }

        enum CGAreaTriggerData
        {
            OverrideScaleCurve = CGObjectDataEnd + 0, // size 7 flags: 
            ExtraScaleCurve = CGObjectDataEnd + 7, // size 7 flags: 
            Caster = CGObjectDataEnd + 14, // size 4 flags: MIRROR_ALL
            Duration = CGObjectDataEnd + 18, // size 1 flags: MIRROR_ALL
            TimeToTarget = CGObjectDataEnd + 19, // size 1 flags: 
            TimeToTargetScale = CGObjectDataEnd + 20, // size 1 flags: 
            TimeToTargetExtraScale = CGObjectDataEnd + 21, // size 1 flags: 
            SpellID = CGObjectDataEnd + 22, // size 1 flags: MIRROR_ALL
            SpellForVisuals = CGObjectDataEnd + 23, // size 1 flags: MIRROR_ALL
            SpellXSpellVisualID = CGObjectDataEnd + 24, // size 1 flags: MIRROR_ALL
            BoundsRadius2D = CGObjectDataEnd + 25, // size 1 flags: 
            DecalPropertiesID = CGObjectDataEnd + 26, // size 1 flags: MIRROR_ALL
            CreatingEffectGUID = CGObjectDataEnd + 27, // size 4 flags: MIRROR_ALL
            CGAreaTriggerDataEnd = CGObjectDataEnd + 31
        }

        enum CGSceneObjectData
        {
            ScriptPackageID = CGObjectDataEnd + 0, // size 1 flags: MIRROR_ALL
            RndSeedVal = CGObjectDataEnd + 1, // size 1 flags: MIRROR_ALL
            CreatedBy = CGObjectDataEnd + 2, // size 4 flags: MIRROR_ALL
            SceneType = CGObjectDataEnd + 6, // size 1 flags: MIRROR_ALL
            CGSceneObjectDataEnd = CGObjectDataEnd + 7
        }

        enum CGConversationData
        {
            LastLineEndTime = CGObjectDataEnd + 0, // size 1 flags: MIRROR_VIEWER_DEPENDENT
            CGConversationDataEnd = CGObjectDataEnd + 1
        }

        enum CGItemDynamicData
        {
            Modifiers = CGObjectDataEnd + 0, // size 4 flags: MIRROR_NONE
            BonusListIDs = CGObjectDataEnd + 1, // size 260 flags: MIRROR_NONE
            ArtifactPowers = CGObjectDataEnd + 2, // size 4 flags: MIRROR_NONE
            Gems = CGObjectDataEnd + 3, // size 4 flags: MIRROR_NONE
            CGItemDynamicDataEnd = CGObjectDataEnd + 4
        }

        enum CGUnitDynamicData
        {
            PassiveSpells = CGObjectDataEnd + 0, // size 513 flags: MIRROR_NONE
            WorldEffects = CGObjectDataEnd + 1, // size 513 flags: MIRROR_NONE
            ChannelObjects = CGObjectDataEnd + 2, // size 513 flags: MIRROR_NONE
            CGUnitDynamicDataEnd = CGObjectDataEnd + 3
        }

        enum CGPlayerDynamicData
        {
            ArenaCooldowns = CGObjectDataEnd + 0, // size 1 flags: MIRROR_NONE
            CGPlayerDynamicDataEnd = CGObjectDataEnd + 1
        }

        enum CGActivePlayerDynamicData
        {
            ResearchSites = CGObjectDataEnd + 0, // size 1 flags: MIRROR_NONE
            ResearchSiteProgress = CGObjectDataEnd + 1, // size 1 flags: MIRROR_NONE
            DailyQuestsCompleted = CGObjectDataEnd + 2, // size 1 flags: MIRROR_NONE
            AvailableQuestLineXQuestIDs = CGObjectDataEnd + 3, // size 1 flags: MIRROR_NONE
            Heirlooms = CGObjectDataEnd + 4, // size 1 flags: MIRROR_NONE
            HeirloomFlags = CGObjectDataEnd + 5, // size 1 flags: MIRROR_NONE
            Toys = CGObjectDataEnd + 6, // size 1 flags: MIRROR_NONE
            Transmog = CGObjectDataEnd + 7, // size 1 flags: MIRROR_NONE
            ConditionalTransmog = CGObjectDataEnd + 8, // size 1 flags: MIRROR_NONE
            SelfResSpells = CGObjectDataEnd + 9, // size 1 flags: MIRROR_NONE
            CharacterRestrictions = CGObjectDataEnd + 10, // size 1 flags: MIRROR_NONE
            SpellPctModByLabel = CGObjectDataEnd + 11, // size 1 flags: MIRROR_NONE
            SpellFlatModByLabel = CGObjectDataEnd + 12, // size 1 flags: MIRROR_NONE
            Research = CGObjectDataEnd + 13, // size 1 flags: MIRROR_NONE
            CGActivePlayerDynamicDataEnd = CGObjectDataEnd + 14
        }

        enum CGGameObjectDynamicData
        {
            EnableDoodadSets = CGObjectDataEnd + 0, // size 1 flags: MIRROR_NONE
            CGGameObjectDynamicDataEnd = CGObjectDataEnd + 1
        }

        enum CGConversationDynamicData
        {
            Actors = CGObjectDataEnd + 0, // size 1 flags: MIRROR_NONE
            Lines = CGObjectDataEnd + 1, // size 256 flags: MIRROR_NONE
            CGConversationDynamicDataEnd = CGObjectDataEnd + 2
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
    }
}