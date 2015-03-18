using System;
using System.Threading.Tasks;
using L4p.Common.Extensions;
using L4p.Common.Loggers;

namespace L4p.Common.Schedulers
{
    public interface IDefferredCall
    {
        void WaitForCompletion();
    }

    public class DefferredCall : IDefferredCall
    {
        private readonly Action _action;
        private readonly Task _task;

        public static IDefferredCall New(Action action)
        {
            return
                new DefferredCall(action);
        }

        private DefferredCall(Action action)
        {
            _action = action;
            _task = Task.Factory.StartNew(action);
        }

        void IDefferredCall.WaitForCompletion()
        {
            try
            {
                _task.Wait();
            }
            catch (Exception ex)
            {
                throw
                    ex.WrapWith<L4pException>("Deferred call to '{0}' failed", _action.Method.Name);
            }
        }
    }
}