using System;
using System.Collections.Generic;
using System.Threading;
using com.fpnn;
using com.fpnn.proto;

namespace asyncStressClient
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 4)
            {
                Console.WriteLine("Usage: asyncStressClient <ip> <port> <connections> <totalQPS>");
                return;
            }

            //-- For record exception infos.
            Config config = new Config();
            config.errorRecorder = new Recorder();
            ClientEngine.Init(config);

            Tester tester = new Tester(args[0], Int32.Parse(args[1]), Int32.Parse(args[2]), Int32.Parse(args[3]));
            tester.Launch();
            tester.ShowStatistics();
        }
    }

    class Recorder : com.fpnn.common.ErrorRecorder
    {
        public void RecordError(Exception e)
        {
            Console.WriteLine(e.ToString());
        }
        public void RecordError(string message)
        {
            Console.WriteLine(message);
        }
        public void RecordError(string message, Exception e)
        {
            Console.WriteLine(message + "\n" + e.ToString());
        }
    }

    class Tester
    {
        private string ip;
        private int port;
        private int threadCount;
        private int qps;
        private volatile bool running;

        private long sendCount;
        private long recvCount;
        private long recvErrorCount;
        private long timeCost;                  //-- in usec

        private List<Thread> threads;

        public Tester(string ip, int port, int threadCount, int qps)
        {
            this.ip = ip;
            this.port = port;
            this.threadCount = threadCount;
            this.qps = qps;

            running = true;

            sendCount = 0;
            recvCount = 0;
            recvErrorCount = 0;
            timeCost = 0;

            threads = new List<Thread>();
        }

        ~Tester()
        {
            Close();
        }

        public void Close()
        {
            Stop();
        }

        public void Launch()
        {
            int pqps = qps / threadCount;
            if (pqps == 0)
                pqps = 1;

            int remain = qps - pqps * threadCount;

            for (int i = 0; i < threadCount; i++)
            {
                Thread thread = new Thread(TestWorker);
                thread.Start(pqps);
                threads.Add(thread);
            }

            if (remain > 0)
            {
                Thread thread = new Thread(TestWorker);
                thread.Start(remain);
                threads.Add(thread);
            }
        }

        public void Stop()
        {
            running = false;
            foreach (Thread thread in threads)
            {
                thread.Join();
            }
        }

        private Quest GenQuest()
        {
            Quest quest = new Quest("two way demo");
            quest.Param("quest", "one");
            quest.Param("int", 2);
            quest.Param("double", 3.3);
            quest.Param("boolean", true);

            List<object> array = new List<object> { "first_vec" , 4 };
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

        public void ShowStatistics()
        {
            const int sleepSeconds = 3;

            Int64 send = 0;
            Int64 recv = 0;
            Int64 recvError = 0;
            Int64 timecost = 0;

            while (running)
            {
                Int64 start = ClientEngine.GetCurrentMicroseconds();

                Thread.Sleep(sleepSeconds * 1000);

                Int64 s = Interlocked.Read(ref sendCount);
                Int64 r = Interlocked.Read(ref recvCount);
                Int64 re = Interlocked.Read(ref recvErrorCount);
                Int64 tc = Interlocked.Read(ref timeCost);

                Int64 ent = ClientEngine.GetCurrentMicroseconds();

                Int64 ds = s - send;
                Int64 dr = r - recv;
                Int64 dre = re - recvError;
                Int64 dtc = tc - timecost;

                send = s;
                recv = r;
                recvError = re;
                timecost = tc;

                Int64 real_time = ent - start;

                if (dr > 0)
                    dtc = dtc / dr;

                ds = ds * 1000 * 1000 / real_time;
                dr = dr * 1000 * 1000 / real_time;
                //dse = dse * 1000 * 1000 / real_time;
                //dre = dre * 1000 * 1000 / real_time;

                Console.WriteLine("time interval: " + (real_time / 1000.0) + " ms, recv error: " + dre);
                Console.WriteLine("[QPS] send: " + ds + ", recv: " + dr + ", per quest time cost: " + dtc + " usec");
            }

        }

        private void ProcessEncrypt(TCPClient client)
        {
            //-- TODO
        }

        private void TestWorker(object obj)
        {
            int qps = (int)obj;
            int msec = 1000 / qps;

            lock (this)
            {
                Console.WriteLine("-- qps: " + qps + ", sleep milliseconds interval: " + msec);
            }

            TCPClient client = new TCPClient(ip, port);
            ProcessEncrypt(client);
            if (!client.SyncConnect())
                lock (this)
                {
                    Console.WriteLine("Client sync connect remote server " + ip + ":" + port + " failed.");
                }

            while (running)
            {
                Quest quest = GenQuest();
                Int64 send_time = ClientEngine.GetCurrentMicroseconds();

                client.SendQuest(quest, (Answer answer, int errorCode) =>
                {
                    if (errorCode != ErrorCode.FPNN_EC_OK)
                    {
                        Interlocked.Add(ref recvErrorCount, 1);
                        if (errorCode == ErrorCode.FPNN_EC_CORE_TIMEOUT)
                        {
                            lock (this)
                                Console.WriteLine("Timeouted occurred when recving.");
                        }
                        else
                        {
                            lock (this)
                                Console.WriteLine("Error occurred when recving..");
                        }
                    }
                    else
                    {
                        Interlocked.Add(ref recvCount, 1);

                        Int64 recv_time = ClientEngine.GetCurrentMicroseconds();
                        Int64 diff = recv_time - send_time;
                        Interlocked.Add(ref timeCost, diff);
                    }
                });

                Interlocked.Add(ref sendCount, 1);

                Int64 sent_time = ClientEngine.GetCurrentMicroseconds();
                Int64 real_usec = msec * 1000 - (sent_time - send_time);
                if (real_usec > 1000)
                    Thread.Sleep((int)(real_usec / 1000));
                else if (real_usec > 500)
                    Thread.Sleep(1);
            }

            client.Close();
        }
    }
}
