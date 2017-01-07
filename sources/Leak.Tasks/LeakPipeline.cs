﻿using System;
using System.Collections.Generic;
using System.Threading;

namespace Leak.Tasks
{
    public class LeakPipeline : PipelineService
    {
        private readonly Thread worker;
        private readonly List<LeakPipelineTimer> ticks;

        private WaitHandle[] handles;
        private LeakPipelineTrigger[] triggers;
        private ManualResetEvent terminator;

        public LeakPipeline()
        {
            worker = new Thread(Execute);
            ticks = new List<LeakPipelineTimer>();

            terminator = new ManualResetEvent(false);
            handles = new WaitHandle[] { terminator };
            triggers = new LeakPipelineTrigger[] { new Terminator() };
        }

        public void Start()
        {
            worker.Start();
        }

        public void Stop()
        {
            terminator.Set();
            worker.Join();
        }

        public void Register(TimeSpan period, Action callback)
        {
            ticks.Add(new LeakPipelineTimer(period, callback));
        }

        public void Remove(Action callback)
        {
        }

        public void Register(LeakPipelineTrigger trigger)
        {
            ManualResetEvent handle = new ManualResetEvent(false);
            WaitHandle[] newHandles = new WaitHandle[handles.Length + 1];
            LeakPipelineTrigger[] newTriggers = new LeakPipelineTrigger[triggers.Length + 1];

            Array.Copy(handles, newHandles, handles.Length);
            Array.Copy(triggers, newTriggers, triggers.Length);

            newHandles[handles.Length] = handle;
            newTriggers[triggers.Length] = trigger;
            newTriggers[triggers.Length].Register(handle);

            triggers = newTriggers;
            handles = newHandles;
        }

        private void Execute()
        {
            TimeSpan period = TimeSpan.FromMilliseconds(50);
            DateTime next = DateTime.Now.Add(period);

            while (true)
            {
                int found = WaitHandle.WaitAny(handles, period);
                DateTime now = DateTime.Now;

                if (next < now)
                {
                    next = now;

                    foreach (LeakPipelineTimer tick in ticks)
                    {
                        tick.Execute(now);
                    }
                }

                if (found == 0)
                    break;

                if (WaitHandle.WaitTimeout != found)
                {
                    foreach (LeakPipelineTrigger trigger in triggers)
                    {
                        trigger.Execute();
                    }
                }
            }
        }

        private class Terminator : LeakPipelineTrigger
        {
            public void Register(ManualResetEvent watch)
            {
            }

            public void Execute()
            {
            }
        }
    }
}