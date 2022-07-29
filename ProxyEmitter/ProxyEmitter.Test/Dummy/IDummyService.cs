namespace ProxyEmitter.Test.Dummy
{
    /// <summary>
    /// All kinds of interfaces
    /// </summary>
    [ProxyNamespace("CatX")]
    public interface IDummyService
    {
        void Fn1();
        int Fn2();
        void Fn3(int a, int b);
        int Fn4(int a, int b);
        int Fn5(int a, int b, int c, int d);
    }
}