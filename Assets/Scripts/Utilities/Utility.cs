using System;
using System.Linq;

public static class Utility
{
    public static T[] ShuffleArray<T>(T[] array)
    {
        Random rand = new Random(RandomNumbers.Seed);

        for (int i = 0; i < array.Length - 1; i++)
        {
            int rIndex = rand.Next(i, array.Length);

            T tempItem = array[rIndex];

            array[rIndex] = array[i];
            array[i] = tempItem;
        }

        return array;
    }

    public static T[] GetEnumValues<T>() where T : Enum
    {
        return Enum.GetValues(typeof(T)).Cast<T>().ToArray();
    }

    public static T[] RandomizeArray<T>(T[] array)
    {
        var rand = new Random();
        return array.OrderBy(x => rand.Next()).ToArray();
    }
}
