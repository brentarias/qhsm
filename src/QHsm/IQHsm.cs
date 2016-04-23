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
// *May 1 2013: 
//  Removed "DispatchSynchronized(IQEvent)".  Only QHsmWithQueue.cs should be used for async calls.


using System;
using System.Threading.Tasks;

namespace qf4net
{
    /// <summary>
    /// Delegate that all state handlers must be a type of
    /// </summary>
    public delegate QState QState(IQEvent qEvent);

    public interface IQHsmState
    {
        string Workflow { get; set; }
    }

    /// <summary>
    /// Interface implemented by a Hierarchical State Machine.
    /// </summary>
    public interface IQHsm
    {
        /// <summary>
        /// Must only be called once by the client of the state machine to initialize the machine.
        /// </summary>
        //void Init();

        /// <summary>
        /// Regardless of how state machines are started, they are all stopped the same way.
        /// </summary>
        void Stop();

        /// <summary>
        /// Determines whether the state machine is in the state specified by <see paramref="inquiredState"/>.
        /// </summary>
        /// <param name="inquiredState">The state to check for.</param>
        /// <returns>
        /// <see langword="true"/> if the state machine is in the specified state; 
        /// <see langword="false"/> otherwise.
        /// </returns>
        /// <remarks>
        /// If the currently active state of a hierarchical state machine is s then it is in the 
        /// state s AND all its parent states.
        /// </remarks>
        bool IsInState(QState inquiredState);

        /// <summary>
        /// Returns the name of the (deepest) state that the state machine is currently in.
        /// </summary>
        string CurrentStateName { get; }

        /// <summary>
        /// Dispatches the specified event to this state machine
        /// </summary>
        /// <param name="qEvent">The <see cref="IQEvent"/> to dispatch.</param>
        void Dispatch(IQEvent qEvent);

        /// <summary>
        /// Dispatch user signals, or messages that will be auto-mapped to user signals.
        /// </summary>
        /// <param name="message"></param>
        void Dispatch(object message);

    }
}
