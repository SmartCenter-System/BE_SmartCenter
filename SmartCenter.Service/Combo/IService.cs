namespace SmartCenter.Service.Combo;

public interface IService
{
    Task<List<Response.ComboResponse>> GetAllCombosAsync();
    Task<Response.ComboResponse?> GetComboByIdAsync(Guid comboId);
    Task<Response.ComboResponse> CreateComboAsync(Request.CreateComboRequest request);
    Task<Response.ComboResponse> UpdateComboAsync(Guid comboId, Request.UpdateComboRequest request);
    Task DeleteComboAsync(Guid comboId);
}