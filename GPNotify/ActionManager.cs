using Dalamud.Plugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GPNotify
{
    //(c) Caraxi / Remind me  https://github.com/Caraxi/RemindMe
    class ActionManager
    {
        public const byte PotionCDGroup = 69;
        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate IntPtr GetActionCooldownSlotDelegate(IntPtr actionManager, int cooldownGroup);

        private GetActionCooldownSlotDelegate getActionCooldownSlot;
        private IntPtr actionManagerStatic;


        public ActionManager(GPNotify plugin)
        {
            actionManagerStatic = plugin.pi.TargetModuleScanner
                .GetStaticAddressFromSig("48 89 05 ?? ?? ?? ?? C3 CC C2 00 00 CC CC CC CC CC CC CC CC CC CC CC CC CC 48 8D 0D ?? ?? ?? ?? E9 ?? ?? ?? ??");
            var getActionCooldownSlotScan = plugin.pi.TargetModuleScanner.ScanText("E8 ?? ?? ?? ?? 0F 57 FF 48 85 C0");
            getActionCooldownSlot = Marshal.GetDelegateForFunctionPointer<GetActionCooldownSlotDelegate>(getActionCooldownSlotScan);
        }
        public IntPtr GetCooldownPointer(byte actionCooldownGroup)
        {
            return getActionCooldownSlot(actionManagerStatic, actionCooldownGroup - 1);
        }

        public CooldownStruct GetCooldown(byte cdGroup)
        {
            var cooldownPtr = this.GetCooldownPointer(cdGroup);
            return Marshal.PtrToStructure<CooldownStruct>(cooldownPtr);
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct CooldownStruct
    {
        [FieldOffset(0x0)] public bool IsCooldown;

        [FieldOffset(0x4)] public uint ActionID;
        [FieldOffset(0x8)] public float CooldownElapsed;
        [FieldOffset(0xC)] public float CooldownTotal;
    }
}
