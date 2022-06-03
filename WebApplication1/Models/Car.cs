namespace WebApplication1.Models;

public class Car
{
    public string Id { get; }
    public string Name { get; }

    public Car(string id, string name)
    {
        Id = id;
        Name = name;
    }
}