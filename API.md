# FPNN C# SDK API Docs

# Index

[TOC]

## Current Version

	public static readonly string com.fpnn.Config.Version = "2.0.0";

## Init & Config SDK

Initialize and configure FPNN SDK. 

	using com.fpnn;
	ClientEngine.Init();
	ClientEngine.Init(Config config);

### com.fpnn.Config Fields

* **Config.taskThreadPoolConfig.initThreadCount**

	Inited threads count of SDK task thread pool. Default value is 1.

* **Config.taskThreadPoolConfig.perfectThreadCount**

	Max resident threads count of SDK task thread pool. Default value is 2.

* **Config.taskThreadPoolConfig.maxThreadCount**

	Max threads count of SDK task thread pool, including resident threads and temporary threads. Default value is 4.

* **Config.taskThreadPoolConfig.maxQueueLengthLimitation**

	Max tasks count of SDK task thread pool. Default value is 0, means no limitation.

* **Config.taskThreadPoolConfig.tempLatencySeconds**

	How many seconds are waited for the next dispatched task before the temporary thread exit. Default value is 60.

* **Config.globalConnectTimeoutSeconds**

	Global client connecting timeout setting when no special connecting timeout are set for a client or connect function.

	Default is 5 seconds.

* **Config.globalQuestTimeoutSeconds**

	Global quest timeout setting when no special quest timeout are set for a client or sendQuest function.

	Default is 5 seconds.

* **Config.maxPayloadSize**

	Max bytes limitation for the quest & answer package. Default is 4MB.

* **Config.errorRecorder**

	Instance of com.fpnn.common.ErrorRecoder implemented. Default is null.


## FPNN TCPClient

### Constructors

	public TCPClient(string host, int port, bool autoConnect = true);
	public static TCPClient Create(string host, int port, bool autoConnect = true);
	public static TCPClient Create(string endpoint, bool autoConnect = true);

* endpoint:

	RTM servers endpoint. Please get your project endpoint from RTM Console.

* host:

	Target server host name or IP address.

* port:

	Target server port.

* autoConnect:

	Auto connect. Note: This parameter is AUTO CONNECT, not KEEP connection.

### Properties

* **ConnectTimeout**

		public volatile int ConnectTimeout { get; set; }

	Connecting timeout in seconds for current TCPClient instance. Default is 0, meaning using the global config. 

* **QuestTimeout**

		public volatile int QuestTimeout { get; set; }

	Quest timeout in seconds for current TCPClient instance. Default is 0, meaning using the global config.

* **AutoConnect**

		public volatile bool AutoConnect { get; set; }

	Auto connect. Note: This parameter is AUTO CONNECT, not KEEP connection.


### Config & Properties Methods

#### public string Endpoint()

	public string Endpoint();

Return current endpoint.

#### public ClientStatus Status()

	public ClientStatus Status();

Return client current status.

Values:

+ ClientStatus.Closed
+ ClientStatus.Connecting
+ ClientStatus.Connected

#### public bool IsConnected()

	public bool IsConnected();

Return client current is connected or not.


#### public void SetErrorRecorder(common.ErrorRecorder recorder)

	public void SetErrorRecorder(common.ErrorRecorder recorder);

Config the ErrorRecorder instance for current client. Default is null.


### Connect & Close Methods


#### public void AsyncConnect()

	public void AsyncConnect();

.....


#### public bool SyncConnect()

	public bool SyncConnect();

......


#### public void AsyncReconnect()

	public void AsyncReconnect();

.....


#### public bool SyncReconnect()

	public bool SyncReconnect();

........

#### public void Close()

	public void Close();

......

### Event Methods

#### public void SetConnectionConnectedDelegate(ConnectionConnectedDelegate ccd)

	public void SetConnectionConnectedDelegate(ConnectionConnectedDelegate ccd);

.... ....

#### public void SetConnectionCloseDelegate(ConnectionCloseDelegate cwcd)

	public void SetConnectionCloseDelegate(ConnectionCloseDelegate cwcd);

....

#### public void SetQuestProcessor(IQuestProcessor processor)

	public void SetQuestProcessor(IQuestProcessor processor);

.....

### Send Quest & Answer Methods


#### public bool SendQuest(Quest quest, IAnswerCallback callback, int timeout = 0)

	public bool SendQuest(Quest quest, IAnswerCallback callback, int timeout = 0);

.....

#### public bool SendQuest(Quest quest, AnswerDelegate callback, int timeout = 0)

	public bool SendQuest(Quest quest, AnswerDelegate callback, int timeout = 0);

.....


#### public Answer SendQuest(Quest quest, int timeout = 0)

	public Answer SendQuest(Quest quest, int timeout = 0);

.....



#### public void SendAnswer(Answer answer)

	public void SendAnswer(Answer answer);

....






