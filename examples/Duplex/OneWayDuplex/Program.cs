using System;
using com.fpnn;
using com.fpnn.proto;

namespace OneWayDuplex
{
    class Program
    {
        class QuestProcessor : IQuestProcessor
        {
            public QuestProcessDelegate GetQuestProcessDelegate(string method)
            {
                if (method == "duplex quest")
                    return DuplexQuest;
                else
                {
                    Console.WriteLine("Received unsupported method: {0}", method);
                    return null;
                }
            }

            public Answer DuplexQuest(Int64 connectionId, string endpoint, Quest quest)
            {
                Console.WriteLine("Received server push. Value of key 'int' is{0}", quest.Want<int>("int"));
                return null;
            }
        }

        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("Usage: TwoWayDuplex <ip> <port>");
                args = new string[] { "52.83.245.22", "13609" };
                //return;
            }

            TCPClient client = new TCPClient(args[0], Int32.Parse(args[1]));
            client.SetQuestProcessor(new QuestProcessor());

            Quest quest = new Quest("duplex demo");
            quest.Param("duplex method", "duplex quest");
            quest.Param("duplex in one way", true);

            Answer answer = client.SendQuest(quest);
            if (answer.IsException())
                Console.WriteLine("Received error answer of quest. code is {0}", answer.ErrorCode());
            else
                Console.WriteLine("Received answer of quest.");
        }
    }
}
