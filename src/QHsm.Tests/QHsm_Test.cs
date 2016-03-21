using qf4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace QHsm.Tests
{
    public class QHsm_Test
    {
        [Fact]
        public void QHsm_InitWithMessageMap_Ok()
        {
            var person = new PersonMachine();
            person.TraceEvent = Program.Trace;
            person.Stopped += (obj, arg) => { Console.WriteLine("STATEMACHINE IS OFFLINE"); };
            person.Start();

            var messages = new object[]
            {
                new PokeMessage(),
                new TiredMessage(),
            };

            foreach (var msg in messages)
            {
                person.Dispatch(msg);
            }
        }

    }
}
