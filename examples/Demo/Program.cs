using System;
using System.Threading;
using com.fpnn;
using com.fpnn.proto;

namespace Demo
{
    class Program
    {
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

        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("Usage: ConnectionEvents <ip> <port>");
                return;
            }

            Config config = new Config();
            config.errorRecorder = new Recorder();
            ClientEngine.Init(config);

            TCPClient client = new TCPClient(args[0], Int32.Parse(args[1]));

            Console.WriteLine("\nBegin demonstration to send one way quest");
            DemoSendOneWayQuest(client);

            Console.WriteLine("\nBegin demonstration to send empty quest");
            DemoSendEmptyQuest(client);
            
            Console.WriteLine("\nBegin demonstration to send quest in synchronous");
            DemoSendQuestInSync(client);
            
            Console.WriteLine("\nBegin demonstration to send quest in asynchronous");
            DemoSendQuestInAsync(client);
        }

        static void ShowErrorAnswer(Answer answer)
        {
            Console.WriteLine("Receive error answer: code: {0}, ex: {1}", answer.ErrorCode(), answer.Ex());
        }

        static void DemoSendEmptyQuest(TCPClient client)
        {
            Quest quest = new Quest("two way demo");
            Answer answer = client.SendQuest(quest);
            if (answer.IsException())
            {
                ShowErrorAnswer(answer);
                return;
            }

            string v1 = (string)answer.Want("Simple");
            int v2 = answer.Want<int>("Simple2");

            Console.WriteLine("Receive answer with 'two way demo' quest: 'Simple':{0}, 'Simple2':{1}", v1, v2);
        }

        static void DemoSendQuestInSync(TCPClient client)
        {
            Quest quest = new Quest("two way demo");
            quest.Param("key1", 12345);
            quest.Param("key2", 123.45);
            quest.Param("key3", "12345");

            Answer answer = client.SendQuest(quest);
            if (answer.IsException())
            {
                ShowErrorAnswer(answer);
                return;
            }

            string v1 = answer.Want<string>("Simple");
            int v2 = answer.Want<int>("Simple2");

            Console.WriteLine("Receive answer with 'two way demo' quest: 'Simple':{0}, 'Simple2':{1}", v1, v2);
        }

        static void DemoSendQuestInAsync(TCPClient client)
        {
            Quest quest = new Quest("httpDemo");
            quest.Param("key1", 12345);
            quest.Param("key2", 123.45);
            quest.Param("key3", "12345");

            bool status = client.SendQuest(quest, (Answer answer, int errorCode) => {
                if (errorCode != ErrorCode.FPNN_EC_OK)
                {
                    if (answer != null)
                        ShowErrorAnswer(answer);
                    else
                        Console.WriteLine("Receive error code {0} without available answer.", errorCode);
                }
                else
                {
                    string v1 = (string)answer.Want("HTTP");
                    int v2 = answer.Want<int>("TEST");

                    Console.WriteLine("Receive answer with 'httpDemo' quest: 'HTTP':{0}, 'TEST':{1}", v1, v2);
                }
            });

            if (status)
                Thread.Sleep(3000);
            else
                Console.WriteLine("Send 'htpDemo' quest in async failed.");
        }

        static void DemoSendOneWayQuest(TCPClient client)
        {
            Quest quest = new Quest("one way demo", true);
            client.SendQuest(quest);
        }
    }
}
