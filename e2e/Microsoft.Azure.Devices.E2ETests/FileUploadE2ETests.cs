﻿using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Azure.Devices.E2ETests
{
    [TestClass]
    public class FileUploadE2ETests
    {
        private const string DevicePrefix = "E2E_FileUpload_CSharp_";
        private static string hubConnectionString;
        private static string hostName;
        private static RegistryManager registryManager;
        private static ServiceClient serviceClient;
        private static FileNotificationReceiver<FileNotification> fileNotificationReceiver;

        private SemaphoreSlim sequentialTestSemaphore = new SemaphoreSlim(1, 1);

        private const string smallFile = "FileUpload_test_small.txt";
        private const string bigFile = "FileUpload_test_big.txt";

        [ClassInitialize]
        static public void ClassInitialize(TestContext testContext)
        {
            var environment = TestUtil.InitializeEnvironment("E2E_FileUpload_CSharp_");
            hubConnectionString = environment.Item1;
            registryManager = environment.Item2;
            hostName = TestUtil.GetHostName(hubConnectionString);

            serviceClient = ServiceClient.CreateFromConnectionString(hubConnectionString);
            fileNotificationReceiver = serviceClient.GetFileNotificationReceiver();

            File.WriteAllBytes(smallFile, new byte[10 * 1024]);
            File.WriteAllBytes(bigFile, new byte[5120 * 1024]);
        }

        [ClassCleanup]
        static public void ClassCleanup()
        {
            serviceClient.CloseAsync().Wait();
            System.IO.File.Delete(smallFile);
            System.IO.File.Delete(bigFile);
            TestUtil.UnInitializeEnvironment(registryManager);
        }

        [TestInitialize]
        public async void Initialize()
        {
            await sequentialTestSemaphore.WaitAsync();
        }

        [TestCleanup]
        public void Cleanup()
        {
            sequentialTestSemaphore.Release(1);
        }

        [TestMethod]
        [TestCategory("FileUpload-E2E")]
        public async Task FileUpload_SmallFile_Amqp()
        {
            await uploadFile(Client.TransportType.Amqp_Tcp_Only, smallFile);
        }

        [TestMethod]
        [TestCategory("FileUpload-E2E")]
        public async Task FileUpload_SmallFile_AmqpWs()
        {
            await uploadFile(Client.TransportType.Amqp_WebSocket_Only, smallFile);
        }

        [TestMethod]
        [TestCategory("FileUpload-E2E")]
        public async Task FileUpload_SmallFile_Mqtt()
        {
            await uploadFile(Client.TransportType.Mqtt_Tcp_Only, smallFile);
        }

        [TestMethod]
        [TestCategory("FileUpload-E2E")]
        public async Task FileUpload_SmallFile_MqttWs()
        {
            await uploadFile(Client.TransportType.Mqtt_WebSocket_Only, smallFile);
        }

        [TestMethod]
        [TestCategory("FileUpload-E2E")]
        public async Task FileUpload_SmallFile_Http()
        {
            await uploadFile(Client.TransportType.Http1, smallFile);
        }

        [TestMethod]
        [TestCategory("FileUpload-E2E")]
        public async Task FileUpload_BigFile_Amqp()
        {
            await uploadFile(Client.TransportType.Amqp_Tcp_Only, bigFile);
        }

        [TestMethod]
        [TestCategory("FileUpload-E2E")]
        public async Task FileUpload_BigFile_AmqpWs()
        {
            await uploadFile(Client.TransportType.Amqp_WebSocket_Only, bigFile);
        }

        [TestMethod]
        [TestCategory("FileUpload-E2E")]
        public async Task FileUpload_BigFile_Mqtt()
        {
            await uploadFile(Client.TransportType.Mqtt_Tcp_Only, bigFile);
        }

        [TestMethod]
        [TestCategory("FileUpload-E2E")]
        public async Task FileUpload_BigFile_MqttWs()
        {
            await uploadFile(Client.TransportType.Mqtt_WebSocket_Only, bigFile);
        }

        [TestMethod]
        [TestCategory("FileUpload-E2E")]
        public async Task FileUpload_BigFile_Http()
        {
            await uploadFile(Client.TransportType.Http1, bigFile);
        }

        [TestMethod]
        [TestCategory("FileUpload-E2E")]
        public async Task FileUpload_X509_SmallFile_Amqp()
        {
            await uploadFile(Client.TransportType.Amqp_Tcp_Only, smallFile, true);
        }

        [TestMethod]
        [TestCategory("FileUpload-E2E")]
        public async Task FileUpload_X509_SmallFile_AmqpWs()
        {
            await uploadFile(Client.TransportType.Amqp_WebSocket_Only, smallFile, true);
        }

        [TestMethod]
        [TestCategory("FileUpload-E2E")]
        public async Task FileUpload_X509_SmallFile_Mqtt()
        {
            await uploadFile(Client.TransportType.Mqtt_Tcp_Only, smallFile, true);
        }

        [TestMethod]
        [TestCategory("FileUpload-E2E")]
        public async Task FileUpload_X509_SmallFile_MqttWs()
        {
            await uploadFile(Client.TransportType.Mqtt_WebSocket_Only, smallFile, true);
        }

        [TestMethod]
        [TestCategory("FileUpload-E2E")]
        public async Task FileUpload_X509_SmallFile_Http()
        {
            await uploadFile(Client.TransportType.Http1, smallFile, true);
        }

        [TestMethod]
        [TestCategory("FileUpload-E2E")]
        [TestCategory("Recovery")]
        public async Task FileUploadSuccess_TcpLoss_Amqp()
        {
            await uploadFileDisconnectTransport(Client.TransportType.Amqp_Tcp_Only,
                smallFile,
                TestUtil.FaultType_Tcp,
                TestUtil.FaultCloseReason_Boom,
                TestUtil.DefaultDelayInSec
                );
        }

        [TestMethod]
        [TestCategory("FileUpload-E2E")]
        [TestCategory("Recovery")]
        public async Task FileUploadSuccess_TcpLoss_AmqpWs()
        {
            await uploadFileDisconnectTransport(Client.TransportType.Amqp_WebSocket_Only,
                smallFile,
                TestUtil.FaultType_Tcp,
                TestUtil.FaultCloseReason_Boom,
                TestUtil.DefaultDelayInSec
                );
        }

        [Ignore]
        [TestMethod]
        [TestCategory("FileUpload-E2E")]
        [TestCategory("Recovery")]
        public async Task FileUploadSuccess_TcpLoss_Mqtt()
        {
            await uploadFileDisconnectTransport(Client.TransportType.Mqtt_Tcp_Only,
                smallFile,
                TestUtil.FaultType_Tcp,
                TestUtil.FaultCloseReason_Boom,
                TestUtil.DefaultDelayInSec
                );
        }

        [Ignore]
        [TestMethod]
        [TestCategory("FileUpload-E2E")]
        [TestCategory("Recovery")]
        public async Task FileUploadSuccess_TcpLoss_MqttWs()
        {
            await uploadFileDisconnectTransport(Client.TransportType.Mqtt_WebSocket_Only,
                smallFile,
                TestUtil.FaultType_Tcp,
                TestUtil.FaultCloseReason_Boom,
                TestUtil.DefaultDelayInSec
                );
        }

        [TestMethod]
        [TestCategory("FileUpload-E2E")]
        [TestCategory("Recovery")]
        public async Task FileUploadSuccess_AmqpConnLoss_Amqp()
        {
            await uploadFileDisconnectTransport(Client.TransportType.Amqp_Tcp_Only,
                smallFile,
                TestUtil.FaultType_AmqpConn,
                TestUtil.FaultCloseReason_Boom,
                TestUtil.DefaultDelayInSec
                );
        }

        [TestMethod]
        [TestCategory("FileUpload-E2E")]
        [TestCategory("Recovery")]
        public async Task FileUploadSuccess_AmqpConnLoss_AmqpWs()
        {
            await uploadFileDisconnectTransport(Client.TransportType.Amqp_WebSocket_Only,
                smallFile,
                TestUtil.FaultType_AmqpConn,
                TestUtil.FaultCloseReason_Boom,
                TestUtil.DefaultDelayInSec
                );
        }

        [TestMethod]
        [TestCategory("FileUpload-E2E")]
        [TestCategory("Recovery")]
        public async Task FileUploadSuccess_Throttled_Amqp()
        {
            await uploadFileDisconnectTransport(Client.TransportType.Amqp_Tcp_Only,
                smallFile,
                TestUtil.FaultType_Throttle,
                TestUtil.FaultCloseReason_Boom,
                TestUtil.DefaultDelayInSec,
                TestUtil.DefaultDurationInSec
                );
        }

        [TestMethod]
        [TestCategory("FileUpload-E2E")]
        [TestCategory("Recovery")]
        public async Task FileUploadSuccess_Throttled_AmqpWs()
        {
            await uploadFileDisconnectTransport(Client.TransportType.Amqp_WebSocket_Only,
                smallFile,
                TestUtil.FaultType_Throttle,
                TestUtil.FaultCloseReason_Boom,
                TestUtil.DefaultDelayInSec,
                TestUtil.DefaultDurationInSec
                );
        }
        [TestMethod]
        [TestCategory("FileUpload-E2E")]
        [TestCategory("Recovery")]
        public async Task FileUploadSuccess_Throttled_Mqtt()
        {
            await uploadFileDisconnectTransport(Client.TransportType.Mqtt_Tcp_Only,
                smallFile,
                TestUtil.FaultType_Throttle,
                TestUtil.FaultCloseReason_Boom,
                TestUtil.DefaultDelayInSec,
                TestUtil.DefaultDurationInSec
                );
        }
        [TestMethod]
        [TestCategory("FileUpload-E2E")]
        [TestCategory("Recovery")]
        public async Task FileUploadSuccess_Throttled_MqttWs()
        {
            await uploadFileDisconnectTransport(Client.TransportType.Mqtt_WebSocket_Only,
                smallFile,
                TestUtil.FaultType_Throttle,
                TestUtil.FaultCloseReason_Boom,
                TestUtil.DefaultDelayInSec,
                TestUtil.DefaultDurationInSec
                );
        }

        [TestMethod]
        [TestCategory("FileUpload-E2E")]
        [TestCategory("Recovery")]
        public async Task FileUploadSuccess_Throttled_Http()
        {
            await uploadFileDisconnectTransport(Client.TransportType.Http1,
                smallFile,
                TestUtil.FaultType_Throttle,
                TestUtil.FaultCloseReason_Boom,
                TestUtil.DefaultDelayInSec,
                TestUtil.DefaultDurationInSec,
                TestUtil.ShortRetryInMilliSec
                );
        }

        [TestMethod]
        [TestCategory("FileUpload-E2E")]
        [TestCategory("Recovery")]
        public async Task FileUploadSuccess_QuotaExceed_Amqp()
        {
            await uploadFileDisconnectTransport(Client.TransportType.Amqp_Tcp_Only,
                smallFile,
                TestUtil.FaultType_QuotaExceeded,
                TestUtil.FaultCloseReason_Boom,
                TestUtil.DefaultDelayInSec,
                TestUtil.DefaultDurationInSec
                );
        }

        [TestMethod]
        [TestCategory("FileUpload-E2E")]
        [TestCategory("Recovery")]
        public async Task FileUploadSuccess_QuotaExceed_AmqpWs()
        {
            await uploadFileDisconnectTransport(Client.TransportType.Amqp_WebSocket_Only,
                smallFile,
                TestUtil.FaultType_QuotaExceeded,
                TestUtil.FaultCloseReason_Boom,
                TestUtil.DefaultDelayInSec,
                TestUtil.DefaultDurationInSec
                );
        }

        [TestMethod]
        [TestCategory("FileUpload-E2E")]
        [TestCategory("Recovery")]
        public async Task FileUploadSuccess_QuotaExceed_Mqtt()
        {
            await uploadFileDisconnectTransport(Client.TransportType.Mqtt_Tcp_Only,
                smallFile,
                TestUtil.FaultType_QuotaExceeded,
                TestUtil.FaultCloseReason_Boom,
                TestUtil.DefaultDelayInSec,
                TestUtil.DefaultDurationInSec
                );
        }

        [TestMethod]
        [TestCategory("FileUpload-E2E")]
        [TestCategory("Recovery")]
        public async Task FileUploadSuccess_QuotaExceed_MqttWs()
        {
            await uploadFileDisconnectTransport(Client.TransportType.Mqtt_WebSocket_Only,
                smallFile,
                TestUtil.FaultType_QuotaExceeded,
                TestUtil.FaultCloseReason_Boom,
                TestUtil.DefaultDelayInSec,
                TestUtil.DefaultDurationInSec
                );
        }

        [TestMethod]
        [TestCategory("FileUpload-E2E")]
        [TestCategory("Recovery")]
        public async Task FileUploadSuccess_QuotaExceed_Http()
        {
            await uploadFileDisconnectTransport(Client.TransportType.Http1,
                smallFile,
                TestUtil.FaultType_QuotaExceeded,
                TestUtil.FaultCloseReason_Boom,
                TestUtil.DefaultDelayInSec,
                TestUtil.DefaultDurationInSec
                );
        }

        async Task uploadFile(Client.TransportType transport, string filename, bool x509auth = false)
        {
            DeviceClient deviceClient;
            Tuple<string, string> deviceInfo;
            if (x509auth)
            {
                deviceInfo = TestUtil.CreateDeviceWithX509(DevicePrefix, hostName, registryManager);

                string certBase64 = Environment.GetEnvironmentVariable("IOTHUB_X509_PFX_CERTIFICATE");
                Byte[] buff = Convert.FromBase64String(certBase64);
                var cert = new X509Certificate2(buff);

                var auth = new DeviceAuthenticationWithX509Certificate(deviceInfo.Item1, cert);
                deviceClient = DeviceClient.Create(deviceInfo.Item2, auth, transport);
            }
            else
            {
                deviceInfo = TestUtil.CreateDevice(DevicePrefix, hostName, registryManager);
                deviceClient = DeviceClient.CreateFromConnectionString(deviceInfo.Item2, transport);
            }

            using (FileStream fileStreamSource = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                await deviceClient.UploadToBlobAsync(filename, fileStreamSource);
            }

            FileNotification fileNotification = await VerifyFileNotification(deviceInfo);

            Assert.IsNotNull(fileNotification, "FileNotification is not received.");
            Assert.AreEqual(deviceInfo.Item1 + "/" + filename, fileNotification.BlobName, "Uploaded file name mismatch in notifications");
            Assert.AreEqual(new FileInfo(filename).Length, fileNotification.BlobSizeInBytes, "Uploaded file size mismatch in notifications");
            Assert.IsFalse(string.IsNullOrEmpty(fileNotification.BlobUri), "File notification blob uri is null or empty");

            await deviceClient.CloseAsync();
            TestUtil.RemoveDevice(deviceInfo.Item1, registryManager);
        }

        private static async Task<FileNotification> VerifyFileNotification(Tuple<string, string> deviceInfo)
        {
            FileNotification fileNotification = null;

            Stopwatch sw = new Stopwatch();
            sw.Start();
            while (sw.Elapsed.Minutes < 1)
            {
                // Receive the file notification from queue
                fileNotification = await fileNotificationReceiver.ReceiveAsync(TimeSpan.FromSeconds(20));
                if (fileNotification != null)
                {
                    if (fileNotification.DeviceId == deviceInfo.Item1)
                    {
                        await fileNotificationReceiver.CompleteAsync(fileNotification);
                        break;
                    }

                    await fileNotificationReceiver.AbandonAsync(fileNotification);
                    fileNotification = null;
                }
            }
            sw.Stop();
            return fileNotification;
        }

        async Task uploadFileDisconnectTransport(Client.TransportType transport, string filename, string faultType, string reason, int delayInSec, 
            int durationInSec = 0, int retryDurationInMilliSec = 24000)
        {
            Tuple<string, string> deviceInfo = TestUtil.CreateDevice(DevicePrefix, hostName, registryManager);
            DeviceClient deviceClient = DeviceClient.CreateFromConnectionString(deviceInfo.Item2, transport);

            deviceClient.OperationTimeoutInMilliseconds = (uint)retryDurationInMilliSec;

            Task fileuploadTask;
            using (FileStream fileStreamSource = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                fileuploadTask = deviceClient.UploadToBlobAsync(filename, fileStreamSource);

                // send error command after 400ms to allow time for the actual fileupload operation to start
                await Task.Delay(400);
                try
                {
                    await
                        deviceClient.SendEventAsync(TestUtil.ComposeErrorInjectionProperties(faultType, reason,
                            delayInSec, durationInSec));
                }
                catch (Exception)
                {
                    // catch and ignore exceptions resulted from error injection and continue to 
                    // check result of the file upload status
                }

                await fileuploadTask;
            }

            FileNotification fileNotification = await VerifyFileNotification(deviceInfo);

            Assert.IsNotNull(fileNotification, "FileNotification is not received.");
            Assert.AreEqual(deviceInfo.Item1 + "/" + filename, fileNotification.BlobName, "Uploaded file name mismatch in notifications");
            Assert.AreEqual(new FileInfo(filename).Length, fileNotification.BlobSizeInBytes, "Uploaded file size mismatch in notifications");
            Assert.IsFalse(string.IsNullOrEmpty(fileNotification.BlobUri), "File notification blob uri is null or empty");

            await deviceClient.CloseAsync();
            TestUtil.RemoveDevice(deviceInfo.Item1, registryManager);
        }
    }
}
