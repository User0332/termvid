import threading

class Counter:
	def __init__(self):
		self.count = 0
		self.lock = threading.Lock()

	def increment(self):
		with self.lock:
			self.count+=1

	def decrement(self):
		with self.lock:
			self.count-=1