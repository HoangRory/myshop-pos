namespace Server.Contract;

public interface IRepository<T> where T : class
{
    // Lấy theo ID, chấp nhận trả về null nếu không thấy
    Task<T?> GetByIdAsync(object id);

    // Lọc dữ liệu linh hoạt bằng object (có thể là Dictionary, Filter class, hoặc Expression)
    Task<IEnumerable<T>> GetAsync(object? filter = null);

    // Thêm mới, trả về số dòng bị ảnh hưởng (thường là 1)
    Task<int> AddAsync(T entity);

    // Cập nhật, trả về số dòng bị ảnh hưởng
    Task<int> UpdateAsync(T entity);

    // Xóa thẳng bằng ID, tối ưu hiệu năng
    Task<int> DeleteByIdAsync(object id);
}