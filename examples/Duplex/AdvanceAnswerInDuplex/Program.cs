using System;
using System.Threading;
using com.fpnn;
using com.fpnn.proto;

namespace AdvanceAnswerInDuplex
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
            AdvanceAnswer.SendAnswer(new Answer(quest));

            Console.WriteLine("Answer is sent, processor will do other thing, and function will not return.");
            Thread.Sleep(2000);
            Console.WriteLine("processor function will return.");
            return null;
        }
    }

    /*class Recorder : com.fpnn.common.ErrorRecorder
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
    }*/

    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("Usage: TwoWayDuplex <ip> <port>");
                return;
            }

            //Config config = new Config();
            //config.errorRecorder = new Recorder();
            //ClientEngine.Init(config);

            TCPClient client = new TCPClient(args[0], Int32.Parse(args[1]));
            client.SetQuestProcessor(new QuestProcessor());

            Quest quest = new Quest("duplex demo");
            quest.Param("duplex method", "duplex quest");
            Answer answer = client.SendQuest(quest);
            if (answer.IsException())
                Console.WriteLine("Received error answer of quest. code is {0}", answer.ErrorCode());
            else
                Console.WriteLine("Received answer of quest.");

            Console.WriteLine("Sleep 5 seconds for waiting server pushed task process function returned.");
            Thread.Sleep(5000);
        }
    }
}
