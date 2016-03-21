# QHSM for .NET

This library provides a port of the QHsm and QHsmWithQueue base classes, which come from [Samek's QP Framework](http://www.state-machine.com/). 
It is compatible with .NET Core (update 1 RC).

## Contents

The Visual Studio 2015 solution contains a single QHsm project, and a QHsm.Tests project. The test project is 
simultaneously an xunit unit-test project, and a demo executable.

The test project contains a "moody person" sample state-machine, with instrumentation attached that sends
messaging output to the console.  The sample state-machine serves both purposes of testing the QHSM
library, and filling an educational role for those who are new to Samek's QHSM.

To run the demo, simply debug-launch (F5) the QHsm.Tests project.

To run the xunit test, open a console window in the QHsm.Tests folder and execute:

    dnu restore
    dnx test
    
## Customization

This port has customizations:

 * The initial state transition mechanism has been replaced with the `TransitionTo(...)` standard mechanism.
 
 This simplifies readability and the learning process, without sacrificing any functionality.
 
 * The QHsmWithQueue `Dispatch` routine has been modified to allow non-blocking, multi-threaded dispatch.
 * The state-machines are now persistable.
 
 This mechanism allows for durable state-machines.  The new capability required state machine initializtion
 to be replaced with `Start` and `Stop` entry points.
 
  * A new `Dispatch` entry point allows `object` instances to serve as signals, instead of enum values.
  
The object-dispatch capability is the recommended way to interact with this .NET port of QHSM.

## Notes

This port is based on a prior [QP .net port provided by Dr. Rainer Hessmer](http://www.hessmer.org/dev/qhsm/). 
The initial check-in for this repo contained an unmodified subset of Hessmer's port, to facilitate 
comparisons between the original and this current distro.
 
