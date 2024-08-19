namespace TermVid.Player;

using System.Drawing;
using System.Text;
using Emgu.CV;
using Emgu.CV.Structure;

static class FrameStringGenerator
{
	const string ASCII_BRIGHTNESS = " `.-':_,^=;><+!rc*/z?sLTv)J7(|Fi{C}fI31tlu[neoZ5Yxjya]2ESwqkP6h9d4VpOGbUAKXHm8RD#$Bg0MNWQ%&@";

	public static
		(bool Success, string[,] Result)
		GenerateFrameStrings(
			VideoCapture cap,
			Size size
		)
	{
		var (success, result, tasks) = GenerateFrameStringsNoWait(cap, size);

		if (!success) return (false, null!);

		Task.WhenAll(tasks).Wait();

		return (true, result);
	}
	
	public static
		(bool Success, string[,] Result, List<Task> processingTasks)
		GenerateFrameStringsNoWait(
			VideoCapture cap,
			Size size
		)
	{
		if (!cap.IsOpened)
		{
			Console.Error.WriteLine("Could not open video!");
			return (false, null!, null!);
		}


		var frameCount = (int) cap.Get(Emgu.CV.CvEnum.CapProp.FrameCount);

		List<Task> tasks = new(frameCount);

		var frame_strings = new string[frameCount, cap.Height];

		int frameNum = 0;

		for (int i = 0; i < frameCount; i++)
		{
			Mat frame = new();

			cap.Read(frame);

			// if (!cap.Read(frame)) break;

			int snapshotNum = frameNum;

			tasks.Add(
				Task.Run(() => {
					CvInvoke.Resize(frame, frame, size);

					var img = frame.ToImage<Bgr, byte>(false);

					for (int y = 0; y < size.Height; y++)
					{
						StringBuilder frame_line = new(size.Width);

						for (int x = 0; x < size.Width; x++)
						{
							Bgr pixel = img[y, x];

							double lum = 0.299*pixel.Red + 0.587*pixel.Green + 0.114*pixel.Blue;

							int charIdx = (int) Math.Floor(lum/256*ASCII_BRIGHTNESS.Length);
							
							char c = ASCII_BRIGHTNESS[charIdx];

							frame_line.Append(c);
						}

						frame_strings[snapshotNum, y] = frame_line.ToString();
					}
				})
			);

			frameNum++;

			if (frameNum % 1000 == 0) Console.WriteLine($"[INFO] Launched {frameNum} out of {frameCount} frame processing tasks");
		}

		Console.WriteLine("[INFO] Launched all frame processing tasks");

		return (true, frame_strings, tasks);
	}

	public static
		(bool Success, string[,] Result, List<Task> processingTasks)
		GenerateFrameStringsInstantaneously(
			VideoCapture cap,
			Size size
		)
	{
		if (!cap.IsOpened)
		{
			Console.Error.WriteLine("Could not open video!");
			return (false, null!, null!);
		}


		var frameCount = (int) cap.Get(Emgu.CV.CvEnum.CapProp.FrameCount);

		List<Task> tasks = new(frameCount);

		var frame_strings = new string[frameCount, cap.Height];


		tasks.Add(
			Task.Run(() => {
				int frameNum = 0;

				for (int i = 0; i < frameCount; i++)
				{
					Mat frame = new();

					cap.Read(frame);

					// if (!cap.Read(frame)) break;

					int snapshotNum = frameNum;

					tasks.Add(
						Task.Run(() => {
							CvInvoke.Resize(frame, frame, size);

							var img = frame.ToImage<Bgr, byte>(false);

							for (int y = 0; y < size.Height; y++)
							{
								StringBuilder frame_line = new(size.Width);

								for (int x = 0; x < size.Width; x++)
								{
									Bgr pixel = img[y, x];

									double lum = 0.299*pixel.Red + 0.587*pixel.Green + 0.114*pixel.Blue;

									int charIdx = (int) Math.Floor(lum/256*ASCII_BRIGHTNESS.Length);
									
									char c = ASCII_BRIGHTNESS[charIdx];

									frame_line.Append(c);
								}

								frame_strings[snapshotNum, y] = frame_line.ToString();
							}
						})
					);

					frameNum++;

					if (frameNum % 1000 == 0) Console.WriteLine($"[INFO] Launched {frameNum} out of {frameCount} frame processing tasks");
				}

				Console.WriteLine("[INFO] Launched all frame processing tasks");
			})
		);

		return (true, frame_strings, tasks);
	}
}