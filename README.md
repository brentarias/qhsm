# QHSM for .NET

This library provides a .Net adaptation of the `QHsm` and `QHsmWithQueue` 
base classes, which come from [Samek's QP Framework](http://www.state-machine.com/). 
These are base classes that allow a simple and direct implementation of finite-automata, 
hierarchical state-charts.  It is compatible with .NET Core (RC1-update2).

This adaption has two primary goals:

 * Enable a persistence or "durable state-machine" capability.
 * Integrate with asynchronous (async / await) event processing.

## Contents

The Visual Studio 2015 solution contains a single QHsm project, and a QHsm.Tests project. 
The test project is simultaneously an xUnit unit-test project and a demo executable.  The 
[xUnit.net framework](https://xunit.github.io/docs/getting-started-dnx.html) was chosen 
because it is the semi-official unit-test framework for .NET Core.

## Usage Examples

The xUnit project contains a state-machine sample of a "moody person".  Using that code for examples, 
the basic components of a state-machine are shown here:

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
    person.Start();
    
If the state machine requires an initial event input before reacting, then the code would be as
follows:

    var person = new PersonMachine();
    person.Start();
    await person.Dispatch(PersonSignals.Noise);
    
    //A user-defined persistence routine which saves the state of the state-machine.
    SaveTheStateSomewhere(person.CurrenState);
    
The `await` allows the calling code to know when the state machine is no longer processing events, 
which means it can then be serialized for persistence and resumed later. To restore the state-machine, 
the code would be:

    //User defined routine to fetch the state.
    var state = GetTheStateFromSomwhere(id);
    person.Start(state);
       
The variable `state` is either a string, or a user defined type.  To declare a user defined type, then
the state machine is declared with that type:

    public class MyStateMachine : QHsm<MyUserType> {...}

The user defined type must implement the `IQHsmState` interface, which is defined as follows:

    public interface IQHsmState
    {
        string Workflow { get; set; }
    }

An example might look like this:

    //Some arbitrary "memento" of state...
    public class PersonState : IQHsmState
    {
        public string Workflow { get; set; }
        /// <summary>
        /// A typical state machine has arbitrary additional state,
        /// called "extended state."  In this case, we are tracking
        /// the amount of noise heard by our Moody Person.
        /// </summary>        
        public int DB; //represents decibels of noise
    }

    public class PersonMachine : ExtendedQHsmQ<PersonState>
    {
        //...the rest as before...
    } 
    
When the state-machine is declared with a user defined state type, the `CurrentState` property
will return that user defined state.

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
 
 This mechanism allows for durable state-machines.  This new capability required state machine initialization
 to be replaced with `Start` and `Stop` entry points.
 
 * Logging event hooks have been added.
 * A new `Dispatch` entry point allows `object` instances to serve as signals, instead of enum values.
  
The object-dispatch capability is the recommended way to interact with these base classes.

## Credits

This adaptation is based on a prior [QP .net port provided by Dr. Rainer Hessmer](http://www.hessmer.org/dev/qhsm/). 
The initial check-in for this repo contained an unmodified 
subset of Hessmer's port, to facilitate comparisons between 
the original and this current distro.
 
