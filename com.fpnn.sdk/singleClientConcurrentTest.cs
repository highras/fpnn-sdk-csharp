using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Fpnn.Connection;
using Fpnn.Protocol;

namespace com.fpnn.sdk
{
	public class singleClientConcurrentTest
	{

		TCPClient client = null;

		public singleClientConcurrentTest()
		{
		}

		public FPQuest buildQuest()
		{
			FPQWriter qw = new FPQWriter(4, "test");
			qw.param("quest", "one");
			qw.param("int", 2);
			qw.param("double", 3.3);
			qw.param("boolean", true);
			return qw.take();
		}

		void showSignDesc()
		{
			Console.WriteLine("Sign:");
			Console.WriteLine("    +: establish connection");
			Console.WriteLine("    ~: close connection");
			Console.WriteLine("    #: connection error");

			Console.WriteLine("    *: send sync quest");
			Console.WriteLine("    &: send async quest");

			Console.WriteLine("    ^: sync answer Ok");
			Console.WriteLine("    ?: sync answer exception");
			Console.WriteLine("    |: sync answer exception by connection closed");
			Console.WriteLine("    (: sync operation fpnn exception");
			Console.WriteLine("    ): sync operation unknown exception");

			Console.WriteLine("    $: async answer Ok");
			Console.WriteLine("    @: async answer exception");
			Console.WriteLine("    ;: async answer exception by connection closed");
			Console.WriteLine("    {: async operation fpnn exception");
			Console.WriteLine("    }: async operation unknown exception");

			Console.WriteLine("    !: close operation");
			Console.WriteLine("    [: close operation fpnn exception");
			Console.WriteLine("    ]: close operation unknown exception");
		}

		void testThread(object param)
		{
			int count = (int)param;
			int act = 0;
			for (int i = 0; i < count; i++)
			{
				Int64 index = (PackCommon.getMilliTimestamp() + i) % 64;
				if (i >= 10)
				{
					if (index < 6)
						act = 2;    //-- close operation
					else if (index < 32)
						act = 1;    //-- async quest
					else
						act = 0;    //-- sync quest
				}
				else
					act = (int)index & 0x1;
				try
				{
					switch (act)
					{
						case 0:
							Console.Write("*");
							FPAReader readerSync = client.sendQuestSync(buildQuest());
							if (!readerSync.isError())
							{
								Console.Write("^");
							}
							else
								Console.Write("?");

							break;
						case 1:
							Console.Write("&");

							client.sendQuest(buildQuest(), delegate (FPAReader reader)
							{
								if (!reader.isError())
								{
									Console.Write("$");
								}
								else
								{
									Console.Write("@");
								}
							});
							break;
						case 2:
							Console.Write("!");
							client.close();
							break;
					}
				}
				catch (Exception e)
				{
					switch (act)
					{
						case 0: Console.Write(')'); break;
						case 1: Console.Write('}'); break;
						case 2: Console.Write(']'); break;
					}
				}

			}
		}

		void test(TCPClient client, int threadCount, int questCount)
		{
			Console.WriteLine("========[ Test: thread " + threadCount + ", per thread quest: " + questCount + " ]==========");

			ArrayList _threads = new ArrayList();

			for (int i = 0; i < threadCount; i++)
			{
				Thread t = new Thread(testThread);
				t.IsBackground = true;
				t.Start(questCount);
				_threads.Add(t);
			}

			System.Threading.Thread.Sleep(5000);

			for (int i = 0; i < _threads.Count; i++)
			{
				Thread t = (Thread)_threads[i];
				t.Join();
			}
		}


		public void launch()
		{
			client = new TCPClient("35.167.185.139:13099");

			client.setConnectionConnectedCallback(delegate ()
			{
				Console.Write("+");
			});

			client.setConnectionWillCloseCallback(delegate (bool causedByError)
			{
				if(causedByError)
					Console.Write("#");
				else
					Console.Write("~");
			});
			client.connect();

			showSignDesc();

			test(client, 10, 300);

			/*test(client, 10, 30000);
			test(client, 20, 30000);
			test(client, 30, 30000);
			test(client, 40, 30000);
			test(client, 50, 30000);
			test(client, 60, 30000);*/
		}
	}
}

