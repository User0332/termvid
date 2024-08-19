using System.Diagnostics;
using System.Drawing;
using Mindmagma.Curses;

static class CursesPlayer
{
	public static void Play(string[,] frames, int nFrames, Size frameSize, int fps)
	{
		var stdscr = NCurses.InitScreen();
		Stopwatch stopwatch = new();

		int msPerFrame = 1000/fps;

		for (int i = 0; i < nFrames; i++)
		{
			stopwatch.Restart();
			
			for (int j = 0; j < frameSize.Height; j++)
			{
				while (frames[i, j] is null);
				
				NCurses.MoveAddString(j, 0, frames[i, j]);
			}

			NCurses.Refresh();

			// keep framerate

			while (stopwatch.Elapsed.Milliseconds < msPerFrame);
		}

		NCurses.EndWin();
	}
}