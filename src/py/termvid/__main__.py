import sys
import curses
from termvid import render


def main():
	if len(sys.argv) < 2:
		print("Need to pass a video as the first argument!", file=sys.stderr)

		exit(1)

	if len(sys.argv) < 3:
		line_multiplier: float = 1.2
	else:
		try: line_multiplier = float(sys.argv[2])
		except ValueError:
			print("Line height multiplier must be a float!", file=sys.stderr)

			exit(1)

	stdscr = curses.initscr()

	render(stdscr, sys.argv[1], line_multiplier=line_multiplier)

if __name__ == "__main__":
	main()