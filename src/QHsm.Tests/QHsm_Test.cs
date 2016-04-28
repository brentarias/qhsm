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
            person.Stopped += (obj, arg) => { Console.WriteLine("STATEMACHINE IS OFFLINE"); };
            var completion = person.Start();

            var messages = new object[]
            {
                new PokeMessage(),
                //Until the new interface is incorporated, multiple
                //events is not the goal.
                //new TiredMessage(),
            };

            foreach (var msg in messages)
            {
                person.Dispatch(msg);
            }

            //It should also be possible to provide the user signal directly...
            //person.Dispatch(PersonSignals.Punch);
        }

        [Fact]
        public void QHsm_InitWithCompletionNotice_Ok()
        {
            var person = new PersonMachine();
            person.TraceEvent = Program.Trace;
            person.Stopped += (obj, arg) => { Console.WriteLine("STATEMACHINE IS OFFLINE"); };
            var completion = person.Start();

            Task.Run(() =>
            {
                var messages = new object[]
                {
                    new TiredMessage(),
                    //Until the new interface is incorporated, multiple
                    //events is not the goal.
                    //new TiredMessage(),
                };

                foreach (var msg in messages)
                {
                    person.Dispatch(msg);
                }
            });

            //In production, a blocking statement like this would not
            //be used.  But a unit test framework requires this kind
            //of "wait for asynchronous test to complete" construct.
            var isSuccess = completion.Wait(80);

            Assert.True(isSuccess && completion.Status == TaskStatus.RanToCompletion);            
        }

    }
}
