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
// *Apr 11 2013: 
//    added MetaData object as parameter-payload.
//    added debugger directive "DebuggerNonUserCode".  No point stepping through boiler-plate.

using System.Collections.Generic;
using System;
using System.Diagnostics;

namespace qf4net
{
    /// <summary>
    ///  
    /// </summary>
    [DebuggerNonUserCode]
    public class QEvent : IQEvent
    {
        private int m_QSignal;
        private object m_MetaData;

        /// <summary>
        /// Default constructor - initializes all fields to default values
        /// </summary>
        public QEvent(int qSignal, object meta = null)
        {
            m_QSignal = qSignal;
            m_MetaData = meta;
        }

        /// <summary>
        /// The identifier of the <see cref="QEvent"/> type.
        /// </summary>
        public int QSignal
        {
            get { return m_QSignal; }
        }
        public object Message
        {
            get { return m_MetaData; }
        }

        /// <summary>
        /// The QSignal in string form. It allows for simpler debugging and logging. 
        /// </summary>
        /// <returns>The signal as string.</returns>
        public override string ToString()
        {
            switch (QSignal)
            {
                case (int)QSignals.Init: return "Init";
                case (int)QSignals.Entry: return "Entry";
                case (int)QSignals.Exit: return "Exit";
                default: return "Signal" + QSignal.ToString();
            }
        }
    }
}
