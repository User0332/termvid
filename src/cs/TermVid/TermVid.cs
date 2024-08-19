using System.Drawing;
using System.Runtime.InteropServices;
using Emgu.CV;
using TermVid.Player;

namespace TermVid;

class TermVidMain
{
	static int Main(string[] args)
	{
		// TODO: check validity and use CommandLineParser
		string video = args[0];
		// double lineMultiplier = double.Parse(args[1]);

		VideoCapture cap = new(video);

		// Console.WriteLine(ConsoleUtils.OriginalFontSize);

		// ConsoleUtils.SetFontSize((1, 1));

		double scale = Math.Min(((double) Console.WindowHeight)/cap.Height, ((double) Console.WindowWidth)/cap.Width);

		Size scaledSize = new((int) Math.Round(cap.Width*scale), (int) Math.Round(cap.Height*scale));

		Console.WriteLine($"Loading {video}...");

		var (success, frameStrings, _) = FrameStringGenerator.GenerateFrameStringsInstantaneously(
			cap,
			scaledSize
		);

		if (!success)
		{
			Console.Error.WriteLine("Could not process video!");

			return 1;	
		}

		Console.WriteLine("Video Successfully Loaded!");

		// CursesPlayer.Play(
		// 	frameStrings,
		// 	(int) cap.Get(Emgu.CV.CvEnum.CapProp.FrameCount),
		// 	scaledSize,
		// 	(int) cap.Get(Emgu.CV.CvEnum.CapProp.Fps)
		// );

		// ConsoleUtils.SetFontSize(ConsoleUtils.OriginalFontSize);

		return 0;
	}
}