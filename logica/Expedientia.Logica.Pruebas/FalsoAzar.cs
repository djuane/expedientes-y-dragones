using Expedientia.Logica.Dados;

namespace Expedientia.Logica.Pruebas;

/// <summary>Devuelve valores fijos en orden, para pruebas deterministas.</summary>
internal sealed class FalsoAzar : IGeneradorAleatorio
{
    private readonly Queue<int> _valores;
    public FalsoAzar(params int[] valores) => _valores = new Queue<int>(valores);
    public int Entre(int minimo, int maximo)
    {
        var v = _valores.Dequeue();
        Assert.InRange(v, minimo, maximo);
        return v;
    }
}
