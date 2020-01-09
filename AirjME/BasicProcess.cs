using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using Process.NET;
using Process.NET.Patterns;

namespace AirjME
{
    public class BasicProcess
    {
        private readonly PatternScanner _patternScanner;
        private readonly ProcessSharp _processSharp;
        public IntPtr handle;

        public BasicProcess(System.Diagnostics.Process process)
        {
            _processSharp = new ProcessSharp(process, Process.NET.Memory.MemoryType.Remote);
            _patternScanner = new PatternScanner(_processSharp[_processSharp.Native.MainModule.ModuleName]);
        }

        public void Open()
        {
            handle = Kernel32.OpenProcess((int)Kernel32.MemoryProtection.Proc_All_Access, false,
                _processSharp.Native.Id);
        }

        public void Close()
        {
            Kernel32.CloseHandle(handle);
        }

        public IntPtr BaseAddress => new IntPtr(_processSharp.Native.MainModule.BaseAddress.ToInt64());

        /** read write memory */
        public void Write<T>(IntPtr address, T value) where T : struct
        {
            int size = Marshal.SizeOf(value);
            byte[] buffer = new byte[size];
            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(value, ptr, false);
            Marshal.Copy(ptr, buffer, 0, size);
            Marshal.FreeHGlobal(ptr);
            _ = Kernel32.WriteProcessMemory(handle, address.ToInt64(), buffer, size, out _);
        }

        public T Read<T>(IntPtr address) where T : struct
        {
            int bytesRead = 0;
            byte[] buffer = new byte[32];
            Kernel32.ReadProcessMemory(handle, address.ToInt64(), buffer, buffer.Length, ref bytesRead);
            T result;
            GCHandle gcHandle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            try
            {
                result = (T)Marshal.PtrToStructure(gcHandle.AddrOfPinnedObject(), typeof(T));
            }
            finally
            {
                gcHandle.Free();
            }

            return result;
        }

        public string ReadString(IntPtr address, int maxLenght = 128)
        {
            int bytesRead = 0;
            byte[] buffer = new byte[maxLenght];
            Kernel32.ReadProcessMemory(handle, address.ToInt64(), buffer, buffer.Length, ref bytesRead);
            return Encoding.UTF8.GetString(buffer).Split('\0')[0];
        }

        public IntPtr GetGlobalAddressFromPattern(string pattern, int offset, int size)
        {
            var scanResult = _patternScanner.Find(new DwordPattern(pattern));
            if (!scanResult.Found)
            {
                return IntPtr.Zero;
            }

            return IntPtr.Add(scanResult.ReadAddress, _processSharp.Memory.Read<int>(scanResult.ReadAddress + offset)) +
                   offset + size;
        }

        public int GetGlobalOffsetByPattern(string pattern, int offset)
        {
            var address = GetGlobalAddressFromPattern(pattern, offset, 4);
            return (int)(address.ToInt64() - BaseAddress.ToInt64());
        }

        public int GetFunctionOffsetByPattern(string pattern, int offset = 0)
        {
            var result = _patternScanner.Find(new DwordPattern(pattern));
            return result.Found ? result.Offset + offset : 0;
        }


        public void SuspendAllThread()
        {
            // _processSharp.Native.MainModule.
            foreach (ProcessThread thread in _processSharp.Native.Threads)
            {
                var hThread = Kernel32.OpenThread(Kernel32.ThreadAccess.SUSPEND_RESUME, false, (uint)thread.Id);
                if (hThread != IntPtr.Zero)
                {
                    Kernel32.SuspendThread(hThread);
                    Kernel32.CloseHandle(hThread);
                }
            }
        }

        public void ResumeAllThread()
        {
            foreach (ProcessThread thread in _processSharp.Native.Threads)
            {
                var hThread = Kernel32.OpenThread(Kernel32.ThreadAccess.SUSPEND_RESUME, false, (uint)thread.Id);
                if (hThread != IntPtr.Zero)
                {
                    while (true)
                    {
                        if (Kernel32.ResumeThread(hThread) <= 0)
                        {
                            break;
                        }
                    }

                    Kernel32.CloseHandle(hThread);
                }
            }
        }
    }
}