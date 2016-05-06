using qf4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace QHsm.Tests
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var person = new PersonMachine();
            person.TraceEvent = Trace;
            person.Quiescent += (obj, arg) => { Console.WriteLine("STATEMACHINE IS OFFLINE"); };
            person.Failed += (obj, arg) => { Console.WriteLine("STATEMACHINE EXCEPTION"); };
            person.Start();

            PersonState state = null;
            char input = 'a';
            do
            {
                Console.WriteLine("Input an event: (P)oke, (T)ired, (N)oise, P(u)nch, (R)estore or E(x)it\n");
                var keyInfo = Console.ReadKey();
                Console.WriteLine();
                input = Char.ToLowerInvariant(keyInfo.KeyChar);
                object evt = null;
                switch (input)
                {
                    case 'p':
                        evt = new PokeMessage();
                        break;
                    case 't':
                        evt = new TiredMessage();
                        break;
                    case 'n':
                        evt = new NoiseMessage();
                        break;
                    case 'u':
                        evt = new PunchMessage();
                        break;
                    case 'x':
                        return;
                    case 'r':
                        //Simulate stopping, persisting, and restoring the state-machine.
                        person.Stop();
                        state = person.CurrentState;
                        person = new PersonMachine();
                        person.TraceEvent = Trace;
                        //Normally only the state-machine is permitted to change these values,
                        //but for testing purposes on the "moody person" machine, this is ok.
                        state.DB = 100;
                        person.Start(state);
                        break;
                    default:
                        Console.WriteLine("Command '{0}' is unrecognized input", input);
                        break;
                }
                if (evt != null)
                {
                    person.Dispatch(evt);
                }


            } while (true);
        }

        [DebuggerHidden]
        public static void Trace(MethodInfo info, IQEvent evt)
        {
            string sigName = null;
            if (evt == null)
            {
                if (info != null)
                {
                    Console.WriteLine("BEGIN TRANSITION to state '{0}'", info.Name);
                }
                else
                {
                    Console.WriteLine("END TRANSITION");
                }
            }
            else
            {
                if ((int)evt.QSignal < (int)QSignals.UserSig)
                {
                    sigName = ((QSignals)evt.QSignal).ToString();
                }
                else
                {
                    sigName = ((PersonSignals)evt.QSignal).ToString();
                }
                Console.WriteLine("Event '{0}' to state '{1}'", sigName, info.Name);
            }
        }
    }
}
