using qf4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
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
            person.Quiescent += (obj, arg) => { Console.WriteLine("STATEMACHINE IS OFFLINE"); };
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

            //It should also be possible to provide the user signal directly...
            person.Dispatch(PersonSignals.Punch);
        }

        [Fact]
        public async Task QHsm_InitWithCompletionNotice_Ok()
        {
            var person = new PersonMachine();
            person.TraceEvent = Program.Trace;
            person.Quiescent += (obj, arg) => { Console.WriteLine("STATEMACHINE IS OFFLINE"); };
            person.Start();
            Task completion = null;

            var messages = new object[]
            {
                new TiredMessage(),
                new TiredMessage(),
            };

            foreach (var msg in messages)
            {
                completion = person.Dispatch(msg);
            }

            bool isSuccess = await Task.WhenAny(completion, Task.Delay(80)) == completion;

            Assert.True(isSuccess && completion.Status == TaskStatus.RanToCompletion);            
        }

    }
}
