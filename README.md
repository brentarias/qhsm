# QHSM for .NET

This library provides a port of the `QHsm` and `QHsmWithQueue` 
base classes, which come from [Samek's QP Framework](http://www.state-machine.com/). 
These are base classes that allow a simple and direct implementation of finite-automata, 
hierarchical state-charts.  It is compatible with .NET Core (update 1 RC).

## Contents

The Visual Studio 2015 solution contains a single QHsm project, and a QHsm.Tests project. 
The test project is simultaneously an xUnit unit-test project, and a demo executable.  The 
[xUnit.net framework](https://xunit.github.io/docs/getting-started-dnx.html) was chosen 
because it is, at the time of this writing, the only unit-test framework (known) as 
compatible with .NET Core.

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

This port has customizations:

 * The initial state transition mechanism has been replaced with the `TransitionTo(...)` standard mechanism.
 
 This simplifies readability and the learning process, without sacrificing any functionality.
 
 * The QHsmWithQueue `Dispatch` routine has been modified to allow non-blocking, multi-threaded dispatch.
 * The state-machines are now persistable.
 
 This mechanism allows for durable state-machines.  This new capability required state machine initializtion
 to be replaced with `Start` and `Stop` entry points.
 
 * Logging event hooks have been added.
 * A new `Dispatch` entry point allows `object` instances to serve as signals, instead of enum values.
  
The object-dispatch capability is the recommended way to interact with this .NET port of QHSM.

## Credits

This port is based on a prior [QP .net port provided by Dr. Rainer Hessmer](http://www.hessmer.org/dev/qhsm/). 
The initial check-in for this repo contained an unmodified 
subset of Hessmer's port, to facilitate comparisons between 
the original and this current distro.
 
