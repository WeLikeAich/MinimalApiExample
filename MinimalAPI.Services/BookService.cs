using Dapper;
using MinimalAPI.Data;

namespace MinimalAPI.Services;

public class BookService : IBookService
{
    private IDbConnectionFactory _connectionFactory;

    public BookService(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<Book>> GetAsync(string? searchTerm)
    {
        if (searchTerm == null)
            return await GetAllBooksAsync();

        return await SearchBooksByTitleAsync(searchTerm);
    }

    public async Task<Book?> GetByISBNAsync(string isbn)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        return await connection.QuerySingleOrDefaultAsync<Book>(@$"SELECT * FROM Books WHERE Isbn = @ISBN LIMIT 1", new { ISBN = isbn });
    }

    public async Task<bool> CreateAsync(Book newBook)
    {
        var existingBook = await GetByISBNAsync(newBook.Isbn);
        if (existingBook is not null)
        {
            return false;
        }

        using var connection = await _connectionFactory.CreateConnectionAsync();
        var result = await connection.ExecuteAsync(
            @"INSERT INTO Books (Isbn, Title, Author, ShortDescription, PageCount, ReleaseDate)
            VALUES (@Isbn, @Title, @Author, @ShortDescription, @PageCount, @ReleaseDate)",
            newBook);
        return result > 0;
    }

    public async Task<bool> UpdateAsync(Book newBook)
    {
        var existingBook = await GetByISBNAsync(newBook.Isbn);
        if (existingBook is null)
        {
            return false;
        }

        using var connection = await _connectionFactory.CreateConnectionAsync();
        var result = await connection.ExecuteAsync(
            @"UPDATE Books SET Title = @Title, Author = @Author, ShortDescription = @ShortDescription,
                PageCount = @PageCount, ReleaseDate = @ReleaseDate WHERE Isbn = @Isbn",
            newBook);

        return result > 0;
    }

    public async Task<bool> DeleteAsync(string isbn)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var result = await connection.ExecuteAsync(
            @"DELETE FROM Books WHERE Isbn = @Isbn",
            new { Isbn = isbn });

        return result > 0;
    }

    private async Task<IEnumerable<Book>> GetAllBooksAsync()
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var result = await connection.QueryAsync<Book>(@"SELECT * FROM Books");
        return result;
    }

    private async Task<IEnumerable<Book>> SearchBooksByTitleAsync(string searchTerm)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        var result = await connection.QueryAsync<Book>(@$"SELECT * FROM Books WHERE Title LIKE '%' || @SearchTerm || '%'", new { SearchTerm = searchTerm });
        return result;
    }
}