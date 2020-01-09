using System;
using System.Text;

namespace AirjME
{
    public class ScriptExecutor
    {

        private IntPtr _scriptData = IntPtr.Zero;
        private readonly WowProcess _process;
        public ScriptExecutor(WowProcess process)
        {
            this._process = process;
        }

        private IntPtr WriteScriptBytes(byte[] data)
        {
            if (_scriptData == IntPtr.Zero)
            {
                _scriptData = Kernel32.VirtualAllocEx(_process.handle, 0, 1024, Kernel32.AllocationType.Commit,
                    Kernel32.MemoryProtection.ReadWrite);
            }

            Kernel32.WriteProcessMemory(_process.handle, _scriptData.ToInt64(), data, data.Length, out _);
            return _scriptData;
        }

        public IntPtr RunScript(string script)
        {
            var dataAddress = WriteScriptBytes(Encoding.UTF8.GetBytes(script + "\0"));
            var offset = _process.GetOffset("runScript");
            var result = _process.CallExecutor.CallFunction(offset, dataAddress.ToInt64(), dataAddress.ToInt64(), 0);
            return new IntPtr(result);
        }

        public IntPtr RunMacroText(string macro)
        {
            return RunScript($"RunMacroText(\"{macro}\")");
        }

    }
}