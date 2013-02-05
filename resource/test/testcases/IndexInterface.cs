// from http://msdn.microsoft.com/en-us/library/tkyhsw31%28v=vs.80%29.aspx

public interface ISomeInterface
{
    int this[int index]
    {
        get;
        set;
    }
}

class IndexerClass : ISomeInterface
{
    private int[] arr = new int[100];
    public int this[int index]   // indexer declaration
    {
        get
        {
            if (index < 0 || index >= 100)
            {
                return 0;
            }
            else
            {
                return arr[index];
            }
        }
        set
        {
            if (!(index < 0 || index >= 100))
            {
                arr[index] = value;
            }
        }
    }
}