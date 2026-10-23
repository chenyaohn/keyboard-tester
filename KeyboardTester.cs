using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace KeyboardTester
{
    internal static class Program
    {
        [STAThread]
        private static int Main(string[] args)
        {
            if (args != null && args.Any(delegate(string arg)
            {
                return string.Equals(arg, "--self-test", StringComparison.OrdinalIgnoreCase);
            }))
            {
                return SelfTests.Run();
            }

            try
            {
                SetProcessDPIAware();
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new MainForm());
                return 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "键盘检测器启动失败：\r\n\r\n" + ex.Message,
                    "键盘检测器",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return 1;
            }
        }

        [DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();
    }

    internal static class Theme
    {
        public static readonly Color Background = Color.FromArgb(10, 13, 20);
        public static readonly Color Surface = Color.FromArgb(17, 22, 32);
        public static readonly Color SurfaceRaised = Color.FromArgb(23, 29, 42);
        public static readonly Color Key = Color.FromArgb(27, 33, 46);
        public static readonly Color KeyFunction = Color.FromArgb(21, 27, 38);
        public static readonly Color KeyHover = Color.FromArgb(37, 46, 63);
        public static readonly Color KeyPressed = Color.FromArgb(47, 129, 247);
        public static readonly Color KeyTested = Color.FromArgb(22, 52, 45);
        public static readonly Color Foreground = Color.FromArgb(235, 241, 250);
        public static readonly Color Muted = Color.FromArgb(145, 157, 176);
        public static readonly Color Border = Color.FromArgb(43, 52, 69);
        public static readonly Color Accent = Color.FromArgb(77, 208, 225);
        public static readonly Color Success = Color.FromArgb(74, 222, 128);
        public static readonly Color Warning = Color.FromArgb(251, 191, 36);
        public static readonly Color Danger = Color.FromArgb(248, 113, 113);

        public static Font Font(float size, FontStyle style)
        {
            try
            {
                return new Font("Microsoft YaHei UI", size, style, GraphicsUnit.Point);
            }
            catch
            {
                return new Font(FontFamily.GenericSansSerif, size, style, GraphicsUnit.Point);
            }
        }
    }

    internal enum LayoutMode
    {
        Full104,
        Tenkeyless87
    }

    internal sealed class KeySpec
    {
        public int Id;
        public uint VirtualKey;
        public bool Extended;
        public string Label;
        public string SecondaryLabel;
        public string Description;
        public float X;
        public float Y;
        public float Width;
        public float Height;
        public bool InNumpad;
        public bool IsFunctionRow;

        public KeySpec(
            int id,
            uint virtualKey,
            bool extended,
            string label,
            string secondaryLabel,
            float x,
            float y,
            float width,
            float height,
            bool inNumpad,
            bool isFunctionRow)
        {
            Id = id;
            VirtualKey = virtualKey;
            Extended = extended;
            Label = label;
            SecondaryLabel = secondaryLabel;
            Description = label;
            X = x;
            Y = y;
            Width = width;
            Height = height;
            InNumpad = inNumpad;
            IsFunctionRow = isFunctionRow;
        }
    }

    internal static class KeyboardModel
    {
        private const int NumpadEnterId = 0x210D;
        private static readonly List<KeySpec> AllSpecs;
        private static readonly Dictionary<int, KeySpec> SpecsById;
        private static readonly Dictionary<uint, KeySpec> StandardByVirtualKey;
        private static readonly Dictionary<uint, KeySpec> ExtendedByVirtualKey;
        private static readonly List<KeySpec> Full104;
        private static readonly List<KeySpec> Tenkeyless87;

        static KeyboardModel()
        {
            AllSpecs = new List<KeySpec>();
            SpecsById = new Dictionary<int, KeySpec>();
            StandardByVirtualKey = new Dictionary<uint, KeySpec>();
            ExtendedByVirtualKey = new Dictionary<uint, KeySpec>();

            Add(0x1B, false, "Esc", "", 0.00f, 0.00f, 1.00f, 0.90f, false, true);

            Add(0x70, false, "F1", "", 2.00f, 0.00f, 1.00f, 0.90f, false, true);
            Add(0x71, false, "F2", "", 3.00f, 0.00f, 1.00f, 0.90f, false, true);
            Add(0x72, false, "F3", "", 4.00f, 0.00f, 1.00f, 0.90f, false, true);
            Add(0x73, false, "F4", "", 5.00f, 0.00f, 1.00f, 0.90f, false, true);
            Add(0x74, false, "F5", "", 6.25f, 0.00f, 1.00f, 0.90f, false, true);
            Add(0x75, false, "F6", "", 7.25f, 0.00f, 1.00f, 0.90f, false, true);
            Add(0x76, false, "F7", "", 8.25f, 0.00f, 1.00f, 0.90f, false, true);
            Add(0x77, false, "F8", "", 9.25f, 0.00f, 1.00f, 0.90f, false, true);
            Add(0x78, false, "F9", "", 10.50f, 0.00f, 1.00f, 0.90f, false, true);
            Add(0x79, false, "F10", "", 11.50f, 0.00f, 1.00f, 0.90f, false, true);
            Add(0x7A, false, "F11", "", 12.50f, 0.00f, 1.00f, 0.90f, false, true);
            Add(0x7B, false, "F12", "", 13.50f, 0.00f, 1.00f, 0.90f, false, true);

            Add(0x2C, false, "PrtSc", "", 15.40f, 0.00f, 1.00f, 0.90f, false, true);
            Add(0x91, false, "ScrLk", "", 16.40f, 0.00f, 1.00f, 0.90f, false, true);
            Add(0x13, false, "Pause", "", 17.40f, 0.00f, 1.00f, 0.90f, false, true);

            Add(0xC0, false, "`", "~", 0.00f, 1.25f, 1.00f, 0.95f, false, false);
            Add(0x31, false, "1", "!", 1.00f, 1.25f, 1.00f, 0.95f, false, false);
            Add(0x32, false, "2", "@", 2.00f, 1.25f, 1.00f, 0.95f, false, false);
            Add(0x33, false, "3", "#", 3.00f, 1.25f, 1.00f, 0.95f, false, false);
            Add(0x34, false, "4", "$", 4.00f, 1.25f, 1.00f, 0.95f, false, false);
            Add(0x35, false, "5", "%", 5.00f, 1.25f, 1.00f, 0.95f, false, false);
            Add(0x36, false, "6", "^", 6.00f, 1.25f, 1.00f, 0.95f, false, false);
            Add(0x37, false, "7", "&", 7.00f, 1.25f, 1.00f, 0.95f, false, false);
            Add(0x38, false, "8", "*", 8.00f, 1.25f, 1.00f, 0.95f, false, false);
            Add(0x39, false, "9", "(", 9.00f, 1.25f, 1.00f, 0.95f, false, false);
            Add(0x30, false, "0", ")", 10.00f, 1.25f, 1.00f, 0.95f, false, false);
            Add(0xBD, false, "-", "_", 11.00f, 1.25f, 1.00f, 0.95f, false, false);
            Add(0xBB, false, "=", "+", 12.00f, 1.25f, 1.00f, 0.95f, false, false);
            Add(0x08, false, "Backspace", "", 13.00f, 1.25f, 2.00f, 0.95f, false, false);

            Add(0x09, false, "Tab", "", 0.00f, 2.32f, 1.50f, 0.95f, false, false);
            Add(0x51, false, "Q", "", 1.50f, 2.32f, 1.00f, 0.95f, false, false);
            Add(0x57, false, "W", "", 2.50f, 2.32f, 1.00f, 0.95f, false, false);
            Add(0x45, false, "E", "", 3.50f, 2.32f, 1.00f, 0.95f, false, false);
            Add(0x52, false, "R", "", 4.50f, 2.32f, 1.00f, 0.95f, false, false);
            Add(0x54, false, "T", "", 5.50f, 2.32f, 1.00f, 0.95f, false, false);
            Add(0x59, false, "Y", "", 6.50f, 2.32f, 1.00f, 0.95f, false, false);
            Add(0x55, false, "U", "", 7.50f, 2.32f, 1.00f, 0.95f, false, false);
            Add(0x49, false, "I", "", 8.50f, 2.32f, 1.00f, 0.95f, false, false);
            Add(0x4F, false, "O", "", 9.50f, 2.32f, 1.00f, 0.95f, false, false);
            Add(0x50, false, "P", "", 10.50f, 2.32f, 1.00f, 0.95f, false, false);
            Add(0xDB, false, "[", "{", 11.50f, 2.32f, 1.00f, 0.95f, false, false);
            Add(0xDD, false, "]", "}", 12.50f, 2.32f, 1.00f, 0.95f, false, false);
            Add(0xDC, false, "\\", "|", 13.50f, 2.32f, 1.50f, 0.95f, false, false);

            Add(0x14, false, "Caps", "", 0.00f, 3.39f, 1.75f, 0.95f, false, false);
            Add(0x41, false, "A", "", 1.75f, 3.39f, 1.00f, 0.95f, false, false);
            Add(0x53, false, "S", "", 2.75f, 3.39f, 1.00f, 0.95f, false, false);
            Add(0x44, false, "D", "", 3.75f, 3.39f, 1.00f, 0.95f, false, false);
            Add(0x46, false, "F", "", 4.75f, 3.39f, 1.00f, 0.95f, false, false);
            Add(0x47, false, "G", "", 5.75f, 3.39f, 1.00f, 0.95f, false, false);
            Add(0x48, false, "H", "", 6.75f, 3.39f, 1.00f, 0.95f, false, false);
            Add(0x4A, false, "J", "", 7.75f, 3.39f, 1.00f, 0.95f, false, false);
            Add(0x4B, false, "K", "", 8.75f, 3.39f, 1.00f, 0.95f, false, false);
            Add(0x4C, false, "L", "", 9.75f, 3.39f, 1.00f, 0.95f, false, false);
            Add(0xBA, false, ";", ":", 10.75f, 3.39f, 1.00f, 0.95f, false, false);
            Add(0xDE, false, "'", "\"", 11.75f, 3.39f, 1.00f, 0.95f, false, false);
            Add(0x0D, false, "Enter", "", 12.75f, 3.39f, 2.25f, 0.95f, false, false);

            Add(0xA0, false, "Shift", "", 0.00f, 4.46f, 2.25f, 0.95f, false, false);
            Add(0x5A, false, "Z", "", 2.25f, 4.46f, 1.00f, 0.95f, false, false);
            Add(0x58, false, "X", "", 3.25f, 4.46f, 1.00f, 0.95f, false, false);
            Add(0x43, false, "C", "", 4.25f, 4.46f, 1.00f, 0.95f, false, false);
            Add(0x56, false, "V", "", 5.25f, 4.46f, 1.00f, 0.95f, false, false);
            Add(0x42, false, "B", "", 6.25f, 4.46f, 1.00f, 0.95f, false, false);
            Add(0x4E, false, "N", "", 7.25f, 4.46f, 1.00f, 0.95f, false, false);
            Add(0x4D, false, "M", "", 8.25f, 4.46f, 1.00f, 0.95f, false, false);
            Add(0xBC, false, ",", "<", 9.25f, 4.46f, 1.00f, 0.95f, false, false);
            Add(0xBE, false, ".", ">", 10.25f, 4.46f, 1.00f, 0.95f, false, false);
            Add(0xBF, false, "/", "?", 11.25f, 4.46f, 1.00f, 0.95f, false, false);
            Add(0xA1, false, "Shift", "", 12.25f, 4.46f, 2.75f, 0.95f, false, false);

            Add(0xA2, false, "Ctrl", "", 0.00f, 5.53f, 1.25f, 0.95f, false, false);
            Add(0x5B, false, "Win", "", 1.25f, 5.53f, 1.25f, 0.95f, false, false);
            Add(0xA4, false, "Alt", "", 2.50f, 5.53f, 1.25f, 0.95f, false, false);
            Add(0x20, false, "Space", "", 3.75f, 5.53f, 6.25f, 0.95f, false, false);
            Add(0xA5, false, "Alt", "", 10.00f, 5.53f, 1.25f, 0.95f, false, false);
            Add(0x5C, false, "Win", "", 11.25f, 5.53f, 1.25f, 0.95f, false, false);
            Add(0x5D, false, "Menu", "", 12.50f, 5.53f, 1.25f, 0.95f, false, false);
            Add(0xA3, false, "Ctrl", "", 13.75f, 5.53f, 1.25f, 0.95f, false, false);

            Add(0x2D, false, "Insert", "", 15.40f, 1.25f, 1.00f, 0.95f, false, false);
            Add(0x24, false, "Home", "", 16.40f, 1.25f, 1.00f, 0.95f, false, false);
            Add(0x21, false, "PgUp", "", 17.40f, 1.25f, 1.00f, 0.95f, false, false);
            Add(0x2E, false, "Delete", "", 15.40f, 2.32f, 1.00f, 0.95f, false, false);
            Add(0x23, false, "End", "", 16.40f, 2.32f, 1.00f, 0.95f, false, false);
            Add(0x22, false, "PgDn", "", 17.40f, 2.32f, 1.00f, 0.95f, false, false);
            Add(0x26, true, "↑", "", 16.40f, 3.85f, 1.00f, 0.95f, false, false);
            Add(0x25, true, "←", "", 15.40f, 5.02f, 1.00f, 0.95f, false, false);
            Add(0x28, true, "↓", "", 16.40f, 5.02f, 1.00f, 0.95f, false, false);
            Add(0x27, true, "→", "", 17.40f, 5.02f, 1.00f, 0.95f, false, false);

            Add(0x90, false, "Num", "Lock", 19.00f, 1.25f, 1.00f, 0.95f, true, false);
            Add(0x6F, false, "/", "", 20.00f, 1.25f, 1.00f, 0.95f, true, false);
            Add(0x6A, false, "*", "", 21.00f, 1.25f, 1.00f, 0.95f, true, false);
            Add(0x6D, false, "-", "", 22.00f, 1.25f, 1.00f, 0.95f, true, false);
            Add(0x67, false, "7", "", 19.00f, 2.32f, 1.00f, 0.95f, true, false);
            Add(0x68, false, "8", "", 20.00f, 2.32f, 1.00f, 0.95f, true, false);
            Add(0x69, false, "9", "", 21.00f, 2.32f, 1.00f, 0.95f, true, false);
            Add(0x6B, false, "+", "", 22.00f, 2.32f, 1.00f, 2.02f, true, false);
            Add(0x64, false, "4", "", 19.00f, 3.39f, 1.00f, 0.95f, true, false);
            Add(0x65, false, "5", "", 20.00f, 3.39f, 1.00f, 0.95f, true, false);
            Add(0x66, false, "6", "", 21.00f, 3.39f, 1.00f, 0.95f, true, false);
            Add(0x61, false, "1", "", 19.00f, 4.46f, 1.00f, 0.95f, true, false);
            Add(0x62, false, "2", "", 20.00f, 4.46f, 1.00f, 0.95f, true, false);
            Add(0x63, false, "3", "", 21.00f, 4.46f, 1.00f, 0.95f, true, false);
            Add(NumpadEnterId, 0x0D, true, "Enter", "", 22.00f, 4.46f, 1.00f, 2.02f, true, false);
            Add(0x60, false, "0", "", 19.00f, 5.53f, 2.00f, 0.95f, true, false);
            Add(0x6E, false, ".", "", 21.00f, 5.53f, 1.00f, 0.95f, true, false);

            Full104 = new List<KeySpec>(AllSpecs);
            Tenkeyless87 = AllSpecs.Where(delegate(KeySpec spec) { return !spec.InNumpad; }).ToList();
        }

        public static IList<KeySpec> All
        {
            get { return AllSpecs.AsReadOnly(); }
        }

        public static IList<KeySpec> GetVisible(LayoutMode mode)
        {
            return mode == LayoutMode.Full104 ? Full104.AsReadOnly() : Tenkeyless87.AsReadOnly();
        }

        public static KeySpec GetById(int id)
        {
            KeySpec spec;
            SpecsById.TryGetValue(id, out spec);
            return spec;
        }

        public static KeySpec Resolve(uint virtualKey, bool extended)
        {
            return Resolve(virtualKey, extended, 0);
        }

        public static KeySpec Resolve(uint virtualKey, bool extended, uint scanCode)
        {
            if (!extended)
            {
                int navigationKey = (int)virtualKey;
                if (scanCode == 0x48 && navigationKey == 0x26)
                {
                    return GetById(0x68);
                }
                if (scanCode == 0x50 && navigationKey == 0x28)
                {
                    return GetById(0x62);
                }
                if (scanCode == 0x4B && navigationKey == 0x25)
                {
                    return GetById(0x64);
                }
                if (scanCode == 0x4D && navigationKey == 0x27)
                {
                    return GetById(0x66);
                }
                if (scanCode == 0x47 && navigationKey == 0x24)
                {
                    return GetById(0x67);
                }
                if (scanCode == 0x4F && navigationKey == 0x23)
                {
                    return GetById(0x61);
                }
                if (scanCode == 0x49 && navigationKey == 0x21)
                {
                    return GetById(0x69);
                }
                if (scanCode == 0x51 && navigationKey == 0x22)
                {
                    return GetById(0x63);
                }
                if (scanCode == 0x52 && navigationKey == 0x2D)
                {
                    return GetById(0x60);
                }
                if (scanCode == 0x53 && navigationKey == 0x2E)
                {
                    return GetById(0x6E);
                }
            }

            int key = (int)virtualKey;
            if (key == 0x0D && extended)
            {
                return GetById(NumpadEnterId);
            }

            KeySpec spec;
            if (extended && ExtendedByVirtualKey.TryGetValue(virtualKey, out spec))
            {
                return spec;
            }

            if (StandardByVirtualKey.TryGetValue(virtualKey, out spec))
            {
                return spec;
            }

            if (key == 0x5B || key == 0x5C || key == 0xA4 || key == 0xA5 ||
                key == 0xA2 || key == 0xA3 || key == 0xA0 || key == 0xA1)
            {
                return GetById(key);
            }

            return null;
        }

        public static string DescribeVirtualKey(uint virtualKey)
        {
            return "VK 0x" + virtualKey.ToString("X2");
        }

        private static void Add(
            uint virtualKey,
            bool extended,
            string label,
            string secondaryLabel,
            float x,
            float y,
            float width,
            float height,
            bool inNumpad,
            bool functionRow)
        {
            int id = (int)virtualKey;
            KeySpec spec = new KeySpec(
                id,
                virtualKey,
                extended,
                label,
                secondaryLabel,
                x,
                y,
                width,
                height,
                inNumpad,
                functionRow);
            AllSpecs.Add(spec);
            SpecsById[id] = spec;
            if (!extended)
            {
                StandardByVirtualKey[virtualKey] = spec;
            }
            else if (id == (int)virtualKey)
            {
                ExtendedByVirtualKey[virtualKey] = spec;
            }
        }

        private static void Add(
            int id,
            uint virtualKey,
            bool extended,
            string label,
            string secondaryLabel,
            float x,
            float y,
            float width,
            float height,
            bool inNumpad,
            bool functionRow)
        {
            KeySpec spec = new KeySpec(
                id,
                virtualKey,
                extended,
                label,
                secondaryLabel,
                x,
                y,
                width,
                height,
                inNumpad,
                functionRow);
            AllSpecs.Add(spec);
            SpecsById[id] = spec;
            if (extended && id == (int)virtualKey)
            {
                ExtendedByVirtualKey[virtualKey] = spec;
            }
        }
    }

    internal sealed class KeyResult
    {
        public KeySpec Spec;
        public int DownCount;
        public int UpCount;
        public int RepeatCount;
        public int BounceCount;
        public long LastDownMs;
        public long LastUpMs = -1;
        public long LastHoldMs;
        public long MaxHoldMs;
        public bool IsDown;
        public bool LongHoldNotified;
        public bool HasBounceWarning;

        public KeyResult(KeySpec spec)
        {
            Spec = spec;
        }

        public bool Tested
        {
            get { return UpCount > 0; }
        }
    }

    internal enum EventKind
    {
        Neutral,
        Press,
        Release,
        Warning,
        Success
    }

    internal sealed class EventEntry
    {
        public long TimeMs;
        public string Text;
        public EventKind Kind;

        public EventEntry(long timeMs, string text, EventKind kind)
        {
            TimeMs = timeMs;
            Text = text;
            Kind = kind;
        }
    }

    internal sealed class KeyboardSession
    {
        private readonly Stopwatch clock;
        private readonly Dictionary<int, KeyResult> results;
        private readonly List<EventEntry> recentEvents;
        private readonly LayoutMode mode;
        private readonly List<KeySpec> visibleSpecs;
        private int ignoredInjectedEvents;

        public KeyboardSession(LayoutMode layoutMode)
        {
            clock = Stopwatch.StartNew();
            results = new Dictionary<int, KeyResult>();
            recentEvents = new List<EventEntry>();
            mode = layoutMode;
            visibleSpecs = new List<KeySpec>(KeyboardModel.GetVisible(mode));
            foreach (KeySpec spec in visibleSpecs)
            {
                results[spec.Id] = new KeyResult(spec);
            }
        }

        public LayoutMode Mode
        {
            get { return mode; }
        }

        public KeySpec LatestKey { get; private set; }
        public bool LatestIsDown { get; private set; }
        public long LatestEventMs { get; private set; }
        public long LatestDurationMs { get; private set; }

        public long NowMs
        {
            get { return clock.ElapsedMilliseconds; }
        }

        public IEnumerable<KeyResult> Results
        {
            get { return results.Values; }
        }

        public int TotalCount
        {
            get { return visibleSpecs.Count; }
        }

        public int TestedCount
        {
            get { return results.Values.Count(delegate(KeyResult r) { return r.Tested; }); }
        }

        public int PressedCount
        {
            get { return results.Values.Count(delegate(KeyResult r) { return r.IsDown; }); }
        }

        public int WarningCount
        {
            get
            {
                long now = NowMs;
                return results.Values.Count(delegate(KeyResult r)
                {
                    return r.HasBounceWarning || (r.IsDown && now - r.LastDownMs >= 5000);
                });
            }
        }

        public int IgnoredInjectedEvents
        {
            get { return ignoredInjectedEvents; }
        }

        public IList<EventEntry> RecentEvents
        {
            get { return recentEvents.AsReadOnly(); }
        }

        public KeyResult GetResult(KeySpec spec)
        {
            if (spec == null)
            {
                return null;
            }

            KeyResult result;
            results.TryGetValue(spec.Id, out result);
            return result;
        }

        public HashSet<int> GetPressedIds()
        {
            return new HashSet<int>(results.Values
                .Where(delegate(KeyResult r) { return r.IsDown; })
                .Select(delegate(KeyResult r) { return r.Spec.Id; }));
        }

        public HashSet<int> GetTestedIds()
        {
            return new HashSet<int>(results.Values
                .Where(delegate(KeyResult r) { return r.Tested; })
                .Select(delegate(KeyResult r) { return r.Spec.Id; }));
        }

        public HashSet<int> GetWarningIds()
        {
            long now = NowMs;
            return new HashSet<int>(results.Values
                .Where(delegate(KeyResult r)
                {
                    return r.HasBounceWarning || (r.IsDown && now - r.LastDownMs >= 5000);
                })
                .Select(delegate(KeyResult r) { return r.Spec.Id; }));
        }

        public void Process(uint virtualKey, bool extended, bool isDown, bool injected)
        {
            Process(virtualKey, extended, 0, isDown, injected);
        }

        public void Process(uint virtualKey, bool extended, uint scanCode, bool isDown, bool injected)
        {
            ProcessAt(virtualKey, extended, scanCode, isDown, injected, NowMs);
        }

        public void ProcessAt(uint virtualKey, bool extended, bool isDown, bool injected, long nowMs)
        {
            ProcessAt(virtualKey, extended, 0, isDown, injected, nowMs);
        }

        public void ProcessAt(
            uint virtualKey,
            bool extended,
            uint scanCode,
            bool isDown,
            bool injected,
            long nowMs)
        {
            if (injected)
            {
                ignoredInjectedEvents++;
                AddEvent(nowMs, "已忽略一条模拟输入", EventKind.Neutral);
                return;
            }

            KeySpec spec = KeyboardModel.Resolve(virtualKey, extended, scanCode);
            if (spec == null)
            {
                string name = KeyboardModel.DescribeVirtualKey(virtualKey);
                LatestKey = null;
                LatestIsDown = isDown;
                LatestEventMs = nowMs;
                LatestDurationMs = 0;
                AddEvent(nowMs, (isDown ? "按下" : "松开") + "未映射按键 " + name, EventKind.Warning);
                return;
            }

            KeyResult result;
            if (!results.TryGetValue(spec.Id, out result))
            {
                return;
            }

            LatestKey = spec;
            LatestIsDown = isDown;
            LatestEventMs = nowMs;
            LatestDurationMs = 0;

            if (isDown)
            {
                if (result.IsDown)
                {
                    result.RepeatCount++;
                    if (result.RepeatCount == 1)
                    {
                        AddEvent(nowMs, spec.Label + " 开始自动重复", EventKind.Neutral);
                    }
                    return;
                }

                if (result.LastUpMs >= 0 && nowMs - result.LastUpMs < 45 && result.DownCount > 0)
                {
                    result.BounceCount++;
                    if (result.BounceCount >= 1)
                    {
                        result.HasBounceWarning = true;
                        AddEvent(nowMs, spec.Label + " 疑似出现快速抖动", EventKind.Warning);
                    }
                }

                result.DownCount++;
                result.IsDown = true;
                result.LastDownMs = nowMs;
                result.LongHoldNotified = false;
                AddEvent(nowMs, "按下 " + spec.Label, EventKind.Press);
                return;
            }

            if (!result.IsDown)
            {
                AddEvent(nowMs, spec.Label + " 收到松开事件，但没有对应的按下记录", EventKind.Warning);
                return;
            }

            long hold = Math.Max(0, nowMs - result.LastDownMs);
            result.IsDown = false;
            result.UpCount++;
            result.LastUpMs = nowMs;
            result.LastHoldMs = hold;
            result.MaxHoldMs = Math.Max(result.MaxHoldMs, hold);
            LatestDurationMs = hold;
            AddEvent(
                nowMs,
                "松开 " + spec.Label + " · " + hold + " ms",
                result.HasBounceWarning ? EventKind.Warning : EventKind.Release);
        }

        public void CheckLongHolds()
        {
            long now = NowMs;
            foreach (KeyResult result in results.Values)
            {
                if (result.IsDown && !result.LongHoldNotified && now - result.LastDownMs >= 5000)
                {
                    result.LongHoldNotified = true;
                    AddEvent(now, result.Spec.Label + " 已按住 5 秒，仍未收到松开", EventKind.Warning);
                }
            }
        }

        public void ResetPressedForFocusLoss()
        {
            long now = NowMs;
            bool changed = false;
            foreach (KeyResult result in results.Values)
            {
                if (result.IsDown)
                {
                    result.IsDown = false;
                    result.LongHoldNotified = false;
                    changed = true;
                }
            }

            LatestIsDown = false;
            if (changed)
            {
                AddEvent(now, "窗口失焦，已重置按下状态", EventKind.Neutral);
            }
        }

        public List<string> GetWarnings()
        {
            List<string> warnings = new List<string>();
            long now = NowMs;
            foreach (KeyResult result in results.Values)
            {
                if (result.HasBounceWarning)
                {
                    warnings.Add(result.Spec.Label + "：疑似连续抖动 " + result.BounceCount + " 次");
                }
                if (result.IsDown && now - result.LastDownMs >= 5000)
                {
                    warnings.Add(result.Spec.Label + "：按住 5 秒仍未松开");
                }
            }
            return warnings;
        }

        public string BuildReport()
        {
            StringBuilder text = new StringBuilder();
            text.AppendLine("键盘检测报告");
            text.AppendLine("生成时间：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            text.AppendLine("测试布局：" + (mode == LayoutMode.Full104 ? "104 键全尺寸" : "87 键（无数字区）"));
            text.AppendLine("检测时长：" + FormatDuration(NowMs));
            text.AppendLine();
            text.AppendLine("结果摘要");
            text.AppendLine("已测试：" + TestedCount + " / " + TotalCount);
            text.AppendLine("当前按下：" + PressedCount);
            text.AppendLine("未测试：" + (TotalCount - TestedCount));
            text.AppendLine("疑似异常：" + WarningCount);
            text.AppendLine("忽略模拟输入：" + ignoredInjectedEvents);
            text.AppendLine();

            List<string> warnings = GetWarnings();
            text.AppendLine("注意事项");
            if (warnings.Count == 0)
            {
                text.AppendLine("暂未发现快速抖动或长时间未松开。");
            }
            else
            {
                foreach (string warning in warnings)
                {
                    text.AppendLine("- " + warning);
                }
            }

            text.AppendLine();
            text.AppendLine("尚未测试的按键");
            List<string> untested = results.Values
                .Where(delegate(KeyResult r) { return !r.Tested; })
                .Select(delegate(KeyResult r) { return r.Spec.Label; })
                .ToList();
            text.AppendLine(untested.Count == 0 ? "无" : string.Join("、", untested.ToArray()));

            text.AppendLine();
            text.AppendLine("逐键记录");
            foreach (KeyResult result in results.Values.OrderBy(delegate(KeyResult r) { return r.Spec.Label; }))
            {
                string mark = result.HasBounceWarning ? "!" : result.Tested ? "OK" : "--";
                text.AppendFormat(
                    "[{0,-2}] {1,-12} 按下 {2} 次，松开 {3} 次，自动重复 {4} 次，最长按住 {5} ms",
                    mark,
                    result.Spec.Label,
                    result.DownCount,
                    result.UpCount,
                    result.RepeatCount,
                    result.MaxHoldMs);
                text.AppendLine();
            }

            text.AppendLine();
            text.AppendLine("说明：本程序通过系统级键盘钩子记录按键按下与松开事件。");
            text.AppendLine("未测试的按键不能判定为正常；Ctrl+Alt+Delete 等 Windows 安全组合键无法被普通程序拦截。");
            return text.ToString();
        }

        public static string FormatDuration(long milliseconds)
        {
            TimeSpan span = TimeSpan.FromMilliseconds(milliseconds);
            if (span.TotalHours >= 1)
            {
                return ((int)span.TotalHours) + " 小时 " + span.Minutes + " 分 " + span.Seconds + " 秒";
            }
            if (span.TotalMinutes >= 1)
            {
                return span.Minutes + " 分 " + span.Seconds + " 秒";
            }
            return Math.Max(0, span.Seconds) + " 秒";
        }

        private void AddEvent(long timeMs, string text, EventKind kind)
        {
            recentEvents.Add(new EventEntry(timeMs, text, kind));
            if (recentEvents.Count > 30)
            {
                recentEvents.RemoveAt(0);
            }
        }
    }

    internal sealed class KeyboardControl : Control
    {
        private HashSet<int> pressedIds;
        private HashSet<int> testedIds;
        private HashSet<int> warningIds;
        private KeySpec hoveredSpec;
        private LayoutMode mode;

        public KeyboardControl()
        {
            pressedIds = new HashSet<int>();
            testedIds = new HashSet<int>();
            warningIds = new HashSet<int>();
            mode = LayoutMode.Full104;
            DoubleBuffered = true;
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            BackColor = Theme.Surface;
            Cursor = Cursors.Default;
        }

        public LayoutMode Mode
        {
            get { return mode; }
            set
            {
                mode = value;
                hoveredSpec = null;
                Invalidate();
            }
        }

        public void SetState(HashSet<int> pressed, HashSet<int> tested, HashSet<int> warnings)
        {
            pressedIds = pressed ?? new HashSet<int>();
            testedIds = tested ?? new HashSet<int>();
            warningIds = warnings ?? new HashSet<int>();
            Invalidate();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            KeySpec found = GetSpecAt(e.Location);
            if (!object.ReferenceEquals(found, hoveredSpec))
            {
                hoveredSpec = found;
                Cursor = found == null ? Cursors.Default : Cursors.Hand;
                Invalidate();
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            hoveredSpec = null;
            Cursor = Cursors.Default;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            g.Clear(Theme.Surface);

            Rectangle panel = new Rectangle(0, 0, Width - 1, Height - 1);
            using (GraphicsPath path = Drawing.RoundedRect(panel, 14))
            using (SolidBrush fill = new SolidBrush(Theme.Surface))
            using (Pen border = new Pen(Theme.Border))
            {
                g.FillPath(fill, path);
                g.DrawPath(border, path);
            }

            using (Font titleFont = Theme.Font(11.5f, FontStyle.Bold))
            using (Font smallFont = Theme.Font(8.5f, FontStyle.Regular))
            {
                TextRenderer.DrawText(
                    g,
                    "键盘状态",
                    titleFont,
                    new Rectangle(20, 13, 200, 26),
                    Theme.Foreground,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
                TextRenderer.DrawText(
                    g,
                    "绿色＝已回弹   橙色＝需要留意   蓝色＝按下中",
                    smallFont,
                    new Rectangle(20, 13, Width - 40, 26),
                    Theme.Muted,
                    TextFormatFlags.Right | TextFormatFlags.VerticalCenter);
            }

            IList<KeySpec> specs = KeyboardModel.GetVisible(mode);
            RectangleF board = new RectangleF(14, 48, Width - 28, Height - 62);
            float totalWidth = mode == LayoutMode.Full104 ? 23.0f : 18.4f;
            float totalHeight = 6.60f;
            float scaleX = board.Width / totalWidth;
            float scaleY = board.Height / totalHeight;
            float scale = Math.Min(scaleX, scaleY);
            float keyboardWidth = totalWidth * scale;
            float keyboardHeight = totalHeight * scale;
            float originX = board.Left + Math.Max(0, (board.Width - keyboardWidth) / 2f);
            float originY = board.Top + Math.Max(0, (board.Height - keyboardHeight) / 2f);

            foreach (KeySpec spec in specs)
            {
                RectangleF rect = new RectangleF(
                    originX + spec.X * scale,
                    originY + spec.Y * scale,
                    spec.Width * scale,
                    spec.Height * scale);
                rect.Width = Math.Max(1, rect.Width - Math.Max(2.0f, scale * 0.105f));
                rect.Height = Math.Max(1, rect.Height - Math.Max(2.0f, scale * 0.105f));
                DrawKey(g, spec, rect, scale);
            }
        }

        private void DrawKey(Graphics g, KeySpec spec, RectangleF rect, float scale)
        {
            bool pressed = pressedIds.Contains(spec.Id);
            bool tested = testedIds.Contains(spec.Id);
            bool warning = warningIds.Contains(spec.Id);
            bool hovered = object.ReferenceEquals(hoveredSpec, spec);

            Color fillColor = pressed
                ? Theme.KeyPressed
                : warning
                    ? Color.FromArgb(58, 47, 27)
                    : tested
                        ? Theme.KeyTested
                        : spec.IsFunctionRow ? Theme.KeyFunction : Theme.Key;
            if (!pressed && hovered)
            {
                fillColor = ControlPaint.Light(fillColor, 0.14f);
            }

            Color borderColor = pressed
                ? Color.FromArgb(116, 185, 255)
                : warning
                    ? Theme.Warning
                    : tested
                        ? Color.FromArgb(45, 112, 83)
                        : Theme.Border;

            using (GraphicsPath path = Drawing.RoundedRect(rect, Math.Min(8.0f, scale * 0.16f)))
            using (SolidBrush fill = new SolidBrush(fillColor))
            using (Pen border = new Pen(borderColor, pressed ? 1.4f : 1.0f))
            {
                g.FillPath(fill, path);
                g.DrawPath(border, path);
            }

            float fontSize = Math.Max(6.5f, Math.Min(10.5f, scale * 0.22f));
            if (spec.Label.Length >= 8)
            {
                fontSize *= 0.75f;
            }
            using (Font font = Theme.Font(fontSize, FontStyle.Bold))
            using (StringFormat center = new StringFormat())
            {
                center.Alignment = StringAlignment.Center;
                center.LineAlignment = StringAlignment.Center;
                center.Trimming = StringTrimming.EllipsisCharacter;
                RectangleF textRect = rect;
                if (!string.IsNullOrEmpty(spec.SecondaryLabel))
                {
                    textRect.Y += scale * 0.05f;
                    textRect.Height = rect.Height * 0.58f;
                }
                using (SolidBrush textBrush = new SolidBrush(Theme.Foreground))
                {
                    g.DrawString(spec.Label, font, textBrush, textRect, center);
                }

                if (!string.IsNullOrEmpty(spec.SecondaryLabel))
                {
                    float secondarySize = Math.Max(5.7f, fontSize * 0.72f);
                    using (Font secondaryFont = Theme.Font(secondarySize, FontStyle.Regular))
                    using (SolidBrush secondaryBrush = new SolidBrush(Theme.Muted))
                    {
                        RectangleF secondaryRect = new RectangleF(
                            rect.Left,
                            rect.Top + rect.Height * 0.56f,
                            rect.Width,
                            rect.Height * 0.35f);
                        g.DrawString(spec.SecondaryLabel, secondaryFont, secondaryBrush, secondaryRect, center);
                    }
                }
            }

            if (tested)
            {
                float dot = Math.Max(2.2f, Math.Min(4.5f, scale * 0.09f));
                using (SolidBrush dotBrush = new SolidBrush(warning ? Theme.Warning : Theme.Success))
                {
                    g.FillEllipse(dotBrush, rect.Right - dot - 4, rect.Top + 4, dot, dot);
                }
            }
        }

        private KeySpec GetSpecAt(Point point)
        {
            IList<KeySpec> specs = KeyboardModel.GetVisible(mode);
            RectangleF board = new RectangleF(14, 48, Width - 28, Height - 62);
            float totalWidth = mode == LayoutMode.Full104 ? 23.0f : 18.4f;
            float totalHeight = 6.60f;
            float scale = Math.Min(board.Width / totalWidth, board.Height / totalHeight);
            float originX = board.Left + (board.Width - totalWidth * scale) / 2f;
            float originY = board.Top + (board.Height - totalHeight * scale) / 2f;

            foreach (KeySpec spec in specs)
            {
                RectangleF rect = new RectangleF(
                    originX + spec.X * scale,
                    originY + spec.Y * scale,
                    spec.Width * scale,
                    spec.Height * scale);
                if (rect.Contains(point))
                {
                    return spec;
                }
            }
            return null;
        }

    }

    internal sealed class SidebarControl : Control
    {
        private KeyboardSession session;

        public SidebarControl()
        {
            DoubleBuffered = true;
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            BackColor = Theme.Background;
        }

        public void SetSession(KeyboardSession value)
        {
            session = value;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            g.Clear(Theme.Background);
            if (session == null)
            {
                return;
            }

            int cardWidth = Width - 2;
            Rectangle currentCard = new Rectangle(0, 0, cardWidth, 104);
            Rectangle progressCard = new Rectangle(0, 116, cardWidth, 112);
            Rectangle warningCard = new Rectangle(0, 240, cardWidth, 128);
            Rectangle eventsCard = new Rectangle(0, 380, cardWidth, Math.Max(96, Height - 380));

            DrawCard(g, currentCard);
            DrawCard(g, progressCard);
            DrawCard(g, warningCard);
            DrawCard(g, eventsCard);
            DrawCurrentKey(g, currentCard);
            DrawProgress(g, progressCard);
            DrawWarnings(g, warningCard);
            DrawEvents(g, eventsCard);
        }

        private void DrawCurrentKey(Graphics g, Rectangle card)
        {
            using (Font labelFont = Theme.Font(8.5f, FontStyle.Regular))
            using (Font keyFont = Theme.Font(21.0f, FontStyle.Bold))
            using (Font detailFont = Theme.Font(8.4f, FontStyle.Regular))
            {
                TextRenderer.DrawText(
                    g,
                    "最后一次输入",
                    labelFont,
                    new Rectangle(card.Left + 16, card.Top + 12, card.Width - 32, 20),
                    Theme.Muted,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter);

                string keyText = session.LatestKey == null ? "等待按键" : session.LatestKey.Label;
                Color keyColor = session.LatestKey == null
                    ? Theme.Foreground
                    : session.LatestIsDown ? Theme.Accent : Theme.Success;
                TextRenderer.DrawText(
                    g,
                    keyText,
                    keyFont,
                    new Rectangle(card.Left + 16, card.Top + 34, card.Width - 32, 34),
                    keyColor,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);

                string detail;
                if (session.LatestKey == null)
                {
                    detail = "按下任意键后，这里会显示响应状态";
                }
                else if (session.LatestIsDown)
                {
                    detail = "正在按下 · 等待松开事件";
                }
                else
                {
                    detail = "已收到松开 · 本次按住 " + session.LatestDurationMs + " ms";
                }
                TextRenderer.DrawText(
                    g,
                    detail,
                    detailFont,
                    new Rectangle(card.Left + 16, card.Top + 74, card.Width - 32, 20),
                    Theme.Muted,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
            }
        }

        private void DrawProgress(Graphics g, Rectangle card)
        {
            int tested = session.TestedCount;
            int total = session.TotalCount;
            float ratio = total == 0 ? 0 : Math.Min(1f, (float)tested / total);
            Rectangle track = new Rectangle(card.Left + 16, card.Top + 54, card.Width - 32, 7);
            Rectangle fill = new Rectangle(track.Left, track.Top, (int)Math.Round(track.Width * ratio), track.Height);

            using (Font labelFont = Theme.Font(8.5f, FontStyle.Regular))
            using (Font valueFont = Theme.Font(9.5f, FontStyle.Bold))
            using (Font smallFont = Theme.Font(8.0f, FontStyle.Regular))
            using (SolidBrush trackBrush = new SolidBrush(Theme.SurfaceRaised))
            using (SolidBrush fillBrush = new SolidBrush(Theme.Success))
            {
                TextRenderer.DrawText(
                    g,
                    "测试进度",
                    labelFont,
                    new Rectangle(card.Left + 16, card.Top + 12, 120, 26),
                    Theme.Muted,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
                TextRenderer.DrawText(
                    g,
                    tested + " / " + total,
                    valueFont,
                    new Rectangle(card.Left + 16, card.Top + 12, card.Width - 32, 26),
                    Theme.Foreground,
                    TextFormatFlags.Right | TextFormatFlags.VerticalCenter);

                Drawing.FillRoundedRect(g, trackBrush, track, 4);
                if (fill.Width > 0)
                {
                    Drawing.FillRoundedRect(g, fillBrush, fill, 4);
                }

                string status = "已回弹 " + tested + "   按下中 " + session.PressedCount +
                                "   未测试 " + (total - tested);
                TextRenderer.DrawText(
                    g,
                    status,
                    smallFont,
                    new Rectangle(card.Left + 16, card.Top + 72, card.Width - 32, 24),
                    Theme.Muted,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
            }
        }

        private void DrawWarnings(Graphics g, Rectangle card)
        {
            List<string> warnings = session.GetWarnings();
            using (Font labelFont = Theme.Font(8.5f, FontStyle.Regular))
            using (Font textFont = Theme.Font(8.2f, FontStyle.Regular))
            {
                TextRenderer.DrawText(
                    g,
                    "检测提示",
                    labelFont,
                    new Rectangle(card.Left + 16, card.Top + 12, card.Width - 32, 24),
                    Theme.Muted,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter);

                Color textColor = warnings.Count == 0 ? Theme.Success : Theme.Warning;
                string first = warnings.Count == 0 ? "暂未发现快速抖动" : warnings[0];
                string second;
                if (warnings.Count > 1)
                {
                    second = warnings[1];
                }
                else if (warnings.Count == 0)
                {
                    second = "请逐键按下并松开完成检查";
                }
                else
                {
                    second = "橙色边框标出了需要复测的按键";
                }

                TextRenderer.DrawText(
                    g,
                    first,
                    textFont,
                    new Rectangle(card.Left + 16, card.Top + 45, card.Width - 32, 32),
                    textColor,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
                TextRenderer.DrawText(
                    g,
                    second,
                    textFont,
                    new Rectangle(card.Left + 16, card.Top + 82, card.Width - 32, 30),
                    Theme.Muted,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
            }
        }

        private void DrawEvents(Graphics g, Rectangle card)
        {
            using (Font labelFont = Theme.Font(8.5f, FontStyle.Regular))
            using (Font lineFont = Theme.Font(7.9f, FontStyle.Regular))
            {
                TextRenderer.DrawText(
                    g,
                    "最近事件",
                    labelFont,
                    new Rectangle(card.Left + 16, card.Top + 12, card.Width - 32, 24),
                    Theme.Muted,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter);

                int visibleLineCount = Math.Max(1, (card.Height - 55) / 23);
                int count = Math.Min(visibleLineCount, session.RecentEvents.Count);
                int start = session.RecentEvents.Count - count;
                int y = card.Top + 43;
                for (int i = session.RecentEvents.Count - 1; i >= start; i--)
                {
                    EventEntry entry = session.RecentEvents[i];
                    Color color = entry.Kind == EventKind.Warning
                        ? Theme.Warning
                        : entry.Kind == EventKind.Press
                            ? Theme.Accent
                            : entry.Kind == EventKind.Release
                                ? Theme.Success
                                : Theme.Muted;
                    string time = TimeSpan.FromMilliseconds(entry.TimeMs).ToString(@"mm\:ss");
                    string line = time + "  " + entry.Text;
                    TextRenderer.DrawText(
                        g,
                        line,
                        lineFont,
                        new Rectangle(card.Left + 16, y, card.Width - 32, 22),
                        color,
                        TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
                    y += 23;
                }

                if (session.RecentEvents.Count == 0)
                {
                    TextRenderer.DrawText(
                        g,
                        "开始逐键测试后，这里会显示事件记录",
                        lineFont,
                        new Rectangle(card.Left + 16, card.Top + 48, card.Width - 32, 30),
                        Theme.Muted,
                        TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
                }
            }
        }

        private static void DrawCard(Graphics g, Rectangle rect)
        {
            using (GraphicsPath path = Drawing.RoundedRect(new RectangleF(
                       rect.Left, rect.Top, rect.Width - 1, rect.Height - 1), 12))
            using (SolidBrush fill = new SolidBrush(Theme.Surface))
            using (Pen border = new Pen(Theme.Border))
            {
                g.FillPath(fill, path);
                g.DrawPath(border, path);
            }
        }
    }

    internal static class Drawing
    {
        public static GraphicsPath RoundedRect(RectangleF rect, float radius)
        {
            GraphicsPath path = new GraphicsPath();
            float diameter = Math.Max(1, Math.Min(radius * 2, Math.Min(rect.Width, rect.Height)));
            RectangleF arc = new RectangleF(rect.X, rect.Y, diameter, diameter);
            if (rect.Width <= 0 || rect.Height <= 0)
            {
                path.AddRectangle(rect);
                return path;
            }

            path.AddArc(arc, 180, 90);
            arc.X = rect.Right - diameter;
            path.AddArc(arc, 270, 90);
            arc.Y = rect.Bottom - diameter;
            path.AddArc(arc, 0, 90);
            arc.X = rect.Left;
            path.AddArc(arc, 90, 90);
            path.CloseFigure();
            return path;
        }

        public static void FillRoundedRect(Graphics g, Brush brush, Rectangle rect, float radius)
        {
            using (GraphicsPath path = RoundedRect(rect, radius))
            {
                g.FillPath(brush, path);
            }
        }
    }

    internal sealed class LowLevelKeyboardEventArgs : EventArgs
    {
        public uint VirtualKey;
        public bool Extended;
        public bool IsDown;
        public bool Injected;
        public uint ScanCode;

        public LowLevelKeyboardEventArgs(uint virtualKey, bool extended, bool isDown, bool injected, uint scanCode)
        {
            VirtualKey = virtualKey;
            Extended = extended;
            IsDown = isDown;
            Injected = injected;
            ScanCode = scanCode;
        }
    }

    internal sealed class KeyboardHook : IDisposable
    {
        private const int WhKeyboardLl = 13;
        private const int WmKeyDown = 0x0100;
        private const int WmKeyUp = 0x0101;
        private const int WmSysKeyDown = 0x0104;
        private const int WmSysKeyUp = 0x0105;
        private const uint LlkhfInjected = 0x10;
        private const uint LlkhfExtended = 0x01;
        private readonly LowLevelKeyboardProc callback;
        private readonly IntPtr targetWindow;
        private IntPtr hookHandle;

        public event EventHandler<LowLevelKeyboardEventArgs> KeyEvent;

        public KeyboardHook(IntPtr windowHandle)
        {
            targetWindow = windowHandle;
            callback = HookCallback;
            IntPtr moduleHandle = GetModuleHandle(null);
            hookHandle = SetWindowsHookEx(WhKeyboardLl, callback, moduleHandle, 0);
            if (hookHandle == IntPtr.Zero)
            {
                throw new InvalidOperationException(
                    "无法安装系统键盘钩子。请关闭安全软件后重试，或尝试以管理员身份运行。");
            }
        }

        public void Dispose()
        {
            if (hookHandle != IntPtr.Zero)
            {
                UnhookWindowsHookEx(hookHandle);
                hookHandle = IntPtr.Zero;
            }
        }

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                int message = wParam.ToInt32();
                bool isDown = message == WmKeyDown || message == WmSysKeyDown;
                bool isUp = message == WmKeyUp || message == WmSysKeyUp;
                if (isDown || isUp)
                {
                    KbdLlHookStruct data = (KbdLlHookStruct)Marshal.PtrToStructure(
                        lParam, typeof(KbdLlHookStruct));
                    uint virtualKey = data.VirtualKey;
                    bool extended = (data.Flags & LlkhfExtended) != 0;
                    bool injected = (data.Flags & LlkhfInjected) != 0;
                    bool isTarget = GetForegroundWindow() == targetWindow;

                    if (isTarget)
                    {
                        EventHandler<LowLevelKeyboardEventArgs> handler = KeyEvent;
                        if (handler != null)
                        {
                            handler(
                                this,
                                new LowLevelKeyboardEventArgs(
                                    virtualKey, extended, isDown, injected, data.ScanCode));
                        }

                        // The tester handles the physical event itself. Swallowing it here
                        // prevents arrows from changing the focused layout picker and
                        // prevents Enter/Space from activating focused buttons.
                        return new IntPtr(1);
                    }
                }
            }

            return CallNextHookEx(hookHandle, nCode, wParam, lParam);
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct KbdLlHookStruct
        {
            public uint VirtualKey;
            public uint ScanCode;
            public uint Flags;
            public uint Time;
            public UIntPtr ExtraInfo;
        }

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(
            int idHook,
            LowLevelKeyboardProc callback,
            IntPtr moduleHandle,
            uint threadId);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool UnhookWindowsHookEx(IntPtr hookHandle);

        [DllImport("user32.dll")]
        private static extern IntPtr CallNextHookEx(
            IntPtr hookHandle,
            int nCode,
            IntPtr wParam,
            IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string moduleName);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();
    }

    internal sealed class MainForm : Form
    {
        private readonly Label titleLabel;
        private readonly Label subtitleLabel;
        private readonly Label statusLabel;
        private readonly Label layoutLabel;
        private readonly ComboBox layoutCombo;
        private readonly Label hintLabel;
        private readonly Button resetButton;
        private readonly Button exportButton;
        private readonly KeyboardControl keyboardControl;
        private readonly SidebarControl sidebar;
        private readonly Timer timer;
        private KeyboardSession session;
        private KeyboardHook hook;
        private LayoutMode mode;
        private bool windowActive;

        public MainForm()
        {
            mode = LayoutMode.Full104;
            session = new KeyboardSession(mode);
            DoubleBuffered = true;
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            BackColor = Theme.Background;
            ForeColor = Theme.Foreground;
            Font = Theme.Font(9.0f, FontStyle.Regular);
            Text = "键盘检测器";
            ClientSize = new Size(1220, 800);
            MinimumSize = new Size(1060, 720);
            StartPosition = FormStartPosition.CenterScreen;
            KeyPreview = true;

            titleLabel = new Label();
            titleLabel.Text = "键盘检测器";
            titleLabel.ForeColor = Theme.Foreground;
            titleLabel.BackColor = Color.Transparent;
            titleLabel.Font = Theme.Font(22.0f, FontStyle.Bold);
            titleLabel.AutoSize = true;
            titleLabel.Location = new Point(44, 25);
            Controls.Add(titleLabel);

            subtitleLabel = new Label();
            subtitleLabel.Text = "逐键检查输入与回弹。点击此窗口后，按下每个键，再松开。";
            subtitleLabel.ForeColor = Theme.Muted;
            subtitleLabel.BackColor = Color.Transparent;
            subtitleLabel.Font = Theme.Font(9.2f, FontStyle.Regular);
            subtitleLabel.AutoSize = true;
            subtitleLabel.Location = new Point(46, 67);
            Controls.Add(subtitleLabel);

            statusLabel = new Label();
            statusLabel.Text = "●  检测窗口未激活";
            statusLabel.ForeColor = Theme.Warning;
            statusLabel.BackColor = Color.Transparent;
            statusLabel.Font = Theme.Font(8.5f, FontStyle.Regular);
            statusLabel.AutoSize = true;
            statusLabel.Location = new Point(1030, 29);
            Controls.Add(statusLabel);

            layoutLabel = new Label();
            layoutLabel.Text = "键盘布局";
            layoutLabel.ForeColor = Theme.Muted;
            layoutLabel.BackColor = Color.Transparent;
            layoutLabel.Font = Theme.Font(8.3f, FontStyle.Regular);
            layoutLabel.AutoSize = true;
            layoutLabel.Location = new Point(948, 58);
            Controls.Add(layoutLabel);

            layoutCombo = new ComboBox();
            layoutCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            layoutCombo.FlatStyle = FlatStyle.Flat;
            layoutCombo.BackColor = Theme.SurfaceRaised;
            layoutCombo.ForeColor = Theme.Foreground;
            layoutCombo.Font = Theme.Font(8.8f, FontStyle.Regular);
            layoutCombo.Items.Add("104 键全尺寸");
            layoutCombo.Items.Add("87 键（无数字区）");
            layoutCombo.SelectedIndex = 0;
            layoutCombo.Size = new Size(160, 28);
            layoutCombo.Location = new Point(1010, 54);
            layoutCombo.SelectedIndexChanged += LayoutCombo_SelectedIndexChanged;
            Controls.Add(layoutCombo);

            keyboardControl = new KeyboardControl();
            Controls.Add(keyboardControl);

            sidebar = new SidebarControl();
            sidebar.SetSession(session);
            Controls.Add(sidebar);

            hintLabel = new Label();
            hintLabel.Text = "每次按下并松开一个键；绿色圆点表示已完成回弹检测。";
            hintLabel.ForeColor = Theme.Muted;
            hintLabel.BackColor = Color.Transparent;
            hintLabel.Font = Theme.Font(8.5f, FontStyle.Regular);
            hintLabel.AutoSize = false;
            hintLabel.TextAlign = ContentAlignment.MiddleLeft;
            Controls.Add(hintLabel);

            resetButton = CreateButton("重新检测", false);
            resetButton.Click += ResetButton_Click;
            Controls.Add(resetButton);

            exportButton = CreateButton("导出报告", true);
            exportButton.Click += ExportButton_Click;
            Controls.Add(exportButton);

            timer = new Timer();
            timer.Interval = 120;
            timer.Tick += Timer_Tick;
            timer.Start();

            Load += MainForm_Load;
            Shown += delegate
            {
                UpdateLayout();
                UpdateActivity(true);
                keyboardControl.Focus();
            };
            Activated += delegate { UpdateActivity(true); };
            Deactivate += delegate
            {
                UpdateActivity(false);
                session.ResetPressedForFocusLoss();
                RefreshInterface();
            };
            Resize += delegate { UpdateLayout(); };
            FormClosing += delegate { DisposeHook(); };
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using (LinearGradientBrush gradient = new LinearGradientBrush(
                       ClientRectangle,
                       Color.FromArgb(14, 18, 28),
                       Theme.Background,
                       LinearGradientMode.Vertical))
            {
                g.FillRectangle(gradient, ClientRectangle);
            }

            using (SolidBrush glow = new SolidBrush(Color.FromArgb(16, 77, 208, 225)))
            {
                g.FillEllipse(glow, ClientSize.Width - 300, -190, 460, 460);
            }
            using (SolidBrush brand = new SolidBrush(Theme.Accent))
            {
                Drawing.FillRoundedRect(g, brand, new Rectangle(24, 25, 5, 54), 2.5f);
            }
            using (Pen line = new Pen(Color.FromArgb(35, Theme.Border)))
            {
                g.DrawLine(line, 24, 104, ClientSize.Width - 24, 104);
                g.DrawLine(line, 24, ClientSize.Height - 78, ClientSize.Width - 24, ClientSize.Height - 78);
            }

            using (Font noteFont = Theme.Font(8.0f, FontStyle.Regular))
            {
                TextRenderer.DrawText(
                    g,
                    "检测期间按键只用于检测，不会操作界面或触发系统快捷键；界面操作请用鼠标，关闭请点右上角 ×。",
                    noteFont,
                    new Rectangle(26, ClientSize.Height - 32, ClientSize.Width - 330, 20),
                    Color.FromArgb(115, Theme.Muted),
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
            }
        }

        private static Button CreateButton(string text, bool primary)
        {
            Button button = new Button();
            button.Text = text;
            button.Font = Theme.Font(9.2f, FontStyle.Bold);
            button.FlatStyle = FlatStyle.Flat;
            button.UseVisualStyleBackColor = false;
            button.BackColor = primary ? Theme.Accent : Theme.SurfaceRaised;
            button.ForeColor = primary ? Color.FromArgb(7, 20, 27) : Theme.Foreground;
            button.FlatAppearance.BorderSize = primary ? 0 : 1;
            button.FlatAppearance.BorderColor = Theme.Border;
            button.FlatAppearance.MouseOverBackColor = primary
                ? Color.FromArgb(102, 220, 235)
                : Color.FromArgb(33, 41, 56);
            button.Size = new Size(116, 38);
            button.Cursor = Cursors.Hand;
            return button;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            UpdateActivity(ContainsFocus);
            try
            {
                hook = new KeyboardHook(Handle);
                hook.KeyEvent += Hook_KeyEvent;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    this,
                    ex.Message + "\r\n\r\n程序仍可显示，但无法可靠截获全部键盘按键。",
                    "无法启动键盘检测",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
            UpdateLayout();
        }

        private void Hook_KeyEvent(object sender, LowLevelKeyboardEventArgs e)
        {
            session.Process(e.VirtualKey, e.Extended, e.ScanCode, e.IsDown, e.Injected);
            RefreshInterface();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            session.CheckLongHolds();
            RefreshInterface();
        }

        private void LayoutCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            LayoutMode next = layoutCombo.SelectedIndex == 1
                ? LayoutMode.Tenkeyless87
                : LayoutMode.Full104;
            if (next == mode)
            {
                return;
            }

            mode = next;
            session = new KeyboardSession(mode);
            sidebar.SetSession(session);
            keyboardControl.Mode = mode;
            RefreshInterface();
        }

        private void ResetButton_Click(object sender, EventArgs e)
        {
            session = new KeyboardSession(mode);
            sidebar.SetSession(session);
            keyboardControl.Mode = mode;
            RefreshInterface();
            keyboardControl.Focus();
        }

        private void ExportButton_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.Title = "导出键盘检测报告";
                dialog.Filter = "文本报告 (*.txt)|*.txt";
                dialog.FileName = "键盘检测报告_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".txt";
                dialog.AddExtension = true;
                dialog.DefaultExt = "txt";
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                try
                {
                    File.WriteAllText(dialog.FileName, session.BuildReport(), new UTF8Encoding(true));
                    MessageBox.Show(
                        this,
                        "报告已保存：\r\n" + dialog.FileName,
                        "导出完成",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        this,
                        "保存报告失败：\r\n" + ex.Message,
                        "导出失败",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
        }

        private void RefreshInterface()
        {
            HashSet<int> pressed = session.GetPressedIds();
            HashSet<int> tested = session.GetTestedIds();
            HashSet<int> warnings = session.GetWarningIds();
            keyboardControl.SetState(pressed, tested, warnings);
            sidebar.Invalidate();
            UpdateStatusText();
        }

        private void UpdateStatusText()
        {
            if (windowActive)
            {
                statusLabel.Text = "●  检测窗口已激活";
                statusLabel.ForeColor = Theme.Success;
            }
            else
            {
                statusLabel.Text = "●  点击窗口以继续";
                statusLabel.ForeColor = Theme.Warning;
            }
        }

        private void UpdateActivity(bool active)
        {
            windowActive = active;
            UpdateStatusText();
        }

        private void UpdateLayout()
        {
            const int margin = 24;
            const int headerBottom = 116;
            const int footerHeight = 78;
            const int sidebarWidth = 300;
            const int gap = 18;
            int contentTop = headerBottom;
            int contentHeight = Math.Max(300, ClientSize.Height - headerBottom - footerHeight);
            int sidebarX = ClientSize.Width - margin - sidebarWidth;
            int keyboardWidth = Math.Max(650, sidebarX - margin - gap);

            keyboardControl.SetBounds(margin, contentTop, keyboardWidth, contentHeight);
            sidebar.SetBounds(sidebarX, contentTop, sidebarWidth, contentHeight);

            hintLabel.SetBounds(margin, ClientSize.Height - 61, Math.Max(300, ClientSize.Width - 500), 34);
            resetButton.Location = new Point(ClientSize.Width - margin - 116, ClientSize.Height - 61);
            exportButton.Location = new Point(resetButton.Left - 12 - 116, ClientSize.Height - 61);

            statusLabel.Location = new Point(ClientSize.Width - margin - statusLabel.Width, 28);
            layoutCombo.Location = new Point(ClientSize.Width - margin - layoutCombo.Width, 51);
            layoutLabel.Location = new Point(layoutCombo.Left - layoutLabel.Width - 10, 57);
            Invalidate();
        }

        private void DisposeHook()
        {
            if (hook != null)
            {
                hook.Dispose();
                hook = null;
            }
        }
    }

    internal static class SelfTests
    {
        public static int Run()
        {
            List<string> lines = new List<string>();
            int failures = 0;

            try
            {
                IList<KeySpec> all = KeyboardModel.All;
                Check(all.Count == 104, "完整布局应为 104 键，实际 " + all.Count, lines, ref failures);
                Check(
                    all.Select(delegate(KeySpec spec) { return spec.Id; }).Distinct().Count() == all.Count,
                    "键位 ID 不应重复",
                    lines,
                    ref failures);
                Check(!string.IsNullOrEmpty(KeyboardModel.Resolve(0x41, false).Label), "A 键映射", lines, ref failures);
                Check(
                    KeyboardModel.Resolve(0x0D, false).Label == "Enter" &&
                    KeyboardModel.Resolve(0x0D, true).Label == "Enter" &&
                    KeyboardModel.Resolve(0x0D, false).Id != KeyboardModel.Resolve(0x0D, true).Id,
                    "主 Enter 与数字区 Enter 应区分",
                    lines,
                    ref failures);
                Check(
                    KeyboardModel.Resolve(0x26, false, 0x48).Id == 0x68,
                    "NumLock 关闭时数字区 8 应映射为数字区上键",
                    lines,
                    ref failures);
                Check(
                    KeyboardModel.Resolve(0x26, true, 0x48).Id == 0x26,
                    "方向键上不应被误映射为数字区 8",
                    lines,
                    ref failures);
                Check(KeyboardModel.GetVisible(LayoutMode.Tenkeyless87).Count == 87, "87 键布局数量", lines, ref failures);

                KeyboardSession session = new KeyboardSession(LayoutMode.Full104);
                session.ProcessAt(0x41, false, true, false, 100);
                session.ProcessAt(0x41, false, true, false, 140);
                session.ProcessAt(0x41, false, false, false, 180);
                KeyResult a = session.GetResult(KeyboardModel.GetById(0x41));
                Check(a.DownCount == 1 && a.UpCount == 1 && a.RepeatCount == 1, "按下、重复和松开计数", lines, ref failures);
                Check(a.Tested && session.TestedCount == 1, "已测试键统计", lines, ref failures);
                Check(a.LastHoldMs == 80, "按住时长计算", lines, ref failures);

                session.ProcessAt(0x42, false, true, false, 200);
                session.ProcessAt(0x42, false, false, false, 210);
                session.ProcessAt(0x42, false, true, false, 218);
                session.ProcessAt(0x42, false, false, false, 228);
                KeyResult b = session.GetResult(KeyboardModel.GetById(0x42));
                Check(b.BounceCount == 1 && b.HasBounceWarning, "快速连续抖动提示", lines, ref failures);

                KeyboardSession numpadSession = new KeyboardSession(LayoutMode.Full104);
                numpadSession.ProcessAt(0x26, false, 0x48, true, false, 100);
                numpadSession.ProcessAt(0x26, false, 0x48, false, false, 160);
                KeyResult numpadUp = numpadSession.GetResult(KeyboardModel.GetById(0x68));
                KeyResult navigationUp = numpadSession.GetResult(KeyboardModel.GetById(0x26));
                Check(numpadUp != null && navigationUp != null &&
                      numpadUp.Tested && !navigationUp.Tested,
                    "数字区方向输入应计到物理数字键",
                    lines,
                    ref failures);

                session.ProcessAt(0x43, false, true, true, 300);
                Check(session.IgnoredInjectedEvents == 1, "模拟输入应忽略", lines, ref failures);
                string report = session.BuildReport();
                Check(report.Contains("键盘检测报告") && report.Contains("已测试：2 / 104"), "报告生成", lines, ref failures);

                KeyboardSession shortSession = new KeyboardSession(LayoutMode.Full104);
                shortSession.ProcessAt(0x44, false, true, false, 1000);
                shortSession.ProcessAt(0x44, false, false, false, 6000);
                Check(shortSession.GetResult(KeyboardModel.GetById(0x44)).LastHoldMs == 5000, "长按时长记录", lines, ref failures);
            }
            catch (Exception ex)
            {
                failures++;
                lines.Add("EXCEPTION " + ex);
            }

            lines.Insert(0, failures == 0 ? "SELF-TEST: PASS" : "SELF-TEST: FAIL (" + failures + ")");
            string path = Path.Combine(Environment.CurrentDirectory, "self-test-result.txt");
            File.WriteAllLines(path, lines.ToArray(), new UTF8Encoding(true));
            return failures == 0 ? 0 : 1;
        }

        private static void Check(bool condition, string name, List<string> lines, ref int failures)
        {
            if (condition)
            {
                lines.Add("PASS  " + name);
            }
            else
            {
                failures++;
                lines.Add("FAIL  " + name);
            }
        }
    }
}
