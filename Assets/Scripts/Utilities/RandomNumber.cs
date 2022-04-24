using System;
using Random = UnityEngine.Random;

public class RandomNumbers : Singleton<RandomNumbers>
{
    /// <summary>
    /// Returns a random seed each time
    /// </summary>
    public static int Seed { get { return Guid.NewGuid().GetHashCode(); } }

    private void Start() => Random.InitState(Seed);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="min">Inclusive</param>
    /// <param name="max">Exclusive</param>
    /// <returns></returns>
    public int Between(int min, int max)
    {
        return Random.Range(min, max);
    }
}
