using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;
using WebApplication1.Repositories;

namespace WebApplication1.Controllers;

[ApiController]
[Route("[controller]")]
public class CarsController : ControllerBase
{
    private readonly ILogger<CarsController> _logger;
    private readonly CarRepository _repository;

    public CarsController(ILogger<CarsController> logger, CarRepository repository)
    {
        _logger = logger;
        _repository = repository;
    }

    [HttpGet]
    public List<Car> Get([FromQuery] MultipleResourceQuery query)
    {
        var s = new Rison().Decode("(ids:!('1','4'))");
        query = JsonSerializer.Deserialize<MultipleResourceQuery>(new Rison().Decode(q), new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        if (query.Ids.Any())
        {
            _logger.LogInformation("select multi cars by ids: {ids}", string.Join(',', query.Ids));
            return _repository.SelectByIds(query.Ids);
        }

        return _repository.SelectAll();
    }
    
    [HttpGet("{id}")]
    public Car Get(string id)
    {
        return _repository.SelectById(id);
    }
}