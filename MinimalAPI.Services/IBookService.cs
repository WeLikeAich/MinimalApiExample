using MinimalAPI.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinimalAPI.Services;

public interface IBookService
{
    public Task<IEnumerable<Book>> GetAsync(string? searchTerm);

    public Task<Book?> GetByISBNAsync(string isbn);

    public Task<bool> CreateAsync(Book newBook);

    public Task<bool> UpdateAsync(Book newBook);

    public Task<bool> DeleteAsync(string isbn);
}