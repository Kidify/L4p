using System;
using System.Threading;
using L4p.Common.Extensions;
using L4p.Common.Loggers;
using NUnit.Framework;

namespace L4p.Common.Schedulers._nunit
{
    [TestFixture, Explicit]
    class CallSequenceTests
    {
        [Test]
        public void call_once()
        {
            int count = 0;
            var seq = CallSequence.New(LogFile.Console);

            seq.Enqueue(() => count++);
            Thread.Sleep(100.Milliseconds());

            Assert.That(count, Is.EqualTo(1));
        }

        [Test]
        public void call_twice()
        {
            int count = 0;
            var seq = CallSequence.New(LogFile.Console);

            seq.Enqueue(() => count++);
            Thread.Sleep(500.Milliseconds());

            seq.Enqueue(() => count++);
            Thread.Sleep(100.Milliseconds());

            Assert.That(count, Is.EqualTo(2));
        }

        [Test]
        public void call_twice_concurrent()
        {
            int count = 0;
            var seq = CallSequence.New(LogFile.Console);

            seq.Enqueue(() => Thread.Sleep(300.Milliseconds()));

            seq.Enqueue(() => count++);
            seq.Enqueue(() => count++);

            Thread.Sleep(600.Milliseconds());

            Assert.That(count, Is.EqualTo(2));
        }

        [Test]
        public void exceptions_are_handled()
        {
            int count = 0;
            var seq = CallSequence.New(LogFile.Console);

            seq.Enqueue(() => Thread.Sleep(300.Milliseconds()));

            seq.Enqueue(() => count++);
            seq.Enqueue(() => { throw new Exception("Simulating bad thing"); } );
            seq.Enqueue(() => count++);

            Thread.Sleep(600.Milliseconds());

            Assert.That(count, Is.EqualTo(2));

            dynamic dump = seq.Dump();
        }
    }
}