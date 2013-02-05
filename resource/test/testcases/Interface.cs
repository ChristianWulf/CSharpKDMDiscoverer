using System;

interface IEagle {

	void fly();
	
	string Name
    {
        get;
        set;
    }
	
	event EventHandler OnFly;
	
	// Indexer declaration:
    string this[int index]
    {
        get;
        set;
    }
}
