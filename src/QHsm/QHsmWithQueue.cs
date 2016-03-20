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


using System;
using System.Collections;
using qf4net;

namespace qf4net
{
    /// <summary>
    /// Designed to allow a state machine to post NEW messages to itself during the handling of a dispatch call. 
    /// The assumption is that most environments (Windows, XWindows) will support event handling and that the 
    /// only time this mechanism will be required is for self-posting. Therefore, the public interface is limited 
    /// to a single method, DispatchQ. And this method could actually be modified to version or override the 
    /// base class Dispatch method...
    /// </summary>
    public abstract class QHsmQ : QHsm
    {
        /// <summary>
        /// FIFO event queue
        /// </summary>
        private Queue m_EventQueue;

        /// <summary>
        /// Constructor for the Quantum Hierarchical State Machine with Queue
        /// </summary>
        public QHsmQ()
        {
            m_EventQueue = new Queue();
        }


        /// <summary>
        /// Designed to be used only for self-posting, but this design could easily be changed
        /// by making this method public.
        /// </summary>
        /// <param name="qEvent">New message posted to self during processing</param>
        protected void Enqueue(QEvent qEvent)
        {
            m_EventQueue.Enqueue(qEvent);
        }

        /// <summary>
        /// Dequeues and dispatches the queued events to this state machine
        /// </summary>
        protected void DispatchQ()
        {
            if (isDispatching)
            {
                return;
            }

            isDispatching = true;
            while (m_EventQueue.Count > 0)
            {
                //new events may be added (self-posted) during the dispatch handling of this first event
                base.Dispatch((QEvent)m_EventQueue.Dequeue());
            }
            isDispatching = false;

        }//DispatchQ

        private bool isDispatching = false;

        /// <summary>
        /// Enqueues the first event then dequeues and dispatches all queued events to this state machine.
        /// Designed to be called in place of base.Dispatch in the event self-posting is to be 
        /// supported.
        /// </summary>
        public void DispatchQ(QEvent qEvent)
        {
            m_EventQueue.Enqueue(qEvent);
            DispatchQ();

        }//DispatchQ

        /// <summary>
        /// Empties the event queue
        /// </summary>
        protected void ClearQ()
        {
            m_EventQueue.Clear();
        }

    }//QHsmQ

}//namespace ReminderHsm