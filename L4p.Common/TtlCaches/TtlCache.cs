using System;
using System.Runtime.CompilerServices;

namespace L4p.Common.TtlCaches
{
    public interface ITtlCache<TFacet, TBody>
        where TFacet : class
        where TBody : class
    {
        void Store(TFacet facet, TBody body);
        TBody[] GetDeadBodies(TimeSpan ttl);
    }

    public class TtlCache<TFacet, TBody> : ITtlCache<TFacet, TBody>
        where TFacet : class
        where TBody : class
    {
        #region members

        private readonly ConditionalWeakTable<TFacet, DeathNotifier> _deathAgent;
        private readonly IItemsRepo<TBody> _repo;

        #endregion

        #region construction

        public static ITtlCache<TFacet, TBody> New()
        {
            return
                new TtlCache<TFacet, TBody>();
        }

        private TtlCache()
        {
            _deathAgent = new ConditionalWeakTable<TFacet, DeathNotifier>();
            _repo = ItemsRepo<TBody>.New();
        }

        #endregion

        #region private
        #endregion

        #region ITtlCache

        void ITtlCache<TFacet, TBody>.Store(TFacet facet, TBody body)
        {
            var item = _repo.GetBy(body);

            if (item == null)
            {
                item = TtlItem<TBody>.New(body);
                item = _repo.Add(item);
            }

            var reference = (IReferenceCounter) item;
            var notifier = new DeathNotifier(reference);

            // if death agent is already aware of the body do nothing
            try
            {
                _deathAgent.Add(facet, notifier);
                reference.LinkBody();
            }
            catch (ArgumentException)   // key already exists
            {
                notifier.Cancel();
            }
        }

        TBody[] ITtlCache<TFacet, TBody>.GetDeadBodies(TimeSpan ttl)
        {
            var items = _repo.GetDeadItems(ttl);

            if (items == null)
                return null;

            if (items.Length == 0)
                return null;

            _repo.Remove(items);

            var count = items.Length;
            var bodies = new TBody[count];

            for (int indx = 0; indx < count; indx++)
            {
                bodies[indx] = items[indx].Body;
            }

            return bodies;
        }

        #endregion
    }
}