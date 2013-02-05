//from http://msdn.microsoft.com/en-us/library/64syzecx%28v=vs.80%29.aspx

interface IEmployee
{
    string Name
    {
        get;
        set;
    }

    int Counter
    {
        get;
    }
}

interface ICitizen
{
    string Name
    {
        get;
        set;
    }
}

class Employee : IEmployee, ICitizen {

	public static int numberOfEmployees;

    private string name;
    public string Name  // read-write instance property
    {
        get
        {
            return name;
        }
        set
        {
            name = value;
        }
    }

    private int counter;
    public int Counter  // read-only instance property
    {
        get
        {
            return counter;
        }
    }

    public Employee()  // constructor
    {
        counter = ++counter + numberOfEmployees;
    }
	
	string ICitizen.Name
	{
	    get { return"Citizen Name"; }
	    set { }
	}

}
