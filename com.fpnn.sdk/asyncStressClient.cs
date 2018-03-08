using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Fpnn.Connection;
using Fpnn.Protocol;

namespace com.fpnn.sdk
{
	public class asyncStressClient
	{

		string _ip;
		int _port;
		int _thread_num;
		int _qps;
		object locker = new object();
		ArrayList _threads = new ArrayList();
		Int64 _send;
		Int64 _recv;
		Int64 _sendError;
		Int64 _recvError;
		Int64 _timecost;

		public FPQuest buildQuest()
		{
			FPQWriter qw = new FPQWriter(4, "test");
			qw.param("quest", "one");
			qw.param("int", 2);
			qw.param("double", 3.3);
			qw.param("boolean", true);
			return qw.take();
		}

		public asyncStressClient(string ip, int port, int thread_num, int qps)
		{
			_ip = ip;
			_port = port;
			_thread_num = thread_num;
			_qps = qps;
			_send = 0;
			_recv = 0;
			_sendError = 0;
			_recvError = 0;
			_timecost = 0;
		}

		~asyncStressClient()
		{
			this.stop();
		}


		void incSend()
		{
			lock (locker)
			{
				_send++;
			}
		}
		void incRecv() { 
			lock (locker)
			{ 
				_recv++; 
			}
		}
		void incSendError()
		{
			lock (locker)
			{
				_sendError++;
			}
		}
		void incRecvError() { 
			lock (locker)
			{
				_recvError++;
			}
		}
		void addTimecost(Int64 cost) 
		{ 
			lock (locker)
			{
				_timecost += cost;
			}
		}

		public void launch()
		{
			int pqps = _qps / _thread_num;
			if (pqps == 0)
				pqps = 1;
			int remain = _qps - pqps * _thread_num;

			for (int i = 0; i < _thread_num; i++)
			{
				Thread t = new Thread(test_worker);
				t.IsBackground = true;
				t.Start(pqps);
				this._threads.Add(t);
			}


			if (remain > 0)
			{
				Thread t = new Thread(test_worker);
				t.IsBackground = true;
				t.Start(remain);
				this._threads.Add(t);
			}
		}

		void stop()
		{
			for (int i = 0; i < _threads.Count; i++)
			{
				Thread t = (Thread)_threads[i];
				t.Join();
			}
		}

		public void showStatistics()
		{
			int sleepSeconds = 3000;

			Int64 send = _send;
			Int64 recv = _recv;
			Int64 sendError = _sendError;
			Int64 recvError = _recvError;
			Int64 timecost = _timecost;


			while (true)
			{
				Int64 start = DateTime.Now.Ticks;

				System.Threading.Thread.Sleep(sleepSeconds);

				Int64 s = _send;
				Int64 r = _recv;
				Int64 se = _sendError;
				Int64 re = _recvError;
				Int64 tc = _timecost;

				Int64 ent = DateTime.Now.Ticks;

				Int64 ds = s - send;
				Int64 dr = r - recv;
				Int64 dse = se - sendError;
				Int64 dre = re - recvError;
				Int64 dtc = tc - timecost;

				send = s;
				recv = r;
				sendError = se;
				recvError = re;
				timecost = tc;

				Int64 real_time = ent - start;

				ds = ds * 1000 * 1000 / real_time;
				dr = dr * 1000 * 1000 / real_time;
				//dse = dse * 1000 * 1000 / real_time;
				//dre = dre * 1000 * 1000 / real_time;
				if (dr > 0)
					dtc = dtc / dr;


				Console.WriteLine("time interval: " + (real_time / 10000.0) + " ms, send error: " + dse + ", recv error: " + dre);
				Console.WriteLine("[QPS] send: " + ds + ", recv: " + dr + ", per quest time cost: " + dtc + " usec");
			}

		}


		public void test_worker(object param)
		{
			int qps = (int)param;
			int usec = 1000 * 1000 / qps;

			Console.WriteLine("-- qps: " + qps + ", usec: " + usec);

			TCPClient client = new TCPClient(_ip, _port);
			client.connect();
			while (true)
			{
				FPQuest quest = buildQuest();
				Int64 send_time = DateTime.Now.Ticks;

				try
				{
					client.sendQuest(quest, delegate (FPAReader reader)
					{
						if (reader.isError())
						{
							incRecvError();
						}
						else {
							incRecv();

							Int64 recv_time = DateTime.Now.Ticks;
							Int64 diff = recv_time - send_time;
							addTimecost(diff);
						}
					});
					incSend();
				}
				catch (Exception e)
				{
					incSendError();
				}
				Int64 sent_time = DateTime.Now.Ticks;
				Int64 real_usec = (usec - (sent_time - send_time)) / 10000;
				if (real_usec > 0)
				{
					System.Threading.Thread.Sleep((int)real_usec);
				}
			}
			client.close();
		}



	}
}

