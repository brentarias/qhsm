# QHSM for .NET

This library provides a .Net adaptation of the `QHsm` and `QHsmWithQueue` 
base classes, which come from [Samek's QP Framework](http://www.state-machine.com/). 
These are base classes that allow a simple and direct implementation of finite-automata, 
hierarchical state-charts.  It is compatible with .NET Core (RC-update2).

## Contents

The Visual Studio 2015 solution contains a single QHsm project, and a QHsm.Tests project. 
The test project is simultaneously an xUnit unit-test project, and a demo executable.  The 
[xUnit.net framework](https://xunit.github.io/docs/getting-started-dnx.html) was chosen 
because it is, at the time of this writing, the only unit-test framework (known) as 
compatible with .NET Core.

## Usage Examples

Using the unit-test "moody person" state machine as an example, the basic components of a 
state-machine are shown here:

    /// <summary>
    /// In the QHSM, enum values represent events.  In contrast, the .NET paradigm
    /// is best suited with objects as events.  To resolve this mismatch,
    /// this customized .NET QHSM maps object messages to enum values by using
    /// a MessageAttribute.
    /// </summary>
    public enum PersonSignals
    {
        [Message(typeof(PokeMessage))]
        Poke = QSignals.UserSig,
        [Message(typeof(TiredMessage))]
        Tired,
        Noise,
        [Message(typeof(PunchMessage))]
        Punch
    };

    /// <summary>
    /// In a real state-machine, these "message objects" likely
    /// would have content (i.e. public properties).
    /// </summary>
    public class PokeMessage { }
    public class TiredMessage { }
    public class PunchMessage { }

    public class PersonMachine : QHsmQ
    {
        // The state machine knows how to convert object messages into
        // the enum "signals" expected by the QHSM, based on reflecting
        // the enum type supplied as a paramter.
        public PersonMachine() : base(typeof(PersonSignals)) { }

        // The transition to the initial event is specified in this
        // method override.
        protected override void InitializeStateMachine()
        {
            //Perform state-machine init.
            TransitionTo(Happy);
        } 
        
        // ... state and action implementation provided by you.
    }

Activating the state machine is as follows:

    var person = new PersonMachine();
    
    //Optionally provide a delegate for logging.
    person.TraceEvent = Trace;
    
    // This will either initialize or resume operations of the state-machine.
    // This example assumes that the state-machine will stop itself when it
    // achieves a particular target state or condition.
    await person.Start();
    
The above code assumes the state-machine will instigate the issueing and reeiving of events. If 
the state machine requires an initial event input before reacting, then the code would be as
follows:

    var person = new PersonMachine();
    Task completion = person.Start();
    person.Dispatch(PersonSignals.Noise);
    await completion;
    
Once the state machine is "Stopped", it can be serialized for persistence and resumed later. 
It can be stopped internaly or externally.  An example of an internally initiated "Stop" is below. 
This time a "UserLifecycle" state-machine will be the example:

    internal enum SubscriberMsg
    {
        [Message(typeof(SubscriberDef))]
        Apply = QSignals.UserSig,
        Accepted
    }

    public class UserLifecycle : QHsmQ
    {
        //Usual setup omitted...

        //Exampe QHSM state...
        protected QState Operational(IQEvent evt)
        {
            switch (evt.QSignal)
            {
                case (int)SubscriberMsg.Apply:
                    var application = (SubscriberDef)evt.Message;
                    AddSubscriberAction(application);
                    return null;
                case (int)SubscriberMsg.Accepted:
                    TransitionTo(Received);
                    Stop();
                    return null;
            }
            return TopState;
        }

        protected async void AddSubscriberAction(SubscriberDef application)
        {
            try
            {
                //The CreateSubscriber.Handle is external logic.
                await CreateSubscriber.Handle(application);
                
                //If the prior action was successful, perform a self-dispatch.
                Dispatch(SubscriberMsg.Accepted);
            }
            catch(Exception ex)
            {
                //One of two things would happen here.  Either we self-dispatch
                //a failure event, and let the state-machine deal with it, or
                //we simply decide that we want the state-machine to stop:
                Stop();
            }
        }

The above code sample has two "Stop()" invocations, one for success and one for failure. The code which
activated the state-machine determines the difference by testing the state of the state-machine upon exit.

## Quick Start

The test project contains a "moody person" sample state-machine, with 
instrumentation attached that sends messaging output to the console. 
The sample state-machine serves both purposes of testing the QHSM 
library, and filling an educational role for those who are new to Samek's QHSM.

### Visual Studio 2015

To run the demo from Visual Studio, simply debug-launch (F5) the QHsm.Tests project.

### Command-line

To run either the xUnit unit-tests or the "moody person" demo, 
open a console window in the QHsm.Tests folder and execute:

    dnu restore

Then, to run the unit-tests:

    dnx test

Or to run the demo:

    dnx run
    
### Visual Studio Code (cross-platform IDE)

As of 3/21/2016, [VSCode offers only experimental debugging 
features for .NET Core](https://blogs.msdn.microsoft.com/visualstudioalm/2016/03/10/experimental-net-core-debugging-in-vs-code/). 
However, to simply launch the "moody person" sample state-machine 
from within VSCode, follow these steps:

 1. Use the VSCode explorer to open any C# file found inside the `QHsm.Tests` folder.  
 In the status bar, at the bottom right corner of VSCode, a `Select Project` option appears.
 1. Use `Select Project` to pick `QHsm.Tests`.
 1. Open the VSCode "command palette" (press F1), type `dnx` and select the `dnx: run command` option.  
 This will bring up two options: `dnx run` and `dnx test`.
 1. Select `dnx run`
 
See [Visual Studio Code documentation](https://code.visualstudio.com/docs/runtimes/aspnet5) 
for more information.

>Note: As of 3/22/2106, Visual Studio Code has instability even with 
the directions given above. It is probably safest to run the tests 
or demo from Visual Studio or the command prompt. 
    
## Customization

This QHSM .Net adaptation consists primarily of the following customizations:

 * The initial state transition mechanism has been replaced with the `TransitionTo(...)` standard mechanism.
 
 This simplifies readability and the learning process, without sacrificing any functionality.
 
 * The QHsmWithQueue `Dispatch` routine has been modified to allow non-blocking, multi-threaded dispatch.
 * The state-machines are now persistable.
 
 This mechanism allows for durable state-machines.  This new capability required state machine initializtion
 to be replaced with `Start` and `Stop` entry points.
 
 * Logging event hooks have been added.
 * A new `Dispatch` entry point allows `object` instances to serve as signals, instead of enum values.
  
The object-dispatch capability is the recommended way to interact with these base classes.

## Credits

This adaptation is based on a prior [QP .net port provided by Dr. Rainer Hessmer](http://www.hessmer.org/dev/qhsm/). 
The initial check-in for this repo contained an unmodified 
subset of Hessmer's port, to facilitate comparisons between 
the original and this current distro.
 
