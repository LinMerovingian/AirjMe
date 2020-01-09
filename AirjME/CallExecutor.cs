using System;
using System.Linq;

namespace AirjME
{
    public class CallExecutor
    {
        public WowProcess process;

        private IntPtr _callFunctionAddress = IntPtr.Zero;
        public CallExecutor(WowProcess process)
        {
            this.process = process;
        }
        /** call function */
        private void SetAsmByteByAddress(byte[] asmBytes, int asmOffset, IntPtr address)
        {
            var addressBytes = BitConverter.GetBytes(address.ToInt64());
            for (int i = 0; i < 8; i++)
            {
                asmBytes[i + asmOffset] = addressBytes[i];
            }
        }

        private void SetAsmByteByOffset(byte[] asmBytes, int asmOffset, int addressOffset, int size = 8)
        {
            var addressBytes = BitConverter.GetBytes(process.BaseAddress.ToInt64() + addressOffset);
            for (int i = 0; i < size; i++)
            {
                asmBytes[i + asmOffset] = addressBytes[i];
            }
        }

        private byte[] Combine(params byte[][] arrays)
        {
            byte[] rv = new byte[arrays.Sum(a => a.Length)];
            int offset = 0;
            foreach (byte[] array in arrays)
            {
                System.Buffer.BlockCopy(array, 0, rv, offset, array.Length);
                offset += array.Length;
            }

            return rv;
        }


        public long CallFunction(int offsetAddress, long arg1 = 0, long arg2 = 0, long arg3 = 0, long arg4 = 0)
        {
            //TODO add multi params implement

            byte[] asmOutValue =
            {
                0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90,
                0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90,
            };

            byte[] asmPushRegisters =
            {
                // 0x50, 0x53, 0x51, 0x52, 0x57, 0x56, 0x55, 0x41, 0x50, 0x41, 0x51, 0x41, 0x52, 0x41, 0x53, 0x41, 0x54, 0x41, 0x55, 0x41, 0x56, 0x41, 0x57,
                0x48, 0x83, 0xEC, 0x28,
            };
            //
            // byte[] asmSetTainted =
            // {
            //     0x48, 0xB9, 0xEF, 0xBE, 0xAD, 0xDE, 0xDE, 0xAD, 0xBE, 0xEF, //mov rcx, XX 
            //     0x48, 0x31, 0xC0, 0x48, 0x89, 0x01, //xor rax, rax; mov [rcx], rax
            // };
            // SetAsmByteByOffset(asmSetTainted, 2, process.GetOffset("tainted"));


            byte[] asmCallFunction =
            {
                0x48, 0xB8, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, //mov rax, xx 
                0x48, 0xB9, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, //mov rcx, xx 
                0x48, 0xBA, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, //mov rdx, xx 
                0x49, 0xB8, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, //mov r8, xx 
                0x49, 0xB9, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, //mov r9, xx 
                0xFF, 0xD0, //call rax  || 0x90, 0x90,
            };

            SetAsmByteByOffset(asmCallFunction, 2, offsetAddress);
            SetAsmByteByAddress(asmCallFunction, 12, new IntPtr(arg1));
            SetAsmByteByAddress(asmCallFunction, 22, new IntPtr(arg2));
            SetAsmByteByAddress(asmCallFunction, 32, new IntPtr(arg3));
            SetAsmByteByAddress(asmCallFunction, 42, new IntPtr(arg4));

            byte[] asmSaveResult =
            {
                0x48, 0xB9, 0xEF, 0xBE, 0xAD, 0xDE, 0xDE, 0xAD, 0xBE, 0xEF, //mov rcx, xx 
                0x48, 0x89, 0x01, //mov [rcx], rax
                0x48, 0x83, 0xC1, 0x08, //add rcx, 8
                0x48, 0x31, 0xC0, //xor rax, rax
                0x48, 0x89, 0x01, //mov [rcx], rax
            };

            byte[] asmPopRegisters =
            {
                0x48, 0x83, 0xC4, 0x28,
                // 0x41, 0x5F, 0x41, 0x5E, 0x41, 0x5D, 0x41, 0x5C, 0x41, 0x5B, 0x41, 0x5A, 0x41, 0x59, 0x41, 0x58, 0x5D, 0x5E, 0x5F, 0x5A, 0x59, 0x5B, 0x58,
                0xC3 //ret
            };

            // var asmArray = new byte[][] { asmJumpFunction };
            var asmArray = new byte[][] { asmOutValue, asmPushRegisters, asmCallFunction, asmSaveResult, asmPopRegisters };

            if (_callFunctionAddress == IntPtr.Zero)
            {
                _callFunctionAddress = Kernel32.VirtualAllocEx(process.handle, 0, 1024, Kernel32.AllocationType.Commit,
                    Kernel32.MemoryProtection.ExecuteReadWrite);
            }

            SetAsmByteByAddress(asmSaveResult, 2, _callFunctionAddress);

            byte[] asm = Combine(asmArray);

            Kernel32.WriteProcessMemory(process.handle, _callFunctionAddress.ToInt64(), asm, asm.Length, out _);
            var threadHandle = Kernel32.CreateRemoteThread(process.handle, IntPtr.Zero, 0, _callFunctionAddress, IntPtr.Zero, 0, out _);
            Kernel32.WaitForSingleObject(threadHandle, 2);
            if (process.Read<long>(_callFunctionAddress + 8) == 0)
            {
                return process.Read<long>(_callFunctionAddress);
            }

            return 0;
        }

        /*public long CallFunctionWithTainted(int offsetAddress, long arg1 = 0, long arg2 = 0, long arg3 = 0, long arg4 = 0)
        {
            byte[] asmOutValue =
            {
                0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90,
                0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90,
            };

            byte[] asmPushRegisters =
            {
                // 0x50, 0x53, 0x51, 0x52, 0x57, 0x56, 0x55, 0x41, 0x50, 0x41, 0x51, 0x41, 0x52, 0x41, 0x53, 0x41, 0x54, 0x41, 0x55, 0x41, 0x56, 0x41, 0x57,
                0x48, 0x83, 0xEC, 0x28,
            };

            byte[] asmSetTainted =
            {
                0x48, 0xB9, 0xEF, 0xBE, 0xAD, 0xDE, 0xDE, 0xAD, 0xBE, 0xEF, //mov rcx, XX 
                0x48, 0x31, 0xC0, 0x48, 0x89, 0x01, //xor rax, rax; mov [rcx], rax
            };
            SetAsmByteByOffset(asmSetTainted, 2, process.GetOffset("tainted"));

            byte[] asmSetHardware =
            {
                0x48, 0xB9, 0xEF, 0xBE, 0xAD, 0xDE, 0xDE, 0xAD, 0xBE, 0xEF, //mov rcx, XX 
                0x48, 0xBA, 0xEF, 0xBE, 0xAD, 0xDE, 0xDE, 0xAD, 0xBE, 0xEF, //mov rdx, xx 
                0x48, 0x8B, 0x02, //mov rax, [rdx]
                0x48, 0x89, 0x01, //mov [rcx], rax
            };
            SetAsmByteByOffset(asmSetHardware, 2, process.GetOffset("hardware"));
            SetAsmByteByOffset(asmSetHardware, 12, process.GetOffset("currentTime"));

            byte[] asmCallFunction =
            {
                0x48, 0xB8, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, //mov rax, xx 
                0x48, 0xB9, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, //mov rcx, xx 
                0x48, 0xBA, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, //mov rdx, xx 
                0x49, 0xB8, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, //mov r8, xx 
                0x49, 0xB9, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, //mov r9, xx 
                0xFF, 0xD0, //call rax  || 0x90, 0x90,
            };

            SetAsmByteByOffset(asmCallFunction, 2, offsetAddress);
            SetAsmByteByAddress(asmCallFunction, 12, new IntPtr(arg1));
            SetAsmByteByAddress(asmCallFunction, 22, new IntPtr(arg2));
            SetAsmByteByAddress(asmCallFunction, 32, new IntPtr(arg3));
            SetAsmByteByAddress(asmCallFunction, 42, new IntPtr(arg4));


            byte[] asmSaveResult =
            {
                0x48, 0xB9, 0xEF, 0xBE, 0xAD, 0xDE, 0xDE, 0xAD, 0xBE, 0xEF, //mov rcx, xx 
                0x48, 0x89, 0x01, //mov [rcx], rax
                0x48, 0x83, 0xC1, 0x08, //add rcx, 8
                0x48, 0x31, 0xC0, //xor rax, rax
                0x48, 0x89, 0x01, //mov [rcx], rax
            };

            byte[] asmPopRegisters =
            {
                0x48, 0x83, 0xC4, 0x28,
                // 0x41, 0x5F, 0x41, 0x5E, 0x41, 0x5D, 0x41, 0x5C, 0x41, 0x5B, 0x41, 0x5A, 0x41, 0x59, 0x41, 0x58, 0x5D, 0x5E, 0x5F, 0x5A, 0x59, 0x5B, 0x58,
                0xC3 //ret
            };

            // var asmArray = new byte[][] { asmJumpFunction };
            var asmArray = new byte[][] { asmOutValue, asmPushRegisters, asmSetTainted, asmCallFunction, asmSaveResult, asmPopRegisters };

            if (_callFunctionAddress == IntPtr.Zero)
            {
                _callFunctionAddress = Kernel32.VirtualAllocEx(handle, 0, 1024, Kernel32.AllocationType.Commit,
                    Kernel32.MemoryProtection.ExecuteReadWrite);
            }

            SetAsmByteByAddress(asmSaveResult, 2, _callFunctionAddress);

            byte[] asm = Combine(asmArray);

            Kernel32.WriteProcessMemory(handle, _callFunctionAddress.ToInt64(), asm, asm.Length, out _);
            var threadHandle = Kernel32.CreateRemoteThread(handle, IntPtr.Zero, 0, _callFunctionAddress, IntPtr.Zero, 0, out _);
            Kernel32.WaitForSingleObject(threadHandle, 2);
            if (Read<long>(_callFunctionAddress + 8) == 0)
            {
                return Read<long>(_callFunctionAddress);
            }

            return 0;
        }
        */

    }
}