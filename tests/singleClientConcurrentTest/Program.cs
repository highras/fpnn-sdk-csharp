using System;
using System.Collections.Generic;
using System.Threading;
using com.fpnn;
using com.fpnn.proto;

namespace singleClientConcurrentTest
{
    class Program
    {
        class Recorder : com.fpnn.common.ErrorRecorder
        {
            public void RecordError(Exception e)
            {
                lock (this)
                    Console.WriteLine(e.ToString());
            }
            public void RecordError(string message)
            {
                //lock (this)
                //    Console.WriteLine(message);
            }
            public void RecordError(string message, Exception e)
            {
                lock (this)
                    Console.WriteLine(message + "\n" + e.ToString());
            }
        }

        class Tester
        {
			private TCPClient client;
            private int questCount;

            internal Tester(string ip, int port)
            {
                client = new TCPClient(ip, port);
                ProcessEncrypt(client);
                client.SetConnectionConnectedDelegate((Int64 connectionId, string endpoint, bool connected) => {
                    if (connected)
                        Console.Write("+");
                    else
                        Console.Write("-");
                });

                client.SetConnectionCloseDelegate((Int64 connectionId, string endpoint, bool causedByError) => {
                    Console.Write(causedByError ? "#" : "~");
                });
            }

			public void ShowSignDesc()
			{
				Console.WriteLine("Sign:");
				Console.WriteLine("    +: establish connection");
                Console.WriteLine("    -: connect failed");
                Console.WriteLine("    ~: close connection");
				Console.WriteLine("    #: connection error");

				Console.WriteLine("    *: send sync quest");
				Console.WriteLine("    &: send async quest");

				Console.WriteLine("    ^: sync answer Ok");
				Console.WriteLine("    ?: sync answer exception");
				Console.WriteLine("    |: sync answer exception by connection closed");
				//Console.WriteLine("    (: sync operation fpnn exception");
				//Console.WriteLine("    ): sync operation unknown exception");

				Console.WriteLine("    $: async answer Ok");
				Console.WriteLine("    @: async answer exception");
				Console.WriteLine("    ;: async answer exception by connection closed");
				//Console.WriteLine("    {: async operation fpnn exception");
				//Console.WriteLine("    }: async operation unknown exception");

				Console.WriteLine("    !: close operation");
				//Console.WriteLine("    [: close operation fpnn exception");
				//Console.WriteLine("    ]: close operation unknown exception");
			}

            private Quest GenQuest()
            {
                Quest quest = new Quest("two way demo");
                quest.Param("quest", "one");
                quest.Param("int", 2);
                quest.Param("double", 3.3);
                quest.Param("boolean", true);

                List<object> array = new List<object> { "first_vec", 4 };
                quest.Param("ARRAY", array);

                Dictionary<string, object> dict = new Dictionary<string, object>
            {
                {"map1", "first_map" },
                {"map2", true },
                {"map3", 5 },
                {"map4", 5.7 },
                {"map5", "中文" }
            };
                quest.Param("MAP", dict);

                return quest;
            }

            private void ProcessEncrypt(TCPClient client)
            {
                //-- TODO
            }

            private void TestWorker()
            {
                int act = 0;
                for (int i = 0; i < questCount; i++)
                {
                    long index = (ClientEngine.GetCurrentMicroseconds() + i) % 64;
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
                        act = (int)(index & 0x1);

                    
                    switch (act)
                    {
                        case 0:
                            {
                                Console.Write("*");
                                Answer answer = client.SendQuest(GenQuest());
                                if (answer != null)
                                {
                                    if (answer.IsException())
                                    {
                                        if (answer.ErrorCode() == ErrorCode.FPNN_EC_CORE_CONNECTION_CLOSED
                                            || answer.ErrorCode() == ErrorCode.FPNN_EC_CORE_INVALID_CONNECTION)
                                            Console.Write("|");
                                        else
                                            Console.Write("?");
                                    }
                                    else
                                        Console.Write("^");
                                }
                                else
                                    Console.Write("?");

                                break;
                            }
                        case 1:
                            {
                                Console.Write("&");
                                bool status = client.SendQuest(GenQuest(), (Answer answer, int errorCode) => {
                                    if (errorCode == 0)
                                        Console.Write("$");
                                    else if (errorCode == ErrorCode.FPNN_EC_CORE_CONNECTION_CLOSED || errorCode == ErrorCode.FPNN_EC_CORE_INVALID_CONNECTION)
                                        Console.Write(";");
                                    else
                                        Console.Write("@");
                                });
                                if (status == false)
                                    Console.Write("@");

                                break;
                            }
				        case 2:
				            {
					            Console.Write("!");
					            client.Close();
					            break;
				            }
                    }
	            }
            }

            public void Test(int threadCount, int perThreadQuestCount)
            {
                questCount = perThreadQuestCount;

                Console.WriteLine("========[ Test: thread {0}, per thread quest: {1} ]==========", threadCount, perThreadQuestCount);

                List<Thread> threads = new List<Thread>();

                for (int i = 0; i < threadCount; i++)
                {
                    Thread thread = new Thread(TestWorker);
                    thread.Start();
                    threads.Add(thread);
                }

                Thread.Sleep(5000);

                foreach (Thread thread in threads)
                    thread.Join();

                Console.WriteLine("");
            }
        }

        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("Usage: asyncStressClient <ip> <port>");
                //return;
                args = new string[] { "52.83.245.22", "13609" };
            }

            //-- For record exception infos.
            Config config = new Config();
            config.errorRecorder = new Recorder();
            ClientEngine.Init(config);

            Tester tester = new Tester(args[0], Int32.Parse(args[1]));
            tester.ShowSignDesc();

            tester.Test(10, 30000);
            tester.Test(20, 30000);
            tester.Test(30, 30000);
            tester.Test(50, 30000);
            tester.Test(60, 30000);
        }
    }
}
