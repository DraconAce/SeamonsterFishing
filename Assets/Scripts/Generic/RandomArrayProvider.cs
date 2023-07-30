using Unity.Collections;
using Unity.Jobs.LowLevel.Unsafe;

public class RandomArrayProvider
{
    public NativeArray<Unity.Mathematics.Random> RandomArray;

    public RandomArrayProvider()
    {
        var randomArray = new Unity.Mathematics.Random[JobsUtility.MaxJobThreadCount];
        var seed = new System.Random();

        for (var i = 0; i < JobsUtility.MaxJobThreadCount; ++i)
            randomArray[i] = new Unity.Mathematics.Random((uint) seed.Next());

        RandomArray = new NativeArray<Unity.Mathematics.Random>(randomArray, Allocator.Persistent);
    }

    public void Dispose() => RandomArray.Dispose();
}