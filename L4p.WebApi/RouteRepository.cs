using System;
using System.Collections.Generic;
using System.Linq;
using L4p.Common.Extensions;
using L4p.Common.Helpers;

namespace L4p.WebApi
{
    public interface IRouteRepository
    {
        int Count { get; }
        SingleRoute AddRoute(SingleRoute route);
        SingleRoute FindRoute(string path);
    }

    class ASplit
    {
        private readonly string[] _segments;

        public string[] Segments
        {
            get { return _segments; }
        }

        private bool is_parameter_pattern(string segment)
        {
            return
                segment.StartsWith("{");
        }

        public ASplit(string path)
        {
            var query =
                from segment in path.Split('/')
                where
                    segment.IsNotEmpty() &&
                    !is_parameter_pattern(segment)
                select segment;

            _segments = query.ToArray();
        }

        public int MatchOther(ASplit other)
        {
            return 
                _segments.Count(other.HasSegment);
        }

        public bool HasSegment(string segment)
        {
            return 
                _segments.Any(x => x == segment);
        }
    }

    public class RouteRepository : IRouteRepository
    {
        #region members

        private readonly Dictionary<string, SingleRoute> _routes;
        private Dictionary<string, SingleRoute> _cache;     // non-readonly: immutable add only cache pattern

        #endregion

        #region construction

        public static IRouteRepository New()
        {
            return 
                new RouteRepository();
        }

        private RouteRepository()
        {
            _routes = new Dictionary<string, SingleRoute>(StringComparer.InvariantCultureIgnoreCase);
            _cache = new Dictionary<string, SingleRoute>(StringComparer.InvariantCultureIgnoreCase);
        }

        #endregion

        #region private

        private void add_to_cache(string path, SingleRoute route)
        {
            Validate.NotNull(route);

            var cache = new Dictionary<string, SingleRoute>(_cache, StringComparer.InvariantCultureIgnoreCase);
            cache.Add(path, route);

            _cache = cache;
        }

        private SingleRoute get_from_cache(string path)
        {
            var cache = _cache;         // capture snapshot

            SingleRoute route;
            cache.TryGetValue(path, out route);

            return route;
        }

        private SingleRoute find_route(string path)
        {
            var split = new ASplit(path);
            int bestMatch = -1;
            SingleRoute bestRoute = null;

            foreach (var route in _routes.Values)
            {
                int hasToBeThereSegments = route.Split.Segments.Length;
                int match = split.MatchOther(route.Split);

                if (match >= hasToBeThereSegments)      // can happen if url has same segments, 
                                                        // fe: /trading/affiliate/35273/user/get/trading/options
                if (match > bestMatch)
                {
                    bestMatch = match;
                    bestRoute = route;
                }
            }

            return bestRoute;
        }

        #endregion

        #region interface

        int IRouteRepository.Count
        {
            get { return _routes.Count; }
        }

        SingleRoute IRouteRepository.AddRoute(SingleRoute route)
        {
            SingleRoute prev;
            string path = route.Path.ToLowerInvariant();

            if (_routes.TryGetValue(path, out prev))
                return prev;

            route.Split = new ASplit(path);
            _routes.Add(path, route);

            return route;
        }

        SingleRoute IRouteRepository.FindRoute(string path)
        {
            path = path.ToLowerInvariant();

            SingleRoute route = get_from_cache(path);

            if (route != null)
                return route;

            route = find_route(path);

            if (route != null)
                add_to_cache(path, route);

            return route;
        }

        #endregion
    }
}