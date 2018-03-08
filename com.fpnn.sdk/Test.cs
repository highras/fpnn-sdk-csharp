using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Fpnn.Connection;
using Fpnn.Protocol;

namespace com.fpnn.sdk
{
	public class Test
	{
		int asyncNum;
		int syncNum;
		TCPClient client;
		uint seqNum = 0;
		object seqLock = new object();
		ArrayList tList = new ArrayList();

		public Test(string endpoint, int asyncN, int syncN)
		{
			this.asyncNum = asyncN;
			this.syncNum = syncN;
			this.client = new TCPClient(endpoint);
			this.client.enableEncryptor(@"-----BEGIN PUBLIC KEY-----
MFYwEAYHKoZIzj0CAQYFK4EEAAoDQgAEfzlRbwpNYwLCw6o4hhLhYHNulTPl092EDSX0pY2ZxX8u
A7URIMfp90zdcsxUiBvnplgEkTf8GApF7u+P1z8CKA==
-----END PUBLIC KEY-----");
			client.setQuestTimeout(5);

			client.setConnectionConnectedCallback(delegate ()
			{
				Console.WriteLine("Connected");
			});

			client.setConnectionWillCloseCallback(delegate (bool causedByError)
			{
				Console.WriteLine("Closed: " + causedByError);
			});

			client.connect();
		}

		public uint getSeq()
		{
			lock (this.seqLock)
			{
				this.seqNum += 1;
				return this.seqNum;
			}
		}

		public void asyncTest()
		{
			while (true)
			{
				uint seqNum = this.getSeq();
				FPQWriter qw = new FPQWriter(1, "test");
				qw.param("seqNum", seqNum);
				FPQuest quest = qw.take();

				this.client.sendQuest(quest, delegate (FPAReader reader)
				{
					if (reader.isError())
					{
						Console.WriteLine("seqNum: " + seqNum);
						Console.WriteLine(reader.errorCode());
						Console.WriteLine(reader.errorException());
						Console.WriteLine(reader.errorMessage());
						Console.WriteLine(reader.errorRaiser());
					}
					else {
						try
						{
							int readSeqNum = reader.want<int>("seqNum");
							if (seqNum != readSeqNum)
							{
								Console.WriteLine("seqnum wrong");
							}
						}
						catch (Exception ex)
						{
							Console.WriteLine("got ex: " + ex);
						}
					}
				});


				Random rd = new Random();
				System.Threading.Thread.Sleep(rd.Next(1, 500));
			}
		}

		public void syncTest()
		{
			while (true)
			{
				uint seqNum = this.getSeq();
				FPQWriter qw = new FPQWriter(1, "test");
				qw.param("seqNum", seqNum);
				FPQuest quest = qw.take();

				FPAReader reader = client.sendQuestSync(quest);

				if (reader.isError())
				{
					Console.WriteLine("sync seqNum: " + seqNum);
					Console.WriteLine(reader.errorCode());
					Console.WriteLine(reader.errorException());
					Console.WriteLine(reader.errorMessage());
					Console.WriteLine(reader.errorRaiser());
				}
				else {
					try
					{
						int readSeqNum = reader.want<int>("seqNum");
						if (seqNum != readSeqNum)
						{
							Console.WriteLine("seqnum wrong");
						}
					}
					catch (Exception ex)
					{
						Console.WriteLine("got ex: " + ex);
					}
				}

				Random rd = new Random();
				System.Threading.Thread.Sleep(rd.Next(1, 500));
			}
		}

		public void start()
		{
			for (int i = 0; i < this.asyncNum; i++)
			{
				Thread t = new Thread(new ThreadStart(asyncTest));
				t.IsBackground = true;
				t.Start();
				this.tList.Add(t);
			}

			for (int i = 0; i < this.syncNum; i++)
			{
				Thread t = new Thread(new ThreadStart(syncTest));
				t.IsBackground = true;
				t.Start();
				this.tList.Add(t);
			}

			while (true)
				System.Threading.Thread.Sleep(1000);
		}
	}
}

