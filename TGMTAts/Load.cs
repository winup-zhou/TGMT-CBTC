using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using System.Linq;
using System.Reflection;

namespace TGMTAts{
	public static partial class TGMTAts {

        public static int[] panel = new int[256];
        public static bool doorOpen;
        public static AtsVehicleSpec vehicleSpec;
        public static double location = -114514;

        public static List<string> debugMessages = new List<string>();
        
        // 0: RM; 1: SM-I; 2: SM-C; 3: AM-I; 4: AM-C; 5: XAM
        public static int selectedMode = 4;
        // 0: RM; 1: SM; 2: AM; 3: XAM
        public static int driveMode = 1;
        // 0: IXL; 1: ITC; 2: CTC
        public static int signalMode = 2;
        // 1: MM; 2: AM; 3: AA
        public static int doorMode = 1;
        // 0: 没有CTC,ITC; 1: 没有CTC; 2: 正常
        public static int deviceCapability = 2;

        // 暂时的预选速度，-1表示没有在预选
        public static int selectingMode = -1;
        public static int selectModeStartTime = 0;

        public static int ebState = 0;
        public static bool releaseSpeed = false;
        public static int ackMessage = 0;

        public static double reverseStartLocation = Config.LessInf;
        
        public static TrackLimit trackLimit = new TrackLimit();

        public static Form debugWindow;
        public static bool pluginReady = false;

        public static HarmonyLib.Harmony harmony = new HarmonyLib.Harmony("cn.zbx1425.bve.trainguardmt");

        [DllExport(CallingConvention.StdCall)]
        public static void Load(){
            Config.Load(Path.Combine(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), 
                "TGMTConfig.txt")
            );

            if (Config.Debug) {
                new Thread(() => {
                    debugWindow = new DebugWindow();
                    Application.Run(debugWindow);
                }).Start();
            }

            harmony.PatchAll();
            TGMTPainter.Initialize();
        }

        [DllExport(CallingConvention.StdCall)]
        public static void SetVehicleSpec(AtsVehicleSpec spec) {
            vehicleSpec = spec;
        }

        [DllExport(CallingConvention.StdCall)]
        public static void Initialize(int initialHandlePosition) {
            driveMode = 1;
            signalMode = 2;
            FixIncompatibleModes();
        }

        static void FixIncompatibleModes() {
            if (selectedMode == 0) signalMode = 0; // 预选了IXL
            if (selectedMode == 1 && signalMode > 1) signalMode = 1; // 预选了ITC
            if (selectedMode == 3 && signalMode > 1) signalMode = 1; // 预选了ITC

            if (deviceCapability == 0) signalMode = 0; // 没有TGMT设备
            if (deviceCapability == 1 && signalMode > 1) signalMode = 1; // 没有无线电信号

            if (signalMode > 0 && driveMode == 0) driveMode = 1; // 有信号就至少是SM
            if (signalMode == 0 && driveMode > 0) driveMode = 0; // 没信号就得是RM
        }

        static int ConvertTime(int human) {
            var hrs = human / 10000 % 60;
            var min = human / 100 % 60;
            var src = human % 60;
            return hrs * 3600 + min * 60 + src;
        }

        [DllExport(CallingConvention.StdCall)]
        public static void SetSignal(int signal){

		}

        [DllExport(CallingConvention.StdCall)]
        public static void Dispose() {
            if (debugWindow != null) debugWindow.Close();
            TGMTPainter.Dispose();
            TextureManager.Dispose();
        }

        public static void Log(string msg) {
            time /= 1000;
            var hrs = time / 3600 % 60;
            var min = time / 60 % 60;
            var sec = time % 60;
            debugMessages.Add(string.Format("{0:D2}:{1:D2}:{2:D2} {3}", hrs, min, sec, msg));
        }
	}
}