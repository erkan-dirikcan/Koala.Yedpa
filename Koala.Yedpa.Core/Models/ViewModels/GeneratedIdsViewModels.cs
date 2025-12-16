namespace Koala.Yedpa.Core.Models.ViewModels;

public class CreateGeneratedIdsViewModel
{
    public string ModuleId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Prefix { get; set; }//lgp
    public int StartNumber { get; set; }//0
    public int LastNumber { get; set; }//1
    public int Digit { get; set; }//6
}
public class UpdateGeneratedIdsViewModel : CreateGeneratedIdsViewModel
{
    public string Id { get; set; } // This should be initialized with a valid GUID
}

public class GeneratedIdsListViewModel
{
    public string Id { get; set; } // This should be initialized with a valid GUID
    public string ModuleId { get; set; }
    public string ModuleName { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Prefix { get; set; }//lgp
    public int StartNumber { get; set; }//0
    public int LastNumber { get; set; }//1
    public int Digit { get; set; }//6
}
public class GetNextNumberViewModel
{
    public string Id { get; set; }
    public string ModuleId { get; set; }
    public string ModuleName { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Prefix { get; set; }//lgp
    public int LastNumber { get; set; }//1
    public int Digit { get; set; }//6
}