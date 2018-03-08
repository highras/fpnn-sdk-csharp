using System;
using System.IO;
using System.Collections.Generic;
using Fpnn.Connection;
using Fpnn.Protocol;

namespace com.fpnn.sdk
{
	public class MyProcessor : FpnnClientProcessor
	{
		public void pushmsg(FPQReader reader)
		{
			Console.WriteLine("method: " + reader.getMethod());

			int from = reader.want<int>("from");
			Console.WriteLine("from: " + from);

			string msg = reader.want<string>("msg");
			Console.WriteLine("msg: " + msg);
		}
	}

	class MainClass
	{
		public static void Main(string[] args)
		{
			TCPClient client = new TCPClient("35.167.185.139:13099");

			client.enableEncryptor(@"-----BEGIN PUBLIC KEY-----
MFYwEAYHKoZIzj0CAQYFK4EEAAoDQgAEfzlRbwpNYwLCw6o4hhLhYHNulTPl092EDSX0pY2ZxX8u
A7URIMfp90zdcsxUiBvnplgEkTf8GApF7u+P1z8CKA==
-----END PUBLIC KEY-----");

			client.setConnectionConnectedCallback(delegate ()
			{
				Console.WriteLine("Connected");
			});

			client.setConnectionWillCloseCallback(delegate (bool causedByError)
			{
				Console.WriteLine("Closed: " + causedByError);
			});

			client.setProcessor(new MyProcessor());

			client.setPushQueueFullCallback(delegate ()
			{
				Console.WriteLine("push queue is full");
			});

			client.setQuestTimeout(5);

			FPQWriter qw = new FPQWriter(2, "two");
			qw.param("k", "key");
			qw.param("v", 1);
			FPQuest quest = qw.take();

			client.sendQuest(quest, delegate (FPAReader reader)
			{
				if (reader.isError())
				{
					Console.WriteLine(reader.errorCode());
					Console.WriteLine(reader.errorException());
					Console.WriteLine(reader.errorMessage());
					Console.WriteLine(reader.errorRaiser());
				}
				else {
					try
					{
						string t = reader.want<string>("answer");
						Console.WriteLine(t);
					}
					catch (Exception ex)
					{
						Console.WriteLine("got ex: " + ex);
					}
				}
			});



			FPAReader readerSync = client.sendQuestSync(quest);

			if (readerSync.isError())
			{
				Console.WriteLine(readerSync.errorCode());
				Console.WriteLine(readerSync.errorException());
				Console.WriteLine(readerSync.errorMessage());
				Console.WriteLine(readerSync.errorRaiser());
			}
			else {
				try
				{
					string t = readerSync.want<string>("answer");
					Console.WriteLine(t);
				}
				catch (Exception ex)
				{
					Console.WriteLine("got ex: " + ex);
				}
			}

			System.Threading.Thread.Sleep(2000);
			client.close();

			/*Test test = new Test("35.167.185.139:13099", 30, 30);
			test.start();

			asyncStressClient tester = new asyncStressClient("35.167.185.139", 13099, 20, 80);

			tester.launch();
			tester.showStatistics();


			singleClientConcurrentTest tester = new singleClientConcurrentTest();
			tester.launch(); */
		}
	}
}
