using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using DNT.Diag.DB;
using DNT.Diag.Data;
using DNT.Diag.Commbox;
using DNT.Diag.Channel;
using DNT.Diag.Formats;

namespace DNT.Diag.ECU
{
    public abstract class DataStreamFunction : AbstractFunction
    {
        private bool stopRead;
        private bool stopCalc;
        private bool readExp;
        private LiveDataList lds;
        private Timer readInterval;
        private Timer calcInterval;
        private Mutex readMutex;
        private Mutex calcMutex;
        private TaskFactory taskFactory;
        private Task[] tasks;
        private byte[] readBuff;
        private Dictionary<string, byte[]> historyBuff;
        public Action BeginRead;
        public Action EndRead;
        public Action BeginCalc;
        public Action EndCalc;

        public DataStreamFunction(VehicleDB db, IChannel chn, IFormat format)
			: base(db, chn, format)
        {
            stopRead = true;
            stopCalc = true;
            readExp = false;
            lds = null;
            readInterval = Timer.FromMilliseconds(10);
            calcInterval = Timer.FromMilliseconds(1);
            readMutex = new Mutex();
            calcMutex = new Mutex();
            taskFactory = new TaskFactory();
            tasks = null;
            readBuff = new byte[128];
            historyBuff = new Dictionary<string, byte[]>();
            BeginRead = null;
            EndRead = null;
            BeginCalc = null;
            EndCalc = null;
        }

        protected abstract void InitCalcDelegates();

        public Timer ReadInterval
        {
            get { return readInterval; }
            set { readInterval = value; }
        }

        public Timer CalcInterval
        {
            get { return calcInterval; }
            set { calcInterval = value; }
        }

        public LiveDataList LiveDataItems
        {
            get { return lds; }
        }

        public bool QueryLiveData(string cls)
        {
            try
            {
                lds = Database.QueryLiveData(cls);
                InitCalcDelegates();
                return true;
            }
            catch
            {
            }
            return false;
        }

        public void Stop()
        {
            if (taskFactory == null || tasks == null || tasks.Length == 0)
                return;

            readMutex.WaitOne();
            stopRead = true;
            readMutex.ReleaseMutex();

            calcMutex.WaitOne();
            stopCalc = true;
            calcMutex.ReleaseMutex();

            foreach (Task t in tasks)
                t.Wait();
        }

        private void ReadBegin()
        {
            stopRead = false;
            if (BeginRead != null)
            {
                try
                {
                    BeginRead();
                }
                catch
                {
                    stopRead = true;
                    readExp = true;
                    throw;
                }
            }
        }

        private void ReadEnd()
        {
            if (EndRead != null)
            {
                try
                {
                    EndRead();
                }
                catch
                {
                }
            }
        }

        private void ReadThread(Dictionary<string, byte[]> cmdMap)
        {
            foreach (var map in cmdMap)
            {
                readMutex.WaitOne();
                if (stopRead)
                {
                    readMutex.ReleaseMutex();
                    return;
                }
                readMutex.ReleaseMutex();

                var ldBuff = lds.GetMsgBuffer(map.Key);
                int length = 0;
                try
                {
                    length = Channel.SendAndRecv(map.Value, 0, map.Value.Length, readBuff);
                    ldBuff.CopyTo(readBuff, 0, length);
                }
                catch
                {
                }
					
                Thread.Sleep(readInterval.ToTimeSpan());
            }
        }

        private void ReadThread()
        {
            ReadBegin();

            while (true)
            {
                readMutex.WaitOne();
                if (stopRead || readExp)
                {
                    readMutex.ReleaseMutex();
                    break;
                }
                readMutex.ReleaseMutex();
                ReadThread(lds.CommandNeed);
            }

            ReadEnd();
        }

        private void CalcBegin()
        {
            if (BeginCalc != null)
                BeginCalc();
        }

        private void CalcEnd()
        {
            if (EndCalc != null)
                EndCalc();
        }

        private void CalcThread(List<LiveDataItem> items)
        {
            foreach (var item in items)
            {
                calcMutex.WaitOne();
                if (stopCalc || readExp)
                {
                    calcMutex.ReleaseMutex();
                    return;
                }
                calcMutex.ReleaseMutex();

                item.CalcValue();

                Thread.Sleep(calcInterval.ToTimeSpan());
            }
        }

        private void CalcThread()
        {
            CalcBegin();


            while (true)
            {
                calcMutex.WaitOne();
                if (stopCalc || readExp)
                {
                    calcMutex.ReleaseMutex();
                    break;
                }
                calcMutex.ReleaseMutex();

                CalcThread(lds.DisplayItems);
            }

            CalcEnd();
        }

        public void StartOnce()
        {
            tasks = new Task[]
            {
                taskFactory.StartNew(() =>
                {
                    ReadBegin();
                    ReadThread(lds.CommandNeed);
                    ReadEnd();

                    CalcBegin();
                    CalcThread(lds.DisplayItems);
                    CalcEnd();
                })
            };

            taskFactory.ContinueWhenAll(tasks, (task) =>
            {
                foreach (var t in tasks)
                {
                    if (t.IsFaulted)
                    {
                        throw t.Exception.InnerException;
                    }
                }
            });
        }

        public void Start()
        {
            tasks = new Task[]
            {
                taskFactory.StartNew(() =>
                {
                    CalcThread();
                }),
                taskFactory.StartNew(() =>
                {
                    ReadThread();
                })
            };

            taskFactory.ContinueWhenAll(tasks, (task) =>
            {
                foreach (var t in tasks)
                {
                    if (t.IsFaulted)
                    {
                        throw t.Exception.InnerException;
                    }
                }
            });
        }

        protected static void CheckOutOfRange(double value, LiveDataItem ld)
        {
            try
            {
                double min = Convert.ToDouble(ld.MinValue);
                double max = Convert.ToDouble(ld.MaxValue);

                if (value < min || value > max)
                    ld.IsOutOfRange = true;
                else
                    ld.IsOutOfRange = false;
            }
            catch
            {
            }
        }

        public Dictionary<string, byte[]> HistoryBuff
        {
            get { return historyBuff; }
        }
    }
}

