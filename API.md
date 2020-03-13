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

* Config.taskThreadPoolConfig.initThreadCount

	Inited threads count of SDK task thread pool. Default value is 1.

* Config.taskThreadPoolConfig.perfectThreadCount

	Max resident threads count of SDK task thread pool. Default value is 2.

* Config.taskThreadPoolConfig.maxThreadCount

	Max threads count of SDK task thread pool, including resident threads and temporary threads. Default value is 4.

* Config.taskThreadPoolConfig.maxQueueLengthLimitation

	Max tasks count of SDK task thread pool. Default value is 0, means no limitation.

* Config.taskThreadPoolConfig.tempLatencySeconds

	How many seconds are waited for the next dispatched task before the temporary thread exit. Default value is 60.

* Config.globalConnectTimeoutSeconds

	Global client connecting timeout setting when no special connecting timeout are set for a client or connect function.

	Default is 5 seconds.

* Config.globalQuestTimeoutSeconds

	Global quest timeout setting when no special quest timeout are set for a client or sendQuest function.

	Default is 5 seconds.

* Config.maxPayloadSize

	Max bytes limitation for the quest & answer package. Default is 4MB.

* Config.errorRecorder

	Instance of com.fpnn.common.ErrorRecoder implemented. Default is null.


## FPNN TCPClient

### Constructors

	public TCPClient(string host, int port, bool autoConnect = true);
	public static TCPClient Create(string host, int port, bool autoConnect = true);
	public static TCPClient Create(string endpoint, bool autoConnect = true);

* endpoint:

	RTM servers endpoint. Please get your project endpoint from RTM Console.

* pid:

	Project ID. Please get your project id from RTM Console.

* uid:

	User id assigned by your project.

* serverPushProcessor:

	Instance of events processor implemented com.fpnn.rtm.IRTMQuestProcessor.

	Please refer [Event Process](EventProcess.md)

### Properties

* **ConnectTimeout**

		public int ConnectTimeout { get; set; }

	Connecting/Login timeout for this RTMClient instance. Default is 0, meaning using the global config. 

* **public int QuestTimeout**

		public int QuestTimeout { get; set; }

	Quest timeout for this RTMClient instance. Default is 0, meaning using the global config.

* **public int Pid**

		public int Pid { get; }

	Project ID.

* **public int Uid**

		public int Uid { get; }

	User ID assigned by you project.

* **public RTMClient.ClientStatus Status**

		public RTMClient.ClientStatus Status { get; }

	RTM client current status.

	Values:

		+ ClientStatus.Closed
		+ ClientStatus.Connecting
		+ ClientStatus.Connected

* **public com.fpnn.common.ErrorRecorder ErrorRecorder**

		public com.fpnn.common.ErrorRecorder ErrorRecorder { set; }

	Config the ErrorRecorder instance for this RTMClient. Default is null.


### Delegates

Please refer [RTM Delegates](Delegates.md)

### Event Process

Please refer [Event Process](EventProcess.md)

### Methods

#### Login & Logout Functions

