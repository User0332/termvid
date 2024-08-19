using System.Runtime.InteropServices;

namespace TermVid;

static class ConsoleUtils
{
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	struct CONSOLE_FONT_INFOEX
	{
		public uint cbSize;
		public uint nFont;
		public COORD dwFontSize;
		public int FontFamily;
		public int FontWeight;
		
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
		public string FaceName;
	}

	[StructLayout(LayoutKind.Sequential)]
	struct COORD(short x, short y)
	{
		public short X = x;
		public short Y = y;
	}

	[DllImport("kernel32.dll", SetLastError = true)]
	static extern bool SetCurrentConsoleFontEx(
		nint consoleOutput,
		bool maximumWindow,
		ref CONSOLE_FONT_INFOEX consoleCurrentFontEx
	);

	[DllImport("kernel32.dll", SetLastError = true)]
	private static extern bool GetCurrentConsoleFontEx(
		IntPtr consoleOutput,
		bool maximumWindow,
		ref CONSOLE_FONT_INFOEX consoleCurrentFontEx
	);

	[DllImport("kernel32.dll", SetLastError = true)]
	static extern nint GetStdHandle(int nStdHandle);

	const int STD_OUTPUT_HANDLE = -11;

	static nint ConsoleHandle = GetStdHandle(STD_OUTPUT_HANDLE);
		
	public static (short X, short Y) OriginalFontSize = GetFontSize();
	
	public static void SetFontSize((short x, short y) size)
	{
		CONSOLE_FONT_INFOEX fontInfo = new()
		{
			cbSize = (uint) Marshal.SizeOf<CONSOLE_FONT_INFOEX>(),
			dwFontSize = new COORD(0, size.y),
			FaceName = "Cascadia Code",
			FontFamily = 0,
			FontWeight = 0,
			nFont = 0,
		};

		SetCurrentConsoleFontEx(ConsoleHandle, false, ref fontInfo);
		Console.WriteLine("set");
		Console.WriteLine(GetFontSize());
		Console.WriteLine(Marshal.GetLastWin32Error());
		// Console
	}

	public static (short x, short y) GetFontSize()
	{
		CONSOLE_FONT_INFOEX fontInfo = new()
		{
			cbSize = (uint) Marshal.SizeOf<CONSOLE_FONT_INFOEX>()
		};

		if (!GetCurrentConsoleFontEx(ConsoleHandle, false, ref fontInfo))
		{
			Console.Error.WriteLine("Could not retrieve console font info!");
			Environment.Exit(1);
		}

		return (fontInfo.dwFontSize.X, fontInfo.dwFontSize.Y);
	}
}