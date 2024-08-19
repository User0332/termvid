from itertools import count
import math
from threading import Thread
import time
from .counter import Counter
import cv2
import curses

ASCII_BRIGHTNESS = " `.-':_,^=;><+!rc*/z?sLTv)J7(|Fi{C}fI31tlu[neoZ5Yxjya]2ESwqkP6h9d4VpOGbUAKXHm8RD#$Bg0MNWQ%&@"
ASCII_BRIGHTNESS_LEN = len(ASCII_BRIGHTNESS)

debug = open("debug", 'w')

def render(stdscr: 'curses._CursesWindow', filename: str, line_multiplier: float=1.2) -> None:
	capture = cv2.VideoCapture(filename)

	if not capture.isOpened():
		raise RuntimeError("Could not open video!")
	
	dimensions: tuple[int, int] = capture.get(cv2.CAP_PROP_FRAME_WIDTH), capture.get(cv2.CAP_PROP_FRAME_HEIGHT)
	original_ratio = round(dimensions[0] / dimensions[1], 2)

	fps: int = capture.get(cv2.CAP_PROP_FPS)

	stdscr_size_yx = stdscr.getmaxyx()

	scale = min(stdscr_size_yx[1]/dimensions[0], stdscr_size_yx[0]/dimensions[1])

	print(scale, file=debug, flush=True)

	scaled_dimensions = (int(dimensions[0]*scale), int(dimensions[1]*scale))

	while 1: # todo: optimize
		new_ratio = round(scaled_dimensions[0]/scaled_dimensions[1], 2)

		if new_ratio > original_ratio:
			scaled_dimensions = (scaled_dimensions[0]-1, scaled_dimensions[1])
			continue
		if new_ratio < original_ratio:
			scaled_dimensions = (scaled_dimensions[0], scaled_dimensions[1]-1)
			continue

		break

	scaled_dimensions = (math.ceil(scaled_dimensions[0]*line_multiplier), scaled_dimensions[1])

	print(scaled_dimensions, file=debug)

	frame_strings: list[list[str]] = []

	counter = Counter()

	num_threads = Counter()

	while 1:
		got, frame = capture.read()

		if not got: break

		frame = cv2.resize(frame, scaled_dimensions, interpolation=cv2.INTER_AREA)

		# print(len(frame), len(frame[0]), file=debug)

		frame_lines: list[str] = []

		def worker():
			for y in range(scaled_dimensions[1]):
				line_string = ""

				for x in range(scaled_dimensions[0]):
					pixel = frame[y, x]

					# calculate BGR luminance
					lum: float = (0.299*pixel[2] + 0.587*pixel[1] + 0.114*pixel[0])

					index = round(((ASCII_BRIGHTNESS_LEN-1)*lum)/255)

					char = ASCII_BRIGHTNESS[index]

					line_string+=char

					# print(line_string, len(line_string), file=debug)

				frame_lines.append(line_string)

			while counter.count != len(frame_lines): frame_strings.append(frame_lines)

			counter.increment()
			num_threads.decrement()

		while num_threads.count > 4: pass

		Thread(target=worker).start()
		num_threads.increment()

	while num_threads.count != 0: pass

	sec_per_frame = 1/fps

	for frame_lines in frame_strings:
		start_time = time.time()
		for i, frame_line in enumerate(frame_lines):
			stdscr.addstr(i, 0, frame_line)

		stdscr.refresh()

		# tick fps
		diff = time.time()-start_time

		if diff > sec_per_frame: continue

		to_sleep = sec_per_frame-diff

		time.sleep(to_sleep)

def DEPRECATED_TEST_do_not_run(filename: str, pos: tuple[int, int]=(0, 0), scale: float=1.0) -> None:
	import pygame

	pygame.init()
	
	capture = cv2.VideoCapture(filename)

	if not capture.isOpened():
		raise RuntimeError("Could not open video!")
	
	dimensions: tuple[int, int] = capture.get(cv2.CAP_PROP_FRAME_WIDTH), capture.get(cv2.CAP_PROP_FRAME_HEIGHT)

	fps: int = capture.get(cv2.CAP_PROP_FPS)

	display = pygame.display.set_mode((int(dimensions[0]*scale), int(dimensions[1]*scale)))
	pygame.display.set_caption("TermVid Graphical")

	clock = pygame.time.Clock()

	while 1:
		got, frame = capture.read()

		frame = cv2.resize(frame, (int(dimensions[0]*scale), int(dimensions[1]*scale)), interpolation=cv2.INTER_AREA)

		if got:
			for i in range(int(dimensions[0]*scale)):
				for j in range(int(dimensions[1]*scale)):
					(b, g, r) = frame[j, i]


					display.set_at((i, j), (r, g, b))
		else: break

		pygame.display.update()
		clock.tick(fps)

	pygame.quit()