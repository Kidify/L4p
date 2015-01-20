using System;

namespace L4p.Common.FunnelsModel
{
    class NotInitializedFunnels : IFunnelsManager
    {
        private static readonly string NotInitializedMsg = "Funnels are not initialized (or failed to initialize)";

        #region members
        #endregion

        #region construction

        public static IFunnelsManager New()
        {
            return
                new NotInitializedFunnels();
        }

        private NotInitializedFunnels()
        {
        }

        #endregion

        #region private
        #endregion

        #region interface

        IFunnelsManager IFunnelsManager.Start()
        {
            throw
                new FunnelsException(NotInitializedMsg);
        }

        void IFunnelsManager.Stop()
        {
            throw
                new FunnelsException(NotInitializedMsg);
        }

        IFunnel IFunnelsManager.GetFunnelOfInstance<T>(T instance)
        {
            throw
                new FunnelsException(NotInitializedMsg);
        }

        IFunnel IFunnelsManager.GetFunnelOfClass(Type type)
        {
            throw
                new FunnelsException(NotInitializedMsg);
        }

        IFunnel IFunnelsManager.GetCurrentFunnel()
        {
            throw
                new FunnelsException(NotInitializedMsg);
        }

        IFunnel IFunnelsManager.SetCurrentFunnel(IFunnel funnel)
        {
            throw
                new FunnelsException(NotInitializedMsg);
        }

        IFunnel IFunnelsManager.GetFunnelBy(string name)
        {
            throw
                new FunnelsException(NotInitializedMsg);
        }

        IFunnel IFunnelsManager.GetFunnelBy<E>(E enumValue)
        {
            throw
                new FunnelsException(NotInitializedMsg);
        }

        IFunnel IFunnelsManager.GetFunnelBy(string name, string tag)
        {
            throw
                new FunnelsException(NotInitializedMsg);
        }

        IFunnel IFunnelsManager.GetFunnelBy(string name, int tag)
        {
            throw
                new FunnelsException(NotInitializedMsg);
        }

        IFunnel IFunnelsManager.GetFunnelBy(string name, long tag)
        {
            throw
                new FunnelsException(NotInitializedMsg);
        }

        IFunnel IFunnelsManager.GetFunnelBy(string name, Guid tag)
        {
            throw
                new FunnelsException(NotInitializedMsg);
        }

        IFunnel IFunnelsManager.GetFunnelBy<T>(string tag)
        {
            throw
                new FunnelsException(NotInitializedMsg);
        }

        IFunnel IFunnelsManager.GetFunnelBy<T>(int tag)
        {
            throw
                new FunnelsException(NotInitializedMsg);
        }

        IFunnel IFunnelsManager.GetFunnelBy<T>(long tag)
        {
            throw
                new FunnelsException(NotInitializedMsg);
        }

        IFunnel IFunnelsManager.GetFunnelBy<T>(Guid tag)
        {
            throw
                new FunnelsException(NotInitializedMsg);
        }

        IFunnel IFunnelsManager.GetFunnelBy<E>(E enumValue, string tag)
        {
            throw
                new FunnelsException(NotInitializedMsg);
        }

        IFunnel IFunnelsManager.GetFunnelBy<E>(E enumValue, int tag)
        {
            throw
                new FunnelsException(NotInitializedMsg);
        }

        IFunnel IFunnelsManager.GetFunnelBy<E>(E enumValue, long tag)
        {
            throw
                new FunnelsException(NotInitializedMsg);
        }

        IFunnel IFunnelsManager.GetFunnelBy<E>(E enumValue, Guid tag)
        {
            throw
                new FunnelsException(NotInitializedMsg);
        }

        IFunnel IFunnelsManager.NewFunnel(string name)
        {
            throw
                new FunnelsException(NotInitializedMsg);
        }

        IFunnel IFunnelsManager.NewFunnel(string name, string tag)
        {
            throw
                new FunnelsException(NotInitializedMsg);
        }

        IFunnel IFunnelsManager.NewFunnel(string name, int tag)
        {
            throw
                new FunnelsException(NotInitializedMsg);
        }

        IFunnel IFunnelsManager.NewFunnel(string name, long tag)
        {
            throw
                new FunnelsException(NotInitializedMsg);
        }

        IFunnel IFunnelsManager.NewFunnel(string name, Guid tag)
        {
            throw
                new FunnelsException(NotInitializedMsg);
        }

        IFunnel IFunnelsManager.NewFunnel<T>()
        {
            throw
                new FunnelsException(NotInitializedMsg);
        }

        IFunnel IFunnelsManager.NewFunnel<T>(string tag)
        {
            throw
                new FunnelsException(NotInitializedMsg);
        }

        IFunnel IFunnelsManager.NewFunnel<T>(int tag)
        {
            throw
                new FunnelsException(NotInitializedMsg);
        }

        IFunnel IFunnelsManager.NewFunnel<T>(long tag)
        {
            throw
                new FunnelsException(NotInitializedMsg);
        }

        IFunnel IFunnelsManager.NewFunnel<T>(Guid tag)
        {
            throw
                new FunnelsException(NotInitializedMsg);
        }

        IFunnel IFunnelsManager.NewFunnel<E>(E enumValue)
        {
            throw
                new FunnelsException(NotInitializedMsg);
        }

        IFunnel IFunnelsManager.NewFunnel<E>(E enumValue, string tag)
        {
            throw
                new FunnelsException(NotInitializedMsg);
        }

        IFunnel IFunnelsManager.NewFunnel<E>(E enumValue, int tag)
        {
            throw
                new FunnelsException(NotInitializedMsg);
        }

        IFunnel IFunnelsManager.NewFunnel<E>(E enumValue, long tag)
        {
            throw
                new FunnelsException(NotInitializedMsg);
        }

        IFunnel IFunnelsManager.NewFunnel<E>(E enumValue, Guid tag)
        {
            throw
                new FunnelsException(NotInitializedMsg);
        }

        #endregion
    }
}