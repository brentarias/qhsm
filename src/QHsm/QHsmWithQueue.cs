// -----------------------------------------------------------------------------
//                            qf4net Library
//
// Port of Samek's Quantum Framework to C#. The implementation takes the liberty
// to depart from Miro Samek's code where the specifics of desktop systems 
// (compared to embedded systems) seem to warrant a different approach.
// Please see accompanying documentation for details.
// 
// Reference:
// Practical Statecharts in C/C++; Quantum Programming for Embedded Systems
// Author: Miro Samek, Ph.D.
// http://www.quantum-leaps.com/book.htm
//
// -----------------------------------------------------------------------------
//
// Copyright (C) 2003-2004, The qf4net Team
// All rights reserved
// Lead: Rainer Hessmer, Ph.D. (rainer@hessmer.org)
// 
//
//   Redistribution and use in source and binary forms, with or without
//   modification, are permitted provided that the following conditions
//   are met:
//
//     - Redistributions of source code must retain the above copyright
//        notice, this list of conditions and the following disclaimer. 
//
//     - Neither the name of the qf4net-Team, nor the names of its contributors
//        may be used to endorse or promote products derived from this
//        software without specific prior written permission. 
//
//   THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
//   "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
//   LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS
//   FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL
//   THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT,
//   INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
//   (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
//   SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION)
//   HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT,
//   STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
//   ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED
//   OF THE POSSIBILITY OF SUCH DAMAGE.
// -----------------------------------------------------------------------------
// Rev History:
// *Aug 26 2010: changed queue to generic queue.
// *Apr 29 2013: changed generic queue to concurrent queue, and made DispatchQ(...) thread-safe.
// *Apr 2 2013: 
//   converted Dispatch to virtual, to allow telemetry or instrumentation access.
//   added debugger directive "DebuggerNonUserCode".  No point stepping through boiler-plate.
//   enabled object-based messaging.

using System;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace qf4net
{
    /// <summary>
    /// Designed to allow a state machine to post NEW messages to itself during the handling of a dispatch call. 
    /// The assumption is that most environments (Windows, XWindows) will support event handling and that the 
    /// only time this mechanism will be required is for self-posting. Therefore, the public interface is limited 
    /// to a single method, DispatchQ. And this method could actually be modified to version or override the 
    /// base class Dispatch method...
    /// </summary>
    [DebuggerNonUserCode]
    public abstract class QHsmQ : QHsm
    {
        /// <summary>
        /// FIFO event queue
        /// </summary>
        private ConcurrentQueue<IQEvent> m_EventQueue = new ConcurrentQueue<IQEvent>();

        /// <summary>
        /// Can be used for typed message to signal conversion
        /// </summary>
        /// <param name="userSignals">a typeof(myEnum) value</param>
        public QHsmQ(Type userSignals = null) : base(userSignals) { }

        /// <summary>
        /// Designed to be used only for self-posting, but this design could easily be changed
        /// by making this method public.
        /// </summary>
        /// <param name="qEvent">New message posted to self during processing</param>
        protected void Enqueue(IQEvent qEvent)
        {
            m_EventQueue.Enqueue(qEvent);
        }

        /// <summary>
        /// Dequeues and dispatches the queued events to this state machine
        /// </summary>
        protected void Dispatch()
        {
            IQEvent msg = null;
            do
            {
                //It is not the ConcurrentQueue that needs this synchronization lock.
                //The lock is here because only one-thread-at-a-time is allowed to dispatch 
                //to the state machine, and so a lock is necessary to dismiss all other threads.
                lock (m_EventQueue)
                {
                    switch (machineState)
                    {
                        case MachineState.Online:
                            if (msg != null)
                            {
                                isDispatching = false;
                            }
                            if (!isDispatching && m_EventQueue.TryDequeue(out msg))
                            {
                                isDispatching = true;
                            }
                            break;
                        case MachineState.Exiting:
                            machineState = MachineState.Offline;
                            msg = null;
                            OnStop(EventArgs.Empty);
                            break;
                        default:
                            throw new NotSupportedException("QSM needs to send 'Restart' event");
                            //break;
                    }
                }
                if (msg != null)
                {
                    base.Dispatch(msg);
                }
            }
            while (msg != null);
        }//Dispatch

        private bool isDispatching = false;

        /// <summary>
        /// Enqueues the first event then dequeues and dispatches all queued events to this state machine.
        /// Designed to be called in place of base.Dispatch in the event self-posting is to be 
        /// supported.
        /// </summary>
        public override void Dispatch(IQEvent qEvent)
        {
            m_EventQueue.Enqueue(qEvent);
            Dispatch();

        }//Dispatch

        /// <summary>
        /// Prevent further dispatches to the state-machine.  If called internally, the current thread
        /// is given opportunity to back up the call-stack to the Dispatch(IQEvent), before raising
        /// a "Stopped" event to external subscribers.
        /// </summary>
        /// <param name="exitNow"></param>
        public override void Stop()
        {
            lock (m_EventQueue)
            {
                base.Stop();
            }
        }

    }//QHsmQ

}//namespace ReminderHsm