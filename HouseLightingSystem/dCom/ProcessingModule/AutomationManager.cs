using Common;
using System;
using System.Collections.Generic;
using System.Threading;

namespace ProcessingModule
{
    /// <summary>
    /// Class containing logic for automated work.
    /// </summary>
    public class AutomationManager : IAutomationManager, IDisposable
	{
		private Thread automationWorker;
        private AutoResetEvent automationTrigger;
        private IStorage storage;
		private IProcessingManager processingManager;
		private int delayBetweenCommands;
        private IConfiguration configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="AutomationManager"/> class.
        /// </summary>
        /// <param name="storage">The storage.</param>
        /// <param name="processingManager">The processing manager.</param>
        /// <param name="automationTrigger">The automation trigger.</param>
        /// <param name="configuration">The configuration.</param>
        public AutomationManager(IStorage storage, IProcessingManager processingManager, AutoResetEvent automationTrigger, IConfiguration configuration)
		{
			this.storage = storage;
			this.processingManager = processingManager;
            this.configuration = configuration;
            this.automationTrigger = automationTrigger;
        }

        /// <summary>
        /// Initializes and starts the threads.
        /// </summary>
		private void InitializeAndStartThreads()
		{
			InitializeAutomationWorkerThread();
			StartAutomationWorkerThread();
		}

        /// <summary>
        /// Initializes the automation worker thread.
        /// </summary>
		private void InitializeAutomationWorkerThread()
		{
			automationWorker = new Thread(AutomationWorker_DoWork);
			automationWorker.Name = "Aumation Thread";
		}

        /// <summary>
        /// Starts the automation worker thread.
        /// </summary>
		private void StartAutomationWorkerThread()
		{
			automationWorker.Start();
		}


        private void AutomationWorker_DoWork()
        {
            EGUConverter conv = new EGUConverter();
            while (!disposedValue)
            {
                List<PointIdentifier> pointList = new List<PointIdentifier>();

                pointList.Add(new PointIdentifier(PointType.DIGITAL_OUTPUT, 4000)); //L1
                pointList.Add(new PointIdentifier(PointType.DIGITAL_OUTPUT, 4001)); //L2
                pointList.Add(new PointIdentifier(PointType.DIGITAL_OUTPUT, 4002)); //L3
                pointList.Add(new PointIdentifier(PointType.DIGITAL_OUTPUT, 4003)); //L4
                pointList.Add(new PointIdentifier(PointType.DIGITAL_OUTPUT, 4004)); //L5
                pointList.Add(new PointIdentifier(PointType.DIGITAL_OUTPUT, 4005)); //L6
                pointList.Add(new PointIdentifier(PointType.ANALOG_OUTPUT, 5000)); //K
                pointList.Add(new PointIdentifier(PointType.DIGITAL_OUTPUT, 6000)); //I1
                pointList.Add(new PointIdentifier(PointType.DIGITAL_OUTPUT, 6001)); //I2

                List<IPoint> points = storage.GetPoints(pointList);
                int value = (int)conv.ConvertToEGU(points[6].ConfigItem.ScaleFactor, points[6].ConfigItem.Deviation, points[6].RawValue);


                if ((points[0].RawValue == 1 || points[1].RawValue == 1 || points[2].RawValue == 1 || points[3].RawValue == 1 ||
                    points[4].RawValue == 1 || points[5].RawValue == 1) && ((points[6].ConfigItem.LowLimit > value))){
                    processingManager.ExecuteWriteCommand(points[0].ConfigItem, configuration.GetTransactionId(), configuration.UnitAddress, 4000, 0);
                    processingManager.ExecuteWriteCommand(points[1].ConfigItem, configuration.GetTransactionId(), configuration.UnitAddress, 4001, 0);
                    processingManager.ExecuteWriteCommand(points[2].ConfigItem, configuration.GetTransactionId(), configuration.UnitAddress, 4002, 0);
                    processingManager.ExecuteWriteCommand(points[3].ConfigItem, configuration.GetTransactionId(), configuration.UnitAddress, 4003, 0);
                    processingManager.ExecuteWriteCommand(points[4].ConfigItem, configuration.GetTransactionId(), configuration.UnitAddress, 4004, 0);
                    processingManager.ExecuteWriteCommand(points[5].ConfigItem, configuration.GetTransactionId(), configuration.UnitAddress, 4005, 0);
                }


                    //TURN OFF L4 L5, TURN ON I2
                if (points[6].ConfigItem.LowLimit > value)
                {

                    //Turn off L4
                    processingManager.ExecuteWriteCommand(points[3].ConfigItem, configuration.GetTransactionId(), configuration.UnitAddress, 4003, 0);
                    //Turn off L5
                    processingManager.ExecuteWriteCommand(points[4].ConfigItem, configuration.GetTransactionId(), configuration.UnitAddress, 4004, 0);
                    //Turn on I2
                    processingManager.ExecuteWriteCommand(points[8].ConfigItem, configuration.GetTransactionId(), configuration.UnitAddress, 6001, 1);                

                }

                if (points[6].ConfigItem.EGU_Max == value)
                {
                    if (points[7].RawValue == 1)
                    {
                        processingManager.ExecuteWriteCommand(points[7].ConfigItem, configuration.GetTransactionId(), configuration.UnitAddress, 6000, 0);
                    }else if (points[8].RawValue == 1)
                    {
                        processingManager.ExecuteWriteCommand(points[8].ConfigItem, configuration.GetTransactionId(), configuration.UnitAddress, 6001, 0);
                    }
                }
                double change = 0;
                if (points[0].RawValue == 1)
                {
                    change += 0.5;
                }
                if (points[1].RawValue == 1)
                {
                    change += 0.5;
                }
                if (points[2].RawValue == 1)
                {
                    change += 0.5;
                }
                if (points[3].RawValue == 1)
                {
                    change += 1;
                }
                if (points[4].RawValue == 1)
                {
                    change += 1;
                }
                if (points[5].RawValue == 1)
                {
                    change += 0.3;
                }


                if (value - change <= 0)
                {
                    value = 0;
                }
                else
                {
                    value = value - (int)(change);
                    processingManager.ExecuteWriteCommand(points[6].ConfigItem, configuration.GetTransactionId(), configuration.UnitAddress, points[6].ConfigItem.StartAddress, value);
                }


                //Punjenje
                if (points[7].RawValue == 1) {
                    processingManager.ExecuteWriteCommand(points[8].ConfigItem, configuration.GetTransactionId(), configuration.UnitAddress, 6001, 0);
                    value += conv.ConvertToRaw(points[6].ConfigItem.ScaleFactor, points[6].ConfigItem.Deviation, 5);
                    if(value >= 100)
                    {
                        value = 100;
                    }
                    else
                    {
                        processingManager.ExecuteWriteCommand(points[6].ConfigItem, configuration.GetTransactionId(), configuration.UnitAddress, points[6].ConfigItem.StartAddress, value);
                    }
                }else if (points[8].RawValue == 1)
                {
                    processingManager.ExecuteWriteCommand(points[7].ConfigItem, configuration.GetTransactionId(), configuration.UnitAddress, 6000, 0);
                    value += conv.ConvertToRaw(points[6].ConfigItem.ScaleFactor, points[6].ConfigItem.Deviation, 8);
                    if (value >= 100)
                    {
                        value = 100;
                    }
                    else
                    {
                        processingManager.ExecuteWriteCommand(points[6].ConfigItem, configuration.GetTransactionId(), configuration.UnitAddress, points[6].ConfigItem.StartAddress, value);
                    }
                }


             

                automationTrigger.WaitOne(delayBetweenCommands);
            }
        }


        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls


        /// <summary>
        /// Disposes the object.
        /// </summary>
        /// <param name="disposing">Indication if managed objects should be disposed.</param>
		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
				}
				disposedValue = true;
			}
		}


		// This code added to correctly implement the disposable pattern.
		public void Dispose()
		{
			// Do not change this code. Put cleanup code in Dispose(bool disposing) above.
			Dispose(true);
			// GC.SuppressFinalize(this);
		}

        /// <inheritdoc />
        public void Start(int delayBetweenCommands)
		{
			this.delayBetweenCommands = delayBetweenCommands*1000;
            InitializeAndStartThreads();
		}

        /// <inheritdoc />
        public void Stop()
		{
			Dispose();
		}
		#endregion
	}
}
