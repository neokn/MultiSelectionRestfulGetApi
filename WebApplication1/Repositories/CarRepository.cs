using WebApplication1.Models;

namespace WebApplication1.Repositories;

public class CarRepository
{
    private List<Car> _data;

    public CarRepository()
    {
        _data = new List<Car>
        {
            new("1", "Volvo"),
            new("2", "Saab"),
            new("3", "Opel"),
            new("4", "Audi")
        };
    }

    public List<Car> SelectAll()
    {
        return _data;
    }

    public Car SelectById(string id)
    {
        return _data.First(c => c.Id == id);
    }
    
    public List<Car> SelectByIds(List<string> ids)
    {
        return _data.Where(c => ids.Contains(c.Id)).ToList();
    }
}