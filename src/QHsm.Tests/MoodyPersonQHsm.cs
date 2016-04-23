using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using qf4net;
using System.ComponentModel;

namespace QHsm.Tests
{
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
        [Message(typeof(NoiseMessage))]
        Noise,
        [Message(typeof(PunchMessage))]
        Punch
    };

    /// <summary>
    /// In a real state-machine, these message objects likely
    /// would have content (i.e. public properties).
    /// </summary>
    public class PokeMessage { }
    public class TiredMessage { }
    public class NoiseMessage { }
    public class PunchMessage { }

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
        /// <summary>
        /// This list of "actions" is not needed
        /// for a QHSM.  It has been added to this
        /// sample so that the MoodyPerson QHSM can 
        /// more easily provide trace output, for
        /// educatioonal purposes.
        /// </summary>
        protected enum Actions
        {
            Smile,
            Laugh,
            Frown,
            Scream,
            Sigh,
            Yawn,
            Stretch,
            Snore
        }

        public PersonMachine() : base(typeof(PersonSignals)) { }

        protected override void InitializeStateMachine()
        {
            //Perform state-machine init.
            TransitionTo(Happy);
        }

        protected QState Awake(IQEvent evt)
        {
            switch (evt.QSignal)
            {
                case (int)QSignals.Entry:
                    SomeAction(Actions.Stretch);
                    CurrentState.DB = 0;
                    Console.WriteLine("   DB = 0");
                    return null;

                case (int)QSignals.Exit:
                    SomeAction(Actions.Yawn);
                    return null;

                case (int)PersonSignals.Noise:
                    TransitionTo(Elated);
                    return null;

                case (int)PersonSignals.Tired:
                    TransitionTo(Asleep);
                    return null;
            }
            return TopState;
        }

        protected QState Happy(IQEvent evt)
        {
            switch (evt.QSignal)
            {
                case (int)QSignals.Entry:
                    SomeAction(Actions.Smile);
                    return null;

                case (int)PersonSignals.Poke:
                    SomeAction(Actions.Laugh);
                    return null;

                case (int)PersonSignals.Noise:
                    Stop();
                    return null;

                case (int)PersonSignals.Punch:
                    SomeAction(Actions.Scream);
                    TransitionTo(Sad);
                    return null;

                    //It is not necessary to declare either
                    //the 'Entry' or 'Exit' cases...
                    //case (int)QSignals.Exit:
                    //    return null;
            }
            return Awake;
        }

        protected QState Elated(IQEvent evt)
        {
            switch (evt.QSignal)
            {
                case (int)QSignals.Entry:
                    SomeAction(Actions.Laugh);
                    return null;

                case (int)QSignals.Exit:
                    SomeAction(Actions.Stretch);
                    return null;

                case (int)PersonSignals.Punch:
                    SomeAction(Actions.Sigh);
                    TransitionTo(Sad);
                    return null;
            }
            return Happy;
        }

        protected QState Sad(IQEvent evt)
        {
            switch (evt.QSignal)
            {
                case (int)QSignals.Entry:
                    SomeAction(Actions.Frown);
                    return null;

                case (int)PersonSignals.Poke:
                    SomeAction(Actions.Scream);
                    return null;

                case (int)QSignals.Exit:
                    SomeAction(Actions.Sigh);
                    return null;
            }
            return Awake;
        }

        protected QState Asleep(IQEvent evt)
        {
            switch (evt.QSignal)
            {
                case (int)QSignals.Entry:
                    SomeAction(Actions.Snore);
                    return null;

                case (int)PersonSignals.Poke:
                    //This is called a "self-transition".
                    //It causes 'exit' and 'entry' to fire again.
                    TransitionTo(Asleep);
                    return null;

                case (int)PersonSignals.Punch:
                    SomeAction(Actions.Scream);
                    TransitionTo(Sad);
                    return null;

                case (int)PersonSignals.Tired:
                    Stop();
                    return null;

                case (int)PersonSignals.Noise:
                    CurrentState.DB += 20;
                    Console.WriteLine("   DB += 20");
                    if (CurrentState.DB >= 40)
                    {
                        TransitionTo(Awake);
                    }
                    return null;
            }
            return TopState;
        }

        #region actions

        /// <summary>
        /// Typically there would be a seperate function
        /// for each action, but this example doesn't bother.
        /// </summary>
        /// <param name="text"></param>
        protected void SomeAction(Actions act)
        {
            Console.WriteLine("  Action:{0}", act.ToString());
        }
        #endregion
    }
}
