# QHSM for .NET

This library provides a .Net adaptation of the `QHsm` and `QHsmWithQueue` 
base classes, which come from [Samek's QP Framework](http://www.state-machine.com/). 
These are base classes that allow a simple and direct implementation of finite-automata, 
hierarchical state-charts.  The current Visual Studio solution supports .Net Core 3.X, 
but this code was originally created for .NET Core (RC1-update2). In other words, it
will work with all versions in-between if needed.


This adaption has two primary goals:

 * Enable a persistence or "durable state-machine" capability.
 * Integrate with asynchronous (async / await) event processing.

## Contents

The Visual Studio 2019 solution contains a single QHsm project, and a `TestProgram` project. 
The test project is a demo console executable.  

## Usage Examples

The `TestProgram` project contains a state-machine sample of a "moody person".  Using that code for examples, 
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

An example of creating, using, and then persisting the state machine is as follows:

    var person = new PersonMachine();
    
    //Optionally provide a delegate for logging.
    person.TraceEvent = (i,e) => Console.WriteLine(i.Name);
    
    // Initialize the state-machine for first-time use.
    // There is a matching Close() routine that should
    // only be called if an urgent shut-down of the 
    // overall process is needed.
    person.Start();
    
    //Dispatch a couple events, to simulate work...
    // An example of an object-based event.
    await person.Dispatch(new TiredMessage());
    // An example of an enum-based event.
    await person.Dispatch(PersonSignals.Noise);
    
    //Time to persist.  Under normal operations, 
    //this is NOT grounds for calling person.Close().
    
    //The type of 'state' depends on how the state
    //machine was declared.
    var state = person.CurrentState;
    SaveTheStateSomewhere(state);
        
The `await` statements shown above allow the calling code to know when the 
state-machine is no longer processing events, which means it is safe to 
persist the state.  

### Persistence Deep Dive

Because the state-machine is still online (but inactive) during your persistence
step, it can be directed to take contingency steps to compensate for a 
persistence failure: 

    var state = person.CurrentState;
    try
    {
        await SaveTheStateSomewhere(state);
    }
    catch (Exception ex)
    {
        //Both of the examples in this catch block assume
        //that the "PersonSignals" for the "PersonMachine"
        //have had suitable additions...
        
        //Here "Partition" is meant in the CAP theory sense.
        //It is just an example event to send...
        await person.Dispatch(SubscriberSigs.Partition);
        //Or another possibility:
        await person.Dispatch(ex);
    }

In the above example, the state-machine might do something as complex as 
initiating a saga-oriented transaction rollback, or something as simple as
writing a message to an emergency log.

To restore the state-machine from the persistent store, the code would be:

    //User defined routine to fetch the state.
    var state = GetTheStateFromSomwhere(id);
    person.Start(state);
       
The variable `state` is either a string, or a user defined type.  To declare a 
user defined type, then the state machine is declared with that type:

    public class MyStateMachine : QHsm<MyUserType> {...}

The user defined type must implement the `IQHsmState` interface, which is defined 
as follows:

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
    
When the state-machine is declared with a user defined state type, the `CurrentState` 
property will return that user defined state.

### Shunting (asynchronous actions)

Because the `Dispatch` method of the state-machine is awaitable, the internal
implementation of a state-machine must cooperate.  This is done through "shunting,"
as shown in this example:

    //This state is declared within the PersonMachine...
    protected QState Elated(IQEvent evt)
    {
        switch (evt.QSignal)
        {
            //Other signal handlers removed for brevity...
            case (int)PersonSignals.Punch:
                Shunt = SomeAction(Actions.Sigh);
                Shunt = AnotherAction();
                TransitionTo(Sad);
                return null;
        }
        return Happy;
    }
    
    //These actions are also declared within the PersonMachine sample...
    protected async Task SomeAction(PersonalSignals sig)
    {
        await SomeArbitraryCommand(...);
    }
    
    protected async Task AnotherAction()
    {
        await MoreWorkOfSomeKind();
        //When the work is done, naturally we want to punch
        //the moody person...
        Dispatch(PersonSignals.Punch);
    }

Internally, the state-machine `Shunt` property provides reference-counting for 
the number of outstanding or incomplete asynchronous actions.  When all async actions
have completed, then the completion `Task` returned by the `Dispatch` method will
be signalled.  Accordingly, if any async action throws an *unhandled* exception,
the completion `Task` will also be signalled, and will contain the exception.

If any async action has an unhandled exception, the state-machine will enter
an internal `faulted` state, and will no longer respond to further input.  This
is by design; once an *unhandled* exception occurs within the state-machine, there
is no way to know if the state-machine is stable.

>Note: An asynchronous usage of a state-machine only makes sense with the 
QHsmWithQueue base type.  It was adapted for multi-threaded and asynchronous
programming in .Net.

## Quick Start

The test project contains a "moody person" sample state-machine, with 
instrumentation attached that sends messaging output to the console. 
The sample state-machine serves both purposes of testing the QHSM 
library, and filling an educational role for those who are new to Samek's QHSM.

### Visual Studio 2019

To run the demo from Visual Studio, simply debug-launch (F5) the `TestProgram` project.
    
## Customization

This QHSM .Net adaptation consists primarily of the following customizations:

 * Samek's initial state transition mechanism has been replaced with his primary `TransitionTo(...)` mechanism.
 
 This simplifies readability and the learning process, without sacrificing any functionality.  Said differently, intead of
 having two mechanisms for performing transitions, there is now only one.
 
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
 
