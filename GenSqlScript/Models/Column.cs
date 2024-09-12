public class Column
{
    public required string Name { get; set; }
    public bool PrimaryKey { get; set; }
    public required string DataType { get; set; }
    public int Length { get; set; }
}
