namespace Models.dto;

public class CategorySetupRequestDto
{
    public string UserId { get; set; } = string.Empty;
    public List<string> Categories { get; set; } = new();
}