namespace L4p.Common.GcAwareTtlCaches
{
    interface IReferenceCounter
    {
        void LinkInstance();
        void ReleaseInstance();
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
                reference.ReleaseInstance();
        }
    }
}