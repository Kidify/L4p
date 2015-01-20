using System.Threading;

namespace L4p.Common.Concerns
{
    public interface IMyConcernsAreConfigurable
    {
        T ChainIt<T>(T impl) where T : class;
        void SetActiveThread(Thread thread);
    }
}