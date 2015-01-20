namespace L4p.Common.Concerns.nunit
{
    interface ISomeInterface
    {
        void SomeMethod(int a);
        int SomeFunction(string s);
    }

    class ToOneSplitter : SplitToOne<ISomeInterface>, ISomeInterface
    {
        public static ISomeInterface New(ISomeInterface impl, ISomeInterface child) { return new ToOneSplitter(impl, child); }
        private ToOneSplitter(ISomeInterface impl, ISomeInterface child) : base(impl, child) {}

        void ISomeInterface.SomeMethod(int a) { dispatch(impl => impl.SomeMethod(a)); }
        int ISomeInterface.SomeFunction(string s) { return dispatch(impl => impl.SomeFunction(s)); }
    }

    class ToManySplitter : SplitToMany<ISomeInterface>, ISomeInterface
    {
        public static ISomeInterface New(ISomeInterface impl, ISomeInterface[] children) { return new ToManySplitter(impl, children); }
        private ToManySplitter(ISomeInterface impl, ISomeInterface[] children) : base(impl, children) { }

        void ISomeInterface.SomeMethod(int a) { dispatch(impl => impl.SomeMethod(a)); }
        int ISomeInterface.SomeFunction(string s) { return dispatch(impl => impl.SomeFunction(s)); }
    }
}