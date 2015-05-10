namespace L4p.Common.TtlCaches
{
    interface IReferenceCounter
    {
        void LinkBody();
        void ReleaseBody();
    }

    class DeathNotifier
    {
        private IReferenceCounter _reference;

        public DeathNotifier(IReferenceCounter reference)
        {
            _reference = reference;
        }

        public void Cancel()
        {
            _reference = null;
        }

        ~DeathNotifier()
        {
            var reference = _reference;

            if (reference != null)
                reference.ReleaseBody();
        }
    }
}